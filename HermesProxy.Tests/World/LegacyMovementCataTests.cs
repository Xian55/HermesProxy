using System;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// The 4.3.4 movement writer against a reader ported from TrinityCore 4.3.4's
/// Player::ReadMovementInfo, which is what the server runs on every movement packet.
/// </summary>
public sealed class LegacyMovementCataTests
{
    [Fact]
    public void EverySequenceHasElements()
    {
        // A dictionary built before the arrays it points at holds nulls, and each movement
        // packet then went out as a bare opcode.
        foreach (var (opcode, sequence) in LegacyMovementSequencesCata.ByOpcode)
        {
            Assert.NotNull(sequence);
            Assert.True(sequence.Length > 0, opcode.ToString());
        }
    }

    [Theory]
    [InlineData(Opcode.MSG_MOVE_START_FORWARD)]
    [InlineData(Opcode.MSG_MOVE_HEARTBEAT)]
    [InlineData(Opcode.MSG_MOVE_JUMP)]
    [InlineData(Opcode.MSG_MOVE_FALL_LAND)]
    [InlineData(Opcode.MSG_MOVE_SET_FACING)]
    [InlineData(Opcode.CMSG_MOVE_KNOCK_BACK_ACK)]
    public void ServerReadsBackWhatWasWritten(Opcode opcode)
    {
        var info = new MovementInfo
        {
            Flags = MovementFlagModern.Forward | MovementFlagModern.Falling,
            MoveTime = 123456,
            Position = new Vector3(-8423.81f, 1361.3f, 104.671f),
            Orientation = 1.5f,
            FallTime = 250,
            JumpVerticalSpeed = -7.5f,
            JumpSinAngle = 0.25f,
            JumpCosAngle = 0.75f,
            JumpHorizontalSpeed = 7f,
        };
        var mover = new WowGuid64(0x0000000000A1B2C3);
        var sequence = LegacyMovementSequencesCata.ByOpcode[opcode];

        using var packet = new WorldPacket(Opcode.MSG_MOVE_HEARTBEAT);
        LegacyMovementCata.Write(packet, sequence, mover, in info, movementCounter: 7);
        packet.FlushBits();
        // A received WorldPacket starts with its 2-byte opcode.
        byte[] body = packet.GetDataSpan().ToArray();
        byte[] framed = new byte[sizeof(ushort) + body.Length];
        body.CopyTo(framed, sizeof(ushort));
        using var read = new WorldPacket(framed);

        var back = ReadLikeTrinityCore434(read, sequence);

        Assert.Equal(mover.Low, back.Guid);
        Assert.Equal((uint)info.Flags, back.Flags);
        Assert.Equal(info.MoveTime, back.Time);
        Assert.Equal(info.Position, back.Position);
        Assert.Equal(info.Orientation, back.Orientation);
        Assert.Equal(info.FallTime, back.FallTime);
        Assert.Equal(info.JumpVerticalSpeed, back.VerticalSpeed);
        Assert.Equal(info.JumpHorizontalSpeed, back.HorizontalSpeed);
        Assert.Equal(0, read.GetRemainingSpan().Length);
    }

    [Theory]
    [InlineData(Opcode.SMSG_MOVE_SET_RUN_SPEED)]
    [InlineData(Opcode.SMSG_MOVE_SPLINE_SET_RUN_SPEED)]
    [InlineData(Opcode.SMSG_MOVE_ROOT)]
    [InlineData(Opcode.SMSG_MOVE_SPLINE_UNROOT)]
    public void ReaderUndoesTheWriter(Opcode opcode)
    {
        var info = new MovementInfo { Position = new Vector3(1f, 2f, 3f), Orientation = 0.5f, MoveTime = 99 };
        var mover = new WowGuid64(0x0000000000C0FFEE);
        using var packet = new WorldPacket(Opcode.MSG_MOVE_HEARTBEAT);
        LegacyMovementCata.Write(packet, LegacyMovementSequencesCata.ByOpcode[opcode], mover, in info,
            movementCounter: 42, extraFloat: 7.5f);
        packet.FlushBits();
        byte[] body = packet.GetDataSpan().ToArray();
        byte[] framed = new byte[sizeof(ushort) + body.Length];
        body.CopyTo(framed, sizeof(ushort));
        using var read = new WorldPacket(framed);

        Assert.True(LegacyMovementCata.TryRead(read, opcode, null!, out ulong guid, out _, out uint counter, out float extra));

        Assert.Equal(mover.Low, guid);
        var sequence = LegacyMovementSequencesCata.ByOpcode[opcode];
        if (Array.IndexOf(sequence, LegacyMovementElement.Counter) >= 0)
            Assert.Equal(42u, counter);
        if (Array.IndexOf(sequence, LegacyMovementElement.ExtraElement) >= 0)
            Assert.Equal(7.5f, extra);
        Assert.Equal(0, read.GetRemainingSpan().Length);
    }

