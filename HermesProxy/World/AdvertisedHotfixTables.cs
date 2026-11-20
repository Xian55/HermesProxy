using System.Collections.Frozen;

using HermesProxy.Enums;
using HermesProxy.World.Enums;

namespace HermesProxy.World;

/// <summary>
/// The DB2 tables whose hotfix records a client build is told about in
/// <c>SMSG_AVAILABLE_HOTFIXES</c>. The one place that decision is made: the packet filters on it
/// at login, and loaders consult it at startup so they do not build records the client can never
/// ask for (issue #187).
/// </summary>
/// <remarks>
/// <para>
/// 3.4.3 and later advertise only tables where the proxy actually overrides the client's own
/// data. Hotfixes are an override channel — a record identical to the baked-in DB2 costs a
/// round trip and changes nothing.
/// </para>
/// <para>
/// ChrCustomizationChoice (1064 rows) and ChrCustomizationOption (190) used to be advertised.
/// Both were verified byte-identical to the client's 3.4.3.54261 DB2s (1064/1064 and 190/190,
/// zero differing, zero new), i.e. 1254 wasted records per login. Neither store is read anywhere
/// else in the proxy.
/// </para>
/// <para>
/// ItemExtendedCost holds only the rows where the server's vendor prices differ from the
/// client's, CurrencyTypes a 4.3.4 server's weekly caps, GlobalStrings the 4.4.2 client's honor
/// lines in front of a 3.3.5a server.
/// </para>
/// <para>
/// Not being advertised is not the same as being unreachable. <c>GameData.UpdateHotfix</c> finds
/// item records by record id from <c>HotfixItemBegin</c> on, and
/// <c>GameData.GenerateHotFixMessage</c> pushes what it finds in <c>SMSG_HOTFIX_MESSAGE</c>
/// without consulting this filter. <c>ItemEffect</c> is skipped anyway because that path always
/// rewrites a record's content before sending it, so a record loaded from CSV gave it nothing but
/// a hotfix id. Only skip a table's records once every reader of <c>GameData.Hotfixes</c> that
/// can reach it has been ruled out.
/// </para>
/// </remarks>
public static class AdvertisedHotfixTables
{
    private static readonly FrozenSet<DB2Hash> WotLKClassicOrLater = FrozenSet.Create(
        DB2Hash.AreaTrigger,
        DB2Hash.ItemExtendedCost,
        DB2Hash.CurrencyTypes,
        DB2Hash.GlobalStrings);

    /// <summary>
    /// The advertised tables for <paramref name="build"/>, or <c>null</c> when every loaded
    /// record is advertised (1.14 and 2.5 builds).
    /// </summary>
    public static FrozenSet<DB2Hash>? For(ClientVersionBuild build)
        => VersionChecker.IsWotLKClassicOrLater(build) ? WotLKClassicOrLater : null;

    /// <summary>Whether <paramref name="build"/> is ever told about <paramref name="table"/>'s records.</summary>
    public static bool IsAdvertised(DB2Hash table, ClientVersionBuild build)
        => For(build) is not { } tables || tables.Contains(table);
}
