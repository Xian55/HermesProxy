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
