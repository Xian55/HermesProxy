# HermesProxy

WoW protocol translation proxy — allows modern retail clients to connect to legacy server emulators by translating between protocol versions.

## Solution Structure

| Project | Purpose |
|---|---|
| `Framework` | Shared library: networking, cryptography, packet I/O, protobuf, utilities |
| `HermesProxy` | Main proxy executable (console app) |
| `HermesProxy.Tests` | xUnit test suite |
| `HermesProxy.Benchmarks` | BenchmarkDotNet performance benchmarks |
| `HermesProxy.Analyzers` | Roslyn rules that fail the build, for `Framework` and `HermesProxy`. No generators: those emit into whatever loads them |

## Build & Run

```bash
dotnet build                                    # Build all projects
dotnet run --project HermesProxy                # Run the proxy
dotnet test                                     # Run all tests
dotnet run --project HermesProxy.Benchmarks -c Release -- --filter "*Name*"  # Run benchmarks
```

## Dev Loop: Hot Reload

Packet work runs the proxy under `dotnet watch` and patches it while the client stays connected.
Rebuild, restart and log in again is what an edit falls back to, not the loop.

```bash
pwsh .claude/skills/hermes-run/scripts/run.ps1 -Mode watch -Profile <name>   # /hermes-run watch
```

Save a file and the running proxy changes, usually within 2 s (about 5 for the first patch of a
session). Then repeat the action in game.

| Edit | What happens |
|---|---|
| A method body: handler, codec `Read`, layout `Write`, anything in `Framework` | patched in |
| A `[LoggerMessage]` text | patched in |
| A new log method | patched in, but its first call has thrown `NullReferenceException`: add it with a restart (Ctrl+R) |
| A `[Descriptor*]` attribute on a `*Field` enum | generator reruns, the writer is patched in |
| A new `[HandlesCmsg]` / `[HandlesSmsg]`, method, class, or field on a class | patched in |
| A new enum member (`ENC0021`) | proxy restarts |
| A method that contains `stackalloc` (`ENC0044`) | proxy restarts |
| A new field on a `record struct` packet (`ENC0030`) | proxy restarts |
| A static field's initializer: a packet's layout list, a lookup table (`ENC0118`) | proxy restarts |

- **The table says what restarts, not what to avoid.** `stackalloc`, `record struct` packets and
  `static readonly` tables are there for the path that ships. Never choose or rewrite a design so
  that an edit becomes patchable: take the restart.
- **Read what the edit came to** in `HermesProxy/bin/Debug/Logs/dotnet-watch.log`. Its last lines
  say `changes applied`, or `error CS…` (nothing was applied and the proxy still runs the code from
  before), or name the `ENC…` rule that restarted it.
- **The hermes log dates each patch:** `Hot reload #N patched <types>`. Packets logged above that
  line ran the old code. Check it before reading a log as evidence for a fix.
- **A restart drops the client**, which then has to log in again. Make restarting edits together,
  and say so first when somebody is in the world.
- **A patch replaces code, not state.** What an earlier packet stored in the session stays as the
  old code wrote it, and `GameData` stays as loaded. To see a fix to stored state, send the packet
  again or restart (Ctrl+R in the watch window).
- **A watch session is a Debug build**: logs, `PacketsLog` and `AccountData` are under `bin/Debug`.
  No performance figure comes from one; measure in Release.
- The dispatch tables are built once, by a static initializer no runtime runs twice.
  `HotReloadHandler` builds them again after every patch through the generated `Rebuild()`, which
  is what makes a newly attributed handler reachable. Anything else that has to follow a patch
  goes there.

## Target Framework & Global Settings

- **.NET 10.0** — set centrally in `Directory.Packages.props`
- **Central package management** — all versions in `Directory.Packages.props`; projects use `<PackageReference>` without version attributes
- **Nullable** enabled solution-wide
- **Global using**: `System.Numerics`

## Code Style

