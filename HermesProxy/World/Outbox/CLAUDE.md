# World/Outbox — sending now, later, or after something

The one place a handler says **"send this later"** or **"send this after that"**. A session owns two
outboxes:

- `ctx.ToClient` (`ClientOutbox`): packets to the modern client, also reached by
  `SendPacketToClient`
- `ctx.ToServer` (`ServerOutbox`): packets to the legacy server, also reached by
  `SendPacketToServer`

See the root [CLAUDE.md](../../../CLAUDE.md) for solution-wide conventions.

| File | Holds |
|---|---|
| `PacketOutbox.cs` | The engine: holds, triggers, gates, timers, lanes, release, teardown |
| `ClientOutbox.cs` | Client facade: socket routing and **parks**, `Send`, `SendOn`, `After`, `AfterBatch`; `IClientWire` |
| `ServerOutbox.cs` | Server facade: `Send`, `AfterHandled`; `IServerWire` |
| `SessionWires.cs` | Production wires: the session's realm and instance sockets; its `WorldClient` |
| `OutboxTypes.cs` | `OutboxEvent`, `OutboxGate`, `OutboxScope`, `HoldKey`, `HoldOptions`, `OutboxOptions` |

## Why this exists

Before it, every reason to delay a packet grew its own mechanism on the session:
- `_delayedPacketsToClient/ToServer`
- `PendingRealmPackets`, `PendingUninstancedPackets` + a 30 s `Thread.Sleep`
- `DeferredObjectUpdates`, `PendingPetUpdateBatches`, `PendingPetSpells`, `PendingMailListPacket`
- flag deferrals, the guild rank coalescer, and `Thread.Sleep` pacing in mail and auction

Each had its own locking (or none) and its own ordering; one drained newest-first. None was cleared as
a unit on disconnect, and none had a test. They were fed and drained from four or more threads. The
migration moves them here one slice at a time. What has moved and what hasn't is listed below.

## Map 1: send, park, hold, trigger, release

```
 handler code  (any thread today; the session owner once the executor lands)
   │  ctx.ToClient.Send(p) / SendOn(conn,p) / After(opcode,p) / AfterBatch(p)
   │  ctx.ToServer.Send(p) / AfterHandled(opcode,p)
   │  either: When(event|gate, …) / WhenAll(events, …) / Delay / Paced / Coalesce / Exclusive
   │          When(event, state, release) / Delay(t, state, release) → Peek<T>(key) / Claim(key, state)
   │          Cancel / Release / LaneDone
   ▼
 ┌─ SEND ── client: both sockets attached and nothing parked → straight to the socket (no lock) ─┐
 │          server: straight to the world client                                                  │
 └───────────────────────────────────────────────────────────────────────────────────────────────┘
   │ client socket not attached, or something already parked
   ▼
 ┌─ PARK (client only) ── serialize on this thread, then under the route lock ─────────┐
 │  realm not attached            → head-of-line queue                                 │
 │  instance not requested yet    → early-instance queue (realm packets keep flowing)  │
 │  instance connecting           → head-of-line queue (everything waits, in order)    │
 │  Attach(conn) → one drainer writes them out; later sends queue behind it            │
 └──────────────────────────────────────────────────────────────────────────────────────┘

   │ hold
   ▼
 ┌─ under the outbox lock ─────────────────────────────────────────────────────┐
 │  register: event lists · gate lists · key index · lanes · deadline          │
 │  trigger:  move every hold that is now satisfied onto a local release list  │
 └──────────────────────────────────┬──────────────────────────────────────────┘
                                    │ lock released. Nothing below runs under it
                                    ▼
        release in registration order: send the packet (it may park), or run the continuation
        (a release that triggers more releases appends to the same run, breadth first,
         and the thread that fired the trigger finishes the whole run before returning)

        Claim(key, state) instead: the hold is taken off every list and never released; the
        caller does its work inline (e.g. folds a held pet batch into the packet it is building)

 triggers  OpcodeSent ............ automatic when a client packet reaches its socket (incl. drains)
           OpcodeHandled ......... GlobalSessionData.OnLegacyPacketHandled, after every SMSG handler
           Signal(UpdateBatchEnd)  same place, after SMSG_(COMPRESSED_)UPDATE_OBJECT
           ItemTemplate(entry) ... QueryHandler.HandleItemQueryResponse, valid or invalid answer
           ItemText(id) .......... MailHandler.HandleQueryItemTextResponse (pre-3.3.0)
           GuidKnown(player) ..... UpdateHandler.SendUpdateBatch end, V3_4_3, at EVERY batch while the
                                   client has the player (so a late hold still goes out next batch)
           Gate InWorld .......... CharacterHandler: opened on SMSG_LOGIN_VERIFY_WORLD,
                                   closed on login failure and ReplaceGameState
           Tick() / TickParks() .. same place (legacy receive thread) · timer thread for timer-safe
           Attach / Detach ....... WorldSocket.HandleEnterEncryptedModeAck; LOG_DISCONNECT; closed-socket send;
                                   logout; OnDisconnect · BeginInstanceConnect: CharacterSystem login
 teardown  Discard(GameState) + DiscardParked + gates closed ← GlobalSessionData.ReplaceGameState
           Discard(LegacyConnection) ← WorldClient.Disconnect
           Discard(Session)          ← GlobalSessionData.OnDisconnect (also closes gates, frees lanes)
```

