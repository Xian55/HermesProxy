using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Movement;

/// <summary>
/// CMSG_MOVE_SET_COLLISION_HEIGHT_ACK is a movement ack (the mover's movement and the move counter)
/// followed by the height, the mount display and the reason (TrinityCore cata_classic and
/// wotlk_classic MoveSetCollisionHeightAck::Read). The codec used to read the height straight after
/// the GUID, which nobody noticed while the ack was thrown away.
/// </summary>
public sealed class CollisionHeightAckCodecTests
{
    [Fact]
    public void Read_TakesTheMovementAckBeforeTheHeight()
    {
        var guid = new WowGuid128(77, 0x0800040000000000);
        using var payload = new WorldPacket();
        payload.WritePackedGuid128(guid);
        bool bitFlags = ModernVersion.Build is ClientVersionBuild.V1_14_0_40237
            or ClientVersionBuild.V2_5_2_39570;
        // a stationary movement block, written by hand: flags, time, XYZO, pitch, elevation, counts
        if (!bitFlags)
            for (int i = 0; i < 3; i++) payload.WriteUInt32(0);
        for (int i = 0; i < 9; i++) payload.WriteUInt32(0);
        if (bitFlags)
            for (int i = 0; i < 6; i++) payload.WriteUInt8(0); // 30 + 18 flag bits
        payload.WriteUInt8(0); // no transport, fall, spline or optional data
        payload.WriteUInt32(6); // move counter
        payload.WriteFloat(2.0313f);
        payload.WriteUInt32(2410); // mount display
        payload.WriteUInt8(1); // Mount
        var reader = new SpanPacketReader(payload.GetDataSpan());

        MoveSetCollisionHeightAckCodec.Read(ref reader, out var decoded);

        Assert.Equal(guid, decoded.MoverGUID);
        Assert.Equal(6u, decoded.Ack.MoveCounter);
        Assert.Equal(2.0313f, decoded.Height);
        Assert.Equal(2410u, decoded.MountDisplayID);
        Assert.Equal(1, decoded.Reason);
        Assert.Equal(0, reader.Remaining);
    }
}
