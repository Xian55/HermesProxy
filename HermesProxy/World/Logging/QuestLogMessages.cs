using HermesProxy.Enums;
using HermesProxy.World.Enums;
using Microsoft.Extensions.Logging;

namespace HermesProxy.World.Logging;

/// <summary>
/// Source-generated logging for quest packets, replacing interpolated
/// <c>Log.Print(LogType.Trace, $"[QuestStatusTrace] …")</c> calls in the quest-giver status
/// writers. Those formatted a GUID and a status enum per quest giver on every status packet with
/// Trace off; the status name also rebuilt its enum's name table after each GC.
/// </summary>
/// <remarks>
/// Guids are logged as their two raw halves, as in <see cref="UpdateHandlerLogMessages"/>.
/// EventId 1600-1609 is reserved for this file.
/// </remarks>
internal static partial class QuestLogMessages
{
    [LoggerMessage(
        EventId = 1600,
        Level = LogLevel.Trace,
        Message = "[QuestStatusTrace] SMSG_QUEST_GIVER_STATUS write{Path}: guidLow={GuidLow} guidHigh={GuidHigh} " +
                  "entry={Entry} modern={Status} (0x{Encoded:X}) build={Build}")]
    public static partial void QuestGiverStatusWrite(
        ILogger logger, string path, ulong guidLow, ulong guidHigh, uint entry,
        QuestGiverStatusModern status, uint encoded, ClientVersionBuild build);

    [LoggerMessage(
        EventId = 1601,
        Level = LogLevel.Trace,
        Message = "[QuestStatusTrace] SMSG_QUEST_GIVER_STATUS_MULTIPLE write{Path}: count={Count} build={Build}")]
    public static partial void QuestGiverStatusMultipleWrite(
        ILogger logger, string path, int count, ClientVersionBuild build);

    [LoggerMessage(
        EventId = 1602,
        Level = LogLevel.Trace,
        Message = "[QuestStatusTrace]   {Path}[{Index}] guidLow={GuidLow} guidHigh={GuidHigh} entry={Entry} " +
                  "modern={Status} (0x{Encoded:X})")]
    public static partial void QuestGiverStatusMultipleEntry(
        ILogger logger, string path, int index, ulong guidLow, ulong guidHigh, uint entry,
        QuestGiverStatusModern status, uint encoded);

    [LoggerMessage(
        EventId = 1603,
        Level = LogLevel.Information,
        Message = "[QuestItemCredit] quest={QuestId} item={ItemId} have={Have}/{Required}")]
    public static partial void QuestItemCredit(
        ILogger logger, uint questId, uint itemId, uint have, ushort required);
}
