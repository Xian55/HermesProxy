using System;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

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
            // Vanilla SMSG_ITEM_PUSH_RESULT has no inventory-total field, and the legacy
            // server doesn't slot-track item progress in PLAYER_QUEST_LOG_*_2 (kills only).
            // The player + item UpdateObject for this loot has already been applied by the
            // time we get here, so summing matching stacks gives the correct post-loot total.
            uint currentCount = GetSession().GameState.GetItemCountInInventory(item.Item.ItemID);
            item.QuantityInInventory = currentCount > 0 ? currentCount : item.Quantity;
        }

        if (item.Slot == Enums.Classic.InventorySlots.Bag0 && rawSlotInBag >= 0 &&
            item.PlayerGUID == GetSession().GameState.CurrentPlayerGuid)
            item.ItemGUID = GetSession().GameState.GetInventorySlotItem(rawSlotInBag).To128(GetSession().GameState);
        else
            item.ItemGUID = WowGuid128.Empty;
        
        SendPacketToClient(item);
        if (item.Item.ItemID != 0)
            SendItemQuestCredit(item.Item.ItemID);
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
                failure.Variant = new InventoryFailureLevelData(packet.ReadInt32());
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
                failure.Variant = new InventoryFailureLevelData(packet.ReadInt32());
                break;
            case InventoryResult.EventAutoEquipBindConfirm:
            {
                var srcContainer = packet.ReadGuid().To128(GetSession().GameState);
                var srcSlot = packet.ReadInt32();
                var dstContainer = packet.ReadGuid().To128(GetSession().GameState);
                failure.Variant = new InventoryFailureAutoEquipData(srcContainer, srcSlot, dstContainer);
                break;
            }
            case InventoryResult.ItemMaxLimitCategoryCountExceeded:
            case InventoryResult.ItemMaxLimitCategorySocketedExceeded:
            case InventoryResult.ItemMaxLimitCategoryEquippedExceeded:
                failure.Variant = new InventoryFailureLimitData(packet.ReadInt32());
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

        // The legacy packet carries no duration, so read it off the item template rather
        // than asserting a flat 30s for every item. Slots were indexed by the legacy spell
        // id when the item query was parsed. Keep the old constant as the last resort so a
        // backend that does send this opcode still shows *some* sweep.
        item.Cooldown = 30000;
        uint cooldownItemId = GetSession().GameState.GetItemId(item.ItemGuid);
        if (cooldownItemId != 0 && GameData.GetItemTemplate(cooldownItemId) is { } cooldownTemplate)
        {
            byte cooldownSlot = GameData.GetItemEffectSlot(cooldownItemId, item.SpellID);
            if (cooldownSlot < cooldownTemplate.TriggeredSpellCooldowns.Length)
            {
                int ms = cooldownTemplate.TriggeredSpellCooldowns[cooldownSlot];
                if (ms <= 0)
                    ms = cooldownTemplate.TriggeredSpellCategoryCooldowns[cooldownSlot];
                if (ms > 0)
                    item.Cooldown = (uint)ms;
            }
        }
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
    // 3.3.5a SMSG_SOCKET_GEMS_RESULT (0x50B) — the authoritative reply to CMSG_SOCKET_GEMS.
    // Payload is the item guid plus the enchant ids of SOCK1..SOCK3 and BONUS. Modern
    // clients have no equivalent packet (the proxy answers CMSG_SOCKET_GEMS optimistically
    // with SMSG_SOCKET_GEMS_SUCCESS), so nothing is forwarded — this refreshes the gem
    // cache that the V3_4_3 ItemData Gems dynamic field reads from, keeping it correct
    // even if the item's Values update omits the socket enchant slots.
    [PacketHandler(Opcode.SMSG_SOCKET_GEMS)]
    void HandleSocketGemsResult(WorldPacket packet)
    {
        var gameState = GetSession().GameState;
        WowGuid128 itemGuid = packet.ReadGuid().To128(gameState);

        Span<uint?> gems = stackalloc uint?[ItemConst.MaxGemSockets];
        for (int i = 0; i < ItemConst.MaxGemSockets; i++)
        {
            uint enchantId = packet.ReadUInt32();
            gems[i] = enchantId != 0 ? GameData.GetGemFromEnchantId(enchantId) : 0u;
        }
        // Trailing BONUS_ENCHANTMENT_SLOT id — the socket bonus, not a gem.

        gameState.SaveGemsForItem(itemGuid, gems);
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

    // The legacy packet is (owner, caster, item ENTRY, enchant id) — no item guid and no
    // enchantment slot (AzerothCore Item.cpp: SendEnchantmentLog(owner, caster, entry, id),
    // emitted for every slot below MAX_INSPECTED_ENCHANTMENT_SLOT). The modern packet needs
    // both. The item's own UPDATE_OBJECT carries the guid and the slot but not the caster,
    // and it always follows in the same server tick — so park the log here and let
    // ResolvePendingEnchantmentLog complete it from that update.
    //
    // The previous implementation guessed the guid by scanning equipped slots for a matching
    // entry (wrong item whenever the player owns two of the same entry) and left EnchantSlot
    // at a hardcoded 1 (TEMP) regardless of the real slot.
    [PacketHandler(Opcode.SMSG_ENCHANTMENT_LOG)]
    void HandleEnchantmentLog(WorldPacket packet)
    {
        var gameState = GetSession().GameState;

        WowGuid128 owner, caster;
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            owner = packet.ReadPackedGuid().To128(gameState);
            caster = packet.ReadPackedGuid().To128(gameState);
        }
        else
        {
            owner = packet.ReadGuid().To128(gameState);
            caster = packet.ReadGuid().To128(gameState);
        }
        int itemId = packet.ReadInt32();
        int enchantId = packet.ReadInt32();

        // An unresolved predecessor means no item update ever matched it — most often the
        // "old enchant cleared" log that precedes a replacement. Flush it best-effort so a
        // log is never silently swallowed.
        FlushPendingEnchantmentLog();

        gameState.PendingEnchantmentLog = new GameSessionData.PendingEnchantmentLogData
        {
            IsSet = true,
            Owner = owner,
            Caster = caster,
            ItemId = (uint)itemId,
            EnchantId = enchantId,
        };
    }

    /// <summary>
    /// Completes a parked SMSG_ENCHANTMENTLOG once the matching item update names the slot.
    /// Called from the item branch of the update parser.
    /// </summary>
    internal void ResolvePendingEnchantmentLog(WowGuid128 itemGuid, Objects.ItemData item)
    {
        var gameState = GetSession().GameState;
        ref var pending = ref gameState.PendingEnchantmentLog;
        if (!pending.IsSet || item.Enchantment == null)
            return;

        if (gameState.GetItemId(itemGuid) != pending.ItemId)
            return;

        for (int slot = 0; slot < item.Enchantment.Length; slot++)
        {
            if (item.Enchantment[slot]?.ID != pending.EnchantId)
                continue;

            SendPacketToClient(new EnchantmentLog
            {
                Owner = pending.Owner,
                Caster = pending.Caster,
                ItemGUID = itemGuid,
                ItemID = (int)pending.ItemId,
                Enchantment = pending.EnchantId,
                EnchantSlot = slot,
            });
            pending.IsSet = false;
            return;
        }
    }

    /// <summary>
    /// Sends a parked log that no item update claimed, resolving the guid by entry as a last
    /// resort. Slot is left at 0 (PERM) because nothing on the wire identifies it.
    /// </summary>
    private void FlushPendingEnchantmentLog()
    {
        var gameState = GetSession().GameState;
        ref var pending = ref gameState.PendingEnchantmentLog;
        if (!pending.IsSet)
            return;

        pending.IsSet = false;

        // Equipped gear plus equipped bags: slots 0..BagEnd-1.
        WowGuid128 itemGuid = default;
        for (int i = 0; i < Enums.Classic.InventorySlots.BagEnd; i++)
        {
            WowGuid128 slotGuid = gameState.GetInventorySlotItem(i).To128(gameState);
            if (gameState.GetItemId(slotGuid) == pending.ItemId)
            {
                itemGuid = slotGuid;
                break;
            }
        }
        if (itemGuid == default)
            return;

        SendPacketToClient(new EnchantmentLog
        {
            Owner = pending.Owner,
            Caster = pending.Caster,
            ItemGUID = itemGuid,
            ItemID = (int)pending.ItemId,
            Enchantment = pending.EnchantId,
            EnchantSlot = 0,
        });
    }
}
