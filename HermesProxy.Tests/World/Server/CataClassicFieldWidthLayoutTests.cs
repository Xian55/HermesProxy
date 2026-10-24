using System;
using System.Buffers;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// Packets whose 4.4.2 shape only changes a field's width, or adds one field (docs/protocol, the
/// clients' own readers, and TrinityCore cata_classic agree on each). Every old layout is pinned
/// to the writer it replaced, written out again here; every new one to the bytes the 4.4.2
/// client reads.
/// </summary>
public sealed class CataClassicFieldWidthLayoutTests
{
    private static readonly WowGuid128 Player = new(3039, 0x0800040000000000);
    private static readonly WowGuid128 Creature = new(0x155A, 0x2000040000004AC0);

    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.IsType<ItemPushResult.IntQualityLayout>(ItemPushResult.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<ItemPushResult.ByteQualityLayout>(ItemPushResult.Layouts.For(ClientVersionBuild.V4_4_2_60895));
        Assert.IsType<StartMirrorTimer.IntTimerLayout>(StartMirrorTimer.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<StartMirrorTimer.ByteTimerLayout>(StartMirrorTimer.Layouts.For(ClientVersionBuild.V4_4_2_60895));
        Assert.IsType<PauseMirrorTimer.IntTimerLayout>(PauseMirrorTimer.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<PauseMirrorTimer.ByteTimerLayout>(PauseMirrorTimer.Layouts.For(ClientVersionBuild.V4_4_2_60895));
        Assert.IsType<StopMirrorTimer.IntTimerLayout>(StopMirrorTimer.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<StopMirrorTimer.ByteTimerLayout>(StopMirrorTimer.Layouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    // ---- SMSG_ITEM_PUSH_RESULT ----

    private static ItemPushResult Push() => new()
    {
        PlayerGUID = Player,
        Slot = 255,
        SlotInBag = 3,
        Quantity = 2,
        QuantityInInventory = 5,
        BattlePetBreedQuality = 1,
        ItemGUID = new WowGuid128(77, 0x4000000000000000),
        Pushed = true,
        DisplayText = ItemPushResult.DisplayType.Received,
        IsEncounterLoot = true,
        Item = new ItemInstance { ItemID = 2589 },
    };

    [Fact]
    public void ItemPush_IntQualityLayout_MatchesTheOldWriter()
    {
        var p = Push();
        using var old = new WorldPacket(1u);
        old.WritePackedGuid128(p.PlayerGUID);
        old.WriteUInt8(p.Slot);
        old.WriteInt32(p.SlotInBag);
        old.WriteInt32(p.QuestLogItemID);
        old.WriteUInt32(p.Quantity);
        old.WriteUInt32(p.QuantityInInventory);
        old.WriteInt32(p.DungeonEncounterID);
        old.WriteInt32(p.BattlePetSpeciesID);
        old.WriteInt32(p.BattlePetBreedID);
        old.WriteUInt32(p.BattlePetBreedQuality);
        old.WriteInt32(p.BattlePetLevel);
        old.WritePackedGuid128(p.ItemGUID);
        old.WriteBit(p.Pushed);
        old.WriteBit(p.Created);
        old.WriteBits((uint)p.DisplayText, 3);
        old.WriteBit(p.IsBonusRoll);
        old.WriteBit(p.IsEncounterLoot);
        old.FlushBits();
        p.Item.Write(old);

        AssertBothSerialisers(new ItemPushResult.IntQualityLayout(), p, p.MaxSize, old);
    }

    [Fact]
    public void ItemPush_ByteQualityLayout_WritesAByteQualityAndTheUnusedBit()
    {
        var p = Push();
        using var expected = new WorldPacket(1u);
        expected.WritePackedGuid128(p.PlayerGUID);
        expected.WriteUInt8(p.Slot);
        expected.WriteInt32(p.SlotInBag);
        expected.WriteInt32(p.QuestLogItemID);
        expected.WriteUInt32(p.Quantity);
        expected.WriteUInt32(p.QuantityInInventory);
        expected.WriteInt32(p.DungeonEncounterID);
        expected.WriteInt32(p.BattlePetSpeciesID);
        expected.WriteInt32(p.BattlePetBreedID);
        expected.WriteUInt8(1);                     // BattlePetBreedQuality
        expected.WriteInt32(p.BattlePetLevel);
        expected.WritePackedGuid128(p.ItemGUID);
        // Pushed, Created, unused, DisplayText (3), IsBonusRoll, IsEncounterLoot
        expected.WriteUInt8(0b1000_0101);
        p.Item.Write(expected);

        AssertBothSerialisers(new ItemPushResult.ByteQualityLayout(), p, p.MaxSize, expected);
    }

    // ---- SMSG_START_MIRROR_TIMER / SMSG_PAUSE_MIRROR_TIMER / SMSG_STOP_MIRROR_TIMER ----

    private static StartMirrorTimer Breath() => new()
    {
        Timer = MirrorTimerType.Breath,
        Value = 180000,
        MaxValue = 180000,
        Scale = -1,
        Paused = true,
    };

    [Fact]
    public void StartMirrorTimer_IntTimerLayout_MatchesTheOldWriter()
    {
        var p = Breath();
        using var old = new WorldPacket(1u);
        old.WriteInt32((int)p.Timer);
        old.WriteInt32(p.Value);
        old.WriteInt32(p.MaxValue);
        old.WriteInt32(p.Scale);
        old.WriteInt32(p.SpellID);
        old.WriteBit(p.Paused);
        old.FlushBits();

        AssertBothSerialisers(new StartMirrorTimer.IntTimerLayout(), p, p.MaxSize, old);
    }

    [Fact]
    public void StartMirrorTimer_ByteTimerLayout_WritesTheTimerAsAByte()
    {
        var p = Breath();
        using var expected = new WorldPacket(1u);
        expected.WriteUInt8((byte)p.Timer);
        expected.WriteInt32(p.Value);
        expected.WriteInt32(p.MaxValue);
        expected.WriteInt32(p.Scale);
        expected.WriteInt32(p.SpellID);
        expected.WriteUInt8(0x80);

        AssertBothSerialisers(new StartMirrorTimer.ByteTimerLayout(), p, p.MaxSize, expected);
    }

    [Fact]
    public void PauseMirrorTimer_IntTimerLayout_MatchesTheOldWriter()
    {
        var p = new PauseMirrorTimer { Timer = MirrorTimerType.Breath, Paused = true };
        using var old = new WorldPacket(1u);
        old.WriteInt32((int)p.Timer);
        old.WriteBit(p.Paused);
        old.FlushBits();

        AssertBothSerialisers(new PauseMirrorTimer.IntTimerLayout(), p, p.MaxSize, old);
    }

    [Fact]
    public void PauseMirrorTimer_ByteTimerLayout_WritesTheTimerAsAByte()
    {
        var p = new PauseMirrorTimer { Timer = MirrorTimerType.Breath, Paused = true };
        using var expected = new WorldPacket(1u);
        expected.WriteUInt8((byte)p.Timer);
        expected.WriteUInt8(0x80);

        AssertBothSerialisers(new PauseMirrorTimer.ByteTimerLayout(), p, p.MaxSize, expected);
    }

    [Fact]
    public void StopMirrorTimer_IntTimerLayout_MatchesTheOldWriter()
    {
        var p = new StopMirrorTimer { Timer = MirrorTimerType.Breath };
        using var old = new WorldPacket(1u);
        old.WriteInt32((int)p.Timer);

        AssertBothSerialisers(new StopMirrorTimer.IntTimerLayout(), p, p.MaxSize, old);
    }

    [Fact]
    public void StopMirrorTimer_ByteTimerLayout_WritesTheTimerAsAByte()
    {
        var p = new StopMirrorTimer { Timer = MirrorTimerType.Breath };
        using var expected = new WorldPacket(1u);
        expected.WriteUInt8((byte)p.Timer);

        AssertBothSerialisers(new StopMirrorTimer.ByteTimerLayout(), p, p.MaxSize, expected);
    }

    // ---- SMSG_CHANNEL_NOTIFY_JOINED ----

    private static ChannelNotifyJoined General() => new()
    {
        Channel = "General - Elwynn Forest",
        ChannelFlags = (ChannelFlags)0x18,
        ChatChannelID = 1,
        ChannelGUID = new WowGuid128(1, 0x3C00000000000000),
    };

    [Fact]
    public void ChannelNotifyJoined_3_4_3_MatchesTheOldWriter()
    {
        var p = General();
        using var old = new WorldPacket(1u);
        old.WriteBits(p.Channel.GetByteCount(), 7);
        old.WriteBits(p.ChannelWelcomeMsg.GetByteCount(), 11);
        old.WriteUInt32((uint)p.ChannelFlags);
        old.WriteInt32(p.ChatChannelID);
        old.WriteUInt64(p.InstanceID);
        old.WritePackedGuid128(p.ChannelGUID);
        old.WriteString(p.Channel);
        old.WriteString(p.ChannelWelcomeMsg);

        AssertBothSerialisers(ChannelNotifyJoined.Layouts.For(ClientVersionBuild.V3_4_3_54261), p, p.MaxSize, old);
    }

    [Fact]
    public void ChannelNotifyJoined_4_4_2_PutsAByteBeforeTheChannelId()
    {
        var p = General();
        using var expected = new WorldPacket(1u);
        expected.WriteBits(p.Channel.GetByteCount(), 7);
        expected.WriteBits(p.ChannelWelcomeMsg.GetByteCount(), 11);
        expected.WriteUInt32((uint)p.ChannelFlags);
        expected.WriteUInt8(0);
        expected.WriteInt32(p.ChatChannelID);
        expected.WriteUInt64(p.InstanceID);
        expected.WritePackedGuid128(p.ChannelGUID);
        expected.WriteString(p.Channel);
        expected.WriteString(p.ChannelWelcomeMsg);

        AssertBothSerialisers(ChannelNotifyJoined.Layouts.For(ClientVersionBuild.V4_4_2_60895), p, p.MaxSize, expected);
    }

    // ---- SMSG_SPELL_ENERGIZE_LOG ----

    private static SpellEnergizeLog Energize() => new()
    {
        TargetGUID = Player,
        CasterGUID = Player,
        SpellID = 2687,
        Type = PowerType.Rage,
        Amount = 200,
    };

    [Fact]
    public void SpellEnergizeLog_3_4_3_MatchesTheOldWriter()
    {
        var p = Energize();
        using var old = new WorldPacket(1u);
        old.WritePackedGuid128(p.TargetGUID);
        old.WritePackedGuid128(p.CasterGUID);
        old.WriteUInt32(p.SpellID);
        old.WriteUInt32((uint)p.Type);
        old.WriteInt32(p.Amount);
        old.WriteInt32(p.OverEnergize);
        old.WriteBit(false);
        old.FlushBits();

        AssertBothSerialisers(SpellEnergizeLog.Layouts.For(ClientVersionBuild.V3_4_3_54261), p, p.MaxSize, old);
    }

    [Fact]
    public void SpellEnergizeLog_4_4_2_WritesThePowerTypeAsAByte()
    {
        var p = Energize();
        using var expected = new WorldPacket(1u);
        expected.WritePackedGuid128(p.TargetGUID);
        expected.WritePackedGuid128(p.CasterGUID);
        expected.WriteUInt32(p.SpellID);
        expected.WriteUInt8((byte)PowerType.Rage);
        expected.WriteInt32(p.Amount);
        expected.WriteInt32(p.OverEnergize);
        expected.WriteUInt8(0);

        AssertBothSerialisers(SpellEnergizeLog.Layouts.For(ClientVersionBuild.V4_4_2_60895), p, p.MaxSize, expected);
    }

    // ---- SMSG_SPELL_EXECUTE_LOG ----

    private static SpellExecuteLog ManaDrain()
    {
        var effect = new SpellExecuteLogEffect { Effect = 8 };
        effect.PowerDrainTargets.Add(new SpellLogEffectPowerDrain
        {
            Victim = Creature,
            Points = 120,
            PowerType = (uint)PowerType.Mana,
            Amplitude = 1.5f,
        });
        var packet = new SpellExecuteLog { Caster = Player, SpellID = 5138 };
        packet.Effects.Add(effect);
        return packet;
    }

    private static void WriteHeadAndCounts(WorldPacket data, SpellExecuteLog p)
    {
        data.WritePackedGuid128(p.Caster);
        data.WriteInt32(p.SpellID);
        data.WriteUInt32(1);
        data.WriteInt32(8);
        data.WriteUInt32(1);
        for (int i = 0; i < 5; i++)
            data.WriteUInt32(0);
        data.WritePackedGuid128(Creature);
        data.WriteUInt32(120);
    }

    [Fact]
    public void SpellExecuteLog_3_4_3_WritesThePowerTypeAsAUInt32()
    {
        var p = ManaDrain();
        using var old = new WorldPacket(1u);
        WriteHeadAndCounts(old, p);
        old.WriteUInt32((uint)PowerType.Mana);
        old.WriteFloat(1.5f);
        old.WriteBit(false);
        old.FlushBits();

        AssertBothSerialisers(SpellExecuteLog.Layouts.For(ClientVersionBuild.V3_4_3_54261), p, p.MaxSize, old);
    }

    [Fact]
    public void SpellExecuteLog_4_4_2_WritesThePowerTypeAsAByte()
    {
        var p = ManaDrain();
        using var expected = new WorldPacket(1u);
        WriteHeadAndCounts(expected, p);
        expected.WriteUInt8((byte)PowerType.Mana);
        expected.WriteFloat(1.5f);
        expected.WriteUInt8(0);

        AssertBothSerialisers(SpellExecuteLog.Layouts.For(ClientVersionBuild.V4_4_2_60895), p, p.MaxSize, expected);
    }

    // ---- SMSG_PARTY_UPDATE ----

    private static PartyUpdate Party() => new()
    {
        PartyFlags = (GroupFlags)0x1,
        PartyType = (GroupType)1,
        MyIndex = 0,
        PartyGUID = new WowGuid128(9, 0x1F00000000000000),
        SequenceNum = 3,
        LeaderGUID = Player,
        LeaderFactionGroup = 1,
    };

    private static void WritePartyHead(WorldPacket data, PartyUpdate p)
    {
        data.WriteUInt16((ushort)p.PartyFlags);
        data.WriteUInt8(p.PartyIndex);
        data.WriteUInt8((byte)p.PartyType);
        data.WriteInt32(p.MyIndex);
        data.WritePackedGuid128(p.PartyGUID);
        data.WriteInt32(p.SequenceNum);
        data.WritePackedGuid128(p.LeaderGUID);
        if (ModernVersion.IsWotLKClassicOrLater)
            data.WriteUInt8(p.LeaderFactionGroup);
    }

    [Fact]
    public void PartyUpdate_3_4_3_MatchesTheOldWriter()
    {
        var p = Party();
        using var old = new WorldPacket(1u);
        WritePartyHead(old, p);
        old.WriteInt32(0);
        old.WriteUInt8(0);

        using var data = new WorldPacket(1u);
        PartyUpdate.Layouts.For(ClientVersionBuild.V3_4_3_54261).Write(p, data);
        Assert.Equal(old.GetDataSpan().ToArray(), data.GetDataSpan().ToArray());
    }

    [Fact]
    public void PartyUpdate_4_4_2_AddsThePingRestriction()
    {
        var p = Party();
        using var expected = new WorldPacket(1u);
        WritePartyHead(expected, p);
        expected.WriteInt32(0);                     // PingRestriction
        expected.WriteInt32(0);
        expected.WriteUInt8(0);

        using var data = new WorldPacket(1u);
        PartyUpdate.Layouts.For(ClientVersionBuild.V4_4_2_60895).Write(p, data);
        Assert.Equal(expected.GetDataSpan().ToArray(), data.GetDataSpan().ToArray());
    }

    // ---- SMSG_RAID_INSTANCE_MESSAGE ----

    private static RaidInstanceMessage Welcome() => new()
    {
        Type = InstanceResetWarningType.Welcome,
        MapID = 249,
        DifficultyID = (DifficultyModern)3,
        TimeLeft = 86400,
        Locked = true,
    };

    [Fact]
    public void RaidInstanceMessage_3_4_3_MatchesTheOldWriter()
    {
        var p = Welcome();
        using var old = new WorldPacket(1u);
        old.WriteUInt8((byte)p.Type);
        old.WriteUInt32(p.MapID);
        old.WriteUInt32((uint)p.DifficultyID);
        old.WriteBit(p.Locked);
        old.WriteBit(p.Extended);
        old.FlushBits();

        AssertBothSerialisers(RaidInstanceMessage.Layouts.For(ClientVersionBuild.V3_4_3_54261), p, p.MaxSize, old);
    }

    [Fact]
    public void RaidInstanceMessage_4_4_2_WritesAnIntTypeAndTheTimeLeft()
    {
        var p = Welcome();
        using var expected = new WorldPacket(1u);
        expected.WriteInt32((int)InstanceResetWarningType.Welcome);
        expected.WriteUInt32(249);
        expected.WriteUInt32(3);
        expected.WriteInt32(86400);
        expected.WriteUInt8(0);                     // warning text length
        expected.WriteUInt8(0x80);                  // Locked, Extended

        AssertBothSerialisers(RaidInstanceMessage.Layouts.For(ClientVersionBuild.V4_4_2_60895), p, p.MaxSize, expected);
    }

    // ---- SMSG_DISPLAY_TOAST ----

    private static DisplayToast QuestXpToast() => new() { QuestID = 28, Quantity = 60, Type = 2 };

    [Fact]
    public void DisplayToast_3_4_3_MatchesTheOldWriter()
    {
        var p = QuestXpToast();
        using var old = new WorldPacket(1u);
        old.WriteUInt64(p.Quantity);
        old.WriteUInt8(p.DisplayToastMethod);
        old.WriteUInt32(p.QuestID);
        old.WriteBit(p.Mailed);
        old.WriteBits(p.Type, 2);
        old.FlushBits();

        using var data = new WorldPacket(1u);
        DisplayToast.Layouts.For(ClientVersionBuild.V3_4_3_54261).Write(p, data);
        Assert.Equal(old.GetDataSpan().ToArray(), data.GetDataSpan().ToArray());
    }

    [Fact]
    public void DisplayToast_4_4_2_WritesTheMethodAsAUInt32()
    {
        var p = QuestXpToast();
        using var expected = new WorldPacket(1u);
        expected.WriteUInt64(60);
        expected.WriteUInt32(p.DisplayToastMethod);
        expected.WriteUInt32(28);
        expected.WriteUInt8(0b0100_0000);           // Mailed, Type (2) = 2, IsSecondaryResult

        using var data = new WorldPacket(1u);
        DisplayToast.Layouts.For(ClientVersionBuild.V4_4_2_60895).Write(p, data);
        Assert.Equal(expected.GetDataSpan().ToArray(), data.GetDataSpan().ToArray());
    }

    // ---- SMSG_AUCTION_LIST_ITEMS_RESULT / OWNED / BIDDED ----

    private static AuctionItem Lot() => new()
    {
        Count = 2,
        AuctionID = 7,
        Owner = Player,
        DurationLeft = 3600,
        CensorServerSideInfo = true,
        CensorBidInfo = true,
    };

    private static byte[] Bytes(Action<WorldPacket> write)
    {
        using var data = new WorldPacket(1u);
        write(data);
        return data.GetDataSpan().ToArray();
    }

    [Fact]
    public void AuctionItem_CataClassic_AddsAnIntAfterTheDeleteReason()
    {
        var lot = Lot();
        byte[] old = Bytes(lot.Write);
        byte[] cata = Bytes(d => lot.Write(d, cataClassic: true));

        // bits (2), Count, Charges, Flags, AuctionID, Owner, DurationLeft, DeleteReason
        int at = 2 + 4 * 4 + Bytes(d => d.WritePackedGuid128(Player)).Length + 4 + 1;
        Assert.Equal(old.Length + 4, cata.Length);
        Assert.Equal(old[..at], cata[..at]);
        Assert.Equal(new byte[4], cata[at..(at + 4)]);
        Assert.Equal(old[at..], cata[(at + 4)..]);
    }

    [Fact]
    public void AuctionListItemsResult_3_4_3_MatchesTheOldWriter()
    {
        var p = new AuctionListItemsResult { TotalItemsCount = 9, OnlyUsable = true };
        p.Items.Add(Lot());

        byte[] old = Bytes(d =>
        {
            d.WriteInt32(1);
            d.WriteInt32(9);
            d.WriteUInt32(300);
            d.WriteBool(true);
            Lot().Write(d);
        });
        Assert.Equal(old, Bytes(d => AuctionListItemsResult.Layouts.For(ClientVersionBuild.V3_4_3_54261).Write(p, d)));
    }

    [Fact]
    public void AuctionListItemsResult_4_4_2_EndsWithTheBucketKeyAndTheTotal()
    {
        var p = new AuctionListItemsResult { TotalItemsCount = 9, OnlyUsable = true };
        p.Items.Add(Lot());

        byte[] expected = Bytes(d =>
        {
            d.WriteInt32(1);
            d.WriteInt32(0);
            d.WriteUInt32(300);
            Lot().Write(d, cataClassic: true);
            d.WriteUInt8(0b1010_0000);              // ListType Items, HasMoreResults: 1 of 9
            d.WriteBytes(new byte[5]);              // an empty bucket key: 33 bits
            d.WriteInt32(9);
        });
        Assert.Equal(expected, Bytes(d => AuctionListItemsResult.Layouts.For(ClientVersionBuild.V4_4_2_60895).Write(p, d)));
    }

    [Fact]
    public void AuctionListItemsResult_4_4_2_NamesItsBucketAndListType()
    {
        var p = new AuctionListItemsResult
        {
            TotalItemsCount = 25,
            DesiredDelay = 0,
            ListType = AuctionHouseListType.Commodities,
            HasMoreResults = false,
            BucketKey = new AuctionBucketKey { ItemID = 2589, ItemLevel = 5, BattlePetSpeciesID = null, SuffixItemNameDescriptionID = null },
        };

        Assert.Equal(
            "00000000" + "00000000" + "00000000"    // no auctions, the unused int, the delay
            + "40"                                  // ListType Commodities, no more results
            + "00A1D00500"                          // item 2589 in 20 bits, no pet, level 5 in 11, no suffix
            + "19000000",
            Convert.ToHexString(Bytes(d => AuctionListItemsResult.Layouts.For(ClientVersionBuild.V4_4_2_60895).Write(p, d))));
    }

    [Fact]
    public void AuctionBucketKey_ReadsWhatItWrites()
    {
        using var data = new WorldPacket(1u);
        new AuctionBucketKey { ItemID = 2589, ItemLevel = 5, BattlePetSpeciesID = null, SuffixItemNameDescriptionID = 77 }.Write(data);
        byte[] bytes = data.GetDataSpan().ToArray();
        Assert.Equal("00A1D005804D00", Convert.ToHexString(bytes));

        var reader = new SpanPacketReader(bytes);
        AuctionBucketKey key = AuctionBucketKey.Read(ref reader);
        Assert.Equal(2589u, key.ItemID);
        Assert.Equal(5, key.ItemLevel);
        Assert.Null(key.BattlePetSpeciesID);
        Assert.Equal((ushort)77, key.SuffixItemNameDescriptionID);
        Assert.Equal(0, reader.Remaining);
    }

    // TrinityCore cata_classic AuctionListBucketsResult::Write and BucketInfo, which the client's
    // reader in docs/protocol/4.4.2.60895 matches field for field.
    [Fact]
    public void AuctionListBucketsResult_WritesOneRowPerItem()
    {
        var p = new AuctionBucketList { DesiredDelay = 300, HasMoreResults = true };
        p.Buckets.Add(new AuctionBucketInfo
        {
            Key = new AuctionBucketKey { ItemID = 2589, ItemLevel = 5, BattlePetSpeciesID = null, SuffixItemNameDescriptionID = null },
            TotalQuantity = 25,
            RequiredLevel = 3,
            MinPrice = 80,
            ContainsOwnerItem = true,
        });

        Assert.Equal(
            "01000000" + "2C010000" + "00000000" + "00000000"
            + "40"                                  // BrowseMode Search, HasMoreResults
            + "00A1D00500"                          // the key
            + "19000000" + "03000000" + "5000000000000000"
            + "00000000"                            // no appearances
            + "0C",                                 // no pet fields; has the player's own; all appearances collected
            Convert.ToHexString(Bytes(p.Write)));
    }

    [Theory]
    [InlineData(true, "E0" + "1D0A0000" + "2C010000" + "6009000000000000" + "08000000" + "3075000000000000")]
    [InlineData(false, "00" + "1D0A0000" + "2C010000")]
    public void AuctionGetCommodityQuoteResult_APriceOrNone(bool quoted, string expectedHex)
    {
        var p = new AuctionCommodityQuote { ItemID = 2589, DesiredDelay = 300 };
        if (quoted)
        {
            p.TotalPrice = 2400;
            p.Quantity = 8;
            p.QuoteDuration = 30000;
        }

        Assert.Equal(expectedHex, Convert.ToHexString(Bytes(p.Write)));
    }

    // The retail-style window files every auction of the player's under its item's key, and
    // takes a price that is present as a price, 0 included.
    [Fact]
    public void AuctionListMyItemsResult_4_4_2_KeysEachAuctionAndDropsThePricesItLacks()
    {
        const uint unknownItem = 0xF_FFFF;
        AuctionItem Posted() => new()
        {
            Item = new ItemInstance { ItemID = unknownItem },
            Count = 1,
            AuctionID = 7,
            Owner = Player,
            DurationLeft = 3600,
            MinBid = 100,
            MinIncrement = 0,
            BuyoutPrice = 0,
            BidAmount = 0,
        };

        var p = new AuctionListMyItemsResult(Opcode.SMSG_AUCTION_LIST_BIDDED_ITEMS_RESULT) { TotalItemsCount = 1 };
        p.Items.Add(Posted());

        AuctionItem shaped = Posted();
        shaped.MinIncrement = null;
        shaped.BuyoutPrice = null;
        shaped.BidAmount = null;
        shaped.AuctionBucketKey = new AuctionBucketKey { ItemID = unknownItem, BattlePetSpeciesID = null, SuffixItemNameDescriptionID = null };

        byte[] expected = Bytes(d =>
        {
            d.WriteInt32(1);
            d.WriteUInt32(300);
            d.WriteUInt8(0);                        // HasMoreResults
            shaped.Write(d, cataClassic: true);
        });
        Assert.Equal(expected, Bytes(d => AuctionListMyItemsResult.Layouts.For(ClientVersionBuild.V4_4_2_60895).Write(p, d)));
    }

    [Theory]
    [InlineData(Opcode.SMSG_AUCTION_LIST_OWNED_ITEMS_RESULT)]
    [InlineData(Opcode.SMSG_AUCTION_LIST_BIDDED_ITEMS_RESULT)]
    public void AuctionListMyItemsResult_3_4_3_MatchesTheOldWriter(Opcode opcode)
    {
        var p = new AuctionListMyItemsResult(opcode) { TotalItemsCount = 1 };
        p.Items.Add(Lot());

        byte[] old = Bytes(d =>
        {
            d.WriteInt32(1);
            d.WriteInt32(1);
            d.WriteUInt32(300);
            Lot().Write(d);
        });
        Assert.Equal(old, Bytes(d => AuctionListMyItemsResult.Layouts.For(ClientVersionBuild.V3_4_3_54261).Write(p, d)));
    }

    [Theory]
    [InlineData(Opcode.SMSG_AUCTION_LIST_OWNED_ITEMS_RESULT, true)]
    [InlineData(Opcode.SMSG_AUCTION_LIST_BIDDED_ITEMS_RESULT, false)]
    public void AuctionListMyItemsResult_4_4_2_DropsTheTotalAndOnlyTheOwnedListCountsSoldItems(Opcode opcode, bool soldCount)
    {
        var p = new AuctionListMyItemsResult(opcode) { TotalItemsCount = 1 };
        p.Items.Add(Lot());

        byte[] expected = Bytes(d =>
        {
            d.WriteInt32(1);
            if (soldCount)
                d.WriteInt32(0);
            d.WriteUInt32(300);
            d.WriteUInt8(0);                        // HasMoreResults
            Lot().Write(d, cataClassic: true);
        });
        Assert.Equal(expected, Bytes(d => AuctionListMyItemsResult.Layouts.For(ClientVersionBuild.V4_4_2_60895).Write(p, d)));
    }

    // ---- auction house mail: the text record in the body ----

    // Taken from a 4.3.4 server's mail list: the buyer's GUID in a 16-wide hex field.
    private const string LegacyAuctionBody = "               3:20000:20000:100:1000";

    private static MailListEntry AuctionMail(string subject) => new()
    {
        SenderType = MailType.Auction,
        Subject = subject,
        Body = LegacyAuctionBody,
        DaysLeft = 3600f / 86400f,
        Auction = AuctionMailRecord.FromLegacy(subject, LegacyAuctionBody, _ => Player),
    };

    [Fact]
    public void AuctionMailRecord_ReadsTheLegacyBody()
    {
        WowGuid64 seen = default;
        var record = AuctionMailRecord.FromLegacy("3300:0:6:8:2", LegacyAuctionBody, guid => { seen = guid; return Player; });

        Assert.Equal(new AuctionMailRecord(AuctionMailRecord.SalePending, Player, 20000, 20000, 100, 1000, 0, 0), record);
        Assert.Equal(new WowGuid64(3), seen);
        Assert.Null(AuctionMailRecord.FromLegacy("Hello", "there", _ => Player));
    }

    [Fact]
    public void AuctionMailBody_UpTo3_4_3_IsTheLegacyRecord()
    {
        Assert.Equal(LegacyAuctionBody,
            AuctionMailBody.Shapes.For(ClientVersionBuild.V3_4_3_54261).Of(AuctionMail("3300:0:6:8:2"), now: 1000));
    }

    [Theory]
    [InlineData("3300:0:1:8:2", "Player-1-00000BDF:20000:20000:0")]                 // won
    [InlineData("3300:0:2:8:2", "Player-1-00000BDF:20000:20000:100:1000:0")]        // sold
    [InlineData("3300:0:0:8:2", "")]                                                // outbid: no body
    public void AuctionMailBody_4_4_2_IsTheClientsOwnRecord(string subject, string expected)
    {
        Assert.Equal(expected, AuctionMailBody.Shapes.For(ClientVersionBuild.V4_4_2_60895).Of(AuctionMail(subject), now: 1000));
    }

    [Fact]
    public void AuctionMailBody_4_4_2_PendingSale_GetsTheDelayFromWhatTheNoticeHasLeft()
    {
        const long now = 1_800_000_000;
        uint eta = Time.GetPackedTimeFromUnixTime(now + 3600);

        Assert.Equal($"Player-1-00000BDF:20000:20000:100:1000:3600:{eta}:0",
            AuctionMailBody.Shapes.For(ClientVersionBuild.V4_4_2_60895).Of(AuctionMail("3300:0:6:8:2"), now));
    }

    [Fact]
    public void MailListEntry_OnlyAnAuctionMailHasItsBodyRewritten()
    {
        var letter = new MailListEntry { SenderType = MailType.Normal, Body = LegacyAuctionBody, Auction = AuctionMail("3300:0:2:8:2").Auction };
        Assert.Equal(LegacyAuctionBody, AuctionMailBody.Shapes.For(ClientVersionBuild.V3_4_3_54261).Of(letter, 0));

        // No record read: whatever the body is goes out as it came.
        var odd = new MailListEntry { SenderType = MailType.Auction, Body = "not a record" };
        Assert.Equal("not a record", AuctionMailBody.Shapes.For(ClientVersionBuild.V4_4_2_60895).Of(odd, 0));
    }

    // ---- AuthWaitInfo: SMSG_AUTH_RESPONSE's queue block and SMSG_WAIT_QUEUE_UPDATE ----

    [Theory]
    [InlineData(ClientVersionBuild.V3_4_3_54261, "07000000" + "03000000" + "80")]
    [InlineData(ClientVersionBuild.V4_4_2_60895, "07000000" + "03000000" + "00000000" + "80")]
    public void AuthWaitInfo_WritesTheShapeOfTheBuild(ClientVersionBuild build, string expectedHex)
    {
        var info = new AuthWaitInfo { WaitCount = 7, WaitTime = 3, HasFCM = true };
        var shape = AuthWaitInfo.Shapes.For(build);
        byte[] expected = Convert.FromHexString(expectedHex);

        using var data = new WorldPacket(1u);
        info.Write(data, shape);
        Assert.Equal(expected, data.GetDataSpan().ToArray());

        Span<byte> buffer = stackalloc byte[AuthWaitInfo.MaxSize];
        var writer = new SpanPacketWriter(buffer);
        info.Write(ref writer, shape);
        Assert.Equal(expected, buffer[..writer.Position].ToArray());
    }

    // ---- CMSG_SET_FACTION_AT_WAR / NOT_AT_WAR ----

    [Fact]
    public void SetFactionAtWar_CataClassic_ReadsTheIndexAsAUInt16()
    {
        // Frame like the wire: WorldPacket's read-mode ctor consumes a 2-byte opcode prefix.
        byte[] framed = [0, 0, 0x45, 0x00];
        var r = new SpanPacketReader(new WorldPacket(framed).GetRemainingSpan());
        SetFactionAtWarCodecCataClassic.Read(ref r, out var atWar);
        Assert.Equal(0x45, atWar.FactionIndex);
        Assert.Equal(0, r.Remaining);

        r = new SpanPacketReader(new WorldPacket(framed).GetRemainingSpan());
        SetFactionNotAtWarCodecCataClassic.Read(ref r, out var notAtWar);
        Assert.Equal(0x45, notAtWar.FactionIndex);
        Assert.Equal(0, r.Remaining);
    }

    private static void AssertBothSerialisers<T>(ServerPacketLayout<T> layout, T packet, int maxSize, WorldPacket expectedPacket)
        where T : ServerPacket
    {
        byte[] expected = expectedPacket.GetDataSpan().ToArray();

        using var data = new WorldPacket(1u);
        layout.Write(packet, data);
        Assert.Equal(expected, data.GetDataSpan().ToArray());

        byte[] buffer = ArrayPool<byte>.Shared.Rent(maxSize);
        try
        {
            int written = layout.WriteToSpan(packet, buffer);
            Assert.Equal(expected, buffer.AsSpan(0, written).ToArray());
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
