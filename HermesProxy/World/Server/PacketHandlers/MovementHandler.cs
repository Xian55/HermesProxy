using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Server;

public partial class WorldSocket
{
    // Handlers for CMSG opcodes coming from the modern client
    [PacketHandler(Opcode.CMSG_MOVE_CHANGE_TRANSPORT)]
    [PacketHandler(Opcode.CMSG_MOVE_FALL_LAND)]
    [PacketHandler(Opcode.CMSG_MOVE_FALL_RESET)]
    [PacketHandler(Opcode.CMSG_MOVE_HEARTBEAT)]
    [PacketHandler(Opcode.CMSG_MOVE_JUMP)]
    [PacketHandler(Opcode.CMSG_MOVE_REMOVE_MOVEMENT_FORCES)]
    [PacketHandler(Opcode.CMSG_MOVE_SET_FACING)]
    [PacketHandler(Opcode.CMSG_MOVE_SET_FACING_HEARTBEAT)]
    [PacketHandler(Opcode.CMSG_MOVE_SET_FLY)]
    [PacketHandler(Opcode.CMSG_MOVE_SET_PITCH)]
    [PacketHandler(Opcode.CMSG_MOVE_SET_RUN_MODE)]
    [PacketHandler(Opcode.CMSG_MOVE_SET_WALK_MODE)]
    [PacketHandler(Opcode.CMSG_MOVE_START_ASCEND)]
    [PacketHandler(Opcode.CMSG_MOVE_START_BACKWARD)]
    [PacketHandler(Opcode.CMSG_MOVE_START_DESCEND)]
    [PacketHandler(Opcode.CMSG_MOVE_START_FORWARD)]
    [PacketHandler(Opcode.CMSG_MOVE_START_PITCH_DOWN)]
    [PacketHandler(Opcode.CMSG_MOVE_START_PITCH_UP)]
    [PacketHandler(Opcode.CMSG_MOVE_START_SWIM)]
    [PacketHandler(Opcode.CMSG_MOVE_START_TURN_LEFT)]
    [PacketHandler(Opcode.CMSG_MOVE_START_TURN_RIGHT)]
    [PacketHandler(Opcode.CMSG_MOVE_START_STRAFE_LEFT)]
    [PacketHandler(Opcode.CMSG_MOVE_START_STRAFE_RIGHT)]
    [PacketHandler(Opcode.CMSG_MOVE_STOP)]
    [PacketHandler(Opcode.CMSG_MOVE_STOP_ASCEND)]
    [PacketHandler(Opcode.CMSG_MOVE_STOP_PITCH)]
    [PacketHandler(Opcode.CMSG_MOVE_STOP_STRAFE)]
    [PacketHandler(Opcode.CMSG_MOVE_STOP_SWIM)]
    [PacketHandler(Opcode.CMSG_MOVE_STOP_TURN)]
    [PacketHandler(Opcode.CMSG_MOVE_DOUBLE_JUMP)]
    void HandlePlayerMove(ClientPlayerMovement movement)
    {
        string opcodeName = movement.GetUniversalOpcode().ToString();
        opcodeName = opcodeName.Replace("CMSG", "MSG");
        uint opcode = Opcodes.GetOpcodeValueForVersion(opcodeName, LegacyVersion.Build);
        if (opcode == 0)
            opcode = Opcodes.GetOpcodeValueForVersion("MSG_MOVE_SET_FACING", LegacyVersion.Build);

        WorldPacket packet = new WorldPacket(opcode);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            packet.WritePackedGuid(movement.Guid.To64());
        movement.MoveInfo.WriteMovementInfoLegacy(packet);
        SendPacketToServer(packet);

        CheckLegacyAreaTriggerProximity(movement.MoveInfo.Position);
    }

    // V3_4_3 client's AreaTrigger.db2 is missing legacy walk-through triggers
    // (notably the Blasted Lands Dark Portal). When the player walks into a
    // known legacy trigger volume, synthesize CMSG_AREA_TRIGGER to the legacy
    // server. One-shot per entry into the sphere; cleared on map change.
    void CheckLegacyAreaTriggerProximity(Vector3 pos)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        var mapId = GetSession().GameState.CurrentMapId;
        if (mapId is null)
            return;

        if (!AreaTriggerReconciliation.ProximityByMap.TryGetValue(mapId.Value, out var entries))
            return;

