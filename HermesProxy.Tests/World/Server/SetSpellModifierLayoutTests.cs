using System;
using System.Buffers;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// SMSG_SET_FLAT_SPELL_MODIFIER and SMSG_SET_PCT_SPELL_MODIFIER carry each value as an int32 up to
/// 4.4.0 and as a float from 4.4.1 (WowPacketParser; TrinityCore cata_classic writes a float). The
/// old layout keeps the old bytes; the new one writes the same amount as a float.
/// </summary>
public sealed class SetSpellModifierLayoutTests
{
    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.IsType<SetSpellModifier.IntValueLayout>(SetSpellModifier.Layouts.For(ClientVersionBuild.V1_14_2_42597));
        Assert.IsType<SetSpellModifier.IntValueLayout>(SetSpellModifier.Layouts.For(ClientVersionBuild.V2_5_3_41750));
        Assert.IsType<SetSpellModifier.IntValueLayout>(SetSpellModifier.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<SetSpellModifier.FloatValueLayout>(SetSpellModifier.Layouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    [Fact]
    public void IntValueLayout_MatchesTheOldWriter()
    {
        var packet = LongAndRapidCharge();

        using var old = new WorldPacket(1u);
        old.WriteInt32(2);
        old.WriteUInt8(5);
        old.WriteInt32(2);
        old.WriteInt32(5);
        old.WriteUInt8(0);
        old.WriteInt32(5);
        old.WriteUInt8(24);
        old.WriteUInt8(11);
        old.WriteInt32(1);
        old.WriteInt32(-1000);
        old.WriteUInt8(0);

        AssertBothSerialisers(new SetSpellModifier.IntValueLayout(), packet, old.GetDataSpan().ToArray());
    }

    [Fact]
    public void FloatValueLayout_WritesEachAmountAsAFloat()
    {
        var packet = LongAndRapidCharge();

        // What TrinityCore 4.3.4 sent for Glyph of Long Charge (+5 yards, class bits 0 and 24) and
        // Glyph of Rapid Charge (-1000 ms), and what TrinityCore cata_classic would send for them.
        byte[] expected = Convert.FromHexString(
            "02000000" + "05" + "02000000" + "0000a040" + "00" + "0000a040" + "18"
                       + "0b" + "01000000" + "00007ac4" + "00");

        AssertBothSerialisers(new SetSpellModifier.FloatValueLayout(), packet, expected);
    }

    [Fact]
    public void WriteToSpan_PastTheCap_FallsBack()
    {
        var packet = new SetSpellModifier(Opcode.SMSG_SET_PCT_SPELL_MODIFIER);
        var mod = new SpellModifierInfo { ModIndex = 0 };
        for (byte i = 0; i < 9; i++)
            mod.ModifierData.Add(new SpellModifierData { ClassIndex = i, ModifierValue = 15 });
        packet.Modifiers.Add(mod);

        byte[] buffer = new byte[packet.MaxSize];
        Assert.Equal(-1, new SetSpellModifier.IntValueLayout().WriteToSpan(packet, buffer));
        Assert.Equal(-1, new SetSpellModifier.FloatValueLayout().WriteToSpan(packet, buffer));
    }

    private static SetSpellModifier LongAndRapidCharge()
    {
        var packet = new SetSpellModifier(Opcode.SMSG_SET_FLAT_SPELL_MODIFIER);

        var range = new SpellModifierInfo { ModIndex = 5 };
        range.ModifierData.Add(new SpellModifierData { ClassIndex = 0, ModifierValue = 5 });
        range.ModifierData.Add(new SpellModifierData { ClassIndex = 24, ModifierValue = 5 });
        packet.Modifiers.Add(range);

        var cooldown = new SpellModifierInfo { ModIndex = 11 };
        cooldown.ModifierData.Add(new SpellModifierData { ClassIndex = 0, ModifierValue = -1000 });
        packet.Modifiers.Add(cooldown);

        return packet;
    }

    private static void AssertBothSerialisers(ServerPacketLayout<SetSpellModifier> layout, SetSpellModifier packet, byte[] expected)
    {
        using var data = new WorldPacket(1u);
        layout.Write(packet, data);
        Assert.Equal(expected, data.GetDataSpan().ToArray());

        byte[] buffer = ArrayPool<byte>.Shared.Rent(packet.MaxSize);
        try
        {
            int written = layout.WriteToSpan(packet, buffer);
            Assert.Equal(expected, buffer.AsSpan(0, written).ToArray());
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
