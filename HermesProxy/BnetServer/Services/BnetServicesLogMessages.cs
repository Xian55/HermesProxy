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
}
