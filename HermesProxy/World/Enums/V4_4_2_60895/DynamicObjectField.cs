using HermesProxy.World.Objects;
using HermesProxy.World.Objects.Version.Attributes;

namespace HermesProxy.World.Enums.V4_4_2_60895;

// V3_4_3 DynamicObject section — descriptor-driven WriteCreateDynamicObjectData /
// WriteUpdateDynamicObjectData / HasAnyDynamicObjectFieldSet emit. Layout verified against
// TC 3.4.3 UpdateFields.cpp DynamicObjectData::WriteCreate / ::WriteUpdate; the struct is
// HasChangesMask<7>, and the Update path writes a single 7-bit changesMask with bit 0 as
// the group gate.
//
// Before this file was wired, DynamicObject had a hand-written Create writer and NO Update
// path at all — UpdateHandler parsed Radius / SpellID / CastTime out of legacy Values
// blocks and the builder then discarded them. Persistent-AoE spells (Blizzard, Rain of
// Fire, Consecration, Death and Decay) are the visible case.
//
// Previous-life note: this file held legacy DWORD slot offsets (DYNAMICOBJECT_CASTER = 7,
// etc.) for the pre-Cataclysm reader. Slot indices for V3_4_3 ingest live in the legacy
// V3_3_5a_12340 enums, so the V3_4_3 copy was unreferenced and safe to repurpose.
[DescriptorSection(DataType = typeof(DynamicObjectData), MaskMode = MaskMode.Flat, MaskWidth = 7)]
public enum DynamicObjectField
{
    [DescriptorCreateField(nameof(DynamicObjectData.Caster), DescriptorType.PackedGuid128)]
    [DescriptorUpdateField(nameof(DynamicObjectData.Caster), DescriptorType.PackedGuid128, bit: 1)]
    DYNAMICOBJECT_CASTER,

    // Type is a legacy-unsourced byte (TC writes uint8(Type)); the hand-port shipped 0.
    [DescriptorCreatePlaceholder(DescriptorType.UInt8)]
    [DescriptorUpdateField(nameof(DynamicObjectData.Type), DescriptorType.UInt8, bit: 2, Cast = "(byte)")]
    DYNAMICOBJECT_TYPE,

    [DescriptorCreateField(nameof(DynamicObjectData.SpellXSpellVisualID), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(DynamicObjectData.SpellXSpellVisualID), DescriptorType.Int32, bit: 3)]
    DYNAMICOBJECT_SPELL_X_SPELL_VISUAL_ID,

    [DescriptorCreateField(nameof(DynamicObjectData.SpellID), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(DynamicObjectData.SpellID), DescriptorType.Int32, bit: 4)]
    DYNAMICOBJECT_SPELLID,

    [DescriptorCreateField(nameof(DynamicObjectData.Radius), DescriptorType.Float)]
    [DescriptorUpdateField(nameof(DynamicObjectData.Radius), DescriptorType.Float, bit: 5)]
    DYNAMICOBJECT_RADIUS,

    [DescriptorCreateField(nameof(DynamicObjectData.CastTime), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(DynamicObjectData.CastTime), DescriptorType.UInt32, bit: 6)]
    DYNAMICOBJECT_CASTTIME,
}
