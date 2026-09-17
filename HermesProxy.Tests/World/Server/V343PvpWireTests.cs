using System;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// Byte-for-byte pin for SMSG_PVP_SEASON, whose V3_4_3 layout differs from the shared one, taken
/// from the native 3.4.3 captures in <c>refs/native-captures/</c>.
/// </summary>
/// <remarks>
/// Worth pinning as literal bytes rather than field-by-field because the bug it guards was an
/// ordering and length bug, which a field-by-field assertion cannot see: the writer and the
/// assertion would simply agree on the wrong order. The packet is also written twice — once in
/// <c>Write()</c> and once in <c>WriteToSpan</c> — so the field list appears twice and only the
/// bytes tie them together.
/// <para>
/// <c>ModernVersion.Build</c> is fixed for the test process, so the V3_4_3 arm is reached via
/// <c>PvpWire.ForceV343ForTests</c>, the same escape hatch <c>LootRollWire</c> uses.
/// </para>
/// </remarks>
[Collection("V343ValuesFilter")]
public class V343PvpWireTests
{
    private static byte[] SpanBytes(ServerPacket packet)
    {
        var spanWritable = (ISpanWritable)packet;
        var buffer = new byte[spanWritable.MaxSize];
        int written = spanWritable.WriteToSpan(buffer);
        Assert.True(written >= 0, "WriteToSpan reported MaxSize exceeded.");
        return buffer[..written];
    }

    /// <summary>
    /// SMSG_PVP_SEASON (0x25C1), 25 bytes in all five captures. Taken from the wintergrasp
    /// capture, packet #127: two zero Mythic+ ids, arena season 32, previous 31, then
    /// ConquestWeeklyProgressCurrencyID, PvpSeasonID and the flushed bit byte.
    /// </summary>
    /// <remarks>
    /// The bug this pins: the packet used to be written in the later five-int32 order, so it went
    /// out four bytes short and the two arena seasons landed one slot early — the client read the
    /// current season as a Mythic+ id and the previous season as the current one, then ran off the
    /// end of the packet for the last two fields.
    /// </remarks>
    [Fact]
    public void SeasonInfo_MatchesTheNativeWire()
    {
        PvpWire.ForceV343ForTests = true;
        try
        {
            var packet = new SeasonInfo
            {
                CurrentSeason = 32,
                PreviousSeason = 31,
            };

            byte[] expected = Convert.FromHexString(
                "00000000" + // MythicPlusDisplaySeasonID
                "00000000" + // MythicPlusSeasonID
                "20000000" + // CurrentSeason  = 32
                "1F000000" + // PreviousSeason = 31
                "00000000" + // ConquestWeeklyProgressCurrencyID
                "00000000" + // PvpSeasonID
                "00");       // WeeklyRewardChestsEnabled, flushed

            Assert.Equal(25, expected.Length);
            Assert.Equal(Convert.ToHexString(expected), Convert.ToHexString(SpanBytes(packet)));
        }
        finally
        {
            PvpWire.ForceV343ForTests = null;
        }
    }

    /// <summary>
    /// SMSG_RATED_PVP_INFO is a fixed seven bracket records of nineteen int32 plus a flushed bit,
    /// so 7 x 77 = 539 bytes with no counts and no length prefix anywhere.
    /// </summary>
    /// <remarks>
    /// Pinned as a length because that is the failure mode: the field list appears twice (Write
    /// and WriteToSpan) and once more as a loop of eleven zeroes in the span body, so dropping or
    /// doubling one int32 is easy and produces a packet the client silently discards. There is no
    /// native capture to compare bytes against — Wrathion answers this opcode but no capture in
    /// refs/native-captures covers the PvP panel — so the arithmetic is what is checked.
    /// </remarks>
    [Fact]
    public void RatedPvpInfo_IsSevenFixedBracketRecords()
    {
        var packet = new RatedPvpInfo
        {
            Brackets = [new RatedBracketInfo { PersonalRating = 1842, SeasonPlayed = 9, SeasonWon = 5 }],
        };

        byte[] actual = SpanBytes(packet);

        Assert.Equal(7 * (19 * 4 + 1), actual.Length);
        Assert.Equal(539, actual.Length);

        // First bracket leads with PersonalRating; the rest of the array is absent and writes zero.
        Assert.Equal(1842, BitConverter.ToInt32(actual, 0));
        Assert.Equal(0, BitConverter.ToInt32(actual, 77));
    }
}
