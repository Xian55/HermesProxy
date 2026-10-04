using System;
using Framework.Logging;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;

namespace HermesProxy.World.Objects;

/// <summary>
/// Repairs a legacy server's movement state before a modern client sees it.
/// </summary>
/// <remarks>
/// The rules are the ones a modern server applies to movement it receives from a client
/// (TrinityCore <c>WorldSession::ValidateMovementInfo</c>). The proxy stands where that server
/// would, so it applies them to what the legacy server relays: a 3.3.5a core forwards another
/// player's flags unchecked, and a modern client that is handed a rooted unit that also reports
/// movement freezes.
/// </remarks>
public static class MovementSanitizer
{
    private static readonly Microsoft.Extensions.Logging.ILogger _log = Log.CreateMelLogger(Log.CategoryServer);

    private const float TwoPi = (float)(Math.PI * 2f);

    // Past this the wrap is done by remainder. Repeated subtraction is what every in-range value
    // has always been wrapped by, and its result differs from the remainder in the last bit, so it
    // stays for those; no real orientation is anywhere near this many turns out.
    private const float WrapByRemainderAbove = 1000f;

    /// <summary>Brings an orientation into [0, 2pi].</summary>
    public static void ClampOrientation(ref float orientation)
    {
        // A float past ~1.3e8 is unchanged by adding or subtracting 2pi, and an infinity always
        // is, so the loops below would never end on one. NaN fails both comparisons and passes
        // through untouched, as it always has.
        if (MathF.Abs(orientation) > WrapByRemainderAbove)
            orientation = float.IsInfinity(orientation) ? 0f : orientation % TwoPi;

        while (orientation < 0)
            orientation += TwoPi;
        while (orientation > TwoPi)
            orientation -= TwoPi;
    }

    /// <summary>
    /// Removes the flag combinations a modern client cannot hold together. Each rule sees the
    /// result of the ones before it.
    /// </summary>
    /// <param name="flagsExtra">Only reported when a rule fires.</param>
    public static MovementFlagModern SanitizeFlags(MovementFlagModern flags, uint flagsExtra, float splineElevation)
    {
        // A rooted unit that also reports movement freezes the clients that receive it.
        if (flags.HasAnyFlag(MovementFlagModern.Root) && flags.HasAnyFlag(MovementFlagModern.MaskMoving))
            Remove(ref flags, flagsExtra, MovementFlagModern.MaskMoving);

        if (flags.HasAnyFlag(MovementFlagModern.Ascending) && flags.HasAnyFlag(MovementFlagModern.Descending))
            Remove(ref flags, flagsExtra, MovementFlagModern.Ascending | MovementFlagModern.Descending);

        if (flags.HasAnyFlag(MovementFlagModern.TurnLeft) && flags.HasAnyFlag(MovementFlagModern.TurnRight))
            Remove(ref flags, flagsExtra, MovementFlagModern.TurnLeft | MovementFlagModern.TurnRight);

        if (flags.HasAnyFlag(MovementFlagModern.StrafeLeft) && flags.HasAnyFlag(MovementFlagModern.StrafeRight))
            Remove(ref flags, flagsExtra, MovementFlagModern.StrafeLeft | MovementFlagModern.StrafeRight);

        if (flags.HasAnyFlag(MovementFlagModern.PitchUp) && flags.HasAnyFlag(MovementFlagModern.PitchDown))
            Remove(ref flags, flagsExtra, MovementFlagModern.PitchUp | MovementFlagModern.PitchDown);

        if (flags.HasAnyFlag(MovementFlagModern.Forward) && flags.HasAnyFlag(MovementFlagModern.Backward))
            Remove(ref flags, flagsExtra, MovementFlagModern.Forward | MovementFlagModern.Backward);

        if (flags.HasAnyFlag(MovementFlagModern.DisableGravity | MovementFlagModern.CanFly) && flags.HasAnyFlag(MovementFlagModern.Falling))
            Remove(ref flags, flagsExtra, MovementFlagModern.Falling);

        // The client checks the elevation first and the flag second, so the two must agree.
        // Two comparisons, not one negated: a NaN elevation fails both and leaves the flag alone.
        if (flags.HasAnyFlag(MovementFlagModern.SplineElevation) && MathF.Abs(splineElevation) <= 1e-5f)
            Remove(ref flags, flagsExtra, MovementFlagModern.SplineElevation);
        if (MathF.Abs(splineElevation) > 1e-5f)
            flags |= MovementFlagModern.SplineElevation;

        return flags;
    }

    /// <summary>Clamps both orientations and repairs the flags, in place.</summary>
    public static void Sanitize(ref MovementInfo info)
    {
        float orientation = info.Orientation;
        ClampOrientation(ref orientation);
        info.Orientation = orientation;
        info.Flags = SanitizeFlags(info.Flags, info.FlagsExtra, info.SplineElevation);

        if (info.Transport is { } ridden)
        {
            float transportOrientation = ridden.Orientation;
            ClampOrientation(ref transportOrientation);
            // The transport part is stored back whole, so only when the clamp moved something.
            // Bits, not values: a NaN compares unequal to itself.
            if (BitConverter.SingleToUInt32Bits(transportOrientation) != BitConverter.SingleToUInt32Bits(ridden.Orientation))
            {
                ridden.Orientation = transportOrientation;
                info.Transport = ridden;
            }
        }
    }

    private static void Remove(ref MovementFlagModern flags, uint flagsExtra, MovementFlagModern mask)
    {
        MovementLogMessages.ViolatingFlagsRemoved(_log, (uint)flags, flagsExtra, mask);
        flags &= ~mask;
    }
}
