using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Framework.Logging;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;

namespace HermesProxy.World.Objects;

/// <summary>
/// Writes a movement block for a 4.3.4 server, in the per-opcode order of
/// <see cref="LegacyMovementSequencesCata"/>. Each element is written the way TrinityCore 4.3.4's
/// Player::ReadMovementInfo reads it back: most presence bits are inverted (a set bit means the
/// field is absent), GUIDs are masked (a bit per non-zero byte, the bytes XORed with 1), and the
/// transport and fall sub-fields only exist behind their own bits.
/// </summary>
public static class LegacyMovementCata
{
    private static readonly Microsoft.Extensions.Logging.ILogger _log = Log.CreateMelLogger(Log.CategoryServer);

    private const MovementFlagModern PitchFlags = MovementFlagModern.Swimming | MovementFlagModern.Flying;
    private const uint FlagsExtraMask = 0xFFF;          // 4.3.4 sends 12 bits of extra flags

    // The extra flag that moved between 4.3.4 and the 4.4.2 client: being able to pass from swimming
    // to flying is 0x400 to TrinityCore 4.3.4 and 0x100 to the client, whose 0x400 is turning while
    // falling. Passed through as it is, every movement packet tells the server the ability is gone,
    // and a dismount in the air leaves it with the client.
    private const uint CanSwimToFlyLegacy = 0x400;
    private const uint CanSwimToFlyModern = 0x100;

    internal static uint FlagsExtraToLegacy(uint modern) =>
        (modern & FlagsExtraMask & ~(CanSwimToFlyLegacy | CanSwimToFlyModern))
        | ((modern & CanSwimToFlyModern) != 0 ? CanSwimToFlyLegacy : 0);

    internal static uint FlagsExtraFromLegacy(uint legacy) =>
        (legacy & ~(CanSwimToFlyLegacy | CanSwimToFlyModern))
        | ((legacy & CanSwimToFlyLegacy) != 0 ? CanSwimToFlyModern : 0);

    /// <summary>
    /// The opcode's sequence, or null when 4.3.4 has none for it. <paramref name="extraFloat"/>
    /// fills the sequence's extra element: the new speed of a speed-change ack, the height of a
    /// collision-height ack, whose second extra element is <paramref name="extraBits"/>.
    /// </summary>
    public static bool TryWrite(WorldPacket packet, Opcode opcode, WowGuid64 mover, in MovementInfo info,
        uint movementCounter = 0, float extraFloat = 0f, byte extraBits = 0)
    {
        if (!LegacyMovementSequencesCata.ByOpcode.TryGetValue(opcode, out LegacyMovementElement[]? sequence))
            return false;

        Write(packet, sequence, mover, in info, movementCounter, extraFloat, TwoBitExtraAt(opcode), extraBits);
        return true;
    }

    /// <summary>
    /// The 4.3.4 opcode for a movement report of the client's that has no MSG_ opcode to go out as,
    /// or MSG_NULL_ACTION. TrinityCore 4.3.4 reads taking off and landing as CMSG_MOVE_SET_CAN_FLY,
    /// and on it drops the auras that end on landing or flight and updates the zone. A change of
    /// transport, such as leaving a vehicle, keeps its own opcode and sequence.
    /// </summary>
    public static Opcode ReportWithoutMsg(Opcode clientOpcode) => clientOpcode switch
    {
        Opcode.CMSG_MOVE_SET_FLY => Opcode.CMSG_MOVE_SET_CAN_FLY,
        Opcode.CMSG_MOVE_CHANGE_TRANSPORT => Opcode.CMSG_MOVE_CHANGE_TRANSPORT,
        _ => Opcode.MSG_NULL_ACTION,
    };

    /// <summary>
    /// Which of the sequence's extra elements is two bits rather than a float: the reason of a
    /// collision-height change, written before the height and read back after it (TrinityCore
    /// 4.3.4 SendHeightChangeToMover and HandleSetCollisionHeightAck, MSEExtraTwoBits). -1: none.
    /// </summary>
    private static int TwoBitExtraAt(Opcode opcode) => opcode switch
    {
        Opcode.SMSG_MOVE_SET_COLLISION_HEIGHT => 0,
        Opcode.CMSG_MOVE_SET_COLLISION_HEIGHT_ACK => 1,
        _ => -1,
    };

