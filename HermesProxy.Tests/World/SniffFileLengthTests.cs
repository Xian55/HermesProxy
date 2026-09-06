using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using HermesProxy.World;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// The record length <see cref="SniffFile"/> writes for a server packet must be the payload, not
/// the <see cref="ArrayPool{T}"/> rental it was received into.
///
/// This is the test that was missing while triaging
/// <see href="https://github.com/Xian55/HermesProxy/issues/248">#248</see>: a 42-byte
/// <c>SMSG_AUTH_CHALLENGE</c> arrives in a 64-byte rental, the capture recorded all 64, and the
/// resulting apparent 62-byte body (against a real 40) was misread as proof of a customised
/// server core. A stock TrinityCore produces the identical inflated capture.
/// </summary>
public class SniffFileLengthTests
{
    private const ushort LegacyBuild = 12340;
    private const ushort SmsgAuthChallenge = 0x1EC;

    // 2-byte opcode + 40-byte body, the real 3.3.5a SMSG_AUTH_CHALLENGE frame.
    private const int PacketSize = 42;

    /// <summary>PKT header: "PKT" + uint16 version + uint16 build + 40 session-key bytes.</summary>
    private const int PktHeaderSize = 3 + 2 + 2 + 40;

    /// <summary>Per-record prologue: direction byte + unixtime + tickcount + size.</summary>
    private const int RecordPrologueSize = 1 + 4 + 4 + 4;

    [Fact]
    public void WritePacket_ServerPacketFromPooledBuffer_RecordsPayloadLengthNotRentalLength()
    {
        byte[] rental = ArrayPool<byte>.Shared.Rent(PacketSize);
        SniffFile? sniff = null;
        try
        {
            Assert.True(
                rental.Length > PacketSize,
                $"ArrayPool returned an exact-size {rental.Length}-byte array; this test no longer proves anything.");

            // Poison the slack so a regression shows up as recorded bytes, not just a length.
            rental.AsSpan().Fill(0xAA);
            BinaryPrimitives.WriteUInt16LittleEndian(rental, SmsgAuthChallenge);
            rental.AsSpan(2, PacketSize - 2).Clear();

            using var packet = new WorldPacket(rental, PacketSize, isPooled: false);

            // EnsureOpen rather than the ctor: it is what production uses, and it writes the
            // PKT header before publishing the reference.
            SniffFile slot = null!;
            sniff = SniffFile.EnsureOpen(ref slot, "test-authchallenge", LegacyBuild);
            sniff.WritePacket(packet.GetOpcode(), isFromClient: false, packet.GetDataSpan());
            sniff.CloseFile();

            byte[] written = File.ReadAllBytes(sniff.FilePath);

            uint recordedSize = BinaryPrimitives.ReadUInt32LittleEndian(
                written.AsSpan(PktHeaderSize + RecordPrologueSize - 4, 4));

            // SniffFile prefixes the server record with its own uint16 opcode.
            Assert.Equal((uint)(PacketSize + sizeof(ushort)), recordedSize);
            Assert.Equal(PktHeaderSize + RecordPrologueSize + PacketSize + sizeof(ushort), written.Length);

            // None of the 0xAA slack may have reached the file.
            Assert.DoesNotContain((byte)0xAA, written);
        }
        finally
        {
            sniff?.CloseFile();
            if (sniff is not null && File.Exists(sniff.FilePath))
                File.Delete(sniff.FilePath);
            ArrayPool<byte>.Shared.Return(rental);
        }
    }
}
