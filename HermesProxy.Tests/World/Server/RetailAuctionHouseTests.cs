using System.Collections.Generic;
using System.Linq;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server;
using HermesProxy.World.Server.Packets;
using HermesProxy.World.Server.Systems;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// The 4.4.2 client's auction house, answered from a legacy server's paged auction list. The
/// legacy pages are fed in by hand; what the engine asks the server for and what it tells the
/// client are both recorded.
/// </summary>
public sealed class RetailAuctionHouseTests
{
    private static readonly WowGuid128 Npc = new(0x155A, 0x2000040000004AC0);
    private static readonly WowGuid128 Me = new(3039, 0x0800040000000000);
    private static readonly WowGuid128 Other = new(3040, 0x0800040000000000);

    private const uint Sword = 900001;      // one to an auction, uncommon, a one-hand sword
    private const uint Ore = 900002;        // stacks to 20, common, a trade good
    private const uint Stranger = 900003;   // nothing known of it

    private sealed class Wire : IRetailAuctionWire
    {
        public readonly List<LegacyAuctionQuery> Queries = [];
        public readonly List<(uint AuctionId, ulong Amount)> Bids = [];
        public readonly List<object> Sent = [];
        public readonly List<string> Notices = [];

        public WowGuid128 Player => Me;

        public AuctionItemFacts Item(uint itemId) => itemId switch
        {
            Sword => new AuctionItemFacts("Sword", MaxStack: 1, ItemLevel: 35, RequiredLevel: 30, Quality: 2,
                ItemClass: 2, ItemSubclass: 7, InventoryType: 13),
            Ore => new AuctionItemFacts("Copper Ore", MaxStack: 20, ItemLevel: 10, RequiredLevel: 0, Quality: 1,
                ItemClass: 7, ItemSubclass: 7, InventoryType: 0),
            _ => AuctionItemFacts.Unknown,
        };

        public void ListItems(in LegacyAuctionQuery query) => Queries.Add(query);
        public void PlaceBid(WowGuid128 auctioneer, uint auctionId, ulong amount) => Bids.Add((auctionId, amount));
        public void Send(AuctionBucketList buckets) => Sent.Add(buckets);
        public void Send(AuctionListItemsResult items) => Sent.Add(items);
        public void Send(AuctionCommodityQuote quote) => Sent.Add(quote);
        public void Send(AuctionCommandResult result) => Sent.Add(result);
        public void Notify(string text) => Notices.Add(text);

        public T Only<T>() => Assert.IsType<T>(Assert.Single(Sent));
    }

    // As the legacy reader leaves one: every price filled, 0 for none.
    private static AuctionItem Lot(uint auctionId, uint itemId, int count, ulong buyout, WowGuid128? owner = null) => new()
    {
        AuctionID = auctionId,
        Item = new ItemInstance { ItemID = itemId },
        Count = count,
        Owner = owner ?? Other,
        MinBid = 1,
        MinIncrement = 0,
        BuyoutPrice = buyout,
        BidAmount = 0,
        Bidder = WowGuid128.Empty,
        DurationLeft = 1000,
    };

    private static AuctionBrowseQuery Query(uint offset = 0, string name = "", AuctionHouseFilterMask filters = 0,
        List<ClassFilter>? classes = null)
        => new(Npc, offset, MinLevel: 0, MaxLevel: 0, filters, name, classes ?? []);

    private static AuctionBucketKey Key(uint itemId) => new()
    {
        ItemID = itemId,
        BattlePetSpeciesID = null,
        SuffixItemNameDescriptionID = null,
    };

    [Fact]
    public void OnLegacyList_WithNoSearchWaiting_LeavesThePageToTheCaller()
    {
        var wire = new Wire();
        var house = new RetailAuctionHouse(wire);

        Assert.False(house.OnLegacyList([Lot(1, Sword, 1, 500)], totalCount: 1, desiredDelay: 300));
        Assert.Empty(wire.Sent);
    }

