using HermesProxy.Enums;

namespace HermesProxy.Configuration.Options;

[ConfigSection(2)]
public sealed class LegacyServerOptions
{
    [Setting("The legacy server's build: auto, V1_12_1_5875, V2_4_3_8606, V3_3_5a_12340 or V4_3_4_15595. auto takes the best match for the client.")]
    public string Build { get; set; } = "auto";

    [Setting("Address of the legacy auth server, what you would put in SET REALMLIST.")]
    public string Address { get; set; } = "127.0.0.1";

    [Setting("Port of the legacy auth server.")]
    public int Port { get; set; } = 3724;

    [Setting("The legacy account's password, needed only by clients that log in with SRP (Cataclysm Classic 4.4.x), which never send theirs. Set it in the environment, never in a file. Empty means the proxy asks at startup.")]
    public string Password { get; set; } = "";

    // Populated post-bind by LegacyServerBuildResolver. If Build == "auto",
    // resolves via VersionChecker.GetBestLegacyVersion(ClientOptions.ClientBuild);
    // otherwise parses Build as an enum member.
    public ClientVersionBuild ResolvedBuild { get; internal set; }
}