## Map 2: where the session is heading

```
                 Before B          After B                         After C (now)    After D-E
 hold-backs      ~20 bespoke       transport + sleeps moved;       outbox only      outbox only
                                   data holds still bespoke
 threads         4+                4+                              4+               1 owner at a time
 locks           4 session         ObjectCache + DeferredUpdates   ObjectCache      none on session state
                                   + outbox + route                + outbox + route
 handler sleeps  mail, auction,    none                            none             none; connect spin gone
                 30 s instance
```

**Moved in slice B:**
- `PendingRealmPackets` and `PendingUninstancedPackets` + the 30 s sleep → parks
- `_delayedPacketsToServer` (name queries) → `When(InWorld)`
- `_delayedPacketsToClient`: spell history → `After(SMSG_SEND_UNLEARN_SPELLS)`; collision height → `AfterBatch`
- guild rank coalescer → `Coalesce`
- vanilla mail sleep → `Paced`
- auction split sleeps → `PartialStackAuctionPost` on `Exclusive` + `When(UpdateBatchEnd)`
- direct `RealmSocket.SendX()` calls in legacy handlers → `WorldSocket.BuildX` + `SendPacketToClient`

**Moved in slice C** (all `GameState` scope, so a relog drops them):

| Was | Now | Timeout |
|---|---|---|
| `DeferredObjectUpdates` + lock | `WhenAll(ItemTemplate…)` → `QueryHandler.FlushDeferredUpdate` | 10 s, Release |
| `PendingPetUpdateBatches` | `When(GuidKnown(player), HeldPetUpdateBatch)`: claimed and merged by the deferred player batch, otherwise sent by `SendUpdateBatch` | 20 s, Release |
| `PendingPetSpells` (+ legacy guid) | `Delay(10 s, HeldPetSpells)`: claimed by whichever path sends the pet create; newer message or clear cancels | 10 s, Release (re-GUID best effort) |
| `PendingMailListPacket` + `RequestedItemTextIds` | `WhenAll(ItemText…)`, newest list cancels the older | 5 s, Release |
| `DeferredCorpseDestroys` | `When(UpdateBatchEnd)` per corpse; a recreate `Cancel`s it | 5 s, Release |
| `PendingToysSync` | `When(GuidKnown(player))`, newest request cancels the older | 60 s, Release (re-checks) |

Two fixes rode along: a pet batch whose player create was never deferred used to wait forever, and
an older mail list could be sent after a newer one. A pet batch that carries the player's own create
is no longer held, and `SMSG_NEW_WORLD` cancels pet batches held for the old map.

**Also moved:** `DeferredAttackStop` / `WaitingForAttackStart` → `World/Server/Systems/MeleeAttackOrder.cs`.
A `CMSG_ATTACK_STOP` is `When(OutboxGate.SwingAnswered)` under key `AttackStop`: the swing closes the
gate, `SMSG_ATTACK_START`, a swing error, a player stop and `SMSG_CANCEL_COMBAT` open it, and a new
swing `Cancel`s the held stop so the server never sees one wedged between two swings. 2 s, Release.
The plan had this waiting for slice D because the two bools were written on the socket thread and read
on the legacy thread; the gate removes the race, so it landed here instead.

