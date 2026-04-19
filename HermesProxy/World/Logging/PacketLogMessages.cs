using Microsoft.Extensions.Logging;

namespace HermesProxy.World.Logging;

/// <summary>
/// Source-generated logging methods for <see cref="ServerPacket"/> hot paths.
/// The generator produces a static partial method body that:
///   - calls <c>logger.IsEnabled(level)</c> before building the event
///   - captures arguments into a struct state (no params array, no boxing for value types)
///   - uses the message template verbatim at compile time (no runtime parsing)
/// Template placeholders must match parameter names (case-insensitive for Serilog output lookup).
///
/// SYSLIB1015 is suppressed: <c>SourceFile</c> and <c>NetDir</c> are intentionally
/// emitted as overflow properties so the Serilog output template can render them
/// in their own columns rather than inlining them into the message.
/// </summary>
#pragma warning disable SYSLIB1015
internal static partial class PacketLogMessages
{
    // EventId 1-2 range is reserved for ServerPacket span-path logs.

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "{Name} exceeded MaxSize ({Max}), using fallback")]
    public static partial void SpanMissExceededMaxSize(
        ILogger logger,
        string SourceFile,
        string Name,
        int Max);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Trace, // MEL Trace maps to Serilog Verbose
        Message = "{Name}: {Bytes}/{Max} bytes")]
    public static partial void SpanStats(
        ILogger logger,
        string SourceFile,
        string Name,
        int Bytes,
        int Max);
}
