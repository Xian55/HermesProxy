using System;
using System.Reflection;
using System.Text;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// Small packets of a 4.3.4 server that had no handler: who joins and leaves a channel, the
/// graveyards for a dead player's map, another player's skipped time, the guild group state and the
/// kinds of PvP on offer. The incoming bytes are written out after TrinityCore 4.3.4's writers
/// (Channel::JoinNotify and LeaveNotify, RequestCemeteryListResponse::Write,
/// HandleMoveTimeSkippedOpcode, Guild::HandleGuildPartyRequest, PVPOptionsEnabled::Write); the
/// outgoing ones after the 4.4.2 client's layouts.
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
}
