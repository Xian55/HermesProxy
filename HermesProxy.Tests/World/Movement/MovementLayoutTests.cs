using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Objects;
using Xunit;

namespace HermesProxy.Tests.World.Movement;

/// <summary>
/// The mapping from a build to its movement layout, and every layout exercised in one process.
/// </summary>
/// <remarks>
/// The other classes in this folder follow the process's build pair, so a plain run covers one
/// layout per side. A layout is a value, though, and the codecs take one: the replay tests here
/// feed each pair's golden snapshot back through the codecs under that pair's layouts, whatever
/// the process is pinned to. That puts all six pairs under every <c>dotnet test</c>, with
/// <c>run-version-matrix.sh</c> still the only thing that runs the handlers around them.
/// </remarks>
public class MovementLayoutTests
{
    // ---- build -> layout ------------------------------------------------------------------

    [Theory]
    [InlineData(ClientVersionBuild.V1_12_1_5875, LegacyMovementLayout.Vanilla)]
    [InlineData(ClientVersionBuild.V1_12_2_6005, LegacyMovementLayout.Vanilla)]
    [InlineData(ClientVersionBuild.V1_12_3_6141, LegacyMovementLayout.Vanilla)]
    [InlineData(ClientVersionBuild.V2_4_3_8606, LegacyMovementLayout.Tbc)]
    [InlineData(ClientVersionBuild.V3_3_5a_12340, LegacyMovementLayout.WotLK)]
    public void LegacyLayout_ForEachSupportedBuild(ClientVersionBuild build, LegacyMovementLayout expected)
    {
        Assert.True(VersionChecker.IsSupportedLegacyVersion(build));
        Assert.Equal(expected, LegacyMovementLayouts.For(build));
    }

    [Fact]
    public void LegacyLayout_CoversEverySupportedBuild()
    {
        foreach (var build in Enum.GetValues<ClientVersionBuild>().Where(VersionChecker.IsSupportedLegacyVersion))
        {
            string name = build.ToString();
            LegacyMovementLayout expected =
                name.StartsWith("V1_", StringComparison.Ordinal) ? LegacyMovementLayout.Vanilla :
                name.StartsWith("V2_", StringComparison.Ordinal) ? LegacyMovementLayout.Tbc :
                LegacyMovementLayout.WotLK;
            Assert.Equal(expected, LegacyMovementLayouts.For(build));
        }
    }

    /// <summary>What each legacy layout struct says, against the wire: the table the codec is compiled from.</summary>
    [Fact]
    public void LegacyLayouts_StateTheirWireFacts()
    {
        Assert.Equal((LegacyMovementLayout.Vanilla, false, false, false), Facts<VanillaMovementLayout>());
        Assert.Equal((LegacyMovementLayout.Tbc, false, true, false), Facts<TbcMovementLayout>());
        Assert.Equal((LegacyMovementLayout.WotLK, true, true, true), Facts<WotLKMovementLayout>());

        static (LegacyMovementLayout, bool, bool, bool) Facts<T>() where T : struct, ILegacyMovementLayout
            => (T.Era, T.PackedTransportGuid, T.HasTransportTime, T.HasTransportSeat);
    }

    /// <summary>
    /// Every modern build the proxy accepts, by the family its name puts it in. A build added to
    /// <c>IsSupportedModernVersion</c> without a row here fails, which is the point: its movement
    /// layout is a decision, not a default.
    /// </summary>
    [Fact]
    public void ModernLayout_ForEverySupportedBuild()
    {
        (string Prefix, ModernMovementLayout Layout)[] families =
        [
            ("V1_14_0_", ModernMovementLayout.BitFlags),
            ("V1_14_1_", ModernMovementLayout.WordFlags),
            ("V1_14_2_", ModernMovementLayout.WordFlags),
            ("V2_5_2_", ModernMovementLayout.BitFlags),
            ("V2_5_3_", ModernMovementLayout.WordFlags),
            ("V3_4_3_", ModernMovementLayout.WotLKClassic),
            ("V4_4_2_", ModernMovementLayout.WotLKClassic),
        ];

        int checkedBuilds = 0;
        foreach (var build in Enum.GetValues<ClientVersionBuild>().Where(VersionChecker.IsSupportedModernVersion))
        {
            string name = build.ToString();
            var family = families.SingleOrDefault(f => name.StartsWith(f.Prefix, StringComparison.Ordinal));
            Assert.True(family.Prefix != null, $"{build} is supported but has no movement layout family in this test.");
            Assert.True(family.Layout == ModernMovementLayouts.For(build), $"{build}: {ModernMovementLayouts.For(build)}");
            checkedBuilds++;
        }
        Assert.True(checkedBuilds > 50);
    }

