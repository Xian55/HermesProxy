using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HermesProxy.Auth;
using HermesProxy.Configuration.Options;
using HermesProxy.Enums;
using Xunit;
using AuthResult = HermesProxy.Auth.AuthResult;

namespace HermesProxy.Tests.Auth;

/// <summary>
/// <see cref="AuthClient"/> against a socket standing in for the legacy auth server. The login
/// never gets past the challenge: what is under test is how answers reach the parsers, not SRP.
/// </summary>
public class AuthClientTests
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);

    private static readonly byte[] Refusal =
        [(byte)AuthCommand.LOGON_CHALLENGE, 0, (byte)AuthResult.FAIL_UNKNOWN_ACCOUNT];

    private static byte[] Challenge(byte topByteOfKey)
    {
        byte[] modulus = Convert.FromHexString("B79B3E2A87823CAB8F5EBFBF8EB10108535006298B5BADBD5B53E1895E644B89");
        byte[] publicKey = new byte[32];
        publicKey[0] = 5;
        publicKey[^1] = topByteOfKey;
        return [(byte)AuthCommand.LOGON_CHALLENGE, 0, 0, .. publicKey, 1, 7, 32, .. modulus, .. new byte[32], .. new byte[16], 0];
    }

    private static GlobalSessionData Session(int port) => new(
        new ClientOptions(),
        new LegacyServerOptions { Address = "127.0.0.1", Port = port },
        new ProxyNetworkOptions(),
        new DiagnosticsOptions(),
        new ThrottlingOptions());

    private static byte[] RealmList(string name)
    {
        using var body = new MemoryStream();
        using var writer = new BinaryWriter(body);

        writer.Write(0u);
        bool vanilla = LegacyVersion.Build < ClientVersionBuild.V2_0_3_6299;
        if (vanilla)
            writer.Write((byte)1);
        else
            writer.Write((ushort)1);

        if (vanilla)
        {
            writer.Write(0u); // type
        }
        else
        {
            writer.Write((byte)0); // type
            writer.Write((byte)0); // locked
        }

        writer.Write((byte)0); // flags
        writer.Write(Encoding.ASCII.GetBytes(name));
        writer.Write((byte)0);
        writer.Write(Encoding.ASCII.GetBytes("127.0.0.1:8085"));
        writer.Write((byte)0);
        writer.Write(0f); // population
        writer.Write((byte)0); // characters
        writer.Write((byte)0); // timezone
        writer.Write((byte)0);
        writer.Flush();

        byte[] payload = body.ToArray();
        return [(byte)AuthCommand.REALM_LIST, (byte)payload.Length, (byte)(payload.Length >> 8), .. payload];
    }

    /// <summary>Logs in against <paramref name="serve"/>, which starts with the challenge read.</summary>
    private static async Task<(AuthResult Result, AuthClient Client)> Login(
        Func<Socket, CancellationToken, Task> serve, TimeSpan? timeout = null)
    {
        var ct = TestContext.Current.CancellationToken;
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var client = new AuthClient(Session(((IPEndPoint)listener.LocalEndpoint).Port));
        if (timeout is { } shorter)
            client.SetLoginTimeout(shorter);

        var login = client.ConnectToAuthServerAsync("tester", "secret", "enUS");

        using Socket server = await listener.AcceptSocketAsync(ct);
        server.NoDelay = true;

        var challenge = new byte[256];
        int read = await server.ReceiveAsync(challenge, SocketFlags.None, ct);
        Assert.True(read > 0);
        Assert.Equal((byte)AuthCommand.LOGON_CHALLENGE, challenge[0]);

        await serve(server, ct);

        return (await login.WaitAsync(Patience, ct), client);
    }

    [Fact]
    public async Task Login_ServerRefuses_ReturnsItsReason()
    {
        var (result, _) = await Login((server, ct) => server.SendAsync(Refusal, SocketFlags.None, ct).AsTask());

        Assert.Equal(AuthResult.FAIL_UNKNOWN_ACCOUNT, result);
    }

    // The proof goes out from inside the receive loop, which then has to read the answer to it.
    [Fact]
    public async Task Login_ServerRefusesTheProof_ReturnsItsReason()
    {
        var (result, _) = await Login(async (server, ct) =>
        {
            await server.SendAsync(Challenge(topByteOfKey: 1), SocketFlags.None, ct);

            var proof = new byte[75];
            Assert.True(await server.ReceiveAsync(proof, SocketFlags.None, ct) > 0);
            Assert.Equal((byte)AuthCommand.LOGON_PROOF, proof[0]);

            await server.SendAsync(new byte[] { (byte)AuthCommand.LOGON_PROOF, (byte)AuthResult.FAIL_INCORRECT_PASSWORD, 0, 0 }, SocketFlags.None, ct);
        });

        Assert.Equal(AuthResult.FAIL_INCORRECT_PASSWORD, result);
    }

    [Fact]
    public async Task Login_ChallengeCoresReadDifferently_AsksAgainOnANewConnectionWithoutAProof()
    {
        var ct = TestContext.Current.CancellationToken;
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var client = new AuthClient(Session(((IPEndPoint)listener.LocalEndpoint).Port));
        var login = client.ConnectToAuthServerAsync("tester", "secret", "enUS");

        using Socket first = await listener.AcceptSocketAsync(ct);
        await first.ReceiveAsync(new byte[256], SocketFlags.None, ct);
        await first.SendAsync(Challenge(topByteOfKey: 0), SocketFlags.None, ct);

        using Socket second = await listener.AcceptSocketAsync(ct);
        var challenge = new byte[256];
        await second.ReceiveAsync(challenge, SocketFlags.None, ct);
        Assert.Equal((byte)AuthCommand.LOGON_CHALLENGE, challenge[0]);

        // The first connection ended there, with nothing behind the challenge it was given.
        Assert.Equal(0, await first.ReceiveAsync(new byte[1], SocketFlags.None, ct));

        await second.SendAsync(Refusal, SocketFlags.None, ct);
        Assert.Equal(AuthResult.FAIL_UNKNOWN_ACCOUNT, await login.WaitAsync(Patience, ct));
    }

    [Fact]
    public async Task Login_EveryChallengeCoresReadDifferently_UsesTheLastOne()
    {
        var ct = TestContext.Current.CancellationToken;
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var client = new AuthClient(Session(((IPEndPoint)listener.LocalEndpoint).Port));
        var login = client.ConnectToAuthServerAsync("tester", "secret", "enUS");

        var proof = new byte[75];
        int read = 0;
        for (int connection = 0; read == 0 || proof[0] != (byte)AuthCommand.LOGON_PROOF; connection++)
        {
            Assert.InRange(connection, 0, 20);

            using Socket server = await listener.AcceptSocketAsync(ct);
            await server.ReceiveAsync(new byte[256], SocketFlags.None, ct);
            await server.SendAsync(Challenge(topByteOfKey: 0), SocketFlags.None, ct);

            read = await server.ReceiveAsync(proof, SocketFlags.None, ct);
            if (read > 0 && proof[0] == (byte)AuthCommand.LOGON_PROOF)
                await server.SendAsync(new byte[] { (byte)AuthCommand.LOGON_PROOF, (byte)AuthResult.FAIL_INCORRECT_PASSWORD, 0, 0 }, SocketFlags.None, ct);
        }

        Assert.Equal(AuthResult.FAIL_INCORRECT_PASSWORD, await login.WaitAsync(Patience, ct));
    }

    [Fact]
    public async Task Login_ServerCloses_Fails()
    {
        var (result, _) = await Login((server, _) =>
        {
            server.Shutdown(SocketShutdown.Both);
            return Task.CompletedTask;
        });

        Assert.Equal(AuthResult.FAIL_INTERNAL_ERROR, result);
    }

    [Fact]
    public async Task Login_ServerResets_Fails()
    {
        var (result, _) = await Login((server, _) =>
        {
            server.LingerState = new LingerOption(true, 0);
            server.Close();
            return Task.CompletedTask;
        });

        Assert.Equal(AuthResult.FAIL_INTERNAL_ERROR, result);
    }

    [Fact]
    public async Task Login_NobodyListens_Fails()
    {
        var ct = TestContext.Current.CancellationToken;
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        var result = await new AuthClient(Session(port))
            .ConnectToAuthServerAsync("tester", "secret", "enUS")
            .WaitAsync(Patience, ct);

        Assert.Equal(AuthResult.FAIL_INTERNAL_ERROR, result);
    }

    [Fact]
    public async Task Login_ServerStaysSilent_GivesUpAndDisconnects()
    {
        var (result, client) = await Login((_, _) => Task.CompletedTask, TimeSpan.FromMilliseconds(100));

        Assert.Equal(AuthResult.FAIL_INTERNAL_ERROR, result);
        Assert.False(client.IsConnected());
    }

    [Fact]
    public async Task Login_UnknownOpcode_Fails()
    {
        var (result, _) = await Login((server, ct) =>
            server.SendAsync(new byte[] { (byte)AuthCommand.TRANSFER_DATA, 0 }, SocketFlags.None, ct).AsTask());

        Assert.Equal(AuthResult.FAIL_INTERNAL_ERROR, result);
    }

    // A refusal behind the list proves it was parsed whole and that the loop went on reading.
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    public async Task RealmList_SplitAcrossReads_IsAssembled(int firstPiece)
    {
        byte[] list = RealmList("Split");

        var (result, client) = await Login(async (server, ct) =>
        {
            await server.SendAsync(list[..firstPiece], SocketFlags.None, ct);
            await Task.Delay(30, ct);
            await server.SendAsync(list[firstPiece..(firstPiece + 1)], SocketFlags.None, ct);
            await Task.Delay(30, ct);
            await server.SendAsync(list[(firstPiece + 1)..], SocketFlags.None, ct);
            await Task.Delay(30, ct);
            await server.SendAsync(Refusal, SocketFlags.None, ct);
        });

        Assert.Equal(AuthResult.FAIL_UNKNOWN_ACCOUNT, result);
        Assert.Equal("Split", Assert.Single(client.GetSession().RealmManager.GetRealms()).Name);
    }

    [Fact]
    public async Task RealmList_SharingAReadWithTheNextMessage_LeavesThatMessageWhole()
    {
        byte[] list = RealmList("Shared");

        var (result, client) = await Login((server, ct) =>
            server.SendAsync(list.Concat(Refusal).ToArray(), SocketFlags.None, ct).AsTask());

        Assert.Equal(AuthResult.FAIL_UNKNOWN_ACCOUNT, result);
        Assert.Equal("Shared", Assert.Single(client.GetSession().RealmManager.GetRealms()).Name);
    }

    [Fact]
    public async Task RealmList_SentTwice_KeepsReading()
    {
        var (result, client) = await Login(async (server, ct) =>
        {
            await server.SendAsync(RealmList("First"), SocketFlags.None, ct);
            await Task.Delay(30, ct);
            await server.SendAsync(RealmList("Second"), SocketFlags.None, ct);
            await Task.Delay(30, ct);
            await server.SendAsync(Refusal, SocketFlags.None, ct);
        });

        Assert.Equal(AuthResult.FAIL_UNKNOWN_ACCOUNT, result);
        Assert.Equal("Second", Assert.Single(client.GetSession().RealmManager.GetRealms()).Name);
    }

    [Fact]
    public void MessageLength_AnythingButARealmList_IsTheWholeRead()
    {
        Assert.Equal(3, AuthClient.MessageLength([(byte)AuthCommand.LOGON_CHALLENGE, 0, 4]));
        Assert.Equal(1, AuthClient.MessageLength([(byte)AuthCommand.LOGON_PROOF]));
    }

    [Theory]
    [InlineData(1, 0)] // no size yet
    [InlineData(2, 0)] // half the size
    [InlineData(3, 0)] // size, no body
    [InlineData(6, 0)] // one byte short
    [InlineData(7, 7)] // exactly the message
    [InlineData(9, 7)] // the message and the start of the next
    public void MessageLength_RealmList_TakesItsSizeFromTheHeader(int available, int expected)
    {
        byte[] pending = new byte[available];
        pending[0] = (byte)AuthCommand.REALM_LIST;
        if (available > 1)
            pending[1] = 4;

        Assert.Equal(expected, AuthClient.MessageLength(pending));
    }

    [Fact]
    public void MessageLength_LargestRealmList_FitsWhatTheLoopReceives()
    {
        byte[] pending = new byte[3 + ushort.MaxValue];
        pending[0] = (byte)AuthCommand.REALM_LIST;
        pending[1] = 0xFF;
        pending[2] = 0xFF;

        Assert.Equal(pending.Length, AuthClient.MessageLength(pending));
    }
}
