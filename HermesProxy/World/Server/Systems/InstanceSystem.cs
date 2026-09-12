using System;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>Translation for the modern client's instance CMSGs. Behaviour only.</summary>
public static class InstanceSystem
{
    [HandlesCmsg(Opcode.CMSG_RESET_INSTANCES)]
    public static void HandleResetInstances(in EmptyClientPacket reset, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_RESET_INSTANCES);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_REQUEST_RAID_INFO)]
    public static void HandleRequestRaidInfo(in EmptyClientPacket reset, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_REQUEST_RAID_INFO);
        ctx.SendPacketToServer(packet);
    }
}
