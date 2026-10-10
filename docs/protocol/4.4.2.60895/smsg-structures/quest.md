# Quest — server packet layouts, 4.4.2.60895

### SMSG_DAILY_QUESTS_RESET (0x4f0000)

- Modern: 5177344 (0x4f0000) · 4.3.4: — · Area: Quest
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_QUEST_COMPLETION_NPC_RESPONSE (0x4f0001)

- Modern: 5177345 (0x4f0001) · 4.3.4: 30113 (0x75a1) · Area: Quest
- Layout: `u32 loop[ u32*2 loop[ u32 ] ]`

### SMSG_QUEST_ITEM_USABILITY_RESPONSE (0x4f0002)

- Modern: 5177346 (0x4f0002) · 4.3.4: — · Area: Quest
- Layout: `guid u32*2 loop[ guid ] loop[ u8 ]`

### SMSG_QUEST_GIVER_QUEST_COMPLETE (0x4f0003)

- Modern: 5177347 (0x4f0003) · 4.3.4: 21924 (0x55a4) · Area: Quest
- Layout: `u32*2 u64 u32*2 u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · 1 · 1 · 1 · (4 unused); byte 37: 1 · (7 unused) · 6 · (2 unused)

### SMSG_IS_QUEST_COMPLETE_RESPONSE (0x4f0004)

- Modern: 5177348 (0x4f0004) · 4.3.4: — · Area: Quest
- Layout: `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_QUEST_GIVER_INVALID_QUEST (0x4f0005)

- Modern: 5177349 (0x4f0005) · 4.3.4: 16406 (0x4016) · Area: Quest
- Layout: `u32*2 u8*2 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · 9 · (6 unused)

### SMSG_QUEST_GIVER_QUEST_FAILED (0x4f0006)

- Modern: 5177350 (0x4f0006) · 4.3.4: 16950 (0x4236) · Area: Quest
- Layout: `struct`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32

### SMSG_QUEST_LOG_FULL (0x4f0007)

- Modern: 5177351 (0x4f0007) · 4.3.4: 3638 (0xe36) · Area: Quest
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_QUEST_NON_LOG_UPDATE_COMPLETE (0x4f0008)

- Modern: 5177352 (0x4f0008) · 4.3.4: — · Area: Quest
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_QUEST_UPDATE_COMPLETE (0x4f0009)

- Modern: 5177353 (0x4f0009) · 4.3.4: 10551 (0x2937) · Area: Quest
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_QUEST_UPDATE_FAILED (0x4f000a)

- Modern: 5177354 (0x4f000a) · 4.3.4: 25380 (0x6324) · Area: Quest
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_QUEST_UPDATE_FAILED_TIMER (0x4f000b)

- Modern: 5177355 (0x4f000b) · 4.3.4: 25639 (0x6427) · Area: Quest
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_QUEST_UPDATE_ADD_CREDIT (0x4f000c)

- Modern: 5177356 (0x4f000c) · 4.3.4: — · Area: Quest
- Layout: `guid u32*2 u16*2 u8`
- Size: 13 bytes (packed GUIDs not counted)

### SMSG_QUEST_UPDATE_ADD_CREDIT_SIMPLE (0x4f000d)

- Modern: 5177357 (0x4f000d) · 4.3.4: — · Area: Quest
- Layout: `struct`
- Struct fields the client uses (9 bytes): +0: u32, +4: u32, +8: u8

### SMSG_QUEST_UPDATE_ADD_PVP_CREDIT (0x4f000e)

- Modern: 5177358 (0x4f000e) · 4.3.4: 17430 (0x4416) · Area: Quest
- Layout: `struct`
- Struct fields the client uses (6 bytes): +0: u32, +4: u16

### SMSG_QUEST_CONFIRM_ACCEPT (0x4f000f)

- Modern: 5177359 (0x4f000f) · 4.3.4: 28423 (0x6f07) · Area: Quest
- Layout: `u32 guid u8*2 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 10 · (6 unused)

### SMSG_QUEST_PUSH_RESULT (0x4f0010)

