using System;

namespace HermesProxy.World.Objects.Version.Attributes;

// Descriptor-tree vocabulary consumed by HermesProxy.SourceGen.ObjectUpdateBuilderGenerator.
// Phase 5b: scalar Create + Update paths, fixed-length arrays, flat-mask sections, create
// placeholders. Additional attributes (nested struct masks, dynamic fields, owner-gating
// regions) land in follow-up sections (Item / Container / Unit / Player / ActivePlayer).

/// <summary>
/// Class-level attribute on a per-version descriptor-tree field enum that declares which
/// data section the generator is producing writers for. Required for every enum the
/// generator recognises — the generator scans for this attribute, not for fixed enum names.
/// </summary>
/// <remarks>
/// Example:
/// <code>
/// [DescriptorSection(SectionName = "Object", DataType = typeof(ObjectData),
///                    MaskMode = MaskMode.Blocks, MaskWidth = 4)]
/// public enum ObjectField { ... }
/// </code>
/// SectionName drives method names: <c>WriteCreate{Section}Data</c>, <c>WriteUpdate{Section}Data</c>,
/// <c>HasAny{Section}FieldSet</c>. DataType points at the data class whose nullable
/// fields the generator reads (validated at gen time via <c>HPSG003</c>).
/// </remarks>
[AttributeUsage(AttributeTargets.Enum)]
public sealed class DescriptorSectionAttribute : Attribute
{
    /// <summary>
    /// Optional explicit section name. When unset (default), the generator derives
    /// it from <see cref="DataType"/>'s name by stripping a trailing <c>"Data"</c>
    /// suffix — e.g. <c>typeof(ActivePlayerData)</c> → <c>"ActivePlayer"</c> →
    /// methods <c>WriteCreateActivePlayerData</c> / <c>WriteUpdateActivePlayerData</c> /
    /// <c>HasAnyActivePlayerFieldSet</c>. Set explicitly only when the naming
    /// convention doesn't apply.
    /// </summary>
    public string SectionName { get; set; } = "";
    public Type DataType { get; set; } = typeof(object);
    public MaskMode MaskMode { get; set; } = MaskMode.Blocks;
    public int MaskWidth { get; set; }

    /// <summary>
    /// V3_4_3-only block-bit-0 cascade rule. When true, after Pass-1 builds the
    /// <c>blocks</c> bit-mask but before the blocks-mask prefix is written, the
    /// generator emits a loop that force-sets bit 0 of each of the first 4 blocks
    /// when that block has any non-zero bit. Matches the hand-port behavior at
    /// V3_4_3_54261/ObjectUpdateBuilder.cs:2025-2031 — the V3_4_3 client requires
    /// block-bit-0 as the group gate for the field group housed in blocks 0/1/2/3.
    /// Blocks 4-7 don't follow the rule (bit 0 there is a real array-element bit).
    /// Harmless no-op for sections that don't set the flag.
    /// </summary>
    public bool Cascade { get; set; }

    /// <summary>
    /// Shape of the blocks-mask prefix on the Update path (Blocks-mode sections only).
    /// Defaults to <see cref="Attributes.BlockMaskShape.Bits"/> (current behavior:
    /// single <c>WriteBits(blocksMask, MaskWidth)</c> call, byte-sized mask).
    /// ActivePlayer-only: <see cref="Attributes.BlockMaskShape.UInt32PlusBits16"/>
    /// splits the 48-block mask into <c>WriteUInt32(blocksMask0)</c> +
    /// <c>WriteBits(blocksMask1, 16)</c>. Required because a single 48-bit
    /// <c>WriteBits</c> call exceeds the 32-bit <c>WriteBits</c> limit.
    /// </summary>
    public BlockMaskShape BlockMaskShape { get; set; } = BlockMaskShape.Bits;
}

/// <summary>
/// Wire shape of the blocks-mask prefix on the Update path (Blocks-mode sections).
/// </summary>
public enum BlockMaskShape
{
    /// <summary>
    /// Single <c>data.WriteBits(blocksMask, MaskWidth)</c> call with a byte-typed
    /// accumulator. Used by Item / Container / Unit / Player.
    /// </summary>
    Bits,

