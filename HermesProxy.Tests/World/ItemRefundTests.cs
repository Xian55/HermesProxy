using System;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// The two packets about handing an item back, read from a 3.3.5a and from a 4.3.4 server.
///
/// The legacy side is written out here a second time, the way the servers write it:
/// <see cref="WrathWritesInfo"/> and <see cref="WrathWritesResult"/> are TrinityCore / AzerothCore
/// 3.3.5a <c>Player::SendRefundInfo</c> and <c>Player::RefundItem</c>; <see cref="CataWritesInfo"/>
/// and <see cref="CataWritesResult"/> are TrinityCore 4.3.4 <c>Player::SendRefundInfo</c> and
/// <c>Player::SendItemRefundResult</c>, bit by bit. So the orders in the code are checked against
/// the source they came from, not against themselves.
/// </summary>
public sealed class ItemRefundTests
{
    private const ulong ItemGuid = 0x4700_0000_0012_34A5;

    private sealed class Cost
    {
        public uint Money;
        public uint Honor;
        public uint Arena;
        public (uint Id, uint Count)[] Items = new (uint, uint)[5];
        public (uint Id, uint Count)[] Currencies = new (uint, uint)[5];
    }

    // --- 3.3.5a

    private static void WrathCost(WorldPacket data, Cost cost)
    {
        data.WriteUInt32(cost.Money);                   // money cost
        data.WriteUInt32(cost.Honor);                   // honor point cost
        data.WriteUInt32(cost.Arena);                   // arena point cost
        for (int i = 0; i < 5; i++)                     // item cost data
        {
            data.WriteUInt32(cost.Items[i].Id);
            data.WriteUInt32(cost.Items[i].Count);
        }
    }

    private static byte[] WrathWritesInfo(ulong guid, Cost cost, uint playedAtPurchase)
    {
        using var data = new WorldPacket();
        data.WriteUInt16(0);                            // where a received packet has its opcode
        data.WriteUInt64(guid);                         // item guid
        WrathCost(data, cost);
        data.WriteUInt32(0);
        data.WriteUInt32(playedAtPurchase);             // GetTotalPlayedTime() - item->GetPlayedTime()
        return data.GetData();
    }

    private static byte[] WrathWritesResult(ulong guid, uint error, Cost cost)
    {
        using var data = new WorldPacket();
        data.WriteUInt16(0);
        data.WriteUInt64(guid);                         // item guid
        data.WriteUInt32(error);                        // 0, or error code
        if (error == 0)
            WrathCost(data, cost);
        return data.GetData();
    }

    // --- 4.3.4

    private static void Bit(WorldPacket data, ulong guid, int index) => data.WriteBit((byte)(guid >> (index * 8)) != 0);

    private static void ByteSeq(WorldPacket data, ulong guid, int index)
    {
        byte value = (byte)(guid >> (index * 8));
        if (value != 0)
            data.WriteUInt8((byte)(value ^ 1));
    }

    private static void CataItems(WorldPacket data, Cost cost)
    {
        for (int i = 0; i < 5; i++)                     // item cost data
        {
            data.WriteUInt32(cost.Items[i].Count);
            data.WriteUInt32(cost.Items[i].Id);
        }
    }

    private static void CataCurrencies(WorldPacket data, Cost cost)
    {
        for (int i = 0; i < 5; i++)                     // currency cost data
        {
            data.WriteUInt32(cost.Currencies[i].Count); // RequiredCurrencyCount / precision
            data.WriteUInt32(cost.Currencies[i].Id);
        }
    }

    private static byte[] CataWritesInfo(ulong guid, Cost cost, uint refundTimeLeftMs)
    {
        using var data = new WorldPacket();
        data.WriteUInt16(0);
        Bit(data, guid, 3);
        Bit(data, guid, 5);
        Bit(data, guid, 7);
        Bit(data, guid, 6);
        Bit(data, guid, 2);
        Bit(data, guid, 4);
        Bit(data, guid, 0);
        Bit(data, guid, 1);
        data.FlushBits();

        ByteSeq(data, guid, 7);
        data.WriteUInt32(refundTimeLeftMs);             // Refund Time Left
        CataItems(data, cost);

        ByteSeq(data, guid, 6);
        ByteSeq(data, guid, 4);
        ByteSeq(data, guid, 3);
        ByteSeq(data, guid, 2);
        CataCurrencies(data, cost);

        ByteSeq(data, guid, 1);
        ByteSeq(data, guid, 5);
        data.WriteUInt32(0);
        ByteSeq(data, guid, 0);
        data.WriteUInt32(cost.Money);                   // money cost
        return data.GetData();
    }

