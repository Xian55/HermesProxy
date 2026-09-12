#!/usr/bin/env python3
"""Diff a converted handler body against the original it was ported from.

The codec equivalence tests prove a packet's *bytes* are read the same. They say nothing about
whether the handler still *behaves* the same — and handler bodies have no oracle, because moving
one deletes the original. That gap shipped a real regression: CMSG_CHAT_MESSAGE_EMOTE was ported
with lang=0 instead of Language.Common, dropping a deliberate cMaNGOS workaround whose seven-line
comment predicted the exact failure ("unknown language" notification). The codec was byte-perfect;
the handler was not.

So: move handler bodies verbatim, then run this before playtesting.

    python verify-handler-port.py \
        --original-ref HEAD \
        --original HermesProxy/World/Server/PacketHandlers/ChatHandler.cs \
        --ported   HermesProxy/World/Server/Systems/ChatSystem.cs \
        HandleChatMessageEmote HandleChatMessageWhisper ...

Exit code is the number of mismatches, so it can gate a commit.

Normalisation covers only the rewrites the conversion *requires* — `ctx.` prefixes, and the opcode
arriving as a parameter instead of being read back off the packet. Everything else is a real
difference and is reported.
"""
import argparse
import re
import subprocess
import sys


def extract_body(text: str, method: str) -> str | None:
    """The braced body of `method`, by brace matching rather than indentation."""
    m = re.search(r'\b' + re.escape(method) + r'\s*\([^)]*\)\s*\n?\s*\{', text)
    if not m:
        return None

    i = text.index('{', m.start())
    depth, j = 0, i
    while j < len(text):
        if text[j] == '{':
            depth += 1
        elif text[j] == '}':
            depth -= 1
            if depth == 0:
                return text[i + 1:j]
        j += 1
    return None


def normalise(body: str) -> list[str]:
    # Required by the conversion itself:
    body = body.replace('ctx.GetSession()', 'GetSession()')
    body = body.replace('ctx.SendPacketToServer', 'SendPacketToServer')
    body = body.replace('ctx.SendPacketToClient', 'SendPacketToClient')
    body = body.replace('ctx.SendPacket(', 'SendPacket(')
    body = body.replace('ctx.GameState', 'GetSession().GameState')
    body = body.replace('Session.GameState', 'GetSession().GameState')

    # Shape B: the opcode is a parameter now rather than a call on the packet, and a data-only
    # packet has no GetOpcode() either. Collapse all three spellings to one token — specific
    # patterns first, or the general one corrupts the replacements the others just made.
    body = re.sub(r'\w+\.GetUniversalOpcode\(\)', 'OPCODESOURCE', body)
    body = re.sub(r'\w+\.GetOpcode\(\)', 'OPCODESOURCE', body)
    body = re.sub(r'\bopcode\b', 'OPCODESOURCE', body)

    body = re.sub(r'//.*', '', body)                      # comments
    body = re.sub(r'/\*.*?\*/', '', body, flags=re.S)
    return [s.strip() for s in re.sub(r'\s+', ' ', body).strip().split(';') if s.strip()]


def read_original(ref: str, path: str) -> str:
    return subprocess.run(
        ['git', 'show', f'{ref}:{path}'],
        capture_output=True, text=True, encoding='utf-8', check=True).stdout


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument('--original-ref', default='HEAD')
    ap.add_argument('--original', required=True, help='path of the pre-conversion handler file')
    ap.add_argument('--ported', required=True, help='path of the new system file')
    ap.add_argument('methods', nargs='+')
    args = ap.parse_args()

    original = read_original(args.original_ref, args.original)
    ported = open(args.ported, encoding='utf-8-sig').read()

    mismatches = 0
    for method in args.methods:
        a_raw, b_raw = extract_body(original, method), extract_body(ported, method)
        if a_raw is None or b_raw is None:
            where = 'original' if a_raw is None else 'ported'
            print(f'  ??  {method}: not found in {where}')
            mismatches += 1
            continue

        a, b = normalise(a_raw), normalise(b_raw)
        if a == b:
            print(f'  ok  {method}')
            continue

        mismatches += 1
        print(f'  !=  {method}')
        import difflib
        for line in difflib.unified_diff(a, b, 'original', 'ported', lineterm='', n=1):
            if line.startswith(('---', '+++', '@@')):
                continue
            print(f'        {line}')

    print(f'\n{mismatches} mismatch(es)')
    return mismatches


if __name__ == '__main__':
    sys.exit(main())
