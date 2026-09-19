using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World;
using Xunit;

namespace HermesProxy.Tests.Framework;

/// <summary>
/// The <see cref="WorldPacket"/> object itself, built for every packet in both directions.
/// </summary>
public class PacketObjectTests
{
    private static object? _escape;

    /// <summary>
    /// A finalizer made every packet object take the runtime's slow allocation path and register
    /// with the finalization queue, about four times the cost of the object itself. Every packet is
    /// disposed, and a rental that misses its Dispose is collected like any other array.
    /// </summary>
    [Fact]
    public void PacketTypes_HaveNoFinalizer()
    {
        for (Type? type = typeof(WorldPacket); type != null && type != typeof(object); type = type.BaseType)
            Assert.Null(type.GetMethod("Finalize", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }

    [Fact]
    public void ReceivedPacket_Is48Bytes()
    {
        byte[] wire = [0x7C, 0x00, 0x01, 0x02];
        MeasurePacket(wire);

        long allocated = MeasurePacket(wire);

        Assert.Equal(48, allocated);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long MeasurePacket(byte[] wire)
    {
        long before = GC.GetAllocatedBytesForCurrentThread();
        var packet = new WorldPacket(wire, wire.Length, isPooled: false);
        _escape = packet;
        long after = GC.GetAllocatedBytesForCurrentThread();
        _escape = null;
        return after - before;
    }

    /// <summary>The receive time is always an <see cref="Environment.TickCount"/>, which wraps negative.</summary>
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public void ReceiveTime_RoundTripsEveryTickCount(int tickCount)
    {
        using var packet = new WorldPacket([0x7C, 0x00]);

        packet.SetReceiveTime(tickCount);

        Assert.Equal((long)tickCount, (long)packet.GetReceivedTime());
    }
}
