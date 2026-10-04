using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HermesProxy.Enums;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using VerifyXunit;
using Xunit;

namespace HermesProxy.Tests.World.Movement;

/// <summary>
/// Every packet that carries a movement block, through the real handlers, as bytes: what the
/// modern client is sent for each legacy packet, and what the legacy server is sent for each
/// client packet.
/// </summary>
/// <remarks>
/// <para>
/// The snapshots were captured from the handlers at a45aa8e1, before <c>MovementInfo</c> was split
/// into a value type, a create block and two codecs. A movement block that changes on the wire
/// does not throw; the client rubber-bands, drops a spline or falls through a boat. This is what
/// turns that into a failing test.
/// </para>
/// <para>
/// <b>One snapshot set per build pair.</b> <c>LegacyVersion</c> and <c>ModernVersion</c> are fixed
/// for the process, so a plain run checks the default pair only. <c>run-version-matrix.sh</c> in
/// this folder runs the class once per pair that has a distinct wire layout; run it after any
/// change to the movement codecs.
/// </para>
/// <para>
/// A create's snapshot holds its movement block alone, written by the version's
/// <c>ObjectUpdateBuilder</c>, rather than the whole SMSG_UPDATE_OBJECT. The values that follow
/// it belong to other tests, and a change to them should not land here.
/// </para>
/// </remarks>
public class MovementWireGoldenTests
{
    private static string BuildPair => $"{LegacyVersion.Build}.{ModernVersion.Build}";

    private static Task VerifyGolden(StringBuilder text)
        => Verifier.Verify(text.ToString(), extension: "txt")
            .UseDirectory("Golden")
            .UseTextForParameters(BuildPair);

    private static LegacyHandlerHarness NewHarness()
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true, recordServerPackets: true);
        harness.SetActivePlayer(MovementScenarios.ActivePlayer);
        return harness;
    }

    [Fact]
    public Task ServerMoves()
    {
        var text = new StringBuilder();
        foreach (var scenario in MovementScenarios.ServerMoves())
        {
            var harness = NewHarness();
            text.Append("## ").AppendLine(scenario.Name);
            text.Append("in  ").AppendLine(Convert.ToHexString(scenario.Wire));
            try
            {
                harness.Deliver(scenario.Opcode, scenario.Wire, scenario.Handler(harness.Client));
            }
            catch (UnmappedOpcodeException)
            {
                // The modern build has no opcode for what the handler builds; the dispatcher
                // logs and drops the packet.
                text.AppendLine("out (opcode unmapped on this modern build)");
            }
            AppendClientPackets(text, harness);
            text.AppendLine();
        }

        return VerifyGolden(text);
    }

    [Fact]
    public Task Creates()
    {
        var text = new StringBuilder();
        foreach (var (name, create) in MovementScenarios.Creates())
        {
            var harness = NewHarness();
            byte[] wire = LegacyCreateWire.Build(create);
            text.Append("## ").AppendLine(name);
            text.Append("in  ").AppendLine(Convert.ToHexString(wire));
            harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, wire, harness.Client.HandleUpdateObject);

            text.Append("out ").AppendLine(harness.ClientWire.Sent.Count == 0
                ? "(nothing)"
                : string.Join(' ', harness.ClientWire.Sent.Select(s => s.Opcode)));
            foreach (var update in harness.ClientWire.Sent.Select(s => s.Packet).OfType<UpdateObject>().SelectMany(p => p.ObjectUpdates))
            {
                text.Append("    ").Append(update.Type).Append(' ').Append(update.CreateData?.ObjectType)
                    .Append(' ').AppendLine(Convert.ToHexString(CreateMovementBlock.Write(update, harness.Session.GameState)));
            }
            text.AppendLine();
        }

        return VerifyGolden(text);
    }

    [Fact]
    public Task ClientMoves()
    {
        var text = new StringBuilder();
        // One harness only to resolve the guids the scenarios embed; each runs on its own.
        foreach (var scenario in MovementScenarios.ClientMoves(NewHarness().Session.GameState))
        {
            var harness = NewHarness();
            var ctx = new SessionContext(harness.Session, socket: null, harness.Client);
            text.Append("## ").AppendLine(scenario.Name);
            text.Append("in  ").AppendLine(Convert.ToHexString(scenario.Framed.AsSpan(2)));
            try
            {
                MovementScenarios.Dispatch(scenario, in ctx);
            }
            catch (UnmappedOpcodeException)
            {
                // The legacy build has no such opcode; the dispatcher logs and drops it.
                text.AppendLine("out (opcode unmapped on this legacy build)");
            }

            foreach (var sent in harness.ServerWire.Sent)
            {
                text.Append("out ").Append(LegacyVersion.GetUniversalOpcode(sent.Opcode)).Append(':')
                    .AppendLine(Convert.ToHexString(sent.Bytes));
            }
            AppendClientPackets(text, harness);
            text.AppendLine();
        }

        return VerifyGolden(text);
    }

    /// <summary>
    /// The same scenarios twice must serialize identically, or a snapshot is recording a clock.
    /// </summary>
    [Fact]
    public void Creates_AreDeterministic()
    {
        Assert.Equal(CreateBlocks(), CreateBlocks());

        static List<string> CreateBlocks()
        {
            var blocks = new List<string>();
            foreach (var (_, create) in MovementScenarios.Creates())
            {
                var harness = NewHarness();
                harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, LegacyCreateWire.Build(create), harness.Client.HandleUpdateObject);
                blocks.AddRange(harness.ClientWire.Sent.Select(s => Convert.ToHexString(s.Bytes)));
            }
            return blocks;
        }
    }

    private static void AppendClientPackets(StringBuilder text, LegacyHandlerHarness harness)
    {
        foreach (var sent in harness.ClientWire.Sent)
            text.Append("out ").Append(sent.Opcode).Append(':').AppendLine(Convert.ToHexString(sent.Bytes));
        if (harness.ClientWire.Sent.Count == 0 && harness.ServerWire.Sent.Count == 0)
            text.AppendLine("out (nothing)");
    }
}

