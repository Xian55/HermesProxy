using HermesProxy.Enums;

namespace HermesProxy.Configuration.Options;

public sealed class LegacyServerOptions
{
    public string Build { get; set; } = "auto";

    public string Address { get; set; } = "127.0.0.1";

    public int Port { get; set; } = 3724;

    // Populated post-bind by LegacyServerBuildResolver. If Build == "auto",
    // resolves via VersionChecker.GetBestLegacyVersion(ClientOptions.ClientBuild);
    // otherwise parses Build as an enum member.
    public ClientVersionBuild ResolvedBuild { get; set; }
}
