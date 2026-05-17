using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace HermesProxy.SourceGen;

/// <summary>
/// Emits the descriptor-tree <c>WriteCreate{Section}Data</c>, <c>WriteUpdate{Section}Data</c>,
/// and <c>HasAny{Section}FieldSet</c> partial methods on per-version
/// <c>ObjectUpdateBuilder</c> classes — the serializers for WotLK Classic 3.4.3's
/// hierarchical <c>SMSG_UPDATE_OBJECT</c> wire format.
///
/// Scanning rule: any per-version <c>V*</c> namespace under <c>HermesProxy.World.Enums</c>
/// containing one or more enums decorated with <c>[DescriptorSection]</c> is processed.
/// Each such enum drives one set of methods on the matching
/// <c>HermesProxy.World.Objects.Version.{Ver}.ObjectUpdateBuilder</c>.
///
/// Diagnostics:
///   HPSG003 — a <c>[DescriptorCreateField]</c> / <c>[DescriptorUpdateField]</c>
///             <c>SourceProperty</c> references a member that does not exist on the
///             section's <c>DataType</c>. Generator skips that field.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class ObjectUpdateBuilderGenerator : IIncrementalGenerator
{
    private const string SectionAttrFullName = "HermesProxy.World.Objects.Version.Attributes.DescriptorSectionAttribute";
    private const string CreateFieldAttrFullName = "HermesProxy.World.Objects.Version.Attributes.DescriptorCreateFieldAttribute";
    private const string UpdateFieldAttrFullName = "HermesProxy.World.Objects.Version.Attributes.DescriptorUpdateFieldAttribute";
    private const string CreatePlaceholderAttrFullName = "HermesProxy.World.Objects.Version.Attributes.DescriptorCreatePlaceholderAttribute";
    private const string CreateBitsPlaceholderAttrFullName = "HermesProxy.World.Objects.Version.Attributes.DescriptorCreateBitsPlaceholderAttribute";
    private const string CustomFieldAttrFullName = "HermesProxy.World.Objects.Version.Attributes.DescriptorCustomFieldAttribute";
    private const string MaskPreambleAttrFullName = "HermesProxy.World.Objects.Version.Attributes.DescriptorMaskPreambleAttribute";
    private const string UpdateBitsPreambleAttrFullName = "HermesProxy.World.Objects.Version.Attributes.DescriptorUpdateBitsPreambleAttribute";
    private const string MaskMutatorAttrFullName = "HermesProxy.World.Objects.Version.Attributes.DescriptorMaskMutatorAttribute";
    private const string UpdatePostFlushAttrFullName = "HermesProxy.World.Objects.Version.Attributes.DescriptorUpdatePostFlushAttribute";
    private const string DescriptorTypeFullName = "HermesProxy.World.Objects.Version.Attributes.DescriptorType";
    private const string MaskModeFullName = "HermesProxy.World.Objects.Version.Attributes.MaskMode";
    private const string EnumsNamespace = "HermesProxy.World.Enums";
    private const string BuilderNamespacePrefix = "HermesProxy.World.Objects.Version.";
    private const string WorldPacketFullName = "HermesProxy.World.WorldPacket";
    private const string StackBitMaskFullName = "Framework.Util.StackBitMask";

    private static readonly DiagnosticDescriptor HPSG003_UnknownSourceProperty = new(
        id: "HPSG003",
        title: "Descriptor source property not found",
        messageFormat: "Descriptor field references property '{0}' on '{1}' but that property does not exist; field will be skipped",
        category: "HermesProxy.SourceGen",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var model = context.CompilationProvider.Select(BuildModel);

        context.RegisterSourceOutput(model, static (ctx, m) =>
        {
            if (m is null)
                return;

            foreach (var diag in m.Diagnostics)
                ctx.ReportDiagnostic(diag);

            foreach (var version in m.Versions)
                ctx.AddSource(version.VersionName + ".ObjectUpdateBuilder.g.cs", Emit(version));
        });
    }

    private static GeneratorModel? BuildModel(Compilation compilation, System.Threading.CancellationToken ct)
    {
        var sectionAttr = compilation.GetTypeByMetadataName(SectionAttrFullName);
        var createAttr = compilation.GetTypeByMetadataName(CreateFieldAttrFullName);
        var updateAttr = compilation.GetTypeByMetadataName(UpdateFieldAttrFullName);
        var placeholderAttr = compilation.GetTypeByMetadataName(CreatePlaceholderAttrFullName);
        var bitsPlaceholderAttr = compilation.GetTypeByMetadataName(CreateBitsPlaceholderAttrFullName);
        var customFieldAttr = compilation.GetTypeByMetadataName(CustomFieldAttrFullName);
        var maskPreambleAttr = compilation.GetTypeByMetadataName(MaskPreambleAttrFullName);
        var updateBitsPreambleAttr = compilation.GetTypeByMetadataName(UpdateBitsPreambleAttrFullName);
        var maskMutatorAttr = compilation.GetTypeByMetadataName(MaskMutatorAttrFullName);
        var updatePostFlushAttr = compilation.GetTypeByMetadataName(UpdatePostFlushAttrFullName);
        var descriptorTypeEnum = compilation.GetTypeByMetadataName(DescriptorTypeFullName);
        if (sectionAttr is null || createAttr is null || descriptorTypeEnum is null)
            return null;

        var enumsNs = ResolveNamespace(compilation.GlobalNamespace, EnumsNamespace);
        if (enumsNs is null)
            return null;

        var model = new GeneratorModel();

        foreach (var child in enumsNs.GetNamespaceMembers())
        {
            // Per-version namespaces start with V + digit, e.g. V3_4_3_54261.
            if (child.Name.Length < 2 || child.Name[0] != 'V' || !char.IsDigit(child.Name[1]))
                continue;

            var sections = new List<SectionEntry>();
            foreach (var type in child.GetTypeMembers())
            {
                if (type.TypeKind != TypeKind.Enum)
                    continue;
                var sectionAttrData = type.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, sectionAttr));
                if (sectionAttrData is null)
                    continue;

                var section = BuildSection(type, sectionAttrData, createAttr, updateAttr, placeholderAttr, bitsPlaceholderAttr, customFieldAttr, maskPreambleAttr, updateBitsPreambleAttr, maskMutatorAttr, updatePostFlushAttr, model);
                if (section is not null)
                    sections.Add(section);
            }

            if (sections.Count == 0)
                continue;

            model.Versions.Add(new VersionEntry(child.Name, sections));
        }

        return model.Versions.Count == 0 && model.Diagnostics.Count == 0 ? null : model;
    }

    private static SectionEntry? BuildSection(
        INamedTypeSymbol enumType,
        AttributeData sectionAttrData,
        INamedTypeSymbol createAttr,
        INamedTypeSymbol? updateAttr,
        INamedTypeSymbol? placeholderAttr,
        INamedTypeSymbol? bitsPlaceholderAttr,
        INamedTypeSymbol? customFieldAttr,
        INamedTypeSymbol? maskPreambleAttr,
        INamedTypeSymbol? updateBitsPreambleAttr,
        INamedTypeSymbol? maskMutatorAttr,
        INamedTypeSymbol? updatePostFlushAttr,
        GeneratorModel model)
    {
        // Read [DescriptorSection] named args
        string? sectionName = null;
        INamedTypeSymbol? dataType = null;
        int maskMode = 0;       // 0 = Blocks, 1 = Flat
        int maskWidth = 0;
        bool cascade = false;
        int blockMaskShape = 0; // 0 = Bits, 1 = UInt32PlusBits16
        foreach (var named in sectionAttrData.NamedArguments)
        {
            switch (named.Key)
            {
                case "SectionName":     sectionName = named.Value.Value as string; break;
                case "DataType":        dataType = named.Value.Value as INamedTypeSymbol; break;
                case "MaskMode":        maskMode = (named.Value.Value as int?) ?? 0; break;
                case "MaskWidth":       maskWidth = (named.Value.Value as int?) ?? 0; break;
                case "Cascade":         cascade = (named.Value.Value as bool?) ?? false; break;
                case "BlockMaskShape":  blockMaskShape = (named.Value.Value as int?) ?? 0; break;
            }
        }
        if (dataType is null)
            return null;
        // SectionName is optional: derive from DataType.Name by stripping trailing "Data"
        // when not explicitly specified. All 7 V3_4_3 sections follow XxxField+XxxData.
        if (string.IsNullOrEmpty(sectionName))
        {
            var typeName = dataType.Name;
            sectionName = typeName.EndsWith("Data", System.StringComparison.Ordinal)
                ? typeName.Substring(0, typeName.Length - 4)
                : typeName;
        }

        var members = new List<MemberEntry>();
        foreach (var member in enumType.GetMembers().OfType<IFieldSymbol>())
        {
            if (!member.IsConst)
                continue;

            foreach (var attrData in member.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attrData.AttributeClass, createAttr))
                {
                    var ce = ReadCreateField(attrData, member, dataType, model);
                    if (ce is not null) members.Add(MemberEntry.FromCreate(member.Name, ce));
                }
                else if (updateAttr is not null
                      && SymbolEqualityComparer.Default.Equals(attrData.AttributeClass, updateAttr))
                {
                    var ue = ReadUpdateField(attrData, member, dataType, model);
                    if (ue is not null) members.Add(MemberEntry.FromUpdate(member.Name, ue));
                }
                else if (placeholderAttr is not null
                      && SymbolEqualityComparer.Default.Equals(attrData.AttributeClass, placeholderAttr))
                {
                    var pe = ReadCreatePlaceholder(attrData);
                    if (pe is not null) members.Add(MemberEntry.FromPlaceholder(member.Name, pe));
                }
                else if (bitsPlaceholderAttr is not null
                      && SymbolEqualityComparer.Default.Equals(attrData.AttributeClass, bitsPlaceholderAttr))
                {
                    var be = ReadCreateBitsPlaceholder(attrData);
                    if (be is not null) members.Add(MemberEntry.FromBitsPlaceholder(member.Name, be));
                }
                else if (customFieldAttr is not null
                      && SymbolEqualityComparer.Default.Equals(attrData.AttributeClass, customFieldAttr))
                {
                    var cf = ReadCustomField(attrData);
                    if (cf is not null) members.Add(MemberEntry.FromCustom(member.Name, cf));
                }
                else if (maskPreambleAttr is not null
                      && SymbolEqualityComparer.Default.Equals(attrData.AttributeClass, maskPreambleAttr))
                {
                    var mp = ReadMaskPreamble(attrData);
                    if (mp is not null) members.Add(MemberEntry.FromMaskPreamble(member.Name, mp));
                }
                else if (updateBitsPreambleAttr is not null
                      && SymbolEqualityComparer.Default.Equals(attrData.AttributeClass, updateBitsPreambleAttr))
                {
                    var ubp = ReadUpdateBitsPreamble(attrData);
                    if (ubp is not null) members.Add(MemberEntry.FromUpdateBitsPreamble(member.Name, ubp));
                }
                else if (maskMutatorAttr is not null
                      && SymbolEqualityComparer.Default.Equals(attrData.AttributeClass, maskMutatorAttr))
                {
                    var mm = ReadMaskMutator(attrData);
                    if (mm is not null) members.Add(MemberEntry.FromMaskMutator(member.Name, mm));
                }
                else if (updatePostFlushAttr is not null
                      && SymbolEqualityComparer.Default.Equals(attrData.AttributeClass, updatePostFlushAttr))
                {
                    var pf = ReadUpdatePostFlush(attrData);
                    if (pf is not null) members.Add(MemberEntry.FromUpdatePostFlush(member.Name, pf));
                }
            }
        }

        var updateFields = members.Where(m => m.Update is not null).Select(m => m.Update!).ToList();
        var customFields = members.Where(m => m.Custom is not null).Select(m => m.Custom!).ToList();
        var maskPreambles = members.Where(m => m.MaskPreamble is not null).Select(m => m.MaskPreamble!).ToList();
        var updateBitsPreambles = members.Where(m => m.UpdateBitsPreamble is not null).Select(m => m.UpdateBitsPreamble!).ToList();
        var maskMutators = members.Where(m => m.MaskMutator is not null).Select(m => m.MaskMutator!).ToList();
        var postFlushes = members.Where(m => m.UpdatePostFlush is not null).Select(m => m.UpdatePostFlush!).ToList();

        // Create-path emits both real fields and placeholders in *enum declaration order*
        // so the wire byte sequence matches the legacy descriptor-tree slot layout. Build
        // a single ordered list of (kind, payload) entries.
        var createSequence = members
            .Where(m => m.Create is not null || m.Placeholder is not null || m.BitsPlaceholder is not null)
            .Select(m =>
            {
                if (m.Create is not null) return (CreateSequenceKind.Field, (object)m.Create);
                if (m.Placeholder is not null) return (CreateSequenceKind.Placeholder, (object)m.Placeholder);
                return (CreateSequenceKind.BitsPlaceholder, (object)m.BitsPlaceholder!);
            })
            .ToList();

        if (createSequence.Count == 0 && updateFields.Count == 0 && customFields.Count == 0)
            return null;

        // BlockCount inference for Blocks-mode. Highest bit any field touches:
        //   scalar field: f.Bit
        //   PerElement array: f.Bit + ArrayCount - 1
        //   Grouped array: f.Bit (one bit covers all)
        //   custom field: f.Bit
        // Plus include each field's ParentBit (the parent bit may live in a higher block).
        int maxBit = 0;
        foreach (var f in updateFields)
        {
            int hi = f.Bit;
            if (f.ArrayCount > 0 && f.ArrayMode == ArrayMode.PerElement)
                hi = f.Bit + f.ArrayCount - 1;
            if (hi > maxBit) maxBit = hi;
            if (f.ParentBit >= 0 && f.ParentBit > maxBit) maxBit = f.ParentBit;
        }
        foreach (var cf in customFields)
        {
            if (cf.Bit > maxBit) maxBit = cf.Bit;
            if (cf.ParentBit >= 0 && cf.ParentBit > maxBit) maxBit = cf.ParentBit;
        }
        int blockCount = (maxBit / 32) + 1;

        return new SectionEntry(
            sectionName!,
            dataType,
            (MaskMode)maskMode,
            maskWidth,
            blockCount,
            cascade,
            (BlockMaskShape)blockMaskShape,
            createSequence,
            updateFields,
            customFields,
            maskPreambles,
            maskMutators,
            postFlushes,
            updateBitsPreambles);
    }

    private static UpdateBitsPreambleEntry? ReadUpdateBitsPreamble(AttributeData attrData)
    {
        if (attrData.ConstructorArguments.Length < 2)
            return null;
        var value = attrData.ConstructorArguments[0].Value as uint?;
        var bitCount = attrData.ConstructorArguments[1].Value as int?;
        if (value is null || bitCount is null)
            return null;
        return new UpdateBitsPreambleEntry(value.Value, bitCount.Value);
    }

    private static MaskMutatorEntry? ReadMaskMutator(AttributeData attrData)
    {
        if (attrData.ConstructorArguments.Length < 1)
            return null;
        var customWriter = attrData.ConstructorArguments[0].Value as string;
        if (string.IsNullOrEmpty(customWriter))
            return null;
        string? hasAnyPredicate = null;
        foreach (var named in attrData.NamedArguments)
        {
            switch (named.Key)
            {
                case "HasAnyPredicate": hasAnyPredicate = named.Value.Value as string; break;
            }
        }
        return new MaskMutatorEntry(customWriter!, hasAnyPredicate);
    }

    private static UpdatePostFlushEntry? ReadUpdatePostFlush(AttributeData attrData)
    {
        if (attrData.ConstructorArguments.Length < 1)
            return null;
        var afterBit = attrData.ConstructorArguments[0].Value as int?;
        if (afterBit is null)
            return null;
        return new UpdatePostFlushEntry(afterBit.Value);
    }

    private static MaskPreambleEntry? ReadMaskPreamble(AttributeData attrData)
    {
        if (attrData.ConstructorArguments.Length < 2)
            return null;
        var bit = attrData.ConstructorArguments[0].Value as int?;
        var customWriter = attrData.ConstructorArguments[1].Value as string;
        if (bit is null || string.IsNullOrEmpty(customWriter))
            return null;
        return new MaskPreambleEntry(bit.Value, customWriter!);
    }

    private static CustomFieldEntry? ReadCustomField(AttributeData attrData)
    {
        if (attrData.ConstructorArguments.Length < 3)
            return null;
        var label = attrData.ConstructorArguments[0].Value as string;
        var bit = attrData.ConstructorArguments[1].Value as int?;
        var customWriter = attrData.ConstructorArguments[2].Value as string;
        if (string.IsNullOrEmpty(label) || bit is null || string.IsNullOrEmpty(customWriter))
            return null;
        int parentBit = -1;
        bool writeOnly = false;
        foreach (var named in attrData.NamedArguments)
        {
            switch (named.Key)
            {
                case "ParentBit": parentBit = (named.Value.Value as int?) ?? -1; break;
                case "WriteOnly": writeOnly = (named.Value.Value as bool?) ?? false; break;
            }
        }
        return new CustomFieldEntry(label!, bit.Value, customWriter!, parentBit, writeOnly);
    }

    private static CreateFieldEntry? ReadCreateField(AttributeData attrData, IFieldSymbol member, INamedTypeSymbol dataType, GeneratorModel model)
    {
        if (attrData.ConstructorArguments.Length < 2)
            return null;

        var sourceProperty = attrData.ConstructorArguments[0].Value as string;
        if (string.IsNullOrEmpty(sourceProperty))
            return null;

        var typeOrdinal = attrData.ConstructorArguments[1].Value as int?;
        if (typeOrdinal is null)
            return null;

        string? defaultExpression = null;
        string? defaultExpressionByIndex = null;
        int arrayCount = 0;
        int arrayMode = 0;
        bool ownerOnly = false;
        string? customWriter = null;
        string? cast = null;
        foreach (var named in attrData.NamedArguments)
        {
            switch (named.Key)
            {
                case "DefaultExpression": defaultExpression = named.Value.Value as string; break;
                case "DefaultExpressionByIndex": defaultExpressionByIndex = named.Value.Value as string; break;
                case "ArrayCount": arrayCount = (named.Value.Value as int?) ?? 0; break;
                case "ArrayMode": arrayMode = (named.Value.Value as int?) ?? 0; break;
                case "OwnerOnly": ownerOnly = (named.Value.Value as bool?) ?? false; break;
                case "CustomWriter": customWriter = named.Value.Value as string; break;
                case "Cast": cast = named.Value.Value as string; break;
            }
        }

        if (dataType.GetMembers(sourceProperty!).IsDefaultOrEmpty)
        {
            model.Diagnostics.Add(Diagnostic.Create(
                HPSG003_UnknownSourceProperty,
                member.Locations.FirstOrDefault() ?? Location.None,
                sourceProperty,
                dataType.ToDisplayString()));
            return null;
        }

        return new CreateFieldEntry(
            sourceProperty: sourceProperty!,
            type: (DescriptorType)typeOrdinal.Value,
            defaultExpression: defaultExpression,
            arrayCount: arrayCount,
            arrayMode: (ArrayMode)arrayMode,
            defaultExpressionByIndex: defaultExpressionByIndex,
            ownerOnly: ownerOnly,
            customWriter: customWriter,
            cast: cast);
    }

    private static UpdateFieldEntry? ReadUpdateField(AttributeData attrData, IFieldSymbol member, INamedTypeSymbol dataType, GeneratorModel model)
    {
        if (attrData.ConstructorArguments.Length < 3)
            return null;

        var sourceProperty = attrData.ConstructorArguments[0].Value as string;
        if (string.IsNullOrEmpty(sourceProperty))
            return null;

        var typeOrdinal = attrData.ConstructorArguments[1].Value as int?;
        if (typeOrdinal is null)
            return null;

        var bit = attrData.ConstructorArguments[2].Value as int?;
        if (bit is null)
            return null;

        int arrayCount = 0;
        int arrayMode = 0;
        int parentBit = -1;
        string? defaultExpressionByIndex = null;
        string? customWriter = null;
        string? cast = null;
        string? customPredicate = null;
        bool maskOnly = false;
        int writeOrder = 0;
        foreach (var named in attrData.NamedArguments)
        {
            switch (named.Key)
            {
                case "ArrayCount": arrayCount = (named.Value.Value as int?) ?? 0; break;
                case "ArrayMode": arrayMode = (named.Value.Value as int?) ?? 0; break;
                case "ParentBit": parentBit = (named.Value.Value as int?) ?? -1; break;
                case "DefaultExpressionByIndex": defaultExpressionByIndex = named.Value.Value as string; break;
                case "CustomWriter": customWriter = named.Value.Value as string; break;
                case "Cast": cast = named.Value.Value as string; break;
                case "CustomPredicate": customPredicate = named.Value.Value as string; break;
                case "MaskOnly": maskOnly = (named.Value.Value as bool?) ?? false; break;
                case "WriteOrder": writeOrder = (named.Value.Value as int?) ?? 0; break;
            }
        }

        if (dataType.GetMembers(sourceProperty!).IsDefaultOrEmpty)
        {
            model.Diagnostics.Add(Diagnostic.Create(
                HPSG003_UnknownSourceProperty,
                member.Locations.FirstOrDefault() ?? Location.None,
                sourceProperty,
                dataType.ToDisplayString()));
            return null;
        }

        return new UpdateFieldEntry(
            sourceProperty: sourceProperty!,
            type: (DescriptorType)typeOrdinal.Value,
            bit: bit.Value,
            arrayCount: arrayCount,
            arrayMode: (ArrayMode)arrayMode,
            parentBit: parentBit,
            defaultExpressionByIndex: defaultExpressionByIndex,
            customWriter: customWriter,
            cast: cast,
            customPredicate: customPredicate,
            maskOnly: maskOnly,
            writeOrder: writeOrder);
    }

    private static CreatePlaceholderEntry? ReadCreatePlaceholder(AttributeData attrData)
    {
        if (attrData.ConstructorArguments.Length < 1)
            return null;

        var typeOrdinal = attrData.ConstructorArguments[0].Value as int?;
        if (typeOrdinal is null)
            return null;

        // Type-only overload: literal omitted → emit time falls back to ZeroLiteralFor(type).
        string literal = attrData.ConstructorArguments.Length >= 2
            ? (attrData.ConstructorArguments[1].Value as string) ?? ""
            : "";

        bool ownerOnly = false;
        string? customWriter = null;
        foreach (var named in attrData.NamedArguments)
        {
            switch (named.Key)
            {
                case "OwnerOnly": ownerOnly = (named.Value.Value as bool?) ?? false; break;
                case "CustomWriter": customWriter = named.Value.Value as string; break;
            }
        }

        return new CreatePlaceholderEntry((DescriptorType)typeOrdinal.Value, literal, ownerOnly, customWriter);
    }

    private static CreateBitsPlaceholderEntry? ReadCreateBitsPlaceholder(AttributeData attrData)
    {
        if (attrData.ConstructorArguments.Length < 2)
            return null;

        var value = attrData.ConstructorArguments[0].Value as uint?;
        if (value is null)
            return null;

        var bitCount = attrData.ConstructorArguments[1].Value as int?;
        if (bitCount is null)
            return null;

        bool flush = true;
        bool ownerOnly = false;
        foreach (var named in attrData.NamedArguments)
        {
            switch (named.Key)
            {
                case "Flush": flush = (named.Value.Value as bool?) ?? true; break;
                case "OwnerOnly": ownerOnly = (named.Value.Value as bool?) ?? false; break;
            }
        }

        return new CreateBitsPlaceholderEntry(value.Value, bitCount.Value, flush, ownerOnly);
    }

    private static string Emit(VersionEntry version)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.Append("namespace ").Append(BuilderNamespacePrefix).Append(version.VersionName).AppendLine(";");
        sb.AppendLine();
        sb.AppendLine("public partial class ObjectUpdateBuilder");
        sb.AppendLine("{");

        bool first = true;
        foreach (var section in version.Sections)
        {
            if (!first) sb.AppendLine();
            first = false;
            EmitSection(sb, section);
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static void EmitSection(StringBuilder sb, SectionEntry section)
    {
        bool hasCreate = section.CreateSequence.Count > 0;
        bool hasUpdate = section.UpdateFields.Count > 0;

        if (hasCreate)
        {
            EmitCreate(sb, section);
        }

        if (hasUpdate)
        {
            if (hasCreate) sb.AppendLine();
            EmitUpdate(sb, section);
            sb.AppendLine();
            EmitHasAny(sb, section);
        }
    }

    private static string DataAccessor(SectionEntry section) =>
        // Convention: data class lives at HermesProxy.World.Objects.{TypeName}; the matching
        // field on ObjectUpdate is the type name unchanged (e.g. GameObjectData).
        "_updateData." + section.DataType.Name;

    private static void EmitCreate(StringBuilder sb, SectionEntry section)
    {
        sb.Append("    internal void WriteCreate").Append(section.SectionName).Append("Data(")
          .Append(WorldPacketFullName).AppendLine(" data)");
        sb.AppendLine("    {");
        sb.Append("        var src = ").Append(DataAccessor(section)).Append(" ?? new ")
          .Append(section.DataType.ToDisplayString()).AppendLine("();");
        foreach (var (kind, payload) in section.CreateSequence)
        {
            if (kind == CreateSequenceKind.Field)
            {
                var f = (CreateFieldEntry)payload;
                EmitCreateField(sb, f);
            }
            else if (kind == CreateSequenceKind.Placeholder)
            {
                var p = (CreatePlaceholderEntry)payload;
                if (p.OwnerOnly) sb.AppendLine("        if (IsOwner) {");
                string indent = p.OwnerOnly ? "            " : "        ";
                if (!string.IsNullOrEmpty(p.CustomWriter))
                {
                    sb.Append(indent).Append(p.CustomWriter).AppendLine("(data, src);");
                }
                else
                {
                    string literal = string.IsNullOrEmpty(p.LiteralExpression)
                        ? ZeroLiteralFor(p.Type)
                        : p.LiteralExpression;
                    sb.Append(indent).Append("data.").Append(WriteMethodNameFor(p.Type)).Append("(")
                      .Append(literal).AppendLine(");");
                }
                if (p.OwnerOnly) sb.AppendLine("        }");
            }
            else // BitsPlaceholder
            {
                var b = (CreateBitsPlaceholderEntry)payload;
                if (b.OwnerOnly) sb.AppendLine("        if (IsOwner) {");
                string indent = b.OwnerOnly ? "            " : "        ";
                sb.Append(indent).Append("data.WriteBits(").Append(b.Value).Append("u, ").Append(b.BitCount).AppendLine(");");
                if (b.Flush) sb.Append(indent).AppendLine("data.FlushBits();");
                if (b.OwnerOnly) sb.AppendLine("        }");
            }
        }
        sb.AppendLine("    }");
    }

    private static void EmitCreateField(StringBuilder sb, CreateFieldEntry f)
    {
        string baseIndent = f.OwnerOnly ? "            " : "        ";
        if (f.OwnerOnly) sb.AppendLine("        if (IsOwner) {");

        // Scalar field-level CustomWriter: delegate to a builder method, no inline write.
        // Array CustomWriter routes via the per-element loop branch below.
        if (f.ArrayCount == 0 && !string.IsNullOrEmpty(f.CustomWriter))
        {
            sb.Append(baseIndent).Append(f.CustomWriter).AppendLine("(data, src);");
            if (f.OwnerOnly) sb.AppendLine("        }");
            return;
        }

        if (f.ArrayCount > 0)
        {
            // CustomWriter array branch: delegate per-element to a hand-rolled writer on
            // ObjectUpdateBuilder (e.g. WriteEnchantmentCreate). The writer handles its
            // own null check + zero fallback.
            if (!string.IsNullOrEmpty(f.CustomWriter))
            {
                for (int i = 0; i < f.ArrayCount; i++)
                {
                    sb.Append(baseIndent).Append(f.CustomWriter).Append("(data, src.")
                      .Append(f.SourceProperty).Append(", ").Append(i).AppendLine(");");
                }
            }
            else
            {
                // Array create-path: per-index write with per-index defaults. Shape is the
                // same for Grouped and PerElement on the Create path — both write all
                // ArrayCount slots in order.
                var defaults = (f.DefaultExpressionByIndex ?? f.DefaultExpression ?? "")
                               .Split(',').Select(s => s.Trim()).ToArray();
                for (int i = 0; i < f.ArrayCount; i++)
                {
                    string fallback = i < defaults.Length && !string.IsNullOrEmpty(defaults[i])
                        ? defaults[i]
                        : ZeroLiteralFor(f.Type);
                    string castPrefix = f.Cast ?? "";
                    if (f.Type == DescriptorType.PackedGuid128)
                    {
                        sb.Append(baseIndent).Append("data.WritePackedGuid128(src.").Append(f.SourceProperty)
                          .Append(" != null && src.").Append(f.SourceProperty).Append("[").Append(i)
                          .Append("] != null ? src.").Append(f.SourceProperty).Append("[").Append(i)
                          .Append("]!.Value : ").Append(fallback).AppendLine(");");
                    }
                    else if (string.IsNullOrEmpty(f.Cast))
                    {
                        sb.Append(baseIndent).Append("data.").Append(WriteMethodNameFor(f.Type))
                          .Append("(src.").Append(f.SourceProperty)
                          .Append(" != null && src.").Append(f.SourceProperty).Append("[").Append(i)
                          .Append("].HasValue ? src.").Append(f.SourceProperty).Append("[").Append(i)
                          .Append("]!.Value : ").Append(fallback).AppendLine(");");
                    }
                    else
                    {
                        // Cast set: paren-wrap the ternary so the cast binds correctly.
                        sb.Append(baseIndent).Append("data.").Append(WriteMethodNameFor(f.Type))
                          .Append("(").Append(f.Cast).Append("(src.").Append(f.SourceProperty)
                          .Append(" != null && src.").Append(f.SourceProperty).Append("[").Append(i)
                          .Append("].HasValue ? src.").Append(f.SourceProperty).Append("[").Append(i)
                          .Append("]!.Value : ").Append(fallback).AppendLine("));");
                    }
                }
            }
        }
        else if (f.Type == DescriptorType.PackedGuid128)
        {
            string fb = f.DefaultExpression ?? "HermesProxy.World.WowGuid128.Empty";
            sb.Append(baseIndent).Append("data.WritePackedGuid128(src.").Append(f.SourceProperty)
              .Append(" ?? ").Append(fb).AppendLine(");");
        }
        else
        {
            // When a Cast prefix is set, paren-wrap the default-fallback expression so
            // the cast binds correctly: `(byte)(src.X ?? def)`. With no cast, keep the
            // historical raw shape `src.X ?? def` (no extra parens) so existing
            // snapshots stay byte-identical.
            string valueExpr = f.DefaultExpression is null
                ? string.IsNullOrEmpty(f.Cast)
                    ? $"src.{f.SourceProperty}.GetValueOrDefault()"
                    : $"{f.Cast}src.{f.SourceProperty}.GetValueOrDefault()"
                : string.IsNullOrEmpty(f.Cast)
                    ? $"src.{f.SourceProperty} ?? {f.DefaultExpression}"
                    : $"{f.Cast}(src.{f.SourceProperty} ?? {f.DefaultExpression})";

            sb.Append(baseIndent).Append("data.").Append(WriteMethodNameFor(f.Type)).Append("(")
              .Append(valueExpr).AppendLine(");");
        }

        if (f.OwnerOnly) sb.AppendLine("        }");
    }

    private static void EmitUpdate(StringBuilder sb, SectionEntry section)
    {
        sb.Append("    internal void WriteUpdate").Append(section.SectionName).Append("Data(")
          .Append(WorldPacketFullName).AppendLine(" data)");
        sb.AppendLine("    {");
        sb.Append("        var src = ").Append(DataAccessor(section)).Append(" ?? new ")
          .Append(section.DataType.ToDisplayString()).AppendLine("();");

        if (section.MaskMode == MaskMode.Flat)
        {
            EmitUpdateFlat(sb, section);
        }
        else
        {
            EmitUpdateBlocks(sb, section);
        }

        sb.AppendLine("    }");
    }

    // -------------------- Flat-mode Update emit --------------------

    private static void EmitUpdateFlat(StringBuilder sb, SectionEntry section)
    {
        // Flat: single 'uint mask' covers all bits. Group gate is bit 0; the generator
        // sets it automatically if any child bit fires.
        sb.AppendLine("        uint mask = 0u;");
        foreach (var f in section.UpdateFields)
        {
            EmitFlatMaskBits(sb, f);
        }
        sb.AppendLine("        if (mask != 0) mask |= 1u;");
        sb.Append("        data.WriteBits(mask, ").Append(section.MaskWidth).AppendLine(");");
        sb.AppendLine("        data.FlushBits();");
        sb.AppendLine("        if ((mask & 1) == 0) return;");
        // Wire layout is bit-ascending — sort writes by Bit. (Object + GameObject enums
        // already declare in bit order, but Container declares Slots before NumSlots so
        // generator-emit must reorder regardless.)
        foreach (var f in section.UpdateFields.OrderBy(x => x.Bit))
        {
            EmitFlatUpdateWrite(sb, f);
        }
    }

    private static string ArrayElementPresence(UpdateFieldEntry f, int i)
    {
        // CustomPredicate wins — author-specified expression with {i} substitution.
        if (!string.IsNullOrEmpty(f.CustomPredicate))
            return f.CustomPredicate!.Replace("{i}", i.ToString());
        // CustomWriter implies a class-array (e.g. ItemEnchantment?[]) — predicate is != null.
        if (!string.IsNullOrEmpty(f.CustomWriter))
            return $"src.{f.SourceProperty}[{i}] != null";
        return f.Type == DescriptorType.PackedGuid128
            ? $"src.{f.SourceProperty}[{i}] != null"
            : $"src.{f.SourceProperty}[{i}].HasValue";
    }

    private static string ScalarPresence(UpdateFieldEntry f)
    {
        if (!string.IsNullOrEmpty(f.CustomPredicate))
            return f.CustomPredicate!;
        return f.Type == DescriptorType.PackedGuid128
            ? $"src.{f.SourceProperty} != null"
            : $"src.{f.SourceProperty}.HasValue";
    }

    private static void EmitFlatMaskBits(StringBuilder sb, UpdateFieldEntry f)
    {
        // Flat mode doesn't carry per-field ParentBit (only Blocks mode multi-bit gating
        // applies). If a Flat-mode descriptor sets ParentBit it's ignored here — the group
        // gate is always bit 0, set unconditionally above.
        if (f.ArrayCount > 0 && f.ArrayMode == ArrayMode.PerElement)
        {
            // Per-element bits at Bit..Bit+ArrayCount-1.
            for (int i = 0; i < f.ArrayCount; i++)
            {
                sb.Append("        if (src.").Append(f.SourceProperty)
                  .Append(" != null && ").Append(ArrayElementPresence(f, i))
                  .Append(") mask |= (1u << ").Append(f.Bit + i).AppendLine(");");
            }
            return;
        }

        if (f.ArrayCount > 0)
        {
            // Grouped: single bit covers all.
            sb.Append("        if (src.").Append(f.SourceProperty)
              .Append(" != null && (");
            for (int i = 0; i < f.ArrayCount; i++)
            {
                if (i > 0) sb.Append(" || ");
                string elemPresence = f.Type == DescriptorType.PackedGuid128
                    ? $"src.{f.SourceProperty}[{i}] != null"
                    : $"src.{f.SourceProperty}[{i}].HasValue";
                sb.Append(elemPresence);
            }
            sb.Append(")) mask |= (1u << ").Append(f.Bit).AppendLine(");");
            return;
        }

        sb.Append("        if (").Append(ScalarPresence(f)).Append(") mask |= (1u << ").Append(f.Bit).AppendLine(");");
    }

    private static void EmitFlatUpdateWrite(StringBuilder sb, UpdateFieldEntry f)
    {
        if (f.ArrayCount > 0 && f.ArrayMode == ArrayMode.PerElement)
        {
            // PerElement: write each element gated on its own bit.
            string castPrefix = f.Cast ?? "";
            for (int i = 0; i < f.ArrayCount; i++)
            {
                if (!string.IsNullOrEmpty(f.CustomWriter))
                {
                    sb.Append("        if ((mask & (1u << ").Append(f.Bit + i).Append(")) != 0) ")
                      .Append(f.CustomWriter).Append("(data, src.").Append(f.SourceProperty)
                      .Append("!, ").Append(i).AppendLine(");");
                }
                else if (f.Type == DescriptorType.PackedGuid128)
                {
                    sb.Append("        if ((mask & (1u << ").Append(f.Bit + i).Append(")) != 0) data.WritePackedGuid128(src.")
                      .Append(f.SourceProperty).Append("![").Append(i).AppendLine("]!.Value);");
                }
                else
                {
                    sb.Append("        if ((mask & (1u << ").Append(f.Bit + i).Append(")) != 0) data.")
                      .Append(WriteMethodNameFor(f.Type))
                      .Append("(").Append(castPrefix).Append("src.").Append(f.SourceProperty).Append("![").Append(i).AppendLine("]!.Value);");
                }
            }
            return;
        }

        if (f.ArrayCount > 0)
        {
            var defaults = (f.DefaultExpressionByIndex ?? "")
                           .Split(',').Select(s => s.Trim()).ToArray();
            sb.Append("        if ((mask & (1u << ").Append(f.Bit).AppendLine(")) != 0)");
            sb.AppendLine("        {");
            for (int i = 0; i < f.ArrayCount; i++)
            {
                string fallback = i < defaults.Length && !string.IsNullOrEmpty(defaults[i])
                    ? defaults[i]
                    : ZeroLiteralFor(f.Type);
                if (f.Type == DescriptorType.PackedGuid128)
                {
                    sb.Append("            data.WritePackedGuid128(src.").Append(f.SourceProperty)
                      .Append("![").Append(i).Append("] ?? ").Append(fallback).AppendLine(");");
                }
                else
                {
                    sb.Append("            data.").Append(WriteMethodNameFor(f.Type))
                      .Append("(src.").Append(f.SourceProperty).Append("![").Append(i)
                      .Append("] ?? ").Append(fallback).AppendLine(");");
                }
            }
            sb.AppendLine("        }");
            return;
        }

        // Scalar field-level CustomWriter: delegate inline write to a builder method.
        if (!string.IsNullOrEmpty(f.CustomWriter))
        {
            sb.Append("        if ((mask & (1u << ").Append(f.Bit).Append(")) != 0) ")
              .Append(f.CustomWriter).AppendLine("(data, src);");
            return;
        }

        if (f.Type == DescriptorType.PackedGuid128)
        {
            sb.Append("        if (src.").Append(f.SourceProperty).Append(" != null) data.WritePackedGuid128(src.")
              .Append(f.SourceProperty).AppendLine(".Value);");
            return;
        }

        string scalarCast = f.Cast ?? "";
        sb.Append("        if (src.").Append(f.SourceProperty).Append(".HasValue) data.")
          .Append(WriteMethodNameFor(f.Type)).Append("(").Append(scalarCast).Append("src.").Append(f.SourceProperty).AppendLine(".Value);");
    }

    // -------------------- Blocks-mode Update emit --------------------

    private static void EmitUpdateBlocks(StringBuilder sb, SectionEntry section)
    {
        int blockCount = section.BlockCount;
        sb.Append("        System.Span<uint> blocksBuf = stackalloc uint[").Append(blockCount).AppendLine("];");
        sb.Append("        var blocks = new ").Append(StackBitMaskFullName).AppendLine("(blocksBuf);");

        // MaskMutators run before any field's bit-setting. They capture-and-consume
        // session state (e.g. ActiveGlyphsDirty) and fan bits across multiple positions
        // in a single side-effecting call. Method signature on builder:
        // void Method(ref StackBitMask blocks, {DataType} src).
        foreach (var mm in section.MaskMutators)
        {
            sb.Append("        ").Append(mm.CustomWriter).AppendLine("(ref blocks, src);");
        }

        // Pass 1: set bits. Mask-build can run in declaration order — order doesn't
        // affect wire bytes (only the final mask state matters).
        foreach (var f in section.UpdateFields)
        {
            EmitBlocksMaskBits(sb, f);
        }
        // Custom-field group writers add their parent + own bit during Pass-1 so the
        // blocks-mask prefix accounts for the group's presence. Custom writers
        // themselves are presence-driven by the section's `IsAny*` predicate — but the
        // generator can't know what the custom writer wants to write, so we conservatively
        // ALWAYS set the bit when the section reaches this point (matches hand-port
        // behaviour for static groups like Power/Stats that always emit when the parent
        // field group has any presence). For surgical control, custom writers can guard
        // their own work and skip writes — the bit-set is harmless. Future refinement
        // could take an explicit Predicate from DescriptorCustomFieldAttribute.
        foreach (var cf in section.CustomFields)
        {
            // WriteOnly custom-fields rely on sibling MaskOnly UpdateField arrays to
            // set the bits — skip the Pass-1 unconditional bit-set emit.
            if (cf.WriteOnly) continue;
            if (cf.ParentBit >= 0)
                sb.Append("        blocks.SetBit(").Append(cf.ParentBit).AppendLine(");");
            sb.Append("        blocks.SetBit(").Append(cf.Bit).AppendLine(");");
        }

        // V3_4_3-only cascade rule: force-set bit 0 of each of the first 4 blocks
        // when that block carries any other set bit. Bits 0/32/64/96 are the group
        // gates the client reads to know whether to consume the block's payload.
        if (section.Cascade)
        {
            sb.AppendLine("        for (int __c = 0; __c < 4 && __c < " + blockCount + "; __c++)");
            sb.AppendLine("            if (blocks[__c] != 0) blocks.SetBit(__c * 32);");
        }

        // Emit blocks-mask prefix + per-block writes. Shape selects between single
        // WriteBits (Item/Container/Unit/Player) and split UInt32+Bits16 (ActivePlayer).
        if (section.BlockMaskShape == BlockMaskShape.UInt32PlusBits16)
        {
            sb.AppendLine("        uint blocksMask0 = 0;");
            sb.Append("        for (int __i = 0; __i < 32 && __i < ").Append(blockCount).AppendLine("; __i++)");
            sb.AppendLine("            if (blocks[__i] != 0) blocksMask0 |= (1u << __i);");
            sb.AppendLine("        uint blocksMask1 = 0;");
            sb.Append("        for (int __i = 32; __i < ").Append(blockCount).AppendLine("; __i++)");
            sb.AppendLine("            if (blocks[__i] != 0) blocksMask1 |= (1u << (__i - 32));");
            sb.AppendLine("        data.WriteUInt32(blocksMask0);");
            sb.AppendLine("        data.WriteBits(blocksMask1, 16);");
            sb.Append("        for (int __i = 0; __i < ").Append(blockCount).AppendLine("; __i++)");
            sb.AppendLine("        {");
            sb.AppendLine("            bool __blockSet = __i < 32 ? (blocksMask0 & (1u << __i)) != 0 : (blocksMask1 & (1u << (__i - 32))) != 0;");
            sb.AppendLine("            if (__blockSet) data.WriteBits(blocks[__i], 32);");
            sb.AppendLine("        }");
        }
        else
        {
            sb.AppendLine("        byte blocksMask = 0;");
            sb.Append("        for (int __i = 0; __i < ").Append(blockCount).AppendLine("; __i++)");
            sb.AppendLine("            if (blocks[__i] != 0) blocksMask |= (byte)(1 << __i);");
            sb.Append("        data.WriteBits(blocksMask, ").Append(section.MaskWidth).AppendLine(");");
            sb.Append("        for (int __i = 0; __i < ").Append(blockCount).AppendLine("; __i++)");
            sb.AppendLine("            if ((blocksMask & (1 << __i)) != 0) data.WriteBits(blocks[__i], 32);");
        }

        // Mask-preamble emit: synthetic custom-field calls between per-block writes and
        // FlushBits. Used for DynamicUpdateField preambles (ChannelObjects: size + bitmask)
        // that must be bit-aligned with the blocks-mask prefix, not byte-aligned with
        // field payload. Sorted by Bit for deterministic emit order.
        foreach (var mp in section.MaskPreambles.OrderBy(x => x.Bit))
        {
            sb.Append("        if (blocks.IsBitSet(").Append(mp.Bit)
              .Append(")) ").Append(mp.CustomWriter).AppendLine("(data, ref blocks, src);");
        }

        // Unconditional Update-side bit preambles. Used for fixed marker bits between the
        // blocks-mask write and FlushBits (e.g. Player's IsQuestLogChangesMaskSkipped).
        foreach (var ubp in section.UpdateBitsPreambles)
        {
            sb.Append("        data.WriteBits(").Append(ubp.Value).Append("u, ").Append(ubp.BitCount).AppendLine(");");
        }

        sb.AppendLine("        data.FlushBits();");
        if (section.BlockMaskShape == BlockMaskShape.UInt32PlusBits16)
            sb.AppendLine("        if (blocksMask0 == 0 && blocksMask1 == 0) return;");
        else
            sb.AppendLine("        if (blocksMask == 0) return;");

        // Pass 2: field payload writes, gated on the actual bit set on `blocks`. The wire
        // layout is *bit-ascending* (TC: writes are in changesMask bit-index order), not
        // enum-declaration order. Merge UpdateFields + CustomFields and sort by Bit.
        var writeOps = section.UpdateFields
            .Select(f => (SortKey: f.WriteOrder != 0 ? f.WriteOrder : f.Bit, Bit: f.Bit, IsCustom: false, FieldEntry: (object)f))
            .Concat(section.CustomFields.Select(cf => (SortKey: cf.Bit, Bit: cf.Bit, IsCustom: true, FieldEntry: (object)cf)))
            .OrderBy(x => x.SortKey)
            .ToList();
        var postFlushAfterBit = new HashSet<int>(section.UpdatePostFlushes.Select(pf => pf.AfterBit));
        foreach (var op in writeOps)
        {
            if (op.IsCustom)
            {
                var cf = (CustomFieldEntry)op.FieldEntry;
                sb.Append("        if (blocks.IsBitSet(").Append(cf.Bit)
                  .Append(")) ").Append(cf.CustomWriter).AppendLine("(data, ref blocks, src);");
            }
            else
            {
                var f = (UpdateFieldEntry)op.FieldEntry;
                if (f.MaskOnly) continue;   // sibling CustomGroupWriter handles the write
                EmitBlocksUpdateWrite(sb, f);
            }

            if (postFlushAfterBit.Contains(op.Bit))
                sb.AppendLine("        data.FlushBits();");
        }
    }

    private static void EmitBlocksMaskBits(StringBuilder sb, UpdateFieldEntry f)
    {
        string parentSetter = f.ParentBit >= 0
            ? $"blocks.SetBit({f.ParentBit}); "
            : "";

        if (f.ArrayCount > 0 && f.ArrayMode == ArrayMode.PerElement)
        {
            for (int i = 0; i < f.ArrayCount; i++)
            {
                sb.Append("        if (src.").Append(f.SourceProperty)
                  .Append(" != null && ").Append(ArrayElementPresence(f, i))
                  .Append(") { ").Append(parentSetter)
                  .Append("blocks.SetBit(").Append(f.Bit + i).AppendLine("); }");
            }
            return;
        }

        if (f.ArrayCount > 0)
        {
            // Grouped: any-element-set sets single bit.
            sb.Append("        if (src.").Append(f.SourceProperty)
              .Append(" != null && (");
            for (int i = 0; i < f.ArrayCount; i++)
            {
                if (i > 0) sb.Append(" || ");
                string elemPresence = f.Type == DescriptorType.PackedGuid128
                    ? $"src.{f.SourceProperty}[{i}] != null"
                    : $"src.{f.SourceProperty}[{i}].HasValue";
                sb.Append(elemPresence);
            }
            sb.Append(")) { ").Append(parentSetter)
              .Append("blocks.SetBit(").Append(f.Bit).AppendLine("); }");
            return;
        }

        sb.Append("        if (").Append(ScalarPresence(f))
          .Append(") { ").Append(parentSetter)
          .Append("blocks.SetBit(").Append(f.Bit).AppendLine("); }");
    }

    private static void EmitBlocksUpdateWrite(StringBuilder sb, UpdateFieldEntry f)
    {
        if (f.ArrayCount > 0 && f.ArrayMode == ArrayMode.PerElement)
        {
            string castPrefix = f.Cast ?? "";
            for (int i = 0; i < f.ArrayCount; i++)
            {
                if (!string.IsNullOrEmpty(f.CustomWriter))
                {
                    sb.Append("        if (blocks.IsBitSet(").Append(f.Bit + i)
                      .Append(")) ").Append(f.CustomWriter).Append("(data, src.").Append(f.SourceProperty)
                      .Append("!, ").Append(i).AppendLine(");");
                }
                else if (f.Type == DescriptorType.PackedGuid128)
                {
                    sb.Append("        if (blocks.IsBitSet(").Append(f.Bit + i)
                      .Append(")) data.WritePackedGuid128(src.").Append(f.SourceProperty)
                      .Append("![").Append(i).AppendLine("]!.Value);");
                }
                else
                {
                    sb.Append("        if (blocks.IsBitSet(").Append(f.Bit + i)
                      .Append(")) data.").Append(WriteMethodNameFor(f.Type))
                      .Append("(").Append(castPrefix).Append("src.").Append(f.SourceProperty).Append("![").Append(i).AppendLine("]!.Value);");
                }
            }
            return;
        }

        if (f.ArrayCount > 0)
        {
            var defaults = (f.DefaultExpressionByIndex ?? "")
                           .Split(',').Select(s => s.Trim()).ToArray();
            sb.Append("        if (blocks.IsBitSet(").Append(f.Bit).AppendLine("))");
            sb.AppendLine("        {");
            for (int i = 0; i < f.ArrayCount; i++)
            {
                string fallback = i < defaults.Length && !string.IsNullOrEmpty(defaults[i])
                    ? defaults[i]
                    : ZeroLiteralFor(f.Type);
                if (f.Type == DescriptorType.PackedGuid128)
                {
                    sb.Append("            data.WritePackedGuid128(src.").Append(f.SourceProperty)
                      .Append("![").Append(i).Append("] ?? ").Append(fallback).AppendLine(");");
                }
                else
                {
                    sb.Append("            data.").Append(WriteMethodNameFor(f.Type))
                      .Append("(src.").Append(f.SourceProperty).Append("![").Append(i)
                      .Append("] ?? ").Append(fallback).AppendLine(");");
                }
            }
            sb.AppendLine("        }");
            return;
        }

        // Scalar field-level CustomWriter: delegate inline write to a builder method.
        if (!string.IsNullOrEmpty(f.CustomWriter))
        {
            sb.Append("        if (blocks.IsBitSet(").Append(f.Bit)
              .Append(")) ").Append(f.CustomWriter).AppendLine("(data, src);");
            return;
        }

        if (f.Type == DescriptorType.PackedGuid128)
        {
            sb.Append("        if (blocks.IsBitSet(").Append(f.Bit)
              .Append(")) data.WritePackedGuid128(src.").Append(f.SourceProperty).AppendLine("!.Value);");
            return;
        }

        string scalarCast = f.Cast ?? "";
        sb.Append("        if (blocks.IsBitSet(").Append(f.Bit)
          .Append(")) data.").Append(WriteMethodNameFor(f.Type))
          .Append("(").Append(scalarCast).Append("src.").Append(f.SourceProperty).AppendLine("!.Value);");
    }

    // -------------------- HasAny --------------------

    private static void EmitHasAny(StringBuilder sb, SectionEntry section)
    {
        sb.Append("    internal bool HasAny").Append(section.SectionName).AppendLine("FieldSet()");
        sb.AppendLine("    {");
        sb.Append("        var src = ").Append(DataAccessor(section)).AppendLine(";");
        sb.AppendLine("        if (src == null) return false;");
        // MaskMutator HasAnyPredicate: covers bits set by mutators that aren't tied
        // to any declared UpdateField presence check (e.g. ActivePlayer InvSlots fan
        // from GetModernInvSlot, GlyphsDirty from _gameState). Without these the
        // Update writer is skipped and the modern client never sees the change.
        foreach (var mm in section.MaskMutators)
        {
            if (string.IsNullOrEmpty(mm.HasAnyPredicate))
                continue;
            sb.Append("        if (").Append(mm.HasAnyPredicate).AppendLine(") return true;");
        }
        foreach (var f in section.UpdateFields)
        {
            // Scalar CustomPredicate takes priority — used when the source property doesn't
            // fit the default .HasValue/!=null shape (e.g. ActivePlayer.KnownTitles is an
            // array source but emitted as a single dynamic-field write, predicated on
            // "any non-null element").
            if (f.ArrayCount == 0 && !string.IsNullOrEmpty(f.CustomPredicate))
            {
                sb.Append("        if (").Append(f.CustomPredicate).AppendLine(") return true;");
                continue;
            }
            if (f.ArrayCount > 0)
            {
                // CustomPredicate (when set) is authoritative — handles cases where the
                // wire bit-count exceeds the source array length (e.g. PvpInfo: 7 wire
                // slots, 6 in PVPInfo[6]; the predicate guards with `i < src.PvpInfo.Length`).
                if (!string.IsNullOrEmpty(f.CustomPredicate))
                {
                    for (int i = 0; i < f.ArrayCount; i++)
                    {
                        sb.Append("        if (").Append(f.CustomPredicate!.Replace("{i}", i.ToString())).AppendLine(") return true;");
                    }
                    continue;
                }
                // CustomWriter implies a class-array (e.g. ItemEnchantment?[]) → != null predicate.
                bool useNotNull = f.Type == DescriptorType.PackedGuid128 || !string.IsNullOrEmpty(f.CustomWriter);
                if (useNotNull)
                {
                    sb.Append("        if (src.").Append(f.SourceProperty).Append(" != null) for (int i = 0; i < ")
                      .Append(f.ArrayCount).Append("; i++) if (src.").Append(f.SourceProperty)
                      .AppendLine("[i] != null) return true;");
                }
                else
                {
                    sb.Append("        if (src.").Append(f.SourceProperty).Append(" != null) for (int i = 0; i < ")
                      .Append(f.ArrayCount).Append("; i++) if (src.").Append(f.SourceProperty)
                      .AppendLine("[i].HasValue) return true;");
                }
            }
            else if (f.Type == DescriptorType.PackedGuid128)
            {
                sb.Append("        if (src.").Append(f.SourceProperty).AppendLine(" != null) return true;");
            }
            else
            {
                sb.Append("        if (src.").Append(f.SourceProperty).AppendLine(".HasValue) return true;");
            }
        }
        sb.AppendLine("        return false;");
        sb.AppendLine("    }");
    }

    private static string WriteMethodNameFor(DescriptorType type) => type switch
    {
        DescriptorType.Int32         => "WriteInt32",
        DescriptorType.UInt32        => "WriteUInt32",
        DescriptorType.Int64         => "WriteInt64",
        DescriptorType.UInt64        => "WriteUInt64",
        DescriptorType.Float         => "WriteFloat",
        DescriptorType.Int8          => "WriteInt8",
        DescriptorType.UInt8         => "WriteUInt8",
        DescriptorType.Int16         => "WriteInt16",
        DescriptorType.UInt16        => "WriteUInt16",
        DescriptorType.PackedGuid128 => "WritePackedGuid128",
        _ => throw new InvalidOperationException("Unknown DescriptorType: " + type),
    };

    private static string ZeroLiteralFor(DescriptorType type) => type switch
    {
        DescriptorType.Int32         => "0",
        DescriptorType.UInt32        => "0u",
        DescriptorType.Int64         => "0L",
        DescriptorType.UInt64        => "0uL",
        DescriptorType.Float         => "0f",
        DescriptorType.Int8          => "(sbyte)0",
        DescriptorType.UInt8         => "(byte)0",
        DescriptorType.Int16         => "(short)0",
        DescriptorType.UInt16        => "(ushort)0",
        DescriptorType.PackedGuid128 => "HermesProxy.World.WowGuid128.Empty",
        _ => "default",
    };

    private static INamespaceSymbol? ResolveNamespace(INamespaceSymbol globalNs, string dottedName)
    {
        INamespaceSymbol current = globalNs;
        foreach (var part in dottedName.Split('.'))
        {
            var next = current.GetNamespaceMembers().FirstOrDefault(n => n.Name == part);
            if (next is null)
                return null;
            current = next;
        }
        return current;
    }

    // Mirror of HermesProxy.World.Objects.Version.Attributes.DescriptorType — ordinal-stable.
    private enum DescriptorType
    {
        Int32 = 0,
        UInt32 = 1,
        Int64 = 2,
        UInt64 = 3,
        Float = 4,
        Int8 = 5,
        UInt8 = 6,
        Int16 = 7,
        UInt16 = 8,
        PackedGuid128 = 9,
    }

    // Mirror of HermesProxy.World.Objects.Version.Attributes.MaskMode — ordinal-stable.
    private enum MaskMode
    {
        Blocks = 0,
        Flat = 1,
    }

    // Mirror of HermesProxy.World.Objects.Version.Attributes.ArrayMode — ordinal-stable.
    private enum ArrayMode
    {
        Grouped = 0,
        PerElement = 1,
    }

    private enum CreateSequenceKind { Field, Placeholder, BitsPlaceholder }

    private sealed record CreateFieldEntry(
        string sourceProperty,
        DescriptorType type,
        string? defaultExpression,
        int arrayCount,
        ArrayMode arrayMode,
        string? defaultExpressionByIndex,
        bool ownerOnly,
        string? customWriter,
        string? cast)
    {
        public string SourceProperty => sourceProperty;
        public DescriptorType Type => type;
        public string? DefaultExpression => defaultExpression;
        public int ArrayCount => arrayCount;
        public ArrayMode ArrayMode => arrayMode;
        public string? DefaultExpressionByIndex => defaultExpressionByIndex;
        public bool OwnerOnly => ownerOnly;
        public string? CustomWriter => customWriter;
        public string? Cast => cast;
    }

    private sealed record UpdateFieldEntry(
        string sourceProperty,
        DescriptorType type,
        int bit,
        int arrayCount,
        ArrayMode arrayMode,
        int parentBit,
        string? defaultExpressionByIndex,
        string? customWriter,
        string? cast,
        string? customPredicate,
        bool maskOnly,
        int writeOrder)
    {
        public string SourceProperty => sourceProperty;
        public DescriptorType Type => type;
        public int Bit => bit;
        public int ArrayCount => arrayCount;
        public ArrayMode ArrayMode => arrayMode;
        public int ParentBit => parentBit;
        public string? DefaultExpressionByIndex => defaultExpressionByIndex;
        public string? CustomWriter => customWriter;
        public string? Cast => cast;
        public string? CustomPredicate => customPredicate;
        public bool MaskOnly => maskOnly;
        public int WriteOrder => writeOrder;
    }

    private sealed record CustomFieldEntry(string Label, int Bit, string CustomWriter, int ParentBit, bool WriteOnly);

    private sealed record MaskPreambleEntry(int Bit, string CustomWriter);

    private sealed record UpdateBitsPreambleEntry(uint Value, int BitCount);

    private sealed record CreatePlaceholderEntry(DescriptorType Type, string LiteralExpression, bool OwnerOnly, string? CustomWriter);

    private sealed record CreateBitsPlaceholderEntry(uint Value, int BitCount, bool Flush, bool OwnerOnly);

    private sealed class MemberEntry
    {
        public string Name { get; }
        public CreateFieldEntry? Create { get; }
        public UpdateFieldEntry? Update { get; }
        public CreatePlaceholderEntry? Placeholder { get; }
        public CreateBitsPlaceholderEntry? BitsPlaceholder { get; }
        public CustomFieldEntry? Custom { get; }
        public MaskPreambleEntry? MaskPreamble { get; }
        public UpdateBitsPreambleEntry? UpdateBitsPreamble { get; }
        public MaskMutatorEntry? MaskMutator { get; }
        public UpdatePostFlushEntry? UpdatePostFlush { get; }

        private MemberEntry(string name, CreateFieldEntry? c, UpdateFieldEntry? u, CreatePlaceholderEntry? p, CreateBitsPlaceholderEntry? b, CustomFieldEntry? cf, MaskPreambleEntry? mp, UpdateBitsPreambleEntry? ubp, MaskMutatorEntry? mm, UpdatePostFlushEntry? pf)
        {
            Name = name; Create = c; Update = u; Placeholder = p; BitsPlaceholder = b; Custom = cf; MaskPreamble = mp; UpdateBitsPreamble = ubp; MaskMutator = mm; UpdatePostFlush = pf;
        }

        public static MemberEntry FromCreate(string name, CreateFieldEntry c) => new(name, c, null, null, null, null, null, null, null, null);
        public static MemberEntry FromUpdate(string name, UpdateFieldEntry u) => new(name, null, u, null, null, null, null, null, null, null);
        public static MemberEntry FromPlaceholder(string name, CreatePlaceholderEntry p) => new(name, null, null, p, null, null, null, null, null, null);
        public static MemberEntry FromBitsPlaceholder(string name, CreateBitsPlaceholderEntry b) => new(name, null, null, null, b, null, null, null, null, null);
        public static MemberEntry FromCustom(string name, CustomFieldEntry cf) => new(name, null, null, null, null, cf, null, null, null, null);
        public static MemberEntry FromMaskPreamble(string name, MaskPreambleEntry mp) => new(name, null, null, null, null, null, mp, null, null, null);
        public static MemberEntry FromUpdateBitsPreamble(string name, UpdateBitsPreambleEntry ubp) => new(name, null, null, null, null, null, null, ubp, null, null);
        public static MemberEntry FromMaskMutator(string name, MaskMutatorEntry mm) => new(name, null, null, null, null, null, null, null, mm, null);
        public static MemberEntry FromUpdatePostFlush(string name, UpdatePostFlushEntry pf) => new(name, null, null, null, null, null, null, null, null, pf);
    }

    private sealed record SectionEntry(
        string SectionName,
        INamedTypeSymbol DataType,
        MaskMode MaskMode,
        int MaskWidth,
        int BlockCount,
        bool Cascade,
        BlockMaskShape BlockMaskShape,
        List<(CreateSequenceKind Kind, object Payload)> CreateSequence,
        List<UpdateFieldEntry> UpdateFields,
        List<CustomFieldEntry> CustomFields,
        List<MaskPreambleEntry> MaskPreambles,
        List<MaskMutatorEntry> MaskMutators,
        List<UpdatePostFlushEntry> UpdatePostFlushes,
        List<UpdateBitsPreambleEntry> UpdateBitsPreambles);

    private sealed record MaskMutatorEntry(string CustomWriter, string? HasAnyPredicate);

    private sealed record UpdatePostFlushEntry(int AfterBit);

    // Mirror of HermesProxy.World.Objects.Version.Attributes.BlockMaskShape — ordinal-stable.
    private enum BlockMaskShape
    {
        Bits = 0,
        UInt32PlusBits16 = 1,
    }

    private sealed record VersionEntry(string VersionName, List<SectionEntry> Sections);

    private sealed class GeneratorModel
    {
        public List<VersionEntry> Versions { get; } = new();
        public List<Diagnostic> Diagnostics { get; } = new();
    }
}
