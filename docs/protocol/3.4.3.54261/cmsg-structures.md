# Client packet layouts (CMSG), 3.4.3.54261

Notation: see [the README](README.md).

### CMSG_GUILD_PROMOTE_MEMBER (0x305d)

- Modern: 12381 (0x305d) · 3.3.5a: 139 (0x8b)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `GuildPromoteMember` — matches

### CMSG_GUILD_DEMOTE_MEMBER (0x305e)

- Modern: 12382 (0x305e) · 3.3.5a: 140 (0x8c)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `GuildDemoteMember` — matches

### — (0x305f)

- Modern: 12383 (0x305f) · 3.3.5a: —
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GUILD_DECLINE_INVITATION (0x3060)

- Modern: 12384 (0x3060) · 3.3.5a: 133 (0x85)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_GUILD_AUTO_DECLINE_INVITATION (0x3061)

- Modern: 12385 (0x3061) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_GUILD_LEAVE (0x3062)

- Modern: 12386 (0x3062) · 3.3.5a: 141 (0x8d)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_GUILD_OFFICER_REMOVE_MEMBER (0x3063)

- Modern: 12387 (0x3063) · 3.3.5a: 142 (0x8e)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `GuildOfficerRemoveMember` — matches

### CMSG_GUILD_ADD_RANK (0x3064)

- Modern: 12388 (0x3064) · 3.3.5a: 562 (0x232)
- Layout (after the opcode): `bits(7) flush u32 bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)
- HermesProxy: `GuildAddRank` — matches

### CMSG_GUILD_DELETE_RANK (0x3065)

- Modern: 12389 (0x3065) · 3.3.5a: 563 (0x233)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `GuildDeleteRank` — matches

### — (0x3066)

- Modern: 12390 (0x3066) · 3.3.5a: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)

### CMSG_GUILD_SET_RANK_PERMISSIONS (0x3067)

- Modern: 12391 (0x3067) · 3.3.5a: 561 (0x231)
- Layout (after the opcode): `u32*4 loop[ u32*2 ] u32 bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 68: 7 · (1 unused)
- HermesProxy: `GuildSetRankPermissions` — matches

### CMSG_GUILD_DELETE (0x3068)

- Modern: 12392 (0x3068) · 3.3.5a: 143 (0x8f)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### — (0x306b)

- Modern: 12395 (0x306b) · 3.3.5a: —
- Layout (after the opcode): `guid u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_GUILD_GET_RANKS (0x306d)

- Modern: 12397 (0x306d) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_GUILD_SET_ACHIEVEMENT_TRACKING (0x306f)

- Modern: 12399 (0x306f) · 3.3.5a: —
- Layout (after the opcode): `u32 loop[ u32 ]`

### CMSG_GUILD_SET_MEMBER_NOTE (0x3072)

- Modern: 12402 (0x3072) · 3.3.5a: —
- Layout (after the opcode): `guid u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 1 · (7 unused)
- HermesProxy: `GuildSetMemberNote` — matches

### CMSG_GUILD_GET_ROSTER (0x3073)

- Modern: 12403 (0x3073) · 3.3.5a: 137 (0x89)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_GUILD_UPDATE_MOTD_TEXT (0x3074)

- Modern: 12404 (0x3074) · 3.3.5a: 145 (0x91)
- Layout (after the opcode): `u8 bits(3) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 11 · (5 unused)
- HermesProxy: `GuildUpdateMotdText` — matches

### CMSG_GUILD_UPDATE_INFO_TEXT (0x3075)

- Modern: 12405 (0x3075) · 3.3.5a: 764 (0x2fc)
- Layout (after the opcode): `u8 bits(3) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 11 · (5 unused)
- HermesProxy: `GuildUpdateInfoText` — matches

### — (0x307e)

- Modern: 12414 (0x307e) · 3.3.5a: —
- Layout (after the opcode): `bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)

### CMSG_GUILD_BANK_LOG_QUERY (0x3082)

- Modern: 12418 (0x3082) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `GuildBankLogQuery` — matches

### CMSG_GUILD_BANK_REMAINING_WITHDRAW_MONEY_QUERY (0x3083)

- Modern: 12419 (0x3083) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_GUILD_PERMISSIONS_QUERY (0x3084)

- Modern: 12420 (0x3084) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_GUILD_EVENT_LOG_QUERY (0x3085)

- Modern: 12421 (0x3085) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_GUILD_BANK_SET_TAB_TEXT (0x3086)

- Modern: 12422 (0x3086) · 3.3.5a: 1035 (0x40b)
- Layout (after the opcode): `u32 u8 bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: (1 unused) · 13 · (2 unused)
- HermesProxy: `GuildBankSetTabText` — differs

### CMSG_GUILD_BANK_TEXT_QUERY (0x3087)

- Modern: 12423 (0x3087) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `GuildBankTextQuery` — matches

### — (0x3088)

- Modern: 12424 (0x3088) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x308c)

- Modern: 12428 (0x308c) · 3.3.5a: —
- Layout (after the opcode): `u64 guid flush`
- Bit fields (message of zeros, widths in order written): byte 10: 1 · (7 unused)

### — (0x3126)

- Modern: 12582 (0x3126) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3127)

- Modern: 12583 (0x3127) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_TOY_CLEAR_FANFARE (0x3128)

- Modern: 12584 (0x3128) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `ToyClearFanfare` — matches

### CMSG_INITIATE_TRADE (0x3156)

- Modern: 12630 (0x3156) · 3.3.5a: 278 (0x116)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InitiateTrade` — matches

### CMSG_BEGIN_TRADE (0x3157)

- Modern: 12631 (0x3157) · 3.3.5a: 279 (0x117)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_BUSY_TRADE (0x3158)

- Modern: 12632 (0x3158) · 3.3.5a: 280 (0x118)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_IGNORE_TRADE (0x3159)

- Modern: 12633 (0x3159) · 3.3.5a: 281 (0x119)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_ACCEPT_TRADE (0x315a)

- Modern: 12634 (0x315a) · 3.3.5a: 282 (0x11a)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `AcceptTrade` — matches

### CMSG_UNACCEPT_TRADE (0x315b)

- Modern: 12635 (0x315b) · 3.3.5a: 283 (0x11b)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_CANCEL_TRADE (0x315c)

- Modern: 12636 (0x315c) · 3.3.5a: 284 (0x11c)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_SET_TRADE_ITEM (0x315d)

- Modern: 12637 (0x315d) · 3.3.5a: 285 (0x11d)
- Layout (after the opcode): `u8*3`
- Size: 3 bytes (packed GUIDs not counted)
- HermesProxy: `SetTradeItem` — matches

### CMSG_CLEAR_TRADE_ITEM (0x315e)

- Modern: 12638 (0x315e) · 3.3.5a: 286 (0x11e)
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `ClearTradeItem` — matches

### CMSG_SET_TRADE_GOLD (0x315f)

- Modern: 12639 (0x315f) · 3.3.5a: 287 (0x11f)
- Layout (after the opcode): `u64`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `SetTradeGold` — matches

### CMSG_STABLE_PET (0x3168)

- Modern: 12648 (0x3168) · 3.3.5a: 624 (0x270)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `StablePet` — matches

### CMSG_UNSTABLE_PET (0x3169)

- Modern: 12649 (0x3169) · 3.3.5a: 625 (0x271)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `UnstablePet` — matches

### CMSG_STABLE_SWAP_PET (0x316a)

- Modern: 12650 (0x316a) · 3.3.5a: 629 (0x275)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `StableSwapPet` — matches

### CMSG_BUY_STABLE_SLOT (0x316b)

- Modern: 12651 (0x316b) · 3.3.5a: 626 (0x272)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `BuyStableSlot` — matches

### CMSG_SET_CURRENCY_FLAGS (0x316c)

- Modern: 12652 (0x316c) · 3.3.5a: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_BATTLEFIELD_LEAVE (0x3175)

- Modern: 12661 (0x3175) · 3.3.5a: 737 (0x2e1)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `BattlefieldLeave` — matches

### CMSG_QUERY_QUEST_COMPLETION_NPCS (0x3177)

- Modern: 12663 (0x3177) · 3.3.5a: 1161 (0x489)
- Layout (after the opcode): `u32 loop[ u32 ]`

### — (0x3178)

- Modern: 12664 (0x3178) · 3.3.5a: —
- Layout (after the opcode): `guid u32 loop[ guid ]`

### CMSG_REQUEST_CEMETERY_LIST (0x3179)

- Modern: 12665 (0x3179) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x317a)

- Modern: 12666 (0x317a) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_REQUEST_HONOR_STATS (0x317e)

- Modern: 12670 (0x317e) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `Inspect` — matches

### CMSG_PVP_LOG_DATA (0x317f)

- Modern: 12671 (0x317f) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `PVPLogDataRequest` — matches

### CMSG_BATTLEFIELD_LIST (0x3181)

- Modern: 12673 (0x3181) · 3.3.5a: 572 (0x23c)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `BattlefieldListRequest` — matches

### CMSG_CANCEL_QUEUED_SPELL (0x3182)

- Modern: 12674 (0x3182) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_OBJECT_UPDATE_FAILED (0x3183)

- Modern: 12675 (0x3183) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ObjectUpdateFailed` — matches

### — (0x3184)

- Modern: 12676 (0x3184) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_VIOLENCE_LEVEL (0x3187)

- Modern: 12679 (0x3187) · 3.3.5a: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### CMSG_USED_FOLLOW (0x3189)

- Modern: 12681 (0x3189) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_STAND_STATE_CHANGE (0x318c)

- Modern: 12684 (0x318c) · 3.3.5a: 257 (0x101)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `StandStateChange` — matches

### — (0x318d)

- Modern: 12685 (0x318d) · 3.3.5a: —
- Layout (after the opcode): `guid u32 guid f32*3`
- Size: 16 bytes (packed GUIDs not counted)

### CMSG_SAVE_CUF_PROFILES (0x318e)

- Modern: 12686 (0x318e) · 3.3.5a: —
- Layout (after the opcode): `u32 loop[ { bits(7) opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush u16*2 u8*5 u16*3 bytes } ]`
- HermesProxy: `SaveCUFProfiles` — matches

### CMSG_REQUEST_PVP_REWARDS (0x3196)

- Modern: 12694 (0x3196) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3197)

- Modern: 12695 (0x3197) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31a2)

- Modern: 12706 (0x31a2) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31a3)

- Modern: 12707 (0x31a3) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31a4)

- Modern: 12708 (0x31a4) · 3.3.5a: —
- Layout (after the opcode): `guid u32*2 loop[ guid ] loop[ guid ]`

### — (0x31a5)

- Modern: 12709 (0x31a5) · 3.3.5a: —
- Layout (after the opcode): `guid guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_QUERY_COUNTDOWN_TIMER (0x31aa)

- Modern: 12714 (0x31aa) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CANCEL_AURA (0x31af)

- Modern: 12719 (0x31af) · 3.3.5a: 310 (0x136)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `CancelAura` — matches

### — (0x31b1)

- Modern: 12721 (0x31b1) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31b2)

- Modern: 12722 (0x31b2) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31b9)

- Modern: 12729 (0x31b9) · 3.3.5a: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)

### — (0x31ba)

- Modern: 12730 (0x31ba) · 3.3.5a: —
- Layout (after the opcode): `u32*2 guid flush`
- Bit fields (message of zeros, widths in order written): byte 10: 1 · (7 unused)

### CMSG_AREA_TRIGGER (0x31d6)

- Modern: 12758 (0x31d6) · 3.3.5a: 180 (0xb4)
- Layout (after the opcode): `u32 opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · 1 · (6 unused)
- HermesProxy: `AreaTriggerPkt` — matches

### — (0x31df)

- Modern: 12767 (0x31df) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x31e0)

- Modern: 12768 (0x31e0) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_REQUEST_FORCED_REACTIONS (0x3205)

- Modern: 12805 (0x3205) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3207)

- Modern: 12807 (0x3207) · 3.3.5a: —
- Layout (after the opcode): `u64 u32`
- Size: 12 bytes (packed GUIDs not counted)

### CMSG_CONFIRM_RESPEC_WIPE (0x320d)

- Modern: 12813 (0x320d) · 3.3.5a: —
- Layout (after the opcode): `guid u8`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `ConfirmRespecWipe` — matches

### — (0x320e)

- Modern: 12814 (0x320e) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_LOOT_UNIT (0x320f)

- Modern: 12815 (0x320f) · 3.3.5a: 349 (0x15d)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `LootUnit` — matches

### CMSG_LOOT_MONEY (0x3210)

- Modern: 12816 (0x3210) · 3.3.5a: 350 (0x15e)
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `LootMoney` — matches; see #369

### CMSG_AUTOSTORE_LOOT_ITEM / CMSG_LOOT_ITEM (0x3211)

- Modern: 12817 (0x3211) · 3.3.5a: 264 (0x108)
- Layout (after the opcode): `u32 loop[ guid u8 ] flush`
- HermesProxy: `LootItemPkt` — matches

### CMSG_LOOT_MASTER_GIVE (0x3212)

- Modern: 12818 (0x3212) · 3.3.5a: 675 (0x2a3)
- Layout (after the opcode): `u32 guid loop[ guid u8 ]`
- HermesProxy: `LootMasterGive` — matches

### CMSG_LOOT_RELEASE (0x3213)

- Modern: 12819 (0x3213) · 3.3.5a: 351 (0x15f)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `LootRelease` — matches

### CMSG_LOOT_ROLL (0x3214)

- Modern: 12820 (0x3214) · 3.3.5a: 672 (0x2a0)
- Layout (after the opcode): `guid u8*2`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8
- HermesProxy: `LootRoll` — matches

### — (0x321f)

- Modern: 12831 (0x321f) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3220)

- Modern: 12832 (0x3220) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3221)

- Modern: 12833 (0x3221) · 3.3.5a: —
- Layout (after the opcode): `bits(6) flush u32 bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 6 · (2 unused)

### CMSG_SET_DIFFICULTY_ID (0x3222)

- Modern: 12834 (0x3222) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3223)

- Modern: 12835 (0x3223) · 3.3.5a: —
- Layout (after the opcode): `*2`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_MAIL_DELETE (0x3225)

- Modern: 12837 (0x3225) · 3.3.5a: 585 (0x249)
- Layout (after the opcode): `u64 u32`
- Size: 12 bytes (packed GUIDs not counted)
- HermesProxy: `MailDelete` — matches

### CMSG_REQUEST_VEHICLE_EXIT (0x3237)

- Modern: 12855 (0x3237) · 3.3.5a: 1142 (0x476)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `RequestVehicleSeatChange` — matches

### CMSG_REQUEST_VEHICLE_PREV_SEAT (0x3238)

- Modern: 12856 (0x3238) · 3.3.5a: 1143 (0x477)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `RequestVehicleSeatChange` — matches

### CMSG_REQUEST_VEHICLE_NEXT_SEAT (0x3239)

- Modern: 12857 (0x3239) · 3.3.5a: 1144 (0x478)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `RequestVehicleSeatChange` — matches

### CMSG_REQUEST_VEHICLE_SWITCH_SEAT (0x323a)

- Modern: 12858 (0x323a) · 3.3.5a: 1145 (0x479)
- Layout (after the opcode): `guid u8`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `RequestVehicleSwitchSeat` — matches

### CMSG_RIDE_VEHICLE_INTERACT (0x323b)

- Modern: 12859 (0x323b) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `RideVehicleInteract` — matches

### CMSG_EJECT_PASSENGER (0x323c)

- Modern: 12860 (0x323c) · 3.3.5a: 1193 (0x4a9)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EjectPassenger` — matches

### — (0x3241)

- Modern: 12865 (0x3241) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3247)

- Modern: 12871 (0x3247) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_ATTACK_SWING (0x3255)

- Modern: 12885 (0x3255) · 3.3.5a: 321 (0x141)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `AttackSwing` — matches

### CMSG_ATTACK_STOP (0x3256)

- Modern: 12886 (0x3256) · 3.3.5a: 322 (0x142)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `AttackStop` — matches

### CMSG_CANCEL_CHANNELLING (0x326a)

- Modern: 12906 (0x326a) · 3.3.5a: 315 (0x13b)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `CancelChannelling` — matches

### CMSG_CANCEL_GROWTH_AURA (0x326f)

- Modern: 12911 (0x326f) · 3.3.5a: 667 (0x29b)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_QUERY_CREATURE (0x3270)

- Modern: 12912 (0x3270) · 3.3.5a: 96 (0x60)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `QueryCreature` — matches

### CMSG_QUERY_GAME_OBJECT (0x3271)

