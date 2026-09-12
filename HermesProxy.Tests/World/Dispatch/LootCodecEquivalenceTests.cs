using System;
using Framework.IO;
using HermesProxy;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;
using Frozen = HermesProxy.Tests.World.Dispatch.Reference.FrozenPackets;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Equivalence for the loot and trade codecs against the frozen <c>Read()</c> bodies they replaced.
/// </summary>
/// <remarks>
/// Every case asserts field values <b>and</b> the reader's final position, over readers built
/// through <c>GetRemainingSpan()</c>. <c>LootItemPkt</c> and <c>LootMasterGive</c> get the most
/// attention: both size a loop from the wire, and <c>LootMasterGive</c> reads its target GUID
/// <i>between</i> the count and the elements, so a reordering there is invisible to an assertion
/// that only checks fields.
/// </remarks>
public class LootCodecEquivalenceTests
{
    static LootCodecEquivalenceTests()
    {
        if (VersionBootstrap.ModernBuild == ClientVersionBuild.Zero)
            VersionBootstrap.ModernBuild = ClientVersionBuild.V1_14_2_42597;
        if (VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            VersionBootstrap.LegacyBuild = ClientVersionBuild.V3_3_5a_12340;
    }

    private static (WorldPacket Oracle, byte[] Framed) Build(Action<WorldPacket> write)
    {
        using var w = new WorldPacket(1u);
        write(w);
        byte[] payload = w.GetData();
        byte[] framed = new byte[payload.Length + 2];
        payload.CopyTo(framed, 2);
        return (new WorldPacket(framed), framed);
    }

    private static SpanPacketReader ReaderOver(byte[] framed)
        => new(new WorldPacket(framed).GetRemainingSpan());

    private static readonly WowGuid128 Guid = new(0xDEADBEEFCAFEUL, 0x0123456789ABCDEFUL);
    private static readonly WowGuid128 Guid2 = new(0x1122334455667788UL, 0x99AABBCCDDEEFF00UL);
    private static readonly WowGuid128 Zero = default;

    // ---- guid-only ----

    [Fact]
    public void LootRelease_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.LootRelease(); e.Read(o);
        var r = ReaderOver(f); LootReleaseCodec.Read(ref r, out var a);
        Assert.Equal(e.Owner, a.Owner);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void LootUnit_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.LootUnit(); e.Read(o);
        var r = ReaderOver(f); LootUnitCodec.Read(ref r, out var a);
        Assert.Equal(e.Unit, a.Unit);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>An all-zero guid packs to two mask bytes and nothing else — the minimum encoding.</summary>
    [Fact]
    public void LootUnit_ZeroGuid_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Zero));
        var e = new Frozen.LootUnit(); e.Read(o);
        var r = ReaderOver(f); LootUnitCodec.Read(ref r, out var a);
        Assert.Equal(e.Unit, a.Unit);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>An empty body. The codec must consume nothing, not default a field into existence.</summary>
    [Fact]
    public void LootMoney_Matches()
    {
        var (o, f) = Build(_ => { });
        var e = new Frozen.LootMoney(); e.Read(o);
        var r = ReaderOver(f); LootMoneyCodec.Read(ref r, out var a);
        Assert.Equal(default, a);
        Assert.Equal(o.Remaining(), r.Remaining);
        Assert.Equal(0, r.Remaining);
        Assert.NotNull(e);
    }

    // ---- counted lists ----

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public void LootItemPkt_Matches(int count)
    {
        var (o, f) = Build(w =>
        {
            w.WriteUInt32((uint)count);
            for (int i = 0; i < count; i++)
            {
                w.WritePackedGuid128(i % 2 == 0 ? Guid : Guid2);
                w.WriteUInt8((byte)(i + 1));
            }
        });

        var e = new Frozen.LootItemPkt(); e.Read(o);
        var r = ReaderOver(f); LootItemPktCodec.Read(ref r, out var a);

        Assert.Equal(count, a.Loot.Count);
        Assert.Equal(e.Loot.Count, a.Loot.Count);
        for (int i = 0; i < count; i++)
        {
            Assert.Equal(e.Loot[i].LootObj, a.Loot[i].LootObj);
            Assert.Equal(e.Loot[i].LootListID, a.Loot[i].LootListID);
        }
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(4)]
    public void LootMasterGive_Matches(int count)
    {
        var (o, f) = Build(w =>
        {
            w.WriteUInt32((uint)count);
            w.WritePackedGuid128(Guid2);        // target, between the count and the elements
            for (int i = 0; i < count; i++)
            {
                w.WritePackedGuid128(Guid);
                w.WriteUInt8((byte)(i + 10));
            }
        });

        var e = new Frozen.LootMasterGive(); e.Read(o);
        var r = ReaderOver(f); LootMasterGiveCodec.Read(ref r, out var a);

        Assert.Equal(e.TargetGUID, a.TargetGUID);
        Assert.Equal(Guid2, a.TargetGUID);
        Assert.Equal(e.Loot.Count, a.Loot.Count);
        for (int i = 0; i < count; i++)
        {
            Assert.Equal(e.Loot[i].LootObj, a.Loot[i].LootObj);
            Assert.Equal(e.Loot[i].LootListID, a.Loot[i].LootListID);
        }
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- flat fields ----

    [Theory]
    [InlineData((sbyte)0, (byte)0, 0u)]
    [InlineData((sbyte)-1, (byte)2, 4u)]
    [InlineData((sbyte)127, (byte)1, uint.MaxValue)]
    public void SetLootMethod_Matches(sbyte partyIndex, byte method, uint threshold)
    {
        var (o, f) = Build(w =>
        {
            w.WriteInt8(partyIndex);
            w.WriteUInt8(method);
            w.WritePackedGuid128(Guid);
            w.WriteUInt32(threshold);
        });

        var e = new Frozen.SetLootMethod(); e.Read(o);
        var r = ReaderOver(f); SetLootMethodCodec.Read(ref r, out var a);

        Assert.Equal(e.PartyIndex, a.PartyIndex);
        Assert.Equal(e.LootMethod, a.LootMethod);
        Assert.Equal(e.LootMasterGUID, a.LootMasterGUID);
        Assert.Equal(e.LootThreshold, a.LootThreshold);
        Assert.Equal(partyIndex, a.PartyIndex);
        Assert.Equal(threshold, a.LootThreshold);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>A single bit, so the bit reader byte accounting is what is under test.</summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void OptOutOfLoot_Matches(bool passOnLoot)
    {
        var (o, f) = Build(w => w.WriteBit(passOnLoot));
        var e = new Frozen.OptOutOfLoot(); e.Read(o);
        var r = ReaderOver(f); OptOutOfLootCodec.Read(ref r, out var a);
        Assert.Equal(e.PassOnLoot, a.PassOnLoot);
        Assert.Equal(passOnLoot, a.PassOnLoot);
    }

    [Theory]
    [InlineData((byte)0, (byte)0)]
    [InlineData((byte)7, (byte)2)]
    [InlineData((byte)255, (byte)255)]
    public void LootRoll_Matches(byte lootListId, byte rollType)
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteUInt8(lootListId);
            w.WriteUInt8(rollType);
        });

        var e = new Frozen.LootRoll(); e.Read(o);
        var r = ReaderOver(f); LootRollCodec.Read(ref r, out var a);

        Assert.Equal(e.LootObj, a.LootObj);
        Assert.Equal(e.LootListID, a.LootListID);
        Assert.Equal(e.RollType, a.RollType);
        Assert.Equal(lootListId, a.LootListID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- trade ----

    [Fact]
    public void InitiateTrade_Matches()
    {
        var (o, f) = Build(w => w.WritePackedGuid128(Guid));
        var e = new Frozen.InitiateTrade(); e.Read(o);
        var r = ReaderOver(f); InitiateTradeCodec.Read(ref r, out var a);
        Assert.Equal(e.Guid, a.Guid);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>
    /// Coinage is 64-bit on the modern wire and narrows to 32 in the system. The codec must read
    /// the full width — a value above uint.MaxValue is the only way to catch a truncation here.
    /// </summary>
    [Theory]
    [InlineData(0ul)]
    [InlineData(1234567ul)]
    [InlineData((ulong)uint.MaxValue)]
    [InlineData(ulong.MaxValue)]
    public void SetTradeGold_Matches(ulong coinage)
    {
        var (o, f) = Build(w => w.WriteUInt64(coinage));
        var e = new Frozen.SetTradeGold(); e.Read(o);
        var r = ReaderOver(f); SetTradeGoldCodec.Read(ref r, out var a);
        Assert.Equal(e.Coinage, a.Coinage);
        Assert.Equal(coinage, a.Coinage);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(9u)]
    [InlineData(uint.MaxValue)]
    public void AcceptTrade_Matches(uint stateIndex)
    {
        var (o, f) = Build(w => w.WriteUInt32(stateIndex));
        var e = new Frozen.AcceptTrade(); e.Read(o);
        var r = ReaderOver(f); AcceptTradeCodec.Read(ref r, out var a);
        Assert.Equal(e.StateIndex, a.StateIndex);
        Assert.Equal(stateIndex, a.StateIndex);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)6)]
    [InlineData((byte)255)]
    public void ClearTradeItem_Matches(byte tradeSlot)
    {
        var (o, f) = Build(w => w.WriteUInt8(tradeSlot));
        var e = new Frozen.ClearTradeItem(); e.Read(o);
        var r = ReaderOver(f); ClearTradeItemCodec.Read(ref r, out var a);
        Assert.Equal(e.TradeSlot, a.TradeSlot);
        Assert.Equal(tradeSlot, a.TradeSlot);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>Three consecutive bytes — the one shape where a field swap is silent.</summary>
    [Theory]
    [InlineData((byte)0, (byte)0, (byte)0)]
    [InlineData((byte)1, (byte)2, (byte)3)]
    [InlineData((byte)255, (byte)19, (byte)23)]
    public void SetTradeItem_Matches(byte tradeSlot, byte packSlot, byte itemSlotInPack)
    {
        var (o, f) = Build(w =>
        {
            w.WriteUInt8(tradeSlot);
            w.WriteUInt8(packSlot);
            w.WriteUInt8(itemSlotInPack);
        });

        var e = new Frozen.SetTradeItem(); e.Read(o);
        var r = ReaderOver(f); SetTradeItemCodec.Read(ref r, out var a);

        Assert.Equal(e.TradeSlot, a.TradeSlot);
        Assert.Equal(e.PackSlot, a.PackSlot);
        Assert.Equal(e.ItemSlotInPack, a.ItemSlotInPack);
        Assert.Equal(tradeSlot, a.TradeSlot);
        Assert.Equal(packSlot, a.PackSlot);
        Assert.Equal(itemSlotInPack, a.ItemSlotInPack);
        Assert.Equal(o.Remaining(), r.Remaining);
    }
}
