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
}
