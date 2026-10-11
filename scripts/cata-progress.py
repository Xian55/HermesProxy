#!/usr/bin/env python3
"""Generate the Cataclysm progress page: the 4.4.2 client side and the 4.3.4 legacy side.

Client side: every opcode whose layout the 4.4.2 client reads differently from the 3.4.3 one
(docs/protocol, the clients' own readers), against whether HermesProxy's writer or codec for it
has a 4.4.2 shape yet, and whether 4.4.2 sessions sent it (modern .pkt captures).

Legacy side: every opcode HermesProxy reads from, or writes to, a legacy server gets one row,
built from three sources and nothing typed by hand:

    code   is the opcode in the 4.3.4 table, and does its handler (SMSG) or the method that
           builds it (CMSG) carry a 4.3.4 branch: IsCataLegacy, a *Cata method or codec
    WPP    did the layout change in 4.x: a parser in WowPacketParser's 4.3.4 module, or a
           Cataclysm / V4 branch in the shared parser (optional, see --wpp)
    logs   what the proxy saw in sessions against a 4.3.4 server: the most recent session
           that touched the opcode, and whether it went through cleanly

A clean pass in a log only means nothing threw: a field read from the wrong offset can still
come out wrong without an exception. Rows whose layout changed and that have no 4.3.4 branch
are therefore listed as suspect even when the logs are clean.

Usage
-----
    python scripts/cata-progress.py --wpp <WowPacketParser checkout>
    python scripts/cata-progress.py --wpp <...> --logs HermesProxy/bin/Release/Logs --out docs/cata-progress.md

The WowPacketParser path can also come from HERMES_WPP_DIR.
"""
from __future__ import annotations

import argparse
import glob
import os
import re
from collections import defaultdict
from dataclasses import dataclass, field
from datetime import datetime
from difflib import SequenceMatcher
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
HP = REPO / 'HermesProxy'
LEGACY_BUILD = 'V4_3_4_15595'

# What marks code as 4.3.4-aware: the legacy-expansion gate, a *Cata reader or writer, a
# masked GUID, or a version check against a 4.x build (code that already tracked a 4.x change).
CATA_MARKERS = re.compile(r'IsCataLegacy|LegacyHasItemQuery|\w+Cata\s*\(|MaskedGuid\.|ClientVersionBuild\.V4_\d')


def enum_names(path: Path) -> dict[str, int]:
    text = path.read_text(encoding='utf-8-sig')
    return {m.group(1): int(m.group(2), 0)
            for m in re.finditer(r'^\s*([A-Z][A-Z0-9_]+)\s*=\s*(0x[0-9A-Fa-f]+|\d+)', text, re.M)}


NOT_METHODS = {'if', 'for', 'foreach', 'while', 'switch', 'using', 'lock', 'catch', 'fixed', 'return',
               'new', 'await', 'nameof', 'typeof', 'sizeof', 'when', 'base', 'this'}
DECL = re.compile(r'^[ \t]+(?:(?:public|private|internal|protected|static|override|virtual|async|unsafe|readonly|'
                  r'sealed|extern|partial)\s+)*[\w<>\[\],?.]+\s+(\w+)\s*\(', re.M)


def _match(text: str, i: int, open_c: str, close_c: str) -> int:
    """Index of the bracket closing the one at i, skipping strings and comments."""
    depth = 0
    n = len(text)
    while i < n:
        c = text[i]
        if c == '/' and text.startswith('//', i):
            i = text.find('\n', i)
            if i == -1:
                return n
            continue
        if c == '/' and text.startswith('/*', i):
            i = text.find('*/', i) + 2
            continue
        if c == '"':
            i += 1
            while i < n and text[i] != '"':
                i += 2 if text[i] == '\\' else 1
        elif c == "'":
            i += 1
            while i < n and text[i] != "'":
                i += 2 if text[i] == '\\' else 1
        elif c == open_c:
            depth += 1
        elif c == close_c:
            depth -= 1
            if depth == 0:
                return i
        i += 1
    return n


