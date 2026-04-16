using Framework;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using static HermesProxy.World.Server.Packets.TradeUpdated;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [PacketHandler(Opcode.SMSG_TRADE_STATUS)]
    void HandleTradeStatus(WorldPacket packet)
    {
        TradeStatusPkt trade = new();
        trade.Status = (TradeStatus)packet.ReadUInt32();

        TradeSession? tradeSession = GetSession().GameState.CurrentTrade;
        if (tradeSession == null)
        {
            switch (trade.Status)
            {
                case TradeStatus.Initiated:
                case TradeStatus.Proposed:
                {
                    tradeSession = new TradeSession();
                    GetSession().GameState.CurrentTrade = tradeSession;
                    break;
                }
                default:
                {
                    Log.Print(LogType.Error, $"Got SMSG_TRADE_STATUS without trade session (status: {trade.Status})");
                    SendPacketToClient(new TradeStatusPkt { Status = TradeStatus.Cancelled });
                    return;
                }
            }
        } 

        switch (trade.Status)
        {
            case TradeStatus.Proposed:
            {
                var partner = packet.ReadGuid().To128(GetSession().GameState);
                var partnerAccount = GetSession().GetGameAccountGuidForPlayer(partner);
                tradeSession.Partner = partner;
                tradeSession.PartnerAccount = partnerAccount;
                trade.Variant = new TradeProposedData(partner, partnerAccount);
                break;
            }
            case TradeStatus.Initiated:
            {
                uint id = LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180)
                    ? packet.ReadUInt32()
                    : TradeSession.GlobalTradeIdCounter++;
                tradeSession.TradeId = id;
                trade.Variant = new TradeInitiatedData(id);
                break;
            }
            case TradeStatus.Failed:
            {
                var bagResult = LegacyVersion.ConvertInventoryResult(packet.ReadUInt32());
                var failureForYou = packet.ReadBool();
                var itemID = packet.ReadUInt32();
                trade.Variant = new TradeFailedData(failureForYou, bagResult, itemID);
                break;
            }
            case TradeStatus.WrongRealm:
            case TradeStatus.NotOnTaplist:
                trade.Variant = new TradeSlotData(packet.ReadUInt8());
                break;
        }

        bool tradeIsDone = trade.Status is not (TradeStatus.Proposed or TradeStatus.Initiated or TradeStatus.Accepted or TradeStatus.Unaccepted or TradeStatus.StateChanged or TradeStatus.WrongRealm);
        if (tradeIsDone)
            GetSession().GameState.CurrentTrade = null;

        SendPacketToClient(trade);
    }

    [PacketHandler(Opcode.SMSG_TRADE_STATUS_EXTENDED)]
    void HandleTradeStatusExtended(WorldPacket packet)
    {
        var tradeSession = GetSession().GameState.CurrentTrade;
        if (tradeSession == null)
        {
            Log.Print(LogType.Error, "Got SMSG_TRADE_STATUS_EXTENDED without trade session");
            return;
        }
        tradeSession.ServerStateIndex++;

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
