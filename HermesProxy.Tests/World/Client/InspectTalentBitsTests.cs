using System;
using HermesProxy.World.Client;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// A 2.4.3 inspect packs a talent position seven to a byte, the 2.5 clients read eight to a byte,
/// and both sides put each talent at the same position.
/// </summary>
public sealed class InspectTalentBitsTests
{
    // A level 61 rogue off cMaNGOS TBC (46/6/0): its 61-byte array starts with these, the rest is zero.
    private static readonly byte[] RogueStart = [0x14, 0x24, 0x18, 0x50, 0x00, 0x0D, 0x40, 0x06, 0x60, 0x14, 0x00, 0x10];

    // The same rogue as the 2.5.3 client showed it, every talent and rank the player's own frame has.
    private static readonly byte[] RogueModern = [0x14, 0x12, 0x06, 0x0A, 0x68, 0x00, 0x0D, 0x60, 0x0A, 0x00, 0x02];

    private static byte[] Convert(ReadOnlySpan<byte> legacy)
    {
        var modern = new byte[InspectTalentBits.ModernLength];
        InspectTalentBits.FromLegacy(legacy, modern);
        return modern;
    }

    [Fact]
    public void ARogue_KeepsEveryTalentAtItsPosition()
    {
        var legacy = new byte[0x3D];
        RogueStart.CopyTo(legacy, 0);

        byte[] modern = Convert(legacy);

        Assert.Equal(RogueModern, modern.AsSpan(0, RogueModern.Length).ToArray());
        Assert.All(modern[RogueModern.Length..], b => Assert.Equal(0, b));
    }

    [Fact]
    public void EachPosition_KeepsItsNumber()
    {
        for (int position = 0; position < InspectTalentBits.ModernLength * 8; position++)
        {
            var legacy = new byte[0x3D];
            legacy[position / 7] = (byte)(1 << (position % 7));

            byte[] modern = Convert(legacy);

            var expected = new byte[InspectTalentBits.ModernLength];
            expected[position / 8] = (byte)(1 << (position % 8));
            Assert.Equal(expected, modern);
        }
    }

    [Fact]
    public void TheHighBit_IsNoPosition()
    {
        byte[] modern = Convert([0x80, 0x80, 0x80]);

        Assert.All(modern, b => Assert.Equal(0, b));
    }

    [Fact]
    public void APositionPastTheClientsArray_IsDropped()
    {
        var legacy = new byte[0x3D];
        legacy[200 / 7] = (byte)(1 << (200 % 7));

        byte[] modern = Convert(legacy);

        Assert.All(modern, b => Assert.Equal(0, b));
    }
}
