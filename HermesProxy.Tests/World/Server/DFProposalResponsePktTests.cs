using Framework.IO;
using HermesProxy.Enums;
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
/// <remarks>
/// Now exercised through <c>DFProposalResponsePktCodec</c> and <c>RideTicket.Read(ref
/// SpanPacketReader)</c>, the span twin added when the LFG opcodes converted. The wire is built to
/// match whichever build the suite is bootstrapped to, so the Unknown925 bit is present exactly
/// when the reader expects it — hard-coding either shape would make this pass for the wrong reason
/// under the other bootstrap.
/// </remarks>
public class DFProposalResponsePktTests
{
    private const ulong InstanceId = 0x1122334455667788ul;
    private const uint ProposalId = 0xDEADBEEFu;

    private static bool SendsUnknown925 => ModernVersion.Build == ClientVersionBuild.V3_4_3_54261;

    /// <summary>
    /// Builds the wire bytes a client sends for CMSG_DF_PROPOSAL_RESPONSE, matching
    /// RideTicket.Read's layout for the build the test suite is pinned to.
    /// </summary>
    private static byte[] BuildPacket(bool accepted)
    {
        var payload = new WorldPacket(1u);
        payload.WritePackedGuid128(WowGuid128.Empty); // RideTicket.RequesterGuid
        payload.WriteUInt32(7u);                      // RideTicket.Id
        payload.WriteUInt32(2u);                      // RideTicket.Type
        payload.WriteInt64(1234567890L);              // RideTicket.Time
        if (SendsUnknown925)
        {
            payload.WriteBit(false);                  // RideTicket.Unknown925
            payload.FlushBits();                      // and the byte-align that follows it
        }
        payload.WriteUInt64(InstanceId);
        payload.WriteUInt32(ProposalId);
        payload.WriteBit(accepted);
        payload.FlushBits();

        byte[] body = payload.GetData();
        var framed = new byte[body.Length + 2];       // the reader is built past the u16 opcode
        body.CopyTo(framed, 2);
        return framed;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Read_ParsesWholePacketWithoutOverrunning(bool accepted)
    {
        var r = new SpanPacketReader(new WorldPacket(BuildPacket(accepted)).GetRemainingSpan());

        DFProposalResponsePktCodec.Read(ref r, out var packet);

        Assert.Equal(InstanceId, packet.InstanceID);
        Assert.Equal(ProposalId, packet.ProposalID);
        Assert.Equal(accepted, packet.Accepted);
        Assert.Equal(7u, packet.Ticket.Id);
        Assert.Equal(RideType.Lfg, packet.Ticket.Type);
        Assert.Equal(1234567890L, packet.Ticket.Time);
    }

    /// <summary>
    /// The bug was an over-read, so the reader finishing exactly at the end is the assertion that
    /// would have caught it — the field values alone were still plausible right up until the throw.
    /// </summary>
    [Fact]
    public void Read_ConsumesExactlyTheWholePacket()
    {
        var r = new SpanPacketReader(new WorldPacket(BuildPacket(true)).GetRemainingSpan());

        DFProposalResponsePktCodec.Read(ref r, out _);

        Assert.Equal(0, r.Remaining);
    }
}
