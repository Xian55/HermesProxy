using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
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
/// <c>MovementInfo</c> stays a class here. It is a mutable builder for the *outbound* direction —
/// 46 sites populate one field-by-field while assembling a packet for the client — so making it a
/// value type is outbound work. The packet structs hold a reference to it, which means the one
/// MovementInfo allocation per movement packet survives this slice; everything around it does not.
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
        var moveInfo = movement.MoveInfo;
        var gameState = ctx.GetSession().GameState;
        if (moveInfo.TransportGuid != gameState.LastReportedTransportGuid)
        {
            if (_melTransportRider.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Trace))
            {
                // Both halves: a HighGuid::Transport guid keeps its identity in High and
                // has Low = 0, so Low alone reads as "0 -> 0" on AzerothCore.
                TransportLogMessages.ClientTransportChanged(_melTransportRider, universalOpcode.ToString(),
                    gameState.LastReportedTransportGuid.Low, gameState.LastReportedTransportGuid.High,
                    moveInfo.TransportGuid.Low, moveInfo.TransportGuid.High,
                    moveInfo.StandingOnGameObjectGuid.Low,
                    moveInfo.TransportOffset.X, moveInfo.TransportOffset.Y, moveInfo.TransportOffset.Z,
                    moveInfo.TransportOrientation, moveInfo.TransportSeat,
                    moveInfo.Position.X, moveInfo.Position.Y, moveInfo.Position.Z);
            }
            gameState.LastReportedTransportGuid = moveInfo.TransportGuid;
        }

        WorldPacket packet = new WorldPacket(legacyOpcode);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            packet.WritePackedGuid(movement.Guid.To64());
        moveInfo.WriteMovementInfoLegacy(packet);
        ctx.SendPacketToServer(packet);

        CheckProximityAreaTriggers(moveInfo.Position, gameState, ctx);
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
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
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
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            packet.WritePackedGuid(speed.MoverGUID.To64());
        else
            packet.WriteGuid(speed.MoverGUID.To64());
        packet.WriteUInt32(speed.Ack.MoveCounter);
        speed.Ack.MoveInfo.WriteMovementInfoLegacy(packet);
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
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            packet.WritePackedGuid(movementAck.MoverGUID.To64());
        else
            packet.WriteGuid(movementAck.MoverGUID.To64());
        packet.WriteUInt32(movementAck.Ack.MoveCounter);
        movementAck.Ack.MoveInfo.WriteMovementInfoLegacy(packet);
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
        WorldPacket packet = new WorldPacket(opcode);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            packet.WritePackedGuid(movementAck.MoverGUID.To64());
        else
            packet.WriteGuid(movementAck.MoverGUID.To64());
        packet.WriteUInt32(movementAck.Ack.MoveCounter);
        movementAck.Ack.MoveInfo.WriteMovementInfoLegacy(packet);
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
        packet.WriteGuid(move.MoverGUID.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_MOVE_INIT_ACTIVE_MOVER_COMPLETE)]
    public static void HandleMoveInitActiveMoverComplete(in InitActiveMoverComplete move, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_ACTIVE_MOVER);
        packet.WriteGuid(ctx.GetSession().GameState.CurrentPlayerGuid.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_MOVE_SPLINE_DONE)]
    public static void HandleMoveSplineDone(in MoveSplineDone movement, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_MOVE_SPLINE_DONE);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            packet.WritePackedGuid(movement.Guid.To64());
        movement.MoveInfo.WriteMovementInfoLegacy(packet);
        packet.WriteInt32(movement.SplineID);
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            packet.WriteFloat(0); // Spline Type
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_MOVE_TIME_SKIPPED)]
    public static void HandleMoveTimeSkipped(in MoveTimeSkipped movement, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_MOVE_TIME_SKIPPED);
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
}
