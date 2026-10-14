using Framework.Constants;
using Microsoft.Extensions.Logging;

namespace BNetServer.Services;

/// <summary>
/// Source-generated logging methods for the BnetServices RPC dispatch. Covers the highest-volume
/// call sites in <see cref="BnetServices.ServiceManager"/>; other <c>ServiceLog</c> callers can be
/// migrated incrementally.
/// </summary>
#pragma warning disable SYSLIB1015
internal static partial class BnetServicesLogMessages
{
    // EventId 500-599 range is reserved for BnetServices.

    [LoggerMessage(
        EventId = 500,
        Level = LogLevel.Warning,
        Message = "{Prefix} Client requested service {ServiceHash}/m:{MethodId} but this service is not implemented?")]
    public static partial void ServiceNotImplemented(
        ILogger logger,
        string SourceFile,
        string NetDir,
        string Prefix,
        OriginalHash ServiceHash,
        uint MethodId);

    [LoggerMessage(
        EventId = 501,
        Level = LogLevel.Warning,
        Message = "{Prefix} Client requested service {ServiceHash}/m:{MethodId} but with invalid state, required: {Required} but only has {Current}!")]
    public static partial void ServiceInvalidState(
        ILogger logger,
        string SourceFile,
        string NetDir,
        string Prefix,
        OriginalHash ServiceHash,
        uint MethodId,
        ServiceRequirement Required,
        ServiceRequirement Current);

    [LoggerMessage(
        EventId = 502,
        Level = LogLevel.Debug,
        Message = "{Prefix} Client requested service {ServiceHash}/m:{MethodId}")]
    public static partial void ServiceRequested(
        ILogger logger,
        string SourceFile,
        string NetDir,
        string Prefix,
        OriginalHash ServiceHash,
        uint MethodId);

    [LoggerMessage(
        EventId = 503,
        Level = LogLevel.Error,
        Message = "Tried to override ServiceHandler: {Existing} with {MethodName} (ServiceHash: {ServiceHash} MethodId: {MethodId})")]
    public static partial void ServiceHandlerOverrideAttempt(
        ILogger logger,
        string SourceFile,
        string NetDir,
        string Existing,
        string MethodName,
        OriginalHash ServiceHash,
        uint MethodId);

    [LoggerMessage(
        EventId = 504,
        Level = LogLevel.Error,
        Message = "Method: {MethodName} needs atleast one parameter")]
    public static partial void ServiceHandlerMissingParameters(
        ILogger logger,
        string SourceFile,
        string NetDir,
        string MethodName);

    // 505-509: which realm a client goes to after logging in. A client that gets a last realm
    // joins it at once; one that does not opens its realm selection and waits for a click, which
    // from the outside looks like a login that hangs.

    [LoggerMessage(
        EventId = 505,
        Level = LogLevel.Information,
        Message = "{Prefix} Last realm of this account: '{Realm}' (last character: {Character}). The client goes straight to that realm.")]
    public static partial void LastRealmAnswered(
        ILogger logger,
        string SourceFile,
        string NetDir,
        string Prefix,
        string Character,
        string Realm);

    [LoggerMessage(
        EventId = 506,
        Level = LogLevel.Information,
        Message = "{Prefix} No last realm is stored for this account. The client opens its realm selection.")]
    public static partial void NoLastRealmStored(
        ILogger logger,
        string SourceFile,
        string NetDir,
        string Prefix);

    [LoggerMessage(
        EventId = 507,
        Level = LogLevel.Information,
        Message = "{Prefix} The last realm of this account, '{Realm}', is not among the {Count} realm(s) of this server (or is offline). The client opens its realm selection.")]
    public static partial void LastRealmNotOffered(
        ILogger logger,
        string SourceFile,
        string NetDir,
        string Prefix,
        string Realm,
        int Count);

    [LoggerMessage(
        EventId = 508,
        Level = LogLevel.Information,
        Message = "{Prefix} Realm selection: sent the client {Count} realm(s).")]
    public static partial void RealmSelectionSent(
        ILogger logger,
        string SourceFile,
        string NetDir,
        string Prefix,
        int Count);

    [LoggerMessage(
        EventId = 509,
        Level = LogLevel.Information,
        Message = "{Prefix} Joining realm '{Realm}'.")]
    public static partial void JoiningRealm(
        ILogger logger,
        string SourceFile,
        string NetDir,
        string Prefix,
        string Realm);

    [LoggerMessage(
        EventId = 510,
        Level = LogLevel.Debug,
        Message = "Battlenet.LogonRequest: the client's standard offset from UTC is {Offset}; its calendar goes by {Zone}")]
    public static partial void ClientTimeZone(ILogger logger, System.TimeSpan? Offset, string Zone);

    // 511-512: the messages themselves, as JSON. Callers build the JSON only when Trace is on.

    [LoggerMessage(
        EventId = 511,
        Level = LogLevel.Trace,
        Message = "{Prefix} {ServiceHash}/m:{MethodId} token {Token} request: {Content}")]
    public static partial void RequestContent(
        ILogger logger,
        string SourceFile,
        string NetDir,
        string Prefix,
        OriginalHash ServiceHash,
        uint MethodId,
        uint Token,
        string Content);

    [LoggerMessage(
        EventId = 512,
        Level = LogLevel.Trace,
        Message = "{Prefix} {ServiceHash}/m:{MethodId} token {Token} answer {Status}: {Content}")]
    public static partial void ResponseContent(
        ILogger logger,
        string SourceFile,
        string NetDir,
        string Prefix,
        OriginalHash ServiceHash,
        uint MethodId,
        uint Token,
        BattlenetRpcErrorCode Status,
        string Content);
}
