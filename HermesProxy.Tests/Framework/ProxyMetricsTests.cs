using System;
using Framework.Metrics;
using System.Threading.Tasks;
using Xunit;

namespace HermesProxy.Tests.Framework;

public class ProxyMetricsTests
{
    private enum TestOpcode : uint
    {
        Alpha = 0x1234,
        Beta = 0x5678,
    }

    [Fact]
    public void RecordClientToServerLatency_TracksLatency()
    {
        var metrics = new ProxyMetrics();

        metrics.RecordClientToServerLatency(0x1234, 1.5);
        metrics.RecordClientToServerLatency(0x1234, 2.5);
        metrics.RecordClientToServerLatency(0x1234, 3.5);

        var stats = metrics.GetClientToServerStats(0x1234);
        Assert.NotNull(stats);
        Assert.Equal(3, stats.Value.Latency.Count);
        Assert.Equal(1.5, stats.Value.Latency.Min);
        Assert.Equal(3.5, stats.Value.Latency.Max);
        Assert.Equal(2.5, stats.Value.Latency.Average, 2);
    }

    [Fact]
    public void RecordServerToClientLatency_TracksLatency()
    {
        var metrics = new ProxyMetrics();

        metrics.RecordServerToClientLatency(0x5678, 10.0);
        metrics.RecordServerToClientLatency(0x5678, 20.0);

        var stats = metrics.GetServerToClientStats(0x5678);
        Assert.NotNull(stats);
        Assert.Equal(2, stats.Value.Latency.Count);
        Assert.Equal(10.0, stats.Value.Latency.Min);
        Assert.Equal(20.0, stats.Value.Latency.Max);
        Assert.Equal(15.0, stats.Value.Latency.Average, 2);
    }

    [Fact]
    public void RecordWithAllocation_TracksBytesAndLifetimeTotals()
    {
        var metrics = new ProxyMetrics();

        metrics.RecordClientToServer(0x1234, 1.0, 100);
        metrics.RecordClientToServer(0x1234, 1.0, 300);

        var stats = metrics.GetClientToServerStats(0x1234);
        Assert.NotNull(stats);
        Assert.Equal(2, stats.Value.Allocation.Count);
        Assert.Equal(2, stats.Value.Allocation.TotalCount);
        Assert.Equal(400.0, stats.Value.Allocation.TotalSum);
        Assert.Equal(100.0, stats.Value.Allocation.Min);
        Assert.Equal(300.0, stats.Value.Allocation.Max);
        Assert.Equal(200.0, stats.Value.Allocation.Average, 2);
        Assert.Equal(2, metrics.ClientToServerPacketCount);
    }

    [Fact]
    public void GenericEnumOverload_RoutesToSameKeyAsInt()
    {
        var metrics = new ProxyMetrics();

        metrics.RecordClientToServer(TestOpcode.Alpha, 2.0, 64);
        metrics.RecordServerToClient(TestOpcode.Beta, 4.0, 128);

        Assert.Equal(64.0, metrics.GetClientToServerStats((int)TestOpcode.Alpha)?.Allocation.TotalSum);
        Assert.Equal(128.0, metrics.GetServerToClientStats((int)TestOpcode.Beta)?.Allocation.TotalSum);
    }

    [Fact]
    public void GenericEnumOverload_DoesNotBoxAfterWarmup()
    {
        var metrics = new ProxyMetrics();

        // First call pays for the ConcurrentDictionary entry and the sample buffers.
        metrics.RecordClientToServer(TestOpcode.Alpha, 1.0, 0);

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            metrics.RecordClientToServer(TestOpcode.Alpha, 1.0, 0);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(0, allocated);
    }

    [Fact]
    public void GetStats_ReturnsNullForUnknownOpcode()
    {
        var metrics = new ProxyMetrics();

        var stats = metrics.GetClientToServerStats(0x9999);
        Assert.Null(stats);
    }

