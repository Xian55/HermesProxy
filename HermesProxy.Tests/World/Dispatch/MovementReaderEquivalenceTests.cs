using System;
using Framework.IO;
using HermesProxy;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Pins the two implementations of the modern movement read against each other.
/// </summary>
/// <remarks>
/// <para>
/// <c>MovementInfo</c> now has <c>ReadMovementInfoModern(WorldPacket)</c> and
/// <c>ReadMovementInfoModern(ref SpanPacketReader)</c>. Two copies of one wire layout is exactly
/// the hand-sync hazard <c>docs/version-shape-dispatch.md</c> describes for
/// <c>Write()</c>/<c>WriteToSpan()</c>: a fix applied to one and forgotten in the other shows up
/// only on whichever path the traffic happens to take, and never throws.
/// </para>
/// <para>
/// The span version was generated mechanically from the WorldPacket one, so they start identical.
/// This is what keeps them that way. The WorldPacket pair exists only for <c>SpellCastRequest</c>,
/// the last unconverted caller — when it converts, delete both and this test with them.
/// </para>
/// </remarks>
public class MovementReaderEquivalenceTests
{
    static MovementReaderEquivalenceTests()
    {
        if (VersionBootstrap.ModernBuild == ClientVersionBuild.Zero)
            VersionBootstrap.ModernBuild = ClientVersionBuild.V1_14_2_42597;
        if (VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            VersionBootstrap.LegacyBuild = ClientVersionBuild.V3_3_5a_12340;
    }

    /// Builds a real payload with the production writer, so the bytes are the shape the readers
    /// actually meet rather than something invented for the test.
    private static byte[] Frame(MovementInfo source, WowGuid128 mover)
    {
        using var w = new WorldPacket(1u);
        source.WriteMovementInfoModern(w, mover);
        byte[] payload = w.GetData();
        byte[] framed = new byte[payload.Length + 2];
        payload.CopyTo(framed, 2);
        return framed;
    }

    private static MovementInfo Sample(bool falling, bool onTransport)
    {
        var info = new MovementInfo
        {
            MoveTime = 123456,
            Position = new Vector3(1234.5f, -678.25f, 42.125f),
            Orientation = 3.14f,
            SwimPitch = -0.5f,
            SplineElevation = 2.25f,
        };

        if (falling)
        {
            info.Flags = (uint)MovementFlagModern.Falling;
            info.FallTime = 777;
            info.JumpVerticalSpeed = -9.81f;
            info.JumpSinAngle = 0.5f;
            info.JumpCosAngle = 0.86f;
            info.JumpHorizontalSpeed = 7.5f;
        }

        if (onTransport)
        {
            info.TransportGuid = new WowGuid128(0xABCDEF, 0x123456);
            info.TransportOffset = new Vector3(1f, 2f, 3f);
            info.TransportOrientation = 1.5f;
            info.TransportSeat = 3;
            info.TransportTime = 999;
        }

        return info;
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void BothReaders_AgreeOnEveryFieldAndFinalPosition(bool falling, bool onTransport)
    {
        var mover = new WowGuid128(0x1122334455667788UL, 0x99AABBCCDDEEFF00UL);
        byte[] framed = Frame(Sample(falling, onTransport), mover);

        // WorldPacket path: consume the mover GUID first, exactly as the packet classes did.
        var viaWorldPacket = new WorldPacket(framed);
        viaWorldPacket.ReadPackedGuid128();
        var expected = new MovementInfo();
        expected.ReadMovementInfoModern(viaWorldPacket);

        // Span path: through the same accessor the dispatch site uses.
        var reader = new SpanPacketReader(new WorldPacket(framed).GetRemainingSpan());
        reader.ReadPackedGuid128();
        var actual = new MovementInfo();
        actual.ReadMovementInfoModern(ref reader);

        Assert.Equal(expected.Flags, actual.Flags);
        Assert.Equal(expected.FlagsExtra, actual.FlagsExtra);
        Assert.Equal(expected.FlagsExtra2, actual.FlagsExtra2);
        Assert.Equal(expected.MoveTime, actual.MoveTime);
        Assert.Equal(expected.Position, actual.Position);
        Assert.Equal(expected.Orientation, actual.Orientation);
        Assert.Equal(expected.SwimPitch, actual.SwimPitch);
        Assert.Equal(expected.SplineElevation, actual.SplineElevation);

        Assert.Equal(expected.FallTime, actual.FallTime);
        Assert.Equal(expected.JumpVerticalSpeed, actual.JumpVerticalSpeed);
        Assert.Equal(expected.JumpSinAngle, actual.JumpSinAngle);
        Assert.Equal(expected.JumpCosAngle, actual.JumpCosAngle);
        Assert.Equal(expected.JumpHorizontalSpeed, actual.JumpHorizontalSpeed);

        Assert.Equal(expected.TransportGuid, actual.TransportGuid);
        Assert.Equal(expected.TransportOffset, actual.TransportOffset);
        Assert.Equal(expected.TransportOrientation, actual.TransportOrientation);
        Assert.Equal(expected.TransportSeat, actual.TransportSeat);
        Assert.Equal(expected.TransportTime, actual.TransportTime);
        Assert.Equal(expected.TransportTime2, actual.TransportTime2);
        Assert.Equal(expected.VehicleId, actual.VehicleId);

        // Position is what catches a reader that agreed on every field of this fixture but
        // consumed a different number of bytes — fatal in a stream, invisible in isolation.
        Assert.Equal(viaWorldPacket.Remaining(), reader.Remaining);
    }

    [Fact]
    public void TransportSeatDefault_SurvivesAPacketWithNoTransport()
    {
        // MovementInfo initialises TransportSeat to -1, not 0. A reader that skipped the
        // transport block but zeroed the field instead of leaving it would look correct in every
        // aggregate test and be wrong about "no seat".
        byte[] framed = Frame(Sample(falling: false, onTransport: false), default);

        var reader = new SpanPacketReader(new WorldPacket(framed).GetRemainingSpan());
        reader.ReadPackedGuid128();
        var info = new MovementInfo();
        info.ReadMovementInfoModern(ref reader);

        Assert.Equal(-1, info.TransportSeat);
        Assert.Equal(Quaternion.Identity, info.Rotation);
    }

    [Fact]
    public void ClientPlayerMovementCodec_ReadsGuidThenMovementInfo()
    {
        var mover = new WowGuid128(0xDEADBEEFUL, 0xCAFEBABEUL);
        byte[] framed = Frame(Sample(falling: true, onTransport: true), mover);

        var reader = new SpanPacketReader(new WorldPacket(framed).GetRemainingSpan());
        ClientPlayerMovementCodec.Read(ref reader, out var packet);

        Assert.Equal(mover, packet.Guid);
        Assert.NotNull(packet.MoveInfo);
        Assert.Equal(123456u, packet.MoveInfo.MoveTime);
        Assert.Equal(3, packet.MoveInfo.TransportSeat);
        Assert.Equal(0, reader.Remaining);
    }
}
