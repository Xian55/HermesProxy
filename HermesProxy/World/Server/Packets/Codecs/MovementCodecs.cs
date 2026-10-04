using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Objects;

namespace HermesProxy.World.Server.Packets;

// Movement CMSG codecs — the highest-rate inbound family.
//
// The movement block itself is read by ModernMovementCodec (World/Objects), which owns the
// per-build layout. MovementInfo is a value type, so the packet structs below hold it inline and
// a movement packet is decoded without an allocation.

public static class MovementAckCodec
{
    /// <summary>Shared by the ack packets. Mirrors <c>MovementAck.Read</c>.</summary>
    public static void Read(ref SpanPacketReader r, out MovementAck ack)
    {
        ModernMovementCodec.Read(ref r, out MovementInfo moveInfo);
        ack = new MovementAck { MoveInfo = moveInfo, MoveCounter = r.ReadUInt32() };
    }
}

public static class ClientPlayerMovementCodec
{
    public static void Read(ref SpanPacketReader r, out ClientPlayerMovement packet)
    {
        packet.Guid = r.ReadPackedGuid128();
        ModernMovementCodec.Read(ref r, out packet.MoveInfo);
    }
}

public static class MoveTeleportAckCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out MoveTeleportAck packet)
    {
        WowGuid128 mover = r.ReadPackedGuid128();
        uint moveCounter = r.ReadUInt32();
        uint moveTime = r.ReadUInt32();
        packet = new MoveTeleportAck(mover, moveCounter, moveTime);
    }
}

public static class WorldPortResponseCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out WorldPortResponse packet)
        => packet = default;
}

public static class MovementSpeedAckCodec
{
    public static void Read(ref SpanPacketReader r, out MovementSpeedAck packet)
    {
        WowGuid128 mover = r.ReadPackedGuid128();
        MovementAckCodec.Read(ref r, out var ack);
        float speed = r.ReadFloat();
        packet = new MovementSpeedAck(mover, ack, speed);
    }
}

public static class MovementAckMessageCodec
{
    public static void Read(ref SpanPacketReader r, out MovementAckMessage packet)
    {
        WowGuid128 mover = r.ReadPackedGuid128();
        MovementAckCodec.Read(ref r, out var ack);
        packet = new MovementAckMessage(mover, ack);
    }
}

public static class MoveSetCollisionHeightAckCodec
{
    public static void Read(ref SpanPacketReader r, out MoveSetCollisionHeightAck packet)
    {
        WowGuid128 mover = r.ReadPackedGuid128();
        float height = r.ReadFloat();
        uint mountDisplayId = r.ReadUInt32();
        byte reason = r.ReadUInt8();
        packet = new MoveSetCollisionHeightAck(mover, height, mountDisplayId, reason);
    }
}

public static class SetActiveMoverCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SetActiveMover packet)
        => packet = new SetActiveMover(r.ReadPackedGuid128());
}

public static class InitActiveMoverCompleteCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out InitActiveMoverComplete packet)
        => packet = new InitActiveMoverComplete(r.ReadUInt32());
}

public static class MoveSplineDoneCodec
{
    public static void Read(ref SpanPacketReader r, out MoveSplineDone packet)
    {
        WowGuid128 guid = r.ReadPackedGuid128();
        ModernMovementCodec.Read(ref r, out MovementInfo moveInfo);
        int splineId = r.ReadInt32();
        packet = new MoveSplineDone(guid, moveInfo, splineId);
    }
}

public static class MoveTimeSkippedCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out MoveTimeSkipped packet)
    {
        WowGuid128 mover = r.ReadPackedGuid128();
        uint timeSkipped = r.ReadUInt32();
        packet = new MoveTimeSkipped(mover, timeSkipped);
    }
}

public static class RequestVehicleSeatChangeCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out RequestVehicleSeatChange packet)
        => packet = default;
}
