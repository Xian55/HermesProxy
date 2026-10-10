# Combat — server packet layouts, 4.4.2.60895

### SMSG_BATTLEFIELD_STATUS_NEED_CONFIRMATION (0x410000)

- Modern: 4259840 (0x410000) · 4.3.4: 22944 (0x59a0) · Area: Combat
- Layout: `{ { guid u32*2 u64 u8 } u32 u8*3 u32 loop[ u64 ] u8 } u32*2 u8`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused); byte 23: 8 · 8 · 8; byte 30: 1 · 1 · (6 unused)

### SMSG_BATTLEFIELD_STATUS_ACTIVE (0x410001)

- Modern: 4259841 (0x410001) · 4.3.4: 29860 (0x74a4) · Area: Combat
- Layout: `{ { guid u32*2 u64 u8 } u32 u8*3 u32 loop[ u64 ] u8 } u32*3 u8`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused); byte 23: 8 · 8 · 8; byte 30: 1 · 1 · (6 unused); byte 43: 1 · 1 · 1 · (5 unused)

### SMSG_BATTLEFIELD_STATUS_QUEUED (0x410002)

- Modern: 4259842 (0x410002) · 4.3.4: 13729 (0x35a1) · Area: Combat
- Layout: `{ { guid u32*2 u64 u8 } u32 u8*3 u32 loop[ u64 ] u8 } u32*3 u8`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused); byte 23: 8 · 8 · 8; byte 30: 1 · 1 · (6 unused); byte 43: 1 · 1 · 1 · (5 unused)

### SMSG_BATTLEFIELD_STATUS_NONE (0x410003)

- Modern: 4259843 (0x410003) · 4.3.4: — · Area: Combat
- Layout: `{ guid u32*2 u64 u8 }`
- Size: 17 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused)

### SMSG_BATTLEFIELD_STATUS_FAILED (0x410004)

- Modern: 4259844 (0x410004) · 4.3.4: 29095 (0x71a7) · Area: Combat
- Layout: `{ guid u32*2 u64 u8 } u64 u32 guid`
- Size: 29 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused)

### SMSG_BATTLEFIELD_LIST (0x410005)

- Modern: 4259845 (0x410005) · 4.3.4: 29109 (0x71b5) · Area: Combat
- Layout: `guid u32*2 u8*2 u32 loop[ u32 ] u8`
- Bit fields (all-zero packet, widths in arrival order): byte 10: 8 · 8; byte 16: 1 · 1 · (6 unused)

### SMSG_BATTLEGROUND_PLAYER_POSITIONS (0x410006)

- Modern: 4259846 (0x410006) · 4.3.4: — · Area: Combat
- Layout: `u32 loop[ guid u32*2 u8*2 ]`

### SMSG_BATTLEGROUND_PLAYER_JOINED (0x410009)

- Modern: 4259849 (0x410009) · 4.3.4: 20656 (0x50b0) · Area: Combat
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_BATTLEGROUND_PLAYER_LEFT (0x41000a)

- Modern: 4259850 (0x41000a) · 4.3.4: 22950 (0x59a6) · Area: Combat
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_BATTLEFIELD_PORT_DENIED (0x41000b)

- Modern: 4259851 (0x41000b) · 4.3.4: 13731 (0x35a3) · Area: Combat
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_BATTLEGROUND_INFO_THROTTLED (0x41000c)

- Modern: 4259852 (0x41000c) · 4.3.4: 13490 (0x34b2) · Area: Combat
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_BATTLEFIELD_STATUS_WAIT_FOR_GROUPS (0x41000d)

