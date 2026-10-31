/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using System;
using Framework.Constants;
using HermesProxy.Enums;
using Framework.GameMath;
using Framework.IO;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

class AuctionHelloResponse : ServerPacket, ISpanWritable
{
    public AuctionHelloResponse() : base(Opcode.SMSG_AUCTION_HELLO_RESPONSE) { }

    // The 3.4.3 client reads two delivery delays and then the auction house id, before the
    // OpenForBusiness bit (without the delays the bit lands past the payload and the client shows
    // "auction house closed", #85). Its house id is what it matches outbid notices against: sent
    // first (as TrinityCore 3.4.3 does), the id landed in a delay and the client held house 0, so
    // it ignored every outbid and kept showing the player as the high bidder.
    private static bool HasDeliveryDelays => ForceV343ForTests ?? ModernVersion.IsWotLKClassicOrLater;
    // The test process never runs as V3_4_3; tests set this to reach the 3.4.3 layout.
    internal static bool? ForceV343ForTests;

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Guid);
        if (HasDeliveryDelays)
        {
            _worldPacket.WriteUInt32(PurchasedItemDeliveryDelay);
            _worldPacket.WriteUInt32(CancelledItemDeliveryDelay);
        }
        _worldPacket.WriteUInt32(AuctionHouseID);
        _worldPacket.WriteBit(OpenForBusiness);
        _worldPacket.FlushBits();
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 5 + 8; // GUID + uint + bit + 2 V3_4_3 uints

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Guid.Low, Guid.High);
        if (HasDeliveryDelays)
        {
            writer.WriteUInt32(PurchasedItemDeliveryDelay);
            writer.WriteUInt32(CancelledItemDeliveryDelay);
        }
        writer.WriteUInt32(AuctionHouseID);
        writer.WriteBit(OpenForBusiness);
        writer.FlushBits();
        return writer.Position;
    }

    public WowGuid128 Guid;
    public uint AuctionHouseID;
    public uint PurchasedItemDeliveryDelay;
    public uint CancelledItemDeliveryDelay;
    public bool OpenForBusiness = true;
}

public readonly record struct AuctionListBidderItems(
    WowGuid128 Auctioneer, uint Offset, List<uint> AuctionItemIDs);

public readonly record struct AuctionListOwnerItems(WowGuid128 Auctioneer, uint Offset);

// ---- the 4.4.2 client's retail-style auction house; answered by Systems/RetailAuctionHouse ----

/// <summary>CMSG_AUCTION_BROWSE_QUERY: the Buy tab's search, answered with one row per item.</summary>
public readonly record struct AuctionBrowseQuery(
    WowGuid128 Auctioneer, uint Offset, byte MinLevel, byte MaxLevel, AuctionHouseFilterMask Filters,
    string Name, List<ClassFilter> ClassFilters);

/// <summary>CMSG_AUCTION_LIST_ITEMS_BY_BUCKET_KEY: the auctions behind one row.</summary>
public readonly record struct AuctionListItemsByBucketKey(
    WowGuid128 Auctioneer, uint Offset, AuctionBucketKey BucketKey);

/// <summary>CMSG_AUCTION_LIST_ITEMS_BY_ITEM_ID: every auction of an item, whatever its row.</summary>
public readonly record struct AuctionListItemsByItemID(
    WowGuid128 Auctioneer, uint ItemID, uint SuffixItemNameDescriptionID, uint Offset);

/// <summary>CMSG_AUCTION_LIST_BUCKETS_BY_BUCKET_KEYS: the rows of keys the client kept, such as favourites.</summary>
public readonly record struct AuctionListBucketsByBucketKeys(
    WowGuid128 Auctioneer, List<AuctionBucketKey> BucketKeys);

/// <summary>CMSG_AUCTION_SELL_COMMODITY: stackable items, sold at a price each.</summary>
public readonly record struct AuctionSellCommodity(
    WowGuid128 Auctioneer, ulong UnitPrice, uint RunTime, List<AuctionItemForSale> Items);

public readonly record struct AuctionGetCommodityQuote(WowGuid128 Auctioneer, uint ItemID, uint Quantity);

public readonly record struct AuctionConfirmCommoditiesPurchase(WowGuid128 Auctioneer, uint ItemID, uint Quantity);

public readonly record struct AuctionCancelCommoditiesPurchase(WowGuid128 Auctioneer);

public readonly record struct AuctionListItems(
    uint Offset, WowGuid128 Auctioneer, byte MinLevel, byte MaxLevel, int Quality,
    byte MaxPetLevel, List<byte> KnownPets, string Name, bool OnlyUsable, bool ExactMatch,
    List<ClassFilter> ClassFilters, List<AuctionSort> Sorts);

public readonly record struct AuctionSort(byte Type, byte Direction);

public readonly record struct ClassFilter(int ItemClass, List<SubClassFilter> SubClassFilters);

