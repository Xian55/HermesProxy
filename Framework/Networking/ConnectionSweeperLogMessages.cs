using Microsoft.Extensions.Logging;

namespace Framework.Networking;

#pragma warning disable SYSLIB1015
internal static partial class ConnectionSweeperLogMessages
{
    // EventId 900-909 range is reserved for ConnectionSweeper.

    [LoggerMessage(EventId = 900, Level = LogLevel.Error, Message = "Checking a connection failed, dropping it: {Message}")]
    public static partial void UpdateFailed(
        ILogger logger, System.Exception ex, string SourceFile, string NetDir, string Message);

    [LoggerMessage(EventId = 901, Level = LogLevel.Error, Message = "Closing a connection failed: {Message}")]
    public static partial void CloseFailed(
        ILogger logger, System.Exception ex, string SourceFile, string NetDir, string Message);

    [LoggerMessage(EventId = 902, Level = LogLevel.Critical, Message = "Connections are no longer being swept: {Message}")]
    public static partial void SweepEnded(
        ILogger logger, System.Exception ex, string SourceFile, string NetDir, string Message);
}
