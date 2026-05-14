using Framework;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [PacketHandler(Opcode.SMSG_INIT_WORLD_STATES)]
    void HandleInitWorldStates(WorldPacket packet)
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
            if (variable != 0 || value != 0)
                states.AddState(variable, value);

            if (variable == (uint)WorldStates.WsgFlagStateAlliance)
                GetSession().GameState.HasWsgAllyFlagCarrier = value == 2;
            else if (variable == (uint)WorldStates.WsgFlagStateHorde)
                GetSession().GameState.HasWsgHordeFlagCarrier = value == 2;
        }
        states.AddClassicStates();

        // V1_14_x Classic Era / V2_5_x TBC Classic clients overflow an on-stack
        // parse buffer in SMSG_INIT_WORLD_STATES when the count gets large (issue
        // #64 — V1_14_0 macOS reproduces it intermittently on high-state zones).
        // The V3_4_3 WotLK Classic client tolerates the full list. Cap the
        // forwarded count for pre-V3_4_3 modern clients.
        if (ModernVersion.Build < ClientVersionBuild.V3_4_3_54261)
        {
            int dropped = states.TruncateExcess();
            if (dropped > 0)
                Log.Print(LogType.Network,
                    $"[WorldStates] Capped SMSG_INIT_WORLD_STATES at {InitWorldStates.MaxWorldStates} entries for {ModernVersion.Build} — dropped {dropped} hardcoded additions to fit pre-V3_4_3 client buffer.");
        }

        SendPacketToClient(states);

        // These packets don't exist in old versions.
        if (LegacyVersion.ExpansionVersion <= 1 || ModernVersion.ExpansionVersion <= 1)
            SendPacketToClient(new SetupCurrency());
        SendPacketToClient(new AllAccountCriteria());

        if (GetSession().GameState.HasWsgHordeFlagCarrier || GetSession().GameState.HasWsgAllyFlagCarrier)
        {
            WorldPacket packet2 = new WorldPacket(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
            SendPacket(packet2);
        }

        if (GetSession().GameState.CurrentZoneId != states.ZoneID)
        {
            string oldZoneName = GameData.GetAreaName(GetSession().GameState.CurrentZoneId);
            string newZoneName = GameData.GetAreaName(states.ZoneID);
            GetSession().GameState.CurrentZoneId = states.ZoneID;
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

    [PacketHandler(Opcode.SMSG_UPDATE_WORLD_STATE)]
    void HandleUpdateWorldState(WorldPacket packet)
    {
        UpdateWorldState update = new UpdateWorldState();
        update.VariableID = packet.ReadUInt32();
        update.Value = packet.ReadInt32();
        SendPacketToClient(update);

        if (update.VariableID == (uint)WorldStates.WsgFlagStateAlliance)
        {
            WorldPacket packet2 = new WorldPacket(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
            SendPacket(packet2);
            GetSession().GameState.HasWsgAllyFlagCarrier = update.Value == 2;
        }    
        else if (update.VariableID == (uint)WorldStates.WsgFlagStateHorde)
        {
            WorldPacket packet2 = new WorldPacket(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
            SendPacket(packet2);
            GetSession().GameState.HasWsgHordeFlagCarrier = update.Value == 2;
        }
    }
}
