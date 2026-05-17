using System;
using System.Runtime.CompilerServices;

namespace Framework.Util;

// Stack-backed changesMask helper for descriptor-tree serialization.
// Caller owns the backing Span<uint> (typically from stackalloc); this
// struct is a thin facade that hides the bit/block index math.
public readonly ref struct StackBitMask
{
    private readonly Span<uint> _blocks;

    public StackBitMask(Span<uint> blocks)
    {
        _blocks = blocks;
        _blocks.Clear();
    }

    public int BlockCount => _blocks.Length;

    public uint this[int blockIndex] => _blocks[blockIndex];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBit(int bit) => _blocks[bit >> 5] |= 1u << (bit & 31);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBitSet(int bit) => (_blocks[bit >> 5] & (1u << (bit & 31))) != 0;

    public bool AnyBitSet => _blocks.ContainsAnyExcept(0u);
}
