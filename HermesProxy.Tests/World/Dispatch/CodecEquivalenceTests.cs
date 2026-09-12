using System;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Server.Packets;
using Xunit;
using Frozen = HermesProxy.Tests.World.Dispatch.Reference.FrozenClientPackets;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Proves each converted codec reads exactly what the <c>ClientPacket.Read()</c> it replaced read.
/// </summary>
/// <remarks>
/// <para>
/// This is the gate that matters for the dispatch migration. A codec that reads the right fields
/// in the wrong order, or stops one field early, still compiles and still "works" until a value
/// happens to be non-zero — and the failure surfaces as the client dropping or mis-rendering a
/// packet, with nothing thrown and nothing logged. Comparing both readers over the same bytes is
/// the only cheap way to know.
/// </para>
/// <para>
/// Each case asserts field values <b>and</b> final position. Position is what catches the codec
/// that happens to agree on every field of this fixture but consumed a different number of bytes —
/// which in a real packet stream desynchronises everything after it.
/// </para>
/// </remarks>
public class CodecEquivalenceTests
{
    /// Frames a payload the way the wire does: WorldPacket's read-mode ctor eats a 2-byte opcode
    /// prefix that GetData() does not produce, so it has to be prepended.
    private static (WorldPacket Oracle, byte[] Framed) Build(Action<WorldPacket> write)
    {
        using var w = new WorldPacket(1u);
        write(w);
        byte[] payload = w.GetData();
        byte[] framed = new byte[payload.Length + 2];
        payload.CopyTo(framed, 2);
        return (new WorldPacket(framed), framed);
    }

    /// <summary>
    /// Positions the reader exactly as production does — via the same accessor the dispatch site
    /// calls, not a hand-written offset.
    /// </summary>
    /// <remarks>
    /// This used to be <c>framed.AsSpan(2)</c>, hardcoding the opcode-prefix width. That made the
    /// test agree with itself rather than with production: the dispatch site was handing the
    /// reader <c>GetDataSpan()</c>, which starts at index 0 and therefore included the two opcode
    /// bytes the WorldPacket constructor had already consumed. Every converted packet parsed two
    /// bytes early in the proxy while every test passed. Going through the real accessor is what
    /// makes an offset bug expressible here at all.
    /// </remarks>
    private static SpanPacketReader ReaderOver(byte[] framed)
        => new(new WorldPacket(framed).GetRemainingSpan());

    [Theory]
    [InlineData(0UL, 0UL)]
    [InlineData(1UL, 0UL)]
    [InlineData(0xDEADBEEFUL, 0x0123456789ABCDEFUL)]
    [InlineData(ulong.MaxValue, ulong.MaxValue)]
    public void AttackSwing_MatchesFrozenReader(ulong low, ulong high)
    {
        var guid = new WowGuid128(low, high);
        var (oracle, framed) = Build(w => w.WritePackedGuid128(guid));

        var expected = new Frozen.AttackSwing();
        expected.Read(oracle);

        var r = ReaderOver(framed);
        AttackSwingCodec.Read(ref r, out var actual);

        Assert.Equal(expected.Victim, actual.Victim);
        Assert.Equal(oracle.Remaining(), r.Remaining);
    }

