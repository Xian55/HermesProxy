using Framework;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using static HermesProxy.World.Server.Packets.TradeUpdated;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [HandlesSmsg(Opcode.SMSG_TRADE_STATUS)]
    internal void HandleTradeStatus(WorldPacket packet)
    {
        TradeStatusPkt trade = new();
        if (IsCataLegacy)
        {
            // TrinityCore 4.3.4 TradeStatus::Write: a bit, then the status in five, and numbered
            // its own way. Read as a uint32, no status ever matched and no trade ever opened.
            packet.HasBit();                        // PartnerIsSameBnetAccount
            trade.Status = TradeStatusFromCata(packet.ReadBits<uint>(5));
        }
        else
            trade.Status = (TradeStatus)packet.ReadUInt32();

        var gameState = GetSession().GameState;
        TradeSession? tradeSession = gameState.CurrentTrade;
        gameState.TradeJustCompleted = trade.Status == TradeStatus.Complete;
        WorldClientLogMessages.TradeStatusReceived(_melLog, _sourceFile, _netDirNone, trade.Status, gameState.CurrentPlayerInfo?.Name, tradeSession != null);

        // Only the target of CMSG_INITIATE_TRADE is sent Proposed, so the initiator has no
        // session until Initiated, and every refusal of its request (target busy, too far away,
        // dead, ...) arrives without one. Those must reach the client with their own status,
        // not a substituted Cancelled (#228). Verified on V3_4_3 only.
        if (tradeSession == null && trade.Status is not (TradeStatus.Proposed or TradeStatus.Initiated)
            && !ModernVersion.IsWotLKClassicOrLater)
        {
            Log.Print(LogType.Error, $"Got SMSG_TRADE_STATUS without trade session (status: {trade.Status})");
            SendPacketToClient(new TradeStatusPkt { Status = TradeStatus.Cancelled });
            return;
        }

        switch (trade.Status)
        {
            case TradeStatus.Proposed:
                tradeSession ??= gameState.CurrentTrade = new TradeSession();
                trade.Partner = tradeSession.Partner = (IsCataLegacy
                    ? ReadMaskedGuid(packet, [2, 4, 6, 0, 1, 3, 7, 5], [4, 1, 2, 3, 0, 7, 6, 5])
                    : packet.ReadGuid()).To128(gameState);
                trade.PartnerAccount = tradeSession.PartnerAccount = GetSession().GetGameAccountGuidForPlayer(trade.Partner);
                break;
            case TradeStatus.Initiated:
                tradeSession ??= gameState.CurrentTrade = new TradeSession();
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    trade.Id = packet.ReadUInt32();
                else
                    trade.Id = TradeSession.GlobalTradeIdCounter++;
                tradeSession.TradeId = trade.Id;
                break;
            case TradeStatus.Failed when IsCataLegacy:
                trade.FailureForYou = packet.HasBit();
                trade.BagResult = LegacyVersion.ConvertInventoryResult(packet.ReadUInt32());
                trade.ItemID = packet.ReadUInt32();
                break;
            case TradeStatus.Failed:
                trade.BagResult = LegacyVersion.ConvertInventoryResult(packet.ReadUInt32());
                trade.FailureForYou = packet.ReadBool();
                trade.ItemID = packet.ReadUInt32();
                break;
            case TradeStatus.WrongRealm:
            case TradeStatus.NotOnTaplist:
                trade.TradeSlot = packet.ReadUInt8();
                break;
        }

        bool tradeIsDone = trade.Status is not (TradeStatus.Proposed or TradeStatus.Initiated or TradeStatus.Accepted or TradeStatus.Unaccepted or TradeStatus.StateChanged or TradeStatus.WrongRealm);
        if (tradeIsDone)
            GetSession().GameState.CurrentTrade = null;

        SendPacketToClient(trade);
    }

    /// <summary>TrinityCore 4.3.4 numbers the trade statuses its own way (SharedDefines.h TradeStatus).</summary>
    private static TradeStatus TradeStatusFromCata(uint status) => status switch
    {
        0 => TradeStatus.Initiated,
        2 => TradeStatus.NotOnTaplist,
        3 => TradeStatus.LoggingOut,
        4 => TradeStatus.PlayerIgnored,
        5 => TradeStatus.TargetDead,
        6 => TradeStatus.Accepted,
        7 => TradeStatus.TargetLoggingOut,
        9 => TradeStatus.Complete,
        10 => TradeStatus.RestrictedAccount,
        12 => TradeStatus.Proposed,
        13 => TradeStatus.Dead,
        16 => TradeStatus.TooFarAway,
        17 => TradeStatus.NoTarget,
        18 => TradeStatus.AlreadyTrading,
        19 => TradeStatus.CurrencyNotTradable,
        20 => TradeStatus.WrongFaction,
        21 => TradeStatus.PlayerBusy,
        22 => TradeStatus.StateChanged,
        23 => TradeStatus.Cancelled,
        24 => TradeStatus.NotEnoughCurrency,
        25 => TradeStatus.Unaccepted,
        26 => TradeStatus.WrongRealm,
        27 => TradeStatus.Stunned,
        29 => TradeStatus.TargetStunned,
        31 => TradeStatus.Failed,
        _ => TradeStatus.Cancelled,
    };

    private static WowGuid64 ReadMaskedGuid(WorldPacket packet, ReadOnlySpan<byte> bitOrder, ReadOnlySpan<byte> byteOrder)
    {
        Span<bool> mask = stackalloc bool[8];
        Span<byte> bytes = stackalloc byte[8];
        MaskedGuid.ReadMaskBits(packet, mask, bitOrder);
        MaskedGuid.ReadBytes(packet, mask, bytes, byteOrder);
        return new WowGuid64(MaskedGuid.ToUInt64(bytes));
    }

    /// <summary>
    /// TrinityCore 4.3.4 TradeUpdated::Write: the header first, then every item's GUID masks and
    /// flags as bits, then every item's fields with the GUID bytes strewn among them.
    /// </summary>
    private void HandleTradeStatusExtendedCata(WorldPacket packet, TradeSession tradeSession)
    {
        TradeUpdated trade = new();
        packet.ReadUInt32();                        // ID
        packet.ReadInt32();                         // CurrencyType
        trade.Gold = packet.ReadUInt64();
        trade.ProposedEnchantment = packet.ReadInt32();
        uint clientStateIndex = packet.ReadUInt32();
        packet.ReadInt32();                         // CurrencyQuantity
        trade.WhichPlayer = packet.ReadUInt8();
        uint currentStateIndex = packet.ReadUInt32();

        trade.Id = tradeSession.TradeId;
        // 4.3.4 keeps both counters itself and holds an accept against its own: TrinityCore's
        // HandleAcceptTradeOpcode answers "state changed" to any other number, and its counter
        // is random. So the client gets the server's numbers, not the ones the proxy counts
        // for the older servers that have none.
        trade.ClientStateIndex = clientStateIndex;
        trade.CurrentStateIndex = currentStateIndex;

        int count = (int)packet.ReadBits<uint>(22);
        if (count > 16)
            return; // a trade has seven slots; anything larger is not this layout

        Span<bool> giftMasks = stackalloc bool[count * 8];
        Span<bool> creatorMasks = stackalloc bool[count * 8];
        Span<bool> unwrapped = stackalloc bool[count];
        Span<bool> locked = stackalloc bool[count];
        for (int i = 0; i < count; i++)
        {
            Span<bool> gift = giftMasks.Slice(i * 8, 8);
            Span<bool> creator = creatorMasks.Slice(i * 8, 8);
            MaskedGuid.ReadMaskBits(packet, gift, [7, 1]);
            unwrapped[i] = packet.HasBit();
            MaskedGuid.ReadMaskBits(packet, gift, [3]);
            if (unwrapped[i])
            {
                MaskedGuid.ReadMaskBits(packet, creator, [7, 1, 4, 6, 2, 3, 5]);
                locked[i] = packet.HasBit();
                MaskedGuid.ReadMaskBits(packet, creator, [0]);
            }
            MaskedGuid.ReadMaskBits(packet, gift, [6, 4, 2, 0, 5]);
        }

        Span<byte> giftBytes = stackalloc byte[8];
        Span<byte> creatorBytes = stackalloc byte[8];
        for (int i = 0; i < count; i++)
        {
            ReadOnlySpan<bool> gift = giftMasks.Slice(i * 8, 8);
            ReadOnlySpan<bool> creator = creatorMasks.Slice(i * 8, 8);
            giftBytes.Clear();
            creatorBytes.Clear();

            TradeItem item = new TradeItem();
            if (unwrapped[i])
            {
                item.Unwrapped = new UnwrappedTradeItem();
                item.Unwrapped.Lock = locked[i];
                MaskedGuid.ReadBytes(packet, creator, creatorBytes, [1]);
                item.Unwrapped.EnchantID = packet.ReadInt32();
                for (int socket = 0; socket < 3; socket++)
                    packet.ReadInt32();             // SocketEnchant
                item.Unwrapped.MaxDurability = packet.ReadUInt32();
                MaskedGuid.ReadBytes(packet, creator, creatorBytes, [6, 2, 7, 4]);
                packet.ReadInt32();                 // ReforgeEnchantmentID
                item.Unwrapped.Durability = packet.ReadUInt32();
                item.Item.RandomPropertiesID = packet.ReadUInt32();
                MaskedGuid.ReadBytes(packet, creator, creatorBytes, [3]);
                packet.ReadInt32();
                MaskedGuid.ReadBytes(packet, creator, creatorBytes, [0]);
                item.Unwrapped.Charges = packet.ReadInt32();
                item.Item.RandomPropertiesSeed = packet.ReadUInt32();
                MaskedGuid.ReadBytes(packet, creator, creatorBytes, [5]);
                item.Unwrapped.Creator = new WowGuid64(MaskedGuid.ToUInt64(creatorBytes)).To128(GetSession().GameState);
            }

            MaskedGuid.ReadBytes(packet, gift, giftBytes, [6, 1, 7, 4]);
            item.Item.ItemID = packet.ReadUInt32();
            MaskedGuid.ReadBytes(packet, gift, giftBytes, [0]);
            item.StackCount = packet.ReadInt32();
            MaskedGuid.ReadBytes(packet, gift, giftBytes, [5]);
            item.Slot = packet.ReadUInt8();
            MaskedGuid.ReadBytes(packet, gift, giftBytes, [2, 3]);
            item.GiftCreator = new WowGuid64(MaskedGuid.ToUInt64(giftBytes)).To128(GetSession().GameState);
            trade.Items.Add(item);
        }

        SendPacketToClient(trade);
    }

    [HandlesSmsg(Opcode.SMSG_TRADE_STATUS_EXTENDED)]
    internal void HandleTradeStatusExtended(WorldPacket packet)
    {
        var tradeSession = GetSession().GameState.CurrentTrade;
        if (tradeSession == null)
        {
            Log.Print(LogType.Error, "Got SMSG_TRADE_STATUS_EXTENDED without trade session");
            return;
        }
        tradeSession.ServerStateIndex++;

        if (IsCataLegacy)
        {
            HandleTradeStatusExtendedCata(packet, tradeSession);
            return;
        }

        TradeUpdated trade = new();
        trade.WhichPlayer = packet.ReadUInt8();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            var actualTradeId = packet.ReadUInt32();
            if (actualTradeId != trade.Id)
            {
                Log.Print(LogType.Error, $"Got SMSG_TRADE_STATUS_EXTENDED with wrong tradeId (expected {trade.Id} but got {actualTradeId})");
                return;
            }
        }
        trade.Id = tradeSession.TradeId;

        // these might be the client/current state indexes
        // but mangos sends TRADE_SLOT_COUNT here
        _ = packet.ReadUInt32();
        _ = packet.ReadUInt32();

        trade.ClientStateIndex = tradeSession.ClientStateIndex;
        trade.CurrentStateIndex = tradeSession.ServerStateIndex;

        trade.Gold = packet.ReadUInt32();
        trade.ProposedEnchantment = packet.ReadInt32();
        while (packet.CanRead())
        {
            TradeItem item = new TradeItem();
            item.Unwrapped = new UnwrappedTradeItem();
            item.Slot = packet.ReadUInt8();
            item.Item.ItemID = packet.ReadUInt32();
            packet.ReadUInt32(); // Item Display ID
            item.StackCount = packet.ReadInt32();
            packet.ReadUInt32(); // Is Wrapped
            item.GiftCreator = packet.ReadGuid().To128(GetSession().GameState);
            item.Unwrapped.EnchantID = packet.ReadInt32();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                for (var i = 0; i < 3; ++i)
                    packet.ReadUInt32(); // Item Enchantment Id
            }
            item.Unwrapped.Creator = packet.ReadGuid().To128(GetSession().GameState);
            item.Unwrapped.Charges = packet.ReadInt32();
            item.Item.RandomPropertiesSeed = packet.ReadUInt32();
            item.Item.RandomPropertiesID = packet.ReadUInt32();
            item.Unwrapped.Lock = packet.ReadUInt32() != 0;
            item.Unwrapped.MaxDurability = packet.ReadUInt32();
            item.Unwrapped.Durability = packet.ReadUInt32();
            trade.Items.Add(item);
        }
        SendPacketToClient(trade);
    }
}
