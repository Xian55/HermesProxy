using HermesProxy.World.Enums;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class DFProposalPlayer
{
    public byte Roles;
    public bool Me;
    public bool SameParty;
    public bool MyParty;
    public bool Responded;
    public bool Accepted;
}

public class DFProposalUpdate : ServerPacket
{
    public RideTicket Ticket = new();
    public ulong InstanceID;
    public uint ProposalID;
    public uint Slot;
    public sbyte State;
    public uint CompletedMask;
    public uint EncounterMask;
    public List<DFProposalPlayer> Players = new();
    public bool ValidCompletedMask;
    public bool ProposalSilent;
    public bool IsRequeue;

    public DFProposalUpdate() : base(Opcode.SMSG_LFG_PROPOSAL_UPDATE) { }

    public override void Write()
    {
        Ticket.Write(_worldPacket);
        _worldPacket.WriteBit(false); // RideTicket trailing Unknown925
        _worldPacket.FlushBits();
        _worldPacket.WriteUInt64(InstanceID);
        _worldPacket.WriteUInt32(ProposalID);
        _worldPacket.WriteUInt32(Slot);
        _worldPacket.WriteInt8(State);
        _worldPacket.WriteUInt32(CompletedMask);
        _worldPacket.WriteUInt32(EncounterMask);
        _worldPacket.WriteUInt32((uint)Players.Count);
        _worldPacket.WriteUInt8(0); // Unused
        _worldPacket.WriteBit(ValidCompletedMask);
        _worldPacket.WriteBit(ProposalSilent);
        _worldPacket.WriteBit(IsRequeue);
        _worldPacket.FlushBits();
        foreach (var player in Players)
        {
            _worldPacket.WriteUInt8(player.Roles);
            _worldPacket.WriteBit(player.Me);
            _worldPacket.WriteBit(player.SameParty);
            _worldPacket.WriteBit(player.MyParty);
            _worldPacket.WriteBit(player.Responded);
            _worldPacket.WriteBit(player.Accepted);
            _worldPacket.FlushBits();
        }
    }
}