/// <remarks>
/// The two fields swap order between builds — V3_4_3 sends InvTypeMask (as a uint64) first, every
/// earlier build sends ItemSubclass first. That is one of the reasons AuctionListItems needs a
/// ranged codec pair rather than a branch inside one reader.
/// </remarks>
public readonly record struct SubClassFilter(int ItemSubclass, uint InvTypeMask);

/// <summary>The player's own auctions, or the ones they bid on; the opcode says which.</summary>
public sealed class AuctionListMyItemsResult : ServerPacket
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<AuctionListMyItemsResult>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V4_4_2_60895, new TotalCountLayout()),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new MoreResultsLayout()));

    private static readonly ServerPacketLayout<AuctionListMyItemsResult> Layout = Layouts.ForRunningClient();

    private readonly bool _owned;

    public AuctionListMyItemsResult(Opcode opcode) : base(opcode)
    {
        _owned = opcode == Opcode.SMSG_AUCTION_LIST_OWNED_ITEMS_RESULT;
    }

    public override void Write() => Layout.Write(this, _worldPacket);

    /// <summary>Up to 3.4.3: both lists are (count, total, delay, items).</summary>
    internal sealed class TotalCountLayout : ServerPacketLayout<AuctionListMyItemsResult>
    {
        public override void Write(AuctionListMyItemsResult packet, WorldPacket data)
        {
            data.WriteInt32(packet.Items.Count);
            data.WriteInt32(packet.TotalItemsCount);
            data.WriteUInt32(packet.DesiredDelay);

            foreach (AuctionItem item in packet.Items)
                item.Write(data);
        }
    }

    /// <summary>
    /// 4.4.2 (TrinityCore cata_classic, WowPacketParser, and the client's readers): the total is
    /// gone and a HasMoreResults bit follows the delay. The owned list carries a count of sold
    /// items after its own, and those items after the others; the bid list has neither.
    /// </summary>
    internal sealed class MoreResultsLayout : ServerPacketLayout<AuctionListMyItemsResult>
    {
        public override void Write(AuctionListMyItemsResult packet, WorldPacket data)
        {
            data.WriteInt32(packet.Items.Count);
            if (packet._owned)
                data.WriteInt32(0); // sold items
            data.WriteUInt32(packet.DesiredDelay);
            data.WriteBit(packet.Items.Count < packet.TotalItemsCount);
            data.FlushBits();

            foreach (AuctionItem item in packet.Items)
            {
                // The retail-style window files each auction under its item's key.
                AuctionItem shaped = item.Item is null
                    ? item
                    : RetailAuctionShape.ForOwnList(item, AuctionItemFacts.Of(item.Item.ItemID));
                shaped.Write(data, cataClassic: true);
            }
        }
    }

    public List<AuctionItem> Items = new();
    public int TotalItemsCount;
    public uint DesiredDelay = 300;
}

public sealed class AuctionListItemsResult : ServerPacket
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<AuctionListItemsResult>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V4_4_2_60895, new OnlyUsableLayout()),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new BucketKeyLayout()));

    private static readonly ServerPacketLayout<AuctionListItemsResult> Layout = Layouts.ForRunningClient();

    public AuctionListItemsResult() : base(Opcode.SMSG_AUCTION_LIST_ITEMS_RESULT) { }

    public override void Write() => Layout.Write(this, _worldPacket);

    /// <summary>Up to 3.4.3: an OnlyUsable byte follows the header when there are items.</summary>
    internal sealed class OnlyUsableLayout : ServerPacketLayout<AuctionListItemsResult>
    {
        public override void Write(AuctionListItemsResult packet, WorldPacket data)
        {
            data.WriteInt32(packet.Items.Count);
            data.WriteInt32(packet.TotalItemsCount);
            data.WriteUInt32(packet.DesiredDelay);

            if (packet.Items.Count > 0)
                data.WriteBool(packet.OnlyUsable);

            foreach (AuctionItem item in packet.Items)
                item.Write(data);
        }
    }

    /// <summary>
    /// 4.4.2 (TrinityCore cata_classic, WowPacketParser, and the client's reader): OnlyUsable is
    /// gone and so is the total after the count, which TrinityCore leaves 0. After the items come
    /// the list type, HasMoreResults, the key of the bucket the list is of, and a total: of items,
    /// not auctions.
    /// </summary>
    internal sealed class BucketKeyLayout : ServerPacketLayout<AuctionListItemsResult>
    {
        // The key's optionals default to present-and-zero, so say they are absent.
        private static readonly AuctionBucketKey NoBucket = new() { BattlePetSpeciesID = null, SuffixItemNameDescriptionID = null };

        public override void Write(AuctionListItemsResult packet, WorldPacket data)
        {
            data.WriteInt32(packet.Items.Count);
            data.WriteInt32(0);
            data.WriteUInt32(packet.DesiredDelay);

            foreach (AuctionItem item in packet.Items)
                item.Write(data, cataClassic: true);

            data.WriteBits((uint)packet.ListType, 2);
            data.WriteBit(packet.HasMoreResults ?? packet.Items.Count < packet.TotalItemsCount);
            data.FlushBits();
            (packet.BucketKey ?? NoBucket).Write(data);
            data.WriteInt32(packet.TotalItemsCount);
        }
    }

    public List<AuctionItem> Items = new();
    public int TotalItemsCount;
    public uint DesiredDelay = 300;
    public bool OnlyUsable;

    // 4.4.2 only. HasMoreResults unset: more when the server holds more than this page.
    public AuctionHouseListType ListType = AuctionHouseListType.Items;
    public AuctionBucketKey? BucketKey;
    public bool? HasMoreResults;
}

