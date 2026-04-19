# HermesProxy ![Build](https://github.com/Xian55/HermesProxy/actions/workflows/Build_Proxy.yml/badge.svg)

This project enables play on existing legacy WoW emulation cores using the modern clients. It serves as a translation layer, converting all network traffic to the appropriate format each side can understand.

There are 4 major components to the application:
- The modern BNetServer to which the client initially logs into.
- The legacy AuthClient which will in turn login to the remote authentication server (realmd).
- The modern WorldServer to which the game client will connect once a realm has been selected.
- The legacy WorldClient which communicates with the remote world server (mangosd).

## Supported Versions

HermesProxy translates between modern WoW Classic clients and legacy private server emulators.

### Modern Client Versions (What You Play With)

These are the Blizzard WoW Classic client versions you can use:

| Version | Expansion      | Build Range       | Notes                    |
|---------|----------------|-------------------|--------------------------|
| 1.14.0  | Classic Era    | 39802 - 40618     | Season of Mastery        |
| 1.14.1  | Classic Era    | 40487 - 42032     |                          |
| 1.14.2  | Classic Era    | 41858 - 42597     |                          |
| 2.5.2   | TBC Classic    | 39570 - 41510     |                          |
| 2.5.3   | TBC Classic    | 41402 - 42598     |                          |

### Legacy Server Versions (What Emulators Run)

These are the private server versions HermesProxy can connect to:

| Version | Expansion | Build | Server Software          |
|---------|-----------|-------|--------------------------|
| 1.12.1  | Vanilla   | 5875  | CMaNGOS, VMaNGOS, etc.   |
| 1.12.2  | Vanilla   | 6005  | CMaNGOS, VMaNGOS, etc.   |
| 1.12.3  | Vanilla   | 6141  | CMaNGOS, VMaNGOS, etc.   |
| 2.4.3   | TBC       | 8606  | CMaNGOS, etc.            |

### Version Mapping

The proxy automatically selects the best legacy version based on your client:

| Modern Client | Connects To    |
|---------------|----------------|
| 1.14.x        | 1.12.x (Vanilla) |
| 2.5.x         | 2.4.3 (TBC)    |

