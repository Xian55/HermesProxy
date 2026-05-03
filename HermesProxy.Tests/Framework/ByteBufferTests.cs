using System;
using System.Buffers.Binary;
using System.Text;
using Framework.IO;
using Xunit;

namespace HermesProxy.Tests.Framework;

public class ByteBufferReadCStringTests
{
    [Fact]
    public void ReadCString_WithEmptyString_ReturnsEmpty()
    {
        // Arrange - just a null terminator
        var data = new byte[] { 0x00 };
        using var buffer = new ByteBuffer(data);

        // Act
        var result = buffer.ReadCString();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ReadCString_WithSimpleAscii_ReturnsCorrectString()
    {
        // Arrange - "Hello" + null terminator
        var data = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x00 };
        using var buffer = new ByteBuffer(data);

        // Act
        var result = buffer.ReadCString();

        // Assert
        Assert.Equal("Hello", result);
    }

    [Fact]
    public void ReadCString_WithUtf8_ReturnsCorrectString()
    {
        // Arrange - UTF-8 encoded string with multi-byte characters
        var testString = "Héllo Wörld";
        var stringBytes = Encoding.UTF8.GetBytes(testString);
        var data = new byte[stringBytes.Length + 1];
        stringBytes.CopyTo(data, 0);
        data[^1] = 0x00; // null terminator

        using var buffer = new ByteBuffer(data);

        // Act
        var result = buffer.ReadCString();

        // Assert
        Assert.Equal(testString, result);
    }

    [Fact]
    public void ReadCString_WithMultipleStrings_ReadsSequentially()
    {
        // Arrange - "One" + null + "Two" + null
        var data = new byte[] { 0x4F, 0x6E, 0x65, 0x00, 0x54, 0x77, 0x6F, 0x00 };
        using var buffer = new ByteBuffer(data);

        // Act
        var result1 = buffer.ReadCString();
        var result2 = buffer.ReadCString();

        // Assert
        Assert.Equal("One", result1);
        Assert.Equal("Two", result2);
    }

    [Fact]
    public void ReadCString_WithLongString_ReturnsCorrectString()
    {
        // Arrange - string longer than 256 bytes (stackalloc threshold)
        var testString = new string('A', 300);
        var stringBytes = Encoding.UTF8.GetBytes(testString);
        var data = new byte[stringBytes.Length + 1];
        stringBytes.CopyTo(data, 0);
        data[^1] = 0x00;

        using var buffer = new ByteBuffer(data);

        // Act
        var result = buffer.ReadCString();

        // Assert
        Assert.Equal(testString, result);
        Assert.Equal(300, result.Length);
    }

    [Fact]
    public void ReadCString_With256ByteString_UsesStackalloc()
    {
        // Arrange - exactly 256 bytes (boundary case for stackalloc)
        var testString = new string('B', 256);
        var stringBytes = Encoding.UTF8.GetBytes(testString);
        var data = new byte[stringBytes.Length + 1];
        stringBytes.CopyTo(data, 0);
        data[^1] = 0x00;

        using var buffer = new ByteBuffer(data);

        // Act
        var result = buffer.ReadCString();

        // Assert
        Assert.Equal(testString, result);
        Assert.Equal(256, result.Length);
    }

    [Fact]
    public void ReadCString_WithSpecialCharacters_ReturnsCorrectString()
    {
        // Arrange - string with special UTF-8 characters (emoji, CJK)
        var testString = "Test 🎮 游戏";
        var stringBytes = Encoding.UTF8.GetBytes(testString);
        var data = new byte[stringBytes.Length + 1];
        stringBytes.CopyTo(data, 0);
        data[^1] = 0x00;

        using var buffer = new ByteBuffer(data);

        // Act
        var result = buffer.ReadCString();

        // Assert
        Assert.Equal(testString, result);
    }

