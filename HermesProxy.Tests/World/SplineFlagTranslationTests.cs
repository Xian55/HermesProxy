using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// Pins the legacy → modern spline flag predicates against the flag sets the real backends send.
/// Issue #301: an exact <c>WalkMode | Flying</c> match missed AzerothCore's taxi spline entirely,
/// and no legacy flag set ever produced modern <c>CatmullRom</c>, so flights were flown as
/// straight legs.
/// </summary>
public class SplineFlagTranslationTests
{
    // AzerothCore / cMaNGOS FlightPathMovementGenerator: SetFly() + SetVelocity() and nothing else,
    // so the wire carries Flying alone. AzerothCore has no uncompressed-path flag at all —
    // PacketBuilder::WriteMonsterMove picks the Catmull-Rom point layout off Mask_CatmullRom,
    // which Flying is part of.
    private const SplineFlagWotLK AzerothCoreTaxi = SplineFlagWotLK.Flying;

    // Native 3.4.3 FlightPathMovementGenerator::DoReset: Fly + Smooth + Uncompressed + Walk.
    private const SplineFlagWotLK NativeTaxi =
        SplineFlagWotLK.WalkMode | SplineFlagWotLK.Flying |
        SplineFlagWotLK.CatmullRom | SplineFlagWotLK.UncompressedPath;

    // The flag set the old exact-equality test was written against.
    private const SplineFlagWotLK WalkingTaxi =
        SplineFlagWotLK.WalkMode | SplineFlagWotLK.Flying;

    [Theory]
    [InlineData(AzerothCoreTaxi)]
    [InlineData(NativeTaxi)]
    [InlineData(WalkingTaxi)]
    public void IsServerFlight_WotLK_TaxiSplines_AreDetected(SplineFlagWotLK flags)
    {
        Assert.True(SplineFlagTranslation.IsServerFlight(flags));
    }

    [Theory]
    [InlineData(SplineFlagWotLK.None)]
    [InlineData(SplineFlagWotLK.WalkMode)]
    [InlineData(SplineFlagWotLK.CatmullRom | SplineFlagWotLK.UncompressedPath)]
    [InlineData(SplineFlagWotLK.Knockback | SplineFlagWotLK.Trajectory)]
    public void IsServerFlight_WotLK_GroundSplines_AreNotDetected(SplineFlagWotLK flags)
    {
        Assert.False(SplineFlagTranslation.IsServerFlight(flags));
    }

    [Theory]
    [InlineData(AzerothCoreTaxi)]
    [InlineData(NativeTaxi)]
    [InlineData(WalkingTaxi)]
    [InlineData(SplineFlagWotLK.CatmullRom)]
    public void IsSmoothPath_WotLK_FlyingOrCatmullRom_IsSmooth(SplineFlagWotLK flags)
    {
        Assert.True(SplineFlagTranslation.IsSmoothPath(flags));
    }

    [Theory]
    [InlineData(SplineFlagWotLK.None)]
    [InlineData(SplineFlagWotLK.WalkMode)]
    [InlineData(SplineFlagWotLK.Knockback | SplineFlagWotLK.Trajectory)]
    public void IsSmoothPath_WotLK_GroundSplines_AreNotSmooth(SplineFlagWotLK flags)
    {
        Assert.False(SplineFlagTranslation.IsSmoothPath(flags));
    }

    [Theory]
    [InlineData(SplineFlagVanilla.Runmode | SplineFlagVanilla.Flying, true)]
    [InlineData(SplineFlagVanilla.Flying, true)]
    [InlineData(SplineFlagVanilla.Runmode, false)]
    [InlineData(SplineFlagVanilla.None, false)]
    public void Vanilla_FlyingDrivesBothPredicates(SplineFlagVanilla flags, bool expected)
    {
        Assert.Equal(expected, SplineFlagTranslation.IsServerFlight(flags));
        Assert.Equal(expected, SplineFlagTranslation.IsSmoothPath(flags));
    }

    [Theory]
    [InlineData(SplineFlagTBC.Runmode | SplineFlagTBC.Flying, true)]
    [InlineData(SplineFlagTBC.Flying, true)]
    [InlineData(SplineFlagTBC.Runmode, false)]
    [InlineData(SplineFlagTBC.None, false)]
    public void TBC_FlyingDrivesBothPredicates(SplineFlagTBC flags, bool expected)
    {
        Assert.Equal(expected, SplineFlagTranslation.IsServerFlight(flags));
        Assert.Equal(expected, SplineFlagTranslation.IsSmoothPath(flags));
    }

    /// <summary>
    /// Pins the modern dword the proxy emits for an AzerothCore taxi spline. 4194816 is what a
    /// V3_4_3 sniff showed before this fix — <c>Flying | UncompressedPath</c>, no CatmullRom, which
    /// the 3.4.3 client reads as a linear path because its <c>isSmooth()</c> tests CatmullRom alone.
    /// </summary>
    [Fact]
    public void ModernFlyingPath_GainsCatmullRom()
    {
        const SplineFlagModern sniffedBeforeFix =
            SplineFlagModern.Flying | SplineFlagModern.UncompressedPath;
        Assert.Equal(4194816u, (uint)sniffedBeforeFix);

        const SplineFlagModern afterFix = sniffedBeforeFix | SplineFlagModern.CatmullRom;
        Assert.Equal(4196864u, (uint)afterFix);
    }
}