    /// <summary>
    /// Two-step write: <c>data.WriteUInt32(blocksMask0)</c> covers blocks 0-31 then
    /// <c>data.WriteBits(blocksMask1, 16)</c> covers blocks 32-47. ActivePlayer's
    /// 48-block changesMask. <c>MaskWidth</c> is ignored under this shape.
    /// </summary>
    UInt32PlusBits16,
}

/// <summary>
/// Layout of the changesMask preamble on the Update path.
/// </summary>
public enum MaskMode
{
    /// <summary>
    /// Blocks-mode: writes an N-bit blocks-mask prefix followed by one 32-bit mask per
    /// non-zero block. Used by Item / Container (2-bit), Unit (8-bit), Player (4-bit),
    /// ActivePlayer (UInt32 + 16-bit).
    /// </summary>
    Blocks,

    /// <summary>
    /// Flat-mode: writes the entire changesMask as a single <c>WriteBits(mask, MaskWidth)</c>
    /// call with no blocks prefix. Used by Object (4-bit flat) and GameObject (20-bit flat).
    /// Bit 0 of the mask is still the group-gate convention, but it lives inside the flat
    /// window.
    /// </summary>
    Flat,
}

/// <summary>
/// Shape of a fixed-length array field on a descriptor.
/// </summary>
public enum ArrayMode
{
    /// <summary>
    /// One bit (at <c>Bit</c>) covers all elements. Presence = "any element set". The
    /// Update path writes all <c>ArrayCount</c> elements in order, applying per-index
    /// defaults when the source slot is null. Example: GameObjectData.ParentRotation[4].
    /// </summary>
    Grouped,

    /// <summary>
    /// Each element has its own bit at <c>Bit + i</c> for i in <c>[0, ArrayCount)</c>.
    /// Update writes only the elements whose bit is set, in ascending index order.
    /// Example: ContainerData.Slots[36].
    /// </summary>
    PerElement,
}

/// <summary>
/// Annotates a member of a per-version descriptor-tree field enum (e.g.
/// <c>V3_4_3_54261.ObjectField.OBJECT_FIELD_ENTRY</c>) with the mapping needed for the
/// generator to emit the matching <c>WriteCreate{Type}Data</c> scalar write.
/// </summary>
/// <remarks>
/// Example usage:
/// <code>
/// [DescriptorCreateField(nameof(ObjectData.EntryID), DescriptorType.Int32)]
/// OBJECT_FIELD_ENTRY = 4,
/// </code>
/// Enum members without this attribute are skipped — several descriptor-tree slots
/// (e.g. <c>OBJECT_FIELD_GUID</c>) aren't written as fields because the GUID is emitted
/// separately in the update preamble.
/// </remarks>
[AttributeUsage(AttributeTargets.Field)]
public sealed class DescriptorCreateFieldAttribute : Attribute
{
    public DescriptorCreateFieldAttribute(string sourceProperty, DescriptorType type)
    {
        SourceProperty = sourceProperty;
        Type = type;
    }

    /// <summary>Property name on the source data struct (e.g. <c>EntryID</c> on <c>ObjectData</c>).</summary>
    public string SourceProperty { get; }

    /// <summary>Controls which <c>ByteBuffer.Write…</c> overload the generator emits.</summary>
    public DescriptorType Type { get; }

    /// <summary>
    /// Optional literal-string expression used when the source property is null. For
    /// <c>float? Scale</c> the fork writes <c>Scale ?? 1f</c> — set <c>DefaultExpression = "1f"</c>.
    /// When null, the generator emits <c>.GetValueOrDefault()</c> (i.e. zero for numeric types).
    /// </summary>
    public string? DefaultExpression { get; set; }

    /// <summary>
    /// For array source properties: fixed element count. Generator emits a per-index write
    /// loop using either <see cref="DefaultExpressionByIndex"/> (comma-separated) or
    /// <see cref="DefaultExpression"/> uniformly when the source slot is null.
    /// </summary>
    public int ArrayCount { get; set; }

    /// <summary>
    /// Comma-separated per-index defaults for array fields. e.g. <c>"0f,0f,0f,1f"</c> for
    /// a 4-element ParentRotation where index 3's default is the quaternion identity W.
    /// Falls back to <see cref="DefaultExpression"/> when unset.
    /// </summary>
    public string? DefaultExpressionByIndex { get; set; }

