using BNetServer.Networking;
using BNetServer.Services;
using Framework.Logging;
using Framework.Metrics;
using HermesProxy.Auth;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Server;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HermesProxy.World.Enums;
using HermesProxy.Enums;

namespace HermesProxy;

partial class Server
{
    // These MEL loggers are captured during Server's static init, which runs BEFORE
    // Log.Configure rebuilds the Serilog pipeline. Log.CreateMelLogger returns a SwappableMelLogger
    // that re-resolves through the current MEL factory on every call — so captures made here
    // remain valid after Log.Configure without any runtime reinitialisation.
    private static readonly Microsoft.Extensions.Logging.ILogger _melServer = Log.CreateMelLogger(Log.CategoryServer);
    private static readonly Microsoft.Extensions.Logging.ILogger _melNetwork = Log.CreateMelLogger(Log.CategoryNetwork);
    private static readonly string _sourceFile = nameof(Server).PadRight(15);
    private const string _netDirNone = "";

    /// <summary>
    /// Global metrics collector for packet statistics.
    /// Only active when Diagnostics.EnableMetrics is on.
    /// </summary>
    public static readonly ProxyMetrics Metrics = new();

    /// <summary>
    /// Whether metrics collection is enabled. Set once by ProxyHostedService from DiagnosticsOptions.
    /// </summary>
    public static bool MetricsEnabled { get; internal set; }

    internal static void LogVersion()
        => ServerLogMessages.Version(_melServer, _sourceFile, _netDirNone, GetVersionInformation());

    internal static void LogClientAndServerBuild(ClientVersionBuild clientBuild, ClientVersionBuild serverBuild)
    {
        ServerLogMessages.ModernClientBuild(_melServer, _sourceFile, _netDirNone, clientBuild);
        ServerLogMessages.LegacyServerBuild(_melServer, _sourceFile, _netDirNone, serverBuild);
    }

    internal static void LogExternalIp(string address)
        => ServerLogMessages.ExternalIp(_melNetwork, _sourceFile, _netDirNone, address);

    internal static void LogExternalAddressLoopback(string externalAddress, string bindAddress)
        => ServerLogMessages.ExternalAddressLoopback(_melNetwork, _sourceFile, _netDirNone, externalAddress, bindAddress);

    internal static void LogOldFilesSwept(int count, string kind, int days, long megabytes)
        => ServerLogMessages.OldFilesSwept(_melServer, _sourceFile, _netDirNone, count, kind, days, megabytes);

    internal static void LogStartingService(string serviceName, string endpoint)
        => ServerLogMessages.StartingService(_melServer, _sourceFile, _netDirNone, serviceName, endpoint);

    internal static void RegisterLogCallerMappings()
    {
        // Route legacy Log.Print(LogType.Warn|Error, ...) calls to the appropriate Serilog category
        // based on the caller file that the [CallerFilePath] attribute resolves to.
        Log.RegisterCallerMapping(nameof(AuthClient), Log.Network);
        Log.RegisterCallerMapping(nameof(BnetTcpSession), Log.Network);
        Log.RegisterCallerMapping(nameof(BnetServices), Log.Network);
        Log.RegisterCallerMapping(nameof(BNetServer.LoginServiceManager), Log.Network);
        Log.RegisterCallerMapping(nameof(RealmSocket), Log.Network);
        Log.RegisterCallerMapping("ConnectionSweeper", Log.Network); // generic type, no clean nameof form
        Log.RegisterCallerMapping(nameof(WorldClient), Log.Packet);
        Log.RegisterCallerMapping(nameof(WorldSocket), Log.Packet);
        Log.RegisterCallerMapping(nameof(WorldSocketManager), Log.Packet);
        Log.RegisterCallerMapping(nameof(GameData), Log.Storage);
    }

    internal static async Task CheckForUpdate()
    {
        const string hermesGitHubRepo = "Xian55/HermesProxy";

        try
        {
            #pragma warning disable CS0162 // GitVersion constants vary per build environment
            // Releases are built from master, before their tag exists, so the commits since the
            // last tag can't tell one from a test branch. A branch is not behind a release anyway.
            if (GitVersionInformation.BranchName != "master" || GitVersionInformation.UncommittedChanges != "0")
                return;

            using var client = new HttpClient();
            #pragma warning restore CS0162
            client.Timeout = TimeSpan.FromSeconds(5);
            client.DefaultRequestHeaders.Add("User-Agent", "curl/7.0.0"); // otherwise we get blocked
            var response = await client.GetAsync($"https://api.github.com/repos/{hermesGitHubRepo}/releases/latest");
            response.EnsureSuccessStatusCode();

            string rawJson = await response.Content.ReadAsStringAsync();
            var parsedJson = JsonSerializer.Deserialize(rawJson, HermesJsonContext.Default.DictionaryStringObject);

            string? latest = parsedJson!["tag_name"].ToString();
            if (IsNewerRelease(latest, GitVersionInformation.MajorMinorPatch))
            {
                ServerLogMessages.UpdateAvailable(_melServer, _sourceFile, _netDirNone,
                    latest!, GitVersionInformation.MajorMinorPatch, $"https://github.com/{hermesGitHubRepo}/releases/latest");
            }
        }
        catch
        {
            // ignore
        }
    }

    internal static bool IsNewerRelease(string? latestTag, string current)
        => Version.TryParse(latestTag?.TrimStart('v'), out var latest)
        && Version.TryParse(current, out var mine)
        && latest > mine;

    // Every metrics summary names each of its opcode rows, and a value's name never changes, so
    // each is resolved once. Enum.IsDefined boxed the value, and the name table ToString reads
    // does not survive a GC, so a summary a minute after the last one rebuilt all of it.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, string> _opcodeNames = new();

    internal static string ResolveOpcodeName(int opcode) => _opcodeNames.GetOrAdd(opcode, static op =>
        Enum.IsDefined(typeof(Opcode), (uint)op) ? ((Opcode)op).ToString() : $"0x{op:X4}");

    private static readonly string? _buildTag = null;
    #pragma warning disable CS0162 // GitVersion constants vary per build environment
    internal static string GetVersionInformation()
    {
        var commitDate = DateTime.Parse(GitVersionInformation.CommitDate, CultureInfo.InvariantCulture).ToUniversalTime();

        string version = $"{commitDate:yyyy-MM-dd} {_buildTag}{GitVersionInformation.MajorMinorPatch}";
        if (GitVersionInformation.CommitsSinceVersionSource != "0")
            version += $"+{GitVersionInformation.CommitsSinceVersionSource}({GitVersionInformation.ShortSha})";
        if (GitVersionInformation.UncommittedChanges != "0")
            version += " dirty";
        return version;
    }
    #pragma warning restore CS0162
}
