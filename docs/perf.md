# Performance Optimizations

HermesProxy has been extensively optimized to minimize latency and memory allocations in packet handling hot paths.

## Baseline 2026-09 (pre-refactor)

Dated reference point for the packet-path redesign (struct packets, per-session loop,
zero-copy send). Raw tables and the full method are in
[perf-baseline-2026-09.md](perf-baseline-2026-09.md). Branch `feature/wotlk-classic-v3.4.3`
at 6ada57bf plus the measurement commit.

### How it was measured

- `--metrics` now records, per opcode and direction, handler latency **and**
  `GC.GetAllocatedBytesForCurrentThread()` delta per packet, plus a GC delta line every 60 s.
- Micro-benchmarks: `PacketDispatchBenchmarks`, `SendPipelineBenchmarks`,
  `PackedGuidBenchmarks`, run on the Windows dev box (i7-6700K) and on a quiet Mac mini M4.
- Live: `test-loop2.ps1 -AcoreBots -EnterWorld -Metrics -QuietLogs` against AzerothCore +
  mod-playerbots on the Mac mini (192.168.88.44:3725, separate machine, so the legacy socket
  crosses a real LAN hop). Proxy at Information log levels, sniff capture on (the
  production default).

### Micro-benchmark headlines (allocated bytes are host-independent)

| Path | Today | Floor | Notes |
|---|---:|---:|---|
| Inbound dispatch, value-only packet (`BuyBackItem`) | 368 B, 280 ns (x64) / 122 ns (M4) | 0 B, 6 ns / 2 ns | Reflection is ~256 B + ~190 ns of it; the rest is `ClientPacket` + `WorldPacket` |
| Inbound dispatch, two strings (`ChatMessageWhisper`) | 552 B | 104 B | Floor is the two `string`s |
| Outbound `PowerUpdate`, construct → wire | 544 B (x64) | ~0 B | 2056 B on macOS: BouncyCastle GCM fallback |
| Outbound `ServerPacket` constructed, never sent | 165 B + finalizer (~300 ns, Gen1/Gen2 promotion) | 0 | `ByteBuffer` finalizer on the undisposed 256-byte rental |
| `WritePackedGuid128` | 64 B | 0 B | `PackUInt64` allocates `byte[8]` per half |
| `ReadPackedGuid128` via `WorldPacket` | 64 B, 187 ns | 0 B, 6.5 ns | Includes the read-mode `WorldPacket` object |

### Live headlines (Release, Information logs, sniff on)

| Scenario | S→C rate | Allocation | GC | Heap |
|---|---:|---:|---:|---:|
| Idle in Orgrimmar after login | 3 pkt/s | 1.5-2 MB/min | 0-1 gen0/min | 116 MB flat |
| Arathi Basin with ~30 bots, full match | 390-620 pkt/s | 1.5-2.3 MB/s | 22-35 gen0/min, 1 gen2 in 15 min | 92 MB flat |

CPU during the match: 1.8% of one core, GC pause 0.02%, lock contention ~0. The proxy is
allocation-bound, not CPU- or lock-bound.

Where the bytes go (15-minute match, 1,009 MB server → client):

| Opcode | Share | Per packet | Why |
|---|---:|---:|---|
| `SMSG_COMPRESSED_UPDATE_OBJECT` | 79% | 94 KB avg, 3.3 MB max | `ObjectUpdate` ctor builds `UnitData` + `PlayerData` + `ActivePlayerData` (~30 KB of nullable arrays) for every Player update; bots are Players. Also 34% of CPU. |
| `SMSG_AURA_UPDATE` | 5% | 3.1 KB | per-aura lists |
| `SMSG_ITEM_QUERY_SINGLE_RESPONSE` | 4% | 1.25 MB | 73% of bytes in a non-BG session; first sight of every item |
| `SMSG_ON_MONSTER_MOVE` | 2% | 1.5 KB | parse + 744 B wire path |
| `SMSG_PARTY_MEMBER_PARTIAL_STATE` | 1% | 64 B | 254,885 packets; throttle from 2026-08-25 holds |

