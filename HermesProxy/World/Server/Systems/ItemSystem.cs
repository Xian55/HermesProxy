using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's item CMSGs: vendors, bags, equipment and gems.
/// </summary>
/// <remarks>
/// <para>
/// Bodies were moved from <c>World/Server/PacketHandlers/ItemHandler.cs</c>, not retyped;
/// <c>verify-handler-port.py</c> diffs each one against the original.
/// </para>
/// <para>
/// Most of what these do is slot translation. V3_4_3 numbers inventory slots as InvSlots
/// descriptor indexes where 3.3.5a uses its own scheme, and the <c>Bag0</c> test that recurs
/// throughout decides whether the bag or the slot is the one needing adjustment — a backpack item
/// carries its position in the slot, a bag item in the container.
/// </para>
/// </remarks>
public static class ItemSystem
{
    // Server category, not the Packet one every other handler uses: these traces have always
    // rendered under "S", and the test profile runs ServerLevel=Verbose with PacketLevel=Debug,
    // so moving them to Packet would silently drop every line.
    private static readonly Microsoft.Extensions.Logging.ILogger _melServerItem =
        Log.CreateMelLogger(Log.CategoryServer);
    private static readonly string _logSourceItem = "ItemHandler".PadRight(15);

    [HandlesCmsg(Opcode.CMSG_BUY_ITEM)]
    public static void HandleBuyItem(in BuyItem item, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_BUY_ITEM);
        packet.WriteGuid(item.VendorGUID.To64());
        packet.WriteUInt32(item.Item.ItemID);
        uint quantity = item.Quantity / ctx.GetSession().GameState.GetItemBuyCount(item.Item.ItemID);

