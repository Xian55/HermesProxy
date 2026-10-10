# Client packet layouts (CMSG), 2.5.3.42328

Notation: see [the README](README.md).

### — (0x305c)

- Modern: 12380 (0x305c) · 2.4.3: —
- Layout (after the opcode): `guid u32*4`
- Size: 16 bytes (packed GUIDs not counted)

### CMSG_GUILD_PROMOTE_MEMBER (0x305d)

- Modern: 12381 (0x305d) · 2.4.3: 139 (0x8b)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `GuildPromoteMember` — matches

### CMSG_GUILD_DEMOTE_MEMBER (0x305e)

- Modern: 12382 (0x305e) · 2.4.3: 140 (0x8c)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `GuildDemoteMember` — matches

### CMSG_GUILD_ASSIGN_MEMBER_RANK (0x305f)

- Modern: 12383 (0x305f) · 2.4.3: —
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_DECLINE_INVITATION (0x3060)

- Modern: 12384 (0x3060) · 2.4.3: 133 (0x85)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_GUILD_AUTO_DECLINE_INVITATION (0x3061)

- Modern: 12385 (0x3061) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_GUILD_LEAVE (0x3062)

- Modern: 12386 (0x3062) · 2.4.3: 141 (0x8d)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_GUILD_OFFICER_REMOVE_MEMBER (0x3063)

- Modern: 12387 (0x3063) · 2.4.3: 142 (0x8e)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `GuildOfficerRemoveMember` — matches

### CMSG_GUILD_ADD_RANK (0x3064)

- Modern: 12388 (0x3064) · 2.4.3: 562 (0x232)
- Layout (after the opcode): `bits(7) flush u32 bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)
- HermesProxy: `GuildAddRank` — matches

### CMSG_GUILD_DELETE_RANK (0x3065)

- Modern: 12389 (0x3065) · 2.4.3: 563 (0x233)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `GuildDeleteRank` — matches

### CMSG_GUILD_SHIFT_RANK (0x3066)

- Modern: 12390 (0x3066) · 2.4.3: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)

### CMSG_GUILD_SET_RANK_PERMISSIONS (0x3067)

- Modern: 12391 (0x3067) · 2.4.3: 561 (0x231)
- Layout (after the opcode): `u32*4 loop[ u32*2 ] u32 bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 68: 7 · (1 unused)
- HermesProxy: `GuildSetRankPermissions` — matches

### CMSG_GUILD_DELETE (0x3068)

- Modern: 12392 (0x3068) · 2.4.3: 143 (0x8f)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_GUILD_QUERY_MEMBER_RECIPES (0x3069)

- Modern: 12393 (0x3069) · 2.4.3: —
- Layout (after the opcode): `guid guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_QUERY_RECIPES (0x306a)

- Modern: 12394 (0x306a) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_GUILD_QUERY_MEMBERS_FOR_RECIPE (0x306b)

- Modern: 12395 (0x306b) · 2.4.3: —
- Layout (after the opcode): `guid u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_GUILD_QUERY_NEWS (0x306c)

- Modern: 12396 (0x306c) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_GUILD_GET_RANKS (0x306d)

- Modern: 12397 (0x306d) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_GUILD_NEWS_UPDATE_STICKY (0x306e)

- Modern: 12398 (0x306e) · 2.4.3: —
- Layout (after the opcode): `guid u32 flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)

### CMSG_GUILD_SET_ACHIEVEMENT_TRACKING (0x306f)

- Modern: 12399 (0x306f) · 2.4.3: —
- Layout (after the opcode): `u32 loop[ u32 ]`

### CMSG_GUILD_SET_FOCUSED_ACHIEVEMENT (0x3070)

- Modern: 12400 (0x3070) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_GET_ACHIEVEMENT_MEMBERS (0x3071)

- Modern: 12401 (0x3071) · 2.4.3: —
- Layout (after the opcode): `guid guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_SET_MEMBER_NOTE (0x3072)

- Modern: 12402 (0x3072) · 2.4.3: —
- Layout (after the opcode): `guid u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 1 · (7 unused)
- HermesProxy: `GuildSetMemberNote` — matches

### CMSG_GUILD_GET_ROSTER (0x3073)

- Modern: 12403 (0x3073) · 2.4.3: 137 (0x89)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_GUILD_UPDATE_MOTD_TEXT (0x3074)

- Modern: 12404 (0x3074) · 2.4.3: 145 (0x91)
- Layout (after the opcode): `u8 bits(3) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 11 · (5 unused)
- HermesProxy: `GuildUpdateMotdText` — matches

### CMSG_GUILD_UPDATE_INFO_TEXT (0x3075)

- Modern: 12405 (0x3075) · 2.4.3: 764 (0x2fc)
- Layout (after the opcode): `u8 bits(3) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 11 · (5 unused)
- HermesProxy: `GuildUpdateInfoText` — matches

### — (0x3076)

- Modern: 12406 (0x3076) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3077)

- Modern: 12407 (0x3077) · 2.4.3: —
- Layout (after the opcode): `u64`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x3078)

- Modern: 12408 (0x3078) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3079)

- Modern: 12409 (0x3079) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x307a)

- Modern: 12410 (0x307a) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_GUILD_CHALLENGE_UPDATE_REQUEST (0x307b)

- Modern: 12411 (0x307b) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x307c)

- Modern: 12412 (0x307c) · 2.4.3: —
- Layout (after the opcode): `guid u8 bits(2) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 2: 10 · (6 unused)

### — (0x307d)

- Modern: 12413 (0x307d) · 2.4.3: —
- Layout (after the opcode): `guid guid u8 bits(2) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 10 · (6 unused)

### CMSG_GUILD_CHANGE_NAME_REQUEST (0x307e)

- Modern: 12414 (0x307e) · 2.4.3: —
- Layout (after the opcode): `bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)

### — (0x307f)

- Modern: 12415 (0x307f) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x3080)

- Modern: 12416 (0x3080) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3081)

- Modern: 12417 (0x3081) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_GUILD_BANK_LOG_QUERY (0x3082)

- Modern: 12418 (0x3082) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `GuildBankLogQuery` — matches

### CMSG_GUILD_BANK_REMAINING_WITHDRAW_MONEY_QUERY (0x3083)

- Modern: 12419 (0x3083) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_GUILD_PERMISSIONS_QUERY (0x3084)

- Modern: 12420 (0x3084) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_GUILD_EVENT_LOG_QUERY (0x3085)

- Modern: 12421 (0x3085) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_GUILD_BANK_SET_TAB_TEXT (0x3086)

- Modern: 12422 (0x3086) · 2.4.3: 1034 (0x40a)
- Layout (after the opcode): `u32 u8 bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: (1 unused) · 13 · (2 unused)
- HermesProxy: `GuildBankSetTabText` — differs

### CMSG_GUILD_BANK_TEXT_QUERY (0x3087)

- Modern: 12423 (0x3087) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `GuildBankTextQuery` — matches

### CMSG_GUILD_REPLACE_GUILD_MASTER (0x3088)

- Modern: 12424 (0x3088) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3089)

- Modern: 12425 (0x3089) · 2.4.3: —
- Layout (after the opcode): `bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)

### — (0x308a)

- Modern: 12426 (0x308a) · 2.4.3: —
- Layout (after the opcode): `bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)

### — (0x308b)

- Modern: 12427 (0x308b) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x308c)

- Modern: 12428 (0x308c) · 2.4.3: —
- Layout (after the opcode): `guid u8 bits(2) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 2: 10 · (6 unused)

### — (0x308d)

- Modern: 12429 (0x308d) · 2.4.3: —
- Layout (after the opcode): `u64 guid flush`
- Bit fields (message of zeros, widths in order written): byte 10: 1 · (7 unused)

### — (0x308e)

- Modern: 12430 (0x308e) · 2.4.3: —
- Layout (after the opcode): `guid u64`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x308f)

- Modern: 12431 (0x308f) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3090)

- Modern: 12432 (0x3090) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3091)

- Modern: 12433 (0x3091) · 2.4.3: —
- Layout (after the opcode): `f32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3124)

- Modern: 12580 (0x3124) · 2.4.3: —
- Layout (after the opcode): `opt[ u8 ] flush u8`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 1 · (6 unused) · 8

### — (0x3125)

- Modern: 12581 (0x3125) · 2.4.3: —
- Layout (after the opcode): `bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 6 · (2 unused)

### — (0x3126)

- Modern: 12582 (0x3126) · 2.4.3: —
- Layout (after the opcode): `u32 bytes`

### CMSG_TWITTER_CONNECT (0x3127)

- Modern: 12583 (0x3127) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3128)

- Modern: 12584 (0x3128) · 2.4.3: —
- Layout (after the opcode): `u8*2 flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 0: (7 unused) · 1 · (7 unused) · 1

### — (0x3129)

- Modern: 12585 (0x3129) · 2.4.3: —
- Layout (after the opcode): `u32*2 u16 u32*2 loop[ { u8*2 bits(2) flush bytes bytes } ] loop[ { u8*2 bits(2) flush bytes bytes } ]`

### CMSG_TWITTER_CHECK_STATUS (0x312a)

- Modern: 12586 (0x312a) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_TWITTER_DISCONNECT (0x312b)

- Modern: 12587 (0x312b) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PET_CLEAR_FANFARE (0x312c)

- Modern: 12588 (0x312c) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_MOUNT_CLEAR_FANFARE (0x312d)

- Modern: 12589 (0x312d) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_TOY_CLEAR_FANFARE (0x312e)

- Modern: 12590 (0x312e) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `ToyClearFanfare` — matches

### CMSG_CLEAR_NEW_APPEARANCE (0x312f)

- Modern: 12591 (0x312f) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3130)

- Modern: 12592 (0x3130) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3131)

- Modern: 12593 (0x3131) · 2.4.3: —
- Layout (after the opcode): `u64 u32`
- Size: 12 bytes (packed GUIDs not counted)

### — (0x3132)

- Modern: 12594 (0x3132) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3133)

- Modern: 12595 (0x3133) · 2.4.3: —
- Layout (after the opcode): `u64 u32`
- Size: 12 bytes (packed GUIDs not counted)

### — (0x3134)

- Modern: 12596 (0x3134) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3135)

- Modern: 12597 (0x3135) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3136)

- Modern: 12598 (0x3136) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3137)

- Modern: 12599 (0x3137) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3138)

- Modern: 12600 (0x3138) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3139)

- Modern: 12601 (0x3139) · 2.4.3: —
- Layout (after the opcode): `u64 u32`
- Size: 12 bytes (packed GUIDs not counted)

### — (0x313a)

- Modern: 12602 (0x313a) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_INITIATE_TRADE (0x3156)

- Modern: 12630 (0x3156) · 2.4.3: 278 (0x116)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InitiateTrade` — matches

### CMSG_BEGIN_TRADE (0x3157)

- Modern: 12631 (0x3157) · 2.4.3: 279 (0x117)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_BUSY_TRADE (0x3158)

- Modern: 12632 (0x3158) · 2.4.3: 280 (0x118)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_IGNORE_TRADE (0x3159)

- Modern: 12633 (0x3159) · 2.4.3: 281 (0x119)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_ACCEPT_TRADE (0x315a)

- Modern: 12634 (0x315a) · 2.4.3: 282 (0x11a)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `AcceptTrade` — matches

### CMSG_UNACCEPT_TRADE (0x315b)

- Modern: 12635 (0x315b) · 2.4.3: 283 (0x11b)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_CANCEL_TRADE (0x315c)

- Modern: 12636 (0x315c) · 2.4.3: 284 (0x11c)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_SET_TRADE_ITEM (0x315d)

- Modern: 12637 (0x315d) · 2.4.3: 285 (0x11d)
- Layout (after the opcode): `u8*3`
- Size: 3 bytes (packed GUIDs not counted)
- HermesProxy: `SetTradeItem` — matches

### CMSG_CLEAR_TRADE_ITEM (0x315e)

- Modern: 12638 (0x315e) · 2.4.3: 286 (0x11e)
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `ClearTradeItem` — matches

### CMSG_SET_TRADE_GOLD (0x315f)

- Modern: 12639 (0x315f) · 2.4.3: 287 (0x11f)
- Layout (after the opcode): `u64`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `SetTradeGold` — matches

### CMSG_SET_TRADE_CURRENCY (0x3160)

- Modern: 12640 (0x3160) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x3161)

- Modern: 12641 (0x3161) · 2.4.3: —
- Layout (after the opcode): `bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)

### — (0x3162)

- Modern: 12642 (0x3162) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3163)

- Modern: 12643 (0x3163) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3164)

- Modern: 12644 (0x3164) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3165)

- Modern: 12645 (0x3165) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x3166)

- Modern: 12646 (0x3166) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3167)

- Modern: 12647 (0x3167) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x3168)

- Modern: 12648 (0x3168) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_STABLE_PET (0x3169)

- Modern: 12649 (0x3169) · 2.4.3: 624 (0x270)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `StablePet` — matches

### CMSG_UNSTABLE_PET (0x316a)

- Modern: 12650 (0x316a) · 2.4.3: 625 (0x271)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `UnstablePet` — matches

### CMSG_STABLE_SWAP_PET (0x316b)

- Modern: 12651 (0x316b) · 2.4.3: 629 (0x275)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `StableSwapPet` — matches

### CMSG_BUY_STABLE_SLOT (0x316c)

- Modern: 12652 (0x316c) · 2.4.3: 626 (0x272)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `BuyStableSlot` — matches

### — (0x316d)

- Modern: 12653 (0x316d) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x316e)

- Modern: 12654 (0x316e) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x316f)

- Modern: 12655 (0x316f) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3170)

- Modern: 12656 (0x3170) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x3171)

- Modern: 12657 (0x3171) · 2.4.3: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)

### — (0x3172)

- Modern: 12658 (0x3172) · 2.4.3: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)

### — (0x3173)

- Modern: 12659 (0x3173) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3174)

- Modern: 12660 (0x3174) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3175)

- Modern: 12661 (0x3175) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_BATTLEFIELD_LEAVE (0x3176)

- Modern: 12662 (0x3176) · 2.4.3: 737 (0x2e1)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `BattlefieldLeave` — matches

### CMSG_SURRENDER_ARENA (0x3177)

- Modern: 12663 (0x3177) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_QUERY_QUEST_COMPLETION_NPCS (0x3178)

- Modern: 12664 (0x3178) · 2.4.3: —
- Layout (after the opcode): `u32 loop[ u32 ]`

### CMSG_REQUEST_CEMETERY_LIST (0x3179)

- Modern: 12665 (0x3179) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_SET_PREFERRED_CEMETERY (0x317a)

- Modern: 12666 (0x317a) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_JOIN_RATED_BATTLEGROUND (0x317b)

- Modern: 12667 (0x317b) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x317c)

- Modern: 12668 (0x317c) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x317d)

- Modern: 12669 (0x317d) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_INSPECT_HONOR_STATS (0x317e)

- Modern: 12670 (0x317e) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `Inspect` — matches

### CMSG_PVP_LOG_DATA (0x317f)

- Modern: 12671 (0x317f) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `PVPLogDataRequest` — matches

### — (0x3180)

- Modern: 12672 (0x3180) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_REQUEST_CATEGORY_COOLDOWNS (0x3181)

- Modern: 12673 (0x3181) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_BATTLEFIELD_LIST (0x3182)

- Modern: 12674 (0x3182) · 2.4.3: 572 (0x23c)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `BattlefieldListRequest` — matches

### CMSG_CANCEL_QUEUED_SPELL (0x3183)

- Modern: 12675 (0x3183) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_OBJECT_UPDATE_FAILED (0x3184)

- Modern: 12676 (0x3184) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ObjectUpdateFailed` — matches

### CMSG_OBJECT_UPDATE_RESCUED (0x3185)

- Modern: 12677 (0x3185) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3186)

- Modern: 12678 (0x3186) · 2.4.3: —
- Layout (after the opcode): `u8 flush bytes`

### — (0x3187)

- Modern: 12679 (0x3187) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_VIOLENCE_LEVEL (0x3188)

- Modern: 12680 (0x3188) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x3189)

- Modern: 12681 (0x3189) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_USED_FOLLOW (0x318a)

- Modern: 12682 (0x318a) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x318b)

- Modern: 12683 (0x318b) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x318c)

- Modern: 12684 (0x318c) · 2.4.3: —
- Layout (after the opcode): `flush guid`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### CMSG_STAND_STATE_CHANGE (0x318d)

- Modern: 12685 (0x318d) · 2.4.3: 257 (0x101)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `StandStateChange` — matches

### CMSG_MISSILE_TRAJECTORY_COLLISION (0x318e)

- Modern: 12686 (0x318e) · 2.4.3: —
- Layout (after the opcode): `guid u32 guid f32*3`
- Size: 16 bytes (packed GUIDs not counted)

### CMSG_SAVE_CUF_PROFILES (0x318f)

- Modern: 12687 (0x318f) · 2.4.3: —
- Layout (after the opcode): `u32 loop[ { bits(7) opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush u16*2 u8*5 u16*3 bytes } ]`
- HermesProxy: `SaveCUFProfiles` — matches

### — (0x3190)

- Modern: 12688 (0x3190) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3191)

- Modern: 12689 (0x3191) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3192)

- Modern: 12690 (0x3192) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3193)

- Modern: 12691 (0x3193) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x3194)

- Modern: 12692 (0x3194) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x3195)

- Modern: 12693 (0x3195) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x3196)

- Modern: 12694 (0x3196) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3197)

- Modern: 12695 (0x3197) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3198)

- Modern: 12696 (0x3198) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3199)

- Modern: 12697 (0x3199) · 2.4.3: —
- Layout (after the opcode): `u32 guid loop[ u32*4 ] flush`

### — (0x319a)

- Modern: 12698 (0x319a) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x319b)

- Modern: 12699 (0x319b) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x319c)

- Modern: 12700 (0x319c) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x319d)

- Modern: 12701 (0x319d) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x319e)

- Modern: 12702 (0x319e) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x319f)

- Modern: 12703 (0x319f) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x31a0)

- Modern: 12704 (0x31a0) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x31a1)

- Modern: 12705 (0x31a1) · 2.4.3: —
- Layout (after the opcode): `guid flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### — (0x31a2)

- Modern: 12706 (0x31a2) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_UNLOCK_VOID_STORAGE (0x31a3)

- Modern: 12707 (0x31a3) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_QUERY_VOID_STORAGE (0x31a4)

- Modern: 12708 (0x31a4) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_VOID_STORAGE_TRANSFER (0x31a5)

- Modern: 12709 (0x31a5) · 2.4.3: —
- Layout (after the opcode): `guid u32*2 loop[ guid ] loop[ guid ]`

### CMSG_SWAP_VOID_ITEM (0x31a6)

- Modern: 12710 (0x31a6) · 2.4.3: —
- Layout (after the opcode): `guid guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_UNLEARN_SPECIALIZATION (0x31a7)

- Modern: 12711 (0x31a7) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### CMSG_CLEAR_RAID_MARKER (0x31a8)

- Modern: 12712 (0x31a8) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### CMSG_REQUEST_GUILD_REWARDS_LIST (0x31a9)

- Modern: 12713 (0x31a9) · 2.4.3: —
- Layout (after the opcode): `u64`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_REQUEST_GUILD_PARTY_STATE (0x31aa)

- Modern: 12714 (0x31aa) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_QUERY_COUNTDOWN_TIMER (0x31ab)

- Modern: 12715 (0x31ab) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_ARTIFACT_ADD_POWER (0x31ac)

- Modern: 12716 (0x31ac) · 2.4.3: —
- Layout (after the opcode): `guid guid u32 loop[ { u32 u8 } ]`

### CMSG_CONFIRM_ARTIFACT_RESPEC (0x31ad)

- Modern: 12717 (0x31ad) · 2.4.3: —
- Layout (after the opcode): `guid guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_ARTIFACT_SET_APPEARANCE (0x31ae)

- Modern: 12718 (0x31ae) · 2.4.3: —
- Layout (after the opcode): `guid guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CANCEL_MOD_SPEED_NO_CONTROL_AURAS (0x31af)

- Modern: 12719 (0x31af) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_CANCEL_AURA (0x31b0)

- Modern: 12720 (0x31b0) · 2.4.3: 310 (0x136)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `CancelAura` — matches

### — (0x31b1)

- Modern: 12721 (0x31b1) · 2.4.3: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GAME_EVENT_DEBUG_ENABLE (0x31b2)

- Modern: 12722 (0x31b2) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_GAME_EVENT_DEBUG_DISABLE (0x31b3)

- Modern: 12723 (0x31b3) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31b4)

- Modern: 12724 (0x31b4) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31b5)

- Modern: 12725 (0x31b5) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x31b6)

- Modern: 12726 (0x31b6) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x31b7)

- Modern: 12727 (0x31b7) · 2.4.3: —
- Layout (after the opcode): `flush u32`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x31b8)

- Modern: 12728 (0x31b8) · 2.4.3: —
- Layout (after the opcode): `flush u32`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x31b9)

- Modern: 12729 (0x31b9) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x31ba)

- Modern: 12730 (0x31ba) · 2.4.3: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)

### — (0x31bb)

- Modern: 12731 (0x31bb) · 2.4.3: —
- Layout (after the opcode): `u32*2 guid flush`
- Bit fields (message of zeros, widths in order written): byte 10: 1 · (7 unused)

### — (0x31bc)

- Modern: 12732 (0x31bc) · 2.4.3: —
- Layout (after the opcode): `u32*2 loop[ u32 ] loop[ u32*3 loop[ guid ] ]`

### — (0x31bd)

- Modern: 12733 (0x31bd) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x31be)

- Modern: 12734 (0x31be) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x31bf)

- Modern: 12735 (0x31bf) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31c0)

- Modern: 12736 (0x31c0) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31c1)

- Modern: 12737 (0x31c1) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31c2)

- Modern: 12738 (0x31c2) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31c3)

- Modern: 12739 (0x31c3) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31c4)

- Modern: 12740 (0x31c4) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31c5)

- Modern: 12741 (0x31c5) · 2.4.3: —
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)

### — (0x31c6)

- Modern: 12742 (0x31c6) · 2.4.3: —
- Layout (after the opcode): `u32 { bits(7) flush bytes }`
- Bit fields (message of zeros, widths in order written): byte 4: 7 · (1 unused)

### — (0x31c7)

- Modern: 12743 (0x31c7) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31c8)

- Modern: 12744 (0x31c8) · 2.4.3: —
- Layout (after the opcode): `guid { u8 flush bytes }`

### — (0x31c9)

- Modern: 12745 (0x31c9) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x31ca)

- Modern: 12746 (0x31ca) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x31cb)

- Modern: 12747 (0x31cb) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x31cc)

- Modern: 12748 (0x31cc) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x31cd)

- Modern: 12749 (0x31cd) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x31ce)

- Modern: 12750 (0x31ce) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x31cf)

- Modern: 12751 (0x31cf) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x31d0)

- Modern: 12752 (0x31d0) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x31d1)

- Modern: 12753 (0x31d1) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x31d2)

- Modern: 12754 (0x31d2) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x31d3)

- Modern: 12755 (0x31d3) · 2.4.3: —
- Layout (after the opcode): `guid f32*3`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_NEUTRAL_PLAYER_SELECT_FACTION (0x31d4)

- Modern: 12756 (0x31d4) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x31d5)

- Modern: 12757 (0x31d5) · 2.4.3: —
- Layout (after the opcode): `u8 flush u32 bytes`

### — (0x31d6)

- Modern: 12758 (0x31d6) · 2.4.3: —
- Layout (after the opcode): `u8 flush u32 bytes`

### CMSG_AREA_TRIGGER (0x31d7)

- Modern: 12759 (0x31d7) · 2.4.3: 180 (0xb4)
- Layout (after the opcode): `u32 opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · 1 · (6 unused)
- HermesProxy: `AreaTriggerPkt` — matches

### — (0x31d8)

- Modern: 12760 (0x31d8) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31d9)

- Modern: 12761 (0x31d9) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_PET_BATTLE_REQUEST_WILD (0x31da)

- Modern: 12762 (0x31da) · 2.4.3: —
- Layout (after the opcode): `guid { u32 f32*4 loop[ f32*3 ] }`

### CMSG_PET_BATTLE_WILD_LOCATION_FAIL (0x31db)

- Modern: 12763 (0x31db) · 2.4.3: —
- Layout (after the opcode): `guid f32*3`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_PET_BATTLE_REQUEST_PVP (0x31dc)

- Modern: 12764 (0x31dc) · 2.4.3: —
- Layout (after the opcode): `guid { u32 f32*4 loop[ f32*3 ] }`

### CMSG_PET_BATTLE_REQUEST_UPDATE (0x31dd)

- Modern: 12765 (0x31dd) · 2.4.3: —
- Layout (after the opcode): `guid flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_JOIN_PET_BATTLE_QUEUE (0x31de)

- Modern: 12766 (0x31de) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_LEAVE_PET_BATTLE_QUEUE (0x31df)

- Modern: 12767 (0x31df) · 2.4.3: —
- Layout (after the opcode): `{ guid u32*2 u64 }`
- Size: 16 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PET_UPDATE_NOTIFY (0x31e0)

- Modern: 12768 (0x31e0) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PET_UPDATE_DISPLAY_NOTIFY (0x31e1)

