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


using Framework.Constants;
using Framework.GameMath;
using Framework.IO;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    class UpdateInstanceOwnership : ServerPacket, ISpanWritable
    {
        public UpdateInstanceOwnership() : base(Opcode.SMSG_UPDATE_INSTANCE_OWNERSHIP) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(IOwnInstance);
        }

        public int MaxSize => 4; // uint

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32(IOwnInstance);
            return writer.Position;
        }

        public uint IOwnInstance;
    }

    class UpdateLastInstance : ServerPacket, ISpanWritable
    {
        public UpdateLastInstance() : base(Opcode.SMSG_UPDATE_LAST_INSTANCE) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(MapID);
        }

        public int MaxSize => 4; // uint

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32(MapID);
            return writer.Position;
        }

        public uint MapID;
    }

    class InstanceReset : ServerPacket, ISpanWritable
    {
        public InstanceReset() : base(Opcode.SMSG_INSTANCE_RESET) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(MapID);
        }

        public int MaxSize => 4; // uint

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32(MapID);
            return writer.Position;
        }

        public uint MapID;
    }

    class InstanceResetFailed : ServerPacket, ISpanWritable
    {
        public InstanceResetFailed() : base(Opcode.SMSG_INSTANCE_RESET_FAILED) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(MapID);
            _worldPacket.WriteBits(ResetFailedReason, 2);
            _worldPacket.FlushBits();
        }

        public int MaxSize => 5; // uint + 1 byte for 2 bits

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32(MapID);
            writer.WriteBits((uint)ResetFailedReason, 2);
            writer.FlushBits();
            return writer.Position;
        }

        public uint MapID;
        public ResetFailedReason ResetFailedReason;
    }

    class ResetFailedNotify : ServerPacket, ISpanWritable
    {
        public ResetFailedNotify() : base(Opcode.SMSG_RESET_FAILED_NOTIFY) { }

        public override void Write() { }

        public int MaxSize => 0;

        public int WriteToSpan(Span<byte> buffer) => 0;
    }

    class RaidInstanceInfo : ServerPacket, ISpanWritable
    {
        public RaidInstanceInfo() : base(Opcode.SMSG_RAID_INSTANCE_INFO) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(LockList.Count);

            foreach (InstanceLock lockInfos in LockList)
                lockInfos.Write(_worldPacket);
        }

        // Cap for raid lockouts - players typically have few
        private const int MaxLocks = 16;
        // Each lock: 2 uints(8) + ulong(8) + int(4) + uint(4) + 2 bits(1) = 25 bytes
        private const int LockSize = 25;
        // count(4) + locks
        public int MaxSize => 4 + MaxLocks * LockSize;

        public int WriteToSpan(Span<byte> buffer)
        {
            if (LockList.Count > MaxLocks)
                return -1;

            var writer = new SpanPacketWriter(buffer);
            writer.WriteInt32(LockList.Count);

            foreach (InstanceLock lockInfos in LockList)
            {
                writer.WriteUInt32(lockInfos.MapID);
                writer.WriteUInt32((uint)lockInfos.DifficultyID);
                writer.WriteUInt64(lockInfos.InstanceID);
                writer.WriteInt32(lockInfos.TimeRemaining);
                writer.WriteUInt32(lockInfos.CompletedMask);
                writer.WriteBit(lockInfos.Locked);
                writer.WriteBit(lockInfos.Extended);
                writer.FlushBits();
            }
            return writer.Position;
        }

        public List<InstanceLock> LockList = new();
    }

    public class InstanceLock
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt32(MapID);
            data.WriteUInt32((uint)DifficultyID);
            data.WriteUInt64(InstanceID);
            data.WriteInt32(TimeRemaining);
            data.WriteUInt32(CompletedMask);

            data.WriteBit(Locked);
            data.WriteBit(Extended);
            data.FlushBits();
        }

        public uint MapID;
        public DifficultyModern DifficultyID;
        public ulong InstanceID;
        public int TimeRemaining;
        public uint CompletedMask = 1;

        public bool Locked = true;
        public bool Extended;
    }

    class InstanceSaveCreated : ServerPacket, ISpanWritable
    {
        public InstanceSaveCreated() : base(Opcode.SMSG_INSTANCE_SAVE_CREATED) { }

        public override void Write()
        {
            _worldPacket.WriteBit(Gm);
            _worldPacket.FlushBits();
        }

        public int MaxSize => 1; // 1 byte for bit

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteBit(Gm);
            writer.FlushBits();
            return writer.Position;
        }

        public bool Gm;
    }

    class RaidGroupOnly : ServerPacket, ISpanWritable
    {
        public RaidGroupOnly() : base(Opcode.SMSG_RAID_GROUP_ONLY) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Delay);
            _worldPacket.WriteUInt32((uint)Reason);
        }

        public int MaxSize => 8; // int + uint

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteInt32(Delay);
            writer.WriteUInt32((uint)Reason);
            return writer.Position;
        }

        public int Delay;
        public RaidGroupReason Reason;
    }

    class RaidInstanceMessage : ServerPacket, ISpanWritable
    {
        public RaidInstanceMessage() : base(Opcode.SMSG_RAID_INSTANCE_MESSAGE) { }

        public override void Write()
        {
            _worldPacket.WriteUInt8((byte)Type);
            _worldPacket.WriteUInt32(MapID);
            _worldPacket.WriteUInt32((uint)DifficultyID);
            _worldPacket.WriteBit(Locked);
            _worldPacket.WriteBit(Extended);
            _worldPacket.FlushBits();
        }

        public int MaxSize => 10; // byte + 2 uint + 1 byte for bits

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt8((byte)Type);
            writer.WriteUInt32(MapID);
            writer.WriteUInt32((uint)DifficultyID);
            writer.WriteBit(Locked);
            writer.WriteBit(Extended);
            writer.FlushBits();
            return writer.Position;
        }

        public InstanceResetWarningType Type;
        public uint MapID;
        public DifficultyModern DifficultyID;
        public bool Locked;
        public bool Extended;
    }
}
