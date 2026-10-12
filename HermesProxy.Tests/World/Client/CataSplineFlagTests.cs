using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// 4.3.4 spline flags as the modern client is given them. The flags from 0x10 up to
/// UncompressedPath have the same values on both sides; Animation and Parabolic sit one bit lower
/// on 4.3.4 (0x01000000 and 0x02000000).
/// </summary>
public sealed class CataSplineFlagTests
{
    private const uint AnimationCata = 0x01000000;
    private const uint ParabolicCata = 0x02000000;

    /// <summary>
    /// A leap. The modern Parabolic flag makes the client read the spline's jump data, which no
    /// writer here sends, and the 4.4.2 client crashes on the read (null + 4, three times at a
    /// battleground's opening, when warriors leapt).
    /// </summary>
    [Theory]
    [InlineData(ParabolicCata)]
    [InlineData(ParabolicCata | 0x00100000)] // with SmoothGroundPath, as the server sent it
    [InlineData(ParabolicCata | AnimationCata)]
    public void Parabolic_IsNeverPassedOn(uint legacy)
        => Assert.False(WorldClient.ConvertSplineFlagsCata(legacy).HasFlag(SplineFlagModern.Parabolic));

    [Fact]
    public void Animation_MovesUpOneBit()
        => Assert.Equal(SplineFlagModern.Animation, WorldClient.ConvertSplineFlagsCata(AnimationCata));

    [Theory]
    [InlineData(0x00000200u)] // Flying
    [InlineData(0x00100000u)] // SmoothGroundPath
    [InlineData(0x00200000u)] // CanSwim
    [InlineData(0x00400000u)] // UncompressedPath
    [InlineData(0x00008000u)] // TransportEnter
    public void SharedFlags_KeepTheirValue(uint legacy)
        => Assert.Equal(legacy, (uint)WorldClient.ConvertSplineFlagsCata(legacy));

    /// <summary>4.3.4's FadeObject and Steering share their bits with modern Parabolic and FadeObject.</summary>
    [Theory]
    [InlineData(0x04000000u)]
    [InlineData(0x08000000u)]
    public void FlagsAboveParabolic_AreDropped(uint legacy)
        => Assert.Equal(SplineFlagModern.None, WorldClient.ConvertSplineFlagsCata(legacy));
}
