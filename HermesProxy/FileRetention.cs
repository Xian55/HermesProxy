using System;
using System.IO;
using HermesProxy.World;

namespace HermesProxy;

/// <summary>
/// Removes old captures and log files, so a host that runs for months can keep writing them: they
/// are what a bug report starts from, and turning them off to save the disk loses that.
/// </summary>
internal static class FileRetention
{
    internal const int MaxDays = 3650;

    public static readonly TimeSpan SweepInterval = TimeSpan.FromHours(1);

    // Only the names the proxy writes. These folders also collect parsed .txt output and files
    // people put there themselves, and those are not ours to delete.
    public static readonly string[] Captures = [$"{SniffFile.LegacyName}_*.pkt", $"{SniffFile.ModernName}_*.pkt"];
    public static readonly string[] Logs = ["hermes-*.log"];

    public readonly record struct Removed(int Files, long Bytes);

    /// <summary>Deletes the matching files last written before <paramref name="cutoffUtc"/>.</summary>
    public static Removed Sweep(string directory, string[] patterns, DateTime cutoffUtc)
    {
        var total = new Removed();
        foreach (string pattern in patterns)
        {
            try
            {
                foreach (var file in new DirectoryInfo(directory).EnumerateFiles(pattern))
                {
                    if (file.LastWriteTimeUtc >= cutoffUtc)
                        continue;

                    long length = file.Length;
                    if (TryDelete(file))
                        total = new Removed(total.Files + 1, total.Bytes + length);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // No folder yet, or one this user can't read. A sweep is housekeeping and must
                // not take the proxy down; the next one tries again.
            }
        }

        return total;
    }

    private static bool TryDelete(FileInfo file)
    {
        try
        {
            file.Delete();
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Still open, or read-only. The next sweep tries again.
            return false;
        }
    }
}
