using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Framework.Metrics;

/// <summary>
/// Thread-safe per-opcode collector for handler latency and managed allocation, plus
/// process-wide GC deltas. Enabled by <c>--metrics</c>. Callers gate the timestamp and
/// <see cref="GC.GetAllocatedBytesForCurrentThread"/> reads behind the same flag, so the
/// disabled path pays nothing.
/// </summary>
public sealed class ProxyMetrics
{
    private const int MaxSamplesPerOpcode = 1000;

    private readonly ConcurrentDictionary<int, OpcodeSamples> _clientToServer = new();
    private readonly ConcurrentDictionary<int, OpcodeSamples> _serverToClient = new();

    private readonly DateTime _startTime = DateTime.UtcNow;

    private long _clientToServerPackets;
    private long _serverToClientPackets;

    // Read and replaced only by the periodic summary caller; see CaptureGcDelta. Null until
    // the first capture so constructing the (always-present) Server.Metrics instance makes no
    // GC API call when --metrics is off.
    private GcSnapshot? _lastGcSnapshot;
    private readonly long _constructedTimestamp = Stopwatch.GetTimestamp();
    private long _lastSummaryTimestamp = Stopwatch.GetTimestamp();
    private long _lastSummaryClientToServer;
    private long _lastSummaryServerToClient;

    /// <summary>
    /// Record one handled modern-client packet. The generic constraint keeps the enum
    /// unboxed; the previous <see cref="Enum"/>-typed overload boxed on every packet.
    /// </summary>
    public void RecordClientToServer<TOpcode>(TOpcode opcode, double milliseconds, long allocatedBytes)
        where TOpcode : unmanaged, Enum
        => RecordClientToServer(EnumToInt(opcode), milliseconds, allocatedBytes);

    /// <summary>Record one handled legacy-server packet.</summary>
    public void RecordServerToClient<TOpcode>(TOpcode opcode, double milliseconds, long allocatedBytes)
        where TOpcode : unmanaged, Enum
        => RecordServerToClient(EnumToInt(opcode), milliseconds, allocatedBytes);

    internal void RecordClientToServer(int opcode, double milliseconds, long allocatedBytes)
    {
        Interlocked.Increment(ref _clientToServerPackets);
        _clientToServer.GetOrAdd(opcode, static _ => new OpcodeSamples(MaxSamplesPerOpcode))
            .Add(milliseconds, allocatedBytes);
    }

    internal void RecordServerToClient(int opcode, double milliseconds, long allocatedBytes)
    {
        Interlocked.Increment(ref _serverToClientPackets);
        _serverToClient.GetOrAdd(opcode, static _ => new OpcodeSamples(MaxSamplesPerOpcode))
            .Add(milliseconds, allocatedBytes);
    }

    /// <summary>Latency-only overload kept for tests and callers that have no allocation figure.</summary>
    internal void RecordClientToServerLatency(int opcode, double milliseconds)
        => RecordClientToServer(opcode, milliseconds, 0);

    /// <summary>Latency-only overload kept for tests and callers that have no allocation figure.</summary>
    internal void RecordServerToClientLatency(int opcode, double milliseconds)
        => RecordServerToClient(opcode, milliseconds, 0);