    [Fact]
    public void AttackStop_ConsumesNothing_LikeTheFrozenReader()
    {
        // An empty payload is still worth pinning: a codec that read even one byte here would
        // desync every packet after it on the same buffer.
        var (oracle, framed) = Build(w => w.WriteUInt32(0xCAFEBABE));

        var expected = new Frozen.AttackStop();
        expected.Read(oracle);

        var r = ReaderOver(framed);
        AttackStopCodec.Read(ref r, out _);

        Assert.Equal(oracle.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(int.MinValue, true)]
    [InlineData(int.MaxValue, false)]
    public void SetSheathed_MatchesFrozenReader(int sheathState, bool animate)
    {
        var (oracle, framed) = Build(w =>
        {
            w.WriteInt32(sheathState);
            w.WriteBit(animate);
            w.FlushBits();
        });

        var expected = new Frozen.SetSheathed();
        expected.Read(oracle);

        var r = ReaderOver(framed);
        SetSheathedCodec.Read(ref r, out var actual);

        Assert.Equal(expected.SheathState, actual.SheathState);
        Assert.Equal(expected.Animate, actual.Animate);
    }

    [Fact]
    public void SetSheathed_DefaultOfTheOldClass_IsNotSilentlyLost()
    {
        // The class this replaced initialised Animate to true, which a positional record struct
        // cannot express. That is only safe because the codec assigns it on every path; if a
        // future edit ever made the bit conditional, the struct would yield false where the class
        // yielded true. Pin the initializer so that change cannot pass unnoticed.
        Assert.True(new Frozen.SetSheathed().Animate);
    }

    [Theory]
    [InlineData(0UL, 0u)]
    [InlineData(0xDEADBEEFUL, 3u)]
    [InlineData(ulong.MaxValue, uint.MaxValue)]
    public void BuyBackItem_MatchesFrozenReader(ulong vendorLow, uint slot)
    {
        var vendor = new WowGuid128(vendorLow, 0x1122334455667788UL);
        var (oracle, framed) = Build(w =>
        {
            w.WritePackedGuid128(vendor);
            w.WriteUInt32(slot);
        });

        var expected = new Frozen.BuyBackItem();
        expected.Read(oracle);

        var r = ReaderOver(framed);
        BuyBackItemCodec.Read(ref r, out var actual);

        Assert.Equal(expected.VendorGUID, actual.VendorGUID);
        Assert.Equal(expected.Slot, actual.Slot);
        Assert.Equal(oracle.Remaining(), r.Remaining);
    }

    [Fact]
    public void GeneratedDispatch_StartsReadingWhereTheWorldPacketLeftOff()
    {
        // The bug this pins shipped once and was invisible to every other test here.
        //
        // A read-mode WorldPacket's constructor consumes the 2-byte opcode, so its read position
        // is already past it. GetDataSpan() ignores that position and returns the buffer from
        // index 0 — so handing it to a fresh SpanPacketReader replays the opcode as payload and
        // shifts every field by two bytes. Nothing throws; the packet just parses wrong. In the
        // proxy that meant CMSG_ATTACK_SWING decoded an empty victim GUID, the attack system took
        // its "already attacking this target" early return, and auto-attack silently stopped
        // working while all 1905 tests stayed green.
        //
        // The equivalence tests could not catch it because they built their reader with a
        // hand-written AsSpan(2) rather than the accessor the dispatch site uses, so they agreed
        // with themselves instead of with production. This asserts the actual property: the two
        // accessors differ, and only one of them continues from the read position.
        var (packet, framed) = Build(w => w.WriteUInt32(0xAABBCCDD));

        Assert.Equal(framed.Length, packet.GetDataSpan().Length);
        Assert.Equal(framed.Length - 2, packet.GetRemainingSpan().Length);

        var correct = new SpanPacketReader(packet.GetRemainingSpan());
        Assert.Equal(0xAABBCCDDu, correct.ReadUInt32());

        // Spelled out so the failure mode is legible rather than folded into a length check.
        var shifted = new SpanPacketReader(packet.GetDataSpan());
        Assert.NotEqual(0xAABBCCDDu, shifted.ReadUInt32());
    }

    [Fact]
    public void ConvertedPacketsAreDataOnly()
    {
        // The design rule, asserted rather than assumed: packets carry no behaviour. A Read or
        // Write creeping back onto one is how the split quietly erodes.
        foreach (Type t in new[]
                 {
                     typeof(AttackSwing), typeof(AttackStop),
                     typeof(SetSheathed), typeof(BuyBackItem),
                 })
        {
            Assert.True(t.IsValueType, $"{t.Name} must be a struct");

            var declared = t.GetMethods(System.Reflection.BindingFlags.Public |
                                        System.Reflection.BindingFlags.Instance |
                                        System.Reflection.BindingFlags.DeclaredOnly);

            foreach (var m in declared)
            {
                Assert.False(m.Name is "Read" or "Write",
                    $"{t.Name}.{m.Name} — packets are data; parsing belongs in the codec");
            }
        }
    }
}