- Modern: 5177360 (0x4f0010) · 4.3.4: — · Area: Quest
- Layout: `guid u8*3 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 8 · 9 · (7 unused)

### SMSG_QUEST_GIVER_STATUS_MULTIPLE (0x4f0011)

- Modern: 5177361 (0x4f0011) · 4.3.4: 20261 (0x4f25) · Area: Quest
- Layout: `u32 loop[ guid u64 ]`

### SMSG_QUEST_GIVER_QUEST_DETAILS (0x4f0012)

- Modern: 5177362 (0x4f0012) · 4.3.4: 9253 (0x2425) · Area: Quest
- Layout: `{ guid guid u32*18 loop[ u32 ] loop[ { u32*2 } ] loop[ u32*4 ] u8*10 { loop[ u32*2 ] loop[ u32*3 ] u32*4 u64 u32*4 loop[ u32*4 ] loop[ u32 ] u32*4 loop[ u32 ] u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8 } bytes bytes bytes bytes bytes bytes bytes loop[ { u32*2 u8*2 bytes } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 76: 9 · 12 · 12; byte 80: 10 · 8 · 10 · 8 · 1 · 1 · 1 · 1 · 1 · 1 · (5 unused); byte 314: 2 · (6 unused); byte 327: 1 · (7 unused) · 6 · (2 unused); byte 333: 2 · (6 unused); byte 346: 1 · (7 unused) · 6 · (2 unused); byte 352: 2 · (6 unused); byte 365: 1 · (7 unused) · 6 · (2 unused); byte 371: 2 · (6 unused); byte 384: 1 · (7 unused) · 6 · (2 unused); byte 390: 2 · (6 unused); byte 403: 1 · (7 unused) · 6 · (2 unused); byte 409: 2 · (6 unused); byte 422: 1 · (7 unused) · 6 · (2 unused); byte 428: 1 · (7 unused)

### SMSG_QUEST_GIVER_REQUEST_ITEMS (0x4f0013)

- Modern: 5177363 (0x4f0013) · 4.3.4: 25142 (0x6236) · Area: Quest
- Layout: `{ { u32*2 guid u32*11 loop[ u32*3 ] loop[ u32*2 ] u8 } u32*2 u8*3 loop[ { u32*2 u8*2 bytes } ] bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 54: 1 · 1 · (6 unused); byte 63: 9 · 12 · (3 unused)

### SMSG_QUEST_GIVER_OFFER_REWARD_MESSAGE (0x4f0014)

- Modern: 5177364 (0x4f0014) · 4.3.4: 9255 (0x2427) · Area: Quest
- Layout: `{ { { loop[ u32*2 ] loop[ u32*3 ] u32*4 u64 u32*4 loop[ u32*4 ] loop[ u32 ] u32*4 loop[ u32 ] u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } u32 u8 } u32 guid u32*7 loop[ u32*2 ] u8 } u32*7 u8*8 loop[ { u32*2 u8*2 bytes } ] bytes bytes bytes bytes bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 228: 2 · (6 unused); byte 241: 1 · (7 unused) · 6 · (2 unused); byte 247: 2 · (6 unused); byte 260: 1 · (7 unused) · 6 · (2 unused); byte 266: 2 · (6 unused); byte 279: 1 · (7 unused) · 6 · (2 unused); byte 285: 2 · (6 unused); byte 298: 1 · (7 unused) · 6 · (2 unused); byte 304: 2 · (6 unused); byte 317: 1 · (7 unused) · 6 · (2 unused); byte 323: 2 · (6 unused); byte 336: 1 · (7 unused) · 6 · (2 unused); byte 342: 1 · (7 unused); byte 377: 1 · 1 · 1 · (5 unused); byte 406: 9 · 12 · 10 · 8 · 10; byte 412: 8 · (7 unused)

### SMSG_SHOW_QUEST_COMPLETION_TEXT (0x4f0015)

- Modern: 5177365 (0x4f0015) · 4.3.4: — · Area: Quest
- Layout: `{ u32*7 u8*8 loop[ { u32*2 u8*2 bytes } ] bytes bytes bytes bytes bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 28: 9 · 12 · 10 · 8 · 10; byte 34: 8 · (7 unused)

### SMSG_QUERY_QUEST_INFO_RESPONSE (0x4f0016)

- Modern: 5177366 (0x4f0016) · 4.3.4: 26934 (0x6936) · Area: Quest
- Layout: `u32 u8 opt[ { u32*17 loop[ u32 ] u32*56 loop[ u32*4 ] u32 loop[ u32*2 ] u32*3 u64 u32 u64 u32*6 loop[ u32 ] loop[ u32 ] u8*3 bits(4) u8 alt[ u8 ] { opt[ u8 ] alt[ u8 ] opt[ u8 ] } opt[ u8 ] alt[ u8 ] bits(2) u8*2 alt[ u8 ] opt[ u8 ] alt[ u8 ] u8 alt[ u8 ] opt[ u8 ] loop[ u32*2 u8 u32*6 loop[ u32 ] u8 bytes ] bytes bytes bytes bytes bytes bytes bytes bytes bytes loop[ { u32*2 u8*2 bytes } ] loop[ { u32*2 u8*2 bytes } ] } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_GOSSIP_COMPLETE (0x4f0017)

- Modern: 5177367 (0x4f0017) · 4.3.4: 2054 (0x806) · Area: Quest
- Layout: `u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_GOSSIP_MESSAGE (0x4f0018)

- Modern: 5177368 (0x4f0018) · 4.3.4: 8245 (0x2035) · Area: Quest
- Layout: `{ guid u32*5 u8 loop[ { u32 u8*2 u32*4 u8*5 u32 loop[ u8 u32*2 u8 ] bytes bytes opt[ u32 ] opt[ u32 ] { bytes } } ] opt[ u32 ] opt[ u32 ] loop[ { u32*9 u8*2 bytes } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 1 · 1 · (6 unused)

### SMSG_GOSSIP_QUEST_UPDATE (0x4f0019)

- Modern: 5177369 (0x4f0019) · 4.3.4: — · Area: Quest
- Layout: `guid { u32*9 u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 38: 1 · 1 · 1 · 1 · 9 · (3 unused)

### SMSG_QUEST_GIVER_QUEST_LIST_MESSAGE (0x4f001a)

- Modern: 5177370 (0x4f001a) · 4.3.4: 308 (0x134) · Area: Quest
- Layout: `guid u32*3 u8 bits(3) loop[ { u32*9 u8*2 bytes } ] bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 11 · (5 unused)

### SMSG_QUEST_GIVER_STATUS (0x4f001b)

- Modern: 5177371 (0x4f001b) · 4.3.4: 8469 (0x2115) · Area: Quest
- Layout: `guid u64`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_QUEST_FORCE_REMOVED (0x4f001c)

- Modern: 5177372 (0x4f001c) · 4.3.4: 26117 (0x6605) · Area: Quest
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_QUEST_POI_QUERY_RESPONSE (0x4f001d)

- Modern: 5177373 (0x4f001d) · 4.3.4: 25348 (0x6304) · Area: Quest
- Layout: `u32*2 loop[ { u32*2 loop[ { u32*13 loop[ u16*3 ] u8 } ] } ]`

### SMSG_DISPLAY_QUEST_POPUP (0x4f001e)

- Modern: 5177374 (0x4f001e) · 4.3.4: — · Area: Quest
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_QUEST_POI_UPDATE_RESPONSE (0x4f001f)

- Modern: 5177375 (0x4f001f) · 4.3.4: — · Area: Quest
- Layout: `u32 loop[ { u32*5 u8 } ]`

### SMSG_RESET_QUEST_POI (0x4f0020)

- Modern: 5177376 (0x4f0020) · 4.3.4: — · Area: Quest
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_COVENANT_CALLINGS_AVAILABILITY_RESPONSE (0x4f0024)

- Modern: 5177380 (0x4f0024) · 4.3.4: — · Area: Quest
- Layout: `u8 u32 loop[ u32 ]`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### SMSG_GOSSIP_OPTION_NPC_INTERACTION (0x4f0028)

- Modern: 5177384 (0x4f0028) · 4.3.4: — · Area: Quest
- Layout: `guid u32 u8 alt[ u32 ]`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)
