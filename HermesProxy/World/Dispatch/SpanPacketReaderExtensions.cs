using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World.Client;

namespace HermesProxy.World.Dispatch;

/// <summary>
/// The reads that <see cref="WorldPacket"/> offers but <see cref="SpanPacketReader"/> cannot:
/// their return types live in HermesProxy, and Framework does not reference it.
/// </summary>
/// <remarks>
/// Each one mirrors the <see cref="WorldPacket"/> member of the same name byte for byte, so a
/// handler body converted from <c>packet.ReadGuid()</c> to the span reader keeps its meaning.
/// Extension methods on a <see langword="ref struct"/> must take it by <see langword="ref"/>,
/// which also keeps the reader's position advancing in the caller.
/// </remarks>
public static class SpanPacketReaderExtensions
{
    /// <summary>A full 8-byte GUID. Mirrors <c>WorldPacket.ReadGuid</c>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static WowGuid64 ReadGuid(this ref SpanPacketReader reader)
        => new(reader.ReadUInt64());

    /// <summary>
    /// A legacy packed GUID — one mask byte then the non-zero bytes it names.
    /// Mirrors <c>WorldPacket.ReadPackedGuid</c>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static WowGuid64 ReadPackedGuid(this ref SpanPacketReader reader)
        => new(reader.ReadPackedUInt64());

    /// <summary>
    /// A modern packed GUID — a low mask byte, a high mask byte, then each half's bytes.
    /// Mirrors <c>WorldPacket.ReadPackedGuid128</c>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static WowGuid128 ReadPackedGuid128(this ref SpanPacketReader reader)
    {
        reader.ReadPackedGuid128(out ulong low, out ulong high);
        return new WowGuid128(low, high);
    }

    /// <summary>
    /// An entry id. Bit 31 marks the entry invalid, or on some opcodes tells NPCs and GOs
    /// apart, so it is returned alongside the masked-off value rather than folded in.
    /// Mirrors <c>WorldPacket.ReadEntry</c>.
    /// </summary>
    public static KeyValuePair<int, bool> ReadEntry(this ref SpanPacketReader reader)
    {
        uint entry = reader.ReadUInt32();
        uint realEntry = entry & 0x7FFFFFFF;
        return new KeyValuePair<int, bool>((int)realEntry, realEntry != entry);
    }

    /// <summary>Mirrors <c>WorldPacket.ReadUpdateField</c>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UpdateField ReadUpdateField(this ref SpanPacketReader reader)
        => new(reader.ReadUInt32());
}
