using System;
using System.Reflection;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// Pins <see cref="InspectHonorStatsResultWotLKClassic"/> to a native 3.4.3 capture.
/// </summary>
/// <remarks>
/// A 3.4.3 client answered with the TBC layout (30 payload bytes where it expects 42) read past the
/// end of the packet and allocated on a garbage length until it froze at ~10 GB. Nothing short of
/// the client catches that, so the layout is fixed here against the bytes a native server actually
/// sent: <c>World_SMSG_INSPECT_HONOR_STATS_SMSG_INSPECT_RESULT.pkt</c>, captured from Wrathion on
/// 2026-09-13, inspecting a character with one honourable kill.
/// <para>
/// Every check runs against both bodies. <c>WritePacketData</c> ships <c>WriteToSpan</c>'s bytes and
/// only falls back to <c>Write()</c> on overflow, so testing through it alone would leave
/// <c>Write()</c> unchecked; <see cref="SpanWriteParityTests"/> does compare the two, but only at
/// default values, where a swapped counter still reads as all zeros.
/// </para>
/// <para>
/// Worth knowing if this ever needs re-deriving: WowPacketParser mis-parses this packet on 3.4.3 —
/// it reports <c>YesterdayHK: 1</c> and leaves 33 of the 47 bytes unread — so the field order below
/// comes from the 3.4.3 server's <c>InspectHonorStatsResult::Write</c>, not from a WPP dump.
/// </para>
/// </remarks>
public class InspectHonorStatsPacketTests
{
    private static readonly FieldInfo WorldPacketField =
        typeof(ServerPacket).GetField("_worldPacket", BindingFlags.NonPublic | BindingFlags.Instance)!;

    // Player/0 R1/S0 Map: 0 Low: 82 -> packs to 01 A0 52 04 08.
    private static readonly WowGuid128 CapturedTarget = new(Low: 0x52, High: 0x0800040000000000);

    private static readonly byte[] CapturedPayload =
    [
        0x01, 0xA0, 0x52, 0x04, 0x08,                   // packed GUID128
        0x00,                                           // LifetimeMaxRank
        0x01, 0x00,                                     // TodayHK = 1
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,             // TodayDK, YesterdayHK, YesterdayDK
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // LastWeekHK/DK, ThisWeekHK/DK
        0x01, 0x00, 0x00, 0x00,                         // LifeTimeHK = 1
        0x00, 0x00, 0x00, 0x00,                         // LifeTimeDK
        0x00, 0x00, 0x00, 0x00,                         // YesterdayHonor
        0x00, 0x00, 0x00, 0x00,                         // LastWeekHonor
        0x00, 0x00, 0x00, 0x00,                         // ThisWeekHonor
        0x00, 0x00, 0x00, 0x00,                         // Standing
        0x00,                                           // RankProgress
    ];

    public enum Body { Span, Write }

    /// <summary>Serialises through exactly one body, so each can fail on its own.</summary>
    /// <remarks>
    /// Returns a view rather than a copy. Both backing arrays are heap-allocated and owned by this
    /// call, so the span stays valid for the caller.
    /// </remarks>
    private static ReadOnlySpan<byte> Serialise(InspectHonorStatsResultWotLKClassic packet, Body body)
    {
        if (body == Body.Write)
        {
            packet.Write();
            return ((WorldPacket)WorldPacketField.GetValue(packet)!).GetData();
        }

        // Exactly MaxSize rather than a pool rental, so an undercounted MaxSize throws here.
        byte[] buffer = new byte[packet.MaxSize];
        int written = packet.WriteToSpan(buffer);
        Assert.True(written >= 0, "a fixed-layout packet must never report overflow");
        return buffer.AsSpan(0, written);
    }

    [Theory]
    [InlineData(Body.Span)]
    [InlineData(Body.Write)]
    public void MatchesNativeCaptureByteForByte(Body body)
    {
        var packet = new InspectHonorStatsResultWotLKClassic
        {
            PlayerGUID = CapturedTarget,
            TodayHK = 1,
            LifeTimeHK = 1,
        };

        Assert.Equal(CapturedPayload, Serialise(packet, body));
    }

