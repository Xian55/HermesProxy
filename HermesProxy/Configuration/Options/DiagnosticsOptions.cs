namespace HermesProxy.Configuration.Options;

[ConfigSection(5)]
public sealed class DiagnosticsOptions
{
    [Setting("Dump each session's raw packets to PacketsLog/*.pkt for replay and inspection. PacketsLogRetentionDays limits how long they stay.")]
    public bool PacketsLog { get; set; } = true;

    [Setting("Days to keep the captures the proxy wrote. Older ones are deleted at startup and every hour. 0 keeps them forever (0-3650).")]
    public int PacketsLogRetentionDays { get; set; } = 30;

    [Setting("Collect per-opcode latency and allocation metrics, as --metrics does.")]
    public bool EnableMetrics { get; set; }

    /// <summary>
    /// Seconds between metrics summaries, and the length of the interval the summary's
    /// per-interval columns (packet rates, GC delta, IntMax) cover. Short intervals isolate a
    /// burst from the quiet traffic around it; long ones keep the log small.
    /// </summary>
    [Setting("Seconds between metrics summaries (1-3600), as --metrics-interval sets them.")]
    public int MetricsIntervalSeconds { get; set; } = 60;

    [Setting("Check for a newer version at startup.")]
    public bool EnableVersionCheck { get; set; } = true;

    /// <summary>
    /// Forward legacy transport CreateObjects to a V3_4_3 client instead of filtering
    /// them — both TRANSPORT (type 11: elevators, subway cars, ICC sleds) and
    /// MO_TRANSPORT (type 15: zeppelins, boats). See upstream issue #96.
    /// Creates carrying a placeholder position are still filtered regardless, which is
    /// what the original blanket filter was protecting against. Set to false to restore
    /// the old behaviour of hiding transports entirely.
    /// </summary>
    [Setting("Forward legacy transports (elevators, zeppelins, boats) to a 3.4.3 client. false hides them.")]
    public bool ForwardTransportsV343 { get; set; } = true;
}
