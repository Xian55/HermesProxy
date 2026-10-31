using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Framework.Networking;
using Xunit;

namespace HermesProxy.Tests.Framework;

public sealed class ConnectionSweeperTests
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan Fast = TimeSpan.FromMilliseconds(1);
    private static readonly TimeSpan NeverDuringATest = TimeSpan.FromHours(1);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private sealed class MockSocket : ISocket
    {
        private int _updates;

        public bool IsOpenValue { get; set; } = true;
        public bool UpdateReturnValue { get; set; } = true;
        public bool UpdateThrows { get; set; }
        public bool CloseThrows { get; set; }
        public int UpdateCallCount => Volatile.Read(ref _updates);
        public int CloseSocketCallCount { get; private set; }

        public void Accept() { }

        public bool Update()
        {
            Interlocked.Increment(ref _updates);
            if (UpdateThrows)
                throw new InvalidOperationException("update failed");

            return UpdateReturnValue;
        }

        public bool IsOpen() => IsOpenValue;

        public void CloseSocket()
        {
            CloseSocketCallCount++;
            if (CloseThrows)
                throw new InvalidOperationException("close failed");

            IsOpenValue = false;
        }
    }

    private static ConnectionSweeper<MockSocket> IdleSweeper() => new(NeverDuringATest);

    private static async Task WaitUntil(Func<bool> condition)
    {
        using var deadline = new CancellationTokenSource(Patience);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(deadline.Token, Ct);
        while (!condition())
            await Task.Delay(1, linked.Token);
    }

    [Fact]
    public void Add_CountsEachConnection()
    {
        var sweeper = IdleSweeper();

        sweeper.Add(new MockSocket());
        sweeper.Add(new MockSocket());
        sweeper.Add(new MockSocket());

        Assert.Equal(3, sweeper.ConnectionCount);
    }

    [Fact]
    public void Sweep_ConnectedSocket_IsCheckedAndKept()
    {
        var sweeper = IdleSweeper();
        var socket = new MockSocket();
        sweeper.Add(socket);

        sweeper.Sweep();

        Assert.Equal(1, socket.UpdateCallCount);
        Assert.Equal(1, sweeper.ConnectionCount);
    }

    [Fact]
    public void Sweep_SocketThatEnded_IsForgotten()
    {
        var sweeper = IdleSweeper();
        var socket = new MockSocket { UpdateReturnValue = false };
        sweeper.Add(socket);

        sweeper.Sweep();
        sweeper.Sweep();

        Assert.Equal(1, socket.UpdateCallCount);
        Assert.Equal(0, sweeper.ConnectionCount);
    }

    [Fact]
    public void Sweep_SocketAlreadyClosedWhenAdded_IsNeverChecked()
    {
        var sweeper = IdleSweeper();
        var socket = new MockSocket { IsOpenValue = false };
        sweeper.Add(socket);

        sweeper.Sweep();

        Assert.Equal(0, socket.UpdateCallCount);
        Assert.Equal(0, sweeper.ConnectionCount);
    }

    [Fact]
    public void Sweep_UpdateFalseWhileStillOpen_ClosesSocket()
    {
        var sweeper = IdleSweeper();
        var socket = new MockSocket { UpdateReturnValue = false };
        sweeper.Add(socket);

        sweeper.Sweep();

        Assert.Equal(1, socket.CloseSocketCallCount);
    }

    [Fact]
    public void Sweep_UpdateFalseAndAlreadyClosed_DoesNotCloseAgain()
    {
        var sweeper = IdleSweeper();
        var socket = new MockSocket();
        sweeper.Add(socket);
        sweeper.Sweep();
        socket.UpdateReturnValue = false;
        socket.IsOpenValue = false;

        sweeper.Sweep();

        Assert.Equal(0, socket.CloseSocketCallCount);
    }

    [Fact]
    public void Sweep_EverySocketEnded_RemovesAllWithoutSkipping()
    {
        var sweeper = IdleSweeper();
        var sockets = Enumerable.Range(0, 10).Select(_ => new MockSocket { UpdateReturnValue = false }).ToList();
        sockets.ForEach(sweeper.Add);

        sweeper.Sweep();

        Assert.Equal(0, sweeper.ConnectionCount);
        Assert.All(sockets, socket => Assert.Equal(1, socket.UpdateCallCount));
    }

    [Theory]
    [InlineData(10, 2)]
    [InlineData(100, 3)]
    public void Sweep_MixedSockets_KeepsOnlyTheLiving(int count, int everyNthEnds)
    {
        var sweeper = IdleSweeper();
        var sockets = Enumerable.Range(0, count)
            .Select(i => new MockSocket { UpdateReturnValue = i % everyNthEnds != 0 })
            .ToList();
        sockets.ForEach(sweeper.Add);

        sweeper.Sweep();
        sweeper.Sweep();

        Assert.Equal(sockets.Count(s => s.UpdateReturnValue), sweeper.ConnectionCount);
        Assert.All(sockets.Where(s => s.UpdateReturnValue), socket => Assert.Equal(2, socket.UpdateCallCount));
        Assert.All(sockets.Where(s => !s.UpdateReturnValue), socket => Assert.Equal(1, socket.UpdateCallCount));
    }

    [Fact]
    public void Sweep_SocketAddedBetweenSweeps_IsCheckedInTheNext()
    {
        var sweeper = IdleSweeper();
        sweeper.Add(new MockSocket());
        sweeper.Sweep();
        var late = new MockSocket();

        sweeper.Add(late);
        sweeper.Sweep();

        Assert.Equal(1, late.UpdateCallCount);
        Assert.Equal(2, sweeper.ConnectionCount);
    }

    [Fact]
    public void Sweep_UpdateThrows_DropsThatSocketAndChecksTheRest()
    {
        var sweeper = IdleSweeper();
        var before = new MockSocket();
        var broken = new MockSocket { UpdateThrows = true };
        var after = new MockSocket();
        sweeper.Add(before);
        sweeper.Add(broken);
        sweeper.Add(after);

        sweeper.Sweep();

        Assert.Equal(1, before.UpdateCallCount);
        Assert.Equal(1, after.UpdateCallCount);
        Assert.Equal(1, broken.CloseSocketCallCount);
        Assert.Equal(2, sweeper.ConnectionCount);
    }

    [Fact]
    public void Sweep_CloseThrows_StillForgetsTheSocket()
    {
        var sweeper = IdleSweeper();
        var socket = new MockSocket { UpdateReturnValue = false, CloseThrows = true };
        var other = new MockSocket();
        sweeper.Add(socket);
        sweeper.Add(other);

        sweeper.Sweep();

        Assert.Equal(1, sweeper.ConnectionCount);
        Assert.Equal(1, other.UpdateCallCount);
    }

    [Fact]
    public async Task Start_SweepsOnTheInterval()
    {
        var sweeper = new ConnectionSweeper<MockSocket>(Fast);
        var socket = new MockSocket();
        sweeper.Add(socket);

        sweeper.Start();
        await WaitUntil(() => socket.UpdateCallCount >= 2);
        await sweeper.DisposeAsync();

        Assert.True(sweeper.SweepLoop.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task DisposeAsync_WhileWaitingForATick_EndsWithoutFault()
    {
        var sweeper = IdleSweeper();
        sweeper.Start();

        await sweeper.DisposeAsync().AsTask().WaitAsync(Patience, Ct);

        Assert.True(sweeper.SweepLoop.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task DisposeAsync_CalledTwice_ClosesOnce()
    {
        var sweeper = IdleSweeper();
        var socket = new MockSocket();
        sweeper.Add(socket);
        sweeper.Sweep();

        await sweeper.DisposeAsync();
        await sweeper.DisposeAsync();

        Assert.Equal(1, socket.CloseSocketCallCount);
    }

    [Fact]
    public async Task DisposeAsync_WithoutStart_StillClosesSockets()
    {
        var sweeper = IdleSweeper();
        var socket = new MockSocket();
        sweeper.Add(socket);

        await sweeper.DisposeAsync();

        Assert.Equal(1, socket.CloseSocketCallCount);
    }

    [Fact]
    public async Task DisposeAsync_ClosesSweptAndNotYetSweptSockets()
    {
        var sweeper = IdleSweeper();
        var swept = new MockSocket();
        var pending = new MockSocket();
        sweeper.Add(swept);
        sweeper.Sweep();
        sweeper.Add(pending);

        await sweeper.DisposeAsync();

        Assert.Equal(1, swept.CloseSocketCallCount);
        Assert.Equal(1, pending.CloseSocketCallCount);
    }

    [Fact]
    public async Task DisposeAsync_OneCloseThrows_ClosesTheRest()
    {
        var sweeper = IdleSweeper();
        var broken = new MockSocket { CloseThrows = true };
        var others = new List<MockSocket> { new(), new() };
        sweeper.Add(broken);
        others.ForEach(sweeper.Add);
        sweeper.Sweep();

        await sweeper.DisposeAsync();

        Assert.All(others, socket => Assert.Equal(1, socket.CloseSocketCallCount));
    }

    [Fact]
    public async Task DisposeAsync_AlreadyClosedSocket_IsNotClosedAgain()
    {
        var sweeper = IdleSweeper();
        var socket = new MockSocket();
        sweeper.Add(socket);
        sweeper.Sweep();
        socket.IsOpenValue = false;

        await sweeper.DisposeAsync();

        Assert.Equal(0, socket.CloseSocketCallCount);
    }
}