    /// <summary>
    /// 4.4.2 is not supported yet, so the loop above does not reach it. Its client reads the
    /// movement block exactly as 3.4.3 does (docs/protocol/4.4.2.60895).
    /// </summary>
    [Fact]
    public void ModernLayout_4_4_2_IsTheWotLKClassicLayout()
    {
        Assert.Equal(ModernMovementLayout.WotLKClassic, ModernMovementLayouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    /// <summary>What each modern layout struct says, against the wire: the table the codec is compiled from.</summary>
    [Fact]
    public void ModernLayouts_StateTheirWireFacts()
    {
        Assert.Equal((false, false, false, false, false), Facts<BitFlagsMovementLayout>());
        Assert.Equal((true, true, false, false, false), Facts<WordFlagsMovementLayout>());
        Assert.Equal((true, true, true, true, true), Facts<WotLKClassicMovementLayout>());

        static (bool, bool, bool, bool, bool) Facts<T>() where T : struct, IModernMovementLayout
            => (T.FlagsAreWords, T.HasInertia, T.HasStandingOnGameObject, T.HasAdvFlying, T.WritesExtraFlagsAsZero);
    }

    /// <summary>
    /// The layouts in use against the version predicates the codecs branched on before they had
    /// layouts. Run under <c>run-version-matrix.sh</c> this holds for every pair.
    /// </summary>
    [Fact]
    public void CurrentLayouts_AgreeWithTheVersionPredicates()
    {
        bool wotlk = LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056);
        bool tbc = LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180);
        Assert.Equal(
            wotlk ? LegacyMovementLayout.WotLK : tbc ? LegacyMovementLayout.Tbc : LegacyMovementLayout.Vanilla,
            LegacyMovementLayouts.Current);
        // The one predicate the era does not imply: every supported 3.x build is past 3.1.0.
        Assert.Equal(wotlk, LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767));

