using HermesProxy.Tests.Support;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// The spline of a 4.3.4 create, for a unit first seen in mid-leap. TrinityCore 4.3.4 sends the
/// leap's gravity, but only 25 bits of spline flags, and Parabolic is the 26th: the gravity is
/// what tells a leap. The modern client draws the arc from the Parabolic flag and the spline's
/// jump data, which have to come together.
/// </summary>
public sealed class CataCreateSplineTests
{
    private static readonly Vector3[] Points =
    [
        new(-9456.18f, 73.75f, 56.77f),
        new(-9455.43f, 73.23f, 56.77f),
        new(-9465.52f, 74.01f, 56.78f),
        new(-9465.52f, 74.01f, 56.78f),
    ];

    private static readonly Vector3 End = new(-9465.52f, 74.01f, 56.78f);

    // MovementPacketBuilder::WriteCreateData of TrinityCore 4.3.4, after the bits: points z, x, y
    private static ServerSideMovement Read(float? gravity, uint? effectStartTime)
    {
        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_UPDATE_OBJECT, p =>
        {
            if (gravity is { } verticalAcceleration)
                p.WriteFloat(verticalAcceleration);
            p.WriteUInt32(191);             // time passed
            foreach (var point in Points)
            {
                p.WriteFloat(point.Z);
                p.WriteFloat(point.X);
                p.WriteFloat(point.Y);
            }
            p.WriteFloat(1f);               // duration multiplier next
            p.WriteUInt32(481);             // duration
            if (effectStartTime is { } start)
                p.WriteUInt32(start);
            p.WriteFloat(1f);               // duration multiplier
            p.WriteFloat(End.Z);
            p.WriteFloat(End.X);
            p.WriteFloat(End.Y);
            p.WriteUInt32(3170);            // spline id
        });

        var harness = new LegacyHandlerHarness(recordClientPackets: false);
        using var packet = LegacyPacketBuilder.Receive(wire);
        return harness.Client.ReadCreateSplineCata(packet, hasSplineMove: true, hasVerticalAcceleration: gravity != null,
            hasEffectStartTime: effectStartTime != null, splineMode: 0, pointCount: (uint)Points.Length, facing: 3, flags: 0,
            facingMask: []);
    }

    [Fact]
    public void ALeap_GoesOnAsAnArcWithItsJumpData()
    {
        ServerSideMovement spline = Read(gravity: 172.89f, effectStartTime: 120);

        Assert.True(spline.SplineFlags.HasFlag(SplineFlagModern.Parabolic));
        Assert.Equal(172.89f, spline.JumpGravity);
        Assert.Equal(120u, spline.JumpStartTime);
        Assert.Equal(191u, spline.SplineTime);
        Assert.Equal(481u, spline.SplineTimeFull);
        Assert.Equal(Points, spline.SplinePoints);
        Assert.Equal(End, spline.EndPosition);
        Assert.Equal(3170u, spline.SplineId);
    }

    [Fact]
    public void AnAnimatedMove_WithoutAGravity_HasNeitherTheFlagNorJumpData()
    {
        // TrinityCore sends the effect start time for an Animation spline too
        ServerSideMovement spline = Read(gravity: null, effectStartTime: 120);

        Assert.False(spline.SplineFlags.HasFlag(SplineFlagModern.Parabolic));
        Assert.Null(spline.JumpGravity);
        Assert.Equal(481u, spline.SplineTimeFull);
        Assert.Equal(3170u, spline.SplineId);
    }
}
