using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Framework.Logging;
using HermesProxy.World.Logging;

namespace HermesProxy.World.Outbox;

/// <summary>
/// Sends packets now, or holds them until something happens: an event, a gate opening, a
/// deadline. The one place a handler says "send this later" or "send this after that".
/// </summary>
/// <remarks>
/// <para>
/// <b>Why it exists.</b> Before it, each reason to delay a packet grew its own queue on the
/// session, each with its own locking, ordering and cleanup, fed and drained from whichever thread
/// happened to be running. See <c>World/Outbox/CLAUDE.md</c> for the maps and the rules.
/// </para>
/// <para>
/// <b>Threading.</b> Safe to call from any thread. State changes happen under one lock, and nothing
/// that can call back into proxy code runs while it is held: wire writes, continuations and packet
/// disposal all happen after the lock is released. Whoever triggers a release writes the released
/// packets before its own call returns, so a thread's sends leave in the order it made them.
/// </para>
/// <para>
/// <b>Cost when idle.</b> With nothing held, <see cref="Notify"/> and <see cref="Tick"/> are one
/// volatile read each, and sending is a direct wire write.
/// </para>
/// </remarks>
public abstract class PacketOutbox<TPacket> : IDisposable where TPacket : class
{
    private static readonly Microsoft.Extensions.Logging.ILogger _log = Log.CreateMelLogger(Log.CategoryServer);

    // A release that keeps triggering further releases is a loop, not a burst. Stop it instead of
    // letting one packet pin a thread.
    private const int MaxReleasesPerRun = 10_000;
    private static readonly TimeSpan TimeoutWarningInterval = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan MinimumTimerDue = TimeSpan.FromMilliseconds(1);

    private sealed class Hold
    {
        public OutboxHoldKind Kind;
        public TPacket? Packet;
        public Action? Continuation;
        public HoldKey? Key;
        public bool Coalescing;
        public OutboxScope Scope;
        public OutboxTimeoutAction OnTimeout;
        public bool DefaultedTimeout;
        public bool RunsOnTimer;
        public long RegisteredAt;
        public long Deadline;
        public long Sequence;
        public int Remaining;
        public bool Done;
        public OutboxEvent FirstEvent;
        public OutboxEvent[]? Events;
    }

    // One per thread that is currently releasing for this outbox. A release that triggers more
    // releases on the same outbox appends to it instead of recursing, so a chain can't grow the
    // stack and holds freed by an earlier trigger go out before holds freed by a later one.
    private sealed class ReleaseRun(object owner)
    {
        public readonly object Owner = owner;
        public readonly List<Hold> Queue = [];
    }

    [ThreadStatic] private static ReleaseRun? t_run;

    private static readonly Comparison<Hold> BySequence = static (a, b) => a.Sequence.CompareTo(b.Sequence);

    private readonly string _direction;
    private readonly TimeProvider _time;
    private readonly OutboxOptions _options;
    private readonly Action? _onOverflow;

    private readonly Lock _lock = new();
    // Every waiting hold has a deadline (a timeout, or the time a delayed hold is due), so this
    // list is the complete set. Completed holds are purged from it lazily.
    private readonly List<Hold> _holds = [];
    private readonly Dictionary<OutboxEvent, List<Hold>> _byEvent = [];
    private readonly Dictionary<HoldKey, List<Hold>> _byKey = [];
    private readonly Dictionary<HoldKey, long> _pacedNext = [];
    private readonly bool[] _gates = new bool[Enum.GetValues<OutboxGate>().Length];

    private int _pending;
    private long _sequence;
    private long _nextTickDeadline = long.MaxValue;
    private long _timerDue = long.MaxValue;
    private long _lastTimeoutWarning = long.MinValue;
    private bool _overflowReported;
    private bool _disposed;
    private ITimer? _timer;

