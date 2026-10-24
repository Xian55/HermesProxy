using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;
using static HermesProxy.World.Server.Packets.PVPMatchStatisticsMessage;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [HandlesSmsg(Opcode.SMSG_BATTLEFIELD_LIST, RemovedIn = ClientVersionBuild.V2_0_1_6180)]
    internal void HandleBattlefieldListVanilla(WorldPacket packet)
    {
        BattlefieldList bglist = new BattlefieldList();
        bglist.BattlemasterGuid = packet.ReadGuid().To128(GetSession().GameState);
        GetSession().GameState.CurrentInteractedWithNPC = bglist.BattlemasterGuid;
        bglist.BattlemasterListID = GameData.GetBattlegroundIdFromMapId(packet.ReadUInt32());
        packet.ReadUInt8(); // bracket id
        var instancesCount = packet.ReadUInt32();
        for (var i = 0; i < instancesCount; i++)
        {
            int instanceId = packet.ReadInt32();
            bglist.BattlefieldInstances.Add(instanceId);
        }
        SendPacketToClient(bglist);
    }

    [HandlesSmsg(Opcode.SMSG_BATTLEFIELD_LIST, AddedIn = ClientVersionBuild.V2_0_1_6180, RemovedIn = ClientVersionBuild.V3_0_2_9056)]
    internal void HandleBattlefieldListTBC(WorldPacket packet)
    {
        BattlefieldList bglist = new BattlefieldList();
        bglist.BattlemasterGuid = packet.ReadGuid().To128(GetSession().GameState);
        GetSession().GameState.CurrentInteractedWithNPC = bglist.BattlemasterGuid;
        bglist.BattlemasterListID = packet.ReadUInt32();
        packet.ReadUInt8(); // bracket id
        var instancesCount = packet.ReadUInt32();
        for (var i = 0; i < instancesCount; i++)
        {
            int instanceId = packet.ReadInt32();
            bglist.BattlefieldInstances.Add(instanceId);
        }
        SendPacketToClient(bglist);
    }

    [HandlesSmsg(Opcode.SMSG_BATTLEFIELD_LIST, AddedIn = ClientVersionBuild.V3_0_2_9056)]
    internal void HandleBattlefieldListWotLK(WorldPacket packet)
    {
        if (IsCataLegacy)
        {
            HandleBattlefieldListCata(packet);
            return;
        }

        BattlefieldList bglist = new BattlefieldList();
        bglist.BattlemasterGuid = packet.ReadGuid().To128(GetSession().GameState);
        GetSession().GameState.CurrentInteractedWithNPC = bglist.BattlemasterGuid;
        bglist.PvpAnywhere = packet.ReadBool(); // from UI
        bglist.BattlemasterListID = packet.ReadUInt32();
        bglist.MinLevel = packet.ReadUInt8();
        bglist.MaxLevel = packet.ReadUInt8();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685))
        {
            // Every list has the holiday battleground's rewards, the random battleground's list its
            // own after them. The cores give both the same amounts; the later block is kept.
            packet.ReadBool(); // Has Win
            var rewards = (WinHonor: packet.ReadInt32(), WinArena: packet.ReadInt32(), LossHonor: packet.ReadInt32());

            if (packet.ReadBool()) // Is random
            {
                bglist.HasRandomWinToday = packet.ReadBool();
                rewards = (packet.ReadInt32(), packet.ReadInt32(), packet.ReadInt32());
            }

            // The window asked for its rewards before this list came; new amounts go to it first,
            // so that the list's own refresh already shows them.
            if (GetSession().GameState.WotLKBattlegroundRewards != rewards)
            {
                GetSession().GameState.WotLKBattlegroundRewards = rewards;
                SendPacketToClient(BuildPvpRewardsWotLK(rewards));
            }
        }
        var instancesCount = packet.ReadUInt32();
        for (var i = 0; i < instancesCount; i++)
        {
            int instanceId = packet.ReadInt32();
            bglist.BattlefieldInstances.Add(instanceId);
        }
        if (Log.IsDebugEnabled)
            Log.Print(LogType.Debug, $"[BG] SMSG_BATTLEFIELD_LIST (WotLK): BattlemasterListID={bglist.BattlemasterListID} guid={bglist.BattlemasterGuid} MinLevel={bglist.MinLevel} MaxLevel={bglist.MaxLevel} PvpAnywhere={bglist.PvpAnywhere} HasRandomWinToday={bglist.HasRandomWinToday} instances={bglist.BattlefieldInstances.Count} [{string.Join(",", bglist.BattlefieldInstances)}].");
        SendPacketToClient(bglist);
    }

    [HandlesSmsg(Opcode.SMSG_BATTLEFIELD_STATUS, RemovedIn = ClientVersionBuild.V2_0_1_6180)]
    internal void HandleBattlefieldStatusVanilla(WorldPacket packet)
    {
        BattlefieldStatusHeader hdr = new BattlefieldStatusHeader();
        hdr.Ticket.Id = 1 + packet.ReadUInt32(); // Queue Slot
        hdr.Ticket.RequesterGuid = GetSession().GameState.CurrentPlayerGuid;
        hdr.Ticket.Time = GetSession().GameState.GetBattleFieldQueueTime(hdr.Ticket.Id);
        hdr.Ticket.Type = RideType.Battlegrounds;

        uint mapId = packet.ReadUInt32();
        if (mapId != 0)
        {
            uint battlefieldListId = GameData.GetBattlegroundIdFromMapId(mapId);
            hdr.BattlefieldListIDs.Add(battlefieldListId);
            packet.ReadUInt8(); // bracket id
            hdr.InstanceID = packet.ReadUInt32();
            BattleGroundStatus status = (BattleGroundStatus)packet.ReadUInt32();
            switch (status)
            {
                case BattleGroundStatus.WaitQueue:
                {
                    BattlefieldStatusQueued queue = new BattlefieldStatusQueued();
                    queue.Hdr = hdr;
                    queue.AverageWaitTime = packet.ReadUInt32();
                    queue.WaitTime = packet.ReadUInt32();
                    SendPacketToClient(queue);
                    break;
                }
                case BattleGroundStatus.WaitJoin:
                {
                    BattlefieldStatusNeedConfirmation confirm = new BattlefieldStatusNeedConfirmation();
                    confirm.Hdr = hdr;
                    confirm.Mapid = mapId;
                    confirm.Timeout = packet.ReadUInt32();
                    SendPacketToClient(confirm);
                    break;
                }
                case BattleGroundStatus.InProgress:
                {
                    BattlefieldStatusActive active = new BattlefieldStatusActive();
                    active.Hdr = hdr;
                    active.Mapid = mapId;
                    active.ShutdownTimer = packet.ReadUInt32();
                    active.StartTimer = packet.ReadUInt32();
                    if (active.ShutdownTimer == 0)
                    {
                        BattlegroundInit init = new BattlegroundInit();
                        init.Milliseconds = 1154756799;
                        SendPacketToClient(init);
                    }
                    SendPacketToClient(active);
                    break;
                }
                default:
                {
                    Log.Print(LogType.Error, $"Unexpected BG status {status}.");
                    break;
                }
            }
        }
        else
        {
            uint queuedMapId = GetSession().GameState.GetBattleFieldQueueType(hdr.Ticket.Id);
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180) &&
                queuedMapId == GetSession().GameState.CurrentMapId)
            {
                // Clear BG group properly on vanilla servers.
                var bgGroup = GetSession().GameState.CurrentGroups[1];
                if (bgGroup != null)
                {
                    PartyUpdate party = new PartyUpdate();
                    party.SequenceNum = GetSession().GameState.GroupUpdateCounter++;
                    party.PartyFlags = GroupFlags.FakeRaid | GroupFlags.Destroyed;
                    party.PartyIndex = 1;
                    party.PartyGUID = bgGroup.PartyGUID;
                    party.LeaderGUID = WowGuid128.Empty;
                    party.MyIndex = -1;
                    GetSession().GameState.CurrentGroups[1] = null;
                    SendPacketToClient(party);
                }
            }

            BattlefieldStatusFailed failed = new BattlefieldStatusFailed();
            failed.Ticket = hdr.Ticket;
            failed.Reason = 30;
            failed.BattlefieldListId = GameData.GetBattlegroundIdFromMapId(queuedMapId);
            SendPacketToClient(failed);
            GetSession().GameState.RemoveBattleFieldQueue(hdr.Ticket.Id);
        }
        GetSession().GameState.StoreBattleFieldQueueType(hdr.Ticket.Id, mapId);
    }

    [HandlesSmsg(Opcode.SMSG_BATTLEFIELD_STATUS, AddedIn = ClientVersionBuild.V2_0_1_6180)]
    internal void HandleBattlefieldStatusTBC(WorldPacket packet)
    {
        if (IsCataLegacy)
        {
            HandleBattlefieldStatusNoneCata(packet);
            return;
        }

        BattlefieldStatusHeader hdr = new BattlefieldStatusHeader();
        hdr.Ticket.Id = 1 + packet.ReadUInt32(); // Queue Slot
        hdr.Ticket.RequesterGuid = GetSession().GameState.CurrentPlayerGuid;
        hdr.Ticket.Time = GetSession().GameState.GetBattleFieldQueueTime(hdr.Ticket.Id);
        hdr.Ticket.Type = RideType.Battlegrounds;

        hdr.ArenaTeamSize = packet.ReadUInt8();
        byte bracketId = packet.ReadUInt8(); // bracket id, echoed back on PORT/LEAVE
        uint battlefieldListId = packet.ReadUInt32();
        packet.ReadUInt16(); // 0x1F90

        Log.Print(LogType.Debug, $"[BG] SMSG_BATTLEFIELD_STATUS (TBC+): ticketId={hdr.Ticket.Id} battlefieldListId={battlefieldListId} arenaTeamSize={hdr.ArenaTeamSize} (listId=0 means the legacy server reports no/removed BG -> client sees FAILED).");

        if (battlefieldListId != 0)
        {
            hdr.BattlefieldListIDs.Add(battlefieldListId);

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685))
            {
                hdr.RangeMin = packet.ReadUInt8();
                hdr.RangeMax = packet.ReadUInt8();
            }

            hdr.InstanceID = packet.ReadUInt32();
            hdr.IsArena = packet.ReadBool();
            BattleGroundStatus status = (BattleGroundStatus)packet.ReadUInt32();
            switch (status)
            {
                case BattleGroundStatus.WaitQueue:
                {
                    BattlefieldStatusQueued queue = new BattlefieldStatusQueued();
                    queue.Hdr = hdr;
                    queue.AverageWaitTime = packet.ReadUInt32();
                    queue.WaitTime = packet.ReadUInt32();
                    SendPacketToClient(queue);
                    break;
                }
                case BattleGroundStatus.WaitJoin:
                {
                    BattlefieldStatusNeedConfirmation confirm = new BattlefieldStatusNeedConfirmation();
                    confirm.Hdr = hdr;
                    confirm.Mapid = packet.ReadUInt32();
                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_5_12213))
                        packet.ReadUInt64(); // unk
                    confirm.Timeout = packet.ReadUInt32();
                    SendPacketToClient(confirm);
                    break;
                }
                case BattleGroundStatus.InProgress:
                {
                    BattlefieldStatusActive active = new BattlefieldStatusActive();
                    active.Hdr = hdr;
                    active.Mapid = packet.ReadUInt32();
                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_5_12213))
                        packet.ReadUInt64(); // unk
                    active.ShutdownTimer = packet.ReadUInt32();
                    active.StartTimer = packet.ReadUInt32();
                    active.ArenaFaction = packet.ReadUInt8();
                    if (active.ShutdownTimer == 0)
                    {
                        BattlegroundInit init = new BattlegroundInit();
                        init.Milliseconds = 1154756799;
                        SendPacketToClient(init);
                    }
                    SendPacketToClient(active);
                    break;
                }
                default:
                {
                    Log.Print(LogType.Error, $"Unexpected BG status {status}.");
                    break;
                }
            }
        }
        else
        {
            BattlefieldStatusFailed failed = new BattlefieldStatusFailed();
            failed.Ticket = hdr.Ticket;
            failed.Reason = 30;
            failed.BattlefieldListId = GetSession().GameState.GetBattleFieldQueueType(hdr.Ticket.Id);
            SendPacketToClient(failed);
            GetSession().GameState.RemoveBattleFieldQueue(hdr.Ticket.Id);
        }
        GetSession().GameState.StoreBattleFieldQueueType(hdr.Ticket.Id, battlefieldListId);
        if (battlefieldListId != 0)
        {
            GetSession().GameState.StoreBattleFieldQueueArenaType(hdr.Ticket.Id, hdr.ArenaTeamSize);
            GetSession().GameState.StoreBattleFieldQueueBracketId(hdr.Ticket.Id, bracketId);
        }
    }

    [HandlesSmsg(Opcode.MSG_PVP_LOG_DATA, RemovedIn = ClientVersionBuild.V2_0_1_6180)]
    internal void HandlePvPLogDataVanilla(WorldPacket packet)
    {
        PVPMatchStatisticsMessage pvp = new PVPMatchStatisticsMessage();
        if (packet.ReadBool()) // Has Winner
            pvp.Winner = packet.ReadUInt8();

        int count = packet.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            PVPMatchPlayerStatistics player = new PVPMatchPlayerStatistics();
            player.PlayerGUID = packet.ReadGuid().To128(GetSession().GameState);
            player.Rank = packet.ReadInt32();
            player.Kills = packet.ReadUInt32();
            player.Honor = new();
            player.Honor.HonorKills = packet.ReadUInt32();
            player.Honor.Deaths = packet.ReadUInt32();
            player.Honor.ContributionPoints = packet.ReadUInt32();

            int statsCount = packet.ReadInt32();
            for (int j = 0; j < statsCount; j++)
                player.Stats.Add(packet.ReadUInt32());

            FillBgScoreAppearance(player, setFactionFromRace: true);
            pvp.PlayerCount[player.Faction ? 1 : 0]++;
            pvp.Statistics.Add(player);
        }
        SendPacketToClient(pvp);
    }

    [HandlesSmsg(Opcode.MSG_PVP_LOG_DATA, AddedIn = ClientVersionBuild.V2_0_1_6180)]
    internal void HandlePvPLogDataTBC(WorldPacket packet)
    {
        PVPMatchStatisticsMessage pvp = new PVPMatchStatisticsMessage();
        if (packet.ReadBool()) // Has Arena Teams
        {
            pvp.ArenaTeams = new ArenaTeamsInfo();
            pvp.ArenaTeams.Guids[0] = WowGuid128.Empty;
            pvp.ArenaTeams.Guids[1] = WowGuid128.Empty;

            for (int i = 0; i < 2; i++)
            {
                packet.ReadUInt32(); // Rating Lost
                packet.ReadUInt32(); // Rating gained
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                    packet.ReadUInt32(); // MMR
            }

            for (int i = 0; i < 2; i++)
            {
                pvp.ArenaTeams.Names[i] = packet.ReadCString();
            }
        }

        if (packet.ReadBool()) // Has Winner
            pvp.Winner = packet.ReadUInt8();

        int count = packet.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            PVPMatchPlayerStatistics player = new PVPMatchPlayerStatistics();
            player.PlayerGUID = packet.ReadGuid().To128(GetSession().GameState);
            player.Kills = packet.ReadUInt32();

            if (pvp.ArenaTeams == null)
            {
                player.Honor = new();
                player.Honor.HonorKills = packet.ReadUInt32();
                player.Honor.Deaths = packet.ReadUInt32();
                player.Honor.ContributionPoints = packet.ReadUInt32();
            }
            else
            {
                player.Faction = packet.ReadBool();
                pvp.PlayerCount[player.Faction ? 1 : 0]++;
            }

            player.DamageDone = packet.ReadUInt32();
            player.HealingDone = packet.ReadUInt32();

            int statsCount = packet.ReadInt32();
            for (int j = 0; j < statsCount; j++)
                player.Stats.Add(packet.ReadUInt32());

            FillBgScoreAppearance(player, setFactionFromRace: pvp.ArenaTeams == null);
            if (pvp.ArenaTeams == null)
                pvp.PlayerCount[player.Faction ? 1 : 0]++;
            pvp.Statistics.Add(player);
        }
        SendPacketToClient(pvp);
    }

    void FillBgScoreAppearance(PVPMatchPlayerStatistics player, bool setFactionFromRace)
    {
        if (GetSession().GameState.TryGetCachedPlayerAppearance(player.PlayerGUID, out var race, out var classId, out var sex))
        {
            player.Sex = sex;
            player.PlayerRace = race;
            player.PlayerClass = classId;
            if (setFactionFromRace)
                player.Faction = GameData.IsAllianceRace(race);
            return;
        }

        player.Sex = Gender.Male;
        player.PlayerRace = Race.Human;
        player.PlayerClass = Class.Warrior;
        if (setFactionFromRace)
            player.Faction = InferBgFactionFromRaid(player.PlayerGUID);
    }

    bool InferBgFactionFromRaid(WowGuid128 guid)
    {
        var bgRaid = GetSession().GameState.CurrentGroups[1];
        if (bgRaid?.PlayerList == null)
            return false;
        bool inOurRaid = bgRaid.PlayerList.Exists(m => m.GUID == guid);
        bool weAreAlliance = GetSession().GameState.IsAlliancePlayer(GetSession().GameState.CurrentPlayerGuid);
        return inOurRaid ? weAreAlliance : !weAreAlliance;
    }

    /// <summary>
    /// A battle's raid ends where the battle's map is left. TrinityCore 4.3.4 says so itself, with
    /// a group list carrying the removal mark, before it moves the player out; a server that only
    /// moves the player leaves the client in the raid of a battle it is no longer in, unit frames
    /// and all, and nothing the player can click takes it away.
    /// </summary>
    internal void EndBattlegroundRaidLeftBehind()
    {
        var raid = GetSession().GameState.CurrentGroups[1];
        if (raid == null || raid.PartyType != GroupType.PvP)
            return;

        PartyUpdate party = new PartyUpdate();
        party.SequenceNum = GetSession().GameState.GroupUpdateCounter++;
        party.PartyFlags = GroupFlags.Destroyed;
        party.PartyIndex = 1;
        party.PartyGUID = raid.PartyGUID;
        party.LeaderGUID = WowGuid128.Empty;
        party.MyIndex = -1;
        GetSession().GameState.CurrentGroups[1] = null;
        if (GetSession().GameState.LastAnnouncedPartyIndex == 1)
            GetSession().GameState.LastAnnouncedPartyIndex = 0;
        if (GetSession().GameState.CurrentGroups[0] == null)
            GetSession().GameState.GroupAssignedRoles.Clear();
        SendPacketToClient(party);
    }

    BattlegroundPlayerPosition ReadBattlegroundPlayerPosition(WorldPacket packet)
    {
        BattlegroundPlayerPosition position = new BattlegroundPlayerPosition();
        position.Guid = packet.ReadGuid().To128(GetSession().GameState);
        position.Pos = packet.ReadVector2();
        return position;
    }

    [HandlesSmsg(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS, RemovedIn = ClientVersionBuild.V2_0_1_6180)]
    internal void HandleBattlegroundPlayerPositionsVanilla(WorldPacket packet)
    {
        GetSession().GameState.FlagCarrierGuids.Clear();
        BattlegroundPlayerPositions bglist = new BattlegroundPlayerPositions();
        uint teamMembersCount = packet.ReadUInt32();
        for (uint i = 0; i < teamMembersCount; i++)
        {
            ReadBattlegroundPlayerPosition(packet);
        }

        bool hasFlagCarrier = packet.ReadBool();
        if (hasFlagCarrier)
        {
            var position = ReadBattlegroundPlayerPosition(packet);

            if (GetSession().GameState.IsAlliancePlayer(position.Guid))
            {
                position.IconID = 1;
                position.ArenaSlot = 3;
            }
            else
            {
                position.IconID = 2;
                position.ArenaSlot = 2;
            }

            bglist.FlagCarriers.Add(position);
            GetSession().GameState.FlagCarrierGuids.Add(position.Guid);
        }
        SendPacketToClient(bglist);
    }

    [HandlesSmsg(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS, AddedIn = ClientVersionBuild.V2_0_1_6180)]
    internal void HandleBattlegroundPlayerPositionsTBC(WorldPacket packet)
    {
        BattlegroundPlayerPositions bglist = new BattlegroundPlayerPositions();
        uint teamMembersCount = packet.ReadUInt32();
        uint flagCarriersCount = packet.ReadUInt32();
        for (uint i = 0; i < teamMembersCount; i++)
        {
            ReadBattlegroundPlayerPosition(packet);
        }
        GetSession().GameState.FlagCarrierGuids.Clear();
        for (uint i = 0; i < flagCarriersCount; i++)
        {
            var position = ReadBattlegroundPlayerPosition(packet);

            if (GetSession().GameState.IsAlliancePlayer(position.Guid))
            {
                position.IconID = 1;
                position.ArenaSlot = 3;
            }
            else
            {
                position.IconID = 2;
                position.ArenaSlot = 2;
            }

            bglist.FlagCarriers.Add(position);
            GetSession().GameState.FlagCarrierGuids.Add(position.Guid);
        }
        SendPacketToClient(bglist);
    }

    // Legacy 0x2E8 is SMSG_GROUP_JOINED_BATTLEGROUND (int32 result). The 3.3.5
    // table names it SMSG_BATTLEFIELD_STATUS_QUEUED. Rated join-as-group failures
    // only send this packet, so dropping it made Join as Group look like a no-op.
    [HandlesSmsg(Opcode.SMSG_BATTLEFIELD_STATUS_QUEUED)]
    internal void HandleGroupJoinedBattleground(WorldPacket packet)
    {
        if (IsCataLegacy)
        {
            HandleBattlefieldStatusQueuedCata(packet);
            return;
        }

        int result = packet.ReadInt32();
        WowGuid128? playerGuid = null;
        if (packet.CanRead())
            playerGuid = packet.ReadGuid().To128(GetSession().GameState);

        Log.Print(LogType.Debug, $"[BG] SMSG_GROUP_JOINED_BATTLEGROUND: result={result} hasGuid={playerGuid.HasValue}.");

        if (result > 0 || result == -1)
            return;

        string? playerName = playerGuid.HasValue
            ? GetSession().GameState.GetPlayerName(playerGuid.Value)
            : null;
        string? text = BattlefieldQueueArenaType.JoinErrorText(result, playerName);
        // With the ticket flag in place the 3.4.3 client prints the error codes from
        // BATTLEFIELD_STATUS_FAILED itself, so the text would show twice. Result 0 has no
        // client text and keeps it.
        bool clientPrintsError = BattlefieldStatusFailed.HasTicketFlag && result < 0;
        if (!clientPrintsError && !string.IsNullOrEmpty(text))
        {
            PrintNotification notify = new PrintNotification();
            notify.NotifyText = text;
            SendPacketToClient(notify);
        }

        BattlefieldStatusFailed failed = new BattlefieldStatusFailed();
        failed.Ticket.Id = 1;
        failed.Ticket.RequesterGuid = GetSession().GameState.CurrentPlayerGuid;
        failed.Ticket.Time = GetSession().GameState.GetBattleFieldQueueTime(failed.Ticket.Id);
        failed.Ticket.Type = RideType.Battlegrounds;
        failed.BattlefieldListId = GetSession().GameState.GetBattleFieldQueueType(failed.Ticket.Id);
        if (failed.BattlefieldListId == 0)
            failed.BattlefieldListId = 6; // BATTLEGROUND_AA
        failed.Reason = BattlefieldQueueArenaType.ToModernJoinError(result);
        if (playerGuid.HasValue)
            failed.ClientID = playerGuid.Value;
        SendPacketToClient(failed);
    }

    [HandlesSmsg(Opcode.SMSG_BATTLEGROUND_PLAYER_JOINED)]
    [HandlesSmsg(Opcode.SMSG_BATTLEGROUND_PLAYER_LEFT)]
    internal void HandleBattlegroundPlayerLeftOrJoined(WorldPacket packet)
    {
        BattlegroundPlayerLeftOrJoined player = new BattlegroundPlayerLeftOrJoined(packet.GetUniversalOpcode(false));
        if (IsCataLegacy)
        {
            // TrinityCore 4.3.4 BuildPlayerJoinedBattlegroundPacket and BuildPlayerLeftBattlegroundPacket
            Span<bool> mask = stackalloc bool[8];
            Span<byte> guid = stackalloc byte[8];
            guid.Clear();
            if (packet.GetUniversalOpcode(false) == Opcode.SMSG_BATTLEGROUND_PLAYER_JOINED)
            {
                MaskedGuid.ReadMaskBits(packet, mask, [0, 4, 3, 5, 7, 6, 2, 1]);
                MaskedGuid.ReadBytes(packet, mask, guid, [1, 5, 3, 2, 0, 7, 4, 6]);
            }
            else
            {
                MaskedGuid.ReadMaskBits(packet, mask, [7, 6, 2, 4, 5, 1, 3, 0]);
                MaskedGuid.ReadBytes(packet, mask, guid, [4, 2, 5, 7, 0, 6, 1, 3]);
            }
            player.Guid = new WowGuid64(MaskedGuid.ToUInt64(guid)).To128(GetSession().GameState);
        }
        else
            player.Guid = packet.ReadGuid().To128(GetSession().GameState);
        SendPacketToClient(player);
    }

    // ----- 4.3.4 -----
    // TrinityCore 4.3.4 has one opcode for each state of a queue slot where 3.3.5a has one packet
    // with a state field. Each carries the player's GUID and the battleground's, bit by bit among
    // the other fields. The battleground's is its battlemaster list id under a high type, the
    // same number the modern client uses as its queue id.

    // The header's level range has an upper end and 4.3.4 sends only the lower one.
    private const byte CataMaxLevel = 85;

    private BattlefieldStatusHeader CataBattlefieldStatusHeader(
        uint queueSlot, uint battlefieldListId, uint teamSize, byte minLevel, uint instanceId, bool rated, uint joinTime)
    {
        var state = GetSession().GameState;
        bool isArena = GameData.Battlegrounds.TryGetValue(battlefieldListId, out var bg) && bg.IsArena;

        BattlefieldStatusHeader hdr = new BattlefieldStatusHeader();
        hdr.Ticket.Id = 1 + queueSlot;
        hdr.Ticket.RequesterGuid = state.CurrentPlayerGuid;
        hdr.Ticket.Time = state.GetBattleFieldQueueTime(hdr.Ticket.Id);
        hdr.Ticket.Type = RideType.Battlegrounds;
        hdr.BattlefieldListIDs.Add(battlefieldListId);
        hdr.RangeMin = minLevel;
        hdr.RangeMax = CataMaxLevel;
        // the server says 1 for a battleground, where 3.3.5a says 0
        hdr.ArenaTeamSize = isArena ? (byte)teamSize : (byte)0;
        hdr.InstanceID = instanceId;
        hdr.IsArena = rated;

        state.StoreBattleFieldQueueType(hdr.Ticket.Id, battlefieldListId);
        state.StoreBattleFieldQueueArenaType(hdr.Ticket.Id, hdr.ArenaTeamSize);
        state.StoreBattleFieldQueueJoinTime(hdr.Ticket.Id, joinTime);
        return hdr;
    }

    // TrinityCore 4.3.4 WorldPackets::Battleground::BattlefieldList::Write
    internal void HandleBattlefieldListCata(WorldPacket packet)
    {
        BattlefieldList bglist = new BattlefieldList();
        packet.ReadInt32(); // conquest bonus, holiday
        int randomWinConquest = packet.ReadInt32();
        packet.ReadInt32(); // honor for a loss, holiday
        bglist.BattlemasterListID = packet.ReadUInt32();
        int randomLossHonor = packet.ReadInt32();
        int randomWinHonor = packet.ReadInt32();
        packet.ReadInt32(); // honor for a win, holiday
        bglist.MaxLevel = packet.ReadUInt8();
        bglist.MinLevel = packet.ReadUInt8();

        Span<bool> mask = stackalloc bool[8];
        Span<byte> guid = stackalloc byte[8];
        guid.Clear();
        MaskedGuid.ReadMaskBits(packet, mask, [0, 1, 7]);
        packet.HasBit(); // has holiday win today
        bglist.HasRandomWinToday = packet.HasBit();
        uint instancesCount = packet.ReadBits<uint>(24);
        MaskedGuid.ReadMaskBits(packet, mask, [6, 4, 2, 3]);
        packet.HasBit(); // is random
        MaskedGuid.ReadMaskBits(packet, mask, [5]);
        bglist.PvpAnywhere = packet.HasBit();

        MaskedGuid.ReadBytes(packet, mask, guid, [6, 1, 7, 5]);
        for (uint i = 0; i < instancesCount && packet.CanRead(); i++)
            bglist.BattlefieldInstances.Add(packet.ReadInt32());
        MaskedGuid.ReadBytes(packet, mask, guid, [0, 2, 4, 3]);

        // empty when the list was asked for from the PvP window, which 4.3.4 allows anywhere
        ulong battlemaster = MaskedGuid.ToUInt64(guid);
        if (battlemaster != 0)
        {
            bglist.BattlemasterGuid = new WowGuid64(battlemaster).To128(GetSession().GameState);
            GetSession().GameState.CurrentInteractedWithNPC = bglist.BattlemasterGuid;
        }
        else
            bglist.BattlemasterGuid = WowGuid128.Empty;

        // The 4.4.2 window asked for its rewards before this list came; new amounts go to it first,
        // so that the list's own refresh already shows them.
        var rewards = (randomWinHonor, randomWinConquest, randomLossHonor);
        if (GetSession().GameState.CataRandomBattlegroundRewards != rewards)
        {
            GetSession().GameState.CataRandomBattlegroundRewards = rewards;
            SendPacketToClient(BuildPvpRewardsCata());
        }
        SendPacketToClient(bglist);
    }

    // TrinityCore 4.3.4 RequestPVPRewardsResponse: this week's conquest and its caps, overall, from
    // rated battlegrounds and from arenas. The 4.4.2 window takes those from the currencies and
    // wants the rewards here instead.
    [HandlesSmsg(Opcode.SMSG_REQUEST_PVP_REWARDS_RESPONSE)]
    internal void HandleRequestPvpRewardsResponse(WorldPacket packet)
    {
        SendPacketToClient(BuildPvpRewardsCata());
    }

    // The random battleground's amounts as the last 4.3.4 list gave them, for a first win and for
    // a later one alike: the server already chose between the two by whether one was won today.
    // A 4.3.4 server says nothing about rated battlegrounds and arenas here.
    private RequestPvpRewardsResponse BuildPvpRewardsCata()
    {
        RequestPvpRewardsResponse response = new RequestPvpRewardsResponse();
        if (GetSession().GameState.CataRandomBattlegroundRewards is not { } rewards)
            return response;

        LFGPlayerQuestReward win = new LFGPlayerQuestReward { Honor = rewards.WinHonor };
        if (rewards.WinConquest > 0)
            win.Currency = [new LFGPlayerQuestRewardCurrency { CurrencyID = ConquestCurrencyId, Quantity = rewards.WinConquest * 100 }];
        LFGPlayerQuestReward loss = new LFGPlayerQuestReward { Honor = rewards.LossHonor };
        response.FirstRandomBGWinReward = win;
        response.NthRandomBGWinReward = win;
        response.FirstRandomBGLossReward = loss;
        response.NthRandomBGLossReward = loss;
        return response;
    }

    private const int ConquestCurrencyId = 390;

    // The amounts of the last 3.3.5a list in every battleground slot the client reads: the server
    // already chose between a first win of the day and a later one, and TrinityCore, AzerothCore and
    // cMaNGOS pay a holiday battleground as they pay a random one.
    internal static RequestPvpRewardsResponse BuildPvpRewardsWotLK((int WinHonor, int WinArena, int LossHonor) rewards)
    {
        LFGPlayerQuestReward win = new LFGPlayerQuestReward { Honor = rewards.WinHonor };
        if (rewards.WinArena > 0)
            win.Currency = [new LFGPlayerQuestRewardCurrency { CurrencyID = (int)Currency.ArenaPoints, Quantity = rewards.WinArena }];
        LFGPlayerQuestReward loss = new LFGPlayerQuestReward { Honor = rewards.LossHonor };
        return new RequestPvpRewardsResponse
        {
            FirstRandomBGWinReward = win,
            NthRandomBGWinReward = win,
            FirstRandomBGLossReward = loss,
            NthRandomBGLossReward = loss,
            HolidayBGWinReward = win,
            HolidayBGLossReward = loss,
        };
    }

    // SMSG_BATTLEFIELD_STATUS on 4.3.4 is the "no longer in this queue" state alone.
    internal void HandleBattlefieldStatusNoneCata(WorldPacket packet)
    {
        Span<bool> mask = stackalloc bool[8];
        Span<byte> player = stackalloc byte[8];
        MaskedGuid.ReadMaskBits(packet, mask, [0, 4, 7, 1, 6, 3, 5, 2]);
        MaskedGuid.ReadBytes(packet, mask, player, [5, 6, 7, 2]);
        packet.ReadUInt32(); // team size
        MaskedGuid.ReadBytes(packet, mask, player, [3, 1]);
        uint queueSlot = packet.ReadUInt32();
        packet.ReadUInt32(); // join time
        MaskedGuid.ReadBytes(packet, mask, player, [0, 4]);

        var state = GetSession().GameState;
        BattlefieldStatusFailed failed = new BattlefieldStatusFailed();
        failed.Ticket.Id = 1 + queueSlot;
        failed.Ticket.RequesterGuid = state.CurrentPlayerGuid;
        failed.Ticket.Time = state.GetBattleFieldQueueTime(failed.Ticket.Id);
        failed.Ticket.Type = RideType.Battlegrounds;
        failed.Reason = 30; // as the 3.3.5a path: takes the slot away and prints nothing
        failed.BattlefieldListId = state.GetBattleFieldQueueType(failed.Ticket.Id);
        SendPacketToClient(failed);
        state.RemoveBattleFieldQueue(failed.Ticket.Id);
    }

    internal void HandleBattlefieldStatusQueuedCata(WorldPacket packet)
    {
        Span<bool> playerMask = stackalloc bool[8];
        Span<bool> bgMask = stackalloc bool[8];
        Span<byte> player = stackalloc byte[8];
        Span<byte> battleground = stackalloc byte[8];
        battleground.Clear();

        MaskedGuid.ReadMaskBits(packet, playerMask, [3, 0]);
        MaskedGuid.ReadMaskBits(packet, bgMask, [3]);
        MaskedGuid.ReadMaskBits(packet, playerMask, [2]);
        bool eligible = packet.HasBit();
        packet.HasBit(); // join failed
        MaskedGuid.ReadMaskBits(packet, bgMask, [2]);
        MaskedGuid.ReadMaskBits(packet, playerMask, [1]);
        MaskedGuid.ReadMaskBits(packet, bgMask, [0, 6, 4]);
        MaskedGuid.ReadMaskBits(packet, playerMask, [6, 7]);
        MaskedGuid.ReadMaskBits(packet, bgMask, [7, 5]);
        MaskedGuid.ReadMaskBits(packet, playerMask, [4, 5]);
        bool rated = packet.HasBit();
        bool waitingOnOtherActivity = packet.HasBit();
        MaskedGuid.ReadMaskBits(packet, bgMask, [1]);

        MaskedGuid.ReadBytes(packet, playerMask, player, [0]);
        uint teamSize = packet.ReadUInt32();
        MaskedGuid.ReadBytes(packet, bgMask, battleground, [5]);
        MaskedGuid.ReadBytes(packet, playerMask, player, [3]);
        uint estimatedWait = packet.ReadUInt32();
        MaskedGuid.ReadBytes(packet, bgMask, battleground, [7, 1, 2]);
        packet.ReadUInt8();
        MaskedGuid.ReadBytes(packet, bgMask, battleground, [4]);
        MaskedGuid.ReadBytes(packet, playerMask, player, [2]);
        packet.ReadUInt8();
        MaskedGuid.ReadBytes(packet, bgMask, battleground, [6]);
        MaskedGuid.ReadBytes(packet, playerMask, player, [7]);
        MaskedGuid.ReadBytes(packet, bgMask, battleground, [3]);
        MaskedGuid.ReadBytes(packet, playerMask, player, [6]);
        MaskedGuid.ReadBytes(packet, bgMask, battleground, [0]);
        uint joinTime = packet.ReadUInt32();
        uint queueSlot = packet.ReadUInt32();
        byte minLevel = packet.ReadUInt8();
        uint timeInQueue = packet.ReadUInt32();
        MaskedGuid.ReadBytes(packet, playerMask, player, [1, 5]);
        uint instanceId = packet.ReadUInt32();
        MaskedGuid.ReadBytes(packet, playerMask, player, [4]);

        BattlefieldStatusQueued queue = new BattlefieldStatusQueued();
        queue.Hdr = CataBattlefieldStatusHeader(
            queueSlot, (uint)MaskedGuid.ToUInt64(battleground), teamSize, minLevel, instanceId, rated, joinTime);
        queue.AverageWaitTime = estimatedWait;
        queue.WaitTime = timeInQueue;
        queue.EligibleForMatchmaking = eligible;
        queue.SuspendedQueue = waitingOnOtherActivity;
        SendPacketToClient(queue);
    }

    [HandlesSmsg(Opcode.SMSG_BATTLEFIELD_STATUS_NEED_CONFIRMATION)]
    internal void HandleBattlefieldStatusNeedConfirmationCata(WorldPacket packet)
    {
        uint instanceId = packet.ReadUInt32();
        uint timeout = packet.ReadUInt32();
        packet.ReadUInt8();
        uint queueSlot = packet.ReadUInt32();
        uint joinTime = packet.ReadUInt32();
        byte minLevel = packet.ReadUInt8();
        uint teamSize = packet.ReadUInt32();
        uint mapId = packet.ReadUInt32();
        packet.ReadUInt8();

        Span<bool> playerMask = stackalloc bool[8];
        Span<bool> bgMask = stackalloc bool[8];
        Span<byte> player = stackalloc byte[8];
        Span<byte> battleground = stackalloc byte[8];
        battleground.Clear();

        MaskedGuid.ReadMaskBits(packet, playerMask, [5, 2, 1]);
        MaskedGuid.ReadMaskBits(packet, bgMask, [2]);
        MaskedGuid.ReadMaskBits(packet, playerMask, [4]);
        MaskedGuid.ReadMaskBits(packet, bgMask, [6, 3]);
        bool rated = packet.HasBit();
        MaskedGuid.ReadMaskBits(packet, playerMask, [7, 3]);
        MaskedGuid.ReadMaskBits(packet, bgMask, [7, 0, 4]);
        MaskedGuid.ReadMaskBits(packet, playerMask, [6]);
        MaskedGuid.ReadMaskBits(packet, bgMask, [1, 5]);
        MaskedGuid.ReadMaskBits(packet, playerMask, [0]);

        MaskedGuid.ReadBytes(packet, bgMask, battleground, [6, 5, 7, 2]);
        MaskedGuid.ReadBytes(packet, playerMask, player, [0, 7]);
        MaskedGuid.ReadBytes(packet, bgMask, battleground, [4]);
        MaskedGuid.ReadBytes(packet, playerMask, player, [1]);
        MaskedGuid.ReadBytes(packet, bgMask, battleground, [0]);
        MaskedGuid.ReadBytes(packet, playerMask, player, [4]);
        MaskedGuid.ReadBytes(packet, bgMask, battleground, [1]);
        MaskedGuid.ReadBytes(packet, playerMask, player, [5]);
        MaskedGuid.ReadBytes(packet, bgMask, battleground, [3]);
        MaskedGuid.ReadBytes(packet, playerMask, player, [6, 2, 3]);

        BattlefieldStatusNeedConfirmation confirm = new BattlefieldStatusNeedConfirmation();
        confirm.Hdr = CataBattlefieldStatusHeader(
            queueSlot, (uint)MaskedGuid.ToUInt64(battleground), teamSize, minLevel, instanceId, rated, joinTime);
        confirm.Mapid = mapId;
        confirm.Timeout = timeout;
        confirm.Role = HermesProxy.World.Server.Systems.BattlegroundSystem.InvitationRole(
            GetSession().GameState.BattlefieldJoinRoles);
        SendPacketToClient(confirm);
    }

    [HandlesSmsg(Opcode.SMSG_BATTLEFIELD_STATUS_ACTIVE)]
    internal void HandleBattlefieldStatusActiveCata(WorldPacket packet)
    {
        Span<bool> playerMask = stackalloc bool[8];
        Span<bool> bgMask = stackalloc bool[8];
        Span<byte> player = stackalloc byte[8];
        Span<byte> battleground = stackalloc byte[8];
        battleground.Clear();

        MaskedGuid.ReadMaskBits(packet, playerMask, [2, 7]);
        MaskedGuid.ReadMaskBits(packet, bgMask, [7, 1]);
        MaskedGuid.ReadMaskBits(packet, playerMask, [5]);
        bool alliance = packet.HasBit();
        MaskedGuid.ReadMaskBits(packet, bgMask, [0]);
        MaskedGuid.ReadMaskBits(packet, playerMask, [1]);
        MaskedGuid.ReadMaskBits(packet, bgMask, [3]);
        MaskedGuid.ReadMaskBits(packet, playerMask, [6]);
        MaskedGuid.ReadMaskBits(packet, bgMask, [5]);
        bool rated = packet.HasBit();
        MaskedGuid.ReadMaskBits(packet, playerMask, [4]);
        MaskedGuid.ReadMaskBits(packet, bgMask, [6, 4, 2]);
        MaskedGuid.ReadMaskBits(packet, playerMask, [3, 0]);

        MaskedGuid.ReadBytes(packet, bgMask, battleground, [4, 5]);
        MaskedGuid.ReadBytes(packet, playerMask, player, [5]);
        MaskedGuid.ReadBytes(packet, bgMask, battleground, [1, 6, 3, 7]);
        MaskedGuid.ReadBytes(packet, playerMask, player, [6]);
        uint joinTime = packet.ReadUInt32();
        packet.ReadUInt8();
        MaskedGuid.ReadBytes(packet, playerMask, player, [4, 1]);
        uint queueSlot = packet.ReadUInt32();
        packet.ReadUInt8();
        uint teamSize = packet.ReadUInt32();
        uint mapId = packet.ReadUInt32();
        byte minLevel = packet.ReadUInt8();
        uint elapsed = packet.ReadUInt32();
        MaskedGuid.ReadBytes(packet, playerMask, player, [2]);
        uint remaining = packet.ReadUInt32();
        MaskedGuid.ReadBytes(packet, playerMask, player, [0, 3]);
        MaskedGuid.ReadBytes(packet, bgMask, battleground, [2]);
        uint instanceId = packet.ReadUInt32();
        MaskedGuid.ReadBytes(packet, bgMask, battleground, [0]);
        MaskedGuid.ReadBytes(packet, playerMask, player, [7]);

        BattlefieldStatusActive active = new BattlefieldStatusActive();
        active.Hdr = CataBattlefieldStatusHeader(
            queueSlot, (uint)MaskedGuid.ToUInt64(battleground), teamSize, minLevel, instanceId, rated, joinTime);
        active.Mapid = mapId;
        active.ShutdownTimer = remaining;
        active.StartTimer = elapsed;
        active.ArenaFaction = alliance ? (byte)1 : (byte)0;
        if (active.ShutdownTimer == 0)
        {
            BattlegroundInit init = new BattlegroundInit();
            init.Milliseconds = 1154756799;
            SendPacketToClient(init);
        }
        SendPacketToClient(active);
    }

    // TrinityCore 4.3.4 BuildStatusFailedPacket: who caused it, the battleground, and a third
    // GUID the server leaves empty. 4.3.4 has no SMSG_GROUP_JOINED_BATTLEGROUND; every refused
    // join comes here, and its result is of the modern client's own numbering.
    [HandlesSmsg(Opcode.SMSG_BATTLEFIELD_STATUS_FAILED)]
    internal void HandleBattlefieldStatusFailedCata(WorldPacket packet)
    {
        Span<bool> playerMask = stackalloc bool[8];
        Span<bool> bgMask = stackalloc bool[8];
        Span<bool> otherMask = stackalloc bool[8];
        Span<byte> player = stackalloc byte[8];
        Span<byte> battleground = stackalloc byte[8];
        Span<byte> other = stackalloc byte[8];
        player.Clear();
        battleground.Clear();

        MaskedGuid.ReadMaskBits(packet, bgMask, [3]);
        MaskedGuid.ReadMaskBits(packet, otherMask, [3]);
        MaskedGuid.ReadMaskBits(packet, playerMask, [3]);
        MaskedGuid.ReadMaskBits(packet, otherMask, [0]);
        MaskedGuid.ReadMaskBits(packet, bgMask, [6]);
        MaskedGuid.ReadMaskBits(packet, playerMask, [5, 6, 4, 2]);
        MaskedGuid.ReadMaskBits(packet, otherMask, [1]);
        MaskedGuid.ReadMaskBits(packet, bgMask, [1]);
        MaskedGuid.ReadMaskBits(packet, otherMask, [5, 6]);
        MaskedGuid.ReadMaskBits(packet, playerMask, [1]);
        MaskedGuid.ReadMaskBits(packet, bgMask, [7]);
        MaskedGuid.ReadMaskBits(packet, otherMask, [4]);
        MaskedGuid.ReadMaskBits(packet, bgMask, [2, 5]);
        MaskedGuid.ReadMaskBits(packet, otherMask, [7]);
        MaskedGuid.ReadMaskBits(packet, bgMask, [4, 0]);
        MaskedGuid.ReadMaskBits(packet, playerMask, [0]);
        MaskedGuid.ReadMaskBits(packet, otherMask, [2]);
        MaskedGuid.ReadMaskBits(packet, playerMask, [7]);

        MaskedGuid.ReadBytes(packet, bgMask, battleground, [1]);
        packet.ReadUInt32(); // always 1
        uint queueSlot = packet.ReadUInt32();
        MaskedGuid.ReadBytes(packet, playerMask, player, [6, 3, 7, 4]);
        MaskedGuid.ReadBytes(packet, bgMask, battleground, [0]);
        MaskedGuid.ReadBytes(packet, playerMask, player, [5]);
        MaskedGuid.ReadBytes(packet, bgMask, battleground, [7, 6, 2]);
        MaskedGuid.ReadBytes(packet, otherMask, other, [6, 3]);
        MaskedGuid.ReadBytes(packet, playerMask, player, [1]);
        MaskedGuid.ReadBytes(packet, bgMask, battleground, [3]);
        MaskedGuid.ReadBytes(packet, otherMask, other, [0, 1, 4]);
        MaskedGuid.ReadBytes(packet, playerMask, player, [0]);
        MaskedGuid.ReadBytes(packet, bgMask, battleground, [5]);
        MaskedGuid.ReadBytes(packet, otherMask, other, [7]);
        MaskedGuid.ReadBytes(packet, bgMask, battleground, [4]);
        MaskedGuid.ReadBytes(packet, playerMask, player, [2]);
        int result = packet.ReadInt32();
        MaskedGuid.ReadBytes(packet, otherMask, other, [2]);
        packet.ReadUInt32(); // join time
        MaskedGuid.ReadBytes(packet, otherMask, other, [5]);

        var state = GetSession().GameState;
        BattlefieldStatusFailed failed = new BattlefieldStatusFailed();
        failed.Ticket.Id = 1 + queueSlot;
        failed.Ticket.RequesterGuid = state.CurrentPlayerGuid;
        failed.Ticket.Time = state.GetBattleFieldQueueTime(failed.Ticket.Id);
        failed.Ticket.Type = RideType.Battlegrounds;
        failed.BattlefieldListId = (uint)MaskedGuid.ToUInt64(battleground);
        failed.Reason = result;
        ulong culprit = MaskedGuid.ToUInt64(player);
        if (culprit != 0)
            failed.ClientID = new WowGuid64(culprit).To128(state);
        SendPacketToClient(failed);
    }

    // TrinityCore 4.3.4 Battleground::BuildPvPLogDataPacket with BattlegroundScore::AppendToPacket:
    // every player's bits first, then every player's bytes. The server never sets the four rating
    // bits; what follows each is named after WowPacketParser's 4.3.4 parser. That parser has the
    // two leading bits and the two team sizes the other way round, and this is the server's order.
    [HandlesSmsg(Opcode.SMSG_PVP_LOG_DATA)]
    internal void HandlePvPLogDataCata(WorldPacket packet)
    {
        PVPMatchStatisticsMessage pvp = new PVPMatchStatisticsMessage();
        bool rated = packet.HasBit();
        bool arena = packet.HasBit();
        uint[] teamNameLengths = new uint[2];
        if (arena)
        {
            for (int i = 0; i < 2; i++)
                teamNameLengths[i] = packet.ReadBits<uint>(8);
        }

        int count = (int)packet.ReadBits<uint>(21);
        if (count > 200)
            return; // far past any battleground; not this layout

        bool[] masks = new bool[count * 8];
        bool[] hasRatingChange = new bool[count];
        bool[] hasPreMatchRating = new bool[count];
        bool[] hasPreMatchMmr = new bool[count];
        bool[] hasMmrChange = new bool[count];
        bool[] hasHonor = new bool[count];
        bool[] alliance = new bool[count];
        int[] statCounts = new int[count];
        for (int i = 0; i < count; i++)
        {
            Span<bool> mask = masks.AsSpan(i * 8, 8);
            packet.HasBit();
            hasPreMatchRating[i] = packet.HasBit();
            MaskedGuid.ReadMaskBits(packet, mask, [2]);
            hasHonor[i] = packet.HasBit();
            hasPreMatchMmr[i] = packet.HasBit();
            hasRatingChange[i] = packet.HasBit();
            hasMmrChange[i] = packet.HasBit();
            MaskedGuid.ReadMaskBits(packet, mask, [3, 0, 5, 1, 6]);
            alliance[i] = packet.HasBit();
            MaskedGuid.ReadMaskBits(packet, mask, [7]);
            statCounts[i] = (int)packet.ReadBits<uint>(24);
            MaskedGuid.ReadMaskBits(packet, mask, [4]);
        }
        bool ended = packet.HasBit();

        if (rated)
        {
            for (int i = 0; i < 2; i++)
            {
                packet.ReadUInt32(); // matchmaker rating
                packet.ReadUInt32(); // rating lost
                packet.ReadUInt32(); // rating won
            }
        }

        Span<byte> guidBytes = stackalloc byte[8];
        for (int i = 0; i < count; i++)
        {
            ReadOnlySpan<bool> mask = masks.AsSpan(i * 8, 8);
            guidBytes.Clear();

            PVPMatchPlayerStatistics player = new PVPMatchPlayerStatistics();
            player.Faction = alliance[i];
            player.HealingDone = packet.ReadUInt32();
            player.DamageDone = packet.ReadUInt32();
            if (hasHonor[i])
            {
                player.Honor = new();
                player.Honor.ContributionPoints = packet.ReadUInt32();
                player.Honor.Deaths = packet.ReadUInt32();
                player.Honor.HonorKills = packet.ReadUInt32();
            }
            MaskedGuid.ReadBytes(packet, mask, guidBytes, [4]);
            player.Kills = packet.ReadUInt32();
            if (hasRatingChange[i])
                player.RatingChange = packet.ReadInt32();
            MaskedGuid.ReadBytes(packet, mask, guidBytes, [5]);
            if (hasMmrChange[i])
                player.MmrChange = packet.ReadInt32();
            if (hasPreMatchRating[i])
                player.PreMatchRating = packet.ReadUInt32();
            MaskedGuid.ReadBytes(packet, mask, guidBytes, [1, 6]);
            player.PrimaryTalentTree = packet.ReadInt32();
            for (int j = 0; j < statCounts[i] && packet.CanRead(); j++)
                player.Stats.Add(packet.ReadUInt32());
            MaskedGuid.ReadBytes(packet, mask, guidBytes, [0, 3]);
            if (hasPreMatchMmr[i])
                player.PreMatchMMR = packet.ReadUInt32();
            MaskedGuid.ReadBytes(packet, mask, guidBytes, [7, 2]);

            player.PlayerGUID = new WowGuid64(MaskedGuid.ToUInt64(guidBytes)).To128(GetSession().GameState);
            FillBgScoreAppearance(player, setFactionFromRace: false);
            pvp.Statistics.Add(player);
        }

        if (arena)
        {
            pvp.ArenaTeams = new ArenaTeamsInfo();
            for (int i = 0; i < 2; i++)
            {
                pvp.ArenaTeams.Guids[i] = WowGuid128.Empty;
                pvp.ArenaTeams.Names[i] = packet.ReadString(teamNameLengths[i]);
            }
        }

        pvp.PlayerCount[0] = (sbyte)packet.ReadUInt8(); // Horde
        if (ended)
            pvp.Winner = packet.ReadUInt8();
        pvp.PlayerCount[1] = (sbyte)packet.ReadUInt8(); // Alliance
        SendPacketToClient(pvp);
    }

    // TrinityCore 4.3.4 WorldPackets::Misc::StartTimer: sent on entering a battle that has not
    // begun, and every ten seconds until it does.
    [HandlesSmsg(Opcode.SMSG_START_TIMER)]
    internal void HandleStartTimer(WorldPacket packet)
    {
        StartTimer timer = new StartTimer();
        timer.Type = packet.ReadUInt32();
        timer.TimeLeft = packet.ReadInt32();
        timer.TotalTime = packet.ReadInt32();
        SendPacketToClient(timer);
    }

    // TrinityCore 4.3.4 HandleBattlegroundPlayerPositionsOpcode: the flag carriers, in two lists.
    // Which side a carrier is on comes from the carrier, as in the older handlers.
    [HandlesSmsg(Opcode.SMSG_BATTLEFIELD_PLAYER_POSITIONS)]
    internal void HandleBattlegroundPlayerPositionsCata(WorldPacket packet)
    {
        const int MaxCarriers = 4;
        Span<bool> firstMasks = stackalloc bool[8 * MaxCarriers];
        Span<bool> secondMasks = stackalloc bool[8 * MaxCarriers];
        Span<byte> guid = stackalloc byte[8];

        // TrinityCore 4.3.4 lists the Alliance flag's carrier first and the Horde flag's second,
        // one at most in each; another 4.3.4 core puts both carriers in the second list.
        uint firstCount = packet.ReadBits<uint>(22);
        if (firstCount > MaxCarriers)
            return;
        for (int i = 0; i < firstCount; i++)
            MaskedGuid.ReadMaskBits(packet, firstMasks.Slice(i * 8, 8), [3, 5, 1, 6, 7, 0, 2, 4]);
        uint secondCount = packet.ReadBits<uint>(22);
        if (secondCount > MaxCarriers)
            return;
        for (int i = 0; i < secondCount; i++)
            MaskedGuid.ReadMaskBits(packet, secondMasks.Slice(i * 8, 8), [6, 5, 4, 7, 2, 1, 0, 3]);

        GetSession().GameState.FlagCarrierGuids.Clear();
        BattlegroundPlayerPositions bglist = new BattlegroundPlayerPositions();
        for (int i = 0; i < secondCount; i++)
        {
            ReadOnlySpan<bool> mask = secondMasks.Slice(i * 8, 8);
            guid.Clear();
            Vector2 pos;
            MaskedGuid.ReadBytes(packet, mask, guid, [2, 1]);
            pos.Y = packet.ReadFloat();
            MaskedGuid.ReadBytes(packet, mask, guid, [5, 4, 7, 0, 6, 3]);
            pos.X = packet.ReadFloat();
            AddFlagCarrier(bglist, MaskedGuid.ToUInt64(guid), pos);
        }
        for (int i = 0; i < firstCount; i++)
        {
            ReadOnlySpan<bool> mask = firstMasks.Slice(i * 8, 8);
            guid.Clear();
            Vector2 pos;
            MaskedGuid.ReadBytes(packet, mask, guid, [6]);
            pos.X = packet.ReadFloat();
            MaskedGuid.ReadBytes(packet, mask, guid, [5, 3]);
            pos.Y = packet.ReadFloat();
            MaskedGuid.ReadBytes(packet, mask, guid, [1, 7, 0, 2, 4]);
            AddFlagCarrier(bglist, MaskedGuid.ToUInt64(guid), pos);
        }
        SendPacketToClient(bglist);
    }

    private void AddFlagCarrier(BattlegroundPlayerPositions bglist, ulong carrier, Vector2 pos)
    {
        BattlegroundPlayerPosition position = new BattlegroundPlayerPosition();
        position.Guid = new WowGuid64(carrier).To128(GetSession().GameState);
        position.Pos = pos;
        if (GetSession().GameState.IsAlliancePlayer(position.Guid))
        {
            position.IconID = 1;
            position.ArenaSlot = 3;
        }
        else
        {
            position.IconID = 2;
            position.ArenaSlot = 2;
        }
        bglist.FlagCarriers.Add(position);
        GetSession().GameState.FlagCarrierGuids.Add(position.Guid);
    }

    [HandlesSmsg(Opcode.SMSG_AREA_SPIRIT_HEALER_TIME)]
    internal void HandleAreaSpiritHealerTime(WorldPacket packet)
    {
        AreaSpiritHealerTime healer = new AreaSpiritHealerTime();
        healer.HealerGuid = packet.ReadGuid().To128(GetSession().GameState);
        healer.TimeLeft = packet.ReadUInt32();
        SendPacketToClient(healer);
    }

    [HandlesSmsg(Opcode.SMSG_PVP_CREDIT)]
    internal void HandlePvPCredit(WorldPacket packet)
    {
        PvPCredit credit = new PvPCredit();
        credit.OriginalHonor = packet.ReadInt32();
        credit.Target = packet.ReadGuid().To128(GetSession().GameState);
        credit.Rank = packet.ReadUInt32();
        SendPacketToClient(credit);
    }

    [HandlesSmsg(Opcode.SMSG_PLAYER_SKINNED)]
    internal void HandlePlayerSkinned(WorldPacket packet)
    {
        PlayerSkinned skinned = new PlayerSkinned();
        if (packet.CanRead())
            skinned.FreeRepop = packet.ReadBool();
        SendPacketToClient(skinned);
    }
    /// <summary>
    /// TrinityCore 4.3.4 <c>PVPOptionsEnabled::Write</c>: five bits. It names the second, fourth
    /// and fifth (war game battlegrounds, rated battlegrounds, rated arenas) and sets the other two,
    /// calling them ignored by the client; they are taken to be the 4.4.2 client's unrated
    /// battlegrounds and war game arenas, which sit in those places among its first five.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_PVP_OPTIONS_ENABLED)]
    internal void HandlePvPOptionsEnabled(WorldPacket packet)
    {
        PVPOptionsEnabled options = new();
        options.PugBattlegrounds = packet.HasBit();
        options.WargameBattlegrounds = packet.HasBit();
        options.WargameArenas = packet.HasBit();
        options.RatedBattlegrounds = packet.HasBit();
        options.RatedArenas = packet.HasBit();
        SendPacketToClient(options);
    }
}
