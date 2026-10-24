using System.Linq;
using HermesProxy.Enums;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// A 4.3.4 server's SetPower sends SMSG_POWER_UPDATE and writes the same value into the unit's
/// power field, which goes out in the next SMSG_UPDATE_OBJECT. The 4.4.2 client got every such
/// change twice: the forwarded packet and one more the proxy built from the field (as #471 found
/// for 3.4.3 on a 3.3.5a server). A change the field alone carries still goes out.
/// </summary>
/// <remarks>Runs only under a 4.3.4 legacy build (<c>HERMES_TEST_LEGACY_BUILD</c>).</remarks>
public sealed class CataPowerUpdateOnceTests
{
    private static readonly WowGuid64 LegacyPlayer = new(HighGuidTypeLegacy.Player, 5);

    [Fact]
    public void OwnPowerUpdate_RepeatedByTheNextValuesBlock_IsSentOnce()
    {
        if (!WorldClient.IsCataLegacy)
            return;

        var (harness, player) = InWorld();
        harness.Deliver(Opcode.SMSG_POWER_UPDATE, LegacyRunicPower(800), harness.Client.HandlePowerUpdate);
        harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, RunicPowerField(800), harness.Client.HandleUpdateObject);
        harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, RunicPowerField(780), harness.Client.HandleUpdateObject);

        Assert.Equal([800, 780], SentPowers(harness, player));
    }

    [Fact]
    public void OwnPowerUpdate_FollowedByAnotherValue_IsNotHeldForLater()
    {
        if (!WorldClient.IsCataLegacy)
            return;

        var (harness, player) = InWorld();
        harness.Deliver(Opcode.SMSG_POWER_UPDATE, LegacyRunicPower(800), harness.Client.HandlePowerUpdate);
        harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, RunicPowerField(790), harness.Client.HandleUpdateObject);
        harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, RunicPowerField(800), harness.Client.HandleUpdateObject);

        Assert.Equal([800, 790, 800], SentPowers(harness, player));
    }

    private static (LegacyHandlerHarness Harness, WowGuid128 Player) InWorld()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        return (harness, harness.SetActivePlayer(LegacyPlayer));
    }

    // TrinityCore 4.3.4 PowerUpdate: the unit, a count, then (type, value) pairs.
    private static byte[] LegacyRunicPower(int value) => LegacyPacketBuilder.Build(Opcode.SMSG_POWER_UPDATE, packet =>
    {
        packet.WritePackedGuid(LegacyPlayer);
        packet.WriteUInt32(1);
        packet.WriteUInt8((byte)PowerType.RunicPower);
        packet.WriteInt32(value);
    });

    // A death knight's runic power is its first power slot.
    private static byte[] RunicPowerField(uint value) => LegacyPacketBuilder.Build(Opcode.SMSG_UPDATE_OBJECT, packet =>
    {
        uint bytes0 = (uint)Race.Human | ((uint)Class.Deathknight << 8) | ((uint)PowerType.RunicPower << 24);
        packet.WriteUInt16(0); // map
        packet.WriteUInt32(1);
        LegacyPacketBuilder.WriteValuesBlock(packet, LegacyPlayer, LegacyVersion.GetUpdateField(PlayerField.PLAYER_END),
            (LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_BYTES_0), bytes0),
            (LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_POWER1), value));
    });

    private static int[] SentPowers(LegacyHandlerHarness harness, WowGuid128 player)
        => [.. harness.ClientWire.Sent.Select(s => s.Packet).OfType<PowerUpdate>()
            .Where(p => p.Guid == player)
            .SelectMany(p => p.Powers.Where(power => power.PowerType == (byte)PowerType.RunicPower))
            .Select(power => power.Power)];
}
