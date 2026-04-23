using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace HermesProxy.SourceGen;

/// <summary>
/// Emits the descriptor-tree <c>WriteCreate{Type}Data</c> partial methods on per-version
/// <c>ObjectUpdateBuilder</c> classes — the serializer for WotLK Classic 3.4.3's hierarchical
/// <c>SMSG_UPDATE_OBJECT</c> wire format. Bootstrap scope: Object type only.
///
/// For each per-version <c>V*</c> namespace in <c>HermesProxy.World.Enums</c> that contains
/// an <c>ObjectField</c> enum decorated with <c>[DescriptorCreateField]</c> attributes, this
/// generator emits a partial method on the matching
/// <c>HermesProxy.World.Objects.Version.{Ver}.ObjectUpdateBuilder</c> class. Enum members
/// without the attribute are skipped — several descriptor slots (e.g. <c>OBJECT_FIELD_GUID</c>)
/// aren't written as fields because their value is emitted separately in the update preamble.
///
/// Diagnostics:
///   HPSG003 — <c>[DescriptorCreateField]</c> <c>SourceProperty</c> references a member that
///             doesn't exist on the target data type. Generator skips that field.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class ObjectUpdateBuilderGenerator : IIncrementalGenerator
{
    private const string CreateFieldAttrFullName = "HermesProxy.World.Objects.Version.Attributes.DescriptorCreateFieldAttribute";
    private const string DescriptorTypeFullName = "HermesProxy.World.Objects.Version.Attributes.DescriptorType";
    private const string ObjectDataFullName = "HermesProxy.World.Objects.ObjectData";
    private const string EnumsNamespace = "HermesProxy.World.Enums";
    private const string BuilderNamespacePrefix = "HermesProxy.World.Objects.Version.";
    private const string WorldPacketFullName = "HermesProxy.World.WorldPacket";

    private static readonly DiagnosticDescriptor HPSG003_UnknownSourceProperty = new(
        id: "HPSG003",
        title: "DescriptorCreateField source property not found",
        messageFormat: "[DescriptorCreateField] references property '{0}' on '{1}' but that property does not exist; field will be skipped",
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
        var attr = compilation.GetTypeByMetadataName(CreateFieldAttrFullName);
        var descriptorTypeEnum = compilation.GetTypeByMetadataName(DescriptorTypeFullName);
        var objectData = compilation.GetTypeByMetadataName(ObjectDataFullName);
        if (attr is null || descriptorTypeEnum is null || objectData is null)
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

            var objectFieldEnum = child.GetTypeMembers("ObjectField").FirstOrDefault(t => t.TypeKind == TypeKind.Enum);
            if (objectFieldEnum is null)
                continue;

            var fields = new List<CreateFieldEntry>();
            foreach (var member in objectFieldEnum.GetMembers().OfType<IFieldSymbol>())
            {
                if (!member.IsConst)
                    continue;

                var attrData = member.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attr));
                if (attrData is null)
                    continue;

                if (attrData.ConstructorArguments.Length < 2)
                    continue;

                var sourceProperty = attrData.ConstructorArguments[0].Value as string;
                if (string.IsNullOrEmpty(sourceProperty))
                    continue;

                var typeOrdinal = attrData.ConstructorArguments[1].Value as int?;
                if (typeOrdinal is null)
                    continue;

                string? defaultExpression = null;
                foreach (var named in attrData.NamedArguments)
                {
                    if (named.Key == "DefaultExpression")
                        defaultExpression = named.Value.Value as string;
                }

                // Validate the source property exists on ObjectData. Skip-with-warning if not.
                var objectDataMembers = objectData.GetMembers(sourceProperty!);
                if (objectDataMembers.IsDefaultOrEmpty)
                {
                    model.Diagnostics.Add(Diagnostic.Create(
                        HPSG003_UnknownSourceProperty,
                        member.Locations.FirstOrDefault() ?? Location.None,
                        sourceProperty,
                        objectData.ToDisplayString()));
                    continue;
                }

                fields.Add(new CreateFieldEntry(
                    enumMemberName: member.Name,
                    sourceProperty: sourceProperty!,
                    type: (DescriptorType)typeOrdinal.Value,
                    defaultExpression: defaultExpression));
            }

            if (fields.Count == 0)
                continue;

            model.Versions.Add(new VersionEntry(child.Name, fields));
        }

        return model.Versions.Count == 0 && model.Diagnostics.Count == 0 ? null : model;
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
        sb.Append("    private void WriteCreateObjectData(").Append(WorldPacketFullName).AppendLine(" data)");
        sb.AppendLine("    {");
        sb.AppendLine("        var obj = _updateData.ObjectData;");
        foreach (var f in version.Fields)
        {
            sb.Append("        ").Append(WriteCallFor(f)).AppendLine(";");
        }
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string WriteCallFor(CreateFieldEntry field)
    {
        var valueExpr = field.DefaultExpression is null
            ? $"obj.{field.SourceProperty}.GetValueOrDefault()"
            : $"obj.{field.SourceProperty} ?? {field.DefaultExpression}";

        return field.Type switch
        {
            DescriptorType.Int32  => $"data.WriteInt32({valueExpr})",
            DescriptorType.UInt32 => $"data.WriteUInt32({valueExpr})",
            DescriptorType.Int64  => $"data.WriteInt64({valueExpr})",
            DescriptorType.UInt64 => $"data.WriteUInt64({valueExpr})",
            DescriptorType.Float  => $"data.WriteFloat({valueExpr})",
            _ => throw new InvalidOperationException("Unknown DescriptorType: " + field.Type),
        };
    }

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
    }

    private sealed record CreateFieldEntry(
        string enumMemberName,
        string sourceProperty,
        DescriptorType type,
        string? defaultExpression)
    {
        public string EnumMemberName => enumMemberName;
        public string SourceProperty => sourceProperty;
        public DescriptorType Type => type;
        public string? DefaultExpression => defaultExpression;
    }

    private sealed record VersionEntry(string VersionName, List<CreateFieldEntry> Fields);

    private sealed class GeneratorModel
    {
        public List<VersionEntry> Versions { get; } = new();
        public List<Diagnostic> Diagnostics { get; } = new();
    }
}
