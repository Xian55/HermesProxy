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
using HermesProxy.Enums;
using Framework.GameMath;
using Framework.IO;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

class AuctionHelloResponse : ServerPacket, ISpanWritable
{
    public AuctionHelloResponse() : base(Opcode.SMSG_AUCTION_HELLO_RESPONSE) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Guid);
        _worldPacket.WriteUInt32(AuctionHouseID);
        // V3_4_3 added the delivery-delay fields between AuctionHouseID and the
        // OpenForBusiness bit; omitting them shifts the bit past the payload so
        // the client reads it as 0 and shows "auction house closed" (#85).
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            _worldPacket.WriteUInt32(PurchasedItemDeliveryDelay);
            _worldPacket.WriteUInt32(CancelledItemDeliveryDelay);
        }
        _worldPacket.WriteBit(OpenForBusiness);
        _worldPacket.FlushBits();
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 5 + 8; // GUID + uint + bit + 2 V3_4_3 uints

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Guid.Low, Guid.High);
        writer.WriteUInt32(AuctionHouseID);
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            writer.WriteUInt32(PurchasedItemDeliveryDelay);
            writer.WriteUInt32(CancelledItemDeliveryDelay);
        }
        writer.WriteBit(OpenForBusiness);
        writer.FlushBits();
        return writer.Position;
    }

    public WowGuid128 Guid;
    public uint AuctionHouseID;
    public uint PurchasedItemDeliveryDelay;
    public uint CancelledItemDeliveryDelay;
    public bool OpenForBusiness = true;
}

public readonly record struct AuctionListBidderItems(
    WowGuid128 Auctioneer, uint Offset, List<uint> AuctionItemIDs);

public readonly record struct AuctionListOwnerItems(WowGuid128 Auctioneer, uint Offset);

public readonly record struct AuctionListItems(
    uint Offset, WowGuid128 Auctioneer, byte MinLevel, byte MaxLevel, int Quality,
    byte MaxPetLevel, List<byte> KnownPets, string Name, bool OnlyUsable, bool ExactMatch,
    List<ClassFilter> ClassFilters, List<AuctionSort> Sorts);

public readonly record struct AuctionSort(byte Type, byte Direction);

public readonly record struct ClassFilter(int ItemClass, List<SubClassFilter> SubClassFilters);

/// <remarks>
/// The two fields swap order between builds — V3_4_3 sends InvTypeMask (as a uint64) first, every
/// earlier build sends ItemSubclass first. That is one of the reasons AuctionListItems needs a
/// ranged codec pair rather than a branch inside one reader.
/// </remarks>
public readonly record struct SubClassFilter(int ItemSubclass, uint InvTypeMask);

public class AuctionListMyItemsResult : ServerPacket
{
    public AuctionListMyItemsResult(Opcode opcode) : base(opcode) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Items.Count);
        _worldPacket.WriteInt32(TotalItemsCount);
        _worldPacket.WriteUInt32(DesiredDelay);

        foreach (AuctionItem item in Items)
            item.Write(_worldPacket);
    }

    public List<AuctionItem> Items = new();
    public int TotalItemsCount;
    public uint DesiredDelay = 300;
}

public class AuctionListItemsResult : ServerPacket
{
    public AuctionListItemsResult() : base(Opcode.SMSG_AUCTION_LIST_ITEMS_RESULT) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Items.Count);
        _worldPacket.WriteInt32(TotalItemsCount);
        _worldPacket.WriteUInt32(DesiredDelay);

        if (Items.Count > 0)
            _worldPacket.WriteBool(OnlyUsable);

        foreach (AuctionItem item in Items)
            item.Write(_worldPacket);
    }

    public List<AuctionItem> Items = new();
    public int TotalItemsCount;
    public uint DesiredDelay = 300;
    public bool OnlyUsable;
}

