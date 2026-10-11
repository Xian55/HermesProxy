using System;
using System.Collections.Generic;
using HermesProxy.Enums;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>One page request to a legacy server's <c>CMSG_AUCTION_LIST_ITEMS</c>; -1 leaves a filter open.</summary>
internal record struct LegacyAuctionQuery(
    WowGuid128 Auctioneer, uint Offset, string Name, byte MinLevel, byte MaxLevel,
    int InventoryType, int ItemClass, int ItemSubclass, int Quality, bool OnlyUsable, bool ExactMatch);

/// <summary>What <see cref="RetailAuctionHouse"/> sends, and for whom.</summary>
internal interface IRetailAuctionWire
{
    WowGuid128 Player { get; }
    AuctionItemFacts Item(uint itemId);
    void ListItems(in LegacyAuctionQuery query);
    void PlaceBid(WowGuid128 auctioneer, uint auctionId, ulong amount);
    void Send(AuctionBucketList buckets);
    void Send(AuctionListItemsResult items);
    void Send(AuctionCommodityQuote quote);
    void Send(AuctionCommandResult result);

    /// <summary>A line in the player's chat, for what the auction house window has no way to say.</summary>
    void Notify(string text);
}

/// <summary>The session's legacy server and modern client.</summary>
internal sealed class SessionRetailAuctionWire(GlobalSessionData session) : IRetailAuctionWire
{
    public WowGuid128 Player => session.GameState.CurrentPlayerGuid;

    public AuctionItemFacts Item(uint itemId) => AuctionItemFacts.Of(itemId);

    public void ListItems(in LegacyAuctionQuery query)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_AUCTION_LIST_ITEMS);
        packet.WriteGuid(query.Auctioneer.To64());
        packet.WriteUInt32(query.Offset);
        packet.WriteCString(query.Name);
        packet.WriteUInt8(query.MinLevel);
        packet.WriteUInt8(query.MaxLevel);
        packet.WriteInt32(query.InventoryType);
        packet.WriteInt32(query.ItemClass);
        packet.WriteInt32(query.ItemSubclass);
        packet.WriteInt32(query.Quality);
        packet.WriteBool(query.OnlyUsable);

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            // getAll. Set, a server answers with every auction at once and then refuses the next
            // such request for minutes; the pages are asked for one by one instead.
            packet.WriteBool(false);
            if (WorldClient.IsCataLegacy)
                packet.WriteBool(query.ExactMatch);
            packet.WriteUInt8(0); // sorts
        }

        session.ToServer.Send(packet);
    }

    public void PlaceBid(WowGuid128 auctioneer, uint auctionId, ulong amount)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_AUCTION_PLACE_BID);
        packet.WriteGuid(auctioneer.To64());
        packet.WriteUInt32(auctionId);
        if (WorldClient.IsCataLegacy)
            packet.WriteUInt64(amount);
        else
            packet.WriteUInt32((uint)amount);
        session.ToServer.Send(packet);
    }

    public void Send(AuctionBucketList buckets) => session.ToClient.Send(new AuctionListBucketsResult(buckets));
    public void Send(AuctionListItemsResult items) => session.ToClient.Send(items);
    public void Send(AuctionCommodityQuote quote) => session.ToClient.Send(new AuctionGetCommodityQuoteResult(quote));
    public void Send(AuctionCommandResult result) => session.ToClient.Send(result);
    public void Notify(string text) => session.SendHermesTextMessage(text);
}