    private sealed record Read(ulong Guid, uint Flags, uint Time, Vector3 Position, float Orientation,
        uint FallTime, float VerticalSpeed, float HorizontalSpeed);

    // TrinityCore 4.3.4 Player::ReadMovementInfo, reduced to the fields asserted above.
    private static Read ReadLikeTrinityCore434(WorldPacket data, LegacyMovementElement[] sequence)
    {
        Span<bool> mask = stackalloc bool[8];
        Span<byte> guid = stackalloc byte[8];
        bool hasFlags = false, hasFlags2 = false, hasTime = false, hasOrientation = false, hasTransport = false;
        bool hasPitch = false, hasFall = false, hasFallDirection = false, hasSplineElevation = false;
        uint flags = 0, time = 0, fallTime = 0;
        float x = 0, y = 0, z = 0, o = 0, vertical = 0, horizontal = 0;

        foreach (var element in sequence)
        {
            switch (element)
            {
                case >= LegacyMovementElement.HasGuidByte0 and <= LegacyMovementElement.HasGuidByte7:
                    mask[element - LegacyMovementElement.HasGuidByte0] = data.HasBit();
                    break;
                case >= LegacyMovementElement.GuidByte0 and <= LegacyMovementElement.GuidByte7:
                    MaskedGuid.ReadByte(data, mask, guid, element - LegacyMovementElement.GuidByte0);
                    break;
                case LegacyMovementElement.HasMovementFlags: hasFlags = !data.HasBit(); break;
                case LegacyMovementElement.HasMovementFlags2: hasFlags2 = !data.HasBit(); break;
                case LegacyMovementElement.HasTimestamp: hasTime = !data.HasBit(); break;
                case LegacyMovementElement.HasOrientation: hasOrientation = !data.HasBit(); break;
                case LegacyMovementElement.HasTransportData: hasTransport = data.HasBit(); break;
                case LegacyMovementElement.HasPitch: hasPitch = !data.HasBit(); break;
                case LegacyMovementElement.HasFallData: hasFall = data.HasBit(); break;
                case LegacyMovementElement.HasFallDirection: if (hasFall) hasFallDirection = data.HasBit(); break;
                case LegacyMovementElement.HasSplineElevation: hasSplineElevation = !data.HasBit(); break;
                case LegacyMovementElement.HasSpline:
                case LegacyMovementElement.HasHeightChangeFailed:
                case LegacyMovementElement.ZeroBit:
                case LegacyMovementElement.OneBit:
                    data.HasBit();
                    break;
                case LegacyMovementElement.MovementFlags: if (hasFlags) flags = data.ReadBits<uint>(30); break;
                case LegacyMovementElement.MovementFlags2: if (hasFlags2) data.ReadBits<uint>(12); break;
                case LegacyMovementElement.Timestamp: if (hasTime) time = data.ReadUInt32(); break;
                case LegacyMovementElement.PositionX: x = data.ReadFloat(); break;
                case LegacyMovementElement.PositionY: y = data.ReadFloat(); break;
                case LegacyMovementElement.PositionZ: z = data.ReadFloat(); break;
                case LegacyMovementElement.Orientation: if (hasOrientation) o = data.ReadFloat(); break;
                case LegacyMovementElement.Pitch: if (hasPitch) data.ReadFloat(); break;
                case LegacyMovementElement.FallTime: if (hasFall) fallTime = data.ReadUInt32(); break;
                case LegacyMovementElement.FallVerticalSpeed: if (hasFall) vertical = data.ReadFloat(); break;
                case LegacyMovementElement.FallCosAngle:
                case LegacyMovementElement.FallSinAngle:
                    if (hasFall && hasFallDirection) data.ReadFloat();
                    break;
                case LegacyMovementElement.FallHorizontalSpeed: if (hasFall && hasFallDirection) horizontal = data.ReadFloat(); break;
                case LegacyMovementElement.SplineElevation: if (hasSplineElevation) data.ReadFloat(); break;
                case LegacyMovementElement.Counter: data.ReadUInt32(); break;
                case LegacyMovementElement.FlushBits: data.ResetBitPos(); break;
                default:
                    Assert.False(hasTransport, $"transport element {element} with transport data");
                    break;
            }
        }

        return new Read(MaskedGuid.ToUInt64(guid), flags, time, new Vector3(x, y, z), o, fallTime, vertical, horizontal);
    }
}
