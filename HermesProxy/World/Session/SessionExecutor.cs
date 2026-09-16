using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Framework.Logging;
using Framework.Metrics;
using HermesProxy.World.Logging;
using Microsoft.Extensions.Logging;

namespace HermesProxy.World.Session;

/// <summary>
/// What the executor tells its session when the queue runs away from it. Both calls come from a
/// producer thread, never from inside an event.
/// </summary>
public interface ISessionExecutorHost
{
    /// <summary>Stop reading from the sockets: the owner is behind and the queue is growing.</summary>
    void PauseReads();

    /// <summary>The queue drained; start reading again.</summary>
    void ResumeReads();

    /// <summary>The queue passed its hard cap. The session cannot catch up and has to go.</summary>
    void Overflowed();
}

/// <summary>
/// One session's work, run by one thread at a time.
///
/// A packet handler, a timer callback or a teardown is posted here instead of running wherever it
/// arrived. When nothing else is running the poster runs it itself, on its own thread, before
/// <see cref="Post"/> returns — the common case pays a lock, not a hop. When the session is busy the
/// event is queued and the thread that already owns the session drains it in order.
///
/// The point is not parallelism, it is that session state has a single owner: with every entry
/// funnelled through here, the locks around <c>GameState</c> and the outbox exist only to protect
/// against threads that no longer touch it.
/// </summary>
public sealed class SessionExecutor : IThreadPoolWorkItem, IDisposable
{
    /// <summary>Events one owner runs before handing the rest to the thread pool.</summary>
    public const int DrainBatch = 64;

    /// <summary>How long one owner keeps draining before handing over, however few events that was.</summary>
    public static readonly TimeSpan DrainSlice = TimeSpan.FromMilliseconds(2);

    /// <summary>Queue depth that stops the sockets being read, so the backlog pushes back onto TCP.</summary>
    public const int HighWaterMark = 512;

    /// <summary>Queue depth that lets reading start again, low enough not to flap.</summary>
    public const int LowWaterMark = 128;

    /// <summary>Queue depth no session can be behind by and still recover.</summary>
    public const int HardCap = 8192;

    /// <summary>
    /// Every live executor, so <c>--metrics</c> can report how long work waited for its owner. That
    /// number is what says whether giving the session one owner cost the client anything.
    /// </summary>
    /// <remarks>
    /// Weak keys. A session that ends — including a login that failed — is not disposed by anyone,
    /// so a strong registry would keep one executor per login attempt for the life of the process.
    /// </remarks>
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SessionExecutor, object> Live = new();

    private readonly record struct Entry(Action<object?> Run, object? State, long EnqueuedTicks);

    private readonly Lock _lock = new();
    private readonly Queue<Entry> _queue = new();
    private readonly SampleWindow _queueWaitMs = new(1000);
    private readonly ILogger _log;
    private readonly string _name;
    private readonly ISessionExecutorHost? _host;

    private bool _running;
    private bool _paused;
    private bool _disposed;
    private int _ownerThreadId;
    private long _sliceDeadline;

    // Since the last metrics line. The lifetime window keeps 1000 samples, so on a quiet session
    // login's waits never age out of it and hide what combat is doing.
    private long _intervalCount;
    private double _intervalMax;

    public SessionExecutor(string name, ISessionExecutorHost? host = null, ILogger? log = null)
    {
        _name = name;
        _host = host;
        _log = log ?? Log.CreateMelLogger(Log.CategoryServer);
        Live.Add(this, this);
    }

    /// <summary>
    /// One line per live session for the metrics summary, or null when nothing has been queued at
    /// all — which is itself the answer: every event ran inline on the thread that posted it.
    /// </summary>
    public static string? SummaryLine()
    {
        int sessions = 0;
        long queued = 0;
        double max = 0;
        double p99 = 0;
        int depth = 0;
        long intervalQueued = 0;
        double intervalMax = 0;

        foreach (var (executor, _) in Live)
        {
            if (executor._disposed)
                continue;

            sessions++;
            depth += executor.QueueDepth;

            var (intCount, intMax) = executor.TakeInterval();
            intervalQueued += intCount;
            if (intMax > intervalMax) intervalMax = intMax;

            var stats = executor.QueueWaitMs;
            queued += stats.TotalCount;
            if (stats.Count == 0)
                continue;
            if (stats.Max > max) max = stats.Max;
            if (stats.P99 > p99) p99 = stats.P99;
        }

        if (sessions == 0)
            return null;

        return $"Executor: {sessions} session(s), {depth} waiting now; this interval {intervalQueued} queued, "
             + $"max {intervalMax:F2} ms; lifetime {queued} queued, p99 {p99:F2} ms, max {max:F2} ms";
    }

    /// <summary>Events waiting for the owner. Zero while the owner is running the last one.</summary>
    public int QueueDepth
    {
        get { lock (_lock) return _queue.Count; }
    }

    /// <summary>How long posted events waited before they ran. The number slice D is judged on.</summary>
    public SampleStats QueueWaitMs
    {
        get { lock (_lock) return _queueWaitMs.GetStats(); }
    }

    /// <summary>True while the calling thread is the one running this session's events.</summary>
    public bool IsOwner => Volatile.Read(ref _ownerThreadId) == Environment.CurrentManagedThreadId;

