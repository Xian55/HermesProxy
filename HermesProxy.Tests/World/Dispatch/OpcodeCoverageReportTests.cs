using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Generates <c>docs/opcode-coverage.md</c> from the dispatch attributes and the generated opcode
/// tables, and fails when the committed file is stale.
/// </summary>
/// <remarks>
/// <para>
/// The attributes already carry everything needed to answer "which opcodes do we translate for
/// which build, and where does a packet's shape change between builds" — but only as data spread
/// across 829 declarations, which nobody can read. This renders it.
/// </para>
/// <para>
/// Coverage is computed from the <b>attributes and their ranges</b>, not from
/// <c>GeneratedCmsgDispatch.ClaimedOpcodes</c>. The claimed set is built once for whichever build
/// the test process bootstrapped, so it can answer for exactly one build; the attributes can
/// answer for all of them.
/// </para>
/// <para>
/// Regenerating on mismatch rather than only asserting means the fix for a failure is
/// <c>git add docs/opcode-coverage.md</c>, which is the same contract as the Verify snapshots.
/// </para>
/// </remarks>
public partial class OpcodeCoverageReportTests
{
    static OpcodeCoverageReportTests()
    {
        if (VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            VersionBootstrap.LegacyBuild = ClientVersionBuild.V3_3_5a_12340;
        if (VersionBootstrap.ModernBuild == ClientVersionBuild.Zero)
            VersionBootstrap.ModernBuild = ClientVersionBuild.V3_4_3_54261;
    }

    /// <summary>Builds with a generated opcode table, split by which side they can appear on.</summary>
    private static readonly ClientVersionBuild[] ModernBuilds =
    [
        ClientVersionBuild.V1_14_1_40688,
        ClientVersionBuild.V2_5_2_39570,
        ClientVersionBuild.V2_5_3_41750,
        ClientVersionBuild.V3_4_3_54261,
    ];

    private static readonly ClientVersionBuild[] LegacyBuilds =
    [
        ClientVersionBuild.V1_12_1_5875,
        ClientVersionBuild.V2_4_3_8606,
        ClientVersionBuild.V3_3_5a_12340,
    ];

    [Fact]
    public void CoverageReportIsUpToDate()
    {
        string generated = BuildReport();
        string path = Path.Combine(FindRepoRoot(), "docs", "opcode-coverage.md");

        string? existing = File.Exists(path) ? File.ReadAllText(path) : null;
        if (existing is not null && Normalize(existing) == Normalize(generated))
            return;

        File.WriteAllText(path, generated);
        Assert.Fail(
            $"docs/opcode-coverage.md was {(existing is null ? "missing" : "stale")} and has been " +
            "regenerated. Review the diff and commit it.");
    }

    // CRLF/LF and trailing-whitespace differences are not signal; the repo normalises line endings
    // on checkout, so comparing raw text would fail on a fresh clone for no reason.
    private static string Normalize(string s)
        => string.Join('\n', s.Replace("\r\n", "\n").Split('\n').Select(l => l.TrimEnd())).TrimEnd();

    // ---- model ----

    private sealed record Ranged(Opcode Opcode, ClientVersionBuild AddedIn, ClientVersionBuild RemovedIn, string Handler);

    private sealed record CodecRange(string Packet, string Codec, ClientVersionBuild AddedIn, ClientVersionBuild RemovedIn, bool AgainstLegacy);

    private static bool Covers(ClientVersionBuild build, ClientVersionBuild addedIn, ClientVersionBuild removedIn)
    {
        // Mirrors VersionChecker: AddedInVersion is Build >= addedIn, RemovedInVersion is
        // Build < removedIn, and Zero means unbounded on either end.
        if (addedIn != ClientVersionBuild.Zero && build < addedIn) return false;
        if (removedIn != ClientVersionBuild.Zero && build >= removedIn) return false;
        return true;
    }

    private static IEnumerable<MethodInfo> AllMethods()
        => typeof(SessionContext).Assembly.GetTypes()
            .SelectMany(t => t.GetMethods(
                BindingFlags.Static | BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));

    private static List<Ranged> Handlers<TAttribute>() where TAttribute : Attribute
    {
        var result = new List<Ranged>();
        foreach (var m in AllMethods())
        {
            foreach (var a in m.GetCustomAttributes<TAttribute>())
            {
                var t = typeof(TAttribute);
                var opcode = (Opcode)t.GetProperty("Opcode")!.GetValue(a)!;
                if (opcode == Opcode.MSG_NULL_ACTION)
                    continue;

                result.Add(new Ranged(
                    opcode,
                    (ClientVersionBuild)t.GetProperty("AddedIn")!.GetValue(a)!,
                    (ClientVersionBuild)t.GetProperty("RemovedIn")!.GetValue(a)!,
                    $"{m.DeclaringType!.Name}.{m.Name}"));
            }
        }
        return result;
    }

    private static List<CodecRange> Codecs()
    {
        var result = new List<CodecRange>();
        foreach (var t in typeof(SessionContext).Assembly.GetTypes())
        {
            var a = t.GetCustomAttribute<PacketCodecAttribute>();
            if (a is null)
                continue;

            result.Add(new CodecRange(a.PacketType.Name, t.Name, a.AddedIn, a.RemovedIn, a.AgainstLegacyVersion));
        }
        return result;
    }

    /// <summary>Universal opcodes that the given build's table actually maps (0 = no mapping).</summary>
    private static HashSet<Opcode> DefinedFor(ClientVersionBuild build)
    {
        if (!GeneratedOpcodeTables.TryGet(build, out _, out uint[] universalToCurrent))
            throw new InvalidOperationException($"No generated opcode table for {build}.");

        var set = new HashSet<Opcode>();
        for (int i = 0; i < universalToCurrent.Length; i++)
            if (universalToCurrent[i] != 0)
                set.Add((Opcode)i);
        return set;
    }

    // ---- report ----

    private static string BuildReport()
    {
        var cmsg = Handlers<HandlesCmsgAttribute>();
        var smsg = Handlers<HandlesSmsgAttribute>();
        var codecs = Codecs();
        var equality = ScanEqualitySites();

        var sb = new StringBuilder();
        sb.AppendLine("# Opcode coverage and version lifecycle");
        sb.AppendLine();
        sb.AppendLine("<!-- Generated by HermesProxy.Tests/World/Dispatch/OpcodeCoverageReportTests.cs.");
        sb.AppendLine("     Do not edit by hand: the test rewrites this file and fails when it is stale. -->");
        sb.AppendLine();
        sb.AppendLine("Two independent axes, and conflating them is the easiest mistake to make here:");
        sb.AppendLine();
        sb.AppendLine("- **Modern / client axis** — `ModernVersion`, the retail or Classic client talking to the");
        sb.AppendLine("  proxy. `[HandlesCmsg]` and most `[PacketCodec]` ranges are measured against it.");
        sb.AppendLine("- **Legacy / server axis** — `LegacyVersion`, the emulator the proxy talks to.");
        sb.AppendLine("  `[HandlesSmsg]` ranges are measured against it.");
        sb.AppendLine();
        sb.AppendLine("A build number is only comparable within its own branch (`ClientBranch`): the Classic-line");
        sb.AppendLine("`V3_4_3_54261` is 54261 while the original-line `V4_3_4_15595` is 15595, so an ordered");
        sb.AppendLine("comparison across branches is meaningless. `VersionChecker.AssertComparableBranch` enforces");
        sb.AppendLine("that at runtime.");
        sb.AppendLine();

        AppendCoverage(sb, cmsg, smsg);
        AppendRangedHandlers(sb, smsg, cmsg);
        AppendCodecRanges(sb, codecs);
        AppendEqualitySites(sb, equality);

        return sb.ToString();
    }

    private static void AppendCoverage(StringBuilder sb, List<Ranged> cmsg, List<Ranged> smsg)
    {
        sb.AppendLine("## 1. Dispatch coverage per build");
        sb.AppendLine();
        sb.AppendLine("`defined` is how many opcodes of that direction the build's generated table maps at all.");
        sb.AppendLine("`handled` is how many of those have a handler whose range covers the build. `gap` is the");
        sb.AppendLine("remainder — opcodes the client can send, or the server can send us, that fall through to a");
        sb.AppendLine("\"No handler for opcode\" log line. A gap is not automatically a bug: much of it is traffic");
        sb.AppendLine("the proxy has no reason to translate.");
        sb.AppendLine();

        sb.AppendLine("### Client to proxy (CMSG), by client build");
        sb.AppendLine();
        sb.AppendLine("| build | defined | handled | gap |");
        sb.AppendLine("|---|---:|---:|---:|");
        foreach (var b in ModernBuilds)
            AppendCoverageRow(sb, b, cmsg, "CMSG_");
        sb.AppendLine();

        sb.AppendLine("### Server to proxy (SMSG), by emulator build");
        sb.AppendLine();
        sb.AppendLine("| build | defined | handled | gap |");
        sb.AppendLine("|---|---:|---:|---:|");
        foreach (var b in LegacyBuilds)
            AppendCoverageRow(sb, b, smsg, "SMSG_");
        sb.AppendLine();
    }

    private static void AppendCoverageRow(StringBuilder sb, ClientVersionBuild build, List<Ranged> handlers, string prefix)
    {
        var defined = DefinedFor(build).Where(o => o.ToString().StartsWith(prefix, StringComparison.Ordinal)).ToHashSet();
        var handled = handlers
            .Where(h => Covers(build, h.AddedIn, h.RemovedIn))
            .Select(h => h.Opcode)
            .Where(defined.Contains)
            .ToHashSet();

        sb.AppendLine($"| `{build}` | {defined.Count} | {handled.Count} | {defined.Count - handled.Count} |");
    }

    private static void AppendRangedHandlers(StringBuilder sb, List<Ranged> smsg, List<Ranged> cmsg)
    {
        var ranged = smsg.Concat(cmsg)
            .Where(h => h.AddedIn != ClientVersionBuild.Zero || h.RemovedIn != ClientVersionBuild.Zero)
            .OrderBy(h => h.Opcode.ToString(), StringComparer.Ordinal)
            .ToList();

        sb.AppendLine("## 2. Version-ranged handlers");
        sb.AppendLine();
        sb.AppendLine("Opcodes whose *handling* differs by build — the handler itself is selected per build when");
        sb.AppendLine("the dispatch table is constructed, not per packet. All of these are on the legacy axis;");
        sb.AppendLine("there are no ranged `[HandlesCmsg]` sites yet, which is itself the finding: every per-build");
        sb.AppendLine("difference on the modern side currently hides inside a handler body instead (see section 4).");
        sb.AppendLine();
        sb.AppendLine($"{ranged.Count} ranged handler declarations.");
        sb.AppendLine();
        sb.AppendLine("| opcode | added in | removed in | handler |");
        sb.AppendLine("|---|---|---|---|");
        foreach (var h in ranged)
        {
            sb.AppendLine($"| `{h.Opcode}` | {Show(h.AddedIn)} | {Show(h.RemovedIn)} | `{h.Handler}` |");
        }
        sb.AppendLine();
    }

    private static void AppendCodecRanges(StringBuilder sb, List<CodecRange> codecs)
    {
        var byPacket = codecs
            .GroupBy(c => c.Packet)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .ToList();

        sb.AppendLine("## 3. Packets whose wire layout changes between builds");
        sb.AppendLine();
        sb.AppendLine("Each of these has more than one codec, chosen by build range at table-build time. For");
        sb.AppendLine("anyone adding a client this is the short list worth checking first: a layout that already");
        sb.AppendLine("changed once is the most likely to have changed again.");
        sb.AppendLine();
        sb.AppendLine($"{byPacket.Count} packets across {codecs.Count} codecs.");
        sb.AppendLine();
        sb.AppendLine("| packet | codec | axis | added in | removed in |");
        sb.AppendLine("|---|---|---|---|---|");
        foreach (var g in byPacket)
        {
            foreach (var c in g.OrderBy(c => c.Codec, StringComparer.Ordinal))
            {
                string axis = c.AgainstLegacy ? "legacy" : "modern";
                sb.AppendLine($"| `{c.Packet}` | `{c.Codec}` | {axis} | {Show(c.AddedIn)} | {Show(c.RemovedIn)} |");
            }
        }
        sb.AppendLine();
    }

    private static void AppendEqualitySites(StringBuilder sb, List<(string File, string Side, string Build, int Count)> sites)
    {
        sb.AppendLine("## 4. Exact-equality version checks — the new-client work list");
        sb.AppendLine();
        sb.AppendLine("`ModernVersion.Build == ClientVersionBuild.X` (or `!=`) inside a method body. These are the");
        sb.AppendLine("sites that do **not** carry forward: a build the code has never seen fails the equality and");
        sb.AppendLine("takes the `else`, which is generally the oldest supported layout — the opposite of what a");
        sb.AppendLine("newer client wants, and silent when wrong.");
        sb.AppendLine();
        sb.AppendLine("Converting one to a range (`AddedInVersion`, or a second `[PacketCodec]`) makes it inherit");
        sb.AppendLine("correctly by default. They cannot be swept mechanically: some genuinely mean \"this build");
        sb.AppendLine("only\" — a quirk a later client fixed — and widening those would propagate a bug forward.");
        sb.AppendLine("Each needs a judgement call against the client or WowPacketParser.");
        sb.AppendLine();
        sb.AppendLine("Counts per file rather than line numbers, so unrelated edits do not churn this file.");
        sb.AppendLine();

        int total = sites.Sum(s => s.Count);
        sb.AppendLine($"{total} sites across {sites.Select(s => s.File).Distinct().Count()} files.");
        sb.AppendLine();

        foreach (var side in sites.Select(s => s.Side).Distinct().OrderBy(s => s, StringComparer.Ordinal))
        {
            var forSide = sites.Where(s => s.Side == side).ToList();
            sb.AppendLine($"### `{side}.Build` compared by equality — {forSide.Sum(s => s.Count)} sites");
            sb.AppendLine();
            sb.AppendLine("| file | build compared | sites |");
            sb.AppendLine("|---|---|---:|");
            foreach (var s in forSide
                         .OrderByDescending(s => s.Count)
                         .ThenBy(s => s.File, StringComparer.Ordinal)
                         .ThenBy(s => s.Build, StringComparer.Ordinal))
            {
                sb.AppendLine($"| `{s.File}` | `{s.Build}` | {s.Count} |");
            }
            sb.AppendLine();
        }
    }

    private static string Show(ClientVersionBuild b)
        => b == ClientVersionBuild.Zero ? "—" : $"`{b}`";

    // ---- source scan ----

    [GeneratedRegex(@"\b(ModernVersion|LegacyVersion)\.Build\s*(?:==|!=)\s*ClientVersionBuild\.(\w+)")]
    private static partial Regex EqualitySite();

    private static List<(string File, string Side, string Build, int Count)> ScanEqualitySites()
    {
        string root = FindRepoRoot();
        var counts = new Dictionary<(string, string, string), int>();

        foreach (var project in (string[])["HermesProxy", "Framework"])
        {
            string dir = Path.Combine(root, project);
            if (!Directory.Exists(dir))
                continue;

            foreach (var file in Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories))
            {
                // obj/ holds generated copies of the same sites; counting them would double everything.
                string rel = Path.GetRelativePath(root, file).Replace('\\', '/');
                if (rel.Contains("/obj/", StringComparison.Ordinal) || rel.Contains("/bin/", StringComparison.Ordinal))
                    continue;

                foreach (Match m in EqualitySite().Matches(File.ReadAllText(file)))
                {
                    var key = (rel, m.Groups[1].Value, m.Groups[2].Value);
                    counts[key] = counts.GetValueOrDefault(key) + 1;
                }
            }
        }

        return counts
            .Select(kv => (File: kv.Key.Item1, Side: kv.Key.Item2, Build: kv.Key.Item3, Count: kv.Value))
            .ToList();
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