    /// <summary>
    /// Shape of the array — controls predicate / write emit. Default is <see cref="Attributes.ArrayMode.Grouped"/>.
    /// Ignored when <see cref="ArrayCount"/> is 0.
    /// </summary>
    public ArrayMode ArrayMode { get; set; }

    /// <summary>
    /// When true, generator wraps this field's Create-path write in <c>if (IsOwner) { … }</c>.
    /// Matches the existing <c>ObjectUpdateBuilder.IsOwner</c> property used by the hand-port.
    /// Update path is unaffected — it relies on field presence (<c>.HasValue</c>) alone.
    /// </summary>
    public bool OwnerOnly { get; set; }

    /// <summary>
    /// Optional method name on <c>ObjectUpdateBuilder</c> to delegate the wire write to.
    /// Used for nested-struct array elements (e.g. <c>WriteEnchantmentCreate</c>) where the
    /// element has its own internal layout the generator can't synthesise. The generator
    /// emits <c>{CustomWriter}(data, src.{SourceProperty}, i)</c> per index in array mode.
    /// </summary>
    public string? CustomWriter { get; set; }

    /// <summary>
    /// Optional literal cast prefix applied before the value expression. Used when the
    /// source property is a wider type than the wire type (e.g. <c>uint?</c> source
    /// written as <c>UInt8</c>): set <c>Cast = "(byte)"</c> → <c>data.WriteUInt8((byte)src.X.Value)</c>.
    /// </summary>
    public string? Cast { get; set; }
}

/// <summary>
/// Annotates a member of a per-version descriptor-tree field enum with the
/// changesMask bit + source-property mapping needed for the generator to emit the
/// matching <c>WriteUpdate{Type}Data</c> partial-update writer.
/// </summary>
/// <remarks>
/// The <paramref name="bit"/> argument is the position within the section's
/// changesMask (the value sent on the wire), independent of the descriptor-tree
/// enum value. For V3_4_3 <c>ObjectData</c> the layout is bit 0 = group gate,
/// bit 1 = EntryID, bit 2 = DynamicFlags, bit 3 = Scale.
/// </remarks>
[AttributeUsage(AttributeTargets.Field)]
public sealed class DescriptorUpdateFieldAttribute : Attribute
{
    public DescriptorUpdateFieldAttribute(string sourceProperty, DescriptorType type, int bit)
    {
        SourceProperty = sourceProperty;
        Type = type;
        Bit = bit;
    }

    /// <summary>Property name on the source data struct.</summary>
    public string SourceProperty { get; }

    /// <summary>Wire type — selects the <c>ByteBuffer.Write…</c> overload.</summary>
    public DescriptorType Type { get; }

    /// <summary>changesMask bit position within the section.</summary>
    public int Bit { get; }

    /// <summary>
    /// For array source properties: fixed element count. <see cref="ArrayMode"/> controls
    /// whether <see cref="Bit"/> is a single any-element-set indicator (Grouped) or the
    /// start of a per-element bit-range Bit..Bit+ArrayCount-1 (PerElement).
    /// </summary>
    public int ArrayCount { get; set; }

    /// <summary>
    /// Shape of the array — controls predicate / write emit. Default is <see cref="Attributes.ArrayMode.Grouped"/>.
    /// Ignored when <see cref="ArrayCount"/> is 0.
    /// </summary>
    public ArrayMode ArrayMode { get; set; }

    /// <summary>
    /// Comma-separated per-index defaults for array fields (see <see cref="DescriptorCreateFieldAttribute.DefaultExpressionByIndex"/>).
    /// </summary>
    public string? DefaultExpressionByIndex { get; set; }

    /// <summary>
    /// Optional parent group-bit. When the field's presence predicate fires the generator
    /// sets both <see cref="Bit"/> and <see cref="ParentBit"/> on the section's changesMask.
    /// Default <c>-1</c> means "no parent gate" (no extra bit set). Used by Container
    /// (bit 0 gates NumSlots, bit 2 gates the Slots array), Unit (block-3 group bit 96 gates
    /// ComboTarget), etc.
    /// </summary>
    public int ParentBit { get; set; } = -1;

