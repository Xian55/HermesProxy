using System.Runtime.CompilerServices;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Client;

/// <summary>
/// Legacy → modern spline flag predicates, shared by <c>SMSG_ON_MONSTER_MOVE</c> and the
/// CreateObject spline block so the two paths cannot drift apart.
/// </summary>
/// <remarks>
/// <para>
/// <b>Smooth path.</b> Every pre-3.4 client treats a bare <c>Flying</c> flag as "interpolate this
/// path": AzerothCore <c>MoveSplineFlag.h</c> has <c>Mask_CatmullRom = Flying | Catmullrom</c> and
/// <c>isSmooth()</c> tests that mask. The 3.4.3 client dropped the alias — TrinityCore
/// wotlk_classic <c>MoveSplineFlag.h</c> <c>isSmooth()</c> tests <c>Catmullrom</c> alone — so a
/// legacy flying path reaches the modern client as a linear one unless <c>CatmullRom</c> is added
/// on translation. Native 3.4.3 sets Fly + Smooth + Uncompressed on its own taxi spline
/// (<c>FlightPathMovementGenerator::DoReset</c>), which is what the translation reproduces.
/// </para>
/// <para>
/// <b>Server flight.</b> Taxi starts used to be recognised by an exact <c>WalkMode | Flying</c>
/// match. That only holds when the flight happens to start from a walking player:
/// <c>MoveSplineInit</c>'s constructor mixes the unit's current walk state into the spline, and
/// neither AzerothCore's nor cMaNGOS's <c>FlightPathMovementGenerator</c> sets <c>WalkMode</c> or
/// <c>Catmullrom</c> explicitly — both call <c>SetFly()</c> and <c>SetVelocity()</c> and nothing
/// else. On AzerothCore the taxi spline is therefore <c>Flying | UncompressedPath</c>, the exact
/// match never fired, and the flight ran without the proxy ever noticing it had started (#301).
/// </para>
/// </remarks>
internal static class SplineFlagTranslation
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsSmoothPath(SplineFlagVanilla flags) =>
        (flags & SplineFlagVanilla.Flying) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsSmoothPath(SplineFlagTBC flags) =>
        (flags & SplineFlagTBC.Flying) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsSmoothPath(SplineFlagWotLK flags) =>
        (flags & (SplineFlagWotLK.Flying | SplineFlagWotLK.CatmullRom)) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsServerFlight(SplineFlagVanilla flags) =>
        (flags & SplineFlagVanilla.Flying) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsServerFlight(SplineFlagTBC flags) =>
        (flags & SplineFlagTBC.Flying) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsServerFlight(SplineFlagWotLK flags) =>
        (flags & SplineFlagWotLK.Flying) != 0;
}
