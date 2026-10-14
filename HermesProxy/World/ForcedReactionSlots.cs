using System;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World;

/// <summary>
/// Forced reactions for a client that has no packet for them.
///
/// A forced reaction makes a faction treat the player as a given standing would, whatever the
/// real one is: a disguise, mostly. The older clients are told with SMSG_SET_FORCED_REACTIONS,
/// which carries the whole list every time. The 4.4.2 client has no such packet; it reads them
/// off its own player object, which has room for <see cref="Count"/> of them, and is told of a
/// change slot by slot.
///
/// The slots are kept the way a server of that client keeps them (TrinityCore cata_classic,
/// <c>Player::SetVisibleForcedReaction</c> / <c>RemoveVisibleForcedReaction</c>): a faction stays
/// in the slot it took, a new one takes the first empty slot, and one that ends leaves its slot
/// empty, faction 0.
/// </summary>
public static class ForcedReactionSlots
{
    /// <summary>How many forced reactions the player object holds.</summary>
    public const int Count = 32;

    /// <summary>
    /// Makes <paramref name="slots"/> say what the server's list says. Returns one bit per slot
    /// that changed. A list longer than the slots loses what does not fit, as it does on a server
    /// of the client's own.
    /// </summary>
    public static uint Apply(Span<ForcedReaction> slots, ReadOnlySpan<ForcedReaction> wanted)
    {
        uint changed = 0;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].Faction == 0 || IndexOf(wanted, slots[i].Faction) >= 0)
                continue;
            slots[i] = default;
            changed |= 1u << i;
        }

        foreach (ForcedReaction reaction in wanted)
        {
            // faction 0 is how the client knows an empty slot
            if (reaction.Faction == 0)
                continue;

            int slot = IndexOf(slots, reaction.Faction);
            if (slot < 0)
                slot = IndexOf(slots, 0);
            if (slot < 0)
                continue;
            if (slots[slot].Faction == reaction.Faction && slots[slot].Reaction == reaction.Reaction)
                continue;

            slots[slot] = reaction;
            changed |= 1u << slot;
        }

        return changed;
    }

    /// <summary>The slots as a new player object carries them; none at all is every slot empty.</summary>
    public static void WriteCreate(WorldPacket data, ReadOnlySpan<ForcedReaction> slots)
    {
        for (int i = 0; i < Count; i++)
        {
            ForcedReaction slot = i < slots.Length ? slots[i] : default;
            data.WriteInt32(slot.Faction);
            data.WriteInt32(slot.Reaction);
        }
    }

    /// <summary>One slot as a change of the player object carries it: both of its fields, always.</summary>
    public static void WriteUpdate(WorldPacket data, ForcedReaction slot)
    {
        data.WriteBits(0x07u, 3);
        data.FlushBits();
        data.WriteInt32(slot.Faction);
        data.WriteInt32(slot.Reaction);
    }

    private static int IndexOf(ReadOnlySpan<ForcedReaction> reactions, int faction)
    {
        for (int i = 0; i < reactions.Length; i++)
        {
            if (reactions[i].Faction == faction)
                return i;
        }
        return -1;
    }
}
