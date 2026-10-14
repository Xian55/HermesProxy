using System;
using System.Reflection;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using HermesProxy.World.Server.Systems;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// Guild news of a 4.3.4 server for the 4.4.2 client. The incoming bytes are a list as
/// TrinityCore 4.3.4 sent it in a session, and packets written out after its
/// <c>GuildNews::Write</c>; the outgoing ones follow the 4.4.2 client's layout.
/// </summary>
public sealed class CataGuildNewsTests
{
    private static readonly PropertyInfo WorldPacketField =
        typeof(ServerPacket).GetProperty("_worldPacket", BindingFlags.NonPublic | BindingFlags.Instance)!;

    // 14 October 2026, 03:19, as the server packed it
    private const uint PackedDate = 0x1A9358D3;

    private readonly LegacyHandlerHarness _harness = new(recordClientPackets: true);
    private readonly WowGuid128 _player;
    private readonly WowGuid128 _other;

    public CataGuildNewsTests()
    {
        _player = _harness.SetActivePlayer(new WowGuid64(2));
        _other = _harness.AddKnownObject(new WowGuid64(0x21), ObjectType.Player);
    }

    private readonly record struct Entry(ulong Member, int Kind, int Value, int Id, int Flags = 0, ulong[]? Present = null);

    // GuildNews::Write, TrinityCore 4.3.4. A received packet starts with its opcode, which the
    // handler never looks at.
    private static byte[] Body(params Entry[] entries)
    {
        using var packet = new WorldPacket();
        packet.WriteUInt16(0);
        packet.WriteBits(entries.Length, 21);
        foreach (Entry entry in entries)
        {
            ulong[] present = entry.Present ?? [];
            packet.WriteBits(present.Length, 26);
            MaskedGuid.WriteMaskBits(packet, entry.Member, [7]);
            foreach (ulong member in present)
                MaskedGuid.WriteMaskBits(packet, member, [7, 1, 5, 3, 4, 6, 0, 2]);
            MaskedGuid.WriteMaskBits(packet, entry.Member, [0, 6, 5, 4, 3, 1, 2]);
        }
        packet.FlushBits();
        foreach (Entry entry in entries)
        {
            foreach (ulong member in entry.Present ?? [])
                MaskedGuid.WriteBytes(packet, member, [0, 1, 4, 7, 5, 6, 3, 2]);
            MaskedGuid.WriteBytes(packet, entry.Member, [5]);
            packet.WriteInt32(entry.Flags);
            packet.WriteInt32(entry.Value);
            packet.WriteInt32(0);
            MaskedGuid.WriteBytes(packet, entry.Member, [7, 6, 2, 3, 0, 4, 1]);
            packet.WriteInt32(entry.Id);
            packet.WriteInt32(entry.Kind);
            packet.WriteUInt32(PackedDate);
        }
        return packet.GetDataSpan().ToArray();
    }

    private GuildNews Deliver(byte[] wire)
    {
        _harness.Deliver(Opcode.SMSG_GUILD_NEWS_UPDATE, wire, _harness.Client.HandleGuildNews);
        return Assert.IsType<GuildNews>(_harness.ClientWire.Sent[^1].Packet);
    }

    private static byte[] Written(ServerPacket packet)
    {
        packet.Write();
        return ((WorldPacket)WorldPacketField.GetValue(packet)!).GetData();
    }

    private static string Ints(params int[] values)
        => string.Concat(Array.ConvertAll(values, v => Convert.ToHexString(BitConverter.GetBytes(v))));

    [Fact]
    public void HandleGuildNews_AListAsTheServerSentIt()
    {
        // TrinityCore 4.3.4, a guild's first two news: player 2 earned achievement 9 (entry 0),
        // then crafted item 55060 (entry 1)
        var news = Deliver([0, 0, .. Convert.FromHexString(
            "000010" + "00000080" + "0000002000"
            + "00000000" + "09000000" + "00000000" + "03" + "00000000" + "01000000" + "D358931A"
            + "00000000" + "14D70000" + "00000000" + "03" + "01000000" + "04000000" + "D358931A")]);

        Assert.Equal(2, news.Events.Count);
        GuildNewsEvent achievement = news.Events[0];
        Assert.Equal(1, achievement.Type);
        Assert.Equal(9, achievement.Data[0]);
        Assert.Equal(_player, achievement.MemberGuid);
        Assert.Equal(0, achievement.Flags);
        Assert.Equal(Time.GetUnixTimeFromPackedTime(PackedDate), achievement.CompletedDate);
        Assert.Empty(achievement.MemberList);
        Assert.Null(achievement.Item);

        GuildNewsEvent crafted = news.Events[1];
        Assert.Equal(4, crafted.Type);
        Assert.Equal(55060, crafted.Data[0]);
        Assert.Equal(_player, crafted.MemberGuid);
    }

    [Fact]
    public void HandleGuildNews_HandsEveryIdOnOneHigher()
    {
        // the server numbers a guild's news from 0, an id the 4.4.2 client cannot pin
        var news = Deliver(Body(new Entry(2, Kind: 1, Value: 9, Id: 0), new Entry(2, Kind: 1, Value: 10, Id: 249)));

        Assert.Equal([1, 250], news.Events.ConvertAll(e => e.Id));
    }

    [Theory]
    [InlineData(3)]     // looted
    [InlineData(4)]     // crafted
    [InlineData(5)]     // purchased
    public void HandleGuildNews_AnItemEntry_KeepsItsValueAndGetsAnItemRecord(int kind)
    {
        var news = Deliver(Body(new Entry(2, kind, Value: 55060, Id: 0)));

        GuildNewsEvent entry = Assert.Single(news.Events);
        Assert.Equal(55060, entry.Data[0]);
        Assert.NotNull(entry.Item);
        Assert.Equal(55060u, entry.Item.ItemID);
    }

