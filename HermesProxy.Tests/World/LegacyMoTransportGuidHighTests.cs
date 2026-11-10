using System;
using System.Linq;
using HermesProxy.Enums;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// CMaNGOS classic numbers ships and zeppelins 0x8000 since 2026-11-06, every other core and every
/// older CMaNGOS 0x1FC0. The proxy knew only the second: a ship in view became a null guid and the
/// exception took its update batch with it (issue #590). Both have to work, in both directions.
/// </summary>
[Collection("LegacyItemGuidHigh")]
public sealed class LegacyMoTransportGuidHighTests : IDisposable
{
    // CMaNGOS uses the gameobject entry as the counter.
    private const uint Counter = 175080;

    public LegacyMoTransportGuidHighTests() => LegacyMoTransportGuidHigh.Reset();

    public void Dispose() => LegacyMoTransportGuidHigh.Reset();

    [Theory]
    [InlineData(HighGuidTypeLegacy.MOTransport)]
    [InlineData(HighGuidTypeLegacy.MOTransportMangos)]
    public void To64_Ship_KeepsTheHighItArrivedWith(HighGuidTypeLegacy high)
    {
        var legacy = new WowGuid64(high, Counter);

        var modern = legacy.To128(null!);

        Assert.True(modern.IsTransport());
        Assert.Equal(legacy, modern.To64());
    }

    [Fact]
    public void To128_SameShipUnderEitherHigh_IsTheSameModernGuid()
    {
        var before = new WowGuid64(HighGuidTypeLegacy.MOTransport, Counter).To128(null!);
        var after = new WowGuid64(HighGuidTypeLegacy.MOTransportMangos, Counter).To128(null!);

        Assert.Equal(before, after);
    }

    /// <summary>A backend swapped for a build from the other side of the commit, proxy left running.</summary>
    [Fact]
    public void To64_BackendChangedItsHigh_FollowsTheLastOneSeen()
    {
        var modern = new WowGuid64(HighGuidTypeLegacy.MOTransportMangos, Counter).To128(null!);
        Assert.Equal(HighGuidTypeLegacy.MOTransportMangos, modern.To64().GetHighGuidTypeLegacy());

        new WowGuid64(HighGuidTypeLegacy.MOTransport, Counter).To128(null!);

        Assert.Equal(HighGuidTypeLegacy.MOTransport, modern.To64().GetHighGuidTypeLegacy());
    }

    [Fact]
    public void To64_Elevator_IsNotRebuiltWithTheShipHigh()
    {
        new WowGuid64(HighGuidTypeLegacy.MOTransportMangos, Counter).To128(null!);
        var elevator = new WowGuid64(HighGuidTypeLegacy.Transport, 0, 6);

        Assert.Equal(elevator, elevator.To128(null!).To64());
    }

    /// <summary>The create that threw: through the real handler, as the server sends it.</summary>
    [Fact]
    public void Create_ShipUnderThe8000High_ReachesTheClient()
    {
        var ship = new WowGuid64(HighGuidTypeLegacy.MOTransportMangos, Counter);
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(MovementScenarios.ActivePlayer);
        var create = MovementScenarios.Creates().First(c => c.Name == "mo-transport").Create with { Guid = ship };

        harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, LegacyCreateWire.Build(create), harness.Client.HandleUpdateObject);

        var update = Assert.Single(harness.ClientWire.Sent
            .Select(s => s.Packet).OfType<UpdateObject>().SelectMany(p => p.ObjectUpdates));
        Assert.True(update.Guid.IsTransport());
        Assert.Equal(ship, update.Guid.To64());
    }
}
