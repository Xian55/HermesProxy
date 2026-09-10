using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using Bgs.Protocol;
using BNetServer.Networking;
using Google.Protobuf;
using Xunit;

namespace HermesProxy.Tests.BnetServer;

public class BnetPacketParserTests
{
    /// <summary>
    /// Creates a valid Bnet packet with the given payload size.
    /// </summary>
    private static byte[] CreateValidPacket(uint serviceHash, uint methodId, uint token, int payloadSize)
    {
        var header = new Header
        {
            ServiceId = 1,
            ServiceHash = serviceHash,
            MethodId = methodId,
            Token = token,
            Size = (uint)payloadSize
        };

        var headerBytes = header.ToByteArray();
        var headerLength = (ushort)headerBytes.Length;

        var packet = new byte[2 + headerLength + payloadSize];

        // Write header length (big-endian)
        BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(0, 2), headerLength);

        // Write header
        Array.Copy(headerBytes, 0, packet, 2, headerLength);

        // Fill payload with test pattern
        for (int i = 0; i < payloadSize; i++)
        {
            packet[2 + headerLength + i] = (byte)(i & 0xFF);
        }

        return packet;
    }

    [Fact]
    public void ParseFromListOriginal_WithEmptyBuffer_ReturnsIncomplete()
    {
        // Arrange
        var buffer = new List<byte>();

        // Act
        var result = BnetPacketParser.ParseFromListOriginal(buffer);

        // Assert
        Assert.False(result.Success);
    }

    [Fact]
    public void ParseFromListOriginal_WithOnlyOneByteBuffer_ReturnsIncomplete()
    {
        // Arrange
        var buffer = new List<byte> { 0x00 };

        // Act
        var result = BnetPacketParser.ParseFromListOriginal(buffer);

        // Assert
        Assert.False(result.Success);
    }

    [Fact]
    public void ParseFromListOriginal_WithTwoBytesBuffer_ReturnsIncomplete()
    {
        // Arrange
        var buffer = new List<byte> { 0x00, 0x10 }; // Header length = 16, but no header data

        // Act
        var result = BnetPacketParser.ParseFromListOriginal(buffer);

        // Assert
        Assert.False(result.Success);
    }

    [Fact]
    public void ParseFromListOriginal_WithValidCompletePacket_ReturnsSuccess()
    {
        // Arrange
        var packet = CreateValidPacket(serviceHash: 0x12345678, methodId: 1, token: 100, payloadSize: 16);
        var buffer = new List<byte>(packet);

        // Act
        var result = BnetPacketParser.ParseFromListOriginal(buffer);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Header);
        Assert.Equal(0x12345678u, result.Header!.ServiceHash);
        Assert.Equal(1u, result.Header.MethodId);
        Assert.Equal(100u, result.Header.Token);
        Assert.Equal(16, result.Payload!.Length);
        Assert.Equal(packet.Length, result.TotalLength);
    }

    [Fact]
    public void ParseFromListOriginal_WithIncompletePayload_ReturnsIncomplete()
    {
        // Arrange
        var fullPacket = CreateValidPacket(serviceHash: 0x12345678, methodId: 1, token: 100, payloadSize: 100);
        // Only include part of the payload
        var partialPacket = new byte[fullPacket.Length - 50];
        Array.Copy(fullPacket, partialPacket, partialPacket.Length);
        var buffer = new List<byte>(partialPacket);

        // Act
        var result = BnetPacketParser.ParseFromListOriginal(buffer);

        // Assert
        Assert.False(result.Success);
    }

    [Fact]
    public void ParseFromListOptimized_WithEmptyBuffer_ReturnsIncomplete()
    {
        // Arrange
        var buffer = new List<byte>();

        // Act
        var result = BnetPacketParser.ParseFromListOptimized(buffer);

        // Assert
        Assert.False(result.Success);
    }

    [Fact]
    public void ParseFromListOptimized_WithValidCompletePacket_ReturnsSuccess()
    {
        // Arrange
        var packet = CreateValidPacket(serviceHash: 0x12345678, methodId: 1, token: 100, payloadSize: 16);
        var buffer = new List<byte>(packet);

        // Act
        var result = BnetPacketParser.ParseFromListOptimized(buffer);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Header);
        Assert.Equal(0x12345678u, result.Header!.ServiceHash);
        Assert.Equal(1u, result.Header.MethodId);
        Assert.Equal(100u, result.Header.Token);
        Assert.Equal(16, result.Payload!.Length);
        Assert.Equal(packet.Length, result.TotalLength);
    }

    [Fact]
    public void BothImplementations_ProduceSameResults()
    {
        // Arrange
        var packet = CreateValidPacket(serviceHash: 0xABCDEF01, methodId: 42, token: 999, payloadSize: 64);
        var buffer1 = new List<byte>(packet);
        var buffer2 = new List<byte>(packet);

        // Act
        var resultOriginal = BnetPacketParser.ParseFromListOriginal(buffer1);
        var resultOptimized = BnetPacketParser.ParseFromListOptimized(buffer2);

        // Assert
        Assert.Equal(resultOriginal.Success, resultOptimized.Success);
        Assert.Equal(resultOriginal.TotalLength, resultOptimized.TotalLength);
        Assert.Equal(resultOriginal.HeaderLength, resultOptimized.HeaderLength);
        Assert.Equal(resultOriginal.Header?.ServiceHash, resultOptimized.Header?.ServiceHash);
        Assert.Equal(resultOriginal.Header?.MethodId, resultOptimized.Header?.MethodId);
        Assert.Equal(resultOriginal.Header?.Token, resultOptimized.Header?.Token);
        Assert.Equal(resultOriginal.Payload, resultOptimized.Payload);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(16)]
    [InlineData(64)]
    [InlineData(256)]
    [InlineData(1024)]
    public void BothImplementations_HandleVariousPayloadSizes(int payloadSize)
    {
        // Arrange
        var packet = CreateValidPacket(serviceHash: 0x11111111, methodId: 1, token: 1, payloadSize: payloadSize);
        var buffer1 = new List<byte>(packet);
        var buffer2 = new List<byte>(packet);

        // Act
        var resultOriginal = BnetPacketParser.ParseFromListOriginal(buffer1);
        var resultOptimized = BnetPacketParser.ParseFromListOptimized(buffer2);

        // Assert
        Assert.True(resultOriginal.Success);
        Assert.True(resultOptimized.Success);
        Assert.Equal(payloadSize, resultOriginal.Payload!.Length);
        Assert.Equal(payloadSize, resultOptimized.Payload!.Length);
        Assert.Equal(resultOriginal.Payload, resultOptimized.Payload);
    }

    [Fact]
    public void ParseFromListOriginal_WithMultiplePackets_ParsesFirstOnly()
    {
        // Arrange
        var packet1 = CreateValidPacket(serviceHash: 0x11111111, methodId: 1, token: 1, payloadSize: 8);
        var packet2 = CreateValidPacket(serviceHash: 0x22222222, methodId: 2, token: 2, payloadSize: 16);
        var buffer = new List<byte>(packet1.Length + packet2.Length);
        buffer.AddRange(packet1);
        buffer.AddRange(packet2);

        // Act
        var result = BnetPacketParser.ParseFromListOriginal(buffer);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0x11111111u, result.Header!.ServiceHash);
        Assert.Equal(packet1.Length, result.TotalLength);
    }

    [Fact]
    public void ParseFromListOptimized_WithMultiplePackets_ParsesFirstOnly()
    {
        // Arrange
        var packet1 = CreateValidPacket(serviceHash: 0x11111111, methodId: 1, token: 1, payloadSize: 8);
        var packet2 = CreateValidPacket(serviceHash: 0x22222222, methodId: 2, token: 2, payloadSize: 16);
        var buffer = new List<byte>(packet1.Length + packet2.Length);
        buffer.AddRange(packet1);
        buffer.AddRange(packet2);

        // Act
        var result = BnetPacketParser.ParseFromListOptimized(buffer);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0x11111111u, result.Header!.ServiceHash);
        Assert.Equal(packet1.Length, result.TotalLength);
    }

    [Fact]
    public void PayloadContainsCorrectData()
    {
        // Arrange
        var packet = CreateValidPacket(serviceHash: 0x12345678, methodId: 1, token: 1, payloadSize: 10);
        var buffer = new List<byte>(packet);

        // Act
        var result = BnetPacketParser.ParseFromListOriginal(buffer);

        // Assert
        Assert.True(result.Success);
        // Verify payload contains test pattern (0, 1, 2, 3, ...)
        for (int i = 0; i < result.Payload!.Length; i++)
        {
            Assert.Equal((byte)(i & 0xFF), result.Payload[i]);
        }
    }

    // ========== ArrayPool (Pooled) Implementation Tests ==========

    [Fact]
    public void ParseFromListPooled_WithEmptyBuffer_ReturnsIncomplete()
    {
        // Arrange
        var buffer = new List<byte>();

        // Act
        var result = BnetPacketParser.ParseFromListPooled(buffer);

        // Assert
        Assert.False(result.Success);
        result.ReturnPayload(); // Safe to call even on incomplete
    }

    [Fact]
    public void ParseFromListPooled_WithOnlyOneByteBuffer_ReturnsIncomplete()
    {
        // Arrange
        var buffer = new List<byte> { 0x00 };

        // Act
        var result = BnetPacketParser.ParseFromListPooled(buffer);

        // Assert
        Assert.False(result.Success);
        result.ReturnPayload();
    }

    [Fact]
    public void ParseFromListPooled_WithValidCompletePacket_ReturnsSuccess()
    {
        // Arrange
        var packet = CreateValidPacket(serviceHash: 0x12345678, methodId: 1, token: 100, payloadSize: 16);
        var buffer = new List<byte>(packet);

        // Act
        var result = BnetPacketParser.ParseFromListPooled(buffer);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Header);
        Assert.Equal(0x12345678u, result.Header!.ServiceHash);
        Assert.Equal(1u, result.Header.MethodId);
        Assert.Equal(100u, result.Header.Token);
        Assert.Equal(16, result.PayloadLength);
        Assert.Equal(packet.Length, result.TotalLength);

        // Verify payload data is correct
        var payloadSpan = result.PayloadSpan;
        for (int i = 0; i < payloadSpan.Length; i++)
        {
            Assert.Equal((byte)(i & 0xFF), payloadSpan[i]);
        }

        result.ReturnPayload();
    }

    [Fact]
    public void ParseFromListPooled_WithIncompletePayload_ReturnsIncomplete()
    {
        // Arrange
        var fullPacket = CreateValidPacket(serviceHash: 0x12345678, methodId: 1, token: 100, payloadSize: 100);
        var partialPacket = new byte[fullPacket.Length - 50];
        Array.Copy(fullPacket, partialPacket, partialPacket.Length);
        var buffer = new List<byte>(partialPacket);

        // Act
        var result = BnetPacketParser.ParseFromListPooled(buffer);

        // Assert
        Assert.False(result.Success);
        result.ReturnPayload();
    }

    [Fact]
    public void AllThreeImplementations_ProduceSameResults()
    {
        // Arrange
        var packet = CreateValidPacket(serviceHash: 0xABCDEF01, methodId: 42, token: 999, payloadSize: 64);
        var buffer1 = new List<byte>(packet);
        var buffer2 = new List<byte>(packet);
        var buffer3 = new List<byte>(packet);

        // Act
        var resultOriginal = BnetPacketParser.ParseFromListOriginal(buffer1);
        var resultOptimized = BnetPacketParser.ParseFromListOptimized(buffer2);
        var resultPooled = BnetPacketParser.ParseFromListPooled(buffer3);

        // Assert
        Assert.Equal(resultOriginal.Success, resultPooled.Success);
        Assert.Equal(resultOriginal.TotalLength, resultPooled.TotalLength);
        Assert.Equal(resultOriginal.HeaderLength, resultPooled.HeaderLength);
        Assert.Equal(resultOriginal.Header?.ServiceHash, resultPooled.Header?.ServiceHash);
        Assert.Equal(resultOriginal.Header?.MethodId, resultPooled.Header?.MethodId);
        Assert.Equal(resultOriginal.Header?.Token, resultPooled.Header?.Token);
        Assert.Equal(resultOriginal.Payload!.Length, resultPooled.PayloadLength);
        Assert.True(resultOriginal.Payload.AsSpan().SequenceEqual(resultPooled.PayloadSpan));

        resultPooled.ReturnPayload();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(16)]
    [InlineData(64)]
    [InlineData(256)]
    [InlineData(1024)]
    [InlineData(4096)]
    public void ParseFromListPooled_HandlesVariousPayloadSizes(int payloadSize)
    {
        // Arrange
        var packet = CreateValidPacket(serviceHash: 0x11111111, methodId: 1, token: 1, payloadSize: payloadSize);
        var bufferOriginal = new List<byte>(packet);
        var bufferPooled = new List<byte>(packet);

        // Act
        var resultOriginal = BnetPacketParser.ParseFromListOriginal(bufferOriginal);
        var resultPooled = BnetPacketParser.ParseFromListPooled(bufferPooled);

        // Assert
        Assert.True(resultOriginal.Success);
        Assert.True(resultPooled.Success);
        Assert.Equal(payloadSize, resultOriginal.Payload!.Length);
        Assert.Equal(payloadSize, resultPooled.PayloadLength);

        if (payloadSize > 0)
        {
            Assert.True(resultOriginal.Payload.AsSpan().SequenceEqual(resultPooled.PayloadSpan));
        }

        resultPooled.ReturnPayload();
    }

    [Fact]
    public void ParseFromListPooled_WithMultiplePackets_ParsesFirstOnly()
    {
        // Arrange
        var packet1 = CreateValidPacket(serviceHash: 0x11111111, methodId: 1, token: 1, payloadSize: 8);
        var packet2 = CreateValidPacket(serviceHash: 0x22222222, methodId: 2, token: 2, payloadSize: 16);
        var buffer = new List<byte>(packet1.Length + packet2.Length);
        buffer.AddRange(packet1);
        buffer.AddRange(packet2);

        // Act
        var result = BnetPacketParser.ParseFromListPooled(buffer);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0x11111111u, result.Header!.ServiceHash);
        Assert.Equal(packet1.Length, result.TotalLength);

        result.ReturnPayload();
    }

    [Fact]
    public void ParseFromListPooled_WithZeroPayload_ReturnsNullArray()
    {
        // Arrange
        var packet = CreateValidPacket(serviceHash: 0x12345678, methodId: 1, token: 1, payloadSize: 0);
        var buffer = new List<byte>(packet);

        // Act
        var result = BnetPacketParser.ParseFromListPooled(buffer);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.PayloadArray);
        Assert.Equal(0, result.PayloadLength);
        Assert.True(result.PayloadSpan.IsEmpty);

        result.ReturnPayload(); // Should be safe even with null array
    }

    // ========== ParseFromSpan (Zero-allocation) Tests ==========

    [Fact]
    public void ParseFromSpan_WithEmptyBuffer_ReturnsIncomplete()
    {
        // Arrange
        ReadOnlySpan<byte> buffer = ReadOnlySpan<byte>.Empty;

        // Act
        var result = BnetPacketParser.ParseFromSpan(buffer);

        // Assert
        Assert.False(result.Success);
        result.ReturnPayload();
    }

    [Fact]
    public void ParseFromSpan_WithValidCompletePacket_ReturnsSuccess()
    {
        // Arrange
        var packet = CreateValidPacket(serviceHash: 0x12345678, methodId: 1, token: 100, payloadSize: 16);

        // Act
        var result = BnetPacketParser.ParseFromSpan(packet);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Header);
        Assert.Equal(0x12345678u, result.Header!.ServiceHash);
        Assert.Equal(1u, result.Header.MethodId);
        Assert.Equal(100u, result.Header.Token);
        Assert.Equal(16, result.PayloadLength);
        Assert.Equal(packet.Length, result.TotalLength);

        // Verify payload data
        var payloadSpan = result.PayloadSpan;
        for (int i = 0; i < payloadSpan.Length; i++)
        {
            Assert.Equal((byte)(i & 0xFF), payloadSpan[i]);
        }

        result.ReturnPayload();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(16)]
    [InlineData(64)]
    [InlineData(256)]
    [InlineData(1024)]
    [InlineData(4096)]
    public void ParseFromSpan_MatchesListPooledResults(int payloadSize)
    {
        // Arrange
        var packet = CreateValidPacket(serviceHash: 0x11111111, methodId: 1, token: 1, payloadSize: payloadSize);
        var listBuffer = new List<byte>(packet);

        // Act
        var resultList = BnetPacketParser.ParseFromListPooled(listBuffer);
        var resultSpan = BnetPacketParser.ParseFromSpan(packet);

        // Assert
        Assert.Equal(resultList.Success, resultSpan.Success);
        Assert.Equal(resultList.TotalLength, resultSpan.TotalLength);
        Assert.Equal(resultList.HeaderLength, resultSpan.HeaderLength);
        Assert.Equal(resultList.Header?.ServiceHash, resultSpan.Header?.ServiceHash);
        Assert.Equal(resultList.PayloadLength, resultSpan.PayloadLength);

        if (payloadSize > 0)
        {
            Assert.True(resultList.PayloadSpan.SequenceEqual(resultSpan.PayloadSpan));
        }

        resultList.ReturnPayload();
        resultSpan.ReturnPayload();
    }

    // ========== PooledByteBuffer Tests ==========

    [Fact]
    public void PooledByteBuffer_InitialState_IsEmpty()
    {
        // Arrange & Act
        using var buffer = new PooledByteBuffer();

        // Assert
        Assert.Equal(0, buffer.Length);
        Assert.True(buffer.Span.IsEmpty);
    }

    [Fact]
    public void PooledByteBuffer_Append_IncreasesLength()
    {
        // Arrange
        using var buffer = new PooledByteBuffer();
        var data = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        buffer.Append(data, data.Length);

        // Assert
        Assert.Equal(5, buffer.Length);
        Assert.True(buffer.Span.SequenceEqual(data));
    }

    [Fact]
    public void PooledByteBuffer_Advance_DecreasesLength()
    {
        // Arrange
        using var buffer = new PooledByteBuffer();
        var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        buffer.Append(data, data.Length);

        // Act
        buffer.Advance(3);

        // Assert
        Assert.Equal(7, buffer.Length);
        Assert.True(buffer.Span.SequenceEqual(new byte[] { 4, 5, 6, 7, 8, 9, 10 }));
    }

    [Fact]
    public void PooledByteBuffer_MultipleAppendAndAdvance_WorksCorrectly()
    {
        // Arrange
        using var buffer = new PooledByteBuffer();

        // Act - simulate packet processing
        buffer.Append(new byte[] { 1, 2, 3 }, 3);
        buffer.Append(new byte[] { 4, 5, 6 }, 3);
        Assert.Equal(6, buffer.Length);

        buffer.Advance(2);
        Assert.Equal(4, buffer.Length);
        Assert.True(buffer.Span.SequenceEqual(new byte[] { 3, 4, 5, 6 }));

        buffer.Append(new byte[] { 7, 8 }, 2);
        Assert.Equal(6, buffer.Length);
        Assert.True(buffer.Span.SequenceEqual(new byte[] { 3, 4, 5, 6, 7, 8 }));
    }

    [Fact]
    public void PooledByteBuffer_Clear_ResetsBuffer()
    {
        // Arrange
        using var buffer = new PooledByteBuffer();
        buffer.Append(new byte[] { 1, 2, 3, 4, 5 }, 5);

        // Act
        buffer.Clear();

        // Assert
        Assert.Equal(0, buffer.Length);
        Assert.True(buffer.Span.IsEmpty);
    }

    [Fact]
    public void PooledByteBuffer_WithParseFromSpan_WorksCorrectly()
    {
        // Arrange
        using var buffer = new PooledByteBuffer();
        var packet1 = CreateValidPacket(serviceHash: 0x11111111, methodId: 1, token: 1, payloadSize: 32);
        var packet2 = CreateValidPacket(serviceHash: 0x22222222, methodId: 2, token: 2, payloadSize: 64);

        buffer.Append(packet1, packet1.Length);
        buffer.Append(packet2, packet2.Length);

        // Act - parse first packet
        var result1 = BnetPacketParser.ParseFromSpan(buffer.Span);
        Assert.True(result1.Success);
        Assert.Equal(0x11111111u, result1.Header!.ServiceHash);
        buffer.Advance(result1.TotalLength);
        result1.ReturnPayload();

        // Parse second packet
        var result2 = BnetPacketParser.ParseFromSpan(buffer.Span);
        Assert.True(result2.Success);
        Assert.Equal(0x22222222u, result2.Header!.ServiceHash);
        buffer.Advance(result2.TotalLength);
        result2.ReturnPayload();

        // Assert
        Assert.Equal(0, buffer.Length);
    }
}