    /// <summary>
    /// Debug-only check that nobody touches session state behind the owner's back. Costs nothing in
    /// Release. It fires only while an owner is actually running: a test that drives a handler
    /// directly, with nothing posted, is not what this is looking for — a second thread reaching
    /// <c>GameState</c> while the owner is mid-packet is.
    /// </summary>
    [Conditional("DEBUG")]
    public void AssertOwner(string what)
    {
        if (Volatile.Read(ref _ownerThreadId) is int owner && owner != 0 && owner != Environment.CurrentManagedThreadId)
            throw new InvalidOperationException($"{what} ran on another thread while the session executor ({_name}) was running");
    }

    /// <summary>
    /// Runs <paramref name="run"/> against the session: inline on this thread when the session is
    /// free, queued in arrival order when it isn't. <paramref name="state"/> is handed back to the
    /// delegate, so a caller can post without allocating a closure.
    /// </summary>
    public void Post(Action<object?> run, object? state = null)
    {
        ArgumentNullException.ThrowIfNull(run);

        bool overflowed = false;
        bool pause = false;
        bool own = false;
        lock (_lock)
        {
            if (_disposed)
                return;

            if (_running || _queue.Count > 0)
            {
                _queue.Enqueue(new Entry(run, state, Stopwatch.GetTimestamp()));

                // On the crossing only: past the cap every further post would say the same thing.
                if (_queue.Count == HardCap)
                    overflowed = true;
                else if (!_paused && _queue.Count >= HighWaterMark)
                    pause = _paused = true;
            }
            else
            {
                // Free and nothing waiting: this thread becomes the owner and runs it now.
                _running = true;
                _ownerThreadId = Environment.CurrentManagedThreadId;
                _sliceDeadline = Stopwatch.GetTimestamp() + (long)(DrainSlice.TotalSeconds * Stopwatch.Frequency);
                own = true;
            }
        }

        // Outside the lock: an event can block for as long as it likes without stopping other
        // threads posting behind it. Running it under the lock deadlocks the moment one does.
        if (own)
        {
            RunAndDrain(new Entry(run, state, 0));
            return;
        }

        if (overflowed)
        {
            SessionExecutorLogMessages.Overflow(_log, _name, HardCap, HardCap);
            _host?.Overflowed();
        }
        else if (pause)
        {
            SessionExecutorLogMessages.BackpressureOn(_log, _name, HighWaterMark);
            _host?.PauseReads();
        }
    }

    /// <summary>Continues a drain that ran out of its slice, on a pool thread.</summary>
    void IThreadPoolWorkItem.Execute()
    {
        Entry entry;
        lock (_lock)
        {
            if (_disposed || _queue.Count == 0)
            {
                _running = false;
                _ownerThreadId = 0;
                return;
            }

            _ownerThreadId = Environment.CurrentManagedThreadId;
            _sliceDeadline = Stopwatch.GetTimestamp() + (long)(DrainSlice.TotalSeconds * Stopwatch.Frequency);
            entry = Dequeue();
        }

        RunAndDrain(entry);
    }

    /// <summary>
    /// Runs one event, then keeps taking events until the queue is empty or this owner's slice is
    /// used up. Called with the lock held for the first entry only; the lock is released around
    /// every event, because an event sends packets and may post more work.
    /// </summary>
    private void RunAndDrain(Entry first)
    {
        Entry entry = first;
        int ran = 0;

        while (true)
        {
            Execute(entry);
            ran++;

            bool resume = false;
            lock (_lock)
            {
                if (_paused && _queue.Count <= LowWaterMark)
                    resume = !(_paused = false);

                if (_disposed || _queue.Count == 0)
                {
                    _running = false;
                    _ownerThreadId = 0;
                    if (resume) NotifyResume();
                    return;
                }

                if (ran >= DrainBatch || Stopwatch.GetTimestamp() >= _sliceDeadline)
                {
                    // Still the owner, so producers keep queueing; a pool thread picks the rest up.
                    // Without this a busy session would keep whichever socket thread got here first.
                    _ownerThreadId = 0;
                    ThreadPool.UnsafeQueueUserWorkItem(this, preferLocal: false);
                    if (resume) NotifyResume();
                    return;
                }

                entry = Dequeue();
            }

            if (resume) NotifyResume();
        }
    }

    private Entry Dequeue()
    {
        Entry entry = _queue.Dequeue();
        if (entry.EnqueuedTicks != 0)
        {
            double waited = (Stopwatch.GetTimestamp() - entry.EnqueuedTicks) * 1000.0 / Stopwatch.Frequency;
            _queueWaitMs.Add(waited);
            _intervalCount++;
            if (waited > _intervalMax)
                _intervalMax = waited;
        }

        return entry;
    }

    /// <summary>Queue waits since the last call, then starts a fresh interval.</summary>
    private (long Count, double Max) TakeInterval()
    {
        lock (_lock)
        {
            var interval = (_intervalCount, _intervalMax);
            _intervalCount = 0;
            _intervalMax = 0;
            return interval;
        }
    }

    private void NotifyResume()
    {
        SessionExecutorLogMessages.BackpressureOff(_log, _name, LowWaterMark);
        _host?.ResumeReads();
    }

    private void Execute(in Entry entry)
    {
        try
        {
            entry.Run(entry.State);
        }
        catch (Exception ex)
        {
            // One bad packet must not take the session with it: the old per-frame catch in
            // ReadHandler had the same job.
            SessionExecutorLogMessages.EventFailed(_log, ex, _name);
        }
    }

    /// <summary>
    /// Stops accepting work. Events already queued are dropped: whatever they were going to do, the
    /// session they would have done it to is gone.
    /// </summary>
    public void Dispose()
    {
        Live.Remove(this);
        lock (_lock)
        {
            _disposed = true;
            _queue.Clear();
        }
    }
}
