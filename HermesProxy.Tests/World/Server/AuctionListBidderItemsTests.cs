using System.Linq;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// Pins the auction-id list on <c>CMSG_AUCTION_LIST_BIDDED_ITEMS</c>.
/// </summary>
/// <remarks>
/// The reader used to assign through the list indexer — <c>AuctionItemIDs[i] = …</c> — into a list
/// that <c>Read</c> never grows. <see cref="System.Collections.Generic.List{T}"/>'s setter requires
/// an index below <c>Count</c>, so every request carrying at least one bid threw
/// <c>ArgumentOutOfRangeException</c> out of the handler. The count byte is 7 bits, which is why it
/// survived: a client with no active bids sends zero, the loop never runs, and the empty case is
/// the one anybody testing an auction house hits first.
/// </remarks>
public class AuctionListBidderItemsTests
{
    private static AuctionListBidderItems Read(uint[] auctionIds)
    {
        using var w = new WorldPacket(1u);
        w.WritePackedGuid128(new WowGuid128(0xABCDUL, 0x1234UL));
        w.WriteUInt32(42);
        w.WriteBits((uint)auctionIds.Length, 7);
        w.FlushBits();
        foreach (uint id in auctionIds)
            w.WriteUInt32(id);

        byte[] payload = w.GetData();
        byte[] framed = new byte[payload.Length + 2];
        payload.CopyTo(framed, 2);

        var packet = new AuctionListBidderItems(new WorldPacket(framed));
        packet.Read();
        return packet;
    }

    [Fact]
    public void Read_WithNoBids_YieldsAnEmptyList()
    {
        var packet = Read([]);
        Assert.Equal(42u, packet.Offset);
        Assert.Empty(packet.AuctionItemIDs);
    }

    [Fact]
    public void Read_WithBids_YieldsThemInOrder()
    {
        var packet = Read([7u, 9u, 4294967295u]);
        Assert.Equal(42u, packet.Offset);
        Assert.Equal([7u, 9u, 4294967295u], packet.AuctionItemIDs.ToArray());
    }

    /// The count field is 7 bits, so 127 is the largest the client can ask for.
    [Fact]
    public void Read_AtTheSevenBitCountLimit_ReadsEveryId()
    {
        uint[] ids = Enumerable.Range(1, 127).Select(i => (uint)i).ToArray();
        var packet = Read(ids);
        Assert.Equal(ids, packet.AuctionItemIDs.ToArray());
    }
}
