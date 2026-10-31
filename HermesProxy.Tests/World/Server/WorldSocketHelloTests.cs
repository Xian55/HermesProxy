using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HermesProxy.Configuration.Options;
using HermesProxy.World.Server;
using Microsoft.Extensions.Options;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// TCP has no message boundaries, so a hello has to work however it is split across reads.
/// </summary>
public class WorldSocketHelloTests
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);

    private static readonly byte[] ClientHello = Encoding.ASCII.GetBytes("WORLD OF WARCRAFT CONNECTION - CLIENT TO SERVER - V2\n");
    private static readonly int ServerHelloLength = "WORLD OF WARCRAFT CONNECTION - SERVER TO CLIENT - V2\n".Length;

    private static async Task<(WorldSocket Socket, Socket Peer)> Accepted()
    {
        var ct = TestContext.Current.CancellationToken;
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var peer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
        await peer.ConnectAsync((IPEndPoint)listener.LocalEndpoint, ct);
        Socket accepted = await listener.AcceptSocketAsync(ct);

        var socket = new WorldSocket(accepted, Options.Create(new ProxyNetworkOptions()));
        socket.Accept();
        return (socket, peer);
    }

    private static async Task<int> ReadExactly(Socket peer, int count, CancellationToken ct)
    {
        byte[] buffer = new byte[count];
        int total = 0;
        while (total < count)
        {
            int read = await peer.ReceiveAsync(buffer.AsMemory(total), SocketFlags.None, ct).AsTask().WaitAsync(Patience, ct);
            if (read == 0)
                break;
            total += read;
        }

        return total;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(53)]
    public async Task Hello_InPieces_IsAnsweredWithTheChallenge(int pieces)
    {
        var (socket, peer) = await Accepted();
        using (peer)
        {
            var ct = TestContext.Current.CancellationToken;
            Assert.Equal(ServerHelloLength, await ReadExactly(peer, ServerHelloLength, ct));

            int pieceLength = ClientHello.Length / pieces;
            for (int i = 0; i < pieces; i++)
            {
                int start = i * pieceLength;
                int end = i == pieces - 1 ? ClientHello.Length : start + pieceLength;
                await peer.SendAsync(ClientHello.AsMemory(start, end - start), SocketFlags.None, ct);
                await Task.Delay(5, ct);
            }

            Assert.Equal(1, await ReadExactly(peer, 1, ct));
            Assert.True(socket.IsOpen());

            peer.Close();
            await socket.ReceiveLoop.WaitAsync(Patience, ct);
            Assert.True(socket.ReceiveLoop.IsCompletedSuccessfully);
        }
    }

    [Fact]
    public async Task Hello_Wrong_ClosesTheConnection()
    {
        var (socket, peer) = await Accepted();
        using (peer)
        {
            var ct = TestContext.Current.CancellationToken;
            Assert.Equal(ServerHelloLength, await ReadExactly(peer, ServerHelloLength, ct));

            await peer.SendAsync(new byte[ClientHello.Length], SocketFlags.None, ct);

            await socket.ReceiveLoop.WaitAsync(Patience, ct);
            Assert.True(socket.ReceiveLoop.IsCompletedSuccessfully);
            Assert.Equal(0, await ReadExactly(peer, 1, ct));
        }
    }

    [Fact]
    public async Task Hello_Unfinished_EndsWhenThePeerLeaves()
    {
        var (socket, peer) = await Accepted();
        var ct = TestContext.Current.CancellationToken;
        Assert.Equal(ServerHelloLength, await ReadExactly(peer, ServerHelloLength, ct));

        await peer.SendAsync(ClientHello.AsMemory(0, 4), SocketFlags.None, ct);
        peer.Close();

        await socket.ReceiveLoop.WaitAsync(Patience, ct);
        Assert.True(socket.ReceiveLoop.IsCompletedSuccessfully);
    }
}
