using System.Collections.Generic;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server;

/// <summary>
/// An LFG queue slot packs the dungeon type into the high byte and the LFGDungeons ID into
/// the low 24 bits.
/// </summary>
public static class LfgSlots
{
    private const uint DungeonIdMask = 0xFFFFFF;

    // 3.3.5a LFGDungeons.dbc tops out well below this: 195 rows, highest ID 262. The
    // ceiling is deliberate slack rather than the table size, so a legacy ID we never
    // enumerated still forwards. V3_4_3 added Titan Rune Protocol above it, and a legacy
    // backend drops CMSG_LFG_JOIN for those IDs with no reply, hanging the DF UI.
    //
    // Do not whitelist against SMSG_LFG_PLAYER_INFO: that packet only lists randoms +
    // locks. Eligible specific dungeons are implicit, so a "not in that set" check
    // rejected every Specific Dungeons queue (issue #103).
    public const uint MaxLegacyDungeonId = 512;

    public const uint LfgTypeDungeon = 1;
    public const uint LfgTypeRandom = 6;

    // Titan Rune Protocol, added to LFGDungeons.db2 in 3.4.3.54261 and absent from 3.3.5a.
    // Each category is a header row followed by a contiguous block of child dungeons, so a
    // header ID doubles as the first ID of its block. The header is what the client sends
    // when the category itself is picked, which is why it queues as LfgTypeRandom.
    public const uint TitanRuneGammaHeaderId = 2447;
    public const uint TitanRuneGammaLastDungeonId = 2463;
    public const uint TitanRuneBetaHeaderId = 2470;
    public const uint TitanRuneBetaLastDungeonId = 2483;
    public const uint TitanRuneAlphaHeaderId = 2485;
    public const uint TitanRuneAlphaLastDungeonId = 2497;

    public static uint GetDungeonId(uint slot) => slot & DungeonIdMask;

    public static uint PackSlot(uint type, uint dungeonId) => (type << 24) | (dungeonId & DungeonIdMask);

    public static bool IsLegacyDungeon(uint dungeonId) => dungeonId <= MaxLegacyDungeonId;

    public static uint TypeForUnserviceable(uint dungeonId) =>
        dungeonId is TitanRuneGammaHeaderId or TitanRuneBetaHeaderId or TitanRuneAlphaHeaderId
            ? LfgTypeRandom
            : LfgTypeDungeon;

    // The only LFGDungeons.db2 rows above MaxLegacyDungeonId on 3.4.3.54261.
    public static IEnumerable<uint> EnumerateUnserviceableDungeonIds()
    {
        for (uint id = TitanRuneGammaHeaderId; id <= TitanRuneGammaLastDungeonId; id++)
            yield return id;
        for (uint id = TitanRuneBetaHeaderId; id <= TitanRuneBetaLastDungeonId; id++)
            yield return id;
        for (uint id = TitanRuneAlphaHeaderId; id <= TitanRuneAlphaLastDungeonId; id++)
            yield return id;
    }

    /// <summary>
    /// V3_4_3 hides a row when SoftLock is Unk2. Same mapping the player-info
    /// path already uses. Party-info used to write 0, which left Titan Rune /
    /// out-of-range randoms greyed in the Type dropdown instead of gone.
    /// </summary>
    public static uint ToSoftLock(uint lockStatus) => (LFGLockStatus)lockStatus switch
    {
        LFGLockStatus.InsufficientExpansion
        or LFGLockStatus.TooLowLevel
        or LFGLockStatus.TooHighLevel
        or LFGLockStatus.NotInSeason => (uint)LFGSoftLock.Unk2,
        _ => (uint)LFGSoftLock.None,
    };

    // Party-info SoftLock only hides content the backend cannot serve. Level
    // and gear locks stay SoftLock=None so 3.4.3 prints the real per-player
    // reason (e.g. "Caklick must advance to a higher level.") instead of a
    // bare "You may not queue for this dungeon."
    public static uint ToPartySoftLock(uint lockStatus) => (LFGLockStatus)lockStatus switch
    {
        LFGLockStatus.InsufficientExpansion
        or LFGLockStatus.NotInSeason => (uint)LFGSoftLock.Unk2,
        _ => (uint)LFGSoftLock.None,
    };

