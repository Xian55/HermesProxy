/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using System;
using Framework.Constants;
using Framework.GameMath;
using Framework.IO;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class SetProficiency : ServerPacket, ISpanWritable
{
    public SetProficiency() : base(Opcode.SMSG_SET_PROFICIENCY, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(ProficiencyMask);
        _worldPacket.WriteUInt8(ProficiencyClass);
    }

    public int MaxSize => 5; // uint + byte

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(ProficiencyMask);
        writer.WriteUInt8(ProficiencyClass);
        return writer.Position;
    }

    public uint ProficiencyMask;
    public byte ProficiencyClass;
}

public class BuyBackItem : ClientPacket
{
    public BuyBackItem(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        VendorGUID = _worldPacket.ReadPackedGuid128();
        Slot = _worldPacket.ReadUInt32();
    }

    public WowGuid128 VendorGUID;
    public uint Slot;
}

public class BuyItem : ClientPacket
{
    public BuyItem(WorldPacket packet) : base(packet)
    {
        Item = new ItemInstance();
    }

    public override void Read()
    {
        VendorGUID = _worldPacket.ReadPackedGuid128();
        ContainerGUID = _worldPacket.ReadPackedGuid128();
        Quantity = _worldPacket.ReadUInt32();
        Slot = _worldPacket.ReadUInt32();
        BagSlot = _worldPacket.ReadUInt32();
        Item.Read(_worldPacket);
        ItemType = (ItemVendorType)_worldPacket.ReadBits<int>(3);
    }

    public WowGuid128 VendorGUID;
    public ItemInstance Item;
    public uint Slot;
    public uint BagSlot;
    public ItemVendorType ItemType;
    public uint Quantity;
    public WowGuid128 ContainerGUID;
}

public class BuySucceeded : ServerPacket, ISpanWritable
{
    public BuySucceeded() : base(Opcode.SMSG_BUY_SUCCEEDED) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(VendorGUID);
        _worldPacket.WriteUInt32(Slot);
        _worldPacket.WriteInt32(NewQuantity);
        _worldPacket.WriteUInt32(QuantityBought);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 12; // GUID + uint + int + uint

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(VendorGUID.Low, VendorGUID.High);
        writer.WriteUInt32(Slot);
        writer.WriteInt32(NewQuantity);
        writer.WriteUInt32(QuantityBought);
        return writer.Position;
    }

    public WowGuid128 VendorGUID;
    public uint Slot;
    public int NewQuantity;
    public uint QuantityBought;
}

public class BuyFailed : ServerPacket, ISpanWritable
{
    public BuyFailed() : base(Opcode.SMSG_BUY_FAILED) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(VendorGUID);
        _worldPacket.WriteUInt32(Slot);
        _worldPacket.WriteUInt8((byte)Reason);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 5; // GUID + uint + byte

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(VendorGUID.Low, VendorGUID.High);
        writer.WriteUInt32(Slot);
        writer.WriteUInt8((byte)Reason);
        return writer.Position;
    }

    public WowGuid128 VendorGUID;
    public uint Slot;
    public BuyResult Reason = BuyResult.CantFindItem;
}

class ItemPushResult : ServerPacket, ISpanWritable
{
    public ItemPushResult() : base(Opcode.SMSG_ITEM_PUSH_RESULT) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(PlayerGUID);
        _worldPacket.WriteUInt8(Slot);
        _worldPacket.WriteInt32(SlotInBag);
        _worldPacket.WriteInt32(QuestLogItemID);
        _worldPacket.WriteUInt32(Quantity);
        _worldPacket.WriteUInt32(QuantityInInventory);
        _worldPacket.WriteInt32(DungeonEncounterID);
        _worldPacket.WriteInt32(BattlePetSpeciesID);
        _worldPacket.WriteInt32(BattlePetBreedID);
        _worldPacket.WriteUInt32(BattlePetBreedQuality);
        _worldPacket.WriteInt32(BattlePetLevel);
        _worldPacket.WritePackedGuid128(ItemGUID);
        _worldPacket.WriteBit(Pushed);
        _worldPacket.WriteBit(Created);
        _worldPacket.WriteBits((uint)DisplayText, 3);
        _worldPacket.WriteBit(IsBonusRoll);
        _worldPacket.WriteBit(IsEncounterLoot);
        _worldPacket.FlushBits();