- Modern: 12913 (0x3271) · 3.3.5a: 94 (0x5e)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `QueryGameObject` — matches

### CMSG_QUERY_NPC_TEXT (0x3272)

- Modern: 12914 (0x3272) · 3.3.5a: 383 (0x17f)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `QueryNPCText` — matches

### CMSG_QUERY_QUEST_INFO (0x3273)

- Modern: 12915 (0x3273) · 3.3.5a: 92 (0x5c)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `QueryQuestInfo` — matches

### CMSG_QUERY_PAGE_TEXT (0x3274)

- Modern: 12916 (0x3274) · 3.3.5a: 90 (0x5a)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `QueryPageText` — matches

### CMSG_QUERY_PET_NAME (0x3275)

- Modern: 12917 (0x3275) · 3.3.5a: 82 (0x52)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `QueryPetName` — matches

### — (0x3276)

- Modern: 12918 (0x3276) · 3.3.5a: —
- Layout (after the opcode): `guid guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_QUERY_PETITION (0x3277)

- Modern: 12919 (0x3277) · 3.3.5a: 454 (0x1c6)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `QueryPetition` — matches

### CMSG_REQUEST_PLAYED_TIME (0x327a)

- Modern: 12922 (0x327a) · 3.3.5a: 460 (0x1cc)
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `RequestPlayedTime` — matches

### CMSG_SET_TITLE (0x327e)

- Modern: 12926 (0x327e) · 3.3.5a: 884 (0x374)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `SetTitle` — matches

### CMSG_CANCEL_MOUNT_AURA (0x327f)

- Modern: 12927 (0x327f) · 3.3.5a: 885 (0x375)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_MOUNT_SPECIAL_ANIM (0x3280)

- Modern: 12928 (0x3280) · 3.3.5a: 369 (0x171)
- Layout (after the opcode): `u32*2 loop[ u32 ]`
- HermesProxy: `MountSpecial` — matches

### — (0x3292)

- Modern: 12946 (0x3292) · 3.3.5a: —
- Layout (after the opcode): `u32 loop[ u32*3 ]`

### CMSG_DESTROY_ITEM (0x3293)

- Modern: 12947 (0x3293) · 3.3.5a: 273 (0x111)
- Layout (after the opcode): `u32 u8*2`
- Size: 6 bytes (packed GUIDs not counted)
- HermesProxy: `DestroyItem` — matches

### — (0x3297)

- Modern: 12951 (0x3297) · 3.3.5a: —
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_USE_ITEM (0x3298)

- Modern: 12952 (0x3298) · 3.3.5a: 171 (0xab)
- Layout (after the opcode): `u8*2 guid { guid loop[ u32 ] u32*2 f32*2 guid u32*3 loop[ u32*2 ] bits(5) opt[ u8 ] bits(2) opt[ u8 ] flush { { opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] bits(4) } opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] bits(7) flush guid guid opt[ guid f32*3 ] opt[ guid f32*3 ] opt[ f32 ] opt[ u32 ] bytes } opt[ u64 ] loop[ u32*3 flush opt[ u8 ] ] loop[ u32*3 flush opt[ u8 ] ] opt[ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } ] loop[ bits(2) flush u32*2 ] }`
- Bit fields (message of zeros, widths in order written): byte 0: 8 · 8; byte 44: 5 · 1 · 2 · 1 · (7 unused) · 28; byte 49: 1 · 1 · 1 · 1 · 7 · (1 unused)
- HermesProxy: `UseItem` — differs (#365)

### CMSG_ADD_TOY (0x3299)

- Modern: 12953 (0x3299) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `AddToy` — matches

### CMSG_USE_TOY (0x329a)

- Modern: 12954 (0x329a) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32*2 f32*2 guid u32*3 loop[ u32*2 ] bits(5) opt[ u8 ] bits(2) opt[ u8 ] flush { { opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] bits(4) } opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] bits(7) flush guid guid opt[ guid f32*3 ] opt[ guid f32*3 ] opt[ f32 ] opt[ u32 ] bytes } opt[ u64 ] loop[ u32*3 flush opt[ u8 ] ] loop[ u32*3 flush opt[ u8 ] ] opt[ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } ] loop[ bits(2) flush u32*2 ] }`
- Bit fields (message of zeros, widths in order written): byte 40: 5 · 1 · 2 · 1 · (7 unused) · 28; byte 45: 1 · 1 · 1 · 1 · 7 · (1 unused)
- HermesProxy: `UseToy` — differs (#365)

### CMSG_PET_CAST_SPELL (0x329b)

- Modern: 12955 (0x329b) · 3.3.5a: 496 (0x1f0)
- Layout (after the opcode): `guid { guid loop[ u32 ] u32*2 f32*2 guid u32*3 loop[ u32*2 ] bits(5) opt[ u8 ] bits(2) opt[ u8 ] flush { { opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] bits(4) } opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] bits(7) flush guid guid opt[ guid f32*3 ] opt[ guid f32*3 ] opt[ f32 ] opt[ u32 ] bytes } opt[ u64 ] loop[ u32*3 flush opt[ u8 ] ] loop[ u32*3 flush opt[ u8 ] ] opt[ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } ] loop[ bits(2) flush u32*2 ] }`
- Bit fields (message of zeros, widths in order written): byte 42: 5 · 1 · 2 · 1 · (7 unused) · 28; byte 47: 1 · 1 · 1 · 1 · 7 · (1 unused)
- HermesProxy: `PetCastSpell` — differs (#365)

### CMSG_CAST_SPELL (0x329c)

- Modern: 12956 (0x329c) · 3.3.5a: 302 (0x12e)
- Layout (after the opcode): `{ guid loop[ u32 ] u32*2 f32*2 guid u32*3 loop[ u32*2 ] bits(5) opt[ u8 ] bits(2) opt[ u8 ] flush { { opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] bits(4) } opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] bits(7) flush guid guid opt[ guid f32*3 ] opt[ guid f32*3 ] opt[ f32 ] opt[ u32 ] bytes } opt[ u64 ] loop[ u32*3 flush opt[ u8 ] ] loop[ u32*3 flush opt[ u8 ] ] opt[ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } ] loop[ bits(2) flush u32*2 ] }`
- Bit fields (message of zeros, widths in order written): byte 40: 5 · 1 · 2 · 1 · (7 unused) · 28; byte 45: 1 · 1 · 1 · 1 · 7 · (1 unused)
- HermesProxy: `CastSpell` — differs (#365)

### — (0x329d)

- Modern: 12957 (0x329d) · 3.3.5a: —
- Layout (after the opcode): `u32*2 guid`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x329e)

- Modern: 12958 (0x329e) · 3.3.5a: —
- Layout (after the opcode): `u32*2 guid`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_CANCEL_CAST (0x329f)

- Modern: 12959 (0x329f) · 3.3.5a: 303 (0x12f)
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `CancelCast` — matches

### — (0x32a2)

- Modern: 12962 (0x32a2) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_REQUEST_LFG_LIST_BLACKLIST (0x32a4)

- Modern: 12964 (0x32a4) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_SAVE_GUILD_EMBLEM (0x32a8)

- Modern: 12968 (0x32a8) · 3.3.5a: —
- Layout (after the opcode): `guid u32*5`
- Size: 20 bytes (packed GUIDs not counted)
- HermesProxy: `SaveGuildEmblem` — matches

### CMSG_TABARD_VENDOR_ACTIVATE (0x32a9)

- Modern: 12969 (0x32a9) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_TOGGLE_PVP (0x32ab)

- Modern: 12971 (0x32ab) · 3.3.5a: 595 (0x253)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_SET_PVP (0x32ac)

- Modern: 12972 (0x32ac) · 3.3.5a: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `SetPvP` — matches

### CMSG_BATTLEMASTER_HELLO (0x32b1)

- Modern: 12977 (0x32b1) · 3.3.5a: 727 (0x2d7)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_REQUEST_CONQUEST_FORMULA_CONSTANTS (0x32b4)

- Modern: 12980 (0x32b4) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### — (0x32b5)

- Modern: 12981 (0x32b5) · 3.3.5a: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### CMSG_ITEM_TEXT_QUERY (0x32c5)

- Modern: 12997 (0x32c5) · 3.3.5a: 579 (0x243)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ItemTextQuery` — matches

### CMSG_OPEN_ITEM (0x32c6)

- Modern: 12998 (0x32c6) · 3.3.5a: 172 (0xac)
- Layout (after the opcode): `u8*2`
- Size: 2 bytes (packed GUIDs not counted)
- HermesProxy: `OpenItem` — matches

### CMSG_READ_ITEM (0x32c7)

- Modern: 12999 (0x32c7) · 3.3.5a: 173 (0xad)
- Layout (after the opcode): `u8*2`
- Size: 2 bytes (packed GUIDs not counted)
- HermesProxy: `ReadItem` — matches

### — (0x32c8)

- Modern: 13000 (0x32c8) · 3.3.5a: —
- Layout (after the opcode): `u32*2 flush`
- Bit fields (message of zeros, widths in order written): byte 8: 1 · (7 unused)

### — (0x32c9)

- Modern: 13001 (0x32c9) · 3.3.5a: —
- Layout (after the opcode): `u32*2 flush`
- Bit fields (message of zeros, widths in order written): byte 8: 1 · (7 unused)

### — (0x32ca)

- Modern: 13002 (0x32ca) · 3.3.5a: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x32d9)

- Modern: 13017 (0x32d9) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x32dc)

- Modern: 13020 (0x32dc) · 3.3.5a: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x32dd)

- Modern: 13021 (0x32dd) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x32de)

- Modern: 13022 (0x32de) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x32df)

- Modern: 13023 (0x32df) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x32e0)

- Modern: 13024 (0x32e0) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x32f0)

- Modern: 13040 (0x32f0) · 3.3.5a: —
- Layout (after the opcode): `{ u32 f32 u32 u8 bits(2) opt[ u8 ] alt[ u8 ] bits(3) opt[ u8 ] alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u8 ] opt[ u8 ] }`
- Bit fields (message of zeros, widths in order written): byte 12: 10 · 11 · 8 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (1 unused)

### — (0x32f1)

- Modern: 13041 (0x32f1) · 3.3.5a: —
- Layout (after the opcode): `guid u32*2 u64 flush { u32 f32 u32 u8 bits(2) opt[ u8 ] alt[ u8 ] bits(3) opt[ u8 ] alt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush bytes bytes bytes opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u8 ] opt[ u8 ] }`
- Bit fields (message of zeros, widths in order written): byte 18: 1 · (7 unused); byte 31: 10 · 11 · 8 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (1 unused)

### — (0x32f2)

- Modern: 13042 (0x32f2) · 3.3.5a: —
- Layout (after the opcode): `u32*2 u8*2`
- Size: 10 bytes (packed GUIDs not counted)

### — (0x32f3)

- Modern: 13043 (0x32f3) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x32f4)

- Modern: 13044 (0x32f4) · 3.3.5a: —
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)

### — (0x32fb)

- Modern: 13051 (0x32fb) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_OFFER_PETITION (0x32fd)

- Modern: 13053 (0x32fd) · 3.3.5a: 451 (0x1c3)
- Layout (after the opcode): `u32 guid guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `OfferPetition` — matches

### CMSG_REMOVE_GLYPH (0x3300)

- Modern: 13056 (0x3300) · 3.3.5a: 1162 (0x48a)
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `RemoveGlyph` — matches

### — (0x3302)

- Modern: 13058 (0x3302) · 3.3.5a: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x3303)

- Modern: 13059 (0x3303) · 3.3.5a: —
- Layout (after the opcode): `{ u32*3 opt[ u32 ] opt[ u32*3 ] opt[ u32 ] loop[ u32*4 ] u8 flush bytes } u32*2`
- Bit fields (message of zeros, widths in order written): byte 12: 9 · (7 unused)

### — (0x330e)

- Modern: 13070 (0x330e) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_SEND_TEXT_EMOTE (0x3488)

- Modern: 13448 (0x3488) · 3.3.5a: 260 (0x104)
- Layout (after the opcode): `guid u32*2 { u32*2 loop[ u32 ] }`
- HermesProxy: `CTextEmote` — matches

### CMSG_SET_SHEATHED (0x3489)

- Modern: 13449 (0x3489) · 3.3.5a: 480 (0x1e0)
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)
- HermesProxy: `SetSheathed` — matches

### CMSG_PET_SET_ACTION (0x348a)

- Modern: 13450 (0x348a) · 3.3.5a: 372 (0x174)
- Layout (after the opcode): `guid u32*2 flush opt[ u32*2 ]`
- Bit fields (message of zeros, widths in order written): byte 10: 1 · (7 unused)
- HermesProxy: `PetSetAction` — matches; see #369

### CMSG_PET_ACTION (0x348b)

- Modern: 13451 (0x348b) · 3.3.5a: 373 (0x175)
- Layout (after the opcode): `guid u32 guid f32*3`
- Size: 16 bytes (packed GUIDs not counted)
- HermesProxy: `PetAction` — matches

### CMSG_PET_STOP_ATTACK (0x348c)

- Modern: 13452 (0x348c) · 3.3.5a: 746 (0x2ea)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `PetStopAttack` — matches

### CMSG_PET_ABANDON (0x348d)

- Modern: 13453 (0x348d) · 3.3.5a: 374 (0x176)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `PetAbandon` — matches

### CMSG_PET_CANCEL_AURA (0x348e)

- Modern: 13454 (0x348e) · 3.3.5a: 619 (0x26b)
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `PetCancelAura` — matches

### — (0x348f)

- Modern: 13455 (0x348f) · 3.3.5a: —
- Layout (after the opcode): `guid u32 flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)

### CMSG_REQUEST_PET_INFO (0x3490)

- Modern: 13456 (0x3490) · 3.3.5a: 633 (0x279)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_REQUEST_STABLED_PETS (0x3491)

- Modern: 13457 (0x3491) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `RequestStabledPets` — matches

### CMSG_TALK_TO_GOSSIP (0x3492)

- Modern: 13458 (0x3492) · 3.3.5a: 379 (0x17b)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_CLOSE_INTERACTION (0x3493)

- Modern: 13459 (0x3493) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `CloseInteraction` — matches

### CMSG_GOSSIP_SELECT_OPTION (0x3494)

- Modern: 13460 (0x3494) · 3.3.5a: 380 (0x17c)
- Layout (after the opcode): `guid u32*2 u8 flush bytes`
- HermesProxy: `GossipSelectOption` — matches

### CMSG_SPELL_CLICK (0x3495)

- Modern: 13461 (0x3495) · 3.3.5a: 1016 (0x3f8)
- Layout (after the opcode): `guid flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)
- HermesProxy: `InteractWithNPC` — matches; see #369

### CMSG_QUEST_GIVER_HELLO (0x3496)

- Modern: 13462 (0x3496) · 3.3.5a: 388 (0x184)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `QuestGiverHello` — matches

### CMSG_QUEST_GIVER_QUERY_QUEST (0x3497)

- Modern: 13463 (0x3497) · 3.3.5a: 390 (0x186)
- Layout (after the opcode): `guid u32 flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)
- HermesProxy: `QuestGiverQueryQuest` — matches

### CMSG_QUEST_GIVER_ACCEPT_QUEST (0x3498)

- Modern: 13464 (0x3498) · 3.3.5a: 393 (0x189)
- Layout (after the opcode): `guid u32 flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)
- HermesProxy: `QuestGiverAcceptQuest` — matches

### CMSG_QUEST_GIVER_COMPLETE_QUEST (0x3499)

- Modern: 13465 (0x3499) · 3.3.5a: 394 (0x18a)
- Layout (after the opcode): `guid u32 flush`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)
- HermesProxy: `QuestGiverCompleteQuest` — matches

### CMSG_QUEST_GIVER_CHOOSE_REWARD (0x349a)

- Modern: 13466 (0x349a) · 3.3.5a: 398 (0x18e)
- Layout (after the opcode): `guid u32 { bits(2) flush { u32*3 flush { bits(6) flush loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } u32 }`
- Bit fields (message of zeros, widths in order written): byte 6: 2 · (6 unused); byte 19: 1 · (7 unused) · 6 · (2 unused)
- HermesProxy: `QuestGiverChooseReward` — matches

### CMSG_QUEST_GIVER_REQUEST_REWARD (0x349b)

- Modern: 13467 (0x349b) · 3.3.5a: 396 (0x18c)
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `QuestGiverRequestReward` — matches

### CMSG_QUEST_GIVER_STATUS_QUERY (0x349c)

- Modern: 13468 (0x349c) · 3.3.5a: 386 (0x182)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `QuestGiverStatusQuery` — matches

