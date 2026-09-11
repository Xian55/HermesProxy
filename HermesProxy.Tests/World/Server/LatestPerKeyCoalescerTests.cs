using System.Linq;
using HermesProxy.World.Server;
using Xunit;

namespace HermesProxy.Tests.World.Server;

public class LatestPerKeyCoalescerTests
{
    [Fact]
    public void Offer_OpensABatchOnlyForTheFirstValue()
    {
        var coalescer = new LatestPerKeyCoalescer<uint, string>();

        Assert.True(coalescer.Offer(1, "a"));
        Assert.False(coalescer.Offer(1, "b"));
        Assert.False(coalescer.Offer(2, "c"));
    }

    [Fact]
    public void Drain_KeepsTheNewestPerKey()
    {
        var coalescer = new LatestPerKeyCoalescer<uint, string>();
        coalescer.Offer(1, "rank1-a");
        coalescer.Offer(2, "rank2-a");
        coalescer.Offer(1, "rank1-b");

        var drained = coalescer.Drain(out int offered);

        Assert.Equal(["rank1-b", "rank2-a"], drained.Order());
        Assert.Equal(3, offered);
    }

    // The native capture's shape: one Apply, five packets for the same rank.
    [Fact]
    public void Drain_CollapsesABurstForOneKeyToItsLastValue()
    {
        var coalescer = new LatestPerKeyCoalescer<uint, int>();
        for (int tabFlags = 1; tabFlags <= 5; tabFlags++)
            coalescer.Offer(1, tabFlags);

        Assert.Equal([5], coalescer.Drain(out int offered));
        Assert.Equal(5, offered);
    }

    [Fact]
    public void Drain_EmptiesTheBatch_SoTheNextOfferOpensANewOne()
    {
        var coalescer = new LatestPerKeyCoalescer<uint, string>();
        coalescer.Offer(1, "a");
        coalescer.Drain(out _);

        Assert.Empty(coalescer.Drain(out int offered));
        Assert.Equal(0, offered);
        Assert.True(coalescer.Offer(1, "b"));
    }
}
