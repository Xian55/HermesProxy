using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using Bgs.Protocol;
using BNetServer.Networking;
using Google.Protobuf;
using Xunit;

namespace HermesProxy.Tests.BnetServer
{
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
    }
}
