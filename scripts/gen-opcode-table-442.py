"""Complete the 4.4.2 opcode table from the client's own list.

usage: python -I scripts/gen-opcode-table-442.py [--check]

HermesProxy/World/Enums/V4_4_2_60895/Opcode.cs began as the 3.4.3 table with the 4.4.2 values,
so it only held the opcodes the proxy already handled for 3.4.3. The client has many more, and
a packet outside the table is logged as MSG_NULL_ACTION with a number and dropped: whoever
reads the log has to look the number up to learn that a button was pressed.

docs/protocol/4.4.2.60895 lists every opcode the client has. This script appends each named one
the table lacks, under the name the docs give it, in a block of its own at the end of the enum,
and adds the names the universal enum (World/Enums/Opcodes.cs) does not have yet.

What it leaves alone:
  - every member above the block: the values and the names chosen there stand. Where the docs
    know an opcode under another name (CMSG_TUTORIAL for CMSG_TUTORIAL_FLAG), the table's name
    wins, because the handlers are written against it;
  - the rows the docs have without a name.

The block is rewritten whole on every run, so running it twice changes nothing. --check changes
no file and exits 1 when a run would.
"""
import os
import re
import sys

REPO = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DOCS = os.path.join(REPO, 'docs', 'protocol', '4.4.2.60895')
TABLE = os.path.join(REPO, 'HermesProxy', 'World', 'Enums', 'V4_4_2_60895', 'Opcode.cs')
UNIVERSAL = os.path.join(REPO, 'HermesProxy', 'World', 'Enums', 'Opcodes.cs')

MARKER = "\t// The rest of the client's list, written by scripts/gen-opcode-table-442.py from docs/protocol."
ROW = re.compile(r'^\| (\w+) \| 0x([0-9a-f]+) \| (\d+) \|')
MEMBER = re.compile(r'^\s*(\w+) = (0x[0-9A-Fa-f]+|\d+)u?,')


def read(path):
    with open(path, encoding='utf-8-sig', newline='') as f:
        text = f.read()
    return text, ('\r\n' if '\r\n' in text else '\n')


def write(path, text):
    with open(path, 'w', encoding='utf-8-sig', newline='') as f:
        f.write(text)


def client_list():
    rows = {}
    for name in ('cmsg-opcodes.md', 'smsg-opcodes.md'):
        with open(os.path.join(DOCS, name), encoding='utf-8') as f:
            for line in f:
                m = ROW.match(line)
                if m:
                    rows[m.group(1)] = int(m.group(3))
    return rows


def complete_table(rows):
    text, newline = read(TABLE)
    lines = text.split(newline)
    close = max(i for i, line in enumerate(lines) if line.strip() == '}')
    start = lines.index(MARKER) if MARKER in lines else close
    kept = lines[:start]

    names, values = set(), set()
    for line in kept:
        m = MEMBER.match(line)
        if m:
            names.add(m.group(1))
            values.add(int(m.group(2), 0))

    added = sorted((name, value) for name, value in rows.items() if value not in values)
    clash = [name for name, _ in added if name in names]
    if clash:
        sys.exit('the docs give these names a value the table has under another one: ' + ', '.join(clash))

    block = [MARKER] + ['\t%s = 0x%Xu,' % row for row in added]
    return newline.join(kept + block + lines[close:]), text, [name for name, _ in added]


def complete_universal(wanted):
    text, newline = read(UNIVERSAL)
    lines = text.split(newline)
    start = next(i for i, line in enumerate(lines) if re.match(r'\s*public enum Opcode\b', line))
    end = next(i for i in range(start, len(lines)) if lines[i].strip() == '}')
    member = re.compile(r'^\s*([A-Z][A-Z0-9_]+)\s*(=\s*[^,]+)?,')

    def members():
        return [(i, member.match(lines[i]).group(1)) for i in range(start, end) if member.match(lines[i])]

    have = {name for _, name in members()}
    new = sorted(name for name in wanted if name not in have)
    for name in new:
        prefix = name.split('_', 1)[0] + '_'
        same = [(i, n) for i, n in members() if n.startswith(prefix)]
        after = [i for i, n in same if n > name]
        # in its place among the names of its kind, or after the last of them
        at = after[0] if after else same[-1][0] + 1
        lines.insert(at, '    %s,' % name)
        end += 1
    return newline.join(lines), text, new


def main():
    check = '--check' in sys.argv
    rows = client_list()
    table, table_before, added = complete_table(rows)
    universal, universal_before, new_names = complete_universal(added)

    changed = [path for path, now, before in ((TABLE, table, table_before), (UNIVERSAL, universal, universal_before))
               if now != before]
    print('client list: %d named opcodes; generated block: %d; new universal names: %d'
          % (len(rows), len(added), len(new_names)))
    if check:
        if changed:
            sys.exit('out of date: ' + ', '.join(os.path.relpath(p, REPO) for p in changed))
        return
    if table != table_before:
        write(TABLE, table)
    if universal != universal_before:
        write(UNIVERSAL, universal)
    for path in changed:
        print('wrote', os.path.relpath(path, REPO))


main()
