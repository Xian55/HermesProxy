#!/usr/bin/env python3
"""Extract ClientPacket classes verbatim into frozen oracle copies.

The equivalence tests compare each new codec against the Read() body it replaced. Those oracles
have to be the *original* code — hand-transcribing them risks a typo that makes the test agree
with a bug, which is worse than having no test at all. This lifts them mechanically instead.

Run it BEFORE converting a packet, while the class still exists:

    python freeze-oracles.py QueryQuestInfo QueryCreature ... >> FrozenClientPackets.g.cs

It rewrites only what it must: drop the ClientPacket base and its constructor, and turn
`public override void Read()` into `public void Read(WorldPacket p)` with `_worldPacket` renamed
to `p`. Field declarations and the body — including initializers, which a positional record struct
cannot express — are copied unchanged.
"""
import re
import sys
from pathlib import Path

PACKETS_DIR = Path(__file__).resolve().parents[4] / "HermesProxy" / "World" / "Server" / "Packets"


def find_class(name: str):
    pattern = re.compile(
        r"^(?:public |internal )?(?:sealed )?class " + re.escape(name) + r" : ClientPacket\s*\{",
        re.MULTILINE,
    )
    for path in sorted(PACKETS_DIR.glob("*.cs")):
        text = path.read_text(encoding="utf-8-sig")
        m = pattern.search(text)
        if not m:
            continue
        # Walk braces from the opening one so nested blocks don't end the class early.
        depth, i = 0, m.end() - 1
        while i < len(text):
            if text[i] == "{":
                depth += 1
            elif text[i] == "}":
                depth -= 1
                if depth == 0:
                    return path.name, text[m.start():i + 1]
            i += 1
    return None, None


def freeze(name: str, body: str, source: str) -> str:
    inner = body[body.index("{") + 1:body.rindex("}")]

    # Drop the constructor; the oracle is constructed with no arguments.
    inner = re.sub(
        r"\n\s*public " + re.escape(name) + r"\(WorldPacket packet\) : base\(packet\) \{ \}\n",
        "\n",
        inner,
    )
    inner = inner.replace("public override void Read()", "public void Read(WorldPacket p)")
    inner = inner.replace("_worldPacket", "p")

    lines = [ln for ln in inner.strip("\n").split("\n")]
    indented = "\n".join(("    " + ln) if ln.strip() else "" for ln in lines)

    return (
        f"    /// Frozen verbatim from <c>{source}</c>.\n"
        f"    internal sealed class {name}\n"
        f"    {{\n{indented}\n    }}\n"
    )


def main() -> int:
    names = sys.argv[1:]
    if not names:
        print(__doc__, file=sys.stderr)
        return 2

    out, missing = [], []
    for name in names:
        source, body = find_class(name)
        if body is None:
            missing.append(name)
            continue
        out.append(freeze(name, body, source))

    if missing:
        print(f"// NOT FOUND: {', '.join(missing)}", file=sys.stderr)

    print("\n".join(out))
    return 1 if missing else 0


if __name__ == "__main__":
    sys.exit(main())
