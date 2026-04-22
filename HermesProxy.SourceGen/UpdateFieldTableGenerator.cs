using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace HermesProxy.SourceGen;

/// <summary>
/// Emits the UpdateField descriptor tables that <c>LegacyVersion.UpdateFields&lt;T&gt;</c> and
/// <c>ModernVersion.UpdateFields&lt;T&gt;</c> consume. Replaces the reflective
/// <c>LoadUFDictionariesInto</c> method that used <c>Assembly.GetType(string)</c> + attribute
/// reflection to build tables at runtime.
///
/// For each (update-field-defining build, universal field enum type) combination it emits:
///   - an <c>int[]</c> of per-version wire offsets (sorted ascending)
///   - a parallel <c>UpdateFieldInfo[]</c> with Name / Size / Format literals
///   - a <c>Dictionary&lt;string,int&gt;</c> of name → per-version-offset for
///     <c>GetUpdateField&lt;T&gt;(T field)</c> callers.
///
/// Format resolution mirrors the old code: each universal enum member carries one or more
/// <c>[UpdateField(type, sinceVersion)]</c> attributes; the generator picks the attribute
/// whose <c>Version</c> is highest but still <= the target defining build. Members with no
/// qualifying attribute default to <c>UpdateFieldType.Default</c>.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class UpdateFieldTableGenerator : IIncrementalGenerator
{
    private const string UpdateFieldAttrFullName = "HermesProxy.World.Enums.UpdateFieldAttribute";
    private const string UpdateFieldTypeFullName = "HermesProxy.World.Enums.UpdateFieldType";
    private const string UpdateFieldInfoFullName = "HermesProxy.UpdateFieldInfo";
    private const string ClientVersionBuildFullName = "HermesProxy.Enums.ClientVersionBuild";
    private const string EnumsNamespace = "HermesProxy.World.Enums";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var model = context.CompilationProvider.Select(BuildModel);

        context.RegisterSourceOutput(model, static (ctx, m) =>
        {
            if (m is null)
                return;
            ctx.AddSource("GeneratedUpdateFieldTables.g.cs", Emit(m));
        });
    }

    private static GeneratorModel? BuildModel(Compilation compilation, System.Threading.CancellationToken ct)
    {
        var ufAttr = compilation.GetTypeByMetadataName(UpdateFieldAttrFullName);
        var ufType = compilation.GetTypeByMetadataName(UpdateFieldTypeFullName);
        var cvb = compilation.GetTypeByMetadataName(ClientVersionBuildFullName);
        if (ufAttr is null || ufType is null || cvb is null)
            return null;

        var enumsNs = ResolveNamespace(compilation.GlobalNamespace, EnumsNamespace);
        if (enumsNs is null)
            return null;

        // Find universal field enum types in HermesProxy.World.Enums (not sub-namespaces). We pick
        // every namespace-level enum that also exists with the same name in at least one V* child
        // namespace — that's the exact constraint the old loader enforced.
        var versionNamespaces = new List<INamespaceSymbol>();
        foreach (var child in enumsNs.GetNamespaceMembers())
        {
            if (child.Name.Length > 1 && child.Name[0] == 'V' && char.IsDigit(child.Name[1]))
                versionNamespaces.Add(child);
        }

        var universalEnums = new List<INamedTypeSymbol>();
        foreach (var t in enumsNs.GetTypeMembers())
        {
            if (t.TypeKind != TypeKind.Enum) continue;
            if (t.Name is "UpdateFieldType" or "Opcode") continue;
            if (versionNamespaces.Any(vns => vns.GetTypeMembers(t.Name).Any(vt => vt.TypeKind == TypeKind.Enum)))
                universalEnums.Add(t);
        }

        // Build lookup: universal enum member name → list of (version, format) attribute entries.
        // Same attribute may appear multiple times (AllowMultiple = true) — one per version threshold.
        var universalAttrs = new Dictionary<string, Dictionary<string, ImmutableArray<AttrEntry>>>(StringComparer.Ordinal);
        foreach (var ue in universalEnums)
        {
            var byMember = new Dictionary<string, ImmutableArray<AttrEntry>>(StringComparer.Ordinal);
            foreach (var member in ue.GetMembers().OfType<IFieldSymbol>())
            {
                if (!member.IsConst) continue;
                var entries = ImmutableArray.CreateBuilder<AttrEntry>();
                foreach (var attr in member.GetAttributes())
                {
                    if (!SymbolEqualityComparer.Default.Equals(attr.AttributeClass, ufAttr))
                        continue;
                    // Ctor args: (UpdateFieldType attrib) OR (UpdateFieldType attrib, ClientVersionBuild fromVersion)
                    if (attr.ConstructorArguments.Length < 1)
                        continue;
                    var format = Convert.ToInt32(attr.ConstructorArguments[0].Value);
                    uint version = 0;
                    if (attr.ConstructorArguments.Length >= 2 && attr.ConstructorArguments[1].Value is not null)
                        version = Convert.ToUInt32(attr.ConstructorArguments[1].Value);
                    entries.Add(new AttrEntry(version, format));
                }
                byMember[member.Name] = entries.ToImmutable();
            }
            universalAttrs[ue.Name] = byMember;
        }

        // For each per-version namespace, pair up with the universal enums by type name.
        var tablesByBuild = ImmutableArray.CreateBuilder<BuildTables>();
        foreach (var vns in versionNamespaces)
        {
            ct.ThrowIfCancellationRequested();

            // The version namespace name is e.g. "V1_12_1_5875"; the ClientVersionBuild member of
            // the same name must exist or we can't address this build in the generated switch.
            if (!cvb.GetMembers(vns.Name).OfType<IFieldSymbol>().Any())
                continue;

            // Build number for attribute-version filtering comes from the ClientVersionBuild member.
            var cvbField = cvb.GetMembers(vns.Name).OfType<IFieldSymbol>().First();
            if (!cvbField.HasConstantValue)
                continue;
            uint buildValue = Convert.ToUInt32(cvbField.ConstantValue);

            var perType = ImmutableArray.CreateBuilder<TypeTable>();
            foreach (var universalEnum in universalEnums)
            {
                var perVersionEnum = vns.GetTypeMembers(universalEnum.Name).FirstOrDefault(t => t.TypeKind == TypeKind.Enum);
                if (perVersionEnum is null) continue;

                var entries = new List<(uint Value, string Name, int Format)>();
                foreach (var field in perVersionEnum.GetMembers().OfType<IFieldSymbol>())
                {
                    if (!field.IsConst || !field.HasConstantValue) continue;
                    uint value = Convert.ToUInt32(field.ConstantValue);

                    int format = 0; // UpdateFieldType.Default
                    if (universalAttrs.TryGetValue(universalEnum.Name, out var byMember)
                        && byMember.TryGetValue(field.Name, out var attrList))
                    {
                        // Pick attr with highest Version <= buildValue
                        int best = -1;
                        uint bestVersion = 0;
                        for (int i = 0; i < attrList.Length; i++)
                        {
                            var a = attrList[i];
                            if (a.Version > buildValue) continue;
                            if (best < 0 || a.Version > bestVersion)
                            {
                                best = i;
                                bestVersion = a.Version;
                            }
                        }
                        if (best >= 0) format = attrList[best].Format;
                    }

                    entries.Add((value, field.Name, format));
                }

                if (entries.Count == 0) continue;

                // Sort by value, compute Size[i] = keys[i+1] - keys[i] for all but last.
                entries.Sort((a, b) => a.Value.CompareTo(b.Value));
                var sortedEntries = ImmutableArray.CreateBuilder<EntryModel>();
                for (int i = 0; i < entries.Count; i++)
                {
                    uint value = entries[i].Value;
                    int size = i < entries.Count - 1 ? (int)(entries[i + 1].Value - value) : 0;
                    sortedEntries.Add(new EntryModel((int)value, entries[i].Name, size, entries[i].Format));
                }

                perType.Add(new TypeTable(universalEnum.Name, sortedEntries.ToImmutable()));
            }

            if (perType.Count > 0)
                tablesByBuild.Add(new BuildTables(vns.Name, perType.ToImmutable()));
        }

        if (tablesByBuild.Count == 0) return null;

        return new GeneratorModel(tablesByBuild.ToImmutable());
    }

    private static INamespaceSymbol? ResolveNamespace(INamespaceSymbol root, string fullyQualified)
    {
        INamespaceSymbol? current = root;
        foreach (var part in fullyQualified.Split('.'))
        {
            current = current?.GetNamespaceMembers().FirstOrDefault(n => n.Name == part);
            if (current is null) return null;
        }
        return current;
    }

    private static string Emit(GeneratorModel model)
    {
        var sb = new StringBuilder(1024 * 1024);
        sb.AppendLine("// <auto-generated>");
        sb.AppendLine("// Produced by HermesProxy.SourceGen.UpdateFieldTableGenerator.");
        sb.AppendLine("// Do not edit — regenerate by editing the per-version UpdateFields.cs enums or");
        sb.AppendLine("// the universal HermesProxy.World.Enums.UpdateFields.cs attributes.");
        sb.AppendLine("// </auto-generated>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using HermesProxy.Enums;");
        sb.AppendLine("using HermesProxy.World.Enums;");
        sb.AppendLine();
        sb.AppendLine("namespace HermesProxy;");
        sb.AppendLine();
        sb.AppendLine("internal static class GeneratedUpdateFieldTables");
        sb.AppendLine("{");

        // Top-level dispatch method: (definingBuild, Type) → tables.
        sb.AppendLine("    public static bool TryGet(ClientVersionBuild definingBuild, Type enumType,");
        sb.AppendLine("        out int[] keys, out UpdateFieldInfo[] infos, out Dictionary<string, int>? namesToValues)");
        sb.AppendLine("    {");
        sb.AppendLine("        switch (definingBuild)");
        sb.AppendLine("        {");
        foreach (var b in model.Builds)
        {
            sb.Append("            case ClientVersionBuild.").Append(b.Namespace).AppendLine(":");
            sb.Append("                return TryGet_").Append(b.Namespace).AppendLine("(enumType, out keys, out infos, out namesToValues);");
        }
        sb.AppendLine("        }");
        sb.AppendLine("        keys = Array.Empty<int>(); infos = Array.Empty<UpdateFieldInfo>(); namesToValues = null;");
        sb.AppendLine("        return false;");
        sb.AppendLine("    }");
        sb.AppendLine();

        foreach (var b in model.Builds)
        {
            // Per-build dispatch: pick the type.
            sb.Append("    private static bool TryGet_").Append(b.Namespace).AppendLine("(Type t,");
            sb.AppendLine("        out int[] keys, out UpdateFieldInfo[] infos, out Dictionary<string, int>? namesToValues)");
            sb.AppendLine("    {");
            foreach (var tt in b.Tables)
            {
                sb.Append("        if (t == typeof(").Append(tt.TypeName).AppendLine("))");
                sb.AppendLine("        {");
                sb.Append("            keys = ").Append(b.Namespace).Append('_').Append(tt.TypeName).AppendLine("_Keys;");
                sb.Append("            infos = ").Append(b.Namespace).Append('_').Append(tt.TypeName).AppendLine("_Infos;");
                sb.Append("            namesToValues = ").Append(b.Namespace).Append('_').Append(tt.TypeName).AppendLine("_Names;");
                sb.AppendLine("            return true;");
                sb.AppendLine("        }");
            }
            sb.AppendLine("        keys = Array.Empty<int>(); infos = Array.Empty<UpdateFieldInfo>(); namesToValues = null;");
            sb.AppendLine("        return false;");
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        // Per-(build,type) literal tables.
        foreach (var b in model.Builds)
        {
            foreach (var tt in b.Tables)
            {
                EmitTypeTable(sb, b.Namespace, tt);
            }
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static void EmitTypeTable(StringBuilder sb, string ns, TypeTable tt)
    {
        var prefix = ns + "_" + tt.TypeName;

        // Keys
        sb.Append("    private static readonly int[] ").Append(prefix).Append("_Keys = new int[").Append(tt.Entries.Length).AppendLine("]");
        sb.AppendLine("    {");
        for (int i = 0; i < tt.Entries.Length; i++)
        {
            sb.Append("        ").Append(tt.Entries[i].Value);
            if (i < tt.Entries.Length - 1) sb.Append(',');
            sb.AppendLine();
        }
        sb.AppendLine("    };");

        // Infos (array of UpdateFieldInfo literals)
        sb.Append("    private static readonly UpdateFieldInfo[] ").Append(prefix).Append("_Infos = new UpdateFieldInfo[").Append(tt.Entries.Length).AppendLine("]");
        sb.AppendLine("    {");
        for (int i = 0; i < tt.Entries.Length; i++)
        {
            var e = tt.Entries[i];
            sb.Append("        new UpdateFieldInfo { Value = ").Append(e.Value)
              .Append(", Name = \"").Append(EscapeString(e.Name))
              .Append("\", Size = ").Append(e.Size)
              .Append(", Format = (UpdateFieldType)").Append(e.Format)
              .Append(" }");
            if (i < tt.Entries.Length - 1) sb.Append(',');
            sb.AppendLine();
        }
        sb.AppendLine("    };");

        // Names → Value dictionary
        sb.Append("    private static readonly Dictionary<string, int> ").Append(prefix).Append("_Names = new Dictionary<string, int>(").Append(tt.Entries.Length).AppendLine(")");
        sb.AppendLine("    {");
        for (int i = 0; i < tt.Entries.Length; i++)
        {
            var e = tt.Entries[i];
            sb.Append("        { \"").Append(EscapeString(e.Name)).Append("\", ").Append(e.Value).Append(" }");
            if (i < tt.Entries.Length - 1) sb.Append(',');
            sb.AppendLine();
        }
        sb.AppendLine("    };");
        sb.AppendLine();
    }

    private static string EscapeString(string s)
    {
        // Enum member names — no special chars in practice; keep this defensive and cheap.
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private sealed record AttrEntry(uint Version, int Format);
    private sealed record EntryModel(int Value, string Name, int Size, int Format);
    private sealed record TypeTable(string TypeName, ImmutableArray<EntryModel> Entries);
    private sealed record BuildTables(string Namespace, ImmutableArray<TypeTable> Tables);
    private sealed record GeneratorModel(ImmutableArray<BuildTables> Builds);
}
