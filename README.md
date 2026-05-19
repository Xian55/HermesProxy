# HermesProxy ![Build](https://github.com/Xian55/HermesProxy/actions/workflows/Build_Proxy.yml/badge.svg)

This project enables play on existing legacy WoW emulation cores using the modern clients. It serves as a translation layer, converting all network traffic to the appropriate format each side can understand.

There are 4 major components to the application:
- The modern **BNetServer** to which the client initially logs into.
- The legacy **AuthClient** which will in turn login to the remote authentication server (realmd).
- The modern **WorldServer** to which the game client will connect once a realm has been selected.
- The legacy **WorldClient** which communicates with the remote world server (mangosd).

## Supported Versions

HermesProxy translates between modern WoW Classic clients and legacy private server emulators. **WotLK Classic (3.4.x → 3.3.5a) support is experimental** — see [wotlk.md](wotlk.md) for the current status and known limitations.

### Modern Client Versions (What You Play With)

These are the Blizzard WoW Classic client versions you can use:

| Version | Expansion      | Build Range       | Notes                    |
|---------|----------------|-------------------|--------------------------|
| 1.14.0  | Classic Era    | 39802 - 40618     | Season of Mastery        |
| 1.14.1  | Classic Era    | 40487 - 42032     |                          |
| 1.14.2  | Classic Era    | 41858 - 42597     |                          |
| 2.5.2   | TBC Classic    | 39570 - 41510     |                          |
| 2.5.3   | TBC Classic    | 41402 - 42598     |                          |
| 3.4.3   | WotLK Classic  | 51505 - 54261     | Experimental — see [wotlk.md](wotlk.md) |

### Legacy Server Versions (What Emulators Run)

These are the private server versions HermesProxy can connect to:

| Version | Expansion | Build | Server Software          |
|---------|-----------|-------|--------------------------|
| 1.12.1  | Vanilla   | 5875  | CMaNGOS, VMaNGOS, etc.   |
| 1.12.2  | Vanilla   | 6005  | CMaNGOS, VMaNGOS, etc.   |
| 1.12.3  | Vanilla   | 6141  | CMaNGOS, VMaNGOS, etc.   |
| 2.4.3   | TBC       | 8606  | CMaNGOS, etc.            |
| 3.3.5a  | WotLK     | 12340 | [TrinityCore](https://www.ownedcore.com/forums/world-of-warcraft/world-of-warcraft-emulator-servers/wow-emu-general-releases/966706-repack-trinitycore-npcbots-eluna-extras-335a-solo-lan-2022-a.html), AzerothCore, CMaNGOS |

### Version Mapping

The proxy automatically selects the best legacy version based on your client:

| Modern Client | Connects To    |
|---------------|----------------|
| 1.14.x        | 1.12.x (Vanilla) |
| 2.5.x         | 2.4.3 (TBC)    |
| 3.4.x         | 3.3.5a (WotLK) |

## Download HermesProxy

Stable Downloads: [Releases](https://github.com/Xian55/HermesProxy/releases)

## Usage Instructions

- Edit `appsettings.json` next to the proxy executable — set `ClientOptions:ClientBuild` for your game client, and `LegacyServerOptions:Build` / `:Address` / `:Port` for the remote realmd. Anything in the file can also be overridden at launch via `--Section:Key=Value` or the legacy `--set Key=Value`.
- Go into your game folder, in the Classic or Classic Era subdirectory, and edit `WTF/Config.wtf` to set the portal to `127.0.0.1`.
- Download [Arctium Launcher](https://arctium.io/wow/) into the main game folder
   - Vanilla `--staticseed --version=ClassicEra`
   - TBC `--staticseed --version=Classic`
   - WotLK `--staticseed --version=Classic` (Arctium reuses the same flag as TBC)
- Start the proxy app and login through the game with your usual credentials.

## Ingame Settings

Note: Keep `Optimize Network for Speed` **enabled** (it's under `System` -> `Network`), otherwise you will get kicked every now and then.

## Known Issues

See [docs/known-issues.md](docs/known-issues.md) for current client/server quirks and their workarounds.

## Chat Commands

HermesProxy provides some internal chat commands:

| Command                    | Description                                                                  |
|----------------------------|------------------------------------------------------------------------------|
| `!qcomplete <questId>`     | Manually marks a quest as already completed (useful for quest helper addons) |
| `!quncomplete <questId>`   | Unmarks a quest as completed                                                 |

## Command Line Arguments

| Argument                                | Description                                                                                     |
|-----------------------------------------|-------------------------------------------------------------------------------------------------|
| `--config <path>`                       | Use an alternate `appsettings.json` (default: `appsettings.json` in the working directory)      |
| `--Section:Key=Value`                   | Native override of any appsettings key (repeatable). Section = `ClientOptions`, `LegacyServerOptions`, `ProxyNetworkOptions`, `LoggingOptions`, `DiagnosticsOptions`. |
| `--set <Key>=<Value>`                   | Legacy override syntax — flat keys translated to their section-qualified equivalents.           |
| `--no-version-check`                    | Disable the check for newer versions on startup                                                 |
| `--metrics`                             | Enable per-opcode latency metrics collection. A top-20 summary is logged every 60 s (see below) |

Environment variables prefixed with `HERMES_` are also picked up — e.g. `HERMES_LegacyServerOptions__Address=logon.example.com`.

**Examples:**
```bash
# Use a custom appsettings file
HermesProxy --config MyServer.json

# Override server address (native section:key syntax)
HermesProxy --LegacyServerOptions:Address=logon.example.com

# Override multiple values
HermesProxy --LegacyServerOptions:Address=logon.example.com --LegacyServerOptions:Port=3725

# Legacy --set form still works
HermesProxy --set ServerAddress=logon.example.com --set ServerPort=3725

# Run with latency metrics enabled (a summary is written to the log every 60 seconds)
HermesProxy --metrics

# Metrics + verbose packet capture, handy when profiling a suspicious opcode
HermesProxy --metrics --LoggingOptions:PacketLevel=Debug
```

### `--metrics` Details

When enabled, HermesProxy tracks per-opcode round-trip latency for both directions (Client → Server and Server → Client) and emits a summary line every minute through the `Server` logging category. Nothing is printed when no traffic has been observed, so idle proxies stay quiet.

```
17:42:14 | I | S | Server          | Latency Metrics: 842 C->S opcodes, 1203 S->C opcodes tracked
17:42:14 | I | S | Server          | <top 20 opcodes with min / max / avg / p99 latency>
```

Useful for: identifying slow-dispatching opcodes, spotting regressions after a packet-handler change, and validating that hot-path migrations didn't shift the latency profile. Leave it off in normal play — it has a small per-packet timestamp overhead.

## Configuration Reference

Primary config is `appsettings.json` (loaded from the working directory, required). Layered overrides applied in order:

1. `appsettings.json` — base.
2. `appsettings.{Environment}.json` — environment-specific overlay (e.g. `appsettings.Development.json`), optional.
3. `HERMES_*` environment variables — `HERMES_Section__Key=Value` (`__` doubles as the section separator).
4. CLI args — `--Section:Key=Value` native or `--set Key=Value` legacy.

### ClientOptions

| Key                | Default                            | Description                                                                                |
|--------------------|------------------------------------|--------------------------------------------------------------------------------------------|
| `ClientBuild`      | `V2_5_2_40892`                     | `ClientVersionBuild` enum value: `V1_14_0_40618`, `V1_14_1_41794`, `V1_14_2_42597`, `V2_5_2_40892`, `V2_5_3_42328`, `V3_4_3_54261` (experimental). |
| `SeedHex`          | `179D3DC3235629D07113A9B3867F97A7` | 32-character hex string (16 bytes).                                                        |
| `ReportedOS`       | `OSX`                              | OS identifier sent to the legacy server (`OSX`, `Win`, etc.).                              |
| `ReportedPlatform` | `x86`                              | Platform identifier sent to legacy server (`x86`, `x64`).                                  |

### LegacyServerOptions

| Key       | Default     | Description                                                                                       |
|-----------|-------------|---------------------------------------------------------------------------------------------------|
| `Build`   | `auto`      | Legacy server build to target. `auto`, `V1_12_1_5875`, `V2_4_3_8606`, `V3_3_5a_12340`.            |
| `Address` | `127.0.0.1` | Address of the legacy realmd (what you'd use in `SET REALMLIST`).                                 |
| `Port`    | `3724`      | Port of the legacy authentication server.                                                         |

### ProxyNetworkOptions

All ports must be in the range `1-65535`.

| Key               | Default     | Description                                                              |
|-------------------|-------------|--------------------------------------------------------------------------|
| `ExternalAddress` | `127.0.0.1` | Your IP address for others to connect (for hosting).                     |
| `BNetPort`        | `1119`      | BNet/Portal server (use this in your `Config.wtf`).                      |
| `RestPort`        | `8081`      | REST API server.                                                         |
| `RealmPort`       | `8084`      | Realm server.                                                            |
| `InstancePort`    | `8086`      | Instance server.                                                         |

### LoggingOptions

Serilog-backed pipe-delimited logger. Per-category levels (`Server` / `Network` / `Storage` / `Packet`), color-coded console + plain rolling file at `Logs/hermes-YYYYMMDD.log`. Override at runtime via `--LoggingOptions:PacketLevel=Debug`. Optional movement trace via `HERMES_TRACE_MOVEMENT=1`.

Full reference (line format, severity/category letters, settings table, CLI examples, movement trace): [docs/logging.md](docs/logging.md).

### DiagnosticsOptions

| Key                  | Default | Description                                                                  |
|----------------------|---------|------------------------------------------------------------------------------|
| `PacketsLog`         | `true`  | Dump each session's raw packets to `PacketsLog/*.pkt` for replay/inspection. |
| `EnableMetrics`      | `false` | Same as `--metrics` flag; per-opcode latency summary every 60 s.             |
| `EnableVersionCheck` | `true`  | Check for newer HermesProxy versions on startup.                             |

### Example Configuration

```json
{
  "ClientOptions": {
    "ClientBuild": "V2_5_2_40892",
    "SeedHex": "179D3DC3235629D07113A9B3867F97A7",
    "ReportedOS": "OSX",
    "ReportedPlatform": "x86"
  },
  "LegacyServerOptions": {
    "Build": "auto",
    "Address": "127.0.0.1",
    "Port": 3724
  },
  "ProxyNetworkOptions": {
    "ExternalAddress": "127.0.0.1",
    "RestPort": 8081,
    "BNetPort": 1119,
    "RealmPort": 8084,
    "InstancePort": 8086
  },
  "LoggingOptions": {
    "MinimumLevel": "Information",
    "ServerLevel": "Information",
    "NetworkLevel": "Information",
    "StorageLevel": "Information",
    "PacketLevel": "Warning",
    "ConsoleLevel": "Information",
    "ToFile": true,
    "Directory": "Logs"
  },
  "DiagnosticsOptions": {
    "PacketsLog": true,
    "EnableMetrics": false,
    "EnableVersionCheck": true
  }
}
```

## Diagnostics

The proxy auto-enables the .NET `createdump` facility on startup so native crashes (AVs, FailFast, stack overflow, GC corruption) drop a heap mini-dump to `bin/<Release|Debug>/Logs/crash-<pid>.dmp`. Managed exceptions are routed through the existing log/flush handlers and don't need this. Open the dump in WinDbg / Visual Studio (`Debug → Open → Dump`) for the full managed stack + reachable object graph at the crash site.

To opt out — for example to defer to a different crash reporter — set `DOTNET_DbgEnableMiniDump` to any value before launch (the proxy honours a pre-existing value and skips its own configuration).

Related env vars set automatically (overridable):

| Variable                       | Default value                  | Purpose                                                |
|--------------------------------|--------------------------------|--------------------------------------------------------|
| `DOTNET_DbgEnableMiniDump`     | `1`                            | Master switch for createdump on unrecoverable faults   |
| `DOTNET_DbgMiniDumpType`       | `2` (Heap)                     | Dump scope — `1`=Mini, `2`=Heap, `3`=Triage, `4`=Full  |
| `DOTNET_DbgMiniDumpName`       | `Logs/crash-<pid>.dmp`         | Output path                                            |
| `DOTNET_CreateDumpDiagnostics` | `1`                            | Verbose stderr logging if dump generation fails        |

## Performance Optimizations

Zero-allocation packet I/O via `Span<T>`/`ref struct`, ArrayPool-backed `ByteBuffer`, cached enum mappings, `FrozenDictionary` opcode lookups, value-type `WowGuid`. Benchmarks and full breakdown in [docs/perf.md](docs/perf.md).

## ISpanWritable Implementation Status

Current coverage: **84.7%** (272 / 321 server packets converted). See [docs/ispanwritable.md](docs/ispanwritable.md) for the full converted/unconverted breakdown and MaxSize optimization table.

## Acknowledgements

Parts of this project's code are based on [CypherCore](https://github.com/CypherCore/CypherCore) and [BotFarm](https://github.com/jackpoz/BotFarm). I would like to extend my sincere thanks to these projects, as the creation of this app might have never happened without them. And I would also like to expressly thank [Modox](https://github.com/mdx7) for all his work on reverse engineering the classic clients and all the help he has personally given me.

[WowPacketParser](https://github.com/TrinityCore/WowPacketParser) is the protocol source of truth used throughout development — every field layout, opcode definition, and version-gated change in this proxy was verified against WPP's per-build handlers before being implemented.

### WotLK Classic References

The 3.4.3 → 3.3.5a port leans heavily on the following projects:

- [HermesProxy-WOTLK](https://github.com/advocaite/HermesProxy-WOTLK) — upstream WotLK Classic fork; reference for V3_4_3 field descriptors and packet layouts.
- [lineagedr/3.4.3_Source](https://github.com/lineagedr/3.4.3_Source/) — native 3.4.3 server source, used as a behavioral oracle for V3_4_3 protocol semantics.
- [RioMcBoo/CypherCoreClassicWOTLK](https://github.com/RioMcBoo/CypherCoreClassicWOTLK) — CypherCore branch targeting 3.4.3, used as a secondary oracle for server-side packet shapes.
