using System.Runtime.CompilerServices;
using HermesProxy;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World;

public class ObjectSpecificDataUnionTests
{
    static ObjectSpecificDataUnionTests()
    {
        if (global::Framework.Settings.ClientBuild == ClientVersionBuild.Zero)
            global::Framework.Settings.ClientBuild = ClientVersionBuild.V1_14_2_42597;
    }

    private static GlobalSessionData CreateGlobalSession()
    {
        return (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));
    }

    [Fact]
    public void Specific_ItemGuid_IsItemVariantData()
    {
        var guid = WowGuid128.Create(HighGuidType703.Item, 1);
        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, CreateGlobalSession());

        Assert.IsType<ItemVariantData>(update.Specific.Value);
    }

    [Fact]
    public void Specific_CreatureGuid_IsUnitVariantData()
    {
        var guid = WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 1);
        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, CreateGlobalSession());

        Assert.IsType<UnitVariantData>(update.Specific.Value);
    }

    [Fact]
    public void Specific_PlayerGuid_IsPlayerVariantData()
    {
        var guid = WowGuid128.Create(HighGuidType703.Player, 1);
        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, CreateGlobalSession());

        Assert.IsType<PlayerVariantData>(update.Specific.Value);
    }

    [Fact]
    public void Specific_GameObjectGuid_IsGameObjectVariantData()
    {
        var guid = WowGuid128.Create(HighGuidType703.GameObject, 0, 5678, 1);
        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, CreateGlobalSession());

        Assert.IsType<GameObjectVariantData>(update.Specific.Value);
    }

    [Fact]
    public void Specific_DynamicObjectGuid_IsDynamicObjectVariantData()
    {
        var guid = WowGuid128.Create(HighGuidType703.DynamicObject, 0, 100, 1);
        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, CreateGlobalSession());

        Assert.IsType<DynamicObjectVariantData>(update.Specific.Value);
    }

    [Fact]
    public void Specific_CorpseGuid_IsCorpseVariantData()
    {
        var guid = WowGuid128.Create(HighGuidType703.Corpse, 0, 200, 1);
        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, CreateGlobalSession());

        Assert.IsType<CorpseVariantData>(update.Specific.Value);
    }

    [Fact]
    public void Specific_PlayerVariant_SharesSameUnitDataInstanceAcrossAccessors()
    {
        // PlayerVariantData carries one UnitData; both UnitData and (implicit player Unit) accessors return it
        var guid = WowGuid128.Create(HighGuidType703.Player, 1);
        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, CreateGlobalSession());

        var unitData = update.UnitData;
        Assert.NotNull(unitData);
        Assert.Same(unitData, ((PlayerVariantData)update.Specific.Value!).Unit);
    }

    [Fact]
    public void Accessor_ItemData_NullForCreatureGuid()
    {
        // Wrong-type accessor returns null (via null!) — preserves prior nullable-field behavior
        var guid = WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 1);
        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, CreateGlobalSession());

        Assert.Null(update.ItemData);
        Assert.Null(update.PlayerData);
        Assert.Null(update.GameObjectData);
    }

    [Fact]
    public void PatternMatching_OnSpecific_IsExhaustive()
    {
        // Demonstrate the union enables clean exhaustive pattern matching
        var guid = WowGuid128.Create(HighGuidType703.Player, 1);
        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, CreateGlobalSession());

        string description = update.Specific.Value switch
        {
            ItemVariantData => "item",
            UnitVariantData => "creature",
            PlayerVariantData => "player",
            GameObjectVariantData => "gameobject",
            DynamicObjectVariantData => "dynobj",
            CorpseVariantData => "corpse",
            null => "none",
            _ => "unknown",
        };

        Assert.Equal("player", description);
    }
}