- Modern: 4259853 (0x41000d) · 4.3.4: 30114 (0x75a2) · Area: Combat
- Layout: `{ { guid u32*2 u64 u8 } u32 u8*3 u32 loop[ u64 ] u8 } u32*2 u8*4 { u8 loop[ u8*3 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · (7 unused); byte 23: 8 · 8 · 8; byte 30: 1 · 1 · (6 unused); byte 39: 8 · 16; byte 42: 8 · 1 · (7 unused) · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8

### SMSG_RATED_PVP_INFO (0x41000f)

- Modern: 4259855 (0x41000f) · 4.3.4: — · Area: Combat
- Layout: `loop[ { u32*19 u8 } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 76: 1 · (7 unused); byte 153: 1 · (7 unused); byte 230: 1 · (7 unused); byte 307: 1 · (7 unused); byte 384: 1 · (7 unused); byte 461: 1 · (7 unused); byte 538: 1 · (7 unused); byte 615: 1 · (7 unused); byte 692: 1 · (7 unused)

### SMSG_INSPECT_HONOR_STATS (0x410011)

- Modern: 4259857 (0x410011) · 4.3.4: 31141 (0x79a5) · Area: Combat
- Layout: `guid u8 u16*8 u32*6 u8`
- Size: 42 bytes (packed GUIDs not counted)

### SMSG_PVP_LOG_DATA (0x410012)

- Modern: 4259858 (0x410012) · 4.3.4: 23730 (0x5cb2) · Area: Combat
- Layout: `{ u8 opt[ u8*2 guid bytes guid bytes ] u32 u8*2 opt[ u32*6 ] opt[ u8 ] loop[ { guid u32*5 u8 u32*5 loop[ u32 ] u8 opt[ u32*3 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · (5 unused); byte 5: 8 · 8

### SMSG_WARGAME_REQUEST_SUCCESSFULLY_SENT_TO_OPPONENT (0x410013)

- Modern: 4259859 (0x410013) · 4.3.4: — · Area: Combat
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_WARGAME_REQUEST_OPPONENT_RESPONSE (0x410015)

- Modern: 4259861 (0x410015) · 4.3.4: — · Area: Combat
- Layout: `guid u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### SMSG_PVP_OPTIONS_ENABLED (0x410016)

- Modern: 4259862 (0x410016) · 4.3.4: 20641 (0x50a1) · Area: Combat
- Layout: `u8*2`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (6 unused)

### SMSG_REQUEST_PVP_REWARDS_RESPONSE (0x410017)

- Modern: 4259863 (0x410017) · 4.3.4: 23972 (0x5da4) · Area: Combat
- Layout: `{ u8 u32*5 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } { u8 u32*5 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } { u8 u32*5 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } { u8 u32*5 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } { u8 u32*5 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } { u8 u32*5 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } { u8 u32*5 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] } { u8 u32*5 loop[ u32*2 ] loop[ u32*2 ] loop[ u32*2 ] u8 opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 21: 1 · 1 · 1 · 1 · (4 unused) · 8; byte 43: 1 · 1 · 1 · 1 · (4 unused) · 8; byte 65: 1 · 1 · 1 · 1 · (4 unused) · 8; byte 87: 1 · 1 · 1 · 1 · (4 unused) · 8; byte 109: 1 · 1 · 1 · 1 · (4 unused) · 8; byte 131: 1 · 1 · 1 · 1 · (4 unused) · 8; byte 153: 1 · 1 · 1 · 1 · (4 unused) · 8; byte 175: 1 · 1 · 1 · 1 · 4

### SMSG_REQUEST_SCHEDULED_PVP_INFO_RESPONSE (0x410018)

- Modern: 4259864 (0x410018) · 4.3.4: — · Area: Combat
- Layout: `{ u32*3 u8*5 bytes bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 12: 1 · 9 · 10 · 14

### SMSG_BREAK_TARGET (0x41001a)

- Modern: 4259866 (0x41001a) · 4.3.4: 261 (0x105) · Area: Combat
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_ATTACK_START (0x41001b)

- Modern: 4259867 (0x41001b) · 4.3.4: 11541 (0x2d15) · Area: Combat
- Layout: `guid guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_ATTACK_STOP (0x41001c)

- Modern: 4259868 (0x41001c) · 4.3.4: 2356 (0x934) · Area: Combat
- Layout: `guid guid u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_COMBAT_EVENT_FAILED (0x41001d)

- Modern: 4259869 (0x41001d) · 4.3.4: 11015 (0x2b07) · Area: Combat
- Layout: `guid guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_DUEL_REQUESTED (0x41001e)

- Modern: 4259870 (0x41001e) · 4.3.4: 17668 (0x4504) · Area: Combat
- Layout: `guid guid guid u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)

### SMSG_DUEL_ARRANGED (0x41001f)

- Modern: 4259871 (0x41001f) · 4.3.4: — · Area: Combat
- Layout: `guid guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_DUEL_OUT_OF_BOUNDS (0x410020)

- Modern: 4259872 (0x410020) · 4.3.4: 3110 (0xc26) · Area: Combat
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_DUEL_IN_BOUNDS (0x410021)

- Modern: 4259873 (0x410021) · 4.3.4: 2599 (0xa27) · Area: Combat
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_DUEL_COUNTDOWN (0x410022)

- Modern: 4259874 (0x410022) · 4.3.4: 18486 (0x4836) · Area: Combat
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_DUEL_COMPLETE (0x410023)

- Modern: 4259875 (0x410023) · 4.3.4: 9511 (0x2527) · Area: Combat
- Layout: `u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_DUEL_WINNER (0x410024)

- Modern: 4259876 (0x410024) · 4.3.4: 11574 (0x2d36) · Area: Combat
- Layout: `bits(6) bits(6) opt[ u8 ] u32*2 bytes bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 6 · 6 · 1 · (3 unused)

### SMSG_CAN_DUEL_RESULT (0x410025)

- Modern: 4259877 (0x410025) · 4.3.4: — · Area: Combat
- Layout: `guid u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · 1 · (6 unused)

### SMSG_CLEAR_TARGET (0x410026)

- Modern: 4259878 (0x410026) · 4.3.4: 19238 (0x4b26) · Area: Combat
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_RESET_RANGED_COMBAT_TIMER (0x410027)

- Modern: 4259879 (0x410027) · 4.3.4: — · Area: Combat
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_PVP_CREDIT (0x410028)

- Modern: 4259880 (0x410028) · 4.3.4: 24597 (0x6015) · Area: Combat
- Layout: `u32*2 guid u32 u8`
- Size: 13 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · (7 unused)

### SMSG_CANCEL_COMBAT (0x410029)

- Modern: 4259881 (0x410029) · 4.3.4: 20228 (0x4f04) · Area: Combat
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_ATTACK_SWING_ERROR (0x41002a)

- Modern: 4259882 (0x41002a) · 4.3.4: — · Area: Combat
- Layout: `bits(3)`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 3 · (5 unused)

### SMSG_ATTACK_SWING_LANDED_LOG (0x41002b)

- Modern: 4259883 (0x41002b) · 4.3.4: — · Area: Combat
- Layout: `u8 opt[ { u64 u32*5 u8*2 loop[ u8 u32*2 ] } ] u32 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_BATTLEGROUND_POINTS (0x41002c)

- Modern: 4259884 (0x41002c) · 4.3.4: — · Area: Combat
- Layout: `u16 u8`
- Size: 3 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)

### SMSG_BATTLEGROUND_INIT (0x41002d)

- Modern: 4259885 (0x41002d) · 4.3.4: — · Area: Combat
- Layout: `struct`
- Struct fields the client uses (6 bytes): +0: u32, +4: u16

### SMSG_MAP_OBJECTIVES_INIT (0x41002e)

- Modern: 4259886 (0x41002e) · 4.3.4: — · Area: Combat
- Layout: `u32 loop[ { guid u32*2 u8 opt[ u64 u32 ] } ]`

### SMSG_BOSS_KILL (0x41002f)

- Modern: 4259887 (0x41002f) · 4.3.4: — · Area: Combat
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_ATTACKER_STATE_UPDATE (0x410030)

- Modern: 4259888 (0x410030) · 4.3.4: 2853 (0xb25) · Area: Combat
- Layout: `u8 opt[ { u64 u32*5 u8*2 loop[ u8 u32*2 ] } ] u32 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_PVP_MATCH_START (0x410031)

- Modern: 4259889 (0x410031) · 4.3.4: — · Area: Combat
- Layout: `u32*2 u8*2 u32 u64 loop[ { guid u8 u32*10 loop[ u32 ] loop[ u32 ] u32*2 loop[ u32 ] loop[ u32 ] loop[ guid u32*2 ] loop[ u32 ] loop[ { u32*5 loop[ u32 ] loop[ u32 ] } ] u8 opt[ u32*2 loop[ u32 u16 u8 ] loop[ u32 u8*2 ] ] opt[ { u32*5 loop[ u32 ] loop[ u32*2 ] loop[ u32*3 ] } ] } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 8 · 1 · (7 unused)

### SMSG_PVP_MATCH_INITIALIZE (0x410034)

- Modern: 4259892 (0x410034) · 4.3.4: — · Area: Combat
- Layout: `u32 u8 u64*2 u8 u32 u8 opt[ u32*3 ]`
- Bit fields (all-zero packet, widths in arrival order): byte 26: 1 · 1 · 1 · (5 unused)
