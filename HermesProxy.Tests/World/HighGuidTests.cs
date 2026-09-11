using HermesProxy.World;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// Covers the raw-value → <see cref="HighGuidType"/> mapping directly. The guid-level tests in
/// <see cref="WowGuidTests"/> exercise it through GetHighType(), but the two cases that matter
/// most here have no guid to hang off: cmangos's 0x4700 item high, and an unrecognised high,
/// which must return Null rather than throw (issue #101 — the exception used to reach the
/// WorldClient receive loop and disconnect the client).
/// </summary>
public class HighGuidTests
{
    [Theory]
    [InlineData(HighGuidTypeLegacy.None, HighGuidType.Null)]
    [InlineData(HighGuidTypeLegacy.Player, HighGuidType.Player)]
    [InlineData(HighGuidTypeLegacy.Item, HighGuidType.Item)]
    [InlineData(HighGuidTypeLegacy.ItemContainer, HighGuidType.Item)]
    [InlineData(HighGuidTypeLegacy.Group, HighGuidType.RaidGroup)]
    [InlineData(HighGuidTypeLegacy.Group2, HighGuidType.RaidGroup)]
    [InlineData(HighGuidTypeLegacy.MOTransport, HighGuidType.MOTransport)]
    [InlineData(HighGuidTypeLegacy.Transport, HighGuidType.Transport)]
    [InlineData(HighGuidTypeLegacy.GameObject, HighGuidType.GameObject)]
    [InlineData(HighGuidTypeLegacy.DynamicObject, HighGuidType.DynamicObject)]
    [InlineData(HighGuidTypeLegacy.Creature, HighGuidType.Creature)]
    [InlineData(HighGuidTypeLegacy.Pet, HighGuidType.Pet)]
    [InlineData(HighGuidTypeLegacy.Vehicle, HighGuidType.Vehicle)]
    [InlineData(HighGuidTypeLegacy.Corpse, HighGuidType.Corpse)]
    public void FromLegacy_KnownHigh_MapsToExpectedType(HighGuidTypeLegacy high, HighGuidType expected)
    {
        Assert.Equal(expected, HighGuid.FromLegacy(high));
    }

    [Theory]
    [InlineData(0x1234)]
    [InlineData(0xF160)]
    [InlineData(0x4701)]
    public void FromLegacy_UnknownHigh_ReturnsNullWithoutThrowing(int high)
    {
        Assert.Equal(HighGuidType.Null, HighGuid.FromLegacy((HighGuidTypeLegacy)high));
    }

    [Theory]
    [InlineData(HighGuidType703.Null, HighGuidType.Null)]
    [InlineData(HighGuidType703.Player, HighGuidType.Player)]
    [InlineData(HighGuidType703.Item, HighGuidType.Item)]
    [InlineData(HighGuidType703.Transport, HighGuidType.Transport)]
    [InlineData(HighGuidType703.Creature, HighGuidType.Creature)]
    [InlineData(HighGuidType703.Pet, HighGuidType.Pet)]
    [InlineData(HighGuidType703.GameObject, HighGuidType.GameObject)]
    [InlineData(HighGuidType703.Corpse, HighGuidType.Corpse)]
    [InlineData(HighGuidType703.LootObject, HighGuidType.LootObject)]
    [InlineData(HighGuidType703.RaidGroup, HighGuidType.RaidGroup)]
    [InlineData(HighGuidType703.ArenaTeam, HighGuidType.ArenaTeam)]
    [InlineData(HighGuidType703.Invalid, HighGuidType.Invalid)]
    public void From703_KnownHigh_MapsToExpectedType(HighGuidType703 high, HighGuidType expected)
    {
        Assert.Equal(expected, HighGuid.From703((byte)high));
    }

    // 53-62 sit between ArenaTeam (52) and Invalid (63) and have no type.
    [Theory]
    [InlineData(53)]
    [InlineData(62)]
    public void From703_UnknownHigh_ReturnsNullWithoutThrowing(byte high)
    {
        Assert.Equal(HighGuidType.Null, HighGuid.From703(high));
    }

    // The 703 side takes a 6-bit field, so every value a guid can produce is in range and the
    // mapping must answer for all of them.
    [Fact]
    public void From703_EveryValueInSixBitRange_DoesNotThrow()
    {
        for (byte high = 0; high < 64; high++)
            HighGuid.From703(high);
    }

    // Both mappings feed HasEntry/GetCounter/GetEntry, so a guid built from cmangos's item high
    // has to behave like an item all the way up, not just report the type.
    [Fact]
    public void ItemContainerGuid_BehavesAsItem()
    {
        var guid = new WowGuid64(HighGuidTypeLegacy.ItemContainer, 579);

        Assert.Equal(HighGuidType.Item, guid.GetHighType());
        Assert.True(guid.IsItem());
        Assert.False(guid.HasEntry());
        Assert.Equal(579ul, guid.GetCounter());
    }
}