    [Fact]
    public void ReadCString_PositionAdvancesCorrectly()
    {
        // Arrange - "ABC" + null + extra bytes
        var data = new byte[] { 0x41, 0x42, 0x43, 0x00, 0xFF, 0xFF };
        using var buffer = new ByteBuffer(data);

        // Act
        var result = buffer.ReadCString();
        var nextByte = buffer.ReadUInt8();

        // Assert
        Assert.Equal("ABC", result);
        Assert.Equal(0xFF, nextByte);
    }
}

public class ByteBufferGetDataTests
{
    [Fact]
    public void GetData_WithEmptyBuffer_ReturnsEmptyArray()
    {
        // Arrange
        using var buffer = new ByteBuffer();

        // Act
        var result = buffer.GetData();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetData_WithWrittenData_ReturnsCorrectBytes()
    {
        // Arrange
        using var buffer = new ByteBuffer();
        buffer.WriteUInt8(0x01);
        buffer.WriteUInt8(0x02);
        buffer.WriteUInt8(0x03);

        // Act
        var result = buffer.GetData();

        // Assert
        Assert.Equal(new byte[] { 0x01, 0x02, 0x03 }, result);
    }

    [Fact]
    public void GetData_WithLargeData_ReturnsAllBytes()
    {
        // Arrange
        using var buffer = new ByteBuffer();
        var testData = new byte[4096];
        for (int i = 0; i < testData.Length; i++)
            testData[i] = (byte)(i & 0xFF);

        buffer.WriteBytes(testData);

        // Act
        var result = buffer.GetData();

        // Assert
        Assert.Equal(testData.Length, result.Length);
        Assert.Equal(testData, result);
    }

    [Fact]
    public void GetData_PreservesBufferState()
    {
        // Arrange
        using var buffer = new ByteBuffer();
        buffer.WriteUInt32(0x12345678);
        buffer.WriteUInt32(0xDEADBEEF);

        // Act - GetData should not affect the buffer's write position
        var result = buffer.GetData();

        // Write more data after GetData to verify position is preserved
        buffer.WriteUInt32(0xCAFEBABE);
        var resultAfterWrite = buffer.GetData();

        // Assert
        Assert.Equal(8, result.Length);
        Assert.Equal(12, resultAfterWrite.Length); // Original 8 + new 4 bytes
    }

    [Fact]
    public void GetData_WithReadBuffer_ReturnsAllBytes()
    {
        // Arrange
        var sourceData = new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55 };
        using var buffer = new ByteBuffer(sourceData);

        // Read some data first
        buffer.ReadUInt8();
        buffer.ReadUInt8();

        // Act
        var result = buffer.GetData();

        // Assert - should return ALL data, not just unread portion
        Assert.Equal(sourceData, result);
    }

    [Fact]
    public void GetData_CalledMultipleTimes_ReturnsSameData()
    {
        // Arrange
        using var buffer = new ByteBuffer();
        buffer.WriteUInt32(0xCAFEBABE);

        // Act
        var result1 = buffer.GetData();
        var result2 = buffer.GetData();

        // Assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void GetData_WithMixedTypes_ReturnsCorrectBytes()
    {
        // Arrange
        using var buffer = new ByteBuffer();
        buffer.WriteUInt8(0xFF);
        buffer.WriteUInt16(0x1234);
        buffer.WriteUInt32(0xDEADBEEF);
        buffer.WriteFloat(1.5f);

        // Act
        var result = buffer.GetData();

        // Assert
        Assert.Equal(11, result.Length); // 1 + 2 + 4 + 4
    }

    [Theory]
    [InlineData(16)]
    [InlineData(256)]
    [InlineData(1024)]
    [InlineData(4096)]
    [InlineData(65536)]
    public void GetData_VariousSizes_ReturnsCorrectLength(int size)
    {
        // Arrange
        using var buffer = new ByteBuffer();
        var testData = new byte[size];
        new Random(42).NextBytes(testData);
        buffer.WriteBytes(testData);

        // Act
        var result = buffer.GetData();

        // Assert
        Assert.Equal(size, result.Length);
        Assert.Equal(testData, result);
    }
}

/// <summary>
/// Coverage for <see cref="ByteBuffer.ReadBits{T}(int)"/> — locks the post-fix
/// behavior. The previous formulation used an <c>int</c> accumulator and
/// <see cref="Convert.ChangeType(object, Type)"/>, which threw
/// <see cref="OverflowException"/> at <c>bitCount=32</c> with the high bit set
/// (the negative-int → uint conversion path). The fix switches to a
/// <see cref="uint"/> accumulator and a
/// <see cref="System.Runtime.CompilerServices.Unsafe.As{TFrom, TTo}(ref TFrom)"/>
/// reinterpret. These tests exercise the previously broken cases at the bit
/// boundaries plus a sampling of small-T cases to confirm the reinterpret
/// truncates correctly.
/// </summary>
public class ByteBufferReadBitsTests
{
    private static byte[] MsbFirst32(uint value)
    {
        // ReadBits reads MSB-first per byte, so for a 32-bit value we want
        // the most significant byte at offset 0 — i.e., big-endian encoding.
        var data = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(data, value);
        return data;
    }

