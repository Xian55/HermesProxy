using System;
using System.Linq;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using HermesProxy.World.Server.Systems;
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

    private static byte[] FrameClientBody(byte[] body)
    {
        var framed = new byte[body.Length + 2];
        body.CopyTo(framed, 2);
        return framed;
    }

    /// Through the production accessor, exactly as the dispatch site builds it — a hand-written
    /// AsSpan(2) would agree with itself rather than with the two-byte opcode the read-mode
    /// WorldPacket constructor has already consumed.
    private static GuildSetRankPermissions ReadNativeOfficer()
    {
        var reader = new SpanPacketReader(new WorldPacket(FrameClientBody(NativeOfficer)).GetRemainingSpan());
        GuildSetRankPermissionsCodec.Read(ref reader, out var rank);
        Assert.Equal(0, reader.Remaining);
        return rank;
    }

    [Fact]
    public void Read_NativeWrathionPacket_MatchesCapturedFields()
    {
        var rank = ReadNativeOfficer();

        Assert.Equal(1u, rank.RankID);
        Assert.Equal(1u, rank.RankOrder);
        Assert.Equal(0x00DDFFBFu, rank.Flags);
        Assert.Equal(1, rank.WithdrawGoldLimit);
        Assert.Equal(7u, rank.TabFlags[0]);
        Assert.Equal(1u, rank.TabWithdrawItemLimit[0]);
        Assert.All(Enumerable.Range(1, GuildConst.MaxBankTabs - 1), i => Assert.Equal(0u, rank.TabFlags[i]));
        Assert.Equal(0x00DDFFBFu, rank.OldFlags);
        Assert.Equal("Officer", rank.RankName);
    }

    [Fact]
    public void BuildLegacyGuildRank_WritesThe335aLayout()
    {
        var rank = ReadNativeOfficer();

        byte[] expected = Convert.FromHexString(
            "01000000" + "BFFFDD00"                          // rank id, rights
            + "4F666669636572" + "00"                        // "Officer\0"
            + "01000000"                                     // gold per day
            + "07000000" + "01000000"                        // tab 0: rights, slots per day
            + new string('0', 80));                          // tabs 1-5

        using var legacy = GuildSystem.BuildLegacyGuildRank(rank);
        Assert.Equal(expected, legacy.GetData());
    }
}
