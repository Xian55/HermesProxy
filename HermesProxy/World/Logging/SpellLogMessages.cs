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
}
