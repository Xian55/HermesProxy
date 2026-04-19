using Microsoft.Extensions.Logging;

// Kept in the global namespace to match RealmManager/Realm which also live there —
// introducing a HermesProxy.Realm.* namespace would shadow the Realm type for callers.

/// <summary>
/// Source-generated logging methods for <see cref="RealmManager"/>.
/// </summary>
#pragma warning disable SYSLIB1015
internal static partial class RealmManagerLogMessages
{
    // EventId 400-499 range is reserved for RealmManager.

    [LoggerMessage(
        EventId = 400,
        Level = LogLevel.Information,
        Message = "Added realm \"{Name}\" at {Address}:{Port}")]
    public static partial void AddedRealm(
        ILogger logger,
        string SourceFile,
        string NetDir,
        string Name,
        string Address,
        int Port);

    [LoggerMessage(
        EventId = 401,
        Level = LogLevel.Information,
        Message = "Updating realm \"{Name}\" at {Address}:{Port}")]
    public static partial void UpdatingRealm(
        ILogger logger,
        string SourceFile,
        string NetDir,
        string Name,
        string Address,
        int Port);

    [LoggerMessage(
        EventId = 402,
        Level = LogLevel.Debug,
        Message = "{Line}")]
    public static partial void RealmListLine(
        ILogger logger,
        string SourceFile,
        string NetDir,
        string Line);
}
