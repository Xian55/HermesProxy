using Framework.IO;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HermesProxy.World.Objects;

// Dirty-field mask for the V1_14/V2_5 flat update-field path. Same block math as
// Framework.Util.StackBitMask, but heap-backed: it accumulates across SetUpdateField calls and is
// cached with its UpdateFieldsArray in ObjectCacheModern, which a ref struct cannot be.
// Block j bit k written little-endian lands at byte 4j + k/8, bit k%8 — the layout the previous
// BitArray.CopyTo(byte[]) implementation produced.
public sealed class UpdateMask
{
    private readonly uint[] _blocks;
    private readonly uint _fieldCount;

    public UpdateMask(uint valuesCount)
    {
        _fieldCount = valuesCount;
        _blocks = new uint[BlockCount((int)valuesCount)];
    }

    public uint GetCount() { return _fieldCount; }

    public ReadOnlySpan<uint> Blocks => _blocks;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BlockCount(int fieldCount) => (fieldCount + 31) >> 5;

    public void AppendToPacket(ByteBuffer data)
    {
        data.WriteUInt8((byte)_blocks.Length);
        WriteUInt32s(data, _blocks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetBit(int index)
    {
        CheckIndex(index);
        return (_blocks[index >> 5] & (1u << (index & 31))) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBit(int index)
    {
        CheckIndex(index);
        _blocks[index >> 5] |= 1u << (index & 31);
    }

    public void Clear() => Array.Clear(_blocks);

    // The wire wants little-endian uint32s, which on a little-endian host are the span's own bytes.
    internal static void WriteUInt32s(ByteBuffer data, ReadOnlySpan<uint> values)
    {
        if (BitConverter.IsLittleEndian)
        {
            data.WriteBytes(MemoryMarshal.AsBytes(values));
            return;
        }

        foreach (var value in values)
            data.WriteUInt32(value);
    }

    // BitArray bounded indices by its length; the last block's spare bits must stay unreachable
    // or a stray SetBit would put a field on the wire that has no value behind it.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckIndex(int index)
    {
        if ((uint)index >= _fieldCount)
            ThrowIndexOutOfRange(index);
    }

    [DoesNotReturn]
    private static void ThrowIndexOutOfRange(int index) =>
        throw new ArgumentOutOfRangeException(nameof(index), index, null);
}

public enum DynamicFieldChangeType
{
    Unchanged = 0,
    ValueChanged = 0x7FFF,
    ValueAndSizeChanged = 0x8000
}
