using System;
using System.Security.Cryptography;
using Framework.Cryptography;
using HermesProxy.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server;

/// <summary>
/// The world socket's key schedule: how long the challenges are, the digests that prove the client
/// holds the Battle.net session key, and the session and packet encryption keys derived from it.
/// </summary>
/// <remarks>
/// Clients up to 3.4.x run it on SHA-256 with 16-byte challenges and seeds and an AES-128 key.
/// Cataclysm Classic 4.4 moved every step to SHA-512, with 32-byte challenges and seeds and an
/// AES-256 key; the steps themselves are unchanged (TrinityCore cata_classic, WorldSocket.cpp and
/// AuthenticationPackets.cpp). The two are instances, not types, because only these values differ.
/// </remarks>
public sealed class WorldHandshake
{
    private static readonly WorldHandshake Sha256Handshake = new(
        HashAlgorithmName.SHA256, challengeLength: 16, encryptKeyLength: 16,
        authCheckSeed: [0xC5, 0xC6, 0x98, 0x95, 0x76, 0x3F, 0x1D, 0xCD, 0xB6, 0xA1, 0x37, 0x28, 0xB3, 0x12, 0xFF, 0x8A],
        sessionKeySeed: [0x58, 0xCB, 0xCF, 0x40, 0xFE, 0x2E, 0xCE, 0xA6, 0x5A, 0x90, 0xB8, 0x01, 0x68, 0x6C, 0x28, 0x0B],
        continuedSessionSeed: [0x16, 0xAD, 0x0C, 0xD4, 0x46, 0xF9, 0x4F, 0xB2, 0xEF, 0x7D, 0xEA, 0x2A, 0x17, 0x66, 0x4D, 0x2F],
        encryptionKeySeed: [0xE9, 0x75, 0x3C, 0x50, 0x90, 0x93, 0x61, 0xDA, 0x3B, 0x07, 0xEE, 0xFA, 0xFF, 0x9D, 0x41, 0xB8],
        enableEncryptionSeed: [0x90, 0x9C, 0xD0, 0x50, 0x5A, 0x2C, 0x14, 0xDD, 0x5C, 0x2C, 0xC0, 0x64, 0x14, 0xF3, 0xFE, 0xC9]);

    private static readonly WorldHandshake Sha512Handshake = new(
        HashAlgorithmName.SHA512, challengeLength: 32, encryptKeyLength: 32,
        authCheckSeed:
        [
            0xDE, 0x3A, 0x2A, 0x8E, 0x6B, 0x89, 0x52, 0x66, 0x88, 0x9D, 0x7E, 0x7A, 0x77, 0x1D, 0x5D, 0x1F,
            0x4E, 0xD9, 0x0C, 0x23, 0x9B, 0xCD, 0x0E, 0xDC, 0xD2, 0xE8, 0x04, 0x3A, 0x68, 0x64, 0xC7, 0xB0,
        ],
        sessionKeySeed:
        [
            0xE8, 0x1E, 0x8B, 0x59, 0x27, 0x62, 0x1E, 0xAA, 0x86, 0x15, 0x18, 0xEA, 0xC0, 0xBF, 0x66, 0x8C,
            0x6D, 0xBF, 0x83, 0x93, 0xBC, 0xAA, 0x80, 0x52, 0x5B, 0x1E, 0xDC, 0x23, 0xA0, 0x12, 0xB7, 0x50,
        ],
        continuedSessionSeed:
        [
            0x56, 0x5C, 0x61, 0x9C, 0x48, 0x3A, 0x52, 0x1F, 0x61, 0x5D, 0x05, 0x49, 0xB2, 0x9A, 0x39, 0xBF,
            0x4B, 0x97, 0xB0, 0x1B, 0xF9, 0x6C, 0xDE, 0xD6, 0x80, 0x1D, 0xAB, 0x26, 0x02, 0xA9, 0x9B, 0x9D,
        ],
        encryptionKeySeed:
        [
            0x71, 0xC9, 0xED, 0x5A, 0xA7, 0x0E, 0x4D, 0xFF, 0x4C, 0x36, 0xA6, 0x5A, 0x3E, 0x46, 0x8A, 0x4A,
            0x5D, 0xA1, 0x48, 0xC8, 0x30, 0x47, 0x4A, 0xDE, 0xF6, 0x0D, 0x6C, 0xBE, 0x6F, 0xE4, 0x55, 0x73,
        ],
        enableEncryptionSeed:
        [
            0x66, 0xBE, 0x29, 0x79, 0xEF, 0xF2, 0xD5, 0xB5, 0x61, 0x53, 0xF6, 0x5F, 0x45, 0xAE, 0x81, 0xCB,
            0x32, 0xEC, 0x94, 0xEC, 0x75, 0xB3, 0x5F, 0x44, 0x6A, 0x63, 0x43, 0x67, 0x17, 0x20, 0x44, 0x34,
        ]);

