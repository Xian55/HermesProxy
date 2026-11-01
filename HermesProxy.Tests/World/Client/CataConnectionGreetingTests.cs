using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HermesProxy.World.Client;
using Xunit;

namespace HermesProxy.Tests.World.Client;

public class CataConnectionGreetingTests
{
    private const string ServerText = "WORLD OF WARCRAFT CONNECTION - SERVER TO CLIENT";
    private const string ClientText = "WORLD OF WARCRAFT CONNECTION - CLIENT TO SERVER";

    private static byte[] Framed(string text, params byte[] tail)
    {
        byte[] body = [.. Encoding.ASCII.GetBytes(text), .. tail];
        return [(byte)(body.Length >> 8), (byte)body.Length, .. body];
    }

    /// <summary>
    /// Runs the exchange over a real socket pair with <paramref name="greeting"/> as the server's
    /// half. The server closes after it has sent, so a refused greeting leaves nothing to read.
    /// </summary>
    private static async Task<(bool Accepted, byte[] Answer)> Exchange(byte[] greeting)
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        using var proxySide = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await proxySide.ConnectAsync((IPEndPoint)listener.LocalEndpoint, TestContext.Current.CancellationToken);
        using var serverSide = await listener.AcceptSocketAsync(TestContext.Current.CancellationToken);

        var client = new WorldClient();
        typeof(WorldClient)
            .GetField("_clientSocket", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(client, proxySide);

        await serverSide.SendAsync(greeting, SocketFlags.None, TestContext.Current.CancellationToken);
        serverSide.Shutdown(SocketShutdown.Send);

        bool accepted = await client.ExchangeConnectionGreeting();
        proxySide.Shutdown(SocketShutdown.Send);

        byte[] received = new byte[256];
        int total = 0;
        while (true)
        {
            int read = await serverSide.ReceiveAsync(received.AsMemory(total), SocketFlags.None, TestContext.Current.CancellationToken);
            if (read == 0)
                break;
            total += read;
        }

        return (accepted, received[..total]);
    }

    [Fact]
    public async Task Exchange_TextAlone_AnswersWithTheTextAlone()
    {
        var (accepted, answer) = await Exchange(Framed(ServerText));

        Assert.True(accepted);
        Assert.Equal(Framed(ClientText), answer);
    }

    [Fact]
    public async Task Exchange_TextAndItsNul_AnswersInTheSameForm()
    {
        var (accepted, answer) = await Exchange(Framed(ServerText, 0));

        Assert.True(accepted);
        Assert.Equal(Framed(ClientText, 0), answer);
    }

    [Fact]
    public async Task Exchange_ExtraByteIsNotANul_Refuses()
    {
        var (accepted, answer) = await Exchange(Framed(ServerText, (byte)'!'));

        Assert.False(accepted);
        Assert.Empty(answer);
    }

    [Fact]
    public async Task Exchange_TwoExtraBytes_Refuses()
    {
        var (accepted, answer) = await Exchange(Framed(ServerText, 0, 0));

        Assert.False(accepted);
        Assert.Empty(answer);
    }

    [Theory]
    [InlineData("WORLD OF WARCRAFT CONNECTION - CLIENT TO SERVER")]
    [InlineData("WORLD OF WARCRAFT CONNECTION - SERVER TO CLIENT - V2")]
    [InlineData("WORLD OF WARCRAFT CONNECTION")]
    public async Task Exchange_OtherText_Refuses(string text)
    {
        var (accepted, answer) = await Exchange(Framed(text));

        Assert.False(accepted);
        Assert.Empty(answer);
    }

    [Fact]
    public async Task Exchange_ConnectionEndsInsideTheText_Refuses()
    {
        byte[] greeting = Framed(ServerText, 0);

        var (accepted, answer) = await Exchange(greeting.Take(greeting.Length - 1).ToArray());

        Assert.False(accepted);
        Assert.Empty(answer);
    }
}
