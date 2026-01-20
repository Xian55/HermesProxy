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

namespace HermesProxy.World.Server.Packets
{
    class LootUnit : ClientPacket
    {
        public LootUnit(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Unit = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 Unit;
    }

    public class LootResponse : ServerPacket, ISpanWritable
    {
        public LootResponse() : base(Opcode.SMSG_LOOT_RESPONSE, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Owner);
            _worldPacket.WritePackedGuid128(LootObj);
            _worldPacket.WriteUInt8((byte)FailureReason);
            _worldPacket.WriteUInt8((byte)AcquireReason);
            _worldPacket.WriteUInt8((byte)LootMethod);
            _worldPacket.WriteUInt8(Threshold);
            _worldPacket.WriteUInt32(Coins);
            _worldPacket.WriteInt32(Items.Count);
            _worldPacket.WriteInt32(Currencies.Count);
            _worldPacket.WriteBit(Acquired);
            _worldPacket.WriteBit(AELooting);
            _worldPacket.FlushBits();

            foreach (LootItemData item in Items)
                item.Write(_worldPacket);

            foreach (LootCurrency currency in Currencies)
            {
                _worldPacket.WriteUInt32(currency.CurrencyID);
                _worldPacket.WriteUInt32(currency.Quantity);
                _worldPacket.WriteUInt8(currency.LootListID);
                _worldPacket.WriteBits(currency.UIType, 3);
                _worldPacket.FlushBits();
            }
        }

        private const int MaxItems = 16;
        private const int MaxCurrencies = 4;
        // LootItemData: 1 (bits) + ItemInstance + 4 + 1 + 1 = 7 + ItemInstanceMaxSize
        private const int LootItemDataSize = 7 + ItemPacketHelpers.ItemInstanceMaxSize;
        // LootCurrency: 4 + 4 + 1 + 1 = 10
        private const int LootCurrencySize = 10;

        // 2 GUIDs + 4 bytes + uint + 2 ints + 1 byte bits + items + currencies
        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2 + 4 + 4 + 8 + 1 +
                              MaxItems * LootItemDataSize + MaxCurrencies * LootCurrencySize;

        public int WriteToSpan(Span<byte> buffer)
        {
            if (Items.Count > MaxItems || Currencies.Count > MaxCurrencies)
                return -1;

            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(Owner.Low, Owner.High);
            writer.WritePackedGuid128(LootObj.Low, LootObj.High);
            writer.WriteUInt8((byte)FailureReason);
            writer.WriteUInt8((byte)AcquireReason);
            writer.WriteUInt8((byte)LootMethod);
            writer.WriteUInt8(Threshold);
            writer.WriteUInt32(Coins);
            writer.WriteInt32(Items.Count);
            writer.WriteInt32(Currencies.Count);
            writer.WriteBit(Acquired);
            writer.WriteBit(AELooting);
            writer.FlushBits();

            foreach (LootItemData item in Items)
            {
                writer.WriteBits(item.Type, 2);
                writer.WriteBits((byte)item.UIType, 3);
                writer.WriteBit(item.CanTradeToTapList);
                writer.FlushBits();

                if (!ItemPacketHelpers.WriteItemInstance(ref writer, item.Loot))
                    return -1;

                writer.WriteUInt32(item.Quantity);
                writer.WriteUInt8(item.LootItemType);
                writer.WriteUInt8(item.LootListID);
            }

            foreach (LootCurrency currency in Currencies)
            {
                writer.WriteUInt32(currency.CurrencyID);
                writer.WriteUInt32(currency.Quantity);
                writer.WriteUInt8(currency.LootListID);
                writer.WriteBits(currency.UIType, 3);
                writer.FlushBits();
            }

            return writer.Position;
        }

        public WowGuid128 Owner;
        public WowGuid128 LootObj;
        public LootError FailureReason = LootError.NoLoot; // Most common value
        public LootType AcquireReason;
        public LootMethod LootMethod;
        public byte Threshold = 2; // Most common value, 2 = Uncommon
        public uint Coins;
        public List<LootItemData> Items = new();
        public List<LootCurrency> Currencies = new();
        public bool Acquired = true;
        public bool AELooting;
    }

    public class LootItemData
    {
        public void Write(WorldPacket data)
        {
            data.WriteBits(Type, 2);
            data.WriteBits(UIType, 3);
            data.WriteBit(CanTradeToTapList);
            data.FlushBits();
            Loot.Write(data); // WorldPackets::Item::ItemInstance
            data.WriteUInt32(Quantity);
            data.WriteUInt8(LootItemType);
            data.WriteUInt8(LootListID);
        }

        public byte Type;
        public LootSlotTypeModern UIType;
        public uint Quantity;
        public byte LootItemType;
        public byte LootListID;
        public bool CanTradeToTapList;
        public ItemInstance Loot = new();
    }

