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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Runtime.CompilerServices;

namespace System;

public static partial class Extensions
{

    public static string ToHexString(this byte[] array) => Convert.ToHexString(array);

    /// <summary>
    /// Reads the bytes as an unsigned little-endian number, so a set top bit is not a sign.
    /// </summary>
    public static BigInteger ToBigInteger(this byte[] array) => new(array, isUnsigned: true);

    /// <summary>
    /// The little-endian bytes of a non-negative number, without a sign byte. Zero has no bytes:
    /// <see cref="BigInteger.ToByteArray(bool, bool)"/> alone would give it one.
    /// </summary>
    public static byte[] ToCleanByteArray(this BigInteger b) => b.IsZero ? [] : b.ToByteArray(isUnsigned: true);

    public static BigInteger ModPow(this BigInteger value, BigInteger pow, BigInteger mod)
    {
        return BigInteger.ModPow(value, pow, mod);
    }

    // Returns true if value and flag share any bit.
    //
    // Both sides must have the same type. The overload these replaced took IConvertible, which
    // boxed both arguments and, for an enum, boxed the underlying value again inside ToUInt64:
    // four allocations per check. The V3_4_3 update writer alone makes nine checks per object,
    // three times over, which put it at 14% of all proxy allocation. It is gone rather than kept
    // as a fallback, so a check that mixes types (a uint field against an enum constant) fails to
    // compile instead of quietly boxing: cast the constant to the field's type.
    //
    // One difference from the old overload: ToUInt64 threw OverflowException for a negative
    // signed value, these compare the bits.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasAnyFlag<T>(this T value, T flag) where T : struct, Enum
    {
        // SizeOf is a JIT-time constant, so only one branch survives per enum.
        if (Unsafe.SizeOf<T>() == sizeof(byte))
            return (Unsafe.As<T, byte>(ref value) & Unsafe.As<T, byte>(ref flag)) != 0;
        if (Unsafe.SizeOf<T>() == sizeof(ushort))
            return (Unsafe.As<T, ushort>(ref value) & Unsafe.As<T, ushort>(ref flag)) != 0;
        if (Unsafe.SizeOf<T>() == sizeof(uint))
            return (Unsafe.As<T, uint>(ref value) & Unsafe.As<T, uint>(ref flag)) != 0;
        return (Unsafe.As<T, ulong>(ref value) & Unsafe.As<T, ulong>(ref flag)) != 0;
    }

    public static byte[] Combine(this byte[] data, params ReadOnlySpan<byte[]> pData)
    {
        // Pre-calculate total size to allocate once instead of resizing per array
        int totalLength = data.Length;
        foreach (var arr in pData)
            totalLength += arr.Length;

        var combined = new byte[totalLength];

        data.CopyTo(combined);

        int offset = data.Length;
        foreach (var arr in pData)
        {
            arr.CopyTo(combined, offset);
            offset += arr.Length;
        }

        return combined;
    }

    /// <summary>
    /// Original implementation for benchmarking comparison. DO NOT USE.
    /// </summary>
    internal static byte[] CombineOriginal(this byte[] data, params byte[][] pData)
    {
        var combined = data;

        foreach (var arr in pData)
        {
            var currentSize = combined.Length;

            Array.Resize(ref combined, currentSize + arr.Length);

            Buffer.BlockCopy(arr, 0, combined, currentSize, arr.Length);
        }

        return combined;
    }

    public static TTarget CastFlags<TTarget>(this Enum input) where TTarget : struct, Enum
    {
        return FlagMappingCache.ConvertFlags<TTarget>(input);
    }

    public static TTarget CastEnum<TTarget>(this Enum input) where TTarget : struct, Enum
    {
        return FlagMappingCache.ConvertSingle<TTarget>(input);
    }

