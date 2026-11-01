using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace HermesProxy.SourceGen;

/// <summary>
/// Emits <c>ConfigReference.CreateSettings()</c>: one entry for every setting of every class marked
/// <c>[ConfigSection]</c>, which <c>--help</c> and docs/configuration.md are rendered from.
/// </summary>
/// <remarks>
/// <para>
/// A generator can only add C#, so it cannot write the page. <c>ConfigReference</c> renders it at
/// run time, and a test holds the committed file to that output.
/// </para>
/// <para>
/// Defaults are read in the emitted code, from a new instance of the options class, rather than
/// from the initializer's syntax here. Whatever the initializer is, the value listed is the value
/// the proxy gets.
/// </para>
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class ConfigReferenceGenerator : IIncrementalGenerator
{
    private const string SectionAttributeFullName = "HermesProxy.Configuration.ConfigSectionAttribute";
    private const string SettingAttributeFullName = "HermesProxy.Configuration.SettingAttribute";
    private const string SettingInfoFullName = "global::HermesProxy.Configuration.SettingInfo";

    private static readonly DiagnosticDescriptor HPSG010_UndescribedSetting = new(
        id: "HPSG010",
        title: "Setting has no description",
        messageFormat: "'{0}.{1}' can be set from outside but has no [Setting] description, so --help and docs/configuration.md cannot list it. Add one, or give the property an internal setter if the proxy derives it.",
        category: "HermesProxy.SourceGen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Both transforms return plain values, so the incremental cache compares by value and an
        // edit elsewhere in the project emits nothing.
        var sections = context.SyntaxProvider.ForAttributeWithMetadataName(
                SectionAttributeFullName,
                static (node, _) => node is ClassDeclarationSyntax,
                static (ctx, _) => ReadSection((INamedTypeSymbol)ctx.TargetSymbol, ctx.Attributes[0]))
            .Collect();

        context.RegisterSourceOutput(sections, static (ctx, s) =>
        {
            if (!s.IsEmpty)
                ctx.AddSource("ConfigReference.g.cs", Emit(s));
        });

        var undescribed = context.SyntaxProvider.ForAttributeWithMetadataName(
                SectionAttributeFullName,
                static (node, _) => node is ClassDeclarationSyntax,
                static (ctx, _) => FindUndescribed((INamedTypeSymbol)ctx.TargetSymbol))
            .SelectMany(static (gaps, _) => gaps);

        context.RegisterSourceOutput(undescribed, static (ctx, gap) => ctx.ReportDiagnostic(Diagnostic.Create(
            HPSG010_UndescribedSetting,
            Location.Create(gap.Path, gap.Span, gap.Lines),
            gap.Section,
            gap.Key)));
    }

    /// <summary>The binder fills public setters and nothing else, so those are the settings.</summary>
    private static IEnumerable<IPropertySymbol> SettingsOf(INamedTypeSymbol type)
        => type.GetMembers().OfType<IPropertySymbol>().Where(static p =>
            !p.IsStatic
            && p.DeclaredAccessibility == Accessibility.Public
            && p.SetMethod is { DeclaredAccessibility: Accessibility.Public, IsInitOnly: false });

    private static string? DescriptionOf(IPropertySymbol property)
        => property.GetAttributes()
            .FirstOrDefault(static a => a.AttributeClass?.ToDisplayString() == SettingAttributeFullName)
            ?.ConstructorArguments[0].Value as string;

    private static Section ReadSection(INamedTypeSymbol type, AttributeData attribute)
    {
        string typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string sectionLiteral = SymbolDisplay.FormatLiteral(type.Name, quote: true);

        var entries = new StringBuilder();
        foreach (var property in SettingsOf(type))
        {
            if (DescriptionOf(property) is not string description)
                continue;

            entries.Append("            new(").Append(sectionLiteral)
                .Append(", ").Append(SymbolDisplay.FormatLiteral(property.Name, quote: true))
                .Append(", Format(new ").Append(typeName).Append("().").Append(property.Name).Append(')')
                .Append(", ").Append(SymbolDisplay.FormatLiteral(description, quote: true))
                .AppendLine("),");
        }

        return new Section((int)attribute.ConstructorArguments[0].Value!, entries.ToString());
    }

    private static ImmutableArray<UndescribedSetting> FindUndescribed(INamedTypeSymbol type)
    {
        var gaps = ImmutableArray.CreateBuilder<UndescribedSetting>();
        foreach (var property in SettingsOf(type))
        {
            if (DescriptionOf(property) is not null || property.Locations.FirstOrDefault() is not { SourceTree: not null } location)
                continue;

            gaps.Add(new UndescribedSetting(
                type.Name,
                property.Name,
                location.SourceTree.FilePath,
                location.SourceSpan,
                location.GetLineSpan().Span));
        }

        return gaps.ToImmutable();
    }

    private static string Emit(ImmutableArray<Section> sections)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("namespace HermesProxy.Configuration;");
        sb.AppendLine();
        sb.AppendLine("internal static partial class ConfigReference");
        sb.AppendLine("{");
        sb.Append("    private static partial ").Append(SettingInfoFullName).AppendLine("[] CreateSettings()");
        sb.AppendLine("    {");
        sb.AppendLine("        return");
        sb.AppendLine("        [");
        foreach (var section in sections.OrderBy(static s => s.Order))
            sb.Append(section.Entries);
        sb.AppendLine("        ];");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private sealed record Section(int Order, string Entries);

    private sealed record UndescribedSetting(string Section, string Key, string Path, TextSpan Span, LinePositionSpan Lines);
}
