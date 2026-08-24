using System.Collections.Generic;
using Framework.Logging;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server;

public readonly record struct CharacterListSlot(ulong GuidLow, byte ListPosition);

/// <summary>
/// Applies a saved character-list order onto an enum result. 3.3.5a has no
/// CMSG_REORDER_CHARACTERS, so the proxy stores the modern client's order and
/// stamps ListPosition (and array order) on the next SMSG_ENUM_CHARACTERS_RESULT.
/// V3_4_3 sends positions 10,20,30… not 0,1,2 — compacting to 0-based made
/// the first character (ListPosition=0) render last.
/// </summary>
public static class CharacterListOrder
{
    public const byte PositionStep = 10;

    public static List<CharacterListSlot> Normalize(IReadOnlyList<CharacterListSlot> slots)
    {
        var ordered = new List<CharacterListSlot>(slots);
        ordered.Sort((a, b) =>
        {
            int byPos = a.ListPosition.CompareTo(b.ListPosition);
            return byPos != 0 ? byPos : a.GuidLow.CompareTo(b.GuidLow);
        });
        var normalized = new List<CharacterListSlot>(ordered.Count);
        for (int i = 0; i < ordered.Count; i++)
            normalized.Add(new CharacterListSlot(ordered[i].GuidLow, PositionAt(i)));
        return normalized;
    }

    /// <summary>
    /// Drops saved slots whose GUID is not in the live enum. Keeps the file
    /// bounded by the realm character cap and stops a reused GUID from
    /// inheriting a deleted character's slot
    /// </summary>
    public static List<CharacterListSlot> Prune(IReadOnlyList<CharacterListSlot> saved, IReadOnlyList<EnumCharactersResult.CharacterInfo> characters)
    {
        if (saved.Count == 0)
            return new List<CharacterListSlot>();
        if (characters.Count == 0)
            return new List<CharacterListSlot>(saved);

        var live = new HashSet<ulong>(characters.Count);
        foreach (var character in characters)
            live.Add(character.Guid.Low);

        var pruned = new List<CharacterListSlot>(saved.Count);
        foreach (var slot in saved)
        {
            if (live.Contains(slot.GuidLow))
                pruned.Add(slot);
        }
        return pruned;
    }

    /// <summary>
    /// Stamps the live enum with the saved order. Returns the pruned save so
    /// the caller can persist it when deleted characters were dropped
    /// </summary>
    public static List<CharacterListSlot> Apply(List<EnumCharactersResult.CharacterInfo> characters, IReadOnlyList<CharacterListSlot> saved)
    {
        var pruned = Prune(saved, characters);
        if (characters.Count == 0 || pruned.Count == 0)
            return pruned;

        byte nextFallback = 0;
        foreach (var slot in pruned)
        {
            if (slot.ListPosition >= nextFallback)
                nextFallback = (byte)(slot.ListPosition + 1);
        }

        foreach (var character in characters)
        {
            bool found = false;
            foreach (var slot in pruned)
            {
                if (slot.GuidLow != character.Guid.Low)
                    continue;
                character.ListPosition = slot.ListPosition;
                found = true;
                break;
            }
            if (!found)
                character.ListPosition = nextFallback++;
        }

        characters.Sort((a, b) => a.ListPosition.CompareTo(b.ListPosition));
        for (int i = 0; i < characters.Count; i++)
            characters[i].ListPosition = PositionAt(i);
        return pruned;
    }

    // ListPosition is a byte; 26+ rows would wrap (260 -> 4) and scramble the list
    public static byte PositionAt(int index)
    {
        int raw = (index + 1) * PositionStep;
        if (raw > byte.MaxValue)
        {
            Log.Print(LogType.Warn, $"character list position {raw} exceeds {byte.MaxValue}, clamping");
            return byte.MaxValue;
        }
        return (byte)raw;
    }

    /// <summary>
    /// The client often sends only the characters whose slot changed, with
    /// in-between positions (15, 17, …). Overlay those onto the last full
    /// save, then snap back to 10,20,30… so the next drag has a clean grid.
    /// </summary>
    public static List<CharacterListSlot> Merge(IReadOnlyList<CharacterListSlot> existing, IReadOnlyList<CharacterListSlot> incoming)
    {
        var merged = new List<CharacterListSlot>(existing);
        foreach (var update in incoming)
        {
            int idx = merged.FindIndex(s => s.GuidLow == update.GuidLow);
            if (idx >= 0)
                merged[idx] = update;
            else
                merged.Add(update);
        }
        return Normalize(merged);
    }
}