def method_bodies(paths: list[str]) -> dict[str, list[tuple[str, str]]]:
    """Every method's name -> [(file, attribute lines + signature + body)]."""
    out: dict[str, list[tuple[str, str]]] = defaultdict(list)
    for f in paths:
        text = Path(f).read_text(encoding='utf-8-sig', errors='ignore')
        rel = os.path.relpath(f, REPO).replace('\\', '/')
        pos = 0
        while (m := DECL.search(text, pos)):
            pos = m.end()
            name = m.group(1)
            if name in NOT_METHODS:
                continue
            close = _match(text, m.end() - 1, '(', ')')
            rest = text[close + 1:close + 400].lstrip()
            rest = re.sub(r'^where[^{=]*', '', rest).lstrip()
            if rest.startswith('{'):
                body_open = text.index('{', close)
                end = _match(text, body_open, '{', '}')
            elif rest.startswith('=>'):
                end = text.find(';', close)
            else:
                continue
            # attribute lines directly above the signature
            start = m.start()
            while True:
                prev = text.rfind('\n', 0, start - 1)
                line = text[prev + 1:start].strip()
                if line.startswith('['):
                    start = prev + 1
                else:
                    break
            out[name].append((rel, text[start:end + 1]))
            pos = end + 1
    return out


def ported(body: str, methods: dict[str, list[tuple[str, str]]], depth: int = 2) -> bool:
    """A 4.3.4 branch in the code that reads or writes this packet: the body itself, or a
    method it hands the packet to. Callees that never see the packet are not followed, or a
    handler would count as ported for an unrelated packet it happens to send."""
    if CATA_MARKERS.search(body):
        return True
    if depth == 0:
        return False
    for callee in set(re.findall(r'\b([A-Z]\w+)\s*\(\s*(?:ref\s+)?packet\b', body)):
        for _, callee_body in methods.get(callee, []):
            if ported(callee_body, methods, depth - 1):
                return True
    return False


def touched_files(rel: str, body: str, methods: dict[str, list[tuple[str, str]]], depth: int = 2) -> set[str]:
    """The file holding this code and the files of every method it hands the packet to."""
    files = {rel}
    if depth:
        for callee in set(re.findall(r'\b([A-Z]\w+)\s*\(\s*(?:ref\s+)?packet\b', body)):
            for callee_rel, callee_body in methods.get(callee, []):
                files |= touched_files(callee_rel, callee_body, methods, depth - 1)
    return files


def session_time(session: str) -> float:
    return datetime.strptime(session, '%Y%m%d_%H%M%S').timestamp()


def send_segment(body: str, opcode: str) -> str:
    """The code from `new WorldPacket(Opcode.X)` to the call that sends it."""
    start = body.find(f'new WorldPacket(Opcode.{opcode}')
    end = body.find('SendPacketToServer(', start)
    return body[start:end if end != -1 else len(body)]


def wpp_signals(wpp: Path | None) -> dict[str, str]:
    if wpp is None:
        return {}
    signals: dict[str, str] = {}

    def scan(paths, module):
        for f in paths:
            text = Path(f).read_text(encoding='utf-8-sig', errors='ignore')
            for m in re.finditer(r'\[Parser\(Opcode\.([A-Z0-9_]+)', text):
                op = m.group(1)
                body_start = text.find('{', m.end())
                nxt = text.find('[Parser(Opcode.', body_start)
                chunk = text[m.start(): nxt if nxt != -1 else len(text)]
                if module:
                    signals[op] = 'changed'
                elif signals.get(op) != 'changed':
                    signals[op] = 'changed' if re.search(r'V4_\d_\d|ClientType\.Cataclysm', chunk) else 'same'

    scan(glob.glob(str(wpp / 'WowPacketParser' / 'Parsing' / 'Parsers' / '*.cs')), module=False)
    scan(glob.glob(str(wpp / 'WowPacketParserModule.V4_3_4_15595' / 'Parsers' / '*.cs')), module=True)
    return signals


@dataclass
class Seen:
    session: str = ''
    count: int = 0
    errors: int = 0
    dropped: int = 0
    unhandled: int = 0
    sample: str = ''


@dataclass
class LogEvidence:
    sessions: list[str] = field(default_factory=list)
    smsg: dict[str, Seen] = field(default_factory=dict)       # legacy server -> proxy
    cmsg: dict[str, Seen] = field(default_factory=dict)       # proxy -> legacy server
    client_errors: dict[str, Seen] = field(default_factory=dict)  # 4.4.2 client side


