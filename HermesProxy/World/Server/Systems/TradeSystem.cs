using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's trade CMSGs.
/// </summary>
/// <remarks>
/// Three of these are shape B. They report the opcode when a trade action arrives with no trade
/// session open, and previously read it back off the packet with <c>GetUniversalOpcode()</c>; as a
/// dispatch parameter it arrives as a literal the JIT folds into the call instead.
/// </remarks>
public static class TradeSystem
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melLog = Log.CreateMelLogger(Log.CategoryPacket);
    private static readonly string _sourceFile = nameof(WorldSocket).PadRight(15);
    private static readonly string _netDirRecv = Log.FormatDir(LogNetDir.C2P);

    [HandlesCmsg(Opcode.CMSG_BEGIN_TRADE)]
    [HandlesCmsg(Opcode.CMSG_BUSY_TRADE)]
    [HandlesCmsg(Opcode.CMSG_CANCEL_TRADE)]
    [HandlesCmsg(Opcode.CMSG_UNACCEPT_TRADE)]
    [HandlesCmsg(Opcode.CMSG_IGNORE_TRADE)]
    public static void HandleEmptyTradePacket(Opcode opcode, in EmptyClientPacket trade, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(opcode);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_INITIATE_TRADE)]
    public static void HandleInitiateTrade(in InitiateTrade trade, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_INITIATE_TRADE);
        packet.WriteGuid(trade.Guid.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SET_TRADE_GOLD)]
    public static void HandleSetTradeGold(Opcode opcode, in SetTradeGold trade, in SessionContext ctx)
    {
        var tradeSession = ctx.GetSession().GameState.CurrentTrade;
        if (tradeSession == null)
        {
            LogTradeActionWithoutSession(in ctx, opcode);
            return;
        }
        tradeSession.ClientStateIndex++;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_TRADE_GOLD);
        packet.WriteInt32((int)trade.Coinage);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_ACCEPT_TRADE)]
    public static void HandleAcceptTrade(in AcceptTrade trade, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_ACCEPT_TRADE);
        packet.WriteUInt32(trade.StateIndex);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_CLEAR_TRADE_ITEM)]
    public static void HandleClearTradeItem(Opcode opcode, in ClearTradeItem trade, in SessionContext ctx)
    {
        var tradeSession = ctx.GetSession().GameState.CurrentTrade;
        if (tradeSession == null)
        {
            LogTradeActionWithoutSession(in ctx, opcode);
            return;
        }
        tradeSession.ClientStateIndex++;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_CLEAR_TRADE_ITEM);
        packet.WriteUInt8(trade.TradeSlot);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SET_TRADE_ITEM)]
    public static void HandleSetTradeItem(Opcode opcode, in SetTradeItem trade, in SessionContext ctx)
    {
        var tradeSession = ctx.GetSession().GameState.CurrentTrade;
        if (tradeSession == null)
        {
            LogTradeActionWithoutSession(in ctx, opcode);
            return;
        }
        tradeSession.ClientStateIndex++;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_TRADE_ITEM);
        packet.WriteUInt8(trade.TradeSlot);
        byte containerSlot = trade.PackSlot != Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(trade.PackSlot) : trade.PackSlot;
        byte slot = trade.PackSlot == Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(trade.ItemSlotInPack) : trade.ItemSlotInPack;
        packet.WriteUInt8(containerSlot);
        packet.WriteUInt8(slot);
        ctx.SendPacketToServer(packet);
    }

    static void LogTradeActionWithoutSession(in SessionContext ctx, Opcode opcode)
    {
        if (ctx.GetSession().GameState.TradeJustCompleted)
            WorldSocketLogMessages.TradeActionAfterComplete(_melLog, _sourceFile, _netDirRecv, opcode);
        else
            WorldSocketLogMessages.TradeActionWithoutSession(_melLog, _sourceFile, _netDirRecv, opcode);
    }
}