        Item.Write(_worldPacket);
    }

    // 2 GUIDs (18 each) + byte + 9 ints + 1 byte bits + ItemInstance
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2 + 1 + 9 * 4 + 1 +
                          ItemPacketHelpers.ItemInstanceMaxSize;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(PlayerGUID.Low, PlayerGUID.High);
        writer.WriteUInt8(Slot);
        writer.WriteInt32(SlotInBag);
        writer.WriteInt32(QuestLogItemID);
        writer.WriteUInt32(Quantity);
        writer.WriteUInt32(QuantityInInventory);
        writer.WriteInt32(DungeonEncounterID);
        writer.WriteInt32(BattlePetSpeciesID);
        writer.WriteInt32(BattlePetBreedID);
        writer.WriteUInt32(BattlePetBreedQuality);
        writer.WriteInt32(BattlePetLevel);
        writer.WritePackedGuid128(ItemGUID.Low, ItemGUID.High);
        writer.WriteBit(Pushed);
        writer.WriteBit(Created);
        writer.WriteBits((uint)DisplayText, 3);
        writer.WriteBit(IsBonusRoll);
        writer.WriteBit(IsEncounterLoot);
        writer.FlushBits();

        if (!ItemPacketHelpers.WriteItemInstance(ref writer, Item))
            return -1;

        return writer.Position;
    }

    public WowGuid128 PlayerGUID;
    public byte Slot;
    public int SlotInBag;
    public ItemInstance Item = new();
    public int QuestLogItemID;// Item ID used for updating quest progress
                              // only set if different than real ID (similar to CreatureTemplate.KillCredit)
    public uint Quantity;
    public uint QuantityInInventory;
    public int DungeonEncounterID;
    public int BattlePetSpeciesID;
    public int BattlePetBreedID;
    public uint BattlePetBreedQuality;
    public int BattlePetLevel;
    public WowGuid128 ItemGUID;
    public bool Pushed;
    public DisplayType DisplayText;
    public bool Created;
    public bool IsBonusRoll;
    public bool IsEncounterLoot;

    public enum DisplayType
    {
        Hidden = 0,
        Received = 1,
        EncounterLoot = 2,
        Loot = 3,
    }
}

public class SellItem : ClientPacket
{
    public SellItem(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        VendorGUID = _worldPacket.ReadPackedGuid128();
        ItemGUID = _worldPacket.ReadPackedGuid128();
        Amount = _worldPacket.ReadUInt32();
    }

    public WowGuid128 VendorGUID;
    public WowGuid128 ItemGUID;
    public uint Amount;
}

public class SellResponse : ServerPacket, ISpanWritable
{
    public SellResponse() : base(Opcode.SMSG_SELL_RESPONSE) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(VendorGUID);
        _worldPacket.WritePackedGuid128(ItemGUID);
        _worldPacket.WriteUInt8(Reason);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2 + 1; // 2 GUIDs + byte

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(VendorGUID.Low, VendorGUID.High);
        writer.WritePackedGuid128(ItemGUID.Low, ItemGUID.High);
        writer.WriteUInt8(Reason);
        return writer.Position;
    }

    public WowGuid128 VendorGUID;
    public WowGuid128 ItemGUID;
    public byte Reason;
}

public class SplitItem : ClientPacket
{
    public SplitItem(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Inv = new InvUpdate(_worldPacket);
        FromPackSlot = _worldPacket.ReadUInt8();
        FromSlot = _worldPacket.ReadUInt8();
        ToPackSlot = _worldPacket.ReadUInt8();
        ToSlot = _worldPacket.ReadUInt8();
        Quantity = _worldPacket.ReadInt32();
    }

    public byte ToSlot;
    public byte ToPackSlot;
    public byte FromPackSlot;
    public int Quantity;
    public InvUpdate Inv;
    public byte FromSlot;
}

public class SwapInvItem : ClientPacket
{
    public SwapInvItem(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Inv = new InvUpdate(_worldPacket);
        Slot2 = _worldPacket.ReadUInt8();
        Slot1 = _worldPacket.ReadUInt8();
    }

