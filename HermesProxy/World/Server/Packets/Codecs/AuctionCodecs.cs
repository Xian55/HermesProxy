using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;

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
