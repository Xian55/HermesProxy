using System;
using System.Collections.Generic;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Client;

/// <summary>
/// The 4.3.4 parts of SMSG_UPDATE_OBJECT that differ from 3.3.5a: the block type numbering and
/// the movement block, which TrinityCore 4.3.4 (Object::BuildMovementUpdate) packs into a bit
/// section of presence flags and masked GUIDs followed by the values in a scrambled order.
/// </summary>
public partial class WorldClient
{
    /// <summary>4.3.4 numbers the block types without the 3.3.5a Movement and NearObjects ones.</summary>
    private static UpdateTypeLegacy ReadUpdateTypeCata(WorldPacket packet) => packet.ReadUInt8() switch
    {
        0 => UpdateTypeLegacy.Values,
        1 => UpdateTypeLegacy.CreateObject1,
        2 => UpdateTypeLegacy.CreateObject2,
        3 => UpdateTypeLegacy.FarObjects,
        var other => (UpdateTypeLegacy)(0x80 | other),
    };

    // 4.3.4 spline flags (25 bits in the create block) match the modern ones from 0x10 up to
    // UncompressedPath; Animation and Parabolic sit one bit lower than modern's.
    private const uint SplineFlagsSharedWithModern = 0x00FFFFF0;
    private const uint SplineFlagAnimationCata = 0x01000000;
    private const uint SplineFlagParabolicCata = 0x02000000;

    private static SplineFlagModern ConvertSplineFlagsCata(uint flags)
    {
        var modern = (SplineFlagModern)(flags & SplineFlagsSharedWithModern);
        if ((flags & SplineFlagAnimationCata) != 0)
            modern |= SplineFlagModern.Animation;
        if ((flags & SplineFlagParabolicCata) != 0)
            modern |= SplineFlagModern.Parabolic;
        return modern;
    }

