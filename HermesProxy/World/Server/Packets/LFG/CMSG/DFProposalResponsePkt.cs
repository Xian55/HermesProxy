namespace HermesProxy.World.Server.Packets;

public readonly record struct DFProposalResponsePkt(
    RideTicket Ticket, ulong InstanceID, uint ProposalID, bool Accepted);
