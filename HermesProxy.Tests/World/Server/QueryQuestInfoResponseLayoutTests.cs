using System;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// <see cref="QueryQuestInfoResponse"/> went from one writer with 3.4.3 branches to one layout per
/// shape. The 1.14/2.5 and 3.4.3 layouts must give the bytes the old branches did; the 4.4.2 layout
/// must give the bytes of native TrinityCore cata_classic packets.
/// </summary>
public sealed class QueryQuestInfoResponseLayoutTests
{
    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.IsType<QueryQuestInfoResponse.ClassicEraLayout>(QueryQuestInfoResponse.Layouts.For(ClientVersionBuild.V1_14_2_42597));
        Assert.IsType<QueryQuestInfoResponse.ClassicEraLayout>(QueryQuestInfoResponse.Layouts.For(ClientVersionBuild.V2_5_3_41750));
        Assert.IsType<QueryQuestInfoResponse.WotLKClassicLayout>(QueryQuestInfoResponse.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<QueryQuestInfoResponse.CataClassicLayout>(QueryQuestInfoResponse.Layouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ClassicEraLayout_MatchesTheOldLegacyBranch(bool allow)
        => Assert.Equal(OldQuestInfo(Sample(allow), isV343: false), Write(new QueryQuestInfoResponse.ClassicEraLayout(), Sample(allow)));

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WotLKClassicLayout_MatchesTheOld3_4_3Branch(bool allow)
        => Assert.Equal(OldQuestInfo(Sample(allow), isV343: true), Write(new QueryQuestInfoResponse.WotLKClassicLayout(), Sample(allow)));

    [Fact]
    public void CataClassicLayout_MatchesNativePacket925()
        => Assert.Equal(Native925, Convert.ToHexString(Write(new QueryQuestInfoResponse.CataClassicLayout(), NativeSample925())));

    [Fact]
    public void CataClassicLayout_MatchesNativePacket932()
        => Assert.Equal(Native932, Convert.ToHexString(Write(new QueryQuestInfoResponse.CataClassicLayout(), NativeSample932())));

    private static byte[] Write(ServerPacketLayout<QueryQuestInfoResponse> layout, QueryQuestInfoResponse packet)
    {
        using var data = new WorldPacket(1u);
        layout.Write(packet, data);
        return data.GetDataSpan().ToArray();
    }

    private static QueryQuestInfoResponse Sample(bool allow)
    {
        var info = new QuestTemplate
        {
            QuestID = 783,
            QuestType = 2,
            QuestLevel = 1,
            QuestScalingFactionGroup = 3,
            QuestMaxScalingLevel = 255,
            QuestPackageID = 4,
            MinLevel = 1,
            QuestSortID = 12,
            QuestInfoID = 81,
            SuggestedGroupNum = 5,
            RewardNextQuest = 7,
            RewardXPDifficulty = 2,
            RewardMoney = 100,
            RewardMoneyDifficulty = 3,
            RewardBonusMoney = 60,
            RewardSpell = 133,
            RewardHonor = 9,
            RewardKillHonor = 1.5f,
            RewardArtifactXPDifficulty = 2,
            RewardArtifactXPMultiplier = 0.5f,
            RewardArtifactCategoryID = 3,
            StartItem = 745,
            Flags = 8,
            FlagsEx = 0x40,
            FlagsEx2 = 2,
            POIContinent = 0,
            POIx = -8913.2f,
            POIy = -136.1f,
            POIPriority = 1,
            AllowableRaces = 0x44D,
            LogTitle = "A Threat Within",
            LogDescription = "Speak with Marshal McBride.",
            QuestDescription = "I suspect there is a threat within, $N.",
            AreaDescription = "Northshire",
            RewardTitle = 1,
            RewardArenaPoints = 2,
            RewardSkillLineID = 3,
            RewardNumSkillUps = 4,
            PortraitGiver = 5,
            PortraitGiverMount = 6,
            PortraitGiverModelSceneID = 7,
            PortraitTurnIn = 8,
            ManagedWorldStateID = 9,
            QuestSessionBonus = 10,
            QuestGiverCreatureID = 823,
            PortraitGiverText = "giver text",
            PortraitGiverName = "giver",
            PortraitTurnInText = "turn-in text",
            PortraitTurnInName = "turn-in",
            QuestCompletionLog = "Report to Marshal McBride.",
            RewardFactionFlags = 1,
            AcceptedSoundKitID = 890,
            CompleteSoundKitID = 878,
            AreaGroupID = 11,
            TimeAllowed = 900,
            TreasurePickerID = 12,
            Expansion = 2,
            ReadyForTranslation = true,
        };
        info.RewardDisplaySpell[1] = 2;
        info.RewardItems[0] = 57255;
        info.RewardAmount[0] = 1;
        info.ItemDrop[2] = 3;
        info.ItemDropQuantity[2] = 4;
        info.UnfilteredChoiceItems[1] = new QuestInfoChoiceItem { ItemID = 6070, Quantity = 1, DisplayID = 9 };
        info.RewardFactionID[0] = 72;
        info.RewardFactionValue[0] = 5;
        info.RewardFactionOverride[0] = -1;
        info.RewardFactionCapIn[0] = 7;
        info.RewardCurrencyID[3] = 390;
        info.RewardCurrencyQty[3] = 25;
        info.Objectives.Add(new QuestObjective { Id = 1, Type = QuestObjectiveType.Monster, StorageIndex = 0, ObjectID = 299, Amount = 8,
            Flags2 = 3, ProgressBarWeight = 0.25f, Description = "Wolves slain", VisualEffects = [11, 12] });
        info.Objectives.Add(new QuestObjective { Id = 2, Type = QuestObjectiveType.Item, StorageIndex = 1, ObjectID = 750, Amount = 10 });
        return new QueryQuestInfoResponse { QuestID = 783, Allow = allow, Info = info };
    }

    private static QueryQuestInfoResponse NativeSample925()
    {
        var info = new QuestTemplate
        {
            QuestID = 28766,
            QuestType = 2,
            QuestLevel = 3,
            QuestScalingFactionGroup = 0,
            QuestMaxScalingLevel = 255,
            QuestPackageID = 0,
            MinLevel = 1,
            QuestSortID = 12,
            QuestInfoID = 0,
            SuggestedGroupNum = 0,
            RewardNextQuest = 28774,
            RewardXPDifficulty = 8,
            RewardXPMultiplier = 1.0f,
            RewardMoney = 50,
            RewardMoneyDifficulty = 5,
            RewardMoneyMultiplier = 1.0f,
            RewardBonusMoney = 300,
            RewardArtifactXPMultiplier = 1.0f,
            Flags = 524288,
            AcceptedSoundKitID = 890,
            CompleteSoundKitID = 878,
            AllowableRaces = 1,
            LogTitle = "Beating Them Back!",
            LogDescription = "Kill 6 Blackrock Battle Worgs.",
            QuestDescription = "So you're the new recruit from Stormwind, eh? I'm Marshal McBride, commander of this garrison. Glad to have you on board...$B$B<McBride looks through some papers.>$B$B$N. It is $N, right?$B$BYou've arrived just in time. The Blackrock orcs have managed to sneak into Northshire through a break in the mountain. My soldiers are doing the best that they can to push them back, but I fear they will be overwhelmed soon.$B$BHead northwest into the forest and kill the attacking Blackrock worgs! Help my soldiers!",
            PortraitGiverText = "This is a Blackrock Battle Worg.",
            PortraitGiverName = "Blackrock Battle Worg",
            QuestCompletionLog = "Return to Marshal McBride at Northshire Abbey in Elwynn Forest.",
        };
        info.RewardItems[0] = 57255;
        info.RewardAmount[0] = 1;
        info.RewardFactionID[0] = 72;
        info.RewardFactionValue[0] = 5;
        info.RewardFactionCapIn[0] = 7;
        info.RewardFactionCapIn[1] = 7;
        info.RewardFactionCapIn[2] = 7;
        info.RewardFactionCapIn[3] = 7;
        info.RewardFactionCapIn[4] = 7;
        info.Objectives.Add(new QuestObjective { Id = 442541, Type = (QuestObjectiveType)0, StorageIndex = 0, ObjectID = 49871, Amount = 6 });
        return new QueryQuestInfoResponse { QuestID = 28766, Allow = true, Info = info };
    }

    private static QueryQuestInfoResponse NativeSample932()
    {
        var info = new QuestTemplate
        {
            QuestID = 12,
            QuestType = 2,
            QuestLevel = 12,
            QuestScalingFactionGroup = 0,
            QuestMaxScalingLevel = 255,
            QuestPackageID = 0,
            MinLevel = 9,
            QuestSortID = 40,
            QuestInfoID = 0,
            SuggestedGroupNum = 0,
            RewardNextQuest = 13,
            RewardXPDifficulty = 5,
            RewardXPMultiplier = 1.0f,
            RewardMoney = 500,
            RewardMoneyDifficulty = 5,
            RewardMoneyMultiplier = 1.0f,
            RewardBonusMoney = 540,
            RewardArtifactXPMultiplier = 1.0f,
            Flags = 16392,
            AcceptedSoundKitID = 890,
            CompleteSoundKitID = 878,
            AllowableRaces = -1,
            LogTitle = "The People's Militia",
            LogDescription = "Gryan Stoutmantle wants you to kill 15 Defias Trappers and 15 Defias Smugglers then return to him on Sentinel Hill.",
            QuestDescription = "The People's Militia has but one goal:  To defend the lands of Westfall and return peace to our surroundings.  Unfortunately, the price of peace is often blood. $b$bOne of my scouts has brought word of a band of Defias Trappers wreaking havoc nearby.  I have reports of Defias Trapper sightings near the Jangolode Mine to the Northwest as well as at the Molsen Farm and Furlbrow's Pumpkin Farm. If you seek to join our ranks, slay 15 Defias Trappers and 15 Defias Smugglers then return to me.",
            QuestCompletionLog = "Return to Gryan Stoutmantle at Sentinel Hill in Westfall.",
        };
        info.RewardFactionID[0] = 72;
        info.RewardFactionValue[0] = 5;
        info.RewardFactionCapIn[0] = 7;
        info.RewardFactionCapIn[1] = 7;
        info.RewardFactionCapIn[2] = 7;
        info.RewardFactionCapIn[3] = 7;
        info.RewardFactionCapIn[4] = 7;
        info.Objectives.Add(new QuestObjective { Id = 379993, Type = (QuestObjectiveType)0, StorageIndex = 0, ObjectID = 504, Amount = 15 });
        info.Objectives.Add(new QuestObjective { Id = 379994, Type = (QuestObjectiveType)0, StorageIndex = 1, ObjectID = 95, Amount = 15 });
        return new QueryQuestInfoResponse { QuestID = 12, Allow = true, Info = info };
    }

    // SMSG_QUERY_QUEST_INFO_RESPONSE from TrinityCore cata_classic to a 4.4.2.60895 client
    // (refs/native-captures/tc_cata_442_20261010-150803.pkt, packet 925).
    private const string Native925 =
        "5E700000805E700000020000000300000000000000FF00000000000000010000000C000000000000000000000066700000080000000000803F32000000050000" +
        "000000803F2C010000000000000000000000000000000000000000000000000000000000000000803F0000000000000000000008000000000000000000A7DF00" +
        "00010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" +
        "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" +
        "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000004800000005000000000000" +
        "00070000000000000000000000000000000700000000000000000000000000000007000000000000000000000000000000070000000000000000000000000000" +
        "00070000000000000000000000000000000000000000000000000000000000000000000000000000007A0300006E030000000000000000000000000000010000" +
        "0001000000000000000000000000000000000000000000000000000000000000000900F0FD8002015000001F80ADC006000000000000CFC20000060000000000" +
        "00000000000000000000000000000042656174696E67205468656D204261636B214B696C6C203620426C61636B726F636B20426174746C6520576F7267732E53" +
        "6F20796F7527726520746865206E657720726563727569742066726F6D2053746F726D77696E642C2065683F2049276D204D61727368616C204D634272696465" +
        "2C20636F6D6D616E646572206F662074686973206761727269736F6E2E20476C616420746F206861766520796F75206F6E20626F6172642E2E2E244224423C4D" +
        "634272696465206C6F6F6B73207468726F75676820736F6D65207061706572732E3E24422442244E2E20497420697320244E2C2072696768743F24422442596F" +
        "752776652061727269766564206A75737420696E2074696D652E2054686520426C61636B726F636B206F7263732068617665206D616E6167656420746F20736E" +
        "65616B20696E746F204E6F7274687368697265207468726F756768206120627265616B20696E20746865206D6F756E7461696E2E204D7920736F6C6469657273" +
        "2061726520646F696E67207468652062657374207468617420746865792063616E20746F2070757368207468656D206261636B2C206275742049206665617220" +
        "746865792077696C6C206265206F7665727768656C6D656420736F6F6E2E2442244248656164206E6F7274687765737420696E746F2074686520666F72657374" +
        "20616E64206B696C6C207468652061747461636B696E6720426C61636B726F636B20776F726773212048656C70206D7920736F6C646965727321546869732069" +
        "73206120426C61636B726F636B20426174746C6520576F72672E426C61636B726F636B20426174746C6520576F726752657475726E20746F204D61727368616C" +
        "204D634272696465206174204E6F727468736869726520416262657920696E20456C77796E6E20466F726573742E";

    // SMSG_QUERY_QUEST_INFO_RESPONSE from TrinityCore cata_classic to a 4.4.2.60895 client
    // (refs/native-captures/tc_cata_442_20261010-150803.pkt, packet 932).
    private const string Native932 =
        "0C000000800C000000020000000C00000000000000FF00000000000000090000002800000000000000000000000D000000050000000000803FF4010000050000" +
        "000000803F1C020000000000000000000000000000000000000000000000000000000000000000803F0000000000000000084000000000000000000000000000" +
        "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" +
        "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" +
        "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000004800000005000000000000" +
        "00070000000000000000000000000000000700000000000000000000000000000007000000000000000000000000000000070000000000000000000000000000" +
        "00070000000000000000000000000000000000000000000000000000000000000000000000000000007A0300006E030000000000000000000000000000020000" +
        "00FFFFFFFFFFFFFFFF0000000000000000000000000000000000000000000000000A0398F60000000000001C8059CC05000000000000F80100000F0000000000" +
        "0000000000000000000000000000005ACC050000000000015F0000000F00000000000000000000000000000000000000005468652050656F706C652773204D69" +
        "6C69746961477279616E2053746F75746D616E746C652077616E747320796F7520746F206B696C6C2031352044656669617320547261707065727320616E6420" +
        "31352044656669617320536D7567676C657273207468656E2072657475726E20746F2068696D206F6E2053656E74696E656C2048696C6C2E5468652050656F70" +
        "6C652773204D696C697469612068617320627574206F6E6520676F616C3A2020546F20646566656E6420746865206C616E6473206F66205765737466616C6C20" +
        "616E642072657475726E20706561636520746F206F757220737572726F756E64696E67732E2020556E666F7274756E6174656C792C2074686520707269636520" +
        "6F66207065616365206973206F6674656E20626C6F6F642E20246224624F6E65206F66206D792073636F757473206861732062726F7567687420776F7264206F" +
        "6620612062616E64206F662044656669617320547261707065727320777265616B696E67206861766F63206E65617262792E2020492068617665207265706F72" +
        "7473206F66204465666961732054726170706572207369676874696E6773206E65617220746865204A616E676F6C6F6465204D696E6520746F20746865204E6F" +
        "727468776573742061732077656C6C20617320617420746865204D6F6C73656E204661726D20616E64204675726C62726F7727732050756D706B696E20466172" +
        "6D2E20496620796F75207365656B20746F206A6F696E206F75722072616E6B732C20736C61792031352044656669617320547261707065727320616E64203135" +
        "2044656669617320536D7567676C657273207468656E2072657475726E20746F206D652E52657475726E20746F20477279616E2053746F75746D616E746C6520" +
        "61742053656E74696E656C2048696C6C20696E205765737466616C6C2E";

    // QueryQuestInfoResponse.Write as it stood before the layouts, frozen; isV343 was
    // ModernVersion.IsWotLKClassicOrLater.

    private static byte[] OldQuestInfo(QueryQuestInfoResponse p, bool isV343)
    {
        using var w = new WorldPacket(1u);
        w.WriteUInt32(p.QuestID);
        w.WriteBit(p.Allow);
        w.FlushBits();

        if (p.Allow)
        {
            w.WriteUInt32(p.Info.QuestID);
            w.WriteInt32(p.Info.QuestType);
            w.WriteInt32(p.Info.QuestLevel);
            w.WriteInt32(p.Info.QuestScalingFactionGroup);
            w.WriteInt32(p.Info.QuestMaxScalingLevel);
            w.WriteUInt32(p.Info.QuestPackageID);
            w.WriteInt32(p.Info.MinLevel);
            w.WriteInt32(p.Info.QuestSortID);
            w.WriteUInt32(p.Info.QuestInfoID);
            w.WriteUInt32(p.Info.SuggestedGroupNum);
            w.WriteUInt32(p.Info.RewardNextQuest);
            w.WriteUInt32(p.Info.RewardXPDifficulty);

            w.WriteFloat(p.Info.RewardXPMultiplier);

            w.WriteInt32(p.Info.RewardMoney);
            w.WriteUInt32(p.Info.RewardMoneyDifficulty);
            w.WriteFloat(p.Info.RewardMoneyMultiplier);
            w.WriteUInt32(p.Info.RewardBonusMoney);

            for (uint i = 0; i < QuestConst.QuestRewardDisplaySpellCount; ++i)
                w.WriteUInt32(p.Info.RewardDisplaySpell[i]);

            w.WriteUInt32(p.Info.RewardSpell);
            w.WriteUInt32(p.Info.RewardHonor);

            w.WriteFloat(p.Info.RewardKillHonor);

            w.WriteInt32(p.Info.RewardArtifactXPDifficulty);
            w.WriteFloat(p.Info.RewardArtifactXPMultiplier);
            w.WriteInt32(p.Info.RewardArtifactCategoryID);

            w.WriteUInt32(p.Info.StartItem);
            w.WriteUInt32(p.Info.Flags);
            w.WriteUInt32(p.Info.FlagsEx);
            w.WriteUInt32(p.Info.FlagsEx2);

            for (uint i = 0; i < QuestConst.QuestRewardItemCount; ++i)
            {
                w.WriteUInt32(p.Info.RewardItems[i]);
                w.WriteUInt32(p.Info.RewardAmount[i]);
                w.WriteInt32(p.Info.ItemDrop[i]);
                w.WriteInt32(p.Info.ItemDropQuantity[i]);
            }

            for (uint i = 0; i < QuestConst.QuestRewardChoicesCount; ++i)
            {
                w.WriteUInt32(p.Info.UnfilteredChoiceItems[i].ItemID);
                w.WriteUInt32(p.Info.UnfilteredChoiceItems[i].Quantity);
                w.WriteUInt32(p.Info.UnfilteredChoiceItems[i].DisplayID);
            }

            w.WriteUInt32(p.Info.POIContinent);
            w.WriteFloat(p.Info.POIx);
            w.WriteFloat(p.Info.POIy);
            w.WriteUInt32(p.Info.POIPriority);

            w.WriteUInt32(p.Info.RewardTitle);
            w.WriteInt32(p.Info.RewardArenaPoints);
            w.WriteUInt32(p.Info.RewardSkillLineID);
            w.WriteUInt32(p.Info.RewardNumSkillUps);

            // V3_4_3 layout (fork QueryQuestInfoResponse:82-112): adds
            // PortraitGiverModelSceneID between Mount and TurnIn, uses INT32 for
            // portrait fields (instead of UINT32), promotes TimeAllowed from
            // UINT32 to INT64, treats AllowableRaces as UINT64, and appends
            // ManagedWorldStateID/QuestSessionBonus/QuestGiverCreatureID. Without
            // these, the V3_4_3 client mis-parses the title-length bits at line
            // ~113 of the writer, then reads garbage as a ConditionalQuestText
            // length prefix → ~5 TB allocation crash (?AUConditionalQuestText@@).
            
            if (isV343)
            {
                w.WriteInt32((int)p.Info.PortraitGiver);
                w.WriteInt32((int)p.Info.PortraitGiverMount);
                w.WriteInt32((int)p.Info.PortraitGiverModelSceneID);
                w.WriteInt32((int)p.Info.PortraitTurnIn);
            }
            else
            {
                w.WriteUInt32(p.Info.PortraitGiver);
                w.WriteUInt32(p.Info.PortraitGiverMount);
                w.WriteUInt32(p.Info.PortraitTurnIn);

                w.WriteInt32(0); // Unk 2.5.2
            }

            for (uint i = 0; i < QuestConst.QuestRewardReputationsCount; ++i)
            {
                w.WriteUInt32(p.Info.RewardFactionID[i]);
                w.WriteInt32(p.Info.RewardFactionValue[i]);
                w.WriteInt32(p.Info.RewardFactionOverride[i]);
                w.WriteInt32(p.Info.RewardFactionCapIn[i]);
            }

            w.WriteUInt32(p.Info.RewardFactionFlags);

            for (uint i = 0; i < QuestConst.QuestRewardCurrencyCount; ++i)
            {
                w.WriteUInt32(p.Info.RewardCurrencyID[i]);
                w.WriteUInt32(p.Info.RewardCurrencyQty[i]);
            }

            w.WriteUInt32(p.Info.AcceptedSoundKitID);
            w.WriteUInt32(p.Info.CompleteSoundKitID);

            if (isV343)
            {
                w.WriteInt32((int)p.Info.AreaGroupID);
                w.WriteInt64(p.Info.TimeAllowed);
            }
            else
            {
                w.WriteUInt32(p.Info.AreaGroupID);
                w.WriteUInt32(p.Info.TimeAllowed);
            }

            w.WriteInt32(p.Info.Objectives.Count);
            if (isV343)
                w.WriteUInt64((ulong)p.Info.AllowableRaces);
            else
                w.WriteInt64(p.Info.AllowableRaces);
            w.WriteInt32(p.Info.TreasurePickerID);
            w.WriteInt32(p.Info.Expansion);

            if (isV343)
            {
                w.WriteInt32(p.Info.ManagedWorldStateID);
                w.WriteInt32(p.Info.QuestSessionBonus);
                w.WriteInt32((int)p.Info.QuestGiverCreatureID);
            }

            w.WriteBits(p.Info.LogTitle.GetByteCount(), 9);
            w.WriteBits(p.Info.LogDescription.GetByteCount(), 12);
            w.WriteBits(p.Info.QuestDescription.GetByteCount(), 12);
            w.WriteBits(p.Info.AreaDescription.GetByteCount(), 9);
            w.WriteBits(p.Info.PortraitGiverText.GetByteCount(), 10);
            w.WriteBits(p.Info.PortraitGiverName.GetByteCount(), 8);
            w.WriteBits(p.Info.PortraitTurnInText.GetByteCount(), 10);
            w.WriteBits(p.Info.PortraitTurnInName.GetByteCount(), 8);
            w.WriteBits(p.Info.QuestCompletionLog.GetByteCount(), 11);
            w.WriteBit(p.Info.ReadyForTranslation);
            w.FlushBits();

            foreach (QuestObjective questObjective in p.Info.Objectives)
            {
                w.WriteUInt32(questObjective.Id);
                w.WriteUInt8((byte)questObjective.Type);
                w.WriteInt8(questObjective.StorageIndex);
                w.WriteInt32(questObjective.ObjectID);
                w.WriteInt32(questObjective.Amount);
                w.WriteUInt32((uint)questObjective.Flags);
                w.WriteUInt32(questObjective.Flags2);
                w.WriteFloat(questObjective.ProgressBarWeight);

                w.WriteInt32(questObjective.VisualEffects.Length);
                foreach (var visualEffect in questObjective.VisualEffects)
                    w.WriteInt32(visualEffect);

                w.WriteBits(questObjective.Description.GetByteCount(), 8);
                w.FlushBits();

                w.WriteString(questObjective.Description);
            }

            w.WriteString(p.Info.LogTitle);
            w.WriteString(p.Info.LogDescription);
            w.WriteString(p.Info.QuestDescription);
            w.WriteString(p.Info.AreaDescription);
            w.WriteString(p.Info.PortraitGiverText);
            w.WriteString(p.Info.PortraitGiverName);
            w.WriteString(p.Info.PortraitTurnInText);
            w.WriteString(p.Info.PortraitTurnInName);
            w.WriteString(p.Info.QuestCompletionLog);
        }
        return w.GetDataSpan().ToArray();
    }
}
