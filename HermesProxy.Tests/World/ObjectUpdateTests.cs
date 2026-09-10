using System.Runtime.CompilerServices;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World;

public class ObjectUpdateConstructorTests
{
    private static GlobalSessionData CreateGlobalSession()
    {
        return (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));
    }

    [Fact]
    public void NeedsWmoMapObjectFlag_Type15MoTransport_TrueForAnyDisplay()
    {
        Assert.True(ObjectUpdate.NeedsWmoMapObjectFlag((sbyte)GameObjectTypeModern.MOTransport, 8409, ClientVersionBuild.V3_4_3_54261));
        Assert.True(ObjectUpdate.NeedsWmoMapObjectFlag((sbyte)GameObjectTypeModern.MOTransport, 455, ClientVersionBuild.V3_4_3_54261));
        Assert.True(ObjectUpdate.NeedsWmoMapObjectFlag((sbyte)GameObjectTypeModern.MOTransport, null, ClientVersionBuild.V3_4_3_54261));
    }

    [Fact]
    public void NeedsWmoMapObjectFlag_Type11_SplitsOnDisplay()
    {
        GameData.LoadWmoGameObjectDisplays();
        Assert.NotEmpty(GameData.WmoGameObjectDisplays);

        // Strand of the Ancients / Isle of Conquest gunships -- WMO displays.
        Assert.True(ObjectUpdate.NeedsWmoMapObjectFlag((sbyte)GameObjectTypeModern.Transport, 8409, ClientVersionBuild.V3_4_3_54261));
        Assert.True(ObjectUpdate.NeedsWmoMapObjectFlag((sbyte)GameObjectTypeModern.Transport, 8410, ClientVersionBuild.V3_4_3_54261));
        Assert.True(ObjectUpdate.NeedsWmoMapObjectFlag((sbyte)GameObjectTypeModern.Transport, 8587, ClientVersionBuild.V3_4_3_54261));

        // Undercity elevator / Deeprun tram car -- M2 doodads, must stay unflagged.
        Assert.False(ObjectUpdate.NeedsWmoMapObjectFlag((sbyte)GameObjectTypeModern.Transport, 455, ClientVersionBuild.V3_4_3_54261));
        Assert.False(ObjectUpdate.NeedsWmoMapObjectFlag((sbyte)GameObjectTypeModern.Transport, 3831, ClientVersionBuild.V3_4_3_54261));
        Assert.False(ObjectUpdate.NeedsWmoMapObjectFlag((sbyte)GameObjectTypeModern.Transport, null, ClientVersionBuild.V3_4_3_54261));
    }

    [Fact]
    public void NeedsWmoMapObjectFlag_DoorOrGeneric_False()
    {
        GameData.LoadWmoGameObjectDisplays();

        Assert.False(ObjectUpdate.NeedsWmoMapObjectFlag(0, 8409, ClientVersionBuild.V3_4_3_54261));
        Assert.False(ObjectUpdate.NeedsWmoMapObjectFlag(5, 8409, ClientVersionBuild.V3_4_3_54261));
        Assert.False(ObjectUpdate.NeedsWmoMapObjectFlag(null, 8409, ClientVersionBuild.V3_4_3_54261));
    }

    [Fact]
    public void NeedsWmoMapObjectFlag_Type33DestructibleBuilding_V343Only()
    {
        GameData.LoadWmoGameObjectDisplays();

        // Type 33 destructible buildings DO take the flag, at any display: a native 3.4.3
        // Strand of the Ancients capture sends all 8 of its gates and walls with Flags
        // 0x100020, i.e. the map-object bit set, where we were sending 0x20 and the client
        // drew nothing at all (issue #184).
        Assert.True(ObjectUpdate.NeedsWmoMapObjectFlag(
            (sbyte)GameObjectTypeModern.DestructibleBuilding, 8409, ClientVersionBuild.V3_4_3_54261));
        Assert.True(ObjectUpdate.NeedsWmoMapObjectFlag(
            (sbyte)GameObjectTypeModern.DestructibleBuilding, null, ClientVersionBuild.V3_4_3_54261));

        // ...but only for V3_4_3. Type 33 was added in 3.0, so no vanilla or TBC backend can
        // produce one, and the older wire formats stay byte-identical.
        Assert.False(ObjectUpdate.NeedsWmoMapObjectFlag(
            (sbyte)GameObjectTypeModern.DestructibleBuilding, 8409, ClientVersionBuild.V1_14_2_42597));
        Assert.False(ObjectUpdate.NeedsWmoMapObjectFlag(
            (sbyte)GameObjectTypeModern.DestructibleBuilding, 8409, ClientVersionBuild.V2_5_3_42598));
    }

    [Fact]
    public void InitializePlaceholders_ValuesUpdateWithoutTypeId_KeepsWmoMapObjectFlag()
    {
        var session = CreateGlobalSession();
        var gameState = (GameSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GameSessionData));
        gameState.WmoMapObjectGuids = [];
        session.GameState = gameState;

        var guid = WowGuid128.Create(HighGuidType703.GameObject, 0, 190375, 7);

        // The create carries GAMEOBJECT_BYTES_1, so the type resolves and the flag goes on.
        var create = new ObjectUpdate(guid, UpdateTypeModern.CreateObject1, session);
        create.GameObjectData.TypeID = (sbyte)GameObjectTypeModern.MOTransport;
        create.GameObjectData.Flags = 0x20;
        create.InitializePlaceholders();

        Assert.Equal(0x100020u, create.GameObjectData.Flags!.Value);

        // A later Values update rewrites GAMEOBJECT_FLAGS without GAMEOBJECT_BYTES_1 in its
        // mask, so TypeID is null and the flag can only come from what the create established.
        // Losing it here is what made destructible buildings vanish the moment they took
        // damage -- the damage transition is exactly this shape of update (issue #184).
        var values = new ObjectUpdate(guid, UpdateTypeModern.Values, session);
        values.GameObjectData.Flags = 0x20;
        values.InitializePlaceholders();

        Assert.Null(values.GameObjectData.TypeID);
        Assert.Equal(0x100020u, values.GameObjectData.Flags!.Value);
    }

    [Fact]
    public void InitializePlaceholders_ValuesUpdateWithoutFlags_DoesNotFabricateFlags()
    {
        var session = CreateGlobalSession();
        var gameState = (GameSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GameSessionData));
        gameState.WmoMapObjectGuids = [];
        session.GameState = gameState;

        var guid = WowGuid128.Create(HighGuidType703.GameObject, 0, 193182, 13);

        var create = new ObjectUpdate(guid, UpdateTypeModern.CreateObject1, session);
        create.GameObjectData.TypeID = (sbyte)GameObjectTypeModern.MOTransport;
        create.GameObjectData.Flags = 0x8;
        create.InitializePlaceholders();
        Assert.Equal(0x100008u, create.GameObjectData.Flags!.Value);

        // A Values delta that does not carry GAMEOBJECT_FLAGS must not publish one. Publishing
        // MAP_OBJECT alone would clear every other bit on the client -- GO_FLAG_TRANSPORT (0x8)
        // among them, which is what lets a player attach to the deck. TrinityCore sends
        // exactly such a delta (ParentRotation + DynamicFlags) right after a boat's create.
        var values = new ObjectUpdate(guid, UpdateTypeModern.Values, session);
        values.ObjectData.DynamicFlags = 0;
        values.InitializePlaceholders();

        Assert.Null(values.GameObjectData.Flags);
    }

    [Fact]
    public void InitializePlaceholders_ValuesUpdateForUnknownGameObject_LeavesFlagsAlone()
    {
        var session = CreateGlobalSession();
        var gameState = (GameSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GameSessionData));
        gameState.WmoMapObjectGuids = [];
        session.GameState = gameState;

        // Nothing ever established this guid as a map object, so the memory must not invent
        // the flag for it -- an M2 doodad given GO_FLAG_MAP_OBJECT renders as an untextured
        // placeholder.
        var guid = WowGuid128.Create(HighGuidType703.GameObject, 0, 3831, 2);
        var values = new ObjectUpdate(guid, UpdateTypeModern.Values, session);
        values.GameObjectData.Flags = 0x20;
        values.InitializePlaceholders();

        Assert.Equal(0x20u, values.GameObjectData.Flags!.Value);
    }

    [Fact]
    public void Constructor_ItemGuid_InitializesItemData_ContainerDataOnDemand()
    {
        var guid = WowGuid128.Create(HighGuidType703.Item, 1);
        var session = CreateGlobalSession();

        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, session);

        Assert.NotNull(update.ItemData);
        Assert.NotNull(update.ObjectData);
        // A bag and a plain item share a guid type, so the 36-slot container block is only
        // materialised once a container field is written.
        Assert.Null(update.ContainerData);

        var container = update.EnsureContainerData();
        Assert.Same(container, update.ContainerData);
        Assert.Same(container, update.EnsureContainerData());
    }

    [Fact]
    public void Constructor_CreatureGuid_InitializesUnitData()
    {
        var guid = WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 1);
        var session = CreateGlobalSession();

        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, session);

        Assert.NotNull(update.UnitData);
    }

    [Fact]
    public void Constructor_PlayerGuid_InitializesUnitAndPlayerDataButNotActivePlayerData()
    {
        var guid = WowGuid128.Create(HighGuidType703.Player, 1);
        var session = CreateGlobalSession();

        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, session);

        Assert.NotNull(update.UnitData);
        Assert.NotNull(update.PlayerData);
        // ActivePlayerData is owner-only and ~32 KB, so the ctor leaves it null. Every other
        // player in view -- every bot in a battleground -- would otherwise allocate a block
        // the wire never carries.
        Assert.Null(update.ActivePlayerData);
    }

    [Fact]
    public void EnsureActivePlayerData_MaterialisesOnceAndIsIdempotent()
    {
        var guid = WowGuid128.Create(HighGuidType703.Player, 1);
        var session = CreateGlobalSession();

        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, session);

        var first = update.EnsureActivePlayerData();
        Assert.NotNull(first);
        Assert.Same(first, update.ActivePlayerData);
        Assert.Same(first, update.EnsureActivePlayerData());
    }

    [Fact]
    public void Constructor_GameObjectGuid_InitializesGameObjectData()
    {
        var guid = WowGuid128.Create(HighGuidType703.GameObject, 0, 5678, 1);
        var session = CreateGlobalSession();

        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, session);

        Assert.NotNull(update.GameObjectData);
    }

    [Fact]
    public void Constructor_DynamicObjectGuid_InitializesDynamicObjectData()
    {
        var guid = WowGuid128.Create(HighGuidType703.DynamicObject, 0, 100, 1);
        var session = CreateGlobalSession();

        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, session);

        Assert.NotNull(update.DynamicObjectData);
    }

    [Fact]
    public void Constructor_CorpseGuid_InitializesCorpseData()
    {
        var guid = WowGuid128.Create(HighGuidType703.Corpse, 0, 200, 1);
        var session = CreateGlobalSession();

        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, session);

        Assert.NotNull(update.CorpseData);
    }

    [Fact]
    public void Constructor_CreateObject1_InitializesCreateData()
    {
        var guid = WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 1);
        var session = CreateGlobalSession();

        var update = new ObjectUpdate(guid, UpdateTypeModern.CreateObject1, session);

        Assert.NotNull(update.CreateData);
    }

    [Fact]
    public void Constructor_CreateObject2_InitializesCreateData()
    {
        var guid = WowGuid128.Create(HighGuidType703.Player, 1);
        var session = CreateGlobalSession();

        var update = new ObjectUpdate(guid, UpdateTypeModern.CreateObject2, session);

        Assert.NotNull(update.CreateData);
    }

    [Fact]
    public void Constructor_ValuesType_DoesNotInitializeCreateData()
    {
        var guid = WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 1);
        var session = CreateGlobalSession();

        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, session);

        Assert.Null(update.CreateData);
    }
}

