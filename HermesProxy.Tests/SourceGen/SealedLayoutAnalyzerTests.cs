using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HermesProxy.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace HermesProxy.Tests.SourceGen;

/// <summary>
/// <see cref="SealedLayoutAnalyzer"/> (HPSG009): a class deriving from <c>ServerPacketLayout&lt;T&gt;</c>
/// has to be sealed.
/// </summary>
public class SealedLayoutAnalyzerTests
{
    private const string LayoutBase = """
        namespace HermesProxy.World.Server.Packets
        {
            public abstract class ServerPacket { }
            public sealed class GossipPOI : ServerPacket { }
            public abstract class ServerPacketLayout<TPacket> where TPacket : ServerPacket { }
        }
        """;

    [Fact]
    public async Task SealedLayout_IsAccepted()
    {
        var diagnostics = await Analyze("""
            namespace HermesProxy.World.Server.Packets
            {
                internal sealed class FlatLayout : ServerPacketLayout<GossipPOI> { }
            }
            """);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task UnsealedLayout_IsAnError()
    {
        var diagnostics = await Analyze("""
            namespace HermesProxy.World.Server.Packets
            {
                internal class FlatLayout : ServerPacketLayout<GossipPOI> { }
            }
            """);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("HPSG009", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal("'FlatLayout' derives from ServerPacketLayout<GossipPOI> and must be sealed", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AbstractIntermediate_AndItsUnsealedSubclass_AreBothErrors()
    {
        var diagnostics = await Analyze("""
            namespace HermesProxy.World.Server.Packets
            {
                internal abstract class SharedLayout : ServerPacketLayout<GossipPOI> { }
                internal class FlatLayout : SharedLayout { }
                internal sealed class RetailLayout : SharedLayout { }
            }
            """);

        Assert.Equal(["'FlatLayout'", "'SharedLayout'"],
            diagnostics.Select(d => d.GetMessage().Split(' ')[0]).Order());
    }

    [Fact]
    public async Task UnrelatedClass_IsIgnored()
    {
        var diagnostics = await Analyze("""
            namespace HermesProxy.World.Server.Packets
            {
                internal class Helper { }
            }
            """);

        Assert.Empty(diagnostics);
    }

    private static async Task<ImmutableArray<Diagnostic>> Analyze(string source)
    {
        string runtimeDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Where(path => Path.GetDirectoryName(path) == runtimeDirectory)
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create(
            nameof(SealedLayoutAnalyzerTests),
            [CSharpSyntaxTree.ParseText(LayoutBase), CSharpSyntaxTree.ParseText(source)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Assert.Empty(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        return await compilation
            .WithAnalyzers([new SealedLayoutAnalyzer()])
            .GetAnalyzerDiagnosticsAsync();
    }
}
