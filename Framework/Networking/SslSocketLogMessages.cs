using Microsoft.Extensions.Logging;

namespace Framework.Networking;

#pragma warning disable SYSLIB1015
internal static partial class SslSocketLogMessages
{
    // EventId 910-919 range is reserved for SSLSocket.

    [LoggerMessage(EventId = 910, Level = LogLevel.Error, Message = "Read handler faulted for {Endpoint}: {Message}")]
    public static partial void ReadHandlerFaulted(
        ILogger logger, System.Exception ex, string SourceFile, string NetDir, string Endpoint, string Message);
}