    void ReadMovementUpdateBlockCata(WorldPacket packet, WowGuid128 guid, ObjectUpdate? updateData)
    {
        MovementInfo? moveInfo = null;
        MovementSpeeds speeds = default;
        uint transportPathTimer = 0;
        uint vehicleId = 0;
        float vehicleOrientation = 0f;
        Quaternion? rotation = null;

        packet.HasBit();                                    // PlayerHoverAnim
        packet.HasBit();                                    // SupressedGreetings
        bool hasRotation = packet.HasBit();
        bool hasAnimKit = packet.HasBit();
        bool hasCombatVictim = packet.HasBit();
        bool thisIsYou = packet.HasBit();
        bool hasVehicle = packet.HasBit();
        bool hasMovementUpdate = packet.HasBit();
        uint pauseTimeCount = packet.ReadBits<uint>(24);
        packet.HasBit();                                    // NoBirthAnim
        bool hasMovementTransport = packet.HasBit();
        bool hasStationary = packet.HasBit();
        bool hasAreaTrigger = packet.HasBit();
        packet.HasBit();                                    // EnablePortals
        bool hasServerTime = packet.HasBit();

        if (thisIsYou || (ModernVersion.IsWotLKClassicOrLater && guid == GetSession().GameState.CurrentPlayerGuid))
        {
            if (updateData != null)
                updateData.CreateData.ThisIsYou = true;
            GetSession().GameState.CurrentPlayerCreateTime = packet.GetReceivedTime();
        }

        Span<bool> moverMask = stackalloc bool[8];
        Span<bool> transportMask = stackalloc bool[8];
        Span<bool> goTransportMask = stackalloc bool[8];
        Span<bool> victimMask = stackalloc bool[8];
        Span<bool> facingMask = stackalloc bool[8];
        uint movementFlags = 0;
        uint movementFlagsExtra = 0;
        bool hasOrientation = false, hasPitch = false, hasSpline = false, hasFallData = false;
        bool hasSplineElevation = false, hasTransport = false, hasTime = false, hasTransportTime2 = false;
        bool hasVehicleRecId = false, hasFallDirection = false, hasSplineMove = false;
        bool hasEffectStartTime = false, hasVerticalAcceleration = false;
        uint splineMode = 0, splinePointCount = 0, splineFacing = 3, splineFlags = 0;
        bool hasGoTransportTime3 = false, hasGoTransportTime2 = false;
        bool hasAiAnimKit = false, hasMovementAnimKit = false, hasMeleeAnimKit = false;

        if (hasMovementUpdate)
        {
            bool hasMovementFlags = !packet.HasBit();
            hasOrientation = !packet.HasBit();
            moverMask[7] = packet.HasBit();
            moverMask[3] = packet.HasBit();
            moverMask[2] = packet.HasBit();
            if (hasMovementFlags)
                movementFlags = packet.ReadBits<uint>(30);

            packet.HasBit();                                // has spline data, creatures only
            hasPitch = !packet.HasBit();
            hasSpline = packet.HasBit();
            hasFallData = packet.HasBit();
            hasSplineElevation = !packet.HasBit();
            moverMask[5] = packet.HasBit();
            hasTransport = packet.HasBit();
            hasTime = !packet.HasBit();

            if (hasTransport)
            {
                transportMask[1] = packet.HasBit();
                hasTransportTime2 = packet.HasBit();
                transportMask[4] = packet.HasBit();
                transportMask[0] = packet.HasBit();
                transportMask[6] = packet.HasBit();
                hasVehicleRecId = packet.HasBit();
                transportMask[7] = packet.HasBit();
                transportMask[5] = packet.HasBit();
                transportMask[3] = packet.HasBit();
                transportMask[2] = packet.HasBit();
            }

            moverMask[4] = packet.HasBit();

            if (hasSpline)
            {
                hasSplineMove = packet.HasBit();
                if (hasSplineMove)
                {
                    splineMode = packet.ReadBits<uint>(2);
                    hasEffectStartTime = packet.HasBit();
                    splinePointCount = packet.ReadBits<uint>(22);
                    splineFacing = packet.ReadBits<uint>(2);
                    if (splineFacing == 2)
                        MaskedGuid.ReadMaskBits(packet, facingMask, [4, 3, 7, 2, 6, 1, 0, 5]);
                    hasVerticalAcceleration = packet.HasBit();
                    splineFlags = packet.ReadBits<uint>(25);
                }
            }

            moverMask[6] = packet.HasBit();
            if (hasFallData)
                hasFallDirection = packet.HasBit();
            moverMask[0] = packet.HasBit();
            moverMask[1] = packet.HasBit();
            packet.HasBit();                                // HeightChangeFailed
            if (!packet.HasBit())
                movementFlagsExtra = packet.ReadBits<uint>(12);
        }

        if (hasMovementTransport)
        {
            goTransportMask[5] = packet.HasBit();
            hasGoTransportTime3 = packet.HasBit();
            goTransportMask[0] = packet.HasBit();
            goTransportMask[3] = packet.HasBit();
            goTransportMask[6] = packet.HasBit();
            goTransportMask[1] = packet.HasBit();
            goTransportMask[4] = packet.HasBit();
            goTransportMask[2] = packet.HasBit();
            hasGoTransportTime2 = packet.HasBit();
            goTransportMask[7] = packet.HasBit();
        }

        if (hasCombatVictim)
            MaskedGuid.ReadMaskBits(packet, victimMask, [2, 7, 0, 4, 5, 6, 1, 3]);

        if (hasAnimKit)
        {
            hasAiAnimKit = !packet.HasBit();
            hasMovementAnimKit = !packet.HasBit();
            hasMeleeAnimKit = !packet.HasBit();
        }

        packet.ResetBitPos();

        for (uint i = 0; i < pauseTimeCount; i++)
            packet.ReadUInt32();

        if (hasMovementUpdate)
        {
            Span<byte> mover = stackalloc byte[8];
            var living = new MovementInfo { Flags = (MovementFlagModern)movementFlags, FlagsExtra = movementFlagsExtra };
            float x, y, z;

            MaskedGuid.ReadByte(packet, moverMask, mover, 4);
            speeds.RunBack = packet.ReadFloat();
            if (hasFallData)
            {
                if (hasFallDirection)
                {
                    living.JumpHorizontalSpeed = packet.ReadFloat();
                    living.JumpSinAngle = packet.ReadFloat();
                    living.JumpCosAngle = packet.ReadFloat();
                }
                living.FallTime = packet.ReadUInt32();
                living.JumpVerticalSpeed = packet.ReadFloat();
            }

            speeds.SwimBack = packet.ReadFloat();
            if (hasSplineElevation)
                living.SplineElevation = packet.ReadFloat();

            if (hasSpline)
            {
                living.HasSplineData = true;
                ServerSideMovement monsterMove = ReadCreateSplineCata(packet, hasSplineMove, hasVerticalAcceleration, hasEffectStartTime,
                    splineMode, splinePointCount, splineFacing, splineFlags, facingMask);
                if (updateData != null)
                    updateData.CreateData.MoveSpline = monsterMove;
            }

            z = packet.ReadFloat();
            MaskedGuid.ReadByte(packet, moverMask, mover, 5);

            if (hasTransport)
            {
                Span<byte> transport = stackalloc byte[8];
                var info = new TransportInfo();
                MaskedGuid.ReadByte(packet, transportMask, transport, 5);
                MaskedGuid.ReadByte(packet, transportMask, transport, 7);
                info.Time = packet.ReadUInt32();
                info.Orientation = packet.ReadFloat();
                if (hasTransportTime2)
                    info.PrevTime = packet.ReadUInt32();
                float ty = packet.ReadFloat();
                float tx = packet.ReadFloat();
                MaskedGuid.ReadByte(packet, transportMask, transport, 3);
                float tz = packet.ReadFloat();
                MaskedGuid.ReadByte(packet, transportMask, transport, 0);
                if (hasVehicleRecId)
                    info.VehicleId = packet.ReadUInt32();
                info.Seat = packet.ReadInt8();
                MaskedGuid.ReadBytes(packet, transportMask, transport, [1, 6, 2, 4]);
                info.Offset = new Vector3(tx, ty, tz);
                info.Guid = new WowGuid64(MaskedGuid.ToUInt64(transport)).To128(GetSession().GameState);
                living.Transport = info;
            }

            x = packet.ReadFloat();
            speeds.PitchRate = packet.ReadFloat();
            MaskedGuid.ReadBytes(packet, moverMask, mover, [3, 0]);
            speeds.Swim = packet.ReadFloat();
            y = packet.ReadFloat();
            MaskedGuid.ReadBytes(packet, moverMask, mover, [7, 1, 2]);
            speeds.Walk = packet.ReadFloat();
            if (hasTime)
                living.MoveTime = packet.ReadUInt32();
            speeds.TurnRate = packet.ReadFloat();
            MaskedGuid.ReadByte(packet, moverMask, mover, 6);
            speeds.Flight = packet.ReadFloat();
            if (hasOrientation)
                living.Orientation = packet.ReadFloat();
            speeds.Run = packet.ReadFloat();
            if (hasPitch)
                living.SwimPitch = packet.ReadFloat();
            speeds.FlightBack = packet.ReadFloat();

            living.Position = new Vector3(x, y, z);
            moveInfo = living;
        }

        if (hasVehicle)
        {
            vehicleOrientation = packet.ReadFloat();
            vehicleId = packet.ReadUInt32();
            GetSession().GameState.SetVehicleRecId(guid, vehicleId);
        }

        if (hasMovementTransport)
        {
            Span<byte> transport = stackalloc byte[8];
            var info = new TransportInfo();
            MaskedGuid.ReadBytes(packet, goTransportMask, transport, [0, 5]);
            if (hasGoTransportTime3)
                info.VehicleId = packet.ReadUInt32();
            MaskedGuid.ReadByte(packet, goTransportMask, transport, 3);
            float tx = packet.ReadFloat();
            MaskedGuid.ReadBytes(packet, goTransportMask, transport, [4, 6, 1]);
            info.Time = packet.ReadUInt32();
            float ty = packet.ReadFloat();
            MaskedGuid.ReadBytes(packet, goTransportMask, transport, [2, 7]);
            float tz = packet.ReadFloat();
            info.Seat = packet.ReadInt8();
            info.Orientation = packet.ReadFloat();
            if (hasGoTransportTime2)
                info.PrevTime = packet.ReadUInt32();
            info.Offset = new Vector3(tx, ty, tz);
            info.Guid = new WowGuid64(MaskedGuid.ToUInt64(transport)).To128(GetSession().GameState);

            var placed = moveInfo ?? new MovementInfo();
            placed.Transport = info;
            moveInfo = placed;
        }

        if (hasRotation)
            rotation = packet.ReadPackedQuaternion();

        if (hasAreaTrigger)
        {
            // Sixteen floats with a byte after the fourth; the client does not use them.
            for (int i = 0; i < 16; i++)
            {
                packet.ReadFloat();
                if (i == 3)
                    packet.ReadUInt8();
            }
        }

        if (hasStationary)
        {
            float orientation = packet.ReadFloat();
            Vector3 position = packet.ReadVector3();
            var placed = moveInfo ?? new MovementInfo();
            placed.Position = position;
            placed.Orientation = orientation;
            moveInfo = placed;
        }

        if (hasCombatVictim)
        {
            Span<byte> victim = stackalloc byte[8];
            MaskedGuid.ReadBytes(packet, victimMask, victim, [4, 0, 3, 5, 7, 6, 2, 1]);
            if (updateData != null)
                updateData.CreateData.AutoAttackVictim = new WowGuid64(MaskedGuid.ToUInt64(victim)).To128(GetSession().GameState);
        }

        if (hasAnimKit)
        {
            if (hasAiAnimKit)
                packet.ReadUInt16();
            if (hasMovementAnimKit)
                packet.ReadUInt16();
            if (hasMeleeAnimKit)
                packet.ReadUInt16();
        }

        if (hasServerTime)
            transportPathTimer = packet.ReadUInt32();

        if (updateData != null && moveInfo is { } read)
        {
            MovementSanitizer.Sanitize(ref read);
            var create = updateData.CreateData;
            create.MoveInfo = read;
            create.Speeds = speeds;
            create.TransportPathTimer = transportPathTimer;
            create.VehicleId = vehicleId;
            create.VehicleOrientation = vehicleOrientation;
            if (rotation is { } sentRotation)
                create.Rotation = sentRotation;
        }
    }

