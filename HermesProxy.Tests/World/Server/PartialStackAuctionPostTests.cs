using System;
using System.Collections.Generic;
using System.Linq;
using HermesProxy.Tests.World.Outbox;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Outbox;
using HermesProxy.World.Server.Packets;
using HermesProxy.World.Server.Systems;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace HermesProxy.Tests.World.Server;

public class PartialStackAuctionPostTests
{
    private sealed class FakeInventory : IAuctionInventory
    {
        public readonly Dictionary<WowGuid128, uint> Stacks = [];
        public readonly Dictionary<(byte, byte), WowGuid64> Slots = [];

        public uint GetItemStackCount(WowGuid128 item) => Stacks.GetValueOrDefault(item);

        public (byte containerSlot, byte slot)? FindEmptyInventorySlot()
        {
            for (byte slot = 30; slot < 40; slot++)
            {
                if (!Slots.ContainsKey((255, slot)))
                    return (255, slot);
            }
            return null;
        }
        public (byte containerSlot, byte slot)? FindItemInInventory(WowGuid64 item) => (255, 23);
        public WowGuid64 GetInventorySlotItem(byte containerSlot, byte slot) => Slots.GetValueOrDefault((containerSlot, slot));
    }

    private static readonly WowGuid128 Stack = WowGuid128.Create(HighGuidType703.Item, 100);
    private static readonly WowGuid128 OtherStack = WowGuid128.Create(HighGuidType703.Item, 200);
    private static readonly WowGuid64 SplitResult = new(HighGuidTypeLegacy.Item, 0, 900);
    private static readonly WowGuid64 SplitResult2 = new(HighGuidTypeLegacy.Item, 0, 901);

    private static uint[] Opcodes(RecordingServerWire wire) => wire.Writes.Select(w => w.Packet.GetOpcode()).ToArray();

    private static uint SplitOpcode => HermesProxy.LegacyVersion.GetCurrentOpcode(Opcode.CMSG_SPLIT_ITEM);
    private static uint SellOpcode => HermesProxy.LegacyVersion.GetCurrentOpcode(Opcode.CMSG_AUCTION_SELL_ITEM);

    private static void EndBatch(ServerOutbox outbox) => outbox.Notify(OutboxEvent.Signal(OutboxSignal.UpdateBatchEnd));

    private static PartialStackAuctionPost Post(ServerOutbox outbox, FakeInventory inventory, TimeProvider time, params AuctionItemForSale[] items)
        => new(outbox, inventory, WowGuid128.Create(HighGuidType703.Creature, 1), 100, 200, 720, [.. items], time);

    [Fact]
    public void WholeStack_IsAuctionedDirectly_WithoutASplit()
    {
        var wire = new RecordingServerWire();
        var outbox = new ServerOutbox(wire);
        var inventory = new FakeInventory { Stacks = { [Stack] = 5 } };

        Post(outbox, inventory, TimeProvider.System, new AuctionItemForSale(Stack, 5)).Start();

        Assert.Equal([SellOpcode], Opcodes(wire));
        Assert.Equal(0, outbox.PendingCount);
    }

    [Fact]
    public void PartialStack_SplitsFirst_AndSellsOnlyOnceTheSlotFills()
    {
        var wire = new RecordingServerWire();
        var outbox = new ServerOutbox(wire);
        var inventory = new FakeInventory { Stacks = { [Stack] = 20 } };

        Post(outbox, inventory, TimeProvider.System, new AuctionItemForSale(Stack, 5)).Start();
        Assert.Equal([SplitOpcode], Opcodes(wire));

        EndBatch(outbox); // an unrelated update: slot still empty
        Assert.Equal([SplitOpcode], Opcodes(wire));

        inventory.Slots[(255, 30)] = SplitResult;
        EndBatch(outbox);

        Assert.Equal([SplitOpcode, SellOpcode], Opcodes(wire));
        Assert.Equal(0, outbox.PendingCount);
    }

