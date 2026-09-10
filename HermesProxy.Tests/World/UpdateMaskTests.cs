using System;
using System.Collections;
using HermesProxy.World.Client;
using Xunit;

namespace HermesProxy.Tests.World;

// ReadValuesUpdateBlock used to build its update mask as new BitArray(int[]) and then widen it
// through mask.Length. BuildUpdateMask replaces both steps with one allocation; these pin it to
// the exact bits and length the old construction produced, since every field the proxy reads
// from a legacy update is selected by this mask.
public class UpdateMaskTests
{
    private static BitArray Reference(int[] words, int length)
    {
        var mask = new BitArray(words);
        if (mask.Length < length)
            mask.Length = length;
        return mask;
    }

    private static void AssertSame(BitArray expected, BitArray actual)
    {
        Assert.Equal(expected.Length, actual.Length);
        for (int i = 0; i < expected.Length; i++)
            Assert.True(expected[i] == actual[i], $"bit {i}: expected {expected[i]}, got {actual[i]}");
    }

    [Fact]
    public void NoWords_NoLength_IsEmpty()
        => AssertSame(Reference([], 0), WorldClient.BuildUpdateMask([], 0));

    [Fact]
    public void NoWords_WidensToLength()
        => AssertSame(Reference([], 148), WorldClient.BuildUpdateMask([], 148));

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(0x55555555)]
    [InlineData(unchecked((int)0xAAAAAAAA))]
    public void SingleWord_KeepsEveryBit(int word)
        => AssertSame(Reference([word], 32), WorldClient.BuildUpdateMask([word], 32));

    [Fact]
    public void LengthShorterThanWords_KeepsTheWordsLength()
    {
        // BitArray(int[]) is only ever widened, never truncated, so neither is the builder.
        int[] words = [0, 1 << 31, 7];
        AssertSame(Reference(words, 40), WorldClient.BuildUpdateMask(words, 40));
    }

    [Fact]
    public void RandomMasks_MatchReference()
    {
        var rnd = new Random(12345);
        for (int iter = 0; iter < 2000; iter++)
        {
            int n = rnd.Next(0, 48);
            var words = new int[n];
            // Real deltas are sparse: most words are zero, a few carry a handful of bits.
            for (int i = 0; i < n; i++)
                words[i] = rnd.Next(4) == 0 ? rnd.Next(int.MinValue, int.MaxValue) : 0;

            int length = rnd.Next(3) switch
            {
                0 => n * 32,
                1 => n * 32 + rnd.Next(1, 1400), // widened to an object's *_END
                _ => rnd.Next(0, n * 32 + 1),    // shorter than the words
            };

            AssertSame(Reference(words, length), WorldClient.BuildUpdateMask(words, length));
        }
    }
}
