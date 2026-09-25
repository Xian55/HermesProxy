using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bgs.Protocol;
using Bgs.Protocol.Authentication.V1;
using BNetServer;
using Framework.Constants;
using Google.Protobuf;
using HermesProxy.Configuration.Options;
using Xunit;
using static HermesProxy.Tests.BnetServer.BnetSessionHarness;

namespace HermesProxy.Tests.BnetServer;

public class LauncherLoginTests
{
    private static LogonRequest Request(string ticket) => new()
    {
        Program = "WoW", Platform = "MacA", Locale = "ruRU",
        ApplicationVersion = ModernVersion.BuildInt,
        CachedWebCredentials = ByteString.CopyFromUtf8(ticket)
    };

    private static void AppendLogon(RecordingSession session, LogonRequest request)
        => AppendFrame(session, new Header
        {
            ServiceId = 0, ServiceHash = (uint)OriginalHash.AuthenticationService,
            MethodId = 1, Token = 42
        }, request);

    [Fact]
    public async Task UnknownLauncherTicket_IsDeniedWithoutWebChallenge()
    {
        await WithSession(async session =>
        {
            AppendLogon(session, Request("unknown-" + Guid.NewGuid()));
            await session.ProcessCurrentBuffer();
            var reply = Assert.Single(session.Replies);
            Assert.Equal(BattlenetRpcErrorCode.Denied, reply.Status);
            Assert.Equal(42u, reply.Token);
        });
    }

    [Fact]
    public async Task ValidLauncherTicket_CompletesLoginAndAcknowledgesLogon()
    {
        string ticket = "synthetic-" + Guid.NewGuid();
        // This test exercises only BNet authentication, without starting a legacy socket.
        var login = (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));
        login.Username = "SYNTHETIC";
        login.AuthClient = new(login); // OnDisconnect clears it, so its presence marks a live session
        BnetSessionTicketStorage.SessionsByTicket[ticket] = login;
        try
        {
            await WithSession(async session =>
            {
                AppendLogon(session, Request(ticket));
                await session.ProcessCurrentBuffer();
                Assert.Equal(2, session.Replies.Count);
                var complete = Assert.Single(session.Replies.Where(r => r.Service == OriginalHash.AuthenticationListener));
                var result = Assert.IsType<LogonResult>(complete.Message);
                Assert.Equal(5u, complete.MethodId);
                Assert.Equal(0u, result.ErrorCode);
                Assert.Single(result.GameAccountId);
                Assert.Equal(64, result.SessionKey.Length);
                var acknowledgement = Assert.Single(session.Replies.Where(r => r.Token == 42 && r.ServiceId == 0xFE));
                Assert.Equal(BattlenetRpcErrorCode.Ok, acknowledgement.Status);
                Assert.IsType<NoData>(acknowledgement.Message);
            });
        }
        finally { BnetSessionTicketStorage.SessionsByTicket.TryRemove(ticket, out _); }
    }

    [Fact]
    public async Task LauncherTicket_DoesNotSkipPlatformValidation()
    {
        await WithSession(async session =>
        {
            var request = Request("synthetic");
            request.Platform = "invalid";
            AppendLogon(session, request);
            await session.ProcessCurrentBuffer();
            Assert.Equal(BattlenetRpcErrorCode.BadPlatform, Assert.Single(session.Replies).Status);
        });
    }

    [Fact]
    public async Task TicketOfTornDownSession_IsDeniedAndForgotten()
    {
        string ticket = "synthetic-" + Guid.NewGuid();
        // No AuthClient, as OnDisconnect leaves it: the logon raced the teardown.
        var login = (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));
        login.Username = "SYNTHETIC";
        BnetSessionTicketStorage.SessionsByTicket[ticket] = login;
        try
        {
            await WithSession(async session =>
            {
                AppendLogon(session, Request(ticket));
                await session.ProcessCurrentBuffer();
                Assert.Equal(BattlenetRpcErrorCode.Denied, Assert.Single(session.Replies).Status);
            });
            Assert.False(BnetSessionTicketStorage.SessionsByTicket.ContainsKey(ticket));
        }
        finally { BnetSessionTicketStorage.SessionsByTicket.TryRemove(ticket, out _); }
    }

    [Fact]
    public void OnDisconnect_ForgetsTheSessionsTicket()
    {
        string ticket = "synthetic-" + Guid.NewGuid();
        var login = new GlobalSessionData(new ClientOptions(), new LegacyServerOptions(),
            new ProxyNetworkOptions(), new DiagnosticsOptions { PacketsLog = false }, new ThrottlingOptions());
        login.LoginTicket = ticket;
        BnetSessionTicketStorage.SessionsByTicket[ticket] = login;
        try
        {
            login.OnDisconnect();
            Assert.False(BnetSessionTicketStorage.SessionsByTicket.ContainsKey(ticket));
        }
        finally
        {
            BnetSessionTicketStorage.SessionsByTicket.TryRemove(ticket, out _);
            login.Executor.Dispose();
        }
    }
}
