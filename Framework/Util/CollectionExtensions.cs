/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace System.Collections.Generic;

public static class CollectionExtensions
{
    public static bool Empty<TValue>(this ICollection<TValue> collection)
    {
        return collection.Count == 0;
    }

    /// <summary>
    /// Returns the entry in this dictionary at the given key, or the default value of the key
    /// if none.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="dict">The dictionary to operate on.</param>
    /// <param name="key">The key of the element to retrieve.</param>
    /// <returns>The value (if any).</returns>
    public static TValue? LookupByKey<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        => dict.TryGetValue(key, out var val) ? val : default;

    /// <summary>
    /// Flattens an IEnumerable
    /// Example:
    /// [1, 2, [3, [4]], 5] -> [1, 2, 3, 4, 5]
    /// </summary>
    /// <typeparam name="T">Type of each object</typeparam>
    /// <param name="values">Input IEnumerable</param>
    /// <returns>Flatten result</returns>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<T> values)
    {
        foreach (var item in values)
        {
            if (item is not IEnumerable<T> children)
            {
                yield return item;
                continue;
            }

            foreach (var child in children.Flatten())
                yield return child;
        }
    }
}
