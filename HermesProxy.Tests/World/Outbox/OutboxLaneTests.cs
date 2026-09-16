using System;
using System.Collections.Generic;
using HermesProxy.World.Outbox;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace HermesProxy.Tests.World.Outbox;

public class OutboxLaneTests
{
    private static readonly HoldKey Lane = new(HoldKeyKind.Test, 42);

    [Fact]
    public void Exclusive_RunsNowWhenFree_OthersWaitAndRunInOrder()
    {
        var outbox = new ServerOutbox(new RecordingServerWire());
        var ran = new List<int>();

        outbox.Exclusive(Lane, () => ran.Add(1));
        outbox.Exclusive(Lane, () => ran.Add(2));
        outbox.Exclusive(Lane, () => ran.Add(3));
        Assert.Equal([1], ran);

        outbox.LaneDone(Lane);
        Assert.Equal([1, 2], ran);

        outbox.LaneDone(Lane);
        outbox.LaneDone(Lane);
        Assert.Equal([1, 2, 3], ran);

        outbox.Exclusive(Lane, () => ran.Add(4));
        Assert.Equal([1, 2, 3, 4], ran);
    }

    [Fact]
    public void LaneWaiter_ThatTimesOut_IsDropped_NeverStarted()
    {
        var time = new FakeTimeProvider();
        var outbox = new ServerOutbox(new RecordingServerWire(), time);
        var ran = new List<int>();

        outbox.Exclusive(Lane, () => ran.Add(1));
        outbox.Exclusive(Lane, () => ran.Add(2), new HoldOptions(Timeout: TimeSpan.FromSeconds(1)));
        time.Advance(TimeSpan.FromSeconds(2));

        outbox.LaneDone(Lane);
        Assert.Equal([1], ran);

        outbox.Exclusive(Lane, () => ran.Add(3));
        Assert.Equal([1, 3], ran);
    }

    [Fact]
    public void DiscardingTheHoldersScope_FreesTheLane()
    {
        var outbox = new ServerOutbox(new RecordingServerWire());
        var ran = new List<int>();

        outbox.Exclusive(Lane, () => ran.Add(1), new HoldOptions(Scope: OutboxScope.GameState));
        outbox.Discard(OutboxScope.GameState);
        outbox.Exclusive(Lane, () => ran.Add(2));

        Assert.Equal([1, 2], ran);
    }
}
