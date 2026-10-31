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
/// <see cref="SocketBase"/>'s receive loop against a real peer over loopback.
/// </summary>
public class SocketReceiveLoopTests
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);

    private sealed class RecordingSocket(Socket socket, Action<RecordingSocket>? onRead = null) : SocketBase(socket)
    {
        private readonly List<byte> _bytes = [];
        private int _inHandler;

        public SemaphoreSlim ReadArrived { get; } = new(0);
        public List<int> StackDepths { get; } = [];
        public byte[] Bytes => _bytes.ToArray();
        public bool Overlapped { get; private set; }
        public int Closes;

        public override void Accept() => BeginReceive();

        public override void ReadHandler(byte[] data, int length)
        {
            if (Interlocked.Increment(ref _inHandler) != 1)
                Overlapped = true;

            _bytes.AddRange(data.AsSpan(0, length));
            StackDepths.Add(new StackTrace(false).FrameCount);
            Interlocked.Decrement(ref _inHandler);

            ReadArrived.Release();
            onRead?.Invoke(this);
        }

        public override void OnClose()
        {
            Interlocked.Increment(ref Closes);
            base.OnClose();
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
    public async Task Receive_OneByteAtATime_ReachesTheHandlerInOrder()
    {
        var (accepted, peer) = await Connected();
        using (peer)
        {
            var reader = new RecordingSocket(accepted);
            reader.Accept();

            byte[] sent = Enumerable.Range(1, 16).Select(i => (byte)i).ToArray();
            foreach (byte value in sent)
            {
                await peer.SendAsync(new[] { value }, SocketFlags.None, TestContext.Current.CancellationToken);
                Assert.True(await reader.ReadArrived.WaitAsync(Patience, TestContext.Current.CancellationToken));
            }

            Assert.Equal(sent, reader.Bytes);
            Assert.False(reader.Overlapped);

            peer.Close();
            await reader.ReceiveLoop.WaitAsync(Patience, TestContext.Current.CancellationToken);
            Assert.Equal(1, reader.Closes);
        }
    }

    [Fact]
    public async Task Receive_PeerKeepsDataWaiting_DoesNotDeepenTheStack()
    {
        const int Total = 8 * 1024 * 1024;

        var (accepted, peer) = await Connected();
        using (peer)
        {
            var reader = new RecordingSocket(accepted);

            // Started before anyone reads, so that reads find data waiting and finish without
            // going async.
            var ct = TestContext.Current.CancellationToken;
            var send = peer.SendAsync(new byte[Total], SocketFlags.None, ct).AsTask();
            await Task.Delay(50, ct);
            reader.Accept();

            await send.WaitAsync(Patience, ct);
            peer.Close();
            await reader.ReceiveLoop.WaitAsync(Patience, ct);

            Assert.Equal(Total, reader.Bytes.Length);
            Assert.False(reader.Overlapped);
            Assert.True(reader.StackDepths.Count > 100, $"{reader.StackDepths.Count} reads");
            // Well above where a read starts from, and well below a few frames per read.
            Assert.InRange(reader.StackDepths.Max(), 1, 200);
        }
    }

    [Fact]
    public async Task ReadHandler_Throws_ClosesThatConnectionAlone()
    {
        var (faultingSocket, faultingPeer) = await Connected();
        var (otherSocket, otherPeer) = await Connected();
        using (faultingPeer)
        using (otherPeer)
        {
            var ct = TestContext.Current.CancellationToken;
            var faulting = new RecordingSocket(faultingSocket, _ => throw new InvalidOperationException("handler bug"));
            var other = new RecordingSocket(otherSocket);
            faulting.Accept();
            other.Accept();

            await faultingPeer.SendAsync(new byte[] { 1 }, SocketFlags.None, ct);
            await faulting.ReceiveLoop.WaitAsync(Patience, ct);

            Assert.True(faulting.ReceiveLoop.IsCompletedSuccessfully);
            Assert.Equal(1, faulting.Closes);
            Assert.Equal(0, await faultingPeer.ReceiveAsync(new byte[1], SocketFlags.None, ct));

            await otherPeer.SendAsync(new byte[] { 2 }, SocketFlags.None, ct);
            Assert.True(await other.ReadArrived.WaitAsync(Patience, ct));
            Assert.True(other.IsOpen());

            other.CloseSocket();
        }
    }

    [Fact]
    public async Task ReadHandler_ClosesItsOwnConnection_ClosesOnce()
    {
        var (accepted, peer) = await Connected();
        using (peer)
        {
            var ct = TestContext.Current.CancellationToken;
            var reader = new RecordingSocket(accepted, self => self.CloseSocket());
            reader.Accept();

            await peer.SendAsync(new byte[] { 1 }, SocketFlags.None, ct);
            await reader.ReceiveLoop.WaitAsync(Patience, ct);

            Assert.True(reader.ReceiveLoop.IsCompletedSuccessfully);
            Assert.Equal(1, reader.Closes);
        }
    }

    [Fact]
    public async Task Receive_PeerResets_ClosesOnce()
    {
        var (accepted, peer) = await Connected();
        var ct = TestContext.Current.CancellationToken;
        var reader = new RecordingSocket(accepted);
        reader.Accept();
        await Task.Delay(50, ct);

        // A zero linger turns the close into an RST, after which Connected is false and a close
        // that went by it never reached OnClose.
        peer.LingerState = new LingerOption(true, 0);
        peer.Close();

        await reader.ReceiveLoop.WaitAsync(Patience, ct);
        Assert.True(reader.ReceiveLoop.IsCompletedSuccessfully);
        Assert.Equal(1, reader.Closes);
    }

    [Fact]
    public async Task Receive_PeerCloses_ClosesOnce()
    {
        var (accepted, peer) = await Connected();
        var ct = TestContext.Current.CancellationToken;
        var reader = new RecordingSocket(accepted);
        reader.Accept();

        peer.Shutdown(SocketShutdown.Send);
        await reader.ReceiveLoop.WaitAsync(Patience, ct);
        peer.Dispose();

        Assert.True(reader.ReceiveLoop.IsCompletedSuccessfully);
        Assert.Equal(1, reader.Closes);
    }

    [Fact]
    public async Task CloseSocket_DuringARead_EndsTheLoopAndClosesOnce()
    {
        var (accepted, peer) = await Connected();
        using (peer)
        {
            var ct = TestContext.Current.CancellationToken;
            var reader = new RecordingSocket(accepted);
            reader.Accept();
            await Task.Delay(50, ct);

            reader.CloseSocket();
            reader.CloseSocket();
            await reader.ReceiveLoop.WaitAsync(Patience, ct);

            Assert.True(reader.ReceiveLoop.IsCompletedSuccessfully);
            Assert.Equal(1, reader.Closes);
        }
    }

    [Fact]
    public async Task CloseSocket_BeforeAnyRead_RunsOnClose()
    {
        var (accepted, peer) = await Connected();
        using (peer)
        {
            var reader = new RecordingSocket(accepted);

            reader.CloseSocket();

            Assert.Equal(1, reader.Closes);
            Assert.True(reader.ReceiveLoop.IsCompletedSuccessfully);
        }
    }
}
