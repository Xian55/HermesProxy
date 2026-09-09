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
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;

namespace Framework.IO;

public static partial class ZLib
{
    public static byte[] Compress(byte[] data)
    {
        using ByteBuffer buffer = new ByteBuffer();
        buffer.WriteUInt8(0x78);
        buffer.WriteUInt8(0x9c);

        uint adler32 = Adler32.Update(1, data);
        var ms = new MemoryStream();
        using (var deflateStream = new DeflateStream(ms, CompressionMode.Compress))
        {
            deflateStream.Write(data, 0, data.Length);
            deflateStream.Flush();
        }
        if (ms.TryGetBuffer(out var msBuffer))
            buffer.WriteBytes(new Span<byte>(msBuffer.Array!, msBuffer.Offset, msBuffer.Count));
        else
            buffer.WriteBytes(ms.ToArray());
        Span<byte> adlerBytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(adlerBytes, adler32);
        buffer.WriteBytes(adlerBytes);

        return buffer.GetData();
    }

    public static byte[] Decompress(byte[] data, uint unpackedSize)
    {
        byte[] decompressData = new byte[unpackedSize];
        Decompress(data, 0, data.Length, decompressData.AsSpan(0, (int)unpackedSize));
        return decompressData;
    }

    /// <summary>
    /// Inflates a zlib blob straight into <paramref name="destination"/>.
    /// </summary>
    /// <remarks>
    /// The previous shape decompressed into a growing <see cref="MemoryStream"/> and then copied
    /// it out one <c>ReadByte()</c> at a time, so a 1.3 MB update object cost ~5.4 MB of garbage
    /// and a virtual call per byte. Reading into the caller's buffer removes both, and lets the
    /// caller rent that buffer.
    ///
    /// The offset/count pair lets the caller point at the compressed bytes already sitting in a
    /// received packet instead of copying them out first.
    /// </remarks>
    public static void Decompress(byte[] data, int offset, int count, Span<byte> destination)
    {
        // 2-byte zlib header up front, 4-byte adler32 checksum on the end; DeflateStream wants
        // neither.
        using var deflateStream = new DeflateStream(
            new MemoryStream(data, offset + 2, count - 6), CompressionMode.Decompress);

        // A stream shorter than unpackedSize used to leave the tail at whatever the freshly
        // allocated array held, i.e. zeroes. A pooled destination holds the previous packet
        // instead, so clear the remainder rather than let stale bytes be parsed as payload.
        int read = deflateStream.ReadAtLeast(destination, destination.Length, throwOnEndOfStream: false);
        if (read < destination.Length)
            destination[read..].Clear();
    }
}
