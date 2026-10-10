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

public readonly record struct InteractWithNPC(WowGuid128 CreatureGUID);

public sealed class GossipMessagePkt : ServerPacket
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<GossipMessagePkt>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V3_4_3_54261, new ClassicEraLayout()),
        (ClientVersionBuild.V3_4_3_54261, ClientVersionBuild.V4_4_2_60895, new WotLKClassicLayout()),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new CataClassicLayout()));

    private static readonly ServerPacketLayout<GossipMessagePkt> Layout = Layouts.ForRunningClient();

    public GossipMessagePkt() : base(Opcode.SMSG_GOSSIP_MESSAGE) { }

    public override void Write() => Layout.Write(this, _worldPacket);

    /// <summary>1.14 and 2.5.</summary>
    internal sealed class ClassicEraLayout : ServerPacketLayout<GossipMessagePkt>
    {
        public override void Write(GossipMessagePkt packet, WorldPacket data)
        {
            data.WritePackedGuid128(packet.GossipGUID);
            data.WriteInt32(packet.GossipID);
            data.WriteInt32(packet.FriendshipFactionID);
            data.WriteInt32(packet.TextID);

            data.WriteInt32(packet.GossipOptions.Count);
            data.WriteInt32(packet.GossipQuests.Count);

            foreach (ClientGossipOption options in packet.GossipOptions)
            {
                data.WriteInt32(options.OptionIndex);
                data.WriteUInt8(options.OptionIcon);
                data.WriteUInt8(options.OptionFlags);
                data.WriteInt32(options.OptionCost);
                if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
                    data.WriteUInt32(options.Language);

                data.WriteBits(options.Text.GetByteCount(), 12);
                data.WriteBits(options.Confirm.GetByteCount(), 12);
                data.WriteBits((byte)options.Status, 2);
                data.WriteBit(options.SpellID.HasValue);
                data.FlushBits();

                options.Treasure.Write(data);

                data.WriteString(options.Text);
                data.WriteString(options.Confirm);

                if (options.SpellID.HasValue)
                    data.WriteInt32(options.SpellID.Value);
            }

            foreach (ClientGossipQuest text in packet.GossipQuests)
                text.Write(data);
        }
    }

    /// <summary>
    /// 3.4.3 (WotLK Classic) uses a distinct on-the-wire shape: TextID is at the END of the
    /// packet (not after FriendshipFactionID), per-option fields include a duplicated
    /// OptionIndex + an extra reserved Int32 + an extra trailing bit, and there are two leading
    /// bits before the options array. Without this, the V3_4_3 client mis-parses the bit cascade
    /// and the quest list reads `ConditionalQuestText` as garbage → 5 TB allocation OOM
    /// (observed crash: ?AUConditionalQuestText@@, line -6). Layout mirrors HermesProxy-WOTLK's
    /// GossipMessagePkt.WriteWotLK exactly.
    /// </summary>
    internal sealed class WotLKClassicLayout : ServerPacketLayout<GossipMessagePkt>
    {
        public override void Write(GossipMessagePkt packet, WorldPacket data)
        {
            data.WritePackedGuid128(packet.GossipGUID);
            data.WriteInt32(packet.GossipID);
            data.WriteInt32(packet.FriendshipFactionID);
            data.WriteUInt32((uint)packet.GossipOptions.Count);
            data.WriteUInt32((uint)packet.GossipQuests.Count);
            data.WriteBit(true);
            data.WriteBit(false);
            data.FlushBits();

            foreach (ClientGossipOption options in packet.GossipOptions)
            {
                data.WriteInt32(options.OptionIndex);
                data.WriteUInt8(options.OptionIcon);
                data.WriteInt8((sbyte)options.OptionFlags);
                data.WriteInt32(options.OptionCost);
                data.WriteUInt32(options.Language);
                data.WriteInt32(0);
                data.WriteInt32(options.OptionIndex);
                data.WriteBits(options.Text.GetByteCount(), 12);
                data.WriteBits(options.Confirm.GetByteCount(), 12);
                data.WriteBits((byte)options.Status, 2);
                data.WriteBit(options.SpellID.HasValue);
                data.WriteBit(false);
                data.FlushBits();

                options.Treasure.Write(data);

                data.WriteString(options.Text);
                data.WriteString(options.Confirm);

                if (options.SpellID.HasValue)
                    data.WriteInt32(options.SpellID.Value);
            }

            data.WriteInt32(packet.TextID);

            foreach (ClientGossipQuest quest in packet.GossipQuests)
                quest.WriteWotLK(data);
        }
    }

    /// <summary>
    /// Cataclysm Classic 4.4.2 (TrinityCore cata_classic NPCPackets.cpp, and the client's own
    /// reader): the 3.4.3 shape plus LfgDungeonsID after GossipID; per option an 8-bit
    /// FailureDescription length (+1) after the OverrideIconID bit and a trailing ItemContext
    /// byte on every treasure item; per quest Unused1102, a third QuestFlags word and the
    /// ResetByScheduler and Meta bits.
    /// </summary>
    internal sealed class CataClassicLayout : ServerPacketLayout<GossipMessagePkt>
    {
        public override void Write(GossipMessagePkt packet, WorldPacket data)
        {
            data.WritePackedGuid128(packet.GossipGUID);
            data.WriteInt32(packet.GossipID);
            data.WriteInt32(0);                         // LfgDungeonsID
            data.WriteInt32(packet.FriendshipFactionID);
            data.WriteUInt32((uint)packet.GossipOptions.Count);
            data.WriteUInt32((uint)packet.GossipQuests.Count);
            data.WriteBit(true);                        // TextID present
            data.WriteBit(false);                       // BroadcastTextID present
            data.FlushBits();

            foreach (ClientGossipOption options in packet.GossipOptions)
            {
                data.WriteInt32(options.OptionIndex);   // GossipOptionID
                data.WriteUInt8(options.OptionIcon);    // OptionNPC
                data.WriteInt8((sbyte)options.OptionFlags);
                data.WriteInt32(options.OptionCost);
                data.WriteUInt32(options.Language);
                data.WriteInt32(0);                     // Flags
                data.WriteInt32(options.OptionIndex);   // OrderIndex
                data.WriteBits(options.Text.GetByteCount(), 12);
                data.WriteBits(options.Confirm.GetByteCount(), 12);
                data.WriteBits((byte)options.Status, 2);
                data.WriteBit(options.SpellID.HasValue);
                data.WriteBit(false);                   // OverrideIconID present
                data.WriteBits(1, 8);                   // FailureDescription length + 1: none
                data.FlushBits();

                options.Treasure.WriteCataClassic(data);

                data.WriteString(options.Text);
                data.WriteString(options.Confirm);

                if (options.SpellID.HasValue)
                    data.WriteInt32(options.SpellID.Value);
            }

            data.WriteInt32(packet.TextID);

            foreach (ClientGossipQuest quest in packet.GossipQuests)
                quest.WriteCataClassic(data);
        }
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

    public void WriteCataClassic(WorldPacket data)
    {
        data.WriteInt32(Items.Count);
        foreach (TreasureItem treasureItem in Items)
            treasureItem.WriteCataClassic(data);
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

    // 4.4.2 appends the item context.
    public void WriteCataClassic(WorldPacket data)
    {
        Write(data);
        data.WriteInt8(0);                          // ItemContext
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

    // V3_4_3.54261 wire layout (WPP V3_4_0 ReadGossipQuestTextData range
    // V3_4_3_51505 → V3_4_4_59817). Two bits before the bits9 title length:
    // Repeatable and Important. Newer V3_4_4+ adds Unused1102, QuestFlags[2],
    // ResetByScheduler/Meta — those are NOT present in build 54261.
    public void WriteWotLK(WorldPacket data)
    {
        data.WriteInt32((int)QuestID);
        data.WriteInt32((int)ContentTuningID);
        data.WriteInt32(QuestType);
        data.WriteInt32(QuestLevel);
        data.WriteInt32(QuestMaxLevel);     // QuestMaxScalingLevel
        data.WriteInt32((int)QuestFlags);
        data.WriteInt32((int)QuestFlagsEx);
        data.WriteBit(Repeatable);
        data.WriteBit(false);               // Important
        data.WriteBits(QuestTitle.GetByteCount(), 9);
        data.FlushBits();
        data.WriteString(QuestTitle);
    }

    // 4.4.2 (TrinityCore cata_classic ClientGossipText): Unused1102 after the max scaling level,
    // a third QuestFlags word, and ResetByScheduler / Meta around Important.
    public void WriteCataClassic(WorldPacket data)
    {
        data.WriteInt32((int)QuestID);
        data.WriteInt32((int)ContentTuningID);
        data.WriteInt32(QuestType);
        data.WriteInt32(QuestLevel);
        data.WriteInt32(QuestMaxLevel);     // QuestMaxScalingLevel
        data.WriteInt32(0);                 // Unused1102
        data.WriteInt32((int)QuestFlags);
        data.WriteInt32((int)QuestFlagsEx);
        data.WriteInt32(0);                 // QuestFlags[2]
        data.WriteBit(Repeatable);
        data.WriteBit(false);               // ResetByScheduler
        data.WriteBit(false);               // Important
        data.WriteBit(false);               // Meta
        data.WriteBits(QuestTitle.GetByteCount(), 9);
        data.FlushBits();
        data.WriteString(QuestTitle);
    }
}

public readonly record struct GossipSelectOption(
    WowGuid128 GossipUnit,
    uint GossipID,
    uint GossipIndex,
    string PromotionCode);

public class GossipComplete : ServerPacket, ISpanWritable
{
    public GossipComplete() : base(Opcode.SMSG_GOSSIP_COMPLETE) { }

    public override void Write()
    {
        if (ModernVersion.IsWotLKClassicOrLater
            || ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
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
        if (ModernVersion.IsWotLKClassicOrLater
            || ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
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
        // V3_4_3 wire-opcode 10378 is SMSG_NPC_INTERACTION_OPEN_RESULT
        // (Guid + Int32 InteractionType + bit Success). Same shape as
        // ShowBank / SpiritHealerConfirm. WPP V3_4_3_51666 has no
        // SMSG_BINDER_CONFIRM; type Binder (20) opens the innkeeper dialog.
        _worldPacket.WritePackedGuid128(Guid);
        if (ModernVersion.IsWotLKClassicOrLater)
        {
            _worldPacket.WriteInt32((int)PlayerInteractionType.Binder);
            _worldPacket.WriteBit(true);
            _worldPacket.FlushBits();
        }
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size
        + (ModernVersion.IsWotLKClassicOrLater ? 5 : 0);

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Guid.Low, Guid.High);
        if (ModernVersion.IsWotLKClassicOrLater)
        {
            writer.WriteInt32((int)PlayerInteractionType.Binder);
            writer.WriteBit(true);
            writer.FlushBits();
        }
        return writer.Position;
    }

    public WowGuid128 Guid;
}

public sealed class VendorInventory : ServerPacket
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<VendorInventory>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V3_4_3_54261, new ClassicEraLayout()),
        (ClientVersionBuild.V3_4_3_54261, ClientVersionBuild.V4_4_2_60895, new WotLKClassicLayout()),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new CataClassicLayout()));

    private static readonly ServerPacketLayout<VendorInventory> Layout = Layouts.ForRunningClient();

    public VendorInventory() : base(Opcode.SMSG_VENDOR_INVENTORY, ConnectionType.Instance) { }

    public override void Write()
    {
        Log.Print(LogType.Trace,
            $"[VendorTrace] SMSG_VENDOR_INVENTORY write: VendorGUID={VendorGUID} " +
            $"Reason={Reason} Items.Count={Items.Count}");

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
        }

        Layout.Write(this, _worldPacket);
    }

    /// <summary>1.14 and 2.5.</summary>
    internal sealed class ClassicEraLayout : ServerPacketLayout<VendorInventory>
    {
        public override void Write(VendorInventory packet, WorldPacket data)
        {
            data.WritePackedGuid128(packet.VendorGUID);
            data.WriteUInt8(packet.Reason);
            data.WriteInt32(packet.Items.Count);

            foreach (VendorItem item in packet.Items)
                item.WriteClassicEra(data);
        }
    }

    /// <summary>3.4.3.</summary>
    internal sealed class WotLKClassicLayout : ServerPacketLayout<VendorInventory>
    {
        public override void Write(VendorInventory packet, WorldPacket data)
        {
            data.WritePackedGuid128(packet.VendorGUID);
            data.WriteUInt8(packet.Reason);
            data.WriteInt32(packet.Items.Count);

            foreach (VendorItem item in packet.Items)
                item.WriteWotLKClassic(data);
        }
    }

    /// <summary>4.4.2 (TrinityCore cata_classic, and the client's reader): Reason is an int32.</summary>
    internal sealed class CataClassicLayout : ServerPacketLayout<VendorInventory>
    {
        public override void Write(VendorInventory packet, WorldPacket data)
        {
            data.WritePackedGuid128(packet.VendorGUID);
            data.WriteInt32(packet.Reason);
            data.WriteInt32(packet.Items.Count);

            foreach (VendorItem item in packet.Items)
                item.WriteCataClassic(data);
        }
    }

    public byte Reason = 0;
    public List<VendorItem> Items = new();
    public WowGuid128 VendorGUID;
}