    [Fact]
    public void Browse_GroupsThePageIntoOneRowPerItem()
    {
        var wire = new Wire();
        var house = new RetailAuctionHouse(wire);

        house.Browse(Query(name: "o"));
        LegacyAuctionQuery asked = Assert.Single(wire.Queries);
        Assert.Equal(new LegacyAuctionQuery(Npc, 0, "o", 0, 0, -1, -1, -1, -1, false, false), asked);

        Assert.True(house.OnLegacyList(
        [
            Lot(1, Sword, 1, 500),
            Lot(2, Ore, 20, 2000),
            Lot(3, Ore, 5, 400),
            Lot(4, Sword, 1, 300, owner: Me),
        ], totalCount: 4, desiredDelay: 300));

        var result = wire.Only<AuctionBucketList>();
        Assert.Equal(AuctionHouseBrowseMode.Search, result.BrowseMode);
        Assert.False(result.HasMoreResults);
        Assert.Equal(300u, result.DesiredDelay);
        Assert.Equal(2, result.Buckets.Count);

        AuctionBucketInfo swords = result.Buckets[0];
        Assert.Equal(Sword, swords.Key.ItemID);
        Assert.Equal(35, swords.Key.ItemLevel);
        Assert.Null(swords.Key.SuffixItemNameDescriptionID);
        Assert.Equal(2, swords.TotalQuantity);
        Assert.Equal(30, swords.RequiredLevel);
        Assert.Equal(300ul, swords.MinPrice);
        Assert.True(swords.ContainsOwnerItem);

        // A commodity's price is per piece: 400 for 5 beats 2000 for 20.
        AuctionBucketInfo ore = result.Buckets[1];
        Assert.Equal(Ore, ore.Key.ItemID);
        Assert.Equal(25, ore.TotalQuantity);
        Assert.Equal(80ul, ore.MinPrice);
        Assert.False(ore.ContainsOwnerItem);
    }

    [Fact]
    public void Browse_ReadsItsPageBudget_ThenTheClientAsksForMore()
    {
        var wire = new Wire();
        var house = new RetailAuctionHouse(wire);
        List<AuctionItem> page = [.. Enumerable.Range(1, 50).Select(i => Lot((uint)i, Sword, 1, 500))];

        house.Browse(Query());
        for (int i = 0; i < RetailAuctionHouse.BrowsePages; i++)
            Assert.True(house.OnLegacyList(page, totalCount: 260, desiredDelay: 0));

        Assert.Equal([0u, 50u, 100u, 150u], wire.Queries.Select(q => q.Offset));
        var first = wire.Only<AuctionBucketList>();
        Assert.True(first.HasMoreResults);
        Assert.Equal(200, Assert.Single(first.Buckets).TotalQuantity);

        // The client's offset counts rows; the legacy list is read on from where it was left.
        wire.Sent.Clear();
        house.Browse(Query(offset: 1));
        Assert.Equal(200u, wire.Queries[^1].Offset);

        // A page of nothing new does not end the request: the budget is spent only on news.
        Assert.True(house.OnLegacyList(page, totalCount: 260, desiredDelay: 0));
        Assert.Empty(wire.Sent);
        Assert.Equal(250u, wire.Queries[^1].Offset);

        Assert.True(house.OnLegacyList([Lot(99, Ore, 10, 1000)], totalCount: 251, desiredDelay: 0));
        var second = wire.Only<AuctionBucketList>();
        Assert.False(second.HasMoreResults);
        Assert.Equal(Ore, Assert.Single(second.Buckets).Key.ItemID);

        // Read to its end, there is nothing more to ask the server for.
        wire.Sent.Clear();
        int asked = wire.Queries.Count;
        house.Browse(Query(offset: 2));
        Assert.Equal(asked, wire.Queries.Count);
        Assert.Empty(wire.Only<AuctionBucketList>().Buckets);
    }

    [Fact]
    public void Browse_StopsAtTheHardLimit_WhenNoPageHasAnythingNew()
    {
        var wire = new Wire();
        var house = new RetailAuctionHouse(wire);
        List<AuctionItem> page = [Lot(1, Sword, 1, 500)];

        house.Browse(Query());
        for (int i = 0; i < RetailAuctionHouse.BrowsePages; i++)
            Assert.True(house.OnLegacyList(page, totalCount: 100000, desiredDelay: 0));
        Assert.Single(wire.Only<AuctionBucketList>().Buckets);

        wire.Sent.Clear();
        wire.Queries.Clear();
        house.Browse(Query(offset: 1));
        for (int i = 0; i < RetailAuctionHouse.MaxPages; i++)
            Assert.True(house.OnLegacyList(page, totalCount: 100000, desiredDelay: 0));

        // The list is not at its end, so the client may ask again and the reading goes on.
        Assert.Equal(RetailAuctionHouse.MaxPages, wire.Queries.Count);
        var result = wire.Only<AuctionBucketList>();
        Assert.Empty(result.Buckets);
        Assert.True(result.HasMoreResults);
        Assert.False(house.OnLegacyList(page, totalCount: 100000, desiredDelay: 0));
    }

