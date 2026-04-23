using System;
using System.IO;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace HermesProxy.Tests.SourceGen;

/// <summary>
/// Snapshot tests for <c>HermesProxy.SourceGen.ObjectUpdateBuilderGenerator</c>'s emitted
/// <c>.g.cs</c> files. The generator runs as an Analyzer on HermesProxy.csproj; its output
/// lands under <c>HermesProxy/obj/Generated/HermesProxy.SourceGen/.../</c> at build time
/// (via <c>EmitCompilerGeneratedFiles=true</c>). Verify compares each emitted file to a
/// committed <c>.verified.txt</c> snapshot in the same directory as this test.
///
/// When a snapshot mismatches, Verify writes a <c>.received.txt</c> next to the
/// <c>.verified.txt</c> and fails with a diff. To accept the new output: replace the
/// <c>.verified.txt</c> with the <c>.received.txt</c> and re-run.
/// </summary>
public class ObjectUpdateBuilderGeneratorTests
{
    [Fact]
    public Task WriteCreateObjectData_V3_4_3_54261()
    {
        var emitted = ReadEmitted(
            generator: "HermesProxy.SourceGen.ObjectUpdateBuilderGenerator",
            fileName: "V3_4_3_54261.ObjectUpdateBuilder.g.cs");

        // Use extension `txt` so the committed `.verified.txt` doesn't land in MSBuild's C#
        // compile glob — the snapshot is source text for comparison, not a file we compile.
        return Verifier.Verify(emitted, extension: "txt");
    }

    // The compiler places generated files at:
    //   {repoRoot}/HermesProxy/obj/Generated/HermesProxy.SourceGen/{generatorFullName}/{fileName}
    // Walk up from the test runtime dir (HermesProxy.Tests/bin/{cfg}/{tfm}/) to the repo root,
    // then down to the generated file.
    private static string ReadEmitted(string generator, string fileName)
    {
        var repoRoot = FindRepoRoot();
        var path = Path.Combine(
            repoRoot,
            "HermesProxy", "obj", "Generated",
            "HermesProxy.SourceGen", generator, fileName);

        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"Generated file not found at '{path}'. " +
                $"Ensure HermesProxy.csproj has <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles> " +
                $"and has been built against this branch.", path);

        return File.ReadAllText(path);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "HermesProxy.sln")))
                return dir.FullName;
            dir = dir.Parent;
        }
        throw new InvalidOperationException(
            $"Couldn't locate HermesProxy.sln walking up from '{AppContext.BaseDirectory}'.");
    }
}
