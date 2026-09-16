using System;
using System.Collections.Generic;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Outbox;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace HermesProxy.Tests.World.Outbox;

/// <summary>
/// Holds that carry state, and claiming them: the shapes the slice C data-dependency holds use
/// (deferred player batch, pet batches, pet spell bar, corpse destroy, mail list).
/// </summary>
public class OutboxClaimTests
{
    private sealed record Work(int Id);

    private static readonly HoldKey Key = new(HoldKeyKind.Test, 1);
    private static readonly OutboxEvent PlayerKnown = OutboxEvent.GuidKnown(WowGuid128.Create(HighGuidType703.Player, 1));
    private static readonly OutboxEvent BatchEnd = OutboxEvent.Signal(OutboxSignal.UpdateBatchEnd);

    [Fact]
    public void StateHold_ReleasedByItsEvent_RunsWithItsState()
    {
        var outbox = OutboxTestExtensions.InWorld(new RecordingClientWire());
        List<int> ran = [];

        outbox.When(PlayerKnown, new Work(7), w => ran.Add(w.Id), new HoldOptions(Key: Key));
        Assert.Empty(ran);

        outbox.Notify(PlayerKnown);

        Assert.Equal([7], ran);
        Assert.Null(outbox.Peek<Work>(Key));
        Assert.Equal(0, outbox.PendingCount);
    }

    [Fact]
    public void Claim_TakesTheHold_SoItsEventNoLongerReleasesIt()
    {
        // A pet batch merged into the player's deferred batch must not also go out on its own.
        var outbox = OutboxTestExtensions.InWorld(new RecordingClientWire());
        List<int> ran = [];
        outbox.When(PlayerKnown, new Work(1), w => ran.Add(w.Id), new HoldOptions(Key: Key));

        var held = outbox.Peek<Work>(Key);
        Assert.NotNull(held);
        Assert.True(outbox.Claim(Key, held));

        outbox.Notify(PlayerKnown);

        Assert.Empty(ran);
        Assert.Equal(0, outbox.PendingCount);
    }

    [Fact]
    public void Claim_AfterTheHoldReleased_ReturnsFalse()
    {
        var outbox = OutboxTestExtensions.InWorld(new RecordingClientWire());
        var work = new Work(1);
        outbox.When(PlayerKnown, work, _ => { }, new HoldOptions(Key: Key));

        outbox.Notify(PlayerKnown);

        Assert.False(outbox.Claim(Key, work));
    }

    [Fact]
    public void Claim_TakesOnlyTheStateThatWasPeeked()
    {
        // A newer spell bar registered after the peek replaces the old one; the claim for the old
        // one must fail rather than take the new one.
        var outbox = OutboxTestExtensions.InWorld(new RecordingClientWire());
        List<int> ran = [];
        outbox.Delay(TimeSpan.FromSeconds(10), new Work(1), w => ran.Add(w.Id), new HoldOptions(Key: Key));
        var peeked = outbox.Peek<Work>(Key)!;

        outbox.Cancel(Key);
        outbox.Delay(TimeSpan.FromSeconds(10), new Work(2), w => ran.Add(w.Id), new HoldOptions(Key: Key));

        Assert.False(outbox.Claim(Key, peeked));
        Assert.Equal(2, outbox.Peek<Work>(Key)!.Id);
        Assert.Equal(1, outbox.PendingCount);
    }

    [Fact]
    public void PeekAndClaimLoop_TakesEveryHold_OldestFirst()
    {
        var outbox = OutboxTestExtensions.InWorld(new RecordingClientWire());
        for (int i = 1; i <= 3; i++)
            outbox.When(PlayerKnown, new Work(i), _ => { }, new HoldOptions(Key: Key));

        List<int> claimed = [];
        while (outbox.Peek<Work>(Key) is { } held && outbox.Claim(Key, held))
            claimed.Add(held.Id);

        Assert.Equal([1, 2, 3], claimed);
        Assert.Equal(0, outbox.PendingCount);
    }

    [Fact]
    public void Peek_IgnoresHoldsWithoutStateOrOfAnotherType()
    {
        var outbox = OutboxTestExtensions.InWorld(new RecordingClientWire());
        outbox.When(PlayerKnown, () => { }, new HoldOptions(Key: Key));
        outbox.When(PlayerKnown, "text", _ => { }, new HoldOptions(Key: Key));

        Assert.Null(outbox.Peek<Work>(Key));
        Assert.Equal("text", outbox.Peek<string>(Key));
    }

