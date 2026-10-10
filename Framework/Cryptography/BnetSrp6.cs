using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Framework.Cryptography;

/// <summary>
/// The server side of the Battle.net web login's SRP-6, for clients that send a proof instead of
/// their password (4.4.2 on).
/// </summary>
/// <remarks>
/// <para>
/// A port of TrinityCore's <c>Trinity::Crypto::SRP::BnetSRP6v1</c> / <c>BnetSRP6v2</c> with
/// SHA-256 (src/common/Cryptography/Authentication/SRP6.cpp), quirks included, because the client
/// computes the same numbers and a single differing byte fails the proof:
/// </para>
/// <list type="bullet">
/// <item>v1's <c>x</c> reads its SHA-256 digest little-endian; every other hash is read big-endian.</item>
/// <item>v2's <c>x</c> is PBKDF2-HMAC-SHA512 (15,000 rounds) read as a signed 512-bit number, then
/// reduced into [0, N - 1).</item>
/// <item>The evidence hashes take each number at <c>(bits + 8) / 8</c> bytes, so a number whose top
/// byte is full gets a leading zero byte.</item>
/// </list>
/// <para>
/// The verifier is made from the password here, at the start of each login, rather than read from
/// an account table: the proxy has no accounts of its own, only the password it was given.
/// </para>
/// </remarks>
public sealed class BnetSrp6
{
    public const int SaltLength = 32;

    private static readonly BigInteger N1 = ParseHex(
        "86A7F6DEEB306CE519770FE37D556F29944132554DED0BD68205E27F3231FEF5A10108238A3150C59CAF7B0B6478691C" +
        "13A6ACF5E1B5ADAFD4A943D4A21A142B800E8A55F8BFBAC700EB77A7235EE5A609E350EA9FC19F10D921C2FA832E44" +
        "61B7125D38D254A0BE873DFC27858ACB3F8B9F258461E4373BC3A6C2A9634324AB");

    private static readonly BigInteger N2 = ParseHex(
        "AC6BDB41324A9A9BF166DE5E1389582FAF72B6651987EE07FC3192943DB56050A37329CBB4A099ED8193E0757767A13D" +
        "D52312AB4B03310DCD7F48A9DA04FD50E8083969EDB767B0CF6095179A163AB3661A05FBD5FAAAE82918A9962F0B93B8" +
        "55F97993EC975EEAA80D740ADBF4FF747359D041D5C33EA71D281E446B14773BCA97B43A23FB801676BD207A436C6481" +
        "F1D2B9078717461A5B9D32E688F87748544523B524B0D57D5EA77A2775D2ECFA032CFBDBF52FB3786160279004E57AE6" +
        "AF874E7303CE53299CCC041C7BC308D82A5698F3A8D0C38271AE35F8E9DBFBB694B5C803D89F7AE435DE236D525F5475" +
        "9B65E372FCD68EF20FA7111F9E4AFF73");

    private static readonly BigInteger G = 2;

    private readonly BigInteger _b;
    private readonly BigInteger _v;
    private readonly int _paddedLength;
    private bool _used;

    /// <summary>1 or 2.</summary>
    public byte Version { get; }

    /// <summary>PBKDF2 rounds for <c>x</c>: 1 for v1, 15,000 for v2.</summary>
    public uint Iterations => Version == 2 ? 15000u : 1u;

    public BigInteger N { get; }

    public BigInteger Generator => G;

    /// <summary>The name the client hashes into <c>x</c>: uppercase hex SHA-256 of the account name.</summary>
    public string Username { get; }

    public byte[] Salt { get; }

    /// <summary>The server's public ephemeral value, <c>B = k·v + g^b mod N</c>.</summary>
    public BigInteger B { get; }

    private BnetSrp6(byte version, string username, byte[] salt, BigInteger verifier)
    {
        Version = version;
        N = version == 2 ? N2 : N1;
        _paddedLength = version == 2 ? 256 : 128;
        Username = username;
        Salt = salt;
        _v = verifier;

        _b = RandomBelow(N);
        BigInteger k = FromBigEndian(SHA256.HashData(Concat(Pad(N, _paddedLength), Pad(G, _paddedLength))));
        B = (BigInteger.ModPow(G, _b, N) + _v * k) % N;
    }

