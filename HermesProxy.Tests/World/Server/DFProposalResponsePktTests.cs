using HermesProxy.World;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// Regression coverage for issue #103: DFProposalResponsePkt consumed the RideTicket
/// trailing Unknown925 bit a second time after RideTicket.Read had already taken it,
/// over-reading the buffer by a byte so the final Accepted bit threw
/// IndexOutOfRangeException and crashed the proxy on every dungeon proposal reply.
/// </summary>
public class DFProposalResponsePktTests
{
    private const ulong InstanceId = 0x1122334455667788ul;
    private const uint ProposalId = 0xDEADBEEFu;

    /// <summary>
    /// Builds the wire bytes a client sends for CMSG_DF_PROPOSAL_RESPONSE, matching
    /// RideTicket.Read's layout for the build the test suite is pinned to.
    /// </summary>
    private static WorldPacket BuildPacket(bool accepted)
    {
        var payload = new WorldPacket(1u);
        payload.WritePackedGuid128(WowGuid128.Empty); // RideTicket.RequesterGuid
        payload.WriteUInt32(7u);                      // RideTicket.Id
        payload.WriteUInt32(2u);                      // RideTicket.Type
        payload.WriteInt64(1234567890L);              // RideTicket.Time
        payload.WriteUInt64(InstanceId);
        payload.WriteUInt32(ProposalId);
        payload.WriteBit(accepted);
        payload.FlushBits();

        byte[] body = payload.GetData();
        var framed = new byte[body.Length + 2];       // read ctor consumes a u16 opcode first
        body.CopyTo(framed, 2);
        return new WorldPacket(framed);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Read_ParsesWholePacketWithoutOverrunning(bool accepted)
    {
        using var packet = new DFProposalResponsePkt(BuildPacket(accepted));

        packet.Read();

        Assert.Equal(InstanceId, packet.InstanceID);
        Assert.Equal(ProposalId, packet.ProposalID);
        Assert.Equal(accepted, packet.Accepted);
        Assert.Equal(7u, packet.Ticket.Id);
    }
}
