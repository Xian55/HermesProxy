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
}
