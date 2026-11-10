using System.Runtime.CompilerServices;
using System.Threading;
using Framework.Logging;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;

namespace HermesProxy.World;

/// <summary>
/// The high-guid part the legacy backend uses for ships and zeppelins, learned from what it sends,
/// as <see cref="LegacyItemGuidHigh"/> learns items'.
///
/// <para>
/// Every core used <c>HIGHGUID_MO_TRANSPORT = 0x1FC0</c> until cMaNGOS classic moved to
/// <c>0x8000</c> (mangos-classic d9d50349b, 2026-11-06). Servers built before and after that
/// commit are both in use, and the build a server reports is the same for both, so the value
/// cannot be told from the version. A guid sent back under the other high names nothing on the
/// server: boarding and every move on deck would miss (issue #590).
/// </para>
/// </summary>
public static class LegacyMoTransportGuidHigh
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melServer =
        Log.CreateMelLogger(Log.CategoryServer);

    // Process-global for the reason LegacyItemGuidHigh gives: WowGuid64.Create has no session.
    private static int _current = (int)HighGuidTypeLegacy.MOTransport;

    /// <summary>The high to rebuild ship and zeppelin guids with. Defaults to 0x1FC0.</summary>
    public static HighGuidTypeLegacy Current
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (HighGuidTypeLegacy)Volatile.Read(ref _current);
    }

    /// <summary>
    /// Records the high a ship or zeppelin guid arrived with. Follows the last one seen instead
    /// of latching as the item high does: the proxy builds these guids from <see cref="Current"/>
    /// alone, so nothing of its own can flip the value, and a backend replaced by a build from
    /// the other side of the commit is picked up without restarting the proxy.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Observe(HighGuidTypeLegacy high)
    {
        if ((int)high != Volatile.Read(ref _current))
            Learn(high);
    }

    // Off the inlined path: this runs once per backend, on the first transport it sends.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Learn(HighGuidTypeLegacy high)
    {
        // Two sessions reading their first transport can both get here; only the one that
        // changes the value logs.
        if (Interlocked.Exchange(ref _current, (int)high) != (int)high)
            GuidLogMessages.LearnedLegacyMoTransportHigh(_melServer, (uint)high);
    }

    /// <summary>Restores the 0x1FC0 default. For tests, which share one process.</summary>
    internal static void Reset() => Volatile.Write(ref _current, (int)HighGuidTypeLegacy.MOTransport);
}
