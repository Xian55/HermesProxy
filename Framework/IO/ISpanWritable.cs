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

using System;

namespace Framework.IO
{
    /// <summary>
    /// Interface for packets that support zero-allocation Span-based writing.
    /// Packets implementing this interface can be written using SpanPacketWriter
    /// for improved performance in hot paths.
    /// </summary>
    public interface ISpanWritable
    {
        /// <summary>
        /// Writes the packet data to the provided buffer using SpanPacketWriter.
        /// </summary>
        /// <param name="buffer">The buffer to write to. Must be at least MaxSize bytes.</param>
        /// <returns>
        /// The number of bytes written, or a negative value to indicate that the packet
        /// exceeds MaxSize and the caller should fall back to the standard Write() method.
        /// This allows packets with variable-length data to use a capped MaxSize for common
        /// cases while gracefully handling rare oversized packets.
        /// </returns>
        int WriteToSpan(Span<byte> buffer);

        /// <summary>
        /// Returns the maximum size in bytes this packet could be.
        /// Used for buffer allocation. Should be a constant or cheap to compute.
        /// For packets with variable-length data, this may be a practical cap rather than
        /// a theoretical maximum. If actual data exceeds this, WriteToSpan should return
        /// a negative value to trigger fallback.
        /// </summary>
        int MaxSize { get; }
    }
}
