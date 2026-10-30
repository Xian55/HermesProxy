using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Client;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's auction-house CMSGs.
/// </summary>
/// <remarks>
/// <see cref="HandleAuctionListItems"/> reads through a ranged codec pair rather than a branch: the
/// V3_4_3 and pre-V3_4_3 layouts disagree on field order, on field widths, and on whether a
/// tainted-addon block is present at all. See AuctionCodecs.cs.
/// <para>
/// <see cref="HandleAuctionSellItem"/> carries the awkward part of this domain. Servers before
/// 3.2.2a have no quantity field — they auction the whole item — so selling part of a stack means
/// splitting it to a temporary bag slot first and auctioning that. <see cref="PartialStackAuctionPost"/>
/// sequences the splits on the server's inventory updates. It used to sleep the dispatch thread,
/// which froze the client's other packets and would deadlock once one thread owns the session.
/// </para>
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

    [HandlesCmsg(Opcode.CMSG_AUCTION_LIST_BIDDED_ITEMS)]
    public static void HandleAuctionListBidderItems(in AuctionListBidderItems auction, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_AUCTION_LIST_BIDDED_ITEMS);
        packet.WriteGuid(auction.Auctioneer.To64());
        packet.WriteUInt32(auction.Offset);
        packet.WriteInt32(auction.AuctionItemIDs.Count);
        foreach (var itemId in auction.AuctionItemIDs)
            packet.WriteUInt32(itemId);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_AUCTION_LIST_ITEMS)]
    public static void HandleAuctionListItems(in AuctionListItems auction, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_AUCTION_LIST_ITEMS);
        packet.WriteGuid(auction.Auctioneer.To64());
        packet.WriteUInt32(auction.Offset);
        packet.WriteCString(auction.Name);
        packet.WriteUInt8(auction.MinLevel);
        packet.WriteUInt8(auction.MaxLevel);

        if (auction.ClassFilters.Count > 0)
        {
            if (auction.ClassFilters[0].SubClassFilters.Count == 1)
            {
                packet.WriteInt32(ModernToLegacyInventorySlotType(auction.ClassFilters[0].SubClassFilters[0].InvTypeMask));
                packet.WriteInt32(auction.ClassFilters[0].ItemClass);
                packet.WriteInt32(auction.ClassFilters[0].SubClassFilters[0].ItemSubclass);
            }
            else
            {
                packet.WriteInt32(-1); // inventorySlotId
                packet.WriteInt32(auction.ClassFilters[0].ItemClass);
                packet.WriteInt32(-1); // auctionSubCategory
            }
        }
        else
        {
            packet.WriteInt32(-1); // inventorySlotId
            packet.WriteInt32(-1); // auctionMainCategory
            packet.WriteInt32(-1); // auctionSubCategory
        }

        packet.WriteInt32(auction.Quality);
        packet.WriteBool(auction.OnlyUsable);

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            // getAll. Set, a server ignores the filters and sends its whole house in one packet.
            packet.WriteBool(false);
            // TrinityCore 4.3.4 reads one more byte here. Without it the sort count was read from
            // the first sort and the search was dropped. 3.3.5a has no exact match to ask for.
            if (WorldClient.IsCataLegacy)
                packet.WriteBool(auction.ExactMatch);
            packet.WriteUInt8((byte)auction.Sorts.Count);

            foreach (var sort in auction.Sorts)
            {
                packet.WriteUInt8(sort.Type);
                packet.WriteUInt8(sort.Direction);
            }
        }

        ctx.GameState.AuctionExactMatchName = auction.ExactMatch ? auction.Name : null;
        ctx.SendPacketToServer(packet);
    }

    /// <remarks>
    /// The modern client can search several inventory types at once; legacy takes one, so this
    /// picks the lowest set bit. The pre-conversion handler carried this twice — once as a local
    /// function inside HandleAuctionListItems and once as an instance method that local shadowed
    /// and nothing else called. Only the reachable one survives.
    /// </remarks>
    static int ModernToLegacyInventorySlotType(uint modernInventoryFlag)
    {
        if (modernInventoryFlag == uint.MaxValue)
            return -1;

        for (int i = 0; i < 32; i++)
        {
            if ((modernInventoryFlag & (1 << i)) > 0)
            {
                return i;
            }
        }

        return -1;
    }

    // ---- the 4.4.2 client's retail-style auction house; RetailAuctionHouse answers these ----

    [HandlesCmsg(Opcode.CMSG_AUCTION_BROWSE_QUERY)]
    public static void HandleAuctionBrowseQuery(in AuctionBrowseQuery query, in SessionContext ctx)
        => ctx.GameState.RetailAuctions.Browse(query);

    [HandlesCmsg(Opcode.CMSG_AUCTION_LIST_ITEMS_BY_BUCKET_KEY)]
    public static void HandleAuctionListItemsByBucketKey(in AuctionListItemsByBucketKey request, in SessionContext ctx)
        => ctx.GameState.RetailAuctions.ListItems(request.Auctioneer, request.BucketKey, keyOnItems: false);

    [HandlesCmsg(Opcode.CMSG_AUCTION_LIST_ITEMS_BY_ITEM_ID)]
    public static void HandleAuctionListItemsByItemID(in AuctionListItemsByItemID request, in SessionContext ctx)
    {
        // The answer names the item and no row of it, as TrinityCore's does.
        var key = new AuctionBucketKey
        {
            ItemID = request.ItemID,
            BattlePetSpeciesID = null,
            SuffixItemNameDescriptionID = null,
        };
        ctx.GameState.RetailAuctions.ListItems(request.Auctioneer, key, keyOnItems: true);
    }

    [HandlesCmsg(Opcode.CMSG_AUCTION_LIST_BUCKETS_BY_BUCKET_KEYS)]
    public static void HandleAuctionListBucketsByBucketKeys(in AuctionListBucketsByBucketKeys request, in SessionContext ctx)
        => ctx.GameState.RetailAuctions.ListBucketsByKeys();

    [HandlesCmsg(Opcode.CMSG_AUCTION_GET_COMMODITY_QUOTE)]
    public static void HandleAuctionGetCommodityQuote(in AuctionGetCommodityQuote request, in SessionContext ctx)
        => ctx.GameState.RetailAuctions.GetCommodityQuote(request.Auctioneer, request.ItemID, request.Quantity);

    [HandlesCmsg(Opcode.CMSG_AUCTION_CONFIRM_COMMODITIES_PURCHASE)]
    public static void HandleAuctionConfirmCommoditiesPurchase(in AuctionConfirmCommoditiesPurchase request, in SessionContext ctx)
        => ctx.GameState.RetailAuctions.ConfirmCommodityPurchase(request.Auctioneer, request.ItemID);

    [HandlesCmsg(Opcode.CMSG_AUCTION_CANCEL_COMMODITIES_PURCHASE)]
    public static void HandleAuctionCancelCommoditiesPurchase(in AuctionCancelCommoditiesPurchase request, in SessionContext ctx)
        => ctx.GameState.RetailAuctions.CancelCommodityPurchase();

    /// <remarks>
    /// A legacy server has no price per piece and no auction larger than a stack. Each stack the
    /// client offers becomes an auction of its own, bid and buyout both at its price, so that a
    /// commodity search finds every one of them buyable.
    /// </remarks>
    [HandlesCmsg(Opcode.CMSG_AUCTION_SELL_COMMODITY)]
    public static void HandleAuctionSellCommodity(in AuctionSellCommodity sale, in SessionContext ctx)
    {
        foreach (AuctionItemForSale item in sale.Items)
        {
            ulong price = sale.UnitPrice * item.UseCount;
            HandleAuctionSellItem(
                new AuctionSellItem(sale.Auctioneer, price, price, sale.RunTime, TaintedBy: null, [item]), ctx);
        }
    }

    [HandlesCmsg(Opcode.CMSG_AUCTION_SELL_ITEM)]
    public static void HandleAuctionSellItem(in AuctionSellItem auction, in SessionContext ctx)
    {
        uint expireTime = auction.ExpireTime;

        // A legacy server drops a sale with no opening bid without a word (TrinityCore
        // HandleAuctionSellItem: "if (!bid || !etime) return"). The retail-style window lets a
        // sale name only a buyout, which then is the bid as well.
        ulong minBid = auction.MinBid != 0 ? auction.MinBid : auction.BuyoutPrice;

        // auction durations were increased in tbc
        // server ignores packet if you send wrong duration
        if (LegacyVersion.ExpansionVersion <= 1 &&
            ModernVersion.ExpansionVersion > 1)
        {
            switch (expireTime)
            {
                case 1 * 12 * 60: // 720
                {
                    expireTime = 1 * 2 * 60; // 120
                    break;
                }
                case 2 * 12 * 60: // 1440
                {
                    expireTime = 4 * 2 * 60; // 480
                    break;
                }
                case 4 * 12 * 60: // 2880
                {
                    expireTime = 12 * 2 * 60; // 1440
                    break;
                }
            }
        }
        else if (LegacyVersion.ExpansionVersion > 1 &&
                 ModernVersion.ExpansionVersion <= 1)
        {
            switch (expireTime)
            {
                case 1 * 2 * 60:
                {
                    expireTime = 1 * 12 * 60;
                    break;
                }
                case 4 * 2 * 60:
                {
                    expireTime = 2 * 12 * 60;
                    break;
                }
                case 12 * 2 * 60:
                {
                    expireTime = 4 * 12 * 60;
                    break;
                }
            }
        }

        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_2_2a_10505))
        {
            // Pre-3.2.2a servers have no quantity field — they auction the entire item.
            // A partial stack is split to a temp slot and that is auctioned instead; the
            // original item keeps the remainder, which is what the modern client expects.
            // Each split waits for the server's inventory update, so the post runs as a
            // sequence of outbox holds rather than sleeps on this thread.
            var session = ctx.GetSession();
            new PartialStackAuctionPost(
                session.ToServer,
                new SessionAuctionInventory(session),
                auction.Auctioneer, minBid, auction.BuyoutPrice, expireTime,
                [.. auction.Items]).Start();
        }
        else
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_AUCTION_SELL_ITEM);
            packet.WriteGuid(auction.Auctioneer.To64());
            packet.WriteInt32(auction.Items.Count);
            foreach (var item in auction.Items)
            {
                packet.WriteGuid(item.Guid.To64());
                packet.WriteUInt32(item.UseCount);
            }
            // TrinityCore 4.3.4 HandleAuctionSellItem: bid and buyout are uint64.
            if (WorldClient.IsCataLegacy)
            {
                packet.WriteUInt64(minBid);
                packet.WriteUInt64(auction.BuyoutPrice);
            }
            else
            {
                packet.WriteUInt32((uint)minBid);
                packet.WriteUInt32((uint)auction.BuyoutPrice);
            }
            packet.WriteUInt32(expireTime);
            ctx.SendPacketToServer(packet);
        }
    }

    [HandlesCmsg(Opcode.CMSG_AUCTION_REMOVE_ITEM)]
    public static void HandleAuctionRemoveItem(in AuctionRemoveItem auction, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_AUCTION_REMOVE_ITEM);
        packet.WriteGuid(auction.Auctioneer.To64());
        packet.WriteUInt32(auction.AuctionID);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_AUCTION_PLACE_BID)]
    public static void HandleAuctionPlaceBId(in AuctionPlaceBid auction, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_AUCTION_PLACE_BID);
        packet.WriteGuid(auction.Auctioneer.To64());
        packet.WriteUInt32(auction.AuctionID);
        // TrinityCore 4.3.4 HandleAuctionPlaceBid: the price is a uint64.
        if (WorldClient.IsCataLegacy)
            packet.WriteUInt64(auction.BidAmount);
        else
            packet.WriteUInt32((uint)auction.BidAmount);
        ctx.SendPacketToServer(packet);
    }
}
