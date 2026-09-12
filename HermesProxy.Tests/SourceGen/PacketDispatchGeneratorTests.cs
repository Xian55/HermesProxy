using System;
using System.IO;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace HermesProxy.Tests.SourceGen;

/// <summary>
/// Snapshots the two dispatch tables <c>PacketDispatchGenerator</c> emits.
/// </summary>
/// <remarks>
/// <para>
/// These pin the <i>shape</i> of the generated dispatch — the table type, the lookup, the
/// coexistence fallback, the claimed-opcode set — so that a slice which converts a handful of
/// opcodes shows up as a handful of added thunks and table slots, and nothing else. The value is
/// in the diff being small and readable: this is generated code that runs on every inbound
/// packet, and a change to its framing is far easier to review here than to reason about from
/// the attributes.
/// </para>
/// <para>
/// At slice 1 both tables are empty by design — no system carries <c>[HandlesCmsg]</c> or
/// <c>[HandlesSmsg]</c> yet, so every opcode still falls through to the reflective registry and
/// behaviour is provably unchanged. That empty table is the baseline every later slice is read
/// against.
/// </para>
/// <para>
/// Accepting a change: confirm the diff contains only what you meant, with
/// <c>diff --strip-trailing-cr &lt;verified&gt;.txt &lt;received&gt;.txt | grep '^[&lt;&gt;]'</c>, then edit the
/// <c>.verified.txt</c> <b>in place</b>. The snapshots are UTF-8 with BOM and CRLF; moving the
/// <c>.received.txt</c> over them rewrites the line endings and turns a small review into a
/// whole-file one.
/// </para>
/// </remarks>
public class PacketDispatchGeneratorTests
{
    [Fact]
    public Task GeneratedCmsgDispatch()
        => Verifier.Verify(ReadEmitted("GeneratedCmsgDispatch.g.cs"), extension: "txt");

    [Fact]
    public Task GeneratedSmsgDispatch()
        => Verifier.Verify(ReadEmitted("GeneratedSmsgDispatch.g.cs"), extension: "txt");

    private static string ReadEmitted(string fileName)
    {
        var path = Path.Combine(
            FindRepoRoot(),
            "HermesProxy", "obj", "Generated",
            "HermesProxy.SourceGen", "HermesProxy.SourceGen.PacketDispatchGenerator", fileName);

        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"Generated file not found at '{path}'. Ensure HermesProxy.csproj has " +
                "<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles> and has been built " +
                "against this branch.", path);

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
