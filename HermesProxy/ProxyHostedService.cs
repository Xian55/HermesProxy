using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BNetServer;
using BNetServer.Networking;
using Framework.Logging;
using Framework.Networking;
using HermesProxy.Configuration.Options;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HermesProxy;

internal sealed class ProxyHostedService : BackgroundService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IOptions<ClientOptions> _clientOptions;
    private readonly IOptions<LegacyServerOptions> _legacyServerOptions;
    private readonly IOptions<ProxyNetworkOptions> _networkOptions;
    private readonly IOptions<LoggingOptions> _loggingOptions;
    private readonly IOptions<DiagnosticsOptions> _diagnosticsOptions;
    private readonly LoginServiceManager _loginServiceManager;
    private readonly SocketManager<BnetTcpSession> _bnetSocketManager;
    private readonly SocketManager<BnetRestApiSession> _restSocketManager;
    private readonly SocketManager<RealmSocket> _realmSocketManager;
    private readonly SocketManager<WorldSocket> _worldSocketManager;

    public ProxyHostedService(
        IHostApplicationLifetime lifetime,
        IOptions<ClientOptions> clientOptions,
        IOptions<LegacyServerOptions> legacyServerOptions,
        IOptions<ProxyNetworkOptions> networkOptions,
        IOptions<LoggingOptions> loggingOptions,
        IOptions<DiagnosticsOptions> diagnosticsOptions,
        LoginServiceManager loginServiceManager,
        SocketManager<BnetTcpSession> bnetSocketManager,
        SocketManager<BnetRestApiSession> restSocketManager,
        SocketManager<RealmSocket> realmSocketManager,
        SocketManager<WorldSocket> worldSocketManager)
    {
        _lifetime = lifetime;
        _clientOptions = clientOptions;
        _legacyServerOptions = legacyServerOptions;
        _networkOptions = networkOptions;
        _loggingOptions = loggingOptions;
        _diagnosticsOptions = diagnosticsOptions;
        _loginServiceManager = loginServiceManager;
        _bnetSocketManager = bnetSocketManager;
        _restSocketManager = restSocketManager;
        _realmSocketManager = realmSocketManager;
        _worldSocketManager = worldSocketManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
#if !DEBUG
        if (_diagnosticsOptions.Value.EnableVersionCheck)
        {
            try { await Server.CheckForUpdate().WaitAsync(TimeSpan.FromSeconds(15), stoppingToken); }
            catch { /* ignore */ }
        }
#endif

        Server.MetricsEnabled = _diagnosticsOptions.Value.EnableMetrics;

        Log.Print(LogType.Server, "Starting Hermes Proxy...");
        Server.LogVersion();
        if (Server.MetricsEnabled)
            Log.Print(LogType.Server, "Latency metrics collection enabled");
        Log.Start();

        if (Environment.CurrentDirectory != Path.GetDirectoryName(AppContext.BaseDirectory))
        {
            Log.Print(LogType.Storage, "Switching working directory");
            Log.Print(LogType.Storage, $"Old: {Environment.CurrentDirectory}");
            Environment.CurrentDirectory = Path.GetDirectoryName(AppContext.BaseDirectory)!;
            Log.Print(LogType.Storage, $"New: {Environment.CurrentDirectory}");
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

        Log.Configure(_loggingOptions.Value.ToLogBootstrapOptions());
        Server.RegisterLogCallerMappings();
        Log.Print(LogType.Debug, "Debug logging enabled");

        Server.LogClientAndServerBuild(_clientOptions.Value.ClientBuild, _legacyServerOptions.Value.ResolvedBuild);

        // Must run before GameData.LoadEverything and before any code path accesses
        // ModernVersion / LegacyVersion. Those two classes are pure static-readonly shells
        // whose field initializers read these values on first type touch — if we haven't
        // assigned them yet, RequireBuild() throws and the Host fails to start (intentional).
        VersionBootstrap.ModernBuild = _clientOptions.Value.ClientBuild;
        VersionBootstrap.LegacyBuild = _legacyServerOptions.Value.ResolvedBuild;

        // Build the generated dispatch tables here, immediately after the bootstrap they depend
        // on. Their static initializers read the version statics, and a type initializer that
        // throws poisons its type for the life of the process — every later access rethrows a
        // TypeInitializationException naming the wrong culprit. Forcing them at a known point
        // keeps that failure at startup rather than on the first packet of a session.
        GeneratedCmsgDispatch.EnsureInitialized();
        GeneratedSmsgDispatch.EnsureInitialized();

        GameData.LoadEverything();

        var net = _networkOptions.Value;
        var bindIp = NetworkUtils.ResolveOrDirectIPv64(net.ExternalAddress);
        if (!IPAddress.IsLoopback(bindIp))
            bindIp = IPAddress.Any;

        Server.LogExternalIp(net.ExternalAddress);
        // Service-locator shim for the 2 call sites that still access LoginServiceManager.Instance
        // directly. Removed in Phase 4 when those call sites move to ctor injection.
        LoginServiceManager.Instance = _loginServiceManager;
        _loginServiceManager.Initialize();

        BnetServerCertificate.Initialize(net.CertificatePfxPath, net.CertificatePfxPassword);

        StartListener(_bnetSocketManager,  typeof(BnetTcpSession).Name,     new IPEndPoint(bindIp, net.BNetPort));
        StartListener(_restSocketManager,  typeof(BnetRestApiSession).Name, new IPEndPoint(bindIp, net.RestPort));
        StartListener(_realmSocketManager, typeof(RealmSocket).Name,        new IPEndPoint(bindIp, net.RealmPort));
        StartListener(_worldSocketManager, typeof(WorldSocket).Name,        new IPEndPoint(bindIp, net.InstancePort));

        try
        {
            int metricsLogCounter = 0;
            const int metricsLogIntervalSeconds = 60;
            const int loopIntervalSeconds = 10;
            const int displayMetricCount = 20;

            while (!stoppingToken.IsCancellationRequested &&
                   (_bnetSocketManager.IsListening || _restSocketManager.IsListening
                    || _realmSocketManager.IsListening || _worldSocketManager.IsListening))
            {
                await Task.Delay(TimeSpan.FromSeconds(loopIntervalSeconds), stoppingToken);

                if (Server.MetricsEnabled)
                {
                    metricsLogCounter += loopIntervalSeconds;
                    if (metricsLogCounter >= metricsLogIntervalSeconds)
                    {
                        metricsLogCounter = 0;

                        // The GC line goes out even on an idle proxy so a baseline run has a
                        // continuous allocation/collection series to line up against packet bursts.
                        var gc = Server.Metrics.CaptureGcDelta();
                        if (Server.Metrics.ClientToServerOpcodeCount > 0 || Server.Metrics.ServerToClientOpcodeCount > 0)
                        {
                            Log.Print(LogType.Server, $"Proxy Metrics: {Server.Metrics.ClientToServerOpcodeCount} C->S opcodes, {Server.Metrics.ServerToClientOpcodeCount} S->C opcodes tracked");

                            foreach (var line in Server.Metrics.GetSummary(displayMetricCount, Server.ResolveOpcodeName, gc).Split('\n'))
                            {
                                if (!string.IsNullOrWhiteSpace(line))
                                    Log.Print(LogType.Server, line);
                            }
                        }
                        else
                        {
                            Log.Print(LogType.Server, gc.ToSummaryLine());
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException) { /* normal shutdown */ }
        finally
        {
            TryStop(_worldSocketManager);
            TryStop(_realmSocketManager);
            TryStop(_restSocketManager);
            TryStop(_bnetSocketManager);
        }
    }

    private static void StartListener<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(SocketManager<T> socketManager, string serviceName, IPEndPoint endpoint)
        where T : ISocket
    {
        Server.LogStartingService(serviceName, endpoint.ToString());
        if (!socketManager.StartNetwork(endpoint.Address.ToString(), endpoint.Port))
            throw new Exception($"Failed to start {serviceName} service");

        Thread.Sleep(50); // Let the listener thread log before the next service starts.
    }

    private static void TryStop<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(SocketManager<T> socketManager) where T : ISocket
    {
        try
        {
            if (socketManager.IsListening)
                socketManager.StopNetwork();
        }
        catch (Exception ex)
        {
            Log.outException(ex);
        }
    }
}
