# Cache — server packet layouts, 2.5.2.40892

### SMSG_DB_REPLY (0x290e)

- Modern: 10510 (0x290e) · 2.4.3: — · Area: Cache
- Layout: `{ u32*3 u8 u32 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 12: 3 · (5 unused)
- HermesProxy: `DBReply` — matches

### SMSG_AVAILABLE_HOTFIXES (0x290f)

- Modern: 10511 (0x290f) · 2.4.3: — · Area: Cache
- Layout: `{ u32*2 loop[ u32*2 ] }`
- HermesProxy: `AvailableHotfixes` — matches

### SMSG_HOTFIX_MESSAGE (0x2910)

- Modern: 10512 (0x2910) · 2.4.3: — · Area: Cache
- Layout: `{ u32 loop[ { u32*5 u8 } ] u32 bytes }`
- HermesProxy: `HotFixMessage` — matches

### SMSG_HOTFIX_CONNECT (0x2911)

- Modern: 10513 (0x2911) · 2.4.3: — · Area: Cache
- Layout: `{ u32 loop[ { u32*5 u8 } ] u32 bytes }`
- HermesProxy: `HotfixConnect` — matches

### SMSG_REALM_QUERY_RESPONSE (0x2913)

- Modern: 10515 (0x2913) · 2.4.3: — · Area: Cache
- Layout: `{ u32 u8 opt[ { u8*3 bytes bytes } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 8 · 1 · 1 · 8 · 8 · (6 unused)

### SMSG_QUERY_CREATURE_RESPONSE (0x2914)

- Modern: 10516 (0x2914) · 2.4.3: 97 (0x61) · Area: Cache
- Layout: `{ u32 u8 opt[ { u8*4 loop[ opt[ u8*2 ] opt[ u8 alt[ u8 ] opt[ u8 ] ] alt[ u8 ] alt[ opt[ u8 ] alt[ u8 ] ] ] loop[ { opt[ bytes ] } { opt[ bytes ] } ] loop[ u32 ] u32*4 loop[ u32 ] u32 u32 loop[ u32*3 ] u32*11 { opt[ bytes ] } { opt[ bytes ] } { opt[ bytes ] } loop[ u32 ] } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `QueryCreatureResponse` — matches

### SMSG_QUERY_GAME_OBJECT_RESPONSE (0x2915)

- Modern: 10517 (0x2915) · 2.4.3: 95 (0x5f) · Area: Cache
- Layout: `{ u32 guid u8 u32 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)
- HermesProxy: `QueryGameObjectResponse` — matches

### SMSG_QUERY_NPC_TEXT_RESPONSE (0x2916)

- Modern: 10518 (0x2916) · 2.4.3: 384 (0x180) · Area: Cache
- Layout: `{ u32 u8 u32 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `QueryNPCTextResponse` — matches

### SMSG_QUERY_PAGE_TEXT_RESPONSE (0x2917)

- Modern: 10519 (0x2917) · 2.4.3: 91 (0x5b) · Area: Cache
- Layout: `{ u32 u8 opt[ u32 loop[ u32*3 u8*3 bytes ] ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `QueryPageTextResponse` — matches

### SMSG_INVALIDATE_PAGE_TEXT (0x2918)

- Modern: 10520 (0x2918) · 2.4.3: — · Area: Cache
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_QUERY_PET_NAME_RESPONSE (0x2919)

- Modern: 10521 (0x2919) · 2.4.3: 83 (0x53) · Area: Cache
- Layout: `{ { guid u8 opt[ u8 loop[ opt[ u8 ] alt[ u8 ] ] ] opt[ loop[ bytes ] u64 bytes ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)
- HermesProxy: `QueryPetNameResponse` — matches

### SMSG_QUERY_BATTLE_PET_NAME_RESPONSE (0x291a)

- Modern: 10522 (0x291a) · 2.4.3: — · Area: Cache
- Layout: `{ { guid u32 u64 u8 opt[ u8 loop[ opt[ u8 ] alt[ u8 ] ] ] opt[ loop[ bytes ] bytes ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · (7 unused)

### SMSG_QUERY_PETITION_RESPONSE (0x291b)

- Modern: 10523 (0x291b) · 2.4.3: 455 (0x1c7) · Area: Cache
- Layout: `{ u32 u8 opt[ { u32 guid u32*7 u16 u32*5 u8*3 loop[ opt[ u8 ] alt[ u8 ] ] loop[ bytes ] bytes bytes } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `QueryPetitionResponse` — matches

### SMSG_CACHE_VERSION (0x291c)

- Modern: 10524 (0x291c) · 2.4.3: — · Area: Cache
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `ClientCacheVersion` — matches

### SMSG_CACHE_INFO (0x291d)

- Modern: 10525 (0x291d) · 2.4.3: — · Area: Cache
- Layout: `{ u32 u8 loop[ u8*2 bytes bytes ] bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 6 · (2 unused)

### SMSG_QUERY_ITEM_TEXT_RESPONSE (0x291e)

- Modern: 10526 (0x291e) · 2.4.3: 580 (0x244) · Area: Cache
- Layout: `{ u8*3 bytes guid }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused) · 13 · (3 unused)
- HermesProxy: `QueryItemTextResponse` — matches

### SMSG_TREASURE_PICKER_RESPONSE (0x291f)

- Modern: 10527 (0x291f) · 2.4.3: — · Area: Cache
- Layout: `{ u32*2 { u32*2 u64 u32*2 loop[ u32*2 ] loop[ { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u32 ] loop[ u32*2 u64 loop[ u32*2 ] u8 loop[ { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u32 ] ] } }`

### SMSG_QUERY_ARENA_TEAM_RESPONSE (0x2920)

- Modern: 10528 (0x2920) · 2.4.3: — · Area: Cache
- Layout: `{ u32 u8 opt[ u32*7 u8 bytes ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `ArenaTeamQueryResponse` — matches
