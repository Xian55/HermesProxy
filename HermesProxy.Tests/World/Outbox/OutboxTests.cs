using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Framework.Constants;
using HermesProxy.World.Enums;
using HermesProxy.World.Outbox;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace HermesProxy.Tests.World.Outbox;

public class ClientOutboxOrderTests
{
    private static readonly Opcode Prepare = Opcode.SMSG_SPELL_PREPARE;
    private static readonly Opcode Start = Opcode.SMSG_SPELL_START;
    private static readonly Opcode Failed = Opcode.SMSG_CAST_FAILED;
    private static readonly Opcode Unlearn = Opcode.SMSG_SEND_UNLEARN_SPELLS;

    [Fact]
    public void Send_NoHolds_WritesInCallOrderAcrossBothConnections()
    {
        // The ad0122ee regression: SpellPrepare (realm) and SpellStart / CastFailed (instance)
        // written back to back must reach the wire in that order. Nothing in the outbox may
        // queue one connection's packets behind the other's.
        var wire = new RecordingClientWire();
        var outbox = OutboxTestExtensions.InWorld(wire);

        outbox.Send(new TestServerPacket(Prepare, 1, ConnectionType.Realm));
        outbox.SendOn(ConnectionType.Instance, new TestServerPacket(Start, 2, ConnectionType.Instance));
        outbox.Send(new TestServerPacket(Failed, 3, ConnectionType.Instance));
        outbox.SendOn(ConnectionType.Instance, new TestServerPacket(Prepare, 4, ConnectionType.Realm));

        Assert.Equal([1, 2, 3, 4], wire.Ids);
        Assert.Equal(
            [ConnectionType.Realm, ConnectionType.Instance, ConnectionType.Instance, ConnectionType.Instance],
            wire.Writes.Select(w => w.On).ToArray());
        Assert.All(wire.Writes, w => Assert.Equal(Environment.CurrentManagedThreadId, w.ThreadId));
        Assert.Equal(0, outbox.PendingCount);
    }

    [Fact]
    public void After_ReleasesRightAfterTheTriggerPacket_BeforeSendReturns()
    {
        var wire = new RecordingClientWire();
        var outbox = OutboxTestExtensions.InWorld(wire);

        outbox.After(Unlearn, new TestServerPacket(Failed, 2));
        Assert.Empty(wire.Writes);

        outbox.Send(new TestServerPacket(Unlearn, 1));

        Assert.Equal([1, 2], wire.Ids);
        Assert.Equal(0, outbox.PendingCount);
    }

    [Fact]
    public void After_WaitsForTheNextOccurrence_NotOneThatAlreadyHappened()
    {
        var wire = new RecordingClientWire();
        var outbox = OutboxTestExtensions.InWorld(wire);

        outbox.Send(new TestServerPacket(Unlearn, 1));
        outbox.After(Unlearn, new TestServerPacket(Failed, 2));
        Assert.Equal([1], wire.Ids);

        outbox.Send(new TestServerPacket(Unlearn, 3));
        Assert.Equal([1, 3, 2], wire.Ids);
    }

    [Fact]
    public void After_SeveralHoldsOnOneTrigger_ReleaseInRegistrationOrder()
    {
        var wire = new RecordingClientWire();
        var outbox = OutboxTestExtensions.InWorld(wire);

        outbox.After(Unlearn, new TestServerPacket(Failed, 2));
        outbox.After(Unlearn, new TestServerPacket(Start, 3));
        outbox.After(Unlearn, new TestServerPacket(Prepare, 4));

        outbox.Send(new TestServerPacket(Unlearn, 1));

        Assert.Equal([1, 2, 3, 4], wire.Ids);
    }

    [Fact]
    public void ReleaseChain_IsBreadthFirst_EarlierTriggerFirst()
    {
        var wire = new RecordingClientWire();
        var outbox = OutboxTestExtensions.InWorld(wire);

        outbox.After(Unlearn, new TestServerPacket(Prepare, 2)); // releases Prepare...
        outbox.After(Unlearn, new TestServerPacket(Failed, 3));
        outbox.After(Prepare, new TestServerPacket(Start, 4));   // ...which releases this

        outbox.Send(new TestServerPacket(Unlearn, 1));

        Assert.Equal([1, 2, 3, 4], wire.Ids);
    }