public class AuctionItem
{
    public void Write(WorldPacket data)
    {
        data.WriteBit(Item != null);
        data.WriteBits(Enchantments.Count, 4);
        data.WriteBits(Gems.Count, 2);
        data.WriteBit(MinBid.HasValue);
        data.WriteBit(MinIncrement.HasValue);
        data.WriteBit(BuyoutPrice.HasValue);
        data.WriteBit(UnitPrice.HasValue);
        data.WriteBit(CensorServerSideInfo);
        data.WriteBit(CensorBidInfo);
        data.WriteBit(AuctionBucketKey != null);
        data.WriteBit(Creator != default);
        if (!CensorBidInfo)
        {
            data.WriteBit(Bidder != default);
            data.WriteBit(BidAmount.HasValue);
        }

        data.FlushBits();

        if (Item != null)
            Item.Write(data);

        data.WriteInt32(Count);
        data.WriteInt32(Charges);
        data.WriteUInt32(Flags);
        data.WriteUInt32(AuctionID);
        data.WritePackedGuid128(Owner);
        data.WriteInt32(DurationLeft);
        data.WriteUInt8(DeleteReason);

        foreach (ItemEnchantData enchant in Enchantments)
            enchant.Write(data);

        if (MinBid.HasValue)
            data.WriteUInt64(MinBid.Value);

        if (MinIncrement.HasValue)
            data.WriteUInt64(MinIncrement.Value);

        if (BuyoutPrice.HasValue)
            data.WriteUInt64(BuyoutPrice.Value);

        if (UnitPrice.HasValue)
            data.WriteUInt64(UnitPrice.Value);

        if (!CensorServerSideInfo)
        {
            data.WritePackedGuid128(ItemGuid);
            data.WritePackedGuid128(OwnerAccountID);
            data.WriteUInt32(EndTime);
        }

        if (Creator != default)
            data.WritePackedGuid128(Creator);

        if (!CensorBidInfo)
        {
            if (Bidder != default)
                data.WritePackedGuid128(Bidder);

            if (BidAmount.HasValue)
                data.WriteUInt64(BidAmount.Value);
        }

        foreach (ItemGemData gem in Gems)
            gem.Write(data);

        if (AuctionBucketKey != null)
            AuctionBucketKey.Write(data);
    }

    public ItemInstance Item = null!;
    public int Count;
    public int Charges;
    public List<ItemEnchantData> Enchantments = new();
    public uint Flags = 196608;
    public uint AuctionID;
    public WowGuid128 Owner;
    public ulong? MinBid;
    public ulong? MinIncrement;
    public ulong? BuyoutPrice;
    public ulong? UnitPrice;
    public int DurationLeft;
    public byte DeleteReason;
    public bool CensorServerSideInfo;
    public bool CensorBidInfo;
    public WowGuid128 ItemGuid = WowGuid128.Empty;
    public WowGuid128 OwnerAccountID;
    public uint EndTime;
    public WowGuid128 Creator;
    public WowGuid128 Bidder;
    public ulong? BidAmount;
    public List<ItemGemData> Gems = new();
    public AuctionBucketKey AuctionBucketKey = null!;
}

public class AuctionBucketKey
{
    public AuctionBucketKey() { }

    public AuctionBucketKey(WorldPacket data)
    {
        data.ResetBitPos();
        ItemID = data.ReadBits<uint>(20);

        if (data.HasBit())
            BattlePetSpeciesID = new();

        ItemLevel = data.ReadBits<ushort>(11);

        if (data.HasBit())
            SuffixItemNameDescriptionID = new();

        if (BattlePetSpeciesID.HasValue)
            BattlePetSpeciesID = data.ReadUInt16();

        if (SuffixItemNameDescriptionID.HasValue)
            SuffixItemNameDescriptionID = data.ReadUInt16();
    }

    public void Write(WorldPacket data)
    {
        data.WriteBits(ItemID, 20);
        data.WriteBit(BattlePetSpeciesID.HasValue);
        data.WriteBits(ItemLevel, 11);
        data.WriteBit(SuffixItemNameDescriptionID.HasValue);
        data.FlushBits();

        if (BattlePetSpeciesID.HasValue)
            data.WriteUInt16(BattlePetSpeciesID.Value);

        if (SuffixItemNameDescriptionID.HasValue)
            data.WriteUInt16(SuffixItemNameDescriptionID.Value);
    }

    public uint ItemID;
    public ushort ItemLevel;
    public ushort? BattlePetSpeciesID = new();
    public ushort? SuffixItemNameDescriptionID = new();
}

public class ItemEnchantData
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt32(ID);
        data.WriteUInt32(Expiration);
        data.WriteInt32(Charges);
        data.WriteUInt8(Slot);
    }

    public uint ID;
    public uint Expiration;
    public int Charges;
    public byte Slot;
}

public readonly record struct AuctionSellItem(
    WowGuid128 Auctioneer, ulong MinBid, ulong BuyoutPrice, uint ExpireTime,
    AddOnInfo? TaintedBy, List<AuctionItemForSale> Items);

public readonly record struct AuctionItemForSale(WowGuid128 Guid, uint UseCount);

