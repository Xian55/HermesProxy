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

using Framework.IO;
using Framework.Constants;
using Framework.Logging;
using System;
using System.Buffers;
using System.Buffers.Binary;
using HermesProxy.World.Enums;
using HermesProxy.World.Client;
using HermesProxy.World.Logging;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HermesProxy.World;

/// <summary>
/// Per-connection packet-log configuration snapshot. Captured once in the owning session so
/// LogPacket can read PacketsLog / ClientBuild from instance fields instead of static
/// Framework.Settings lookups on every packet.
/// </summary>
public readonly record struct PacketLogContext(bool PacketsLog, HermesProxy.Enums.ClientVersionBuild ClientBuild)
{
    public static readonly PacketLogContext Disabled = new(false, HermesProxy.Enums.ClientVersionBuild.Zero);
}

public abstract class ClientPacket : IDisposable
{
    protected ClientPacket(WorldPacket worldPacket)
    {
        _worldPacket = worldPacket;
    }

    public abstract void Read();

    public void Dispose()
    {
        _worldPacket.Dispose();
    }

    public uint GetOpcode() { return _worldPacket.GetOpcode(); }
    public Opcode GetUniversalOpcode()
    {
        return ModernVersion.GetUniversalOpcode(GetOpcode());
    }

    public void LogPacket(ref SniffFile sniffFile, in PacketLogContext context)
        => _worldPacket.LogPacket(ref sniffFile, in context);

    protected WorldPacket _worldPacket;
}

public abstract class ServerPacket
{
    // Source-generated [LoggerMessage] methods use the MEL logger below.
    // MinimumLevel.Override("Packet", _packetSwitch) still applies because we create the MEL logger
    // with the "Packet" category name, which becomes SourceContext on the Serilog side.
    private static readonly Microsoft.Extensions.Logging.ILogger _melLog = Log.CreateMelLogger(Log.CategoryPacket);
    private static readonly string _sourceFile = nameof(ServerPacket).PadRight(15);

    protected ServerPacket(Opcode universalOpcode)
    {
        connectionType = ConnectionType.Realm;

        opcode = ModernVersion.GetCurrentOpcode(universalOpcode);
        if (opcode == 0)
            throw new UnmappedOpcodeException(universalOpcode, isModern: true);
    }

    protected ServerPacket(Opcode universalOpcode, ConnectionType type = ConnectionType.Realm)
    {
        connectionType = type;

        opcode = ModernVersion.GetCurrentOpcode(universalOpcode);
        if (opcode == 0)
            throw new UnmappedOpcodeException(universalOpcode, isModern: true);
    }

    public void Clear()
    {
        worldPacket?.Clear();
        ReleaseData();
    }

    public uint GetOpcode()
    {
        return worldPacket?.GetOpcode() ?? opcode;
    }
    public Opcode GetUniversalOpcode()
    {
        return ModernVersion.GetUniversalOpcode(GetOpcode());
    }

    /// <summary>The serialized packet, valid from <see cref="WritePacketData"/> until <see cref="ReleaseData"/>.</summary>
    public ReadOnlySpan<byte> GetDataSpan() => buffer.AsSpan(0, bufferLength);

    /// <summary>A copy of the serialized packet. The send path reads <see cref="GetDataSpan"/>.</summary>
    public byte[]? GetData() => buffer?.AsSpan(0, bufferLength).ToArray();

    public void LogPacket(ref SniffFile sniffFile, in PacketLogContext context)
    {
        if (!context.PacketsLog)
            return;

        var sniff = SniffFile.EnsureOpen(ref sniffFile, "modern", (ushort)context.ClientBuild);
        sniff.WritePacket(GetOpcode(), false, GetDataSpan());
    }

    public abstract void Write();

    /// <remarks>
    /// The bytes stay in the pooled array they were written into, and <see cref="ReleaseData"/>
    /// hands it back once they are on the wire. They used to be copied out into an exact-size
    /// array for every packet sent, about 30 MB over an 18-minute Alterac Valley.
    /// </remarks>
    public void WritePacketData()
    {
        if (buffer != null)
            return;

        // Fast path: Use Span-based writing for packets that support it
        if (this is ISpanWritable spanWritable)
        {
            byte[] pooledBuffer = ArrayPool<byte>.Shared.Rent(spanWritable.MaxSize);
            int bytesWritten = spanWritable.WriteToSpan(pooledBuffer);

            // Negative return means packet exceeded MaxSize cap, fall back to standard Write()
            if (bytesWritten < 0)
            {
                ArrayPool<byte>.Shared.Return(pooledBuffer);
                PacketLogMessages.SpanMissExceededMaxSize(_melLog, _sourceFile, GetType().Name, spanWritable.MaxSize);
                Write();
                TakeWrittenData();
            }
            else
            {
                PacketLogMessages.SpanStats(_melLog, _sourceFile, GetType().Name, bytesWritten, spanWritable.MaxSize);
                buffer = pooledBuffer;
                bufferLength = bytesWritten;
                bufferPooled = true;
            }
        }
        else
        {
            // Standard path: Use ByteBuffer-based writing
            Write();
            TakeWrittenData();
        }

        // A later WritePacketData, after ReleaseData, serializes into a fresh one.
        worldPacket?.Dispose();
        worldPacket = null;
    }

