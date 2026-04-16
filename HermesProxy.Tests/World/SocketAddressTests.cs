using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World;

public class SocketAddressUnionTests
{
    [Fact]
    public void SocketAddress_IPv4_PatternMatches()
    {
        var bytes = new byte[] { 192, 168, 1, 1 };
        ConnectTo.SocketAddress address = new ConnectTo.IPv4Address(bytes);

        var matched = address.Value switch
        {
            ConnectTo.IPv4Address ipv4 => ipv4.Bytes,
            ConnectTo.IPv6Address => null,
            ConnectTo.NamedSocketAddress => null,
            _ => null,
        };

        Assert.Equal(bytes, matched);
    }

    [Fact]
    public void SocketAddress_IPv6_PatternMatches()
    {
        var bytes = new byte[16];
        for (byte i = 0; i < 16; i++) bytes[i] = i;
        ConnectTo.SocketAddress address = new ConnectTo.IPv6Address(bytes);

        var matched = address.Value switch
        {
            ConnectTo.IPv6Address ipv6 => ipv6.Bytes,
            ConnectTo.IPv4Address => null,
            ConnectTo.NamedSocketAddress => null,
            _ => null,
        };

        Assert.Equal(bytes, matched);
    }

    [Fact]
    public void SocketAddress_NamedSocket_PatternMatches()
    {
        const string name = "/tmp/socket";
        ConnectTo.SocketAddress address = new ConnectTo.NamedSocketAddress(name);

        var matched = address.Value switch
        {
            ConnectTo.NamedSocketAddress named => named.Name,
            ConnectTo.IPv4Address => null,
            ConnectTo.IPv6Address => null,
            _ => null,
        };

        Assert.Equal(name, matched);
    }

    [Fact]
    public void SocketAddress_IPv4_DistinguishesFromIPv6_DespiteSharedByteArray()
    {
        // Verify wrapper records solve the shared-byte[] discrimination problem.
        // Both IPv4Address and IPv6Address wrap byte[] internally, but the union
        // discriminates by the wrapper type.
        var ipv4Bytes = new byte[] { 10, 0, 0, 1 };
        var ipv6Bytes = new byte[16];

        ConnectTo.SocketAddress ipv4 = new ConnectTo.IPv4Address(ipv4Bytes);
        ConnectTo.SocketAddress ipv6 = new ConnectTo.IPv6Address(ipv6Bytes);

        Assert.IsType<ConnectTo.IPv4Address>(ipv4.Value);
        Assert.IsType<ConnectTo.IPv6Address>(ipv6.Value);
    }
}