    [Fact]
    public void Percentiles_CalculatedCorrectly()
    {
        var metrics = new ProxyMetrics();

        for (int i = 1; i <= 100; i++)
            metrics.RecordClientToServerLatency(0x1234, i);

        var stats = metrics.GetClientToServerStats(0x1234);
        Assert.NotNull(stats);
        var latency = stats.Value.Latency;
        Assert.Equal(100, latency.Count);
        Assert.Equal(1.0, latency.Min);
        Assert.Equal(100.0, latency.Max);
        Assert.Equal(50.5, latency.Average, 2);
        Assert.Equal(50.5, latency.P50, 1);
        Assert.Equal(95.05, latency.P95, 1);
        Assert.Equal(99.01, latency.P99, 1);
    }

    [Fact]
    public void CircularBuffer_OverwritesOldSamples_KeepsLifetimeTotals()
    {
        var samples = new SampleWindow(5);

        for (int i = 1; i <= 10; i++)
            samples.Add(i);

        var stats = samples.GetStats();
        Assert.Equal(5, stats.Count);
        Assert.Equal(8.0, stats.Average, 2); // window holds 6..10
        Assert.Equal(10, stats.TotalCount);
        Assert.Equal(55.0, stats.TotalSum); // 1..10
        Assert.Equal(1.0, stats.Min);
        Assert.Equal(10.0, stats.Max);
    }

    [Fact]
    public void Reset_ClearsAllMetrics()
    {
        var metrics = new ProxyMetrics();

        metrics.RecordClientToServerLatency(0x1234, 1.0);
        metrics.RecordServerToClientLatency(0x5678, 2.0);

        metrics.Reset();

        Assert.Equal(0, metrics.ClientToServerOpcodeCount);
        Assert.Equal(0, metrics.ServerToClientOpcodeCount);
        Assert.Equal(0, metrics.ClientToServerPacketCount);
        Assert.Equal(0, metrics.ServerToClientPacketCount);
    }

    [Fact]
    public void ThreadSafety_ConcurrentRecording()
    {
        var metrics = new ProxyMetrics();
        const int iterations = 10000;

        Parallel.For(0, iterations, i =>
        {
            metrics.RecordClientToServer(0x1234, i * 0.001, i);
            metrics.RecordServerToClient(0x5678, i * 0.001, i);
        });

        var c2sStats = metrics.GetClientToServerStats(0x1234);
        var s2cStats = metrics.GetServerToClientStats(0x5678);

        Assert.NotNull(c2sStats);
        Assert.NotNull(s2cStats);
        Assert.Equal(iterations, c2sStats.Value.Latency.TotalCount);
        Assert.Equal(iterations, s2cStats.Value.Allocation.TotalCount);
        Assert.Equal(iterations, metrics.ClientToServerPacketCount);
    }

    [Fact]
    public void GetSummary_ReturnsFormattedString()
    {
        var metrics = new ProxyMetrics();

        metrics.RecordClientToServer(0x1234, 1.5, 512);
        metrics.RecordServerToClient(0x5678, 2.5, 0);

        var summary = metrics.GetSummary();

        Assert.Contains("Client -> Server (top 1 by max latency this interval", summary);
        Assert.Contains("Client -> Server (top 1 by allocated bytes", summary);
        Assert.Contains("Server -> Client (top 1 by max latency this interval", summary);
        // Zero bytes recorded on this side, so no allocation table.
        Assert.DoesNotContain("Server -> Client (top 1 by allocated bytes", summary);
        Assert.Contains("0x1234", summary);
        Assert.Contains("0x5678", summary);
        Assert.Contains("C->S 1 pkts", summary);
    }

    [Fact]
    public void SampleWindow_ResetInterval_KeepsLifetimeMaxAndWindow()
    {
        var samples = new SampleWindow(5);
        samples.Add(100);
        samples.ResetInterval();
        samples.Add(3);
        samples.Add(7);

        var stats = samples.GetStats();
        Assert.Equal(2, stats.IntervalCount);
        Assert.Equal(7.0, stats.IntervalMax);
        Assert.Equal(100.0, stats.Max);
        Assert.Equal(3, stats.Count);
        Assert.Equal(3, stats.TotalCount);
    }

    [Fact]
    public void SampleWindow_IntervalWithNoSamples_ReportsZero()
    {
        var samples = new SampleWindow(5);
        samples.Add(42);
        samples.ResetInterval();

        var stats = samples.GetStats();
        Assert.Equal(0, stats.IntervalCount);
        Assert.Equal(0.0, stats.IntervalMax);
        Assert.Equal(42.0, stats.Max);
    }

