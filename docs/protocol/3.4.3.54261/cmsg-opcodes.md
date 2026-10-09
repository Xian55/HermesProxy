# Client opcodes (CMSG), 3.4.3.54261

Every client-to-server opcode the client sends. Layouts are in [cmsg-structures](cmsg-structures.md).

| Name | Hex | Dec | 3.3.5a | Layout | HermesProxy |
|---|---|---:|---|---|---|
| CMSG_GUILD_PROMOTE_MEMBER | 0x305d | 12381 | 139 (0x8b) | fixed, 0 bytes | matches |
| CMSG_GUILD_DEMOTE_MEMBER | 0x305e | 12382 | 140 (0x8c) | fixed, 0 bytes | matches |
| — | 0x305f | 12383 | — | fixed, 4 bytes | — |
| CMSG_GUILD_DECLINE_INVITATION | 0x3060 | 12384 | 133 (0x85) | empty | matches |
| CMSG_GUILD_AUTO_DECLINE_INVITATION | 0x3061 | 12385 | — | empty | matches |
| CMSG_GUILD_LEAVE | 0x3062 | 12386 | 141 (0x8d) | empty | matches |
| CMSG_GUILD_OFFICER_REMOVE_MEMBER | 0x3063 | 12387 | 142 (0x8e) | fixed, 0 bytes | matches |
| CMSG_GUILD_ADD_RANK | 0x3064 | 12388 | 562 (0x232) | variable | matches |
| CMSG_GUILD_DELETE_RANK | 0x3065 | 12389 | 563 (0x233) | fixed, 4 bytes | matches |
| — | 0x3066 | 12390 | — | variable | — |
| CMSG_GUILD_SET_RANK_PERMISSIONS | 0x3067 | 12391 | 561 (0x231) | variable | matches |
| CMSG_GUILD_DELETE | 0x3068 | 12392 | 143 (0x8f) | empty | matches |
| — | 0x306b | 12395 | — | fixed, 12 bytes | — |
| CMSG_GUILD_GET_RANKS | 0x306d | 12397 | — | fixed, 0 bytes | no handler |
| CMSG_GUILD_SET_ACHIEVEMENT_TRACKING | 0x306f | 12399 | — | variable | no handler |
| CMSG_GUILD_SET_MEMBER_NOTE | 0x3072 | 12402 | — | variable | matches |
| CMSG_GUILD_GET_ROSTER | 0x3073 | 12403 | 137 (0x89) | empty | matches |
| CMSG_GUILD_UPDATE_MOTD_TEXT | 0x3074 | 12404 | 145 (0x91) | variable | matches |
| CMSG_GUILD_UPDATE_INFO_TEXT | 0x3075 | 12405 | 764 (0x2fc) | variable | matches |
| — | 0x307e | 12414 | — | variable | — |
| CMSG_GUILD_BANK_LOG_QUERY | 0x3082 | 12418 | — | fixed, 4 bytes | matches |
| CMSG_GUILD_BANK_REMAINING_WITHDRAW_MONEY_QUERY | 0x3083 | 12419 | — | empty | matches |
| CMSG_GUILD_PERMISSIONS_QUERY | 0x3084 | 12420 | — | empty | matches |
| CMSG_GUILD_EVENT_LOG_QUERY | 0x3085 | 12421 | — | empty | no handler |
| CMSG_GUILD_BANK_SET_TAB_TEXT | 0x3086 | 12422 | 1035 (0x40b) | variable | differs |
| CMSG_GUILD_BANK_TEXT_QUERY | 0x3087 | 12423 | — | fixed, 4 bytes | matches |
| — | 0x3088 | 12424 | — | empty | — |
| — | 0x308c | 12428 | — | variable | — |
| — | 0x3126 | 12582 | — | fixed, 0 bytes | — |
| — | 0x3127 | 12583 | — | fixed, 4 bytes | — |
| CMSG_TOY_CLEAR_FANFARE | 0x3128 | 12584 | — | fixed, 4 bytes | matches |
| CMSG_INITIATE_TRADE | 0x3156 | 12630 | 278 (0x116) | fixed, 0 bytes | matches |
| CMSG_BEGIN_TRADE | 0x3157 | 12631 | 279 (0x117) | empty | matches |
| CMSG_BUSY_TRADE | 0x3158 | 12632 | 280 (0x118) | empty | matches |
| CMSG_IGNORE_TRADE | 0x3159 | 12633 | 281 (0x119) | empty | matches |
| CMSG_ACCEPT_TRADE | 0x315a | 12634 | 282 (0x11a) | fixed, 4 bytes | matches |
| CMSG_UNACCEPT_TRADE | 0x315b | 12635 | 283 (0x11b) | empty | matches |
| CMSG_CANCEL_TRADE | 0x315c | 12636 | 284 (0x11c) | empty | matches |
| CMSG_SET_TRADE_ITEM | 0x315d | 12637 | 285 (0x11d) | fixed, 3 bytes | matches |
| CMSG_CLEAR_TRADE_ITEM | 0x315e | 12638 | 286 (0x11e) | fixed, 1 bytes | matches |
| CMSG_SET_TRADE_GOLD | 0x315f | 12639 | 287 (0x11f) | fixed, 8 bytes | matches |
| CMSG_STABLE_PET | 0x3168 | 12648 | 624 (0x270) | fixed, 0 bytes | matches |
| CMSG_UNSTABLE_PET | 0x3169 | 12649 | 625 (0x271) | fixed, 4 bytes | matches |
| CMSG_STABLE_SWAP_PET | 0x316a | 12650 | 629 (0x275) | fixed, 4 bytes | matches |
| CMSG_BUY_STABLE_SLOT | 0x316b | 12651 | 626 (0x272) | fixed, 0 bytes | matches |
| CMSG_SET_CURRENCY_FLAGS | 0x316c | 12652 | — | fixed, 8 bytes | no handler |
| CMSG_BATTLEFIELD_LEAVE | 0x3175 | 12661 | 737 (0x2e1) | empty | matches |
| CMSG_QUERY_QUEST_COMPLETION_NPCS | 0x3177 | 12663 | 1161 (0x489) | variable | no handler |
| — | 0x3178 | 12664 | — | variable | — |
| CMSG_REQUEST_CEMETERY_LIST | 0x3179 | 12665 | — | empty | no handler |
| — | 0x317a | 12666 | — | fixed, 4 bytes | — |
| CMSG_REQUEST_HONOR_STATS | 0x317e | 12670 | — | fixed, 0 bytes | matches |
| CMSG_PVP_LOG_DATA | 0x317f | 12671 | — | empty | matches |
| CMSG_BATTLEFIELD_LIST | 0x3181 | 12673 | 572 (0x23c) | fixed, 4 bytes | matches |
| CMSG_CANCEL_QUEUED_SPELL | 0x3182 | 12674 | — | empty | no handler (#365) |
| CMSG_OBJECT_UPDATE_FAILED | 0x3183 | 12675 | — | fixed, 0 bytes | matches |
| — | 0x3184 | 12676 | — | fixed, 0 bytes | — |
| CMSG_VIOLENCE_LEVEL | 0x3187 | 12679 | — | fixed, 1 bytes | no handler |
| CMSG_USED_FOLLOW | 0x3189 | 12681 | — | empty | no handler |
| CMSG_STAND_STATE_CHANGE | 0x318c | 12684 | 257 (0x101) | fixed, 4 bytes | matches |
| — | 0x318d | 12685 | — | fixed, 16 bytes | — |
| CMSG_SAVE_CUF_PROFILES | 0x318e | 12686 | — | variable | matches |
| CMSG_REQUEST_PVP_REWARDS | 0x3196 | 12694 | — | empty | no handler |
| — | 0x3197 | 12695 | — | empty | — |
| — | 0x31a2 | 12706 | — | fixed, 0 bytes | — |
| — | 0x31a3 | 12707 | — | fixed, 0 bytes | — |
| — | 0x31a4 | 12708 | — | variable | — |
| — | 0x31a5 | 12709 | — | fixed, 4 bytes | — |
| CMSG_QUERY_COUNTDOWN_TIMER | 0x31aa | 12714 | — | fixed, 4 bytes | no handler |
| CMSG_CANCEL_AURA | 0x31af | 12719 | 310 (0x136) | fixed, 4 bytes | matches |
| — | 0x31b1 | 12721 | — | empty | — |
| — | 0x31b2 | 12722 | — | empty | — |
| — | 0x31b9 | 12729 | — | variable | — |
| — | 0x31ba | 12730 | — | variable | — |
| CMSG_AREA_TRIGGER | 0x31d6 | 12758 | 180 (0xb4) | variable | matches |
| — | 0x31df | 12767 | — | fixed, 0 bytes | — |
| — | 0x31e0 | 12768 | — | empty | — |
| CMSG_REQUEST_FORCED_REACTIONS | 0x3205 | 12805 | — | empty | no handler |
| — | 0x3207 | 12807 | — | fixed, 12 bytes | — |
| CMSG_CONFIRM_RESPEC_WIPE | 0x320d | 12813 | — | fixed, 1 bytes | matches |
| — | 0x320e | 12814 | — | empty | — |
| CMSG_LOOT_UNIT | 0x320f | 12815 | 349 (0x15d) | fixed, 0 bytes | matches |
| CMSG_LOOT_MONEY | 0x3210 | 12816 | 350 (0x15e) | variable | matches; see #369 |
| CMSG_AUTOSTORE_LOOT_ITEM / CMSG_LOOT_ITEM | 0x3211 | 12817 | 264 (0x108) | variable | matches |
| CMSG_LOOT_MASTER_GIVE | 0x3212 | 12818 | 675 (0x2a3) | variable | matches |
| CMSG_LOOT_RELEASE | 0x3213 | 12819 | 351 (0x15f) | fixed, 0 bytes | matches |
| CMSG_LOOT_ROLL | 0x3214 | 12820 | 672 (0x2a0) | fixed, 2 bytes | matches |
| — | 0x321f | 12831 | — | fixed, 4 bytes | — |
| — | 0x3220 | 12832 | — | fixed, 4 bytes | — |
| — | 0x3221 | 12833 | — | variable | — |
| CMSG_SET_DIFFICULTY_ID | 0x3222 | 12834 | — | fixed, 4 bytes | no handler |
| — | 0x3223 | 12835 | — | fixed, 0 bytes | — |
| CMSG_MAIL_DELETE | 0x3225 | 12837 | 585 (0x249) | fixed, 12 bytes | matches |
| CMSG_REQUEST_VEHICLE_EXIT | 0x3237 | 12855 | 1142 (0x476) | empty | matches |
| CMSG_REQUEST_VEHICLE_PREV_SEAT | 0x3238 | 12856 | 1143 (0x477) | empty | matches |
| CMSG_REQUEST_VEHICLE_NEXT_SEAT | 0x3239 | 12857 | 1144 (0x478) | empty | matches |
| CMSG_REQUEST_VEHICLE_SWITCH_SEAT | 0x323a | 12858 | 1145 (0x479) | fixed, 1 bytes | matches |
| CMSG_RIDE_VEHICLE_INTERACT | 0x323b | 12859 | — | fixed, 0 bytes | matches |
| CMSG_EJECT_PASSENGER | 0x323c | 12860 | 1193 (0x4a9) | fixed, 0 bytes | matches |
| — | 0x3241 | 12865 | — | fixed, 0 bytes | — |
| — | 0x3247 | 12871 | — | fixed, 4 bytes | — |
| CMSG_ATTACK_SWING | 0x3255 | 12885 | 321 (0x141) | fixed, 0 bytes | matches |
| CMSG_ATTACK_STOP | 0x3256 | 12886 | 322 (0x142) | empty | matches |
| CMSG_CANCEL_CHANNELLING | 0x326a | 12906 | 315 (0x13b) | fixed, 8 bytes | matches |
| CMSG_CANCEL_GROWTH_AURA | 0x326f | 12911 | 667 (0x29b) | empty | no handler |
| CMSG_QUERY_CREATURE | 0x3270 | 12912 | 96 (0x60) | fixed, 4 bytes | matches |
| CMSG_QUERY_GAME_OBJECT | 0x3271 | 12913 | 94 (0x5e) | fixed, 4 bytes | matches |
| CMSG_QUERY_NPC_TEXT | 0x3272 | 12914 | 383 (0x17f) | fixed, 4 bytes | matches |
| CMSG_QUERY_QUEST_INFO | 0x3273 | 12915 | 92 (0x5c) | fixed, 4 bytes | matches |
| CMSG_QUERY_PAGE_TEXT | 0x3274 | 12916 | 90 (0x5a) | fixed, 4 bytes | matches |
| CMSG_QUERY_PET_NAME | 0x3275 | 12917 | 82 (0x52) | fixed, 0 bytes | matches |
| — | 0x3276 | 12918 | — | fixed, 0 bytes | — |
| CMSG_QUERY_PETITION | 0x3277 | 12919 | 454 (0x1c6) | fixed, 4 bytes | matches |
| CMSG_REQUEST_PLAYED_TIME | 0x327a | 12922 | 460 (0x1cc) | variable | matches |
| CMSG_SET_TITLE | 0x327e | 12926 | 884 (0x374) | fixed, 4 bytes | matches |
| CMSG_CANCEL_MOUNT_AURA | 0x327f | 12927 | 885 (0x375) | empty | matches |
| CMSG_MOUNT_SPECIAL_ANIM | 0x3280 | 12928 | 369 (0x171) | variable | matches |
| — | 0x3292 | 12946 | — | variable | — |
| CMSG_DESTROY_ITEM | 0x3293 | 12947 | 273 (0x111) | fixed, 6 bytes | matches |
| — | 0x3297 | 12951 | — | fixed, 4 bytes | — |
| CMSG_USE_ITEM | 0x3298 | 12952 | 171 (0xab) | variable | differs (#365) |
| CMSG_ADD_TOY | 0x3299 | 12953 | — | fixed, 0 bytes | matches |
| CMSG_USE_TOY | 0x329a | 12954 | — | variable | differs (#365) |
| CMSG_PET_CAST_SPELL | 0x329b | 12955 | 496 (0x1f0) | variable | differs (#365) |
| CMSG_CAST_SPELL | 0x329c | 12956 | 302 (0x12e) | variable | differs (#365) |
| — | 0x329d | 12957 | — | fixed, 8 bytes | — |
| — | 0x329e | 12958 | — | fixed, 8 bytes | — |
| CMSG_CANCEL_CAST | 0x329f | 12959 | 303 (0x12f) | fixed, 4 bytes | matches |
| — | 0x32a2 | 12962 | — | empty | — |
| CMSG_REQUEST_LFG_LIST_BLACKLIST | 0x32a4 | 12964 | — | empty | matches |
| CMSG_SAVE_GUILD_EMBLEM | 0x32a8 | 12968 | — | fixed, 20 bytes | matches |
| CMSG_TABARD_VENDOR_ACTIVATE | 0x32a9 | 12969 | — | fixed, 0 bytes | matches |
| CMSG_TOGGLE_PVP | 0x32ab | 12971 | 595 (0x253) | empty | matches |
| CMSG_SET_PVP | 0x32ac | 12972 | — | variable | matches |
| CMSG_BATTLEMASTER_HELLO | 0x32b1 | 12977 | 727 (0x2d7) | fixed, 0 bytes | matches |
| CMSG_REQUEST_CONQUEST_FORMULA_CONSTANTS | 0x32b4 | 12980 | — | empty | matches |
| — | 0x32b5 | 12981 | — | variable | — |
| CMSG_ITEM_TEXT_QUERY | 0x32c5 | 12997 | 579 (0x243) | fixed, 0 bytes | matches |
| CMSG_OPEN_ITEM | 0x32c6 | 12998 | 172 (0xac) | fixed, 2 bytes | matches |
| CMSG_READ_ITEM | 0x32c7 | 12999 | 173 (0xad) | fixed, 2 bytes | matches |
| — | 0x32c8 | 13000 | — | variable | — |
| — | 0x32c9 | 13001 | — | variable | — |
| — | 0x32ca | 13002 | — | variable | — |
| — | 0x32d9 | 13017 | — | fixed, 4 bytes | — |
| — | 0x32dc | 13020 | — | fixed, 8 bytes | — |
| — | 0x32dd | 13021 | — | empty | — |
| — | 0x32de | 13022 | — | empty | — |
| — | 0x32df | 13023 | — | fixed, 0 bytes | — |
| — | 0x32e0 | 13024 | — | fixed, 0 bytes | — |
| — | 0x32f0 | 13040 | — | variable | — |
| — | 0x32f1 | 13041 | — | variable | — |
| — | 0x32f2 | 13042 | — | fixed, 10 bytes | — |
| — | 0x32f3 | 13043 | — | fixed, 4 bytes | — |
| — | 0x32f4 | 13044 | — | fixed, 5 bytes | — |
| — | 0x32fb | 13051 | — | empty | — |
| CMSG_OFFER_PETITION | 0x32fd | 13053 | 451 (0x1c3) | fixed, 4 bytes | matches |
| CMSG_REMOVE_GLYPH | 0x3300 | 13056 | 1162 (0x48a) | fixed, 1 bytes | matches |
| — | 0x3302 | 13058 | — | fixed, 1 bytes | — |
| — | 0x3303 | 13059 | — | variable | — |
| — | 0x330e | 13070 | — | empty | — |
| CMSG_SEND_TEXT_EMOTE | 0x3488 | 13448 | 260 (0x104) | variable | matches |
| CMSG_SET_SHEATHED | 0x3489 | 13449 | 480 (0x1e0) | variable | matches |
| CMSG_PET_SET_ACTION | 0x348a | 13450 | 372 (0x174) | variable | matches; see #369 |
| CMSG_PET_ACTION | 0x348b | 13451 | 373 (0x175) | fixed, 16 bytes | matches |
| CMSG_PET_STOP_ATTACK | 0x348c | 13452 | 746 (0x2ea) | fixed, 0 bytes | matches |
| CMSG_PET_ABANDON | 0x348d | 13453 | 374 (0x176) | fixed, 0 bytes | matches |
| CMSG_PET_CANCEL_AURA | 0x348e | 13454 | 619 (0x26b) | fixed, 4 bytes | matches |
| — | 0x348f | 13455 | — | variable | — |
| CMSG_REQUEST_PET_INFO | 0x3490 | 13456 | 633 (0x279) | empty | matches |
| CMSG_REQUEST_STABLED_PETS | 0x3491 | 13457 | — | fixed, 0 bytes | matches |
| CMSG_TALK_TO_GOSSIP | 0x3492 | 13458 | 379 (0x17b) | fixed, 0 bytes | matches |
| CMSG_CLOSE_INTERACTION | 0x3493 | 13459 | — | fixed, 0 bytes | matches |
| CMSG_GOSSIP_SELECT_OPTION | 0x3494 | 13460 | 380 (0x17c) | variable | matches |
| CMSG_SPELL_CLICK | 0x3495 | 13461 | 1016 (0x3f8) | variable | matches; see #369 |
| CMSG_QUEST_GIVER_HELLO | 0x3496 | 13462 | 388 (0x184) | fixed, 0 bytes | matches |
| CMSG_QUEST_GIVER_QUERY_QUEST | 0x3497 | 13463 | 390 (0x186) | variable | matches |
| CMSG_QUEST_GIVER_ACCEPT_QUEST | 0x3498 | 13464 | 393 (0x189) | variable | matches |
| CMSG_QUEST_GIVER_COMPLETE_QUEST | 0x3499 | 13465 | 394 (0x18a) | variable | matches |
| CMSG_QUEST_GIVER_CHOOSE_REWARD | 0x349a | 13466 | 398 (0x18e) | variable | matches |
| CMSG_QUEST_GIVER_REQUEST_REWARD | 0x349b | 13467 | 396 (0x18c) | fixed, 4 bytes | matches |
| CMSG_QUEST_GIVER_STATUS_QUERY | 0x349c | 13468 | 386 (0x182) | fixed, 0 bytes | matches |
| CMSG_QUEST_GIVER_STATUS_MULTIPLE_QUERY | 0x349d | 13469 | 1047 (0x417) | empty | matches |
| CMSG_QUEST_CONFIRM_ACCEPT | 0x349e | 13470 | 411 (0x19b) | fixed, 4 bytes | matches |
| CMSG_PUSH_QUEST_TO_PARTY | 0x349f | 13471 | 413 (0x19d) | fixed, 4 bytes | matches |
| CMSG_QUEST_PUSH_RESULT | 0x34a0 | 13472 | — | fixed, 5 bytes | matches |
| CMSG_LIST_INVENTORY | 0x34a1 | 13473 | 414 (0x19e) | fixed, 0 bytes | matches |
| CMSG_SELL_ITEM | 0x34a2 | 13474 | 416 (0x1a0) | fixed, 4 bytes | matches |
| CMSG_BUY_ITEM | 0x34a3 | 13475 | 418 (0x1a2) | variable | matches |
| CMSG_BUY_BACK_ITEM | 0x34a4 | 13476 | 656 (0x290) | fixed, 4 bytes | matches |
| CMSG_TAXI_NODE_STATUS_QUERY | 0x34a8 | 13480 | 426 (0x1aa) | fixed, 0 bytes | matches |
| CMSG_ENABLE_TAXI_NODE | 0x34a9 | 13481 | 1171 (0x493) | fixed, 0 bytes | matches |
| CMSG_TAXI_QUERY_AVAILABLE_NODES | 0x34aa | 13482 | 428 (0x1ac) | fixed, 0 bytes | matches |
| CMSG_ACTIVATE_TAXI | 0x34ab | 13483 | 429 (0x1ad) | fixed, 12 bytes | matches |
| — | 0x34ac | 13484 | — | empty | — |
| CMSG_TRAINER_LIST | 0x34ad | 13485 | 432 (0x1b0) | fixed, 0 bytes | matches |
| CMSG_TRAINER_BUY_SPELL | 0x34ae | 13486 | 434 (0x1b2) | fixed, 8 bytes | matches |
| CMSG_SPIRIT_HEALER_ACTIVATE | 0x34af | 13487 | 540 (0x21c) | fixed, 0 bytes | matches |
| CMSG_AREA_SPIRIT_HEALER_QUERY | 0x34b0 | 13488 | 738 (0x2e2) | fixed, 0 bytes | matches |
| CMSG_AREA_SPIRIT_HEALER_QUEUE | 0x34b1 | 13489 | 739 (0x2e3) | fixed, 0 bytes | matches |
| CMSG_BINDER_ACTIVATE | 0x34b2 | 13490 | 437 (0x1b5) | fixed, 0 bytes | matches |
| CMSG_BANKER_ACTIVATE | 0x34b3 | 13491 | 439 (0x1b7) | fixed, 0 bytes | matches |
| CMSG_BUY_BANK_SLOT | 0x34b4 | 13492 | 441 (0x1b9) | fixed, 0 bytes | matches |
| CMSG_GUILD_BANK_ACTIVATE | 0x34b5 | 13493 | 998 (0x3e6) | variable | matches |
| CMSG_AUTO_GUILD_BANK_ITEM | 0x34b6 | 13494 | — | variable | matches |
| CMSG_STORE_GUILD_BANK_ITEM | 0x34b7 | 13495 | — | variable | matches |
| CMSG_SWAP_ITEM_WITH_GUILD_BANK_ITEM | 0x34b8 | 13496 | — | variable | matches |
| CMSG_SWAP_GUILD_BANK_ITEM_WITH_GUILD_BANK_ITEM | 0x34b9 | 13497 | — | variable | no handler |
| CMSG_MOVE_GUILD_BANK_ITEM | 0x34ba | 13498 | — | fixed, 4 bytes | matches |
| CMSG_MERGE_ITEM_WITH_GUILD_BANK_ITEM | 0x34bb | 13499 | — | variable | matches |
| CMSG_SPLIT_ITEM_TO_GUILD_BANK | 0x34bc | 13500 | — | variable | matches |
| CMSG_MERGE_GUILD_BANK_ITEM_WITH_ITEM | 0x34bd | 13501 | — | variable | matches |
| CMSG_SPLIT_GUILD_BANK_ITEM_TO_INVENTORY | 0x34be | 13502 | — | variable | matches |
| CMSG_AUTO_STORE_GUILD_BANK_ITEM | 0x34bf | 13503 | — | fixed, 2 bytes | matches |
| CMSG_MERGE_GUILD_BANK_ITEM_WITH_GUILD_BANK_ITEM | 0x34c0 | 13504 | — | fixed, 8 bytes | matches |
| CMSG_SPLIT_GUILD_BANK_ITEM | 0x34c1 | 13505 | — | fixed, 8 bytes | matches |
| CMSG_GUILD_BANK_QUERY_TAB | 0x34c2 | 13506 | 999 (0x3e7) | variable | matches |
| CMSG_GUILD_BANK_BUY_TAB | 0x34c3 | 13507 | 1002 (0x3ea) | fixed, 1 bytes | matches |
| CMSG_GUILD_BANK_UPDATE_TAB | 0x34c4 | 13508 | 1003 (0x3eb) | variable | matches |
| CMSG_GUILD_BANK_DEPOSIT_MONEY | 0x34c5 | 13509 | 1004 (0x3ec) | fixed, 8 bytes | matches |
| CMSG_GUILD_BANK_WITHDRAW_MONEY | 0x34c6 | 13510 | 1005 (0x3ed) | fixed, 8 bytes | matches |
| — | 0x34c7 | 13511 | — | fixed, 0 bytes | — |
| CMSG_PETITION_BUY | 0x34c8 | 13512 | 445 (0x1bd) | variable | matches |
| CMSG_PETITION_SHOW_SIGNATURES | 0x34c9 | 13513 | 446 (0x1be) | fixed, 0 bytes | matches |
| CMSG_AUCTION_HELLO_REQUEST | 0x34ca | 13514 | — | fixed, 0 bytes | matches |
| CMSG_AUCTION_SELL_ITEM | 0x34cb | 13515 | 598 (0x256) | variable | matches |
| CMSG_AUCTION_REMOVE_ITEM | 0x34cc | 13516 | 599 (0x257) | variable | matches |
| CMSG_AUCTION_LIST_ITEMS | 0x34cd | 13517 | 600 (0x258) | variable | differs |
| — | 0x34ce | 13518 | — | variable | — |
| CMSG_AUCTION_LIST_OWNED_ITEMS | 0x34cf | 13519 | 601 (0x259) | variable | matches; see #369 |
| CMSG_AUCTION_LIST_BIDDED_ITEMS | 0x34d0 | 13520 | 612 (0x264) | variable | differs (#367) |
| CMSG_AUCTION_PLACE_BID | 0x34d1 | 13521 | 602 (0x25a) | variable | matches |
| CMSG_AUCTION_LIST_PENDING_SALES | 0x34d2 | 13522 | 1167 (0x48f) | empty | no handler |
| CMSG_QUERY_TIME | 0x34d5 | 13525 | 462 (0x1ce) | empty | matches |
| CMSG_LOGOUT_REQUEST | 0x34d6 | 13526 | 75 (0x4b) | variable | matches |
| CMSG_LOGOUT_CANCEL | 0x34d8 | 13528 | 78 (0x4e) | empty | matches |
| — | 0x34d9 | 13529 | — | empty | — |
| CMSG_RECLAIM_CORPSE | 0x34db | 13531 | 466 (0x1d2) | fixed, 0 bytes | matches |
| — | 0x34dd | 13533 | — | empty | — |
| CMSG_SET_FACTION_AT_WAR | 0x34de | 13534 | 293 (0x125) | fixed, 1 bytes | matches |
| CMSG_SET_FACTION_NOT_AT_WAR | 0x34df | 13535 | — | fixed, 1 bytes | matches |
| CMSG_SET_FACTION_INACTIVE | 0x34e0 | 13536 | 791 (0x317) | variable | matches |
| CMSG_SET_WATCHED_FACTION | 0x34e1 | 13537 | 792 (0x318) | fixed, 4 bytes | matches |
| CMSG_DUEL_RESPONSE | 0x34e2 | 13538 | — | variable | matches |
| CMSG_UNLEARN_SKILL | 0x34e5 | 13541 | 514 (0x202) | fixed, 4 bytes | matches |
| CMSG_CANCEL_AUTO_REPEAT_SPELL | 0x34e7 | 13543 | 621 (0x26d) | empty | matches |
| CMSG_FAR_SIGHT | 0x34e8 | 13544 | 634 (0x27a) | variable | matches |
| CMSG_SOCKET_GEMS | 0x34eb | 13547 | 839 (0x347) | variable | matches |
| CMSG_REPAIR_ITEM | 0x34ec | 13548 | 680 (0x2a8) | variable | matches |
| CMSG_GAME_OBJ_USE | 0x34ee | 13550 | 177 (0xb1) | fixed, 0 bytes | matches |
| CMSG_GAME_OBJ_REPORT_USE | 0x34ef | 13551 | 1153 (0x481) | fixed, 0 bytes | matches |
| CMSG_CANCEL_TEMP_ENCHANTMENT | 0x34f2 | 13554 | 889 (0x379) | fixed, 4 bytes | matches |
| — | 0x34f3 | 13555 | — | variable | — |
| — | 0x34f4 | 13556 | — | fixed, 0 bytes | — |
| CMSG_ALTER_APPEARANCE | 0x34f5 | 13557 | 1062 (0x426) | variable | matches |
| CMSG_OPT_OUT_OF_LOOT | 0x34f6 | 13558 | 1033 (0x409) | variable | matches |
| CMSG_TOTEM_DESTROYED | 0x34f8 | 13560 | 1044 (0x414) | fixed, 1 bytes | matches |
| CMSG_DISMISS_CRITTER | 0x34f9 | 13561 | 1165 (0x48d) | fixed, 0 bytes | matches |
| — | 0x3500 | 13568 | — | fixed, 0 bytes | — |
| CMSG_HEARTH_AND_RESURRECT | 0x3506 | 13574 | 1180 (0x49c) | empty | no handler |
| CMSG_SAVE_EQUIPMENT_SET | 0x3509 | 13577 | 1213 (0x4bd) | variable | matches |
| CMSG_DELETE_EQUIPMENT_SET | 0x350a | 13578 | — | fixed, 8 bytes | matches |
| CMSG_INSTANCE_LOCK_RESPONSE | 0x350b | 13579 | 319 (0x13f) | variable | matches |
| — | 0x3512 | 13586 | — | variable | — |
| CMSG_DECLINE_GUILD_INVITES | 0x351d | 13597 | — | variable | differs |
| CMSG_OVERRIDE_SCREEN_FLASH | 0x351e | 13598 | — | variable | no handler |
| CMSG_BATTLEMASTER_JOIN | 0x3520 | 13600 | 750 (0x2ee) | variable | matches |
| CMSG_BATTLEMASTER_JOIN_ARENA | 0x3521 | 13601 | 856 (0x358) | fixed, 2 bytes | matches |
| CMSG_BATTLEMASTER_JOIN_SKIRMISH | 0x3522 | 13602 | — | variable | matches |
| CMSG_BATTLEFIELD_PORT | 0x3525 | 13605 | 725 (0x2d5) | variable | matches |
| CMSG_REPOP_REQUEST | 0x3526 | 13606 | 346 (0x15a) | variable | matches |
| CMSG_SET_SELECTION | 0x3528 | 13608 | 317 (0x13d) | fixed, 0 bytes | matches |
| CMSG_INSPECT | 0x3529 | 13609 | 276 (0x114) | fixed, 0 bytes | matches |
| CMSG_REQUEST_CROWD_CONTROL_SPELL | 0x352a | 13610 | — | fixed, 0 bytes | no handler |
| — | 0x352b | 13611 | — | fixed, 0 bytes | — |
| CMSG_QUEST_LOG_REMOVE_QUEST | 0x352e | 13614 | 404 (0x194) | fixed, 1 bytes | matches |
| CMSG_GET_ITEM_PURCHASE_DATA | 0x352f | 13615 | 1203 (0x4b3) | fixed, 0 bytes | no handler |
| — | 0x3530 | 13616 | — | fixed, 0 bytes | — |
| CMSG_SELF_RES | 0x3531 | 13617 | 691 (0x2b3) | fixed, 4 bytes | matches |
| CMSG_SET_ACTION_BAR_TOGGLES | 0x3532 | 13618 | 703 (0x2bf) | fixed, 1 bytes | matches |
| CMSG_SIGN_PETITION | 0x3533 | 13619 | 448 (0x1c0) | fixed, 1 bytes | matches |
| CMSG_DECLINE_PETITION | 0x3534 | 13620 | — | fixed, 0 bytes | matches |
| CMSG_TURN_IN_PETITION | 0x3535 | 13621 | 452 (0x1c4) | fixed, 20 bytes | reads the first part |
| CMSG_MAIL_GET_LIST | 0x3536 | 13622 | 570 (0x23a) | fixed, 0 bytes | matches |
| CMSG_MAIL_TAKE_MONEY | 0x3537 | 13623 | 581 (0x245) | fixed, 16 bytes | matches |
| CMSG_MAIL_TAKE_ITEM | 0x3538 | 13624 | 582 (0x246) | fixed, 16 bytes | matches |
| CMSG_QUERY_NEXT_MAIL_TIME | 0x3539 | 13625 | — | empty | matches |
| CMSG_MAIL_MARK_AS_READ | 0x353a | 13626 | 583 (0x247) | fixed, 8 bytes | matches |
| CMSG_MAIL_CREATE_TEXT_ITEM | 0x353b | 13627 | 586 (0x24a) | fixed, 8 bytes | matches |
| CMSG_EMOTE | 0x3541 | 13633 | 258 (0x102) | empty | no handler (#362) |
| CMSG_OPENING_CINEMATIC | 0x3543 | 13635 | 249 (0xf9) | empty | matches |
| CMSG_NEXT_CINEMATIC_CAMERA | 0x3544 | 13636 | 251 (0xfb) | empty | matches |
| CMSG_COMPLETE_CINEMATIC | 0x3545 | 13637 | 252 (0xfc) | empty | matches |
| — | 0x3546 | 13638 | — | fixed, 4 bytes | — |
| CMSG_QUEST_GIVER_CLOSE_QUEST | 0x3549 | 13641 | — | fixed, 4 bytes | matches |
| CMSG_LEARN_TALENT | 0x3552 | 13650 | 593 (0x251) | fixed, 6 bytes | matches |
| — | 0x3553 | 13651 | — | variable | — |
| CMSG_PET_LEARN_TALENT | 0x3554 | 13652 | 1146 (0x47a) | fixed, 6 bytes | matches |
| — | 0x3555 | 13653 | — | variable | — |
| — | 0x355b | 13659 | — | fixed, 8 bytes | — |
| CMSG_SET_ACTION_BUTTON | 0x355d | 13661 | 296 (0x128) | fixed, 5 bytes | differs |
| CMSG_SET_AMMO | 0x355e | 13662 | 616 (0x268) | fixed, 4 bytes | matches |
| CMSG_PLAYER_SHOWING_HELM | 0x3568 | 13672 | 697 (0x2b9) | variable | matches |
| CMSG_PLAYER_SHOWING_CLOAK | 0x3569 | 13673 | 698 (0x2ba) | variable | matches |
| — | 0x356b | 13675 | — | variable | — |
| — | 0x35d4 | 13780 | — | fixed, 5 bytes | — |
| CMSG_ADDON_LIST | 0x35d8 | 13784 | — | variable | no handler |
| CMSG_SET_ROLE | 0x35d9 | 13785 | — | variable | differs |
| — | 0x35da | 13786 | — | variable | — |
| CMSG_REQUEST_BATTLEFIELD_STATUS | 0x35dd | 13789 | — | empty | matches |
| — | 0x35df | 13791 | — | variable | — |
| — | 0x35e0 | 13792 | — | variable | — |
| — | 0x35e1 | 13793 | — | variable | — |
| CMSG_REQUEST_RATED_PVP_INFO | 0x35e4 | 13796 | — | empty | matches |
| CMSG_DB_QUERY_BULK | 0x35e5 | 13797 | — | variable | matches |
| CMSG_HOTFIX_REQUEST | 0x35e6 | 13798 | — | variable | matches |
| CMSG_GENERATE_RANDOM_CHARACTER_NAME | 0x35e8 | 13800 | — | fixed, 2 bytes | matches |
| CMSG_ENUM_CHARACTERS | 0x35e9 | 13801 | 55 (0x37) | empty | matches |
| CMSG_REORDER_CHARACTERS | 0x35ea | 13802 | — | variable | matches |
| CMSG_PLAYER_LOGIN | 0x35eb | 13803 | 61 (0x3d) | fixed, 4 bytes | matches |
| — | 0x35ed | 13805 | — | variable | — |
| — | 0x35f0 | 13808 | — | variable | — |
| — | 0x35f1 | 13809 | — | fixed, 4 bytes | — |
| — | 0x35f2 | 13810 | — | variable | — |
| — | 0x35f3 | 13811 | — | fixed, 11 bytes | — |
| — | 0x35f4 | 13812 | — | variable | — |
| — | 0x35f5 | 13813 | — | fixed, 23 bytes | — |
| — | 0x35f6 | 13814 | — | empty | — |
| CMSG_REQUEST_PARTY_JOIN_UPDATES | 0x35f8 | 13816 | — | variable | no handler |
| CMSG_LOADING_SCREEN_NOTIFY | 0x35f9 | 13817 | — | variable | matches |
| CMSG_WORLD_PORT_RESPONSE | 0x35fa | 13818 | — | empty | matches |
| CMSG_SEND_MAIL | 0x35fb | 13819 | 568 (0x238) | variable | matches |
| CMSG_ACCEPT_GUILD_INVITE | 0x35fe | 13822 | 132 (0x84) | empty | matches |
| CMSG_PARTY_INVITE | 0x3604 | 13828 | 110 (0x6e) | variable | differs (#368) |
| CMSG_PARTY_INVITE_RESPONSE | 0x3606 | 13830 | — | variable | matches |
| CMSG_GUILD_INVITE_BY_NAME | 0x3608 | 13832 | 130 (0x82) | variable | matches |
| CMSG_DF_PROPOSAL_RESPONSE | 0x3609 | 13833 | — | variable | matches |
| CMSG_DF_JOIN | 0x360b | 13835 | — | variable | matches |
| — | 0x360c | 13836 | — | variable | — |
| CMSG_LFG_LIST_GET_STATUS | 0x360d | 13837 | — | empty | matches |
| — | 0x360e | 13838 | — | variable | — |
| — | 0x360f | 13839 | — | variable | — |
| — | 0x3610 | 13840 | — | variable | — |
| — | 0x3611 | 13841 | — | variable | — |
| — | 0x3612 | 13842 | — | variable | — |
| — | 0x3613 | 13843 | — | variable | — |
| CMSG_DF_LEAVE | 0x3614 | 13844 | — | variable | reads the first part |
| CMSG_DF_GET_SYSTEM_INFO | 0x3615 | 13845 | — | variable | differs |
| CMSG_DF_GET_JOIN_STATUS | 0x3616 | 13846 | 662 (0x296) | empty | matches |
| CMSG_DF_SET_ROLES | 0x3617 | 13847 | — | variable | matches; see #369 |
| — | 0x3618 | 13848 | — | variable | — |
| CMSG_DF_TELEPORT | 0x3619 | 13849 | — | variable | matches |
| CMSG_SET_EVERYONE_IS_ASSISTANT | 0x361a | 13850 | — | variable | matches |
| — | 0x361c | 13852 | — | variable | — |
| CMSG_BATTLE_PET_REQUEST_JOURNAL_LOCK | 0x3624 | 13860 | — | empty | no handler |
| CMSG_BATTLE_PET_REQUEST_JOURNAL | 0x3625 | 13861 | — | empty | matches |
| CMSG_BATTLE_PET_SUMMON | 0x362a | 13866 | — | fixed, 0 bytes | matches |
| — | 0x362e | 13870 | — | fixed, 1 bytes | — |
| CMSG_BATTLE_PET_SET_FLAGS | 0x3631 | 13873 | — | variable | matches |
| CMSG_MOUNT_SET_FAVORITE | 0x3633 | 13875 | — | variable | matches |
| CMSG_COLLECTION_ITEM_SET_FAVORITE | 0x3634 | 13876 | — | variable | matches |
| CMSG_DO_READY_CHECK | 0x3635 | 13877 | — | variable | matches |
| CMSG_READY_CHECK_RESPONSE | 0x3636 | 13878 | — | variable | matches |
| CMSG_CREATE_CHARACTER | 0x3645 | 13893 | 54 (0x36) | variable | matches |
| CMSG_SUPPORT_TICKET_SUBMIT_COMPLAINT | 0x3647 | 13895 | — | variable | differs |
| CMSG_SUPPORT_TICKET_SUBMIT_BUG | 0x3648 | 13896 | — | variable | no handler |
| CMSG_SUPPORT_TICKET_SUBMIT_SUGGESTION | 0x3649 | 13897 | — | variable | no handler |
| CMSG_PARTY_UNINVITE | 0x364a | 13898 | — | variable | differs |
| CMSG_SET_LOOT_METHOD | 0x364b | 13899 | 122 (0x7a) | variable | differs (#368) |
| CMSG_LEAVE_GROUP | 0x364c | 13900 | — | variable | differs (#368) |
| CMSG_SET_PARTY_LEADER | 0x364d | 13901 | 120 (0x78) | variable | differs (#368) |
| CMSG_MINIMAP_PING | 0x364e | 13902 | — | variable | matches |
| CMSG_GROUP_CHANGE_SUB_GROUP | 0x364f | 13903 | 638 (0x27e) | variable | matches |
| CMSG_GROUP_SWAP_SUB_GROUP | 0x3650 | 13904 | 640 (0x280) | variable | differs |
| CMSG_CONVERT_RAID | 0x3651 | 13905 | 654 (0x28e) | variable | matches |
| CMSG_SET_ASSISTANT_LEADER | 0x3652 | 13906 | 655 (0x28f) | variable | matches |
| CMSG_UPDATE_RAID_TARGET | 0x3653 | 13907 | — | variable | differs (#368) |
| — | 0x3654 | 13908 | — | variable | — |
| — | 0x3655 | 13909 | — | variable | — |
| CMSG_REQUEST_PARTY_MEMBER_STATS | 0x3656 | 13910 | 639 (0x27f) | variable | differs (#368) |
| CMSG_RANDOM_ROLL | 0x3657 | 13911 | — | variable | matches |
| CMSG_MAIL_RETURN_TO_SENDER | 0x3658 | 13912 | 584 (0x248) | fixed, 8 bytes | matches |
| — | 0x3659 | 13913 | — | empty | — |
| — | 0x365c | 13916 | — | variable | — |
| CMSG_QUERY_CORPSE_LOCATION_FROM_CLIENT | 0x3662 | 13922 | — | fixed, 0 bytes | matches |
| — | 0x3663 | 13923 | — | fixed, 0 bytes | — |
| CMSG_CAN_DUEL | 0x3664 | 13924 | — | variable | matches; see #369 |
| — | 0x3666 | 13926 | — | fixed, 4 bytes | — |
| CMSG_RESET_INSTANCES | 0x366a | 13930 | 797 (0x31d) | empty | matches |
| CMSG_SUMMON_RESPONSE | 0x366c | 13932 | 684 (0x2ac) | variable | matches |
| CMSG_COMPLAINT | 0x366e | 13934 | 967 (0x3c7) | variable | no handler |
| — | 0x3671 | 13937 | — | empty | — |
| — | 0x3672 | 13938 | — | fixed, 8 bytes | — |
| — | 0x3673 | 13939 | — | fixed, 11 bytes | — |
| — | 0x3674 | 13940 | — | variable | — |
| — | 0x3675 | 13941 | — | fixed, 24 bytes | — |
| — | 0x3676 | 13942 | — | fixed, 17 bytes | — |
| — | 0x3677 | 13943 | — | fixed, 25 bytes | — |
| — | 0x3678 | 13944 | — | fixed, 25 bytes | — |
| — | 0x3679 | 13945 | — | fixed, 28 bytes | — |
| — | 0x367a | 13946 | — | fixed, 28 bytes | — |
| — | 0x367b | 13947 | — | fixed, 16 bytes | — |
| CMSG_CALENDAR_GET_NUM_PENDING | 0x367c | 13948 | 1095 (0x447) | empty | no handler |
| — | 0x367d | 13949 | — | variable | — |
| — | 0x367f | 13951 | — | variable | — |
| — | 0x3680 | 13952 | — | variable | — |
| CMSG_KEEP_ALIVE | 0x3681 | 13953 | 1031 (0x407) | empty | no handler |
| — | 0x3682 | 13954 | — | variable | — |
| CMSG_WHO | 0x3683 | 13955 | 98 (0x62) | variable | reads the first part |
| CMSG_SET_DUNGEON_DIFFICULTY | 0x3684 | 13956 | — | fixed, 4 bytes | matches |
| CMSG_RESURRECT_RESPONSE | 0x3685 | 13957 | 348 (0x15c) | fixed, 4 bytes | matches |
| CMSG_PET_RENAME | 0x3686 | 13958 | 375 (0x177) | variable | matches |
| CMSG_BUG | 0x3687 | 13959 | 458 (0x1ca) | variable | no handler |
| CMSG_SET_PLAYER_DECLINED_NAMES | 0x3689 | 13961 | 1049 (0x419) | variable | matches |
| — | 0x368a | 13962 | — | fixed, 4 bytes | — |
| CMSG_QUERY_GUILD_INFO | 0x368b | 13963 | 84 (0x54) | fixed, 0 bytes | matches |
| — | 0x368c | 13964 | — | variable | — |
| CMSG_GM_TICKET_GET_SYSTEM_STATUS | 0x368e | 13966 | 538 (0x21a) | empty | matches |
| CMSG_GM_TICKET_GET_CASE_STATUS | 0x368f | 13967 | — | empty | matches |
| CMSG_GM_TICKET_ACKNOWLEDGE_SURVEY | 0x3690 | 13968 | — | fixed, 4 bytes | no handler |
| — | 0x3692 | 13970 | — | variable | — |
| CMSG_SUBMIT_USER_FEEDBACK | 0x3693 | 13971 | — | variable | no handler |
| CMSG_REQUEST_ACCOUNT_DATA | 0x3694 | 13972 | 522 (0x20a) | variable | matches |
| CMSG_UPDATE_ACCOUNT_DATA | 0x3695 | 13973 | 523 (0x20b) | variable | matches |
| CMSG_SERVER_TIME_OFFSET_REQUEST | 0x369c | 13980 | — | empty | no handler |
| CMSG_CHAR_DELETE | 0x369d | 13981 | 56 (0x38) | fixed, 0 bytes | matches |
| — | 0x36a1 | 13985 | — | variable | — |
| CMSG_INSPECT_PVP | 0x36a3 | 13987 | — | fixed, 0 bytes | matches |
| CMSG_QUERY_ARENA_TEAM | 0x36a4 | 13988 | — | fixed, 4 bytes | no handler |
| — | 0x36aa | 13994 | — | variable | — |
| CMSG_QUEST_POI_QUERY | 0x36b2 | 14002 | 483 (0x1e3) | variable | matches |
| CMSG_ARENA_TEAM_ROSTER | 0x36b8 | 14008 | 845 (0x34d) | fixed, 4 bytes | matches |
| CMSG_ARENA_TEAM_ACCEPT | 0x36b9 | 14009 | 849 (0x351) | fixed, 0 bytes | matches |
| CMSG_ARENA_TEAM_DECLINE | 0x36ba | 14010 | 850 (0x352) | fixed, 0 bytes | matches |
| CMSG_ARENA_TEAM_LEAVE | 0x36bb | 14011 | 851 (0x353) | fixed, 4 bytes | matches |
| CMSG_ARENA_TEAM_REMOVE | 0x36bc | 14012 | 852 (0x354) | fixed, 4 bytes | matches |
| CMSG_ARENA_TEAM_DISBAND | 0x36bd | 14013 | 853 (0x355) | fixed, 4 bytes | matches |
| CMSG_ARENA_TEAM_LEADER | 0x36be | 14014 | 854 (0x356) | fixed, 4 bytes | matches |
| CMSG_GET_ACCOUNT_CHARACTER_LIST | 0x36bf | 14015 | — | variable | matches; see #369 |
| — | 0x36c0 | 14016 | — | variable | — |
| — | 0x36c1 | 14017 | — | variable | — |
| — | 0x36c2 | 14018 | — | variable | — |
| CMSG_BATTLE_PAY_GET_PRODUCT_LIST | 0x36c4 | 14020 | — | empty | no handler |
| CMSG_BATTLE_PAY_GET_PURCHASE_LIST | 0x36c5 | 14021 | — | empty | no handler |
| CMSG_CHARACTER_RENAME_REQUEST | 0x36c9 | 14025 | 711 (0x2c7) | variable | matches |
| — | 0x36ca | 14026 | — | fixed, 8 bytes | — |
| — | 0x36cb | 14027 | — | fixed, 16 bytes | — |
| — | 0x36cc | 14028 | — | fixed, 0 bytes | — |
| — | 0x36cd | 14029 | — | fixed, 4 bytes | — |
| — | 0x36ce | 14030 | — | empty | — |
| CMSG_GUILD_SET_GUILD_MASTER | 0x36d0 | 14032 | 144 (0x90) | variable | matches |
| CMSG_PETITION_RENAME_GUILD | 0x36d1 | 14033 | — | variable | matches |
| CMSG_REQUEST_RAID_INFO | 0x36d2 | 14034 | 717 (0x2cd) | empty | matches |
| — | 0x36d3 | 14035 | — | variable | — |
| — | 0x36d4 | 14036 | — | variable | — |
| — | 0x36d5 | 14037 | — | fixed, 4 bytes | — |
| CMSG_CONTACT_LIST | 0x36d7 | 14039 | 102 (0x66) | fixed, 4 bytes | matches |
| CMSG_ADD_FRIEND | 0x36d8 | 14040 | 105 (0x69) | variable | matches |
| CMSG_DEL_FRIEND | 0x36d9 | 14041 | 106 (0x6a) | fixed, 4 bytes | matches |
| CMSG_SET_CONTACT_NOTES | 0x36da | 14042 | 107 (0x6b) | variable | matches |
| — | 0x36db | 14043 | — | variable | — |
| CMSG_ADD_IGNORE | 0x36dc | 14044 | 108 (0x6c) | variable | matches |
| CMSG_DEL_IGNORE | 0x36dd | 14045 | 109 (0x6d) | fixed, 4 bytes | matches |
| CMSG_SET_RAID_DIFFICULTY | 0x36e3 | 14051 | — | fixed, 5 bytes | reads the first part |
| CMSG_TUTORIAL_FLAG | 0x36e4 | 14052 | 254 (0xfe) | variable | matches |
| — | 0x36e5 | 14053 | — | empty | — |
| — | 0x36e6 | 14054 | — | fixed, 4 bytes | — |
| CMSG_GET_UNDELETE_CHARACTER_COOLDOWN_STATUS | 0x36e7 | 14055 | — | empty | no handler |
| — | 0x36eb | 14059 | — | variable | — |
| — | 0x36ec | 14060 | — | fixed, 4 bytes | — |
| — | 0x36ed | 14061 | — | fixed, 4 bytes | — |
| — | 0x36ee | 14062 | — | fixed, 20 bytes | — |
| — | 0x36ef | 14063 | — | variable | — |
| — | 0x36f0 | 14064 | — | fixed, 4 bytes | — |
| — | 0x36f1 | 14065 | — | fixed, 12 bytes | — |
| — | 0x36f2 | 14066 | — | variable | — |
| — | 0x36f3 | 14067 | — | fixed, 4 bytes | — |
| — | 0x36f4 | 14068 | — | fixed, 16 bytes | — |
| — | 0x36f5 | 14069 | — | variable | — |
| CMSG_COMMERCE_TOKEN_GET_LOG | 0x36f6 | 14070 | — | fixed, 4 bytes | no handler |
| — | 0x36f8 | 14072 | — | fixed, 8 bytes | — |
| — | 0x36f9 | 14073 | — | fixed, 8 bytes | — |
| — | 0x36fa | 14074 | — | variable | — |
| CMSG_UPDATE_VAS_PURCHASE_STATES | 0x36fb | 14075 | — | empty | no handler |
| CMSG_BATTLENET_REQUEST | 0x36fd | 14077 | — | variable | matches |
| — | 0x36ff | 14079 | — | variable | — |
| CMSG_CHANGE_REALM_TICKET | 0x3701 | 14081 | — | variable | matches |
| CMSG_REPORT_ENABLED_ADDONS | 0x3706 | 14086 | — | variable | no handler |
| CMSG_REPORT_CLIENT_VARIABLES | 0x3707 | 14087 | — | variable | no handler |
| CMSG_REPORT_KEYBINDING_EXECUTION_COUNTS | 0x3708 | 14088 | — | variable | no handler |
| — | 0x370b | 14091 | — | variable | — |
| — | 0x370c | 14092 | — | variable | — |
| — | 0x370d | 14093 | — | variable | — |
| — | 0x370f | 14095 | — | fixed, 4 bytes | — |
| — | 0x3710 | 14096 | — | fixed, 8 bytes | — |
| — | 0x3711 | 14097 | — | empty | — |
| — | 0x3712 | 14098 | — | fixed, 12 bytes | — |
| — | 0x3713 | 14099 | — | variable | — |
| — | 0x3714 | 14100 | — | fixed, 4 bytes | — |
| — | 0x3716 | 14102 | — | empty | — |
| — | 0x3717 | 14103 | — | variable | — |
| — | 0x3718 | 14104 | — | fixed, 1 bytes | — |
| — | 0x371a | 14106 | — | variable | — |
| — | 0x371b | 14107 | — | variable | — |
| CMSG_GET_ACCOUNT_NOTIFICATIONS | 0x373c | 14140 | — | empty | no handler |
| — | 0x373d | 14141 | — | fixed, 16 bytes | — |
| — | 0x3741 | 14145 | — | variable | — |
| — | 0x3742 | 14146 | — | variable | — |
| CMSG_SOCIAL_CONTRACT_REQUEST | 0x374c | 14156 | — | empty | no handler |
| — | 0x374d | 14157 | — | empty | — |
| — | 0x3751 | 14161 | — | variable | — |
| — | 0x3764 | 14180 | — | fixed, 8 bytes | — |
| CMSG_AUTH_SESSION | 0x3765 | 14181 | 493 (0x1ed) | variable | no handler |
| CMSG_AUTH_CONTINUED_SESSION | 0x3766 | 14182 | 1298 (0x512) | variable | no handler |
| CMSG_ENTER_ENCRYPTED_MODE_ACK | 0x3767 | 14183 | — | empty | no handler |
| CMSG_PING | 0x3768 | 14184 | 476 (0x1dc) | fixed, 8 bytes | no handler |
| CMSG_LOG_DISCONNECT | 0x3769 | 14185 | — | fixed, 4 bytes | no handler |
| CMSG_SUSPEND_TOKEN_RESPONSE | 0x376a | 14186 | — | fixed, 4 bytes | no handler |
| CMSG_ENABLE_NAGLE | 0x376b | 14187 | — | empty | no handler |
| CMSG_QUEUED_MESSAGES_END | 0x376c | 14188 | — | fixed, 4 bytes | no handler |
| — | 0x376d | 14189 | — | variable | — |
| — | 0x376f | 14191 | — | fixed, 8 bytes | — |
| — | 0x3770 | 14192 | — | variable | — |
| — | 0x3771 | 14193 | — | variable | — |
| CMSG_QUERY_PLAYER_NAMES | 0x3772 | 14194 | — | variable | matches |
| CMSG_CHAT_JOIN_CHANNEL | 0x37c8 | 14280 | 151 (0x97) | variable | matches; see #362 |
| CMSG_CHAT_LEAVE_CHANNEL | 0x37c9 | 14281 | 152 (0x98) | variable | matches |
| — | 0x37cb | 14283 | — | fixed, 1 bytes | — |
| — | 0x37cc | 14284 | — | fixed, 0 bytes | — |
| CMSG_CHAT_REGISTER_ADDON_PREFIXES | 0x37cd | 14285 | — | variable | matches |
| CMSG_CHAT_UNREGISTER_ALL_ADDON_PREFIXES | 0x37ce | 14286 | — | empty | matches |
| CMSG_CHAT_MESSAGE_CHANNEL | 0x37cf | 14287 | — | variable | matches |
| CMSG_CHAT_MESSAGE_WHISPER | 0x37d0 | 14288 | — | variable | matches |
| CMSG_CHAT_MESSAGE_GUILD | 0x37d1 | 14289 | — | variable | differs (#362) |
| CMSG_CHAT_MESSAGE_OFFICER | 0x37d2 | 14290 | — | variable | differs (#362) |
| CMSG_CHAT_MESSAGE_AFK | 0x37d3 | 14291 | — | variable | matches |
| CMSG_CHAT_MESSAGE_DND | 0x37d4 | 14292 | — | variable | matches |
| CMSG_CHAT_CHANNEL_LIST | 0x37d5 | 14293 | 154 (0x9a) | variable | matches |
| CMSG_CHAT_CHANNEL_DISPLAY_LIST | 0x37d6 | 14294 | 978 (0x3d2) | variable | matches |
| CMSG_CHAT_CHANNEL_PASSWORD | 0x37d7 | 14295 | 156 (0x9c) | variable | matches; see #362 |
| CMSG_CHAT_CHANNEL_SET_OWNER | 0x37d8 | 14296 | 157 (0x9d) | variable | matches; see #362 |
| CMSG_CHAT_CHANNEL_OWNER | 0x37d9 | 14297 | 158 (0x9e) | variable | matches |
| CMSG_CHAT_CHANNEL_MODERATOR | 0x37db | 14299 | 159 (0x9f) | variable | matches; see #362 |
| CMSG_CHAT_CHANNEL_UNMODERATOR | 0x37dc | 14300 | 160 (0xa0) | variable | matches; see #362 |
| CMSG_CHAT_CHANNEL_INVITE | 0x37df | 14303 | 163 (0xa3) | variable | matches; see #362 |
| CMSG_CHAT_CHANNEL_KICK | 0x37e0 | 14304 | 164 (0xa4) | variable | matches; see #362 |
| CMSG_CHAT_CHANNEL_BAN | 0x37e1 | 14305 | 165 (0xa5) | variable | matches; see #362 |
| CMSG_CHAT_CHANNEL_UNBAN | 0x37e2 | 14306 | 166 (0xa6) | variable | matches; see #362 |
| CMSG_CHAT_CHANNEL_ANNOUNCEMENTS | 0x37e3 | 14307 | 167 (0xa7) | variable | matches |
| CMSG_CHAT_CHANNEL_SILENCE_ALL | 0x37e4 | 14308 | 973 (0x3cd) | variable | no handler (#362) |
| CMSG_CHAT_CHANNEL_UNSILENCE_ALL | 0x37e5 | 14309 | 975 (0x3cf) | variable | no handler (#362) |
| CMSG_CHAT_CHANNEL_DECLINE_INVITE | 0x37e6 | 14310 | 1040 (0x410) | variable | matches |
| CMSG_CHAT_MESSAGE_SAY | 0x37e7 | 14311 | — | variable | matches |
| CMSG_CHAT_MESSAGE_EMOTE | 0x37e8 | 14312 | — | variable | matches |
| CMSG_CHAT_MESSAGE_YELL | 0x37e9 | 14313 | — | variable | differs (#362) |
| CMSG_CHAT_MESSAGE_PARTY | 0x37ea | 14314 | — | variable | matches |
| CMSG_CHAT_MESSAGE_RAID | 0x37eb | 14315 | — | variable | matches |
| CMSG_CHAT_MESSAGE_INSTANCE_CHAT | 0x37ec | 14316 | — | variable | matches |
| CMSG_CHAT_MESSAGE_RAID_WARNING | 0x37ed | 14317 | — | variable | matches |
| CMSG_CHAT_ADDON_MESSAGE | 0x37ee | 14318 | — | variable | matches |
| — | 0x37ef | 14319 | — | variable | — |
| CMSG_WRAP_ITEM | 0x3994 | 14740 | 467 (0x1d3) | variable | matches |
| CMSG_USE_EQUIPMENT_SET | 0x3995 | 14741 | — | variable | matches |
| CMSG_AUTOSTORE_BANK_ITEM | 0x3996 | 14742 | 642 (0x282) | variable | matches |
| CMSG_AUTOBANK_ITEM | 0x3997 | 14743 | 643 (0x283) | variable | matches |
| CMSG_AUTO_EQUIP_ITEM | 0x3998 | 14744 | 266 (0x10a) | variable | matches |
| CMSG_AUTO_STORE_BAG_ITEM | 0x3999 | 14745 | 267 (0x10b) | variable | matches |
| CMSG_SWAP_ITEM | 0x399a | 14746 | 268 (0x10c) | variable | matches |
| CMSG_SWAP_INV_ITEM | 0x399b | 14747 | 269 (0x10d) | variable | matches |
| CMSG_SPLIT_ITEM | 0x399c | 14748 | 270 (0x10e) | variable | matches |
| CMSG_AUTO_EQUIP_ITEM_SLOT | 0x399d | 14749 | 271 (0x10f) | variable | matches |
| CMSG_MOVE_START_FORWARD | 0x39e4 | 14820 | — | variable | differs (#366) |
| CMSG_MOVE_START_BACKWARD | 0x39e5 | 14821 | — | variable | differs (#366) |
| CMSG_MOVE_STOP | 0x39e6 | 14822 | — | variable | differs (#366) |
| CMSG_MOVE_START_STRAFE_LEFT | 0x39e7 | 14823 | — | variable | differs (#366) |
| CMSG_MOVE_START_STRAFE_RIGHT | 0x39e8 | 14824 | — | variable | differs (#366) |
| CMSG_MOVE_STOP_STRAFE | 0x39e9 | 14825 | — | variable | differs (#366) |
| CMSG_MOVE_JUMP | 0x39ea | 14826 | — | variable | differs (#366) |
| CMSG_MOVE_DOUBLE_JUMP | 0x39eb | 14827 | — | variable | differs (#366) |
| CMSG_MOVE_START_TURN_LEFT | 0x39ec | 14828 | — | variable | differs (#366) |
| CMSG_MOVE_START_TURN_RIGHT | 0x39ed | 14829 | — | variable | differs (#366) |
| CMSG_MOVE_STOP_TURN | 0x39ee | 14830 | — | variable | differs (#366) |
| CMSG_MOVE_START_PITCH_UP | 0x39ef | 14831 | — | variable | differs (#366) |
| CMSG_MOVE_START_PITCH_DOWN | 0x39f0 | 14832 | — | variable | differs (#366) |
| CMSG_MOVE_STOP_PITCH | 0x39f1 | 14833 | — | variable | differs (#366) |
| CMSG_MOVE_SET_RUN_MODE | 0x39f2 | 14834 | — | variable | differs (#366) |
| CMSG_MOVE_SET_WALK_MODE | 0x39f3 | 14835 | — | variable | differs (#366) |
| CMSG_MOVE_TELEPORT_ACK | 0x39fa | 14842 | — | fixed, 8 bytes | matches |
| CMSG_MOVE_FALL_LAND | 0x39fb | 14843 | — | variable | differs (#366) |
| CMSG_MOVE_START_SWIM | 0x39fc | 14844 | — | variable | differs (#366) |
| CMSG_MOVE_STOP_SWIM | 0x39fd | 14845 | — | variable | differs (#366) |
| CMSG_MOVE_SET_FACING | 0x3a09 | 14857 | — | variable | differs (#366) |
| CMSG_MOVE_SET_PITCH | 0x3a0a | 14858 | — | variable | differs (#366) |
| CMSG_MOVE_FORCE_RUN_SPEED_CHANGE_ACK | 0x3a0b | 14859 | 227 (0xe3) | variable | differs (#366) |
| CMSG_MOVE_FORCE_RUN_BACK_SPEED_CHANGE_ACK | 0x3a0c | 14860 | 229 (0xe5) | variable | differs (#366) |
| CMSG_MOVE_FORCE_SWIM_SPEED_CHANGE_ACK | 0x3a0d | 14861 | 231 (0xe7) | variable | differs (#366) |
| CMSG_MOVE_FORCE_ROOT_ACK | 0x3a0e | 14862 | 233 (0xe9) | variable | differs (#366) |
| CMSG_MOVE_FORCE_UNROOT_ACK | 0x3a0f | 14863 | 235 (0xeb) | variable | differs (#366) |
| CMSG_MOVE_HEARTBEAT | 0x3a10 | 14864 | — | variable | differs (#366) |
| CMSG_MOVE_KNOCK_BACK_ACK | 0x3a12 | 14866 | 240 (0xf0) | variable | differs (#369) |
| CMSG_MOVE_HOVER_ACK | 0x3a13 | 14867 | 246 (0xf6) | variable | differs (#366) |
| CMSG_MOVE_SET_VEHICLE_REC_ID_ACK | 0x3a14 | 14868 | — | variable | no handler |
| — | 0x3a15 | 14869 | — | variable | — |
| — | 0x3a16 | 14870 | — | variable | — |
| CMSG_MOVE_REMOVE_MOVEMENT_FORCES | 0x3a17 | 14871 | — | variable | differs (#366) |
| CMSG_MOVE_SPLINE_DONE | 0x3a18 | 14872 | 713 (0x2c9) | variable | differs (#366) |
| CMSG_MOVE_FALL_RESET | 0x3a19 | 14873 | 714 (0x2ca) | variable | differs (#366) |
| — | 0x3a1a | 14874 | — | variable | — |
| CMSG_MOVE_TIME_SKIPPED | 0x3a1b | 14875 | 718 (0x2ce) | fixed, 4 bytes | matches |
| CMSG_MOVE_FEATHER_FALL_ACK | 0x3a1c | 14876 | 719 (0x2cf) | variable | differs (#366) |
| CMSG_MOVE_WATER_WALK_ACK | 0x3a1d | 14877 | 720 (0x2d0) | variable | differs (#366) |
| — | 0x3a1e | 14878 | — | variable | — |
| CMSG_MOVE_FORCE_WALK_SPEED_CHANGE_ACK | 0x3a21 | 14881 | 731 (0x2db) | variable | differs (#366) |
| CMSG_MOVE_FORCE_SWIM_BACK_SPEED_CHANGE_ACK | 0x3a22 | 14882 | 733 (0x2dd) | variable | differs (#366) |
| CMSG_MOVE_FORCE_TURN_RATE_CHANGE_ACK | 0x3a23 | 14883 | 735 (0x2df) | variable | differs (#366) |
| — | 0x3a24 | 14884 | — | variable | — |
| — | 0x3a25 | 14885 | — | variable | — |
| — | 0x3a26 | 14886 | — | variable | — |
| CMSG_MOVE_SET_CAN_FLY_ACK | 0x3a27 | 14887 | 837 (0x345) | variable | differs (#366) |
| CMSG_MOVE_SET_FLY | 0x3a28 | 14888 | 838 (0x346) | variable | differs (#366) |
| CMSG_MOVE_START_ASCEND | 0x3a29 | 14889 | — | variable | differs (#366) |
| CMSG_MOVE_STOP_ASCEND | 0x3a2a | 14890 | — | variable | differs (#366) |
| CMSG_MOVE_FORCE_FLIGHT_SPEED_CHANGE_ACK | 0x3a2d | 14893 | 898 (0x382) | variable | differs (#366) |
| CMSG_MOVE_FORCE_FLIGHT_BACK_SPEED_CHANGE_ACK | 0x3a2e | 14894 | 900 (0x384) | variable | differs (#366) |
| CMSG_MOVE_CHANGE_TRANSPORT | 0x3a2f | 14895 | 909 (0x38d) | variable | differs (#366) |
| CMSG_MOVE_START_DESCEND | 0x3a30 | 14896 | — | variable | differs (#366) |
| CMSG_MOVE_FORCE_PITCH_RATE_CHANGE_ACK | 0x3a32 | 14898 | 1117 (0x45d) | variable | differs (#366) |
| CMSG_MOVE_DISMISS_VEHICLE | 0x3a33 | 14899 | — | variable | reads the first part |
| — | 0x3a34 | 14900 | — | variable | — |
| CMSG_MOVE_GRAVITY_DISABLE_ACK | 0x3a35 | 14901 | 1231 (0x4cf) | variable | differs (#366) |
| CMSG_MOVE_GRAVITY_ENABLE_ACK | 0x3a36 | 14902 | 1233 (0x4d1) | variable | differs (#366) |
| — | 0x3a37 | 14903 | — | variable | — |
| — | 0x3a38 | 14904 | — | variable | — |
| — | 0x3a39 | 14905 | — | variable | — |
| — | 0x3a3a | 14906 | — | variable | — |
| CMSG_MOVE_SET_COLLISION_HEIGHT_ACK | 0x3a3b | 14907 | — | variable | differs (#366) |
| CMSG_SET_ACTIVE_MOVER | 0x3a3c | 14908 | 618 (0x26a) | fixed, 0 bytes | matches |
| CMSG_TIME_SYNC_RESPONSE | 0x3a3d | 14909 | 913 (0x391) | fixed, 8 bytes | matches |
| — | 0x3a3e | 14910 | — | fixed, 4 bytes | — |
| CMSG_TIME_SYNC_RESPONSE_DROPPED | 0x3a3f | 14911 | — | fixed, 8 bytes | no handler |
| — | 0x3a40 | 14912 | — | fixed, 8 bytes | — |
| CMSG_DISCARDED_TIME_SYNC_ACKS | 0x3a41 | 14913 | — | fixed, 4 bytes | no handler |
| — | 0x3a42 | 14914 | — | variable | — |
| CMSG_UPDATE_MISSILE_TRAJECTORY | 0x3a43 | 14915 | 1122 (0x462) | variable | no handler |
| — | 0x3a44 | 14916 | — | variable | — |
| CMSG_MOVE_INIT_ACTIVE_MOVER_COMPLETE | 0x3a46 | 14918 | — | fixed, 4 bytes | matches |
| — | 0x3a4e | 14926 | — | variable | — |
| — | 0x3a4f | 14927 | — | variable | — |
| — | 0x3a50 | 14928 | — | variable | — |
| — | 0x3a51 | 14929 | — | variable | — |
| — | 0x3a52 | 14930 | — | variable | — |
| — | 0x3a53 | 14931 | — | variable | — |
| — | 0x3a54 | 14932 | — | variable | — |
| — | 0x3a55 | 14933 | — | variable | — |
| — | 0x3a56 | 14934 | — | variable | — |
| — | 0x3a57 | 14935 | — | variable | — |
| — | 0x3a58 | 14936 | — | variable | — |
| — | 0x3a59 | 14937 | — | variable | — |
| — | 0x3a5a | 14938 | — | variable | — |
| — | 0x3a5b | 14939 | — | variable | — |
| — | 0x3a5c | 14940 | — | variable | — |
| — | 0x3a5d | 14941 | — | variable | — |
| — | 0x3a5e | 14942 | — | variable | — |
| CMSG_MOVE_SET_FACING_HEARTBEAT | 0x3a5f | 14943 | — | variable | differs (#366) |
| — | 0x3a60 | 14944 | — | variable | — |
| — | 0x3a63 | 14947 | — | variable | — |
