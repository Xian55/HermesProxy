using Framework.GameMath;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [HandlesSmsg(Opcode.MSG_MOVE_START_FORWARD)]
    [HandlesSmsg(Opcode.MSG_MOVE_START_BACKWARD)]
    [HandlesSmsg(Opcode.MSG_MOVE_STOP)]
    [HandlesSmsg(Opcode.MSG_MOVE_START_STRAFE_LEFT)]
    [HandlesSmsg(Opcode.MSG_MOVE_START_STRAFE_RIGHT)]
    [HandlesSmsg(Opcode.MSG_MOVE_STOP_STRAFE)]
    [HandlesSmsg(Opcode.MSG_MOVE_START_ASCEND)]
    [HandlesSmsg(Opcode.MSG_MOVE_START_DESCEND)]
    [HandlesSmsg(Opcode.MSG_MOVE_STOP_ASCEND)]
    [HandlesSmsg(Opcode.MSG_MOVE_JUMP)]
    [HandlesSmsg(Opcode.MSG_MOVE_START_TURN_LEFT)]
    [HandlesSmsg(Opcode.MSG_MOVE_START_TURN_RIGHT)]
    [HandlesSmsg(Opcode.MSG_MOVE_STOP_TURN)]
    [HandlesSmsg(Opcode.MSG_MOVE_START_PITCH_UP)]
    [HandlesSmsg(Opcode.MSG_MOVE_START_PITCH_DOWN)]
    [HandlesSmsg(Opcode.MSG_MOVE_STOP_PITCH)]
    [HandlesSmsg(Opcode.MSG_MOVE_SET_RUN_MODE)]
    [HandlesSmsg(Opcode.MSG_MOVE_SET_WALK_MODE)]
    [HandlesSmsg(Opcode.MSG_MOVE_TELEPORT)]
    [HandlesSmsg(Opcode.MSG_MOVE_SET_FACING)]
    [HandlesSmsg(Opcode.MSG_MOVE_SET_PITCH)]
    [HandlesSmsg(Opcode.MSG_MOVE_TOGGLE_COLLISION_CHEAT)]
    [HandlesSmsg(Opcode.MSG_MOVE_GRAVITY_CHNG)]
    [HandlesSmsg(Opcode.MSG_MOVE_ROOT)]
    [HandlesSmsg(Opcode.MSG_MOVE_UNROOT)]
    [HandlesSmsg(Opcode.MSG_MOVE_START_SWIM)]
    [HandlesSmsg(Opcode.MSG_MOVE_STOP_SWIM)]
    [HandlesSmsg(Opcode.MSG_MOVE_START_SWIM_CHEAT)]
    [HandlesSmsg(Opcode.MSG_MOVE_STOP_SWIM_CHEAT)]
    [HandlesSmsg(Opcode.MSG_MOVE_HEARTBEAT)]
    [HandlesSmsg(Opcode.MSG_MOVE_FALL_LAND)]
    [HandlesSmsg(Opcode.MSG_MOVE_UPDATE_CAN_FLY)]
    [HandlesSmsg(Opcode.MSG_MOVE_UPDATE_CAN_TRANSITION_BETWEEN_SWIM_AND_FLY)]
    [HandlesSmsg(Opcode.MSG_MOVE_HOVER)]
    [HandlesSmsg(Opcode.MSG_MOVE_FEATHER_FALL)]
    [HandlesSmsg(Opcode.MSG_MOVE_WATER_WALK)]
    internal void HandleMovementMessages(WorldPacket packet)
    {
        MoveUpdate moveUpdate = new MoveUpdate();
        moveUpdate.MoverGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        moveUpdate.MoveInfo = new();
        moveUpdate.MoveInfo.ReadMovementInfoLegacy(packet, GetSession().GameState);
        moveUpdate.MoveInfo.Flags = (uint)(((MovementFlagWotLK)moveUpdate.MoveInfo.Flags).CastFlags<MovementFlagModern>());
        moveUpdate.MoveInfo.ValidateMovementInfo();
        SendPacketToClient(moveUpdate);
    }

    [HandlesSmsg(Opcode.MSG_MOVE_KNOCK_BACK)]
    internal void HandleMoveKnockBack(WorldPacket packet)
    {
        MoveUpdateKnockBack knockback = new MoveUpdateKnockBack();
        knockback.MoverGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        knockback.MoveInfo = new();
        knockback.MoveInfo.ReadMovementInfoLegacy(packet, GetSession().GameState);
        knockback.MoveInfo.Flags = (uint)(((MovementFlagWotLK)knockback.MoveInfo.Flags).CastFlags<MovementFlagModern>());
        knockback.MoveInfo.JumpSinAngle = packet.ReadFloat();
        knockback.MoveInfo.JumpCosAngle = packet.ReadFloat();
        knockback.MoveInfo.JumpHorizontalSpeed = packet.ReadFloat();
        knockback.MoveInfo.JumpVerticalSpeed = packet.ReadFloat();
        knockback.MoveInfo.ValidateMovementInfo();
        SendPacketToClient(knockback);
    }

    [HandlesSmsg(Opcode.SMSG_MOVE_KNOCK_BACK)]
    internal void HandleMoveForceKnockBack(WorldPacket packet)
    {
        MoveKnockBack knockback = new MoveKnockBack();
        knockback.MoverGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        knockback.MoveCounter = packet.ReadUInt32();
        knockback.Direction = packet.ReadVector2();
        knockback.HorizontalSpeed = packet.ReadFloat();
        knockback.VerticalSpeed = packet.ReadFloat();
        SendPacketToClient(knockback);
    }

    [HandlesSmsg(Opcode.SMSG_CONTROL_UPDATE)]
    internal void HandleControlUpdate(WorldPacket packet)
    {
        ControlUpdate control = new ControlUpdate();
        control.Guid = packet.ReadPackedGuid().To128(GetSession().GameState);
        control.HasControl = packet.ReadBool();
        SendPacketToClient(control);

        // V3_4_3 client routes WASD via a separate active-mover slot updated only by
        // SMSG_MOVE_SET_ACTIVE_MOVER. 3.3.5 only emits SMSG_CLIENT_CONTROL_UPDATE, so
        // without this synthesis the modern client treats CONTROL_UPDATE as camera-only
        // and never sends client movement for vehicle/charm/possess targets like the
        // Eye of Acherus (quest 12641).
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261 && control.HasControl)
        {
            MoveSetActiveMover setMover = new MoveSetActiveMover();
            setMover.MoverGUID = control.Guid;
            SendPacketToClient(setMover);
        }
    }

    [HandlesSmsg(Opcode.MSG_MOVE_TELEPORT_ACK)]
    internal void HandleMoveTeleportAck(WorldPacket packet)
    {
        WowGuid128 guid = packet.ReadPackedGuid().To128(GetSession().GameState);

        if (GetSession().GameState.IsInTaxiFlight &&
            GetSession().GameState.CurrentPlayerGuid == guid)
        {
            ControlUpdate control = new ControlUpdate();
            control.Guid = guid;
            control.HasControl = true;
            SendPacketToClient(control);
            GetSession().GameState.IsInTaxiFlight = false;
        }

        MoveTeleport teleport = new MoveTeleport();
        teleport.MoverGUID = guid;
        teleport.MoveCounter = packet.ReadUInt32();
        MovementInfo moveInfo = new();
        moveInfo.ReadMovementInfoLegacy(packet, GetSession().GameState);
        moveInfo.Flags = (uint)(((MovementFlagWotLK)moveInfo.Flags).CastFlags<MovementFlagModern>());
        moveInfo.ValidateMovementInfo();
        // A mover riding something expects deck-relative Pos/Facing, not world coords:
        // Unit::SendTeleportPacket runs the position through CalculatePassengerOffset
        // before filling MoveTeleport. Sending world coords makes the client add them
        // to the transport's own position and strands the player off the map.
        if (moveInfo.TransportGuid != default)
        {
            teleport.Position = moveInfo.TransportOffset;
            teleport.Orientation = moveInfo.TransportOrientation;
        }
        else
        {
            teleport.Position = moveInfo.Position;
            teleport.Orientation = moveInfo.Orientation;
        }
        teleport.TransportGUID = moveInfo.TransportGuid;
        if (moveInfo.TransportSeat > 0)
        {
            teleport.Vehicle = new();
            teleport.Vehicle.VehicleSeatIndex = moveInfo.TransportSeat;
        }
        SendPacketToClient(teleport);
    }

    [HandlesSmsg(Opcode.SMSG_TRANSFER_PENDING)]
    internal void HandleTransferPending(WorldPacket packet)
    {
        if (GetSession().GameState.IsWaitingForWorldPortAck)
        {
            Log.Print(LogType.Error, "Skipping SMSG_TRANSFER_PENDING, client is already being teleported.");
            return;
        }

        TransferPending transfer = new TransferPending();
        transfer.MapID = GetSession().GameState.PendingTransferMapId = packet.ReadUInt32();
        transfer.OldMapPosition = Vector3.Zero;

        // A map change driven by a transport carries the transport's entry and the map it
        // is leaving (AzerothCore Player.cpp:1609-1613). Without it the modern client
        // treats this as an ordinary teleport, so it detaches the player from the deck and
        // they arrive in freefall.
        if (packet.CanRead(8))
        {
            transfer.Ship = new TransferPending.ShipTransferPending
            {
                Id = packet.ReadUInt32(),
                OriginMapID = packet.ReadInt32(),
            };
            GetSession().GameState.TransferPendingShipEntry = transfer.Ship.Id;
            Log.Print(LogType.Trace,
                $"[Transport] SMSG_TRANSFER_PENDING on transport entry={transfer.Ship.Id} " +
                $"fromMap={transfer.Ship.OriginMapID} toMap={transfer.MapID}");
        }
        else
            GetSession().GameState.TransferPendingShipEntry = 0;

        SendPacketToClient(transfer);
        GetSession().GameState.IsFirstEnterWorld = false;
        GetSession().GameState.IsWaitingForNewWorld = true;

        SuspendToken suspend = new();
        suspend.SequenceIndex = 3;
        suspend.Reason = 1;
        SendPacketToClient(suspend);
    }

    [HandlesSmsg(Opcode.SMSG_TRANSFER_ABORTED)]
    internal void HandleTransferAborted(WorldPacket packet)
    {
        TransferAborted transfer = new TransferAborted();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            transfer.MapID = packet.ReadUInt32();
        else
            transfer.MapID = GetSession().GameState.PendingTransferMapId;

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            transfer.Reason = (TransferAbortReasonModern)packet.ReadUInt8();
        else
        {
            TransferAbortReasonLegacy legacyReason = (TransferAbortReasonLegacy)packet.ReadUInt8();
            transfer.Reason = legacyReason.CastEnum<TransferAbortReasonModern>();
        }

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            transfer.Arg = packet.ReadUInt8();

        SendPacketToClient(transfer);
        GetSession().GameState.IsWaitingForNewWorld = false;
    }

    [HandlesSmsg(Opcode.SMSG_NEW_WORLD)]
    internal void HandleNewWorld(WorldPacket packet)
    {
        NewWorld teleport = new NewWorld();
        GetSession().GameState.CurrentMapId = teleport.MapID = packet.ReadUInt32();
        teleport.Position = packet.ReadVector3();
        teleport.Orientation = packet.ReadFloat();
        teleport.Reason = 4;
        GetSession().GameState.IsFirstEnterWorld = false;

        if (GetSession().GameState.IsWaitingForNewWorld)
        {
            GetSession().GameState.IsWaitingForNewWorld = false;
            GetSession().GameState.IsWaitingForWorldPortAck = true;

            // SMSG_NEW_WORLD tears down the client's entire world model, the player object
            // included, and the server re-sends a CreateObject for everything on the new map.
            // ClientKnownGuids has to follow or it stays stale across the transition: the
            // Values filter would then forward deltas for objects the client no longer has,
            // which come straight back as CMSG_OBJECT_UPDATE_FAILED. Observed as a player
            // Values sent in the gap between the teleport and the re-create.
            if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
                GetSession().GameState.ClientKnownGuids.Clear();

            SendPacketToClient(teleport);
            if (teleport.MapID > 1)
            {
                UpdateLastInstance instance = new();
                instance.MapID = teleport.MapID;
                SendPacketToClient(instance);

                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                    SendPacketToClient(new TimeSyncRequest());
            }

            // HandleTransferPending sends a SuspendToken for every transfer, so the resume
            // has to be unconditional or the client stays suspended. It used to be nested
            // in the MapID > 1 branch, which left the two continents unbalanced: riding a
            // zeppelin to Northrend (571) arrived fine while the return trip to Tirisfal
            // (0), and Orgrimmar (1), dropped the player through the deck on arrival.
            ResumeToken resume = new();
            resume.SequenceIndex = 3;
            resume.Reason = 1;
            SendPacketToClient(resume);

            WorldServerInfo info = new();
            if (teleport.MapID > 1)
            {
                info.DifficultyID = 1;
                info.InstanceGroupSize = 5;
            }
            SendPacketToClient(info);
        }
    }

    // for server controlled units
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_SET_FLIGHT_BACK_SPEED)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_SET_FLIGHT_SPEED)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_SET_PITCH_RATE)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_SET_RUN_BACK_SPEED)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_SET_RUN_SPEED)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_SET_SWIM_BACK_SPEED)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_SET_SWIM_SPEED)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_SET_TURN_RATE)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_SET_WALK_SPEED)]
    internal void HandleMoveSplineSetSpeed(WorldPacket packet)
    {
        MoveSplineSetSpeed speed = new MoveSplineSetSpeed(packet.GetUniversalOpcode(false));
        speed.MoverGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        speed.Speed = packet.ReadFloat();
        SendPacketToClient(speed);
    }

    // for own player
    [HandlesSmsg(Opcode.SMSG_FORCE_WALK_SPEED_CHANGE)]
    [HandlesSmsg(Opcode.SMSG_FORCE_RUN_SPEED_CHANGE)]
    [HandlesSmsg(Opcode.SMSG_FORCE_RUN_BACK_SPEED_CHANGE)]
    [HandlesSmsg(Opcode.SMSG_FORCE_SWIM_SPEED_CHANGE)]
    [HandlesSmsg(Opcode.SMSG_FORCE_SWIM_BACK_SPEED_CHANGE)]
    [HandlesSmsg(Opcode.SMSG_FORCE_TURN_RATE_CHANGE)]
    [HandlesSmsg(Opcode.SMSG_FORCE_FLIGHT_SPEED_CHANGE)]
    [HandlesSmsg(Opcode.SMSG_FORCE_FLIGHT_BACK_SPEED_CHANGE)]
    [HandlesSmsg(Opcode.SMSG_FORCE_PITCH_RATE_CHANGE)]
    internal void HandleMoveForceSpeedChange(WorldPacket packet)
    { // for own player
        string opcodeName = packet.GetUniversalOpcode(false).ToString().Replace("SMSG_FORCE_", "SMSG_MOVE_SET_").Replace("_CHANGE", "");
        Opcode universalOpcode = Opcodes.GetUniversalOpcode(opcodeName);

        MoveSetSpeed speed = new MoveSetSpeed(universalOpcode);
        speed.MoverGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        speed.MoveCounter = packet.ReadUInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) &&
            packet.GetUniversalOpcode(false) == Opcode.SMSG_FORCE_RUN_SPEED_CHANGE)
        {
            packet.ReadUInt8(); // unk byte
        }

        speed.Speed = packet.ReadFloat();
        SendPacketToClient(speed);

        // Convenience in vanilla to use SwimSpeed as FlySpeed
        if (universalOpcode is Opcode.SMSG_MOVE_SET_SWIM_SPEED
                            or Opcode.SMSG_MOVE_SET_SWIM_BACK_SPEED &&
            LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            var flyOpcode = (Opcode) Enum.Parse(typeof(Opcode), universalOpcode.ToString().Replace("SWIM", "FLIGHT"));
            MoveSetSpeed flySpeed = new MoveSetSpeed(flyOpcode);
            flySpeed.MoverGUID = speed.MoverGUID;
            flySpeed.MoveCounter = speed.MoveCounter;
            flySpeed.Speed = speed.Speed;
            SendPacketToClient(flySpeed);
        }
    }

    // for other players
    [HandlesSmsg(Opcode.MSG_MOVE_SET_FLIGHT_BACK_SPEED)]
    [HandlesSmsg(Opcode.MSG_MOVE_SET_FLIGHT_SPEED)]
    [HandlesSmsg(Opcode.MSG_MOVE_SET_PITCH_RATE)]
    [HandlesSmsg(Opcode.MSG_MOVE_SET_RUN_BACK_SPEED)]
    [HandlesSmsg(Opcode.MSG_MOVE_SET_RUN_SPEED)]
    [HandlesSmsg(Opcode.MSG_MOVE_SET_SWIM_BACK_SPEED)]
    [HandlesSmsg(Opcode.MSG_MOVE_SET_SWIM_SPEED)]
    [HandlesSmsg(Opcode.MSG_MOVE_SET_TURN_RATE)]
    [HandlesSmsg(Opcode.MSG_MOVE_SET_WALK_SPEED)]
    internal void HandleMoveUpdateSpeed(WorldPacket packet)
    { // for other players
        string opcodeName = packet.GetUniversalOpcode(false).ToString().Replace("MSG_MOVE_SET", "SMSG_MOVE_UPDATE");
        Opcode universalOpcode = Opcodes.GetUniversalOpcode(opcodeName);

        MoveUpdateSpeed speed = new MoveUpdateSpeed(universalOpcode);
        speed.MoverGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        speed.MoveInfo = new MovementInfo();
        speed.MoveInfo.ReadMovementInfoLegacy(packet, GetSession().GameState);
        var newFlags = ((MovementFlagWotLK)speed.MoveInfo.Flags).CastFlags<MovementFlagModern>();
        speed.MoveInfo.Flags = (uint)(newFlags);
        speed.MoveInfo.ValidateMovementInfo();
        speed.Speed = packet.ReadFloat();
        SendPacketToClient(speed);

        // Convenience in vanilla to use SwimSpeed as FlySpeed
        if (universalOpcode is Opcode.SMSG_MOVE_UPDATE_SWIM_SPEED
                            or Opcode.SMSG_MOVE_UPDATE_SWIM_BACK_SPEED &&
            LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            var flyOpcode = (Opcode) Enum.Parse(typeof(Opcode), universalOpcode.ToString().Replace("SWIM", "FLIGHT"));
            MoveUpdateSpeed flySpeed = new MoveUpdateSpeed(flyOpcode);
            flySpeed.MoverGUID = speed.MoverGUID;
            flySpeed.MoveInfo = speed.MoveInfo;
            flySpeed.Speed = speed.Speed;
            SendPacketToClient(flySpeed);
        }
    }

    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_ROOT)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_UNROOT)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_ENABLE_GRAVITY)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_DISABLE_GRAVITY)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_SET_FEATHER_FALL)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_SET_NORMAL_FALL)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_SET_HOVER)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_UNSET_HOVER)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_SET_WATER_WALK)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_SET_LAND_WALK)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_START_SWIM)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_STOP_SWIM)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_SET_RUN_MODE)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_SET_WALK_MODE)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_SET_FLYING)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SPLINE_UNSET_FLYING)]
    internal void HandleSplineMovementMessages(WorldPacket packet)
    {
        MoveSplineSetFlag spline = new MoveSplineSetFlag(packet.GetUniversalOpcode(false));
        spline.MoverGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        SendPacketToClient(spline);
    }

    [HandlesSmsg(Opcode.SMSG_MOVE_ROOT)]
    [HandlesSmsg(Opcode.SMSG_MOVE_UNROOT)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SET_WATER_WALK)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SET_LAND_WALK)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SET_HOVERING)]
    [HandlesSmsg(Opcode.SMSG_MOVE_UNSET_HOVERING)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SET_CAN_FLY)]
    [HandlesSmsg(Opcode.SMSG_MOVE_UNSET_CAN_FLY)]
    [HandlesSmsg(Opcode.SMSG_MOVE_ENABLE_TRANSITION_BETWEEN_SWIM_AND_FLY)]
    [HandlesSmsg(Opcode.SMSG_MOVE_DISABLE_TRANSITION_BETWEEN_SWIM_AND_FLY)]
    [HandlesSmsg(Opcode.SMSG_MOVE_DISABLE_GRAVITY)]
    [HandlesSmsg(Opcode.SMSG_MOVE_ENABLE_GRAVITY)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SET_FEATHER_FALL)]
    [HandlesSmsg(Opcode.SMSG_MOVE_SET_NORMAL_FALL)]
    internal void HandleMoveForceFlagChange(WorldPacket packet)
    {
        MoveSetFlag flag = new MoveSetFlag(packet.GetUniversalOpcode(false));
        flag.MoverGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        flag.MoveCounter = packet.ReadUInt32();
        SendPacketToClient(flag);
    }

    [HandlesSmsg(Opcode.SMSG_COMPRESSED_MOVES)]
    internal void HandleCompressedMoves(WorldPacket packet)
    {
        var uncompressedSize = packet.ReadInt32();

        // Inflate hands back a pooled buffer; without the dispose the rental only comes back
        // via the finalizer.
        using WorldPacket pkt = packet.Inflate(uncompressedSize);

        while (pkt.CanRead())
        {
            var size = pkt.ReadUInt8();
            var opc = pkt.ReadUInt16();
            var data = pkt.ReadBytes((uint)(size - 2));

            var pkt2 = new WorldPacket(opc, data);
            pkt2.SetReceiveTime(pkt.GetReceivedTime());
            HandlePacket(pkt2);
        }
    }

    [HandlesSmsg(Opcode.SMSG_ON_MONSTER_MOVE)]
    [HandlesSmsg(Opcode.SMSG_MONSTER_MOVE_TRANSPORT)]
    internal void HandleMonsterMove(WorldPacket packet)
    {
        WowGuid128 guid = packet.ReadPackedGuid().To128(GetSession().GameState);
        ServerSideMovement moveSpline = new();

        if (packet.GetUniversalOpcode(false) == Opcode.SMSG_MONSTER_MOVE_TRANSPORT)
        {
            moveSpline.TransportGuid = packet.ReadPackedGuid().To128(GetSession().GameState);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                moveSpline.TransportSeat = packet.ReadInt8();
        }

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767)) // no idea when this was added exactly
            packet.ReadBool(); // "Toggle AnimTierInTrans"

        moveSpline.StartPosition = packet.ReadVector3();
        moveSpline.SplineId = packet.ReadUInt32();
        SplineTypeLegacy type = (SplineTypeLegacy)packet.ReadUInt8();
        switch (type)
        {
            case SplineTypeLegacy.FacingSpot:
            {
                moveSpline.SplineType = SplineTypeModern.FacingSpot;
                moveSpline.FinalFacingSpot = packet.ReadVector3();
                break;
            }
            case SplineTypeLegacy.FacingTarget:
            {
                moveSpline.SplineType = SplineTypeModern.FacingTarget;
                moveSpline.FinalFacingGuid = packet.ReadGuid().To128(GetSession().GameState);
                break;
            }
            case SplineTypeLegacy.FacingAngle:
            {
                moveSpline.SplineType = SplineTypeModern.FacingAngle;
                moveSpline.FinalOrientation = packet.ReadFloat();
                MovementInfo.ClampOrientation(ref moveSpline.FinalOrientation);
                break;
            }
            case SplineTypeLegacy.Stop:
            {
                moveSpline.SplineType = SplineTypeModern.None;
                MonsterMove moveStop = new MonsterMove(guid, moveSpline);
                SendPacketToClient(moveStop);
                return;
            }
        }

        bool hasAnimTier;
        bool hasTrajectory;
        bool hasCatmullRom;
        bool hasTaxiFlightFlags;
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            var splineFlags = (SplineFlagVanilla)packet.ReadUInt32();
            hasAnimTier = false;
            hasTrajectory = false;
            hasCatmullRom = splineFlags.HasAnyFlag(SplineFlagVanilla.Flying);
            hasTaxiFlightFlags = splineFlags == (SplineFlagVanilla.Runmode | SplineFlagVanilla.Flying);

            if (splineFlags == SplineFlagVanilla.Runmode) // Default spline flags used by Vanilla and TBC servers
            {
                // Modern Classic spline decoration. Unknown5/Steering/Unknown10 are required across
                // V1_14 / V2_5 / V3_4_3 — without them server-spawned creatures don't render
                // (issue #74 reopen confirmed for V1_14, c730414 for V2_5).
                moveSpline.SplineFlags = SplineFlagModern.Unknown5;
                UnitFlagsVanilla unitFlags = (UnitFlagsVanilla)GetSession().GameState.GetLegacyFieldValueUInt32(guid, UnitField.UNIT_FIELD_FLAGS);
                if (unitFlags.HasFlag(UnitFlagsVanilla.CanSwim))
                    moveSpline.SplineFlags |= SplineFlagModern.CanSwim;
                if (type == SplineTypeLegacy.Normal && !unitFlags.HasFlag(UnitFlagsVanilla.InCombat))
                    moveSpline.SplineFlags |= SplineFlagModern.Steering | SplineFlagModern.Unknown10;
            }
            else
                moveSpline.SplineFlags = splineFlags.CastFlags<SplineFlagModern>();
        }
        else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
        {
            var splineFlags = (SplineFlagTBC)packet.ReadUInt32();
            hasAnimTier = false;
            hasTrajectory = false;
            hasCatmullRom = splineFlags.HasAnyFlag(SplineFlagTBC.Flying);
            hasTaxiFlightFlags = splineFlags == (SplineFlagTBC.Runmode | SplineFlagTBC.Flying);

            if (splineFlags == SplineFlagTBC.Runmode) // Default spline flags used by Vanilla and TBC servers
            {
                // Same modern Classic decoration as the Vanilla branch — required for V1_14 / V2_5 / V3_4_3.
                moveSpline.SplineFlags = SplineFlagModern.Unknown5;
                UnitFlags unitFlags = (UnitFlags)GetSession().GameState.GetLegacyFieldValueUInt32(guid, UnitField.UNIT_FIELD_FLAGS);
                if (unitFlags.HasFlag(UnitFlags.CanSwim))
                    moveSpline.SplineFlags |= SplineFlagModern.CanSwim;
                if (type == SplineTypeLegacy.Normal && !unitFlags.HasFlag(UnitFlags.InCombat))
                    moveSpline.SplineFlags |= SplineFlagModern.Steering | SplineFlagModern.Unknown10;
            }
            else
                moveSpline.SplineFlags = splineFlags.CastFlags<SplineFlagModern>();
        }
        else
        {
            var splineFlags = (SplineFlagWotLK)packet.ReadUInt32();
            hasAnimTier = splineFlags.HasAnyFlag(SplineFlagWotLK.AnimationTier);
            hasTrajectory = splineFlags.HasAnyFlag(SplineFlagWotLK.Trajectory);
            hasCatmullRom = splineFlags.HasAnyFlag(SplineFlagWotLK.Flying | SplineFlagWotLK.CatmullRom);
            hasTaxiFlightFlags = splineFlags == (SplineFlagWotLK.WalkMode | SplineFlagWotLK.Flying);
            moveSpline.SplineFlags = splineFlags.CastFlags<SplineFlagModern>();
        }

        if (hasAnimTier)
        {
            packet.ReadUInt8(); // Animation State
            packet.ReadInt32(); // Async-time in ms
        }

        moveSpline.SplineTimeFull = packet.ReadUInt32();

        if (hasTrajectory)
        {
            packet.ReadFloat(); // Vertical Speed
            packet.ReadInt32(); // Async-time in ms
        }

        moveSpline.SplineCount = packet.ReadUInt32();

        if (hasCatmullRom)
        {
            for (var i = 0; i < moveSpline.SplineCount; i++)
            {
                Vector3 vec = packet.ReadVector3();
                moveSpline.SplinePoints.Add(vec);
            }
            moveSpline.SplineFlags |= SplineFlagModern.UncompressedPath;
        }
        else
        {
            moveSpline.EndPosition = packet.ReadVector3();

            Vector3 mid = (moveSpline.StartPosition + moveSpline.EndPosition) * 0.5f;

            for (var i = 1; i < moveSpline.SplineCount; i++)
            {
                var vec = packet.ReadPackedVector3();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    vec = mid - vec;
                else
                    vec = moveSpline.EndPosition - vec;

                moveSpline.SplinePoints.Add(vec);
            }
        }

        bool isTaxiFlight = (hasTaxiFlightFlags &&
                            (GetSession().GameState.IsWaitingForTaxiStart ||
                             Math.Abs(packet.GetReceivedTime() - GetSession().GameState.CurrentPlayerCreateTime) <= 1000) &&
                             GetSession().GameState.CurrentPlayerGuid == guid);

        if (isTaxiFlight)
        {
            // Exact sequence of packets from sniff.
            // Client instantly teleports to destination if anything is left out.

            ServerSideMovement stopSpline = new();
            stopSpline.StartPosition = moveSpline.StartPosition;
            stopSpline.SplineId = moveSpline.SplineId - 2;
            MonsterMove moveStop = new MonsterMove(guid, stopSpline);
            SendPacketToClient(moveStop);

            ControlUpdate update = new();
            update.Guid = guid;
            update.HasControl = false;
            SendPacketToClient(update);

            stopSpline.SplineId = moveSpline.SplineId - 1;
            moveStop = new MonsterMove(guid, stopSpline);
            SendPacketToClient(moveStop);

            update = new();
            update.Guid = guid;
            update.HasControl = false;
            SendPacketToClient(update);

            // Taxi-flight spline decoration. Modern Classic flags universal across V1_14 / V2_5 / V3_4_3.
            moveSpline.SplineFlags = SplineFlagModern.Flying |
                                     SplineFlagModern.CatmullRom |
                                     SplineFlagModern.CanSwim |
                                     SplineFlagModern.UncompressedPath |
                                     SplineFlagModern.Unknown5 |
                                     SplineFlagModern.Steering |
                                     SplineFlagModern.Unknown10;

            if (!hasCatmullRom && moveSpline.EndPosition != Vector3.Zero)
                moveSpline.SplinePoints.Add(moveSpline.EndPosition);
        }

        // Opt-in legacy→modern translation trace. Enable with HERMES_TRACE_MOVEMENT=1.
        if (MovementTrace.Enabled)
            Log.Print(LogType.Server,
                $"[MonsterMove/In   ] v{ModernVersion.ExpansionVersion} mover=0x{guid.Low:X} entry={guid.GetEntry()} " +
                $"legacyType={type} modernFace={moveSpline.SplineType} " +
                $"flags=0x{(uint)moveSpline.SplineFlags:X8} taxi={isTaxiFlight} " +
                $"orient={moveSpline.FinalOrientation:F3} faceGuid=0x{moveSpline.FinalFacingGuid.Low:X}");

        MonsterMove monsterMove = new MonsterMove(guid, moveSpline);

        // Monster-moves are forwarded unconditionally. A rate limit used to live here, on the
        // theory that mob-patrol volume exhausted the V3_4_3 client's per-move allocation
        // budget. Dropping splines is not a safe trade: the client keeps extrapolating the
        // last spline it received, so a dropped update renders a *wrong* trajectory rather
        // than a slightly stale one — visible as units sliding past patrol endpoints, and as
        // melee targets becoming untrackable. SMSG_MONSTER_MOVE also carries any server-driven
        // spline, not just creatures (AzerothCore's MoveSplineInit takes a Unit*), so bots and
        // charge/knockback on real players went through the same limiter.
        SendPacketToClient(monsterMove);

        if (isTaxiFlight)
        {
            if (GetSession().GameState.IsWaitingForTaxiStart)
            {
                ActivateTaxiReplyPkt taxi = new();
                taxi.Reply = ActivateTaxiReply.Ok;
                SendPacketToClient(taxi);
                GetSession().GameState.IsWaitingForTaxiStart = false;
            }
            GetSession().GameState.IsInTaxiFlight = true;
        }
    }
}
