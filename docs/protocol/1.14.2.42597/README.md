# Classic Era 1.14.2.42597 protocol reference

Packet layouts of the 1.14.2.42597 client, opcode by opcode, and how HermesProxy's writers and codecs
compare. The layouts are the client's: what it reads from each server packet and what it writes in
each client packet. They are not taken from an emulator's source or from a sniff parser.

| File | Contents |
|---|---|
| [smsg-opcodes.md](smsg-opcodes.md) | All 1,287 server-to-client opcodes: number, 1.12.1 counterpart, area, layout kind, HermesProxy status |
| [smsg-structures/](smsg-structures/README.md) | Per server packet: layout, size, bit fields, fixed-struct fields, HermesProxy writer; one file per area |
| [cmsg-opcodes.md](cmsg-opcodes.md) | All 1,275 client-to-server opcodes: number, 1.12.1 counterpart, layout kind, HermesProxy status |
| [cmsg-structures.md](cmsg-structures.md) | Per client packet: layout, size, bit fields, HermesProxy codec |

HermesProxy serves this client with its `V2_5_3_41750` opcode table. Every client-to-server number in
that table is written by this client under that number. The comparison assumes a 1.12.1 server behind
the proxy.

The differences found are not yet triaged: a "differs" below is what the comparison reports, not a
confirmed defect. Two kinds are known to need care. The client-to-server movement block of 1.14.0 is
compared byte by byte as different while its bit fields agree; and a few writers build their bytes
elsewhere, so they are not checked.

## Reading a layout

```
u8 u16 u32 u64     an integer of that many bytes, little-endian
f32 f64            a float / double
guid               a packed GUID: two mask bytes, then one byte per set bit
cstring            a zero-terminated string
bytes              a run of bytes whose length came earlier (a string or a blob)
bits(N)            N bits of a bit field; bit fields are packed most significant bit first
u8 next to bits    a byte of bit fields: see "Bit fields" for how its bits are split
flush              (client packets) the pending bits are padded to a whole byte
struct             a fixed-size packet taken as a whole: see "Struct fields"
{ ... }            a sub-structure (an item instance, a movement block, ...)
loop[ ... ]        repeated; the count came earlier
opt[ ... ]         present only when a flag or condition says so
alt[ ... ]         the other branch of a condition
x*N                N of x in a row
```

Order is the order on the wire. Counts, conditions and string lengths come from earlier fields, usually
a bit field.

**Size** is the byte count of a packet with no optional part, with every packed GUID counted as 0
(add 2 for each empty one).

**Bit fields** are the widths of the fields in each byte of bit fields, in the order they arrive, for
a packet whose every value is zero (no optional part present, no loop run). `byte 13: 1 · 2 · 1 · 1 · (3
unused)` means: byte 13 of the packet holds a 1-bit field, a 2-bit field, two 1-bit fields, then 3
padding bits. A field can continue into the next byte (`byte 33: 11 · 11 · ...`). `?` marks a bit whose
value could not be followed. Bit groups that only appear inside optional parts are not listed.

**Struct fields** (fixed-size packets with no per-field reads) are the offsets and widths the client
actually uses. Fields it ignores are not listed; a load of 8 or 16 bytes may be several fields read
together.

## The HermesProxy column

| Value | Meaning |
|---|---|
| matches | every path through HermesProxy's writer or codec agrees with the client's layout, bit fields included where compared |
| differs | the client's layout and HermesProxy's writer or codec disagree; not yet triaged |
| client takes a plain struct | HermesProxy writes the fields; the client takes the packet as a fixed struct by offset |
| reads the first part | the codec stops early; the rest of the client's packet is ignored |
| not checked | the writer builds its bytes elsewhere, so it could not be compared |
| no handler | the client sends it, HermesProxy has no handler, and it is dropped |
| — | HermesProxy neither sends nor handles it |
| ignored by the client (opcode list) | the client has no handler for this server opcode at all |
