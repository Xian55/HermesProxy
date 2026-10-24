using System;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// Pins for the 3.4.3 PvP panel's packets, whose field lists appear twice (Write and WriteToSpan).
/// SMSG_PVP_SEASON is in <see cref="SeasonInfoLayoutTests"/>.
/// </summary>
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
