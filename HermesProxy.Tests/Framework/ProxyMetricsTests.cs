using Framework.Metrics;
using System.Threading.Tasks;
using Xunit;

namespace HermesProxy.Tests.Framework
{
    public class ProxyMetricsTests
    {
        [Fact]
        public void RecordClientToServerLatency_TracksLatency()
        {
            var metrics = new ProxyMetrics();

            metrics.RecordClientToServerLatency(0x1234, 1.5);
            metrics.RecordClientToServerLatency(0x1234, 2.5);
            metrics.RecordClientToServerLatency(0x1234, 3.5);

            var stats = metrics.GetClientToServerStats(0x1234);
            Assert.NotNull(stats);
            Assert.Equal(3, stats.Value.Count);
            Assert.Equal(1.5, stats.Value.Min);
            Assert.Equal(3.5, stats.Value.Max);
            Assert.Equal(2.5, stats.Value.Average, 2);
        }

        [Fact]
        public void RecordServerToClientLatency_TracksLatency()
        {
            var metrics = new ProxyMetrics();

            metrics.RecordServerToClientLatency(0x5678, 10.0);
            metrics.RecordServerToClientLatency(0x5678, 20.0);

            var stats = metrics.GetServerToClientStats(0x5678);
            Assert.NotNull(stats);
            Assert.Equal(2, stats.Value.Count);
            Assert.Equal(10.0, stats.Value.Min);
            Assert.Equal(20.0, stats.Value.Max);
            Assert.Equal(15.0, stats.Value.Average, 2);
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

            // Add 100 samples: 1, 2, 3, ..., 100
            for (int i = 1; i <= 100; i++)
            {
                metrics.RecordClientToServerLatency(0x1234, i);
            }

            var stats = metrics.GetClientToServerStats(0x1234);
            Assert.NotNull(stats);
            Assert.Equal(100, stats.Value.Count);
            Assert.Equal(1.0, stats.Value.Min);
            Assert.Equal(100.0, stats.Value.Max);
            Assert.Equal(50.5, stats.Value.Average, 2);
            Assert.Equal(50.5, stats.Value.P50, 1); // Median
            Assert.Equal(95.05, stats.Value.P95, 1); // 95th percentile
            Assert.Equal(99.01, stats.Value.P99, 1); // 99th percentile
        }

        [Fact]
        public void CircularBuffer_OverwritesOldSamples()
        {
            var samples = new LatencySamples(5);

            // Add 10 samples, only last 5 should be kept
            for (int i = 1; i <= 10; i++)
            {
                samples.Add(i);
            }

            var stats = samples.GetStats();
            Assert.Equal(5, stats.Count);
            // Average of 6, 7, 8, 9, 10 = 8
            Assert.Equal(8.0, stats.Average, 2);
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
        }

        [Fact]
        public void ThreadSafety_ConcurrentRecording()
        {
            var metrics = new ProxyMetrics();
            const int iterations = 10000;

            Parallel.For(0, iterations, i =>
            {
                metrics.RecordClientToServerLatency(0x1234, i * 0.001);
                metrics.RecordServerToClientLatency(0x5678, i * 0.001);
            });

            var c2sStats = metrics.GetClientToServerStats(0x1234);
            var s2cStats = metrics.GetServerToClientStats(0x5678);

            Assert.NotNull(c2sStats);
            Assert.NotNull(s2cStats);
            // Should have samples (may be less than iterations due to circular buffer)
            Assert.True(c2sStats.Value.Count > 0);
            Assert.True(s2cStats.Value.Count > 0);
        }

        [Fact]
        public void GetSummary_ReturnsFormattedString()
        {
            var metrics = new ProxyMetrics();

            metrics.RecordClientToServerLatency(0x1234, 1.5);
            metrics.RecordServerToClientLatency(0x5678, 2.5);

            var summary = metrics.GetSummary();

            Assert.Contains("Client -> Server", summary);
            Assert.Contains("Server -> Client", summary);
            Assert.Contains("0x1234", summary);
            Assert.Contains("0x5678", summary);
        }

        [Fact]
        public void MultipleOpcodes_TrackedSeparately()
        {
            var metrics = new ProxyMetrics();

            metrics.RecordClientToServerLatency(0x0001, 1.0);
            metrics.RecordClientToServerLatency(0x0002, 2.0);
            metrics.RecordClientToServerLatency(0x0003, 3.0);

            Assert.Equal(3, metrics.ClientToServerOpcodeCount);

            var stats1 = metrics.GetClientToServerStats(0x0001);
            var stats2 = metrics.GetClientToServerStats(0x0002);
            var stats3 = metrics.GetClientToServerStats(0x0003);

            Assert.Equal(1.0, stats1?.Average);
            Assert.Equal(2.0, stats2?.Average);
            Assert.Equal(3.0, stats3?.Average);
        }
    }
}
