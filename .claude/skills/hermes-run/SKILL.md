---
name: hermes-run
description: Launch HermesProxy with the WotLK V3_4_3 → 3.3.5a configuration, in Debug (verbose logs) or Release (profiling) mode
user-invocable: true
---

# HermesProxy Run

Wrap `dotnet run --project HermesProxy` with the long `--set` configuration chain that targets the cMaNGOS / TrinityCore 3.3.5a backend at `192.168.88.55:3726` from a V3_4_3_54261 client. Two modes:

- `debug` — `-c Debug`, log levels `Verbose`/`Debug`. For active development + log reading.
- `release` — `-c Release`, log levels `Information`. For profiling source (`hermes-profile cpu` / `counters`). Verbose logging skews CPU traces.

## Usage

```
/hermes-run                          # debug mode (default)
/hermes-run release                  # release mode, no metrics
/hermes-run release --metrics        # release + per-opcode latency/allocation report every 60s
/hermes-run debug --no-build         # skip dotnet build
/hermes-run release --address 127.0.0.1 --port 3724    # override target server

# Profiles
/hermes-run -ListProfiles                              # enumerate available profiles
/hermes-run -Profile wotlk-cmangos                     # load HermesProxy/profiles/wotlk-cmangos.json
/hermes-run -Profile wotlk-cmangos -Mode release       # same, release build
/hermes-run -Profile wotlk-cmangos -ServerAddress 1.2.3.4   # profile + targeted override
/hermes-run -ConfigPath C:\my\config.json              # arbitrary external config file
/hermes-run -Environment cmangos                       # DOTNET_ENVIRONMENT overlay (see "Profiles")
```

## What the script does

`scripts/run.ps1` resolves the repo root via `git -C $PSScriptRoot rev-parse --show-toplevel` (no hardcoded paths) and runs:

```
dotnet run --project HermesProxy -c <Debug|Release> -- \
  --set ClientBuild=V3_4_3_54261 \
  --set ServerBuild=V3_3_5a_12340 \
  --set ServerAddress=192.168.88.55 \
  --set ServerPort=3726 \
  --set BNetPort=1119 \
  --set RestPort=8082 \
  --set RealmPort=8085 \
  --set InstancePort=8087 \
  --set DebugOutput=true \
  --set Log.Packet.MinimumLevel=<Debug|Information> \
  --set Log.Server.MinimumLevel=<Verbose|Information> \
  --set Log.Console.MinimumLevel=<Verbose|Information> \
  [--metrics | --metrics-interval <N>]
```

The `--` separator before app args is required so `dotnet run` doesn't try to parse `--set`. The script always inserts it.

## Parameters (all optional)

| Param | Default | Notes |
|---|---|---|
| `-Mode` | `debug` | `debug` or `release` |
| `-Profile` | (none) | Load `HermesProxy/profiles/<name>.json` as base config via `--config` |
| `-ConfigPath` | (none) | Direct path to a custom config JSON (mutually exclusive with `-Profile`) |
| `-Environment` | (none) | Sets `DOTNET_ENVIRONMENT=<name>` for the layered overlay pattern |
| `-ListProfiles` | (off) | Enumerate available profiles and exit |
| `-ServerAddress` | `192.168.88.55` | Legacy auth/realm host |
| `-ServerPort` | `3726` | Legacy auth port |
| `-BNetPort` | `1119` | Modern client BNet TLS port |
| `-RestPort` | `8082` | Modern client REST port |
| `-RealmPort` | `8085` | Modern client realm-list port |
| `-InstancePort` | `8087` | Modern client world port |
| `-ClientBuild` | `V3_4_3_54261` | |
| `-ServerBuild` | `V3_3_5a_12340` | |
| `-Metrics` | (off) | Adds `--metrics` flag, enables `Framework/Metrics/ProxyMetrics.cs` per-opcode latency + allocated bytes and a GC delta line every 60s |
| `-MetricsInterval` | (off) | Adds `--metrics-interval <N>`: metrics on, summary every N seconds (1-3600). Rates, GC line and the `Int`/`IntMax` columns cover one interval |
| `-NoBuild` | (off) | Adds `--no-build` to `dotnet run` |
| `-DryRun` | (off) | Print the command and exit without running |

