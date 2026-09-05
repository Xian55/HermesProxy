using HermesProxy.World.Enums;

using Microsoft.Extensions.Logging;

namespace HermesProxy.World.Logging;

/// <summary>
/// Source-generated logging for the GameObject field translation, emitted once per object per
/// SMSG_UPDATE_OBJECT after every field has been translated — for creates and for Values
/// deltas alike.
///
/// Added while chasing issue #184, where Strand of the Ancients and Wintergrasp destructible
/// buildings were targetable but never drawn. Every existing trace covered the create path
/// only, so a create whose fields matched a native 3.4.3 capture byte for byte looked correct
/// in the log while a Values delta one packet later silently republished DynamicFlags as 0 and
/// undid it. Reading the two against each other needs both sides logged in the same shape,
/// which is what this file is for: diff a guid's create line against its Values lines and the
/// field that moves is the one to look at.
///
/// Guids are logged as their two raw halves rather than as a WowGuid128, matching
/// <see cref="ObjectLifecycleLogMessages"/>. The record struct's generated ToString allocates
/// and these fire per object per batch. Grep one object's whole history with its Low value.
///
/// Nullable fields are logged as -1 when absent, so "did not publish" is distinguishable from
/// "published zero" — that distinction is the entire point of the file, since DynamicFlags=0
/// and DynamicFlags absent produce very different client behaviour.
///
/// All Trace level, so they cost nothing unless Log.Server.MinimumLevel=Verbose (which
/// test-loop2.ps1 sets). EventId 1100-1109 is reserved for this file.
/// </summary>
internal static partial class GameObjectFieldLogMessages
{
    [LoggerMessage(
        EventId = 1100,
        Level = LogLevel.Trace,
        Message = "[GoLifecycle] {UpdateType} guidLow={GuidLow} entry={Entry} typeId={TypeId} " +
                  "dynFlags=0x{DynamicFlags:X8} flags=0x{Flags:X8} displayId={DisplayId} " +
                  "state={State} pctHealth={PercentHealth} " +
                  "parentRot=({ParentRotX},{ParentRotY},{ParentRotZ},{ParentRotW}) " +
                  "faction={FactionTemplate} level={Level}")]
    public static partial void GameObjectFieldsPublished(
        ILogger logger,
        string updateType,
        ulong guidLow,
        uint entry,
        int typeId,
        uint dynamicFlags,
        uint flags,
        int displayId,
        int state,
        int percentHealth,
        float parentRotX,
        float parentRotY,
        float parentRotZ,
        float parentRotW,
        int factionTemplate,
        int level);

    // The two below bracket the *incoming* legacy values-update for a GameObject, before any
    // translation. GameObjectFieldsPublished above records what we ended up sending; these
    // record what arrived, so a field that the handler never reads shows up as a gap between
    // the two rather than as silence. Both fire per GameObject per SMSG_UPDATE_OBJECT, so the
    // caller must keep them inside an IsEnabled block -- see the note in UpdateHandler.
    [LoggerMessage(
        EventId = 1101,
        Level = LogLevel.Trace,
        Message = "[GoIngest] enter guidLow={GuidLow} guidHigh={GuidHigh} entry={Entry} " +
                  "dynFlagsBefore={DynamicFlagsBefore} isTransport={IsTransport}")]
    public static partial void GameObjectIngestEnter(
        ILogger logger, ulong guidLow, ulong guidHigh, int entry, long dynamicFlagsBefore, bool isTransport);

    [LoggerMessage(
        EventId = 1102,
        Level = LogLevel.Trace,
        Message = "[GoIngest] field guidLow={GuidLow} {Field}@{Index} u32=0x{Raw:X8} i32={Signed} f32={Value}")]
    public static partial void GameObjectFieldIngested(
        ILogger logger, ulong guidLow, GameObjectField field, int index, uint raw, int signed, float value);
}
