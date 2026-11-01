using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace HermesProxy.Configuration;

/// <param name="Default">What a new options class holds, which appsettings.json has to agree with.</param>
internal readonly record struct SettingInfo(string Section, string Key, string Default, string Description)
{
    public string ConfigPath => $"{Section}:{Key}";

    public string EnvironmentVariable => $"HERMES_{Section}__{Key}";

    public string CommandLine => $"--{Section}:{Key}=<value>";
}

/// <summary>
/// Every setting the proxy reads, and the two places that list them: <c>--help</c>, and
/// docs/configuration.md, which a test holds to <see cref="ToMarkdown"/>. The list itself is
/// generated from the <c>[ConfigSection]</c> classes, so a setting describes itself where it is
/// declared. To change the page: edit this file or a <c>[Setting]</c>, then run
/// <c>UPDATE_CONFIG_REFERENCE=1 dotnet test --filter ConfigReferenceTests</c>.
/// </summary>
internal static partial class ConfigReference
{
    private static readonly (string Usage, string Meaning)[] Arguments =
    [
        ("--help", "Print the arguments and settings, then exit."),
        ("--config <path>", "Use another settings file instead of appsettings.json."),
        ("--<Section>:<Key>=<value>", "Set any setting below. Repeatable."),
        ("--set <Key>=<value>", "The older form of the same, for the settings that have a flat key. Repeatable."),
        ("--no-version-check", "Skip the check for a newer version at startup."),
        ("--metrics", "Collect per-opcode latency and allocation metrics, with a summary in the log every 60 s."),
        ("--metrics-interval <seconds>", "The same, with a summary every <seconds> instead (1-3600)."),
    ];

    public static IReadOnlyList<SettingInfo> Settings { get; } = CreateSettings();

    private static partial SettingInfo[] CreateSettings();

    private static string Format(object? value) => value switch
    {
        null => "(unset)",
        "" => "(empty)",
        bool flag => flag ? "true" : "false",
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString()!,
    };

    private static IEnumerable<string> LegacyKeys(SettingInfo setting)
        => Program.LegacySetKeyMap
            .Where(mapping => mapping.Value == setting.ConfigPath)
            .Select(mapping => mapping.Key)
            .Order(StringComparer.Ordinal);

    public static string ToHelp()
    {
        var text = new StringBuilder("Usage: HermesProxy [arguments]\n\nArguments:\n");
        foreach ((string usage, string meaning) in Arguments)
            text.Append(CultureInfo.InvariantCulture, $"  {usage,-30} {meaning}\n");

        text.Append("\nSettings apply in this order, later ones winning: appsettings.json or the --config file,\n")
            .Append("appsettings.{Environment}.json, HERMES_<Section>__<Key> environment variables, the command line.\n");

        foreach (var section in Settings.GroupBy(static setting => setting.Section))
        {
            text.Append(CultureInfo.InvariantCulture, $"\n{section.Key}\n");
            foreach (var setting in section)
                text.Append(CultureInfo.InvariantCulture, $"  {setting.Key} (default: {setting.Default})\n      {setting.Description}\n");
        }

        return text.ToString();
    }

    public static string ToMarkdown()
    {
        var page = new StringBuilder();
        page.Append("""
            <!-- Generated from the settings' declarations: see HermesProxy/Configuration/ConfigReference.cs. -->
            # Configuration

            Every setting of the proxy, in each of the ways to set it. `HermesProxy --help` prints the same list, and running in Docker has [its own page](docker.md).

            ## Where settings come from

            Later sources override earlier ones:

            1. `appsettings.json` in the working directory, or the file passed with `--config <path>` in its place. One of them is required.
            2. `appsettings.{Environment}.json`, if there is one. `DOTNET_ENVIRONMENT` names it, and it only needs the keys that differ. A name that matches no file is ignored without a word.
            3. Environment variables, `HERMES_<Section>__<Key>`. `__` separates the section from the key.
            4. Command-line arguments.

            Keep passwords out of files: use the environment.

            ## Command line

            | Argument | Meaning |
            |---|---|

            """);
        foreach ((string usage, string meaning) in Arguments)
            page.Append(CultureInfo.InvariantCulture, $"| `{usage}` | {meaning} |\n");

        page.Append("""

            ```bash
            # Use another settings file
            HermesProxy --config MyServer.json

            # Set the server address, then the same in the older form
            HermesProxy --LegacyServerOptions:Address=logon.example.com
            HermesProxy --set ServerAddress=logon.example.com

            # The same from the environment
            HERMES_LegacyServerOptions__Address=logon.example.com HermesProxy

            # Metrics every 15 seconds, with verbose packet logging
            HermesProxy --metrics-interval 15 --LoggingOptions:PacketLevel=Debug
            ```

            What the metrics summary shows: [metrics.md](metrics.md).

            ## Settings

            """);
        foreach (var section in Settings.GroupBy(static setting => setting.Section))
        {
            page.Append(CultureInfo.InvariantCulture, $"\n### {section.Key}\n\n");
            page.Append("| `appsettings.json` key | Environment variable | Command line | `--set` key | Default | Description |\n");
            page.Append("|---|---|---|---|---|---|\n");
            foreach (var setting in section)
            {
                string legacyKeys = string.Join(", ", LegacyKeys(setting).Select(static key => $"`{key}`"));
                page.Append(CultureInfo.InvariantCulture,
                    $"| `{setting.Key}` | `{setting.EnvironmentVariable}` | `{setting.CommandLine}` | {legacyKeys} | `{setting.Default}` | {setting.Description} |\n");
            }
        }

        return page.ToString();
    }
}
