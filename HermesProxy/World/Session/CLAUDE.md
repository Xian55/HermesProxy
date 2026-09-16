# World/Session — one owner per session

`SessionExecutor` is where a session's work runs. A packet handler, a timer callback or a teardown is
**posted** here instead of running on whichever thread happened to deliver it.

Before this, every session was entered by up to twelve threads (realm socket, instance socket, legacy
receive, four kinds of timer, the outbox drain, BNet REST, BNet TCP, the auth receive callback, the
connect callback). `GameState` was guarded by whatever lock the author of each field remembered to
take. The executor replaces that with an ownership rule.

## The rule

```
 Post(run, state)
   │
   ├─ session free and nothing queued → this thread becomes the owner and runs it NOW,
   │                                    before Post returns            ← the common case, 0 hops
   │
   └─ someone already owns it        → queued; the owner drains in FIFO order
                                        after 64 events or 2 ms it hands the rest to the pool,
                                        so no socket thread is held by a busy session
```

- **One owner at a time.** Two events never run together, so state needs no lock against itself.
- **Order is arrival order.** Per producer it is exactly FIFO; across producers it is whatever order
  they posted in, which is what a single-threaded proxy would have seen anyway.
- **An event that throws is logged and dropped.** The session lives on, as the per-frame `catch` in
  `ReadHandler` used to ensure.
- **Backpressure.** Past `HighWaterMark` the host is told to stop reading (the backlog then pushes
  back onto TCP); past `HardCap` the session is dropped rather than grown without limit.
- **`AssertOwner`** runs in Debug only, from `SessionContext.GameState`. It fires when a thread
  reaches session state while an owner is mid-event — the exact bug the executor exists to remove.

## What is posted, and what is not

| Path | Posted? | Why |
|---|---|---|
| Legacy SMSG (`WorldClient.DispatchPacket`) | yes, with the pooled buffer's ownership | the firehose; the receive loop must not free the buffer under a queued handler |
| Legacy handshake (`SMSG_AUTH_CHALLENGE`, the first `SMSG_AUTH_RESPONSE`) | **no** | `SendAuthResponse` turns encryption on right after writing `CMSG_AUTH_SESSION`, and the thread that would own the session is parked in `ConnectToWorldServer` waiting for this very reply. Posting it deadlocks until the 30 s timeout |
| Modern CMSG through the generated dispatch (`WorldSocket.DispatchPacket`) | yes | `ReadHeader` sizes a fresh buffer per frame, so the packet can outlive the read |
| The connection-level opcodes in `WorldSocket.ReadData` (auth, connect-to, encrypted-mode ack, ping, log-disconnect) | **no** | they run before a session exists, or they initialise the crypt that the next frame's decrypt depends on — and that decrypt is on this same thread |
| Keep-alive timer, group ready-check timer | yes | both write session state or send packets |
| Outbox timer (`PacketOutbox.OnTimer`) | not yet | it still runs timer-safe work itself; it moves in slice E, when the outbox lock goes |
| BNet REST / BNet TCP session setup | not yet | they build the session before the executor has anything to own |

## Measuring it

`--metrics` prints one `Executor:` line per interval: events that had to wait, the p99 and max wait,
and the depth right now. **Slice D is judged on that p99.** A queue wait is the price a client packet
pays for arriving while a server packet is being handled; the AV baseline says the worst server
handler in combat is ~7 ms, so a few milliseconds is expected and tens are not.

Queue wait is deliberately **outside** the per-opcode metrics bracket, which still measures only the
handler itself. The two numbers answer different questions.

## Anti-patterns

- **Touching `GameState` from a timer or a socket callback.** Post it. Debug builds will tell you.
- **Blocking inside an event.** The owner is the session; a sleep or a `Task.Wait` in an event stops
  every other packet for that player. The handshake waits are the exception, and they are the reason
  the handshake is not posted.
- **Posting the legacy handshake.** See above — it deadlocks.
- **Assuming a posted event runs on the caller's thread.** It usually does, but not while the session
  is busy, and not after a drain hands over to the pool.