    [Fact]
    public void HoldAddedDuringARelease_WaitsForTheNextTrigger()
    {
        var wire = new RecordingClientWire();
        var outbox = OutboxTestExtensions.InWorld(wire);

        outbox.After(Unlearn, () => outbox.After(Unlearn, new TestServerPacket(Failed, 9)));

        outbox.Send(new TestServerPacket(Unlearn, 1));
        Assert.Equal([1], wire.Ids);
        Assert.Equal(1, outbox.PendingCount);

        outbox.Send(new TestServerPacket(Unlearn, 2));
        Assert.Equal([1, 2, 9], wire.Ids);
    }

    [Fact]
    public void AfterBatch_ReleasedByTheBatchEndSignal()
    {
        var wire = new RecordingClientWire();
        var outbox = OutboxTestExtensions.InWorld(wire);

        outbox.AfterBatch(new TestServerPacket(Failed, 2));
        outbox.Send(new TestServerPacket(Start, 1));
        Assert.Equal([1], wire.Ids);

        outbox.Notify(OutboxEvent.Signal(OutboxSignal.UpdateBatchEnd));
        Assert.Equal([1, 2], wire.Ids);
    }

    [Fact]
    public void Callouts_NeverRunUnderTheOutboxLock()
    {
        // If the release path held the lock while writing, a second thread registering a hold
        // would block until the write returned, and this write waits for that thread: deadlock.
        var wire = new RecordingClientWire();
        var outbox = OutboxTestExtensions.InWorld(wire);
        bool otherThreadGotIn = false;

        wire.OnWrite = packet =>
        {
            if (packet.Id != 2)
                return;
            var other = Task.Run(() => outbox.After(Prepare, new TestServerPacket(Failed, 3)));
            otherThreadGotIn = other.Wait(TimeSpan.FromSeconds(10));
        };

        outbox.After(Unlearn, new TestServerPacket(Start, 2));
        outbox.Send(new TestServerPacket(Unlearn, 1));

        Assert.True(otherThreadGotIn);
    }

    [Fact]
    public void ThrowingContinuation_IsIsolated_LaterHoldsStillRelease()
    {
        var wire = new RecordingClientWire();
        var outbox = OutboxTestExtensions.InWorld(wire);

        outbox.After(Unlearn, () => throw new InvalidOperationException("handler bug"));
        outbox.After(Unlearn, new TestServerPacket(Failed, 2));

        outbox.Send(new TestServerPacket(Unlearn, 1));

        Assert.Equal([1, 2], wire.Ids);
        Assert.Equal(0, outbox.PendingCount);
    }

    [Fact]
    public void ConcurrentHoldsAndTriggers_EveryHoldReleasedExactlyOnce()
    {
        const int producers = 4;
        const int holdsPerProducer = 500;
        var wire = new RecordingClientWire();
        var outbox = OutboxTestExtensions.InWorld(wire);
        int registered = 0;

        var trigger = Task.Run(() =>
        {
            int id = 1_000_000;
            while (Volatile.Read(ref registered) < producers * holdsPerProducer)
                outbox.Send(new TestServerPacket(Unlearn, id++));
        });

        Parallel.For(0, producers, p =>
        {
            for (int i = 0; i < holdsPerProducer; i++)
            {
                outbox.After(Unlearn, new TestServerPacket(Failed, p * holdsPerProducer + i));
                Interlocked.Increment(ref registered);
            }
        });

        trigger.Wait(TimeSpan.FromSeconds(30));
        outbox.Send(new TestServerPacket(Unlearn, 2_000_000));

        int[] released = wire.Writes.Where(w => w.Packet.GetUniversalOpcode() == Failed).Select(w => w.Packet.Id).ToArray();
        Assert.Equal(producers * holdsPerProducer, released.Length);
        Assert.Equal(producers * holdsPerProducer, released.Distinct().Count());
        Assert.Equal(0, outbox.PendingCount);
    }
}