    /// <summary>
    /// What CMSG_CHANGE_SEATS_ON_CONTROLLED_VEHICLE weaves through its sequence's extra elements:
    /// the seat, then the masked GUID of the vehicle it belongs to (TrinityCore 4.3.4
    /// HandleChangeSeatsOnControlledVehicle, accessoryGuid).
    /// </summary>
    private static readonly LegacyMovementElement[] ChangeSeatsExtra =
    [
        LegacyMovementElement.ExtraInt8,
        LegacyMovementElement.HasGuidByte2, LegacyMovementElement.HasGuidByte4, LegacyMovementElement.HasGuidByte7,
        LegacyMovementElement.HasGuidByte6, LegacyMovementElement.HasGuidByte5, LegacyMovementElement.HasGuidByte0,
        LegacyMovementElement.HasGuidByte1, LegacyMovementElement.HasGuidByte3,
        LegacyMovementElement.GuidByte6, LegacyMovementElement.GuidByte1, LegacyMovementElement.GuidByte2,
        LegacyMovementElement.GuidByte5, LegacyMovementElement.GuidByte3, LegacyMovementElement.GuidByte0,
        LegacyMovementElement.GuidByte4, LegacyMovementElement.GuidByte7,
    ];

    /// <summary>
    /// A seat change from the vehicle UI: the vehicle's movement block, the seat to move to and the
    /// vehicle that seat is on. An empty <paramref name="dstVehicle"/> asks for the next seat when
    /// <paramref name="dstSeat"/> is positive, the previous one otherwise.
    /// </summary>
    public static void WriteChangeSeats(WorldPacket packet, WowGuid64 vehicle, in MovementInfo info,
        WowGuid64 dstVehicle, sbyte dstSeat)
        => Write(packet, LegacyMovementSequencesCata.ByOpcode[Opcode.CMSG_CHANGE_SEATS_ON_CONTROLLED_VEHICLE], vehicle,
            in info, extraSequence: ChangeSeatsExtra, extraGuid: dstVehicle.Low, extraInt8: dstSeat);