        ItemLogMessages.VendorBuyItemForward(_melServerItem, _logSourceItem,
            item.VendorGUID.Low, item.VendorGUID.High, item.Item.ItemID, quantity,
            item.Quantity, (uint)item.MuID, (uint)item.Slot, (uint)item.BagSlot, (uint)item.ItemType);

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
        {
            // Legacy slot is the 1-based vendor-array index. 3.4.3 sends that as MuID.
            uint legacySlot = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261 ? item.MuID : item.Slot;
            packet.WriteUInt32(legacySlot);
            packet.WriteUInt32(quantity);
        }
        else
            packet.WriteUInt8((byte)quantity);
        packet.WriteUInt8((byte)item.BagSlot);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SELL_ITEM)]
    public static void HandleSellItem(in SellItem item, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SELL_ITEM);
        packet.WriteGuid(item.VendorGUID.To64());
        packet.WriteGuid(item.ItemGUID.To64());
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192)) // not sure when this was changed exactly
            packet.WriteUInt32(item.Amount);
        else
            packet.WriteUInt8((byte)item.Amount);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SPLIT_ITEM)]
    public static void HandleSplitItem(in SplitItem item, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SPLIT_ITEM);
        byte containerSlot1 = item.FromPackSlot != Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.FromPackSlot) : item.FromPackSlot;
        byte slot1 = item.FromPackSlot == Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.FromSlot) : item.FromSlot;
        byte containerSlot2 = item.ToPackSlot != Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.ToPackSlot) : item.ToPackSlot;
        byte slot2 = item.ToPackSlot == Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.ToSlot) : item.ToSlot;
        packet.WriteUInt8(containerSlot1);
        packet.WriteUInt8(slot1);
        packet.WriteUInt8(containerSlot2);
        packet.WriteUInt8(slot2);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            packet.WriteInt32(item.Quantity);
        else
            packet.WriteUInt8((byte)item.Quantity);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SWAP_INV_ITEM)]
    public static void HandleSwapInvItem(in SwapInvItem item, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SWAP_INV_ITEM);
        byte slot1 = ModernVersion.AdjustModernInventorySlotToLegacy(item.Slot1);
        byte slot2 = ModernVersion.AdjustModernInventorySlotToLegacy(item.Slot2);

        // The V3_4_3 client packs source into Slot2 (read first) and destination
        // into Slot1 (read second) — opposite of the field naming. The legacy
        // 3.3.5a CMSG_SWAP_INV_ITEM expects srcSlot then dstSlot, so for V3_4_3
        // we must flip our forward order. Without this, dragging an inventory
        // item to an equip slot reaches the server as "move from empty equip
        // slot to backpack slot" and silently fails. Mirrors fork
        // HermesProxy-WOTLK Server/WorldSocket.cs:HandleSwapInvItem.
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            ItemLogMessages.SwapInvItemForwardV343(_melServerItem, _logSourceItem, item.Slot2, item.Slot1, slot2, slot1);
            packet.WriteUInt8(slot2);
            packet.WriteUInt8(slot1);
        }
        else
        {
            packet.WriteUInt8(slot1);
            packet.WriteUInt8(slot2);
        }
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SWAP_ITEM)]
    public static void HandleSwapItem(in SwapItem item, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SWAP_ITEM);
        byte containerSlotB = item.ContainerSlotB != Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.ContainerSlotB) : item.ContainerSlotB;
        byte slotB = item.ContainerSlotB == Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.SlotB) : item.SlotB;
        byte containerSlotA = item.ContainerSlotA != Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.ContainerSlotA) : item.ContainerSlotA;
        byte slotA = item.ContainerSlotA == Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.SlotA) : item.SlotA;
        packet.WriteUInt8(containerSlotB);
        packet.WriteUInt8(slotB);
        packet.WriteUInt8(containerSlotA);
        packet.WriteUInt8(slotA);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_DESTROY_ITEM)]
    public static void HandleDestroyItem(in DestroyItem item, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_DESTROY_ITEM);
        byte containerSlot = item.ContainerId != Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.ContainerId) : item.ContainerId;
        byte slot = item.ContainerId == Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.SlotNum) : item.SlotNum;
        packet.WriteUInt8(containerSlot);
        packet.WriteUInt8(slot);
        packet.WriteUInt32(item.Count);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_AUTO_STORE_BAG_ITEM)]
    public static void HandleAutoStoreBagItem(in AutoStoreBagItem item, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_AUTO_STORE_BAG_ITEM);
        byte srcBag = item.ContainerSlotA != Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.ContainerSlotA) : item.ContainerSlotA;
        byte srcSlot = item.ContainerSlotA == Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.SlotA) : item.SlotA;
        byte dstBag = item.ContainerSlotB != Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.ContainerSlotB) : item.ContainerSlotB;
        packet.WriteUInt8(srcBag);
        packet.WriteUInt8(srcSlot);
        packet.WriteUInt8(dstBag);

        ItemLogMessages.AutoStoreBagItemForward(_melServerItem, _logSourceItem,
            item.ContainerSlotA, item.SlotA, item.ContainerSlotB, srcBag, srcSlot, dstBag);

        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_AUTO_EQUIP_ITEM)]
    [HandlesCmsg(Opcode.CMSG_AUTOSTORE_BANK_ITEM)]
    [HandlesCmsg(Opcode.CMSG_AUTOBANK_ITEM)]
    public static void HandleAutoEquipItem(Opcode opcode, in AutoEquipItem item, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(opcode);
        byte containerSlot = item.PackSlot != Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.PackSlot) : item.PackSlot;
        byte slot = item.PackSlot == Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.Slot) : item.Slot;
        packet.WriteUInt8(containerSlot);
        packet.WriteUInt8(slot);

        ItemLogMessages.AutoEquipForward(_melServerItem, _logSourceItem, opcode,
            item.PackSlot, item.Slot, containerSlot, slot);

        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_AUTO_EQUIP_ITEM_SLOT)]
    public static void HandleAutoEquipItemSlot(in AutoEquipItemSlot item, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_AUTO_EQUIP_ITEM_SLOT);
        packet.WriteGuid(item.Item.To64());
        byte slot = ModernVersion.AdjustModernInventorySlotToLegacy(item.ItemDstSlot);
        packet.WriteUInt8(slot);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_READ_ITEM)]
    public static void HandleReadItem(in ReadItem item, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_READ_ITEM);
        byte containerSlot = item.PackSlot != Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.PackSlot) : item.PackSlot;
        byte slot = item.PackSlot == Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.Slot) : item.Slot;
        packet.WriteUInt8(containerSlot);
        packet.WriteUInt8(slot);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_REPAIR_ITEM)]
    public static void HandleRepairItem(in RepairItem item, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_REPAIR_ITEM);
        packet.WriteGuid(item.VendorGUID.To64());
        packet.WriteGuid(item.ItemGUID.To64());
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            packet.WriteBool(item.UseGuildBank);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SOCKET_GEMS)]
    public static void HandleSocketGems(in SocketGems gems, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SOCKET_GEMS);
        packet.WriteGuid(gems.ItemGuid.To64());
        for (int i = 0; i < ItemConst.MaxGemSockets; ++i)
            packet.WriteGuid(gems.Gems[i].To64());
        ctx.SendPacketToServer(packet);

        // Packet does not exist in old clients.
        SocketGemsSuccess success = new SocketGemsSuccess();
        success.ItemGuid = gems.ItemGuid;
        ctx.SendPacket(success);
    }

    [HandlesCmsg(Opcode.CMSG_OPEN_ITEM)]
    public static void HandleOpenItem(in OpenItem item, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_OPEN_ITEM);
        byte containerSlot = item.PackSlot != Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.PackSlot) : item.PackSlot;
        byte slot = item.PackSlot == Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.Slot) : item.Slot;
        packet.WriteUInt8(containerSlot);
        packet.WriteUInt8(slot);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SET_AMMO)]
    public static void HandleSetAmmo(in SetAmmo ammo, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_AMMO);
        packet.WriteUInt32(ammo.ItemId);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_CANCEL_TEMP_ENCHANTMENT)]
    public static void HandleCancelTempEnchantment(in CancelTempEnchantment cancel, in SessionContext ctx)
    {
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            return;
        WorldPacket packet = new WorldPacket(Opcode.CMSG_CANCEL_TEMP_ENCHANTMENT);
        packet.WriteUInt32(cancel.EnchantmentSlot);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_WRAP_ITEM)]
    public static void HandleWrapItem(in WrapItem item, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_WRAP_ITEM);
        byte giftBag = item.GiftBag != Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.GiftBag) : item.GiftBag;
        byte giftSlot = item.GiftBag == Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.GiftSlot) : item.GiftSlot;
        byte itemBag = item.ItemBag != Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.ItemBag) : item.ItemBag;
        byte itemSlot = item.ItemBag == Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.ItemSlot) : item.ItemSlot;
        packet.WriteUInt8(giftBag);
        packet.WriteUInt8(giftSlot);
        packet.WriteUInt8(itemBag);
        packet.WriteUInt8(itemSlot);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_BUY_BACK_ITEM)]
    public static void HandleBuyBackItem(in BuyBackItem item, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_BUY_BACK_ITEM);
        packet.WriteGuid(item.VendorGUID.To64());
        byte slot = ModernVersion.AdjustModernInventorySlotToLegacy((byte)item.Slot);
        packet.WriteUInt32(slot);
        ctx.SendPacketToServer(packet);
    }
}
