using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Framework.Constants;
using HermesProxy.World.Enums;
using HermesProxy.World.Outbox;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace HermesProxy.Tests.World.Outbox;

public class ClientOutboxParkTests
{
    private static TestServerPacket Realm(int id) => new(Opcode.SMSG_SPELL_PREPARE, id, ConnectionType.Realm);
    private static TestServerPacket Instance(int id) => new(Opcode.SMSG_SPELL_START, id, ConnectionType.Instance);

    [Fact]
    public void RealmNotAttached_PacketsPark_ThenDrainInOrderOnAttach()
    {
        // First login: the legacy server sends SMSG_TUTORIAL_FLAGS and friends before the realm
        // socket exists. They must reach the client after it attaches, in the order they came.
        var wire = new RecordingClientWire();
        var outbox = new ClientOutbox(wire);
        outbox.Attach(ConnectionType.Instance);

        outbox.Send(Realm(1));
        outbox.Send(Realm(2));
        Assert.Empty(wire.Writes);
        Assert.Equal(2, outbox.ParkedCount);

        outbox.Attach(ConnectionType.Realm);

        Assert.Equal([1, 2], wire.Ids);
        Assert.Equal(0, outbox.ParkedCount);
    }

    [Fact]
    public void InstanceNotRequested_OnlyInstancePacketsWait_RealmKeepsFlowing()
    {
        var wire = new RecordingClientWire();
        var outbox = new ClientOutbox(wire);
        outbox.Attach(ConnectionType.Realm);

        outbox.Send(Instance(1));
        outbox.Send(Realm(2));
        Assert.Equal([2], wire.Ids);

        outbox.BeginInstanceConnect();
        outbox.Attach(ConnectionType.Instance);

        Assert.Equal([2, 1], wire.Ids);
    }

    [Fact]
    public void InstanceConnecting_EverythingWaitsInOneQueue_SoNothingOvertakes()
    {
        // The ad0122ee ordering again, now through a park: an instance SpellStart held for the
        // connection must not be overtaken by a realm packet sent after it.
        var wire = new RecordingClientWire();
        var outbox = new ClientOutbox(wire);
        outbox.Attach(ConnectionType.Realm);
        outbox.BeginInstanceConnect();

        outbox.Send(Realm(1));    // nothing parked yet: goes straight out
        outbox.Send(Instance(2)); // parks
        outbox.Send(Realm(3));    // parks behind 2
        outbox.Send(Instance(4));
        Assert.Equal([1], wire.Ids);

        outbox.Attach(ConnectionType.Instance);

        Assert.Equal([1, 2, 3, 4], wire.Ids);
        Assert.Equal(
            [ConnectionType.Realm, ConnectionType.Instance, ConnectionType.Realm, ConnectionType.Instance],
            wire.Writes.Select(w => w.On).ToArray());
    }

    [Fact]
    public void EarlyInstancePackets_GoOutBeforeThoseParkedDuringTheConnect()
    {
        var wire = new RecordingClientWire();
        var outbox = new ClientOutbox(wire);
        outbox.Attach(ConnectionType.Realm);

        outbox.Send(Instance(1));
        outbox.BeginInstanceConnect();
        outbox.Send(Instance(2));
        outbox.Attach(ConnectionType.Instance);

        Assert.Equal([1, 2], wire.Ids);
    }

    [Fact]
    public void ParkedPacket_IsSerializedOnTheThreadThatParksIt()
    {
        var wire = new RecordingClientWire();
        var outbox = new ClientOutbox(wire);
        outbox.Attach(ConnectionType.Realm);
        outbox.BeginInstanceConnect();
        var packet = Instance(1);

        int producer = Task.Run(() =>
        {
            outbox.Send(packet);
            return Environment.CurrentManagedThreadId;
        }).Result;

        Assert.Equal(producer, packet.SerializedOnThread);
        Task.Run(() => outbox.Attach(ConnectionType.Instance)).Wait();
        Assert.Equal(producer, packet.SerializedOnThread);
        Assert.Equal([1], wire.Ids);
    }

