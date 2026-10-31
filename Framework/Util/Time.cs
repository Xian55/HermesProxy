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

public static class Time
{
    public const int Minute = 60;
    public const int Hour = Minute * 60;
    public const int Day = Hour * 24;
    public const int Month = Day * 30;
    public const int InMilliseconds = 1000;

    // Lock-free uptime baseline. Environment.TickCount64 reads an interlocked OS counter
    // (~5 ns) versus DateTime.UtcNow which is a syscall (~50-150 ns).
    private static readonly long _startTickCount64 = Environment.TickCount64;

    /// <summary>
    /// Gets the current Unix time.
    /// </summary>
    public static long UnixTime => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    /// <summary>
    /// Gets the current Unix time, in milliseconds.
    /// </summary>
    public static long UnixTimeMilliseconds => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public static uint GetMSTime()
    {
        return (uint)(Environment.TickCount64 - _startTickCount64);
    }

    public static DateTime UnixTimeToDateTime(long unixTime)
    {
        return DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;
    }

    public static long DateTimeToUnixTime(DateTime dateTime)
    {
        return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
    }

    public static long GetUnixTimeFromPackedTime(uint packedDate)
    {
        var time = new DateTime((int)((packedDate >> 24) & 0x1F) + 2000, (int)((packedDate >> 20) & 0xF) + 1, (int)((packedDate >> 14) & 0x3F) + 1, (int)(packedDate >> 6) & 0x1F, (int)(packedDate & 0x3F), 0);
        return (uint)DateTimeToUnixTime(time);
    }

    public static uint GetPackedTimeFromUnixTime(long unixTime) => GetPackedTimeFromDateTime(UnixTimeToDateTime(unixTime));

    public static uint GetPackedTimeFromDateTime(DateTime now)
    {
        return Convert.ToUInt32((now.Year - 2000) << 24 | (now.Month - 1) << 20 | (now.Day - 1) << 14 | (int)now.DayOfWeek << 11 | now.Hour << 6 | now.Minute);
    }
}

