using System;
using System.Collections.Generic;
using System.Threading;

namespace HermesProxy.World;

/// <summary>
/// Gives a greeting the legacy server sends as text the BroadcastText id the modern client will
/// know it by, and the same id on every run.
/// </summary>
/// <remarks>
/// A 3.3.5a server sends an NPC greeting as text. The modern client wants an id, asks for the row
/// behind it once, and keeps the answer in its own cache across sessions. An id that named another
/// text during an earlier proxy run therefore shows that earlier text.
/// <para>
/// A text in <c>CSV/BroadcastTexts{expansion}.csv</c> keeps its real id. Any other text gets an
/// id derived from the text itself: a translation, a private server's own greeting, a row the
/// file lacks. It no longer matters which NPC was talked to first. Nothing is sent for either
/// kind until the client asks.
/// </para>
/// </remarks>
internal sealed class BroadcastTextRegistry
{
    /// <summary>
    /// First derived id: above every id a legacy database uses, below the sign bit. The 1.14.2,
    /// 2.5.3 and 3.4.3 clients were each checked to ask for, and display, a record this high.
    /// </summary>
    internal const uint DerivedIdBase = 0x40000000;
    private const uint DerivedIdMask = 0x3FFFFFFF;

    // One process serves every session, and each session runs on its own thread.
    private readonly Lock _lock = new();
    private readonly Dictionary<uint, GameData.BroadcastText> _texts = [];
    private readonly Dictionary<Key, uint> _ids = [];

    internal BroadcastTextRegistry(IEnumerable<GameData.BroadcastText> known)
    {
        foreach (GameData.BroadcastText text in known)
        {
            _texts.Add(text.Entry, text);

            // A row is found by what the server puts on the wire for it, not by what it stores:
            // the other gender's text when one is empty, trimmed as the handler trims it.
            string male = (text.MaleText.Length != 0 ? text.MaleText : text.FemaleText).TrimEnd();
            string female = (text.FemaleText.Length != 0 ? text.FemaleText : text.MaleText).TrimEnd();
            Key key = new(male, female, text.Language, text.EmoteDelays, text.Emotes);
            if (!_ids.TryGetValue(key, out uint lowest) || text.Entry < lowest)
                _ids[key] = text.Entry;
        }
    }

    /// <summary>
    /// The row behind <paramref name="id"/>, or null for an id this process has not handed out.
    /// </summary>
    internal GameData.BroadcastText? Get(uint id)
    {
        lock (_lock)
            return _texts.GetValueOrDefault(id);
    }

    /// <summary>The id for a greeting, as it arrived in SMSG_NPC_TEXT_UPDATE.</summary>
    internal uint Resolve(string maleText, string femaleText, uint language, ReadOnlySpan<ushort> emoteDelays, ReadOnlySpan<ushort> emotes)
    {
        Key key = new(maleText, femaleText, language, emoteDelays, emotes);

        lock (_lock)
        {
            if (_ids.TryGetValue(key, out uint id))
                return id;

            GameData.BroadcastText text = new()
            {
                MaleText = maleText,
                FemaleText = femaleText,
                Language = language,
                EmoteDelays = [.. emoteDelays],
                Emotes = [.. emotes],
            };

            // Two texts can derive the same id. The later one takes the next free id, so for
            // that pair alone the id still depends on which arrived first.
            id = DerivedIdBase | (key.StableHash() & DerivedIdMask);
            while (!_texts.TryAdd(id, text))
                id = DerivedIdBase | ((id + 1) & DerivedIdMask);

            text.Entry = id;
            _ids.Add(key, id);
            return id;
        }
    }

    private readonly record struct Key(string Male, string Female, uint Language, ulong Delays, ulong Emotes)
    {
        internal Key(string male, string female, uint language, ReadOnlySpan<ushort> delays, ReadOnlySpan<ushort> emotes)
            : this(male, female, language, Pack(delays), Pack(emotes))
        {
        }

        private static ulong Pack(ReadOnlySpan<ushort> values)
            => values[0] | ((ulong)values[1] << 16) | ((ulong)values[2] << 32);

        /// <summary>
        /// FNV-1a over the fields. Not <see cref="GetHashCode"/>: string hashes are seeded per
        /// process, and clients keep the ids this produces, so it has to give the same value on
        /// every run and every machine. Changing it orphans every cached derived id.
        /// </summary>
        internal uint StableHash()
        {
            ulong hash = 14695981039346656037;
            hash = Mix(hash, Male);
            hash = Mix(hash, '\0', 2); // keeps ("ab", "c") apart from ("a", "bc")
            hash = Mix(hash, Female);
            hash = Mix(hash, '\0', 2);
            hash = Mix(hash, Language, 4);
            hash = Mix(hash, Delays, 6);
            hash = Mix(hash, Emotes, 6);
            return (uint)(hash ^ (hash >> 32));
        }

        private static ulong Mix(ulong hash, string text)
        {
            foreach (char unit in text)
                hash = Mix(hash, unit, 2);
            return hash;
        }

        private static ulong Mix(ulong hash, ulong value, int bytes)
        {
            for (int i = 0; i < bytes; i++)
            {
                hash = (hash ^ (byte)value) * 1099511628211;
                value >>= 8;
            }

            return hash;
        }
    }
}
