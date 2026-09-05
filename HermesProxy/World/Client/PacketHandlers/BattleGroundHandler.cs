using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World;
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
    [PacketHandler(Opcode.SMSG_BATTLEFIELD_LIST, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
    void HandleBattlefieldListVanilla(WorldPacket packet)
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

    [PacketHandler(Opcode.SMSG_BATTLEFIELD_LIST, ClientVersionBuild.V2_0_1_6180, ClientVersionBuild.V3_0_2_9056)]
    void HandleBattlefieldListTBC(WorldPacket packet)
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

    [PacketHandler(Opcode.SMSG_BATTLEFIELD_LIST, ClientVersionBuild.V3_0_2_9056)]
    void HandleBattlefieldListWotLK(WorldPacket packet)
    {
        BattlefieldList bglist = new BattlefieldList();
        bglist.BattlemasterGuid = packet.ReadGuid().To128(GetSession().GameState);
        GetSession().GameState.CurrentInteractedWithNPC = bglist.BattlemasterGuid;
        bglist.PvpAnywhere = packet.ReadBool(); // from UI
        bglist.BattlemasterListID = packet.ReadUInt32();
        bglist.MinLevel = packet.ReadUInt8();
        bglist.MaxLevel = packet.ReadUInt8();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685))
        {
            packet.ReadBool(); // Has Win
            packet.ReadInt32(); // Winner Honor Reward
            packet.ReadInt32(); // Winner Arena Reward
            packet.ReadInt32(); // Loser Honor Reward

            if (packet.ReadBool()) // Is random
            {
                bglist.HasRandomWinToday = packet.ReadBool();
                packet.ReadInt32(); // Random Winner Honor Reward
                packet.ReadInt32(); // Random Winner Arena Reward
                packet.ReadInt32(); // Random Loser Honor Reward
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

    [PacketHandler(Opcode.SMSG_BATTLEFIELD_STATUS, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
    void HandleBattlefieldStatusVanilla(WorldPacket packet)
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

    [PacketHandler(Opcode.SMSG_BATTLEFIELD_STATUS, ClientVersionBuild.V2_0_1_6180)]
    void HandleBattlefieldStatusTBC(WorldPacket packet)
    {
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

    [PacketHandler(Opcode.MSG_PVP_LOG_DATA, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
    void HandlePvPLogDataVanilla(WorldPacket packet)
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

    [PacketHandler(Opcode.MSG_PVP_LOG_DATA, ClientVersionBuild.V2_0_1_6180)]
    void HandlePvPLogDataTBC(WorldPacket packet)
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

    BattlegroundPlayerPosition ReadBattlegroundPlayerPosition(WorldPacket packet)
    {
        BattlegroundPlayerPosition position = new BattlegroundPlayerPosition();
        position.Guid = packet.ReadGuid().To128(GetSession().GameState);
        position.Pos = packet.ReadVector2();
        return position;
    }

    [PacketHandler(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
    void HandleBattlegroundPlayerPositionsVanilla(WorldPacket packet)
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

    [PacketHandler(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS, ClientVersionBuild.V2_0_1_6180)]
    void HandleBattlegroundPlayerPositionsTBC(WorldPacket packet)
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
    [PacketHandler(Opcode.SMSG_BATTLEFIELD_STATUS_QUEUED)]
    void HandleGroupJoinedBattleground(WorldPacket packet)
    {
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
        if (!string.IsNullOrEmpty(text))
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

    [PacketHandler(Opcode.SMSG_BATTLEGROUND_PLAYER_JOINED)]
    [PacketHandler(Opcode.SMSG_BATTLEGROUND_PLAYER_LEFT)]
    void HandleBattlegroundPlayerLeftOrJoined(WorldPacket packet)
    {
        BattlegroundPlayerLeftOrJoined player = new BattlegroundPlayerLeftOrJoined(packet.GetUniversalOpcode(false));
        player.Guid = packet.ReadGuid().To128(GetSession().GameState);
        SendPacketToClient(player);
    }

    [PacketHandler(Opcode.SMSG_AREA_SPIRIT_HEALER_TIME)]
    void HandleAreaSpiritHealerTime(WorldPacket packet)
    {
        AreaSpiritHealerTime healer = new AreaSpiritHealerTime();
        healer.HealerGuid = packet.ReadGuid().To128(GetSession().GameState);
        healer.TimeLeft = packet.ReadUInt32();
        SendPacketToClient(healer);
    }

    [PacketHandler(Opcode.SMSG_PVP_CREDIT)]
    void HandlePvPCredit(WorldPacket packet)
    {
        PvPCredit credit = new PvPCredit();
        credit.OriginalHonor = packet.ReadInt32();
        credit.Target = packet.ReadGuid().To128(GetSession().GameState);
        credit.Rank = packet.ReadUInt32();
        SendPacketToClient(credit);
    }

    [PacketHandler(Opcode.SMSG_PLAYER_SKINNED)]
    void HandlePlayerSkinned(WorldPacket packet)
    {
        PlayerSkinned skinned = new PlayerSkinned();
        if (packet.CanRead())
            skinned.FreeRepop = packet.ReadBool();
        SendPacketToClient(skinned);
    }
}
