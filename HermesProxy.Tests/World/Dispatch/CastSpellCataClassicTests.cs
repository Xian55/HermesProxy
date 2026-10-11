using System;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// CMSG_CAST_SPELL bodies a 4.4.2 client sent, from a capture. Read the 3.4.3 way, the
/// CraftingFlags byte stood in for the cast flag bits, so the target flags came out 0 and the
/// cast lost its unit target.
/// </summary>
public sealed class CastSpellCataClassicTests
{
    // Rend on a creature: no movement update.
    private const string Rend =
        "01 83 0e 02 c1 bc 00 00 00 00 00 00 00 00 04 03 00 00 58 9d 03 00 00 00 00 00 00 00 00 00 " +
        "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 40 00 00 00 00 20 00 03 a3 5a 15 c0 4a 04 20 00 00";

    // Heroic Strike on a creature, cast while moving: a movement update follows the target.
    private const string HeroicStrikeMoving =
        "01 83 02 82 13 bc 00 00 00 00 00 00 00 00 4e 00 00 00 e3 9c 03 00 00 00 00 00 00 00 00 00 " +
        "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 44 00 00 00 00 20 00 03 a3 3f 15 c0 4a 04 20 " +
        "00 00 03 a0 b7 0f 04 08 00 00 00 00 00 02 00 00 00 00 00 00 49 1e 32 02 87 03 0d c6 d0 8c " +
        "0e c3 43 ab a6 42 85 a5 47 40 56 63 a0 be 00 00 00 00 00 00 00 00 00 00 00 00 00";

    // Frame like the wire: WorldPacket's read-mode ctor consumes a 2-byte opcode prefix.
    private static byte[] Framed(string hex)
    {
        byte[] payload = Convert.FromHexString(hex.Replace(" ", ""));
        byte[] framed = new byte[payload.Length + 2];
        payload.CopyTo(framed, 2);
        return framed;
    }

    private static SpanPacketReader ReaderOver(byte[] framed)
        => new(new WorldPacket(framed).GetRemainingSpan());

    [Fact]
    public void CastSpell_CataClassic_KeepsTheUnitTarget()
    {
        var r = ReaderOver(Framed(Rend));
        CastSpellCodecCataClassic.Read(ref r, out CastSpell packet);

        Assert.Equal(772u, packet.Cast.SpellID);
        Assert.Equal(SpellCastTargetFlags.Unit, packet.Cast.Target.Flags);
        Assert.Equal(new WowGuid128(Low: 0x155AUL, High: 0x2000040000004AC0UL), packet.Cast.Target.Unit);
        Assert.Null(packet.Cast.MoveUpdate);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void CastSpell_CataClassic_WhileMoving_KeepsTheUnitTarget()
    {
        var r = ReaderOver(Framed(HeroicStrikeMoving));
        CastSpellCodecCataClassic.Read(ref r, out CastSpell packet);

        Assert.Equal(78u, packet.Cast.SpellID);
        Assert.Equal(SpellCastTargetFlags.Unit, packet.Cast.Target.Flags);
        Assert.Equal(new WowGuid128(Low: 0x153FUL, High: 0x2000040000004AC0UL), packet.Cast.Target.Unit);
        Assert.NotNull(packet.Cast.MoveUpdate);
    }
}
