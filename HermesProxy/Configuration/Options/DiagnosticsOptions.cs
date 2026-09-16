namespace HermesProxy.Configuration.Options;

public sealed class DiagnosticsOptions
{
    public bool PacketsLog { get; set; } = true;

    public bool EnableMetrics { get; set; }

    /// <summary>
    /// Seconds between metrics summaries, and the length of the interval the summary's
    /// per-interval columns (packet rates, GC delta, IntMax) cover. Short intervals isolate a
    /// burst from the quiet traffic around it; long ones keep the log small.
    /// </summary>
    public int MetricsIntervalSeconds { get; set; } = 60;

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
