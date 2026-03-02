# HermesProxy (Main Application)

Console application — the actual proxy executable that translates between modern and legacy WoW protocols. See root [CLAUDE.md](../CLAUDE.md) for solution-wide conventions.

## Build & Run

```bash
dotnet run --project HermesProxy
dotnet run --project HermesProxy -- --config path/to/config
dotnet run --project HermesProxy -- --set ServerAddress=logon.example.com --no-version-check
dotnet run --project HermesProxy -- --metrics
```

## Entry Point

`Program.cs` → parses CLI args via `System.CommandLine` → calls `Server.ServerMain()`

### CLI Options

| Flag | Description |
|---|---|
| `--config <path>` | Config file path (default: `HermesProxy.config`) |
| `--set Key=Value` | Override config values (repeatable) |
| `--no-version-check` | Skip update check on startup |
| `--metrics` | Enable per-opcode latency metrics |

## Directories

| Directory | Contents |
|---|---|
| `Auth/` | Legacy authentication client (SRP6-based login) |
| `BnetServer/` | Modern Battle.net server (TLS, protobuf RPC) |
| `Configuration/` | Config file parsing and settings |
| `CSV/` | Static data files (copied to output at build) |
| `CSV/Hotfix/` | Hotfix data files (copied to output at build) |
| `Realm/` | Realm list management |
| `World/` | Packet handling — the largest area of the codebase |

## Packet Handling (World/)

- `World/Server/Packets/` — modern packet structure definitions (sent to/from retail client)
- `World/Server/PacketHandlers/` — handler logic that translates between protocol versions
- `World/Client/` — legacy packet structures and handlers (communication with emulator)

## Embedded Resources & Static Data

- `BNetServer.pfx` — TLS certificate for BNet server
- `*.CSV.HotfixDb.*.bin` — hotfix database binaries
- `CSV/` and `CSV/Hotfix/` — CSV data files copied to output directory

## Dependencies

- **Framework** (project reference)
- **Sep** — high-performance CSV parsing
- **System.CommandLine** — CLI argument parsing
- **GitVersion.MsBuild** — automatic semantic versioning
