using System;
using System.Security.Cryptography;
using Framework.Cryptography;
using Framework.Serialization;
using Framework.Web;
using Xunit;

namespace HermesProxy.Tests.Framework.Cryptography;

public sealed class BnetSrp6Tests
{
    private const string Account = "TESTACCOUNT";

    /// <summary>
    /// The client's half of the exchange, from nothing but the challenge it is sent and the
    /// password typed into it.
    /// </summary>
    private static (BigInteger A, BigInteger M1, BigInteger S) ClientProof(BnetSrp6 challenge, string password)
    {
        BigInteger n = challenge.N;
        BigInteger g = challenge.Generator;
        int length = challenge.Version == 2 ? 256 : 128;

        BigInteger k = BnetSrp6.FromBigEndian(SHA256.HashData([.. BnetSrp6.Pad(n, length), .. BnetSrp6.Pad(g, length)]));
        BigInteger a = BnetSrp6.FromBigEndian(RandomNumberGenerator.GetBytes(32));
        BigInteger publicA = BigInteger.ModPow(g, a, n);
        BigInteger u = BnetSrp6.FromBigEndian(SHA256.HashData([.. BnetSrp6.Pad(publicA, length), .. BnetSrp6.Pad(challenge.B, length)]));
        BigInteger x = BnetSrp6.CalculateX(challenge.Version, challenge.Username, password, challenge.Salt, n);

        BigInteger baseValue = ((challenge.B - k * BigInteger.ModPow(g, x, n)) % n + n) % n;
        BigInteger s = BigInteger.ModPow(baseValue, a + u * x, n);
        return (publicA, BnetSrp6.CalculateEvidence(publicA, challenge.B, s), s);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void VerifyClientEvidence_RightPassword_ReturnsTheClientsSecret(byte version)
    {
        BnetSrp6 srp = BnetSrp6.Create(version, Account, "Correct horse");
        (BigInteger a, BigInteger m1, BigInteger clientS) = ClientProof(srp, "Correct horse");

        BigInteger? serverS = srp.VerifyClientEvidence(a, m1);

        Assert.Equal(clientS, serverS);
        Assert.Equal(BnetSrp6.CalculateEvidence(a, m1, clientS), BnetSrp6.CalculateServerEvidence(a, m1, serverS!.Value));
    }

    [Theory]
    [InlineData(1, "Wrong horse")]
    [InlineData(2, "Wrong horse")]
    [InlineData(2, "correct horse")]
    public void VerifyClientEvidence_WrongPassword_ReturnsNull(byte version, string typed)
    {
        BnetSrp6 srp = BnetSrp6.Create(version, Account, "Correct horse");
        (BigInteger a, BigInteger m1, _) = ClientProof(srp, typed);

        Assert.Null(srp.VerifyClientEvidence(a, m1));
    }

    [Fact]
    public void VerifyClientEvidence_V1_IgnoresTheCaseOfLatinLetters()
    {
        BnetSrp6 srp = BnetSrp6.Create(1, Account, "Correct horse");
        (BigInteger a, BigInteger m1, _) = ClientProof(srp, "CORRECT HORSE");

        Assert.NotNull(srp.VerifyClientEvidence(a, m1));
    }

    [Fact]
    public void VerifyClientEvidence_SecondProof_Throws()
    {
        BnetSrp6 srp = BnetSrp6.Create(2, Account, "Correct horse");
        (BigInteger a, BigInteger m1, _) = ClientProof(srp, "Correct horse");
        srp.VerifyClientEvidence(a, m1);

        Assert.Throws<InvalidOperationException>(() => srp.VerifyClientEvidence(a, m1));
    }

    [Fact]
    public void VerifyClientEvidence_PublicAZeroModN_ReturnsNull()
    {
        BnetSrp6 srp = BnetSrp6.Create(2, Account, "Correct horse");

        Assert.Null(srp.VerifyClientEvidence(srp.N, BigInteger.One));
    }

    [Fact]
    public void Create_UsernameIsUppercaseHexSha256OfTheAccount()
    {
        BnetSrp6 srp = BnetSrp6.Create(2, Account, "Correct horse");

        Assert.Equal(Convert.ToHexString(SHA256.HashData("TESTACCOUNT"u8)), srp.Username);
        Assert.Equal(BnetSrp6.SaltLength, srp.Salt.Length);
        Assert.Equal(15000u, srp.Iterations);
        Assert.Equal(2048, srp.N.GetBitLength());
    }

    [Theory]
    [InlineData(0x00, "00")]
    [InlineData(0x7F, "7F")]
    [InlineData(0x80, "0080")]   // a full top byte gains a zero byte
    [InlineData(0x0100, "0100")]
    [InlineData(0xFFFF, "00FFFF")]
    public void EvidenceBytes_IsBitLengthPlusEightOverEightBytes(int value, string expectedHex)
        => Assert.Equal(expectedHex, Convert.ToHexString(BnetSrp6.EvidenceBytes(value)));

    [Theory]
    [InlineData(0x02, "02")]
    [InlineData(0x0ABC, "0ABC")]
    [InlineData(0xFF00, "FF00")]
    public void ToHex_IsUppercaseWholeBytesWithoutLeadingZeroBytes(int value, string expected)
        => Assert.Equal(expected, BnetSrp6.ToHex(value));

    [Theory]
    [InlineData("0abc", true, 0x0ABC)]
    [InlineData("FF", true, 0xFF)]
    [InlineData("xyz", false, 0)]
    [InlineData("", false, 0)]
    [InlineData(null, false, 0)]
    public void TryParseHex_ReadsClientHex(string? hex, bool ok, int expected)
    {
        Assert.Equal(ok, BnetSrp6.TryParseHex(hex, out BigInteger value));
        if (ok)
            Assert.Equal(expected, value);
    }

    [Fact]
    public void SrpLoginChallenge_SerializesTheClientsFieldNames()
    {
        string json = Json.CreateString(new SrpLoginChallenge { Version = 2, Iterations = 15000, PublicB = "AB" });

        Assert.Contains("\"version\":2", json);
        Assert.Contains("\"iterations\":15000", json);
        Assert.Contains("\"public_B\":\"AB\"", json);
    }

    [Fact]
    public void LogonResult_WithoutSrp_LeavesServerEvidenceOut()
    {
        Assert.DoesNotContain("server_evidence_M2", Json.CreateString(new LogonResult { AuthenticationState = "DONE" }));
        Assert.Contains("\"server_evidence_M2\":\"AB\"", Json.CreateString(new LogonResult { ServerEvidenceM2 = "AB" }));
    }
}
