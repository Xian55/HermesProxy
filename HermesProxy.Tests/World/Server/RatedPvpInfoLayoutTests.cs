using System;
using System.Buffers;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// SMSG_RATED_PVP_INFO is a fixed run of bracket records: seven for the 3.4.3 client, nine for the
/// 4.4.2 one (TrinityCore cata_classic <c>Bracket[9]</c>, docs/protocol/4.4.2.60895). The proxy sent
/// the 4.4.2 client seven, and it read the last two past the end of the packet.
/// </summary>
public sealed class RatedPvpInfoLayoutTests
{
    private const int RecordSize = 19 * 4 + 1;

    private static RatedPvpInfo WithATeam() => new()
    {
        Brackets =
        [
            new RatedBracketInfo { PersonalRating = 1510, WeeklyPlayed = 4, WeeklyWon = 3, SeasonPlayed = 12, SeasonWon = 7 },
            new RatedBracketInfo(),
        ],
    };

    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.Equal(7, Assert.IsType<RatedPvpInfo.BracketsLayout>(RatedPvpInfo.Layouts.For(ClientVersionBuild.V3_4_3_54261)).BracketCount);
        Assert.Equal(9, Assert.IsType<RatedPvpInfo.BracketsLayout>(RatedPvpInfo.Layouts.For(ClientVersionBuild.V4_4_2_60895)).BracketCount);
    }

    [Fact]
    public void SevenBrackets_MatchTheOldWriter()
    {
        var p = WithATeam();
        using var old = new WorldPacket(1u);
        for (int i = 0; i < 7; i++)
            OldBracket(old, i < p.Brackets.Length ? p.Brackets[i] : null);

        AssertBothSerialisers(RatedPvpInfo.Layouts.For(ClientVersionBuild.V3_4_3_54261), p, old, 7 * RecordSize);
    }

    [Fact]
    public void NineBrackets_AreTheSevenAndTwoMoreOfNothing()
    {
        var p = WithATeam();
        using var expected = new WorldPacket(1u);
        for (int i = 0; i < 9; i++)
            OldBracket(expected, i < p.Brackets.Length ? p.Brackets[i] : null);

        AssertBothSerialisers(RatedPvpInfo.Layouts.For(ClientVersionBuild.V4_4_2_60895), p, expected, 9 * RecordSize);
    }

    // The writer before the 4.4.2 layout, one record
    private static void OldBracket(WorldPacket data, RatedBracketInfo? b)
    {
        data.WriteInt32((int)(b?.PersonalRating ?? 0));
        data.WriteInt32(0);
        data.WriteInt32((int)(b?.SeasonPlayed ?? 0));
        data.WriteInt32((int)(b?.SeasonWon ?? 0));
        data.WriteInt32(0);
        data.WriteInt32(0);
        data.WriteInt32((int)(b?.WeeklyPlayed ?? 0));
        data.WriteInt32((int)(b?.WeeklyWon ?? 0));
        for (int n = 0; n < 11; n++)
            data.WriteInt32(0);
        data.WriteBit(false);
        data.FlushBits();
    }

    private static void AssertBothSerialisers(ServerPacketLayout<RatedPvpInfo> layout, RatedPvpInfo packet,
        WorldPacket expectedPacket, int size)
    {
        byte[] expected = expectedPacket.GetDataSpan().ToArray();
        Assert.Equal(size, expected.Length);
        Assert.Equal(size, layout.MaxSize);

        using var data = new WorldPacket(1u);
        layout.Write(packet, data);
        Assert.Equal(expected, data.GetDataSpan().ToArray());

        byte[] buffer = ArrayPool<byte>.Shared.Rent(layout.MaxSize);
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
