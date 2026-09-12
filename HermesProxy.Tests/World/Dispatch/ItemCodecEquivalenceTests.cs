using System;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;
using Frozen = HermesProxy.Tests.World.Dispatch.Reference.FrozenPackets;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Equivalence for the item codecs against the frozen <c>Read()</c> bodies they replaced.
/// </summary>
/// <remarks>
/// <para>
/// <c>BuyItem</c> is the second packet in the migration whose layout differs by client build, and
/// it gets the treatment the chat codecs got: the pre-WotLK codec is proven against the oracle,
/// and the V3_4_3 codec against explicit expectations, because the oracle's own branch is decided
/// by <c>ModernVersion.Build</c> — <see langword="static readonly"/>, fixed for the process — and
/// the suite runs as V1_14. That the V3_4_3 branch was unreachable in-process is exactly what
/// ranged codecs fix.
/// </para>
/// <para>
/// <c>InvUpdate</c> gets its own attention. No handler reads it, so a codec that mis-sized it would
/// pass every field assertion and corrupt every field after it — the reader-position assertion is
/// the only thing that catches that, and the 0/1/2/3 count cases are what make it bite.
/// </para>
/// </remarks>
public class ItemCodecEquivalenceTests
{
    static ItemCodecEquivalenceTests()
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

    /// The suite's build, asserted rather than assumed — it decides which BuyItem codec the oracle
    /// can prove.
    [Fact]
    public void OracleBranchIsThePreWotLKClassicOne()
        => Assert.NotEqual(ClientVersionBuild.V3_4_3_54261, ModernVersion.Build);

    // ---- InvUpdate, the preamble on every drag ----

