using System.Collections.Frozen;
using System.Linq;

namespace HermesProxy.World.Server;

// V3_4_3 client / 3.3.5a server AreaTrigger ID drift reconciliation.
//
// Cataclysm renumbered most static AreaTrigger.dbc IDs and dropped many of the
// pre-BC walk-through triggers. The 2.5.3/3.4.3 Classic clients ship the
// post-Cataclysm DB2, while 3.3.5a server emulators (TC, cMaNGOS, …) keep the
// original WotLK-era IDs in `areatrigger_teleport`. Two failure modes show up:
//
//   1. Modern client fires CMSG_AREA_TRIGGER with a renumbered id (e.g. 4356)
//      that the legacy server doesn't know about. → remap to the legacy id.
//   2. Modern client has no static entry on this map at all (e.g. the Blasted
//      Lands Dark Portal vanished from AreaTrigger.db2 entirely). → synthesize
//      CMSG_AREA_TRIGGER from movement position.
internal static class AreaTriggerReconciliation
{
    internal readonly record struct Entry(
        uint LegacyId,
        uint? ModernId,
        uint MapId,
        Vector3 Center,
        float Radius);

    private static readonly Entry[] All =
    [
        // Dark Portal — Blasted Lands → Outland. No entry in 2.5.3/3.4.3
        // AreaTrigger.db2 (verified empirically). Center placed at the portal
        // frame's footprint (published 3.3.5a coords ~(-11898,-3210,-22) plus
        // empirical observation of the player walking the platform at Z ~ -16).
        // Generous 30-unit radius — covers walking through from either approach.
        new(LegacyId: 4354, ModernId: null, MapId: 0,
            Center: new Vector3(-11900f, -3210f, -16f),
            Radius: 30f),

        // Dark Portal — Outland → Blasted Lands. TC 3.3.5a's
        // areatrigger_teleport row 4352 ("Dark Portal - E. Kingdoms Target")
        // is the correct legacy ID — *not* 4524, which is what public sniff
        // data and the modern DB2 use. The 2.5.3/3.4.3 DB2 has trigger 4356
        // at (-248, 1042, 54) R=50 but its AreaTriggerActionSet 26557 is a
        // dangling reference — the V3_4_3 client never fires it anyway.
        // ModernId=4356 kept as defensive remap; the real path is proximity
        // synth on the south face of the Outland portal frame. Tight radius
        // so the BL→Outland spawn at (-248, 922.9, 84) sits OUTSIDE the
        // sphere — otherwise we'd auto-re-teleport on arrival.
        new(LegacyId: 4352, ModernId: 4356, MapId: 530,
            Center: new Vector3(-248f, 905f, 84f),
            Radius: 10f),
    ];

    internal static readonly FrozenDictionary<uint, uint> ModernToLegacy =
        All.Where(e => e.ModernId is not null)
           .ToFrozenDictionary(e => e.ModernId!.Value, e => e.LegacyId);

    // All entries with a positive radius get proximity synthesis, regardless
    // of whether they also have a modern remap id. Multiple sends of the same
    // legacy id are idempotent server-side (volume check + teleport).
    internal static readonly FrozenDictionary<uint, Entry[]> ProximityByMap =
        All.Where(e => e.Radius > 0f)
           .GroupBy(e => e.MapId)
           .ToFrozenDictionary(g => g.Key, g => g.ToArray());
}