    private static byte[] CataWritesResult(ulong guid, byte error, Cost cost)
    {
        using var data = new WorldPacket();
        data.WriteUInt16(0);
        Bit(data, guid, 4);
        Bit(data, guid, 5);
        Bit(data, guid, 1);
        Bit(data, guid, 6);
        Bit(data, guid, 7);
        Bit(data, guid, 0);
        Bit(data, guid, 3);
        Bit(data, guid, 2);
        data.WriteBit(error == 0);
        data.WriteBit(cost.Money > 0);
        data.FlushBits();
        if (error == 0)
        {
            CataCurrencies(data, cost);
            data.WriteUInt32(cost.Money);               // money cost
            CataItems(data, cost);
        }

        ByteSeq(data, guid, 0);
        ByteSeq(data, guid, 3);
        ByteSeq(data, guid, 1);
        ByteSeq(data, guid, 6);
        ByteSeq(data, guid, 4);
        ByteSeq(data, guid, 2);
        ByteSeq(data, guid, 7);
        ByteSeq(data, guid, 5);

        data.WriteUInt8(error);                         // error code
        return data.GetData();
    }

    // --- 3.3.5a

    [Fact]
    public void WrathInfo_HonorAndArenaPoints_BecomeCurrencies()
    {
        var cost = new Cost { Money = 12, Honor = 300, Arena = 45 };
        cost.Items[0] = (20558, 3);
        cost.Items[4] = (29434, 1);
        using WorldPacket packet = LegacyPacketBuilder.Receive(WrathWritesInfo(ItemGuid, cost, 86_400));

        ItemPurchaseContents contents = new();
        ulong guid = ItemRefund.ReadInfoWrath(packet, contents, out uint purchaseTime);

        Assert.Equal(ItemGuid, guid);
        Assert.Equal(86_400u, purchaseTime);
        Assert.Equal(12ul, contents.Money);
        Assert.Equal((20558u, 3u), contents.Items[0]);
        Assert.Equal((0u, 0u), contents.Items[1]);
        Assert.Equal((29434u, 1u), contents.Items[4]);
        Assert.Equal(((uint)Currency.HonorPoints, 300u), contents.Currencies[0]);
        Assert.Equal(((uint)Currency.ArenaPoints, 45u), contents.Currencies[1]);
        Assert.Equal((0u, 0u), contents.Currencies[2]);
    }

    [Fact]
    public void WrathInfo_ArenaPointsAlone_TakeTheFirstCurrencySlot()
    {
        using WorldPacket packet = LegacyPacketBuilder.Receive(WrathWritesInfo(ItemGuid, new Cost { Arena = 1000 }, 5));

        ItemPurchaseContents contents = new();
        ItemRefund.ReadInfoWrath(packet, contents, out _);

        Assert.Equal(((uint)Currency.ArenaPoints, 1000u), contents.Currencies[0]);
        Assert.Equal((0u, 0u), contents.Currencies[1]);
    }

    [Fact]
    public void WrathResult_Refunded_CarriesWhatCameBack()
    {
        var cost = new Cost { Honor = 2000 };
        cost.Items[1] = (43228, 20);
        using WorldPacket packet = LegacyPacketBuilder.Receive(WrathWritesResult(ItemGuid, 0, cost));

        ItemPurchaseContents contents = new();
        bool refunded = ItemRefund.ReadResultWrath(packet, contents, out ulong guid, out byte result);

        Assert.True(refunded);
        Assert.Equal(ItemGuid, guid);
        Assert.Equal(0, result);
        Assert.Equal((43228u, 20u), contents.Items[1]);
        Assert.Equal(((uint)Currency.HonorPoints, 2000u), contents.Currencies[0]);
    }

    [Fact]
    public void WrathResult_Refused_IsTheGuidAndTheErrorOnly()
    {
        using WorldPacket packet = LegacyPacketBuilder.Receive(WrathWritesResult(ItemGuid, 10, new Cost()));

        ItemPurchaseContents contents = new();
        bool refunded = ItemRefund.ReadResultWrath(packet, contents, out ulong guid, out byte result);

        Assert.False(refunded);
        Assert.Equal(ItemGuid, guid);
        Assert.Equal(10, result);
    }

    // --- 4.3.4

