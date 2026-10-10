# Cache — server packet layouts, 4.4.2.60895

### SMSG_DB_REPLY (0x3f0000)

- Modern: 4128768 (0x3f0000) · 4.3.4: 14500 (0x38a4) · Area: Cache
- Layout: `u32*3 u8 u32 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 12: 3 · (5 unused)

### SMSG_AVAILABLE_HOTFIXES (0x3f0001)

- Modern: 4128769 (0x3f0001) · 4.3.4: — · Area: Cache
- Layout: `u32*2 loop[ u32*2 ]`

### SMSG_HOTFIX_MESSAGE (0x3f0002)

- Modern: 4128770 (0x3f0002) · 4.3.4: — · Area: Cache
- Layout: `u32 loop[ { u32*5 u8 } ] u32 bytes`

### SMSG_HOTFIX_CONNECT (0x3f0003)

- Modern: 4128771 (0x3f0003) · 4.3.4: — · Area: Cache
- Layout: `u32 loop[ { u32*5 u8 } ] u32 bytes`

### SMSG_REALM_QUERY_RESPONSE (0x3f0005)

- Modern: 4128773 (0x3f0005) · 4.3.4: — · Area: Cache
- Layout: `u32 u8 opt[ { u8*3 bytes bytes } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 8 · 1 · 1 · 8 · 8 · (6 unused)

### SMSG_QUERY_CREATURE_RESPONSE (0x3f0006)

- Modern: 4128774 (0x3f0006) · 4.3.4: 24612 (0x6024) · Area: Cache
- Layout: `u32 u8 opt[ { u8*4 loop[ opt[ u8 ] alt[ u8 ] bits(3) opt[ u8 ] alt[ u8 ] bits(3) ] loop[ { bytes } { bytes } ] loop[ u32 ] u32*8 loop[ u32*3 ] u32*12 { bytes } { bytes } { bytes } loop[ u32 ] loop[ u32 ] } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_QUERY_GAME_OBJECT_RESPONSE (0x3f0007)

- Modern: 4128775 (0x3f0007) · 4.3.4: 2325 (0x915) · Area: Cache
- Layout: `u32 guid u8 u32 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)

### SMSG_QUERY_NPC_TEXT_RESPONSE (0x3f0008)

- Modern: 4128776 (0x3f0008) · 4.3.4: 17462 (0x4436) · Area: Cache
- Layout: `u32 u8 u32 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_QUERY_PAGE_TEXT_RESPONSE (0x3f0009)

- Modern: 4128777 (0x3f0009) · 4.3.4: 11028 (0x2b14) · Area: Cache
- Layout: `u32 u8 opt[ u32 loop[ u32*3 u8*3 bytes ] ]`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_INVALIDATE_PAGE_TEXT (0x3f000a)

- Modern: 4128778 (0x3f000a) · 4.3.4: — · Area: Cache
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_QUERY_PET_NAME_RESPONSE (0x3f000b)

- Modern: 4128779 (0x3f000b) · 4.3.4: 19511 (0x4c37) · Area: Cache
- Layout: `{ guid u8 opt[ u8*5 bytes bytes bytes bytes bytes u64 bytes ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### SMSG_QUERY_BATTLE_PET_NAME_RESPONSE (0x3f000c)

- Modern: 4128780 (0x3f000c) · 4.3.4: — · Area: Cache
- Layout: `{ guid u32 u64 u8 opt[ u8*5 bytes bytes bytes bytes bytes bytes ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · (7 unused)

### SMSG_QUERY_PETITION_RESPONSE (0x3f000d)

- Modern: 4128781 (0x3f000d) · 4.3.4: — · Area: Cache
- Layout: `u32 u8 opt[ { u32 guid u32*7 u16 u32*5 u8*3 loop[ u8 ] loop[ bytes ] bytes bytes } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_CACHE_VERSION (0x3f000e)

- Modern: 4128782 (0x3f000e) · 4.3.4: 10036 (0x2734) · Area: Cache
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_CACHE_INFO (0x3f000f)

- Modern: 4128783 (0x3f000f) · 4.3.4: — · Area: Cache
- Layout: `u32 bits(6) loop[ bits(6) bits(6) bytes bytes ] bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 6 · (2 unused)

### SMSG_QUERY_ITEM_TEXT_RESPONSE (0x3f0010)

- Modern: 4128784 (0x3f0010) · 4.3.4: 10021 (0x2725) · Area: Cache
- Layout: `u8*2 bits(5) bytes guid`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused) · 13 · (3 unused)

### SMSG_TREASURE_PICKER_RESPONSE (0x3f0011)

- Modern: 4128785 (0x3f0011) · 4.3.4: — · Area: Cache
- Layout: `u32*2 { u32*2 u64 u32*2 loop[ u32*2 ] u8 loop[ { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 ] loop[ u32*2 u64 loop[ u32*2 ] u8 loop[ { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 ] ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 32: 1 · (7 unused)

### SMSG_QUERY_ARENA_TEAM_RESPONSE (0x3f0012)

- Modern: 4128786 (0x3f0012) · 4.3.4: — · Area: Cache
- Layout: `u32 u8 opt[ u32*7 u8 bytes ]`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
