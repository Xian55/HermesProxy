# LFG — server packet layouts, 4.4.2.60895

### SMSG_LFG_JOIN_RESULT (0x490000)

- Modern: 4784128 (0x490000) · 4.3.4: 14518 (0x38b6) · Area: LFG
- Layout: `{ { { guid u32*2 u64 u8 } u8*2 u32*2 loop[ { u8 u32 opt[ guid ] loop[ u32*5 ] } ] loop[ u8 ] loop[ { bytes } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused) · 8 · 8

### SMSG_LFG_LIST_JOIN_RESULT (0x490001)

- Modern: 4784129 (0x490001) · 4.3.4: — · Area: LFG
- Layout: `{ { guid u32*2 u64 u8 } u8*2 }`
- Size: 19 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused) · 8 · 8

### SMSG_LFG_LIST_SEARCH_RESULTS (0x490002)

- Modern: 4784130 (0x490002) · 4.3.4: — · Area: LFG
- Layout: `{ u16 u32 loop[ { { guid u32*2 u64 u8 } u32 u8 guid guid guid guid guid u32*8 u64 u8 guid { loop[ u32 u8 ] } u32 loop[ guid ] loop[ guid ] loop[ guid ] { u8*6 { u32*3 loop[ u32*4 u8 ] } u32*2 loop[ u32 ] bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u8 ] } u8 { u32*3 loop[ u32*4 u8 ] } loop[ { guid u8*3 u32 u8*2 } ] } ] }`

### SMSG_LFG_LIST_SEARCH_STATUS (0x490003)

- Modern: 4784131 (0x490003) · 4.3.4: — · Area: LFG
- Layout: `{ { guid u32*2 u64 u8 } u8*2 }`
- Size: 19 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused) · 8 · 1 · (7 unused)

### SMSG_LFG_QUEUE_STATUS (0x490004)

- Modern: 4784132 (0x490004) · 4.3.4: 30900 (0x78b4) · Area: LFG
- Layout: `{ { guid u32*2 u64 u8 } u32*3 loop[ u32 u8 ] u32 }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused)

### SMSG_LFG_ROLE_CHECK_UPDATE (0x490005)

- Modern: 4784133 (0x490005) · 4.3.4: 822 (0x336) · Area: LFG
- Layout: `{ { u8*2 u32*4 loop[ u32 ] loop[ u64 ] u8 loop[ guid u8*3 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8; byte 18: 1 · 1 · (6 unused)

### SMSG_LFG_READY_CHECK_UPDATE (0x490006)

- Modern: 4784134 (0x490006) · 4.3.4: — · Area: LFG
- Layout: `{ u8*2 u32*2 loop[ u64 ] u8 loop[ guid u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 8 · 8; byte 10: 1 · (7 unused)

### SMSG_LFG_UPDATE_STATUS (0x490008)

- Modern: 4784136 (0x490008) · 4.3.4: 12708 (0x31a4) · Area: LFG
- Layout: `{ { { guid u32*2 u64 u8 } u8*2 u32 u8 u32*2 loop[ u32 ] loop[ guid ] u8 } }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused) · 8 · 8; byte 34: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### SMSG_LFG_INSTANCE_SHUTDOWN_COUNTDOWN (0x490009)

- Modern: 4784137 (0x490009) · 4.3.4: — · Area: LFG
- Layout: `{ { guid u32*2 u64 u8 } u32 }`
- Size: 21 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused)

### SMSG_LFG_LIST_UPDATE_STATUS (0x49000a)

- Modern: 4784138 (0x49000a) · 4.3.4: — · Area: LFG
- Layout: `{ { guid u32*2 u64 u8 } u64 u8 { u8*6 { u32*3 loop[ u32*4 u8 ] } u32*2 loop[ u32 ] bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u8 ] } u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused); byte 27: 8 · 5 · 10 · 11; byte 31: 8 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (5 unused); byte 54: 1 · (7 unused)

### SMSG_LFG_LIST_UPDATE_EXPIRATION (0x49000b)

- Modern: 4784139 (0x49000b) · 4.3.4: — · Area: LFG
- Layout: `{ { guid u32*2 u64 u8 } u64 u8 }`
- Size: 26 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused)

### SMSG_LFG_LIST_APPLICATION_STATUS_UPDATE (0x49000c)

- Modern: 4784140 (0x49000c) · 4.3.4: — · Area: LFG
- Layout: `{ { guid u32*2 u64 u8 } u64 u8*2 { guid u32*2 u64 u8 } u8 }`
- Size: 45 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused); byte 27: 8 · 8; byte 47: 1 · (7 unused) · 4 · (4 unused)

### SMSG_LFG_LIST_APPLY_TO_GROUP_RESULT (0x49000d)