    [Fact]
    public void Browse_AsksTheServerForWhatItCanFilter_AndNarrowsTheRestItself()
    {
        var wire = new Wire();
        var house = new RetailAuctionHouse(wire);

        // One class, one subclass, one inventory type, one quality: all the server's to filter.
        house.Browse(Query(
            filters: AuctionHouseFilterMask.UncommonQuality | AuctionHouseFilterMask.UsableOnly | AuctionHouseFilterMask.ExactMatch,
            name: "Sword",
            classes: [new ClassFilter(2, [new SubClassFilter(7, 1u << 13)])]));
        Assert.Equal(new LegacyAuctionQuery(Npc, 0, "Sword", 0, 0, 13, 2, 7, 2, true, true), wire.Queries[^1]);

        // Two qualities and two subclasses are more than a legacy search takes.
        house.Browse(Query(
            filters: AuctionHouseFilterMask.RareQuality | AuctionHouseFilterMask.EpicQuality,
            classes: [new ClassFilter(2, [new SubClassFilter(7, uint.MaxValue), new SubClassFilter(8, uint.MaxValue)])]));
        Assert.Equal(new LegacyAuctionQuery(Npc, 0, "", 0, 0, -1, 2, -1, -1, false, false), wire.Queries[^1]);

        // The sword is uncommon and the ore no weapon; the unknown item cannot be ruled out.
        Assert.True(house.OnLegacyList(
            [Lot(1, Sword, 1, 500), Lot(2, Ore, 20, 2000), Lot(3, Stranger, 1, 700)], totalCount: 3, desiredDelay: 0));
        Assert.Equal(Stranger, Assert.Single(wire.Only<AuctionBucketList>().Buckets).Key.ItemID);
    }

    [Fact]
    public void ListItems_SearchesByName_AndKeepsTheItemsAuctionsCheapestFirst()
    {
        var wire = new Wire();
        var house = new RetailAuctionHouse(wire);
        AuctionBucketKey key = Key(Sword);

        house.ListItems(Npc, key, keyOnItems: false);
        Assert.Equal(new LegacyAuctionQuery(Npc, 0, "Sword", 0, 0, -1, -1, -1, -1, false, true), Assert.Single(wire.Queries));

        AuctionItem bidOn = Lot(3, Sword, 1, 900);
        bidOn.Bidder = Other;
        bidOn.BidAmount = 120;
        bidOn.MinIncrement = 6;

        // A name search also finds other items of that name.
        Assert.True(house.OnLegacyList(
            [Lot(1, Sword, 1, 500), Lot(9, Stranger, 1, 100), bidOn, Lot(2, Sword, 1, 300, owner: Me)],
            totalCount: 4, desiredDelay: 300));

        var result = wire.Only<AuctionListItemsResult>();
        Assert.Equal(AuctionHouseListType.Items, result.ListType);
        Assert.Same(key, result.BucketKey);
        Assert.False(result.HasMoreResults);
        Assert.Equal(3, result.TotalItemsCount);
        Assert.Equal([2u, 1u, 3u], result.Items.Select(i => i.AuctionID));

        AuctionItem mine = result.Items[0];
        Assert.False(mine.CensorServerSideInfo);
        Assert.True(mine.CensorBidInfo);
        Assert.Equal(300ul, mine.BuyoutPrice);
        Assert.Equal(1ul, mine.MinBid);
        Assert.Null(mine.MinIncrement);
        Assert.Null(mine.BidAmount);
        Assert.Null(mine.UnitPrice);
        Assert.Null(mine.AuctionBucketKey);
        Assert.Equal(Sword, mine.Item.ItemID);

        Assert.True(result.Items[1].CensorServerSideInfo);

        AuctionItem withBid = result.Items[2];
        Assert.False(withBid.CensorBidInfo);
        Assert.Equal(Other, withBid.Bidder);
        Assert.Equal(120ul, withBid.BidAmount);
        Assert.Equal(6ul, withBid.MinIncrement);
    }

