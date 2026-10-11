using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

// Auction-house CMSG codecs.
//
// AuctionListItems is the reason these were held back from the empty-payload slice. Its old reader
// branched on `ModernVersion.Build == V3_4_3_54261` and the two sides are not variations on a
// theme — they disagree on field *order* (V3_4_3 sends Auctioneer before Offset, everything else
// the reverse), on the width of the known-pet count, on SubClassFilter's field order and the width
// of InvTypeMask, and V3_4_3 alone carries a tainted-addon block and a sort-data size. A ranged
// codec pair states that as two layouts rather than one reader with a fork in it, and the pair is
// resolved once when the dispatch table is built.
//
// Three packets carry an optional AddOnInfo the proxy never forwards. It still has to be read, or
// everything after it misaligns — so it is parsed and kept on the packet rather than skipped by
// byte count, which would silently rot if the addon block ever changed shape.

[PacketCodec(typeof(AuctionListBidderItems), RemovedIn = ClientVersionBuild.V4_4_2_60895)]
public static class AuctionListBidderItemsCodec
{
    /// <remarks>
    /// The auction-id count is seven bits, so the list is capped at 127 by the wire format and
    /// pre-sizes on the count directly.
    /// </remarks>
    public static void Read(ref SpanPacketReader r, out AuctionListBidderItems packet)
    {
        WowGuid128 auctioneer = r.ReadPackedGuid128();
        uint offset = r.ReadUInt32();

        uint auctionIdCount = r.ReadBits<uint>(7);
        r.ResetBitReader();

        var auctionItemIds = new List<uint>((int)auctionIdCount);
        for (var i = 0; i < auctionIdCount; ++i)
            auctionItemIds.Add(r.ReadUInt32());

        packet = new AuctionListBidderItems(auctioneer, offset, auctionItemIds);
    }
}

// 4.4.2 (TrinityCore cata_classic, and the client's writer): the TaintedBy bit comes before the
// auction-id count, and a 2-bit count of sort orders after it; the sorts trail the ids. Read the
// 3.4.3 way, the id count came out shifted by one bit.
[PacketCodec(typeof(AuctionListBidderItems), AddedIn = ClientVersionBuild.V4_4_2_60895)]
public static class AuctionListBidderItemsCodecCataClassic
{
    public static void Read(ref SpanPacketReader r, out AuctionListBidderItems packet)
    {
        WowGuid128 auctioneer = r.ReadPackedGuid128();
        uint offset = r.ReadUInt32();

        bool hasTaintedBy = r.HasBit();
        uint auctionIdCount = r.ReadBits<uint>(7);
        uint sortCount = r.ReadBits<uint>(2);
        if (hasTaintedBy)
            new AddOnInfo().Read(ref r);
        else
            r.ResetBitReader();

        var auctionItemIds = new List<uint>((int)auctionIdCount);
        for (var i = 0; i < auctionIdCount; ++i)
            auctionItemIds.Add(r.ReadUInt32());

        AuctionSortCodec.Skip(ref r, sortCount);
        packet = new AuctionListBidderItems(auctioneer, offset, auctionItemIds);
    }
}

/// <summary>4.4.2's AuctionSortDef: a sort order byte and a reverse bit. Legacy servers sort themselves.</summary>
internal static class AuctionSortCodec
{
    public static void Skip(ref SpanPacketReader r, uint count)
    {
        for (uint i = 0; i < count; i++)
        {
            r.ResetBitReader();
            r.ReadUInt8();
            r.HasBit();
        }
        r.ResetBitReader();
    }
}

// ---- AuctionListItems: the ranged pair ----

