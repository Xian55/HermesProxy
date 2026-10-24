using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using BNetServer;

namespace HermesProxy.World.Client;

/// <summary>
/// Where each party's world markers are, for every session of this proxy. A 4.3.4 server sends a
/// party only the mask of its markers; the places are in the markers' dynamic objects, which reach
/// only the members near one. A member far from a marker, or one who joins after it was put down,
/// takes its place from a member who saw it (#415).
/// </summary>
/// <remarks>
/// Only parties whose members come through this proxy are helped; a marker nobody here has seen is
/// still left out.
/// </remarks>
internal static class RaidMarkerBoard
{
    private sealed class Party
    {
        public readonly Lock Lock = new();
        public readonly (uint Map, Vector3 Place)?[] Slots = new (uint Map, Vector3 Place)?[8];
        // Dropped from the board; a writer that still holds it takes a fresh one.
        public bool Retired;
    }

    private static readonly ConcurrentDictionary<WowGuid128, Party> Parties = new();

    /// <summary>A member saw this marker: kept, and every other session is asked to take it.</summary>
    public static void Set(WowGuid128 party, int slot, uint map, Vector3 place, GlobalSessionData from)
    {
        while (true)
        {
            Party entry = Parties.GetOrAdd(party, static _ => new Party());
            lock (entry.Lock)
            {
                if (entry.Retired)
                    continue;
                entry.Slots[slot] = (map, place);
                break;
            }
        }

        // Each session checks its own party on its own executor; another session's state is not
        // read from here.
        foreach (GlobalSessionData session in BnetSessionTicketStorage.SessionsByName.Values)
        {
            if (ReferenceEquals(session, from) || session.WorldClient is not { } client || !client.IsConnected())
                continue;
            session.Executor.Post(static state => ((WorldClient)state!).TakeRaidMarkersFromBoard(), client);
        }
    }

    public static bool TryGet(WowGuid128 party, int slot, out uint map, out Vector3 place)
    {
        map = 0;
        place = default;
        if (!Parties.TryGetValue(party, out Party? entry))
            return false;
        lock (entry.Lock)
        {
            if (entry.Slots[slot] is not { } known)
                return false;
            (map, place) = known;
            return true;
        }
    }

    /// <summary>The server said this marker is gone. A party left without markers is dropped.</summary>
    public static void Clear(WowGuid128 party, int slot)
    {
        if (!Parties.TryGetValue(party, out Party? entry))
            return;
        lock (entry.Lock)
        {
            entry.Slots[slot] = null;
            foreach (var known in entry.Slots)
            {
                if (known != null)
                    return;
            }
            entry.Retired = true;
            Parties.TryRemove(KeyValuePair.Create(party, entry));
        }
    }
}