## Profiles

Two ways to load alternate configurations, both backed by HermesProxy's existing infrastructure (`Program.cs:74-82`):

### Mode A — full replacement via `-Profile <name>`

A profile file at `HermesProxy/profiles/<name>.json` contains a complete `appsettings.json`-shaped document. The script resolves it to an absolute path and passes it via `--config <path>`, which `Program.cs` uses *instead* of the default `appsettings.json` (see `PreprocessArgs` at line 174-180).

- File path: `HermesProxy/profiles/<name>.json` (preferred) or `HermesProxy/appsettings.<name>.json` (fallback).
- Does **not** need to be in the csproj `<Content>` glob — `--config` accepts absolute paths.
- When `-Profile` is set, the script **only emits `--set` for parameters you explicitly passed on the command line**. Profile values speak unless you override.

Example: `HermesProxy/profiles/wotlk-cmangos.json` ships out of the box and matches the WotLK / 3.3.5a target at `192.168.88.55:3726`.

To author a new profile, copy `HermesProxy/appsettings.json` to `HermesProxy/profiles/<name>.json` and tune the values. All five Options sections (`ClientOptions`, `LegacyServerOptions`, `ProxyNetworkOptions`, `LoggingOptions`, `DiagnosticsOptions`) are accepted.

### Mode B — overlay via `-Environment <name>`

The proxy already auto-loads `appsettings.{DOTNET_ENVIRONMENT}.json` on top of the base (`Program.cs:80`). The script's `-Environment` switch sets that env var for the spawned process.

Caveat: the overlay file must be **copied to the build output**, otherwise it isn't found at runtime. Today only `appsettings.json` and `appsettings.Development.json` are explicit `<Content>` items in `HermesProxy.csproj` (lines 98–104). To support arbitrary overlays:

```xml
<Content Include="appsettings.*.json">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</Content>
```

Mode B is **only useful for incremental overrides** (a few keys changed); for self-contained config Mode A is simpler.

### Override precedence

```
appsettings.json (base)
  ← appsettings.{DOTNET_ENVIRONMENT}.json (overlay, if -Environment is used)
  ← --config <profile> (full replacement, if -Profile or -ConfigPath is used)
  ← HERMES_* environment variables
  ← --set / --section:key=value command-line args (highest)
```

So `-Profile cmangos -ServerAddress 1.2.3.4` works as: profile loaded as base → script-emitted `--set ServerAddress=1.2.3.4` overrides.

## Why these defaults

Verified against `HermesProxy/Program.cs:72-208` (`LegacySetKeyMap` lines 125–154) and `README.md:69-74`. Every key maps to a real Options field; `Verbose`/`Debug`/`Information` are valid `Serilog.Events.LogEventLevel` values. Server target matches `project_cmangos_wotlk_server` in user memory.

## Companion skill

After `/hermes-run release` is up, attach diagnostics in another shell with `/hermes-profile cpu` / `counters` / `heap` / `dump`. Run `/hermes-profile install` once first to fetch the `dotnet-trace` family of CLI tools.

## Pitfalls

- **`-c Debug` distorts profiling.** Inlining is off, JIT optimizations are off, hot-path rankings get skewed. Use `release` for profiler runs.
- **Config name has a colon, the override syntax uses dots.** Internally `Log.Packet.MinimumLevel` becomes `LoggingOptions:PacketLevel`; the dot form is what `--set` expects.
- **Don't forget `--` before app args.** `dotnet run --set Foo=Bar` may parse `--set` as a `dotnet run` arg in some .NET versions. The script always uses `dotnet run … -- --set Foo=Bar`.
- **`dotnet run` won't propagate Ctrl+C to the proxy on first press in some shells.** If the proxy hangs after Ctrl+C, hit it again or run `Get-Process -Name HermesProxy,dotnet -ErrorAction SilentlyContinue | Stop-Process -Force`.
