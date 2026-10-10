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
}
