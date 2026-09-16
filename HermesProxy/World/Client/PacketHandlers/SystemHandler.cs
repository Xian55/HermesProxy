using Framework;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [HandlesSmsg(Opcode.SMSG_FEATURE_SYSTEM_STATUS)]
    internal void HandleFeatureSystemStatus(WorldPacket packet)
    {
        SendPacketToClient(WorldSocket.BuildFeatureSystemStatus(GetSession()));
    }

    // Handlers for SMSG opcodes coming the legacy world server
    [HandlesSmsg(Opcode.SMSG_MOTD)]
    internal void HandleMotd(WorldPacket packet)
    {
        MOTD motd = new MOTD();
        uint count = packet.ReadUInt32();
        for (uint i = 0; i < count; i++)
            motd.Text.Add(packet.ReadCString());
        SendPacketToClient(motd);

        // These packets don't exist in old clients (for vanilla servers we send them after account data times along with others).
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            SendPacketToClient(WorldSocket.BuildSetTimeZoneInformation());
            SendPacketToClient(WorldSocket.BuildSeasonInfo());
        }
    }
}
