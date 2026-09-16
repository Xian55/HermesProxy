using System;
using System.Collections.Generic;
using Framework.Constants;
using HermesProxy;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Outbox;
using HermesProxy.World.Server.Packets;
using HermesProxy.Tests.World.Outbox;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// Issue #300: an update for the player's own guid that arrives while the client does not have the
/// player object yet.
/// </summary>
/// <remarks>
/// Two windows produce one: at login the player's CreateObject is held until the legacy server has
/// answered every <c>CMSG_ITEM_QUERY_SINGLE</c> it depends on (issue #34), and at a teleport the
/// client is between <c>SMSG_NEW_WORLD</c> and the re-create. Whatever the server sends in that
/// window used to be stripped as <c>unknown-guid</c> and lost, which is how a warrior who logged in
/// in Battle Stance got the empty non-stance action bar, and a character who logged out mounted got
/// the mount aura with no mount under them.
/// <para>
/// <c>ModernVersion.Build</c> is fixed for the test process, so the filter's V3_4_3 arm is reached
/// through <c>UpdateObject.ForceV343ForTests</c>.
/// </para>
/// </remarks>
public class PlayerValuesDuringLoginHoldTests
{
    private static readonly WowGuid128 Player = WowGuid128.Create(HighGuidType703.Player, 4020);
    private static readonly WowGuid128 Creature = WowGuid128.Create(HighGuidType703.Creature, 1, 299, 7);

    private static GameSessionData SessionWithPlayer()
    {
        // The factory is the only way in; nothing the filter touches needs the owning session.
        var state = GameSessionData.CreateNewGameSessionData(null!);
        state.CurrentPlayerGuid = Player;
        state.CurrentMapId = 0;
        return state;
    }

