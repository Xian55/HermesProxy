using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// 4.4.2 changed the roster's member and the rank's bank tabs, not the packets around them
/// (docs/protocol, read from the client, and TrinityCore cata_classic GuildPackets.cpp). The
/// earlier layouts keep the bytes they had.
/// </summary>
public sealed class GuildRosterLayoutTests
{
    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.IsType<GuildRoster.EarlierLayout>(GuildRoster.Layouts.For(ClientVersionBuild.V2_5_3_41750));
        Assert.IsType<GuildRoster.EarlierLayout>(GuildRoster.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<GuildRoster.CataClassicLayout>(GuildRoster.Layouts.For(ClientVersionBuild.V4_4_2_60895));

        Assert.IsType<GuildRanks.EarlierLayout>(GuildRanks.Layouts.For(ClientVersionBuild.V2_5_3_41750));
        Assert.IsType<GuildRanks.EarlierLayout>(GuildRanks.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<GuildRanks.CataClassicLayout>(GuildRanks.Layouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    [Fact]
    public void Roster_EarlierLayout_MatchesTheOldWriter()
    {
        GuildRoster packet = Roster();

        using var old = new WorldPacket(1u);
        old.WriteUInt32(packet.NumAccounts);
        old.WritePackedTime(packet.CreateDate);
        old.WriteInt32(packet.GuildFlags);
        old.WriteInt32(packet.MemberData.Count);
        old.WriteBits(packet.WelcomeText.Length, 11);
        old.WriteBits(packet.InfoText.Length, 11);
        old.FlushBits();
        packet.MemberData.ForEach(p => p.Write(old));
        old.WriteString(packet.WelcomeText);
        old.WriteString(packet.InfoText);

        using var data = new WorldPacket(1u);
        new GuildRoster.EarlierLayout().Write(packet, data);
        Assert.Equal(old.GetDataSpan().ToArray(), data.GetDataSpan().ToArray());
    }

    [Fact]
    public void Roster_CataClassicLayout_WritesTheMemberTheClientReads()
    {
        GuildRoster packet = Roster();
        GuildRosterMemberData member = packet.MemberData[0];

        using var expected = new WorldPacket(1u);
        expected.WriteUInt32(packet.NumAccounts);
        expected.WritePackedTime(packet.CreateDate);
        expected.WriteInt32(packet.GuildFlags);
        expected.WriteInt32(1);
        expected.WriteBits(packet.WelcomeText.Length, 11);
        expected.WriteBits(packet.InfoText.Length, 11);
        expected.FlushBits();

        expected.WritePackedGuid128(member.Guid);
        expected.WriteInt32(member.RankID);
        expected.WriteInt32(member.AreaID);
        expected.WriteInt32(member.PersonalAchievementPoints);
        expected.WriteInt32(member.GuildReputation);
        expected.WriteFloat(member.LastSave);
        for (int i = 0; i < 2; i++)
        {
            expected.WriteInt32(member.Profession[i].DbID);
            expected.WriteInt32(member.Profession[i].Rank);
            expected.WriteInt32(member.Profession[i].Step);
        }
        expected.WriteUInt32(member.VirtualRealmAddress);
        expected.WriteUInt8(member.Status);
        expected.WriteUInt8(member.Level);
        expected.WriteUInt8((byte)member.ClassID);
        expected.WriteUInt8((byte)member.SexID);
        expected.WriteUInt64(member.GuildClubMemberID);
        expected.WriteUInt8((byte)member.RaceID);
        expected.WriteBits(member.Name.Length, 6);
        expected.WriteBits(member.Note.Length, 8);
        expected.WriteBits(member.OfficerNote.Length, 8);
        expected.WriteBit(true);                    // Authenticated, and no SorEligible after it
        expected.FlushBits();
        expected.WriteFloat(0);                     // the dungeon score summary, empty
        expected.WriteFloat(0);
        expected.WriteUInt32(0);
        expected.WriteString(member.Name);
        expected.WriteString(member.Note);
        expected.WriteString(member.OfficerNote);

        expected.WriteString(packet.WelcomeText);
        expected.WriteString(packet.InfoText);

        using var data = new WorldPacket(1u);
        new GuildRoster.CataClassicLayout().Write(packet, data);
        Assert.Equal(expected.GetDataSpan().ToArray(), data.GetDataSpan().ToArray());
    }

    [Theory]
    [InlineData(false, GuildConst.MaxBankTabs)]
    [InlineData(true, GuildConst.MaxBankTabsCata)]
    public void Ranks_WriteTheBankTabsOfTheirClient(bool cataClassic, int bankTabs)
    {
        var rank = new GuildRankData { RankID = 1, RankOrder = 1, Flags = 0x43, WithdrawGoldLimit = 500, RankName = "Officer" };
        for (int tab = 0; tab < GuildConst.MaxBankTabsCata; tab++)
        {
            rank.TabFlags[tab] = (uint)(0x10 + tab);
            rank.TabWithdrawItemLimit[tab] = (uint)(0x20 + tab);
        }
        var packet = new GuildRanks();
        packet.Ranks.Add(rank);

        using var expected = new WorldPacket(1u);
        expected.WriteInt32(1);
        expected.WriteUInt8(1);
        expected.WriteUInt32(1);
        expected.WriteUInt32(0x43);
        expected.WriteInt32(500);
        for (int tab = 0; tab < bankTabs; tab++)
        {
            expected.WriteUInt32((uint)(0x10 + tab));
            expected.WriteUInt32((uint)(0x20 + tab));
        }
        expected.WriteBits("Officer".Length, 7);
        expected.FlushBits();
        expected.WriteString("Officer");

        ServerPacketLayout<GuildRanks> layout = cataClassic ? new GuildRanks.CataClassicLayout() : new GuildRanks.EarlierLayout();
        using var data = new WorldPacket(1u);
        layout.Write(packet, data);
        Assert.Equal(expected.GetDataSpan().ToArray(), data.GetDataSpan().ToArray());
    }

    private static GuildRoster Roster()
    {
        var member = new GuildRosterMemberData
        {
            Guid = WowGuid128.Create(HighGuidType703.Player, 7),
            RankID = 2,
            AreaID = 1519,
            PersonalAchievementPoints = 120,
            GuildReputation = 3,
            LastSave = 0.25f,
            VirtualRealmAddress = 0x01020003,
            Status = 1,
            Level = 23,
            ClassID = Class.Mage,
            SexID = Gender.Female,
            RaceID = Race.Human,
            Authenticated = true,
            Name = "Elmagi",
            Note = "alt",
            OfficerNote = "trial",
        };
        member.Profession[0] = new GuildRosterProfessionData { DbID = 171, Rank = 75, Step = 1 };

        var packet = new GuildRoster { NumAccounts = 2, CreateDate = 1700000000, WelcomeText = "hello", InfoText = "raids on Tuesday" };
        packet.MemberData.Add(member);
        return packet;
    }
}
