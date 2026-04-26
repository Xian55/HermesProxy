using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Framework.Logging;

public enum LogType
{
    Server,
    Network,
    Debug,
    Error,
    Warn,
    Storage,
    SpanMiss,
    SpanStats,
    Trace,      // Routes to (Server, Verbose) — for high-volume code-path tracing.
                // Enable with --set Log.Server.MinimumLevel=Verbose (or globally with
                // Log.MinimumLevel=Verbose). Off by default; cheap when filtered.
}

public enum LogNetDir // Network direction
{
    C2P, // C>P S
    P2S, // C P>S
    S2P, // C P<S
    P2C, // C<P S
}

public sealed record LogBootstrapOptions(
    LogEventLevel MinimumLevel,
    LogEventLevel ServerLevel,
    LogEventLevel NetworkLevel,
    LogEventLevel StorageLevel,
    LogEventLevel PacketLevel,
    LogEventLevel ConsoleLevel,
    bool ToFile,
    string Directory);

public static class Log
{
    public const string CategoryServer = "Server";
    public const string CategoryNetwork = "Network";
    public const string CategoryStorage = "Storage";
    public const string CategoryPacket = "Packet";

    // Console template uses the ANSI-pre-colored category letter so each category keeps its own color
    // (Server=Blue, Network=Green, Storage=Cyan, Packet=Magenta). The level letter is colored by the
    // custom theme below. The message text stays the terminal's default color.
    private const string ConsoleOutputTemplate =
        "{Timestamp:HH:mm:ss} | {Level:u1} | {CategoryAnsi:l} | {SourceFile,-15} | {NetDir:l}{Message:lj}{NewLine}{Exception}";

    // File template uses the plain category letter so rolling log files never contain ANSI escape codes.
    private const string FileOutputTemplate =
        "{Timestamp:HH:mm:ss} | {Level:u1} | {Category} | {SourceFile,-15} | {NetDir:l}{Message:lj}{NewLine}{Exception}";

    // Custom theme:
    //   - Level letter colored per severity (V/D=gray, W=yellow, E/F=red, I=default)
    //   - Literal text in messages stays default color
    //   - Property values (substituted into the message by {Message:lj}) are colored by their
    //     .NET type so e.g. "opcode CMSG_FOO (14053)" renders the string and number distinctly.
    private static readonly AnsiConsoleTheme ConsoleTheme = new(new Dictionary<ConsoleThemeStyle, string>
    {
        [ConsoleThemeStyle.LevelVerbose]     = "\x1b[90m",          // bright black / gray
        [ConsoleThemeStyle.LevelDebug]       = "\x1b[90m",          // bright black / gray
        [ConsoleThemeStyle.LevelInformation] = string.Empty,        // default terminal color
        [ConsoleThemeStyle.LevelWarning]     = "\x1b[33m",          // yellow
        [ConsoleThemeStyle.LevelError]       = "\x1b[31m",          // red
        [ConsoleThemeStyle.LevelFatal]       = "\x1b[1;31m",        // bold red

        // Property value coloring inside {Message:lj}
        [ConsoleThemeStyle.String]           = "\x1b[96m",          // bright cyan  (e.g. "CMSG_BATTLE_PAY_GET_PURCHASE_LIST")
        [ConsoleThemeStyle.Number]           = "\x1b[93m",          // bright yellow (e.g. 14019)
        [ConsoleThemeStyle.Boolean]          = "\x1b[35m",          // magenta       (true / false)
        [ConsoleThemeStyle.Null]             = "\x1b[90m",          // gray          (null)
        [ConsoleThemeStyle.Scalar]           = "\x1b[96m",          // bright cyan   (enum values like CMSG_PING)
        [ConsoleThemeStyle.Name]             = "\x1b[38;5;117m",    // light blue    (@-prefixed property name, rarely used here)
    });

    private static readonly LoggingLevelSwitch _globalSwitch = new(LogEventLevel.Information);
    private static readonly LoggingLevelSwitch _serverSwitch = new(LogEventLevel.Information);
    private static readonly LoggingLevelSwitch _networkSwitch = new(LogEventLevel.Information);
    private static readonly LoggingLevelSwitch _storageSwitch = new(LogEventLevel.Information);
    private static readonly LoggingLevelSwitch _packetSwitch = new(LogEventLevel.Warning);

