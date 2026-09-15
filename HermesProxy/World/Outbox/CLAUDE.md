# World/Outbox — sending now, later, or after something

The one place a handler says **"send this later"** or **"send this after that"**. A session owns two
outboxes:

- `ctx.ToClient` (`ClientOutbox`): packets to the modern client
- `ctx.ToServer` (`ServerOutbox`): packets to the legacy server

See the root [CLAUDE.md](../../../CLAUDE.md) for solution-wide conventions.

| File | Holds |
|---|---|
| `PacketOutbox.cs` | The engine: holds, triggers, gates, timers, release, teardown |
| `ClientOutbox.cs` | Client facade: `Send`, `SendOn`, `After`, `AfterBatch`; `IClientWire` |
| `ServerOutbox.cs` | Server facade: `Send`, `AfterHandled`; `IServerWire` |
| `SessionWires.cs` | Production wires over `WorldClient` and the session's sockets |
| `OutboxTypes.cs` | `OutboxEvent`, `OutboxGate`, `OutboxScope`, `HoldKey`, `HoldOptions`, `OutboxOptions` |

## Why this exists

Before it, every reason to delay a packet grew its own mechanism on the session:
- `_delayedPacketsToClient/ToServer`
- `PendingRealmPackets`, `PendingUninstancedPackets` + a 30 s `Thread.Sleep`
- `DeferredObjectUpdates`, `PendingPetUpdateBatches`, `PendingPetSpells`, `PendingMailListPacket`
- flag deferrals, the guild rank coalescer, and `Thread.Sleep` pacing in mail and auction

Each had its own locking (or none) and its own ordering; one drained newest-first. None was cleared as
a unit on disconnect, and none had a test. They were fed and drained from four or more threads. The
migration plan moves them here one slice at a time. Until a mechanism has moved, it still works the old
way.

## Map 1: send, hold, trigger, release

```
 handler code  (any thread today; the session owner once the executor lands)
   │  ctx.ToClient.Send(p) / SendOn(conn,p) / After(opcode,p) / AfterBatch(p)
   │  ctx.ToServer.Send(p) / AfterHandled(opcode,p)
   │  either: When(event|gate, …) / WhenAll(events, …) / Delay / Paced / Coalesce / Cancel / Release
   ▼
 ┌─ SEND ── direct wire write on the calling thread; with nothing held that is all it costs ──┐
 └────────────────────────────────────────────────────────────────────────────────────────────┘
   │ hold
   ▼
 ┌─ under the outbox lock ─────────────────────────────────────────────────────┐
 │  register: event lists · gate lists · key index · deadline (every hold)     │
 │  trigger:  move every hold that is now satisfied onto a local release list  │
 └──────────────────────────────────┬──────────────────────────────────────────┘
                                    │ lock released. Nothing below runs under it
                                    ▼
        release in registration order: write the packet, or run the continuation
        (a release that triggers more releases appends to the same run, breadth first,
         and the thread that fired the trigger finishes the whole run before returning)
                                    │
               ┌────────────────────┴─────────────────────┐
               ▼                                          ▼
      IClientWire.Write / WriteOn                  IServerWire.Write
      (WorldClient.SendPacketToClient,             (WorldClient.SendPacketToServer)
       or a named socket)

 triggers  Notify(OutboxEvent): OpcodeSent (automatic on ToClient sends) · OpcodeHandled · Signal
                                · ItemTemplate · ItemText · GuidKnown
           SetGate(gate, open) · deadline (timer thread, or Tick) · Release(key)
 teardown  Discard(GameState)        ← GlobalSessionData.ReplaceGameState
           Discard(LegacyConnection) ← WorldClient.Disconnect
           Discard(Session)          ← GlobalSessionData.OnDisconnect (also closes gates)
```

## Map 2: where the session is heading

```
                 Today (slice A)          After slices B-C             After slices D-E
 hold-backs      ~20 bespoke + outbox     outbox only                  outbox only
 threads         4+ touch session state   4+ (unchanged)               1 owner at a time
 locks           4 session + outbox       outbox lock only             none on session state
 handler sleeps  mail, auction, 30 s      gone (Paced, continuations)  gone; connect spin gone
 disconnect      mostly never cleared     scoped discard               scoped discard
```

Slice B also adds **parks** for the realm and instance sockets (packets that wait for a connection to
attach), and moves the transport holds and sleeps here. Slice C moves the data-dependency holds. Slice D
puts one owner on the session. The plan and its reasoning live with whoever is driving the migration;
this file describes what is in the code now.

## Which call for which situation

| Situation | Call | Example |
|---|---|---|
| Send now | `ToClient.Send(p)` / `ToServer.Send(p)` | almost everything |
| Reply on the socket a request arrived on, whatever the packet's type says | `ToClient.SendOn(conn, p)` | `SpellPrepare` (a realm packet) answering on the instance socket |
| Send right after the client got packet X | `ToClient.After(opcode, p)` | spell history after `SMSG_SEND_UNLEARN_SPELLS` |
| Send after the current legacy update batch is fully out | `ToClient.AfterBatch(p)`, and the handler calls `Notify(Signal(UpdateBatchEnd))` | collision height after the mount Values |
| Send once the legacy server's packet X has been handled | `ToServer.AfterHandled(opcode, p)` | |
| Wait for a state | `When(OutboxGate.X, p)` plus `SetGate(X, true)` where the state changes | name queries until in world |
| Wait for several pieces of data | `WhenAll([ItemTemplate(a), ItemTemplate(b)], build)` plus `Notify` as each arrives | player create waiting for item templates |
| Space packets apart | `Paced(key, interval, p)` | multi-attachment mail on vanilla |
| Collapse a burst to the newest | `Coalesce(key, window, build, new(RunOnTimer: true))` | guild rank permissions |
| Undo a hold | `Cancel(key)` | a corpse destroy undone by a recreate |
| Force a hold out early | `Release(key)` | |