public class VendorItem
{
    public void WriteClassicEra(WorldPacket data)
    {
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

    // V3_4_3 reorders the vendor item record and inserts a MuID slot index.
    // Without this layout the client mis-parses the field stream and the
    // vendor window renders empty / corrupted. Layout mirrors
    // HermesProxy-WOTLK Server/Packets/VendorItem.cs:WriteWotLK exactly.
    public void WriteWotLKClassic(WorldPacket data)
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

    // 4.4.2 dropped Durability.
    public void WriteCataClassic(WorldPacket data)
    {
        data.WriteUInt64(Price);
        data.WriteUInt32(MuID);
        data.WriteInt32(Type);
        data.WriteInt32((int)StackCount);
        data.WriteInt32(Quantity);
        data.WriteInt32(ExtendedCostID);
        data.WriteInt32(PlayerConditionFailed);
        data.WriteBit(false);                       // Locked
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
        // V3_4_3 wire-opcode 10378 is actually SMSG_NPC_INTERACTION_OPEN_RESULT
        // (Guid + Int32 InteractionType + bit Success). Without the type+success
        // tail the client reads InteractionType=None and the bank UI never opens.
        _worldPacket.WritePackedGuid128(Guid);
        if (ModernVersion.IsWotLKClassicOrLater)
        {
            _worldPacket.WriteInt32((int)PlayerInteractionType.Banker);
            _worldPacket.WriteBit(true);
            _worldPacket.FlushBits();
        }
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size
        + (ModernVersion.IsWotLKClassicOrLater ? 5 : 0);

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Guid.Low, Guid.High);
        if (ModernVersion.IsWotLKClassicOrLater)
        {
            writer.WriteInt32((int)PlayerInteractionType.Banker);
            writer.WriteBit(true);
            writer.FlushBits();
        }
        return writer.Position;
    }

