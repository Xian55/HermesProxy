using Microsoft.Extensions.Logging;

namespace Framework.Networking;

#pragma warning disable SYSLIB1015
internal static partial class SocketBaseLogMessages
{
    // EventId 930-939 range is reserved for SocketBase.

    [LoggerMessage(EventId = 930, Level = LogLevel.Error, Message = "Reading from {Endpoint} failed, closing it: {Message}")]
    public static partial void ReadFailed(
        ILogger logger, System.Exception ex, string SourceFile, string NetDir, string Endpoint, string Message);

    [LoggerMessage(EventId = 931, Level = LogLevel.Error, Message = "Closing {Endpoint} failed: {Message}")]
    public static partial void CloseFailed(
        ILogger logger, System.Exception ex, string SourceFile, string NetDir, string Endpoint, string Message);
}
