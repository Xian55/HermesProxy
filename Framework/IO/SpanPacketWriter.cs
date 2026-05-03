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
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Framework.GameMath;

namespace Framework.IO;

/// <summary>
/// High-performance packet writer over a <see cref="Span{T}"/> of
/// <see cref="byte"/> for zero-allocation writes. This is a
/// <see langword="ref struct"/> that lives on the stack.
/// </summary>
public ref struct SpanPacketWriter
{
    private readonly Span<byte> _buffer;
    private int _position;
    private byte _bitValue;
    private byte _bitPosition;

    public SpanPacketWriter(Span<byte> buffer)
    {
        _buffer = buffer;
        _position = 0;
        _bitValue = 0;
        _bitPosition = 8;
    }

    public readonly int Position => _position;
    public readonly int Remaining => _buffer.Length - _position;

    #region Integer Write Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt8(byte value)
    {
        FlushBits();
        _buffer[_position++] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt8(sbyte value)
    {
        FlushBits();
        _buffer[_position++] = (byte)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt16(ushort value)
    {
        FlushBits();
        BinaryPrimitives.WriteUInt16LittleEndian(_buffer.Slice(_position), value);
        _position += 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt16(short value)
    {
        FlushBits();
        BinaryPrimitives.WriteInt16LittleEndian(_buffer.Slice(_position), value);
        _position += 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt32(uint value)
    {
        FlushBits();
        BinaryPrimitives.WriteUInt32LittleEndian(_buffer.Slice(_position), value);
        _position += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt32(int value)
    {
        FlushBits();
        BinaryPrimitives.WriteInt32LittleEndian(_buffer.Slice(_position), value);
        _position += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt64(ulong value)
    {
        FlushBits();
        BinaryPrimitives.WriteUInt64LittleEndian(_buffer.Slice(_position), value);
        _position += 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt64(long value)
    {
        FlushBits();
        BinaryPrimitives.WriteInt64LittleEndian(_buffer.Slice(_position), value);
        _position += 8;
    }

    #endregion

    #region Float Write Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFloat(float value)
    {
        FlushBits();
        BinaryPrimitives.WriteSingleLittleEndian(_buffer.Slice(_position), value);
        _position += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteDouble(double value)
    {
        FlushBits();
        BinaryPrimitives.WriteDoubleLittleEndian(_buffer.Slice(_position), value);
        _position += 8;
    }

    #endregion

    #region Vector Write Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVector2(Vector2 value)
    {
        WriteFloat(value.X);
        WriteFloat(value.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVector3(Vector3 value)
    {
        WriteFloat(value.X);
        WriteFloat(value.Y);
        WriteFloat(value.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVector4(Vector4 value)
    {
        WriteFloat(value.X);
        WriteFloat(value.Y);
        WriteFloat(value.Z);
        WriteFloat(value.W);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WritePackXYZ(Vector3 pos)
    {
        // Pack X (11 bits), Y (11 bits), Z (10 bits) into a single uint32
        uint packed = 0;
        packed |= (uint)((int)(pos.X / 0.25f)) & 0x7FF;
        packed |= ((uint)((int)(pos.Y / 0.25f)) & 0x7FF) << 11;
        packed |= ((uint)((int)(pos.Z / 0.25f)) & 0x3FF) << 22;
        WriteUInt32(packed);
    }

    #endregion

    #region Byte/String Write Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBytes(ReadOnlySpan<byte> data)
    {
        FlushBits();
        data.CopyTo(_buffer.Slice(_position));
        _position += data.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBool(bool value)
    {
        FlushBits();
        _buffer[_position++] = value ? (byte)1 : (byte)0;
    }

    public void WriteCString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            WriteUInt8(0);
            return;
        }

        WriteString(value);
        WriteUInt8(0);
    }

    public void WriteString(string value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        FlushBits();
        int bytesWritten = Encoding.UTF8.GetBytes(value, _buffer.Slice(_position));
        _position += bytesWritten;
    }

    #endregion

    #region Bit Write Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool WriteBit(bool value)
    {
        --_bitPosition;

        if (value)
            _bitValue |= (byte)(1 << _bitPosition);

        if (_bitPosition == 0)
        {
            _buffer[_position++] = _bitValue;
            _bitPosition = 8;
            _bitValue = 0;
        }

        return value;
    }

    public void WriteBits(uint value, int bitCount)
    {
        for (int i = bitCount - 1; i >= 0; --i)
            WriteBit(((value >> i) & 1) != 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBits<T>(T value, int bitCount) where T : unmanaged, Enum
    {
        uint v;
        if (Unsafe.SizeOf<T>() == 1)
            v = Unsafe.As<T, byte>(ref value);
        else if (Unsafe.SizeOf<T>() == 2)
            v = Unsafe.As<T, ushort>(ref value);
        else if (Unsafe.SizeOf<T>() == 4)
            v = Unsafe.As<T, uint>(ref value);
        else
            v = (uint)Unsafe.As<T, ulong>(ref value);
        WriteBits(v, bitCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FlushBits()
    {
        if (_bitPosition == 8)
            return;

        _buffer[_position++] = _bitValue;
        _bitValue = 0;
        _bitPosition = 8;
    }

    public readonly bool HasUnfinishedBitPack => _bitPosition != 8;

    #endregion

    #region Packed GUID Methods

    /// <summary>
    /// Writes a packed GUID128 (low and high parts).
    /// </summary>
    public void WritePackedGuid128(ulong low, ulong high)
    {
        FlushBits();
        int written = PackedGuidHelper.WritePackedGuid128(_buffer.Slice(_position), low, high);
        _position += written;
    }

    /// <summary>
    /// Writes a packed UInt64.
    /// </summary>
    public void WritePackedUInt64(ulong value)
    {
        FlushBits();
        int written = PackedGuidHelper.WritePackedUInt64(_buffer.Slice(_position), value);
        _position += written;
    }

    #endregion

    /// <summary>
    /// Returns the written portion of the buffer as a ReadOnlySpan.
    /// </summary>
    public ReadOnlySpan<byte> GetWrittenSpan()
    {
        FlushBits();
        return _buffer.Slice(0, _position);
    }

    /// <summary>
    /// Copies the written data to a new byte array.
    /// </summary>
    public byte[] ToArray()
    {
        return GetWrittenSpan().ToArray();
    }
}
