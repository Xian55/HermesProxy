# Cache — server packet layouts, 3.4.3.54261

### SMSG_DB_REPLY (0x290e)

- Modern: 10510 (0x290e) · 3.3.5a: — · Area: Cache
- Layout: `u32*3 u8 u32 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 12: 3 · (5 unused)
- HermesProxy: `DBReply` — matches

### SMSG_AVAILABLE_HOTFIXES (0x290f)

- Modern: 10511 (0x290f) · 3.3.5a: — · Area: Cache
- Layout: `u32*2 loop[ u32*2 ]`
- HermesProxy: `AvailableHotfixes` — matches

### SMSG_HOTFIX_MESSAGE (0x2910)

- Modern: 10512 (0x2910) · 3.3.5a: — · Area: Cache
- Layout: `u32 loop[ { u32*5 u8 } ] u32 bytes`
- HermesProxy: `HotFixMessage` — matches

### SMSG_HOTFIX_CONNECT (0x2911)

- Modern: 10513 (0x2911) · 3.3.5a: — · Area: Cache
- Layout: `u32 loop[ { u32*5 u8 } ] u32 bytes`
- HermesProxy: `HotfixConnect` — matches

### — (0x2913)

- Modern: 10515 (0x2913) · 3.3.5a: — · Area: Cache
- Layout: `u32 u8 opt[ { u8*3 bytes bytes } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 8 · 1 · 1 · 8 · 8 · (6 unused)

### SMSG_QUERY_CREATURE_RESPONSE (0x2914)

- Modern: 10516 (0x2914) · 3.3.5a: 97 (0x61) · Area: Cache
- Layout: `u32 u8 opt[ { u8*4 loop[ opt[ u8 ] alt[ u8 ] bits(3) opt[ u8 ] alt[ u8 ] bits(3) ] loop[ { opt[ bytes ] } { opt[ bytes ] } ] loop[ u32 ] u32*7 u32 loop[ u32*3 ] u32*11 { opt[ bytes ] } { opt[ bytes ] } { opt[ bytes ] } loop[ u32 ] } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `QueryCreatureResponse` — matches

### SMSG_QUERY_GAME_OBJECT_RESPONSE (0x2915)

- Modern: 10517 (0x2915) · 3.3.5a: 95 (0x5f) · Area: Cache
- Layout: `u32 guid u8 u32 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)
- HermesProxy: `QueryGameObjectResponse` — matches

### SMSG_QUERY_NPC_TEXT_RESPONSE (0x2916)

- Modern: 10518 (0x2916) · 3.3.5a: 384 (0x180) · Area: Cache
- Layout: `u32 u8 u32 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `QueryNPCTextResponse` — matches

### SMSG_QUERY_PAGE_TEXT_RESPONSE (0x2917)

- Modern: 10519 (0x2917) · 3.3.5a: 91 (0x5b) · Area: Cache
- Layout: `u32 u8 opt[ u32 loop[ u32*3 u8*3 bytes ] ]`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `QueryPageTextResponse` — matches

### — (0x2918)

- Modern: 10520 (0x2918) · 3.3.5a: — · Area: Cache
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_QUERY_PET_NAME_RESPONSE (0x2919)

- Modern: 10521 (0x2919) · 3.3.5a: 83 (0x53) · Area: Cache
- Layout: `{ guid u8 opt[ u8*5 bytes bytes bytes bytes bytes u64 bytes ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 1 · (7 unused)
- HermesProxy: `QueryPetNameResponse` — matches

### — (0x291a)

- Modern: 10522 (0x291a) · 3.3.5a: — · Area: Cache
- Layout: `{ guid u32 u64 u8 opt[ u8*5 bytes bytes bytes bytes bytes bytes ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 1 · (7 unused)

### SMSG_QUERY_PETITION_RESPONSE (0x291b)

- Modern: 10523 (0x291b) · 3.3.5a: 455 (0x1c7) · Area: Cache
- Layout: `u32 u8 opt[ { u32 guid u32*7 u16 u32*5 u8*3 loop[ opt[ u8 ] alt[ u8 ] ] loop[ bytes ] bytes bytes } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `QueryPetitionResponse` — matches

### SMSG_CACHE_VERSION (0x291c)

- Modern: 10524 (0x291c) · 3.3.5a: 1195 (0x4ab) · Area: Cache
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `ClientCacheVersion` — matches

### — (0x291d)

- Modern: 10525 (0x291d) · 3.3.5a: — · Area: Cache
- Layout: `u32 bits(6) loop[ bits(6) bits(6) bytes bytes ] bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 6 · (2 unused)

### SMSG_QUERY_ITEM_TEXT_RESPONSE (0x291e)

- Modern: 10526 (0x291e) · 3.3.5a: 580 (0x244) · Area: Cache
- Layout: `u8*2 bits(5) bytes guid`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused) · 13 · (3 unused)
- HermesProxy: `QueryItemTextResponse` — matches

### — (0x291f)

- Modern: 10527 (0x291f) · 3.3.5a: — · Area: Cache
- Layout: `u32*2 { u32 u32 u64 u32 u32 loop[ u32*2 ] loop[ { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 ] loop[ u32 u32 u64 loop[ u32*2 ] u8 loop[ { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 ] ] }`

### SMSG_QUERY_ARENA_TEAM_RESPONSE (0x2920)

- Modern: 10528 (0x2920) · 3.3.5a: — · Area: Cache
- Layout: `u32 u8 opt[ u32*7 u8 bytes ]`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `ArenaTeamQueryResponse` — matches
