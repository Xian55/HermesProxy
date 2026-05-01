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


using Framework.Constants;
using Framework.GameMath;
using Framework.IO;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HermesProxy.World.Server.Packets;

public class InteractWithNPC : ClientPacket
{
    public InteractWithNPC(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        CreatureGUID = _worldPacket.ReadPackedGuid128();
    }

    public WowGuid128 CreatureGUID;
}

public class GossipMessagePkt : ServerPacket
{
    public GossipMessagePkt() : base(Opcode.SMSG_GOSSIP_MESSAGE) { }

    public override void Write()
    {
        // V3_4_3 (WotLK Classic) uses a distinct on-the-wire shape: TextID is at
        // the END of the packet (not after FriendshipFactionID), per-option fields
        // include a duplicated OptionIndex + an extra reserved Int32 + an extra
        // trailing bit, and there are two leading bits before the options array.
        // Without this, the V3_4_3 client mis-parses the bit cascade and the quest
        // list reads `ConditionalQuestText` as garbage → 5 TB allocation OOM
        // (observed crash: ?AUConditionalQuestText@@, line -6). Layout mirrors
        // HermesProxy-WOTLK's GossipMessagePkt.WriteWotLK exactly.
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            WriteWotLK();
            return;
        }

        _worldPacket.WritePackedGuid128(GossipGUID);
        _worldPacket.WriteInt32(GossipID);
        _worldPacket.WriteInt32(FriendshipFactionID);
        _worldPacket.WriteInt32(TextID);

        _worldPacket.WriteInt32(GossipOptions.Count);
        _worldPacket.WriteInt32(GossipQuests.Count);

        foreach (ClientGossipOption options in GossipOptions)
        {
            _worldPacket.WriteInt32(options.OptionIndex);
            _worldPacket.WriteUInt8(options.OptionIcon);
            _worldPacket.WriteUInt8(options.OptionFlags);
            _worldPacket.WriteInt32(options.OptionCost);
            if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
                _worldPacket.WriteUInt32(options.Language);

            _worldPacket.WriteBits(options.Text.GetByteCount(), 12);
            _worldPacket.WriteBits(options.Confirm.GetByteCount(), 12);
            _worldPacket.WriteBits((byte)options.Status, 2);
            _worldPacket.WriteBit(options.SpellID.HasValue);
            _worldPacket.FlushBits();

            options.Treasure.Write(_worldPacket);

            _worldPacket.WriteString(options.Text);
            _worldPacket.WriteString(options.Confirm);

            if (options.SpellID.HasValue)
                _worldPacket.WriteInt32(options.SpellID.Value);
        }

        foreach (ClientGossipQuest text in GossipQuests)
            text.Write(_worldPacket);
    }

    private void WriteWotLK()
    {
        _worldPacket.WritePackedGuid128(GossipGUID);
        _worldPacket.WriteInt32(GossipID);
        _worldPacket.WriteInt32(FriendshipFactionID);
        _worldPacket.WriteUInt32((uint)GossipOptions.Count);
        _worldPacket.WriteUInt32((uint)GossipQuests.Count);
        _worldPacket.WriteBit(true);
        _worldPacket.WriteBit(false);
        _worldPacket.FlushBits();

        foreach (ClientGossipOption options in GossipOptions)
        {
            _worldPacket.WriteInt32(options.OptionIndex);
            _worldPacket.WriteUInt8(options.OptionIcon);
            _worldPacket.WriteInt8((sbyte)options.OptionFlags);
            _worldPacket.WriteInt32(options.OptionCost);
            _worldPacket.WriteUInt32(options.Language);
            _worldPacket.WriteInt32(0);
            _worldPacket.WriteInt32(options.OptionIndex);
            _worldPacket.WriteBits(options.Text.GetByteCount(), 12);
            _worldPacket.WriteBits(options.Confirm.GetByteCount(), 12);
            _worldPacket.WriteBits((byte)options.Status, 2);
            _worldPacket.WriteBit(options.SpellID.HasValue);
            _worldPacket.WriteBit(false);
            _worldPacket.FlushBits();

            options.Treasure.Write(_worldPacket);

            _worldPacket.WriteString(options.Text);
            _worldPacket.WriteString(options.Confirm);

            if (options.SpellID.HasValue)
                _worldPacket.WriteInt32(options.SpellID.Value);
        }

        _worldPacket.WriteInt32(TextID);

        foreach (ClientGossipQuest quest in GossipQuests)
            quest.WriteWotLK(_worldPacket);
    }

    public List<ClientGossipOption> GossipOptions = new();
    public int FriendshipFactionID;
    public WowGuid128 GossipGUID;
    public List<ClientGossipQuest> GossipQuests = new();
    public int TextID;
    public int GossipID;
}

