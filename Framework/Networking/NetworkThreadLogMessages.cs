using Microsoft.Extensions.Logging;

namespace Framework.Networking;

#pragma warning disable SYSLIB1015
internal static partial class NetworkThreadLogMessages
{
    // EventId 900 range is reserved for NetworkThread.

    [LoggerMessage(EventId = 900, Level = LogLevel.Information, Message = "Network Thread Starting")]
    public static partial void NetworkThreadStarting(
        ILogger logger, string SourceFile, string NetDir);
}
