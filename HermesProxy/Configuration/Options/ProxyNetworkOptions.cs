namespace HermesProxy.Configuration.Options;

public sealed class ProxyNetworkOptions
{
    public string ExternalAddress { get; set; } = "127.0.0.1";

    public int RestPort { get; set; } = 8081;

    public int BNetPort { get; set; } = 1119;

    public int RealmPort { get; set; } = 8084;

    public int InstancePort { get; set; } = 8086;
}