- Modern: 12769 (0x31e1) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_PET_BATTLE_QUIT_NOTIFY (0x31e2)

- Modern: 12770 (0x31e2) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_PET_BATTLE_FINAL_NOTIFY (0x31e3)

- Modern: 12771 (0x31e3) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_PET_BATTLE_SCRIPT_ERROR_NOTIFY (0x31e4)

- Modern: 12772 (0x31e4) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31e5)

- Modern: 12773 (0x31e5) · 2.4.3: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)

### — (0x31e6)

- Modern: 12774 (0x31e6) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31e7)

- Modern: 12775 (0x31e7) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31e8)

- Modern: 12776 (0x31e8) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x31e9)

- Modern: 12777 (0x31e9) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x31ea)

- Modern: 12778 (0x31ea) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x31eb)

- Modern: 12779 (0x31eb) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x31ec)

- Modern: 12780 (0x31ec) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x31ed)

- Modern: 12781 (0x31ed) · 2.4.3: —
- Layout (after the opcode): `u8*2`
- Size: 2 bytes (packed GUIDs not counted)

### — (0x31ee)

- Modern: 12782 (0x31ee) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x31ef)

- Modern: 12783 (0x31ef) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x31f0)

- Modern: 12784 (0x31f0) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x31f1)

- Modern: 12785 (0x31f1) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31f2)

- Modern: 12786 (0x31f2) · 2.4.3: —
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)

### — (0x31f3)

- Modern: 12787 (0x31f3) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x31f4)

- Modern: 12788 (0x31f4) · 2.4.3: —
- Layout (after the opcode): `u32*2 u8 flush bytes`

### — (0x31f5)

- Modern: 12789 (0x31f5) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x31f6)

- Modern: 12790 (0x31f6) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x31f7)

- Modern: 12791 (0x31f7) · 2.4.3: —
- Layout (after the opcode): `u8 flush bytes`

### — (0x31f8)

- Modern: 12792 (0x31f8) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31f9)

- Modern: 12793 (0x31f9) · 2.4.3: —
- Layout (after the opcode): `u8 opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 0: 8 · 1 · 1 · (6 unused)

### — (0x31fa)

- Modern: 12794 (0x31fa) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31fb)

- Modern: 12795 (0x31fb) · 2.4.3: —
- Layout (after the opcode): `u32 opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · 1 · (6 unused)

### — (0x31fc)

- Modern: 12796 (0x31fc) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x31fd)

- Modern: 12797 (0x31fd) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31fe)

- Modern: 12798 (0x31fe) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x31ff)

- Modern: 12799 (0x31ff) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3200)

- Modern: 12800 (0x3200) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_ADVENTURE_JOURNAL_OPEN_QUEST (0x3201)

- Modern: 12801 (0x3201) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_RESET_CHALLENGE_MODE (0x3202)

- Modern: 12802 (0x3202) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_RESET_CHALLENGE_MODE_CHEAT (0x3203)

- Modern: 12803 (0x3203) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_REQUEST_MYTHIC_PLUS_AFFIXES (0x3204)

- Modern: 12804 (0x3204) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_REQUEST_MYTHIC_PLUS_SEASON_DATA (0x3205)

- Modern: 12805 (0x3205) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3206)

- Modern: 12806 (0x3206) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_REQUEST_FORCED_REACTIONS (0x3207)

- Modern: 12807 (0x3207) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3208)

- Modern: 12808 (0x3208) · 2.4.3: —
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)

### CMSG_ASSIGN_EQUIPMENT_SET_SPEC (0x3209)

- Modern: 12809 (0x3209) · 2.4.3: —
- Layout (after the opcode): `u64 u32`
- Size: 12 bytes (packed GUIDs not counted)

### — (0x320a)

- Modern: 12810 (0x320a) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CONFIRM_RESPEC_WIPE (0x320b)

- Modern: 12811 (0x320b) · 2.4.3: —
- Layout (after the opcode): `guid u8`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `ConfirmRespecWipe` — matches

### CMSG_LOOT_UNIT (0x320c)

- Modern: 12812 (0x320c) · 2.4.3: 349 (0x15d)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `LootUnit` — matches

### CMSG_LOOT_MONEY (0x320d)

- Modern: 12813 (0x320d) · 2.4.3: 350 (0x15e)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `LootMoney` — matches

### CMSG_LOOT_ITEM (0x320e)

- Modern: 12814 (0x320e) · 2.4.3: —
- Layout (after the opcode): `u32 loop[ { guid u8 } ]`
- HermesProxy: `LootItemPkt` — matches

### CMSG_LOOT_MASTER_GIVE (0x320f)

- Modern: 12815 (0x320f) · 2.4.3: 675 (0x2a3)
- Layout (after the opcode): `u32 guid loop[ { guid u8 } ]`
- HermesProxy: `LootMasterGive` — matches

### CMSG_LOOT_RELEASE (0x3210)

- Modern: 12816 (0x3210) · 2.4.3: 351 (0x15f)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `LootRelease` — matches

### CMSG_LOOT_ROLL (0x3211)

- Modern: 12817 (0x3211) · 2.4.3: 672 (0x2a0)
- Layout (after the opcode): `guid u8*2`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8
- HermesProxy: `LootRoll` — matches

### — (0x3212)

- Modern: 12818 (0x3212) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3213)

- Modern: 12819 (0x3213) · 2.4.3: —
- Layout (after the opcode): `u32*2 { u8 } flush`
- Bit fields (message of zeros, widths in order written): byte 8: 8 · 1 · (7 unused)

### — (0x3214)

- Modern: 12820 (0x3214) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x3215)

- Modern: 12821 (0x3215) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3216)

- Modern: 12822 (0x3216) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3217)

- Modern: 12823 (0x3217) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3218)

- Modern: 12824 (0x3218) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3219)

- Modern: 12825 (0x3219) · 2.4.3: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)

### — (0x321a)

- Modern: 12826 (0x321a) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x321b)

- Modern: 12827 (0x321b) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_SCENE_PLAYBACK_COMPLETE (0x321c)

- Modern: 12828 (0x321c) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SCENE_PLAYBACK_CANCELED (0x321d)

- Modern: 12829 (0x321d) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SCENE_TRIGGER_EVENT (0x321e)

- Modern: 12830 (0x321e) · 2.4.3: —
- Layout (after the opcode): `bits(6) flush u32 bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 6 · (2 unused)

### CMSG_SET_DIFFICULTY_ID (0x321f)

- Modern: 12831 (0x321f) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_KEYBOUND_OVERRIDE (0x3220)

- Modern: 12832 (0x3220) · 2.4.3: —
- Layout (after the opcode): `*2`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_PET_BATTLE_QUEUE_PROPOSE_MATCH_RESULT (0x3221)

- Modern: 12833 (0x3221) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### CMSG_MAIL_DELETE (0x3222)

- Modern: 12834 (0x3222) · 2.4.3: 585 (0x249)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `MailDelete` — matches

### CMSG_SET_ACHIEVEMENTS_HIDDEN (0x3223)

- Modern: 12835 (0x3223) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### CMSG_MAKE_CONTITIONAL_APPEARANCE_PERMANENT (0x3224)

- Modern: 12836 (0x3224) · 2.4.3: —
- Layout (after the opcode): `guid guid u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### — (0x3225)

- Modern: 12837 (0x3225) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3226)

- Modern: 12838 (0x3226) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x3227)

- Modern: 12839 (0x3227) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3228)

- Modern: 12840 (0x3228) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3229)

- Modern: 12841 (0x3229) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x322a)

- Modern: 12842 (0x322a) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x322b)

- Modern: 12843 (0x322b) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x322c)

- Modern: 12844 (0x322c) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x322d)

- Modern: 12845 (0x322d) · 2.4.3: —
- Layout (after the opcode): `u32*2 flush`
- Bit fields (message of zeros, widths in order written): byte 8: 1 · (7 unused)

### — (0x322e)

- Modern: 12846 (0x322e) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x322f)

- Modern: 12847 (0x322f) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3230)

- Modern: 12848 (0x3230) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3231)

- Modern: 12849 (0x3231) · 2.4.3: —
- Layout (after the opcode): `guid flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### CMSG_REQUEST_VEHICLE_EXIT (0x3232)

- Modern: 12850 (0x3232) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `RequestVehicleSeatChange` — matches

### CMSG_REQUEST_VEHICLE_PREV_SEAT (0x3233)

- Modern: 12851 (0x3233) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `RequestVehicleSeatChange` — matches

### CMSG_REQUEST_VEHICLE_NEXT_SEAT (0x3234)

- Modern: 12852 (0x3234) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `RequestVehicleSeatChange` — matches

### CMSG_REQUEST_VEHICLE_SWITCH_SEAT (0x3235)

- Modern: 12853 (0x3235) · 2.4.3: —
- Layout (after the opcode): `guid u8`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `RequestVehicleSwitchSeat` — matches

### CMSG_RIDE_VEHICLE_INTERACT (0x3236)

- Modern: 12854 (0x3236) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `RideVehicleInteract` — matches

### CMSG_EJECT_PASSENGER (0x3237)

- Modern: 12855 (0x3237) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EjectPassenger` — matches

### — (0x3238)

- Modern: 12856 (0x3238) · 2.4.3: —
- Layout (after the opcode): `*2 u8`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x3239)

- Modern: 12857 (0x3239) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x323a)

- Modern: 12858 (0x323a) · 2.4.3: —
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x323b)

- Modern: 12859 (0x323b) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_USE_CRITTER_ITEM (0x323c)

- Modern: 12860 (0x323c) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x323d)

- Modern: 12861 (0x323d) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x323e)

- Modern: 12862 (0x323e) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x323f)

- Modern: 12863 (0x323f) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3240)

- Modern: 12864 (0x3240) · 2.4.3: —
- Layout (after the opcode): `u64*2 u32`
- Size: 20 bytes (packed GUIDs not counted)

### — (0x3241)

- Modern: 12865 (0x3241) · 2.4.3: —
- Layout (after the opcode): `u64 u32`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_CHECK_IS_ADVENTURE_MAP_POI_VALID (0x3242)

- Modern: 12866 (0x3242) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3243)

- Modern: 12867 (0x3243) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3244)

- Modern: 12868 (0x3244) · 2.4.3: —
- Layout (after the opcode): `u32 f32 u32`
- Size: 12 bytes (packed GUIDs not counted)

### — (0x3245)

- Modern: 12869 (0x3245) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3246)

- Modern: 12870 (0x3246) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3247)

- Modern: 12871 (0x3247) · 2.4.3: —
- Layout (after the opcode): `guid loop[ u32 ] u32 u64`

### — (0x3248)

- Modern: 12872 (0x3248) · 2.4.3: —
- Layout (after the opcode): `u32 opt[ u8 ] u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · 1 · 8 · (6 unused)

### — (0x3249)

- Modern: 12873 (0x3249) · 2.4.3: —
- Layout (after the opcode): `f32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x324a)

- Modern: 12874 (0x324a) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x324b)

- Modern: 12875 (0x324b) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x324c)

- Modern: 12876 (0x324c) · 2.4.3: —
- Layout (after the opcode): `u8 flush u32 bytes`

### — (0x324d)

- Modern: 12877 (0x324d) · 2.4.3: —
- Layout (after the opcode): `u8 flush bytes`

### — (0x324e)

- Modern: 12878 (0x324e) · 2.4.3: —
- Layout (after the opcode): `u32*2 flush`
- Bit fields (message of zeros, widths in order written): byte 8: 1 · (7 unused)

### — (0x324f)

- Modern: 12879 (0x324f) · 2.4.3: —
- Layout (after the opcode): `guid guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_ATTACK_SWING (0x3250)

- Modern: 12880 (0x3250) · 2.4.3: 321 (0x141)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `AttackSwing` — matches

### CMSG_ATTACK_STOP (0x3251)

- Modern: 12881 (0x3251) · 2.4.3: 322 (0x142)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `AttackStop` — matches

### — (0x3252)

- Modern: 12882 (0x3252) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3253)

- Modern: 12883 (0x3253) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3254)

- Modern: 12884 (0x3254) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3255)

- Modern: 12885 (0x3255) · 2.4.3: —
- Layout (after the opcode): `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x3256)

- Modern: 12886 (0x3256) · 2.4.3: —
- Layout (after the opcode): `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x3257)

- Modern: 12887 (0x3257) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x3258)

- Modern: 12888 (0x3258) · 2.4.3: —
- Layout (after the opcode): `flush guid`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x3259)

- Modern: 12889 (0x3259) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x325a)

- Modern: 12890 (0x325a) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x325b)

- Modern: 12891 (0x325b) · 2.4.3: —
- Layout (after the opcode): `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x325c)

- Modern: 12892 (0x325c) · 2.4.3: —
- Layout (after the opcode): `guid flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### — (0x325d)

- Modern: 12893 (0x325d) · 2.4.3: —
- Layout (after the opcode): `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x325e)

- Modern: 12894 (0x325e) · 2.4.3: —
- Layout (after the opcode): `guid flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### — (0x325f)

- Modern: 12895 (0x325f) · 2.4.3: —
- Layout (after the opcode): `guid u32 flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)

### — (0x3260)

- Modern: 12896 (0x3260) · 2.4.3: —
- Layout (after the opcode): `u32 f32*3`
- Size: 16 bytes (packed GUIDs not counted)

### — (0x3261)

- Modern: 12897 (0x3261) · 2.4.3: —
- Layout (after the opcode): `guid guid u32*2 u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 12: 8 · 1 · (7 unused)

### — (0x3262)

- Modern: 12898 (0x3262) · 2.4.3: —
- Layout (after the opcode): `guid u32 u8 flush bytes`

### — (0x3263)

- Modern: 12899 (0x3263) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3264)

- Modern: 12900 (0x3264) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_CANCEL_CHANNELLING (0x3265)

- Modern: 12901 (0x3265) · 2.4.3: 315 (0x13b)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `CancelChannelling` — matches

### — (0x3266)

- Modern: 12902 (0x3266) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3267)

- Modern: 12903 (0x3267) · 2.4.3: —
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3268)

- Modern: 12904 (0x3268) · 2.4.3: —
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3269)

- Modern: 12905 (0x3269) · 2.4.3: —
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CANCEL_GROWTH_AURA (0x326a)

- Modern: 12906 (0x326a) · 2.4.3: 667 (0x29b)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_QUERY_CREATURE (0x326b)

- Modern: 12907 (0x326b) · 2.4.3: 96 (0x60)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `QueryCreature` — matches

### CMSG_QUERY_GAME_OBJECT (0x326c)

- Modern: 12908 (0x326c) · 2.4.3: 94 (0x5e)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `QueryGameObject` — matches

### CMSG_QUERY_NPC_TEXT (0x326d)

- Modern: 12909 (0x326d) · 2.4.3: 383 (0x17f)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `QueryNPCText` — matches

### CMSG_QUERY_QUEST_INFO (0x326e)

- Modern: 12910 (0x326e) · 2.4.3: 92 (0x5c)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `QueryQuestInfo` — matches

### CMSG_QUERY_PAGE_TEXT (0x326f)

- Modern: 12911 (0x326f) · 2.4.3: 90 (0x5a)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `QueryPageText` — matches

### CMSG_QUERY_PET_NAME (0x3270)

- Modern: 12912 (0x3270) · 2.4.3: 82 (0x52)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `QueryPetName` — matches

### CMSG_QUERY_BATTLE_PET_NAME (0x3271)

- Modern: 12913 (0x3271) · 2.4.3: —
- Layout (after the opcode): `guid guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_QUERY_PETITION (0x3272)

- Modern: 12914 (0x3272) · 2.4.3: 454 (0x1c6)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `QueryPetition` — matches

### — (0x3273)

- Modern: 12915 (0x3273) · 2.4.3: —
- Layout (after the opcode): `guid u32 flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)

### — (0x3274)

- Modern: 12916 (0x3274) · 2.4.3: —
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_REQUEST_PLAYED_TIME (0x3275)

- Modern: 12917 (0x3275) · 2.4.3: 460 (0x1cc)
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `RequestPlayedTime` — matches

### — (0x3276)

- Modern: 12918 (0x3276) · 2.4.3: —
- Layout (after the opcode): `guid u8 bits(2) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 2: 10 · (6 unused)

### — (0x3277)

- Modern: 12919 (0x3277) · 2.4.3: —
- Layout (after the opcode): `{ u32*8 opt[ u8 ] u8 bits(2) opt[ u8 ] opt[ u8 ] flush { bits(6) opt[ alt[ bits(7) alt[ bits(6) alt[ bits(5) alt[ bits(4) alt[ bits(3) alt[ bits(2) ] alt[ u8 ] ] ] ] ] ] ] loop[ u32 u8 ] } bytes opt[ u64 ] }`
- Bit fields (message of zeros, widths in order written): byte 32: 1 · 1 · 10 · 1 · 1 · (2 unused) · 6 · (2 unused)

### — (0x3278)

- Modern: 12920 (0x3278) · 2.4.3: —
- Layout (after the opcode): `u32*2 opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 8: 1 · 1 · (6 unused)

### CMSG_SET_TITLE (0x3279)

- Modern: 12921 (0x3279) · 2.4.3: 884 (0x374)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `SetTitle` — matches

### CMSG_CANCEL_MOUNT_AURA (0x327a)

- Modern: 12922 (0x327a) · 2.4.3: 885 (0x375)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_MOUNT_SPECIAL_ANIM (0x327b)

- Modern: 12923 (0x327b) · 2.4.3: 369 (0x171)
- Layout (after the opcode): `{ u32*2 loop[ u32 ] }`
- HermesProxy: `MountSpecial` — matches

### — (0x327c)

- Modern: 12924 (0x327c) · 2.4.3: —
- Layout (after the opcode): `guid u64`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x327d)

- Modern: 12925 (0x327d) · 2.4.3: —
- Layout (after the opcode): `guid { u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)

### — (0x327e)

- Modern: 12926 (0x327e) · 2.4.3: —
- Layout (after the opcode): `guid { u32 u8 }`
- Size: 5 bytes (packed GUIDs not counted)

### — (0x327f)

- Modern: 12927 (0x327f) · 2.4.3: —
- Layout (after the opcode): `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x3280)

- Modern: 12928 (0x3280) · 2.4.3: —
- Layout (after the opcode): `guid flush opt[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)

### — (0x3281)

- Modern: 12929 (0x3281) · 2.4.3: —
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3282)

- Modern: 12930 (0x3282) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x3283)

- Modern: 12931 (0x3283) · 2.4.3: —
- Layout (after the opcode): `guid bits(2) flush`
- Bit fields (message of zeros, widths in order written): byte 2: 2 · (6 unused)

### — (0x3284)

- Modern: 12932 (0x3284) · 2.4.3: —
- Layout (after the opcode): `guid u32*2 flush`
- Bit fields (message of zeros, widths in order written): byte 10: 1 · (7 unused)

### — (0x3285)

- Modern: 12933 (0x3285) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3286)

- Modern: 12934 (0x3286) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3287)

- Modern: 12935 (0x3287) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3288)

- Modern: 12936 (0x3288) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x3289)

- Modern: 12937 (0x3289) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x328a)

- Modern: 12938 (0x328a) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_DESTROY_ITEM (0x328b)

- Modern: 12939 (0x328b) · 2.4.3: 273 (0x111)
- Layout (after the opcode): `u32 u8*2`
- Size: 6 bytes (packed GUIDs not counted)
- HermesProxy: `DestroyItem` — matches

### — (0x328c)

- Modern: 12940 (0x328c) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x328d)

- Modern: 12941 (0x328d) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x328e)

- Modern: 12942 (0x328e) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GET_MIRROR_IMAGE_DATA (0x328f)

- Modern: 12943 (0x328f) · 2.4.3: 1024 (0x400)
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_USE_ITEM (0x3290)

- Modern: 12944 (0x3290) · 2.4.3: 171 (0xab)
- Layout (after the opcode): `u8*2 guid { guid loop[ u32 ] u32*2 f32*2 guid u32*2 loop[ { u32*2 } ] loop[ { u32*2 } ] bits(5) opt[ u8 ] bits(2) flush { { loop[ opt[ u8 ] alt[ u8 ] ] bits(2) } opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] bits(7) flush guid guid opt[ guid f32*3 ] opt[ guid f32*3 ] opt[ f32 ] opt[ u32 ] bytes } opt[ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } ] loop[ bits(2) flush u32*2 ] }`
- Bit fields (message of zeros, widths in order written): byte 0: 8 · 8; byte 40: 5 · 1 · 2 · 26; byte 44: 1 · 1 · 1 · 1 · 7 · (3 unused)
- HermesProxy: `UseItem` — differs

### — (0x3291)

- Modern: 12945 (0x3291) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3292)

- Modern: 12946 (0x3292) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32*2 f32*2 guid u32*2 loop[ { u32*2 } ] loop[ { u32*2 } ] bits(5) opt[ u8 ] bits(2) flush { { loop[ opt[ u8 ] alt[ u8 ] ] bits(2) } opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] bits(7) flush guid guid opt[ guid f32*3 ] opt[ guid f32*3 ] opt[ f32 ] opt[ u32 ] bytes } opt[ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } ] loop[ bits(2) flush u32*2 ] }`
- Bit fields (message of zeros, widths in order written): byte 36: 5 · 1 · 2 · 26; byte 40: 1 · 1 · 1 · 1 · 7 · (3 unused)

### CMSG_PET_CAST_SPELL (0x3293)

- Modern: 12947 (0x3293) · 2.4.3: 496 (0x1f0)
- Layout (after the opcode): `guid { guid loop[ u32 ] u32*2 f32*2 guid u32*2 loop[ { u32*2 } ] loop[ { u32*2 } ] bits(5) opt[ u8 ] bits(2) flush { { loop[ opt[ u8 ] alt[ u8 ] ] bits(2) } opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] bits(7) flush guid guid opt[ guid f32*3 ] opt[ guid f32*3 ] opt[ f32 ] opt[ u32 ] bytes } opt[ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } ] loop[ bits(2) flush u32*2 ] }`
- Bit fields (message of zeros, widths in order written): byte 38: 5 · 1 · 2 · 26; byte 42: 1 · 1 · 1 · 1 · 7 · (3 unused)
- HermesProxy: `PetCastSpell` — differs

### CMSG_CAST_SPELL (0x3294)

- Modern: 12948 (0x3294) · 2.4.3: 302 (0x12e)
- Layout (after the opcode): `{ guid loop[ u32 ] u32*2 f32*2 guid u32*2 loop[ { u32*2 } ] loop[ { u32*2 } ] bits(5) opt[ u8 ] bits(2) flush { { loop[ opt[ u8 ] alt[ u8 ] ] bits(2) } opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] bits(7) flush guid guid opt[ guid f32*3 ] opt[ guid f32*3 ] opt[ f32 ] opt[ u32 ] bytes } opt[ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } ] loop[ bits(2) flush u32*2 ] }`
- Bit fields (message of zeros, widths in order written): byte 36: 5 · 1 · 2 · 26; byte 40: 1 · 1 · 1 · 1 · 7 · (3 unused)
- HermesProxy: `CastSpell` — differs

### CMSG_UPDATE_SPELL_VISUAL (0x3295)

- Modern: 12949 (0x3295) · 2.4.3: —
- Layout (after the opcode): `u32*2 guid`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_UPDATE_AREA_TRIGGER_VISUAL (0x3296)

- Modern: 12950 (0x3296) · 2.4.3: —
- Layout (after the opcode): `u32*2 guid`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_CANCEL_CAST (0x3297)

- Modern: 12951 (0x3297) · 2.4.3: 303 (0x12f)
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `CancelCast` — matches

### — (0x3298)

- Modern: 12952 (0x3298) · 2.4.3: —
- Layout (after the opcode): `guid u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 2: 9 · (7 unused)

### CMSG_CHOICE_RESPONSE (0x3299)

- Modern: 12953 (0x3299) · 2.4.3: —
- Layout (after the opcode): `u32*2 flush`
- Bit fields (message of zeros, widths in order written): byte 8: 1 · (7 unused)

### CMSG_CLOSE_QUEST_CHOICE (0x329a)

- Modern: 12954 (0x329a) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x329b)

- Modern: 12955 (0x329b) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_REQUEST_LFG_LIST_BLACKLIST (0x329c)

- Modern: 12956 (0x329c) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### — (0x329d)

- Modern: 12957 (0x329d) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x329e)

- Modern: 12958 (0x329e) · 2.4.3: —
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x329f)

- Modern: 12959 (0x329f) · 2.4.3: —
- Layout (after the opcode): `u64`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_SAVE_GUILD_EMBLEM (0x32a0)

- Modern: 12960 (0x32a0) · 2.4.3: —
- Layout (after the opcode): `guid u32*5`
- Size: 20 bytes (packed GUIDs not counted)
- HermesProxy: `SaveGuildEmblem` — matches

### CMSG_TABARD_VENDOR_ACTIVATE (0x32a1)

- Modern: 12961 (0x32a1) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### — (0x32a2)

- Modern: 12962 (0x32a2) · 2.4.3: —
- Layout (after the opcode): `bits(3) flush u32 opt[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 3 · (5 unused)

### CMSG_TOGGLE_PVP (0x32a3)

- Modern: 12963 (0x32a3) · 2.4.3: 595 (0x253)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_SET_PVP (0x32a4)

- Modern: 12964 (0x32a4) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `SetPvP` — matches

### CMSG_SET_WAR_MODE (0x32a5)

- Modern: 12965 (0x32a5) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x32a6)

- Modern: 12966 (0x32a6) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x32a7)

- Modern: 12967 (0x32a7) · 2.4.3: —
- Layout (after the opcode): `flush opt[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x32a8)