Keys need a `HoldKeyKind`. Add one per feature to `OutboxTypes.cs`, so two features can't collide on
the same number.

## Guarantees

- **Order.**
  - Holds released by one trigger go out in the order they were registered.
  - A packet written by `Send` goes out before anything its trigger releases.
  - A thread's own sends leave in the order it made them, on both connections: there is no send pump
    and no per-socket queue.
- **Next occurrence.** An event releases holds registered **before** it fired. A hold registered during
  a release waits for the next trigger.
- **Gates are state.** A hold on an open gate goes out immediately. The check and the hold are one step,
  so a gate that opens concurrently can't strand a hold.
- **Bounded.**
  - **Timeouts:** every hold has one. When none is given, it is `OutboxOptions.DefaultTimeout` (60 s),
    and the packet is **dropped**, not sent.
  - **Hold cap:** `MaxPendingHolds` (4096). Past it a hold is refused and the overflow callback runs
    once per episode.
  - **Release chains:** stopped after 10,000 releases in one run.
- **Contained.** A throwing continuation, wire write or timer callback is logged and dropped. The other
  holds and the session carry on.
- **Late data is harmless.** An event after its hold timed out, was cancelled or was discarded does
  nothing to that hold.
- **Nothing leaks.** Dropped packets are disposed: `ServerPacket.Discard`, `WorldPacket.Dispose`.

## Threading rules

- **Any thread may call anything.** State changes happen under one `Lock`. Wire writes, continuations,
  overflow callbacks and packet disposal never run while it is held. A test pins this by registering a
  hold from another thread in the middle of a write.
- **Deadlines and the timer thread.** A deadline only acts on the timer thread when acting reads no
  session state:
  - a legacy packet (fully built when handed over);
  - a drop;
  - a hold marked `RunOnTimer`.

  **Everything else waits for `Tick()`:** a client packet (writing an `UpdateObject` reads GameState) or
  an ordinary continuation. Call `Tick()` from a thread that owns session state; with nothing due it
  costs one volatile read. Slice B wires `ToClient.Tick()` / `ToServer.Tick()` into
  `WorldClient.HandlePacket`. Until then only timer-safe deadlines act.
- **Client packets are serialized when released,** on the releasing thread, because
  `UpdateObject.Write` reads and mutates session state. The parks added in slice B serialize at park
  time instead, for the same reason.
- **Opcode tracking.** `ToClient.Send` reads `HasPending` before writing. With nothing held it never
  looks up the opcode.

## Anti-patterns

- **A new `Pending*` / `Deferred*` / `Delayed*` field on the session.** Add an event, gate or key here.
- **`Thread.Sleep` in a handler to space packets or wait for the other thread.** Use `Paced`, `Delay`, or
  a continuation on the event you were waiting for. A sleep blocks a socket thread today, and under the
  executor it would block the whole session.
- **A per-socket send queue or send pump.** ad0122ee gave each modern socket its own channel and
  pump. SpellPrepare (realm) and SpellStart/SpellGo (instance), written back to back, then reached the
  wire in either order, and action-bar highlights stuck. It was reverted in d3f61b9b.
- **Queueing a legacy send while the crypt is switched on outside that queue.** The uncommitted
  "Wave 2-C" loop let `InitializeEncryption` run before `CMSG_AUTH_SESSION` reached the socket, so its
  header went out encrypted. If a legacy queue is ever added, enabling encryption must travel through it.
- **Logging a hold, park or serialization to the `.pkt` sniff.** The capture record belongs at the
  point that sets wire order (`WorldSocket.SendPacket` / `WorldClient.SendPacket`), or captures stop
  matching the wire. Before 9cfe3c74 they didn't match, so treat any ordering conclusion drawn from an
  older capture as unverified.
- **A hold whose timeout sends a stale packet by default.** Choose `OnTimeout: Release` only when late is
  better than never.

## Logging and tests

- **Logging.** `World/Logging/OutboxLogMessages.cs`, EventId 1500-1519, category Server.
  - Held and released: Debug.
  - A timeout: Warning at most once per 10 s per outbox, Debug for the rest.
  - Callout failures, overflow and runaway chains: Error.
  - A wire with no connection to write to: Warning.
- **Tests.** `HermesProxy.Tests/World/Outbox`. The doubles record every write with its thread;
  `FakeTimeProvider` (`Microsoft.Extensions.TimeProvider.Testing`) drives deadlines.
  - The ad0122ee order pin is `Send_NoHolds_WritesInCallOrderAcrossBothConnections`.
  - The concurrency tests are `ConcurrentHoldsAndTriggers_EveryHoldReleasedExactlyOnce` and
    `Gate_CheckAndHold_IsAtomicAgainstAConcurrentOpen`.

## Adding a trigger

1. Add the event kind (or signal, gate, key kind) to `OutboxTypes.cs`, with a factory on `OutboxEvent`.
2. Call `Notify(...)` or `SetGate(...)` at the one place the fact becomes true, and say in a comment why
   that is the place.
3. Test it through `ClientOutbox` or `ServerOutbox` with the recording wires: registration before and
   after the event, timeout, and discard.
4. Add a row to the table above.
