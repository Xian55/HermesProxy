using Framework.IO;
using Framework.GameMath;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using Xunit;
using System;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.Tests.Framework
{
    public class SpanPacketWriterTests
    {
        [Fact]
        public void WriteInt8_MatchesByteBuffer()
        {
            sbyte testValue = -42;

            using var buffer = new ByteBuffer();
            buffer.WriteInt8(testValue);
            byte[] expected = buffer.GetData();

            Span<byte> span = stackalloc byte[1];
            var writer = new SpanPacketWriter(span);
            writer.WriteInt8(testValue);

            Assert.Equal(expected, writer.ToArray());
        }

        [Fact]
        public void WriteUInt8_MatchesByteBuffer()
        {
            byte testValue = 0xAB;

            using var buffer = new ByteBuffer();
            buffer.WriteUInt8(testValue);
            byte[] expected = buffer.GetData();

            Span<byte> span = stackalloc byte[1];
            var writer = new SpanPacketWriter(span);
            writer.WriteUInt8(testValue);

            Assert.Equal(expected, writer.ToArray());
        }

        [Fact]
        public void WriteInt16_MatchesByteBuffer()
        {
            short testValue = -12345;

            using var buffer = new ByteBuffer();
            buffer.WriteInt16(testValue);
            byte[] expected = buffer.GetData();

            Span<byte> span = stackalloc byte[2];
            var writer = new SpanPacketWriter(span);
            writer.WriteInt16(testValue);

            Assert.Equal(expected, writer.ToArray());
        }

        [Fact]
        public void WriteUInt16_MatchesByteBuffer()
        {
            ushort testValue = 0xABCD;

            using var buffer = new ByteBuffer();
            buffer.WriteUInt16(testValue);
            byte[] expected = buffer.GetData();

            Span<byte> span = stackalloc byte[2];
            var writer = new SpanPacketWriter(span);
            writer.WriteUInt16(testValue);

            Assert.Equal(expected, writer.ToArray());
        }

        [Fact]
        public void WriteInt32_MatchesByteBuffer()
        {
            int testValue = -123456789;

            using var buffer = new ByteBuffer();
            buffer.WriteInt32(testValue);
            byte[] expected = buffer.GetData();

            Span<byte> span = stackalloc byte[4];
            var writer = new SpanPacketWriter(span);
            writer.WriteInt32(testValue);

            Assert.Equal(expected, writer.ToArray());
        }

        [Fact]
        public void WriteUInt32_MatchesByteBuffer()
        {
            uint testValue = 0xDEADBEEF;

            using var buffer = new ByteBuffer();
            buffer.WriteUInt32(testValue);
            byte[] expected = buffer.GetData();

            Span<byte> span = stackalloc byte[4];
            var writer = new SpanPacketWriter(span);
            writer.WriteUInt32(testValue);

            Assert.Equal(expected, writer.ToArray());
        }

        [Fact]
        public void WriteInt64_MatchesByteBuffer()
        {
            long testValue = -0x123456789ABCDEF0;

            using var buffer = new ByteBuffer();
            buffer.WriteInt64(testValue);
            byte[] expected = buffer.GetData();

            Span<byte> span = stackalloc byte[8];
            var writer = new SpanPacketWriter(span);
            writer.WriteInt64(testValue);

            Assert.Equal(expected, writer.ToArray());
        }

        [Fact]
        public void WriteUInt64_MatchesByteBuffer()
        {
            ulong testValue = 0xDEADBEEFCAFEBABE;

            using var buffer = new ByteBuffer();
            buffer.WriteUInt64(testValue);
            byte[] expected = buffer.GetData();

            Span<byte> span = stackalloc byte[8];
            var writer = new SpanPacketWriter(span);
            writer.WriteUInt64(testValue);

            Assert.Equal(expected, writer.ToArray());
        }

        [Fact]
        public void WriteFloat_MatchesByteBuffer()
        {
            float testValue = 3.14159f;

            using var buffer = new ByteBuffer();
            buffer.WriteFloat(testValue);
            byte[] expected = buffer.GetData();

            Span<byte> span = stackalloc byte[4];
            var writer = new SpanPacketWriter(span);
            writer.WriteFloat(testValue);

            Assert.Equal(expected, writer.ToArray());
        }

        [Fact]
        public void WriteDouble_MatchesByteBuffer()
        {
            double testValue = 3.141592653589793;

            using var buffer = new ByteBuffer();
            buffer.WriteDouble(testValue);
            byte[] expected = buffer.GetData();

            Span<byte> span = stackalloc byte[8];
            var writer = new SpanPacketWriter(span);
            writer.WriteDouble(testValue);

            Assert.Equal(expected, writer.ToArray());
        }

        [Fact]
        public void WriteVector3_MatchesByteBuffer()
        {
            var testValue = new Vector3(1.5f, 2.5f, 3.5f);

            using var buffer = new ByteBuffer();
            buffer.WriteVector3(testValue);
            byte[] expected = buffer.GetData();

            Span<byte> span = stackalloc byte[12];
            var writer = new SpanPacketWriter(span);
            writer.WriteVector3(testValue);

            Assert.Equal(expected, writer.ToArray());
        }

        [Fact]
        public void WriteBit_MatchesByteBuffer()
        {
            using var buffer = new ByteBuffer();
            buffer.WriteBit(true);
            buffer.WriteBit(false);
            buffer.WriteBit(true);
            buffer.WriteBit(true);
            buffer.WriteBit(false);
            buffer.WriteBit(false);
            buffer.WriteBit(true);
            buffer.WriteBit(false);
            buffer.FlushBits();
            byte[] expected = buffer.GetData();

            Span<byte> span = stackalloc byte[1];
            var writer = new SpanPacketWriter(span);
            writer.WriteBit(true);
            writer.WriteBit(false);
            writer.WriteBit(true);
            writer.WriteBit(true);
            writer.WriteBit(false);
            writer.WriteBit(false);
            writer.WriteBit(true);
            writer.WriteBit(false);
            writer.FlushBits();

            Assert.Equal(expected, writer.ToArray());
        }

        [Fact]
        public void WriteBits_MatchesByteBuffer()
        {
            uint testValue = 0b101101;
            int bitCount = 6;

            using var buffer = new ByteBuffer();
            buffer.WriteBits(testValue, bitCount);
            buffer.FlushBits();
            byte[] expected = buffer.GetData();

            Span<byte> span = stackalloc byte[1];
            var writer = new SpanPacketWriter(span);
            writer.WriteBits(testValue, bitCount);
            writer.FlushBits();

            Assert.Equal(expected, writer.ToArray());
        }

        [Fact]
        public void WriteMixed_MatchesByteBuffer()
        {
            // Test a realistic packet: int32, float, bits, uint8
            int intVal = 42;
            float floatVal = 1.5f;
            uint bits = 0b110;
            byte byteVal = 0xFF;

            using var buffer = new ByteBuffer();
            buffer.WriteInt32(intVal);
            buffer.WriteFloat(floatVal);
            buffer.WriteBits(bits, 3);
            buffer.FlushBits();
            buffer.WriteUInt8(byteVal);
            byte[] expected = buffer.GetData();

            Span<byte> span = stackalloc byte[10];
            var writer = new SpanPacketWriter(span);
            writer.WriteInt32(intVal);
            writer.WriteFloat(floatVal);
            writer.WriteBits(bits, 3);
            writer.FlushBits();
            writer.WriteUInt8(byteVal);

            Assert.Equal(expected, writer.ToArray());
        }

        [Fact]
        public void Position_TracksCorrectly()
        {
            Span<byte> span = stackalloc byte[32];
            var writer = new SpanPacketWriter(span);

            Assert.Equal(0, writer.Position);

            writer.WriteUInt32(1);
            Assert.Equal(4, writer.Position);

            writer.WriteUInt64(2);
            Assert.Equal(12, writer.Position);

            writer.WriteUInt8(3);
            Assert.Equal(13, writer.Position);
        }
    }

    public class SpanPacketReaderTests
    {
        [Fact]
        public void ReadInt8_MatchesByteBuffer()
        {
            sbyte testValue = -42;

            using var writeBuffer = new ByteBuffer();
            writeBuffer.WriteInt8(testValue);
            byte[] data = writeBuffer.GetData();

            var byteBufferReader = new ByteBuffer(data);
            sbyte expected = byteBufferReader.ReadInt8();

            var spanReader = new SpanPacketReader(data);
            sbyte actual = spanReader.ReadInt8();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReadUInt32_MatchesByteBuffer()
        {
            uint testValue = 0xDEADBEEF;

            using var writeBuffer = new ByteBuffer();
            writeBuffer.WriteUInt32(testValue);
            byte[] data = writeBuffer.GetData();

            var byteBufferReader = new ByteBuffer(data);
            uint expected = byteBufferReader.ReadUInt32();

            var spanReader = new SpanPacketReader(data);
            uint actual = spanReader.ReadUInt32();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReadInt64_MatchesByteBuffer()
        {
            long testValue = -0x123456789ABCDEF0;

            using var writeBuffer = new ByteBuffer();
            writeBuffer.WriteInt64(testValue);
            byte[] data = writeBuffer.GetData();

            var byteBufferReader = new ByteBuffer(data);
            long expected = byteBufferReader.ReadInt64();

            var spanReader = new SpanPacketReader(data);
            long actual = spanReader.ReadInt64();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReadFloat_MatchesByteBuffer()
        {
            float testValue = 3.14159f;

            using var writeBuffer = new ByteBuffer();
            writeBuffer.WriteFloat(testValue);
            byte[] data = writeBuffer.GetData();

            var byteBufferReader = new ByteBuffer(data);
            float expected = byteBufferReader.ReadFloat();

            var spanReader = new SpanPacketReader(data);
            float actual = spanReader.ReadFloat();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReadVector3_MatchesByteBuffer()
        {
            var testValue = new Vector3(1.5f, 2.5f, 3.5f);

            using var writeBuffer = new ByteBuffer();
            writeBuffer.WriteVector3(testValue);
            byte[] data = writeBuffer.GetData();

            var byteBufferReader = new ByteBuffer(data);
            var expected = byteBufferReader.ReadVector3();

            var spanReader = new SpanPacketReader(data);
            var actual = spanReader.ReadVector3();

            Assert.Equal(expected.X, actual.X);
            Assert.Equal(expected.Y, actual.Y);
            Assert.Equal(expected.Z, actual.Z);
        }

        [Fact]
        public void ReadBit_MatchesByteBuffer()
        {
            using var writeBuffer = new ByteBuffer();
            writeBuffer.WriteBit(true);
            writeBuffer.WriteBit(false);
            writeBuffer.WriteBit(true);
            writeBuffer.WriteBit(true);
            writeBuffer.WriteBit(false);
            writeBuffer.WriteBit(false);
            writeBuffer.WriteBit(true);
            writeBuffer.WriteBit(false);
            writeBuffer.FlushBits();
            byte[] data = writeBuffer.GetData();

            var byteBufferReader = new ByteBuffer(data);
            var spanReader = new SpanPacketReader(data);

            for (int i = 0; i < 8; i++)
            {
                Assert.Equal(byteBufferReader.ReadBit(), spanReader.ReadBit());
            }
        }

        [Fact]
        public void ReadBits_MatchesByteBuffer()
        {
            uint testValue = 0b101101;
            int bitCount = 6;

            using var writeBuffer = new ByteBuffer();
            writeBuffer.WriteBits(testValue, bitCount);
            writeBuffer.FlushBits();
            byte[] data = writeBuffer.GetData();

            var byteBufferReader = new ByteBuffer(data);
            uint expected = byteBufferReader.ReadBits<uint>(bitCount);

            var spanReader = new SpanPacketReader(data);
            uint actual = spanReader.ReadBits<uint>(bitCount);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReadCString_MatchesByteBuffer()
        {
            string testValue = "Hello, World!";

            using var writeBuffer = new ByteBuffer();
            writeBuffer.WriteCString(testValue);
            byte[] data = writeBuffer.GetData();

            var byteBufferReader = new ByteBuffer(data);
            string expected = byteBufferReader.ReadCString();

            var spanReader = new SpanPacketReader(data);
            string actual = spanReader.ReadCString();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReadMixed_MatchesByteBuffer()
        {
            int intVal = 42;
            float floatVal = 1.5f;
            uint bits = 0b110;
            byte byteVal = 0xFF;

            using var writeBuffer = new ByteBuffer();
            writeBuffer.WriteInt32(intVal);
            writeBuffer.WriteFloat(floatVal);
            writeBuffer.WriteBits(bits, 3);
            writeBuffer.FlushBits();
            writeBuffer.WriteUInt8(byteVal);
            byte[] data = writeBuffer.GetData();

            var byteBufferReader = new ByteBuffer(data);
            var spanReader = new SpanPacketReader(data);

            Assert.Equal(byteBufferReader.ReadInt32(), spanReader.ReadInt32());
            Assert.Equal(byteBufferReader.ReadFloat(), spanReader.ReadFloat());
            Assert.Equal(byteBufferReader.ReadBits<uint>(3), spanReader.ReadBits<uint>(3));
            Assert.Equal(byteBufferReader.ReadUInt8(), spanReader.ReadUInt8());
        }

        [Fact]
        public void Position_TracksCorrectly()
        {
            byte[] data = new byte[32];
            var reader = new SpanPacketReader(data);

            Assert.Equal(0, reader.Position);

            reader.ReadUInt32();
            Assert.Equal(4, reader.Position);

            reader.ReadUInt64();
            Assert.Equal(12, reader.Position);

            reader.ReadUInt8();
            Assert.Equal(13, reader.Position);
        }

        [Fact]
        public void CanRead_ReturnsCorrectly()
        {
            byte[] data = new byte[4];
            var reader = new SpanPacketReader(data);

            Assert.True(reader.CanRead);
            reader.ReadUInt32();
            Assert.False(reader.CanRead);
        }
    }

    public class SpanPacketRoundTripTests
    {
        [Fact]
        public void RoundTrip_SpanWriter_ByteBufferReader()
        {
            // Write with SpanPacketWriter, read with ByteBuffer
            long testValue = 0x123456789ABCDEF0;

            Span<byte> span = stackalloc byte[8];
            var writer = new SpanPacketWriter(span);
            writer.WriteInt64(testValue);

            var reader = new ByteBuffer(writer.ToArray());
            long result = reader.ReadInt64();

            Assert.Equal(testValue, result);
        }

        [Fact]
        public void RoundTrip_ByteBufferWriter_SpanReader()
        {
            // Write with ByteBuffer, read with SpanPacketReader
            long testValue = 0x123456789ABCDEF0;

            using var buffer = new ByteBuffer();
            buffer.WriteInt64(testValue);
            byte[] data = buffer.GetData();

            var reader = new SpanPacketReader(data);
            long result = reader.ReadInt64();

            Assert.Equal(testValue, result);
        }

        [Fact]
        public void RoundTrip_ComplexPacket()
        {
            // Simulate a realistic packet structure
            uint packetId = 0x1234;
            var position = new Vector3(100.5f, 200.5f, 300.5f);
            bool hasExtra = true;
            string name = "TestPlayer";

            // Write with SpanPacketWriter
            Span<byte> span = stackalloc byte[64];
            var writer = new SpanPacketWriter(span);
            writer.WriteUInt32(packetId);
            writer.WriteVector3(position);
            writer.WriteBit(hasExtra);
            writer.FlushBits();
            writer.WriteCString(name);

            // Read back with SpanPacketReader
            var reader = new SpanPacketReader(writer.GetWrittenSpan());
            Assert.Equal(packetId, reader.ReadUInt32());

            var readPos = reader.ReadVector3();
            Assert.Equal(position.X, readPos.X);
            Assert.Equal(position.Y, readPos.Y);
            Assert.Equal(position.Z, readPos.Z);

            Assert.Equal(hasExtra, reader.ReadBit());
            reader.ResetBitPos();
            Assert.Equal(name, reader.ReadCString());
        }
    }

    /// <summary>
    /// Note: Full MovementInfo comparison tests cannot run in unit tests because
    /// WriteMovementInfoModern depends on ModernVersion which requires application-level
    /// configuration. The Span-based implementation should be verified through integration
    /// testing with the actual proxy.
    ///
    /// These tests verify that the Span-based writer produces reasonable output and
    /// doesn't crash for various MovementInfo configurations.
    /// </summary>
    public class MovementInfoSpanTests
    {
        static MovementInfoSpanTests()
        {
            // Initialize required settings for ModernVersion static constructor
            // This must run before any test that uses ModernVersion.AddedInVersion()
            if (global::Framework.Settings.ClientBuild == ClientVersionBuild.Zero)
            {
                global::Framework.Settings.ClientBuild = ClientVersionBuild.V9_0_1_36216;
            }
        }

        [Fact]
        public void WriteMovementInfoModernToSpan_Simple_ProducesOutput()
        {
            var moveInfo = new MovementInfo
            {
                Flags = 0,
                FlagsExtra = 0,
                FlagsExtra2 = 0,
                MoveTime = 12345,
                Position = new Vector3(100.5f, 200.5f, 300.5f),
                Orientation = 1.5f,
                SwimPitch = 0.0f,
                SplineElevation = 0.0f,
                HasSplineData = false
            };

            var guid = new WowGuid128(0x123456789ABCDEF0, 0xFEDCBA9876543210);

            Span<byte> span = stackalloc byte[MovementInfo.MaxMovementInfoSize];
            int bytesWritten = moveInfo.WriteMovementInfoModernToSpan(span, guid.Low, guid.High);

            // Should write at least GUID + basic movement data
            Assert.True(bytesWritten > 0);
            Assert.True(bytesWritten <= MovementInfo.MaxMovementInfoSize);
        }

        [Fact]
        public void WriteMovementInfoModernToSpan_WithTransport_ProducesLargerOutput()
        {
            var moveInfoWithoutTransport = new MovementInfo
            {
                Flags = 0,
                MoveTime = 12345,
                Position = new Vector3(100.5f, 200.5f, 300.5f),
                Orientation = 1.5f
            };

            var moveInfoWithTransport = new MovementInfo
            {
                Flags = 0,
                MoveTime = 12345,
                Position = new Vector3(100.5f, 200.5f, 300.5f),
                Orientation = 1.5f,
                TransportGuid = new WowGuid128(0x1111111111111111, 0x2222222222222222),
                TransportOffset = new Vector3(1.0f, 2.0f, 3.0f),
                TransportOrientation = 0.5f,
                TransportSeat = 0,
                TransportTime = 1000
            };

            var guid = new WowGuid128(0xAAAABBBBCCCCDDDD, 0xEEEEFFFF00001111);

            Span<byte> span1 = stackalloc byte[MovementInfo.MaxMovementInfoSize];
            int bytesWithoutTransport = moveInfoWithoutTransport.WriteMovementInfoModernToSpan(span1, guid.Low, guid.High);

            Span<byte> span2 = stackalloc byte[MovementInfo.MaxMovementInfoSize];
            int bytesWithTransport = moveInfoWithTransport.WriteMovementInfoModernToSpan(span2, guid.Low, guid.High);

            // Transport data should make the packet larger
            Assert.True(bytesWithTransport > bytesWithoutTransport);
        }

        [Fact]
        public void WriteMovementInfoModernToSpan_WithFall_ProducesLargerOutput()
        {
            var moveInfoWithoutFall = new MovementInfo
            {
                Flags = 0,
                MoveTime = 99999,
                Position = new Vector3(10.0f, 20.0f, 30.0f),
                Orientation = 0.0f
            };

            var moveInfoWithFall = new MovementInfo
            {
                Flags = (uint)MovementFlagModern.Falling,
                MoveTime = 99999,
                Position = new Vector3(10.0f, 20.0f, 30.0f),
                Orientation = 0.0f,
                FallTime = 500,
                JumpVerticalSpeed = 5.0f,
                JumpSinAngle = 0.707f,
                JumpCosAngle = 0.707f,
                JumpHorizontalSpeed = 10.0f
            };

            var guid = new WowGuid128(0x0000000000000001, 0x0000000000000000);

            Span<byte> span1 = stackalloc byte[MovementInfo.MaxMovementInfoSize];
            int bytesWithoutFall = moveInfoWithoutFall.WriteMovementInfoModernToSpan(span1, guid.Low, guid.High);

            Span<byte> span2 = stackalloc byte[MovementInfo.MaxMovementInfoSize];
            int bytesWithFall = moveInfoWithFall.WriteMovementInfoModernToSpan(span2, guid.Low, guid.High);

            // Fall data should make the packet larger
            Assert.True(bytesWithFall > bytesWithoutFall);
        }

        [Fact]
        public void WriteMovementInfoModernToSpan_MaxSize_IsAdequate()
        {
            // Create a maximally complex movement info
            var moveInfo = new MovementInfo
            {
                Flags = (uint)MovementFlagModern.Falling,
                FlagsExtra = 0xFFFFFFFF,
                FlagsExtra2 = 0xFFFFFFFF,
                MoveTime = 0xFFFFFFFF,
                Position = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
                Orientation = float.MaxValue,
                SwimPitch = float.MaxValue,
                SplineElevation = float.MaxValue,
                HasSplineData = true,
                TransportGuid = new WowGuid128(ulong.MaxValue, ulong.MaxValue),
                TransportOffset = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
                TransportOrientation = float.MaxValue,
                TransportSeat = sbyte.MaxValue,
                TransportTime = uint.MaxValue,
                VehicleId = uint.MaxValue,
                FallTime = uint.MaxValue,
                JumpVerticalSpeed = float.MaxValue,
                JumpSinAngle = float.MaxValue,
                JumpCosAngle = float.MaxValue,
                JumpHorizontalSpeed = float.MaxValue
            };

            var guid = new WowGuid128(ulong.MaxValue, ulong.MaxValue);

            // Should not throw due to buffer overflow
            Span<byte> span = stackalloc byte[MovementInfo.MaxMovementInfoSize];
            int bytesWritten = moveInfo.WriteMovementInfoModernToSpan(span, guid.Low, guid.High);

            Assert.True(bytesWritten > 0);
            Assert.True(bytesWritten <= MovementInfo.MaxMovementInfoSize);
        }

        [Fact]
        public void WriteMovementInfoModernToSpan_DifferentGuids_ProduceDifferentOutput()
        {
            var moveInfo = new MovementInfo
            {
                Flags = 0,
                MoveTime = 12345,
                Position = new Vector3(100.5f, 200.5f, 300.5f),
                Orientation = 1.5f
            };

            var guid1 = new WowGuid128(0x0000000000000001, 0x0000000000000000);
            var guid2 = new WowGuid128(0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF);

            Span<byte> span1 = stackalloc byte[MovementInfo.MaxMovementInfoSize];
            int bytes1 = moveInfo.WriteMovementInfoModernToSpan(span1, guid1.Low, guid1.High);

            Span<byte> span2 = stackalloc byte[MovementInfo.MaxMovementInfoSize];
            int bytes2 = moveInfo.WriteMovementInfoModernToSpan(span2, guid2.Low, guid2.High);

            // Different GUIDs should produce different output
            // (at minimum the packed GUID bytes will differ)
            Assert.NotEqual(span1.Slice(0, bytes1).ToArray(), span2.Slice(0, bytes2).ToArray());
        }
    }
}