    public static void Write(WorldPacket packet, ReadOnlySpan<LegacyMovementElement> sequence, WowGuid64 mover,
        in MovementInfo info, uint movementCounter = 0, float extraFloat = 0f, int twoBitExtraAt = -1, byte extraBits = 0,
        ReadOnlySpan<LegacyMovementElement> extraSequence = default, ulong extraGuid = 0, sbyte extraInt8 = 0)
    {
        int extraIndex = 0;
        uint flags = (uint)info.Flags & 0x3FFFFFFF;
        uint flagsExtra = FlagsExtraToLegacy(info.FlagsExtra);
        TransportInfo transport = info.Transport.GetValueOrDefault();
        ulong guid = mover.Low;
        ulong transportGuid = info.TransportGuid != default ? transport.Guid.To64().Low : 0;

        bool hasTransport = transportGuid != 0;
        bool hasTransportTime2 = hasTransport && transport.PrevTime != 0;
        bool hasVehicleId = hasTransport && transport.VehicleId != 0;
        bool hasPitch = (info.Flags & PitchFlags) != 0
                        || (info.FlagsExtra & (uint)MovementFlagExtra.AlwaysAllowPitching) != 0;
        bool hasFallDirection = info.Flags.HasFlag(MovementFlagModern.Falling);
        bool hasFallData = hasFallDirection || info.FallTime != 0;
        bool hasSplineElevation = info.Flags.HasFlag(MovementFlagModern.SplineElevation);

        foreach (LegacyMovementElement element in sequence)
        {
            switch (element)
            {
                case >= LegacyMovementElement.HasGuidByte0 and <= LegacyMovementElement.HasGuidByte7:
                    packet.WriteBit(ByteAt(guid, element - LegacyMovementElement.HasGuidByte0) != 0);
                    break;
                case >= LegacyMovementElement.HasTransportGuidByte0 and <= LegacyMovementElement.HasTransportGuidByte7:
                    if (hasTransport)
                        packet.WriteBit(ByteAt(transportGuid, element - LegacyMovementElement.HasTransportGuidByte0) != 0);
                    break;
                case >= LegacyMovementElement.GuidByte0 and <= LegacyMovementElement.GuidByte7:
                    WriteMaskedByte(packet, ByteAt(guid, element - LegacyMovementElement.GuidByte0));
                    break;
                case >= LegacyMovementElement.TransportGuidByte0 and <= LegacyMovementElement.TransportGuidByte7:
                    if (hasTransport)
                        WriteMaskedByte(packet, ByteAt(transportGuid, element - LegacyMovementElement.TransportGuidByte0));
                    break;
                case LegacyMovementElement.HasMovementFlags:
                    packet.WriteBit(flags == 0);
                    break;
                case LegacyMovementElement.HasMovementFlags2:
                    packet.WriteBit(flagsExtra == 0);
                    break;
                case LegacyMovementElement.HasTimestamp:
                case LegacyMovementElement.HasOrientation:
                    packet.WriteBit(false);
                    break;
                case LegacyMovementElement.HasTransportData:
                    packet.WriteBit(hasTransport);
                    break;
                case LegacyMovementElement.HasTransportTime2:
                    if (hasTransport)
                        packet.WriteBit(hasTransportTime2);
                    break;
                case LegacyMovementElement.HasVehicleId:
                    if (hasTransport)
                        packet.WriteBit(hasVehicleId);
                    break;
                case LegacyMovementElement.HasPitch:
                    packet.WriteBit(!hasPitch);
                    break;
                case LegacyMovementElement.HasFallData:
                    packet.WriteBit(hasFallData);
                    break;
                case LegacyMovementElement.HasFallDirection:
                    if (hasFallData)
                        packet.WriteBit(hasFallDirection);
                    break;
                case LegacyMovementElement.HasSplineElevation:
                    packet.WriteBit(!hasSplineElevation);
                    break;
                case LegacyMovementElement.HasSpline:
                case LegacyMovementElement.HasHeightChangeFailed:
                case LegacyMovementElement.ZeroBit:
                    packet.WriteBit(false);
                    break;
                case LegacyMovementElement.OneBit:
                    packet.WriteBit(true);
                    break;
                case LegacyMovementElement.MovementFlags:
                    if (flags != 0)
                        packet.WriteBits(flags, 30);
                    break;
                case LegacyMovementElement.MovementFlags2:
                    if (flagsExtra != 0)
                        packet.WriteBits(flagsExtra, 12);
                    break;
                case LegacyMovementElement.Timestamp:
                    packet.WriteUInt32(info.MoveTime);
                    break;
                case LegacyMovementElement.PositionX:
                    packet.WriteFloat(info.Position.X);
                    break;
                case LegacyMovementElement.PositionY:
                    packet.WriteFloat(info.Position.Y);
                    break;
                case LegacyMovementElement.PositionZ:
                    packet.WriteFloat(info.Position.Z);
                    break;
                case LegacyMovementElement.Orientation:
                    packet.WriteFloat(info.Orientation);
                    break;
                case LegacyMovementElement.TransportPositionX:
                    if (hasTransport)
                        packet.WriteFloat(transport.Offset.X);
                    break;
                case LegacyMovementElement.TransportPositionY:
                    if (hasTransport)
                        packet.WriteFloat(transport.Offset.Y);
                    break;
                case LegacyMovementElement.TransportPositionZ:
                    if (hasTransport)
                        packet.WriteFloat(transport.Offset.Z);
                    break;
                case LegacyMovementElement.TransportOrientation:
                    if (hasTransport)
                        packet.WriteFloat(transport.Orientation);
                    break;
                case LegacyMovementElement.TransportSeat:
                    if (hasTransport)
                        packet.WriteInt8(transport.Seat);
                    break;
                case LegacyMovementElement.TransportTime:
                    if (hasTransport)
                        packet.WriteUInt32(transport.Time);
                    break;
                case LegacyMovementElement.TransportTime2:
                    if (hasTransportTime2)
                        packet.WriteUInt32(transport.PrevTime);
                    break;
                case LegacyMovementElement.TransportVehicleId:
                    if (hasVehicleId)
                        packet.WriteUInt32(transport.VehicleId);
                    break;
                case LegacyMovementElement.Pitch:
                    if (hasPitch)
                        packet.WriteFloat(info.SwimPitch);
                    break;
                case LegacyMovementElement.FallTime:
                    if (hasFallData)
                        packet.WriteUInt32(info.FallTime);
                    break;
                case LegacyMovementElement.FallVerticalSpeed:
                    if (hasFallData)
                        packet.WriteFloat(info.JumpVerticalSpeed);
                    break;
                case LegacyMovementElement.FallCosAngle:
                    if (hasFallDirection)
                        packet.WriteFloat(info.JumpCosAngle);
                    break;
                case LegacyMovementElement.FallSinAngle:
                    if (hasFallDirection)
                        packet.WriteFloat(info.JumpSinAngle);
                    break;
                case LegacyMovementElement.FallHorizontalSpeed:
                    if (hasFallDirection)
                        packet.WriteFloat(info.JumpHorizontalSpeed);
                    break;
                case LegacyMovementElement.SplineElevation:
                    if (hasSplineElevation)
                        packet.WriteFloat(info.SplineElevation);
                    break;
                case LegacyMovementElement.Counter:
                    packet.WriteUInt32(movementCounter);
                    break;
                case LegacyMovementElement.FlushBits:
                    packet.FlushBits();
                    break;
                case LegacyMovementElement.ExtraElement:
                    if (!extraSequence.IsEmpty)
                        WriteExtra(packet, extraSequence[extraIndex++], extraGuid, extraInt8);
                    else if (extraIndex++ == twoBitExtraAt)
                        packet.WriteBits(extraBits, 2);
                    else
                        packet.WriteFloat(extraFloat);
                    break;
                case LegacyMovementElement.End:
                    return;
            }
        }
    }

    /// <summary>
    /// Reads a movement block a 4.3.4 server wrote in <paramref name="opcode"/>'s order: the
    /// inverse of <see cref="Write"/>, as TrinityCore 4.3.4's Unit::WriteMovementInfo produces it.
    /// <paramref name="extraFloat"/> is the sequence's extra element, the speed of a speed packet.
    /// False, with a warning, when no order this opcode is written in ends where the packet ends.
    /// </summary>
    public static bool TryRead(WorldPacket packet, Opcode opcode, GameSessionData gameState, out ulong mover,
        out MovementInfo info, out uint movementCounter, out float extraFloat)
        => TryRead(packet, opcode, gameState, out mover, out info, out movementCounter, out extraFloat, out _);

