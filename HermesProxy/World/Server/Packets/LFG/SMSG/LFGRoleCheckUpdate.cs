using HermesProxy.World.Enums;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class LFGRoleCheckMember
{
    public WowGuid128 Guid = WowGuid128.Empty;
    public uint RolesDesired;
    public byte Level;
    public bool RoleCheckComplete;
}

public class LFGRoleCheckUpdate : ServerPacket
{
    public byte PartyIndex;
    public byte RoleCheckStatus;
    public List<uint> JoinSlots = new();
    public int GroupFinderActivityID;
    public List<LFGRoleCheckMember> Members = new();
    public bool IsBeginning;
    public bool IsRequeue;

    public LFGRoleCheckUpdate() : base(Opcode.SMSG_LFG_ROLE_CHECK_UPDATE) { }

    public override void Write()
    {
        _worldPacket.WriteUInt8(PartyIndex);
        _worldPacket.WriteUInt8(RoleCheckStatus);
        _worldPacket.WriteUInt32((uint)JoinSlots.Count);
        _worldPacket.WriteUInt32(0); // BgQueueIDs count
        _worldPacket.WriteInt32(GroupFinderActivityID);
        _worldPacket.WriteUInt32((uint)Members.Count);
        foreach (var slot in JoinSlots)
            _worldPacket.WriteUInt32(slot);
        _worldPacket.WriteBit(IsBeginning);
        _worldPacket.WriteBit(IsRequeue);
        _worldPacket.FlushBits();
        foreach (var m in Members)
        {
            _worldPacket.WritePackedGuid128(m.Guid);
            _worldPacket.WriteUInt8((byte)m.RolesDesired); // WPP V3_4_4 reads ByteE<LfgRoleFlag>
            _worldPacket.WriteUInt8(m.Level);
            _worldPacket.WriteBit(m.RoleCheckComplete);
            _worldPacket.FlushBits();
        }
    }
}