/// <summary>
/// Answers the 4.4.2 client's auction house from a legacy server's auction list.
/// </summary>
/// <remarks>
/// <para>
/// That client has the retail auction house. Its Buy tab lists <i>buckets</i>, one row per item
/// with a quantity and a lowest price, and opening a row asks for that item's auctions. Stackable
/// items are <i>commodities</i>: listed by unit price, bought by quantity through a price quote.
/// A legacy server knows none of this. It has one request, a filtered search that returns 50
/// auctions a page.
/// </para>
/// <para>
/// So each client request becomes a run of legacy pages, read one after another, and the answer is
/// built here once enough have arrived:
/// <list type="bullet">
/// <item>a browse reads <see cref="BrowsePages"/> pages, groups them by item and sends the buckets
/// the client has not been given yet; the client asks again for more;</item>
/// <item>an item's auctions are a name search, filtered to that item, read to the end;</item>
/// <item>a commodity quote is the same search, priced over the cheapest stacks. A legacy auction
/// cannot be bought in part, and the client turns down a quote whose total is not the one its own
/// list adds up to, so a quantity is quoted only when whole stacks make it up exactly. For any
/// other the player is told in chat which quantities can be had.</item>
/// </list>
/// </para>
/// <para>
/// The legacy list result carries nothing that names its request, so one search runs at a time and
/// a new request replaces the one in flight. The client sends its requests one at a time too: it
/// holds the next until the last was answered, which is why an unanswered one froze the window.
/// </para>
/// </remarks>
internal sealed class RetailAuctionHouse(IRetailAuctionWire wire)
{
    /// <summary>Legacy pages read for one browse request before the buckets so far are sent.</summary>
    internal const int BrowsePages = 4;

    /// <summary>
    /// Legacy pages one request may read at most: an item's auctions, or a browse whose pages hold
    /// nothing new. 1000 auctions; what lies beyond is left out rather than awaited.
    /// </summary>
    internal const int MaxPages = 20;

    internal static readonly TimeSpan QuoteDuration = TimeSpan.FromSeconds(30);

    private Search? _search;

    // The browse the client is paging through: its filters, how far into the legacy list it has
    // read, and the items already sent as a bucket.
    private LegacyAuctionQuery _browseQuery;
    private AuctionHouseFilterMask _browseFilters;
    private List<ClassFilter> _browseClasses = [];
    private bool _browseExhausted = true;
    private readonly HashSet<uint> _browsedItems = [];

    private CommodityQuote? _quote;

    public void Browse(in AuctionBrowseQuery request)
    {
        if (request.Offset == 0)
        {
            _browseQuery = ToLegacyQuery(request);
            _browseFilters = request.Filters;
            _browseClasses = request.ClassFilters;
            _browseExhausted = false;
            _browsedItems.Clear();
        }
        else if (_browseExhausted)
        {
            wire.Send(new AuctionBucketList());
            return;
        }

        Start(new Search(SearchKind.Browse, _browseQuery));
    }

    /// <param name="key">The bucket key the answer names; the request's own.</param>
    /// <param name="keyOnItems">Whether each auction repeats its key, as a search by item id does.</param>
    public void ListItems(WowGuid128 auctioneer, AuctionBucketKey key, bool keyOnItems)
    {
        if (!TryNameSearch(auctioneer, key.ItemID, out LegacyAuctionQuery query))
        {
            wire.Send(BuildItemList(key, keyOnItems, [], desiredDelay: 0));
            return;
        }

        Start(new Search(SearchKind.Items, query) { Key = key, KeyOnItems = keyOnItems });
    }

    /// <summary>
    /// The buckets of a list of keys the client kept, such as favourites. None are kept for it
    /// here, so the answer is empty; it still has to be given.
    /// </summary>
    public void ListBucketsByKeys()
        => wire.Send(new AuctionBucketList { BrowseMode = AuctionHouseBrowseMode.SpecificKeys });

    public void GetCommodityQuote(WowGuid128 auctioneer, uint itemId, uint quantity)
    {
        _quote = null;
        if (quantity == 0 || !TryNameSearch(auctioneer, itemId, out LegacyAuctionQuery query))
        {
            wire.Send(new AuctionCommodityQuote { ItemID = itemId });
            return;
        }

        Start(new Search(SearchKind.Quote, query)
        {
            Key = RetailAuctionShape.KeyFor(itemId, wire.Item(itemId)),
            Quantity = quantity,
        });
    }