    protected PacketOutbox(string direction, TimeProvider? time, OutboxOptions? options, Action? onOverflow)
    {
        _direction = direction;
        _time = time ?? TimeProvider.System;
        _options = options ?? new OutboxOptions();
        _onOverflow = onOverflow;
    }

    /// <summary>Holds still waiting.</summary>
    public int PendingCount => Volatile.Read(ref _pending);

    /// <summary>True while anything is held. One volatile read.</summary>
    public bool HasPending => Volatile.Read(ref _pending) != 0;

    /// <summary>Writes the packet now.</summary>
    public abstract void Send(TPacket packet);

    /// <summary>Releases a packet that will never be written.</summary>
    protected abstract void Drop(TPacket packet);

    /// <summary>
    /// Whether a held packet of this direction may be written from the timer thread. False when
    /// writing it reads session state, which only the threads that own that state may do.
    /// </summary>
    protected abstract bool PacketsAreTimerSafe { get; }

    // ---- holds ------------------------------------------------------------------------------

    /// <summary>Sends the packet the next time <paramref name="evt"/> occurs.</summary>
    public void When(OutboxEvent evt, TPacket packet, in HoldOptions options = default)
        => RegisterEvents(new Hold { Packet = packet }, [evt], options);

    /// <summary>Runs <paramref name="release"/> the next time <paramref name="evt"/> occurs.</summary>
    public void When(OutboxEvent evt, Action release, in HoldOptions options = default)
        => RegisterEvents(new Hold { Continuation = release }, [evt], options);

    /// <summary>Runs <paramref name="release"/> once every event in <paramref name="events"/> has occurred.</summary>
    public void WhenAll(ReadOnlySpan<OutboxEvent> events, Action release, in HoldOptions options = default)
        => RegisterEvents(new Hold { Continuation = release }, events, options);

    /// <summary>Sends the packet once <paramref name="gate"/> is open; immediately if it already is.</summary>
    public void When(OutboxGate gate, TPacket packet, in HoldOptions options = default)
        => RegisterGate(new Hold { Packet = packet }, gate, options);

    /// <summary>Runs <paramref name="release"/> once <paramref name="gate"/> is open; immediately if it already is.</summary>
    public void When(OutboxGate gate, Action release, in HoldOptions options = default)
        => RegisterGate(new Hold { Continuation = release }, gate, options);

    /// <summary>Sends the packet after <paramref name="delay"/>.</summary>
    public void Delay(TimeSpan delay, TPacket packet, in HoldOptions options = default)
        => RegisterTimer(new Hold { Packet = packet }, delay, options);

    /// <summary>Runs <paramref name="release"/> after <paramref name="delay"/>.</summary>
    public void Delay(TimeSpan delay, Action release, in HoldOptions options = default)
        => RegisterTimer(new Hold { Continuation = release }, delay, options);

    /// <summary>
    /// Sends packets that share <paramref name="key"/> at least <paramref name="interval"/> apart,
    /// in the order given. The first goes out immediately when the key is idle.
    /// </summary>
    public void Paced(HoldKey key, TimeSpan interval, TPacket packet, in HoldOptions options = default)
    {
        bool sendNow = false;
        bool refused = false;
        lock (_lock)
        {
            if (_disposed)
            {
                refused = true;
            }
            else
            {
                long now = _time.GetTimestamp();
                long intervalTicks = ToTimestampTicks(interval);
                long next = _pacedNext.TryGetValue(key, out long queuedNext) && queuedNext > now ? queuedNext : now;
                // A packet still waiting under this key keeps its place even if its slot has
                // passed (the timer is late, or it waits for Tick); sending past it would reorder.
                bool behindWaiting = _byKey.ContainsKey(key);
                if (behindWaiting && next == now)
                    next = now + 1;
                if (next == now)
                {
                    sendNow = true;
                    _pacedNext[key] = now + intervalTicks;
                }
                else if (!TryReserveSlot())
                {
                    refused = true;
                }
                else
                {
                    var hold = new Hold { Packet = packet, Kind = OutboxHoldKind.Timer, Key = key };
                    ApplyTimerOptions(hold, now, next, options);
                    Link(hold);
                    _pacedNext[key] = next + intervalTicks;
                    ArmTimer();
                    LogHeld(hold);
                }
            }
        }

        if (refused)
            Refuse(packet);
        else if (sendNow)
            SafeSend(packet);
    }