    [Theory]
    [InlineData(0)]     // guild achievement
    [InlineData(1)]     // player achievement
    [InlineData(2)]     // dungeon encounter
    public void HandleGuildNews_AnEntryThatIsNoItem_HasNoItemRecord(int kind)
    {
        var news = Deliver(Body(new Entry(2, kind, Value: 4860, Id: 3)));

        Assert.Null(Assert.Single(news.Events).Item);
    }

    [Fact]
    public void HandleGuildNews_AGuildLevel_IsLeftOutOfAList()
    {
        // the 4.4.2 client has no guild levels and no entry of this kind
        var news = Deliver(Body(new Entry(0, Kind: 6, Value: 2, Id: 0), new Entry(2, Kind: 1, Value: 9, Id: 1)));

        GuildNewsEvent entry = Assert.Single(news.Events);
        Assert.Equal(1, entry.Type);
        Assert.Equal(2, entry.Id);
    }

    [Fact]
    public void HandleGuildNews_AGuildLevelAlone_SendsNothing()
    {
        _harness.Deliver(Opcode.SMSG_GUILD_NEWS_UPDATE, Body(new Entry(0, Kind: 6, Value: 2, Id: 0)), _harness.Client.HandleGuildNews);

        Assert.Empty(_harness.ClientWire.Sent);
    }

    [Fact]
    public void HandleGuildNews_AnEmptyList_IsPassedOnEmpty()
    {
        // what a guild without news answers: the client's list is then known to be empty
        var news = Deliver(Body());

        Assert.Empty(news.Events);
        Assert.Equal(Convert.FromHexString(Ints(0)), Written(news));
    }

    [Fact]
    public void HandleGuildNews_AnEntryOfTheGuild_HasNoMember()
    {
        var news = Deliver(Body(new Entry(0, Kind: 0, Value: 4860, Id: 5)));

        Assert.Equal(WowGuid128.Empty, Assert.Single(news.Events).MemberGuid);
    }

    [Fact]
    public void HandleGuildNews_NamesTheMembersWhoWereThere()
    {
        // TrinityCore never fills the list, the layout has it: their masks sit inside the mask of
        // the entry's own member, their bytes in front of its
        var news = Deliver(Body(
            new Entry(2, Kind: 2, Value: 1035, Id: 4, Present: [0x21, 2]),
            new Entry(0x21, Kind: 1, Value: 9, Id: 5, Flags: 1)));

        Assert.Equal(2, news.Events.Count);
        Assert.Equal([_other, _player], news.Events[0].MemberList);
        Assert.Equal(_player, news.Events[0].MemberGuid);
        Assert.Equal(1035, news.Events[0].Data[0]);
        Assert.Empty(news.Events[1].MemberList);
        Assert.Equal(_other, news.Events[1].MemberGuid);
        Assert.Equal(1, news.Events[1].Flags);
        Assert.Equal(6, news.Events[1].Id);
    }

    [Fact]
    public void HandleGuildNews_ACountNoGuildHas_IsNotThisLayout()
    {
        using var packet = new WorldPacket();
        packet.WriteUInt16(0);
        packet.WriteBits(1_000_000, 21);
        packet.FlushBits();

        _harness.Deliver(Opcode.SMSG_GUILD_NEWS_UPDATE, packet.GetDataSpan().ToArray(), _harness.Client.HandleGuildNews);

        Assert.Empty(_harness.ClientWire.Sent);
    }

    [Fact]
    public void GuildNews_Layout()
    {
        long date = Time.GetUnixTimeFromPackedTime(PackedDate);
        var news = new GuildNews();
        var pinned = new GuildNewsEvent { Id = 1, CompletedDate = date, Type = 1, Flags = 1 };
        pinned.Data[0] = 9;
        var crafted = new GuildNewsEvent { Id = 2, CompletedDate = date, Type = 4, Item = new ItemInstance { ItemID = 55060 } };
        crafted.Data[0] = 55060;
        news.Events.AddRange([pinned, crafted]);

        Assert.Equal(Convert.FromHexString(
            Ints(2)
            + Ints(1) + "D358931A" + Ints(1, 1, 9, 0)       // id, date, kind, flags, two values
            + "0000" + Ints(0) + "00"                       // no member, nobody there, no item
            + Ints(2) + "D358931A" + Ints(4, 0, 55060, 0)
            + "0000" + Ints(0) + "80"                       // an item follows
            + Ints(55060, 0, 0) + "00" + "00"),             // the item, no bonuses, no modifiers
            Written(news));
    }

    [Theory]
    // as they went to a TrinityCore 4.3.4 server: guild 43's entry 0 pinned, guild 44's let go
    [InlineData(43u, 1, true, "00000000" + "1E00" + "F12A1E")]
    [InlineData(44u, 1, false, "00000000" + "1600" + "F12D1E")]
    [InlineData(44u, 250, true, "F9000000" + "1E00" + "F12D1E")]
    public void WriteCataNewsUpdateSticky_TakesTheShiftOffAgain(uint guildId, int newsId, bool sticky, string expected)
    {
        using var packet = new WorldPacket();

        GuildSystem.WriteCataNewsUpdateSticky(packet, new GuildNewsUpdateSticky(WowGuid128.Create(HighGuidType703.Guild, guildId), newsId, sticky));

        Assert.Equal(Convert.FromHexString(expected), packet.GetDataSpan().ToArray());
    }
}