public class OutboxGateAndEventTests
{
    [Fact]
    public void Gate_Closed_HoldsUntilOpened()
    {
        var wire = new RecordingServerWire();
        var outbox = new ServerOutbox(wire);
        var nameQuery = OutboxTestExtensions.Legacy(0x50);

        outbox.When(OutboxGate.InWorld, nameQuery);
        Assert.Empty(wire.Writes);

        outbox.SetGate(OutboxGate.InWorld, open: true);

        Assert.Same(nameQuery, Assert.Single(wire.Writes).Packet);
    }

    [Fact]
    public void Gate_AlreadyOpen_SendsImmediately_WithoutHolding()
    {
        var wire = new RecordingServerWire();
        var outbox = new ServerOutbox(wire);
        outbox.SetGate(OutboxGate.InWorld, open: true);

        outbox.When(OutboxGate.InWorld, OutboxTestExtensions.Legacy(0x50));

        Assert.Single(wire.Writes);
        Assert.Equal(0, outbox.PendingCount);
    }

    [Fact]
    public void Gate_CheckAndHold_IsAtomicAgainstAConcurrentOpen()
    {
        // The IsInWorld race this replaces: check on one thread, flip on another, and the hold
        // lands after the flip and waits forever. Here it must always go out.
        for (int round = 0; round < 200; round++)
        {
            var wire = new RecordingServerWire();
            var outbox = new ServerOutbox(wire);
            using var start = new ManualResetEventSlim();

            var holder = Task.Run(() =>
            {
                start.Wait();
                for (int i = 0; i < 20; i++)
                    outbox.When(OutboxGate.InWorld, OutboxTestExtensions.Legacy(0x50));
            });
            var opener = Task.Run(() =>
            {
                start.Wait();
                outbox.SetGate(OutboxGate.InWorld, open: true);
            });

            start.Set();
            Task.WaitAll([holder, opener], TimeSpan.FromSeconds(10));

            Assert.Equal(20, wire.Writes.Length);
            Assert.Equal(0, outbox.PendingCount);
        }
    }

    [Fact]
    public void WhenAll_ReleasesOnlyAfterEveryEvent_DuplicatesCountOnce()
    {
        var outbox = new ServerOutbox(new RecordingServerWire());
        int released = 0;

        outbox.WhenAll(
            [OutboxEvent.ItemTemplate(10), OutboxEvent.ItemTemplate(20), OutboxEvent.ItemTemplate(10)],
            () => released++);

        outbox.Notify(OutboxEvent.ItemTemplate(10));
        outbox.Notify(OutboxEvent.ItemTemplate(10));
        outbox.Notify(OutboxEvent.ItemTemplate(99));
        Assert.Equal(0, released);

        outbox.Notify(OutboxEvent.ItemTemplate(20));
        Assert.Equal(1, released);
        Assert.Equal(0, outbox.PendingCount);
    }

    [Fact]
    public void AfterHandled_ReleasedByTheHandledOpcode()
    {
        var wire = new RecordingServerWire();
        var outbox = new ServerOutbox(wire);

        outbox.AfterHandled(Opcode.SMSG_LOGIN_VERIFY_WORLD, OutboxTestExtensions.Legacy(0x50));
        outbox.Notify(OutboxEvent.OpcodeHandled(Opcode.SMSG_UPDATE_OBJECT));
        Assert.Empty(wire.Writes);

        outbox.Notify(OutboxEvent.OpcodeHandled(Opcode.SMSG_LOGIN_VERIFY_WORLD));
        Assert.Single(wire.Writes);
    }

    [Fact]
    public void Notify_WithNothingHeld_DoesNothing()
    {
        var outbox = new ServerOutbox(new RecordingServerWire());
        outbox.Notify(OutboxEvent.Signal(OutboxSignal.UpdateBatchEnd));
        Assert.Equal(0, outbox.PendingCount);
    }
}