    [Fact]
    public void SlotAlreadyFilledWhenTheWaitStarts_ProceedsWithoutAnotherBatch()
    {
        var wire = new RecordingServerWire();
        var outbox = new ServerOutbox(wire);
        var inventory = new FakeInventory { Stacks = { [Stack] = 20 } };
        // The server's update lands, and its batch ends, between the split going out and the
        // wait's hold being registered. No later batch comes.
        wire.OnWrite = packet =>
        {
            if (packet.GetOpcode() == SplitOpcode)
            {
                inventory.Slots[(255, 30)] = SplitResult;
                EndBatch(outbox);
            }
        };

        Post(outbox, inventory, TimeProvider.System, new AuctionItemForSale(Stack, 5)).Start();

        Assert.Equal([SplitOpcode, SellOpcode], Opcodes(wire));
        Assert.Equal(0, outbox.PendingCount);
    }

    [Fact]
    public void SplitThatNeverFills_AbortsTheRestOfThePost()
    {
        var time = new FakeTimeProvider();
        var wire = new RecordingServerWire();
        var outbox = new ServerOutbox(wire, time);
        var inventory = new FakeInventory { Stacks = { [Stack] = 20, [OtherStack] = 3 } };

        Post(outbox, inventory, time, new AuctionItemForSale(Stack, 5), new AuctionItemForSale(OtherStack, 3)).Start();
        time.Advance(PartialStackAuctionPost.SplitFillTimeout);
        outbox.Tick();

        Assert.Equal([SplitOpcode], Opcodes(wire));
        Assert.Equal(0, outbox.PendingCount);
    }

    [Fact]
    public void SeveralPartialStacks_WaitForTheTempSlotToEmptyBetweenSplits()
    {
        var wire = new RecordingServerWire();
        var outbox = new ServerOutbox(wire);
        var inventory = new FakeInventory { Stacks = { [Stack] = 20, [OtherStack] = 20 } };

        Post(outbox, inventory, TimeProvider.System, new AuctionItemForSale(Stack, 5), new AuctionItemForSale(OtherStack, 7)).Start();

        inventory.Slots[(255, 30)] = SplitResult;
        EndBatch(outbox);
        Assert.Equal([SplitOpcode, SellOpcode], Opcodes(wire));

        EndBatch(outbox); // auction not processed yet: slot still holds the first split
        Assert.Equal([SplitOpcode, SellOpcode], Opcodes(wire));

        inventory.Slots.Remove((255, 30));
        EndBatch(outbox);
        Assert.Equal([SplitOpcode, SellOpcode, SplitOpcode], Opcodes(wire));

        inventory.Slots[(255, 30)] = SplitResult2;
        EndBatch(outbox);
        Assert.Equal([SplitOpcode, SellOpcode, SplitOpcode, SellOpcode], Opcodes(wire));
        Assert.Equal(0, outbox.PendingCount);
    }

    [Fact]
    public void SecondPost_WaitsForTheFirst_SoTheyDontShareTheTempSlot()
    {
        var wire = new RecordingServerWire();
        var outbox = new ServerOutbox(wire);
        var inventory = new FakeInventory { Stacks = { [Stack] = 20, [OtherStack] = 20 } };

        Post(outbox, inventory, TimeProvider.System, new AuctionItemForSale(Stack, 5)).Start();
        Post(outbox, inventory, TimeProvider.System, new AuctionItemForSale(OtherStack, 7)).Start();
        Assert.Equal([SplitOpcode], Opcodes(wire));

        inventory.Slots[(255, 30)] = SplitResult;
        EndBatch(outbox);

        // The first post has sold. Only now does the second pick its temp slot, so it sees the
        // first one still occupied and splits into the next, then waits for that to fill.
        Assert.Equal([SplitOpcode, SellOpcode, SplitOpcode], Opcodes(wire));

        inventory.Slots[(255, 31)] = SplitResult2;
        EndBatch(outbox);

        Assert.Equal([SplitOpcode, SellOpcode, SplitOpcode, SellOpcode], Opcodes(wire));
        Assert.Equal(0, outbox.PendingCount);
    }
}
