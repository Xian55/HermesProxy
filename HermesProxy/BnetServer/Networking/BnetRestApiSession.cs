// Copyright (c) CypherCore <http://github.com/CypherCore> All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE. See LICENSE file in the project root for full license information.

using Framework.Constants;
using Framework.Cryptography;
using Framework.Networking;
using Framework.Serialization;
using Framework.Web;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HermesProxy;
using HermesProxy.Auth;
using HermesProxy.Configuration.Options;
using HermesProxy.Enums;
using HermesProxy.World.Server;
using Microsoft.Extensions.Options;

namespace BNetServer.Networking;

public sealed class BnetRestApiSession : SSLSocket
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melServer = Framework.Logging.Log.CreateMelLogger(Framework.Logging.Log.CategoryServer);
    private static readonly string _sourceFile = nameof(BnetRestApiSession).PadRight(15);
    private const string _netDirNone = "";

    private const string BNET_SERVER_BASE_PATH = "/bnetserver/";
    private const string TICKET_PREFIX = "HP-"; // Hermes Proxy

    // v2 takes the password as typed, where v1 would uppercase it; TrinityCore makes new accounts
    // v2, so that is the path its clients are known to take.
    private const byte SrpVersion = 2;

    // An SRP login is two POSTs, the challenge and then the proof, which need not share a
    // connection. Keyed by account name; a new challenge replaces an unanswered one.
    private static readonly ConcurrentDictionary<string, PendingSrpLogin> _pendingSrpLogins = new();
    private static readonly TimeSpan SrpChallengeLifetime = TimeSpan.FromMinutes(5);

    private sealed record PendingSrpLogin(BnetSrp6 Srp, long StartedTimestamp);

    private readonly IOptions<ClientOptions> _clientOptions;
    private readonly IOptions<LegacyServerOptions> _legacyServerOptions;
    private readonly IOptions<ProxyNetworkOptions> _networkOptions;
    private readonly IOptions<DiagnosticsOptions> _diagnosticsOptions;
    private readonly IOptions<ThrottlingOptions> _throttlingOptions;

    public BnetRestApiSession(
        Socket socket,
        IOptions<ClientOptions> clientOptions,
        IOptions<LegacyServerOptions> legacyServerOptions,
        IOptions<ProxyNetworkOptions> networkOptions,
        IOptions<DiagnosticsOptions> diagnosticsOptions,
        IOptions<ThrottlingOptions> throttlingOptions) : base(socket, useTls: !LoginServiceManager.UsesPlainHttp)
    {
        _clientOptions = clientOptions;
        _legacyServerOptions = legacyServerOptions;
        _networkOptions = networkOptions;
        _diagnosticsOptions = diagnosticsOptions;
        _throttlingOptions = throttlingOptions;
    }

    public override void Accept()
    {
        if (LoginServiceManager.UsesPlainHttp)
            _ = AsyncRead();
        else
            _ = AsyncHandshake(BnetServerCertificate.Certificate);
    }

    public override async Task ReadHandler(byte[] data, int receivedLength)
    {
        try
        {
            var httpRequest = HttpHelper.ParseRequest(data, receivedLength);
            if (httpRequest == null)
            {
                // The request line only: the body of a login POST carries the password.
                int lineEnd = Array.IndexOf(data, (byte)'\n', 0, receivedLength);
                string requestLine = System.Text.Encoding.ASCII.GetString(data, 0, lineEnd < 0 ? Math.Min(receivedLength, 200) : Math.Min(lineEnd, 200)).TrimEnd('\r');
                BnetRestApiSessionLogMessages.RequestUnparsed(_melServer, _sourceFile, _netDirNone, receivedLength, requestLine);
                CloseSocket();
                return;
            }
            if (!RequestRouter(httpRequest))
            {
                CloseSocket();
                return;
            }
        }
        catch (Exception ex)
        {
            // SSLSocket.AsyncRead cannot await this method, so an escaping exception would go
            // unlogged and skip the AsyncRead below, leaving the session silently unreadable.
            BnetRestApiSessionLogMessages.RequestHandlingFailed(_melServer, ex, _sourceFile, _netDirNone,
                GetRemoteIpEndPoint()?.ToString() ?? "<unknown>", ex.Message);
            CloseSocket();
            return;
        }

        await AsyncRead(); // Read next request
    }

    public bool RequestRouter(HttpHeader httpRequest)
    {
        if (_melServer.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
            BnetRestApiSessionLogMessages.RequestReceived(_melServer, _sourceFile, _netDirNone,
                httpRequest.Method ?? "", httpRequest.Path ?? "", GetRemoteIpEndPoint()?.ToString() ?? "<unknown>");

        if (!httpRequest.Path!.StartsWith(BNET_SERVER_BASE_PATH))
        {
            BnetRestApiSessionLogMessages.RouteNotFound(_melServer, _sourceFile, _netDirNone, httpRequest.Method ?? "", httpRequest.Path);
            _ = SendEmptyResponse(HttpCode.NotFound);
            return false;
        }

        string path = httpRequest.Path.Substring(BNET_SERVER_BASE_PATH.Length);
        string[] pathElements = path.Split('/');

        switch (pathElements[0], httpRequest.Method)
        {
            case ("login", "GET"):
                _ = SendResponse(HttpCode.Ok, LoginServiceManager.Instance.GetFormInput(GetRemoteIpEndPoint()?.Address ?? System.Net.IPAddress.None));
                return true;
            case ("login", "POST") when pathElements.Length > 1 && pathElements[1] == "srp":
                _ = HandleSrpChallengeRequest(httpRequest);
                return true;
            case ("login", "POST"):
                _ = HandleLoginRequest(pathElements, httpRequest);
                return true;
            default:
                BnetRestApiSessionLogMessages.RouteNotFound(_melServer, _sourceFile, _netDirNone, httpRequest.Method ?? "", httpRequest.Path);
                _ = SendEmptyResponse(HttpCode.NotFound);
                return false;
        };
    }

    public Task HandleLoginRequest(string[] pathElements, HttpHeader request)
    {
        LogonData? loginForm = Json.CreateObject<LogonData>(request.Content!);
        if (loginForm == null)
            return SendEmptyResponse(HttpCode.InternalServerError);

        HermesProxy.GlobalSessionData globalSession = new(_clientOptions.Value, _legacyServerOptions.Value, _networkOptions.Value, _diagnosticsOptions.Value, _throttlingOptions.Value);

        // Format: "login/$platform/$build/$locale/"
        globalSession.OS = pathElements[1];
        globalSession.Build = uint.Parse(pathElements[2]);
        globalSession.Locale = pathElements[3];

        // Should never happen. Session.HandleLogon checks version already
        if (ModernVersion.Build != (ClientVersionBuild) globalSession.Build)
            return SendAuthError(AuthResult.FAIL_WRONG_MODERN_VER);

        string login = "";
        string password = "";

        foreach (var field in loginForm.Inputs)
        {
            switch (field.Id)
            {
                case "account_name": login = field.Value!.Trim().ToUpperInvariant(); break;
                case "password": password = field.Value!.Trim(); break;
            }
        }

        string? serverEvidenceM2 = null;
        if (loginForm["public_A"] is { } publicA)
        {
            if (!_pendingSrpLogins.TryRemove(login, out PendingSrpLogin? pending)
                || Stopwatch.GetElapsedTime(pending.StartedTimestamp) > SrpChallengeLifetime)
            {
                BnetRestApiSessionLogMessages.SrpProofWithoutChallenge(_melServer, _sourceFile, _netDirNone, login);
                return SendAuthError(AuthResult.FAIL_INTERNAL_ERROR);
            }

            if (!BnetSrp6.TryParseHex(publicA, out BigInteger a)
                || !BnetSrp6.TryParseHex(loginForm["client_evidence_M1"], out BigInteger clientM1)
                || pending.Srp.VerifyClientEvidence(a, clientM1) is not { } sessionKey)
            {
                BnetRestApiSessionLogMessages.SrpProofRejected(_melServer, _sourceFile, _netDirNone, login);
                return SendAuthError(AuthResult.FAIL_INCORRECT_PASSWORD);
            }

            serverEvidenceM2 = BnetSrp6.ToHex(BnetSrp6.CalculateServerEvidence(a, clientM1, sessionKey));
            password = _legacyServerOptions.Value.Password;
        }

        globalSession.AuthClient = new(globalSession);
        AuthResult response = globalSession.AuthClient.ConnectToAuthServer(login, password, globalSession.Locale);
        if (response != AuthResult.SUCCESS)
        { // Error handling
            return SendAuthError(response);
        }
        else
        {
            // Request realmlist now, we probably need it later anyways
            globalSession.AuthClient.SendRealmListUpdateRequest();

            // Ticket creation
            LogonResult loginResult = new();
            byte[] ticket = Array.Empty<byte>().GenerateRandomKey(20);
            string loginTicket = TICKET_PREFIX + ticket.ToHexString();

            globalSession.LoginTicket = loginTicket;
            globalSession.Username = login;
            globalSession.AccountMetaDataMgr = new AccountMetaDataManager(login);
            BnetSessionTicketStorage.AddNewSessionByName(login, globalSession);
            BnetSessionTicketStorage.AddNewSessionByTicket(loginTicket, globalSession);

            loginResult.LoginTicket = loginTicket;
            loginResult.AuthenticationState = "DONE";
            loginResult.ServerEvidenceM2 = serverEvidenceM2;
            return SendResponse(HttpCode.Ok, loginResult);
        }
    }

    /// <summary>
    /// The first half of an SRP login: the client names its account and gets back what it needs
    /// to prove the password. The verifier comes from the configured legacy password, so a client
    /// whose proof checks out typed the password the proxy will log into the legacy server with.
    /// </summary>
    public Task HandleSrpChallengeRequest(HttpHeader request)
    {
        LogonData? loginForm = Json.CreateObject<LogonData>(request.Content!);
        if (loginForm == null)
            return SendEmptyResponse(HttpCode.BadRequest);

        string login = loginForm["account_name"]?.Trim().ToUpperInvariant() ?? "";
        string password = _legacyServerOptions.Value.Password;
        if (login.Length == 0 || password.Length == 0)
        {
            if (password.Length == 0)
                BnetRestApiSessionLogMessages.SrpPasswordNotConfigured(_melServer, _sourceFile, _netDirNone, login);
            return SendAuthError(AuthResult.FAIL_INTERNAL_ERROR);
        }

        BnetSrp6 srp = BnetSrp6.Create(SrpVersion, login, password);
        PruneExpiredSrpLogins();
        _pendingSrpLogins[login] = new PendingSrpLogin(srp, Stopwatch.GetTimestamp());
        BnetRestApiSessionLogMessages.SrpChallengeIssued(_melServer, _sourceFile, _netDirNone, login, srp.Version);

        return SendResponse(HttpCode.Ok, new SrpLoginChallenge
        {
            Version = srp.Version,
            Iterations = srp.Iterations,
            Modulus = BnetSrp6.ToHex(srp.N),
            Generator = BnetSrp6.ToHex(srp.Generator),
            HashFunction = "SHA-256",
            Username = srp.Username,
            Salt = Convert.ToHexString(srp.Salt),
            PublicB = BnetSrp6.ToHex(srp.B),
        });
    }

    private static void PruneExpiredSrpLogins()
    {
        foreach (var (login, pending) in _pendingSrpLogins)
            if (Stopwatch.GetElapsedTime(pending.StartedTimestamp) > SrpChallengeLifetime)
                _pendingSrpLogins.TryRemove(KeyValuePair.Create(login, pending));
    }

    async Task SendResponse<T>(HttpCode code, T response)
    {
        await AsyncWrite(HttpHelper.CreateResponse(code, Json.CreateString(response)));
    }

    async Task SendAuthError(AuthResult response)
    {
        LogonResult loginResult = new();
        (loginResult.AuthenticationState, loginResult.ErrorCode, loginResult.ErrorMessage) = response switch
        {
            AuthResult.FAIL_UNKNOWN_ACCOUNT    => ("LOGIN", "UNABLE_TO_DECODE", "Invalid username or password."),
            AuthResult.FAIL_INCORRECT_PASSWORD => ("LOGIN", "UNABLE_TO_DECODE", "Invalid password."),
            AuthResult.FAIL_BANNED             => ("LOGIN", "UNABLE_TO_DECODE", "This account has been closed and is no longer available for use."),
            AuthResult.FAIL_SUSPENDED          => ("LOGIN", "UNABLE_TO_DECODE", "This account has been temporarily suspended."),
            AuthResult.FAIL_VERSION_INVALID    => ("LOGIN", "UNABLE_TO_DECODE", "Your version is not supported by this server.\nMake sure you are using the latest HermesProxy version from GitHub.\n(Maybe HermesProxy is blocked on the server)\n"),

            AuthResult.FAIL_INTERNAL_ERROR     => ("LOGON", "UNABLE_TO_DECODE", "There was an internal error. Please try again later."),
            _ => ("LOGON", "UNABLE_TO_DECODE", $"Error: {response}"),
        };

        await SendResponse(HttpCode.BadRequest, loginResult);
    }

    async Task SendEmptyResponse(HttpCode code)
    {
        await SendResponse<object>(code, new{});
    }
}
