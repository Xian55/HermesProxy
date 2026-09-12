using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's duel CMSGs.
/// </summary>
/// <remarks>
/// <see cref="HandleCanDuel"/> never reaches the legacy server: modern clients ask permission before
/// sending the duel request and legacy has no such round trip, so the proxy answers yes itself and
/// lets the server refuse the duel proper if it wants to.
/// </remarks>
public static class DuelSystem
{
    [HandlesCmsg(Opcode.CMSG_CAN_DUEL)]
    public static void HandleCanDuel(in CanDuel request, in SessionContext ctx)
    {
        CanDuelResult result = new CanDuelResult();
        result.TargetGUID = request.TargetGUID;
        result.Result = true;
        ctx.SendPacket(result);
    }

    [HandlesCmsg(Opcode.CMSG_DUEL_RESPONSE)]
    public static void HandleDuelResponse(in DuelResponse response, in SessionContext ctx)
    {
        if (response.Accepted)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_DUEL_ACCEPTED);
            packet.WriteGuid(response.ArbiterGUID.To64());
            ctx.SendPacketToServer(packet);
        }
        else
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_DUEL_CANCELLED);
            packet.WriteGuid(response.ArbiterGUID.To64());
            ctx.SendPacketToServer(packet);
        }
    }
}
