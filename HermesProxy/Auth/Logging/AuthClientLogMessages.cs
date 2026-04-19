using System;
using Microsoft.Extensions.Logging;

namespace HermesProxy.Auth.Logging;

/// <summary>
/// Source-generated logging methods for <see cref="AuthClient"/>. Covers the SRP6 login flow,
/// realmlist request, and the auth socket's receive/send callbacks. <c>SourceFile</c> and
/// <c>NetDir</c> are intentional overflow properties — the Serilog output template renders them
/// in their own columns.
/// </summary>
#pragma warning disable SYSLIB1015
internal static partial class AuthClientLogMessages
{
    // EventId 300-399 range is reserved for AuthClient.

    [LoggerMessage(
        EventId = 300,
        Level = LogLevel.Information,
        Message = "Connecting to auth server... (realmlist addr: {Address}:{Port}) (resolved as: {ResolvedAddress}:{Port})")]
    public static partial void ConnectingToAuthServer(
        ILogger logger,
        string SourceFile,
        string NetDir,
        string Address,
        int Port,
        string ResolvedAddress);

    [LoggerMessage(
        EventId = 301,
        Level = LogLevel.Information,
        Message = "Reconnecting to auth server... (realmlist addr: {Address}:{Port}) (resolved as: {ResolvedAddress}:{Port})")]
    public static partial void ReconnectingToAuthServer(
        ILogger logger,
        string SourceFile,
        string NetDir,
        string Address,
        int Port,
        string ResolvedAddress);

    [LoggerMessage(EventId = 302, Level = LogLevel.Error, Message = "Socket Error: {Message}")]
    public static partial void SocketError(
        ILogger logger,
        Exception ex,
        string SourceFile,
        string NetDir,
        string Message);

    [LoggerMessage(EventId = 303, Level = LogLevel.Error, Message = "Connect Error: {Message}")]
    public static partial void ConnectError(
        ILogger logger,
        Exception ex,
        string SourceFile,
        string NetDir,
        string Message);

    [LoggerMessage(EventId = 304, Level = LogLevel.Error, Message = "Socket Closed By Server")]
    public static partial void SocketClosedByServer(
        ILogger logger,
        string SourceFile,
        string NetDir);

    [LoggerMessage(EventId = 305, Level = LogLevel.Error, Message = "Packet Read Error: {Message}")]
    public static partial void PacketReadError(
        ILogger logger,
        Exception ex,
        string SourceFile,
        string NetDir,
        string Message);

    [LoggerMessage(EventId = 306, Level = LogLevel.Error, Message = "Packet Send Error: {Message}")]
    public static partial void PacketSendError(
        ILogger logger,
        Exception ex,
        string SourceFile,
        string NetDir,
        string Message);

    [LoggerMessage(EventId = 307, Level = LogLevel.Error, Message = "Packet Write Error: {Message}")]
    public static partial void PacketWriteError(
        ILogger logger,
        Exception ex,
        string SourceFile,
        string NetDir,
        string Message);

    [LoggerMessage(EventId = 308, Level = LogLevel.Debug, Message = "Received opcode {Opcode} size {Size}.")]
    public static partial void PacketReceived(
        ILogger logger,
        string SourceFile,
        string NetDir,
        AuthCommand Opcode,
        int Size);

    [LoggerMessage(EventId = 309, Level = LogLevel.Error, Message = "No handler for opcode {Opcode}!")]
    public static partial void NoHandlerForOpcode(
        ILogger logger,
        string SourceFile,
        string NetDir,
        AuthCommand Opcode);

    [LoggerMessage(EventId = 310, Level = LogLevel.Error, Message = "Login failed. Reason: {Reason}")]
    public static partial void LoginFailed(
        ILogger logger,
        string SourceFile,
        string NetDir,
        AuthResult Reason);

    [LoggerMessage(EventId = 311, Level = LogLevel.Error, Message = "Authentication failed!")]
    public static partial void AuthenticationFailed(
        ILogger logger,
        string SourceFile,
        string NetDir);

    [LoggerMessage(EventId = 312, Level = LogLevel.Information, Message = "Authentication succeeded!")]
    public static partial void AuthenticationSucceeded(
        ILogger logger,
        string SourceFile,
        string NetDir);

    [LoggerMessage(EventId = 313, Level = LogLevel.Error, Message = "Reconnect failed. Reason: {Reason}")]
    public static partial void ReconnectFailed(
        ILogger logger,
        string SourceFile,
        string NetDir,
        AuthResult Reason);

    [LoggerMessage(EventId = 314, Level = LogLevel.Information, Message = "Requesting RealmList update for {Username}")]
    public static partial void RequestingRealmListUpdate(
        ILogger logger,
        string SourceFile,
        string NetDir,
        string Username);

    [LoggerMessage(EventId = 315, Level = LogLevel.Information, Message = "Received {Count} realms.")]
    public static partial void ReceivedRealms(
        ILogger logger,
        string SourceFile,
        string NetDir,
        ushort Count);
}
