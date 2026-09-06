using Framework.Logging;

using Serilog;
using Serilog.Core;
using Serilog.Events;

using Xunit;

// Serilog exports a static Log of its own, and this file needs both namespaces.
using Log = Framework.Logging.Log;

namespace HermesProxy.Tests.Framework;

/// <summary>
/// Pins the fast-path level gates (<see cref="Log.IsTraceEnabled"/>, <see cref="Log.IsDebugEnabled"/>)
/// to the routing that <see cref="Log.Print"/> actually uses.
///
/// These two drifted apart: the gates ANDed the global switch with the category switch, but the
/// pipeline wires categories with <c>MinimumLevel.Override</c>, which *replaces* the global minimum
/// for a source context rather than combining with it. The result was a gate stricter than the sink
/// it guards — with a Verbose Server category under an Information global (what test-loop2
/// configures) every gated trace site went silent while ungated ones kept writing.
/// </summary>
public class LogLevelGateTests
{
    /// <summary>
    /// The premise the gates rest on, asserted directly against Serilog with no HermesProxy state:
    /// an Override wins over a stricter global for its own source context.
    /// </summary>
    [Fact]
    public void MinimumLevelOverride_ReplacesGlobalMinimum_ForItsSourceContext()
    {
        var global = new LoggingLevelSwitch(LogEventLevel.Information);
        var server = new LoggingLevelSwitch(LogEventLevel.Verbose);

        using var root = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(global)
            .MinimumLevel.Override("Server", server)
            .CreateLogger();

        var scoped = root.ForContext(Constants.SourceContextPropertyName, "Server");

        Assert.True(scoped.IsEnabled(LogEventLevel.Verbose));

        // Contrast: a context with no override falls back to the stricter global.
        var unscoped = root.ForContext(Constants.SourceContextPropertyName, "Unmapped");
        Assert.False(unscoped.IsEnabled(LogEventLevel.Verbose));
    }

    [Theory]
    // global,            server,            trace, debug
    [InlineData(LogEventLevel.Information, LogEventLevel.Verbose, true, true)]   // the config that regressed
    [InlineData(LogEventLevel.Verbose, LogEventLevel.Verbose, true, true)]
    [InlineData(LogEventLevel.Information, LogEventLevel.Debug, false, true)]
    [InlineData(LogEventLevel.Information, LogEventLevel.Information, false, false)]
    [InlineData(LogEventLevel.Verbose, LogEventLevel.Information, false, false)]  // global alone must not open it
    public void FastPathGates_MatchRoutedIsEnabled(
        LogEventLevel global, LogEventLevel server, bool expectTrace, bool expectDebug)
    {
        try
        {
            Configure(global, server);

            Assert.Equal(expectTrace, Log.IsTraceEnabled);
            Assert.Equal(expectDebug, Log.IsDebugEnabled);

            // The invariant that matters: the cheap gate and the real routing agree, so a gated
            // site is silent exactly when Log.Print would have dropped the line anyway.
            Assert.Equal(Log.IsEnabled(LogType.Trace), Log.IsTraceEnabled);
            Assert.Equal(Log.IsEnabled(LogType.Debug), Log.IsDebugEnabled);
        }
        finally
        {
            RestoreDefaults();
        }
    }

    private static void Configure(LogEventLevel global, LogEventLevel server) =>
        Log.Configure(new LogBootstrapOptions(
            MinimumLevel: global,
            ServerLevel: server,
            NetworkLevel: LogEventLevel.Information,
            StorageLevel: LogEventLevel.Information,
            PacketLevel: LogEventLevel.Warning,
            ConsoleLevel: LogEventLevel.Information,
            ToFile: false,
            Directory: "Logs"));

    /// <summary>Mirrors the switch defaults declared in <c>Log</c>'s field initialisers.</summary>
    private static void RestoreDefaults() =>
        Configure(LogEventLevel.Information, LogEventLevel.Information);
}