public class ClientGossipOption
{
    public int OptionIndex;
    public byte OptionIcon;
    public byte OptionFlags;
    public int OptionCost;
    public uint Language;
    public GossipOptionStatus Status;
    public string Text = string.Empty;
    public string Confirm = string.Empty;
    public TreasureLootList Treasure = new();
    public int? SpellID;
}

public class TreasureLootList
{
    public List<TreasureItem> Items = new();

    public void Write(WorldPacket data)
    {
        data.WriteInt32(Items.Count);
        foreach (TreasureItem treasureItem in Items)
            treasureItem.Write(data);
    }
}

public struct TreasureItem
{
    public GossipOptionRewardType Type;
    public int ID;
    public int Quantity;

    public void Write(WorldPacket data)
    {
        data.WriteBits((byte)Type, 1);
        data.WriteInt32(ID);
        data.WriteInt32(Quantity);
    }
}

public class ClientGossipQuest
{
    public uint QuestID;
    public uint ContentTuningID;
    public int QuestType; // 2 not taken, 4 taken
    public int QuestLevel;
    public int QuestMaxLevel = 255;
    public bool Repeatable;
    public string QuestTitle = string.Empty;
    public uint QuestFlags = 8;
    public uint QuestFlagsEx;

    public void Write(WorldPacket data)
    {
        data.WriteUInt32(QuestID);
        data.WriteUInt32(ContentTuningID);
        data.WriteInt32(QuestType);
        data.WriteInt32(QuestLevel);
        data.WriteInt32(QuestMaxLevel);
        data.WriteUInt32(QuestFlags);
        data.WriteUInt32(QuestFlagsEx);

        data.WriteBit(Repeatable);
        data.WriteBits(QuestTitle.GetByteCount(), 9);
        data.FlushBits();

        data.WriteString(QuestTitle);
    }

    // V3_4_3 layout adds an extra reserved bit between Repeatable and the title
    // length. Without it, the V3_4_3 client reads the title length 1 bit shifted
    // and then mis-parses the next field as a ConditionalQuestText length prefix,
    // producing absurd allocation requests (~5 TB) → client OOM crash.
    public void WriteWotLK(WorldPacket data)
    {
        data.WriteInt32((int)QuestID);
        data.WriteInt32((int)ContentTuningID);
        data.WriteInt32(QuestType);
        data.WriteInt32(QuestLevel);
        data.WriteInt32(QuestMaxLevel);
        data.WriteInt32((int)QuestFlags);
        data.WriteInt32((int)QuestFlagsEx);
        data.WriteBit(Repeatable);
        data.WriteBit(false);
        data.WriteBits(QuestTitle.GetByteCount(), 9);
        data.FlushBits();
        data.WriteString(QuestTitle);
    }
}

public class GossipSelectOption : ClientPacket
{
    public GossipSelectOption(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        GossipUnit = _worldPacket.ReadPackedGuid128();
        GossipID = _worldPacket.ReadUInt32();
        GossipIndex = _worldPacket.ReadUInt32();

        uint length = _worldPacket.ReadBits<uint>(8);
        PromotionCode = _worldPacket.ReadString(length);
    }

    public WowGuid128 GossipUnit;
    public uint GossipIndex;
    public uint GossipID;
    public string PromotionCode = string.Empty;
}