    public void ConfirmCommodityPurchase(WowGuid128 auctioneer, uint itemId)
    {
        CommodityQuote? quote = _quote;
        _quote = null;

        if (quote is null || quote.ItemID != itemId)
        {
            wire.Send(new AuctionCommandResult
            {
                Command = AuctionHouseAction.Bid,
                ErrorCode = AuctionHouseError.ItemNotFound,
            });
            return;
        }

        // Each buyout is answered by the server with its own command result, which reaches the
        // client the usual way.
        foreach ((uint auctionId, ulong buyout) in quote.Stacks)
            wire.PlaceBid(auctioneer, auctionId, buyout);
    }

    public void CancelCommodityPurchase() => _quote = null;

    /// <summary>
    /// A page of the legacy auction list. False when no retail-style search is waiting for one,
    /// and the page is the answer to a client that asked for it directly.
    /// </summary>
    public bool OnLegacyList(List<AuctionItem> page, int totalCount, uint desiredDelay)
    {
        Search? search = _search;
        if (search is null)
            return false;

        search.Pages++;
        search.Query = search.Query with { Offset = search.Query.Offset + (uint)page.Count };

        foreach (AuctionItem auction in page)
        {
            if (auction.Item is null || !Wants(search, auction))
                continue;

            search.Found.Add(auction);
            search.FoundNewItem |= !_browsedItems.Contains(auction.Item.ItemID);
        }

        bool exhausted = page.Count == 0 || search.Query.Offset >= (uint)Math.Max(totalCount, 0);
        if (!exhausted && WantsAnotherPage(search))
        {
            wire.ListItems(search.Query);
            return true;
        }

        _search = null;
        switch (search.Kind)
        {
            case SearchKind.Browse:
                FinishBrowse(search, exhausted, desiredDelay);
                break;
            case SearchKind.Items:
                wire.Send(BuildItemList(search.Key, search.KeyOnItems, search.Found, desiredDelay));
                break;
            case SearchKind.Quote:
                FinishQuote(search, desiredDelay);
                break;
        }

        return true;
    }

    private void Start(Search search)
    {
        _search = search;
        wire.ListItems(search.Query);
    }

    private bool Wants(Search search, AuctionItem auction)
    {
        uint itemId = auction.Item.ItemID;
        if (search.Kind != SearchKind.Browse)
            return itemId == search.Key.ItemID;

        AuctionItemFacts facts = wire.Item(itemId);
        return RetailAuctionShape.MatchesQuality(facts, _browseFilters)
            && RetailAuctionShape.MatchesClasses(facts, _browseClasses)
            && (!_browseFilters.HasFlag(AuctionHouseFilterMask.ExactMatch)
                || RetailAuctionShape.HasName(facts, search.Query.Name));
    }

    private static bool WantsAnotherPage(Search search)
    {
        if (search.Pages >= MaxPages)
            return false;

        // A browse stops at its page budget, unless every auction so far was of an item the
        // client already has a bucket for: an answer with nothing new in it shows as the end.
        return search.Kind != SearchKind.Browse || search.Pages < BrowsePages || !search.FoundNewItem;
    }

    private void FinishBrowse(Search search, bool exhausted, uint desiredDelay)
    {
        _browseQuery = search.Query;
        _browseExhausted = exhausted;

        var result = new AuctionBucketList { DesiredDelay = desiredDelay, HasMoreResults = !exhausted };
        var buckets = new Dictionary<uint, AuctionBucketInfo>();

        foreach (AuctionItem auction in search.Found)
        {
            uint itemId = auction.Item.ItemID;
            if (_browsedItems.Contains(itemId))
                continue;

            AuctionItemFacts facts = wire.Item(itemId);
            if (!buckets.TryGetValue(itemId, out AuctionBucketInfo? bucket))
            {
                bucket = new AuctionBucketInfo
                {
                    Key = RetailAuctionShape.KeyFor(itemId, facts),
                    RequiredLevel = facts.RequiredLevel,
                    MinPrice = ulong.MaxValue,
                };
                buckets.Add(itemId, bucket);
                result.Buckets.Add(bucket);
            }

            ulong price = RetailAuctionShape.Price(auction);
            if (facts.IsCommodity)
                price = RetailAuctionShape.UnitPrice(price, auction.Count);

            bucket.TotalQuantity += auction.Count;
            bucket.MinPrice = Math.Min(bucket.MinPrice, price);
            bucket.ContainsOwnerItem |= auction.Owner == wire.Player;
        }

        foreach (uint itemId in buckets.Keys)
            _browsedItems.Add(itemId);

        wire.Send(result);
    }

