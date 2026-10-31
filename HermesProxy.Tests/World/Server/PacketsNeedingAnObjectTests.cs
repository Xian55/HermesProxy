using System;
using System.Linq;
using System.Reflection;
using HermesProxy.Tests.Support;
using HermesProxy.Tests.World.Outbox;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Outbox;
using HermesProxy.World.Server;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>What each packet says it needs, and what that does for a sender nobody told about it.</summary>
public sealed class PacketsNeedingAnObjectTests
{
    private static readonly WowGuid128 Player = WowGuid128.Create(HighGuidType703.Player, 531);
    private static readonly WowGuid128 Other = WowGuid128.Create(HighGuidType703.Player, 532);

    public static TheoryData<ServerPacket> PacketsAboutThePlayer() =>
    [
        new PowerUpdate(Player),
        new SpellStart { Cast = new SpellCastData { CasterUnit = Player } },
        new SpellGo { Cast = new SpellCastData { CasterUnit = Player } },
        new MoveSetFlag(Opcode.SMSG_MOVE_SET_CAN_FLY) { MoverGUID = Player },
        new MoveSetSpeed(Opcode.SMSG_MOVE_SET_RUN_SPEED) { MoverGUID = Player },
        new ControlUpdate { Guid = Player },
        new MoveSetActiveMover { MoverGUID = Player },
        new MoveSetCollisionHeight { MoverGUID = Player },
        new MonsterMove(Player, new ServerSideMovement()),
        new MonsterMove(Other, new ServerSideMovement { TransportGuid = Player }),
    ];

    [Theory]
    [MemberData(nameof(PacketsAboutThePlayer))]
    public void PacketAboutThePlayer_NeedsThePlayer(ServerPacket packet)
    {
        Assert.True(packet.NeedsObject(Player));
        Assert.False(packet.NeedsObject(WowGuid128.Create(HighGuidType703.Player, 999)));
        packet.Discard();
    }

    [Fact]
    public void EveryPacketNamingAMoverOrACaster_SaysWhatItNeeds()
    {
        // The rule's point: a new packet of this shape cannot be forgotten the way the old ones were.
        string[] named = [nameof(MoveSetFlag.MoverGUID), nameof(SpellFailure.CasterUnit)];

        var silent = typeof(ServerPacket).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ServerPacket)) && !t.IsAbstract)
            .Where(t => t.GetFields(BindingFlags.Public | BindingFlags.Instance).Any(f => named.Contains(f.Name)))
            .Where(t => t.GetMethod(nameof(ServerPacket.NeedsObject))!.DeclaringType != t)
            .Select(t => t.Name)
            .ToArray();

        Assert.Empty(silent);
    }

    [Fact]
    public void ValuesNeedTheirObject_ACreateBringsIt()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: false);
        var state = harness.Session.GameState;

        var values = new UpdateObject(state);
        values.ObjectUpdates.Add(new ObjectUpdate(Player, UpdateTypeModern.Values, harness.Session));
        Assert.True(values.NeedsObject(Player));
        Assert.False(values.NeedsObject(Other));

        var created = new UpdateObject(state);
        created.ObjectUpdates.Add(new ObjectUpdate(Player, UpdateTypeModern.CreateObject1, harness.Session));
        created.ObjectUpdates.Add(new ObjectUpdate(Player, UpdateTypeModern.Values, harness.Session));
        Assert.False(created.NeedsObject(Player));

        var tooLate = new UpdateObject(state);
        tooLate.ObjectUpdates.Add(new ObjectUpdate(Player, UpdateTypeModern.Values, harness.Session));
        tooLate.ObjectUpdates.Add(new ObjectUpdate(Player, UpdateTypeModern.CreateObject1, harness.Session));
        Assert.True(tooLate.NeedsObject(Player));

        values.Discard();
        created.Discard();
        tooLate.Discard();
    }

    [Fact]
    public void SummonedPetValues_SentAheadOfTheCreate_WaitForIt()
    {
        // A companion left out at logout comes back at SMSG_SEND_KNOWN_SPELLS, ahead of the
        // player's create. Nothing in CollectionSync knows about that window.
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        var state = harness.Session.GameState;
        state.CurrentPlayerGuid = Player;
        state.ClientHasPlayerObject = false;
        state.SummonedBattlePetGuid = WowGuid128.Create(HighGuidType703.BattlePet, 39);

        CollectionSync.SendSummonedBattlePet(harness.Session);

        Assert.Empty(harness.ClientWire.Sent);
        Assert.Equal(1, harness.Session.ToClient.PendingCount);

        state.ClientHasPlayerObject = true;
        harness.Session.ToClient.Notify(OutboxEvent.GuidKnown(Player));

        Assert.Single(harness.ClientWire.Sent);
        Assert.Equal(Opcode.SMSG_UPDATE_OBJECT, harness.ClientWire.Sent[0].Opcode);
    }
}