### CMSG_QUEST_GIVER_STATUS_MULTIPLE_QUERY (0x349d)

- Modern: 13469 (0x349d) · 3.3.5a: 1047 (0x417)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_QUEST_CONFIRM_ACCEPT (0x349e)

- Modern: 13470 (0x349e) · 3.3.5a: 411 (0x19b)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `QuestConfirmAcceptResponse` — matches

### CMSG_PUSH_QUEST_TO_PARTY (0x349f)

- Modern: 13471 (0x349f) · 3.3.5a: 413 (0x19d)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `PushQuestToParty` — matches

### CMSG_QUEST_PUSH_RESULT (0x34a0)

- Modern: 13472 (0x34a0) · 3.3.5a: —
- Layout (after the opcode): `guid u32 u8`
- Size: 5 bytes (packed GUIDs not counted)
- HermesProxy: `QuestPushResultResponse` — matches

### CMSG_LIST_INVENTORY (0x34a1)

- Modern: 13473 (0x34a1) · 3.3.5a: 414 (0x19e)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_SELL_ITEM (0x34a2)

- Modern: 13474 (0x34a2) · 3.3.5a: 416 (0x1a0)
- Layout (after the opcode): `guid guid u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `SellItem` — matches

### CMSG_BUY_ITEM (0x34a3)

- Modern: 13475 (0x34a3) · 3.3.5a: 418 (0x1a2)
- Layout (after the opcode): `guid guid u32*4 { u32*3 flush { bits(6) flush loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 32: 1 · (7 unused) · 6 · (2 unused)
- HermesProxy: `BuyItem` — matches

### CMSG_BUY_BACK_ITEM (0x34a4)

- Modern: 13476 (0x34a4) · 3.3.5a: 656 (0x290)
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `BuyBackItem` — matches

### CMSG_TAXI_NODE_STATUS_QUERY (0x34a8)

- Modern: 13480 (0x34a8) · 3.3.5a: 426 (0x1aa)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_ENABLE_TAXI_NODE (0x34a9)

- Modern: 13481 (0x34a9) · 3.3.5a: 1171 (0x493)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_TAXI_QUERY_AVAILABLE_NODES (0x34aa)

- Modern: 13482 (0x34aa) · 3.3.5a: 428 (0x1ac)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_ACTIVATE_TAXI (0x34ab)

- Modern: 13483 (0x34ab) · 3.3.5a: 429 (0x1ad)
- Layout (after the opcode): `guid u32*3`
- Size: 12 bytes (packed GUIDs not counted)
- HermesProxy: `ActivateTaxi` — matches

### — (0x34ac)

- Modern: 13484 (0x34ac) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_TRAINER_LIST (0x34ad)

- Modern: 13485 (0x34ad) · 3.3.5a: 432 (0x1b0)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_TRAINER_BUY_SPELL (0x34ae)

- Modern: 13486 (0x34ae) · 3.3.5a: 434 (0x1b2)
- Layout (after the opcode): `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `TrainerBuySpell` — matches

### CMSG_SPIRIT_HEALER_ACTIVATE (0x34af)

- Modern: 13487 (0x34af) · 3.3.5a: 540 (0x21c)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_AREA_SPIRIT_HEALER_QUERY (0x34b0)

- Modern: 13488 (0x34b0) · 3.3.5a: 738 (0x2e2)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_AREA_SPIRIT_HEALER_QUEUE (0x34b1)

- Modern: 13489 (0x34b1) · 3.3.5a: 739 (0x2e3)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_BINDER_ACTIVATE (0x34b2)

- Modern: 13490 (0x34b2) · 3.3.5a: 437 (0x1b5)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_BANKER_ACTIVATE (0x34b3)

- Modern: 13491 (0x34b3) · 3.3.5a: 439 (0x1b7)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_BUY_BANK_SLOT (0x34b4)

- Modern: 13492 (0x34b4) · 3.3.5a: 441 (0x1b9)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `BuyBankSlot` — matches

### CMSG_GUILD_BANK_ACTIVATE (0x34b5)

- Modern: 13493 (0x34b5) · 3.3.5a: 998 (0x3e6)
- Layout (after the opcode): `guid flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)
- HermesProxy: `GuildBankAtivate` — matches

### CMSG_AUTO_GUILD_BANK_ITEM (0x34b6)

- Modern: 13494 (0x34b6) · 3.3.5a: —
- Layout (after the opcode): `guid u8*3 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 8 · 1 · (7 unused)
- HermesProxy: `AutoGuildBankItem` — matches

### CMSG_STORE_GUILD_BANK_ITEM (0x34b7)

- Modern: 13495 (0x34b7) · 3.3.5a: —
- Layout (after the opcode): `guid u8*3 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 8 · 1 · (7 unused)
- HermesProxy: `AutoGuildBankItem` — matches

### CMSG_SWAP_ITEM_WITH_GUILD_BANK_ITEM (0x34b8)

- Modern: 13496 (0x34b8) · 3.3.5a: —
- Layout (after the opcode): `guid u8*3 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 8 · 1 · (7 unused)
- HermesProxy: `AutoGuildBankItem` — matches

### CMSG_SWAP_GUILD_BANK_ITEM_WITH_GUILD_BANK_ITEM (0x34b9)

- Modern: 13497 (0x34b9) · 3.3.5a: —
- Layout (after the opcode): `guid loop[ u8*2 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 16

### CMSG_MOVE_GUILD_BANK_ITEM (0x34ba)

- Modern: 13498 (0x34ba) · 3.3.5a: —
- Layout (after the opcode): `guid u8*4`
- Size: 4 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 8 · 8
- HermesProxy: `MoveGuildBankItem` — matches

### CMSG_MERGE_ITEM_WITH_GUILD_BANK_ITEM (0x34bb)

- Modern: 13499 (0x34bb) · 3.3.5a: —
- Layout (after the opcode): `guid u8*3 u32 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 8; byte 9: 1 · (7 unused)
- HermesProxy: `SplitItemToGuildBank` — matches

### CMSG_SPLIT_ITEM_TO_GUILD_BANK (0x34bc)

- Modern: 13500 (0x34bc) · 3.3.5a: —
- Layout (after the opcode): `guid u8*3 u32 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 8; byte 9: 1 · (7 unused)
- HermesProxy: `SplitItemToGuildBank` — matches

### CMSG_MERGE_GUILD_BANK_ITEM_WITH_ITEM (0x34bd)

- Modern: 13501 (0x34bd) · 3.3.5a: —
- Layout (after the opcode): `guid u8*3 u32 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 8; byte 9: 1 · (7 unused)
- HermesProxy: `SplitItemToGuildBank` — matches

### CMSG_SPLIT_GUILD_BANK_ITEM_TO_INVENTORY (0x34be)

- Modern: 13502 (0x34be) · 3.3.5a: —
- Layout (after the opcode): `guid u8*3 u32 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 8; byte 9: 1 · (7 unused)
- HermesProxy: `SplitItemToGuildBank` — matches

### CMSG_AUTO_STORE_GUILD_BANK_ITEM (0x34bf)

- Modern: 13503 (0x34bf) · 3.3.5a: —
- Layout (after the opcode): `guid u8*2`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8
- HermesProxy: `AutoStoreGuildBankItem` — matches

### CMSG_MERGE_GUILD_BANK_ITEM_WITH_GUILD_BANK_ITEM (0x34c0)

- Modern: 13504 (0x34c0) · 3.3.5a: —
- Layout (after the opcode): `guid u8*4 u32`
- Size: 8 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 8 · 8
- HermesProxy: `SplitGuildBankItem` — matches

### CMSG_SPLIT_GUILD_BANK_ITEM (0x34c1)

- Modern: 13505 (0x34c1) · 3.3.5a: —
- Layout (after the opcode): `guid u8*4 u32`
- Size: 8 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 8 · 8
- HermesProxy: `SplitGuildBankItem` — matches

### CMSG_GUILD_BANK_QUERY_TAB (0x34c2)

- Modern: 13506 (0x34c2) · 3.3.5a: 999 (0x3e7)
- Layout (after the opcode): `guid u8 flush`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 1 · (7 unused)
- HermesProxy: `GuildBankQueryTab` — matches

### CMSG_GUILD_BANK_BUY_TAB (0x34c3)

- Modern: 13507 (0x34c3) · 3.3.5a: 1002 (0x3ea)
- Layout (after the opcode): `guid u8`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `GuildBankBuyTab` — matches

### CMSG_GUILD_BANK_UPDATE_TAB (0x34c4)

- Modern: 13508 (0x34c4) · 3.3.5a: 1003 (0x3eb)
- Layout (after the opcode): `guid u8 bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`
- HermesProxy: `GuildBankUpdateTab` — matches

### CMSG_GUILD_BANK_DEPOSIT_MONEY (0x34c5)

- Modern: 13509 (0x34c5) · 3.3.5a: 1004 (0x3ec)
- Layout (after the opcode): `guid u64`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `GuildBankDepositMoney` — matches

### CMSG_GUILD_BANK_WITHDRAW_MONEY (0x34c6)

- Modern: 13510 (0x34c6) · 3.3.5a: 1005 (0x3ed)
- Layout (after the opcode): `guid u64`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `GuildBankWithdrawMoney` — matches

### — (0x34c7)

- Modern: 13511 (0x34c7) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_PETITION_BUY (0x34c8)

- Modern: 13512 (0x34c8) · 3.3.5a: 445 (0x1bd)
- Layout (after the opcode): `bits(7) flush guid u32 bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)
- HermesProxy: `PetitionBuy` — matches

### CMSG_PETITION_SHOW_SIGNATURES (0x34c9)

- Modern: 13513 (0x34c9) · 3.3.5a: 446 (0x1be)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `PetitionShowSignatures` — matches

### CMSG_AUCTION_HELLO_REQUEST (0x34ca)

- Modern: 13514 (0x34ca) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `InteractWithNPC` — matches

### CMSG_AUCTION_SELL_ITEM (0x34cb)

- Modern: 13515 (0x34cb) · 3.3.5a: 598 (0x256)
- Layout (after the opcode): `guid u64*2 u32 bits(6) flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush bytes bytes } ] loop[ guid u32 ]`
- Bit fields (message of zeros, widths in order written): byte 22: 1 · 6 · (1 unused)
- HermesProxy: `AuctionSellItem` — matches

### CMSG_AUCTION_REMOVE_ITEM (0x34cc)

- Modern: 13516 (0x34cc) · 3.3.5a: 599 (0x257)
- Layout (after the opcode): `guid u32 flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush bytes bytes } ]`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)
- HermesProxy: `AuctionRemoveItem` — matches

### CMSG_AUCTION_LIST_ITEMS (0x34cd)

- Modern: 13517 (0x34cd) · 3.3.5a: 600 (0x258)
- Layout (after the opcode): `{ guid u32 u8*2 u32 u8 u32 u8 loop[ u8 ] u8 flush bytes bits(3) opt[ u8 ] opt[ u8 ] flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush bytes bytes } ] loop[ u32 bits(5) flush loop[ u64 u32 ] ] u32 bytes }`
- Bit fields (message of zeros, widths in order written): byte 6: 8 · 8; byte 17: 8 · 1 · (15 unused) · 3 · 1 · 1 · (3 unused)
- HermesProxy: `AuctionListItems` — differs

### — (0x34ce)

- Modern: 13518 (0x34ce) · 3.3.5a: —
- Layout (after the opcode): `guid u32*4 flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush bytes bytes } ]`
- Bit fields (message of zeros, widths in order written): byte 18: 1 · (7 unused)

### CMSG_AUCTION_LIST_OWNED_ITEMS (0x34cf)

- Modern: 13519 (0x34cf) · 3.3.5a: 601 (0x259)
- Layout (after the opcode): `guid u32 flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush bytes bytes } ]`
- Bit fields (message of zeros, widths in order written): byte 6: 1 · (7 unused)
- HermesProxy: `AuctionListOwnerItems` — matches; see #369

### CMSG_AUCTION_LIST_BIDDED_ITEMS (0x34d0)

- Modern: 13520 (0x34d0) · 3.3.5a: 612 (0x264)
- Layout (after the opcode): `guid u32 bits(7) opt[ u8 ] flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush bytes bytes } ] loop[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 6: 7 · 1
- HermesProxy: `AuctionListBidderItems` — differs (#367)

### CMSG_AUCTION_PLACE_BID (0x34d1)

- Modern: 13521 (0x34d1) · 3.3.5a: 602 (0x25a)
- Layout (after the opcode): `guid u32 u64 flush opt[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush bytes bytes } ]`
- Bit fields (message of zeros, widths in order written): byte 14: 1 · (7 unused)
- HermesProxy: `AuctionPlaceBid` — matches

### CMSG_AUCTION_LIST_PENDING_SALES (0x34d2)

- Modern: 13522 (0x34d2) · 3.3.5a: 1167 (0x48f)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_QUERY_TIME (0x34d5)

- Modern: 13525 (0x34d5) · 3.3.5a: 462 (0x1ce)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_LOGOUT_REQUEST (0x34d6)

- Modern: 13526 (0x34d6) · 3.3.5a: 75 (0x4b)
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `LogoutRequest` — matches

### CMSG_LOGOUT_CANCEL (0x34d8)

- Modern: 13528 (0x34d8) · 3.3.5a: 78 (0x4e)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### — (0x34d9)

- Modern: 13529 (0x34d9) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_RECLAIM_CORPSE (0x34db)

- Modern: 13531 (0x34db) · 3.3.5a: 466 (0x1d2)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ReclaimCorpse` — matches

### — (0x34dd)

- Modern: 13533 (0x34dd) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_SET_FACTION_AT_WAR (0x34de)

- Modern: 13534 (0x34de) · 3.3.5a: 293 (0x125)
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `SetFactionAtWar` — matches

### CMSG_SET_FACTION_NOT_AT_WAR (0x34df)

- Modern: 13535 (0x34df) · 3.3.5a: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `SetFactionNotAtWar` — matches

### CMSG_SET_FACTION_INACTIVE (0x34e0)

- Modern: 13536 (0x34e0) · 3.3.5a: 791 (0x317)
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)
- HermesProxy: `SetFactionInactive` — matches

### CMSG_SET_WATCHED_FACTION (0x34e1)

- Modern: 13537 (0x34e1) · 3.3.5a: 792 (0x318)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `SetWatchedFaction` — matches

### CMSG_DUEL_RESPONSE (0x34e2)

- Modern: 13538 (0x34e2) · 3.3.5a: —
- Layout (after the opcode): `guid opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · 1 · (6 unused)
- HermesProxy: `DuelResponse` — matches

### CMSG_UNLEARN_SKILL (0x34e5)

- Modern: 13541 (0x34e5) · 3.3.5a: 514 (0x202)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `UnlearnSkill` — matches

### CMSG_CANCEL_AUTO_REPEAT_SPELL (0x34e7)

- Modern: 13543 (0x34e7) · 3.3.5a: 621 (0x26d)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_FAR_SIGHT (0x34e8)

- Modern: 13544 (0x34e8) · 3.3.5a: 634 (0x27a)
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `FarSight` — matches

### CMSG_SOCKET_GEMS (0x34eb)

- Modern: 13547 (0x34eb) · 3.3.5a: 839 (0x347)
- Layout (after the opcode): `guid loop[ guid ]`
- HermesProxy: `SocketGems` — matches

### CMSG_REPAIR_ITEM (0x34ec)

- Modern: 13548 (0x34ec) · 3.3.5a: 680 (0x2a8)
- Layout (after the opcode): `guid guid flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)
- HermesProxy: `RepairItem` — matches

### CMSG_GAME_OBJ_USE (0x34ee)

- Modern: 13550 (0x34ee) · 3.3.5a: 177 (0xb1)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `GameObjUse` — matches

### CMSG_GAME_OBJ_REPORT_USE (0x34ef)

- Modern: 13551 (0x34ef) · 3.3.5a: 1153 (0x481)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `GameObjReportUse` — matches

### CMSG_CANCEL_TEMP_ENCHANTMENT (0x34f2)

- Modern: 13554 (0x34f2) · 3.3.5a: 889 (0x379)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `CancelTempEnchantment` — matches

### — (0x34f3)

- Modern: 13555 (0x34f3) · 3.3.5a: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x34f4)

- Modern: 13556 (0x34f4) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_ALTER_APPEARANCE (0x34f5)

- Modern: 13557 (0x34f5) · 3.3.5a: 1062 (0x426)
- Layout (after the opcode): `u32 u8 u32*2 loop[ u32*2 ]`
- HermesProxy: `AlterAppearance` — matches

### CMSG_OPT_OUT_OF_LOOT (0x34f6)

- Modern: 13558 (0x34f6) · 3.3.5a: 1033 (0x409)
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `OptOutOfLoot` — matches

### CMSG_TOTEM_DESTROYED (0x34f8)

- Modern: 13560 (0x34f8) · 3.3.5a: 1044 (0x414)
- Layout (after the opcode): `u8 guid`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `TotemDestroyed` — matches

### CMSG_DISMISS_CRITTER (0x34f9)

- Modern: 13561 (0x34f9) · 3.3.5a: 1165 (0x48d)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `DismissCritter` — matches

### — (0x3500)

- Modern: 13568 (0x3500) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_HEARTH_AND_RESURRECT (0x3506)

- Modern: 13574 (0x3506) · 3.3.5a: 1180 (0x49c)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_SAVE_EQUIPMENT_SET (0x3509)

- Modern: 13577 (0x3509) · 3.3.5a: 1213 (0x4bd)
- Layout (after the opcode): `{ u32 u64 u32*2 loop[ guid u32 ] loop[ u32 ] u32*4 u8*2 opt[ u8 ] flush opt[ u32 ] bytes bytes }`
- Bit fields (message of zeros, widths in order written): byte 158: 1 · 8 · 9 · (6 unused)
- HermesProxy: `SaveEquipmentSet` — matches

### CMSG_DELETE_EQUIPMENT_SET (0x350a)

- Modern: 13578 (0x350a) · 3.3.5a: —
- Layout (after the opcode): `u64`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `DeleteEquipmentSet` — matches

### CMSG_INSTANCE_LOCK_RESPONSE (0x350b)

- Modern: 13579 (0x350b) · 3.3.5a: 319 (0x13f)
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `InstanceLockResponse` — matches

### — (0x3512)

- Modern: 13586 (0x3512) · 3.3.5a: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### CMSG_DECLINE_GUILD_INVITES (0x351d)

- Modern: 13597 (0x351d) · 3.3.5a: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `SetAutoDeclineGuildInvites` — differs

### CMSG_OVERRIDE_SCREEN_FLASH (0x351e)

- Modern: 13598 (0x351e) · 3.3.5a: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### CMSG_BATTLEMASTER_JOIN (0x3520)

- Modern: 13600 (0x3520) · 3.3.5a: 750 (0x2ee)
- Layout (after the opcode): `u64 u8 loop[ u32 ] guid u32*2 flush`
- Bit fields (message of zeros, widths in order written): byte 27: 1 · (7 unused)
- HermesProxy: `BattlemasterJoin` — matches

### CMSG_BATTLEMASTER_JOIN_ARENA (0x3521)

- Modern: 13601 (0x3521) · 3.3.5a: 856 (0x358)
- Layout (after the opcode): `guid u8*2`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8
- HermesProxy: `BattlemasterJoinArena` — matches

### CMSG_BATTLEMASTER_JOIN_SKIRMISH (0x3522)

- Modern: 13602 (0x3522) · 3.3.5a: —
- Layout (after the opcode): `guid u8*2 opt[ u8 ] flush`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 8 · 1 · 1 · (6 unused)
- HermesProxy: `BattlemasterJoinSkirmish` — matches

### CMSG_BATTLEFIELD_PORT (0x3525)

- Modern: 13605 (0x3525) · 3.3.5a: 725 (0x2d5)
- Layout (after the opcode): `guid u32*2 u64 flush flush`
- Bit fields (message of zeros, widths in order written): byte 18: 1 · (7 unused) · 1 · (7 unused)
- HermesProxy: `BattlefieldPort` — matches

### CMSG_REPOP_REQUEST (0x3526)

- Modern: 13606 (0x3526) · 3.3.5a: 346 (0x15a)
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `RepopRequest` — matches

### CMSG_SET_SELECTION (0x3528)

- Modern: 13608 (0x3528) · 3.3.5a: 317 (0x13d)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `SetSelection` — matches

### CMSG_INSPECT (0x3529)

- Modern: 13609 (0x3529) · 3.3.5a: 276 (0x114)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `Inspect` — matches

### CMSG_REQUEST_CROWD_CONTROL_SPELL (0x352a)

- Modern: 13610 (0x352a) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x352b)

- Modern: 13611 (0x352b) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_QUEST_LOG_REMOVE_QUEST (0x352e)

- Modern: 13614 (0x352e) · 3.3.5a: 404 (0x194)
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `QuestLogRemoveQuest` — matches

### CMSG_GET_ITEM_PURCHASE_DATA (0x352f)

- Modern: 13615 (0x352f) · 3.3.5a: 1203 (0x4b3)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3530)

- Modern: 13616 (0x3530) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_SELF_RES (0x3531)

- Modern: 13617 (0x3531) · 3.3.5a: 691 (0x2b3)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `SelfRes` — matches

### CMSG_SET_ACTION_BAR_TOGGLES (0x3532)

- Modern: 13618 (0x3532) · 3.3.5a: 703 (0x2bf)
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `SetActionBarToggles` — matches

### CMSG_SIGN_PETITION (0x3533)

- Modern: 13619 (0x3533) · 3.3.5a: 448 (0x1c0)
- Layout (after the opcode): `guid u8`
- Size: 1 bytes (packed GUIDs not counted)
- HermesProxy: `SignPetition` — matches

### CMSG_DECLINE_PETITION (0x3534)

- Modern: 13620 (0x3534) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `DeclinePetition` — matches

### CMSG_TURN_IN_PETITION (0x3535)

- Modern: 13621 (0x3535) · 3.3.5a: 452 (0x1c4)
- Layout (after the opcode): `guid u32*5`
- Size: 20 bytes (packed GUIDs not counted)
- HermesProxy: `TurnInPetition` — reads the first part

### CMSG_MAIL_GET_LIST (0x3536)

- Modern: 13622 (0x3536) · 3.3.5a: 570 (0x23a)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `MailGetList` — matches

### CMSG_MAIL_TAKE_MONEY (0x3537)

- Modern: 13623 (0x3537) · 3.3.5a: 581 (0x245)
- Layout (after the opcode): `guid u64*2`
- Size: 16 bytes (packed GUIDs not counted)
- HermesProxy: `MailTakeMoney` — matches

### CMSG_MAIL_TAKE_ITEM (0x3538)

- Modern: 13624 (0x3538) · 3.3.5a: 582 (0x246)
- Layout (after the opcode): `guid u64*2`
- Size: 16 bytes (packed GUIDs not counted)
- HermesProxy: `MailTakeItem` — matches

### CMSG_QUERY_NEXT_MAIL_TIME (0x3539)

- Modern: 13625 (0x3539) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_MAIL_MARK_AS_READ (0x353a)

- Modern: 13626 (0x353a) · 3.3.5a: 583 (0x247)
- Layout (after the opcode): `guid u64`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `MailMarkAsRead` — matches

### CMSG_MAIL_CREATE_TEXT_ITEM (0x353b)

- Modern: 13627 (0x353b) · 3.3.5a: 586 (0x24a)
- Layout (after the opcode): `guid u64`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `MailCreateTextItem` — matches

### CMSG_EMOTE (0x3541)

- Modern: 13633 (0x3541) · 3.3.5a: 258 (0x102)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_OPENING_CINEMATIC (0x3543)

- Modern: 13635 (0x3543) · 3.3.5a: 249 (0xf9)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ClientCinematicPkt` — matches

### CMSG_NEXT_CINEMATIC_CAMERA (0x3544)

- Modern: 13636 (0x3544) · 3.3.5a: 251 (0xfb)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ClientCinematicPkt` — matches

### CMSG_COMPLETE_CINEMATIC (0x3545)

- Modern: 13637 (0x3545) · 3.3.5a: 252 (0xfc)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ClientCinematicPkt` — matches

### — (0x3546)

- Modern: 13638 (0x3546) · 3.3.5a: —
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_QUEST_GIVER_CLOSE_QUEST (0x3549)

- Modern: 13641 (0x3549) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `QuestGiverCloseQuest` — matches

### CMSG_LEARN_TALENT (0x3552)

- Modern: 13650 (0x3552) · 3.3.5a: 593 (0x251)
- Layout (after the opcode): `u32 u16`
- Size: 6 bytes (packed GUIDs not counted)
- HermesProxy: `LearnTalent` — matches

### — (0x3553)

- Modern: 13651 (0x3553) · 3.3.5a: —
- Layout (after the opcode): `u32 loop[ u32 u8 ]`

### CMSG_PET_LEARN_TALENT (0x3554)

- Modern: 13652 (0x3554) · 3.3.5a: 1146 (0x47a)
- Layout (after the opcode): `guid u32 u16`
- Size: 6 bytes (packed GUIDs not counted)
- HermesProxy: `LearnPetTalent` — matches

### — (0x3555)

- Modern: 13653 (0x3555) · 3.3.5a: —
- Layout (after the opcode): `guid u32 loop[ u32 u8 ]`

### — (0x355b)

- Modern: 13659 (0x355b) · 3.3.5a: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_SET_ACTION_BUTTON (0x355d)

- Modern: 13661 (0x355d) · 3.3.5a: 296 (0x128)
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)
- HermesProxy: `SetActionButton` — differs

### CMSG_SET_AMMO (0x355e)

- Modern: 13662 (0x355e) · 3.3.5a: 616 (0x268)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `SetAmmo` — matches

### CMSG_PLAYER_SHOWING_HELM (0x3568)

- Modern: 13672 (0x3568) · 3.3.5a: 697 (0x2b9)
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `PlayerShowingHelmOrCloak` — matches

### CMSG_PLAYER_SHOWING_CLOAK (0x3569)

- Modern: 13673 (0x3569) · 3.3.5a: 698 (0x2ba)
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `PlayerShowingHelmOrCloak` — matches

### — (0x356b)

- Modern: 13675 (0x356b) · 3.3.5a: —
- Layout (after the opcode): `u32 loop[ guid ]`

### — (0x35d4)

- Modern: 13780 (0x35d4) · 3.3.5a: —
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)

