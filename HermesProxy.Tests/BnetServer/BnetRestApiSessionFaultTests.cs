using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using BNetServer.Networking;
using HermesProxy.Configuration.Options;
using Microsoft.Extensions.Options;
using Xunit;

namespace HermesProxy.Tests.BnetServer;

/// <summary>
/// Issue #266. BnetRestApiSession shares the SSLSocket read loop, so an exception escaping its
/// ReadHandler skipped the AsyncRead() that re-arms the loop and vanished into a discarded task.
/// </summary>
public class BnetRestApiSessionFaultTests
{
    // HttpHelper.ParseRequest stores headers in a Dictionary via Add, so a repeated header name
    // throws ArgumentException. Real clients do send duplicate headers.
    private const string DuplicateHeaderRequest =
        "GET /bnetserver/login/ HTTP/1.1\r\n" +
        "Accept: application/json\r\n" +
        "Accept: text/plain\r\n" +
        "\r\n";

    [Fact]
    public async Task ReadHandler_RequestThatFailsToParse_ClosesConnectionInsteadOfFaulting()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        using var client = new TcpClient();
        await client.ConnectAsync((IPEndPoint)listener.LocalEndpoint, TestContext.Current.CancellationToken);
        using var socket = await listener.AcceptSocketAsync(TestContext.Current.CancellationToken);
        using var session = new BnetRestApiSession(
            socket,
            Options.Create(new ClientOptions()),
            Options.Create(new LegacyServerOptions()),
            Options.Create(new ProxyNetworkOptions()),
            Options.Create(new DiagnosticsOptions()),
            Options.Create(new ThrottlingOptions()));

        var data = Encoding.UTF8.GetBytes(DuplicateHeaderRequest);

        await session.ReadHandler(data, data.Length);

        Assert.False(session.IsOpen());
    }
}