/// <summary>
/// 4.4.2: the Buy tab's rows, one per item rather than one per auction (TrinityCore cata_classic
/// AuctionListBucketsResult, and the client's reader).
/// </summary>
/// <remarks>
/// Only 4.4.2 has the opcode, so the packet cannot be made under another client build. What it
/// carries is <see cref="AuctionBucketList"/>, which can.
/// </remarks>
public sealed class AuctionListBucketsResult(AuctionBucketList list) : ServerPacket(Opcode.SMSG_AUCTION_LIST_BUCKETS_RESULT)
{
    public override void Write() => list.Write(_worldPacket);
}

public sealed class AuctionBucketList
{
    public void Write(WorldPacket data)
    {
        data.WriteInt32(Buckets.Count);
        data.WriteUInt32(DesiredDelay);
        data.WriteInt32(0); // Unknown830_0
        data.WriteInt32(0); // Unknown830_1
        data.WriteBits((uint)BrowseMode, 1);
        data.WriteBit(HasMoreResults);
        data.FlushBits();

        foreach (AuctionBucketInfo bucket in Buckets)
            bucket.Write(data);
    }

    public List<AuctionBucketInfo> Buckets = new();
    public uint DesiredDelay;
    public AuctionHouseBrowseMode BrowseMode;
    public bool HasMoreResults;
}

public sealed class AuctionBucketInfo
{
    public void Write(WorldPacket data)
    {
        Key.Write(data);
        data.WriteInt32(TotalQuantity);
        data.WriteInt32(RequiredLevel);
        data.WriteUInt64(MinPrice);
        data.WriteInt32(0);     // ItemModifiedAppearanceIDs
        data.WriteBit(false);   // MaxBattlePetQuality
        data.WriteBit(false);   // MaxBattlePetLevel
        data.WriteBit(false);   // BattlePetBreedID
        data.WriteBit(false);   // BattlePetLevelMask
        data.WriteBit(ContainsOwnerItem);
        data.WriteBit(true);    // ContainsOnlyCollectedAppearances
        data.FlushBits();
    }

    public AuctionBucketKey Key = null!;
    public int TotalQuantity;
    public int RequiredLevel;
    public ulong MinPrice;
    public bool ContainsOwnerItem;
}

/// <summary>
/// 4.4.2: what a quantity of a commodity costs right now. A quote with no price tells the client
/// the quantity cannot be had. Like <see cref="AuctionListBucketsResult"/>, a 4.4.2-only packet
/// around a body that is not.
/// </summary>
public sealed class AuctionGetCommodityQuoteResult(AuctionCommodityQuote quote) : ServerPacket(Opcode.SMSG_AUCTION_GET_COMMODITY_QUOTE_RESULT)
{
    public override void Write() => quote.Write(_worldPacket);
}

public sealed class AuctionCommodityQuote
{
    public void Write(WorldPacket data)
    {
        data.WriteBit(TotalPrice.HasValue);
        data.WriteBit(Quantity.HasValue);
        data.WriteBit(QuoteDuration.HasValue);
        data.FlushBits();
        data.WriteUInt32(ItemID);
        data.WriteUInt32(DesiredDelay);

        if (TotalPrice.HasValue)
            data.WriteUInt64(TotalPrice.Value);

        if (Quantity.HasValue)
            data.WriteUInt32(Quantity.Value);

        if (QuoteDuration.HasValue)
            data.WriteInt64(QuoteDuration.Value);
    }

    public uint ItemID;
    public uint DesiredDelay;
    public ulong? TotalPrice;
    public uint? Quantity;
    public long? QuoteDuration; // milliseconds
}

