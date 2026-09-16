using System;
using System.Net;
using System.Net.Sockets;
using Framework.Networking;
using Xunit;

namespace HermesProxy.Tests.Framework;

internal sealed class TestSocket(Socket socket) : SocketBase(socket)
{
    public override void Accept() { }

    public override void ReadHandler(SocketAsyncEventArgs args) { }
}

public class SocketSendTimeoutTests
{
    /// <summary>
    /// A peer that accepts the connection and then never reads a byte. Both buffers are shrunk so
    /// the sender's window fills after a few kilobytes instead of megabytes.
    /// </summary>
    private static (TestSocket Sender, Socket Peer, TcpListener Listener) StalledPeer(bool unbuffered)
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 512);

        var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        // A zero-length send buffer makes every send wait for the peer's window rather than the
        // kernel's copy, which is what a congested or stalled client looks like from here.
        client.SendBufferSize = unbuffered ? 0 : 512;
        client.Connect((IPEndPoint)listener.LocalEndpoint);

        Socket peer = listener.AcceptSocket();
        peer.ReceiveBufferSize = 512;
        return (new TestSocket(client), peer, listener);
    }

    [Fact]
    public void SendToAPeerThatNeverReads_TimesOutAndClosesTheConnection()
    {
        var (sender, peer, listener) = StalledPeer(unbuffered: true);
        try
        {
            sender.SetSendTimeout(TimeSpan.FromMilliseconds(200));
            var payload = new byte[256 * 1024];

            // The peer's window closes after a few writes; from then on a send can only wait.
            // The caller never sees the exception: a timed-out send is handled like any other
            // dropped connection.
            for (int i = 0; i < 64 && sender.IsOpen(); i++)
                sender.AsyncWrite(payload);

            Assert.False(sender.IsOpen());
        }
        finally
        {
            peer.Dispose();
            listener.Stop();
            sender.Dispose();
        }
    }

    [Fact]
    public void SendToAPeerThatReads_IsUnaffectedByTheTimeout()
    {
        var (sender, peer, listener) = StalledPeer(unbuffered: false);
        try
        {
            sender.SetSendTimeout(TimeSpan.FromSeconds(5));
            var payload = new byte[8 * 1024];
            var sink = new byte[8 * 1024];

            var reader = System.Threading.Tasks.Task.Run(() =>
            {
                int total = 0;
                while (total < payload.Length)
                {
                    int read = peer.Receive(sink, 0, sink.Length, SocketFlags.None);
                    if (read == 0) break;
                    total += read;
                }
                return total;
            });

            sender.AsyncWrite(payload);

            Assert.True(reader.Wait(TimeSpan.FromSeconds(10)));
            Assert.Equal(payload.Length, reader.Result);
            Assert.True(sender.IsOpen());
        }
        finally
        {
            peer.Dispose();
            listener.Stop();
            sender.Dispose();
        }
    }

    [Fact]
    public void EnableKeepAlive_TurnsProbesOn_AndToleratesUnsupportedOptions()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            client.Connect((IPEndPoint)listener.LocalEndpoint);
            using Socket peer = listener.AcceptSocket();

            NetworkUtils.EnableKeepAlive(client, idleSeconds: 30, intervalSeconds: 5, retryCount: 3);

            Assert.NotEqual(0, (int)client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive)!);
        }
        finally
        {
            client.Dispose();
            listener.Stop();
        }
    }
}
