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
        // RideTicket.Read owns the V3_4_3 trailing Unknown925 bit and the byte-align that
        // follows it (see BattleGroundPackets.RideTicket). This used to consume a second
        // one here, which over-read the buffer by a byte and made the final Accepted bit
        // throw IndexOutOfRangeException, crashing the proxy on every proposal reply (#103).
        Ticket.Read(_worldPacket);
        InstanceID = _worldPacket.ReadUInt64();
        ProposalID = _worldPacket.ReadUInt32();
        Accepted = _worldPacket.HasBit();
    }
}