- **PascalCase** for types, methods, properties, public fields
- **_camelCase** for private fields (leading underscore)
- **File-scoped namespaces** in newer code (`namespace Foo;`)
- **`sealed` by default** — every class is `sealed` unless it is meant to be derived from (an abstraction). Server packet layouts are enforced by `HPSG009`; for the rest it is the rule for new and touched code
- **CypherCore GPL v3 headers** on legacy/ported files — preserve these when editing
- Prefer `var` when the type is obvious from context

## Performance Philosophy

- Zero-allocation hot paths — avoid allocations in packet processing loops
- `Span<T>` / `ref struct` for packet I/O (`SpanPacketReader`, `SpanPacketWriter`)
- `ArrayPool<byte>` for temporary buffers
- `FrozenDictionary` / `FrozenSet` for static lookup tables
- `[MethodImpl(MethodImplOptions.AggressiveInlining)]` on hot-path methods

## Logging

**Use source-generated `[LoggerMessage]`. This applies to temporary debug logging too — no exceptions.**

`Log.Print(LogType type, object text)` takes an already-built string. The `IsEnabled` check happens *inside*, so an interpolated call pays its full cost even when the level is disabled:

```csharp
// WRONG — string is built, GUID is ToString()'d, and the helper loops all run
// before Log.Print gets a chance to discard it.
Log.Print(LogType.Trace, $"[Trace] guid={guid} anyField={ScanAllSlots()} hp={u?.Health}");

// RIGHT — generated method checks IsEnabled before formatting.
UpdateHandlerLogMessages.ValuesTrace(_log, guid, hp);
```

- Add methods to the subsystem's `*LogMessages.cs` partial class (`World/Logging/`, `Auth/Logging/`, …). EventId ranges are reserved per subsystem — check the file header before picking one.
- **`[LoggerMessage]` does not stop argument evaluation.** Anything expensive passed as an argument — a loop, a dictionary lookup, `ToString()` on a GUID or record — still runs at every call. Wrap those in an explicit `if (logger.IsEnabled(LogLevel.Trace))` block.
- Never `ToString()` a `WowGuid128`/record in a log argument; the compiler-generated `ToString()` allocates.
- `LogType.Server`/`Network`/`Storage` route to **Information** — enabled in production. `Trace` → Verbose, `Debug` → Debug.

Temporary traces are how this rule gets broken: they outlive the bug they were added for, and an ungated one in a per-packet path costs on every packet forever. Write it the fast way the first time, or delete it before committing.

## Key Architecture

```
Modern Client <--BNet/TCP--> BNetServer  ──┐
                                           ├── HermesProxy ──> AuthClient ──> Legacy Emulator
Modern Client <---TCP-----> WorldServer ──┘                   WorldClient ──> Legacy Emulator
```

- **BNetServer** — accepts modern client Battle.net connections (TLS, protobuf)
- **AuthClient** — connects to legacy emulator auth/login server
- **WorldServer** — accepts modern client game connections
- **WorldClient** — connects to legacy emulator world server
- Packets are translated bidirectionally between modern and legacy opcodes
- **Delaying or reordering a packet** goes through the session's outboxes, `ctx.ToClient` /
  `ctx.ToServer`. Never add a new pending queue or a `Thread.Sleep`. See
  [HermesProxy/World/Outbox/CLAUDE.md](HermesProxy/World/Outbox/CLAUDE.md).
- **Threading:** handlers, timer callbacks and teardown are posted to the session's
  `SessionExecutor`, which runs one of them at a time — inline on the posting thread when the session
  is free. See [HermesProxy/World/Session/CLAUDE.md](HermesProxy/World/Session/CLAUDE.md) for what is
  posted and what deliberately isn't (the legacy handshake and the connection-level opcodes).
  Cross-socket send order still comes from synchronous writes on the calling thread. Don't introduce
  a per-socket send queue (see the outbox handbook for the reverted attempt).
- **Work that isn't awaited** goes through `ObserveFaults`. `HPA001` fails the build on a dropped
  `Task`, `_ =` included, and `HPA002` on `async void`. A dropped task's exception stays hidden until
  a GC finalizes it, which may be never, and a loop that dies that way dies without a word (#540).
  A long-lived loop needs an owner instead: `AsyncAcceptor` keeps its task, handles every failure
  inside it, and awaits it on the way out.
