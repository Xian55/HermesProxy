using HermesProxy.World.Enums;
using Microsoft.Extensions.Logging;

namespace HermesProxy.World.Logging;

/// <summary>
/// Source-generated logging for movement info translation.
/// </summary>
/// <remarks>
/// EventId 1610-1619 is reserved for this file.
/// </remarks>
internal static partial class MovementLogMessages
{
    [LoggerMessage(
        EventId = 1610,
        Level = LogLevel.Error,
        Message = "Violation of MovementFlags found. MovementFlags: {Flags}, MovementFlags2: {FlagsExtra}. Mask {Mask} will be removed.")]
    public static partial void ViolatingFlagsRemoved(
        ILogger logger, uint flags, uint flagsExtra, MovementFlagModern mask);

    [LoggerMessage(
        EventId = 1611,
        Level = LogLevel.Warning,
        Message = "{Opcode} ({Length} bytes) fits none of the shapes TrinityCore 4.3.4 writes it in; dropped.")]
    public static partial void LegacyMovementFitsNoShape(ILogger logger, Opcode opcode, int length);

    [LoggerMessage(
        EventId = 1612,
        Level = LogLevel.Debug,
        Message = "{Opcode} ends a fall the client reported no landing for; sent MSG_MOVE_FALL_LAND before it.")]
    public static partial void LeftOutLandingReported(ILogger logger, Opcode opcode);
}
