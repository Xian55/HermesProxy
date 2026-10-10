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

using System;
using Framework.Constants;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class AccountDataTimes : ServerPacket, ISpanWritable
{
    public AccountDataTimes() : base(Opcode.SMSG_ACCOUNT_DATA_TIMES) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(PlayerGuid);
        _worldPacket.WriteInt64(ServerTime);
        foreach (var accounttime in AccountTimes)
            _worldPacket.WriteInt64(accounttime);
    }

    // GUID(18) + ServerTime(8) + max 13 account times (8 each) = 130 bytes
    private const int MaxAccountDataCount = 13;
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 8 + MaxAccountDataCount * 8;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(PlayerGuid.Low, PlayerGuid.High);
        writer.WriteInt64(ServerTime);
        foreach (var accounttime in AccountTimes)
            writer.WriteInt64(accounttime);
        return writer.Position;
    }

    public WowGuid128 PlayerGuid;
    public long ServerTime;
    public long[] AccountTimes = Array.Empty<long>();
}

public class ClientCacheVersion : ServerPacket, ISpanWritable
{
    public ClientCacheVersion() : base(Opcode.SMSG_CACHE_VERSION) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(CacheVersion);
    }

    public int MaxSize => 4; // uint

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(CacheVersion);
        return writer.Position;
    }

    public uint CacheVersion = 0;
}

public readonly record struct RequestAccountData(WowGuid128 PlayerGuid, uint DataType);

public sealed class UpdateAccountData : ServerPacket, ISpanWritable
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<UpdateAccountData>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V4_4_2_60895, new PlayerFirstLayout()),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new TimeFirstLayout()));

    private static readonly ServerPacketLayout<UpdateAccountData> Layout = Layouts.ForRunningClient();

    public UpdateAccountData(AccountData data) : base(Opcode.SMSG_UPDATE_ACCOUNT_DATA)
    {
        Player = data.Guid;
        Time = data.Timestamp;
        Size = data.UncompressedSize;
        DataType = data.Type;
        CompressedData = data.CompressedData;
    }

    public override void Write() => Layout.Write(this, _worldPacket);

    // Reduced from 16KB to 2KB based on typical usage (235 bytes observed)
    private const int MaxCompressedDataSize = 2048;

    public int MaxSize => Layout.MaxSize;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (CompressedData != null && CompressedData.Length > MaxCompressedDataSize)
            return -1;
        return Layout.WriteToSpan(this, buffer);
    }

    /// <summary>Up to 3.4.3: Player, Time, Size, then DataType in 3 or 4 bits.</summary>
    internal sealed class PlayerFirstLayout : ServerPacketLayout<UpdateAccountData>
    {
        // GUID(18) + long(8) + uint(4) + bits(1) + length(4) + max compressed data
        public override int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 17 + MaxCompressedDataSize;

        public override void Write(UpdateAccountData packet, WorldPacket data)
        {
            data.WritePackedGuid128(packet.Player);
            data.WriteInt64(packet.Time);
            data.WriteUInt32(packet.Size);

            if (ModernVersion.GetAccountDataCount() <= 8)
                data.WriteBits(packet.DataType, 3);
            else
                data.WriteBits(packet.DataType, 4);

            if (packet.CompressedData == null)
                data.WriteUInt32(0);
            else
            {
                data.WriteInt32(packet.CompressedData.Length);
                data.WriteBytes(packet.CompressedData);
            }
        }

        public override int WriteToSpan(UpdateAccountData packet, Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(packet.Player.Low, packet.Player.High);
            writer.WriteInt64(packet.Time);
            writer.WriteUInt32(packet.Size);

            if (ModernVersion.GetAccountDataCount() <= 8)
                writer.WriteBits(packet.DataType, 3);
            else
                writer.WriteBits(packet.DataType, 4);

            if (packet.CompressedData == null)
                writer.WriteUInt32(0);
            else
            {
                writer.WriteInt32(packet.CompressedData.Length);
                writer.WriteBytes(packet.CompressedData);
            }
            return writer.Position;
        }
    }

    /// <summary>4.4.2: Time, Size, Player, then DataType as an int32 (TrinityCore cata_classic).</summary>
    internal sealed class TimeFirstLayout : ServerPacketLayout<UpdateAccountData>
    {
        // long(8) + uint(4) + GUID(18) + int(4) + length(4) + max compressed data
        public override int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 20 + MaxCompressedDataSize;

        public override void Write(UpdateAccountData packet, WorldPacket data)
        {
            data.WriteInt64(packet.Time);
            data.WriteUInt32(packet.Size);
            data.WritePackedGuid128(packet.Player);
            data.WriteUInt32(packet.DataType);
            data.WriteInt32(packet.CompressedData?.Length ?? 0);
            if (packet.CompressedData != null)
                data.WriteBytes(packet.CompressedData);
        }

        public override int WriteToSpan(UpdateAccountData packet, Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteInt64(packet.Time);
            writer.WriteUInt32(packet.Size);
            writer.WritePackedGuid128(packet.Player.Low, packet.Player.High);
            writer.WriteUInt32(packet.DataType);
            writer.WriteInt32(packet.CompressedData?.Length ?? 0);
            if (packet.CompressedData != null)
                writer.WriteBytes(packet.CompressedData);
            return writer.Position;
        }
    }

    public WowGuid128 Player;
    public long Time; // UnixTime
    public uint Size; // decompressed size
    public uint DataType;
    public byte[] CompressedData = Array.Empty<byte>();
}

public readonly record struct UserClientUpdateAccountData(
    WowGuid128 PlayerGuid,
    long Time,          // UnixTime
    uint Size,          // decompressed size
    uint DataType,
    byte[] CompressedData);

public readonly record struct SaveCUFProfiles(byte[] Data);

public class LoadCUFProfiles : ServerPacket, ISpanWritable
{
    public LoadCUFProfiles() : base(Opcode.SMSG_LOAD_CUF_PROFILES, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteBytes(Data);
    }

    // Cap for CUF profile data - reduced from 2048 to 256 based on typical usage (30 bytes observed)
    private const int MaxDataSize = 256;
    public int MaxSize => MaxDataSize;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (Data == null)
            return 0;

        if (Data.Length > MaxDataSize)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteBytes(Data);
        return writer.Position;
    }

    public byte[] Data = Array.Empty<byte>();
}
