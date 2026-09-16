using Microsoft.Extensions.Logging;

namespace HermesProxy.World.Logging;

/// <summary>
/// Source-generated logging for the object lifecycle as the client sees it: which guids we
/// register as created, which Values deltas we forward or strip, and when a guid stops being
/// known. Added because the V3_4_3 client answers CMSG_OBJECT_UPDATE_FAILED once per corpse
/// while the corpse Values deltas are being forwarded, and the existing traces cover only the
/// Values path — creates and destroys were invisible, so the ordering could not be read off a
/// log and was being guessed at instead.
///
/// Guids are logged as their two raw halves rather than as a WowGuid128. The record struct's
/// generated ToString allocates, and these fire per object per batch. Grep a single object's
/// history with the Low value.
///
/// All Trace level, so they cost nothing unless Log.Server.MinimumLevel=Verbose (which
/// test-loop2.ps1 sets). EventId 900-919 is reserved for this file.
/// </summary>
internal static partial class ObjectLifecycleLogMessages
{
    [LoggerMessage(
        EventId = 900,
        Level = LogLevel.Trace,
        Message = "[ObjLife] create registered guidLow={GuidLow} guidHigh={GuidHigh} updateType={UpdateType}")]
    public static partial void CreateRegistered(
        ILogger logger, ulong guidLow, ulong guidHigh, string updateType);

    [LoggerMessage(
        EventId = 901,
        Level = LogLevel.Trace,
        Message = "[ObjLife] values stripped guidLow={GuidLow} guidHigh={GuidHigh} reason={Reason}")]
    public static partial void ValuesStripped(
        ILogger logger, ulong guidLow, ulong guidHigh, string reason);

    [LoggerMessage(
        EventId = 902,
        Level = LogLevel.Trace,
        Message = "[ObjLife] values forwarded guidLow={GuidLow} guidHigh={GuidHigh} hasCorpse={HasCorpse} hasDynObj={HasDynObj}")]
    public static partial void ValuesForwarded(
        ILogger logger, ulong guidLow, ulong guidHigh, bool hasCorpse, bool hasDynObj);

    [LoggerMessage(
        EventId = 903,
        Level = LogLevel.Trace,
        Message = "[ObjLife] guid no longer known guidLow={GuidLow} guidHigh={GuidHigh} cause={Cause} wasKnown={WasKnown}")]
    public static partial void KnownGuidRemoved(
        ILogger logger, ulong guidLow, ulong guidHigh, string cause, bool wasKnown);

    [LoggerMessage(
        EventId = 904,
        Level = LogLevel.Trace,
        Message = "[ObjLife] toys sync deferred guidLow={GuidLow} guidHigh={GuidHigh} (player CreateObject not yet delivered)")]
    public static partial void ToysDeferred(
        ILogger logger, ulong guidLow, ulong guidHigh);

    [LoggerMessage(
        EventId = 905,
        Level = LogLevel.Trace,
        Message = "[ObjLife] toys sync flushed guidLow={GuidLow} guidHigh={GuidHigh} after player CreateObject")]
    public static partial void ToysFlushed(
        ILogger logger, ulong guidLow, ulong guidHigh);

    [LoggerMessage(
        EventId = 906,
        Level = LogLevel.Trace,
        Message = "[ObjLife] corpse destroy deferred guidLow={GuidLow} guidHigh={GuidHigh} outboxHolds={OutboxHolds}")]
    public static partial void CorpseDestroyDeferred(
        ILogger logger, ulong guidLow, ulong guidHigh, int outboxHolds);

    [LoggerMessage(
        EventId = 907,
        Level = LogLevel.Trace,
        Message = "[ObjLife] corpse recreate skipped guidLow={GuidLow} guidHigh={GuidHigh} reason={Reason}")]
    public static partial void CorpseRecreateSkipped(
        ILogger logger, ulong guidLow, ulong guidHigh, string reason);

    [LoggerMessage(
        EventId = 908,
        Level = LogLevel.Trace,
        Message = "[ObjLife] player values held guidLow={GuidLow} guidHigh={GuidHigh} entries={Entries} reason={Reason}")]
    public static partial void PlayerValuesHeld(
        ILogger logger, ulong guidLow, ulong guidHigh, int entries, string reason);

    [LoggerMessage(
        EventId = 909,
        Level = LogLevel.Trace,
        Message = "[ObjLife] player movement held guidLow={GuidLow} guidHigh={GuidHigh} opcode={Opcode} reason={Reason}")]
    public static partial void PlayerMovementHeld(
        ILogger logger, ulong guidLow, ulong guidHigh, string opcode, string reason);

    [LoggerMessage(
        EventId = 910,
        Level = LogLevel.Trace,
        Message = "[ObjLife] sent while client has no player object opcode={Opcode} playerLow={PlayerLow}")]
    public static partial void SentWhileClientHasNoPlayer(
        ILogger logger, string opcode, ulong playerLow);

    [LoggerMessage(
        EventId = 911,
        Level = LogLevel.Trace,
        Message = "[ObjLife] sent while client has no pet object opcode={Opcode} petLow={PetLow} sinceSummonMs={SinceSummonMs}")]
    public static partial void SentWhileClientHasNoPet(
        ILogger logger, string opcode, ulong petLow, long sinceSummonMs);
}
