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

    // SMSG_CHAT_SERVER_MESSAGE: a line of the server's own, shown as it is.
    private const int ServerMessageString = 3;

    // Handlers for SMSG opcodes coming the legacy world server
    [HandlesSmsg(Opcode.SMSG_MOTD)]
    internal void HandleMotd(WorldPacket packet)
    {
        uint count = packet.ReadUInt32();
        if (ModernVersion.GetCurrentOpcode(Opcode.SMSG_MOTD) != 0)
        {
            MOTD motd = new MOTD();
            for (uint i = 0; i < count; i++)
                motd.Text.Add(packet.ReadCString());
            SendPacketToClient(motd);
        }
        else
        {
            // The 4.4.2 client has no message of the day packet. A server of its own sends each
            // line as a server message (TrinityCore cata_classic HandlePlayerLogin), and so does
            // this. Building a MOTD for that client threw, and with it went the rest of this
            // handler: the time zone and the season were never sent in the world.
            for (uint i = 0; i < count; i++)
            {
                ChatServerMessage line = new ChatServerMessage();
                line.MessageID = ServerMessageString;
                line.StringParam = packet.ReadCString();
                SendPacketToClient(line);
            }
        }

        // These packets don't exist in old clients (for vanilla servers we send them after account data times along with others).
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            SendPacketToClient(WorldSocket.BuildSetTimeZoneInformation());
            SendPacketToClient(WorldSocket.BuildSeasonInfo());
        }
    }
}
