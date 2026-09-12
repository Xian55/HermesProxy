namespace HermesProxy.Enums;

/// <summary>
/// The release line a <see cref="ClientVersionBuild"/> belongs to.
/// </summary>
/// <remarks>
/// Build numbers are only comparable inside one branch. <see cref="ClientVersionBuild"/> is valued
/// by raw build number, so <c>V3_4_3_54261</c> (54261) sorts above <c>V4_3_4_15595</c> (15595) even
/// though 3.4.3 is the older game — any ordered comparison that crosses lines answers nonsense.
/// Expansion/major/minor comparisons are parsed from the enum *name* and stay correct across
/// branches; raw-build comparisons do not, which is why the build-taking overloads on
/// <c>ModernVersion</c> are obsolete.
/// <para>
/// The three branches mirror the three version triples the ranged predicates have always taken
/// (retail, classic-era, classic), so this names an existing discrimination rather than adding one.
/// </para>
/// </remarks>
public enum ClientBranch : byte
{
    /// <summary>Not determined — only for an unset build.</summary>
    Unknown = 0,

    /// <summary>
    /// The original progression: 1.12.x through 4.3.4, and 5.x onward. Everything the proxy talks
    /// to on the legacy side is on this line.
    /// </summary>
    Retail,

    /// <summary>Classic Era and Season of Mastery — 1.13.x, 1.14.x, 1.15.x.</summary>
    ClassicEra,

    /// <summary>
    /// The re-released expansion line — TBC Classic 2.5.x, Wrath Classic 3.4.x,
    /// Cataclysm Classic 4.4.x.
    /// </summary>
    Classic,
}
