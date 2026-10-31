using System;
using System.Linq;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Outbox;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace HermesProxy.Tests.World.Outbox;

/// <summary>
/// While the client waits for an object, a packet that needs it waits too, whoever sends it. A
/// sender that had to opt in was how the same drop kept coming back (issue #314).
/// </summary>
public class AwaitedObjectHoldTests
{
    private static readonly WowGuid128 Player = WowGuid128.Create(HighGuidType703.Player, 1);
    private static readonly WowGuid128 Other = WowGuid128.Create(HighGuidType703.Player, 2);
    private static readonly OutboxEvent PlayerKnown = OutboxEvent.GuidKnown(Player);

    private static TestServerPacket Needing(WowGuid128 guid, int id) =>
        new(Opcode.SMSG_POWER_UPDATE, id) { Needed = guid };

    private static int[] Written(RecordingClientWire wire) => wire.Writes.Select(w => w.Packet.Id).ToArray();

    [Fact]
    public void PacketNeedingTheAwaitedObject_WaitsForIt_AndTheRestKeepsFlowing()
    {
        var wire = new RecordingClientWire { AwaitedObject = Player };
        var outbox = OutboxTestExtensions.InWorld(wire);

        outbox.Send(Needing(Player, 1));
        outbox.Send(Needing(Other, 2));
        outbox.Send(new TestServerPacket(Opcode.SMSG_POWER_UPDATE, 3));
        outbox.Send(Needing(Player, 4));

        Assert.Equal([2, 3], Written(wire));
        Assert.Equal(2, outbox.PendingCount);

        wire.AwaitedObject = default;
        outbox.Notify(PlayerKnown);

        Assert.Equal([2, 3, 1, 4], Written(wire));
        Assert.Equal(0, outbox.PendingCount);
    }

    [Fact]
    public void NothingAwaited_SendsEverything()
    {
        var wire = new RecordingClientWire();
        var outbox = OutboxTestExtensions.InWorld(wire);

        outbox.Send(Needing(Player, 1));

        Assert.Equal([1], Written(wire));
        Assert.Equal(0, outbox.PendingCount);
    }

    [Fact]
    public void ReleasedWhileTheObjectIsStillAwaited_WaitsAgain()
    {
        // A held packet released while the client has lost the object again, as across a teleport,
        // must not go out ahead of the next create.
        var wire = new RecordingClientWire { AwaitedObject = Player };
        var outbox = OutboxTestExtensions.InWorld(wire);
        outbox.Send(Needing(Player, 1));

        outbox.Notify(PlayerKnown);

        Assert.Empty(Written(wire));
        Assert.Equal(1, outbox.PendingCount);

        wire.AwaitedObject = default;
        outbox.Notify(PlayerKnown);

        Assert.Equal([1], Written(wire));
    }

    [Fact]
    public void ACreateThatNeverComes_DropsThePacketRatherThanSendingIt()
    {
        var time = new FakeTimeProvider();
        var wire = new RecordingClientWire { AwaitedObject = Player };
        var outbox = OutboxTestExtensions.InWorld(wire, time);
        outbox.Send(Needing(Player, 1));

        time.Advance(TimeSpan.FromSeconds(21));
        outbox.Tick();

        Assert.Empty(Written(wire));
        Assert.Equal(0, outbox.PendingCount);
    }

    [Fact]
    public void Cancel_DropsWhatWaitedForTheOldMapsCreate()
    {
        var wire = new RecordingClientWire { AwaitedObject = Player };
        var outbox = OutboxTestExtensions.InWorld(wire);
        outbox.Send(Needing(Player, 1));

        outbox.Cancel(ClientOutbox.AwaitedObjectKey);
        wire.AwaitedObject = default;
        outbox.Notify(PlayerKnown);

        Assert.Empty(Written(wire));
    }
}
