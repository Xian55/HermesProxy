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
| `--help` | Print the arguments and settings, then exit. |
| `--config <path>` | Use another settings file instead of appsettings.json. |
| `--<Section>:<Key>=<value>` | Set any setting below. Repeatable. |
| `--set <Key>=<value>` | The older form of the same, for the settings that have a flat key. Repeatable. |
| `--no-version-check` | Skip the check for a newer version at startup. |
| `--metrics` | Collect per-opcode latency and allocation metrics, with a summary in the log every 60 s. |
| `--metrics-interval <seconds>` | The same, with a summary every <seconds> instead (1-3600). |

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

### ClientOptions

| `appsettings.json` key | Environment variable | Command line | `--set` key | Default | Description |
|---|---|---|---|---|---|
| `ClientBuild` | `HERMES_ClientOptions__ClientBuild` | `--ClientOptions:ClientBuild=<value>` | `ClientBuild` | `V2_5_2_40892` | The game client's build: V1_14_0_40618, V1_14_1_41794, V1_14_2_42597, V2_5_2_40892, V2_5_3_42328, V3_4_3_54261 (beta) or V4_4_2_60895 (preview). |
| `SeedHex` | `HERMES_ClientOptions__SeedHex` | `--ClientOptions:SeedHex=<value>` | `ClientSeed`, `SeedHex` | `179D3DC3235629D07113A9B3867F97A7` | The client seed, as 32 hex characters (16 bytes). It has to match the seed your launcher uses. |
| `ReportedOS` | `HERMES_ClientOptions__ReportedOS` | `--ClientOptions:ReportedOS=<value>` | `ReportedOS` | `OSX` | The OS reported to the legacy server, such as OSX or Win. Some servers only accept Win. |
| `ReportedPlatform` | `HERMES_ClientOptions__ReportedPlatform` | `--ClientOptions:ReportedPlatform=<value>` | `ReportedPlatform` | `x86` | The platform reported to the legacy server: x86 or x64. |

### LegacyServerOptions

| `appsettings.json` key | Environment variable | Command line | `--set` key | Default | Description |
|---|---|---|---|---|---|
| `Build` | `HERMES_LegacyServerOptions__Build` | `--LegacyServerOptions:Build=<value>` | `ServerBuild` | `auto` | The legacy server's build: auto, V1_12_1_5875, V2_4_3_8606, V3_3_5a_12340 or V4_3_4_15595. auto takes the best match for the client. |
| `Address` | `HERMES_LegacyServerOptions__Address` | `--LegacyServerOptions:Address=<value>` | `ServerAddress` | `127.0.0.1` | Address of the legacy auth server, what you would put in SET REALMLIST. |
| `Port` | `HERMES_LegacyServerOptions__Port` | `--LegacyServerOptions:Port=<value>` | `ServerPort` | `3724` | Port of the legacy auth server. |
| `Password` | `HERMES_LegacyServerOptions__Password` | `--LegacyServerOptions:Password=<value>` |  | `(empty)` | The legacy account's password, needed only by clients that log in with SRP (Cataclysm Classic 4.4.x), which never send theirs. Set it in the environment, never in a file. Empty means the proxy asks at startup. |

### ProxyNetworkOptions

| `appsettings.json` key | Environment variable | Command line | `--set` key | Default | Description |
|---|---|---|---|---|---|
| `ExternalAddress` | `HERMES_ProxyNetworkOptions__ExternalAddress` | `--ProxyNetworkOptions:ExternalAddress=<value>` | `ExternalAddress` | `127.0.0.1` | The IP address clients use to reach the proxy. It is sent to them as the address to connect to, in the realm address, the login page and SMSG_CONNECT_TO, so it cannot be 0.0.0.0 or a host name. |
| `BindAddress` | `HERMES_ProxyNetworkOptions__BindAddress` | `--ProxyNetworkOptions:BindAddress=<value>` | `BindAddress` | `(unset)` | The IPv4 address the proxy listens on, 0.0.0.0 for every interface. Unset: loopback when ExternalAddress is loopback, otherwise every interface. The Docker image sets 0.0.0.0. |
| `RestPort` | `HERMES_ProxyNetworkOptions__RestPort` | `--ProxyNetworkOptions:RestPort=<value>` | `RestPort` | `8081` | Port of the REST login server. |
| `BNetPort` | `HERMES_ProxyNetworkOptions__BNetPort` | `--ProxyNetworkOptions:BNetPort=<value>` | `BNetPort` | `1119` | Port of the BNet server. Any other than 1119 also goes in the portal in Config.wtf. |
| `RealmPort` | `HERMES_ProxyNetworkOptions__RealmPort` | `--ProxyNetworkOptions:RealmPort=<value>` | `RealmPort` | `8084` | Port of the realm server. |
| `InstancePort` | `HERMES_ProxyNetworkOptions__InstancePort` | `--ProxyNetworkOptions:InstancePort=<value>` | `InstancePort` | `8086` | Port of the instance server. |
| `CertificatePfxPath` | `HERMES_ProxyNetworkOptions__CertificatePfxPath` | `--ProxyNetworkOptions:CertificatePfxPath=<value>` | `CertificatePfxPath` | `(unset)` | Path to a PKCS#12 (.pfx) certificate served on BNetPort and RestPort. Unset: the embedded TrinityCore-compatible certificate. See docs/tls-certificate.md. |
| `CertificatePfxPassword` | `HERMES_ProxyNetworkOptions__CertificatePfxPassword` | `--ProxyNetworkOptions:CertificatePfxPassword=<value>` | `CertificatePfxPassword` | `(unset)` | Password for CertificatePfxPath. Leave it unset for a passwordless pfx. |

