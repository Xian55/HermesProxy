using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// A 3.3.5a server keeps honor in a player field in whole points, which the proxy publishes as
/// currency 1901 in SMSG_SETUP_CURRENCY. The 4.4.2 client counts 1901 in hundredths and divides
/// what it is sent by 100: 200 honor sent as it was read as 2. The 3.4.3 client counts it whole.
/// </summary>
/// <remarks>
/// Runs under a 3.4.3 or a 4.4.2 modern build with a 3.3.5a server (<c>HERMES_TEST_MODERN_BUILD</c>);
/// the older clients have no currency list and a 4.3.4 server sends its own.
/// </remarks>
public sealed class HonorCurrencyWotLKTests
{
    private static bool ProxyPublishesHonor
        => ModernVersion.ExpansionVersion > 1 && !WorldClient.IsCataLegacy;

    private static SetupCurrency.Record DeliverHonor(uint honor)
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var player = new WowGuid64(HighGuidTypeLegacy.Player, 0x2225);
        harness.SetActivePlayer(player);

        harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, LegacyPacketBuilder.Build(Opcode.SMSG_UPDATE_OBJECT, packet =>
        {
            packet.WriteUInt32(1);
            LegacyPacketBuilder.WriteValuesBlock(packet, player, LegacyVersion.GetUpdateField(PlayerField.PLAYER_END),
                (LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_HONOR_CURRENCY), honor));
        }), harness.Client.HandleUpdateObject);

        var list = Assert.Single(harness.ClientWire.Sent.Select(s => s.Packet).OfType<SetupCurrency>());
        return Assert.Single(list.Data, r => r.Type == (uint)Currency.HonorPoints);
    }

    [Fact]
    public void Honor_ReachesThe442ClientInHundredths()
    {
        if (!ProxyPublishesHonor || !ModernVersion.CurrenciesInHundredths)
            return;

        Assert.Equal(20_000u, DeliverHonor(200).Quantity);
    }

    [Fact]
    public void Honor_ReachesThe343ClientInWholePoints()
    {
        if (!ProxyPublishesHonor || ModernVersion.CurrenciesInHundredths)
            return;

        Assert.Equal(200u, DeliverHonor(200).Quantity);
    }
}
