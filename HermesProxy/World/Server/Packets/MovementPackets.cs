/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Framework.Constants;
using Framework.GameMath;
using Framework.IO;
using Framework.Logging;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

/// <remarks>
/// Not a positional record like the other inbound packets. A movement block is 136 bytes and this
/// is the highest-rate packet a client sends, so the codec reads the block straight into
/// <see cref="MoveInfo"/> rather than building it beside the packet and copying it in, and the
/// handler takes it by reference. Each of those copies measured about 3 ns on a 60 ns packet.
/// </remarks>
public struct ClientPlayerMovement
{
    public WowGuid128 Guid;
    public MovementInfo MoveInfo;
}
public class MoveUpdate : ServerPacket, ISpanWritable
{
    public MoveUpdate() : base(Opcode.SMSG_MOVE_UPDATE, ConnectionType.Instance) { }

    public override void Write()
    {
        ModernMovementCodec.Write(_worldPacket, MoverGUID, in MoveInfo);
    }

    public int MaxSize => ModernMovementCodec.MaxSize;

    public int WriteToSpan(Span<byte> buffer)
    {
        return ModernMovementCodec.Write(buffer, MoverGUID, in MoveInfo);
    }

    public WowGuid128 MoverGUID;
    public MovementInfo MoveInfo;
}

public class MonsterMove : ServerPacket, ISpanWritable
{
    // Practical cap for spline points - covers real-world movement patterns
    // Corruption guard, not a sizing cap. SplineCount is read straight off the legacy wire
    // (MovementHandler.cs:563) and is unbounded, so a garbage count must not turn into a
    // huge ArrayPool rent; beyond this WriteToSpan bails to the ByteBuffer path. MaxSize
    // below sizes the rent from the spline this packet actually carries, so a normal path
    // of any length still takes the span path.
    //
    // A previous fixed cap of 16 (reduced from 64 "based on actual usage data") was measured
    // against light traffic. Under a bot-populated battleground it missed 370 times in ten
    // minutes, and every miss rented a pooled buffer it discarded, re-wrote the packet through
    // ByteBuffer, and emitted a Warn-level log on a per-packet path.
    private const int MaxSplinePoints = 4096;

    public MonsterMove(WowGuid128 guid, ServerSideMovement moveSpline) : base(Opcode.SMSG_ON_MONSTER_MOVE, ConnectionType.Instance)
    {
        if (moveSpline.SplineFlags.HasFlag(SplineFlagModern.UncompressedPath))
        {
            _layout = moveSpline.SplineFlags.HasFlag(SplineFlagModern.Cyclic) ? PointLayout.CyclicPath : PointLayout.Path;
            _pathHasEnd = moveSpline.EndPosition != Vector3.Zero;
        }
        // An end position of zero means "none", except on a transport: the seat a passenger is
        // moved to is the origin of the vehicle it boards. A native 3.4.3 server sends that as
        // one point at (0,0,0); without it every boarding and seat change went out pointless.
        // SplineCount is zero for a stop, which has no end to send.
        else if (moveSpline.EndPosition != Vector3.Zero ||
                 (moveSpline.TransportGuid != default && moveSpline.SplineCount > 0))
            _layout = moveSpline.SplinePoints.Count > 0 ? PointLayout.EndWithDeltas : PointLayout.End;

        MoverGUID = guid;
        MoveSpline = moveSpline;
    }

    // Which of the spline's positions go on the wire, and how, is decided here once. The positions
    // themselves are read from MoveSpline as the packet is written: copying them into a points
    // list and a deltas list first cost two lists and their arrays per packet, ~15 MB over an
    // 18-minute Alterac Valley.
    private enum PointLayout : byte
    {
        None,
        Path,          // the spline's points, then the end position when there is one
        CyclicPath,    // the end position when there is one, then the spline's points
        End,           // the end position alone
        EndWithDeltas, // the end position, each spline point packed as a delta from the midpoint
    }