    /// <summary>
    /// <see cref="TryRead(WorldPacket, Opcode, GameSessionData, out ulong, out MovementInfo, out uint, out float)"/>
    /// for a sequence with a two-bit extra element as well (<paramref name="extraBits"/>).
    /// </summary>
    public static bool TryRead(WorldPacket packet, Opcode opcode, GameSessionData gameState, out ulong mover,
        out MovementInfo info, out uint movementCounter, out float extraFloat, out byte extraBits)
    {
        mover = 0;
        info = default;
        movementCounter = 0;
        extraFloat = 0f;
        extraBits = 0;
        if (!LegacyMovementSequencesCata.ByOpcode.TryGetValue(opcode, out LegacyMovementElement[]? sequence))
            return false;

        bool withPresenceBits = WithPresenceBits.TryGetValue(opcode, out LegacyMovementElement[]? withBits);
        int start = packet.ReadPosition;
        int length = packet.Remaining();
        if (!WrittenWithoutPresenceBits.Contains(opcode))
        {
            // A read that does not end where the packet ends took the wrong order, and its mover is
            // made of other bytes. Where another core writes the client's order, the length tells
            // the two apart, as it tells the guesses below.
            if (ReadsToTheEnd(packet, start, sequence, implicitPresence: false, hasFall: false, hasTransport: false,
                    TwoBitExtraAt(opcode), gameState, out mover, out info, out movementCounter, out extraFloat, out extraBits))
                return true;
        }
        else
        {
            // Whether the fall data and the transport are there only the length tells: each guess is
            // read from the start again until one ends where the packet ends.
            for (int guess = 0; guess < 4; guess++)
            {
                if (ReadsToTheEnd(packet, start, sequence, implicitPresence: true, hasFall: (guess & 1) != 0,
                        hasTransport: (guess & 2) != 0, twoBitExtraAt: -1, gameState, out mover, out info,
                        out movementCounter, out extraFloat, out extraBits))
                    return true;
            }
        }
        if (withPresenceBits && ReadsToTheEnd(packet, start, withBits!, implicitPresence: false, hasFall: false,
                hasTransport: false, twoBitExtraAt: -1, gameState, out mover, out info, out movementCounter,
                out extraFloat, out extraBits))
            return true;
        MovementLogMessages.LegacyMovementFitsNoShape(_log, opcode, length);
        mover = 0;
        info = default;
        return false;
    }

    private static bool ReadsToTheEnd(WorldPacket packet, int start, LegacyMovementElement[] sequence,
        bool implicitPresence, bool hasFall, bool hasTransport, int twoBitExtraAt, GameSessionData gameState,
        out ulong mover, out MovementInfo info, out uint movementCounter, out float extraFloat, out byte extraBits)
    {
        packet.RewindTo(start);
        try
        {
            bool withTransport = Read(packet, sequence, implicitPresence, hasFall, hasTransport, twoBitExtraAt,
                out mover, out info, out movementCounter, out extraFloat, out extraBits, out ulong transportGuid);
            if (packet.Remaining() != 0)
                return false;
            if (withTransport)
                SetTransportGuid(ref info, transportGuid, gameState);
            return true;
        }
        catch (Exception e) when (e is ArgumentOutOfRangeException or IndexOutOfRangeException)
        {
            mover = 0;
            info = default;
            movementCounter = 0;
            extraFloat = 0f;
            extraBits = 0;
            return false;
        }
    }

    /// <summary>
    /// TrinityCore 4.3.4's tables for these observer updates hold a timestamp, fall data, a
    /// transport and a pitch but none of the bits that say whether they are there, and its writer
    /// sends them all the same: the timestamp always, the pitch when swimming or flying, the fall
    /// data while falling or with a fall time left over from the last one, the transport aboard one.
    /// </summary>
    private static readonly FrozenSet<Opcode> WrittenWithoutPresenceBits = LegacyMovementSequencesCata.ByOpcode
        .Where(p => p.Value.Contains(LegacyMovementElement.Timestamp) && !p.Value.Contains(LegacyMovementElement.HasTimestamp))
        .Select(p => p.Key)
        .ToFrozenSet();

