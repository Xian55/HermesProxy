using HermesProxy.Enums;

using Microsoft.Extensions.Logging;

namespace HermesProxy.World;

#pragma warning disable SYSLIB1015
internal static partial class GameDataLogMessages
{
    // EventId 700-799 range is reserved for GameData.

    [LoggerMessage(EventId = 700, Level = LogLevel.Information, Message = "Loading data files...")]
    public static partial void LoadingDataFiles(ILogger logger, string SourceFile, string NetDir);

    [LoggerMessage(EventId = 701, Level = LogLevel.Information, Message = "Finished loading data. Time taken: {Milliseconds} ms")]
    public static partial void FinishedLoadingData(
        ILogger logger, string SourceFile, string NetDir, double Milliseconds);

    // 702-704: ItemSparse hotfix wire-size tracing. These fire once per item hotfix build and
    // were interpolated Log.Print(LogType.Trace, $"...") calls, which formatted unconditionally.
    // Pass the Server-category logger — LogType.Trace routes to Server at Verbose, so using the
    // Storage logger here would put them behind a different level switch.

    // Build renders quoted (build="V3_4_3_54261") because it is a structured property rather
    // than pre-interpolated text. A :l specifier would strip the quotes but throws
    // FormatException on an enum via IFormattable, so the quotes stay.
    [LoggerMessage(EventId = 702, Level = LogLevel.Trace,
        Message = "[ItemSparseHotfix] item={ItemId} preScalingSize={PreScalingSize} build={Build}")]
    public static partial void ItemSparseHotfixPreScaling(
        ILogger logger, string SourceFile, string NetDir, int ItemId, uint PreScalingSize, ClientVersionBuild Build);

    [LoggerMessage(EventId = 703, Level = LogLevel.Trace,
        Message = "[ItemSparseHotfix] item={ItemId} taking V3_4_3 path")]
    public static partial void ItemSparseHotfixV343Path(
        ILogger logger, string SourceFile, string NetDir, int ItemId);

    [LoggerMessage(EventId = 704, Level = LogLevel.Trace,
        Message = "[ItemSparseHotfixSize] item={ItemId} buildTotal={BuildTotal}")]
    public static partial void ItemSparseHotfixSize(
        ILogger logger, string SourceFile, string NetDir, int ItemId, uint BuildTotal);
}
