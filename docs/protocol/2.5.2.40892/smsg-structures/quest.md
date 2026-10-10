# Quest — server packet layouts, 2.5.2.40892

### SMSG_DAILY_QUESTS_RESET (0x2a80)

- Modern: 10880 (0x2a80) · 2.4.3: — · Area: Quest
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_QUEST_COMPLETION_NPC_RESPONSE (0x2a81)

- Modern: 10881 (0x2a81) · 2.4.3: — · Area: Quest
- Layout: `{ { u32 alt[ ] loop[ u32*2 loop[ u32 ] ] } }`

### SMSG_QUEST_GIVER_QUEST_COMPLETE (0x2a83)

- Modern: 10883 (0x2a83) · 2.4.3: 401 (0x191) · Area: Quest
- Layout: `{ u32*2 u64 u32*2 u8 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 24: 1 · 1 · 1 · 1 · (4 unused); byte 37: 1 · (7 unused) · 6 · (2 unused)
- HermesProxy: `QuestGiverQuestComplete` — matches

### SMSG_IS_QUEST_COMPLETE_RESPONSE (0x2a84)

- Modern: 10884 (0x2a84) · 2.4.3: — · Area: Quest
- Layout: `{ u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)

### SMSG_QUEST_GIVER_INVALID_QUEST (0x2a85)

