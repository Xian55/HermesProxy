using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Game-object interaction CMSGs.
/// </summary>
/// <remarks>
/// The two look alike and are not: use forwards to the legacy server, report-use only records
/// which object the player last interacted with, because the legacy side has nothing to tell.
/// </remarks>
public static class GameObjectSystem
{
    [HandlesCmsg(Opcode.CMSG_GAME_OBJ_USE)]
    public static void HandleGameObjUse(in GameObjUse use, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GAME_OBJ_USE);
        packet.WriteGuid(use.Guid.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GAME_OBJ_REPORT_USE)]
    public static void HandleGameObjReportUse(in GameObjReportUse use, in SessionContext ctx)
    {
        ctx.GetSession().GameState.CurrentInteractedWithGO = use.Guid;
    }
}
