using System;
using System.Buffers.Binary;
using HermesProxy.World;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// Wire cover for <see cref="PacketHeader"/>, which became a struct with an
/// <c>[InlineArray]</c> tag. The bytes on the wire — 4-byte little-endian size followed by
/// the 12 AES-GCM tag bytes — must be byte-identical to the previous class implementation,
/// and the tag storage must stay writable in place so <c>WorldCrypt.Encrypt</c> can fill it.
/// </summary>
public class PacketHeaderTests
{
    private static byte[] BuildWireHeader(int size, byte tagSeed)
    {
        var bytes = new byte[PacketHeader.StructSize];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, size);
        for (int i = 0; i < PacketHeader.TagSize; i++)
            bytes[sizeof(int) + i] = (byte)(tagSeed + i);
        return bytes;
    }

    [Fact]
    public void StructSize_IsSizePlusTag()
    {
        Assert.Equal(12, PacketHeader.TagSize);
        Assert.Equal(16, PacketHeader.StructSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(512)]
    [InlineData(0x3FFFF)]
    public void ReadThenWrite_RoundTripsExactBytes(int size)
    {
        var wire = BuildWireHeader(size, 0xA0);

        var header = new PacketHeader();
        header.Read(wire);

        Assert.Equal(size, header.Size);
        for (int i = 0; i < PacketHeader.TagSize; i++)
            Assert.Equal(wire[sizeof(int) + i], header.Tag[i]);

        var written = new byte[PacketHeader.StructSize];
        header.Write(written);

        Assert.Equal(wire, written);
    }

    [Fact]
    public void Read_IgnoresBytesPastTheHeader()
    {
        var wire = new byte[PacketHeader.StructSize + 8];
        BuildWireHeader(64, 0x10).CopyTo(wire, 0);
        wire[^1] = 0xFF;

        var header = new PacketHeader();
        header.Read(wire);

        Assert.Equal(64, header.Size);
        Assert.Equal(0x10 + PacketHeader.TagSize - 1, header.Tag[PacketHeader.TagSize - 1]);
    }

    [Fact]
    public void Write_LeavesTrailingDestinationBytesAlone()
    {
        var header = new PacketHeader { Size = 128 };
        var destination = new byte[PacketHeader.StructSize + 4];
        Array.Fill(destination, (byte)0xCC);

        header.Write(destination);

        Assert.Equal(128, BinaryPrimitives.ReadInt32LittleEndian(destination));
        for (int i = 0; i < PacketHeader.TagSize; i++)
            Assert.Equal(0, destination[sizeof(int) + i]);
        for (int i = PacketHeader.StructSize; i < destination.Length; i++)
            Assert.Equal(0xCC, destination[i]);
    }

    /// <summary>
    /// The tag must be addressable storage inside the header, not a copy: production code
    /// hands <c>header.Tag</c> to <c>WorldCrypt.Encrypt</c> and expects the written tag to
    /// come back out through <see cref="PacketHeader.Write"/>.
    /// </summary>
    [Fact]
    public void Tag_IsWrittenInPlaceThroughASpan()
    {
        var header = new PacketHeader { Size = 32 };

        Span<byte> tag = header.Tag;
        for (int i = 0; i < PacketHeader.TagSize; i++)
            tag[i] = (byte)(0x50 + i);

        var written = new byte[PacketHeader.StructSize];
        header.Write(written);

        for (int i = 0; i < PacketHeader.TagSize; i++)
            Assert.Equal(0x50 + i, written[sizeof(int) + i]);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(0x3FFFF, true)]
    [InlineData(0x40000, false)]
    [InlineData(int.MaxValue, false)]
    [InlineData(-1, false)]        // used to pass: `Size < 0x40000` accepted negatives
    [InlineData(int.MinValue, false)]
    public void IsValidSize_RejectsOversizedAndNegative(int size, bool expected)
    {
        var header = new PacketHeader { Size = size };
        Assert.Equal(expected, header.IsValidSize());
    }
}