    private readonly PointLayout _layout;
    private readonly bool _pathHasEnd;

    public int PointCount => _layout switch
    {
        PointLayout.Path or PointLayout.CyclicPath => MoveSpline.SplinePoints.Count + (_pathHasEnd ? 1 : 0),
        PointLayout.End or PointLayout.EndWithDeltas => 1,
        _ => 0,
    };

    public Vector3 Point(int index) => _layout switch
    {
        PointLayout.Path => index < MoveSpline.SplinePoints.Count ? MoveSpline.SplinePoints[index] : MoveSpline.EndPosition,
        PointLayout.CyclicPath when _pathHasEnd => index == 0 ? MoveSpline.EndPosition : MoveSpline.SplinePoints[index - 1],
        PointLayout.CyclicPath => MoveSpline.SplinePoints[index],
        _ => MoveSpline.EndPosition,
    };

    public int PackedDeltaCount => _layout == PointLayout.EndWithDeltas ? MoveSpline.SplinePoints.Count : 0;

    public Vector3 PackedDelta(int index) =>
        (MoveSpline.StartPosition + MoveSpline.EndPosition) / 2.0f - MoveSpline.SplinePoints[index];

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(MoverGUID);
        _worldPacket.WriteVector3(MoveSpline.StartPosition);

        _worldPacket.WriteUInt32(MoveSpline.SplineId);
        _worldPacket.WriteVector3(Vector3.Zero); // Destination
        _worldPacket.WriteBit(false); // CrzTeleport
        _worldPacket.WriteBits(PointCount == 0 ? 2 : 0, 3); // StopDistanceTolerance

        _worldPacket.WriteUInt32((uint)MoveSpline.SplineFlags);
        _worldPacket.WriteInt32(0); // Elapsed
        _worldPacket.WriteUInt32(MoveSpline.SplineTimeFull);
        _worldPacket.WriteUInt32(0); // FadeObjectTime
        _worldPacket.WriteUInt8(MoveSpline.SplineMode);
        _worldPacket.WritePackedGuid128(MoveSpline.TransportGuid); // != default ? MoveSpline.TransportGuid : WowGuid128.Empty
        _worldPacket.WriteInt8(MoveSpline.TransportSeat);
        _worldPacket.WriteBits((byte)MoveSpline.SplineType, 2);
        _worldPacket.WriteBits(PointCount, 16);
        _worldPacket.WriteBit(false); // VehicleExitVoluntary ;
        _worldPacket.WriteBit(false); // Interpolate
        _worldPacket.WriteBits(PackedDeltaCount, 16);
        _worldPacket.WriteBit(false); // SplineFilter.HasValue
        _worldPacket.WriteBit(false); // SpellEffectExtraData.HasValue
        _worldPacket.WriteBit(false); // JumpExtraData.HasValue
        _worldPacket.FlushBits();

        //if (SplineFilter.HasValue)
        //    SplineFilter.Value.Write(data);

        switch (MoveSpline.SplineType)
        {
            case SplineTypeModern.FacingSpot:
                _worldPacket.WriteVector3(MoveSpline.FinalFacingSpot);
                break;
            case SplineTypeModern.FacingTarget:
                // Universal modern Classic wire: float FaceDirection + PackedGuid128 FaceGUID.
                // Matches TrinityCore wotlk_classic src/server/game/Server/Packets/MovementPackets.cpp
                // (operator<< for MovementSpline, MONSTER_MOVE_FACING_TARGET case). Applies to V1_14,
                // V2_5, V3_4_3 — dropping the float corrupts FaceGUID + every subsequent point by
                // 4 bytes on the client side (issue #74 reopen, modern_*_parsed.txt confirms WPP
                // ArgumentOutOfRangeException on FacingGUID high-byte after FaceDirection read).
                _worldPacket.WriteFloat(MoveSpline.FinalOrientation);
                _worldPacket.WritePackedGuid128(MoveSpline.FinalFacingGuid);
                break;
            case SplineTypeModern.FacingAngle:
                _worldPacket.WriteFloat(MoveSpline.FinalOrientation);
                break;
        }