    /// <summary>
    /// Optional method name on <c>ObjectUpdateBuilder</c> to delegate the wire write to.
    /// For arrays in <see cref="ArrayMode"/> = <see cref="Attributes.ArrayMode.PerElement"/>,
    /// generator emits <c>{CustomWriter}(data, src.{SourceProperty}, i)</c> per element
    /// gated on the element's bit. For scalar fields, generator emits
    /// <c>if (blocks.IsBitSet({Bit})) {CustomWriter}(data, src);</c> instead of the
    /// inline scalar write. Used for sanitised fields (Unit's Flags2 → SanitizeFlags2)
    /// and impersonation overrides (Race/Class/Sex on Create).
    /// </summary>
    public string? CustomWriter { get; set; }

    /// <summary>
    /// Optional literal cast prefix applied before the value expression. e.g.
    /// <c>Cast = "(byte)"</c> → <c>data.WriteUInt8((byte)src.X.Value)</c>.
    /// </summary>
    public string? Cast { get; set; }

    /// <summary>
    /// Optional C# expression that overrides the default presence check. Default for a
    /// scalar field is <c>src.X.HasValue</c> (value type) or <c>src.X != null</c>
    /// (reference / nullable struct). For arrays the default per-element check is the
    /// same applied to <c>[i]</c>. The string may contain the literal token <c>{i}</c>,
    /// which the generator string-replaces with the current element index. Example:
    /// <c>CustomPredicate = "src.NpcFlags[{i}].HasValue &amp;&amp; src.NpcFlags[{i}] != 0"</c>.
    /// </summary>
    public string? CustomPredicate { get; set; }

    /// <summary>
    /// When true, the generator runs Pass-1 mask-bit setting normally but emits
    /// <i>nothing</i> in Pass-2 write phase. Used for arrays whose wire writes are
    /// claimed by a sibling <see cref="DescriptorCustomFieldAttribute"/> group writer
    /// that interleaves across multiple arrays (e.g. Unit Stats/StatPosBuff/StatNegBuff
    /// share bit-174 parent and write interleaved-by-index, not by per-array bit order).
    /// </summary>
    public bool MaskOnly { get; set; }

    /// <summary>
    /// Optional override for the Pass-2 sort key. When set, the generator sorts this
    /// field's write emit by <see cref="WriteOrder"/> instead of <see cref="Bit"/>.
    /// Used for fields whose wire-order differs from their changesMask bit position —
    /// notably ActivePlayer's <c>PvpInfo</c> (bit 607 but written AFTER bit-1512
    /// GlyphSlots/Glyphs, matching TC's ActivePlayerData::WriteUpdate layout).
    /// Default 0 = use <see cref="Bit"/> for sorting (current behavior).
    /// </summary>
    public int WriteOrder { get; set; }
}

/// <summary>
/// Marks a synthetic descriptor enum member that does not map to a single source
/// property — instead the generator emits a delegated call to a custom writer that
/// handles an entire field group (interleaved arrays, nested dynamic-update-fields,
/// inline composite structs). The custom writer receives the section's
/// <c>StackBitMask</c> by ref so it can read per-element bits within the group.
/// </summary>
/// <remarks>
/// Example:
/// <code>
/// [DescriptorCustomField("PowerGroup", bit: 116, customWriter: "WriteUnitPowerGroup")]
/// UNIT_GROUP_POWER,
/// </code>
/// Generator emits, on Update path:
/// <code>
/// if (blocks.IsBitSet(116)) WriteUnitPowerGroup(data, ref blocks);
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Field)]
public sealed class DescriptorCustomFieldAttribute : Attribute
{
    public DescriptorCustomFieldAttribute(string label, int bit, string customWriter)
    {
        Label = label;
        Bit = bit;
        CustomWriter = customWriter;
    }

    /// <summary>Free-form label for diagnostics. Not emitted into generated code.</summary>
    public string Label { get; }

    /// <summary>changesMask bit position the writer is gated on.</summary>
    public int Bit { get; }

    /// <summary>Method name on <c>ObjectUpdateBuilder</c>. Generator emits the call.</summary>
    public string CustomWriter { get; }

    /// <summary>Optional parent group-bit. Same semantics as scalar Update field's <c>ParentBit</c>.</summary>
    public int ParentBit { get; set; } = -1;

