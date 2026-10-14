using System;
using System.Reflection;
using System.Text;
using Framework.IO;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// Small packets of a 4.3.4 server that had no handler: who joins and leaves a channel, the
/// graveyards for a dead player's map, another player's skipped time, the guild group state, the
/// kinds of PvP on offer and whom a quest is handed in to. The incoming bytes are written out
/// after TrinityCore 4.3.4's writers (Channel::JoinNotify and LeaveNotify,
/// RequestCemeteryListResponse::Write, HandleMoveTimeSkippedOpcode,
/// Guild::HandleGuildPartyRequest, PVPOptionsEnabled::Write, HandleQuestNPCQuery); the outgoing
/// ones after the 4.4.2 client's layouts.
/// </summary>
public sealed class CataSmallPacketTests
{
    private static readonly PropertyInfo WorldPacketField =
        typeof(ServerPacket).GetProperty("_worldPacket", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private readonly LegacyHandlerHarness _harness = new(recordClientPackets: true);
    private readonly WowGuid128 _other;

    public CataSmallPacketTests()
    {
        _harness.SetActivePlayer(new WowGuid64(2));
        _other = _harness.AddKnownObject(new WowGuid64(0x21), ObjectType.Player);
    }

    // a received packet starts with its opcode, which the handlers never look at
    private static byte[] Wire(string hex) => [0, 0, .. Convert.FromHexString(hex.Replace(" ", ""))];

    private static string Text(string text) => Convert.ToHexString(Encoding.UTF8.GetBytes(text)) + "00";

    private T Deliver<T>(Opcode opcode, string hex, Action<WorldPacket> handler) where T : ServerPacket
    {
        _harness.Deliver(opcode, Wire(hex), handler);
        return Assert.IsType<T>(_harness.ClientWire.Sent[^1].Packet);
    }

    private static byte[] Written(ServerPacket packet)
    {
        packet.Write();
        return ((WorldPacket)WorldPacketField.GetValue(packet)!).GetData();
    }

    [Fact]
    public void HandleUserlistAdd_NamesThePlayerTheChannelAndItsId()
    {
        // GUID 0x21, member flags 2, channel flags 0x18, three members, the name
        var joined = Deliver<UserlistAdd>(Opcode.SMSG_USERLIST_ADD,
            "21 00 00 00 00 00 00 00 02 18 03 00 00 00" + Text("Trade - City"), _harness.Client.HandleUserlistAddCata);

        Assert.Equal(_other, joined.UserGUID);
        Assert.Equal(2, joined.UserFlags);
        Assert.Equal(0x18u, joined.ChannelFlags);
        Assert.Equal("Trade - City", joined.ChannelName);
    }

    [Fact]
    public void HandleUserlistAdd_UsesTheIdTheChannelWasJoinedWith()
    {
        _harness.Session.GameState.SetChannelId("Trade - City", 2);

        var joined = Deliver<UserlistAdd>(Opcode.SMSG_USERLIST_ADD,
            "21 00 00 00 00 00 00 00 00 18 03 00 00 00" + Text("Trade - City"), _harness.Client.HandleUserlistAddCata);

        Assert.Equal(2u, joined.ChannelID);
    }

    [Fact]
    public void HandleUserlistRemove_HasNoMemberFlags()
    {
        var left = Deliver<UserlistRemove>(Opcode.SMSG_USERLIST_REMOVE,
            "21 00 00 00 00 00 00 00 18 02 00 00 00" + Text("Trade - City"), _harness.Client.HandleUserlistRemoveCata);

        Assert.Equal(_other, left.UserGUID);
        Assert.Equal(0x18u, left.ChannelFlags);
        Assert.Equal("Trade - City", left.ChannelName);
    }

    [Theory]
    [InlineData(false, 0x40000Du)]
    [InlineData(true, 0x40000Fu)]
    public void UserlistAdd_AndUpdate_ShareALayout(bool update, uint opcode)
    {
        var packet = new UserlistAdd(update) { UserFlags = 2, ChannelFlags = 0x18, ChannelID = 2, ChannelName = "Trade" };

        Assert.Equal(update ? Opcode.SMSG_USERLIST_UPDATE : Opcode.SMSG_USERLIST_ADD, packet.GetUniversalOpcode());
        Assert.Equal(opcode, (uint)global::HermesProxy.World.Enums.V4_4_2_60895.Opcode.SMSG_USERLIST_ADD + (update ? 2u : 0u));
        Assert.Equal(
            [
                0x00, 0x00,                     // an empty GUID
                0x02,                           // member flags
                0x18, 0x00, 0x00, 0x00,         // channel flags
                0x02, 0x00, 0x00, 0x00,         // channel id
                0x0A,                           // seven bits of name length: 5
                0x54, 0x72, 0x61, 0x64, 0x65,   // Trade
            ],
            Written(packet));
    }

    [Fact]
    public void UserlistRemove_Layout()
    {
        var packet = new UserlistRemove { ChannelFlags = 0x18, ChannelID = 2, ChannelName = "Trade" };

        Assert.Equal(
            [
                0x00, 0x00,
                0x18, 0x00, 0x00, 0x00,
                0x02, 0x00, 0x00, 0x00,
                0x0A,
                0x54, 0x72, 0x61, 0x64, 0x65,
            ],
            Written(packet));
    }

    [Fact]
    public void HandleRequestCemeteryListResponse_ReadsTheCountFromTwentyFourBits()
    {
        // not from a gossip, two graveyards: 4 and 106
        var list = Deliver<RequestCemeteryListResponse>(Opcode.SMSG_REQUEST_CEMETERY_LIST_RESPONSE,
            "00 00 01 00 04 00 00 00 6a 00 00 00", _harness.Client.HandleRequestCemeteryListResponse);

        Assert.False(list.IsGossipTriggered);
        Assert.Equal([4u, 106u], list.CemeteryID);
    }

    [Fact]
    public void HandleRequestCemeteryListResponse_FromAGossip_SetsTheBit()
    {
        var list = Deliver<RequestCemeteryListResponse>(Opcode.SMSG_REQUEST_CEMETERY_LIST_RESPONSE,
            "80 00 00 80 04 00 00 00", _harness.Client.HandleRequestCemeteryListResponse);

        Assert.True(list.IsGossipTriggered);
        Assert.Equal([4u], list.CemeteryID);
    }

    [Fact]
    public void RequestCemeteryListResponse_Layout()
    {
        var packet = new RequestCemeteryListResponse { IsGossipTriggered = true, CemeteryID = [4, 106] };

        Assert.Equal(
            [
                0x80,                       // the one bit, then the byte is closed
                0x02, 0x00, 0x00, 0x00,
                0x04, 0x00, 0x00, 0x00,
                0x6A, 0x00, 0x00, 0x00,
            ],
            Written(packet));
    }

    [Fact]
    public void HandleMoveTimeSkippedCata_ReadsTheTimeBeforeTheGuid()
    {
        // 1000 ms, then GUID 0x21 packed
        var skip = Deliver<MoveSkipTime>(Opcode.MSG_MOVE_TIME_SKIPPED, "e8 03 00 00 01 21",
            _harness.Client.HandleMoveTimeSkippedCata);

        Assert.Equal(_other, skip.MoverGUID);
        Assert.Equal(1000u, skip.TimeSkipped);
    }

    [Fact]
    public void MoveSkipTime_Layout()
    {
        var packet = new MoveSkipTime { TimeSkipped = 1000 };

        Assert.Equal([0x00, 0x00, 0xE8, 0x03, 0x00, 0x00], Written(packet));
    }

    [Fact]
    public void HandleGuildPartyState_ReadsTheMultiplierBeforeTheCounts()
    {
        // a guild group, experience times 1.5, three of the guild in it, three needed
        var state = Deliver<GuildPartyState>(Opcode.SMSG_GUILD_PARTY_STATE,
            "80 00 00 c0 3f 03 00 00 00 03 00 00 00", _harness.Client.HandleGuildPartyState);

        Assert.True(state.InGuildParty);
        Assert.Equal(1.5f, state.GuildXPEarnedMult);
        Assert.Equal(3, state.NumMembers);
        Assert.Equal(3, state.NumRequired);
    }

    [Fact]
    public void GuildPartyState_WritesTheCountsBeforeTheMultiplier()
    {
        var packet = new GuildPartyState { InGuildParty = true, NumMembers = 2, NumRequired = 3, GuildXPEarnedMult = 1.5f };

        Assert.Equal(
            [
                0x80,
                0x02, 0x00, 0x00, 0x00,
                0x03, 0x00, 0x00, 0x00,
                0x00, 0x00, 0xC0, 0x3F,
            ],
            Written(packet));
    }

    [Theory]
    [InlineData("f8", true, true, true, true, true)]        // what TrinityCore 4.3.4 sends
    [InlineData("00", false, false, false, false, false)]
    [InlineData("48", false, true, false, false, true)]     // war game battlegrounds and rated arenas
    public void HandlePvPOptionsEnabled_ReadsFiveBits(string hex, bool pug, bool wargameBattlegrounds, bool wargameArenas,
        bool ratedBattlegrounds, bool ratedArenas)
    {
        var options = Deliver<PVPOptionsEnabled>(Opcode.SMSG_PVP_OPTIONS_ENABLED, hex, _harness.Client.HandlePvPOptionsEnabled);

        Assert.Equal(pug, options.PugBattlegrounds);
        Assert.Equal(wargameBattlegrounds, options.WargameBattlegrounds);
        Assert.Equal(wargameArenas, options.WargameArenas);
        Assert.Equal(ratedBattlegrounds, options.RatedBattlegrounds);
        Assert.Equal(ratedArenas, options.RatedArenas);
    }

    /// <summary>Rated battlegrounds lead on the 4.4.2 wire, and five more bits follow the five.</summary>
    [Fact]
    public void PVPOptionsEnabled_WritesTenBits_RatedBattlegroundsFirst()
    {
        Assert.Equal([0xF8, 0x00], Written(new PVPOptionsEnabled
        {
            RatedBattlegrounds = true, PugBattlegrounds = true, WargameBattlegrounds = true, WargameArenas = true, RatedArenas = true,
        }));
        Assert.Equal([0x80, 0x00], Written(new PVPOptionsEnabled { RatedBattlegrounds = true }));
        Assert.Equal([0x08, 0x00], Written(new PVPOptionsEnabled { RatedArenas = true }));
    }

    [Fact]
    public void HandleQuestCompletionNPCResponse_ReadsAllCountsBeforeAnyQuest()
    {
        // two quests with one and two entries: 23 bits, 24, 24; then quest 100 with creature 5000,
        // and quest 200 with creature 6000 and game object 7000
        var answer = Deliver<QuestCompletionNPCResponse>(Opcode.SMSG_QUEST_COMPLETION_NPC_RESPONSE,
            "00 00 04 00 00 02 00 00 04 64 00 00 00 88 13 00 00 c8 00 00 00 70 17 00 00 58 1b 00 80",
            _harness.Client.HandleQuestCompletionNPCResponse);

        Assert.Equal(2, answer.Quests.Count);
        Assert.Equal(100, answer.Quests[0].QuestID);
        Assert.Equal([5000u], answer.Quests[0].NPCs);
        Assert.Equal(200, answer.Quests[1].QuestID);
        Assert.Equal([6000u, 7000u | 0x80000000u], answer.Quests[1].NPCs);
    }

    [Fact]
    public void HandleQuestCompletionNPCResponse_OfNoQuests_IsAnEmptyAnswer()
    {
        var answer = Deliver<QuestCompletionNPCResponse>(Opcode.SMSG_QUEST_COMPLETION_NPC_RESPONSE,
            "00 00 00", _harness.Client.HandleQuestCompletionNPCResponse);

        Assert.Empty(answer.Quests);
    }

    [Fact]
    public void QuestCompletionNPCResponse_WritesEachQuestWithItsOwnCount()
    {
        var packet = new QuestCompletionNPCResponse();
        packet.Quests.Add((100, [5000]));
        packet.Quests.Add((200, [6000, 7000 | 0x80000000]));

        Assert.Equal(
            [
                0x02, 0x00, 0x00, 0x00,
                0x64, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x88, 0x13, 0x00, 0x00,
                0xC8, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x70, 0x17, 0x00, 0x00, 0x58, 0x1B, 0x00, 0x80,
            ],
            Written(packet));
    }

    [Fact]
    public void HandleGuildChallengeUpdate_DropsTheExperienceAndReordersTheRest()
    {
        // experience 0 30 40 50, gold at the cap 0 125 500 250, counting 0 7 1 3, gold 0 25 100 50, done 0 2 0 1
        var challenges = Deliver<GuildChallengeUpdate>(Opcode.SMSG_GUILD_CHALLENGE_UPDATE,
            Ints(0, 30, 40, 50) + Ints(0, 125, 500, 250) + Ints(0, 7, 1, 3) + Ints(0, 25, 100, 50) + Ints(0, 2, 0, 1),
            _harness.Client.HandleGuildChallengeUpdate);

        Assert.Equal([0, 2, 0, 1, 0, 0], challenges.CurrentCount);
        Assert.Equal([0, 7, 1, 3, 0, 0], challenges.MaxCount);
        Assert.Equal([0, 125, 500, 250, 0, 0], challenges.MaxLevelGold);
        Assert.Equal([0, 25, 100, 50, 0, 0], challenges.Gold);
    }

    /// <summary>Done, counting, gold, gold at the level cap: the order the client's guild window showed.</summary>
    [Fact]
    public void GuildChallengeUpdate_IsFourRunsOfSix_DoneFirst()
    {
        var packet = new GuildChallengeUpdate();
        packet.CurrentCount[1] = 2;
        packet.MaxCount[1] = 7;
        packet.MaxLevelGold[1] = 125;
        packet.Gold[1] = 25;

        byte[] written = Written(packet);

        Assert.Equal(4 * GuildChallengeUpdate.Types * sizeof(int), written.Length);
        Assert.Equal(2, BitConverter.ToInt32(written, 1 * 4));
        Assert.Equal(7, BitConverter.ToInt32(written, (6 + 1) * 4));
        Assert.Equal(25, BitConverter.ToInt32(written, (12 + 1) * 4));
        Assert.Equal(125, BitConverter.ToInt32(written, (18 + 1) * 4));
    }

    [Fact]
    public void HandleCalendarSendNumPending_PassesTheCountOn()
    {
        var pending = Deliver<CalendarSendNumPending>(Opcode.SMSG_CALENDAR_SEND_NUM_PENDING,
            Ints(3), _harness.Client.HandleCalendarSendNumPending);

        Assert.Equal(3u, pending.NumPending);
        Assert.Equal(Convert.FromHexString(Ints(3)), Written(pending));
    }

    [Fact]
    public void GuildSetAchievementTrackingCodec_ReadsACountAndTheIds()
    {
        var reader = new SpanPacketReader(Convert.FromHexString(Ints(2, 4860, 5014)));

        GuildSetAchievementTrackingCodec.Read(ref reader, out var tracking);

        Assert.Equal([4860u, 5014u], tracking.AchievementIds);
    }

    [Fact]
    public void GuildSetAchievementTrackingCodec_RefusesACountThePacketCannotBack()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var reader = new SpanPacketReader(Convert.FromHexString(Ints(3, 4860)));
            GuildSetAchievementTrackingCodec.Read(ref reader, out _);
        });
    }

    [Fact]
    public void SetCurrencyFlagsCodec_ReadsTheCurrencyThenTheFlags()
    {
        var reader = new SpanPacketReader(Convert.FromHexString(Ints(395, 4)));

        SetCurrencyFlagsCodec.Read(ref reader, out var currency);

        Assert.Equal(395u, currency.CurrencyID);
        Assert.Equal(4u, currency.Flags);
    }

    [Fact]
    public void CataCurrencyId_IsTheInverseOfModernCurrencyId()
    {
        Assert.Equal(392u, HermesProxy.World.Client.WorldClient.CataCurrencyId(1901));
        Assert.Equal(395u, HermesProxy.World.Client.WorldClient.CataCurrencyId(395));
        Assert.Equal(1901u, HermesProxy.World.Client.WorldClient.ModernCurrencyId(
            HermesProxy.World.Client.WorldClient.CataCurrencyId(1901)));
    }

    // a received packet as a 4.3.4 server's own writer puts it together
    private static byte[] Body(Action<WorldPacket> write)
    {
        using var packet = new WorldPacket();
        packet.WriteUInt16(0);
        write(packet);
        return packet.GetDataSpan().ToArray();
    }

    private T DeliverBody<T>(Opcode opcode, Action<WorldPacket> write, Action<WorldPacket> handler) where T : ServerPacket
    {
        _harness.Deliver(opcode, Body(write), handler);
        return Assert.IsType<T>(_harness.ClientWire.Sent[^1].Packet);
    }

    [Fact]
    public void HandleGuildMemberDailyReset_IsPassedOnEmpty()
    {
        var reset = Deliver<GuildMemberDailyReset>(Opcode.SMSG_GUILD_MEMBER_DAILY_RESET, "",
            _harness.Client.HandleGuildMemberDailyReset);

        Assert.Empty(Written(reset));
    }

    [Fact]
    public void HandleAllGuildAchievements_ReadsADateAndAnIdForEach()
    {
        // AchievementMgr<Guild>::SendAllAchievementData
        long earnedAt = 1_700_000_040;
        var achievements = DeliverBody<AllGuildAchievements>(Opcode.SMSG_ALL_GUILD_ACHIEVEMENTS, packet =>
        {
            packet.WriteBits(2, 23);
            packet.FlushBits();
            packet.WritePackedTime(earnedAt);
            packet.WriteUInt32(4860);
            packet.WritePackedTime(earnedAt);
            packet.WriteUInt32(5014);
        }, _harness.Client.HandleAllGuildAchievements);

        Assert.Equal([4860u, 5014u], achievements.Earned.ConvertAll(e => e.Id));
        Assert.Equal(Time.GetUnixTimeFromPackedTime(Time.GetPackedTimeFromUnixTime(earnedAt)), achievements.Earned[0].Date);
        Assert.Equal(WowGuid128.Empty, achievements.Earned[0].Owner);
        // a count, then id, date, an empty GUID (two mask bytes) and two realm addresses for each
        Assert.Equal(4 + 2 * (4 + 4 + 2 + 4 + 4), Written(achievements).Length);
    }

    [Fact]
    public void HandleGuildEventLogQueryResults_ReadsBothGuidsAroundTheNumbers()
    {
        // GuildEventLogQueryResults::Write, one entry: player 2 promoted player 0x21 to rank 3, 90 seconds ago
        const ulong player = 2, other = 0x21;
        var log = DeliverBody<GuildEventLogQueryResults>(Opcode.SMSG_GUILD_EVENT_LOG_QUERY_RESULTS, packet =>
        {
            packet.WriteBits(1, 23);
            MaskedGuid.WriteMaskBits(packet, player, [2, 4]);
            MaskedGuid.WriteMaskBits(packet, other, [7, 6]);
            MaskedGuid.WriteMaskBits(packet, player, [3]);
            MaskedGuid.WriteMaskBits(packet, other, [3, 5]);
            MaskedGuid.WriteMaskBits(packet, player, [7, 5, 0]);
            MaskedGuid.WriteMaskBits(packet, other, [4, 2, 0, 1]);
            MaskedGuid.WriteMaskBits(packet, player, [1, 6]);
            packet.FlushBits();
            MaskedGuid.WriteBytes(packet, other, [3, 2, 5]);
            packet.WriteUInt8(3);
            MaskedGuid.WriteBytes(packet, other, [4]);
            MaskedGuid.WriteBytes(packet, player, [0, 4]);
            packet.WriteUInt32(90);
            MaskedGuid.WriteBytes(packet, player, [7, 3]);
            MaskedGuid.WriteBytes(packet, other, [0, 6, 7]);
            MaskedGuid.WriteBytes(packet, player, [5]);
            packet.WriteUInt8(3);
            MaskedGuid.WriteBytes(packet, other, [1]);
            MaskedGuid.WriteBytes(packet, player, [2, 6, 1]);
        }, _harness.Client.HandleGuildEventLogQueryResults);

        var entry = Assert.Single(log.Entries);
        Assert.Equal(new WowGuid64(player).To128(_harness.Session.GameState), entry.PlayerGUID);
        Assert.Equal(_other, entry.OtherGUID);
        Assert.Equal(3, entry.TransactionType);
        Assert.Equal(3, entry.RankID);
        Assert.Equal(90u, entry.TransactionDate);
    }

    [Fact]
    public void HandleGuildEventLogQueryResults_NoSecondPlayer_IsAnEmptyGuid()
    {
        const ulong player = 2;
        var log = DeliverBody<GuildEventLogQueryResults>(Opcode.SMSG_GUILD_EVENT_LOG_QUERY_RESULTS, packet =>
        {
            packet.WriteBits(1, 23);
            MaskedGuid.WriteMaskBits(packet, player, [2, 4]);
            packet.WriteBits(0, 2);
            MaskedGuid.WriteMaskBits(packet, player, [3]);
            packet.WriteBits(0, 2);
            MaskedGuid.WriteMaskBits(packet, player, [7, 5, 0]);
            packet.WriteBits(0, 4);
            MaskedGuid.WriteMaskBits(packet, player, [1, 6]);
            packet.FlushBits();
            packet.WriteUInt8(0);       // rank
            MaskedGuid.WriteBytes(packet, player, [0, 4]);
            packet.WriteUInt32(5);      // seconds ago
            MaskedGuid.WriteBytes(packet, player, [7, 3, 5]);
            packet.WriteUInt8(2);       // joined
            MaskedGuid.WriteBytes(packet, player, [2, 6, 1]);
        }, _harness.Client.HandleGuildEventLogQueryResults);

        var entry = Assert.Single(log.Entries);
        Assert.Equal(WowGuid128.Empty, entry.OtherGUID);
        Assert.Equal(2, entry.TransactionType);
        Assert.Equal(5u, entry.TransactionDate);
    }

    [Fact]
    public void GuildEventLogQueryResults_Layout()
    {
        var log = new GuildEventLogQueryResults();
        log.Entries.Add(new GuildEventEntry { TransactionType = 2, RankID = 4, TransactionDate = 90 });

        // a count; two empty GUIDs of two mask bytes each, the kind, the rank, the age
        Assert.Equal(Convert.FromHexString("01000000" + "0000" + "0000" + "02" + "04" + "5A000000"), Written(log));
    }

    [Fact]
    public void HandleGuildRewardList_ReordersAnItemAndWidensItsRaceMask()
    {
        // GuildRewardList::Write: reputation, race mask, item, cost, an unused number, achievement; the time last
        var rewards = DeliverBody<GuildRewardList>(Opcode.SMSG_GUILD_REWARD_LIST, packet =>
        {
            packet.WriteBits(2, 21);
            packet.FlushBits();
            packet.WriteUInt32(5); packet.WriteUInt32(0x2FF); packet.WriteUInt32(63125);
            packet.WriteUInt64(3_000_000); packet.WriteUInt32(0); packet.WriteUInt32(4989);
            packet.WriteUInt32(4); packet.WriteUInt32(0xFFFFFFFF); packet.WriteUInt32(65435);
            packet.WriteUInt64(1_500_000); packet.WriteUInt32(0); packet.WriteUInt32(0);
            packet.WriteInt32(1_700_000_000);
        }, _harness.Client.HandleGuildRewardList);

        Assert.Equal(1_700_000_000, rewards.Version);
        Assert.Equal(2, rewards.RewardItems.Count);
        Assert.Equal(63125u, rewards.RewardItems[0].ItemID);
        Assert.Equal(5, rewards.RewardItems[0].MinGuildRep);
        Assert.Equal(0x2FFul, rewards.RewardItems[0].RaceMask);
        Assert.Equal(3_000_000ul, rewards.RewardItems[0].Cost);
        Assert.Equal([4989u], rewards.RewardItems[0].AchievementsRequired);
        Assert.Equal(0xFFFFFFFFul, rewards.RewardItems[1].RaceMask);
        Assert.Empty(rewards.RewardItems[1].AchievementsRequired);
    }

    [Fact]
    public void GuildRewardList_Layout()
    {
        var rewards = new GuildRewardList { Version = 7 };
        rewards.RewardItems.Add(new GuildRewardItem
        {
            ItemID = 63125, MinGuildRep = 5, RaceMask = 0x2FF, Cost = 3_000_000, AchievementsRequired = { 4989 },
        });

        Assert.Equal(Convert.FromHexString(
            "0700000000000000" + Ints(1)                    // the time of the list, how many
            + Ints(63125, 0, 1) + "FF02000000000000"         // item, unused, achievements, race mask
            + Ints(0, 5) + "C0C62D0000000000"                // guild level, reputation, cost
            + Ints(4989)), Written(rewards));
    }

    [Fact]
    public void RequestGuildRewardsListCodec_ReadsSixtyFourBits()
    {
        var reader = new SpanPacketReader(Convert.FromHexString("00F1536500000000"));

        RequestGuildRewardsListCodec.Read(ref reader, out var request);

        Assert.Equal(0x6553F100, request.CurrentVersion);
    }

    [Theory]
    [InlineData(1, Opcode.SMSG_TITLE_EARNED)]
    [InlineData(0, Opcode.SMSG_TITLE_LOST)]
    public void HandleTitleEarned_PicksThePacketByTheSecondNumber(int earned, Opcode expected)
    {
        var title = Deliver<TitleEarned>(Opcode.SMSG_TITLE_EARNED, Ints(43, earned), _harness.Client.HandleTitleEarnedCata);

        Assert.Equal(expected, _harness.ClientWire.Sent[^1].Opcode);
        Assert.Equal(Convert.FromHexString(Ints(43)), Written(title));
    }

    [Fact]
    public void FourByteNotices_ArePassedOnAsTheyCame()
    {
        Assert.Equal(Convert.FromHexString(Ints(16)), Written(Deliver<TriggerMovie>(
            Opcode.SMSG_TRIGGER_MOVIE, Ints(16), _harness.Client.HandleTriggerMovieCata)));
        Assert.Equal(Convert.FromHexString(Ints(3)), Written(Deliver<MountResult>(
            Opcode.SMSG_MOUNT_RESULT, Ints(3), _harness.Client.HandleMountResultCata)));
        Assert.Equal(Convert.FromHexString(Ints(4989)), Written(Deliver<AchievementDeleted>(
            Opcode.SMSG_ACHIEVEMENT_DELETED, Ints(4989), _harness.Client.HandleAchievementDeletedCata)));
    }

    [Fact]
    public void RealmQueryResponse_ForARealmThatWasFound_CarriesItsNames()
    {
        var response = new RealmQueryResponse { VirtualRealmAddress = 0x01010001, LookupState = RealmQueryResponse.Found };
        response.NameInfo = new VirtualRealmNameInfo(true, false, "Trinity 4.3.4", "Trinity4.3.4");

        // the address, found, then two bits and two 8-bit lengths (13 and 12) and the two names
        Assert.Equal(Convert.FromHexString("01000101" + "00" + "83" + "43" + "00"
            + Convert.ToHexString(Encoding.UTF8.GetBytes("Trinity 4.3.4")) + Convert.ToHexString(Encoding.UTF8.GetBytes("Trinity4.3.4"))), Written(response));
    }

    [Fact]
    public void RealmQueryResponse_ForARealmThatWasNot_IsTheAddressAndOne()
    {
        var response = new RealmQueryResponse { VirtualRealmAddress = 7 };

        Assert.Equal(Convert.FromHexString("07000000" + "01"), Written(response));
    }

    [Fact]
    public void QueryRealmNameCodec_ReadsTheAddress()
    {
        var reader = new SpanPacketReader(Convert.FromHexString("01000101"));

        QueryRealmNameCodec.Read(ref reader, out var query);

        Assert.Equal(0x01010001u, query.VirtualRealmAddress);
    }

    [Fact]
    public void HandleMoveSetActiveMover_ReadsTheMaskedGuid()
    {
        // MoveSetActiveMover::Write for player 0x21: only byte 0 is set, so one mask bit (the
        // fifth) and one byte, written as it is XORed with 1
        var mover = DeliverBody<MoveSetActiveMover>(Opcode.SMSG_MOVE_SET_ACTIVE_MOVER, packet =>
        {
            MaskedGuid.WriteMaskBits(packet, 0x21, [5, 7, 3, 6, 0, 4, 1, 2]);
            packet.FlushBits();
            MaskedGuid.WriteBytes(packet, 0x21, [6, 2, 3, 0, 5, 7, 1, 4]);
        }, _harness.Client.HandleMoveSetActiveMover);

        Assert.Equal(_other, mover.MoverGUID);
    }

    [Fact]
    public void HandleMoveSetActiveMover_AFullGuid_KeepsEveryByteInItsPlace()
    {
        // a creature's GUID has something in every byte: a wrong order would move one
        const ulong creature = 0xF130D4A900123456;
        var mover = DeliverBody<MoveSetActiveMover>(Opcode.SMSG_MOVE_SET_ACTIVE_MOVER, packet =>
        {
            MaskedGuid.WriteMaskBits(packet, creature, [5, 7, 3, 6, 0, 4, 1, 2]);
            packet.FlushBits();
            MaskedGuid.WriteBytes(packet, creature, [6, 2, 3, 0, 5, 7, 1, 4]);
        }, _harness.Client.HandleMoveSetActiveMover);

        Assert.Equal(new WowGuid64(creature).To128(_harness.Session.GameState), mover.MoverGUID);
    }

    private static string Ints(params int[] values)
        => string.Concat(Array.ConvertAll(values, v => Convert.ToHexString(BitConverter.GetBytes(v))));
}