### CMSG_ADDON_LIST (0x35d8)

- Modern: 13784 (0x35d8) · 3.3.5a: —
- Layout (after the opcode): `guid u32 u16 u8 u32 loop[ { u8 bits(2) opt[ u8 ] alt[ u8 ] bits(2) opt[ u8 ] opt[ u8 ] flush bytes bytes } ]`

### CMSG_SET_ROLE (0x35d9)

- Modern: 13785 (0x35d9) · 3.3.5a: —
- Layout (after the opcode): `flush guid u8 opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (6 unused) · 1
- HermesProxy: `SetRole` — differs

### — (0x35da)

- Modern: 13786 (0x35da) · 3.3.5a: —
- Layout (after the opcode): `flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### CMSG_REQUEST_BATTLEFIELD_STATUS (0x35dd)

- Modern: 13789 (0x35dd) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `RequestBattlefieldStatus` — matches

### — (0x35df)

- Modern: 13791 (0x35df) · 3.3.5a: —
- Layout (after the opcode): `guid u32 u16 u64 flush`
- Bit fields (message of zeros, widths in order written): byte 16: 1 · (7 unused)

### — (0x35e0)

- Modern: 13792 (0x35e0) · 3.3.5a: —
- Layout (after the opcode): `guid u32 u16 guid u32 u16 u64 flush`
- Bit fields (message of zeros, widths in order written): byte 24: 1 · (7 unused)

### — (0x35e1)

- Modern: 13793 (0x35e1) · 3.3.5a: —
- Layout (after the opcode): `guid u64 flush`
- Bit fields (message of zeros, widths in order written): byte 10: 1 · (7 unused)

### CMSG_REQUEST_RATED_PVP_INFO (0x35e4)

- Modern: 13796 (0x35e4) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `RequestRatedPvpInfo` — matches

### CMSG_DB_QUERY_BULK (0x35e5)

