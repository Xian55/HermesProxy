using System;
using System.Linq;
using System.Runtime.CompilerServices;
using HermesProxy.Tests.World.Outbox;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Outbox;
using HermesProxy.World.Server.Systems;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace HermesProxy.Tests.World.Server;

public class MeleeAttackOrderTests
{
    private static readonly WowGuid64 Wolf = new(HighGuidTypeLegacy.Creature, 299, 2290);
    private static readonly WowGuid64 Boar = new(HighGuidTypeLegacy.Creature, 113, 2291);

    private static uint SwingOpcode => HermesProxy.LegacyVersion.GetCurrentOpcode(Opcode.CMSG_ATTACK_SWING);
    private static uint StopOpcode => HermesProxy.LegacyVersion.GetCurrentOpcode(Opcode.CMSG_ATTACK_STOP);

    private sealed class Session
    {
        public readonly FakeTimeProvider Time = new();
        public readonly RecordingServerWire Wire = new();
        public readonly ServerOutbox ToServer;
        public readonly GameSessionData State = (GameSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GameSessionData));

        public Session()
        {
            ToServer = new ServerOutbox(Wire, Time);
            // As GlobalSessionData does for every new GameState.
            ToServer.SetGate(OutboxGate.SwingAnswered, open: true);
        }