    /// <summary>
    /// When true, the generator skips the Pass-1 unconditional <c>blocks.SetBit({Bit});</c>
    /// emit (relies on sibling <see cref="DescriptorUpdateFieldAttribute"/> declarations
    /// with <c>MaskOnly = true</c> to set the bits). Pass-2 still emits the gated
    /// <c>if (blocks.IsBitSet({Bit})) {CustomWriter}(data, ref blocks);</c> call. Used
    /// when one custom writer fans out across multiple array fields' bits.
    /// </summary>
    public bool WriteOnly { get; set; }
}

/// <summary>
/// Marks a synthetic descriptor enum member that emits a delegated call <i>between</i>
/// the blocks-mask prefix write and the section's <c>FlushBits()</c>. The custom writer
/// receives the section's <c>StackBitMask</c> by ref. Used for dynamic-update-field
/// preambles like ChannelObjects' size + per-element bitmask, which must be byte-aligned
/// before the field-payload writes start. Generator emits:
/// <code>
/// if (blocks.IsBitSet({Bit})) {CustomWriter}(data, ref blocks);
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class DescriptorMaskPreambleAttribute : Attribute
{
    public DescriptorMaskPreambleAttribute(int bit, string customWriter)
    {
        Bit = bit;
        CustomWriter = customWriter;
    }

    /// <summary>changesMask bit that gates the preamble emit.</summary>
    public int Bit { get; }

    /// <summary>Method name on <c>ObjectUpdateBuilder</c>. Generator emits the call.</summary>
    public string CustomWriter { get; }
}

/// <summary>
/// Marks a synthetic descriptor enum member that emits a delegated call at the very
/// start of Pass-1 mask-build (before any field's <c>blocks.SetBit</c> emit). The
/// method receives the section's <c>StackBitMask</c> by ref and may set bits, read
/// instance state (<c>_gameState</c>, <c>_updateData</c>), and perform side-effects
/// like consuming a dirty flag. Used for ActivePlayer's GlyphsDirty capture-and-clear
/// + multi-bit fan-out (sets 1512 + 1513-1518 + 1519-1524 in one shot).
/// Generator emits:
/// <code>
/// {CustomWriter}(ref blocks);
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class DescriptorMaskMutatorAttribute : Attribute
{
    public DescriptorMaskMutatorAttribute(string customWriter)
    {
        CustomWriter = customWriter;
    }

    /// <summary>Method name on <c>ObjectUpdateBuilder</c>. Generator emits the call.</summary>
    public string CustomWriter { get; }

    /// <summary>
    /// Optional C# expression evaluated inside <c>HasAny{Section}FieldSet</c>. When it
    /// returns true the section is considered to have changes and the Update writer fires.
    /// Required when the mutator sets bits that aren't covered by any
    /// <see cref="DescriptorUpdateFieldAttribute"/>'s default presence check —
    /// otherwise loot/bag-pickup / glyph-dirty / etc. silently skip the Values update.
    /// Has access to <c>src</c> (the section's data) and instance members
    /// (<c>_gameState</c>, etc.).
    /// </summary>
    public string? HasAnyPredicate { get; set; }
}

/// <summary>
/// Marks a synthetic descriptor enum member that emits an unconditional
/// <c>data.FlushBits()</c> after the named bit's scalar payload write but before
/// subsequent array payload writes. Used for ActivePlayer Block 102 where the
/// hand-port has an explicit mid-write FlushBits after the last scalar
/// (<c>GlyphsEnabled</c> at bit 120, file:1857) before the array writes for
/// InvSlots and beyond.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class DescriptorUpdatePostFlushAttribute : Attribute
{
    public DescriptorUpdatePostFlushAttribute(int afterBit)
    {
        AfterBit = afterBit;
    }

    /// <summary>Bit position whose write the flush follows. Inserted in Pass-2 sorted-by-Bit emission.</summary>
    public int AfterBit { get; }
}

/// <summary>
/// Marks a synthetic descriptor enum member that emits an unconditional literal bit-write
/// between the blocks-mask prefix write and <c>FlushBits()</c>. Counterpart of
/// <see cref="DescriptorCreateBitsPlaceholderAttribute"/> but on the Update path.
/// Generator emits <c>data.WriteBits({Value}u, {BitCount});</c> at that position, with no
/// gate and no trailing flush (the surrounding emit handles FlushBits). Used for fixed
/// preamble bits like Player's <c>IsQuestLogChangesMaskSkipped = true</c> marker.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class DescriptorUpdateBitsPreambleAttribute : Attribute
{
    public DescriptorUpdateBitsPreambleAttribute(uint value, int bitCount)
    {
        Value = value;
        BitCount = bitCount;
    }

    /// <summary>Literal value written by <c>WriteBits</c>.</summary>
    public uint Value { get; }

    /// <summary>Bit-count argument to <c>WriteBits(value, bitCount)</c>.</summary>
    public int BitCount { get; }
}

