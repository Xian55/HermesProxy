using System;

namespace HermesProxy.World;

/// <summary>
/// The GUID encoding of 4.x legacy servers (TrinityCore 4.3.4 ObjectGuid bit/byte sequences): one
/// bit per byte saying whether it is non-zero, and later the non-zero bytes XORed with 1. Each
/// packet has its own bit order and byte order, and often interleaves them with other fields.
/// </summary>
public static class MaskedGuid
{
    /// <summary>The presence bits, in <paramref name="order"/>.</summary>
    public static void WriteMaskBits(WorldPacket packet, ulong guid, ReadOnlySpan<byte> order)
    {
        foreach (byte i in order)
            packet.WriteBit(ByteAt(guid, i) != 0);
    }

    /// <summary>The non-zero bytes, XORed with 1, in <paramref name="order"/>.</summary>
    public static void WriteBytes(WorldPacket packet, ulong guid, ReadOnlySpan<byte> order)
    {
        foreach (byte i in order)
        {
            byte value = ByteAt(guid, i);
            if (value != 0)
                packet.WriteUInt8((byte)(value ^ 1));
        }
    }

    /// <summary>A GUID that is the whole packet, or its own section: bits, flush, bytes.</summary>
    public static void Write(WorldPacket packet, ulong guid, ReadOnlySpan<byte> bitOrder, ReadOnlySpan<byte> byteOrder)
    {
        WriteMaskBits(packet, guid, bitOrder);
        packet.FlushBits();
        WriteBytes(packet, guid, byteOrder);
    }

    private static byte ByteAt(ulong guid, int index) => (byte)(guid >> (index * 8));

    /// <summary>Reads presence bits into <paramref name="mask"/>, in <paramref name="order"/>.</summary>
    public static void ReadMaskBits(WorldPacket packet, Span<bool> mask, ReadOnlySpan<byte> order)
    {
        foreach (byte i in order)
            mask[i] = packet.HasBit();
    }

    /// <summary>Reads one byte when its presence bit is set, undoing the XOR.</summary>
    public static void ReadByte(WorldPacket packet, ReadOnlySpan<bool> mask, Span<byte> bytes, int index)
    {
        if (mask[index])
            bytes[index] = (byte)(packet.ReadUInt8() ^ 1);
    }

    /// <summary>Reads the bytes whose presence bits are set, in <paramref name="order"/>.</summary>
    public static void ReadBytes(WorldPacket packet, ReadOnlySpan<bool> mask, Span<byte> bytes, ReadOnlySpan<byte> order)
    {
        foreach (byte i in order)
            ReadByte(packet, mask, bytes, i);
    }

    public static ulong ToUInt64(ReadOnlySpan<byte> bytes) => System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(bytes);
}
