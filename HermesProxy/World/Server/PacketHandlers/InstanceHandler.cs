using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Server;

public partial class WorldSocket
{
    // Handlers for CMSG opcodes coming from the modern client

    /// <summary>
    /// The player's answer to the pending-bind prompt. Modern sends a single bit; legacy reads a
    /// byte (WorldSession::HandleInstanceLockResponse). Accepting binds them, declining sends them
    /// to the graveyard, and either way the server clears the pending bind - so dropping this left
    /// the player stuck with a pending bind that nothing resolved.
    /// </summary>
    [PacketHandler(Opcode.CMSG_INSTANCE_LOCK_RESPONSE)]
    void HandleInstanceLockResponse(InstanceLockResponse response)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_INSTANCE_LOCK_RESPONSE);
        packet.WriteBool(response.AcceptLock);
        SendPacketToServer(packet);
    }




}
