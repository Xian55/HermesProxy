using System.Net;
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

internal sealed class ProxyNetworkOptionsValidator : IValidateOptions<ProxyNetworkOptions>
{
    public ValidateOptionsResult Validate(string? name, ProxyNetworkOptions options)
    {
        if (!IsValidPort(options.RestPort))
            return ValidateOptionsResult.Fail($"{nameof(ProxyNetworkOptions)}.{nameof(ProxyNetworkOptions.RestPort)} ({options.RestPort}) out of allowed range (1-65534)");
        if (!IsValidPort(options.BNetPort))
            return ValidateOptionsResult.Fail($"{nameof(ProxyNetworkOptions)}.{nameof(ProxyNetworkOptions.BNetPort)} ({options.BNetPort}) out of allowed range (1-65534)");
        if (!IsValidPort(options.RealmPort))
            return ValidateOptionsResult.Fail($"{nameof(ProxyNetworkOptions)}.{nameof(ProxyNetworkOptions.RealmPort)} ({options.RealmPort}) out of allowed range (1-65534)");
        if (!IsValidPort(options.InstancePort))
            return ValidateOptionsResult.Fail($"{nameof(ProxyNetworkOptions)}.{nameof(ProxyNetworkOptions.InstancePort)} ({options.InstancePort}) out of allowed range (1-65534)");

        return ValidateOptionsResult.Success;
    }

    private static bool IsValidPort(int port) => port > IPEndPoint.MinPort && port < IPEndPoint.MaxPort;
}
