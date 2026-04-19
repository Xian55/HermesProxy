using System;
using BenchmarkDotNet.Attributes;
using Framework.Logging;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace HermesProxy.Benchmarks;

/// <summary>
/// Four equivalent "received opcode" calls benchmarked side-by-side:
///   1. Interpolated string through Serilog (old pattern, pre-migration)
///   2. Manual structured template through Serilog (current migration Phase B pattern)
///   3. Source-generated [LoggerMessage] through the MEL -> Serilog bridge (Phase C pattern)
/// Each is measured in both the enabled and disabled state. The key column is
/// <c>Allocated</c> on the <c>*_Disabled</c> rows — zero there is the whole point of
/// migrating a hot path.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class LoggingBenchmarks
{
    private Serilog.ILogger _sinkSerilog = null!;
    private Serilog.ILogger _disabledSinkSerilog = null!;
    private Microsoft.Extensions.Logging.ILogger _sinkMel = null!;
    private Microsoft.Extensions.Logging.ILogger _disabledSinkMel = null!;
    // Mirrors Log.CreateMelLogger's production behavior: a SwappableMelLogger-equivalent wrapper
    // that re-resolves through the current factory on every call. Adds one extra virtual dispatch
    // plus one dictionary lookup (the factory's name->logger cache).
    private Microsoft.Extensions.Logging.ILogger _sinkSwappable = null!;
    private Microsoft.Extensions.Logging.ILogger _disabledSinkSwappable = null!;

    private const string Opcode = "CMSG_BATTLE_PAY_GET_PURCHASE_LIST";
    private const int OpcodeId = 14019;
    private const string SourceFile = "WorldSocket    ";
    private const string NetDir = "C>P S | ";

    [GlobalSetup]
    public void Setup()
    {
        var enabledRoot = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Sink(new NullSink())
            .CreateLogger();
        _sinkSerilog = enabledRoot;
        var enabledFactory = new Serilog.Extensions.Logging.SerilogLoggerFactory(enabledRoot, dispose: false);
        _sinkMel = enabledFactory.CreateLogger("Packet");
        _sinkSwappable = new TestSwappableMelLogger(enabledFactory, "Packet");

        var disabledRoot = new LoggerConfiguration()
            .MinimumLevel.Error()
            .WriteTo.Sink(new NullSink())
            .CreateLogger();
        _disabledSinkSerilog = disabledRoot;
        var disabledFactory = new Serilog.Extensions.Logging.SerilogLoggerFactory(disabledRoot, dispose: false);
        _disabledSinkMel = disabledFactory.CreateLogger("Packet");
        _disabledSinkSwappable = new TestSwappableMelLogger(disabledFactory, "Packet");
    }

    // --- Interpolated (old pattern, always allocates) ---

    [Benchmark(Baseline = true, Description = "1a. Interpolated, disabled")]
    public void Interpolated_Disabled()
    {
        _disabledSinkSerilog.Debug($"Received opcode {Opcode} ({OpcodeId}).");
    }

    [Benchmark(Description = "1b. Interpolated, enabled")]
    public void Interpolated_Enabled()
    {
        _sinkSerilog.Debug($"Received opcode {Opcode} ({OpcodeId}).");
    }

    // --- Manual structured template (current Phase B pattern) ---

    [Benchmark(Description = "2a. Structured, disabled")]
    public void Structured_Disabled()
    {
        if (_disabledSinkSerilog.IsEnabled(LogEventLevel.Debug))
            _disabledSinkSerilog.Debug("Received opcode {Opcode} ({OpcodeId}).", Opcode, OpcodeId);
    }

    [Benchmark(Description = "2b. Structured, enabled")]
    public void Structured_Enabled()
    {
        if (_sinkSerilog.IsEnabled(LogEventLevel.Debug))
            _sinkSerilog.Debug("Received opcode {Opcode} ({OpcodeId}).", Opcode, OpcodeId);
    }

    // --- Source-generated [LoggerMessage] through MEL -> Serilog (Phase C pattern) ---

    [Benchmark(Description = "3a. SourceGen, disabled")]
    public void SourceGen_Disabled()
    {
        BenchmarkLogMessages.PacketReceivedStr(_disabledSinkMel, SourceFile, NetDir, Opcode, OpcodeId);
    }

    [Benchmark(Description = "3b. SourceGen, enabled")]
    public void SourceGen_Enabled()
    {
        BenchmarkLogMessages.PacketReceivedStr(_sinkMel, SourceFile, NetDir, Opcode, OpcodeId);
    }

    // --- Source-generated [LoggerMessage] via SwappableMelLogger (actual production path) ---

    [Benchmark(Description = "4a. SourceGen+Swappable, disabled")]
    public void SourceGenSwappable_Disabled()
    {
        BenchmarkLogMessages.PacketReceivedStr(_disabledSinkSwappable, SourceFile, NetDir, Opcode, OpcodeId);
    }

    [Benchmark(Description = "4b. SourceGen+Swappable, enabled")]
    public void SourceGenSwappable_Enabled()
    {
        BenchmarkLogMessages.PacketReceivedStr(_sinkSwappable, SourceFile, NetDir, Opcode, OpcodeId);
    }

    private sealed class NullSink : ILogEventSink
    {
        public void Emit(LogEvent logEvent) { }
    }

    /// <summary>
    /// Local mirror of <c>Framework.Logging.Log.SwappableMelLogger</c>. Caches the inner MEL
    /// logger per-instance and only re-resolves when the underlying factory changes reference.
    /// </summary>
    private sealed class TestSwappableMelLogger : Microsoft.Extensions.Logging.ILogger
    {
        private readonly Microsoft.Extensions.Logging.ILoggerFactory _factory;
        private readonly string _name;
        private Microsoft.Extensions.Logging.ILoggerFactory? _cachedFactory;
        private Microsoft.Extensions.Logging.ILogger? _cachedLogger;

        public TestSwappableMelLogger(Microsoft.Extensions.Logging.ILoggerFactory factory, string name)
        { _factory = factory; _name = name; }

        private Microsoft.Extensions.Logging.ILogger Current
        {
            get
            {
                if (!ReferenceEquals(_factory, _cachedFactory))
                {
                    _cachedFactory = _factory;
                    _cachedLogger = _factory.CreateLogger(_name);
                }
                return _cachedLogger!;
            }
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            => Current.BeginScope(state);
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
            => Current.IsEnabled(logLevel);
        public void Log<TState>(
            Microsoft.Extensions.Logging.LogLevel logLevel,
            Microsoft.Extensions.Logging.EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Current.Log(logLevel, eventId, state, exception, formatter);
    }
}

#pragma warning disable SYSLIB1015
internal static partial class BenchmarkLogMessages
{
    // Mirrors WorldSocketLogMessages.PacketReceived but uses a string for Opcode
    // so we can feed it constants in the benchmark. Shape and generated code are identical.
    [LoggerMessage(
        EventId = 9999,
        Level = LogLevel.Debug,
        Message = "Received opcode {Opcode} ({OpcodeId}).")]
    public static partial void PacketReceivedStr(
        Microsoft.Extensions.Logging.ILogger logger,
        string SourceFile,
        string NetDir,
        string Opcode,
        int OpcodeId);
}