    public WowGuid128 Guid;
}

public readonly record struct BuyBankSlot(WowGuid128 Guid);

public sealed class TrainerList : ServerPacket, ISpanWritable
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<TrainerList>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V4_4_2_60895, new SpellsLayout(unk440: false)),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new SpellsLayout(unk440: true)));

    private static readonly ServerPacketLayout<TrainerList> Layout = Layouts.ForRunningClient();

    public TrainerList() : base(Opcode.SMSG_TRAINER_LIST, ConnectionType.Instance) { }

    public override void Write() => Layout.Write(this, _worldPacket);

    // MaxSize: GUID(18) + 2 ints(8) + count(4) + max 200 spells (34 each) + bits(2) + greeting(256) = 7088
    // TrainerListSpell: 4 uints(16) + 3 reqAbility(12) + 4.4.2's Unk440(4) + 2 bytes(2) = 34
    private const int MaxSpells = 200;
    private const int SpellSize = 34;
    private const int MaxGreetingBytes = 256;
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 12 + MaxSpells * SpellSize + 2 + MaxGreetingBytes;

    public int WriteToSpan(Span<byte> buffer) => Layout.WriteToSpan(this, buffer);

    /// <summary>4.4.2 (TrinityCore cata_classic, and the client's reader) adds a uint32 after ReqAbility.</summary>
    internal sealed class SpellsLayout(bool unk440) : ServerPacketLayout<TrainerList>
    {
        public override void Write(TrainerList packet, WorldPacket data)
        {
            data.WritePackedGuid128(packet.TrainerGUID);
            data.WriteInt32(packet.TrainerType);
            data.WriteUInt32(packet.TrainerID);

            data.WriteInt32(packet.Spells.Count);
            foreach (TrainerListSpell spell in packet.Spells)
            {
                data.WriteUInt32(spell.SpellID);
                data.WriteUInt32(spell.MoneyCost);
                data.WriteUInt32(spell.ReqSkillLine);
                data.WriteUInt32(spell.ReqSkillRank);

                for (uint i = 0; i < 3; ++i)
                    data.WriteUInt32(spell.ReqAbility[i]);

                if (unk440)
                    data.WriteUInt32(0);

                data.WriteUInt8((byte)spell.Usable);
                data.WriteUInt8(spell.ReqLevel);
            }

            data.WriteBits(packet.Greeting.GetByteCount(), 11);
            data.FlushBits();
            data.WriteString(packet.Greeting);
        }

        public override int WriteToSpan(TrainerList packet, Span<byte> buffer)
        {
            int greetingBytes = Encoding.UTF8.GetByteCount(packet.Greeting ?? "");
            if (packet.Spells.Count > MaxSpells || greetingBytes > 2047) // 11 bits max
                return -1;

            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(packet.TrainerGUID.Low, packet.TrainerGUID.High);
            writer.WriteInt32(packet.TrainerType);
            writer.WriteUInt32(packet.TrainerID);

            writer.WriteInt32(packet.Spells.Count);
            foreach (var spell in packet.Spells)
            {
                writer.WriteUInt32(spell.SpellID);
                writer.WriteUInt32(spell.MoneyCost);
                writer.WriteUInt32(spell.ReqSkillLine);
                writer.WriteUInt32(spell.ReqSkillRank);

                for (int i = 0; i < 3; ++i)
                    writer.WriteUInt32(spell.ReqAbility[i]);

                if (unk440)
                    writer.WriteUInt32(0);

                writer.WriteUInt8((byte)spell.Usable);
                writer.WriteUInt8(spell.ReqLevel);
            }

            writer.WriteBits((uint)greetingBytes, 11);
            writer.FlushBits();
            writer.WriteString(packet.Greeting ?? "");
            return writer.Position;
        }
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

public readonly record struct TrainerBuySpell(WowGuid128 TrainerGUID, uint TrainerID, uint SpellID);

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

public readonly record struct ConfirmRespecWipe(WowGuid128 TrainerGUID, SpecResetType RespecType);

sealed class GossipPOI : ServerPacket, ISpanWritable
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<GossipPOI>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V3_4_3_54261, new RetailLayout()),
        (ClientVersionBuild.V3_4_3_54261, ClientVersionBuild.Zero, new FlatLayout()));

    private static readonly ServerPacketLayout<GossipPOI> Layout = Layouts.ForRunningClient();

    public GossipPOI() : base(Opcode.SMSG_GOSSIP_POI) { }

    public override void Write() => Layout.Write(this, _worldPacket);

    public int MaxSize => Layout.MaxSize;

    public int WriteToSpan(Span<byte> buffer) => Layout.WriteToSpan(this, buffer);

    // Cap for POI name - limited by 6 bits = 64 bytes max
    private const int MaxNameBytes = 64;

    public uint Id = 1;
    public uint Flags;
    public Vector3 Pos;
    public uint Icon;
    public uint Importance;
    public uint Unknown905;
    public string Name = string.Empty;

    /// <summary>1.14 and 2.5: Flags is a 14-bit field after the fixed part.</summary>
    internal sealed class RetailLayout : ServerPacketLayout<GossipPOI>
    {
        // 7 uint(32) + 20 bits (Flags + name length) flushed into 3 bytes + name
        public override int MaxSize => 28 + 3 + MaxNameBytes;

        public override void Write(GossipPOI packet, WorldPacket data)
        {
            data.WriteUInt32(packet.Id);
            data.WriteFloat(packet.Pos.X);
            data.WriteFloat(packet.Pos.Y);
            data.WriteFloat(packet.Pos.Z);
            data.WriteUInt32(packet.Icon);
            data.WriteUInt32(packet.Importance);
            data.WriteUInt32(packet.Unknown905);
            data.WriteBits(packet.Flags, 14);
            data.WriteBits(packet.Name.GetByteCount(), 6);
            data.FlushBits();
            data.WriteString(packet.Name);
        }

        public override int WriteToSpan(GossipPOI packet, Span<byte> buffer)
        {
            int nameBytes = Encoding.UTF8.GetByteCount(packet.Name);
            if (nameBytes > MaxNameBytes)
                return -1;

            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32(packet.Id);
            writer.WriteFloat(packet.Pos.X);
            writer.WriteFloat(packet.Pos.Y);
            writer.WriteFloat(packet.Pos.Z);
            writer.WriteUInt32(packet.Icon);
            writer.WriteUInt32(packet.Importance);
            writer.WriteUInt32(packet.Unknown905);
            writer.WriteBits(packet.Flags, 14);
            writer.WriteBits((uint)nameBytes, 6);
            writer.FlushBits();
            writer.WriteString(packet.Name);
            return writer.Position;
        }
    }

    /// <summary>
    /// 3.4.3 on: Flags is a full uint32 in field position 2, not a 14-bit field at the end. The
    /// retail layout shifts the whole packet and the client silently drops the POI. Matches
    /// TrinityCore 3.4.3 GossipPOI::Write, and the 4.4.2 client reads the same layout.
    /// </summary>
    internal sealed class FlatLayout : ServerPacketLayout<GossipPOI>
    {
        // 8 uint(32) + 1 byte for the flushed 6-bit name length + name
        public override int MaxSize => 32 + 1 + MaxNameBytes;

        public override void Write(GossipPOI packet, WorldPacket data)
        {
            data.WriteUInt32(packet.Id);
            data.WriteUInt32(packet.Flags);
            data.WriteFloat(packet.Pos.X);
            data.WriteFloat(packet.Pos.Y);
            data.WriteFloat(packet.Pos.Z);
            data.WriteUInt32(packet.Icon);
            data.WriteUInt32(packet.Importance);
            data.WriteUInt32(packet.Unknown905);
            data.WriteBits(packet.Name.GetByteCount(), 6);
            data.FlushBits();
            data.WriteString(packet.Name);
        }

        public override int WriteToSpan(GossipPOI packet, Span<byte> buffer)
        {
            int nameBytes = Encoding.UTF8.GetByteCount(packet.Name);
            if (nameBytes > MaxNameBytes)
                return -1;

            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32(packet.Id);
            writer.WriteUInt32(packet.Flags);
            writer.WriteFloat(packet.Pos.X);
            writer.WriteFloat(packet.Pos.Y);
            writer.WriteFloat(packet.Pos.Z);
            writer.WriteUInt32(packet.Icon);
            writer.WriteUInt32(packet.Importance);
            writer.WriteUInt32(packet.Unknown905);
            writer.WriteBits((uint)nameBytes, 6);
            writer.FlushBits();
            writer.WriteString(packet.Name);
            return writer.Position;
        }
    }
}