    /// <summary>
    /// Runs the newest <paramref name="release"/> offered under <paramref name="key"/>,
    /// <paramref name="window"/> after the first offer. Later offers inside the window replace the
    /// earlier one without moving the deadline, so a burst collapses to one release.
    /// </summary>
    public void Coalesce(HoldKey key, TimeSpan window, Action release, in HoldOptions options = default)
    {
        bool refused = false;
        lock (_lock)
        {
            if (_disposed)
            {
                refused = true;
            }
            else
            {
                if (_byKey.TryGetValue(key, out var keyed))
                {
                    foreach (var existing in keyed)
                    {
                        if (existing.Coalescing && !existing.Done)
                        {
                            existing.Continuation = release;
                            return;
                        }
                    }
                }

                if (!TryReserveSlot())
                {
                    refused = true;
                }
                else
                {
                    long now = _time.GetTimestamp();
                    var hold = new Hold { Continuation = release, Kind = OutboxHoldKind.Timer, Key = key, Coalescing = true };
                    ApplyTimerOptions(hold, now, now + ToTimestampTicks(window), options);
                    Link(hold);
                    ArmTimer();
                    LogHeld(hold);
                }
            }
        }

        if (refused)
            Refuse(null);
    }

    // ---- triggers ---------------------------------------------------------------------------

    /// <summary>
    /// Records that <paramref name="evt"/> happened, releasing every hold waiting for it. Holds
    /// registered after this call wait for the next occurrence.
    /// </summary>
    public void Notify(OutboxEvent evt)
    {
        if (Volatile.Read(ref _pending) == 0)
            return;

        List<Hold>? released = null;
        lock (_lock)
        {
            CollectEvent(evt, ref released);
        }

        if (released != null)
            RunReleases(released);
    }

    /// <summary>Opens or closes a gate. Opening releases every hold waiting on it.</summary>
    public void SetGate(OutboxGate gate, bool open)
    {
        List<Hold>? released = null;
        lock (_lock)
        {
            bool wasOpen = _gates[(int)gate];
            _gates[(int)gate] = open;
            if (open && !wasOpen)
                CollectEvent(OutboxEvent.GateOpened(gate), ref released);
        }

        if (released != null)
            RunReleases(released);
    }

    public bool IsGateOpen(OutboxGate gate)
    {
        lock (_lock)
        {
            return _gates[(int)gate];
        }
    }

    /// <summary>Drops every waiting hold named <paramref name="key"/>. True if any was dropped.</summary>
    public bool Cancel(HoldKey key)
    {
        List<Hold>? cancelled = null;
        lock (_lock)
        {
            CollectKey(key, ref cancelled);
        }

        if (cancelled == null)
            return false;

        OutboxLogMessages.Cancelled(_log, _direction, cancelled.Count, key.Kind, key.A);
        foreach (var hold in cancelled)
            DropHold(hold);
        return true;
    }

    /// <summary>Releases every waiting hold named <paramref name="key"/> now. True if any was released.</summary>
    public bool Release(HoldKey key)
    {
        List<Hold>? released = null;
        lock (_lock)
        {
            CollectKey(key, ref released);
        }

        if (released == null)
            return false;

        RunReleases(released);
        return true;
    }

