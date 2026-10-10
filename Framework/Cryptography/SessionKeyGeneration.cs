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
using System.Security.Cryptography;

namespace Framework.Cryptography;

public sealed class SessionKeyGenerator
{
    private readonly HashAlgorithmName _hash;
    private readonly int _hashSize;
    private readonly byte[] _o0;
    private readonly byte[] _o1;
    private readonly byte[] _o2;
    private int _taken;

    public SessionKeyGenerator(ReadOnlySpan<byte> buff) : this(buff, HashAlgorithmName.SHA256) { }

    /// <param name="hash">SHA-256 up to 3.4.x; Cataclysm Classic 4.4 runs it on SHA-512.</param>
    public SessionKeyGenerator(ReadOnlySpan<byte> buff, HashAlgorithmName hash)
    {
        _hash = hash;
        _hashSize = hash == HashAlgorithmName.SHA512 ? SHA512.HashSizeInBytes : SHA256.HashSizeInBytes;
        _o0 = new byte[_hashSize];
        _o1 = new byte[_hashSize];
        _o2 = new byte[_hashSize];

        int halfSize = buff.Length / 2;
        CryptographicOperations.HashData(hash, buff[..halfSize], _o1);
        CryptographicOperations.HashData(hash, buff[halfSize..], _o2);
        FillUp();
    }

    public SessionKeyGenerator(byte[] buff, int size) : this(buff.AsSpan(0, size)) { }

    public void Generate(Span<byte> buf)
    {
        for (int i = 0; i < buf.Length; i++)
        {
            if (_taken == _hashSize)
                FillUp();

            buf[i] = _o0[_taken];
            _taken++;
        }
    }

    public void Generate(byte[] buf, uint sz) => Generate(buf.AsSpan(0, (int)sz));

    private void FillUp()
    {
        using var ih = IncrementalHash.CreateHash(_hash);
        ih.AppendData(_o1);
        ih.AppendData(_o0);
        ih.AppendData(_o2);

        // Hash directly into _o0 to avoid the byte[] allocation that GetHashAndReset would produce.
        if (!ih.TryGetHashAndReset(_o0, out int written) || written != _hashSize)
            throw new CryptographicException("SessionKeyGenerator.FillUp: hash produced unexpected output size.");

        _taken = 0;
    }
}
