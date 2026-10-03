namespace HermesProxy.World.Objects;

/// <summary>
/// What the proxy remembers about a type 11 GAMEOBJECT_TYPE_TRANSPORT with a stop frame,
/// whose parking and sailing it translates for the modern client. See
/// <c>GameSessionData.SynthesizedTransports</c>.
/// </summary>
/// <param name="StopFrame">
/// The single stop frame emitted as <c>PauseTimes[0]</c> -- the path progress, in ms, of the
/// far end of the route. Comes from the legacy GAMEOBJECT_LEVEL (gameobject_template.data[0]).
/// It is also how long the sail takes: the client interpolates path progress in real time,
/// so a boat parked at progress 0 needs exactly <c>StopFrame</c> ms to reach it.
/// </param>
/// <param name="Position">Stationary position from the create -- where the deck is while parked.</param>
/// <param name="Orientation">Facing from the create, needed to turn a world position into a deck offset.</param>
/// <param name="SailDeadline">
/// The GameObjectData.Level written on the last state flip: the proxy-clock time at which the
/// client parks the transport at <paramref name="SailTargetState"/>'s stop position. Zero when
/// no sail has been scheduled. Republished on repeated creates and state deltas, even after
/// the deadline passes, so neither TrinityCore visibility re-creates nor AzerothCore's
/// per-tick updates restart a crossing.
/// </param>
/// <param name="SailTargetState">The modern GO state that deadline was scheduled for.</param>
public readonly record struct SynthesizedTransport(
    uint StopFrame,
    Vector3 Position,
    float Orientation,
    uint SailDeadline = 0,
    sbyte SailTargetState = 0)
{
    /// <summary>
    /// True while a scheduled sail has not yet reached its deadline. Signed comparison so a
    /// deadline just past reads as expired rather than as 49 days away.
    /// </summary>
    public bool IsSailing(uint now) => SailDeadline != 0 && (int)(SailDeadline - now) > 0;
}
