namespace HermesProxy.World.Server.Packets;

public class DFProposalResponsePkt : ClientPacket
{
    public RideTicket Ticket = new();
    public ulong InstanceID;
    public uint ProposalID;
    public bool Accepted;

    public DFProposalResponsePkt(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Ticket.Read(_worldPacket);
        _worldPacket.HasBit(); // RideTicket trailing Unknown925
        _worldPacket.ResetBitReader();
        InstanceID = _worldPacket.ReadUInt64();
        ProposalID = _worldPacket.ReadUInt32();
        Accepted = _worldPacket.HasBit();
    }
}