    [Theory]
    [InlineData(0x80000000u)] // the bit that triggered the original OverflowException
    [InlineData(0xFFFFFFFFu)]
    [InlineData(0xCAFEBABEu)]
    [InlineData(0xAAAAAAAAu)]
    [InlineData(0x55555555u)]
    [InlineData(0u)]
    public void ReadBits_uint_32bit(uint expected)
    {
        using var buffer = new ByteBuffer(MsbFirst32(expected));
        Assert.Equal(expected, buffer.ReadBits<uint>(32));
    }

    [Fact]
    public void ReadBits_int_32bit_AllOnes_IsMinusOne()
    {
        using var buffer = new ByteBuffer(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
        Assert.Equal(-1, buffer.ReadBits<int>(32));
    }

    [Fact]
    public void ReadBits_int_32bit_HighBitOnly_IsIntMinValue()
    {
        using var buffer = new ByteBuffer(new byte[] { 0x80, 0x00, 0x00, 0x00 });
        Assert.Equal(int.MinValue, buffer.ReadBits<int>(32));
    }

    [Theory]
    [InlineData(0u, 8)]
    [InlineData(0xFFu, 8)]
    [InlineData(0xAAu, 8)]
    [InlineData(0x7u, 3)]
    [InlineData(0x1Fu, 5)]
    public void ReadBits_byte_RoundTrip(uint value, int bitCount)
    {
        // Pad the 8-bit value into the high bits of a single byte for MSB-first reading.
        byte b = (byte)(value << (8 - bitCount));
        using var buffer = new ByteBuffer(new byte[] { b });
        Assert.Equal((byte)value, buffer.ReadBits<byte>(bitCount));
    }

    [Theory]
    [InlineData(0u, 16)]
    [InlineData(0xFFFFu, 16)]
    [InlineData(0xCAFEu, 16)]
    [InlineData(0x7FFu, 11)]    // ItemLevel — actual production caller
    public void ReadBits_ushort_RoundTrip(uint value, int bitCount)
    {
        var data = new byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(data, (ushort)(value << (16 - bitCount)));
        using var buffer = new ByteBuffer(data);
        Assert.Equal((ushort)value, buffer.ReadBits<ushort>(bitCount));
    }

    [Fact]
    public void ReadBits_uint_30bit_AllOnes_StillCorrect()
    {
        // MovementInfo.Flags is a real 30-bit production read. Locks that the
        // fix doesn't regress it.
        const uint expected = (1u << 30) - 1u;
        var data = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(data, expected << 2);
        using var buffer = new ByteBuffer(data);
        Assert.Equal(expected, buffer.ReadBits<uint>(30));
    }

    [Fact]
    public void ReadBits_Zero_BitCount_ReturnsZero()
    {
        using var buffer = new ByteBuffer(new byte[] { 0xFF });
        Assert.Equal(0u, buffer.ReadBits<uint>(0));
    }
}

/// <summary>
/// Round-trip coverage for ByteBuffer.WriteBits across bit widths, value patterns,
/// and starting bit positions. Locks the wire format byte-identical to the
/// SpanPacketWriter chunked-packing path (commit 27d7e52). Pattern: write via
/// write-mode ByteBuffer → FlushBits → GetData → wrap in read-mode ByteBuffer →
/// <see cref="ByteBuffer.ReadBits{T}(int)"/>.
/// </summary>
public class ByteBufferWriteBitsRoundTripTests
{
    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    [InlineData(5)] [InlineData(6)] [InlineData(7)] [InlineData(8)]
    [InlineData(9)] [InlineData(12)] [InlineData(16)] [InlineData(24)]
    [InlineData(32)]
    public void WriteBits_RoundTrip_Zero(int bitCount)
    {
        using var writer = new ByteBuffer();
        writer.WriteBits(0u, bitCount);
        writer.FlushBits();
        var data = writer.GetData();

        using var reader = new ByteBuffer(data);
        uint actual = reader.ReadBits<uint>(bitCount);
        Assert.Equal(0u, actual);
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    [InlineData(5)] [InlineData(6)] [InlineData(7)] [InlineData(8)]
    [InlineData(9)] [InlineData(12)] [InlineData(16)] [InlineData(24)]
    [InlineData(31)] [InlineData(32)]
    public void WriteBits_RoundTrip_AllOnes(int bitCount)
    {
        uint expected = bitCount == 32 ? uint.MaxValue : (1u << bitCount) - 1u;

        using var writer = new ByteBuffer();
        writer.WriteBits(expected, bitCount);
        writer.FlushBits();
        var data = writer.GetData();

        using var reader = new ByteBuffer(data);
        uint actual = reader.ReadBits<uint>(bitCount);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0, 1)] [InlineData(1, 1)] [InlineData(3, 1)] [InlineData(5, 1)] [InlineData(7, 1)]
    [InlineData(0, 4)] [InlineData(1, 4)] [InlineData(3, 4)] [InlineData(5, 4)] [InlineData(7, 4)]
    [InlineData(0, 8)] [InlineData(1, 8)] [InlineData(3, 8)] [InlineData(5, 8)] [InlineData(7, 8)]
    [InlineData(0, 9)] [InlineData(1, 9)] [InlineData(3, 9)] [InlineData(5, 9)] [InlineData(7, 9)]
    [InlineData(0, 16)] [InlineData(1, 16)] [InlineData(3, 16)] [InlineData(5, 16)] [InlineData(7, 16)]
    [InlineData(0, 24)] [InlineData(1, 24)] [InlineData(3, 24)] [InlineData(5, 24)] [InlineData(7, 24)]
    [InlineData(0, 32)] [InlineData(1, 32)] [InlineData(3, 32)] [InlineData(5, 32)] [InlineData(7, 32)]
    public void WriteBits_RoundTrip_StartingPositions(int paddingBits, int bitCount)
    {
        uint mask = bitCount == 32 ? uint.MaxValue : (1u << bitCount) - 1u;
        uint value = 0xCAFEBABEu & mask;

        using var writer = new ByteBuffer();
        if (paddingBits > 0)
            writer.WriteBits(0u, paddingBits);
        writer.WriteBits(value, bitCount);
        writer.FlushBits();
        var data = writer.GetData();

        using var reader = new ByteBuffer(data);
        if (paddingBits > 0)
            reader.ReadBits<uint>(paddingBits);
        uint actual = reader.ReadBits<uint>(bitCount);
        Assert.Equal(value, actual);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(7)] [InlineData(8)] [InlineData(9)] [InlineData(15)]
    [InlineData(16)] [InlineData(23)] [InlineData(24)] [InlineData(31)]
    public void WriteBits_RoundTrip_SingleBitSet(int bitPos)
    {
        uint value = 1u << bitPos;

        using var writer = new ByteBuffer();
        writer.WriteBits(value, 32);
        writer.FlushBits();
        var data = writer.GetData();

        using var reader = new ByteBuffer(data);
        uint actual = reader.ReadBits<uint>(32);
        Assert.Equal(value, actual);
    }

