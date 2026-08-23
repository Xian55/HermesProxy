using System.Collections.Generic;

namespace HermesProxy.World.Server;

/// <summary>
/// An LFG queue slot packs the dungeon type into the high byte and the LFGDungeons ID into
/// the low 24 bits.
/// </summary>
public static class LfgSlots
{
    private const uint DungeonIdMask = 0xFFFFFF;

    public static uint GetDungeonId(uint slot) => slot & DungeonIdMask;

    /// <summary>
    /// Finds the first requested dungeon the legacy backend has never mentioned. The V3_4_3
    /// client offers content that postdates 3.3.5a, and legacy servers drop a join naming an
    /// unknown dungeon without replying at all, hanging the client's Dungeon Finder UI.
    /// Returns false when <paramref name="knownDungeonIds"/> is empty, since an unpopulated
    /// set means SMSG_LFG_PLAYER_INFO has not arrived yet rather than that nothing is valid.
    /// </summary>
    public static bool TryFindUnknownDungeon(HashSet<uint> knownDungeonIds, IEnumerable<uint> requestedSlots, out uint unknownDungeonId)
    {
        unknownDungeonId = 0;
        if (knownDungeonIds.Count == 0)
            return false;

        foreach (uint slot in requestedSlots)
        {
            uint dungeonId = GetDungeonId(slot);
            if (knownDungeonIds.Contains(dungeonId))
                continue;

            unknownDungeonId = dungeonId;
            return true;
        }

        return false;
    }
}