RECV = re.compile(r'\| C P<S \| Received opcode "([A-Z0-9_]+)"')
SENT = re.compile(r'\| C P>S \| Sending opcode "([A-Z0-9_]+)"')
SMSG_EXC = re.compile(r'Unhandled exception in handler for ((?:SMSG|MSG)_[A-Z0-9_]+)')
CMSG_EXC = re.compile(r'C>P S \| Unhandled exception in handler for ((?:CMSG|MSG)_[A-Z0-9_]+)')
NO_LEGACY_MAP = re.compile(r'No ' + LEGACY_BUILD + r' opcode mapping for ([A-Z0-9_]+)')
NO_MODERN_MAP = re.compile(r'No V\d_\d_\d_\d+ opcode mapping for ([A-Z0-9_]+)')
NO_HANDLER = re.compile(r'C P<S \| No handler for opcode ([A-Z0-9_]+)')


def read_capture(path: Path, names: dict[int, list[str]]) -> tuple[dict[str, int], dict[str, int]]:
    """Opcode counts from a legacy .pkt capture: (from the server, to the server). The log only
    names the opcodes it logs, and it leaves out the busy ones; the capture has every packet."""
    data = path.read_bytes()
    smsg: dict[str, int] = defaultdict(int)
    cmsg: dict[str, int] = defaultdict(int)
    off = 47
    while off + 13 <= len(data):
        direction = data[off]
        size = int.from_bytes(data[off + 9:off + 13], 'little')
        body = data[off + 13:off + 13 + size]
        off += 13 + size
        if direction == 0xff and len(body) >= 2:
            target, op = smsg, int.from_bytes(body[:2], 'little')
        elif len(body) >= 4:
            target, op = cmsg, int.from_bytes(body[:4], 'little')
        else:
            continue
        for name in names.get(op, []):
            target[name] += 1
    return smsg, cmsg


def read_logs(dirs: list[Path], names: dict[int, list[str]]) -> LogEvidence:
    ev = LogEvidence()
    files = sorted({f for d in dirs for f in d.glob('hermes-*.log')}, key=lambda p: p.name)
    for f in files:
        with f.open(encoding='utf-8', errors='ignore') as fh:
            head = ''.join(next(fh, '') for _ in range(40))
            if f'Legacy (Server) Build: "{LEGACY_BUILD}"' not in head:
                continue
            session = f.stem.removeprefix('hermes-')
            ev.sessions.append(session)
            per_smsg: dict[str, Seen] = {}
            per_cmsg: dict[str, Seen] = {}
            per_client: dict[str, Seen] = {}
            fh.seek(0)
            for line in fh:
                if (m := RECV.search(line)):
                    per_smsg.setdefault(m.group(1), Seen(session)).count += 1
                elif (m := SENT.search(line)):
                    per_cmsg.setdefault(m.group(1), Seen(session)).count += 1
                elif (m := CMSG_EXC.search(line)):
                    s = per_client.setdefault(m.group(1), Seen(session)); s.errors += 1; s.sample = 'exception'
                elif (m := SMSG_EXC.search(line)):
                    s = per_smsg.setdefault(m.group(1), Seen(session)); s.errors += 1
                elif (m := NO_LEGACY_MAP.search(line)):
                    s = per_cmsg.setdefault(m.group(1), Seen(session)); s.dropped += 1
                elif (m := NO_MODERN_MAP.search(line)):
                    s = per_client.setdefault(m.group(1), Seen(session)); s.dropped += 1; s.sample = 'no 4.4.2 mapping'
                elif (m := NO_HANDLER.search(line)):
                    s = per_smsg.setdefault(m.group(1), Seen(session)); s.unhandled += 1
            for capture in sorted((f.parent.parent / 'PacketsLog').glob(f'legacy_*_{session}_*.pkt')):
                smsg_counts, cmsg_counts = read_capture(capture, names)
                for op, n in smsg_counts.items():
                    per_smsg.setdefault(op, Seen(session)).count = n
                for op, n in cmsg_counts.items():
                    per_cmsg.setdefault(op, Seen(session)).count = n
            # the most recent session that touched an opcode is the one that counts
            ev.smsg.update(per_smsg)
            ev.cmsg.update(per_cmsg)
            ev.client_errors.update(per_client)
    return ev


DOCS = REPO / 'docs' / 'protocol'


