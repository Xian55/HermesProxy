using System;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// 4.4.2 CMSG codecs whose shape adds a field to the 3.4.3 one (docs/protocol, the client's own
/// writer, and TrinityCore cata_classic agree on each). Each body is written the way the 4.4.2
/// client writes it, and the codec must land on every field and consume all of it.
/// </summary>
public sealed class CataClassicCodecTests
{
    private static readonly WowGuid128 Npc = new(0x155A, 0x2000040000004AC0);

    // Frame like the wire: WorldPacket's read-mode ctor consumes a 2-byte opcode prefix.
    private static byte[] Framed(Action<WorldPacket> write)
    {
        using var w = new WorldPacket(1u);
        write(w);
        byte[] payload = w.GetData();
        byte[] framed = new byte[payload.Length + 2];
        payload.CopyTo(framed, 2);
        return framed;
    }

    private static SpanPacketReader ReaderOver(byte[] framed)
        => new(new WorldPacket(framed).GetRemainingSpan());

    // Captured from the 4.4.2 client: "gooday" whispered to Mirabilis.
    [Fact]
    public void ChatMessageWhisper_CataClassic_ReadsACapturedWhisper()
    {
        byte[] payload = Convert.FromHexString(
            "07000000" + "03A095210408" + "01000101" + "050070" + "4D69726162696C697300" + "676F6F64617900");
        byte[] framed = new byte[payload.Length + 2];
        payload.CopyTo(framed, 2);

        var r = ReaderOver(framed);
        ChatMessageWhisperCodecCataClassic.Read(ref r, out var packet);
        Assert.Equal(7u, packet.Language);
        Assert.Equal("Mirabilis", packet.Target);
        Assert.Equal("gooday", packet.Text);
        Assert.False(packet.TargetGUID.IsEmpty());
        Assert.Equal(0, r.Remaining);
    }