    /// <summary>
    /// <see cref="CastFlags{TTarget}(Enum)"/> without the box: the <c>Enum</c> receiver there boxes
    /// the value on every call, and the mapping is looked up by type pair each time. Here both
    /// are resolved once per pair. For per-packet call sites; the result is identical.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TTarget CastFlags<TSource, TTarget>(this TSource input)
        where TSource : struct, Enum
        where TTarget : struct, Enum
        => FlagMap<TSource, TTarget>.Convert(input);
}

internal static class FlagMappingCache
{
    // Cache: (sourceType, targetType) -> mapping of source values to target values
    private static readonly ConcurrentDictionary<(Type, Type), Dictionary<ulong, ulong>> _cache = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TTarget ConvertSingle<TTarget>(Enum source) where TTarget : struct, Enum
    {
        var sourceType = source.GetType();
        var targetType = typeof(TTarget);

        var mapping = _cache.GetOrAdd((sourceType, targetType), static key => BuildMapping(key.Item1, key.Item2));

        ulong sourceValue = ToUInt64(source);

        if (mapping.TryGetValue(sourceValue, out ulong targetValue))
        {
            return Unsafe.As<ulong, TTarget>(ref targetValue);
        }

        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TTarget ConvertFlags<TTarget>(Enum source) where TTarget : struct, Enum
    {
        var sourceType = source.GetType();
        var targetType = typeof(TTarget);

        var mapping = _cache.GetOrAdd((sourceType, targetType), static key => BuildMapping(key.Item1, key.Item2));

        ulong sourceValue = ToUInt64(source);
        ulong result = 0;

        // Fast path: check if direct mapping exists (common for single flags)
        if (mapping.TryGetValue(sourceValue, out ulong directResult))
        {
            return Unsafe.As<ulong, TTarget>(ref directResult);
        }

        // Iterate through each set bit and map it
        foreach (var kvp in mapping)
        {
            if ((sourceValue & kvp.Key) == kvp.Key && kvp.Key != 0)
            {
                result |= kvp.Value;
            }
        }

        return Unsafe.As<ulong, TTarget>(ref result);
    }

    private static Dictionary<ulong, ulong> BuildMapping(Type sourceType, Type targetType)
    {
        var result = new Dictionary<ulong, ulong>();

        // Build lookup of target enum by name
        var targetByName = new Dictionary<string, ulong>();
        foreach (var targetValue in Enum.GetValues(targetType))
        {
            var name = targetValue.ToString()!;
            targetByName[name] = ToUInt64((Enum)targetValue);
        }

        // Map source values to target values by matching names
        foreach (var sourceValue in Enum.GetValues(sourceType))
        {
            var name = sourceValue.ToString()!;
            if (targetByName.TryGetValue(name, out ulong targetVal))
            {
                result[ToUInt64((Enum)sourceValue)] = targetVal;
            }
        }

        return result;
    }

    internal static Dictionary<ulong, ulong> GetMapping(Type sourceType, Type targetType)
        => _cache.GetOrAdd((sourceType, targetType), static key => BuildMapping(key.Item1, key.Item2));

    // Unboxes the underlying value straight out of the already-boxed enum. Convert.ToUInt64 went
    // through IConvertible, which boxed that value a second time on every call; movement and unit
    // flags pass through here per packet. Checked, so a negative value still throws the
    // OverflowException Convert.ToUInt64 threw.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong ToUInt64(Enum value) => value.GetTypeCode() switch
    {
        TypeCode.Byte => (byte)(object)value,
        TypeCode.UInt16 => (ushort)(object)value,
        TypeCode.UInt32 => (uint)(object)value,
        TypeCode.UInt64 => (ulong)(object)value,
        TypeCode.SByte => checked((ulong)(sbyte)(object)value),
        TypeCode.Int16 => checked((ulong)(short)(object)value),
        TypeCode.Int32 => checked((ulong)(int)(object)value),
        TypeCode.Int64 => checked((ulong)(long)(object)value),
        _ => System.Convert.ToUInt64(value),
    };
}

/// <summary>
/// The name-matched mapping <see cref="FlagMappingCache"/> builds, held per type pair so a call
/// needs neither the cache lookup nor a boxed source.
/// </summary>
internal static class FlagMap<TSource, TTarget>
    where TSource : struct, Enum
    where TTarget : struct, Enum
{
    private static readonly Dictionary<ulong, ulong> Mapping =
        FlagMappingCache.GetMapping(typeof(TSource), typeof(TTarget));

    private static readonly KeyValuePair<ulong, ulong>[] Bits = [.. Mapping.Where(kv => kv.Key != 0)];

    // An enum's type code is its underlying type's; readonly, so tier-1 folds the switch below.
    private static readonly TypeCode SourceCode = Type.GetTypeCode(typeof(TSource));

    public static TTarget Convert(TSource source)
    {
        ulong value = ToUInt64(source);

        if (Mapping.TryGetValue(value, out ulong direct))
            return Unsafe.As<ulong, TTarget>(ref direct);

        ulong result = 0;
        foreach (var (key, target) in Bits)
        {
            if ((value & key) == key)
                result |= target;
        }
        return Unsafe.As<ulong, TTarget>(ref result);
    }

    // Same conversion as FlagMappingCache.ToUInt64, read straight from the value: checked, so a
    // negative signed value throws where Convert.ToUInt64 did.
    private static ulong ToUInt64(TSource v) => SourceCode switch
    {
        TypeCode.Byte => Unsafe.As<TSource, byte>(ref v),
        TypeCode.UInt16 => Unsafe.As<TSource, ushort>(ref v),
        TypeCode.UInt32 => Unsafe.As<TSource, uint>(ref v),
        TypeCode.UInt64 => Unsafe.As<TSource, ulong>(ref v),
        TypeCode.SByte => checked((ulong)Unsafe.As<TSource, sbyte>(ref v)),
        TypeCode.Int16 => checked((ulong)Unsafe.As<TSource, short>(ref v)),
        TypeCode.Int32 => checked((ulong)Unsafe.As<TSource, int>(ref v)),
        _ => checked((ulong)Unsafe.As<TSource, long>(ref v)),
    };
}

/// <summary>
/// Flag tests and edits for any integer type, so a class holding flags in an integer field does
/// not hand-roll its own Has/Add/Remove. The enum overload of <c>HasAnyFlag</c> lives in
/// <see cref="Extensions"/>; a separate class because the two differ only by constraint.
/// </summary>
/// <remarks>
/// Both sides must have the same type, as with the enum overload: cast an enum constant to the
/// field's type rather than widening the field.
/// </remarks>
public static class IntegerFlagExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasAnyFlag<T>(this T value, T flag) where T : struct, IBinaryInteger<T>
        => (value & flag) != T.Zero;
}

// Re-open Extensions class for remaining methods
public static partial class Extensions
{

    #region Strings
    public static bool IsEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static T ToEnum<T>(this string str) where T : struct
        => Enum.TryParse(str, out T value) ? value : default;

    public static string Reverse(this string str)
        => string.Create(str.Length, str, static (chars, source) =>
        {
            source.CopyTo(chars);
            chars.Reverse();
        });

    public static byte[] ToCString(this string str)
    {
        byte[] data = new byte[Encoding.UTF8.GetByteCount(str) + 1];
        Encoding.UTF8.GetBytes(str, data);
        return data;
    }

    // A trailing odd digit is dropped, not an error.
    public static byte[] ParseAsByteArray(this string str)
    {
        str = str.Replace(" ", string.Empty);
        return Convert.FromHexString(str.AsSpan(0, str.Length & ~1));
    }

    public static int GetByteCount(this string str)
    {
        if (str.IsEmpty())
            return 0;

        return Encoding.UTF8.GetByteCount(str);
    }

    #endregion
}