    public struct LootCurrency
    {
        public uint CurrencyID;
        public uint Quantity;
        public byte LootListID;
        public byte UIType;
    }

    class LootRelease : ClientPacket
    {
        public LootRelease(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Owner = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 Owner;
    }

    class LootReleaseResponse : ServerPacket, ISpanWritable
    {
        public LootReleaseResponse() : base(Opcode.SMSG_LOOT_RELEASE) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(LootObj);
            _worldPacket.WritePackedGuid128(Owner);
        }

        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2; // 2 GUIDs

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(LootObj.Low, LootObj.High);
            writer.WritePackedGuid128(Owner.Low, Owner.High);
            return writer.Position;
        }

        public WowGuid128 LootObj;
        public WowGuid128 Owner;
    }

    class LootMoney : ClientPacket
    {
        public LootMoney(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    class LootMoneyNotify : ServerPacket, ISpanWritable
    {
        public LootMoneyNotify() : base(Opcode.SMSG_LOOT_MONEY_NOTIFY) { }

        public override void Write()
        {
            _worldPacket.WriteUInt64(Money);
            _worldPacket.WriteUInt64(MoneyMod);
            _worldPacket.WriteBit(SoleLooter);
            _worldPacket.FlushBits();
        }

        public int MaxSize => 17; // 2 uint64 + 1 byte for bit

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt64(Money);
            writer.WriteUInt64(MoneyMod);
            writer.WriteBit(SoleLooter);
            writer.FlushBits();
            return writer.Position;
        }

        public ulong Money;
        public ulong MoneyMod;
        public bool SoleLooter;
    }

    class CoinRemoved : ServerPacket, ISpanWritable
    {
        public CoinRemoved() : base(Opcode.SMSG_COIN_REMOVED) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(LootObj);
        }

        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size;

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(LootObj.Low, LootObj.High);
            return writer.Position;
        }

        public WowGuid128 LootObj;
    }

    class LootItemPkt : ClientPacket
    {
        public LootItemPkt(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            uint Count = _worldPacket.ReadUInt32();

            for (uint i = 0; i < Count; ++i)
            {
                var loot = new LootRequest()
                {
                    LootObj = _worldPacket.ReadPackedGuid128(),
                    LootListID = _worldPacket.ReadUInt8()
                };

                Loot.Add(loot);
            }
        }

        public List<LootRequest> Loot = new();
    }
    public struct LootRequest
    {
        public WowGuid128 LootObj;
        public byte LootListID;
    }

    class LootRemoved : ServerPacket, ISpanWritable
    {
        public LootRemoved() : base(Opcode.SMSG_LOOT_REMOVED, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Owner);
            _worldPacket.WritePackedGuid128(LootObj);
            _worldPacket.WriteUInt8(LootListID);
        }

        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2 + 1; // 2 GUIDs + byte

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(Owner.Low, Owner.High);
            writer.WritePackedGuid128(LootObj.Low, LootObj.High);
            writer.WriteUInt8(LootListID);
            return writer.Position;
        }

        public WowGuid128 Owner;
        public WowGuid128 LootObj;
        public byte LootListID;
    }

    class SetLootMethod : ClientPacket
    {
        public SetLootMethod(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PartyIndex = _worldPacket.ReadInt8();
            LootMethod = (LootMethod)_worldPacket.ReadUInt8();
            LootMasterGUID = _worldPacket.ReadPackedGuid128();
            LootThreshold = _worldPacket.ReadUInt32();
        }

        public sbyte PartyIndex;
        public LootMethod LootMethod;
        public WowGuid128 LootMasterGUID;
        public uint LootThreshold;
    }

    class OptOutOfLoot : ClientPacket
    {
        public OptOutOfLoot(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PassOnLoot = _worldPacket.HasBit();
        }

        public bool PassOnLoot;
    }

    class StartLootRoll : ServerPacket, ISpanWritable
    {
        public StartLootRoll() : base(Opcode.SMSG_LOOT_START_ROLL) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(LootObj);
            _worldPacket.WriteUInt32(MapID);
            _worldPacket.WriteUInt32(RollTime);
            _worldPacket.WriteUInt8((byte)ValidRolls);
            _worldPacket.WriteUInt8((byte)Method);
            Item.Write(_worldPacket);
        }

        // LootItemData: 1 (bits) + ItemInstance + 4 + 1 + 1 = 7 + ItemInstanceMaxSize
        private const int LootItemDataSize = 7 + ItemPacketHelpers.ItemInstanceMaxSize;
        // GUID(18) + 2 uints(8) + 2 bytes(2) + LootItemData
        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 8 + 2 + LootItemDataSize;

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(LootObj.Low, LootObj.High);
            writer.WriteUInt32(MapID);
            writer.WriteUInt32(RollTime);
            writer.WriteUInt8((byte)ValidRolls);
            writer.WriteUInt8((byte)Method);

            // Write LootItemData inline
            writer.WriteBits(Item.Type, 2);
            writer.WriteBits((byte)Item.UIType, 3);
            writer.WriteBit(Item.CanTradeToTapList);
            writer.FlushBits();

            if (!ItemPacketHelpers.WriteItemInstance(ref writer, Item.Loot))
                return -1;

            writer.WriteUInt32(Item.Quantity);
            writer.WriteUInt8(Item.LootItemType);
            writer.WriteUInt8(Item.LootListID);

            return writer.Position;
        }

        public WowGuid128 LootObj;
        public uint MapID;
        public uint RollTime;
        public LootMethod Method = LootMethod.GroupLoot;
        public RollMask ValidRolls;
        public LootItemData Item = new();
    }

    class LootRoll : ClientPacket
    {
        public LootRoll(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            LootObj = _worldPacket.ReadPackedGuid128();
            LootListID = _worldPacket.ReadUInt8();
            RollType = (RollType)_worldPacket.ReadUInt8();
        }

        public WowGuid128 LootObj;
        public byte LootListID;
        public RollType RollType;
    }

    class LootRollBroadcast : ServerPacket, ISpanWritable
    {
        public LootRollBroadcast() : base(Opcode.SMSG_LOOT_ROLL) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(LootObj);
            _worldPacket.WritePackedGuid128(Player);
            _worldPacket.WriteInt32(Roll);
            _worldPacket.WriteUInt8((byte)RollType);
            Item.Write(_worldPacket);
            _worldPacket.WriteBit(Autopassed);
            _worldPacket.FlushBits();
        }

        // LootItemData: 1 (bits) + ItemInstance + 6 = 7 + ItemInstanceMaxSize
        private const int LootItemDataSize = 7 + ItemPacketHelpers.ItemInstanceMaxSize;
        // 2 GUIDs + int + byte + LootItemData + 1 bit
        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2 + 4 + 1 + LootItemDataSize + 1;

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(LootObj.Low, LootObj.High);
            writer.WritePackedGuid128(Player.Low, Player.High);
            writer.WriteInt32(Roll);
            writer.WriteUInt8((byte)RollType);

            // Write LootItemData inline
            writer.WriteBits(Item.Type, 2);
            writer.WriteBits((byte)Item.UIType, 3);
            writer.WriteBit(Item.CanTradeToTapList);
            writer.FlushBits();

            if (!ItemPacketHelpers.WriteItemInstance(ref writer, Item.Loot))
                return -1;

            writer.WriteUInt32(Item.Quantity);
            writer.WriteUInt8(Item.LootItemType);
            writer.WriteUInt8(Item.LootListID);

            writer.WriteBit(Autopassed);
            writer.FlushBits();

            return writer.Position;
        }

        public WowGuid128 LootObj;
        public WowGuid128 Player;
        public int Roll;
        public RollType RollType;
        public LootItemData Item = new();
        public bool Autopassed = false;    // Triggers message |HlootHistory:%d|h[Loot]|h: You automatically passed on: %s because you cannot loot that item.
    }

    class LootRollWon : ServerPacket, ISpanWritable
    {
        public LootRollWon() : base(Opcode.SMSG_LOOT_ROLL_WON) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(LootObj);
            _worldPacket.WritePackedGuid128(Winner);
            _worldPacket.WriteInt32(Roll);
            _worldPacket.WriteUInt8((byte)RollType);
            Item.Write(_worldPacket);
            _worldPacket.WriteUInt8(MainSpec);
        }

        // LootItemData: 1 (bits) + ItemInstance + 6 = 7 + ItemInstanceMaxSize
        private const int LootItemDataSize = 7 + ItemPacketHelpers.ItemInstanceMaxSize;
        // 2 GUIDs + int + byte + LootItemData + byte
        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 2 + 4 + 1 + LootItemDataSize + 1;

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(LootObj.Low, LootObj.High);
            writer.WritePackedGuid128(Winner.Low, Winner.High);
            writer.WriteInt32(Roll);
            writer.WriteUInt8((byte)RollType);

            // Write LootItemData inline
            writer.WriteBits(Item.Type, 2);
            writer.WriteBits((byte)Item.UIType, 3);
            writer.WriteBit(Item.CanTradeToTapList);
            writer.FlushBits();

            if (!ItemPacketHelpers.WriteItemInstance(ref writer, Item.Loot))
                return -1;

            writer.WriteUInt32(Item.Quantity);
            writer.WriteUInt8(Item.LootItemType);
            writer.WriteUInt8(Item.LootListID);

            writer.WriteUInt8(MainSpec);

            return writer.Position;
        }

        public WowGuid128 LootObj;
        public WowGuid128 Winner;
        public int Roll;
        public RollType RollType;
        public LootItemData Item = new();
        public byte MainSpec;
    }

    class LootAllPassed : ServerPacket, ISpanWritable
    {
        public LootAllPassed() : base(Opcode.SMSG_LOOT_ALL_PASSED) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(LootObj);
            Item.Write(_worldPacket);
        }

        // LootItemData: 1 (bits) + ItemInstance + 6 = 7 + ItemInstanceMaxSize
        private const int LootItemDataSize = 7 + ItemPacketHelpers.ItemInstanceMaxSize;
        // GUID + LootItemData
        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + LootItemDataSize;

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(LootObj.Low, LootObj.High);

            // Write LootItemData inline
            writer.WriteBits(Item.Type, 2);
            writer.WriteBits((byte)Item.UIType, 3);
            writer.WriteBit(Item.CanTradeToTapList);
            writer.FlushBits();

            if (!ItemPacketHelpers.WriteItemInstance(ref writer, Item.Loot))
                return -1;

            writer.WriteUInt32(Item.Quantity);
            writer.WriteUInt8(Item.LootItemType);
            writer.WriteUInt8(Item.LootListID);

            return writer.Position;
        }

        public WowGuid128 LootObj;
        public LootItemData Item = new();
    }

    class LootRollsComplete : ServerPacket, ISpanWritable
    {
        public LootRollsComplete() : base(Opcode.SMSG_LOOT_ROLLS_COMPLETE) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(LootObj);
            _worldPacket.WriteUInt8(LootListID);
        }

        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 1; // GUID + byte

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(LootObj.Low, LootObj.High);
            writer.WriteUInt8(LootListID);
            return writer.Position;
        }

        public WowGuid128 LootObj;
        public byte LootListID;
    }

    class LootMasterGive : ClientPacket
    {
        public LootMasterGive(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            uint Count = _worldPacket.ReadUInt32();
            TargetGUID = _worldPacket.ReadPackedGuid128();

            for (int i = 0; i < Count; ++i)
            {
                LootRequest lootRequest = new();
                lootRequest.LootObj = _worldPacket.ReadPackedGuid128();
                lootRequest.LootListID = _worldPacket.ReadUInt8();
                Loot.Add(lootRequest);
            }
        }

        public WowGuid128 TargetGUID;
        public List<LootRequest> Loot = new();
    }

    class MasterLootCandidateList : ServerPacket, ISpanWritable
    {
        // Max raid size is 40 players
        private const int MaxPlayers = 40;

        public MasterLootCandidateList() : base(Opcode.SMSG_LOOT_MASTER_LIST, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(LootObj);
            _worldPacket.WriteInt32(Players.Count);
            foreach (var guid in Players)
                _worldPacket.WritePackedGuid128(guid);
        }

        // MaxSize: GUID (18) + count (4) + 40 GUIDs (720) = 742
        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 4 + MaxPlayers * PackedGuidHelper.MaxPackedGuid128Size;

        public int WriteToSpan(Span<byte> buffer)
        {
            if (Players.Count > MaxPlayers)
                return -1;

            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(LootObj.Low, LootObj.High);
            writer.WriteInt32(Players.Count);
            foreach (var guid in Players)
                writer.WritePackedGuid128(guid.Low, guid.High);
            return writer.Position;
        }

        public WowGuid128 LootObj;
        public List<WowGuid128> Players = new();
    }

    class LootList : ServerPacket, ISpanWritable
    {
        public LootList() : base(Opcode.SMSG_LOOT_LIST, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Owner);
            _worldPacket.WritePackedGuid128(LootObj);

            _worldPacket.WriteBit(Master != default);
            _worldPacket.WriteBit(RoundRobinWinner != default);
            _worldPacket.FlushBits();

            if (Master != default)
                _worldPacket.WritePackedGuid128(Master);

            if (RoundRobinWinner != default)
                _worldPacket.WritePackedGuid128(RoundRobinWinner);
        }

        // MaxSize: 2 GUIDs (36) + 2 bits (1) + 2 optional GUIDs (36) = 73
        public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 4 + 1;

        public int WriteToSpan(Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(Owner.Low, Owner.High);
            writer.WritePackedGuid128(LootObj.Low, LootObj.High);

            writer.WriteBit(Master != default);
            writer.WriteBit(RoundRobinWinner != default);
            writer.FlushBits();

            if (Master != default)
                writer.WritePackedGuid128(Master.Low, Master.High);

            if (RoundRobinWinner != default)
                writer.WritePackedGuid128(RoundRobinWinner.Low, RoundRobinWinner.High);

            return writer.Position;
        }

        public WowGuid128 Owner;
        public WowGuid128 LootObj;
        public WowGuid128 Master;
        public WowGuid128 RoundRobinWinner;
    }
}