    /// <summary>
    /// A fresh login for <paramref name="accountName"/> and its password, with a new salt. The
    /// client hashes the <see cref="Username"/> it is sent, so only this side has to agree on how
    /// the account name is cased.
    /// </summary>
    public static BnetSrp6 Create(byte version, string accountName, string password)
    {
        if (version is not (1 or 2))
            throw new ArgumentOutOfRangeException(nameof(version));

        string username = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(accountName)));
        byte[] salt = RandomNumberGenerator.GetBytes(SaltLength);
        return new BnetSrp6(version, username, salt, CalculateVerifier(version, username, password, salt));
    }

    /// <summary><c>v = g^x mod N</c>, with <c>x</c> as the given version derives it.</summary>
    public static BigInteger CalculateVerifier(byte version, string username, string password, byte[] salt)
    {
        BigInteger n = version == 2 ? N2 : N1;
        return BigInteger.ModPow(G, CalculateX(version, username, password, salt, n), n);
    }

    internal static BigInteger CalculateX(byte version, string username, string password, byte[] salt, BigInteger n)
    {
        if (version == 1)
        {
            // v1 uppercases a-z only (TrinityCore: Utf8ToUpperOnlyLatin) and reads the digest
            // little-endian, which is BigNumber's default for a byte array.
            string upper = string.Create(password.Length, password, static (chars, source) =>
            {
                for (int i = 0; i < chars.Length; i++)
                    chars[i] = source[i] is >= 'a' and <= 'z' ? (char)(source[i] - 32) : source[i];
            });
            byte[] inner = SHA256.HashData(Encoding.UTF8.GetBytes(username + ":" + upper));
            return new BigInteger(SHA256.HashData(Concat(salt, inner)), isUnsigned: true, isBigEndian: false);
        }

        byte[] xBytes = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(username + ":" + password), salt,
            15000, HashAlgorithmName.SHA512, 64);
        BigInteger x = FromBigEndian(xBytes);
        if ((xBytes[0] & 0x80) != 0)
            x -= BigInteger.One << 512;

        // BN_nnmod: the remainder is never negative.
        BigInteger m = n - 1;
        return ((x % m) + m) % m;
    }

    /// <summary>
    /// Checks the client's proof <paramref name="clientM1"/> for its public value
    /// <paramref name="a"/>, and returns the shared secret <c>S</c>, or null when the proof is
    /// wrong. One login verifies once.
    /// </summary>
    public BigInteger? VerifyClientEvidence(BigInteger a, BigInteger clientM1)
    {
        if (_used)
            throw new InvalidOperationException("An SRP login verifies only once.");
        _used = true;

        if ((a % N).IsZero)
            return null;

        BigInteger u = FromBigEndian(SHA256.HashData(Concat(Pad(a, _paddedLength), Pad(B, _paddedLength))));
        if ((u % N).IsZero)
            return null;

        BigInteger s = BigInteger.ModPow(a * BigInteger.ModPow(_v, u, N), _b, N);
        return CalculateEvidence(a, B, s) == clientM1 ? s : null;
    }

    /// <summary>The server's proof <c>M2</c> for a login whose client proof checked out.</summary>
    public static BigInteger CalculateServerEvidence(BigInteger a, BigInteger clientM1, BigInteger s)
        => CalculateEvidence(a, clientM1, s);

    /// <summary>SHA-256 over the three numbers in TrinityCore's <c>GetBrokenEvidenceVector</c> encoding.</summary>
    public static BigInteger CalculateEvidence(BigInteger first, BigInteger second, BigInteger third)
        => FromBigEndian(SHA256.HashData(Concat(EvidenceBytes(first), EvidenceBytes(second), EvidenceBytes(third))));

    /// <summary>Big-endian at <c>(bits + 8) / 8</c> bytes.</summary>
    public static byte[] EvidenceBytes(BigInteger value) => Pad(value, (int)((value.GetBitLength() + 8) >> 3));

    /// <summary>OpenSSL's <c>BN_bn2hex</c>: uppercase, two digits per byte, no leading zero bytes.</summary>
    public static string ToHex(BigInteger value)
    {
        if (value.IsZero)
            return "0";
        return Convert.ToHexString(value.ToByteArray(isUnsigned: true, isBigEndian: true));
    }

    /// <summary>A non-negative number from hex of any case and length.</summary>
    public static BigInteger ParseHex(string hex) => BigInteger.Parse("0" + hex, NumberStyles.AllowHexSpecifier);

    /// <summary><see cref="ParseHex"/> for input from the client, which may be missing or malformed.</summary>
    public static bool TryParseHex(string? hex, out BigInteger value)
    {
        value = default;
        return !string.IsNullOrEmpty(hex)
            && BigInteger.TryParse("0" + hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value);
    }

    public static BigInteger FromBigEndian(ReadOnlySpan<byte> bytes) => new(bytes, isUnsigned: true, isBigEndian: true);

    /// <summary>Big-endian, left-padded with zeros to <paramref name="length"/> bytes.</summary>
    public static byte[] Pad(BigInteger value, int length)
    {
        byte[] raw = value.ToByteArray(isUnsigned: true, isBigEndian: true);
        if (value.IsZero)
            raw = [];
        if (raw.Length > length)
            throw new ArgumentException($"{raw.Length}-byte number does not fit {length} bytes.");
        byte[] padded = new byte[length];
        raw.CopyTo(padded, length - raw.Length);
        return padded;
    }

    private static BigInteger RandomBelow(BigInteger n)
    {
        // TrinityCore: SetRand(N.GetNumBits()), then b %= N - 1.
        int bits = (int)n.GetBitLength();
        byte[] bytes = RandomNumberGenerator.GetBytes((bits + 7) / 8);
        int excess = bytes.Length * 8 - bits;
        bytes[0] &= (byte)(0xFF >> excess);
        return FromBigEndian(bytes) % (n - 1);
    }

    private static byte[] Concat(params ReadOnlySpan<byte[]> parts)
    {
        int length = 0;
        foreach (byte[] part in parts)
            length += part.Length;

        byte[] result = new byte[length];
        int offset = 0;
        foreach (byte[] part in parts)
        {
            part.CopyTo(result, offset);
            offset += part.Length;
        }
        return result;
    }
}
