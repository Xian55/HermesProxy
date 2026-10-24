using System;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// SMSG_PVP_SEASON tells the client the arena seasons and whether they use teams, at every login.
/// 1.14 and 2.5 read five int32 and a bit, 3.4.3 six int32 and two bits, 4.4.2 seven int32 and
/// three bits. Pinned as literal bytes from the native captures under <c>refs/native-captures/</c>,
/// because the bugs here were order and length bugs a field-by-field check would agree with.
/// </summary>
public sealed class SeasonInfoLayoutTests
{
    [Fact]
    public void OneFlagLayout_IsFiveIdsAndTheChestBit()
    {
        var packet = new SeasonInfo { CurrentSeason = 2, PreviousSeason = 1, CurrentArenaSeasonUsesTeams = true };

        string expected =
            "00000000" + // MythicPlusSeasonID
            "02000000" + // CurrentSeason
            "01000000" + // PreviousSeason
            "00000000" + // ConquestWeeklyProgressCurrencyID
            "00000000" + // PvpSeasonID
            "00";        // WeeklyRewardChestsEnabled; no teams bit before 3.4.3

        AssertBytes(new SeasonInfo.OneFlagLayout(), packet, expected);
    }

    /// <summary>
    /// The 3.4.3 wire, 25 bytes in all five 3.4.3 captures; wintergrasp #127: two zero Mythic+ ids,
    /// arena season 32, previous 31, then ConquestWeeklyProgressCurrencyID, PvpSeasonID and the
    /// flushed bit byte.
    /// </summary>
    [Fact]
    public void TwoFlagsLayout_MatchesTheNative343Wire()
    {
        var packet = new SeasonInfo { CurrentSeason = 32, PreviousSeason = 31 };

        string expected =
            "00000000" + // MythicPlusDisplaySeasonID
            "00000000" + // MythicPlusSeasonID
            "20000000" + // CurrentSeason  = 32
            "1F000000" + // PreviousSeason = 31
            "00000000" + // ConquestWeeklyProgressCurrencyID
            "00000000" + // PvpSeasonID
            "00";        // WeeklyRewardChestsEnabled, CurrentArenaSeasonUsesTeams, flushed

        AssertBytes(new SeasonInfo.TwoFlagsLayout(), packet, expected);
    }

    /// <summary>
    /// The 4.4.2 wire, 29 bytes on TrinityCore cata_classic (tc_cata_442_20261015-213514 #145):
    /// arena season 9, previous 8, the unnamed int32 and the three flag bits.
    /// </summary>
    [Fact]
    public void ThreeFlagsLayout_MatchesTheNative442Wire()
    {
        var packet = new SeasonInfo { CurrentSeason = 9, PreviousSeason = 8 };

        string expected =
            "00000000" + // MythicPlusDisplaySeasonID
            "00000000" + // MythicPlusSeasonID
            "09000000" + // CurrentSeason  = 9
            "08000000" + // PreviousSeason = 8
            "00000000" + // ConquestWeeklyProgressCurrencyID
            "00000000" + // PvpSeasonID
            "00000000" + // unnamed
            "00";        // WeeklyRewardChestsEnabled, CurrentArenaSeasonUsesTeams, previous season's teams

        AssertBytes(new SeasonInfo.ThreeFlagsLayout(), packet, expected);
    }

    /// <summary>
    /// The client's arena UI follows the uses-teams bit, the second in the flag byte from 3.4.3 on.
    /// On 4.4.2 it used to arrive as part of the unnamed int32, so a team-based server's season
    /// read as a solo one.
    /// </summary>
    [Fact]
    public void UsesTeams_IsTheSecondBitOfTheFlagByte()
    {
        var packet = new SeasonInfo { CurrentArenaSeasonUsesTeams = true };

        Assert.EndsWith("00000000" + "40", Write(new SeasonInfo.TwoFlagsLayout(), packet));
        Assert.EndsWith("00000000" + "00000000" + "40", Write(new SeasonInfo.ThreeFlagsLayout(), packet));
        Assert.EndsWith("00000000" + "00", Write(new SeasonInfo.OneFlagLayout(), packet));
    }

    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.IsType<SeasonInfo.OneFlagLayout>(SeasonInfo.Layouts.For(ClientVersionBuild.V1_14_2_42597));
        Assert.IsType<SeasonInfo.OneFlagLayout>(SeasonInfo.Layouts.For(ClientVersionBuild.V2_5_3_41750));
        Assert.IsType<SeasonInfo.TwoFlagsLayout>(SeasonInfo.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<SeasonInfo.ThreeFlagsLayout>(SeasonInfo.Layouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    private static void AssertBytes(ServerPacketLayout<SeasonInfo> layout, SeasonInfo packet, string expected)
    {
        Assert.Equal(layout.MaxSize * 2, expected.Length);
        Assert.Equal(expected, Write(layout, packet));
        Assert.Equal(expected, WriteToSpan(layout, packet));
    }

    private static string Write(ServerPacketLayout<SeasonInfo> layout, SeasonInfo packet)
    {
        using var data = new WorldPacket(1u);
        layout.Write(packet, data);
        return Convert.ToHexString(data.GetDataSpan());
    }

    private static string WriteToSpan(ServerPacketLayout<SeasonInfo> layout, SeasonInfo packet)
    {
        Span<byte> buffer = stackalloc byte[layout.MaxSize];
        int written = layout.WriteToSpan(packet, buffer);
        return Convert.ToHexString(buffer[..written]);
    }
}
