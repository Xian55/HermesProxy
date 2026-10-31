using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Framework.Networking;
using Xunit;

namespace HermesProxy.Tests.Framework;

/// <summary>
/// <see cref="SSLSocket.AsyncRead"/> over a plain socket, the way the login web service runs for
/// clients that refuse the development certificate. TLS changes nothing about the loop.
/// </summary>
public class SslSocketReadTests
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);

    private sealed class PlainSocket(Socket socket, Func<int, Task>? onRead = null) : SSLSocket(socket, useTls: false)
    {
        public List<int> StackDepths { get; } = [];
        public int Received { get; private set; }

        public override void Accept() { }

        public override Task ReadHandler(byte[] data, int receivedLength)
        {
            Received += receivedLength;
            StackDepths.Add(new StackTrace(false).FrameCount);
            return onRead?.Invoke(receivedLength) ?? Task.CompletedTask;
        }
    }

    private static async Task<(Socket Accepted, Socket Peer)> Connected()
    {
        var ct = TestContext.Current.CancellationToken;
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var peer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
        await peer.ConnectAsync((IPEndPoint)listener.LocalEndpoint, ct);
        return (await listener.AcceptSocketAsync(ct), peer);
    }

    [Fact]
    public async Task AsyncRead_PeerKeepsDataWaiting_DoesNotDeepenTheStack()
    {
        const int Total = 8 * 1024 * 1024;

        var (accepted, peer) = await Connected();
        using (peer)
        {
            var ct = TestContext.Current.CancellationToken;
            var reader = new PlainSocket(accepted);

            // See the same test for SocketBase: reads that find data waiting finish without going async.
            var send = peer.SendAsync(new byte[Total], SocketFlags.None, ct).AsTask();
            await Task.Delay(50, ct);
            var read = reader.AsyncRead();

            await send.WaitAsync(Patience, ct);
            peer.Close();
            await read.WaitAsync(Patience, ct);

            Assert.Equal(Total, reader.Received);
            Assert.True(reader.StackDepths.Count > 100, $"{reader.StackDepths.Count} reads");
            Assert.InRange(reader.StackDepths.Max(), 1, 200);
        }
    }

    [Fact]
    public async Task AsyncRead_HandlerFaults_ClosesTheConnection()
    {
        var (accepted, peer) = await Connected();
        using (peer)
        {
            var ct = TestContext.Current.CancellationToken;
            var reader = new PlainSocket(accepted, _ => throw new InvalidOperationException("handler bug"));
            var read = reader.AsyncRead();

            await peer.SendAsync(new byte[] { 1 }, SocketFlags.None, ct);
            await read.WaitAsync(Patience, ct);

            Assert.True(read.IsCompletedSuccessfully);
            Assert.False(reader.IsOpen());
            Assert.Equal(0, await peer.ReceiveAsync(new byte[1], SocketFlags.None, ct));
        }
    }

    [Fact]
    public async Task AsyncRead_PeerCloses_ClosesTheConnection()
    {
        var (accepted, peer) = await Connected();
        var ct = TestContext.Current.CancellationToken;
        var reader = new PlainSocket(accepted);
        var read = reader.AsyncRead();

        peer.Shutdown(SocketShutdown.Send);
        await read.WaitAsync(Patience, ct);
        peer.Dispose();

        Assert.True(read.IsCompletedSuccessfully);
        Assert.False(reader.IsOpen());
    }

    [Fact]
    public async Task AsyncRead_HandlerClosesTheConnection_EndsTheLoop()
    {
        var (accepted, peer) = await Connected();
        using (peer)
        {
            var ct = TestContext.Current.CancellationToken;
            PlainSocket reader = null!;
            reader = new PlainSocket(accepted, _ =>
            {
                reader.CloseSocket();
                return Task.CompletedTask;
            });
            var read = reader.AsyncRead();

            await peer.SendAsync(new byte[] { 1 }, SocketFlags.None, ct);
            await read.WaitAsync(Patience, ct);

            Assert.True(read.IsCompletedSuccessfully);
        }
    }

    [Fact]
    public async Task AsyncRead_HandlerStillWorking_HoldsTheNextRead()
    {
        var (accepted, peer) = await Connected();
        using (peer)
        {
            var ct = TestContext.Current.CancellationToken;
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            int running = 0;
            int overlapped = 0;
            int calls = 0;

            var reader = new PlainSocket(accepted, async _ =>
            {
                if (Interlocked.Increment(ref running) != 1)
                    Interlocked.Increment(ref overlapped);
                Interlocked.Increment(ref calls);
                await release.Task;
                Interlocked.Decrement(ref running);
            });
            var read = reader.AsyncRead();

            await peer.SendAsync(new byte[] { 1 }, SocketFlags.None, ct);
            await Task.Delay(50, ct);
            await peer.SendAsync(new byte[] { 2 }, SocketFlags.None, ct);
            await Task.Delay(50, ct);

            Assert.Equal(1, Volatile.Read(ref calls));

            release.SetResult();
            peer.Shutdown(SocketShutdown.Send);
            await read.WaitAsync(Patience, ct);

            Assert.Equal(0, overlapped);
            Assert.Equal(2, reader.Received);
        }
    }
}
