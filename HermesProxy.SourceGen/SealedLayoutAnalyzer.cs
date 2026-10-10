using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace HermesProxy.SourceGen;

/// <summary>
/// Every class deriving from <c>ServerPacketLayout&lt;TPacket&gt;</c> must be <c>sealed</c>.
/// </summary>
/// <remarks>
/// A packet keeps its layout in a static readonly field typed as the abstract base, and the JIT
/// devirtualises the call through it only when the object's class cannot have subclasses. Sealed
/// also keeps one layout one type: a layout that varies again is a new layout with its own range,
/// not a subclass of the old one. Code two layouts share goes in a static helper.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SealedLayoutAnalyzer : DiagnosticAnalyzer
{
    private const string LayoutBaseMetadataName = "HermesProxy.World.Server.Packets.ServerPacketLayout`1";

    private static readonly DiagnosticDescriptor LayoutNotSealed = new(
        id: "HPSG009",
        title: "Server packet layout must be sealed",
        messageFormat: "'{0}' derives from ServerPacketLayout<{1}> and must be sealed",
        category: "HermesProxy.SourceGen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Each wire layout is one sealed type: the packet's static readonly layout field is only devirtualised for a sealed class, and a variant is a new layout with its own build range rather than a subclass.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(LayoutNotSealed);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            var layoutBase = start.Compilation.GetTypeByMetadataName(LayoutBaseMetadataName);
            if (layoutBase is null)
                return;

            start.RegisterSymbolAction(symbol => Check(symbol, layoutBase), SymbolKind.NamedType);
        });
    }

    private static void Check(SymbolAnalysisContext context, INamedTypeSymbol layoutBase)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (type.TypeKind != TypeKind.Class || type.IsSealed || SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, layoutBase))
            return;

        for (var baseType = type.BaseType; baseType is not null; baseType = baseType.BaseType)
        {
            if (!SymbolEqualityComparer.Default.Equals(baseType.OriginalDefinition, layoutBase))
                continue;

            foreach (var location in type.Locations)
                context.ReportDiagnostic(Diagnostic.Create(LayoutNotSealed, location, type.Name, baseType.TypeArguments[0].Name));
            return;
        }
    }
}
