using HermesProxy.Configuration.Options;
using Xunit;

namespace HermesProxy.Tests.Configuration;

public sealed class SetSeedOptionTests
{
    private const string Seed = "00112233445566778899AABBCCDDEEFF";

    [Theory]
    [InlineData("SeedHex")]
    [InlineData("ClientSeed")]
    public void PreprocessArgs_SetSeed_TargetsClientSeedHex(string key)
    {
        var args = Program.PreprocessArgs(["--set", $"{key}={Seed}"], out _);

        Assert.Equal($"--{nameof(ClientOptions)}:{nameof(ClientOptions.SeedHex)}={Seed}", Assert.Single(args));
    }
}
