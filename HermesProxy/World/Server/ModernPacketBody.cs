using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using Framework.IO;

namespace HermesProxy.World.Server;

/// <summary>
/// Lays out the body of one modern world packet, the part behind the 16-byte header that gets
/// encrypted: the opcode and payload, or the <c>SMSG_COMPRESSED_PACKET</c> envelope around them.
/// </summary>
/// <remarks>
/// Split out of <see cref="WorldSocket.SendPacket"/> so the wire bytes can be tested without a
/// socket. The layout matches TrinityCore's <c>WorldSocket::WritePacketToBuffer</c>. Every opcode
/// in it, the envelope's and the one inside it, is <c>opcodeSize</c> bytes: 2 before 4.4.0 and 4
/// from it (<see cref="ModernVersion.OpcodeSize"/>).
/// </remarks>
internal static class ModernPacketBody
{
    /// <summary>Bodies with a payload larger than this go out compressed
    /// (TrinityCore's <c>WorldSocket::MinSizeForCompression</c>).</summary>
    public const int MinSizeForCompression = 0x400;

    /// <summary>u32 uncompressed size (opcode included), u32 Adler-32 of opcode + payload,
    /// u32 Adler-32 of the deflated bytes.</summary>
    public const int CompressedInfoSize = 12;

    // The client seeds both checksums with this rather than zlib's 1.
    private const uint AdlerSeed = 0x9827D8F1;

    public static int PlainSize(int opcodeSize, int payloadLength) => opcodeSize + payloadLength;

    public static int CompressedSize(int opcodeSize, int deflatedLength) => opcodeSize + CompressedInfoSize + deflatedLength;

    /// <summary>Writes <paramref name="opcode"/> little-endian into the first
    /// <paramref name="opcodeSize"/> bytes of <paramref name="destination"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteOpcode(Span<byte> destination, uint opcode, int opcodeSize)
    {
        if (opcodeSize == sizeof(uint))
            BinaryPrimitives.WriteUInt32LittleEndian(destination, opcode);
        else
            BinaryPrimitives.WriteUInt16LittleEndian(destination, (ushort)opcode);
    }

    public static void WritePlain(Span<byte> body, uint opcode, int opcodeSize, ReadOnlySpan<byte> payload)
    {
        WriteOpcode(body, opcode, opcodeSize);
        payload.CopyTo(body[opcodeSize..]);
    }

    /// <param name="deflated">Opcode + payload, deflated on the connection's stream and ending in
    /// a sync-flush marker.</param>
    public static void WriteCompressed(Span<byte> body, uint compressedOpcode, uint opcode, int opcodeSize,
        ReadOnlySpan<byte> payload, ReadOnlySpan<byte> deflated)
    {
        Span<byte> opcodeBytes = stackalloc byte[sizeof(uint)];
        opcodeBytes = opcodeBytes[..opcodeSize];
        WriteOpcode(opcodeBytes, opcode, opcodeSize);

        WriteOpcode(body, compressedOpcode, opcodeSize);
        Span<byte> info = body[opcodeSize..];
        BinaryPrimitives.WriteInt32LittleEndian(info, payload.Length + opcodeSize);
        BinaryPrimitives.WriteUInt32LittleEndian(info[4..], Adler32.Update(Adler32.Update(AdlerSeed, opcodeBytes), payload));
        BinaryPrimitives.WriteUInt32LittleEndian(info[8..], Adler32.Update(AdlerSeed, deflated));
        deflated.CopyTo(info[CompressedInfoSize..]);
    }
}