    [Theory]
    [InlineData(0xAAAAAAAAu, 32)]
    [InlineData(0x55555555u, 32)]
    [InlineData(0xAAAAu, 16)]
    [InlineData(0x5555u, 16)]
    [InlineData(0xAAu, 8)]
    [InlineData(0x55u, 8)]
    public void WriteBits_RoundTrip_AlternatingPattern(uint value, int bitCount)
    {
        using var writer = new ByteBuffer();
        writer.WriteBits(value, bitCount);
        writer.FlushBits();
        var data = writer.GetData();

        using var reader = new ByteBuffer(data);
        uint actual = reader.ReadBits<uint>(bitCount);
        Assert.Equal(value, actual);
    }

    [Fact]
    public void WriteBits_InterleavedWithBytesAndFloats()
    {
        using var writer = new ByteBuffer();
        writer.WriteBits(0xAu, 4);          // byte0 hi nibble
        writer.WriteBits(0x5u, 4);          // byte0 lo nibble — flushes byte0 = 0xA5
        writer.WriteUInt8(0x42);            // byte1 = 0x42
        writer.WriteBits(0x7u, 3);          // byte2 hi 3 bits = 0b111
        writer.WriteFloat(1.5f);            // FlushBits emits byte2, then 4 float bytes
        writer.WriteBits(0x123456u, 24);    // 3 byte-aligned bytes
        writer.FlushBits();
        var data = writer.GetData();

        using var reader = new ByteBuffer(data);
        Assert.Equal(0xAu, reader.ReadBits<uint>(4));
        Assert.Equal(0x5u, reader.ReadBits<uint>(4));
        Assert.Equal((byte)0x42, reader.ReadUInt8());
        Assert.Equal(0x7u, reader.ReadBits<uint>(3));
        Assert.Equal(1.5f, reader.ReadFloat());
        Assert.Equal(0x123456u, reader.ReadBits<uint>(24));
    }

