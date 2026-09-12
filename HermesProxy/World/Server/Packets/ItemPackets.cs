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
using System.Runtime.CompilerServices;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
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

/// <summary>Data only — parsing lives in <c>BuyBackItemCodec</c>, behaviour in <c>ItemSystem</c>.</summary>
public readonly record struct BuyBackItem(WowGuid128 VendorGUID, uint Slot);

/// <param name="MuID">
/// The 1-based vendor slot index from SMSG_VENDOR_INVENTORY. Sent only by V3_4_3, which is
/// what the legacy CMSG_BUY_ITEM wants in its slot field.
/// </param>
public readonly record struct BuyItem(
    WowGuid128 VendorGUID,
    WowGuid128 ContainerGUID,
    uint Quantity,
    uint MuID,
    uint Slot,
    uint BagSlot,
    ItemVendorType ItemType,
    ItemInstance Item);

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

public readonly record struct SellItem(WowGuid128 VendorGUID, WowGuid128 ItemGUID, uint Amount);

public class SellResponse : ServerPacket, ISpanWritable
{
    public SellResponse() : base(Opcode.SMSG_SELL_RESPONSE) { }

    public override void Write()
    {
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            _worldPacket.WritePackedGuid128(VendorGUID);
            _worldPacket.WriteUInt32(1);
            _worldPacket.WriteInt32(Reason);
            _worldPacket.WritePackedGuid128(ItemGUID);
        }
        else
        {
            _worldPacket.WritePackedGuid128(VendorGUID);
            _worldPacket.WritePackedGuid128(ItemGUID);
            _worldPacket.WriteUInt8((byte)Reason);
        }
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2 + 8; // 2 GUIDs + max(uint32 count + int32 reason, uint8 reason)

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            writer.WritePackedGuid128(VendorGUID.Low, VendorGUID.High);
            writer.WriteUInt32(1);
            writer.WriteInt32(Reason);
            writer.WritePackedGuid128(ItemGUID.Low, ItemGUID.High);
        }
        else
        {
            writer.WritePackedGuid128(VendorGUID.Low, VendorGUID.High);
            writer.WritePackedGuid128(ItemGUID.Low, ItemGUID.High);
            writer.WriteUInt8((byte)Reason);
        }
        return writer.Position;
    }

    public WowGuid128 VendorGUID;
    public WowGuid128 ItemGUID;
    public int Reason;
}

public readonly record struct SplitItem(
    InvUpdate Inv,
    byte FromPackSlot,
    byte FromSlot,
    byte ToPackSlot,
    byte ToSlot,
    int Quantity);

/// <param name="Slot1">Source slot.</param>
/// <param name="Slot2">Destination slot — but see the handler: V3_4_3 packs them the other way round.</param>
public readonly record struct SwapInvItem(InvUpdate Inv, byte Slot2, byte Slot1);

public readonly record struct SwapItem(
    InvUpdate Inv,
    byte ContainerSlotB,
    byte ContainerSlotA,
    byte SlotB,
    byte SlotA);

public readonly record struct AutoEquipItem(InvUpdate Inv, byte PackSlot, byte Slot);

public readonly record struct AutoStoreBagItem(
    InvUpdate Inv,
    byte ContainerSlotA,
    byte ContainerSlotB,
    byte SlotA);

public readonly record struct AutoEquipItemSlot(InvUpdate Inv, WowGuid128 Item, byte ItemDstSlot);

/// <summary>
/// The inventory-change preamble the modern client prefixes to every item move.
/// </summary>
/// <remarks>
/// No handler reads it — it is consumed to stay aligned with the fields that follow — but it is
/// carried faithfully rather than skipped, so a layout change shows up as a field mismatch
/// instead of a silent offset. The count is two bits, so at most three entries: an inline array
/// keeps the whole thing on the stack, which matters because this rides along with every drag.
/// </remarks>
public readonly record struct InvUpdate(byte Count, InvItems Items)
{
    /// <summary>The count is read from two bits, so it can never exceed this.</summary>
    public const int MaxItems = 3;
}

public readonly record struct InvItem(byte ContainerSlot, byte Slot);

/// <summary>Storage for <see cref="InvUpdate.Items"/>; the 2-bit count caps it at three.</summary>
[InlineArray(InvUpdate.MaxItems)]
public struct InvItems
{
    private InvItem _element0;
}