    private static void WriteInv(WorldPacket w, (byte Container, byte Slot)[] entries)
    {
        w.WriteBits((uint)entries.Length, 2);
        w.FlushBits();
        foreach (var (container, slot) in entries)
        {
            w.WriteUInt8(container);
            w.WriteUInt8(slot);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void AutoEquipItem_MatchesAtEveryInvCount(int invCount)
    {
        var entries = new (byte, byte)[invCount];
        for (int i = 0; i < invCount; i++)
            entries[i] = ((byte)(10 + i), (byte)(20 + i));

        var (o, f) = Build(w => { WriteInv(w, entries); w.WriteUInt8(4); w.WriteUInt8(7); });

        var e = new Frozen.AutoEquipItem(); e.Read(o);
        var r = ReaderOver(f); AutoEquipItemCodec.Read(ref r, out var a);

        Assert.Equal(e.PackSlot, a.PackSlot);
        Assert.Equal(e.Slot, a.Slot);
        Assert.Equal((byte)4, a.PackSlot);
        Assert.Equal((byte)7, a.Slot);

        Assert.Equal(e.Inv.Items.Count, a.Inv.Count);
        for (int i = 0; i < invCount; i++)
        {
            Assert.Equal(e.Inv.Items[i].ContainerSlot, a.Inv.Items[i].ContainerSlot);
            Assert.Equal(e.Inv.Items[i].Slot, a.Inv.Items[i].Slot);
        }

        // The only assertion that catches a mis-sized preamble.
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void SplitItem_Matches()
    {
        var (o, f) = Build(w =>
        {
            WriteInv(w, [((byte)1, (byte)2)]);
            w.WriteUInt8(11); w.WriteUInt8(12); w.WriteUInt8(13); w.WriteUInt8(14);
            w.WriteInt32(5);
        });

        var e = new Frozen.SplitItem(); e.Read(o);
        var r = ReaderOver(f); SplitItemCodec.Read(ref r, out var a);
        Assert.Equal(e.FromPackSlot, a.FromPackSlot);
        Assert.Equal(e.FromSlot, a.FromSlot);
        Assert.Equal(e.ToPackSlot, a.ToPackSlot);
        Assert.Equal(e.ToSlot, a.ToSlot);
        Assert.Equal(e.Quantity, a.Quantity);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>
    /// Slot2 is read first and Slot1 second — the reverse of the names — and the handler forwards
    /// them in a build-dependent order on top of that. Transposing them here would send an
    /// inventory drag as a move out of an empty equip slot.
    /// </summary>
    [Fact]
    public void SwapInvItem_Matches()
    {
        var (o, f) = Build(w => { WriteInv(w, []); w.WriteUInt8(23); w.WriteUInt8(35); });
        var e = new Frozen.SwapInvItem(); e.Read(o);
        var r = ReaderOver(f); SwapInvItemCodec.Read(ref r, out var a);
        Assert.Equal(e.Slot1, a.Slot1);
        Assert.Equal(e.Slot2, a.Slot2);
        Assert.Equal((byte)23, a.Slot2);
        Assert.Equal((byte)35, a.Slot1);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void SwapItem_Matches()
    {
        var (o, f) = Build(w =>
        {
            WriteInv(w, [((byte)3, (byte)4), ((byte)5, (byte)6)]);
            w.WriteUInt8(1); w.WriteUInt8(2); w.WriteUInt8(3); w.WriteUInt8(4);
        });

        var e = new Frozen.SwapItem(); e.Read(o);
        var r = ReaderOver(f); SwapItemCodec.Read(ref r, out var a);
        Assert.Equal(e.ContainerSlotB, a.ContainerSlotB);
        Assert.Equal(e.ContainerSlotA, a.ContainerSlotA);
        Assert.Equal(e.SlotB, a.SlotB);
        Assert.Equal(e.SlotA, a.SlotA);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void AutoStoreBagItem_Matches()
    {
        var (o, f) = Build(w => { WriteInv(w, [((byte)9, (byte)9)]); w.WriteUInt8(1); w.WriteUInt8(2); w.WriteUInt8(3); });
        var e = new Frozen.AutoStoreBagItem(); e.Read(o);
        var r = ReaderOver(f); AutoStoreBagItemCodec.Read(ref r, out var a);
        Assert.Equal(e.ContainerSlotA, a.ContainerSlotA);
        Assert.Equal(e.ContainerSlotB, a.ContainerSlotB);
        Assert.Equal(e.SlotA, a.SlotA);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void AutoEquipItemSlot_Matches()
    {
        var (o, f) = Build(w => { WriteInv(w, []); w.WritePackedGuid128(Guid); w.WriteUInt8(17); });
        var e = new Frozen.AutoEquipItemSlot(); e.Read(o);
        var r = ReaderOver(f); AutoEquipItemSlotCodec.Read(ref r, out var a);
        Assert.Equal(e.Item, a.Item);
        Assert.Equal(e.ItemDstSlot, a.ItemDstSlot);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- vendor ----

    [Theory]
    [InlineData(0u)]
    [InlineData(20u)]
    [InlineData(uint.MaxValue)]
    public void SellItem_Matches(uint amount)
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WritePackedGuid128(Guid2); w.WriteUInt32(amount); });
        var e = new Frozen.SellItem(); e.Read(o);
        var r = ReaderOver(f); SellItemCodec.Read(ref r, out var a);
        Assert.Equal(e.VendorGUID, a.VendorGUID);
        Assert.Equal(e.ItemGUID, a.ItemGUID);
        Assert.Equal(e.Amount, a.Amount);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RepairItem_Matches(bool useGuildBank)
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WritePackedGuid128(Guid2); w.WriteBit(useGuildBank); });
        var e = new Frozen.RepairItem(); e.Read(o);
        var r = ReaderOver(f); RepairItemCodec.Read(ref r, out var a);
        Assert.Equal(e.VendorGUID, a.VendorGUID);
        Assert.Equal(e.ItemGUID, a.ItemGUID);
        Assert.Equal(e.UseGuildBank, a.UseGuildBank);
        Assert.Equal(useGuildBank, a.UseGuildBank);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// An empty ItemInstance: no bonus block, no modifiers. Enough for BuyItem, whose handler only
    /// reads ItemID; QuestCodecEquivalenceTests exercises the full nested chain.
    private static void WriteBareItemInstance(WorldPacket w, uint itemId)
    {
        w.WriteUInt32(itemId);
        w.WriteUInt32(0);
        w.WriteUInt32(0);
        w.WriteBit(false);      // no ItemBonuses
        w.FlushBits();
        w.WriteBits(0u, 6);     // empty ItemModList
        w.FlushBits();
    }

    [Fact]
    public void BuyItem_PreWotLK_MatchesOracle()
    {
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WritePackedGuid128(Guid2);
            w.WriteUInt32(5);
            w.WriteUInt32(3);                       // Slot
            w.WriteUInt32(255);                     // BagSlot
            WriteBareItemInstance(w, 12345);
            w.WriteBits((uint)ItemVendorType.Item, 3);
        });

        var e = new Frozen.BuyItem(); e.Read(o);
        var r = ReaderOver(f); BuyItemCodecPreWotLKClassic.Read(ref r, out var a);

        Assert.Equal(e.VendorGUID, a.VendorGUID);
        Assert.Equal(e.ContainerGUID, a.ContainerGUID);
        Assert.Equal(e.Quantity, a.Quantity);
        Assert.Equal(e.Slot, a.Slot);
        Assert.Equal(e.BagSlot, a.BagSlot);
        Assert.Equal(e.ItemType, a.ItemType);
        Assert.Equal(e.Item.ItemID, a.Item.ItemID);
        Assert.Equal(0u, a.MuID);                   // absent on this layout
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// <summary>
    /// The layout the oracle cannot reach in-process. MuID precedes Slot here and ItemType is a
    /// full int32 rather than three bits — reading the other layout against it shifts every field
    /// from Slot onward, which is the bug this split exists to make unrepresentable.
    /// </summary>
    [Fact]
    public void BuyItem_WotLKClassic_ReadsMuIDAndTheReorderedTail()
    {
        var (_, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WritePackedGuid128(Guid2);
            w.WriteUInt32(5);
            w.WriteUInt32(7);                       // MuID
            w.WriteUInt32(3);                       // Slot
            w.WriteInt32((int)ItemVendorType.Item);
            WriteBareItemInstance(w, 12345);
        });

        var r = ReaderOver(f); BuyItemCodecWotLKClassic.Read(ref r, out var a);

        // No oracle comparison here — the frozen Read cannot take this branch in-process — so the
        // position assertion is against zero rather than against the oracle's remaining bytes.
        Assert.Equal(0, r.Remaining);
        Assert.Equal(Guid, a.VendorGUID);
        Assert.Equal(Guid2, a.ContainerGUID);
        Assert.Equal(5u, a.Quantity);
        Assert.Equal(7u, a.MuID);
        Assert.Equal(3u, a.Slot);
        Assert.Equal(0u, a.BagSlot);                // absent on this layout
        Assert.Equal(ItemVendorType.Item, a.ItemType);
        Assert.Equal(12345u, a.Item.ItemID);
    }

    // ---- the rest ----

    [Fact]
    public void DestroyItem_Matches()
    {
        var (o, f) = Build(w => { w.WriteUInt32(3); w.WriteUInt8(23); w.WriteUInt8(5); });
        var e = new Frozen.DestroyItem(); e.Read(o);
        var r = ReaderOver(f); DestroyItemCodec.Read(ref r, out var a);
        Assert.Equal(e.Count, a.Count);
        Assert.Equal(e.ContainerId, a.ContainerId);
        Assert.Equal(e.SlotNum, a.SlotNum);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void ReadItem_Matches()
    {
        var (o, f) = Build(w => { w.WriteUInt8(23); w.WriteUInt8(4); });
        var e = new Frozen.ReadItem(); e.Read(o);
        var r = ReaderOver(f); ReadItemCodec.Read(ref r, out var a);
        Assert.Equal(e.PackSlot, a.PackSlot);
        Assert.Equal(e.Slot, a.Slot);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void OpenItem_Matches()
    {
        var (o, f) = Build(w => { w.WriteUInt8(23); w.WriteUInt8(4); });
        var e = new Frozen.OpenItem(); e.Read(o);
        var r = ReaderOver(f); OpenItemCodec.Read(ref r, out var a);
        Assert.Equal(e.PackSlot, a.PackSlot);
        Assert.Equal(e.Slot, a.Slot);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// The leading byte is discarded by both readers; if the codec forgot it, every slot would be
    /// off by one field.
    [Fact]
    public void WrapItem_Matches()
    {
        var (o, f) = Build(w =>
        {
            w.WriteUInt8(128);
            w.WriteUInt8(1); w.WriteUInt8(2); w.WriteUInt8(3); w.WriteUInt8(4);
        });

        var e = new Frozen.WrapItem(); e.Read(o);
        var r = ReaderOver(f); WrapItemCodec.Read(ref r, out var a);
        Assert.Equal(e.GiftBag, a.GiftBag);
        Assert.Equal(e.GiftSlot, a.GiftSlot);
        Assert.Equal(e.ItemBag, a.ItemBag);
        Assert.Equal(e.ItemSlot, a.ItemSlot);
        Assert.Equal((byte)1, a.GiftBag);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void SocketGems_Matches()
    {
        WowGuid128[] gems = [Guid, Guid2, default];
        var (o, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            foreach (var g in gems)
                w.WritePackedGuid128(g);
        });

        var e = new Frozen.SocketGems(); e.Read(o);
        var r = ReaderOver(f); SocketGemsCodec.Read(ref r, out var a);
        Assert.Equal(e.ItemGuid, a.ItemGuid);
        for (int i = 0; i < ItemConst.MaxGemSockets; i++)
        {
            Assert.Equal(e.Gems[i], a.Gems[i]);
            Assert.Equal(gems[i], a.Gems[i]);
        }
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void SetAmmo_Matches()
    {
        var (o, f) = Build(w => w.WriteUInt32(2512));
        var e = new Frozen.SetAmmo(); e.Read(o);
        var r = ReaderOver(f); SetAmmoCodec.Read(ref r, out var a);
        Assert.Equal(e.ItemId, a.ItemId);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void CancelTempEnchantment_Matches()
    {
        var (o, f) = Build(w => w.WriteUInt32(1));
        var e = new Frozen.CancelTempEnchantment(); e.Read(o);
        var r = ReaderOver(f); CancelTempEnchantmentCodec.Read(ref r, out var a);
        Assert.Equal(e.EnchantmentSlot, a.EnchantmentSlot);
        Assert.Equal(o.Remaining(), r.Remaining);
    }
}
