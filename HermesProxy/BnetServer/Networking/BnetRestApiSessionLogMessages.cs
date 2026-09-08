using Microsoft.Extensions.Logging;

namespace BNetServer.Networking;

#pragma warning disable SYSLIB1015
internal static partial class BnetRestApiSessionLogMessages
{
    // EventId 850-859 range is reserved for BnetRestApiSession (800-849 BnetTcpSession).

    [LoggerMessage(EventId = 850, Level = LogLevel.Error,
        Message = "Request handling failed for {Endpoint}, closing connection: {Message}")]
    public static partial void RequestHandlingFailed(
        ILogger logger, System.Exception ex, string SourceFile, string NetDir, string Endpoint, string Message);
}
