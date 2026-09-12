using System;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>Translation for the modern client's mail CMSGs. Behaviour only.</summary>
public static class MailSystem
{
    [HandlesCmsg(Opcode.CMSG_QUERY_NEXT_MAIL_TIME)]
    public static void HandleMailGetList(in EmptyClientPacket mail, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_QUERY_NEXT_MAIL_TIME);
        ctx.SendPacketToServer(packet);
    }
}
