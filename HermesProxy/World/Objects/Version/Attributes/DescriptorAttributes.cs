using System;

namespace HermesProxy.World.Objects.Version.Attributes;

// Descriptor-tree vocabulary consumed by HermesProxy.SourceGen.ObjectUpdateBuilderGenerator.
// Phase 5 bootstrap scope: scalar fields on the Create path only. Additional attributes
// (arrays, nested structs, bit-masked Update path, viewer filters) land in follow-up PRs
// as the generator is extended — see wotlk.md's Phase 5 Approach B notes.

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
}