        for (int i = 0; i < PointCount; i++)
            _worldPacket.WriteVector3(Point(i));

        for (int i = 0; i < PackedDeltaCount; i++)
            _worldPacket.WritePackXYZ(PackedDelta(i));

        /*
        if (SpellEffectExtraData.HasValue)
            SpellEffectExtraData.Value.Write(data);

        if (JumpExtraData.HasValue)
            JumpExtraData.Value.Write(data);
        */

        // Opt-in wire trace. Enable with HERMES_TRACE_MOVEMENT=1 for cross-OS / cross-version
        // packet comparison (issue #74 reopen, macOS vs Windows V1_14_x divergence).
        if (MovementTrace.Enabled)
            Log.Print(LogType.Server,
                $"[MonsterMove/Write] v{ModernVersion.ExpansionVersion} mover=0x{MoverGUID.Low:X} entry={MoverGUID.GetEntry()} " +
                $"face={MoveSpline.SplineType} flags=0x{(uint)MoveSpline.SplineFlags:X8} mode={MoveSpline.SplineMode} " +
                $"pts={PointCount} deltas={PackedDeltaCount} " +
                $"orient={MoveSpline.FinalOrientation:F3} faceGuid=0x{MoveSpline.FinalFacingGuid.Low:X} " +
                $"wire={_worldPacket.GetSize()}B");
    }

    // Fixed: GUID(18) + StartPos(12) + SplineId(4) + Dest(12) + flags/times(36) + bits(6) = 88
    // SplineType FacingTarget (worst case, V2_5+): float(4) + GUID(18) = 22
    private const int FixedSize = 88 + 22; // 110 bytes

    // Sized from this packet's own spline: points write as Vector3 (12 B), packed deltas as
    // PackXYZ (4 B) -- see WriteToSpan. Both counts are known from the spline before
    // WritePacketData rents. ArrayPool rounds the rent up to its bucket, so
    // exact sizing costs nothing versus a constant and never under-provisions.
    public int MaxSize => FixedSize + PointCount * 12 + PackedDeltaCount * 4;

    public int WriteToSpan(Span<byte> buffer)
    {
        // Only a corrupt or hostile SplineCount should land here; MaxSize already sized the
        // buffer for this spline's real length.
        if (PointCount > MaxSplinePoints || PackedDeltaCount > MaxSplinePoints)
            return -1;

        var writer = new SpanPacketWriter(buffer);

        writer.WritePackedGuid128(MoverGUID.Low, MoverGUID.High);
        writer.WriteVector3(MoveSpline.StartPosition);

        writer.WriteUInt32(MoveSpline.SplineId);
        writer.WriteVector3(Vector3.Zero); // Destination
        writer.WriteBit(false); // CrzTeleport
        writer.WriteBits((uint)(PointCount == 0 ? 2 : 0), 3); // StopDistanceTolerance

        writer.WriteUInt32((uint)MoveSpline.SplineFlags);
        writer.WriteInt32(0); // Elapsed
        writer.WriteUInt32(MoveSpline.SplineTimeFull);
        writer.WriteUInt32(0); // FadeObjectTime
        writer.WriteUInt8(MoveSpline.SplineMode);
        writer.WritePackedGuid128(MoveSpline.TransportGuid.Low, MoveSpline.TransportGuid.High);
        writer.WriteInt8(MoveSpline.TransportSeat);
        writer.WriteBits((uint)MoveSpline.SplineType, 2);
        writer.WriteBits((uint)PointCount, 16);
        writer.WriteBit(false); // VehicleExitVoluntary
        writer.WriteBit(false); // Interpolate
        writer.WriteBits((uint)PackedDeltaCount, 16);
        writer.WriteBit(false); // SplineFilter.HasValue
        writer.WriteBit(false); // SpellEffectExtraData.HasValue
        writer.WriteBit(false); // JumpExtraData.HasValue
        writer.FlushBits();

        switch (MoveSpline.SplineType)
        {
            case SplineTypeModern.FacingSpot:
                writer.WriteVector3(MoveSpline.FinalFacingSpot);
                break;
            case SplineTypeModern.FacingTarget:
                // See Write() above — universal modern Classic layout. TC wotlk_classic ground truth.
                writer.WriteFloat(MoveSpline.FinalOrientation);
                writer.WritePackedGuid128(MoveSpline.FinalFacingGuid.Low, MoveSpline.FinalFacingGuid.High);
                break;
            case SplineTypeModern.FacingAngle:
                writer.WriteFloat(MoveSpline.FinalOrientation);
                break;
        }

        for (int i = 0; i < PointCount; i++)
            writer.WriteVector3(Point(i));

        for (int i = 0; i < PackedDeltaCount; i++)
            writer.WritePackXYZ(PackedDelta(i));

        // Opt-in wire trace — see Write() above.
        if (MovementTrace.Enabled)
            Log.Print(LogType.Server,
                $"[MonsterMove/Span ] v{ModernVersion.ExpansionVersion} mover=0x{MoverGUID.Low:X} entry={MoverGUID.GetEntry()} " +
                $"face={MoveSpline.SplineType} flags=0x{(uint)MoveSpline.SplineFlags:X8} mode={MoveSpline.SplineMode} " +
                $"pts={PointCount} deltas={PackedDeltaCount} " +
                $"orient={MoveSpline.FinalOrientation:F3} faceGuid=0x{MoveSpline.FinalFacingGuid.Low:X} " +
                $"wire={writer.Position}B");

        return writer.Position;
    }

    public WowGuid128 MoverGUID;
    public ServerSideMovement MoveSpline;
}

