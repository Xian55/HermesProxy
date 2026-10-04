using HermesProxy.World.Enums;

namespace HermesProxy.World.Objects;

/// <summary>
/// One movement block: where a mover is, how it is moving and what it is riding.
/// </summary>
/// <remarks>
/// <para>
/// Data only. <see cref="LegacyMovementCodec"/> and <see cref="ModernMovementCodec"/> read and
/// write it, and <see cref="MovementSanitizer"/> repairs what a legacy server relays. What a
/// create carries beside its movement block (speeds, rotation, vehicle, path timer) lives on
/// <c>CreateObjectData</c>.
/// </para>
/// <para>
/// A value type with settable members, so a codec writes each field into its destination as it
/// reads it. Building one beside the destination and copying it in measured 7 to 8 ns more on a
/// 60 to 85 ns packet (<c>MovementTranslationBenchmarks</c>), which is why the members are not
/// init-only. Being a value, a change made through one variable never shows through another.
/// </para>
/// <para>
/// 136 bytes, so pass it by <see langword="in"/> and take it from a codec by
/// <see langword="out"/>.
/// </para>
/// </remarks>
public record struct MovementInfo
{
    public MovementFlagModern Flags { get; set; }

    /// <summary>
    /// Passed through untranslated in both directions, so its meaning depends on where the block
    /// came from: a legacy server's <see cref="MovementFlagExtra"/>, or the modern client's own
    /// second flag word. The two vocabularies are not the same, which is why a 3.4.3 client is
    /// never sent a legacy server's (<c>IModernMovementLayout.WritesExtraFlagsAsZero</c>).
    /// </summary>
    public uint FlagsExtra { get; set; }
    public uint FlagsExtra2 { get; set; }
    public uint MoveTime { get; set; }
    public Vector3 Position { get; set; }
    public float Orientation { get; set; }
    public float SwimPitch { get; set; }
    public float SplineElevation { get; set; }

    /// <summary>The HasSpline header bit of the modern block. Set by a create that carries a spline.</summary>
    public bool HasSplineData { get; set; }

    public uint FallTime { get; set; }
    public float JumpVerticalSpeed { get; set; }
    public float JumpSinAngle { get; set; }
    public float JumpCosAngle { get; set; }
    public float JumpHorizontalSpeed { get; set; }

    /// <summary>
    /// Null when the block had no transport part. A transport part with an empty guid is kept as
    /// read, but nothing is written for it: writers go by <see cref="TransportGuid"/>.
    /// </summary>
    public TransportInfo? Transport { get; set; }

    /// <summary>
    /// V3_4_3 only: the GameObject the client reports standing on, sent alongside -- and
    /// independently of -- the transport block. Read for diagnostics; nothing on the legacy
    /// wire carries it.
    /// </summary>
    public WowGuid128 StandingOnGameObjectGuid { get; set; }

    /// <summary>What the mover is riding, or empty.</summary>
    // readonly: without it, a call through an `in` parameter copies the whole block first.
    public readonly WowGuid128 TransportGuid => Transport.GetValueOrDefault().Guid;
}

/// <summary>
/// The transport part of a movement block: what the mover rides and where on it.
/// </summary>
public record struct TransportInfo
{
    // A struct's field initializers only run through a constructor, so this is what makes
    // `new TransportInfo { ... }` start from Seat = -1 rather than 0.
    public TransportInfo()
    {
    }

    public WowGuid128 Guid { get; set; }
    public Vector3 Offset { get; set; }
    public float Orientation { get; set; }
    public uint Time { get; set; }

    /// <summary>The legacy block's second time, behind the InterpolateMove extra flag.</summary>
    public uint PrevTime { get; set; }

    /// <summary>
    /// -1 for no seat, which is also what a block from an era without seats (before 3.0.2) gets.
    /// </summary>
    public sbyte Seat { get; set; } = -1;

    /// <summary>VehicleRecID: the vehicle being ridden, not the mover's own.</summary>
    public uint VehicleId { get; set; }
}
