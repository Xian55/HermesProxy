# Player — server packet layouts, 1.14.2.42597

### SMSG_PLAYER_BOUND (0x2ff8)

- Modern: 12280 (0x2ff8) · 1.12.1: 344 (0x158) · Area: Player
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `PlayerBound` — matches

### SMSG_FAILED_PLAYER_CONDITION (0x2ffa)

- Modern: 12282 (0x2ffa) · 1.12.1: — · Area: Player
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_GM_REQUEST_PLAYER_INFO (0x2ffb)

- Modern: 12283 (0x2ffb) · 1.12.1: — · Area: Player
- Layout: `{ u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 6 · (1 unused)

### SMSG_DISPLAY_PLAYER_CHOICE (0x2ffc)

- Modern: 12284 (0x2ffc) · 1.12.1: — · Area: Player
- Layout: `{ { u32*2 alt[ ] guid u32*2 u8 u64 u8*3 loop[ { u32 u16 u32*5 u8 u32 u8*7 opt[ { u32*6 u64 u32*2 u32 u32 u32 loop[ { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u32 ] loop[ { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u32 ] loop[ { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u32 ] loop[ { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u32 ] } ] bytes bytes bytes bytes bytes bytes opt[ u32 ] } ] bytes bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 27: 8 · 8 · 1 · 1 · 1 · (5 unused)

### SMSG_PLAYER_CHOICE_DISPLAY_ERROR (0x2ffd)

- Modern: 12285 (0x2ffd) · 1.12.1: — · Area: Player
- Layout: `{ struct }`

### SMSG_PLAYER_CHOICE_CLEAR (0x2ffe)

- Modern: 12286 (0x2ffe) · 1.12.1: — · Area: Player
- Layout: `{ struct }`

### SMSG_INVALIDATE_PLAYER (0x2fff)

- Modern: 12287 (0x2fff) · 1.12.1: 796 (0x31c) · Area: Player
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InvalidatePlayer` — matches

### SMSG_REPORT_PVP_PLAYER_AFK_RESULT (0x3001)

- Modern: 12289 (0x3001) · 1.12.1: — · Area: Player
- Layout: `{ guid u8*3 }`
- Size: 3 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 8 · 8 · 8

### SMSG_QUERY_PLAYER_NAME_RESPONSE (0x3002)

- Modern: 12290 (0x3002) · 1.12.1: 81 (0x51) · Area: Player
- Layout: `{ u8 guid opt[ { u8 loop[ opt[ u8 ] alt[ u8 ] ] loop[ bytes ] guid guid guid u64 u32 u8*5 bytes } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 3: 1 · 6 · 7 · 7 · 7 · 7 · 7 · (6 unused); byte 27: 8 · 8 · 8 · 8 · 8

### SMSG_QUERY_PLAYER_NAME_BY_COMMUNITY_ID_RESPONSE (0x3003)

- Modern: 12291 (0x3003) · 1.12.1: — · Area: Player
- Layout: `{ u8 guid u64 opt[ { u8 loop[ opt[ u8 ] alt[ u8 ] ] loop[ bytes ] guid guid guid u64 u32 u8*5 bytes } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 11: 1 · 6 · 7 · 7 · 7 · 7 · 7 · (6 unused); byte 35: 8 · 8 · 8 · 8 · 8

### SMSG_SET_PLAYER_DECLINED_NAMES_RESULT (0x3004)

- Modern: 12292 (0x3004) · 1.12.1: — · Area: Player
- Layout: `{ u32 guid }`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `SetPlayerDeclinedNamesResult` — matches

### SMSG_CHANGE_PLAYER_DIFFICULTY_RESULT (0x3005)

- Modern: 12293 (0x3005) · 1.12.1: — · Area: Player
- Layout: `{ u8 opt[ u64 ] opt[ u32*2 ] opt[ u32 ] opt[ guid ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · 1 · (3 unused)

### SMSG_GM_PLAYER_INFO (0x3006)

- Modern: 12294 (0x3006) · 1.12.1: 560 (0x230) · Area: Player
- Layout: `{ { guid u32*12 u8*5 bytes bytes bytes bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 6 · 7 · 11; byte 53: 11 · (5 unused)

### SMSG_PLAYER_SKINNED (0x3007)

- Modern: 12295 (0x3007) · 1.12.1: 700 (0x2bc) · Area: Player
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
- HermesProxy: `PlayerSkinned` — matches

### SMSG_PLAYER_TABARD_VENDOR_ACTIVATE (0x3008)

- Modern: 12296 (0x3008) · 1.12.1: — · Area: Player
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `PlayerTabardVendorActivate` — matches

### SMSG_VIGNETTE_UPDATE (0x3009)

- Modern: 12297 (0x3009) · 1.12.1: — · Area: Player
- Layout: `{ u8 { u32 loop[ guid ] } { u32 loop[ guid ] u32 loop[ u32*3 guid u32*4 ] } { u32 loop[ guid ] u32 loop[ u32*3 guid u32*4 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · (6 unused)

### SMSG_PLAYER_IS_ADVENTURE_MAP_POI_VALID (0x300a)

- Modern: 12298 (0x300a) · 1.12.1: — · Area: Player
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_PLAYER_CONDITION_RESULT (0x300b)

- Modern: 12299 (0x300b) · 1.12.1: — · Area: Player
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_PLAYER_TUTORIAL_UNHIGHLIGHT_SPELL (0x300d)

- Modern: 12301 (0x300d) · 1.12.1: — · Area: Player
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_PLAYER_TUTORIAL_HIGHLIGHT_SPELL (0x300e)

- Modern: 12302 (0x300e) · 1.12.1: — · Area: Player
- Layout: `{ u32 u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 7 · (1 unused)

### SMSG_WORLD_QUEST_UPDATE_RESPONSE (0x3010)

- Modern: 12304 (0x3010) · 1.12.1: — · Area: Player
- Layout: `{ u32 loop[ { u64 u32*4 } ] }`

### SMSG_AREA_POI_UPDATE_RESPONSE (0x3011)

- Modern: 12305 (0x3011) · 1.12.1: — · Area: Player
- Layout: `{ u32 loop[ { u64 u32*4 } ] }`

### SMSG_PLAYER_BONUS_ROLL_FAILED (0x3017)

- Modern: 12311 (0x3017) · 1.12.1: — · Area: Player
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_PLAYER_SHOW_UI_EVENT_TOAST (0x301a)

- Modern: 12314 (0x301a) · 1.12.1: — · Area: Player
- Layout: `{ struct }`

### SMSG_QUERY_PLAYER_NAMES_RESPONSE (0x301c)

- Modern: 12316 (0x301c) · 1.12.1: — · Area: Player
- Layout: `{ u32 loop[ { u8 guid u8 opt[ { u8 loop[ opt[ u8 ] alt[ u8 ] ] loop[ bytes ] guid guid guid u64 u32 u8*5 bytes } ] opt[ u32 guid u8 bytes ] } ] }`