    /// <summary>
    /// Acts on holds whose deadline has passed but which must not be acted on from the timer
    /// thread. Call it from a thread that owns session state; one volatile read when nothing is due.
    /// </summary>
    public void Tick()
    {
        long now = _time.GetTimestamp();
        if (now < Volatile.Read(ref _nextTickDeadline))
            return;

        List<Hold>? released = null;
        List<Hold>? dropped = null;
        lock (_lock)
        {
            if (_disposed)
                return;
            CollectDue(now, onTimerThread: false, ref released, ref dropped);
            ArmTimer();
        }

        ActOnDue(released, dropped);
    }

    /// <summary>
    /// Drops the holds tied to <paramref name="scope"/>. <see cref="OutboxScope.Session"/> drops
    /// everything and closes every gate.
    /// </summary>
    public void Discard(OutboxScope scope)
    {
        List<Hold>? discarded = null;
        lock (_lock)
        {
            foreach (var hold in _holds)
            {
                if (hold.Done || (scope != OutboxScope.Session && hold.Scope != scope))
                    continue;
                Complete(hold, ref discarded);
            }
            PurgeCompleted();

            if (scope == OutboxScope.Session)
            {
                Array.Clear(_gates);
                _pacedNext.Clear();
            }
            ArmTimer();
        }

        if (discarded == null)
            return;

        OutboxLogMessages.Discarded(_log, _direction, discarded.Count, scope);
        foreach (var hold in discarded)
            DropHold(hold);
    }

    public void Dispose()
    {
        List<Hold>? discarded = null;
        ITimer? timer;
        lock (_lock)
        {
            if (_disposed)
                return;
            _disposed = true;
            foreach (var hold in _holds)
            {
                if (!hold.Done)
                    Complete(hold, ref discarded);
            }
            _holds.Clear();
            timer = _timer;
            _timer = null;
        }

        timer?.Dispose();
        if (discarded == null)
            return;

        OutboxLogMessages.Discarded(_log, _direction, discarded.Count, OutboxScope.Session);
        foreach (var hold in discarded)
            DropHold(hold);
    }

    // ---- registration -----------------------------------------------------------------------

    private void RegisterEvents(Hold hold, ReadOnlySpan<OutboxEvent> events, in HoldOptions options)
    {
        hold.Kind = OutboxHoldKind.Event;
        if (events.IsEmpty)
        {
            RunReleases([hold]);
            return;
        }

        bool refused = false;
        lock (_lock)
        {
            if (_disposed || !TryReserveSlot())
            {
                refused = true;
            }
            else
            {
                int distinct = 0;
                OutboxEvent[]? all = events.Length > 1 ? new OutboxEvent[events.Length] : null;
                foreach (var evt in events)
                {
                    if (all != null && Array.IndexOf(all, evt, 0, distinct) >= 0)
                        continue;
                    if (all != null)
                        all[distinct] = evt;
                    distinct++;
                    AddToEvent(evt, hold);
                }

                hold.FirstEvent = events[0];
                hold.Events = all != null ? all[..distinct] : null;
                hold.Remaining = distinct;
                ApplyWaitOptions(hold, options);
                Link(hold);
                ArmTimer();
                LogHeld(hold);
            }
        }

        if (refused)
            Refuse(hold.Packet);
    }

    private void RegisterGate(Hold hold, OutboxGate gate, in HoldOptions options)
    {
        hold.Kind = OutboxHoldKind.Gate;
        bool refused = false;
        bool openNow = false;
        lock (_lock)
        {
            if (_disposed)
            {
                refused = true;
            }
            else if (_gates[(int)gate])
            {
                openNow = true;
            }
            else if (!TryReserveSlot())
            {
                refused = true;
            }
            else
            {
                var evt = OutboxEvent.GateOpened(gate);
                AddToEvent(evt, hold);
                hold.FirstEvent = evt;
                hold.Remaining = 1;
                ApplyWaitOptions(hold, options);
                Link(hold);
                ArmTimer();
                LogHeld(hold);
            }
        }

        if (refused)
            Refuse(hold.Packet);
        else if (openNow)
            RunReleases([hold]);
    }