- Modern: 12968 (0x32a8) · 2.4.3: —
- Layout (after the opcode): `u32 f32 bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 8: 1 · 6 · (1 unused)

### CMSG_BATTLEMASTER_HELLO (0x32a9)

- Modern: 12969 (0x32a9) · 2.4.3: 727 (0x2d7)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### — (0x32aa)

- Modern: 12970 (0x32aa) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x32ab)

- Modern: 12971 (0x32ab) · 2.4.3: —
- Layout (after the opcode): `opt[ u8 bits(2) ] flush opt[ u32 bytes ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (5 unused) · 1 · (1 unused)

### CMSG_REQUEST_CONQUEST_FORMULA_CONSTANTS (0x32ac)

- Modern: 12972 (0x32ac) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_SET_ADVANCED_COMBAT_LOGGING (0x32ad)

- Modern: 12973 (0x32ad) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x32ae)

- Modern: 12974 (0x32ae) · 2.4.3: —
- Layout (after the opcode): `bits(6) opt[ u8 ] flush f32*4 u32 bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 6 · 1 · (1 unused)

### — (0x32af)

- Modern: 12975 (0x32af) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x32b0)

- Modern: 12976 (0x32b0) · 2.4.3: —
- Layout (after the opcode): `flush u32 loop[ u16 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x32b1)

- Modern: 12977 (0x32b1) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x32b2)

- Modern: 12978 (0x32b2) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x32b3)

- Modern: 12979 (0x32b3) · 2.4.3: —
- Layout (after the opcode): `{ opt[ u8 ] flush f32*5 u32*3 loop[ guid f32*2 u32 flush ] }`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 1 · (6 unused)

### — (0x32b4)

- Modern: 12980 (0x32b4) · 2.4.3: —
- Layout (after the opcode): `u32*3 loop[ u32 ] opt[ u8 ] opt[ u8 ] flush`

### — (0x32b5)

- Modern: 12981 (0x32b5) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x32b6)

- Modern: 12982 (0x32b6) · 2.4.3: —
- Layout (after the opcode): `guid u32 flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)

### — (0x32b7)

- Modern: 12983 (0x32b7) · 2.4.3: —
- Layout (after the opcode): `guid u32 f32*2`
- Size: 12 bytes (packed GUIDs not counted)

### — (0x32b8)

- Modern: 12984 (0x32b8) · 2.4.3: —
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x32b9)

- Modern: 12985 (0x32b9) · 2.4.3: —
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x32ba)

- Modern: 12986 (0x32ba) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x32bb)

- Modern: 12987 (0x32bb) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x32bc)

- Modern: 12988 (0x32bc) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_ITEM_TEXT_QUERY (0x32bd)

- Modern: 12989 (0x32bd) · 2.4.3: 579 (0x243)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ItemTextQuery` — matches

### CMSG_OPEN_ITEM (0x32be)

- Modern: 12990 (0x32be) · 2.4.3: 172 (0xac)
- Layout (after the opcode): `u8*2`
- Size: 2 bytes (packed GUIDs not counted)
- HermesProxy: `OpenItem` — matches

### CMSG_READ_ITEM (0x32bf)

- Modern: 12991 (0x32bf) · 2.4.3: 173 (0xad)
- Layout (after the opcode): `u8*2`
- Size: 2 bytes (packed GUIDs not counted)
- HermesProxy: `ReadItem` — matches

### CMSG_CHANGE_BAG_SLOT_FLAG (0x32c0)

- Modern: 12992 (0x32c0) · 2.4.3: —
- Layout (after the opcode): `u32*2 flush`
- Bit fields (message of zeros, widths in order written): byte 8: 1 · (7 unused)

### CMSG_CHANGE_BANK_BAG_SLOT_FLAG (0x32c1)

- Modern: 12993 (0x32c1) · 2.4.3: —
- Layout (after the opcode): `u32*2 flush`
- Bit fields (message of zeros, widths in order written): byte 8: 1 · (7 unused)

### — (0x32c2)

- Modern: 12994 (0x32c2) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x32c3)

- Modern: 12995 (0x32c3) · 2.4.3: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x32c4)

- Modern: 12996 (0x32c4) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x32c5)

- Modern: 12997 (0x32c5) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x32c6)

- Modern: 12998 (0x32c6) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x32c7)

- Modern: 12999 (0x32c7) · 2.4.3: —
- Layout (after the opcode): `u32*8 loop[ u8 ] loop[ u32 ] loop[ u8 ] loop[ u32 ]`

### — (0x32c8)

- Modern: 13000 (0x32c8) · 2.4.3: —
- Layout (after the opcode): `u32 opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · 1 · (6 unused)

### — (0x32c9)

- Modern: 13001 (0x32c9) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x32ca)

- Modern: 13002 (0x32ca) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x32cb)

- Modern: 13003 (0x32cb) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x32cc)

- Modern: 13004 (0x32cc) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x32cd)

- Modern: 13005 (0x32cd) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x32ce)

- Modern: 13006 (0x32ce) · 2.4.3: —
- Layout (after the opcode): `guid u8`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x32cf)

- Modern: 13007 (0x32cf) · 2.4.3: —
- Layout (after the opcode): `u32 bytes`

### — (0x32d0)

- Modern: 13008 (0x32d0) · 2.4.3: —
- Layout (after the opcode): `u32 guid flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)

### — (0x32d1)

- Modern: 13009 (0x32d1) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x32d2)

- Modern: 13010 (0x32d2) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x32d3)

- Modern: 13011 (0x32d3) · 2.4.3: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)

### CMSG_QUERY_TREASURE_PICKER (0x32d4)

- Modern: 13012 (0x32d4) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_REQUEST_WORLD_QUEST_UPDATE (0x32d5)

- Modern: 13013 (0x32d5) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_REQUEST_AREA_POI_UPDATE (0x32d6)

- Modern: 13014 (0x32d6) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_REMOVE_NEW_ITEM (0x32d7)

- Modern: 13015 (0x32d7) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x32d8)

- Modern: 13016 (0x32d8) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x32d9)

- Modern: 13017 (0x32d9) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x32da)

- Modern: 13018 (0x32da) · 2.4.3: —
- Layout (after the opcode): `u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### — (0x32db)

- Modern: 13019 (0x32db) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x32dc)

- Modern: 13020 (0x32dc) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x32dd)

- Modern: 13021 (0x32dd) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x32de)

- Modern: 13022 (0x32de) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x32df)

- Modern: 13023 (0x32df) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x32e0)

- Modern: 13024 (0x32e0) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x32e1)

- Modern: 13025 (0x32e1) · 2.4.3: —
- Layout (after the opcode): `u32 guid { f32*4 u32*2 } u32 flush`
- Bit fields (message of zeros, widths in order written): byte 34: 1 · (7 unused)

### — (0x32e2)

- Modern: 13026 (0x32e2) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x32e3)

- Modern: 13027 (0x32e3) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x32e4)

- Modern: 13028 (0x32e4) · 2.4.3: —
- Layout (after the opcode): `bits(2) flush u32`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused)

### — (0x32e5)

- Modern: 13029 (0x32e5) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x32e6)

- Modern: 13030 (0x32e6) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x32e7)

- Modern: 13031 (0x32e7) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x32e8)

- Modern: 13032 (0x32e8) · 2.4.3: —
- Layout (after the opcode): `{ loop[ u32 ] f32 u32 u8 bits(2) opt[ u8 ] alt[ u8 ] opt[ u8 ] opt[ u8 ] alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush bytes bytes bytes opt[ u32 ] }`
- Bit fields (message of zeros, widths in order written): byte 20: 10 · 9 · 8 · 1 · 1 · 1 · 1 · (1 unused)

### — (0x32e9)

- Modern: 13033 (0x32e9) · 2.4.3: —
- Layout (after the opcode): `{ guid u32*2 u64 } { loop[ u32 ] f32 u32 u8 bits(2) opt[ u8 ] alt[ u8 ] opt[ u8 ] opt[ u8 ] alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush bytes bytes bytes opt[ u32 ] }`
- Bit fields (message of zeros, widths in order written): byte 38: 10 · 9 · 8 · 1 · 1 · 1 · 1 · (1 unused)

### — (0x32ea)

- Modern: 13034 (0x32ea) · 2.4.3: —
- Layout (after the opcode): `u32*2 u8*2`
- Size: 10 bytes (packed GUIDs not counted)

### — (0x32eb)

- Modern: 13035 (0x32eb) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x32ec)

- Modern: 13036 (0x32ec) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x32ed)

- Modern: 13037 (0x32ed) · 2.4.3: —
- Layout (after the opcode): `u32 bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 6 · (2 unused)

### — (0x32ee)

- Modern: 13038 (0x32ee) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x32ef)

- Modern: 13039 (0x32ef) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x32f0)

- Modern: 13040 (0x32f0) · 2.4.3: —
- Layout (after the opcode): `u32 f32*2 u32*2 u8`
- Size: 21 bytes (packed GUIDs not counted)

### — (0x32f1)

- Modern: 13041 (0x32f1) · 2.4.3: —
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_REPORT_SERVER_LAG (0x32f2)

- Modern: 13042 (0x32f2) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x32f3)

- Modern: 13043 (0x32f3) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_OFFER_PETITION (0x32f4)

- Modern: 13044 (0x32f4) · 2.4.3: 451 (0x1c3)
- Layout (after the opcode): `u32 guid guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `OfferPetition` — matches

### — (0x32f5)

- Modern: 13045 (0x32f5) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x32f6)

- Modern: 13046 (0x32f6) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x32f7)

- Modern: 13047 (0x32f7) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x32f8)

- Modern: 13048 (0x32f8) · 2.4.3: —
- Layout (after the opcode): `u32*2 opt[ u8 ] opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 8: 1 · 1 · 1 · (5 unused)

### — (0x32f9)

- Modern: 13049 (0x32f9) · 2.4.3: —
- Layout (after the opcode): `u64*3 u32*2`
- Size: 32 bytes (packed GUIDs not counted)

### — (0x32fa)

- Modern: 13050 (0x32fa) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3476)

- Modern: 13430 (0x3476) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x3477)

- Modern: 13431 (0x3477) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x3478)

- Modern: 13432 (0x3478) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3479)

- Modern: 13433 (0x3479) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x347a)

- Modern: 13434 (0x347a) · 2.4.3: —
- Layout (after the opcode): `u64 u32`
- Size: 12 bytes (packed GUIDs not counted)

### — (0x347b)

- Modern: 13435 (0x347b) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x347c)

- Modern: 13436 (0x347c) · 2.4.3: —
- Layout (after the opcode): `u32*2 u8 bits(3) opt[ u8 ] flush bytes opt[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 8: (10 unused) · 1 · 1 · (4 unused)

### — (0x347d)

- Modern: 13437 (0x347d) · 2.4.3: —
- Layout (after the opcode): `u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 10 · 10 · (4 unused)

### — (0x347e)

- Modern: 13438 (0x347e) · 2.4.3: —
- Layout (after the opcode): `u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 10 · 10 · (4 unused)

### — (0x347f)

- Modern: 13439 (0x347f) · 2.4.3: —
- Layout (after the opcode): `u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) flush u32 bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 10 · 10 · (4 unused)

### — (0x3480)

- Modern: 13440 (0x3480) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3481)

- Modern: 13441 (0x3481) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3482)

- Modern: 13442 (0x3482) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3483)

- Modern: 13443 (0x3483) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3484)

- Modern: 13444 (0x3484) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x3485)

- Modern: 13445 (0x3485) · 2.4.3: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3486)

- Modern: 13446 (0x3486) · 2.4.3: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3487)

- Modern: 13447 (0x3487) · 2.4.3: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SEND_TEXT_EMOTE (0x3488)

- Modern: 13448 (0x3488) · 2.4.3: 260 (0x104)
- Layout (after the opcode): `guid u32*2 { u32*2 loop[ u32 ] }`
- HermesProxy: `CTextEmote` — matches

### CMSG_SET_SHEATHED (0x3489)

- Modern: 13449 (0x3489) · 2.4.3: 480 (0x1e0)
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)
- HermesProxy: `SetSheathed` — matches

### CMSG_PET_SET_ACTION (0x348a)

- Modern: 13450 (0x348a) · 2.4.3: 372 (0x174)
- Layout (after the opcode): `guid u32*2 flush opt[ u32*2 ]`
- Bit fields (message of zeros, widths in order written): byte 10: 1 · (7 unused)
- HermesProxy: `PetSetAction` — matches

### CMSG_PET_ACTION (0x348b)

- Modern: 13451 (0x348b) · 2.4.3: 373 (0x175)
- Layout (after the opcode): `guid u32 guid f32*3`
- Size: 16 bytes (packed GUIDs not counted)
- HermesProxy: `PetAction` — matches

### CMSG_PET_STOP_ATTACK (0x348c)

- Modern: 13452 (0x348c) · 2.4.3: 746 (0x2ea)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `PetStopAttack` — matches

### CMSG_PET_ABANDON (0x348d)

- Modern: 13453 (0x348d) · 2.4.3: 374 (0x176)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `PetAbandon` — matches

### CMSG_PET_CANCEL_AURA (0x348e)

- Modern: 13454 (0x348e) · 2.4.3: 619 (0x26b)
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `PetCancelAura` — matches

### CMSG_PET_SPELL_AUTOCAST (0x348f)

- Modern: 13455 (0x348f) · 2.4.3: 755 (0x2f3)
- Layout (after the opcode): `guid u32 flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)

### CMSG_REQUEST_PET_INFO (0x3490)

- Modern: 13456 (0x3490) · 2.4.3: 633 (0x279)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_REQUEST_STABLED_PETS (0x3491)

- Modern: 13457 (0x3491) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `RequestStabledPets` — matches

### CMSG_TALK_TO_GOSSIP (0x3492)

- Modern: 13458 (0x3492) · 2.4.3: 379 (0x17b)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_CLOSE_INTERACTION (0x3493)

- Modern: 13459 (0x3493) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `CloseInteraction` — matches

### CMSG_GOSSIP_SELECT_OPTION (0x3494)

- Modern: 13460 (0x3494) · 2.4.3: 380 (0x17c)
- Layout (after the opcode): `guid u32*2 u8 flush bytes`
- HermesProxy: `GossipSelectOption` — matches

### CMSG_SPELL_CLICK (0x3495)

- Modern: 13461 (0x3495) · 2.4.3: 1015 (0x3f7)
- Layout (after the opcode): `guid flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_QUEST_GIVER_HELLO (0x3496)

- Modern: 13462 (0x3496) · 2.4.3: 388 (0x184)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `QuestGiverHello` — matches

### CMSG_QUEST_GIVER_QUERY_QUEST (0x3497)

- Modern: 13463 (0x3497) · 2.4.3: 390 (0x186)
- Layout (after the opcode): `{ guid u32 flush }`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)
- HermesProxy: `QuestGiverQueryQuest` — matches

### CMSG_QUEST_GIVER_ACCEPT_QUEST (0x3498)

- Modern: 13464 (0x3498) · 2.4.3: 393 (0x189)
- Layout (after the opcode): `guid u32 flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)
- HermesProxy: `QuestGiverAcceptQuest` — matches

### CMSG_QUEST_GIVER_COMPLETE_QUEST (0x3499)

- Modern: 13465 (0x3499) · 2.4.3: 394 (0x18a)
- Layout (after the opcode): `guid u32 flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)
- HermesProxy: `QuestGiverCompleteQuest` — matches

### CMSG_QUEST_GIVER_CHOOSE_REWARD (0x349a)

- Modern: 13466 (0x349a) · 2.4.3: 398 (0x18e)
- Layout (after the opcode): `guid u32 { bits(2) flush { u32*3 opt[ alt[ bits(7) alt[ bits(6) alt[ bits(5) alt[ bits(4) alt[ bits(3) alt[ bits(2) ] alt[ u8 ] ] ] ] ] ] ] { bits(6) opt[ alt[ bits(7) alt[ bits(6) alt[ bits(5) alt[ bits(4) alt[ bits(3) alt[ bits(2) ] alt[ u8 ] ] ] ] ] ] ] loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 6: 2 · (6 unused); byte 19: 1 · (7 unused) · 6 · (2 unused)
- HermesProxy: `QuestGiverChooseReward` — matches

### CMSG_QUEST_GIVER_REQUEST_REWARD (0x349b)

- Modern: 13467 (0x349b) · 2.4.3: 396 (0x18c)
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `QuestGiverRequestReward` — matches

### CMSG_QUEST_GIVER_STATUS_QUERY (0x349c)

- Modern: 13468 (0x349c) · 2.4.3: 386 (0x182)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `QuestGiverStatusQuery` — matches

### CMSG_QUEST_GIVER_STATUS_MULTIPLE_QUERY (0x349d)

- Modern: 13469 (0x349d) · 2.4.3: 1046 (0x416)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_QUEST_CONFIRM_ACCEPT (0x349e)

- Modern: 13470 (0x349e) · 2.4.3: 411 (0x19b)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `QuestConfirmAcceptResponse` — matches

### CMSG_PUSH_QUEST_TO_PARTY (0x349f)

- Modern: 13471 (0x349f) · 2.4.3: 413 (0x19d)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `PushQuestToParty` — matches

### CMSG_QUEST_PUSH_RESULT (0x34a0)

- Modern: 13472 (0x34a0) · 2.4.3: —
- Layout (after the opcode): `guid u32 u8`
- Size: 5 bytes (packed GUIDs not counted)
- HermesProxy: `QuestPushResultResponse` — matches

### CMSG_LIST_INVENTORY (0x34a1)

- Modern: 13473 (0x34a1) · 2.4.3: 414 (0x19e)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_SELL_ITEM (0x34a2)

- Modern: 13474 (0x34a2) · 2.4.3: 416 (0x1a0)
- Layout (after the opcode): `guid guid u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `SellItem` — matches

### CMSG_BUY_ITEM (0x34a3)

- Modern: 13475 (0x34a3) · 2.4.3: 418 (0x1a2)
- Layout (after the opcode): `guid guid u32*3 { u32*3 opt[ alt[ bits(7) alt[ bits(6) alt[ bits(5) alt[ bits(4) alt[ bits(3) alt[ bits(2) ] alt[ u8 ] ] ] ] ] ] ] { bits(6) opt[ alt[ bits(7) alt[ bits(6) alt[ bits(5) alt[ bits(4) alt[ bits(3) alt[ bits(2) ] alt[ u8 ] ] ] ] ] ] ] loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } bits(3) flush`
- Bit fields (message of zeros, widths in order written): byte 28: 1 · (7 unused) · 6 · (2 unused) · 3 · (5 unused)
- HermesProxy: `BuyItem` — matches

### CMSG_BUY_BACK_ITEM (0x34a4)

- Modern: 13476 (0x34a4) · 2.4.3: 656 (0x290)
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `BuyBackItem` — matches

### — (0x34a5)

- Modern: 13477 (0x34a5) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x34a6)

- Modern: 13478 (0x34a6) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x34a7)

- Modern: 13479 (0x34a7) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_TAXI_NODE_STATUS_QUERY (0x34a8)

- Modern: 13480 (0x34a8) · 2.4.3: 426 (0x1aa)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_ENABLE_TAXI_NODE (0x34a9)

- Modern: 13481 (0x34a9) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_TAXI_QUERY_AVAILABLE_NODES (0x34aa)

- Modern: 13482 (0x34aa) · 2.4.3: 428 (0x1ac)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_ACTIVATE_TAXI (0x34ab)

- Modern: 13483 (0x34ab) · 2.4.3: 429 (0x1ad)
- Layout (after the opcode): `guid u32*3`
- Size: 12 bytes (packed GUIDs not counted)
- HermesProxy: `ActivateTaxi` — matches

### CMSG_TAXI_REQUEST_EARLY_LANDING (0x34ac)

- Modern: 13484 (0x34ac) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_TRAINER_LIST (0x34ad)

- Modern: 13485 (0x34ad) · 2.4.3: 432 (0x1b0)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_TRAINER_BUY_SPELL (0x34ae)

- Modern: 13486 (0x34ae) · 2.4.3: 434 (0x1b2)
- Layout (after the opcode): `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `TrainerBuySpell` — matches

### CMSG_SPIRIT_HEALER_ACTIVATE (0x34af)

- Modern: 13487 (0x34af) · 2.4.3: 540 (0x21c)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_AREA_SPIRIT_HEALER_QUERY (0x34b0)

- Modern: 13488 (0x34b0) · 2.4.3: 738 (0x2e2)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_AREA_SPIRIT_HEALER_QUEUE (0x34b1)

- Modern: 13489 (0x34b1) · 2.4.3: 739 (0x2e3)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_BINDER_ACTIVATE (0x34b2)

- Modern: 13490 (0x34b2) · 2.4.3: 437 (0x1b5)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_BANKER_ACTIVATE (0x34b3)

- Modern: 13491 (0x34b3) · 2.4.3: 439 (0x1b7)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_BUY_BANK_SLOT (0x34b4)

- Modern: 13492 (0x34b4) · 2.4.3: 441 (0x1b9)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `BuyBankSlot` — matches

### CMSG_GUILD_BANK_ACTIVATE (0x34b5)

- Modern: 13493 (0x34b5) · 2.4.3: 997 (0x3e5)
- Layout (after the opcode): `guid flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)
- HermesProxy: `GuildBankAtivate` — matches

### CMSG_AUTO_GUILD_BANK_ITEM (0x34b6)

- Modern: 13494 (0x34b6) · 2.4.3: —
- Layout (after the opcode): `guid u8*3 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 8 · 1 · (7 unused)
- HermesProxy: `AutoGuildBankItem` — matches

### CMSG_STORE_GUILD_BANK_ITEM (0x34b7)

- Modern: 13495 (0x34b7) · 2.4.3: —
- Layout (after the opcode): `guid u8*3 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 8 · 1 · (7 unused)
- HermesProxy: `AutoGuildBankItem` — matches

### CMSG_SWAP_ITEM_WITH_GUILD_BANK_ITEM (0x34b8)

- Modern: 13496 (0x34b8) · 2.4.3: —
- Layout (after the opcode): `guid u8*3 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 8 · 1 · (7 unused)
- HermesProxy: `AutoGuildBankItem` — matches

### CMSG_SWAP_GUILD_BANK_ITEM_WITH_GUILD_BANK_ITEM (0x34b9)

- Modern: 13497 (0x34b9) · 2.4.3: —
- Layout (after the opcode): `guid loop[ u8*2 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 16

### CMSG_MOVE_GUILD_BANK_ITEM (0x34ba)

- Modern: 13498 (0x34ba) · 2.4.3: —
- Layout (after the opcode): `guid u8*4`
- Size: 4 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 8 · 8
- HermesProxy: `MoveGuildBankItem` — matches

### CMSG_MERGE_ITEM_WITH_GUILD_BANK_ITEM (0x34bb)

- Modern: 13499 (0x34bb) · 2.4.3: —
- Layout (after the opcode): `guid u8*3 u32 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 8; byte 9: 1 · (7 unused)
- HermesProxy: `SplitItemToGuildBank` — matches

### CMSG_SPLIT_ITEM_TO_GUILD_BANK (0x34bc)

- Modern: 13500 (0x34bc) · 2.4.3: —
- Layout (after the opcode): `guid u8*3 u32 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 8; byte 9: 1 · (7 unused)
- HermesProxy: `SplitItemToGuildBank` — matches

### CMSG_MERGE_GUILD_BANK_ITEM_WITH_ITEM (0x34bd)

- Modern: 13501 (0x34bd) · 2.4.3: —
- Layout (after the opcode): `guid u8*3 u32 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 8; byte 9: 1 · (7 unused)
- HermesProxy: `SplitItemToGuildBank` — matches

### CMSG_SPLIT_GUILD_BANK_ITEM_TO_INVENTORY (0x34be)

- Modern: 13502 (0x34be) · 2.4.3: —
- Layout (after the opcode): `guid u8*3 u32 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 8; byte 9: 1 · (7 unused)
- HermesProxy: `SplitItemToGuildBank` — matches

### CMSG_AUTO_STORE_GUILD_BANK_ITEM (0x34bf)

- Modern: 13503 (0x34bf) · 2.4.3: —
- Layout (after the opcode): `guid u8*2`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8
- HermesProxy: `AutoStoreGuildBankItem` — matches

### CMSG_MERGE_GUILD_BANK_ITEM_WITH_GUILD_BANK_ITEM (0x34c0)

- Modern: 13504 (0x34c0) · 2.4.3: —
- Layout (after the opcode): `guid u8*4 u32`
- Size: 8 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 8 · 8
- HermesProxy: `SplitGuildBankItem` — matches

### CMSG_SPLIT_GUILD_BANK_ITEM (0x34c1)

- Modern: 13505 (0x34c1) · 2.4.3: —
- Layout (after the opcode): `guid u8*4 u32`
- Size: 8 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 8 · 8
- HermesProxy: `SplitGuildBankItem` — matches

### CMSG_GUILD_BANK_QUERY_TAB (0x34c2)

- Modern: 13506 (0x34c2) · 2.4.3: 998 (0x3e6)
- Layout (after the opcode): `guid u8 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 1 · (7 unused)
- HermesProxy: `GuildBankQueryTab` — matches

### CMSG_GUILD_BANK_BUY_TAB (0x34c3)

- Modern: 13507 (0x34c3) · 2.4.3: 1001 (0x3e9)
- Layout (after the opcode): `guid u8`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `GuildBankBuyTab` — matches

### CMSG_GUILD_BANK_UPDATE_TAB (0x34c4)

- Modern: 13508 (0x34c4) · 2.4.3: 1002 (0x3ea)
- Layout (after the opcode): `guid u8 bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`
- HermesProxy: `GuildBankUpdateTab` — matches

### CMSG_GUILD_BANK_DEPOSIT_MONEY (0x34c5)

- Modern: 13509 (0x34c5) · 2.4.3: 1003 (0x3eb)
- Layout (after the opcode): `guid u64`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `GuildBankDepositMoney` — matches

### CMSG_GUILD_BANK_WITHDRAW_MONEY (0x34c6)

- Modern: 13510 (0x34c6) · 2.4.3: 1004 (0x3ec)
- Layout (after the opcode): `guid u64`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `GuildBankWithdrawMoney` — matches

### CMSG_PETITION_SHOW_LIST (0x34c7)

- Modern: 13511 (0x34c7) · 2.4.3: 443 (0x1bb)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_PETITION_BUY (0x34c8)

- Modern: 13512 (0x34c8) · 2.4.3: 445 (0x1bd)
- Layout (after the opcode): `bits(7) flush guid u32 bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)
- HermesProxy: `PetitionBuy` — matches

