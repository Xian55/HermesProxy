using System;
using System.Collections.Frozen;
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

    public static void Write(WorldPacket packet, ReadOnlySpan<LegacyMovementElement> sequence, WowGuid64 mover,
        in MovementInfo info, uint movementCounter = 0, float extraFloat = 0f, int twoBitExtraAt = -1, byte extraBits = 0)
    {
        int extraIndex = 0;
        uint flags = (uint)info.Flags & 0x3FFFFFFF;
        uint flagsExtra = info.FlagsExtra & FlagsExtraMask;
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
                    if (extraIndex++ == twoBitExtraAt)
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

        if (!WrittenWithoutPresenceBits.Contains(opcode))
        {
            if (Read(packet, sequence, implicitPresence: false, hasFall: false, hasTransport: false, TwoBitExtraAt(opcode),
                    out mover, out info, out movementCounter, out extraFloat, out extraBits, out ulong transportGuid))
                SetTransportGuid(ref info, transportGuid, gameState);
            return true;
        }

        // Whether the fall data and the transport are there only the length tells: each guess is
        // read from the start again until one ends where the packet ends.
        int start = packet.ReadPosition;
        int length = packet.Remaining();
        for (int guess = 0; guess < 4; guess++)
        {
            packet.RewindTo(start);
            try
            {
                bool hasTransport = Read(packet, sequence, implicitPresence: true, hasFall: (guess & 1) != 0,
                    hasTransport: (guess & 2) != 0, twoBitExtraAt: -1, out mover, out info, out movementCounter,
                    out extraFloat, out extraBits, out ulong transportGuid);
                if (packet.Remaining() != 0)
                    continue;
                if (hasTransport)
                    SetTransportGuid(ref info, transportGuid, gameState);
                return true;
            }
            catch (Exception e) when (e is ArgumentOutOfRangeException or IndexOutOfRangeException)
            {
            }
        }
        MovementLogMessages.LegacyMovementFitsNoShape(_log, opcode, length);
        mover = 0;
        info = default;
        return false;
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
                        info.FlagsExtra = packet.ReadBits<uint>(12);
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

    private static void WriteMaskedByte(WorldPacket packet, byte value)
    {
        if (value != 0)
            packet.WriteUInt8((byte)(value ^ 1));
    }
}
