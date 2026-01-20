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
using Framework.GameMath;
using Framework.IO;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    public class GameObjUse : ClientPacket
    {
        public GameObjUse(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Guid = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 Guid;
    }

    public class GameObjReportUse : ClientPacket
    {
        public GameObjReportUse(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Guid = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 Guid;
    }

    class GameObjectDespawn : ServerPacket, ISpanWritable
    {
        public GameObjectDespawn() : base(Opcode.SMSG_GAME_OBJECT_DESPAWN) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(ObjectGUID);
        }

        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size;

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(ObjectGUID.Low, ObjectGUID.High);
            return writer.Position;
        }

        public WowGuid128 ObjectGUID;
    }

    class GameObjectResetState : ServerPacket, ISpanWritable
    {
        public GameObjectResetState() : base(Opcode.SMSG_GAME_OBJECT_RESET_STATE) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(ObjectGUID);
        }

        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size;

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(ObjectGUID.Low, ObjectGUID.High);
            return writer.Position;
        }

        public WowGuid128 ObjectGUID;
    }

    class GameObjectCustomAnim : ServerPacket, ISpanWritable
    {
        public GameObjectCustomAnim() : base(Opcode.SMSG_GAME_OBJECT_CUSTOM_ANIM, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(ObjectGUID);
            _worldPacket.WriteUInt32(CustomAnim);
            _worldPacket.WriteBit(PlayAsDespawn);
            _worldPacket.FlushBits();
        }

        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 5; // GUID + uint + 1 byte for bit

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(ObjectGUID.Low, ObjectGUID.High);
            writer.WriteUInt32(CustomAnim);
            writer.WriteBit(PlayAsDespawn);
            writer.FlushBits();
            return writer.Position;
        }

        public WowGuid128 ObjectGUID;
        public uint CustomAnim;
        public bool PlayAsDespawn;
    }

    class FishNotHooked : ServerPacket, ISpanWritable
    {
        public FishNotHooked() : base(Opcode.SMSG_FISH_NOT_HOOKED) { }

        public override void Write() { }

        public int MaxSize => 0;

        public int WriteToSpan(Span<byte> buffer) => 0;
    }

    class FishEscaped : ServerPacket, ISpanWritable
    {
        public FishEscaped() : base(Opcode.SMSG_FISH_ESCAPED) { }

        public override void Write() { }

        public int MaxSize => 0;

        public int WriteToSpan(Span<byte> buffer) => 0;
    }
}
