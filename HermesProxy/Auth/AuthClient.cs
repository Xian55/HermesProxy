using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Net.Sockets;
using HermesProxy.Enums;
using System.Numerics;
using System.Threading.Tasks;
using Framework.Constants;
using Framework;
using Framework.IO;
using Framework.Logging;
using Framework.Networking;
using HermesProxy.Auth.Logging;

namespace HermesProxy.Auth;

public partial class AuthClient
{
    // Source-generated logging: one MEL logger per target category so per-category
    // MinimumLevel.Override still applies. SourceFile + NetDir strings are cached constants.
    private static readonly Microsoft.Extensions.Logging.ILogger _melNet = Log.CreateMelLogger(Log.CategoryNetwork);
    private static readonly Microsoft.Extensions.Logging.ILogger _melServer = Log.CreateMelLogger(Log.CategoryServer);
    private static readonly string _sourceFile = nameof(AuthClient).PadRight(15);
    private static readonly string _netDirP2S = Log.FormatDir(LogNetDir.P2S);
    private static readonly string _netDirS2P = Log.FormatDir(LogNetDir.S2P);
    private const string _netDirNone = "";

    // For ez debugging: Call this function wherever you want
    private static readonly Action<ByteBuffer> _debugTraceBreakpointHandler = (b) =>
    {
#if DEBUG
        // Debugger.Log(0, "TraceMe", $"{b}");
#endif
    };

    /// <summary>
    /// How long a login waits for the legacy auth server's answer before telling the modern client
    /// the login failed. Long enough to cover a slow or busy server, short enough that a dead one
    /// doesn't leave the player at the login screen.
    /// </summary>
    internal static readonly TimeSpan LoginTimeout = TimeSpan.FromSeconds(30);

    // The realm list is the one message that outgrows a read, and it says how long it is: the
    // command, then a u16 size of what follows. Everything else is a short answer to one request.
    private const int RealmListHeaderSize = 3;
    private const int MaxMessageSize = RealmListHeaderSize + ushort.MaxValue;

    // One challenge in over a hundred needs another, so this many in a row means something else is
    // going on, and the challenge at hand is used as it is.
    private const int MaxFreshChallenges = 8;

    GlobalSessionData _globalSession;
    TimeSpan _loginTimeout = LoginTimeout;
    Socket _clientSocket = null!;
    TaskCompletionSource<AuthResult> _response = null!;
    TaskCompletionSource _hasRealmlist = null!;
    bool _realmlistRequestIsPending;
    int _freshChallenges;
    byte[] _passwordHash = null!;
    byte[] _sessionKey = null!;
    byte[] _m2 = null!;
    string _username = null!;
    string _locale = null!;

    public AuthClient(GlobalSessionData globalSession)
    {
        _globalSession = globalSession;
    }

    public GlobalSessionData GetSession()
    {
        return _globalSession;
    }

    /// <summary>Shortens the wait, so a test for a silent server doesn't take 30 seconds.</summary>
    internal void SetLoginTimeout(TimeSpan timeout) => _loginTimeout = timeout;

    public Task<AuthResult> ConnectToAuthServerAsync(string username, string password, string locale)
    {
        _username = username;
        _locale = locale;

        _response = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _hasRealmlist = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _realmlistRequestIsPending = false;
        _freshChallenges = 0;

        string authstring = $"{_username}:{password}";
        _passwordHash = SHA1.HashData(Encoding.ASCII.GetBytes(authstring.ToUpper()));

        Connect();

        return AwaitResponseAsync();
    }

    private void Connect()
    {
        try
        {
            var serverIpAddress = NetworkUtils.ResolveOrDirectIPv4(_globalSession.LegacyServerOptions.Address);
            AuthClientLogMessages.ConnectingToAuthServer(_melNet, _sourceFile, _netDirP2S, _globalSession.LegacyServerOptions.Address, _globalSession.LegacyServerOptions.Port, serverIpAddress.ToString());
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _clientSocket.SendTimeout = (int)SocketBase.SendTimeout.TotalMilliseconds;
            // Connect to the specified host.
            var endPoint = new IPEndPoint(serverIpAddress, _globalSession.LegacyServerOptions.Port);
            ConnectAndReceive(_clientSocket, endPoint).ObserveFaults(endPoint);
        }
        catch (Exception ex)
        {
            AuthClientLogMessages.SocketError(_melNet, ex, _sourceFile, _netDirP2S, ex.Message);
            SetAuthResponse(AuthResult.FAIL_INTERNAL_ERROR);
        }
    }