public class SpiritHealerConfirm : ServerPacket, ISpanWritable
{
    public SpiritHealerConfirm() : base(Opcode.SMSG_SPIRIT_HEALER_CONFIRM) { }

    public override void Write()
    {
        // V3_4_3 reuses wire opcode 10378 (SMSG_NPC_INTERACTION_OPEN_RESULT):
        // Guid + Int32 InteractionType + bit Success. Without the type=SpiritHealer
        // + success tail the client reads InteractionType=None and never shows the
        // "resurrect with sickness" confirm dialog, so legacy res-via-gossip silently
        // dies (the confirm previously translated to MSG_NULL_ACTION and was dropped).
        _worldPacket.WritePackedGuid128(Guid);
        if (ModernVersion.IsWotLKClassicOrLater)
        {
            _worldPacket.WriteInt32((int)PlayerInteractionType.SpiritHealer);
            _worldPacket.WriteBit(true);
            _worldPacket.FlushBits();
        }
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size
        + (ModernVersion.IsWotLKClassicOrLater ? 5 : 0);

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Guid.Low, Guid.High);
        if (ModernVersion.IsWotLKClassicOrLater)
        {
            writer.WriteInt32((int)PlayerInteractionType.SpiritHealer);
            writer.WriteBit(true);
            writer.FlushBits();
        }
        return writer.Position;
    }

    public WowGuid128 Guid;
}
