# Logging

HermesProxy ships with a Serilog-backed logger that splits **severity** (single letter: `V`, `D`, `I`, `W`, `E`, `F`) from **category** (single letter: `S` = Server, `N` = Network, `T` = Storage, `P` = Packet) and colors property values in the console output (strings in cyan, numbers in yellow) so the interesting bits stand out at a glance.

```
17:00:33 | I | S | Server          | Starting WorldSocket service on 127.0.0.1:8086...
17:00:34 | I | S | BnetTcpSession  | Accepting connection from 127.0.0.1:49695.
17:09:19 | W | P | WorldSocket     | C>P S | No handler for opcode CMSG_BATTLE_PAY_GET_PURCHASE_LIST (14019) (Got unknown packet from ModernClient)
```

Each category has its own minimum level so you can dial verbosity in independently — the console has an additional floor applied on top, which lets you keep the console tidy while the file captures enough detail for a bug report.

## Reading a Log Line

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

In the log file (`Logs/hermes-<yyyyMMdd_HHmmss>.log`) the same layout is used but without ANSI codes, so it stays grep-friendly and safe to paste into a bug report.

## Logging Settings

| Setting                       | Default       | Description                                                                                                          |
|-------------------------------|---------------|----------------------------------------------------------------------------------------------------------------------|
| `Log.MinimumLevel`            | `Information` | Absolute floor across all sinks. Events below this level are dropped before categories/sinks are checked.            |
| `Log.Server.MinimumLevel`     | `Information` | Category override for `Server` (lifecycle, config, startup). Rendered as `S`.                                        |
| `Log.Network.MinimumLevel`    | `Information` | Category override for `Network` (auth/world socket connect, handshake). Rendered as `N`.                             |
| `Log.Storage.MinimumLevel`    | `Information` | Category override for `Storage` (GameData, CSV/DB2, hotfixes). Rendered as `T`.                                      |
| `Log.Packet.MinimumLevel`     | `Warning`     | Category override for `Packet` (opcode dispatch, span fallback/stats). Rendered as `P`. `Verbose` enables SpanStats. |
| `Log.Console.MinimumLevel`    | `Information` | Additional floor applied ONLY to the console. File sink captures everything the categories allow.                    |
| `Log.ToFile`                  | `true`        | Write one log file per run, `Log.Directory/hermes-<yyyyMMdd_HHmmss>.log`, sharing its stamp with that run's captures. At 256 MB it rolls to `hermes-<stamp>_001.log` and so on, and a run keeps its newest 4 files, so the very start of a long run ages out. |
| `Log.Directory`               | `Logs`        | Directory (relative to the proxy's own directory) where log files are placed.                                        |
| `Log.RetentionDays`           | `30`          | Days to keep log files. Older ones are deleted at startup and every hour. `0` keeps them forever.                    |
| `PacketsLog`                  | `true`        | Dump each session's raw packets to a `.pkt` file in `PacketsLog/` for replay / inspection (unrelated to text logs).  |
| `DebugOutput` *(legacy)*      | `false`       | Back-compat shortcut — lowers `Log.MinimumLevel` and `Log.Console.MinimumLevel` to `Debug`.                          |
| `SpanStatsLog` *(legacy)*     | `false`       | Back-compat shortcut — lowers `Log.Packet.MinimumLevel` to `Verbose`.                                                |

Valid level values (case-insensitive): `Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal`.

## CLI Examples

All logging settings can be overridden at runtime with `--set key=value` (repeat the flag for multiple overrides).

```bash
# Capture packet-level debug detail in the log file while keeping the console quiet.
# Ideal default for "always-on" logging — when a user reports an issue, ask for
# that run's Logs/hermes-*.log and you already have the Debug-level context.
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

## Opt-In Movement Trace (`HERMES_TRACE_MOVEMENT`)

When triaging mob-rendering or facing/spline glitches (e.g. issue #74-style "mob invisible / falls through terrain / runs sideways"), set the environment variable `HERMES_TRACE_MOVEMENT` to any non-empty value before launching HermesProxy. The proxy will then emit per-packet `[MonsterMove/In   ]`, `[MonsterMove/Write]`, `[MonsterMove/Span ]`, and `[CreateObj-Move ]` lines into `Logs/hermes-<date>.log`, tagged with the modern `ExpansionVersion` so logs from different client platforms (macOS / Windows / V1_14 / V2_5 / V3_4_3) can be compared side-by-side.

```bash
# Linux / macOS
HERMES_TRACE_MOVEMENT=1 dotnet run --project HermesProxy

# Windows PowerShell
$env:HERMES_TRACE_MOVEMENT='1'; dotnet run --project HermesProxy
```

The trace lines are written at `Information` level into the `Server` category, so no extra `Log.*.MinimumLevel` knob is required. Default is off; the gate is a single `static readonly bool` checked at startup, so zero overhead when not enabled.
