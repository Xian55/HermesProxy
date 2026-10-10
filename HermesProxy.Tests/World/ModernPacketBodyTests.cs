using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// Wire cover for <see cref="ModernPacketBody"/>, which replaced the two-ByteBuffer body build in
/// <c>WorldSocket.SendPacket</c>. The bytes must be identical to what that code produced, and a
/// compressed body must decode the way the client decodes it: one inflater for the whole
/// connection, checksums seeded with 0x9827D8F1.
/// </summary>
/// <remarks>
/// Each layout is checked with both opcode sizes: 2 bytes before 4.4.0, 4 from it, where
/// TrinityCore's cata_classic <c>WorldSocket::WritePacketToBuffer</c> writes a <c>uint32</c> opcode
/// both outside and inside the compressed envelope.
/// </remarks>
public class ModernPacketBodyTests
{
    private const uint AdlerSeed = 0x9827D8F1;

    // SMSG_COMPRESSED_PACKET of 3.4.3 and of 4.4.2, whose opcodes carry a group in the upper bits.
    private const uint CompressedOpcode16 = 0x3052;
    private const uint CompressedOpcode32 = 0x42000A;

    // Inside the envelope: SMSG_UPDATE_OBJECT of the same two builds.
    public static TheoryData<int, uint, uint> Opcodes() => new()
    {
        { sizeof(ushort), 0x27CB, CompressedOpcode16 },
        { sizeof(uint), 0x4B0000, CompressedOpcode32 },
    };

    [Fact]
    public void V3_4_3_MapsCompressedPacketToNativeOpcode()
    {
        // Native 3.4.3 (TrinityCore wotlk_classic / Wrathion Opcodes.h) sends SMSG_COMPRESSED_PACKET
        // as 0x3052, the same value 1.14 and 2.5 use. Unmapped, WorldSocket never compresses.
        Assert.True(GeneratedOpcodeTables.TryGet(ClientVersionBuild.V3_4_3_54261, out _, out uint[] universalToCurrent, out int opcodeSize));
        Assert.Equal(0x3052u, universalToCurrent[(int)Opcode.SMSG_COMPRESSED_PACKET]);
        Assert.Equal(sizeof(ushort), opcodeSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(300)]
    [InlineData(1025)]
    public void WritePlain_MatchesPreviousByteBufferBuild(int payloadLength)
    {
        byte[] payload = Payload(payloadLength, seed: 7);
        const ushort opcode = 0x27CB;

        byte[] expected;
        using (ByteBuffer body = new())
        {
            body.WriteUInt16(opcode);
            body.WriteBytes(payload);
            expected = body.GetData();
        }

        byte[] actual = new byte[ModernPacketBody.PlainSize(sizeof(ushort), payload.Length)];
        ModernPacketBody.WritePlain(actual, opcode, sizeof(ushort), payload);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(300)]
    public void WritePlain_FourByteOpcode_LeadsWithTheWholeOpcode(int payloadLength)
    {
        byte[] payload = Payload(payloadLength, seed: 7);
        const uint opcode = 0x420006;   // 4.4.2 SMSG_PONG

        byte[] actual = new byte[ModernPacketBody.PlainSize(sizeof(uint), payload.Length)];
        ModernPacketBody.WritePlain(actual, opcode, sizeof(uint), payload);

        Assert.Equal(sizeof(uint) + payload.Length, actual.Length);
        Assert.Equal(opcode, BinaryPrimitives.ReadUInt32LittleEndian(actual));
        Assert.Equal(payload, actual.AsSpan(sizeof(uint)).ToArray());
    }

    [Theory]
    [InlineData(1025)]
    [InlineData(4096)]
    [InlineData(70_000)]
    public void WriteCompressed_MatchesPreviousByteBufferBuild(int payloadLength)
    {
        byte[] payload = Payload(payloadLength, seed: 11);
        const ushort opcode = 0x27CB;
        byte[] deflated = DeflateOne(new DeflateSession(), opcode, sizeof(ushort), payload);

        // The envelope exactly as WorldSocket.SendPacket built it before this class existed.
        byte[] expected;
        using (ByteBuffer compressed = new())
        {
            compressed.WriteInt32(payload.Length + 2);
            Span<byte> opcodeBytes = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16LittleEndian(opcodeBytes, opcode);
            compressed.WriteUInt32(Adler32.Update(Adler32.Update(AdlerSeed, opcodeBytes), payload));
            compressed.WriteUInt32(Adler32.Update(AdlerSeed, deflated));
            compressed.WriteBytes(deflated);
            byte[] envelope = compressed.GetData();

            using ByteBuffer body = new();
            body.WriteUInt16((ushort)CompressedOpcode16);
            body.WriteBytes(envelope);
            expected = body.GetData();
        }

        byte[] actual = new byte[ModernPacketBody.CompressedSize(sizeof(ushort), deflated.Length)];
        ModernPacketBody.WriteCompressed(actual, CompressedOpcode16, opcode, sizeof(ushort), payload, deflated);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(Opcodes))]
    public void WriteCompressed_EnvelopeCarriesSizeAndSeededChecksums(int opcodeSize, uint opcode, uint compressedOpcode)
    {
        byte[] payload = Payload(2048, seed: 3);
        byte[] deflated = DeflateOne(new DeflateSession(), opcode, opcodeSize, payload);

        byte[] body = new byte[ModernPacketBody.CompressedSize(opcodeSize, deflated.Length)];
        ModernPacketBody.WriteCompressed(body, compressedOpcode, opcode, opcodeSize, payload, deflated);

        byte[] opcodeAndPayload = new byte[opcodeSize + payload.Length];
        WriteReferenceOpcode(opcodeAndPayload, opcode, opcodeSize);
        payload.CopyTo(opcodeAndPayload, opcodeSize);

        Assert.Equal(compressedOpcode, ReadReferenceOpcode(body, opcodeSize));
        Assert.Equal(payload.Length + opcodeSize, BinaryPrimitives.ReadInt32LittleEndian(body.AsSpan(opcodeSize)));
        Assert.Equal(ReferenceAdler(AdlerSeed, opcodeAndPayload), BinaryPrimitives.ReadUInt32LittleEndian(body.AsSpan(opcodeSize + 4)));
        Assert.Equal(ReferenceAdler(AdlerSeed, deflated), BinaryPrimitives.ReadUInt32LittleEndian(body.AsSpan(opcodeSize + 8)));
        Assert.Equal(deflated, body.AsSpan(opcodeSize + 12).ToArray());
    }