    /// <summary>
    /// Waits for the legacy auth server's verdict. A server that accepts the connection and then
    /// says nothing would otherwise leave this login unanswered for the lifetime of the process.
    /// </summary>
    private async Task<AuthResult> AwaitResponseAsync()
    {
        try
        {
            return await _response.Task.WaitAsync(_loginTimeout);
        }
        catch (TimeoutException)
        {
            AuthClientLogMessages.LoginTimedOut(_melNet, _sourceFile, _netDirP2S,
                _globalSession.LegacyServerOptions.Address, _globalSession.LegacyServerOptions.Port,
                _loginTimeout.TotalSeconds);
            Disconnect();
            return AuthResult.FAIL_INTERNAL_ERROR;
        }
    }

    private void SetAuthResponse(AuthResult response)
    {
        _response.TrySetResult(response);
    }
    
    public void Disconnect()
    {
        if (!IsConnected())
            return;

        _clientSocket.Shutdown(SocketShutdown.Both);
        _clientSocket.Disconnect(false);
    }

    public bool IsConnected()
    {
        return _clientSocket != null && _clientSocket.Connected;
    }

    public byte[] GetSessionKey()
    {
        return _sessionKey;
    }

    // Takes the socket rather than the field: asking for a fresh challenge replaces the field, and
    // the loop of the connection it replaced ends there.
    private async Task ConnectAndReceive(Socket socket, IPEndPoint endPoint)
    {
        try
        {
            await socket.ConnectAsync(endPoint);
            socket.ReceiveBufferSize = 65535;
            SendLogonChallenge();
        }
        catch (Exception ex)
        {
            AuthClientLogMessages.ConnectError(_melNet, ex, _sourceFile, _netDirNone, ex.Message);
            SetAuthResponse(AuthResult.FAIL_INTERNAL_ERROR);
            return;
        }

        byte[] buffer = ArrayPool<byte>.Shared.Rent(MaxMessageSize);
        try
        {
            int pending = 0;
            while (socket == _clientSocket)
            {
                int received = await socket.ReceiveAsync(buffer.AsMemory(pending, MaxMessageSize - pending), SocketFlags.None);
                if (received == 0)
                {
                    SetAuthResponse(AuthResult.FAIL_INTERNAL_ERROR);

                    AuthClientLogMessages.SocketClosedByServer(_melNet, _sourceFile, _netDirS2P);
                    return;
                }

                pending += received;

                int length;
                while (pending > 0 && (length = MessageLength(buffer.AsSpan(0, pending))) > 0)
                {
                    // A server's message can be shorter than what the parsers read, and they get
                    // zeros for the rest, not whatever this buffer last held.
                    Array.Clear(buffer, pending, buffer.Length - pending);
                    HandlePacket(buffer, length);

                    pending -= length;
                    buffer.AsSpan(length, pending).CopyTo(buffer);
                }
            }
        }
        catch (Exception ex)
        {
            AuthClientLogMessages.PacketReadError(_melNet, ex, _sourceFile, _netDirNone, ex.Message);
            SetAuthResponse(AuthResult.FAIL_INTERNAL_ERROR);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// How much of <paramref name="pending"/> is the next message, or 0 if the rest of it has yet
    /// to arrive.
    /// </summary>
    internal static int MessageLength(ReadOnlySpan<byte> pending)
    {
        if ((AuthCommand)pending[0] != AuthCommand.REALM_LIST)
            return pending.Length;

        if (pending.Length < RealmListHeaderSize)
            return 0;

        int length = RealmListHeaderSize + BinaryPrimitives.ReadUInt16LittleEndian(pending[1..]);
        return pending.Length >= length ? length : 0;
    }

    // C P>R: Sends data to realm server
    private void SendPacket(ByteBuffer packet)
    {
        try
        {
            _clientSocket.Send(packet.GetData(), 0, (int)packet.GetSize(), SocketFlags.None);
        }
        catch (Exception ex)
        {
            AuthClientLogMessages.PacketWriteError(_melNet, ex, _sourceFile, _netDirP2S, ex.Message);
            SetAuthResponse(AuthResult.FAIL_INTERNAL_ERROR);
        }
    }

    private void HandlePacket(byte[] buffer, int size)
    {
        ByteBuffer packet = new ByteBuffer(buffer);
        AuthCommand opcode = (AuthCommand)packet.ReadUInt8();
        AuthClientLogMessages.PacketReceived(_melNet, _sourceFile, _netDirS2P, opcode, size);

        switch (opcode)
        {
            case AuthCommand.LOGON_CHALLENGE:
                HandleLogonChallenge(packet);
                break;
            case AuthCommand.LOGON_PROOF:
                HandleLogonProof(packet);
                break;
            case AuthCommand.REALM_LIST:
                HandleRealmList(packet);
                break;
            default:
                AuthClientLogMessages.NoHandlerForOpcode(_melNet, _sourceFile, _netDirS2P, opcode);
                SetAuthResponse(AuthResult.FAIL_INTERNAL_ERROR);
                break;
        }
    }

    private void SendLogonChallenge()
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteUInt8((byte)AuthCommand.LOGON_CHALLENGE);
        buffer.WriteUInt8((byte)(LegacyVersion.ExpansionVersion > 1 ? 8 : 3));
        buffer.WriteUInt16((UInt16)(_username.Length + 30));
        buffer.WriteBytes(Encoding.ASCII.GetBytes("WoW"));
        buffer.WriteUInt8(0);
        buffer.WriteUInt8(LegacyVersion.ExpansionVersion);
        buffer.WriteUInt8(LegacyVersion.MajorVersion);
        buffer.WriteUInt8(LegacyVersion.MinorVersion);
        buffer.WriteUInt16((ushort)LegacyVersion.Build);
        buffer.WriteBytes(Encoding.ASCII.GetBytes(_globalSession.ClientOptions.ReportedPlatform.Reverse()));
        buffer.WriteUInt8(0);
        buffer.WriteBytes(Encoding.ASCII.GetBytes(_globalSession.ClientOptions.ReportedOS.Reverse()));
        buffer.WriteUInt8(0);
        buffer.WriteBytes(Encoding.ASCII.GetBytes(_locale.Reverse()));
        buffer.WriteUInt32(0x3C); // timezone_bias
        buffer.WriteUInt32(0x01_00_00_7F); // IP (127.0.0.1)
        buffer.WriteUInt8((byte)_username.Length);
        buffer.WriteBytes(Encoding.ASCII.GetBytes(_username));
        SendPacket(buffer);
    }

    private void HandleLogonChallenge(ByteBuffer packet)
    {
        byte unk2 = packet.ReadUInt8();
        AuthResult error = (AuthResult)packet.ReadUInt8();
        if (error != AuthResult.SUCCESS)
        {
            AuthClientLogMessages.LoginFailed(_melNet, _sourceFile, _netDirNone, error);
            SetAuthResponse(error);
            return;
        }

        byte[] challenge_B = packet.ReadBytes(32);
        byte challenge_gLen = packet.ReadUInt8();
        byte[] challenge_g = packet.ReadBytes(1);
        byte challenge_nLen = packet.ReadUInt8();
        byte[] challenge_N = packet.ReadBytes(32);
        byte[] challenge_salt = packet.ReadBytes(32);
        byte[] challenge_version = packet.ReadBytes(16);
        byte challenge_securityFlags = packet.ReadUInt8();

        if (!LegacySrp6.IsUnambiguous(challenge_B) && _freshChallenges < MaxFreshChallenges)
        {
            // No proof has gone out yet, so this costs the account nothing on the server.
            ++_freshChallenges;
            AuthClientLogMessages.AskingForAnotherChallenge(_melNet, _sourceFile, _netDirNone, _freshChallenges);
            Socket replaced = _clientSocket;
            Connect();
            replaced.Dispose();
            return;
        }

        LegacySrp6.Proof? proof = LegacySrp6.Compute(_username, _passwordHash, challenge_B, challenge_g, challenge_N, challenge_salt);
        if (proof == null)
        {
            AuthClientLogMessages.ChallengeAllowsNoProof(_melNet, _sourceFile, _netDirNone);
            SetAuthResponse(AuthResult.FAIL_INTERNAL_ERROR);
            return;
        }

        _sessionKey = proof.SessionKey;
        _m2 = proof.ServerEvidence;

        SendLogonProof(proof.PublicKey, proof.ClientEvidence, new byte[20]);
    }

    private void SendLogonProof(byte[] A, byte[] M1, byte[] crc)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteUInt8((byte)AuthCommand.LOGON_PROOF);
        buffer.WriteBytes(A);
        buffer.WriteBytes(M1);
        buffer.WriteBytes(crc);
        buffer.WriteUInt8(0);
        buffer.WriteUInt8(0);

        _debugTraceBreakpointHandler(buffer);

        SendPacket(buffer);
    }

