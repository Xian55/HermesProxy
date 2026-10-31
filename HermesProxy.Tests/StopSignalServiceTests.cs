using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace HermesProxy.Tests;

/// <summary>
/// Cover for <see cref="StopSignalService"/>, the named event test-loop2 sets to stop the proxy
/// gracefully instead of <c>Stop-Process -Force</c>.
/// </summary>
public class StopSignalServiceTests
{
    private sealed class FakeLifetime : IHostApplicationLifetime
    {
        public readonly TaskCompletionSource Stopped = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public CancellationToken ApplicationStarted => CancellationToken.None;
        public CancellationToken ApplicationStopping => CancellationToken.None;
        public CancellationToken ApplicationStopped => CancellationToken.None;
        public void StopApplication() => Stopped.TrySetResult();
    }

    // ExecuteAsync runs on a background thread (.NET 10), so the event appears shortly after
    // StartAsync returns rather than before it.
    [SupportedOSPlatform("windows")]
    private static async Task<EventWaitHandle> WaitForEvent(string name)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        EventWaitHandle? handle;
        while (!EventWaitHandle.TryOpenExisting(name, out handle))
        {
            Assert.True(DateTime.UtcNow < deadline, $"{name} was never created");
            await Task.Delay(10, TestContext.Current.CancellationToken);
        }
        return handle;
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public async Task SettingTheEvent_StopsTheHost()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "Named events are Windows-only; SIGTERM stops the host elsewhere.");

        var lifetime = new FakeLifetime();
        using var service = new StopSignalService(lifetime);
        await service.StartAsync(TestContext.Current.CancellationToken);

        using (var stopEvent = await WaitForEvent(StopSignalService.EventName(Environment.ProcessId)))
            stopEvent.Set();

        await lifetime.Stopped.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.True(StopSignalService.Requested);

        await service.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public async Task StoppingTheHost_ReleasesTheEvent()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "Named events are Windows-only; SIGTERM stops the host elsewhere.");

        var lifetime = new FakeLifetime();
        using var service = new StopSignalService(lifetime);
        await service.StartAsync(TestContext.Current.CancellationToken);
        string name = StopSignalService.EventName(Environment.ProcessId);
        (await WaitForEvent(name)).Dispose();

        await service.StopAsync(TestContext.Current.CancellationToken);

        // The host stopped for its own reasons: no stop request, and no event left for a launcher
        // to set on a proxy that is already gone. Unregistering the thread-pool wait is
        // asynchronous, so the wait thread may hold the handle a moment past StopAsync.
        Assert.False(lifetime.Stopped.Task.IsCompleted);
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (EventWaitHandle.TryOpenExisting(name, out var lingering))
        {
            lingering.Dispose();
            Assert.True(DateTime.UtcNow < deadline, $"{name} still exists after the host stopped");
            await Task.Delay(10, TestContext.Current.CancellationToken);
        }
    }
}
