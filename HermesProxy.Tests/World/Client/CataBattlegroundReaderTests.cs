using System;
using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using HermesProxy.World.Server.Systems;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// The 4.3.4 battleground readers against what a TrinityCore 4.3.4 server sent during one visit to
/// Warsong Gulch: a level 23 human with GUID 2, alone, in queue slot 0. Each packet is the server's
/// own bytes, so a bit read out of place fails here and not in a battle.
/// </summary>
public sealed class CataBattlegroundReaderTests
{
    private const uint WarsongGulch = 2;
    private const uint WarsongGulchMap = 489;
    private const uint JoinTime = 0x09EC8C96;

    private const string Queued =
        "48 c4 00 03 01 00 00 00 00 00 00 00 1e 00 00 11 03 96 8c ec 09 00 00 00 00 0a 00 00 00 00 00 00 00 00";
    private const string NeedConfirmation =
        "01 00 00 00 90 5f 01 00 00 00 00 00 00 96 8c ec 09 14 01 00 00 00 e9 01 00 00 00 04 30 80 11 1e 03 03";
    private const string ActiveBeforeTheTeleport =
        "26 04 40 11 1e 96 8c ec 09 00 00 00 00 00 00 01 00 00 00 e9 01 00 00 14 00 00 00 00 00 00 00 00 03 01 00 00 00 03";
    private const string ActiveInside =
        "26 04 40 11 1e 96 8c ec 09 00 00 00 00 00 00 01 00 00 00 e9 01 00 00 14 61 00 00 00 cc 93 04 00 03 01 00 00 00 03";
    private const string RefusedAsDeserter =
        "08 02 0c 01 00 00 00 00 00 00 00 03 1e 11 03 02 00 00 00 16 8b ec 09";
    private const string None = "80 01 00 00 00 00 00 00 00 96 8c ec 09 03";
    private const string List =
        "64 00 00 00 64 00 00 00 2d 00 00 00 02 00 00 00 2d 00 00 00 0e 01 00 00 0e 01 00 00 55 0a 00 00 00 00 10";
    private const string Scoreboard =
        "00 00 02 21 10 00 00 10 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
        "00 00 00 00 00 00 00 00 00 00 00 00 03 00 01";
    private const string OneFlagCarrier = "00 00 00 00 00 10 20 00 40 b3 44 03 00 80 65 44";
    private const string Countdown = "00 00 00 00 6e 00 00 00 78 00 00 00";

    private readonly LegacyHandlerHarness _harness = new(recordClientPackets: true);
    private readonly WowGuid128 _player;

    public CataBattlegroundReaderTests() => _player = _harness.SetActivePlayer(new WowGuid64(2));

    // a received packet starts with its opcode, which the handlers never look at
    private static byte[] Wire(string hex) => [0, 0, .. Convert.FromHexString(hex.Replace(" ", ""))];

    private T Deliver<T>(Opcode opcode, string hex, Action<WorldPacket> handler) where T : ServerPacket
    {
        _harness.Deliver(opcode, Wire(hex), handler);
        return Assert.IsType<T>(_harness.ClientWire.Sent[^1].Packet);
    }

    private void AssertHeader(BattlefieldStatusHeader hdr, byte minLevel, uint instance)
    {
        Assert.Equal(1u, hdr.Ticket.Id);
        Assert.Equal(_player, hdr.Ticket.RequesterGuid);
        Assert.Equal(RideType.Battlegrounds, hdr.Ticket.Type);
        Assert.Equal([WarsongGulch], hdr.BattlefieldListIDs);
        Assert.Equal(minLevel, hdr.RangeMin);
        Assert.Equal(0, hdr.ArenaTeamSize);
        Assert.Equal(instance, hdr.InstanceID);
        Assert.False(hdr.IsArena);
    }

    [Fact]
    public void Queued_NamesTheBattlegroundAndKeepsWhatThePortNeeds()
    {
        var queued = Deliver<BattlefieldStatusQueued>(
            Opcode.SMSG_BATTLEFIELD_STATUS_QUEUED, Queued, _harness.Client.HandleBattlefieldStatusQueuedCata);

        AssertHeader(queued.Hdr, minLevel: 10, instance: 0);
        Assert.Equal(0u, queued.AverageWaitTime);
        Assert.Equal(0u, queued.WaitTime);
        Assert.True(queued.EligibleForMatchmaking);
        Assert.False(queued.SuspendedQueue);

        var state = _harness.Session.GameState;
        Assert.Equal(WarsongGulch, state.GetBattleFieldQueueType(1));
        Assert.Equal(JoinTime, state.GetBattleFieldQueueJoinTime(1));
    }

    [Fact]
    public void NeedConfirmation_CarriesTheMapTheTimeoutAndTheChosenRole()
    {
        _harness.Session.GameState.BattlefieldJoinRoles = 8;

        var confirm = Deliver<BattlefieldStatusNeedConfirmation>(
            Opcode.SMSG_BATTLEFIELD_STATUS_NEED_CONFIRMATION, NeedConfirmation,
            _harness.Client.HandleBattlefieldStatusNeedConfirmationCata);

        AssertHeader(confirm.Hdr, minLevel: 20, instance: 1);
        Assert.Equal(WarsongGulchMap, confirm.Mapid);
        Assert.Equal(90000u, confirm.Timeout);
        Assert.Equal(2, confirm.Role);
    }

    [Theory]
    [InlineData(2, 0)]
    [InlineData(4, 1)]
    [InlineData(8, 2)]
    [InlineData(2 | 8, 0)]
    public void InvitationRole_IsTheFirstRoleOfTheJoin(byte joinRoles, byte expected)
        => Assert.Equal(expected, BattlegroundSystem.InvitationRole(joinRoles));

