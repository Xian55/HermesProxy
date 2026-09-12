using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's auction-house CMSGs.
/// </summary>
/// <remarks>
/// Only the two that carry no nested helper type convert in this slice. The other five —
/// <c>AUCTION_LIST_ITEMS</c>, <c>SELL_ITEM</c>, <c>LIST_BIDDED_ITEMS</c>, <c>REMOVE_ITEM</c> and
/// <c>PLACE_BID</c> — read <c>AddOnInfo</c>, <c>ClassFilter</c>/<c>SubClassFilter</c> or
/// <c>AuctionItemForSale</c>, and <c>AUCTION_LIST_ITEMS</c> additionally branches on the client
/// build inside one <c>Read</c>. That branch is the ranged-codec case
/// <c>docs/version-shape-dispatch.md</c> describes, and splitting it is the whole job rather than a
/// detail of this one, so the auction packets convert together in their own slice.
/// </remarks>
public static class AuctionSystem
{
    [HandlesCmsg(Opcode.CMSG_AUCTION_HELLO_REQUEST)]
    public static void HandleAuctionHelloRequest(in InteractWithNPC interact, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_AUCTION_HELLO);
        packet.WriteGuid(interact.CreatureGUID.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_AUCTION_LIST_OWNED_ITEMS)]
    public static void HandleAuctionListOwnerItems(in AuctionListOwnerItems auction, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_AUCTION_LIST_OWNED_ITEMS);
        packet.WriteGuid(auction.Auctioneer.To64());
        packet.WriteUInt32(auction.Offset);
        ctx.SendPacketToServer(packet);
    }
}
