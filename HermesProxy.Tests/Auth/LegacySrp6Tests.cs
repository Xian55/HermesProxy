using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using HermesProxy.Auth;
using Xunit;

namespace HermesProxy.Tests.Auth;

/// <summary>
/// <see cref="LegacySrp6"/> against the server side of both families of cores, written from their
/// sources: azerothcore-wotlk <c>SRP6.cpp</c>, as TrinityCore does it, and mangos-wotlk
/// <c>SRP6.cpp</c> with <c>CryptoHash.h</c>, which VMaNGOS follows.
/// </summary>
public class LegacySrp6Tests
{
    private const int Logins = 5000;
    private const string User = "TESTER";

    private static readonly byte[] Modulus = Convert.FromHexString("B79B3E2A87823CAB8F5EBFBF8EB10108535006298B5BADBD5B53E1895E644B89");
    private static readonly byte[] Generator = [7];
    private static readonly byte[] PasswordHash = SHA1.HashData(Encoding.ASCII.GetBytes(User + ":SECRET"));
    private static readonly BigInteger N = Int(Modulus);
    private static readonly BigInteger G = 7;

    private static BigInteger Int(ReadOnlySpan<byte> littleEndian) => new(littleEndian, isUnsigned: true);

    private static byte[] Fixed(BigInteger value, int length)
    {
        byte[] bytes = new byte[length];
        Assert.True(value.TryWriteBytes(bytes, out _, isUnsigned: true));
        return bytes;
    }

    /// <summary>What <c>BigNumber::AsByteArray()</c> gives without a minimum size.</summary>
    private static byte[] Trimmed(BigInteger value) => value.ToByteArray(isUnsigned: true);

    private static byte[] Sha(params byte[][] parts) => SHA1.HashData(parts.SelectMany(part => part).ToArray());

    private static byte[] GroupHash() => Sha(Modulus).Zip(Sha(Generator), (n, g) => (byte)(n ^ g)).ToArray();

    private static byte[] Interleave(byte[] s, int skip)
    {
        byte[] hash0 = SHA1.HashData(Enumerable.Range(0, 16).Select(i => s[2 * i]).Skip(skip).ToArray());
        byte[] hash1 = SHA1.HashData(Enumerable.Range(0, 16).Select(i => s[2 * i + 1]).Skip(skip).ToArray());
        return Enumerable.Range(0, 20).SelectMany(i => new[] { hash0[i], hash1[i] }).ToArray();
    }

    private sealed record Server(byte[] Salt, BigInteger Verifier, BigInteger Secret, byte[] PublicKey);

    /// <summary>Drawn until the server's public key is one both families hash alike.</summary>
    private static Server NewServer()
    {
        while (true)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(32);
            BigInteger verifier = BigInteger.ModPow(G, Int(Sha(salt, PasswordHash)), N);
            BigInteger secret = Int(RandomNumberGenerator.GetBytes(19));
            byte[] publicKey = Fixed((3 * verifier + BigInteger.ModPow(G, secret, N)) % N, 32);

            if (LegacySrp6.IsUnambiguous(publicKey))
                return new Server(salt, verifier, secret, publicKey);
        }
    }

    private static BigInteger SharedSecret(Server server, byte[] a, byte[] hashOfKeys) =>
        BigInteger.ModPow(Int(a) * BigInteger.ModPow(server.Verifier, Int(hashOfKeys), N) % N, server.Secret, N);

    private static void AssertFixedWidthServerAccepts(Server server, LegacySrp6.Proof proof)
    {
        byte[] a = proof.PublicKey;
        byte[] s = Fixed(SharedSecret(server, a, Sha(a, server.PublicKey)), 32);

        int zeroBytes = s.TakeWhile(value => value == 0).Count();
        byte[] key = Interleave(s, (zeroBytes + (zeroBytes & 1)) / 2);
        byte[] m1 = Sha(GroupHash(), Sha(Encoding.ASCII.GetBytes(User)), server.Salt, a, server.PublicKey, key);

        Assert.Equal(32, a.Length);
        Assert.Equal(m1, proof.ClientEvidence);
        Assert.Equal(Sha(a, m1, key), proof.ServerEvidence);
        Assert.Equal(key, proof.SessionKey);
    }

    private static void AssertTrimmingServerAccepts(Server server, LegacySrp6.Proof proof)
    {
        BigInteger a = Int(proof.PublicKey);
        BigInteger b = Int(server.PublicKey);
        BigInteger s = SharedSecret(server, proof.PublicKey, Sha(Trimmed(a), Trimmed(b)));

        BigInteger key = Int(Interleave(Fixed(s, 32), skip: 0));
        byte[] m1 = Sha(Trimmed(Int(GroupHash())), Sha(Encoding.ASCII.GetBytes(User)), Trimmed(Int(server.Salt)),
            Trimmed(a), Trimmed(b), Trimmed(key));

        Assert.Equal(32, proof.PublicKey.Length);
        Assert.Equal(m1, proof.ClientEvidence);
        Assert.Equal(Sha(Trimmed(a), Trimmed(Int(m1)), Trimmed(key)), proof.ServerEvidence);
        Assert.Equal(Trimmed(key), proof.SessionKey);
    }

    private static LegacySrp6.Proof Prove(Server server) =>
        LegacySrp6.Compute(User, PasswordHash, server.PublicKey, Generator, Modulus, server.Salt)!;

    [Fact]
    public void Compute_ManyLogins_FixedWidthServerAcceptsEvery()
    {
        for (int i = 0; i < Logins; i++)
        {
            Server server = NewServer();
            AssertFixedWidthServerAccepts(server, Prove(server));
        }
    }

    // Salts with zero bytes on top are not drawn around: for those a trimming core and the raw bytes
    // the client got differ whatever the client does.
    [Fact]
    public void Compute_ManyLogins_TrimmingServerAcceptsEvery()
    {
        for (int i = 0; i < Logins; i++)
        {
            Server server = NewServer();
            if (server.Salt[^1] == 0)
                continue;

            AssertTrimmingServerAccepts(server, Prove(server));
        }
    }

    [Fact]
    public void Compute_ManyLogins_EveryValueReadsTheSameToBothFamilies()
    {
        for (int i = 0; i < Logins; i++)
        {
            LegacySrp6.Proof proof = Prove(NewServer());

            Assert.Equal(32, proof.PublicKey.Length);
            Assert.NotEqual(0, proof.PublicKey[^1]);
            Assert.NotEqual(0, proof.ClientEvidence[^1]);
            Assert.Equal(40, proof.SessionKey.Length);
            Assert.NotEqual(0, proof.SessionKey[^1]);
        }
    }

    [Fact]
    public void Compute_KeyThatLeavesTheSharedSecretZero_GivesNoProof()
    {
        byte[] salt = new byte[32];
        salt[^1] = 1;
        BigInteger verifier = BigInteger.ModPow(G, Int(Sha(salt, PasswordHash)), N);
        byte[] publicKey = Fixed(3 * verifier % N, 32);

        Assert.Null(LegacySrp6.Compute(User, PasswordHash, publicKey, Generator, Modulus, salt));
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(0x80, true)]
    [InlineData(0, false)]
    public void IsUnambiguous_GoesByTheTopByte(byte topByte, bool expected)
    {
        byte[] publicKey = new byte[32];
        publicKey[0] = 1;
        publicKey[^1] = topByte;

        Assert.Equal(expected, LegacySrp6.IsUnambiguous(publicKey));
    }
}