public readonly record struct MoveTeleportAck(WowGuid128 MoverGUID, uint MoveCounter, uint MoveTime);

public class MoveTeleport : ServerPacket, ISpanWritable
{
    public MoveTeleport() : base(Opcode.SMSG_MOVE_TELEPORT, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(MoverGUID);
        _worldPacket.WriteUInt32(MoveCounter);
        _worldPacket.WriteVector3(Position);
        _worldPacket.WriteFloat(Orientation);
        _worldPacket.WriteUInt8(PreloadWorld);

        _worldPacket.WriteBit(TransportGUID != default);
        _worldPacket.WriteBit(Vehicle != null);
        _worldPacket.FlushBits();

        if (Vehicle != null)
        {
            _worldPacket.WriteInt8(Vehicle.VehicleSeatIndex);
            _worldPacket.WriteBit(Vehicle.VehicleExitVoluntary);
            _worldPacket.WriteBit(Vehicle.VehicleExitTeleport);
            _worldPacket.FlushBits();
        }

        if (TransportGUID != default)
            _worldPacket.WritePackedGuid128(TransportGUID);
    }

    // MaxSize: GUID (18) + uint (4) + Vector3 (12) + float (4) + byte (1) + bits (1) + Vehicle (2) + TransportGUID (18) = 60
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 4 + 12 + 4 + 1 + 1 + 2 + PackedGuidHelper.MaxPackedGuid128Size;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(MoverGUID.Low, MoverGUID.High);
        writer.WriteUInt32(MoveCounter);
        writer.WriteVector3(Position);
        writer.WriteFloat(Orientation);
        writer.WriteUInt8(PreloadWorld);

        writer.WriteBit(TransportGUID != default);
        writer.WriteBit(Vehicle != null);
        writer.FlushBits();

        if (Vehicle != null)
        {
            writer.WriteInt8(Vehicle.VehicleSeatIndex);
            writer.WriteBit(Vehicle.VehicleExitVoluntary);
            writer.WriteBit(Vehicle.VehicleExitTeleport);
            writer.FlushBits();
        }

