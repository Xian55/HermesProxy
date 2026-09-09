using HermesProxy.World.Enums;

using Microsoft.Extensions.Logging;

namespace HermesProxy.World.Logging;

/// <summary>
/// Source-generated logging for the spell / cast translation path.
///
/// EventId 300-399 is reserved for this file (100-199 WorldSocket dispatch, 200-299
/// WorldClient dispatch, 900-909 object lifecycle).
///
/// All Trace level, so they cost nothing unless Log.Server.MinimumLevel=Verbose.
/// </summary>
internal static partial class SpellLogMessages
{
    [LoggerMessage(
        EventId = 300,
        Level = LogLevel.Trace,
        Message = "[SpellCooldown] synthesized from legacy item template itemId={ItemId} spellId={SpellId} cooldownMs={CooldownMs}")]
    public static partial void ItemCooldownSynthesized(
        ILogger logger, uint itemId, uint spellId, int cooldownMs);

    // 300-315 are taken by AuthClientLogMessages despite this file claiming 300-399, so new
    // ids here start at 316. Everything below fires per aura / health / power packet.
    [LoggerMessage(
        EventId = 316,
        Level = LogLevel.Trace,
        Message = "[AuraUpdateTrace] guidLow={GuidLow} guidHigh={GuidHigh} isAll={IsAll} " +
                  "isPlayer={IsPlayer} incomingBytes={IncomingBytes} aurasShipped={AurasShipped} " +
                  "trackedTotal={TrackedTotal} dedupHit={DedupHit}")]
    public static partial void AuraUpdate(
        ILogger logger, ulong guidLow, ulong guidHigh, bool isAll, bool isPlayer,
        uint incomingBytes, int aurasShipped, int trackedTotal, bool dedupHit);

    [LoggerMessage(
        EventId = 317,
        Level = LogLevel.Trace,
        Message = "[AuraDedup] skipped no-op resync guidLow={GuidLow} slots={Slots}")]
    public static partial void AuraDedupSkipped(ILogger logger, ulong guidLow, int slots);

    [LoggerMessage(
        EventId = 318,
        Level = LogLevel.Trace,
        Message = "[HealthUpdateTrace] guidLow={GuidLow} health={Health}")]
    public static partial void HealthUpdate(ILogger logger, ulong guidLow, uint health);

    [LoggerMessage(
        EventId = 319,
        Level = LogLevel.Trace,
        Message = "[PowerUpdateTrace] guidLow={GuidLow} type={PowerType} power={Power}")]
    public static partial void PowerUpdate(
        ILogger logger, ulong guidLow, PowerType powerType, int power);

    // 320-321 trace the client -> legacy cast translation as a pair: what the proxy actually
    // put on the wire, and what the legacy server objected to. Neither is visible otherwise —
    // in the opcode log a rejected cast looks exactly like a dropped one, and the failure
    // reason never appears at all. Issue #269 was diagnosed with these two alone.
    [LoggerMessage(
        EventId = 320,
        Level = LogLevel.Trace,
        Message = "[CastForwardTrace][C P>S] modernSpellId={ModernSpellId} legacySpellId={LegacySpellId} " +
                  "modernFlags=0x{ModernFlags:X} legacyFlags=0x{LegacyFlags:X} legacyTarget=0x{LegacyTarget:X}")]
    public static partial void LegacyCastForwarded(
        ILogger logger, uint modernSpellId, uint legacySpellId,
        uint modernFlags, uint legacyFlags, ulong legacyTarget);

    // Reason is the raw ordinal from the legacy build's own SpellCastResult, logged before
    // ConvertSpellCastResult remaps it onto the modern enum — decode it against that build's
    // SharedDefines.h, not against the modern numbering.
    [LoggerMessage(
        EventId = 321,
        Level = LogLevel.Trace,
        Message = "[CastFailedTrace][C P<S] legacySpellId={SpellId} legacyReason={Reason} " +
                  "arg1={Arg1} arg2={Arg2}")]
    public static partial void LegacyCastFailed(
        ILogger logger, uint spellId, uint reason, int arg1, int arg2);
}