    // Additional floor applied ONLY to the console sink. The file sink sees everything that passes
    // the per-category switches, which is typically more verbose. This lets users keep the console
    // tidy while the file captures enough detail to be useful in a bug report.
    private static readonly LoggingLevelSwitch _consoleSwitch = new(LogEventLevel.Information);

    private static Logger _root = null!;
    // volatile: reassigned in BuildPipeline; SwappableMelLogger reads on every call.
    private static volatile Serilog.Extensions.Logging.SerilogLoggerFactory _melFactory = null!;
    private static readonly Dictionary<string, ILogger> _callerToCategory =
        new(StringComparer.OrdinalIgnoreCase);

    public static ILogger Server { get; private set; } = null!;
    public static ILogger Network { get; private set; } = null!;
    public static ILogger Storage { get; private set; } = null!;
    public static ILogger Packet { get; private set; } = null!;

    static Log()
    {
        BuildPipeline(toFile: false, directory: "Logs");
    }

    public static bool IsLogging => _root is not null;

    /// <summary>Back-compat. When set to true, bumps the global minimum level to Debug.</summary>
    public static bool DebugLogEnabled
    {
        get => _globalSwitch.MinimumLevel <= LogEventLevel.Debug;
        set
        {
            if (value && _globalSwitch.MinimumLevel > LogEventLevel.Debug)
                _globalSwitch.MinimumLevel = LogEventLevel.Debug;
        }
    }

    /// <summary>Back-compat. When set to true, bumps the Packet category to Verbose.</summary>
    public static bool SpanStatsEnabled
    {
        get => _packetSwitch.MinimumLevel <= LogEventLevel.Verbose;
        set
        {
            if (value && _packetSwitch.MinimumLevel > LogEventLevel.Verbose)
                _packetSwitch.MinimumLevel = LogEventLevel.Verbose;
        }
    }

    /// <summary>No-op retained for API compatibility. The pipeline is built eagerly in the static ctor.</summary>
    public static void Start() { }

    public static void Configure(LogBootstrapOptions options)
    {
        _globalSwitch.MinimumLevel = options.MinimumLevel;
        _serverSwitch.MinimumLevel = options.ServerLevel;
        _networkSwitch.MinimumLevel = options.NetworkLevel;
        _storageSwitch.MinimumLevel = options.StorageLevel;
        _packetSwitch.MinimumLevel = options.PacketLevel;
        _consoleSwitch.MinimumLevel = options.ConsoleLevel;

        BuildPipeline(options.ToFile, options.Directory);
    }

    public static void Shutdown()
    {
        _root?.Dispose();
    }

    private static void BuildPipeline(bool toFile, string directory)
    {
        var config = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(_globalSwitch)
            .MinimumLevel.Override(CategoryServer, _serverSwitch)
            .MinimumLevel.Override(CategoryNetwork, _networkSwitch)
            .MinimumLevel.Override(CategoryStorage, _storageSwitch)
            .MinimumLevel.Override(CategoryPacket, _packetSwitch)
            .Enrich.With<CategoryLetterEnricher>()
            .Enrich.With<NetDirEnricher>()
            .Enrich.With<SourceFileEnricher>()
            .WriteTo.Async(a =>
            {
                // Console sub-logger has its own level switch so users can keep console tidy
                // (e.g. Information) while per-category switches allow more verbose events that
                // flow to the file sink only.
                a.Logger(sub => sub
                    .MinimumLevel.ControlledBy(_consoleSwitch)
                    .WriteTo.Console(
                        outputTemplate: ConsoleOutputTemplate,
                        theme: ConsoleTheme));

                if (toFile)
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(directory);
                    }
                    catch
                    {
                        // If the directory can't be created we still want console logging to work.
                    }
                    // File sink has no additional filter — it captures whatever passes the
                    // per-category switches, which is typically the more verbose view.
                    a.File(
                        path: Path.Combine(directory, "hermes-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        outputTemplate: FileOutputTemplate,
                        shared: false);
                }
            });

        var newRoot = config.CreateLogger();
        var oldRoot = _root;
        var oldFactory = _melFactory;
        _root = newRoot;
        _melFactory = new Serilog.Extensions.Logging.SerilogLoggerFactory(newRoot, dispose: false);

        Server = newRoot.ForContext(Serilog.Core.Constants.SourceContextPropertyName, CategoryServer);
        Network = newRoot.ForContext(Serilog.Core.Constants.SourceContextPropertyName, CategoryNetwork);
        Storage = newRoot.ForContext(Serilog.Core.Constants.SourceContextPropertyName, CategoryStorage);
        Packet = newRoot.ForContext(Serilog.Core.Constants.SourceContextPropertyName, CategoryPacket);

