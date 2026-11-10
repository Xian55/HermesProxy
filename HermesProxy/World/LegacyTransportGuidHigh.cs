using System.Runtime.CompilerServices;
using System.Threading;
using Framework.Logging;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;

namespace HermesProxy.World;

/// <summary>
/// The high-guid part the legacy backend uses for elevators and trams (gameobject type 11),
/// learned from what it sends, as <see cref="LegacyMoTransportGuidHigh"/> learns the one for ships.
///
/// <para>
/// <c>HIGHGUID_TRANSPORT</c> is <c>0xF120</c> on every core that gives these objects a high of
/// their own, except cMaNGOS classic, which numbers them <c>0x6000</c> since mangos-classic
/// 85e06ac9d (2026-11-06). Before that commit it sent them as plain gameobjects, <c>0xF110</c>,
/// which never come through here. As with ships, servers from both sides of the commit report the
/// same build. A guid sent back under the other high names nothing on the server: it never takes
/// the rider aboard.
/// </para>
/// </summary>
public static class LegacyTransportGuidHigh
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melServer =
        Log.CreateMelLogger(Log.CategoryServer);

    // Process-global for the reason LegacyItemGuidHigh gives: WowGuid64.Create has no session.
    private static int _current = (int)HighGuidTypeLegacy.Transport;

    /// <summary>The high to rebuild elevator and tram guids with. Defaults to 0xF120.</summary>
    public static HighGuidTypeLegacy Current
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (HighGuidTypeLegacy)Volatile.Read(ref _current);
    }

    /// <summary>
    /// Records the high an elevator or tram guid arrived with. Follows the last one seen, for the
    /// reasons <see cref="LegacyMoTransportGuidHigh.Observe"/> gives.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Observe(HighGuidTypeLegacy high)
    {
        if ((int)high != Volatile.Read(ref _current))
            Learn(high);
    }

    // Off the inlined path: this runs once per backend, on the first elevator it sends.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Learn(HighGuidTypeLegacy high)
    {
        // Two sessions reading their first elevator can both get here; only the one that
        // changes the value logs.
        if (Interlocked.Exchange(ref _current, (int)high) != (int)high)
            GuidLogMessages.LearnedLegacyTransportHigh(_melServer, (uint)high);
    }

    /// <summary>Restores the 0xF120 default. For tests, which share one process.</summary>
    internal static void Reset() => Volatile.Write(ref _current, (int)HighGuidTypeLegacy.Transport);
}
