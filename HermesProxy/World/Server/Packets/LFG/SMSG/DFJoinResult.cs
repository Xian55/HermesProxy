using HermesProxy.World.Enums;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class DFJoinBlackListSlot
{
    public uint Slot;
    public uint Reason;
    public int SubReason1;
    public int SubReason2;
    public uint SoftLock;
}

public class DFJoinBlackList
{
    public WowGuid128? PlayerGuid;
    public List<DFJoinBlackListSlot> Slots = new();
}

public class DFJoinResult : ServerPacket
{
    public RideTicket Ticket = new();
    public byte Result;
    public byte ResultDetail;
    public List<DFJoinBlackList> BlackList = new();

    public DFJoinResult() : base(Opcode.SMSG_LFG_JOIN_RESULT) { }

    public override void Write()
    {
        Ticket.Write(_worldPacket);
        // V3_4_3+ RideTicket trailing "Unknown925" bit. Confirmed against
        // 3.4.3_54261 sniff (World_queue_dungeon_finder_parsed.txt:127006).
        _worldPacket.WriteBit(false);
        _worldPacket.FlushBits();
        _worldPacket.WriteUInt8(Result);
        _worldPacket.WriteUInt8(ResultDetail);
        _worldPacket.WriteUInt32((uint)BlackList.Count);
        _worldPacket.WriteUInt32(0u); // BlackListNames count
        foreach (var entry in BlackList)
        {
            _worldPacket.WriteBit(entry.PlayerGuid != null);
            _worldPacket.WriteUInt32((uint)entry.Slots.Count);
            if (entry.PlayerGuid is { } guid)
                _worldPacket.WritePackedGuid128(guid);
            foreach (var slot in entry.Slots)
            {
                _worldPacket.WriteUInt32(slot.Slot);
                _worldPacket.WriteUInt32(slot.Reason);
                _worldPacket.WriteInt32(slot.SubReason1);
                _worldPacket.WriteInt32(slot.SubReason2);
                _worldPacket.WriteUInt32(slot.SoftLock);
            }
        }
    }
}
