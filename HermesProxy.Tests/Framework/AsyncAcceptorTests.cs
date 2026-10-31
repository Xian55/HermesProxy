using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Framework.Networking;
using Xunit;

namespace HermesProxy.Tests.Framework;

public sealed class AsyncAcceptorTests
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static AsyncAcceptor StartedAcceptor()
    {
        var acceptor = new AsyncAcceptor();
        Assert.True(acceptor.Start("127.0.0.1", 0));
        return acceptor;
    }

    private static Socket Connect(AsyncAcceptor acceptor)
    {
        var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client.Connect(acceptor.LocalEndPoint);
        return client;
    }

    // An accept that answers from a script, then waits to be cancelled like a real pending one.
    private static Func<CancellationToken, ValueTask<Socket>> ScriptedAccept(TaskCompletionSource exhausted, params Exception[] failures)
    {
        int call = 0;
        return async stopping =>
        {
            stopping.ThrowIfCancellationRequested();

            int index = Interlocked.Increment(ref call) - 1;
            if (index < failures.Length)
                throw failures[index];

            exhausted.TrySetResult();
            await Task.Delay(Timeout.Infinite, stopping);
            throw new InvalidOperationException("unreachable");
        };
    }

    private static async Task<AggregateException?> CaptureUnobservedDuring(Func<Task> action)
    {
        AggregateException? unobserved = null;
        EventHandler<UnobservedTaskExceptionEventArgs> handler = (_, e) =>
        {
            if (e.Exception.ToString().Contains(nameof(AsyncAcceptor), StringComparison.Ordinal))
                unobserved = e.Exception;
        };

        TaskScheduler.UnobservedTaskException += handler;
        try
        {
            await action();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        finally
        {
            TaskScheduler.UnobservedTaskException -= handler;
        }
        return unobserved;
    }

    [Fact]
    public void IsListening_BeforeBeginAccept_IsFalse()
    {
        var acceptor = new AsyncAcceptor();

        Assert.False(acceptor.IsListening);
    }

    [Fact]
    public async Task IsListening_WhileAccepting_IsTrue()
    {
        var acceptor = StartedAcceptor();
        acceptor.BeginAccept(static _ => { });

        Assert.True(acceptor.IsListening);

        await acceptor.DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_WithAcceptPending_EndsLoopWithoutFault()
    {
        var acceptor = StartedAcceptor();
        acceptor.BeginAccept(static _ => { });

        await acceptor.DisposeAsync().AsTask().WaitAsync(Patience, Ct);

        Assert.True(acceptor.AcceptLoop.IsCompletedSuccessfully);
        Assert.False(acceptor.IsListening);
    }

    [Fact]
    public async Task DisposeAsync_WithAcceptPending_LeavesNothingUnobserved()
    {
        var unobserved = await CaptureUnobservedDuring(static async () =>
        {
            var acceptor = StartedAcceptor();
            acceptor.BeginAccept(static _ => { });
            await acceptor.DisposeAsync();
        });

        Assert.Null(unobserved);
    }

    [Fact]
    public async Task DisposeAsync_CalledTwice_DoesNotThrow()
    {
        var acceptor = StartedAcceptor();
        acceptor.BeginAccept(static _ => { });

        await acceptor.DisposeAsync();
        await acceptor.DisposeAsync();

        Assert.True(acceptor.AcceptLoop.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task DisposeAsync_WithoutBeginAccept_ReleasesPort()
    {
        var acceptor = StartedAcceptor();
        int port = acceptor.LocalEndPoint.Port;

        await acceptor.DisposeAsync();

        var next = new TcpListener(IPAddress.Loopback, port);
        next.Start();
        next.Stop();
    }

    [Fact]
    public async Task DisposeAsync_WithAcceptPending_ReleasesPort()
    {
        var acceptor = StartedAcceptor();
        int port = acceptor.LocalEndPoint.Port;
        acceptor.BeginAccept(static _ => { });

        await acceptor.DisposeAsync();

        var next = new TcpListener(IPAddress.Loopback, port);
        next.Start();
        next.Stop();
    }

    [Fact]
    public async Task DisposeAsync_WhileHandlerRuns_WaitsForHandler()
    {
        var acceptor = StartedAcceptor();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new ManualResetEventSlim();
        acceptor.BeginAccept(socket =>
        {
            socket.Dispose();
            entered.SetResult();
            release.Wait(Patience);
        });

        using var client = Connect(acceptor);
        await entered.Task.WaitAsync(Patience, Ct);
        var dispose = acceptor.DisposeAsync().AsTask();

        Assert.False(dispose.IsCompleted);

        release.Set();
        await dispose.WaitAsync(Patience, Ct);
    }

    [Fact]
    public async Task BeginAccept_Connections_EachReachHandler()
    {
        var acceptor = StartedAcceptor();
        var accepted = new ConcurrentQueue<Socket>();
        var allThere = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        acceptor.BeginAccept(socket =>
        {
            accepted.Enqueue(socket);
            if (accepted.Count == 3)
                allThere.TrySetResult();
        });

        using var first = Connect(acceptor);
        using var second = Connect(acceptor);
        using var third = Connect(acceptor);
        await allThere.Task.WaitAsync(Patience, Ct);
        await acceptor.DisposeAsync();

        Assert.Equal(3, accepted.Count);
        foreach (var socket in accepted)
            socket.Dispose();
    }

    [Fact]
    public async Task BeginAccept_HandlerThrows_ClosesThatSocketAndKeepsAccepting()
    {
        var acceptor = StartedAcceptor();
        var survivor = new TaskCompletionSource<Socket>(TaskCreationOptions.RunContinuationsAsynchronously);
        int connections = 0;
        acceptor.BeginAccept(socket =>
        {
            if (Interlocked.Increment(ref connections) == 1)
                throw new InvalidOperationException("setup failed");

            survivor.SetResult(socket);
        });

        using var rejected = Connect(acceptor);
        rejected.ReceiveTimeout = (int)Patience.TotalMilliseconds;
        int read;
        try
        {
            read = rejected.Receive(new byte[1]);
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
        {
            read = 0;
        }

        using var second = Connect(acceptor);
        using var accepted = await survivor.Task.WaitAsync(Patience, Ct);
        await acceptor.DisposeAsync();

        Assert.Equal(0, read);
        Assert.True(acceptor.AcceptLoop.IsCompletedSuccessfully);
    }

    [Theory]
    [InlineData(SocketError.ConnectionReset)]
    [InlineData(SocketError.ConnectionAborted)]
    public async Task AcceptLoop_ConnectionLostInBacklog_RetriesWithoutWaiting(SocketError error)
    {
        var exhausted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var accept = ScriptedAccept(exhausted, new SocketException((int)error));
        using var stopping = new CancellationTokenSource();

        // Far longer than the test may take: reaching the next accept proves nothing waited.
        var loop = AsyncAcceptor.AcceptLoopAsync(accept, static _ => { }, TimeSpan.FromHours(1), stopping.Token);
        await exhausted.Task.WaitAsync(Patience, Ct);
        await stopping.CancelAsync();
        await loop.WaitAsync(Patience, Ct);

        Assert.True(loop.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task AcceptLoop_SocketFailure_RetriesAfterDelay()
    {
        var exhausted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var accept = ScriptedAccept(exhausted,
            new SocketException((int)SocketError.TooManyOpenSockets),
            new SocketException((int)SocketError.NoBufferSpaceAvailable));
        using var stopping = new CancellationTokenSource();

        var loop = AsyncAcceptor.AcceptLoopAsync(accept, static _ => { }, TimeSpan.FromMilliseconds(1), stopping.Token);
        await exhausted.Task.WaitAsync(Patience, Ct);
        await stopping.CancelAsync();
        await loop.WaitAsync(Patience, Ct);

        Assert.True(loop.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task AcceptLoop_StoppedDuringRetryDelay_EndsAtOnce()
    {
        var exhausted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var accept = ScriptedAccept(exhausted, new SocketException((int)SocketError.TooManyOpenSockets));
        using var stopping = new CancellationTokenSource();

        var loop = AsyncAcceptor.AcceptLoopAsync(accept, static _ => { }, TimeSpan.FromHours(1), stopping.Token);
        await stopping.CancelAsync();
        await loop.WaitAsync(Patience, Ct);

        Assert.True(loop.IsCompletedSuccessfully);
        Assert.False(exhausted.Task.IsCompleted);
    }

    [Fact]
    public async Task AcceptLoop_UnexpectedException_EndsWithoutFault()
    {
        var exhausted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var accept = ScriptedAccept(exhausted, new InvalidOperationException("a bug"));

        await AsyncAcceptor.AcceptLoopAsync(accept, static _ => { }, TimeSpan.FromMilliseconds(1), CancellationToken.None)
            .WaitAsync(Patience, Ct);

        Assert.False(exhausted.Task.IsCompleted);
    }

    [Fact]
    public async Task AcceptLoop_CancelledWithoutStopping_IsNotTreatedAsStop()
    {
        var exhausted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var accept = ScriptedAccept(exhausted, new OperationCanceledException());
        using var stopping = new CancellationTokenSource();

        await AsyncAcceptor.AcceptLoopAsync(accept, static _ => { }, TimeSpan.FromMilliseconds(1), stopping.Token)
            .WaitAsync(Patience, Ct);

        // A cancellation nobody asked for ends the loop as a logged failure, never as a fault.
        Assert.False(exhausted.Task.IsCompleted);
    }
}
