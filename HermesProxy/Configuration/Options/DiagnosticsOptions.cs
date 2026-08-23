namespace HermesProxy.Configuration.Options;

public sealed class DiagnosticsOptions
{
    public bool PacketsLog { get; set; } = true;

    public bool EnableMetrics { get; set; }

    public bool EnableVersionCheck { get; set; } = true;

    /// <summary>
    /// Forward legacy TRANSPORT (gameobject type 11: elevators, subway cars, ICC sleds)
    /// CreateObjects to a V3_4_3 client instead of filtering them. Off by default: the
    /// filter was added because these creates caused a CMSG_OBJECT_UPDATE_FAILED retry
    /// loop that blocked the loading screen. MO_TRANSPORT (type 15: zeppelins, boats) is
    /// never forwarded regardless, since those entries are absent from the client's
    /// TransportAnimation data. See upstream issue #96.
    /// </summary>
    public bool ForwardTransportsV343 { get; set; }
}