        if (TransportGUID != default)
            writer.WritePackedGuid128(TransportGUID.Low, TransportGUID.High);

        return writer.Position;
    }

    public Vector3 Position;
    public VehicleTeleport Vehicle = null!;
    public uint MoveCounter;
    public WowGuid128 MoverGUID;
    public WowGuid128 TransportGUID;
    public float Orientation;
    public byte PreloadWorld;
}

public class VehicleTeleport
{
    public sbyte VehicleSeatIndex;
    public bool VehicleExitVoluntary;
    public bool VehicleExitTeleport;
}

public class TransferPending : ServerPacket, ISpanWritable
{
    public TransferPending() : base(Opcode.SMSG_TRANSFER_PENDING) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(MapID);
        _worldPacket.WriteVector3(OldMapPosition);
        _worldPacket.WriteBit(Ship != null);
        _worldPacket.WriteBit(TransferSpellID.HasValue);

        if (Ship != null)
        {
            _worldPacket.WriteUInt32(Ship.Id);
            _worldPacket.WriteInt32(Ship.OriginMapID);
        }

        if (TransferSpellID.HasValue)
            _worldPacket.WriteInt32(TransferSpellID.Value);

        _worldPacket.FlushBits();
    }

    // MaxSize: uint (4) + Vector3 (12) + bits (1) + Ship (8) + TransferSpellID (4) = 29
    public int MaxSize => 4 + 12 + 1 + 8 + 4;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(MapID);
        writer.WriteVector3(OldMapPosition);
        writer.WriteBit(Ship != null);
        writer.WriteBit(TransferSpellID.HasValue);

        if (Ship != null)
        {
            writer.WriteUInt32(Ship.Id);
            writer.WriteInt32(Ship.OriginMapID);
        }

        if (TransferSpellID.HasValue)
            writer.WriteInt32(TransferSpellID.Value);

        writer.FlushBits();
        return writer.Position;
    }

    public uint MapID;
    public Vector3 OldMapPosition;
    public ShipTransferPending Ship = null!;
    public int? TransferSpellID;

    public class ShipTransferPending
    {
        public uint Id;              // gameobject_template.entry of the transport the player is teleporting on
        public int OriginMapID;     // Map id the player is currently on (before teleport)
    }
}

public class TransferAborted : ServerPacket, ISpanWritable
{
    public TransferAborted() : base(Opcode.SMSG_TRANSFER_ABORTED) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(MapID);
        _worldPacket.WriteUInt8(Arg);
        _worldPacket.WriteInt32(MapDifficultyXConditionID);
        _worldPacket.WriteBits(Reason, 6);
        _worldPacket.FlushBits();
    }

    public int MaxSize => 10; // uint + byte + int + 1 byte for 6 bits

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(MapID);
        writer.WriteUInt8(Arg);
        writer.WriteInt32(MapDifficultyXConditionID);
        writer.WriteBits((uint)Reason, 6);
        writer.FlushBits();
        return writer.Position;
    }

    public uint MapID;
    public byte Arg;
    public int MapDifficultyXConditionID = -6;
    public TransferAbortReasonModern Reason;
}

public class NewWorld : ServerPacket, ISpanWritable
{
    public NewWorld() : base(Opcode.SMSG_NEW_WORLD) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(MapID);
        _worldPacket.WriteVector3(Position);
        _worldPacket.WriteFloat(Orientation);
        _worldPacket.WriteUInt32(Reason);
        _worldPacket.WriteVector3(MovementOffset);
    }

    public int MaxSize => 36; // 2 uint + 2 Vector3 (24 bytes) + float = 36

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(MapID);
        writer.WriteVector3(Position);
        writer.WriteFloat(Orientation);
        writer.WriteUInt32(Reason);
        writer.WriteVector3(MovementOffset);
        return writer.Position;
    }

    public uint MapID;
    public uint Reason;
    public Vector3 Position = new();
    public float Orientation;
    public Vector3 MovementOffset;    // Adjusts all pending movement events by this offset
}