public readonly record struct AuctionRemoveItem(
    WowGuid128 Auctioneer, uint AuctionID, AddOnInfo? TaintedBy);

public class AddOnInfo
{
    /// <summary>Span-reader twin of <see cref="Read(WorldPacket)"/>, kept in lockstep with it.</summary>
    public void Read(ref Framework.IO.SpanPacketReader data)
    {
        data.ResetBitReader();

        uint nameLength = data.ReadBits<uint>(10);
        uint versionLength = data.ReadBits<uint>(10);
        Loaded = data.HasBit();
        Disabled = data.HasBit();
        if (nameLength > 1)
        {
            Name = data.ReadString(nameLength - 1);
            data.ReadUInt8(); // null terminator
        }
        if (versionLength > 1)
        {
            Version = data.ReadString(versionLength - 1);
            data.ReadUInt8(); // null terminator
        }
    }

    public void Read(WorldPacket data)
    {
        data.ResetBitPos();

        uint nameLength = data.ReadBits<uint>(10);
        uint versionLength = data.ReadBits<uint>(10);
        Loaded = data.HasBit();
        Disabled = data.HasBit();
        if (nameLength > 1)
        {
            Name = data.ReadString(nameLength - 1);
            data.ReadUInt8(); // null terminator
        }
        if (versionLength > 1)
        {
            Version = data.ReadString(versionLength - 1);
            data.ReadUInt8(); // null terminator
        }
    }

    public string Name = string.Empty;
    public string Version = string.Empty;
    public bool Loaded;
    public bool Disabled;
}

public readonly record struct AuctionPlaceBid(
    WowGuid128 Auctioneer, uint AuctionID, ulong BidAmount, AddOnInfo? TaintedBy);

class AuctionCommandResult : ServerPacket, ISpanWritable
{
    public AuctionCommandResult() : base(Opcode.SMSG_AUCTION_COMMAND_RESULT) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(AuctionID);
        _worldPacket.WriteInt32((int)Command);
        _worldPacket.WriteInt32((int)ErrorCode);
        _worldPacket.WriteInt32((int)BagResult);
        _worldPacket.WritePackedGuid128(Guid);
        _worldPacket.WriteUInt64(MinIncrement);
        _worldPacket.WriteUInt64(Money);
        _worldPacket.WriteUInt32(DesiredDelay);
    }

    // 4 ints(16) + GUID(18) + 2 ulongs(16) + uint(4) = 54
    public int MaxSize => 16 + PackedGuidHelper.MaxPackedGuid128Size + 16 + 4;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(AuctionID);
        writer.WriteInt32((int)Command);
        writer.WriteInt32((int)ErrorCode);
        writer.WriteInt32((int)BagResult);
        writer.WritePackedGuid128(Guid.Low, Guid.High);
        writer.WriteUInt64(MinIncrement);
        writer.WriteUInt64(Money);
        writer.WriteUInt32(DesiredDelay);
        return writer.Position;
    }

    public uint AuctionID;                              //< the id of the auction that triggered this notification
    public AuctionHouseAction Command;                  //< the type of action that triggered this notification. Possible values are @ref AuctionAction
    public AuctionHouseError ErrorCode;                 //< the error code that was generated when trying to perform the action. Possible values are @ref AuctionError
    public InventoryResult BagResult = InventoryResult.InternalBagError; //< the bid error. Possible values are @ref AuctionError
    public WowGuid128 Guid = WowGuid128.Empty;          //< the GUID of the bidder for this auction.
    public ulong MinIncrement;                          //< the sum of outbid is (1% of current bid) * 5, if the bid is too small, then this value is 1 copper.
    public ulong Money;                                 //< the amount of money that the player bid in copper
    public uint DesiredDelay;
}

public class AuctionOwnerNotification
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt32(AuctionID);
        data.WriteUInt64(BidAmount);
        Item.Write(data);
    }

    public uint AuctionID;
    public ulong BidAmount;
    public ItemInstance Item = new ItemInstance();
}

internal static class AuctionPacketHelpers
{
    // AuctionOwnerNotification: uint(4) + ulong(8) + ItemInstance
    public const int AuctionOwnerNotificationMaxSize = 12 + ItemPacketHelpers.ItemInstanceMaxSize;

    // AuctionBidderNotification: 2 uints(8) + PackedGuid128(18) + ItemInstance
    public const int AuctionBidderNotificationMaxSize = 8 + PackedGuidHelper.MaxPackedGuid128Size + ItemPacketHelpers.ItemInstanceMaxSize;