**Still bespoke:** `PendingCreateCharName` and `PendingEnchantmentLog` — correlation, not holds, so
they stay domain logic.

## Which call for which situation

| Situation | Call | Example |
|---|---|---|
| Send now | `SendPacketToClient(p)` / `SendPacketToServer(p)` (or `ToClient.Send` / `ToServer.Send`) | almost everything |
| Reply on a named socket, whatever the packet's type says | `ToClient.SendOn(conn, p)` | |
| Send right after the client got packet X | `ToClient.After(opcode, p)` | spell history after `SMSG_SEND_UNLEARN_SPELLS` |
| Send after the legacy update batch being handled is fully out | `ToClient.AfterBatch(p)` | collision height after the mount Values |
| Send once the legacy server's packet X has been handled | `ToServer.AfterHandled(opcode, p)` | |
| Wait for a state | `When(OutboxGate.X, p)` plus `SetGate(X, true)` where the state changes | name queries until in world |
| Wait for several pieces of data | `WhenAll([ItemTemplate(a), ItemTemplate(b)], build)` plus `Notify` as each arrives | player create waiting for item templates; mail list waiting for letter texts |
| Work that goes out on its own **or** gets folded into something sent first | `When(evt, state, release, new(Key: k))`; the other path does `Peek<T>(k)`, decides, then `Claim(k, state)` and does the work inline | pet batch merged into the player's deferred batch |
| Work normally picked up by someone else, with a deadline fallback | `Delay(timeout, state, release, new(Key: k))` + `Peek`/`Claim` | pet spell bar claimed when its pet's create goes out |
| Only the newest request counts | `Cancel(k)`, then register under `k` | mail list, pet spell bar, toy sync |
| Space packets apart | `Paced(key, interval, p)` | multi-attachment mail on vanilla |
| Collapse a burst to the newest | `Coalesce(key, window, build, new(RunOnTimer: true))` | guild rank permissions |
| Run multi-packet work one at a time | `Exclusive(lane, start)` … `LaneDone(lane)` on every exit path | partial-stack auction posts |
| Wait for a condition checked after each update | register `When(Signal(UpdateBatchEnd), check, new(Key: k))`, **then** check once and `Cancel(k)` if already true | `PartialStackAuctionPost.Arm` |
| Undo a hold | `Cancel(key)` | a corpse destroy undone by a recreate |
| Force a hold out early | `Release(key)` | |

Keys need a `HoldKeyKind`. Add one per feature to `OutboxTypes.cs`, so two features can't collide on
the same number.

## Guarantees

- **Order.**
  - Holds released by one trigger go out in registration order.
  - A packet written by `Send` goes out before anything its trigger releases.
  - Parked packets drain in the order they parked. A packet sent during a drain queues behind it.
  - While the instance connection is being made, realm and instance packets share one queue, so neither
    overtakes the other.
- **No send pump, no per-socket queue.** On the fast path a thread's own sends reach the wire in the
  order it made them, on both connections. ad0122ee broke exactly this.
- **Next occurrence.** An event releases holds registered **before** it fired. A hold registered during a
  release waits for the next trigger.
- **Gates are state.** A hold on an open gate goes out immediately. The check and the hold are one step.
- **Bounded.**
  - **Hold timeouts:** 60 s by default, then dropped.
  - **Head-of-line parks:** dropped after `ClientOutbox.ParkTimeout` (30 s). Early-instance parks
    don't expire, because the player may sit at character select.
  - **Caps:** 4096 holds and 8192 parked packets.
  - **Lanes:** a lane waiter that times out is dropped, never started.
- **Contained.** A throwing continuation, wire write or timer callback is logged and dropped.
- **Late data is harmless.** An event after its hold timed out, was cancelled or was discarded does
  nothing.
- **Claimed means never released.** `Claim` succeeds only for the exact state object still waiting;
  once it returns true no trigger, timeout or discard touches that hold again. It returns false if
  the hold already went, and then the caller must not do the work.
- **Nothing leaks.** Dropped packets are disposed.

## Threading rules

- **Any thread may call anything.**
  - Hold state lives under the outbox lock; park state under the client route lock.
  - The two locks are never held together.
  - Wire writes, continuations and disposal never run under either.
- **Parked packets are serialized on the thread that parks them.** `UpdateObject.Write` reads and
  mutates session state, and the thread that drains a park is a socket thread or the pool.
