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
/// Pins the two ways into the modern movement read against each other.
/// </summary>
/// <remarks>
/// There is one reader, over a span. The <c>WorldPacket</c> overload hands it the packet's
/// remaining bytes and then skips what it consumed, so what can go wrong is the bookkeeping: the
/// packet left at a different position than the reader reached, or a bit cache carried across.
/// Its only caller is <c>SpellCastRequest.Read(WorldPacket)</c>, the oracle in
/// <c>SpellCodecEquivalenceTests</c>.
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
        ModernMovementCodec.Write(w, mover, in source);
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
            info = info with
            {
                Flags = MovementFlagModern.Falling,
                FallTime = 777,
                JumpVerticalSpeed = -9.81f,
                JumpSinAngle = 0.5f,
                JumpCosAngle = 0.86f,
                JumpHorizontalSpeed = 7.5f,
            };
        }

        if (onTransport)
        {
            info = info with
            {
                Transport = new TransportInfo
                {
                    Guid = new WowGuid128(0xABCDEF, 0x123456),
                    Offset = new Vector3(1f, 2f, 3f),
                    Orientation = 1.5f,
                    Seat = 3,
                    Time = 999,
                },
            };
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
        ModernMovementCodec.Read(viaWorldPacket, out MovementInfo expected);

        // Span path: through the same accessor the dispatch site uses.
        var reader = new SpanPacketReader(new WorldPacket(framed).GetRemainingSpan());
        reader.ReadPackedGuid128();
        ModernMovementCodec.Read(ref reader, out MovementInfo actual);

        Assert.Equal(expected, actual);
        Assert.Equal(falling, actual.FallTime == 777);
        Assert.Equal(onTransport, actual.Transport != null);

        // Position is what catches a reader that agreed on every field of this fixture but
        // consumed a different number of bytes — fatal in a stream, invisible in isolation.
        Assert.Equal(viaWorldPacket.Remaining(), reader.Remaining);
    }

    [Fact]
    public void NoTransportPart_ReadsAsNoTransport()
    {
        // Seat 0 is a real seat, so "not riding anything" cannot be a transport part of zeroes:
        // it has to be no transport part at all.
        byte[] framed = Frame(Sample(falling: false, onTransport: false), default);

        var reader = new SpanPacketReader(new WorldPacket(framed).GetRemainingSpan());
        reader.ReadPackedGuid128();
        ModernMovementCodec.Read(ref reader, out MovementInfo info);

        Assert.Null(info.Transport);
        Assert.Equal(default, info.TransportGuid);
    }

    [Fact]
    public void ClientPlayerMovementCodec_ReadsGuidThenMovementInfo()
    {
        var mover = new WowGuid128(0xDEADBEEFUL, 0xCAFEBABEUL);
        byte[] framed = Frame(Sample(falling: true, onTransport: true), mover);

        var reader = new SpanPacketReader(new WorldPacket(framed).GetRemainingSpan());
        ClientPlayerMovementCodec.Read(ref reader, out var packet);

        Assert.Equal(mover, packet.Guid);
        Assert.Equal(123456u, packet.MoveInfo.MoveTime);
        Assert.Equal((sbyte)3, packet.MoveInfo.Transport!.Value.Seat);
        Assert.Equal(0, reader.Remaining);
    }
}
