using Framework;
using Framework.GameMath;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [HandlesSmsg(Opcode.SMSG_GOSSIP_MESSAGE)]
    internal void HandleGossipmessage(WorldPacket packet)
    {
        GossipMessagePkt gossip = new GossipMessagePkt();
        gossip.GossipGUID = packet.ReadGuid().To128(GetSession().GameState);
        GetSession().GameState.CurrentInteractedWithNPC = gossip.GossipGUID;

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089))
            gossip.GossipID = packet.ReadInt32();
        else
            gossip.GossipID = (int)gossip.GossipGUID.GetEntry();

        gossip.TextID = packet.ReadInt32();

        uint optionsCount = packet.ReadUInt32();

        for (uint i = 0; i < optionsCount; i++)
        {
            ClientGossipOption option = new ClientGossipOption();
            option.OptionIndex = packet.ReadInt32();
            option.OptionIcon = packet.ReadUInt8();
            option.OptionFlags = (byte)(packet.ReadBool() ? 1 : 0); // Code Box

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                option.OptionCost = packet.ReadInt32();

            option.Text = packet.ReadCString();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                option.Confirm = packet.ReadCString();
            gossip.GossipOptions.Add(option);
        }

        uint questsCount = packet.ReadUInt32();

        for (uint i = 0; i < questsCount; i++)
        {
            ClientGossipQuest quest = ReadGossipQuestOption(packet);
            gossip.GossipQuests.Add(quest);
        }

        if (ModernVersion.IsWotLKClassicOrLater)
        {
            int totalOptionTextLen = 0;
            int maxOptionTextLen = 0;
            foreach (var opt in gossip.GossipOptions)
            {
                int len = opt.Text?.Length ?? 0;
                totalOptionTextLen += len;
                if (len > maxOptionTextLen) maxOptionTextLen = len;
            }
            int totalQuestTextLen = 0;
            int maxQuestTextLen = 0;
            foreach (var q in gossip.GossipQuests)
            {
                int len = q.QuestTitle?.Length ?? 0;
                totalQuestTextLen += len;
                if (len > maxQuestTextLen) maxQuestTextLen = len;
            }
            Framework.Logging.Log.Print(Framework.Logging.LogType.Debug,
                $"[V343Trace][Gossip] guid={gossip.GossipGUID} gossipId={gossip.GossipID} textId={gossip.TextID} options={gossip.GossipOptions.Count} quests={gossip.GossipQuests.Count} optTextLen(total/max)={totalOptionTextLen}/{maxOptionTextLen} questTextLen(total/max)={totalQuestTextLen}/{maxQuestTextLen}");
        }

        var state = GetSession().GameState;
        // V3_4_3 only: a gossip list from the NPC we are mid-quest with would
        // replace the details / RequestItems frame with a dead overlay. Exactly 3.4.3, like
        // the CLOSE_INTERACTION / CLOSE_QUEST handlers that clear this state: on 4.4.2 nothing
        // clears it, and every later gossip was dropped.
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261
            && gossip.GossipGUID == state.CurrentInteractedWithNPC
            && (state.AwaitingQuestRewardId != 0 || state.QuestDetailsOpen))
            return;

        state.LastGossip = gossip;
        state.LastQuestList = null;
        state.CloseQuestDetails();
        SendPacketToClient(gossip);
    }

    [HandlesSmsg(Opcode.SMSG_GOSSIP_COMPLETE)]
    internal void HandleGossipComplete(WorldPacket packet)
    {
        GossipComplete gossip = new GossipComplete();
        SendPacketToClient(gossip);
    }

    [HandlesSmsg(Opcode.SMSG_GOSSIP_POI)]
    internal void HandleGossipPoi(WorldPacket packet)
    {
        GossipPOI poi = new();
        poi.Flags = packet.ReadUInt32();
        var pos2d = packet.ReadVector2();
        poi.Pos = new Vector3(pos2d.X, pos2d.Y, 0);
        poi.Icon = packet.ReadUInt32();
        poi.Importance = packet.ReadUInt32();
        poi.Name = packet.ReadCString();
        SendPacketToClient(poi);
    }

    [HandlesSmsg(Opcode.SMSG_BINDER_CONFIRM)]
    internal void HandleBinderConfirm(WorldPacket packet)
    {
        BinderConfirm confirm = new BinderConfirm();
        confirm.Guid = packet.ReadGuid().To128(GetSession().GameState);
        GetSession().GameState.CurrentInteractedWithNPC = confirm.Guid;
        SendPacketToClient(confirm);
    }

    // Shown (greyed, out of stock) only when a legacy vendor's entire stock is class-filtered to
    // empty, purely so the modern client opens the merchant frame so the player can still sell.
    // Cheap era-ubiquitous item; change freely.
    private const uint PlaceholderVendorItemId = 6948; // Hearthstone

    [HandlesSmsg(Opcode.SMSG_VENDOR_INVENTORY)]
    internal void HandleVendorInventory(WorldPacket packet)
    {
        VendorInventory vendor = new VendorInventory();
        int itemsCount;
        if (IsCataLegacy)
        {
            ReadVendorInventoryCata(packet, vendor);
            itemsCount = vendor.Items.Count;
        }
        else
        {
            vendor.VendorGUID = packet.ReadGuid().To128(GetSession().GameState);
            itemsCount = packet.ReadUInt8();
        }
        GetSession().GameState.CurrentInteractedWithNPC = vendor.VendorGUID;

        if (itemsCount == 0)
        {
            if (!IsCataLegacy)
                vendor.Reason = packet.ReadUInt8();
            // cMaNGOS class-filters a vendor's whole stock server-side (e.g. Cylina Darkheart, a
            // warlock-only vendor, sends 0 items to non-warlocks). The modern client opens the
            // merchant frame only when the list has >=1 item and Reason==0, so otherwise the player
            // can't even sell. Inject one out-of-stock placeholder (Quantity=0 => greyed + unbuyable)
            // so the frame opens. (#88)
            vendor.Reason = 0;
            vendor.Items.Add(new VendorItem
            {
                Slot = 1,
                Quantity = 0,        // 0 left in stock -> client greys it and blocks purchase
                StackCount = 1,
                Price = 1,
                Item = { ItemID = PlaceholderVendorItemId },
            });
            SendPacketToClient(vendor);
            return;
        }

        for (int i = 0; i < itemsCount && !IsCataLegacy; i++)
        {
            VendorItem vendorItem = new();
            vendorItem.Slot = packet.ReadInt32();
            vendorItem.Item.ItemID = packet.ReadUInt32();
            packet.ReadUInt32(); // Display Id
            vendorItem.Quantity = packet.ReadInt32();
            vendorItem.Price = packet.ReadUInt32();
            vendorItem.Durability = packet.ReadInt32();
            vendorItem.StackCount = packet.ReadUInt32();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                vendorItem.ExtendedCostID = packet.ReadInt32();
            // MuID is the 1-based vendor-array slot the server wrote, not the packed row index.
            vendorItem.MuID = (uint)vendorItem.Slot;
            GetSession().GameState.SetItemBuyCount(vendorItem.Item.ItemID, vendorItem.StackCount);
            vendor.Items.Add(vendorItem);
        }

        SendPacketToClient(vendor);
    }

    /// <summary>
    /// TrinityCore 4.3.4 VendorInventory::Write: a masked vendor GUID, a 21-bit item count and
    /// two optional-field bits per item in the bit section; each item then carries its own slot
    /// (MuID), currency-or-item type and display, and the reason byte sits between the GUID bytes.
    /// </summary>
    void ReadVendorInventoryCata(WorldPacket packet, VendorInventory vendor)
    {
        Span<bool> mask = stackalloc bool[8];
        Span<byte> guid = stackalloc byte[8];
        MaskedGuid.ReadMaskBits(packet, mask, [1, 0]);
        int count = (int)packet.ReadBits<uint>(21);
        MaskedGuid.ReadMaskBits(packet, mask, [3, 6, 5, 2, 7]);
        var hasExtendedCost = new bool[count];
        var hasCondition = new bool[count];
        for (int i = 0; i < count; i++)
        {
            hasExtendedCost[i] = !packet.HasBit();
            hasCondition[i] = !packet.HasBit();
        }
        MaskedGuid.ReadMaskBits(packet, mask, [4]);
        packet.ResetBitPos();

        for (int i = 0; i < count; i++)
        {
            VendorItem item = new();
            item.MuID = packet.ReadUInt32();
            item.Slot = (int)item.MuID;
            item.Durability = packet.ReadInt32();
            if (hasExtendedCost[i])
                item.ExtendedCostID = packet.ReadInt32();
            item.Item.ItemID = packet.ReadUInt32();
            item.Type = packet.ReadInt32();
            item.Price = packet.ReadUInt32();
            packet.ReadUInt32();                // ItemDisplayInfoID
            if (hasCondition[i])
                item.PlayerConditionFailed = packet.ReadInt32();
            item.Quantity = packet.ReadInt32();
            item.StackCount = packet.ReadUInt32();
            GetSession().GameState.SetItemBuyCount(item.Item.ItemID, item.StackCount);
            vendor.Items.Add(item);
        }

        MaskedGuid.ReadBytes(packet, mask, guid, [5, 4, 1, 0, 6]);
        vendor.Reason = packet.ReadUInt8();
        MaskedGuid.ReadBytes(packet, mask, guid, [2, 3, 7]);
        vendor.VendorGUID = new WowGuid64(MaskedGuid.ToUInt64(guid)).To128(GetSession().GameState);
    }

    [HandlesSmsg(Opcode.SMSG_SHOW_BANK)]
    internal void HandleShowBank(WorldPacket packet)
    {
        ShowBank bank = new ShowBank();
        bank.Guid = packet.ReadGuid().To128(GetSession().GameState);
        GetSession().GameState.CurrentInteractedWithNPC = bank.Guid;
        SendPacketToClient(bank);
    }

    [HandlesSmsg(Opcode.SMSG_TRAINER_LIST)]
    internal void HandleTrainerList(WorldPacket packet)
    {
        TrainerList trainer = new TrainerList();
        trainer.TrainerGUID = packet.ReadGuid().To128(GetSession().GameState);
        GetSession().GameState.CurrentInteractedWithNPC = trainer.TrainerGUID;
        trainer.TrainerID = trainer.TrainerGUID.GetEntry();
        trainer.TrainerType = packet.ReadInt32();
        // TrinityCore 4.3.4 sends its trainer id, which CMSG_TRAINER_BUY_SPELL has to echo back.
        if (IsCataLegacy)
            trainer.TrainerID = packet.ReadUInt32();
        int count = packet.ReadInt32();
        for (int i = 0; i < count; ++i)
        {
            TrainerListSpell spell = new();
            uint spellId = packet.ReadUInt32();
            if (ModernVersion.ExpansionVersion > 1 &&
                LegacyVersion.ExpansionVersion <= 1)
            {
                // in vanilla the server sends learn spell with effect 36
                // in expansions the server sends the actual spell
                uint realSpellId = GameData.GetRealSpell(spellId);
                if (realSpellId != spellId)
                {
                    GetSession().GameState.StoreRealSpell(realSpellId, spellId);
                    spellId = realSpellId;
                }
            }
            spell.SpellID = spellId;
            TrainerSpellStateLegacy stateOld = (TrainerSpellStateLegacy)packet.ReadUInt8();
            TrainerSpellStateModern stateNew = stateOld.CastEnum<TrainerSpellStateModern>();
            spell.Usable = stateNew;
            spell.MoneyCost = packet.ReadUInt32();
            if (IsCataLegacy)
            {
                // TrinityCore 4.3.4 TrainerList::Write: the profession dialog and button move to
                // the end, and only two required abilities. Read the 3.3.5a way, the first spell
                // ran into the second and the trainer window never opened.
                spell.ReqLevel = packet.ReadUInt8();
                spell.ReqSkillLine = packet.ReadUInt32();
                spell.ReqSkillRank = packet.ReadUInt32();
                spell.ReqAbility[0] = packet.ReadUInt32();
                spell.ReqAbility[1] = packet.ReadUInt32();
                packet.ReadInt32(); // Profession Dialog
                packet.ReadInt32(); // Profession Button
                trainer.Spells.Add(spell);
                continue;
            }
            packet.ReadInt32(); // Profession Dialog
            packet.ReadInt32(); // Profession Button
            spell.ReqLevel = packet.ReadUInt8();
            spell.ReqSkillLine = packet.ReadUInt32();
            spell.ReqSkillRank = packet.ReadUInt32();
            spell.ReqAbility[0] = packet.ReadUInt32();
            spell.ReqAbility[1] = packet.ReadUInt32();
            spell.ReqAbility[2] = packet.ReadUInt32();
            trainer.Spells.Add(spell);
        }
        trainer.Greeting = packet.ReadCString();
        SendPacketToClient(trainer);
    }

    [HandlesSmsg(Opcode.SMSG_TRAINER_BUY_FAILED)]
    internal void HandleTrainerBuyFailed(WorldPacket packet)
    {
        TrainerBuyFailed buy = new();
        buy.TrainerGUID = packet.ReadGuid().To128(GetSession().GameState);
        buy.SpellID = packet.ReadUInt32();
        buy.TrainerFailedReason = packet.ReadUInt32();
        SendPacketToClient(buy);
        ChatPkt chat = new ChatPkt(GetSession(), ChatMessageTypeModern.System, $"Failed to learn Spell {buy.SpellID} (Reason {buy.TrainerFailedReason}).");
        SendPacketToClient(chat);
    }

    [HandlesSmsg(Opcode.MSG_TALENT_WIPE_CONFIRM)]
    internal void HandleTalentWipeConfirm(WorldPacket packet)
    {
        RespecWipeConfirm respec = new();
        respec.TrainerGUID = packet.ReadGuid().To128(GetSession().GameState);
        respec.Cost = packet.ReadUInt32();
        SendPacketToClient(respec);
    }

    [HandlesSmsg(Opcode.SMSG_SPIRIT_HEALER_CONFIRM)]
    internal void HandleSpiritHealerConfirm(WorldPacket packet)
    {
        SpiritHealerConfirm confirm = new SpiritHealerConfirm();
        confirm.Guid = packet.ReadGuid().To128(GetSession().GameState);
        SendPacketToClient(confirm);
    }
}