- Modern: 13797 (0x35e5) · 3.3.5a: —
- Layout (after the opcode): `u32 u8 bits(5) flush loop[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 4: 13 · (3 unused)
- HermesProxy: `DBQueryBulk` — matches

### CMSG_HOTFIX_REQUEST (0x35e6)

- Modern: 13798 (0x35e6) · 3.3.5a: —
- Layout (after the opcode): `u32*3 loop[ u32 ]`
- HermesProxy: `HotfixRequest` — matches

### CMSG_GENERATE_RANDOM_CHARACTER_NAME (0x35e8)

- Modern: 13800 (0x35e8) · 3.3.5a: —
- Layout (after the opcode): `u8*2`
- Size: 2 bytes (packed GUIDs not counted)
- Bit fields (message of zeros, widths in order written): byte 0: 8 · 8
- HermesProxy: `GenerateRandomCharacterNameRequest` — matches

### CMSG_ENUM_CHARACTERS (0x35e9)

- Modern: 13801 (0x35e9) · 3.3.5a: 55 (0x37)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_REORDER_CHARACTERS (0x35ea)

- Modern: 13802 (0x35ea) · 3.3.5a: —
- Layout (after the opcode): `u8 flush loop[ guid u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · (7 unused)
- HermesProxy: `ReorderCharacters` — matches

### CMSG_PLAYER_LOGIN (0x35eb)

- Modern: 13803 (0x35eb) · 3.3.5a: 61 (0x3d)
- Layout (after the opcode): `guid f32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `PlayerLogin` — matches

### — (0x35ed)

- Modern: 13805 (0x35ed) · 3.3.5a: —
- Layout (after the opcode): `u32*2 bytes`

### — (0x35f0)

- Modern: 13808 (0x35f0) · 3.3.5a: —
- Layout (after the opcode): `loop[ opt[ u8 ] alt[ u8 ] ] opt[ u8 ] flush u64 loop[ bytes ]`

### — (0x35f1)

- Modern: 13809 (0x35f1) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x35f2)

- Modern: 13810 (0x35f2) · 3.3.5a: —
- Layout (after the opcode): `bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 6 · (2 unused)

### — (0x35f3)

- Modern: 13811 (0x35f3) · 3.3.5a: —
- Layout (after the opcode): `u32*2 u16 u8`
- Size: 11 bytes (packed GUIDs not counted)

### — (0x35f4)

- Modern: 13812 (0x35f4) · 3.3.5a: —
- Layout (after the opcode): `guid u32 loop[ u32*2 ]`

### — (0x35f5)

- Modern: 13813 (0x35f5) · 3.3.5a: —
- Layout (after the opcode): `u32*2 u16 u8 u64 u32`
- Size: 23 bytes (packed GUIDs not counted)

### — (0x35f6)

- Modern: 13814 (0x35f6) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_REQUEST_PARTY_JOIN_UPDATES (0x35f8)

- Modern: 13816 (0x35f8) · 3.3.5a: —
- Layout (after the opcode): `flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### CMSG_LOADING_SCREEN_NOTIFY (0x35f9)

- Modern: 13817 (0x35f9) · 3.3.5a: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)
- HermesProxy: `LoadingScreenNotify` — matches

### CMSG_WORLD_PORT_RESPONSE (0x35fa)

- Modern: 13818 (0x35fa) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `WorldPortResponse` — matches

### CMSG_SEND_MAIL (0x35fb)

- Modern: 13819 (0x35fb) · 3.3.5a: 568 (0x238)
- Layout (after the opcode): `{ guid u32 u64*2 u8*3 bits(3) bits(5) flush bytes bytes bytes loop[ u8 guid ] }`
- Bit fields (message of zeros, widths in order written): byte 22: 9 · 9 · 11 · 5 · (6 unused)
- HermesProxy: `SendMail` — matches

### CMSG_ACCEPT_GUILD_INVITE (0x35fe)

- Modern: 13822 (0x35fe) · 3.3.5a: 132 (0x84)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_PARTY_INVITE (0x3604)

- Modern: 13828 (0x3604) · 3.3.5a: 110 (0x6e)
- Layout (after the opcode): `flush { u8*2 flush u32 guid bytes bytes } opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused) · 9 · 9 · (6 unused)
- HermesProxy: `PartyInviteClient` — differs (#368)

### CMSG_PARTY_INVITE_RESPONSE (0x3606)

- Modern: 13830 (0x3606) · 3.3.5a: —
- Layout (after the opcode): `opt[ u8 ] opt[ u8 ] flush opt[ u8 ] opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 1 · 1 · (5 unused)
- HermesProxy: `PartyInviteResponse` — matches

### CMSG_GUILD_INVITE_BY_NAME (0x3608)

- Modern: 13832 (0x3608) · 3.3.5a: 130 (0x82)
- Layout (after the opcode): `u8 flush bytes opt[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · 1 · (6 unused)
- HermesProxy: `GuildInviteByName` — matches

### CMSG_DF_PROPOSAL_RESPONSE (0x3609)

- Modern: 13833 (0x3609) · 3.3.5a: —
- Layout (after the opcode): `guid u32*2 u64 flush u64 u32 flush`
- Bit fields (message of zeros, widths in order written): byte 18: 1 · (7 unused); byte 31: 1 · (7 unused)
- HermesProxy: `DFProposalResponsePkt` — matches

### CMSG_DF_JOIN (0x360b)

- Modern: 13835 (0x360b) · 3.3.5a: —
- Layout (after the opcode): `opt[ u8 ] opt[ u8 ] flush u8 u32 opt[ u8 ] loop[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 1 · 1 · (5 unused) · 8
- HermesProxy: `DFJoinPkt` — matches

### — (0x360c)

- Modern: 13836 (0x360c) · 3.3.5a: —
- Layout (after the opcode): `guid u32*2 u64 flush`
- Bit fields (message of zeros, widths in order written): byte 18: 1 · (7 unused)

### CMSG_LFG_LIST_GET_STATUS (0x360d)

- Modern: 13837 (0x360d) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `LFGListGetStatusPkt` — matches

### — (0x360e)

- Modern: 13838 (0x360e) · 3.3.5a: —
- Layout (after the opcode): `{ bits(5) opt[ u8 ] flush loop[ loop[ opt[ opt[ u8 ] alt[ u8 ] ] opt[ u8 ] ] flush loop[ bytes ] ] u32*6 loop[ u32 ] loop[ guid ] }`
- Bit fields (message of zeros, widths in order written): byte 0: 5 · 1 · (2 unused)

### — (0x360f)

- Modern: 13839 (0x360f) · 3.3.5a: —
- Layout (after the opcode): `{ guid u32*2 u64 flush } u32 u8*2 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 18: 1 · (7 unused); byte 23: 8 · 8

### — (0x3610)

- Modern: 13840 (0x3610) · 3.3.5a: —
- Layout (after the opcode): `guid u32*2 u64 flush`
- Bit fields (message of zeros, widths in order written): byte 18: 1 · (7 unused)

### — (0x3611)

- Modern: 13841 (0x3611) · 3.3.5a: —
- Layout (after the opcode): `guid u32*2 u64 flush { guid u32*2 u64 flush }`
- Bit fields (message of zeros, widths in order written): byte 18: 1 · (7 unused); byte 37: 1 · (7 unused)

### — (0x3612)

- Modern: 13842 (0x3612) · 3.3.5a: —
- Layout (after the opcode): `{ guid u32*2 u64 flush } u32 loop[ guid u8 ] guid u32*2 u64 flush`

### — (0x3613)

- Modern: 13843 (0x3613) · 3.3.5a: —
- Layout (after the opcode): `guid u32*2 u64 flush flush`
- Bit fields (message of zeros, widths in order written): byte 18: 1 · (7 unused) · 1 · (7 unused)

### CMSG_DF_LEAVE (0x3614)

- Modern: 13844 (0x3614) · 3.3.5a: —
- Layout (after the opcode): `guid u32*2 u64 flush`
- Bit fields (message of zeros, widths in order written): byte 18: 1 · (7 unused)
- HermesProxy: `DFLeavePkt` — reads the first part

### CMSG_DF_GET_SYSTEM_INFO (0x3615)

- Modern: 13845 (0x3615) · 3.3.5a: —
- Layout (after the opcode): `opt[ u8 ] flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 1 · (6 unused)
- HermesProxy: `DFGetSystemInfoPkt` — differs

### CMSG_DF_GET_JOIN_STATUS (0x3616)

- Modern: 13846 (0x3616) · 3.3.5a: 662 (0x296)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `DFGetJoinStatusPkt` — matches

### CMSG_DF_SET_ROLES (0x3617)

- Modern: 13847 (0x3617) · 3.3.5a: —
- Layout (after the opcode): `flush u8 opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused) · 8
- HermesProxy: `DFSetRolesPkt` — matches; see #369

### — (0x3618)

- Modern: 13848 (0x3618) · 3.3.5a: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### CMSG_DF_TELEPORT (0x3619)

- Modern: 13849 (0x3619) · 3.3.5a: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `DFTeleportPkt` — matches

### CMSG_SET_EVERYONE_IS_ASSISTANT (0x361a)

- Modern: 13850 (0x361a) · 3.3.5a: —
- Layout (after the opcode): `opt[ u8 ] flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 1 · (6 unused)
- HermesProxy: `SetEveryoneIsAssistant` — matches

### — (0x361c)

- Modern: 13852 (0x361c) · 3.3.5a: —
- Layout (after the opcode): `opt[ u8 ] flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 1 · (6 unused)

### CMSG_BATTLE_PET_REQUEST_JOURNAL_LOCK (0x3624)

- Modern: 13860 (0x3624) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PET_REQUEST_JOURNAL (0x3625)

- Modern: 13861 (0x3625) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_BATTLE_PET_SUMMON (0x362a)

- Modern: 13866 (0x362a) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `BattlePetSummon` — matches

### — (0x362e)

- Modern: 13870 (0x362e) · 3.3.5a: —
- Layout (after the opcode): `guid u8`
- Size: 1 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PET_SET_FLAGS (0x3631)

- Modern: 13873 (0x3631) · 3.3.5a: —
- Layout (after the opcode): `guid u16 bits(2) flush`
- Bit fields (message of zeros, widths in order written): byte 4: 2 · (6 unused)
- HermesProxy: `BattlePetSetFlags` — matches

### CMSG_MOUNT_SET_FAVORITE (0x3633)

- Modern: 13875 (0x3633) · 3.3.5a: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)
- HermesProxy: `MountSetFavorite` — matches

### CMSG_COLLECTION_ITEM_SET_FAVORITE (0x3634)

- Modern: 13876 (0x3634) · 3.3.5a: —
- Layout (after the opcode): `u32*2 flush`
- Bit fields (message of zeros, widths in order written): byte 8: 1 · (7 unused)
- HermesProxy: `CollectionItemSetFavorite` — matches

### CMSG_DO_READY_CHECK (0x3635)

- Modern: 13877 (0x3635) · 3.3.5a: —
- Layout (after the opcode): `flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `DoReadyCheck` — matches

### CMSG_READY_CHECK_RESPONSE (0x3636)

- Modern: 13878 (0x3636) · 3.3.5a: —
- Layout (after the opcode): `opt[ u8 ] flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 1 · (6 unused)
- HermesProxy: `ReadyCheckResponseClient` — matches

### CMSG_CREATE_CHARACTER (0x3645)

- Modern: 13893 (0x3645) · 3.3.5a: 54 (0x36)
- Layout (after the opcode): `{ bits(6) opt[ u8 ] opt[ u8 ] opt[ u8 ] flush u8*3 u32 bytes opt[ u32 ] loop[ u32*2 ] }`
- Bit fields (message of zeros, widths in order written): byte 0: 6 · 1 · 1 · 1 · (7 unused) · 8 · 8 · 8
- HermesProxy: `CreateCharacter` — matches

### CMSG_SUPPORT_TICKET_SUBMIT_COMPLAINT (0x3647)

- Modern: 13895 (0x3647) · 3.3.5a: —
- Layout (after the opcode): `{ u32 f32*4 u32 guid u32*4 flush loop[ u64 u8 bits(4) flush bytes ] opt[ u32 ] u8 bits(2) opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ flush ] { u32 loop[ u64 guid opt[ u8 ] opt[ u8 ] opt[ u8 ] u8 bits(4) flush opt[ u64 ] opt[ guid ] opt[ u32 u16 u8 ] opt[ u32 ] bytes ] } bytes opt[ u64 u8 bits(5) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes ] opt[ u64*2 u8 flush bytes ] opt[ guid u8 flush bytes ] opt[ bits(7) flush guid bytes ] opt[ { { guid u32*2 u64 flush } u32 u8 guid guid guid guid guid u8 bits(2) opt[ u8 ] alt[ u8 ] bits(3) opt[ u8 ] alt[ u8 ] flush bytes bytes bytes } ] opt[ guid u32*2 u64 flush u8 flush bytes ] opt[ u64*2 guid u8 bits(4) flush bytes ] opt[ bits(7) flush guid bytes ] }`
- Bit fields (message of zeros, widths in order written): byte 42: 1 · (7 unused) · 10 · (14 unused)
- HermesProxy: `SupportTicketSubmitComplaint` — differs

### CMSG_SUPPORT_TICKET_SUBMIT_BUG (0x3648)

- Modern: 13896 (0x3648) · 3.3.5a: —
- Layout (after the opcode): `u32 f32*4 u32 u8 bits(2) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 24: 10 · (6 unused)

### CMSG_SUPPORT_TICKET_SUBMIT_SUGGESTION (0x3649)

- Modern: 13897 (0x3649) · 3.3.5a: —
- Layout (after the opcode): `u32 f32*4 u32 u8 bits(2) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 24: 10 · (6 unused)

### CMSG_PARTY_UNINVITE (0x364a)

- Modern: 13898 (0x364a) · 3.3.5a: —
- Layout (after the opcode): `u8 flush guid opt[ u8 ] bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 8 · (6 unused) · 1
- HermesProxy: `PartyUninvite` — differs

### CMSG_SET_LOOT_METHOD (0x364b)

- Modern: 13899 (0x364b) · 3.3.5a: 122 (0x7a)
- Layout (after the opcode): `flush u8 guid u32 opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused) · 8
- HermesProxy: `SetLootMethod` — differs (#368)

### CMSG_LEAVE_GROUP (0x364c)

- Modern: 13900 (0x364c) · 3.3.5a: —
- Layout (after the opcode): `flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `LeaveGroup` — differs (#368)

### CMSG_SET_PARTY_LEADER (0x364d)

- Modern: 13901 (0x364d) · 3.3.5a: 120 (0x78)
- Layout (after the opcode): `flush guid opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (6 unused) · 1
- HermesProxy: `SetPartyLeader` — differs (#368)

### CMSG_MINIMAP_PING (0x364e)

- Modern: 13902 (0x364e) · 3.3.5a: —
- Layout (after the opcode): `flush f32*2 opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `MinimapPingClient` — matches

### CMSG_GROUP_CHANGE_SUB_GROUP (0x364f)

- Modern: 13903 (0x364f) · 3.3.5a: 638 (0x27e)
- Layout (after the opcode): `guid u8 flush opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 2: 8 · 1 · (7 unused)
- HermesProxy: `ChangeSubGroup` — matches

### CMSG_GROUP_SWAP_SUB_GROUP (0x3650)

- Modern: 13904 (0x3650) · 3.3.5a: 640 (0x280)
- Layout (after the opcode): `flush guid guid opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (6 unused) · 1
- HermesProxy: `SwapSubGroups` — differs

### CMSG_CONVERT_RAID (0x3651)

- Modern: 13905 (0x3651) · 3.3.5a: 654 (0x28e)
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `ConvertRaid` — matches

### CMSG_SET_ASSISTANT_LEADER (0x3652)

- Modern: 13906 (0x3652) · 3.3.5a: 655 (0x28f)
- Layout (after the opcode): `opt[ u8 ] flush guid opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 1 · (6 unused)
- HermesProxy: `SetAssistantLeader` — matches

### CMSG_UPDATE_RAID_TARGET (0x3653)

- Modern: 13907 (0x3653) · 3.3.5a: —
- Layout (after the opcode): `flush guid u8 opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (6 unused) · 1
- HermesProxy: `UpdateRaidTarget` — differs (#368)

### — (0x3654)

- Modern: 13908 (0x3654) · 3.3.5a: —
- Layout (after the opcode): `opt[ u8 ] flush u8 guid opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 1 · (6 unused) · 8

### — (0x3655)

- Modern: 13909 (0x3655) · 3.3.5a: —
- Layout (after the opcode): `opt[ u8 ] flush guid opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 1 · (6 unused)

### CMSG_REQUEST_PARTY_MEMBER_STATS (0x3656)

- Modern: 13910 (0x3656) · 3.3.5a: 639 (0x27f)
- Layout (after the opcode): `flush guid opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (6 unused) · 1
- HermesProxy: `RequestPartyMemberStats` — differs (#368)

### CMSG_RANDOM_ROLL (0x3657)

- Modern: 13911 (0x3657) · 3.3.5a: —
- Layout (after the opcode): `flush u32*2 opt[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)
- HermesProxy: `RandomRollClient` — matches

### CMSG_MAIL_RETURN_TO_SENDER (0x3658)

- Modern: 13912 (0x3658) · 3.3.5a: 584 (0x248)
- Layout (after the opcode): `u64 guid`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `MailReturnToSender` — matches

### — (0x3659)

- Modern: 13913 (0x3659) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x365c)

- Modern: 13916 (0x365c) · 3.3.5a: —
- Layout (after the opcode): `u64 u32 guid flush`
- Bit fields (message of zeros, widths in order written): byte 14: 1 · (7 unused)

### CMSG_QUERY_CORPSE_LOCATION_FROM_CLIENT (0x3662)

- Modern: 13922 (0x3662) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `QueryCorpseLocationFromClient` — matches

### — (0x3663)

- Modern: 13923 (0x3663) · 3.3.5a: —
- Layout (after the opcode): `guid guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_CAN_DUEL (0x3664)

- Modern: 13924 (0x3664) · 3.3.5a: —
- Layout (after the opcode): `guid flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)
- HermesProxy: `CanDuel` — matches; see #369

### — (0x3666)

- Modern: 13926 (0x3666) · 3.3.5a: —
- Layout (after the opcode): `f32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_RESET_INSTANCES (0x366a)

- Modern: 13930 (0x366a) · 3.3.5a: 797 (0x31d)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_SUMMON_RESPONSE (0x366c)

- Modern: 13932 (0x366c) · 3.3.5a: 684 (0x2ac)
- Layout (after the opcode): `guid flush`
- Bit fields (message of zeros, widths in order written): byte 2: 1 · (7 unused)
- HermesProxy: `SummonResponse` — matches

### CMSG_COMPLAINT (0x366e)

- Modern: 13934 (0x366e) · 3.3.5a: 967 (0x3c7)
- Layout (after the opcode): `u8 guid u32*2 opt[ u64 ] opt[ u64*2 ] opt[ u32*2 u8 bits(4) flush bytes ]`

### — (0x3671)

- Modern: 13937 (0x3671) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3672)

- Modern: 13938 (0x3672) · 3.3.5a: —
- Layout (after the opcode): `u64`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x3673)

- Modern: 13939 (0x3673) · 3.3.5a: —
- Layout (after the opcode): `u64 u8*3`
- Size: 11 bytes (packed GUIDs not counted)

### — (0x3674)

- Modern: 13940 (0x3674) · 3.3.5a: —
- Layout (after the opcode): `u64*3 u8 opt[ u8 ] flush bytes`
- Bit fields (message of zeros, widths in order written): byte 24: 9 · 1 · 1 · (5 unused)

### — (0x3675)

- Modern: 13941 (0x3675) · 3.3.5a: —
- Layout (after the opcode): `guid u64*3`
- Size: 24 bytes (packed GUIDs not counted)

### — (0x3676)

- Modern: 13942 (0x3676) · 3.3.5a: —
- Layout (after the opcode): `u64*2 u8`
- Size: 17 bytes (packed GUIDs not counted)

### — (0x3677)

- Modern: 13943 (0x3677) · 3.3.5a: —
- Layout (after the opcode): `guid u64*3 u8`
- Size: 25 bytes (packed GUIDs not counted)

### — (0x3678)

- Modern: 13944 (0x3678) · 3.3.5a: —
- Layout (after the opcode): `guid u64*3 u8`
- Size: 25 bytes (packed GUIDs not counted)

### — (0x3679)

- Modern: 13945 (0x3679) · 3.3.5a: —
- Layout (after the opcode): `u64*3 u32`
- Size: 28 bytes (packed GUIDs not counted)

### — (0x367a)

- Modern: 13946 (0x367a) · 3.3.5a: —
- Layout (after the opcode): `u64*3 u32`
- Size: 28 bytes (packed GUIDs not counted)

### — (0x367b)

- Modern: 13947 (0x367b) · 3.3.5a: —
- Layout (after the opcode): `guid u64*2`
- Size: 16 bytes (packed GUIDs not counted)

### CMSG_CALENDAR_GET_NUM_PENDING (0x367c)

- Modern: 13948 (0x367c) · 3.3.5a: 1095 (0x447)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x367d)

- Modern: 13949 (0x367d) · 3.3.5a: —
- Layout (after the opcode): `u64*2 flush`
- Bit fields (message of zeros, widths in order written): byte 16: 1 · (7 unused)