    [Theory]
    [MemberData(nameof(Opcodes))]
    public void CompressedBodies_DecodeOnOneClientSideInflater(int opcodeSize, uint opcode, uint compressedOpcode)
    {
        // The client keeps one inflate stream per connection, so each body only decodes after
        // every body before it. Mixed sizes and opcodes, one deflate session, like WorldSocket.
        var session = new DeflateSession();
        (uint Opcode, byte[] Payload)[] packets =
        [
            (opcode, Payload(1500, seed: 1)),
            (opcode + 9, Payload(1025, seed: 2)),
            (opcode, Payload(1500, seed: 1)),   // repeat: exercises back-references across packets
            (opcode + 0x54, Payload(9000, seed: 4)),
        ];
        int infoEnd = opcodeSize + ModernPacketBody.CompressedInfoSize;

        using var wire = new MemoryStream();
        foreach (var (packetOpcode, payload) in packets)
        {
            byte[] deflated = DeflateOne(session, packetOpcode, opcodeSize, payload);
            byte[] body = new byte[ModernPacketBody.CompressedSize(opcodeSize, deflated.Length)];
            ModernPacketBody.WriteCompressed(body, compressedOpcode, packetOpcode, opcodeSize, payload, deflated);

            Assert.True(body.AsSpan(body.Length - 4).SequenceEqual((ReadOnlySpan<byte>)[0x00, 0x00, 0xFF, 0xFF]),
                "each body must end on a sync-flush boundary or the client cannot decode it yet");
            wire.Write(body, infoEnd, body.Length - infoEnd);
        }

        wire.Position = 0;
        using var inflater = new DeflateStream(wire, CompressionMode.Decompress);
        foreach (var (packetOpcode, payload) in packets)
        {
            byte[] got = new byte[opcodeSize + payload.Length];
            inflater.ReadExactly(got);
            Assert.Equal(packetOpcode, ReadReferenceOpcode(got, opcodeSize));
            Assert.Equal(payload, got.AsSpan(opcodeSize).ToArray());
        }
    }

    /// <summary>What WorldSocket keeps per connection: one deflater, sync-flushed per packet.</summary>
    private sealed class DeflateSession
    {
        public readonly MemoryStream Buffer = new();
        public readonly DeflateStream Stream;

        public DeflateSession() => Stream = new DeflateStream(Buffer, CompressionLevel.Fastest, leaveOpen: true);
    }

    private static byte[] DeflateOne(DeflateSession session, uint opcode, int opcodeSize, ReadOnlySpan<byte> payload)
    {
        session.Buffer.SetLength(0);
        byte[] hdr = new byte[opcodeSize];
        WriteReferenceOpcode(hdr, opcode, opcodeSize);
        session.Stream.Write(hdr);
        session.Stream.Write(payload);
        session.Stream.Flush();
        return session.Buffer.ToArray();
    }

    private static void WriteReferenceOpcode(Span<byte> destination, uint opcode, int opcodeSize)
    {
        if (opcodeSize == sizeof(uint))
            BinaryPrimitives.WriteUInt32LittleEndian(destination, opcode);
        else
            BinaryPrimitives.WriteUInt16LittleEndian(destination, checked((ushort)opcode));
    }

    private static uint ReadReferenceOpcode(ReadOnlySpan<byte> source, int opcodeSize)
        => opcodeSize == sizeof(uint)
            ? BinaryPrimitives.ReadUInt32LittleEndian(source)
            : BinaryPrimitives.ReadUInt16LittleEndian(source);

    // Compressible but not trivial: update-object-like runs of small values.
    private static byte[] Payload(int length, int seed)
    {
        var rng = new Random(seed);
        byte[] bytes = new byte[length];
        for (int i = 0; i < length; i++)
            bytes[i] = (byte)(i % 16 < 12 ? rng.Next(4) : rng.Next(256));
        return bytes;
    }

    // Textbook Adler-32, independent of Framework.IO.Adler32.
    private static uint ReferenceAdler(uint adler, ReadOnlySpan<byte> data)
    {
        uint a = adler & 0xFFFF, b = adler >> 16;
        foreach (byte x in data)
        {
            a = (a + x) % 65521;
            b = (b + a) % 65521;
        }
        return (b << 16) | a;
    }
}
