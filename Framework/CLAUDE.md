# Framework

Shared library providing core networking, cryptography, packet I/O, protocol buffers, and utilities used by all other projects. See root [CLAUDE.md](../CLAUDE.md) for solution-wide conventions.

## Key Patterns

- **`SpanPacketReader` / `SpanPacketWriter`** — `ref struct`, zero-allocation packet serialization over `Span<byte>`
- **`ByteBuffer`** — pooled legacy-compatible read/write buffer (uses `ArrayPool<byte>`)
- **`SocketBase`** — abstract TCP networking base class. `BeginReceive` starts one loop per
  connection, which owns its pooled buffer and hands `ReadHandler` one read at a time. The bytes are
  only valid during that call, and a handler doesn't read: the next read is the loop's. A handler
  that throws closes that connection and no other, and however the loop ends, `CloseSocket` runs and
  `OnClose` runs once. `AsyncWrite` is a blocking send, bounded by a 30 s `SendTimeout` — a peer that
  stops reading would otherwise pin the sending thread. A timed-out send has lost the frame boundary,
  so the socket is closed rather than retried. Every socket also gets TCP keep-alive
  (`NetworkUtils.EnableKeepAlive`), so a peer whose machine disappears surfaces within ~90 s instead
  of after TCP's own retransmit limit.
- **`SSLSocket`** — the same contract for the BNet and login connections, with TLS optional: `AsyncRead`
  is the loop and awaits each `ReadHandler`, so a handler still working holds the next read back.
  Writes queue on a semaphore because `SslStream` allows one at a time.
- **`AsyncAcceptor`** — one listening socket, one accept loop, from `BeginAccept` to `DisposeAsync`.
  The loop never faults: a connection lost in the backlog is retried at once, any other socket error
  after a delay, and a handler that throws costs that one connection. `DisposeAsync` cancels, waits
  for the loop, then closes the listener, so stopping has one outcome on every platform and no
  handler runs afterwards. A failure in `OnSocketOpen` belongs to the loop: don't catch it there.
- **`ConnectionSweeper`** — the accepted connections of one listener. One `PeriodicTimer` task every
  5 s forgets the dead ones, and stopping closes the rest. A closed connection stays listed until the
  next tick, which is harmless as no read or write goes through here; don't hang work on it.
- **`TaskObservation.ObserveFaults`** — how a `Task` gets started without being awaited. Costs one
  check when the task already succeeded, one continuation when it is still running.

## Directories

| Directory | Contents |
|---|---|
| `Constants/` | Enums, opcodes, protocol constants |
| `Cryptography/` | SRP6, ARC4, HMAC, packet encryption/decryption |
| `GameMath/` | Vectors, quaternions, game-specific math |
| `IO/` | `ByteBuffer`, `SpanPacketReader`, `SpanPacketWriter`, packet base types |
| `Logging/` | Logging infrastructure |
| `Metrics/` | Performance metrics collection |
| `Networking/` | `SocketBase`, async socket I/O |
| `Proto/` | Battle.net protocol `.proto` schema; Grpc.Tools runs protoc on build, C# lands in `obj/` (messages only, `GrpcServices="None"`) |
| `Serialization/` | Serialization helpers |
| `Util/` | Extension methods, helpers |
| `Web/` | HTTP/web utilities |

## Project Config

- **Unsafe code allowed** (`AllowUnsafeBlocks: true`) for performance-critical paths
- **InternalsVisibleTo**: `HermesProxy.Tests`, `HermesProxy.Benchmarks`
- **Dependencies**: Google.Protobuf, System.Configuration.ConfigurationManager; Grpc.Tools (build-time only — bundled protoc must not be newer than the Google.Protobuf runtime)

## Conventions

- `[MethodImpl(MethodImplOptions.AggressiveInlining)]` on hot-path methods
- Prefer `Span<T>` over `byte[]` for packet data
- Use `ArrayPool<byte>.Shared` for temporary buffers — always return rentals
- Minimize allocations in networking and I/O code paths