### — (0x367f)

- Modern: 13951 (0x367f) · 3.3.5a: —
- Layout (after the opcode): `{ u64 u8 u32*4 u8*2 bits(3) flush loop[ guid u8*2 opt[ u8 ] opt[ u8 ] flush opt[ guid ] opt[ u64 ] opt[ u64 ] ] bytes bytes } u32`
- Bit fields (message of zeros, widths in order written): byte 25: 8 · 11 · (5 unused)

### — (0x3680)

- Modern: 13952 (0x3680) · 3.3.5a: —
- Layout (after the opcode): `u64*3 u8 u32*3 u8*2 bits(3) flush bytes bytes u32`
- Bit fields (message of zeros, widths in order written): byte 37: 8 · 11 · (5 unused)

### CMSG_KEEP_ALIVE (0x3681)

- Modern: 13953 (0x3681) · 3.3.5a: 1031 (0x407)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3682)

- Modern: 13954 (0x3682) · 3.3.5a: —
- Layout (after the opcode): `bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 6 · (2 unused)

### CMSG_WHO (0x3683)

- Modern: 13955 (0x3683) · 3.3.5a: 98 (0x62)
- Layout (after the opcode): `bits(4) opt[ u8 ] flush { u32*2 u64 u32 bits(6) { opt[ u8 ] alt[ u8 ] opt[ u8 ] } bits(7) { opt[ u8 ] alt[ u8 ] opt[ u8 ] } bits(3) opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush loop[ bits(7) flush bytes ] bytes bytes bytes bytes opt[ u32*3 ] } u32 u8 loop[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 4 · 1 · (3 unused); byte 21: (3 unused) · 3 · (6 unused) · 3 · (4 unused) · 3 · (6 unused) · 3 · 3 · 1 · 1 · 1 · 1 · (2 unused)
- HermesProxy: `WhoRequestPkt` — reads the first part

### CMSG_SET_DUNGEON_DIFFICULTY (0x3684)

- Modern: 13956 (0x3684) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `SetDungeonDifficulty` — matches

### CMSG_RESURRECT_RESPONSE (0x3685)

- Modern: 13957 (0x3685) · 3.3.5a: 348 (0x15c)
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `ResurrectResponse` — matches

### CMSG_PET_RENAME (0x3686)

- Modern: 13958 (0x3686) · 3.3.5a: 375 (0x177)
- Layout (after the opcode): `{ guid u32 u8 loop[ opt[ u8 ] alt[ u8 ] ] flush loop[ bytes ] bytes }`
- Bit fields (message of zeros, widths in order written): byte 6: 8 · 1 · (7 unused)
- HermesProxy: `PetRename` — matches

### CMSG_BUG (0x3687)

- Modern: 13959 (0x3687) · 3.3.5a: 458 (0x1ca)
- Layout (after the opcode): `u8 bits(4) opt[ u8 ] alt[ u8 ] bits(2) flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 12 · 10 · (1 unused)

### CMSG_SET_PLAYER_DECLINED_NAMES (0x3689)

- Modern: 13961 (0x3689) · 3.3.5a: 1049 (0x419)
- Layout (after the opcode): `guid loop[ opt[ u8 ] alt[ u8 ] ] flush loop[ bytes ]`
- HermesProxy: `SetPlayerDeclinedNames` — matches

### — (0x368a)

- Modern: 13962 (0x368a) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_QUERY_GUILD_INFO (0x368b)

- Modern: 13963 (0x368b) · 3.3.5a: 84 (0x54)
- Layout (after the opcode): `guid guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `QueryGuildInfo` — matches

### — (0x368c)

- Modern: 13964 (0x368c) · 3.3.5a: —
- Layout (after the opcode): `guid u8 u32 loop[ u32*2 ] bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 7: 6 · (2 unused)

### CMSG_GM_TICKET_GET_SYSTEM_STATUS (0x368e)

- Modern: 13966 (0x368e) · 3.3.5a: 538 (0x21a)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_GM_TICKET_GET_CASE_STATUS (0x368f)

- Modern: 13967 (0x368f) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_GM_TICKET_ACKNOWLEDGE_SURVEY (0x3690)

- Modern: 13968 (0x3690) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3692)

- Modern: 13970 (0x3692) · 3.3.5a: —
- Layout (after the opcode): `bits(6) flush guid u8*3 u32 bytes loop[ u32*2 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · 6 · (1 unused); byte 3: 8 · 8 · 8

### CMSG_SUBMIT_USER_FEEDBACK (0x3693)

- Modern: 13971 (0x3693) · 3.3.5a: —
- Layout (after the opcode): `u32 f32*4 u32 { opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] opt[ u8 ] alt[ u8 ] } opt[ u8 ] flush bytes`
- Bit fields (message of zeros, widths in order written): byte 24: (23 unused) · 1 · 1 · (7 unused)

### CMSG_REQUEST_ACCOUNT_DATA (0x3694)

- Modern: 13972 (0x3694) · 3.3.5a: 522 (0x20a)
- Layout (after the opcode): `guid bits(4) flush`
- Bit fields (message of zeros, widths in order written): byte 2: 4 · (4 unused)
- HermesProxy: `RequestAccountData` — matches

### CMSG_UPDATE_ACCOUNT_DATA (0x3695)

- Modern: 13973 (0x3695) · 3.3.5a: 523 (0x20b)
- Layout (after the opcode): `guid u64 u32 bits(4) flush u32 bytes`
- Bit fields (message of zeros, widths in order written): byte 14: 4 · (4 unused)
- HermesProxy: `UserClientUpdateAccountData` — matches

### CMSG_SERVER_TIME_OFFSET_REQUEST (0x369c)

- Modern: 13980 (0x369c) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_CHAR_DELETE (0x369d)

- Modern: 13981 (0x369d) · 3.3.5a: 56 (0x38)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `CharDelete` — matches

### — (0x36a1)

- Modern: 13985 (0x36a1) · 3.3.5a: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### CMSG_INSPECT_PVP (0x36a3)

- Modern: 13987 (0x36a3) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `Inspect` — matches

### CMSG_QUERY_ARENA_TEAM (0x36a4)

- Modern: 13988 (0x36a4) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x36aa)

- Modern: 13994 (0x36aa) · 3.3.5a: —
- Layout (after the opcode): `u8 bits(2) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 10 · (6 unused)

### CMSG_QUEST_POI_QUERY (0x36b2)

- Modern: 14002 (0x36b2) · 3.3.5a: 483 (0x1e3)
- Layout (after the opcode): `u32 loop[ u32 ]`
- HermesProxy: `QuestPOIQuery` — matches

### CMSG_ARENA_TEAM_ROSTER (0x36b8)

- Modern: 14008 (0x36b8) · 3.3.5a: 845 (0x34d)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `ArenaTeamRosterRequest` — matches

### CMSG_ARENA_TEAM_ACCEPT (0x36b9)

- Modern: 14009 (0x36b9) · 3.3.5a: 849 (0x351)
- Layout (after the opcode): `guid guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ArenaTeamAccept` — matches

### CMSG_ARENA_TEAM_DECLINE (0x36ba)

- Modern: 14010 (0x36ba) · 3.3.5a: 850 (0x352)
- Layout (after the opcode): `guid guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `ArenaTeamAccept` — matches

### CMSG_ARENA_TEAM_LEAVE (0x36bb)

- Modern: 14011 (0x36bb) · 3.3.5a: 851 (0x353)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `ArenaTeamLeave` — matches

### CMSG_ARENA_TEAM_REMOVE (0x36bc)

- Modern: 14012 (0x36bc) · 3.3.5a: 852 (0x354)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `ArenaTeamRemove` — matches

### CMSG_ARENA_TEAM_DISBAND (0x36bd)

- Modern: 14013 (0x36bd) · 3.3.5a: 853 (0x355)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `ArenaTeamLeave` — matches

### CMSG_ARENA_TEAM_LEADER (0x36be)

- Modern: 14014 (0x36be) · 3.3.5a: 854 (0x356)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `ArenaTeamRemove` — matches

### CMSG_GET_ACCOUNT_CHARACTER_LIST (0x36bf)

- Modern: 14015 (0x36bf) · 3.3.5a: —
- Layout (after the opcode): `u32 flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)
- HermesProxy: `GetAccountCharacterListRequest` — matches; see #369

### — (0x36c0)

- Modern: 14016 (0x36c0) · 3.3.5a: —
- Layout (after the opcode): `u32 u8*2 bits(6) flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 8 · 9 · 6 · (1 unused)

### — (0x36c1)

- Modern: 14017 (0x36c1) · 3.3.5a: —
- Layout (after the opcode): `u32 u8 u32 guid bits(6) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`

### — (0x36c2)

- Modern: 14018 (0x36c2) · 3.3.5a: —
- Layout (after the opcode): `u32 u8 guid u8 bits(6) flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 7: 9 · 6 · (1 unused)

### CMSG_BATTLE_PAY_GET_PRODUCT_LIST (0x36c4)

- Modern: 14020 (0x36c4) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_BATTLE_PAY_GET_PURCHASE_LIST (0x36c5)

- Modern: 14021 (0x36c5) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_CHARACTER_RENAME_REQUEST (0x36c9)

- Modern: 14025 (0x36c9) · 3.3.5a: 711 (0x2c7)
- Layout (after the opcode): `guid bits(6) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 2: 6 · (2 unused)
- HermesProxy: `CharacterRenameRequest` — matches

### — (0x36ca)

- Modern: 14026 (0x36ca) · 3.3.5a: —
- Layout (after the opcode): `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x36cb)

- Modern: 14027 (0x36cb) · 3.3.5a: —
- Layout (after the opcode): `u32 u64 guid u32`
- Size: 16 bytes (packed GUIDs not counted)

### — (0x36cc)

- Modern: 14028 (0x36cc) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x36cd)

- Modern: 14029 (0x36cd) · 3.3.5a: —
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x36ce)

- Modern: 14030 (0x36ce) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_GUILD_SET_GUILD_MASTER (0x36d0)

- Modern: 14032 (0x36d0) · 3.3.5a: 144 (0x90)
- Layout (after the opcode): `u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · (7 unused)
- HermesProxy: `GuildSetGuildMaster` — matches

### CMSG_PETITION_RENAME_GUILD (0x36d1)

- Modern: 14033 (0x36d1) · 3.3.5a: —
- Layout (after the opcode): `guid bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 2: 7 · (1 unused)
- HermesProxy: `PetitionRenameGuild` — matches

### CMSG_REQUEST_RAID_INFO (0x36d2)

- Modern: 14034 (0x36d2) · 3.3.5a: 717 (0x2cd)
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### — (0x36d3)

- Modern: 14035 (0x36d3) · 3.3.5a: —
- Layout (after the opcode): `u32*2 guid bits(6) opt[ u8 ] alt[ u8 ] bits(4) bits(7) flush bytes bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 10: 6 · 12; byte 12: 7 · (7 unused)

### — (0x36d4)

- Modern: 14036 (0x36d4) · 3.3.5a: —
- Layout (after the opcode): `flush u32 u64`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x36d5)

- Modern: 14037 (0x36d5) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_CONTACT_LIST (0x36d7)

- Modern: 14039 (0x36d7) · 3.3.5a: 102 (0x66)
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `ContactListRequest` — matches

### CMSG_ADD_FRIEND (0x36d8)

- Modern: 14040 (0x36d8) · 3.3.5a: 105 (0x69)
- Layout (after the opcode): `u8*2 bits(2) flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · 10 · (5 unused)
- HermesProxy: `AddFriend` — matches

### CMSG_DEL_FRIEND (0x36d9)

- Modern: 14041 (0x36d9) · 3.3.5a: 106 (0x6a)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `DelFriend` — matches

### CMSG_SET_CONTACT_NOTES (0x36da)

- Modern: 14042 (0x36da) · 3.3.5a: 107 (0x6b)
- Layout (after the opcode): `u32 guid u8 bits(2) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 6: 10 · (6 unused)
- HermesProxy: `SetContactNotes` — matches

### — (0x36db)

- Modern: 14043 (0x36db) · 3.3.5a: —
- Layout (after the opcode): `u32 bits(3) opt[ bits(6) ] flush opt[ bytes ]`
- Bit fields (message of zeros, widths in order written): byte 4: 3 · (5 unused)

### CMSG_ADD_IGNORE (0x36dc)

- Modern: 14044 (0x36dc) · 3.3.5a: 108 (0x6c)
- Layout (after the opcode): `u8 flush guid bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · (7 unused)
- HermesProxy: `AddIgnore` — matches

### CMSG_DEL_IGNORE (0x36dd)

- Modern: 14045 (0x36dd) · 3.3.5a: 109 (0x6d)
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `DelFriend` — matches

### CMSG_SET_RAID_DIFFICULTY (0x36e3)

- Modern: 14051 (0x36e3) · 3.3.5a: —
- Layout (after the opcode): `u32 u8`
- Size: 5 bytes (packed GUIDs not counted)
- HermesProxy: `SetRaidDifficulty` — reads the first part

### CMSG_TUTORIAL_FLAG (0x36e4)

- Modern: 14052 (0x36e4) · 3.3.5a: 254 (0xfe)
- Layout (after the opcode): `bits(2) flush opt[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused)
- HermesProxy: `TutorialSetFlag` — matches

### — (0x36e5)

- Modern: 14053 (0x36e5) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x36e6)

- Modern: 14054 (0x36e6) · 3.3.5a: —
- Layout (after the opcode): `u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_GET_UNDELETE_CHARACTER_COOLDOWN_STATUS (0x36e7)

- Modern: 14055 (0x36e7) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x36eb)

- Modern: 14059 (0x36eb) · 3.3.5a: —
- Layout (after the opcode): `{ u32*6 u8*2 u32 u8 u32*4 u64 u32 u8 u32*6 u8*4 u64*2 u32 bits(6) bits(6) opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] bits(6) opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush bytes bytes bytes }`
- Bit fields (message of zeros, widths in order written): byte 24: 8 · 8; byte 84: 8 · 8 · 8 · 8; byte 108: 6 · 6 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 6 · 1 · 1 · 1 · 1 · 1 · (4 unused)

### — (0x36ec)

- Modern: 14060 (0x36ec) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x36ed)

- Modern: 14061 (0x36ed) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x36ee)

- Modern: 14062 (0x36ee) · 3.3.5a: —
- Layout (after the opcode): `u64*2 u32`
- Size: 20 bytes (packed GUIDs not counted)

### — (0x36ef)

- Modern: 14063 (0x36ef) · 3.3.5a: —
- Layout (after the opcode): `guid u32*2 u64 flush`
- Bit fields (message of zeros, widths in order written): byte 18: 1 · (7 unused)

### — (0x36f0)

- Modern: 14064 (0x36f0) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x36f1)

- Modern: 14065 (0x36f1) · 3.3.5a: —
- Layout (after the opcode): `u32 guid u64`
- Size: 12 bytes (packed GUIDs not counted)

### — (0x36f2)

- Modern: 14066 (0x36f2) · 3.3.5a: —
- Layout (after the opcode): `u32*2 u64 flush`
- Bit fields (message of zeros, widths in order written): byte 16: 1 · (7 unused)

### — (0x36f3)

- Modern: 14067 (0x36f3) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x36f4)

- Modern: 14068 (0x36f4) · 3.3.5a: —
- Layout (after the opcode): `u64 u32*2`
- Size: 16 bytes (packed GUIDs not counted)

### — (0x36f5)

- Modern: 14069 (0x36f5) · 3.3.5a: —
- Layout (after the opcode): `u32 u64 guid u32 flush`
- Bit fields (message of zeros, widths in order written): byte 18: 1 · (7 unused)

### CMSG_COMMERCE_TOKEN_GET_LOG (0x36f6)

- Modern: 14070 (0x36f6) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x36f8)

- Modern: 14072 (0x36f8) · 3.3.5a: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x36f9)

- Modern: 14073 (0x36f9) · 3.3.5a: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x36fa)

- Modern: 14074 (0x36fa) · 3.3.5a: —
- Layout (after the opcode): `{ u32*2 guid u32*2 guid guid guid bits(6) bits(7) bits(7) bits(6) opt[ u8 ] alt[ u8 ] bits(4) opt[ u8 ] flush bytes bytes bytes bytes bytes }`
- Bit fields (message of zeros, widths in order written): byte 24: 6 · 7 · 7 · 6 · 12 · 1 · (1 unused)

### CMSG_UPDATE_VAS_PURCHASE_STATES (0x36fb)

- Modern: 14075 (0x36fb) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_BATTLENET_REQUEST (0x36fd)

- Modern: 14077 (0x36fd) · 3.3.5a: —
- Layout (after the opcode): `u64*2 u32*2 bytes`
- HermesProxy: `BattlenetRequest` — matches

### — (0x36ff)

- Modern: 14079 (0x36ff) · 3.3.5a: —
- Layout (after the opcode): `flush u32 loop[ u32 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### CMSG_CHANGE_REALM_TICKET (0x3701)

- Modern: 14081 (0x3701) · 3.3.5a: —
- Layout (after the opcode): `u32 loop[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 4: (250 unused) · 1 · (5 unused)
- HermesProxy: `ChangeRealmTicket` — matches

### CMSG_REPORT_ENABLED_ADDONS (0x3706)

- Modern: 14086 (0x3706) · 3.3.5a: —
- Layout (after the opcode): `u32 loop[ bits(7) bits(6) opt[ u8 ] opt[ u8 ] flush bytes bytes ]`

### CMSG_REPORT_CLIENT_VARIABLES (0x3707)

- Modern: 14087 (0x3707) · 3.3.5a: —
- Layout (after the opcode): `u32 loop[ bits(6) opt[ u8 ] alt[ u8 ] bits(2) flush bytes bytes ]`

### CMSG_REPORT_KEYBINDING_EXECUTION_COUNTS (0x3708)

- Modern: 14088 (0x3708) · 3.3.5a: —
- Layout (after the opcode): `u8 bits(2) flush loop[ bits(6) bits(6) flush u32 bytes bytes ]`
- Bit fields (message of zeros, widths in order written): byte 0: 10 · (6 unused)

### — (0x370b)

- Modern: 14091 (0x370b) · 3.3.5a: —
- Layout (after the opcode): `guid guid flush`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · (7 unused)

### — (0x370c)

- Modern: 14092 (0x370c) · 3.3.5a: —
- Layout (after the opcode): `u8*2 flush u32 guid u64 u8 bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · 9 · 1 · (5 unused)

### — (0x370d)

- Modern: 14093 (0x370d) · 3.3.5a: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x370f)