### LoggingOptions

| `appsettings.json` key | Environment variable | Command line | `--set` key | Default | Description |
|---|---|---|---|---|---|
| `MinimumLevel` | `HERMES_LoggingOptions__MinimumLevel` | `--LoggingOptions:MinimumLevel=<value>` | `Log.MinimumLevel` | `Information` | Floor across all outputs: Verbose, Debug, Information, Warning, Error or Fatal. See docs/logging.md. |
| `ServerLevel` | `HERMES_LoggingOptions__ServerLevel` | `--LoggingOptions:ServerLevel=<value>` | `Log.Server.MinimumLevel` | `Information` | Level of the Server category: lifecycle, configuration, startup. |
| `NetworkLevel` | `HERMES_LoggingOptions__NetworkLevel` | `--LoggingOptions:NetworkLevel=<value>` | `Log.Network.MinimumLevel` | `Information` | Level of the Network category: auth and world sockets, handshakes. |
| `StorageLevel` | `HERMES_LoggingOptions__StorageLevel` | `--LoggingOptions:StorageLevel=<value>` | `Log.Storage.MinimumLevel` | `Information` | Level of the Storage category: game data, CSV and DB2 files, hotfixes. |
| `PacketLevel` | `HERMES_LoggingOptions__PacketLevel` | `--LoggingOptions:PacketLevel=<value>` | `Log.Packet.MinimumLevel` | `Warning` | Level of the Packet category: opcode dispatch and packet traces. Verbose adds per-packet span statistics. |
| `ConsoleLevel` | `HERMES_LoggingOptions__ConsoleLevel` | `--LoggingOptions:ConsoleLevel=<value>` | `Log.Console.MinimumLevel` | `Information` | A further floor for the console alone. The log file keeps all that the categories allow. |
| `ToFile` | `HERMES_LoggingOptions__ToFile` | `--LoggingOptions:ToFile=<value>` | `Log.ToFile` | `true` | Write a log file per run to Directory. It rolls at 256 MB and a run keeps its newest 4 files. |
| `Directory` | `HERMES_LoggingOptions__Directory` | `--LoggingOptions:Directory=<value>` | `Log.Directory` | `Logs` | Directory for log files. A relative path starts at the proxy's own directory. |
| `RetentionDays` | `HERMES_LoggingOptions__RetentionDays` | `--LoggingOptions:RetentionDays=<value>` | `Log.RetentionDays` | `30` | Days to keep log files. Older ones are deleted at startup and every hour. 0 keeps them forever (0-3650). |
| `DebugOutput` | `HERMES_LoggingOptions__DebugOutput` | `--LoggingOptions:DebugOutput=<value>` | `DebugOutput` | `false` | Legacy shortcut: lowers MinimumLevel and ConsoleLevel to Debug. |
| `SpanStatsLog` | `HERMES_LoggingOptions__SpanStatsLog` | `--LoggingOptions:SpanStatsLog=<value>` | `SpanStatsLog` | `false` | Legacy shortcut: lowers PacketLevel to Verbose. |

### DiagnosticsOptions

| `appsettings.json` key | Environment variable | Command line | `--set` key | Default | Description |
|---|---|---|---|---|---|
| `PacketsLog` | `HERMES_DiagnosticsOptions__PacketsLog` | `--DiagnosticsOptions:PacketsLog=<value>` | `PacketsLog` | `true` | Dump each session's raw packets to PacketsLog/*.pkt for replay and inspection. PacketsLogRetentionDays limits how long they stay. |
| `PacketsLogRetentionDays` | `HERMES_DiagnosticsOptions__PacketsLogRetentionDays` | `--DiagnosticsOptions:PacketsLogRetentionDays=<value>` | `PacketsLogRetentionDays` | `30` | Days to keep the captures the proxy wrote. Older ones are deleted at startup and every hour. 0 keeps them forever (0-3650). |
| `EnableMetrics` | `HERMES_DiagnosticsOptions__EnableMetrics` | `--DiagnosticsOptions:EnableMetrics=<value>` |  | `false` | Collect per-opcode latency and allocation metrics, as --metrics does. |
| `MetricsIntervalSeconds` | `HERMES_DiagnosticsOptions__MetricsIntervalSeconds` | `--DiagnosticsOptions:MetricsIntervalSeconds=<value>` |  | `60` | Seconds between metrics summaries (1-3600), as --metrics-interval sets them. |
| `EnableVersionCheck` | `HERMES_DiagnosticsOptions__EnableVersionCheck` | `--DiagnosticsOptions:EnableVersionCheck=<value>` |  | `true` | Check for a newer version at startup. |
| `ForwardTransportsV343` | `HERMES_DiagnosticsOptions__ForwardTransportsV343` | `--DiagnosticsOptions:ForwardTransportsV343=<value>` |  | `true` | Forward legacy transports (elevators, zeppelins, boats) to a 3.4.3 client. false hides them. |

### ThrottlingOptions

| `appsettings.json` key | Environment variable | Command line | `--set` key | Default | Description |
|---|---|---|---|---|---|
| `PartyMemberStateMinIntervalMs` | `HERMES_ThrottlingOptions__PartyMemberStateMinIntervalMs` | `--ThrottlingOptions:PartyMemberStateMinIntervalMs=<value>` | `PartyMemberStateMinIntervalMs` | `200` | Minimum milliseconds between forwarded party-member updates that carry only health, power or position. 0 turns the throttle off. |
