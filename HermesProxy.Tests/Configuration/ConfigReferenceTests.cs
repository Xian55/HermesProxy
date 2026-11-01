using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using HermesProxy.Configuration;
using HermesProxy.Configuration.Options;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace HermesProxy.Tests.Configuration;

public sealed class ConfigReferenceTests
{
    private static string RepositoryPath(string relativePath, [CallerFilePath] string thisFile = "")
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisFile)!, "..", "..", relativePath));

    [Fact]
    public void Page_MatchesTheSettings()
    {
        string path = RepositoryPath("docs/configuration.md");
        string expected = ConfigReference.ToMarkdown();

        if (Environment.GetEnvironmentVariable("UPDATE_CONFIG_REFERENCE") == "1")
            File.WriteAllText(path, expected);

        Assert.True(
            expected == File.ReadAllText(path).ReplaceLineEndings("\n"),
            "docs/configuration.md is stale: run UPDATE_CONFIG_REFERENCE=1 dotnet test --filter ConfigReferenceTests");
    }

    [Fact]
    public void Settings_ComeFromEverySectionTheProxyBinds()
    {
        string[] expected =
        [
            nameof(ClientOptions),
            nameof(LegacyServerOptions),
            nameof(ProxyNetworkOptions),
            nameof(LoggingOptions),
            nameof(DiagnosticsOptions),
            nameof(ThrottlingOptions),
        ];

        Assert.Equal(expected, ConfigReference.Settings.Select(static setting => setting.Section).Distinct());
    }

    [Fact]
    public void Settings_LeaveOutWhatTheProxyDerives()
    {
        var keys = ConfigReference.Settings.Select(static setting => setting.Key).ToArray();

        Assert.DoesNotContain(nameof(ClientOptions.ClientSeed), keys);
        Assert.DoesNotContain(nameof(LegacyServerOptions.ResolvedBuild), keys);
    }

    [Fact]
    public void LegacySetKeys_AllNameASetting()
    {
        var paths = ConfigReference.Settings.Select(static setting => setting.ConfigPath).ToHashSet();

        Assert.All(Program.LegacySetKeyMap, mapping => Assert.Contains(mapping.Value, paths));
    }

    [Fact]
    public void Defaults_AgreeWithTheShippedAppSettings()
    {
        var shipped = new ConfigurationBuilder().AddJsonFile(RepositoryPath("HermesProxy/appsettings.json")).Build();

        var keysCompared = 0;
        foreach (var setting in ConfigReference.Settings)
        {
            if (shipped[setting.ConfigPath] is not string value)
                continue;

            keysCompared++;
            Assert.Equal(value, setting.Default, ignoreCase: true);
        }

        Assert.True(keysCompared > 20, $"only {keysCompared} settings were found in appsettings.json");
    }

    [Fact]
    public void Default_NamesAnUnsetAndAnEmptySetting()
    {
        var byKey = ConfigReference.Settings.ToDictionary(static setting => setting.Key);

        Assert.Equal("(unset)", byKey[nameof(ProxyNetworkOptions.BindAddress)].Default);
        Assert.Equal("(empty)", byKey[nameof(LegacyServerOptions.Password)].Default);
        Assert.Equal("true", byKey[nameof(LoggingOptions.ToFile)].Default);
        Assert.Equal("Warning", byKey[nameof(LoggingOptions.PacketLevel)].Default);
    }

    [Fact]
    public void Help_ListsEverySettingInItsSection()
    {
        string help = ConfigReference.ToHelp();

        Assert.All(ConfigReference.Settings, setting =>
        {
            Assert.Contains($"\n{setting.Section}\n", help);
            Assert.Contains($"  {setting.Key} (default: {setting.Default})\n", help);
            Assert.Contains(setting.Description, help);
        });
    }

    [Fact]
    public void Page_ShowsAllTheFormsOfASetting()
    {
        var row = ConfigReference.ToMarkdown().Split('\n').Single(static line => line.StartsWith("| `SeedHex` ", StringComparison.Ordinal));

        Assert.Contains("`HERMES_ClientOptions__SeedHex`", row);
        Assert.Contains("`--ClientOptions:SeedHex=<value>`", row);
        Assert.Contains("`ClientSeed`, `SeedHex`", row);
    }
}
