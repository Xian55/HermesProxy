using System;
using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// A 3.3.5a server keeps a power's regeneration modifiers by power type: mana's whole rate a second,
/// and what an aura adds to the 10 energy a second (AzerothCore Player::UpdateManaRegen and
/// UpdateEnergyRegen). The 3.4.3 and 4.4.2 clients add the same bases from their PowerType table and
/// count the power on their own between updates, from the modifiers of the class's power slot. Without
/// them the 3.4.3 client counted a rogue's energy at 10 a second through Adrenaline Rush, where the
/// server gave 20, and the 4.4.2 client read a druid's mana regeneration as 0.
/// </summary>
/// <remarks>Runs only under a 3.4.3 or a 4.4.2 modern build with a 3.3.5a server (<c>HERMES_TEST_MODERN_BUILD</c>).</remarks>
public sealed class PowerRegenWotLKTests
{
    private static ObjectUpdate DeliverRegen(Class classId, PowerType power, float flat, float interrupted)
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var legacyGuid = new WowGuid64(HighGuidTypeLegacy.Player, 4025);
        harness.SetActivePlayer(legacyGuid);

        int flatField = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_POWER_REGEN_FLAT_MODIFIER) + (int)power;
        int interruptedField = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_POWER_REGEN_INTERRUPTED_FLAT_MODIFIER) + (int)power;
        uint bytes0 = (uint)Race.Human | ((uint)classId << 8) | ((uint)power << 24);
        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_UPDATE_OBJECT, packet =>
        {
            packet.WriteUInt32(1);
            LegacyPacketBuilder.WriteValuesBlock(packet, legacyGuid, LegacyVersion.GetUpdateField(PlayerField.PLAYER_END),
                (LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_BYTES_0), bytes0),
                (flatField, BitConverter.SingleToUInt32Bits(flat)),
                (interruptedField, BitConverter.SingleToUInt32Bits(interrupted)));
        });

        harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, wire, harness.Client.HandleUpdateObject);
        var packet = Assert.Single(harness.ClientWire.Sent.Select(s => s.Packet).OfType<UpdateObject>());
        return Assert.Single(packet.ObjectUpdates);
    }

    private static void SkipUnlessWotLK() =>
        Assert.SkipUnless(ModernVersion.ExpansionVersion >= 3 && LegacyVersion.ExpansionVersion == 3,
            "3.4.3 or 4.4.2 client on a 3.3.5a server");

    [Fact]
    public void RogueEnergy_AdrenalineRush_GoesToTheEnergySlot()
    {
        SkipUnlessWotLK();

        var update = DeliverRegen(Class.Rogue, PowerType.Energy, 10f, 10f);

        Assert.Equal(10f, update.UnitData.PowerRegenFlatModifier[0]);
        Assert.Equal(10f, update.UnitData.PowerRegenInterruptedFlatModifier[0]);
    }

    [Fact]
    public void PriestMana_TheWholeRateGoesToTheManaSlot()
    {
        SkipUnlessWotLK();

        var update = DeliverRegen(Class.Priest, PowerType.Mana, 12.5f, 3.75f);

        Assert.Equal(12.5f, update.UnitData.PowerRegenFlatModifier[0]);
        Assert.Equal(3.75f, update.UnitData.PowerRegenInterruptedFlatModifier[0]);
    }

    [Fact]
    public void DruidEnergy_GoesToTheDruidsEnergySlot()
    {
        SkipUnlessWotLK();

        var update = DeliverRegen(Class.Druid, PowerType.Energy, 10f, 10f);

        sbyte slot = ClassPowerTypes.GetPowerSlotForClass(Class.Druid, PowerType.Energy);
        Assert.Equal(2, slot);
        Assert.Equal(10f, update.UnitData.PowerRegenFlatModifier[slot]);
        Assert.Null(update.UnitData.PowerRegenFlatModifier[0]);
    }
}