public readonly record struct WorldPortResponse;

// for server controlled units
public class MoveSplineSetSpeed : ServerPacket, ISpanWritable
{
    public MoveSplineSetSpeed(Opcode opcode) : base(opcode, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(MoverGUID);
        _worldPacket.WriteFloat(Speed);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 4; // GUID + float

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(MoverGUID.Low, MoverGUID.High);
        writer.WriteFloat(Speed);
        return writer.Position;
    }

    public WowGuid128 MoverGUID;
    public float Speed = 1.0f;
}

// for own player
public class MoveSetSpeed : ServerPacket, ISpanWritable
{
    public MoveSetSpeed(Opcode opcode) : base(opcode, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(MoverGUID);
        _worldPacket.WriteUInt32(MoveCounter);
        _worldPacket.WriteFloat(Speed);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 8; // GUID + uint + float

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(MoverGUID.Low, MoverGUID.High);
        writer.WriteUInt32(MoveCounter);
        writer.WriteFloat(Speed);
        return writer.Position;
    }

    public WowGuid128 MoverGUID;
    public uint MoveCounter = 0;
    public float Speed = 1.0f;
}

public readonly record struct MovementSpeedAck(WowGuid128 MoverGUID, MovementAck Ack, float Speed);

public struct MovementAck
{
    // Read lives in MovementAckCodec now; this had no callers left once the packets
    // holding it converted.

    public MovementInfo MoveInfo;
    public uint MoveCounter;
}

// for other players
public class MoveUpdateSpeed : ServerPacket, ISpanWritable
{
    public MoveUpdateSpeed(Opcode opcode) : base(opcode, ConnectionType.Instance) { }

    public override void Write()
    {
        ModernMovementCodec.Write(_worldPacket, MoverGUID, in MoveInfo);
        _worldPacket.WriteFloat(Speed);
    }

    public int MaxSize => ModernMovementCodec.MaxSize + 4; // MovementInfo + float

    public int WriteToSpan(Span<byte> buffer)
    {
        int written = ModernMovementCodec.Write(buffer, MoverGUID, in MoveInfo);
        var writer = new SpanPacketWriter(buffer.Slice(written));
        writer.WriteFloat(Speed);
        return written + writer.Position;
    }

    public WowGuid128 MoverGUID;
    public MovementInfo MoveInfo;
    public float Speed = 1.0f;
}

public class MoveSplineSetFlag : ServerPacket, ISpanWritable
{
    public MoveSplineSetFlag(Opcode opcode) : base(opcode, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(MoverGUID);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size; // Just GUID

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(MoverGUID.Low, MoverGUID.High);
        return writer.Position;
    }

    public WowGuid128 MoverGUID;
}

public class MoveSetFlag : ServerPacket, ISpanWritable
{
    public MoveSetFlag(Opcode opcode) : base(opcode, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(MoverGUID);
        _worldPacket.WriteUInt32(MoveCounter);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 4; // GUID + uint

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(MoverGUID.Low, MoverGUID.High);
        writer.WriteUInt32(MoveCounter);
        return writer.Position;
    }

    public WowGuid128 MoverGUID;
    public uint MoveCounter = 0;
}

public readonly record struct MovementAckMessage(WowGuid128 MoverGUID, MovementAck Ack);

public sealed class OnCancelExpectedRideVehicleAura : ServerPacket, ISpanWritable
{
    public OnCancelExpectedRideVehicleAura()
        : base(Opcode.SMSG_ON_CANCEL_EXPECTED_RIDE_VEHICLE_AURA, ConnectionType.Instance) { }

    public override void Write() { }

    public int MaxSize => 0;

    public int WriteToSpan(Span<byte> buffer) => 0;
}

