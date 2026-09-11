using System.Runtime.CompilerServices;
using System.Threading;
using Framework.Logging;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;

namespace HermesProxy.World;

/// <summary>
/// The high-guid part the legacy backend uses for items, learned from what it sends.
///
/// <para>
/// Item guids are the one case where legacy cores disagree. TrinityCore, AzerothCore, cMaNGOS
/// classic, cMaNGOS TBC and VMaNGOS all use <c>HIGHGUID_ITEM = 0x4000</c>, but cMaNGOS WotLK
/// uses <c>HIGHGUID_ITEM = HIGHGUID_CONTAINER = 0x470</c> (mangos-wotlk
/// <c>src/game/Entities/ObjectGuid.h:61</c>), so its items arrive as <c>0x4700…</c>.
/// </para>
///
/// <para>
/// The proxy already accepted both on the way in — <see cref="HighGuid.FromLegacy"/> maps
/// ItemContainer to Item — but rebuilt every item as 0x4000 on the way back, so every client
/// packet naming an item by guid missed on cMaNGOS WotLK. Its handlers switch on the guid's
/// high part (<c>GetObjectByTypeMask</c>, <c>GetItemByGuid</c>), find nothing under 0x4000, and
/// silently do nothing. That is issue #278: right-clicking a quest-starting item such as the
/// Riding Training Pamphlet never opened its quest, and selling, repairing, mailing and
/// socketing were all predicted to fail the same way.
/// </para>
///
/// <para>
/// Learning it beats gating on the backend build: the value is a property of the core, not of
/// the version it reports, and forks are free to differ. Cores that use 0x4000 never send
/// 0x4700, so they never leave the default and need no version gate.
/// </para>
/// </summary>
public static class LegacyItemGuidHigh
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melServer =
        Log.CreateMelLogger(Log.CategoryServer);

    // Process-global rather than per-session: WowGuid64.Create(WowGuid128) is static and has no
    // session to consult, and roughly 146 To64() call sites would have to thread one through to
    // change that. One proxy process serves one configured backend, so the value is the same for
    // every session in it.
    //
    // Held as int because Interlocked works on int, not on an enum-typed field.
    private static int _current = (int)HighGuidTypeLegacy.Item;

    /// <summary>The high to rebuild item guids with. Defaults to the standard 0x4000.</summary>
    public static HighGuidTypeLegacy Current
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (HighGuidTypeLegacy)Volatile.Read(ref _current);
    }

    /// <summary>
    /// Records the high an item guid arrived with. Latches: once 0x4700 has been seen it stays,
    /// and a later 0x4000 never flips it back. A core's item high is fixed, so one sighting
    /// settles it — while proxy-built item guids (which use the 0x4000 default) would otherwise
    /// flip the value back and forth mid-session and make the rebuilt guid depend on whichever
    /// item was converted last.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Observe(HighGuidTypeLegacy high)
    {
        if (high != HighGuidTypeLegacy.ItemContainer)
            return;

        if (Volatile.Read(ref _current) != (int)HighGuidTypeLegacy.ItemContainer)
            Latch();
    }

    // Off the inlined path: this runs once per session, on the first item the backend sends.
    //
    // The compare-and-swap is what keeps it once: the fast-path check above is a plain read, so
    // two receive threads decoding the first item batch can both reach here, and only the one
    // that actually flips the value logs. Both would write the same value, so the swap is about
    // the log line and not about correctness of the stored high.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Latch()
    {
        int previous = Interlocked.CompareExchange(
            ref _current,
            (int)HighGuidTypeLegacy.ItemContainer,
            (int)HighGuidTypeLegacy.Item);

        if (previous == (int)HighGuidTypeLegacy.Item)
            GuidLogMessages.LearnedLegacyItemHigh(_melServer, (uint)HighGuidTypeLegacy.ItemContainer);
    }

    /// <summary>Restores the 0x4000 default. For tests, which share one process.</summary>
    internal static void Reset() => Volatile.Write(ref _current, (int)HighGuidTypeLegacy.Item);
}
