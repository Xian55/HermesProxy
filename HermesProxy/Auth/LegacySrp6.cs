using System;
using System.Security.Cryptography;
using System.Text;
using Framework;

namespace HermesProxy.Auth;

/// <summary>
/// The client's half of the legacy SRP6 login.
/// </summary>
/// <remarks>
/// Cores disagree on the bytes of these numbers. TrinityCore and AzerothCore hash them at full width
/// and skip the zero bytes at the low end of S. CMaNGOS and VMaNGOS trim the zero bytes off the top
/// and keep all of S. So a value with a zero byte at the wrong end only works on one family, and
/// about one login in 30 has one. The way through is not to send such a value: nearly all of them
/// follow from a secret the client picks, so <see cref="Compute"/> picks again until every value
/// reads the same to both families. B is the server's: see <see cref="IsUnambiguous"/>.
/// </remarks>
internal static class LegacySrp6
{
    private const int PublicKeySize = 32;
    private const int SecretSize = 19;

    // A draw fails one time in tens. Running out means the server sent a key that leaves S zero.
    private const int MaxDraws = 64;

    private static readonly BigInteger Multiplier = new(3);

    /// <param name="PublicKey">A, for the proof.</param>
    /// <param name="ClientEvidence">M1, for the proof.</param>
    /// <param name="ServerEvidence">M2, what the server has to answer with.</param>
    /// <param name="SessionKey">K, 40 bytes.</param>
    internal sealed record Proof(byte[] PublicKey, byte[] ClientEvidence, byte[] ServerEvidence, byte[] SessionKey);

    /// <summary>
    /// Whether both families hash this B alike. Nothing here can fix one that isn't, as the server
    /// picked it, but a new connection gets a new one.
    /// </summary>
    public static bool IsUnambiguous(ReadOnlySpan<byte> serverPublicKey) => serverPublicKey[^1] != 0;

    /// <summary>The proof for this challenge, or null if the server's values allow none.</summary>
    public static Proof? Compute(string username, byte[] passwordHash, byte[] serverPublicKey,
        byte[] generator, byte[] modulus, byte[] salt)
    {
        BigInteger b = serverPublicKey.ToBigInteger();
        BigInteger g = generator.ToBigInteger();
        BigInteger n = modulus.ToBigInteger();
        BigInteger x = Sha1Of(salt, passwordHash).ToBigInteger();

        byte[] groupHash = SHA1.HashData(n.ToCleanByteArray());
        byte[] generatorHash = SHA1.HashData(g.ToCleanByteArray());
        for (int i = 0; i < groupHash.Length; ++i)
            groupHash[i] ^= generatorHash[i];

        byte[] userHash = SHA1.HashData(Encoding.ASCII.GetBytes(username.ToUpper()));

        for (int draw = 0; draw < MaxDraws; ++draw)
        {
            BigInteger a = RandomNumberGenerator.GetBytes(SecretSize).ToBigInteger();
            byte[] publicKey = g.ModPow(a, n).ToCleanByteArray();
            if (publicKey.Length != PublicKeySize)
                continue;

            BigInteger u = Sha1Of(publicKey, serverPublicKey).ToBigInteger();
            BigInteger s = ((b + Multiplier * (n - g.ModPow(x, n))) % n).ModPow(a + u * x, n);

            byte[] sData = s.ToCleanByteArray();
            if (sData.Length != PublicKeySize || sData[0] == 0)
                continue;

            byte[] sessionKey = HashSessionKey(sData);
            if (sessionKey[^1] == 0)
                continue;

            byte[] clientEvidence = Sha1Of(groupHash, userHash, salt, publicKey, serverPublicKey, sessionKey);
            if (clientEvidence[^1] == 0)
                continue;

            return new Proof(publicKey, clientEvidence, Sha1Of(publicKey, clientEvidence, sessionKey), sessionKey);
        }

        return null;
    }

    private static byte[] HashSessionKey(byte[] sData)
    {
        byte[] sessionKey = new byte[40];
        byte[] half = new byte[sData.Length / 2];

        for (int start = 0; start < 2; ++start)
        {
            for (int i = 0; i < half.Length; ++i)
                half[i] = sData[i * 2 + start];

            byte[] hash = SHA1.HashData(half);
            for (int i = 0; i < hash.Length; ++i)
                sessionKey[i * 2 + start] = hash[i];
        }

        return sessionKey;
    }

    // Multi-buffer SHA1. IncrementalHash streams each part, so no combined buffer is allocated.
    private static byte[] Sha1Of(params ReadOnlySpan<byte[]> parts)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
        foreach (byte[] part in parts)
            hash.AppendData(part);
        return hash.GetHashAndReset();
    }
}