- Modern: 14095 (0x370f) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3710)

- Modern: 14096 (0x3710) · 3.3.5a: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x3711)

- Modern: 14097 (0x3711) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3712)

- Modern: 14098 (0x3712) · 3.3.5a: —
- Layout (after the opcode): `u64 u32`
- Size: 12 bytes (packed GUIDs not counted)

### — (0x3713)

- Modern: 14099 (0x3713) · 3.3.5a: —
- Layout (after the opcode): `u32 u8 bits(3) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 11 · (5 unused)

### — (0x3714)

- Modern: 14100 (0x3714) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3716)

- Modern: 14102 (0x3716) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3717)

- Modern: 14103 (0x3717) · 3.3.5a: —
- Layout (after the opcode): `bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)

### — (0x3718)

- Modern: 14104 (0x3718) · 3.3.5a: —
- Layout (after the opcode): `u8`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x371a)

- Modern: 14106 (0x371a) · 3.3.5a: —
- Layout (after the opcode): `bits(6) bits(7) opt[ u8 ] flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 6 · 7 · 1 · (2 unused)

### — (0x371b)

- Modern: 14107 (0x371b) · 3.3.5a: —
- Layout (after the opcode): `bits(7) opt[ u8 ] flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · 1

### CMSG_GET_ACCOUNT_NOTIFICATIONS (0x373c)

- Modern: 14140 (0x373c) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x373d)

- Modern: 14141 (0x373d) · 3.3.5a: —
- Layout (after the opcode): `u64 u32*2`
- Size: 16 bytes (packed GUIDs not counted)

### — (0x3741)

- Modern: 14145 (0x3741) · 3.3.5a: —
- Layout (after the opcode): `flush`
- Bit fields (message of zeros, widths in order written): byte 0: 1 · (7 unused)

### — (0x3742)

- Modern: 14146 (0x3742) · 3.3.5a: —
- Layout (after the opcode): `{ u32 u64 guid u32*2 guid guid guid u8*3 u32 loop[ u32*2 ] bits(6) bits(7) bits(7) opt[ u8 ] opt[ u8 ] opt[ u8 ] flush bytes bytes bytes }`
- Bit fields (message of zeros, widths in order written): byte 28: 8 · 8 · 8; byte 35: 6 · 7 · 7 · 1 · 1 · 1 · (1 unused)

### CMSG_SOCIAL_CONTRACT_REQUEST (0x374c)

- Modern: 14156 (0x374c) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x374d)

- Modern: 14157 (0x374d) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### — (0x3751)

- Modern: 14161 (0x3751) · 3.3.5a: —
- Layout (after the opcode): `u32 loop[ guid ]`

### — (0x3764)

- Modern: 14180 (0x3764) · 3.3.5a: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_AUTH_SESSION (0x3765)

- Modern: 14181 (0x3765) · 3.3.5a: 493 (0x1ed)
- Layout (after the opcode): `u64 u32*3 loop[ u8 ] loop[ u8 ] flush u32 bytes`
- Bit fields (message of zeros, widths in order written): byte 20: 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 8 · 1 · (7 unused)

### CMSG_AUTH_CONTINUED_SESSION (0x3766)

- Modern: 14182 (0x3766) · 3.3.5a: 1298 (0x512)
- Layout (after the opcode): `u64*2 loop[ u8 ] loop[ u8 ]`
- Bit fields (message of zeros, widths in order written): byte 16: (154 unused) · 1 · (165 unused)

### CMSG_ENTER_ENCRYPTED_MODE_ACK (0x3767)

- Modern: 14183 (0x3767) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_PING (0x3768)

- Modern: 14184 (0x3768) · 3.3.5a: 476 (0x1dc)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_LOG_DISCONNECT (0x3769)

- Modern: 14185 (0x3769) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_SUSPEND_TOKEN_RESPONSE (0x376a)

- Modern: 14186 (0x376a) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_ENABLE_NAGLE (0x376b)

- Modern: 14187 (0x376b) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_QUEUED_MESSAGES_END (0x376c)

- Modern: 14188 (0x376c) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x376d)

- Modern: 14189 (0x376d) · 3.3.5a: —
- Layout (after the opcode): `u8 flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · (7 unused)

### — (0x376f)

- Modern: 14191 (0x376f) · 3.3.5a: —
- Layout (after the opcode): `guid u64`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x3770)

- Modern: 14192 (0x3770) · 3.3.5a: —
- Layout (after the opcode): `u64 u32 loop[ guid u64 ]`

### — (0x3771)

- Modern: 14193 (0x3771) · 3.3.5a: —
- Layout (after the opcode): `u32*2 loop[ u32 u16 u8 u64 u32 ]`

### CMSG_QUERY_PLAYER_NAMES (0x3772)

- Modern: 14194 (0x3772) · 3.3.5a: —
- Layout (after the opcode): `u32 loop[ guid ]`
- HermesProxy: `QueryPlayerNames` — matches

### CMSG_CHAT_JOIN_CHANNEL (0x37c8)

- Modern: 14280 (0x37c8) · 3.3.5a: 151 (0x97)
- Layout (after the opcode): `u32 opt[ u8 ] bits(7) bits(7) flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 1 · 1 · 7 · 7
- HermesProxy: `JoinChannel` — matches; see #362

### CMSG_CHAT_LEAVE_CHANNEL (0x37c9)

- Modern: 14281 (0x37c9) · 3.3.5a: 152 (0x98)
- Layout (after the opcode): `u32 bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 7 · (1 unused)
- HermesProxy: `LeaveChannel` — matches

### — (0x37cb)

- Modern: 14283 (0x37cb) · 3.3.5a: —
- Layout (after the opcode): `guid u8`
- Size: 1 bytes (packed GUIDs not counted)

### — (0x37cc)

- Modern: 14284 (0x37cc) · 3.3.5a: —
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)

### CMSG_CHAT_REGISTER_ADDON_PREFIXES (0x37cd)

- Modern: 14285 (0x37cd) · 3.3.5a: —
- Layout (after the opcode): `u32 loop[ bits(5) flush bytes ]`
- HermesProxy: `ChatRegisterAddonPrefixes` — matches

### CMSG_CHAT_UNREGISTER_ALL_ADDON_PREFIXES (0x37ce)

- Modern: 14286 (0x37ce) · 3.3.5a: —
- Layout: empty
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `EmptyClientPacket` — matches

### CMSG_CHAT_MESSAGE_CHANNEL (0x37cf)

- Modern: 14287 (0x37cf) · 3.3.5a: —
- Layout (after the opcode): `{ u32 guid u8*2 bits(3) opt[ u8 ] opt[ u8 ] flush bytes bytes }`
- Bit fields (message of zeros, widths in order written): byte 6: 9 · 11 · 1 · (3 unused)
- HermesProxy: `ChatMessageChannel` — matches

### CMSG_CHAT_MESSAGE_WHISPER (0x37d0)

- Modern: 14288 (0x37d0) · 3.3.5a: —
- Layout (after the opcode): `u32 u8*2 bits(3) flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 9 · 11 · (4 unused)
- HermesProxy: `ChatMessageWhisper` — matches

### CMSG_CHAT_MESSAGE_GUILD (0x37d1)

- Modern: 14289 (0x37d1) · 3.3.5a: —
- Layout (after the opcode): `u32 u8 bits(3) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 11 · (5 unused)
- HermesProxy: `ChatMessage` — differs (#362)

### CMSG_CHAT_MESSAGE_OFFICER (0x37d2)

- Modern: 14290 (0x37d2) · 3.3.5a: —
- Layout (after the opcode): `u32 u8 bits(3) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 11 · (5 unused)
- HermesProxy: `ChatMessage` — differs (#362)

### CMSG_CHAT_MESSAGE_AFK (0x37d3)

- Modern: 14291 (0x37d3) · 3.3.5a: —
- Layout (after the opcode): `u8 bits(3) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 11 · (5 unused)
- HermesProxy: `ChatMessageAFK` — matches

### CMSG_CHAT_MESSAGE_DND (0x37d4)

- Modern: 14292 (0x37d4) · 3.3.5a: —
- Layout (after the opcode): `u8 bits(3) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 11 · (5 unused)
- HermesProxy: `ChatMessageDND` — matches

### CMSG_CHAT_CHANNEL_LIST (0x37d5)

- Modern: 14293 (0x37d5) · 3.3.5a: 154 (0x9a)
- Layout (after the opcode): `bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)
- HermesProxy: `ChannelCommand` — matches

### CMSG_CHAT_CHANNEL_DISPLAY_LIST (0x37d6)

- Modern: 14294 (0x37d6) · 3.3.5a: 978 (0x3d2)
- Layout (after the opcode): `bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)
- HermesProxy: `ChannelCommand` — matches

### CMSG_CHAT_CHANNEL_PASSWORD (0x37d7)

- Modern: 14295 (0x37d7) · 3.3.5a: 156 (0x9c)
- Layout (after the opcode): `bits(7) bits(7) flush bytes bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · 7 · (2 unused)

### CMSG_CHAT_CHANNEL_SET_OWNER (0x37d8)

- Modern: 14296 (0x37d8) · 3.3.5a: 157 (0x9d)
- Layout (after the opcode): `bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`

### CMSG_CHAT_CHANNEL_OWNER (0x37d9)

- Modern: 14297 (0x37d9) · 3.3.5a: 158 (0x9e)
- Layout (after the opcode): `bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)
- HermesProxy: `ChannelCommand` — matches

### CMSG_CHAT_CHANNEL_MODERATOR (0x37db)

- Modern: 14299 (0x37db) · 3.3.5a: 159 (0x9f)
- Layout (after the opcode): `bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`

### CMSG_CHAT_CHANNEL_UNMODERATOR (0x37dc)

- Modern: 14300 (0x37dc) · 3.3.5a: 160 (0xa0)
- Layout (after the opcode): `bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`

### CMSG_CHAT_CHANNEL_INVITE (0x37df)

- Modern: 14303 (0x37df) · 3.3.5a: 163 (0xa3)
- Layout (after the opcode): `bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`

### CMSG_CHAT_CHANNEL_KICK (0x37e0)

- Modern: 14304 (0x37e0) · 3.3.5a: 164 (0xa4)
- Layout (after the opcode): `bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`

### CMSG_CHAT_CHANNEL_BAN (0x37e1)

- Modern: 14305 (0x37e1) · 3.3.5a: 165 (0xa5)
- Layout (after the opcode): `bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`

### CMSG_CHAT_CHANNEL_UNBAN (0x37e2)

- Modern: 14306 (0x37e2) · 3.3.5a: 166 (0xa6)
- Layout (after the opcode): `bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`

### CMSG_CHAT_CHANNEL_ANNOUNCEMENTS (0x37e3)

- Modern: 14307 (0x37e3) · 3.3.5a: 167 (0xa7)
- Layout (after the opcode): `bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)
- HermesProxy: `ChannelCommand` — matches

### CMSG_CHAT_CHANNEL_SILENCE_ALL (0x37e4)

- Modern: 14308 (0x37e4) · 3.3.5a: 973 (0x3cd)
- Layout (after the opcode): `bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`

### CMSG_CHAT_CHANNEL_UNSILENCE_ALL (0x37e5)

- Modern: 14309 (0x37e5) · 3.3.5a: 975 (0x3cf)
- Layout (after the opcode): `bits(7) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush bytes bytes`

### CMSG_CHAT_CHANNEL_DECLINE_INVITE (0x37e6)

- Modern: 14310 (0x37e6) · 3.3.5a: 1040 (0x410)
- Layout (after the opcode): `bits(7) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 7 · (1 unused)
- HermesProxy: `ChannelCommand` — matches

### CMSG_CHAT_MESSAGE_SAY (0x37e7)

- Modern: 14311 (0x37e7) · 3.3.5a: —
- Layout (after the opcode): `u32 u8 bits(3) opt[ u8 ] flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 11 · 1 · (4 unused)
- HermesProxy: `ChatMessage` — matches

### CMSG_CHAT_MESSAGE_EMOTE (0x37e8)

- Modern: 14312 (0x37e8) · 3.3.5a: —
- Layout (after the opcode): `u8 bits(3) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 11 · (5 unused)
- HermesProxy: `ChatMessageEmote` — matches

### CMSG_CHAT_MESSAGE_YELL (0x37e9)

- Modern: 14313 (0x37e9) · 3.3.5a: —
- Layout (after the opcode): `u32 u8 bits(3) flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 11 · (5 unused)
- HermesProxy: `ChatMessage` — differs (#362)

### CMSG_CHAT_MESSAGE_PARTY (0x37ea)

- Modern: 14314 (0x37ea) · 3.3.5a: —
- Layout (after the opcode): `u32 u8 bits(3) opt[ u8 ] flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 11 · 1 · (4 unused)
- HermesProxy: `ChatMessage` — matches

### CMSG_CHAT_MESSAGE_RAID (0x37eb)

- Modern: 14315 (0x37eb) · 3.3.5a: —
- Layout (after the opcode): `u32 u8 bits(3) opt[ u8 ] flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 11 · 1 · (4 unused)
- HermesProxy: `ChatMessage` — matches

### CMSG_CHAT_MESSAGE_INSTANCE_CHAT (0x37ec)

- Modern: 14316 (0x37ec) · 3.3.5a: —
- Layout (after the opcode): `u32 u8 bits(3) opt[ u8 ] flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 11 · 1 · (4 unused)
- HermesProxy: `ChatMessage` — matches

### CMSG_CHAT_MESSAGE_RAID_WARNING (0x37ed)

- Modern: 14317 (0x37ed) · 3.3.5a: —
- Layout (after the opcode): `u32 u8 bits(3) opt[ u8 ] flush bytes`
- Bit fields (message of zeros, widths in order written): byte 4: 11 · 1 · (4 unused)
- HermesProxy: `ChatMessage` — matches

### CMSG_CHAT_ADDON_MESSAGE (0x37ee)

- Modern: 14318 (0x37ee) · 3.3.5a: —
- Layout (after the opcode): `{ bits(5) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush u32 bytes bytes }`
- Bit fields (message of zeros, widths in order written): byte 0: 5 · 8 · 1 · (2 unused)
- HermesProxy: `ChatAddonMessage` — matches

### — (0x37ef)

- Modern: 14319 (0x37ef) · 3.3.5a: —
- Layout (after the opcode): `u8 flush { bits(5) opt[ u8 ] alt[ u8 ] opt[ u8 ] flush u32 bytes bytes } guid bytes`
- Bit fields (message of zeros, widths in order written): byte 0: 9 · (7 unused) · 5 · 8 · 1 · (2 unused)

