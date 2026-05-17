using HermesProxy.World.Objects;
using HermesProxy.World.Objects.Version.Attributes;

namespace HermesProxy.World.Enums.V3_4_3_54261;

// Phase 5b Item section — descriptor-driven WriteCreateItemData /
// WriteUpdateItemData / HasAnyItemFieldSet emit.
//
// Update path: 43-bit changesMask across 2 blocks of 32 with a 2-bit blocks-prefix.
//   bit 0:  group gate for bits 1-22
//   bit 1:  ArtifactPowers dynamic field (unused)
//   bit 2:  Gems dynamic field (unused)
//   bits 3-22: scalar fields
//   bit 23: group gate for SpellCharges[5]
//   bits 24-28: SpellCharges[0..4] individual change bits
//   bit 29: group gate for Enchantment[13]
//   bits 30-42: Enchantment[0..12] individual change bits
//
// Create path: hand-port emits 4 unconditional PackedGuid128s + owner-gated runs
// of scalars + Enchantment[13] (custom writer) + 11 trailing zero placeholders
// (owner-gated mix) + a final WriteBits(0u, 6) + FlushBits dynamic-field-count
// preamble. Enum declaration order *is* the Create wire byte order.
//
// Previous-life note: file used to hold a legacy descriptor-tree slot-index enum
// (ITEM_FIELD_OWNER = 7, etc.) for the pre-Cataclysm reader. Nothing referenced it
// from V3_4_3_54261 source. Safe to repurpose.
[DescriptorSection(DataType = typeof(ItemData), MaskMode = MaskMode.Blocks, MaskWidth = 2)]
public enum ItemField
{
    // -------------------------------------------------------------------------
    // Create-path emit order = enum declaration order. Mixes real fields, owner-only
    // regions, custom-writer arrays, and trailing zero placeholders into a single
    // flat sequence. The Update path's generator-side sort-by-Bit ignores this
    // ordering and uses the [DescriptorUpdateField(bit: N)] values directly.
    // -------------------------------------------------------------------------

    [DescriptorCreateField(nameof(ItemData.Owner), DescriptorType.PackedGuid128)]
    [DescriptorUpdateField(nameof(ItemData.Owner), DescriptorType.PackedGuid128, bit: 3, ParentBit = 0)]
    ITEM_OWNER,

    [DescriptorCreateField(nameof(ItemData.ContainedIn), DescriptorType.PackedGuid128)]
    [DescriptorUpdateField(nameof(ItemData.ContainedIn), DescriptorType.PackedGuid128, bit: 4, ParentBit = 0)]
    ITEM_CONTAINED_IN,

    [DescriptorCreateField(nameof(ItemData.Creator), DescriptorType.PackedGuid128)]
    [DescriptorUpdateField(nameof(ItemData.Creator), DescriptorType.PackedGuid128, bit: 5, ParentBit = 0)]
    ITEM_CREATOR,

    [DescriptorCreateField(nameof(ItemData.GiftCreator), DescriptorType.PackedGuid128)]
    [DescriptorUpdateField(nameof(ItemData.GiftCreator), DescriptorType.PackedGuid128, bit: 6, ParentBit = 0)]
    ITEM_GIFT_CREATOR,

    // Owner-only region: StackCount, Duration, then SpellCharges[5] per-element.
    [DescriptorCreateField(nameof(ItemData.StackCount), DescriptorType.UInt32, OwnerOnly = true)]
    [DescriptorUpdateField(nameof(ItemData.StackCount), DescriptorType.UInt32, bit: 7, ParentBit = 0)]
    ITEM_STACK_COUNT,

    [DescriptorCreateField(nameof(ItemData.Duration), DescriptorType.UInt32, OwnerOnly = true)]
    [DescriptorUpdateField(nameof(ItemData.Duration), DescriptorType.UInt32, bit: 8, ParentBit = 0)]
    ITEM_DURATION,

    [DescriptorCreateField(nameof(ItemData.SpellCharges), DescriptorType.Int32,
                           ArrayCount = 5, ArrayMode = ArrayMode.PerElement, OwnerOnly = true)]
    [DescriptorUpdateField(nameof(ItemData.SpellCharges), DescriptorType.Int32, bit: 24,
                           ArrayCount = 5, ArrayMode = ArrayMode.PerElement, ParentBit = 23)]
    ITEM_SPELL_CHARGES,

    [DescriptorCreateField(nameof(ItemData.Flags), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(ItemData.Flags), DescriptorType.UInt32, bit: 9, ParentBit = 0)]
    ITEM_FLAGS,

