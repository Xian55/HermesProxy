# Quest — server packet layouts, 3.4.3.54261

### — (0x2a80)

- Modern: 10880 (0x2a80) · 3.3.5a: — · Area: Quest
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### SMSG_QUEST_COMPLETION_NPC_RESPONSE (0x2a81)

- Modern: 10881 (0x2a81) · 3.3.5a: — · Area: Quest
- Layout: `{ u32 alt[ ] loop[ u32*2 loop[ u32 ] ] }`

### — (0x2a82)

- Modern: 10882 (0x2a82) · 3.3.5a: — · Area: Quest
- Layout: `guid u32*2 loop[ guid ] loop[ u8 ]`

### SMSG_QUEST_GIVER_QUEST_COMPLETE (0x2a83)

- Modern: 10883 (0x2a83) · 3.3.5a: 401 (0x191) · Area: Quest
- Layout: `u32*2 u64 u32*2 u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · 1 · 1 · 1 · (4 unused); byte 37: 1 · 1 · (6 unused) · 6 · (2 unused)
- HermesProxy: `QuestGiverQuestComplete` — differs (#369)

### — (0x2a84)

- Modern: 10884 (0x2a84) · 3.3.5a: — · Area: Quest
- Layout: `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_QUEST_GIVER_INVALID_QUEST (0x2a85)

- Modern: 10885 (0x2a85) · 3.3.5a: 399 (0x18f) · Area: Quest
- Layout: `u32*2 u8*2 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · 9 · (6 unused)
- HermesProxy: `QuestGiverInvalidQuest` — matches

### SMSG_QUEST_GIVER_QUEST_FAILED (0x2a86)

- Modern: 10886 (0x2a86) · 3.3.5a: 402 (0x192) · Area: Quest
- Layout: `struct`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32
- HermesProxy: `QuestGiverQuestFailed` — matches

### — (0x2a87)

- Modern: 10887 (0x2a87) · 3.3.5a: — · Area: Quest
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### — (0x2a88)

- Modern: 10888 (0x2a88) · 3.3.5a: — · Area: Quest
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_QUEST_UPDATE_COMPLETE (0x2a89)

- Modern: 10889 (0x2a89) · 3.3.5a: 408 (0x198) · Area: Quest
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_QUEST_UPDATE_FAILED (0x2a8a)

- Modern: 10890 (0x2a8a) · 3.3.5a: 406 (0x196) · Area: Quest
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_QUEST_UPDATE_FAILED_TIMER (0x2a8b)

- Modern: 10891 (0x2a8b) · 3.3.5a: 407 (0x197) · Area: Quest
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_QUEST_UPDATE_ADD_CREDIT (0x2a8c)

- Modern: 10892 (0x2a8c) · 3.3.5a: — · Area: Quest
- Layout: `guid u32*2 u16*2 u8`
- Size: 13 bytes (packed GUIDs not counted)
- HermesProxy: `QuestUpdateAddCredit` — matches

### SMSG_QUEST_UPDATE_ADD_CREDIT_SIMPLE (0x2a8d)

- Modern: 10893 (0x2a8d) · 3.3.5a: — · Area: Quest
- Layout: `struct`
- Struct fields the client uses (9 bytes): +0: u32, +4: u32, +8: u8
- HermesProxy: `QuestUpdateAddCreditSimple` — matches

### — (0x2a8e)

- Modern: 10894 (0x2a8e) · 3.3.5a: — · Area: Quest
- Layout: `struct`
- Struct fields the client uses (6 bytes): +0: u32, +4: u16

### SMSG_QUEST_CONFIRM_ACCEPT (0x2a8f)

- Modern: 10895 (0x2a8f) · 3.3.5a: 412 (0x19c) · Area: Quest
- Layout: `u32 guid u8*2 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 10 · (6 unused)
- HermesProxy: `QuestConfirmAccept` — matches

### SMSG_QUEST_PUSH_RESULT (0x2a90)

