using HermesProxy.World.Objects;
using HermesProxy.World.Objects.Version.Attributes;

namespace HermesProxy.World.Enums.V4_4_2_60895;

// Cataclysm Classic 4.4.2 Item section: as 3.4.3 except that Context is a byte, DynamicFlags2
// became ZoneFlags in the same slot, and ItemEnchantment's two trailing bytes became one uint16
// (same create bytes; the enchant update mask is 5 bits, see WriteEnchantmentUpdate).
//
// Phase 5b Item section — descriptor-driven WriteCreateItemData /
// WriteUpdateItemData / HasAnyItemFieldSet emit.
//
// Update path: 43-bit changesMask across 2 blocks of 32 with a 2-bit blocks-prefix.
//   bit 0:  group gate for bits 1-22
//   bit 1:  ArtifactPowers dynamic field (unused)
//   bit 2:  Gems dynamic field (mask preamble + per-element body, custom writers)
//   bits 3-22: scalar fields
//   bit 23: group gate for SpellCharges[5]
//   bits 24-28: SpellCharges[0..4] individual change bits
//   bit 29: group gate for Enchantment[13]
//   bits 30-42: Enchantment[0..12] individual change bits
//
// Create path: 4 unconditional PackedGuid128s + owner-gated runs of scalars +
// Enchantment[13] (custom writer) + trailing scalars (owner-gated mix) that carry
// the two dynamic-field sizes + the Gems payload + a final WriteBits(0u, 6) +
// FlushBits. That trailing 6-bit write is ItemModList::WriteCreate for the Modifiers
// field, not a dynamic-field-count preamble as an earlier comment here claimed.
// Enum declaration order *is* the Create wire byte order.
//
// Previous-life note: file used to hold a legacy descriptor-tree slot-index enum
// (ITEM_FIELD_OWNER = 7, etc.) for the pre-Cataclysm reader. Nothing referenced it
// from V4_4_2_60895 source. Safe to repurpose.
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
    [DescriptorCreateField(nameof(ItemData.StackCount), DescriptorType.UInt32, Visibility = FieldVisibility.Owner)]
    [DescriptorUpdateField(nameof(ItemData.StackCount), DescriptorType.UInt32, bit: 7, ParentBit = 0)]
    ITEM_STACK_COUNT,

    [DescriptorCreateField(nameof(ItemData.Duration), DescriptorType.UInt32, Visibility = FieldVisibility.Owner)]
    [DescriptorUpdateField(nameof(ItemData.Duration), DescriptorType.UInt32, bit: 8, ParentBit = 0)]
    ITEM_DURATION,

    [DescriptorCreateField(nameof(ItemData.SpellCharges), DescriptorType.Int32,
                           ArrayCount = 5, ArrayMode = ArrayMode.PerElement, Visibility = FieldVisibility.Owner)]
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
                           CustomWriter = nameof(HermesProxy.World.Objects.Version.V4_4_2_60895.ObjectUpdateBuilder.WriteEnchantmentCreate))]
    [DescriptorUpdateField(nameof(ItemData.Enchantment), DescriptorType.Int32, bit: 30,
                           ArrayCount = 13, ArrayMode = ArrayMode.PerElement, ParentBit = 29,
                           CustomWriter = nameof(HermesProxy.World.Objects.Version.V4_4_2_60895.ObjectUpdateBuilder.WriteEnchantmentUpdate))]
    ITEM_ENCHANTMENT,

    // PropertySeed / RandomProperty: source is uint?, wire is Int32 — cast required.
    [DescriptorCreateField(nameof(ItemData.PropertySeed), DescriptorType.Int32, Cast = "(int)")]
    [DescriptorUpdateField(nameof(ItemData.PropertySeed), DescriptorType.Int32, bit: 10, ParentBit = 0, Cast = "(int)")]
    ITEM_PROPERTY_SEED,

    [DescriptorCreateField(nameof(ItemData.RandomProperty), DescriptorType.Int32, Cast = "(int)")]
    [DescriptorUpdateField(nameof(ItemData.RandomProperty), DescriptorType.Int32, bit: 11, ParentBit = 0, Cast = "(int)")]
    ITEM_RANDOM_PROPERTY,

    // Owner-only durability pair.
    [DescriptorCreateField(nameof(ItemData.Durability), DescriptorType.UInt32, Visibility = FieldVisibility.Owner)]
    [DescriptorUpdateField(nameof(ItemData.Durability), DescriptorType.UInt32, bit: 12, ParentBit = 0)]
    ITEM_DURABILITY,

    [DescriptorCreateField(nameof(ItemData.MaxDurability), DescriptorType.UInt32, Visibility = FieldVisibility.Owner)]
    [DescriptorUpdateField(nameof(ItemData.MaxDurability), DescriptorType.UInt32, bit: 13, ParentBit = 0)]
    ITEM_MAX_DURABILITY,

    [DescriptorCreateField(nameof(ItemData.CreatePlayedTime), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(ItemData.CreatePlayedTime), DescriptorType.UInt32, bit: 14, ParentBit = 0)]
    ITEM_CREATE_PLAYED_TIME,

    // -------------------------------------------------------------------------
    // Update-only fields (no Create-path emit — hand-port omits them too).
    // Position in enum is arbitrary; generator sorts Update writes by Bit ascending.
    // -------------------------------------------------------------------------

    [DescriptorUpdateField(nameof(ItemData.Context), DescriptorType.UInt8, bit: 15, ParentBit = 0, Cast = "(byte)")]
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

    // Context, a byte from 4.4.2 on.
    [DescriptorCreatePlaceholder(DescriptorType.UInt8)]
    ITEM_PAD_AFTER_CREATE_PLAYED_INT32,

    [DescriptorCreatePlaceholder(DescriptorType.Int64)]
    ITEM_PAD_INT64,

    [DescriptorCreatePlaceholder(DescriptorType.UInt64, Visibility = FieldVisibility.Owner)]
    ITEM_PAD_OWNER_UINT64,

    [DescriptorCreatePlaceholder(DescriptorType.UInt8, Visibility = FieldVisibility.Owner)]
    ITEM_PAD_OWNER_UINT8,

    // ArtifactPowers.size() — stays zero-size; artifacts do not exist in this content
    // and there is no legacy source for them.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    ITEM_ARTIFACT_POWERS_SIZE,

    // Gems.size() — element count for the Gems dynamic field. The payload itself is
    // written further down, after DEBUGItemLevel (see ITEM_GEMS_BODY_CREATE).
    [DescriptorCreatePlaceholder(DescriptorType.UInt32,
                                 CustomWriter = nameof(HermesProxy.World.Objects.Version.V4_4_2_60895.ObjectUpdateBuilder.WriteCreateItemGemsSize))]
    ITEM_GEMS_SIZE_CREATE,

    // ZoneFlags (DynamicFlags2 in 3.4.3)
    [DescriptorCreatePlaceholder(DescriptorType.UInt32, Visibility = FieldVisibility.Owner)]
    ITEM_PAD_OWNER_UINT32,

    // ItemBonusKey: Int32 ItemID + UInt32 BonusListIDs.size() (always 0 here).
    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    ITEM_PAD_UINT32_3,

    [DescriptorCreatePlaceholder(DescriptorType.UInt32)]
    ITEM_PAD_UINT32_4,

    // DEBUGItemLevel
    [DescriptorCreatePlaceholder(DescriptorType.UInt16, Visibility = FieldVisibility.Owner)]
    ITEM_PAD_OWNER_UINT16,

    // Gems payload — ArtifactPowers[] elements would precede it, but that field is
    // always zero-size so it contributes nothing. 37 bytes per element; the count must
    // agree with ITEM_GEMS_SIZE_CREATE above.
    [DescriptorCreatePlaceholder(DescriptorType.UInt32,
                                 CustomWriter = nameof(HermesProxy.World.Objects.Version.V4_4_2_60895.ObjectUpdateBuilder.WriteCreateItemGemsBody))]
    ITEM_GEMS_BODY_CREATE,

    // Trailing 6-bit zero write + FlushBits — ItemModList::WriteCreate for the
    // Modifiers field (Values.size() == 0).
    [DescriptorCreateBitsPlaceholder(0u, 6)]
    ITEM_PAD_TRAILING_DYNAMIC_FIELDS,

    // -------------------------------------------------------------------------
    // Gems dynamic field, Update path. Two emit phases, mirroring UnitData's
    // ChannelObjects:
    //   1. MaskPreamble between the blocks-mask prefix and FlushBits (32-bit size +
    //      per-element changed bitmask).
    //   2. Body at the bit-2 position in the block-0 payload — emitted ahead of bit 3
    //      (Owner) because the generator sorts Update writes by bit ascending.
    // The preamble sets no bits; the field entry below sets them, gated on
    // HasGemsUpdate. SourceProperty names HasGemsUpdate only to bind a member —
    // CustomPredicate and CustomWriter own both the mask build and the wire write.
    // -------------------------------------------------------------------------

    [DescriptorMaskPreamble(bit: 2, customWriter: nameof(HermesProxy.World.Objects.Version.V4_4_2_60895.ObjectUpdateBuilder.WriteUpdateItemGemsMaskPreamble))]
    ITEM_GEMS_PREAMBLE_UPDATE,

    [DescriptorUpdateField(nameof(ItemData.HasGemsUpdate), DescriptorType.Int32, bit: 2, ParentBit = 0,
                           CustomPredicate = "src.HasGemsUpdate",
                           CustomWriter = nameof(HermesProxy.World.Objects.Version.V4_4_2_60895.ObjectUpdateBuilder.WriteUpdateItemGemsBody))]
    ITEM_GEMS_BODY_UPDATE,
}