- Modern: 10885 (0x2a85) · 2.4.3: 399 (0x18f) · Area: Quest
- Layout: `{ u32*2 u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 8: 1 · 9 · (6 unused)
- HermesProxy: `QuestGiverInvalidQuest` — matches

### SMSG_QUEST_GIVER_QUEST_FAILED (0x2a86)

- Modern: 10886 (0x2a86) · 2.4.3: 402 (0x192) · Area: Quest
- Layout: `{ struct }`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32
- HermesProxy: `QuestGiverQuestFailed` — matches

### SMSG_QUEST_LOG_FULL (0x2a87)

- Modern: 10887 (0x2a87) · 2.4.3: 405 (0x195) · Area: Quest
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_QUEST_NON_LOG_UPDATE_COMPLETE (0x2a88)

- Modern: 10888 (0x2a88) · 2.4.3: — · Area: Quest
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_QUEST_UPDATE_COMPLETE (0x2a89)

- Modern: 10889 (0x2a89) · 2.4.3: 408 (0x198) · Area: Quest
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_QUEST_UPDATE_FAILED (0x2a8a)

- Modern: 10890 (0x2a8a) · 2.4.3: 406 (0x196) · Area: Quest
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_QUEST_UPDATE_FAILED_TIMER (0x2a8b)

- Modern: 10891 (0x2a8b) · 2.4.3: 407 (0x197) · Area: Quest
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_QUEST_UPDATE_ADD_CREDIT (0x2a8c)

- Modern: 10892 (0x2a8c) · 2.4.3: — · Area: Quest
- Layout: `{ guid u32*2 u16*2 u8 }`
- Size: 13 bytes (packed GUIDs not counted)
- HermesProxy: `QuestUpdateAddCredit` — matches

### SMSG_QUEST_UPDATE_ADD_CREDIT_SIMPLE (0x2a8d)

- Modern: 10893 (0x2a8d) · 2.4.3: — · Area: Quest
- Layout: `{ struct }`
- Struct fields the client uses (9 bytes): +0: u32, +4: u32, +8: u8
- HermesProxy: `QuestUpdateAddCreditSimple` — matches

### SMSG_QUEST_UPDATE_ADD_PVP_CREDIT (0x2a8e)

- Modern: 10894 (0x2a8e) · 2.4.3: — · Area: Quest
- Layout: `{ struct }`
- Struct fields the client uses (6 bytes): +0: u32, +4: u16

### SMSG_QUEST_CONFIRM_ACCEPT (0x2a8f)

- Modern: 10895 (0x2a8f) · 2.4.3: 412 (0x19c) · Area: Quest
- Layout: `{ u32 guid u8*2 bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 6: 10 · (6 unused)
- HermesProxy: `QuestConfirmAccept` — matches

### SMSG_QUEST_PUSH_RESULT (0x2a90)

- Modern: 10896 (0x2a90) · 2.4.3: — · Area: Quest
- Layout: `{ guid u8 }`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `QuestPushResult` — matches

### SMSG_QUEST_GIVER_STATUS_MULTIPLE (0x2a91)

- Modern: 10897 (0x2a91) · 2.4.3: 1047 (0x417) · Area: Quest
- Layout: `{ u32 loop[ guid u32 ] }`
- HermesProxy: `QuestGiverStatusMultiple` — matches

### SMSG_QUEST_GIVER_QUEST_DETAILS (0x2a92)

- Modern: 10898 (0x2a92) · 2.4.3: 392 (0x188) · Area: Quest
- Layout: `{ { guid guid u32*6 loop[ u32 ] u32*3 u32 u32*2 loop[ u32 ] loop[ { u32*2 } ] loop[ u32*3 u8 ] u8*10 { u32*2 loop[ u32*2 ] u32*2 u64 u32*4 loop[ u32*4 ] loop[ u32 ] u32 loop[ u32*2 ] u32*3 loop[ u8 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u32 ] u8 } bytes bytes bytes bytes bytes bytes bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 60: 9 · 12 · 12; byte 64: 10 · 8 · 10 · 8 · 1 · 1 · 1 · 1 · (7 unused); byte 282: 2 · (6 unused); byte 295: 1 · (7 unused) · 6 · (2 unused); byte 301: 2 · (6 unused); byte 314: 1 · (7 unused) · 6 · (2 unused); byte 320: 2 · (6 unused); byte 333: 1 · (7 unused) · 6 · (2 unused); byte 339: 2 · (6 unused); byte 352: 1 · (7 unused) · 6 · (2 unused); byte 358: 2 · (6 unused); byte 371: 1 · (7 unused) · 6 · (2 unused); byte 377: 2 · (6 unused); byte 390: 1 · (7 unused) · 6 · (2 unused); byte 396: 1 · (7 unused)
- HermesProxy: `QuestGiverQuestDetails` — matches

### SMSG_QUEST_GIVER_REQUEST_ITEMS (0x2a93)

- Modern: 10899 (0x2a93) · 2.4.3: 395 (0x18b) · Area: Quest
- Layout: `{ { guid u32*4 loop[ u32 ] u32*3 u32 u32 loop[ u32*3 ] loop[ u32*2 ] u8 } u8*3 bytes bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 46: 1 · (7 unused) · 9 · 12 · (3 unused)
- HermesProxy: `QuestGiverRequestItems` — matches

### SMSG_QUEST_GIVER_OFFER_REWARD_MESSAGE (0x2a94)

- Modern: 10900 (0x2a94) · 2.4.3: 397 (0x18d) · Area: Quest
- Layout: `{ { { guid u32*2 loop[ u32 ] u32*2 loop[ u32*2 ] u8 { u32*2 loop[ u32*2 ] u32*2 u64 u32*4 loop[ u32*4 ] loop[ u32 ] u32 loop[ u32*2 ] u32*3 loop[ u8 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u32 ] u8 } } u32*5 u8*8 bytes bytes bytes bytes bytes bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 26: 1 · 1 · (6 unused); byte 239: 2 · (6 unused); byte 252: 1 · (7 unused) · 6 · (2 unused); byte 258: 2 · (6 unused); byte 271: 1 · (7 unused) · 6 · (2 unused); byte 277: 2 · (6 unused); byte 290: 1 · (7 unused) · 6 · (2 unused); byte 296: 2 · (6 unused); byte 309: 1 · (7 unused) · 6 · (2 unused); byte 315: 2 · (6 unused); byte 328: 1 · (7 unused) · 6 · (2 unused); byte 334: 2 · (6 unused); byte 347: 1 · (7 unused) · 6 · (2 unused); byte 353: 1 · (7 unused); byte 374: 9 · 12 · 10 · 8 · 10; byte 380: 8 · (7 unused)
- HermesProxy: `QuestGiverOfferRewardMessage` — matches

### SMSG_SHOW_QUEST_COMPLETION_TEXT (0x2a95)

- Modern: 10901 (0x2a95) · 2.4.3: — · Area: Quest
- Layout: `{ { u32*5 u8*8 bytes bytes bytes bytes bytes bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 20: 9 · 12 · 10 · 8 · 10; byte 26: 8 · (7 unused)

### SMSG_QUERY_QUEST_INFO_RESPONSE (0x2a96)

- Modern: 10902 (0x2a96) · 2.4.3: 93 (0x5d) · Area: Quest
- Layout: `{ u32 u8 opt[ { u32*17 loop[ u32 ] u32*10 loop[ u32*4 ] loop[ u32*3 ] u32*12 loop[ u32*4 ] u32 loop[ u32*2 ] u32*5 u64 u32*2 u8*12 loop[ u32 u8*2 u32*6 loop[ u32 ] u8 bytes ] bytes bytes bytes bytes bytes bytes bytes bytes bytes } ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 4: 1 · (7 unused)
- HermesProxy: `QueryQuestInfoResponse` — matches

### SMSG_GOSSIP_COMPLETE (0x2a97)

- Modern: 10903 (0x2a97) · 2.4.3: 382 (0x17e) · Area: Quest
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
- HermesProxy: `GossipComplete` — matches

### SMSG_GOSSIP_MESSAGE (0x2a98)

- Modern: 10904 (0x2a98) · 2.4.3: 381 (0x17d) · Area: Quest
- Layout: `{ guid u32*5 loop[ { u32 u8*2 u32 u8*4 u32 loop[ u8 u32*2 ] bytes bytes opt[ u32 ] } ] loop[ { u32*5 loop[ u32 ] u8*2 bytes } ] }`
- HermesProxy: `GossipMessagePkt` — matches

### SMSG_GOSSIP_QUEST_UPDATE (0x2a99)

- Modern: 10905 (0x2a99) · 2.4.3: — · Area: Quest
- Layout: `{ guid { u32*5 loop[ u32 ] u8*2 bytes } }`
- Bit fields (all-zero packet, widths in arrival order): byte 30: 1 · 9 · (6 unused)

### SMSG_QUEST_GIVER_QUEST_LIST_MESSAGE (0x2a9a)

- Modern: 10906 (0x2a9a) · 2.4.3: 389 (0x185) · Area: Quest
- Layout: `{ guid u32*3 u8*2 loop[ { u32*5 loop[ u32 ] u8*2 bytes } ] bytes }`
- Bit fields (all-zero packet, widths in arrival order): byte 14: 11 · (5 unused)
- HermesProxy: `QuestGiverQuestListMessage` — matches

### SMSG_QUEST_GIVER_STATUS (0x2a9b)

- Modern: 10907 (0x2a9b) · 2.4.3: 387 (0x183) · Area: Quest
- Layout: `{ guid u32 }`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `QuestGiverStatusPkt` — matches

### SMSG_QUEST_FORCE_REMOVED (0x2a9c)

- Modern: 10908 (0x2a9c) · 2.4.3: — · Area: Quest
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_DISPLAY_QUEST_POPUP (0x2a9e)

- Modern: 10910 (0x2a9e) · 2.4.3: — · Area: Quest
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x2aa3)

- Modern: 10915 (0x2aa3) · 2.4.3: — · Area: Quest
- Layout: `{ u8 u32 loop[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 1 · (7 unused)
