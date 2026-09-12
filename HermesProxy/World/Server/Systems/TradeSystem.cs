using System;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>Translation for the modern client's trade CMSGs. Behaviour only.</summary>
public static class TradeSystem
{
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
}
