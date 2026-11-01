using HermesProxy.Enums;

namespace HermesProxy.Configuration.Options;

[ConfigSection(1)]
public sealed class ClientOptions
{
    [Setting("The game client's build: V1_14_0_40618, V1_14_1_41794, V1_14_2_42597, V2_5_2_40892, V2_5_3_42328, V3_4_3_54261 (beta) or V4_4_2_60895 (preview).")]
    public ClientVersionBuild ClientBuild { get; set; } = ClientVersionBuild.V2_5_2_40892;

    [Setting("The client seed, as 32 hex characters (16 bytes). It has to match the seed your launcher uses.")]
    public string SeedHex { get; set; } = "179D3DC3235629D07113A9B3867F97A7";

    [Setting("The OS reported to the legacy server, such as OSX or Win. Some servers only accept Win.")]
    public string ReportedOS { get; set; } = "OSX";

    [Setting("The platform reported to the legacy server: x86 or x64.")]
    public string ReportedPlatform { get; set; } = "x86";

    // Populated post-bind from SeedHex by ClientSeedParser.
    public byte[] ClientSeed { get; internal set; } = [];
}
