namespace HermesProxy.World.Objects;

/// <summary>
/// A legacy area trigger the modern client no longer knows about, which the proxy fires on the
/// player's behalf once their position falls inside it.
/// </summary>
/// <remarks>
/// <see cref="RadiusSquared"/> is precomputed so the per-movement-packet check never takes a square
/// root. Positions and radius come from the 3.3.5a <c>AreaTrigger.dbc</c>, because the legacy server
/// re-validates the player's distance against that same row before it will act on the trigger.
/// </remarks>
public readonly record struct ProximityAreaTrigger(
    uint LegacyId,
    uint MapId,
    float X,
    float Y,
    float Z,
    float RadiusSquared);
