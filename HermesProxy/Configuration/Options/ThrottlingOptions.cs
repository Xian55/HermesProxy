namespace HermesProxy.Configuration.Options;

/// <summary>
/// Rate limits applied to high-frequency legacy traffic before it is forwarded to the modern
/// client. These exist because the V3_4_3 client has a per-packet allocation budget that a
/// busy world or a bot-filled battleground can exhaust, and because raid frames and mob
/// movement do not need updates at the rate a 3.3.5a server emits them.
///
/// Every value is in milliseconds and can be set to 0 to disable that throttle entirely.
/// </summary>
[ConfigSection(6)]
public sealed class ThrottlingOptions
{
    /// <summary>
    /// Minimum gap between forwarded party-member state updates for the same member, when the
    /// update carries only high-frequency data (health, power, position). Updates carrying
    /// status, level, spec, zone, auras, vehicle seat or power type — and any update reporting
    /// a death — are always forwarded immediately.
    ///
    /// Measured on AzerothCore with mod-playerbots: a 15v15 battleground produced 1,608
    /// SMSG_PARTY_MEMBER_PARTIAL_STATE per second, roughly 107 per raid member per second.
    /// </summary>
    [Setting("Minimum milliseconds between forwarded party-member updates that carry only health, power or position. 0 turns the throttle off.")]
    public int PartyMemberStateMinIntervalMs { get; set; } = 200;
}