### CMSG_WRAP_ITEM (0x3994)

- Modern: 14740 (0x3994) · 3.3.5a: 467 (0x1d3)
- Layout (after the opcode): `bits(2) flush loop[ u8*2 ]`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused)
- HermesProxy: `WrapItem` — matches

### CMSG_USE_EQUIPMENT_SET (0x3995)

- Modern: 14741 (0x3995) · 3.3.5a: —
- Layout (after the opcode): `bits(2) flush loop[ u8*2 ] loop[ guid u8*2 ] u64`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused); byte 3: 8 · 8; byte 7: 8 · 8; byte 11: 8 · 8; byte 15: 8 · 8; byte 19: 8 · 8; byte 23: 8 · 8; byte 27: 8 · 8; byte 31: 8 · 8; byte 35: 8 · 8; byte 39: 8 · 8; byte 43: 8 · 8; byte 47: 8 · 8; byte 51: 8 · 8; byte 55: 8 · 8; byte 59: 8 · 8; byte 63: 8 · 8; byte 67: 8 · 8; byte 71: 8 · 8; byte 75: 8 · 8
- HermesProxy: `UseEquipmentSet` — matches

### CMSG_AUTOSTORE_BANK_ITEM (0x3996)

- Modern: 14742 (0x3996) · 3.3.5a: 642 (0x282)
- Layout (after the opcode): `bits(2) flush loop[ u8*2 ] u8*2`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused) · 8 · 8
- HermesProxy: `AutoEquipItem` — matches

### CMSG_AUTOBANK_ITEM (0x3997)

- Modern: 14743 (0x3997) · 3.3.5a: 643 (0x283)
- Layout (after the opcode): `bits(2) flush loop[ u8*2 ] u8*2`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused) · 8 · 8
- HermesProxy: `AutoEquipItem` — matches

### CMSG_AUTO_EQUIP_ITEM (0x3998)

- Modern: 14744 (0x3998) · 3.3.5a: 266 (0x10a)
- Layout (after the opcode): `bits(2) flush loop[ u8*2 ] u8*2`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused) · 8 · 8
- HermesProxy: `AutoEquipItem` — matches

### CMSG_AUTO_STORE_BAG_ITEM (0x3999)

- Modern: 14745 (0x3999) · 3.3.5a: 267 (0x10b)
- Layout (after the opcode): `bits(2) flush loop[ u8*2 ] u8*3`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused) · 8 · 8 · 8
- HermesProxy: `AutoStoreBagItem` — matches

### CMSG_SWAP_ITEM (0x399a)

- Modern: 14746 (0x399a) · 3.3.5a: 268 (0x10c)
- Layout (after the opcode): `bits(2) flush loop[ u8*2 ] u8*4`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused) · 8 · 8 · 8 · 8
- HermesProxy: `SwapItem` — matches

### CMSG_SWAP_INV_ITEM (0x399b)

- Modern: 14747 (0x399b) · 3.3.5a: 269 (0x10d)
- Layout (after the opcode): `bits(2) flush loop[ u8*2 ] u8*2`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused) · 8 · 8
- HermesProxy: `SwapInvItem` — matches

### CMSG_SPLIT_ITEM (0x399c)

- Modern: 14748 (0x399c) · 3.3.5a: 270 (0x10e)
- Layout (after the opcode): `bits(2) flush loop[ u8*2 ] u8*4 u32`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused) · 8 · 8 · 8 · 8
- HermesProxy: `SplitItem` — matches

### CMSG_AUTO_EQUIP_ITEM_SLOT (0x399d)

- Modern: 14749 (0x399d) · 3.3.5a: 271 (0x10f)
- Layout (after the opcode): `bits(2) flush loop[ u8*2 ] guid u8`
- Bit fields (message of zeros, widths in order written): byte 0: 2 · (6 unused)
- HermesProxy: `AutoEquipItemSlot` — matches

### CMSG_MOVE_START_FORWARD (0x39e4)

- Modern: 14820 (0x39e4) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_START_BACKWARD (0x39e5)

- Modern: 14821 (0x39e5) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_STOP (0x39e6)

- Modern: 14822 (0x39e6) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_START_STRAFE_LEFT (0x39e7)

- Modern: 14823 (0x39e7) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_START_STRAFE_RIGHT (0x39e8)

- Modern: 14824 (0x39e8) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_STOP_STRAFE (0x39e9)

- Modern: 14825 (0x39e9) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_JUMP (0x39ea)

- Modern: 14826 (0x39ea) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_DOUBLE_JUMP (0x39eb)

- Modern: 14827 (0x39eb) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_START_TURN_LEFT (0x39ec)

- Modern: 14828 (0x39ec) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_START_TURN_RIGHT (0x39ed)

- Modern: 14829 (0x39ed) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_STOP_TURN (0x39ee)

- Modern: 14830 (0x39ee) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_START_PITCH_UP (0x39ef)

- Modern: 14831 (0x39ef) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_START_PITCH_DOWN (0x39f0)

- Modern: 14832 (0x39f0) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_STOP_PITCH (0x39f1)

- Modern: 14833 (0x39f1) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_SET_RUN_MODE (0x39f2)

- Modern: 14834 (0x39f2) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_SET_WALK_MODE (0x39f3)

- Modern: 14835 (0x39f3) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_TELEPORT_ACK (0x39fa)

- Modern: 14842 (0x39fa) · 3.3.5a: —
- Layout (after the opcode): `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `MoveTeleportAck` — matches

### CMSG_MOVE_FALL_LAND (0x39fb)

- Modern: 14843 (0x39fb) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_START_SWIM (0x39fc)

- Modern: 14844 (0x39fc) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_STOP_SWIM (0x39fd)

- Modern: 14845 (0x39fd) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_SET_FACING (0x3a09)

- Modern: 14857 (0x3a09) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_SET_PITCH (0x3a0a)

- Modern: 14858 (0x3a0a) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_FORCE_RUN_SPEED_CHANGE_ACK (0x3a0b)

- Modern: 14859 (0x3a0b) · 3.3.5a: 227 (0xe3)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 f32 ] u32 f32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `MovementSpeedAck` — differs (#366)

### CMSG_MOVE_FORCE_RUN_BACK_SPEED_CHANGE_ACK (0x3a0c)

- Modern: 14860 (0x3a0c) · 3.3.5a: 229 (0xe5)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 f32 ] u32 f32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `MovementSpeedAck` — differs (#366)

### CMSG_MOVE_FORCE_SWIM_SPEED_CHANGE_ACK (0x3a0d)

- Modern: 14861 (0x3a0d) · 3.3.5a: 231 (0xe7)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 f32 ] u32 f32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `MovementSpeedAck` — differs (#366)

### CMSG_MOVE_FORCE_ROOT_ACK (0x3a0e)

- Modern: 14862 (0x3a0e) · 3.3.5a: 233 (0xe9)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `MovementAckMessage` — differs (#366)

### CMSG_MOVE_FORCE_UNROOT_ACK (0x3a0f)

- Modern: 14863 (0x3a0f) · 3.3.5a: 235 (0xeb)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `MovementAckMessage` — differs (#366)

### CMSG_MOVE_HEARTBEAT (0x3a10)

- Modern: 14864 (0x3a10) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_KNOCK_BACK_ACK (0x3a12)

- Modern: 14866 (0x3a12) · 3.3.5a: 240 (0xf0)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] flush opt[ f32*2 ]`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1; byte 55: 1 · (7 unused)
- HermesProxy: `MovementAckMessage` — differs (#369)

### CMSG_MOVE_HOVER_ACK (0x3a13)

- Modern: 14867 (0x3a13) · 3.3.5a: 246 (0xf6)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `MovementAckMessage` — differs (#366)

### CMSG_MOVE_SET_VEHICLE_REC_ID_ACK (0x3a14)

- Modern: 14868 (0x3a14) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a15)

- Modern: 14869 (0x3a15) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] { guid f32*6 u32 f32 u32 bits(2) flush }`

### — (0x3a16)

- Modern: 14870 (0x3a16) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] guid`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_REMOVE_MOVEMENT_FORCES (0x3a17)

- Modern: 14871 (0x3a17) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_SPLINE_DONE (0x3a18)

- Modern: 14872 (0x3a18) · 3.3.5a: 713 (0x2c9)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `MoveSplineDone` — differs (#366)

### CMSG_MOVE_FALL_RESET (0x3a19)

- Modern: 14873 (0x3a19) · 3.3.5a: 714 (0x2ca)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### — (0x3a1a)

- Modern: 14874 (0x3a1a) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_TIME_SKIPPED (0x3a1b)

- Modern: 14875 (0x3a1b) · 3.3.5a: 718 (0x2ce)
- Layout (after the opcode): `guid u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `MoveTimeSkipped` — matches

### CMSG_MOVE_FEATHER_FALL_ACK (0x3a1c)

- Modern: 14876 (0x3a1c) · 3.3.5a: 719 (0x2cf)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `MovementAckMessage` — differs (#366)

### CMSG_MOVE_WATER_WALK_ACK (0x3a1d)

- Modern: 14877 (0x3a1d) · 3.3.5a: 720 (0x2d0)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `MovementAckMessage` — differs (#366)

### — (0x3a1e)

- Modern: 14878 (0x3a1e) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_FORCE_WALK_SPEED_CHANGE_ACK (0x3a21)

- Modern: 14881 (0x3a21) · 3.3.5a: 731 (0x2db)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 f32 ] u32 f32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `MovementSpeedAck` — differs (#366)

### CMSG_MOVE_FORCE_SWIM_BACK_SPEED_CHANGE_ACK (0x3a22)

- Modern: 14882 (0x3a22) · 3.3.5a: 733 (0x2dd)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 f32 ] u32 f32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `MovementSpeedAck` — differs (#366)

### CMSG_MOVE_FORCE_TURN_RATE_CHANGE_ACK (0x3a23)

- Modern: 14883 (0x3a23) · 3.3.5a: 735 (0x2df)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 f32 ] u32 f32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `MovementSpeedAck` — differs (#366)

### — (0x3a24)

- Modern: 14884 (0x3a24) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a25)

- Modern: 14885 (0x3a25) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a26)

- Modern: 14886 (0x3a26) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_CAN_FLY_ACK (0x3a27)

- Modern: 14887 (0x3a27) · 3.3.5a: 837 (0x345)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `MovementAckMessage` — differs (#366)

### CMSG_MOVE_SET_FLY (0x3a28)

- Modern: 14888 (0x3a28) · 3.3.5a: 838 (0x346)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_START_ASCEND (0x3a29)

- Modern: 14889 (0x3a29) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_STOP_ASCEND (0x3a2a)

- Modern: 14890 (0x3a2a) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_FORCE_FLIGHT_SPEED_CHANGE_ACK (0x3a2d)

- Modern: 14893 (0x3a2d) · 3.3.5a: 898 (0x382)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 f32 ] u32 f32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `MovementSpeedAck` — differs (#366)

### CMSG_MOVE_FORCE_FLIGHT_BACK_SPEED_CHANGE_ACK (0x3a2e)

- Modern: 14894 (0x3a2e) · 3.3.5a: 900 (0x384)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 f32 ] u32 f32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `MovementSpeedAck` — differs (#366)

### CMSG_MOVE_CHANGE_TRANSPORT (0x3a2f)

- Modern: 14895 (0x3a2f) · 3.3.5a: 909 (0x38d)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_START_DESCEND (0x3a30)

- Modern: 14896 (0x3a30) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### CMSG_MOVE_FORCE_PITCH_RATE_CHANGE_ACK (0x3a32)

- Modern: 14898 (0x3a32) · 3.3.5a: 1117 (0x45d)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 f32 ] u32 f32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `MovementSpeedAck` — differs (#366)

### CMSG_MOVE_DISMISS_VEHICLE (0x3a33)

- Modern: 14899 (0x3a33) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `RequestVehicleSeatChange` — reads the first part

### — (0x3a34)

- Modern: 14900 (0x3a34) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } guid u8`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_GRAVITY_DISABLE_ACK (0x3a35)

- Modern: 14901 (0x3a35) · 3.3.5a: 1231 (0x4cf)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `MovementAckMessage` — differs (#366)

### CMSG_MOVE_GRAVITY_ENABLE_ACK (0x3a36)

- Modern: 14902 (0x3a36) · 3.3.5a: 1233 (0x4d1)
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `MovementAckMessage` — differs (#366)

### — (0x3a37)

- Modern: 14903 (0x3a37) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a38)

- Modern: 14904 (0x3a38) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a39)

- Modern: 14905 (0x3a39) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a3a)

- Modern: 14906 (0x3a3a) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_COLLISION_HEIGHT_ACK (0x3a3b)

- Modern: 14907 (0x3a3b) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32 u32 u8`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `MoveSetCollisionHeightAck` — differs (#366)

### CMSG_SET_ACTIVE_MOVER (0x3a3c)

- Modern: 14908 (0x3a3c) · 3.3.5a: 618 (0x26a)
- Layout (after the opcode): `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `SetActiveMover` — matches

### CMSG_TIME_SYNC_RESPONSE (0x3a3d)

- Modern: 14909 (0x3a3d) · 3.3.5a: 913 (0x391)
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)
- HermesProxy: `TimeSyncResponse` — matches

### — (0x3a3e)

- Modern: 14910 (0x3a3e) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### CMSG_TIME_SYNC_RESPONSE_DROPPED (0x3a3f)

- Modern: 14911 (0x3a3f) · 3.3.5a: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x3a40)

- Modern: 14912 (0x3a40) · 3.3.5a: —
- Layout (after the opcode): `u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### CMSG_DISCARDED_TIME_SYNC_ACKS (0x3a41)

- Modern: 14913 (0x3a41) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x3a42)

- Modern: 14914 (0x3a42) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 f32 ] u32 f32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_UPDATE_MISSILE_TRAJECTORY (0x3a43)

- Modern: 14915 (0x3a43) · 3.3.5a: 1122 (0x462)
- Layout (after the opcode): `guid guid u16 u32 f32*8 flush opt[ { guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } ]`
- Bit fields (message of zeros, widths in order written): byte 42: 1 · (7 unused)

### — (0x3a44)

- Modern: 14916 (0x3a44) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_INIT_ACTIVE_MOVER_COMPLETE (0x3a46)

- Modern: 14918 (0x3a46) · 3.3.5a: —
- Layout (after the opcode): `u32`
- Size: 4 bytes (packed GUIDs not counted)
- HermesProxy: `InitActiveMoverComplete` — matches

### — (0x3a4e)

- Modern: 14926 (0x3a4e) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] u32*2`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a4f)

- Modern: 14927 (0x3a4f) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a50)

- Modern: 14928 (0x3a50) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] { guid f32*6 u32 f32 u32 bits(2) flush }`

### — (0x3a51)

- Modern: 14929 (0x3a51) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a52)

- Modern: 14930 (0x3a52) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a53)

- Modern: 14931 (0x3a53) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 f32 ] u32 f32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a54)

- Modern: 14932 (0x3a54) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 f32 ] u32 f32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a55)

- Modern: 14933 (0x3a55) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 f32 ] u32 f32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a56)

- Modern: 14934 (0x3a56) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 f32 ] u32 f32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a57)

- Modern: 14935 (0x3a57) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 f32 ] u32 f32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a58)

- Modern: 14936 (0x3a58) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 f32 ] u32 f32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a59)

- Modern: 14937 (0x3a59) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32*2`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a5a)

- Modern: 14938 (0x3a5a) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32*2`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a5b)

- Modern: 14939 (0x3a5b) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32*2`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a5c)

- Modern: 14940 (0x3a5c) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] alt[ u32 ] f32*2`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a5d)

- Modern: 14941 (0x3a5d) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 f32 ] u32 f32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a5e)

- Modern: 14942 (0x3a5e) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 f32 ] u32 f32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### CMSG_MOVE_SET_FACING_HEARTBEAT (0x3a5f)

- Modern: 14943 (0x3a5f) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] }`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `ClientPlayerMovement` — differs (#366)

### — (0x3a60)

- Modern: 14944 (0x3a60) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 f32 ] u32 f32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x3a63)

- Modern: 14947 (0x3a63) · 3.3.5a: —
- Layout (after the opcode): `{ guid loop[ u32 ] u32 f32*6 u32*2 loop[ guid ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] opt[ u8 ] flush opt[ { guid f32*4 u8 u32 opt[ u8 ] flush opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32 f32*3 u32 ] opt[ f32*2 ] opt[ u32 f32 flush opt[ f32*3 ] ] } opt[ u32 ] u32`
- Bit fields (message of zeros, widths in order written): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