    [Theory]
    [InlineData(0x4700_0000_0012_34A5ul)]
    [InlineData(0x0000_0000_0000_0001ul)]       // one non-zero byte: the others are not on the wire
    [InlineData(0x0102_0304_0506_0708ul)]
    public void CataInfo_IsReadAroundItsGuid(ulong itemGuid)
    {
        var cost = new Cost { Money = 77 };
        cost.Items[0] = (62898, 2);
        cost.Items[3] = (52722, 9);
        cost.Currencies[1] = (241, 30);         // Champion's Seal: sent as it is
        using WorldPacket packet = LegacyPacketBuilder.Receive(CataWritesInfo(itemGuid, cost, 7_140_000));

        ItemPurchaseContents contents = new();
        ulong guid = ItemRefund.ReadInfoCata(packet, contents, out uint timeLeftMs);

        Assert.Equal(itemGuid, guid);
        Assert.Equal(7_140_000u, timeLeftMs);
        Assert.Equal(77ul, contents.Money);
        Assert.Equal((62898u, 2u), contents.Items[0]);
        Assert.Equal((52722u, 9u), contents.Items[3]);
        Assert.Equal((0u, 0u), contents.Currencies[0]);
        Assert.Equal((241u, 30u), contents.Currencies[1]);
    }

    [Fact]
    public void CataInfo_HonorPoints_AreTheClientsHonorInHundredths()
    {
        var cost = new Cost();
        cost.Currencies[0] = (392, 165);        // 4.3.4 Honor Points, already divided by the server
        cost.Currencies[1] = (395, 1650);       // Justice Points
        using WorldPacket packet = LegacyPacketBuilder.Receive(CataWritesInfo(ItemGuid, cost, 1));

        ItemPurchaseContents contents = new();
        ItemRefund.ReadInfoCata(packet, contents, out _);

        Assert.Equal(((uint)Currency.HonorPoints, 16_500u), contents.Currencies[0]);
        Assert.Equal((395u, 165_000u), contents.Currencies[1]);
    }

    [Theory]
    [InlineData(0x4700_0000_0012_34A5ul)]
    [InlineData(0x0000_0000_0000_0001ul)]
    public void CataResult_Refunded_CarriesWhatCameBack(ulong itemGuid)
    {
        var cost = new Cost { Money = 5 };
        cost.Items[2] = (62898, 2);
        cost.Currencies[0] = (392, 165);
        using WorldPacket packet = LegacyPacketBuilder.Receive(CataWritesResult(itemGuid, 0, cost));

        ItemPurchaseContents contents = new();
        bool refunded = ItemRefund.ReadResultCata(packet, contents, out ulong guid, out byte result);

        Assert.True(refunded);
        Assert.Equal(itemGuid, guid);
        Assert.Equal(0, result);
        Assert.Equal(5ul, contents.Money);
        Assert.Equal((62898u, 2u), contents.Items[2]);
        Assert.Equal(((uint)Currency.HonorPoints, 16_500u), contents.Currencies[0]);
    }

    [Fact]
    public void CataResult_Refused_IsTheGuidAndTheErrorOnly()
    {
        using WorldPacket packet = LegacyPacketBuilder.Receive(CataWritesResult(ItemGuid, 10, new Cost()));

        ItemPurchaseContents contents = new();
        bool refunded = ItemRefund.ReadResultCata(packet, contents, out ulong guid, out byte result);

        Assert.False(refunded);
        Assert.Equal(ItemGuid, guid);
        Assert.Equal(10, result);
    }

    // --- what the client is sent

    [Fact]
    public void Contents_AreMoneyThenFiveItemsThenFiveCurrencies_IdAheadOfCount()
    {
        ItemPurchaseContents contents = new() { Money = 0x0102030405060708 };
        contents.Items[0] = (0xA1, 0xA2);
        contents.Items[4] = (0xA9, 0xAA);
        contents.Currencies[0] = (0xB1, 0xB2);
        contents.Currencies[4] = (0xB9, 0xBA);
        using var packet = new WorldPacket();

        contents.Write(packet);

        byte[] bytes = packet.GetData();
        Assert.Equal(8 + 20 * 4, bytes.Length);     // the client's own size for the block
        Assert.Equal(0x0102030405060708ul, BitConverter.ToUInt64(bytes, 0));
        Assert.Equal(0xA1u, BitConverter.ToUInt32(bytes, 8));
        Assert.Equal(0xA2u, BitConverter.ToUInt32(bytes, 12));
        Assert.Equal(0xA9u, BitConverter.ToUInt32(bytes, 8 + 4 * 8));
        Assert.Equal(0xAAu, BitConverter.ToUInt32(bytes, 8 + 4 * 8 + 4));
        Assert.Equal(0xB1u, BitConverter.ToUInt32(bytes, 48));
        Assert.Equal(0xB2u, BitConverter.ToUInt32(bytes, 52));
        Assert.Equal(0xB9u, BitConverter.ToUInt32(bytes, 48 + 4 * 8));
        Assert.Equal(0xBAu, BitConverter.ToUInt32(bytes, 48 + 4 * 8 + 4));
    }
}