- Modern: 10896 (0x2a90) · 3.3.5a: — · Area: Quest
- Layout: `guid u8*3 bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 2: 8 · 9 · (7 unused)
- HermesProxy: `QuestPushResult` — differs (#369)

### SMSG_QUEST_GIVER_STATUS_MULTIPLE (0x2a91)

- Modern: 10897 (0x2a91) · 3.3.5a: 1048 (0x418) · Area: Quest
- Layout: `u32 loop[ guid u64 ]`
- HermesProxy: `QuestGiverStatusMultiple` — matches

### SMSG_QUEST_GIVER_QUEST_DETAILS (0x2a92)

- Modern: 10898 (0x2a92) · 3.3.5a: 392 (0x188) · Area: Quest
- Layout: `{ guid guid u32*12 u32 u32*4 loop[ u32 ] loop[ u32*2 ] loop[ u32*3 u8 ] bits(9) opt[ u8 ] alt[ u8 ] bits(4) opt[ u8 ] alt[ u8 ] bits(4) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] bits(2) u8 opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] { u32*12 u64 u32*4 loop[ u32*4 ] loop[ u32 ] u32 loop[ u32*2 ] u32*3 u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 u8 } bytes bytes bytes bytes bytes bytes bytes loop[ { u32*2 u8*2 bytes } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 72: 9 · 12 · 12; byte 76: 10 · 8 · 10 · 8 · 1 · 1 · 1 · 1 · (7 unused); byte 294: 2 · (6 unused); byte 307: 1 · (7 unused) · 6 · (2 unused); byte 313: 2 · (6 unused); byte 326: 1 · (7 unused) · 6 · (2 unused); byte 332: 2 · (6 unused); byte 345: 1 · (7 unused) · 6 · (2 unused); byte 351: 2 · (6 unused); byte 364: 1 · (7 unused) · 6 · (2 unused); byte 370: 2 · (6 unused); byte 383: 1 · (7 unused) · 6 · (2 unused); byte 389: 2 · (6 unused); byte 402: 1 · (7 unused) · 6 · (2 unused); byte 408: 1 · (7 unused)
- HermesProxy: `QuestGiverQuestDetails` — matches

### SMSG_QUEST_GIVER_REQUEST_ITEMS (0x2a93)

- Modern: 10899 (0x2a93) · 3.3.5a: 395 (0x18b) · Area: Quest
- Layout: `{ { guid u32*10 u32 u32 loop[ u32*3 ] loop[ u32*2 ] u8 } u32*2 u8*3 loop[ { u32*2 u8*2 bytes } ] bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · (7 unused); byte 59: 9 · 12 · (3 unused)
- HermesProxy: `QuestGiverRequestItems` — matches

### SMSG_QUEST_GIVER_OFFER_REWARD_MESSAGE (0x2a94)

- Modern: 10900 (0x2a94) · 3.3.5a: 397 (0x18d) · Area: Quest
- Layout: `{ guid u32*7 loop[ u32*2 ] u8 { u32*12 u64 u32*4 loop[ u32*4 ] loop[ u32 ] u32 loop[ u32*2 ] u32*3 u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 u8 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } u32 u8 } u32*7 u8*4 bits(2) u8 opt[ u8 ] alt[ u8 ] bits(2) u8 loop[ { u32*2 u8*2 bytes } ] bytes bytes bytes bytes bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 30: 1 · 1 · (6 unused); byte 243: 2 · (6 unused); byte 256: 1 · (7 unused) · 6 · (2 unused); byte 262: 2 · (6 unused); byte 275: 1 · (7 unused) · 6 · (2 unused); byte 281: 2 · (6 unused); byte 294: 1 · (7 unused) · 6 · (2 unused); byte 300: 2 · (6 unused); byte 313: 1 · (7 unused) · 6 · (2 unused); byte 319: 2 · (6 unused); byte 332: 1 · (7 unused) · 6 · (2 unused); byte 338: 2 · (6 unused); byte 351: 1 · (7 unused) · 6 · (2 unused); byte 357: 1 · (7 unused); byte 386: 9 · 12 · 10 · 8 · 10; byte 392: 8 · (7 unused)
- HermesProxy: `QuestGiverOfferRewardMessage` — matches

### — (0x2a95)

- Modern: 10901 (0x2a95) · 3.3.5a: — · Area: Quest
- Layout: `{ u32*7 u8*8 loop[ { u32*2 u8*2 bytes } ] bytes bytes bytes bytes bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 28: 9 · 12 · 10 · 8 · 10; byte 34: 8 · (7 unused)

### SMSG_QUERY_QUEST_INFO_RESPONSE (0x2a96)

- Modern: 10902 (0x2a96) · 3.3.5a: 93 (0x5d) · Area: Quest
- Layout: `u32 u8 opt[ { u32*17 loop[ u32 ] u32*56 loop[ u32*4 ] u32 loop[ u32*2 ] u32*3 u64 u32 u64 u32*4 u32 bits(9) opt[ u8 ] alt[ u8 ] bits(4) opt[ u8 ] alt[ u8 ] bits(4) bits(9) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] bits(3) loop[ u32 u8*2 u32*6 loop[ u32 ] u8 bytes ] bytes bytes bytes bytes bytes bytes bytes bytes bytes loop[ { u32*2 u8*2 bytes } ] loop[ { u32*2 u8*2 bytes } ] } ]`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `QueryQuestInfoResponse` — matches

### SMSG_GOSSIP_COMPLETE (0x2a97)

- Modern: 10903 (0x2a97) · 3.3.5a: 382 (0x17e) · Area: Quest
- Layout: `u8`
- Size: 1 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
- HermesProxy: `GossipComplete` — matches

### SMSG_GOSSIP_MESSAGE (0x2a98)

- Modern: 10904 (0x2a98) · 3.3.5a: 381 (0x17d) · Area: Quest
- Layout: `{ guid u32*4 u8 loop[ { u32 u8*2 u32*4 u8*4 u32 loop[ u8 u32*2 ] bytes bytes opt[ u32 ] opt[ u32 ] } ] opt[ u32 ] opt[ u32 ] loop[ { u32*7 u8*2 bytes } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · 1 · (6 unused)
- HermesProxy: `GossipMessagePkt` — matches

### — (0x2a99)

- Modern: 10905 (0x2a99) · 3.3.5a: — · Area: Quest
- Layout: `guid { u32*7 u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 30: 1 · 1 · 9 · (5 unused)

### SMSG_QUEST_GIVER_QUEST_LIST_MESSAGE (0x2a9a)

- Modern: 10906 (0x2a9a) · 3.3.5a: 389 (0x185) · Area: Quest
- Layout: `guid u32*3 u8 bits(3) loop[ { u32*7 u8*2 bytes } ] bytes`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 11 · (5 unused)
- HermesProxy: `QuestGiverQuestListMessage` — matches

### SMSG_QUEST_GIVER_STATUS (0x2a9b)

- Modern: 10907 (0x2a9b) · 3.3.5a: 387 (0x183) · Area: Quest
- Layout: `guid u64`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `QuestGiverStatusPkt` — matches

### — (0x2a9c)

- Modern: 10908 (0x2a9c) · 3.3.5a: — · Area: Quest
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_QUEST_POI_QUERY_RESPONSE (0x2a9d)

- Modern: 10909 (0x2a9d) · 3.3.5a: 484 (0x1e4) · Area: Quest
- Layout: `u32*2 loop[ u32*2 loop[ { u32*13 loop[ u16*3 ] u8 } ] ]`
- HermesProxy: `QuestPOIQueryResponse` — matches

### — (0x2a9e)

- Modern: 10910 (0x2a9e) · 3.3.5a: — · Area: Quest
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x2a9f)

- Modern: 10911 (0x2a9f) · 3.3.5a: — · Area: Quest
- Layout: `u32 loop[ u32*5 u8 ]`

### — (0x2aa0)

- Modern: 10912 (0x2aa0) · 3.3.5a: — · Area: Quest
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### — (0x2aa3)

- Modern: 10915 (0x2aa3) · 3.3.5a: — · Area: Quest
- Layout: `u8 u32 loop[ u32 ]`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)

### — (0x2aa7)

- Modern: 10919 (0x2aa7) · 3.3.5a: — · Area: Quest
- Layout: `guid u32 u8 opt[ u32 ]`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 1 · (7 unused)