/// <summary>
/// What the retail-style auction house has to know of an item. It comes from the client's own
/// Item and ItemSparse tables, which the *4 CSVs carry whole; -1 stands for a fact that is not
/// known, and an unknown item passes every filter.
/// </summary>
internal readonly record struct AuctionItemFacts(
    string Name, int MaxStack, ushort ItemLevel, int RequiredLevel, int Quality,
    int ItemClass, int ItemSubclass, int InventoryType)
{
    public static readonly AuctionItemFacts Unknown = new(string.Empty, 1, 0, 0, -1, -1, -1, -1);

    /// <summary>The client sells and buys anything that stacks by the piece.</summary>
    public bool IsCommodity => MaxStack > 1;

    /// <summary>
    /// An item's name in the words a client of this locale searches with, or empty. The server's
    /// own name comes first; the item table is enUS, so it speaks for no other client.
    /// </summary>
    public static string SearchNameOf(uint itemId, string? locale)
    {
        string name = GameData.GetItemName(itemId);
        if (name.Length == 0 && locale == "enUS" && GameData.ItemSparseRecordsStore.TryGetValue(itemId, out var sparse))
            name = sparse.Name1;

        return name;
    }

    public static AuctionItemFacts Of(uint itemId)
    {
        GameData.ItemSparseRecordsStore.TryGetValue(itemId, out var sparse);
        GameData.ItemRecordsStore.TryGetValue(itemId, out var record);

        // A legacy server can only be asked for an item by name, so its own name comes first.
        string name = GameData.GetItemName(itemId);
        if (name.Length == 0 && sparse != null)
            name = sparse.Name1;

        return new AuctionItemFacts(
            name,
            sparse?.Stackable ?? 1,
            sparse?.ItemLevel ?? 0,
            sparse?.RequiredLevel ?? 0,
            sparse?.OverallQualityId ?? -1,
            record?.ClassId ?? -1,
            record?.SubclassId ?? -1,
            record?.InventoryType ?? -1);
    }
}

/// <summary>
/// A legacy auction, described the way the 4.4.2 client's retail-style auction house reads one
/// (TrinityCore cata_classic AuctionPosting::BuildAuctionItem).
/// </summary>
internal static class RetailAuctionShape
{
    private const int QualityShift = 6; // AuctionHouseFilterMask.PoorQuality
    private const uint AllQualities = 0x7F;

    /// <summary>
    /// An item's row. The key has no suffix: a legacy random property has no ItemNameDescription
    /// id to put there, so the variants of an item share its row, and each auction still shows
    /// its own stats.
    /// </summary>
    public static AuctionBucketKey KeyFor(uint itemId, in AuctionItemFacts facts) => new()
    {
        ItemID = itemId,
        ItemLevel = facts.ItemLevel,
        BattlePetSpeciesID = null,
        SuffixItemNameDescriptionID = null,
    };