    public Dictionary<int, OpcodeStats> GetClientToServerStats()
        => _clientToServer.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetStats());

    public Dictionary<int, OpcodeStats> GetServerToClientStats()
        => _serverToClient.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetStats());

    public OpcodeStats? GetClientToServerStats(int opcode)
        => _clientToServer.TryGetValue(opcode, out var samples) ? samples.GetStats() : null;

    public OpcodeStats? GetServerToClientStats(int opcode)
        => _serverToClient.TryGetValue(opcode, out var samples) ? samples.GetStats() : null;

    public TimeSpan Uptime => DateTime.UtcNow - _startTime;

    public int ClientToServerOpcodeCount => _clientToServer.Count;
    public int ServerToClientOpcodeCount => _serverToClient.Count;

    public long ClientToServerPacketCount => Interlocked.Read(ref _clientToServerPackets);
    public long ServerToClientPacketCount => Interlocked.Read(ref _serverToClientPackets);

    public void Reset()
    {
        _clientToServer.Clear();
        _serverToClient.Clear();
        Interlocked.Exchange(ref _clientToServerPackets, 0);
        Interlocked.Exchange(ref _serverToClientPackets, 0);
        _lastSummaryClientToServer = 0;
        _lastSummaryServerToClient = 0;
        _lastSummaryTimestamp = Stopwatch.GetTimestamp();
        _lastGcSnapshot = null;
    }

    /// <summary>
    /// GC and allocation change since the previous call, then re-arms. The first call reports
    /// everything since this instance was constructed (collection counts and total allocated
    /// bytes are cumulative from process start, so the baseline is simply zero). Intended for
    /// one periodic caller (the hosted-service summary loop); concurrent callers would each
    /// see a partial interval.
    /// </summary>
    public GcDelta CaptureGcDelta()
    {
        var now = GcSnapshot.Capture();
        var previous = _lastGcSnapshot ?? new GcSnapshot(_constructedTimestamp, 0, 0, 0, 0, 0, 0);
        var delta = GcDelta.Between(previous, now);
        _lastGcSnapshot = now;
        return delta;
    }

    /// <summary>
    /// Formatted summary: packet rates since the previous summary, the GC delta when
    /// supplied, then per direction the top-N opcodes by the largest latency seen since the
    /// previous summary (with allocation columns) and the top-N by total allocated bytes.
    /// Intended for one periodic caller: it closes the interval, so the next summary's
    /// interval columns start from zero.
    /// </summary>
    public string GetSummary(int topN = 10, Func<int, string>? opcodeResolver = null, GcDelta? gc = null)
    {
        opcodeResolver ??= static opcode => $"0x{opcode:X4}";

        long nowTimestamp = Stopwatch.GetTimestamp();
        double intervalSeconds = Stopwatch.GetElapsedTime(_lastSummaryTimestamp, nowTimestamp).TotalSeconds;
        long c2sTotal = ClientToServerPacketCount;
        long s2cTotal = ServerToClientPacketCount;
        double c2sRate = intervalSeconds > 0 ? (c2sTotal - _lastSummaryClientToServer) / intervalSeconds : 0;
        double s2cRate = intervalSeconds > 0 ? (s2cTotal - _lastSummaryServerToClient) / intervalSeconds : 0;
        _lastSummaryTimestamp = nowTimestamp;
        _lastSummaryClientToServer = c2sTotal;
        _lastSummaryServerToClient = s2cTotal;

        var c2sStats = TakeIntervalStats(_clientToServer);
        var s2cStats = TakeIntervalStats(_serverToClient);

        var sb = new StringBuilder();
        sb.AppendLine($"Proxy Metrics (Uptime: {Uptime:hh\\:mm\\:ss}) | C->S {c2sTotal} pkts ({c2sRate:F1}/s) | S->C {s2cTotal} pkts ({s2cRate:F1}/s)");
        if (gc is { } delta)
            sb.AppendLine(delta.ToSummaryLine());
        sb.AppendLine();

        AppendLatencyTable(sb, "Client -> Server", c2sStats, topN, opcodeResolver);
        AppendAllocationTable(sb, "Client -> Server", c2sStats, topN, opcodeResolver);
        AppendLatencyTable(sb, "Server -> Client", s2cStats, topN, opcodeResolver);
        AppendAllocationTable(sb, "Server -> Client", s2cStats, topN, opcodeResolver);

        return sb.ToString();
    }

    private static Dictionary<int, OpcodeStats> TakeIntervalStats(ConcurrentDictionary<int, OpcodeSamples> samples)
        => samples.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetStats(resetInterval: true));

    // Ranked by the interval max, not the windowed p99: lifetime Max keeps the login burst's
    // spike forever and a rare stall barely moves p99, so either ordering hid the one slow
    // packet in an otherwise quiet minute. Opcodes idle this interval are left out, which also
    // drops the one-off login opcodes that would otherwise sit at the top of every table.
    private static void AppendLatencyTable(StringBuilder sb, string direction, Dictionary<int, OpcodeStats> stats, int topN, Func<int, string> resolver)
    {
        var rows = stats.Where(x => x.Value.Latency.IntervalCount > 0)
                        .OrderByDescending(x => x.Value.Latency.IntervalMax)
                        .Take(topN)
                        .ToList();
        if (rows.Count == 0)
            return;

        sb.AppendLine($"{direction} (top {rows.Count} by max latency this interval; percentiles over the last {MaxSamplesPerOpcode} samples):");
        sb.AppendLine($"  {"Opcode",-40} {"Int",7} {"Window",7} {"Total",9} {"Min",9} {"Avg",9} {"P50",9} {"P95",9} {"P99",9} {"IntMax",9} {"Max",9} | {"AvgB",7} {"MaxB",8} {"TotalKB",9}");
        sb.AppendLine($"  {new string('-', 40)} {new string('-', 7)} {new string('-', 7)} {new string('-', 9)} {new string('-', 9)} {new string('-', 9)} {new string('-', 9)} {new string('-', 9)} {new string('-', 9)} {new string('-', 9)} {new string('-', 9)} | {new string('-', 7)} {new string('-', 8)} {new string('-', 9)}");
        foreach (var (opcode, s) in rows)
        {
            var l = s.Latency;
            var a = s.Allocation;
            sb.AppendLine($"  {Truncate(resolver(opcode), 40),-40} {l.IntervalCount,7} {l.Count,7} {l.TotalCount,9} {l.Min,8:F3}ms {l.Average,8:F3}ms {l.P50,8:F3}ms {l.P95,8:F3}ms {l.P99,8:F3}ms {l.IntervalMax,8:F3}ms {l.Max,8:F3}ms | {a.Average,7:F0} {a.Max,8:F0} {a.TotalSum / 1024.0,9:F1}");
        }
        sb.AppendLine();
    }

    private static void AppendAllocationTable(StringBuilder sb, string direction, Dictionary<int, OpcodeStats> stats, int topN, Func<int, string> resolver)
    {
        double directionTotal = stats.Sum(x => x.Value.Allocation.TotalSum);
        var rows = stats.Where(x => x.Value.Allocation.TotalSum > 0)
                        .OrderByDescending(x => x.Value.Allocation.TotalSum)
                        .Take(topN)
                        .ToList();
        if (rows.Count == 0)
            return;

        sb.AppendLine($"{direction} (top {rows.Count} by allocated bytes, {directionTotal / 1024.0 / 1024.0:F2} MB total):");
        sb.AppendLine($"  {"Opcode",-40} {"Total",9} {"AvgB",8} {"P99B",8} {"MaxB",8} {"TotalKB",10} {"Share",7}");
        sb.AppendLine($"  {new string('-', 40)} {new string('-', 9)} {new string('-', 8)} {new string('-', 8)} {new string('-', 8)} {new string('-', 10)} {new string('-', 7)}");
        foreach (var (opcode, s) in rows)
        {
            var a = s.Allocation;
            double share = directionTotal > 0 ? a.TotalSum / directionTotal * 100.0 : 0;
            sb.AppendLine($"  {Truncate(resolver(opcode), 40),-40} {a.TotalCount,9} {a.Average,8:F0} {a.P99,8:F0} {a.Max,8:F0} {a.TotalSum / 1024.0,10:F1} {share,6:F1}%");
        }
        sb.AppendLine();
    }

    private static string Truncate(string s, int max) => s.Length > max ? s[..(max - 3)] + "..." : s;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int EnumToInt<T>(T value) where T : unmanaged, Enum
    {
        if (Unsafe.SizeOf<T>() == 1)
            return Unsafe.As<T, byte>(ref value);
        if (Unsafe.SizeOf<T>() == 2)
            return Unsafe.As<T, ushort>(ref value);
        if (Unsafe.SizeOf<T>() == 4)
            return Unsafe.As<T, int>(ref value);
        return (int)Unsafe.As<T, long>(ref value);
    }
}