public sealed class SetVehicleRecID : ServerPacket
{
    public SetVehicleRecID() : base(Opcode.SMSG_SET_VEHICLE_REC_ID, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(VehicleGUID);
        _worldPacket.WriteUInt32(VehicleRecID);
    }

    public WowGuid128 VehicleGUID;
    public uint VehicleRecID;
}

public sealed class MoveSetVehicleRecID : ServerPacket
{
    public MoveSetVehicleRecID() : base(Opcode.SMSG_MOVE_SET_VEHICLE_REC_ID, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(MoverGUID);
        _worldPacket.WriteUInt32(SequenceIndex);
        _worldPacket.WriteUInt32(VehicleRecID);
    }

    public WowGuid128 MoverGUID;
    public uint SequenceIndex;
    public uint VehicleRecID;
}

public readonly record struct MoveSetVehicleRecIDAck(WowGuid128 MoverGUID, MovementAck Ack, uint VehicleRecID);

public readonly record struct MoveSetCollisionHeightAck(WowGuid128 MoverGUID, float Height, uint MountDisplayID, byte Reason);

class MoveSetCollisionHeight : ServerPacket, ISpanWritable
{
    public MoveSetCollisionHeight() : base(Opcode.SMSG_MOVE_SET_COLLISION_HEIGHT) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(MoverGUID);
        _worldPacket.WriteUInt32(SequenceIndex);
        _worldPacket.WriteFloat(Height);
        _worldPacket.WriteFloat(Scale);
        _worldPacket.WriteByteEnum(Reason);
        _worldPacket.WriteUInt32(MountDisplayID);
        _worldPacket.WriteInt32(ScaleDuration);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 21; // GUID + uint + 2 float + byte + uint + int

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(MoverGUID.Low, MoverGUID.High);
        writer.WriteUInt32(SequenceIndex);
        writer.WriteFloat(Height);
        writer.WriteFloat(Scale);
        writer.WriteUInt8((byte)Reason);
        writer.WriteUInt32(MountDisplayID);
        writer.WriteInt32(ScaleDuration);
        return writer.Position;
    }

    public WowGuid128 MoverGUID;
    public uint SequenceIndex = 1;
    public float Height = 1.0f;
    public float Scale = 1.0f;
    public UpdateCollisionHeightReason Reason;
    public uint MountDisplayID;
    public int ScaleDuration = 2000; // time it takes for "scale"-animation

    public enum UpdateCollisionHeightReason : byte
    {
        Scale = 0,
        Mount = 1,
        Force = 2,
    }
}

class MoveKnockBack : ServerPacket, ISpanWritable
{
    public MoveKnockBack() : base(Opcode.SMSG_MOVE_KNOCK_BACK, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(MoverGUID);
        _worldPacket.WriteUInt32(MoveCounter);
        _worldPacket.WriteVector2(Direction);
        _worldPacket.WriteFloat(HorizontalSpeed);
        _worldPacket.WriteFloat(VerticalSpeed);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 20; // GUID + uint + 2 floats (Vector2) + 2 floats

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(MoverGUID.Low, MoverGUID.High);
        writer.WriteUInt32(MoveCounter);
        writer.WriteVector2(Direction);
        writer.WriteFloat(HorizontalSpeed);
        writer.WriteFloat(VerticalSpeed);
        return writer.Position;
    }

    public WowGuid128 MoverGUID;
    public uint MoveCounter;
    public Vector2 Direction;
    public float HorizontalSpeed;
    public float VerticalSpeed;
}

public class MoveUpdateKnockBack : ServerPacket, ISpanWritable
{
    public MoveUpdateKnockBack() : base(Opcode.SMSG_MOVE_UPDATE_KNOCK_BACK) { }

    public override void Write()
    {
        ModernMovementCodec.Write(_worldPacket, MoverGUID, in MoveInfo);
    }

    public int MaxSize => ModernMovementCodec.MaxSize;

    public int WriteToSpan(Span<byte> buffer)
    {
        return ModernMovementCodec.Write(buffer, MoverGUID, in MoveInfo);
    }