        oldFactory?.Dispose();
        oldRoot?.Dispose();
    }

    /// <summary>
    /// Create a <see cref="Microsoft.Extensions.Logging.ILogger"/> routed to this Serilog pipeline.
    /// Use this as the first parameter of source-generated <c>[LoggerMessage]</c> methods.
    /// The <paramref name="categoryName"/> should be one of <c>"Server"</c>, <c>"Network"</c>,
    /// <c>"Storage"</c>, <c>"Packet"</c> so that the per-category <c>MinimumLevel.Override</c> applies.
    /// <para>
    /// The returned logger is a thin wrapper that re-resolves through the current MEL factory on
    /// every call. This makes static-field captures of the logger safe across
    /// <see cref="Configure"/> rebuilds: classes that initialize BEFORE <c>Log.Configure</c> runs
    /// (e.g. <see cref="HermesProxy.Server"/>) still route their writes through the live pipeline
    /// after it's rebuilt. Overhead per call is one dictionary lookup in the SerilogLoggerFactory's
    /// internal name-&gt;logger cache.
    /// </para>
    /// </summary>
    public static Microsoft.Extensions.Logging.ILogger CreateMelLogger(string categoryName)
        => new SwappableMelLogger(categoryName);

    private sealed class SwappableMelLogger : Microsoft.Extensions.Logging.ILogger
    {
        private readonly string _name;
        // Per-instance cache: resolve the MEL logger once per factory. SerilogLoggerProvider
        // does NOT cache CreateLogger results internally, so without this layer every log call
        // would allocate a fresh SerilogLogger (~150 B / call). We compare factory identities
        // and only re-resolve after Log.Configure swaps _melFactory.
        private Microsoft.Extensions.Logging.ILoggerFactory? _cachedFactory;
        private Microsoft.Extensions.Logging.ILogger? _cachedLogger;

        public SwappableMelLogger(string name) => _name = name;

        private Microsoft.Extensions.Logging.ILogger Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var factory = _melFactory;
                var cachedLogger = _cachedLogger;
                // Fast path: both cache slots agree AND the logger is actually populated.
                // Checking _cachedLogger != null closes a torn-cache race: without it, a
                // concurrent reader could observe the two assignments in the slow path out
                // of order (_cachedFactory = factory visible, _cachedLogger not yet) and
                // return null — a NullReferenceException on the caller's IsEnabled/Log.
                if (cachedLogger != null && ReferenceEquals(factory, _cachedFactory))
                    return cachedLogger;

                return ResolveSlow(factory);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private Microsoft.Extensions.Logging.ILogger ResolveSlow(Microsoft.Extensions.Logging.ILoggerFactory? factory)
        {
            if (factory is null)
                throw new InvalidOperationException($"Logger '{_name}' accessed before Log.Configure ran.");

            // Two concurrent resolvers each creating a logger is harmless — the loggers are
            // semantically equivalent and the final writes below publish whichever won. The
            // fast path stays correct because it re-reads both slots atomically per call.
            var logger = factory.CreateLogger(_name);
            _cachedLogger = logger;
            _cachedFactory = factory;
            return logger;
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

    /// <summary>
    /// Associate a caller file name (from <c>[CallerFilePath]</c>) with a category logger so that
    /// legacy <see cref="LogType.Warn"/> and <see cref="LogType.Error"/> calls are routed appropriately.
    /// Unregistered callers default to <see cref="Server"/>.
    /// </summary>
    public static void RegisterCallerMapping(string callerFileName, ILogger category)
    {
        _callerToCategory[callerFileName] = category;
    }

    public static void Print(LogType type, object text,
        [CallerMemberName] string method = "",
        [CallerFilePath] string path = "")
    {
        var (logger, level) = Route(type, path);
        if (!logger.IsEnabled(level))
            return;

        var sourceFile = FormatCaller(path);
        // Pass the already-formatted text AS the message template (with {/} escaped). This makes
        // Serilog's themed renderer treat it as literal Text, not as a String-typed property
        // value — so the ConsoleThemeStyle.String color does not bleed over the whole message.
        // Structured hot-path calls that actually have placeholders still get per-property coloring.
        logger
            .ForContext("SourceFile", sourceFile)
            .Write(level, EscapeAsLiteralTemplate(text));
    }

    public static void PrintNet(LogType type, LogNetDir netDirection, object text,
        [CallerMemberName] string method = "",
        [CallerFilePath] string path = "")
    {
        var (logger, level) = Route(type, path);
        if (!logger.IsEnabled(level))
            return;

        var sourceFile = FormatCaller(path);
        var dir = FormatDir(netDirection);
        logger
            .ForContext("SourceFile", sourceFile)
            .ForContext("NetDir", dir)
            .Write(level, EscapeAsLiteralTemplate(text));
    }

    // Serilog parses message templates at call time. Any unescaped '{' or '}' in the adapter text
    // would be interpreted as placeholders and produce warnings. Double them so the template is
    // purely literal. For typical log text this allocates only when '{' / '}' actually appear.
    private static string EscapeAsLiteralTemplate(object text)
    {
        var s = text as string ?? text?.ToString() ?? string.Empty;
        if (s.IndexOfAny(['{', '}']) < 0)
            return s;
        return s.Replace("{", "{{").Replace("}", "}}");
    }

    public static void outException(Exception err,
        [CallerMemberName] string method = "",
        [CallerFilePath] string path = "")
    {
        Print(LogType.Error, err.ToString(), method, path);
    }

    private static (ILogger logger, LogEventLevel level) Route(LogType type, string path)
    {
        return type switch
        {
            LogType.Server => (Server, LogEventLevel.Information),
            LogType.Network => (Network, LogEventLevel.Information),
            LogType.Storage => (Storage, LogEventLevel.Information),
            LogType.Debug => (Server, LogEventLevel.Debug),
            LogType.Warn => (ResolveCategoryFromPath(path), LogEventLevel.Warning),
            LogType.Error => (ResolveCategoryFromPath(path), LogEventLevel.Error),
            LogType.SpanMiss => (Packet, LogEventLevel.Warning),
            LogType.SpanStats => (Packet, LogEventLevel.Verbose),
            LogType.Trace => (Server, LogEventLevel.Verbose),
            _ => (Server, LogEventLevel.Information)
        };
    }

    private static ILogger ResolveCategoryFromPath(string path)
    {
        var file = Path.GetFileNameWithoutExtension(path);
        return _callerToCategory.TryGetValue(file, out var logger) ? logger : Server;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string FormatCaller(string path)
        => Path.GetFileNameWithoutExtension(path).PadRight(15, ' ');

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string FormatDir(LogNetDir dir) => dir switch
    {
        LogNetDir.C2P => "C>P S | ",
        LogNetDir.P2S => "C P>S | ",
        LogNetDir.S2P => "C P<S | ",
        LogNetDir.P2C => "C<P S | ",
        _ => "?   ? | "
    };

}

internal sealed class CategoryLetterEnricher : ILogEventEnricher
{
    private const string AnsiReset = "\x1b[0m";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        string letter = "?";
        string ansi = "?";
        if (logEvent.Properties.TryGetValue(Serilog.Core.Constants.SourceContextPropertyName, out var value) &&
            value is ScalarValue { Value: string name })
        {
            (letter, ansi) = name switch
            {
                Log.CategoryServer  => ("S", "\x1b[34mS" + AnsiReset),  // blue
                Log.CategoryNetwork => ("N", "\x1b[32mN" + AnsiReset),  // green
                Log.CategoryStorage => ("T", "\x1b[36mT" + AnsiReset),  // cyan
                Log.CategoryPacket  => ("P", "\x1b[35mP" + AnsiReset),  // magenta
                _ => ("?", "?")
            };
        }
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Category", letter));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CategoryAnsi", ansi));
    }
}

internal sealed class NetDirEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // Ensure NetDir is always present so the output template never emits {NetDir}.
        if (!logEvent.Properties.ContainsKey("NetDir"))
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("NetDir", string.Empty));
    }
}

internal sealed class SourceFileEnricher : ILogEventEnricher
{
    private const int Width = 15;
    private static readonly string _emptyPadded = new(' ', Width);

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Properties.ContainsKey("SourceFile"))
            return;

        if (logEvent.Properties.TryGetValue(Serilog.Core.Constants.SourceContextPropertyName, out var value) &&
            value is ScalarValue { Value: string name })
        {
            var shortName = name.Length > 0 && name.LastIndexOf('.') is int dot && dot >= 0
                ? name[(dot + 1)..]
                : name;
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("SourceFile", shortName.PadRight(Width)));
        }
        else
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SourceFile", _emptyPadded));
        }
    }
}
