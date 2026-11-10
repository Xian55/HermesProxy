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
/// CMaNGOS classic numbers elevators and trams 0x6000 since 2026-11-06 (mangos-classic 85e06ac9d).
/// Every other core that gives them a high of their own uses 0xF120, and an older CMaNGOS classic
/// sends them as plain gameobjects. The proxy did not know 0x6000: an elevator in view became a
/// null guid and the exception took its update batch with it. All three have to work, in both
/// directions.
/// </summary>
[Collection("LegacyItemGuidHigh")]
public sealed class LegacyTransportGuidHighTests : IDisposable
{
    // The Undercity lift on CMaNGOS classic.
    private const uint Entry = 20649;
    private const uint Counter = 44892;

    public LegacyTransportGuidHighTests()
    {
        LegacyTransportGuidHigh.Reset();
        LegacyMoTransportGuidHigh.Reset();
    }

    public void Dispose()
    {
        LegacyTransportGuidHigh.Reset();
        LegacyMoTransportGuidHigh.Reset();
    }

    [Theory]
    [InlineData(HighGuidTypeLegacy.Transport)]
    [InlineData(HighGuidTypeLegacy.TransportMangos)]
    public void To64_Elevator_KeepsTheHighItArrivedWith(HighGuidTypeLegacy high)
    {
        var legacy = new WowGuid64(high, Entry, Counter);

        var modern = legacy.To128(null!);

        Assert.True(modern.IsTransport());
        Assert.Equal(legacy, modern.To64());
    }

    [Fact]
    public void To128_SameElevatorUnderEitherHigh_IsTheSameModernGuid()
    {
        var before = new WowGuid64(HighGuidTypeLegacy.Transport, Entry, Counter).To128(null!);
        var after = new WowGuid64(HighGuidTypeLegacy.TransportMangos, Entry, Counter).To128(null!);

        Assert.Equal(before, after);
    }

    /// <summary>A backend swapped for a build from the other side of the commit, proxy left running.</summary>
    [Fact]
    public void To64_BackendChangedItsHigh_FollowsTheLastOneSeen()
    {
        var modern = new WowGuid64(HighGuidTypeLegacy.TransportMangos, Entry, Counter).To128(null!);
        Assert.Equal(HighGuidTypeLegacy.TransportMangos, modern.To64().GetHighGuidTypeLegacy());

        new WowGuid64(HighGuidTypeLegacy.Transport, Entry, Counter).To128(null!);

        Assert.Equal(HighGuidTypeLegacy.Transport, modern.To64().GetHighGuidTypeLegacy());
    }

    /// <summary>The same server changed both highs a commit apart; neither may leak into the other.</summary>
    [Fact]
    public void To64_ElevatorAndShip_EachKeepItsOwnHigh()
    {
        var elevator = new WowGuid64(HighGuidTypeLegacy.TransportMangos, Entry, Counter);
        var ship = new WowGuid64(HighGuidTypeLegacy.MOTransportMangos, 176231);

        var modernElevator = elevator.To128(null!);
        var modernShip = ship.To128(null!);

        Assert.Equal(elevator, modernElevator.To64());
        Assert.Equal(ship, modernShip.To64());
    }

    /// <summary>An older CMaNGOS classic: the elevator is a plain gameobject and stays one.</summary>
    [Fact]
    public void To64_ElevatorSentAsGameObject_IsNotRebuiltWithATransportHigh()
    {
        new WowGuid64(HighGuidTypeLegacy.TransportMangos, Entry, Counter).To128(null!);
        var harness = new LegacyHandlerHarness(recordClientPackets: false);
        var elevator = new WowGuid64(HighGuidTypeLegacy.GameObject, Entry, Counter);

        Assert.Equal(elevator, elevator.To128(harness.Session.GameState).To64());
    }

    /// <summary>The create that threw: through the real handler, as the server sends it.</summary>
    [Fact]
    public void Create_ElevatorUnderThe6000High_ReachesTheClient()
    {
        var elevator = new WowGuid64(HighGuidTypeLegacy.TransportMangos, Entry, Counter);
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(MovementScenarios.ActivePlayer);
        var create = MovementScenarios.ChestCreate with { Guid = elevator, Entry = Entry };

        harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, LegacyCreateWire.Build(create), harness.Client.HandleUpdateObject);

        var update = Assert.Single(harness.ClientWire.Sent
            .Select(s => s.Packet).OfType<UpdateObject>().SelectMany(p => p.ObjectUpdates));
        Assert.True(update.Guid.IsTransport());
        Assert.Equal(elevator, update.Guid.To64());
    }
}
