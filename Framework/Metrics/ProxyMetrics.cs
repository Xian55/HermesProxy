using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Framework.Metrics
{
    /// <summary>
    /// Thread-safe metrics collector for measuring proxy-introduced latency per opcode.
    /// Tracks min, max, average, and percentiles (p50, p95, p99) for packet processing times.
    /// </summary>
    public class ProxyMetrics
    {
        private const int MaxSamplesPerOpcode = 1000;

        // Conversion factor from ticks to milliseconds (cached for performance)
        private static readonly double TicksToMs = 1000.0 / Stopwatch.Frequency;

        // Per-opcode latency samples (circular buffer)
        private readonly ConcurrentDictionary<int, LatencySamples> _clientToServerLatency = new();
        private readonly ConcurrentDictionary<int, LatencySamples> _serverToClientLatency = new();

        private readonly DateTime _startTime = DateTime.UtcNow;

        /// <summary>
        /// Record latency for a packet processed from modern client and forwarded to legacy server.
        /// </summary>
        /// <param name="opcode">The universal opcode</param>
        /// <param name="elapsedTicks">Elapsed ticks from Stopwatch.GetTimestamp()</param>
        public void RecordClientToServerLatency(Enum opcode, long elapsedTicks)
        {
            RecordClientToServerLatency(Convert.ToInt32(opcode), elapsedTicks * TicksToMs);
        }

        /// <summary>
        /// Record latency for a packet processed from legacy server and forwarded to modern client.
        /// </summary>
        /// <param name="opcode">The universal opcode</param>
        /// <param name="elapsedTicks">Elapsed ticks from Stopwatch.GetTimestamp()</param>
        public void RecordServerToClientLatency(Enum opcode, long elapsedTicks)
        {
            RecordServerToClientLatency(Convert.ToInt32(opcode), elapsedTicks * TicksToMs);
        }

        /// <summary>
        /// Record latency in milliseconds for a packet processed from modern client.
        /// Internal overload for testing and direct ms values.
        /// </summary>
        internal void RecordClientToServerLatency(int opcode, double milliseconds)
        {
            var samples = _clientToServerLatency.GetOrAdd(opcode, _ => new LatencySamples(MaxSamplesPerOpcode));
            samples.Add(milliseconds);
        }

        /// <summary>
        /// Record latency in milliseconds for a packet processed from legacy server.
        /// Internal overload for testing and direct ms values.
        /// </summary>
        internal void RecordServerToClientLatency(int opcode, double milliseconds)
        {
            var samples = _serverToClientLatency.GetOrAdd(opcode, _ => new LatencySamples(MaxSamplesPerOpcode));
            samples.Add(milliseconds);
        }

        /// <summary>
        /// Get latency statistics for client-to-server packets by opcode.
        /// </summary>
        public Dictionary<int, LatencyStats> GetClientToServerStats()
        {
            return _clientToServerLatency.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.GetStats()
            );
        }

        /// <summary>
        /// Get latency statistics for server-to-client packets by opcode.
        /// </summary>
        public Dictionary<int, LatencyStats> GetServerToClientStats()
        {
            return _serverToClientLatency.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.GetStats()
            );
        }

        /// <summary>
        /// Get latency stats for a specific client-to-server opcode.
        /// </summary>
        public LatencyStats? GetClientToServerStats(int opcode)
        {
            return _clientToServerLatency.TryGetValue(opcode, out var samples) ? samples.GetStats() : null;
        }

        /// <summary>
        /// Get latency stats for a specific server-to-client opcode.
        /// </summary>
        public LatencyStats? GetServerToClientStats(int opcode)
        {
            return _serverToClientLatency.TryGetValue(opcode, out var samples) ? samples.GetStats() : null;
        }

        public TimeSpan Uptime => DateTime.UtcNow - _startTime;

        public int ClientToServerOpcodeCount => _clientToServerLatency.Count;
        public int ServerToClientOpcodeCount => _serverToClientLatency.Count;

        /// <summary>
        /// Reset all metrics.
        /// </summary>
        public void Reset()
        {
            _clientToServerLatency.Clear();
            _serverToClientLatency.Clear();
        }

        /// <summary>
        /// Get a formatted summary of the top N slowest opcodes.
        /// </summary>
        /// <param name="topN">Number of top opcodes to show</param>
        /// <param name="opcodeResolver">Optional function to resolve opcode int to human-readable name</param>
        public string GetSummary(int topN = 10, Func<int, string>? opcodeResolver = null)
        {
            opcodeResolver ??= (opcode => $"0x{opcode:X4}");

            var c2sStats = GetClientToServerStats()
                .OrderByDescending(x => x.Value.P99)
                .Take(topN)
                .ToList();

            var s2cStats = GetServerToClientStats()
                .OrderByDescending(x => x.Value.P99)
                .Take(topN)
                .ToList();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Proxy Latency Metrics (Uptime: {Uptime:hh\\:mm\\:ss})");
            sb.AppendLine();

            if (c2sStats.Count > 0)
            {
                sb.AppendLine("Client -> Server (top by p99):");
                sb.AppendLine($"  {"Opcode",-40} {"Count",8} {"Min",9} {"Avg",9} {"P50",9} {"P95",9} {"P99",9} {"Max",9}");
                sb.AppendLine($"  {new string('-', 40)} {new string('-', 8)} {new string('-', 9)} {new string('-', 9)} {new string('-', 9)} {new string('-', 9)} {new string('-', 9)} {new string('-', 9)}");
                foreach (var (opcode, stats) in c2sStats)
                {
                    string opcodeName = opcodeResolver(opcode);
                    if (opcodeName.Length > 40) opcodeName = opcodeName[..37] + "...";
                    sb.AppendLine($"  {opcodeName,-40} {stats.Count,8} {stats.Min,8:F3}ms {stats.Average,8:F3}ms {stats.P50,8:F3}ms {stats.P95,8:F3}ms {stats.P99,8:F3}ms {stats.Max,8:F3}ms");
                }
                sb.AppendLine();
            }

            if (s2cStats.Count > 0)
            {
                sb.AppendLine("Server -> Client (top by p99):");
                sb.AppendLine($"  {"Opcode",-40} {"Count",8} {"Min",9} {"Avg",9} {"P50",9} {"P95",9} {"P99",9} {"Max",9}");
                sb.AppendLine($"  {new string('-', 40)} {new string('-', 8)} {new string('-', 9)} {new string('-', 9)} {new string('-', 9)} {new string('-', 9)} {new string('-', 9)} {new string('-', 9)}");
                foreach (var (opcode, stats) in s2cStats)
                {
                    string opcodeName = opcodeResolver(opcode);
                    if (opcodeName.Length > 40) opcodeName = opcodeName[..37] + "...";
                    sb.AppendLine($"  {opcodeName,-40} {stats.Count,8} {stats.Min,8:F3}ms {stats.Average,8:F3}ms {stats.P50,8:F3}ms {stats.P95,8:F3}ms {stats.P99,8:F3}ms {stats.Max,8:F3}ms");
                }
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Thread-safe circular buffer for latency samples.
    /// </summary>
    public class LatencySamples
    {
        private readonly double[] _samples;
        private readonly object _lock = new();
        private int _count;
        private int _index;
        private double _sum;
        private double _min = double.MaxValue;
        private double _max = double.MinValue;

        public LatencySamples(int maxSamples)
        {
            _samples = new double[maxSamples];
        }

        public void Add(double milliseconds)
        {
            lock (_lock)
            {
                // Update running stats
                if (_count < _samples.Length)
                {
                    _sum += milliseconds;
                    _count++;
                }
                else
                {
                    // Remove old value from sum when overwriting
                    _sum -= _samples[_index];
                    _sum += milliseconds;
                }

                _samples[_index] = milliseconds;
                _index = (_index + 1) % _samples.Length;

                if (milliseconds < _min) _min = milliseconds;
                if (milliseconds > _max) _max = milliseconds;
            }
        }

        public LatencyStats GetStats()
        {
            lock (_lock)
            {
                if (_count == 0)
                {
                    return new LatencyStats();
                }

                // Copy samples for percentile calculation
                var samplesCopy = new double[_count];
                Array.Copy(_samples, samplesCopy, _count);
                Array.Sort(samplesCopy);

                return new LatencyStats
                {
                    Count = _count,
                    Min = _min,
                    Max = _max,
                    Average = _sum / _count,
                    P50 = GetPercentile(samplesCopy, 0.50),
                    P95 = GetPercentile(samplesCopy, 0.95),
                    P99 = GetPercentile(samplesCopy, 0.99)
                };
            }
        }

        private static double GetPercentile(double[] sortedSamples, double percentile)
        {
            if (sortedSamples.Length == 0) return 0;
            if (sortedSamples.Length == 1) return sortedSamples[0];

            double index = percentile * (sortedSamples.Length - 1);
            int lower = (int)Math.Floor(index);
            int upper = (int)Math.Ceiling(index);

            if (lower == upper) return sortedSamples[lower];

            // Linear interpolation
            double fraction = index - lower;
            return sortedSamples[lower] + (sortedSamples[upper] - sortedSamples[lower]) * fraction;
        }
    }

    /// <summary>
    /// Latency statistics for a single opcode.
    /// </summary>
    public struct LatencyStats
    {
        public int Count;
        public double Min;
        public double Max;
        public double Average;
        public double P50;
        public double P95;
        public double P99;

        public override string ToString()
        {
            return $"Count={Count}, Min={Min:F3}ms, Avg={Average:F3}ms, P50={P50:F3}ms, P95={P95:F3}ms, P99={P99:F3}ms, Max={Max:F3}ms";
        }
    }
}
