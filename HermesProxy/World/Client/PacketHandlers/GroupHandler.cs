using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [PacketHandler(Opcode.SMSG_PARTY_COMMAND_RESULT)]
    void HandlePartyCommandResult(WorldPacket packet)
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
    }

    [PacketHandler(Opcode.SMSG_GROUP_DECLINE)]
    void HandleGroupDecline(WorldPacket packet)
    {
        GroupDecline party = new GroupDecline();
        party.Name = packet.ReadCString();
        SendPacketToClient(party);
    }

    [PacketHandler(Opcode.SMSG_PARTY_INVITE)]
    void HandleGroupInvite(WorldPacket packet)
    {
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

    [PacketHandler(Opcode.SMSG_GROUP_LIST, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
    void HandleGroupListVanilla(WorldPacket packet)
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
            GetSession().GameState.CurrentGroups[party.PartyIndex] = party;
        }
        else
        {
            party.PartyFlags |= GroupFlags.Destroyed;
            if (party.PartyIndex == 0)
                party.PartyGUID = WowGuid128.Empty;
            party.LeaderGUID = WowGuid128.Empty;
            party.MyIndex = -1;
            GetSession().GameState.CurrentGroups[party.PartyIndex] = null;

            if (!GetSession().GameState.WeWantToLeaveGroup)
                SendPacketToClient(new GroupUninvite()); // Send kick message
        }

        SendPacketToClient(party);
    }

    [PacketHandler(Opcode.SMSG_GROUP_LIST, ClientVersionBuild.V2_0_1_6180)]
    void HandleGroupListTBC(WorldPacket packet)
    {
        GetSession().GameState.MasterLootCandidates = null;
        GetSession().GameState.LastMasterLootSentTarget = default;
        PartyUpdate party = new PartyUpdate();
        party.SequenceNum = GetSession().GameState.GroupUpdateCounter++;
        // Wire format on 2.x/3.3.x is a single byte of flags, not two bools.
        // TC/CMaNGOS agree: BG/FakeRaid=0x01, Raid=0x02, Lfg=0x08.
        byte groupType = packet.ReadUInt8();
        bool isBattleground = (groupType & 0x01) != 0;
        bool isRaid = (groupType & 0x02) != 0;
        bool isLfg = (groupType & 0x08) != 0;
        byte ownSubGroup = packet.ReadUInt8();
        byte ownGroupFlags = packet.ReadUInt8();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            packet.ReadUInt8(); // own LFG roles
        if (isLfg)
        {
            packet.ReadUInt8();  // LFG dungeon status
            packet.ReadUInt32(); // LFG dungeon ID
        }
        party.PartyIndex = (byte)(isBattleground ? 1 : 0);
        party.PartyGUID = packet.ReadGuid().To128(GetSession().GameState);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            packet.ReadUInt32(); // group counter
        if (party.PartyIndex != 0)
            party.PartyFlags |= GroupFlags.FakeRaid;

        var uniqueMembers = new HashSet<WowGuid128>();
        uint membersCount = packet.ReadUInt32();
        if (membersCount > 0)
        {
            if (isRaid)
                party.PartyFlags |= GroupFlags.Raid;

            if (party.PartyIndex != 0)
                party.PartyType = GroupType.PvP;
            else
                party.PartyType = GroupType.Normal;

            PartyPlayerInfo player = new PartyPlayerInfo();
            player.GUID = GetSession().GameState.CurrentPlayerGuid;
            player.Name = GetSession().GameState.GetPlayerName(player.GUID);
            player.Subgroup = ownSubGroup;
            player.Flags = (GroupMemberFlags)ownGroupFlags;
            player.Status = GroupMemberOnlineStatus.Online;
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
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
                    packet.ReadUInt8(); // member LFG roles
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

            party.DifficultySettings = new PartyDifficultySettings();
            int difficultyId = packet.ReadUInt8();
            party.DifficultySettings.DungeonDifficultyID = ((DifficultyLegacy)difficultyId).CastEnum<DifficultyModern>();

            if (ModernVersion.ExpansionVersion > 1)
                party.DifficultySettings.RaidDifficultyID = DifficultyModern.Raid25N;
            else
                party.DifficultySettings.RaidDifficultyID = DifficultyModern.Raid40;

            GetSession().GameState.WeWantToLeaveGroup = false;
            GetSession().GameState.CurrentGroups[party.PartyIndex] = party;
        }
        else
        {
            party.PartyFlags |= GroupFlags.Destroyed;
            if (party.PartyIndex  == 0)
                party.PartyGUID = WowGuid128.Empty;
            party.LeaderGUID = WowGuid128.Empty;
            party.MyIndex = -1;
            GetSession().GameState.CurrentGroups[party.PartyIndex] = null;

            if (!GetSession().GameState.WeWantToLeaveGroup)
                SendPacketToClient(new GroupUninvite()); // Send kick message
        }

        SendPacketToClient(party);
    }

    [PacketHandler(Opcode.SMSG_GROUP_UNINVITE)]
    void HandleGroupUninvite(WorldPacket packet)
    {
        GroupUninvite party = new GroupUninvite();
        SendPacketToClient(party);
    }

    [PacketHandler(Opcode.SMSG_GROUP_NEW_LEADER)]
    void HandleGroupNewLeader(WorldPacket packet)
    {
        GroupNewLeader party = new GroupNewLeader();
        party.Name = packet.ReadCString();
        party.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();
        SendPacketToClient(party);
    }

    [PacketHandler(Opcode.MSG_RAID_READY_CHECK, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
    void HandleRaidReadyCheckVanilla(WorldPacket packet)
    {
        if (!packet.CanRead())
        {
            ReadyCheckStarted ready = new ReadyCheckStarted();
            ready.InitiatorGUID = GetSession().GameState.GetCurrentGroupLeader();
            ready.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();
            ready.PartyGUID = GetSession().GameState.GetCurrentGroupGuid();
            SendPacketToClient(ready);
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
                ReadyCheckCompleted completed = new ReadyCheckCompleted();
                completed.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();
                completed.PartyGUID = GetSession().GameState.GetCurrentGroupGuid();
                SendPacketToClient(completed);
            }
        }
    }

    [PacketHandler(Opcode.MSG_RAID_READY_CHECK, ClientVersionBuild.V2_0_1_6180)]
    void HandleRaidReadyCheck(WorldPacket packet)
    {
        ReadyCheckStarted ready = new ReadyCheckStarted();
        ready.InitiatorGUID = packet.ReadGuid().To128(GetSession().GameState);
        ready.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();
        ready.PartyGUID = GetSession().GameState.GetCurrentGroupGuid();
        SendPacketToClient(ready);
    }

    [PacketHandler(Opcode.MSG_RAID_READY_CHECK_CONFIRM, ClientVersionBuild.V2_0_1_6180)]
    void HandleRaidReadyCheckConfirm(WorldPacket packet)
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
            ReadyCheckCompleted completed = new ReadyCheckCompleted();
            completed.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();
            completed.PartyGUID = GetSession().GameState.GetCurrentGroupGuid();
            SendPacketToClient(completed);
        }
    }

    [PacketHandler(Opcode.MSG_RAID_READY_CHECK_FINISHED, ClientVersionBuild.V2_0_1_6180)]
    void HandleRaidReadyCheckFinished(WorldPacket packet)
    {
        ReadyCheckCompleted ready = new ReadyCheckCompleted();
        ready.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();
        ready.PartyGUID = GetSession().GameState.GetCurrentGroupGuid();
        SendPacketToClient(ready);
    }

    [PacketHandler(Opcode.MSG_RAID_TARGET_UPDATE)]
    void HandleRaidTargetUpdate(WorldPacket packet)
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

    [PacketHandler(Opcode.SMSG_SUMMON_REQUEST)]
    void HandleSummonRequest(WorldPacket packet)
    {
        SummonRequest summon = new SummonRequest();
        summon.SummonerGUID = packet.ReadGuid().To128(GetSession().GameState);
        summon.SummonerVirtualRealmAddress = GetSession().RealmId.GetAddress();
        summon.AreaID = packet.ReadInt32();
        packet.ReadUInt32(); // time to accept
        SendPacketToClient(summon);
    }

    uint _requestBgPlayerPosCounter = 0;

    [PacketHandler(Opcode.SMSG_PARTY_MEMBER_PARTIAL_STATE, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
    void HandlePartyMemberStats(WorldPacket packet)
    {
        if (GetSession().GameState.CurrentMapId == (uint)BattlegroundMapID.WarsongGulch &&
           (GetSession().GameState.HasWsgAllyFlagCarrier || GetSession().GameState.HasWsgHordeFlagCarrier))
        {
            if (_requestBgPlayerPosCounter++ > 10) // don't spam every time somebody moves
            {
                WorldPacket packet2 = new WorldPacket(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
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
            state.Position = new PartyMemberPartialState.Vector3_UInt16();
            state.Position.X = packet.ReadInt16();
            state.Position.Y = packet.ReadInt16();
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

    [PacketHandler(Opcode.SMSG_PARTY_MEMBER_PARTIAL_STATE, ClientVersionBuild.V2_0_1_6180)]
    void HandlePartyMemberStatsTbc(WorldPacket packet)
    {
        if (GetSession().GameState.CurrentMapId == (uint)BattlegroundMapID.WarsongGulch &&
           (GetSession().GameState.HasWsgAllyFlagCarrier || GetSession().GameState.HasWsgHordeFlagCarrier))
        {
            if (_requestBgPlayerPosCounter++ > 10) // don't spam every time somebody moves
            {
                WorldPacket packet2 = new WorldPacket(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
                SendPacket(packet2);
                _requestBgPlayerPosCounter = 0;
            }
        }

        // NPCBot / Playerbot truncates: it sets `mask = GROUP_UPDATE_FULL` (0x7FFFF) but only
        // writes the leading subset of fields (typically through POSITION). Trusting the mask
        // would over-read the buffer. The parser catches ArgumentOutOfRangeException internally
        // and returns the partial state filled up to the truncation point so the modern client
        // still gets HP / power / position updates instead of nothing.
        PartyMemberPartialState state = ParsePartyMemberPartialState(packet);
        SendPacketToClient(state);
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

    private PartyMemberPartialState ParsePartyMemberPartialState(WorldPacket packet)
    {
        PartyMemberPartialState state = new PartyMemberPartialState();
        const string Op = nameof(Opcode.SMSG_PARTY_MEMBER_PARTIAL_STATE);

        // PackedGuid is variable-size; trust the upstream framing for it (a malformed mask
        // byte here would be a wire-level problem, not the mask-truncation bug we handle below).
        state.AffectedGUID = packet.ReadPackedGuid().To128(GetSession().GameState);

        GroupUpdateFlagTBC updateFlags;
        if (!packet.CanRead(4)) return WarnTruncatedReturn(Op, packet, nameof(updateFlags), 4, state);
        updateFlags = (GroupUpdateFlagTBC)packet.ReadUInt32();

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
            state.Position = new PartyMemberPartialState.Vector3_UInt16();
            state.Position.X = packet.ReadInt16();
            state.Position.Y = packet.ReadInt16();
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
            aura.SpellId = wotlk ? packet.ReadUInt32() : packet.ReadUInt16();
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

    [PacketHandler(Opcode.SMSG_PARTY_MEMBER_FULL_STATE, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
    void HandlePartyMemberStatsFull(WorldPacket packet)
    {
        if (GetSession().GameState.CurrentMapId == (uint)BattlegroundMapID.WarsongGulch &&
           (GetSession().GameState.HasWsgAllyFlagCarrier || GetSession().GameState.HasWsgHordeFlagCarrier))
        {
            if (_requestBgPlayerPosCounter++ > 10) // don't spam every time somebody moves
            {
                WorldPacket packet2 = new WorldPacket(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
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

    [PacketHandler(Opcode.SMSG_PARTY_MEMBER_FULL_STATE, ClientVersionBuild.V2_0_1_6180)]
    void HandlePartyMemberStatsFullTBC(WorldPacket packet)
    {
        if (GetSession().GameState.CurrentMapId == (uint)BattlegroundMapID.WarsongGulch &&
           (GetSession().GameState.HasWsgAllyFlagCarrier || GetSession().GameState.HasWsgHordeFlagCarrier))
        {
            if (_requestBgPlayerPosCounter++ > 10) // don't spam every time somebody moves
            {
                WorldPacket packet2 = new WorldPacket(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
                SendPacket(packet2);
                _requestBgPlayerPosCounter = 0;
            }
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

    [PacketHandler(Opcode.MSG_MINIMAP_PING)]
    void HandleMinimapPing(WorldPacket packet)
    {
        MinimapPing ping = new MinimapPing();
        ping.SenderGUID = packet.ReadGuid().To128(GetSession().GameState);
        ping.Position = packet.ReadVector2();
        SendPacketToClient(ping);
    }

    [PacketHandler(Opcode.MSG_RANDOM_ROLL)]
    void HandleRandomRoll(WorldPacket packet)
    {
        RandomRoll roll = new RandomRoll();
        roll.Min = packet.ReadInt32();
        roll.Max = packet.ReadInt32();
        roll.Result = packet.ReadInt32();
        roll.Roller = packet.ReadGuid().To128(GetSession().GameState);
        roll.RollerWowAccount = GetSession().GetGameAccountGuidForPlayer(roll.Roller);
        SendPacketToClient(roll);
    }
}
