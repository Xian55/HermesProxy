// Copyright (c) CypherCore <http://github.com/CypherCore> All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE. See LICENSE file in the project root for full license information.

using Framework.Constants;
using Framework.Logging;
using Framework.Web;
using HermesProxy.Configuration.Options;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace BNetServer;

public sealed class LoginServiceManager
{
    // Transitional service-locator for the handful of per-session callers that still use
    // LoginServiceManager.Instance. Populated by ProxyHostedService once DI resolves the
    // singleton. Removed in Phase 4 when BnetRestApiSession / BnetServices move to ctor injection.
    public static LoginServiceManager Instance { get; internal set; } = null!;

    private static readonly Microsoft.Extensions.Logging.ILogger _melServer = Log.CreateMelLogger(Log.CategoryServer);
    private static readonly string _sourceFile = nameof(LoginServiceManager).PadRight(15);
    private const string _netDirNone = "";

    private readonly IOptions<ProxyNetworkOptions> _networkOptions;
    private readonly IOptions<LegacyServerOptions> _legacyServerOptions;

    private FormInputs formInputs;
    private IPEndPoint externalAddress = null!;
    private IPEndPoint localAddress = null!;

    public LoginServiceManager(IOptions<ProxyNetworkOptions> networkOptions, IOptions<LegacyServerOptions> legacyServerOptions)
    {
        _networkOptions = networkOptions;
        _legacyServerOptions = legacyServerOptions;
        formInputs = new FormInputs();
    }

    public void Initialize()
    {
        int port = _networkOptions.Value.RestPort;
        if (port < 0 || port > 0xFFFF)
        {
            Log.Print(LogType.Error, $"Specified login service port ({port}) out of allowed range (1-65535), defaulting to 8081");
            port = 8081;
        }

        string configuredAddress = _networkOptions.Value.ExternalAddress;
        IPAddress? address;
        if (!IPAddress.TryParse(configuredAddress, out address))
        {
            Log.Print(LogType.Error, $"Could not resolve LoginREST.ExternalAddress {configuredAddress}");
            return;
        }
        externalAddress = new IPEndPoint(address, port);

        configuredAddress = "127.0.0.1";
        if (!IPAddress.TryParse(configuredAddress, out address))
        {
            Log.Print(LogType.Error, $"Could not resolve local address.");
            return;
        }
        localAddress = new IPEndPoint(address, port);

        // set up form inputs
        formInputs.Type = "LOGIN_FORM";

        var input = new FormInput();
        input.Id = "account_name";
        input.Type = "text";
        input.Label = "E-mail";
        input.MaxLength = 320;
        formInputs.Inputs.Add(input);

        input = new FormInput();
        input.Id = "password";
        input.Type = "password";
        input.Label = "Password";
        input.MaxLength = 128;
        formInputs.Inputs.Add(input);

        input = new FormInput();
        input.Id = "log_in_submit";
        input.Type = "submit";
        input.Label = "Log In";
        formInputs.Inputs.Add(input);
    }

    public IPEndPoint GetAddressForClient(IPAddress address)
    {
        if (IPAddress.IsLoopback(address))
            return localAddress;

        return externalAddress;
    }

    public FormInputs GetFormInput(IPAddress clientAddress)
    {
        if (!UsesSrpLogin)
            return formInputs;

        IPEndPoint endpoint = GetAddressForClient(clientAddress);
        return new FormInputs
        {
            Type = formInputs.Type,
            Inputs = formInputs.Inputs,
            SrpUrl = $"{Scheme}://{endpoint.Address}:{endpoint.Port}/bnetserver/login/srp/",
        };
    }

    /// <summary>
    /// Clients that log in with SRP: they prove they know the password and never send it.
    /// </summary>
    /// <remarks>
    /// The proxy logs into the legacy server with the client's password, so for these clients it
    /// has to be given the password up front (<see cref="LegacyServerOptions.Password"/>), and it
    /// plays the SRP server against that. Only they are offered <c>srp_url</c>: an older client
    /// offered it would stop sending the password the proxy needs.
    /// </remarks>
    public static bool UsesSrpLogin =>
        global::HermesProxy.ModernVersion.Branch == global::HermesProxy.Enums.ClientBranch.Classic
        && global::HermesProxy.ModernVersion.AddedInVersion(4, 4, 0);

    /// <summary>
    /// For SRP clients, asks for the legacy account password at the console when the environment
    /// did not set it. Returns false when SRP logins cannot work.
    /// </summary>
    public bool EnsureSrpPassword()
    {
        LegacyServerOptions options = _legacyServerOptions.Value;
        if (!UsesSrpLogin || options.Password.Length != 0)
            return true;

        if (Console.IsInputRedirected)
        {
            global::HermesProxy.ServerLogMessages.SrpPasswordMissing(_melServer, _sourceFile, _netDirNone);
            return false;
        }

        Console.Write("This client logs in with SRP and never sends its password. Legacy account password: ");
        options.Password = ReadMaskedLine();
        if (options.Password.Length != 0)
            return true;

        global::HermesProxy.ServerLogMessages.SrpPasswordMissing(_melServer, _sourceFile, _netDirNone);
        return false;
    }

    private static string ReadMaskedLine()
    {
        var password = new StringBuilder();
        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
                break;
            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length != 0)
                    password.Length--;
                continue;
            }
            if (!char.IsControl(key.KeyChar))
                password.Append(key.KeyChar);
        }
        Console.WriteLine();
        return password.ToString();
    }

    /// <summary>
    /// Serve the login web service over plain HTTP rather than HTTPS.
    /// </summary>
    /// <remarks>
    /// Clients from 2024 on (Cataclysm Classic 4.4.x here) check that service's certificate against
    /// the system trust store, which never holds the development wildcard certificate: they finish
    /// the TLS handshake, close without a request and report Glue error 14003. TrinityCore serves the
    /// service over HTTP whenever its certificate is that one (SslContext, since 10.2.6), and so do
    /// we, for those clients only; 1.14, 2.5 and 3.4.3 accept it over HTTPS and keep that.
    /// </remarks>
    public static bool UsesPlainHttp =>
        BnetServerCertificate.IsDevWildcard
        && global::HermesProxy.ModernVersion.Branch == global::HermesProxy.Enums.ClientBranch.Classic
        && global::HermesProxy.ModernVersion.AddedInVersion(4, 4, 0);

    /// <summary>"http" or "https", for the login URLs handed to the client.</summary>
    public static string Scheme => UsesPlainHttp ? "http" : "https";
}

public enum ServiceRequirement
{
    Unauthorized,
    LoggedIn,
    Always,
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class ServiceAttribute : Attribute
{
    public ServiceRequirement Requirement { get; set; }
    public OriginalHash ServiceHash { get; set; }
    public uint MethodId { get; set; }

    public ServiceAttribute(ServiceRequirement requirement, OriginalHash serviceHash, uint methodId)
    {
        Requirement = requirement;
        ServiceHash = serviceHash;
        MethodId = methodId;
    }
}