    /// <summary>
    /// The same updates as another 4.3.4 core writes them, with a bit for everything that may be
    /// missing: the order the 4.3.4 client reads them in (WowPacketParser's, for the run-back and
    /// walk speeds). That core's packets end where this order ends and never where the reference
    /// table's does; the reference server's packets fit only the table. Tried after it, by the
    /// same length test. The client's flight-back order has one bit more at the start than the
    /// table's, and the transport's second time and seat.
    /// </summary>
    private static readonly FrozenDictionary<Opcode, LegacyMovementElement[]> WithPresenceBits = new Dictionary<Opcode, LegacyMovementElement[]>
    {
        [Opcode.SMSG_MOVE_UPDATE_RUN_BACK_SPEED] =
        [
            LegacyMovementElement.HasGuidByte1, LegacyMovementElement.HasGuidByte2, LegacyMovementElement.HasSplineElevation,
            LegacyMovementElement.HasGuidByte4, LegacyMovementElement.HasGuidByte3, LegacyMovementElement.HasFallData,
            LegacyMovementElement.HasGuidByte6, LegacyMovementElement.HasTimestamp, LegacyMovementElement.HasGuidByte0,
            LegacyMovementElement.ZeroBit, LegacyMovementElement.HasMovementFlags, LegacyMovementElement.HasPitch,
            LegacyMovementElement.HasSpline, LegacyMovementElement.HasGuidByte5, LegacyMovementElement.HasMovementFlags2,
            LegacyMovementElement.MovementFlags2, LegacyMovementElement.HasOrientation, LegacyMovementElement.MovementFlags,
            LegacyMovementElement.HasFallDirection, LegacyMovementElement.HasTransportData, LegacyMovementElement.HasTransportGuidByte5,
            LegacyMovementElement.HasTransportTime2, LegacyMovementElement.HasTransportGuidByte3, LegacyMovementElement.HasTransportGuidByte1,
            LegacyMovementElement.HasTransportGuidByte6, LegacyMovementElement.HasTransportGuidByte7, LegacyMovementElement.HasTransportGuidByte2,
            LegacyMovementElement.HasTransportGuidByte4, LegacyMovementElement.HasTransportGuidByte0, LegacyMovementElement.HasVehicleId,
            LegacyMovementElement.HasGuidByte7, LegacyMovementElement.FlushBits,
            LegacyMovementElement.TransportPositionX, LegacyMovementElement.TransportGuidByte2, LegacyMovementElement.TransportGuidByte5,
            LegacyMovementElement.TransportGuidByte4, LegacyMovementElement.TransportGuidByte6, LegacyMovementElement.TransportTime2,
            LegacyMovementElement.TransportGuidByte0, LegacyMovementElement.TransportGuidByte3, LegacyMovementElement.TransportPositionY,
            LegacyMovementElement.TransportGuidByte7, LegacyMovementElement.TransportVehicleId, LegacyMovementElement.TransportPositionZ,
            LegacyMovementElement.TransportTime, LegacyMovementElement.TransportSeat, LegacyMovementElement.TransportGuidByte1,
            LegacyMovementElement.TransportOrientation, LegacyMovementElement.GuidByte4, LegacyMovementElement.FallTime,
            LegacyMovementElement.FallHorizontalSpeed, LegacyMovementElement.FallCosAngle, LegacyMovementElement.FallSinAngle,
            LegacyMovementElement.FallVerticalSpeed, LegacyMovementElement.Timestamp, LegacyMovementElement.SplineElevation,
            LegacyMovementElement.GuidByte1, LegacyMovementElement.Orientation, LegacyMovementElement.GuidByte0,
            LegacyMovementElement.GuidByte5, LegacyMovementElement.GuidByte3, LegacyMovementElement.PositionX,
            LegacyMovementElement.PositionY, LegacyMovementElement.Pitch, LegacyMovementElement.GuidByte7,
            LegacyMovementElement.ExtraElement, LegacyMovementElement.GuidByte2, LegacyMovementElement.GuidByte6,
            LegacyMovementElement.PositionZ, LegacyMovementElement.End,
        ],
        [Opcode.SMSG_MOVE_UPDATE_WALK_SPEED] =
        [
            LegacyMovementElement.HasPitch, LegacyMovementElement.HasOrientation, LegacyMovementElement.ZeroBit,
            LegacyMovementElement.HasGuidByte3, LegacyMovementElement.HasSplineElevation, LegacyMovementElement.HasGuidByte2,
            LegacyMovementElement.HasTransportData, LegacyMovementElement.HasTransportGuidByte6, LegacyMovementElement.HasTransportGuidByte3,
            LegacyMovementElement.HasTransportGuidByte2, LegacyMovementElement.HasTransportGuidByte0, LegacyMovementElement.HasTransportGuidByte4,
            LegacyMovementElement.HasTransportTime2, LegacyMovementElement.HasTransportGuidByte7, LegacyMovementElement.HasTransportGuidByte1,
            LegacyMovementElement.HasTransportGuidByte5, LegacyMovementElement.HasVehicleId, LegacyMovementElement.HasGuidByte7,
            LegacyMovementElement.HasGuidByte5, LegacyMovementElement.HasGuidByte1, LegacyMovementElement.HasFallData,
            LegacyMovementElement.HasGuidByte0, LegacyMovementElement.HasMovementFlags2, LegacyMovementElement.HasTimestamp,
            LegacyMovementElement.HasMovementFlags, LegacyMovementElement.HasGuidByte6, LegacyMovementElement.HasFallDirection,
            LegacyMovementElement.MovementFlags2, LegacyMovementElement.MovementFlags, LegacyMovementElement.HasSpline,
            LegacyMovementElement.HasGuidByte4, LegacyMovementElement.FlushBits,
            LegacyMovementElement.Pitch, LegacyMovementElement.TransportGuidByte6, LegacyMovementElement.TransportGuidByte0,
            LegacyMovementElement.TransportGuidByte4, LegacyMovementElement.TransportGuidByte2, LegacyMovementElement.TransportPositionX,
            LegacyMovementElement.TransportGuidByte7, LegacyMovementElement.TransportTime, LegacyMovementElement.TransportTime2,
            LegacyMovementElement.TransportPositionZ, LegacyMovementElement.TransportSeat, LegacyMovementElement.TransportGuidByte5,
            LegacyMovementElement.TransportVehicleId, LegacyMovementElement.TransportOrientation, LegacyMovementElement.TransportGuidByte1,
            LegacyMovementElement.TransportPositionY, LegacyMovementElement.TransportGuidByte3, LegacyMovementElement.SplineElevation,
            LegacyMovementElement.FallVerticalSpeed, LegacyMovementElement.FallHorizontalSpeed, LegacyMovementElement.FallCosAngle,
            LegacyMovementElement.FallSinAngle, LegacyMovementElement.FallTime, LegacyMovementElement.GuidByte1,
            LegacyMovementElement.GuidByte4, LegacyMovementElement.GuidByte2, LegacyMovementElement.GuidByte6,
            LegacyMovementElement.GuidByte7, LegacyMovementElement.Timestamp, LegacyMovementElement.Orientation,
            LegacyMovementElement.PositionY, LegacyMovementElement.GuidByte0, LegacyMovementElement.PositionZ,
            LegacyMovementElement.PositionX, LegacyMovementElement.GuidByte3, LegacyMovementElement.GuidByte5,
            LegacyMovementElement.ExtraElement, LegacyMovementElement.End,
        ],
        [Opcode.SMSG_MOVE_UPDATE_FLIGHT_BACK_SPEED] =
        [
            LegacyMovementElement.PositionY, LegacyMovementElement.ExtraElement, LegacyMovementElement.PositionX,
            LegacyMovementElement.PositionZ, LegacyMovementElement.HasSpline, LegacyMovementElement.HasGuidByte6,
            LegacyMovementElement.HasGuidByte4, LegacyMovementElement.HasPitch, LegacyMovementElement.HasGuidByte7,
            LegacyMovementElement.HasGuidByte1, LegacyMovementElement.HasTransportData, LegacyMovementElement.HasVehicleId,
            LegacyMovementElement.HasTransportGuidByte4, LegacyMovementElement.HasTransportGuidByte7, LegacyMovementElement.HasTransportGuidByte6,
            LegacyMovementElement.HasTransportGuidByte0, LegacyMovementElement.HasTransportTime2, LegacyMovementElement.HasTransportGuidByte2,
            LegacyMovementElement.HasTransportGuidByte3, LegacyMovementElement.HasTransportGuidByte5, LegacyMovementElement.HasTransportGuidByte1,
            LegacyMovementElement.HasGuidByte5, LegacyMovementElement.HasGuidByte0, LegacyMovementElement.HasTimestamp,
            LegacyMovementElement.HasMovementFlags, LegacyMovementElement.MovementFlags, LegacyMovementElement.HasGuidByte3,
            LegacyMovementElement.HasMovementFlags2, LegacyMovementElement.HasHeightChangeFailed, LegacyMovementElement.HasSplineElevation,
            LegacyMovementElement.MovementFlags2, LegacyMovementElement.HasGuidByte2, LegacyMovementElement.HasFallData,
            LegacyMovementElement.HasOrientation, LegacyMovementElement.HasFallDirection, LegacyMovementElement.FlushBits,
            LegacyMovementElement.FallCosAngle, LegacyMovementElement.FallHorizontalSpeed, LegacyMovementElement.FallSinAngle,
            LegacyMovementElement.FallTime, LegacyMovementElement.FallVerticalSpeed, LegacyMovementElement.TransportPositionX,
            LegacyMovementElement.TransportTime, LegacyMovementElement.TransportOrientation, LegacyMovementElement.TransportGuidByte5,
            LegacyMovementElement.TransportPositionZ, LegacyMovementElement.TransportVehicleId, LegacyMovementElement.TransportGuidByte7,
            LegacyMovementElement.TransportGuidByte1, LegacyMovementElement.TransportGuidByte3, LegacyMovementElement.TransportGuidByte6,
            LegacyMovementElement.TransportGuidByte0, LegacyMovementElement.TransportGuidByte2, LegacyMovementElement.TransportTime2,
            LegacyMovementElement.TransportPositionY, LegacyMovementElement.TransportSeat, LegacyMovementElement.TransportGuidByte4,
            LegacyMovementElement.GuidByte1, LegacyMovementElement.GuidByte4, LegacyMovementElement.Orientation,
            LegacyMovementElement.GuidByte5, LegacyMovementElement.Timestamp, LegacyMovementElement.SplineElevation,
            LegacyMovementElement.GuidByte6, LegacyMovementElement.Pitch, LegacyMovementElement.GuidByte3,
            LegacyMovementElement.GuidByte2, LegacyMovementElement.GuidByte0, LegacyMovementElement.GuidByte7,
            LegacyMovementElement.End,
        ],
    }.ToFrozenDictionary();

