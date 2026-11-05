using System;
using System.Numerics;

namespace HermesProxy.World.Client;

/// <summary>
/// An inspected player's talents from a 2.4.3 server, as the 2.5 clients read them.
/// </summary>
/// <remarks>
/// A 2.4.3 server answers an inspect with a bit array: one bit per learned talent, for the rank the
/// player has (cMaNGOS TBC HandleInspectOpcode). A class's three tabs follow one another, and inside
/// a tab the talents go by row, column and id, each taking as many positions as it has ranks. The
/// server packs seven positions to a byte and leaves the high bit clear. The Talent and TalentTab
/// tables of the 2.5.2 and 2.5.3 clients give every talent the position 2.4.3's give it, and the
/// clients read the array eight positions to a byte, so only the packing changes.
/// </remarks>
internal static class InspectTalentBits
{
    /// <summary>200 positions, more than any class has: a warrior's talents take 199.</summary>
    public const int ModernLength = 25;

    public static void FromLegacy(ReadOnlySpan<byte> legacy, Span<byte> modern)
    {
        int positions = modern.Length * 8;
        for (int i = 0; i < legacy.Length; i++)
        {
            int packed = legacy[i] & 0x7F;
            while (packed != 0)
            {
                int position = i * 7 + BitOperations.TrailingZeroCount(packed);
                if (position >= positions)
                    return;

                modern[position >> 3] |= (byte)(1 << (position & 7));
                packed &= packed - 1;
            }
        }
    }
}
