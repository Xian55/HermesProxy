using System.Linq;
using HermesProxy.Enums;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Outbox;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Movement;

/// <summary>
/// Issue #325: a login in mid-flight on a proxy that has just started. The player's create is held
/// until the server has answered the item queries it depends on (issue #34), and the server resumes
/// the flight in that window. The client drops every packet about a player it does not have yet, so
/// a flight sent ahead of the create left the character on foot in mid-air.
/// </summary>
public sealed class ResumedTaxiFlightTests
{
    private static readonly WowGuid64 Player = new(HighGuidTypeLegacy.Player, 77);
    private static readonly WowGuid64 Gryphon = new(HighGuidTypeLegacy.Creature, 3837, 40);

    private const long CreateTime = 50_000;

    // The proxy only puts a taxi start together for the clients whose servers sent one. A 3.4.3
    // flight goes out as the server's spline alone.
    private static bool BuildsTaxiStart => !ModernVersion.IsWotLKClassicOrLater;

    private static (LegacyHandlerHarness Harness, WowGuid128 Player) LoggingIn()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        WowGuid128 player = harness.SetActivePlayer(Player);
        harness.AddKnownObject(Gryphon, ObjectType.Unit);
        harness.Session.GameState.ClientHasPlayerObject = false;
        harness.Session.GameState.CurrentPlayerCreateTime = CreateTime;
        return (harness, player);
    }

    // A flight as the server sends it: a smooth flying path, every point in full.
    private static byte[] Flight(WowGuid64 mover)
        => LegacyPacketBuilder.Build(Opcode.SMSG_ON_MONSTER_MOVE, p =>
        {
            p.WritePackedGuid(mover);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                p.WriteUInt8(0);        // toggle anim tier in transport
            p.WriteFloat(-5128); p.WriteFloat(-790); p.WriteFloat(498);
            p.WriteUInt32(19380261);    // spline id
            p.WriteUInt8(0);            // spline type: normal
            p.WriteUInt32(LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056)
                ? (uint)SplineFlagVanilla.Flying
                : (uint)SplineFlagWotLK.Flying);
            p.WriteUInt32(181587);      // duration
            p.WriteUInt32(2);           // point count
            p.WriteFloat(-5203); p.WriteFloat(-713); p.WriteFloat(480);
            p.WriteFloat(-8833); p.WriteFloat(481); p.WriteFloat(112);
        });

    private static void Deliver(LegacyHandlerHarness harness, WowGuid64 mover)
        => harness.Deliver(Opcode.SMSG_ON_MONSTER_MOVE, Flight(mover), harness.Client.HandleMonsterMove,
            receivedTime: (int)CreateTime + 200);

    private static string[] Sent(LegacyHandlerHarness harness)
        => harness.ClientWire.Sent.Select(s => s.Packet switch
        {
            ControlUpdate c => c.HasControl ? "control" : "no-control",
            MonsterMove m when m.MoveSpline.SplineFlags.HasFlag(SplineFlagModern.Flying) => "flight",
            MonsterMove => "stop",
            ActivateTaxiReplyPkt => "taxi-reply",
            var other => other.GetType().Name,
        }).ToArray();

    private static string[] ResumedFlight
        => BuildsTaxiStart ? ["stop", "no-control", "stop", "no-control", "flight"] : ["flight"];

    [Fact]
    public void AFlightResumedBeforeThePlayersCreate_WaitsForIt_AndKeepsItsOrder()
    {
        if (WorldClient.IsCataLegacy)
            return;
        var (harness, player) = LoggingIn();

        Deliver(harness, Player);

        Assert.Empty(harness.ClientWire.Sent);
        Assert.Equal(ResumedFlight.Length, harness.Session.ToClient.PendingCount);

        // What the update path does once the create has gone to the outbox.
        harness.Session.GameState.ClientHasPlayerObject = true;
        harness.Session.ToClient.Notify(OutboxEvent.GuidKnown(player));

        Assert.Equal(ResumedFlight, Sent(harness));
        Assert.Equal(0, harness.Session.ToClient.PendingCount);
    }

    [Fact]
    public void AFlightTakenInTheWorld_GoesOutAtOnce()
    {
        if (WorldClient.IsCataLegacy)
            return;
        var (harness, _) = LoggingIn();
        harness.Session.GameState.ClientHasPlayerObject = true;
        harness.Session.GameState.IsWaitingForTaxiStart = true;

        Deliver(harness, Player);

        string[] expected = BuildsTaxiStart ? [.. ResumedFlight, "taxi-reply"] : ResumedFlight;
        Assert.Equal(expected, Sent(harness));
        Assert.Equal(0, harness.Session.ToClient.PendingCount);
    }

    /// <summary>Only the player waits: the units around it move as the server says.</summary>
    [Fact]
    public void AnotherUnitsFlight_IsNotHeldForThePlayersCreate()
    {
        if (WorldClient.IsCataLegacy)
            return;
        var (harness, _) = LoggingIn();

        Deliver(harness, Gryphon);

        Assert.Equal(["flight"], Sent(harness));
        Assert.Equal(0, harness.Session.ToClient.PendingCount);
    }
}
