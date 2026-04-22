namespace HermesProxy.Configuration.Options;

public sealed class DiagnosticsOptions
{
    public bool PacketsLog { get; set; } = true;

    public bool EnableMetrics { get; set; }

    public bool EnableVersionCheck { get; set; } = true;
}