    private enum ByteEnum : byte { A = 0, B = 5, Max = 0xFF }
    private enum ShortEnum : short { A = 0, B = 0x1234, Max = 0x7FFF }
    private enum UShortEnum : ushort { A = 0, B = 0x1234, Max = 0xFFFF }
    private enum IntEnum { A = 0, B = 0x12345678, Max = int.MaxValue }
    private enum UIntEnum : uint { A = 0, B = 0x80000001u, Max = uint.MaxValue }
    private enum LongEnum : long { A = 0, B = 0x1_2345_6789L, Max = long.MaxValue }
    private enum ULongEnum : ulong { A = 0, B = 0x1_2345_6789UL, Max = ulong.MaxValue }

    [Fact]
    public void WriteBits_GenericEnum_AllUnderlyingSizes_RoundTrip()
    {
        using var writer = new ByteBuffer();
        writer.WriteBits(ByteEnum.B, 8);
        writer.WriteBits(ShortEnum.B, 16);
        writer.WriteBits(UShortEnum.B, 16);
        writer.WriteBits(IntEnum.B, 32);
        writer.WriteBits(UIntEnum.B, 32);
        writer.WriteBits((LongEnum)0x1_2345_6789L, 32);   // truncates to low 32 bits
        writer.WriteBits((ULongEnum)0x1_2345_6789UL, 32); // truncates to low 32 bits
        writer.FlushBits();
        var data = writer.GetData();

        using var reader = new ByteBuffer(data);
        Assert.Equal((byte)ByteEnum.B,            (byte)reader.ReadBits<uint>(8));
        Assert.Equal((ushort)ShortEnum.B,         (ushort)reader.ReadBits<uint>(16));
        Assert.Equal((ushort)UShortEnum.B,        (ushort)reader.ReadBits<uint>(16));
        Assert.Equal((uint)IntEnum.B,             reader.ReadBits<uint>(32));
        Assert.Equal((uint)UIntEnum.B,            reader.ReadBits<uint>(32));
        Assert.Equal(0x2345_6789u,                reader.ReadBits<uint>(32));
        Assert.Equal(0x2345_6789u,                reader.ReadBits<uint>(32));
    }

