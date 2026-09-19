using System;
using System.Collections.Generic;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.Framework;

/// <summary>
/// <c>CastFlags&lt;TSource, TTarget&gt;</c> replaced <c>CastFlags&lt;TTarget&gt;(this Enum)</c> at the
/// per-packet call sites to drop the box. It must map every value exactly as the Enum overload
/// does, for each type pair it is used with, down to the OverflowException both throw for a
/// negative value of an int-backed enum.
/// </summary>
public class CastFlagsGenericTests
{
    private static IEnumerable<ulong> Samples<TSource>() where TSource : struct, Enum
    {
        yield return 0;
        yield return uint.MaxValue;
        foreach (TSource v in Enum.GetValues<TSource>())
            yield return Convert.ToUInt64(v);
        var rng = new Random(1234);
        for (int i = 0; i < 2000; i++)
            yield return (uint)rng.NextInt64(0, 1L << 32);
    }

    private static void AssertSame<TSource, TTarget>()
        where TSource : struct, Enum
        where TTarget : struct, Enum
    {
        foreach (ulong raw in Samples<TSource>())
        {
            var source = (TSource)Enum.ToObject(typeof(TSource), raw);
            object expected = Outcome(() => ((Enum)source).CastFlags<TTarget>());
            object actual = Outcome(() => source.CastFlags<TSource, TTarget>());
            Assert.True(expected.Equals(actual), $"{typeof(TSource).Name} 0x{raw:X} -> {typeof(TTarget).Name}: expected {expected}, got {actual}");
        }
    }

    private static object Outcome<T>(Func<T> f)
    {
        try { return f()!; }
        catch (Exception e) { return e.GetType(); }
    }

    [Fact]
    public void EveryPairInUse_MatchesTheEnumOverload()
    {
        AssertSame<MovementFlagWotLK, MovementFlagModern>();
        AssertSame<MovementFlagTBC, MovementFlagWotLK>();
        AssertSame<MovementFlagVanilla, MovementFlagWotLK>();
        AssertSame<MovementFlagModern, MovementFlagWotLK>();
        AssertSame<MovementFlagModern, MovementFlagTBC>();
        AssertSame<MovementFlagModern, MovementFlagVanilla>();
        AssertSame<SplineFlagWotLK, SplineFlagModern>();
        AssertSame<SplineFlagTBC, SplineFlagModern>();
        AssertSame<SplineFlagVanilla, SplineFlagModern>();
        AssertSame<UnitFlagsVanilla, UnitFlags>();
        AssertSame<UnitDynamicFlagsLegacy, UnitDynamicFlagsModern>();
        AssertSame<NPCFlagsVanilla, NPCFlags>();
        AssertSame<PlayerFlagsLegacy, PlayerFlags>();
        AssertSame<GameObjectDynamicFlagsLegacy, GameObjectDynamicFlagsModern>();
    }

    /// <summary>
    /// Bit 31 is negative as an int. While these two were int-backed, a legacy value with it set
    /// threw OverflowException instead of translating the bits the target does have.
    /// </summary>
    [Fact]
    public void LegacyNpcAndPlayerFlags_WithBit31Set_MapTheirNamedBits()
    {
        Assert.Equal(NPCFlags.Vendor, unchecked((NPCFlagsVanilla)0x80000004u).CastFlags<NPCFlagsVanilla, NPCFlags>());
        Assert.Equal(PlayerFlags.AFK, unchecked((PlayerFlagsLegacy)0x80000002u).CastFlags<PlayerFlagsLegacy, PlayerFlags>());
    }

    [Fact]
    public void AfterWarmUp_AllocatesNothing()
    {
        var flags = SplineFlagWotLK.Flying | SplineFlagWotLK.FinalPoint;
        _ = flags.CastFlags<SplineFlagWotLK, SplineFlagModern>();

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 100; i++)
            _ = flags.CastFlags<SplineFlagWotLK, SplineFlagModern>();

        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }
}
