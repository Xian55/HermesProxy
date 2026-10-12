using Framework;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Outbox;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [HandlesSmsg(Opcode.SMSG_INIT_WORLD_STATES)]
    internal void HandleInitWorldStates(WorldPacket packet)
    {
        InitWorldStates states = new InitWorldStates();
        states.MapID = packet.ReadUInt32();
        GetSession().GameState.CurrentMapId = states.MapID;
        states.ZoneID = packet.ReadUInt32();
        states.AreaID = LegacyVersion.AddedInVersion(ClientVersionBuild.V2_1_0_6692) ? packet.ReadUInt32() : states.ZoneID;

        GetSession().GameState.HasWsgAllyFlagCarrier = false;
        GetSession().GameState.HasWsgHordeFlagCarrier = false;

        ushort count = packet.ReadUInt16();
        for (ushort i = 0; i < count; i++)
        {
            uint variable = packet.ReadUInt32();
            int value = packet.ReadInt32();
            if (IsCataLegacy && variable == BattlegroundClock.State)
                value = TranslateBattlegroundClock(value);
            if (variable != 0 || value != 0)
                states.AddState(variable, value);

            if (variable == (uint)WorldStates.WsgFlagStateAlliance)
                GetSession().GameState.HasWsgAllyFlagCarrier = value == 2;
            else if (variable == (uint)WorldStates.WsgFlagStateHorde)
                GetSession().GameState.HasWsgHordeFlagCarrier = value == 2;
        }
        states.AddClassicStates();
        SendPacketToClient(states);

        // Cache so deferred-flush can re-emit AFTER the player CreateObject.
        // TC reference order is INIT_WORLD_STATES at #146 (post player); cmangos sends
        // it earlier in the flow. The V3_4_3 client may want to see world states again
        // post-player as part of the world-ready handshake.
        GetSession().GameState.LastInitWorldStates = states;

        // These packets don't exist in old versions.
        if (LegacyVersion.ExpansionVersion <= 1 || ModernVersion.ExpansionVersion <= 1)
            SendPacketToClient(new SetupCurrency());
        SendPacketToClient(new AllAccountCriteria());

        if (GetSession().GameState.HasWsgHordeFlagCarrier || GetSession().GameState.HasWsgAllyFlagCarrier)
        {
            WorldPacket packet2 = new WorldPacket(BattlegroundPlayerPositionsRequest);
            SendPacket(packet2);
        }

        if (GetSession().GameState.CurrentZoneId != states.ZoneID)
        {
            uint previousZone = GetSession().GameState.CurrentZoneId;
            string oldZoneName = GameData.GetAreaName(previousZone);
            string newZoneName = GameData.GetAreaName(states.ZoneID);
            GetSession().GameState.CurrentZoneId = states.ZoneID;
            if (previousZone == BattlefieldMgrTranslation.WintergraspZoneId &&
                states.ZoneID != BattlefieldMgrTranslation.WintergraspZoneId)
                ClearWintergraspMgrState();
            if (!String.IsNullOrEmpty(oldZoneName) && !String.IsNullOrEmpty(newZoneName))
            {
                foreach (var channel in GameData.GetChatChannelsWithFlags(ChannelFlags.AutoJoin | ChannelFlags.ZoneBased))
                {
                    SendChatLeaveChannel(1, channel.Name + " - " + oldZoneName);
                    SendChatJoinChannel(1, channel.Name + " - " + newZoneName, "");
                }
            }
        }
    }

    private static readonly HoldKey BattlegroundClockKey = new(HoldKeyKind.BattlegroundClock);

    // The server's time, as far as the last time query told: the client asks on every map change.
    private long LegacyServerTime
        => DateTimeOffset.UtcNow.ToUnixTimeSeconds() + GetSession().GameState.LegacyServerClockOffset;

    /// <summary>
    /// Turns a 4.3.4 end time into the minutes the modern client prints, and keeps them counting
    /// down. See <see cref="BattlegroundClock"/>.
    /// </summary>
    private int TranslateBattlegroundClock(int value)
    {
        if (!BattlegroundClock.IsEndTime(value))
        {
            StopBattlegroundClock();
            return value;
        }

        var state = GetSession().GameState;
        state.BattlegroundClockEnd = value;
        state.BattlegroundClockMap = state.CurrentMapId;
        ArmBattlegroundClock();
        return BattlegroundClock.MinutesLeft(value, LegacyServerTime);
    }

    private void StopBattlegroundClock()
    {
        GetSession().GameState.BattlegroundClockEnd = 0;
        GetSession().ToClient.Cancel(BattlegroundClockKey);
    }

    private void ArmBattlegroundClock()
    {
        var toClient = GetSession().ToClient;
        toClient.Cancel(BattlegroundClockKey);
        int seconds = BattlegroundClock.SecondsToNextMinute(GetSession().GameState.BattlegroundClockEnd, LegacyServerTime);
        if (seconds == 0)
            return;

        toClient.Delay(TimeSpan.FromSeconds(seconds), SendBattlegroundClockMinute,
            new HoldOptions(Key: BattlegroundClockKey));
    }

    private void SendBattlegroundClockMinute()
    {
        var state = GetSession().GameState;
        if (state.BattlegroundClockEnd == 0)
            return;
        if (state.CurrentMapId != state.BattlegroundClockMap)
        {
            state.BattlegroundClockEnd = 0; // left the battle; its clock is not this map's
            return;
        }

        UpdateWorldState update = new UpdateWorldState();
        update.VariableID = BattlegroundClock.State;
        update.Value = BattlegroundClock.MinutesLeft(state.BattlegroundClockEnd, LegacyServerTime);
        SendPacketToClient(update);
        ArmBattlegroundClock();
    }

    [HandlesSmsg(Opcode.SMSG_UPDATE_WORLD_STATE)]
    internal void HandleUpdateWorldState(WorldPacket packet)
    {
        UpdateWorldState update = new UpdateWorldState();
        update.VariableID = packet.ReadUInt32();
        update.Value = packet.ReadInt32();
        if (IsCataLegacy)
        {
            update.Hidden = packet.ReadBool(); // 4.3.4 UpdateWorldState::Write ends with it
            if (update.VariableID == BattlegroundClock.State)
                update.Value = TranslateBattlegroundClock(update.Value);
            else if (update.VariableID == BattlegroundClock.ActiveState && update.Value == 0)
                StopBattlegroundClock();
        }
        SendPacketToClient(update);

        if (update.VariableID == (uint)WorldStates.WsgFlagStateAlliance)
        {
            WorldPacket packet2 = new WorldPacket(BattlegroundPlayerPositionsRequest);
            SendPacket(packet2);
            GetSession().GameState.HasWsgAllyFlagCarrier = update.Value == 2;
        }    
        else if (update.VariableID == (uint)WorldStates.WsgFlagStateHorde)
        {
            WorldPacket packet2 = new WorldPacket(BattlegroundPlayerPositionsRequest);
            SendPacket(packet2);
            GetSession().GameState.HasWsgHordeFlagCarrier = update.Value == 2;
        }
    }
}
