using System;
using System.Linq;
using HermesProxy.World;
using HermesProxy.World.Server;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

public class GuildRankPermissionsTests
{
    // CMSG_GUILD_SET_RANK_PERMISSIONS for the Officer rank, verbatim from a native Wrathion 3.4.3
    // capture (the fourth of five sent by one Apply). OldFlags precedes the 7-bit name here, which
    // is the reverse of TrinityCore wotlk_classic's reader.
    private static readonly byte[] NativeOfficer = Convert.FromHexString(
        "01000000" + "01000000" + "BFFFDD00" + "01000000"   // RankID, RankOrder, Flags, WithdrawGoldLimit
        + "07000000" + "01000000"                            // tab 0: flags, withdraw item limit
        + new string('0', 80)                                // tabs 1-5
        + "BFFFDD00"                                         // OldFlags
        + "0E" + "4F666669636572");                          // name length 7 in the top 7 bits, "Officer"

    private static WorldPacket FrameClientBody(byte[] body)
    {
        var framed = new byte[body.Length + 2];
        body.CopyTo(framed, 2);
        return new WorldPacket(framed);
    }

    [Fact]
    public void Read_NativeWrathionPacket_MatchesCapturedFields()
    {
        using var rank = new GuildSetRankPermissions(FrameClientBody(NativeOfficer));
        rank.Read();

        Assert.Equal(1u, rank.RankID);
        Assert.Equal(1u, rank.RankOrder);
        Assert.Equal(0x00DDFFBFu, rank.Flags);
        Assert.Equal(1, rank.WithdrawGoldLimit);
        Assert.Equal(7u, rank.TabFlags[0]);
        Assert.Equal(1u, rank.TabWithdrawItemLimit[0]);
        Assert.All(rank.TabFlags.Skip(1), flags => Assert.Equal(0u, flags));
        Assert.Equal(0x00DDFFBFu, rank.OldFlags);
        Assert.Equal("Officer", rank.RankName);
    }

    [Fact]
    public void BuildLegacyGuildRank_WritesThe335aLayout()
    {
        using var rank = new GuildSetRankPermissions(FrameClientBody(NativeOfficer));
        rank.Read();

        byte[] expected = Convert.FromHexString(
            "01000000" + "BFFFDD00"                          // rank id, rights
            + "4F666669636572" + "00"                        // "Officer\0"
            + "01000000"                                     // gold per day
            + "07000000" + "01000000"                        // tab 0: rights, slots per day
            + new string('0', 80));                          // tabs 1-5

        using var legacy = WorldSocket.BuildLegacyGuildRank(rank);
        Assert.Equal(expected, legacy.GetData());
    }
}