def doc_layouts(build_dir: Path) -> dict[str, tuple[str, str]]:
    """Opcode -> (layout, HermesProxy type) from one build's docs/protocol structures."""
    out: dict[str, tuple[str, str]] = {}
    files = list((build_dir / 'smsg-structures').glob('*.md')) + [build_dir / 'cmsg-structures.md']
    for f in files:
        if not f.exists():
            continue
        for block in re.split(r'^### ', f.read_text(encoding='utf-8'), flags=re.M)[1:]:
            name = block.split()[0]
            layout = re.search(r'^- Layout[^`]*`([^`]*)`', block, re.M)
            hp = re.search(r'^- HermesProxy: `(\w+)`', block, re.M)
            out[name] = (layout.group(1) if layout else '', hp.group(1) if hp else '')
    return out


def class_has_442_variant(type_name: str, sources: list[str], cache: dict[str, str]) -> bool:
    """A ServerPacketLayouts entry for 4.4.2 inside the class, or a 4.4.2 [PacketCodec]."""
    codec = re.compile(r'\[PacketCodec\(typeof\(' + re.escape(type_name) + r'\)[^\]]*AddedIn\s*=\s*ClientVersionBuild\.V4_4_2')
    decl = re.compile(r'class ' + re.escape(type_name) + r'\b[^{]*\{')
    for f in sources:
        text = cache.get(f)
        if text is None:
            text = cache[f] = Path(f).read_text(encoding='utf-8-sig', errors='ignore')
        if codec.search(text):
            return True
        m = decl.search(text)
        if m:
            end = _match(text, m.end() - 1, '{', '}')
            if 'V4_4_2_60895' in text[m.start():end]:
                return True
    return False


def modern_counts(log_dirs: list[Path]) -> dict[str, int]:
    """Opcode counts from every 4.4.2 client capture, whatever the backend."""
    table = enum_names(HP / 'World' / 'Enums' / 'V4_4_2_60895' / 'Opcode.cs')
    names: dict[int, list[str]] = defaultdict(list)
    for name, value in table.items():
        if value:
            names[value].append(name)
    counts: dict[str, int] = defaultdict(int)
    for d in log_dirs:
        for capture in (d.parent / 'PacketsLog').glob('modern_60895_*.pkt'):
            # PKT 3.1: a 66-byte header plus its optional block, then records of
            # (direction, connection, tick, extra length, data length), the extra, and the data,
            # which starts with the 4-byte opcode.
            data = capture.read_bytes()
            off = 66 + int.from_bytes(data[62:66], 'little')
            while off + 20 <= len(data):
                extra = int.from_bytes(data[off + 12:off + 16], 'little')
                size = int.from_bytes(data[off + 16:off + 20], 'little')
                body = data[off + 20 + extra:off + 20 + extra + size]
                off += 20 + extra + size
                if len(body) >= 4:
                    for name in names.get(int.from_bytes(body[:4], 'little'), []):
                        counts[name] += 1
    return counts


def normalise(layout: str) -> str:
    """Drop notation that does not change the bytes: grouping braces, and the optional mark on a
    string's bytes (the length already says whether there are any)."""
    layout = layout.replace('{', ' ').replace('}', ' ')
    layout = re.sub(r'opt\[\s*bytes\s*\]', 'bytes', layout)
    # u32*3 and u32 u32 u32 are the same bytes
    layout = re.sub(r'\b(\w+)\*(\d+)\b', lambda m: ' '.join([m.group(1)] * int(m.group(2))), layout)
    return ' '.join(layout.split())


