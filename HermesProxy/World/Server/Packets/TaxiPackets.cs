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
    class TaxiNodeStatusPkt : ServerPacket, ISpanWritable
    {
        public TaxiNodeStatusPkt() : base(Opcode.SMSG_TAXI_NODE_STATUS) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(FlightMaster);
            _worldPacket.WriteBits(Status, 2);
            _worldPacket.FlushBits();
        }

        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 1; // GUID + 1 byte for 2 bits

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(FlightMaster.Low, FlightMaster.High);
            writer.WriteBits((uint)Status, 2);
            writer.FlushBits();
            return writer.Position;
        }

        public WowGuid128 FlightMaster;
        public TaxiNodeStatus Status;
    }

    public class ShowTaxiNodes : ServerPacket, ISpanWritable
    {
        public ShowTaxiNodes() : base(Opcode.SMSG_SHOW_TAXI_NODES) { }

        public override void Write()
        {
            _worldPacket.WriteBit(WindowInfo != null);
            _worldPacket.FlushBits();

            List<byte> canLandNodes = new List<byte>(CanLandNodes);
            CleanupNodes(canLandNodes);
            _worldPacket.WriteInt32(canLandNodes.Count);
            List<byte> canUseNodes = new List<byte>(CanUseNodes);
            CleanupNodes(canUseNodes);
            _worldPacket.WriteInt32(canUseNodes.Count);

            if (WindowInfo != null)
            {
                _worldPacket.WritePackedGuid128(WindowInfo.UnitGUID);
                _worldPacket.WriteUInt32(WindowInfo.CurrentNode);
            }

            foreach (var node in canLandNodes)
                _worldPacket.WriteUInt8(node);

            foreach (var node in canUseNodes)
                _worldPacket.WriteUInt8(node);
        }

        // Cap for taxi node bitmasks - enough for all taxi nodes
        private const int MaxNodeBytes = 128;
        // 1 bit(1) + 2 ints(8) + optional WindowInfo (GUID(18)+uint(4)) + 2 node lists
        public int MaxSize => 1 + 8 + PackedGuidHelper.MaxPackedGuid128Size + 4 + MaxNodeBytes * 2;

        public int WriteToSpan(Span<byte> buffer)
        {
            // Calculate cleaned lengths (don't modify originals)
            int landLength = GetCleanedLength(CanLandNodes);
            int useLength = GetCleanedLength(CanUseNodes);

            if (landLength > MaxNodeBytes || useLength > MaxNodeBytes)
                return -1;

            var writer = new SpanPacketWriter(buffer);
            writer.WriteBit(WindowInfo != null);
            writer.FlushBits();

            writer.WriteInt32(landLength);
            writer.WriteInt32(useLength);

            if (WindowInfo != null)
            {
                writer.WritePackedGuid128(WindowInfo.UnitGUID.Low, WindowInfo.UnitGUID.High);
                writer.WriteUInt32(WindowInfo.CurrentNode);
            }

            for (int i = 0; i < landLength; i++)
                writer.WriteUInt8(CanLandNodes[i]);

            for (int i = 0; i < useLength; i++)
                writer.WriteUInt8(CanUseNodes[i]);

            return writer.Position;
        }

        // Get cleaned length without modifying the list
        private static int GetCleanedLength(List<byte> nodes)
        {
            int lastNonZero = -1;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] != 0)
                    lastNonZero = i;
            }
            return lastNonZero + 1;
        }

        // remove extra zeroes after last node
        private void CleanupNodes(List<byte> nodes)
        {
            int lastIndex = -1;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] != 0)
                    lastIndex = i;
            }

            if ((lastIndex + 1) == nodes.Count)
                return;

            if (lastIndex == -1)
            {
                nodes.Clear();
                return;
            }

            nodes.RemoveRange(lastIndex + 1, nodes.Count - (lastIndex + 1));
        }

        public ShowTaxiNodesWindowInfo WindowInfo;
        public List<byte> CanLandNodes = new(); // Nodes known by player
        public List<byte> CanUseNodes = new(); // Nodes available for use - this can temporarily disable a known node
    }

    public class ShowTaxiNodesWindowInfo
    {
        public WowGuid128 UnitGUID;
        public uint CurrentNode;
    }

    class ActivateTaxi : ClientPacket
    {
        public ActivateTaxi(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            FlightMaster = _worldPacket.ReadPackedGuid128();
            Node = _worldPacket.ReadUInt32();
            GroundMountID = _worldPacket.ReadUInt32();
            FlyingMountID = _worldPacket.ReadUInt32();
        }

        public WowGuid128 FlightMaster;
        public uint Node;
        public uint GroundMountID;
        public uint FlyingMountID;
    }

    class NewTaxiPath : ServerPacket, ISpanWritable
    {
        public NewTaxiPath() : base(Opcode.SMSG_NEW_TAXI_PATH) { }

        public override void Write() { }

        public int MaxSize => 0;

        public int WriteToSpan(Span<byte> buffer) => 0;
    }

    class ActivateTaxiReplyPkt : ServerPacket, ISpanWritable
    {
        public ActivateTaxiReplyPkt() : base(Opcode.SMSG_ACTIVATE_TAXI_REPLY) { }

        public override void Write()
        {
            _worldPacket.WriteBits(Reply, 4);
            _worldPacket.FlushBits();
        }

        public int MaxSize => 1; // 1 byte for 4 bits

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteBits((uint)Reply, 4);
            writer.FlushBits();
            return writer.Position;
        }

        public ActivateTaxiReply Reply;
    }
}
