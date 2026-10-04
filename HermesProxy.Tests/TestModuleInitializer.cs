using System;
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
        //
        // HERMES_TEST_MODERN_BUILD / HERMES_TEST_LEGACY_BUILD swap the pair for one run. Both
        // versions are static readonly, so a build pair is a process, and that is the only way
        // to put the real version branches of another pair under test. Only the classes in
        // World/Movement are written to pass under any pair; run-version-matrix.sh drives them.
        VersionBootstrap.ModernBuild = BuildFromEnvironment("HERMES_TEST_MODERN_BUILD")
            ?? ClientVersionBuild.V1_14_2_42597;

        // LegacyVersion.Build is a static readonly initialised on first touch; if that
        // happens while LegacyBuild is Zero the type initializer throws and the type stays
        // poisoned for the rest of the process. Individual classes used to set this
        // defensively, which only worked if they happened to run first. Set it here so the
        // ordering is guaranteed for every test. V3_3_5a_12340 matches what those classes
        // already chose, and is the backend every V3_4_3 descriptor test translates from.
        if (VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            VersionBootstrap.LegacyBuild = BuildFromEnvironment("HERMES_TEST_LEGACY_BUILD")
                ?? ClientVersionBuild.V3_3_5a_12340;
    }

    private static ClientVersionBuild? BuildFromEnvironment(string variable)
    {
        string? value = Environment.GetEnvironmentVariable(variable);
        if (string.IsNullOrEmpty(value))
            return null;

        // A typo must not fall back to the default pair and report green for the wrong builds.
        if (!Enum.TryParse(value, out ClientVersionBuild build) || !Enum.IsDefined(build))
            throw new InvalidOperationException($"{variable}={value} is not a ClientVersionBuild.");
        return build;
    }
}