    public static bool WriteAuctionOwnerNotification(ref SpanPacketWriter writer, AuctionOwnerNotification info)
    {
        writer.WriteUInt32(info.AuctionID);
        writer.WriteUInt64(info.BidAmount);
        return ItemPacketHelpers.WriteItemInstance(ref writer, info.Item);
    }

    public static bool WriteAuctionBidderNotification(ref SpanPacketWriter writer, AuctionBidderNotification info)
    {
        writer.WriteUInt32(info.Command);
        writer.WriteUInt32(info.AuctionID);
        writer.WritePackedGuid128(info.Bidder.Low, info.Bidder.High);
        return ItemPacketHelpers.WriteItemInstance(ref writer, info.Item);
    }
}

class AuctionClosedNotification : ServerPacket, ISpanWritable
{
    public AuctionClosedNotification() : base(Opcode.SMSG_AUCTION_CLOSED_NOTIFICATION) { }

    public override void Write()
    {
        Info.Write(_worldPacket);
        _worldPacket.WriteFloat(ProceedsMailDelay);
        _worldPacket.WriteBit(Sold);
        _worldPacket.FlushBits();
    }

    // AuctionOwnerNotification + float(4) + bit(1)
    public int MaxSize => AuctionPacketHelpers.AuctionOwnerNotificationMaxSize + 5;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        if (!AuctionPacketHelpers.WriteAuctionOwnerNotification(ref writer, Info))
            return -1;
        writer.WriteFloat(ProceedsMailDelay);
        writer.WriteBit(Sold);
        writer.FlushBits();
        return writer.Position;
    }

    public AuctionOwnerNotification Info = null!;
    public float ProceedsMailDelay = 3600;
    public bool Sold = true;
}

class AuctionOwnerBidNotification : ServerPacket, ISpanWritable
{
    public AuctionOwnerBidNotification() : base(Opcode.SMSG_AUCTION_OWNER_BID_NOTIFICATION) { }

    public override void Write()
    {
        Info.Write(_worldPacket);
        _worldPacket.WriteUInt64(MinIncrement);
        _worldPacket.WritePackedGuid128(Bidder);
    }

    // AuctionOwnerNotification + ulong(8) + PackedGuid128(18)
    public int MaxSize => AuctionPacketHelpers.AuctionOwnerNotificationMaxSize + 8 + PackedGuidHelper.MaxPackedGuid128Size;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        if (!AuctionPacketHelpers.WriteAuctionOwnerNotification(ref writer, Info))
            return -1;
        writer.WriteUInt64(MinIncrement);
        writer.WritePackedGuid128(Bidder.Low, Bidder.High);
        return writer.Position;
    }

    public AuctionOwnerNotification Info = null!;
    public ulong MinIncrement;
    public WowGuid128 Bidder;
}

class AuctionBidderNotification
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt32(Command);
        data.WriteUInt32(AuctionID);
        data.WritePackedGuid128(Bidder);
        Item.Write(data);
    }

    public uint Command = 2;
    public uint AuctionID;
    public WowGuid128 Bidder;
    public ItemInstance Item = new ItemInstance();
}

class AuctionWonNotification : ServerPacket, ISpanWritable
{
    public AuctionWonNotification() : base(Opcode.SMSG_AUCTION_WON_NOTIFICATION) { }

    public override void Write()
    {
        Info.Write(_worldPacket);
    }

    public int MaxSize => AuctionPacketHelpers.AuctionBidderNotificationMaxSize;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        if (!AuctionPacketHelpers.WriteAuctionBidderNotification(ref writer, Info))
            return -1;
        return writer.Position;
    }

    public AuctionBidderNotification Info = null!;
}

class AuctionOutbidNotification : ServerPacket, ISpanWritable
{
    public AuctionOutbidNotification() : base(Opcode.SMSG_AUCTION_OUTBID_NOTIFICATION) { }

    public override void Write()
    {
        Info.Write(_worldPacket);
        _worldPacket.WriteUInt64(BidAmount);
        _worldPacket.WriteUInt64(MinIncrement);
    }

    // AuctionBidderNotification + 2 ulongs(16)
    public int MaxSize => AuctionPacketHelpers.AuctionBidderNotificationMaxSize + 16;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        if (!AuctionPacketHelpers.WriteAuctionBidderNotification(ref writer, Info))
            return -1;
        writer.WriteUInt64(BidAmount);
        writer.WriteUInt64(MinIncrement);
        return writer.Position;
    }

    public AuctionBidderNotification Info = null!;
    public ulong BidAmount;
    public ulong MinIncrement;
}
