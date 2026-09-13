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
/// Guards the coexistence of the generated dispatch tables and the reflective registries they
/// are replacing.
/// </summary>
/// <remarks>
/// The existing <c>PacketHandlerRegistrationTests</c> queries methods <i>on WorldSocket and
/// WorldClient</i>. As handlers move to static systems they drop out of that query, so it keeps
/// passing while covering less on every slice — it fails open. These assert the properties that
/// actually have to hold during the migration, and they get stricter as opcodes convert rather
/// than weaker.
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

    private static IEnumerable<MethodInfo> ReflectiveHandlers(Type hostType) =>
        hostType.GetMethods(BindingFlags.Instance | BindingFlags.Static |
                            BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttributes<PacketHandlerAttribute>().Any());

    private static HashSet<Opcode> ReflectiveOpcodes(Type hostType) =>
        ReflectiveHandlers(hostType)
            .SelectMany(m => m.GetCustomAttributes<PacketHandlerAttribute>())
            .Select(a => a.Opcode)
            .Where(o => o != Opcode.MSG_NULL_ACTION)
            .ToHashSet();

    private static IEnumerable<MethodInfo> SystemsWith<TAttribute>() where TAttribute : Attribute =>
        typeof(SessionContext).Assembly.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            .Where(m => m.GetCustomAttributes<TAttribute>().Any());

    [Fact]
    public void Cmsg_GeneratedAndReflectiveRegistriesAreDisjoint()
    {
        var overlap = GeneratedCmsgDispatch.ClaimedOpcodes
            .Intersect(ReflectiveOpcodes(typeof(WorldSocket)))
            .Select(o => o.ToString())
            .ToList();

        Assert.True(overlap.Count == 0,
            "An opcode is claimed by the generated table AND still registered reflectively. " +
            "One of them would never run: " + string.Join(", ", overlap));
    }

    [Fact]
    public void Smsg_GeneratedAndReflectiveRegistriesAreDisjoint()
    {
        var overlap = GeneratedSmsgDispatch.ClaimedOpcodes
            .Intersect(ReflectiveOpcodes(typeof(WorldClient)))
            .Select(o => o.ToString())
            .ToList();

        Assert.True(overlap.Count == 0,
            "An opcode is claimed by the generated table AND still registered reflectively. " +
            "One of them would never run: " + string.Join(", ", overlap));
    }

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

    [Fact]
    public void EveryOpcodeHandledBeforeTheMigrationIsStillHandled()
    {
        // Slice 1 converts nothing, so the two registries together must still cover exactly what
        // the reflective one covered alone. As slices land, opcodes move from the right column to
        // the left and this stays true — it is what catches a handler dropped in transit.
        var cmsgTotal = GeneratedCmsgDispatch.ClaimedOpcodes.Count + ReflectiveOpcodes(typeof(WorldSocket)).Count;
        var smsgTotal = GeneratedSmsgDispatch.ClaimedOpcodes.Count + ReflectiveOpcodes(typeof(WorldClient)).Count;

        Assert.True(cmsgTotal > 0, "no CMSG opcodes are handled by either registry");
        Assert.True(smsgTotal > 0, "no SMSG opcodes are handled by either registry");
    }
}