public class GossipComplete : ServerPacket, ISpanWritable
{
    public GossipComplete() : base(Opcode.SMSG_GOSSIP_COMPLETE) { }

    public override void Write()
    {
        if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
        {
            _worldPacket.WriteBit(SuppressSound);
            _worldPacket.FlushBits();
        }
    }

    // MaxSize: optional bit (1 byte when flushed)
    public int MaxSize => 1;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
        {
            writer.WriteBit(SuppressSound);
            writer.FlushBits();
        }
        return writer.Position;
    }

    public bool SuppressSound;
}

public class BinderConfirm : ServerPacket, ISpanWritable
{
    public BinderConfirm() : base(Opcode.SMSG_BINDER_CONFIRM) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Guid);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Guid.Low, Guid.High);
        return writer.Position;
    }

    public WowGuid128 Guid;
}

public class VendorInventory : ServerPacket
{
    public VendorInventory() : base(Opcode.SMSG_VENDOR_INVENTORY, ConnectionType.Instance) { }

    public override void Write()
    {
        Log.Print(LogType.Trace,
            $"[VendorTrace] SMSG_VENDOR_INVENTORY write: VendorGUID={VendorGUID} " +
            $"Reason={Reason} Items.Count={Items.Count} layoutPath={(ModernVersion.Build == ClientVersionBuild.V3_4_3_54261 ? "WotLK" : "Vanilla")}");

        _worldPacket.WritePackedGuid128(VendorGUID);
        _worldPacket.WriteUInt8(Reason);
        _worldPacket.WriteInt32(Items.Count);

        for (int i = 0; i < Items.Count; i++)
        {
            VendorItem item = Items[i];
            if (i < 3 || i == Items.Count - 1)
            {
                Log.Print(LogType.Trace,
                    $"[VendorTrace] item[{i}]: Slot={item.Slot} ItemID={item.Item.ItemID} MuID={item.MuID} " +
                    $"Type={item.Type} Quantity={item.Quantity} Price={item.Price} StackCount={item.StackCount} " +
                    $"ExtCost={item.ExtendedCostID} Durability={item.Durability}");
            }
            item.Write(_worldPacket);
        }
    }

    public byte Reason = 0;
    public List<VendorItem> Items = new();
    public WowGuid128 VendorGUID;
}

public class VendorItem
{
    public void Write(WorldPacket data)
    {
        // V3_4_3 reorders the vendor item record and inserts a MuID slot index.
        // Without this layout the client mis-parses the field stream and the
        // vendor window renders empty / corrupted. Layout mirrors
        // HermesProxy-WOTLK Server/Packets/VendorItem.cs:WriteWotLK exactly.
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            WriteWotLK(data);
            return;
        }

        data.WriteInt32(Slot);
        data.WriteInt32(Type);
        data.WriteInt32(Quantity);
        data.WriteUInt64(Price);
        data.WriteInt32(Durability);
        data.WriteUInt32(StackCount);
        data.WriteInt32(ExtendedCostID);
        data.WriteInt32(PlayerConditionFailed);
        Item.Write(data);
        data.WriteBit(DoNotFilterOnVendor);
        data.WriteBit(Refundable);
        data.FlushBits();
    }

    private void WriteWotLK(WorldPacket data)
    {
        data.WriteUInt64(Price);
        data.WriteUInt32(MuID);
        data.WriteInt32(Type);
        data.WriteInt32(Durability);
        data.WriteInt32((int)StackCount);
        data.WriteInt32(Quantity);
        data.WriteInt32(ExtendedCostID);
        data.WriteInt32(PlayerConditionFailed);
        data.WriteBit(false);
        data.WriteBit(DoNotFilterOnVendor);
        data.WriteBit(Refundable);
        data.FlushBits();
        Item.Write(data);
    }

    public int Slot;
    public int Type = 1;
    public ItemInstance Item = new();
    public int Quantity = -1;
    public ulong Price;
    public int Durability;
    public uint StackCount;
    public int ExtendedCostID;
    public int PlayerConditionFailed;
    public bool DoNotFilterOnVendor;
    public bool Refundable;
    public uint MuID;
}

