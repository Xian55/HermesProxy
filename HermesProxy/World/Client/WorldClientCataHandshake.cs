using System;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;

namespace HermesProxy.World.Client;

/// <summary>
/// The world-server handshake of a Cataclysm 4.x legacy server (TrinityCore 4.3.4,
/// AuthenticationPackets.cpp and WorldSocket.cpp): a plain-text greeting each way, then an auth
/// challenge, session and response whose layouts are all new.
/// </summary>
public partial class WorldClient
{
    private const string ServerConnectionGreeting = "WORLD OF WARCRAFT CONNECTION - SERVER TO CLIENT";
    private const string ClientConnectionGreeting = "WORLD OF WARCRAFT CONNECTION - CLIENT TO SERVER";

    /// <summary>The legacy server speaks the 4.x protocol: greeting, new auth layouts, compression.</summary>
    private static bool IsCataLegacy => LegacyVersion.ExpansionVersion >= 4;

    /// <summary>
    /// Reads the server's greeting and answers with the client's. Each is a big-endian uint16 length
    /// and the text, with no opcode and no encryption; the server sends its auth challenge only
    /// after the answer.
    /// </summary>
    private async ValueTask<bool> ExchangeConnectionGreeting()
    {
        byte[] sizeBytes = new byte[sizeof(ushort)];
        if (!await ReceiveBufferFully(sizeBytes))
            return false;

        ushort size = BinaryPrimitives.ReadUInt16BigEndian(sizeBytes);
        if (size != ServerConnectionGreeting.Length)
        {
            WorldClientLogMessages.UnexpectedConnectionGreeting(_melNet, _sourceFile, _netDirRecv, size);
            return false;
        }

        byte[] text = new byte[size];
        if (!await ReceiveBufferFully(text) || Encoding.ASCII.GetString(text) != ServerConnectionGreeting)
        {
            WorldClientLogMessages.UnexpectedConnectionGreeting(_melNet, _sourceFile, _netDirRecv, size);
            return false;
        }

        byte[] answer = new byte[sizeof(ushort) + ClientConnectionGreeting.Length];
        BinaryPrimitives.WriteUInt16BigEndian(answer, (ushort)ClientConnectionGreeting.Length);
        Encoding.ASCII.GetBytes(ClientConnectionGreeting, answer.AsSpan(sizeof(ushort)));
        lock (_sendLock)
            _clientSocket.Send(answer, SocketFlags.None);

        return true;
    }

    /// <summary>u32[8] DosChallenge, then the 4-byte server seed, then u8 DosZeroBits.</summary>
    private void HandleAuthChallengeCata(WorldPacket packet)
    {
        packet.ReadBytes(8 * sizeof(uint));
        uint serverSeed = packet.ReadUInt32();
        packet.ReadUInt8();

        uint clientSeed = (uint)RandomNumberGenerator.GetInt32(int.MaxValue);
        SendAuthSessionCata(clientSeed, serverSeed);
    }

    /// <summary>
    /// CMSG_AUTH_SESSION, with the SHA-1 digest's bytes scattered between the other fields in the
    /// order TrinityCore 4.3.4's AuthSession::Read takes them, and the account name last behind a
    /// 12-bit length. The digest itself is the 3.3.5a one.
    /// </summary>
    private void SendAuthSessionCata(uint clientSeed, uint serverSeed)
    {
        var authClient = GetSession().AuthClient;
        if (authClient == null)
        {
            WorldClientLogMessages.AuthClientGoneBeforeWorldAuth(_melNet, _sourceFile, _netDirSend);
            _isSuccessful = false;
            return;
        }

        string account = _username.ToUpperInvariant();
        byte[] digest;
        {
            using var ih = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
            ih.AppendData(Encoding.ASCII.GetBytes(account));
            ih.AppendData(BitConverter.GetBytes(0u));
            ih.AppendData(BitConverter.GetBytes(clientSeed));
            ih.AppendData(BitConverter.GetBytes(serverSeed));
            ih.AppendData(authClient.GetSessionKey());
            digest = ih.GetHashAndReset();
        }

        WorldPacket packet = new WorldPacket(Opcode.CMSG_AUTH_SESSION);
        packet.WriteInt32(0);                       // LoginServerID
        packet.WriteUInt32(_realm.Id.Site);         // BattlegroupID
        packet.WriteInt8(0);                        // LoginServerType: grunt
        packet.WriteUInt8(digest[10]);
        packet.WriteUInt8(digest[18]);
        packet.WriteUInt8(digest[12]);
        packet.WriteUInt8(digest[5]);
        packet.WriteUInt64(0);                      // DosResponse
        packet.WriteUInt8(digest[15]);
        packet.WriteUInt8(digest[9]);
        packet.WriteUInt8(digest[19]);
        packet.WriteUInt8(digest[4]);
        packet.WriteUInt8(digest[7]);
        packet.WriteUInt8(digest[16]);
        packet.WriteUInt8(digest[3]);
        packet.WriteUInt16((ushort)LegacyVersion.Build);
        packet.WriteUInt8(digest[8]);
        packet.WriteUInt32(_realm.Id.Index);        // RealmID
        packet.WriteInt8(0);                        // BuildType
        packet.WriteUInt8(digest[17]);
        packet.WriteUInt8(digest[6]);
        packet.WriteUInt8(digest[0]);
        packet.WriteUInt8(digest[1]);
        packet.WriteUInt8(digest[11]);
        packet.WriteUInt32(clientSeed);             // LocalChallenge
        packet.WriteUInt8(digest[2]);
        packet.WriteUInt32(_realm.Id.Region);       // RegionID
        packet.WriteUInt8(digest[14]);
        packet.WriteUInt8(digest[13]);
        packet.WriteUInt32((uint)EmptyAddonInfoBlob.Length);
        packet.WriteBytes(EmptyAddonInfoBlob);
        packet.WriteBit(false);                     // UseIPv6
        packet.WriteBits(account.Length, 12);
        packet.FlushBits();
        packet.WriteString(account);

        SendPacket(packet);

        InitializeEncryption(authClient.GetSessionKey());
    }

    /// <summary>
    /// SMSG_AUTH_RESPONSE: a wait-queue bit (and its FCM bit), a success bit, the success block,
    /// then the result byte and, when queued, the queue position.
    /// </summary>
    private void HandleAuthResponseCata(WorldPacket packet)
    {
        bool hasWaitInfo = packet.HasBit();
        if (hasWaitInfo)
            packet.HasBit();                        // HasFCM
        bool hasSuccessInfo = packet.HasBit();
        packet.ResetBitPos();

        if (hasSuccessInfo)
        {
            packet.ReadUInt32();                    // TimeRemain
            packet.ReadUInt8();                     // ActiveExpansionLevel
            packet.ReadUInt32();                    // TimeSecondsUntilPCKick
            packet.ReadUInt8();                     // AccountExpansionLevel
            packet.ReadUInt32();                    // TimeRested
            packet.ReadUInt8();                     // TimeOptions
        }

        AuthResult result = (AuthResult)packet.ReadUInt8();
        uint queuePosition = hasWaitInfo ? packet.ReadUInt32() : 0;
        ApplyAuthResult(result, queuePosition);
    }
}
