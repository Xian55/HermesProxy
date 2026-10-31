using System;
using System.Threading;
using System.Threading.Tasks;

namespace HermesProxy.World.Client;

/// <summary>
/// Where the legacy world server's authentication stands, set from the threads a connection runs
/// on and waited for by the one that asked for it.
/// </summary>
/// <remarks>
/// The answer is not final: a player waiting in the server's queue is accepted, and can still be
/// refused once the queue ends. What is final is the end of the wait.
/// </remarks>
internal sealed class LegacyHandshake
{
    private enum State { Pending, Accepted, Refused }

    private readonly TaskCompletionSource _settled = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private volatile State _state;

    public bool IsPending => _state == State.Pending;

    public bool IsAccepted => _state == State.Accepted;

    public bool IsRefused => _state == State.Refused;

    /// <summary>What the server said. Replaces whatever stood before.</summary>
    public void Accept() => Settle(State.Accepted);

    /// <inheritdoc cref="Accept"/>
    public void Refuse() => Settle(State.Refused);

    /// <summary>
    /// For a failure that only counts while nothing was decided: a connection lost after the
    /// server accepted does not undo that. True if this is what ended the wait.
    /// </summary>
    public bool TryRefuse()
    {
        if (Interlocked.CompareExchange(ref _state, State.Refused, State.Pending) != State.Pending)
            return false;

        _settled.TrySetResult();
        return true;
    }

    /// <summary>False if nothing settled it in time. Wakes the moment something does.</summary>
    public bool Wait(TimeSpan timeout) => _settled.Task.Wait(timeout);

    private void Settle(State state)
    {
        _state = state;
        _settled.TrySetResult();
    }
}
