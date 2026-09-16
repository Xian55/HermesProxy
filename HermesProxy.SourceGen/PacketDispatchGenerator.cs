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
/// A null slot means no handler covers that opcode for the running build, and the dispatch site
/// logs "No handler for opcode" and drops the packet. During the migration a null slot meant
/// "not converted yet" and fell through to the reflective registries; those were deleted once
/// every handler carried an attribute, so there is no second path any more.
/// </para>
/// <para>
/// <b>Why a function-pointer array and not a switch.</b> A switch over ~800 sparse opcodes lowers
/// to a binary-search tree plus bucketed jump tables, in one method body the JIT must compile as a
/// unit on the first packet. The array is one bounds check, one load, one <c>calli</c> — and a null
/// slot is a free "unhandled" test. <c>delegate*[]</c> holds unmanaged pointers, so
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

    // The legacy side keeps its handlers as instance methods on WorldClient that read straight
    // off a WorldPacket. Converting 444 of those to record structs and codecs would rewrite
    // ~18,500 lines of translation logic for a path whose per-packet cost is already a dictionary
    // hash and a delegate invoke - so the SMSG table carries this second thunk shape instead, and
    // conversion there is an attribute with no body change.
    private const string WorldClientFullName = "HermesProxy.World.Client.WorldClient";
    private const string WorldPacketFullName = "HermesProxy.World.WorldPacket";
    private const string CodecAttributeFullName = "HermesProxy.World.Dispatch.PacketCodecAttribute";

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
        var codecs = CollectCodecs(context);

        context.RegisterSourceOutput(cmsg.Combine(codecs), static (ctx, pair) =>
            EmitTable(ctx, pair.Left, pair.Right, "GeneratedCmsgDispatch", ModernVersionFullName,
                      "modern client → proxy (CMSG)", legacyShape: false));

        context.RegisterSourceOutput(smsg.Combine(codecs), static (ctx, pair) =>
            EmitTable(ctx, pair.Left, pair.Right, "GeneratedSmsgDispatch", LegacyVersionFullName,
                      "legacy emulator → proxy (SMSG)", legacyShape: true));
    }

    /// Codecs that declare a build range. A packet with none of these keeps the convention
    /// lookup, so ranging is opt-in per packet rather than a new requirement on all of them.
    private static IncrementalValueProvider<ImmutableArray<CodecModel>> CollectCodecs(
        IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
            .ForAttributeWithMetadataName(
                CodecAttributeFullName,
                predicate: static (node, _) => true,
                transform: static (ctx, _) => ParseCodec(ctx))
            .Where(static m => m is not null)
            .Select(static (m, _) => m!)
            .Collect();
    }

    private static CodecModel? ParseCodec(GeneratorAttributeSyntaxContext ctx)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol codec)
            return null;

        var attr = ctx.Attributes[0];
        if (attr.ConstructorArguments.Length != 1)
            return null;
        if (attr.ConstructorArguments[0].Value is not INamedTypeSymbol packetType)
            return null;

        bool againstLegacy = false;
        foreach (var na in attr.NamedArguments)
        {
            if (na.Key == "AgainstLegacyVersion" && na.Value.Value is bool b)
                againstLegacy = b;
        }

        return new CodecModel(
            CodecFullName: codec.ToDisplayString(),
            PacketFullName: packetType.ToDisplayString(),
            AddedIn: NamedBuild(attr, "AddedIn"),
            RemovedIn: NamedBuild(attr, "RemovedIn"),
            AgainstLegacyVersion: againstLegacy,
            Location: codec.Locations.FirstOrDefault() ?? Location.None);
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

        bool legacyShape = ctx.Attributes.Length > 0
            && ctx.Attributes[0].AttributeClass?.ToDisplayString() == SmsgAttributeFullName;

        string? shapeError = legacyShape
            ? ValidateLegacyShape(method)
            : ValidateShape(method, out _, out _);

        bool takesOpcode = false;
        INamedTypeSymbol? packetType = null;
        if (!legacyShape)
            ValidateShape(method, out takesOpcode, out packetType);

        string? codecName = null;
        if (!legacyShape && shapeError is null && packetType is not null)
            codecName = ResolveCodec(ctx.SemanticModel.Compilation, packetType);

        foreach (var attr in ctx.Attributes)
        {
            var opcode = OpcodeOf(attr);
            if (opcode is null)
                continue;

            results.Add(new HandlerModel(
                OpcodeName: opcode.Value.Name,
                OpcodeValue: opcode.Value.Value,
                MethodFullName: method.ContainingType.ToDisplayString() + "." + method.Name,
                MethodDisplay: methodName,
                PacketTypeFullName: packetType?.ToDisplayString(),
                CodecFullName: codecName,
                TakesOpcode: takesOpcode,
                MethodName: method.Name,
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

    /// <summary>
    /// The legacy shape: <c>void HandleX(WorldPacket)</c> on WorldClient, exactly as the
    /// reflective registrar required.
    /// </summary>
    /// <remarks>
    /// Deliberately the same contract the registrar enforced at runtime - instance method, one
    /// WorldPacket parameter - so adding the attribute to an existing handler is the whole
    /// conversion. The checks the registrar used to make by logging at startup are made here by
    /// the compiler instead.
    /// </remarks>
    private static string? ValidateLegacyShape(IMethodSymbol method)
    {
        if (method.IsStatic)
            return "it is static; legacy SMSG handlers are instance methods on WorldClient";
        if (!method.ReturnsVoid)
            return "it does not return void";
        if (method.ContainingType.ToDisplayString() != WorldClientFullName)
            return $"it is not declared on {WorldClientFullName}";

        var ps = method.Parameters;
        if (ps.Length != 1)
            return $"it takes {ps.Length} parameters, expected one WorldPacket";
        if (ps[0].RefKind != RefKind.None || ps[0].Type.ToDisplayString() != WorldPacketFullName)
            return "its parameter must be a by-value WorldPacket";

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

    /// The member *name* is what gets emitted, so no enum is ever mirrored here. The value is
    /// carried only to size the table, which has to be the numerically largest claimed opcode.
    private static (string Name, long Value)? OpcodeOf(AttributeData attr)
    {
        if (attr.ConstructorArguments.Length != 1)
            return null;
        var arg = attr.ConstructorArguments[0];
        if (arg.Type is not INamedTypeSymbol enumType || enumType.TypeKind != TypeKind.Enum)
            return null;
        string? name = MemberNameForValue(enumType, arg.Value);
        if (name is null || arg.Value is null)
            return null;
        return (name, Convert.ToInt64(arg.Value));
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
        ImmutableArray<CodecModel> codecs,
        string className,
        string versionClass,
        string directionDescription,
        bool legacyShape)
    {
        // Ranged codecs, indexed by the packet they read.
        var rangedByPacket = new Dictionary<string, List<CodecModel>>(StringComparer.Ordinal);
        foreach (var c in codecs)
        {
            if (!rangedByPacket.TryGetValue(c.PacketFullName, out var list))
                rangedByPacket[c.PacketFullName] = list = new List<CodecModel>();
            list.Add(c);
        }

        var usable = new List<HandlerModel>();
        foreach (var h in handlers)
        {
            if (h.ShapeError is not null)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(BadSignature, h.Location, h.MethodDisplay, h.ShapeError));
                continue;
            }

            // Legacy handlers read straight off the WorldPacket, so there is no packet type to
            // find a codec for and none of the codec machinery below applies to them.
            if (legacyShape)
            {
                usable.Add(h);
                continue;
            }

            // A packet with declared codecs uses them, one table candidate each, chosen by the
            // build once at table-build time. Otherwise the convention lookup stands.
            if (h.PacketTypeFullName is not null &&
                rangedByPacket.TryGetValue(h.PacketTypeFullName, out var declared))
            {
                foreach (var c in declared)
                {
                    usable.Add(h with
                    {
                        CodecFullName = c.CodecFullName,
                        CodecAddedIn = c.AddedIn,
                        CodecRemovedIn = c.RemovedIn,
                        CodecAgainstLegacy = c.AgainstLegacyVersion,
                    });
                }
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
        // The codec is part of the key: one handler can yield several candidates for the same
        // opcode — one per ranged codec — and without it their order falls out of whatever order
        // Roslyn happened to visit syntax trees in. That makes the emitted source flap between
        // builds, which would turn the snapshot gate into noise and train everyone to accept it.
        usable.Sort(static (a, b) =>
        {
            int byOpcode = string.CompareOrdinal(a.OpcodeName, b.OpcodeName);
            if (byOpcode != 0)
                return byOpcode;

            int byMethod = string.CompareOrdinal(a.MethodFullName, b.MethodFullName);
            return byMethod != 0 ? byMethod : string.CompareOrdinal(a.CodecFullName, b.CodecFullName);
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
        sb.AppendLine("    /// <summary>Indexed by (uint)Opcode. A null slot means no handler covers the opcode for");
        sb.AppendLine("    /// the running build; the dispatch site logs it and drops the packet.</summary>");
        sb.Append("    private static readonly ").Append(DelegateType(legacyShape)).AppendLine("[] _table = BuildTable();");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>Opcodes this table owns. DispatchRegistryTests asserts each resolves to a");
        sb.AppendLine("    /// thunk and that the count never falls below what was converted.</summary>");
        sb.Append("    internal static readonly global::System.Collections.Frozen.FrozenSet<")
          .Append(OpcodeFullName).AppendLine("> ClaimedOpcodes = BuildClaimed();");
        sb.AppendLine();
        EmitGet(sb, legacyShape);
        EmitEnsureInitialized(sb, className);
        EmitThunks(sb, usable, legacyShape);
        EmitBuildTable(sb, usable, versionClass, legacyShape);
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

            // "Ranged" means the candidate is guarded by *something* — a handler range or a codec
            // range. A packet with two ranged codecs legitimately produces two candidates for one
            // opcode, and that must not read as a duplicate claim.
            static bool IsRanged(HandlerModel h)
                => h.AddedIn is not null || h.RemovedIn is not null
                || h.CodecAddedIn is not null || h.CodecRemovedIn is not null;

            var unranged = all.Where(h => !IsRanged(h)).ToList();
            var ranged = all.Where(IsRanged).ToList();

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
                if (ranged[i].AddedIn == ranged[i - 1].AddedIn &&
                    ranged[i].RemovedIn == ranged[i - 1].RemovedIn &&
                    ranged[i].CodecAddedIn == ranged[i - 1].CodecAddedIn &&
                    ranged[i].CodecRemovedIn == ranged[i - 1].CodecRemovedIn)
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(OverlappingRanges, ranged[i].Location,
                        group.Key, ranged[i - 1].MethodDisplay, ranged[i].MethodDisplay));
                }
            }
        }
    }

    /// <summary>The function-pointer type a table's slots hold.</summary>
    private static string DelegateType(bool legacyShape)
        => legacyShape
            ? $"delegate*<global::{WorldClientFullName}, global::{WorldPacketFullName}, void>"
            : $"delegate*<ref {ReaderFullName}, in {ContextFullName}, void>";

    private static void EmitGet(StringBuilder sb, bool legacyShape)
    {
        sb.AppendLine("    /// <summary>The thunk for <paramref name=\"opcode\"/>, or null when unconverted.</summary>");
        sb.AppendLine("    [global::System.Runtime.CompilerServices.MethodImpl(");
        sb.AppendLine("        global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.Append("    internal static ").Append(DelegateType(legacyShape))
          .Append(" Get(").Append(OpcodeFullName).AppendLine(" opcode)");
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

    private static void EmitThunks(StringBuilder sb, List<HandlerModel> usable, bool legacyShape)
    {
        if (usable.Count == 0)
        {
            sb.AppendLine("    // No systems carry the attribute — the table is empty and every opcode is");
            sb.AppendLine("    // unhandled.");
            sb.AppendLine();
            return;
        }

        foreach (var h in usable)
        {
            if (legacyShape)
            {
                // No codec and no packet object: the handler still parses inline off the
                // WorldPacket exactly as it did under the reflective registry. All this replaces
                // is the dictionary lookup and the delegate that reached it.
                sb.Append("    private static void ").Append(ThunkName(h))
                  .Append("(global::").Append(WorldClientFullName).Append(" client, global::")
                  .Append(WorldPacketFullName).AppendLine(" packet)");
                sb.AppendLine("    {");
                sb.Append("        client.").Append(h.MethodName).AppendLine("(packet);");
                sb.AppendLine("    }");
                sb.AppendLine();
                continue;
            }

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

    private static void EmitBuildTable(StringBuilder sb, List<HandlerModel> usable, string versionClass, bool legacyShape)
    {
        sb.Append("    private static ").Append(DelegateType(legacyShape)).AppendLine("[] BuildTable()");
        sb.AppendLine("    {");

        if (usable.Count == 0)
        {
            // Array.Empty<T>() is unavailable: a function pointer type cannot be a generic
            // type argument. A zero-length array is equivalent here and allocates once.
            sb.Append("        return new ").Append(DelegateType(legacyShape)).AppendLine("[0];");
            sb.AppendLine("    }");
            sb.AppendLine();
            return;
        }

        // Sized to the highest opcode actually claimed, not the highest that exists: a table sized
        // to the whole enum would be mostly null and touched cold on every dispatch.
        sb.Append("        var table = new ").Append(DelegateType(legacyShape)).AppendLine("[MaxClaimedOpcode + 1];");
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

        // Sized by the numerically largest claimed opcode. Ordering by name would only agree
        // with that while the universal enum stays alphabetical, which is not a property worth
        // depending on — a member added out of order would size the table short.
        string maxOpcode = usable.OrderByDescending(h => h.OpcodeValue).First().OpcodeName;
        sb.Append("    private const int MaxClaimedOpcode = (int)").Append(OpcodeFullName).Append('.')
          .Append(maxOpcode).AppendLine(";");
        sb.AppendLine();
    }

    /// The range is literal data, so the guard is real code the JIT sees — and because the version
    /// statics are `static readonly`, it is evaluated exactly once when the table is built.
    private static string BuildCondition(HandlerModel h, string versionClass)
    {
        var parts = new List<string>();

        if (h.AddedIn is not null)
            parts.Add(Added(versionClass, h.AddedIn));
        if (h.RemovedIn is not null)
            parts.Add(Removed(versionClass, h.RemovedIn));

        // A packet's *shape* is chosen by whichever side's build produced it, which is not always
        // the side the handler is registered on: a legacy SMSG's layout follows the server build,
        // while a modern CMSG's follows the client build.
        string codecVersion = h.CodecAgainstLegacy ? LegacyVersionFullName : versionClass;
        if (h.CodecAddedIn is not null)
            parts.Add(Added(codecVersion, h.CodecAddedIn));
        if (h.CodecRemovedIn is not null)
            parts.Add(Removed(codecVersion, h.CodecRemovedIn));

        return parts.Count == 0 ? "true" : string.Join(" && ", parts);
    }

    /// <summary>
    /// Emits a lower-bound test against <paramref name="buildName"/>.
    /// </summary>
    /// <remarks>
    /// On the modern axis this must be the expansion/major/minor overload, never the raw-build one.
    /// ClientVersionBuild is valued by build number, and the modern side spans two release lines —
    /// a Classic Era client (1.14.2 = 42597) compared against a Classic build (3.4.3 = 54261) is a
    /// cross-branch comparison whose answer is meaningless. It happens to come out right for this
    /// pair and would not for the next one. The expansion/major/minor form is parsed from the enum
    /// *name*, so it stays correct across lines. VersionChecker.AssertComparableBranch catches the
    /// raw form in DEBUG, and did catch this generator emitting it.
    /// <para>
    /// The legacy axis keeps the raw-build form: LegacyVersion has no triple overload, and every
    /// supported legacy build is on the original line, which ClientBranchTests asserts.
    /// </para>
    /// </remarks>
    private static string Added(string versionClass, string buildName)
    {
        if (versionClass == LegacyVersionFullName)
            return $"{versionClass}.AddedInVersion(global::HermesProxy.Enums.ClientVersionBuild.{buildName})";

        var (expansion, major, minor) = ParseVersion(buildName);
        return $"{versionClass}.AddedInVersion({expansion}, {major}, {minor})";
    }

    /// <summary>Upper bound, exclusive. There is no three-byte RemovedInVersion, so negate.</summary>
    private static string Removed(string versionClass, string buildName)
    {
        if (versionClass == LegacyVersionFullName)
            return $"{versionClass}.RemovedInVersion(global::HermesProxy.Enums.ClientVersionBuild.{buildName})";

        var (expansion, major, minor) = ParseVersion(buildName);
        return $"!{versionClass}.AddedInVersion({expansion}, {major}, {minor})";
    }

    /// Parses "V3_4_3_54261" into (3, 4, 3). The minor segment can carry a patch letter
    /// ("V3_0_8a_9506"), which is stripped — the same rule VersionChecker's own parser uses.
    private static (int Expansion, int Major, int Minor) ParseVersion(string buildName)
    {
        string[] parts = buildName.TrimStart('V').Split('_');
        int expansion = int.Parse(parts[0]);
        int major = int.Parse(parts[1]);

        string minorText = parts[2];
        int end = minorText.Length;
        while (end > 0 && !char.IsDigit(minorText[end - 1]))
            end--;

        return (expansion, major, int.Parse(minorText.Substring(0, end)));
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

        sb.Append("        return global::System.Collections.Frozen.FrozenSet.ToFrozenSet(new ")
          .Append(OpcodeFullName).AppendLine("[]");
        sb.AppendLine("        {");
        foreach (var name in usable.Select(h => h.OpcodeName).Distinct(StringComparer.Ordinal))
            sb.Append("            ").Append(OpcodeFullName).Append('.').Append(name).AppendLine(",");
        sb.AppendLine("        });");
        sb.AppendLine("    }");
    }

    /// One handler can now produce several table candidates — one per ranged codec — so the
    /// codec has to be part of the name or they collide.
    private static string ThunkName(HandlerModel h)
    {
        string method = h.MethodFullName.Replace('.', '_');
        string name = "Thunk_" + method + "_" + h.OpcodeName;

        if (h.CodecAddedIn is not null || h.CodecRemovedIn is not null)
        {
            int lastDot = h.CodecFullName!.LastIndexOf('.');
            name += "_" + (lastDot >= 0 ? h.CodecFullName.Substring(lastDot + 1) : h.CodecFullName);
        }

        return name;
    }

    private sealed record HandlerModel(
        string OpcodeName,
        long OpcodeValue,
        string MethodFullName,
        string MethodDisplay,
        string? PacketTypeFullName,
        string? CodecFullName,
        bool TakesOpcode,
        string? AddedIn,
        string? RemovedIn,
        string MethodName,
        string? ShapeError,
        Location Location)
    {
        /// Set when the codec came from a [PacketCodec] range rather than the convention lookup.
        public string? CodecAddedIn { get; init; }
        public string? CodecRemovedIn { get; init; }
        public bool CodecAgainstLegacy { get; init; }
    }

    private sealed record CodecModel(
        string CodecFullName,
        string PacketFullName,
        string? AddedIn,
        string? RemovedIn,
        bool AgainstLegacyVersion,
        Location Location);
}
