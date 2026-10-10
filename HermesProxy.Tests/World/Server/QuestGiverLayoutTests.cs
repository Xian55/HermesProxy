using System;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// The quest-giver packets went from one writer with a 3.4.3 branch to one layout per shape. The
/// 1.14/2.5 and 3.4.3 layouts must give the bytes the old branches did; the 4.4.2 quest details must
/// give the bytes of a native TrinityCore cata_classic packet.
/// </summary>
public sealed class QuestGiverLayoutTests
{
    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.IsType<QuestGiverQuestDetails.ClassicEraLayout>(QuestGiverQuestDetails.Layouts.For(ClientVersionBuild.V1_14_2_42597));
        Assert.IsType<QuestGiverQuestDetails.ClassicEraLayout>(QuestGiverQuestDetails.Layouts.For(ClientVersionBuild.V2_5_3_41750));
        Assert.IsType<QuestGiverQuestDetails.WotLKClassicLayout>(QuestGiverQuestDetails.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<QuestGiverQuestDetails.CataClassicLayout>(QuestGiverQuestDetails.Layouts.For(ClientVersionBuild.V4_4_2_60895));

        Assert.IsType<QuestGiverQuestListMessage.WotLKClassicLayout>(QuestGiverQuestListMessage.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<QuestGiverQuestListMessage.CataClassicLayout>(QuestGiverQuestListMessage.Layouts.For(ClientVersionBuild.V4_4_2_60895));
        Assert.IsType<QuestGiverRequestItems.WotLKClassicLayout>(QuestGiverRequestItems.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<QuestGiverRequestItems.CataClassicLayout>(QuestGiverRequestItems.Layouts.For(ClientVersionBuild.V4_4_2_60895));
        Assert.IsType<QuestGiverOfferRewardMessage.WotLKClassicLayout>(QuestGiverOfferRewardMessage.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<QuestGiverOfferRewardMessage.CataClassicLayout>(QuestGiverOfferRewardMessage.Layouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    [Fact]
    public void Details_ClassicEraLayout_MatchesTheOldLegacyBranch()
        => Assert.Equal(OldDetailsClassicEra(SampleDetails()), Write(new QuestGiverQuestDetails.ClassicEraLayout(), SampleDetails()));

    [Fact]
    public void Details_WotLKClassicLayout_MatchesTheOld3_4_3Branch()
        => Assert.Equal(OldDetailsWotLK(SampleDetails()), Write(new QuestGiverQuestDetails.WotLKClassicLayout(), SampleDetails()));

    [Fact]
    public void List_ClassicEraLayout_MatchesTheOldLegacyBranch()
        => Assert.Equal(OldListClassicEra(SampleList()), Write(new QuestGiverQuestListMessage.ClassicEraLayout(), SampleList()));

    [Fact]
    public void List_WotLKClassicLayout_MatchesTheOld3_4_3Branch()
        => Assert.Equal(OldListWotLK(SampleList()), Write(new QuestGiverQuestListMessage.WotLKClassicLayout(), SampleList()));

    [Fact]
    public void RequestItems_ClassicEraLayout_MatchesTheOldLegacyBranch()
        => Assert.Equal(OldRequestItemsClassicEra(SampleRequestItems()), Write(new QuestGiverRequestItems.ClassicEraLayout(), SampleRequestItems()));

    [Fact]
    public void RequestItems_WotLKClassicLayout_MatchesTheOld3_4_3Branch()
        => Assert.Equal(OldRequestItemsWotLK(SampleRequestItems()), Write(new QuestGiverRequestItems.WotLKClassicLayout(), SampleRequestItems()));

    [Fact]
    public void Offer_ClassicEraLayout_MatchesTheOldLegacyBranch()
        => Assert.Equal(OldOfferClassicEra(SampleOffer()), Write(new QuestGiverOfferRewardMessage.ClassicEraLayout(), SampleOffer()));

    [Fact]
    public void Offer_WotLKClassicLayout_MatchesTheOld3_4_3Branch()
        => Assert.Equal(OldOfferWotLK(SampleOffer()), Write(new QuestGiverOfferRewardMessage.WotLKClassicLayout(), SampleOffer()));

    [Fact]
    public void Details_CataClassicLayout_MatchesANativePacket()
    {
        var packet = new QuestGiverQuestDetails
        {
            QuestGiverGUID = new WowGuid128(188, 2305847407260217664),
            InformUnit = WowGuid128.Empty,
            QuestID = 28766,
            QuestGiverCreatureID = 197,
            AutoLaunched = true,
            QuestTitle = "Beating Them Back!",
            DescriptionText = "So you're the new recruit from Stormwind, eh? I'm Marshal McBride, commander of this garrison. Glad to have you on board...$B$B<McBride looks through some papers.>$B$B$N. It is $N, right?$B$BYou've arrived just in time. The Blackrock orcs have managed to sneak into Northshire through a break in the mountain. My soldiers are doing the best that they can to push them back, but I fear they will be overwhelmed soon.$B$BHead northwest into the forest and kill the attacking Blackrock worgs! Help my soldiers!",
            LogDescription = "Kill 6 Blackrock Battle Worgs.",
            PortraitGiverText = "This is a Blackrock Battle Worg.",
            PortraitGiverName = "Blackrock Battle Worg",
        };
        packet.QuestFlags[0] = 0x80000;                 // AutoAccept
        packet.DescEmotes[0] = new QuestDescEmote { Type = 6 };
        packet.DescEmotes[1] = new QuestDescEmote { Type = 1 };
        packet.DescEmotes[2] = new QuestDescEmote { Type = 1 };
        packet.DescEmotes[3] = new QuestDescEmote { Type = 5 };
        packet.Objectives.Add(new QuestObjectiveSimple { Id = 442541, Type = 0, ObjectID = 49871, Amount = 6 });
        packet.Rewards.ItemID[0] = 57255;
        packet.Rewards.ItemQty[0] = 1;
        packet.Rewards.ItemCount = 1;
        packet.Rewards.Money = 50;
        packet.Rewards.XP = 550;
        packet.Rewards.FactionID[0] = 72;
        packet.Rewards.FactionValue[0] = 5;

        Assert.Equal(NativeDetails, Convert.ToHexString(Write(new QuestGiverQuestDetails.CataClassicLayout(), packet)));
    }

    // SMSG_QUEST_GIVER_QUEST_DETAILS from TrinityCore cata_classic to a 4.4.2.60895 client
    // (refs/native-captures/tc_cata_442_20261010-150803.pkt, packet 922).
    private const string NativeDetails =
        "01A3BC4031042000005E700000000000000000000000000000000000000000000000000800000000000000000000000000000000000400000001000000000000" +
        "000000000000000000C5000000000000000600000000000000010000000000000001000000000000000500000000000000ADC0060000000000CFC20000060000" +
        "00090FD80F0402A0000400A7DF000001000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" +
        "00000000000000000000000000000000000000000000000000000000000000010000003200000026020000000000000000000000000000000000000000000000" +
        "00000048000000050000000000000007000000000000000000000000000000070000000000000000000000000000000700000000000000000000000000000007" +
        "00000000000000000000000000000007000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" +
        "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" +
        "0000000000000000000000000000000000000000000000000000000000000000000042656174696E67205468656D204261636B21536F20796F75277265207468" +
        "65206E657720726563727569742066726F6D2053746F726D77696E642C2065683F2049276D204D61727368616C204D6342726964652C20636F6D6D616E646572" +
        "206F662074686973206761727269736F6E2E20476C616420746F206861766520796F75206F6E20626F6172642E2E2E244224423C4D634272696465206C6F6F6B" +
        "73207468726F75676820736F6D65207061706572732E3E24422442244E2E20497420697320244E2C2072696768743F24422442596F7527766520617272697665" +
        "64206A75737420696E2074696D652E2054686520426C61636B726F636B206F7263732068617665206D616E6167656420746F20736E65616B20696E746F204E6F" +
        "7274687368697265207468726F756768206120627265616B20696E20746865206D6F756E7461696E2E204D7920736F6C64696572732061726520646F696E6720" +
        "7468652062657374207468617420746865792063616E20746F2070757368207468656D206261636B2C206275742049206665617220746865792077696C6C2062" +
        "65206F7665727768656C6D656420736F6F6E2E2442244248656164206E6F7274687765737420696E746F2074686520666F7265737420616E64206B696C6C2074" +
        "68652061747461636B696E6720426C61636B726F636B20776F726773212048656C70206D7920736F6C6469657273214B696C6C203620426C61636B726F636B20" +
        "426174746C6520576F7267732E54686973206973206120426C61636B726F636B20426174746C6520576F72672E426C61636B726F636B20426174746C6520576F" +
        "7267";

    private static byte[] Write<T>(ServerPacketLayout<T> layout, T packet) where T : ServerPacket
    {
        using var data = new WorldPacket(1u);
        layout.Write(packet, data);
        return data.GetDataSpan().ToArray();
    }

    private static QuestRewards SampleRewards()
    {
        var rewards = new QuestRewards
        {
            ChoiceItemCount = 2,
            ItemCount = 1,
            Money = 1234,
            XP = 870,
            ArtifactXP = 5,
            ArtifactCategoryID = 6,
            Honor = 7,
            Title = 8,
            FactionFlags = 9,
            SpellCompletionID = 10,
            SkillLineID = 11,
            NumSkillUps = 12,
            TreasurePickerID = 13,
            IsBoostSpell = true,
        };
        rewards.ItemID[0] = 2589;
        rewards.ItemQty[0] = 3;
        rewards.FactionID[1] = 72;
        rewards.FactionValue[1] = -250;
        rewards.FactionOverride[1] = 4;
        rewards.FactionCapIn[1] = 7;
        rewards.SpellCompletionDisplayID[2] = 133;
        rewards.CurrencyID[3] = 390;
        rewards.CurrencyQty[3] = 25;
        rewards.ChoiceItems[1].LootItemType = 1;
        rewards.ChoiceItems[1].Item.ItemID = 6070;
        rewards.ChoiceItems[1].Quantity = 1;
        return rewards;
    }

    private static QuestGiverQuestDetails SampleDetails()
    {
        var packet = new QuestGiverQuestDetails
        {
            QuestGiverGUID = new WowGuid128(188, 0x2000040000003140),
            InformUnit = new WowGuid128(77, 0x0800000000000001),
            QuestGiverCreatureID = 197,
            QuestID = 783,
            QuestPackageID = 4,
            SuggestedPartyMembers = 3,
            Rewards = SampleRewards(),
            PortraitTurnIn = 11,
            PortraitGiver = 12,
            PortraitGiverMount = 13,
            PortraitGiverModelSceneID = 14,
            QuestStartItemID = 15,
            QuestSessionBonus = 16,
            PortraitGiverText = "giver text",
            PortraitGiverName = "giver",
            PortraitTurnInText = "turn-in text",
            PortraitTurnInName = "turn-in",
            QuestTitle = "A Threat Within",
            DescriptionText = "I suspect there is a threat within.",
            LogDescription = "Speak with Marshal McBride.",
            DisplayPopup = true,
            StartCheat = true,
            AutoLaunched = true,
        };
        packet.QuestFlags[0] = 8;
        packet.QuestFlags[1] = 0x40;
        packet.Objectives.Add(new QuestObjectiveSimple { Id = 1, ObjectID = 299, Amount = 8, Type = 0 });
        packet.Objectives.Add(new QuestObjectiveSimple { Id = 2, ObjectID = 750, Amount = 10, Type = 1 });
        packet.DescEmotes[0] = new QuestDescEmote { Type = 1, Delay = 0 };
        packet.DescEmotes[2] = new QuestDescEmote { Type = 5, Delay = 500 };
        packet.LearnSpells.Add(133);
        return packet;
    }

    private static QuestGiverQuestListMessage SampleList()
    {
        var packet = new QuestGiverQuestListMessage
        {
            QuestGiverGUID = new WowGuid128(5435, 0x2000040000014E40),
            GreetEmoteDelay = 100,
            GreetEmoteType = 2,
            Greeting = "Greetings, $N.",
        };
        packet.QuestOptions.Add(new ClientGossipQuest { QuestID = 783, QuestType = 2, QuestLevel = 1, QuestTitle = "A Threat Within" });
        packet.QuestOptions.Add(new ClientGossipQuest { QuestID = 7, ContentTuningID = 3, QuestType = 4, QuestLevel = 2, Repeatable = true, QuestFlagsEx = 9, QuestTitle = "Kobold Camp Cleanup" });
        return packet;
    }

    private static QuestGiverRequestItems SampleRequestItems()
    {
        var packet = new QuestGiverRequestItems
        {
            QuestGiverGUID = new WowGuid128(5435, 0x2000040000014E40),
            QuestGiverCreatureID = 823,
            QuestID = 7,
            CompEmoteDelay = 50,
            CompEmoteType = 6,
            AutoLaunched = true,
            SuggestPartyMembers = 2,
            MoneyToGet = 500,
            StatusFlags = QuestGiverRequestItems.StatusComplete,
            QuestTitle = "Kobold Camp Cleanup",
            CompletionText = "Have you killed them?",
        };
        packet.QuestFlags[0] = 8;
        packet.QuestFlags[1] = 0x40;
        packet.Collect.Add(new QuestObjectiveCollect { ObjectID = 752, Amount = 8, Flags = 1 });
        packet.Currency.Add(new QuestCurrency { CurrencyID = 390, Amount = 25 });
        return packet;
    }

    private static QuestGiverOfferRewardMessage SampleOffer()
    {
        var packet = new QuestGiverOfferRewardMessage
        {
            PortraitTurnIn = 11,
            PortraitGiver = 12,
            PortraitGiverMount = 13,
            PortraitGiverModelSceneID = 14,
            QuestTitle = "Kobold Camp Cleanup",
            RewardText = "Well done, $N.",
            PortraitGiverText = "giver text",
            PortraitGiverName = "giver",
            PortraitTurnInText = "turn-in text",
            PortraitTurnInName = "turn-in",
            QuestPackageID = 4,
        };
        packet.QuestData.QuestGiverGUID = new WowGuid128(5435, 0x2000040000014E40);
        packet.QuestData.QuestGiverCreatureID = 823;
        packet.QuestData.QuestID = 7;
        packet.QuestData.AutoLaunched = true;
        packet.QuestData.SuggestedPartyMembers = 2;
        packet.QuestData.Rewards = SampleRewards();
        packet.QuestData.Emotes.Add(new QuestDescEmote { Type = 4, Delay = 60 });
        packet.QuestData.QuestFlags[0] = 8;
        packet.QuestData.QuestFlags[1] = 0x40;
        return packet;
    }

    // The writers as they stood before the layouts, frozen.

    private static byte[] OldDetailsClassicEra(QuestGiverQuestDetails p)
    {
        using var w = new WorldPacket(1u);
        w.WritePackedGuid128(p.QuestGiverGUID);
        w.WritePackedGuid128(p.InformUnit);
        w.WriteUInt32(p.QuestID);
        w.WriteInt32(p.QuestPackageID);
        w.WriteUInt32(p.PortraitGiver);
        w.WriteUInt32(p.PortraitGiverMount);
        w.WriteUInt32(p.PortraitGiverModelSceneID);
        w.WriteUInt32(p.PortraitTurnIn);
        w.WriteUInt32(p.QuestFlags[0]); // Flags
        w.WriteUInt32(p.QuestFlags[1]); // FlagsEx
        w.WriteUInt32(p.SuggestedPartyMembers);
        w.WriteInt32(p.LearnSpells.Count);
        w.WriteInt32(p.DescEmotes.Length);
        w.WriteInt32(p.Objectives.Count);
        w.WriteInt32(p.QuestStartItemID);
        w.WriteInt32(p.QuestSessionBonus);

        foreach (uint spell in p.LearnSpells)
            w.WriteUInt32(spell);

        foreach (QuestDescEmote emote in p.DescEmotes)
        {
            w.WriteUInt32(emote.Type);
            w.WriteUInt32(emote.Delay);
        }

        foreach (QuestObjectiveSimple obj in p.Objectives)
        {
            w.WriteUInt32(obj.Id);
            w.WriteInt32(obj.ObjectID);
            w.WriteInt32(obj.Amount);
            w.WriteUInt8(obj.Type);
        }

        w.WriteBits(p.QuestTitle.GetByteCount(), 9);
        w.WriteBits(p.DescriptionText.GetByteCount(), 12);
        w.WriteBits(p.LogDescription.GetByteCount(), 12);
        w.WriteBits(p.PortraitGiverText.GetByteCount(), 10);
        w.WriteBits(p.PortraitGiverName.GetByteCount(), 8);
        w.WriteBits(p.PortraitTurnInText.GetByteCount(), 10);
        w.WriteBits(p.PortraitTurnInName.GetByteCount(), 8);
        w.WriteBit(p.AutoLaunched);
        w.WriteBit(false);   // unused in client
        w.WriteBit(p.StartCheat);
        w.WriteBit(p.DisplayPopup);
        w.FlushBits();

        p.Rewards.Write(w);

        w.WriteString(p.QuestTitle);
        w.WriteString(p.DescriptionText);
        w.WriteString(p.LogDescription);
        w.WriteString(p.PortraitGiverText);
        w.WriteString(p.PortraitGiverName);
        w.WriteString(p.PortraitTurnInText);
        w.WriteString(p.PortraitTurnInName);
        return w.GetDataSpan().ToArray();
    }

    private static byte[] OldDetailsWotLK(QuestGiverQuestDetails p)
    {
        using var w = new WorldPacket(1u);
        w.WritePackedGuid128(p.QuestGiverGUID);
        w.WritePackedGuid128(p.InformUnit);
        w.WriteInt32((int)p.QuestID);
        w.WriteInt32(p.QuestPackageID);
        w.WriteInt32((int)p.PortraitGiver);
        w.WriteInt32((int)p.PortraitGiverMount);
        w.WriteInt32((int)p.PortraitGiverModelSceneID);
        w.WriteInt32((int)p.PortraitTurnIn);
        w.WriteUInt32(p.QuestFlags[0]);    // Flags
        w.WriteUInt32(p.QuestFlags[1]);    // FlagsEx
        w.WriteUInt32(0);                // FlagsEx2 (V3_4_3 only)
        w.WriteInt32((int)p.SuggestedPartyMembers);
        w.WriteUInt32((uint)p.LearnSpells.Count);
        w.WriteUInt32((uint)p.DescEmotes.Length);
        w.WriteUInt32((uint)p.Objectives.Count);
        w.WriteInt32(p.QuestStartItemID);
        w.WriteInt32(p.QuestSessionBonus);
        w.WriteInt32((int)p.QuestGiverCreatureID);
        w.WriteUInt32(0);                // ConditionalDescriptionText.size

        foreach (uint spell in p.LearnSpells)
            w.WriteInt32((int)spell);

        foreach (QuestDescEmote emote in p.DescEmotes)
        {
            w.WriteInt32((int)emote.Type);
            w.WriteUInt32(emote.Delay);
        }

        foreach (QuestObjectiveSimple obj in p.Objectives)
        {
            w.WriteUInt32(obj.Id);
            w.WriteInt32(obj.ObjectID);
            w.WriteInt32(obj.Amount);
            w.WriteUInt8(obj.Type);
        }

        w.WriteBits(p.QuestTitle.GetByteCount(), 9);
        w.WriteBits(p.DescriptionText.GetByteCount(), 12);
        w.WriteBits(p.LogDescription.GetByteCount(), 12);
        w.WriteBits(p.PortraitGiverText.GetByteCount(), 10);
        w.WriteBits(p.PortraitGiverName.GetByteCount(), 8);
        w.WriteBits(p.PortraitTurnInText.GetByteCount(), 10);
        w.WriteBits(p.PortraitTurnInName.GetByteCount(), 8);
        w.WriteBit(p.AutoLaunched);
        w.WriteBit(false);
        w.WriteBit(p.StartCheat);
        w.WriteBit(p.DisplayPopup);
        w.FlushBits();

        p.Rewards.Write(w);

        w.WriteString(p.QuestTitle);
        w.WriteString(p.DescriptionText);
        w.WriteString(p.LogDescription);
        w.WriteString(p.PortraitGiverText);
        w.WriteString(p.PortraitGiverName);
        w.WriteString(p.PortraitTurnInText);
        w.WriteString(p.PortraitTurnInName);
        return w.GetDataSpan().ToArray();
    }

    private static byte[] OldListClassicEra(QuestGiverQuestListMessage p)
    {
        using var w = new WorldPacket(1u);
        w.WritePackedGuid128(p.QuestGiverGUID);
        w.WriteUInt32(p.GreetEmoteDelay);
        w.WriteUInt32(p.GreetEmoteType);
        w.WriteInt32(p.QuestOptions.Count);
        w.WriteBits(p.Greeting.GetByteCount(), 11);
        w.FlushBits();

        foreach (ClientGossipQuest quest in p.QuestOptions)
            quest.Write(w);

        w.WriteString(p.Greeting);
        return w.GetDataSpan().ToArray();
    }

    private static byte[] OldListWotLK(QuestGiverQuestListMessage p)
    {
        using var w = new WorldPacket(1u);
        w.WritePackedGuid128(p.QuestGiverGUID);
        w.WriteUInt32(p.GreetEmoteDelay);
        w.WriteUInt32(p.GreetEmoteType);
        w.WriteUInt32((uint)p.QuestOptions.Count);
        w.WriteBits(p.Greeting.GetByteCount(), 11);
        w.FlushBits();

        foreach (ClientGossipQuest quest in p.QuestOptions)
            quest.WriteWotLK(w);

        w.WriteString(p.Greeting);
        return w.GetDataSpan().ToArray();
    }

    private static byte[] OldRequestItemsClassicEra(QuestGiverRequestItems p)
    {
        using var w = new WorldPacket(1u);
        w.WritePackedGuid128(p.QuestGiverGUID);
        w.WriteUInt32(p.QuestGiverCreatureID);
        w.WriteUInt32(p.QuestID);
        w.WriteUInt32(p.CompEmoteDelay);
        w.WriteUInt32(p.CompEmoteType);
        w.WriteUInt32(p.QuestFlags[0]);
        w.WriteUInt32(p.QuestFlags[1]);
        w.WriteUInt32(p.SuggestPartyMembers);
        w.WriteInt32(p.MoneyToGet);
        w.WriteInt32(p.Collect.Count);
        w.WriteInt32(p.Currency.Count);
        w.WriteUInt32(p.StatusFlags);

        foreach (QuestObjectiveCollect obj in p.Collect)
        {
            w.WriteUInt32(obj.ObjectID);
            w.WriteUInt32(obj.Amount);
            w.WriteUInt32(obj.Flags);
        }
        foreach (QuestCurrency cur in p.Currency)
        {
            w.WriteUInt32(cur.CurrencyID);
            w.WriteInt32(cur.Amount);
        }

        w.WriteBit(p.AutoLaunched);
        w.FlushBits();

        w.WriteBits(p.QuestTitle.GetByteCount(), 9);
        w.WriteBits(p.CompletionText.GetByteCount(), 12);

        w.WriteString(p.QuestTitle);
        w.WriteString(p.CompletionText);
        return w.GetDataSpan().ToArray();
    }

    private static byte[] OldRequestItemsWotLK(QuestGiverRequestItems p)
    {
        using var w = new WorldPacket(1u);
        w.WritePackedGuid128(p.QuestGiverGUID);
        w.WriteInt32((int)p.QuestGiverCreatureID);
        w.WriteInt32((int)p.QuestID);
        w.WriteUInt32(p.CompEmoteDelay);
        w.WriteInt32((int)p.CompEmoteType);
        w.WriteUInt32(p.QuestFlags[0]);
        w.WriteUInt32(p.QuestFlags[1]);
        w.WriteUInt32(0);                   // QuestFlagsEx2
        w.WriteInt32((int)p.SuggestPartyMembers);
        w.WriteInt32(p.MoneyToGet);
        w.WriteInt32(p.Collect.Count);
        w.WriteInt32(p.Currency.Count);
        w.WriteInt32((int)p.StatusFlags);

        foreach (QuestObjectiveCollect obj in p.Collect)
        {
            w.WriteInt32((int)obj.ObjectID);
            w.WriteInt32((int)obj.Amount);
            w.WriteUInt32(obj.Flags);        // V3_4_3 only
        }
        foreach (QuestCurrency cur in p.Currency)
        {
            w.WriteInt32((int)cur.CurrencyID);
            w.WriteInt32(cur.Amount);
        }

        w.WriteBit(p.AutoLaunched);
        w.FlushBits();

        w.WriteInt32((int)p.QuestGiverCreatureID);  // duplicated
        w.WriteInt32(0);                          // ConditionalCompletionText.Count

        w.WriteBits(p.QuestTitle.GetByteCount(), 9);
        w.WriteBits(p.CompletionText.GetByteCount(), 12);
        w.FlushBits();

        w.WriteString(p.QuestTitle);
        w.WriteString(p.CompletionText);
        return w.GetDataSpan().ToArray();
    }

    private static byte[] OldOfferClassicEra(QuestGiverOfferRewardMessage p)
    {
        using var w = new WorldPacket(1u);
        p.QuestData.Write(w);
        w.WriteUInt32(p.QuestPackageID);
        w.WriteUInt32(p.PortraitGiver);
        w.WriteUInt32(p.PortraitGiverMount);
        w.WriteUInt32(p.PortraitGiverModelSceneID);
        w.WriteUInt32(p.PortraitTurnIn);

        w.WriteBits(p.QuestTitle.GetByteCount(), 9);
        w.WriteBits(p.RewardText.GetByteCount(), 12);
        w.WriteBits(p.PortraitGiverText.GetByteCount(), 10);
        w.WriteBits(p.PortraitGiverName.GetByteCount(), 8);
        w.WriteBits(p.PortraitTurnInText.GetByteCount(), 10);
        w.WriteBits(p.PortraitTurnInName.GetByteCount(), 8);

        w.WriteString(p.QuestTitle);
        w.WriteString(p.RewardText);
        w.WriteString(p.PortraitGiverText);
        w.WriteString(p.PortraitGiverName);
        w.WriteString(p.PortraitTurnInText);
        w.WriteString(p.PortraitTurnInName);
        return w.GetDataSpan().ToArray();
    }

    private static byte[] OldOfferWotLK(QuestGiverOfferRewardMessage p)
    {
        using var w = new WorldPacket(1u);
        p.QuestData.WriteWotLK(w);
        w.WriteInt32((int)p.QuestPackageID);
        w.WriteInt32((int)p.PortraitGiver);
        w.WriteInt32((int)p.PortraitGiverMount);
        w.WriteInt32((int)p.PortraitGiverModelSceneID);
        w.WriteInt32((int)p.PortraitTurnIn);
        w.WriteInt32((int)p.QuestData.QuestGiverCreatureID);  // duplicated
        w.WriteInt32(0);                                    // ConditionalRewardText.Count

        w.WriteBits(p.QuestTitle.GetByteCount(), 9);
        w.WriteBits(p.RewardText.GetByteCount(), 12);
        w.WriteBits(p.PortraitGiverText.GetByteCount(), 10);
        w.WriteBits(p.PortraitGiverName.GetByteCount(), 8);
        w.WriteBits(p.PortraitTurnInText.GetByteCount(), 10);
        w.WriteBits(p.PortraitTurnInName.GetByteCount(), 8);
        w.FlushBits();

        w.WriteString(p.QuestTitle);
        w.WriteString(p.RewardText);
        w.WriteString(p.PortraitGiverText);
        w.WriteString(p.PortraitGiverName);
        w.WriteString(p.PortraitTurnInText);
        w.WriteString(p.PortraitTurnInName);
        return w.GetDataSpan().ToArray();
    }
}
