using System;
using HermesProxy.World.Objects;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// AzerothCore resends the NPC flags in every unit Values block, so they are set on nearly every
/// <see cref="UnitData"/> the proxy builds. They live in the object rather than in a lazily
/// allocated array.
/// </summary>
public class UnitDataNpcFlagsTests
{
    [Fact]
    public void NpcFlags_NeverWritten_ReadAsNotSent()
    {
        var unit = new UnitData();

        Assert.Equal(2, unit.NpcFlags.Length);
        Assert.Null(unit.NpcFlags[0]);
        Assert.Null(unit.NpcFlags[1]);
    }

    [Fact]
    public void EnsureNpcFlags_Write_IsVisibleThroughNpcFlags()
    {
        var unit = new UnitData();

        unit.EnsureNpcFlags()[1] = 0x80u;

        Assert.Null(unit.NpcFlags[0]);
        Assert.Equal(0x80u, unit.NpcFlags[1]);
    }

    [Fact]
    public void NpcFlags_OfTwoUnits_AreIndependent()
    {
        var first = new UnitData();
        var second = new UnitData();

        first.EnsureNpcFlags()[0] = 1u;

        Assert.Null(second.NpcFlags[0]);
    }

    [Fact]
    public void EnsureNpcFlags_AllocatesNothing()
    {
        var unit = new UnitData();
        var warm = new UnitData();
        warm.EnsureNpcFlags()[0] = 1u;

        long before = GC.GetAllocatedBytesForCurrentThread();
        unit.EnsureNpcFlags()[0] = 1u;
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(0, allocated);
    }
}
