using System;
using System.Buffers;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Framework.Networking;
using Xunit;

namespace HermesProxy.Tests.Framework;

internal sealed class TestSslSocket(Socket socket) : SSLSocket(socket)
{
    public override void Accept() { }

    public override Task ReadHandler(byte[] data, int receivedLength) => Task.CompletedTask;
}

/// <summary>
/// A real TLS connection over loopback. With <c>tinyBuffers</c> both socket buffers are shrunk and
/// the client does not read until the test asks it to, so a large server write stays pending —
/// the backpressure a slow or congested client produces.
/// </summary>
internal static class TlsLoopback
{
    private static readonly Lazy<X509Certificate2> Certificate = new(() =>
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=hermes-test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var ephemeral = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        // SChannel refuses an ephemeral key for a server credential; a PFX round trip persists it.
        return X509CertificateLoader.LoadPkcs12(ephemeral.Export(X509ContentType.Pfx), null);
    });

    public static async Task With<TSocket>(Func<Socket, TSocket> create, bool tinyBuffers,
        Func<TSocket, SslStream, Task> test) where TSocket : SSLSocket
    {
        var ct = TestContext.Current.CancellationToken;
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        using var tcp = new TcpClient();
        if (tinyBuffers)
            tcp.ReceiveBufferSize = 1024;
        await tcp.ConnectAsync((IPEndPoint)listener.LocalEndpoint, ct);
        var serverSocket = await listener.AcceptSocketAsync(ct);
        if (tinyBuffers)
            serverSocket.SendBufferSize = 1024;

        var server = create(serverSocket);
        try
        {
            using var client = new SslStream(tcp.GetStream(), false, (_, _, _, _) => true);
            // AsyncHandshake parks in AsyncRead once authenticated, so it is started, not awaited.
            _ = server.AsyncHandshake(Certificate.Value);
            await client.AuthenticateAsClientAsync(
                new SslClientAuthenticationOptions { TargetHost = "hermes-test" }, ct);
            while (!server._stream.IsAuthenticated)
                await Task.Delay(10, ct);

            await test(server, client);
        }
        finally
        {
            server.CloseSocket();
            server.Dispose();
        }
    }

    public static byte[] Pattern(int length, byte seed)
    {
        var bytes = new byte[length];
        for (int i = 0; i < length; i++)
            bytes[i] = (byte)(i * 31 + seed);
        return bytes;
    }

    public static async Task<byte[]> ReadExactly(SslStream client, int length)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));
        var received = new byte[length];
        await client.ReadExactlyAsync(received, timeout.Token);
        return received;
    }

    /// <summary>Waits until the first write is genuinely stuck behind a full peer window.</summary>
    public static async Task AssertStalled(Task write)
    {
        await Task.Delay(200, TestContext.Current.CancellationToken);
        Assert.False(write.IsCompleted, "precondition: the bulk write should be waiting on the peer");
    }
}

public class SslSocketWriteTests
{
    private const int BulkSize = 4 * 1024 * 1024;

    [Fact]
    public async Task AsyncWrite_WhileAnotherWriteIsStalled_QueuesBehindItInOrder()
    {
        await TlsLoopback.With(s => new TestSslSocket(s), tinyBuffers: true, async (server, client) =>
        {
            var bulk = TlsLoopback.Pattern(BulkSize, seed: 1);
            var tail = TlsLoopback.Pattern(16, seed: 200);

            var first = server.AsyncWrite(bulk);
            await TlsLoopback.AssertStalled(first);
            // SslStream throws NotSupportedException for a WriteAsync that starts while another is
            // pending; unserialised, this frame was logged and dropped.
            var second = server.AsyncWrite(tail);

            var received = await TlsLoopback.ReadExactly(client, bulk.Length + tail.Length);
            await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(15), TestContext.Current.CancellationToken);

            Assert.True(received.AsSpan(0, bulk.Length).SequenceEqual(bulk));
            Assert.True(received.AsSpan(bulk.Length).SequenceEqual(tail));
        });
    }

    [Fact]
    public async Task AsyncWrite_PooledBuffer_SendsOnlyTheRequestedLength()
    {
        await TlsLoopback.With(s => new TestSslSocket(s), tinyBuffers: false, async (server, client) =>
        {
            var head = TlsLoopback.Pattern(10, seed: 7);
            var tail = TlsLoopback.Pattern(16, seed: 200);
            var rented = ArrayPool<byte>.Shared.Rent(64);
            head.CopyTo(rented, 0);
            rented.AsSpan(head.Length).Fill(0xEE);

            await server.AsyncWrite(rented, head.Length, returnToPool: true);
            await server.AsyncWrite(tail);

            var received = await TlsLoopback.ReadExactly(client, head.Length + tail.Length);
            Assert.True(received.AsSpan(0, head.Length).SequenceEqual(head));
            Assert.True(received.AsSpan(head.Length).SequenceEqual(tail));
        });
    }

    [Fact]
    public async Task CloseSocketAfterWrites_DeliversQueuedWritesBeforeClosing()
    {
        await TlsLoopback.With(s => new TestSslSocket(s), tinyBuffers: true, async (server, client) =>
        {
            var bulk = TlsLoopback.Pattern(BulkSize, seed: 1);
            var tail = TlsLoopback.Pattern(16, seed: 200);

            var first = server.AsyncWrite(bulk);
            await TlsLoopback.AssertStalled(first);
            _ = server.AsyncWrite(tail);
            var close = server.CloseSocketAfterWrites(TimeSpan.FromSeconds(30));

            Assert.True(server.IsOpen());
            var received = await TlsLoopback.ReadExactly(client, bulk.Length + tail.Length);
            await close.WaitAsync(TimeSpan.FromSeconds(15), TestContext.Current.CancellationToken);

            Assert.False(server.IsOpen());
            Assert.True(received.AsSpan(bulk.Length).SequenceEqual(tail));
        });
    }

    [Fact]
    public async Task CloseSocketAfterWrites_ClosesAnywayWhenThePeerStopsReading()
    {
        await TlsLoopback.With(s => new TestSslSocket(s), tinyBuffers: true, async (server, client) =>
        {
            var first = server.AsyncWrite(TlsLoopback.Pattern(BulkSize, seed: 1));
            await TlsLoopback.AssertStalled(first);

            await server.CloseSocketAfterWrites(TimeSpan.FromMilliseconds(200))
                .WaitAsync(TimeSpan.FromSeconds(15), TestContext.Current.CancellationToken);

            Assert.False(server.IsOpen());
            // Once the socket is gone the stalled write fails and is swallowed rather than hanging.
            await first.WaitAsync(TimeSpan.FromSeconds(15), TestContext.Current.CancellationToken);
        });
    }
}