    [Fact]
    public void DelayedState_NobodyClaims_ReleasedByTickAtTheDeadline()
    {
        // The pet spell bar whose pet create never comes still goes out, from a session thread.
        var time = new FakeTimeProvider();
        var outbox = OutboxTestExtensions.InWorld(new RecordingClientWire(), time);
        List<(int Id, int Thread)> ran = [];
        outbox.Delay(TimeSpan.FromSeconds(10), new Work(4), w => ran.Add((w.Id, Environment.CurrentManagedThreadId)),
            new HoldOptions(Key: Key));

        time.Advance(TimeSpan.FromSeconds(10));
        Assert.Empty(ran);

        outbox.Tick();

        Assert.Equal([(4, Environment.CurrentManagedThreadId)], ran);
    }

    [Fact]
    public void Discard_GameState_DropsStateHolds()
    {
        var outbox = OutboxTestExtensions.InWorld(new RecordingClientWire());
        List<int> ran = [];
        outbox.When(PlayerKnown, new Work(1), w => ran.Add(w.Id), new HoldOptions(Key: Key));

        outbox.Discard(OutboxScope.GameState);
        outbox.Notify(PlayerKnown);

        Assert.Empty(ran);
        Assert.Null(outbox.Peek<Work>(Key));
    }

    [Fact]
    public void WaitForSeveralItemTemplates_ReleasesOnTheLast_OrSendsAnywayAtTheTimeout()
    {
        var time = new FakeTimeProvider();
        var outbox = OutboxTestExtensions.InWorld(new RecordingClientWire(), time);
        int released = 0;
        var hold = new HoldOptions(Timeout: TimeSpan.FromSeconds(10), OnTimeout: OutboxTimeoutAction.Release);

        outbox.WhenAll([OutboxEvent.ItemTemplate(100), OutboxEvent.ItemTemplate(200)], () => released++, hold);
        outbox.Notify(OutboxEvent.ItemTemplate(100));
        outbox.Notify(OutboxEvent.ItemTemplate(100));
        Assert.Equal(0, released);
        outbox.Notify(OutboxEvent.ItemTemplate(200));
        Assert.Equal(1, released);

        // A server that never answers one of the queries.
        outbox.WhenAll([OutboxEvent.ItemTemplate(300), OutboxEvent.ItemTemplate(400)], () => released++, hold);
        outbox.Notify(OutboxEvent.ItemTemplate(300));
        time.Advance(TimeSpan.FromSeconds(10));
        outbox.Tick();
        Assert.Equal(2, released);

        // Its late answer changes nothing.
        outbox.Notify(OutboxEvent.ItemTemplate(400));
        Assert.Equal(2, released);
    }

    [Fact]
    public void CorpseDestroy_CancelledByACreateInTheSameBatch_OtherwiseSentAtBatchEnd()
    {
        var outbox = OutboxTestExtensions.InWorld(new RecordingClientWire());
        List<ulong> destroyed = [];
        var corpseA = new HoldKey(HoldKeyKind.CorpseDestroy, 10);
        var corpseB = new HoldKey(HoldKeyKind.CorpseDestroy, 11);

        outbox.When(BatchEnd, () => destroyed.Add(10), new HoldOptions(Key: corpseA));
        outbox.When(BatchEnd, () => destroyed.Add(11), new HoldOptions(Key: corpseB));

        // The batch recreates corpse A: its destroy and the create both go.
        Assert.True(outbox.Cancel(corpseA));
        outbox.Notify(BatchEnd);

        Assert.Equal([11ul], destroyed);
        Assert.False(outbox.Cancel(corpseB));
    }

    [Fact]
    public void MailList_ANewerListReplacesTheWaitingOne_SoTheOldOneIsNeverSent()
    {
        // The old PendingMailListPacket was never cleared: a later text answer sent a stale list
        // after the newer one.
        var outbox = OutboxTestExtensions.InWorld(new RecordingClientWire());
        List<string> sent = [];
        var mailList = new HoldKey(HoldKeyKind.MailList);

        outbox.Cancel(mailList);
        outbox.WhenAll([OutboxEvent.ItemText(1)], () => sent.Add("old"), new HoldOptions(Key: mailList));

        // The newer list needs no texts, so it goes out at once.
        outbox.Cancel(mailList);
        sent.Add("new");

        outbox.Notify(OutboxEvent.ItemText(1));

        Assert.Equal(["new"], sent);
        Assert.Equal(0, outbox.PendingCount);
    }

    [Fact]
    public void PlayerKnownRaisedEveryBatch_ReleasesAHoldRegisteredAfterTheFirstRaise()
    {
        // The toy sync can be requested after the batch that made the player known; it goes out at
        // the next batch end rather than waiting forever.
        var outbox = OutboxTestExtensions.InWorld(new RecordingClientWire());
        int synced = 0;

        outbox.Notify(PlayerKnown);
        outbox.When(PlayerKnown, () => synced++, new HoldOptions(Key: new HoldKey(HoldKeyKind.ToysSync)));
        Assert.Equal(0, synced);

        outbox.Notify(PlayerKnown);

        Assert.Equal(1, synced);
    }
}
