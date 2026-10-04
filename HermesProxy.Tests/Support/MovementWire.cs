using System;
using Framework.GameMath;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;

namespace HermesProxy.Tests.Support;

// Movement blocks written straight from the wire layout, sharing no code with the production
// readers and writers they are fed to. That independence is the point: a fixture built with the
// production writer agrees with the production reader by construction.
//
// Each encoder picks its layout from the process-wide LegacyVersion / ModernVersion, so the same
// sample comes out in whatever layout the run is pinned to.

internal enum LegacyEra { Vanilla, Tbc, WotLK }

/// <summary>A movement state as a legacy server reports it, in the WotLK flag vocabulary.</summary>
internal sealed record LegacyMove
{
    /// <summary>Translated by name for the older eras, which is how the proxy normalises them.</summary>
    public MovementFlagWotLK Flags { get; init; }

    /// <summary>ORed in untranslated, for bits only one era has (TBC Flying2, vanilla FixedZ).</summary>
    public uint RawEraFlags { get; init; }

    public ushort ExtraFlags { get; init; }
    public uint Time { get; init; } = 123456;
    public Vector3 Position { get; init; } = new(1199.4f, 1472.1f, 307.5f);
    public float Orientation { get; init; } = 1.5f;
    public LegacyTransport? Transport { get; init; }
    public float Pitch { get; init; }
    public uint FallTime { get; init; }
    public float JumpVerticalSpeed { get; init; }
    public float JumpSin { get; init; }
    public float JumpCos { get; init; }
    public float JumpHorizontalSpeed { get; init; }
    public float SplineElevation { get; init; }
}

internal sealed record LegacyTransport(WowGuid64 Guid, Vector3 Offset, float Orientation, uint Time, sbyte Seat, uint Time2);

internal static class LegacyMovementWire
{
    public static LegacyEra Era =>
        LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056) ? LegacyEra.WotLK :
        LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) ? LegacyEra.Tbc :
        LegacyEra.Vanilla;

    public static void Write(WorldPacket packet, LegacyMove move)
    {
        MovementFlagWotLK flags = move.Flags;
        if (move.Transport != null)
            flags |= MovementFlagWotLK.OnTransport;

        ushort extraFlags;
        bool hasPitch;
        switch (Era)
        {
            case LegacyEra.WotLK:
            {
                uint wire = (uint)flags | move.RawEraFlags;
                extraFlags = move.ExtraFlags;
                packet.WriteUInt32(wire);
                packet.WriteUInt16(extraFlags);
                hasPitch = (wire & (uint)(MovementFlagWotLK.Swimming | MovementFlagWotLK.Flying)) != 0
                           || (extraFlags & (ushort)MovementFlagExtra.AlwaysAllowPitching) != 0;
                break;
            }
            case LegacyEra.Tbc:
            {
                uint wire = (uint)flags.CastFlags<MovementFlagWotLK, MovementFlagTBC>() | move.RawEraFlags;
                extraFlags = (byte)move.ExtraFlags;
                packet.WriteUInt32(wire);
                packet.WriteUInt8((byte)extraFlags);
                hasPitch = (wire & (uint)(MovementFlagTBC.Swimming | MovementFlagTBC.Flying2)) != 0;
                break;
            }
            default:
            {
                uint wire = (uint)flags.CastFlags<MovementFlagWotLK, MovementFlagVanilla>() | move.RawEraFlags;
                extraFlags = 0;
                packet.WriteUInt32(wire);
                hasPitch = (wire & (uint)MovementFlagVanilla.Swimming) != 0;
                break;
            }
        }

        packet.WriteUInt32(move.Time);
        packet.WriteVector3(move.Position);
        packet.WriteFloat(move.Orientation);

        if (move.Transport is { } transport)
        {
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                packet.WritePackedGuid(transport.Guid);
            else
                packet.WriteGuid(transport.Guid);

            packet.WriteVector3(transport.Offset);
            packet.WriteFloat(transport.Orientation);

            if (Era != LegacyEra.Vanilla)
                packet.WriteUInt32(transport.Time);
            if (Era == LegacyEra.WotLK)
                packet.WriteInt8(transport.Seat);
            if ((extraFlags & (ushort)MovementFlagExtra.InterpolateMove) != 0)
                packet.WriteUInt32(transport.Time2);
        }

        if (hasPitch)
            packet.WriteFloat(move.Pitch);

        packet.WriteUInt32(move.FallTime);
        if (flags.HasFlag(MovementFlagWotLK.Falling))
        {
            packet.WriteFloat(move.JumpVerticalSpeed);
            packet.WriteFloat(move.JumpSin);
            packet.WriteFloat(move.JumpCos);
            packet.WriteFloat(move.JumpHorizontalSpeed);
        }

        if (flags.HasFlag(MovementFlagWotLK.SplineElevation))
            packet.WriteFloat(move.SplineElevation);
    }
}

