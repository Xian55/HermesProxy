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

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Framework.IO
{
    /// <summary>
    /// Helper methods for encoding/decoding packed GUIDs in Span-based packet I/O.
    /// </summary>
    public static class PackedGuidHelper
    {
        /// <summary>
        /// Maximum size of a packed GUID128 (2 mask bytes + 16 value bytes).
        /// </summary>
        public const int MaxPackedGuid128Size = 18;

        /// <summary>
        /// Writes a packed GUID128 to the buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="low">The low 64 bits of the GUID.</param>
        /// <param name="high">The high 64 bits of the GUID.</param>
        /// <returns>The number of bytes written (2-18).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WritePackedGuid128(Span<byte> buffer, ulong low, ulong high)
        {
            // Empty GUID
            if (low == 0 && high == 0)
            {
                buffer[0] = 0;
                buffer[1] = 0;
                return 2;
            }

            Span<byte> lowPacked = stackalloc byte[8];
            Span<byte> highPacked = stackalloc byte[8];

            int loSize = PackUInt64(low, out byte lowMask, lowPacked);
            int hiSize = PackUInt64(high, out byte highMask, highPacked);

            buffer[0] = lowMask;
            buffer[1] = highMask;

            lowPacked.Slice(0, loSize).CopyTo(buffer.Slice(2));
            highPacked.Slice(0, hiSize).CopyTo(buffer.Slice(2 + loSize));

            return 2 + loSize + hiSize;
        }

        /// <summary>
        /// Writes a packed UInt64 to the buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="value">The value to pack.</param>
        /// <returns>The number of bytes written (1-9).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WritePackedUInt64(Span<byte> buffer, ulong value)
        {
            Span<byte> packed = stackalloc byte[8];
            int size = PackUInt64(value, out byte mask, packed);

            buffer[0] = mask;
            packed.Slice(0, size).CopyTo(buffer.Slice(1));

            return 1 + size;
        }

        /// <summary>
        /// Reads a packed GUID128 from the buffer.
        /// </summary>
        /// <param name="buffer">The buffer to read from.</param>
        /// <param name="low">The low 64 bits of the GUID.</param>
        /// <param name="high">The high 64 bits of the GUID.</param>
        /// <returns>The number of bytes read.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadPackedGuid128(ReadOnlySpan<byte> buffer, out ulong low, out ulong high)
        {
            byte lowMask = buffer[0];
            byte highMask = buffer[1];
            int pos = 2;

            low = UnpackUInt64(buffer.Slice(pos), lowMask, out int lowSize);
            pos += lowSize;

            high = UnpackUInt64(buffer.Slice(pos), highMask, out int highSize);
            pos += highSize;

            return pos;
        }

        /// <summary>
        /// Reads a packed UInt64 from the buffer.
        /// </summary>
        /// <param name="buffer">The buffer to read from.</param>
        /// <param name="value">The unpacked value.</param>
        /// <returns>The number of bytes read.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadPackedUInt64(ReadOnlySpan<byte> buffer, out ulong value)
        {
            byte mask = buffer[0];
            value = UnpackUInt64(buffer.Slice(1), mask, out int size);
            return 1 + size;
        }

        /// <summary>
        /// Packs a ulong into non-zero bytes with a bitmask indicating which byte positions are present.
        /// Uses unrolled mask computation and TrailingZeroCount for optimal performance.
        /// </summary>
        /// <param name="value">The value to pack.</param>
        /// <param name="mask">Output: bitmask where bit i is set if byte i of value is non-zero.</param>
        /// <param name="result">Output buffer for packed bytes (must be at least 8 bytes).</param>
        /// <returns>Number of bytes written to result.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int PackUInt64(ulong value, out byte mask, Span<byte> result)
        {
            // Compute 8-bit mask: bit i set iff byte i of value != 0
            // Unrolled for instruction-level parallelism (no loop-carried dependency)
            mask = 0;
            mask |= (byte)(((value) & 0xFFUL) != 0 ? 1 << 0 : 0);
            mask |= (byte)(((value >> 8) & 0xFFUL) != 0 ? 1 << 1 : 0);
            mask |= (byte)(((value >> 16) & 0xFFUL) != 0 ? 1 << 2 : 0);
            mask |= (byte)(((value >> 24) & 0xFFUL) != 0 ? 1 << 3 : 0);
            mask |= (byte)(((value >> 32) & 0xFFUL) != 0 ? 1 << 4 : 0);
            mask |= (byte)(((value >> 40) & 0xFFUL) != 0 ? 1 << 5 : 0);
            mask |= (byte)(((value >> 48) & 0xFFUL) != 0 ? 1 << 6 : 0);
            mask |= (byte)(((value >> 56) & 0xFFUL) != 0 ? 1 << 7 : 0);

            if (mask == 0)
                return 0;

            // Write bytes in order using TrailingZeroCount to find set bits efficiently.
            // Iterates exactly PopCount(mask) times.
            ref byte dst = ref MemoryMarshal.GetReference(result);
            int size = 0;
            uint m = mask;

            while (m != 0)
            {
                int bit = BitOperations.TrailingZeroCount(m);
                Unsafe.Add(ref dst, size) = (byte)(value >> (bit * 8));
                size++;
                m &= (m - 1); // Clear lowest set bit
            }

            return size;
        }

        /// <summary>
        /// Unpacks bytes into a ulong based on a bitmask indicating which byte positions are present.
        /// Uses TrailingZeroCount for optimal bit scanning.
        /// </summary>
        /// <param name="buffer">Input buffer containing packed bytes.</param>
        /// <param name="mask">Bitmask where bit i indicates byte i is present in buffer.</param>
        /// <param name="bytesRead">Output: number of bytes consumed from buffer.</param>
        /// <returns>The unpacked ulong value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong UnpackUInt64(ReadOnlySpan<byte> buffer, byte mask, out int bytesRead)
        {
            if (mask == 0)
            {
                bytesRead = 0;
                return 0;
            }

            ref byte src = ref MemoryMarshal.GetReference(buffer);
            ulong value = 0;
            int idx = 0;
            uint m = mask;

            // Reads exactly PopCount(mask) bytes
            while (m != 0)
            {
                int bit = BitOperations.TrailingZeroCount(m);
                value |= (ulong)Unsafe.Add(ref src, idx) << (bit * 8);
                idx++;
                m &= (m - 1); // Clear lowest set bit
            }

            bytesRead = idx;
            return value;
        }
    }
}