    [Fact]
    public void WriteBits_GenericEnum_MatchesExplicitUintCast()
    {
        // The new generic overload must produce bit-identical output to the
        // existing WriteBits(uint, int) path that callers used to invoke via
        // an explicit (uint) cast.
        using var generic = new ByteBuffer();
        generic.WriteBits(IntEnum.B, 32);
        generic.WriteBits(ByteEnum.B, 8);
        generic.WriteBits(UShortEnum.B, 16);
        generic.FlushBits();

        using var explicitCast = new ByteBuffer();
        explicitCast.WriteBits((uint)IntEnum.B, 32);
        explicitCast.WriteBits((uint)ByteEnum.B, 8);
        explicitCast.WriteBits((uint)UShortEnum.B, 16);
        explicitCast.FlushBits();

        Assert.Equal(explicitCast.GetData(), generic.GetData());
    }
}

/// <summary>
/// Round-trip coverage for ByteBuffer's write primitives that the upcoming
/// optimization commit will touch: WriteBool, WriteVector{2,3,4}, WritePackXYZ.
/// These freeze the wire format so the LE-blit / branchless changes don't drift.
/// </summary>
public class ByteBufferWritePrimitiveRoundTripTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WriteBool_RoundTrip(bool value)
    {
        using var writer = new ByteBuffer();
        writer.WriteBool(value);
        var data = writer.GetData();

        Assert.Single(data);
        Assert.Equal(value ? (byte)1 : (byte)0, data[0]);

        using var reader = new ByteBuffer(data);
        Assert.Equal(value, reader.ReadBool());
    }

    [Fact]
    public void WriteVector2_RoundTrip()
    {
        var value = new Vector2(1.5f, -2.25f);

        using var writer = new ByteBuffer();
        writer.WriteVector2(value);
        var data = writer.GetData();

        Assert.Equal(8, data.Length);

        using var reader = new ByteBuffer(data);
        var actual = reader.ReadVector2();
        Assert.Equal(value.X, actual.X);
        Assert.Equal(value.Y, actual.Y);
    }

    [Fact]
    public void WriteVector3_RoundTrip()
    {
        var value = new Vector3(1.5f, -2.25f, 0.125f);

        using var writer = new ByteBuffer();
        writer.WriteVector3(value);
        var data = writer.GetData();

        Assert.Equal(12, data.Length);

        using var reader = new ByteBuffer(data);
        var actual = reader.ReadVector3();
        Assert.Equal(value.X, actual.X);
        Assert.Equal(value.Y, actual.Y);
        Assert.Equal(value.Z, actual.Z);
    }

    [Fact]
    public void WriteVector4_RoundTrip()
    {
        var value = new Vector4(1.5f, -2.25f, 0.125f, 1024.0f);

        using var writer = new ByteBuffer();
        writer.WriteVector4(value);
        var data = writer.GetData();

        Assert.Equal(16, data.Length);

        using var reader = new ByteBuffer(data);
        // ByteBuffer doesn't expose ReadVector4 by name in its API; re-read via floats.
        Assert.Equal(value.X, reader.ReadFloat());
        Assert.Equal(value.Y, reader.ReadFloat());
        Assert.Equal(value.Z, reader.ReadFloat());
        Assert.Equal(value.W, reader.ReadFloat());
    }

