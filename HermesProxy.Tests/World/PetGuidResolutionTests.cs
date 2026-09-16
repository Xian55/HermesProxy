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
        state.PetNameQueryGuidByNumber = [];
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

    [Fact]
    public void PetNameQuery_ResolvesBeforeThePetsCreateHasRegisteredIt()
    {
        // Issue #299: a relog asks for the pet's name before the pet's create arrives, so
        // PetModernGuidByNumber is still empty when the response lands. The request's own
        // registration is what routes it.
        var state = NewState();
        state.RegisterPetNameQuery(PetNumber, Stale(327));

        Assert.Equal(Stale(327), state.TakePetNameQueryGuid(PetNumber));
    }

    [Fact]
    public void PetNameQuery_NamesTheSpawnAskedAbout_NotTheLastRegisteredOne()
    {
        // The registration map holds the last spawn seen for a pet number, so answering from it
        // after a stable swap names the pet that went away.
        var state = NewState();
        Register(state, 327);
        state.RegisterPetNameQuery(PetNumber, Corrected(328));

        Assert.Equal(Corrected(328), state.TakePetNameQueryGuid(PetNumber));
        Assert.Equal(Corrected(327), state.GetPetGuidByNumber(PetNumber));
    }

    [Fact]
    public void PetNameQuery_IsConsumedOnce()
    {
        var state = NewState();
        state.RegisterPetNameQuery(PetNumber, Stale(327));

        Assert.Equal(Stale(327), state.TakePetNameQueryGuid(PetNumber));
        Assert.Equal(default, state.TakePetNameQueryGuid(PetNumber));
    }

    [Fact]
    public void RegisteredPet_ReverseResolvesToTheLegacyGuidTheServerKnows()
    {
        // Attack swings, selections and spell targets used to go out through the plain To64(),
        // which leaves creature_template.entry in the entry slot. The legacy server keyed the pet
        // by pet_number and looks units up by the whole guid, so nothing matched and AzerothCore
        // answered the swing with SMSG_ATTACK_STOP.
        var state = NewState();
        Register(state, 327);

        Assert.Equal(PetNumber, Corrected(327).To64(state).GetEntry());
        Assert.Equal(CreatureEntry, Corrected(327).To64().GetEntry());
    }

    [Fact]
    public void UnregisteredPet_ReverseResolvesToThePassThrough()
    {
        // TrinityCore-style backends never register (the entry slot already holds the creature
        // entry), so the pass-through is the right answer there.
        var state = NewState();

        Assert.Equal(Corrected(327).To64(), Corrected(327).To64(state));
    }

    [Fact]
    public void PetNameQuery_UnknownNumber_ResolvesToNothing()
    {
        var state = NewState();

        Assert.Equal(default, state.TakePetNameQueryGuid(PetNumber));
    }
}
