using System;
using System.Collections.Generic;
using System.Linq;
using Framework.IO;
using HermesProxy;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;
using Frozen = HermesProxy.Tests.World.Dispatch.Reference.FrozenPackets;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Equivalence for the auction codecs against the frozen <c>Read()</c> bodies they replaced.
/// </summary>
/// <remarks>
/// <c>AuctionListItems</c> became a ranged pair, and its two arms are not near-identical: they
/// disagree on field order, on the width of the known-pet count, on <c>SubClassFilter</c>'s field
/// order and the width of <c>InvTypeMask</c>, and only V3_4_3 carries a tainted-addon block and a
/// sort-data size. The frozen oracle still contains the original branch, so each arm is asserted
/// against the wire it actually claims and the two are additionally shown to disagree — otherwise
/// a pair where both arms did the same thing would pass everything here.
/// </remarks>
public class AuctionCodecEquivalenceTests
{
    static AuctionCodecEquivalenceTests()
    {
        if (VersionBootstrap.ModernBuild == ClientVersionBuild.Zero)
            VersionBootstrap.ModernBuild = ClientVersionBuild.V1_14_2_42597;
        if (VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            VersionBootstrap.LegacyBuild = ClientVersionBuild.V3_3_5a_12340;
    }

    private static (WorldPacket Oracle, byte[] Framed) Build(Action<WorldPacket> write)
    {
        using var w = new WorldPacket(1u);
        write(w);
        byte[] payload = w.GetData();
        byte[] framed = new byte[payload.Length + 2];
        payload.CopyTo(framed, 2);
        return (new WorldPacket(framed), framed);
    }

    private static SpanPacketReader ReaderOver(byte[] framed)
        => new(new WorldPacket(framed).GetRemainingSpan());

    private static readonly WowGuid128 Auctioneer = new(0xDEADBEEFCAFEUL, 0x0123456789ABCDEFUL);
    private static readonly WowGuid128 ItemGuid = new(0x1122334455667788UL, 0x99AABBCCDDEEFF00UL);

    // ---- bid list ----

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(127)]   // the 7-bit count ceiling
    public void AuctionListBidderItems_Matches(int idCount)
    {
        uint[] ids = Enumerable.Range(1, idCount).Select(i => (uint)i * 3u).ToArray();

        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Auctioneer);
            w.WriteUInt32(42);
            w.WriteBits((uint)ids.Length, 7);
            w.FlushBits();
            foreach (uint id in ids)
                w.WriteUInt32(id);
        });

        var e = new Frozen.AuctionListBidderItems(); e.Read(o);
        var r = ReaderOver(f); AuctionListBidderItemsCodec.Read(ref r, out var a);

        Assert.Equal(e.Auctioneer, a.Auctioneer);
        Assert.Equal(e.Offset, a.Offset);
        Assert.Equal(e.AuctionItemIDs, a.AuctionItemIDs);
        Assert.Equal(ids, a.AuctionItemIDs.ToArray());
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- the ranged pair ----

    /// <summary>Writes the wire in the shape the pre-WotLK arm expects.</summary>
    private static (WorldPacket Oracle, byte[] Framed) BuildListItemsPreWotLK(
        string name, int classFilters, int subFilters, int sorts, int knownPets)
    {
        return Build(w =>
        {
            w.WriteUInt32(17);                       // Offset first on this layout
            w.WritePackedGuid128(Auctioneer);
            w.WriteUInt8(10);                        // MinLevel
            w.WriteUInt8(60);                        // MaxLevel
            w.WriteInt32(3);                         // Quality
            w.WriteUInt8((byte)sorts);
            w.WriteUInt32((uint)knownPets);          // uint32 here, int32 on V3_4_3
            w.WriteUInt8(25);                        // MaxPetLevel
            for (int i = 0; i < knownPets; i++)
                w.WriteUInt8((byte)(i + 1));

            w.WriteBits((uint)name.Length, 8);
            w.WriteString(name);
            w.WriteBits((uint)classFilters, 3);
            w.WriteBit(true);                        // OnlyUsable
            w.WriteBit(false);                       // ExactMatch
            w.FlushBits();

            for (int i = 0; i < classFilters; i++)
            {
                w.WriteInt32(4 + i);                 // ItemClass
                w.WriteBits((uint)subFilters, 5);
                w.FlushBits();
                for (int j = 0; j < subFilters; j++)
                {
                    w.WriteInt32(j);                 // ItemSubclass FIRST on this layout
                    w.WriteUInt32(1u << j);          // InvTypeMask, 32 bits
                }
            }

            w.WriteUInt32((uint)(sorts * 2));        // sort blob size
            for (int i = 0; i < sorts; i++)
            {
                w.WriteUInt8((byte)(i + 1));         // Type
                w.WriteUInt8((byte)(i % 2));         // Direction
            }
        });
    }

    [Theory]
    [InlineData("", 0, 0, 0, 0)]
    [InlineData("Thunderfury", 0, 0, 0, 0)]
    [InlineData("cloth", 1, 1, 0, 0)]
    [InlineData("cloth", 1, 3, 2, 0)]
    [InlineData("pet", 2, 1, 1, 3)]
    public void AuctionListItems_PreWotLKClassic_Matches(
        string name, int classFilters, int subFilters, int sorts, int knownPets)
    {
        var (o, f) = BuildListItemsPreWotLK(name, classFilters, subFilters, sorts, knownPets);

        var e = new Frozen.AuctionListItems(); e.Read(o);
        var r = ReaderOver(f); AuctionListItemsCodecPreWotLKClassic.Read(ref r, out var a);

        Assert.Equal(e.Offset, a.Offset);
        Assert.Equal(e.Auctioneer, a.Auctioneer);
        Assert.Equal(e.MinLevel, a.MinLevel);
        Assert.Equal(e.MaxLevel, a.MaxLevel);
        Assert.Equal(e.Quality, a.Quality);
        Assert.Equal(e.MaxPetLevel, a.MaxPetLevel);
        Assert.Equal(e.KnownPets, a.KnownPets);
        Assert.Equal(e.Name, a.Name);
        Assert.Equal(e.OnlyUsable, a.OnlyUsable);
        Assert.Equal(e.ExactMatch, a.ExactMatch);
        Assert.Equal(name, a.Name);

        Assert.Equal(e.ClassFilters.Count, a.ClassFilters.Count);
        for (int i = 0; i < a.ClassFilters.Count; i++)
        {
            Assert.Equal(e.ClassFilters[i].ItemClass, a.ClassFilters[i].ItemClass);
            Assert.Equal(e.ClassFilters[i].SubClassFilters.Count, a.ClassFilters[i].SubClassFilters.Count);
            for (int j = 0; j < a.ClassFilters[i].SubClassFilters.Count; j++)
            {
                Assert.Equal(e.ClassFilters[i].SubClassFilters[j].ItemSubclass,
                             a.ClassFilters[i].SubClassFilters[j].ItemSubclass);
                Assert.Equal(e.ClassFilters[i].SubClassFilters[j].InvTypeMask,
                             a.ClassFilters[i].SubClassFilters[j].InvTypeMask);
            }
        }

        Assert.Equal(e.Sorts.Count, a.Sorts.Count);
        for (int i = 0; i < a.Sorts.Count; i++)
        {
            Assert.Equal(e.Sorts[i].Type, a.Sorts[i].Type);
            Assert.Equal(e.Sorts[i].Direction, a.Sorts[i].Direction);
        }

        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>Writes the wire in the shape the V3_4_3 arm expects — a genuinely different layout.</summary>
    private static byte[] BuildListItemsWotLK(
        string name, int classFilters, int subFilters, int sorts, int knownPets, bool tainted)
    {
        var (_, framed) = Build(w =>
        {
            w.WritePackedGuid128(Auctioneer);        // Auctioneer FIRST on this layout
            w.WriteUInt32(17);                       // Offset
            w.WriteUInt8(10);                        // MinLevel
            w.WriteUInt8(60);                        // MaxLevel
            w.WriteInt32(3);                         // Quality
            w.WriteUInt8((byte)sorts);
            w.WriteInt32(knownPets);                 // int32 here, uint32 on the old layout
            w.WriteInt8(25);                         // MaxPetLevel, signed here
            for (int i = 0; i < knownPets; i++)
                w.WriteUInt8((byte)(i + 1));

            w.WriteBit(tainted);
            w.WriteBits((uint)name.Length, 8);
            w.WriteString(name);

            w.FlushBits();
            w.WriteBits((uint)classFilters, 3);
            w.WriteBit(true);                        // OnlyUsable
            w.WriteBit(false);                       // ExactMatch

            if (tainted)
            {
                w.FlushBits();
                w.WriteBits(1u, 10);                 // addon name length (<=1 means absent)
                w.WriteBits(1u, 10);                 // addon version length
                w.WriteBit(true);                    // Loaded
                w.WriteBit(false);                   // Disabled
                w.FlushBits();
            }

            for (int i = 0; i < classFilters; i++)
            {
                w.WriteInt32(4 + i);                 // ItemClass
                w.WriteBits((uint)subFilters, 5);
                w.FlushBits();
                for (int j = 0; j < subFilters; j++)
                {
                    w.WriteUInt64(1ul << j);         // InvTypeMask FIRST, and 64 bits wide
                    w.WriteInt32(j);                 // ItemSubclass
                }
            }

            w.WriteInt32(sorts * 2);                 // sortDataSize
            for (int i = 0; i < sorts; i++)
            {
                w.WriteUInt8((byte)(i + 1));         // Type
                w.WriteUInt8((byte)(i % 2));         // Direction
            }
        });
        return framed;
    }

    [Theory]
    [InlineData("", 0, 0, 0, 0, false)]
    [InlineData("Thunderfury", 0, 0, 0, 0, false)]
    [InlineData("cloth", 1, 1, 0, 0, false)]
    [InlineData("cloth", 1, 2, 2, 0, false)]
    [InlineData("pet", 2, 1, 1, 3, false)]
    [InlineData("addon", 1, 1, 1, 0, true)]
    public void AuctionListItems_WotLKClassic_ReadsItsOwnLayout(
        string name, int classFilters, int subFilters, int sorts, int knownPets, bool tainted)
    {
        byte[] f = BuildListItemsWotLK(name, classFilters, subFilters, sorts, knownPets, tainted);

        var r = ReaderOver(f);
        AuctionListItemsCodecWotLKClassic.Read(ref r, out var a);

        Assert.Equal(Auctioneer, a.Auctioneer);
        Assert.Equal(17u, a.Offset);
        Assert.Equal((byte)10, a.MinLevel);
        Assert.Equal((byte)60, a.MaxLevel);
        Assert.Equal(3, a.Quality);
        Assert.Equal((byte)25, a.MaxPetLevel);
        Assert.Equal(knownPets, a.KnownPets.Count);
        Assert.Equal(name, a.Name);
        Assert.True(a.OnlyUsable);
        Assert.False(a.ExactMatch);

        Assert.Equal(classFilters, a.ClassFilters.Count);
        for (int i = 0; i < classFilters; i++)
        {
            Assert.Equal(4 + i, a.ClassFilters[i].ItemClass);
            Assert.Equal(subFilters, a.ClassFilters[i].SubClassFilters.Count);
            for (int j = 0; j < subFilters; j++)
            {
                Assert.Equal(j, a.ClassFilters[i].SubClassFilters[j].ItemSubclass);
                Assert.Equal(1u << j, a.ClassFilters[i].SubClassFilters[j].InvTypeMask);
            }
        }

        Assert.Equal(sorts, a.Sorts.Count);
        for (int i = 0; i < sorts; i++)
        {
            Assert.Equal((byte)(i + 1), a.Sorts[i].Type);
            Assert.Equal((byte)(i % 2), a.Sorts[i].Direction);
        }

        Assert.Equal(0, r.Remaining);
    }

    /// <summary>
    /// The two arms must genuinely differ. Feeding the V3_4_3 wire to the pre-WotLK arm has to
    /// produce something else — if it did not, the pair would be ceremony and every assertion above
    /// would hold for a single shared codec.
    /// </summary>
    [Fact]
    public void AuctionListItems_ArmsDisagreeOnTheSameBytes()
    {
        byte[] wotlkWire = BuildListItemsWotLK("cloth", 1, 1, 1, 0, tainted: false);

        var r1 = ReaderOver(wotlkWire);
        AuctionListItemsCodecWotLKClassic.Read(ref r1, out var viaWotLK);

        AuctionListItems viaOld = default;
        bool oldThrew = false;
        try
        {
            var r2 = ReaderOver(wotlkWire);
            AuctionListItemsCodecPreWotLKClassic.Read(ref r2, out viaOld);
        }
        catch (Exception)
        {
            oldThrew = true;
        }

        // Auctioneer and Offset swap places between the layouts, so the old arm cannot agree.
        Assert.True(oldThrew || viaOld.Auctioneer != viaWotLK.Auctioneer || viaOld.Offset != viaWotLK.Offset);
    }

    // ---- the three carrying an optional AddOnInfo ----

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(false, 3)]
    [InlineData(true, 1)]
    public void AuctionSellItem_Matches(bool tainted, int itemCount)
    {
        int itemCountBits = ModernVersion.AddedInClassicVersion(1, 14, 3, 2, 5, 4) ? 6 : 5;

        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Auctioneer);
            w.WriteUInt64(5_000_000_000ul);          // MinBid, above uint.MaxValue on purpose
            w.WriteUInt64(9_000_000_000ul);          // BuyoutPrice
            w.WriteUInt32(1440);                     // ExpireTime
            w.WriteBit(tainted);
            w.WriteBits((uint)itemCount, itemCountBits);
            w.FlushBits();

            if (tainted)
            {
                w.WriteBits(1u, 10);                 // addon name length
                w.WriteBits(1u, 10);                 // addon version length
                w.WriteBit(true);                    // Loaded
                w.WriteBit(false);                   // Disabled
                w.FlushBits();
            }

            for (int i = 0; i < itemCount; i++)
            {
                w.WritePackedGuid128(ItemGuid);
                w.WriteUInt32((uint)(i + 1));
            }
        });

        var e = new Frozen.AuctionSellItem(); e.Read(o);
        var r = ReaderOver(f); AuctionSellItemCodec.Read(ref r, out var a);

        Assert.Equal(e.Auctioneer, a.Auctioneer);
        Assert.Equal(e.MinBid, a.MinBid);
        Assert.Equal(e.BuyoutPrice, a.BuyoutPrice);
        Assert.Equal(e.ExpireTime, a.ExpireTime);
        Assert.Equal(e.TaintedBy != null, a.TaintedBy != null);
        Assert.Equal(tainted, a.TaintedBy != null);

        // The money fields exceed uint.MaxValue, so a 32-bit read anywhere would show up here.
        Assert.Equal(5_000_000_000ul, a.MinBid);
        Assert.Equal(9_000_000_000ul, a.BuyoutPrice);

        Assert.Equal(e.Items.Count, a.Items.Count);
        for (int i = 0; i < a.Items.Count; i++)
        {
            Assert.Equal(e.Items[i].Guid, a.Items[i].Guid);
            Assert.Equal(e.Items[i].UseCount, a.Items[i].UseCount);
        }
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AuctionRemoveItem_Matches(bool tainted)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Auctioneer);
            w.WriteUInt32(99);
            w.WriteBit(tainted);
            w.FlushBits();
            if (tainted)
            {
                w.WriteBits(1u, 10);
                w.WriteBits(1u, 10);
                w.WriteBit(true);
                w.WriteBit(false);
                w.FlushBits();
            }
        });

        var e = new Frozen.AuctionRemoveItem(); e.Read(o);
        var r = ReaderOver(f); AuctionRemoveItemCodec.Read(ref r, out var a);

        Assert.Equal(e.Auctioneer, a.Auctioneer);
        Assert.Equal(e.AuctionID, a.AuctionID);
        Assert.Equal(e.TaintedBy != null, a.TaintedBy != null);
        Assert.Equal(99u, a.AuctionID);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AuctionPlaceBid_Matches(bool tainted)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Auctioneer);
            w.WriteUInt32(1234);
            w.WriteUInt64(7_000_000_000ul);          // above uint.MaxValue
            w.WriteBit(tainted);
            w.FlushBits();
            if (tainted)
            {
                w.WriteBits(1u, 10);
                w.WriteBits(1u, 10);
                w.WriteBit(true);
                w.WriteBit(false);
                w.FlushBits();
            }
        });

        var e = new Frozen.AuctionPlaceBid(); e.Read(o);
        var r = ReaderOver(f); AuctionPlaceBidCodec.Read(ref r, out var a);

        Assert.Equal(e.Auctioneer, a.Auctioneer);
        Assert.Equal(e.AuctionID, a.AuctionID);
        Assert.Equal(e.BidAmount, a.BidAmount);
        Assert.Equal(e.TaintedBy != null, a.TaintedBy != null);
        Assert.Equal(7_000_000_000ul, a.BidAmount);
    }
}
