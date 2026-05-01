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
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

class DBQueryBulk : ClientPacket
{
    public DBQueryBulk(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        TableHash = (DB2Hash)_worldPacket.ReadUInt32();

        uint count = _worldPacket.ReadBits<uint>(13);
        for (uint i = 0; i < count; ++i)
        {
            Queries.Add(_worldPacket.ReadUInt32());
        }
    }

    public DB2Hash TableHash;
    public List<uint> Queries = new();
}

public class DBReply : ServerPacket
{
    public DBReply() : base(Opcode.SMSG_DB_REPLY) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32((uint)TableHash);
        _worldPacket.WriteUInt32(RecordID);
        _worldPacket.WriteUInt32(Timestamp);
        _worldPacket.WriteBits((byte)Status, 3);
        _worldPacket.WriteUInt32(Data.GetSize());
        _worldPacket.WriteBytes(Data.GetData());
    }

    public DB2Hash TableHash;
    public uint Timestamp;
    public uint RecordID;
    public HotfixStatus Status = HotfixStatus.Invalid;

    public ByteBuffer Data = new();
}

class AvailableHotfixes : ServerPacket
{
    public AvailableHotfixes() : base(Opcode.SMSG_AVAILABLE_HOTFIXES) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(VirtualRealmAddress);
        if (IncludeRecords)
        {
            // Filter records if a TableFilter is set. V3_4_3 multi-char rendering only
            // needs ChrCustomizationChoice + ChrCustomizationOption — shipping the full
            // ~600k Item/Spell index produces a ~5 MB packet the client never even logs
            // ("ClientAvailableHotfixes" line missing) suggesting parse-abort. CypherCore's
            // working session shipped 245 distinct PushIDs total. Filtered scope keeps the
            // index in the same order of magnitude.
            int count = TableFilter == null
                ? GameData.Hotfixes.Count
                : System.Linq.Enumerable.Count(GameData.Hotfixes.Values, h => TableFilter.Contains(h.TableHash));
            _worldPacket.WriteInt32(count);
            foreach (var hotfix in GameData.Hotfixes)
            {
                if (TableFilter != null && !TableFilter.Contains(hotfix.Value.TableHash))
                    continue;
                hotfix.Value.WriteAvailable(_worldPacket);
            }
        }
        else
        {
            _worldPacket.WriteInt32(0);
        }
    }

    public uint VirtualRealmAddress;
    public bool IncludeRecords = true;
    public HashSet<DB2Hash>? TableFilter;
}

class HotfixRequest : ClientPacket
{
    public HotfixRequest(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        ClientBuild = _worldPacket.ReadUInt32();
        DataBuild = _worldPacket.ReadUInt32();

        uint hotfixCount = _worldPacket.ReadUInt32();
        for (var i = 0; i < hotfixCount; ++i)
            Hotfixes.Add(_worldPacket.ReadUInt32());
    }

    public uint ClientBuild;
    public uint DataBuild;
    public List<uint> Hotfixes = new();
}

class HotfixConnect : ServerPacket
{
    public HotfixConnect() : base(Opcode.SMSG_HOTFIX_CONNECT) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Hotfixes.Count);
        uint totalDataSize = 0;
        foreach (HotfixRecord hotfix in Hotfixes)
        {
            totalDataSize += hotfix.HotfixContent.GetSize();
            hotfix.WriteHotFixMessageContent(_worldPacket);
        }

        _worldPacket.WriteUInt32(totalDataSize);
        foreach(HotfixRecord hotfix in Hotfixes)
        {
            _worldPacket.WriteBytes(hotfix.HotfixContent);
        }
    }

    public List<HotfixRecord> Hotfixes = new();
}

public class HotFixMessage : ServerPacket
{
    public HotFixMessage() : base(Opcode.SMSG_HOTFIX_MESSAGE) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Hotfixes.Count);
        uint totalDataSize = 0;
        foreach (HotfixRecord hotfix in Hotfixes)
        {
            totalDataSize += hotfix.HotfixContent.GetSize();
            hotfix.WriteHotFixMessageContent(_worldPacket);
        }

        _worldPacket.WriteUInt32(totalDataSize);
        foreach(HotfixRecord hotfix in Hotfixes)
        {
            _worldPacket.WriteBytes(hotfix.HotfixContent);
        }
    }

    public List<HotfixRecord> Hotfixes = new();
}
