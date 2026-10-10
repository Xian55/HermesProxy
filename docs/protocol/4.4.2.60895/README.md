# Cataclysm Classic 4.4.2.60895 protocol reference

Packet layouts of the 4.4.2.60895 client, opcode by opcode. The layouts are the client's: what it reads
from each server packet and what it writes in each client packet. They are not taken from an
emulator's source or from a sniff parser.

| File | Contents |
|---|---|
| [smsg-opcodes.md](smsg-opcodes.md) | All 1,344 server-to-client opcodes: number, 4.3.4 counterpart, area, layout kind |
| [smsg-structures/](smsg-structures/README.md) | Per server packet: layout, size, bit fields, fixed-struct fields; one file per area |
| [cmsg-opcodes.md](cmsg-opcodes.md) | All 713 client-to-server opcodes: number, 4.3.4 counterpart, layout kind |
| [cmsg-structures.md](cmsg-structures.md) | Per client packet: layout, size, bit fields |

HermesProxy does not serve this client, so unlike the other builds there is no HermesProxy column.

Opcode numbers carry their group in the upper bits: server opcodes start at 0x3b0000 (general),
0x400000 (chat), 0x4c0000 (movement) and so on; client opcodes lie in groups from 0x2e0000 to
0x3a0000. Of the 1,344 server opcodes, 901 have a handler in the client and 443 are ignored by it.

Names follow WowPacketParser's 4.4.2 opcode table; the numbers and layouts come from the client
itself. A row named "—" is an opcode the client has but that table does not name (446 server
opcodes, most of them ignored by the client; 9 client opcodes). The 4.3.4 column is the original
Cataclysm number of the same name, where there is one.

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
a packet whose every value is zero (no optional part present, no loop run). `byte 35: 11 · 11 · 5 · 7 ·
12 · ...` means: byte 35 of the packet starts an 11-bit field, then another 11-bit field, a 5-bit one,
and so on. A field can continue into the next byte. `?` marks a bit whose value could not be followed.
Bit groups that only appear inside optional parts are not listed.

**Struct fields** (fixed-size packets with no per-field reads) are the offsets and widths the client
actually uses. Fields it ignores are not listed; a load of 8 or 16 bytes may be several fields read
together.

The layout kind "ignored by the client" in the opcode list means the client has no handler for that
server opcode at all.