### CMSG_PETITION_SHOW_SIGNATURES (0x34c9)

- Modern: 13513 (0x34c9) · 2.4.3: 446 (0x1be)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `PetitionShowSignatures` — matches

### CMSG_AUCTION_HELLO_REQUEST (0x34ca)

- Modern: 13514 (0x34ca) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_AUCTION_SELL_ITEM (0x34cb)

- Modern: 13515 (0x34cb) · 2.4.3: 598 (0x256)
- Layout (after the opcode): `guid u64*2 u32 bits(5) flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush bytes bytes } ] loop[ guid u32 ]`
- Bit fields (message of zeros, widths in order written): byte 22: 1 · 5 · (2 unused)
- HermesProxy: `AuctionSellItem` — matches

### CMSG_AUCTION_REMOVE_ITEM (0x34cc)

- Modern: 13516 (0x34cc) · 2.4.3: 599 (0x257)
- Layout (after the opcode): `guid u32 flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush bytes bytes } ]`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)
- HermesProxy: `AuctionRemoveItem` — matches

### CMSG_AUCTION_LIST_ITEMS (0x34cd)

- Modern: 13517 (0x34cd) · 2.4.3: 600 (0x258)
- Layout (after the opcode): `{ u32 guid u8*2 u32 u8 u32 u8 loop[ u8 ] u8 flush bytes bits(3) opt[ u8 ] opt[ u8 ] flush loop[ { u32 bits(5) flush loop[ u32*2 ] } ] u32 bytes }`
- Bit fields (message of zeros, widths in order written): byte 6: 8 · 8; byte 17: 8 · (8 unused) · 3 · 1 · 1 · (3 unused)
- HermesProxy: `AuctionListItems` — not checked

### CMSG_AUCTION_REPLICATE_ITEMS (0x34ce)

- Modern: 13518 (0x34ce) · 2.4.3: —
- Layout (after the opcode): `guid u32*4 flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush bytes bytes } ]`
- Bit fields (message of zeros, widths in order written): byte 18: 1 · (7 unused)

### CMSG_AUCTION_LIST_OWNED_ITEMS (0x34cf)

- Modern: 13519 (0x34cf) · 2.4.3: 601 (0x259)
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `AuctionListOwnerItems` — matches

### CMSG_AUCTION_LIST_BIDDED_ITEMS (0x34d0)

- Modern: 13520 (0x34d0) · 2.4.3: 612 (0x264)
- Layout (after the opcode): `guid u32 bits(7) flush loop[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 6: 7 · (1 unused)
- HermesProxy: `AuctionListBidderItems` — matches

### CMSG_AUCTION_PLACE_BID (0x34d1)

- Modern: 13521 (0x34d1) · 2.4.3: 602 (0x25a)
- Layout (after the opcode): `guid u32 u64 flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush bytes bytes } ]`
- Bit fields (message of zeros, widths in order written): byte 14: 1 · (7 unused)
- HermesProxy: `AuctionPlaceBid` — matches

### CMSG_AUCTION_LIST_PENDING_SALES (0x34d2)

- Modern: 13522 (0x34d2) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x34d3)

- Modern: 13523 (0x34d3) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x34d4)

- Modern: 13524 (0x34d4) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_QUERY_TIME (0x34d5)

- Modern: 13525 (0x34d5) · 2.4.3: 462 (0x1ce)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_LOGOUT_REQUEST (0x34d6)

- Modern: 13526 (0x34d6) · 2.4.3: 75 (0x4b)
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `LogoutRequest` — matches

### — (0x34d7)

- Modern: 13527 (0x34d7) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_LOGOUT_CANCEL (0x34d8)

- Modern: 13528 (0x34d8) · 2.4.3: 78 (0x4e)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_LOGOUT_INSTANT (0x34d9)

- Modern: 13529 (0x34d9) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x34da)

- Modern: 13530 (0x34da) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_RECLAIM_CORPSE (0x34db)

- Modern: 13531 (0x34db) · 2.4.3: 466 (0x1d2)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ReclaimCorpse` — matches

### — (0x34dc)

- Modern: 13532 (0x34dc) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_COMPLETE_MOVIE (0x34dd)

- Modern: 13533 (0x34dd) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_SET_FACTION_AT_WAR (0x34de)

- Modern: 13534 (0x34de) · 2.4.3: 293 (0x125)
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `SetFactionAtWar` — matches

### CMSG_SET_FACTION_NOT_AT_WAR (0x34df)

- Modern: 13535 (0x34df) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `SetFactionNotAtWar` — matches

### CMSG_SET_FACTION_INACTIVE (0x34e0)

- Modern: 13536 (0x34e0) · 2.4.3: 791 (0x317)
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)
- HermesProxy: `SetFactionInactive` — matches

### CMSG_SET_WATCHED_FACTION (0x34e1)

- Modern: 13537 (0x34e1) · 2.4.3: 792 (0x318)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `SetWatchedFaction` — matches

### CMSG_DUEL_RESPONSE (0x34e2)

- Modern: 13538 (0x34e2) · 2.4.3: —
- Layout (after the opcode): `guid opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · (6 unused)
- HermesProxy: `DuelResponse` — matches

### — (0x34e3)

- Modern: 13539 (0x34e3) · 2.4.3: —
- Layout (after the opcode): `u32 u8 flush bytes`

### — (0x34e4)

- Modern: 13540 (0x34e4) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_UNLEARN_SKILL (0x34e5)

- Modern: 13541 (0x34e5) · 2.4.3: 514 (0x202)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `UnlearnSkill` — matches

### — (0x34e6)

- Modern: 13542 (0x34e6) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### CMSG_CANCEL_AUTO_REPEAT_SPELL (0x34e7)

- Modern: 13543 (0x34e7) · 2.4.3: 621 (0x26d)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_FAR_SIGHT (0x34e8)

- Modern: 13544 (0x34e8) · 2.4.3: 634 (0x27a)
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `FarSight` — matches

### — (0x34e9)

- Modern: 13545 (0x34e9) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x34ea)

- Modern: 13546 (0x34ea) · 2.4.3: —
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SOCKET_GEMS (0x34eb)

- Modern: 13547 (0x34eb) · 2.4.3: 839 (0x347)
- Layout (after the opcode): `guid loop[ guid ]`
- HermesProxy: `SocketGems` — matches

### CMSG_REPAIR_ITEM (0x34ec)

- Modern: 13548 (0x34ec) · 2.4.3: 680 (0x2a8)
- Layout (after the opcode): `guid guid flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)
- HermesProxy: `RepairItem` — matches

### — (0x34ed)

- Modern: 13549 (0x34ed) · 2.4.3: —
- Layout (after the opcode): `f32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GAME_OBJ_USE (0x34ee)

- Modern: 13550 (0x34ee) · 2.4.3: 177 (0xb1)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `GameObjUse` — matches

### CMSG_GAME_OBJ_REPORT_USE (0x34ef)

- Modern: 13551 (0x34ef) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `GameObjReportUse` — matches

### — (0x34f0)

- Modern: 13552 (0x34f0) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x34f1)

- Modern: 13553 (0x34f1) · 2.4.3: —
- Layout (after the opcode): `opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 1 · (6 unused)

### CMSG_CANCEL_TEMP_ENCHANTMENT (0x34f2)

- Modern: 13554 (0x34f2) · 2.4.3: 889 (0x379)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `CancelTempEnchantment` — matches

### CMSG_SET_TAXI_BENCHMARK_MODE (0x34f3)

- Modern: 13555 (0x34f3) · 2.4.3: 905 (0x389)
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### CMSG_REPORT_PVP_PLAYER_AFK (0x34f4)

- Modern: 13556 (0x34f4) · 2.4.3: 995 (0x3e3)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_ALTER_APPEARANCE (0x34f5)

- Modern: 13557 (0x34f5) · 2.4.3: —
- Layout (after the opcode): `u32 u8 loop[ { u32*2 } ]`
- HermesProxy: `AlterAppearance` — matches

### CMSG_OPT_OUT_OF_LOOT (0x34f6)

- Modern: 13558 (0x34f6) · 2.4.3: 1032 (0x408)
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `OptOutOfLoot` — matches

### — (0x34f7)

- Modern: 13559 (0x34f7) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_TOTEM_DESTROYED (0x34f8)

- Modern: 13560 (0x34f8) · 2.4.3: 1043 (0x413)
- Layout (after the opcode): `u8 guid`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `TotemDestroyed` — matches

### CMSG_DISMISS_CRITTER (0x34f9)

- Modern: 13561 (0x34f9) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `DismissCritter` — matches

### — (0x34fa)

- Modern: 13562 (0x34fa) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x34fb)

- Modern: 13563 (0x34fb) · 2.4.3: —
- Layout (after the opcode): `flush guid`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x34fc)

- Modern: 13564 (0x34fc) · 2.4.3: —
- Layout (after the opcode): `flush u8 u32`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused) · 8

### — (0x34fd)

- Modern: 13565 (0x34fd) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x34fe)

- Modern: 13566 (0x34fe) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x34ff)

- Modern: 13567 (0x34ff) · 2.4.3: —
- Layout (after the opcode): `u32 u16`
- Size: 6 bytes (packed GUIDs not counted)

### CMSG_QUERY_INSPECT_ACHIEVEMENTS (0x3500)

- Modern: 13568 (0x3500) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3501)

- Modern: 13569 (0x3501) · 2.4.3: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)

### — (0x3502)

- Modern: 13570 (0x3502) · 2.4.3: —
- Layout (after the opcode): `u64*2 u32`
- Size: 20 bytes (packed GUIDs not counted)

### — (0x3503)

- Modern: 13571 (0x3503) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3504)

- Modern: 13572 (0x3504) · 2.4.3: —
- Layout (after the opcode): `opt[ u8 ] opt[ u8 ] opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 1 · 1 · 1 · (4 unused)

### — (0x3505)

- Modern: 13573 (0x3505) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_HEARTH_AND_RESURRECT (0x3506)

- Modern: 13574 (0x3506) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3507)

- Modern: 13575 (0x3507) · 2.4.3: —
- Layout (after the opcode): `u8 flush bytes`

### — (0x3508)

- Modern: 13576 (0x3508) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### CMSG_SAVE_EQUIPMENT_SET (0x3509)

- Modern: 13577 (0x3509) · 2.4.3: —
- Layout (after the opcode): `{ u32 u64 u32*2 loop[ guid u32 ] loop[ u32 ] u32*4 u8*2 opt[ u8 ] flush opt[ u32 ] bytes bytes }`
- Bit fields (message of zeros, widths in order written): byte 158: 1 · 8 · 9 · (6 unused)
- HermesProxy: `SaveEquipmentSet` — matches

### CMSG_DELETE_EQUIPMENT_SET (0x350a)

- Modern: 13578 (0x350a) · 2.4.3: —
- Layout (after the opcode): `u64`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `DeleteEquipmentSet` — matches

### CMSG_INSTANCE_LOCK_RESPONSE (0x350b)

- Modern: 13579 (0x350b) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `InstanceLockResponse` — matches

### — (0x350c)

- Modern: 13580 (0x350c) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x350d)

- Modern: 13581 (0x350d) · 2.4.3: —
- Layout (after the opcode): `opt[ u8 ] flush u32*3`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 1 · (6 unused)

### — (0x350e)

- Modern: 13582 (0x350e) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x350f)

- Modern: 13583 (0x350f) · 2.4.3: —
- Layout (after the opcode): `u32 loop[ u16 ] flush opt[ guid ]`

### — (0x3510)

- Modern: 13584 (0x3510) · 2.4.3: —
- Layout (after the opcode): `u32 loop[ u16 ] flush opt[ guid ]`

### — (0x3511)

- Modern: 13585 (0x3511) · 2.4.3: —
- Layout (after the opcode): `u32 loop[ u16 ] flush opt[ guid ]`

### CMSG_LOW_LEVEL_RAID2 (0x3512)

- Modern: 13586 (0x3512) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x3513)

- Modern: 13587 (0x3513) · 2.4.3: —
- Layout (after the opcode): `u8 bits(3) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: (10 unused) · 1 · (5 unused)

### — (0x3514)

- Modern: 13588 (0x3514) · 2.4.3: —
- Layout (after the opcode): `u32 loop[ { u32*2 } ]`

### — (0x3515)

- Modern: 13589 (0x3515) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3516)

- Modern: 13590 (0x3516) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3517)

- Modern: 13591 (0x3517) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3518)

- Modern: 13592 (0x3518) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3519)

- Modern: 13593 (0x3519) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x351a)

- Modern: 13594 (0x351a) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x351b)

- Modern: 13595 (0x351b) · 2.4.3: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)

### — (0x351c)

- Modern: 13596 (0x351c) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_DECLINE_GUILD_INVITES (0x351d)

- Modern: 13597 (0x351d) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `SetAutoDeclineGuildInvites` — differs

### — (0x351e)

- Modern: 13598 (0x351e) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_BATTLEMASTER_JOIN (0x351f)

- Modern: 13599 (0x351f) · 2.4.3: 750 (0x2ee)
- Layout (after the opcode): `u64 u8 loop[ u32 ] guid u32*2 flush`
- Bit fields (message of zeros, widths in order written): byte 27: 1 · (7 unused)
- HermesProxy: `BattlemasterJoin` — matches

### CMSG_BATTLEMASTER_JOIN_ARENA (0x3520)

- Modern: 13600 (0x3520) · 2.4.3: 856 (0x358)
- Layout (after the opcode): `guid u8*2`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8
- HermesProxy: `BattlemasterJoinArena` — matches

### CMSG_BATTLEMASTER_JOIN_SKIRMISH (0x3521)

- Modern: 13601 (0x3521) · 2.4.3: —
- Layout (after the opcode): `guid u8*2 opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 1 · 1 · (6 unused)
- HermesProxy: `BattlemasterJoinSkirmish` — matches

### CMSG_BATTLEMASTER_JOIN_BRAWL (0x3522)

- Modern: 13602 (0x3522) · 2.4.3: —
- Layout (after the opcode): `u8 flush`
- Bit fields (message of zeros, widths in order written): byte 0: 8 · 1 · (7 unused)

### — (0x3523)

- Modern: 13603 (0x3523) · 2.4.3: —
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)

### CMSG_BATTLEFIELD_PORT (0x3524)

- Modern: 13604 (0x3524) · 2.4.3: 725 (0x2d5)
- Layout (after the opcode): `{ guid u32*2 u64 } flush`
- Bit fields (message of zeros, widths in order written): byte 18: 1 · (7 unused)
- HermesProxy: `BattlefieldPort` — matches

### CMSG_REPOP_REQUEST (0x3525)

- Modern: 13605 (0x3525) · 2.4.3: 346 (0x15a)
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `RepopRequest` — matches

### CMSG_CLIENT_PORT_GRAVEYARD (0x3526)

- Modern: 13606 (0x3526) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_SET_SELECTION (0x3527)

- Modern: 13607 (0x3527) · 2.4.3: 317 (0x13d)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `SetSelection` — matches

### CMSG_INSPECT (0x3528)

- Modern: 13608 (0x3528) · 2.4.3: 276 (0x114)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `Inspect` — matches

### CMSG_REQUEST_CROWD_CONTROL_SPELL (0x3529)

- Modern: 13609 (0x3529) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_BLACK_MARKET_OPEN (0x352a)

- Modern: 13610 (0x352a) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_BLACK_MARKET_REQUEST_ITEMS (0x352b)

- Modern: 13611 (0x352b) · 2.4.3: —
- Layout (after the opcode): `guid u64`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_BLACK_MARKET_BID_ON_ITEM (0x352c)

- Modern: 13612 (0x352c) · 2.4.3: —
- Layout (after the opcode): `guid u32 u64 { u32*3 opt[ alt[ bits(7) alt[ bits(6) alt[ bits(5) alt[ bits(4) alt[ bits(3) alt[ bits(2) ] alt[ u8 ] ] ] ] ] ] ] { bits(6) opt[ alt[ bits(7) alt[ bits(6) alt[ bits(5) alt[ bits(4) alt[ bits(3) alt[ bits(2) ] alt[ u8 ] ] ] ] ] ] ] loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 26: 1 · (7 unused) · 6 · (2 unused)

### CMSG_QUEST_LOG_REMOVE_QUEST (0x352d)

- Modern: 13613 (0x352d) · 2.4.3: 404 (0x194)
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `QuestLogRemoveQuest` — matches

### CMSG_GET_ITEM_PURCHASE_DATA (0x352e)

- Modern: 13614 (0x352e) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_ITEM_PURCHASE_REFUND (0x352f)

- Modern: 13615 (0x352f) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_SELF_RES (0x3530)

- Modern: 13616 (0x3530) · 2.4.3: 691 (0x2b3)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `SelfRes` — matches

### CMSG_SET_ACTION_BAR_TOGGLES (0x3531)

- Modern: 13617 (0x3531) · 2.4.3: 703 (0x2bf)
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `SetActionBarToggles` — matches

### CMSG_SIGN_PETITION (0x3532)

- Modern: 13618 (0x3532) · 2.4.3: 448 (0x1c0)
- Layout (after the opcode): `guid u8`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `SignPetition` — matches

### CMSG_DECLINE_PETITION (0x3533)

- Modern: 13619 (0x3533) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `DeclinePetition` — matches

### CMSG_TURN_IN_PETITION (0x3534)

- Modern: 13620 (0x3534) · 2.4.3: 452 (0x1c4)
- Layout (after the opcode): `guid u32*5`
- Size: 20 bytes (packed GUIDs not counted)
- HermesProxy: `TurnInPetition` — reads the first part

### CMSG_MAIL_GET_LIST (0x3535)

- Modern: 13621 (0x3535) · 2.4.3: 570 (0x23a)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `MailGetList` — matches

### CMSG_MAIL_TAKE_MONEY (0x3536)

- Modern: 13622 (0x3536) · 2.4.3: 581 (0x245)
- Layout (after the opcode): `guid u32 u64`
- Size: 12 bytes (packed GUIDs not counted)
- HermesProxy: `MailTakeMoney` — matches

### CMSG_MAIL_TAKE_ITEM (0x3537)

- Modern: 13623 (0x3537) · 2.4.3: 582 (0x246)
- Layout (after the opcode): `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `MailTakeItem` — matches

### CMSG_QUERY_NEXT_MAIL_TIME (0x3538)

- Modern: 13624 (0x3538) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_MAIL_MARK_AS_READ (0x3539)

- Modern: 13625 (0x3539) · 2.4.3: 583 (0x247)
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `MailMarkAsRead` — matches

### CMSG_MAIL_CREATE_TEXT_ITEM (0x353a)

- Modern: 13626 (0x353a) · 2.4.3: 586 (0x24a)
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `MailCreateTextItem` — matches

### — (0x353b)

- Modern: 13627 (0x353b) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x353c)

- Modern: 13628 (0x353c) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x353d)

- Modern: 13629 (0x353d) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SET_LOOT_SPECIALIZATION (0x353e)

- Modern: 13630 (0x353e) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x353f)

- Modern: 13631 (0x353f) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### CMSG_EMOTE (0x3540)

- Modern: 13632 (0x3540) · 2.4.3: 258 (0x102)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3541)

- Modern: 13633 (0x3541) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_OPENING_CINEMATIC (0x3542)

- Modern: 13634 (0x3542) · 2.4.3: 249 (0xf9)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ClientCinematicPkt` — matches

### CMSG_NEXT_CINEMATIC_CAMERA (0x3543)

- Modern: 13635 (0x3543) · 2.4.3: 251 (0xfb)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ClientCinematicPkt` — matches

### CMSG_COMPLETE_CINEMATIC (0x3544)

- Modern: 13636 (0x3544) · 2.4.3: 252 (0xfc)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ClientCinematicPkt` — matches

### CMSG_CONVERSATION_LINE_STARTED (0x3545)

- Modern: 13637 (0x3545) · 2.4.3: —
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CONVERSATION_CINEMATIC_READY (0x3546)

- Modern: 13638 (0x3546) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3547)

- Modern: 13639 (0x3547) · 2.4.3: —
- Layout (after the opcode): `u32 f32`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_QUEST_GIVER_CLOSE_QUEST (0x3548)

- Modern: 13640 (0x3548) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `QuestGiverCloseQuest` — matches

### CMSG_START_CHALLENGE_MODE (0x3549)

- Modern: 13641 (0x3549) · 2.4.3: —
- Layout (after the opcode): `u8 u32 guid`
- Size: 5 bytes (packed GUIDs not counted)

### — (0x354a)

- Modern: 13642 (0x354a) · 2.4.3: —
- Layout (after the opcode): `{ guid opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · 1 · 1 · (4 unused)

### — (0x354b)

- Modern: 13643 (0x354b) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x354c)

- Modern: 13644 (0x354c) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x354d)

- Modern: 13645 (0x354d) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x354e)

- Modern: 13646 (0x354e) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x354f)

- Modern: 13647 (0x354f) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3550)

- Modern: 13648 (0x3550) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_LEARN_TALENT (0x3551)

- Modern: 13649 (0x3551) · 2.4.3: 593 (0x251)
- Layout (after the opcode): `u32 u16`
- Size: 6 bytes (packed GUIDs not counted)
- HermesProxy: `LearnTalent` — matches

### — (0x3552)

- Modern: 13650 (0x3552) · 2.4.3: —
- Layout (after the opcode): `bits(6) flush loop[ u16 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 6 · (2 unused)

### — (0x3553)

- Modern: 13651 (0x3553) · 2.4.3: —
- Layout (after the opcode): `u32 loop[ { u16 u8 } ]`

### CMSG_LEARN_PVP_TALENTS (0x3554)

- Modern: 13652 (0x3554) · 2.4.3: —
- Layout (after the opcode): `u32 loop[ { u16 u8 } ]`

### CMSG_CONTRIBUTION_CONTRIBUTE (0x3555)

- Modern: 13653 (0x3555) · 2.4.3: —
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CONTRIBUTION_LAST_UPDATE_REQUEST (0x3556)

- Modern: 13654 (0x3556) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x3557)

- Modern: 13655 (0x3557) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### CMSG_SET_ACTION_BUTTON (0x3558)

- Modern: 13656 (0x3558) · 2.4.3: 296 (0x128)
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)
- HermesProxy: `SetActionButton` — differs

### CMSG_SET_AMMO (0x3559)

- Modern: 13657 (0x3559) · 2.4.3: 616 (0x268)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `SetAmmo` — matches

### — (0x355a)

- Modern: 13658 (0x355a) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x355b)

- Modern: 13659 (0x355b) · 2.4.3: —
- Layout (after the opcode): `{ guid opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ u32 ] opt[ u64 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (5 unused)

### — (0x355c)

- Modern: 13660 (0x355c) · 2.4.3: —
- Layout (after the opcode): `{ guid opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ u8 ] opt[ u32 ] }`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · 1 · (5 unused)

### — (0x355d)

- Modern: 13661 (0x355d) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x355e)

- Modern: 13662 (0x355e) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x355f)

- Modern: 13663 (0x355f) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3560)

- Modern: 13664 (0x3560) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3561)

- Modern: 13665 (0x3561) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3562)

- Modern: 13666 (0x3562) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_PLAYER_SHOWING_HELM (0x3563)

- Modern: 13667 (0x3563) · 2.4.3: 697 (0x2b9)
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `PlayerShowingHelmOrCloak` — matches

### CMSG_PLAYER_SHOWING_CLOAK (0x3564)

- Modern: 13668 (0x3564) · 2.4.3: 698 (0x2ba)
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `PlayerShowingHelmOrCloak` — matches

### CMSG_CONNECT_TO_FAILED (0x35d4)

- Modern: 13780 (0x35d4) · 2.4.3: —
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)

### — (0x35d5)

- Modern: 13781 (0x35d5) · 2.4.3: —
- Layout (after the opcode): `u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 10 · 10 · (4 unused)