    [Fact]
    public void ListItems_ReadsEveryPage()
    {
        var wire = new Wire();
        var house = new RetailAuctionHouse(wire);

        house.ListItems(Npc, Key(Sword), keyOnItems: true);
        Assert.True(house.OnLegacyList([Lot(1, Sword, 1, 500)], totalCount: 2, desiredDelay: 0));
        Assert.Empty(wire.Sent);
        Assert.Equal(1u, wire.Queries[^1].Offset);

        Assert.True(house.OnLegacyList([Lot(2, Sword, 1, 400)], totalCount: 2, desiredDelay: 0));
        var result = wire.Only<AuctionListItemsResult>();
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, i => Assert.Equal(Sword, i.AuctionBucketKey.ItemID));
    }

    [Fact]
    public void ListItems_OfACommodity_IsAUnitPriceListWithoutTheItem()
    {
        var wire = new Wire();
        var house = new RetailAuctionHouse(wire);

        house.ListItems(Npc, Key(Ore), keyOnItems: false);
        Assert.True(house.OnLegacyList(
            [Lot(1, Ore, 20, 2000), Lot(2, Ore, 5, 400), Lot(3, Ore, 10, buyout: 0), Lot(4, Ore, 3, 100)],
            totalCount: 4, desiredDelay: 0));

        var result = wire.Only<AuctionListItemsResult>();
        Assert.Equal(AuctionHouseListType.Commodities, result.ListType);
        Assert.Equal(28, result.TotalItemsCount);

        // 100 for 3 is 34 a piece, rounded up; the stack with no buyout cannot be bought by the piece.
        Assert.Equal([(4u, 34ul, 3), (2u, 80ul, 5), (1u, 100ul, 20)],
            result.Items.Select(i => (i.AuctionID, i.UnitPrice!.Value, i.Count)));
        Assert.All(result.Items, i =>
        {
            Assert.Null(i.Item);
            Assert.Null(i.BuyoutPrice);
            Assert.Null(i.MinBid);
            Assert.True(i.CensorBidInfo);
        });
    }

    [Fact]
    public void ListItems_OfAnItemWithNoName_IsAnsweredEmptyAtOnce()
    {
        var wire = new Wire();
        var house = new RetailAuctionHouse(wire);

        house.ListItems(Npc, Key(Stranger), keyOnItems: false);

        Assert.Empty(wire.Queries);
        Assert.Empty(wire.Only<AuctionListItemsResult>().Items);
    }

    [Fact]
    public void ListBucketsByKeys_IsAnsweredEmpty()
    {
        var wire = new Wire();
        new RetailAuctionHouse(wire).ListBucketsByKeys();

        var result = wire.Only<AuctionBucketList>();
        Assert.Equal(AuctionHouseBrowseMode.SpecificKeys, result.BrowseMode);
        Assert.Empty(result.Buckets);
    }

    private static List<AuctionItem> OreOnSale() =>
    [
        Lot(1, Ore, 20, 2000),
        Lot(2, Ore, 5, 400),
        Lot(3, Ore, 5, 50, owner: Me),      // one's own are not for sale to oneself
        Lot(4, Ore, 5, buyout: 0),
        Lot(5, Ore, 20, 3000),
    ];

    [Fact]
    public void CommodityQuote_PricesWholeStacksCheapestFirst_AndTheConfirmBuysThem()
    {
        var wire = new Wire();
        var house = new RetailAuctionHouse(wire);

        house.GetCommodityQuote(Npc, Ore, quantity: 25);
        Assert.Equal("Copper Ore", Assert.Single(wire.Queries).Name);
        Assert.True(house.OnLegacyList(OreOnSale(), totalCount: 5, desiredDelay: 300));

        var quote = wire.Only<AuctionCommodityQuote>();
        Assert.Equal(Ore, quote.ItemID);
        Assert.Equal(2400ul, quote.TotalPrice);
        Assert.Equal(25u, quote.Quantity);
        Assert.Equal(30000, quote.QuoteDuration);
        Assert.Empty(wire.Notices);

        wire.Sent.Clear();
        house.ConfirmCommodityPurchase(Npc, Ore);
        Assert.Equal([(2u, 400ul), (1u, 2000ul)], wire.Bids);
        Assert.Empty(wire.Sent);

        // A quote is good for one purchase.
        house.ConfirmCommodityPurchase(Npc, Ore);
        Assert.Equal(2, wire.Bids.Count);
        Assert.Equal(AuctionHouseError.ItemNotFound, wire.Only<AuctionCommandResult>().ErrorCode);
    }

    // Seen in play: one piece of a stack of two, quoted at the stack's price, and the client
    // showed "no longer available". It takes only the total its own list adds up to.
    [Theory]
    [InlineData(8u, "5 or 25")]
    [InlineData(3u, "buy 5 of")]
    [InlineData(26u, "25 or 45")]
    public void CommodityQuote_ForPartOfAStack_HasNoPriceAndSaysWhatCanBeBought(uint quantity, string expected)
    {
        var wire = new Wire();
        var house = new RetailAuctionHouse(wire);

        house.GetCommodityQuote(Npc, Ore, quantity);
        Assert.True(house.OnLegacyList(OreOnSale(), totalCount: 5, desiredDelay: 0));

        Assert.Null(wire.Only<AuctionCommodityQuote>().TotalPrice);
        Assert.Contains(expected, Assert.Single(wire.Notices));

        wire.Sent.Clear();
        house.ConfirmCommodityPurchase(Npc, Ore);
        Assert.Empty(wire.Bids);
    }

    [Fact]
    public void CommodityQuote_AtOnePrice_PassesOverAStackTooLargeForASmallerOne()
    {
        var wire = new Wire();
        var house = new RetailAuctionHouse(wire);

        house.GetCommodityQuote(Npc, Ore, quantity: 8);
        Assert.True(house.OnLegacyList(
            [Lot(1, Ore, 20, 2000), Lot(2, Ore, 5, 500), Lot(3, Ore, 3, 300)], totalCount: 3, desiredDelay: 0));

        Assert.Equal(800ul, wire.Only<AuctionCommodityQuote>().TotalPrice);
        house.ConfirmCommodityPurchase(Npc, Ore);
        Assert.Equal([(2u, 500ul), (3u, 300ul)], wire.Bids);
    }

    [Fact]
    public void CommodityQuote_TotalsWhatTheListShows_WhenAStackDoesNotDivideEvenly()
    {
        var wire = new Wire();
        var house = new RetailAuctionHouse(wire);

        // 100 for 3 is listed at 34 a piece: the client expects 102, the buyout costs 100.
        house.GetCommodityQuote(Npc, Ore, quantity: 3);
        Assert.True(house.OnLegacyList([Lot(1, Ore, 3, 100)], totalCount: 1, desiredDelay: 0));

        Assert.Equal(102ul, wire.Only<AuctionCommodityQuote>().TotalPrice);
        house.ConfirmCommodityPurchase(Npc, Ore);
        Assert.Equal([(1u, 100ul)], wire.Bids);
    }

    [Fact]
    public void CommodityQuote_ForMoreThanIsOnSale_HasNoPrice()
    {
        var wire = new Wire();
        var house = new RetailAuctionHouse(wire);

        house.GetCommodityQuote(Npc, Ore, quantity: 30);
        Assert.True(house.OnLegacyList([Lot(1, Ore, 20, 2000)], totalCount: 1, desiredDelay: 0));

        var quote = wire.Only<AuctionCommodityQuote>();
        Assert.Null(quote.TotalPrice);
        Assert.Null(quote.Quantity);
        Assert.Null(quote.QuoteDuration);
        Assert.Empty(wire.Notices);     // the client's own "no longer available" fits

        wire.Sent.Clear();
        house.ConfirmCommodityPurchase(Npc, Ore);
        Assert.Empty(wire.Bids);
        Assert.IsType<AuctionCommandResult>(Assert.Single(wire.Sent));
    }

    [Fact]
    public void CancelCommodityPurchase_DropsTheQuote()
    {
        var wire = new Wire();
        var house = new RetailAuctionHouse(wire);

        house.GetCommodityQuote(Npc, Ore, quantity: 5);
        Assert.True(house.OnLegacyList([Lot(1, Ore, 20, 2000)], totalCount: 1, desiredDelay: 0));
        house.CancelCommodityPurchase();
        house.ConfirmCommodityPurchase(Npc, Ore);

        Assert.Empty(wire.Bids);
    }
}