/// <summary>
/// Latency and allocation windows for one opcode, guarded by one lock so a sample lands in
/// both or neither.
/// </summary>
public sealed class OpcodeSamples
{
    private readonly SampleWindow _latency;
    private readonly SampleWindow _allocation;
    private readonly Lock _lock = new();

    public OpcodeSamples(int maxSamples)
    {
        _latency = new SampleWindow(maxSamples);
        _allocation = new SampleWindow(maxSamples);
    }

    public void Add(double milliseconds, long allocatedBytes)
    {
        lock (_lock)
        {
            _latency.Add(milliseconds);
            _allocation.Add(allocatedBytes);
        }
    }

    /// <param name="resetInterval">
    /// Close the interval after reading it, so both windows' interval columns restart at zero
    /// under the same lock as the read and no sample falls between the two.
    /// </param>
    public OpcodeStats GetStats(bool resetInterval = false)
    {
        lock (_lock)
        {
            var stats = new OpcodeStats(_latency.GetStats(), _allocation.GetStats());
            if (resetInterval)
            {
                _latency.ResetInterval();
                _allocation.ResetInterval();
            }
            return stats;
        }
    }
}

/// <summary>
/// Circular sample buffer with running min/max and lifetime totals. Not synchronised;
/// <see cref="OpcodeSamples"/> owns the lock.
/// </summary>
public sealed class SampleWindow
{
    private readonly double[] _samples;
    private int _count;
    private int _index;
    private double _sum;
    private double _min = double.MaxValue;
    private double _max = double.MinValue;
    private long _totalCount;
    private double _totalSum;
    private long _intervalCount;
    private double _intervalMax;

    public SampleWindow(int maxSamples)
    {
        _samples = new double[maxSamples];
    }

    public void Add(double value)
    {
        if (_count < _samples.Length)
        {
            _sum += value;
            _count++;
        }
        else
        {
            _sum -= _samples[_index];
            _sum += value;
        }

        _samples[_index] = value;
        _index = (_index + 1) % _samples.Length;

        if (value < _min) _min = value;
        if (value > _max) _max = value;

        _totalCount++;
        _totalSum += value;

        if (_intervalCount == 0 || value > _intervalMax) _intervalMax = value;
        _intervalCount++;
    }

