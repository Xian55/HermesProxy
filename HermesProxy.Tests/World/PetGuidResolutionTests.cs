using System.Runtime.CompilerServices;
using System.Threading;
using HermesProxy.World;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// cMaNGOS puts the pet number in a pet guid's entry slot. A guid translated before the pet's create
/// registered it carries that number, and ResolveStalePetGuid swaps in the creature entry.
/// </summary>
public class PetGuidResolutionTests
{
    private const uint PetNumber = 9568;
    private const uint CreatureEntry = 2031;

    private static GameSessionData NewState()
    {
        var state = (GameSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GameSessionData));
        // The field is readonly and the object skipped its constructor, so set the lock by reflection.
#pragma warning disable CS9216
        typeof(GameSessionData).GetField(nameof(GameSessionData.ObjectCacheLock))!.SetValue(state, new Lock());
#pragma warning restore CS9216
        state.PetRealEntryByLegacyGuid = [];
        state.PetLegacyGuidByModern = [];
        state.PetModernGuidByNumber = [];
        return state;
    }

    private static WowGuid128 Stale(ulong counter) => WowGuid128.Create(HighGuidType703.Pet, 0, PetNumber, counter);
    private static WowGuid128 Corrected(ulong counter) => WowGuid128.Create(HighGuidType703.Pet, 0, CreatureEntry, counter);

    private static void Register(GameSessionData state, ulong counter)
        => state.RegisterPet(new WowGuid64(HighGuidTypeLegacy.Pet, PetNumber, (uint)counter), Corrected(counter), CreatureEntry, PetNumber);

    [Fact]
    public void StaleGuid_OfTheRegisteredSpawn_GetsTheCreatureEntry()
    {
        var state = NewState();
        Register(state, 327);

        Assert.Equal(Corrected(327), state.ResolveStalePetGuid(Stale(327)));
    }

    [Fact]
    public void StaleGuid_OfANewSpawn_KeepsItsOwnCounter_NotThePreviousSpawns()
    {
        // Stable swap: the player's Summon for the pet coming out of the stable (spawn 328) was
        // translated before that spawn's create registered it, while spawn 327, the one just put
        // away, was still the registration. Resolving to 327 left the unit frame unbound.
        var state = NewState();
        Register(state, 327);

        Assert.Equal(Corrected(328), state.ResolveStalePetGuid(Stale(328)));
    }

    [Fact]
    public void AlreadyCorrectGuid_IsLeftAlone()
    {
        // TrinityCore: the pet number slot already holds the creature entry.
        var state = NewState();
        state.RegisterPet(new WowGuid64(HighGuidTypeLegacy.Pet, CreatureEntry, 5), Corrected(5), CreatureEntry, CreatureEntry);

        Assert.Null(state.ResolveStalePetGuid(Corrected(6)));
    }

    [Fact]
    public void UnregisteredPet_OrNotAPet_ResolvesToNothing()
    {
        var state = NewState();

        Assert.Null(state.ResolveStalePetGuid(Stale(1)));
        Assert.Null(state.ResolveStalePetGuid(WowGuid128.Create(HighGuidType703.Creature, 0, PetNumber, 1)));
    }
}
