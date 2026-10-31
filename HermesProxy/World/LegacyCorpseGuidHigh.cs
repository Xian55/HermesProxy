using System.Runtime.CompilerServices;
using System.Threading;
using HermesProxy.World.Enums;

namespace HermesProxy.World;

/// <summary>
/// The high-guid part the legacy backend uses for corpses, learned from what it sends, as
/// <see cref="LegacyItemGuidHigh"/> learns items'. CMaNGOS uses <c>HIGHGUID_CORPSE = 0xF50</c>, so
/// its corpses arrive as <c>0xF500…</c>; a corpse guid sent back has to carry that high.
/// </summary>
public static class LegacyCorpseGuidHigh
{
    private static int _current = (int)HighGuidTypeLegacy.Corpse;

    /// <summary>The high to rebuild corpse guids with. Defaults to the standard 0xF101.</summary>
    public static HighGuidTypeLegacy Current
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (HighGuidTypeLegacy)Volatile.Read(ref _current);
    }

    /// <summary>Records the high a corpse guid arrived with. Once 0xF500 has been seen it stays.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Observe(HighGuidTypeLegacy high)
    {
        if (high == HighGuidTypeLegacy.CorpseMangos)
            Volatile.Write(ref _current, (int)HighGuidTypeLegacy.CorpseMangos);
    }

    /// <summary>Restores the 0xF101 default. For tests, which share one process.</summary>
    internal static void Reset() => Volatile.Write(ref _current, (int)HighGuidTypeLegacy.Corpse);
}
