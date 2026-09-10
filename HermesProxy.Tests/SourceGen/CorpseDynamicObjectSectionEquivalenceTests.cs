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

// Byte-equivalence oracles for the Corpse and DynamicObject sections.
//
// These two sections were the last hand-written Create writers, and neither had an Update
// path at all — UpdateHandler parsed their fields out of legacy Values blocks and the
// builder discarded them. So the oracles here come from two different places:
//
//   Create — a frozen copy of the hand-port that was deleted when the sections were wired.
//            Same contract as the other *SectionEquivalenceTests: do not "fix" the copy.
//   Update — transcribed from TC 3.4.3 UpdateFields.cpp (CorpseData::WriteUpdate /
//            DynamicObjectData::WriteUpdate). There was no prior implementation to freeze,
//            so the reference is the server source the client is built against. The
//            transcription computes its own changesMask from field presence rather than
//            borrowing the generated one, so the two agree only if both read the layout
//            the same way.
public class CorpseDynamicObjectSectionEquivalenceTests
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

    // =====================================================================
    // Corpse
    // =====================================================================

    public static System.Collections.Generic.IEnumerable<object[]> CorpseScenarios()
    {
        yield return new object[] { "empty", (System.Action<CorpseData>)(_ => { }) };
        yield return new object[] { "dynamicflags-only", (System.Action<CorpseData>)(c => c.DynamicFlags = 1u) };
        yield return new object[] { "flags-only", (System.Action<CorpseData>)(c => c.Flags = 0x08u) };
        yield return new object[] { "owner-only", (System.Action<CorpseData>)(c =>
            c.Owner = WowGuid128.Create(HighGuidType703.Player, 42)) };
        yield return new object[] { "guids", (System.Action<CorpseData>)(c =>
        {
            c.Owner = WowGuid128.Create(HighGuidType703.Player, 42);
            c.PartyGUID = WowGuid128.Create(HighGuidType703.Party, 7);
            c.GuildGUID = WowGuid128.Create(HighGuidType703.Guild, 99);
        }) };
        yield return new object[] { "appearance", (System.Action<CorpseData>)(c =>
        {
            c.DisplayID = 12345u;
            c.RaceId = (byte)1;
            c.SexId = (byte)0;
            c.ClassId = (byte)4;
            c.FactionTemplate = 1801;
        }) };
        yield return new object[] { "items-sparse", (System.Action<CorpseData>)(c =>
        {
            c.Items[0] = 1000u;
            c.Items[9] = 2000u;
            c.Items[18] = 3000u;
        }) };
        yield return new object[] { "items-full", (System.Action<CorpseData>)(c =>
        {
            for (int i = 0; i < 19; i++) c.Items[i] = (uint)(100 + i);
        }) };
        yield return new object[] { "all-fields", (System.Action<CorpseData>)(c =>
        {
            c.DynamicFlags = 1u;
            c.Owner = WowGuid128.Create(HighGuidType703.Player, 42);
            c.PartyGUID = WowGuid128.Create(HighGuidType703.Party, 7);
            c.GuildGUID = WowGuid128.Create(HighGuidType703.Guild, 99);
            c.DisplayID = 12345u;
            for (int i = 0; i < 19; i++) c.Items[i] = (uint)(100 + i);
            c.RaceId = (byte)1;
            c.SexId = (byte)0;
            c.ClassId = (byte)4;
            c.Flags = 0x08u;
            c.FactionTemplate = 1801;
        }) };

        // A real corpse always carries appearance choices — UpdateHandler builds them from
        // the legacy CORPSE_FIELD_BYTES_1/_2 bytes. Native 3.4.3 ships 5 for a human corpse.
        yield return new object[] { "customizations", new System.Action<CorpseData>(c =>
        {
            c.DynamicFlags = 0u;
            c.Owner = WowGuid128.Create(HighGuidType703.Player, 95);
            c.DisplayID = 16125u;
            c.RaceId = (byte)11;
            c.SexId = (byte)0;
            c.ClassId = (byte)1;
            c.Flags = 1u;
            c.FactionTemplate = 1629;
            c.Customizations[0] = new ChrCustomizationChoice(128, 18027);
            c.Customizations[1] = new ChrCustomizationChoice(129, 18047);
            c.Customizations[2] = new ChrCustomizationChoice(130, 28662);
            c.Customizations[3] = new ChrCustomizationChoice(131, 18058);
            c.Customizations[4] = new ChrCustomizationChoice(132, 18072);
        }) };
    }

    [Theory]
    [MemberData(nameof(CorpseScenarios))]
    public void WriteCreateCorpseData_GeneratedMatchesHandPort(string _, System.Action<CorpseData> populate)
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Corpse, 0, 1234, 1);
        var builder = MakeBuilder(guid, session, out var update);
        update.CorpseData ??= new CorpseData();
        populate(update.CorpseData);

        var actual = new WorldPacket();
        builder.WriteCreateCorpseData(actual);

        var expected = new WorldPacket();
        WriteCreateCorpseData_HandPort(expected, update.CorpseData);

        Assert.Equal(expected.GetData(), actual.GetData());
    }

    [Theory]
    [MemberData(nameof(CorpseScenarios))]
    public void WriteUpdateCorpseData_GeneratedMatchesTrinityLayout(string _, System.Action<CorpseData> populate)
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.Corpse, 0, 1234, 1);
        var builder = MakeBuilder(guid, session, out var update);
        update.CorpseData ??= new CorpseData();
        populate(update.CorpseData);

        var actual = new WorldPacket();
        builder.WriteUpdateCorpseData(actual);

        var expected = new WorldPacket();
        WriteUpdateCorpseData_TrinityLayout(expected, update.CorpseData);

        Assert.Equal(expected.GetData(), actual.GetData());
    }

    // Frozen copy of the pre-migration hand-port (ObjectUpdateBuilder.WriteCreateCorpseData).
    private static void WriteCreateCorpseData_HandPort(WorldPacket data, CorpseData corpse)
    {
        // TC343 field order: DynamicFlags FIRST, then Owner, Party, Guild, etc.
        data.WriteUInt32(corpse.DynamicFlags.GetValueOrDefault());
        data.WritePackedGuid128(corpse.Owner ?? WowGuid128.Empty);
        data.WritePackedGuid128(corpse.PartyGUID ?? WowGuid128.Empty);
        data.WritePackedGuid128(corpse.GuildGUID ?? WowGuid128.Empty);
        data.WriteUInt32(corpse.DisplayID.GetValueOrDefault());
        for (int i = 0; i < 19; i++)
            data.WriteUInt32(corpse.Items?[i].GetValueOrDefault() ?? 0);
        data.WriteUInt8(corpse.RaceId.GetValueOrDefault());
        data.WriteUInt8(corpse.SexId.GetValueOrDefault());
        data.WriteUInt8(corpse.ClassId.GetValueOrDefault());
        // Customizations.size(), then Flags/FactionTemplate, then the payload — matching
        // TC CorpseData::WriteCreate and the native 3.4.3 capture
        // (refs/native-captures/wrathion_343_corpse_bones_20260829.pkt, packet 2639, 5 entries).
        int customizationCount = 0;
        if (corpse.Customizations != null)
            for (int i = 0; i < corpse.Customizations.Length; i++)
                if (corpse.Customizations[i] != null) customizationCount++;
        data.WriteUInt32((uint)customizationCount);
        data.WriteUInt32(corpse.Flags.GetValueOrDefault());
        data.WriteInt32(corpse.FactionTemplate.GetValueOrDefault());
        if (corpse.Customizations != null)
        {
            for (int i = 0; i < corpse.Customizations.Length; i++)
            {
                var choice = corpse.Customizations[i];
                if (choice != null)
                {
                    data.WriteUInt32(choice.ChrCustomizationOptionID);
                    data.WriteUInt32(choice.ChrCustomizationChoiceID);
                }
            }
        }
    }

    // Transcribed from TC 3.4.3 UpdateFields.cpp CorpseData::WriteUpdate. Note bit 12 gates
    // Items[19] at bits 13-31 and is a sibling of bit 0, not nested under it. Bit 1 is the
    // Customizations dynamic field, which the proxy never sends.
    private static void WriteUpdateCorpseData_TrinityLayout(WorldPacket data, CorpseData c)
    {
        uint mask = 0u;
        if (c.DynamicFlags.HasValue) mask |= 1u << 2;
        if (c.Owner != null) mask |= 1u << 3;
        if (c.PartyGUID != null) mask |= 1u << 4;
        if (c.GuildGUID != null) mask |= 1u << 5;
        if (c.DisplayID.HasValue) mask |= 1u << 6;
        if (c.RaceId.HasValue) mask |= 1u << 7;
        if (c.SexId.HasValue) mask |= 1u << 8;
        if (c.ClassId.HasValue) mask |= 1u << 9;
        if (c.Flags.HasValue) mask |= 1u << 10;
        if (c.FactionTemplate.HasValue) mask |= 1u << 11;
        for (int i = 0; i < 19; i++)
        {
            if (c.Items[i].HasValue)
            {
                mask |= 1u << (13 + i);
                mask |= 1u << 12;
            }
        }
        if (mask != 0) mask |= 1u;

        data.WriteBits(mask, 32);
        data.FlushBits();
        if ((mask & 1) == 0) return;

        if ((mask & (1u << 2)) != 0) data.WriteUInt32(c.DynamicFlags!.Value);
        if ((mask & (1u << 3)) != 0) data.WritePackedGuid128(c.Owner!.Value);
        if ((mask & (1u << 4)) != 0) data.WritePackedGuid128(c.PartyGUID!.Value);
        if ((mask & (1u << 5)) != 0) data.WritePackedGuid128(c.GuildGUID!.Value);
        if ((mask & (1u << 6)) != 0) data.WriteUInt32(c.DisplayID!.Value);
        if ((mask & (1u << 7)) != 0) data.WriteUInt8(c.RaceId!.Value);
        if ((mask & (1u << 8)) != 0) data.WriteUInt8(c.SexId!.Value);
        if ((mask & (1u << 9)) != 0) data.WriteUInt8(c.ClassId!.Value);
        if ((mask & (1u << 10)) != 0) data.WriteUInt32(c.Flags!.Value);
        if ((mask & (1u << 11)) != 0) data.WriteInt32(c.FactionTemplate!.Value);
        if ((mask & (1u << 12)) != 0)
        {
            for (int i = 0; i < 19; i++)
                if ((mask & (1u << (13 + i))) != 0)
                    data.WriteUInt32(c.Items[i]!.Value);
        }
    }

    // =====================================================================
    // DynamicObject
    // =====================================================================

    public static System.Collections.Generic.IEnumerable<object[]> DynamicObjectScenarios()
    {
        yield return new object[] { "empty", (System.Action<DynamicObjectData>)(_ => { }) };
        yield return new object[] { "caster-only", (System.Action<DynamicObjectData>)(d =>
            d.Caster = WowGuid128.Create(HighGuidType703.Player, 42)) };
        yield return new object[] { "radius-only", (System.Action<DynamicObjectData>)(d => d.Radius = 8.5f) };
        yield return new object[] { "casttime-only", (System.Action<DynamicObjectData>)(d => d.CastTime = 4200u) };
        yield return new object[] { "spell", (System.Action<DynamicObjectData>)(d =>
        {
            d.SpellID = 42208;
            d.SpellXSpellVisualID = 1234;
        }) };
        yield return new object[] { "type-only", (System.Action<DynamicObjectData>)(d => d.Type = 1u) };
        yield return new object[] { "all-fields", (System.Action<DynamicObjectData>)(d =>
        {
            d.Caster = WowGuid128.Create(HighGuidType703.Player, 42);
            d.Type = 1u;
            d.SpellXSpellVisualID = 1234;
            d.SpellID = 42208;
            d.Radius = 8.5f;
            d.CastTime = 4200u;
        }) };
    }

    [Theory]
    [MemberData(nameof(DynamicObjectScenarios))]
    public void WriteCreateDynamicObjectData_GeneratedMatchesHandPort(string _, System.Action<DynamicObjectData> populate)
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.DynamicObject, 0, 1234, 1);
        var builder = MakeBuilder(guid, session, out var update);
        update.DynamicObjectData ??= new DynamicObjectData();
        populate(update.DynamicObjectData);

        var actual = new WorldPacket();
        builder.WriteCreateDynamicObjectData(actual);

        var expected = new WorldPacket();
        WriteCreateDynamicObjectData_HandPort(expected, update.DynamicObjectData);

        Assert.Equal(expected.GetData(), actual.GetData());
    }

    [Theory]
    [MemberData(nameof(DynamicObjectScenarios))]
    public void WriteUpdateDynamicObjectData_GeneratedMatchesTrinityLayout(string _, System.Action<DynamicObjectData> populate)
    {
        var session = CreateGameSession();
        var guid = WowGuid128.Create(HighGuidType703.DynamicObject, 0, 1234, 1);
        var builder = MakeBuilder(guid, session, out var update);
        update.DynamicObjectData ??= new DynamicObjectData();
        populate(update.DynamicObjectData);

        var actual = new WorldPacket();
        builder.WriteUpdateDynamicObjectData(actual);

        var expected = new WorldPacket();
        WriteUpdateDynamicObjectData_TrinityLayout(expected, update.DynamicObjectData);

        Assert.Equal(expected.GetData(), actual.GetData());
    }

    // Frozen copy of the pre-migration hand-port.
    private static void WriteCreateDynamicObjectData_HandPort(WorldPacket data, DynamicObjectData dyn)
    {
        data.WritePackedGuid128(dyn.Caster ?? WowGuid128.Empty);
        data.WriteUInt8(0);
        data.WriteInt32(dyn.SpellXSpellVisualID.GetValueOrDefault());
        data.WriteInt32(dyn.SpellID.GetValueOrDefault());
        data.WriteFloat(dyn.Radius.GetValueOrDefault());
        data.WriteUInt32(dyn.CastTime.GetValueOrDefault());
    }

    // Transcribed from TC 3.4.3 UpdateFields.cpp DynamicObjectData::WriteUpdate.
    private static void WriteUpdateDynamicObjectData_TrinityLayout(WorldPacket data, DynamicObjectData d)
    {
        uint mask = 0u;
        if (d.Caster != null) mask |= 1u << 1;
        if (d.Type.HasValue) mask |= 1u << 2;
        if (d.SpellXSpellVisualID.HasValue) mask |= 1u << 3;
        if (d.SpellID.HasValue) mask |= 1u << 4;
        if (d.Radius.HasValue) mask |= 1u << 5;
        if (d.CastTime.HasValue) mask |= 1u << 6;
        if (mask != 0) mask |= 1u;

        data.WriteBits(mask, 7);
        data.FlushBits();
        if ((mask & 1) == 0) return;

        if ((mask & (1u << 1)) != 0) data.WritePackedGuid128(d.Caster!.Value);
        if ((mask & (1u << 2)) != 0) data.WriteUInt8((byte)d.Type!.Value);
        if ((mask & (1u << 3)) != 0) data.WriteInt32(d.SpellXSpellVisualID!.Value);
        if ((mask & (1u << 4)) != 0) data.WriteInt32(d.SpellID!.Value);
        if ((mask & (1u << 5)) != 0) data.WriteFloat(d.Radius!.Value);
        if ((mask & (1u << 6)) != 0) data.WriteUInt32(d.CastTime!.Value);
    }
}