### — (0x35d6)

- Modern: 13782 (0x35d6) · 2.4.3: —
- Layout (after the opcode): `bits(7) flush u8 bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused) · 8

### — (0x35d7)

- Modern: 13783 (0x35d7) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_ADDON_LIST (0x35d8)

- Modern: 13784 (0x35d8) · 2.4.3: —
- Layout (after the opcode): `guid u32 u16 u8 u32 loop[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush bytes bytes } ]`

### CMSG_SET_ROLE (0x35d9)

- Modern: 13785 (0x35d9) · 2.4.3: —
- Layout (after the opcode): `u8 guid u32`
- Size: 5 bytes (packed GUIDs not counted)
- HermesProxy: `SetRole` — matches

### CMSG_INITIATE_ROLE_POLL (0x35da)

- Modern: 13786 (0x35da) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x35db)

- Modern: 13787 (0x35db) · 2.4.3: —
- Layout (after the opcode): `u32*2 f32*3 flush`
- Bit fields (message of zeros, widths in order written): byte 20: 1 · (7 unused)

### — (0x35dc)

- Modern: 13788 (0x35dc) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_REQUEST_BATTLEFIELD_STATUS (0x35dd)

- Modern: 13789 (0x35dd) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `RequestBattlefieldStatus` — matches

### — (0x35de)

- Modern: 13790 (0x35de) · 2.4.3: —
- Layout (after the opcode): `u32 f32*2`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_START_WAR_GAME (0x35df)

- Modern: 13791 (0x35df) · 2.4.3: —
- Layout (after the opcode): `{ guid u32 u16 } u64 flush`
- Bit fields (message of zeros, widths in order written): byte 16: 1 · (7 unused)

### CMSG_START_SPECTATOR_WAR_GAME (0x35e0)

- Modern: 13792 (0x35e0) · 2.4.3: —
- Layout (after the opcode): `{ guid u32 u16 } { guid u32 u16 } u64 flush`
- Bit fields (message of zeros, widths in order written): byte 24: 1 · (7 unused)

### CMSG_ACCEPT_WARGAME_INVITE (0x35e1)

- Modern: 13793 (0x35e1) · 2.4.3: —
- Layout (after the opcode): `guid u64 flush`
- Bit fields (message of zeros, widths in order written): byte 10: 1 · (7 unused)

### — (0x35e2)

- Modern: 13794 (0x35e2) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x35e3)

- Modern: 13795 (0x35e3) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### CMSG_REQUEST_RATED_PVP_INFO (0x35e4)

- Modern: 13796 (0x35e4) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `RequestRatedPvpInfo` — matches

### CMSG_DB_QUERY_BULK (0x35e5)

- Modern: 13797 (0x35e5) · 2.4.3: —
- Layout (after the opcode): `u32 u8 bits(5) flush loop[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 4: 13 · (3 unused)
- HermesProxy: `DBQueryBulk` — matches

### CMSG_HOTFIX_REQUEST (0x35e6)

- Modern: 13798 (0x35e6) · 2.4.3: —
- Layout (after the opcode): `u32*3 loop[ u32 ]`
- HermesProxy: `HotfixRequest` — matches

### — (0x35e7)

- Modern: 13799 (0x35e7) · 2.4.3: —
- Layout (after the opcode): `u32 loop[ { bits(6) flush u32 bytes } ]`

### CMSG_GENERATE_RANDOM_CHARACTER_NAME (0x35e8)

- Modern: 13800 (0x35e8) · 2.4.3: —
- Layout (after the opcode): `u8*2`
- Size: 2 bytes (packed GUIDs not counted)
- HermesProxy: `GenerateRandomCharacterNameRequest` — matches

### CMSG_ENUM_CHARACTERS (0x35e9)

- Modern: 13801 (0x35e9) · 2.4.3: 55 (0x37)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_REORDER_CHARACTERS (0x35ea)

- Modern: 13802 (0x35ea) · 2.4.3: —
- Layout (after the opcode): `u8 flush loop[ guid u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · (7 unused)
- HermesProxy: `ReorderCharacters` — matches

### CMSG_PLAYER_LOGIN (0x35eb)

- Modern: 13803 (0x35eb) · 2.4.3: 61 (0x3d)
- Layout (after the opcode): `guid { f32 } flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)
- HermesProxy: `PlayerLogin` — matches

### — (0x35ec)

- Modern: 13804 (0x35ec) · 2.4.3: —
- Layout (after the opcode): `guid { f32 }`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_WARDEN3_DATA (0x35ed)

- Modern: 13805 (0x35ed) · 2.4.3: —
- Layout (after the opcode): `u32*2 bytes`

### — (0x35ee)

- Modern: 13806 (0x35ee) · 2.4.3: —
- Layout (after the opcode): `bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)

### CMSG_GET_PVP_OPTIONS_ENABLED (0x35ef)

- Modern: 13807 (0x35ef) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_COMMENTATOR_START_WARGAME (0x35f0)

- Modern: 13808 (0x35f0) · 2.4.3: —
- Layout (after the opcode): `loop[ opt[ u8 ] alt[ u8 ] ] opt[ u8 ] flush u64 loop[ bytes ]`

### CMSG_COMMENTATOR_ENABLE (0x35f1)

- Modern: 13809 (0x35f1) · 2.4.3: 948 (0x3b4)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_COMMENTATOR_GET_MAP_INFO (0x35f2)

- Modern: 13810 (0x35f2) · 2.4.3: 950 (0x3b6)
- Layout (after the opcode): `bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 6 · (2 unused)

### CMSG_COMMENTATOR_GET_PLAYER_INFO (0x35f3)

- Modern: 13811 (0x35f3) · 2.4.3: 952 (0x3b8)
- Layout (after the opcode): `u32*2 u16 u8`
- Size: 11 bytes (packed GUIDs not counted)

### CMSG_COMMENTATOR_GET_PLAYER_COOLDOWNS (0x35f4)

- Modern: 13812 (0x35f4) · 2.4.3: —
- Layout (after the opcode): `guid u32 loop[ u32 ]`

### CMSG_COMMENTATOR_ENTER_INSTANCE (0x35f5)

- Modern: 13813 (0x35f5) · 2.4.3: 955 (0x3bb)
- Layout (after the opcode): `u32*2 u16 u8 u64 u32`
- Size: 23 bytes (packed GUIDs not counted)

### CMSG_COMMENTATOR_EXIT_INSTANCE (0x35f6)

- Modern: 13814 (0x35f6) · 2.4.3: 956 (0x3bc)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x35f7)

- Modern: 13815 (0x35f7) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_REQUEST_PARTY_JOIN_UPDATES (0x35f8)

- Modern: 13816 (0x35f8) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### CMSG_LOADING_SCREEN_NOTIFY (0x35f9)

- Modern: 13817 (0x35f9) · 2.4.3: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)
- HermesProxy: `LoadingScreenNotify` — matches

### CMSG_WORLD_PORT_RESPONSE (0x35fa)

- Modern: 13818 (0x35fa) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `WorldPortResponse` — matches

### CMSG_SEND_MAIL (0x35fb)

- Modern: 13819 (0x35fb) · 2.4.3: 568 (0x238)
- Layout (after the opcode): `{ guid u32 u64*2 u8*3 bits(3) bits(5) flush bytes bytes bytes loop[ { u8 guid } ] }`
- Bit fields (message of zeros, widths in order written): byte 22: 9 · 9 · 11 · 5 · (6 unused)
- HermesProxy: `SendMail` — matches

### — (0x35fc)

- Modern: 13820 (0x35fc) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_ACCEPT_GUILD_INVITE (0x35fd)

- Modern: 13821 (0x35fd) · 2.4.3: 132 (0x84)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### — (0x35fe)

- Modern: 13822 (0x35fe) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x35ff)

- Modern: 13823 (0x35ff) · 2.4.3: —
- Layout (after the opcode): `bits(6) flush u64 bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 6 · (2 unused)

### — (0x3600)

- Modern: 13824 (0x3600) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3601)

- Modern: 13825 (0x3601) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3602)

- Modern: 13826 (0x3602) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_PARTY_INVITE (0x3603)

- Modern: 13827 (0x3603) · 2.4.3: 110 (0x6e)
- Layout (after the opcode): `u8 { u8*2 flush u32 guid bytes bytes }`
- Bit fields (message of zeros, widths in order written): byte 0: 8 · 9 · 9 · (6 unused)
- HermesProxy: `PartyInviteClient` — matches

### — (0x3604)

- Modern: 13828 (0x3604) · 2.4.3: —
- Layout (after the opcode): `u8 { u8*2 flush u32 guid bytes bytes }`
- Bit fields (message of zeros, widths in order written): byte 0: 8 · 9 · 9 · (6 unused)

### CMSG_PARTY_INVITE_RESPONSE (0x3605)

- Modern: 13829 (0x3605) · 2.4.3: —
- Layout (after the opcode): `u8 opt[ u8 ] flush opt[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 8 · 1 · 1 · (6 unused)
- HermesProxy: `PartyInviteResponse` — matches

### — (0x3606)

- Modern: 13830 (0x3606) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_GUILD_INVITE_BY_NAME (0x3607)

- Modern: 13831 (0x3607) · 2.4.3: 130 (0x82)
- Layout (after the opcode): `u8 flush bytes opt[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · 1 · (6 unused)
- HermesProxy: `GuildInviteByName` — matches

### CMSG_DF_PROPOSAL_RESPONSE (0x3608)

- Modern: 13832 (0x3608) · 2.4.3: —
- Layout (after the opcode): `{ guid u32*2 u64 } u64 u32 flush`
- Bit fields (message of zeros, widths in order written): byte 30: 1 · (7 unused)
- HermesProxy: `DFProposalResponsePkt` — matches

### CMSG_DF_CONFIRM_EXPAND_SEARCH (0x3609)

- Modern: 13833 (0x3609) · 2.4.3: —
- Layout (after the opcode): `{ guid u32*2 u64 } flush`
- Bit fields (message of zeros, widths in order written): byte 18: 1 · (7 unused)

### CMSG_DF_JOIN (0x360a)

- Modern: 13834 (0x360a) · 2.4.3: —
- Layout (after the opcode): `opt[ u8 ] flush u8 u32*2 loop[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 1 · (6 unused) · 8
- HermesProxy: `DFJoinPkt` — differs

### CMSG_LFG_LIST_LEAVE (0x360b)

- Modern: 13835 (0x360b) · 2.4.3: —
- Layout (after the opcode): `{ guid u32*2 u64 }`
- Size: 16 bytes (packed GUIDs not counted)

### CMSG_LFG_LIST_GET_STATUS (0x360c)

- Modern: 13836 (0x360c) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `LFGListGetStatusPkt` — matches

### CMSG_LFG_LIST_SEARCH (0x360d)

- Modern: 13837 (0x360d) · 2.4.3: —
- Layout (after the opcode): `{ bits(5) flush loop[ loop[ opt[ opt[ u8 ] alt[ u8 ] ] opt[ u8 ] ] flush loop[ bytes ] ] u32*6 loop[ u32 ] loop[ guid ] }`
- Bit fields (message of zeros, widths in order written): byte 0: 5 · (3 unused)

### CMSG_LFG_LIST_APPLY_TO_GROUP (0x360e)

- Modern: 13838 (0x360e) · 2.4.3: —
- Layout (after the opcode): `{ guid u32*2 u64 } u32 u8*2 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 22: 8 · 8

### CMSG_LFG_LIST_CANCEL_APPLICATION (0x360f)

- Modern: 13839 (0x360f) · 2.4.3: —
- Layout (after the opcode): `{ guid u32*2 u64 }`
- Size: 16 bytes (packed GUIDs not counted)

### CMSG_LFG_LIST_DECLINE_APPLICANT (0x3610)

- Modern: 13840 (0x3610) · 2.4.3: —
- Layout (after the opcode): `{ guid u32*2 u64 } { guid u32*2 u64 }`
- Size: 32 bytes (packed GUIDs not counted)

### CMSG_LFG_LIST_INVITE_APPLICANT (0x3611)

- Modern: 13841 (0x3611) · 2.4.3: —
- Layout (after the opcode): `{ guid u32*2 u64 } { guid u32*2 u64 } u32 loop[ { guid u8 } ]`

### CMSG_LFG_LIST_INVITE_RESPONSE (0x3612)

- Modern: 13842 (0x3612) · 2.4.3: —
- Layout (after the opcode): `{ guid u32*2 u64 } flush`
- Bit fields (message of zeros, widths in order written): byte 18: 1 · (7 unused)

### CMSG_DF_LEAVE (0x3613)

- Modern: 13843 (0x3613) · 2.4.3: —
- Layout (after the opcode): `{ guid u32*2 u64 }`
- Size: 16 bytes (packed GUIDs not counted)
- HermesProxy: `DFLeavePkt` — reads the first part

### CMSG_DF_GET_SYSTEM_INFO (0x3614)

- Modern: 13844 (0x3614) · 2.4.3: —
- Layout (after the opcode): `flush u8`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused) · 8
- HermesProxy: `DFGetSystemInfoPkt` — matches

### CMSG_DF_GET_JOIN_STATUS (0x3615)

- Modern: 13845 (0x3615) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `DFGetJoinStatusPkt` — matches

### CMSG_DF_SET_ROLES (0x3616)

- Modern: 13846 (0x3616) · 2.4.3: —
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)
- HermesProxy: `DFSetRolesPkt` — differs

### CMSG_DF_BOOT_PLAYER_VOTE (0x3617)

- Modern: 13847 (0x3617) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### CMSG_DF_TELEPORT (0x3618)

- Modern: 13848 (0x3618) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `DFTeleportPkt` — matches

### CMSG_SET_EVERYONE_IS_ASSISTANT (0x3619)

- Modern: 13849 (0x3619) · 2.4.3: —
- Layout (after the opcode): `u8 flush`
- Bit fields (message of zeros, widths in order written): byte 0: 8 · 1 · (7 unused)
- HermesProxy: `SetEveryoneIsAssistant` — matches

### CMSG_DF_READY_CHECK_RESPONSE (0x361a)

- Modern: 13850 (0x361a) · 2.4.3: —
- Layout (after the opcode): `u8 flush`
- Bit fields (message of zeros, widths in order written): byte 0: 8 · 1 · (7 unused)

### — (0x361b)

- Modern: 13851 (0x361b) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x361c)

- Modern: 13852 (0x361c) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x361d)

- Modern: 13853 (0x361d) · 2.4.3: —
- Layout (after the opcode): `guid u32*3 u8 bits(2) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 14: 10 · (6 unused)

### — (0x361e)

- Modern: 13854 (0x361e) · 2.4.3: —
- Layout (after the opcode): `u32*4 u8 bits(2) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 16: 1 · 10 · (5 unused)

### — (0x361f)

- Modern: 13855 (0x361f) · 2.4.3: —
- Layout (after the opcode): `u32*4`
- Size: 16 bytes (packed GUIDs not counted)

### — (0x3620)

- Modern: 13856 (0x3620) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3621)

- Modern: 13857 (0x3621) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x3622)

- Modern: 13858 (0x3622) · 2.4.3: —
- Layout (after the opcode): `u8 bits(3) opt[ u8 ] alt[ u8 ] bits(2) flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 11 · 10 · (3 unused)

### — (0x3623)

- Modern: 13859 (0x3623) · 2.4.3: —
- Layout (after the opcode): `u32 u8 bits(2) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 10 · (6 unused)

### CMSG_BATTLE_PET_REQUEST_JOURNAL_LOCK (0x3624)

- Modern: 13860 (0x3624) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PET_REQUEST_JOURNAL (0x3625)

- Modern: 13861 (0x3625) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_BATTLE_PET_DELETE_PET (0x3626)

- Modern: 13862 (0x3626) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PET_DELETE_PET_CHEAT (0x3627)

- Modern: 13863 (0x3627) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3628)

- Modern: 13864 (0x3628) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PET_MODIFY_NAME (0x3629)

- Modern: 13865 (0x3629) · 2.4.3: —
- Layout (after the opcode): `guid bits(7) opt[ u8 ] flush opt[ { loop[ opt[ u8 ] alt[ u8 ] ] flush loop[ bytes ] } ] bytes`
- Bit fields (message of zeros, widths in order written): byte 2: 7 · 1

### CMSG_BATTLE_PET_SUMMON (0x362a)

- Modern: 13866 (0x362a) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `BattlePetSummon` — matches

### — (0x362b)

- Modern: 13867 (0x362b) · 2.4.3: —
- Layout (after the opcode): `u32 u16 loop[ guid ]`

### — (0x362c)

- Modern: 13868 (0x362c) · 2.4.3: —
- Layout (after the opcode): `u32*2 u16 loop[ guid ]`

### — (0x362d)

- Modern: 13869 (0x362d) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PET_SET_BATTLE_SLOT (0x362e)

- Modern: 13870 (0x362e) · 2.4.3: —
- Layout (after the opcode): `guid u8`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x362f)

- Modern: 13871 (0x362f) · 2.4.3: —
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)

### — (0x3630)

- Modern: 13872 (0x3630) · 2.4.3: —
- Layout (after the opcode): `opt[ u8 ] flush u32*3`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 1 · (6 unused)

### CMSG_BATTLE_PET_SET_FLAGS (0x3631)

- Modern: 13873 (0x3631) · 2.4.3: —
- Layout (after the opcode): `guid u32 bits(2) flush`
- Bit fields (message of zeros, widths in order written): byte 6: 2 · (6 unused)
- HermesProxy: `BattlePetSetFlags` — differs

### — (0x3632)

- Modern: 13874 (0x3632) · 2.4.3: —
- Layout (after the opcode): `guid u16 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)

### CMSG_MOUNT_SET_FAVORITE (0x3633)

- Modern: 13875 (0x3633) · 2.4.3: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)
- HermesProxy: `MountSetFavorite` — matches

### CMSG_COLLECTION_ITEM_SET_FAVORITE (0x3634)

- Modern: 13876 (0x3634) · 2.4.3: —
- Layout (after the opcode): `u32*2 flush`
- Bit fields (message of zeros, widths in order written): byte 8: 1 · (7 unused)
- HermesProxy: `CollectionItemSetFavorite` — matches

### CMSG_DO_READY_CHECK (0x3635)

- Modern: 13877 (0x3635) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `DoReadyCheck` — matches

### CMSG_READY_CHECK_RESPONSE (0x3636)

- Modern: 13878 (0x3636) · 2.4.3: —
- Layout (after the opcode): `u8 flush`
- Bit fields (message of zeros, widths in order written): byte 0: 8 · 1 · (7 unused)
- HermesProxy: `ReadyCheckResponseClient` — matches

### — (0x3637)

- Modern: 13879 (0x3637) · 2.4.3: —
- Layout (after the opcode): `u64 u8`
- Size: 9 bytes (packed GUIDs not counted)

### — (0x3638)

- Modern: 13880 (0x3638) · 2.4.3: —
- Layout (after the opcode): `u32 u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 1 · (7 unused)

### — (0x3639)

- Modern: 13881 (0x3639) · 2.4.3: —
- Layout (after the opcode): `u32 u8 flush bytes`

### — (0x363a)

- Modern: 13882 (0x363a) · 2.4.3: —
- Layout (after the opcode): `u32 u8 flush bytes`

### — (0x363b)

- Modern: 13883 (0x363b) · 2.4.3: —
- Layout (after the opcode): `u32 u8 flush bytes`

### — (0x363c)

- Modern: 13884 (0x363c) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x363d)

- Modern: 13885 (0x363d) · 2.4.3: —
- Layout (after the opcode): `u32 u8 flush bytes`

### — (0x363e)

- Modern: 13886 (0x363e) · 2.4.3: —
- Layout (after the opcode): `u32 u8 flush bytes`

### — (0x363f)

- Modern: 13887 (0x363f) · 2.4.3: —
- Layout (after the opcode): `u32 u8 flush bytes`

### — (0x3640)

- Modern: 13888 (0x3640) · 2.4.3: —
- Layout (after the opcode): `u32 u8 flush bytes`

### — (0x3641)

- Modern: 13889 (0x3641) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x3642)

- Modern: 13890 (0x3642) · 2.4.3: —
- Layout (after the opcode): `{ u8*4 u32*2 flush }`
- Bit fields (message of zeros, widths in order written): byte 0: 8 · 8 · 8 · 8; byte 12: 1 · (7 unused)

### CMSG_PET_BATTLE_INPUT (0x3643)

- Modern: 13891 (0x3643) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### CMSG_PET_BATTLE_REPLACE_FRONT_PET (0x3644)

- Modern: 13892 (0x3644) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CREATE_CHARACTER (0x3645)

- Modern: 13893 (0x3645) · 2.4.3: 54 (0x36)
- Layout (after the opcode): `{ bits(6) opt[ u8 ] opt[ u8 ] opt[ u8 ] flush u8*3 u32 bytes opt[ u32 ] loop[ { u32*2 } ] }`
- Bit fields (message of zeros, widths in order written): byte 0: 6 · 1 · 1 · 1 · (7 unused) · 8 · 8 · 8
- HermesProxy: `CreateCharacter` — matches

### CMSG_CHECK_CHARACTER_NAME_AVAILABILITY (0x3646)

- Modern: 13894 (0x3646) · 2.4.3: —
- Layout (after the opcode): `u32 bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 6 · (2 unused)

### CMSG_SUPPORT_TICKET_SUBMIT_COMPLAINT (0x3647)

- Modern: 13895 (0x3647) · 2.4.3: —
- Layout (after the opcode): `{ { u32 f32*4 } guid { u32 flush loop[ u64 u8 bits(4) flush bytes ] opt[ u32 ] } bits(5) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ flush ] { u32 loop[ u64 guid opt[ u8 ] opt[ u8 ] opt[ u8 ] u8 bits(4) flush opt[ u64 ] opt[ guid ] opt[ u32 u16 u8 ] opt[ u32 ] bytes ] } bytes opt[ u32 u8 bits(5) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes ] opt[ u64*2 u8 flush bytes ] opt[ guid u8 flush bytes ] opt[ bits(7) flush guid bytes ] opt[ { { guid u32*2 u64 } u32 guid guid guid guid guid u8 bits(2) opt[ u8 ] alt[ u8 ] opt[ u8 ] opt[ u8 ] alt[ u8 ] flush bytes bytes bytes } ] opt[ { guid u32*2 u64 } u8 flush bytes ] opt[ u64*2 guid u8 bits(4) flush bytes ] opt[ bits(7) flush guid bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 26: 1 · (7 unused) · 5 · 10 · (9 unused)
- HermesProxy: `SupportTicketSubmitComplaint` — differs

### — (0x3648)

- Modern: 13896 (0x3648) · 2.4.3: —
- Layout (after the opcode): `{ u32 f32*4 } u8 bits(2) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 20: 10 · (6 unused)

### — (0x3649)

- Modern: 13897 (0x3649) · 2.4.3: —
- Layout (after the opcode): `{ u32 f32*4 } u8 bits(2) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 20: 10 · (6 unused)

### CMSG_PARTY_UNINVITE (0x364a)

- Modern: 13898 (0x364a) · 2.4.3: —
- Layout (after the opcode): `u8 guid u8 flush bytes`
- HermesProxy: `PartyUninvite` — matches

### CMSG_SET_LOOT_METHOD (0x364b)

- Modern: 13899 (0x364b) · 2.4.3: 122 (0x7a)
- Layout (after the opcode): `u8*2 guid u32`
- Size: 6 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 0: 8 · 8
- HermesProxy: `SetLootMethod` — matches

### CMSG_LEAVE_GROUP (0x364c)

- Modern: 13900 (0x364c) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `LeaveGroup` — matches

### CMSG_SET_PARTY_LEADER (0x364d)

- Modern: 13901 (0x364d) · 2.4.3: 120 (0x78)
- Layout (after the opcode): `u8 guid`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `SetPartyLeader` — matches

### CMSG_MINIMAP_PING (0x364e)

- Modern: 13902 (0x364e) · 2.4.3: —
- Layout (after the opcode): `f32*2 u8`
- Size: 9 bytes (packed GUIDs not counted)
- HermesProxy: `MinimapPingClient` — matches

### CMSG_GROUP_CHANGE_SUB_GROUP (0x364f)

- Modern: 13903 (0x364f) · 2.4.3: 638 (0x27e)
- Layout (after the opcode): `guid u8*2`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8
- HermesProxy: `ChangeSubGroup` — matches

### CMSG_GROUP_SWAP_SUB_GROUP (0x3650)

- Modern: 13904 (0x3650) · 2.4.3: 640 (0x280)
- Layout (after the opcode): `u8 guid guid`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `SwapSubGroups` — matches

### CMSG_CONVERT_RAID (0x3651)

- Modern: 13905 (0x3651) · 2.4.3: 654 (0x28e)
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `ConvertRaid` — matches

### CMSG_SET_ASSISTANT_LEADER (0x3652)

- Modern: 13906 (0x3652) · 2.4.3: 655 (0x28f)
- Layout (after the opcode): `u8 guid flush`
- Bit fields (message of zeros, widths in order written): byte 3: 1 · (7 unused)
- HermesProxy: `SetAssistantLeader` — matches

### CMSG_UPDATE_RAID_TARGET (0x3653)

- Modern: 13907 (0x3653) · 2.4.3: —
- Layout (after the opcode): `u8 guid u8`
- Size: 2 bytes (packed GUIDs not counted)
- HermesProxy: `UpdateRaidTarget` — matches

### CMSG_SET_PARTY_ASSIGNMENT (0x3654)

- Modern: 13908 (0x3654) · 2.4.3: —
- Layout (after the opcode): `u8*2 guid flush`
- Bit fields (message of zeros, widths in order written): byte 0: 8 · 8; byte 4: 1 · (7 unused)

### CMSG_SILENCE_PARTY_TALKER (0x3655)

- Modern: 13909 (0x3655) · 2.4.3: —
- Layout (after the opcode): `u8 guid flush`
- Bit fields (message of zeros, widths in order written): byte 3: 1 · (7 unused)

### CMSG_REQUEST_PARTY_MEMBER_STATS (0x3656)

- Modern: 13910 (0x3656) · 2.4.3: 639 (0x27f)
- Layout (after the opcode): `u8 guid`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `RequestPartyMemberStats` — matches

### CMSG_RANDOM_ROLL (0x3657)

- Modern: 13911 (0x3657) · 2.4.3: —
- Layout (after the opcode): `u32*2 u8`
- Size: 9 bytes (packed GUIDs not counted)
- HermesProxy: `RandomRollClient` — matches

### CMSG_MAIL_RETURN_TO_SENDER (0x3658)

- Modern: 13912 (0x3658) · 2.4.3: 584 (0x248)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `MailReturnToSender` — matches

### CMSG_QUERY_SCENARIO_POI (0x3659)

- Modern: 13913 (0x3659) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_TOGGLE_DIFFICULTY (0x365a)

- Modern: 13914 (0x365a) · 2.4.3: —
- Layout (after the opcode): `bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 6 · (2 unused)