# Differences someone checked and found harmless, with the reason. The only hand-kept part of
# the client side: a docs difference stays a todo until it has a 4.4.2 shape in HermesProxy or
# a line here.
REVIEWED_HUNKS = {
    'loop[ u32 [u8 → ] ] opt[': 'item modifiers swap to (type, value); HermesProxy writes none, '
                                'and a read takes the same five bytes',
    'u8 loop[ [ → u8] u32 u8': 'item modifiers, as above',
    'flush loop[ [ → u8] u32 u8': 'item modifiers, as above',
    'u32 u32 [ → u32 u32] u8 u8': 'SpellCastLogData gains two ints; HermesProxy never sends log data',
    'u8 loop[ [u32 → u8] u32 u32': 'SpellCastLogData power type; never sent, as above',
}
REVIEWED_OPCODES = {
    'SMSG_AURA_UPDATE': 'notation: both builds read a 1-bit and a 9-bit field first',
    'SMSG_RESURRECT_REQUEST': 'notation: 11-bit name length and two bits in both; HermesProxy '
                              'matches TrinityCore cata_classic',
    'SMSG_CHAT_SERVER_MESSAGE': 'notation: an 11-bit length in both',
    'SMSG_TRADE_STATUS': 'notation: HermesProxy matches TrinityCore cata_classic bit for bit',
    'SMSG_TRADE_UPDATED': 'notation, plus item modifiers (never written)',
    'SMSG_LFG_TELEPORT_DENIED': 'notation: four bits in one byte either way',
    'SMSG_RESUME_TOKEN': 'notation: two bits in one byte either way',
    'SMSG_SUSPEND_TOKEN': 'notation: two bits in one byte either way',
    'SMSG_PET_SPELLS_MESSAGE': 'same bytes: the 3.4.3 u16 is (CommandState, Flag 0), 4.4.2 reads '
                               'two bytes',
    'SMSG_PARTY_MEMBER_FULL_STATE': 'phase flags widen to u32; HermesProxy sends no phases',
    'SMSG_PARTY_MEMBER_PARTIAL_STATE': 'phase flags widen to u32; HermesProxy sends no phases',
    # Identical "Bit fields" lines in both docs: the difference is how the RE wrote the bits.
    'SMSG_CHANNEL_NOTIFY_LEFT': 'notation: bit fields 7 · 1 in both',
    'SMSG_GUILD_EVENT_MOTD': 'notation: bit fields 11 in both',
    'SMSG_GUILD_EVENT_PLAYER_JOINED': 'notation: bit fields 6 in both',
    'SMSG_GENERATE_RANDOM_CHARACTER_NAME_RESULT': 'notation: bit fields 1 · 6 in both',
    'SMSG_ARENA_TEAM_EVENT': 'notation: bit fields 8 · 9 · 9 · 9 in both',
    'SMSG_QUERY_PETITION_RESPONSE': 'notation: bit fields 1 in both',
    'SMSG_LFG_PLAYER_INFO': 'notation: bit fields 1 in both',
    'CMSG_PET_RENAME': 'notation: bit fields 8 · 1 in both',
    'SMSG_GUILD_EVENT_PRESENCE_CHANGE': '4.4.2 reads one bit fewer of the same byte (6 · 1 against '
                                        '6 · 1 · 1); the extra bit is ignored',
}
for _ack in ('COLLISION_HEIGHT', 'KNOCK_BACK', 'FEATHER_FALL', 'FORCE_ROOT', 'FORCE_UNROOT',
             'GRAVITY_DISABLE', 'GRAVITY_ENABLE', 'HOVER', 'SET_CAN_FLY', 'WATER_WALK'):
    REVIEWED_OPCODES[f'CMSG_MOVE_{_ack}_ACK' if _ack != 'COLLISION_HEIGHT' else 'CMSG_MOVE_SET_COLLISION_HEIGHT_ACK'] = \
        'notation: MovementInfo then the ack index in both'
for _speed in ('FLIGHT_BACK', 'FLIGHT', 'PITCH_RATE', 'RUN_BACK', 'RUN', 'SWIM_BACK', 'SWIM',
               'TURN_RATE', 'WALK'):
    _name = f'CMSG_MOVE_FORCE_{_speed}_SPEED_CHANGE_ACK' if 'RATE' not in _speed \
        else f'CMSG_MOVE_FORCE_{_speed}_CHANGE_ACK'
    REVIEWED_OPCODES[_name] = 'notation: MovementInfo, the ack index and the speed in both'


def reviewed(name: str, hunks: list[str]) -> str:
    """Why a docs difference is harmless, or '' while it is not known to be."""
    if name in REVIEWED_OPCODES:
        return REVIEWED_OPCODES[name]
    if hunks and all(h in REVIEWED_HUNKS for h in hunks):
        return '; '.join(dict.fromkeys(REVIEWED_HUNKS[h] for h in hunks))
    return ''