    /// <summary>Starts a new interval; the lifetime and windowed figures are untouched.</summary>
    public void ResetInterval()
    {
        _intervalCount = 0;
        _intervalMax = 0;
    }

    public SampleStats GetStats()
    {
        if (_count == 0)
            return default;

        var sorted = new double[_count];
        Array.Copy(_samples, sorted, _count);
        Array.Sort(sorted);

        return new SampleStats
        {
            Count = _count,
            TotalCount = _totalCount,
            TotalSum = _totalSum,
            Min = _min,
            Max = _max,
            Average = _sum / _count,
            P50 = GetPercentile(sorted, 0.50),
            P95 = GetPercentile(sorted, 0.95),
            P99 = GetPercentile(sorted, 0.99),
            IntervalCount = _intervalCount,
            IntervalMax = _intervalMax,
        };
    }

    private static double GetPercentile(double[] sortedSamples, double percentile)
    {
        if (sortedSamples.Length == 0) return 0;
        if (sortedSamples.Length == 1) return sortedSamples[0];

        double index = percentile * (sortedSamples.Length - 1);
        int lower = (int)Math.Floor(index);
        int upper = (int)Math.Ceiling(index);

        if (lower == upper) return sortedSamples[lower];

        double fraction = index - lower;
        return sortedSamples[lower] + (sortedSamples[upper] - sortedSamples[lower]) * fraction;
    }
}

/// <summary>
/// Windowed percentiles over the most recent samples plus lifetime count and sum.
/// </summary>
public struct SampleStats
{
    /// <summary>Samples currently in the window (capped).</summary>
    public int Count;
    /// <summary>Samples ever recorded.</summary>
    public long TotalCount;
    /// <summary>Sum of every sample ever recorded.</summary>
    public double TotalSum;
    public double Min;
    public double Max;
    /// <summary>Mean over the window.</summary>
    public double Average;
    public double P50;
    public double P95;
    public double P99;
    /// <summary>Samples recorded since the interval was last reset.</summary>
    public long IntervalCount;
    /// <summary>Largest sample since the interval was last reset; 0 when <see cref="IntervalCount"/> is 0.</summary>
    public double IntervalMax;

    public override string ToString()
        => $"Count={Count}, Total={TotalCount}, Min={Min:F3}, Avg={Average:F3}, P50={P50:F3}, P95={P95:F3}, P99={P99:F3}, Max={Max:F3}, IntervalCount={IntervalCount}, IntervalMax={IntervalMax:F3}";
}

public readonly record struct OpcodeStats(SampleStats Latency, SampleStats Allocation);

/// <summary>Point-in-time GC counters.</summary>
public readonly record struct GcSnapshot(
    long Timestamp,
    int Gen0,
    int Gen1,
    int Gen2,
    long TotalAllocatedBytes,
    double PauseTimePercentage,
    long HeapSizeBytes)
{
    public static GcSnapshot Capture()
    {
        var info = GC.GetGCMemoryInfo();
        return new GcSnapshot(
            Stopwatch.GetTimestamp(),
            GC.CollectionCount(0),
            GC.CollectionCount(1),
            GC.CollectionCount(2),
            GC.GetTotalAllocatedBytes(precise: true),
            info.PauseTimePercentage,
            info.HeapSizeBytes);
    }
}

/// <summary>Change between two <see cref="GcSnapshot"/>s.</summary>
public readonly record struct GcDelta(
    TimeSpan Elapsed,
    int Gen0,
    int Gen1,
    int Gen2,
    long AllocatedBytes,
    double PauseTimePercentage,
    long HeapSizeBytes)
{
    public static GcDelta Between(GcSnapshot from, GcSnapshot to)
        => new(
            Stopwatch.GetElapsedTime(from.Timestamp, to.Timestamp),
            to.Gen0 - from.Gen0,
            to.Gen1 - from.Gen1,
            to.Gen2 - from.Gen2,
            to.TotalAllocatedBytes - from.TotalAllocatedBytes,
            to.PauseTimePercentage,
            to.HeapSizeBytes);

    public double AllocatedMegabytesPerSecond
        => Elapsed.TotalSeconds > 0 ? AllocatedBytes / 1024.0 / 1024.0 / Elapsed.TotalSeconds : 0;

    public string ToSummaryLine()
        => $"GC over {Elapsed.TotalSeconds:F0}s: gen0 +{Gen0} gen1 +{Gen1} gen2 +{Gen2} | allocated {AllocatedBytes / 1024.0 / 1024.0:F1} MB ({AllocatedMegabytesPerSecond:F2} MB/s) | heap {HeapSizeBytes / 1024.0 / 1024.0:F1} MB | pause {PauseTimePercentage:F2}%";
}
