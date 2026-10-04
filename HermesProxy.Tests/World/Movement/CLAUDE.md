# HermesProxy.Tests/World/Movement — movement block gates

Tests for the movement block: `MovementInfo`, the two codecs that read and write it
(`LegacyMovementCodec`, `ModernMovementCodec` in `HermesProxy/World/Objects`), `MovementSanitizer`,
and the movement part of a create. A movement block that changes on the wire does not throw. The
client rubber-bands, drops a spline, or falls through a boat, so these are what stands between a
codec edit and a playtest. See the project [CLAUDE.md](../../CLAUDE.md) for runner conventions.

| File | Gate |
|---|---|
| `MovementWireGoldenTests.cs` | every packet that carries a movement block, through the real handlers, byte for byte against `Golden/` |
| `MovementLayoutTests.cs` | build to layout mapping, and every pair's snapshot replayed through the codecs in one process |
| `MovementSanitizerTests.cs` | the flag and orientation repairs |
| `MovementInfoTests.cs` | `MovementInfo` as a value: defaults, "no transport", "no movement block", reads into a used destination |
| `run-version-matrix.sh` | runs this folder once per build pair |

The packets come from `../../Support/MovementScenarios.cs`, which `MovementTranslationBenchmarks`
shares, so a benchmark number is for output a test here has already proved unchanged.

## One build pair per process

`LegacyVersion` and `ModernVersion` are `static readonly`. A plain `dotnet test` runs the handlers
under the default pair (3.3.5a to 1.14.2) and nothing else.

- **`run-version-matrix.sh`** sets `HERMES_TEST_LEGACY_BUILD` / `HERMES_TEST_MODERN_BUILD` and runs
  this folder under six pairs: each legacy era, each modern movement layout, each of the five
  `ObjectUpdateBuilder` families. **Run it after any change to a codec, the sanitizer, a handler that
  reads a movement block, or a create's movement block.** Only this folder is written to pass under
  a non-default pair; the rest of the suite is not.
- **`MovementLayoutTests`' replay tests** cover all six pairs in a plain run, but only the codecs:
  a layout is named by an enum value, so they pass each pair's layouts explicitly. They do not run
  the handlers. A handler that stops sanitising, or a create that loses its rotation, shows up only
  in the matrix.

## Snapshots

One set per pair, in `Golden/`, named `<Test>_<legacy>.<modern>.verified.txt`.

- They were captured from the handlers at a45aa8e1, while `MovementInfo` was still one class that
  read and wrote itself, and have not changed since: they are the record that splitting it moved
  no byte.
- A create's snapshot holds its **movement block alone**, written by the pair's builder. The values
  after it belong to other tests.
- A pair with no snapshot fails and leaves a `.received.txt`. Read it before renaming it.
- **A changed snapshot means the client or the legacy server is being sent different bytes.** Accept
  one only for a change you meant, and say in the commit which scenarios moved and why.
- Adding a scenario adds lines to every pair's snapshot, so the matrix is how you regenerate them.
  Check that the diff only adds:
  `diff --strip-trailing-cr <old> <new> | grep -c '^<'` is `0`.

## Writing a scenario

- Build the bytes with `LegacyMovementWire` / `ModernMovementWire`, not with a production writer. A
  fixture written by the production writer agrees with the production reader by construction.
- Give each optional part of the block one scenario with it and one without.
- A combination that trips a sanitising rule logs at Error. Keep those to a handful of named
  scenarios.
- A create must not reach a clock: give a transport a non-zero path timer, or `ServerTime` is read
  from the wall clock and the snapshot is not reproducible. `Creates_AreDeterministic` catches it.