- **Held client packets are serialized when released,** on the releasing thread.
- **Deadlines and the timer thread.** A deadline only acts on the timer thread when acting reads no
  session state:
  - a legacy packet;
  - a drop;
  - a hold marked `RunOnTimer`.

  **Everything else waits for `Tick()`,** which `OnLegacyPacketHandled` calls after every legacy packet.
  A deadline-driven client or continuation release therefore needs the legacy server to be sending,
  which it constantly is while in the world.
- **Drains** hand the rest of a long backlog to the thread pool after 8 batches of 64, so a socket
  thread's next read isn't held up by the login burst.

## Anti-patterns

- **A new `Pending*` / `Deferred*` / `Delayed*` field on the session.** Add an event, gate, key or lane
  here.
- **`Thread.Sleep` in a handler to space packets or wait for the other thread.** Use `Paced`, `Delay`, or
  a continuation on the event you were waiting for.
- **Writing to `session.RealmSocket` / `InstanceSocket` directly from a legacy handler.** It throws while
  the socket is absent and skips the parks. Build the packet and `SendPacketToClient` it.
- **Checking a condition, then registering a hold for it.** The update can land in between. Register
  first, then check, and `Cancel` the hold if the check already passed.
- **A per-socket send queue or send pump.** ad0122ee gave each modern socket its own channel. SpellPrepare
  (realm) and SpellStart/SpellGo (instance), written back to back, then reached the wire in either
  order, and action-bar highlights stuck. It was reverted in d3f61b9b.
- **Queueing a legacy send while the crypt is switched on outside that queue.** The uncommitted
  "Wave 2-C" loop let `InitializeEncryption` run before `CMSG_AUTH_SESSION` reached the socket.
- **Logging a hold, park or serialization to the `.pkt` sniff.** The record belongs where wire order is
  set, in `WorldSocket.SendPacket` / `WorldClient.SendPacket`. Captures from before 9cfe3c74 didn't
  follow this, so treat ordering conclusions drawn from them as unverified.
- **A hold whose timeout sends a stale packet by default.** Choose `OnTimeout: Release` only when late is
  better than never.
- **`Release(key)` or `Notify` from inside a continuation, expecting the work to happen right there.**
  Inside a release run they append to the run, so that work goes out after the current continuation
  finishes. The deferred player batch has to send the pet spell bar before its world-entry handshake,
  which is why it `Claim`s the bar and sends it inline.
- **A per-packet lock or allocation to look for a hold.** Guard with `HasPending` (one volatile read)
  before `Peek`, `Cancel` or a lookup on the update path; closures and state records belong on the
  rare path that registers the hold.

## Logging and tests

- **Logging.** `World/Logging/OutboxLogMessages.cs`, EventId 1500-1519.
  - **Held, released, parked, park discarded:** Debug.
  - **A timeout:** Warning at most once per 10 s per outbox.
  - **Parks dropped after waiting:** Warning.
  - **Callout failures, overflow, park overflow, runaway chains:** Error.
- **Tests.**
  - **`HermesProxy.Tests/World/Outbox`:** the recording wires record each write with its thread and
    can simulate a closed socket; `FakeTimeProvider` drives deadlines.
  - **Pins:**
    - `Send_NoHolds_WritesInCallOrderAcrossBothConnections` and
      `InstanceConnecting_EverythingWaitsInOneQueue_SoNothingOvertakes` (ad0122ee);
    - `PacketSentDuringADrain_QueuesBehindIt`;
    - `ParkedPacket_IsSerializedOnTheThreadThatParksIt`;
    - `Gate_CheckAndHold_IsAtomicAgainstAConcurrentOpen`.
  - **State holds and claims:** `OutboxClaimTests.cs`, including the slice C shapes (item-template
    wait, corpse cancel, newest mail list, late toy sync).
  - **The auction sequence:** `World/Server/PartialStackAuctionPostTests.cs`.

## Adding a trigger

1. Add the event kind (or signal, gate, key kind) to `OutboxTypes.cs`, with a factory on `OutboxEvent`.
2. Call `Notify(...)` or `SetGate(...)` at the one place the fact becomes true, and say in a comment why
   that is the place.
3. Test it through `ClientOutbox` or `ServerOutbox` with the recording wires: registration before and
   after the event, timeout, and discard.
4. Add a row to the table above.