    private AuctionListItemsResult BuildItemList(AuctionBucketKey key, bool keyOnItems, List<AuctionItem> found, uint desiredDelay)
    {
        AuctionItemFacts facts = wire.Item(key.ItemID);
        bool commodity = facts.IsCommodity;
        var result = new AuctionListItemsResult
        {
            DesiredDelay = desiredDelay,
            ListType = commodity ? AuctionHouseListType.Commodities : AuctionHouseListType.Items,
            BucketKey = key,
            HasMoreResults = false,
        };

        WowGuid128 player = wire.Player;
        foreach (AuctionItem auction in SortedByPrice(found, commodity))
        {
            // A commodity row is a unit price to buy at; a stack with no buyout has none.
            if (commodity && !(auction.BuyoutPrice > 0))
                continue;

            result.Items.Add(commodity
                ? RetailAuctionShape.ForCommodityList(auction, facts, player, keyOnItems)
                : RetailAuctionShape.ForItemList(auction, facts, player, keyOnItems));
            result.TotalItemsCount += auction.Count;
        }

        return result;
    }

    /// <remarks>
    /// The client prices the quantity itself, cheapest pieces first, and shows "no longer
    /// available" for a quote with any other total (seen in play: one piece of a stack of two,
    /// quoted at the stack's price). So the stacks are taken in the client's order, a stack too
    /// large for what is still wanted is passed over only for a smaller one at the same price, and
    /// the total is the one the list shows: unit price times pieces, which rounding can put a
    /// little above what the buyouts then cost.
    /// </remarks>
    private void FinishQuote(Search search, uint desiredDelay)
    {
        var result = new AuctionCommodityQuote { ItemID = search.Key.ItemID, DesiredDelay = desiredDelay };

        WowGuid128 player = wire.Player;
        var onSale = new List<(ulong UnitPrice, AuctionItem Auction)>();
        foreach (AuctionItem auction in search.Found)
        {
            if (auction.BuyoutPrice > 0 && auction.Owner != player && auction.Count > 0)
                onSale.Add((RetailAuctionShape.UnitPrice(auction.BuyoutPrice.Value, auction.Count), auction));
        }
        onSale.Sort(static (a, b) => a.UnitPrice != b.UnitPrice
            ? a.UnitPrice.CompareTo(b.UnitPrice)
            : b.Auction.Count.CompareTo(a.Auction.Count));

        var stacks = new List<(uint AuctionId, ulong Buyout)>();
        ulong shownTotal = 0;
        long wanted = search.Quantity;
        long taken = 0;
        long? nextLarger = null;        // the least that can be bought above what was asked for
        ulong? passedOverAt = null;

        foreach ((ulong unitPrice, AuctionItem auction) in onSale)
        {
            if (taken == wanted || (passedOverAt.HasValue && unitPrice != passedOverAt))
                break;

            if (auction.Count > wanted - taken)
            {
                passedOverAt = unitPrice;
                nextLarger = Math.Min(nextLarger ?? long.MaxValue, taken + auction.Count);
                continue;
            }

            stacks.Add((auction.AuctionID, auction.BuyoutPrice!.Value));
            shownTotal += unitPrice * (ulong)auction.Count;
            taken += auction.Count;
        }

        if (taken == wanted)
        {
            _quote = new CommodityQuote(search.Key.ItemID, stacks);
            result.TotalPrice = shownTotal;
            result.Quantity = search.Quantity;
            result.QuoteDuration = (long)QuoteDuration.TotalMilliseconds;
        }
        else if (nextLarger.HasValue)
        {
            // Without a price the client says the item is no longer available, which is not it.
            wire.Notify(taken > 0
                ? $"Stacks are sold whole on this server: you can buy {taken} or {nextLarger} of this item right now."
                : $"Stacks are sold whole on this server: you can buy {nextLarger} of this item right now.");
        }

        wire.Send(result);
    }