Client → server: every movement packet averages 12-66 KB because a recurring ~394 KB
allocation lands on the modern-socket thread (~284 KB on the legacy thread). Unattributed
so far; see the open question in the companion file.

Second CPU consumer after the update builder: Serilog at ~22%, from per-packet Warn-level
messages (`MonsterMove exceeded MaxSize`, unmapped-opcode drops) formatting enums through the
console theme, plus an `UnmappedOpcodeException` per unmapped legacy packet.

Environment notes: the Mac mini hosting AzerothCore idle-sleeps after one minute and freezes
the Docker VM with it (fixed for the session with `caffeinate`, durable fix
`sudo pmset -c sleep 0 powernap 0`); never attach `dotnet-counters` and `dotnet-trace` at
the same time, the counters' own formatting dominates the trace.

---

## Generated dispatch, measured 2026-09-12

First converted slice: `CMSG_ATTACK_SWING`, `CMSG_ATTACK_STOP`, `CMSG_SET_SHEATHED`,
`CMSG_BUY_BACK_ITEM` moved from the reflective registry to the generated function-pointer table,
with `readonly record struct` packets and codecs over `SpanPacketReader`.

`PacketDispatchBenchmarks`, Windows dev box (i7-6700K), `--job short`. `*_Activator` is the
pre-migration path, kept alive against frozen copies of the old `ClientPacket` classes so the
comparison survives the conversion; `*_Codec` is what production runs now.

| Method | Mean | Allocated |
|---|---:|---:|
| `BuyBackItem_Activator` | 270.8 ns | 368 B |
| `BuyBackItem_Direct` | 85.8 ns | 112 B |
| **`BuyBackItem_Codec`** | **6.0 ns** | **0 B** |
| `AttackSwing_Activator` | 280.0 ns | 360 B |
| `AttackSwing_Direct` | 84.5 ns | 104 B |
| **`AttackSwing_Codec`** | **5.1 ns** | **0 B** |

That lands on the 0 B / 6 ns x64 floor the baseline predicted — ~45x on latency and the whole
per-packet allocation gone. Reflective dispatch was only 1.3% of CPU, so the value here is
allocation and the per-session startup scan, not throughput; see the baseline table above for
where the bytes actually are.

Numbers quoted in a PR should come from the Mac mini M4, not this host.

---

The sections below are historical: undated micro-benchmark results from the original
Span/ByteBuffer work, kept for reference. Compare new work against the dated baseline above.

## Span-Based Packet I/O (Zero-Allocation)

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

## ByteBuffer Optimizations

The core `ByteBuffer` class has been refactored for improved performance:
- ArrayPool-based buffer management reduces GC pressure
- Direct `BinaryPrimitives` usage eliminates BinaryReader/BinaryWriter overhead
- `MemoryStream.ToArray()` optimization for `GetData()`:

| Buffer Size | Original     | Optimized   | Speedup |
|-------------|--------------|-------------|---------|
| Small       | 46.87 ns     | 10.31 ns    | ~4.5x   |
| Medium      | 649.49 ns    | 70.88 ns    | ~9.2x   |
| Large       | 36,383.19 ns | 4,234.92 ns | ~8.6x   |

## Additional Optimizations

- **Enum Conversions**: Cached name-based mappings replace `Enum.Parse(typeof(T), x.ToString())` pattern (8-25x speedup, 95% memory reduction)
- **Opcode Lookups**: `FrozenDictionary` for O(1) opcode resolution
- **WowGuid**: Refactored to value-type record structs eliminating heap allocations
- **NetworkThread**: O(1) socket removal with `ConcurrentQueue`
- **BnetTcpSession**: Zero-allocation buffer management with `Span<T>`
- **Movement Handlers**: Fixed monster/pet movement zig-zag at tile boundaries
