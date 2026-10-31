using System;
using System.Numerics;
using Xunit;

namespace HermesProxy.Tests.Framework;

public class ExtensionsBigIntegerTests
{
    [Theory]
    [InlineData("7F", 127)]
    [InlineData("80", 128)]
    [InlineData("FF", 255)]
    [InlineData("0080", 32768)]
    [InlineData("0100", 1)]
    public void ToBigInteger_ReadsUnsignedLittleEndian(string hex, int expected)
    {
        Assert.Equal(new BigInteger(expected), Convert.FromHexString(hex).ToBigInteger());
    }

    [Theory]
    [InlineData(0, "")]
    [InlineData(1, "01")]
    [InlineData(127, "7F")]
    [InlineData(128, "80")]
    [InlineData(255, "FF")]
    [InlineData(256, "0001")]
    [InlineData(32768, "0080")]
    public void ToCleanByteArray_HasNoSignByte(int value, string expectedHex)
    {
        Assert.Equal(Convert.FromHexString(expectedHex), new BigInteger(value).ToCleanByteArray());
    }

    // LegacySrp6 hashes these bytes, so a difference between the two is a failed login. The
    // zero top bytes are the case the login fix of #541 is about.
    [Fact]
    public void BothMatchTheHandWrittenOriginals()
    {
        var random = new Random(541);
        for (int i = 0; i < 5000; ++i)
        {
            byte[] bytes = new byte[1 + random.Next(40)];
            random.NextBytes(bytes);
            if (i % 3 == 0)
                bytes[^1] = 0;

            BigInteger value = bytes.ToBigInteger();

            Assert.Equal(OriginalToBigInteger(bytes), value);
            Assert.Equal(OriginalToCleanByteArray(value), value.ToCleanByteArray());
        }
    }

    private static BigInteger OriginalToBigInteger(byte[] array)
    {
        byte[] temp;
        if ((array[array.Length - 1] & 0x80) == 0x80)
        {
            temp = new byte[array.Length + 1];
            temp[array.Length] = 0;
        }
        else
            temp = new byte[array.Length];

        Array.Copy(array, temp, array.Length);
        return new BigInteger(temp);
    }

    private static byte[] OriginalToCleanByteArray(BigInteger b)
    {
        byte[] array = b.ToByteArray();
        if (array[array.Length - 1] != 0)
            return array;

        byte[] temp = new byte[array.Length - 1];
        Array.Copy(array, temp, temp.Length);
        return temp;
    }
}

public class ExtensionsStringTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("0AFF", "0AFF")]
    [InlineData("0a ff", "0AFF")]
    [InlineData("0AF", "0A")]
    public void ParseAsByteArray_ReadsHexPairs(string text, string expectedHex)
    {
        Assert.Equal(Convert.FromHexString(expectedHex), text.ParseAsByteArray());
    }

    [Fact]
    public void ToHexString_IsUpperCaseWithoutSeparators()
    {
        Assert.Equal("0AFF00", ((byte[])[0x0A, 0xFF, 0x00]).ToHexString());
    }

    [Theory]
    [InlineData("", "00")]
    [InlineData("ab", "616200")]
    [InlineData("é", "C3A900")]
    public void ToCString_IsUtf8WithTerminator(string text, string expectedHex)
    {
        Assert.Equal(Convert.FromHexString(expectedHex), text.ToCString());
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("a", "a")]
    [InlineData("enUS", "SUne")]
    public void Reverse_ReversesTheCharacters(string text, string expected)
    {
        Assert.Equal(expected, text.Reverse());
    }

    [Theory]
    [InlineData("Saturday", DayOfWeek.Saturday)]
    [InlineData("nonsense", default(DayOfWeek))]
    public void ToEnum_FallsBackToDefault(string text, DayOfWeek expected)
    {
        Assert.Equal(expected, text.ToEnum<DayOfWeek>());
    }
}
