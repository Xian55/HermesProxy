using HermesProxy.World.Enums;
using Microsoft.Extensions.Logging;

namespace HermesProxy.World.Logging;

/// <summary>
/// Source-generated logging methods for <see cref="Server.WorldSocket"/> hot paths.
/// <c>NetDir</c> and <c>SourceFile</c> are intentional overflow properties so the
/// Serilog output template can render them in their own columns.
/// </summary>
#pragma warning disable SYSLIB1015
internal static partial class WorldSocketLogMessages
{
    // EventId 100-199 range is reserved for WorldSocket packet dispatch.

    [LoggerMessage(
        EventId = 100,
        Level = LogLevel.Debug,
        Message = "Received opcode {Opcode} ({OpcodeId}).")]
    public static partial void PacketReceived(
        ILogger logger,
        string SourceFile,
        string NetDir,
        Opcode Opcode,
        uint OpcodeId);

    [LoggerMessage(
        EventId = 101,
        Level = LogLevel.Debug,
        Message = "Sending opcode {Opcode} ({OpcodeId}).")]
    public static partial void PacketSent(
        ILogger logger,
        string SourceFile,
        string NetDir,
        Opcode Opcode,
        uint OpcodeId);

    [LoggerMessage(
        EventId = 102,
        Level = LogLevel.Warning,
        Message = "No handler for opcode {Opcode} ({OpcodeId}) (Got unknown packet from ModernClient)")]
    public static partial void NoHandlerForOpcode(
        ILogger logger,
        string SourceFile,
        string NetDir,
        Opcode Opcode,
        uint OpcodeId);
}