    private void TakeWrittenData()
    {
        buffer = _worldPacket.DetachBuffer(out bufferLength, out bufferPooled);
    }

    /// <summary>
    /// Returns the serialized bytes to the pool. Called once they are on the wire; a packet that is
    /// sent again serializes itself again.
    /// </summary>
    public void ReleaseData()
    {
        if (buffer != null && bufferPooled)
            ArrayPool<byte>.Shared.Return(buffer);
        buffer = null;
        bufferLength = 0;
        bufferPooled = false;
    }

    /// <summary>
    /// Releases the pooled buffers of a packet that will never be sent.
    /// </summary>
    /// <remarks>
    /// A packet dropped before it is written holds the rental of its first write into
    /// <see cref="_worldPacket"/>; one dropped after writing but before sending - the socket closed
    /// in between, or a park that expired - holds its serialized bytes. Safe to call twice.
    /// </remarks>
    public void Discard()
    {
        worldPacket?.Dispose();
        ReleaseData();
    }

    public ConnectionType GetConnection() { return connectionType; }

    byte[]? buffer;
    int bufferLength;
    bool bufferPooled;
    ConnectionType connectionType;
    readonly uint opcode;
    WorldPacket? worldPacket;

    /// <summary>
    /// The buffer <see cref="Write"/> serialises into, created on first use.
    /// </summary>
    /// <remarks>
    /// ISpanWritable packets never touch it, and a scratch packet that turns out to have nothing
    /// to say (the aura and power updates built for every Values block) is never written. Both
    /// used to carry a WorldPacket from construction to send for nothing. Named like a field so
    /// the Write() of every packet type reads unchanged.
    /// </remarks>
    protected WorldPacket _worldPacket => worldPacket ??= new WorldPacket(opcode);
}

public class WorldPacket : ByteBuffer
{
    public WorldPacket(uint opcode = 0)
    {
        this.opcode = opcode;
    }

    public WorldPacket(Opcode opcode)
    {
        this.opcode = LegacyVersion.GetCurrentOpcode(opcode);
        if (this.opcode == 0)
            throw new UnmappedOpcodeException(opcode, isModern: false);
    }

    public WorldPacket(uint opcode, byte[] data) : base(data)
    {
        this.opcode = opcode;

        // Was a Trace.Assert. Both callers are legacy *receive* paths — SMSG_COMPRESSED_MOVES
        // reads this opcode straight off the wire, and Inflate() copies it from the parent
        // packet — so a zero here means corrupt or misaligned input, not a programming error.
        // Aborting the process on malformed server data is the worst possible response; log it
        // and let the dispatcher drop it as an unknown opcode.
        if (this.opcode == 0)
            Log.Print(LogType.Warn, "Constructed a legacy packet with opcode 0 from received data; it will be dropped as unknown.");
    }

    public WorldPacket(byte[] data) : base(data)
    {
        opcode = ReadUInt16();
    }

    /// Read-mode ctor for a possibly-oversized backing buffer with an explicit payload length.
    /// Pass isPooled=true when `data` came from ArrayPool<byte>.Shared so Dispose returns it.
    public WorldPacket(byte[] data, int length, bool isPooled) : base(data, length, isPooled)
    {
        opcode = ReadUInt16();
    }

    /// Read-mode ctor for a pooled, possibly-oversized buffer whose opcode is already known
    /// (Inflate carries it over from the parent packet rather than reading it from the payload).
    /// Pass isPooled=true when `data` came from ArrayPool<byte>.Shared so Dispose returns it.
    public WorldPacket(uint opcode, byte[] data, int length, bool isPooled) : base(data, length, isPooled)
    {
        this.opcode = opcode;

        if (this.opcode == 0)
            Log.Print(LogType.Warn, "Constructed a legacy packet with opcode 0 from received data; it will be dropped as unknown.");
    }

    public KeyValuePair<int, bool> ReadEntry()
    {
        // Entries masked with 0x80000000 are invalid entries OR used to tell apart NPCs and GOs

        var entry = ReadUInt32();
        var realEntry = entry & 0x7FFFFFFF;

        return new KeyValuePair<int, bool>((int)realEntry, realEntry != entry);
    }

