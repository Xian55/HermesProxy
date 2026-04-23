using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using HermesProxy.Enums;
using System.Numerics;
using Framework.Constants;
using Framework.Cryptography;
using Framework;
using Framework.IO;
using Framework.Logging;
using HermesProxy.World.Enums;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using Framework.Networking;
using HermesProxy.World.Server;
using System.Collections.Frozen;
using System.Diagnostics;
using HermesProxy.World.Logging;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melLog = Log.CreateMelLogger(Log.CategoryPacket);
    private static readonly Microsoft.Extensions.Logging.ILogger _melNet = Log.CreateMelLogger(Log.CategoryNetwork);
    private static readonly string _sourceFile = nameof(WorldClient).PadRight(15);
    private static readonly string _netDirRecv = Log.FormatDir(LogNetDir.S2P);
    private static readonly string _netDirSend = Log.FormatDir(LogNetDir.P2S);
    private const string _netDirNone = "";

    Socket _clientSocket = null!;
    bool? _isSuccessful;
    uint _queuePosition;
    string _username = null!;
    Realm _realm = null!;
    LegacyWorldCrypt _worldCrypt = null!;
    FrozenDictionary<Opcode, Action<WorldPacket>> _packetHandlers = null!;
    GlobalSessionData _globalSession = null!;
    readonly Lock _sendLock = new();
    Timer? _keepAliveTimer;
    uint _keepAlivePingSerial;
    const int KeepAliveIntervalMs = 30_000;

    // packet order is not always the same as new client, sometimes we need to delay packet until another one
    Dictionary<Opcode, List<WorldPacket>> _delayedPacketsToServer = null!;
    Dictionary<Opcode, List<ServerPacket>> _delayedPacketsToClient = null!;

    public WorldClient()
    {
        InitializePacketHandlers();
    }

    public GlobalSessionData GetSession()
    {
        return _globalSession;
    }

    public GlobalSessionData Session => _globalSession;

    public bool ConnectToWorldServer(Realm realm, GlobalSessionData globalSession)
    {
        _worldCrypt = null!;
        _realm = realm;
        _globalSession = globalSession;
        _username = globalSession.Username;
        _isSuccessful = null;
        _delayedPacketsToServer = new Dictionary<Opcode, List<WorldPacket>>();
        _delayedPacketsToClient = new Dictionary<Opcode, List<ServerPacket>>();

        WorldClientLogMessages.ConnectingToWorldServer(_melNet, _sourceFile, _netDirNone);
        try
        {
            var ip = NetworkUtils.ResolveOrDirectIPv4(realm.ExternalAddress);
            WorldClientLogMessages.WorldServerResolved(_melNet, _sourceFile, _netDirNone, realm.ExternalAddress, realm.Port, ip.ToString());
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Connect to the specified host.
            var endPoint = new IPEndPoint(ip, realm.Port);
            _clientSocket.BeginConnect(endPoint, ConnectCallback, null);
        }
        catch (Exception ex)
        {
            Log.Print(LogType.Error, $"Socket Error: {ex.Message}");
            _isSuccessful = false;
        }

        while (_isSuccessful == null)
        {
            Thread.Sleep(1);
        }

        return (bool)_isSuccessful;
    }

    public bool IsAuthenticated()
    {
        return _isSuccessful == true;
    }

    private void InitializeEncryption(byte[] sessionKey)
    {
        switch (LegacyVersion.Build)
        {
            case ClientVersionBuild.V1_12_1_5875:
            case ClientVersionBuild.V1_12_2_6005:
            case ClientVersionBuild.V1_12_3_6141:
                _worldCrypt = new VanillaWorldCrypt();
                break;
            case ClientVersionBuild.V2_4_3_8606:
                _worldCrypt = new TbcWorldCrypt();
                break;
        }

        if (_worldCrypt != null)
            _worldCrypt.Initialize(sessionKey);
    }

    public void Disconnect()
    {
        StopKeepAliveTimer();

        if (!IsConnected())
            return;

        _clientSocket.Shutdown(SocketShutdown.Both);
        _clientSocket.Disconnect(false);

        if (GetSession().WorldClient == this)
            GetSession().WorldClient = null;
    }

    public bool IsConnected()
    {
        return _clientSocket != null && _clientSocket.Connected;
    }

    public void SetNoDelay(bool enable)
    {
        _clientSocket?.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, enable);
    }

    public uint GetQueuePosition()
    {
        return _queuePosition;
    }

    private void ConnectCallback(IAsyncResult AR)
    {
        try
        {
            WorldClientLogMessages.ConnectionEstablished(_melNet, _sourceFile, _netDirNone);

            _clientSocket.EndConnect(AR);
            _clientSocket.ReceiveBufferSize = 65535;
            _clientSocket.NoDelay = true;

            _ = Task.Run(ReceiveLoop);
        }
        catch (Exception ex)
        {
            Log.Print(LogType.Error, $"Connect Error: {ex.Message}");
            if (_isSuccessful == null)
                _isSuccessful = false;
        }
    }

    private async Task<bool> ReceiveBufferFully(Memory<byte> bufferToFill)
    {
        int alreadyReceived = 0;

        while (alreadyReceived < bufferToFill.Length)
        {
            int received = await _clientSocket.ReceiveAsync(
                bufferToFill[alreadyReceived..],
                SocketFlags.None
            ).ConfigureAwait(false);
            
            if (received == 0)
                return false;

            alreadyReceived += received;
        }

        return true;
    }

    private readonly byte[] _headerBuffer = new byte[LegacyServerPacketHeader.StructSize];

    private void HandleDisconnect(string reason)
    {
        Log.PrintNet(LogType.Error, LogNetDir.S2P, $"Socket Closed By GameWorldServer ({reason})");
        if (_isSuccessful == null)
        {
            _isSuccessful = false;
        }
        else
        {
            Disconnect();
            GetSession().OnDisconnect();
        }
    }

    private async Task ReceiveLoop()
    {
        try
        {
            while (true)
            {
                if (!await ReceiveBufferFully(_headerBuffer.AsMemory()))
                {
                    HandleDisconnect("header");
                    return;
                }

                if (_worldCrypt != null)
                    _worldCrypt.Decrypt(_headerBuffer.AsSpan(0, LegacyServerPacketHeader.StructSize));

                LegacyServerPacketHeader header = new();
                header.Read(_headerBuffer);
                ushort packetSize = header.Size;

                if (packetSize == 0)
                {
                    continue;
                }

                byte[] buffer = new byte[packetSize];

                // copy the opcode into the new buffer
                buffer[0] = _headerBuffer[2];
                buffer[1] = _headerBuffer[3];

                if (!await ReceiveBufferFully(buffer.AsMemory(2, packetSize - 2)))
                {
                    HandleDisconnect("payload");
                    return;
                }

                WorldPacket packet = new WorldPacket(buffer);
                packet.SetReceiveTime(Environment.TickCount);
                HandlePacket(packet);
            }
        }
        catch(Exception e)
        {
            WorldClientLogMessages.PacketReadError(_melLog, e, _sourceFile, _netDirRecv, e.Message);
            if (_isSuccessful == null)
                _isSuccessful = false;
            else
            {
                Disconnect();
                GetSession().OnDisconnect();
            }
        }
    }

    // C P>S: Sends data to world server
    private void SendPacket(WorldPacket packet)
    {
        lock (_sendLock)
        {
            try
            {
                ByteBuffer buffer = new ByteBuffer();
                LegacyClientPacketHeader header = new LegacyClientPacketHeader();

                header.Size = (ushort)(packet.GetSize() + sizeof(uint)); // size includes the opcode
                header.Opcode = packet.GetOpcode();
                header.Write(buffer);

                WorldClientLogMessages.PacketSent(_melLog, _sourceFile, _netDirSend, LegacyVersion.GetUniversalOpcode(header.Opcode), header.Opcode, header.Size);

                byte[] headerArray = buffer.GetData();
                if (_worldCrypt != null)
                    _worldCrypt.Encrypt(headerArray.AsSpan(0, LegacyClientPacketHeader.StructSize));
                buffer.Clear();
                buffer.WriteBytes(headerArray);

                buffer.WriteBytes(packet.GetData(), packet.GetSize());

                _clientSocket.Send(buffer.GetData(), SocketFlags.None);
            }
            catch (Exception ex)
            {
                Log.PrintNet(LogType.Error, LogNetDir.P2S, $"Packet Write Error: {ex.Message}");
                if (_isSuccessful == null)
                    _isSuccessful = false;
            }
        }
    }

    public void SendPacketToClient(ServerPacket packet, Opcode delayUntilOpcode = Opcode.MSG_NULL_ACTION)
    {
        Opcode opcode = packet.GetUniversalOpcode();
        if (delayUntilOpcode != Opcode.MSG_NULL_ACTION)
        {
            if (_delayedPacketsToClient.ContainsKey(delayUntilOpcode))
                _delayedPacketsToClient[delayUntilOpcode].Add(packet);
            else
            {
                List<ServerPacket> packets = new List<ServerPacket>();
                packets.Add(packet);
                _delayedPacketsToClient.Add(delayUntilOpcode, packets);
            }
            return;
        }

        SendPacketToClientDirect(packet);
        SendDelayedPacketsToClientOnOpcode(opcode);
    }

    private void SendPacketToClientDirect(ServerPacket packet)
    {
        var gameState = GetSession().GameState;
        var pendingPackets = gameState.PendingUninstancedPackets;
        var pendingLock = gameState.PendingUninstancedPacketsLock;
        if (packet.GetConnection() == ConnectionType.Realm)
        {
            GetSession().RealmSocket.SendPacket(packet);
        }
        else
        {
            if (GetSession().InstanceSocket == null &&
               !gameState.IsConnectedToInstance)
            {
                lock (pendingLock)
                {
                    if (GetSession().InstanceSocket == null &&
                        !gameState.IsConnectedToInstance)
                    {
                        pendingPackets.Enqueue(packet);
                        Log.PrintNet(LogType.Warn, LogNetDir.P2C, $"Can't send opcode {packet.GetUniversalOpcode()} ({packet.GetOpcode()}) before entering world! Queue");
                        return;
                    }
                }
            }

            // block these packets until connected to instance
            while (GetSession().InstanceSocket == null)
            {
                Log.PrintNet(LogType.Network, LogNetDir.P2C, $"Waiting to send {packet.GetUniversalOpcode()} ({packet.GetOpcode()}).");
                System.Threading.Thread.Sleep(200);
            }

            var socket = GetSession().InstanceSocket;
            if (pendingPackets.Count > 0)
            {
                lock (pendingLock)
                {
                    while (pendingPackets.TryDequeue(out var oldPacket))
                    {
                        socket.SendPacket(oldPacket);
                    }
                }
            }

            socket.SendPacket(packet);
        }
    }

    public void SendPacketToServer(WorldPacket packet, Opcode delayUntilOpcode = Opcode.MSG_NULL_ACTION)
    {
        Opcode opcode = packet.GetUniversalOpcode(false);
        if (delayUntilOpcode != Opcode.MSG_NULL_ACTION)
        {
            if (_delayedPacketsToServer.ContainsKey(delayUntilOpcode))
                _delayedPacketsToServer[delayUntilOpcode].Add(packet);
            else
            {
                List<WorldPacket> packets = new List<WorldPacket>();
                packets.Add(packet);
                _delayedPacketsToServer.Add(delayUntilOpcode, packets);
            }
            return;
        }

        SendPacket(packet);
        SendDelayedPacketsToServerOnOpcode(opcode);
    }

    private void SendDelayedPacketsToServerOnOpcode(Opcode opcode)
    {
        if (_delayedPacketsToServer.ContainsKey(opcode))
        {
            List<WorldPacket> packets = _delayedPacketsToServer[opcode];
            for (int i = packets.Count - 1; i >= 0; i--)
            {
                SendPacket(packets[i]);
                packets.RemoveAt(i);
            }
        }
    }

    private void SendDelayedPacketsToClientOnOpcode(Opcode opcode)
    {
        if (_delayedPacketsToClient.ContainsKey(opcode))
        {
            List<ServerPacket> packets = _delayedPacketsToClient[opcode];
            for (int i = packets.Count - 1; i >= 0; i--)
            {
                SendPacketToClientDirect(packets[i]);
                packets.RemoveAt(i);
            }
        }
    }

    private void HandlePacket(WorldPacket packet)
    {
        Opcode universalOpcode = packet.GetUniversalOpcode(false);
        WorldClientLogMessages.PacketReceived(_melLog, _sourceFile, _netDirRecv, universalOpcode, packet.GetOpcode());

        long startTimestamp = HermesProxy.Server.MetricsEnabled ? System.Diagnostics.Stopwatch.GetTimestamp() : 0;

        switch (universalOpcode)
        {
            case Opcode.SMSG_AUTH_CHALLENGE:
                HandleAuthChallenge(packet);
                break;
            case Opcode.SMSG_AUTH_RESPONSE:
                HandleAuthResponse(packet);
                break;
            case Opcode.SMSG_ADDON_INFO:
                break; // don't need to handle
            default:
                if (_packetHandlers.ContainsKey(universalOpcode))
                {
                    _packetHandlers[universalOpcode](packet);
                }
                else
                {
                    WorldClientLogMessages.NoHandlerForOpcode(_melLog, _sourceFile, _netDirRecv, universalOpcode, packet.GetOpcode());
                    if (_isSuccessful == null)
                        _isSuccessful = false;
                }
                break;
        }

        if (HermesProxy.Server.MetricsEnabled)
        {
            HermesProxy.Server.Metrics.RecordServerToClientLatency(universalOpcode, Stopwatch.GetElapsedTime(startTimestamp).Ticks);
        }

        SendDelayedPacketsToServerOnOpcode(universalOpcode);
    }

    private void HandleAuthChallenge(WorldPacket packet)
    {
        if (LegacyVersion.Build >= ClientVersionBuild.V3_3_5a_12340)
        {
            uint one = packet.ReadUInt32();
        }

        uint seed = packet.ReadUInt32();

        if (LegacyVersion.Build >= ClientVersionBuild.V3_3_5a_12340)
        {
            BigInteger seed1 = packet.ReadBytes(16).ToBigInteger();
            BigInteger seed2 = packet.ReadBytes(16).ToBigInteger();
        }

        var rand = System.Security.Cryptography.RandomNumberGenerator.Create();
        byte[] bytes = new byte[4];
        rand.GetBytes(bytes);
        BigInteger ourSeed = bytes.ToBigInteger();

        SendAuthResponse((uint)ourSeed, seed);
    }

    public void SendAuthResponse(uint clientSeed, uint serverSeed)
    {
        uint zero = 0;

        byte[] authResponse = HashAlgorithm.SHA1.Hash
        (
            Encoding.ASCII.GetBytes(_username.ToUpper()),
            BitConverter.GetBytes(zero),
            BitConverter.GetBytes(clientSeed),
            BitConverter.GetBytes(serverSeed),
            GetSession().AuthClient.GetSessionKey()
        );

        WorldPacket packet = new WorldPacket(Opcode.CMSG_AUTH_SESSION);
        packet.WriteUInt32((uint)LegacyVersion.Build);
        packet.WriteUInt32(_realm.Id.Index);
        packet.WriteBytes(_username.ToUpper().ToCString());

        if (LegacyVersion.Build >= ClientVersionBuild.V3_0_2_9056)
            packet.WriteUInt32(zero); // LoginServerType

        packet.WriteUInt32(clientSeed);

        if (LegacyVersion.Build >= ClientVersionBuild.V3_3_5a_12340)
        {
            packet.WriteUInt32(_realm.Id.Region);
            packet.WriteUInt32(_realm.Id.Site);
            packet.WriteUInt32(_realm.Id.Index);
        }

        if (LegacyVersion.Build >= ClientVersionBuild.V3_2_0_10192)
            packet.WriteUInt64(zero); // DosResponse

        packet.WriteBytes(authResponse);

        // packet.WriteUInt32(zero); // length of addon data
        Span<byte> addonBytes = [208, 1, 0, 0, 120, 156, 117, 207, 61, 14, 194, 48, 12, 5, 224, 114, 14, 184, 12, 97, 64, 149, 154, 133, 150, 25, 153, 196, 173, 172, 38, 78, 21, 82, 126, 58, 113, 66, 206, 68, 81, 133, 24, 98, 188, 126, 126, 79, 182, 114, 52, 77, 16, 237, 105, 59, 154, 68, 129, 143, 101, 177, 242, 183, 77, 85, 204, 163, 190, 166, 32, 37, 135, 45, 161, 179, 154, 152, 60, 12, 210, 18, 177, 37, 238, 230, 130, 87, 102, 187, 224, 207, 144, 170, 208, 9, 185, 197, 26, 188, 39, 9, 35, 180, 73, 188, 105, 175, 235, 49, 94, 241, 33, 227, 72, 206, 42, 224, 94, 212, 146, 47, 3, 154, 79, 237, 58, 183, 132, 190, 14, 166, 199, 180, 252, 146, 167, 53, 152, 24, 102, 121, 102, 114, 0, 178, 51, 196, 12, 26, 112, 200, 242, 27, 77, 4, 139, 117, 79, 206, 253, 99, 98, 140, 178, 145, 71, 13, 12, 29, 198, 159, 190, 1, 43, 0, 141, 195];
        packet.WriteBytes(addonBytes);

        SendPacket(packet);

        InitializeEncryption(GetSession().AuthClient.GetSessionKey());
    }

    private void HandleAuthResponse(WorldPacket packet)
    {
        AuthResult result = (AuthResult)packet.ReadUInt8();

        if (_isSuccessful == null)
        {
            uint billingTimeRemaining = packet.ReadUInt32();
            byte billingFlags = packet.ReadUInt8();
            uint billingTimeRested = packet.ReadUInt32();

            if (LegacyVersion.Build >= ClientVersionBuild.V2_0_1_6180)
            {
                byte expansion = packet.ReadUInt8();
            }
        }

        if (result == AuthResult.AUTH_OK)
        {
            WorldClientLogMessages.AuthenticationSucceeded(_melNet, _sourceFile, _netDirNone);
            if (_queuePosition != 0 && GetSession().RealmSocket != null)
            {
                _queuePosition = 0;
                GetSession().RealmSocket.SendAuthWaitQue(_queuePosition);
            }
            _isSuccessful = true;
            StartKeepAliveTimer();
        }
        else if (result == AuthResult.AUTH_WAIT_QUEUE)
        {
            _queuePosition = packet.ReadUInt32();
            Log.Print(LogType.Network, $"Position in queue is {_queuePosition}.");
            if (_isSuccessful != null && GetSession().RealmSocket != null)
                GetSession().RealmSocket.SendAuthWaitQue(_queuePosition);
            _isSuccessful = true;
        }
        else
        {
            Log.Print(LogType.Network, "Authentication failed!");
            _isSuccessful = false;
        }
    }

    public void SendPing(uint ping, uint latency)
    {
        if (!IsConnected() || _isSuccessful == false)
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_PING);
        packet.WriteUInt32(ping);
        packet.WriteUInt32(latency);
        SendPacket(packet);
    }

    private void StartKeepAliveTimer()
    {
        _keepAliveTimer = new Timer(SendKeepAlivePing, null, KeepAliveIntervalMs, KeepAliveIntervalMs);
    }

    private void StopKeepAliveTimer()
    {
        _keepAliveTimer?.Dispose();
        _keepAliveTimer = null;
    }

    private void SendKeepAlivePing(object? state)
    {
        uint serial = Interlocked.Increment(ref _keepAlivePingSerial);
        SendPing(serial | 0x80000000, 0);
    }

    public void InitializePacketHandlers()
    {
        Dictionary<Opcode, Action<WorldPacket>> dict = [];

        foreach (var methodInfo in typeof(WorldClient).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            foreach (var msgAttr in methodInfo.GetCustomAttributes<PacketHandlerAttribute>())
            {
                if (msgAttr == null)
                    continue;

                if (msgAttr.Opcode == Opcode.MSG_NULL_ACTION)
                    continue;

                if (dict.ContainsKey(msgAttr.Opcode))
                {
                    Log.Print(LogType.Error, $"Tried to override OpcodeHandler of {_packetHandlers[msgAttr.Opcode]} with {methodInfo.Name} (Opcode {msgAttr.Opcode})");
                    continue;
                }

                var parameters = methodInfo.GetParameters();
                if (parameters.Length == 0)
                {
                    Log.Print(LogType.Error, $"Method: {methodInfo.Name} Has no parameters");
                    continue;
                }

                if (parameters[0].ParameterType != typeof(WorldPacket))
                {
                    Log.Print(LogType.Error, $"Method: {methodInfo.Name} has wrong BaseType");
                    continue;
                }

                var del = (Action<WorldPacket>)Delegate.CreateDelegate(typeof(Action<WorldPacket>), this, methodInfo);

                dict[msgAttr.Opcode] = del;
            }
        }

        _packetHandlers = dict.ToFrozenDictionary();
    }
}