    private void HandleLogonProof(ByteBuffer packet)
    {
        AuthResult error = (AuthResult)packet.ReadUInt8();
        if (error != AuthResult.SUCCESS)
        {
            AuthClientLogMessages.LoginFailed(_melNet, _sourceFile, _netDirNone, error);
            SetAuthResponse(error);
            return;
        }

        byte[] M2 = packet.ReadBytes(20);
        uint accountFlags = 0;
        uint surveyId = 0;
        ushort loginFlags = 0;

        if (LegacyVersion.Build < ClientVersionBuild.V2_0_3_6299)
        {
            surveyId = packet.ReadUInt32();
        }
        else if (LegacyVersion.Build < ClientVersionBuild.V2_4_0_8089)
        {
            surveyId = packet.ReadUInt32();
            loginFlags = packet.ReadUInt16();
        }
        else
        {
            accountFlags = packet.ReadUInt32();
            surveyId = packet.ReadUInt32();
            loginFlags = packet.ReadUInt16();
        }

        bool equal = _m2 != null && _m2!.Length == 20;
        for (int i = 0; equal && i < _m2!.Length; ++i)
            if (!(equal = _m2[i] == M2[i]))
                break;

        if (!equal)
        {
            AuthClientLogMessages.AuthenticationFailed(_melNet, _sourceFile, _netDirNone);
            SetAuthResponse(AuthResult.FAIL_INTERNAL_ERROR);
        }
        else
        {
            AuthClientLogMessages.AuthenticationSucceeded(_melNet, _sourceFile, _netDirNone);
            SetAuthResponse(AuthResult.SUCCESS);
        }
    }

