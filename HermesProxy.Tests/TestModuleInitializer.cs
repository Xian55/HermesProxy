using System.Runtime.CompilerServices;
using HermesProxy;
using HermesProxy.Enums;

namespace HermesProxy.Tests;

internal static class TestModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        // Assign via the bootstrap holder so first access to ModernVersion in any test fires
        // its static field initializers against this build. The actual opcode-dictionary load
        // is deferred to the first test that touches ModernVersion (keeps the xUnit v3
        // stdin/stdout handshake clean — static init doesn't log under ModuleInitializer).
        VersionBootstrap.ModernBuild = ClientVersionBuild.V1_14_2_42597;
    }
}
