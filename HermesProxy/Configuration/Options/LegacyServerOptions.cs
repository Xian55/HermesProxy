using HermesProxy.Enums;

namespace HermesProxy.Configuration.Options;

public sealed class LegacyServerOptions
{
    public string Build { get; set; } = "auto";

    public string Address { get; set; } = "127.0.0.1";

    public int Port { get; set; } = 3724;

    /// <summary>
    /// The legacy account's password, for clients that log in with SRP (Cataclysm Classic 4.4.x)
    /// and so never send theirs. Set it through the HERMES_LegacyServerOptions__Password
    /// environment variable, or leave it empty to be asked at startup; never in a config file.
    /// </summary>
    public string Password { get; set; } = "";

    // Populated post-bind by LegacyServerBuildResolver. If Build == "auto",
    // resolves via VersionChecker.GetBestLegacyVersion(ClientOptions.ClientBuild);
    // otherwise parses Build as an enum member.
    public ClientVersionBuild ResolvedBuild { get; set; }
}