    [Fact]
    public void GetSummary_ClosesInterval_LoginSpikeDoesNotMaskLaterMax()
    {
        var metrics = new ProxyMetrics();
        metrics.RecordServerToClientLatency(0x1234, 135.0); // login burst
        metrics.GetSummary();

        metrics.RecordServerToClientLatency(0x1234, 4.0);
        metrics.RecordServerToClientLatency(0x1234, 9.0);

        var stats = metrics.GetServerToClientStats(0x1234);
        Assert.NotNull(stats);
        Assert.Equal(9.0, stats.Value.Latency.IntervalMax);
        Assert.Equal(2, stats.Value.Latency.IntervalCount);
        Assert.Equal(135.0, stats.Value.Latency.Max);
    }

    [Fact]
    public void GetStats_WithoutReset_LeavesIntervalOpen()
    {
        var metrics = new ProxyMetrics();
        metrics.RecordClientToServerLatency(0x1234, 5.0);

        metrics.GetClientToServerStats();
        metrics.RecordClientToServerLatency(0x1234, 1.0);

        var stats = metrics.GetClientToServerStats(0x1234);
        Assert.NotNull(stats);
        Assert.Equal(2, stats.Value.Latency.IntervalCount);
        Assert.Equal(5.0, stats.Value.Latency.IntervalMax);
    }

    [Fact]
    public void GetSummary_LatencyTable_OmitsOpcodesIdleThisInterval_RanksByIntervalMax()
    {
        var metrics = new ProxyMetrics();
        metrics.RecordServerToClientLatency(0x1111, 500.0); // one-off, previous interval
        metrics.RecordServerToClientLatency(0x2222, 1.0);
        metrics.GetSummary();

        metrics.RecordServerToClientLatency(0x2222, 2.0);
        metrics.RecordServerToClientLatency(0x3333, 30.0);

        var summary = metrics.GetSummary(opcodeResolver: static op => $"OP_{op:X4}");

        Assert.Contains("Server -> Client (top 2 by max latency this interval", summary);
        Assert.DoesNotContain("OP_1111", summary);
        Assert.True(summary.IndexOf("OP_3333", StringComparison.Ordinal) < summary.IndexOf("OP_2222", StringComparison.Ordinal));
    }

    [Fact]
    public void GetSummary_IncludesGcLineWhenSupplied()
    {
        var metrics = new ProxyMetrics();
        var gc = new GcDelta(TimeSpan.FromSeconds(60), 3, 1, 0, 60L * 1024 * 1024, 0.5, 100L * 1024 * 1024);

        var summary = metrics.GetSummary(gc: gc);

        Assert.Contains("gen0 +3 gen1 +1 gen2 +0", summary);
        Assert.Contains("allocated 60.0 MB (1.00 MB/s)", summary);
        Assert.Contains("heap 100.0 MB", summary);
    }

    [Fact]
    public void CaptureGcDelta_ReflectsAllocationSinceLastCapture()
    {
        var metrics = new ProxyMetrics();
        metrics.CaptureGcDelta(); // arm

        var keepAlive = new byte[4 * 1024 * 1024];
        GC.KeepAlive(keepAlive);

        var delta = metrics.CaptureGcDelta();

        Assert.True(delta.AllocatedBytes >= keepAlive.Length, $"expected at least {keepAlive.Length} bytes, saw {delta.AllocatedBytes}");
        Assert.True(delta.Elapsed >= TimeSpan.Zero);
        Assert.True(delta.HeapSizeBytes > 0);
    }

    [Fact]
    public void MultipleOpcodes_TrackedSeparately()
    {
        var metrics = new ProxyMetrics();

        metrics.RecordClientToServerLatency(0x0001, 1.0);
        metrics.RecordClientToServerLatency(0x0002, 2.0);
        metrics.RecordClientToServerLatency(0x0003, 3.0);

        Assert.Equal(3, metrics.ClientToServerOpcodeCount);

        Assert.Equal(1.0, metrics.GetClientToServerStats(0x0001)?.Latency.Average);
        Assert.Equal(2.0, metrics.GetClientToServerStats(0x0002)?.Latency.Average);
        Assert.Equal(3.0, metrics.GetClientToServerStats(0x0003)?.Latency.Average);
    }
}