    internal static readonly ServerPacketLayouts<WorldHandshake> Handshakes = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V4_4_2_60895, Sha256Handshake),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, Sha512Handshake));

    /// <summary>The running client's handshake.</summary>
    public static readonly WorldHandshake Current = Handshakes.ForRunningClient();

    /// <summary>The length of the session key both sides keep after the handshake.</summary>
    public const int SessionKeyLength = 40;

    private readonly HashAlgorithmName _hash;
    private readonly byte[] _authCheckSeed;
    private readonly byte[] _sessionKeySeed;
    private readonly byte[] _continuedSessionSeed;
    private readonly byte[] _encryptionKeySeed;
    private readonly byte[] _enableEncryptionSeed;

    /// <summary>Length of the server's challenge and of the client's local challenge.</summary>
    public int ChallengeLength { get; }

    /// <summary>Length of the AES-GCM packet key.</summary>
    public int EncryptKeyLength { get; }

    private WorldHandshake(HashAlgorithmName hash, int challengeLength, int encryptKeyLength, byte[] authCheckSeed,
        byte[] sessionKeySeed, byte[] continuedSessionSeed, byte[] encryptionKeySeed, byte[] enableEncryptionSeed)
    {
        _hash = hash;
        ChallengeLength = challengeLength;
        EncryptKeyLength = encryptKeyLength;
        _authCheckSeed = authCheckSeed;
        _sessionKeySeed = sessionKeySeed;
        _continuedSessionSeed = continuedSessionSeed;
        _encryptionKeySeed = encryptionKeySeed;
        _enableEncryptionSeed = enableEncryptionSeed;
    }

    /// <summary>
    /// Checks the digest of CMSG_AUTH_SESSION, which the client keys with its Battle.net session
    /// key and the auth seed built into its executable.
    /// </summary>
    public bool CheckAuthSessionDigest(ReadOnlySpan<byte> bnetSessionKey, ReadOnlySpan<byte> buildSeed,
        ReadOnlySpan<byte> localChallenge, ReadOnlySpan<byte> serverChallenge, ReadOnlySpan<byte> digest)
    {
        using IncrementalHash keyHash = IncrementalHash.CreateHash(_hash);
        keyHash.AppendData(bnetSessionKey);
        keyHash.AppendData(buildSeed);

        return DigestMatches(Hmac(keyHash.GetHashAndReset(), localChallenge, serverChallenge, _authCheckSeed), digest);
    }

    /// <summary>The session key of a realm connection, from the Battle.net session key.</summary>
    public byte[] DeriveSessionKey(ReadOnlySpan<byte> bnetSessionKey, ReadOnlySpan<byte> serverChallenge, ReadOnlySpan<byte> localChallenge)
    {
        byte[] keyData = CryptographicOperations.HashData(_hash, bnetSessionKey);
        byte[] seed = Hmac(keyData, serverChallenge, localChallenge, _sessionKeySeed);

        byte[] sessionKey = new byte[SessionKeyLength];
        new SessionKeyGenerator(seed, _hash).Generate(sessionKey);
        return sessionKey;
    }

    /// <summary>Checks the digest of CMSG_AUTH_CONTINUED_SESSION, keyed with the realm connection's session key.</summary>
    public bool CheckContinuedSessionDigest(ReadOnlySpan<byte> sessionKey, ulong connectKey,
        ReadOnlySpan<byte> localChallenge, ReadOnlySpan<byte> serverChallenge, ReadOnlySpan<byte> digest)
    {
        Span<byte> key = stackalloc byte[sizeof(ulong)];
        BitConverter.TryWriteBytes(key, connectKey);
        return DigestMatches(Hmac(sessionKey, key, localChallenge, serverChallenge, _continuedSessionSeed), digest);
    }

    /// <summary>The AES-GCM packet key of one connection.</summary>
    public byte[] DeriveEncryptKey(ReadOnlySpan<byte> sessionKey, ReadOnlySpan<byte> localChallenge, ReadOnlySpan<byte> serverChallenge)
        => Hmac(sessionKey, localChallenge, serverChallenge, _encryptionKeySeed).AsSpan(0, EncryptKeyLength).ToArray();

    /// <summary>What SMSG_ENTER_ENCRYPTED_MODE signs.</summary>
    public byte[] EnterEncryptedModeDigest(ReadOnlySpan<byte> encryptKey, bool enabled)
        => Hmac(encryptKey, [enabled ? (byte)1 : (byte)0], _enableEncryptionSeed);

    private byte[] Hmac(ReadOnlySpan<byte> key, ReadOnlySpan<byte> first, ReadOnlySpan<byte> second,
        ReadOnlySpan<byte> third = default, ReadOnlySpan<byte> fourth = default)
    {
        using IncrementalHash hmac = IncrementalHash.CreateHMAC(_hash, key);
        hmac.AppendData(first);
        hmac.AppendData(second);
        hmac.AppendData(third);
        hmac.AppendData(fourth);
        return hmac.GetHashAndReset();
    }

    // The client sends the first 24 bytes of the HMAC.
    private static bool DigestMatches(ReadOnlySpan<byte> hmac, ReadOnlySpan<byte> digest)
        => digest.Length <= hmac.Length && CryptographicOperations.FixedTimeEquals(hmac[..digest.Length], digest);
}
