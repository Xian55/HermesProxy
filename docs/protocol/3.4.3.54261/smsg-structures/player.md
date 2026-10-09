# Player — server packet layouts, 3.4.3.54261

### SMSG_PLAYER_BOUND (0x2ff8)

- Modern: 12280 (0x2ff8) · 3.3.5a: 344 (0x158) · Area: Player
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `PlayerBound` — matches

### — (0x2ffa)

- Modern: 12282 (0x2ffa) · 3.3.5a: — · Area: Player
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x2ffb)

- Modern: 12283 (0x2ffb) · 3.3.5a: — · Area: Player
- Layout: `u8 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 6 · (1 unused)

### — (0x2ffc)

- Modern: 12284 (0x2ffc) · 3.3.5a: — · Area: Player
- Layout: `{ u32*2 guid u32*3 u8 u64 u8*3 loop[ { u32 u16 u32*5 u8 u32 u8*6 bits(7) opt[ u8 ] opt[ u8 ] opt[ { u32*6 u64 u32*2 u32 u32 u32 loop[ { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 ] loop[ { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 ] loop[ { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 ] loop[ { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 ] } ] bytes bytes bytes bytes bytes bytes opt[ u32 ] } ] bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 31: 8 · 8 · 1 · 1 · 1 · (5 unused)

### — (0x2ffd)

- Modern: 12285 (0x2ffd) · 3.3.5a: — · Area: Player
- Layout: `struct`

### — (0x2ffe)

- Modern: 12286 (0x2ffe) · 3.3.5a: — · Area: Player
- Layout: `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_INVALIDATE_PLAYER (0x2fff)

- Modern: 12287 (0x2fff) · 3.3.5a: 796 (0x31c) · Area: Player
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InvalidatePlayer` — matches

### — (0x3001)

- Modern: 12289 (0x3001) · 3.3.5a: — · Area: Player
- Layout: `guid u8*3`
- Size: 3 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 8 · 8 · 8

### — (0x3002)

- Modern: 12290 (0x3002) · 3.3.5a: — · Area: Player
- Layout: `u8 guid u64 opt[ { u8*6 bytes bytes bytes bytes bytes guid guid guid u64 u32 u8*5 bytes } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 11: 1 · 6 · 7 · 7 · 7 · 7 · 7 · (6 unused); byte 35: 8 · 8 · 8 · 8 · 8

### SMSG_SET_PLAYER_DECLINED_NAMES_RESULT (0x3003)

- Modern: 12291 (0x3003) · 3.3.5a: 1050 (0x41a) · Area: Player
- Layout: `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `SetPlayerDeclinedNamesResult` — matches

### — (0x3004)

- Modern: 12292 (0x3004) · 3.3.5a: — · Area: Player
- Layout: `u8 opt[ u64 ] opt[ u32*2 ] opt[ u32 ] opt[ guid ]`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · 1 · (3 unused)

### — (0x3005)

- Modern: 12293 (0x3005) · 3.3.5a: — · Area: Player
- Layout: `{ guid u32*12 u8*5 bytes bytes bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 6 · 7 · 11; byte 53: 11 · (5 unused)

### SMSG_PLAYER_SKINNED (0x3006)

- Modern: 12294 (0x3006) · 3.3.5a: 700 (0x2bc) · Area: Player
- Layout: `u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
- HermesProxy: `PlayerSkinned` — matches

### — (0x3008)

- Modern: 12296 (0x3008) · 3.3.5a: — · Area: Player
- Layout: `u8 u32 { u32 u32 loop[ guid ] loop[ u32*3 guid u32*4 ] } { u32 u32 loop[ guid ] loop[ u32*3 guid u32*4 ] } loop[ guid ]`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · (6 unused)

### — (0x3009)

- Modern: 12297 (0x3009) · 3.3.5a: — · Area: Player
- Layout: `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### — (0x300a)

- Modern: 12298 (0x300a) · 3.3.5a: — · Area: Player
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### — (0x300c)

- Modern: 12300 (0x300c) · 3.3.5a: — · Area: Player
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### — (0x300d)

- Modern: 12301 (0x300d) · 3.3.5a: — · Area: Player
- Layout: `u32 u8 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 7 · (1 unused)

### — (0x300f)

- Modern: 12303 (0x300f) · 3.3.5a: — · Area: Player
- Layout: `u32 loop[ u64 u32*4 ]`

### — (0x3010)

- Modern: 12304 (0x3010) · 3.3.5a: — · Area: Player
- Layout: `u32 loop[ u64 u32*4 ]`

### — (0x3016)

- Modern: 12310 (0x3016) · 3.3.5a: — · Area: Player
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### — (0x3019)

- Modern: 12313 (0x3019) · 3.3.5a: — · Area: Player
- Layout: `struct`

### SMSG_QUERY_PLAYER_NAMES_RESPONSE (0x301b)

- Modern: 12315 (0x301b) · 3.3.5a: — · Area: Player
- Layout: `{ u32 loop[ u8 guid u8 opt[ { u8*6 bytes bytes bytes bytes bytes guid guid guid u64 u32 u8*5 bytes } ] opt[ u32 guid u8 bytes ] ] }`

### — (0x301c)

- Modern: 12316 (0x301c) · 3.3.5a: — · Area: Player
- Layout: `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### — (0x301d)

- Modern: 12317 (0x301d) · 3.3.5a: — · Area: Player
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x301e)

- Modern: 12318 (0x301e) · 3.3.5a: — · Area: Player
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x301f)

- Modern: 12319 (0x301f) · 3.3.5a: — · Area: Player
- Layout: `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### — (0x3020)

- Modern: 12320 (0x3020) · 3.3.5a: — · Area: Player
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x3021)

- Modern: 12321 (0x3021) · 3.3.5a: — · Area: Player
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x3022)

- Modern: 12322 (0x3022) · 3.3.5a: — · Area: Player
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32
