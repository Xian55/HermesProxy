# World/Server/Systems — CMSG translation

One `static class <Domain>System` per domain. Each `[HandlesCmsg]` method takes a decoded packet
and a `SessionContext`, then does one of two things:
- **Translates it for the legacy emulator:** builds a legacy `WorldPacket` and calls
  `ctx.SendPacketToServer`.
- **Answers the modern client itself:** calls `ctx.SendPacketToClient` or `ctx.SendPacket`.

If a packet has to wait for something, such as another packet, the player entering the world, data
arriving or a delay, hold it through `ctx.ToClient` / `ctx.ToServer` ([World/Outbox/CLAUDE.md](../../Outbox/CLAUDE.md)).
Don't add a `Pending*` field or a `Thread.Sleep`.

The packet was decoded by a codec in `World/Server/Packets/Codecs`. Wiring, handler shapes and
generator diagnostics are covered in [World/Dispatch/CLAUDE.md](../../Dispatch/CLAUDE.md). See the
root [CLAUDE.md](../../../../CLAUDE.md) for solution-wide conventions.

The other direction is not here. Legacy SMSG handlers are `WorldClient` instance methods in
`World/Client/PacketHandlers`, marked `[HandlesSmsg]`.

## Merge into the existing file, never write it fresh

Most domains already have a `*System.cs`, often holding an `EmptyClientPacket` handler for that
domain's zero-length opcodes. Creating the file anew, whether with the Write tool, `cat >`, or a
script opening it `"w"`, deletes every handler already in it. **Nothing else notices:**
- the build stays green, because the generator finds handlers by attribute and nothing references
  them by name;
- the test suite stays green, because no test names the opcode.

The only signal is removed thunks in the dispatch snapshot. So:

1. `ls` this folder before adding a system. If the domain exists, insert into that file.
2. After building, read `PacketDispatchGeneratorTests.GeneratedCmsgDispatch` before accepting:
   `diff --strip-trailing-cr <verified> <received> | grep -c '^<'` must be `0` for an addition.

This has happened twice (`TradeSystem`, `SupportTicketSystem`).

## Moving a handler body

Move bodies **verbatim**. Never retype or tidy one on the way. The codec equivalence tests prove the
bytes are read the same; nothing proves the handler still *behaves* the same, because moving it
deletes the original. A paraphrased `CMSG_CHAT_MESSAGE_EMOTE` once sent `lang=0` instead of
`Language.Common`. It dropped a cMaNGOS workaround whose comment predicted the exact failure, while
every test passed.

The conversion allows exactly two rewrites:
- prefix session calls with `ctx.`;
- on shape B, take the opcode as a parameter instead of reading it back with
  `packet.GetUniversalOpcode()`.

`HermesProxy.Tests/World/Dispatch/Reference/verify-handler-port.py` diffs a moved body against its
original at a git ref, normalising only those two rewrites. It exits with the mismatch count. A
rename of the method is fine; say so in the class remarks, as `GroupSystem` does.

## Writing a handler

- **State lives on the session**, via `ctx.GameState` / `ctx.GetSession()`. Systems are static and
  one proxy serves many sessions, so a mutable `static` field here is shared by every player.
  Static `readonly` loggers and lookup tables are fine.
- **Client-layout differences go in the codec**, as a `[PacketCodec]` range. Differences in what
  the *legacy* server expects stay in the body as `LegacyVersion.AddedInVersion(…)` checks, which
  the JIT folds to constants.
- **Version-gate new behaviour.** These files serve every modern client (1.14, 2.5, 3.4.3). A
  change aimed at one build must not alter the bytes another build sends.
- **Shape B forwarders** (`new WorldPacket(opcode)`) rely entirely on the universal → legacy
  opcode table. If you add one covering a contiguous opcode block, add its values to
  `ShapeBOpcodeForwardingTests`.
- **Logging:** new log lines use `[LoggerMessage]`, per the root rules. Existing CMSG messages are
  in `World/Logging/WorldSocketLogMessages.cs`. The `Log.Print` calls still in some systems came
  across verbatim with their bodies; don't copy that pattern.
- Keep the class `<summary>` saying which domain it translates. Use `<remarks>` for anything
  non-obvious about where the bodies came from.

## Verifying

1. Build and run the tests. Check the snapshot diff as above; `docs/opcode-coverage.md`
   regenerates if coverage changed, so commit it.
2. Playtest the action on a real backend. In the proxy log, the opcode should show `Received`
   **and** a matching `Sending` to the server. `Received` alone means the handler returned early,
   usually on data the codec mis-read.
