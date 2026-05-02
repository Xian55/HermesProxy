---
name: hermes-logs
description: Read and analyze HermesProxy runtime logs (hermes-*.log) — errors, opcodes, directions, session summary
user-invocable: true
---

# HermesProxy Logs

Analyse HermesProxy runtime logs (`hermes-*.log`) without dumping a multi-MB file into context. Resolve the right session, extract the slice you need (errors, opcodes, directions, summary), and link companion artifacts (screenshot PNG, `.pkt` capture).

## Usage

```
/hermes-logs                       # summary of latest session
/hermes-logs <subcommand> [args]
/hermes-logs <bare-arg>            # auto-classified — see table below
```

### Argument classifier (priority order)

When the first arg isn't a known subcommand, classify it:

| Pattern | Dispatched as |
|---|---|
| `^(C?MSG\|SMSG)_[A-Z0-9_]+$` or pure digits | `opcode <arg>` |
| `c2p` / `p2s` / `p2c` / `c2s` | `direction <arg>` |
| `^\d{2}:\d{2}:\d{2}$` | `since <arg>` |
| First arg is `client` | `client <subcommand>` (see "WoW client logs" section) |
| Existing file path or `hermes-*` basename | `summary <arg>` |
| Empty / unrecognized | `summary` (latest) |

So `/hermes-logs CMSG_AUTH_SESSION` works without typing `opcode`.

## Log format

Single unified `.log` per session at:
- `HermesProxy/bin/Release/Logs/hermes-YYYYMMDD[_HHmmss][_label].log`
- `HermesProxy/bin/Debug/Logs/hermes-YYYYMMDD[_HHmmss][_label].log`

Each line:
```
HH:mm:ss | LEVEL | CAT | SourceFile           | [NetDir |] Message
```

| Column | Codes | Notes |
|---|---|---|
| LEVEL | `D` Debug, `I` Info, `W` Warn, `E` Error, `V` Verbose | single char |
| CAT | `S` Server, `N` Network, `T` Storage/Data, `P` Packet | single char |
| NetDir (packet lines only) | `C>P S`, `C P>S`, `C P<S`, `C<P S` | see below |
| SourceFile | left-padded to ≥15, **not truncated** (e.g. `ProxyHostedService` is 18) | split on `\|`, never fixed-width slice |

**NetDir meanings** (modern client ↔ proxy ↔ legacy server):
- `C>P S` — modern client → proxy (received from client side)
- `C<P S` — proxy → modern client
- `C P>S` — proxy → legacy server
- `C P<S` — proxy ← legacy server

**Tested regexes** (use these directly with Grep):
- Whole line: `^(\d{2}:\d{2}:\d{2}) \| ([DIWEV]) \| ([SNTP]) \| (.{1,16}?)\s*\|(?: (C[<>]P [<>]?S|C P[<>]S) \|)? (.*)$`
- Packet lines only: `^\d{2}:\d{2}:\d{2} \| . \| P \|`
- Errors + warnings: `^\d{2}:\d{2}:\d{2} \| [WE] \|`
- Header (first ~20 lines): match `Modern \(Client\) Build|Legacy \(Server\) Build|Loaded \d+ .* opcodes|External IP|Starting .* service`

Multi-line stack traces have **no leading `HH:mm:ss`** — use Grep with `-A 3` (or higher) when matching errors so continuations aren't lost.

## Subcommands

For each subcommand below: if `[file]` is omitted, default to the latest log via `scripts/resolve.ps1 -Mode latest -Count 1`. All paths shown are relative to the repository root — invoke from there.

### `latest [N=1]`
Print path(s) of the N newest logs.
```
pwsh .claude\skills\hermes-logs\scripts\resolve.ps1 -Mode latest -Count <N>
```

### `errors [file]`
Extract Warning + Error lines with stack-trace context.
```
Grep -n -C 0 -A 3 -P "^\d{2}:\d{2}:\d{2} \| [WE] \|"  <file>
```

### `opcode <name|id> [file]`
Find packet lines mentioning an opcode by name or numeric ID.
- For names (e.g. `CMSG_AUTH_SESSION`): Grep pattern `"\b<NAME>\b"` on packet lines.
- For numeric IDs (e.g. `14181`): Grep pattern `\(<ID>\)` on packet lines.
```
Grep -n -P "^\d{2}:\d{2}:\d{2} \| . \| P \|.*\"<NAME>\"" <file>
Grep -n -P "^\d{2}:\d{2}:\d{2} \| . \| P \|.*\(<ID>\)"  <file>
```

### `direction <c2p|p2s|p2c|c2s> [file]`
Filter packet lines by net direction.

