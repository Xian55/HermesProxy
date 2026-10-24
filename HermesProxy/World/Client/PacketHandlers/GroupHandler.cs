using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Objects;
using HermesProxy.World.Server;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [HandlesSmsg(Opcode.SMSG_PARTY_COMMAND_RESULT)]
    internal void HandlePartyCommandResult(WorldPacket packet)
    {
        PartyCommandResult party = new PartyCommandResult();
        party.Command = (byte)packet.ReadUInt32();
        party.Name = packet.ReadCString();
        uint partyResult = packet.ReadUInt32();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            party.Result = (byte)partyResult;
        else
            party.Result = (byte)((PartyResultVanilla)partyResult).CastEnum<PartyResultModern>();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            party.ResultData = packet.ReadUInt32();
        SendPacketToClient(party);

        // The server's answer to its own player leaving, before it takes them out. TrinityCore
        // 4.3.4 then sends the list that removes the player; another 4.3.4 core sends none for a
        // dungeon finder group, left inside its dungeon or after teleporting out. The client stayed
        // in a group of five the server no longer had them in, and a Leave after that went
        // unanswered.
        if (IsCataLegacy && party.Command == PartyOperationLeave && party.Result == (byte)PartyResultModern.Ok)
            SendLeftGroup();
    }

    // TrinityCore PartyOperation PARTY_OP_LEAVE
    private const byte PartyOperationLeave = 2;

    /// <summary>
    /// The player is out of the group announced last: what the server's removal list does, said
    /// before it comes. That list then names no group the client is in, and goes no further.
    /// </summary>
    private void SendLeftGroup()
    {
        var state = GetSession().GameState;
        byte index = state.LastAnnouncedPartyIndex;
        PartyUpdate? group = state.CurrentGroups[index];
        if (group == null)
            return;

        state.MasterLootCandidates = null;
        state.LastMasterLootSentTarget = default;
        PartyUpdate party = new PartyUpdate();
        party.SequenceNum = state.GroupUpdateCounter++;
        party.PartyIndex = index;
        party.PartyFlags |= GroupFlags.Destroyed;
        party.PartyGUID = index == 0 ? WowGuid128.Empty : group.PartyGUID;
        party.LeaderGUID = WowGuid128.Empty;
        party.MyIndex = -1;
        state.CurrentGroups[index] = null;
        state.LastAnnouncedPartyIndex = 0;
        if (state.CurrentGroups[0] == null && state.CurrentGroups[1] == null)
            state.GroupAssignedRoles.Clear();
        SendPacketToClient(party);

        if (state.LastGroupWasLfg)
        {
            state.LastGroupWasLfg = false;
            SendLfgAssociationEnded();
        }
    }

    [HandlesSmsg(Opcode.SMSG_GROUP_DECLINE)]
    internal void HandleGroupDecline(WorldPacket packet)
    {
        GroupDecline party = new GroupDecline();
        party.Name = packet.ReadCString();
        SendPacketToClient(party);
    }

    // TrinityCore 4.3.4's world marker spells, markers 1 to 5 (Spell::EffectSummonRaidMarker)
    private const int FirstRaidMarkerSpell = 84996;
    private const int RaidMarkerSpellCount = 5;

    internal static bool IsRaidMarkerSpell(int spellId)
        => spellId >= FirstRaidMarkerSpell && spellId < FirstRaidMarkerSpell + RaidMarkerSpellCount;

    /// <summary>
    /// TrinityCore 4.3.4 sends only the mask of the world markers that are out. The 4.4.2 client
    /// wants each one's map and place with it, one entry per set bit, and crashes on a set bit
    /// without one (#415). The places come from the markers' dynamic objects in sight, this
    /// session's or another member's (<see cref="RaidMarkerBoard"/>); a marker whose place is not
    /// known is left out until its object shows.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_RAID_MARKERS_CHANGED)]
    internal void HandleRaidMarkersChanged(WorldPacket packet)
    {
        var state = GetSession().GameState;
        WowGuid128 party = state.GetCurrentGroupGuid();
        state.RaidMarkerMask = packet.ReadUInt32();
        for (int slot = 0; slot < state.RaidMarkerPlaces.Length; slot++)
        {
            if ((state.RaidMarkerMask & (1u << slot)) != 0)
            {
                if (state.RaidMarkerPlaces[slot] == null && !party.IsEmpty() &&
                    RaidMarkerBoard.TryGet(party, slot, out uint map, out System.Numerics.Vector3 place))
                {
                    state.RaidMarkerPlaces[slot] = place;
                    state.RaidMarkerMaps[slot] = map;
                }
                continue;
            }
            state.RaidMarkerPlaces[slot] = null;
            if (!party.IsEmpty())
                RaidMarkerBoard.Clear(party, slot);
        }
        SendRaidMarkers();
    }

    /// <summary>
    /// A world marker's dynamic object came into sight: its place is kept, the markers go to the
    /// client again if the server already said that one is out, and the party's other sessions
    /// are told.
    /// </summary>
    internal void NoteRaidMarker(int spellId, System.Numerics.Vector3 place)
    {
        if (!IsRaidMarkerSpell(spellId))
            return;

        int slot = spellId - FirstRaidMarkerSpell;
        var state = GetSession().GameState;
        uint map = state.CurrentMapId ?? 0;
        state.RaidMarkerPlaces[slot] = place;
        state.RaidMarkerMaps[slot] = map;
        if ((state.RaidMarkerMask & (1u << slot)) != 0)
            SendRaidMarkers();

        WowGuid128 party = state.GetCurrentGroupGuid();
        if (!party.IsEmpty())
            RaidMarkerBoard.Set(party, slot, map, place, GetSession());
    }

    /// <summary>
    /// Another session saw one of its party's markers: a marker this one knows is out takes its
    /// place from the board, where it had none or the marker was put down again elsewhere.
    /// </summary>
    internal void TakeRaidMarkersFromBoard()
    {
        var state = GetSession().GameState;
        WowGuid128 party = state.GetCurrentGroupGuid();
        if (party.IsEmpty())
            return;

        bool taken = false;
        for (int slot = 0; slot < state.RaidMarkerPlaces.Length; slot++)
        {
            if ((state.RaidMarkerMask & (1u << slot)) == 0 ||
                !RaidMarkerBoard.TryGet(party, slot, out uint map, out System.Numerics.Vector3 place) ||
                (state.RaidMarkerPlaces[slot] == place && state.RaidMarkerMaps[slot] == map))
                continue;
            state.RaidMarkerPlaces[slot] = place;
            state.RaidMarkerMaps[slot] = map;
            taken = true;
        }
        if (taken)
            SendRaidMarkers();
    }

    private void SendRaidMarkers()
    {
        var state = GetSession().GameState;
        RaidMarkersChanged markers = new RaidMarkersChanged();
        for (int slot = 0; slot < state.RaidMarkerPlaces.Length; slot++)
        {
            if ((state.RaidMarkerMask & (1u << slot)) == 0 || state.RaidMarkerPlaces[slot] is not System.Numerics.Vector3 place)
                continue;
            markers.ActiveMarkers |= 1u << slot;
            markers.Markers.Add(new RaidMarker(state.RaidMarkerMaps[slot], place.X, place.Y, place.Z));
        }
        SendPacketToClient(markers);
    }

    [HandlesSmsg(Opcode.SMSG_GROUP_DESTROYED)]
    internal void HandleGroupDestroyed(WorldPacket packet)
    {
        SendPacketToClient(new GroupDestroyed());
    }

    /// <summary>
    /// TrinityCore 4.3.4 PartyInvite::Write: the inviter's GUID masked, with the flags, the two
    /// name lengths and the dungeon count among its bits; then the strings without terminators.
    /// Read the 3.3.5a way it threw, and the invited player was never asked.
    /// </summary>
    private void HandleGroupInviteCata(WorldPacket packet)
    {
        PartyInvite party = new PartyInvite();
        Span<bool> mask = stackalloc bool[8];
        Span<byte> guid = stackalloc byte[8];

        party.MustBeBNetFriend = packet.HasBit();
        MaskedGuid.ReadMaskBits(packet, mask, [0, 3, 2]);
        party.CanAccept = packet.HasBit();
        MaskedGuid.ReadMaskBits(packet, mask, [6, 5]);
        uint realmLength = packet.ReadBits<uint>(9);
        MaskedGuid.ReadMaskBits(packet, mask, [4]);
        uint nameLength = packet.ReadBits<uint>(7);
        uint lfgSlots = packet.ReadBits<uint>(24);
        party.IsXRealm = packet.HasBit();
        MaskedGuid.ReadMaskBits(packet, mask, [1, 7]);

        MaskedGuid.ReadBytes(packet, mask, guid, [1, 4]);
        packet.ReadUInt32();                        // Timestamp
        party.ProposedRoles = packet.ReadUInt32();
        party.LfgCompletedMask = packet.ReadInt32();
        MaskedGuid.ReadBytes(packet, mask, guid, [6, 0, 2, 3]);
        if (lfgSlots > 64)
            return; // a count no invite carries: not this layout
        for (uint i = 0; i < lfgSlots; i++)
            party.LfgSlots.Add(packet.ReadInt32());
        MaskedGuid.ReadBytes(packet, mask, guid, [5]);
        packet.ReadString(realmLength);             // the inviter's realm, by the server's name for it
        MaskedGuid.ReadBytes(packet, mask, guid, [7]);
        party.InviterName = packet.ReadString(nameLength);

        var realm = GetSession().RealmManager.GetRealm(GetSession().RealmId)!;
        party.InviterRealm = new VirtualRealmInfo(realm.Id.GetAddress(), true, false, realm.Name, realm.NormalizedName);
        party.InviterGUID = new WowGuid64(MaskedGuid.ToUInt64(guid)).To128(GetSession().GameState);
        party.InviterBNetAccountId = GetSession().GetBnetAccountGuidForPlayer(party.InviterGUID);
        SendPacketToClient(party);
    }

    [HandlesSmsg(Opcode.SMSG_PARTY_INVITE)]
    internal void HandleGroupInvite(WorldPacket packet)
    {
        if (IsCataLegacy)
        {
            HandleGroupInviteCata(packet);
            return;
        }

        PartyInvite party = new PartyInvite();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            party.CanAccept = packet.ReadBool();

        var realm = GetSession().RealmManager.GetRealm(GetSession().RealmId)!;
        party.InviterRealm = new VirtualRealmInfo(realm.Id.GetAddress(), true, false, realm.Name, realm.NormalizedName);

        party.InviterName = packet.ReadCString();
        party.InviterGUID = GetSession().GameState.GetPlayerGuidByName(party.InviterName);
        if (party.InviterGUID == default)
        {
            party.InviterBNetAccountId = WowGuid128.Empty;
        }
        else
            party.InviterBNetAccountId = GetSession().GetBnetAccountGuidForPlayer(party.InviterGUID);

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
        {
            party.ProposedRoles = packet.ReadUInt32();
            var lfgSlotsCount = packet.ReadUInt8();
            for (var i = 0; i < lfgSlotsCount; ++i)
                party.LfgSlots.Add(packet.ReadInt32());
            party.LfgCompletedMask = packet.ReadInt32();
        }

        SendPacketToClient(party);
    }

    [HandlesSmsg(Opcode.SMSG_GROUP_LIST, RemovedIn = ClientVersionBuild.V2_0_1_6180)]
    internal void HandleGroupListVanilla(WorldPacket packet)
    {
        GetSession().GameState.MasterLootCandidates = null;
        GetSession().GameState.LastMasterLootSentTarget = default;
        PartyUpdate party = new PartyUpdate();
        party.SequenceNum = GetSession().GameState.GroupUpdateCounter++;
        bool isRaid = packet.ReadBool();
        byte ownSubGroupAndFlags = packet.ReadUInt8();
        party.PartyIndex = (byte)(isRaid && GetSession().GameState.IsInBattleground() ? 1 : 0);
        party.PartyGUID = WowGuid128.Create(HighGuidType703.Party, (ulong)(1000 + party.PartyIndex));
        if (party.PartyIndex != 0)
            party.PartyFlags |= GroupFlags.FakeRaid;

        var uniqueMembers = new HashSet<WowGuid128>();
        uint membersCount = packet.ReadUInt32();
        if (membersCount > 0)
        {
            if (isRaid)
                party.PartyFlags |= GroupFlags.Raid;

            party.DifficultySettings = new PartyDifficultySettings();
            party.DifficultySettings.DungeonDifficultyID = DifficultyModern.Normal;

            if (ModernVersion.ExpansionVersion > 1)
                party.DifficultySettings.RaidDifficultyID = DifficultyModern.Raid25N;
            else
                party.DifficultySettings.RaidDifficultyID = DifficultyModern.Raid40;

            if (party.PartyIndex != 0)
                party.PartyType = GroupType.PvP;
            else
                party.PartyType = GroupType.Normal;

            PartyPlayerInfo player = new PartyPlayerInfo();
            player.GUID = GetSession().GameState.CurrentPlayerGuid;
            player.Name = GetSession().GameState.GetPlayerName(player.GUID);
            player.Subgroup = (byte)(ownSubGroupAndFlags & 0xF);
            player.Flags = (ownSubGroupAndFlags & 0x80) != 0 ? GroupMemberFlags.Assistant : GroupMemberFlags.None;
            player.Status = GroupMemberOnlineStatus.Online;
            party.PlayerList.Add(player);

            bool allAssist = true;
            for (uint i = 0; i < membersCount; i++)
            {
                PartyPlayerInfo member = new PartyPlayerInfo();
                member.Name = packet.ReadCString();
                member.GUID = packet.ReadGuid().To128(GetSession().GameState);
                member.Status = (GroupMemberOnlineStatus)packet.ReadUInt8();
                byte subGroupAndFlags = packet.ReadUInt8();
                member.Subgroup = (byte)(subGroupAndFlags & 0xF);
                member.Flags = (subGroupAndFlags & 0x80) != 0 ? GroupMemberFlags.Assistant : GroupMemberFlags.None;
                member.ClassId = GetSession().GameState.GetUnitClass(member.GUID);
                if (!member.Flags.HasAnyFlag(GroupMemberFlags.Assistant))
                    allAssist = false;

                if (!uniqueMembers.Contains(member.GUID))
                {
                    party.PlayerList.Add(member);
                    uniqueMembers.Add(member.GUID);
                }

                Session.GameState.UpdatePlayerCache(member.GUID, new PlayerCache
                { // it is not guaranteed that the client will invoke a QUERY_PLAYER_NAME. Client caches in between logins
                    Name = member.Name,
                    ClassId = member.ClassId,
                });
            }

            if (allAssist)
                party.PartyFlags |= GroupFlags.EveryoneAssistant;

            party.LeaderGUID = packet.ReadGuid().To128(GetSession().GameState);

            party.LootSettings = new PartyLootSettings();
            party.LootSettings.Method = (LootMethod)packet.ReadUInt8();
            party.LootSettings.LootMaster = packet.ReadGuid().To128(GetSession().GameState);
            party.LootSettings.Threshold = packet.ReadUInt8();

            GetSession().GameState.WeWantToLeaveGroup = false;
            // The packets about the group name this category (GetCurrentPartyIndex): a
            // battleground's raid is the instance group.
            GetSession().GameState.LastAnnouncedPartyIndex = party.PartyIndex;
            GetSession().GameState.CurrentGroups[party.PartyIndex] = party;
        }
        else
        {
            party.PartyFlags |= GroupFlags.Destroyed;
            if (party.PartyIndex == 0)
                party.PartyGUID = WowGuid128.Empty;
            party.LeaderGUID = WowGuid128.Empty;
            party.MyIndex = -1;
            GetSession().GameState.LastAnnouncedPartyIndex = 0;
            GetSession().GameState.CurrentGroups[party.PartyIndex] = null;
            if (GetSession().GameState.CurrentGroups[0] == null && GetSession().GameState.CurrentGroups[1] == null)
                GetSession().GameState.GroupAssignedRoles.Clear();

            if (!GetSession().GameState.WeWantToLeaveGroup)
                SendPacketToClient(new GroupUninvite()); // Send kick message
        }

        SendPacketToClient(party);
    }

    /// <summary>
    /// Reads the group-type flags at the head of <c>SMSG_GROUP_LIST</c>. 2.x (2.4.3 / CMaNGOS)
    /// writes two separate bytes — isRaid, then isBattleground; 3.3.x packs them into a single
    /// bitmask (BG = 0x01, Raid = 0x02, Lfg = 0x08). Reading the 2.x form as packed shifts every
    /// following field by one byte, so the member count is misread and the member loop runs off
    /// the end of the packet.
    /// </summary>
    internal static (bool IsRaid, bool IsBattleground, bool IsLfg) ReadGroupTypeFlags(WorldPacket packet, bool packed)
        => ReadGroupTypeFlags(packet, packed, out _);

    /// <param name="removed">
    /// The packed byte carries 0x10: the list is the server telling the player they are no longer
    /// in this group (TrinityCore <c>Group::RemoveMember</c> writes exactly that byte, then the
    /// group's GUID and no members).
    /// </param>
    internal static (bool IsRaid, bool IsBattleground, bool IsLfg) ReadGroupTypeFlags(WorldPacket packet, bool packed, out bool removed)
    {
        removed = false;
        if (packed)
        {
            byte groupType = packet.ReadUInt8();
            removed = (groupType & 0x10) != 0;
            return ((groupType & 0x02) != 0, (groupType & 0x01) != 0, (groupType & 0x08) != 0);
        }

        return (packet.ReadUInt8() != 0, packet.ReadUInt8() != 0, false);
    }

    [HandlesSmsg(Opcode.SMSG_GROUP_LIST, AddedIn = ClientVersionBuild.V2_0_1_6180)]
    internal void HandleGroupListTBC(WorldPacket packet)
    {
        GetSession().GameState.MasterLootCandidates = null;
        GetSession().GameState.LastMasterLootSentTarget = default;
        PartyUpdate party = new PartyUpdate();
        party.SequenceNum = GetSession().GameState.GroupUpdateCounter++;
        var (isRaid, isBattleground, isLfg) =
            ReadGroupTypeFlags(packet, LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958), out bool removed);
        byte ownSubGroup = packet.ReadUInt8();
        byte ownGroupFlags = packet.ReadUInt8();
        byte ownRoles = 0;
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            ownRoles = packet.ReadUInt8(); // own LFG roles
        byte lfgDungeonStatus = 0;
        uint lfgDungeonId = 0;
        if (isLfg)
        {
            // AC/TC 3.3.5a Group::BuildGroupList: "2 == finished dungeon, else 0", then
            // GetDungeon(guid, asId: true) — a BARE dungeon ID, not a full LFG slot.
            lfgDungeonStatus = packet.ReadUInt8();
            lfgDungeonId = packet.ReadUInt32();
            if (IsCataLegacy)
                packet.ReadUInt8(); // TrinityCore 4.3.4 Group::SendUpdateToPlayer: one more byte here
        }
        party.PartyIndex = (byte)(isBattleground ? 1 : 0);
        party.PartyGUID = packet.ReadGuid().To128(GetSession().GameState);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            packet.ReadUInt32(); // group counter
        if (party.PartyIndex != 0)
            party.PartyFlags |= GroupFlags.FakeRaid;

        if (isLfg)
        {
            // Legacy signals "this party is in an LFG dungeon" purely through the group type
            // flags and this trailing block; the LFG status packets never say it. Dropping it
            // left the modern client with no idea it was inside a dungeon, so the minimap eye
            // only ever offered "Leave Queue" and reported the queue as paused — no
            // "Teleport out of Dungeon" / "Teleport to Dungeon" entry, which is what a native
            // 3.3.5a client shows. Field semantics mirror TC 3.4.3 Group::SendUpdateToPlayer.
            // TC 3.4.3 Group::ConvertToLFG sets BOTH flags together and moves the group into
            // the instance category:
            //   m_groupFlags   |= GROUP_FLAG_LFG | GROUP_FLAG_LFG_RESTRICTED
            //   m_groupCategory = GROUP_CATEGORY_INSTANCE
            // PartyIndex IS that category (HOME = 0, INSTANCE = 1). Announcing an LFG dungeon
            // group as a home group left the client's "Leave Instance Group" entry with no
            // instance-category group to act on, so clicking it sent nothing at all. Legacy
            // only exposes the restricted bit sometimes (AC persists groupType 8, not 12), so
            // set it unconditionally for LFG groups the way the modern server does.
            party.PartyFlags |= GroupFlags.Lfg | GroupFlags.LfgRestricted;
            party.PartyIndex = 1; // GROUP_CATEGORY_INSTANCE

            uint lfgSlot = GetSession().GameState.GetLfgSlotForDungeon(lfgDungeonId);
            party.LfgInfos = new PartyLFGInfo
            {
                Slot = lfgSlot,
                MyFlags = lfgDungeonStatus, // 2 == dungeon completed, matching TC's own mapping
                MyRandomSlot = lfgSlot,
                BootCount = 0,
                Aborted = false,
                MyPartialClear = 0,
                MyGearDiff = 0.0f,
                MyStrangerCount = 0,
                MyKickVoteCount = 0,
                MyFirstReward = false,
            };
        }

        var uniqueMembers = new HashSet<WowGuid128>();
        uint membersCount = packet.ReadUInt32();
        // A 4.3.4 server makes the group at the first invite and lists it at once, with nobody in
        // it yet; earlier servers list a group once someone has joined, and one of none only to
        // say it is gone. A list of none that still names a group is that group waiting for its
        // first member: passed on as a group gone, the inviter was told they had been removed.
        //
        // The one such list that must go through is the server taking the player out of the group
        // of the instance category. A battleground's raid ends with nothing else: no removal
        // packet as a party gets, only this list with the removal mark, and dropping it left the
        // client in a raid of a battle it had left.
        //
        // And a battleground's raid begins the other way round: the player is put into it alone on
        // entering, the others follow one list at a time. That list of none is a raid of one, and
        // the client is in the battle's raid from the moment it is in the battle.
        bool raidOfOne = membersCount == 0 && IsCataLegacy && isBattleground && !removed && !party.PartyGUID.IsEmpty();
        //
        // The same list with the removal mark ends a party too. SMSG_GROUP_DESTROYED comes with it
        // and only says so in chat: without the list the client stayed in a party of two that the
        // server no longer had, after leaving it or after the other member had left.
        bool leftInstanceGroup = false;
        bool leftHomeGroup = false;
        if (membersCount == 0 && IsCataLegacy && !party.PartyGUID.IsEmpty() && !raidOfOne)
        {
            var instanceGroup = GetSession().GameState.CurrentGroups[1];
            leftInstanceGroup = removed && instanceGroup != null && instanceGroup.PartyGUID == party.PartyGUID;
            var homeGroup = GetSession().GameState.CurrentGroups[0];
            leftHomeGroup = removed && !leftInstanceGroup && homeGroup != null && homeGroup.PartyGUID == party.PartyGUID;
            if (!leftInstanceGroup && !leftHomeGroup)
                return;
        }

        if (membersCount > 0 || raidOfOne)
        {
            // A legacy group can MOVE between party categories mid-life — a premade party sits
            // in the home category and jumps to the instance category the moment it converts to
            // LFG. The client tracks the two categories independently, so announcing the group
            // under its new index while leaving the old one alive left a phantom party behind
            // that nothing could ever remove: "Leave Party" produced CMSG_GROUP_DISBAND, the
            // server had no group left to disband and answered nothing, and the client stayed
            // stuck in a group that no longer existed. Tear the old category down first.
            byte previousIndex = GetSession().GameState.LastAnnouncedPartyIndex;
            if (previousIndex != party.PartyIndex && GetSession().GameState.CurrentGroups[previousIndex] != null)
            {
                PartyUpdate migrated = new PartyUpdate();
                migrated.SequenceNum = GetSession().GameState.GroupUpdateCounter++;
                migrated.PartyIndex = previousIndex;
                migrated.PartyFlags |= GroupFlags.Destroyed;
                migrated.PartyGUID = WowGuid128.Empty;
                migrated.LeaderGUID = WowGuid128.Empty;
                migrated.MyIndex = -1;
                GetSession().GameState.CurrentGroups[previousIndex] = null;
                SendPacketToClient(migrated);
            }

            if (isRaid)
                party.PartyFlags |= GroupFlags.Raid;

            // Only battlegrounds are a PvP group. LFG dungeon groups also sit in the instance
            // category (PartyIndex 1) but are still GROUP_TYPE_NORMAL — TC 3.4.3 sends
            // `PartyType = IsCreated() ? GROUP_TYPE_NORMAL : GROUP_TYPE_NONE` irrespective of
            // category, so this has to key off the BG flag rather than the index.
            if (isBattleground)
                party.PartyType = GroupType.PvP;
            else
                party.PartyType = GroupType.Normal;

            PartyPlayerInfo player = new PartyPlayerInfo();
            player.GUID = GetSession().GameState.CurrentPlayerGuid;
            player.Name = GetSession().GameState.GetPlayerName(player.GUID);
            player.Subgroup = ownSubGroup;
            player.Flags = (GroupMemberFlags)ownGroupFlags;
            player.Status = GroupMemberOnlineStatus.Online;
            player.RolesAssigned = ResolveAssignedRole(player.GUID, ownRoles);
            party.PlayerList.Add(player);

            bool allAssist = true;
            for (uint i = 0; i < membersCount; i++)
            {
                PartyPlayerInfo member = new PartyPlayerInfo();
                member.Name = packet.ReadCString();
                member.GUID = packet.ReadGuid().To128(GetSession().GameState);
                member.Status = (GroupMemberOnlineStatus)packet.ReadUInt8();
                member.Subgroup = packet.ReadUInt8();
                member.Flags = (GroupMemberFlags)packet.ReadUInt8();
                byte memberRoles = 0;
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
                    memberRoles = packet.ReadUInt8(); // member LFG roles
                member.RolesAssigned = ResolveAssignedRole(member.GUID, memberRoles);
                member.ClassId = GetSession().GameState.GetUnitClass(member.GUID);
                if (!member.Flags.HasAnyFlag(GroupMemberFlags.Assistant))
                    allAssist = false;

                if (!uniqueMembers.Contains(member.GUID))
                {
                    party.PlayerList.Add(member);
                    uniqueMembers.Add(member.GUID);
                }

                Session.GameState.UpdatePlayerCache(member.GUID, new PlayerCache
                { // it is not guaranteed that the client will invoke a QUERY_PLAYER_NAME. Client caches in between logins
                    Name = member.Name,
                    ClassId = member.ClassId,
                });
            }

            if (allAssist && !raidOfOne)
                party.PartyFlags |= GroupFlags.EveryoneAssistant;

            party.LeaderGUID = packet.ReadGuid().To128(GetSession().GameState);

            party.LootSettings = new PartyLootSettings();
            party.DifficultySettings = new PartyDifficultySettings();
            int difficultyId = 0;
            if (raidOfOne)
            {
                // TrinityCore 4.3.4 Group::SendUpdateToPlayer writes loot and difficulty only for a
                // group of more than one; the list ends with the leader.
                party.LootSettings.LootMaster = WowGuid128.Empty;
            }
            else
            {
                party.LootSettings.Method = (LootMethod)packet.ReadUInt8();
                party.LootSettings.LootMaster = packet.ReadGuid().To128(GetSession().GameState);
                party.LootSettings.Threshold = packet.ReadUInt8();
                difficultyId = packet.ReadUInt8();
            }
            party.DifficultySettings.DungeonDifficultyID = ((DifficultyLegacy)difficultyId).CastEnum<DifficultyModern>();

            if (raidOfOne)
            {
                party.DifficultySettings.RaidDifficultyID = RaidDifficulties.ToLegacyId(0);
                party.DifficultySettings.LegacyRaidDifficultyID = DifficultyModern.Raid10N;
            }
            else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            {
                byte acRaid = packet.ReadUInt8();
                // Native puts the active raid difficulty in RaidDifficultyID as 3-6 and leaves
                // LegacyRaidDifficultyID at its default; the client's picker reads the former.
                // The Classic ids (175/176/193/194) belong to a different difficulty family and
                // match no row in the WotLK raid dropdown.
                party.DifficultySettings.RaidDifficultyID = RaidDifficulties.ToLegacyId(acRaid);
                party.DifficultySettings.LegacyRaidDifficultyID = DifficultyModern.Raid10N;
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958) && !IsCataLegacy)
                    packet.ReadUInt8(); // 3.3 dynamic heroic bit; gone again in 4.3.4
            }
            else if (ModernVersion.ExpansionVersion > 1)
                party.DifficultySettings.RaidDifficultyID = DifficultyModern.Raid25N;
            else
                party.DifficultySettings.RaidDifficultyID = DifficultyModern.Raid40;

            GetSession().GameState.WeWantToLeaveGroup = false;
            GetSession().GameState.LastAnnouncedPartyIndex = party.PartyIndex;
            var previousGroup = GetSession().GameState.CurrentGroups[party.PartyIndex];
            GetSession().GameState.CurrentGroups[party.PartyIndex] = party;
            PruneAssignedRoles(party.PlayerList);

            if (previousGroup == null || previousGroup.PartyGUID != party.PartyGUID)
                RequestRaidTargetIconList();
        }
        else
        {
            // A disbanded group arrives with its type flags already cleared, so the LFG/BG bits
            // that placed it in the instance category are gone and PartyIndex computes to 0.
            // Addressing the destroy to the home category would leave the client's instance
            // group alive forever — the minimap LFG eye stayed up, stuck on "paused", after
            // "Leave Instance Group" had genuinely removed the group server-side.
            if (leftHomeGroup)
            {
                // named by its GUID: it may be left while a battleground's raid is the group
                // announced last, and that one stays
                party.PartyIndex = 0;
                if (GetSession().GameState.LastAnnouncedPartyIndex == 0 || GetSession().GameState.CurrentGroups[1] == null)
                    GetSession().GameState.LastAnnouncedPartyIndex = 0;
            }
            else
            {
                party.PartyIndex = GetSession().GameState.LastAnnouncedPartyIndex;
                GetSession().GameState.LastAnnouncedPartyIndex = 0;
            }

            party.PartyFlags |= GroupFlags.Destroyed;
            if (party.PartyIndex  == 0)
                party.PartyGUID = WowGuid128.Empty;
            party.LeaderGUID = WowGuid128.Empty;
            party.MyIndex = -1;
            GetSession().GameState.CurrentGroups[party.PartyIndex] = null;
            if (GetSession().GameState.CurrentGroups[0] == null && GetSession().GameState.CurrentGroups[1] == null)
                GetSession().GameState.GroupAssignedRoles.Clear();

            // leaving a battle is not being removed from a group; nothing to say. TrinityCore
            // 4.3.4 says the rest itself: SMSG_GROUP_UNINVITE to a member it removes, and
            // SMSG_GROUP_DESTROYED to everyone in a group it disbands. Said here as well, a leader
            // who removed the last member, and everyone in a disbanded group, read "You have been
            // removed from the group." after "Your group has been disbanded.". A 3.3.5a server
            // sends the same, but no 3.3.5a session has been played without this.
            if (!GetSession().GameState.WeWantToLeaveGroup && !leftInstanceGroup && !IsCataLegacy)
                SendPacketToClient(new GroupUninvite()); // Send kick message
        }

        SendPacketToClient(party);

        // Legacy never announces the END of an LFG association. Disbanding an LFG group emits
        // only SMSG_GROUP_LIST, and LFGMgr::LeaveLfg has no LFG_STATE_DUNGEON case at all, so
        // the client keeps the last LFG status it received — the minimap eye stayed up on
        // "paused" long after the group was gone server-side. The group dropping its LFG flag
        // is the only signal available, and it is an honest one: unlike a teleport-out (where
        // the association legitimately survives and the eye SHOULD stay), here it really has
        // ended, and a modern server would send REMOVED_FROM_QUEUE of its own accord.
        bool wasLfg = GetSession().GameState.LastGroupWasLfg;
        GetSession().GameState.LastGroupWasLfg = isLfg;
        if (wasLfg && !isLfg)
            SendLfgAssociationEnded();
    }

    /// <summary>
    /// Tells the modern client its LFG association is over, mirroring what TC 3.4.3 emits from
    /// SendLfgUpdateStatus for LFG_UPDATETYPE_REMOVED_FROM_QUEUE.
    /// </summary>
    private void SendLfgAssociationEnded()
    {
        Log.Print(LogType.Debug, "LFG[diag]: group dropped its LFG flag, synthesising REMOVED_FROM_QUEUE");

        // The client tracks party-scoped and player-scoped LFG state separately, so a removal
        // has to be announced for BOTH or the half that was not told keeps the minimap eye
        // alive. Legacy servers do exactly this: on REMOVED_FROM_QUEUE, AC sends
        // SendLfgUpdateParty followed by SendLfgUpdatePlayer (LFGMgr.cpp 939 / 953) and TC 3.4.3
        // calls SendLfgUpdateStatus with party=true then party=false (LFGMgr.cpp 676 / 687).
        //
        // Sending only the party variant looked like it worked because leaving from INSIDE a
        // dungeon is followed by a teleport, and the world transfer reloads client state and
        // clears the eye as a side effect. Leaving from outside produces no teleport, and the
        // stale player-scoped state was then plainly visible.
        SendLfgAssociationEnded(isParty: true);
        SendLfgAssociationEnded(isParty: false);

        // Both removals must carry the ticket the client has been seeing, so only forget it
        // afterwards — the next queue then mints a fresh one, as TC does per JoinLfg.
        GetSession().GameState.ResetLfgTicket();
    }

    private void SendLfgAssociationEnded(bool isParty)
    {
        bool isV343 = ModernVersion.IsWotLKClassicOrLater;

        DFUpdateStatus status = new DFUpdateStatus();
        status.Ticket = MakeLfgTicket();
        status.RequestedRoles = GetSession().GameState.LfgRequestedRoles;
        status.SubType = isV343 ? LfgUpdateTypes.ModernQueueDungeon : (byte)0;
        status.Reason = isV343 ? LfgUpdateTypes.ModernRemovedFromQueue : (byte)0;
        status.NotifyUI = true;
        status.IsParty = isParty;
        status.Joined = false;
        status.LfgJoined = false;
        status.Queued = false;

        Log.Print(LogType.Debug,
            $"LFG[diag]: DFUpdateStatus(removed) subType={status.SubType} reason={status.Reason} " +
            $"requestedRoles=0x{status.RequestedRoles:X2} isParty={status.IsParty}");

        SendPacketToClient(status);
    }

    [HandlesSmsg(Opcode.SMSG_GROUP_UNINVITE)]
    internal void HandleGroupUninvite(WorldPacket packet)
    {
        GroupUninvite party = new GroupUninvite();
        SendPacketToClient(party);
    }

    [HandlesSmsg(Opcode.SMSG_GROUP_NEW_LEADER)]
    internal void HandleGroupNewLeader(WorldPacket packet)
    {
        GroupNewLeader party = new GroupNewLeader();
        party.Name = packet.ReadCString();
        party.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();
        SendPacketToClient(party);
    }

    [HandlesSmsg(Opcode.MSG_RAID_READY_CHECK, RemovedIn = ClientVersionBuild.V2_0_1_6180)]
    internal void HandleRaidReadyCheckVanilla(WorldPacket packet)
    {
        if (!packet.CanRead())
        {
            ReadyCheckStarted ready = new ReadyCheckStarted();
            ready.InitiatorGUID = GetSession().GameState.GetCurrentGroupLeader();
            ready.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();
            ready.PartyGUID = GetSession().GameState.GetCurrentGroupGuid();
            SendPacketToClient(ready);

            GetSession().GameState.GroupReadyCheckResponses = 0;
            ArmReadyCheckDeadline(ready.Duration);
        }
        else
        {
            ReadyCheckResponse ready = new ReadyCheckResponse();
            ready.Player = packet.ReadGuid().To128(GetSession().GameState);
            ready.IsReady = packet.ReadBool();
            ready.PartyGUID = GetSession().GameState.GetCurrentGroupGuid();
            SendPacketToClient(ready);

            GetSession().GameState.GroupReadyCheckResponses++;
            if (GetSession().GameState.GroupReadyCheckResponses >= GetSession().GameState.GetCurrentGroupSize())
            {
                GetSession().GameState.GroupReadyCheckResponses = 0;
                StopReadyCheckDeadline();
                ReadyCheckCompleted completed = new ReadyCheckCompleted();
                completed.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();
                completed.PartyGUID = GetSession().GameState.GetCurrentGroupGuid();
                SendPacketToClient(completed);
            }
        }
    }

    [HandlesSmsg(Opcode.MSG_RAID_READY_CHECK, AddedIn = ClientVersionBuild.V2_0_1_6180)]
    internal void HandleRaidReadyCheck(WorldPacket packet)
    {
        ReadyCheckStarted ready = new ReadyCheckStarted();
        ready.InitiatorGUID = packet.ReadGuid().To128(GetSession().GameState);
        ready.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();
        ready.PartyGUID = GetSession().GameState.GetCurrentGroupGuid();
        SendPacketToClient(ready);

        // A check that ends without a full set of answers used to leave the counter
        // non-zero, so the next one completed early.
        GetSession().GameState.GroupReadyCheckResponses = 0;
        ArmReadyCheckDeadline(ready.Duration);
    }

    [HandlesSmsg(Opcode.MSG_RAID_READY_CHECK_CONFIRM, AddedIn = ClientVersionBuild.V2_0_1_6180)]
    internal void HandleRaidReadyCheckConfirm(WorldPacket packet)
    {
        ReadyCheckResponse ready = new ReadyCheckResponse();
        ready.Player = packet.ReadGuid().To128(GetSession().GameState);
        ready.IsReady = packet.ReadBool();
        ready.PartyGUID = GetSession().GameState.GetCurrentGroupGuid();
        SendPacketToClient(ready);

        GetSession().GameState.GroupReadyCheckResponses++;
        if (GetSession().GameState.GroupReadyCheckResponses >= GetSession().GameState.GetCurrentGroupSize())
        {
            GetSession().GameState.GroupReadyCheckResponses = 0;
            StopReadyCheckDeadline();
            ReadyCheckCompleted completed = new ReadyCheckCompleted();
            completed.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();
            completed.PartyGUID = GetSession().GameState.GetCurrentGroupGuid();
            SendPacketToClient(completed);
        }
    }

    [HandlesSmsg(Opcode.MSG_RAID_READY_CHECK_FINISHED, AddedIn = ClientVersionBuild.V2_0_1_6180)]
    internal void HandleRaidReadyCheckFinished(WorldPacket packet)
    {
        GetSession().GameState.GroupReadyCheckResponses = 0;
        StopReadyCheckDeadline();

        ReadyCheckCompleted ready = new ReadyCheckCompleted();
        ready.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();
        ready.PartyGUID = GetSession().GameState.GetCurrentGroupGuid();
        SendPacketToClient(ready);
    }

    // The modern client waits for the server to end a ready check once the Duration it
    // was given lapses. Legacy 3.3.5a has no server-side timer: MSG_RAID_READY_CHECK_FINISHED
    // travels *up* from the leader's own client, which the modern client never sends, so
    // the legacy server never broadcasts it and a check nobody answers stays pending
    // forever. Synthesize the deadline here instead.
    // Armed on the packet thread, disposed from either that thread or the timer's own
    // callback, so every swap goes through Interlocked - a plain read-then-dispose can
    // drop a timer another thread has just armed.
    private System.Threading.Timer? _readyCheckTimer;

    private void ArmReadyCheckDeadline(ulong durationMs)
    {
        // Fire on the duration boundary, the way a native server ends the check
        // (Group::UpdateReadyCheck, READYCHECK_DURATION = 35000). Landing late means the
        // client has already torn its ready-check frame down and the completion arrives
        // with nothing left to close.
        long dueMs = (long)durationMs;
        var timer = new System.Threading.Timer(OnReadyCheckDeadline, dueMs, dueMs, System.Threading.Timeout.Infinite);
        System.Threading.Interlocked.Exchange(ref _readyCheckTimer, timer)?.Dispose();
    }

    private void StopReadyCheckDeadline()
    {
        System.Threading.Interlocked.Exchange(ref _readyCheckTimer, null)?.Dispose();
    }

    private void OnReadyCheckDeadline(object? state)
    {
        // The body reads and writes GameState, so it belongs to the session's owner, not to the
        // timer thread.
        var session = _globalSession;
        if (session == null)
            return;

        // A ready check happens a few times an hour, so the closure here costs nothing worth saving.
        session.Executor.Post(due => RunReadyCheckDeadline(due), state);
    }

    private void RunReadyCheckDeadline(object? state)
    {
        StopReadyCheckDeadline();

        // An escaped exception on a timer thread has no handler above it and would take
        // the whole proxy down, so nothing in here is allowed to propagate.
        try
        {
            if (_closing || !IsConnected())
                return;

            var gameState = GetSession().GameState;
            uint responses = gameState.GroupReadyCheckResponses;
            uint expected = gameState.GetCurrentGroupSize();
            gameState.GroupReadyCheckResponses = 0;

            // Everyone answered in time - the response counter already completed the check.
            if (responses >= expected)
                return;

            WowGuid128 partyGuid = gameState.GetCurrentGroupGuid();

            // The group broke up while the check was running; there is nothing to complete.
            if (partyGuid.IsEmpty())
                return;

            WorldClientLogMessages.ReadyCheckDeadlineLapsed(_melLog, _sourceFile, _netDirNone, (long)state!, responses, expected);

            ReadyCheckCompleted completed = new ReadyCheckCompleted();
            completed.PartyIndex = gameState.GetCurrentPartyIndex();
            completed.PartyGUID = partyGuid;
            SendPacketToClient(completed);
        }
        catch (Exception ex)
        {
            WorldClientLogMessages.ReadyCheckDeadlineFailed(_melLog, _sourceFile, _netDirNone, ex);
        }
    }

    // Legacy only sends the full icon list when a client asks for it - Group::SendTargetIconList
    // has exactly one caller, the icon == 0xFF branch of HandleRaidTargetUpdateOpcode. The 3.3.5a
    // client sent that request itself on joining a group; the modern client never does, because a
    // modern server pushes SMSG_SEND_RAID_TARGET_UPDATE_ALL unsolicited. Without this, someone
    // joining a group that already carries marks sees none of them.
    private void RequestRaidTargetIconList()
    {
        if (!ModernVersion.IsWotLKClassicOrLater)
            return;

        WorldPacket packet = new WorldPacket(Opcode.MSG_RAID_TARGET_UPDATE);
        packet.WriteUInt8(0xFF);
        SendPacket(packet);
    }

    [HandlesSmsg(Opcode.MSG_RAID_TARGET_UPDATE)]
    internal void HandleRaidTargetUpdate(WorldPacket packet)
    {
        bool isFullUpdate = packet.ReadBool();
        if (isFullUpdate)
        {
            SendRaidTargetUpdateAll update = new SendRaidTargetUpdateAll();
            update.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();
            while (packet.CanRead())
            {
                sbyte symbol = packet.ReadInt8();
                WowGuid128 guid = packet.ReadGuid().To128(GetSession().GameState);
                update.TargetIcons.Add(new Tuple<sbyte, WowGuid128>(symbol, guid));
            }
            SendPacketToClient(update);
        }
        else
        {
            SendRaidTargetUpdateSingle update = new SendRaidTargetUpdateSingle();
            update.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                update.ChangedBy = packet.ReadGuid().To128(GetSession().GameState);
            else
                update.ChangedBy = GetSession().GameState.CurrentPlayerGuid;
            
            update.Symbol = packet.ReadInt8();
            update.Target = packet.ReadGuid().To128(GetSession().GameState);
            SendPacketToClient(update);
        }
    }

    [HandlesSmsg(Opcode.SMSG_SUMMON_REQUEST)]
    internal void HandleSummonRequest(WorldPacket packet)
    {
        SummonRequest summon = new SummonRequest();
        summon.SummonerGUID = packet.ReadGuid().To128(GetSession().GameState);
        summon.SummonerVirtualRealmAddress = GetSession().RealmId.GetAddress();
        summon.AreaID = packet.ReadInt32();
        packet.ReadUInt32(); // time to accept
        SendPacketToClient(summon);
    }

    uint _requestBgPlayerPosCounter = 0;

    [HandlesSmsg(Opcode.SMSG_PARTY_MEMBER_PARTIAL_STATE, RemovedIn = ClientVersionBuild.V2_0_1_6180)]
    internal void HandlePartyMemberStats(WorldPacket packet)
    {
        if (GetSession().GameState.CurrentMapId == (uint)BattlegroundMapID.WarsongGulch &&
           (GetSession().GameState.HasWsgAllyFlagCarrier || GetSession().GameState.HasWsgHordeFlagCarrier))
        {
            if (_requestBgPlayerPosCounter++ > 10) // don't spam every time somebody moves
            {
                WorldPacket packet2 = new WorldPacket(BattlegroundPlayerPositionsRequest);
                SendPacket(packet2);
                _requestBgPlayerPosCounter = 0;
            }
        }

        PartyMemberPartialState state = new PartyMemberPartialState();
        state.AffectedGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        var updateFlags = (GroupUpdateFlagVanilla)packet.ReadUInt32();

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.Status))
            state.StatusFlags = packet.ReadUInt8();// GroupMemberOnlineStatus

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.CurrentHealth))
            state.CurrentHealth = packet.ReadUInt16();

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.MaxHealth))
            state.MaxHealth = packet.ReadUInt16();

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PowerType))
            state.PowerType = packet.ReadUInt8();

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.CurrentPower))
            state.CurrentPower = packet.ReadUInt16();

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.MaxPower))
            state.MaxPower = packet.ReadUInt16();

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.Level))
            state.Level = packet.ReadUInt16();

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.Zone))
            state.ZoneID = packet.ReadUInt16();

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.Position))
        {
            PartyMemberPartialState.Vector3_UInt16 position = default;
            position.X = packet.ReadInt16();
            position.Y = packet.ReadInt16();
            state.Position = position;
        }

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.Auras))
        {
            if (state.Auras == null)
                state.Auras = new List<PartyMemberAuraStates>();

            var auraMask = packet.ReadUInt32(); // Positive Aura Mask

            byte maxAura = 32;

            for (byte i = 0; i < maxAura; ++i)
            {
                if ((auraMask & (1ul << i)) == 0)
                    continue;

                PartyMemberAuraStates aura = new PartyMemberAuraStates();
                aura.SpellId = packet.ReadUInt16();
                if (aura.SpellId != 0)
                {
                    aura.ActiveFlags = 1;
                    aura.AuraFlags = (ushort)AuraFlagsModern.Positive;
                }
                state.Auras.Add(aura);
            }
        }

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.AurasNegative))
        {
            if (state.Auras == null)
                state.Auras = new List<PartyMemberAuraStates>();

            var auraMask = packet.ReadUInt16(); // Negative Aura Mask

            byte maxAura = 48;

            for (byte i = 0; i < maxAura; ++i)
            {
                if ((auraMask & (1ul << i)) == 0)
                    continue;

                PartyMemberAuraStates aura = new PartyMemberAuraStates();
                aura.SpellId = packet.ReadUInt16();
                if (aura.SpellId != 0)
                {
                    aura.ActiveFlags = 1;
                    aura.AuraFlags = (ushort)AuraFlagsModern.Negative;
                }
                state.Auras.Add(aura);
            }
        }

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetGuid))
        {
            if (state.Pet == null)
                state.Pet = new PartyMemberPetStats();
            state.Pet.NewPetGuid = packet.ReadGuid().To128(GetSession().GameState);
        }
            

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetName))
        {
            if (state.Pet == null)
                state.Pet = new PartyMemberPetStats();
            state.Pet.NewPetName = packet.ReadCString();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetModelId))
        {
            if (state.Pet == null)
                state.Pet = new PartyMemberPetStats();
            state.Pet.DisplayID = packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetCurrentHealth))
        {
            if (state.Pet == null)
                state.Pet = new PartyMemberPetStats();
            state.Pet.Health = packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetMaxHealth))
        {
            if (state.Pet == null)
                state.Pet = new PartyMemberPetStats();
            state.Pet.MaxHealth = packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetPowerType))
            packet.ReadUInt8(); // Pet Power Type

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetCurrentPower))
            packet.ReadInt16(); // Pet Current Power

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetMaxPower))
            packet.ReadInt16(); // Pet Max Power

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetAuras))
        {
            if (state.Pet == null)
                state.Pet = new PartyMemberPetStats();
            if (state.Pet.Auras == null)
                state.Pet.Auras = new List<PartyMemberAuraStates>();

            var auraMask = packet.ReadUInt32(); // Pet Positive Aura Mask

            byte maxAura = 32;

            for (byte i = 0; i < maxAura; ++i)
            {
                if ((auraMask & (1ul << i)) == 0)
                    continue;

                PartyMemberAuraStates aura = new PartyMemberAuraStates();
                aura.SpellId = packet.ReadUInt16();
                if (aura.SpellId != 0)
                {
                    aura.ActiveFlags = 1;
                    aura.AuraFlags = (ushort)AuraFlagsModern.Positive;
                }
                state.Pet.Auras.Add(aura);
            }
        }

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetAurasNegative))
        {
            if (state.Pet == null)
                state.Pet = new PartyMemberPetStats();
            if (state.Pet.Auras == null)
                state.Pet.Auras = new List<PartyMemberAuraStates>();

            var auraMask = packet.ReadUInt16(); // Pet Negative Aura Mask

            byte maxAura = 48;

            for (byte i = 0; i < maxAura; ++i)
            {
                if ((auraMask & (1ul << i)) == 0)
                    continue;

                PartyMemberAuraStates aura = new PartyMemberAuraStates();
                aura.SpellId = packet.ReadUInt16();
                if (aura.SpellId != 0)
                {
                    aura.ActiveFlags = 1;
                    aura.AuraFlags = (ushort)AuraFlagsModern.Negative;
                }
                state.Pet.Auras.Add(aura);
            }
        }

        SendPacketToClient(state);
    }

    [HandlesSmsg(Opcode.SMSG_PARTY_MEMBER_PARTIAL_STATE, AddedIn = ClientVersionBuild.V2_0_1_6180)]
    internal void HandlePartyMemberStatsTbc(WorldPacket packet)
    {
        if (GetSession().GameState.CurrentMapId == (uint)BattlegroundMapID.WarsongGulch &&
           (GetSession().GameState.HasWsgAllyFlagCarrier || GetSession().GameState.HasWsgHordeFlagCarrier))
        {
            if (_requestBgPlayerPosCounter++ > 10) // don't spam every time somebody moves
            {
                WorldPacket packet2 = new WorldPacket(BattlegroundPlayerPositionsRequest);
                SendPacket(packet2);
                _requestBgPlayerPosCounter = 0;
            }
        }

        // The GUID and the update mask are the first two fields and neither allocates
        // (WowGuid128 is a readonly record struct). Decide the throttle from them so a
        // rate-limited update costs a dictionary probe instead of a packet object, an
        // aura list and a WorldPacket that are immediately discarded. Measured on a
        // 15v15 AzerothCore battleground: 95.6% of updates are dropped, at ~470 bytes
        // each, which is 74% of everything this handler allocated.
        WowGuid128 affectedGuid = packet.ReadPackedGuid().To128(GetSession().GameState);

        const string Op = nameof(Opcode.SMSG_PARTY_MEMBER_PARTIAL_STATE);
        if (!packet.CanRead(4))
        {
            WarnTruncated(Op, packet, "updateFlags", 4);
            return;
        }

        if (IsCataLegacy)
        {
            HandlePartyMemberPartialStateCata(packet, affectedGuid);
            return;
        }

        GroupUpdateFlagTBC updateFlags = (GroupUpdateFlagTBC)packet.ReadUInt32();

        if (ShouldThrottlePartyMemberState(affectedGuid, updateFlags))
            return;

        // NPCBot / Playerbot truncates: it sets `mask = GROUP_UPDATE_FULL` (0x7FFFF) but only
        // writes the leading subset of fields (typically through POSITION). Trusting the mask
        // would over-read the buffer. The parser checks CanRead before each conditional field
        // and returns the partial state filled up to the truncation point so the modern client
        // still gets HP / power / position updates instead of nothing.
        PartyMemberPartialState state = ParsePartyMemberPartialState(packet, affectedGuid, updateFlags);
        SendPacketToClient(state);
    }

    // Bot-filled battlegrounds re-send position/health for every raid member on every step.
    // Forward those at most once per PartyMemberStateMinIntervalMs per member; anything
    // carrying state the frame cannot infer (status, level, spec, zone, auras, vehicle seat,
    // power type) or a death goes straight through, so nothing the player needs is delayed.
    // Fields a raid frame cannot infer between updates. Anything outside this set — status,
    // level, zone, auras, pet, vehicle seat — must reach the client promptly, so only updates
    // whose mask falls entirely inside it are candidates for rate limiting.
    private const GroupUpdateFlagTBC HighFrequencyFlags =
        GroupUpdateFlagTBC.CurrentHealth | GroupUpdateFlagTBC.MaxHealth |
        GroupUpdateFlagTBC.PowerType | GroupUpdateFlagTBC.CurrentPower | GroupUpdateFlagTBC.MaxPower |
        GroupUpdateFlagTBC.Position;

    // Bot-filled battlegrounds re-send health, power and position for every raid member on
    // every step: 356 updates per second sustained on a 15v15, of which the client needs
    // about 4%. Decided from the update mask alone so a dropped update never allocates.
    private bool ShouldThrottlePartyMemberState(WowGuid128 affectedGuid, GroupUpdateFlagTBC updateFlags)
    {
        if (!ModernVersion.IsWotLKClassicOrLater)
            return false;

        int minIntervalMs = GetSession().ThrottlingOptions.PartyMemberStateMinIntervalMs;
        if (minIntervalMs <= 0)
            return false;

        if ((updateFlags & ~HighFrequencyFlags) != 0)
            return false;

        long nowMs = Environment.TickCount64;
        // Single hash lookup: the ref is the slot, so the "seen before" check and the
        // timestamp write share one probe instead of TryGetValue + indexer set.
        ref long lastMs = ref CollectionsMarshal.GetValueRefOrAddDefault(
            GetSession().GameState.LastPartyMemberStateTickMs, affectedGuid, out bool existed);

        if (existed && nowMs - lastMs < minIntervalMs)
            return true;

        lastMs = nowMs;
        return false;
    }

    // NPCBot/Playerbot wire bug: bot sets `mask = GROUP_UPDATE_FULL (0x7FFFF)` claiming all 19
    // group-update flags but only writes the leading subset of fields (typically through
    // POSITION). To avoid trusting the mask blindly, every conditional field read below first
    // checks `packet.CanRead(N)` and bails cleanly through `WarnTruncated` when the payload
    // runs out — the partial state already populated is what gets forwarded to the client.
    // Rate-limited warn (first 3 per process) preserves diagnostic value without spamming.
    private static int s_partyMemberDumpBudget = 3;

    private static bool WarnTruncated(string opcodeName, WorldPacket packet, string field, int needed)
    {
        int remaining = System.Threading.Interlocked.Decrement(ref s_partyMemberDumpBudget);
        if (remaining >= 0)
        {
            byte[] all = packet.GetData();
            int len = (int)packet.GetSize();
            int dumpLen = Math.Min(160, len);
            string hex = BitConverter.ToString(all, 0, dumpLen);
            Log.Print(LogType.Warn,
                $"{opcodeName} truncated at field={field}: need {needed} bytes, have {packet.Remaining()} (size={len}). Forwarding partial state. hex={hex}");
        }
        return false;
    }

    // The GUID and update mask are read by the caller so the throttle can drop an update
    // without allocating anything; both are passed in rather than re-read here.
    private PartyMemberPartialState ParsePartyMemberPartialState(
        WorldPacket packet, WowGuid128 affectedGuid, GroupUpdateFlagTBC updateFlags)
    {
        PartyMemberPartialState state = new PartyMemberPartialState();
        const string Op = nameof(Opcode.SMSG_PARTY_MEMBER_PARTIAL_STATE);
        state.AffectedGUID = affectedGuid;

        bool wotlk = LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056);
        int hpSize = wotlk ? 4 : 2;
        int auraEntrySize = wotlk ? 5 : 3; // uint32 spellid + uint8 flags  vs  uint16 spellid + uint8 flags

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.Status))
        {
            if (!packet.CanRead(2)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.Status), 2, state);
            state.StatusFlags = packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.CurrentHealth))
        {
            if (!packet.CanRead(hpSize)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.CurrentHealth), hpSize, state);
            state.CurrentHealth = wotlk ? packet.ReadUInt32() : packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.MaxHealth))
        {
            if (!packet.CanRead(hpSize)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.MaxHealth), hpSize, state);
            state.MaxHealth = wotlk ? packet.ReadUInt32() : packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.PowerType))
        {
            if (!packet.CanRead(1)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.PowerType), 1, state);
            state.PowerType = packet.ReadUInt8();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.CurrentPower))
        {
            if (!packet.CanRead(2)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.CurrentPower), 2, state);
            state.CurrentPower = packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.MaxPower))
        {
            if (!packet.CanRead(2)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.MaxPower), 2, state);
            state.MaxPower = packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.Level))
        {
            if (!packet.CanRead(2)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.Level), 2, state);
            state.Level = packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.Zone))
        {
            if (!packet.CanRead(2)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.Zone), 2, state);
            state.ZoneID = packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.Position))
        {
            if (!packet.CanRead(4)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.Position), 4, state);
            PartyMemberPartialState.Vector3_UInt16 position = default;
            position.X = packet.ReadInt16();
            position.Y = packet.ReadInt16();
            state.Position = position;
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.Auras))
        {
            if (!packet.CanRead(8)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.Auras), 8, state);
            state.Auras ??= new List<PartyMemberAuraStates>();
            if (!TryReadPartyMemberAuras(packet, state.Auras, auraEntrySize, Op, nameof(GroupUpdateFlagTBC.Auras))) return state;
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetGuid))
        {
            if (!packet.CanRead(8)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.PetGuid), 8, state);
            state.Pet ??= new PartyMemberPetStats();
            state.Pet.NewPetGuid = packet.ReadGuid().To128(GetSession().GameState);
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetName))
        {
            // CString is variable-size; bail if at least the terminator byte isn't there.
            if (!packet.CanRead(1)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.PetName), 1, state);
            state.Pet ??= new PartyMemberPetStats();
            state.Pet.NewPetName = packet.ReadCString();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetModelId))
        {
            if (!packet.CanRead(2)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.PetModelId), 2, state);
            state.Pet ??= new PartyMemberPetStats();
            state.Pet.DisplayID = packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetCurrentHealth))
        {
            if (!packet.CanRead(hpSize)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.PetCurrentHealth), hpSize, state);
            state.Pet ??= new PartyMemberPetStats();
            state.Pet.Health = wotlk ? packet.ReadUInt32() : packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetMaxHealth))
        {
            if (!packet.CanRead(hpSize)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.PetMaxHealth), hpSize, state);
            state.Pet ??= new PartyMemberPetStats();
            state.Pet.MaxHealth = wotlk ? packet.ReadUInt32() : packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetPowerType))
        {
            if (!packet.CanRead(1)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.PetPowerType), 1, state);
            packet.ReadUInt8();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetCurrentPower))
        {
            if (!packet.CanRead(2)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.PetCurrentPower), 2, state);
            packet.ReadInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetMaxPower))
        {
            if (!packet.CanRead(2)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.PetMaxPower), 2, state);
            packet.ReadInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetAuras))
        {
            if (!packet.CanRead(8)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.PetAuras), 8, state);
            state.Pet ??= new PartyMemberPetStats();
            state.Pet.Auras ??= new List<PartyMemberAuraStates>();
            if (!TryReadPartyMemberAuras(packet, state.Pet.Auras, auraEntrySize, Op, nameof(GroupUpdateFlagTBC.PetAuras))) return state;
        }

        if (wotlk && updateFlags.HasFlag(GroupUpdateFlagTBC.VehicleSeat))
        {
            if (!packet.CanRead(4)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.VehicleSeat), 4, state);
            state.VehicleSeatRecID = packet.ReadUInt32();
        }

        return state;
    }

    // Wrapper that returns the partially-populated state after warning. Lets callers do
    // `return WarnTruncatedReturn(...)` in one line instead of WarnTruncated + return state.
    private static T WarnTruncatedReturn<T>(string opcodeName, WorldPacket packet, string field, int needed, T state)
    {
        WarnTruncated(opcodeName, packet, field, needed);
        return state;
    }

    // Consumes one player/pet aura section as written by 3.0+ legacy servers:
    //   uint64 visible-aura mask, then for each set bit:
    //     uint16 (pre-3.0) / uint32 (WotLK+) spell id
    //     uint8  flags / type
    // Returns false (with warn) if the per-entry payload runs short of what the mask claims.
    // Older code iterated a hard-coded GetAuraSlotsCount which on WotLK underreads bits 56-63.
    private static bool TryReadPartyMemberAuras(WorldPacket packet, List<PartyMemberAuraStates> output, int entrySize, string opcodeName, string field)
    {
        ulong auraMask = packet.ReadUInt64();
        bool wotlk = entrySize == 5;
        while (auraMask != 0)
        {
            // Pop the lowest set bit so the loop runs exactly once per aura entry written.
            auraMask &= auraMask - 1;

            if (!packet.CanRead(entrySize))
            {
                WarnTruncated(opcodeName, packet, field + "Entry", entrySize);
                return false;
            }

            PartyMemberAuraStates aura = new PartyMemberAuraStates();
            aura.SpellId = AuraSpellRemap.ToModern(wotlk ? packet.ReadUInt32() : packet.ReadUInt16());
            packet.ReadUInt8(); // aura flags / charge byte (unused by modern packet)
            if (aura.SpellId != 0)
            {
                aura.ActiveFlags = 1;
                aura.AuraFlags = (ushort)AuraFlagsModern.Positive;
            }
            output.Add(aura);
        }
        return true;
    }

    [HandlesSmsg(Opcode.SMSG_PARTY_MEMBER_FULL_STATE, RemovedIn = ClientVersionBuild.V2_0_1_6180)]
    internal void HandlePartyMemberStatsFull(WorldPacket packet)
    {
        if (GetSession().GameState.CurrentMapId == (uint)BattlegroundMapID.WarsongGulch &&
           (GetSession().GameState.HasWsgAllyFlagCarrier || GetSession().GameState.HasWsgHordeFlagCarrier))
        {
            if (_requestBgPlayerPosCounter++ > 10) // don't spam every time somebody moves
            {
                WorldPacket packet2 = new WorldPacket(BattlegroundPlayerPositionsRequest);
                SendPacket(packet2);
                _requestBgPlayerPosCounter = 0;
            }
        }

        PartyMemberFullState state = new PartyMemberFullState();
        if (GetSession().GameState.IsInBattleground())
        {
            state.PartyType[0] = 0;
            state.PartyType[1] = 2;
        }
        else
        {
            state.PartyType[0] = 1;
            state.PartyType[1] = 0;
        }
        
        state.MemberGuid = packet.ReadPackedGuid().To128(GetSession().GameState);
        var updateFlags = (GroupUpdateFlagVanilla)packet.ReadUInt32();

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.Status))
            state.StatusFlags = (GroupMemberOnlineStatus)packet.ReadUInt8();

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.CurrentHealth))
            state.CurrentHealth = packet.ReadUInt16();

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.MaxHealth))
            state.MaxHealth = packet.ReadUInt16();

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PowerType))
            state.PowerType = packet.ReadUInt8();

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.CurrentPower))
            state.CurrentPower = packet.ReadUInt16();

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.MaxPower))
            state.MaxPower = packet.ReadUInt16();

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.Level))
            state.Level = packet.ReadUInt16();

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.Zone))
            state.ZoneID = packet.ReadUInt16();

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.Position))
        {
            state.PositionX = packet.ReadInt16();
            state.PositionY = packet.ReadInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.Auras))
        {
            if (state.Auras == null)
                state.Auras = new List<PartyMemberAuraStates>();

            var auraMask = packet.ReadUInt32(); // Positive Aura Mask

            byte maxAura = 32;

            for (byte i = 0; i < maxAura; ++i)
            {
                if ((auraMask & (1ul << i)) == 0)
                    continue;

                PartyMemberAuraStates aura = new PartyMemberAuraStates();
                aura.SpellId = packet.ReadUInt16();
                if (aura.SpellId != 0)
                {
                    aura.ActiveFlags = 1;
                    aura.AuraFlags = (ushort)AuraFlagsModern.Positive;
                }
                state.Auras.Add(aura);
            }
        }

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.AurasNegative))
        {
            if (state.Auras == null)
                state.Auras = new List<PartyMemberAuraStates>();

            var auraMask = packet.ReadUInt16(); // Negative Aura Mask

            byte maxAura = 48;

            for (byte i = 0; i < maxAura; ++i)
            {
                if ((auraMask & (1ul << i)) == 0)
                    continue;

                PartyMemberAuraStates aura = new PartyMemberAuraStates();
                aura.SpellId = packet.ReadUInt16();
                if (aura.SpellId != 0)
                {
                    aura.ActiveFlags = 1;
                    aura.AuraFlags = (ushort)AuraFlagsModern.Negative;
                }
                state.Auras.Add(aura);
            }
        }

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetGuid))
        {
            if (state.Pet == null)
                state.Pet = new PartyMemberPetStats();
            state.Pet.NewPetGuid = packet.ReadGuid().To128(GetSession().GameState);
        }


        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetName))
        {
            if (state.Pet == null)
                state.Pet = new PartyMemberPetStats();
            state.Pet.NewPetName = packet.ReadCString();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetModelId))
        {
            if (state.Pet == null)
                state.Pet = new PartyMemberPetStats();
            state.Pet.DisplayID = packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetCurrentHealth))
        {
            if (state.Pet == null)
                state.Pet = new PartyMemberPetStats();
            state.Pet.Health = packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetMaxHealth))
        {
            if (state.Pet == null)
                state.Pet = new PartyMemberPetStats();
            state.Pet.MaxHealth = packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetPowerType))
            packet.ReadUInt8(); // Pet Power Type

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetCurrentPower))
            packet.ReadInt16(); // Pet Current Power

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetMaxPower))
            packet.ReadInt16(); // Pet Max Power

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetAuras))
        {
            if (state.Pet == null)
                state.Pet = new PartyMemberPetStats();
            if (state.Pet.Auras == null)
                state.Pet.Auras = new List<PartyMemberAuraStates>();

            var auraMask = packet.ReadUInt32(); // Pet Positive Aura Mask

            byte maxAura = 32;

            for (byte i = 0; i < maxAura; ++i)
            {
                if ((auraMask & (1ul << i)) == 0)
                    continue;

                PartyMemberAuraStates aura = new PartyMemberAuraStates();
                aura.SpellId = packet.ReadUInt16();
                if (aura.SpellId != 0)
                {
                    aura.ActiveFlags = 1;
                    aura.AuraFlags = (ushort)AuraFlagsModern.Positive;
                }
                state.Pet.Auras.Add(aura);
            }
        }

        if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetAurasNegative))
        {
            if (state.Pet == null)
                state.Pet = new PartyMemberPetStats();
            if (state.Pet.Auras == null)
                state.Pet.Auras = new List<PartyMemberAuraStates>();

            var auraMask = packet.ReadUInt16(); // Pet Negative Aura Mask

            byte maxAura = 48;

            for (byte i = 0; i < maxAura; ++i)
            {
                if ((auraMask & (1ul << i)) == 0)
                    continue;

                PartyMemberAuraStates aura = new PartyMemberAuraStates();
                aura.SpellId = packet.ReadUInt16();
                if (aura.SpellId != 0)
                {
                    aura.ActiveFlags = 1;
                    aura.AuraFlags = (ushort)AuraFlagsModern.Negative;
                }
                state.Pet.Auras.Add(aura);
            }
        }

        SendPacketToClient(state);
    }

    [HandlesSmsg(Opcode.SMSG_PARTY_MEMBER_FULL_STATE, AddedIn = ClientVersionBuild.V2_0_1_6180)]
    internal void HandlePartyMemberStatsFullTBC(WorldPacket packet)
    {
        if (GetSession().GameState.CurrentMapId == (uint)BattlegroundMapID.WarsongGulch &&
           (GetSession().GameState.HasWsgAllyFlagCarrier || GetSession().GameState.HasWsgHordeFlagCarrier))
        {
            if (_requestBgPlayerPosCounter++ > 10) // don't spam every time somebody moves
            {
                WorldPacket packet2 = new WorldPacket(BattlegroundPlayerPositionsRequest);
                SendPacket(packet2);
                _requestBgPlayerPosCounter = 0;
            }
        }

        if (IsCataLegacy)
        {
            HandlePartyMemberFullStateCata(packet);
            return;
        }

        PartyMemberFullState state = ParsePartyMemberFullStateTbc(packet);
        SendPacketToClient(state);
    }

    private PartyMemberFullState ParsePartyMemberFullStateTbc(WorldPacket packet)
    {
        PartyMemberFullState state = new PartyMemberFullState();
        const string Op = nameof(Opcode.SMSG_PARTY_MEMBER_PARTIAL_STATE);

        if (GetSession().GameState.IsInBattleground())
        {
            state.PartyType[0] = 0;
            state.PartyType[1] = 2;
        }
        else
        {
            state.PartyType[0] = 1;
            state.PartyType[1] = 0;
        }

        bool wotlk = LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056);
        int hpSize = wotlk ? 4 : 2;
        int auraEntrySize = wotlk ? 5 : 3;

        // 3.0+ legacy server prefixes the packet with a "for enemy / group type" byte.
        if (wotlk)
        {
            if (!packet.CanRead(1)) return WarnTruncatedReturn(Op, packet, nameof(PartyMemberFullState.ForEnemy), 1, state);
            packet.ReadUInt8(); // ForEnemy flag (not forwarded)
        }

        state.MemberGuid = packet.ReadPackedGuid().To128(GetSession().GameState);

        GroupUpdateFlagTBC updateFlags;
        if (!packet.CanRead(4)) return WarnTruncatedReturn(Op, packet, nameof(updateFlags), 4, state);
        updateFlags = (GroupUpdateFlagTBC)packet.ReadUInt32();

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.Status))
        {
            if (!packet.CanRead(2)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.Status), 2, state);
            state.StatusFlags = (GroupMemberOnlineStatus)packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.CurrentHealth))
        {
            if (!packet.CanRead(hpSize)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.CurrentHealth), hpSize, state);
            state.CurrentHealth = wotlk ? (int)packet.ReadUInt32() : packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.MaxHealth))
        {
            if (!packet.CanRead(hpSize)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.MaxHealth), hpSize, state);
            state.MaxHealth = wotlk ? (int)packet.ReadUInt32() : packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.PowerType))
        {
            if (!packet.CanRead(1)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.PowerType), 1, state);
            state.PowerType = packet.ReadUInt8();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.CurrentPower))
        {
            if (!packet.CanRead(2)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.CurrentPower), 2, state);
            state.CurrentPower = packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.MaxPower))
        {
            if (!packet.CanRead(2)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.MaxPower), 2, state);
            state.MaxPower = packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.Level))
        {
            if (!packet.CanRead(2)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.Level), 2, state);
            state.Level = packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.Zone))
        {
            if (!packet.CanRead(2)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.Zone), 2, state);
            state.ZoneID = packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.Position))
        {
            if (!packet.CanRead(4)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.Position), 4, state);
            state.PositionX = packet.ReadInt16();
            state.PositionY = packet.ReadInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.Auras))
        {
            if (!packet.CanRead(8)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.Auras), 8, state);
            state.Auras ??= new List<PartyMemberAuraStates>();
            if (!TryReadPartyMemberAuras(packet, state.Auras, auraEntrySize, Op, nameof(GroupUpdateFlagTBC.Auras))) return state;
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetGuid))
        {
            if (!packet.CanRead(8)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.PetGuid), 8, state);
            state.Pet ??= new PartyMemberPetStats();
            state.Pet.NewPetGuid = packet.ReadGuid().To128(GetSession().GameState);
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetName))
        {
            if (!packet.CanRead(1)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.PetName), 1, state);
            state.Pet ??= new PartyMemberPetStats();
            state.Pet.NewPetName = packet.ReadCString();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetModelId))
        {
            if (!packet.CanRead(2)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.PetModelId), 2, state);
            state.Pet ??= new PartyMemberPetStats();
            state.Pet.DisplayID = packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetCurrentHealth))
        {
            if (!packet.CanRead(hpSize)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.PetCurrentHealth), hpSize, state);
            state.Pet ??= new PartyMemberPetStats();
            state.Pet.Health = wotlk ? packet.ReadUInt32() : packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetMaxHealth))
        {
            if (!packet.CanRead(hpSize)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.PetMaxHealth), hpSize, state);
            state.Pet ??= new PartyMemberPetStats();
            state.Pet.MaxHealth = wotlk ? packet.ReadUInt32() : packet.ReadUInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetPowerType))
        {
            if (!packet.CanRead(1)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.PetPowerType), 1, state);
            packet.ReadUInt8();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetCurrentPower))
        {
            if (!packet.CanRead(2)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.PetCurrentPower), 2, state);
            packet.ReadInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetMaxPower))
        {
            if (!packet.CanRead(2)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.PetMaxPower), 2, state);
            packet.ReadInt16();
        }

        if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetAuras))
        {
            if (!packet.CanRead(8)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.PetAuras), 8, state);
            state.Pet ??= new PartyMemberPetStats();
            state.Pet.Auras ??= new List<PartyMemberAuraStates>();
            if (!TryReadPartyMemberAuras(packet, state.Pet.Auras, auraEntrySize, Op, nameof(GroupUpdateFlagTBC.PetAuras))) return state;
        }

        // WotLK trailing uint32 vehicle/mount seat id (see HandlePartyMemberStatsTbc).
        if (wotlk && updateFlags.HasFlag(GroupUpdateFlagTBC.VehicleSeat))
        {
            if (!packet.CanRead(4)) return WarnTruncatedReturn(Op, packet, nameof(GroupUpdateFlagTBC.VehicleSeat), 4, state);
            state.VehicleSeat = (int)packet.ReadUInt32();
        }

        return state;
    }

    [HandlesSmsg(Opcode.MSG_MINIMAP_PING)]
    internal void HandleMinimapPing(WorldPacket packet)
    {
        MinimapPing ping = new MinimapPing();
        ping.SenderGUID = packet.ReadGuid().To128(GetSession().GameState);
        ping.Position = packet.ReadVector2();
        SendPacketToClient(ping);
    }

    [HandlesSmsg(Opcode.MSG_RANDOM_ROLL)]
    internal void HandleRandomRoll(WorldPacket packet)
    {
        RandomRoll roll = new RandomRoll();
        roll.Min = packet.ReadInt32();
        roll.Max = packet.ReadInt32();
        roll.Result = packet.ReadInt32();
        roll.Roller = packet.ReadGuid().To128(GetSession().GameState);
        roll.RollerWowAccount = GetSession().GetGameAccountGuidForPlayer(roll.Roller);
        SendPacketToClient(roll);
    }

    byte ResolveAssignedRole(WowGuid128 guid, byte serverRole)
    {
        if (GetSession().GameState.GroupAssignedRoles.TryGetValue(guid, out var assigned))
            return assigned;
        return serverRole;
    }

    void PruneAssignedRoles(List<PartyPlayerInfo> members)
    {
        var assigned = GetSession().GameState.GroupAssignedRoles;
        if (assigned.Count == 0)
            return;

        var keep = new HashSet<WowGuid128>();
        foreach (var member in members)
            keep.Add(member.GUID);

        var otherIndex = GetSession().GameState.LastAnnouncedPartyIndex == 0 ? 1 : 0;
        var other = GetSession().GameState.CurrentGroups[otherIndex];
        if (other != null)
        {
            foreach (var member in other.PlayerList)
                keep.Add(member.GUID);
        }

        List<WowGuid128>? drop = null;
        foreach (var guid in assigned.Keys)
        {
            if (keep.Contains(guid))
                continue;
            drop ??= new List<WowGuid128>();
            drop.Add(guid);
        }
        if (drop == null)
            return;
        foreach (var guid in drop)
            assigned.Remove(guid);
    }
    // --- party member stats of a 4.3.4 server

    /// <summary>What <see cref="TryReadPartyMemberStatsCata"/> found, field by field.</summary>
    private struct PartyMemberStatsCata
    {
        public ushort? Status;
        public uint? CurrentHealth;
        public uint? MaxHealth;
        public byte? PowerType;
        public ushort? CurrentPower;
        public ushort? MaxPower;
        public ushort? Level;
        public ushort? ZoneID;
        public PartyMemberPartialState.Vector3_UInt16? Position;
        public List<PartyMemberAuraStates>? Auras;
        public PartyMemberPetStats? Pet;
        public uint? VehicleSeat;
    }

    /// <summary>
    /// The change mask as the throttle knows it. Health, power and position are what a bot-filled
    /// battleground repeats; any other 4.3.4 flag is given a bit the throttle never holds back.
    /// </summary>
    internal static GroupUpdateFlagTBC ThrottleFlagsOf(GroupUpdateFlagCata flags)
    {
        const GroupUpdateFlagCata SameBits = (GroupUpdateFlagCata)0xFF;    // status to zone
        GroupUpdateFlagTBC known = (GroupUpdateFlagTBC)(uint)(flags & SameBits);
        if ((flags & GroupUpdateFlagCata.Position) != 0)
            known |= GroupUpdateFlagTBC.Position;
        if ((flags & ~(SameBits | GroupUpdateFlagCata.Position)) != 0)
            known |= GroupUpdateFlagTBC.Auras;
        return known;
    }

    internal void HandlePartyMemberPartialStateCata(WorldPacket packet, WowGuid128 affectedGuid)
    {
        GroupUpdateFlagCata flags = (GroupUpdateFlagCata)packet.ReadUInt32();
        if (ShouldThrottlePartyMemberState(affectedGuid, ThrottleFlagsOf(flags)))
            return;

        PartyMemberStatsCata stats = default;
        TryReadPartyMemberStatsCata(packet, flags, ref stats);

        PartyMemberPartialState state = new PartyMemberPartialState();
        state.AffectedGUID = affectedGuid;
        state.StatusFlags = stats.Status;
        state.CurrentHealth = stats.CurrentHealth;
        state.MaxHealth = stats.MaxHealth;
        state.PowerType = stats.PowerType;
        state.CurrentPower = stats.CurrentPower;
        state.MaxPower = stats.MaxPower;
        state.Level = stats.Level;
        state.ZoneID = stats.ZoneID;
        state.Position = stats.Position;
        state.Auras = stats.Auras;
        if (stats.Pet != null)
            state.Pet = stats.Pet;
        state.VehicleSeatRecID = stats.VehicleSeat;
        SendPacketToClient(state);
    }

    internal void HandlePartyMemberFullStateCata(WorldPacket packet)
    {
        const string Op = nameof(Opcode.SMSG_PARTY_MEMBER_FULL_STATE);
        PartyMemberFullState state = new PartyMemberFullState();
        if (GetSession().GameState.IsInBattleground())
        {
            state.PartyType[0] = 0;
            state.PartyType[1] = 2;
        }
        else
        {
            state.PartyType[0] = 1;
            state.PartyType[1] = 0;
        }

        // one byte ahead of the member, in the full state only
        if (!packet.CanRead(1 + 1 + 4))
        {
            WarnTruncated(Op, packet, nameof(PartyMemberFullState.MemberGuid), 6);
            return;
        }
        packet.ReadUInt8();
        state.MemberGuid = packet.ReadPackedGuid().To128(GetSession().GameState);
        if (!packet.CanRead(4))
        {
            WarnTruncated(Op, packet, "ChangeMask", 4);
            return;
        }
        GroupUpdateFlagCata flags = (GroupUpdateFlagCata)packet.ReadUInt32();

        PartyMemberStatsCata stats = default;
        TryReadPartyMemberStatsCata(packet, flags, ref stats);

        state.StatusFlags = (GroupMemberOnlineStatus)(stats.Status ?? 0);
        state.CurrentHealth = (int)(stats.CurrentHealth ?? 0);
        state.MaxHealth = (int)(stats.MaxHealth ?? 0);
        state.PowerType = stats.PowerType ?? 0;
        state.CurrentPower = stats.CurrentPower ?? 0;
        state.MaxPower = stats.MaxPower ?? 0;
        state.Level = stats.Level ?? 0;
        state.ZoneID = stats.ZoneID ?? 0;
        if (stats.Position is { } position)
        {
            state.PositionX = position.X;
            state.PositionY = position.Y;
            state.PositionZ = position.Z;
        }
        if (stats.Auras != null)
            state.Auras = stats.Auras;
        if (stats.Pet != null)
            state.Pet = stats.Pet;
        state.VehicleSeat = (int)(stats.VehicleSeat ?? 0);
        SendPacketToClient(state);
    }

    /// <summary>
    /// TrinityCore 4.3.4 <c>PartyMemberState::Write</c>, from after the change mask. Returns false
    /// where the packet ends before the mask says it should; what was read by then stands.
    /// </summary>
    private bool TryReadPartyMemberStatsCata(WorldPacket packet, GroupUpdateFlagCata flags, ref PartyMemberStatsCata stats)
    {
        const string Op = nameof(Opcode.SMSG_PARTY_MEMBER_PARTIAL_STATE);

        if ((flags & GroupUpdateFlagCata.Status) != 0)
        {
            if (!packet.CanRead(2)) return WarnTruncated(Op, packet, nameof(GroupUpdateFlagCata.Status), 2);
            stats.Status = packet.ReadUInt16();
        }

        if ((flags & GroupUpdateFlagCata.CurrentHealth) != 0)
        {
            if (!packet.CanRead(4)) return WarnTruncated(Op, packet, nameof(GroupUpdateFlagCata.CurrentHealth), 4);
            stats.CurrentHealth = packet.ReadUInt32();
        }

        if ((flags & GroupUpdateFlagCata.MaxHealth) != 0)
        {
            if (!packet.CanRead(4)) return WarnTruncated(Op, packet, nameof(GroupUpdateFlagCata.MaxHealth), 4);
            stats.MaxHealth = packet.ReadUInt32();
        }

        if ((flags & GroupUpdateFlagCata.PowerType) != 0)
        {
            if (!packet.CanRead(1)) return WarnTruncated(Op, packet, nameof(GroupUpdateFlagCata.PowerType), 1);
            stats.PowerType = packet.ReadUInt8();
        }

        if ((flags & GroupUpdateFlagCata.CurrentPower) != 0)
        {
            if (!packet.CanRead(2)) return WarnTruncated(Op, packet, nameof(GroupUpdateFlagCata.CurrentPower), 2);
            stats.CurrentPower = packet.ReadUInt16();
        }

        if ((flags & GroupUpdateFlagCata.MaxPower) != 0)
        {
            if (!packet.CanRead(2)) return WarnTruncated(Op, packet, nameof(GroupUpdateFlagCata.MaxPower), 2);
            stats.MaxPower = packet.ReadUInt16();
        }

        if ((flags & GroupUpdateFlagCata.Level) != 0)
        {
            if (!packet.CanRead(2)) return WarnTruncated(Op, packet, nameof(GroupUpdateFlagCata.Level), 2);
            stats.Level = packet.ReadUInt16();
        }

        if ((flags & GroupUpdateFlagCata.Zone) != 0)
        {
            if (!packet.CanRead(2)) return WarnTruncated(Op, packet, nameof(GroupUpdateFlagCata.Zone), 2);
            stats.ZoneID = packet.ReadUInt16();
        }

        if ((flags & GroupUpdateFlagCata.WmoGroupId) != 0)
        {
            // Not passed on: no older server has it, so the client has never been sent one here.
            if (!packet.CanRead(2)) return WarnTruncated(Op, packet, nameof(GroupUpdateFlagCata.WmoGroupId), 2);
            packet.ReadInt16();
        }

        if ((flags & GroupUpdateFlagCata.Position) != 0)
        {
            if (!packet.CanRead(6)) return WarnTruncated(Op, packet, nameof(GroupUpdateFlagCata.Position), 6);
            PartyMemberPartialState.Vector3_UInt16 position = default;
            position.X = packet.ReadInt16();
            position.Y = packet.ReadInt16();
            position.Z = packet.ReadInt16();
            stats.Position = position;
        }

        if ((flags & GroupUpdateFlagCata.Auras) != 0)
        {
            if (!TryReadPartyMemberAurasCata(packet, ref stats.Auras, Op, nameof(GroupUpdateFlagCata.Auras))) return false;
        }

        if ((flags & GroupUpdateFlagCata.PetGuid) != 0)
        {
            if (!packet.CanRead(8)) return WarnTruncated(Op, packet, nameof(GroupUpdateFlagCata.PetGuid), 8);
            stats.Pet ??= new PartyMemberPetStats();
            stats.Pet.NewPetGuid = packet.ReadGuid().To128(GetSession().GameState);
        }

        if ((flags & GroupUpdateFlagCata.PetName) != 0)
        {
            if (!packet.CanRead(1)) return WarnTruncated(Op, packet, nameof(GroupUpdateFlagCata.PetName), 1);
            stats.Pet ??= new PartyMemberPetStats();
            stats.Pet.NewPetName = packet.ReadCString();
        }

        if ((flags & GroupUpdateFlagCata.PetModelId) != 0)
        {
            if (!packet.CanRead(2)) return WarnTruncated(Op, packet, nameof(GroupUpdateFlagCata.PetModelId), 2);
            stats.Pet ??= new PartyMemberPetStats();
            stats.Pet.DisplayID = packet.ReadUInt16();
        }

        if ((flags & GroupUpdateFlagCata.PetCurrentHealth) != 0)
        {
            if (!packet.CanRead(4)) return WarnTruncated(Op, packet, nameof(GroupUpdateFlagCata.PetCurrentHealth), 4);
            stats.Pet ??= new PartyMemberPetStats();
            stats.Pet.Health = packet.ReadUInt32();
        }

        if ((flags & GroupUpdateFlagCata.PetMaxHealth) != 0)
        {
            if (!packet.CanRead(4)) return WarnTruncated(Op, packet, nameof(GroupUpdateFlagCata.PetMaxHealth), 4);
            stats.Pet ??= new PartyMemberPetStats();
            stats.Pet.MaxHealth = packet.ReadUInt32();
        }

        if ((flags & GroupUpdateFlagCata.PetPowerType) != 0)
        {
            if (!packet.CanRead(1)) return WarnTruncated(Op, packet, nameof(GroupUpdateFlagCata.PetPowerType), 1);
            packet.ReadUInt8();
        }

        if ((flags & GroupUpdateFlagCata.PetCurrentPower) != 0)
        {
            if (!packet.CanRead(2)) return WarnTruncated(Op, packet, nameof(GroupUpdateFlagCata.PetCurrentPower), 2);
            packet.ReadUInt16();
        }

        if ((flags & GroupUpdateFlagCata.PetMaxPower) != 0)
        {
            if (!packet.CanRead(2)) return WarnTruncated(Op, packet, nameof(GroupUpdateFlagCata.PetMaxPower), 2);
            packet.ReadUInt16();
        }

        if ((flags & GroupUpdateFlagCata.PetAuras) != 0)
        {
            // Sent for a member without a pet too, with an empty mask: no pet is made of that.
            List<PartyMemberAuraStates>? petAuras = null;
            if (!TryReadPartyMemberAurasCata(packet, ref petAuras, Op, nameof(GroupUpdateFlagCata.PetAuras))) return false;
            if (petAuras != null)
            {
                stats.Pet ??= new PartyMemberPetStats();
                stats.Pet.Auras = petAuras;
            }
        }

        if ((flags & GroupUpdateFlagCata.VehicleSeat) != 0)
        {
            if (!packet.CanRead(4)) return WarnTruncated(Op, packet, nameof(GroupUpdateFlagCata.VehicleSeat), 4);
            stats.VehicleSeat = packet.ReadUInt32();
        }

        if ((flags & GroupUpdateFlagCata.Phase) != 0)
        {
            // Read past, not passed on: the client is told of no phases for a member of an older
            // server either, and a wrong one greys the member out as being somewhere else.
            if (!packet.CanRead(8)) return WarnTruncated(Op, packet, nameof(GroupUpdateFlagCata.Phase), 8);
            packet.ReadUInt32();
            uint phases = packet.ReadUInt32();
            if (phases > packet.Remaining() / 2) return WarnTruncated(Op, packet, nameof(GroupUpdateFlagCata.Phase) + "List", (int)Math.Min(phases * 2, int.MaxValue));
            for (uint i = 0; i < phases; i++)
                packet.ReadUInt16();
        }

        return true;
    }

    /// <summary>
    /// One aura block of a 4.3.4 member: a byte, the mask of the slots that follow, a count the
    /// server sets to its 64 slots whatever the mask says, then for each set bit the spell, 16 bits
    /// of flags and, for a scalable aura, the values of its three effects. The client is given
    /// the values of the effects the aura has.
    /// </summary>
    private static bool TryReadPartyMemberAurasCata(WorldPacket packet, ref List<PartyMemberAuraStates>? output, string opcodeName, string field)
    {
        if (!packet.CanRead(1 + 8 + 4)) return WarnTruncated(opcodeName, packet, field, 13);
        packet.ReadUInt8();
        ulong auraMask = packet.ReadUInt64();
        packet.ReadUInt32();

        while (auraMask != 0)
        {
            auraMask &= auraMask - 1;
            if (!packet.CanRead(6)) return WarnTruncated(opcodeName, packet, field + "Entry", 6);

            PartyMemberAuraStates aura = new PartyMemberAuraStates();
            aura.SpellId = AuraSpellRemap.ToModern(packet.ReadUInt32());
            AuraFlagsWotLK legacyFlags = (AuraFlagsWotLK)packet.ReadUInt16();

            AuraFlagsModern modernFlags = (legacyFlags & AuraFlagsWotLK.Negative) != 0 ? AuraFlagsModern.Negative : AuraFlagsModern.Positive;
            if ((legacyFlags & AuraFlagsWotLK.Duration) != 0) modernFlags |= AuraFlagsModern.Duration;
            if ((legacyFlags & AuraFlagsWotLK.NoCaster) != 0) modernFlags |= AuraFlagsModern.NoCaster;

            if ((legacyFlags & AuraFlagsWotLK.Scalable) != 0)
            {
                if (!packet.CanRead(12)) return WarnTruncated(opcodeName, packet, field + "Points", 12);
                for (int effect = 0; effect < 3; effect++)
                {
                    int points = packet.ReadInt32();
                    if (((uint)legacyFlags & (1u << effect)) != 0)
                        aura.Points.Add(points);
                }
                if (aura.Points.Count > 0)
                    modernFlags |= AuraFlagsModern.Scalable;
            }

            if (aura.SpellId != 0)
            {
                aura.ActiveFlags = (uint)(legacyFlags & (AuraFlagsWotLK.EffectIndex0 | AuraFlagsWotLK.EffectIndex1 | AuraFlagsWotLK.EffectIndex2));
                aura.AuraFlags = (ushort)modernFlags;
            }
            (output ??= new List<PartyMemberAuraStates>()).Add(aura);
        }
        return true;
    }
}
