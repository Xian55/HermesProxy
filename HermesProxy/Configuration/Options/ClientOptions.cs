using HermesProxy.Enums;

namespace HermesProxy.Configuration.Options;

public sealed class ClientOptions
{
    public ClientVersionBuild ClientBuild { get; set; } = ClientVersionBuild.V2_5_2_40892;

    public string SeedHex { get; set; } = "179D3DC3235629D07113A9B3867F97A7";

    public string ReportedOS { get; set; } = "OSX";

    public string ReportedPlatform { get; set; } = "x86";

    // Populated post-bind from SeedHex by ClientSeedParser.
    public byte[] ClientSeed { get; set; } = [];
}
