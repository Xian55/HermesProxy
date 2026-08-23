namespace HermesProxy.Configuration.Options;

public sealed class DiagnosticsOptions
{
    public bool PacketsLog { get; set; } = true;

    public bool EnableMetrics { get; set; }

    public bool EnableVersionCheck { get; set; } = true;

    /// <summary>
    /// Forward legacy transport CreateObjects to a V3_4_3 client instead of filtering
    /// them — both TRANSPORT (type 11: elevators, subway cars, ICC sleds) and
    /// MO_TRANSPORT (type 15: zeppelins, boats). Off by default: the filter was added
    /// because creates with a placeholder position caused a CMSG_OBJECT_UPDATE_FAILED
    /// retry loop that blocked the loading screen. See upstream issue #96.
    /// </summary>
    public bool ForwardTransportsV343 { get; set; }
}
