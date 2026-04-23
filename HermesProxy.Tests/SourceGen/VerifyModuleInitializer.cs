using System.Runtime.CompilerServices;
using DiffEngine;

namespace HermesProxy.Tests.SourceGen;

// Disable Verify's auto-diff tool launch. On CI and headless local runs we never want a
// mismatch to spawn vim/WinMerge/VS Code diff — a Verify failure should just fail the test
// with the standard assertion output, leaving the .received.txt next to the .verified.txt
// for a human to inspect manually.
internal static class VerifyModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        DiffRunner.Disabled = true;
    }
}
