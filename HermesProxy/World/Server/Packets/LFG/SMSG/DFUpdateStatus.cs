using HermesProxy.World.Enums;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class DFUpdateStatus : ServerPacket
{
    public RideTicket Ticket = new();
    public byte SubType;
    public byte Reason;
    public List<uint> Slots = new();
    public byte RequestedRoles;
    public List<WowGuid128> SuspendedPlayers = new();
    public uint QueueMapID;
    public bool IsParty;
    public bool NotifyUI;
    public bool Joined;
    public bool LfgJoined;
    public bool Queued;

    public DFUpdateStatus() : base(Opcode.SMSG_LFG_UPDATE_STATUS) { }

    public override void Write()
    {
        Ticket.Write(_worldPacket);
        _worldPacket.WriteBit(false); // RideTicket trailing Unknown925
        _worldPacket.FlushBits();
        _worldPacket.WriteUInt8(SubType);
        _worldPacket.WriteUInt8(Reason);
        _worldPacket.WriteUInt32((uint)Slots.Count);
        _worldPacket.WriteUInt8(RequestedRoles);
        _worldPacket.WriteUInt32((uint)SuspendedPlayers.Count);
        _worldPacket.WriteUInt32(QueueMapID);
        foreach (var slot in Slots)
            _worldPacket.WriteUInt32(slot);
        foreach (var guid in SuspendedPlayers)
            _worldPacket.WritePackedGuid128(guid);
        _worldPacket.WriteBit(IsParty);
        _worldPacket.WriteBit(NotifyUI);
        _worldPacket.WriteBit(Joined);
        _worldPacket.WriteBit(LfgJoined);
        _worldPacket.WriteBit(Queued);
        _worldPacket.WriteBit(false); // Unused
        _worldPacket.FlushBits();
    }
}