    [Theory]
    [InlineData(0.0f, 0.0f, 0.0f)]
    [InlineData(1.5f, -2.25f, 0.125f)]
    [InlineData(-100.5f, 100.5f, -50.0f)] // negative coords — two's-complement preservation
    [InlineData(255.75f, -255.75f, 127.5f)]
    public void WritePackXYZ_RoundTrip(float x, float y, float z)
    {
        var value = new Vector3(x, y, z);

        using var writer = new ByteBuffer();
        writer.WritePackXYZ(value);
        var data = writer.GetData();

        Assert.Equal(4, data.Length);

        using var reader = new ByteBuffer(data);
        var actual = reader.ReadPackedVector3();

        // 0.25 = quantization step (X/Y 11 bits, Z 10 bits)
        Assert.True(Math.Abs(actual.X - value.X) < 0.25f, $"X off by {actual.X - value.X}");
        Assert.True(Math.Abs(actual.Y - value.Y) < 0.25f, $"Y off by {actual.Y - value.Y}");
        Assert.True(Math.Abs(actual.Z - value.Z) < 0.25f, $"Z off by {actual.Z - value.Z}");
    }
}

/// <summary>
/// Round-trip coverage for WriteCString / WriteString. Locks current null-handling
/// behavior before the upcoming string? signature change in the perf commit.
/// </summary>
public class ByteBufferWriteStringRoundTripTests
{
    [Fact]
    public void WriteCString_Empty_WritesSingleNul()
    {
        using var writer = new ByteBuffer();
        writer.WriteCString(string.Empty);
        var data = writer.GetData();

        Assert.Equal(new byte[] { 0x00 }, data);

        using var reader = new ByteBuffer(data);
        Assert.Equal(string.Empty, reader.ReadCString());
    }

    [Fact]
    public void WriteCString_Null_WritesSingleNul()
    {
        using var writer = new ByteBuffer();
        writer.WriteCString(null!);
        var data = writer.GetData();

        Assert.Equal(new byte[] { 0x00 }, data);

        using var reader = new ByteBuffer(data);
        Assert.Equal(string.Empty, reader.ReadCString());
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("Hello, World!")]
    [InlineData("héllo wörld 你好")]
    [InlineData("Test 🎮 游戏")]
    public void WriteCString_RoundTrip(string value)
    {
        using var writer = new ByteBuffer();
        writer.WriteCString(value);
        var data = writer.GetData();

        int byteCount = Encoding.UTF8.GetByteCount(value);
        Assert.Equal(byteCount + 1, data.Length);
        Assert.Equal(0, data[^1]);

        using var reader = new ByteBuffer(data);
        Assert.Equal(value, reader.ReadCString());
    }

    [Fact]
    public void WriteCString_Long_ForcesBufferGrow_RoundTrip()
    {
        // 1024 chars > DefaultWriteCapacity (256) — exercises EnsureCapacity grow path.
        var value = new string('A', 1024);

        using var writer = new ByteBuffer();
        writer.WriteCString(value);
        var data = writer.GetData();

        Assert.Equal(1025, data.Length); // 1024 + NUL

        using var reader = new ByteBuffer(data);
        Assert.Equal(value, reader.ReadCString());
    }

    [Fact]
    public void WriteString_Empty_WritesNothing()
    {
        using var writer = new ByteBuffer();
        writer.WriteString(string.Empty);
        var data = writer.GetData();

        Assert.Empty(data);
    }

    [Fact]
    public void WriteString_Null_WritesNothing()
    {
        using var writer = new ByteBuffer();
        writer.WriteString(null!);
        var data = writer.GetData();

        Assert.Empty(data);
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("héllo wörld 你好")]
    [InlineData("Test 🎮 游戏")]
    public void WriteString_RoundTrip(string value)
    {
        using var writer = new ByteBuffer();
        writer.WriteString(value);
        var data = writer.GetData();

        int byteCount = Encoding.UTF8.GetByteCount(value);
        Assert.Equal(byteCount, data.Length);

        using var reader = new ByteBuffer(data);
        Assert.Equal(value, reader.ReadString((uint)byteCount));
    }