    public InvUpdate Inv;
    public byte Slot1; // Source Slot
    public byte Slot2; // Destination Slot
}

public class SwapItem : ClientPacket
{
    public SwapItem(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Inv = new InvUpdate(_worldPacket);
        ContainerSlotB = _worldPacket.ReadUInt8();
        ContainerSlotA = _worldPacket.ReadUInt8();
        SlotB = _worldPacket.ReadUInt8();
        SlotA = _worldPacket.ReadUInt8();
    }

    public InvUpdate Inv;
    public byte SlotA;
    public byte ContainerSlotB;
    public byte SlotB;
    public byte ContainerSlotA;
}

public class AutoEquipItem : ClientPacket
{
    public AutoEquipItem(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Inv = new InvUpdate(_worldPacket);
        PackSlot = _worldPacket.ReadUInt8();
        Slot = _worldPacket.ReadUInt8();
    }

    public byte Slot;
    public InvUpdate Inv;
    public byte PackSlot;
}

class AutoEquipItemSlot : ClientPacket
{
    public AutoEquipItemSlot(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Inv = new InvUpdate(_worldPacket);
        Item = _worldPacket.ReadPackedGuid128();
        ItemDstSlot = _worldPacket.ReadUInt8();
    }

    public WowGuid128 Item;
    public byte ItemDstSlot;
    public InvUpdate Inv;
}

public struct InvUpdate
{
    public InvUpdate(WorldPacket data)
    {
        Items = new List<InvItem>();
        int size = data.ReadBits<int>(2);
        data.ResetBitPos();
        for (int i = 0; i < size; ++i)
        {
            var item = new InvItem
            {
                ContainerSlot = data.ReadUInt8(),
                Slot = data.ReadUInt8()
            };
            Items.Add(item);
        }
    }

    public List<InvItem> Items;

    public struct InvItem
    {
        public byte ContainerSlot;
        public byte Slot;
    }
}

public class DestroyItem : ClientPacket
{
    public DestroyItem(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Count = _worldPacket.ReadUInt32();
        ContainerId = _worldPacket.ReadUInt8();
        SlotNum = _worldPacket.ReadUInt8();
    }

    public uint Count;
    public byte SlotNum;
    public byte ContainerId;
}

public class ItemInstance
{
    public uint ItemID;
    public uint RandomPropertiesSeed;
    public uint RandomPropertiesID;
    public ItemBonuses ItemBonus = null!;
    public ItemModList Modifications = new();

    public void Write(WorldPacket data)
    {
        data.WriteUInt32(ItemID);
        data.WriteUInt32(RandomPropertiesSeed);
        data.WriteUInt32(RandomPropertiesID);

        data.WriteBit(ItemBonus != null);
        data.FlushBits();

        Modifications.Write(data);

        if (ItemBonus != null)
            ItemBonus.Write(data);
    }

    public void Read(WorldPacket data)
    {
        ItemID = data.ReadUInt32();
        RandomPropertiesSeed = data.ReadUInt32();
        RandomPropertiesID = data.ReadUInt32();

        if (data.HasBit())
            ItemBonus = new();
        data.ResetBitPos();

        Modifications.Read(data);

        if (ItemBonus != null)
            ItemBonus.Read(data);
    }
}

public class ItemBonuses
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt8((byte)Context);
        data.WriteInt32(BonusListIDs.Count);
        foreach (uint bonusID in BonusListIDs)
            data.WriteUInt32(bonusID);
    }

    public void Read(WorldPacket data)
    {
        Context = (ItemContext)data.ReadUInt8();
        uint bonusListIdSize = data.ReadUInt32();

        BonusListIDs = new List<uint>();
        for (uint i = 0u; i < bonusListIdSize; ++i)
        {
            uint bonusId = data.ReadUInt32();
            BonusListIDs.Add(bonusId);
        }
    }

    public ItemContext Context;
    public List<uint> BonusListIDs = new();
}

public class ItemMod
{
    public uint Value;
    public ItemModifier Type;

    public ItemMod()
    {
        Type = ItemModifier.Max;
    }
    public ItemMod(uint value, ItemModifier type)
    {
        Value = value;
        Type = type;
    }