    // Captured from the 4.4.2 client: C_ChatInfo.SendAddonMessage("HP", "hello", "WHISPER",
    // "Elpriest"). No channel, the target by GUID and by name, the name's length with its NUL.
    [Fact]
    public void ChatAddonMessageTargeted_CataClassic_ReadsACapturedWhisper()
    {
        byte[] payload = Convert.FromHexString(
            "1028" + "07000000" + "4850" + "68656C6C6F" + "0000" + "03A025220408" + "01000101"
            + "048000" + "456C70726965737400");
        byte[] f = new byte[payload.Length + 2];
        payload.CopyTo(f, 2);

        var r = ReaderOver(f);
        ChatAddonMessageTargetedCodecCataClassic.Read(ref r, out var packet);
        Assert.Equal("HP", packet.Params.Prefix);
        Assert.Equal("hello", packet.Params.Text);
        Assert.Equal(ChatMessageTypeModern.Whisper, packet.Params.Type);
        Assert.Equal("Elpriest", packet.Target);
        Assert.True(packet.ChannelGuid.IsEmpty());
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void AutoBankItem_CataClassic_SkipsTheBankType()
    {
        var f = Framed(w =>
        {
            w.WriteBits(0u, 2);                     // InvUpdate: no items
            w.FlushBits();
            w.WriteUInt8(0);                        // BankType
            w.WriteUInt8(255);                      // Bag
            w.WriteUInt8(23);                       // Slot
        });

        var r = ReaderOver(f);
        AutoBankItemCodecCataClassic.Read(ref r, out var packet);
        Assert.Equal(255, packet.PackSlot);
        Assert.Equal(23, packet.Slot);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void AuctionRemoveItem_CataClassic_SkipsTheItemId()
    {
        var f = Framed(w =>
        {
            w.WritePackedGuid128(Npc);
            w.WriteUInt32(4711);                    // AuctionID
            w.WriteInt32(2589);                     // ItemID
            w.WriteBit(false);                      // TaintedBy
            w.FlushBits();
        });

        var r = ReaderOver(f);
        AuctionRemoveItemCodecCataClassic.Read(ref r, out var packet);
        Assert.Equal(Npc, packet.Auctioneer);
        Assert.Equal(4711u, packet.AuctionID);
        Assert.Null(packet.TaintedBy);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void AuctionListBidderItems_CataClassic_ReadsTheTaintBitBeforeTheCount()
    {
        var f = Framed(w =>
        {
            w.WritePackedGuid128(Npc);
            w.WriteUInt32(50);                      // Offset
            w.WriteBit(false);                      // TaintedBy
            w.WriteBits(2u, 7);                     // auction ids
            w.WriteBits(1u, 2);                     // sorts
            w.FlushBits();
            w.WriteUInt32(101);
            w.WriteUInt32(102);
            w.WriteUInt8(4);                        // SortOrder
            w.WriteBit(true);                       // ReverseSort
            w.FlushBits();
        });

        var r = ReaderOver(f);
        AuctionListBidderItemsCodecCataClassic.Read(ref r, out var packet);
        Assert.Equal(Npc, packet.Auctioneer);
        Assert.Equal(50u, packet.Offset);
        Assert.Equal([101u, 102u], packet.AuctionItemIDs);
        Assert.Equal(0, r.Remaining);
    }

    // ---- the retail-style auction house: written the way TrinityCore cata_classic reads each ----

    private static void WriteSort(WorldPacket w, byte order, bool reverse)
    {
        w.WriteUInt8(order);
        w.WriteBit(reverse);
        w.FlushBits();
    }

    private static void WriteKey(WorldPacket w, uint itemId, ushort itemLevel)
    {
        w.WriteBits(itemId, 20);
        w.WriteBit(false);                          // BattlePetSpeciesID
        w.WriteBits((uint)itemLevel, 11);
        w.WriteBit(false);                          // ItemSuffix
        w.FlushBits();
    }

    [Fact]
    public void AuctionBrowseQuery_ReadsTheSearchAndItsClassFilters()
    {
        var f = Framed(w =>
        {
            w.WritePackedGuid128(Npc);
            w.WriteUInt32(50);                      // Offset
            w.WriteUInt8(10);                       // MinLevel
            w.WriteUInt8(20);                       // MaxLevel
            w.WriteUInt8(0);                        // Unused1007_1
            w.WriteUInt8(0);                        // Unused1007_2
            w.WriteUInt32(0x24);                    // Filters: UsableOnly | ExactMatch
            w.WriteUInt32(2);                       // KnownPets size
            w.WriteUInt8(25);                       // MaxPetLevel
            w.WriteUInt32(0);                       // Unused1026
            w.WriteUInt8(0xAA);                     // KnownPets
            w.WriteUInt8(0xBB);
            w.WriteBit(false);                      // TaintedBy
            w.WriteBits(5u, 8);                     // name length
            w.WriteBits(1u, 3);                     // class filters
            w.WriteBits(1u, 2);                     // sorts
            w.FlushBits();
            w.WriteString("Linen");
            w.WriteInt32(4);                        // ItemClass
            w.WriteBits(2u, 5);                     // subclass filters
            w.FlushBits();
            w.WriteUInt64(0x20);                    // InvTypeMask
            w.WriteInt32(1);                        // ItemSubclass
            w.WriteUInt64(ulong.MaxValue);
            w.WriteInt32(2);
            WriteSort(w, 1, reverse: false);
        });

        var r = ReaderOver(f);
        AuctionBrowseQueryCodec.Read(ref r, out var packet);
        Assert.Equal(Npc, packet.Auctioneer);
        Assert.Equal(50u, packet.Offset);
        Assert.Equal(10, packet.MinLevel);
        Assert.Equal(20, packet.MaxLevel);
        Assert.Equal(AuctionHouseFilterMask.UsableOnly | AuctionHouseFilterMask.ExactMatch, packet.Filters);
        Assert.Equal("Linen", packet.Name);
        ClassFilter armor = Assert.Single(packet.ClassFilters);
        Assert.Equal(4, armor.ItemClass);
        Assert.Equal([new SubClassFilter(1, 0x20), new SubClassFilter(2, uint.MaxValue)], armor.SubClassFilters);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void AuctionListItemsByBucketKey_ReadsTheKeyBetweenTheBitsAndTheSorts()
    {
        var f = Framed(w =>
        {
            w.WritePackedGuid128(Npc);
            w.WriteUInt32(0);                       // Offset
            w.WriteUInt8(1);                        // Unknown830
            w.WriteBit(false);                      // TaintedBy
            w.WriteBits(1u, 2);                     // sorts
            w.FlushBits();
            WriteKey(w, 2589, 5);
            WriteSort(w, 0, reverse: true);
        });

        var r = ReaderOver(f);
        AuctionListItemsByBucketKeyCodec.Read(ref r, out var packet);
        Assert.Equal(Npc, packet.Auctioneer);
        Assert.Equal(0u, packet.Offset);
        Assert.Equal(2589u, packet.BucketKey.ItemID);
        Assert.Equal(5, packet.BucketKey.ItemLevel);
        Assert.Null(packet.BucketKey.SuffixItemNameDescriptionID);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void AuctionListItemsByItemID_ReadsTheItemBeforeTheOffset()
    {
        var f = Framed(w =>
        {
            w.WritePackedGuid128(Npc);
            w.WriteUInt32(2589);                    // ItemID
            w.WriteUInt32(0);                       // SuffixItemNameDescriptionID
            w.WriteUInt32(50);                      // Offset
            w.WriteBit(false);                      // TaintedBy
            w.WriteBits(2u, 2);                     // sorts
            w.FlushBits();
            WriteSort(w, 0, reverse: false);
            WriteSort(w, 3, reverse: true);
        });

        var r = ReaderOver(f);
        AuctionListItemsByItemIDCodec.Read(ref r, out var packet);
        Assert.Equal(new AuctionListItemsByItemID(Npc, 2589, 0, 50), packet);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void AuctionListBucketsByBucketKeys_ReadsEveryKey()
    {
        var f = Framed(w =>
        {
            w.WritePackedGuid128(Npc);
            w.WriteBit(false);                      // TaintedBy
            w.WriteBits(2u, 7);                     // keys
            w.WriteBits(1u, 2);                     // sorts
            w.FlushBits();
            WriteKey(w, 2589, 5);
            WriteKey(w, 2770, 10);
            WriteSort(w, 1, reverse: false);
        });

        var r = ReaderOver(f);
        AuctionListBucketsByBucketKeysCodec.Read(ref r, out var packet);
        Assert.Equal(Npc, packet.Auctioneer);
        Assert.Equal([2589u, 2770u], packet.BucketKeys.ConvertAll(k => k.ItemID));
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void AuctionSellCommodity_ReadsTheStacksOffered()
    {
        var stack = new WowGuid128(77, 0x1000040000000000);
        var f = Framed(w =>
        {
            w.WritePackedGuid128(Npc);
            w.WriteUInt64(125);                     // UnitPrice
            w.WriteUInt32(1440);                    // RunTime
            w.WriteBit(false);                      // TaintedBy
            w.WriteBits(1u, 6);                     // items
            w.FlushBits();
            w.WritePackedGuid128(stack);
            w.WriteUInt32(12);                      // UseCount
        });

        var r = ReaderOver(f);
        AuctionSellCommodityCodec.Read(ref r, out var packet);
        Assert.Equal(Npc, packet.Auctioneer);
        Assert.Equal(125ul, packet.UnitPrice);
        Assert.Equal(1440u, packet.RunTime);
        Assert.Equal([new AuctionItemForSale(stack, 12)], packet.Items);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void AuctionCommodityPurchase_QuoteConfirmAndCancel()
    {
        var f = Framed(w =>
        {
            w.WritePackedGuid128(Npc);
            w.WriteUInt32(2770);                    // ItemID
            w.WriteUInt32(8);                       // Quantity
            w.WriteBit(false);                      // TaintedBy
            w.FlushBits();
        });

        var r = ReaderOver(f);
        AuctionGetCommodityQuoteCodec.Read(ref r, out var quote);
        Assert.Equal(new AuctionGetCommodityQuote(Npc, 2770, 8), quote);
        Assert.Equal(0, r.Remaining);

        r = ReaderOver(f);
        AuctionConfirmCommoditiesPurchaseCodec.Read(ref r, out var confirm);
        Assert.Equal(new AuctionConfirmCommoditiesPurchase(Npc, 2770, 8), confirm);
        Assert.Equal(0, r.Remaining);

        var cancelBytes = Framed(w =>
        {
            w.WritePackedGuid128(Npc);
            w.WriteBit(false);                      // TaintedBy
            w.FlushBits();
        });
        r = ReaderOver(cancelBytes);
        AuctionCancelCommoditiesPurchaseCodec.Read(ref r, out var cancel);
        Assert.Equal(Npc, cancel.Auctioneer);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void AuctionListOwnerItems_CataClassic_SkipsTheSorts()
    {
        var f = Framed(w =>
        {
            w.WritePackedGuid128(Npc);
            w.WriteUInt32(0);                       // Offset
            w.WriteBit(false);                      // TaintedBy
            w.WriteBits(2u, 2);                     // sorts
            w.FlushBits();
            for (int i = 0; i < 2; i++)
            {
                w.WriteUInt8((byte)i);
                w.WriteBit(false);
                w.FlushBits();
            }
        });

        var r = ReaderOver(f);
        AuctionListOwnerItemsCodecCataClassic.Read(ref r, out var packet);
        Assert.Equal(Npc, packet.Auctioneer);
        Assert.Equal(0u, packet.Offset);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void GuildSetRankPermissions_CataClassic_ReadsAByteRankEightTabsAndTheOldFlagsLast()
    {
        var f = Framed(w =>
        {
            w.WriteUInt8(3);                        // RankID
            w.WriteUInt32(3);                       // RankOrder
            w.WriteUInt32(0x1F);                    // Flags
            w.WriteInt32(500);                      // WithdrawGoldLimit
            for (uint tab = 0; tab < 8; tab++)
            {
                w.WriteUInt32(tab + 1);             // TabFlags
                w.WriteUInt32((tab + 1) * 10);      // TabWithdrawItemLimit
            }
            w.WriteBits(7u, 7);                     // rank name length
            w.FlushBits();
            w.WriteString("Officer");
            w.WriteUInt32(0x0F);                    // OldFlags
        });
        Assert.Equal(82 + 7 + 2, f.Length);         // the client's fixed part is 82 bytes

        var r = ReaderOver(f);
        GuildSetRankPermissionsCodecCataClassic.Read(ref r, out var packet);
        Assert.Equal(3u, packet.RankID);
        Assert.Equal(3u, packet.RankOrder);
        Assert.Equal(0x1Fu, packet.Flags);
        Assert.Equal(500, packet.WithdrawGoldLimit);
        for (int tab = 0; tab < 8; tab++)
        {
            Assert.Equal((uint)tab + 1, packet.TabFlags[tab]);
            Assert.Equal(((uint)tab + 1) * 10, packet.TabWithdrawItemLimit[tab]);
        }
        Assert.Equal("Officer", packet.RankName);
        Assert.Equal(0x0Fu, packet.OldFlags);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void AlterAppearance_CataClassic_SkipsTheUnalteredVisualRace()
    {
        var f = Framed(w =>
        {
            w.WriteUInt32(1);                       // customization count
            w.WriteUInt8(1);                        // NewSex
            w.WriteUInt32(1);                       // CustomizedRace
            w.WriteUInt32(49);                      // CustomizedChrModelID
            w.WriteInt32(1);                        // UnalteredVisualRaceID
            w.WriteUInt32(17);                      // ChrCustomizationOptionID
            w.WriteUInt32(150);                     // ChrCustomizationChoiceID
        });

        var r = ReaderOver(f);
        AlterAppearanceCodecCataClassic.Read(ref r, out var packet);
        Assert.Equal((Gender)1, packet.NewSexId);
        Assert.Equal((Race)1, packet.CustomizedRace);
        Assert.Equal(49u, packet.CustomizedChrModelId);
        var choice = Assert.Single(packet.Customizations);
        Assert.Equal(new ChrCustomizationChoice(17, 150), choice);
        Assert.Equal(0, r.Remaining);
    }
}
