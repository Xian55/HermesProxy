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
using HermesProxy.World.Enums;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
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
        public long[] AccountTimes;
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

    public class RequestAccountData : ClientPacket
    {
        public RequestAccountData(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PlayerGuid = _worldPacket.ReadPackedGuid128();

            if (ModernVersion.GetAccountDataCount() <= 8)
                DataType = (uint)_worldPacket.ReadBits<uint>(3);
            else
                DataType = (uint)_worldPacket.ReadBits<uint>(4);
        }

        public WowGuid128 PlayerGuid;
        public uint DataType;
    }

    public class UpdateAccountData : ServerPacket, ISpanWritable
    {
        public UpdateAccountData(AccountData data) : base(Opcode.SMSG_UPDATE_ACCOUNT_DATA)
        {
            Player = data.Guid;
            Time = data.Timestamp;
            Size = data.UncompressedSize;
            DataType = data.Type;
            CompressedData = data.CompressedData;
        }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Player);
            _worldPacket.WriteInt64(Time);
            _worldPacket.WriteUInt32(Size);

            if (ModernVersion.GetAccountDataCount() <= 8)
                _worldPacket.WriteBits(DataType, 3);
            else
                _worldPacket.WriteBits(DataType, 4);

            if (CompressedData == null)
                _worldPacket.WriteUInt32(0);
            else
            {
                _worldPacket.WriteInt32(CompressedData.Length);
                _worldPacket.WriteBytes(CompressedData);
            }
        }

        // MaxSize: GUID(18) + long(8) + uint(4) + bits(1) + length(4) + max compressed data
        // Reduced from 16KB to 2KB based on typical usage (235 bytes observed)
        private const int MaxCompressedDataSize = 2048;
        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 17 + MaxCompressedDataSize;

        public int WriteToSpan(Span<byte> buffer)
        {
            if (CompressedData != null && CompressedData.Length > MaxCompressedDataSize)
                return -1;

            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(Player.Low, Player.High);
            writer.WriteInt64(Time);
            writer.WriteUInt32(Size);

            if (ModernVersion.GetAccountDataCount() <= 8)
                writer.WriteBits(DataType, 3);
            else
                writer.WriteBits(DataType, 4);

            if (CompressedData == null)
                writer.WriteUInt32(0);
            else
            {
                writer.WriteInt32(CompressedData.Length);
                writer.WriteBytes(CompressedData);
            }
            return writer.Position;
        }

        public WowGuid128 Player;
        public long Time; // UnixTime
        public uint Size; // decompressed size
        public uint DataType;
        public byte[] CompressedData;
    }

    public class UserClientUpdateAccountData : ClientPacket
    {
        public UserClientUpdateAccountData(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PlayerGuid = _worldPacket.ReadPackedGuid128();
            Time = _worldPacket.ReadInt64();
            Size = _worldPacket.ReadUInt32();

            if (ModernVersion.GetAccountDataCount() <= 8)
                DataType = (uint)_worldPacket.ReadBits<uint>(3);
            else
                DataType = (uint)_worldPacket.ReadBits<uint>(4);

            uint compressedSize = _worldPacket.ReadUInt32();
            if (compressedSize != 0)
            {
                CompressedData = _worldPacket.ReadBytes(compressedSize);
            }
        }

        public WowGuid128 PlayerGuid;
        public long Time; // UnixTime
        public uint Size; // decompressed size
        public uint DataType;
        public byte[] CompressedData;
    }

    class SetAdvancedCombatLogging : ClientPacket
    {
        public SetAdvancedCombatLogging(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Enable = _worldPacket.HasBit();
        }

        public bool Enable;
    }

    class SaveCUFProfiles : ClientPacket
    {
        public SaveCUFProfiles(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Data = _worldPacket.ReadToEnd();
        }

        public byte[] Data;
    }

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

        public byte[] Data;
    }
}
