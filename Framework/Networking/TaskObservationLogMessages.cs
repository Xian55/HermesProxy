using Microsoft.Extensions.Logging;

namespace Framework.Networking;

#pragma warning disable SYSLIB1015
internal static partial class TaskObservationLogMessages
{
    // EventId 910-919 range is reserved for TaskObservation.

    [LoggerMessage(EventId = 910, Level = LogLevel.Error, Message = "{Operation} faulted for {Context}: {Message}")]
    public static partial void BackgroundWorkFaulted(
        ILogger logger, System.Exception ex, string SourceFile, string NetDir, string Operation, string Context, string Message);
}