    [Fact]
    public void PacketSentDuringADrain_QueuesBehindIt()
    {
        var wire = new RecordingClientWire();
        var outbox = new ClientOutbox(wire);
        outbox.Attach(ConnectionType.Realm);
        outbox.BeginInstanceConnect();
        outbox.Send(Instance(1));
        outbox.Send(Instance(2));

        using var drainingFirst = new ManualResetEventSlim();
        using var lateSent = new ManualResetEventSlim();
        wire.OnWrite = packet =>
        {
            if (packet.Id != 1)
                return;
            drainingFirst.Set();
            lateSent.Wait(TimeSpan.FromSeconds(10));
        };

        var drainer = Task.Run(() => outbox.Attach(ConnectionType.Instance));
        drainingFirst.Wait(TimeSpan.FromSeconds(10));
        var late = Task.Run(() => outbox.Send(Realm(3)));
        // The late send parks rather than writing past the drain; give it the chance to be wrong.
        late.Wait(TimeSpan.FromMilliseconds(200));
        lateSent.Set();
        Task.WaitAll([drainer, late], TimeSpan.FromSeconds(10));

        Assert.Equal([1, 2, 3], wire.Ids);
    }

    [Fact]
    public void SocketClosesMidWrite_PacketReparks_AndGoesOutOnReattach()
    {
        var wire = new RecordingClientWire();
        var outbox = OutboxTestExtensions.InWorld(wire);

        wire.RealmOpen = false;
        outbox.Send(Realm(1));
        outbox.Send(Realm(2));
        Assert.Empty(wire.Writes);

        wire.RealmOpen = true;
        outbox.Attach(ConnectionType.Realm);

        Assert.Equal([1, 2], wire.Ids);
    }

    [Fact]
    public void LongBacklog_DrainsCompletelyAndInOrder_AcrossTheHandOff()
    {
        var wire = new RecordingClientWire();
        var outbox = new ClientOutbox(wire);
        outbox.Attach(ConnectionType.Instance);
        const int count = 64 * 8 + 100;
        for (int i = 0; i < count; i++)
            outbox.Send(Realm(i));

        outbox.Attach(ConnectionType.Realm);
        SpinWait.SpinUntil(() => wire.Writes.Length == count, TimeSpan.FromSeconds(10));

        Assert.Equal(Enumerable.Range(0, count).ToArray(), wire.Ids);
    }

    [Fact]
    public void HeadOfLinePark_ExpiresAfterTheTimeout()
    {
        var time = new FakeTimeProvider();
        var wire = new RecordingClientWire();
        var outbox = new ClientOutbox(wire, time);
        outbox.Attach(ConnectionType.Realm);
        outbox.BeginInstanceConnect();
        outbox.Send(Instance(1));

        time.Advance(ClientOutbox.ParkTimeout - TimeSpan.FromSeconds(1));
        outbox.TickParks();
        Assert.Equal(1, outbox.ParkedCount);

        time.Advance(TimeSpan.FromSeconds(1));
        outbox.TickParks();
        Assert.Equal(0, outbox.ParkedCount);

        outbox.Attach(ConnectionType.Instance);
        Assert.Empty(wire.Writes);
    }

    [Fact]
    public void EarlyInstancePark_DoesNotExpire_WhileThePlayerStaysAtCharacterSelect()
    {
        var time = new FakeTimeProvider();
        var outbox = new ClientOutbox(new RecordingClientWire(), time);
        outbox.Attach(ConnectionType.Realm);
        outbox.Send(Instance(1));

        time.Advance(TimeSpan.FromMinutes(10));
        outbox.TickParks();

        Assert.Equal(1, outbox.ParkedCount);
    }

    [Fact]
    public void DiscardParked_DropsEverythingWaiting()
    {
        var wire = new RecordingClientWire();
        var outbox = new ClientOutbox(wire);
        outbox.Send(Realm(1));
        outbox.Send(Instance(2));

        outbox.DiscardParked();
        outbox.Attach(ConnectionType.Realm);
        outbox.Attach(ConnectionType.Instance);

        Assert.Empty(wire.Writes);
        Assert.Equal(0, outbox.ParkedCount);
    }

    [Fact]
    public void After_FiresWhenAParkedTriggerPacketIsFinallyWritten()
    {
        var wire = new RecordingClientWire();
        var outbox = new ClientOutbox(wire);
        outbox.Attach(ConnectionType.Instance);

        outbox.After(Opcode.SMSG_SPELL_PREPARE, new TestServerPacket(Opcode.SMSG_CAST_FAILED, 2, ConnectionType.Instance));
        outbox.Send(Realm(1));
        Assert.Empty(wire.Writes);

        outbox.Attach(ConnectionType.Realm);

        Assert.Equal([1, 2], wire.Ids);
    }
}