        public uint[] Sent => Wire.Writes.Select(w => w.Packet.GetOpcode()).ToArray();
    }

    [Fact]
    public void StopAfterTheServerStartedTheAttack_GoesOutAtOnce()
    {
        var s = new Session();
        MeleeAttackOrder.Swing(s.State, s.ToServer, Wolf);
        MeleeAttackOrder.SwingAnswered(s.ToServer);

        MeleeAttackOrder.Stop(s.State, s.ToServer);

        Assert.Equal([SwingOpcode, StopOpcode], s.Sent);
        Assert.True(s.State.CurrentAttackTarget.IsEmpty());
    }

    [Fact]
    public void StopBeforeTheServerAnswered_IsSentTheMomentTheAttackStarts()
    {
        // The playtest bug: with latency, this stop used to wait for an SMSG_ATTACK_STOP that a server
        // still attacking never sends, and the swings kept landing.
        var s = new Session();
        MeleeAttackOrder.Swing(s.State, s.ToServer, Wolf);
        MeleeAttackOrder.Stop(s.State, s.ToServer);
        Assert.Equal([SwingOpcode], s.Sent);

        MeleeAttackOrder.SwingAnswered(s.ToServer);

        Assert.Equal([SwingOpcode, StopOpcode], s.Sent);
        Assert.Equal(0, s.ToServer.PendingCount);
    }

    [Fact]
    public void StopThenSwingAtAnotherTarget_OnlyTheSwingGoesOut()
    {
        // The cMaNGOS target switch: the server must never see the stop wedged between two swings.
        var s = new Session();
        MeleeAttackOrder.Swing(s.State, s.ToServer, Wolf);
        MeleeAttackOrder.Stop(s.State, s.ToServer);
        MeleeAttackOrder.Swing(s.State, s.ToServer, Boar);
        MeleeAttackOrder.SwingAnswered(s.ToServer);

        Assert.Equal([SwingOpcode, SwingOpcode], s.Sent);
        Assert.Equal(Boar, s.State.CurrentAttackTarget);
    }

    [Fact]
    public void StopThenSwingAtTheSameTarget_NothingMoreGoesOut()
    {
        // The stop never reached the server, so it is still attacking the wolf.
        var s = new Session();
        MeleeAttackOrder.Swing(s.State, s.ToServer, Wolf);
        MeleeAttackOrder.Stop(s.State, s.ToServer);
        MeleeAttackOrder.Swing(s.State, s.ToServer, Wolf);
        MeleeAttackOrder.SwingAnswered(s.ToServer);

        Assert.Equal([SwingOpcode], s.Sent);
        Assert.Equal(Wolf, s.State.CurrentAttackTarget);
    }

    [Fact]
    public void StopPressedSeveralTimesWhileWaiting_SendsOneStop()
    {
        var s = new Session();
        MeleeAttackOrder.Swing(s.State, s.ToServer, Wolf);
        MeleeAttackOrder.Stop(s.State, s.ToServer);
        MeleeAttackOrder.Stop(s.State, s.ToServer);
        MeleeAttackOrder.Stop(s.State, s.ToServer);

        MeleeAttackOrder.SwingAnswered(s.ToServer);

        Assert.Equal([SwingOpcode, StopOpcode], s.Sent);
    }

    [Fact]
    public void ServerThatNeverAnswers_StillGetsTheStopAfterTheTimeout()
    {
        var s = new Session();
        MeleeAttackOrder.Swing(s.State, s.ToServer, Wolf);
        MeleeAttackOrder.Stop(s.State, s.ToServer);

        s.Time.Advance(MeleeAttackOrder.HeldStopTimeout);
        s.ToServer.Tick();

        Assert.Equal([SwingOpcode, StopOpcode], s.Sent);
        Assert.True(s.State.CurrentAttackTarget.IsEmpty());
    }

    [Fact]
    public void SwingError_IsAnAnswerToo_AndReleasesTheHeldStop()
    {
        var s = new Session();
        MeleeAttackOrder.Swing(s.State, s.ToServer, Wolf);
        MeleeAttackOrder.Stop(s.State, s.ToServer);

        MeleeAttackOrder.SwingAnswered(s.ToServer); // SMSG_ATTACKSWING_NOTINRANGE

        Assert.Equal([SwingOpcode, StopOpcode], s.Sent);
    }

    [Fact]
    public void ServerStoppedAttackingTheTarget_AttackingItAgainIsForwarded()
    {
        // Evade, death or leaving range: without forgetting the target, the next swing at the same
        // mob was dropped as a repeat.
        var s = new Session();
        MeleeAttackOrder.Swing(s.State, s.ToServer, Wolf);
        MeleeAttackOrder.SwingAnswered(s.ToServer);

        MeleeAttackOrder.ServerStopped(s.State, s.ToServer, Wolf);
        MeleeAttackOrder.Swing(s.State, s.ToServer, Wolf);

        Assert.Equal([SwingOpcode, SwingOpcode], s.Sent);
    }

    [Fact]
    public void ServerStopForTheOldTarget_DoesNotAnswerTheNewSwing()
    {
        var s = new Session();
        MeleeAttackOrder.Swing(s.State, s.ToServer, Wolf);
        MeleeAttackOrder.SwingAnswered(s.ToServer);
        MeleeAttackOrder.Swing(s.State, s.ToServer, Boar);
        MeleeAttackOrder.Stop(s.State, s.ToServer);

        MeleeAttackOrder.ServerStopped(s.State, s.ToServer, Wolf);
        Assert.Equal([SwingOpcode, SwingOpcode], s.Sent);

        MeleeAttackOrder.SwingAnswered(s.ToServer); // SMSG_ATTACK_START for the boar
        Assert.Equal([SwingOpcode, SwingOpcode, StopOpcode], s.Sent);
    }

    [Fact]
    public void CombatCancelled_DropsTheHeldStop_AndReopens()
    {
        var s = new Session();
        MeleeAttackOrder.Swing(s.State, s.ToServer, Wolf);
        MeleeAttackOrder.Stop(s.State, s.ToServer);

        MeleeAttackOrder.CombatCancelled(s.State, s.ToServer);
        Assert.Equal([SwingOpcode], s.Sent);
        Assert.True(s.State.CurrentAttackTarget.IsEmpty());

        MeleeAttackOrder.Stop(s.State, s.ToServer);
        Assert.Equal([SwingOpcode, StopOpcode], s.Sent);
    }
}
