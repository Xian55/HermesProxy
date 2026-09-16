using System.Globalization;
using System.Runtime.CompilerServices;
using HermesProxy;
using HermesProxy.Enums;

namespace HermesProxy.Tests;

internal static class TestModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        // Same pinning the proxy does at Program.cs:49. The proxy pins it at startup, so its own
        // number handling is invariant however the host is configured; the test host pins nothing
        // and inherits the machine locale, which is a different environment from the one the code
        // under test was written for. On a decimal-comma locale that difference alone fails the
        // suite -- ProxyMetrics formats its summary with the ambient culture, and most of
        // GameData's CSV loaders parse '.' decimals with it. That is what made the Mac runs need
        // LANG=en_US.UTF-8. Tests that want a different culture set it on their own thread
        // (see AreaTriggerProximityTests), because xUnit shares threads across collections.
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        // Assign via the bootstrap holder so first access to ModernVersion in any test fires
        // its static field initializers against this build. The actual opcode-dictionary load
        // is deferred to the first test that touches ModernVersion (keeps the xUnit v3
        // stdin/stdout handshake clean — static init doesn't log under ModuleInitializer).
        VersionBootstrap.ModernBuild = ClientVersionBuild.V1_14_2_42597;

        // LegacyVersion.Build is a static readonly initialised on first touch; if that
        // happens while LegacyBuild is Zero the type initializer throws and the type stays
        // poisoned for the rest of the process. Individual classes used to set this
        // defensively, which only worked if they happened to run first. Set it here so the
        // ordering is guaranteed for every test. V3_3_5a_12340 matches what those classes
        // already chose, and is the backend every V3_4_3 descriptor test translates from.
        if (VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            VersionBootstrap.LegacyBuild = ClientVersionBuild.V3_3_5a_12340;
    }
}
