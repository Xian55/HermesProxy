using System.Net;
using System.Net.Sockets;
using HermesProxy.Enums;
using Microsoft.Extensions.Options;

namespace HermesProxy.Configuration.Options;

internal sealed class ClientOptionsValidator : IValidateOptions<ClientOptions>
{
    public ValidateOptionsResult Validate(string? name, ClientOptions options)
    {
        if (options.ClientSeed.Length != 16)
            return ValidateOptionsResult.Fail($"{nameof(ClientOptions)}.{nameof(ClientOptions.SeedHex)} must decode to 16 bytes (32 hex characters)");

        if (!VersionChecker.IsSupportedModernVersion(options.ClientBuild))
            return ValidateOptionsResult.Fail($"Unsupported {nameof(ClientOptions)}.{nameof(ClientOptions.ClientBuild)} '{options.ClientBuild}'");

        return ValidateOptionsResult.Success;
    }
}

internal sealed class LegacyServerOptionsValidator : IValidateOptions<LegacyServerOptions>
{
    public ValidateOptionsResult Validate(string? name, LegacyServerOptions options)
    {
        if (!VersionChecker.IsSupportedLegacyVersion(options.ResolvedBuild))
            return ValidateOptionsResult.Fail(
                $"Unsupported {nameof(LegacyServerOptions)}.{nameof(LegacyServerOptions.Build)} '{options.Build}' (resolved to {options.ResolvedBuild}); use 'auto' to pick the best match");

        if (!IsValidPort(options.Port))
            return ValidateOptionsResult.Fail($"{nameof(LegacyServerOptions)}.{nameof(LegacyServerOptions.Port)} ({options.Port}) out of allowed range (1-65534)");

        return ValidateOptionsResult.Success;
    }

    private static bool IsValidPort(int port) => port > IPEndPoint.MinPort && port < IPEndPoint.MaxPort;
}

internal sealed class LoggingOptionsValidator : IValidateOptions<LoggingOptions>
{
    public ValidateOptionsResult Validate(string? name, LoggingOptions options)
    {
        if (options.RetentionDays is < 0 or > FileRetention.MaxDays)
            return ValidateOptionsResult.Fail(
                $"{nameof(LoggingOptions)}.{nameof(LoggingOptions.RetentionDays)} ({options.RetentionDays}) out of allowed range (0-{FileRetention.MaxDays})");

        return ValidateOptionsResult.Success;
    }
}

internal sealed class DiagnosticsOptionsValidator : IValidateOptions<DiagnosticsOptions>
{
    internal const int MinMetricsIntervalSeconds = 1;
    internal const int MaxMetricsIntervalSeconds = 3600;

    public ValidateOptionsResult Validate(string? name, DiagnosticsOptions options)
    {
        if (options.PacketsLogRetentionDays is < 0 or > FileRetention.MaxDays)
            return ValidateOptionsResult.Fail(
                $"{nameof(DiagnosticsOptions)}.{nameof(DiagnosticsOptions.PacketsLogRetentionDays)} ({options.PacketsLogRetentionDays}) out of allowed range (0-{FileRetention.MaxDays})");


        if (options.MetricsIntervalSeconds is < MinMetricsIntervalSeconds or > MaxMetricsIntervalSeconds)
            return ValidateOptionsResult.Fail(
                $"{nameof(DiagnosticsOptions)}.{nameof(DiagnosticsOptions.MetricsIntervalSeconds)} ({options.MetricsIntervalSeconds}) out of allowed range ({MinMetricsIntervalSeconds}-{MaxMetricsIntervalSeconds})");

        return ValidateOptionsResult.Success;
    }
}

internal sealed class ProxyNetworkOptionsValidator : IValidateOptions<ProxyNetworkOptions>
{
    public ValidateOptionsResult Validate(string? name, ProxyNetworkOptions options)
    {
        // The client gets this address in packets that carry an IP, so a host name cannot work.
        if (!IPAddress.TryParse(options.ExternalAddress, out IPAddress? externalAddress))
            return ValidateOptionsResult.Fail($"{nameof(ProxyNetworkOptions)}.{nameof(ProxyNetworkOptions.ExternalAddress)} ('{options.ExternalAddress}') must be the IP address clients use to reach this proxy, not a host name");
        if (externalAddress.Equals(IPAddress.Any) || externalAddress.Equals(IPAddress.IPv6Any))
            return ValidateOptionsResult.Fail($"{nameof(ProxyNetworkOptions)}.{nameof(ProxyNetworkOptions.ExternalAddress)} ('{options.ExternalAddress}') is sent to clients as the address to connect to. Use the IP address they reach this proxy on; {nameof(ProxyNetworkOptions.BindAddress)} sets what the proxy listens on");

        if (!string.IsNullOrEmpty(options.BindAddress)
            && (!IPAddress.TryParse(options.BindAddress, out IPAddress? bindAddress) || bindAddress.AddressFamily != AddressFamily.InterNetwork))
            return ValidateOptionsResult.Fail($"{nameof(ProxyNetworkOptions)}.{nameof(ProxyNetworkOptions.BindAddress)} ('{options.BindAddress}') must be an IPv4 address, 0.0.0.0 for every interface");

        if (!IsValidPort(options.RestPort))
            return ValidateOptionsResult.Fail($"{nameof(ProxyNetworkOptions)}.{nameof(ProxyNetworkOptions.RestPort)} ({options.RestPort}) out of allowed range (1-65534)");
        if (!IsValidPort(options.BNetPort))
            return ValidateOptionsResult.Fail($"{nameof(ProxyNetworkOptions)}.{nameof(ProxyNetworkOptions.BNetPort)} ({options.BNetPort}) out of allowed range (1-65534)");
        if (!IsValidPort(options.RealmPort))
            return ValidateOptionsResult.Fail($"{nameof(ProxyNetworkOptions)}.{nameof(ProxyNetworkOptions.RealmPort)} ({options.RealmPort}) out of allowed range (1-65534)");
        if (!IsValidPort(options.InstancePort))
            return ValidateOptionsResult.Fail($"{nameof(ProxyNetworkOptions)}.{nameof(ProxyNetworkOptions.InstancePort)} ({options.InstancePort}) out of allowed range (1-65534)");

        if (!string.IsNullOrEmpty(options.CertificatePfxPath) && !System.IO.File.Exists(options.CertificatePfxPath))
            return ValidateOptionsResult.Fail($"{nameof(ProxyNetworkOptions)}.{nameof(ProxyNetworkOptions.CertificatePfxPath)} file not found: '{options.CertificatePfxPath}'");

        return ValidateOptionsResult.Success;
    }

    private static bool IsValidPort(int port) => port > IPEndPoint.MinPort && port < IPEndPoint.MaxPort;
}
