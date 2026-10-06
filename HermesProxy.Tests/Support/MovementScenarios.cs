using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using HermesProxy.World.Server.Systems;

namespace HermesProxy.Tests.Support;

/// <summary>One legacy packet and the handler it belongs to.</summary>
internal sealed record ServerMoveScenario(string Name, Opcode Opcode, byte[] Wire, Func<WorldClient, Action<WorldPacket>> Handler);

internal enum ClientMoveKind { PlayerMove, SpeedAck, FlagAck, PlainAck, SplineDone }

/// <summary>One modern client packet body, framed as the dispatch site meets it.</summary>
internal sealed record ClientMoveScenario(string Name, Opcode Opcode, ClientMoveKind Kind, byte[] Framed);

/// <summary>
/// The movement traffic the golden wire tests and <c>MovementTranslationBenchmarks</c> share:
/// every packet that carries a movement block, in both directions, with each optional part of
/// the block present in at least one scenario and absent in another.
/// </summary>
/// <remarks>
/// Everything is built in the layout of the process's legacy and modern builds, and a scenario
/// whose opcode the legacy build does not have is left out, so the same list runs under any pair.
/// </remarks>
internal static class MovementScenarios
{
    public static readonly WowGuid64 ActivePlayer = new(HighGuidTypeLegacy.Player, 1);
    public static readonly WowGuid64 OtherPlayer = new(HighGuidTypeLegacy.Player, 13053);
    public static readonly WowGuid64 Creature = new(HighGuidTypeLegacy.Creature, 13358, 100);
    public static readonly WowGuid64 Vehicle = new(HighGuidTypeLegacy.Vehicle, 28670, 55);
    public static readonly WowGuid64 Boat = new(HighGuidTypeLegacy.MOTransport, 7);
    public static readonly WowGuid64 Chest = new(HighGuidTypeLegacy.GameObject, 2843, 12);
    public static readonly WowGuid64 Crate = new(HighGuidTypeLegacy.GameObject, 190584, 13);
    public static readonly WowGuid64 Sword = new(HighGuidTypeLegacy.Item, 500);

    private static readonly LegacyTransport OnBoat = new(Boat, new Vector3(1.5f, -2.25f, 6f), 0.75f, 999, 2, 555);

    // ---- legacy movement states ----------------------------------------------------------

    public static readonly LegacyMove Forward = new() { Flags = MovementFlagWotLK.Forward };

    public static readonly LegacyMove FallingOnBoat = new()
    {
        Flags = MovementFlagWotLK.Forward | MovementFlagWotLK.Falling,
        Transport = OnBoat,
        FallTime = 777,
        JumpVerticalSpeed = -9.81f,
        JumpSin = 0.5f,
        JumpCos = 0.86f,
        JumpHorizontalSpeed = 7.5f,
    };

    public static readonly LegacyMove ForwardOnSpline = new() { Flags = MovementFlagWotLK.Forward | MovementFlagWotLK.SplineEnabled };

    // Everything the sanitiser repairs at once, so each handler can be shown to apply it: a
    // rooted mover that reports movement, opposing turns, and both orientations out of range.
    private static readonly LegacyMove NeedsRepair = new()
    {
        Flags = MovementFlagWotLK.Root | MovementFlagWotLK.Forward | MovementFlagWotLK.TurnLeft | MovementFlagWotLK.TurnRight,
        Orientation = -1f,
        Transport = OnBoat with { Orientation = 20f },
    };

