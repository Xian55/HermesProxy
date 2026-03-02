# Framework

Shared library providing core networking, cryptography, packet I/O, protocol buffers, and utilities used by all other projects. See root [CLAUDE.md](../CLAUDE.md) for solution-wide conventions.

## Key Patterns

- **`SpanPacketReader` / `SpanPacketWriter`** — `ref struct`, zero-allocation packet serialization over `Span<byte>`
- **`ByteBuffer`** — pooled legacy-compatible read/write buffer (uses `ArrayPool<byte>`)
- **`SocketBase`** — abstract async TCP networking base class

## Directories

| Directory | Contents |
|---|---|
| `Constants/` | Enums, opcodes, protocol constants |
| `Cryptography/` | SRP6, ARC4, HMAC, packet encryption/decryption |
| `Debugging/` | Debug utilities |
| `GameMath/` | Vectors, quaternions, game-specific math |
| `IO/` | `ByteBuffer`, `SpanPacketReader`, `SpanPacketWriter`, packet base types |
| `Logging/` | Logging infrastructure |
| `Metrics/` | Performance metrics collection |
| `Networking/` | `SocketBase`, async socket I/O |
| `Proto/` | Protobuf-generated classes (Battle.net protocol) |
| `Serialization/` | Serialization helpers |
| `Singleton/` | Singleton pattern base |
| `Util/` | Extension methods, helpers |
| `Web/` | HTTP/web utilities |

## Project Config

- **Unsafe code allowed** (`AllowUnsafeBlocks: true`) for performance-critical paths
- **InternalsVisibleTo**: `HermesProxy.Tests`, `HermesProxy.Benchmarks`
- **Dependencies**: Google.Protobuf, System.Configuration.ConfigurationManager

## Conventions

- `[MethodImpl(MethodImplOptions.AggressiveInlining)]` on hot-path methods
- Prefer `Span<T>` over `byte[]` for packet data
- Use `ArrayPool<byte>.Shared` for temporary buffers — always return rentals
- Minimize allocations in networking and I/O code paths
