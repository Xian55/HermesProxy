using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace HermesProxy.SourceGen;

/// <summary>
/// Emits literal Opcode translation tables for every per-version <c>Opcode</c> enum found under
/// <c>HermesProxy.World.Enums.V*</c>. Replaces the reflective enum-walking that
/// <c>LegacyVersion.LoadOpcodeTables</c> / <c>ModernVersion.LoadOpcodeTables</c> used to do at
/// runtime — the trade is one-time compile cost for zero runtime reflection and one fewer
/// <c>IL2026</c> trim warning on the published binary.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class OpcodeTableGenerator : IIncrementalGenerator
{
    private const string UniversalEnumFullName = "HermesProxy.World.Enums.Opcode";
    private const string ClientVersionBuildFullName = "HermesProxy.Enums.ClientVersionBuild";
    private const string EnumsNamespace = "HermesProxy.World.Enums";

    private static readonly DiagnosticDescriptor MissingUniversalMember = new(
        id: "HPSG001",
        title: "Opcode missing from universal enum",
        messageFormat: "Per-version opcode '{0}' (build {1}) has no match in HermesProxy.World.Enums.Opcode and will be dropped from the generated table",
        category: "HermesProxy.SourceGen",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor MissingClientVersionBuildMember = new(
        id: "HPSG002",
        title: "Per-version namespace has no matching ClientVersionBuild",
        messageFormat: "Namespace HermesProxy.World.Enums.{0} has an Opcode enum but there is no ClientVersionBuild.{0} member; the opcode table will not be emitted",
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

            var source = Emit(m);
            ctx.AddSource("GeneratedOpcodeTables.g.cs", source);
        });
    }

    private static GeneratorModel? BuildModel(Compilation compilation, System.Threading.CancellationToken ct)
    {
        var universal = compilation.GetTypeByMetadataName(UniversalEnumFullName);
        if (universal is null || universal.EnumUnderlyingType is null)
            return null;

        // Build a name → universal int lookup once.
        var universalByName = new Dictionary<string, uint>(StringComparer.Ordinal);
        foreach (var field in universal.GetMembers().OfType<IFieldSymbol>())
        {
            if (!field.IsConst || !field.HasConstantValue)
                continue;
            universalByName[field.Name] = Convert.ToUInt32(field.ConstantValue);
        }

        // Resolve ClientVersionBuild and collect its member names so we can validate that each
        // per-version namespace has a matching enum case — otherwise our generated switch would
        // not compile.
        var cvb = compilation.GetTypeByMetadataName(ClientVersionBuildFullName);
        if (cvb is null)
            return null;
        var cvbNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var field in cvb.GetMembers().OfType<IFieldSymbol>())
        {
            if (field.IsConst)
                cvbNames.Add(field.Name);
        }

        // Find the HermesProxy.World.Enums namespace and walk its sub-namespaces that start with "V".
        var enumsNs = ResolveNamespace(compilation.GlobalNamespace, EnumsNamespace);
        if (enumsNs is null)
            return null;

        var diagnostics = new List<Diagnostic>();
        var versions = ImmutableArray.CreateBuilder<VersionModel>();

        foreach (var childNs in enumsNs.GetNamespaceMembers())
        {
            ct.ThrowIfCancellationRequested();

            if (!LooksLikeVersionNamespace(childNs.Name))
                continue;

            var opcodeEnum = childNs.GetTypeMembers("Opcode").FirstOrDefault(t => t.TypeKind == TypeKind.Enum);
            if (opcodeEnum is null)
                continue;

            // Only emit versions whose namespace name maps 1:1 onto a ClientVersionBuild member.
            // Otherwise the generated switch arm would reference a non-existent enum value.
            if (!cvbNames.Contains(childNs.Name))
            {
                diagnostics.Add(Diagnostic.Create(
                    MissingClientVersionBuildMember,
                    opcodeEnum.Locations.FirstOrDefault(),
                    childNs.Name));
                continue;
            }

            var entries = ImmutableArray.CreateBuilder<OpcodeEntry>();
            foreach (var field in opcodeEnum.GetMembers().OfType<IFieldSymbol>())
            {
                if (!field.IsConst || !field.HasConstantValue)
                    continue;

                uint versionValue = Convert.ToUInt32(field.ConstantValue);
                if (!universalByName.TryGetValue(field.Name, out uint universalValue))
                {
                    // MSG_NULL_ACTION is expected in every version but value 0 and is excluded from
                    // the translation tables (the zero slot is used as the "not found" sentinel).
                    if (field.Name != "MSG_NULL_ACTION")
                    {
                        diagnostics.Add(Diagnostic.Create(
                            MissingUniversalMember,
                            field.Locations.FirstOrDefault(),
                            field.Name,
                            childNs.Name));
                    }
                    continue;
                }

                if (versionValue == 0)
                    continue;

                entries.Add(new OpcodeEntry(versionValue, universalValue, field.Name));
            }

            versions.Add(new VersionModel(childNs.Name, entries.ToImmutable()));
        }

        if (versions.Count == 0)
            return null;

        return new GeneratorModel(versions.ToImmutable(), diagnostics.ToImmutableArray());
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

    private static bool LooksLikeVersionNamespace(string name)
    {
        // Accept V1_12_1_5875, V2_4_3_8606, V3_3_5_12340, V2_5_2_39570, V1_14_1_40688, V2_5_3_41750, etc.
        // Shape: "V" + three or four "<digits>_" segments. Keep the check permissive — the
        // generator only acts on namespaces that also contain an Opcode enum.
        return name.Length > 2 && name[0] == 'V' && char.IsDigit(name[1]);
    }

    private static string Emit(GeneratorModel model)
    {
        var sb = new StringBuilder(1024 * 64);
        sb.AppendLine("// <auto-generated>");
        sb.AppendLine("// Produced by HermesProxy.SourceGen.OpcodeTableGenerator.");
        sb.AppendLine("// Do not edit — regenerate by editing the per-version Opcode enum sources.");
        sb.AppendLine("// </auto-generated>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using HermesProxy.Enums;");
        sb.AppendLine("using HermesProxy.World.Enums;");
        sb.AppendLine();
        sb.AppendLine("namespace HermesProxy;");
        sb.AppendLine();
        sb.AppendLine("internal static class GeneratedOpcodeTables");
        sb.AppendLine("{");

        // TryGet switch — one arm per discovered per-version enum.
        sb.AppendLine("    public static bool TryGet(ClientVersionBuild build, out Opcode[] currentToUniversal, out uint[] universalToCurrent)");
        sb.AppendLine("    {");
        sb.AppendLine("        switch (build)");
        sb.AppendLine("        {");
        foreach (var v in model.Versions)
        {
            sb.Append("            case ClientVersionBuild.");
            sb.Append(v.Namespace);
            sb.AppendLine(":");
            sb.Append("                currentToUniversal = ");
            sb.Append(v.Namespace);
            sb.AppendLine("_C2U;");
            sb.Append("                universalToCurrent = ");
            sb.Append(v.Namespace);
            sb.AppendLine("_U2C;");
            sb.AppendLine("                return true;");
        }
        sb.AppendLine("            default:");
        sb.AppendLine("                currentToUniversal = Array.Empty<Opcode>();");
        sb.AppendLine("                universalToCurrent = Array.Empty<uint>();");
        sb.AppendLine("                return false;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Emit one pair of arrays per version.
        foreach (var v in model.Versions)
        {
            EmitVersionArrays(sb, v);
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static void EmitVersionArrays(StringBuilder sb, VersionModel v)
    {
        if (v.Entries.Length == 0)
        {
            sb.Append("    private static readonly Opcode[] ").Append(v.Namespace).AppendLine("_C2U = Array.Empty<Opcode>();");
            sb.Append("    private static readonly uint[] ").Append(v.Namespace).AppendLine("_U2C = Array.Empty<uint>();");
            sb.AppendLine();
            return;
        }

        // Size arrays to match max observed values (+1 for the inclusive slot).
        uint maxCurrent = 0;
        uint maxUniversal = 0;
        foreach (var e in v.Entries)
        {
            if (e.CurrentValue > maxCurrent) maxCurrent = e.CurrentValue;
            if (e.UniversalValue > maxUniversal) maxUniversal = e.UniversalValue;
        }

        // Forward table: Opcode[maxCurrent + 1] — default slot value is Opcode.MSG_NULL_ACTION (0),
        // which matches the "not found" sentinel used by the previous FrozenDictionary path.
        sb.Append("    private static readonly Opcode[] ").Append(v.Namespace).Append("_C2U = new Opcode[")
          .Append(maxCurrent + 1).AppendLine("]");
        sb.AppendLine("    {");
        var c2uSlots = new string[maxCurrent + 1];
        for (int i = 0; i <= maxCurrent; i++)
            c2uSlots[i] = "Opcode.MSG_NULL_ACTION";
        foreach (var e in v.Entries)
            c2uSlots[(int)e.CurrentValue] = "Opcode." + e.UniversalName;
        for (int i = 0; i <= maxCurrent; i++)
        {
            sb.Append("        ").Append(c2uSlots[i]);
            if (i < maxCurrent) sb.Append(',');
            sb.AppendLine();
        }
        sb.AppendLine("    };");
        sb.AppendLine();

        // Reverse table: uint[maxUniversal + 1] — default 0 means "not mapped" (caller interprets
        // 0 as invalid, same as the FrozenDictionary fallback).
        sb.Append("    private static readonly uint[] ").Append(v.Namespace).Append("_U2C = new uint[")
          .Append(maxUniversal + 1).AppendLine("]");
        sb.AppendLine("    {");
        var u2cSlots = new uint[maxUniversal + 1];
        foreach (var e in v.Entries)
            u2cSlots[(int)e.UniversalValue] = e.CurrentValue;
        for (int i = 0; i <= maxUniversal; i++)
        {
            sb.Append("        ").Append(u2cSlots[i]).Append('u');
            if (i < maxUniversal) sb.Append(',');
            sb.AppendLine();
        }
        sb.AppendLine("    };");
        sb.AppendLine();
    }

    private sealed record OpcodeEntry(uint CurrentValue, uint UniversalValue, string UniversalName);
    private sealed record VersionModel(string Namespace, ImmutableArray<OpcodeEntry> Entries);
    private sealed record GeneratorModel(ImmutableArray<VersionModel> Versions, ImmutableArray<Diagnostic> Diagnostics);
}
