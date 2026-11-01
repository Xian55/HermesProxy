using Framework.Logging;
using Serilog.Events;

namespace HermesProxy.Configuration.Options;

[ConfigSection(4)]
public sealed class LoggingOptions
{
    [Setting("Floor across all outputs: Verbose, Debug, Information, Warning, Error or Fatal. See docs/logging.md.")]
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;

    [Setting("Level of the Server category: lifecycle, configuration, startup.")]
    public LogEventLevel ServerLevel { get; set; } = LogEventLevel.Information;

    [Setting("Level of the Network category: auth and world sockets, handshakes.")]
    public LogEventLevel NetworkLevel { get; set; } = LogEventLevel.Information;

    [Setting("Level of the Storage category: game data, CSV and DB2 files, hotfixes.")]
    public LogEventLevel StorageLevel { get; set; } = LogEventLevel.Information;

    [Setting("Level of the Packet category: opcode dispatch and packet traces. Verbose adds per-packet span statistics.")]
    public LogEventLevel PacketLevel { get; set; } = LogEventLevel.Warning;

    [Setting("A further floor for the console alone. The log file keeps all that the categories allow.")]
    public LogEventLevel ConsoleLevel { get; set; } = LogEventLevel.Information;

    [Setting("Write a log file per run to Directory. It rolls at 256 MB and a run keeps its newest 4 files.")]
    public bool ToFile { get; set; } = true;

    [Setting("Directory for log files. A relative path starts at the proxy's own directory.")]
    public string Directory { get; set; } = "Logs";

    [Setting("Days to keep log files. Older ones are deleted at startup and every hour. 0 keeps them forever (0-3650).")]
    public int RetentionDays { get; set; } = 30;

    // Legacy flags preserved only for LoggingLegacyFlagsTranslator; not exposed in appsettings.json
    // by default. Delete alongside the translator in a follow-up release.
    [Setting("Legacy shortcut: lowers MinimumLevel and ConsoleLevel to Debug.")]
    public bool DebugOutput { get; set; }

    [Setting("Legacy shortcut: lowers PacketLevel to Verbose.")]
    public bool SpanStatsLog { get; set; }

    public LogBootstrapOptions ToLogBootstrapOptions()
        => new(MinimumLevel, ServerLevel, NetworkLevel, StorageLevel, PacketLevel, ConsoleLevel, ToFile, Directory);
}
