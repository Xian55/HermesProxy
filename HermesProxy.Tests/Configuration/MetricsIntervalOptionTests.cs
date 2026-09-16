using System;
using HermesProxy.Configuration.Options;
using Xunit;

namespace HermesProxy.Tests.Configuration;

public class MetricsIntervalOptionTests
{
    [Fact]
    public void PreprocessArgs_MetricsInterval_SetsIntervalAndEnablesMetrics()
    {
        var args = Program.PreprocessArgs(["--metrics-interval", "15"], out _);

        string[] expected =
        [
            $"--{nameof(DiagnosticsOptions)}:{nameof(DiagnosticsOptions.EnableMetrics)}=true",
            $"--{nameof(DiagnosticsOptions)}:{nameof(DiagnosticsOptions.MetricsIntervalSeconds)}=15",
        ];
        Assert.Equal(expected, args);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("-5")]
    [InlineData("1.5")]
    public void PreprocessArgs_MetricsInterval_RejectsNonWholeNumber(string value)
    {
        Assert.Throws<ArgumentException>(() => Program.PreprocessArgs(["--metrics-interval", value], out _));
    }

    [Fact]
    public void PreprocessArgs_MetricsInterval_RequiresValue()
    {
        Assert.Throws<ArgumentException>(() => Program.PreprocessArgs(["--metrics-interval"], out _));
    }

    [Fact]
    public void DiagnosticsOptions_DefaultInterval_IsOneMinute()
    {
        Assert.Equal(60, new DiagnosticsOptions().MetricsIntervalSeconds);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(60, true)]
    [InlineData(3600, true)]
    [InlineData(0, false)]
    [InlineData(3601, false)]
    public void DiagnosticsOptionsValidator_BoundsInterval(int seconds, bool valid)
    {
        var result = new DiagnosticsOptionsValidator().Validate(null, new DiagnosticsOptions { MetricsIntervalSeconds = seconds });

        Assert.Equal(valid, result.Succeeded);
    }
}
