using HermesProxy.Enums;
using Microsoft.Extensions.Logging;

namespace HermesProxy;

/// <summary>
/// Source-generated logging methods for the top-level server lifecycle
/// (<see cref="Server"/> and <see cref="VersionChecker"/> startup lines).
/// </summary>
#pragma warning disable SYSLIB1015
internal static partial class ServerLogMessages
{
    // EventId 600-699 range is reserved for server startup / lifecycle.

    [LoggerMessage(EventId = 600, Level = LogLevel.Information, Message = "Version {Version}")]
    public static partial void Version(
        ILogger logger, string SourceFile, string NetDir, string Version);

    [LoggerMessage(EventId = 601, Level = LogLevel.Information, Message = "Modern (Client) Build: {Build}")]
    public static partial void ModernClientBuild(
        ILogger logger, string SourceFile, string NetDir, ClientVersionBuild Build);

    [LoggerMessage(EventId = 602, Level = LogLevel.Information, Message = "Legacy (Server) Build: {Build}")]
    public static partial void LegacyServerBuild(
        ILogger logger, string SourceFile, string NetDir, ClientVersionBuild Build);

    [LoggerMessage(EventId = 603, Level = LogLevel.Information, Message = "External IP: {Address}")]
    public static partial void ExternalIp(
        ILogger logger, string SourceFile, string NetDir, string Address);

    [LoggerMessage(EventId = 604, Level = LogLevel.Information, Message = "Starting {ServiceName} service on {Endpoint}...")]
    public static partial void StartingService(
        ILogger logger, string SourceFile, string NetDir, string ServiceName, string Endpoint);

    [LoggerMessage(EventId = 605, Level = LogLevel.Information, Message = "Loaded {Count} modern opcodes.")]
    public static partial void LoadedModernOpcodes(
        ILogger logger, string SourceFile, string NetDir, int Count);

    [LoggerMessage(EventId = 606, Level = LogLevel.Information, Message = "Loaded {Count} legacy opcodes.")]
    public static partial void LoadedLegacyOpcodes(
        ILogger logger, string SourceFile, string NetDir, int Count);

    [LoggerMessage(EventId = 607, Level = LogLevel.Information, Message = "Stop requested through {EventName}, shutting down.")]
    public static partial void StopRequested(
        ILogger logger, string SourceFile, string NetDir, string EventName);

    [LoggerMessage(EventId = 608, Level = LogLevel.Error, Message =
        "This client logs in with SRP and never sends its password, and no legacy account password was given: " +
        "set HERMES_LegacyServerOptions__Password. Logins will fail until then.")]
    public static partial void SrpPasswordMissing(
        ILogger logger, string SourceFile, string NetDir);
}
