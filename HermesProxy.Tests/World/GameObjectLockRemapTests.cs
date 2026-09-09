using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// Issue #269 — the V3_4_3 client picks its OPEN_LOCK spell from its own Lock.db2, whose
/// row 99 says LOCKTYPE_OPEN (5) where the legacy 3.3.5a Lock.dbc says LOCKTYPE_OPEN_ATTACKING
/// (14). Casting the client's choice at a GameObject using that lock earns
/// SPELL_FAILED_BAD_TARGETS from Spell::CanOpenLock.
/// </summary>
public class GameObjectLockRemapTests
{
    [Fact]
    public void Opening_OnLock99_IsRewrittenToAttacking()
    {
        Assert.Equal(
            KnownSpellIds.OpeningAttacking,
            GameObjectLockRemap.ResolveLegacyOpenLockSpell(KnownSpellIds.Opening, legacyLockId: 99));
    }

    [Theory]
    [InlineData(0u)]      // template not seen yet
    [InlineData(1u)]
    [InlineData(1748u)]   // also drifts between the builds, but harmlessly
    public void Opening_OnAnyOtherLock_IsForwardedUnchanged(uint lockId)
    {
        Assert.Equal(0u, GameObjectLockRemap.ResolveLegacyOpenLockSpell(KnownSpellIds.Opening, lockId));
    }

    [Theory]
    [InlineData(2366u)]   // Herb Gathering — EffectMiscValue 2
    [InlineData(2575u)]   // Mining — EffectMiscValue 3
    [InlineData(1804u)]   // Pick Lock — EffectMiscValue 1
    [InlineData(KnownSpellIds.OpeningAttacking)]
    public void OtherOpenLockSpells_OnLock99_AreForwardedUnchanged(uint spellId)
    {
        // Only the client's "Open" choice is wrong for this lock. A gathering or lockpicking
        // cast that happens to land on it still means what it says.
        Assert.Equal(0u, GameObjectLockRemap.ResolveLegacyOpenLockSpell(spellId, legacyLockId: 99));
    }

    [Fact]
    public void LegacyLockId_ForGoober_ComesFromData0()
    {
        // Statue of Queen Azshara (181964): GAMEOBJECT_TYPE_GOOBER, goober.lockId = data[0].
        var stats = new GameObjectStats { Type = (uint)GameObjectTypeLegacy.Goober };
        stats.Data[0] = 99;
        stats.Data[1] = 9683;

        Assert.Equal(99u, stats.LegacyLockId);
    }

    [Theory]
    [InlineData(GameObjectTypeLegacy.Door)]
    [InlineData(GameObjectTypeLegacy.Button)]
    public void LegacyLockId_ForDoorAndButton_ComesFromData1(GameObjectTypeLegacy type)
    {
        // These two keep the lock id one slot further along than every other type.
        var stats = new GameObjectStats { Type = (uint)type };
        stats.Data[0] = 1;      // startOpen
        stats.Data[1] = 99;

        Assert.Equal(99u, stats.LegacyLockId);
    }

    [Fact]
    public void LegacyLockId_ForFishingHole_ComesFromData4()
    {
        var stats = new GameObjectStats { Type = (uint)GameObjectTypeLegacy.FishingHole };
        stats.Data[0] = 20;     // radius
        stats.Data[4] = 99;

        Assert.Equal(99u, stats.LegacyLockId);
    }

    [Fact]
    public void LegacyLockId_ForTypeWithoutLock_IsZero()
    {
        // Type 11 (Transport) has no lock; data[0] is its taxi path id and must not be
        // mistaken for one.
        var stats = new GameObjectStats { Type = 11 };
        stats.Data[0] = 99;

        Assert.Equal(0u, stats.LegacyLockId);
    }
}
