using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace Framework.Networking;

#pragma warning disable SYSLIB1015
internal static partial class AsyncAcceptorLogMessages
{
    // EventId 920-929 range is reserved for AsyncAcceptor.

    [LoggerMessage(EventId = 920, Level = LogLevel.Debug, Message = "A connection was lost before it could be accepted: {Error}")]
    public static partial void ConnectionLostInBacklog(
        ILogger logger, string SourceFile, string NetDir, SocketError Error);

    [LoggerMessage(EventId = 921, Level = LogLevel.Error, Message = "Accept failed with {Error}, trying again in {Delay}")]
    public static partial void AcceptFailed(
        ILogger logger, System.Exception ex, string SourceFile, string NetDir, SocketError Error, System.TimeSpan Delay);

    [LoggerMessage(EventId = 922, Level = LogLevel.Error, Message = "Setting up an accepted connection failed: {Message}")]
    public static partial void ConnectionSetupFailed(
        ILogger logger, System.Exception ex, string SourceFile, string NetDir, string Message);

    [LoggerMessage(EventId = 923, Level = LogLevel.Critical, Message = "The listener stopped accepting connections: {Message}")]
    public static partial void AcceptLoopEnded(
        ILogger logger, System.Exception ex, string SourceFile, string NetDir, string Message);
}
