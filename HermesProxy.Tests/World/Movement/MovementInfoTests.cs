using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Movement;

/// <summary>
/// What <see cref="MovementInfo"/> is as a value: its defaults, and the places where "absent" used
/// to be spelled as a null reference or a field initializer and now has to be spelled otherwise.
/// </summary>
public class MovementInfoTests
{
    [Fact]
    public void Default_IsABlockWithNoTransport()
    {
        MovementInfo info = default;

        Assert.Null(info.Transport);
        Assert.Equal(default, info.TransportGuid);
        Assert.Equal(MovementFlagModern.None, info.Flags);
        Assert.Equal(new MovementInfo(), info);
    }

    /// <summary>
    /// Seat 0 is a real seat. A struct's default cannot be -1, so "no seat" is only reachable
    /// through the constructor, and "no transport" only as a null transport part.
    /// </summary>
    [Fact]
    public void TransportInfo_ConstructedStartsWithNoSeat_DefaultDoesNot()
    {
        Assert.Equal(-1, new TransportInfo().Seat);
        Assert.Equal(-1, new TransportInfo { Guid = new WowGuid128(1, 2) }.Seat);
        Assert.Equal(0, default(TransportInfo).Seat);
    }

    [Fact]
    public void TransportGuid_FollowsTheTransportPart()
    {
        var guid = new WowGuid128(0xABCDEF, 0x123456);
        var info = new MovementInfo { Transport = new TransportInfo { Guid = guid } };

        Assert.Equal(guid, info.TransportGuid);
        Assert.Equal(default, (info with { Transport = null }).TransportGuid);
    }

    [Fact]
    public void Equality_IsByValue_IncludingTheTransportPart()
    {
        var a = new MovementInfo { MoveTime = 7, Transport = new TransportInfo { Seat = 2 } };
        var b = new MovementInfo { MoveTime = 7, Transport = new TransportInfo { Seat = 2 } };

        Assert.Equal(a, b);
        Assert.NotEqual(a, b with { Transport = new TransportInfo { Seat = 3 } });
        Assert.NotEqual(a, b with { Transport = null });
    }

    /// <summary>
    /// A create with no position has no movement block at all, and half the create path gates on
    /// that. A value type's default would read as "has a block at the origin".
    /// </summary>
    [Fact]
    public void CreateObjectData_StartsWithNoMovementBlockAndAnIdentityRotation()
    {
        var create = new CreateObjectData();

        Assert.Null(create.MoveInfo);
        // The zero quaternion is not a rotation, and a 3.4.3 client rejects a create carrying it.
        Assert.Equal(Quaternion.Identity, create.Rotation);
        Assert.False(create.PlayHoverAnim);
        Assert.Equal(0u, create.VehicleId);
    }

    [Fact]
    public void Sanitize_RepairsFlagsAndBothOrientations_AndNothingElse()
    {
        var info = new MovementInfo
        {
            Flags = MovementFlagModern.Root | MovementFlagModern.Forward,
            Orientation = -1f,
            Transport = new TransportInfo { Guid = new WowGuid128(1, 2), Orientation = 20f, Seat = 4 },
            MoveTime = 99,
        };
        var before = info;

        MovementSanitizer.Sanitize(ref info);

        Assert.Equal(MovementFlagModern.Root, info.Flags);
        Assert.InRange(info.Orientation, 0f, 6.2831855f);
        Assert.InRange(info.Transport!.Value.Orientation, 0f, 6.2831855f);
        Assert.Equal(before with { Flags = info.Flags, Orientation = info.Orientation, Transport = info.Transport }, info);
        Assert.Equal(before.Transport!.Value with { Orientation = info.Transport.Value.Orientation }, info.Transport.Value);
    }

    /// <summary>Each of the three repairs on its own, so none of them rides on another's write.</summary>
    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public void Sanitize_AppliesEachRepairIndependently(bool badFlags, bool badOrientation, bool badTransportOrientation)
    {
        var info = new MovementInfo
        {
            Flags = badFlags ? MovementFlagModern.Forward | MovementFlagModern.Backward : MovementFlagModern.Forward,
            Orientation = badOrientation ? -1f : 1f,
            Transport = new TransportInfo { Guid = new WowGuid128(1, 2), Orientation = badTransportOrientation ? 20f : 2f },
        };

        MovementSanitizer.Sanitize(ref info);

        Assert.Equal(badFlags ? MovementFlagModern.None : MovementFlagModern.Forward, info.Flags);
        Assert.Equal(badOrientation ? 5.2831855f : 1f, info.Orientation, precision: 4);
        Assert.Equal(badTransportOrientation ? 1.1504436f : 2f, info.Transport!.Value.Orientation, precision: 4);
    }

