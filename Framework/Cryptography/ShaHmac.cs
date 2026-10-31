/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * Copyright (C) 2012-2014 Arctium Emulation <http://arctium.org>
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
using System.Security.Cryptography;

namespace Framework.Cryptography;

public sealed class Sha256 : IDisposable
{
    private readonly IncrementalHash _hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

    public void Process(byte[] data, int length) => _hash.AppendData(data.AsSpan(0, length));

    public void Process(uint data)
    {
        Span<byte> b = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32LittleEndian(b, data);
        _hash.AppendData(b);
    }

    public void Finish(ReadOnlySpan<byte> finalBlock)
    {
        _hash.AppendData(finalBlock);
        Digest = _hash.GetHashAndReset();
    }

    public void Finish(byte[] finalBlock) => Finish(finalBlock.AsSpan());

    public byte[]? Digest { get; private set; }

    public void Dispose() => _hash.Dispose();
}

public sealed class HmacSha256 : IDisposable
{
    private readonly IncrementalHash _hash;

    public HmacSha256(ReadOnlySpan<byte> key)
    {
        _hash = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, key);
    }

    public HmacSha256(byte[] key) : this(key.AsSpan()) { }

    public void Process(byte[] data, int length) => _hash.AppendData(data.AsSpan(0, length));

    public void Finish(ReadOnlySpan<byte> finalBlock)
    {
        _hash.AppendData(finalBlock);
        Digest = _hash.GetHashAndReset();
    }

    public void Finish(byte[] finalBlock, int length) => Finish(finalBlock.AsSpan(0, length));

    public byte[]? Digest { get; private set; }

    public void Dispose() => _hash.Dispose();
}
