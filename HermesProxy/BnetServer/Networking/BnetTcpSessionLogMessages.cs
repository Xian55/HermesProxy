using Microsoft.Extensions.Logging;

namespace BNetServer.Networking;

#pragma warning disable SYSLIB1015
internal static partial class BnetTcpSessionLogMessages
{
    // EventId 800-849 range is reserved for BnetTcpSession (850-859 BnetRestApiSession).

    [LoggerMessage(EventId = 800, Level = LogLevel.Information, Message = "Accepting connection from {Endpoint}.")]
    public static partial void AcceptingConnection(
        ILogger logger, string SourceFile, string NetDir, string Endpoint);

    [LoggerMessage(EventId = 801, Level = LogLevel.Error,
        Message = "Dropping frame service {ServiceHash} method {MethodId} token {Token}: {Message}")]
    public static partial void FrameDispatchFailed(
        ILogger logger, System.Exception ex, string SourceFile, string NetDir,
        uint ServiceHash, uint MethodId, uint Token, string Message);

    [LoggerMessage(EventId = 802, Level = LogLevel.Error,
        Message = "Malformed frame header from {Endpoint}, closing connection: {Message}")]
    public static partial void FrameParseFailed(
        ILogger logger, System.Exception ex, string SourceFile, string NetDir, string Endpoint, string Message);

    [LoggerMessage(EventId = 803, Level = LogLevel.Error,
        Message = "Buffer processing failed for {Endpoint}, closing connection: {Message}")]
    public static partial void BufferProcessingFailed(
        ILogger logger, System.Exception ex, string SourceFile, string NetDir, string Endpoint, string Message);
}