### — (0x365b)

- Modern: 13915 (0x365b) · 2.4.3: —
- Layout (after the opcode): `bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 6 · (2 unused)

### CMSG_ADD_BATTLENET_FRIEND (0x365c)

- Modern: 13916 (0x365c) · 2.4.3: —
- Layout (after the opcode): `u64 u32 guid flush`
- Bit fields (message of zeros, widths in order written): byte 14: 1 · (7 unused)

### — (0x365d)

- Modern: 13917 (0x365d) · 2.4.3: —
- Layout (after the opcode): `guid u32 f32*4`
- Size: 20 bytes (packed GUIDs not counted)

### — (0x365e)

- Modern: 13918 (0x365e) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x365f)

- Modern: 13919 (0x365f) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3660)

- Modern: 13920 (0x3660) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3661)

- Modern: 13921 (0x3661) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_QUERY_CORPSE_LOCATION_FROM_CLIENT (0x3662)

- Modern: 13922 (0x3662) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `QueryCorpseLocationFromClient` — matches

### CMSG_QUERY_CORPSE_TRANSPORT (0x3663)

- Modern: 13923 (0x3663) · 2.4.3: —
- Layout (after the opcode): `guid guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_CAN_DUEL (0x3664)

- Modern: 13924 (0x3664) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `CanDuel` — matches

### — (0x3665)

- Modern: 13925 (0x3665) · 2.4.3: —
- Layout (after the opcode): `u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · (7 unused)

### CMSG_UPDATE_CLIENT_SETTINGS (0x3666)

- Modern: 13926 (0x3666) · 2.4.3: —
- Layout (after the opcode): `{ f32 }`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3667)

- Modern: 13927 (0x3667) · 2.4.3: —
- Layout (after the opcode): `u8 bits(2) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 10 · (6 unused)

### — (0x3668)

- Modern: 13928 (0x3668) · 2.4.3: —
- Layout (after the opcode): `u8 bits(2) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 10 · (6 unused)

### — (0x3669)

- Modern: 13929 (0x3669) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_RESET_INSTANCES (0x366a)

- Modern: 13930 (0x366a) · 2.4.3: 797 (0x31d)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### — (0x366b)

- Modern: 13931 (0x366b) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_SUMMON_RESPONSE (0x366c)

- Modern: 13932 (0x366c) · 2.4.3: 684 (0x2ac)
- Layout (after the opcode): `guid flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)
- HermesProxy: `SummonResponse` — matches

### — (0x366d)

- Modern: 13933 (0x366d) · 2.4.3: —
- Layout (after the opcode): `u32*3 opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 12: 1 · 1 · (6 unused)

### CMSG_COMPLAINT (0x366e)

- Modern: 13934 (0x366e) · 2.4.3: 966 (0x3c6)
- Layout (after the opcode): `u8 { guid u32*2 } opt[ u32 ] opt[ u64*2 ] opt[ { u32*2 u8 bits(4) flush bytes } ]`

### — (0x366f)

- Modern: 13935 (0x366f) · 2.4.3: —
- Layout (after the opcode): `u8 flush bytes`

### — (0x3670)

- Modern: 13936 (0x3670) · 2.4.3: —
- Layout (after the opcode): `u8 flush bytes`

### CMSG_CALENDAR_GET (0x3671)

- Modern: 13937 (0x3671) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_GET_EVENT (0x3672)

- Modern: 13938 (0x3672) · 2.4.3: —
- Layout (after the opcode): `u64`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_COMMUNITY_INVITE (0x3673)

- Modern: 13939 (0x3673) · 2.4.3: —
- Layout (after the opcode): `u64 u8*3`
- Size: 11 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_INVITE (0x3674)

- Modern: 13940 (0x3674) · 2.4.3: —
- Layout (after the opcode): `u64*3 u8 opt[ u8 ] flush bytes`
- Bit fields (message of zeros, widths in order written): byte 24: 9 · 1 · 1 · (5 unused)

### CMSG_CALENDAR_REMOVE_INVITE (0x3675)

- Modern: 13941 (0x3675) · 2.4.3: —
- Layout (after the opcode): `guid u64*3`
- Size: 24 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_RSVP (0x3676)

- Modern: 13942 (0x3676) · 2.4.3: —
- Layout (after the opcode): `u64*2 u8`
- Size: 17 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_STATUS (0x3677)

- Modern: 13943 (0x3677) · 2.4.3: —
- Layout (after the opcode): `guid u64*3 u8`
- Size: 25 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_MODERATOR_STATUS (0x3678)

- Modern: 13944 (0x3678) · 2.4.3: —
- Layout (after the opcode): `guid u64*3 u8`
- Size: 25 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_REMOVE_EVENT (0x3679)

- Modern: 13945 (0x3679) · 2.4.3: —
- Layout (after the opcode): `u64*3 u32`
- Size: 28 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_COPY_EVENT (0x367a)

- Modern: 13946 (0x367a) · 2.4.3: —
- Layout (after the opcode): `u64*3 u32`
- Size: 28 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_COMPLAIN (0x367b)

- Modern: 13947 (0x367b) · 2.4.3: —
- Layout (after the opcode): `guid u64*2`
- Size: 16 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_GET_NUM_PENDING (0x367c)

- Modern: 13948 (0x367c) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_EVENT_SIGN_UP (0x367d)

- Modern: 13949 (0x367d) · 2.4.3: —
- Layout (after the opcode): `u64*2 flush`
- Bit fields (message of zeros, widths in order written): byte 16: 1 · (7 unused)

### — (0x367e)

- Modern: 13950 (0x367e) · 2.4.3: —
- Layout (after the opcode): `guid u64*3 u8 flush bytes`

### CMSG_CALENDAR_ADD_EVENT (0x367f)

- Modern: 13951 (0x367f) · 2.4.3: —
- Layout (after the opcode): `{ u64 u8 u32*4 u8*2 bits(3) flush loop[ guid u8*2 opt[ u8 ] opt[ u8 ] flush opt[ guid ] opt[ u64 ] opt[ u64 ] ] bytes bytes } u32`
- Bit fields (message of zeros, widths in order written): byte 25: 8 · 11 · (5 unused)

### CMSG_CALENDAR_UPDATE_EVENT (0x3680)

- Modern: 13952 (0x3680) · 2.4.3: —
- Layout (after the opcode): `{ u64*3 u8 u32*3 u8*2 bits(3) flush bytes bytes } u32`
- Bit fields (message of zeros, widths in order written): byte 37: 8 · 11 · (5 unused)

### CMSG_KEEP_ALIVE (0x3681)

- Modern: 13953 (0x3681) · 2.4.3: 1030 (0x406)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_WHO_IS (0x3682)

- Modern: 13954 (0x3682) · 2.4.3: 100 (0x64)
- Layout (after the opcode): `bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 6 · (2 unused)

### CMSG_WHO (0x3683)

- Modern: 13955 (0x3683) · 2.4.3: 98 (0x62)
- Layout (after the opcode): `bits(4) flush { u32*2 u64 u32 bits(6) opt[ u8 ] alt[ u8 ] opt[ u8 ] bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] bits(3) opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush loop[ bits(7) flush bytes ] bytes bytes bytes bytes opt[ u32*3 ] } u32 loop[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 4 · (4 unused); byte 21: 6 · 9 · 7 · 9 · 3 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `WhoRequestPkt` — matches

### CMSG_SET_DUNGEON_DIFFICULTY (0x3684)

- Modern: 13956 (0x3684) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `SetDungeonDifficulty` — matches

### CMSG_RESURRECT_RESPONSE (0x3685)

- Modern: 13957 (0x3685) · 2.4.3: 348 (0x15c)
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `ResurrectResponse` — matches

### CMSG_PET_RENAME (0x3686)

- Modern: 13958 (0x3686) · 2.4.3: 375 (0x177)
- Layout (after the opcode): `{ guid u32 u8 loop[ opt[ u8 ] alt[ u8 ] ] flush loop[ bytes ] bytes }`
- Bit fields (message of zeros, widths in order written): byte 6: 8 · 1 · (5 unused) · 1 · (1 unused)
- HermesProxy: `PetRename` — differs

### CMSG_BUG_REPORT (0x3687)

- Modern: 13959 (0x3687) · 2.4.3: —
- Layout (after the opcode): `u8 bits(4) opt[ u8 ] alt[ u8 ] bits(2) flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 12 · 10 · (1 unused)

### — (0x3688)

- Modern: 13960 (0x3688) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_SET_PLAYER_DECLINED_NAMES (0x3689)

- Modern: 13961 (0x3689) · 2.4.3: 1048 (0x418)
- Layout (after the opcode): `guid loop[ opt[ u8 ] alt[ u8 ] ] flush loop[ bytes ]`
- HermesProxy: `SetPlayerDeclinedNames` — matches

### CMSG_QUERY_REALM_NAME (0x368a)

- Modern: 13962 (0x368a) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_QUERY_GUILD_INFO (0x368b)

- Modern: 13963 (0x368b) · 2.4.3: 84 (0x54)
- Layout (after the opcode): `guid guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `QueryGuildInfo` — matches

### CMSG_CHAR_CUSTOMIZE (0x368c)

- Modern: 13964 (0x368c) · 2.4.3: —
- Layout (after the opcode): `guid u8 u32 loop[ { u32*2 } ] bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 7: 6 · (2 unused)

### — (0x368d)

- Modern: 13965 (0x368d) · 2.4.3: —
- Layout (after the opcode): `u8 flush bytes`

### CMSG_GM_TICKET_GET_SYSTEM_STATUS (0x368e)

- Modern: 13966 (0x368e) · 2.4.3: 538 (0x21a)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_GM_TICKET_GET_CASE_STATUS (0x368f)

- Modern: 13967 (0x368f) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_GM_TICKET_ACKNOWLEDGE_SURVEY (0x3690)

- Modern: 13968 (0x3690) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3691)

- Modern: 13969 (0x3691) · 2.4.3: —
- Layout (after the opcode): `bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 6 · (2 unused)

### CMSG_CHAR_RACE_OR_FACTION_CHANGE (0x3692)

- Modern: 13970 (0x3692) · 2.4.3: —
- Layout (after the opcode): `bits(6) flush guid u8*2 u32 bytes loop[ { u32*2 } ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 6 · (1 unused); byte 3: 8 · 8

### CMSG_SUBMIT_USER_FEEDBACK (0x3693)

- Modern: 13971 (0x3693) · 2.4.3: —
- Layout (after the opcode): `{ u32 f32*4 } loop[ u8 ] flush bytes`
- Bit fields (message of zeros, widths in order written): byte 20: (23 unused) · 1 · 1 · (7 unused)

### CMSG_REQUEST_ACCOUNT_DATA (0x3694)

- Modern: 13972 (0x3694) · 2.4.3: 522 (0x20a)
- Layout (after the opcode): `guid bits(4) flush`
- Bit fields (message of zeros, widths in order written): byte 2: 4 · (4 unused)
- HermesProxy: `RequestAccountData` — matches

### CMSG_UPDATE_ACCOUNT_DATA (0x3695)

- Modern: 13973 (0x3695) · 2.4.3: 523 (0x20b)
- Layout (after the opcode): `guid u64 u32 bits(4) flush u32 bytes`
- Bit fields (message of zeros, widths in order written): byte 14: 4 · (4 unused)
- HermesProxy: `UserClientUpdateAccountData` — matches

### — (0x3696)

- Modern: 13974 (0x3696) · 2.4.3: —
- Layout (after the opcode): `u8 u32*3 opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 13: 1 · 1 · (6 unused)

### — (0x3697)

- Modern: 13975 (0x3697) · 2.4.3: —
- Layout (after the opcode): `u8 u32*3 opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 13: 1 · 1 · (6 unused)

### — (0x3698)

- Modern: 13976 (0x3698) · 2.4.3: —
- Layout (after the opcode): `u32 u8*2`
- Size: 6 bytes (packed GUIDs not counted)

### — (0x3699)

- Modern: 13977 (0x3699) · 2.4.3: —
- Layout (after the opcode): `u8 u32 flush`
- Bit fields (message of zeros, widths in order written): byte 5: 1 · (7 unused)

### — (0x369a)

- Modern: 13978 (0x369a) · 2.4.3: —
- Layout (after the opcode): `u64`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_SERVER_TIME_OFFSET_REQUEST (0x369b)

- Modern: 13979 (0x369b) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_CHAR_DELETE (0x369c)

- Modern: 13980 (0x369c) · 2.4.3: 56 (0x38)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `CharDelete` — matches

### — (0x369d)

- Modern: 13981 (0x369d) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x369e)

- Modern: 13982 (0x369e) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x369f)

- Modern: 13983 (0x369f) · 2.4.3: —
- Layout (after the opcode): `bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)

### CMSG_LOW_LEVEL_RAID1 (0x36a0)

- Modern: 13984 (0x36a0) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x36a1)

- Modern: 13985 (0x36a1) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_INSPECT_PVP (0x36a2)

- Modern: 13986 (0x36a2) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `Inspect` — matches

### CMSG_ARENA_TEAM_QUERY (0x36a3)

- Modern: 13987 (0x36a3) · 2.4.3: 843 (0x34b)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `ArenaTeamQuery` — matches

### — (0x36a4)

- Modern: 13988 (0x36a4) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x36a5)

- Modern: 13989 (0x36a5) · 2.4.3: —
- Layout (after the opcode): `u32 u8 bits(2) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 10 · (6 unused)

### — (0x36a6)

- Modern: 13990 (0x36a6) · 2.4.3: —
- Layout (after the opcode): `u32 u8 bits(2) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 10 · (6 unused)

### — (0x36a7)

- Modern: 13991 (0x36a7) · 2.4.3: —
- Layout (after the opcode): `u8 bits(3) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 11 · (5 unused)

### — (0x36a8)

- Modern: 13992 (0x36a8) · 2.4.3: —
- Layout (after the opcode): `bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 6 · (2 unused)

### — (0x36a9)

- Modern: 13993 (0x36a9) · 2.4.3: —
- Layout (after the opcode): `u8 bits(2) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 10 · (6 unused)

### — (0x36aa)

- Modern: 13994 (0x36aa) · 2.4.3: —
- Layout (after the opcode): `guid u32*3 loop[ u8 ] opt[ u8 ] flush`

### — (0x36ab)

- Modern: 13995 (0x36ab) · 2.4.3: —
- Layout (after the opcode): `bits(6) flush u32 bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 6 · (2 unused)

### — (0x36ac)

- Modern: 13996 (0x36ac) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x36ad)

- Modern: 13997 (0x36ad) · 2.4.3: —
- Layout (after the opcode): `u64`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x36ae)

- Modern: 13998 (0x36ae) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x36af)

- Modern: 13999 (0x36af) · 2.4.3: —
- Layout (after the opcode): `u32 bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 6 · (2 unused)

### — (0x36b0)

- Modern: 14000 (0x36b0) · 2.4.3: —
- Layout (after the opcode): `u32 bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 6 · (2 unused)

### — (0x36b1)

- Modern: 14001 (0x36b1) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x36b2)

- Modern: 14002 (0x36b2) · 2.4.3: —
- Layout (after the opcode): `bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 6 · (2 unused)

### — (0x36b3)

- Modern: 14003 (0x36b3) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x36b4)

- Modern: 14004 (0x36b4) · 2.4.3: —
- Layout (after the opcode): `u32 bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 6 · (2 unused)

### — (0x36b5)

- Modern: 14005 (0x36b5) · 2.4.3: —
- Layout (after the opcode): `bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 6 · (2 unused)

### CMSG_ARENA_TEAM_ROSTER (0x36b6)

- Modern: 14006 (0x36b6) · 2.4.3: 845 (0x34d)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `ArenaTeamRosterRequest` — matches

### CMSG_ARENA_TEAM_ACCEPT (0x36b7)

- Modern: 14007 (0x36b7) · 2.4.3: 849 (0x351)
- Layout (after the opcode): `guid guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ArenaTeamAccept` — matches

### CMSG_ARENA_TEAM_DECLINE (0x36b8)

- Modern: 14008 (0x36b8) · 2.4.3: 850 (0x352)
- Layout (after the opcode): `guid guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ArenaTeamAccept` — matches

### CMSG_ARENA_TEAM_LEAVE (0x36b9)

- Modern: 14009 (0x36b9) · 2.4.3: 851 (0x353)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `ArenaTeamLeave` — matches

### CMSG_ARENA_TEAM_REMOVE (0x36ba)

- Modern: 14010 (0x36ba) · 2.4.3: 852 (0x354)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `ArenaTeamRemove` — matches

### CMSG_ARENA_TEAM_DISBAND (0x36bb)

- Modern: 14011 (0x36bb) · 2.4.3: 853 (0x355)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `ArenaTeamLeave` — matches

### CMSG_ARENA_TEAM_LEADER (0x36bc)

- Modern: 14012 (0x36bc) · 2.4.3: 854 (0x356)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `ArenaTeamRemove` — matches

### CMSG_GET_ACCOUNT_CHARACTER_LIST (0x36bd)

- Modern: 14013 (0x36bd) · 2.4.3: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)
- HermesProxy: `GetAccountCharacterListRequest` — matches

### CMSG_LIVE_REGION_GET_ACCOUNT_CHARACTER_LIST (0x36be)

- Modern: 14014 (0x36be) · 2.4.3: —
- Layout (after the opcode): `u32 u8*2 bits(6) flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 9 · 6 · (1 unused)

### CMSG_LIVE_REGION_CHARACTER_COPY (0x36bf)

- Modern: 14015 (0x36bf) · 2.4.3: —
- Layout (after the opcode): `u32 u8 u32 guid bits(6) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`

### CMSG_LIVE_REGION_ACCOUNT_RESTORE (0x36c0)

- Modern: 14016 (0x36c0) · 2.4.3: —
- Layout (after the opcode): `u32 u8 guid u8 bits(6) flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 7: 9 · 6 · (1 unused)

### CMSG_LIVE_REGION_KEY_BINDINGS_COPY (0x36c1)

- Modern: 14017 (0x36c1) · 2.4.3: —
- Layout (after the opcode): `u32 u8 u32 guid bits(6) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`

### CMSG_BATTLE_PAY_GET_PRODUCT_LIST (0x36c2)

- Modern: 14018 (0x36c2) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PAY_GET_PURCHASE_LIST (0x36c3)

- Modern: 14019 (0x36c3) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x36c4)

- Modern: 14020 (0x36c4) · 2.4.3: —
- Layout (after the opcode): `guid u8 flush bytes`

### — (0x36c5)

- Modern: 14021 (0x36c5) · 2.4.3: —
- Layout (after the opcode): `u64 u32*2`
- Size: 16 bytes (packed GUIDs not counted)

### — (0x36c6)

- Modern: 14022 (0x36c6) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_CHARACTER_RENAME_REQUEST (0x36c7)

- Modern: 14023 (0x36c7) · 2.4.3: 711 (0x2c7)
- Layout (after the opcode): `guid bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 2: 6 · (2 unused)
- HermesProxy: `CharacterRenameRequest` — matches

### CMSG_SHOW_TRADE_SKILL (0x36c8)

- Modern: 14024 (0x36c8) · 2.4.3: —
- Layout (after the opcode): `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PAY_DISTRIBUTION_ASSIGN_TO_TARGET (0x36c9)

- Modern: 14025 (0x36c9) · 2.4.3: —
- Layout (after the opcode): `u32 u64 guid u32`
- Size: 16 bytes (packed GUIDs not counted)

### CMSG_CHARACTER_UPGRADE_MANUAL_UNREVOKE_REQUEST (0x36ca)

- Modern: 14026 (0x36ca) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_CHARACTER_UPGRADE_START (0x36cb)

- Modern: 14027 (0x36cb) · 2.4.3: —
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CHARACTER_CHECK_UPGRADE (0x36cc)

- Modern: 14028 (0x36cc) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x36cd)

- Modern: 14029 (0x36cd) · 2.4.3: —
- Layout (after the opcode): `u32 u64 u32 guid`
- Size: 16 bytes (packed GUIDs not counted)

### CMSG_GUILD_SET_GUILD_MASTER (0x36ce)

- Modern: 14030 (0x36ce) · 2.4.3: 144 (0x90)
- Layout (after the opcode): `u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · (7 unused)
- HermesProxy: `GuildSetGuildMaster` — matches

### CMSG_PETITION_RENAME_GUILD (0x36cf)

- Modern: 14031 (0x36cf) · 2.4.3: —
- Layout (after the opcode): `guid bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 2: 7 · (1 unused)
- HermesProxy: `PetitionRenameGuild` — matches

### CMSG_REQUEST_RAID_INFO (0x36d0)

- Modern: 14032 (0x36d0) · 2.4.3: 717 (0x2cd)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_BATTLE_PAY_START_PURCHASE (0x36d1)

- Modern: 14033 (0x36d1) · 2.4.3: —
- Layout (after the opcode): `u32*2 guid bits(6) opt[ u8 ] alt[ u8 ] bits(4) bits(7) flush bytes bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 10: 6 · 12; byte 12: 7 · (7 unused)

### CMSG_BATTLE_PAY_CONFIRM_PURCHASE_RESPONSE (0x36d2)

- Modern: 14034 (0x36d2) · 2.4.3: —
- Layout (after the opcode): `flush u32 u64`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### CMSG_BATTLE_PAY_ACK_FAILED_RESPONSE (0x36d3)

- Modern: 14035 (0x36d3) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x36d4)

- Modern: 14036 (0x36d4) · 2.4.3: —
- Layout (after the opcode): `u64`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_CONTACT_LIST (0x36d5)

- Modern: 14037 (0x36d5) · 2.4.3: 102 (0x66)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `ContactListRequest` — matches

### CMSG_ADD_FRIEND (0x36d6)

- Modern: 14038 (0x36d6) · 2.4.3: 105 (0x69)
- Layout (after the opcode): `u8*2 bits(2) flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · 10 · (5 unused)
- HermesProxy: `AddFriend` — matches

### CMSG_DEL_FRIEND (0x36d7)

- Modern: 14039 (0x36d7) · 2.4.3: 106 (0x6a)
- Layout (after the opcode): `{ u32 guid }`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `DelFriend` — matches

### CMSG_SET_CONTACT_NOTES (0x36d8)

- Modern: 14040 (0x36d8) · 2.4.3: 107 (0x6b)
- Layout (after the opcode): `{ u32 guid } u8 bits(2) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 6: 10 · (6 unused)
- HermesProxy: `SetContactNotes` — matches

### CMSG_BATTLENET_CHALLENGE_RESPONSE (0x36d9)

- Modern: 14041 (0x36d9) · 2.4.3: —
- Layout (after the opcode): `u32 bits(3) opt[ bits(6) ] flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 4: 3 · (5 unused)

### CMSG_ADD_IGNORE (0x36da)

- Modern: 14042 (0x36da) · 2.4.3: 108 (0x6c)
- Layout (after the opcode): `u8 flush guid bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · (7 unused)
- HermesProxy: `AddIgnore` — matches

### CMSG_DEL_IGNORE (0x36db)

- Modern: 14043 (0x36db) · 2.4.3: 109 (0x6d)
- Layout (after the opcode): `{ u32 guid }`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `DelFriend` — matches

### — (0x36dc)

- Modern: 14044 (0x36dc) · 2.4.3: —
- Layout (after the opcode): `guid u32 u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 6: 9 · (7 unused)

### — (0x36dd)

- Modern: 14045 (0x36dd) · 2.4.3: —
- Layout (after the opcode): `guid u32 u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 6: 9 · (7 unused)

### — (0x36de)

- Modern: 14046 (0x36de) · 2.4.3: —
- Layout (after the opcode): `guid u32*2 u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 10: 9 · (7 unused)

