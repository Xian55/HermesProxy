using System;
using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server;
using HermesProxy.World.Server.Packets;
using HermesProxy.World.Server.Systems;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// The dungeon finder's lists from a 4.3.4 server for the 4.4.2 client. The incoming bytes are
/// written after TrinityCore 4.3.4's LFGPlayerInfo::Write and LFGPartyInfo::Write.
/// </summary>
public sealed class CataLfgTests
{
    // 4.4.2 client dungeons a 4.3.4 server does not have: 3 + 17 + 14 + 13 + 1 + 10 + 10 + 3
    private const int ClientOnlyDungeons = 71;

    private readonly LegacyHandlerHarness _harness = new(recordClientPackets: true);
    private readonly WowGuid128 _other;

    public CataLfgTests()
    {
        _harness.SetActivePlayer(new WowGuid64(2));
        _other = _harness.AddKnownObject(new WowGuid64(0x21), ObjectType.Player);
    }

    private T Deliver<T>(Action<WorldPacket> write, Action<WorldPacket> handler) where T : ServerPacket
    {
        using var packet = new WorldPacket();
        packet.WriteUInt16(0);
        write(packet);
        _harness.Deliver(Opcode.SMSG_LFG_PLAYER_INFO, packet.GetDataSpan().ToArray(), handler);
        return Assert.IsType<T>(_harness.ClientWire.Sent[^1].Packet);
    }

    private static uint Slot(uint type, uint dungeon) => LfgSlots.PackSlot(type, dungeon);

    // LfgPlayerQuestReward: money, experience, then items and currencies, marked by the last byte
    private static void WriteReward(WorldPacket packet, int money, int xp, (int Id, int Display, int Count, bool Currency)[] entries)
    {
        packet.WriteInt32(money);
        packet.WriteInt32(xp);
        packet.WriteUInt8((byte)entries.Length);
        foreach (var entry in entries)
        {
            packet.WriteInt32(entry.Id);
            packet.WriteInt32(entry.Display);
            packet.WriteInt32(entry.Count);
            packet.WriteUInt8((byte)(entry.Currency ? 1 : 0));
        }
    }

    [Fact]
    public void HandleLFGPlayerInfoCata_ReadsADungeonItsRewardsAndTheLocks()
    {
        var info = Deliver<LFGPlayerInfoPkt>(packet =>
        {
            packet.WriteUInt8(1);
            packet.WriteUInt32(Slot(LfgSlots.LfgTypeRandom, 258));
            packet.WriteUInt8(1);                                   // first reward
            for (int i = 1; i <= 12; i++)
                packet.WriteInt32(i);                               // quantities and limits
            packet.WriteUInt32(0x10);                               // completed mask
            packet.WriteUInt8(0);                                   // no role is short
            packet.WriteUInt32(0); packet.WriteUInt32(0); packet.WriteUInt32(0);
            WriteReward(packet, 4500, 1200, [(395, 0, 7, true), (51999, 31337, 1, false)]);
            packet.WriteUInt32(1);
            packet.WriteUInt32(Slot(LfgSlots.LfgTypeRandom, 300));
            packet.WriteUInt32(2);                                  // too low a level
            packet.WriteUInt32(0);
            packet.WriteUInt32(0);
        }, _harness.Client.HandleLFGPlayerInfoCata);

        LFGPlayerDungeonInfo dungeon = Assert.Single(info.Dungeons);
        Assert.Equal(Slot(LfgSlots.LfgTypeRandom, 258), dungeon.Slot);
        Assert.True(dungeon.FirstReward);
        Assert.Equal(1, dungeon.CompletionQuantity);
        Assert.Equal(2, dungeon.CompletionLimit);
        Assert.Equal(11, dungeon.PurseLimit);
        Assert.Equal(12, dungeon.Quantity);
        Assert.Equal(0x10u, dungeon.CompletedMask);
        Assert.Equal(4500, dungeon.Rewards.RewardMoney);
        Assert.Equal(1200, dungeon.Rewards.RewardXP);
        Assert.Equal(51999, Assert.Single(dungeon.Rewards.Items!).ItemID);
        LFGPlayerQuestRewardCurrency currency = Assert.Single(dungeon.Rewards.Currency!);
        Assert.Equal(395, currency.CurrencyID);
        Assert.Equal(7, currency.Quantity);

        LFGBlackListSlot locked = info.BlackList.Slots[0];
        Assert.Equal(Slot(LfgSlots.LfgTypeRandom, 300), locked.Slot);
        Assert.Equal(2u, locked.Reason);
        Assert.Equal(1 + ClientOnlyDungeons, info.BlackList.Slots.Count);
    }