    public static IEnumerable<(string Name, LegacyMove Move)> LegacyMoves()
    {
        yield return ("forward", Forward);
        yield return ("idle", new LegacyMove());
        yield return ("swimming-pitch", new LegacyMove { Flags = MovementFlagWotLK.Forward | MovementFlagWotLK.Swimming, Pitch = -0.5f });
        yield return ("falling", new LegacyMove
        {
            Flags = MovementFlagWotLK.Forward | MovementFlagWotLK.Falling,
            FallTime = 777, JumpVerticalSpeed = -9.81f, JumpSin = 0.5f, JumpCos = 0.86f, JumpHorizontalSpeed = 7.5f,
        });
        yield return ("falling-far", new LegacyMove { Flags = MovementFlagWotLK.FallingFar, FallTime = 400 });
        yield return ("fall-time-only", new LegacyMove { Flags = MovementFlagWotLK.Forward, FallTime = 250 });
        yield return ("on-transport", new LegacyMove { Flags = MovementFlagWotLK.Forward, Transport = OnBoat });
        yield return ("on-transport-interpolated", new LegacyMove
        {
            ExtraFlags = (ushort)MovementFlagExtra.InterpolateMove,
            Transport = OnBoat,
        });
        yield return ("on-transport-falling", FallingOnBoat);
        yield return ("on-transport-seat-zero", new LegacyMove { Transport = OnBoat with { Guid = Vehicle, Seat = 0 } });
        yield return ("spline-elevation", new LegacyMove { Flags = MovementFlagWotLK.Forward | MovementFlagWotLK.SplineElevation, SplineElevation = 2.25f });
        // The flag with a zero elevation is stripped, and an elevation without the flag cannot
        // be sent at all: the float only exists on the wire behind the flag.
        yield return ("spline-elevation-zero", new LegacyMove { Flags = MovementFlagWotLK.SplineElevation });
        yield return ("spline-enabled", ForwardOnSpline);
        yield return ("flying-pitch", new LegacyMove { Flags = MovementFlagWotLK.Flying | MovementFlagWotLK.CanFly | MovementFlagWotLK.Forward, Pitch = 0.25f });
        yield return ("always-allow-pitching", new LegacyMove { ExtraFlags = (ushort)MovementFlagExtra.AlwaysAllowPitching, Pitch = 0.125f });
        yield return ("extra-flags", new LegacyMove { Flags = MovementFlagWotLK.Forward, ExtraFlags = 0x0218 });
        yield return ("passive-flags", new LegacyMove
        {
            Flags = MovementFlagWotLK.WalkMode | MovementFlagWotLK.Hover | MovementFlagWotLK.Waterwalking
                    | MovementFlagWotLK.CanSafeFall | MovementFlagWotLK.DisableGravity,
        });
        yield return ("root-while-moving", new LegacyMove { Flags = MovementFlagWotLK.Root | MovementFlagWotLK.Forward | MovementFlagWotLK.StrafeLeft });
        yield return ("opposing-directions", new LegacyMove
        {
            Flags = MovementFlagWotLK.Forward | MovementFlagWotLK.Backward
                    | MovementFlagWotLK.StrafeLeft | MovementFlagWotLK.StrafeRight
                    | MovementFlagWotLK.TurnLeft | MovementFlagWotLK.TurnRight
                    | MovementFlagWotLK.PitchUp | MovementFlagWotLK.PitchDown
                    | MovementFlagWotLK.Ascending | MovementFlagWotLK.Descending,
        });
        yield return ("can-fly-while-falling", new LegacyMove
        {
            Flags = MovementFlagWotLK.CanFly | MovementFlagWotLK.Falling,
            FallTime = 100, JumpVerticalSpeed = 1f, JumpSin = 0f, JumpCos = 1f, JumpHorizontalSpeed = 2f,
        });
        yield return ("orientation-negative", new LegacyMove { Orientation = -1f, Transport = OnBoat with { Orientation = -0.5f } });
        yield return ("orientation-over-two-pi", new LegacyMove { Orientation = 7.5f, Transport = OnBoat with { Orientation = 20f } });

        if (LegacyMovementWire.Era == LegacyEra.Tbc)
            yield return ("tbc-flying2-pitch", new LegacyMove { RawEraFlags = (uint)MovementFlagTBC.Flying2, Pitch = 0.3f });
        if (LegacyMovementWire.Era == LegacyEra.Vanilla)
            yield return ("vanilla-fixed-z", new LegacyMove { RawEraFlags = (uint)MovementFlagVanilla.FixedZ });
    }

    // ---- legacy server -> client ---------------------------------------------------------

    public static byte[] Heartbeat(LegacyMove move) => MoverThenMove(Opcode.MSG_MOVE_HEARTBEAT, OtherPlayer, move);

    public static byte[] SetRunSpeed(LegacyMove move) => MoverThenMove(Opcode.MSG_MOVE_SET_RUN_SPEED, OtherPlayer, move, p => p.WriteFloat(9.8f));

    public static byte[] KnockBack(LegacyMove move) => MoverThenMove(Opcode.MSG_MOVE_KNOCK_BACK, OtherPlayer, move, p =>
    {
        p.WriteFloat(0.25f);    // sin
        p.WriteFloat(0.96f);    // cos
        p.WriteFloat(12f);      // horizontal speed
        p.WriteFloat(-8f);      // vertical speed
    });

