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
using System.Threading;
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
        Log.RegisterCallerMapping("NetworkThread", Log.Network); // generic type, no clean nameof form
        Log.RegisterCallerMapping(nameof(WorldClient), Log.Packet);
        Log.RegisterCallerMapping(nameof(WorldSocket), Log.Packet);
        Log.RegisterCallerMapping(nameof(WorldSocketManager), Log.Packet);
        Log.RegisterCallerMapping(nameof(GameData), Log.Storage);
    }

    internal static async Task CheckForUpdate()
    {
        const string hermesGitHubRepo = "WowLegacyCore/HermesProxy";

        try
        {
            #pragma warning disable CS0162 // GitVersion constants vary per build environment
            if (GitVersionInformation.CommitsSinceVersionSource != "0" || GitVersionInformation.UncommittedChanges != "0")
                return; // we are probably in a test branch

            using var client = new HttpClient();
            #pragma warning restore CS0162
            client.Timeout = TimeSpan.FromSeconds(5);
            client.DefaultRequestHeaders.Add("User-Agent", "curl/7.0.0"); // otherwise we get blocked
            var response = await client.GetAsync($"https://api.github.com/repos/{hermesGitHubRepo}/releases/latest");
            response.EnsureSuccessStatusCode();

            string rawJson = await response.Content.ReadAsStringAsync();
            var parsedJson = JsonSerializer.Deserialize<Dictionary<string, object>>(rawJson);

            string? commitDateStr = parsedJson!["created_at"].ToString();
            DateTime commitDate = DateTime.Parse(commitDateStr!, CultureInfo.InvariantCulture).ToUniversalTime();

            string myCommitDateStr = GitVersionInformation.CommitDate;
            DateTime myCommitDate = DateTime.Parse(myCommitDateStr, CultureInfo.InvariantCulture).ToUniversalTime();

            if (commitDate > myCommitDate)
            {
                Console.WriteLine("------------------------");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"HermesProxy update available v{GitVersionInformation.Major}.{GitVersionInformation.Minor} => {parsedJson!["tag_name"]} ({commitDate:yyyy-MM-dd})");
                Console.WriteLine("Please download new version from https://github.com/WowLegacyCore/HermesProxy/releases/latest");
                Console.ResetColor();
                Console.WriteLine("------------------------");
                Console.WriteLine();
                Thread.Sleep(10_000);
            }
        }
        catch
        {
            // ignore
        }
    }

    internal static string ResolveOpcodeName(int opcode)
    {
        if (Enum.IsDefined(typeof(Opcode), (uint)opcode))
            return ((Opcode)opcode).ToString();
        return $"0x{opcode:X4}";
    }

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