    private void RegisterTimer(Hold hold, TimeSpan delay, in HoldOptions options)
    {
        hold.Kind = OutboxHoldKind.Timer;
        bool refused = false;
        lock (_lock)
        {
            if (_disposed || !TryReserveSlot())
            {
                refused = true;
            }
            else
            {
                long now = _time.GetTimestamp();
                hold.Key = options.Key;
                ApplyTimerOptions(hold, now, now + ToTimestampTicks(delay), options);
                Link(hold);
                ArmTimer();
                LogHeld(hold);
            }
        }

        if (refused)
            Refuse(hold.Packet);
    }

    private void ApplyWaitOptions(Hold hold, in HoldOptions options)
    {
        long now = _time.GetTimestamp();
        hold.RegisteredAt = now;
        hold.Key = options.Key;
        hold.Scope = options.Scope;
        hold.DefaultedTimeout = options.Timeout == null;
        // A timeout nobody chose means nobody decided the packet is still worth sending late.
        hold.OnTimeout = hold.DefaultedTimeout ? OutboxTimeoutAction.Discard : options.OnTimeout;
        hold.Deadline = now + ToTimestampTicks(options.Timeout ?? _options.DefaultTimeout);
        hold.RunsOnTimer = options.RunOnTimer
            || hold.OnTimeout == OutboxTimeoutAction.Discard
            || (hold.Packet != null && PacketsAreTimerSafe);
    }

    private void ApplyTimerOptions(Hold hold, long now, long due, in HoldOptions options)
    {
        hold.RegisteredAt = now;
        hold.Scope = options.Scope;
        hold.OnTimeout = OutboxTimeoutAction.Release;
        hold.Deadline = due;
        hold.RunsOnTimer = options.RunOnTimer || (hold.Packet != null && PacketsAreTimerSafe);
    }

    private bool TryReserveSlot()
    {
        if (_pending < _options.MaxPendingHolds)
        {
            if (_pending < _options.MaxPendingHolds / 2)
                _overflowReported = false;
            return true;
        }
        return false;
    }

    private void Refuse(TPacket? packet)
    {
        bool firstInEpisode;
        bool disposed;
        lock (_lock)
        {
            disposed = _disposed;
            firstInEpisode = !disposed && !_overflowReported;
            if (firstInEpisode)
                _overflowReported = true;
        }

        if (disposed)
            OutboxLogMessages.RefusedAfterDispose(_log, _direction);
        else
            OutboxLogMessages.Overflow(_log, _direction, _options.MaxPendingHolds);

        if (packet != null)
            SafeDrop(packet);

        if (firstInEpisode)
        {
            try
            {
                _onOverflow?.Invoke();
            }
            catch (Exception ex)
            {
                OutboxLogMessages.CalloutFailed(_log, ex, _direction, "overflow");
            }
        }
    }

    // ---- bookkeeping (all under _lock) ------------------------------------------------------

    private void Link(Hold hold)
    {
        hold.Sequence = ++_sequence;
        _holds.Add(hold);
        _pending++;
        if (hold.Key is { } key)
        {
            if (!_byKey.TryGetValue(key, out var keyed))
                _byKey[key] = keyed = [];
            keyed.Add(hold);
        }
    }

    private void AddToEvent(OutboxEvent evt, Hold hold)
    {
        if (!_byEvent.TryGetValue(evt, out var waiting))
            _byEvent[evt] = waiting = [];
        waiting.Add(hold);
    }

    private void Complete(Hold hold, ref List<Hold>? into)
    {
        hold.Done = true;
        _pending--;
        (into ??= []).Add(hold);

        if (hold.Kind != OutboxHoldKind.Timer)
        {
            if (hold.Events != null)
            {
                foreach (var evt in hold.Events)
                    RemoveFromEvent(evt, hold);
            }
            else
            {
                RemoveFromEvent(hold.FirstEvent, hold);
            }
        }

        if (hold.Key is { } key && _byKey.TryGetValue(key, out var keyed))
        {
            keyed.Remove(hold);
            if (keyed.Count == 0)
                _byKey.Remove(key);
        }
    }

