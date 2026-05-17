using System.Runtime.CompilerServices;
using HermesProxy;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Objects.Version.V3_4_3_54261;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.SourceGen;

// Byte-equivalence oracle for Phase 5b GameObject section. Production
// WriteCreateGameObjectData / WriteUpdateGameObjectData / HasAnyGameObjectFieldSet are
// now generator-emitted from [DescriptorCreateField] + [DescriptorUpdateField] +
// [DescriptorCreatePlaceholder] attrs on V3_4_3_54261.GameObjectField. These tests inline
// the pre-Phase-5b hand-port logic and assert the generated methods produce byte-identical
// output for representative payloads (empty, all-fields-set, individual-field-set,
// partial-rotation, owner-set, etc.).
public class GameObjectSectionEquivalenceTests
{
    private static GlobalSessionData CreateGlobalSession()
        => (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));

    private static GameSessionData CreateGameSession()
    {
        var session = (GameSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GameSessionData));
        typeof(GameSessionData).GetField(nameof(GameSessionData.OriginalObjectTypes))!
            .SetValue(session, new System.Collections.Generic.Dictionary<WowGuid128, ObjectType>());
        return session;
    }

    private static ObjectUpdateBuilder MakeBuilder(WowGuid128 guid, GameSessionData session, out ObjectUpdate update)
    {
        var globalSession = CreateGlobalSession();
        update = new ObjectUpdate(guid, UpdateTypeModern.Values, globalSession);
        return new ObjectUpdateBuilder(update, session);
    }

    // ---------------------------------------------------------------------
    // WriteCreate path — byte-eq vs frozen hand-port
    // ---------------------------------------------------------------------

    public static System.Collections.Generic.IEnumerable<object[]> CreateScenarios()
    {
        // Each scenario: setup-action delegate that populates GameObjectData.
        yield return new object[] { "empty",         (System.Action<GameObjectData>)(_ => { }) };
        yield return new object[] { "all-scalar",    (System.Action<GameObjectData>)(go => {
            go.DisplayID = 7000;
            go.SpellVisualID = 1234u;
            go.StateSpellVisualID = 5678u;
            go.StateAnimID = 9u;
            go.StateAnimKitID = 11u;
            go.Flags = 0x40u;
            go.FactionTemplate = 14;
            go.Level = 60;
            go.State = 1;
            go.TypeID = 2;
            go.PercentHealth = (byte)75;
            go.ArtKit = (byte)3;
            go.CustomParam = 0xDEADBEEFu;
        }) };
        yield return new object[] { "createdby-only", (System.Action<GameObjectData>)(go => {
            go.CreatedBy = WowGuid128.Create(HighGuidType703.Player, 42);
        }) };
        yield return new object[] { "guild-only", (System.Action<GameObjectData>)(go => {
            go.GuildGUID = WowGuid128.Create(HighGuidType703.Guild, 99);
        }) };
        yield return new object[] { "parent-rotation-full", (System.Action<GameObjectData>)(go => {
            go.ParentRotation[0] = 0f;
            go.ParentRotation[1] = 0f;
            go.ParentRotation[2] = 0.292f;
            go.ParentRotation[3] = 0.956f;
        }) };
        yield return new object[] { "parent-rotation-partial", (System.Action<GameObjectData>)(go => {
            go.ParentRotation[3] = 1f;
        }) };
        yield return new object[] { "parent-rotation-none", (System.Action<GameObjectData>)(_ => { /* all null */ }) };
    }

    [Theory]
    [MemberData(nameof(CreateScenarios))]
    public void WriteCreateGameObjectData_GeneratedMatchesHandPort(string _label, System.Action<GameObjectData> populate)
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.GameObject, 0, 5678, 1);
        var builder = MakeBuilder(guid, session, out var update);
        populate(update.GameObjectData!);

        var actual = new WorldPacket();
        builder.WriteCreateGameObjectData(actual);

        var expected = new WorldPacket();
        WriteCreateGameObjectData_HandPort(expected, update.GameObjectData!);

        Assert.Equal(expected.GetData(), actual.GetData());
    }

    // ---------------------------------------------------------------------
    // WriteUpdate path — byte-eq vs frozen hand-port
    // ---------------------------------------------------------------------

    public static System.Collections.Generic.IEnumerable<object[]> UpdateScenarios()
    {
        yield return new object[] { "empty",                 (System.Action<GameObjectData>)(_ => { }) };
        yield return new object[] { "displayid-only",        (System.Action<GameObjectData>)(go => go.DisplayID = 7000) };
        yield return new object[] { "flags-only",            (System.Action<GameObjectData>)(go => go.Flags = 0x40u) };
        yield return new object[] { "createdby-only",        (System.Action<GameObjectData>)(go => go.CreatedBy = WowGuid128.Create(HighGuidType703.Player, 42)) };
        yield return new object[] { "guild-only",            (System.Action<GameObjectData>)(go => go.GuildGUID = WowGuid128.Create(HighGuidType703.Guild, 99)) };
        yield return new object[] { "rotation-only",         (System.Action<GameObjectData>)(go => {
            go.ParentRotation[2] = 0.292f;
            go.ParentRotation[3] = 0.956f;
        }) };
        yield return new object[] { "state-typeid-percenthealth", (System.Action<GameObjectData>)(go => {
            go.State = 1;
            go.TypeID = 2;
            go.PercentHealth = (byte)75;
        }) };
        yield return new object[] { "all-fields",            (System.Action<GameObjectData>)(go => {
            go.DisplayID = 7000;
            go.SpellVisualID = 1234u;
            go.StateSpellVisualID = 5678u;
            go.StateAnimID = 9u;
            go.StateAnimKitID = 11u;
            go.CreatedBy = WowGuid128.Create(HighGuidType703.Player, 42);
            go.GuildGUID = WowGuid128.Create(HighGuidType703.Guild, 99);
            go.Flags = 0x40u;
            go.ParentRotation[0] = 0f;
            go.ParentRotation[1] = 0f;
            go.ParentRotation[2] = 0.292f;
            go.ParentRotation[3] = 0.956f;
            go.FactionTemplate = 14;
            go.Level = 60;
            go.State = 1;
            go.TypeID = 2;
            go.PercentHealth = (byte)75;
            go.ArtKit = (byte)3;
            go.CustomParam = 0xDEADBEEFu;
        }) };
    }

    [Theory]
    [MemberData(nameof(UpdateScenarios))]
    public void WriteUpdateGameObjectData_GeneratedMatchesHandPort(string _label, System.Action<GameObjectData> populate)
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.GameObject, 0, 5678, 1);
        var builder = MakeBuilder(guid, session, out var update);
        populate(update.GameObjectData!);

        var actual = new WorldPacket();
        builder.WriteUpdateGameObjectData(actual);

        var expected = new WorldPacket();
        WriteUpdateGameObjectData_HandPort(expected, update.GameObjectData!);

        Assert.Equal(expected.GetData(), actual.GetData());
    }

    // ---------------------------------------------------------------------
    // HasAny — semantic eq
    // ---------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(CreateScenarios))]
    public void HasAnyGameObjectFieldSet_GeneratedMatchesHandPort(string _label, System.Action<GameObjectData> populate)
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.GameObject, 0, 5678, 1);
        var builder = MakeBuilder(guid, session, out var update);
        populate(update.GameObjectData!);

        Assert.Equal(HasAnyGameObjectFieldSet_HandPort(update.GameObjectData!), builder.HasAnyGameObjectFieldSet());
    }

    // ---------------------------------------------------------------------
    // Inlined pre-Phase-5b hand-port — byte-for-byte oracle. Frozen here so the
    // production hand-port can be deleted in favour of the source-generated
    // equivalent. Identical to the bodies removed from
    // V3_4_3_54261/ObjectUpdateBuilder.cs (lines 1088-1132 / 1467-1482 /
    // 3248-3349 before the deletion), minus the trace log lines (no wire bytes).
    // ---------------------------------------------------------------------

    private static void WriteCreateGameObjectData_HandPort(WorldPacket data, GameObjectData go)
    {
        data.WriteInt32(go.DisplayID.GetValueOrDefault());
        data.WriteUInt32(go.SpellVisualID.GetValueOrDefault());
        data.WriteUInt32(go.StateSpellVisualID.GetValueOrDefault());
        data.WriteUInt32(go.StateAnimID.GetValueOrDefault());
        data.WriteUInt32(go.StateAnimKitID.GetValueOrDefault());
        data.WriteUInt32(0u);
        data.WritePackedGuid128(go.CreatedBy ?? WowGuid128.Empty);
        data.WritePackedGuid128(WowGuid128.Empty);
        data.WriteUInt32(go.Flags.GetValueOrDefault());
        if (go.ParentRotation != null
            && (go.ParentRotation[0].HasValue || go.ParentRotation[1].HasValue
                || go.ParentRotation[2].HasValue || go.ParentRotation[3].HasValue))
        {
            data.WriteFloat(go.ParentRotation[0].GetValueOrDefault(0f));
            data.WriteFloat(go.ParentRotation[1].GetValueOrDefault(0f));
            data.WriteFloat(go.ParentRotation[2].GetValueOrDefault(0f));
            data.WriteFloat(go.ParentRotation[3].GetValueOrDefault(1f));
        }
        else
        {
            data.WriteFloat(0f);
            data.WriteFloat(0f);
            data.WriteFloat(0f);
            data.WriteFloat(1f);
        }
        data.WriteInt32(go.FactionTemplate.GetValueOrDefault());
        data.WriteInt32(go.Level.GetValueOrDefault());
        data.WriteInt8(go.State.GetValueOrDefault());
        data.WriteInt8(go.TypeID.GetValueOrDefault());
        data.WriteUInt8(go.PercentHealth ?? 0);
        data.WriteUInt32(go.ArtKit.GetValueOrDefault());
        data.WriteUInt32(0u);
        data.WriteUInt32(go.CustomParam.GetValueOrDefault());
        data.WriteUInt32(0u);
    }

    private static void WriteUpdateGameObjectData_HandPort(WorldPacket data, GameObjectData go)
    {
        uint mask = 0u;
        if (go.DisplayID.HasValue) { mask |= 1u; mask |= 1u << 4; }
        if (go.SpellVisualID.HasValue) { mask |= 1u; mask |= 1u << 5; }
        if (go.StateSpellVisualID.HasValue) { mask |= 1u; mask |= 1u << 6; }
        if (go.StateAnimID.HasValue) { mask |= 1u; mask |= 1u << 7; }
        if (go.StateAnimKitID.HasValue) { mask |= 1u; mask |= 1u << 8; }
        if (go.CreatedBy != null) { mask |= 1u; mask |= 1u << 9; }
        if (go.GuildGUID != null) { mask |= 1u; mask |= 1u << 10; }
        if (go.Flags.HasValue) { mask |= 1u; mask |= 1u << 11; }
        bool hasRotation = false;
        if (go.ParentRotation != null)
            for (int i = 0; i < 4; i++)
                if (go.ParentRotation[i].HasValue) hasRotation = true;
        if (hasRotation) { mask |= 1u; mask |= 1u << 12; }
        if (go.FactionTemplate.HasValue) { mask |= 1u; mask |= 1u << 13; }
        if (go.Level.HasValue) { mask |= 1u; mask |= 1u << 14; }
        if (go.State.HasValue) { mask |= 1u; mask |= 1u << 15; }
        if (go.TypeID.HasValue) { mask |= 1u; mask |= 1u << 16; }
        if (go.PercentHealth.HasValue) { mask |= 1u; mask |= 1u << 17; }
        if (go.ArtKit.HasValue) { mask |= 1u; mask |= 1u << 18; }
        if (go.CustomParam.HasValue) { mask |= 1u; mask |= 1u << 19; }

        data.WriteBits(mask, 20);
        data.FlushBits();

        if ((mask & 1) != 0)
        {
            if ((mask & (1u << 4)) != 0) data.WriteInt32(go.DisplayID!.Value);
            if ((mask & (1u << 5)) != 0) data.WriteUInt32(go.SpellVisualID!.Value);
            if ((mask & (1u << 6)) != 0) data.WriteUInt32(go.StateSpellVisualID!.Value);
            if ((mask & (1u << 7)) != 0) data.WriteUInt32(go.StateAnimID!.Value);
            if ((mask & (1u << 8)) != 0) data.WriteUInt32(go.StateAnimKitID!.Value);
            if ((mask & (1u << 9)) != 0) data.WritePackedGuid128(go.CreatedBy!.Value);
            if ((mask & (1u << 10)) != 0) data.WritePackedGuid128(go.GuildGUID!.Value);
            if ((mask & (1u << 11)) != 0) data.WriteUInt32(go.Flags!.Value);
            if ((mask & (1u << 12)) != 0)
            {
                data.WriteFloat(go.ParentRotation![0] ?? 0f);
                data.WriteFloat(go.ParentRotation![1] ?? 0f);
                data.WriteFloat(go.ParentRotation![2] ?? 0f);
                data.WriteFloat(go.ParentRotation![3] ?? 1f);
            }
            if ((mask & (1u << 13)) != 0) data.WriteInt32(go.FactionTemplate!.Value);
            if ((mask & (1u << 14)) != 0) data.WriteInt32(go.Level!.Value);
            if ((mask & (1u << 15)) != 0) data.WriteInt8(go.State!.Value);
            if ((mask & (1u << 16)) != 0) data.WriteInt8(go.TypeID!.Value);
            if ((mask & (1u << 17)) != 0) data.WriteUInt8(go.PercentHealth!.Value);
            if ((mask & (1u << 18)) != 0) data.WriteUInt32(go.ArtKit!.Value);
            if ((mask & (1u << 19)) != 0) data.WriteUInt32(go.CustomParam!.Value);
        }
    }

    private static bool HasAnyGameObjectFieldSet_HandPort(GameObjectData go)
    {
        if (go == null) return false;
        if (go.DisplayID.HasValue || go.SpellVisualID.HasValue || go.StateSpellVisualID.HasValue) return true;
        if (go.StateAnimID.HasValue || go.StateAnimKitID.HasValue) return true;
        if (go.CreatedBy != null || go.GuildGUID != null) return true;
        if (go.Flags.HasValue || go.FactionTemplate.HasValue || go.Level.HasValue) return true;
        if (go.State.HasValue || go.TypeID.HasValue || go.PercentHealth.HasValue) return true;
        if (go.ArtKit.HasValue || go.CustomParam.HasValue) return true;
        if (go.ParentRotation != null)
            for (int i = 0; i < 4; i++)
                if (go.ParentRotation[i].HasValue) return true;
        return false;
    }
}