## Ingame Settings
Note: Keep `Optimize Network for Speed` **enabled** (it's under `System` -> `Network`), otherwise you will get kicked every now and then.

## Usage Instructions

- Edit the app's config to specify the exact versions of your game client and the remote server, along with the address.
- Go into your game folder, in the Classic or Classic Era subdirectory, and edit WTF/Config.wtf to set the portal to 127.0.0.1.
- Download [Arctium Launcher](https://github.com/Arctium/WoW-Launcher/releases/tag/latest) into the main game folder, and then run it
with `--staticseed --version=ClassicEra` for vanilla
or `--staticseed --version=Classic` for TBC.
- Start the proxy app and login through the game with your usual credentials.

## Chat Commands

HermesProxy provides some internal chat commands:

| Command                    | Description                                                                  |
|----------------------------|------------------------------------------------------------------------------|
| `!qcomplete <questId>`     | Manually marks a quest as already completed (useful for quest helper addons) |
| `!quncomplete <questId>`   | Unmarks a quest as completed                                                 |

## Command Line Arguments

| Argument                   | Description                                                                                     |
|----------------------------|-------------------------------------------------------------------------------------------------|
| `--config <filename>`      | Specify a config file (default: `HermesProxy.config`)                                           |
| `--set <key>=<value>`      | Override a specific config value at runtime (repeatable)                                        |
| `--no-version-check`       | Disable the check for newer versions on startup                                                 |
| `--metrics`                | Enable per-opcode latency metrics collection. A top-20 summary is logged every 60 s (see below) |

**Examples:**
```bash
# Use a custom config file
HermesProxy --config MyServer.config

# Override server address
HermesProxy --set ServerAddress=logon.example.com

# Override multiple values
HermesProxy --set ServerAddress=logon.example.com --set ServerPort=3725

# Combine config file with overrides
HermesProxy --config Production.config --set DebugOutput=true

# Run with latency metrics enabled (a summary is written to the log every 60 seconds)
HermesProxy --metrics

# Metrics + verbose packet capture, handy when profiling a suspicious opcode
HermesProxy --metrics --set Log.Packet.MinimumLevel=Debug
```

### `--metrics` Details

When enabled, HermesProxy tracks per-opcode round-trip latency for both directions (Client → Server and Server → Client) and emits a summary line every minute through the `Server` logging category. Nothing is printed when no traffic has been observed, so idle proxies stay quiet.

```
17:42:14 | I | S | Server          | Latency Metrics: 842 C->S opcodes, 1203 S->C opcodes tracked
17:42:14 | I | S | Server          | <top 20 opcodes with min / max / avg / p99 latency>
```

Useful for: identifying slow-dispatching opcodes, spotting regressions after a packet-handler change, and validating that hot-path migrations didn't shift the latency profile. Leave it off in normal play — it has a small per-packet timestamp overhead.

## Configuration Reference

Configuration is stored in `HermesProxy.config` (XML format). All settings can also be overridden via `--set` command line arguments.

### Connection Settings

| Setting         | Default       | Description                                                              |
|-----------------|---------------|--------------------------------------------------------------------------|
| `ServerAddress` | `127.0.0.1`   | Address of the legacy server (what you'd use in `SET REALMLIST`)         |
| `ServerPort`    | `3724`        | Port of the legacy authentication server                                 |
| `ExternalAddress` | `127.0.0.1` | Your IP address for others to connect (for hosting)                      |

### Version Settings

| Setting       | Default                            | Valid Values                                    |
|---------------|------------------------------------|-------------------------------------------------|
| `ClientBuild` | `40618` (1.14.0)                   | `40618` (1.14.0)<br>`41794` (1.14.1)<br>`42597` (1.14.2)<br>`40892` (2.5.2)<br>`42328` (2.5.3) |
| `ServerBuild` | `auto`                             | `auto`<br>`5875` (1.12.1)<br>`8606` (2.4.3)         |
| `ClientSeed`  | `179D3DC3235629D07113A9B3867F97A7` | 32-character hex string (16 bytes)              |

### Port Settings

All ports must be in the range `1-65535`.

| Setting        | Default | Description                                                        |
|----------------|---------|--------------------------------------------------------------------|
| `BNetPort`     | `1119`  | Port for BNet/Portal server (use this in your `Config.wtf`)        |
| `RestPort`     | `8081`  | Port for the REST API server                                       |
| `RealmPort`    | `8084`  | Port for the realm server                                          |
| `InstancePort` | `8086`  | Port for the instance server                                       |

### Platform Settings

| Setting            | Default | Valid Values                    | Description                              |
|--------------------|---------|---------------------------------|------------------------------------------|
| `ReportedOS`       | `OSX`   | `OSX`, `Win`, etc.              | OS identifier sent to the legacy server  |
| `ReportedPlatform` | `x86`   | `x86`, `x64`                    | Platform identifier sent to legacy server|

### Logging

HermesProxy ships with a Serilog-backed logger that splits **severity** (single letter: `V`, `D`, `I`, `W`, `E`, `F`) from **category** (single letter: `S` = Server, `N` = Network, `T` = Storage, `P` = Packet) and colors property values in the console output (strings in cyan, numbers in yellow) so the interesting bits stand out at a glance.

```
17:00:33 | I | S | Server          | Starting WorldSocket service on 127.0.0.1:8086...
17:00:34 | I | S | BnetTcpSession  | Accepting connection from 127.0.0.1:49695.
17:09:19 | W | P | WorldSocket     | C>P S | No handler for opcode CMSG_BATTLE_PAY_GET_PURCHASE_LIST (14019) (Got unknown packet from ModernClient)
```

Each category has its own minimum level so you can dial verbosity in independently — the console has an additional floor applied on top, which lets you keep the console tidy while the file captures enough detail for a bug report.

#### Reading a Log Line

Every line has the same pipe-delimited shape so it's easy to scan vertically:

```
HH:mm:ss | L | C | SourceFile      | [NetDir | ] message
```

Worked example:

```
17:09:19 | W | P | WorldSocket     | C>P S | No handler for opcode CMSG_BATTLE_PAY_GET_PURCHASE_LIST (14019)
   ▲       ▲   ▲        ▲              ▲                       ▲                                      ▲
   │       │   │        │              │                       │                                      └── number, yellow
   │       │   │        │              │                       └── string, bright cyan
   │       │   │        │              └── optional network-direction tag (only on packet-flow events)
   │       │   │        └── caller file (class name, padded to 15 chars) — always default color
   │       │   └── category letter — blue(S) / green(N) / cyan(T) / magenta(P)
   │       └── severity letter — yellow(W) / red(E,F) / gray(V,D) / default(I)
   └── local clock (24h)
```

**Severity letter — `L`**

The first letter encodes the severity so you can grep / filter without remembering words:

| Letter | Level         | Console color | When you see it                                                     |
|--------|---------------|---------------|---------------------------------------------------------------------|
| `V`    | `Verbose`     | gray          | Per-packet `SpanStats` — only when `Log.Packet.MinimumLevel=Verbose` |
| `D`    | `Debug`       | gray          | Opcode send/receive traces when debug is enabled                    |
| `I`    | `Information` | default       | Normal lifecycle events — the default minimum                       |
| `W`    | `Warning`     | yellow        | Recoverable issues: unknown opcode, unimplemented service, SpanMiss |
| `E`    | `Error`       | red           | Socket errors, auth failures, decrypt failures                      |
| `F`    | `Fatal`       | bold red      | Reserved for unrecoverable conditions                               |

**Category letter — `C`**

The second letter tells you *which subsystem* is talking, independent of severity:

| Letter | Category  | Console color | Covers                                                                |
|--------|-----------|---------------|-----------------------------------------------------------------------|
| `S`    | `Server`  | blue          | Lifecycle, config, startup, version checks, service managers         |
| `N`    | `Network` | green         | Auth/world-socket connect/disconnect, handshake, network thread      |
| `T`    | `Storage` | cyan          | GameData load, CSV/DB2 readers, hotfix files                          |
| `P`    | `Packet`  | magenta       | Opcode dispatch, per-packet traces, Span-based serialization fallback |

This is also the selector for per-category verbosity: `Log.Packet.MinimumLevel=Debug` affects only lines where the category letter is `P`.

**Network direction tag — `NetDir`** *(optional)*

When a line represents packet flow between client, proxy, and server, the message is prefixed with a 5-character flow diagram. `C` = game client, `P` = HermesProxy, `S` = legacy server; the `>` or `<` marks the direction:

| Tag     | Meaning                                       |
|---------|-----------------------------------------------|
| `C>P S` | Client → Proxy — packet received from client  |
| `C<P S` | Proxy → Client — packet sent to client        |
| `C P>S` | Proxy → Server — packet forwarded to server   |
| `C P<S` | Server → Proxy — packet received from server  |

Lines without this tag are not packet-flow events (startup, config, etc.).

**Message and property values**

Everything after the final `|` is the human-readable message. The literal text stays at the terminal default color; property values substituted into the message are colored by type — strings go bright cyan (`CMSG_BATTLE_PAY_GET_PURCHASE_LIST`, `PROTONOX`, `192.168.88.55`), numbers go bright yellow (`14019`, `8085`, `42597`). When you're scanning for a specific opcode or account, you can spot it on color alone.

In the rolling file (`Logs/hermes-YYYYMMDD.log`) the same layout is used but without ANSI codes, so it stays grep-friendly and safe to paste into a bug report.

#### Logging Settings

| Setting                       | Default       | Description                                                                                                          |
|-------------------------------|---------------|----------------------------------------------------------------------------------------------------------------------|
| `Log.MinimumLevel`            | `Information` | Absolute floor across all sinks. Events below this level are dropped before categories/sinks are checked.            |
| `Log.Server.MinimumLevel`     | `Information` | Category override for `Server` (lifecycle, config, startup). Rendered as `S`.                                        |
| `Log.Network.MinimumLevel`    | `Information` | Category override for `Network` (auth/world socket connect, handshake). Rendered as `N`.                             |
| `Log.Storage.MinimumLevel`    | `Information` | Category override for `Storage` (GameData, CSV/DB2, hotfixes). Rendered as `T`.                                      |
| `Log.Packet.MinimumLevel`     | `Warning`     | Category override for `Packet` (opcode dispatch, span fallback/stats). Rendered as `P`. `Verbose` enables SpanStats. |
| `Log.Console.MinimumLevel`    | `Information` | Additional floor applied ONLY to the console. File sink captures everything the categories allow.                    |
| `Log.ToFile`                  | `true`        | Write a daily rolling log file to `Log.Directory/hermes-YYYY-MM-DD.log`. Last 30 days are retained automatically.    |
| `Log.Directory`               | `Logs`        | Directory (relative to the working directory) where rolling log files are placed.                                    |
| `PacketsLog`                  | `true`        | Dump each session's raw packets to a `.pkt` file in `PacketsLog/` for replay / inspection (unrelated to text logs).  |
| `DebugOutput` *(legacy)*      | `false`       | Back-compat shortcut — lowers `Log.MinimumLevel` and `Log.Console.MinimumLevel` to `Debug`.                          |
| `SpanStatsLog` *(legacy)*     | `false`       | Back-compat shortcut — lowers `Log.Packet.MinimumLevel` to `Verbose`.                                                |

Valid level values (case-insensitive): `Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal`.

#### CLI Examples

All logging settings can be overridden at runtime with `--set key=value` (repeat the flag for multiple overrides).

```bash
# Capture packet-level debug detail in the log file while keeping the console quiet.
# Ideal default for "always-on" logging — when a user reports an issue, ask for
# Logs/hermes-YYYYMMDD.log and you already have the Debug-level context.
HermesProxy --set Log.Packet.MinimumLevel=Debug --set Log.Console.MinimumLevel=Information

# Crank everything to maximum verbosity (console included) for interactive debugging.
HermesProxy --set Log.MinimumLevel=Verbose --set Log.Console.MinimumLevel=Verbose --set Log.Packet.MinimumLevel=Verbose

# Enable per-packet SpanStats output to the file only (useful for profiling packet serialization).
HermesProxy --set Log.Packet.MinimumLevel=Verbose --set Log.Console.MinimumLevel=Information

# Reduce noise on a long-running proxy — warnings and above only, even in the file.
HermesProxy --set Log.MinimumLevel=Warning --set Log.Console.MinimumLevel=Warning

# Silence the Packet category entirely (e.g. when you're debugging auth and don't care about
# "No handler for opcode" warnings), while leaving Server/Network/Storage at defaults.
HermesProxy --set Log.Packet.MinimumLevel=Error

# Opt out of the rolling file sink if disk writes are undesirable (headless containers, etc.).
HermesProxy --set Log.ToFile=false

# Point the file sink at a different location.
HermesProxy --set Log.Directory=D:/HermesLogs

# Legacy flags still work via the back-compat shortcuts.
HermesProxy --set DebugOutput=true          # == Log.MinimumLevel=Debug + Log.Console.MinimumLevel=Debug
HermesProxy --set SpanStatsLog=true         # == Log.Packet.MinimumLevel=Verbose
```

#### Bug-Report Workflow

With the defaults (`Log.ToFile=true`, file captures per-category levels, console filtered by `Log.Console.MinimumLevel`), a typical error triage goes:

1. User reports the issue and shares `Logs/hermes-<date>.log` for the day it happened.
2. The file contains all startup context (version, client/server build, bound ports, resolved realms) and the per-session flow (auth, realm-list, world-socket handshake, opcode handler warnings, etc.).
3. If you need even more detail for a particular subsystem, ask the user to rerun with for example `--set Log.Packet.MinimumLevel=Debug` and resend the log.

### Example Configuration

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <appSettings>
    <add key="ClientBuild" value="40618" />
    <add key="ServerBuild" value="auto" />
    <add key="ServerAddress" value="logon.example.com" />
    <add key="ServerPort" value="3724" />
    <add key="BNetPort" value="1119" />
    <add key="PacketsLog" value="true" />
    <add key="Log.MinimumLevel" value="Information" />
    <add key="Log.Packet.MinimumLevel" value="Debug" />
    <add key="Log.Console.MinimumLevel" value="Information" />
    <add key="Log.ToFile" value="true" />
  </appSettings>
</configuration>
```

## Performance Optimizations

HermesProxy has been extensively optimized to minimize latency and memory allocations in packet handling hot paths.

### Span-Based Packet I/O (Zero-Allocation)

The packet serialization system uses `Span<T>` and `ref struct` types for zero-allocation packet writing and reading:

**SpanPacketWriter vs ByteBuffer (Write Operations)**

| Operation    | ByteBuffer | SpanWriter | Speedup | Memory      |
|--------------|------------|------------|---------|-------------|
| WriteInt64   | 93.37 ns   | 0.29 ns    | ~317x   | 80B → 0B    |
| WriteVector3 | 102.99 ns  | 0.68 ns    | ~151x   | 88B → 0B    |
| WriteMixed   | 109.30 ns  | 1.29 ns    | ~85x    | 96B → 0B    |

**SpanPacketReader vs ByteBuffer (Read Operations)**

| Operation    | ByteBuffer | SpanReader | Speedup  | Memory      |
|--------------|------------|------------|----------|-------------|
| ReadInt64    | 157.98 ns  | 0.08 ns    | ~1948x   | 48B → 0B    |
| ReadVector3  | 178.31 ns  | 0.75 ns    | ~238x    | 48B → 0B    |
| ReadCString  | 294.61 ns  | 23.51 ns   | ~12.5x   | 104B → 56B  |

### ByteBuffer Optimizations

The core `ByteBuffer` class has been refactored for improved performance:
- ArrayPool-based buffer management reduces GC pressure
- Direct `BinaryPrimitives` usage eliminates BinaryReader/BinaryWriter overhead
- `MemoryStream.ToArray()` optimization for `GetData()`:

| Buffer Size | Original     | Optimized   | Speedup |
|-------------|--------------|-------------|---------|
| Small       | 46.87 ns     | 10.31 ns    | ~4.5x   |
| Medium      | 649.49 ns    | 70.88 ns    | ~9.2x   |
| Large       | 36,383.19 ns | 4,234.92 ns | ~8.6x   |

### Additional Optimizations

- **Enum Conversions**: Cached name-based mappings replace `Enum.Parse(typeof(T), x.ToString())` pattern (8-25x speedup, 95% memory reduction)
- **Opcode Lookups**: `FrozenDictionary` for O(1) opcode resolution
- **WowGuid**: Refactored to value-type record structs eliminating heap allocations
- **NetworkThread**: O(1) socket removal with `ConcurrentQueue`
- **BnetTcpSession**: Zero-allocation buffer management with `Span<T>`
- **Movement Handlers**: Fixed monster/pet movement zig-zag at tile boundaries

## ISpanWritable Implementation Status

This section tracks the progress of implementing `ISpanWritable` interface for server packets to enable zero-allocation span-based packet writing.

### Current Progress

| Metric | Count |
|--------|-------|
| **Packets WITH ISpanWritable** | 272 |
| **Packets WITHOUT ISpanWritable** | 49 |
| **Total ServerPackets** | 321 |
| **Coverage** | 84.7% |

### Converted Packets (272)

<details>
<summary>Click to expand full list</summary>

#### AuthenticationPackets.cs (3)
- `Pong`
- `WaitQueueFinish`
- `ResumeComms`

#### AuctionPackets.cs (5)
- `AuctionHelloResponse`
- `AuctionClosedNotification` *(uses AuctionPacketHelpers for ItemInstance)*
- `AuctionOwnerBidNotification` *(uses AuctionPacketHelpers for ItemInstance)*
- `AuctionWonNotification` *(uses AuctionPacketHelpers for ItemInstance)*
- `AuctionOutbidNotification` *(uses AuctionPacketHelpers for ItemInstance)*

#### BattleGroundPackets.cs (5)
- `BattlegroundInit`
- `BattlegroundPlayerLeftOrJoined`
- `AreaSpiritHealerTime`
- `PvPCredit`
- `PlayerSkinned`

#### CharacterPackets.cs (13)
- `CreateChar`
- `DeleteChar`
- `LoginVerifyWorld`
- `CharacterLoginFailed`
- `LogoutResponse`
- `LogoutComplete`
- `LogoutCancelAck`
- `LogXPGain`
- `PlayedTime`
- `InspectHonorStatsResultClassic`
- `InspectHonorStatsResultTBC`
- `GenerateRandomCharacterNameResult` *(bounded string - player name)*
- `CharacterRenameResult` *(bounded string - player name)*

#### ChatPackets.cs (2)
- `STextEmote`
- `ChatPlayerNotfound` *(bounded string - player name)*

#### ClientConfigPackets.cs (1)
- `ClientCacheVersion`

#### CombatPackets.cs (6)
- `AttackSwingError`
- `SAttackStart`
- `SAttackStop`
- `CancelCombat`
- `AIReaction`
- `PartyKillLog`

#### DuelPackets.cs (7)
- `CanDuelResult`
- `DuelRequested`
- `DuelCountdown`
- `DuelComplete`
- `DuelInBounds`
- `DuelOutOfBounds`
- `DuelWinner` *(bounded string - 2 player names)*

#### GameObjectPackets.cs (5)
- `GameObjectDespawn`
- `GameObjectResetState`
- `GameObjectCustomAnim`
- `FishNotHooked`
- `FishEscaped`

#### GroupPackets.cs (12)
- `GroupUninvite`
- `ReadyCheckStarted`
- `ReadyCheckResponse`
- `ReadyCheckCompleted`
- `SendRaidTargetUpdateSingle`
- `SendRaidTargetUpdateAll` *(capped 8 raid markers)*
- `SummonRequest`
- `MinimapPing`
- `RandomRoll`
- `GroupDecline` *(bounded string - player name)*
- `GroupNewLeader` *(bounded string - player name)*
- `PartyCommandResult` *(bounded string - player name)*

#### GuildPackets.cs (16)
- `GuildCommandResult` *(bounded string - guild/player name)*
- `GuildSendRankChange`
- `GuildEventDisbanded`
- `GuildEventRanksUpdated`
- `GuildEventTabAdded`
- `GuildEventBankMoneyChanged`
- `GuildEventTabTextChanged`
- `PlayerTabardVendorActivate`
- `PlayerSaveGuildEmblem`
- `GuildBankRemainingWithdrawMoney`
- `GuildEventPlayerJoined` *(bounded string - player name)*
- `GuildEventPlayerLeft` *(bounded string - player names)*
- `GuildEventNewLeader` *(bounded string - 2 player names)*
- `GuildEventPresenceChange` *(bounded string - player name)*
- `GuildInviteDeclined` *(bounded string - player name)*

#### InstancePackets.cs (8)
- `UpdateInstanceOwnership`
- `UpdateLastInstance`
- `InstanceReset`
- `InstanceResetFailed`
- `ResetFailedNotify`
- `InstanceSaveCreated`
- `RaidGroupOnly`
- `RaidInstanceMessage`

#### ItemPackets.cs (13)
- `SetProficiency`
- `BuySucceeded`
- `BuyFailed`
- `SellResponse`
- `ReadItemResultFailed`
- `ReadItemResultOK`
- `SocketGemsSuccess`
- `DurabilityDamageDeath`
- `ItemCooldown`
- `ItemEnchantTimeUpdate`
- `EnchantmentLog`
- `ItemPushResult` *(uses ItemPacketHelpers for ItemInstance)*
- `InventoryChangeFailure` *(conditional fields based on BagResult)*

#### LootPackets.cs (8)
- `LootReleaseResponse`
- `LootMoneyNotify`
- `CoinRemoved`
- `LootRemoved`
- `LootRollsComplete`
- `MasterLootCandidateList` *(capped 40 raid members)*
- `LootList` *(optional fields)*
- `LootResponse` *(capped 16 items, 4 currencies)*

#### MailPackets.cs (2)
- `NotifyReceivedMail`
- `MailCommandResult`

#### MiscPackets.cs (22)
- `BindPointUpdate`
- `PlayerBound`
- `ServerTimeOffset`
- `TutorialFlags`
- `CorpseReclaimDelay`
- `TimeSyncRequest`
- `WeatherPkt`
- `StartLightningStorm`
- `LoginSetTimeSpeed`
- `AreaTriggerMessage`
- `DungeonDifficultySet`
- `InitialSetup`
- `CorpseLocation`
- `DeathReleaseLoc`
- `StandStateUpdate`
- `ExplorationExperience`
- `PlayMusic`
- `PlaySound` *(version-specific)*
- `PlayObjectSound` *(version-specific)*
- `TriggerCinematic`
- `StartMirrorTimer`
- `PauseMirrorTimer`
- `StopMirrorTimer`
- `ConquestFormulaConstants`
- `SeasonInfo`
- `InvalidatePlayer`
- `ZoneUnderAttack`

#### MovementPackets.cs (17)
- `MonsterMove` *(capped 64 waypoints, real usage: 1-2 points)*
- `MoveUpdate`
- `MoveTeleport` *(optional Vehicle + TransportGUID)*
- `TransferPending` *(optional Ship + TransferSpellID)*
- `TransferAborted`
- `NewWorld`
- `MoveSplineSetSpeed`
- `MoveSetSpeed`
- `MoveUpdateSpeed`
- `MoveSplineSetFlag`
- `MoveSetFlag`
- `MoveSetCollisionHeight`
- `MoveKnockBack`
- `MoveUpdateKnockBack`
- `SuspendToken`
- `ResumeToken`
- `ControlUpdate`

#### NPCPackets.cs (6)
- `GossipComplete` *(version-specific)*
- `BinderConfirm`
- `ShowBank`
- `TrainerBuyFailed`
- `RespecWipeConfirm`
- `SpiritHealerConfirm`

#### PetitionPackets.cs (3)
- `PetitionSignResults`
- `TurnInPetitionResult`
- `PetitionRenameGuildResponse` *(bounded string - guild name)*

#### PetPackets.cs (3)
- `PetClearSpells`
- `PetActionSound`
- `PetStableResult`

#### QueryPackets.cs (2)
- `QueryTimeResponse`
- `QueryPetNameResponse` *(bounded string - pet name + declined names)*

#### QuestPackets.cs (5)
- `QuestGiverQuestFailed`
- `QuestUpdateStatus`
- `QuestUpdateAddCredit`
- `QuestUpdateAddCreditSimple`
- `QuestPushResult`

#### ReputationPackets.cs (1)
- `SetFactionVisible`

#### SessionPackets.cs (1)
- `ConnectionStatus`

#### SpellPackets.cs (28)
- `CancelAutoRepeat`
- `SpellPrepare`
- `CastFailed`
- `PetCastFailed`
- `SpellFailure`
- `SpellFailedOther`
- `CooldownEvent`
- `ClearCooldown`
- `CooldownCheat`
- `SpellDelayed`
- `SpellChannelStart` *(optional InterruptImmunities + HealPrediction)*
- `SpellChannelUpdate`
- `SpellInstakillLog`
- `PlaySpellVisualKit`
- `TotemCreated`
- `ResurrectRequest` *(bounded string - player name)*
- `LearnedSpells` *(capped 8 spells)*
- `UnlearnedSpells` *(capped 8 spells)*
- `SendUnlearnSpells` *(capped 8 spells)*
- `SpellCooldownPkt` *(capped 64 cooldowns)*
- `SendSpellHistory` *(capped 64 entries)*
- `SendSpellCharges` *(capped 16 entries)*
- `SpellEnergizeLog` *(optional SpellCastLogData, capped 10 power entries)*
- `SpellDamageShield` *(optional SpellCastLogData, capped 10 power entries)*
- `EnvironmentalDamageLog` *(optional SpellCastLogData, capped 10 power entries)*
- `SpellHealLog` *(optional SpellCastLogData + ContentTuning, high-frequency)*
- `SpellNonMeleeDamageLog` *(optional SpellCastLogData + ContentTuning, high-frequency)*
- `SetSpellModifier` *(capped 8 modifiers × 8 data entries, high-frequency)*

#### ArenaPackets.cs (2)
- `ArenaTeamCommandResult` *(bounded string - team + player names)*
- `ArenaTeamInvite` *(bounded string - team + player names)*

#### TaxiPackets.cs (3)
- `TaxiNodeStatusPkt`
- `NewTaxiPath`
- `ActivateTaxiReplyPkt`

#### UpdatePackets.cs (1)
- `PowerUpdate` *(capped 16 power types)*

#### WorldStatePackets.cs (1)
- `UpdateWorldState`

</details>

### Unconverted Packets (49)

These packets cannot implement `ISpanWritable` due to dynamic lists or complex data structures that cannot determine `MaxSize` at compile time.

<details>
<summary>Dynamic Lists / Complex Data</summary>

| Packet | File | Blocking Field(s) |
|--------|------|-------------------|
| `AuctionListMyItemsResult` | AuctionPackets.cs | `List<AuctionItem>` |
| `AuctionListItemsResult` | AuctionPackets.cs | `List<AuctionItem>` |
| `PVPMatchStatisticsMessage` | BattleGroundPackets.cs | `List<PVPMatchPlayerStatistics>` |
| `EnumCharactersResult` | CharacterPackets.cs | `List<CharacterListEntry> Characters` |
| `GetAccountCharacterListResult` | CharacterPackets.cs | `List<AccountCharacterListEntry>` |
| `InspectResult` | CharacterPackets.cs | Multiple lists (talents, glyphs, items) |
| `AttackerStateUpdate` | CombatPackets.cs | Complex damage info with lists |
| `PartyUpdate` | GroupPackets.cs | `List<PartyPlayerInfo> PlayerList` |
| `PartyMemberPartialState` | GroupPackets.cs | Complex nested state |
| `PartyMemberFullState` | GroupPackets.cs | Complex nested state |
| `QueryGuildInfoResponse` | GuildPackets.cs | Multiple rank names |
| `GuildRoster` | GuildPackets.cs | `List<GuildRosterMemberData>` |
| `GuildRanks` | GuildPackets.cs | `List<GuildRankData>` |
| `GuildBankQueryResults` | GuildPackets.cs | `List<GuildBankItemInfo>` |
| `GuildBankLogQueryResults` | GuildPackets.cs | `List<GuildBankLogEntry>` |
| `DBReply` | HotfixPackets.cs | Dynamic data blob |
| `AvailableHotfixes` | HotfixPackets.cs | `List<HotfixRecord>` |
| `HotfixConnect` | HotfixPackets.cs | `List<HotfixRecord>` |
| `HotFixMessage` | HotfixPackets.cs | `List<HotfixData>` |
| `MailListResult` | MailPackets.cs | `List<MailListEntry>` |
| `GossipMessagePkt` | NPCPackets.cs | `List<GossipOption>`, `List<GossipQuest>` |
| `VendorInventory` | NPCPackets.cs | `List<VendorItem>` |
| `PetSpells` | PetPackets.cs | `List<uint> ActionButtons`, `List<PetSpellCooldown>` |
| `QueryQuestInfoResponse` | QueryPackets.cs | Complex quest data with lists |
| `QueryCreatureResponse` | QueryPackets.cs | Variable-length strings |
| `QueryGameObjectResponse` | QueryPackets.cs | Variable-length strings |
| `QueryPageTextResponse` | QueryPackets.cs | Variable-length text |
| `WhoResponsePkt` | QueryPackets.cs | `List<WhoEntry>` |
| `QuestGiverQuestDetails` | QuestPackets.cs | Multiple lists (rewards, objectives) |
| `QuestGiverQuestListMessage` | QuestPackets.cs | `List<GossipQuest>` |
| `QuestGiverRequestItems` | QuestPackets.cs | `List<QuestObjectiveCollect>` |
| `QuestGiverOfferRewardMessage` | QuestPackets.cs | Multiple reward lists |
| `QuestGiverQuestComplete` | QuestPackets.cs | Optional reward display |
| `DisplayToast` | QuestPackets.cs | Multiple switch-based writes |
| `AuraUpdate` | SpellPackets.cs | `List<AuraInfo> Auras` |
| `SpellStart` | SpellPackets.cs | Complex spell cast data |
| `SpellGo` | SpellPackets.cs | Complex spell cast data |
| `SpellPeriodicAuraLog` | SpellPackets.cs | `List<SpellLogEffect>` |
| `FeatureSystemStatus` | SystemPackets.cs | Complex with many optional fields |
| `FeatureSystemStatusGlueScreen` | SystemPackets.cs | Complex with version-dependent lists |
| `TradeUpdated` | TradePackets.cs | `List<TradeItem>` |
| `UpdateObject` | UpdatePackets.cs | Complex object update data |

</details>

<details>
<summary>Complex Authentication / Session Packets</summary>

| Packet | File | Reason |
|--------|------|--------|
| `AuthResponse` | AuthenticationPackets.cs | Complex with optional sections |
| `ConnectTo` | AuthenticationPackets.cs | Connection data with addresses |
| `EnterEncryptedMode` | AuthenticationPackets.cs | Encryption keys |
| `BattlenetNotification` | SessionPackets.cs | Bnet protocol data |
| `BattlenetResponse` | SessionPackets.cs | Bnet protocol data |
| `ChangeRealmTicketResponse` | SessionPackets.cs | Auth ticket data |

</details>

<details>
<summary>Unbounded Strings / Other</summary>

| Packet | File | Reason |
|--------|------|--------|
| `PartyInvite` | GroupPackets.cs | Multiple unbounded strings (realm names, etc.) |

</details>

### MaxSize Optimizations

Based on `spanstats.log` analysis, several packets were allocating far more memory than needed. These have been optimized with reduced caps that still use fallback to `Write()` for rare oversized cases.

| Packet | Old MaxSize | New MaxSize | Reduction |
|--------|-------------|-------------|-----------|
| `ContactList` | 36,605 | ~2,933 | **92%** |
| `UpdateAccountData` | 16,419 | ~2,083 | **87%** |
| `AllAccountCriteria` | 13,060 | ~1,636 | **87%** |
| `ChatPkt` | 8,511 | ~1,087 | **87%** |
| `SendKnownSpells` | 4,617 | ~1,097 | **76%** |
| `SetupCurrency` | 3,844 | ~484 | **87%** |
| `SetAllTaskProgress` | 3,332 | ~420 | **87%** |
| `LoadCUFProfiles` | 2,048 | 256 | **88%** |
| `MOTD` | 2,065 | ~517 | **75%** |
| `MonsterMove` | 1,134 | ~366 | **68%** |
| `LFGListUpdateBlacklist` | 1,028 | ~132 | **87%** |
| `ChannelNotifyJoined` | 357 | ~165 | **54%** |
| `MoveUpdate` | 256 | 192 | **25%** |

## Acknowledgements

Parts of this project's code are based on [CypherCore](https://github.com/CypherCore/CypherCore) and [BotFarm](https://github.com/jackpoz/BotFarm). I would like to extend my sincere thanks to these projects, as the creation of this app might have never happened without them. And I would also like to expressly thank [Modox](https://github.com/mdx7) for all his work on reverse engineering the classic clients and all the help he has personally given me.

## Download HermesProxy
Stable Downloads: [Releases](https://github.com/Xian55/HermesProxy/releases)