public class ObjectUpdateFieldExclusivityTests
{
    private static GlobalSessionData CreateGlobalSession()
    {
        return (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));
    }

    [Fact]
    public void Constructor_ItemGuid_UnitDataIsNull()
    {
        var guid = WowGuid128.Create(HighGuidType703.Item, 1);
        var session = CreateGlobalSession();

        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, session);

        Assert.Null(update.UnitData);
        Assert.Null(update.PlayerData);
        Assert.Null(update.ActivePlayerData);
        Assert.Null(update.GameObjectData);
        Assert.Null(update.DynamicObjectData);
        Assert.Null(update.CorpseData);
    }

    [Fact]
    public void Constructor_UnitGuid_ItemDataIsNull()
    {
        var guid = WowGuid128.Create(HighGuidType703.Creature, 0, 1234, 1);
        var session = CreateGlobalSession();

        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, session);

        Assert.Null(update.ItemData);
        Assert.Null(update.ContainerData);
        Assert.Null(update.PlayerData);
        Assert.Null(update.ActivePlayerData);
        Assert.Null(update.GameObjectData);
        Assert.Null(update.DynamicObjectData);
        Assert.Null(update.CorpseData);
    }

    [Fact]
    public void Constructor_PlayerGuid_ItemAndGameObjectDataAreNull()
    {
        var guid = WowGuid128.Create(HighGuidType703.Player, 1);
        var session = CreateGlobalSession();

        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, session);

        Assert.Null(update.ItemData);
        Assert.Null(update.ContainerData);
        Assert.Null(update.GameObjectData);
        Assert.Null(update.DynamicObjectData);
        Assert.Null(update.CorpseData);
    }

    [Fact]
    public void Constructor_GameObjectGuid_UnitAndItemDataAreNull()
    {
        var guid = WowGuid128.Create(HighGuidType703.GameObject, 0, 5678, 1);
        var session = CreateGlobalSession();

        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, session);

        Assert.Null(update.ItemData);
        Assert.Null(update.ContainerData);
        Assert.Null(update.UnitData);
        Assert.Null(update.PlayerData);
        Assert.Null(update.ActivePlayerData);
        Assert.Null(update.DynamicObjectData);
        Assert.Null(update.CorpseData);
    }

    [Fact]
    public void Constructor_AlwaysInitializesObjectData()
    {
        var guid = WowGuid128.Create(HighGuidType703.Player, 1);
        var session = CreateGlobalSession();

        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, session);

        Assert.NotNull(update.ObjectData);
    }

    [Fact]
    public void Constructor_StoresGuidAndType()
    {
        var guid = WowGuid128.Create(HighGuidType703.Player, 42);
        var session = CreateGlobalSession();

        var update = new ObjectUpdate(guid, UpdateTypeModern.CreateObject1, session);

        Assert.Equal(guid, update.Guid);
        Assert.Equal(UpdateTypeModern.CreateObject1, update.Type);
        Assert.Same(session, update.GlobalSession);
    }
}