### — (0x36df)

- Modern: 14047 (0x36df) · 2.4.3: —
- Layout (after the opcode): `guid u32 { u8 u32*3 flush opt[ u64 ] } u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 19: 1 · (7 unused) · 9 · (7 unused)

### — (0x36e0)

- Modern: 14048 (0x36e0) · 2.4.3: —
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SET_RAID_DIFFICULTY (0x36e1)

- Modern: 14049 (0x36e1) · 2.4.3: —
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)
- HermesProxy: `SetRaidDifficulty` — reads the first part

### CMSG_TUTORIAL_FLAG (0x36e2)

- Modern: 14050 (0x36e2) · 2.4.3: 254 (0xfe)
- Layout (after the opcode): `bits(2) flush opt[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused)
- HermesProxy: `TutorialSetFlag` — matches

### CMSG_ENUM_CHARACTERS_DELETED_BY_CLIENT (0x36e3)

- Modern: 14051 (0x36e3) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_UNDELETE_CHARACTER (0x36e4)

- Modern: 14052 (0x36e4) · 2.4.3: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GET_UNDELETE_CHARACTER_COOLDOWN_STATUS (0x36e5)

- Modern: 14053 (0x36e5) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x36e6)

- Modern: 14054 (0x36e6) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x36e7)

- Modern: 14055 (0x36e7) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x36e8)

- Modern: 14056 (0x36e8) · 2.4.3: —
- Layout (after the opcode): `f32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_ENGINE_SURVEY (0x36e9)

- Modern: 14057 (0x36e9) · 2.4.3: —
- Layout (after the opcode): `{ u32*6 u8*2 u32 u8 u32*4 u64 u32 u8*2 u32*6 u8*4 u64*2 u32 opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush }`
- Bit fields (message of zeros, widths in order written): byte 24: 8 · 8; byte 59: 8 · 8; byte 85: 8 · 8 · 8 · 8; byte 109: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (7 unused)

### CMSG_COMMERCE_TOKEN_GET_COUNT (0x36ea)

- Modern: 14058 (0x36ea) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_COMMERCE_TOKEN_GET_MARKET_PRICE (0x36eb)

- Modern: 14059 (0x36eb) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_AUCTIONABLE_TOKEN_SELL (0x36ec)

- Modern: 14060 (0x36ec) · 2.4.3: —
- Layout (after the opcode): `u64*2 u32`
- Size: 20 bytes (packed GUIDs not counted)

### CMSG_AUCTIONABLE_TOKEN_SELL_AT_MARKET_PRICE (0x36ed)

- Modern: 14061 (0x36ed) · 2.4.3: —
- Layout (after the opcode): `guid u32*2 u64 flush`
- Bit fields (message of zeros, widths in order written): byte 18: 1 · (7 unused)

### CMSG_CONSUMABLE_TOKEN_CAN_VETERAN_BUY (0x36ee)

- Modern: 14062 (0x36ee) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CONSUMABLE_TOKEN_BUY (0x36ef)

- Modern: 14063 (0x36ef) · 2.4.3: —
- Layout (after the opcode): `u32 guid u64`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_CONSUMABLE_TOKEN_BUY_AT_MARKET_PRICE (0x36f0)

- Modern: 14064 (0x36f0) · 2.4.3: —
- Layout (after the opcode): `u32*2 u64 flush`
- Bit fields (message of zeros, widths in order written): byte 16: 1 · (7 unused)

### CMSG_GET_REMAINING_GAME_TIME (0x36f1)

- Modern: 14065 (0x36f1) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CONSUMABLE_TOKEN_REDEEM (0x36f2)

- Modern: 14066 (0x36f2) · 2.4.3: —
- Layout (after the opcode): `u64 u32*2`
- Size: 16 bytes (packed GUIDs not counted)

### CMSG_CONSUMABLE_TOKEN_REDEEM_CONFIRMATION (0x36f3)

- Modern: 14067 (0x36f3) · 2.4.3: —
- Layout (after the opcode): `u32 u64 guid u32 flush`
- Bit fields (message of zeros, widths in order written): byte 18: 1 · (7 unused)

### CMSG_COMMERCE_TOKEN_GET_LOG (0x36f4)

- Modern: 14068 (0x36f4) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x36f5)

- Modern: 14069 (0x36f5) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_GET_VAS_ACCOUNT_CHARACTER_LIST (0x36f6)

- Modern: 14070 (0x36f6) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_GET_VAS_TRANSFER_TARGET_REALM_LIST (0x36f7)

- Modern: 14071 (0x36f7) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PAY_START_VAS_PURCHASE (0x36f8)

- Modern: 14072 (0x36f8) · 2.4.3: —
- Layout (after the opcode): `{ u32*2 guid u32*2 guid guid guid bits(6) bits(7) bits(7) bits(6) opt[ u8 ] alt[ u8 ] bits(4) opt[ u8 ] flush bytes bytes bytes bytes bytes }`
- Bit fields (message of zeros, widths in order written): byte 24: 6 · 7 · 7 · 6 · 12 · 1 · (1 unused)

### CMSG_UPDATE_VAS_PURCHASE_STATES (0x36f9)

- Modern: 14073 (0x36f9) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x36fa)

- Modern: 14074 (0x36fa) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_BATTLENET_REQUEST (0x36fb)

- Modern: 14075 (0x36fb) · 2.4.3: —
- Layout (after the opcode): `{ u64*2 u32 } u32 bytes`
- HermesProxy: `BattlenetRequest` — matches

### — (0x36fc)

- Modern: 14076 (0x36fc) · 2.4.3: —
- Layout (after the opcode): `flush u32`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### CMSG_CLUB_PRESENCE_SUBSCRIBE (0x36fd)

- Modern: 14077 (0x36fd) · 2.4.3: —
- Layout (after the opcode): `flush u32 loop[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x36fe)

- Modern: 14078 (0x36fe) · 2.4.3: —
- Layout (after the opcode): `flush u32 loop[ guid ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### CMSG_CHANGE_REALM_TICKET (0x36ff)

- Modern: 14079 (0x36ff) · 2.4.3: —
- Layout (after the opcode): `u32 loop[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 4: (250 unused) · 1 · (5 unused)
- HermesProxy: `ChangeRealmTicket` — matches

### — (0x3700)

- Modern: 14080 (0x3700) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3701)

- Modern: 14081 (0x3701) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3702)

- Modern: 14082 (0x3702) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x3703)

- Modern: 14083 (0x3703) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3704)

- Modern: 14084 (0x3704) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3705)

- Modern: 14085 (0x3705) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_REPORT_ENABLED_ADDONS (0x3706)

- Modern: 14086 (0x3706) · 2.4.3: —
- Layout (after the opcode): `u32 loop[ bits(7) bits(6) opt[ u8 ] opt[ u8 ] flush bytes bytes ]`

### CMSG_REPORT_CLIENT_VARIABLES (0x3707)

- Modern: 14087 (0x3707) · 2.4.3: —
- Layout (after the opcode): `u32 loop[ bits(6) opt[ u8 ] alt[ u8 ] bits(2) flush bytes bytes ]`

### CMSG_REPORT_KEYBINDING_EXECUTION_COUNTS (0x3708)

- Modern: 14088 (0x3708) · 2.4.3: —
- Layout (after the opcode): `u8 bits(2) flush loop[ bits(6) bits(6) flush u32 bytes bytes ]`
- Bit fields (message of zeros, widths in order written): byte 0: 10 · (6 unused)

### — (0x3709)

- Modern: 14089 (0x3709) · 2.4.3: —
- Layout (after the opcode): `u32 bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 6 · (2 unused)

### CMSG_QUICK_JOIN_SIGNAL_TOAST_DISPLAYED (0x370a)

- Modern: 14090 (0x370a) · 2.4.3: —
- Layout (after the opcode): `guid f32 u32 loop[ guid ] opt[ u8 ] flush`

### CMSG_QUICK_JOIN_RESPOND_TO_INVITE (0x370b)

- Modern: 14091 (0x370b) · 2.4.3: —
- Layout (after the opcode): `guid guid flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)

### CMSG_QUICK_JOIN_AUTO_ACCEPT_REQUESTS (0x370c)

- Modern: 14092 (0x370c) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### CMSG_CAN_REDEEM_TOKEN_FOR_BALANCE (0x370d)

- Modern: 14093 (0x370d) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PAY_REQUEST_PRICE_INFO (0x370e)

- Modern: 14094 (0x370e) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_VAS_GET_SERVICE_STATUS (0x370f)

- Modern: 14095 (0x370f) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_VAS_GET_QUEUE_MINUTES (0x3710)

- Modern: 14096 (0x3710) · 2.4.3: —
- Layout (after the opcode): `u64 u32`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_VAS_CHECK_TRANSFER_OK (0x3711)

- Modern: 14097 (0x3711) · 2.4.3: —
- Layout (after the opcode): `u32 u8 bits(3) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 11 · (5 unused)

### CMSG_BATTLE_PAY_OPEN_CHECKOUT (0x3712)

- Modern: 14098 (0x3712) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3713)

- Modern: 14099 (0x3713) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_VOICE_CHAT_LOGIN (0x3714)

- Modern: 14100 (0x3714) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_VOICE_CHANNEL_STT_TOKEN_REQUEST (0x3715)

- Modern: 14101 (0x3715) · 2.4.3: —
- Layout (after the opcode): `bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)

### CMSG_VOICE_CHAT_JOIN_CHANNEL (0x3716)

- Modern: 14102 (0x3716) · 2.4.3: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x3717)

- Modern: 14103 (0x3717) · 2.4.3: —
- Layout (after the opcode): `bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: (6 unused) · 1 · (1 unused)

### — (0x3718)

- Modern: 14104 (0x3718) · 2.4.3: —
- Layout (after the opcode): `bits(6) bits(7) opt[ u8 ] flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 6 · 7 · 1 · (2 unused)

### CMSG_BATTLE_PAY_CANCEL_OPEN_CHECKOUT (0x3719)

- Modern: 14105 (0x3719) · 2.4.3: —
- Layout (after the opcode): `bits(7) opt[ u8 ] flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · 1

### — (0x371a)

- Modern: 14106 (0x371a) · 2.4.3: —
- Layout (after the opcode): `u32*3 flush`
- Bit fields (message of zeros, widths in order written): byte 12: 1 · (7 unused)

### — (0x371b)

- Modern: 14107 (0x371b) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x371c)

- Modern: 14108 (0x371c) · 2.4.3: —
- Layout (after the opcode): `u32*2 flush`
- Bit fields (message of zeros, widths in order written): byte 8: 1 · (7 unused)

### — (0x371d)

- Modern: 14109 (0x371d) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_DO_COUNTDOWN (0x371e)

- Modern: 14110 (0x371e) · 2.4.3: —
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)

### CMSG_CLUB_FINDER_POST (0x371f)

- Modern: 14111 (0x371f) · 2.4.3: —
- Layout (after the opcode): `bits(7) opt[ u8 ] alt[ u8 ] bits(4) bits(3) flush u64*2 u32*3 bytes bytes`

### CMSG_CLUB_FINDER_REQUEST_CLUBS_LIST (0x3720)

- Modern: 14112 (0x3720) · 2.4.3: —
- Layout (after the opcode): `u8 bits(3) flush u32*2 bytes loop[ { bits(3) flush { bits(3) opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] loop[ opt[ u8 ] alt[ u8 ] ] flush opt[ bytes ] opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u64 ] } } ]`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · 3 · (4 unused)

### CMSG_CLUB_FINDER_REQUEST_MEMBERSHIP_TO_CLUB (0x3721)

- Modern: 14113 (0x3721) · 2.4.3: —
- Layout (after the opcode): `guid u64 u8 bits(2) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 10: 10 · (6 unused)

### CMSG_CLUB_FINDER_GET_APPLICANTS_LIST (0x3722)

- Modern: 14114 (0x3722) · 2.4.3: —
- Layout (after the opcode): `bits(3) flush`
- Bit fields (message of zeros, widths in order written): byte 0: 3 · (5 unused)

### CMSG_CLUB_FINDER_RESPOND_TO_APPLICANT (0x3723)

- Modern: 14115 (0x3723) · 2.4.3: —
- Layout (after the opcode): `guid guid bits(3) opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · 3 · 1 · (3 unused)

### CMSG_CLUB_FINDER_APPLICATION_RESPONSE (0x3724)

- Modern: 14116 (0x3724) · 2.4.3: —
- Layout (after the opcode): `guid bits(3) bits(3) flush`
- Bit fields (message of zeros, widths in order written): byte 2: 3 · 3 · (2 unused)

### CMSG_CLUB_FINDER_REQUEST_PENDING_CLUBS_LIST (0x3725)

- Modern: 14117 (0x3725) · 2.4.3: —
- Layout (after the opcode): `bits(3) flush`
- Bit fields (message of zeros, widths in order written): byte 0: 3 · (5 unused)

### CMSG_CLUB_FINDER_REQUEST_CLUBS_DATA (0x3726)

- Modern: 14118 (0x3726) · 2.4.3: —
- Layout (after the opcode): `u32*2 loop[ u32 ] bits(3) opt[ u8 ] flush loop[ { bits(3) flush { bits(3) opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] loop[ opt[ u8 ] alt[ u8 ] ] flush opt[ bytes ] opt[ u32 ] opt[ u32 ] opt[ u64 ] opt[ u64 ] } } ]`

### CMSG_CLUB_FINDER_REQUEST_SUBSCRIBED_CLUB_POSTING_IDS (0x3727)

- Modern: 14119 (0x3727) · 2.4.3: —
- Layout (after the opcode): `u32 loop[ u64 ]`

### CMSG_GET_RAF_ACCOUNT_INFO (0x3728)

- Modern: 14120 (0x3728) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_RAF_CLAIM_NEXT_REWARD (0x3729)

- Modern: 14121 (0x3729) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_RAF_UPDATE_RECRUITMENT_INFO (0x372a)

- Modern: 14122 (0x372a) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_RAF_GENERATE_RECRUITMENT_LINK (0x372b)

- Modern: 14123 (0x372b) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_REMOVE_RAF_RECRUIT (0x372c)

- Modern: 14124 (0x372c) · 2.4.3: —
- Layout (after the opcode): `u64`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x372d)

- Modern: 14125 (0x372d) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x372e)

- Modern: 14126 (0x372e) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x372f)

- Modern: 14127 (0x372f) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3730)

- Modern: 14128 (0x3730) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3731)

- Modern: 14129 (0x3731) · 2.4.3: —
- Layout (after the opcode): `guid guid u32*3 loop[ u8 ] flush bytes`
- Bit fields (message of zeros, widths in order written): byte 16: (23 unused) · 1

### — (0x3732)

- Modern: 14130 (0x3732) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_QUEST_SESSION_REQUEST_STOP (0x3733)

- Modern: 14131 (0x3733) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3734)

- Modern: 14132 (0x3734) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3735)

- Modern: 14133 (0x3735) · 2.4.3: —
- Layout (after the opcode): `{ opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush u32 }`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 1 · 1 · 1 · 1 · (3 unused)

### — (0x3736)

- Modern: 14134 (0x3736) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3737)

- Modern: 14135 (0x3737) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_QUICK_JOIN_REQUEST_INVITE_WITH_CONFIRMATION (0x3738)

- Modern: 14136 (0x3738) · 2.4.3: —
- Layout (after the opcode): `u8*2 flush u32 guid u32 bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · 9 · (6 unused)

### — (0x3739)

- Modern: 14137 (0x3739) · 2.4.3: —
- Layout (after the opcode): `u32 loop[ { u64 u32*2 } ]`

### CMSG_GET_ACCOUNT_NOTIFICATIONS (0x373a)

- Modern: 14138 (0x373a) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_ACCOUNT_NOTIFICATION_ACKNOWLEDGED (0x373b)

- Modern: 14139 (0x373b) · 2.4.3: —
- Layout (after the opcode): `u64 u32*2`
- Size: 16 bytes (packed GUIDs not counted)

### CMSG_AUCTION_SET_FAVORITE_ITEM (0x373c)

- Modern: 14140 (0x373c) · 2.4.3: —
- Layout (after the opcode): `flush { u32*5 }`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x373d)

- Modern: 14141 (0x373d) · 2.4.3: —
- Layout (after the opcode): `u64 opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 8: 1 · 1 · (6 unused)

### CMSG_SET_CHAT_DISABLED (0x373e)

- Modern: 14142 (0x373e) · 2.4.3: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### CMSG_BATTLE_PAY_DISTRIBUTION_ASSIGN_VAS (0x373f)

- Modern: 14143 (0x373f) · 2.4.3: —
- Layout (after the opcode): `{ u32 u64 guid u32*2 guid guid guid bits(6) bits(7) bits(7) opt[ u8 ] opt[ u8 ] flush bytes bytes bytes }`
- Bit fields (message of zeros, widths in order written): byte 28: 6 · 7 · 7 · 1 · 1 · (2 unused)

### — (0x3740)

- Modern: 14144 (0x3740) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_SUSPEND_COMMS_ACK (0x3764)

- Modern: 14180 (0x3764) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_AUTH_SESSION (0x3765)

- Modern: 14181 (0x3765) · 2.4.3: 493 (0x1ed)
- Layout (after the opcode): `u64 u32*3 loop[ u8 ] loop[ u8 ] flush u32 bytes`
- Bit fields (message of zeros, widths in order written): byte 20: 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 1 · (7 unused)

### CMSG_AUTH_CONTINUED_SESSION (0x3766)

- Modern: 14182 (0x3766) · 2.4.3: —
- Layout (after the opcode): `u64*2 loop[ u8 ] loop[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 16: (154 unused) · 1 · (165 unused)

### CMSG_ENTER_ENCRYPTED_MODE_ACK (0x3767)

- Modern: 14183 (0x3767) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_PING (0x3768)

- Modern: 14184 (0x3768) · 2.4.3: 476 (0x1dc)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_LOG_DISCONNECT (0x3769)

- Modern: 14185 (0x3769) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SUSPEND_TOKEN_RESPONSE (0x376a)

- Modern: 14186 (0x376a) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_ENABLE_NAGLE (0x376b)

- Modern: 14187 (0x376b) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_QUEUED_MESSAGES_END (0x376c)

- Modern: 14188 (0x376c) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_LOG_STREAMING_ERROR (0x376d)

- Modern: 14189 (0x376d) · 2.4.3: —
- Layout (after the opcode): `u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · (7 unused)

### — (0x376e)

- Modern: 14190 (0x376e) · 2.4.3: —
- Layout (after the opcode): `u32 loop[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 4: (250 unused) · 1 · (5 unused)

### CMSG_QUERY_PLAYER_NAME (0x376f)

- Modern: 14191 (0x376f) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `QueryPlayerName` — matches

### CMSG_QUERY_PLAYER_NAME_BY_COMMUNITY_ID (0x3770)

- Modern: 14192 (0x3770) · 2.4.3: —
- Layout (after the opcode): `{ guid u64 }`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_QUERY_PLAYER_NAMES_FOR_COMMUNITY (0x3771)

- Modern: 14193 (0x3771) · 2.4.3: —
- Layout (after the opcode): `u64 u32 loop[ { guid u64 } ]`

### CMSG_LATENCY_REPORT (0x3772)

- Modern: 14194 (0x3772) · 2.4.3: —
- Layout (after the opcode): `u32*2 loop[ { u32 u16 u8 u64 u32 } ]`

### CMSG_QUERY_PLAYER_NAMES (0x3773)

- Modern: 14195 (0x3773) · 2.4.3: —
- Layout (after the opcode): `u32 loop[ guid ]`
- HermesProxy: `QueryPlayerNames` — matches

### CMSG_CHAT_JOIN_CHANNEL (0x37c8)

- Modern: 14280 (0x37c8) · 2.4.3: 151 (0x97)
- Layout (after the opcode): `u32 opt[ u8 ] bits(7) bits(7) flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · 1 · 7 · 7
- HermesProxy: `JoinChannel` — differs

### CMSG_CHAT_LEAVE_CHANNEL (0x37c9)

- Modern: 14281 (0x37c9) · 2.4.3: 152 (0x98)
- Layout (after the opcode): `u32 bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 7 · (1 unused)
- HermesProxy: `LeaveChannel` — matches

### — (0x37ca)

- Modern: 14282 (0x37ca) · 2.4.3: —
- Layout (after the opcode): `u8*3 flush guid bytes bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · 9 · 8 · (6 unused)

### CMSG_CHAT_REPORT_IGNORED (0x37cb)

- Modern: 14283 (0x37cb) · 2.4.3: 549 (0x225)
- Layout (after the opcode): `guid u8`
- Size: 1 bytes (packed GUIDs not counted)

### CMSG_CHAT_REPORT_FILTERED (0x37cc)

- Modern: 14284 (0x37cc) · 2.4.3: 817 (0x331)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_CHAT_REGISTER_ADDON_PREFIXES (0x37cd)

- Modern: 14285 (0x37cd) · 2.4.3: —
- Layout (after the opcode): `u32 loop[ bits(5) flush bytes ]`
- HermesProxy: `ChatRegisterAddonPrefixes` — matches

### CMSG_CHAT_UNREGISTER_ALL_ADDON_PREFIXES (0x37ce)

- Modern: 14286 (0x37ce) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_CHAT_MESSAGE_CHANNEL (0x37cf)

- Modern: 14287 (0x37cf) · 2.4.3: —
- Layout (after the opcode): `u32 guid u8*2 flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 6: 9 · 9 · (6 unused)
- HermesProxy: `ChatMessageChannel` — matches

### CMSG_CHAT_MESSAGE_WHISPER (0x37d0)

- Modern: 14288 (0x37d0) · 2.4.3: —
- Layout (after the opcode): `u32 u8*2 flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 9 · 9 · (6 unused)
- HermesProxy: `ChatMessageWhisper` — matches

### CMSG_CHAT_MESSAGE_GUILD (0x37d1)

- Modern: 14289 (0x37d1) · 2.4.3: —
- Layout (after the opcode): `u32 u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 9 · (7 unused)
- HermesProxy: `ChatMessage` — matches

### CMSG_CHAT_MESSAGE_OFFICER (0x37d2)

- Modern: 14290 (0x37d2) · 2.4.3: —
- Layout (after the opcode): `u32 u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 9 · (7 unused)
- HermesProxy: `ChatMessage` — matches

### CMSG_CHAT_MESSAGE_AFK (0x37d3)

- Modern: 14291 (0x37d3) · 2.4.3: —
- Layout (after the opcode): `u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · (7 unused)
- HermesProxy: `ChatMessageAFK` — matches

### CMSG_CHAT_MESSAGE_DND (0x37d4)

- Modern: 14292 (0x37d4) · 2.4.3: —
- Layout (after the opcode): `u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · (7 unused)
- HermesProxy: `ChatMessageDND` — matches

### CMSG_CHAT_CHANNEL_LIST (0x37d5)

- Modern: 14293 (0x37d5) · 2.4.3: 154 (0x9a)
- Layout (after the opcode): `bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)
- HermesProxy: `ChannelCommand` — matches

### CMSG_CHAT_CHANNEL_DISPLAY_LIST (0x37d6)

- Modern: 14294 (0x37d6) · 2.4.3: 977 (0x3d1)
- Layout (after the opcode): `bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)
- HermesProxy: `ChannelCommand` — matches

### CMSG_CHAT_CHANNEL_PASSWORD (0x37d7)

- Modern: 14295 (0x37d7) · 2.4.3: 156 (0x9c)
- Layout (after the opcode): `bits(7) bits(7) flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · 7 · (2 unused)
- HermesProxy: `ChannelPassword` — matches

### CMSG_CHAT_CHANNEL_SET_OWNER (0x37d8)

- Modern: 14296 (0x37d8) · 2.4.3: 157 (0x9d)
- Layout (after the opcode): `bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`
- HermesProxy: `ChannelPlayerCommand` — matches

### CMSG_CHAT_CHANNEL_OWNER (0x37d9)

- Modern: 14297 (0x37d9) · 2.4.3: 158 (0x9e)
- Layout (after the opcode): `bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)
- HermesProxy: `ChannelCommand` — matches

### — (0x37da)

- Modern: 14298 (0x37da) · 2.4.3: —
- Layout (after the opcode): `bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)

### CMSG_CHAT_CHANNEL_MODERATOR (0x37db)

- Modern: 14299 (0x37db) · 2.4.3: 159 (0x9f)
- Layout (after the opcode): `bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`
- HermesProxy: `ChannelPlayerCommand` — matches

### CMSG_CHAT_CHANNEL_UNMODERATOR (0x37dc)

- Modern: 14300 (0x37dc) · 2.4.3: 160 (0xa0)
- Layout (after the opcode): `bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`
- HermesProxy: `ChannelPlayerCommand` — matches

### — (0x37dd)

- Modern: 14301 (0x37dd) · 2.4.3: —
- Layout (after the opcode): `bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`

### — (0x37de)

- Modern: 14302 (0x37de) · 2.4.3: —
- Layout (after the opcode): `bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`

### CMSG_CHAT_CHANNEL_INVITE (0x37df)

- Modern: 14303 (0x37df) · 2.4.3: 163 (0xa3)
- Layout (after the opcode): `bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`
- HermesProxy: `ChannelPlayerCommand` — matches

### CMSG_CHAT_CHANNEL_KICK (0x37e0)

