using System;
using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// The 3.4.3 client removes the queue a SMSG_BATTLEFIELD_STATUS_FAILED ticket names, whatever the
/// reason. A refused join was sent as ticket 1 with that queue's join time, an exact match for the
/// player's first queue, which then left the client while the server kept it. From a 3.4.3 session
/// on AzerothCore: queued for Alterac Valley, then a third join refused with -4.
/// </summary>
public sealed class BattlefieldJoinRefusedTests
{
    private const int TooManyQueues = -4;

    private static void Refuse(LegacyHandlerHarness harness, int result)
    {
        harness.Deliver(Opcode.SMSG_BATTLEFIELD_STATUS_QUEUED,
            LegacyPacketBuilder.Build(Opcode.SMSG_BATTLEFIELD_STATUS_QUEUED, p => p.WriteInt32(result)),
            harness.Client.HandleGroupJoinedBattleground);
    }

    private static LegacyHandlerHarness QueuedHarness()
    {
        Assert.SkipWhen(WorldClient.IsCataLegacy, "A 4.3.4 server reports a refused join in its own packet.");
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        harness.SetActivePlayer(new WowGuid64(HighGuidTypeLegacy.Player, 703));
        harness.Session.GameState.StoreBattleFieldQueueType(1, 1);
        _ = harness.Session.GameState.GetBattleFieldQueueTime(1);
        return harness;
    }

    [Fact]
    public void HandleGroupJoinedBattleground_RefusedJoin_NamesNoQueue()
    {
        var harness = QueuedHarness();

        Refuse(harness, TooManyQueues);

        var failed = Assert.Single(harness.ClientWire.Sent.Select(s => s.Packet).OfType<BattlefieldStatusFailed>());
        Assert.Equal(0u, failed.Ticket.Id);
        Assert.Equal(0, failed.Ticket.Time);
        Assert.Equal(4, failed.Reason);
    }

    [Fact]
    public void HandleGroupJoinedBattleground_AcceptedJoin_SendsNoFailure()
    {
        var harness = QueuedHarness();

        Refuse(harness, 3);

        Assert.Empty(harness.ClientWire.Sent.Select(s => s.Packet).OfType<BattlefieldStatusFailed>());
    }
}
