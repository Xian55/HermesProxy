using System.Collections.Generic;

namespace HermesProxy.World.Server;

/// <summary>
/// An LFG queue slot packs the dungeon type into the high byte and the LFGDungeons ID into
/// the low 24 bits.
/// </summary>
public static class LfgSlots
{
    private const uint DungeonIdMask = 0xFFFFFF;

    // 3.3.5a LFGDungeons.dbc tops out well below this. V3_4_3 added Titan Rune Protocol
    // (2447 / 2470 / 2485) and remapped those headers' children into the 2400s. A legacy
    // backend drops CMSG_LFG_JOIN for those IDs with no reply, hanging the DF UI.
    //
    // Do not whitelist against SMSG_LFG_PLAYER_INFO: that packet only lists randoms +
    // locks. Eligible specific dungeons are implicit, so a "not in that set" check
    // rejected every Specific Dungeons queue (issue #103).
    public const uint MaxLegacyDungeonId = 512;

    public const uint LfgTypeDungeon = 1;
    public const uint LfgTypeRandom = 6;

    public static uint GetDungeonId(uint slot) => slot & DungeonIdMask;

    public static uint PackSlot(uint type, uint dungeonId) => (type << 24) | (dungeonId & DungeonIdMask);

    public static bool IsLegacyDungeon(uint dungeonId) => dungeonId <= MaxLegacyDungeonId;

    public static uint TypeForUnserviceable(uint dungeonId) =>
        dungeonId is 2447 or 2470 or 2485 ? LfgTypeRandom : LfgTypeDungeon;

    // Titan Rune Gamma 2447-2463, Beta 2470-2483, Alpha 2485-2497. The only
    // LFGDungeons.db2 rows above MaxLegacyDungeonId on 3.4.3.54261.
    public static IEnumerable<uint> EnumerateUnserviceableDungeonIds()
    {
        for (uint id = 2447; id <= 2463; id++)
            yield return id;
        for (uint id = 2470; id <= 2483; id++)
            yield return id;
        for (uint id = 2485; id <= 2497; id++)
            yield return id;
    }

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
