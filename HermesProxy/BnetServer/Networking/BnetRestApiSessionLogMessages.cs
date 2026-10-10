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

    [LoggerMessage(EventId = 851, Level = LogLevel.Debug,
        Message = "REST {Method} {Path} from {Endpoint}")]
    public static partial void RequestReceived(
        ILogger logger, string SourceFile, string NetDir, string Method, string Path, string Endpoint);

    // A client asking for a route the proxy does not serve gets a 404 and a closed socket, which
    // it shows as nothing more than a login that never finishes.
    [LoggerMessage(EventId = 852, Level = LogLevel.Warning,
        Message = "REST {Method} {Path} has no handler; answered 404 and closed the connection")]
    public static partial void RouteNotFound(
        ILogger logger, string SourceFile, string NetDir, string Method, string Path);

    [LoggerMessage(EventId = 853, Level = LogLevel.Warning,
        Message = "REST request of {Length} bytes could not be parsed, closing the connection; request line: {RequestLine}")]
    public static partial void RequestUnparsed(
        ILogger logger, string SourceFile, string NetDir, int Length, string RequestLine);

    [LoggerMessage(EventId = 854, Level = LogLevel.Debug,
        Message = "SRP challenge (v{Version}) issued for account {Account}")]
    public static partial void SrpChallengeIssued(
        ILogger logger, string SourceFile, string NetDir, string Account, byte Version);

    // The proof is all an SRP client sends, so a wrong password and a proxy computing the
    // exchange differently from the client look the same from here.
    [LoggerMessage(EventId = 855, Level = LogLevel.Warning,
        Message = "SRP proof for account {Account} did not verify: the password typed in the client is not the configured legacy password")]
    public static partial void SrpProofRejected(
        ILogger logger, string SourceFile, string NetDir, string Account);

    [LoggerMessage(EventId = 856, Level = LogLevel.Warning,
        Message = "SRP proof for account {Account} has no challenge to answer, or it expired")]
    public static partial void SrpProofWithoutChallenge(
        ILogger logger, string SourceFile, string NetDir, string Account);

    [LoggerMessage(EventId = 857, Level = LogLevel.Error,
        Message = "SRP login for account {Account} refused: no legacy account password is configured (HERMES_LegacyServerOptions__Password)")]
    public static partial void SrpPasswordNotConfigured(
        ILogger logger, string SourceFile, string NetDir, string Account);
}