    private static void SetTransportGuid(ref MovementInfo info, ulong transportGuid, GameSessionData gameState)
    {
        TransportInfo transport = info.Transport.GetValueOrDefault();
        transport.Guid = new WowGuid64(transportGuid).To128(gameState);
        info.Transport = transport;
    }

    /// <summary>
    /// One pass over <paramref name="sequence"/>. With <paramref name="implicitPresence"/> the
    /// presence of the fields the sequence has no bit for is taken from <paramref name="hasFall"/>
    /// and <paramref name="hasTransport"/>, the timestamp is always there and the pitch follows the
    /// movement flags. Returns whether a transport was read; its GUID is left to the caller.
    /// </summary>
    private static bool Read(WorldPacket packet, LegacyMovementElement[] sequence, bool implicitPresence,
        bool hasFall, bool hasTransport, int twoBitExtraAt, out ulong mover, out MovementInfo info,
        out uint movementCounter, out float extraFloat, out byte extraBits, out ulong transportGuidValue)
    {
        info = default;
        movementCounter = 0;
        extraFloat = 0f;
        extraBits = 0;
        int extraIndex = 0;
        Span<bool> guidMask = stackalloc bool[8];
        Span<byte> guid = stackalloc byte[8];
        Span<bool> transportMask = stackalloc bool[8];
        Span<byte> transportGuid = stackalloc byte[8];
        bool hasFlags = false, hasFlags2 = false, hasTime = implicitPresence, hasOrientation = false;
        bool hasTransportTime2 = false, hasVehicleId = false;
        bool hasPitch = false, hasFallDirection = false, hasSplineElevation = false;
        TransportInfo transport = new();
        Vector3 position = default;

        foreach (LegacyMovementElement element in sequence)
        {
            switch (element)
            {
                case >= LegacyMovementElement.HasGuidByte0 and <= LegacyMovementElement.HasGuidByte7:
                    guidMask[element - LegacyMovementElement.HasGuidByte0] = packet.HasBit();
                    break;
                case >= LegacyMovementElement.HasTransportGuidByte0 and <= LegacyMovementElement.HasTransportGuidByte7:
                    if (hasTransport)
                        transportMask[element - LegacyMovementElement.HasTransportGuidByte0] = packet.HasBit();
                    break;
                case >= LegacyMovementElement.GuidByte0 and <= LegacyMovementElement.GuidByte7:
                    MaskedGuid.ReadByte(packet, guidMask, guid, element - LegacyMovementElement.GuidByte0);
                    break;
                case >= LegacyMovementElement.TransportGuidByte0 and <= LegacyMovementElement.TransportGuidByte7:
                    if (hasTransport)
                        MaskedGuid.ReadByte(packet, transportMask, transportGuid, element - LegacyMovementElement.TransportGuidByte0);
                    break;
                case LegacyMovementElement.HasMovementFlags: hasFlags = !packet.HasBit(); break;
                case LegacyMovementElement.HasMovementFlags2: hasFlags2 = !packet.HasBit(); break;
                case LegacyMovementElement.HasTimestamp: hasTime = !packet.HasBit(); break;
                case LegacyMovementElement.HasOrientation: hasOrientation = !packet.HasBit(); break;
                case LegacyMovementElement.HasTransportData: hasTransport = packet.HasBit(); break;
                case LegacyMovementElement.HasTransportTime2: if (hasTransport) hasTransportTime2 = packet.HasBit(); break;
                case LegacyMovementElement.HasVehicleId: if (hasTransport) hasVehicleId = packet.HasBit(); break;
                case LegacyMovementElement.HasPitch: hasPitch = !packet.HasBit(); break;
                case LegacyMovementElement.HasFallData: hasFall = packet.HasBit(); break;
                case LegacyMovementElement.HasFallDirection: if (hasFall) hasFallDirection = packet.HasBit(); break;
                case LegacyMovementElement.HasSplineElevation: hasSplineElevation = !packet.HasBit(); break;
                case LegacyMovementElement.HasSpline:
                case LegacyMovementElement.HasHeightChangeFailed:
                case LegacyMovementElement.ZeroBit:
                case LegacyMovementElement.OneBit:
                    packet.HasBit();
                    break;
                case LegacyMovementElement.MovementFlags:
                    if (hasFlags)
                        info.Flags = (MovementFlagModern)packet.ReadBits<uint>(30);
                    break;
                case LegacyMovementElement.MovementFlags2:
                    if (hasFlags2)
                        info.FlagsExtra = FlagsExtraFromLegacy(packet.ReadBits<uint>(12));
                    break;
                case LegacyMovementElement.Timestamp: if (hasTime) info.MoveTime = packet.ReadUInt32(); break;
                case LegacyMovementElement.PositionX: position.X = packet.ReadFloat(); break;
                case LegacyMovementElement.PositionY: position.Y = packet.ReadFloat(); break;
                case LegacyMovementElement.PositionZ: position.Z = packet.ReadFloat(); break;
                case LegacyMovementElement.Orientation: if (hasOrientation) info.Orientation = packet.ReadFloat(); break;
                case LegacyMovementElement.TransportPositionX:
                    if (hasTransport) transport.Offset = transport.Offset with { X = packet.ReadFloat() };
                    break;
                case LegacyMovementElement.TransportPositionY:
                    if (hasTransport) transport.Offset = transport.Offset with { Y = packet.ReadFloat() };
                    break;
                case LegacyMovementElement.TransportPositionZ:
                    if (hasTransport) transport.Offset = transport.Offset with { Z = packet.ReadFloat() };
                    break;
                case LegacyMovementElement.TransportOrientation: if (hasTransport) transport.Orientation = packet.ReadFloat(); break;
                case LegacyMovementElement.TransportSeat: if (hasTransport) transport.Seat = packet.ReadInt8(); break;
                case LegacyMovementElement.TransportTime: if (hasTransport) transport.Time = packet.ReadUInt32(); break;
                case LegacyMovementElement.TransportTime2: if (hasTransportTime2) transport.PrevTime = packet.ReadUInt32(); break;
                case LegacyMovementElement.TransportVehicleId: if (hasVehicleId) transport.VehicleId = packet.ReadUInt32(); break;
                case LegacyMovementElement.Pitch:
                    if (hasPitch || implicitPresence && (info.Flags & PitchFlags) != 0)
                        info.SwimPitch = packet.ReadFloat();
                    break;
                case LegacyMovementElement.FallTime: if (hasFall) info.FallTime = packet.ReadUInt32(); break;
                case LegacyMovementElement.FallVerticalSpeed: if (hasFall) info.JumpVerticalSpeed = packet.ReadFloat(); break;
                case LegacyMovementElement.FallCosAngle: if (hasFallDirection) info.JumpCosAngle = packet.ReadFloat(); break;
                case LegacyMovementElement.FallSinAngle: if (hasFallDirection) info.JumpSinAngle = packet.ReadFloat(); break;
                case LegacyMovementElement.FallHorizontalSpeed: if (hasFallDirection) info.JumpHorizontalSpeed = packet.ReadFloat(); break;
                case LegacyMovementElement.SplineElevation: if (hasSplineElevation) info.SplineElevation = packet.ReadFloat(); break;
                case LegacyMovementElement.Counter: movementCounter = packet.ReadUInt32(); break;
                case LegacyMovementElement.FlushBits: packet.ResetBitPos(); break;
                case LegacyMovementElement.ExtraElement:
                    if (extraIndex++ == twoBitExtraAt)
                        extraBits = (byte)packet.ReadBits<uint>(2);
                    else
                        extraFloat = packet.ReadFloat();
                    break;
                case LegacyMovementElement.End: break;
            }
        }

        info.Position = position;
        mover = MaskedGuid.ToUInt64(guid);
        transportGuidValue = 0;
        if (hasTransport)
        {
            transportGuidValue = MaskedGuid.ToUInt64(transportGuid);
            info.Transport = transport;
        }
        return hasTransport;
    }

    private static byte ByteAt(ulong value, int index) => (byte)(value >> (index * 8));

    // One element of an extra sequence, as TrinityCore 4.3.4's ExtraMovementStatusElement reads it.
    private static void WriteExtra(WorldPacket packet, LegacyMovementElement element, ulong guid, sbyte int8)
    {
        switch (element)
        {
            case >= LegacyMovementElement.HasGuidByte0 and <= LegacyMovementElement.HasGuidByte7:
                packet.WriteBit(ByteAt(guid, element - LegacyMovementElement.HasGuidByte0) != 0);
                break;
            case >= LegacyMovementElement.GuidByte0 and <= LegacyMovementElement.GuidByte7:
                WriteMaskedByte(packet, ByteAt(guid, element - LegacyMovementElement.GuidByte0));
                break;
            case LegacyMovementElement.ExtraInt8:
                packet.WriteInt8(int8);
                break;
        }
    }

    private static void WriteMaskedByte(WorldPacket packet, byte value)
    {
        if (value != 0)
            packet.WriteUInt8((byte)(value ^ 1));
    }
}