- Modern: 4784141 (0x49000d) · 4.3.4: — · Area: LFG
- Layout: `{ { guid u32*2 u64 u8 } u64 u8*2 { guid u32*2 u64 u8 } u8 { { guid u32*2 u64 u8 } u32 u8 guid guid guid guid guid u32*8 u64 u8 guid { loop[ u32 u8 ] } u32 loop[ guid ] loop[ guid ] loop[ guid ] { u8*6 { u32*3 loop[ u32*4 u8 ] } u32*2 loop[ u32 ] bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u8 ] } u8 { u32*3 loop[ u32*4 u8 ] } loop[ { guid u8*3 u32 u8*2 } ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused); byte 27: 8 · 8; byte 47: 1 · (7 unused) · 4 · (4 unused); byte 67: 1 · (7 unused); byte 175: 5 · 10 · 11; byte 178: 8 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (5 unused); byte 201: 1 · (7 unused)

### SMSG_LFG_LIST_UPDATE_BLACKLIST (0x49000e)

- Modern: 4784142 (0x49000e) · 4.3.4: — · Area: LFG
- Layout: `{ { u32 loop[ u32*2 ] } }`

### SMSG_LFG_LIST_APPLICANT_LIST_UPDATE (0x49000f)

- Modern: 4784143 (0x49000f) · 4.3.4: — · Area: LFG
- Layout: `{ { guid u32*2 u64 u8 } u32*2 loop[ { { guid u32*2 u64 u8 } guid u32 u8*2 loop[ guid u32*3 u8*2 { u32*2 } u32 { u32 { loop[ u32 u8 ] } u32*2 loop[ u32*2 ] { u32*3 loop[ u32*4 u8 ] } } ] bytes } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused)

### SMSG_LFG_LIST_SEARCH_RESULTS_UPDATE (0x490010)

- Modern: 4784144 (0x490010) · 4.3.4: — · Area: LFG
- Layout: `{ u32 loop[ { { guid u32*2 u64 u8 } u32*2 u8*3 { u8*6 { u32*3 loop[ u32*4 u8 ] } u32*2 loop[ u32 ] bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u8 ] } opt[ guid ] opt[ u32 ] opt[ u32 ] opt[ guid ] opt[ guid ] opt[ guid ] opt[ guid ] loop[ { guid u8*3 u32 u8*2 } ] } ] }`

### SMSG_LFG_PROPOSAL_UPDATE (0x490011)

- Modern: 4784145 (0x490011) · 4.3.4: 32166 (0x7da6) · Area: LFG
- Layout: `{ { { guid u32*2 u64 u8 } u64 u32*2 u8 u32*3 u8*2 loop[ u8*2 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused); byte 48: 8 · 1 · 1 · 1 · (5 unused)

### SMSG_SET_DF_FAST_LAUNCH_RESULT (0x490012)

- Modern: 4784146 (0x490012) · 4.3.4: 13750 (0x35b6) · Area: LFG
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_LFG_SLOT_INVALID (0x490014)

- Modern: 4784148 (0x490014) · 4.3.4: 21685 (0x54b5) · Area: LFG
- Layout: `{ struct }`
- Struct fields the client uses (12 bytes): +0: u32, +4: u32, +8: u32

### SMSG_OPEN_LFG_DUNGEON_FINDER (0x490015)

- Modern: 4784149 (0x490015) · 4.3.4: 11319 (0x2c37) · Area: LFG
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_LFG_TELEPORT_DENIED (0x490016)

- Modern: 4784150 (0x490016) · 4.3.4: 3604 (0xe14) · Area: LFG
- Layout: `{ u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · (4 unused)

### SMSG_LFG_DISABLED (0x490017)

- Modern: 4784151 (0x490017) · 4.3.4: 2069 (0x815) · Area: LFG
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_LFG_OFFER_CONTINUE (0x490018)

- Modern: 4784152 (0x490018) · 4.3.4: 27431 (0x6b27) · Area: LFG
- Layout: `{ struct }`
- Struct fields the client uses (3 bytes): +0: 3 bytes

### SMSG_LFG_BOOT_PLAYER (0x490019)

- Modern: 4784153 (0x490019) · 4.3.4: — · Area: LFG
- Layout: `{ { u8*2 guid u32*4 bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 9 · (3 unused)

### SMSG_LFG_PARTY_INFO (0x49001a)

- Modern: 4784154 (0x49001a) · 4.3.4: 8997 (0x2325) · Area: LFG
- Layout: `{ u32 loop[ { u8 u32 opt[ guid ] loop[ u32*5 ] } ] }`

### SMSG_LFG_PLAYER_INFO (0x49001b)

- Modern: 4784155 (0x49001b) · 4.3.4: 19254 (0x4b36) · Area: LFG
- Layout: `{ u32 { u8 u32 opt[ guid ] loop[ u32*5 ] } loop[ { u32*16 u8 { u8 u32*5 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } loop[ { u8 u32*5 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_LFG_PLAYER_REWARD (0x49001c)

- Modern: 4784156 (0x49001c) · 4.3.4: 26676 (0x6834) · Area: LFG
- Layout: `{ u32*5 loop[ { u8 opt[ { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } ] u32*2 opt[ u32 ] } ] }`

### SMSG_ROLE_CHOSEN (0x49001d)

- Modern: 4784157 (0x49001d) · 4.3.4: — · Area: LFG
- Layout: `{ guid u8*2 }`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 8 · 1 · (7 unused)

### SMSG_LFG_READY_CHECK_RESULT (0x49001e)

- Modern: 4784158 (0x49001e) · 4.3.4: — · Area: LFG
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### SMSG_LFG_EXPAND_SEARCH_PROMPT (0x49001f)

- Modern: 4784159 (0x49001f) · 4.3.4: — · Area: LFG
- Layout: `{ { guid u32*2 u64 u8 } }`
- Size: 17 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused)

### SMSG_LFG_JOIN_LOBBY_MATCHMAKER_QUEUE (0x490020)

- Modern: 4784160 (0x490020) · 4.3.4: — · Area: LFG
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)
