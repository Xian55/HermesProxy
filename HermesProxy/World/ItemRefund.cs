using System;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World;

/// <summary>
/// What a legacy server says about an item that can still be handed back for what it cost (bought
/// for honor, points or other items, within two hours of play): its two packets, read the 3.3.5a
/// way and the 4.3.4 way into what the modern clients are sent.
///
/// The 3.3.5a side is TrinityCore / AzerothCore <c>Player::SendRefundInfo</c> and
/// <c>Player::RefundItem</c>, the 4.3.4 side TrinityCore 4.3.4 <c>Player::SendRefundInfo</c> and
/// <c>Player::SendItemRefundResult</c>.
/// </summary>
public static class ItemRefund
{
    /// <summary>
    /// 3.3.5a SMSG_ITEM_REFUND_INFO_RESPONSE. Honor and arena points are fields of their own
    /// there and currencies for the client. The time is the character's played time when the item
    /// was bought, which is what the modern client is sent too.
    /// </summary>
    public static ulong ReadInfoWrath(WorldPacket packet, ItemPurchaseContents contents, out uint purchaseTime)
    {
        ulong guid = packet.ReadUInt64();
        ReadCostWrath(packet, contents);
        packet.ReadUInt32();
        purchaseTime = packet.ReadUInt32();
        return guid;
    }

    /// <summary>
    /// 3.3.5a SMSG_ITEM_PURCHASE_REFUND_RESULT: the cost follows only when the refund went
    /// through. Returns whether it did.
    /// </summary>
    public static bool ReadResultWrath(WorldPacket packet, ItemPurchaseContents contents, out ulong guid, out byte result)
    {
        guid = packet.ReadUInt64();
        uint error = packet.ReadUInt32();
        result = (byte)Math.Min(error, byte.MaxValue);
        if (error != 0)
            return false;

        ReadCostWrath(packet, contents);
        return true;
    }

    private static void ReadCostWrath(WorldPacket packet, ItemPurchaseContents contents)
    {
        contents.Money = packet.ReadUInt32();
        uint honor = packet.ReadUInt32();
        uint arena = packet.ReadUInt32();
        for (int i = 0; i < ItemPurchaseContents.Slots; i++)
        {
            uint item = packet.ReadUInt32();
            uint count = packet.ReadUInt32();
            contents.Items[i] = (item, count);
        }

        // The server's honor is in whole points, the 4.4.2 client's in hundredths.
        int slot = 0;
        if (honor != 0)
            contents.Currencies[slot++] = ((uint)Currency.HonorPoints,
                ModernVersion.CurrenciesInHundredths ? honor * WorldClient.ModernCurrencyScale((uint)Currency.HonorPoints) : honor);
        if (arena != 0)
            contents.Currencies[slot] = ((uint)Currency.ArenaPoints, arena);
    }

    /// <summary>
    /// 4.3.4 SMSG_ITEM_REFUND_INFO_RESPONSE: a masked GUID around the fields, counts ahead of
    /// ids, and the time as the milliseconds the refund is still open, which the modern client
    /// has no use for (it is sent the played time at the purchase).
    /// </summary>
    public static ulong ReadInfoCata(WorldPacket packet, ItemPurchaseContents contents, out uint refundTimeLeftMs)
    {
        Span<bool> mask = stackalloc bool[8];
        Span<byte> guid = stackalloc byte[8];
        MaskedGuid.ReadMaskBits(packet, mask, [3, 5, 7, 6, 2, 4, 0, 1]);
        MaskedGuid.ReadByte(packet, mask, guid, 7);
        refundTimeLeftMs = packet.ReadUInt32();
        ReadItemsCata(packet, contents);
        MaskedGuid.ReadBytes(packet, mask, guid, [6, 4, 3, 2]);
        ReadCurrenciesCata(packet, contents);
        MaskedGuid.ReadBytes(packet, mask, guid, [1, 5]);
        packet.ReadUInt32();
        MaskedGuid.ReadByte(packet, mask, guid, 0);
        contents.Money = packet.ReadUInt32();
        return MaskedGuid.ToUInt64(guid);
    }

    /// <summary>4.3.4 SMSG_ITEM_PURCHASE_REFUND_RESULT. Returns whether the refund went through.</summary>
    public static bool ReadResultCata(WorldPacket packet, ItemPurchaseContents contents, out ulong guid, out byte result)
    {
        Span<bool> mask = stackalloc bool[8];
        Span<byte> bytes = stackalloc byte[8];
        MaskedGuid.ReadMaskBits(packet, mask, [4, 5, 1, 6, 7, 0, 3, 2]);
        bool refunded = packet.HasBit();
        packet.HasBit();        // whether money was paid; the amount says so itself
        if (refunded)
        {
            ReadCurrenciesCata(packet, contents);
            contents.Money = packet.ReadUInt32();
            ReadItemsCata(packet, contents);
        }
        MaskedGuid.ReadBytes(packet, mask, bytes, [0, 3, 1, 6, 4, 2, 7, 5]);
        result = packet.ReadUInt8();
        guid = MaskedGuid.ToUInt64(bytes);
        return refunded;
    }

    private static void ReadItemsCata(WorldPacket packet, ItemPurchaseContents contents)
    {
        for (int i = 0; i < ItemPurchaseContents.Slots; i++)
        {
            uint count = packet.ReadUInt32();
            uint item = packet.ReadUInt32();
            contents.Items[i] = (item, count);
        }
    }

    // A 4.3.4 server has divided the amount by the currency's precision; the 4.4.2 client divides
    // what it is sent, as with every other currency amount (see WorldClient.ModernCurrencyScale).
    private static void ReadCurrenciesCata(WorldPacket packet, ItemPurchaseContents contents)
    {
        for (int i = 0; i < ItemPurchaseContents.Slots; i++)
        {
            uint count = packet.ReadUInt32();
            uint id = packet.ReadUInt32();
            if (id == 0)
            {
                contents.Currencies[i] = default;
                continue;
            }

            uint modern = WorldClient.ModernCurrencyId(id);
            contents.Currencies[i] = (modern, count * WorldClient.ModernCurrencyScale(modern));
        }
    }
}