/// <summary>A movement state as the modern client reports it.</summary>
internal sealed record ModernMove
{
    public MovementFlagModern Flags { get; init; }
    public uint ExtraFlags { get; init; }
    public uint ExtraFlags2 { get; init; }
    public uint Time { get; init; } = 123456;
    public Vector3 Position { get; init; } = new(1199.4f, 1472.1f, 307.5f);
    public float Orientation { get; init; } = 1.5f;
    public float Pitch { get; init; }
    public float SplineElevation { get; init; }
    public WowGuid128[] RemoveForces { get; init; } = [];
    public uint MoveIndex { get; init; }
    public ModernTransport? Transport { get; init; }
    public ModernFall? Fall { get; init; }
    public bool HasSpline { get; init; }
    public bool HeightChangeFailed { get; init; }
    public bool RemoteTimeValid { get; init; }

    /// <summary>Only on the wire for layouts with 32-bit flag words; dropped otherwise.</summary>
    public ModernInertia? Inertia { get; init; }

    /// <summary>Only on the 3.4.3 wire; dropped otherwise.</summary>
    public WowGuid128? StandingOnGameObject { get; init; }

    /// <summary>Only on the 3.4.3 wire; dropped otherwise.</summary>
    public ModernAdvFlying? AdvFlying { get; init; }
}

internal sealed record ModernTransport(WowGuid128 Guid, Vector3 Offset, float Orientation, sbyte Seat, uint Time, uint? PrevTime, uint? VehicleId);
internal sealed record ModernFall(uint Time, float JumpVelocity, ModernFallDirection? Direction);
internal sealed record ModernFallDirection(float Sin, float Cos, float Speed);
internal sealed record ModernInertia(WowGuid128 Guid, Vector3 Force, uint Lifetime);
internal sealed record ModernAdvFlying(float ForwardVelocity, float UpVelocity);