    [Fact]
    public void HandleLFGPlayerInfoCata_AShortRole_HasARewardForEachRole()
    {
        var info = Deliver<LFGPlayerInfoPkt>(packet =>
        {
            packet.WriteUInt8(1);
            packet.WriteUInt32(Slot(LfgSlots.LfgTypeRandom, 301));
            packet.WriteUInt8(0);
            for (int i = 0; i < 12; i++)
                packet.WriteInt32(0);
            packet.WriteUInt32(0);
            packet.WriteUInt8(1);                                   // a role is short
            packet.WriteUInt32(2); WriteReward(packet, 100, 0, [(52000, 1, 1, false)]);
            packet.WriteUInt32(2); WriteReward(packet, 0, 0, []);
            packet.WriteUInt32(2); WriteReward(packet, 0, 0, []);
            WriteReward(packet, 9000, 0, [(395, 0, 70, true)]);
            packet.WriteUInt32(0);
        }, _harness.Client.HandleLFGPlayerInfoCata);

        LFGPlayerDungeonInfo dungeon = Assert.Single(info.Dungeons);
        Assert.Equal(9000, dungeon.Rewards.RewardMoney);
        Assert.Equal(70, Assert.Single(dungeon.Rewards.Currency!).Quantity);
        Assert.Equal(ClientOnlyDungeons, info.BlackList.Slots.Count);
    }

    [Fact]
    public void HandleLFGPartyInfoCata_FourNumbersALock()
    {
        var info = Deliver<LFGPartyInfo>(packet =>
        {
            packet.WriteUInt8(1);
            packet.WriteUInt64(0x21);
            packet.WriteUInt32(1);
            packet.WriteUInt32(Slot(LfgSlots.LfgTypeDungeon, 6));
            packet.WriteUInt32(2);
            packet.WriteUInt32(10);
            packet.WriteUInt32(20);
        }, _harness.Client.HandleLFGPartyInfoCata);

        LFGBlackListEntry player = Assert.Single(info.Players);
        Assert.Equal(_other, player.PlayerGuid);
        LFGLockInfoData locked = player.Locks[0];
        Assert.Equal(Slot(LfgSlots.LfgTypeDungeon, 6), locked.Slot);
        Assert.Equal(2u, locked.LockStatus);
        Assert.Equal(10u, locked.SubReason1);
        Assert.Equal(20u, locked.SubReason2);
        Assert.Equal(1 + ClientOnlyDungeons, player.Locks.Count);
    }

    private T DeliverHex<T>(string hex, Action<WorldPacket> handler) where T : ServerPacket
    {
        _harness.Deliver(Opcode.SMSG_LFG_UPDATE_STATUS, [0, 0, .. Convert.FromHexString(hex.Replace(" ", ""))], handler);
        return Assert.IsType<T>(_harness.ClientWire.Sent[^1].Packet);
    }

    // a queue alone for Random Classic Dungeon (258) as TrinityCore 4.3.4 sent it: ticket 213 of
    // player 2, type 3, time 0x6ACF0F36
    private const uint QueuedSlot = 0x06000102;

    [Fact]
    public void HandleLFGJoinResultCata_AJoinAsTheServerSentIt()
    {
        var result = DeliverHex<DFJoinResult>("03000000 00 D5000000 00 360FCF6A 10000000 03", _harness.Client.HandleLFGJoinResultCata);

        Assert.Equal(LfgJoinResults.ModernOk, result.Result);
        Assert.Equal(213u, result.Ticket.Id);
        Assert.Equal(0x6ACF0F36L, result.Ticket.Time);
        Assert.Equal((RideType)3, result.Ticket.Type);
        Assert.Equal(_harness.Session.GameState.CurrentPlayerGuid, result.Ticket.RequesterGuid);
        Assert.Empty(result.BlackList);
        Assert.Same(result.Ticket, _harness.Session.GameState.LfgTicket);
    }

    [Fact]
    public void HandleLFGUpdateStatusCata_TheThreeUpdatesOfAQueue()
    {
        // joined, then queued, then (after leaving) removed
        var joined = DeliverHex<DFUpdateStatus>("00000050 00C0 18 D5000000 360FCF6A 000000 03 03000000 02010006", _harness.Client.HandleLFGUpdateStatusCata);
        Assert.Equal<uint>([QueuedSlot], joined.Slots);
        Assert.True(joined.Joined);
        Assert.True(joined.LfgJoined);
        Assert.False(joined.Queued);
        Assert.Equal((byte)25, joined.Reason);                    // JOIN_QUEUE_INITIAL, 24 on 4.3.4
        Assert.Equal(213u, joined.Ticket.Id);

        var queued = DeliverHex<DFUpdateStatus>("00000050 00C8 0D D5000000 360FCF6A 000000 03 03000000 02010006", _harness.Client.HandleLFGUpdateStatusCata);
        Assert.True(queued.Queued);
        Assert.Equal(LfgUpdateTypes.ModernAddedToQueue, queued.Reason);
        Assert.Equal<uint>([QueuedSlot], _harness.Session.GameState.LfgQueuedSlots);

        var removed = DeliverHex<DFUpdateStatus>("00000000 0040 08 D5000000 360FCF6A 000000 03 03000000", _harness.Client.HandleLFGUpdateStatusCata);
        Assert.Empty(removed.Slots);
        Assert.False(removed.LfgJoined);
        Assert.False(removed.Queued);
        Assert.Equal(LfgUpdateTypes.ModernRemovedFromQueue, removed.Reason);
        Assert.Empty(_harness.Session.GameState.LfgQueuedSlots);
    }

