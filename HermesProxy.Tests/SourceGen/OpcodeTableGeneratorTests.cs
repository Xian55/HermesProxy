using System;
using System.IO;
using System.Linq;
using System.Reflection;
using HermesProxy.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace HermesProxy.Tests.SourceGen;

/// <summary>
/// Runs <see cref="OpcodeTableGenerator"/> over a small compilation of its own, compiles what it
/// emits and reads the tables back.
/// </summary>
/// <remarks>
/// The opcode values are the real ones (TrinityCore's wotlk_classic and cata_classic Opcodes.h).
/// Every build HermesProxy has a table for today has 16-bit opcodes, so the real tables only ever
/// exercise group 0. The 4.4.2-style enum here carries its group in the upper 16 bits, the way
/// clients from 4.4.0 do: it is what proves the per-group arrays and the 4-byte opcode size.
/// </remarks>
public class OpcodeTableGeneratorTests
{
    private const string Source = """
        namespace HermesProxy.Enums
        {
            public enum ClientVersionBuild { Zero = 0, V3_4_3_54261 = 54261, V4_4_2_60895 = 60895 }
        }

        namespace HermesProxy.World.Enums
        {
            public enum Opcode : uint
            {
                MSG_NULL_ACTION = 0,
                CMSG_PING = 1,
                SMSG_PONG = 2,
                SMSG_COMPRESSED_PACKET = 3,
                SMSG_AUTH_CHALLENGE = 4,
                SMSG_UPDATE_OBJECT = 5,
            }
        }

        namespace HermesProxy.World.Enums.V3_4_3_54261
        {
            public enum Opcode : uint
            {
                MSG_NULL_ACTION = 0,
                CMSG_PING = 0x3768,
                SMSG_PONG = 0x304E,
                SMSG_COMPRESSED_PACKET = 0x3052,
                SMSG_AUTH_CHALLENGE = 0x3048,
                SMSG_UPDATE_OBJECT = 0x27CB,
            }
        }

        namespace HermesProxy.World.Enums.V4_4_2_60895
        {
            public enum Opcode : uint
            {
                MSG_NULL_ACTION = 0,
                CMSG_PING = 0x3A0004,
                SMSG_PONG = 0x420006,
                SMSG_COMPRESSED_PACKET = 0x42000A,
                SMSG_AUTH_CHALLENGE = 0x420000,
                SMSG_UPDATE_OBJECT = 0x4B0000,
            }
        }
        """;

    private static readonly Lazy<Assembly> Generated = new(Compile);

    [Fact]
    public void SixteenBitBuild_HasOneGroupAndTwoByteOpcodes()
    {
        var (groups, universalToCurrent, opcodeSize) = Tables("V3_4_3_54261");

        Assert.Equal(2, opcodeSize);
        Assert.Single(groups);
        Assert.Equal(0x3769, groups[0].Length);
        Assert.Equal(1u, groups[0][0x3768]);
        Assert.Equal(5u, groups[0][0x27CB]);
        Assert.Equal(0x3052u, universalToCurrent[3]);
    }

    [Fact]
    public void GroupedBuild_SplitsTablesByUpperBitsAndUsesFourByteOpcodes()
    {
        var (groups, universalToCurrent, opcodeSize) = Tables("V4_4_2_60895");

        Assert.Equal(4, opcodeSize);
        Assert.Equal(0x4C, groups.Length);

        // Each group is sized by its own highest index, not by the whole opcode.
        Assert.Equal(5, groups[0x3A].Length);
        Assert.Equal(1u, groups[0x3A][4]);
        Assert.Equal(11, groups[0x42].Length);
        Assert.Equal(4u, groups[0x42][0]);
        Assert.Equal(2u, groups[0x42][6]);
        Assert.Equal(3u, groups[0x42][0xA]);
        Assert.Single(groups[0x4B]);
        Assert.Equal(5u, groups[0x4B][0]);

        // Groups with no opcode are empty, so a lookup there bounds-checks to "not found".
        Assert.Empty(groups[0x00]);
        Assert.Empty(groups[0x43]);

        // The reverse table keeps the whole value, group included.
        Assert.Equal(0x3A0004u, universalToCurrent[1]);
        Assert.Equal(0x42000Au, universalToCurrent[3]);
    }

    [Fact]
    public void UnknownBuild_ReturnsFalse()
    {
        var assembly = Generated.Value;
        var buildEnum = assembly.GetType("HermesProxy.Enums.ClientVersionBuild")!;
        var tryGet = assembly.GetType("HermesProxy.GeneratedOpcodeTables")!
            .GetMethod("TryGet", BindingFlags.Public | BindingFlags.Static)!;
        object?[] args = [Enum.ToObject(buildEnum, 0), null, null, null];

        Assert.False((bool)tryGet.Invoke(null, args)!);
        Assert.Equal(0, (int)args[3]!);
    }

    /// <summary>The tables for <paramref name="build"/>, with every opcode as its raw value.</summary>
    private static (uint[][] Groups, uint[] UniversalToCurrent, int OpcodeSize) Tables(string build)
    {
        var assembly = Generated.Value;
        var buildEnum = assembly.GetType("HermesProxy.Enums.ClientVersionBuild")!;
        var tryGet = assembly.GetType("HermesProxy.GeneratedOpcodeTables")!
            .GetMethod("TryGet", BindingFlags.Public | BindingFlags.Static)!;

        object?[] args = [Enum.Parse(buildEnum, build), null, null, null];
        Assert.True((bool)tryGet.Invoke(null, args)!);

        var groups = ((Array)args[1]!).Cast<Array>()
            .Select(group => group.Cast<object>().Select(Convert.ToUInt32).ToArray())
            .ToArray();
        return (groups, (uint[])args[2]!, (int)args[3]!);
    }

    private static Assembly Compile()
    {
        // The runtime's own assemblies only: the test host's list also holds HermesProxy.dll, whose
        // real per-build enums the generator would pick up next to these.
        string runtimeDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Where(path => Path.GetDirectoryName(path) == runtimeDirectory)
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create(
            "OpcodeTableGeneratorTests",
            [CSharpSyntaxTree.ParseText(Source)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        CSharpGeneratorDriver.Create(new OpcodeTableGenerator())
            .RunGeneratorsAndUpdateCompilation(compilation, out var output, out var generatorDiagnostics);
        Assert.Empty(generatorDiagnostics);

        using var image = new MemoryStream();
        var result = output.Emit(image);
        Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        return Assembly.Load(image.ToArray());
    }
}
