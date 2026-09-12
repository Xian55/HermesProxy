using System;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>Translation for the modern client's GM-ticket CMSGs. Behaviour only.</summary>
public static class SupportTicketSystem
{
    [HandlesCmsg(Opcode.CMSG_GM_TICKET_GET_SYSTEM_STATUS)]
    public static void HandleGMTicketGetSystemStatus(in EmptyClientPacket packet, in SessionContext ctx)
    {
        // Forward so the answer reflects the backend's own ticket-system setting.
        ctx.SendPacketToServer(new WorldPacket(Opcode.CMSG_GM_TICKET_GET_SYSTEM_STATUS));
    }

    [HandlesCmsg(Opcode.CMSG_GM_TICKET_GET_CASE_STATUS)]
    public static void HandleGMTicketGetCaseStatus(in EmptyClientPacket packet, in SessionContext ctx)
    {
        ctx.SendPacket(new GMTicketCaseStatus());
    }
}
