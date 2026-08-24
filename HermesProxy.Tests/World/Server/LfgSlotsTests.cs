using HermesProxy.World.Server;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// The V3_4_3 client offers Titan Rune Protocol dungeons (LFGDungeons 2447 / 2470 / 2485
/// and remapped children in the 2400s) that do not exist in 3.3.5a. Queueing for one
/// made the legacy server drop CMSG_LFG_JOIN with no reply at all. Real 3.3.5 specifics
/// (issue #103) must still be forwarded — they never appear in SMSG_LFG_PLAYER_INFO.
/// </summary>
public class LfgSlotsTests
{
    private const uint RandomDungeonType = 6u << 24;
    private const uint SpecificDungeonType = 1u << 24;

    private const uint RandomLichKingDungeon = RandomDungeonType | 261u;
    private const uint RandomLichKingHeroic = RandomDungeonType | 262u;
    private const uint GundrakHeroic = SpecificDungeonType | 219u;
    private const uint TitanRuneGamma = RandomDungeonType | 2447u;
    private const uint TitanRuneChild = SpecificDungeonType | 2458u;

    [Fact]
    public void GetDungeonId_StripsTheTypeByte()
    {
        Assert.Equal(261u, LfgSlots.GetDungeonId(RandomLichKingDungeon));
        Assert.Equal(2447u, LfgSlots.GetDungeonId(TitanRuneGamma));
        Assert.Equal(219u, LfgSlots.GetDungeonId(GundrakHeroic));
    }

    [Fact]
    public void TryFindUnknownDungeon_WithServiceableDungeon_ReturnsFalse()
    {
        Assert.False(LfgSlots.TryFindUnknownDungeon(
            new[] { RandomLichKingDungeon }, out uint unknown));
        Assert.Equal(0u, unknown);
    }

    [Fact]
    public void TryFindUnknownDungeon_WithLegacySpecific_ReturnsFalse()
    {
        // Eligible specifics are implicit on 3.3.5 — they are not in PLAYER_INFO.
        Assert.False(LfgSlots.TryFindUnknownDungeon(
            new[] { GundrakHeroic }, out uint unknown));
        Assert.Equal(0u, unknown);
    }

    [Fact]
    public void TryFindUnknownDungeon_WithTitanRune_ReportsIt()
    {
        Assert.True(LfgSlots.TryFindUnknownDungeon(
            new[] { TitanRuneGamma }, out uint unknown));
        Assert.Equal(2447u, unknown);
    }

    [Fact]
    public void TryFindUnknownDungeon_WithTitanRuneChild_ReportsIt()
    {
        Assert.True(LfgSlots.TryFindUnknownDungeon(
            new[] { TitanRuneChild }, out uint unknown));
        Assert.Equal(2458u, unknown);
    }

    [Fact]
    public void TryFindUnknownDungeon_WithMixedRequest_ReportsTheUnserviceableOne()
    {
        Assert.True(LfgSlots.TryFindUnknownDungeon(
            new[] { GundrakHeroic, RandomLichKingHeroic, TitanRuneGamma }, out uint unknown));
        Assert.Equal(2447u, unknown);
    }

    [Fact]
    public void GetTitanRuneHideSlots_CoversHeadersAndChildren()
    {
        var slots = LfgSlots.GetTitanRuneHideSlots(System.Array.Empty<uint>());
        Assert.Contains(TitanRuneGamma, slots);
        Assert.Contains(TitanRuneChild, slots);
        Assert.Equal(44, slots.Count);
        Assert.DoesNotContain(GundrakHeroic, slots);
        Assert.DoesNotContain(RandomLichKingHeroic, slots);
    }

    [Fact]
    public void GetTitanRuneHideSlots_SkipsIdsAlreadyListed()
    {
        var slots = LfgSlots.GetTitanRuneHideSlots(new[] { 2447u, 2458u });
        Assert.DoesNotContain(TitanRuneGamma, slots);
        Assert.DoesNotContain(TitanRuneChild, slots);
        Assert.Equal(42, slots.Count);
    }
}