public class ShowBank : ServerPacket, ISpanWritable
{
    public ShowBank() : base(Opcode.SMSG_SHOW_BANK, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Guid);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Guid.Low, Guid.High);
        return writer.Position;
    }

    public WowGuid128 Guid;
}

public class BuyBankSlot : ClientPacket
{
    public BuyBankSlot(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Guid = _worldPacket.ReadPackedGuid128();
    }

    public WowGuid128 Guid;
}

public class TrainerList : ServerPacket, ISpanWritable
{
    public TrainerList() : base(Opcode.SMSG_TRAINER_LIST, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(TrainerGUID);
        _worldPacket.WriteInt32(TrainerType);
        _worldPacket.WriteUInt32(TrainerID);

        _worldPacket.WriteInt32(Spells.Count);
        foreach (TrainerListSpell spell in Spells)
        {
            _worldPacket.WriteUInt32(spell.SpellID);
            _worldPacket.WriteUInt32(spell.MoneyCost);
            _worldPacket.WriteUInt32(spell.ReqSkillLine);
            _worldPacket.WriteUInt32(spell.ReqSkillRank);

            for (uint i = 0; i < 3; ++i)
                _worldPacket.WriteUInt32(spell.ReqAbility[i]);

            _worldPacket.WriteUInt8((byte)spell.Usable);
            _worldPacket.WriteUInt8(spell.ReqLevel);
        }

        _worldPacket.WriteBits(Greeting.GetByteCount(), 11);
        _worldPacket.FlushBits();
        _worldPacket.WriteString(Greeting);
    }

    // MaxSize: GUID(18) + 2 ints(8) + count(4) + max 200 spells (30 each) + bits(2) + greeting(256) = 6288
    // TrainerListSpell: 4 uints(16) + 3 reqAbility(12) + 2 bytes(2) = 30
    private const int MaxSpells = 200;
    private const int SpellSize = 30;
    private const int MaxGreetingBytes = 256;
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 12 + MaxSpells * SpellSize + 2 + MaxGreetingBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        int greetingBytes = Encoding.UTF8.GetByteCount(Greeting ?? "");
        if (Spells.Count > MaxSpells || greetingBytes > 2047) // 11 bits max
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(TrainerGUID.Low, TrainerGUID.High);
        writer.WriteInt32(TrainerType);
        writer.WriteUInt32(TrainerID);

        writer.WriteInt32(Spells.Count);
        foreach (var spell in Spells)
        {
            writer.WriteUInt32(spell.SpellID);
            writer.WriteUInt32(spell.MoneyCost);
            writer.WriteUInt32(spell.ReqSkillLine);
            writer.WriteUInt32(spell.ReqSkillRank);

            for (int i = 0; i < 3; ++i)
                writer.WriteUInt32(spell.ReqAbility[i]);

            writer.WriteUInt8((byte)spell.Usable);
            writer.WriteUInt8(spell.ReqLevel);
        }

        writer.WriteBits((uint)greetingBytes, 11);
        writer.FlushBits();
        writer.WriteString(Greeting ?? "");
        return writer.Position;
    }

    public WowGuid128 TrainerGUID;
    public int TrainerType;
    public uint TrainerID = 1;
    public List<TrainerListSpell> Spells = new();
    public string Greeting = string.Empty;
}

public class TrainerListSpell
{
    public uint SpellID;
    public uint MoneyCost;
    public uint ReqSkillLine;
    public uint ReqSkillRank;
    public uint[] ReqAbility = new uint[3];
    public TrainerSpellStateModern Usable;
    public byte ReqLevel;
}

class TrainerBuySpell : ClientPacket
{
    public TrainerBuySpell(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        TrainerGUID = _worldPacket.ReadPackedGuid128();
        TrainerID = _worldPacket.ReadUInt32();
        SpellID = _worldPacket.ReadUInt32();
    }