public readonly record struct DestroyItem(uint Count, byte ContainerId, byte SlotNum);

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

    /// <inheritdoc cref="Read(WorldPacket)"/>
    public void Read(ref SpanPacketReader data)
    {
        ItemID = data.ReadUInt32();
        RandomPropertiesSeed = data.ReadUInt32();
        RandomPropertiesID = data.ReadUInt32();

        if (data.HasBit())
            ItemBonus = new();
        data.ResetBitPos();

        Modifications.Read(ref data);

        if (ItemBonus != null)
            ItemBonus.Read(ref data);
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

    /// <inheritdoc cref="Read(WorldPacket)"/>
    public void Read(ref SpanPacketReader data)
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

    /// <inheritdoc cref="Read(WorldPacket)"/>
    /// <remarks>Generated from the WorldPacket reader; ItemInstanceReaderEquivalenceTests keeps the pair in step.</remarks>
    public void Read(ref SpanPacketReader data)
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

    /// <inheritdoc cref="Read(WorldPacket)"/>
    public void Read(ref SpanPacketReader data)
    {
        var itemModListCount = data.ReadBits<uint>(6);
        data.ResetBitPos();

        for (var i = 0; i < itemModListCount; ++i)
        {
            var itemMod = new ItemMod();
            itemMod.Read(ref data);
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

public readonly record struct ReadItem(byte PackSlot, byte Slot);

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

public class InventoryChangeFailure : ServerPacket, ISpanWritable
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melServer =
        Framework.Logging.Log.CreateMelLogger(Framework.Logging.Log.CategoryServer);
    // Matches the column the [CallerFilePath] form used to render, so existing greps still work.
    private static readonly string _logSource = "ItemPackets".PadRight(15);

    public InventoryChangeFailure() : base(Opcode.SMSG_INVENTORY_CHANGE_FAILURE) { }

    public override void Write()
    {
        ItemLogMessages.InventoryChangeFailureWrite(_melServer, _logSource, "write", BagResult, (int)BagResult,
            Item[0].Low, Item[1].Low, ContainerBSlot, Level, LimitCategory);

        // BagResult width is version-dependent: V3_4_3 reads it as Int32, while
        // 1.14.x / 2.5.x read a single byte. Writing the wrong width shifts every
        // subsequent field, so Item[0]/[1] and ContainerBSlot decode as garbage and
        // the inventory UI leaves the slot greyed out until the player relogs.
        if (ModernVersion.ExpansionVersion >= 3)
            _worldPacket.WriteInt32((int)BagResult);
        else
            _worldPacket.WriteInt8((sbyte)BagResult);
        _worldPacket.WritePackedGuid128(Item[0]);
        _worldPacket.WritePackedGuid128(Item[1]);
        _worldPacket.WriteUInt8(ContainerBSlot); // bag type subclass, used with EQUIP_ERR_EVENT_AUTOEQUIP_BIND_CONFIRM and EQUIP_ERR_WRONG_BAG_TYPE_2

        switch (BagResult)
        {
            case InventoryResult.CantEquipLevel:
            case InventoryResult.PurchaseLevelTooLow:
                _worldPacket.WriteInt32(Level);
                break;
            case InventoryResult.EventAutoEquipBindConfirm:
                _worldPacket.WritePackedGuid128(SrcContainer);
                _worldPacket.WriteInt32(SrcSlot);
                _worldPacket.WritePackedGuid128(DstContainer);
                break;
            case InventoryResult.ItemMaxLimitCategoryCountExceeded:
            case InventoryResult.ItemMaxLimitCategorySocketedExceeded:
            case InventoryResult.ItemMaxLimitCategoryEquippedExceeded:
                _worldPacket.WriteInt32(LimitCategory);
                break;
        }
    }

    // Fixed: int32 (worst case; 1.14/2.5 write 1 byte) + 2 GUIDs + byte = 5 + 36 = 41
    // Max additional (EventAutoEquipBindConfirm): 2 GUIDs + int = 40
    public int MaxSize => 5 + PackedGuidHelper.MaxPackedGuid128Size * 2 +
                          PackedGuidHelper.MaxPackedGuid128Size * 2 + 4;

    public int WriteToSpan(Span<byte> buffer)
    {
        ItemLogMessages.InventoryChangeFailureWrite(_melServer, _logSource, "span-write", BagResult, (int)BagResult,
            Item[0].Low, Item[1].Low, ContainerBSlot, Level, LimitCategory);

        var writer = new SpanPacketWriter(buffer);
        if (ModernVersion.ExpansionVersion >= 3)
            writer.WriteInt32((int)BagResult);
        else
            writer.WriteInt8((sbyte)BagResult);
        writer.WritePackedGuid128(Item[0].Low, Item[0].High);
        writer.WritePackedGuid128(Item[1].Low, Item[1].High);
        writer.WriteUInt8(ContainerBSlot);

        switch (BagResult)
        {
            case InventoryResult.CantEquipLevel:
            case InventoryResult.PurchaseLevelTooLow:
                writer.WriteInt32(Level);
                break;
            case InventoryResult.EventAutoEquipBindConfirm:
                writer.WritePackedGuid128(SrcContainer.Low, SrcContainer.High);
                writer.WriteInt32(SrcSlot);
                writer.WritePackedGuid128(DstContainer.Low, DstContainer.High);
                break;
            case InventoryResult.ItemMaxLimitCategoryCountExceeded:
            case InventoryResult.ItemMaxLimitCategorySocketedExceeded:
            case InventoryResult.ItemMaxLimitCategoryEquippedExceeded:
                writer.WriteInt32(LimitCategory);
                break;
        }

        return writer.Position;
    }

    public InventoryResult BagResult;
    public byte ContainerBSlot;
    public WowGuid128 SrcContainer;
    public WowGuid128 DstContainer;
    public int SrcSlot;
    public int LimitCategory;
    public int Level;
    public WowGuid128[] Item = new WowGuid128[2];
}

public readonly record struct RepairItem(WowGuid128 VendorGUID, WowGuid128 ItemGUID, bool UseGuildBank);

public readonly record struct SocketGems(WowGuid128 ItemGuid, GemSockets Gems);

/// <summary>Storage for <see cref="SocketGems.Gems"/>, one entry per socket.</summary>
[InlineArray(ItemConst.MaxGemSockets)]
public struct GemSockets
{
    private WowGuid128 _element0;
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

public readonly record struct OpenItem(byte PackSlot, byte Slot);

public readonly record struct SetAmmo(uint ItemId);

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
    public int EnchantSlot;
}

public readonly record struct CancelTempEnchantment(uint EnchantmentSlot);

public readonly record struct WrapItem(byte GiftBag, byte GiftSlot, byte ItemBag, byte ItemSlot);

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
