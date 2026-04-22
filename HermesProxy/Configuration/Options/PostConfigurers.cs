using System;
using HermesProxy.Enums;
using Microsoft.Extensions.Options;
using Serilog.Events;

namespace HermesProxy.Configuration.Options;

internal sealed class ClientSeedParser : IPostConfigureOptions<ClientOptions>
{
    public void PostConfigure(string? name, ClientOptions options)
    {
        var seed = string.IsNullOrWhiteSpace(options.SeedHex) ? [] : options.SeedHex.ParseAsByteArray();
        options.ClientSeed = seed;
    }
}

internal sealed class LegacyServerBuildResolver : IPostConfigureOptions<LegacyServerOptions>
{
    private readonly IOptionsMonitor<ClientOptions> _clientOptions;

    public LegacyServerBuildResolver(IOptionsMonitor<ClientOptions> clientOptions)
    {
        _clientOptions = clientOptions;
    }

    public void PostConfigure(string? name, LegacyServerOptions options)
    {
        if (string.Equals(options.Build, "auto", StringComparison.OrdinalIgnoreCase))
        {
            options.ResolvedBuild = VersionChecker.GetBestLegacyVersion(_clientOptions.CurrentValue.ClientBuild);
            return;
        }

        if (Enum.TryParse<ClientVersionBuild>(options.Build, ignoreCase: true, out var parsed))
        {
            options.ResolvedBuild = parsed;
            return;
        }

        if (int.TryParse(options.Build, out var numericBuild) && Enum.IsDefined(typeof(ClientVersionBuild), numericBuild))
        {
            options.ResolvedBuild = (ClientVersionBuild)numericBuild;
            return;
        }

        options.ResolvedBuild = ClientVersionBuild.Zero;
    }
}

internal sealed class LoggingLegacyFlagsTranslator : IPostConfigureOptions<LoggingOptions>
{
    public void PostConfigure(string? name, LoggingOptions options)
    {
        if (options.DebugOutput && options.MinimumLevel > LogEventLevel.Debug)
            options.MinimumLevel = LogEventLevel.Debug;
        if (options.DebugOutput && options.ConsoleLevel > LogEventLevel.Debug)
            options.ConsoleLevel = LogEventLevel.Debug;
        if (options.SpanStatsLog && options.PacketLevel > LogEventLevel.Verbose)
            options.PacketLevel = LogEventLevel.Verbose;
    }
}