    [Fact]
    public void Sanitize_ABlockThatNeedsNoRepair_IsUnchanged()
    {
        var info = new MovementInfo
        {
            Flags = MovementFlagModern.Forward | MovementFlagModern.Swimming,
            Orientation = 1.5f,
            SwimPitch = float.NaN,
            Transport = new TransportInfo { Guid = new WowGuid128(1, 2), Orientation = 0.75f, Seat = 2 },
        };
        var before = info;

        MovementSanitizer.Sanitize(ref info);

        Assert.Equal(before, info);
    }

    [Fact]
    public void Sanitize_WithoutATransportPart_DoesNotInventOne()
    {
        var info = new MovementInfo { Orientation = 7.5f };

        MovementSanitizer.Sanitize(ref info);

        Assert.Null(info.Transport);
        Assert.InRange(info.Orientation, 0f, 6.2831855f);
    }

    // ---- read in place --------------------------------------------------------------------
    //
    // The codecs write each field into the destination as they read it. A destination that
    // already holds a block (a reused local, a packet being refilled) must come out exactly as a
    // fresh one would: anything the new block does not carry is cleared, not inherited.

    private static readonly MovementInfo Leftover = new()
    {
        Flags = MovementFlagModern.Swimming | MovementFlagModern.Falling,
        FlagsExtra = 0xFFFF,
        FlagsExtra2 = 0xFFFF,
        SwimPitch = 9f,
        SplineElevation = 9f,
        HasSplineData = true,
        FallTime = 999,
        JumpVerticalSpeed = 9f,
        JumpSinAngle = 9f,
        JumpCosAngle = 9f,
        JumpHorizontalSpeed = 9f,
        Transport = new TransportInfo { Guid = new WowGuid128(7, 8), Seat = 5, VehicleId = 9 },
        StandingOnGameObjectGuid = new WowGuid128(5, 6),
    };

    [Fact]
    public void LegacyRead_IntoAUsedDestination_LeavesNothingBehind()
    {
        var gameState = new LegacyHandlerHarness(recordClientPackets: false).Session.GameState;
        using var wire = new WorldPacket();
        LegacyMovementWire.Write(wire, MovementScenarios.Forward);
        byte[] bytes = wire.GetDataSpan().ToArray();

        using var freshPacket = new WorldPacket(1, bytes);
        LegacyMovementCodec.Read(freshPacket, gameState, out MovementInfo fresh, out _);

        MovementInfo reused = Leftover;
        using var reusedPacket = new WorldPacket(1, bytes);
        LegacyMovementCodec.Read(reusedPacket, gameState, out reused, out _);

        Assert.Equal(fresh, reused);
        Assert.Null(reused.Transport);
        Assert.Equal(MovementFlagModern.Forward, reused.Flags);
    }

    [Fact]
    public void ModernRead_IntoAUsedDestination_LeavesNothingBehind()
    {
        using var wire = new WorldPacket();
        ModernMovementWire.Write(wire, MovementScenarios.ClientForward);
        byte[] bytes = wire.GetDataSpan().ToArray();

        var freshReader = new SpanPacketReader(bytes);
        ModernMovementCodec.Read(ref freshReader, out MovementInfo fresh);

        MovementInfo reused = Leftover;
        var reusedReader = new SpanPacketReader(bytes);
        ModernMovementCodec.Read(ref reusedReader, out reused);

        Assert.Equal(fresh, reused);
        Assert.Null(reused.Transport);
        Assert.Equal(default, reused.StandingOnGameObjectGuid);
        Assert.Equal(0u, reused.FallTime);
    }

    /// <summary>
    /// A member that is not readonly, called through an <see langword="in"/> parameter, makes the
    /// compiler copy all 136 bytes first. Auto-property getters are readonly by themselves; this
    /// is the one computed member, and it sits on both write paths.
    /// </summary>
    [Fact]
    public void TransportGuid_IsReadonly_SoAnInParameterIsNotCopiedToCallIt()
    {
        var getter = typeof(MovementInfo).GetProperty(nameof(MovementInfo.TransportGuid))!.GetMethod!;
        Assert.Contains(getter.CustomAttributes, a => a.AttributeType.Name == "IsReadOnlyAttribute");
    }

    /// <summary>
    /// It is passed by <see langword="in"/> and held inline in packet structs on that basis. If it
    /// grows well past this, a copy stops being cheap and the by-value sites want another look.
    /// </summary>
    [Fact]
    public void Size_StaysSmallEnoughToHoldInline()
        => Assert.InRange(Unsafe.SizeOf<MovementInfo>(), 1, 160);
}
