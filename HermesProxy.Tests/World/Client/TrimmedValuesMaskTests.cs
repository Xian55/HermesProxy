using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// A Values block whose update mask stops at the highest word it needed, rather than spanning the
/// whole object the way TrinityCore, AzerothCore and cMaNGOS all send it
/// (<c>UpdateMask::SetCount(m_valuesCount)</c>).
///
/// Both masks the reader hands on are indexed by field id by the ~200 reads in
/// <c>StoreObjectUpdateInternal</c> and by the collision-height hook, so a short one used to throw
/// <see cref="System.ArgumentOutOfRangeException"/> — and because that escapes
/// <c>HandleUpdateObject</c>, it took every other object block in the same packet with it.
/// </summary>
public class TrimmedValuesMaskTests
{
    private static int Field(UnitField field) => LegacyVersion.GetUpdateField(field);

    private static byte[] OneBlock(WowGuid64 guid, UnitField field, uint value)
        => LegacyPacketBuilder.Build(Opcode.SMSG_UPDATE_OBJECT, packet =>
        {
            packet.WriteUInt32(1);
            LegacyPacketBuilder.WriteValuesBlock(packet, guid, Field(field) + 1, (Field(field), value));
        });

    /// <summary>The player's own block: the one the collision-height hook reads past the mask for.</summary>
    [Fact]
    public void KnownPlayer_TrimmedMask_StillSendsThePowerUpdate()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var scenario = new AlteracValleyScenario(harness);

        harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, OneBlock(scenario.LegacyPlayer, UnitField.UNIT_FIELD_POWER1, 4321),
            harness.Client.HandleUpdateObject);

        var power = Assert.Single(harness.ClientWire.Sent.Select(s => s.Packet).OfType<PowerUpdate>());
        Assert.Equal(4321, Assert.Single(power.Powers).Power);
    }

    /// <summary>
    /// A guid with no create behind it: the mask is not widened from a cached field set either, so
    /// every one of the field reads in the translator was exposed, not just the hook's four.
    /// </summary>
    [Fact]
    public void UnknownPlayer_TrimmedMask_StillSendsThePowerUpdate()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var legacyGuid = new WowGuid64(HighGuidTypeLegacy.Player, 77);
        harness.Session.GameState.CurrentPlayerGuid = legacyGuid.To128(harness.Session.GameState);

        harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, OneBlock(legacyGuid, UnitField.UNIT_FIELD_POWER1, 1234),
            harness.Client.HandleUpdateObject);

        var power = Assert.Single(harness.ClientWire.Sent.Select(s => s.Packet).OfType<PowerUpdate>());
        Assert.Equal(1234, Assert.Single(power.Powers).Power);
    }

    /// <summary>
    /// A block that declares no mask words at all. Every object type starts with the object
    /// section, so even <c>OBJECT_FIELD_GUID</c> — the first thing the translator reads — sits
    /// past a zero-length mask.
    /// </summary>
    [Fact]
    public void ZeroWordMask_IsDroppedWithoutTakingThePacketDown()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var legacyGuid = new WowGuid64(HighGuidTypeLegacy.Creature, 1234, 55);
        harness.AddKnownObject(legacyGuid, ObjectType.Unit);

        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_UPDATE_OBJECT, packet =>
        {
            packet.WriteUInt32(1);
            packet.WriteUInt8((byte)UpdateTypeLegacy.Values);
            packet.WritePackedGuid(legacyGuid);
            packet.WriteUInt8(0); // no mask words, so no values follow
        });

        harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, wire, harness.Client.HandleUpdateObject);
    }

    /// <summary>
    /// A guid whose high the proxy does not map resolves to <see cref="ObjectType.Object"/>, which
    /// neither writer can build. The block is dropped at ingest; it used to survive translation and
    /// throw inside the writer at send time, losing the batch it travelled with.
    /// </summary>
    [Fact]
    public void UnwritableObjectType_IsDroppedWithoutReachingTheWriter()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var legacyGuid = new WowGuid64(HighGuidTypeLegacy.Group, 4242);

        harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, OneBlock(legacyGuid, UnitField.UNIT_FIELD_HEALTH, 500),
            harness.Client.HandleUpdateObject);

        Assert.DoesNotContain(harness.ClientWire.Sent, s => s.Opcode == Opcode.SMSG_UPDATE_OBJECT);
    }

    /// <summary>
    /// The batch still ships the blocks around an unwritable one.
    /// </summary>
    [Fact]
    public void UnwritableObjectType_DoesNotCostTheRestOfTheBatch()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var creature = new WowGuid64(HighGuidTypeLegacy.Creature, 1234, 91);
        var unwritable = new WowGuid64(HighGuidTypeLegacy.Group, 4243);
        harness.AddKnownObject(creature, ObjectType.Unit);

        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_UPDATE_OBJECT, packet =>
        {
            packet.WriteUInt32(2);
            LegacyPacketBuilder.WriteValuesBlock(packet, unwritable, Field(UnitField.UNIT_FIELD_HEALTH) + 1,
                (Field(UnitField.UNIT_FIELD_HEALTH), 500u));
            LegacyPacketBuilder.WriteValuesBlock(packet, creature, LegacyVersion.GetUpdateField(UnitField.UNIT_END),
                (Field(UnitField.UNIT_FIELD_HEALTH), 777u));
        });

        harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, wire, harness.Client.HandleUpdateObject);

        Assert.Contains(harness.ClientWire.Sent, s => s.Opcode == Opcode.SMSG_UPDATE_OBJECT);
    }

    [Fact]
    public void UnknownCreature_TrimmedMask_StillSendsTheUpdate()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var legacyGuid = new WowGuid64(HighGuidTypeLegacy.Creature, 1234, 88);

        harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, OneBlock(legacyGuid, UnitField.UNIT_FIELD_HEALTH, 500),
            harness.Client.HandleUpdateObject);

        Assert.Contains(harness.ClientWire.Sent, s => s.Opcode == Opcode.SMSG_UPDATE_OBJECT);
    }
}
