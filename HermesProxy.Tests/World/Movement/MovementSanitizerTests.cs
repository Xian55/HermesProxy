using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using Xunit;

namespace HermesProxy.Tests.World.Movement;

/// <summary>
/// The repairs applied to a legacy server's movement state before a modern client sees it:
/// orientations brought into [0, 2pi] and flag combinations the client cannot hold removed.
/// </summary>
public class MovementSanitizerTests
{
    private const MovementFlagModern F = MovementFlagModern.Forward;
    private const MovementFlagModern B = MovementFlagModern.Backward;

    // ---- the production side ------------------------------------------------------------

    private static MovementFlagModern Sanitize(MovementFlagModern flags, float splineElevation)
        => MovementSanitizer.SanitizeFlags(flags, flagsExtra: 0, splineElevation);

    private static float Clamp(float orientation)
    {
        MovementSanitizer.ClampOrientation(ref orientation);
        return orientation;
    }

    // ---- flags ----------------------------------------------------------------------------

    [Theory]
    // A rooted unit that also reports movement freezes the clients that receive it.
    [InlineData(MovementFlagModern.Root | F, MovementFlagModern.Root)]
    [InlineData(MovementFlagModern.Root | MovementFlagModern.Falling | MovementFlagModern.StrafeLeft, MovementFlagModern.Root)]
    [InlineData(MovementFlagModern.Root | MovementFlagModern.Ascending | MovementFlagModern.Descending, MovementFlagModern.Root)]
    [InlineData(MovementFlagModern.Root | MovementFlagModern.TurnLeft, MovementFlagModern.Root | MovementFlagModern.TurnLeft)]
    [InlineData(MovementFlagModern.Root, MovementFlagModern.Root)]
    // Opposing directions cancel out entirely rather than one of them winning.
    [InlineData(F | B, MovementFlagModern.None)]
    [InlineData(F | B | MovementFlagModern.WalkMode, MovementFlagModern.WalkMode)]
    [InlineData(MovementFlagModern.StrafeLeft | MovementFlagModern.StrafeRight, MovementFlagModern.None)]
    [InlineData(MovementFlagModern.TurnLeft | MovementFlagModern.TurnRight, MovementFlagModern.None)]
    [InlineData(MovementFlagModern.PitchUp | MovementFlagModern.PitchDown, MovementFlagModern.None)]
    [InlineData(MovementFlagModern.Ascending | MovementFlagModern.Descending, MovementFlagModern.None)]
    [InlineData(F | MovementFlagModern.StrafeLeft | MovementFlagModern.TurnRight | MovementFlagModern.PitchUp | MovementFlagModern.Ascending,
        F | MovementFlagModern.StrafeLeft | MovementFlagModern.TurnRight | MovementFlagModern.PitchUp | MovementFlagModern.Ascending)]
    // Nothing that can fly or ignores gravity is falling.
    [InlineData(MovementFlagModern.CanFly | MovementFlagModern.Falling, MovementFlagModern.CanFly)]
    [InlineData(MovementFlagModern.DisableGravity | MovementFlagModern.Falling, MovementFlagModern.DisableGravity)]
    [InlineData(MovementFlagModern.Falling, MovementFlagModern.Falling)]
    [InlineData(MovementFlagModern.CanFly | MovementFlagModern.FallingFar, MovementFlagModern.CanFly | MovementFlagModern.FallingFar)]
    public void Sanitize_RemovesFlagsTheClientCannotHoldTogether(MovementFlagModern flags, MovementFlagModern expected)
        => Assert.Equal(expected, Sanitize(flags, splineElevation: 0f));

    [Theory]
    // The client checks the elevation first and the flag second, so the two must agree.
    [InlineData(MovementFlagModern.SplineElevation, 0f, MovementFlagModern.None)]
    [InlineData(MovementFlagModern.SplineElevation, 1e-6f, MovementFlagModern.None)]
    [InlineData(MovementFlagModern.SplineElevation, -1e-6f, MovementFlagModern.None)]
    [InlineData(MovementFlagModern.SplineElevation, 2.25f, MovementFlagModern.SplineElevation)]
    [InlineData(MovementFlagModern.None, 2.25f, MovementFlagModern.SplineElevation)]
    [InlineData(MovementFlagModern.None, -2.25f, MovementFlagModern.SplineElevation)]
    [InlineData(F, 1e-6f, F)]
    public void Sanitize_MakesTheSplineElevationFlagFollowTheValue(MovementFlagModern flags, float elevation, MovementFlagModern expected)
        => Assert.Equal(expected, Sanitize(flags, elevation));

    // ---- orientation ----------------------------------------------------------------------

    [Theory]
    [InlineData(0f)]
    [InlineData(1.5f)]
    [InlineData(6.2831855f)]    // 2pi itself is left alone: only values above it wrap
    public void ClampOrientation_InRange_IsUnchanged(float orientation)
        => Assert.Equal(orientation, Clamp(orientation));

    /// <summary>
    /// Adding or subtracting 2pi no longer changes a float past about 1.3e8, and never changes an
    /// infinity, so wrapping one by repeated subtraction hung the session's thread for good. The
    /// value is the legacy server's to send.
    /// </summary>
    [Theory]
    [InlineData(1.4e8f)]
    [InlineData(-1.4e8f)]
    [InlineData(1e30f)]
    [InlineData(float.MaxValue)]
    [InlineData(float.MinValue)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void ClampOrientation_TooLargeToWrapByStepping_StillEndsInRange(float orientation)
        => Assert.InRange(Clamp(orientation), 0f, 6.2831855f);

    [Fact]
    public void ClampOrientation_NaN_IsLeftAlone()
        => Assert.True(float.IsNaN(Clamp(float.NaN)));

    [Fact]
    public void Sanitize_NaNElevation_LeavesTheFlagAsItWas()
    {
        Assert.Equal(MovementFlagModern.SplineElevation, Sanitize(MovementFlagModern.SplineElevation, float.NaN));
        Assert.Equal(MovementFlagModern.None, Sanitize(MovementFlagModern.None, float.NaN));
    }

    [Theory]
    [InlineData(-1f, 5.2831855f)]
    [InlineData(7.5f, 1.2168145f)]
    [InlineData(-13f, 5.8495560f)]
    [InlineData(20f, 1.1504436f)]
    public void ClampOrientation_OutOfRange_WrapsByWholeTurns(float orientation, float expected)
        => Assert.Equal(expected, Clamp(orientation), precision: 4);

    [Fact]
    public void ClampOrientation_AcrossManyTurns_EndsInRangeAWholeNumberOfTurnsAway()
    {
        for (float orientation = -1000f; orientation <= 1000f; orientation += 0.173f)
        {
            float clamped = Clamp(orientation);

            Assert.InRange(clamped, 0f, 6.2831855f);
            double turns = (clamped - orientation) / (2 * System.Math.PI);
            Assert.Equal(System.Math.Round(turns), turns, precision: 3);
        }
    }
}