internal static class ModernMovementWire
{
    /// <summary>Flags as three 32-bit words up front, rather than 30 + 18 bits before the header bits.</summary>
    public static bool FlagsAreWords => ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3);

    public static bool IsWotLKClassic => ModernVersion.Build == ClientVersionBuild.V3_4_3_54261;

    /// <summary>The MovementInfo block, without the mover guid that precedes it in most packets.</summary>
    public static void Write(WorldPacket packet, ModernMove move)
    {
        if (FlagsAreWords)
        {
            packet.WriteUInt32((uint)move.Flags);
            packet.WriteUInt32(move.ExtraFlags);
            packet.WriteUInt32(move.ExtraFlags2);
        }

        packet.WriteUInt32(move.Time);
        packet.WriteVector3(move.Position);
        packet.WriteFloat(move.Orientation);
        packet.WriteFloat(move.Pitch);
        packet.WriteFloat(move.SplineElevation);

        packet.WriteUInt32((uint)move.RemoveForces.Length);
        packet.WriteUInt32(move.MoveIndex);
        foreach (var force in move.RemoveForces)
            packet.WritePackedGuid128(force);

        if (!FlagsAreWords)
        {
            packet.WriteBits((uint)move.Flags, 30);
            packet.WriteBits(move.ExtraFlags, 18);
        }

        bool hasStandingOn = IsWotLKClassic && move.StandingOnGameObject != null;
        bool hasInertia = FlagsAreWords && move.Inertia != null;
        bool hasAdvFlying = IsWotLKClassic && move.AdvFlying != null;

        if (IsWotLKClassic)
            packet.WriteBit(hasStandingOn);
        packet.WriteBit(move.Transport != null);
        packet.WriteBit(move.Fall != null);
        packet.WriteBit(move.HasSpline);
        packet.WriteBit(move.HeightChangeFailed);
        packet.WriteBit(move.RemoteTimeValid);
        if (FlagsAreWords)
            packet.WriteBit(hasInertia);
        if (IsWotLKClassic)
            packet.WriteBit(hasAdvFlying);
        packet.FlushBits();

        if (move.Transport is { } transport)
        {
            packet.WritePackedGuid128(transport.Guid);
            packet.WriteVector3(transport.Offset);
            packet.WriteFloat(transport.Orientation);
            packet.WriteInt8(transport.Seat);
            packet.WriteUInt32(transport.Time);
            packet.WriteBit(transport.PrevTime != null);
            packet.WriteBit(transport.VehicleId != null);
            packet.FlushBits();
            if (transport.PrevTime is { } prevTime)
                packet.WriteUInt32(prevTime);
            if (transport.VehicleId is { } vehicleId)
                packet.WriteUInt32(vehicleId);
        }

        if (hasStandingOn)
            packet.WritePackedGuid128(move.StandingOnGameObject!.Value);

        if (hasInertia)
        {
            packet.WritePackedGuid128(move.Inertia!.Guid);
            packet.WriteVector3(move.Inertia.Force);
            packet.WriteUInt32(move.Inertia.Lifetime);
        }

        if (hasAdvFlying)
        {
            packet.WriteFloat(move.AdvFlying!.ForwardVelocity);
            packet.WriteFloat(move.AdvFlying.UpVelocity);
        }

        if (move.Fall is { } fall)
        {
            packet.WriteUInt32(fall.Time);
            packet.WriteFloat(fall.JumpVelocity);
            packet.WriteBit(fall.Direction != null);
            packet.FlushBits();
            if (fall.Direction is { } direction)
            {
                packet.WriteFloat(direction.Sin);
                packet.WriteFloat(direction.Cos);
                packet.WriteFloat(direction.Speed);
            }
        }
    }

    /// <summary>
    /// A client packet body framed the way the dispatch site meets it: the read-mode
    /// <see cref="WorldPacket"/> constructor consumes a two-byte opcode prefix.
    /// </summary>
    public static byte[] Frame(Action<WorldPacket> writeBody)
    {
        using var packet = new WorldPacket();
        packet.WriteUInt16(0);
        writeBody(packet);
        return packet.GetDataSpan().ToArray();
    }
}

/// <summary>The movement block of a legacy create, plus the handful of fields the handlers need.</summary>
internal sealed record LegacyCreate
{
    public required WowGuid64 Guid { get; init; }
    public required ObjectTypeLegacy Type { get; init; }
    public uint Entry { get; init; }
    public UpdateTypeLegacy UpdateType { get; init; } = UpdateTypeLegacy.CreateObject1;
    public bool Self { get; init; }

    public LegacyMove? Living { get; init; }
    public LegacySpeeds Speeds { get; init; } = new();
    public LegacySpline? Spline { get; init; }

    /// <summary>UPDATEFLAG_POSITION, 3.x only: an object riding a transport.</summary>
    public LegacyGoPosition? GoPosition { get; init; }
    public LegacyStationary? Stationary { get; init; }

    public bool LowAndHighGuid { get; init; }
    public WowGuid64? AttackingTarget { get; init; }
    public uint? TransportPathTimer { get; init; }
    public LegacyVehicle? Vehicle { get; init; }
    public Quaternion? Rotation { get; init; }

    /// <summary>Update fields beyond the object header, which is always written.</summary>
    public (int Field, uint Value)[] Fields { get; init; } = [];
}

internal sealed record LegacySpeeds(
    float Walk = 2.5f, float Run = 7f, float RunBack = 4.5f, float Swim = 4.722222f, float SwimBack = 2.5f,
    float Flight = 7f, float FlightBack = 4.5f, float Turn = 3.141594f, float Pitch = 3.141594f);

