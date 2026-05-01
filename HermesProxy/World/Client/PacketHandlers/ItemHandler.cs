using Framework;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [PacketHandler(Opcode.SMSG_SET_PROFICIENCY)]
    void HandleSetProficiency(WorldPacket packet)
    {
        SetProficiency proficiency = new SetProficiency();
        proficiency.ProficiencyClass = packet.ReadUInt8();
        proficiency.ProficiencyMask = packet.ReadUInt32();
        SendPacketToClient(proficiency);
    }
    [PacketHandler(Opcode.SMSG_BUY_SUCCEEDED)]
    void HandleBuySucceeded(WorldPacket packet)
    {
        BuySucceeded buy = new BuySucceeded();
        buy.VendorGUID = packet.ReadGuid().To128(GetSession().GameState);
        buy.Slot = packet.ReadUInt32();
        buy.NewQuantity = packet.ReadInt32();
        buy.QuantityBought = packet.ReadUInt32();
        SendPacketToClient(buy);
    }
    [PacketHandler(Opcode.SMSG_ITEM_PUSH_RESULT)]
    void HandleItemPushResult(WorldPacket packet)
    {
        ItemPushResult item = new ItemPushResult();
        item.PlayerGUID = packet.ReadGuid().To128(GetSession().GameState);
        bool fromNPC = packet.ReadUInt32() == 1;
        item.Created = packet.ReadUInt32() == 1;
        bool showInChat = packet.ReadUInt32() == 1;

        // V3_4_3 client renders the loot/receive chat line based on a
        // combination of DisplayText and Pushed/Created bits. Empirical
        // CypherCore behavior (see World_chat_messages_looted_item_chat_message
        // sniff): both vendor-buy and loot drops use DisplayText=1 — Pushed
        // distinguishes the chat line ("you receive item" vs "you loot").
        // Sending DisplayText=Loot (=3) silently hides the chat line on V3_4_3.
        if (fromNPC && !item.Created)
        {
            item.DisplayText = ItemPushResult.DisplayType.Received;
            item.Pushed = true;
        }
        else if (!showInChat)
            item.DisplayText = ItemPushResult.DisplayType.Hidden;
        else
            item.DisplayText = ItemPushResult.DisplayType.Received;

        item.Slot = packet.ReadUInt8();
        // Legacy SMSG_ITEM_PUSHED reports SlotInBag in legacy slot space; the
        // V3_4_3 client expects the InvSlots descriptor index (e.g. legacy 23 →
        // descriptor 35 for first backpack position). Without this translation
        // the V3_4_3 GetInventorySlotItem lookup below would return a stale GUID
        // and the chat line / item-tooltip wouldn't link to the new item.
        int rawSlotInBag = packet.ReadInt32();
        item.SlotInBag = item.Slot == Enums.Classic.InventorySlots.Bag0 && rawSlotInBag >= 0
            ? ModernVersion.AdjustLegacyInventorySlotToModern((byte)rawSlotInBag)
            : rawSlotInBag;
        item.Item.ItemID = packet.ReadUInt32();
        item.Item.RandomPropertiesSeed = packet.ReadUInt32();
        item.Item.RandomPropertiesID = packet.ReadUInt32();
        item.Quantity = packet.ReadUInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            item.QuantityInInventory = packet.ReadUInt32();
        else
        {
            uint currentCount = 0;
            QuestObjective? objective = GameData.GetQuestObjectiveForItem(item.Item.ItemID);
            if (objective != null)
            {
                var updateFields = GetSession().GameState.GetCachedObjectFieldsLegacy(GetSession().GameState.CurrentPlayerGuid);
                int questsCount = LegacyVersion.GetQuestLogSize();
                for (int i = 0; i < questsCount; i++)
                {
                    QuestLog? logEntry = ReadQuestLogEntry(i, null, updateFields!);
                    if (logEntry == null || logEntry.QuestID == null)
                        continue;
                    if (logEntry.QuestID != objective.QuestID)
                        continue;
                    if (logEntry.ObjectiveProgress[objective.StorageIndex] == null)
                        continue;

                    currentCount = (uint)logEntry.ObjectiveProgress[objective.StorageIndex]!;
                    break;
                }
            }
            item.QuantityInInventory = item.Quantity + currentCount;
        }

        if (item.Slot == Enums.Classic.InventorySlots.Bag0 && rawSlotInBag >= 0 &&
            item.PlayerGUID == GetSession().GameState.CurrentPlayerGuid)
            item.ItemGUID = GetSession().GameState.GetInventorySlotItem(rawSlotInBag).To128(GetSession().GameState);
        else
            item.ItemGUID = WowGuid128.Empty;
        
        SendPacketToClient(item);
    }
    [PacketHandler(Opcode.SMSG_READ_ITEM_RESULT_OK)]
    void HandleReadItemResultOk(WorldPacket packet)
    {
        ReadItemResultOK read = new ReadItemResultOK();
        read.ItemGUID = packet.ReadGuid().To128(GetSession().GameState);
        SendPacketToClient(read);
    }
    [PacketHandler(Opcode.SMSG_READ_ITEM_RESULT_FAILED)]
    void HandleReadItemResultFailed(WorldPacket packet)
    {
        ReadItemResultFailed read = new ReadItemResultFailed();
        read.ItemGUID = packet.ReadGuid().To128(GetSession().GameState);
        read.Subcode = 2;
        SendPacketToClient(read);
    }
    [PacketHandler(Opcode.SMSG_BUY_FAILED)]
    void HandleBuyFailed(WorldPacket packet)
    {
        BuyFailed fail = new BuyFailed();
        fail.VendorGUID = packet.ReadGuid().To128(GetSession().GameState);
        fail.Slot = packet.ReadUInt32();
        fail.Reason = (BuyResult)packet.ReadUInt8();
        SendPacketToClient(fail);
    }
    [PacketHandler(Opcode.SMSG_INVENTORY_CHANGE_FAILURE, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
    void HandleInventoryChangeFailureVanilla(WorldPacket packet)
    {
        InventoryChangeFailure failure = new();
        failure.BagResult = LegacyVersion.ConvertInventoryResult(packet.ReadUInt8());
        if (failure.BagResult == InventoryResult.Ok)
            return;

        switch (failure.BagResult)
        {
            case InventoryResult.CantEquipLevel:
                failure.Level = packet.ReadInt32();
                break;
        }

        failure.Item[0] = packet.ReadGuid().To128(GetSession().GameState);
        failure.Item[1] = packet.ReadGuid().To128(GetSession().GameState);
        failure.ContainerBSlot = packet.ReadUInt8();

        SendPacketToClient(failure);

        // Check if item use cast failed (queue-based)
        if (GetSession().GameState.TryDequeueItemCast(failure.Item[0], out var pendingCast))
        {
            GetSession().InstanceSocket.SendCastRequestFailed(pendingCast!, false);
        }
    }
    [PacketHandler(Opcode.SMSG_INVENTORY_CHANGE_FAILURE, ClientVersionBuild.V2_0_1_6180)]
    void HandleInventoryChangeFailure(WorldPacket packet)
    {
        InventoryChangeFailure failure = new();
        failure.BagResult = LegacyVersion.ConvertInventoryResult(packet.ReadUInt8());
        if (failure.BagResult == InventoryResult.Ok)
            return;

        failure.Item[0] = packet.ReadGuid().To128(GetSession().GameState);
        failure.Item[1] = packet.ReadGuid().To128(GetSession().GameState);
        failure.ContainerBSlot = packet.ReadUInt8();

        switch (failure.BagResult)
        {
            case InventoryResult.CantEquipLevel:
            case InventoryResult.PurchaseLevelTooLow:
                failure.Level = packet.ReadInt32();
                break;
            case InventoryResult.EventAutoEquipBindConfirm:
                failure.SrcContainer = packet.ReadGuid().To128(GetSession().GameState);
                failure.SrcSlot = packet.ReadInt32();
                failure.DstContainer = packet.ReadGuid().To128(GetSession().GameState);
                break;
            case InventoryResult.ItemMaxLimitCategoryCountExceeded:
            case InventoryResult.ItemMaxLimitCategorySocketedExceeded:
            case InventoryResult.ItemMaxLimitCategoryEquippedExceeded:
                failure.LimitCategory = packet.ReadInt32();
                break;
        }
        SendPacketToClient(failure);

        // Check if item use cast failed (queue-based)
        if (GetSession().GameState.TryDequeueItemCast(failure.Item[0], out var pendingCast))
        {
            GetSession().InstanceSocket.SendCastRequestFailed(pendingCast!, false);
        }
    }
    [PacketHandler(Opcode.SMSG_DURABILITY_DAMAGE_DEATH)]
    void HandleDurabilityDamageDeath(WorldPacket packet)
    {
        DurabilityDamageDeath death = new DurabilityDamageDeath();
        death.Percent = 10;
        SendPacketToClient(death);
    }
    [PacketHandler(Opcode.SMSG_ITEM_COOLDOWN)]
    void HandleItemCooldown(WorldPacket packet)
    {
        ItemCooldown item = new ItemCooldown();
        item.ItemGuid = packet.ReadGuid().To128(GetSession().GameState);
        item.SpellID = packet.ReadUInt32();
        item.Cooldown = 30000;
        SendPacketToClient(item);
    }
    [PacketHandler(Opcode.SMSG_SELL_RESPONSE)]
    void HandleSellResponse(WorldPacket packet)
    {
        SellResponse sell = new SellResponse();
        sell.VendorGUID = packet.ReadGuid().To128(GetSession().GameState);
        sell.ItemGUID = packet.ReadGuid().To128(GetSession().GameState);
        sell.Reason = packet.ReadUInt8();
        SendPacketToClient(sell);
    }
    [PacketHandler(Opcode.SMSG_ITEM_ENCHANT_TIME_UPDATE)]
    void HandleItemEnchantTimeUpdate(WorldPacket packet)
    {
        ItemEnchantTimeUpdate enchant = new ItemEnchantTimeUpdate();
        enchant.ItemGuid = packet.ReadGuid().To128(GetSession().GameState);
        enchant.Slot = packet.ReadUInt32();
        enchant.DurationLeft = packet.ReadUInt32();
        enchant.OwnerGuid = packet.ReadGuid().To128(GetSession().GameState);
        SendPacketToClient(enchant);
    }

    [PacketHandler(Opcode.SMSG_ENCHANTMENT_LOG)]
    void HandleEnchantmentLog(WorldPacket packet)
    {
        EnchantmentLog enchantment = new EnchantmentLog();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            enchantment.Owner = packet.ReadPackedGuid().To128(GetSession().GameState);
            enchantment.Caster = packet.ReadPackedGuid().To128(GetSession().GameState);
        }
        else
        {
            enchantment.Owner = packet.ReadGuid().To128(GetSession().GameState);
            enchantment.Caster = packet.ReadGuid().To128(GetSession().GameState);
        }
        enchantment.ItemID = packet.ReadInt32();
        var session = GetSession().GameState;

        for (int i = 0; i < 23; i++)
        {
            if (session.GetItemId(session.GetInventorySlotItem(i).To128(session)).Equals((uint)enchantment.ItemID))
            {
                enchantment.ItemGUID = session.GetInventorySlotItem(i).To128(session);
                break;
            }
        }
        if (enchantment.ItemGUID == default)
            return;

        enchantment.Enchantment = packet.ReadInt32();
        SendPacketToClient(enchantment);
    }
}
