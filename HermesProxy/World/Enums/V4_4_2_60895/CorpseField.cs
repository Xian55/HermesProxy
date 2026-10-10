using HermesProxy.World.Objects;
using HermesProxy.World.Objects.Version.Attributes;

namespace HermesProxy.World.Enums.V4_4_2_60895;

// V3_4_3 Corpse section — descriptor-driven WriteCreateCorpseData / WriteUpdateCorpseData /
// HasAnyCorpseFieldSet emit. Layout verified against TC 3.4.3 UpdateFields.cpp
// CorpseData::WriteCreate / ::WriteUpdate; the struct is HasChangesMask<32>.
//
// Mask shape is unusual and worth stating plainly, because it is why Flat mode had to learn
// ParentBit. TC gates bits 2-11 behind bit 0, but Items[19] live at bits 13-31 behind their
// OWN bit-12 gate, which is a sibling of bit 0, not nested under it. Bit 1 is the
// Customizations dynamic field — the proxy never sends it (create writes size 0), so no
// member claims bit 1 and the nested-mask branch stays dormant.
//
// Before this file was wired, Corpse had a hand-written Create writer and NO Update path at
// all — UpdateHandler parsed DynamicFlags / Flags / Items out of legacy Values blocks and
// the builder then discarded them. The lootable-insignia flag on a battleground corpse is
// the visible case.
//
// Previous-life note: this file held legacy DWORD slot offsets (CORPSE_FIELD_OWNER = 7,
// etc.) for the pre-Cataclysm reader. Slot indices for V3_4_3 ingest live in the legacy
// V3_3_5a_12340 enums, so the V3_4_3 copy was unreferenced and safe to repurpose.
// 4.4.2: CorpseData::WriteUpdate leads with a 1-bit blocks mask and writes the 32-bit block
// only when it is non-zero, so the section is Blocks mode with one block; bits 2-11 hang off the
// bit-0 group gate explicitly, Items keeps its bit-12 gate. The fields are unchanged.
[DescriptorSection(DataType = typeof(CorpseData), MaskMode = MaskMode.Blocks, MaskWidth = 1)]
public enum CorpseField
{
    // Create-path emit order = declaration order, and TC writes DynamicFlags FIRST —
    // ahead of Owner — which is not the bit order. Do not "tidy" this into bit order.
    [DescriptorCreateField(nameof(CorpseData.DynamicFlags), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(CorpseData.DynamicFlags), DescriptorType.UInt32, bit: 2, ParentBit = 0)]
    CORPSE_FIELD_DYNAMIC_FLAGS,

    [DescriptorCreateField(nameof(CorpseData.Owner), DescriptorType.PackedGuid128)]
    [DescriptorUpdateField(nameof(CorpseData.Owner), DescriptorType.PackedGuid128, bit: 3, ParentBit = 0)]
    CORPSE_FIELD_OWNER,

    [DescriptorCreateField(nameof(CorpseData.PartyGUID), DescriptorType.PackedGuid128)]
    [DescriptorUpdateField(nameof(CorpseData.PartyGUID), DescriptorType.PackedGuid128, bit: 4, ParentBit = 0)]
    CORPSE_FIELD_PARTY_GUID,

    [DescriptorCreateField(nameof(CorpseData.GuildGUID), DescriptorType.PackedGuid128)]
    [DescriptorUpdateField(nameof(CorpseData.GuildGUID), DescriptorType.PackedGuid128, bit: 5, ParentBit = 0)]
    CORPSE_FIELD_GUILD_GUID,

    [DescriptorCreateField(nameof(CorpseData.DisplayID), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(CorpseData.DisplayID), DescriptorType.UInt32, bit: 6, ParentBit = 0)]
    CORPSE_FIELD_DISPLAY_ID,

    // Items[19] on the Create path are written inline here (bit order puts them last on the
    // Update path — bits 13-31 behind the bit-12 gate).
    [DescriptorCreateField(nameof(CorpseData.Items), DescriptorType.UInt32, ArrayCount = 19)]
    [DescriptorUpdateField(nameof(CorpseData.Items), DescriptorType.UInt32, bit: 13, ParentBit = 12,
        ArrayMode = ArrayMode.PerElement, ArrayCount = 19)]
    CORPSE_FIELD_ITEMS,

    [DescriptorCreateField(nameof(CorpseData.RaceId), DescriptorType.UInt8)]
    [DescriptorUpdateField(nameof(CorpseData.RaceId), DescriptorType.UInt8, bit: 7, ParentBit = 0)]
    CORPSE_FIELD_RACE_ID,

    [DescriptorCreateField(nameof(CorpseData.SexId), DescriptorType.UInt8)]
    [DescriptorUpdateField(nameof(CorpseData.SexId), DescriptorType.UInt8, bit: 8, ParentBit = 0)]
    CORPSE_FIELD_SEX_ID,

    [DescriptorCreateField(nameof(CorpseData.ClassId), DescriptorType.UInt8)]
    [DescriptorUpdateField(nameof(CorpseData.ClassId), DescriptorType.UInt8, bit: 9, ParentBit = 0)]
    CORPSE_FIELD_CLASS_ID,

    // Customizations.size(). Counts the non-null entries UpdateHandler populated from the
    // legacy CORPSE_FIELD_BYTES_1/_2 appearance bytes; the payload is written after
    // FactionTemplate below, matching TC CorpseData::WriteCreate. A native 3.4.3 server
    // always ships these (5 entries for a human corpse) — see
    // refs/native-captures/wrathion_343_corpse_bones_20260829.pkt packet 2639.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V4_4_2_60895.ObjectUpdateBuilder.WriteCreateCorpseCustomizationsCount))]
    CORPSE_CUSTOMIZATIONS_COUNT,

    [DescriptorCreateField(nameof(CorpseData.Flags), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(CorpseData.Flags), DescriptorType.UInt32, bit: 10, ParentBit = 0)]
    CORPSE_FIELD_FLAGS,

    [DescriptorCreateField(nameof(CorpseData.FactionTemplate), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(CorpseData.FactionTemplate), DescriptorType.Int32, bit: 11, ParentBit = 0)]
    CORPSE_FIELD_FACTION_TEMPLATE,

    // Customizations payload — 2x UInt32 per non-null entry, written last, after
    // FactionTemplate. Count is emitted above between Class and Flags.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32,
        CustomWriter = nameof(HermesProxy.World.Objects.Version.V4_4_2_60895.ObjectUpdateBuilder.WriteCreateCorpseCustomizationsData))]
    CORPSE_CUSTOMIZATIONS_DATA_CUSTOM,
}
