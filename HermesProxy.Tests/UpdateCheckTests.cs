using Xunit;

namespace HermesProxy.Tests;

public sealed class UpdateCheckTests
{
    [Theory]
    [InlineData("v4.7.1", "4.7.0", true)]
    [InlineData("v4.8.0", "4.7.9", true)]
    [InlineData("v5.0.0", "4.7.0", true)]
    [InlineData("v4.10.0", "4.9.0", true)]      // numbers, not text
    [InlineData("v4.7.0", "4.7.0", false)]
    [InlineData("v4.6.113", "4.7.0", false)]
    [InlineData("v3.10", "4.7.0", false)]       // the old repository's last release
    public void IsNewerRelease_ComparesTheTagWithThisBuild(string tag, string current, bool expected)
    {
        Assert.Equal(expected, Server.IsNewerRelease(tag, current));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("nightly")]
    public void IsNewerRelease_TagIsNoVersion_IsNotNewer(string? tag)
    {
        Assert.False(Server.IsNewerRelease(tag, "4.7.0"));
    }
}
