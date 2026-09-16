using System.Collections.Frozen;
using System.IO;
using System.Linq;

using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;

using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// Issue #304 — Eye of the Storm flag captures never credit on a V3_4_3 client. Cataclysm dropped
/// the WotLK tower triggers (4476 / 4514 / 4516 / 4518) from the client's AreaTrigger data and left
/// only the buff triggers (4568-4571) on map 566, about 18 yd away. Every 3.3.5a-era core gates the
/// capture on the old ids and re-checks the sender's distance against its own DBC row first, so no
/// id translation can work — the proxy has to fire the old id from the player's own position.
/// </summary>
/// <remarks>
/// The coordinates and radius below are the 3.3.5a <c>AreaTrigger.dbc</c> rows, read out of a live
/// AzerothCore install. They are what <c>Player::IsInAreaTriggerRadius</c> tests the player against,
/// so drifting from them silently stops the capture working again.
/// </remarks>
public class AreaTriggerProximityTests
{
    private const uint EyeOfTheStormMapId = 566;

    private static FrozenDictionary<uint, ProximityAreaTrigger[]> Load()
        => GameData.ParseAreaTriggerProximity(Path.Combine("CSV", "AreaTriggerProximity3.csv"));

    [Fact]
    public void EyeOfTheStorm_HasAllFourTowerTriggers()
    {
        var triggers = Load()[EyeOfTheStormMapId];

        Assert.Equal([4476u, 4514u, 4516u, 4518u], triggers.Select(t => t.LegacyId).Order());
    }

    [Theory]
    // LegacyId, x, y, z — 3.3.5a AreaTrigger.dbc, map 566.
    [InlineData(4476u, 2048.48f, 1393.60f, 1194.54f)] // Blood Elf Tower
    [InlineData(4514u, 2044.00f, 1729.73f, 1190.03f)] // Fel Reaver Ruins
    [InlineData(4516u, 2284.78f, 1731.12f, 1190.08f)] // Mage Tower
    [InlineData(4518u, 2286.56f, 1402.36f, 1197.29f)] // Draenei Ruins
    public void TowerTrigger_MatchesTheLegacyDbcRow(uint legacyId, float x, float y, float z)
    {
        var trigger = Load()[EyeOfTheStormMapId].Single(t => t.LegacyId == legacyId);

        Assert.Equal(x, trigger.X, 2);
        Assert.Equal(y, trigger.Y, 2);
        Assert.Equal(z, trigger.Z, 2);
        // Radius 3 in the DBC; the loader squares it so the hot path needs no square root.
        Assert.Equal(9f, trigger.RadiusSquared, 3);
        Assert.Equal(EyeOfTheStormMapId, trigger.MapId);
    }

    [Fact]
    public void MapsWithoutTriggers_AreAbsent()
    {
        Assert.False(Load().ContainsKey((uint)BattlegroundMapID.WarsongGulch));
    }
}