    public WowGuid64 ReadGuid()
    {
        var guid = new WowGuid64(ReadUInt64());
        return guid;
    }

    public WowGuid64 ReadPackedGuid()
    {
        var guid = new WowGuid64(ReadPackedUInt64(ReadUInt8()));
        return guid;
    }

    public WowGuid128 ReadPackedGuid128()
    {
        var loLength = ReadUInt8();
        var hiLength = ReadUInt8();
        var low = ReadPackedUInt64(loLength);
        return new WowGuid128(low, ReadPackedUInt64(hiLength));
    }

    private ulong ReadPackedUInt64(byte length)
    {
        if (length == 0)
            return 0;

        var guid = 0ul;

        for (var i = 0; i < 8; i++)
            if ((1 << i & length) != 0)
                guid |= (ulong)ReadUInt8() << (i * 8);

        return guid;
    }

    public UpdateField ReadUpdateField()
    {
        uint val = ReadUInt32();

        var field = new UpdateField(val);
        return field;
    }

    /// <summary>
    /// Inflates the rest of this packet into a new one. The caller owns the result and must
    /// dispose it — the payload buffer is pooled.
    /// </summary>
    /// <remarks>
    /// SMSG_COMPRESSED_UPDATE_OBJECT is the single largest allocator on the receive path, so
    /// neither copy this used to make is affordable: the compressed bytes are inflated straight
    /// out of this packet's buffer, and the destination is rented rather than allocated.
    /// </remarks>
    public WorldPacket Inflate(int inflatedSize)
    {
        byte[] rented = ArrayPool<byte>.Shared.Rent(inflatedSize);
        InflateRemainingInto(rented.AsSpan(0, inflatedSize));

        var pkt = new WorldPacket(GetOpcode(), rented, inflatedSize, isPooled: true);
        pkt.SetReceiveTime(GetReceivedTime());
        return pkt;
    }

    public void WriteGuid(WowGuid64 guid)
    {
        WriteUInt64(guid.GetLowValue());
    }

    public void WritePackedGuid(WowGuid64 guid)
    {
        WritePackedUInt64(guid.Low);
    }

    // Packed on the stack: the previous shape allocated a byte[8] per half of every GUID written,
    // 28 MB over an 18-minute Alterac Valley. WriteBytes flushes pending bits exactly as the
    // WriteUInt8 calls it replaces did, so the bytes are unchanged.
    public void WritePackedGuid128(WowGuid128 guid)
    {
        Span<byte> packed = stackalloc byte[PackedGuidHelper.MaxPackedGuid128Size];
        WriteBytes(packed[..PackedGuidHelper.WritePackedGuid128(packed, guid.GetLowValue(), guid.GetHighValue())]);
    }

    public void WritePackedUInt64(ulong guid)
    {
        Span<byte> packed = stackalloc byte[1 + sizeof(ulong)];
        WriteBytes(packed[..PackedGuidHelper.WritePackedUInt64(packed, guid)]);
    }

    public void WriteBytes(WorldPacket data)
    {
        FlushBits();
        WriteBytes(data.GetData());
    }

    /// <summary>
    /// Writes this inbound packet to the modern sniff. Lives here rather than on
    /// <see cref="ClientPacket"/> because generated dispatch reads straight off the
    /// <see cref="WorldPacket"/> and never builds a <see cref="ClientPacket"/> — and losing the
    /// sniff for converted opcodes would be invisible, with the capture still looking plausible.
    /// </summary>
    public void LogPacket(ref SniffFile sniffFile, in PacketLogContext context)
    {
        if (!context.PacketsLog)
            return;

        var sniff = SniffFile.EnsureOpen(ref sniffFile, "modern", (ushort)context.ClientBuild);
        // GetDataSpan: this packet was received into a pooled rental, and GetData would hand back
        // the whole bucket-rounded buffer rather than the payload (issue #248).
        sniff.WritePacket(GetOpcode(), true, GetDataSpan());
    }

    public uint GetOpcode() { return opcode; }
    public Opcode GetUniversalOpcode(bool isModern)
    {
        if (isModern)
            return ModernVersion.GetUniversalOpcode(GetOpcode());
        else
            return LegacyVersion.GetUniversalOpcode(GetOpcode());
    }

    public int GetReceivedTime() { return m_receivedTime; }
    public void SetReceiveTime(int receivedTime) { m_receivedTime = receivedTime; }

    uint opcode;
    // An Environment.TickCount, which is an int and wraps every ~25 days. Held as a long, it cost
    // this object eight bytes (and, with the alignment, sixteen) for no extra range.
    int m_receivedTime;
}

