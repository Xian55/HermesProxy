using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HermesProxy.World;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// <see cref="SniffFile"/> writes PKT 2.1 for 2-byte opcodes and PKT 3.1 for 4-byte ones. Each
/// capture here is read back the way WowPacketParser's <c>BinaryPacketReader</c> reads that
/// version, so a record that parses here parses there.
/// </summary>
public class SniffFileFormatTests
{
    private const uint Build = 54261;
    private const uint ServerToClient = 0x47534D53;
    private const uint ClientToServer = 0x47534D43;

    // SMSG_PONG and CMSG_PING are 0x304E and 0x3768 in 3.4.3, 0x420006 and 0x3A0004 in 4.4.2.
    private static readonly byte[] ServerPayload = [0x01, 0x02, 0x03];
    private static readonly byte[] ClientPayload = [0x0A, 0x0B];

    [Fact]
    public void TwoByteOpcodes_WritePkt21()
    {
        var records = WriteAndRead(sizeof(ushort), serverOpcode: 0x304E, clientOpcode: 0x3768, out ushort version, out uint build);

        Assert.Equal(0x201, version);
        Assert.Equal(Build, build);
        List<(bool, uint, byte[])> expected = [(false, 0x304Eu, ServerPayload), (true, 0x3768u, ClientPayload)];
        Assert.Equal(expected, records, RecordComparer.Instance);
    }

    [Fact]
    public void FourByteOpcodes_WritePkt31()
    {
        var records = WriteAndRead(sizeof(uint), serverOpcode: 0x420006, clientOpcode: 0x3A0004, out ushort version, out uint build);

        Assert.Equal(0x301, version);
        Assert.Equal(Build, build);
        List<(bool, uint, byte[])> expected = [(false, 0x420006u, ServerPayload), (true, 0x3A0004u, ClientPayload)];
        Assert.Equal(expected, records, RecordComparer.Instance);
    }

    private static List<(bool FromClient, uint Opcode, byte[] Payload)> WriteAndRead(
        int opcodeSize, uint serverOpcode, uint clientOpcode, out ushort version, out uint build)
    {
        SniffFile slot = null!;
        SniffFile sniff = SniffFile.EnsureOpen(ref slot, $"test-format-{opcodeSize}", Build, opcodeSize);
        try
        {
            sniff.WritePacket(serverOpcode, isFromClient: false, ServerPayload);
            sniff.WritePacket(clientOpcode, isFromClient: true, ClientPayload);
            sniff.CloseFile();

            using var reader = new BinaryReader(new MemoryStream(File.ReadAllBytes(sniff.FilePath)));
            return Read(reader, out version, out build);
        }
        finally
        {
            sniff.CloseFile();
            File.Delete(sniff.FilePath);
        }
    }

    // BinaryPacketReader.ReadHeader and Read, the V2_1 and V3_1 arms.
    private static List<(bool, uint, byte[])> Read(BinaryReader reader, out ushort version, out uint build)
    {
        Assert.Equal("PKT", Encoding.ASCII.GetString(reader.ReadBytes(3)));
        version = reader.ReadUInt16();
        if (version == 0x201)
        {
            build = reader.ReadUInt16();
            reader.ReadBytes(40);
        }
        else
        {
            Assert.Equal(0x301, version);
            reader.ReadByte();
            build = reader.ReadUInt32();
            Assert.Equal("enUS", Encoding.ASCII.GetString(reader.ReadBytes(4)));
            reader.ReadBytes(40);
            reader.ReadUInt32();
            reader.ReadUInt32();
            reader.ReadBytes(reader.ReadInt32());
        }

        var records = new List<(bool, uint, byte[])>();
        while (reader.BaseStream.Position != reader.BaseStream.Length)
        {
            if (version == 0x201)
            {
                bool fromClient = reader.ReadByte() != 0xff;
                reader.ReadInt32();
                reader.ReadInt32();
                int length = reader.ReadInt32();
                uint opcode = fromClient ? reader.ReadUInt32() : reader.ReadUInt16();
                records.Add((fromClient, opcode, reader.ReadBytes(length - (fromClient ? 4 : 2))));
            }
            else
            {
                uint direction = reader.ReadUInt32();
                Assert.Contains(direction, (uint[])[ServerToClient, ClientToServer]);
                reader.ReadInt32();
                reader.ReadUInt32();
                int additionalSize = reader.ReadInt32();
                int length = reader.ReadInt32();
                reader.ReadBytes(additionalSize);
                uint opcode = reader.ReadUInt32();
                records.Add((direction == ClientToServer, opcode, reader.ReadBytes(length - 4)));
            }
        }
        return records;
    }

    private sealed class RecordComparer : IEqualityComparer<(bool FromClient, uint Opcode, byte[] Payload)>
    {
        public static readonly RecordComparer Instance = new();

        public bool Equals((bool FromClient, uint Opcode, byte[] Payload) x, (bool FromClient, uint Opcode, byte[] Payload) y)
            => x.FromClient == y.FromClient && x.Opcode == y.Opcode && x.Payload.AsSpan().SequenceEqual(y.Payload);

        public int GetHashCode((bool FromClient, uint Opcode, byte[] Payload) obj) => HashCode.Combine(obj.FromClient, obj.Opcode);
    }
}