public class OutboxTimeTests
{
    [Fact]
    public void DefaultTimeout_DropsTheHold_NeverSendsIt()
    {
        var time = new FakeTimeProvider();
        var wire = new RecordingServerWire();
        var outbox = new ServerOutbox(wire, time);
        var packet = OutboxTestExtensions.Legacy(0x50);

        outbox.When(OutboxGate.InWorld, packet);
        time.Advance(TimeSpan.FromSeconds(59));
        Assert.False(packet.IsDisposed());

        time.Advance(TimeSpan.FromSeconds(2));

        Assert.Empty(wire.Writes);
        Assert.True(packet.IsDisposed());
        Assert.Equal(0, outbox.PendingCount);
    }

    [Fact]
    public void Timeout_Release_ServerPacketGoesOutFromTheTimer()
    {
        var time = new FakeTimeProvider();
        var wire = new RecordingServerWire();
        var outbox = new ServerOutbox(wire, time);

        outbox.When(OutboxGate.InWorld, OutboxTestExtensions.Legacy(0x50),
            new HoldOptions(Timeout: TimeSpan.FromSeconds(5), OnTimeout: OutboxTimeoutAction.Release));

        time.Advance(TimeSpan.FromSeconds(5));

        Assert.Single(wire.Writes);
    }

    [Fact]
    public void Timeout_Release_ClientPacketWaitsForTick()
    {
        // Writing a client packet can read session state, so the timer thread may not do it.
        var time = new FakeTimeProvider();
        var wire = new RecordingClientWire();
        var outbox = OutboxTestExtensions.InWorld(wire, time);

        outbox.After(Opcode.SMSG_SEND_UNLEARN_SPELLS, new TestServerPacket(Opcode.SMSG_CAST_FAILED, 7),
            new HoldOptions(Timeout: TimeSpan.FromSeconds(5), OnTimeout: OutboxTimeoutAction.Release));

        time.Advance(TimeSpan.FromSeconds(6));
        Assert.Empty(wire.Writes);

        outbox.Tick();

        Assert.Equal([7], wire.Ids);
        Assert.Equal(Environment.CurrentManagedThreadId, wire.Writes[0].ThreadId);
    }

    [Fact]
    public void Tick_BeforeAnyDeadline_DoesNothing()
    {
        var time = new FakeTimeProvider();
        var wire = new RecordingClientWire();
        var outbox = OutboxTestExtensions.InWorld(wire, time);

        outbox.AfterBatch(new TestServerPacket(Opcode.SMSG_CAST_FAILED, 1),
            new HoldOptions(Timeout: TimeSpan.FromSeconds(5), OnTimeout: OutboxTimeoutAction.Release));
        time.Advance(TimeSpan.FromSeconds(1));
        outbox.Tick();

        Assert.Empty(wire.Writes);
        Assert.Equal(1, outbox.PendingCount);
    }

    [Fact]
    public void LateEventAfterTimeout_IsANoOp()
    {
        var time = new FakeTimeProvider();
        var wire = new RecordingServerWire();
        var outbox = new ServerOutbox(wire, time);

        outbox.AfterHandled(Opcode.SMSG_LOGIN_VERIFY_WORLD, OutboxTestExtensions.Legacy(0x50),
            new HoldOptions(Timeout: TimeSpan.FromSeconds(1), OnTimeout: OutboxTimeoutAction.Release));
        time.Advance(TimeSpan.FromSeconds(1));
        Assert.Single(wire.Writes);

        outbox.Notify(OutboxEvent.OpcodeHandled(Opcode.SMSG_LOGIN_VERIFY_WORLD));

        Assert.Single(wire.Writes);
    }

    [Fact]
    public void Delay_ServerPacket_WrittenWhenDue()
    {
        var time = new FakeTimeProvider();
        var wire = new RecordingServerWire();
        var outbox = new ServerOutbox(wire, time);

        outbox.Delay(TimeSpan.FromMilliseconds(500), OutboxTestExtensions.Legacy(0x50));
        time.Advance(TimeSpan.FromMilliseconds(499));
        Assert.Empty(wire.Writes);

        time.Advance(TimeSpan.FromMilliseconds(1));
        Assert.Single(wire.Writes);
    }