def changes(layout: str, new_layout: str) -> list[str]:
    """What changed between two layouts, as `before [old → new] after` hunks with two tokens of
    context. The same hunk in several packets is a nested structure they share."""
    a, b = normalise(layout).split(), normalise(new_layout).split()
    out: list[str] = []
    for tag, i1, i2, j1, j2 in SequenceMatcher(None, a, b, autojunk=False).get_opcodes():
        if tag == 'equal':
            continue
        hunk = f'{" ".join(a[max(0, i1 - 2):i1])} [{" ".join(a[i1:i2])} → {" ".join(b[j1:j2])}] {" ".join(a[i2:i2 + 2])}'
        if hunk not in out:
            out.append(hunk.strip())
    return out


def client_layout_section(log_dirs: list[Path], client_errors: dict) -> list[str]:
    old = doc_layouts(DOCS / '3.4.3.54261')
    new = doc_layouts(DOCS / '4.4.2.60895')
    if not old or not new:
        return []
    sources = glob.glob(str(HP / 'World' / '**' / '*.cs'), recursive=True)
    cache: dict[str, str] = {}
    seen = modern_counts(log_dirs)

    rows = []
    unresolved: list[str] = []
    for name, (layout, hp_type) in old.items():
        if name not in new or not hp_type or hp_type == 'EmptyClientPacket':
            continue
        new_layout = new[name][0]
        if not new_layout or normalise(new_layout) == normalise(layout):
            continue
        if 'struct' in layout or 'struct' in new_layout:
            unresolved.append(name)        # the RE left one side unresolved; nothing to compare
            continue
        has = class_has_442_variant(hp_type, sources, cache)
        rows.append((has, -seen.get(name, 0), name, hp_type, layout, new_layout, changes(layout, new_layout)))
    rows.sort()
    notes = {r[2]: reviewed(r[2], r[6]) for r in rows}
    todo = sum(1 for r in rows if not r[0] and not notes[r[2]])

    def icon(has: bool, name: str) -> str:
        return '✅' if has else ('🟰' if notes[name] else '⬜')

    by_hunk: dict[str, list[tuple[bool, str]]] = defaultdict(list)
    for has, _, name, _, _, _, hunks in rows:
        for hunk in hunks:
            by_hunk[hunk].append((has, name))
    shared = sorted(((h, ops) for h, ops in by_hunk.items() if len(ops) > 1),
                    key=lambda x: (-sum(1 for has, _ in x[1] if not has), -len(x[1]), x[0]))

    md = ['## 4.4.2 client side', '']
    if client_errors:
        md += ['### Errors in 4.3.4 sessions', '',
               'A CMSG reader that threw, or a translated SMSG with no 4.4.2 opcode.', '',
               '| Opcode | Problem | Count | Session |', '|---|---|---:|---|']
        for op, e in sorted(client_errors.items()):
            md.append(f'| `{op}` | {e.sample} | {e.errors + e.dropped} | {e.session} |')
        md.append('')
    if shared:
        md += ['### Changes several packets share', '',
               'The same change in more than one packet below: most likely a nested structure they all '
               'write, so one writer fix covers them. ⬜ packets have no 4.4.2 shape yet. Only a change '
               'that writes a non-empty part matters; an empty loop or an absent optional reads the same.', '',
               '| Change | Packets |', '|---|---|']
        for hunk, ops in shared:
            note = f' 🟰 {REVIEWED_HUNKS[hunk]}' if hunk in REVIEWED_HUNKS else ''
            md.append(f'| `{hunk}`{note} | ' + ', '.join(f'{icon(has, n)} `{n}`' for has, n in ops) + ' |')
        md.append('')
    md += ['### Layouts that changed from 3.4.3', '',
          f'Opcodes HermesProxy writes or reads whose 4.4.2 layout differs from 3.4.3 (docs/protocol, from the '
          f'clients\' own readers): {len(rows)}, of which {todo} have no 4.4.2 shape yet and no review '
          'that rules them out (🟰, the reason under the change; REVIEWED_* in the script). "Change" is the '
          'difference as `before [3.4.3 → 4.4.2] after`. "Sent" counts the opcode in 4.4.2 client captures, '
          'either backend; the busiest gaps are first.', '',
          '| | Opcode | HP type | Change | Sent | 3.4.3 | 4.4.2 |', '|---|---|---|---|---:|---|---|']
    for has, sent, name, hp_type, layout, new_layout, hunks in rows:
        shown = '<br>'.join(f'`{h}`' for h in hunks[:4]) + (f'<br>… {len(hunks) - 4} more' if len(hunks) > 4 else '')
        if notes[name] and not has:
            shown += f'<br>🟰 {notes[name]}'
        md.append(f'| {icon(has, name)} | `{name}` | `{hp_type}` | {shown} | {-sent or ""} '
                  f'| `{layout}` | `{new_layout}` |')
    md.append('')
    if unresolved:
        md += [f'Not compared, one side unresolved in the docs ({len(unresolved)}): '
               + ', '.join(f'`{n}`' for n in sorted(unresolved)), '']
    return md


