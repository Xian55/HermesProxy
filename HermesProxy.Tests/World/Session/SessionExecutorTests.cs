using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HermesProxy.World.Session;
using Xunit;

namespace HermesProxy.Tests.World.Session;

public class SessionExecutorTests
{
    private sealed class RecordingHost : ISessionExecutorHost
    {
        public int Pauses;
        public int Resumes;
        public int Overflows;

        public void PauseReads() => Interlocked.Increment(ref Pauses);
        public void ResumeReads() => Interlocked.Increment(ref Resumes);
        public void Overflowed() => Interlocked.Increment(ref Overflows);
    }

    /// <summary>Counts any moment two threads are inside an event at once.</summary>
    private sealed class OwnershipCounter
    {
        private int _inside;
        public int Violations;

        public IDisposable Enter()
        {
            if (Interlocked.Increment(ref _inside) != 1)
                Interlocked.Increment(ref Violations);
            return new Exit(this);
        }

        private sealed class Exit(OwnershipCounter owner) : IDisposable
        {
            public void Dispose() => Interlocked.Decrement(ref owner._inside);
        }
    }

    [Fact]
    public void PostOnAFreeExecutor_RunsInlineBeforeItReturns()
    {
        using var executor = new SessionExecutor("test");
        int? ranOn = null;

        executor.Post(_ => ranOn = Environment.CurrentManagedThreadId);

        Assert.Equal(Environment.CurrentManagedThreadId, ranOn);
    }

    [Fact]
    public void PostFromInsideAnEvent_RunsAfterTheCurrentOneFinishes()
    {
        using var executor = new SessionExecutor("test");
        var order = new List<string>();

        executor.Post(_ =>
        {
            order.Add("outer start");
            executor.Post(_ => order.Add("inner"));
            order.Add("outer end");
        });

        Assert.Equal(["outer start", "outer end", "inner"], order);
    }

    [Fact]
    public void WhileTheOwnerIsBusy_APostFromAnotherThreadQueuesInsteadOfRunning()
    {
        using var executor = new SessionExecutor("test");
        using var ownerInside = new ManualResetEventSlim();
        using var letOwnerFinish = new ManualResetEventSlim();
        using var otherFinished = new ManualResetEventSlim();
        // Appended on the executor thread and read on this one, so it cannot be a List: an
        // assertion that enumerates one mid-Add throws "Collection was modified", which is what
        // this test did on a loaded CI runner while passing every time on a quiet dev box.
        var ran = new ConcurrentQueue<string>();

        var owner = Task.Run(() => executor.Post(_ =>
        {
            ran.Enqueue("owner");
            ownerInside.Set();
            letOwnerFinish.Wait();
        }));

        Assert.True(ownerInside.Wait(TimeSpan.FromSeconds(5)));

        bool posterReturnedFirst = false;
        var poster = Task.Run(() =>
        {
            executor.Post(_ =>
            {
                ran.Enqueue("other");
                otherFinished.Set();
            });
            posterReturnedFirst = !letOwnerFinish.IsSet;
        });

        Assert.True(poster.Wait(TimeSpan.FromSeconds(5)));
        Assert.True(posterReturnedFirst);
        Assert.Equal<string>(["owner"], ran);

        letOwnerFinish.Set();
        Assert.True(owner.Wait(TimeSpan.FromSeconds(5)));
        // Wait for the callback to finish rather than merely to leave the queue. QueueDepth
        // reaches zero when the item is dequeued, which is before its body has appended
        // anything — that gap is the race, and SpinWait on it is what made this flaky.
        Assert.True(otherFinished.Wait(TimeSpan.FromSeconds(5)));
        Assert.Equal<string>(["owner", "other"], ran);
    }

    [Fact]
    public void ManyProducers_KeepEachProducersOrder_AndNeverRunTogether()
    {
        using var executor = new SessionExecutor("test");
        var ownership = new OwnershipCounter();
        var seen = new ConcurrentQueue<(int Producer, int Sequence)>();
        const int producers = 4;
        const int perProducer = 2000;

        Parallel.For(0, producers, producer =>
        {
            for (int i = 0; i < perProducer; i++)
            {
                (int Producer, int Sequence) item = (producer, i);
                executor.Post(state =>
                {
                    using (ownership.Enter())
                        seen.Enqueue(((int, int))state!);
                }, item);
            }
        });

        Assert.True(SpinWait.SpinUntil(() => seen.Count == producers * perProducer, TimeSpan.FromSeconds(30)));
        Assert.Equal(0, ownership.Violations);

        foreach (var group in seen.GroupBy(x => x.Producer))
            Assert.Equal(Enumerable.Range(0, perProducer), group.Select(x => x.Sequence));
    }