    [Fact]
    public void Paced_FirstImmediately_RestSpacedApartInOrder()
    {
        var time = new FakeTimeProvider();
        var wire = new RecordingServerWire();
        var outbox = new ServerOutbox(wire, time);
        var key = new HoldKey(HoldKeyKind.Test, 1);
        var mails = Enumerable.Range(0, 3).Select(i => OutboxTestExtensions.Legacy(0x238u + (uint)i)).ToArray();

        foreach (var mail in mails)
            outbox.Paced(key, TimeSpan.FromMilliseconds(500), mail);

        Assert.Same(mails[0], Assert.Single(wire.Writes).Packet);

        time.Advance(TimeSpan.FromMilliseconds(500));
        Assert.Equal(2, wire.Writes.Length);

        time.Advance(TimeSpan.FromMilliseconds(500));
        Assert.Equal(mails, wire.Writes.Select(w => w.Packet).ToArray());
    }

    [Fact]
    public void Paced_ANewPacketNeverOvertakesOneStillWaiting()
    {
        // The client direction releases due holds only on Tick, so a slot can pass with its
        // packet still held. A later packet must queue behind it rather than go out first.
        var time = new FakeTimeProvider();
        var wire = new RecordingClientWire();
        var outbox = OutboxTestExtensions.InWorld(wire, time);
        var key = new HoldKey(HoldKeyKind.Test, 2);

        outbox.Paced(key, TimeSpan.FromMilliseconds(100), new TestServerPacket(Opcode.SMSG_CAST_FAILED, 1));
        outbox.Paced(key, TimeSpan.FromMilliseconds(100), new TestServerPacket(Opcode.SMSG_CAST_FAILED, 2));
        time.Advance(TimeSpan.FromMilliseconds(500));
        outbox.Paced(key, TimeSpan.FromMilliseconds(100), new TestServerPacket(Opcode.SMSG_CAST_FAILED, 3));
        Assert.Equal([1], wire.Ids);

        outbox.Tick();
        time.Advance(TimeSpan.FromMilliseconds(500));
        outbox.Tick();

        Assert.Equal([1, 2, 3], wire.Ids);
    }

    [Fact]
    public void Coalesce_NewestOfferWins_ReleasedOnceAfterTheWindow()
    {
        var time = new FakeTimeProvider();
        var outbox = new ServerOutbox(new RecordingServerWire(), time);
        var key = new HoldKey(HoldKeyKind.Test, 3);
        var releases = new ConcurrentQueue<int>();

        var stateFree = new HoldOptions(RunOnTimer: true);

        outbox.Coalesce(key, TimeSpan.FromMilliseconds(100), () => releases.Enqueue(1), stateFree);
        time.Advance(TimeSpan.FromMilliseconds(60));
        outbox.Coalesce(key, TimeSpan.FromMilliseconds(100), () => releases.Enqueue(2), stateFree);
        outbox.Coalesce(key, TimeSpan.FromMilliseconds(100), () => releases.Enqueue(3), stateFree);
        Assert.Empty(releases);

        // Deadline is fixed by the first offer, so later offers cannot starve the release.
        time.Advance(TimeSpan.FromMilliseconds(40));

        Assert.Equal([3], releases.ToArray());
        Assert.Equal(0, outbox.PendingCount);
    }

    [Fact]
    public void DeadlineContinuation_WithoutRunOnTimer_WaitsForTick()
    {
        var time = new FakeTimeProvider();
        var outbox = new ServerOutbox(new RecordingServerWire(), time);
        int runOn = 0;

        outbox.Delay(TimeSpan.FromMilliseconds(100), () => runOn = Environment.CurrentManagedThreadId);
        time.Advance(TimeSpan.FromMilliseconds(200));
        Assert.Equal(0, runOn);

        outbox.Tick();

        Assert.Equal(Environment.CurrentManagedThreadId, runOn);
    }
}

