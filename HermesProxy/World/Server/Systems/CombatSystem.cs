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
        var victim64 = attack.Victim.To64();
        var state = ctx.GetSession().GameState;

        if (state.CurrentAttackTarget == victim64)
            return;

        // If we had a pending stop (STOP→SWING sequence), cancel it — just send the new SWING
        // The server handles target switching within ATTACK_SWING without needing an explicit STOP
        if (state.DeferredAttackStop)
            state.DeferredAttackStop = false;

        state.CurrentAttackTarget = victim64;
        state.WaitingForAttackStart = true;
        WorldPacket packet = new WorldPacket(Opcode.CMSG_ATTACK_SWING);
        packet.WriteGuid(victim64);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_ATTACK_STOP)]
    public static void HandleAttackStop(in AttackStop attack, in SessionContext ctx)
    {
        var state = ctx.GetSession().GameState;

        // Only defer ATTACK_STOP while waiting for server to acknowledge our SWING.
        // During this window, a rapid STOP→SWING (target switch) would corrupt cMangos
        // combat state. Once SMSG_ATTACK_START has arrived, server state is stable and
        // can handle a clean stop (ESC, /stopattack, etc.)
        if (state.WaitingForAttackStart)
        {
            state.DeferredAttackStop = true;
            return;
        }

        state.CurrentAttackTarget = default;
        WorldPacket packet = new WorldPacket(Opcode.CMSG_ATTACK_STOP);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SET_SHEATHED)]
    public static void HandleSetSheathed(in SetSheathed sheath, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_SHEATHED);
        packet.WriteInt32(sheath.SheathState);
        ctx.SendPacketToServer(packet);
    }
}
