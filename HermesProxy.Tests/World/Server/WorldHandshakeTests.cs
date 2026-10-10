using System;
using System.Security.Cryptography;
using Framework.Cryptography;
using HermesProxy.Enums;
using HermesProxy.World.Server;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// The SHA-256 handshake is the key schedule WorldSocket ran inline before 4.4.2; the oracle
/// below is that code, frozen, so moving it into <see cref="WorldHandshake"/> provably changed
/// nothing for 1.14, 2.5 and 3.4.3.
/// </summary>
public sealed class WorldHandshakeTests
{
    private static readonly byte[] OldAuthCheckSeed = [0xC5, 0xC6, 0x98, 0x95, 0x76, 0x3F, 0x1D, 0xCD, 0xB6, 0xA1, 0x37, 0x28, 0xB3, 0x12, 0xFF, 0x8A];
    private static readonly byte[] OldSessionKeySeed = [0x58, 0xCB, 0xCF, 0x40, 0xFE, 0x2E, 0xCE, 0xA6, 0x5A, 0x90, 0xB8, 0x01, 0x68, 0x6C, 0x28, 0x0B];
    private static readonly byte[] OldContinuedSessionSeed = [0x16, 0xAD, 0x0C, 0xD4, 0x46, 0xF9, 0x4F, 0xB2, 0xEF, 0x7D, 0xEA, 0x2A, 0x17, 0x66, 0x4D, 0x2F];
    private static readonly byte[] OldEncryptionKeySeed = [0xE9, 0x75, 0x3C, 0x50, 0x90, 0x93, 0x61, 0xDA, 0x3B, 0x07, 0xEE, 0xFA, 0xFF, 0x9D, 0x41, 0xB8];
    private static readonly byte[] OldEnableEncryptionSeed = [0x90, 0x9C, 0xD0, 0x50, 0x5A, 0x2C, 0x14, 0xDD, 0x5C, 0x2C, 0xC0, 0x64, 0x14, 0xF3, 0xFE, 0xC9];

    private static readonly WorldHandshake Sha256Handshake = WorldHandshake.Handshakes.For(ClientVersionBuild.V3_4_3_54261);
    private static readonly WorldHandshake Sha512Handshake = WorldHandshake.Handshakes.For(ClientVersionBuild.V4_4_2_60895);

    private static byte[] OldAuthDigest(byte[] bnetKey, byte[] seed, byte[] local, byte[] server)
    {
        Sha256 digestKeyHash = new();
        digestKeyHash.Process(bnetKey, bnetKey.Length);
        digestKeyHash.Finish(seed);
        HmacSha256 hmac = new(digestKeyHash.Digest!);
        hmac.Process(local, local.Length);
        hmac.Process(server, 16);
        hmac.Finish(OldAuthCheckSeed, 16);
        return hmac.Digest![..24];
    }

    private static byte[] OldSessionKey(byte[] bnetKey, byte[] server, byte[] local)
    {
        Sha256 keyData = new();
        keyData.Finish(bnetKey);
        HmacSha256 sessionKeyHmac = new(keyData.Digest!);
        sessionKeyHmac.Process(server, 16);
        sessionKeyHmac.Process(local, local.Length);
        sessionKeyHmac.Finish(OldSessionKeySeed, 16);

        byte[] sessionKey = new byte[40];
        new SessionKeyGenerator(sessionKeyHmac.Digest!, 32).Generate(sessionKey, 40);
        return sessionKey;
    }

    private static byte[] OldEncryptKey(byte[] sessionKey, byte[] local, byte[] server)
    {
        HmacSha256 encryptKeyGen = new(sessionKey);
        encryptKeyGen.Process(local, local.Length);
        encryptKeyGen.Process(server, 16);
        encryptKeyGen.Finish(OldEncryptionKeySeed, 16);
        return encryptKeyGen.Digest![..16];
    }