def status(direction: str, mapped: bool, is_ported: bool, layout: str, seen: Seen | None, changed_since: bool) -> str:
    if not mapped:
        return 'unmapped'
    if seen and changed_since:
        return 'retest'
    if seen and (seen.errors or seen.dropped):
        return 'failing'
    changed = layout == 'changed'
    if changed and not is_ported:
        return 'suspect' if seen and seen.count else 'todo'
    if seen and seen.count:
        return 'seen ok'
    return 'ported, unseen' if is_ported else 'unseen'


ORDER = ['failing', 'unmapped', 'todo', 'suspect', 'retest', 'ported, unseen', 'unseen', 'seen ok']
ICON = {'failing': '❌', 'unmapped': '🚫', 'todo': '⬜', 'suspect': '⚠️', 'retest': '🔁', 'ported, unseen': '🔧',
        'unseen': '·', 'seen ok': '✅'}


def main() -> None:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument('--wpp', type=Path, default=os.environ.get('HERMES_WPP_DIR'))
    ap.add_argument('--logs', type=Path, action='append')
    ap.add_argument('--out', type=Path, default=REPO / 'docs' / 'cata-progress.md')
    args = ap.parse_args()
    logs = args.logs or [p for p in (HP / 'bin' / 'Release' / 'Logs', HP / 'bin' / 'Debug' / 'Logs') if p.is_dir()]

    v335 = enum_names(HP / 'World' / 'Enums' / 'V3_3_5a_12340' / 'Opcode.cs')
    v434 = enum_names(HP / 'World' / 'Enums' / 'V4_3_4_15595' / 'Opcode.cs')
    sources = glob.glob(str(HP / 'World' / '**' / '*.cs'), recursive=True)
    methods = method_bodies(sources)
    wpp = wpp_signals(Path(args.wpp) if args.wpp else None)
    names: dict[int, list[str]] = defaultdict(list)
    for name, value in v434.items():
        names[value].append(name)
    ev = read_logs(logs, names)

    rows = []   # (dir, opcode, mapped, ported, layout, seen, where)
    smsg_handlers: dict[str, list[tuple[str, str]]] = defaultdict(list)
    cmsg_sites: dict[str, list[tuple[str, str]]] = defaultdict(list)
    for name, defs in methods.items():
        for rel, body in defs:
            for m in re.finditer(r'HandlesSmsg\(Opcode\.([A-Z0-9_]+)', body):
                if '/Client/' in rel:
                    smsg_handlers[m.group(1)].append((rel, body))
            for m in re.finditer(r'new WorldPacket\(Opcode\.((?:CMSG|MSG)_[A-Z0-9_]+)', body):
                cmsg_sites[m.group(1)].append((rel, body))

    for op, defs in smsg_handlers.items():
        if op not in v335:
            continue
        is_ported = any(ported(b, methods) for _, b in defs)
        files = set().union(*(touched_files(r, b, methods) for r, b in defs))
        rows.append(('S', op, op in v434, is_ported, wpp.get(op, '?'), ev.smsg.get(op), defs[0][0], files))
    # Movement CMSGs are built from a variable opcode, so no literal site names them; the ones
    # with a generated 4.3.4 sequence are written through LegacyMovementCata.
    sequences_file = HP / 'World' / 'Objects' / 'LegacyMovementSequencesCata.cs'
    sequenced = set(re.findall(r'\[Opcode\.((?:CMSG|MSG)_[A-Z0-9_]+)\]', sequences_file.read_text(encoding='utf-8-sig'))) \
        if sequences_file.exists() else set()
    movement_writer = os.path.relpath(HP / 'World' / 'Objects' / 'LegacyMovementCata.cs', REPO).replace(os.sep, '/')

    for op in set(cmsg_sites) | {o for o in ev.cmsg if o.startswith(('CMSG', 'MSG'))} | sequenced:
        if op not in v335:
            continue
        defs = cmsg_sites.get(op, [])
        # A sender inside a method named *Cata is the 4.3.4 version by construction.
        in_cata_method = any(re.search(r'\b\w+Cata\s*\(', b[:b.find('{')]) for _, b in defs)
        is_ported = op in sequenced or in_cata_method or any(ported(send_segment(b, op), methods) for _, b in defs)
        where = defs[0][0] if defs else (movement_writer if op in sequenced else '(variable opcode)')
        files = set().union(*(touched_files(r, send_segment(b, op), methods) for r, b in defs)) if defs else set()
        if op in sequenced:
            files |= {movement_writer, os.path.relpath(sequences_file, REPO).replace(os.sep, '/')}
        rows.append(('C', op, op in v434, is_ported, wpp.get(op, '?'), ev.cmsg.get(op), where, files))

    def changed_since(seen: Seen | None, files: set[str]) -> bool:
        if not seen or not files:
            return False
        return max((REPO / f).stat().st_mtime for f in files) > session_time(seen.session)

    graded = [(status(d, mp, po, lay, seen, changed_since(seen, files)), d, op, po, lay, seen, where)
              for d, op, mp, po, lay, seen, where, files in rows]
    graded.sort(key=lambda r: (ORDER.index(r[0]), r[1], r[2]))
    counts = defaultdict(int)
    for g in graded:
        counts[g[0]] += 1

    md = ['# Cataclysm — progress',
          '',
          '<!-- Generated by scripts/cata-progress.py. Do not edit by hand; re-run it. -->',
          '',
          f'{len(graded)} opcodes HermesProxy reads from or writes to the legacy server. '
          f'Logs: {len(ev.sessions)} session(s) against {LEGACY_BUILD}'
          + (f', latest {ev.sessions[-1]}' if ev.sessions else '') + '. '
          + ('WPP layout data included.' if wpp else 'No WPP checkout given: layout column is "?".'),
          '',
          '| Status | Count | Meaning |',
          '|---|---:|---|',
          f'| {ICON["failing"]} failing | {counts["failing"]} | the latest session that touched it threw or dropped it |',
          f'| {ICON["unmapped"]} unmapped | {counts["unmapped"]} | not in the 4.3.4 table: renamed, split or removed in 4.x |',
          f'| {ICON["todo"]} todo | {counts["todo"]} | layout changed in 4.x, no 4.3.4 branch, not seen yet |',
          f'| {ICON["suspect"]} suspect | {counts["suspect"]} | layout changed, no 4.3.4 branch, but passed without an error — likely misread |',
          f'| {ICON["retest"]} retest | {counts["retest"]} | its file changed after the latest session that exercised it |',
          f'| {ICON["ported, unseen"]} ported, unseen | {counts["ported, unseen"]} | has a 4.3.4 branch, waiting for a session to exercise it |',
          f'| {ICON["unseen"]} unseen | {counts["unseen"]} | same layout as 3.3.5a per WPP, not exercised yet |',
          f'| {ICON["seen ok"]} seen ok | {counts["seen ok"]} | ported or unchanged, and the latest session passed it cleanly |',
          '']

    md += client_layout_section(logs, ev.client_errors)

    md += ['## Legacy side (4.3.4 server)', '',
           '| | Dir | Opcode | 4.3.4 branch | WPP 4.x layout | Last session | Seen | Errors | HP file |',
           '|---|---|---|---|---|---|---:|---:|---|']
    for st, d, op, po, lay, seen, where in graded:
        last = seen.session if seen else ''
        n = seen.count if seen else ''
        err = (seen.errors + seen.dropped) if seen and (seen.errors or seen.dropped) else ''
        md.append(f'| {ICON[st]} | {d} | `{op}` | {"yes" if po else ""} | {lay} | {last} | {n} | {err} | {where} |')
    md.append('')

    args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_text('\n'.join(md), encoding='utf-8')
    print(f'wrote {os.path.relpath(args.out, REPO)}: ' + ', '.join(f'{k} {counts[k]}' for k in ORDER))


if __name__ == '__main__':
    main()
