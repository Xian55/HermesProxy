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
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class InitiateTrade : ClientPacket
{
    public InitiateTrade(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Guid = _worldPacket.ReadPackedGuid128();
    }

    public WowGuid128 Guid;
}

public class AcceptTrade : ClientPacket
{
    public AcceptTrade(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        StateIndex = _worldPacket.ReadUInt32();
    }

    public uint StateIndex;
}

public class ClearTradeItem : ClientPacket
{
    public ClearTradeItem(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        TradeSlot = _worldPacket.ReadUInt8();
    }

    public byte TradeSlot;
}

// Variant payloads for TradeStatusVariant union — each shape carries
// only the fields valid for its TradeStatus dispatch case.
public sealed record TradeFailedData(bool FailureForYou, InventoryResult BagResult, uint ItemID);
public sealed record TradeInitiatedData(uint Id);
public sealed record TradeProposedData(WowGuid128 Partner, WowGuid128 PartnerAccount);
public sealed record TradeSlotData(byte TradeSlot);
public sealed record TradeCurrencyData(int CurrencyType, int CurrencyQuantity);

public union TradeStatusVariant(
    TradeFailedData,
    TradeInitiatedData,
    TradeProposedData,
    TradeSlotData,
    TradeCurrencyData);

public class TradeStatusPkt : ServerPacket, ISpanWritable
{
    public TradeStatusPkt() : base(Opcode.SMSG_TRADE_STATUS, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteBit(PartnerIsSameBnetAccount);
        _worldPacket.WriteBits(Status, 5);
        switch (Variant.Value)
        {
            case TradeFailedData failed:
                _worldPacket.WriteBit(failed.FailureForYou);
                _worldPacket.WriteInt32((int)failed.BagResult);
                _worldPacket.WriteUInt32(failed.ItemID);
                break;
            case TradeInitiatedData init:
                _worldPacket.WriteUInt32(init.Id);
                break;
            case TradeProposedData proposed:
                _worldPacket.WritePackedGuid128(proposed.Partner);
                _worldPacket.WritePackedGuid128(proposed.PartnerAccount);
                break;
            case TradeSlotData slot:
                _worldPacket.WriteUInt8(slot.TradeSlot);
                break;
            case TradeCurrencyData currency:
                _worldPacket.WriteInt32(currency.CurrencyType);
                _worldPacket.WriteInt32(currency.CurrencyQuantity);
                break;
            default:
                _worldPacket.FlushBits();
                break;
        }
    }

    // Max size: bits(1) + largest case is Proposed with 2 GUIDs(36) = 37 bytes
    // Failed: bits(1) + int(4) + uint(4) = 9
    // Initiated: bits(1) + uint(4) = 5
    // Proposed: bits(1) + 2 GUIDs(36) = 37
    // WrongRealm/NotOnTaplist: bits(1) + byte(1) = 2
    // Currency: bits(1) + 2 ints(8) = 9
    public int MaxSize => 1 + PackedGuidHelper.MaxPackedGuid128Size * 2;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteBit(PartnerIsSameBnetAccount);
        writer.WriteBits((uint)Status, 5);
        switch (Variant.Value)
        {
            case TradeFailedData failed:
                writer.WriteBit(failed.FailureForYou);
                writer.WriteInt32((int)failed.BagResult);
                writer.WriteUInt32(failed.ItemID);
                break;
            case TradeInitiatedData init:
                writer.WriteUInt32(init.Id);
                break;
            case TradeProposedData proposed:
                writer.WritePackedGuid128(proposed.Partner.Low, proposed.Partner.High);
                writer.WritePackedGuid128(proposed.PartnerAccount.Low, proposed.PartnerAccount.High);
                break;
            case TradeSlotData slot:
                writer.WriteUInt8(slot.TradeSlot);
                break;
            case TradeCurrencyData currency:
                writer.WriteInt32(currency.CurrencyType);
                writer.WriteInt32(currency.CurrencyQuantity);
                break;
            default:
                writer.FlushBits();
                break;
        }
        return writer.Position;
    }

    public bool PartnerIsSameBnetAccount;
    public TradeStatus Status = TradeStatus.Initiated;

    /// <summary>
    /// Discriminated union holding fields specific to Status. Only populated for
    /// statuses that carry payload (Failed, Initiated, Proposed, slot/currency cases);
    /// other statuses leave Variant.Value as null and the writer falls through to
    /// the default branch.
    /// </summary>
    public TradeStatusVariant Variant;
}

public class SetTradeGold : ClientPacket
{
    public SetTradeGold(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Coinage = _worldPacket.ReadUInt64();
    }

    public ulong Coinage;
}

public class SetTradeItem : ClientPacket
{
    public SetTradeItem(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        TradeSlot = _worldPacket.ReadUInt8();
        PackSlot = _worldPacket.ReadUInt8();
        ItemSlotInPack = _worldPacket.ReadUInt8();
    }

    public byte TradeSlot;
    public byte PackSlot;
    public byte ItemSlotInPack;
}

public class TradeUpdated : ServerPacket
{
    public TradeUpdated() : base(Opcode.SMSG_TRADE_UPDATED, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt8(WhichPlayer);
        _worldPacket.WriteUInt32(Id);
        _worldPacket.WriteUInt32(ClientStateIndex);
        _worldPacket.WriteUInt32(CurrentStateIndex);
        _worldPacket.WriteUInt64(Gold);
        _worldPacket.WriteInt32(CurrencyType);
        _worldPacket.WriteInt32(CurrencyQuantity);
        _worldPacket.WriteInt32(ProposedEnchantment);
        _worldPacket.WriteInt32(Items.Count);

        Items.ForEach(item => item.Write(_worldPacket));
    }

    public class UnwrappedTradeItem
    {
        public void Write(WorldPacket data)
        {
            data.WriteInt32(EnchantID);
            data.WriteInt32(OnUseEnchantmentID);
            data.WritePackedGuid128(Creator);
            data.WriteInt32(Charges);
            data.WriteUInt32(MaxDurability);
            data.WriteUInt32(Durability);
            data.WriteBits(Gems.Count, 2);
            data.WriteBit(Lock);
            data.FlushBits();

            foreach (var gem in Gems)
                gem.Write(data);
        }

        public int EnchantID;
        public int OnUseEnchantmentID;
        public WowGuid128 Creator;
        public int Charges;
        public bool Lock;
        public uint MaxDurability;
        public uint Durability;
        public List<ItemGemData> Gems = new();
    }

    public class TradeItem
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt8(Slot);
            data.WriteInt32(StackCount);
            data.WritePackedGuid128(GiftCreator);
            Item.Write(data);
            data.WriteBit(Unwrapped != null);
            data.FlushBits();

            if (Unwrapped != null)
                Unwrapped.Write(data);
        }

        public byte Slot;
        public ItemInstance Item = new();
        public int StackCount;
        public WowGuid128 GiftCreator;
        public UnwrappedTradeItem Unwrapped = null!;
    }

    public ulong Gold;
    public uint CurrentStateIndex;
    public byte WhichPlayer;
    public uint ClientStateIndex;
    public List<TradeItem> Items = new();
    public int CurrencyType;
    public uint Id;
    public int ProposedEnchantment;
    public int CurrencyQuantity;
}

public class ItemGemData
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt8(Slot);
        Item.Write(data);
    }

    public void Read(WorldPacket data)
    {
        Slot = data.ReadUInt8();
        Item.Read(data);
    }

    public byte Slot;
    public ItemInstance Item = new();
}