    public void Read(WorldPacket data)
    {
        Value = data.ReadUInt32();
        Type = (ItemModifier)data.ReadUInt8();
    }

    public void Write(WorldPacket data)
    {
        data.WriteUInt32(Value);
        data.WriteUInt8((byte)Type);
    }
}

public class ItemModList
{
    public List<ItemMod> Values = new((int)ItemModifier.Max);

    public void Read(WorldPacket data)
    {
        var itemModListCount = data.ReadBits<uint>(6);
        data.ResetBitPos();

        for (var i = 0; i < itemModListCount; ++i)
        {
            var itemMod = new ItemMod();
            itemMod.Read(data);
            Values.Add(itemMod);
        }
    }

    public void Write(WorldPacket data)
    {
        data.WriteBits(Values.Count, 6);
        data.FlushBits();

        foreach (ItemMod itemMod in Values)
            itemMod.Write(data);
    }
}

class ReadItem : ClientPacket
{
    public ReadItem(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        PackSlot = _worldPacket.ReadUInt8();
        Slot = _worldPacket.ReadUInt8();
    }

    public byte PackSlot;
    public byte Slot;
}

class ReadItemResultFailed : ServerPacket, ISpanWritable
{
    public ReadItemResultFailed() : base(Opcode.SMSG_READ_ITEM_RESULT_FAILED) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(ItemGUID);
        _worldPacket.WriteUInt32(Delay);
        _worldPacket.WriteBits(Subcode, 2);
        _worldPacket.FlushBits();
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 5; // GUID + uint + 1 byte for bits

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(ItemGUID.Low, ItemGUID.High);
        writer.WriteUInt32(Delay);
        writer.WriteBits(Subcode, 2);
        writer.FlushBits();
        return writer.Position;
    }

    public WowGuid128 ItemGUID;
    public uint Delay;
    public byte Subcode;
}

class ReadItemResultOK : ServerPacket, ISpanWritable
{
    public ReadItemResultOK() : base(Opcode.SMSG_READ_ITEM_RESULT_OK) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(ItemGUID);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(ItemGUID.Low, ItemGUID.High);
        return writer.Position;
    }

    public WowGuid128 ItemGUID;
}

// Variant payloads for InventoryFailureVariant union — each shape carries
// only the fields valid for its BagResult dispatch case.
public sealed record InventoryFailureLevelData(int Level);
public sealed record InventoryFailureAutoEquipData(WowGuid128 SrcContainer, int SrcSlot, WowGuid128 DstContainer);
public sealed record InventoryFailureLimitData(int LimitCategory);

public union InventoryFailureVariant(
    InventoryFailureLevelData,
    InventoryFailureAutoEquipData,
    InventoryFailureLimitData);

public class InventoryChangeFailure : ServerPacket, ISpanWritable
{
    public InventoryChangeFailure() : base(Opcode.SMSG_INVENTORY_CHANGE_FAILURE) { }

    public override void Write()
    {
        _worldPacket.WriteInt8((sbyte)BagResult);
        _worldPacket.WritePackedGuid128(Item[0]);
        _worldPacket.WritePackedGuid128(Item[1]);
        _worldPacket.WriteUInt8(ContainerBSlot); // bag type subclass, used with EQUIP_ERR_EVENT_AUTOEQUIP_BIND_CONFIRM and EQUIP_ERR_WRONG_BAG_TYPE_2

        switch (Variant.Value)
        {
            case InventoryFailureLevelData level:
                _worldPacket.WriteInt32(level.Level);
                break;
            case InventoryFailureAutoEquipData autoEquip:
                _worldPacket.WritePackedGuid128(autoEquip.SrcContainer);
                _worldPacket.WriteInt32(autoEquip.SrcSlot);
                _worldPacket.WritePackedGuid128(autoEquip.DstContainer);
                break;
            case InventoryFailureLimitData limit:
                _worldPacket.WriteInt32(limit.LimitCategory);
                break;
        }
    }

