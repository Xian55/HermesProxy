using Framework.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Networking;

/// <summary>
/// The accepted connections of one listener: forgets the ones that died, and closes the rest on
/// the way out.
/// </summary>
/// <remarks>
/// A connection's reads and writes never pass through here, so nothing waits on the interval. A
/// thread per listener polling at 100 Hz used to do this and was most of an idle proxy's CPU.
/// </remarks>
public sealed class ConnectionSweeper<TSocketType> : IAsyncDisposable where TSocketType : ISocket
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melNet = Log.CreateMelLogger(Log.CategoryNetwork);
    private const string _sourceFile = "ConnectionSweeper";
    private const string _netDirNone = "";

    public static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(5);

    readonly CancellationTokenSource _stopping = new();
    readonly System.Threading.PeriodicTimer _timer;
    readonly ConcurrentQueue<TSocketType> _newSockets = new();

    // Only the sweep touches this, and DisposeAsync once the sweep has ended.
    readonly List<TSocketType> _sockets = [];

    Task _sweepLoop = Task.CompletedTask;
    int _connections;

    public ConnectionSweeper(TimeSpan interval)
    {
        _timer = new System.Threading.PeriodicTimer(interval);
    }

    public int ConnectionCount => Volatile.Read(ref _connections);

    internal Task SweepLoop => _sweepLoop;

    public void Start() => _sweepLoop = SweepLoopAsync(_stopping.Token);

    public void Add(TSocketType socket)
    {
        Interlocked.Increment(ref _connections);
        _newSockets.Enqueue(socket);
    }

    private async Task SweepLoopAsync(CancellationToken stopping)
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(stopping))
                Sweep();
        }
        catch (OperationCanceledException) when (stopping.IsCancellationRequested)
        {
            // DisposeAsync asked for this.
        }
        catch (Exception ex)
        {
            ConnectionSweeperLogMessages.SweepEnded(_melNet, ex, _sourceFile, _netDirNone, ex.Message);
        }
    }

    internal void Sweep()
    {
        while (_newSockets.TryDequeue(out var socket))
        {
            if (socket.IsOpen())
                _sockets.Add(socket);
            else
                Interlocked.Decrement(ref _connections);
        }

        // Backwards, so swapping the last element into a freed slot skips nothing.
        for (int i = _sockets.Count - 1; i >= 0; i--)
        {
            TSocketType socket = _sockets[i];
            if (IsAlive(socket))
                continue;

            Close(socket);
            Interlocked.Decrement(ref _connections);

            int lastIndex = _sockets.Count - 1;
            _sockets[i] = _sockets[lastIndex];
            _sockets.RemoveAt(lastIndex);
        }
    }

    // One connection's failure stays that connection's: the sweep has every other one still to do.
    private static bool IsAlive(TSocketType socket)
    {
        try
        {
            return socket.Update();
        }
        catch (Exception ex)
        {
            ConnectionSweeperLogMessages.UpdateFailed(_melNet, ex, _sourceFile, _netDirNone, ex.Message);
            return false;
        }
    }

    private static void Close(TSocketType socket)
    {
        try
        {
            if (socket.IsOpen())
                socket.CloseSocket();
        }
        catch (Exception ex)
        {
            ConnectionSweeperLogMessages.CloseFailed(_melNet, ex, _sourceFile, _netDirNone, ex.Message);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_stopping.IsCancellationRequested)
            return;

        await _stopping.CancelAsync();
        await _sweepLoop;
        _timer.Dispose();
        _stopping.Dispose();

        // Close every accepted socket on shutdown. Without this the peer (e.g. the WoW client)
        // sees no FIN and stays ESTABLISHED, which causes the OS to hold the listening port -
        // the next proxy run then fails to bind with WSAEACCES until the peer is restarted.
        while (_newSockets.TryDequeue(out var pending))
            Close(pending);

        foreach (var socket in _sockets)
            Close(socket);

        _sockets.Clear();
    }
}
