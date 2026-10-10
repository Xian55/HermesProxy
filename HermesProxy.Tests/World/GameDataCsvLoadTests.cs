using System;
using System.IO;
using HermesProxy.World;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// Loads every CSV through the proxy's own loaders, for the build pair the test process is pinned
/// to. This is the check a regenerated CSV set has to pass: a value outside the type a loader
/// parses it with, or a column out of place, throws here rather than at proxy startup.
/// </summary>
/// <remarks>
/// Opt-in, because it changes the process's working directory and fills GameData's global stores.
/// Run it on its own after building, e.g. for the 4.4.2 set:
/// <code>
/// HERMES_TEST_CSV_LOAD=1 HERMES_TEST_MODERN_BUILD=V4_4_2_60895 \
///     dotnet HermesProxy.Tests/bin/Debug/net10.0/HermesProxy.Tests.dll \
///     --filter-class HermesProxy.Tests.World.GameDataCsvLoadTests
/// </code>
/// See HermesProxy/CSV/README.md.
/// </remarks>
public class GameDataCsvLoadTests
{
    [Fact]
    public void LoadEverything_ForThePinnedBuild()
    {
        Assert.SkipUnless(Environment.GetEnvironmentVariable("HERMES_TEST_CSV_LOAD") == "1",
            "Opt-in: set HERMES_TEST_CSV_LOAD=1 and run this class on its own.");

        string root = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(root, "HermesProxy.sln")))
            root = Path.GetDirectoryName(root.TrimEnd(Path.DirectorySeparatorChar))!;

        string previous = Environment.CurrentDirectory;
        Environment.CurrentDirectory = Path.Combine(root, "HermesProxy", "bin", "Debug");
        try
        {
            string item = Path.Combine("CSV", $"Item{ModernVersion.ExpansionVersion}.csv");
            Assert.True(File.Exists(item), $"{item} is not in the build output; build HermesProxy first.");
            GameData.LoadEverything();
        }
        finally
        {
            Environment.CurrentDirectory = previous;
        }
    }
}