    [Fact]
    public void WriteString_Long_ForcesBufferGrow_RoundTrip()
    {
        var value = new string('B', 1024);

        using var writer = new ByteBuffer();
        writer.WriteString(value);
        var data = writer.GetData();

        Assert.Equal(1024, data.Length);

        using var reader = new ByteBuffer(data);
        Assert.Equal(value, reader.ReadString(1024));
    }
}

/// <summary>
/// Cross-mode layout-equivalence tests proving that ByteBuffer and SpanPacket{Reader,Writer}
/// produce/consume the same wire bytes for the surfaces touched by the perf commit.
/// Small, targeted set — exhaustive coverage lives in the same-implementation tests above.
/// </summary>
public class ByteBufferCrossModeRoundTripTests
{
    [Fact]
    public void CrossMode_WriteBits_ByteBuffer_ReadVia_SpanPacketReader()
    {
        using var writer = new ByteBuffer();
        writer.WriteBits(0x123u, 9);
        writer.WriteBits(0xFFu, 8);
        writer.FlushBits();
        var data = writer.GetData();

        var reader = new SpanPacketReader(data);
        Assert.Equal(0x123u, reader.ReadBits<uint>(9));
        Assert.Equal(0xFFu, reader.ReadBits<uint>(8));
    }

    [Fact]
    public void CrossMode_WriteBits_SpanPacketWriter_ReadVia_ByteBuffer()
    {
        Span<byte> span = stackalloc byte[8];
        var spanWriter = new SpanPacketWriter(span);
        spanWriter.WriteBits(0x123u, 9);
        spanWriter.WriteBits(0xFFu, 8);
        spanWriter.FlushBits();
        var data = spanWriter.ToArray();

        using var reader = new ByteBuffer(data);
        Assert.Equal(0x123u, reader.ReadBits<uint>(9));
        Assert.Equal(0xFFu, reader.ReadBits<uint>(8));
    }

    [Fact]
    public void CrossMode_WriteVector3_ByteBuffer_ReadVia_SpanPacketReader()
    {
        var value = new Vector3(100.5f, -200.25f, 300.125f);

        using var writer = new ByteBuffer();
        writer.WriteVector3(value);
        var data = writer.GetData();

        var reader = new SpanPacketReader(data);
        var actual = reader.ReadVector3();
        Assert.Equal(value.X, actual.X);
        Assert.Equal(value.Y, actual.Y);
        Assert.Equal(value.Z, actual.Z);
    }

    [Fact]
    public void CrossMode_WriteVector3_SpanPacketWriter_ReadVia_ByteBuffer()
    {
        var value = new Vector3(100.5f, -200.25f, 300.125f);

        Span<byte> span = stackalloc byte[12];
        var spanWriter = new SpanPacketWriter(span);
        spanWriter.WriteVector3(value);
        var data = spanWriter.ToArray();

        using var reader = new ByteBuffer(data);
        var actual = reader.ReadVector3();
        Assert.Equal(value.X, actual.X);
        Assert.Equal(value.Y, actual.Y);
        Assert.Equal(value.Z, actual.Z);
    }

    [Fact]
    public void CrossMode_WriteCString_ByteBuffer_ReadVia_SpanPacketReader()
    {
        const string value = "héllo wörld 你好";

        using var writer = new ByteBuffer();
        writer.WriteCString(value);
        var data = writer.GetData();

        var reader = new SpanPacketReader(data);
        Assert.Equal(value, reader.ReadCString());
    }

    [Fact]
    public void CrossMode_WriteCString_SpanPacketWriter_ReadVia_ByteBuffer()
    {
        const string value = "héllo wörld 你好";

        Span<byte> span = stackalloc byte[64];
        var spanWriter = new SpanPacketWriter(span);
        spanWriter.WriteCString(value);
        var data = spanWriter.ToArray();

        using var reader = new ByteBuffer(data);
        Assert.Equal(value, reader.ReadCString());
    }
}