internal sealed record LegacySpline(bool FinalOrientation, float Orientation, uint Time, uint TimeFull, uint Id, Vector3[] Points, Vector3 EndPoint);
internal sealed record LegacyGoPosition(WowGuid64 Transport, Vector3 Position, Vector3 Offset, float Orientation, float CorpseOrientation);
internal sealed record LegacyStationary(Vector3 Position, float Orientation);
internal sealed record LegacyVehicle(uint Id, float Orientation);

internal static class LegacyCreateWire
{
    private const uint TypeMaskObject = 0x1;

    public static uint TypeMask(ObjectTypeLegacy type) => type switch
    {
        ObjectTypeLegacy.Item => TypeMaskObject | 0x2,
        ObjectTypeLegacy.Unit => TypeMaskObject | 0x8,
        ObjectTypeLegacy.Player => TypeMaskObject | 0x8 | 0x10,
        ObjectTypeLegacy.GameObject => TypeMaskObject | 0x20,
        _ => TypeMaskObject,
    };

    public static int FieldEnd(ObjectTypeLegacy type) => type switch
    {
        ObjectTypeLegacy.Item => LegacyVersion.GetUpdateField(ItemField.ITEM_END),
        ObjectTypeLegacy.Unit => LegacyVersion.GetUpdateField(UnitField.UNIT_END),
        ObjectTypeLegacy.Player => LegacyVersion.GetUpdateField(PlayerField.PLAYER_END),
        ObjectTypeLegacy.GameObject => LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_END),
        _ => LegacyVersion.GetUpdateField(ObjectField.OBJECT_END),
    };

    public static uint Bits(float value) => BitConverter.SingleToUInt32Bits(value);

    /// <summary>A whole SMSG_UPDATE_OBJECT carrying this one create.</summary>
    public static byte[] Build(LegacyCreate create) => LegacyPacketBuilder.Build(Opcode.SMSG_UPDATE_OBJECT, packet =>
    {
        packet.WriteUInt32(1);
        if (LegacyMovementWire.Era != LegacyEra.WotLK)
            packet.WriteBool(false); // has transport

        packet.WriteUInt8((byte)create.UpdateType);
        packet.WritePackedGuid(create.Guid);
        packet.WriteUInt8((byte)create.Type);
        WriteMovementBlock(packet, create);
        WriteFields(packet, create);
    });

    private static void WriteMovementBlock(WorldPacket packet, LegacyCreate create)
    {
        UpdateFlag flags = UpdateFlag.None;
        if (create.Self) flags |= UpdateFlag.Self;
        if (create.Living != null) flags |= UpdateFlag.Living;
        if (create.GoPosition != null) flags |= UpdateFlag.GOPosition;
        if (create.Stationary != null) flags |= UpdateFlag.StationaryObject;
        if (create.LowAndHighGuid) flags |= UpdateFlag.LowGuid | UpdateFlag.HighGuid;
        if (create.AttackingTarget != null) flags |= UpdateFlag.AttackingTarget;
        if (create.TransportPathTimer != null) flags |= UpdateFlag.Transport;
        if (create.Vehicle != null) flags |= UpdateFlag.Vehicle;
        if (create.Rotation != null) flags |= UpdateFlag.GORotation;

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
            packet.WriteUInt16((ushort)flags);
        else
            packet.WriteUInt8((byte)flags);

        if (create.Living is { } living)
        {
            LegacyMove move = create.Spline != null
                ? living with { Flags = living.Flags | MovementFlagWotLK.SplineEnabled }
                : living;
            LegacyMovementWire.Write(packet, move);

            var speeds = create.Speeds;
            packet.WriteFloat(speeds.Walk);
            packet.WriteFloat(speeds.Run);
            packet.WriteFloat(speeds.RunBack);
            packet.WriteFloat(speeds.Swim);
            packet.WriteFloat(speeds.SwimBack);
            if (LegacyMovementWire.Era != LegacyEra.Vanilla)
            {
                packet.WriteFloat(speeds.Flight);
                packet.WriteFloat(speeds.FlightBack);
            }
            packet.WriteFloat(speeds.Turn);
            if (LegacyMovementWire.Era == LegacyEra.WotLK)
                packet.WriteFloat(speeds.Pitch);

            if (create.Spline is { } spline)
                WriteSpline(packet, spline);
        }
        else if (create.GoPosition is { } goPosition)
        {
            packet.WritePackedGuid(goPosition.Transport);
            packet.WriteVector3(goPosition.Position);
            packet.WriteVector3(goPosition.Offset);
            packet.WriteFloat(goPosition.Orientation);
            packet.WriteFloat(goPosition.CorpseOrientation);
        }
        else if (create.Stationary is { } stationary)
        {
            packet.WriteVector3(stationary.Position);
            packet.WriteFloat(stationary.Orientation);
        }

        if (create.LowAndHighGuid)
        {
            packet.WriteUInt32(0x11111111);
            packet.WriteUInt32(0x22222222);
        }

        if (create.AttackingTarget is { } target)
            packet.WritePackedGuid(target);
        if (create.TransportPathTimer is { } pathTimer)
            packet.WriteUInt32(pathTimer);
        if (create.Vehicle is { } vehicle)
        {
            packet.WriteUInt32(vehicle.Id);
            packet.WriteFloat(vehicle.Orientation);
        }
        if (create.Rotation is { } rotation)
            packet.WriteInt64(rotation.GetPackedRotation());
    }

    private static void WriteSpline(WorldPacket packet, LegacySpline spline)
    {
        uint finalOrientation = LegacyMovementWire.Era switch
        {
            LegacyEra.WotLK => (uint)SplineFlagWotLK.FinalOrientation,
            LegacyEra.Tbc => (uint)SplineFlagTBC.FinalOrientation,
            _ => (uint)SplineFlagVanilla.FinalOrientation,
        };

        packet.WriteUInt32(spline.FinalOrientation ? finalOrientation : 0u);
        if (spline.FinalOrientation)
            packet.WriteFloat(spline.Orientation);

        packet.WriteUInt32(spline.Time);
        packet.WriteUInt32(spline.TimeFull);
        packet.WriteUInt32(spline.Id);

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
        {
            packet.WriteFloat(1f);  // duration multiplier
            packet.WriteFloat(1f);  // duration multiplier next
            packet.WriteInt32(0);   // vertical acceleration
            packet.WriteInt32(0);   // start time
        }

        packet.WriteUInt32((uint)spline.Points.Length);
        foreach (var point in spline.Points)
            packet.WriteVector3(point);

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_8_9464))
            packet.WriteUInt8(0);   // spline mode

        packet.WriteVector3(spline.EndPoint);
    }

    private static void WriteFields(WorldPacket packet, LegacyCreate create)
    {
        int guidField = LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_GUID);
        ReadOnlySpan<(int Field, uint Value)> header =
        [
            (guidField, (uint)create.Guid.Low),
            (guidField + 1, (uint)(create.Guid.Low >> 32)),
            (LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_TYPE), TypeMask(create.Type)),
            (LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_ENTRY), create.Entry),
            (LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_SCALE_X), Bits(1f)),
        ];

        // The mask spans the object's whole field range, as every 3.3.5a core sizes it.
        int fieldEnd = FieldEnd(create.Type);
        int words = (fieldEnd + 31) / 32;
        Span<uint> mask = stackalloc uint[words];
        Span<uint> values = new uint[fieldEnd];

        foreach (var (field, value) in header)
            Set(mask, values, field, value);
        foreach (var (field, value) in create.Fields)
            Set(mask, values, field, value);

        packet.WriteUInt8((byte)words);
        foreach (uint word in mask)
            packet.WriteUInt32(word);
        for (int field = 0; field < fieldEnd; field++)
        {
            if ((mask[field / 32] & (1u << (field % 32))) != 0)
                packet.WriteUInt32(values[field]);
        }
    }

    // A field this era does not have resolves to -1 and is left off the wire.
    private static void Set(Span<uint> mask, Span<uint> values, int field, uint value)
    {
        if (field < 0 || field >= values.Length)
            return;
        mask[field / 32] |= 1u << (field % 32);
        values[field] = value;
    }
}
