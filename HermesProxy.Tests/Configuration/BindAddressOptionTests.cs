using System.Net;
using HermesProxy.Configuration.Options;
using Xunit;

namespace HermesProxy.Tests.Configuration;

public sealed class BindAddressOptionTests
{
    [Theory]
    [InlineData("127.0.0.1", null, "127.0.0.1")]
    [InlineData("127.0.0.5", null, "127.0.0.1")]
    [InlineData("::1", null, "127.0.0.1")]
    [InlineData("192.168.1.50", null, "0.0.0.0")]
    [InlineData("2001:db8::1", null, "0.0.0.0")]
    [InlineData("127.0.0.1", "", "127.0.0.1")]
    [InlineData("127.0.0.1", "0.0.0.0", "0.0.0.0")]
    [InlineData("192.168.1.50", "127.0.0.1", "127.0.0.1")]
    [InlineData("192.168.1.50", "10.0.0.7", "10.0.0.7")]
    public void ResolveBindAddress_FollowsBindAddressElseExternalAddress(string external, string? bind, string expected)
    {
        var options = new ProxyNetworkOptions { ExternalAddress = external, BindAddress = bind };

        Assert.Equal(IPAddress.Parse(expected), options.ResolveBindAddress());
    }

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("172.18.0.1")]
    [InlineData("2001:db8::1")]
    public void Validator_AcceptsAddressesAClientCanReach(string external)
    {
        var result = new ProxyNetworkOptionsValidator().Validate(null, new ProxyNetworkOptions { ExternalAddress = external });

        Assert.True(result.Succeeded, result.FailureMessage);
    }

    [Theory]
    [InlineData("0.0.0.0")]
    [InlineData("::")]
    [InlineData("example.com")]
    [InlineData("localhost")]
    [InlineData("")]
    public void Validator_RefusesAddressesAClientCannotBeSent(string external)
    {
        var result = new ProxyNetworkOptionsValidator().Validate(null, new ProxyNetworkOptions { ExternalAddress = external });

        Assert.False(result.Succeeded);
        Assert.Contains(nameof(ProxyNetworkOptions.ExternalAddress), result.FailureMessage);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("0.0.0.0", true)]
    [InlineData("127.0.0.1", true)]
    [InlineData("::", false)]
    [InlineData("::1", false)]
    [InlineData("example.com", false)]
    public void Validator_BindAddressMustBeIPv4(string? bind, bool valid)
    {
        var result = new ProxyNetworkOptionsValidator().Validate(null, new ProxyNetworkOptions { BindAddress = bind });

        Assert.Equal(valid, result.Succeeded);
    }

    [Fact]
    public void PreprocessArgs_SetBindAddress_TargetsNetworkOptions()
    {
        var args = Program.PreprocessArgs(["--set", "BindAddress=0.0.0.0"], out _);

        Assert.Equal($"--{nameof(ProxyNetworkOptions)}:{nameof(ProxyNetworkOptions.BindAddress)}=0.0.0.0", Assert.Single(args));
    }
}
