using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

public class FriendlyAuraFlagsTests
{
    [Theory]
    // AzerothCore clears Positive for a paladin's aura on an ally: effects only.
    // Beneficial, but never Cancelable: it is the other paladin's to cancel.
    [InlineData(0x01, true, 0x0100)]
    [InlineData(0x01, false, 0x0100)]
    [InlineData(0x11, true, 0x0102)] // ordinary positive/self aura
    [InlineData(0x11, false, 0x0100)]
    [InlineData(0x81, true, 0x0010)] // real harmful aura
    [InlineData(0x81, false, 0x0010)]
    public void WotlkAura_PreservesBeneficialAndHarmfulClassificationOnWire(byte legacyFlags,
        bool onLocalPlayer, ushort expectedFlags)
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var player = new WowGuid64(HighGuidTypeLegacy.Player, 77);
        var ally = new WowGuid64(HighGuidTypeLegacy.Player, 78);
        var target = onLocalPlayer ? player : ally;
        harness.SetActivePlayer(player);
        harness.AddKnownObject(ally, ObjectType.Player);
        var wire = BuildAura(target, ally, legacyFlags, 54043);
        harness.Deliver(Opcode.SMSG_AURA_UPDATE, wire, harness.Client.HandleAuraUpdate);
        var sent = Assert.Single(harness.ClientWire.Sent);
        var update = Assert.IsType<AuraUpdate>(sent.Packet);
        var aura = Assert.Single(update.Auras).AuraData;
        Assert.Equal(54043u, aura.SpellID); // Retribution Aura rank 7
        Assert.Equal(ally.To128(harness.Session.GameState), aura.CastUnit);
        Assert.Equal(1u, aura.ActiveFlags);

        // Independently decode SMSG_AURA_UPDATE through the actual 16-bit flags.
        using var reader = new WorldPacket(1, sent.Bytes);
        Assert.False(reader.ReadBit()); // not UpdateAll
        Assert.Equal(1u, reader.ReadBits<uint>(9));
        Assert.Equal((byte)0, reader.ReadUInt8());
        Assert.True(reader.ReadBit());
        reader.ResetBitPos();
        reader.ReadPackedGuid128(); // cast ID
        Assert.Equal(54043u, reader.ReadUInt32());
        reader.ReadUInt32(); // spell visual
        Assert.Equal(expectedFlags, reader.ReadUInt16());
    }

    [Fact]
    public void AuraRemoval_StillRemovesTheSameSlot()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var player = new WowGuid64(HighGuidTypeLegacy.Player, 77);
        harness.SetActivePlayer(player);
        harness.Deliver(Opcode.SMSG_AURA_UPDATE, BuildAura(player, player, 1, 54043), harness.Client.HandleAuraUpdate);
        harness.Deliver(Opcode.SMSG_AURA_UPDATE, BuildAura(player, player, 0, 0), harness.Client.HandleAuraUpdate);
        var removed = Assert.IsType<AuraUpdate>(harness.ClientWire.Sent.Last().Packet);
        Assert.Null(Assert.Single(removed.Auras).AuraData);
        Assert.Empty(harness.Session.GameState.KnownAuras[player.To128(harness.Session.GameState)]);
    }

    private static byte[] BuildAura(WowGuid64 target, WowGuid64 caster, byte flags, uint spellId)
        => LegacyPacketBuilder.Build(Opcode.SMSG_AURA_UPDATE, packet =>
        {
            packet.WritePackedGuid(target);
            packet.WriteUInt8(0);
            packet.WriteUInt32(spellId);
            if (spellId == 0) return;
            packet.WriteUInt8(flags);
            packet.WriteUInt8(80);
            packet.WriteUInt8(1);
            packet.WritePackedGuid(caster);
        });
}