    public WowGuid128 TrainerGUID;
    public uint TrainerID;
    public uint SpellID;
}

class TrainerBuyFailed : ServerPacket, ISpanWritable
{
    public TrainerBuyFailed() : base(Opcode.SMSG_TRAINER_BUY_FAILED) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(TrainerGUID);
        _worldPacket.WriteUInt32(SpellID);
        _worldPacket.WriteUInt32(TrainerFailedReason);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 8; // GUID + 2 uints

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(TrainerGUID.Low, TrainerGUID.High);
        writer.WriteUInt32(SpellID);
        writer.WriteUInt32(TrainerFailedReason);
        return writer.Position;
    }

    public WowGuid128 TrainerGUID;
    public uint SpellID;
    public uint TrainerFailedReason;
}

class RespecWipeConfirm : ServerPacket, ISpanWritable
{
    public RespecWipeConfirm() : base(Opcode.SMSG_RESPEC_WIPE_CONFIRM) { }

    public override void Write()
    {
        _worldPacket.WriteInt8((sbyte)RespecType);
        _worldPacket.WriteUInt32(Cost);
        _worldPacket.WritePackedGuid128(TrainerGUID);
    }

    public int MaxSize => 5 + PackedGuidHelper.MaxPackedGuid128Size; // sbyte + uint + GUID

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt8((sbyte)RespecType);
        writer.WriteUInt32(Cost);
        writer.WritePackedGuid128(TrainerGUID.Low, TrainerGUID.High);
        return writer.Position;
    }

    public SpecResetType RespecType = SpecResetType.Talents;
    public uint Cost;
    public WowGuid128 TrainerGUID;
}

class ConfirmRespecWipe : ClientPacket
{
    public ConfirmRespecWipe(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        TrainerGUID = _worldPacket.ReadPackedGuid128();
        RespecType = (SpecResetType)_worldPacket.ReadUInt8();
    }

    public WowGuid128 TrainerGUID;
    public SpecResetType RespecType;
}

class GossipPOI : ServerPacket, ISpanWritable
{
    public GossipPOI() : base(Opcode.SMSG_GOSSIP_POI) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(Id);
        _worldPacket.WriteFloat(Pos.X);
        _worldPacket.WriteFloat(Pos.Y);
        _worldPacket.WriteFloat(Pos.Z);
        _worldPacket.WriteUInt32(Icon);
        _worldPacket.WriteUInt32(Importance);
        _worldPacket.WriteUInt32(Unknown905);
        _worldPacket.WriteBits(Flags, 14);
        _worldPacket.WriteBits(Name.GetByteCount(), 6);
        _worldPacket.FlushBits();
        _worldPacket.WriteString(Name);
    }

    // Cap for POI name - limited by 6 bits = 64 bytes max
    private const int MaxNameBytes = 64;
    // 4 uint(16) + 3 floats(12) + 3 bytes for bits + name
    public int MaxSize => 16 + 12 + 3 + MaxNameBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        int nameBytes = Encoding.UTF8.GetByteCount(Name);
        if (nameBytes > MaxNameBytes)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(Id);
        writer.WriteFloat(Pos.X);
        writer.WriteFloat(Pos.Y);
        writer.WriteFloat(Pos.Z);
        writer.WriteUInt32(Icon);
        writer.WriteUInt32(Importance);
        writer.WriteUInt32(Unknown905);
        writer.WriteBits(Flags, 14);
        writer.WriteBits((uint)nameBytes, 6);
        writer.FlushBits();
        writer.WriteString(Name);
        return writer.Position;
    }

    public uint Id = 1;
    public uint Flags;
    public Vector3 Pos;
    public uint Icon;
    public uint Importance;
    public uint Unknown905;
    public string Name = string.Empty;
}

public class SpiritHealerConfirm : ServerPacket, ISpanWritable
{
    public SpiritHealerConfirm() : base(Opcode.SMSG_SPIRIT_HEALER_CONFIRM) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Guid);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Guid.Low, Guid.High);
        return writer.Position;
    }

    public WowGuid128 Guid;
}
