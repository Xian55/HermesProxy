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

using Framework.GameMath;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Framework.IO;

public class ByteBuffer : IDisposable
{
    private const int DefaultWriteCapacity = 256;

    private byte[] _buffer;
    private int _position;
    private int _length;
    private bool _isPooledBuffer;
    private readonly bool _isWriteMode;
    private bool _disposed;
    private byte _bitPosition = 8;
    private byte _bitValue;

    private MemoryStream? _compatStream;

    public ByteBuffer()
    {
        _buffer = ArrayPool<byte>.Shared.Rent(DefaultWriteCapacity);
        _position = 0;
        _length = 0;
        _isPooledBuffer = true;
        _isWriteMode = true;
    }

    public ByteBuffer(byte[] data)
    {
        _buffer = data;
        _position = 0;
        _length = data.Length;
        _isPooledBuffer = false;
        _isWriteMode = false;
    }

    ~ByteBuffer()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;

        if (_isPooledBuffer && _buffer != null)
        {
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = null!;
        }

        if (disposing)
            _compatStream?.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCapacity(int additionalBytes)
    {
        int required = _position + additionalBytes;
        if (required <= _buffer.Length) return;

        int newSize = Math.Max(_buffer.Length * 2, required);
        byte[] newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
        _buffer.AsSpan(0, _length).CopyTo(newBuffer);

        if (_isPooledBuffer)
            ArrayPool<byte>.Shared.Return(_buffer);

        _buffer = newBuffer;
        _isPooledBuffer = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AdvanceWrite(int bytes)
    {
        _position += bytes;
        if (_position > _length)
            _length = _position;
    }

    #region Read Methods
    public sbyte ReadInt8()
    {
        ResetBitPos();
        sbyte value = (sbyte)_buffer[_position];
        _position++;
        return value;
    }

    public short ReadInt16()
    {
        ResetBitPos();
        short value = BinaryPrimitives.ReadInt16LittleEndian(_buffer.AsSpan(_position));
        _position += 2;
        return value;
    }

    public int ReadInt32()
    {
        ResetBitPos();
        int value = BinaryPrimitives.ReadInt32LittleEndian(_buffer.AsSpan(_position));
        _position += 4;
        return value;
    }

    public long ReadInt64()
    {
        ResetBitPos();
        long value = BinaryPrimitives.ReadInt64LittleEndian(_buffer.AsSpan(_position));
        _position += 8;
        return value;
    }

    public byte ReadUInt8()
    {
        ResetBitPos();
        byte value = _buffer[_position];
        _position++;
        return value;
    }

    public ushort ReadUInt16()
    {
        ResetBitPos();
        ushort value = BinaryPrimitives.ReadUInt16LittleEndian(_buffer.AsSpan(_position));
        _position += 2;
        return value;
    }

    public uint ReadUInt32()
    {
        ResetBitPos();
        uint value = BinaryPrimitives.ReadUInt32LittleEndian(_buffer.AsSpan(_position));
        _position += 4;
        return value;
    }

    public ulong ReadUInt64()
    {
        ResetBitPos();
        ulong value = BinaryPrimitives.ReadUInt64LittleEndian(_buffer.AsSpan(_position));
        _position += 8;
        return value;
    }

    public float ReadFloat()
    {
        ResetBitPos();
        float value = BinaryPrimitives.ReadSingleLittleEndian(_buffer.AsSpan(_position));
        _position += 4;
        return value;
    }

    public double ReadDouble()
    {
        ResetBitPos();
        double value = BinaryPrimitives.ReadDoubleLittleEndian(_buffer.AsSpan(_position));
        _position += 8;
        return value;
    }

    public T ReadByteEnum<T>() where T : Enum
    {
        return (T)(object)ReadUInt8();
    }

    public string ReadCString()
    {
        ResetBitPos();

        int startPos = _position;

        // Scan for null terminator
        while (_position < _length && _buffer[_position] != 0)
        {
            _position++;
        }

        int strLength = _position - startPos;

        // Skip null terminator
        if (_position < _length)
            _position++;

        if (strLength <= 0)
            return string.Empty;

        return Encoding.UTF8.GetString(_buffer, startPos, strLength);
    }

    /// <summary>
    /// Original implementation for benchmarking comparison. DO NOT USE.
    /// </summary>
    internal string ReadCStringOriginal()
    {
        ResetBitPos();
        StringBuilder tmpString = new StringBuilder();

        while (_position < _length)
        {
            byte b = _buffer[_position++];
            if (b == 0)
                break;
            tmpString.Append((char)b);
        }

        return tmpString.ToString();
    }

    public string ReadString(uint length)
    {
        if (length == 0)
            return "";

        ResetBitPos();
        return Encoding.UTF8.GetString(ReadBytes(length));
    }

    public bool ReadBool()
    {
        ResetBitPos();
        byte value = _buffer[_position];
        _position++;
        return value != 0;
    }

    public byte[] ReadBytes(uint count)
    {
        ResetBitPos();
        int available = _length - _position;
        int toRead = Math.Min((int)count, available);

        if (toRead <= 0)
            return [];

        byte[] result = new byte[toRead];
        _buffer.AsSpan(_position, toRead).CopyTo(result);
        _position += toRead;
        return result;
    }

    public void Skip(int count)
    {
        ResetBitPos();
        _position += count;
    }

    public bool CanRead()
    {
        return _position < _length;
    }

    public uint ReadPackedTime()
    {
        return (uint)Time.GetUnixTimeFromPackedTime(ReadUInt32());
    }

    public DateTime ReadTime()
    {
        return DateTimeOffset.FromUnixTimeSeconds(ReadUInt32()).DateTime;
    }

    public DateTime ReadTime64()
    {
        return DateTimeOffset.FromUnixTimeSeconds((int)ReadUInt64()).DateTime;
    }

    public Vector2 ReadVector2()
    {
        return new Vector2(ReadFloat(), ReadFloat());
    }

    public Vector3 ReadVector3()
    {
        return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
    }

    public Vector3 ReadPackedVector3()
    {
        int packed = ReadInt32();
        float x = ((packed & 0x7FF) << 21 >> 21) * 0.25f;
        float y = ((((packed >> 11) & 0x7FF) << 21) >> 21) * 0.25f;
        float z = ((packed >> 22 << 22) >> 22) * 0.25f;
        return new Vector3(x, y, z);
    }

    public Vector4 ReadVector4()
    {
        return new Vector4(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
    }

    public Quaternion ReadPackedQuaternion()
    {
        long packed = ReadInt64();
        return NumericsExtensions.FromPackedLong(packed);
    }

    public Quaternion ReadQuaternion()
    {
        return new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
    }

    // BitPacking
    public bool ReadBit()
    {
        if (_bitPosition == 8)
        {
            _bitValue = ReadUInt8();
            _bitPosition = 0;
        }

        int returnValue = _bitValue;
        _bitValue = (byte)(2 * returnValue); // BitValue <<= 1;
        ++_bitPosition;

        return (returnValue >> 7) != 0;
    }

    public bool HasBit()
    {
        if (_bitPosition == 8)
        {
            _bitValue = ReadUInt8();
            _bitPosition = 0;
        }

        int returnValue = _bitValue;
        _bitValue = (byte)(2 * returnValue);
        ++_bitPosition;

        return Convert.ToBoolean(returnValue >> 7);
    }

    /// <summary>
    /// Reads <paramref name="bitCount"/> bits MSB-first into a <see cref="uint"/>
    /// accumulator and reinterprets it as <typeparamref name="T"/>. Supports
    /// <c>bitCount</c> in [0, 32]. <typeparamref name="T"/> must be at most
    /// 4 bytes (byte/sbyte, ushort/short, uint/int) — larger types would read
    /// uninitialized stack bytes.
    ///
    /// The previous formulation used <c>int</c> as the accumulator, which made
    /// the bit-31 contribution equal to <see cref="int.MinValue"/>; the trailing
    /// <see cref="Convert.ChangeType(object, Type)"/> then threw
    /// <see cref="OverflowException"/> when converting that negative int back to
    /// <see cref="uint"/>. The new <see cref="uint"/> accumulator + reinterpret
    /// also drops the per-call <c>Convert.ChangeType</c> boxing.
    /// </summary>
    public T ReadBits<T>(int bitCount) where T : unmanaged
    {
        uint value = 0;

        for (var i = bitCount - 1; i >= 0; --i)
            if (HasBit())
                value |= 1u << i;

        return Unsafe.As<uint, T>(ref value);
    }
    #endregion

    #region Write Methods
    public void WriteInt8(sbyte data)
    {
        FlushBits();
        EnsureCapacity(1);
        _buffer[_position] = (byte)data;
        AdvanceWrite(1);
    }

    public void WriteInt16(short data)
    {
        FlushBits();
        EnsureCapacity(2);
        BinaryPrimitives.WriteInt16LittleEndian(_buffer.AsSpan(_position), data);
        AdvanceWrite(2);
    }

    public void WriteInt32(int data)
    {
        FlushBits();
        EnsureCapacity(4);
        BinaryPrimitives.WriteInt32LittleEndian(_buffer.AsSpan(_position), data);
        AdvanceWrite(4);
    }

    public void WriteInt64(long data)
    {
        FlushBits();
        EnsureCapacity(8);
        BinaryPrimitives.WriteInt64LittleEndian(_buffer.AsSpan(_position), data);
        AdvanceWrite(8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBool(bool data)
    {
        FlushBits();
        EnsureCapacity(1);
        _buffer[_position] = Unsafe.As<bool, byte>(ref data);
        AdvanceWrite(1);
    }

    /// <summary>
    /// Original implementation for benchmarking comparison. DO NOT USE.
    /// </summary>
    internal void WriteBoolOriginal(bool data)
    {
        FlushBits();
        EnsureCapacity(1);
        _buffer[_position] = data ? (byte)1 : (byte)0;
        AdvanceWrite(1);
    }

    public void WriteUInt8(byte data)
    {
        FlushBits();
        EnsureCapacity(1);
        _buffer[_position] = data;
        AdvanceWrite(1);
    }

    public void WriteUInt16(ushort data)
    {
        FlushBits();
        EnsureCapacity(2);
        BinaryPrimitives.WriteUInt16LittleEndian(_buffer.AsSpan(_position), data);
        AdvanceWrite(2);
    }

    public void WriteUInt32(uint data)
    {
        FlushBits();
        EnsureCapacity(4);
        BinaryPrimitives.WriteUInt32LittleEndian(_buffer.AsSpan(_position), data);
        AdvanceWrite(4);
    }

    public void WriteUInt64(ulong data)
    {
        FlushBits();
        EnsureCapacity(8);
        BinaryPrimitives.WriteUInt64LittleEndian(_buffer.AsSpan(_position), data);
        AdvanceWrite(8);
    }

    public void WriteFloat(float data)
    {
        FlushBits();
        EnsureCapacity(4);
        BinaryPrimitives.WriteSingleLittleEndian(_buffer.AsSpan(_position), data);
        AdvanceWrite(4);
    }

    public void WriteDouble(double data)
    {
        FlushBits();
        EnsureCapacity(8);
        BinaryPrimitives.WriteDoubleLittleEndian(_buffer.AsSpan(_position), data);
        AdvanceWrite(8);
    }

    /// <summary>
    /// Writes a UTF-8 string to the buffer terminated with a NUL byte. Single-pass:
    /// one FlushBits, one EnsureCapacity, encode directly into the destination span,
    /// then write the NUL byte. Avoids the byte[] allocation the original WriteString
    /// went through.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteCString(string? str)
    {
        FlushBits();

        if (string.IsNullOrEmpty(str))
        {
            EnsureCapacity(1);
            _buffer[_position] = 0;
            AdvanceWrite(1);
            return;
        }

        int byteCount = Encoding.UTF8.GetByteCount(str);
        EnsureCapacity(byteCount + 1);
        Encoding.UTF8.GetBytes(str, _buffer.AsSpan(_position, byteCount));
        _buffer[_position + byteCount] = 0;
        AdvanceWrite(byteCount + 1);
    }

    /// <summary>
    /// Original implementation for benchmarking comparison. DO NOT USE.
    /// </summary>
    internal void WriteCStringOriginal(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            WriteUInt8(0);
            return;
        }

        WriteStringOriginal(str);
        WriteUInt8(0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteString(string? str)
    {
        if (string.IsNullOrEmpty(str))
            return;

        FlushBits();
        int byteCount = Encoding.UTF8.GetByteCount(str);
        EnsureCapacity(byteCount);
        Encoding.UTF8.GetBytes(str, _buffer.AsSpan(_position, byteCount));
        AdvanceWrite(byteCount);
    }

    /// <summary>
    /// Original implementation for benchmarking comparison. DO NOT USE.
    /// </summary>
    internal void WriteStringOriginal(string str)
    {
        if (str.IsEmpty())
            return;

        byte[] sBytes = Encoding.UTF8.GetBytes(str);
        WriteBytes(sBytes);
    }

    public void WriteBytes(byte[] data)
    {
        FlushBits();
        EnsureCapacity(data.Length);
        data.CopyTo(_buffer.AsSpan(_position));
        AdvanceWrite(data.Length);
    }

    public void WriteBytes(Span<byte> data)
    {
        FlushBits();
        EnsureCapacity(data.Length);
        data.CopyTo(_buffer.AsSpan(_position));
        AdvanceWrite(data.Length);
    }

    public void WriteBytes(byte[] data, uint count)
    {
        FlushBits();
        EnsureCapacity((int)count);
        data.AsSpan(0, (int)count).CopyTo(_buffer.AsSpan(_position));
        AdvanceWrite((int)count);
    }

    public void WriteBytes(ByteBuffer buffer)
    {
        WriteBytes(buffer.GetData());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVector4(Vector4 pos)
    {
        FlushBits();
        EnsureCapacity(16);
        var dst = _buffer.AsSpan(_position, 16);
        if (BitConverter.IsLittleEndian)
        {
            MemoryMarshal.Write(dst, in pos);
        }
        else
        {
            BinaryPrimitives.WriteSingleLittleEndian(dst,             pos.X);
            BinaryPrimitives.WriteSingleLittleEndian(dst.Slice(4),    pos.Y);
            BinaryPrimitives.WriteSingleLittleEndian(dst.Slice(8),    pos.Z);
            BinaryPrimitives.WriteSingleLittleEndian(dst.Slice(12),   pos.W);
        }
        AdvanceWrite(16);
    }

    /// <summary>
    /// Original implementation for benchmarking comparison. DO NOT USE.
    /// </summary>
    internal void WriteVector4Original(Vector4 pos)
    {
        WriteFloat(pos.X);
        WriteFloat(pos.Y);
        WriteFloat(pos.Z);
        WriteFloat(pos.W);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVector3(Vector3 pos)
    {
        FlushBits();
        EnsureCapacity(12);
        var dst = _buffer.AsSpan(_position, 12);
        if (BitConverter.IsLittleEndian)
        {
            MemoryMarshal.Write(dst, in pos);
        }
        else
        {
            BinaryPrimitives.WriteSingleLittleEndian(dst,           pos.X);
            BinaryPrimitives.WriteSingleLittleEndian(dst.Slice(4),  pos.Y);
            BinaryPrimitives.WriteSingleLittleEndian(dst.Slice(8),  pos.Z);
        }
        AdvanceWrite(12);
    }

    /// <summary>
    /// Original implementation for benchmarking comparison. DO NOT USE.
    /// </summary>
    internal void WriteVector3Original(Vector3 pos)
    {
        WriteFloat(pos.X);
        WriteFloat(pos.Y);
        WriteFloat(pos.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVector2(Vector2 pos)
    {
        FlushBits();
        EnsureCapacity(8);
        var dst = _buffer.AsSpan(_position, 8);
        if (BitConverter.IsLittleEndian)
        {
            MemoryMarshal.Write(dst, in pos);
        }
        else
        {
            BinaryPrimitives.WriteSingleLittleEndian(dst,           pos.X);
            BinaryPrimitives.WriteSingleLittleEndian(dst.Slice(4),  pos.Y);
        }
        AdvanceWrite(8);
    }

    /// <summary>
    /// Original implementation for benchmarking comparison. DO NOT USE.
    /// </summary>
    internal void WriteVector2Original(Vector2 pos)
    {
        WriteFloat(pos.X);
        WriteFloat(pos.Y);
    }

    public void WritePackXYZ(Vector3 pos)
    {
        // Pack X (11 bits), Y (11 bits), Z (10 bits) into a single uint32.
        // Multiply by 4f instead of dividing by 0.25f — the JIT cannot rewrite
        // float division to multiplication automatically due to rounding semantics.
        uint packed  =  (uint)(int)(pos.X * 4f) & 0x7FF;
        packed      |= ((uint)(int)(pos.Y * 4f) & 0x7FF) << 11;
        packed      |= ((uint)(int)(pos.Z * 4f) & 0x3FF) << 22;
        WriteUInt32(packed);
    }

    /// <summary>
    /// Original implementation for benchmarking comparison. DO NOT USE.
    /// </summary>
    internal void WritePackXYZOriginal(Vector3 pos)
    {
        // Cast to int first to preserve negative values (two's complement),
        // then mask to extract the correct number of bits
        uint packed = 0;
        packed |= (uint)((int)(pos.X / 0.25f)) & 0x7FF;
        packed |= ((uint)((int)(pos.Y / 0.25f)) & 0x7FF) << 11;
        packed |= ((uint)((int)(pos.Z / 0.25f)) & 0x3FF) << 22;
        WriteUInt32(packed);
    }

    public bool WriteBit(bool bit)
    {
        --_bitPosition;

        if (bit)
            _bitValue |= (byte)(1 << _bitPosition);

        if (_bitPosition == 0)
        {
            EnsureCapacity(1);
            _buffer[_position] = _bitValue;
            AdvanceWrite(1);

            _bitPosition = 8;
            _bitValue = 0;
        }
        return bit;
    }

    /// <summary>
    /// Chunked bit packing — takes <c>min(bitCount, _bitPosition)</c> bits per
    /// iteration, ORs them into the pending byte at the right shift, and emits
    /// the byte when full. Identical wire output to the per-bit loop, ~10-30x
    /// fewer iterations on common widths (4, 6, 8, 9, 16, 24, 32).
    /// </summary>
    public void WriteBits(uint value, int bitCount)
    {
        while (bitCount > 0)
        {
            int take = Math.Min(bitCount, (int)_bitPosition);
            int shift = bitCount - take;
            uint chunk = (value >> shift) & ((1u << take) - 1u);
            _bitPosition -= (byte)take;
            _bitValue |= (byte)(chunk << _bitPosition);
            bitCount = shift;

            if (_bitPosition == 0)
            {
                EnsureCapacity(1);
                _buffer[_position] = _bitValue;
                AdvanceWrite(1);
                _bitPosition = 8;
                _bitValue = 0;
            }
        }
    }

    /// <summary>
    /// Convenience overload for the many call sites that pass <see cref="int"/>
    /// (e.g. <c>List.Count</c>, <c>Encoding.UTF8.GetByteCount(s)</c>, small enum
    /// casts). The <c>(uint)</c> cast wraps negative values to <c>0xFFFFFFFF</c>;
    /// WriteBits is only ever called with non-negative bit-pack values in this
    /// codebase, so wraparound is the safer behavior than the throwing
    /// <c>Convert.ToUInt32(int)</c> the obsolete <c>(object, int)</c> shim
    /// preserves.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBits(int value, int bitCount) => WriteBits((uint)value, bitCount);

    /// <summary>
    /// Legacy overload kept for source compatibility — boxes via
    /// <see cref="Convert.ToUInt32(object)"/>. Prefer <c>WriteBits(uint,int)</c>
    /// or <c>WriteBits(int,int)</c>.
    /// </summary>
    [Obsolete("Use WriteBits(uint, int) or WriteBits(int, int) — the (object, int) overload boxes its first argument.")]
    public void WriteBits(object bit, int count) => WriteBits(Convert.ToUInt32(bit), count);

    /// <summary>
    /// Original implementation for benchmarking comparison. DO NOT USE.
    /// </summary>
    internal void WriteBitsOriginal(uint bit, int count)
    {
        for (int i = count - 1; i >= 0; --i)
            WriteBit(((bit >> i) & 1) != 0);
    }

    public void WritePackedTime(long time)
    {
        WriteUInt32(Time.GetPackedTimeFromUnixTime(time));
    }

    public void WritePackedTime()
    {
        WriteUInt32(Time.GetPackedTimeFromDateTime(DateTime.Now));
    }

    public void WriteByteEnum<T>(T x) where T : Enum
    {
        WriteUInt8((byte)(object)x);
    }

    public void WriteUint32Enum<T>(T x) where T : Enum
    {
        WriteUInt32((uint)(object)x);
    }

    #endregion

    public bool HasUnfinishedBitPack()
    {
        return _bitPosition != 8;
    }

    public void FlushBits()
    {
        if (_bitPosition == 8)
            return;

        EnsureCapacity(1);
        _buffer[_position] = _bitValue;
        AdvanceWrite(1);
        _bitValue = 0;
        _bitPosition = 8;
    }

    public void ResetBitPos()
    {
        if (_bitPosition > 7)
            return;

        _bitPosition = 8;
        _bitValue = 0;
    }

    public void ResetReadPos()
    {
        _position = 0;
        ResetBitPos();
    }

    public byte[] ReadToEnd()
    {
        var remaining = (uint)(_length - _position);
        return ReadBytes(remaining);
    }

    public byte[] GetData()
    {
        if (_isWriteMode)
        {
            FlushBits();
            // Return only actual data, not the potentially larger pooled buffer
            byte[] result = new byte[_length];
            _buffer.AsSpan(0, _length).CopyTo(result);
            return result;
        }
        else
        {
            // For read mode, return the original buffer (which IS the right size)
            return _buffer;
        }
    }

    /// <summary>
    /// Original implementation for benchmarking comparison. DO NOT USE.
    /// </summary>
    internal byte[] GetDataOriginal()
    {
        if (_isWriteMode)
        {
            FlushBits();
            var data = new byte[_length];
            for (int i = 0; i < _length; i++)
                data[i] = _buffer[i];
            return data;
        }
        else
        {
            var data = new byte[_length];
            for (int i = 0; i < _length; i++)
                data[i] = _buffer[i];
            return data;
        }
    }

    public uint GetSize()
    {
        return (uint)_length;
    }

    [Obsolete("Use GetData() instead. This creates a MemoryStream copy for compatibility.")]
    public Stream GetCurrentStream()
    {
        if (_compatStream == null)
        {
            if (_isWriteMode)
            {
                FlushBits();
                _compatStream = new MemoryStream(_buffer, 0, _length);
            }
            else
            {
                _compatStream = new MemoryStream(_buffer, 0, _length);
            }
        }
        return _compatStream;
    }

    public void Clear()
    {
        _bitPosition = 8;
        _bitValue = 0;
        _position = 0;
        _length = 0;
        _compatStream?.Dispose();
        _compatStream = null;
        // Keep the existing buffer for reuse
    }

    // Hex Printer from WPP
    // https://github.com/TrinityCore/WowPacketParser/blob/7edfda7e4daf9a5b9069083806a9a3c261dea8a7/WowPacketParser/Misc/Utilities.cs#L48
    public void DebugPrintHex()
    {
        const bool shortVersion = false;
        const int offset = 0;
        const bool noOffsetFirstLine = false;

        var data = GetData();

        var n = Environment.NewLine;

        var prefix = new string(' ', offset);

        var hexDump = new StringBuilder(noOffsetFirstLine ? "" : prefix);

        if (!shortVersion)
        {
            var header = "|-------------------------------------------------|---------------------------------|" + n +
                         "| 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F | 0 1 2 3 4 5 6 7 8 9 A B C D E F |" + n +
                         "|-------------------------------------------------|---------------------------------|" + n;

            hexDump.Append(header);
        }

        for (var i = 0; i < data.Length; i += 16)
        {
            var text = new StringBuilder();
            var hex = new StringBuilder(i == 0 ? "" : prefix);

            if (!shortVersion)
                hex.Append("| ");

            for (var j = 0; j < 16; j++)
            {
                if (j + i < data.Length)
                {
                    var val = data[j + i];
                    hex.Append(data[j + i].ToString("X2"));

                    if (!shortVersion)
                        hex.Append(" ");

                    if (val >= 32 && val <= 127)
                        text.Append((char)val);
                    else
                        text.Append(".");

                    if (!shortVersion)
                        text.Append(" ");
                }
                else
                {
                    hex.Append(shortVersion ? "  " : "   ");
                    text.Append(shortVersion ? " " : "  ");
                }
            }

            hex.Append(shortVersion ? "|" : "| ");
            hex.Append(text);
            if (!shortVersion)
                hex.Append("|");
            hex.Append(n);
            hexDump.Append(hex);
        }

        if (!shortVersion)
            hexDump.Append("|-------------------------------------------------|---------------------------------|");

        Console.WriteLine(hexDump);
    }
}