- Modern: 14304 (0x37e0) · 2.4.3: 164 (0xa4)
- Layout (after the opcode): `bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`
- HermesProxy: `ChannelPlayerCommand` — matches

### CMSG_CHAT_CHANNEL_BAN (0x37e1)

- Modern: 14305 (0x37e1) · 2.4.3: 165 (0xa5)
- Layout (after the opcode): `bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`
- HermesProxy: `ChannelPlayerCommand` — matches

### CMSG_CHAT_CHANNEL_UNBAN (0x37e2)

- Modern: 14306 (0x37e2) · 2.4.3: 166 (0xa6)
- Layout (after the opcode): `bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`
- HermesProxy: `ChannelPlayerCommand` — matches

### CMSG_CHAT_CHANNEL_ANNOUNCEMENTS (0x37e3)

- Modern: 14307 (0x37e3) · 2.4.3: 167 (0xa7)
- Layout (after the opcode): `bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)
- HermesProxy: `ChannelCommand` — matches

### CMSG_CHAT_CHANNEL_SILENCE_ALL (0x37e4)

- Modern: 14308 (0x37e4) · 2.4.3: 972 (0x3cc)
- Layout (after the opcode): `bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`

### CMSG_CHAT_CHANNEL_UNSILENCE_ALL (0x37e5)

- Modern: 14309 (0x37e5) · 2.4.3: 974 (0x3ce)
- Layout (after the opcode): `bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`

### CMSG_CHAT_CHANNEL_DECLINE_INVITE (0x37e6)

- Modern: 14310 (0x37e6) · 2.4.3: 1039 (0x40f)
- Layout (after the opcode): `bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)
- HermesProxy: `ChannelCommand` — matches

### CMSG_CHAT_MESSAGE_SAY (0x37e7)

- Modern: 14311 (0x37e7) · 2.4.3: —
- Layout (after the opcode): `u32 u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 9 · (7 unused)
- HermesProxy: `ChatMessage` — matches

### CMSG_CHAT_MESSAGE_EMOTE (0x37e8)

- Modern: 14312 (0x37e8) · 2.4.3: —
- Layout (after the opcode): `u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · (7 unused)
- HermesProxy: `ChatMessageEmote` — matches

### CMSG_CHAT_MESSAGE_YELL (0x37e9)

- Modern: 14313 (0x37e9) · 2.4.3: —
- Layout (after the opcode): `u32 u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 9 · (7 unused)
- HermesProxy: `ChatMessage` — matches

### CMSG_CHAT_MESSAGE_PARTY (0x37ea)

- Modern: 14314 (0x37ea) · 2.4.3: —
- Layout (after the opcode): `u32 u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 9 · (7 unused)
- HermesProxy: `ChatMessage` — matches

### CMSG_CHAT_MESSAGE_RAID (0x37eb)

- Modern: 14315 (0x37eb) · 2.4.3: —
- Layout (after the opcode): `u32 u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 9 · (7 unused)
- HermesProxy: `ChatMessage` — matches

### CMSG_CHAT_MESSAGE_INSTANCE_CHAT (0x37ec)

- Modern: 14316 (0x37ec) · 2.4.3: —
- Layout (after the opcode): `u32 u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 9 · (7 unused)
- HermesProxy: `ChatMessage` — matches

### CMSG_CHAT_MESSAGE_RAID_WARNING (0x37ed)

- Modern: 14317 (0x37ed) · 2.4.3: —
- Layout (after the opcode): `u32 u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 9 · (7 unused)
- HermesProxy: `ChatMessage` — matches

### CMSG_CHAT_ADDON_MESSAGE (0x37ee)

- Modern: 14318 (0x37ee) · 2.4.3: —
- Layout (after the opcode): `{ bits(5) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush u32 bytes bytes }`
- Bit fields (message of zeros, widths in order written): byte 0: 5 · 8 · 1 · (2 unused)
- HermesProxy: `ChatAddonMessage` — matches

### CMSG_CHAT_ADDON_MESSAGE_TARGETED (0x37ef)

- Modern: 14319 (0x37ef) · 2.4.3: —
- Layout (after the opcode): `u8 flush { bits(5) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush u32 bytes bytes } guid bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · (7 unused) · 5 · 8 · 1 · (2 unused)
- HermesProxy: `ChatAddonMessageTargeted` — matches

### — (0x37f0)

- Modern: 14320 (0x37f0) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x37f1)

- Modern: 14321 (0x37f1) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x37f2)

- Modern: 14322 (0x37f2) · 2.4.3: —
- Layout (after the opcode): `u32 u16 u8`
- Size: 7 bytes (packed GUIDs not counted)

### — (0x37f3)

- Modern: 14323 (0x37f3) · 2.4.3: —
- Layout (after the opcode): `u32*2 f32 u32`
- Size: 16 bytes (packed GUIDs not counted)

### CMSG_WRAP_ITEM (0x3994)

- Modern: 14740 (0x3994) · 2.4.3: 467 (0x1d3)
- Layout (after the opcode): `{ bits(2) opt[ alt[ bits(7) alt[ bits(6) alt[ bits(5) alt[ bits(4) alt[ bits(3) alt[ bits(2) ] alt[ u8 ] ] ] ] ] ] ] loop[ u8*2 ] }`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused)
- HermesProxy: `WrapItem` — matches

### CMSG_USE_EQUIPMENT_SET (0x3995)

- Modern: 14741 (0x3995) · 2.4.3: —
- Layout (after the opcode): `{ bits(2) opt[ alt[ bits(7) alt[ bits(6) alt[ bits(5) alt[ bits(4) alt[ bits(3) alt[ bits(2) ] alt[ u8 ] ] ] ] ] ] ] loop[ u8*2 ] } loop[ guid u8*2 ] u64`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused); byte 3: 8 · 8; byte 7: 8 · 8; byte 11: 8 · 8; byte 15: 8 · 8; byte 19: 8 · 8; byte 23: 8 · 8; byte 27: 8 · 8; byte 31: 8 · 8; byte 35: 8 · 8; byte 39: 8 · 8; byte 43: 8 · 8; byte 47: 8 · 8; byte 51: 8 · 8; byte 55: 8 · 8; byte 59: 8 · 8; byte 63: 8 · 8; byte 67: 8 · 8; byte 71: 8 · 8; byte 75: 8 · 8
- HermesProxy: `UseEquipmentSet` — matches

### CMSG_AUTOSTORE_BANK_ITEM (0x3996)

- Modern: 14742 (0x3996) · 2.4.3: 642 (0x282)
- Layout (after the opcode): `{ bits(2) opt[ alt[ bits(7) alt[ bits(6) alt[ bits(5) alt[ bits(4) alt[ bits(3) alt[ bits(2) ] alt[ u8 ] ] ] ] ] ] ] loop[ u8*2 ] } u8*2`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused) · 8 · 8
- HermesProxy: `AutoEquipItem` — matches

### CMSG_AUTOBANK_ITEM (0x3997)

- Modern: 14743 (0x3997) · 2.4.3: 643 (0x283)
- Layout (after the opcode): `{ bits(2) opt[ alt[ bits(7) alt[ bits(6) alt[ bits(5) alt[ bits(4) alt[ bits(3) alt[ bits(2) ] alt[ u8 ] ] ] ] ] ] ] loop[ u8*2 ] } u8*2`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused) · 8 · 8
- HermesProxy: `AutoEquipItem` — matches

### CMSG_AUTO_EQUIP_ITEM (0x3998)

- Modern: 14744 (0x3998) · 2.4.3: 266 (0x10a)
- Layout (after the opcode): `{ bits(2) opt[ alt[ bits(7) alt[ bits(6) alt[ bits(5) alt[ bits(4) alt[ bits(3) alt[ bits(2) ] alt[ u8 ] ] ] ] ] ] ] loop[ u8*2 ] } u8*2`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused) · 8 · 8
- HermesProxy: `AutoEquipItem` — matches

### CMSG_AUTO_STORE_BAG_ITEM (0x3999)

- Modern: 14745 (0x3999) · 2.4.3: 267 (0x10b)
- Layout (after the opcode): `{ bits(2) opt[ alt[ bits(7) alt[ bits(6) alt[ bits(5) alt[ bits(4) alt[ bits(3) alt[ bits(2) ] alt[ u8 ] ] ] ] ] ] ] loop[ u8*2 ] } u8*3`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused) · 8 · 8 · 8
- HermesProxy: `AutoStoreBagItem` — matches

### CMSG_SWAP_ITEM (0x399a)

- Modern: 14746 (0x399a) · 2.4.3: 268 (0x10c)
- Layout (after the opcode): `{ bits(2) opt[ alt[ bits(7) alt[ bits(6) alt[ bits(5) alt[ bits(4) alt[ bits(3) alt[ bits(2) ] alt[ u8 ] ] ] ] ] ] ] loop[ u8*2 ] } u8*4`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused) · 8 · 8 · 8 · 8
- HermesProxy: `SwapItem` — matches

### CMSG_SWAP_INV_ITEM (0x399b)

- Modern: 14747 (0x399b) · 2.4.3: 269 (0x10d)
- Layout (after the opcode): `{ bits(2) opt[ alt[ bits(7) alt[ bits(6) alt[ bits(5) alt[ bits(4) alt[ bits(3) alt[ bits(2) ] alt[ u8 ] ] ] ] ] ] ] loop[ u8*2 ] } u8*2`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused) · 8 · 8
- HermesProxy: `SwapInvItem` — matches

### CMSG_SPLIT_ITEM (0x399c)

- Modern: 14748 (0x399c) · 2.4.3: 270 (0x10e)
- Layout (after the opcode): `{ bits(2) opt[ alt[ bits(7) alt[ bits(6) alt[ bits(5) alt[ bits(4) alt[ bits(3) alt[ bits(2) ] alt[ u8 ] ] ] ] ] ] ] loop[ u8*2 ] } u8*4 u32`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused) · 8 · 8 · 8 · 8
- HermesProxy: `SplitItem` — matches

### CMSG_AUTO_EQUIP_ITEM_SLOT (0x399d)

- Modern: 14749 (0x399d) · 2.4.3: 271 (0x10f)
- Layout (after the opcode): `{ bits(2) opt[ alt[ bits(7) alt[ bits(6) alt[ bits(5) alt[ bits(4) alt[ bits(3) alt[ bits(2) ] alt[ u8 ] ] ] ] ] ] ] loop[ u8*2 ] } guid u8`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused)
- HermesProxy: `AutoEquipItemSlot` — matches

### CMSG_MOVE_START_FORWARD (0x39e4)

- Modern: 14820 (0x39e4) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_START_BACKWARD (0x39e5)

- Modern: 14821 (0x39e5) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_STOP (0x39e6)

- Modern: 14822 (0x39e6) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_START_STRAFE_LEFT (0x39e7)

- Modern: 14823 (0x39e7) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_START_STRAFE_RIGHT (0x39e8)

- Modern: 14824 (0x39e8) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_STOP_STRAFE (0x39e9)

- Modern: 14825 (0x39e9) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_JUMP (0x39ea)

- Modern: 14826 (0x39ea) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_DOUBLE_JUMP (0x39eb)

- Modern: 14827 (0x39eb) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_START_TURN_LEFT (0x39ec)

- Modern: 14828 (0x39ec) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_START_TURN_RIGHT (0x39ed)

- Modern: 14829 (0x39ed) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_STOP_TURN (0x39ee)

- Modern: 14830 (0x39ee) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_START_PITCH_UP (0x39ef)

- Modern: 14831 (0x39ef) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_START_PITCH_DOWN (0x39f0)

- Modern: 14832 (0x39f0) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_STOP_PITCH (0x39f1)

- Modern: 14833 (0x39f1) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_SET_RUN_MODE (0x39f2)

- Modern: 14834 (0x39f2) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_SET_WALK_MODE (0x39f3)

- Modern: 14835 (0x39f3) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### — (0x39f4)

- Modern: 14836 (0x39f4) · 2.4.3: —
- Layout (after the opcode): `{ f32*4 u32*2 }`
- Size: 24 bytes (packed GUIDs not counted)

### — (0x39f5)

- Modern: 14837 (0x39f5) · 2.4.3: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x39f6)

- Modern: 14838 (0x39f6) · 2.4.3: —
- Layout (after the opcode): `{ f32*4 u32*2 }`
- Size: 24 bytes (packed GUIDs not counted)

### — (0x39f7)

- Modern: 14839 (0x39f7) · 2.4.3: —
- Layout (after the opcode): `{ f32*4 u32*2 }`
- Size: 24 bytes (packed GUIDs not counted)

### CMSG_MOVE_TELEPORT_ACK (0x39f8)

- Modern: 14840 (0x39f8) · 2.4.3: —
- Layout (after the opcode): `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `MoveTeleportAck` — matches

### CMSG_MOVE_FALL_LAND (0x39f9)

- Modern: 14841 (0x39f9) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_START_SWIM (0x39fa)

- Modern: 14842 (0x39fa) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_STOP_SWIM (0x39fb)

- Modern: 14843 (0x39fb) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### — (0x39fc)

- Modern: 14844 (0x39fc) · 2.4.3: —
- Layout (after the opcode): `f32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x39fd)

- Modern: 14845 (0x39fd) · 2.4.3: —
- Layout (after the opcode): `f32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x39fe)

- Modern: 14846 (0x39fe) · 2.4.3: —
- Layout (after the opcode): `f32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x39ff)

- Modern: 14847 (0x39ff) · 2.4.3: —
- Layout (after the opcode): `f32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3a00)

- Modern: 14848 (0x3a00) · 2.4.3: —
- Layout (after the opcode): `f32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3a01)

- Modern: 14849 (0x3a01) · 2.4.3: —
- Layout (after the opcode): `f32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3a02)

- Modern: 14850 (0x3a02) · 2.4.3: —
- Layout (after the opcode): `f32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3a03)

- Modern: 14851 (0x3a03) · 2.4.3: —
- Layout (after the opcode): `f32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_MOVE_SET_TURN_RATE_CHEAT (0x3a04)

- Modern: 14852 (0x3a04) · 2.4.3: —
- Layout (after the opcode): `f32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3a05)

- Modern: 14853 (0x3a05) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### — (0x3a06)

- Modern: 14854 (0x3a06) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### CMSG_MOVE_SET_FACING (0x3a07)

- Modern: 14855 (0x3a07) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_SET_FACING_HEARTBEAT (0x3a08)

- Modern: 14856 (0x3a08) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_SET_PITCH (0x3a09)

- Modern: 14857 (0x3a09) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_FORCE_RUN_SPEED_CHANGE_ACK (0x3a0a)

- Modern: 14858 (0x3a0a) · 2.4.3: 227 (0xe3)
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `MovementSpeedAck` — matches

### CMSG_MOVE_FORCE_RUN_BACK_SPEED_CHANGE_ACK (0x3a0b)

- Modern: 14859 (0x3a0b) · 2.4.3: 229 (0xe5)
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `MovementSpeedAck` — matches

### CMSG_MOVE_FORCE_SWIM_SPEED_CHANGE_ACK (0x3a0c)

- Modern: 14860 (0x3a0c) · 2.4.3: 231 (0xe7)
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `MovementSpeedAck` — matches

### CMSG_MOVE_FORCE_ROOT_ACK (0x3a0d)

- Modern: 14861 (0x3a0d) · 2.4.3: 233 (0xe9)
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `MovementAckMessage` — matches

### CMSG_MOVE_FORCE_UNROOT_ACK (0x3a0e)

- Modern: 14862 (0x3a0e) · 2.4.3: 235 (0xeb)
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `MovementAckMessage` — matches

### CMSG_MOVE_HEARTBEAT (0x3a0f)

- Modern: 14863 (0x3a0f) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### — (0x3a10)

- Modern: 14864 (0x3a10) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### CMSG_MOVE_KNOCK_BACK_ACK (0x3a11)

- Modern: 14865 (0x3a11) · 2.4.3: 240 (0xf0)
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 } flush opt[ f32*2 ]`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused); byte 55: 1 · (7 unused)
- HermesProxy: `MovementAckMessage` — matches

### CMSG_MOVE_HOVER_ACK (0x3a12)

- Modern: 14866 (0x3a12) · 2.4.3: 246 (0xf6)
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `MovementAckMessage` — matches

### CMSG_MOVE_SET_VEHICLE_REC_ID_ACK (0x3a13)

- Modern: 14867 (0x3a13) · 2.4.3: —
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 } u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `MoveSetVehicleRecIDAck` — matches

### CMSG_MOVE_APPLY_MOVEMENT_FORCE_ACK (0x3a14)

- Modern: 14868 (0x3a14) · 2.4.3: —
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 } { guid f32*6 u32 f32 bits(2) opt[ u8 ] flush opt[ u32 ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused); byte 89: 2 · 1 · (5 unused)

### CMSG_MOVE_REMOVE_MOVEMENT_FORCE_ACK (0x3a15)

- Modern: 14869 (0x3a15) · 2.4.3: —
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 } guid`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### CMSG_MOVE_REMOVE_MOVEMENT_FORCES (0x3a16)

- Modern: 14870 (0x3a16) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_SPLINE_DONE (0x3a17)

- Modern: 14871 (0x3a17) · 2.4.3: 713 (0x2c9)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `MoveSplineDone` — matches

### CMSG_MOVE_FALL_RESET (0x3a18)

- Modern: 14872 (0x3a18) · 2.4.3: 714 (0x2ca)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_UPDATE_FALL_SPEED (0x3a19)

- Modern: 14873 (0x3a19) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### CMSG_MOVE_TIME_SKIPPED (0x3a1a)

- Modern: 14874 (0x3a1a) · 2.4.3: 718 (0x2ce)
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `MoveTimeSkipped` — matches

### CMSG_MOVE_FEATHER_FALL_ACK (0x3a1b)

- Modern: 14875 (0x3a1b) · 2.4.3: 719 (0x2cf)
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `MovementAckMessage` — matches

### CMSG_MOVE_WATER_WALK_ACK (0x3a1c)

- Modern: 14876 (0x3a1c) · 2.4.3: 720 (0x2d0)
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `MovementAckMessage` — matches

### CMSG_MOVE_ENABLE_DOUBLE_JUMP_ACK (0x3a1d)

- Modern: 14877 (0x3a1d) · 2.4.3: —
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### — (0x3a1e)

- Modern: 14878 (0x3a1e) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### — (0x3a1f)

- Modern: 14879 (0x3a1f) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### CMSG_MOVE_FORCE_WALK_SPEED_CHANGE_ACK (0x3a20)

- Modern: 14880 (0x3a20) · 2.4.3: 731 (0x2db)
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `MovementSpeedAck` — matches

### CMSG_MOVE_FORCE_SWIM_BACK_SPEED_CHANGE_ACK (0x3a21)

- Modern: 14881 (0x3a21) · 2.4.3: 733 (0x2dd)
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `MovementSpeedAck` — matches

### CMSG_MOVE_FORCE_TURN_RATE_CHANGE_ACK (0x3a22)

- Modern: 14882 (0x3a22) · 2.4.3: 735 (0x2df)
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `MovementSpeedAck` — matches

### CMSG_MOVE_ENABLE_SWIM_TO_FLY_TRANS_ACK (0x3a23)

- Modern: 14883 (0x3a23) · 2.4.3: —
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### CMSG_MOVE_SET_CAN_TURN_WHILE_FALLING_ACK (0x3a24)

- Modern: 14884 (0x3a24) · 2.4.3: —
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### CMSG_MOVE_SET_IGNORE_MOVEMENT_FORCES_ACK (0x3a25)

- Modern: 14885 (0x3a25) · 2.4.3: —
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### CMSG_MOVE_SET_CAN_FLY_ACK (0x3a26)

- Modern: 14886 (0x3a26) · 2.4.3: 837 (0x345)
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `MovementAckMessage` — matches

### CMSG_MOVE_SET_FLY (0x3a27)

- Modern: 14887 (0x3a27) · 2.4.3: 838 (0x346)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_START_ASCEND (0x3a28)

- Modern: 14888 (0x3a28) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_STOP_ASCEND (0x3a29)

- Modern: 14889 (0x3a29) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### — (0x3a2a)

- Modern: 14890 (0x3a2a) · 2.4.3: —
- Layout (after the opcode): `f32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3a2b)

- Modern: 14891 (0x3a2b) · 2.4.3: —
- Layout (after the opcode): `f32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_MOVE_FORCE_FLIGHT_SPEED_CHANGE_ACK (0x3a2c)

- Modern: 14892 (0x3a2c) · 2.4.3: 898 (0x382)
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `MovementSpeedAck` — matches

### CMSG_MOVE_FORCE_FLIGHT_BACK_SPEED_CHANGE_ACK (0x3a2d)

- Modern: 14893 (0x3a2d) · 2.4.3: 900 (0x384)
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `MovementSpeedAck` — matches

### CMSG_MOVE_CHANGE_TRANSPORT (0x3a2e)

- Modern: 14894 (0x3a2e) · 2.4.3: 909 (0x38d)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### CMSG_MOVE_START_DESCEND (0x3a2f)

- Modern: 14895 (0x3a2f) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `ClientPlayerMovement` — matches

### — (0x3a30)

- Modern: 14896 (0x3a30) · 2.4.3: —
- Layout (after the opcode): `f32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_MOVE_FORCE_PITCH_RATE_CHANGE_ACK (0x3a31)

- Modern: 14897 (0x3a31) · 2.4.3: —
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `MovementSpeedAck` — matches

### CMSG_MOVE_DISMISS_VEHICLE (0x3a32)

- Modern: 14898 (0x3a32) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `RequestVehicleSeatChange` — reads the first part

### CMSG_MOVE_CHANGE_VEHICLE_SEATS (0x3a33)

- Modern: 14899 (0x3a33) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } guid u8`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### CMSG_MOVE_GRAVITY_DISABLE_ACK (0x3a34)

- Modern: 14900 (0x3a34) · 2.4.3: —
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `MovementAckMessage` — matches

### CMSG_MOVE_GRAVITY_ENABLE_ACK (0x3a35)

- Modern: 14901 (0x3a35) · 2.4.3: —
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `MovementAckMessage` — matches

### CMSG_MOVE_INERTIA_DISABLE_ACK (0x3a36)

- Modern: 14902 (0x3a36) · 2.4.3: —
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### CMSG_MOVE_INERTIA_ENABLE_ACK (0x3a37)

- Modern: 14903 (0x3a37) · 2.4.3: —
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### CMSG_MOVE_COLLISION_DISABLE_ACK (0x3a38)

- Modern: 14904 (0x3a38) · 2.4.3: —
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### CMSG_MOVE_COLLISION_ENABLE_ACK (0x3a39)

- Modern: 14905 (0x3a39) · 2.4.3: —
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### CMSG_MOVE_SET_COLLISION_HEIGHT_ACK (0x3a3a)

- Modern: 14906 (0x3a3a) · 2.4.3: —
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 } f32 u32 u8`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `MoveSetCollisionHeightAck` — differs

### CMSG_SET_ACTIVE_MOVER (0x3a3b)

- Modern: 14907 (0x3a3b) · 2.4.3: 618 (0x26a)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `SetActiveMover` — matches

### CMSG_TIME_SYNC_RESPONSE (0x3a3c)

- Modern: 14908 (0x3a3c) · 2.4.3: 913 (0x391)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `TimeSyncResponse` — matches

### CMSG_TIME_SYNC_RESPONSE_FAILED (0x3a3d)

- Modern: 14909 (0x3a3d) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_TIME_SYNC_RESPONSE_DROPPED (0x3a3e)

- Modern: 14910 (0x3a3e) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_TIME_ADJUSTMENT_RESPONSE (0x3a3f)

- Modern: 14911 (0x3a3f) · 2.4.3: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_DISCARDED_TIME_SYNC_ACKS (0x3a40)

- Modern: 14912 (0x3a40) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_MOVE_SET_MOD_MOVEMENT_FORCE_MAGNITUDE_ACK (0x3a41)

- Modern: 14913 (0x3a41) · 2.4.3: —
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### CMSG_UPDATE_MISSILE_TRAJECTORY (0x3a42)

- Modern: 14914 (0x3a42) · 2.4.3: —
- Layout (after the opcode): `guid guid u16 u32 f32*8 flush opt[ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } ]`
- Bit fields (message of zeros, widths in order written): byte 42: 1 · (7 unused)

### CMSG_MOVE_SEAMLESS_TRANSFER_COMPLETE (0x3a43)

- Modern: 14915 (0x3a43) · 2.4.3: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### — (0x3a44)

- Modern: 14916 (0x3a44) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_MOVE_INIT_ACTIVE_MOVER_COMPLETE (0x3a45)

- Modern: 14917 (0x3a45) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `InitActiveMoverComplete` — matches

### — (0x3a46)

- Modern: 14918 (0x3a46) · 2.4.3: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3a47)

- Modern: 14919 (0x3a47) · 2.4.3: —
- Layout (after the opcode): `flush guid`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x3a48)

- Modern: 14920 (0x3a48) · 2.4.3: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_MOVE_APPLY_INERTIA_ACK (0x3a49)

- Modern: 14921 (0x3a49) · 2.4.3: —
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 } guid u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)

### CMSG_MOVE_REMOVE_INERTIA_ACK (0x3a4a)

- Modern: 14922 (0x3a4a) · 2.4.3: —
- Layout (after the opcode): `{ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] ] opt[ guid f32*3 u32 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32 } guid`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · (2 unused)
