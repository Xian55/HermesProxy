using System;
using HermesProxy.World.Enums;
using HermesProxy.World.Outbox;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Keeps the client's <c>CMSG_ATTACK_STOP</c> from reaching the server while it has not yet
/// answered the <c>CMSG_ATTACK_SWING</c> before it.
/// </summary>
/// <remarks>
/// <para>
/// A stop arriving on the heels of a swing tangled cMaNGOS's combat state: rapid tab-targeting
/// left a player "attacking" with no swings (8e735a93). So a stop sent while a swing is unanswered
/// is held until the server answers it with <c>SMSG_ATTACK_START</c>, <c>SMSG_ATTACK_STOP</c>, a
/// swing error or <c>SMSG_CANCEL_COMBAT</c>. A swing that follows a held stop cancels it, so
/// stop-then-swing reaches the server as the plain target switch it handles.
/// </para>
/// <para>
/// The flags this replaces only ever sent a held stop on <c>SMSG_ATTACK_STOP</c>, which a server
/// that is still attacking never sends. With 300 ms of latency, a stop pressed right after
/// attacking did nothing: the swings kept landing until the player pressed stop again. The flags
/// were also checked on the socket thread and flipped on the legacy thread with no lock. Here
/// "answered" is an outbox gate, so checking it and holding the stop happen in one step.
/// </para>
/// </remarks>
internal static class MeleeAttackOrder
{
    internal static readonly HoldKey HeldStopKey = new(HoldKeyKind.AttackStop);

    // Well above a round trip on a poor connection. A server that never answers still gets the stop.
    internal static readonly TimeSpan HeldStopTimeout = TimeSpan.FromSeconds(2);

    private static readonly HoldOptions HeldStopHold = new(
        Timeout: HeldStopTimeout,
        OnTimeout: OutboxTimeoutAction.Release,
        Key: HeldStopKey);

    public static void Swing(GameSessionData state, ServerOutbox toServer, WowGuid64 victim)
    {
        // The server never saw a stop that is still held, so it is still attacking the current target.
        toServer.Cancel(HeldStopKey);

        if (state.CurrentAttackTarget == victim)
            return;

        state.CurrentAttackTarget = victim;
        toServer.SetGate(OutboxGate.SwingAnswered, open: false);
        WorldPacket packet = new WorldPacket(Opcode.CMSG_ATTACK_SWING);
        packet.WriteGuid(victim);
        toServer.Send(packet);
    }

    public static void Stop(GameSessionData state, ServerOutbox toServer)
    {
        // One stop is enough however often it was pressed while waiting.
        toServer.Cancel(HeldStopKey);
        toServer.When(OutboxGate.SwingAnswered, () =>
        {
            state.CurrentAttackTarget = default;
            toServer.Send(new WorldPacket(Opcode.CMSG_ATTACK_STOP));
        }, HeldStopHold);
    }

    /// <summary><c>SMSG_ATTACK_START</c> for the player, or a swing error: the swing was answered.</summary>
    public static void SwingAnswered(ServerOutbox toServer) => toServer.SetGate(OutboxGate.SwingAnswered, open: true);

    /// <summary><c>SMSG_ATTACK_STOP</c> for the player.</summary>
    public static void ServerStopped(GameSessionData state, ServerOutbox toServer, WowGuid64 victim)
    {
        // A stop with no victim is the server refusing the swing rather than reporting on a target:
        // AzerothCore answers an unknown attack target that way (SendAttackStop(nullptr) writes only
        // the attacker). It has to fall through, or the refusal latches CurrentAttackTarget and every
        // later swing at that unit is dropped below as a repeat of an attack that never started.
        if (!victim.IsEmpty() && !state.CurrentAttackTarget.IsEmpty() && state.CurrentAttackTarget != victim)
            return;

        // Forget the target, or attacking it again would be dropped as a repeat of this attack.
        state.CurrentAttackTarget = default;
        SwingAnswered(toServer);
    }

    /// <summary><c>SMSG_CANCEL_COMBAT</c>: nothing is being attacked, so a held stop has nothing to stop.</summary>
    public static void CombatCancelled(GameSessionData state, ServerOutbox toServer)
    {
        toServer.Cancel(HeldStopKey);
        state.CurrentAttackTarget = default;
        SwingAnswered(toServer);
    }
}
