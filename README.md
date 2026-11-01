# HermesProxy ![Build](https://github.com/Xian55/HermesProxy/actions/workflows/Build_Proxy.yml/badge.svg)

This project enables play on existing legacy WoW emulation cores using the modern clients. It serves as a translation layer, converting all network traffic to the appropriate format each side can understand.

There are 4 major components to the application:
- The modern **BNetServer** to which the client initially logs into.
- The legacy **AuthClient** which will in turn login to the remote authentication server (realmd).
- The modern **WorldServer** to which the game client will connect once a realm has been selected.
- The legacy **WorldClient** which communicates with the remote world server (mangosd).

## Supported Versions

HermesProxy translates between modern WoW Classic clients and legacy private server emulators. **WotLK Classic (3.4.3 → 3.3.5a) support is in beta.** Most of the game works, but rather than summarise that here, what has actually been verified — and on which backend — is tracked subsystem by subsystem in the [status matrix](wotlk.md).

**Cataclysm Classic (4.4.2 → 4.3.4) is a preview.** A 4.4.2 client logs in and plays: all ten classes, combat, quests, loot, vendors, trainers, professions, the bank, mail, the auction house, groups and raids, guilds and the guild bank, the dungeon finder, battlegrounds, rated arenas, Tol Barad and Wintergrasp. What is done and what is still open is generated into [docs/cata-progress.md](docs/cata-progress.md); progress is tracked in [#202](https://github.com/Xian55/HermesProxy/issues/202).

### Modern Client Versions (What You Play With)

These are the Blizzard WoW Classic client versions you can use:

| Version | Expansion      | Build Range       | Notes                    |
|---------|----------------|-------------------|--------------------------|
| 1.14.0  | Classic Era    | 39802 - 40618     | Season of Mastery        |
| 1.14.1  | Classic Era    | 40487 - 42032     |                          |
| 1.14.2  | Classic Era    | 41858 - 42597     |                          |
| 2.5.2   | TBC Classic    | 39570 - 41510     |                          |
| 2.5.3   | TBC Classic    | 41402 - 42598     |                          |
| 3.4.3   | WotLK Classic  | 54261             | Beta — `V3_4_3_54261` is the only supported build |
| 4.4.2   | Cataclysm Classic | 60895          | Early preview — `V4_4_2_60895` only, see [docs/cata-progress.md](docs/cata-progress.md) |

### Legacy Server Versions (What Emulators Run)

These are the private server versions HermesProxy can connect to:

| Version | Expansion | Build | Server Software          |
|---------|-----------|-------|--------------------------|
| 1.12.1  | Vanilla   | 5875  | CMaNGOS, VMaNGOS, etc.   |
| 1.12.2  | Vanilla   | 6005  | CMaNGOS, VMaNGOS, etc.   |
| 1.12.3  | Vanilla   | 6141  | CMaNGOS, VMaNGOS, etc.   |
| 2.4.3   | TBC       | 8606  | CMaNGOS, etc.            |
| 3.3.5a  | WotLK     | 12340 | [TrinityCore](https://github.com/TrinityCore/TrinityCore/tree/3.3.5), [AzerothCore](https://github.com/azerothcore/azerothcore-wotlk), CMaNGOS |
| 4.3.4   | Cataclysm | 15595 | [TrinityCore, Cataclysm Preservation Project](https://github.com/The-Cataclysm-Preservation-Project/TrinityCore) (preview) |

#### WotLK backend coverage

What works, opcode by opcode and on which backend, is generated into [docs/wotlk-progress.md](docs/wotlk-progress.md).

#### Cataclysm backend coverage

The 4.4.2 work is playtested against the Cataclysm Preservation Project's TrinityCore 4.3.4, with AzerothCore 3.3.5a as an earlier protocol test bed (`LegacyServerOptions:Build=V3_3_5a_12340`). The 4.3.4 server moves each player to a second connection at character login by default; HermesProxy stays on one, so set `LegacyConnectionModeEnabled = 1` in its `worldserver.conf`. A ready server is [Xian55/TrinityCore-docker](https://github.com/Xian55/TrinityCore-docker), branch `4.3.4`, where that is `TC_LEGACY_CONNECTION_MODE_ENABLED=1` in `tc.env`.

The 4.4.2 client logs in with SRP and never sends its password, so the proxy has to be given the legacy account's password to log in with. Set it in the environment before starting the proxy:

```powershell
$env:HERMES_LegacyServerOptions__Password = "your-legacy-password"   # PowerShell
```

```bash
export HERMES_LegacyServerOptions__Password='your-legacy-password'    # bash / zsh
```

Left unset, the proxy asks for it at startup and reads it without echoing; run without a console to type into, it cannot ask and logins fail. Never put it in a config file.

### Version Mapping

The proxy automatically selects the best legacy version based on your client:

| Modern Client | Connects To    |
|---------------|----------------|
| 1.14.x        | 1.12.x (Vanilla) |
| 2.5.x         | 2.4.3 (TBC)    |
| 3.4.x         | 3.3.5a (WotLK) |
| 4.4.x         | 4.3.4 (Cataclysm) |

## Download HermesProxy

Stable Downloads: [Releases](https://github.com/Xian55/HermesProxy/releases)

Docker: `ghcr.io/xian55/hermesproxy`, for amd64 and arm64. See [docs/docker.md](docs/docker.md).

## Usage Instructions

- Edit `appsettings.json` next to the proxy executable — set `ClientOptions:ClientBuild` for your game client, and `LegacyServerOptions:Build` / `:Address` / `:Port` for the remote realmd. Anything in the file can also be overridden at launch via `--Section:Key=Value` or the legacy `--set Key=Value`.
- Go into your game folder, in the Classic or Classic Era subdirectory, and edit `WTF/Config.wtf` to set the portal to `127.0.0.1`.
- Download [Arctium Launcher](https://arctium.io/wow/) into the main game folder
   - Vanilla `--staticseed --version=ClassicEra`
   - TBC `--staticseed --version=Classic`
   - WotLK `--staticseed --version=Classic` (Arctium reuses the same flag as TBC)
   - Cataclysm Classic: the Arctium **Game** Launcher (tested with 1.5.0), `--version=Classic`
   - If arctium.io is down, the client closes a few seconds after it starts (on 4.4.2, ERROR #134): see [docs/arctium-down.md](docs/arctium-down.md)
- Start the proxy app and login through the game with your usual credentials.
- Cataclysm Classic 4.4.x logs in with SRP and never sends its password, so the proxy needs the legacy account's password to log in on your behalf: set the `HERMES_LegacyServerOptions__Password` environment variable, or leave it unset and type it when the proxy asks at startup. Don't put it in a config file.

## Ingame Settings

Note: Keep `Optimize Network for Speed` **enabled** (it's under `System` -> `Network`), otherwise you will get kicked every now and then.

## Known Issues and Limitations

- [docs/known-issues.md](docs/known-issues.md) — current client/server quirks and their workarounds, per client family. WotLK Classic players should read the 3.4.3 section before reporting a bug; [wotlk.md](wotlk.md) carries the full audited status.
- [docs/known-limitations.md](docs/known-limitations.md) — what HermesProxy structurally **cannot** do. Read this first if you are trying to connect to a public server. **Warden-protected servers do not work, and that includes Warmane.**

## Chat Commands

HermesProxy provides some internal chat commands:

| Command                    | Description                                                                  |
|----------------------------|------------------------------------------------------------------------------|
| `!qcomplete <questId>`     | Manually marks a quest as already completed (useful for quest helper addons) |
| `!quncomplete <questId>`   | Unmarks a quest as completed                                                 |

## Configuration

Every setting, with its `appsettings.json` key, environment variable and command-line form, is in [docs/configuration.md](docs/configuration.md). `HermesProxy --help` prints the same list.

- [docs/docker.md](docs/docker.md): running the proxy in a container.
- [docs/logging.md](docs/logging.md): reading a log line, levels and categories.
- [docs/metrics.md](docs/metrics.md): what `--metrics` measures.
- [docs/tls-certificate.md](docs/tls-certificate.md): when the default certificate is not enough.

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

### Cataclysm Classic References

- [Xian55/TrinityCore-docker](https://github.com/Xian55/TrinityCore-docker) — `docker compose up` servers for TrinityCore 3.3.5, 4.3.4 (Cataclysm Preservation Project), `cata_classic` 4.4.2 and Wrathion 3.4.3, multi-arch with native arm64. The test servers this work is playtested and captured against.
- [The Cataclysm Preservation Project's TrinityCore](https://github.com/The-Cataclysm-Preservation-Project/TrinityCore) — the 4.3.4 server, and the reference for every 4.3.4 packet and movement layout on the legacy side.
- [TrinityCore `cata_classic`](https://github.com/TrinityCore/TrinityCore/tree/cata_classic) — native 4.4.2 server, used for plaintext captures and as a second opinion on 4.4.2 packet shapes, after the client's own readers in [docs/protocol/4.4.2.60895](docs/protocol/4.4.2.60895).
