using Microsoft.Extensions.Logging;

namespace BNetServer.Networking;

#pragma warning disable SYSLIB1015
internal static partial class BnetTcpSessionLogMessages
{
    // EventId 800-899 range is reserved for BnetTcpSession.

    [LoggerMessage(EventId = 800, Level = LogLevel.Information, Message = "Accepting connection from {Endpoint}.")]
    public static partial void AcceptingConnection(
        ILogger logger, string SourceFile, string NetDir, string Endpoint);
}