public class OutboxLifetimeTests
{
    [Fact]
    public void Cancel_DropsTheKeyedHold_AndReportsIt()
    {
        var wire = new RecordingServerWire();
        var outbox = new ServerOutbox(wire);
        var key = new HoldKey(HoldKeyKind.Test, 5);
        var packet = OutboxTestExtensions.Legacy(0x50);

        outbox.When(OutboxGate.InWorld, packet, new HoldOptions(Key: key));

        Assert.True(outbox.Cancel(key));
        Assert.False(outbox.Cancel(key));
        Assert.True(packet.IsDisposed());

        outbox.SetGate(OutboxGate.InWorld, open: true);
        Assert.Empty(wire.Writes);
    }

    [Fact]
    public void Release_SendsTheKeyedHoldNow()
    {
        var wire = new RecordingServerWire();
        var outbox = new ServerOutbox(wire);
        var key = new HoldKey(HoldKeyKind.Test, 6);

        outbox.When(OutboxGate.InWorld, OutboxTestExtensions.Legacy(0x50), new HoldOptions(Key: key));

        Assert.True(outbox.Release(key));
        Assert.Single(wire.Writes);
        Assert.Equal(0, outbox.PendingCount);
    }

    [Fact]
    public void Discard_DropsOnlyTheMatchingScope()
    {
        var wire = new RecordingServerWire();
        var outbox = new ServerOutbox(wire);
        var character = OutboxTestExtensions.Legacy(0x50);
        var connection = OutboxTestExtensions.Legacy(0x51);

        outbox.When(OutboxGate.InWorld, character, new HoldOptions(Scope: OutboxScope.GameState));
        outbox.When(OutboxGate.InWorld, connection, new HoldOptions(Scope: OutboxScope.LegacyConnection));

        outbox.Discard(OutboxScope.GameState);

        Assert.True(character.IsDisposed());
        Assert.False(connection.IsDisposed());
        Assert.Equal(1, outbox.PendingCount);

        outbox.SetGate(OutboxGate.InWorld, open: true);
        Assert.Same(connection, Assert.Single(wire.Writes).Packet);
    }

    [Fact]
    public void Discard_Session_DropsEverything_AndClosesGates()
    {
        var wire = new RecordingServerWire();
        var outbox = new ServerOutbox(wire);
        outbox.SetGate(OutboxGate.InWorld, open: true);
        outbox.AfterHandled(Opcode.SMSG_LOGIN_VERIFY_WORLD, OutboxTestExtensions.Legacy(0x50), new HoldOptions(Scope: OutboxScope.Session));

        outbox.Discard(OutboxScope.Session);

        Assert.Equal(0, outbox.PendingCount);
        Assert.False(outbox.IsGateOpen(OutboxGate.InWorld));
    }

    [Fact]
    public void Overflow_RefusesHoldsPastTheCap_CallbackRunsOncePerEpisode()
    {
        var wire = new RecordingServerWire();
        int overflows = 0;
        var outbox = new ServerOutbox(wire, options: new OutboxOptions { MaxPendingHolds = 2 }, onOverflow: () => overflows++);
        var refused = OutboxTestExtensions.Legacy(0x52);

        outbox.When(OutboxGate.InWorld, OutboxTestExtensions.Legacy(0x50));
        outbox.When(OutboxGate.InWorld, OutboxTestExtensions.Legacy(0x51));
        outbox.When(OutboxGate.InWorld, refused);
        outbox.When(OutboxGate.InWorld, OutboxTestExtensions.Legacy(0x53));

        Assert.Equal(2, outbox.PendingCount);
        Assert.True(refused.IsDisposed());
        Assert.Equal(1, overflows);
    }

    [Fact]
    public void AfterDispose_HoldsAreDropped_AndNothingIsSent()
    {
        var wire = new RecordingServerWire();
        var outbox = new ServerOutbox(wire);
        var waiting = OutboxTestExtensions.Legacy(0x50);
        outbox.When(OutboxGate.InWorld, waiting);

        outbox.Dispose();
        var late = OutboxTestExtensions.Legacy(0x51);
        outbox.When(OutboxGate.InWorld, late);
        outbox.SetGate(OutboxGate.InWorld, open: true);

        Assert.True(waiting.IsDisposed());
        Assert.True(late.IsDisposed());
        Assert.Empty(wire.Writes);
    }
}
