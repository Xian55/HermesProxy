using System.Collections.Generic;
using System.Linq;
using HermesProxy.World;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World;

public class TransportCreateOrderingTests
{
    private static WowGuid128 Transport(uint counter) =>
        WowGuid128.Create(new WowGuid64(HighGuidTypeLegacy.MOTransport, counter), null!);

    private static WowGuid128 Player(ulong counter) =>
        WowGuid128.Create(HighGuidType703.Player, counter);

    private static WowGuid128 Creature(uint entry, ulong counter) =>
        WowGuid128.Create(HighGuidType703.Creature, 0, entry, counter);

    private static List<WowGuid128> Order(params WowGuid128[] guids) =>
        TransportCreateOrdering.TransportsFirst(guids.ToList(), g => g);

    [Fact]
    public void TransportIsHoistedAheadOfThePlayerRidingIt()
    {
        var player = Player(1);
        var ship = Transport(6);

        var ordered = Order(player, ship);

        Assert.Equal(ship, ordered[0]);
        Assert.Equal(player, ordered[1]);
    }

    [Fact]
    public void RelativeOrderOfNonTransportsIsPreserved()
    {
        var a = Creature(100, 1);
        var b = Creature(200, 2);
        var ship = Transport(6);
        var c = Creature(300, 3);

        var ordered = Order(a, b, ship, c);

        Assert.Equal(ship, ordered[0]);
        Assert.Equal(new[] { a, b, c }, ordered.Skip(1));
    }

    [Fact]
    public void MultipleTransportsKeepTheirRelativeOrder()
    {
        var first = Transport(6);
        var second = Transport(11);
        var player = Player(1);

        var ordered = Order(first, player, second);

        Assert.Equal(new[] { first, second, player }, ordered);
    }

    [Fact]
    public void ListWithoutTransportsIsUnchanged()
    {
        var a = Creature(100, 1);
        var b = Creature(200, 2);

        Assert.Equal(new[] { a, b }, Order(a, b));
    }

    [Fact]
    public void ListOfOnlyTransportsIsUnchanged()
    {
        var a = Transport(6);
        var b = Transport(11);

        Assert.Equal(new[] { a, b }, Order(a, b));
    }

    [Fact]
    public void CountsTransportsInTheBatch()
    {
        var batch = new List<WowGuid128> { Player(1), Transport(6), Creature(100, 2), Transport(11) };

        Assert.Equal(2, TransportCreateOrdering.CountTransports(batch, g => g));
    }
}