    /// <summary>
    /// The spline of a create block (TrinityCore 4.3.4 MovementPacketBuilder::WriteCreateData).
    /// Points come as z, x, y; a stopped spline carries only its end point and id.
    /// </summary>
    private ServerSideMovement ReadCreateSplineCata(WorldPacket packet, bool hasSplineMove, bool hasVerticalAcceleration,
        bool hasEffectStartTime, uint splineMode, uint pointCount, uint facing, uint flags, ReadOnlySpan<bool> facingMask)
    {
        var monsterMove = new ServerSideMovement
        {
            SplineFlags = ConvertSplineFlagsCata(flags),
            SplineMode = (byte)splineMode,
            SplinePoints = new List<Vector3>((int)Math.Min(pointCount, 1024u)),
            TransportSeat = -1,
        };

        if (hasSplineMove)
        {
            if (hasVerticalAcceleration)
                packet.ReadFloat();
            monsterMove.SplineTime = packet.ReadUInt32();

            if (facing == 0)
            {
                monsterMove.FinalOrientation = packet.ReadFloat();
                MovementSanitizer.ClampOrientation(ref monsterMove.FinalOrientation);
                monsterMove.SplineType = SplineTypeModern.FacingAngle;
            }
            else if (facing == 2)
            {
                Span<byte> target = stackalloc byte[8];
                MaskedGuid.ReadBytes(packet, facingMask, target, [5, 3, 7, 1, 6, 4, 2, 0]);
                monsterMove.FinalFacingGuid = new WowGuid64(MaskedGuid.ToUInt64(target)).To128(GetSession().GameState);
                monsterMove.SplineType = SplineTypeModern.FacingTarget;
            }

            for (uint i = 0; i < pointCount; i++)
            {
                float z = packet.ReadFloat();
                float x = packet.ReadFloat();
                float y = packet.ReadFloat();
                monsterMove.SplinePoints.Add(new Vector3(x, y, z));
            }
            monsterMove.SplineCount = pointCount;

            if (facing == 1)
            {
                float fx = packet.ReadFloat();
                float fz = packet.ReadFloat();
                float fy = packet.ReadFloat();
                monsterMove.FinalFacingSpot = new Vector3(fx, fy, fz);
                monsterMove.SplineType = SplineTypeModern.FacingSpot;
            }

            packet.ReadFloat();                             // duration multiplier next
            monsterMove.SplineTimeFull = packet.ReadUInt32();
            if (hasEffectStartTime)
                packet.ReadUInt32();
            packet.ReadFloat();                             // duration multiplier
        }

        float ez = packet.ReadFloat();
        float ex = packet.ReadFloat();
        float ey = packet.ReadFloat();
        monsterMove.EndPosition = new Vector3(ex, ey, ez);
        monsterMove.SplineId = packet.ReadUInt32();
        return monsterMove;
    }
}