/// <summary>
/// Storage for <see cref="PacketHeader.Tag"/>. An inline array so the 12 AES-GCM tag bytes
/// live inside the header itself; the implicit <see cref="Span{T}"/> conversion lets
/// <see cref="Framework.Cryptography.PacketCrypt"/> write the tag straight into that storage.
/// </summary>
[InlineArray(PacketHeader.TagSize)]
public struct PacketTag
{
    private byte _element0;
}

public struct PacketHeader
{
    public const int TagSize = 12;
    public const int StructSize = sizeof(int) + TagSize;

    public int Size;
    public PacketTag Tag;

    public void Read(ReadOnlySpan<byte> buffer)
    {
        Size = BinaryPrimitives.ReadInt32LittleEndian(buffer);
        buffer.Slice(sizeof(int), TagSize).CopyTo(Tag);
    }

    /// <summary>Writes the 16-byte header into the front of <paramref name="destination"/>.</summary>
    public readonly void Write(Span<byte> destination)
    {
        BinaryPrimitives.WriteInt32LittleEndian(destination, Size);
        ((ReadOnlySpan<byte>)Tag).CopyTo(destination[sizeof(int)..]);
    }

    public readonly bool IsValidSize() { return (uint)Size < 0x40000; }
}

// A struct: the receive loop parses one of these for every legacy packet, and as a class it was a
// heap object per packet (24 MB over an 18-minute Alterac Valley) for four bytes of header.
public struct LegacyServerPacketHeader
{
    // WotLK-era cores (AzerothCore/TrinityCore 3.3.5a ServerPktHeader) size the header
    // by the payload: normally 2 bytes of big-endian size + 2 bytes of opcode, but once
    // the size (which counts the opcode) passes 0x7FFF the size field grows to 3 bytes
    // and the first one carries a 0x80 marker:
    //
    //     if (isLargePacket())                          // size > 0x7FFF
    //         header[i++] = 0x80 | (0xFF & (size >> 16));
    //     header[i++] = 0xFF & (size >> 8);
    //     header[i++] = 0xFF & size;
    //
    // The whole header is encrypted (EncryptSend over getHeaderLength()), so consuming
    // 4 bytes of a 5-byte header both mis-frames the packet and leaves the RC4 keystream
    // one byte out of step — the stream never resynchronises. Issue #200: a guild roster
    // large enough to cross 0x7FFF desynced the legacy socket, after which every opcode
    // decoded as garbage and the server dropped the connection 60s later.
    //
    // Vanilla and TBC cores have no such branch and always emit the 4-byte form.
    public const int StructSize = sizeof(ushort) + sizeof(ushort);
    public const int LargeStructSize = StructSize + 1;

    public uint Size;
    public ushort Opcode;

    public static bool IsLargePacket(byte firstSizeByte) => (firstSizeByte & 0x80) != 0;

    public void Read(byte[] buffer) => Read(buffer, large: false);

    public void Read(byte[] buffer, bool large)
    {
        if (large)
        {
            Size = (uint)(((buffer[0] & 0x7F) << 16) | (buffer[1] << 8) | buffer[2]);
            Opcode = BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(3));
        }
        else
        {
            Size = BinaryPrimitives.ReadUInt16BigEndian(buffer);
            Opcode = BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(sizeof(ushort)));
        }
    }

    public void Write(ByteBuffer byteBuffer)
    {
        if (Size > 0x7FFF)
        {
            byteBuffer.WriteUInt8((byte)(0x80 | ((Size >> 16) & 0xFF)));
            byteBuffer.WriteUInt8((byte)((Size >> 8) & 0xFF));
            byteBuffer.WriteUInt8((byte)(Size & 0xFF));
        }
        else
        {
            // Big-endian, to mirror Read.
            byteBuffer.WriteUInt8((byte)((Size >> 8) & 0xFF));
            byteBuffer.WriteUInt8((byte)(Size & 0xFF));
        }
        byteBuffer.WriteUInt16(Opcode);
    }
};

public class LegacyClientPacketHeader
{
    public const int StructSize = sizeof(ushort) + sizeof(uint);
    public ushort Size;
    public uint Opcode;
    public void Read(byte[] buffer)
    {
        Size = BinaryPrimitives.ReadUInt16LittleEndian(buffer);
        Opcode = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(sizeof(ushort)));
    }
    public void Write(ByteBuffer byteBuffer)
    {
        byteBuffer.WriteUInt16(Framework.Util.NetworkUtility.EndianConvert(Size));
        byteBuffer.WriteUInt32(Opcode);
    }
};
