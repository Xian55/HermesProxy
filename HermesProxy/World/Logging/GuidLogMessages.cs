using Microsoft.Extensions.Logging;

namespace HermesProxy.World.Logging;

/// <summary>
/// Source-generated logging for guid conversion. Only the unrecognised-high-guid paths log:
/// they mean an object is about to be dropped on the modern side, and they were interpolated
/// <c>Log.Print(LogType.Warn, $"...")</c> calls sitting on the per-guid lookup path.
///
/// Warning level, and both fire per offending guid rather than once — a backend that sends an
/// unmapped high sends it for every object of that kind, so the repetition is the signal that
/// a mapping is missing.
///
/// EventId 1300-1309 is reserved for this file (1200-1219 update handler, 1100-1114 gameobject
/// fields, 900-909 object lifecycle).
/// </summary>
internal static partial class GuidLogMessages
{
    [LoggerMessage(
        EventId = 1300,
        Level = LogLevel.Warning,
        Message = "[HighGuidLegacy] Unknown legacy high-guid 0x{High:X4} — treating as Null. " +
                  "Object will be skipped on the modern side.")]
    public static partial void UnknownLegacyHighGuid(ILogger logger, uint high);

    [LoggerMessage(
        EventId = 1301,
        Level = LogLevel.Warning,
        Message = "[HighGuid703] Unknown 703 high-guid 0x{High:X2} ({High}) — treating as Null. " +
                  "Object will be skipped on the modern side.")]
    public static partial void Unknown703HighGuid(ILogger logger, byte high);

    // Fires at most once per session, on the first item guid a 0x4700 backend sends. Information
    // rather than Debug because it changes what every later item-guid packet puts on the wire, so
    // it is worth having in a user's log when an item action misbehaves.
    [LoggerMessage(
        EventId = 1302,
        Level = LogLevel.Information,
        Message = "[ItemGuid] Backend uses high-guid 0x{High:X4} for items; rebuilding item guids with it.")]
    public static partial void LearnedLegacyItemHigh(ILogger logger, uint high);
}
