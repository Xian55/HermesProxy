using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server;

public partial class WorldSocket
{
    // Handlers for CMSG opcodes coming from the modern client
    [PacketHandler(Opcode.CMSG_BUY_ITEM)]
    void HandleBuyItem(BuyItem item)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_BUY_ITEM);
        packet.WriteGuid(item.VendorGUID.To64());
        packet.WriteUInt32(item.Item.ItemID);
        uint quantity = item.Quantity / GetSession().GameState.GetItemBuyCount(item.Item.ItemID);

        Log.Print(LogType.Trace,
            $"[VendorTrace] CMSG_BUY_ITEM forward: vendor={item.VendorGUID} itemID={item.Item.ItemID} " +
            $"quantity={quantity} (rawQty={item.Quantity}) MuID={item.MuID} Slot={item.Slot} BagSlot={item.BagSlot} ItemType={item.ItemType}");

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
        {
            // The legacy "Slot" field for CMSG_BUY_ITEM is the 1-based vendor
            // index. V3_4_3 sends that as MuID (which we populate in
            // NPCHandler.HandleVendorInventory as i+1); older modern clients
            // sent it as Slot. Forward whichever the client gave us.
            uint legacySlot = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261 ? item.MuID : item.Slot;
            packet.WriteUInt32(legacySlot);
            packet.WriteUInt32(quantity);
        }
        else
            packet.WriteUInt8((byte)quantity);
        packet.WriteUInt8((byte)item.BagSlot);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_SELL_ITEM)]
    void HandleSellItem(SellItem item)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SELL_ITEM);
        packet.WriteGuid(item.VendorGUID.To64());
        packet.WriteGuid(item.ItemGUID.To64());
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192)) // not sure when this was changed exactly
            packet.WriteUInt32(item.Amount);
        else
            packet.WriteUInt8((byte)item.Amount);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_SPLIT_ITEM)]
    void HandleSplitItem(SplitItem item)
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
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_SWAP_INV_ITEM)]
    void HandleSwapInvItem(SwapInvItem item)
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
            Log.Print(LogType.Trace,
                $"[InventoryTrace] CMSG_SWAP_INV_ITEM forward (V3_4_3): " +
                $"raw(Slot2={item.Slot2},Slot1={item.Slot1}) → legacy src={slot2} dst={slot1}");
            packet.WriteUInt8(slot2);
            packet.WriteUInt8(slot1);
        }
        else
        {
            packet.WriteUInt8(slot1);
            packet.WriteUInt8(slot2);
        }
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_SWAP_ITEM)]
    void HandleSwapItem(SwapItem item)
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
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_DESTROY_ITEM)]
    void HandleDestroyItem(DestroyItem item)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_DESTROY_ITEM);
        byte containerSlot = item.ContainerId != Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.ContainerId) : item.ContainerId;
        byte slot = item.ContainerId == Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.SlotNum) : item.SlotNum;
        packet.WriteUInt8(containerSlot);
        packet.WriteUInt8(slot);
        packet.WriteUInt32(item.Count);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_AUTO_EQUIP_ITEM)]
    [PacketHandler(Opcode.CMSG_AUTOSTORE_BANK_ITEM)]
    [PacketHandler(Opcode.CMSG_AUTOBANK_ITEM)]
    void HandleAutoEquipItem(AutoEquipItem item)
    {
        WorldPacket packet = new WorldPacket(item.GetUniversalOpcode());
        byte containerSlot = item.PackSlot != Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.PackSlot) : item.PackSlot;
        byte slot = item.PackSlot == Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.Slot) : item.Slot;
        packet.WriteUInt8(containerSlot);
        packet.WriteUInt8(slot);

        Log.Print(LogType.Trace,
            $"[InventoryTrace] {item.GetUniversalOpcode()} forward: " +
            $"raw(PackSlot={item.PackSlot},Slot={item.Slot}) → legacy bag={containerSlot} slot={slot}");

        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_AUTO_EQUIP_ITEM_SLOT)]
    void HandleAutoEquipItemSlot(AutoEquipItemSlot item)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_AUTO_EQUIP_ITEM_SLOT);
        packet.WriteGuid(item.Item.To64());
        byte slot = ModernVersion.AdjustModernInventorySlotToLegacy(item.ItemDstSlot);
        packet.WriteUInt8(slot);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_READ_ITEM)]
    void HandleReadItem(ReadItem item)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_READ_ITEM);
        byte containerSlot = item.PackSlot != Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.PackSlot) : item.PackSlot;
        byte slot = item.PackSlot == Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.Slot) : item.Slot;
        packet.WriteUInt8(containerSlot);
        packet.WriteUInt8(slot);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_BUY_BACK_ITEM)]
    void HandleBuyBackItem(BuyBackItem item)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_BUY_BACK_ITEM);
        packet.WriteGuid(item.VendorGUID.To64());
        byte slot = ModernVersion.AdjustModernInventorySlotToLegacy((byte)item.Slot);
        packet.WriteUInt32(slot);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_REPAIR_ITEM)]
    void HandleRepairItem(RepairItem item)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_REPAIR_ITEM);
        packet.WriteGuid(item.VendorGUID.To64());
        packet.WriteGuid(item.ItemGUID.To64());
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            packet.WriteBool(item.UseGuildBank);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_SOCKET_GEMS)]
    void HandleSocketGems(SocketGems gems)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SOCKET_GEMS);
        packet.WriteGuid(gems.ItemGuid.To64());
        for (int i = 0; i < ItemConst.MaxGemSockets; ++i)
            packet.WriteGuid(gems.Gems[i].To64());
        SendPacketToServer(packet);

        // Packet does not exist in old clients.
        SocketGemsSuccess success = new SocketGemsSuccess();
        success.ItemGuid = gems.ItemGuid;
        SendPacket(success);
    }

    [PacketHandler(Opcode.CMSG_OPEN_ITEM)]
    void HandleOpenItem(OpenItem item)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_OPEN_ITEM);
        byte containerSlot = item.PackSlot != Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.PackSlot) : item.PackSlot;
        byte slot = item.PackSlot == Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(item.Slot) : item.Slot;
        packet.WriteUInt8(containerSlot);
        packet.WriteUInt8(slot);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_SET_AMMO)]
    void HandleSetAmmo(SetAmmo ammo)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_AMMO);
        packet.WriteUInt32(ammo.ItemId);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_CANCEL_TEMP_ENCHANTMENT)]
    void HandleCancelTempEnchantment(CancelTempEnchantment cancel)
    {
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            return;
        WorldPacket packet = new WorldPacket(Opcode.CMSG_CANCEL_TEMP_ENCHANTMENT);
        packet.WriteUInt32(cancel.EnchantmentSlot);
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_WRAP_ITEM)]
    void HandleWrapItem(WrapItem item)
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
        SendPacketToServer(packet);
    }
}