    // 3.3.5a only sends lock status, not required/current item level. Leaving
    // TooLow/TooHighGearScore on the wire makes 3.4.3 print "Requires: 0.
    // Currently 0." (issue #144). HasRestriction (15) has no FrameXML string
    // and renders INSTANCE_UNAVAILABLE_*_OTHER instead.
    public static uint ToDisplayLockStatus(uint lockStatus) => (LFGLockStatus)lockStatus switch
    {
        LFGLockStatus.TooLowGearScore
        or LFGLockStatus.TooHighGearScore => (uint)LFGLockStatus.HasRestriction,
        _ => lockStatus,
    };

    /// <summary>
    /// Packed slots to inject as SoftLock hide-rows so the 3.4.3 client drops the
    /// Titan Rune Protocol categories from Specific Dungeons. Skips IDs already
    /// present in <paramref name="alreadyListedDungeonIds"/>.
    /// </summary>
    public static List<uint> GetTitanRuneHideSlots(IEnumerable<uint> alreadyListedDungeonIds)
    {
        var have = alreadyListedDungeonIds as HashSet<uint> ?? new HashSet<uint>(alreadyListedDungeonIds);
        var extra = new List<uint>();
        foreach (uint id in EnumerateUnserviceableDungeonIds())
        {
            if (have.Contains(id))
                continue;
            extra.Add(PackSlot(TypeForUnserviceable(id), id));
        }
        return extra;
    }

    // LFGDungeons rows of the 4.4.2.60895 client that TrinityCore 4.3.4's LFGDungeons.dbc (240
    // rows, highest ID 448) does not have: End Time, Well of Eternity and Hour of Twilight under
    // IDs of their own, the Titan Rune Protocol blocks, Rise of the Zandalari and the two Elemental
    // Rune Protocol blocks. A block starts with its random's header row.
    private static readonly (uint First, uint Last)[] CataClientOnlyDungeons =
    [
        (436, 436), (438, 438), (440, 440),
        (TitanRuneGammaHeaderId, TitanRuneGammaLastDungeonId),
        (TitanRuneBetaHeaderId, TitanRuneBetaLastDungeonId),
        (TitanRuneAlphaHeaderId, TitanRuneAlphaLastDungeonId),
        (2735, 2735), (2760, 2769), (2881, 2890), (2894, 2896),
    ];

    private static readonly uint[] CataClientOnlyRandoms =
        [TitanRuneGammaHeaderId, TitanRuneBetaHeaderId, TitanRuneAlphaHeaderId, 2735, 2760, 2881];

    /// <summary>
    /// The same hide-rows for the 4.4.2 client on a 4.3.4 server: every dungeon of the client's
    /// that the server does not have, a random as a random.
    /// </summary>
    public static List<uint> GetCataHideSlots(IEnumerable<uint> alreadyListedDungeonIds)
    {
        var have = alreadyListedDungeonIds as HashSet<uint> ?? new HashSet<uint>(alreadyListedDungeonIds);
        var extra = new List<uint>();
        foreach ((uint first, uint last) in CataClientOnlyDungeons)
        {
            for (uint id = first; id <= last; id++)
            {
                if (have.Contains(id))
                    continue;
                uint type = System.Array.IndexOf(CataClientOnlyRandoms, id) >= 0 ? LfgTypeRandom : LfgTypeDungeon;
                extra.Add(PackSlot(type, id));
            }
        }
        return extra;
    }

    /// <summary>
    /// First requested dungeon the 3.3.5a backend cannot serve. Titan Rune / other
    /// post-3.3.5 LFGDungeons IDs belong here. Real 3.3.5 specifics (Utgarde, Gundrak,
    /// …) do not, even when they never appeared in SMSG_LFG_PLAYER_INFO.
    /// </summary>
    public static bool TryFindUnknownDungeon(IEnumerable<uint> requestedSlots, out uint unknownDungeonId)
    {
        unknownDungeonId = 0;
        foreach (uint slot in requestedSlots)
        {
            uint dungeonId = GetDungeonId(slot);
            if (IsLegacyDungeon(dungeonId))
                continue;

            unknownDungeonId = dungeonId;
            return true;
        }

        return false;
    }
}