    // Fixed: sbyte + 2 GUIDs + byte = 2 + 36 = 38
    // Max additional (EventAutoEquipBindConfirm): 2 GUIDs + int = 40
    public int MaxSize => 2 + PackedGuidHelper.MaxPackedGuid128Size * 2 +
                          PackedGuidHelper.MaxPackedGuid128Size * 2 + 4;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt8((sbyte)BagResult);
        writer.WritePackedGuid128(Item[0].Low, Item[0].High);
        writer.WritePackedGuid128(Item[1].Low, Item[1].High);
        writer.WriteUInt8(ContainerBSlot);

        switch (Variant.Value)
        {
            case InventoryFailureLevelData level:
                writer.WriteInt32(level.Level);
                break;
            case InventoryFailureAutoEquipData autoEquip:
                writer.WritePackedGuid128(autoEquip.SrcContainer.Low, autoEquip.SrcContainer.High);
                writer.WriteInt32(autoEquip.SrcSlot);
                writer.WritePackedGuid128(autoEquip.DstContainer.Low, autoEquip.DstContainer.High);
                break;
            case InventoryFailureLimitData limit:
                writer.WriteInt32(limit.LimitCategory);
                break;
        }

        return writer.Position;
    }

    public InventoryResult BagResult;
    public byte ContainerBSlot;
    public WowGuid128[] Item = new WowGuid128[2];

    /// <summary>
    /// Discriminated union holding fields specific to BagResult. Most BagResult
    /// values carry no payload and leave Variant.Value as null; only the level,
    /// auto-equip and limit-category cases populate it.
    /// </summary>
    public InventoryFailureVariant Variant;
}

public class RepairItem : ClientPacket
{
    public RepairItem(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        VendorGUID = _worldPacket.ReadPackedGuid128();
        ItemGUID = _worldPacket.ReadPackedGuid128();
        UseGuildBank = _worldPacket.HasBit();
    }

    public WowGuid128 VendorGUID;
    public WowGuid128 ItemGUID;
    public bool UseGuildBank;
}

class SocketGems : ClientPacket
{
    public SocketGems(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        ItemGuid = _worldPacket.ReadPackedGuid128();
        for (int i = 0; i < ItemConst.MaxGemSockets; ++i)
            Gems[i] = _worldPacket.ReadPackedGuid128();
    }

    public WowGuid128 ItemGuid;
    public WowGuid128[] Gems = new WowGuid128[ItemConst.MaxGemSockets];
}

class SocketGemsSuccess : ServerPacket, ISpanWritable
{
    public SocketGemsSuccess() : base(Opcode.SMSG_SOCKET_GEMS_SUCCESS, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(ItemGuid);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(ItemGuid.Low, ItemGuid.High);
        return writer.Position;
    }

    public WowGuid128 ItemGuid;
}

class DurabilityDamageDeath : ServerPacket, ISpanWritable
{
    public DurabilityDamageDeath() : base(Opcode.SMSG_DURABILITY_DAMAGE_DEATH) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(Percent);
    }

    public int MaxSize => 4; // uint

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(Percent);
        return writer.Position;
    }

    public uint Percent;
}

class ItemCooldown : ServerPacket, ISpanWritable
{
    public ItemCooldown() : base(Opcode.SMSG_ITEM_COOLDOWN) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(ItemGuid);
        _worldPacket.WriteUInt32(SpellID);
        _worldPacket.WriteUInt32(Cooldown);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 8; // GUID + 2 uints

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(ItemGuid.Low, ItemGuid.High);
        writer.WriteUInt32(SpellID);
        writer.WriteUInt32(Cooldown);
        return writer.Position;
    }

    public WowGuid128 ItemGuid;
    public uint SpellID;
    public uint Cooldown;
}

public class OpenItem : ClientPacket
{
    public OpenItem(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        PackSlot = _worldPacket.ReadUInt8();
        Slot = _worldPacket.ReadUInt8();
    }

    public byte PackSlot;
    public byte Slot;
}

public class SetAmmo : ClientPacket
{
    public SetAmmo(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        ItemId = _worldPacket.ReadUInt32();
    }

    public uint ItemId;
}

