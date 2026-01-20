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
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;
using Framework.GameMath;

namespace Framework.IO
{
    /// <summary>
    /// High-performance packet reader using ReadOnlySpan&lt;byte&gt; for zero-allocation reads.
    /// This is a ref struct that lives on the stack.
    /// </summary>
    public ref struct SpanPacketReader
    {
        private readonly ReadOnlySpan<byte> _buffer;
        private int _position;
        private byte _bitValue;
        private byte _bitPosition;

        public SpanPacketReader(ReadOnlySpan<byte> buffer)
        {
            _buffer = buffer;
            _position = 0;
            _bitValue = 0;
            _bitPosition = 8;
        }

        public readonly int Position => _position;
        public readonly int Length => _buffer.Length;
        public readonly int Remaining => _buffer.Length - _position;
        public readonly bool CanRead => _position < _buffer.Length;

        #region Integer Read Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadUInt8()
        {
            ResetBitPos();
            return _buffer[_position++];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadInt8()
        {
            ResetBitPos();
            return (sbyte)_buffer[_position++];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16()
        {
            ResetBitPos();
            var value = BinaryPrimitives.ReadUInt16LittleEndian(_buffer.Slice(_position));
            _position += 2;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16()
        {
            ResetBitPos();
            var value = BinaryPrimitives.ReadInt16LittleEndian(_buffer.Slice(_position));
            _position += 2;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32()
        {
            ResetBitPos();
            var value = BinaryPrimitives.ReadUInt32LittleEndian(_buffer.Slice(_position));
            _position += 4;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32()
        {
            ResetBitPos();
            var value = BinaryPrimitives.ReadInt32LittleEndian(_buffer.Slice(_position));
            _position += 4;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            ResetBitPos();
            var value = BinaryPrimitives.ReadUInt64LittleEndian(_buffer.Slice(_position));
            _position += 8;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64()
        {
            ResetBitPos();
            var value = BinaryPrimitives.ReadInt64LittleEndian(_buffer.Slice(_position));
            _position += 8;
            return value;
        }

        #endregion

        #region Float Read Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloat()
        {
            ResetBitPos();
            var value = BinaryPrimitives.ReadSingleLittleEndian(_buffer.Slice(_position));
            _position += 4;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadDouble()
        {
            ResetBitPos();
            var value = BinaryPrimitives.ReadDoubleLittleEndian(_buffer.Slice(_position));
            _position += 8;
            return value;
        }

        #endregion

        #region Vector Read Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 ReadVector2()
        {
            return new Vector2(ReadFloat(), ReadFloat());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ReadVector3()
        {
            return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ReadVector4()
        {
            return new Vector4(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }

        #endregion

        #region Byte/String Read Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBool()
        {
            ResetBitPos();
            return _buffer[_position++] != 0;
        }

        public ReadOnlySpan<byte> ReadBytes(int count)
        {
            ResetBitPos();
            int available = Math.Min(count, Remaining);
            if (available <= 0)
                return ReadOnlySpan<byte>.Empty;

            var slice = _buffer.Slice(_position, available);
            _position += available;
            return slice;
        }

        public void Skip(int count)
        {
            ResetBitPos();
            _position += count;
        }

        /// <summary>
        /// Reads a null-terminated string. Only allocates for the final string.
        /// </summary>
        public string ReadCString()
        {
            ResetBitPos();
            int start = _position;

            while (_position < _buffer.Length && _buffer[_position] != 0)
                _position++;

            var slice = _buffer.Slice(start, _position - start);

            // Skip null terminator
            if (_position < _buffer.Length)
                _position++;

            if (slice.Length == 0)
                return string.Empty;

            return Encoding.UTF8.GetString(slice);
        }

        public string ReadString(int length)
        {
            if (length == 0)
                return string.Empty;

            ResetBitPos();
            var slice = _buffer.Slice(_position, length);
            _position += length;
            return Encoding.UTF8.GetString(slice);
        }

        #endregion

        #region Bit Read Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBit()
        {
            if (_bitPosition == 8)
            {
                _bitValue = _buffer[_position++];
                _bitPosition = 0;
            }

            int returnValue = _bitValue;
            _bitValue = (byte)(_bitValue << 1);
            ++_bitPosition;

            return (returnValue >> 7) != 0;
        }

        public T ReadBits<T>(int bitCount) where T : unmanaged
        {
            int value = 0;

            for (int i = bitCount - 1; i >= 0; --i)
                if (ReadBit())
                    value |= (1 << i);

            return (T)Convert.ChangeType(value, typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetBitPos()
        {
            if (_bitPosition > 7)
                return;

            _bitPosition = 8;
            _bitValue = 0;
        }

        #endregion

        #region Packed GUID Methods

        /// <summary>
        /// Reads a packed GUID128 (low and high parts).
        /// </summary>
        public void ReadPackedGuid128(out ulong low, out ulong high)
        {
            ResetBitPos();
            int bytesRead = PackedGuidHelper.ReadPackedGuid128(_buffer.Slice(_position), out low, out high);
            _position += bytesRead;
        }

        /// <summary>
        /// Reads a packed UInt64.
        /// </summary>
        public ulong ReadPackedUInt64()
        {
            ResetBitPos();
            int bytesRead = PackedGuidHelper.ReadPackedUInt64(_buffer.Slice(_position), out ulong value);
            _position += bytesRead;
            return value;
        }

        #endregion

        /// <summary>
        /// Reads remaining bytes to end of buffer.
        /// </summary>
        public ReadOnlySpan<byte> ReadToEnd()
        {
            return ReadBytes(Remaining);
        }

        /// <summary>
        /// Resets position to beginning of buffer.
        /// </summary>
        public void ResetReadPos()
        {
            _position = 0;
            ResetBitPos();
        }
    }
}
