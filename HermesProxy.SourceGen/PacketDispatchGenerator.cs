using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace HermesProxy.SourceGen;

/// <summary>
/// Emits the inbound packet dispatch tables — one per direction — replacing the reflective
/// registries in <c>WorldSocket</c> and <c>WorldClient</c>.
/// </summary>
/// <remarks>
/// <para>
/// Those registries scan every method on their host type with <c>GetCustomAttributes</c> and build
/// a dictionary, <b>per socket instance</b> — twice per session on the modern side, since a session
/// opens a realm socket and an instance socket. This emits the same mapping as a
/// <c>static readonly</c> array of function pointers, indexed by universal opcode, built once per
/// process.
/// </para>
/// <para>
/// A null slot means "not converted yet" and is what lets the generated table and the old
/// reflective one coexist while handlers migrate: the dispatch site falls through to the
/// reflective path on null, so an unconverted opcode behaves exactly as before.
/// </para>
/// <para>
/// <b>Why a function-pointer array and not a switch.</b> A switch over ~800 sparse opcodes lowers
/// to a binary-search tree plus bucketed jump tables, in one method body the JIT must compile as a
/// unit on the first packet. The array is one bounds check, one load, one <c>calli</c> — and a null
/// slot gives the coexistence fallback for free. <c>delegate*[]</c> holds unmanaged pointers, so
/// the GC never traces the elements.
/// </para>
/// <para>
/// <b>Why two attributes rather than one with a direction enum.</b> This generator targets
/// netstandard2.0 and cannot reference the HermesProxy assembly, so any enum it interprets would
/// have to be mirrored here and matched <i>by ordinal</i> — the sharpest footgun in this project
/// per the handbook. Encoding direction in the attribute's identity means matching a
/// fully-qualified metadata name and mirroring nothing.
/// </para>
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class PacketDispatchGenerator : IIncrementalGenerator
{
    private const string CmsgAttributeFullName = "HermesProxy.World.Dispatch.HandlesCmsgAttribute";
    private const string SmsgAttributeFullName = "HermesProxy.World.Dispatch.HandlesSmsgAttribute";

    private const string DispatchNamespace = "HermesProxy.World.Dispatch";
    private const string OpcodeFullName = "global::HermesProxy.World.Enums.Opcode";
    private const string ReaderFullName = "global::Framework.IO.SpanPacketReader";
    private const string ContextFullName = "global::HermesProxy.World.Dispatch.SessionContext";
    private const string LegacyVersionFullName = "global::HermesProxy.LegacyVersion";
    private const string ModernVersionFullName = "global::HermesProxy.ModernVersion";

    private static readonly DiagnosticDescriptor OverlappingRanges = new(
        id: "HPSG004",
        title: "Two handlers claim one opcode over overlapping build ranges",
        messageFormat: "Opcode '{0}' is claimed by both '{1}' and '{2}' for overlapping build ranges; which one wins is undefined",
        category: "HermesProxy.SourceGen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RangedShadowedByUnranged = new(
        id: "HPSG005",
        title: "An unranged handler shadows a ranged one",
        messageFormat: "Opcode '{0}' has an unranged handler '{1}' and a ranged handler '{2}'; the unranged one would win for every build, making the ranged one dead",
        category: "HermesProxy.SourceGen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor MissingCodec = new(
        id: "HPSG006",
        title: "Packet has no codec",
        messageFormat: "System method '{0}' takes packet type '{1}', but no type '{2}' with a 'static void Read(ref SpanPacketReader, out {1})' was found",
        category: "HermesProxy.SourceGen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor BadSignature = new(
        id: "HPSG007",
        title: "Packet system has an unsupported signature",
        messageFormat: "System method '{0}' must be 'public static' and take '(in TPacket, in SessionContext)' or '(Opcode, in TPacket, in SessionContext)'; {1}",
        category: "HermesProxy.SourceGen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var cmsg = Collect(context, CmsgAttributeFullName);
        var smsg = Collect(context, SmsgAttributeFullName);

        context.RegisterSourceOutput(cmsg, static (ctx, handlers) =>
            EmitTable(ctx, handlers, "GeneratedCmsgDispatch", ModernVersionFullName,
                      "modern client → proxy (CMSG)"));

        context.RegisterSourceOutput(smsg, static (ctx, handlers) =>
            EmitTable(ctx, handlers, "GeneratedSmsgDispatch", LegacyVersionFullName,
                      "legacy emulator → proxy (SMSG)"));
    }

    private static IncrementalValueProvider<ImmutableArray<HandlerModel>> Collect(
        IncrementalGeneratorInitializationContext context, string attributeFullName)
    {
        return context.SyntaxProvider
            .ForAttributeWithMetadataName(
                attributeFullName,
                predicate: static (node, _) => true,
                transform: static (ctx, _) => Parse(ctx))
            .SelectMany(static (models, _) => models)
            .Collect();
    }

    /// One method can carry many attributes (movement handlers stack 30+), so one symbol yields
    /// one model per opcode. Each gets its own thunk, which is what lets the opcode reach a
    /// shape-B system as a JIT constant instead of being re-derived from the packet.
    private static ImmutableArray<HandlerModel> Parse(GeneratorAttributeSyntaxContext ctx)
    {
        if (ctx.TargetSymbol is not IMethodSymbol method)
            return ImmutableArray<HandlerModel>.Empty;

        var results = ImmutableArray.CreateBuilder<HandlerModel>();
        string methodName = method.ContainingType.ToDisplayString() + "." + method.Name;
        Location location = method.Locations.FirstOrDefault() ?? Location.None;

        string? shapeError = ValidateShape(method, out bool takesOpcode, out INamedTypeSymbol? packetType);
        string? codecName = null;
        if (shapeError is null && packetType is not null)
            codecName = ResolveCodec(ctx.SemanticModel.Compilation, packetType);

        foreach (var attr in ctx.Attributes)
        {
            string? opcodeName = OpcodeNameOf(attr);
            if (opcodeName is null)
                continue;

            results.Add(new HandlerModel(
                OpcodeName: opcodeName,
                MethodFullName: method.ContainingType.ToDisplayString() + "." + method.Name,
                MethodDisplay: methodName,
                PacketTypeFullName: packetType?.ToDisplayString(),
                CodecFullName: codecName,
                TakesOpcode: takesOpcode,
                AddedIn: NamedBuild(attr, "AddedIn"),
                RemovedIn: NamedBuild(attr, "RemovedIn"),
                ShapeError: shapeError,
                Location: location));
        }

        return results.ToImmutable();
    }

    /// <summary>Shape A is <c>(in TPacket, in SessionContext)</c>; shape B prefixes an Opcode.</summary>
    private static string? ValidateShape(IMethodSymbol method, out bool takesOpcode, out INamedTypeSymbol? packetType)
    {
        takesOpcode = false;
        packetType = null;

        if (!method.IsStatic)
            return "it is an instance method";
        if (method.DeclaredAccessibility != Accessibility.Public)
            return "it is not public";
        if (!method.ReturnsVoid)
            return "it does not return void";

        var ps = method.Parameters;
        if (ps.Length is not (2 or 3))
            return $"it takes {ps.Length} parameters";

        int packetIndex = 0;
        if (ps.Length == 3)
        {
            if (ps[0].Type.ToDisplayString() != "HermesProxy.World.Enums.Opcode" || ps[0].RefKind != RefKind.None)
                return "its first parameter must be a by-value Opcode";
            takesOpcode = true;
            packetIndex = 1;
        }

        if (ps[packetIndex].RefKind != RefKind.In)
            return $"the packet parameter must be passed 'in'";
        if (ps[packetIndex].Type is not INamedTypeSymbol pt || !pt.IsValueType)
            return "the packet parameter must be a struct";
        packetType = pt;

        var last = ps[ps.Length - 1];
        if (last.RefKind != RefKind.In || last.Type.ToDisplayString() != $"{DispatchNamespace}.SessionContext")
            return "the last parameter must be 'in SessionContext'";

        return null;
    }

    /// Codecs are found by convention — packet <c>N.Foo</c> means codec <c>N.FooCodec</c> — and
    /// then verified to exist, so the convention cannot fail silently.
    private static string? ResolveCodec(Compilation compilation, INamedTypeSymbol packetType)
    {
        string ns = packetType.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : packetType.ContainingNamespace.ToDisplayString() + ".";
        string codecMetadataName = ns + packetType.Name + "Codec";

        var codec = compilation.GetTypeByMetadataName(codecMetadataName);
        if (codec is null)
            return null;

        bool hasRead = codec.GetMembers("Read").OfType<IMethodSymbol>().Any(m =>
            m.IsStatic &&
            m.Parameters.Length == 2 &&
            m.Parameters[0].RefKind == RefKind.Ref &&
            m.Parameters[0].Type.ToDisplayString() == "Framework.IO.SpanPacketReader" &&
            m.Parameters[1].RefKind == RefKind.Out &&
            SymbolEqualityComparer.Default.Equals(m.Parameters[1].Type, packetType));

        return hasRead ? codec.ToDisplayString() : null;
    }

    private static string? OpcodeNameOf(AttributeData attr)
    {
        if (attr.ConstructorArguments.Length != 1)
            return null;
        var arg = attr.ConstructorArguments[0];
        // The enum member's *name* is emitted back out, so the generator never needs to know
        // the numeric value and no enum has to be mirrored here.
        if (arg.Type is not INamedTypeSymbol enumType || enumType.TypeKind != TypeKind.Enum)
            return null;
        return MemberNameForValue(enumType, arg.Value);
    }

    private static string? NamedBuild(AttributeData attr, string name)
    {
        foreach (var na in attr.NamedArguments)
        {
            if (na.Key != name)
                continue;
            if (na.Value.Type is not INamedTypeSymbol enumType)
                continue;
            string? member = MemberNameForValue(enumType, na.Value.Value);
            return member == "Zero" ? null : member;   // Zero is the unbounded sentinel
        }
        return null;
    }

    private static string? MemberNameForValue(INamedTypeSymbol enumType, object? value)
    {
        if (value is null)
            return null;
        foreach (var field in enumType.GetMembers().OfType<IFieldSymbol>())
        {
            if (field.IsConst && field.HasConstantValue && Equals(field.ConstantValue, value))
                return field.Name;
        }
        return null;
    }

    private static void EmitTable(
        SourceProductionContext ctx,
        ImmutableArray<HandlerModel> handlers,
        string className,
        string versionClass,
        string directionDescription)
    {
        var usable = new List<HandlerModel>();
        foreach (var h in handlers)
        {
            if (h.ShapeError is not null)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(BadSignature, h.Location, h.MethodDisplay, h.ShapeError));
                continue;
            }
            if (h.CodecFullName is null)
            {
                string packet = h.PacketTypeFullName ?? "?";
                ctx.ReportDiagnostic(Diagnostic.Create(MissingCodec, h.Location, h.MethodDisplay, packet, packet + "Codec"));
                continue;
            }
            usable.Add(h);
        }

        // Deterministic output: the snapshot test compares emitted source, so ordering must not
        // depend on the order Roslyn happened to visit syntax trees in.
        usable.Sort(static (a, b) =>
        {
            int byOpcode = string.CompareOrdinal(a.OpcodeName, b.OpcodeName);
            return byOpcode != 0 ? byOpcode : string.CompareOrdinal(a.MethodFullName, b.MethodFullName);
        });

        ReportClashes(ctx, usable);

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("//");
        sb.AppendLine($"// Inbound dispatch table for {directionDescription}.");
        sb.AppendLine("// Produced by HermesProxy.SourceGen.PacketDispatchGenerator from the [HandlesCmsg] /");
        sb.AppendLine("// [HandlesSmsg] attributes on the packet systems. Do not edit — edit the attributes.");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine($"namespace {DispatchNamespace};");
        sb.AppendLine();
        sb.AppendLine($"internal static unsafe class {className}");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>Indexed by (uint)Opcode. A null slot means the opcode is still handled");
        sb.AppendLine("    /// by the reflective registry, so the dispatch site falls through to it.</summary>");
        sb.Append("    private static readonly delegate*<ref ").Append(ReaderFullName)
          .Append(", in ").Append(ContextFullName).AppendLine(", void>[] _table = BuildTable();");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>Opcodes this table owns. The reflective registrar skips these, and a test");
        sb.AppendLine("    /// asserts the two registries stay disjoint.</summary>");
        sb.Append("    internal static readonly global::System.Collections.Frozen.FrozenSet<")
          .Append(OpcodeFullName).AppendLine("> ClaimedOpcodes = BuildClaimed();");
        sb.AppendLine();
        EmitGet(sb);
        EmitEnsureInitialized(sb, className);
        EmitThunks(sb, usable);
        EmitBuildTable(sb, usable, versionClass);
        EmitBuildClaimed(sb, usable);
        sb.AppendLine("}");

        ctx.AddSource(className + ".g.cs", sb.ToString());
    }

    private static void ReportClashes(SourceProductionContext ctx, List<HandlerModel> usable)
    {
        foreach (var group in usable.GroupBy(h => h.OpcodeName, StringComparer.Ordinal))
        {
            var all = group.ToList();
            if (all.Count < 2)
                continue;

            var unranged = all.Where(h => h.AddedIn is null && h.RemovedIn is null).ToList();
            var ranged = all.Where(h => h.AddedIn is not null || h.RemovedIn is not null).ToList();

            if (unranged.Count > 0 && ranged.Count > 0)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(RangedShadowedByUnranged, ranged[0].Location,
                    group.Key, unranged[0].MethodDisplay, ranged[0].MethodDisplay));
                continue;
            }

            // Two unranged handlers for one opcode always collide. Ranged pairs are only reported
            // when their intervals actually overlap, which needs build ordering the generator does
            // not have — so an identical [AddedIn, RemovedIn) pair is the case it can prove.
            if (unranged.Count > 1)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(OverlappingRanges, unranged[1].Location,
                    group.Key, unranged[0].MethodDisplay, unranged[1].MethodDisplay));
                continue;
            }

            for (int i = 1; i < ranged.Count; i++)
            {
                if (ranged[i].AddedIn == ranged[i - 1].AddedIn && ranged[i].RemovedIn == ranged[i - 1].RemovedIn)
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(OverlappingRanges, ranged[i].Location,
                        group.Key, ranged[i - 1].MethodDisplay, ranged[i].MethodDisplay));
                }
            }
        }
    }

    private static void EmitGet(StringBuilder sb)
    {
        sb.AppendLine("    /// <summary>The thunk for <paramref name=\"opcode\"/>, or null when unconverted.</summary>");
        sb.AppendLine("    [global::System.Runtime.CompilerServices.MethodImpl(");
        sb.AppendLine("        global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.Append("    internal static delegate*<ref ").Append(ReaderFullName)
          .Append(", in ").Append(ContextFullName).Append(", void> Get(").Append(OpcodeFullName).AppendLine(" opcode)");
        sb.AppendLine("    {");
        sb.AppendLine("        var table = _table;");
        sb.AppendLine("        uint index = (uint)opcode;");
        sb.AppendLine("        return index < (uint)table.Length ? table[index] : null;");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void EmitEnsureInitialized(StringBuilder sb, string className)
    {
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Forces the table's static constructor at a point of our choosing.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <remarks>");
        sb.AppendLine("    /// BuildTable reads the version statics, whose own initializers throw when");
        sb.AppendLine("    /// VersionBootstrap has not been assigned — and a type initializer that throws poisons");
        sb.AppendLine("    /// the type for the life of the process, reporting the wrong culprit ever after. Calling");
        sb.AppendLine("    /// this right after bootstrap keeps that failure at startup instead of on the first packet.");
        sb.AppendLine("    /// </remarks>");
        sb.AppendLine("    internal static void EnsureInitialized()");
        sb.AppendLine("    {");
        sb.AppendLine("        _ = _table.Length;");
        sb.AppendLine("        _ = ClaimedOpcodes.Count;");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void EmitThunks(StringBuilder sb, List<HandlerModel> usable)
    {
        if (usable.Count == 0)
        {
            sb.AppendLine("    // No systems carry the attribute yet — the table is empty and every opcode");
            sb.AppendLine("    // falls through to the reflective registry.");
            sb.AppendLine();
            return;
        }

        foreach (var h in usable)
        {
            sb.Append("    private static void ").Append(ThunkName(h))
              .Append("(ref ").Append(ReaderFullName).Append(" reader, in ").Append(ContextFullName).AppendLine(" ctx)");
            sb.AppendLine("    {");
            sb.Append("        global::").Append(h.CodecFullName).AppendLine(".Read(ref reader, out var packet);");
            sb.Append("        global::").Append(h.MethodFullName).Append('(');
            if (h.TakesOpcode)
                sb.Append(OpcodeFullName).Append('.').Append(h.OpcodeName).Append(", ");
            sb.AppendLine("in packet, in ctx);");
            sb.AppendLine("    }");
            sb.AppendLine();
        }
    }

    private static void EmitBuildTable(StringBuilder sb, List<HandlerModel> usable, string versionClass)
    {
        sb.Append("    private static delegate*<ref ").Append(ReaderFullName)
          .Append(", in ").Append(ContextFullName).AppendLine(", void>[] BuildTable()");
        sb.AppendLine("    {");

        if (usable.Count == 0)
        {
            // Array.Empty<T>() is unavailable: a function pointer type cannot be a generic
            // type argument. A zero-length array is equivalent here and allocates once.
            sb.Append("        return new delegate*<ref ").Append(ReaderFullName)
              .Append(", in ").Append(ContextFullName).AppendLine(", void>[0];");
            sb.AppendLine("    }");
            sb.AppendLine();
            return;
        }

        // Sized to the highest opcode actually claimed, not the highest that exists: a table sized
        // to the whole enum would be mostly null and touched cold on every dispatch.
        sb.Append("        var table = new delegate*<ref ").Append(ReaderFullName)
          .Append(", in ").Append(ContextFullName).AppendLine(", void>[MaxClaimedOpcode + 1];");
        sb.AppendLine();

        foreach (var group in usable.GroupBy(h => h.OpcodeName, StringComparer.Ordinal))
        {
            var all = group.ToList();
            string slot = $"table[(int){OpcodeFullName}.{group.Key}]";

            if (all.Count == 1 && all[0].AddedIn is null && all[0].RemovedIn is null)
            {
                sb.Append("        ").Append(slot).Append(" = &").Append(ThunkName(all[0])).AppendLine(";");
                continue;
            }

            sb.AppendLine("        {");
            bool first = true;
            foreach (var h in all)
            {
                string condition = BuildCondition(h, versionClass);
                sb.Append("            ").Append(first ? "if (" : "else if (").Append(condition).AppendLine(")");
                sb.Append("                ").Append(slot).Append(" = &").Append(ThunkName(h)).AppendLine(";");
                first = false;
            }
            sb.AppendLine("        }");
        }

        sb.AppendLine();
        sb.AppendLine("        return table;");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.Append("    private const int MaxClaimedOpcode = (int)").Append(OpcodeFullName).Append('.')
          .Append(usable.OrderByDescending(h => h.OpcodeName, StringComparer.Ordinal).First().OpcodeName)
          .AppendLine(";");
        sb.AppendLine();
    }

    /// The range is literal data, so the guard is real code the JIT sees — and because the version
    /// statics are `static readonly`, it is evaluated exactly once when the table is built.
    private static string BuildCondition(HandlerModel h, string versionClass)
    {
        const string cvb = "global::HermesProxy.Enums.ClientVersionBuild";
        var parts = new List<string>();
        if (h.AddedIn is not null)
            parts.Add($"{versionClass}.AddedInVersion({cvb}.{h.AddedIn})");
        if (h.RemovedIn is not null)
            parts.Add($"{versionClass}.RemovedInVersion({cvb}.{h.RemovedIn})");
        return parts.Count == 0 ? "true" : string.Join(" && ", parts);
    }

    private static void EmitBuildClaimed(StringBuilder sb, List<HandlerModel> usable)
    {
        sb.Append("    private static global::System.Collections.Frozen.FrozenSet<")
          .AppendLine($"{OpcodeFullName}> BuildClaimed()");
        sb.AppendLine("    {");

        if (usable.Count == 0)
        {
            sb.Append("        return global::System.Collections.Frozen.FrozenSet<")
              .Append(OpcodeFullName).AppendLine(">.Empty;");
            sb.AppendLine("    }");
            return;
        }

        sb.Append("        return global::System.Collections.Frozen.FrozenDictionary.ToFrozenSet(new ")
          .Append(OpcodeFullName).AppendLine("[]");
        sb.AppendLine("        {");
        foreach (var name in usable.Select(h => h.OpcodeName).Distinct(StringComparer.Ordinal))
            sb.Append("            ").Append(OpcodeFullName).Append('.').Append(name).AppendLine(",");
        sb.AppendLine("        });");
        sb.AppendLine("    }");
    }

    private static string ThunkName(HandlerModel h)
    {
        string method = h.MethodFullName.Replace('.', '_');
        return "Thunk_" + method + "_" + h.OpcodeName;
    }

    private sealed record HandlerModel(
        string OpcodeName,
        string MethodFullName,
        string MethodDisplay,
        string? PacketTypeFullName,
        string? CodecFullName,
        bool TakesOpcode,
        string? AddedIn,
        string? RemovedIn,
        string? ShapeError,
        Location Location);
}
