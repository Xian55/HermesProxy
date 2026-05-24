using Framework.Constants;
using HermesProxy.World.Enums;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class LFGLockInfoData
{
    public uint Slot;
    public uint LockStatus;
    public uint SubReason1;
    public uint SubReason2;
}

public class LFGBlackListEntry
{
    public WowGuid128 PlayerGuid = WowGuid128.Empty;
    public List<LFGLockInfoData> Locks = new();
}

public class LFGPartyInfo : ServerPacket
{
    public List<LFGBlackListEntry> Players = new();

    public LFGPartyInfo() : base(Opcode.SMSG_LFG_PARTY_INFO, ConnectionType.Instance) { }

    public override void Write()
    {
        // V3_4_3 modern wire: Int32 BlackListCount + per-entry LFGBlackList
        // struct (bit hasPlayerGuid + uint32 lockCount + optional packedGuid128
        // + per-lock 5×uint32). Confirmed against WPP V3_4_4 ReadLFGBlackList.
        _worldPacket.WriteUInt32((uint)Players.Count);
        foreach (var p in Players)
        {
            _worldPacket.WriteBit(true); // hasPlayerGuid
            _worldPacket.WriteUInt32((uint)p.Locks.Count);
            _worldPacket.WritePackedGuid128(p.PlayerGuid);
            foreach (var l in p.Locks)
            {
                _worldPacket.WriteUInt32(l.Slot);
                _worldPacket.WriteUInt32(l.LockStatus);
                _worldPacket.WriteInt32((int)l.SubReason1);
                _worldPacket.WriteInt32((int)l.SubReason2);
                _worldPacket.WriteUInt32(0); // SoftLock
            }
        }
    }
}
