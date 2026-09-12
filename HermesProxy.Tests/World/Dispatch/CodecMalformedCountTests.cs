using System;
using System.Linq;
using HermesProxy.Enums;
using Framework.IO;
using HermesProxy;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Codecs that size a collection from a wire-supplied count must not allocate on that count.
/// </summary>
/// <remarks>
/// The old <c>ClientPacket</c> readers grew their lists from empty, so a corrupt count ran out of
/// buffer and threw after a few elements. Pre-sizing on the count is the optimisation that removed
/// that accident: <c>new List&lt;T&gt;(0x7FFFFFFF)</c> reserves gigabytes <i>before</i> the first
/// element read can fail. These tests pin the bound, and are the reason the codecs clamp or guard.
///
/// Two different remedies, because the collections mean different things. A <c>List&lt;T&gt;</c>
/// capacity is a hint — clamping it changes no observable result, since the list still grows to
/// hold whatever is genuinely on the wire. An array <i>is</i> the packet field, so clamping one
/// would silently drop elements; those guard and throw instead.
///
/// <c>SpanPacketReader</c> is a ref struct and cannot be captured, so the failing cases use an
/// explicit try/catch rather than <c>Assert.Throws</c> with a lambda.
/// </remarks>
public class CodecMalformedCountTests
{
    static CodecMalformedCountTests()
    {
        if (VersionBootstrap.ModernBuild == ClientVersionBuild.Zero)
            VersionBootstrap.ModernBuild = ClientVersionBuild.V1_14_2_42597;
        if (VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            VersionBootstrap.LegacyBuild = ClientVersionBuild.V3_3_5a_12340;
    }

    private static SpanPacketReader ReaderOver(byte[] payload)
    {
        byte[] framed = new byte[payload.Length + 2];
        payload.CopyTo(framed, 2);
        return new SpanPacketReader(new WorldPacket(framed).GetRemainingSpan());
    }

    /// <summary>
    /// A count with nothing behind it. Naively <c>0x7FFFFFFF</c> guids is a 16 GB reserve; the
    /// clamp means the throw comes from the missing guids instead.
    /// </summary>
    [Theory]
    [InlineData(0x7FFFFFFFu)]
    [InlineData(0xFFFFFFFFu)]
    [InlineData(0x01000000u)]
    public void QueryPlayerNames_AbsurdCount_ThrowsOnUnderflowWithoutReserving(uint count)
    {
        byte[] payload = BitConverter.GetBytes(count);

        Exception? caught = null;
        try
        {
            var r = ReaderOver(payload);
            QueryPlayerNamesCodec.Read(ref r, out _);
        }
        catch (Exception e)
        {
            caught = e;
        }

        Assert.NotNull(caught);
        Assert.IsNotType<OutOfMemoryException>(caught);
    }

    /// <summary>A well-formed list still round-trips, so the clamp never costs a real packet.</summary>
    [Fact]
    public void QueryPlayerNames_WellFormed_ReadsEveryGuid()
    {
        var guids = new[]
        {
            new WowGuid128(0xDEADBEEFUL, 0x1UL),
            new WowGuid128(0xCAFEUL, 0x2UL),
            new WowGuid128(0x0UL, 0x0UL),
        };

        using var w = new WorldPacket(1u);
        w.WriteUInt32((uint)guids.Length);
        foreach (var g in guids)
            w.WritePackedGuid128(g);

        var r = ReaderOver(w.GetData());
        QueryPlayerNamesCodec.Read(ref r, out var packet);

        Assert.Equal(guids, packet.Players.ToArray());
        Assert.Equal(0, r.Remaining);
    }

    /// <summary>The array case guards instead of clamping, so the count is rejected outright.</summary>
    [Theory]
    [InlineData(0x7FFFFFFF)]
    [InlineData(0x01000000)]
    [InlineData(-1)]
    public void QuestPOIQuery_AbsurdCount_ThrowsArgumentOutOfRange(int count)
    {
        byte[] payload = BitConverter.GetBytes(count);

        ArgumentOutOfRangeException? caught = null;
        try
        {
            var r = ReaderOver(payload);
            QuestPOIQueryCodec.Read(ref r, out _);
        }
        catch (ArgumentOutOfRangeException e)
        {
            caught = e;
        }

        Assert.NotNull(caught);
        Assert.Contains("quest ids", caught.Message);
    }

    /// <summary>A count the buffer can actually satisfy is untouched by the guard.</summary>
    [Fact]
    public void QuestPOIQuery_WellFormed_ReadsEveryId()
    {
        int[] ids = { 1, 62, 0, int.MaxValue };

        using var w = new WorldPacket(1u);
        w.WriteInt32(ids.Length);
        foreach (int id in ids)
            w.WriteInt32(id);

        var r = ReaderOver(w.GetData());
        QuestPOIQueryCodec.Read(ref r, out var packet);

        Assert.Equal(ids, packet.MissingQuestPOIs);
        Assert.Equal(0, r.Remaining);
    }

    /// <summary>A count one past what the buffer holds is the boundary the guard defends.</summary>
    [Fact]
    public void QuestPOIQuery_CountOneTooLarge_Throws()
    {
        using var w = new WorldPacket(1u);
        w.WriteInt32(3);
        w.WriteInt32(10);
        w.WriteInt32(20);   // only two ids behind a count of three

        ArgumentOutOfRangeException? caught = null;
        try
        {
            var r = ReaderOver(w.GetData());
            QuestPOIQueryCodec.Read(ref r, out _);
        }
        catch (ArgumentOutOfRangeException e)
        {
            caught = e;
        }

        Assert.NotNull(caught);
    }
}