    /// <summary>Cheapest first, which is the order both lists open in; the sort is stable.</summary>
    private static List<AuctionItem> SortedByPrice(List<AuctionItem> found, bool commodity)
    {
        var sorted = new List<(ulong Price, int Index)>(found.Count);
        for (int i = 0; i < found.Count; i++)
        {
            ulong price = RetailAuctionShape.Price(found[i]);
            sorted.Add((commodity ? RetailAuctionShape.UnitPrice(price, found[i].Count) : price, i));
        }
        sorted.Sort();

        var result = new List<AuctionItem>(found.Count);
        foreach ((_, int index) in sorted)
            result.Add(found[index]);
        return result;
    }

    private bool TryNameSearch(WowGuid128 auctioneer, uint itemId, out LegacyAuctionQuery query)
    {
        string name = wire.Item(itemId).Name;
        query = new LegacyAuctionQuery(auctioneer, Offset: 0, name, MinLevel: 0, MaxLevel: 0,
            InventoryType: -1, ItemClass: -1, ItemSubclass: -1, Quality: -1, OnlyUsable: false, ExactMatch: true);
        return name.Length > 0;
    }

    private static LegacyAuctionQuery ToLegacyQuery(in AuctionBrowseQuery request)
    {
        // A legacy search takes one class, one subclass and one inventory type. Anything wider is
        // asked for unfiltered and narrowed here, page by page.
        int itemClass = -1, itemSubclass = -1, inventoryType = -1;
        if (request.ClassFilters.Count == 1)
        {
            ClassFilter classFilter = request.ClassFilters[0];
            itemClass = classFilter.ItemClass;
            if (classFilter.SubClassFilters.Count == 1)
            {
                SubClassFilter subClass = classFilter.SubClassFilters[0];
                itemSubclass = subClass.ItemSubclass;
                if (BitOperations.IsPow2(subClass.InvTypeMask))
                    inventoryType = BitOperations.TrailingZeroCount(subClass.InvTypeMask);
            }
        }

        // Servers disagree on whether the quality is a minimum or an exact match. One quality
        // reads the same either way; any other choice is narrowed here.
        uint qualities = RetailAuctionShape.QualityMask(request.Filters);
        int quality = BitOperations.IsPow2(qualities) ? BitOperations.TrailingZeroCount(qualities) : -1;

        return new LegacyAuctionQuery(request.Auctioneer, Offset: 0, request.Name, request.MinLevel, request.MaxLevel,
            inventoryType, itemClass, itemSubclass, quality,
            OnlyUsable: request.Filters.HasFlag(AuctionHouseFilterMask.UsableOnly),
            ExactMatch: request.Filters.HasFlag(AuctionHouseFilterMask.ExactMatch));
    }

    private enum SearchKind
    {
        Browse,
        Items,
        Quote,
    }

    private sealed class Search(SearchKind kind, LegacyAuctionQuery query)
    {
        public SearchKind Kind { get; } = kind;
        public LegacyAuctionQuery Query = query;
        public readonly List<AuctionItem> Found = [];
        public int Pages;
        public bool FoundNewItem;

        public AuctionBucketKey Key = null!;
        public bool KeyOnItems;
        public uint Quantity;
    }

    /// <summary>The stacks a commodity quote priced, bought out when the client confirms.</summary>
    private sealed record CommodityQuote(uint ItemID, List<(uint AuctionId, ulong Buyout)> Stacks);
}
