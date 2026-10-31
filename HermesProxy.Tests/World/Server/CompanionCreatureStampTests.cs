using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// A creature of a companion species is the player's own only when the server says so. Darnassus
/// has two ownerless owls; the species alone made each of them the player's summoned pet.
/// </summary>
public sealed class CompanionCreatureStampTests : IDisposable
{
    private const uint OwlCreature = 7553;
    private const uint OwlSpecies = 68;
    private const uint OwlSummonSpell = 10707;

    private static readonly WowGuid128 Player = WowGuid128.Create(HighGuidType703.Player, 15);
    private static readonly WowGuid128 OtherPlayer = WowGuid128.Create(HighGuidType703.Player, 16);
    private static readonly WowGuid64 LegacyOwl = new(HighGuidTypeLegacy.Creature, OwlCreature, 49851);

    private readonly FrozenDictionary<uint, GameData.BattlePetSpeciesInfo> _speciesBefore =
        GameData.BattlePetSpeciesBySummonSpell;
    private readonly LegacyHandlerHarness _harness = new(recordClientPackets: true);

    public CompanionCreatureStampTests()
    {
        GameData.BattlePetSpeciesBySummonSpell = new Dictionary<uint, GameData.BattlePetSpeciesInfo>
        {
            [OwlSummonSpell] = new(OwlSpecies, OwlCreature, 2),
        }.ToFrozenDictionary();
        _harness.Session.GameState.CurrentPlayerGuid = Player;
        _harness.Session.GameState.ClientHasPlayerObject = true;
    }

    public void Dispose() => GameData.BattlePetSpeciesBySummonSpell = _speciesBefore;

    private ObjectUpdate OwlCreate(WowGuid128? summonedBy = null, WowGuid128? createdBy = null)
    {
        var update = new ObjectUpdate(LegacyOwl.To128(_harness.Session.GameState), UpdateTypeModern.CreateObject1, _harness.Session);
        update.ObjectData.EntryID = (int)OwlCreature;
        update.UnitData.SummonedBy = summonedBy;
        update.UnitData.CreatedBy = createdBy;
        return update;
    }

    [Fact]
    public void OwnerlessCreature_OfACompanionSpecies_IsNotThePlayers()
    {
        var update = OwlCreate();

        CollectionSync.StampCompanionCreature(update, _harness.Session, LegacyOwl);

        Assert.True(_harness.Session.GameState.SummonedBattlePetGuid.IsEmpty());
        Assert.True(_harness.Session.GameState.SummonedCompanionCreatureGuid.IsEmpty());
        Assert.Null(update.UnitData.BattlePetCompanionGUID);
        Assert.Empty(_harness.ClientWire.Sent);
    }

    [Fact]
    public void AnotherPlayersCompanion_LeavesThePlayersOwnSummonAlone()
    {
        var state = _harness.Session.GameState;
        var ownPet = WowGuid128.Create(HighGuidType703.BattlePet, 39);
        state.SummonedBattlePetGuid = ownPet;
        var update = OwlCreate(summonedBy: OtherPlayer);

        CollectionSync.StampCompanionCreature(update, _harness.Session, LegacyOwl);

        Assert.Equal(ownPet, state.SummonedBattlePetGuid);
        Assert.Null(update.UnitData.BattlePetCompanionGUID);
        Assert.Empty(_harness.ClientWire.Sent);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void CreatureOwnedByThePlayer_IsThePlayersCompanion(bool summoned, bool created)
    {
        var state = _harness.Session.GameState;
        var update = OwlCreate(summoned ? Player : null, created ? Player : null);

        CollectionSync.StampCompanionCreature(update, _harness.Session, LegacyOwl);

        Assert.Equal(WowGuid128.Create(HighGuidType703.BattlePet, OwlSpecies), state.SummonedBattlePetGuid);
        Assert.Equal(update.Guid, state.SummonedCompanionCreatureGuid);
        Assert.Equal(state.SummonedBattlePetGuid, update.UnitData.BattlePetCompanionGUID);
    }

    [Fact]
    public void CreatureThePlayersCritterFieldNames_IsThePlayersCompanion()
    {
        var state = _harness.Session.GameState;
        state.SummonedCompanionLegacyGuid = LegacyOwl;
        var update = OwlCreate();

        CollectionSync.StampCompanionCreature(update, _harness.Session, LegacyOwl);

        Assert.Equal(update.Guid, state.SummonedCompanionCreatureGuid);
    }
}