| Arg | NetDir literal |
|---|---|
| `c2p` | `C>P S` (client → proxy) |
| `p2c` | `C<P S` (proxy → client) |
| `p2s` | `C P>S` (proxy → legacy server) |
| `s2p` / `c2s` | `C P<S` (legacy server → proxy) |

```
Grep -n -F "<NetDir literal>" <file>
```

### `summary [file]`
Full session summary (header, counts, top opcodes, unhandled, disconnects, errors).
```
pwsh .claude\skills\hermes-logs\scripts\summarize.ps1 -Path <file>
```

### `tail [N=200] [file]`
Last N lines of the log (for "what just happened").
```
Read <file> with offset = totalLines - N
```
Get total line count via Bash `wc -l <file>` if unknown.

### `since <HH:mm:ss> [file]`
All lines at or after a given timestamp.
```
Grep -n -P "^(<TS>|<TS+1>|...)" <file>     # naïve: anchor on prefix
# Better:
Grep -n -P "^\d{2}:\d{2}:\d{2}" -A 1000000 <file>   # then trim in-memory by timestamp ≥ <TS>
```
Simpler: read whole file via `summarize.ps1 -Since <TS>` if added later; otherwise prefer Read with offset after locating the first matching line via Grep.

### `disconnects [file]`
Session lifecycle markers — connection accept, TLS failures, disconnects, exceptions.
```
Grep -n -P "Accepting connection|TLS handshake failed|Disconnecting|Closed connection|Unhandled exception|^\s+at " <file>
```

### `screenshot [file]`
Resolve the companion PNG (`screenlogs/<basename>.png`, exact basename match) and Read it.
```
pwsh .claude\skills\hermes-logs\scripts\resolve.ps1 -Mode screenshot -LogPath <file>
# Then: Read the printed path as an image.
```

### `pkt [file]`
Find the matching `modern_*.pkt` and `legacy_*.pkt` for the session.

`.pkt` filenames embed the same `yyyyMMdd_HHmmss` session token as the log filename:
```
hermes-20260502_042648.log
modern_54261_20260502_042648_2.pkt
legacy_12340_20260502_042648_1.pkt
```
The resolver does an **exact-token match** first (no proximity, no false neighbours). Older `.pkt` files written before this format change (filename pattern `{stream}_{build}_{unixtime}_{seq}.pkt`) are matched by ±10 min unix-time proximity as a fallback.

```
pwsh .claude\skills\hermes-logs\scripts\resolve.ps1 -Mode pkt -LogPath <file>
```
This skill **does not decode `.pkt`** — hand the resulting path to the `parse-pkt` skill, which runs WowPacketParser and produces a readable `_parsed.txt`.

## Default file resolution

When `[file]` is omitted, always run:
```
pwsh .claude\skills\hermes-logs\scripts\resolve.ps1 -Mode latest -Count 1
```
This picks the newest `hermes-*.log` across **both** `bin/Release/Logs/` and `bin/Debug/Logs/` by `LastWriteTime`. Don't hardcode Release.

## Pitfalls

- **TLS-aborted sessions** — if a log is ≤20 lines and ends in a `W`/`E` line, the session died at handshake. `summary` detects this and prints `**Session aborted at handshake.**` instead of full stats.
- **Stack traces span lines** — continuation lines have no `HH:mm:ss` prefix. Use Grep `-A 3` (or higher) when matching errors.
- **Active-write file lock** — if HermesProxy is still running and writing to the log, scripts open with `FileShare.ReadWrite`. Don't use plain `Get-Content` in a loop without `-ReadCount 0` (it can race).
- **Variable source-file column width** — split on `\|` (with surrounding spaces); never fixed-width slice.
- **`.pkt` basenames diverge from log basenames** — `.pkt` filename pattern is `{modern|legacy}_BUILD_UNIXTIME_SEQ.pkt`. Use `resolve.ps1 -Mode pkt`, don't try basename equality.
- **Two filename styles** — older logs are `hermes-YYYYMMDD.log` (date-only), newer are `hermes-YYYYMMDD_HHmmss.log`. Both are `hermes-*.log`. Optional `_label` suffix (`hermes-20260501_new_char_create.log`) is preserved.
- **Color codes** — file logs are plain text, no ANSI. Console logs (when running interactively) have ANSI; that's not what's on disk.

## WoW client logs (`client` subcommands) — optional

The proxy isn't the only thing that logs — the WoW client does too, and its rolling logs + crash dumps are useful when triaging client-side crashes (ERROR #132, "jam mirror full update failure" — the V3_4_3 player-Values block signature, etc.).

