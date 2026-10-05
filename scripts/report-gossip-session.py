#!/usr/bin/env python3
"""What a playtest session did with NPC greetings, read from its two sniffs and its log.

The proxy turns a greeting the legacy server sends as text into a BroadcastText id for the
modern client (World/BroadcastTextRegistry.cs). Whether that worked is spread over three files
a session leaves under HermesProxy/bin/<config>/:

    PacketsLog/modern_*_<stamp>_2.pkt   SMSG_QUERY_NPC_TEXT_RESPONSE: the ids handed out, all
                                        eight per NPC text, whichever one the client then shows
    PacketsLog/legacy_*_<stamp>_1.pkt   SMSG_NPC_TEXT_UPDATE: the texts the server sent
                                        CMSG_MESSAGECHAT: the "G<n>:<text>" lines that
                                        scripts/drive-gossip.ps1 makes the client say
    Logs/hermes-<stamp>.log             the ids the client asked for, and the replies the
                                        proxy held while it fetched an NPC text itself

Usage
-----
    python scripts/report-gossip-session.py 20261005_214118
    python scripts/report-gossip-session.py 20261005_214118 20261005_214318   # compare runs
    python scripts/report-gossip-session.py --config Debug <stamp>

The stamp is the one in the session's log name, hermes-<stamp>.log.

Reading the report
------------------
    npc text 764 -> 0x46951C0F 0x6228B575
        The ids for that NPC text's variants. An id from 0x40000000 up is derived from the text;
        a plain number is a row of CSV/BroadcastTexts{N}.csv. Two sessions that list the same
        ids for the same NPC text are the proof that an id does not depend on arrival order,
        and the last section says so for every NPC text the sessions share.

    client NPC text queries: 0   replies held for a server fetch: 2
        The client kept the NPC texts from an earlier session and asked a freshly started
        proxy for their rows; the proxy fetched the texts from the server before answering.

    shown '...' -> matches a server text
        What the client displayed is a text the server sent this session, compared up to the
        first $-token because the client fills those in ($c becomes "druid"). When the server
        did not resend the text, because the client had it cached, there is nothing to compare
        against and the line says so rather than guessing.

The client shows one variant of an NPC text per session and keeps to it, so compare sessions on
the id lists, not on which text happened to be shown.
"""

from __future__ import annotations

import argparse
import re
import struct
import sys
import time
from dataclasses import dataclass, field
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent

# The same value on the 1.14, 2.5 and 3.4.3 clients.
SMSG_QUERY_NPC_TEXT_RESPONSE = 10518
# Legacy opcodes, the same from 1.12 to 3.3.5a.
CMSG_MESSAGECHAT = 149
SMSG_NPC_TEXT_UPDATE = 384

DERIVED_ID_BASE = 0x40000000
SNIFF_HEADER = 47
FROM_CLIENT = 0x00


def records(path: Path):
    """Yield (direction, hh:mm:ss, body) for each packet of a HermesProxy .pkt capture."""
    data = path.read_bytes()
    off = SNIFF_HEADER
    while off + 13 <= len(data):
        direction = data[off]
        unixtime, _tick, size = struct.unpack_from("<IiI", data, off + 1)
        body = data[off + 13:off + 13 + size]
        off += 13 + size
        yield direction, time.strftime("%H:%M:%S", time.localtime(unixtime)), body


def cstring(body: bytes, off: int) -> tuple[str, int]:
    end = body.index(0, off)
    return body[off:end].decode("utf-8", "replace"), end + 1


def show(entry: int) -> str:
    return f"0x{entry:08X}" if entry >= DERIVED_ID_BASE else str(entry)


def squash(text: str) -> str:
    return re.sub(r"\s+", " ", text).strip()


@dataclass
class Session:
    stamp: str
    ids: dict[int, list[int]] = field(default_factory=dict)      # npc text id -> its BroadcastText ids
    said: list[tuple[str, str]] = field(default_factory=list)    # (time, "G<n>:<text>")
    server_texts: set[str] = field(default_factory=set)          # each up to its first $-token
    asked: list[int] = field(default_factory=list)               # ids the client requested
    held: int = 0
    client_queries: int = 0


def one(pattern: str, folder: Path) -> Path:
    found = sorted(folder.glob(pattern))
    if not found:
        sys.exit(f"no {pattern} under {folder}")
    return found[-1]


