using System;
using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server;
using HermesProxy.World.Server.Packets;
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