    // Enchantment[13] — nested struct, custom writers handle the 4-field create payload
    // and the inner 4-bit mask + conditional writes for update. Descriptor Type is set
    // to Int32 as a placeholder — CustomWriter routing bypasses the wire-type emit.
    [DescriptorCreateField(nameof(ItemData.Enchantment), DescriptorType.Int32,
                           ArrayCount = 13, ArrayMode = ArrayMode.PerElement,
                           CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteEnchantmentCreate))]
    [DescriptorUpdateField(nameof(ItemData.Enchantment), DescriptorType.Int32, bit: 30,
                           ArrayCount = 13, ArrayMode = ArrayMode.PerElement, ParentBit = 29,
                           CustomWriter = nameof(HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder.WriteEnchantmentUpdate))]
    ITEM_ENCHANTMENT,

    // PropertySeed / RandomProperty: source is uint?, wire is Int32 — cast required.
    [DescriptorCreateField(nameof(ItemData.PropertySeed), DescriptorType.Int32, Cast = "(int)")]
    [DescriptorUpdateField(nameof(ItemData.PropertySeed), DescriptorType.Int32, bit: 10, ParentBit = 0, Cast = "(int)")]
    ITEM_PROPERTY_SEED,

    [DescriptorCreateField(nameof(ItemData.RandomProperty), DescriptorType.Int32, Cast = "(int)")]
    [DescriptorUpdateField(nameof(ItemData.RandomProperty), DescriptorType.Int32, bit: 11, ParentBit = 0, Cast = "(int)")]
    ITEM_RANDOM_PROPERTY,

    // Owner-only durability pair.
    [DescriptorCreateField(nameof(ItemData.Durability), DescriptorType.UInt32, OwnerOnly = true)]
    [DescriptorUpdateField(nameof(ItemData.Durability), DescriptorType.UInt32, bit: 12, ParentBit = 0)]
    ITEM_DURABILITY,

    [DescriptorCreateField(nameof(ItemData.MaxDurability), DescriptorType.UInt32, OwnerOnly = true)]
    [DescriptorUpdateField(nameof(ItemData.MaxDurability), DescriptorType.UInt32, bit: 13, ParentBit = 0)]
    ITEM_MAX_DURABILITY,

    [DescriptorCreateField(nameof(ItemData.CreatePlayedTime), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(ItemData.CreatePlayedTime), DescriptorType.UInt32, bit: 14, ParentBit = 0)]
    ITEM_CREATE_PLAYED_TIME,

    // -------------------------------------------------------------------------
    // Update-only fields (no Create-path emit — hand-port omits them too).
    // Position in enum is arbitrary; generator sorts Update writes by Bit ascending.
    // -------------------------------------------------------------------------

    [DescriptorUpdateField(nameof(ItemData.Context), DescriptorType.Int32, bit: 15, ParentBit = 0)]
    ITEM_CONTEXT_UPDATE_ONLY,

    [DescriptorUpdateField(nameof(ItemData.ArtifactXP), DescriptorType.UInt64, bit: 17, ParentBit = 0)]
    ITEM_ARTIFACT_XP_UPDATE_ONLY,

    [DescriptorUpdateField(nameof(ItemData.ItemAppearanceModID), DescriptorType.UInt8, bit: 18,
                           ParentBit = 0, Cast = "(byte)")]
    ITEM_APPEARANCE_MOD_UPDATE_ONLY,

    // -------------------------------------------------------------------------
    // Trailing Create-path zero placeholders. Hand-port writes these unconditionally
    // (with IsOwner gates as noted). Generator emits them in declaration order.
    // -------------------------------------------------------------------------

    [DescriptorCreatePlaceholder(DescriptorType.Int32)]
    ITEM_PAD_AFTER_CREATE_PLAYED_INT32,

    [DescriptorCreatePlaceholder(DescriptorType.Int64)]
    ITEM_PAD_INT64,

    [DescriptorCreatePlaceholder(DescriptorType.UInt64, OwnerOnly = true)]
    ITEM_PAD_OWNER_UINT64,

    [DescriptorCreatePlaceholder(DescriptorType.UInt8, OwnerOnly = true)]
    ITEM_PAD_OWNER_UINT8,

    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    ITEM_PAD_UINT32_1,

    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    ITEM_PAD_UINT32_2,

    [DescriptorCreatePlaceholder(DescriptorType.UInt32, OwnerOnly = true)]
    ITEM_PAD_OWNER_UINT32,

    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    ITEM_PAD_UINT32_3,

    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    ITEM_PAD_UINT32_4,

    [DescriptorCreatePlaceholder(DescriptorType.UInt16, OwnerOnly = true)]
    ITEM_PAD_OWNER_UINT16,

    // Trailing 6-bit zero write + FlushBits — dynamic field count preamble for
    // ArtifactPowers / Gems (both zero-size for HermesProxy).
    [DescriptorCreateBitsPlaceholder(0u, 6)]
    ITEM_PAD_TRAILING_DYNAMIC_FIELDS,
}