    public static bool HasName(in AuctionItemFacts facts, string name)
        => facts.Name.Length == 0 || string.Equals(facts.Name, name, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Whether an auction answers an exact-match search for <paramref name="name"/>. A random
    /// suffix ("of the Whale") is the auction's, and no name is held for it, so a search that runs
    /// on past the item's name counts: the server has matched the rest of it.
    /// </summary>
    /// <param name="itemName">From <see cref="AuctionItemFacts.SearchNameOf"/>. Empty, the name is
    /// not known: the auction stays, since an extra row does less harm than an empty result.</param>
    public static bool IsExactMatch(AuctionItem auction, string itemName, string name)
    {
        if (auction.Item is null)
            return false;

        if (itemName.Length == 0 || string.Equals(itemName, name, StringComparison.OrdinalIgnoreCase))
            return true;

        return auction.Item.RandomPropertiesID != 0
            && name.Length > itemName.Length
            && name[itemName.Length] == ' '
            && name.StartsWith(itemName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Bit n set: quality n is wanted. None and all both mean no filter.</summary>
    public static uint QualityMask(AuctionHouseFilterMask filters) => ((uint)filters >> QualityShift) & AllQualities;

    public static bool MatchesQuality(in AuctionItemFacts facts, AuctionHouseFilterMask filters)
    {
        uint qualities = QualityMask(filters);
        if (qualities is 0 or AllQualities || facts.Quality < 0)
            return true;

        return facts.Quality < 7 && (qualities & (1u << facts.Quality)) != 0;
    }

    public static bool MatchesClasses(in AuctionItemFacts facts, List<ClassFilter> filters)
    {
        if (filters.Count == 0 || facts.ItemClass < 0)
            return true;

        foreach (ClassFilter classFilter in filters)
        {
            if (classFilter.ItemClass != facts.ItemClass)
                continue;

            if (classFilter.SubClassFilters.Count == 0)
                return true;

            foreach (SubClassFilter subClass in classFilter.SubClassFilters)
            {
                if (subClass.ItemSubclass != facts.ItemSubclass)
                    continue;

                if (subClass.InvTypeMask == 0 || facts.InventoryType is < 0 or > 31 ||
                    (subClass.InvTypeMask & (1u << facts.InventoryType)) != 0)
                    return true;
            }
        }

        return false;
    }

    /// <summary>What an auction costs to take: its buyout, else the standing bid, else the opening bid.</summary>
    public static ulong Price(AuctionItem auction)
    {
        if (auction.BuyoutPrice > 0)
            return auction.BuyoutPrice.Value;

        if (auction.BidAmount > 0)
            return auction.BidAmount.Value;

        return auction.MinBid ?? 0;
    }

    /// <summary>A stack's price per piece, rounded up: a stack is never shown cheaper than it is.</summary>
    public static ulong UnitPrice(ulong stackPrice, int count)
        => count > 1 ? (stackPrice + (ulong)count - 1) / (ulong)count : stackPrice;

    /// <summary>
    /// For the player's own auctions and bids: the item and its key, always. A commodity has a
    /// unit price and nothing else; anything else has the prices it was posted with.
    /// </summary>
    public static AuctionItem ForOwnList(AuctionItem auction, in AuctionItemFacts facts)
    {
        AuctionItem shaped = Copy(auction);
        shaped.AuctionBucketKey = KeyFor(auction.Item.ItemID, facts);
        shaped.CensorServerSideInfo = auction.CensorServerSideInfo;
        shaped.CensorBidInfo = false;
        CopyBid(auction, shaped);

        if (facts.IsCommodity)
            shaped.UnitPrice = UnitPrice(Price(auction), auction.Count);
        else
            CopyPrices(auction, shaped);

        return shaped;
    }

    /// <summary>For the auctions behind a row. Server-side fields only on the player's own.</summary>
    public static AuctionItem ForItemList(AuctionItem auction, in AuctionItemFacts facts, WowGuid128 player, bool withKey)
    {
        AuctionItem shaped = Copy(auction);
        CopyPrices(auction, shaped);
        shaped.CensorServerSideInfo = auction.Owner != player;
        shaped.CensorBidInfo = auction.Bidder.IsEmpty();
        CopyBid(auction, shaped);
        if (withKey)
            shaped.AuctionBucketKey = KeyFor(auction.Item.ItemID, facts);

        return shaped;
    }

    /// <summary>For a commodity's price list: a quantity at a unit price, and no item.</summary>
    public static AuctionItem ForCommodityList(AuctionItem auction, in AuctionItemFacts facts, WowGuid128 player, bool withKey)
    {
        AuctionItem shaped = Copy(auction);
        shaped.Item = null!;
        shaped.Charges = 0;
        shaped.Enchantments = [];
        shaped.Gems = [];
        shaped.ItemGuid = WowGuid128.Empty;
        shaped.UnitPrice = UnitPrice(Price(auction), auction.Count);
        shaped.CensorServerSideInfo = auction.Owner != player;
        shaped.CensorBidInfo = true;
        if (withKey)
            shaped.AuctionBucketKey = KeyFor(auction.Item.ItemID, facts);

        return shaped;
    }

    // Everything but the prices and the bid, which each list states its own way. The legacy
    // reader fills every price, 0 for none; this client takes a present 0 as a price.
    private static AuctionItem Copy(AuctionItem auction) => new()
    {
        Item = auction.Item,
        Count = auction.Count,
        Charges = auction.Charges,
        Enchantments = auction.Enchantments,
        Gems = auction.Gems,
        Flags = auction.Flags,
        AuctionID = auction.AuctionID,
        Owner = auction.Owner,
        DurationLeft = auction.DurationLeft,
        DeleteReason = auction.DeleteReason,
        ItemGuid = auction.ItemGuid,
        OwnerAccountID = auction.OwnerAccountID,
        EndTime = auction.EndTime,
        Creator = auction.Creator,
    };

    private static void CopyPrices(AuctionItem auction, AuctionItem shaped)
    {
        shaped.MinBid = auction.MinBid > 0 ? auction.MinBid : null;
        shaped.MinIncrement = auction.MinIncrement > 0 ? auction.MinIncrement : null;
        shaped.BuyoutPrice = auction.BuyoutPrice > 0 ? auction.BuyoutPrice : null;
    }

    private static void CopyBid(AuctionItem auction, AuctionItem shaped)
    {
        if (auction.Bidder.IsEmpty())
            return;

        shaped.Bidder = auction.Bidder;
        shaped.BidAmount = auction.BidAmount;
    }
}

public class AuctionItem
{
    public void Write(WorldPacket data) => Write(data, cataClassic: false);

    /// <param name="cataClassic">4.4.2 has an int32 after DeleteReason that earlier builds lack.</param>
    public void Write(WorldPacket data, bool cataClassic)
    {
        data.WriteBit(Item != null);
        data.WriteBits(Enchantments.Count, 4);
        data.WriteBits(Gems.Count, 2);
        data.WriteBit(MinBid.HasValue);
        data.WriteBit(MinIncrement.HasValue);
        data.WriteBit(BuyoutPrice.HasValue);
        data.WriteBit(UnitPrice.HasValue);
        data.WriteBit(CensorServerSideInfo);
        data.WriteBit(CensorBidInfo);
        data.WriteBit(AuctionBucketKey != null);
        data.WriteBit(Creator != default);
        if (!CensorBidInfo)
        {
            data.WriteBit(Bidder != default);
            data.WriteBit(BidAmount.HasValue);
        }

        data.FlushBits();

        if (Item != null)
            Item.Write(data);

        data.WriteInt32(Count);
        data.WriteInt32(Charges);
        data.WriteUInt32(Flags);
        data.WriteUInt32(AuctionID);
        data.WritePackedGuid128(Owner);
        data.WriteInt32(DurationLeft);
        data.WriteUInt8(DeleteReason);
        if (cataClassic)
            data.WriteInt32(0);

        foreach (ItemEnchantData enchant in Enchantments)
            enchant.Write(data);

        if (MinBid.HasValue)
            data.WriteUInt64(MinBid.Value);

        if (MinIncrement.HasValue)
            data.WriteUInt64(MinIncrement.Value);

        if (BuyoutPrice.HasValue)
            data.WriteUInt64(BuyoutPrice.Value);

        if (UnitPrice.HasValue)
            data.WriteUInt64(UnitPrice.Value);

        if (!CensorServerSideInfo)
        {
            data.WritePackedGuid128(ItemGuid);
            data.WritePackedGuid128(OwnerAccountID);
            data.WriteUInt32(EndTime);
        }

        if (Creator != default)
            data.WritePackedGuid128(Creator);

        if (!CensorBidInfo)
        {
            if (Bidder != default)
                data.WritePackedGuid128(Bidder);

            if (BidAmount.HasValue)
                data.WriteUInt64(BidAmount.Value);
        }

        foreach (ItemGemData gem in Gems)
            gem.Write(data);

        if (AuctionBucketKey != null)
            AuctionBucketKey.Write(data);
    }

    public ItemInstance Item = null!;
    public int Count;
    public int Charges;
    public List<ItemEnchantData> Enchantments = new();
    public uint Flags = 196608;
    public uint AuctionID;
    public WowGuid128 Owner;
    public ulong? MinBid;
    public ulong? MinIncrement;
    public ulong? BuyoutPrice;
    public ulong? UnitPrice;
    public int DurationLeft;
    public byte DeleteReason;
    public bool CensorServerSideInfo;
    public bool CensorBidInfo;
    public WowGuid128 ItemGuid = WowGuid128.Empty;
    public WowGuid128 OwnerAccountID;
    public uint EndTime;
    public WowGuid128 Creator;
    public WowGuid128 Bidder;
    public ulong? BidAmount;
    public List<ItemGemData> Gems = new();
    public AuctionBucketKey AuctionBucketKey = null!;
}

public class AuctionBucketKey
{
    public AuctionBucketKey() { }

    /// <summary>Span-reader twin of the <see cref="WorldPacket"/> constructor.</summary>
    public static AuctionBucketKey Read(ref SpanPacketReader data)
    {
        data.ResetBitReader();
        uint itemId = data.ReadBits<uint>(20);
        bool hasBattlePetSpecies = data.HasBit();
        ushort itemLevel = data.ReadBits<ushort>(11);
        bool hasSuffix = data.HasBit();
        data.ResetBitReader();

        ushort? battlePetSpecies = hasBattlePetSpecies ? data.ReadUInt16() : null;
        ushort? suffix = hasSuffix ? data.ReadUInt16() : null;

        return new AuctionBucketKey
        {
            ItemID = itemId,
            ItemLevel = itemLevel,
            BattlePetSpeciesID = battlePetSpecies,
            SuffixItemNameDescriptionID = suffix,
        };
    }

    public void Write(WorldPacket data)
    {
        data.WriteBits(ItemID, 20);
        data.WriteBit(BattlePetSpeciesID.HasValue);
        data.WriteBits(ItemLevel, 11);
        data.WriteBit(SuffixItemNameDescriptionID.HasValue);
        data.FlushBits();

        if (BattlePetSpeciesID.HasValue)
            data.WriteUInt16(BattlePetSpeciesID.Value);

        if (SuffixItemNameDescriptionID.HasValue)
            data.WriteUInt16(SuffixItemNameDescriptionID.Value);
    }

    public uint ItemID;
    public ushort ItemLevel;
    public ushort? BattlePetSpeciesID = new();
    public ushort? SuffixItemNameDescriptionID = new();
}

public class ItemEnchantData
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt32(ID);
        data.WriteUInt32(Expiration);
        data.WriteInt32(Charges);
        data.WriteUInt8(Slot);
    }

    public uint ID;
    public uint Expiration;
    public int Charges;
    public byte Slot;
}

public readonly record struct AuctionSellItem(
    WowGuid128 Auctioneer, ulong MinBid, ulong BuyoutPrice, uint ExpireTime,
    AddOnInfo? TaintedBy, List<AuctionItemForSale> Items);

public readonly record struct AuctionItemForSale(WowGuid128 Guid, uint UseCount);

public readonly record struct AuctionRemoveItem(
    WowGuid128 Auctioneer, uint AuctionID, AddOnInfo? TaintedBy);

public class AddOnInfo
{
    /// <summary>Span-reader twin of <see cref="Read(WorldPacket)"/>, kept in lockstep with it.</summary>
    public void Read(ref Framework.IO.SpanPacketReader data)
    {
        data.ResetBitReader();

        uint nameLength = data.ReadBits<uint>(10);
        uint versionLength = data.ReadBits<uint>(10);
        Loaded = data.HasBit();
        Disabled = data.HasBit();
        if (nameLength > 1)
        {
            Name = data.ReadString(nameLength - 1);
            data.ReadUInt8(); // null terminator
        }
        if (versionLength > 1)
        {
            Version = data.ReadString(versionLength - 1);
            data.ReadUInt8(); // null terminator
        }
    }