    public static IEnumerable<ServerMoveScenario> ServerMoves()
    {
        foreach (var (name, move) in LegacyMoves())
        {
            if (Mapped(Opcode.MSG_MOVE_HEARTBEAT))
                yield return new($"heartbeat/{name}", Opcode.MSG_MOVE_HEARTBEAT, Heartbeat(move), c => c.HandleMovementMessages);
        }

        if (Mapped(Opcode.MSG_MOVE_KNOCK_BACK))
        {
            yield return new("knock-back/falling-on-boat", Opcode.MSG_MOVE_KNOCK_BACK,
                KnockBack(FallingOnBoat), c => c.HandleMoveKnockBack);
            yield return new("knock-back/needs-repair", Opcode.MSG_MOVE_KNOCK_BACK,
                KnockBack(NeedsRepair), c => c.HandleMoveKnockBack);
        }

        if (Mapped(Opcode.MSG_MOVE_TELEPORT_ACK))
        {
            foreach (var (name, mover, move) in ((string, WowGuid64, LegacyMove)[])
                     [
                         ("other-player", OtherPlayer, Forward),
                         ("active-player", ActivePlayer, Forward),
                         ("on-transport", OtherPlayer, FallingOnBoat),
                         ("on-transport-seat-zero", OtherPlayer, new LegacyMove { Transport = OnBoat with { Guid = Vehicle, Seat = 0 } }),
                         ("needs-repair", OtherPlayer, NeedsRepair),
                         ("needs-repair-off-transport", OtherPlayer, NeedsRepair with { Transport = null }),
                     ])
            {
                yield return new($"teleport-ack/{name}", Opcode.MSG_MOVE_TELEPORT_ACK,
                    LegacyPacketBuilder.Build(Opcode.MSG_MOVE_TELEPORT_ACK, p =>
                    {
                        p.WritePackedGuid(mover);
                        p.WriteUInt32(42);  // move counter
                        LegacyMovementWire.Write(p, move);
                    }),
                    c => c.HandleMoveTeleportAck);
            }
        }

        foreach (Opcode speed in (Opcode[])
                 [
                     Opcode.MSG_MOVE_SET_RUN_SPEED, Opcode.MSG_MOVE_SET_SWIM_SPEED,
                     Opcode.MSG_MOVE_SET_SWIM_BACK_SPEED, Opcode.MSG_MOVE_SET_FLIGHT_SPEED, Opcode.MSG_MOVE_SET_TURN_RATE,
                 ])
        {
            if (!Mapped(speed))
                continue;
            yield return new($"speed/{speed}", speed,
                MoverThenMove(speed, OtherPlayer, FallingOnBoat, p => p.WriteFloat(9.8f)),
                c => c.HandleMoveUpdateSpeed);
        }

        if (Mapped(Opcode.MSG_MOVE_SET_WALK_SPEED))
        {
            yield return new("speed/needs-repair", Opcode.MSG_MOVE_SET_WALK_SPEED,
                MoverThenMove(Opcode.MSG_MOVE_SET_WALK_SPEED, OtherPlayer, NeedsRepair, p => p.WriteFloat(2.5f)),
                c => c.HandleMoveUpdateSpeed);
        }
    }

    // ---- legacy creates ------------------------------------------------------------------

