using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server;

public partial class WorldSocket
{
    // Handlers for CMSG opcodes coming from the modern client

    [PacketHandler(Opcode.CMSG_WHO)]
    void HandleWhoRequest(WhoRequestPkt who)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_WHO);
        packet.WriteInt32(who.Request.MinLevel);
        packet.WriteInt32(who.Request.MaxLevel);
        packet.WriteCString(who.Request.Name);
        packet.WriteCString(who.Request.Guild);
        packet.WriteInt32((int)who.Request.RaceFilter);
        packet.WriteInt32(who.Request.ClassFilter);

        packet.WriteInt32(who.Areas.Count);
        foreach (int area in who.Areas)
            packet.WriteInt32(area);

        packet.WriteInt32(who.Request.Words.Count);
        foreach (string word in who.Request.Words)
            packet.WriteCString(word);

        SendPacketToServer(packet);
        GetSession().GameState.LastWhoRequestId = who.RequestID;
    }
}