    private static byte[] OldContinuedDigest(byte[] sessionKey, ulong key, byte[] local, byte[] server)
    {
        HmacSha256 hmac = new(sessionKey);
        hmac.Process(BitConverter.GetBytes(key), 8);
        hmac.Process(local, local.Length);
        hmac.Process(server, 16);
        hmac.Finish(OldContinuedSessionSeed, 16);
        return hmac.Digest![..24];
    }

    private static byte[] OldEnterEncryptedModeDigest(byte[] encryptKey, bool enabled)
    {
        HmacSha256 hash = new(encryptKey);
        hash.Process(BitConverter.GetBytes(enabled), 1);
        hash.Finish(OldEnableEncryptionSeed, 16);
        return hash.Digest!;
    }

    [Fact]
    public void Sha256Handshake_MatchesTheOldInlineKeySchedule()
    {
        byte[] bnetKey = RandomNumberGenerator.GetBytes(64);
        byte[] seed = RandomNumberGenerator.GetBytes(16);
        byte[] server = RandomNumberGenerator.GetBytes(16);
        byte[] local = RandomNumberGenerator.GetBytes(16);
        ulong connectKey = 0x8000_1234_5678_9ABCUL;

        Assert.True(Sha256Handshake.CheckAuthSessionDigest(bnetKey, seed, local, server, OldAuthDigest(bnetKey, seed, local, server)));

        byte[] sessionKey = Sha256Handshake.DeriveSessionKey(bnetKey, server, local);
        Assert.Equal(OldSessionKey(bnetKey, server, local), sessionKey);

        byte[] encryptKey = Sha256Handshake.DeriveEncryptKey(sessionKey, local, server);
        Assert.Equal(OldEncryptKey(sessionKey, local, server), encryptKey);

        Assert.True(Sha256Handshake.CheckContinuedSessionDigest(sessionKey, connectKey, local, server,
            OldContinuedDigest(sessionKey, connectKey, local, server)));
        Assert.Equal(OldEnterEncryptedModeDigest(encryptKey, true), Sha256Handshake.EnterEncryptedModeDigest(encryptKey, true));
    }

    [Fact]
    public void Lengths_FollowTheClientFamily()
    {
        Assert.Equal((16, 16), (Sha256Handshake.ChallengeLength, Sha256Handshake.EncryptKeyLength));
        Assert.Equal((32, 32), (Sha512Handshake.ChallengeLength, Sha512Handshake.EncryptKeyLength));
        Assert.Equal(64, Sha512Handshake.EnterEncryptedModeDigest(new byte[32], true).Length);
    }

    [Fact]
    public void Sha512Handshake_DigestsCheckOut()
    {
        byte[] bnetKey = RandomNumberGenerator.GetBytes(64);
        byte[] seed = RandomNumberGenerator.GetBytes(16);
        byte[] server = RandomNumberGenerator.GetBytes(32);
        byte[] local = RandomNumberGenerator.GetBytes(32);

        byte[] keyHash = SHA512.HashData([.. bnetKey, .. seed]);
        byte[] message = [.. local, .. server, .. AuthCheckSeed512];
        byte[] digest = HMACSHA512.HashData(keyHash, message)[..24];
        Assert.True(Sha512Handshake.CheckAuthSessionDigest(bnetKey, seed, local, server, digest));

        digest[0] ^= 1;
        Assert.False(Sha512Handshake.CheckAuthSessionDigest(bnetKey, seed, local, server, digest));
        Assert.Equal(WorldHandshake.SessionKeyLength, Sha512Handshake.DeriveSessionKey(bnetKey, server, local).Length);
    }

    private static readonly byte[] AuthCheckSeed512 =
    [
        0xDE, 0x3A, 0x2A, 0x8E, 0x6B, 0x89, 0x52, 0x66, 0x88, 0x9D, 0x7E, 0x7A, 0x77, 0x1D, 0x5D, 0x1F,
        0x4E, 0xD9, 0x0C, 0x23, 0x9B, 0xCD, 0x0E, 0xDC, 0xD2, 0xE8, 0x04, 0x3A, 0x68, 0x64, 0xC7, 0xB0,
    ];
}