    public string Name = string.Empty;
    public string Version = string.Empty;
    public bool Loaded;
    public bool Disabled;
}

public readonly record struct AuctionPlaceBid(
    WowGuid128 Auctioneer, uint AuctionID, ulong BidAmount, AddOnInfo? TaintedBy);

class AuctionCommandResult : ServerPacket, ISpanWritable
{
    public AuctionCommandResult() : base(Opcode.SMSG_AUCTION_COMMAND_RESULT) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(AuctionID);
        _worldPacket.WriteInt32((int)Command);
        _worldPacket.WriteInt32((int)ErrorCode);
        _worldPacket.WriteInt32((int)BagResult);
        _worldPacket.WritePackedGuid128(Guid);
        _worldPacket.WriteUInt64(MinIncrement);
        _worldPacket.WriteUInt64(Money);
        _worldPacket.WriteUInt32(DesiredDelay);
    }

    // 4 ints(16) + GUID(18) + 2 ulongs(16) + uint(4) = 54
    public int MaxSize => 16 + PackedGuidHelper.MaxPackedGuid128Size + 16 + 4;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(AuctionID);
        writer.WriteInt32((int)Command);
        writer.WriteInt32((int)ErrorCode);
        writer.WriteInt32((int)BagResult);
        writer.WritePackedGuid128(Guid.Low, Guid.High);
        writer.WriteUInt64(MinIncrement);
        writer.WriteUInt64(Money);
        writer.WriteUInt32(DesiredDelay);
        return writer.Position;
    }

    public uint AuctionID;                              //< the id of the auction that triggered this notification
    public AuctionHouseAction Command;                  //< the type of action that triggered this notification. Possible values are @ref AuctionAction
    public AuctionHouseError ErrorCode;                 //< the error code that was generated when trying to perform the action. Possible values are @ref AuctionError
    public InventoryResult BagResult = InventoryResult.InternalBagError; //< the bid error. Possible values are @ref AuctionError
    public WowGuid128 Guid = WowGuid128.Empty;          //< the GUID of the bidder for this auction.
    public ulong MinIncrement;                          //< the sum of outbid is (1% of current bid) * 5, if the bid is too small, then this value is 1 copper.
    public ulong Money;                                 //< the amount of money that the player bid in copper
    public uint DesiredDelay;
}

