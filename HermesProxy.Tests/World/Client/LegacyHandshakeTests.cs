using System;
using System.Threading;
using System.Threading.Tasks;
using HermesProxy.World.Client;
using Xunit;

namespace HermesProxy.Tests.World.Client;

public class LegacyHandshakeTests
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);

    [Fact]
    public void New_IsPending()
    {
        var handshake = new LegacyHandshake();

        Assert.True(handshake.IsPending);
        Assert.False(handshake.IsAccepted);
        Assert.False(handshake.IsRefused);
    }

    [Fact]
    public void Wait_NothingSettlesIt_ReturnsFalse()
    {
        var handshake = new LegacyHandshake();

        Assert.False(handshake.Wait(TimeSpan.FromMilliseconds(20)));
        Assert.True(handshake.IsPending);
    }

    [Fact]
    public async Task Wait_AnotherThreadAccepts_Wakes()
    {
        var handshake = new LegacyHandshake();
        var waiter = Task.Run(() => handshake.Wait(Patience), TestContext.Current.CancellationToken);

        handshake.Accept();

        Assert.True(await waiter.WaitAsync(Patience, TestContext.Current.CancellationToken));
        Assert.True(handshake.IsAccepted);
    }

    [Fact]
    public async Task Wait_AnotherThreadRefuses_Wakes()
    {
        var handshake = new LegacyHandshake();
        var waiter = Task.Run(() => handshake.Wait(Patience), TestContext.Current.CancellationToken);

        Assert.True(handshake.TryRefuse());

        Assert.True(await waiter.WaitAsync(Patience, TestContext.Current.CancellationToken));
        Assert.True(handshake.IsRefused);
    }

    [Fact]
    public void Refuse_AfterAccept_Replaces_TheWayTheEndOfAQueueCan()
    {
        var handshake = new LegacyHandshake();
        handshake.Accept();

        handshake.Refuse();

        Assert.True(handshake.IsRefused);
        Assert.True(handshake.Wait(TimeSpan.Zero));
    }

    [Fact]
    public void Accept_AfterAccept_StaysAccepted_TheWayEachQueueUpdateAccepts()
    {
        var handshake = new LegacyHandshake();
        handshake.Accept();

        handshake.Accept();

        Assert.True(handshake.IsAccepted);
    }

    [Fact]
    public void TryRefuse_AfterAccept_KeepsTheAnswer()
    {
        var handshake = new LegacyHandshake();
        handshake.Accept();

        Assert.False(handshake.TryRefuse());

        Assert.True(handshake.IsAccepted);
    }

    [Fact]
    public void TryRefuse_Twice_OnlyTheFirstEndsTheWait()
    {
        var handshake = new LegacyHandshake();

        Assert.True(handshake.TryRefuse());
        Assert.False(handshake.TryRefuse());

        Assert.True(handshake.IsRefused);
    }

    [Fact]
    public void TryRefuse_RacingAccept_AlwaysEndsTheWait()
    {
        for (int i = 0; i < 2_000; i++)
        {
            var handshake = new LegacyHandshake();
            using var start = new Barrier(2);

            var accept = Task.Run(() =>
            {
                start.SignalAndWait(TestContext.Current.CancellationToken);
                handshake.Accept();
            }, TestContext.Current.CancellationToken);

            start.SignalAndWait(TestContext.Current.CancellationToken);
            handshake.TryRefuse();
            accept.GetAwaiter().GetResult();

            Assert.False(handshake.IsPending);
            Assert.True(handshake.Wait(Patience));
        }
    }
}