        var lastSent = GetSession().GameState.LastLegacyAreaTriggerSendTickMs;
        long nowMs = Environment.TickCount64;

        foreach (var e in entries)
        {
            float distSq = Vector3.DistanceSquared(pos, e.Center);
            bool inside = distSq <= e.Radius * e.Radius;

            if (!inside)
            {
                lastSent.Remove(e.LegacyId);
                continue;
            }

            // While inside our generous proxy-side sphere, re-fire CMSG_AREA_TRIGGER
            // every ~250ms so that whichever heartbeat first sees the player cross
            // the server's stricter DBC volume actually gets accepted and teleports.
            if (lastSent.TryGetValue(e.LegacyId, out var last) &&
                nowMs - last < GameSessionData.LegacyAreaTriggerResendIntervalMs)
                continue;

            lastSent[e.LegacyId] = nowMs;

            WorldPacket packet = new WorldPacket(Opcode.CMSG_AREA_TRIGGER);
            packet.WriteUInt32(e.LegacyId);
            SendPacketToServer(packet);
        }
    }

    [PacketHandler(Opcode.CMSG_MOVE_TELEPORT_ACK)]
    void HandleMoveTeleportAck(MoveTeleportAck teleport)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_MOVE_TELEPORT_ACK);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            packet.WritePackedGuid(teleport.MoverGUID.To64());
        else
            packet.WriteGuid(teleport.MoverGUID.To64());
        packet.WriteUInt32(teleport.MoveCounter);
        packet.WriteUInt32(teleport.MoveTime);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_WORLD_PORT_RESPONSE)]
    void HandleWorldPortResponse(WorldPortResponse teleport)
    {
        GetSession().GameState.IsWaitingForWorldPortAck = false;
        WorldPacket packet = new WorldPacket(Opcode.MSG_MOVE_WORLDPORT_ACK);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_MOVE_FORCE_FLIGHT_BACK_SPEED_CHANGE_ACK)]
    [PacketHandler(Opcode.CMSG_MOVE_FORCE_FLIGHT_SPEED_CHANGE_ACK)]
    [PacketHandler(Opcode.CMSG_MOVE_FORCE_PITCH_RATE_CHANGE_ACK)]
    [PacketHandler(Opcode.CMSG_MOVE_FORCE_RUN_BACK_SPEED_CHANGE_ACK)]
    [PacketHandler(Opcode.CMSG_MOVE_FORCE_RUN_SPEED_CHANGE_ACK)]
    [PacketHandler(Opcode.CMSG_MOVE_FORCE_SWIM_BACK_SPEED_CHANGE_ACK)]
    [PacketHandler(Opcode.CMSG_MOVE_FORCE_SWIM_SPEED_CHANGE_ACK)]
    [PacketHandler(Opcode.CMSG_MOVE_FORCE_TURN_RATE_CHANGE_ACK)]
    [PacketHandler(Opcode.CMSG_MOVE_FORCE_WALK_SPEED_CHANGE_ACK)]
    void HandleMoveForceSpeedChangeAck(MovementSpeedAck speed)
    {
        var opcode = speed.GetUniversalOpcode();
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
        SendPacketToServer(packet);
    }

    MovementFlagModern GetFlagForAckOpcode(Opcode opcode)
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

    [PacketHandler(Opcode.CMSG_MOVE_FEATHER_FALL_ACK)]
    [PacketHandler(Opcode.CMSG_MOVE_HOVER_ACK)]
    [PacketHandler(Opcode.CMSG_MOVE_SET_CAN_FLY_ACK)]
    [PacketHandler(Opcode.CMSG_MOVE_WATER_WALK_ACK)]
    void HandleMoveForceAck1(MovementAckMessage movementAck)
    {
        WorldPacket packet = new WorldPacket(movementAck.GetUniversalOpcode());
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            packet.WritePackedGuid(movementAck.MoverGUID.To64());
        else
            packet.WriteGuid(movementAck.MoverGUID.To64());
        packet.WriteUInt32(movementAck.Ack.MoveCounter);
        movementAck.Ack.MoveInfo.WriteMovementInfoLegacy(packet);
        packet.WriteInt32(movementAck.Ack.MoveInfo.Flags.HasAnyFlag(GetFlagForAckOpcode(movementAck.GetUniversalOpcode())) ? 1 : 0);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_MOVE_FORCE_ROOT_ACK)]
    [PacketHandler(Opcode.CMSG_MOVE_FORCE_UNROOT_ACK)]
    [PacketHandler(Opcode.CMSG_MOVE_KNOCK_BACK_ACK)]
    [PacketHandler(Opcode.CMSG_MOVE_GRAVITY_DISABLE_ACK)]
    [PacketHandler(Opcode.CMSG_MOVE_GRAVITY_ENABLE_ACK)]
    void HandleMoveForceAck2(MovementAckMessage movementAck)
    {
        WorldPacket packet = new WorldPacket(movementAck.GetUniversalOpcode());
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            packet.WritePackedGuid(movementAck.MoverGUID.To64());
        else
            packet.WriteGuid(movementAck.MoverGUID.To64());
        packet.WriteUInt32(movementAck.Ack.MoveCounter);
        movementAck.Ack.MoveInfo.WriteMovementInfoLegacy(packet);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_MOVE_SET_COLLISION_HEIGHT_ACK)]
    void HandleMoveSetCollisionHeightAck(MoveSetCollisionHeightAck collisionHeightAck)
    {
        // This opcode doesn't exist in legacy servers (Vanilla/TBC/WotLK).
        // The modern client sends it in response to SMSG_MOVE_SET_COLLISION_HEIGHT,
        // but legacy servers don't expect or need it. Simply discard the packet.
    }

    [PacketHandler(Opcode.CMSG_SET_ACTIVE_MOVER)]
    void HandleMoveSetActiveMover(SetActiveMover move)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_ACTIVE_MOVER);
        packet.WriteGuid(move.MoverGUID.To64());
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_MOVE_INIT_ACTIVE_MOVER_COMPLETE)]
    void HandleMoveInitActiveMoverComplete(InitActiveMoverComplete move)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_ACTIVE_MOVER);
        packet.WriteGuid(GetSession().GameState.CurrentPlayerGuid.To64());
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_MOVE_SPLINE_DONE)]
    void HandleMoveSplineDone(MoveSplineDone movement)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_MOVE_SPLINE_DONE);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            packet.WritePackedGuid(movement.Guid.To64());
        movement.MoveInfo.WriteMovementInfoLegacy(packet);
        packet.WriteInt32(movement.SplineID);
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            packet.WriteFloat(0); // Spline Type
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_MOVE_TIME_SKIPPED)]
    void HandleMoveSplineDone(MoveTimeSkipped movement)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_MOVE_TIME_SKIPPED);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            packet.WritePackedGuid(movement.MoverGUID.To64());
        else
            packet.WriteGuid(movement.MoverGUID.To64());
        packet.WriteUInt32(movement.TimeSkipped);
        SendPacketToServer(packet);
    }

    // "Leave Vehicle" button on the modern V3_4_3 vehicle UI emits CMSG_MOVE_DISMISS_VEHICLE
    // (with a MovementInfo body) — the legacy 3.3.5a equivalent is CMSG_REQUEST_VEHICLE_EXIT,
    // which is empty and resolves the vehicle from session state. Rewrite the opcode and drop
    // the body to translate. Without this the click was getting routed through HandlePlayerMove
    // and silently degraded to MSG_MOVE_SET_FACING, leaving the player stuck in the vehicle
    // (e.g. Grand Theft Palomino quest 12680).
    //
    // PREV_SEAT / NEXT_SEAT / REQUEST_VEHICLE_EXIT all share the same empty wire shape and
    // are wired here as well, since they did not have any handler at all before.
    [PacketHandler(Opcode.CMSG_REQUEST_VEHICLE_EXIT)]
    [PacketHandler(Opcode.CMSG_REQUEST_VEHICLE_PREV_SEAT)]
    [PacketHandler(Opcode.CMSG_REQUEST_VEHICLE_NEXT_SEAT)]
    [PacketHandler(Opcode.CMSG_MOVE_DISMISS_VEHICLE)]
    void HandleRequestVehicleSeatChange(RequestVehicleSeatChange request)
    {
        Opcode targetOpcode = request.GetUniversalOpcode();
        if (targetOpcode == Opcode.CMSG_MOVE_DISMISS_VEHICLE)
            targetOpcode = Opcode.CMSG_REQUEST_VEHICLE_EXIT;
        WorldPacket packet = new WorldPacket(targetOpcode);
        SendPacketToServer(packet);
    }
}
