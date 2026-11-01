using System;
using System.IO;
using System.Linq;
using Serilog;
using Xunit;
using Log = Framework.Logging.Log;

namespace HermesProxy.Tests.Framework;

public sealed class LogFileRollingTests : IDisposable
{
    private readonly string _directory = Directory.CreateTempSubdirectory("hermes-rolling-").FullName;

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    private string[] WriteLines(int count, long fileSizeLimitBytes)
    {
        var configuration = new LoggerConfiguration();
        Log.WriteToFile(configuration.WriteTo, Path.Combine(_directory, "hermes-20261101_120000.log"), fileSizeLimitBytes);
        using (var logger = configuration.CreateLogger())
        {
            for (int i = 0; i < count; i++)
                logger.Information("line {Number}", i);
        }

        return Directory.GetFiles(_directory, "hermes-20261101_120000*.log").Order(StringComparer.Ordinal).ToArray();
    }

    [Fact]
    public void WriteToFile_RunOutgrowsItsFile_KeepsWritingToANewOne()
    {
        var files = WriteLines(2000, fileSizeLimitBytes: 1024);

        // Without rolling the sink stops at the limit for good, and this line is lost.
        Assert.Contains("line 1999", File.ReadAllText(files[^1]));
    }

    [Fact]
    public void WriteToFile_RunOutgrowsItsFile_KeepsOnlyItsNewestParts()
    {
        var files = WriteLines(2000, fileSizeLimitBytes: 1024);

        Assert.Equal(Log.MaxLogFilesPerRun, files.Length);
        Assert.DoesNotContain(files, static file => file.EndsWith("hermes-20261101_120000.log", StringComparison.Ordinal));
    }

    [Fact]
    public void WriteToFile_RunOutgrowsItsFile_LeavesEarlierRunsAlone()
    {
        string earlier = Path.Combine(_directory, "hermes-20261101_110000.log");
        File.WriteAllText(earlier, "an earlier run");

        WriteLines(2000, fileSizeLimitBytes: 1024);

        Assert.True(File.Exists(earlier));
    }

    [Fact]
    public void WriteToFile_RunInsideTheLimit_IsOneFileUnderTheRunsOwnName()
    {
        var files = WriteLines(3, Log.MaxLogFileBytes);

        Assert.Equal("hermes-20261101_120000.log", Path.GetFileName(Assert.Single(files)));
    }
}