[PacketCodec(typeof(AuctionListItems), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class AuctionListItemsCodecWotLKClassic
{
    /// <remarks>
    /// Wire per CypherCore WotLK-Classic AuctionListItems.Read, the authoritative 54261 layout —
    /// TC 3.4.3_Source and WPP's retail handler both mis-handle it. The class-filter bodies ARE
    /// sent; an earlier revision discarded them and silently dropped category filtering (#85).
    /// </remarks>
    public static void Read(ref SpanPacketReader r, out AuctionListItems packet)
    {
        WowGuid128 auctioneer = r.ReadPackedGuid128();
        uint offset = r.ReadUInt32();
        byte minLevel = r.ReadUInt8();
        byte maxLevel = r.ReadUInt8();
        int quality = r.ReadInt32();
        int sortsCount = r.ReadUInt8();
        int knownPetSize = r.ReadInt32();
        byte maxPetLevel = (byte)r.ReadInt8();

        var knownPets = new List<byte>(knownPetSize > 0 ? knownPetSize : 0);
        for (int i = 0; i < knownPetSize; ++i)
            knownPets.Add(r.ReadUInt8());

        bool tainted = r.HasBit();
        uint nameLen = r.ReadBits<uint>(8);
        string name = r.ReadString(nameLen);

        r.ResetBitReader();
        uint itemClassFilterCount = r.ReadBits<uint>(3);
        bool onlyUsable = r.HasBit();
        bool exactMatch = r.HasBit();

        if (tainted)
        {
            // Consume the AddOnInfo (TaintedBy) body to stay aligned; not forwarded.
            r.ResetBitReader();
            uint addonNameLen = r.ReadBits<uint>(10);
            uint addonVerLen = r.ReadBits<uint>(10);
            r.HasBit(); // Loaded
            r.HasBit(); // Disabled
            if (addonNameLen > 1) { r.ReadString(addonNameLen - 1); r.ReadUInt8(); }
            if (addonVerLen > 1) { r.ReadString(addonVerLen - 1); r.ReadUInt8(); }
        }

        var classFilters = new List<ClassFilter>((int)itemClassFilterCount);
        for (uint i = 0; i < itemClassFilterCount; ++i)
        {
            int itemClass = r.ReadInt32();
            uint subClassFilterCount = r.ReadBits<uint>(5);
            var subFilters = new List<SubClassFilter>((int)subClassFilterCount);
            for (uint j = 0; j < subClassFilterCount; ++j)
            {
                // InvTypeMask first here, and 64 bits wide — the pre-WotLK layout has it second
                // and 32 bits.
                uint invTypeMask = (uint)r.ReadUInt64();
                subFilters.Add(new SubClassFilter(r.ReadInt32(), invTypeMask));
            }
            classFilters.Add(new ClassFilter(itemClass, subFilters));
        }

        r.ReadInt32(); // sortDataSize
        var sorts = new List<AuctionSort>(sortsCount > 0 ? sortsCount : 0);
        for (int i = 0; i < sortsCount; ++i)
        {
            r.ResetBitReader();
            byte type = r.ReadUInt8();
            sorts.Add(new AuctionSort(type, r.ReadUInt8()));
        }

        packet = new AuctionListItems(offset, auctioneer, minLevel, maxLevel, quality, maxPetLevel,
            knownPets, name, onlyUsable, exactMatch, classFilters, sorts);
    }
}

[PacketCodec(typeof(AuctionListItems), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class AuctionListItemsCodecPreWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out AuctionListItems packet)
    {
        uint offset = r.ReadUInt32();
        WowGuid128 auctioneer = r.ReadPackedGuid128();

        byte minLevel = r.ReadUInt8();
        byte maxLevel = r.ReadUInt8();
        int quality = r.ReadInt32();
        var sortCount = r.ReadUInt8();
        var knownPetsCount = r.ReadUInt32();
        byte maxPetLevel = r.ReadUInt8();

        var knownPets = new List<byte>((int)knownPetsCount);
        for (int i = 0; i < knownPetsCount; ++i)
            knownPets.Add(r.ReadUInt8());

        uint nameLength = r.ReadBits<uint>(8);
        string name = r.ReadString(nameLength);

        uint classFiltersCount = r.ReadBits<uint>(3);

        bool onlyUsable = r.HasBit();
        bool exactMatch = r.HasBit();
        r.ResetBitReader();

        var classFilters = new List<ClassFilter>((int)classFiltersCount);
        for (int i = 0; i < classFiltersCount; ++i)
        {
            int itemClass = r.ReadInt32();
            uint subClassFiltersCount = r.ReadBits<uint>(5);
            var subFilters = new List<SubClassFilter>((int)subClassFiltersCount);
            for (uint j = 0; j < subClassFiltersCount; ++j)
            {
                int itemSubclass = r.ReadInt32();
                subFilters.Add(new SubClassFilter(itemSubclass, r.ReadUInt32()));
            }
            classFilters.Add(new ClassFilter(itemClass, subFilters));
        }

        // The sorts arrive as a length-prefixed blob. The old reader copied it into a second
        // WorldPacket and read the pairs from that; here the pairs are read in place and the
        // reader is then advanced past whatever the blob's length claimed, which is the same
        // thing without handing a span that aliases a pooled buffer to anyone.
        var size = r.ReadUInt32();
        int blobStart = r.Position;
        var sorts = new List<AuctionSort>(sortCount > 0 ? sortCount : 0);
        for (var i = 0; i < sortCount; ++i)
        {
            byte type = r.ReadUInt8();
            sorts.Add(new AuctionSort(type, r.ReadUInt8()));
        }
        r.Skip(blobStart + (int)size - r.Position);

        packet = new AuctionListItems(offset, auctioneer, minLevel, maxLevel, quality, maxPetLevel,
            knownPets, name, onlyUsable, exactMatch, classFilters, sorts);
    }
}

// ---- the three that carry an optional AddOnInfo ----

public static class AuctionSellItemCodec
{
    /// <remarks>
    /// The item-count width depends on the client line rather than a single build boundary, so it
    /// stays an inline branch on a static readonly predicate the JIT folds — the case ranged pairs
    /// cannot express, as PlayerLoginCodec documents. Note the tainted bit is read *before* the
    /// count but the addon body *after* it.
    /// </remarks>
    public static void Read(ref SpanPacketReader r, out AuctionSellItem packet)
    {
        WowGuid128 auctioneer = r.ReadPackedGuid128();
        ulong minBid = r.ReadUInt64();
        ulong buyoutPrice = r.ReadUInt64();
        uint expireTime = r.ReadUInt32();

        AddOnInfo? taintedBy = null;
        if (r.HasBit())
            taintedBy = new AddOnInfo();

        int itemCountBits = ModernVersion.AddedInClassicVersion(1, 14, 3, 2, 5, 4) ? 6 : 5;
        uint itemCount = r.ReadBits<uint>(itemCountBits);

        if (taintedBy != null)
            taintedBy.Read(ref r);

        var items = new List<AuctionItemForSale>((int)itemCount);
        for (var i = 0; i < itemCount; ++i)
        {
            WowGuid128 guid = r.ReadPackedGuid128();
            items.Add(new AuctionItemForSale(guid, r.ReadUInt32()));
        }

        packet = new AuctionSellItem(auctioneer, minBid, buyoutPrice, expireTime, taintedBy, items);
    }
}

[PacketCodec(typeof(AuctionRemoveItem), RemovedIn = ClientVersionBuild.V4_4_2_60895)]
public static class AuctionRemoveItemCodec
{
    public static void Read(ref SpanPacketReader r, out AuctionRemoveItem packet)
    {
        WowGuid128 auctioneer = r.ReadPackedGuid128();
        uint auctionId = r.ReadUInt32();

        AddOnInfo? taintedBy = null;
        if (r.HasBit())
            taintedBy = new AddOnInfo();
        if (taintedBy != null)
            taintedBy.Read(ref r);

        packet = new AuctionRemoveItem(auctioneer, auctionId, taintedBy);
    }
}

// 4.4.2 (TrinityCore cata_classic, and the client's writer) sends the item id after the auction
// id. Read the 3.4.3 way, its first byte became the TaintedBy bit.
[PacketCodec(typeof(AuctionRemoveItem), AddedIn = ClientVersionBuild.V4_4_2_60895)]
public static class AuctionRemoveItemCodecCataClassic
{
    public static void Read(ref SpanPacketReader r, out AuctionRemoveItem packet)
    {
        WowGuid128 auctioneer = r.ReadPackedGuid128();
        uint auctionId = r.ReadUInt32();
        r.ReadInt32();                          // ItemID

        AddOnInfo? taintedBy = null;
        if (r.HasBit())
            taintedBy = new AddOnInfo();
        if (taintedBy != null)
            taintedBy.Read(ref r);

        packet = new AuctionRemoveItem(auctioneer, auctionId, taintedBy);
    }
}

public static class AuctionPlaceBidCodec
{
    public static void Read(ref SpanPacketReader r, out AuctionPlaceBid packet)
    {
        WowGuid128 auctioneer = r.ReadPackedGuid128();
        uint auctionId = r.ReadUInt32();
        ulong bidAmount = r.ReadUInt64();

        AddOnInfo? taintedBy = null;
        if (r.HasBit())
            taintedBy = new AddOnInfo();
        if (taintedBy != null)
            taintedBy.Read(ref r);

        packet = new AuctionPlaceBid(auctioneer, auctionId, bidAmount, taintedBy);
    }
}

// ---- the 4.4.2 client's retail-style auction house ----
//
// Only that client has these opcodes. Layouts: docs/protocol/4.4.2.60895/cmsg-structures.md, read
// from the client, and TrinityCore cata_classic's readers, which agree with it. Every request
// leads its bit section with the TaintedBy bit and trails its sort orders; the proxy forwards
// neither.

public static class AuctionBrowseQueryCodec
{
    public static void Read(ref SpanPacketReader r, out AuctionBrowseQuery packet)
    {
        WowGuid128 auctioneer = r.ReadPackedGuid128();
        uint offset = r.ReadUInt32();
        byte minLevel = r.ReadUInt8();
        byte maxLevel = r.ReadUInt8();
        r.ReadUInt8();                          // Unused1007_1
        r.ReadUInt8();                          // Unused1007_2
        var filters = (AuctionHouseFilterMask)r.ReadUInt32();
        uint knownPetsSize = r.ReadUInt32();
        r.ReadUInt8();                          // MaxPetLevel
        r.ReadUInt32();                         // Unused1026
        r.Skip((int)Math.Min(knownPetsSize, (uint)r.Remaining)); // KnownPets

        bool hasTaintedBy = r.HasBit();
        uint nameLength = r.ReadBits<uint>(8);
        uint classFilterCount = r.ReadBits<uint>(3);
        uint sortCount = r.ReadBits<uint>(2);
        r.ResetBitReader();

        if (hasTaintedBy)
            new AddOnInfo().Read(ref r);

        string name = r.ReadString(nameLength);

        var classFilters = new List<ClassFilter>((int)classFilterCount);
        for (uint i = 0; i < classFilterCount; ++i)
        {
            int itemClass = r.ReadInt32();
            uint subClassFilterCount = r.ReadBits<uint>(5);
            r.ResetBitReader();

            var subFilters = new List<SubClassFilter>((int)subClassFilterCount);
            for (uint j = 0; j < subClassFilterCount; ++j)
            {
                // 64 bits wide; no inventory type is past bit 31.
                uint invTypeMask = (uint)r.ReadUInt64();
                subFilters.Add(new SubClassFilter(r.ReadInt32(), invTypeMask));
            }
            classFilters.Add(new ClassFilter(itemClass, subFilters));
        }

        AuctionSortCodec.Skip(ref r, sortCount);
        packet = new AuctionBrowseQuery(auctioneer, offset, minLevel, maxLevel, filters, name, classFilters);
    }
}

public static class AuctionListItemsByBucketKeyCodec
{
    public static void Read(ref SpanPacketReader r, out AuctionListItemsByBucketKey packet)
    {
        WowGuid128 auctioneer = r.ReadPackedGuid128();
        uint offset = r.ReadUInt32();
        r.ReadUInt8();                          // Unknown830

        bool hasTaintedBy = r.HasBit();
        uint sortCount = r.ReadBits<uint>(2);
        AuctionBucketKey bucketKey = AuctionBucketKey.Read(ref r);

        if (hasTaintedBy)
            new AddOnInfo().Read(ref r);

        AuctionSortCodec.Skip(ref r, sortCount);
        packet = new AuctionListItemsByBucketKey(auctioneer, offset, bucketKey);
    }
}

public static class AuctionListItemsByItemIDCodec
{
    public static void Read(ref SpanPacketReader r, out AuctionListItemsByItemID packet)
    {
        WowGuid128 auctioneer = r.ReadPackedGuid128();
        uint itemId = r.ReadUInt32();
        uint suffix = r.ReadUInt32();
        uint offset = r.ReadUInt32();

        bool hasTaintedBy = r.HasBit();
        uint sortCount = r.ReadBits<uint>(2);
        r.ResetBitReader();

        if (hasTaintedBy)
            new AddOnInfo().Read(ref r);

        AuctionSortCodec.Skip(ref r, sortCount);
        packet = new AuctionListItemsByItemID(auctioneer, itemId, suffix, offset);
    }
}

public static class AuctionListBucketsByBucketKeysCodec
{
    /// <remarks>The key count is seven bits, so the list is capped at 127 by the wire format.</remarks>
    public static void Read(ref SpanPacketReader r, out AuctionListBucketsByBucketKeys packet)
    {
        WowGuid128 auctioneer = r.ReadPackedGuid128();

        bool hasTaintedBy = r.HasBit();
        uint keyCount = r.ReadBits<uint>(7);
        uint sortCount = r.ReadBits<uint>(2);
        r.ResetBitReader();

        if (hasTaintedBy)
            new AddOnInfo().Read(ref r);

        var bucketKeys = new List<AuctionBucketKey>((int)keyCount);
        for (uint i = 0; i < keyCount; ++i)
            bucketKeys.Add(AuctionBucketKey.Read(ref r));

        AuctionSortCodec.Skip(ref r, sortCount);
        packet = new AuctionListBucketsByBucketKeys(auctioneer, bucketKeys);
    }
}

public static class AuctionSellCommodityCodec
{
    /// <remarks>The item count is six bits, so the list is capped at 63 by the wire format.</remarks>
    public static void Read(ref SpanPacketReader r, out AuctionSellCommodity packet)
    {
        WowGuid128 auctioneer = r.ReadPackedGuid128();
        ulong unitPrice = r.ReadUInt64();
        uint runTime = r.ReadUInt32();

        bool hasTaintedBy = r.HasBit();
        uint itemCount = r.ReadBits<uint>(6);
        r.ResetBitReader();

        if (hasTaintedBy)
            new AddOnInfo().Read(ref r);

        var items = new List<AuctionItemForSale>((int)itemCount);
        for (uint i = 0; i < itemCount; ++i)
        {
            WowGuid128 guid = r.ReadPackedGuid128();
            items.Add(new AuctionItemForSale(guid, r.ReadUInt32()));
        }

        packet = new AuctionSellCommodity(auctioneer, unitPrice, runTime, items);
    }
}

public static class AuctionGetCommodityQuoteCodec
{
    public static void Read(ref SpanPacketReader r, out AuctionGetCommodityQuote packet)
    {
        WowGuid128 auctioneer = r.ReadPackedGuid128();
        uint itemId = r.ReadUInt32();
        uint quantity = r.ReadUInt32();

        if (r.HasBit())
            new AddOnInfo().Read(ref r);

        packet = new AuctionGetCommodityQuote(auctioneer, itemId, quantity);
    }
}

public static class AuctionConfirmCommoditiesPurchaseCodec
{
    public static void Read(ref SpanPacketReader r, out AuctionConfirmCommoditiesPurchase packet)
    {
        WowGuid128 auctioneer = r.ReadPackedGuid128();
        uint itemId = r.ReadUInt32();
        uint quantity = r.ReadUInt32();

        if (r.HasBit())
            new AddOnInfo().Read(ref r);

        packet = new AuctionConfirmCommoditiesPurchase(auctioneer, itemId, quantity);
    }
}

public static class AuctionCancelCommoditiesPurchaseCodec
{
    public static void Read(ref SpanPacketReader r, out AuctionCancelCommoditiesPurchase packet)
    {
        WowGuid128 auctioneer = r.ReadPackedGuid128();

        if (r.HasBit())
            new AddOnInfo().Read(ref r);

        packet = new AuctionCancelCommoditiesPurchase(auctioneer);
    }
}
