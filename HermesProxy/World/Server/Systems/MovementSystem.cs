using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Client;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's movement CMSGs — the highest-rate inbound family.
/// </summary>
/// <remarks>
/// Bodies were moved mechanically from <c>World/Server/PacketHandlers/MovementHandler.cs</c>, not
/// retyped: the only rewrites are the signature, <c>ctx.</c> prefixes, and the opcode arriving as a
/// parameter rather than being read back off the packet. <c>verify-handler-port.py</c> diffs each
/// one against the original.
/// <para>
/// Since then the movement block moved behind <c>LegacyMovementCodec.Write</c>, which replaced
/// <c>MovementInfo.WriteMovementInfoLegacy</c> call for call.
/// </para>
/// </remarks>
public static class MovementSystem
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melTransportRider =
        Log.CreateMelLogger(Log.CategoryServer);

    // CMSG_MOVE_* (universal, as the modern client sends it) -> MSG_MOVE_* (universal, the name
    // the 3.3.5a-era protocol uses for the same message). Built once from the enum names so it
    // cannot drift from the enum, then resolved through LegacyVersion's array lookup.
    //
    // HandlePlayerMove used to derive this per packet: enum ToString, string Replace, then a
    // reflection-backed Enum.TryParse against the version's opcode enum. That measured 128 B and
    // 595 ns on every movement packet -- the highest-rate client opcode there is -- versus 0 B and
    // 2.3 ns for the lookup below (MovementHandlerPrologueBenchmarks).
    private static readonly FrozenDictionary<Opcode, Opcode> ClientMoveToLegacyMsg =
        BuildClientMoveToLegacyMsg();

    private static FrozenDictionary<Opcode, Opcode> BuildClientMoveToLegacyMsg()
    {
        const string clientPrefix = "CMSG_MOVE";
        var map = new Dictionary<Opcode, Opcode>();
        foreach (Opcode op in Enum.GetValues<Opcode>())
        {
            string name = op.ToString();
            if (!name.StartsWith(clientPrefix, StringComparison.Ordinal))
                continue;
            // "CMSG_MOVE_JUMP" -> "MSG_MOVE_JUMP"; matches the old Replace("CMSG", "MSG").
            if (Enum.TryParse(string.Concat("MSG_", name.AsSpan("CMSG_".Length)), out Opcode legacyMsg))
                map[op] = legacyMsg;
        }
        return map.ToFrozenDictionary();
    }

    private static MovementFlagModern GetFlagForAckOpcode(Opcode opcode)
    {
        switch (opcode)
        {
            case Opcode.CMSG_MOVE_FEATHER_FALL_ACK:
                return MovementFlagModern.CanSafeFall;
            case Opcode.CMSG_MOVE_HOVER_ACK:
                return MovementFlagModern.Hover;
            case Opcode.CMSG_MOVE_SET_CAN_FLY_ACK:
                return MovementFlagModern.CanFly;
            case Opcode.CMSG_MOVE_WATER_WALK_ACK:
                return MovementFlagModern.Waterwalking;
        }
        return MovementFlagModern.None;
    }

    [HandlesCmsg(Opcode.CMSG_MOVE_CHANGE_TRANSPORT)]
    [HandlesCmsg(Opcode.CMSG_MOVE_FALL_LAND)]
    [HandlesCmsg(Opcode.CMSG_MOVE_FALL_RESET)]
    [HandlesCmsg(Opcode.CMSG_MOVE_HEARTBEAT)]
    [HandlesCmsg(Opcode.CMSG_MOVE_JUMP)]
    [HandlesCmsg(Opcode.CMSG_MOVE_REMOVE_MOVEMENT_FORCES)]
    [HandlesCmsg(Opcode.CMSG_MOVE_SET_FACING)]
    [HandlesCmsg(Opcode.CMSG_MOVE_SET_FACING_HEARTBEAT)]
    [HandlesCmsg(Opcode.CMSG_MOVE_SET_FLY)]
    [HandlesCmsg(Opcode.CMSG_MOVE_SET_PITCH)]
    [HandlesCmsg(Opcode.CMSG_MOVE_SET_RUN_MODE)]
    [HandlesCmsg(Opcode.CMSG_MOVE_SET_WALK_MODE)]
    [HandlesCmsg(Opcode.CMSG_MOVE_START_ASCEND)]
    [HandlesCmsg(Opcode.CMSG_MOVE_START_BACKWARD)]
    [HandlesCmsg(Opcode.CMSG_MOVE_START_DESCEND)]
    [HandlesCmsg(Opcode.CMSG_MOVE_START_FORWARD)]
    [HandlesCmsg(Opcode.CMSG_MOVE_START_PITCH_DOWN)]
    [HandlesCmsg(Opcode.CMSG_MOVE_START_PITCH_UP)]
    [HandlesCmsg(Opcode.CMSG_MOVE_START_SWIM)]
    [HandlesCmsg(Opcode.CMSG_MOVE_START_TURN_LEFT)]
    [HandlesCmsg(Opcode.CMSG_MOVE_START_TURN_RIGHT)]
    [HandlesCmsg(Opcode.CMSG_MOVE_START_STRAFE_LEFT)]
    [HandlesCmsg(Opcode.CMSG_MOVE_START_STRAFE_RIGHT)]
    [HandlesCmsg(Opcode.CMSG_MOVE_STOP)]
    [HandlesCmsg(Opcode.CMSG_MOVE_STOP_ASCEND)]
    [HandlesCmsg(Opcode.CMSG_MOVE_STOP_PITCH)]
    [HandlesCmsg(Opcode.CMSG_MOVE_STOP_STRAFE)]
    [HandlesCmsg(Opcode.CMSG_MOVE_STOP_SWIM)]
    [HandlesCmsg(Opcode.CMSG_MOVE_STOP_TURN)]
    [HandlesCmsg(Opcode.CMSG_MOVE_DOUBLE_JUMP)]
    public static void HandlePlayerMove(Opcode opcode, in ClientPlayerMovement movement, in SessionContext ctx)
    {
        Opcode universalOpcode = opcode;
        uint legacyOpcode = ClientMoveToLegacyMsg.TryGetValue(universalOpcode, out Opcode legacyMsg)
            ? LegacyVersion.GetCurrentOpcode(legacyMsg)
            : 0u;
        if (legacyOpcode == 0)
            legacyOpcode = LegacyVersion.GetCurrentOpcode(Opcode.MSG_MOVE_SET_FACING);

        // The client attaches itself to a transport WMO it stands on, the way a 3.3.5a
        // client does, and that is what the backend's Strand of the Ancients boarding relies
        // on. Log the moment that changes -- boarding, leaving -- with the world position and
        // the deck offset side by side. Not every packet: a rider sends a dozen a second.
        ref readonly MovementInfo moveInfo = ref movement.MoveInfo;
        var gameState = ctx.GetSession().GameState;
        bool takesSeatFromTransport = false;
        if (moveInfo.TransportGuid != gameState.LastReportedTransportGuid)
        {
            takesSeatFromTransport = LeavesTransportObjectForUnit(
                gameState.LastReportedTransportGuid, moveInfo.TransportGuid);
            if (_melTransportRider.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Trace))
            {
                // Both halves: a HighGuid::Transport guid keeps its identity in High and
                // has Low = 0, so Low alone reads as "0 -> 0" on AzerothCore.
                TransportInfo transport = moveInfo.Transport ?? new TransportInfo();
                TransportLogMessages.ClientTransportChanged(_melTransportRider, universalOpcode.ToString(),
                    gameState.LastReportedTransportGuid.Low, gameState.LastReportedTransportGuid.High,
                    transport.Guid.Low, transport.Guid.High,
                    moveInfo.StandingOnGameObjectGuid.Low,
                    transport.Offset.X, transport.Offset.Y, transport.Offset.Z,
                    transport.Orientation, transport.Seat,
                    moveInfo.Position.X, moveInfo.Position.Y, moveInfo.Position.Z);
            }
            gameState.LastReportedTransportGuid = moveInfo.TransportGuid;
        }

        WorldPacket packet = new WorldPacket(legacyOpcode);
        if (WorldClient.IsCataLegacy)
            LegacyMovementCata.TryWrite(packet, LegacyVersion.GetUniversalOpcode(legacyOpcode), movement.Guid.To64(), in moveInfo);
        else
        {
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                packet.WritePackedGuid(movement.Guid.To64());
            SeatGravity.WriteClientMovement(packet, in moveInfo, gameState);
        }
        ctx.SendPacketToServer(packet);

        if (takesSeatFromTransport)
            RepeatSeatReport(legacyOpcode, in movement, gameState, in ctx);

        CheckProximityAreaTriggers(moveInfo.Position, gameState, ctx);
    }

    /// <summary>
    /// True when the client's own report moves it from a transport object onto a unit: a vehicle
    /// seat taken from a boat's deck.
    /// </summary>
    /// <remarks>
    /// A 3.3.5a server hands a passenger from one transport object to another and nothing else. A
    /// report naming a unit, from a player it still has down as the boat's passenger, is taken as
    /// "left the transport": the server drops the boat, blanks the transport in what it relays to
    /// everyone else, and forgets the seat in its own copy of the player's movement. The same
    /// report sent again finds no boat to leave and is taken as it stands. A seated player is
    /// rooted and sends nothing further by itself, so without the repeat the vehicle's rider is
    /// left with an empty seat it cannot eject anyone from.
    /// </remarks>
    private static bool LeavesTransportObjectForUnit(WowGuid128 previous, WowGuid128 current) =>
        SeatGravity.Applies &&
        previous.GetObjectType() == ObjectType.GameObject &&
        current.GetObjectType() is ObjectType.Player or ObjectType.Unit;

    // Out of line so the per-packet path above stays as it was.
    private static void RepeatSeatReport(uint legacyOpcode, in ClientPlayerMovement movement, GameSessionData gameState, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(legacyOpcode);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            packet.WritePackedGuid(movement.Guid.To64());
        SeatGravity.WriteClientMovement(packet, in movement.MoveInfo, gameState);
        ctx.SendPacketToServer(packet);
    }

    /// <summary>
    /// Fires the legacy area triggers the modern client cannot send, once the player's own position
    /// enters one.
    /// </summary>
    /// <remarks>
    /// Cataclysm gave Eye of the Storm's towers new trigger rows ~18 yd from the WotLK ones and
    /// dropped the old rows from the client, so a 3.4.3 client never sends the ids
    /// <c>BattlegroundEY::HandleAreaTrigger</c> gates flag captures on. Translating the id is not
    /// enough either: the legacy server re-checks the player against its own AreaTrigger.dbc
    /// position first (<c>WorldSession::HandleAreaTriggerOpcode</c>) and drops anything sent from
    /// 18 yd away. Sending it from the movement path instead means the player really is standing
    /// inside the radius the server is about to test.
    /// <para>
    /// Runs after the movement packet so the server has already applied the position this decision
    /// was made on.
    /// </para>
    /// </remarks>
    private static void CheckProximityAreaTriggers(in Vector3 position, GameSessionData gameState, in SessionContext ctx)
    {
        if (!ModernVersion.IsWotLKClassicOrLater)
            return;

        if (gameState.CurrentMapId is not uint mapId)
            return;

        // Resolved lazily rather than on a map-change hook: an empty array is cached for maps with
        // no triggers, so the dictionary is touched once per map rather than once per packet.
        if (gameState.ProximityTriggers is null || gameState.ProximityTriggersMapId != mapId)
        {
            gameState.ProximityTriggers = GameData.AreaTriggerProximityByMap.TryGetValue(mapId, out var forMap)
                ? forMap
                : [];
            gameState.ProximityTriggersMapId = mapId;
            gameState.ProximityTriggersInsideMask = 0;
        }

        var triggers = gameState.ProximityTriggers;
        if (triggers.Length == 0)
            return;

        uint insideMask = gameState.ProximityTriggersInsideMask;
        for (int i = 0; i < triggers.Length; i++)
        {
            ref readonly var trigger = ref triggers[i];
            float dx = position.X - trigger.X;
            float dy = position.Y - trigger.Y;
            float dz = position.Z - trigger.Z;
            bool inside = (dx * dx) + (dy * dy) + (dz * dz) <= trigger.RadiusSquared;

            uint bit = 1u << i;
            bool wasInside = (insideMask & bit) != 0;
            if (inside == wasInside)
                continue;

            insideMask ^= bit;
            if (!inside)
                continue;

            gameState.LastEnteredAreaTrigger = trigger.LegacyId;
            WorldPacket triggerPacket = new WorldPacket(Opcode.CMSG_AREA_TRIGGER);
            triggerPacket.WriteUInt32(trigger.LegacyId);
            ctx.SendPacketToServer(triggerPacket);
        }
        gameState.ProximityTriggersInsideMask = insideMask;
    }

    [HandlesCmsg(Opcode.CMSG_MOVE_TELEPORT_ACK)]
    public static void HandleMoveTeleportAck(in MoveTeleportAck teleport, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_MOVE_TELEPORT_ACK);
        if (WorldClient.IsCataLegacy)
        {
            // TrinityCore 4.3.4 MoveTeleportAck::Read: the counters first, then a masked GUID.
            packet.WriteUInt32(teleport.MoveCounter);
            packet.WriteUInt32(teleport.MoveTime);
            MaskedGuid.Write(packet, teleport.MoverGUID.To64().Low, [5, 0, 1, 6, 3, 7, 2, 4], [4, 2, 7, 6, 5, 1, 3, 0]);
            ctx.SendPacketToServer(packet);
            return;
        }

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            packet.WritePackedGuid(teleport.MoverGUID.To64());
        else
            packet.WriteGuid(teleport.MoverGUID.To64());
        packet.WriteUInt32(teleport.MoveCounter);
        packet.WriteUInt32(teleport.MoveTime);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_WORLD_PORT_RESPONSE)]
    public static void HandleWorldPortResponse(in WorldPortResponse teleport, in SessionContext ctx)
    {
        ctx.GetSession().GameState.IsWaitingForWorldPortAck = false;
        WorldPacket packet = new WorldPacket(Opcode.MSG_MOVE_WORLDPORT_ACK);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_MOVE_FORCE_FLIGHT_BACK_SPEED_CHANGE_ACK)]
    [HandlesCmsg(Opcode.CMSG_MOVE_FORCE_FLIGHT_SPEED_CHANGE_ACK)]
    [HandlesCmsg(Opcode.CMSG_MOVE_FORCE_PITCH_RATE_CHANGE_ACK)]
    [HandlesCmsg(Opcode.CMSG_MOVE_FORCE_RUN_BACK_SPEED_CHANGE_ACK)]
    [HandlesCmsg(Opcode.CMSG_MOVE_FORCE_RUN_SPEED_CHANGE_ACK)]
    [HandlesCmsg(Opcode.CMSG_MOVE_FORCE_SWIM_BACK_SPEED_CHANGE_ACK)]
    [HandlesCmsg(Opcode.CMSG_MOVE_FORCE_SWIM_SPEED_CHANGE_ACK)]
    [HandlesCmsg(Opcode.CMSG_MOVE_FORCE_TURN_RATE_CHANGE_ACK)]
    [HandlesCmsg(Opcode.CMSG_MOVE_FORCE_WALK_SPEED_CHANGE_ACK)]
    public static void HandleMoveForceSpeedChangeAck(Opcode opcode, in MovementSpeedAck speed, in SessionContext ctx)
    {
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180)
            && opcode is Opcode.CMSG_MOVE_FORCE_FLIGHT_SPEED_CHANGE_ACK
                      or Opcode.CMSG_MOVE_FORCE_FLIGHT_BACK_SPEED_CHANGE_ACK)
            return; // This is probably an ack by our swim to fly speed change for vanilla

        WorldPacket packet = new WorldPacket(opcode);
        if (WorldClient.IsCataLegacy)
        {
            // The new speed is the sequence's extra element (TrinityCore 4.3.4 HandleForceSpeedChangeAck).
            if (LegacyMovementCata.TryWrite(packet, opcode, speed.MoverGUID.To64(), speed.Ack.MoveInfo, speed.Ack.MoveCounter, speed.Speed))
                ctx.SendPacketToServer(packet);
            return;
        }

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            packet.WritePackedGuid(speed.MoverGUID.To64());
        else
            packet.WriteGuid(speed.MoverGUID.To64());
        packet.WriteUInt32(speed.Ack.MoveCounter);
        SeatGravity.WriteClientMovement(packet, speed.Ack.MoveInfo, ctx.GetSession().GameState);
        packet.WriteFloat(speed.Speed);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_MOVE_FEATHER_FALL_ACK)]
    [HandlesCmsg(Opcode.CMSG_MOVE_HOVER_ACK)]
    [HandlesCmsg(Opcode.CMSG_MOVE_SET_CAN_FLY_ACK)]
    [HandlesCmsg(Opcode.CMSG_MOVE_WATER_WALK_ACK)]
    public static void HandleMoveForceAck1(Opcode opcode, in MovementAckMessage movementAck, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(opcode);
        if (WorldClient.IsCataLegacy)
        {
            // 4.3.4 says applied or not through the movement flags alone; there is no trailing int.
            if (LegacyMovementCata.TryWrite(packet, opcode, movementAck.MoverGUID.To64(), movementAck.Ack.MoveInfo, movementAck.Ack.MoveCounter))
                ctx.SendPacketToServer(packet);
            return;
        }

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            packet.WritePackedGuid(movementAck.MoverGUID.To64());
        else
            packet.WriteGuid(movementAck.MoverGUID.To64());
        packet.WriteUInt32(movementAck.Ack.MoveCounter);
        SeatGravity.WriteClientMovement(packet, movementAck.Ack.MoveInfo, ctx.GetSession().GameState);
        packet.WriteInt32(movementAck.Ack.MoveInfo.Flags.HasAnyFlag(GetFlagForAckOpcode(opcode)) ? 1 : 0);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_MOVE_FORCE_ROOT_ACK)]
    [HandlesCmsg(Opcode.CMSG_MOVE_FORCE_UNROOT_ACK)]
    [HandlesCmsg(Opcode.CMSG_MOVE_KNOCK_BACK_ACK)]
    [HandlesCmsg(Opcode.CMSG_MOVE_GRAVITY_DISABLE_ACK)]
    [HandlesCmsg(Opcode.CMSG_MOVE_GRAVITY_ENABLE_ACK)]
    public static void HandleMoveForceAck2(Opcode opcode, in MovementAckMessage movementAck, in SessionContext ctx)
    {
        var gameState = ctx.GetSession().GameState;
        // The proxy's own gravity change for a vehicle seat; the server never asked for it.
        if (SeatGravity.ConsumeOwnAck(opcode, movementAck.Ack.MoveCounter, gameState))
            return;

        WorldPacket packet = new WorldPacket(opcode);
        if (WorldClient.IsCataLegacy)
        {
            if (LegacyMovementCata.TryWrite(packet, opcode, movementAck.MoverGUID.To64(), movementAck.Ack.MoveInfo, movementAck.Ack.MoveCounter))
                ctx.SendPacketToServer(packet);
            return;
        }

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            packet.WritePackedGuid(movementAck.MoverGUID.To64());
        else
            packet.WriteGuid(movementAck.MoverGUID.To64());
        packet.WriteUInt32(movementAck.Ack.MoveCounter);
        SeatGravity.WriteClientMovement(packet, movementAck.Ack.MoveInfo, gameState);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_MOVE_SET_COLLISION_HEIGHT_ACK)]
    public static void HandleMoveSetCollisionHeightAck(in MoveSetCollisionHeightAck collisionHeightAck, in SessionContext ctx)
    {
        // 3.3.5 does have CMSG_MOVE_SET_COLLISION_HGT_ACK (0x517), but Hermes
        // already emits SMSG_MOVE_SET_COLLISION_HEIGHT from UNIT_FIELD_MOUNTDISPLAYID
        // Values. Forwarding the ACK would make AC emit MSG 0x518 and we would
        // double-send SET. Discard.
    }

    [HandlesCmsg(Opcode.CMSG_SET_ACTIVE_MOVER)]
    public static void HandleMoveSetActiveMover(in SetActiveMover move, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_ACTIVE_MOVER);
        WriteActiveMover(packet, move.MoverGUID.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_MOVE_INIT_ACTIVE_MOVER_COMPLETE)]
    public static void HandleMoveInitActiveMoverComplete(in InitActiveMoverComplete move, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_ACTIVE_MOVER);
        WriteActiveMover(packet, ctx.GetSession().GameState.CurrentPlayerGuid.To64());
        ctx.SendPacketToServer(packet);
    }

    // 4.3.4 reads the mover as a masked GUID (TrinityCore 4.3.4 SetActiveMover::Read). Sent the
    // 3.3.5a way it named no unit, so the server had no actively moved unit and silently ignored
    // every cast and movement packet after it.
    static void WriteActiveMover(WorldPacket packet, WowGuid64 mover)
    {
        if (WorldClient.IsCataLegacy)
            MaskedGuid.Write(packet, mover.Low, [7, 2, 1, 0, 4, 5, 6, 3], [3, 2, 4, 0, 5, 1, 6, 7]);
        else
            packet.WriteGuid(mover);
    }

    [HandlesCmsg(Opcode.CMSG_MOVE_SPLINE_DONE)]
    public static void HandleMoveSplineDone(in MoveSplineDone movement, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_MOVE_SPLINE_DONE);
        if (WorldClient.IsCataLegacy)
        {
            // TrinityCore 4.3.4 HandleMoveSplineDoneOpcode: the spline id, then the movement block.
            packet.WriteInt32(movement.SplineID);
            if (LegacyMovementCata.TryWrite(packet, Opcode.CMSG_MOVE_SPLINE_DONE, movement.Guid.To64(), movement.MoveInfo))
                ctx.SendPacketToServer(packet);
            return;
        }

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            packet.WritePackedGuid(movement.Guid.To64());
        SeatGravity.WriteClientMovement(packet, movement.MoveInfo, ctx.GetSession().GameState);
        packet.WriteInt32(movement.SplineID);
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            packet.WriteFloat(0); // Spline Type
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_MOVE_TIME_SKIPPED)]
    public static void HandleMoveTimeSkipped(in MoveTimeSkipped movement, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_MOVE_TIME_SKIPPED);
        if (WorldClient.IsCataLegacy)
        {
            // TrinityCore 4.3.4 HandleMoveTimeSkippedOpcode: the time, then a masked GUID.
            packet.WriteUInt32(movement.TimeSkipped);
            MaskedGuid.Write(packet, movement.MoverGUID.To64().Low, [5, 1, 3, 7, 6, 0, 4, 2], [7, 1, 2, 4, 3, 6, 0, 5]);
            ctx.SendPacketToServer(packet);
            return;
        }

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            packet.WritePackedGuid(movement.MoverGUID.To64());
        else
            packet.WriteGuid(movement.MoverGUID.To64());
        packet.WriteUInt32(movement.TimeSkipped);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_REQUEST_VEHICLE_EXIT)]
    [HandlesCmsg(Opcode.CMSG_REQUEST_VEHICLE_PREV_SEAT)]
    [HandlesCmsg(Opcode.CMSG_REQUEST_VEHICLE_NEXT_SEAT)]
    [HandlesCmsg(Opcode.CMSG_MOVE_DISMISS_VEHICLE)]
    public static void HandleRequestVehicleSeatChange(Opcode opcode, in RequestVehicleSeatChange request, in SessionContext ctx)
    {
        Opcode targetOpcode = opcode;
        if (targetOpcode == Opcode.CMSG_MOVE_DISMISS_VEHICLE)
            targetOpcode = Opcode.CMSG_REQUEST_VEHICLE_EXIT;
        WorldPacket packet = new WorldPacket(targetOpcode);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_RIDE_VEHICLE_INTERACT)]
    public static void HandleRideVehicleInteract(in RideVehicleInteract interact, in SessionContext ctx)
    {
        // Player vehicles arrived with 3.0.2; an older server has no opcode to receive this.
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
            return;

        // 3.3.5a calls the same request CMSG_PLAYER_VEHICLE_ENTER.
        WorldPacket packet = new WorldPacket(Opcode.CMSG_PLAYER_VEHICLE_ENTER);
        packet.WriteGuid(interact.Vehicle.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_EJECT_PASSENGER)]
    public static void HandleEjectPassenger(in EjectPassenger eject, in SessionContext ctx)
    {
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_EJECT_PASSENGER);
        packet.WriteGuid(eject.Passenger.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_REQUEST_VEHICLE_SWITCH_SEAT)]
    public static void HandleRequestVehicleSwitchSeat(in RequestVehicleSwitchSeat request, in SessionContext ctx)
    {
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
            return;

        // Unlike boarding and ejecting, 3.3.5a reads this vehicle guid packed.
        WorldPacket packet = new WorldPacket(Opcode.CMSG_REQUEST_VEHICLE_SWITCH_SEAT);
        packet.WritePackedGuid(request.Vehicle.To64());
        packet.WriteInt8((sbyte)request.SeatIndex);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_MOVE_SET_VEHICLE_REC_ID_ACK)]
    public static void HandleMoveSetVehicleRecIDAck(in MoveSetVehicleRecIDAck ack, in SessionContext ctx)
    {
        // This acknowledges the proxy's own MoveSetVehicleRecID. The legacy
        // PLAYER_VEHICLE_DATA broadcast has no sequence index or corresponding ACK.
    }
}
