using Framework.Logging;
using Serilog.Events;

namespace HermesProxy.Configuration.Options;

public sealed class LoggingOptions
{
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;
    public LogEventLevel ServerLevel { get; set; } = LogEventLevel.Information;
    public LogEventLevel NetworkLevel { get; set; } = LogEventLevel.Information;
    public LogEventLevel StorageLevel { get; set; } = LogEventLevel.Information;
    public LogEventLevel PacketLevel { get; set; } = LogEventLevel.Warning;
    public LogEventLevel ConsoleLevel { get; set; } = LogEventLevel.Information;
    public bool ToFile { get; set; } = true;
    public string Directory { get; set; } = "Logs";

    // Legacy flags preserved only for LoggingLegacyFlagsTranslator; not exposed in appsettings.json
    // by default. Delete alongside the translator in a follow-up release.
    public bool DebugOutput { get; set; }
    public bool SpanStatsLog { get; set; }

    public LogBootstrapOptions ToLogBootstrapOptions()
        => new(MinimumLevel, ServerLevel, NetworkLevel, StorageLevel, PacketLevel, ConsoleLevel, ToFile, Directory);
}
