using System.Net;

namespace HermesProxy.Configuration.Options;

[ConfigSection(3)]
public sealed class ProxyNetworkOptions
{
    [Setting("The IP address clients use to reach the proxy. It is sent to them as the address to connect to, in the realm address, the login page and SMSG_CONNECT_TO, so it cannot be 0.0.0.0 or a host name.")]
    public string ExternalAddress { get; set; } = "127.0.0.1";

    [Setting("The IPv4 address the proxy listens on, 0.0.0.0 for every interface. Unset: loopback when ExternalAddress is loopback, otherwise every interface. The Docker image sets 0.0.0.0.")]
    public string? BindAddress { get; set; }

    [Setting("Port of the REST login server.")]
    public int RestPort { get; set; } = 8081;

    [Setting("Port of the BNet server. Any other than 1119 also goes in the portal in Config.wtf.")]
    public int BNetPort { get; set; } = 1119;

    [Setting("Port of the realm server.")]
    public int RealmPort { get; set; } = 8084;

    [Setting("Port of the instance server.")]
    public int InstancePort { get; set; } = 8086;

    [Setting("Path to a PKCS#12 (.pfx) certificate served on BNetPort and RestPort. Unset: the embedded TrinityCore-compatible certificate. See docs/tls-certificate.md.")]
    public string? CertificatePfxPath { get; set; }

    [Setting("Password for CertificatePfxPath. Leave it unset for a passwordless pfx.")]
    public string? CertificatePfxPassword { get; set; }

    public IPAddress ResolveBindAddress()
    {
        if (!string.IsNullOrEmpty(BindAddress))
            return IPAddress.Parse(BindAddress);

        return IPAddress.IsLoopback(IPAddress.Parse(ExternalAddress)) ? IPAddress.Loopback : IPAddress.Any;
    }
}