    private static (int, uint)[] UnitFields() =>
    [
        (LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_HEALTH), 100u),
        (LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MAXHEALTH), 100u),
        (LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_LEVEL), 10u),
        (LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_FACTIONTEMPLATE), 14u),
        (LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_DISPLAYID), 1234u),
        (LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_NATIVEDISPLAYID), 1234u),
    ];

    private static (int, uint)[] GameObjectFields(uint type, Quaternion? fieldRotation = null)
    {
        var fields = new List<(int, uint)>
        {
            (LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_DISPLAYID), 3015u),
            (LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_FLAGS), 0u),
            (LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_FACTION), 35u),
            // 3.x packs state, type, art kit and animation progress into one field; the older
            // eras have a field each. Whichever the era lacks resolves to -1 and is dropped.
            (LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_BYTES_1), 1u | (type << 8) | (255u << 24)),
            (LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_STATE), 1u),
            (LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_TYPE_ID), type),
            (LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_ANIMPROGRESS), 255u),
        };

        if (fieldRotation is { } rotation)
        {
            // GAMEOBJECT_ROTATION on 1.12 / 2.4.3, GAMEOBJECT_PARENTROTATION on 3.3.5a.
            int field = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_ROTATION);
            if (field < 0)
                field = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_PARENTROTATION);
            if (field >= 0)
            {
                fields.Add((field, LegacyCreateWire.Bits(rotation.X)));
                fields.Add((field + 1, LegacyCreateWire.Bits(rotation.Y)));
                fields.Add((field + 2, LegacyCreateWire.Bits(rotation.Z)));
                fields.Add((field + 3, LegacyCreateWire.Bits(rotation.W)));
            }
        }

        return [.. fields];
    }

    private static readonly LegacyStationary ChestSpot = new(new Vector3(-8913.2f, 554.6f, 93.8f), 5.3f);
    private static readonly Quaternion ChestRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 5.3f);

    public static readonly LegacyCreate CreatureCreate = new()
    {
        Guid = Creature, Type = ObjectTypeLegacy.Unit, Entry = 13358,
        Living = Forward, LowAndHighGuid = true, Fields = UnitFields(),
    };

    public static readonly LegacyCreate ChestCreate = new()
    {
        Guid = Chest, Type = ObjectTypeLegacy.GameObject, Entry = 2843,
        Stationary = ChestSpot,
        Rotation = LegacyMovementWire.Era == LegacyEra.WotLK ? ChestRotation : null,
        Fields = GameObjectFields(type: 3, fieldRotation: ChestRotation),
    };

    /// <summary>An item has no position, so its create has no movement block at all.</summary>
    public static readonly LegacyCreate ItemCreate = new() { Guid = Sword, Type = ObjectTypeLegacy.Item, Entry = 2488 };

    public static IEnumerable<(string Name, LegacyCreate Create)> Creates()
    {
        bool wotlk = LegacyMovementWire.Era == LegacyEra.WotLK;

        yield return ("creature-living", CreatureCreate);
        yield return ("creature-living-default-speeds", CreatureCreate with
        {
            Speeds = new LegacySpeeds(0, 0, 0, 0, 0, 0, 0, 0, 0),
        });
        yield return ("creature-walking-on-spline", CreatureCreate with
        {
            Living = new LegacyMove { Flags = MovementFlagWotLK.WalkMode | MovementFlagWotLK.Forward },
            Spline = new LegacySpline(FinalOrientation: false, 0f, 250, 3000, 77,
                [new Vector3(1200f, 1473f, 307.5f), new Vector3(1205f, 1480f, 308f)], new Vector3(1210f, 1490f, 309f)),
        });
        yield return ("creature-on-spline-facing-angle", CreatureCreate with
        {
            Living = FallingOnBoat,
            Spline = new LegacySpline(FinalOrientation: true, 7.5f, 0, 1500, 78,
                [new Vector3(1200f, 1473f, 307.5f)], new Vector3(1201f, 1474f, 307.5f)),
        });
        yield return ("creature-attacking", CreatureCreate with { AttackingTarget = OtherPlayer });
        yield return ("creature-on-transport", CreatureCreate with { Living = FallingOnBoat });
        yield return ("creature-extra-flags", CreatureCreate with
        {
            Living = new LegacyMove { Flags = MovementFlagWotLK.Root | MovementFlagWotLK.Forward, ExtraFlags = 0x0018, Orientation = -2f },
        });
        yield return ("creature-needs-repair", CreatureCreate with { Living = NeedsRepair });
        yield return ("creature-without-position", CreatureCreate with { Living = null });

        yield return ("player-living", new LegacyCreate
        {
            Guid = OtherPlayer, Type = ObjectTypeLegacy.Player,
            Living = new LegacyMove { Flags = MovementFlagWotLK.Forward | MovementFlagWotLK.Swimming, Pitch = -0.5f },
            Speeds = new LegacySpeeds(Run: 9.8f, Swim: 6.5f),
            Fields = UnitFields(),
        });

        yield return ("gameobject-stationary", ChestCreate);
        yield return ("gameobject-stationary-create2", ChestCreate with { UpdateType = UpdateTypeLegacy.CreateObject2 });
        yield return ("item-without-position", ItemCreate);

        yield return ("mo-transport", new LegacyCreate
        {
            Guid = Boat, Type = ObjectTypeLegacy.GameObject, Entry = 20808,
            Stationary = new LegacyStationary(new Vector3(-4016.4f, -4740.6f, 0.04f), 5.24f),
            TransportPathTimer = 60133,
            Rotation = wotlk ? Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 5.24f) : null,
            Fields = GameObjectFields(type: 15),
        });
        yield return ("mo-transport-at-origin", new LegacyCreate
        {
            Guid = Boat, Type = ObjectTypeLegacy.GameObject, Entry = 20808,
            Stationary = new LegacyStationary(Vector3.Zero, 1f),
            TransportPathTimer = 1,
            Fields = GameObjectFields(type: 15),
        });

        if (wotlk)
        {
            yield return ("creature-vehicle", CreatureCreate with
            {
                Guid = Vehicle, Entry = 28670,
                Vehicle = new LegacyVehicle(312, 1.25f),
            });
            // A vehicle standing on a boat. Its own vehicle id stays in the vehicle part; the
            // transport part names what is ridden, and a boat is no vehicle (issue #344).
            yield return ("creature-vehicle-on-transport", CreatureCreate with
            {
                Guid = Vehicle, Entry = 28670,
                Living = FallingOnBoat,
                Vehicle = new LegacyVehicle(312, 1.25f),
            });
            yield return ("gameobject-on-transport", new LegacyCreate
            {
                Guid = Crate, Type = ObjectTypeLegacy.GameObject, Entry = 190584,
                GoPosition = new LegacyGoPosition(Boat, new Vector3(-4010f, -4735f, 6f), new Vector3(6.4f, 5.6f, 5.96f), 0.9f, 0.3f),
                Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.9f),
                Fields = GameObjectFields(type: 3),
            });
            yield return ("gameobject-zero-rotation", ChestCreate with { Rotation = new Quaternion(0, 0, 0, 0) });
        }
        else
        {
            yield return ("gameobject-field-rotation-absent", ChestCreate with { Fields = GameObjectFields(type: 3) });
        }

        if (LegacyMovementWire.Era == LegacyEra.Vanilla)
        {
            yield return ("creature-hover", CreatureCreate with
            {
                Living = new LegacyMove { RawEraFlags = (uint)MovementFlagVanilla.FixedZ },
            });
        }
    }

    // ---- modern client -> server ---------------------------------------------------------

    public static ModernMove ClientForward { get; } = new() { Flags = MovementFlagModern.Forward };

    public static ModernMove ClientFallingOnBoat(GameSessionData gameState) => new()
    {
        Flags = MovementFlagModern.Forward | MovementFlagModern.Falling,
        Transport = new ModernTransport(Boat.To128(gameState), new Vector3(1.5f, -2.25f, 6f), 0.75f, 2, 999, null, null),
        Fall = new ModernFall(777, -9.81f, new ModernFallDirection(0.5f, 0.86f, 7.5f)),
    };

    public static IEnumerable<(string Name, ModernMove Move)> ModernMoves(GameSessionData gameState)
    {
        var boat = Boat.To128(gameState);
        var transport = new ModernTransport(boat, new Vector3(1.5f, -2.25f, 6f), 0.75f, 2, 999, null, null);

        yield return ("forward", ClientForward);
        yield return ("idle", new ModernMove());
        yield return ("falling", new ModernMove
        {
            Flags = MovementFlagModern.Forward | MovementFlagModern.Falling,
            Fall = new ModernFall(777, -9.81f, new ModernFallDirection(0.5f, 0.86f, 7.5f)),
        });
        yield return ("falling-far", new ModernMove
        {
            Flags = MovementFlagModern.FallingFar,
            Fall = new ModernFall(400, -3f, new ModernFallDirection(0.1f, 0.99f, 1.5f)),
        });
        yield return ("fall-without-direction", new ModernMove { Fall = new ModernFall(250, -1f, null) });
        // The flag without the block: the legacy writer emits a jump block of zeroes.
        yield return ("falling-flag-without-fall-block", new ModernMove { Flags = MovementFlagModern.Falling });
        yield return ("on-transport", new ModernMove { Flags = MovementFlagModern.Forward, Transport = transport });
        yield return ("on-transport-falling", ClientFallingOnBoat(gameState));
        yield return ("on-transport-prev-time-and-vehicle", new ModernMove
        {
            ExtraFlags = (uint)MovementFlagExtra.InterpolateMove,
            Transport = transport with { PrevTime = 555, VehicleId = 312 },
        });
        yield return ("swimming-pitch", new ModernMove { Flags = MovementFlagModern.Forward | MovementFlagModern.Swimming, Pitch = -0.5f });
        yield return ("flying-pitch", new ModernMove
        {
            Flags = MovementFlagModern.Flying | MovementFlagModern.CanFly | MovementFlagModern.Ascending,
            Pitch = 0.25f,
        });
        yield return ("always-allow-pitching", new ModernMove { ExtraFlags = (uint)MovementFlagExtra.AlwaysAllowPitching, Pitch = 0.125f });
        yield return ("spline-elevation", new ModernMove { Flags = MovementFlagModern.SplineElevation, SplineElevation = 2.25f });
        yield return ("passive-flags", new ModernMove
        {
            Flags = MovementFlagModern.WalkMode | MovementFlagModern.Hover | MovementFlagModern.Waterwalking
                    | MovementFlagModern.CanSafeFall | MovementFlagModern.DisableGravity | MovementFlagModern.Root
                    | MovementFlagModern.DisableCollision,
            ExtraFlags = 0x0001FFFF,
            ExtraFlags2 = 0x5,
        });
        yield return ("every-optional-block", new ModernMove
        {
            Flags = MovementFlagModern.Forward | MovementFlagModern.Falling | MovementFlagModern.Swimming,
            Pitch = 0.4f,
            RemoveForces = [new WowGuid128(0x11, 0x22), new WowGuid128(0x3344, 0x5566)],
            MoveIndex = 9,
            Transport = transport with { PrevTime = 555, VehicleId = 312 },
            Fall = new ModernFall(777, -9.81f, new ModernFallDirection(0.5f, 0.86f, 7.5f)),
            HasSpline = true,
            HeightChangeFailed = true,
            RemoteTimeValid = true,
            Inertia = new ModernInertia(new WowGuid128(0x77, 0x88), new Vector3(1f, 2f, 3f), 4000),
            StandingOnGameObject = Chest.To128(gameState),
            AdvFlying = new ModernAdvFlying(12.5f, -3.5f),
        });
    }

    public static byte[] PlayerMove(ModernMove move, GameSessionData gameState) => ModernMovementWire.Frame(p =>
    {
        p.WritePackedGuid128(ActivePlayer.To128(gameState));
        ModernMovementWire.Write(p, move);
    });

    public static IEnumerable<ClientMoveScenario> ClientMoves(GameSessionData gameState)
    {
        var mover = ActivePlayer.To128(gameState);

        foreach (var (name, move) in ModernMoves(gameState))
        {
            yield return new($"player-move/{name}", Opcode.CMSG_MOVE_HEARTBEAT, ClientMoveKind.PlayerMove, PlayerMove(move, gameState));

            yield return new($"speed-ack/{name}", Opcode.CMSG_MOVE_FORCE_RUN_SPEED_CHANGE_ACK, ClientMoveKind.SpeedAck,
                ModernMovementWire.Frame(p =>
                {
                    p.WritePackedGuid128(mover);
                    ModernMovementWire.Write(p, move);
                    p.WriteUInt32(42);      // move counter
                    p.WriteFloat(9.8f);
                }));

            yield return new($"spline-done/{name}", Opcode.CMSG_MOVE_SPLINE_DONE, ClientMoveKind.SplineDone,
                ModernMovementWire.Frame(p =>
                {
                    p.WritePackedGuid128(mover);
                    ModernMovementWire.Write(p, move);
                    p.WriteInt32(77);       // spline id
                }));
        }

        // The flag acks answer with whether the acked flag is set, so each needs it both ways.
        foreach (var (opcode, flag) in ((Opcode, MovementFlagModern)[])
                 [
                     (Opcode.CMSG_MOVE_SET_CAN_FLY_ACK, MovementFlagModern.CanFly),
                     (Opcode.CMSG_MOVE_HOVER_ACK, MovementFlagModern.Hover),
                     (Opcode.CMSG_MOVE_WATER_WALK_ACK, MovementFlagModern.Waterwalking),
                     (Opcode.CMSG_MOVE_FEATHER_FALL_ACK, MovementFlagModern.CanSafeFall),
                 ])
        {
            foreach (bool set in (bool[])[true, false])
            {
                var move = new ModernMove { Flags = set ? flag | MovementFlagModern.Forward : MovementFlagModern.Forward };
                yield return new($"flag-ack/{opcode}/{(set ? "set" : "clear")}", opcode, ClientMoveKind.FlagAck, Ack(mover, move));
            }
        }

        foreach (Opcode opcode in (Opcode[])
                 [
                     Opcode.CMSG_MOVE_FORCE_ROOT_ACK, Opcode.CMSG_MOVE_KNOCK_BACK_ACK, Opcode.CMSG_MOVE_GRAVITY_DISABLE_ACK,
                 ])
        {
            yield return new($"plain-ack/{opcode}", opcode, ClientMoveKind.PlainAck, Ack(mover, ClientFallingOnBoat(gameState)));
        }
    }

    private static byte[] Ack(WowGuid128 mover, ModernMove move) => ModernMovementWire.Frame(p =>
    {
        p.WritePackedGuid128(mover);
        ModernMovementWire.Write(p, move);
        p.WriteUInt32(42);                  // move counter
    });

    /// <summary>Decodes the packet with its production codec and hands it to its system handler.</summary>
    public static void Dispatch(ClientMoveScenario scenario, in SessionContext ctx)
    {
        using var packet = new WorldPacket(scenario.Framed);
        var reader = new SpanPacketReader(packet.GetRemainingSpan());
        switch (scenario.Kind)
        {
            case ClientMoveKind.PlayerMove: PlayerMoveThunk(scenario.Opcode, ref reader, in ctx); break;
            case ClientMoveKind.SpeedAck: SpeedAckThunk(scenario.Opcode, ref reader, in ctx); break;
            case ClientMoveKind.FlagAck: FlagAckThunk(scenario.Opcode, ref reader, in ctx); break;
            case ClientMoveKind.PlainAck: PlainAckThunk(scenario.Opcode, ref reader, in ctx); break;
            case ClientMoveKind.SplineDone: SplineDoneThunk(ref reader, in ctx); break;
        }

        if (reader.Remaining != 0)
            throw new InvalidOperationException($"{scenario.Name}: the codec left {reader.Remaining} bytes unread.");
    }

    // One method per packet type, as the generated dispatch table has one thunk per opcode. Each
    // decoded packet is a local of well over a hundred bytes, and a method zeroes its locals on
    // entry whichever branch it takes: with all five in one frame, every call paid for five.

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void PlayerMoveThunk(Opcode opcode, ref SpanPacketReader reader, in SessionContext ctx)
    {
        ClientPlayerMovementCodec.Read(ref reader, out var movement);
        MovementSystem.HandlePlayerMove(opcode, in movement, in ctx);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void SpeedAckThunk(Opcode opcode, ref SpanPacketReader reader, in SessionContext ctx)
    {
        MovementSpeedAckCodec.Read(ref reader, out var speed);
        MovementSystem.HandleMoveForceSpeedChangeAck(opcode, in speed, in ctx);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void FlagAckThunk(Opcode opcode, ref SpanPacketReader reader, in SessionContext ctx)
    {
        MovementAckMessageCodec.Read(ref reader, out var ack);
        MovementSystem.HandleMoveForceAck1(opcode, in ack, in ctx);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void PlainAckThunk(Opcode opcode, ref SpanPacketReader reader, in SessionContext ctx)
    {
        MovementAckMessageCodec.Read(ref reader, out var ack);
        MovementSystem.HandleMoveForceAck2(opcode, in ack, in ctx);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void SplineDoneThunk(ref SpanPacketReader reader, in SessionContext ctx)
    {
        MoveSplineDoneCodec.Read(ref reader, out var done);
        MovementSystem.HandleMoveSplineDone(in done, in ctx);
    }

    private static bool Mapped(Opcode opcode) => LegacyVersion.GetCurrentOpcode(opcode) != 0;

    private static byte[] MoverThenMove(Opcode opcode, WowGuid64 mover, LegacyMove move, Action<WorldPacket>? tail = null)
        => LegacyPacketBuilder.Build(opcode, p =>
        {
            p.WritePackedGuid(mover);
            LegacyMovementWire.Write(p, move);
            tail?.Invoke(p);
        });
}