/// <summary>
/// Marks an enum member that produces a fixed placeholder write on the Create path (no
/// source-property — emits a literal zero or other constant). GameObject has 4 such
/// placeholder slots interleaved with real fields; rather than re-encode the magic
/// numbers in C# bodies they live as enum members with this attribute.
/// </summary>
/// <remarks>
/// Example:
/// <code>
/// [DescriptorCreatePlaceholder(DescriptorType.UInt32, "0u")]
/// GAMEOBJECT_PLACEHOLDER_PAD_AFTER_STATE_ANIM_KIT,
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Field)]
public sealed class DescriptorCreatePlaceholderAttribute : Attribute
{
    public DescriptorCreatePlaceholderAttribute(DescriptorType type, string literalExpression)
    {
        Type = type;
        LiteralExpression = literalExpression;
    }

    /// <summary>
    /// Type-only overload — generator falls back to the type's natural zero literal
    /// (<c>0</c> / <c>0u</c> / <c>0f</c> / <c>WowGuid128.Empty</c> / ...) at emit time.
    /// Use when the placeholder is a pure zero/default; reserve the
    /// <c>(type, literalExpression)</c> overload for non-default fillers
    /// (e.g. <c>"1f"</c> identity floats, <c>"-1"</c> sentinels).
    /// </summary>
    public DescriptorCreatePlaceholderAttribute(DescriptorType type)
    {
        Type = type;
        LiteralExpression = "";
    }

    /// <summary>Wire type — selects the <c>ByteBuffer.Write…</c> overload.</summary>
    public DescriptorType Type { get; }

    /// <summary>Literal expression to emit (e.g. <c>"0u"</c>, <c>"WowGuid128.Empty"</c>).
    /// When empty, generator falls back to the type's natural zero literal. Ignored when
    /// <see cref="CustomWriter"/> is set.</summary>
    public string LiteralExpression { get; }

    /// <summary>
    /// When true, generator wraps this placeholder write in <c>if (IsOwner) { … }</c>.
    /// </summary>
    public bool OwnerOnly { get; set; }

    /// <summary>
    /// When set, instead of emitting <c>data.Write{Type}({LiteralExpression})</c> the
    /// generator emits <c>{CustomWriter}(data, src);</c> at this position in the Create
    /// sequence. Repurposes the placeholder slot as a Create-path custom-writer hook —
    /// used for interleaved arrays and conditional defaults that don't fit the per-field
    /// descriptor shape.
    /// </summary>
    public string? CustomWriter { get; set; }
}

/// <summary>
/// Marks an enum member that emits a fixed-width bit-write on the Create path, optionally
/// followed by <see cref="ByteBuffer.FlushBits"/>. Used for trailing dynamic-field-count
/// preambles like Item's <c>WriteBits(0u, 6) + FlushBits</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class DescriptorCreateBitsPlaceholderAttribute : Attribute
{
    public DescriptorCreateBitsPlaceholderAttribute(uint value, int bitCount)
    {
        Value = value;
        BitCount = bitCount;
    }

    /// <summary>Literal value written by <c>WriteBits</c>.</summary>
    public uint Value { get; }

    /// <summary>Bit-count argument to <c>WriteBits(value, bitCount)</c>.</summary>
    public int BitCount { get; }

    /// <summary>When true, generator emits <c>FlushBits()</c> immediately after the bits write.</summary>
    public bool Flush { get; set; } = true;

    /// <summary>When true, wraps the bit-write (and optional flush) in <c>if (IsOwner) { … }</c>.</summary>
    public bool OwnerOnly { get; set; }
}

/// <summary>
/// Wire type of a descriptor-tree scalar field. Selects which <c>ByteBuffer.Write…</c>
/// overload the generator emits.
/// </summary>
public enum DescriptorType
{
    Int32,
    UInt32,
    Int64,
    UInt64,
    Float,
    Int8,
    UInt8,
    Int16,
    UInt16,
    PackedGuid128,
}
