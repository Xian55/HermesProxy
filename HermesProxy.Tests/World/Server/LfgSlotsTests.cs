using System.Collections.Generic;
using HermesProxy.World.Server;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// The V3_4_3 client offers Titan Rune Protocol dungeons (LFGDungeons 2447 / 2470 / 2485)
/// that do not exist in 3.3.5a. Queueing for one made the legacy server drop CMSG_LFG_JOIN
/// with no reply at all, leaving Dungeon Finder waiting forever with no error shown.
/// </summary>
public class LfgSlotsTests
{
    private const uint RandomDungeonType = 6u << 24;

    private const uint RandomLichKingDungeon = RandomDungeonType | 261u;
    private const uint RandomLichKingHeroic = RandomDungeonType | 262u;
    private const uint TitanRuneGamma = RandomDungeonType | 2447u;

    private static HashSet<uint> Known(params uint[] ids) => new(ids);

    [Fact]
    public void GetDungeonId_StripsTheTypeByte()
    {
        Assert.Equal(261u, LfgSlots.GetDungeonId(RandomLichKingDungeon));
        Assert.Equal(2447u, LfgSlots.GetDungeonId(TitanRuneGamma));
    }

    [Fact]
    public void TryFindUnknownDungeon_WithServiceableDungeon_ReturnsFalse()
    {
        Assert.False(LfgSlots.TryFindUnknownDungeon(
            Known(261u, 262u), new[] { RandomLichKingDungeon }, out uint unknown));
        Assert.Equal(0u, unknown);
    }

    [Fact]
    public void TryFindUnknownDungeon_WithTitanRune_ReportsIt()
    {
        Assert.True(LfgSlots.TryFindUnknownDungeon(
            Known(261u, 262u), new[] { TitanRuneGamma }, out uint unknown));
        Assert.Equal(2447u, unknown);
    }

    [Fact]
    public void TryFindUnknownDungeon_WithMixedRequest_ReportsTheUnserviceableOne()
    {
        Assert.True(LfgSlots.TryFindUnknownDungeon(
            Known(261u, 262u), new[] { RandomLichKingHeroic, TitanRuneGamma }, out uint unknown));
        Assert.Equal(2447u, unknown);
    }

    [Fact]
    public void TryFindUnknownDungeon_BeforePlayerInfoArrives_AllowsTheJoin()
    {
        // An empty set means SMSG_LFG_PLAYER_INFO has not been seen yet. Rejecting on that
        // would block every queue attempt, which is worse than the hang being fixed here.
        Assert.False(LfgSlots.TryFindUnknownDungeon(
            Known(), new[] { TitanRuneGamma }, out uint unknown));
        Assert.Equal(0u, unknown);
    }
}