    private void RemoveFromEvent(OutboxEvent evt, Hold hold)
    {
        if (_byEvent.TryGetValue(evt, out var waiting))
        {
            waiting.Remove(hold);
            if (waiting.Count == 0)
                _byEvent.Remove(evt);
        }
    }

    private void CollectEvent(OutboxEvent evt, ref List<Hold>? released)
    {
        if (!_byEvent.Remove(evt, out var waiting))
            return;

        foreach (var hold in waiting)
        {
            if (!hold.Done && --hold.Remaining == 0)
                Complete(hold, ref released);
        }

        if (released != null)
            PurgeCompleted();
        ArmTimer();
    }

    private void CollectKey(HoldKey key, ref List<Hold>? collected)
    {
        if (!_byKey.TryGetValue(key, out var keyed))
            return;

        foreach (var hold in keyed.ToArray())
            Complete(hold, ref collected);
        PurgeCompleted();
        ArmTimer();
    }

    private void CollectDue(long now, bool onTimerThread, ref List<Hold>? released, ref List<Hold>? dropped)
    {
        foreach (var hold in _holds)
        {
            if (hold.Done || hold.Deadline > now || (onTimerThread && !hold.RunsOnTimer))
                continue;

            if (hold.OnTimeout == OutboxTimeoutAction.Release)
                Complete(hold, ref released);
            else
                Complete(hold, ref dropped);
        }
        PurgeCompleted();
    }

    private void PurgeCompleted() => _holds.RemoveAll(static h => h.Done);

