---
name: parse-pkt
description: Parse a WoW .pkt sniff capture into a readable _parsed.txt using WowPacketParser
user-invocable: true
---

# Parse PKT

Run WowPacketParser against a `.pkt` sniff capture and produce a sibling `_parsed.txt` with the decoded packet stream.

## Usage

`/parse-pkt <path-to-file.pkt>`

- Single positional argument: the `.pkt` to parse.
- Relative paths are resolved against the current working directory.
- Output: `<input-without-ext>_parsed.txt` written next to the input file.

Example:

```
/parse-pkt HermesProxy\PacketsLog\modern_42597_1775252290.pkt
```

Produces `HermesProxy\PacketsLog\modern_42597_1775252290_parsed.txt`.

## One-time setup

The skill locates `WowPacketParser.exe` via the `HERMES_TOOLS_WPP` environment variable. Set it once at user scope from PowerShell:

```powershell
[Environment]::SetEnvironmentVariable(
  'HERMES_TOOLS_WPP',
  '<path-to>\WowPacketParser.exe',
  'User')
```

Replace `<path-to>` with the absolute path to your local WowPacketParser `Release` build directory.

Reopen any existing PowerShell session afterwards. Verify with `$env:HERMES_TOOLS_WPP` (should print the path) and `Test-Path $env:HERMES_TOOLS_WPP` (should print `True`).

## What the skill does

When invoked, follow these steps in order. Stop and report on the first failure — do not invoke the exe if any precondition fails.

1. **Check env var.** Read `$env:HERMES_TOOLS_WPP`. If empty or `Test-Path` is false, surface the *env-var-unset* or *exe-missing* error from the table below and stop.
2. **Resolve input.** Resolve the argument to an absolute path. If the file does not exist, surface *input-not-found*. If the extension is not `.pkt` (case-insensitive), surface *bad-extension*.
3. **Compute output path.** `expected = [IO.Path]::ChangeExtension($abs, $null) + '_parsed.txt'`. If that file already exists, mention in the final report that it was overwritten.
4. **Invoke WowPacketParser.** Run from the exe's own directory so any sibling `dbc/` or `cache/DBCache.bin` are picked up:
   ```powershell
   $exe = $env:HERMES_TOOLS_WPP
   Push-Location (Split-Path $exe)
   try { & $exe $abs } finally { Pop-Location }
   ```
   Pass the WPP stdout/stderr through to the user.
5. **Verify output.** Confirm `expected` exists and is non-empty. If not, surface *parse-failed* with the WPP exit code and last lines of stderr.
6. **Report.** On success, print one line: the absolute path of the produced `_parsed.txt` plus its size and mtime.

## Error reference

| Failure | Message to surface |
|---|---|
| env-var-unset | `HERMES_TOOLS_WPP is not set. Run the setup command in this skill's docs, then reopen the shell.` |
| exe-missing | `HERMES_TOOLS_WPP points to '<path>' but no file exists there. Update the env var or rebuild WowPacketParser.` |
| input-not-found | `Input .pkt not found: '<path>'.` |
| bad-extension | `Expected a .pkt file, got '<ext>'.` |
| parse-failed | `WowPacketParser exited with code <n> and produced no '<expected>'. Last stderr lines: ...` |

## Notes

- Output naming is hardcoded by WPP (`SniffFile.cs`): `<input>_parsed.txt` lands next to the input regardless of cwd, so `Push-Location` to the exe's folder is safe.
- DBC-aware parsing only kicks in if `dbc/` and `cache/DBCache.bin` exist next to the exe. Their absence is not a failure — WPP just produces a leaner parse.
- This skill parses one file per invocation. For batches, call it repeatedly.