    [Fact]
    public void Active_Inside_CarriesTheTimesAndTheSide()
    {
        var active = Deliver<BattlefieldStatusActive>(
            Opcode.SMSG_BATTLEFIELD_STATUS_ACTIVE, ActiveInside, _harness.Client.HandleBattlefieldStatusActiveCata);

        AssertHeader(active.Hdr, minLevel: 20, instance: 1);
        Assert.Equal(WarsongGulchMap, active.Mapid);
        Assert.Equal(97u, active.StartTimer);
        Assert.Equal(299980u, active.ShutdownTimer);
        Assert.Equal(1, active.ArenaFaction);
        Assert.Single(_harness.ClientWire.Sent);
    }

    [Fact]
    public void Active_BeforeTheTeleport_IsPrecededByTheInit()
    {
        Deliver<BattlefieldStatusActive>(
            Opcode.SMSG_BATTLEFIELD_STATUS_ACTIVE, ActiveBeforeTheTeleport, _harness.Client.HandleBattlefieldStatusActiveCata);

        Opcode[] expected = [Opcode.SMSG_BATTLEGROUND_INIT, Opcode.SMSG_BATTLEFIELD_STATUS_ACTIVE];
        Assert.Equal(expected, _harness.ClientWire.Sent.Select(s => s.Opcode));
    }

    [Fact]
    public void Failed_CarriesTheServersReasonAndWhoCausedIt()
    {
        var failed = Deliver<BattlefieldStatusFailed>(
            Opcode.SMSG_BATTLEFIELD_STATUS_FAILED, RefusedAsDeserter, _harness.Client.HandleBattlefieldStatusFailedCata);

        Assert.Equal(2, failed.Reason); // ERR_GROUP_JOIN_BATTLEGROUND_DESERTERS, the modern client's own number
        Assert.Equal(WarsongGulch, failed.BattlefieldListId);
        Assert.Equal(1u, failed.Ticket.Id);
        Assert.Equal(_player, failed.Ticket.RequesterGuid);
        Assert.Equal(_player, failed.ClientID);
    }

    [Fact]
    public void None_TakesTheSlotAway()
    {
        Deliver<BattlefieldStatusQueued>(
            Opcode.SMSG_BATTLEFIELD_STATUS_QUEUED, Queued, _harness.Client.HandleBattlefieldStatusQueuedCata);

        var gone = Deliver<BattlefieldStatusFailed>(
            Opcode.SMSG_BATTLEFIELD_STATUS, None, _harness.Client.HandleBattlefieldStatusNoneCata);

        Assert.Equal(1u, gone.Ticket.Id);
        Assert.Equal(WarsongGulch, gone.BattlefieldListId);
        Assert.Equal(30, gone.Reason);
        Assert.Equal(0u, _harness.Session.GameState.GetBattleFieldQueueType(1));
        Assert.Equal(0u, _harness.Session.GameState.GetBattleFieldQueueJoinTime(1));
    }

    [Fact]
    public void List_AskedFromThePvpWindow_HasNoBattlemaster()
    {
        var list = Deliver<BattlefieldList>(
            Opcode.SMSG_BATTLEFIELD_LIST, List, _harness.Client.HandleBattlefieldListCata);

        Assert.Equal(WarsongGulch, list.BattlemasterListID);
        Assert.Equal(10, list.MinLevel);
        Assert.Equal(85, list.MaxLevel);
        Assert.True(list.PvpAnywhere);
        Assert.False(list.HasRandomWinToday);
        Assert.Empty(list.BattlefieldInstances);
        Assert.Equal(WowGuid128.Empty, list.BattlemasterGuid);
    }

    [Fact]
    public void Scoreboard_OfOnePlayer_HasItsSideItsColumnsAndTheTeamSizes()
    {
        var scores = Deliver<PVPMatchStatisticsMessage>(
            Opcode.SMSG_PVP_LOG_DATA, Scoreboard, _harness.Client.HandlePvPLogDataCata);

        var row = Assert.Single(scores.Statistics);
        Assert.Equal(_player, row.PlayerGUID);
        Assert.True(row.Faction); // Alliance
        Assert.NotNull(row.Honor);
        Assert.Equal(2, row.Stats.Count); // Warsong: flag captures, flag returns
        Assert.Null(scores.Winner);
        Assert.Null(scores.ArenaTeams);
        Assert.Equal<sbyte[]>([0, 1], scores.PlayerCount); // Horde, Alliance
    }

    [Fact]
    public void FlagCarrier_InTheSecondList_IsReadWithItsPlace()
    {
        var positions = Deliver<BattlegroundPlayerPositions>(
            Opcode.SMSG_BATTLEFIELD_PLAYER_POSITIONS, OneFlagCarrier, _harness.Client.HandleBattlegroundPlayerPositionsCata);

        var carrier = Assert.Single(positions.FlagCarriers);
        Assert.Equal(_player, carrier.Guid);
        Assert.Equal(new Vector2(918f, 1434f), carrier.Pos);
        Assert.Contains(_player, _harness.Session.GameState.FlagCarrierGuids);
    }

    [Fact]
    public void Countdown_KeepsTypeTimeLeftAndTotal()
    {
        _harness.Deliver(Opcode.SMSG_START_TIMER, Wire(Countdown), _harness.Client.HandleStartTimer);

        var timer = Assert.IsType<StartTimer>(_harness.ClientWire.Sent[^1].Packet);
        Assert.Equal(0u, timer.Type);
        Assert.Equal(110, timer.TimeLeft);
        Assert.Equal(120, timer.TotalTime);
        // the 4.4.2 client's layout: total, type, left, and the bit for a player it does not name
        Assert.Equal("7800000000000000000000006E0000000000000000", Convert.ToHexString(_harness.ClientWire.Sent[^1].Bytes));
    }
}
