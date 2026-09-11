# HermesProxy.Benchmarks

BenchmarkDotNet performance benchmarks for the HermesProxy solution. See root [CLAUDE.md](../CLAUDE.md) for solution-wide conventions.

## Run Benchmarks

```bash
# Run all benchmarks
dotnet run --project HermesProxy.Benchmarks -c Release

# Filter by class name
dotnet run --project HermesProxy.Benchmarks -c Release -- --filter "*SpanPacket*"

# List available benchmarks
dotnet run --project HermesProxy.Benchmarks -c Release -- --list flat
```

**Important**: Always run with `-c Release` — BenchmarkDotNet will warn/error in Debug mode.

## Entry Point

`Program.cs` uses `BenchmarkSwitcher.FromAssembly()` — all `[Benchmark]` classes in the assembly are auto-discovered.

## Benchmark Files

| File | Benchmarks |
|---|---|
| `ByteBufferBenchmarks.cs` | `ByteBuffer` read/write performance |
| `SpanPacketBenchmarks.cs` | `SpanPacketReader`/`SpanPacketWriter` vs `ByteBuffer` |
| `BnetPacketParserBenchmarks.cs` | BNet packet parsing performance |
| `BnetRpcCodecBenchmarks.cs` | BNet RPC request parse (span `MergeFrom`) and response framing (`RentRpcFrame`) on login-shaped messages |
| `ExtensionsBenchmarks.cs` | Extension method performance |
| `PacketDispatchBenchmarks.cs` | Inbound dispatch on real `ClientPacket`s: Activator path vs direct vs `SpanPacketReader` floor |
| `SendPipelineBenchmarks.cs` | `ServerPacket` construct → `WritePacketData` → framed + encrypted wire bytes, per stage |
| `PackedGuidBenchmarks.cs` | `WorldPacket` vs `PackedGuidHelper` packed-GUID encode/decode |
| `MovementHandlerPrologueBenchmarks.cs` | `HandlePlayerMove` opcode translation: string/reflection round trip vs the prebuilt map |
| `UpdateMaskBenchmarks.cs` | V1_14/V2_5 `UpdateFieldsArray` / `DynamicUpdateFieldsArray` write path vs the previous `BitArray`-backed mask (verbatim `Legacy*` copies) |
| `HighGuidBenchmarks.cs` | `HighGuid` high-guid type lookup and 64↔128 guid conversion vs the previous per-lookup `HighGuid` object (verbatim `Legacy*` copies) |

## Conventions

- Use `[MemoryDiagnoser]` on benchmark classes to track allocations
- Use `[ShortRunJob]` for quick iteration during development
- Mark the baseline implementation with `[Benchmark(Baseline = true)]`
- Compare Original vs Optimized vs Pooled implementations side-by-side
- Access `internal` members via `InternalsVisibleTo` (set in Framework and HermesProxy `.csproj` files)

## Project References

- `Framework` — benchmark framework internals directly
- `HermesProxy` — benchmark application-level components