**These subcommands are optional.** All proxy-side subcommands above (`latest`, `errors`, `opcode`, `summary`, etc.) work without any client setup. Only the `client *` subcommands need access to the WoW install.

**Optional setup:** to enable `client *` subcommands, set the env var `HERMES_TOOLS_CLIENT_WOTLK` to your WoW 3.4.3 client root (the folder containing `Logs\` and `Errors\` — typically a `_classic_` folder under your WoW install). Persist with:

```powershell
[Environment]::SetEnvironmentVariable('HERMES_TOOLS_CLIENT_WOTLK', '<path>', 'User')
```

If the var is unset when a `client *` subcommand is invoked, the resolver exits with a "not configured" message and Claude should pass it through to the user instead of guessing a path. Nothing about the path is ever checked in.

### `client list`
Show what's available — list of `Logs\*.log` files and the newest crashes/errors in `Errors\`.
```
pwsh .claude\skills\hermes-logs\scripts\resolve.ps1 -Mode client-root
# Then list <root>\Logs and <root>\Errors via Bash/Glob.
```

### `client log <name> [tail N=200]`
Read a specific log under `<root>\Logs\`. `<name>` accepts bare name (`Client`) or with extension (`Client.log`).

Common logs:
- `Client` — main client log (login flow, hotfixes, loading screens, **`jam mirror full update failure`**)
- `Hotfix` — hotfix sync (large; useful when char-list / spell data goes wrong)
- `Connection` / `WowConnection` — networking
- `Sound`, `gx`, `Movie` — subsystems

```
$path = pwsh .claude\skills\hermes-logs\scripts\resolve.ps1 -Mode client-log -Name <name>
# Then Read $path with offset = totalLines - N (use `wc -l` for total).
```

### `client crash [N=1]`
Latest N crash report `.txt` files with parsed summary (header, ERROR code, exception type, stack frames, key tags, thread count, companion `.dmp` path).
```
$crash = pwsh .claude\skills\hermes-logs\scripts\resolve.ps1 -Mode client-crashes -Count <N>
pwsh .claude\skills\hermes-logs\scripts\summarize-crash.ps1 -Path $crash
```

Crash filename pattern: `YYYY-MM-DD HH.mm.ss <Error|Crash> - <PID>.txt` (with companion `.dmp`).

### `client jam`
Extract `jam mirror full update failure` lines from `Client.log` — the V3_4_3 client's signature when it can't reconstruct a Values-block update from a server delta. These almost always indicate a HermesProxy descriptor-write bug for the build the user is running.
```
$path = pwsh .claude\skills\hermes-logs\scripts\resolve.ps1 -Mode client-log -Name Client
Grep -n -P "jam mirror full update failure|^.*span dump " <path>
```
Each failure has two consecutive lines: the failure summary (`expected size N received N`) and the raw payload `span dump <hex>`. The hex dump shows the exact bytes the client rejected — pair this with the matching `hermes-*.log` opcode trace to nail down which Values write is wrong.

### `client errors-during <hermes-log>`
*(Manual recipe.)* Cross-reference: take a `hermes-*.log` (which has wall-clock `HH:mm:ss` from session start in its filename and inside) and find any `Errors\*.txt` whose timestamp falls inside that window. Useful when chasing a crash that aborted a captured session.
```
# Pseudocode — Claude does the timestamp arithmetic:
# 1. Parse log basename: hermes-YYYYMMDD_HHMMSS.log  → session start
# 2. Read first/last timestamp from log body for span
# 3. Glob <root>\Errors\YYYY-MM-DD HH.mm.ss *.txt and keep those whose stamp falls in span
```

## Future: ingesting user-reported GitHub issues

This skill is also intended as the entry point when a user-reported GitHub issue ships log excerpts (or a full `hermes-*.log` attachment). The flow then is:

1. Pull the issue with `gh issue view <number>` — extract any inline log block or attachment URL.
2. If an attachment, download it into `HermesProxy/bin/Release/Logs/` so the existing resolution logic finds it.
3. Run `summary` on the file to get a triage view (builds, errors, unhandled opcodes, disconnect events) without dumping raw log into chat.
4. Cross-reference the modern/legacy build values from the header against the user's reported environment.
5. If the issue mentions an opcode or feature, dispatch `opcode <NAME>` or `direction <dir>` to extract just the relevant slice for the bug report.

When that flow is implemented end-to-end, it will likely live as a sibling subcommand here (e.g. `/hermes-logs issue <number>`), driven by a small wrapper script that uses the `gh` CLI and then delegates back to `summarize.ps1`.