class ItemEnchantTimeUpdate : ServerPacket, ISpanWritable
{
    public ItemEnchantTimeUpdate() : base(Opcode.SMSG_ITEM_ENCHANT_TIME_UPDATE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(ItemGuid);
        _worldPacket.WriteUInt32(DurationLeft);
        _worldPacket.WriteUInt32(Slot);
        _worldPacket.WritePackedGuid128(OwnerGuid);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2 + 8; // 2 GUIDs + 2 uints

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(ItemGuid.Low, ItemGuid.High);
        writer.WriteUInt32(DurationLeft);
        writer.WriteUInt32(Slot);
        writer.WritePackedGuid128(OwnerGuid.Low, OwnerGuid.High);
        return writer.Position;
    }

    public WowGuid128 ItemGuid;
    public uint DurationLeft;
    public uint Slot;
    public WowGuid128 OwnerGuid;
}

class EnchantmentLog : ServerPacket, ISpanWritable
{
    public EnchantmentLog() : base(Opcode.SMSG_ENCHANTMENT_LOG) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Owner);
        _worldPacket.WritePackedGuid128(Caster);
        _worldPacket.WritePackedGuid128(ItemGUID);
        _worldPacket.WriteInt32(ItemID);
        _worldPacket.WriteInt32(Enchantment);
        _worldPacket.WriteInt32(EnchantSlot);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 3 + 12; // 3 GUIDs + 3 ints

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Owner.Low, Owner.High);
        writer.WritePackedGuid128(Caster.Low, Caster.High);
        writer.WritePackedGuid128(ItemGUID.Low, ItemGUID.High);
        writer.WriteInt32(ItemID);
        writer.WriteInt32(Enchantment);
        writer.WriteInt32(EnchantSlot);
        return writer.Position;
    }

    public WowGuid128 Owner;
    public WowGuid128 Caster;
    public WowGuid128 ItemGUID;
    public int ItemID;
    public int Enchantment;
    public int EnchantSlot = 1;
}

public class CancelTempEnchantment : ClientPacket
{
    public CancelTempEnchantment(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        EnchantmentSlot = _worldPacket.ReadUInt32();
    }

    public uint EnchantmentSlot;
}

public class WrapItem : ClientPacket
{
    public WrapItem(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        _ = _worldPacket.ReadUInt8(); // Unknown Value. Usually 128
        GiftBag = _worldPacket.ReadUInt8();
        GiftSlot = _worldPacket.ReadUInt8();
        ItemBag = _worldPacket.ReadUInt8();
        ItemSlot = _worldPacket.ReadUInt8();
    }

    public byte GiftBag;
    public byte GiftSlot;
    public byte ItemBag;
    public byte ItemSlot;
}

internal static class ItemPacketHelpers
{
    public const int MaxItemMods = 8;
    public const int MaxBonusListIDs = 8;

    // ItemInstance: 4+4+4 fixed + 1 bit flush + ItemModList + optional ItemBonuses
    // ItemModList: 1 byte (6 bits count) + mods * 5
    // ItemBonuses: 1 + 4 + bonuses * 4
    public const int ItemInstanceMaxSize = 12 + 1 + 1 + MaxItemMods * 5 + 1 + 4 + MaxBonusListIDs * 4;

    public static bool WriteItemInstance(ref SpanPacketWriter writer, ItemInstance item)
    {
        writer.WriteUInt32(item.ItemID);
        writer.WriteUInt32(item.RandomPropertiesSeed);
        writer.WriteUInt32(item.RandomPropertiesID);

        writer.WriteBit(item.ItemBonus != null);
        writer.FlushBits();

        // ItemModList
        if (item.Modifications.Values.Count > MaxItemMods)
            return false;

        writer.WriteBits((uint)item.Modifications.Values.Count, 6);
        writer.FlushBits();
        foreach (ItemMod itemMod in item.Modifications.Values)
        {
            writer.WriteUInt32(itemMod.Value);
            writer.WriteUInt8((byte)itemMod.Type);
        }

        // ItemBonuses (optional)
        if (item.ItemBonus != null)
        {
            if (item.ItemBonus.BonusListIDs.Count > MaxBonusListIDs)
                return false;

            writer.WriteUInt8((byte)item.ItemBonus.Context);
            writer.WriteInt32(item.ItemBonus.BonusListIDs.Count);
            foreach (uint bonusID in item.ItemBonus.BonusListIDs)
                writer.WriteUInt32(bonusID);
        }

        return true;
    }
}