/// <summary>The create movement block alone, from whichever builder the modern build uses.</summary>
internal static class CreateMovementBlock
{
    public static byte[] Write(ObjectUpdate update, GameSessionData gameState)
    {
        using var packet = new WorldPacket();
        switch (ModernVersion.GetUpdateFieldsDefiningBuild())
        {
            case ClientVersionBuild.V1_14_0_40237:
            {
                var builder = new HermesProxy.World.Objects.Version.V1_14_0_40237.ObjectUpdateBuilder(update, gameState);
                builder.SetCreateObjectBits();
                builder.BuildMovementUpdate(packet);
                break;
            }
            case ClientVersionBuild.V1_14_1_40688:
            {
                var builder = new HermesProxy.World.Objects.Version.V1_14_1_40688.ObjectUpdateBuilder(update, gameState);
                builder.SetCreateObjectBits();
                builder.BuildMovementUpdate(packet);
                break;
            }
            case ClientVersionBuild.V2_5_2_39570:
            {
                var builder = new HermesProxy.World.Objects.Version.V2_5_2_39570.ObjectUpdateBuilder(update, gameState);
                builder.SetCreateObjectBits();
                builder.BuildMovementUpdate(packet);
                break;
            }
            case ClientVersionBuild.V2_5_3_41750:
            {
                var builder = new HermesProxy.World.Objects.Version.V2_5_3_41750.ObjectUpdateBuilder(update, gameState);
                builder.SetCreateObjectBits();
                builder.BuildMovementUpdate(packet);
                break;
            }
            case ClientVersionBuild.V3_4_3_54261:
            {
                var builder = new HermesProxy.World.Objects.Version.V3_4_3_54261.ObjectUpdateBuilder(update, gameState);
                builder.SetCreateObjectBits();
                builder.BuildMovementUpdate(packet);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(update), "No object update builder defined for the current build.");
        }

        return packet.GetDataSpan().ToArray();
    }
}