    private void ArmTimer()
    {
        long timerDue = long.MaxValue;
        long tickDue = long.MaxValue;
        foreach (var hold in _holds)
        {
            if (hold.Done)
                continue;
            if (hold.RunsOnTimer)
                timerDue = Math.Min(timerDue, hold.Deadline);
            else
                tickDue = Math.Min(tickDue, hold.Deadline);
        }

        Volatile.Write(ref _nextTickDeadline, tickDue);

        if (_disposed || timerDue == _timerDue)
            return;

        _timerDue = timerDue;
        if (timerDue == long.MaxValue)
        {
            _timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            return;
        }

        // Never zero: a provider that fires due timers synchronously inside Change would run the
        // callback while this thread still holds the lock.
        var due = TimeSpan.FromTicks((long)((timerDue - _time.GetTimestamp()) * ((double)TimeSpan.TicksPerSecond / _time.TimestampFrequency)));
        if (due < MinimumTimerDue)
            due = MinimumTimerDue;

        _timer ??= _time.CreateTimer(static state => ((PacketOutbox<TPacket>)state!).OnTimer(), this, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        _timer.Change(due, Timeout.InfiniteTimeSpan);
    }

    private long ToTimestampTicks(TimeSpan span)
        => (long)(span.Ticks * ((double)_time.TimestampFrequency / TimeSpan.TicksPerSecond));

    // ---- acting on holds (never under _lock) ------------------------------------------------

    private void OnTimer()
    {
        try
        {
            List<Hold>? released = null;
            List<Hold>? dropped = null;
            lock (_lock)
            {
                if (_disposed)
                    return;
                _timerDue = long.MaxValue;
                CollectDue(_time.GetTimestamp(), onTimerThread: true, ref released, ref dropped);
                ArmTimer();
            }

            ActOnDue(released, dropped);
        }
        catch (Exception ex)
        {
            // Timer callbacks must never throw: an unhandled exception here ends the process.
            OutboxLogMessages.CalloutFailed(_log, ex, _direction, "timer");
        }
    }

    private void ActOnDue(List<Hold>? released, List<Hold>? dropped)
    {
        if (dropped != null)
        {
            foreach (var hold in dropped)
            {
                LogTimedOut(hold);
                DropHold(hold);
            }
        }

        if (released != null)
        {
            foreach (var hold in released)
            {
                if (hold.Kind != OutboxHoldKind.Timer)
                    LogTimedOut(hold);
            }
            RunReleases(released);
        }
    }

    private void RunReleases(List<Hold> released)
    {
        Debug.Assert(!_lock.IsHeldByCurrentThread, "Outbox releases must run outside the outbox lock.");
        released.Sort(BySequence);

        var current = t_run;
        if (current != null && ReferenceEquals(current.Owner, this))
        {
            current.Queue.AddRange(released);
            return;
        }

        var run = new ReleaseRun(this);
        run.Queue.AddRange(released);
        t_run = run;
        try
        {
            for (int i = 0; i < run.Queue.Count; i++)
            {
                if (i == MaxReleasesPerRun)
                {
                    OutboxLogMessages.ReleaseRunLimit(_log, _direction, MaxReleasesPerRun, run.Queue.Count - i);
                    for (int j = i; j < run.Queue.Count; j++)
                        DropHold(run.Queue[j]);
                    break;
                }

                ReleaseOne(run.Queue[i]);
            }
        }
        finally
        {
            t_run = current;
        }
    }

    private void ReleaseOne(Hold hold)
    {
        if (hold.RegisteredAt != 0 && _log.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
            OutboxLogMessages.Released(_log, _direction, hold.Kind, _time.GetElapsedTime(hold.RegisteredAt).TotalMilliseconds);

        if (hold.Packet is { } packet)
        {
            hold.Packet = null;
            SafeSend(packet);
        }
        else if (hold.Continuation is { } continuation)
        {
            hold.Continuation = null;
            try
            {
                continuation();
            }
            catch (Exception ex)
            {
                OutboxLogMessages.CalloutFailed(_log, ex, _direction, "continuation");
            }
        }
    }

    private void SafeSend(TPacket packet)
    {
        try
        {
            Send(packet);
        }
        catch (Exception ex)
        {
            OutboxLogMessages.CalloutFailed(_log, ex, _direction, "send");
        }
    }

    private void DropHold(Hold hold)
    {
        hold.Continuation = null;
        if (hold.Packet is { } packet)
        {
            hold.Packet = null;
            SafeDrop(packet);
        }
    }

    private void SafeDrop(TPacket packet)
    {
        try
        {
            Drop(packet);
        }
        catch (Exception ex)
        {
            OutboxLogMessages.CalloutFailed(_log, ex, _direction, "drop");
        }
    }

    private void LogHeld(Hold hold)
        => OutboxLogMessages.Held(_log, _direction, hold.Kind, hold.FirstEvent.Kind, hold.FirstEvent.A, _pending);

    private void LogTimedOut(Hold hold)
    {
        double heldMs = _time.GetElapsedTime(hold.RegisteredAt).TotalMilliseconds;
        long now = _time.GetTimestamp();
        // One warning per interval at most: a hung server times out many holds at once, and a
        // warning per hold would cost more than the holds themselves.
        long last = Interlocked.Read(ref _lastTimeoutWarning);
        bool warn = last == long.MinValue || _time.GetElapsedTime(last, now) >= TimeoutWarningInterval;
        if (warn && Interlocked.CompareExchange(ref _lastTimeoutWarning, now, last) == last)
            OutboxLogMessages.TimedOutWarning(_log, _direction, hold.Kind, hold.FirstEvent.Kind, hold.FirstEvent.A, heldMs, hold.OnTimeout);
        else
            OutboxLogMessages.TimedOut(_log, _direction, hold.Kind, hold.FirstEvent.Kind, hold.FirstEvent.A, heldMs, hold.OnTimeout);
    }
}