def read_session(stamp: str, bin_dir: Path) -> Session:
    session = Session(stamp)

    for direction, _at, body in records(one(f"modern_*_{stamp}_2.pkt", bin_dir / "PacketsLog")):
        # u16 opcode, u32 text id, 1 byte of bits, i32 size, 8 floats, 8 ids
        if direction == FROM_CLIENT or len(body) < 2 + 4 + 1 + 4 + 32 + 32:
            continue
        if struct.unpack_from("<H", body, 0)[0] == SMSG_QUERY_NPC_TEXT_RESPONSE:
            text_id = struct.unpack_from("<I", body, 2)[0]
            session.ids[text_id] = [i for i in struct.unpack_from("<8I", body, 2 + 4 + 1 + 4 + 32) if i]

    for direction, at, body in records(one(f"legacy_*_{stamp}_1.pkt", bin_dir / "PacketsLog")):
        if direction == FROM_CLIENT:
            # u32 opcode, u32 type, u32 language, text
            if len(body) > 12 and struct.unpack_from("<I", body, 0)[0] == CMSG_MESSAGECHAT:
                text = body[12:body.index(0, 12)].decode("utf-8", "replace")
                if re.match(r"G\d+:", text):
                    session.said.append((at, text))
        elif len(body) > 12 and struct.unpack_from("<H", body, 0)[0] == SMSG_NPC_TEXT_UPDATE:
            # A legacy server record carries the opcode twice, so the payload starts at 4:
            # u32 text id, then 8 x (float, male, female, u32 language, 3 x (u32, u32)).
            off = 8
            try:
                for _ in range(8):
                    off += 4
                    male, off = cstring(body, off)
                    female, off = cstring(body, off)
                    off += 4 + 24
                    for text in (male, female):
                        prefix = squash(text.split("$")[0])
                        if len(prefix) >= 10:
                            session.server_texts.add(prefix)
            except (ValueError, struct.error):
                pass  # a masked entry stops after the id

    log = (bin_dir / "Logs" / f"hermes-{stamp}.log").read_text(encoding="utf-8", errors="replace")
    session.asked = [int(m) for m in re.findall(r"DB_QUERY_BULK requested \(BroadcastText\) #(\d+)", log)]
    session.held = len(re.findall(r'client held kind="Event" event="NpcText"', log))
    session.client_queries = log.count('Received opcode "CMSG_QUERY_NPC_TEXT"')
    return session


def verdict(shown: str, session: Session) -> str:
    # drive-gossip.ps1 cuts the line at 200 bytes, which can land inside a character and
    # leaves the shown text shorter than the server's.
    shown = shown.rstrip("�")
    if not shown or shown == "nil":
        return "EMPTY"
    if "Clear your cache" in shown:
        return "CLEAR YOUR CACHE"
    for prefix in session.server_texts:
        if shown.startswith(prefix) or (len(shown) >= 10 and prefix.startswith(shown)):
            return "matches a server text"
    return "server did not resend this text (client cache); not checked"


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("stamps", nargs="+", metavar="stamp", help="session stamp, as in hermes-<stamp>.log")
    parser.add_argument("--config", default="Release", help="build configuration the session ran from (default: %(default)s)")
    parser.add_argument("--bin", type=Path, help="folder holding Logs/ and PacketsLog/, instead of HermesProxy/bin/<config>")
    args = parser.parse_args()
    bin_dir = args.bin or REPO_ROOT / "HermesProxy" / "bin" / args.config

    sessions = [read_session(stamp, bin_dir) for stamp in args.stamps]
    for session in sessions:
        derived = sum(1 for entry in session.asked if entry >= DERIVED_ID_BASE)
        print(f"== {session.stamp}")
        for text_id, entries in session.ids.items():
            print(f"   npc text {text_id:<6} -> {' '.join(show(e) for e in entries)}")
        print(f"   client asked for: {' '.join(show(e) for e in session.asked) or 'nothing'}")
        print(
            f"   client NPC text queries: {session.client_queries}   "
            f"replies held for a server fetch: {session.held}   derived ids asked: {derived}"
        )
        for at, text in session.said:
            print(f"   {at} shown {text[:70]!r} -> {verdict(squash(text.split(':', 1)[1]), session)}")

    if len(sessions) > 1:
        first, *rest = sessions
        print("== same npc text, same ids?")
        for other in rest:
            shared = sorted(first.ids.keys() & other.ids.keys())
            same = sum(1 for text_id in shared if first.ids[text_id] == other.ids[text_id])
            print(f"   {first.stamp} vs {other.stamp}: {same} of {len(shared)} shared npc texts carry identical ids")
            for text_id in shared:
                if first.ids[text_id] != other.ids[text_id]:
                    print(f"      npc text {text_id}: {first.ids[text_id]} != {other.ids[text_id]}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
