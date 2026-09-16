# HermesProxy

WoW protocol translation proxy — allows modern retail clients to connect to legacy server emulators by translating between protocol versions.

## Solution Structure

| Project | Purpose |
|---|---|
| `Framework` | Shared library: networking, cryptography, packet I/O, protobuf, utilities |
| `HermesProxy` | Main proxy executable (console app) |
| `HermesProxy.Tests` | xUnit test suite |
| `HermesProxy.Benchmarks` | BenchmarkDotNet performance benchmarks |

## Build & Run

```bash
dotnet build                                    # Build all projects
dotnet run --project HermesProxy                # Run the proxy
dotnet test                                     # Run all tests
dotnet run --project HermesProxy.Benchmarks -c Release -- --filter "*Name*"  # Run benchmarks
```

## Target Framework & Global Settings

- **.NET 10.0** — set centrally in `Directory.Packages.props`
- **Central package management** — all versions in `Directory.Packages.props`; projects use `<PackageReference>` without version attributes
- **Nullable** enabled solution-wide
- **Global using**: `System.Numerics`

## Code Style

- **PascalCase** for types, methods, properties, public fields
- **_camelCase** for private fields (leading underscore)
- **File-scoped namespaces** in newer code (`namespace Foo;`)
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
