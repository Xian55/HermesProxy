using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

// Item CMSG codecs — vendors, bags, equipment and gems.
//
// BuyItem is the second packet in the migration whose *shape* differs by client build, and the
// first outside chat. V3_4_3 reordered its trailing fields and inserted MuID, so the old reader
// branched inside one Read; reading the pre-WotLK layout against a 3.4.3 packet shifted every
// field after it and the proxy forwarded a garbage Slot, which the server rejected in silence.
// That branch is now two ranged codecs, decided once when the table is built.
//
// InvUpdate rides along with every drag and no handler reads it, but the codecs carry it rather
// than skipping it: if the preamble ever changes width, a field assertion fails instead of every
// following field quietly shifting.

public static class InvUpdateCodec
{
    /// <summary>Shared preamble. Mirrors the <c>InvUpdate(WorldPacket)</c> constructor.</summary>
    public static void Read(ref SpanPacketReader r, out InvUpdate inv)
    {
        int size = r.ReadBits<int>(2);
        r.ResetBitPos();

        InvItems items = default;
        for (int i = 0; i < size; ++i)
        {
            byte containerSlot = r.ReadUInt8();
            byte slot = r.ReadUInt8();
            items[i] = new InvItem(containerSlot, slot);
        }
        inv = new InvUpdate((byte)size, items);
    }
}

// ---- vendor ----

[PacketCodec(typeof(BuyItem), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class BuyItemCodecWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out BuyItem packet)
    {
        WowGuid128 vendorGuid = r.ReadPackedGuid128();
        WowGuid128 containerGuid = r.ReadPackedGuid128();
        uint quantity = r.ReadUInt32();

        // Layout mirrors fork HermesProxy-WOTLK Server/Packets/BuyItem.cs:Read for
        // ExpansionVersion >= 3. MuID is the 1-based vendor slot index returned in
        // SMSG_VENDOR_INVENTORY, and it is what the legacy server wants in its slot field.
        uint muId = r.ReadUInt32();
        uint slot = r.ReadUInt32();
        var itemType = (ItemVendorType)r.ReadInt32();
        var item = new ItemInstance();
        item.Read(ref r);

        // BagSlot is not on this layout, so it stays 0 — which is what the handler forwards.
        packet = new BuyItem(vendorGuid, containerGuid, quantity, muId, slot, BagSlot: 0, itemType, item);
    }
}

[PacketCodec(typeof(BuyItem), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class BuyItemCodecPreWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out BuyItem packet)
    {
        WowGuid128 vendorGuid = r.ReadPackedGuid128();
        WowGuid128 containerGuid = r.ReadPackedGuid128();
        uint quantity = r.ReadUInt32();

        uint slot = r.ReadUInt32();
        uint bagSlot = r.ReadUInt32();
        var item = new ItemInstance();
        item.Read(ref r);
        var itemType = (ItemVendorType)r.ReadBits<int>(3);

        // MuID does not exist before V3_4_3; the handler only reads it on that build.
        packet = new BuyItem(vendorGuid, containerGuid, quantity, MuID: 0, slot, bagSlot, itemType, item);
    }
}

public static class SellItemCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SellItem packet)
    {
        WowGuid128 vendorGuid = r.ReadPackedGuid128();
        WowGuid128 itemGuid = r.ReadPackedGuid128();
        uint amount = r.ReadUInt32();
        packet = new SellItem(vendorGuid, itemGuid, amount);
    }
}

public static class RepairItemCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out RepairItem packet)
    {
        WowGuid128 vendorGuid = r.ReadPackedGuid128();
        WowGuid128 itemGuid = r.ReadPackedGuid128();
        bool useGuildBank = r.HasBit();
        packet = new RepairItem(vendorGuid, itemGuid, useGuildBank);
    }
}

// ---- moving things around the bags ----

public static class SplitItemCodec
{
    public static void Read(ref SpanPacketReader r, out SplitItem packet)
    {
        InvUpdateCodec.Read(ref r, out var inv);
        byte fromPackSlot = r.ReadUInt8();
        byte fromSlot = r.ReadUInt8();
        byte toPackSlot = r.ReadUInt8();
        byte toSlot = r.ReadUInt8();
        int quantity = r.ReadInt32();
        packet = new SplitItem(inv, fromPackSlot, fromSlot, toPackSlot, toSlot, quantity);
    }
}

