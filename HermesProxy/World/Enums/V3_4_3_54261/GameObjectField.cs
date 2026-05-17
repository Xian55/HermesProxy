using HermesProxy.World.Objects;
using HermesProxy.World.Objects.Version.Attributes;

namespace HermesProxy.World.Enums.V3_4_3_54261;

// Phase 5b GameObject section — descriptor-driven WriteCreateGameObjectData /
// WriteUpdateGameObjectData / HasAnyGameObjectFieldSet emit. The V3_4_3 GameObjectData
// wire layout is documented at TC wotlk_classic UpdateFields.cpp:4918-4920 and verified
// against WPP V3_4_0 UpdateFieldsHandler343.ReadUpdateGameObjectData line 3704.
//
// MaskMode = Flat: the Update path writes a single 20-bit changesMask with NO blocks
// prefix. Bit 0 of the mask is the group gate (set automatically by the generator when
// any of bits 4-19 are present); bits 1-3 are reserved for StateWorldEffectIDs /
// EnableDoodadSets / WorldEffects which the proxy doesn't translate yet.
//
// Previous-life note: this file used to hold a legacy descriptor-tree slot-index enum
// (GAMEOBJECT_DISPLAYID = 15, etc.) for the pre-Cataclysm reader. Nothing referenced it
// from V3_4_3_54261 source — slot indices for V3_4_3 ingest live in the legacy
// V3_3_5a_12340 enums. Safe to repurpose.
[DescriptorSection(DataType = typeof(GameObjectData), MaskMode = MaskMode.Flat, MaskWidth = 20)]
public enum GameObjectField
{
    // -------------------------------------------------------------------------
    // Create-path emit order = enum declaration order. Placeholders interleave with
    // real fields at the slots where the descriptor tree carries unused / proxy-
    // unsupported data (server-side flags, GuildGUID create-time empty, custom-data
    // tail). Matches the pre-Phase-5b hand-port wire byte-for-byte.
    //
    // Update-path emit order = bit ascending. The wire writes set bits in numeric
    // order, so generator iterates UpdateField entries sorted by Bit. Source-line
    // ordering here just keeps create + update grouped per logical field for
    // human readability.
    // -------------------------------------------------------------------------

    [DescriptorCreateField(nameof(GameObjectData.DisplayID), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(GameObjectData.DisplayID), DescriptorType.Int32, bit: 4)]
    GAMEOBJECT_FIELD_DISPLAYID,

    [DescriptorCreateField(nameof(GameObjectData.SpellVisualID), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(GameObjectData.SpellVisualID), DescriptorType.UInt32, bit: 5)]
    GAMEOBJECT_FIELD_SPELL_VISUAL_ID,

    [DescriptorCreateField(nameof(GameObjectData.StateSpellVisualID), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(GameObjectData.StateSpellVisualID), DescriptorType.UInt32, bit: 6)]
    GAMEOBJECT_FIELD_STATE_SPELL_VISUAL_ID,

    [DescriptorCreateField(nameof(GameObjectData.StateAnimID), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(GameObjectData.StateAnimID), DescriptorType.UInt32, bit: 7)]
    GAMEOBJECT_FIELD_STATE_ANIM_ID,

    [DescriptorCreateField(nameof(GameObjectData.StateAnimKitID), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(GameObjectData.StateAnimKitID), DescriptorType.UInt32, bit: 8)]
    GAMEOBJECT_FIELD_STATE_ANIM_KIT_ID,

    // TC field slot we don't translate (server-side spawn-tracking flags).
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    GAMEOBJECT_PAD_AFTER_STATE_ANIM_KIT,

    [DescriptorCreateField(nameof(GameObjectData.CreatedBy), DescriptorType.PackedGuid128)]
    [DescriptorUpdateField(nameof(GameObjectData.CreatedBy), DescriptorType.PackedGuid128, bit: 9)]
    GAMEOBJECT_FIELD_CREATED_BY,

    // Create-time GuildGUID is always Empty in the legacy server; Update path can carry a
    // real value (bit 10) once the GO joins a guild structure.
    [DescriptorCreatePlaceholder(DescriptorType.PackedGuid128)]
    GAMEOBJECT_PAD_GUILD_GUID,

    [DescriptorUpdateField(nameof(GameObjectData.GuildGUID), DescriptorType.PackedGuid128, bit: 10)]
    GAMEOBJECT_FIELD_GUILD_GUID,

    [DescriptorCreateField(nameof(GameObjectData.Flags), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(GameObjectData.Flags), DescriptorType.UInt32, bit: 11)]
    GAMEOBJECT_FIELD_FLAGS,

    // ParentRotation = stored quaternion (X/Y/Z/W); fallback identity (0,0,0,1f) when
    // the legacy ingest never populated it (defensive — should not happen for V3_4_3).
    [DescriptorCreateField(nameof(GameObjectData.ParentRotation), DescriptorType.Float,
                           ArrayCount = 4, DefaultExpressionByIndex = "0f,0f,0f,1f")]
    [DescriptorUpdateField(nameof(GameObjectData.ParentRotation), DescriptorType.Float, bit: 12,
                           ArrayCount = 4, DefaultExpressionByIndex = "0f,0f,0f,1f")]
    GAMEOBJECT_FIELD_PARENT_ROTATION,

    [DescriptorCreateField(nameof(GameObjectData.FactionTemplate), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(GameObjectData.FactionTemplate), DescriptorType.Int32, bit: 13)]
    GAMEOBJECT_FIELD_FACTION_TEMPLATE,

    [DescriptorCreateField(nameof(GameObjectData.Level), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(GameObjectData.Level), DescriptorType.Int32, bit: 14)]
    GAMEOBJECT_FIELD_LEVEL,

    [DescriptorCreateField(nameof(GameObjectData.State), DescriptorType.Int8)]
    [DescriptorUpdateField(nameof(GameObjectData.State), DescriptorType.Int8, bit: 15)]
    GAMEOBJECT_FIELD_STATE,

    [DescriptorCreateField(nameof(GameObjectData.TypeID), DescriptorType.Int8)]
    [DescriptorUpdateField(nameof(GameObjectData.TypeID), DescriptorType.Int8, bit: 16)]
    GAMEOBJECT_FIELD_TYPE_ID,

    [DescriptorCreateField(nameof(GameObjectData.PercentHealth), DescriptorType.UInt8, DefaultExpression = "(byte)0")]
    [DescriptorUpdateField(nameof(GameObjectData.PercentHealth), DescriptorType.UInt8, bit: 17)]
    GAMEOBJECT_FIELD_PERCENT_HEALTH,

    [DescriptorCreateField(nameof(GameObjectData.ArtKit), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(GameObjectData.ArtKit), DescriptorType.UInt32, bit: 18)]
    GAMEOBJECT_FIELD_ART_KIT,

    // TC field slot we don't translate.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    GAMEOBJECT_PAD_AFTER_ART_KIT,

    [DescriptorCreateField(nameof(GameObjectData.CustomParam), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(GameObjectData.CustomParam), DescriptorType.UInt32, bit: 19)]
    GAMEOBJECT_FIELD_CUSTOM_PARAM,

    // Trailing TC slot we don't translate.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    GAMEOBJECT_PAD_TAIL,
}