    [Fact]
    public void ADrainLongerThanOneSlice_HandsTheRestToThePool_AndStillRunsEverything()
    {
        using var executor = new SessionExecutor("test");
        using var ownerInside = new ManualResetEventSlim();
        using var letOwnerFinish = new ManualResetEventSlim();
        int ran = 0;
        const int queued = SessionExecutor.DrainBatch * 3;

        // Park an owner so everything below is queued rather than run inline.
        var owner = Task.Run(() => executor.Post(_ =>
        {
            ownerInside.Set();
            letOwnerFinish.Wait();
        }));
        Assert.True(ownerInside.Wait(TimeSpan.FromSeconds(5)));

        for (int i = 0; i < queued; i++)
            executor.Post(_ => Interlocked.Increment(ref ran));

        letOwnerFinish.Set();
        Assert.True(owner.Wait(TimeSpan.FromSeconds(5)));

        Assert.True(SpinWait.SpinUntil(() => Volatile.Read(ref ran) == queued, TimeSpan.FromSeconds(30)));
        Assert.Equal(0, executor.QueueDepth);
    }

    [Fact]
    public void AnEventThatThrows_IsContainedAndTheNextOneStillRuns()
    {
        using var executor = new SessionExecutor("test");
        bool afterRan = false;

        executor.Post(_ => throw new InvalidOperationException("boom"));
        executor.Post(_ => afterRan = true);

        Assert.True(afterRan);
    }

    [Fact]
    public void PastTheHighWaterMark_ReadsArePaused_AndResumedWhenItDrains()
    {
        var host = new RecordingHost();
        using var executor = new SessionExecutor("test", host);
        using var ownerInside = new ManualResetEventSlim();
        using var letOwnerFinish = new ManualResetEventSlim();

        var owner = Task.Run(() => executor.Post(_ =>
        {
            ownerInside.Set();
            letOwnerFinish.Wait();
        }));
        Assert.True(ownerInside.Wait(TimeSpan.FromSeconds(5)));

        for (int i = 0; i < SessionExecutor.HighWaterMark + 1; i++)
            executor.Post(_ => { });

        Assert.Equal(1, host.Pauses);
        Assert.Equal(0, host.Resumes);

        letOwnerFinish.Set();
        Assert.True(owner.Wait(TimeSpan.FromSeconds(5)));
        Assert.True(SpinWait.SpinUntil(() => executor.QueueDepth == 0, TimeSpan.FromSeconds(30)));
        Assert.Equal(1, host.Resumes);
        Assert.Equal(0, host.Overflows);
    }

    [Fact]
    public void PastTheHardCap_TheSessionIsToldToGo()
    {
        var host = new RecordingHost();
        using var executor = new SessionExecutor("test", host);
        using var ownerInside = new ManualResetEventSlim();
        using var letOwnerFinish = new ManualResetEventSlim();

        var owner = Task.Run(() => executor.Post(_ =>
        {
            ownerInside.Set();
            letOwnerFinish.Wait();
        }));
        Assert.True(ownerInside.Wait(TimeSpan.FromSeconds(5)));

        for (int i = 0; i < SessionExecutor.HardCap + 1; i++)
            executor.Post(_ => { });

        Assert.True(host.Overflows >= 1);

        letOwnerFinish.Set();
        Assert.True(owner.Wait(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public void AfterDispose_PostsAreIgnored()
    {
        var executor = new SessionExecutor("test");
        executor.Dispose();

        bool ran = false;
        executor.Post(_ => ran = true);

        Assert.False(ran);
    }

    [Fact]
    public void QueuedEventsRecordHowLongTheyWaited()
    {
        using var executor = new SessionExecutor("test");
        using var ownerInside = new ManualResetEventSlim();
        using var letOwnerFinish = new ManualResetEventSlim();

        var owner = Task.Run(() => executor.Post(_ =>
        {
            ownerInside.Set();
            letOwnerFinish.Wait();
        }));
        Assert.True(ownerInside.Wait(TimeSpan.FromSeconds(5)));

        executor.Post(_ => { });
        Thread.Sleep(20);
        letOwnerFinish.Set();
        Assert.True(owner.Wait(TimeSpan.FromSeconds(5)));
        Assert.True(SpinWait.SpinUntil(() => executor.QueueDepth == 0, TimeSpan.FromSeconds(5)));

        var stats = executor.QueueWaitMs;
        Assert.Equal(1, stats.Count);
        Assert.True(stats.Max >= 10, $"expected a wait of at least 10 ms, got {stats.Max:F1} ms");
    }

    [Fact]
    public void InlineWork_DoesNotCountAsQueueWait()
    {
        using var executor = new SessionExecutor("test");

        executor.Post(_ => { });

        Assert.Equal(0, executor.QueueWaitMs.Count);
    }
}