    public WowGuid128 MoverGUID;
    public MovementInfo MoveInfo;
}

class SuspendToken : ServerPacket, ISpanWritable
{
    public SuspendToken() : base(Opcode.SMSG_SUSPEND_TOKEN, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(SequenceIndex);
        _worldPacket.WriteBits(Reason, 2);
        _worldPacket.FlushBits();
    }

    public int MaxSize => 5; // uint + 1 byte for 2 bits

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(SequenceIndex);
        writer.WriteBits(Reason, 2);
        writer.FlushBits();
        return writer.Position;
    }

    public uint SequenceIndex = 1;
    public uint Reason = 1;
}

class ResumeToken : ServerPacket, ISpanWritable
{
    public ResumeToken() : base(Opcode.SMSG_RESUME_TOKEN, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(SequenceIndex);
        _worldPacket.WriteBits(Reason, 2);
        _worldPacket.FlushBits();
    }

    public int MaxSize => 5; // uint + 1 byte for 2 bits

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(SequenceIndex);
        writer.WriteBits(Reason, 2);
        writer.FlushBits();
        return writer.Position;
    }

    public uint SequenceIndex = 1;
    public uint Reason = 1;
}

public class ControlUpdate : ServerPacket, ISpanWritable
{
    public ControlUpdate() : base(Opcode.SMSG_CONTROL_UPDATE) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Guid);
        _worldPacket.WriteBit(HasControl);
        _worldPacket.FlushBits();
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 1; // GUID + 1 byte for bit

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Guid.Low, Guid.High);
        writer.WriteBit(HasControl);
        writer.FlushBits();
        return writer.Position;
    }

    public WowGuid128 Guid;
    public bool HasControl;
}

public readonly record struct SetActiveMover(WowGuid128 MoverGUID);

public class MoveSetActiveMover : ServerPacket
{
    public MoveSetActiveMover() : base(Opcode.SMSG_MOVE_SET_ACTIVE_MOVER, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(MoverGUID);
    }

    public WowGuid128 MoverGUID;
}

public class PhaseShiftChange : ServerPacket
{
    public PhaseShiftChange() : base(Opcode.SMSG_PHASE_SHIFT_CHANGE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Client);
        _worldPacket.WriteUInt32(PhaseShiftFlags);
        _worldPacket.WriteUInt32((uint)Phases.Count);
        _worldPacket.WritePackedGuid128(PersonalGUID);
        foreach (var phase in Phases)
        {
            _worldPacket.WriteUInt16(phase.Flags);
            _worldPacket.WriteUInt16(phase.Id);
        }
        _worldPacket.WriteUInt32((uint)VisibleMapIDs.Count);
        foreach (var mapId in VisibleMapIDs)
            _worldPacket.WriteUInt16(mapId);
        _worldPacket.WriteUInt32((uint)PreloadMapIDs.Count);
        foreach (var mapId in PreloadMapIDs)
            _worldPacket.WriteUInt16(mapId);
        _worldPacket.WriteUInt32((uint)UiMapPhaseIDs.Count);
        foreach (var uiMapPhase in UiMapPhaseIDs)
            _worldPacket.WriteUInt16(uiMapPhase);
    }

    public WowGuid128 Client;
    public uint PhaseShiftFlags = 8; // 8 = Unphased — default for normal world entry
    public WowGuid128 PersonalGUID;
    public List<(ushort Flags, ushort Id)> Phases = new();
    public List<ushort> VisibleMapIDs = new();
    public List<ushort> PreloadMapIDs = new();
    public List<ushort> UiMapPhaseIDs = new();
}

public readonly record struct InitActiveMoverComplete(uint Ticks);

public readonly record struct MoveSplineDone(WowGuid128 Guid, MovementInfo MoveInfo, int SplineID);
public readonly record struct MoveTimeSkipped(WowGuid128 MoverGUID, uint TimeSkipped);
