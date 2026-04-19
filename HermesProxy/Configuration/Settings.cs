using HermesProxy.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Framework.Logging;
using Framework.Networking;
using HermesProxy;
using HermesProxy.Configuration;
using Serilog.Events;

namespace Framework;

public static class Settings
{
    public static byte[] ClientSeed = null!;
    public static ClientVersionBuild ClientBuild;
    public static ClientVersionBuild ServerBuild;
    public static string ServerAddress = null!;
    public static int ServerPort;
    public static string ReportedOS = null!;
    public static string ReportedPlatform = null!;
    public static string ExternalAddress = null!;
    public static int RestPort;
    public static int BNetPort;
    public static int RealmPort;
    public static int InstancePort;
    public static bool DebugOutput;
    public static bool PacketsLog;
    public static bool SpanStatsLog;

    public static LogEventLevel LogMinimumLevel;
    public static LogEventLevel LogServerLevel;
    public static LogEventLevel LogNetworkLevel;
    public static LogEventLevel LogStorageLevel;
    public static LogEventLevel LogPacketLevel;
    public static LogEventLevel LogConsoleLevel;
    public static bool LogToFile;
    public static string LogDirectory = "Logs";

    public static bool LoadAndVerifyFrom(ConfigurationParser config)
    {
        ClientSeed = config.GetByteArray("ClientSeed", "179D3DC3235629D07113A9B3867F97A7".ParseAsByteArray());
        ClientBuild = config.GetEnum("ClientBuild", ClientVersionBuild.V2_5_2_40892);
        var serverBuildStr = config.GetString("ServerBuild", "auto");
        if (serverBuildStr == "auto")
            ServerBuild = VersionChecker.GetBestLegacyVersion(ClientBuild);
        else
            ServerBuild = config.GetEnum("ServerBuild", ClientVersionBuild.Zero);
        ServerAddress = config.GetString("ServerAddress", "127.0.0.1");
        ServerPort = config.GetInt("ServerPort", 3724);
        ReportedOS = config.GetString("ReportedOS", "OSX");
        ReportedPlatform = config.GetString("ReportedPlatform", "x86");
        ExternalAddress = config.GetString("ExternalAddress", "127.0.0.1");
        RestPort = config.GetInt("RestPort", 8081);
        BNetPort = config.GetInt("BNetPort", 1119);
        RealmPort = config.GetInt("RealmPort", 8084);
        InstancePort = config.GetInt("InstancePort", 8086);
        DebugOutput = config.GetBoolean("DebugOutput", false);
        PacketsLog = config.GetBoolean("PacketsLog", true);
        SpanStatsLog = config.GetBoolean("SpanStatsLog", false);

        LogMinimumLevel = ParseLogLevel(config.GetString("Log.MinimumLevel", "Information"), LogEventLevel.Information);
        LogServerLevel = ParseLogLevel(config.GetString("Log.Server.MinimumLevel", "Information"), LogEventLevel.Information);
        LogNetworkLevel = ParseLogLevel(config.GetString("Log.Network.MinimumLevel", "Information"), LogEventLevel.Information);
        LogStorageLevel = ParseLogLevel(config.GetString("Log.Storage.MinimumLevel", "Information"), LogEventLevel.Information);
        LogPacketLevel = ParseLogLevel(config.GetString("Log.Packet.MinimumLevel", "Warning"), LogEventLevel.Warning);
        LogConsoleLevel = ParseLogLevel(config.GetString("Log.Console.MinimumLevel", "Information"), LogEventLevel.Information);
        LogToFile = config.GetBoolean("Log.ToFile", true);
        LogDirectory = config.GetString("Log.Directory", "Logs");

        // Back-compat: translate legacy DebugOutput / SpanStatsLog into the new per-category min-levels.
        if (DebugOutput && LogMinimumLevel > LogEventLevel.Debug)
            LogMinimumLevel = LogEventLevel.Debug;
        if (DebugOutput && LogConsoleLevel > LogEventLevel.Debug)
            LogConsoleLevel = LogEventLevel.Debug;
        if (SpanStatsLog && LogPacketLevel > LogEventLevel.Verbose)
            LogPacketLevel = LogEventLevel.Verbose;

        return VerifyConfig();
    }

    private static LogEventLevel ParseLogLevel(string raw, LogEventLevel defaultValue)
    {
        if (Enum.TryParse<LogEventLevel>(raw, ignoreCase: true, out var parsed))
            return parsed;
        return defaultValue;
    }

    public static LogBootstrapOptions ToLogBootstrapOptions()
        => new(LogMinimumLevel, LogServerLevel, LogNetworkLevel, LogStorageLevel, LogPacketLevel, LogConsoleLevel, LogToFile, LogDirectory);
    
    private static bool VerifyConfig()
    {
        if (ClientSeed.Length != 16)
        {
            Log.Print(LogType.Server, "ClientSeed must have byte length of 16 (32 characters)");
            return false;
        }

        if (!VersionChecker.IsSupportedModernVersion(ClientBuild))
        {
            Log.Print(LogType.Server, $"Unsupported ClientBuild '{ClientBuild}'");
            return false;
        }

        if (!VersionChecker.IsSupportedLegacyVersion(ServerBuild))
        {
            Log.Print(LogType.Server, $"Unsupported ServerBuild '{ServerBuild}', use 'auto' to select best");
            return false;
        }

        if (!IsValidPortNumber(RestPort))
        {
            Log.Print(LogType.Server, $"Specified battle.net port ({RestPort}) out of allowed range (1-65535)");
            return false;
        }

        if (!IsValidPortNumber(ServerPort))
        {
            Log.Print(LogType.Server, $"Specified battle.net port ({BNetPort}) out of allowed range (1-65535)");
            return false;
        }

        if (!IsValidPortNumber(BNetPort))
        {
            Log.Print(LogType.Server, $"Specified battle.net port ({BNetPort}) out of allowed range (1-65535)");
            return false;
        }

        if (!IsValidPortNumber(RealmPort))
        {
            Log.Print(LogType.Server, $"Specified battle.net port ({RealmPort}) out of allowed range (1-65535)");
            return false;
        }

        if (!IsValidPortNumber(InstancePort))
        {
            Log.Print(LogType.Server, $"Specified battle.net port ({InstancePort}) out of allowed range (1-65535)");
            return false;
        }

        bool IsValidPortNumber(int someNumber)
        {
            return someNumber > IPEndPoint.MinPort && someNumber < IPEndPoint.MaxPort;
        }

        return true;
    }
}