    [Fact]
    public void HandleLFGQueueStatusCata_AStatusAsTheServerSentIt()
    {
        _harness.Session.GameState.LfgQueuedSlots = [QueuedSlot];

        // the server names the dungeon the random expanded to (1); the client is told the random
        var status = DeliverHex<DFQueueStatus>(
            "40 03 01FFFFFFFF 01FFFFFFFF 02FFFFFFFF FFFFFFFF 360FCF6A 01000001 08000000 D5000000 FFFFFFFF 03000000",
            _harness.Client.HandleLFGQueueStatusCata);

        Assert.Equal<byte>([1, 1, 2], status.LastNeeded);
        Assert.Equal(uint.MaxValue, status.AvgWaitTime);
        Assert.Equal(uint.MaxValue, status.AvgWaitTimeByRole[2]);
        Assert.Equal(8u, status.QueuedTime);
        Assert.Equal(QueuedSlot, status.Slot);
        Assert.Equal(213u, status.Ticket.Id);
    }

    [Fact]
    public void WriteCataJoinAndLeave_AsTheyWentToTheServer()
    {
        using var join = new WorldPacket();
        LfgSystem.WriteCataJoin(join, 0x08, [QueuedSlot]);
        Assert.Equal(Convert.FromHexString("08000000 000000000000000000000000 0000000080 02010006".Replace(" ", "")), join.GetDataSpan().ToArray());

        using var leave = new WorldPacket();
        LfgSystem.WriteCataLeave(leave, new RideTicket { RequesterGuid = _harness.Session.GameState.CurrentPlayerGuid, Id = 213, Type = (RideType)3, Time = 0x6ACF0F36 });
        Assert.Equal(Convert.FromHexString("00000000 360FCF6A 03000000 D5000000 20 03".Replace(" ", "")), leave.GetDataSpan().ToArray());
    }

    [Theory]
    [InlineData(0x00, LfgJoinResults.ModernOk)]
    [InlineData(0x1B, LfgJoinResults.ModernRoleCheckFailed)]
    [InlineData(0x1C, LfgJoinResults.ModernGroupFull)]
    [InlineData(0x24, LfgJoinResults.ModernInvalidSlot)]
    [InlineData(0x25, LfgJoinResults.ModernDeserterPlayer)]
    [InlineData(0x2B, LfgJoinResults.ModernRoleCheckFailed)]
    [InlineData(0x05, LfgJoinResults.ModernNoLfgObject)]
    public void LfgJoinResults_CataToModern(byte cata, byte modern) => Assert.Equal(modern, LfgJoinResults.CataToModern(cata));

    [Theory]
    [InlineData(6, LfgUpdateTypes.ModernJoinQueue)]
    [InlineData(8, LfgUpdateTypes.ModernRemovedFromQueue)]
    [InlineData(13, LfgUpdateTypes.ModernAddedToQueue)]
    [InlineData(14, LfgUpdateTypes.ModernProposalBegin)]
    [InlineData(15, LfgUpdateTypes.ModernUpdateStatus)]
    [InlineData(17, LfgUpdateTypes.ModernGroupDisband)]
    [InlineData(24, 25)]
    [InlineData(2, LfgUpdateTypes.ModernDefault)]
    public void LfgUpdateTypes_CataToModern(byte cata, byte modern) => Assert.Equal(modern, LfgUpdateTypes.CataToModern(cata));

    [Fact]
    public void GetCataHideSlots_EveryClientOnlyDungeon_TheRandomsAsRandoms()
    {
        var slots = LfgSlots.GetCataHideSlots([]);

        Assert.Equal(ClientOnlyDungeons, slots.Count);
        uint[] randoms = slots.Where(s => s >> 24 == LfgSlots.LfgTypeRandom).Select(LfgSlots.GetDungeonId).OrderBy(id => id).ToArray();
        Assert.Equal([2447u, 2470u, 2485u, 2735u, 2760u, 2881u], randoms);
        Assert.Contains(Slot(LfgSlots.LfgTypeDungeon, 436), slots);
        Assert.Contains(Slot(LfgSlots.LfgTypeDungeon, 2896), slots);
        Assert.DoesNotContain(slots, s => LfgSlots.GetDungeonId(s) is 434 or 2464 or 2891);
    }

    [Fact]
    public void GetCataHideSlots_LeavesOutWhatTheServerListed()
    {
        var slots = LfgSlots.GetCataHideSlots([2881u, 436u]);

        Assert.Equal(ClientOnlyDungeons - 2, slots.Count);
        Assert.DoesNotContain(slots, s => LfgSlots.GetDungeonId(s) is 2881 or 436);
    }
}
