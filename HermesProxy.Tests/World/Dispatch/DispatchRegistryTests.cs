using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server;
using Xunit;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Guards the generated dispatch tables now that they are the only inbound registry.
/// </summary>
/// <remarks>
/// The reflective registries these replaced were queried by opcode count, which fails open: a
/// handler that silently stopped being registered just made the set smaller and nothing noticed.
/// These assert properties that get stricter rather than weaker — every claimed opcode resolves
/// to a real thunk, every system has the shape the generator requires, and the claimed counts
/// never fall below what was converted.
/// </remarks>
public class DispatchRegistryTests
{
    static DispatchRegistryTests()
    {
        // Reflecting over the handler hosts touches the version statics, which refuse to
        // initialise until a build is chosen — and a throwing type initializer poisons the type
        // for the whole process.
        if (VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            VersionBootstrap.LegacyBuild = ClientVersionBuild.V3_3_5a_12340;
        if (VersionBootstrap.ModernBuild == ClientVersionBuild.Zero)
            VersionBootstrap.ModernBuild = ClientVersionBuild.V3_4_3_54261;
    }

    private static IEnumerable<MethodInfo> SystemsWith<TAttribute>() where TAttribute : Attribute =>
        typeof(SessionContext).Assembly.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            .Where(m => m.GetCustomAttributes<TAttribute>().Any());

    [Fact]
    public void EveryCmsgSystemHasTheShapeTheGeneratorRequires()
        => AssertSystemShapes(SystemsWith<HandlesCmsgAttribute>());

    [Fact]
    public void EverySmsgSystemHasTheShapeTheGeneratorRequires()
        => AssertSystemShapes(SystemsWith<HandlesSmsgAttribute>());

    /// <summary>
    /// The generator reports HPSG006/HPSG007 for these, so a violation already fails the build.
    /// Asserting them here too means the rule is stated where a reader looks for it, and that a
    /// generator regression which silently drops a handler is caught rather than shipped.
    /// </summary>
    private static void AssertSystemShapes(IEnumerable<MethodInfo> systems)
    {
        var broken = new List<string>();

        foreach (var m in systems)
        {
            string name = $"{m.DeclaringType?.FullName}.{m.Name}";

            if (!m.IsStatic || !m.IsPublic)
            {
                broken.Add($"{name}: must be public static");
                continue;
            }
            if (m.ReturnType != typeof(void))
            {
                broken.Add($"{name}: must return void");
                continue;
            }

            var ps = m.GetParameters();
            if (ps.Length is not (2 or 3))
            {
                broken.Add($"{name}: takes {ps.Length} parameters, expected 2 or 3");
                continue;
            }

            int packetIndex = 0;
            if (ps.Length == 3)
            {
                if (ps[0].ParameterType != typeof(Opcode))
                {
                    broken.Add($"{name}: 3-parameter form must lead with Opcode");
                    continue;
                }
                packetIndex = 1;
            }

            Type packetType = ps[packetIndex].ParameterType;
            if (!packetType.IsByRef || !ps[packetIndex].IsIn)
            {
                broken.Add($"{name}: packet parameter must be passed 'in'");
                continue;
            }

            Type packet = packetType.GetElementType()!;
            if (!packet.IsValueType)
            {
                broken.Add($"{name}: packet type {packet.Name} must be a struct");
                continue;
            }

            var last = ps[^1];
            if (last.ParameterType.GetElementType() != typeof(SessionContext) || !last.IsIn)
            {
                broken.Add($"{name}: last parameter must be 'in SessionContext'");
                continue;
            }

            // A packet either declares ranged codecs with [PacketCodec] — one per build range —
            // or relies on the convention lookup. The generator accepts both and errors when
            // neither is present; assert the same rule so it is discoverable from the tests.
            bool hasRangedCodec = packet.Assembly.GetTypes().Any(t =>
                t.GetCustomAttributes<PacketCodecAttribute>()
                 .Any(a => a.PacketType == packet));

            if (!hasRangedCodec)
            {
                string codecName = packet.FullName + "Codec";
                if (packet.Assembly.GetType(codecName) is null)
                    broken.Add($"{name}: no [PacketCodec] for {packet.Name} and no convention codec {codecName}");
            }
        }

        Assert.True(broken.Count == 0, string.Join(Environment.NewLine, broken));
    }

    [Fact]
    public void ConvertedOpcodesOnlyEverGrow()
    {
        // A converted opcode must never quietly fall back to the reflective path: that would be
        // a silent regression to the allocating dispatch, with no test failing. Every opcode the
        // generated table claims has to resolve to a real thunk.
        AssertAllClaimedResolve(GeneratedCmsgDispatch.ClaimedOpcodes, isModern: true);
        AssertAllClaimedResolve(GeneratedSmsgDispatch.ClaimedOpcodes, isModern: false);
    }

    /// <remarks>
    /// The two tables no longer share a thunk type - the legacy one carries
    /// <c>(WorldClient, WorldPacket)</c> so its handlers can stay instance methods parsing inline
    /// - so the null check cannot be written as one conditional over both.
    /// </remarks>
    private static unsafe void AssertAllClaimedResolve(IReadOnlyCollection<Opcode> claimed, bool isModern)
    {
        var unresolved = claimed
            .Where(o => isModern
                ? GeneratedCmsgDispatch.Get(o) == null
                : GeneratedSmsgDispatch.Get(o) == null)
            .Select(o => o.ToString())
            .ToList();

        Assert.True(unresolved.Count == 0,
            $"{(isModern ? "CMSG" : "SMSG")} opcodes are claimed but have no thunk: {string.Join(", ", unresolved)}");
    }

    // Opcodes claimed at the moment the reflective registries were deleted. There is no second
    // registry left to fall through to, so an opcode that stops being claimed is simply unhandled
    // at runtime with nothing but a log line to show for it. Raise these when opcodes are genuinely
    // added; a drop is a handler lost in transit.
    //
    // These count distinct *opcodes in the table for one build*, which is not the number of
    // [HandlesCmsg]/[HandlesSmsg] attributes in the source: a ranged handler contributes several
    // attributes that resolve to the same slot, so the attribute count is the larger number and
    // makes a useless floor.
    private const int ConvertedCmsgFloor = 384;
    private const int ConvertedSmsgFloor = 431;

    [Fact]
    public void ConvertedOpcodeCountsNeverFallBelowWhatWasMigrated()
    {
        Assert.True(GeneratedCmsgDispatch.ClaimedOpcodes.Count >= ConvertedCmsgFloor,
            $"CMSG dispatch claims {GeneratedCmsgDispatch.ClaimedOpcodes.Count} opcodes, " +
            $"below the {ConvertedCmsgFloor} that were converted. A handler lost its attribute.");

        Assert.True(GeneratedSmsgDispatch.ClaimedOpcodes.Count >= ConvertedSmsgFloor,
            $"SMSG dispatch claims {GeneratedSmsgDispatch.ClaimedOpcodes.Count} opcodes, " +
            $"below the {ConvertedSmsgFloor} that were converted. A handler lost its attribute.");
    }

    /// <summary>
    /// The update-object opcodes carry 79% of inbound bytes; losing their registration is the
    /// single most expensive way for an attribute to slip onto the wrong declaration.
    /// </summary>
    [Fact]
    public void UpdateObjectOpcodesAreHandled()
    {
        Assert.True(GeneratedSmsgDispatch.ClaimedOpcodes.Contains(Opcode.SMSG_UPDATE_OBJECT),
            "SMSG_UPDATE_OBJECT is not claimed by the generated table");
        Assert.True(GeneratedSmsgDispatch.ClaimedOpcodes.Contains(Opcode.SMSG_COMPRESSED_UPDATE_OBJECT),
            "SMSG_COMPRESSED_UPDATE_OBJECT is not claimed by the generated table");
    }
}