public class AuctionOwnerNotification
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt32(AuctionID);
        data.WriteUInt64(BidAmount);
        Item.Write(data);
    }

    public uint AuctionID;
    public ulong BidAmount;
    public ItemInstance Item = new ItemInstance();
}

internal static class AuctionPacketHelpers
{
    // AuctionOwnerNotification: uint(4) + ulong(8) + ItemInstance
    public const int AuctionOwnerNotificationMaxSize = 12 + ItemPacketHelpers.ItemInstanceMaxSize;

    // AuctionBidderNotification: 2 uints(8) + PackedGuid128(18) + ItemInstance
    public const int AuctionBidderNotificationMaxSize = 8 + PackedGuidHelper.MaxPackedGuid128Size + ItemPacketHelpers.ItemInstanceMaxSize;

    public static bool WriteAuctionOwnerNotification(ref SpanPacketWriter writer, AuctionOwnerNotification info)
    {
        writer.WriteUInt32(info.AuctionID);
        writer.WriteUInt64(info.BidAmount);
        return ItemPacketHelpers.WriteItemInstance(ref writer, info.Item);
    }

    public static bool WriteAuctionBidderNotification(ref SpanPacketWriter writer, AuctionBidderNotification info)
    {
        writer.WriteUInt32(info.WireHouseId);
        writer.WriteUInt32(info.AuctionID);
        writer.WritePackedGuid128(info.Bidder.Low, info.Bidder.High);
        return ItemPacketHelpers.WriteItemInstance(ref writer, info.Item);
    }
}

