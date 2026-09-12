using System;
using System.Linq;
using HermesProxy;
using HermesProxy.Enums;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// <see cref="VersionChecker.GetBranch"/> names a discrimination the codebase already performed
/// inline as <c>ExpansionVersion == 1 / 2 || 3 / else</c>. These pin that it is the *same*
/// discrimination over every build the proxy actually accepts, and that the two places it
/// deliberately differs are the ones that were wrong before.
/// </summary>
public class ClientBranchTests
{
    /// The chain that AddedInVersion(9 bytes), AddedInClassicVersion(6 bytes) and
    /// IsClassicVersionBuild() each inlined before ClientBranch existed. Frozen as an oracle —
    /// do not "fix" it; its whole value is that it does not change.
    private enum LegacyArm { Retail, ClassicEra, Classic }

    private static LegacyArm OldChain(byte expansion) =>
        expansion == 1 ? LegacyArm.ClassicEra
        : expansion is 2 or 3 ? LegacyArm.Classic
        : LegacyArm.Retail;

    private static LegacyArm NewChain(ClientBranch branch) => branch switch
    {
        ClientBranch.ClassicEra => LegacyArm.ClassicEra,
        ClientBranch.Classic => LegacyArm.Classic,
        _ => LegacyArm.Retail,
    };

    // Uses the production parsers rather than a second copy: a duplicate would drift, and the
    // ToString().Split() form allocated three times per build across ~500 enum members.
    private static (byte Expansion, byte Major) Parse(ClientVersionBuild build)
        => (VersionChecker.GetExpansionVersion(build), VersionChecker.GetMajorPatchVersion(build));

    private static ClientVersionBuild[] AllBuilds =>
        Enum.GetValues<ClientVersionBuild>().Where(b => b != ClientVersionBuild.Zero).ToArray();

    [Fact]
    public void GetBranch_MatchesTheOldInlineChain_ForEverySupportedModernBuild()
    {
        var mismatches = AllBuilds
            .Where(VersionChecker.IsSupportedModernVersion)
            .Select(b => (Build: b, Parsed: Parse(b)))
            .Where(x => OldChain(x.Parsed.Expansion) !=
                        NewChain(VersionChecker.GetBranch(x.Parsed.Expansion, x.Parsed.Major)))
            .Select(x => x.Build.ToString())
            .ToList();

        Assert.True(mismatches.Count == 0,
            $"ClientBranch changed which version triple these builds select: {string.Join(", ", mismatches)}");
    }

    [Fact]
    public void GetBranch_MatchesTheOldInlineChain_ForEverySupportedLegacyBuild()
    {
        var mismatches = AllBuilds
            .Where(VersionChecker.IsSupportedLegacyVersion)
            .Select(b => (Build: b, Parsed: Parse(b)))
            .Where(x => VersionChecker.GetBranch(x.Parsed.Expansion, x.Parsed.Major) != ClientBranch.Retail)
            .Select(x => x.Build.ToString())
            .ToList();

        // Every supported legacy build is on the original progression, which is what makes the
        // legacy axis' raw-build comparisons (PacketHandlerAttribute's 28 ranged sites) sound.
        Assert.True(mismatches.Count == 0,
            $"Supported legacy builds must be on the Retail line; these were not: {string.Join(", ", mismatches)}");
    }

    [Theory]
    // Original progression — everything the legacy side talks to.
    [InlineData(1, 12, ClientBranch.Retail)]
    [InlineData(2, 4, ClientBranch.Retail)]
    [InlineData(3, 3, ClientBranch.Retail)]
    [InlineData(4, 3, ClientBranch.Retail)]   // Cataclysm 4.3.4 — the issue #202 backend
    [InlineData(9, 1, ClientBranch.Retail)]
    // Re-released lines.
    [InlineData(1, 13, ClientBranch.ClassicEra)]
    [InlineData(1, 14, ClientBranch.ClassicEra)]
    [InlineData(2, 5, ClientBranch.Classic)]
    [InlineData(3, 4, ClientBranch.Classic)]
    [InlineData(4, 4, ClientBranch.Classic)]  // Cataclysm Classic 4.4.2 — the issue #202 client
    [InlineData(0, 0, ClientBranch.Unknown)]
    public void GetBranch_SplitsEachExpansionAtItsClassicThreshold(byte expansion, byte major, ClientBranch expected)
        => Assert.Equal(expected, VersionChecker.GetBranch(expansion, major));

    [Fact]
    public void CataclysmClassic_TakesTheClassicTriple_NotTheRetailOne()
    {
        // The old chain had arms for expansion 1, 2 and 3 only, so a 4.4.x client fell through to
        // `else` and silently took the retail version triple at all 53 ranged call sites (#202).
        Assert.Equal(LegacyArm.Retail, OldChain(expansion: 4));
        Assert.Equal(LegacyArm.Classic, NewChain(VersionChecker.GetBranch(4, 4)));
    }

    [Fact]
    public void RawBuildOrdering_IsMeaninglessAcrossBranches()
    {
        // Why AssertComparableBranch exists: ClientVersionBuild is valued by raw build number, so
        // Wrath Classic outranks every original-line build from Cataclysm through Shadowlands.
        Assert.True((int)ClientVersionBuild.V3_4_3_54261 > (int)ClientVersionBuild.V4_3_4_15595);
        Assert.True((int)ClientVersionBuild.V3_4_3_54261 > (int)ClientVersionBuild.V9_1_5_42010);

        Assert.Equal(ClientBranch.Classic, VersionChecker.GetBranch(3, 4));
        Assert.Equal(ClientBranch.Retail, VersionChecker.GetBranch(4, 3));
        Assert.Equal(ClientBranch.Retail, VersionChecker.GetBranch(9, 1));
    }

    [Fact]
    public void EverySupportedBuildResolvesToAKnownBranch()
    {
        var unknown = AllBuilds
            .Where(b => VersionChecker.IsSupportedModernVersion(b) || VersionChecker.IsSupportedLegacyVersion(b))
            .Where(b => { var (e, m) = Parse(b); return VersionChecker.GetBranch(e, m) == ClientBranch.Unknown; })
            .Select(b => b.ToString())
            .ToList();

        Assert.True(unknown.Count == 0, $"Supported builds with no branch: {string.Join(", ", unknown)}");
    }
}