public static class SwapInvItemCodec
{
    public static void Read(ref SpanPacketReader r, out SwapInvItem packet)
    {
        InvUpdateCodec.Read(ref r, out var inv);
        byte slot2 = r.ReadUInt8();
        byte slot1 = r.ReadUInt8();
        packet = new SwapInvItem(inv, slot2, slot1);
    }
}

public static class SwapItemCodec
{
    public static void Read(ref SpanPacketReader r, out SwapItem packet)
    {
        InvUpdateCodec.Read(ref r, out var inv);
        byte containerSlotB = r.ReadUInt8();
        byte containerSlotA = r.ReadUInt8();
        byte slotB = r.ReadUInt8();
        byte slotA = r.ReadUInt8();
        packet = new SwapItem(inv, containerSlotB, containerSlotA, slotB, slotA);
    }
}

public static class AutoStoreBagItemCodec
{
    public static void Read(ref SpanPacketReader r, out AutoStoreBagItem packet)
    {
        InvUpdateCodec.Read(ref r, out var inv);
        byte containerSlotA = r.ReadUInt8();
        byte containerSlotB = r.ReadUInt8();
        byte slotA = r.ReadUInt8();
        packet = new AutoStoreBagItem(inv, containerSlotA, containerSlotB, slotA);
    }
}

public static class AutoEquipItemCodec
{
    public static void Read(ref SpanPacketReader r, out AutoEquipItem packet)
    {
        InvUpdateCodec.Read(ref r, out var inv);
        byte packSlot = r.ReadUInt8();
        byte slot = r.ReadUInt8();
        packet = new AutoEquipItem(inv, packSlot, slot);
    }
}

public static class AutoEquipItemSlotCodec
{
    public static void Read(ref SpanPacketReader r, out AutoEquipItemSlot packet)
    {
        InvUpdateCodec.Read(ref r, out var inv);
        WowGuid128 item = r.ReadPackedGuid128();
        byte itemDstSlot = r.ReadUInt8();
        packet = new AutoEquipItemSlot(inv, item, itemDstSlot);
    }
}

public static class DestroyItemCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out DestroyItem packet)
    {
        uint count = r.ReadUInt32();
        byte containerId = r.ReadUInt8();
        byte slotNum = r.ReadUInt8();
        packet = new DestroyItem(count, containerId, slotNum);
    }
}

// ---- two bytes, three different meanings ----

public static class ReadItemCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ReadItem packet)
    {
        byte packSlot = r.ReadUInt8();
        byte slot = r.ReadUInt8();
        packet = new ReadItem(packSlot, slot);
    }
}

public static class OpenItemCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out OpenItem packet)
    {
        byte packSlot = r.ReadUInt8();
        byte slot = r.ReadUInt8();
        packet = new OpenItem(packSlot, slot);
    }
}

public static class WrapItemCodec
{
    public static void Read(ref SpanPacketReader r, out WrapItem packet)
    {
        _ = r.ReadUInt8(); // Unknown Value. Usually 128
        byte giftBag = r.ReadUInt8();
        byte giftSlot = r.ReadUInt8();
        byte itemBag = r.ReadUInt8();
        byte itemSlot = r.ReadUInt8();
        packet = new WrapItem(giftBag, giftSlot, itemBag, itemSlot);
    }
}

// ---- gems, ammo, enchantment ----

public static class SocketGemsCodec
{
    public static void Read(ref SpanPacketReader r, out SocketGems packet)
    {
        WowGuid128 itemGuid = r.ReadPackedGuid128();
        GemSockets gems = default;
        for (int i = 0; i < ItemConst.MaxGemSockets; ++i)
            gems[i] = r.ReadPackedGuid128();
        packet = new SocketGems(itemGuid, gems);
    }
}

public static class SetAmmoCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SetAmmo packet)
        => packet = new SetAmmo(r.ReadUInt32());
}

public static class CancelTempEnchantmentCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out CancelTempEnchantment packet)
        => packet = new CancelTempEnchantment(r.ReadUInt32());
}
