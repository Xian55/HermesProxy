using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BNetServer;
using BNetServer.Networking;
using Framework.Logging;
using Framework.Networking;
using HermesProxy.Configuration.Options;
using HermesProxy.World.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HermesProxy;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        // Route crashes to the log file before the process dies. The Serilog
        // async sink buffers entries; without explicit flush, a worker-thread
        // exception (packet handler, network thread, etc.) takes the process
        // down faster than pending entries can reach disk. Bug reporters see
        // an empty log and a terminal window that closed before they could
        // read the stack trace.
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        int exitCode = 1;
        try
        {
            exitCode = await RunHostAsync(args);
        }
        catch (OptionsValidationException ex)
        {
            Console.Error.WriteLine("Configuration failed validation:");
            foreach (var failure in ex.Failures)
                Console.Error.WriteLine($"  - {failure}");
        }
        catch (FileNotFoundException ex)
        {
            Console.Error.WriteLine($"Configuration file not found: {ex.FileName ?? ex.Message}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal startup error: {ex}");
        }

        if (OsSpecific.AreWeInOurOwnConsole())
        {
            // If we would exit immediately the console would close and the user cannot read the error
            // The delay is there if for some reason STDIN is already closed
            Thread.Sleep(TimeSpan.FromSeconds(3));

            Console.WriteLine("Press enter to close");
            Console.ReadLine();
        }

        return exitCode;
    }

    private static async Task<int> RunHostAsync(string[] rawArgs)
    {
        var args = PreprocessArgs(rawArgs, out string? configOverridePath);

        var builder = Host.CreateApplicationBuilder(args);
        builder.Configuration.Sources.Clear();
        builder.Configuration
            .AddJsonFile(configOverridePath ?? "appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables(prefix: "HERMES_")
            .AddCommandLine(args);

        builder.Services.AddOptions<ClientOptions>()
            .Bind(builder.Configuration.GetSection(nameof(ClientOptions)))
            .ValidateOnStart();
        builder.Services.AddOptions<LegacyServerOptions>()
            .Bind(builder.Configuration.GetSection(nameof(LegacyServerOptions)))
            .ValidateOnStart();
        builder.Services.AddOptions<ProxyNetworkOptions>()
            .Bind(builder.Configuration.GetSection(nameof(ProxyNetworkOptions)))
            .ValidateOnStart();
        builder.Services.AddOptions<LoggingOptions>()
            .Bind(builder.Configuration.GetSection(nameof(LoggingOptions)))
            .ValidateOnStart();
        builder.Services.AddOptions<DiagnosticsOptions>()
            .Bind(builder.Configuration.GetSection(nameof(DiagnosticsOptions)))
            .ValidateOnStart();

        builder.Services.AddSingleton<IPostConfigureOptions<ClientOptions>, ClientSeedParser>();
        builder.Services.AddSingleton<IPostConfigureOptions<LegacyServerOptions>, LegacyServerBuildResolver>();
        builder.Services.AddSingleton<IPostConfigureOptions<LoggingOptions>, LoggingLegacyFlagsTranslator>();
        builder.Services.AddSingleton<IValidateOptions<ClientOptions>, ClientOptionsValidator>();
        builder.Services.AddSingleton<IValidateOptions<LegacyServerOptions>, LegacyServerOptionsValidator>();
        builder.Services.AddSingleton<IValidateOptions<ProxyNetworkOptions>, ProxyNetworkOptionsValidator>();

        builder.Services.AddSingleton<LoginServiceManager>();
        builder.Services.AddSingleton<SocketManager<BnetTcpSession>>();
        builder.Services.AddSingleton<SocketManager<BnetRestApiSession>>();
        builder.Services.AddSingleton<SocketManager<RealmSocket>>();
        builder.Services.AddSingleton<SocketManager<WorldSocket>, WorldSocketManager>();

        builder.Services.AddHostedService<ProxyHostedService>();

        using var host = builder.Build();
        await host.RunAsync();
        return 0;
    }

    // Back-compat translation table: maps the flat pre-modernization config keys (as used by
    // `--set KEY=VALUE` against the old HermesProxy.config) to their current section-qualified
    // equivalents under the five Options DTOs. Both sides use nameof where the legacy key
    // happens to match the current property name; the remaining LHS entries are frozen
    // historical strings (Settings.cs is deleted so they have no live symbol).
    private static readonly Dictionary<string, string> LegacySetKeyMap = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(ClientOptions.ClientBuild)]        = $"{nameof(ClientOptions)}:{nameof(ClientOptions.ClientBuild)}",
        ["ClientSeed"]                             = $"{nameof(ClientOptions)}:{nameof(ClientOptions.SeedHex)}",
        [nameof(ClientOptions.ReportedOS)]         = $"{nameof(ClientOptions)}:{nameof(ClientOptions.ReportedOS)}",
        [nameof(ClientOptions.ReportedPlatform)]   = $"{nameof(ClientOptions)}:{nameof(ClientOptions.ReportedPlatform)}",

        ["ServerAddress"]                          = $"{nameof(LegacyServerOptions)}:{nameof(LegacyServerOptions.Address)}",
        ["ServerPort"]                             = $"{nameof(LegacyServerOptions)}:{nameof(LegacyServerOptions.Port)}",
        ["ServerBuild"]                            = $"{nameof(LegacyServerOptions)}:{nameof(LegacyServerOptions.Build)}",

        [nameof(ProxyNetworkOptions.ExternalAddress)] = $"{nameof(ProxyNetworkOptions)}:{nameof(ProxyNetworkOptions.ExternalAddress)}",
        [nameof(ProxyNetworkOptions.RestPort)]     = $"{nameof(ProxyNetworkOptions)}:{nameof(ProxyNetworkOptions.RestPort)}",
        [nameof(ProxyNetworkOptions.BNetPort)]     = $"{nameof(ProxyNetworkOptions)}:{nameof(ProxyNetworkOptions.BNetPort)}",
        [nameof(ProxyNetworkOptions.RealmPort)]    = $"{nameof(ProxyNetworkOptions)}:{nameof(ProxyNetworkOptions.RealmPort)}",
        [nameof(ProxyNetworkOptions.InstancePort)] = $"{nameof(ProxyNetworkOptions)}:{nameof(ProxyNetworkOptions.InstancePort)}",

        [nameof(DiagnosticsOptions.PacketsLog)]    = $"{nameof(DiagnosticsOptions)}:{nameof(DiagnosticsOptions.PacketsLog)}",

        [nameof(LoggingOptions.DebugOutput)]       = $"{nameof(LoggingOptions)}:{nameof(LoggingOptions.DebugOutput)}",
        [nameof(LoggingOptions.SpanStatsLog)]      = $"{nameof(LoggingOptions)}:{nameof(LoggingOptions.SpanStatsLog)}",
        ["Log.MinimumLevel"]                       = $"{nameof(LoggingOptions)}:{nameof(LoggingOptions.MinimumLevel)}",
        ["Log.Server.MinimumLevel"]                = $"{nameof(LoggingOptions)}:{nameof(LoggingOptions.ServerLevel)}",
        ["Log.Network.MinimumLevel"]               = $"{nameof(LoggingOptions)}:{nameof(LoggingOptions.NetworkLevel)}",
        ["Log.Storage.MinimumLevel"]               = $"{nameof(LoggingOptions)}:{nameof(LoggingOptions.StorageLevel)}",
        ["Log.Packet.MinimumLevel"]                = $"{nameof(LoggingOptions)}:{nameof(LoggingOptions.PacketLevel)}",
        ["Log.Console.MinimumLevel"]               = $"{nameof(LoggingOptions)}:{nameof(LoggingOptions.ConsoleLevel)}",
        ["Log.ToFile"]                             = $"{nameof(LoggingOptions)}:{nameof(LoggingOptions.ToFile)}",
        ["Log.Directory"]                          = $"{nameof(LoggingOptions)}:{nameof(LoggingOptions.Directory)}",
    };

    /// Pre-scans raw args and:
    /// 1. Extracts and removes the `--config PATH` pair (returned via out param).
    /// 2. Rewrites bare `--metrics` / `--no-version-check` flags into Section:Key=Value pairs
    ///    that AddCommandLine understands. `--no-version-check` inverts to EnableVersionCheck=false.
    /// 3. Translates legacy `--set KEY=VALUE` pairs into `--Section:Key=VALUE` via LegacySetKeyMap
    ///    so muscle-memory commands from the pre-migration CLI keep working.
    /// Everything else passes through untouched for native `--Section:Key=Value` syntax.
    internal static string[] PreprocessArgs(string[] rawArgs, out string? configPath)
    {
        configPath = null;
        var output = new List<string>(rawArgs.Length);

        for (int i = 0; i < rawArgs.Length; i++)
        {
            var arg = rawArgs[i];
            switch (arg)
            {
                case "--config":
                    if (i + 1 >= rawArgs.Length)
                        throw new ArgumentException("--config requires a file path argument");
                    var candidate = rawArgs[++i];
                    if (!File.Exists(candidate))
                        throw new FileNotFoundException($"Config file '{candidate}' does not exist", candidate);
                    configPath = candidate;
                    continue;
                case "--metrics":
                    output.Add($"--{nameof(DiagnosticsOptions)}:{nameof(DiagnosticsOptions.EnableMetrics)}=true");
                    continue;
                case "--no-version-check":
                    output.Add($"--{nameof(DiagnosticsOptions)}:{nameof(DiagnosticsOptions.EnableVersionCheck)}=false");
                    continue;
                case "--set":
                    if (i + 1 >= rawArgs.Length)
                        throw new ArgumentException("--set requires KEY=VALUE");
                    var pair = rawArgs[++i];
                    var eq = pair.IndexOf('=');
                    if (eq <= 0)
                        throw new ArgumentException($"--set expects KEY=VALUE, got '{pair}'");
                    var legacyKey = pair[..eq];
                    var value = pair[(eq + 1)..];
                    if (!LegacySetKeyMap.TryGetValue(legacyKey, out var mappedKey))
                        throw new ArgumentException(
                            $"--set: unknown legacy key '{legacyKey}'. Either add it to Program.LegacySetKeyMap or use native '--Section:Key=Value' syntax.");
                    output.Add($"--{mappedKey}={value}");
                    continue;
                default:
                    output.Add(arg);
                    continue;
            }
        }

        return output.ToArray();
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        TryLogAndFlushException(e.ExceptionObject as Exception, isTerminating: e.IsTerminating,
            source: "AppDomain.UnhandledException");
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        TryLogAndFlushException(e.Exception, isTerminating: false,
            source: "TaskScheduler.UnobservedTaskException");
        e.SetObserved();
    }

    private static void OnProcessExit(object? sender, EventArgs e)
    {
        // Final drain on normal shutdown — covers clean exits that still left
        // async-sink entries in flight.
        try { Log.Shutdown(); } catch { /* best effort */ }
    }

    private static void TryLogAndFlushException(Exception? ex, bool isTerminating, string source)
    {
        try
        {
            if (Log.IsLogging && ex != null)
                Log.outException(ex);
        }
        catch { /* don't let the crash-logger itself crash */ }

        // Always echo to stderr so something is visible even if Log isn't
        // configured yet (config parse failure, very early startup crash).
        try
        {
            Console.Error.WriteLine($"[{source}] terminating={isTerminating}");
            Console.Error.WriteLine(ex?.ToString() ?? "<null exception>");
        }
        catch { /* best effort */ }

        // Flush Serilog synchronously before the CLR tears everything down.
        // Only on terminating events — a non-terminating UnobservedTaskException
        // shouldn't kill the logging pipeline the rest of the process relies on.
        if (isTerminating)
        {
            try { Log.Shutdown(); } catch { /* best effort */ }
        }
    }
}

internal static class OsSpecific
{
    /// Checks whenever or not we are in our own console
    /// For example on Windows you can just double click the exe which spawns a new Console Window Host
    public static bool AreWeInOurOwnConsole()
    {
        try
        {
#if _WINDOWS
            var consoleWindowHandle = GetConsoleWindow();
            GetWindowThreadProcessId(consoleWindowHandle, out var consoleWindowProcess);
            var weAreTheOwner = (consoleWindowProcess == Environment.ProcessId);
            return weAreTheOwner;
#else
            return true;
#endif
        }
        catch
        {
            return false;
        }
    }

#if _WINDOWS
    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll", SetLastError=true)]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
#endif
}
