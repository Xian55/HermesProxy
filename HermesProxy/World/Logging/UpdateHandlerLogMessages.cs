using HermesProxy.World.Enums;
using Microsoft.Extensions.Logging;

namespace HermesProxy.World.Logging;

/// <summary>
/// Source-generated logging for the legacy SMSG_UPDATE_OBJECT read path. Split out of
/// <see cref="ObjectLifecycleLogMessages"/> because these fire per Values block rather than
/// per object batch: in a bot-populated battleground the Values trace alone ran on every one
/// of ~600 packets a second, and as an interpolated <c>Log.Print</c> it built its string, and
/// scanned six inventory arrays to fill it, whether or not Verbose was on.
///
/// Guids are logged as their two raw halves. WowGuid128's generated ToString allocates and
/// these sites are per packet; grep a single object's history with the Low value.
///
/// EventId 1200-1219 is reserved for this file (900-909 object lifecycle, 1100-1114
/// gameobject fields, 1000-1007 battleground).
/// </summary>
internal static partial class UpdateHandlerLogMessages
{
    [LoggerMessage(
        EventId = 1200,
        Level = LogLevel.Trace,
        Message = "[UpdateValuesTrace][in] i={Index} guidLow={GuidLow} guidHigh={GuidHigh} " +
                  "legacyHigh={LegacyHigh} legacyEntry={LegacyEntry} legacyCounter={LegacyCounter} " +
                  "isPlayer={IsPlayer} hasObj={HasObjectField} hasUnit={HasUnit} " +
                  "unitAnyField={UnitAnyField} hp={Health} maxHp={MaxHealth} flags=0x{Flags:X} " +
                  "hasPlayer={HasPlayer} playerAnyField={PlayerAnyField} " +
                  "hasActive={HasActive} activeAnyField={ActiveAnyField} " +
                  "auras={AuraCount} powers={PowerCount}")]
    public static partial void ValuesUpdateIn(
        ILogger logger, int index, ulong guidLow, ulong guidHigh,
        HighGuidTypeLegacy legacyHigh, uint legacyEntry, ulong legacyCounter,
        bool isPlayer, bool hasObjectField, bool hasUnit, bool unitAnyField,
        long? health, long? maxHealth, uint flags,
        bool hasPlayer, bool playerAnyField,
        bool hasActive, bool activeAnyField,
        int auraCount, int powerCount);

    [LoggerMessage(
        EventId = 1201,
        Level = LogLevel.Debug,
        Message = "[V343Trace][InvSlot] player slot={Slot} guidLow={GuidLow} guidHigh={GuidHigh}")]
    public static partial void OwnerInvSlot(
        ILogger logger, int slot, ulong guidLow, ulong guidHigh);

    // The five below were interpolated Log.Print(Trace) calls, built for every GameObject create,
    // every GameObject dynamic-flags change and every uncompressed SMSG_UPDATE_OBJECT whether or
    // not Verbose was on. Callers still gate on Log.IsTraceEnabled where an argument costs
    // something to compute.

    [LoggerMessage(
        EventId = 1202,
        Level = LogLevel.Trace,
        Message = "[UpdateObjectTrace][C P<S] SMSG_UPDATE_OBJECT rawBytes={RawBytes} numObjUpdates={NumObjUpdates} " +
                  "hasTransport={HasTransport} firstUpdateType={FirstUpdateType} headHex={HeadHex}")]
    public static partial void UpdateObjectEnvelopeIn(
        ILogger logger, int rawBytes, uint numObjUpdates, byte hasTransport, string firstUpdateType, string headHex);

    [LoggerMessage(
        EventId = 1203,
        Level = LogLevel.Trace,
        Message = "{Action} {CreateType} for {LegacyHigh} for V3_4_3 guidLow={GuidLow} guidHigh={GuidHigh} entryID={EntryId}.")]
    public static partial void TransportCreate(
        ILogger logger, string action, string createType, HighGuidTypeLegacy legacyHigh,
        ulong guidLow, ulong guidHigh, int? entryId);

    [LoggerMessage(
        EventId = 1204,
        Level = LogLevel.Trace,
        Message = "Forwarding {CreateType} for GameObject guidLow={GuidLow} guidHigh={GuidHigh} entryID={EntryId} " +
                  "typeID={TypeId} state={State} rot=({RotX:F3},{RotY:F3},{RotZ:F3},{RotW:F3}).")]
    public static partial void GameObjectCreate(
        ILogger logger, string createType, ulong guidLow, ulong guidHigh, int? entryId, sbyte? typeId, sbyte? state,
        float? rotX, float? rotY, float? rotZ, float? rotW);

    [LoggerMessage(
        EventId = 1205,
        Level = LogLevel.Trace,
        Message = "[ItemContainerTrace] Forwarding {CreateType} for 0x4700 ItemContainer guidLow={GuidLow} guidHigh={GuidHigh} entryID={EntryId}.")]
    public static partial void ItemContainerCreate(
        ILogger logger, string createType, ulong guidLow, ulong guidHigh, int? entryId);

    [LoggerMessage(
        EventId = 1206,
        Level = LogLevel.Trace,
        Message = "[Trace][GO DYN_FLAGS] guidLow={GuidLow} guidHigh={GuidHigh} entry={Entry} legacyRaw=0x{LegacyRaw:X8} " +
                  "effective=0x{EffectiveLegacyRaw:X8} ({Flags}) -> modernLow=0x{ModernLow:X8} high=0x{PreservedHigh:X8}, " +
                  "oldDyn=0x{OldValue:X8} oldDynSource={OldDynSource}, finalDyn=0x{FinalDyn:X8}")]
    public static partial void GameObjectDynamicFlags(
        ILogger logger, ulong guidLow, ulong guidHigh, int? entry, uint legacyRaw, uint effectiveLegacyRaw,
        GameObjectDynamicFlagsLegacy flags, uint modernLow, uint preservedHigh, uint oldValue, string oldDynSource,
        uint finalDyn);

    /// <summary>
    /// The object type is logged as its numeric value: this fires for types outside the set the
    /// writers know, where the enum may have no name to render.
    /// </summary>
    [LoggerMessage(
        EventId = 1207,
        Level = LogLevel.Warning,
        Message = "Dropped a {BlockType} block for guidLow={GuidLow} guidHigh={GuidHigh}: object type {ObjectType} " +
                  "is not one the update writer can build.")]
    public static partial void UnwritableObjectTypeDropped(
        ILogger logger, string blockType, ulong guidLow, ulong guidHigh, int objectType);
}