class AuctionClosedNotification : ServerPacket, ISpanWritable
{
    public AuctionClosedNotification() : base(Opcode.SMSG_AUCTION_CLOSED_NOTIFICATION) { }

    public override void Write()
    {
        Info.Write(_worldPacket);
        _worldPacket.WriteFloat(ProceedsMailDelay);
        _worldPacket.WriteBit(Sold);
        _worldPacket.FlushBits();
    }

    // AuctionOwnerNotification + float(4) + bit(1)
    public int MaxSize => AuctionPacketHelpers.AuctionOwnerNotificationMaxSize + 5;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        if (!AuctionPacketHelpers.WriteAuctionOwnerNotification(ref writer, Info))
            return -1;
        writer.WriteFloat(ProceedsMailDelay);
        writer.WriteBit(Sold);
        writer.FlushBits();
        return writer.Position;
    }

    public AuctionOwnerNotification Info = null!;
    public float ProceedsMailDelay = 3600;
    public bool Sold = true;
}

class AuctionOwnerBidNotification : ServerPacket, ISpanWritable
{
    public AuctionOwnerBidNotification() : base(Opcode.SMSG_AUCTION_OWNER_BID_NOTIFICATION) { }

    public override void Write()
    {
        Info.Write(_worldPacket);
        _worldPacket.WriteUInt64(MinIncrement);
        _worldPacket.WritePackedGuid128(Bidder);
    }

    // AuctionOwnerNotification + ulong(8) + PackedGuid128(18)
    public int MaxSize => AuctionPacketHelpers.AuctionOwnerNotificationMaxSize + 8 + PackedGuidHelper.MaxPackedGuid128Size;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        if (!AuctionPacketHelpers.WriteAuctionOwnerNotification(ref writer, Info))
            return -1;
        writer.WriteUInt64(MinIncrement);
        writer.WritePackedGuid128(Bidder.Low, Bidder.High);
        return writer.Position;
    }

    public AuctionOwnerNotification Info = null!;
    public ulong MinIncrement;
    public WowGuid128 Bidder;
}

class AuctionBidderNotification
{
    // The 3.4.3 client reads the first field as the auction house id and applies an outbid only
    // when it matches the open auction house. The fixed 2 (TrinityCore 3.4.3 sends the same) matched
    // only house 2, so outbids at the Horde and Blackwater auction houses were ignored. Other builds
    // keep the 2 until their clients are checked.
    private static bool SendsHouseId => ForceV343ForTests ?? ModernVersion.IsWotLKClassicOrLater;
    // The test process never runs as V3_4_3; tests set this to reach the 3.4.3 layout.
    internal static bool? ForceV343ForTests;

    internal uint WireHouseId => SendsHouseId ? AuctionHouseID : 2;

    public void Write(WorldPacket data)
    {
        data.WriteUInt32(WireHouseId);
        data.WriteUInt32(AuctionID);
        data.WritePackedGuid128(Bidder);
        Item.Write(data);
    }

    public uint AuctionHouseID;
    public uint AuctionID;
    public WowGuid128 Bidder;
    public ItemInstance Item = new ItemInstance();
}

class AuctionWonNotification : ServerPacket, ISpanWritable
{
    public AuctionWonNotification() : base(Opcode.SMSG_AUCTION_WON_NOTIFICATION) { }

    public override void Write()
    {
        Info.Write(_worldPacket);
    }

    public int MaxSize => AuctionPacketHelpers.AuctionBidderNotificationMaxSize;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        if (!AuctionPacketHelpers.WriteAuctionBidderNotification(ref writer, Info))
            return -1;
        return writer.Position;
    }

    public AuctionBidderNotification Info = null!;
}

class AuctionOutbidNotification : ServerPacket, ISpanWritable
{
    public AuctionOutbidNotification() : base(Opcode.SMSG_AUCTION_OUTBID_NOTIFICATION) { }

    public override void Write()
    {
        Info.Write(_worldPacket);
        _worldPacket.WriteUInt64(BidAmount);
        _worldPacket.WriteUInt64(MinIncrement);
    }

    // AuctionBidderNotification + 2 ulongs(16)
    public int MaxSize => AuctionPacketHelpers.AuctionBidderNotificationMaxSize + 16;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        if (!AuctionPacketHelpers.WriteAuctionBidderNotification(ref writer, Info))
            return -1;
        writer.WriteUInt64(BidAmount);
        writer.WriteUInt64(MinIncrement);
        return writer.Position;
    }

    public AuctionBidderNotification Info = null!;
    public ulong BidAmount;
    public ulong MinIncrement;
}
