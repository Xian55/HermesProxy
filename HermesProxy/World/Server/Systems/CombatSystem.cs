using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's combat CMSGs. Behaviour only — the packets are data and
/// the parsing is in the codecs.
/// </summary>
public static class CombatSystem
{
    [HandlesCmsg(Opcode.CMSG_ATTACK_SWING)]
    public static void HandleAttackSwing(in AttackSwing attack, in SessionContext ctx)
    {
        var session = ctx.GetSession();
        // Session-aware To64: a pet's modern guid carries creature_template.entry, but the legacy
        // server keyed it by pet_number and looks the unit up by the whole guid. Plain To64 sends
        // an entry slot no unit matches, and the swing comes back as SMSG_ATTACK_STOP.
        MeleeAttackOrder.Swing(session.GameState, session.ToServer, attack.Victim.To64(session.GameState));
    }

    [HandlesCmsg(Opcode.CMSG_ATTACK_STOP)]
    public static void HandleAttackStop(in AttackStop attack, in SessionContext ctx)
    {
        var session = ctx.GetSession();
        MeleeAttackOrder.Stop(session.GameState, session.ToServer);
    }

    [HandlesCmsg(Opcode.CMSG_SET_SHEATHED)]
    public static void HandleSetSheathed(in SetSheathed sheath, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_SHEATHED);
        packet.WriteInt32(sheath.SheathState);
        ctx.SendPacketToServer(packet);
    }
}
