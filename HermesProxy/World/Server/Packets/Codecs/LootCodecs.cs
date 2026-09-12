using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

// Loot and trade CMSG codecs.
//
// Two of the loot packets carry a wire-counted list. The count is packet data, so the pre-size goes
// through CodecHelpers.WireCountCapacity rather than the count itself — see CodecMalformedCountTests
// for why. A LootRequest is a packed guid128 (never under two bytes, both mask bytes zero) plus one
// byte, so three bytes is the floor.

public static class LootReleaseCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out LootRelease packet)
        => packet = new LootRelease(r.ReadPackedGuid128());
}

public static class LootUnitCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out LootUnit packet)
        => packet = new LootUnit(r.ReadPackedGuid128());
}

public static class LootMoneyCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out LootMoney packet)
        => packet = default;
}

public static class LootItemPktCodec
{
    private const int MinLootRequestBytes = 3;

    public static void Read(ref SpanPacketReader r, out LootItemPkt packet)
    {
        uint count = r.ReadUInt32();
        var loot = new List<LootRequest>(CodecHelpers.WireCountCapacity(count, in r, MinLootRequestBytes));
        for (uint i = 0; i < count; ++i)
        {
            // Guid before list id — the original built this with an object initializer, which
            // evaluates in source order, so the two reads happen in exactly this sequence.
            WowGuid128 lootObj = r.ReadPackedGuid128();
            loot.Add(new LootRequest(lootObj, r.ReadUInt8()));
        }
        packet = new LootItemPkt(loot);
    }
}

public static class LootMasterGiveCodec
{
    private const int MinLootRequestBytes = 3;

    public static void Read(ref SpanPacketReader r, out LootMasterGive packet)
    {
        uint count = r.ReadUInt32();
        WowGuid128 targetGuid = r.ReadPackedGuid128();
        var loot = new List<LootRequest>(CodecHelpers.WireCountCapacity(count, in r, MinLootRequestBytes));
        for (uint i = 0; i < count; ++i)
        {
            WowGuid128 lootObj = r.ReadPackedGuid128();
            loot.Add(new LootRequest(lootObj, r.ReadUInt8()));
        }
        packet = new LootMasterGive(targetGuid, loot);
    }
}

public static class SetLootMethodCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SetLootMethod packet)
    {
        sbyte partyIndex = r.ReadInt8();
        var lootMethod = (LootMethod)r.ReadUInt8();
        WowGuid128 lootMasterGuid = r.ReadPackedGuid128();
        packet = new SetLootMethod(partyIndex, lootMethod, lootMasterGuid, r.ReadUInt32());
    }
}

public static class OptOutOfLootCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out OptOutOfLoot packet)
        => packet = new OptOutOfLoot(r.HasBit());
}

public static class LootRollCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out LootRoll packet)
    {
        WowGuid128 lootObj = r.ReadPackedGuid128();
        byte lootListId = r.ReadUInt8();
        packet = new LootRoll(lootObj, lootListId, (RollType)r.ReadUInt8());
    }
}

// ---- trade ----

public static class InitiateTradeCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out InitiateTrade packet)
        => packet = new InitiateTrade(r.ReadPackedGuid128());
}

public static class SetTradeGoldCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SetTradeGold packet)
        => packet = new SetTradeGold(r.ReadUInt64());
}

public static class AcceptTradeCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out AcceptTrade packet)
        => packet = new AcceptTrade(r.ReadUInt32());
}

public static class ClearTradeItemCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ClearTradeItem packet)
        => packet = new ClearTradeItem(r.ReadUInt8());
}

public static class SetTradeItemCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SetTradeItem packet)
    {
        byte tradeSlot = r.ReadUInt8();
        byte packSlot = r.ReadUInt8();
        packet = new SetTradeItem(tradeSlot, packSlot, r.ReadUInt8());
    }
}
