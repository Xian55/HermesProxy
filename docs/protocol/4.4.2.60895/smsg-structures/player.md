# Player — server packet layouts, 4.4.2.60895

### SMSG_PLAYER_BOUND (0x4e0000)

- Modern: 5111808 (0x4e0000) · 4.3.4: 9494 (0x2516) · Area: Player
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_FAILED_PLAYER_CONDITION (0x4e0002)

- Modern: 5111810 (0x4e0002) · 4.3.4: 6564 (0x19a4) · Area: Player
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_GM_REQUEST_PLAYER_INFO (0x4e0003)

- Modern: 5111811 (0x4e0003) · 4.3.4: — · Area: Player
- Layout: `{ u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 6 · (1 unused)

### SMSG_DISPLAY_PLAYER_CHOICE (0x4e0004)

- Modern: 5111812 (0x4e0004) · 4.3.4: — · Area: Player
- Layout: `{ { u32*2 guid u32*3 u8 u64 u8*3 loop[ { u32 u16 u32*5 u8 u32 u8*7 opt[ { u32*6 u64 u32*5 loop[ { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 ] loop[ { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 ] loop[ { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 ] loop[ { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 ] } ] bytes bytes bytes bytes bytes bytes opt[ u32 ] } ] bytes bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 31: 8 · 8 · 1 · 1 · 1 · (5 unused)

### SMSG_PLAYER_CHOICE_DISPLAY_ERROR (0x4e0005)

- Modern: 5111813 (0x4e0005) · 4.3.4: — · Area: Player
- Layout: `{ struct }`

### SMSG_PLAYER_CHOICE_CLEAR (0x4e0006)

- Modern: 5111814 (0x4e0006) · 4.3.4: — · Area: Player
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_INVALIDATE_PLAYER (0x4e0007)

- Modern: 5111815 (0x4e0007) · 4.3.4: 25381 (0x6325) · Area: Player
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_REPORT_PVP_PLAYER_AFK_RESULT (0x4e0009)

- Modern: 5111817 (0x4e0009) · 4.3.4: — · Area: Player
- Layout: `{ guid u8*3 }`
- Size: 3 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 8 · 8 · 8

### SMSG_QUERY_PLAYER_NAME_BY_COMMUNITY_ID_RESPONSE (0x4e000a)

- Modern: 5111818 (0x4e000a) · 4.3.4: — · Area: Player
- Layout: `{ u8 guid u64 opt[ { u8*6 bytes bytes bytes bytes bytes guid guid guid u64 u32 u8*5 u32 bytes } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 11: 1 · 6 · 7 · 7 · 7 · 7 · 7 · (6 unused); byte 35: 8 · 8 · 8 · 8 · 8

### SMSG_SET_PLAYER_DECLINED_NAMES_RESULT (0x4e000b)

- Modern: 5111819 (0x4e000b) · 4.3.4: 11045 (0x2b25) · Area: Player
- Layout: `{ u32 guid }`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_CHANGE_PLAYER_DIFFICULTY_RESULT (0x4e000c)

- Modern: 5111820 (0x4e000c) · 4.3.4: — · Area: Player
- Layout: `{ u8 opt[ u64 ] opt[ u32*2 ] opt[ u32 ] opt[ guid ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · 1 · (3 unused)

### SMSG_GM_PLAYER_INFO (0x4e000d)

- Modern: 5111821 (0x4e000d) · 4.3.4: 18965 (0x4a15) · Area: Player
- Layout: `{ { guid u32*12 u8*5 bytes bytes bytes bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 6 · 7 · 11; byte 53: 11 · (5 unused)

### SMSG_PLAYER_SKINNED (0x4e000e)

- Modern: 5111822 (0x4e000e) · 4.3.4: 278 (0x116) · Area: Player
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_VIGNETTE_UPDATE (0x4e0010)

- Modern: 5111824 (0x4e0010) · 4.3.4: — · Area: Player
- Layout: `{ u8 u32 { u32*2 loop[ guid ] loop[ u32*3 guid u32*5 u16*2 ] } { u32*2 loop[ guid ] loop[ u32*3 guid u32*5 u16*2 ] } loop[ guid ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · (6 unused)

### SMSG_PLAYER_IS_ADVENTURE_MAP_POI_VALID (0x4e0011)

- Modern: 5111825 (0x4e0011) · 4.3.4: — · Area: Player
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_PLAYER_CONDITION_RESULT (0x4e0012)

- Modern: 5111826 (0x4e0012) · 4.3.4: — · Area: Player
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_PLAYER_TUTORIAL_UNHIGHLIGHT_SPELL (0x4e0014)

- Modern: 5111828 (0x4e0014) · 4.3.4: — · Area: Player
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_PLAYER_TUTORIAL_HIGHLIGHT_SPELL (0x4e0015)

- Modern: 5111829 (0x4e0015) · 4.3.4: — · Area: Player
- Layout: `{ u32 u8 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 7 · (1 unused)

### SMSG_WORLD_QUEST_UPDATE_RESPONSE (0x4e0017)

- Modern: 5111831 (0x4e0017) · 4.3.4: — · Area: Player
- Layout: `{ u32 loop[ { u64 u32*4 } ] }`

### SMSG_AREA_POI_UPDATE_RESPONSE (0x4e0018)

- Modern: 5111832 (0x4e0018) · 4.3.4: — · Area: Player
- Layout: `{ u32 loop[ { u64 u32*4 } ] }`

### SMSG_PLAYER_BONUS_ROLL_FAILED (0x4e0020)

- Modern: 5111840 (0x4e0020) · 4.3.4: — · Area: Player
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_PLAYER_SHOW_UI_EVENT_TOAST (0x4e0023)

- Modern: 5111843 (0x4e0023) · 4.3.4: — · Area: Player
- Layout: `{ struct }`

### SMSG_QUERY_PLAYER_NAMES_RESPONSE (0x4e0025)

- Modern: 5111845 (0x4e0025) · 4.3.4: — · Area: Player
- Layout: `{ u32 loop[ { u8 guid u8 opt[ { u8*6 bytes bytes bytes bytes bytes guid guid guid u64 u32 u8*5 u32 bytes } ] opt[ u32 guid u8 bytes ] } ] }`

### SMSG_PLAYER_BATTLEFIELD_AUTO_QUEUE (0x4e0026)

- Modern: 5111846 (0x4e0026) · 4.3.4: — · Area: Player
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_PLAYER_WORLD_PVP_QUEUE (0x4e0027)

- Modern: 5111847 (0x4e0027) · 4.3.4: — · Area: Player
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_PLAYER_SHOW_GENERIC_WIDGET_DISPLAY (0x4e0028)

- Modern: 5111848 (0x4e0028) · 4.3.4: — · Area: Player
- Layout: `{ struct }`

### SMSG_PLAYER_SHOW_PARTY_POSE_UI (0x4e0029)

- Modern: 5111849 (0x4e0029) · 4.3.4: — · Area: Player
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_PLAYER_SHOW_ARROW_CALLOUT (0x4e002a)

- Modern: 5111850 (0x4e002a) · 4.3.4: — · Area: Player
- Layout: `{ struct }`

### SMSG_PLAYER_HIDE_ARROW_CALLOUT (0x4e002b)

- Modern: 5111851 (0x4e002b) · 4.3.4: — · Area: Player
- Layout: `{ struct }`

### SMSG_PLAYER_ACKNOWLEDGE_ARROW_CALLOUT (0x4e002c)

- Modern: 5111852 (0x4e002c) · 4.3.4: — · Area: Player
- Layout: `{ struct }`

### SMSG_PLAYER_END_OF_MATCH_DETAILS (0x4e002e)

- Modern: 5111854 (0x4e002e) · 4.3.4: — · Area: Player
- Layout: `{ u8 u32*3 }`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