    private static ObjectUpdate Values(WowGuid128 guid, Action<ObjectUpdate> populate)
    {
        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, null!);
        populate(update);
        return update;
    }

    private static void WithV343(Action body)
    {
        UpdateObject.ForceV343ForTests = true;
        try
        {
            body();
        }
        finally
        {
            UpdateObject.ForceV343ForTests = null;
        }
    }

    /// <summary>
    /// The regression itself: the stance delta survives the filter although the client has not been
    /// given the player yet, while an unknown creature's delta is still dropped.
    /// </summary>
    [Fact]
    public void PlayerValues_ForAGuidTheClientDoesNotHaveYet_AreKept()
    {
        WithV343(() =>
        {
            var state = SessionWithPlayer();
            var batch = new UpdateObject(state);
            var stance = Values(Player, u => u.UnitData.ShapeshiftForm = 17);
            var creature = Values(Creature, u => u.UnitData.Health = 100);
            batch.ObjectUpdates.Add(stance);
            batch.ObjectUpdates.Add(creature);

            UpdateObject.FilterV3_4_3Values(batch, state);

            Assert.Contains(stance, batch.ObjectUpdates);
            Assert.DoesNotContain(creature, batch.ObjectUpdates);
        });
    }

    /// <summary>The second symptom in the issue: a mounted login's MountDisplayID.</summary>
    [Fact]
    public void PlayerMountDisplay_ForAGuidTheClientDoesNotHaveYet_IsKept()
    {
        WithV343(() =>
        {
            var state = SessionWithPlayer();
            var batch = new UpdateObject(state);
            var mount = Values(Player, u => u.UnitData.MountDisplayID = 22719);
            batch.ObjectUpdates.Add(mount);

            UpdateObject.FilterV3_4_3Values(batch, state);

            Assert.Contains(mount, batch.ObjectUpdates);
        });
    }

    /// <summary>
    /// The exemption is for the player's own guid only. Another player's delta for a guid the
    /// client never got is still dropped, or it comes straight back as CMSG_OBJECT_UPDATE_FAILED.
    /// </summary>
    [Fact]
    public void AnotherPlayersValues_ForAGuidTheClientDoesNotHaveYet_AreStillStripped()
    {
        WithV343(() =>
        {
            var state = SessionWithPlayer();
            var batch = new UpdateObject(state);
            var other = Values(WowGuid128.Create(HighGuidType703.Player, 4021),
                u => u.UnitData.ShapeshiftForm = 17);
            batch.ObjectUpdates.Add(other);

            UpdateObject.FilterV3_4_3Values(batch, state);

            Assert.Empty(batch.ObjectUpdates);
        });
    }

    /// <summary>
    /// Keeping the player's Values does not mean keeping its no-ops: a delta with nothing set is
    /// still dropped, which is what the filter was written for.
    /// </summary>
    [Fact]
    public void PlayerValues_ThatSayNothing_AreStillStripped()
    {
        WithV343(() =>
        {
            var state = SessionWithPlayer();
            state.ClientKnownGuids.Add(Player);
            var batch = new UpdateObject(state);
            batch.ObjectUpdates.Add(Values(Player, _ => { }));

            UpdateObject.FilterV3_4_3Values(batch, state);

            Assert.Empty(batch.ObjectUpdates);
        });
    }

    // ---- the hold, through the outbox --------------------------------------------------------
    //
    // The handler puts the split-out player Values on ctx.ToClient under one key and the client
    // gets them once it has the player. TestServerPacket stands in for the UpdateObject: what is
    // pinned here is the hold's shape -- order, the deferred batch's claim, the map change, the
    // timeout -- which is where a packet goes missing, not the bytes.

    private static readonly HoldKey Key = new(HoldKeyKind.PlayerValuesBatch);
    private static readonly OutboxEvent PlayerKnown = OutboxEvent.GuidKnown(Player);

    private sealed record HeldValues(TestServerPacket Packet);

    private static void Hold(ClientOutbox outbox, TestServerPacket packet) =>
        outbox.When(PlayerKnown, new HeldValues(packet), h => outbox.Send(h.Packet),
            new HoldOptions(Timeout: TimeSpan.FromSeconds(20),
                OnTimeout: OutboxTimeoutAction.Discard,
                Key: Key));

    private static TestServerPacket Packet(int id) =>
        new(Opcode.SMSG_UPDATE_OBJECT, id, ConnectionType.Instance);

    [Fact]
    public void ValuesHeldDuringTheCreate_GoOutWhenThePlayerIsKnown_InArrivalOrder()
    {
        var wire = new RecordingClientWire();
        var outbox = OutboxTestExtensions.InWorld(wire);

        Hold(outbox, Packet(1));
        Hold(outbox, Packet(2));
        Assert.Empty(wire.Ids);

        // What UpdateHandler.SendUpdateBatch raises at the end of the batch that gave the client
        // the player.
        outbox.Notify(PlayerKnown);

        Assert.Equal([1, 2], wire.Ids);
        Assert.Equal(0, outbox.PendingCount);
    }

    /// <summary>
    /// The deferred-flush path. <c>QueryHandler.FlushDeferredUpdate</c> claims them and sends them
    /// itself, right after the create, rather than letting the GuidKnown notify run them after the
    /// world-entry handshake — a nested release appends to the running release run.
    /// </summary>
    [Fact]
    public void TheDeferredBatch_ClaimsHeldValues_OldestFirst_AndTheEventNoLongerSendsThem()
    {
        var wire = new RecordingClientWire();
        var outbox = OutboxTestExtensions.InWorld(wire);
        Hold(outbox, Packet(1));
        Hold(outbox, Packet(2));

        List<int> claimed = [];
        while (outbox.Peek<HeldValues>(Key) is { } held && outbox.Claim(Key, held))
            claimed.Add(held.Packet.Id);

        Assert.Equal([1, 2], claimed);

        outbox.Notify(PlayerKnown);

        Assert.Empty(wire.Ids);
        Assert.Equal(0, outbox.PendingCount);
    }

    /// <summary>
    /// A map change drops them: the server re-sends the player's whole state in the new map's
    /// create, so a delta read against the old one is stale.
    /// </summary>
    [Fact]
    public void AMapChange_DropsValuesHeldForTheOldMap()
    {
        var wire = new RecordingClientWire();
        var outbox = OutboxTestExtensions.InWorld(wire);
        Hold(outbox, Packet(1));

        Assert.True(outbox.Cancel(Key));

        outbox.Notify(PlayerKnown);

        Assert.Empty(wire.Ids);
        Assert.Equal(0, outbox.PendingCount);
    }

    /// <summary>
    /// A create that never arrives must not end with the Values going out anyway: the client would
    /// answer every one with CMSG_OBJECT_UPDATE_FAILED for a guid it does not have.
    /// </summary>
    [Fact]
    public void ValuesWhosePlayerNeverArrives_AreDropped_NotSent()
    {
        var time = new FakeTimeProvider();
        var wire = new RecordingClientWire();
        var outbox = OutboxTestExtensions.InWorld(wire, time);
        Hold(outbox, Packet(1));

        time.Advance(TimeSpan.FromSeconds(21));
        outbox.Tick();

        Assert.Empty(wire.Ids);
        Assert.Equal(0, outbox.PendingCount);
    }
}