    public void SendRealmListUpdateRequest()
    {
        AuthClientLogMessages.RequestingRealmListUpdate(_melServer, _sourceFile, _netDirNone, _username);
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteUInt8((byte)AuthCommand.REALM_LIST);
        for (int i = 0; i < 4; i++)
            buffer.WriteUInt8(0);
        _realmlistRequestIsPending = true;
        SendPacket(buffer);
    }

    private void HandleRealmList(ByteBuffer packet)
    {
        packet.ReadUInt16(); // packet size
        packet.ReadUInt32(); // unused
        ushort realmsCount = 0;

        if (LegacyVersion.Build < ClientVersionBuild.V2_0_3_6299)
        {
            realmsCount = packet.ReadUInt8();
        }
        else
        {
            realmsCount = packet.ReadUInt16();
        }

        AuthClientLogMessages.ReceivedRealms(_melNet, _sourceFile, _netDirNone, realmsCount);
        List<RealmInfo> realmList = new List<RealmInfo>();

        for (ushort i = 0; i < realmsCount; i++)
        {
            RealmInfo realmInfo = new RealmInfo();
            // Realm IDs are 1-based: WowGuid128.RealmSpecificCreate hardcodes realmId=1 in
            // the high portion of player GUIDs, and the modern 3.4.3 client extracts that to
            // look up the matching VirtualRealmInfo. With realm.Index=0 the lookup fails and
            // the client silently hides characters.
            realmInfo.ID = (uint)(i + 1);

            if (LegacyVersion.Build < ClientVersionBuild.V2_0_3_6299)
            {
                realmInfo.Type = (RealmType)packet.ReadUInt32();
            }
            else
            {
                realmInfo.Type = (RealmType)packet.ReadUInt8();
                realmInfo.IsLocked = packet.ReadUInt8();
            }

            realmInfo.Flags = (RealmFlags)packet.ReadUInt8();
            realmInfo.Name = packet.ReadCString();
            string addressAndPort = packet.ReadCString();
            string[] strArr = addressAndPort.Split(':');
            realmInfo.Address = strArr[0].Trim();
            realmInfo.Port = UInt16.Parse(strArr[1]);
            realmInfo.Population = packet.ReadFloat();
            realmInfo.CharacterCount = packet.ReadUInt8();
            realmInfo.Timezone = packet.ReadUInt8();
            packet.ReadUInt8(); // unk

            if ((realmInfo.Flags & RealmFlags.SpecifyBuild) != 0)
            {
                realmInfo.VersionMajor = packet.ReadUInt8();
                realmInfo.VersionMinor = packet.ReadUInt8();
                realmInfo.VersonBugfix = packet.ReadUInt8();
                realmInfo.Build = packet.ReadUInt16();
            }
            realmList.Add(realmInfo);
        }

        GetSession().RealmManager.UpdateRealms(realmList);
        _hasRealmlist.TrySetResult();
    }

    public void WaitOrRequestRealmList()
    {
        if (_hasRealmlist != null && _hasRealmlist.Task.IsCompletedSuccessfully)
            return;

        if (!IsConnected())
            return;

        if (!_realmlistRequestIsPending)
            SendRealmListUpdateRequest();

        _hasRealmlist?.Task.Wait(TimeSpan.FromSeconds(2));
    }
}