        bool flagsAreWords = ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3);
        Assert.Equal(
            ModernVersion.Build == ClientVersionBuild.V3_4_3_54261 ? ModernMovementLayout.WotLKClassic :
            flagsAreWords ? ModernMovementLayout.WordFlags : ModernMovementLayout.BitFlags,
            ModernMovementLayouts.Current);
        // 3.4.3 is a word-flags build; the layout enum leans on that.
        Assert.True(flagsAreWords || ModernVersion.Build != ClientVersionBuild.V3_4_3_54261);
    }

    // ---- every pair's snapshot, replayed through the codecs -------------------------------

    private sealed record Scenario(string Name, byte[] In, string[] Out);

    private static string GoldenDirectory([CallerFilePath] string thisFile = "")
        => Path.Combine(Path.GetDirectoryName(thisFile)!, "Golden");

    public static TheoryData<string, string> SnapshotPairs()
    {
        var pairs = new TheoryData<string, string>();
        foreach (string file in Directory.EnumerateFiles(GoldenDirectory(), "MovementWireGoldenTests.ServerMoves_*.verified.txt").Order())
        {
            // MovementWireGoldenTests.ServerMoves_<legacy>.<modern>.verified.txt
            string[] builds = Path.GetFileName(file).Split("ServerMoves_")[1].Split('.');
            pairs.Add(builds[0], builds[1]);
        }
        return pairs;
    }

    private static List<Scenario> ReadSnapshot(string test, string legacy, string modern)
    {
        string path = Path.Combine(GoldenDirectory(), $"MovementWireGoldenTests.{test}_{legacy}.{modern}.verified.txt");
        var scenarios = new List<Scenario>();
        string? name = null;
        byte[] input = [];
        var output = new List<string>();

        void Flush()
        {
            if (name != null)
                scenarios.Add(new Scenario(name, input, [.. output]));
            output.Clear();
        }

        foreach (string raw in File.ReadLines(path))
        {
            string line = raw.TrimStart('\uFEFF').TrimEnd();
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                Flush();
                name = line[3..];
            }
            else if (line.StartsWith("in  ", StringComparison.Ordinal))
                input = Convert.FromHexString(line[4..]);
            else if (line.StartsWith("out ", StringComparison.Ordinal))
                output.Add(line[4..]);
        }
        Flush();
        return scenarios;
    }

    [Fact]
    public void Snapshots_CoverEveryLayout()
    {
        var pairs = SnapshotPairs().Select(row => (Legacy: Enum.Parse<ClientVersionBuild>((string)row.Data.Item1), Modern: Enum.Parse<ClientVersionBuild>((string)row.Data.Item2))).ToArray();

        Assert.Equal(Enum.GetValues<LegacyMovementLayout>().Length, pairs.Select(p => LegacyMovementLayouts.For(p.Legacy)).Distinct().Count());
        Assert.Equal(Enum.GetValues<ModernMovementLayout>().Length, pairs.Select(p => ModernMovementLayouts.For(p.Modern)).Distinct().Count());
    }

    /// <summary>
    /// Legacy heartbeats to the client: the legacy read, the repair and the modern write, under
    /// this pair's layouts, against what the handlers produced when the snapshot was taken.
    /// </summary>
    [Theory]
    [MemberData(nameof(SnapshotPairs))]
    public void ServerHeartbeats_ReplayedUnderThePairsLayouts_MatchTheSnapshot(string legacy, string modern)
    {
        var legacyBuild = Enum.Parse<ClientVersionBuild>(legacy);
        var modernBuild = Enum.Parse<ClientVersionBuild>(modern);
        var legacyLayout = LegacyMovementLayouts.For(legacyBuild);
        var modernLayout = ModernMovementLayouts.For(modernBuild);

        var scenarios = ReadSnapshot("ServerMoves", legacy, modern).Where(s => s.Name.StartsWith("heartbeat/", StringComparison.Ordinal)).ToList();
        Assert.True(scenarios.Count >= 20);

        Span<byte> buffer = stackalloc byte[ModernMovementCodec.MaxSize];
        foreach (var scenario in scenarios)
        {
            var harness = new LegacyHandlerHarness(recordClientPackets: false);
            harness.SetActivePlayer(MovementScenarios.ActivePlayer);
            var gameState = harness.Session.GameState;

            using var packet = new WorldPacket(scenario.In);    // consumes the opcode
            WowGuid128 mover = packet.ReadPackedGuid().To128(gameState);
            LegacyMovementCodec.Read(legacyLayout, packet, gameState, out MovementInfo info, out LegacyMovementExtras extras);
            Assert.True(packet.Remaining() == 0, $"{scenario.Name}: {packet.Remaining()} bytes left unread");

            string expected = Assert.Single(scenario.Out);
            if (WorldClient.IsSplineDrivenMove((uint)extras.Flags, modernBuild))
            {
                Assert.Equal("(nothing)", expected);
                continue;
            }

            MovementSanitizer.Sanitize(ref info);
            int written = ModernMovementCodec.Write(modernLayout, buffer, mover, in info);
            string actual = "SMSG_MOVE_UPDATE:" + Convert.ToHexString(buffer[..written]);
            Assert.True(expected == actual, $"{scenario.Name}\n  expected {expected}\n  actual   {actual}");
        }
    }

    /// <summary>
    /// Client movement to the legacy server: the modern read and the legacy write, under this
    /// pair's layouts.
    /// </summary>
    [Theory]
    [MemberData(nameof(SnapshotPairs))]
    public void ClientMoves_ReplayedUnderThePairsLayouts_MatchTheSnapshot(string legacy, string modern)
    {
        var legacyBuild = Enum.Parse<ClientVersionBuild>(legacy);
        var legacyLayout = LegacyMovementLayouts.For(legacyBuild);
        var modernLayout = ModernMovementLayouts.For(Enum.Parse<ClientVersionBuild>(modern));

        var scenarios = ReadSnapshot("ClientMoves", legacy, modern).Where(s => s.Name.StartsWith("player-move/", StringComparison.Ordinal)).ToList();
        Assert.True(scenarios.Count >= 15);

        foreach (var scenario in scenarios)
        {
            var reader = new SpanPacketReader(scenario.In);
            reader.ReadPackedGuid128(out ulong moverLow, out ulong moverHigh);
            var mover = new WowGuid128(moverLow, moverHigh);
            ModernMovementCodec.Read(modernLayout, ref reader, out MovementInfo info);
            Assert.True(reader.Remaining == 0, $"{scenario.Name}: {reader.Remaining} bytes left unread");

            using var packet = new WorldPacket();
            // 3.2.0 on, the mover leads the packet; before that the server takes it from the session.
            if (legacyBuild >= ClientVersionBuild.V3_2_0_10192)
                packet.WritePackedGuid(mover.To64());
            LegacyMovementCodec.Write(legacyLayout, packet, in info);

            string expected = Assert.Single(scenario.Out);
            string actual = "MSG_MOVE_HEARTBEAT:" + Convert.ToHexString(packet.GetDataSpan());
            Assert.True(expected == actual, $"{scenario.Name}\n  expected {expected}\n  actual   {actual}");
        }
    }
}
