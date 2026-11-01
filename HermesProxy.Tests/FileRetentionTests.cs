using System;
using System.IO;
using HermesProxy.Configuration.Options;
using Xunit;

namespace HermesProxy.Tests;

public sealed class FileRetentionTests : IDisposable
{
    private static readonly DateTime Now = new(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Cutoff = Now.AddDays(-30);

    private readonly string _directory = Directory.CreateTempSubdirectory("hermes-retention-").FullName;

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    private string Place(string name, DateTime lastWrite, int size = 10)
    {
        string path = Path.Combine(_directory, name);
        File.WriteAllBytes(path, new byte[size]);
        File.SetLastWriteTimeUtc(path, lastWrite);
        return path;
    }

    [Fact]
    public void Sweep_RemovesCapturesPastTheCutoff_AndCountsThem()
    {
        string legacy = Place("legacy_12340_20260901_120000_1.pkt", Cutoff.AddDays(-1), size: 100);
        string modern = Place("modern_54261_20260901_120000_2.pkt", Cutoff.AddHours(-1), size: 50);

        var removed = FileRetention.Sweep(_directory, FileRetention.Captures, Cutoff);

        Assert.False(File.Exists(legacy));
        Assert.False(File.Exists(modern));
        Assert.Equal(new FileRetention.Removed(2, 150), removed);
    }

    [Theory]
    [InlineData("hermes-20260901_120000.log")]
    [InlineData("hermes-20260901_120000_001.log")]
    [InlineData("hermes-20260901.log")]
    public void Sweep_RemovesLogsPastTheCutoff(string name)
    {
        string path = Place(name, Cutoff.AddDays(-1));

        var removed = FileRetention.Sweep(_directory, FileRetention.Logs, Cutoff);

        Assert.False(File.Exists(path));
        Assert.Equal(1, removed.Files);
    }

    [Fact]
    public void Sweep_KeepsFilesInsideTheCutoff()
    {
        string atCutoff = Place("legacy_12340_20261002_120000_1.pkt", Cutoff);
        string recent = Place("modern_54261_20261031_120000_2.pkt", Now.AddDays(-1));

        var removed = FileRetention.Sweep(_directory, FileRetention.Captures, Cutoff);

        Assert.True(File.Exists(atCutoff));
        Assert.True(File.Exists(recent));
        Assert.Equal(default, removed);
    }

    [Theory]
    [InlineData("parse_something.pkt")]
    [InlineData("legacy_12340_20260901_120000_1_parsed.txt")]
    [InlineData("modern_54261_20260901_120000_1_parsed.txt")]
    [InlineData("counters_20261024_024153.csv")]
    [InlineData("legacy.pkt")]
    [InlineData("my-capture.pkt")]
    public void Sweep_LeavesWhatTheProxyDidNotCapture(string name)
    {
        string path = Place(name, Cutoff.AddYears(-5));

        FileRetention.Sweep(_directory, FileRetention.Captures, Cutoff);

        Assert.True(File.Exists(path));
    }

    [Theory]
    [InlineData("dotnet-watch.log")]
    [InlineData("crash-1234.dmp")]
    [InlineData("hermes-20260901_120000.txt")]
    public void Sweep_LeavesWhatIsNotAProxyLog(string name)
    {
        string path = Place(name, Cutoff.AddYears(-5));

        FileRetention.Sweep(_directory, FileRetention.Logs, Cutoff);

        Assert.True(File.Exists(path));
    }

    [Fact]
    public void Sweep_ReturnsNothing_WhenThereIsNoFolderYet()
    {
        var removed = FileRetention.Sweep(Path.Combine(_directory, "PacketsLog"), FileRetention.Captures, Cutoff);

        Assert.Equal(default, removed);
    }

    [Fact]
    public void Sweep_SkipsAFileThatIsStillOpen_AndRemovesTheRest()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "Only Windows refuses to delete an open file.");
        string open = Place("modern_54261_20260901_120000_1.pkt", Cutoff.AddDays(-1));
        string closed = Place("legacy_12340_20260901_120000_2.pkt", Cutoff.AddDays(-1), size: 7);

        // The way SniffFile holds a capture open.
        using var stream = new FileStream(open, FileMode.Open, FileAccess.Write, FileShare.Read);
        File.SetLastWriteTimeUtc(open, Cutoff.AddDays(-1));
        var removed = FileRetention.Sweep(_directory, FileRetention.Captures, Cutoff);

        Assert.True(File.Exists(open));
        Assert.False(File.Exists(closed));
        Assert.Equal(new FileRetention.Removed(1, 7), removed);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(30, true)]
    [InlineData(FileRetention.MaxDays, true)]
    [InlineData(-1, false)]
    [InlineData(FileRetention.MaxDays + 1, false)]
    public void Validators_BoundRetention(int days, bool valid)
    {
        Assert.Equal(valid, new DiagnosticsOptionsValidator().Validate(null, new DiagnosticsOptions { PacketsLogRetentionDays = days }).Succeeded);
        Assert.Equal(valid, new LoggingOptionsValidator().Validate(null, new LoggingOptions { RetentionDays = days }).Succeeded);
    }

    [Fact]
    public void DefaultRetention_IsThirtyDays()
    {
        Assert.Equal(30, new DiagnosticsOptions().PacketsLogRetentionDays);
        Assert.Equal(30, new LoggingOptions().RetentionDays);
    }
}
