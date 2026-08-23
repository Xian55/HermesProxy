namespace HermesProxy.Configuration.Options;

public sealed class DiagnosticsOptions
{
    public bool PacketsLog { get; set; } = true;

    public bool EnableMetrics { get; set; }

    public bool EnableVersionCheck { get; set; } = true;

    /// <summary>
    /// Forward legacy transport CreateObjects to a V3_4_3 client instead of filtering
    /// them — both TRANSPORT (type 11: elevators, subway cars, ICC sleds) and
    /// MO_TRANSPORT (type 15: zeppelins, boats). See upstream issue #96.
    /// Creates carrying a placeholder position are still filtered regardless, which is
    /// what the original blanket filter was protecting against. Set to false to restore
    /// the old behaviour of hiding transports entirely.
    /// </summary>
    public bool ForwardTransportsV343 { get; set; } = true;
}