    /// <summary>
    /// What production actually sends. Separate from the two bodies above because this is the only
    /// path that picks between them.
    /// </summary>
    [Fact]
    public void WritePacketData_MatchesNativeCapture()
    {
        var packet = new InspectHonorStatsResultWotLKClassic
        {
            PlayerGUID = CapturedTarget,
            TodayHK = 1,
            LifeTimeHK = 1,
        };

        packet.WritePacketData();

        Assert.Equal(CapturedPayload, packet.GetData());
    }

    /// <summary>
    /// The payload after the GUID is 42 bytes regardless of values. A layout that is short by even
    /// one field is exactly what made the client allocate on garbage.
    /// </summary>
    [Theory]
    [InlineData(Body.Span)]
    [InlineData(Body.Write)]
    public void PayloadAfterGuidIsFortyTwoBytes(Body body)
    {
        var packet = new InspectHonorStatsResultWotLKClassic { PlayerGUID = CapturedTarget };

        const int packedGuidLength = 5;
        Assert.Equal(packedGuidLength + 42, Serialise(packet, body).Length);
    }

    /// <summary>
    /// Each counter lands in its own slot. Every field gets a distinct value, so a swapped or shifted
    /// field cannot pass in either body.
    /// </summary>
    [Theory]
    [InlineData(Body.Span)]
    [InlineData(Body.Write)]
    public void PutsEachFieldAtItsOffset(Body body)
    {
        var packet = new InspectHonorStatsResultWotLKClassic
        {
            PlayerGUID = CapturedTarget,
            LifetimeMaxRank = 0x0E,
            TodayHK = 0x0102,
            TodayDK = 0x1112,
            YesterdayHK = 0x0304,
            YesterdayDK = 0x1314,
            LastWeekHK = 0x2122,
            LastWeekDK = 0x2324,
            ThisWeekHK = 0x3132,
            ThisWeekDK = 0x3334,
            LifeTimeHK = 0x05060708,
            LifeTimeDK = 0x15161718,
            YesterdayHonor = 0x090A0B0C,
            LastWeekHonor = 0x191A1B1C,
            ThisWeekHonor = 0x0D0E0F10,
            Standing = 0x1D1E1F20,
            RankProgress = 0x7F,
        };

        ReadOnlySpan<byte> data = Serialise(packet, body);

        Assert.Equal(0x0E, data[5]);                                         // LifetimeMaxRank
        Assert.Equal<byte>([0x02, 0x01], data[6..8]);                        // TodayHK
        Assert.Equal<byte>([0x12, 0x11], data[8..10]);                       // TodayDK
        Assert.Equal<byte>([0x04, 0x03], data[10..12]);                      // YesterdayHK
        Assert.Equal<byte>([0x14, 0x13], data[12..14]);                      // YesterdayDK
        Assert.Equal<byte>([0x22, 0x21], data[14..16]);                      // LastWeekHK
        Assert.Equal<byte>([0x24, 0x23], data[16..18]);                      // LastWeekDK
        Assert.Equal<byte>([0x32, 0x31], data[18..20]);                      // ThisWeekHK
        Assert.Equal<byte>([0x34, 0x33], data[20..22]);                      // ThisWeekDK
        Assert.Equal<byte>([0x08, 0x07, 0x06, 0x05], data[22..26]);          // LifeTimeHK
        Assert.Equal<byte>([0x18, 0x17, 0x16, 0x15], data[26..30]);          // LifeTimeDK
        Assert.Equal<byte>([0x0C, 0x0B, 0x0A, 0x09], data[30..34]);          // YesterdayHonor
        Assert.Equal<byte>([0x1C, 0x1B, 0x1A, 0x19], data[34..38]);          // LastWeekHonor
        Assert.Equal<byte>([0x10, 0x0F, 0x0E, 0x0D], data[38..42]);          // ThisWeekHonor
        Assert.Equal<byte>([0x20, 0x1F, 0x1E, 0x1D], data[42..46]);          // Standing
        Assert.Equal(0x7F, data[46]);                                        // RankProgress
    }
}
