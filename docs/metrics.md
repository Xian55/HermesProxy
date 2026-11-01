# Metrics

`--metrics` turns them on, and `--metrics-interval <seconds>` sets how often the summary is logged. See [configuration.md](configuration.md).

When enabled, HermesProxy tracks per-opcode handler latency **and managed bytes allocated per packet** for both directions (Client → Server and Server → Client), plus process-wide GC counters, and emits a summary every minute (or every `--metrics-interval` seconds) through the `Server` logging category. The GC line is printed even when idle; the per-opcode tables only once traffic has been observed.

Each summary covers one interval. The packet rates, the GC line and the latency table's `Int` (packets) and `IntMax` (slowest packet) columns reset every summary; the latency table lists only opcodes seen in that interval, slowest first. `P50`/`P95`/`P99` are over each opcode's last 1000 packets, and `Max`, `Total` and the allocation table are totals since startup.

```
17:42:14 | I | S | Server          | Proxy Metrics: 842 C->S opcodes, 1203 S->C opcodes tracked
17:42:14 | I | S | Server          | Proxy Metrics (Uptime: 00:12:03) | C->S 18422 pkts (31.2/s) | S->C 90311 pkts (150.8/s)
17:42:14 | I | S | Server          | GC over 60s: gen0 +14 gen1 +2 gen2 +0 | allocated 118.4 MB (1.97 MB/s) | heap 61.2 MB | pause 0.31%
17:42:14 | I | S | Server          | <per direction: top 20 opcodes by max latency this interval with avg/max/total bytes, then top 20 by total allocated bytes>
```

Useful for: identifying slow-dispatching opcodes, finding which opcodes drive allocation and GC pressure, spotting regressions after a packet-handler change, and validating that hot-path migrations didn't shift the profile. Leave it off in normal play — it costs a timestamp and an allocation-counter read per packet.
