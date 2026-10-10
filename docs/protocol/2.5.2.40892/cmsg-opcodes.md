# Client opcodes (CMSG), 2.5.2.40892

Every client-to-server opcode the client sends. Layouts are in [cmsg-structures](cmsg-structures.md).

| Name | Hex | Dec | 2.4.3 | Layout | HermesProxy |
|---|---|---:|---|---|---|
| — | 0x305c | 12380 | — | fixed, 16 bytes | — |
| CMSG_GUILD_PROMOTE_MEMBER | 0x305d | 12381 | 139 (0x8b) | fixed, 0 bytes | matches |
| CMSG_GUILD_DEMOTE_MEMBER | 0x305e | 12382 | 140 (0x8c) | fixed, 0 bytes | matches |
| CMSG_GUILD_ASSIGN_MEMBER_RANK | 0x305f | 12383 | — | fixed, 4 bytes | no handler |
| CMSG_GUILD_DECLINE_INVITATION | 0x3060 | 12384 | 133 (0x85) | empty | matches |
| CMSG_GUILD_AUTO_DECLINE_INVITATION | 0x3061 | 12385 | — | empty | matches |
| CMSG_GUILD_LEAVE | 0x3062 | 12386 | 141 (0x8d) | empty | matches |
| CMSG_GUILD_OFFICER_REMOVE_MEMBER | 0x3063 | 12387 | 142 (0x8e) | fixed, 0 bytes | matches |
| CMSG_GUILD_ADD_RANK | 0x3064 | 12388 | 562 (0x232) | variable | matches |
| CMSG_GUILD_DELETE_RANK | 0x3065 | 12389 | 563 (0x233) | fixed, 4 bytes | matches |
| CMSG_GUILD_SHIFT_RANK | 0x3066 | 12390 | — | variable | no handler |
| CMSG_GUILD_SET_RANK_PERMISSIONS | 0x3067 | 12391 | 561 (0x231) | variable | matches |
| CMSG_GUILD_DELETE | 0x3068 | 12392 | 143 (0x8f) | empty | matches |
| — | 0x3069 | 12393 | — | fixed, 4 bytes | — |
| — | 0x306a | 12394 | — | fixed, 0 bytes | — |
| CMSG_GUILD_QUERY_MEMBERS_FOR_RECIPE | 0x306b | 12395 | — | fixed, 12 bytes | no handler |
| CMSG_GUILD_QUERY_NEWS | 0x306c | 12396 | — | fixed, 0 bytes | no handler |
| CMSG_GUILD_GET_RANKS | 0x306d | 12397 | — | fixed, 0 bytes | no handler |
| — | 0x306e | 12398 | — | variable | — |
| CMSG_GUILD_SET_ACHIEVEMENT_TRACKING | 0x306f | 12399 | — | variable | no handler |
| CMSG_GUILD_SET_FOCUSED_ACHIEVEMENT | 0x3070 | 12400 | — | fixed, 4 bytes | no handler |
| — | 0x3071 | 12401 | — | fixed, 4 bytes | — |
| CMSG_GUILD_SET_MEMBER_NOTE | 0x3072 | 12402 | — | variable | matches |
| CMSG_GUILD_GET_ROSTER | 0x3073 | 12403 | 137 (0x89) | empty | matches |
| CMSG_GUILD_UPDATE_MOTD_TEXT | 0x3074 | 12404 | 145 (0x91) | variable | matches |
| CMSG_GUILD_UPDATE_INFO_TEXT | 0x3075 | 12405 | 764 (0x2fc) | variable | matches |
| — | 0x3076 | 12406 | — | empty | — |
| — | 0x3077 | 12407 | — | fixed, 8 bytes | — |
| — | 0x3078 | 12408 | — | fixed, 0 bytes | — |
| — | 0x3079 | 12409 | — | empty | — |
| — | 0x307a | 12410 | — | fixed, 0 bytes | — |
| CMSG_GUILD_CHALLENGE_UPDATE_REQUEST | 0x307b | 12411 | — | empty | no handler |
| — | 0x307c | 12412 | — | variable | — |
| — | 0x307d | 12413 | — | variable | — |
| CMSG_GUILD_CHANGE_NAME_REQUEST | 0x307e | 12414 | — | variable | no handler |
| — | 0x307f | 12415 | — | variable | — |
| — | 0x3080 | 12416 | — | fixed, 4 bytes | — |
| — | 0x3081 | 12417 | — | empty | — |
| CMSG_GUILD_BANK_LOG_QUERY | 0x3082 | 12418 | — | fixed, 4 bytes | matches |
| CMSG_GUILD_BANK_REMAINING_WITHDRAW_MONEY_QUERY | 0x3083 | 12419 | — | empty | matches |
| CMSG_GUILD_PERMISSIONS_QUERY | 0x3084 | 12420 | — | empty | matches |
| — | 0x3085 | 12421 | — | empty | — |
| CMSG_GUILD_BANK_SET_TAB_TEXT | 0x3086 | 12422 | 1034 (0x40a) | variable | differs |
| CMSG_GUILD_BANK_TEXT_QUERY | 0x3087 | 12423 | — | fixed, 4 bytes | matches |
| CMSG_GUILD_REPLACE_GUILD_MASTER | 0x3088 | 12424 | — | empty | no handler |
| — | 0x3089 | 12425 | — | variable | — |
| — | 0x308a | 12426 | — | variable | — |
| — | 0x308b | 12427 | — | empty | — |
| — | 0x308c | 12428 | — | variable | — |
| CMSG_GUILD_ADD_BATTLENET_FRIEND | 0x308d | 12429 | — | variable | no handler |
| CMSG_MYTHIC_PLUS_REQUEST_MAP_STATS | 0x308e | 12430 | — | fixed, 8 bytes | no handler |
| — | 0x308f | 12431 | — | empty | — |
| — | 0x3090 | 12432 | — | fixed, 4 bytes | — |
| — | 0x3091 | 12433 | — | fixed, 4 bytes | — |
| — | 0x3124 | 12580 | — | variable | — |
| — | 0x3125 | 12581 | — | variable | — |
| — | 0x3126 | 12582 | — | variable | — |
| CMSG_TWITTER_CONNECT | 0x3127 | 12583 | — | empty | no handler |
| — | 0x3128 | 12584 | — | variable | — |
| — | 0x3129 | 12585 | — | variable | — |
| CMSG_TWITTER_CHECK_STATUS | 0x312a | 12586 | — | empty | no handler |
| CMSG_TWITTER_DISCONNECT | 0x312b | 12587 | — | empty | no handler |
| CMSG_BATTLE_PET_CLEAR_FANFARE | 0x312c | 12588 | — | fixed, 0 bytes | no handler |
| — | 0x312d | 12589 | — | fixed, 4 bytes | — |
| — | 0x312e | 12590 | — | fixed, 4 bytes | — |
| — | 0x312f | 12591 | — | fixed, 4 bytes | — |
| — | 0x3130 | 12592 | — | fixed, 4 bytes | — |
| — | 0x3131 | 12593 | — | fixed, 12 bytes | — |
| — | 0x3132 | 12594 | — | empty | — |
| — | 0x3133 | 12595 | — | fixed, 12 bytes | — |
| — | 0x3134 | 12596 | — | empty | — |
| — | 0x3135 | 12597 | — | empty | — |
| — | 0x3136 | 12598 | — | empty | — |
| — | 0x3137 | 12599 | — | empty | — |
| — | 0x3138 | 12600 | — | fixed, 4 bytes | — |
| — | 0x3139 | 12601 | — | fixed, 12 bytes | — |
| — | 0x313a | 12602 | — | fixed, 4 bytes | — |
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
| CMSG_SET_TRADE_CURRENCY | 0x3160 | 12640 | — | fixed, 8 bytes | no handler |
| — | 0x3161 | 12641 | — | variable | — |
| — | 0x3162 | 12642 | — | empty | — |
| — | 0x3163 | 12643 | — | empty | — |
| — | 0x3164 | 12644 | — | fixed, 4 bytes | — |
| — | 0x3165 | 12645 | — | fixed, 8 bytes | — |
| — | 0x3166 | 12646 | — | fixed, 4 bytes | — |
| — | 0x3167 | 12647 | — | fixed, 1 bytes | — |
| CMSG_REQUEST_RESEARCH_HISTORY | 0x3168 | 12648 | — | empty | no handler |
| CMSG_STABLE_PET | 0x3169 | 12649 | 624 (0x270) | fixed, 0 bytes | matches |
| CMSG_UNSTABLE_PET | 0x316a | 12650 | 625 (0x271) | fixed, 4 bytes | matches |
| CMSG_STABLE_SWAP_PET | 0x316b | 12651 | 629 (0x275) | fixed, 4 bytes | matches |
| CMSG_BUY_STABLE_SLOT | 0x316c | 12652 | 626 (0x272) | fixed, 0 bytes | matches |
| — | 0x316d | 12653 | — | fixed, 8 bytes | — |
| — | 0x316e | 12654 | — | empty | — |
| — | 0x316f | 12655 | — | empty | — |
| — | 0x3170 | 12656 | — | fixed, 8 bytes | — |
| — | 0x3171 | 12657 | — | variable | — |
| — | 0x3172 | 12658 | — | variable | — |
| — | 0x3173 | 12659 | — | empty | — |
| — | 0x3174 | 12660 | — | fixed, 4 bytes | — |
| — | 0x3175 | 12661 | — | fixed, 4 bytes | — |
| CMSG_BATTLEFIELD_LEAVE | 0x3176 | 12662 | 737 (0x2e1) | empty | matches |
| CMSG_SURRENDER_ARENA | 0x3177 | 12663 | — | empty | no handler |
| CMSG_QUERY_QUEST_COMPLETION_NPCS | 0x3178 | 12664 | — | variable | no handler |
| CMSG_REQUEST_CEMETERY_LIST | 0x3179 | 12665 | — | empty | no handler |
| CMSG_SET_PREFERRED_CEMETERY | 0x317a | 12666 | — | fixed, 4 bytes | no handler |
| CMSG_JOIN_RATED_BATTLEGROUND | 0x317b | 12667 | — | fixed, 1 bytes | no handler |
| — | 0x317c | 12668 | — | empty | — |
| — | 0x317d | 12669 | — | fixed, 4 bytes | — |
| CMSG_INSPECT_HONOR_STATS | 0x317e | 12670 | — | fixed, 0 bytes | matches |
| CMSG_PVP_LOG_DATA | 0x317f | 12671 | — | empty | matches |
| — | 0x3180 | 12672 | — | empty | — |
| CMSG_REQUEST_CATEGORY_COOLDOWNS | 0x3181 | 12673 | — | empty | no handler |
| CMSG_BATTLEFIELD_LIST | 0x3182 | 12674 | 572 (0x23c) | fixed, 4 bytes | matches |
| CMSG_CANCEL_QUEUED_SPELL | 0x3183 | 12675 | — | empty | no handler |
| CMSG_OBJECT_UPDATE_FAILED | 0x3184 | 12676 | — | fixed, 0 bytes | matches |
| CMSG_OBJECT_UPDATE_RESCUED | 0x3185 | 12677 | — | fixed, 0 bytes | no handler |
| — | 0x3186 | 12678 | — | variable | — |
| — | 0x3187 | 12679 | — | empty | — |
| CMSG_VIOLENCE_LEVEL | 0x3188 | 12680 | — | fixed, 1 bytes | no handler |
| — | 0x3189 | 12681 | — | fixed, 4 bytes | — |
| CMSG_USED_FOLLOW | 0x318a | 12682 | — | empty | no handler |
| — | 0x318b | 12683 | — | variable | — |
| — | 0x318c | 12684 | — | variable | — |
| CMSG_STAND_STATE_CHANGE | 0x318d | 12685 | 257 (0x101) | fixed, 4 bytes | matches |
| CMSG_MISSILE_TRAJECTORY_COLLISION | 0x318e | 12686 | — | fixed, 16 bytes | no handler |
| CMSG_SAVE_CUF_PROFILES | 0x318f | 12687 | — | variable | matches |
| — | 0x3190 | 12688 | — | empty | — |
| — | 0x3191 | 12689 | — | empty | — |
| — | 0x3192 | 12690 | — | empty | — |
| — | 0x3193 | 12691 | — | variable | — |
| — | 0x3194 | 12692 | — | variable | — |
| — | 0x3195 | 12693 | — | variable | — |
| — | 0x3196 | 12694 | — | empty | — |
| CMSG_REQUEST_PVP_REWARDS | 0x3197 | 12695 | — | empty | no handler |
| CMSG_REQUEST_SCHEDULED_PVP_INFO | 0x3198 | 12696 | — | empty | no handler |
| CMSG_TRANSMOGRIFY_ITEMS | 0x3199 | 12697 | — | variable | no handler |
| — | 0x319a | 12698 | — | empty | — |
| — | 0x319b | 12699 | — | empty | — |
| — | 0x319c | 12700 | — | empty | — |
| — | 0x319d | 12701 | — | fixed, 4 bytes | — |
| — | 0x319e | 12702 | — | variable | — |
| — | 0x319f | 12703 | — | fixed, 4 bytes | — |
| — | 0x31a0 | 12704 | — | fixed, 4 bytes | — |
| — | 0x31a1 | 12705 | — | variable | — |
| — | 0x31a2 | 12706 | — | fixed, 0 bytes | — |
| CMSG_UNLOCK_VOID_STORAGE | 0x31a3 | 12707 | — | fixed, 0 bytes | no handler |
| CMSG_QUERY_VOID_STORAGE | 0x31a4 | 12708 | — | fixed, 0 bytes | no handler |
| CMSG_VOID_STORAGE_TRANSFER | 0x31a5 | 12709 | — | variable | no handler |
| CMSG_SWAP_VOID_ITEM | 0x31a6 | 12710 | — | fixed, 4 bytes | no handler |
| CMSG_UNLEARN_SPECIALIZATION | 0x31a7 | 12711 | — | fixed, 1 bytes | no handler |
| CMSG_CLEAR_RAID_MARKER | 0x31a8 | 12712 | — | fixed, 1 bytes | no handler |
| CMSG_REQUEST_GUILD_REWARDS_LIST | 0x31a9 | 12713 | — | fixed, 8 bytes | no handler |
| CMSG_REQUEST_GUILD_PARTY_STATE | 0x31aa | 12714 | — | fixed, 0 bytes | no handler |
| CMSG_QUERY_COUNTDOWN_TIMER | 0x31ab | 12715 | — | fixed, 4 bytes | no handler |
| CMSG_ARTIFACT_ADD_POWER | 0x31ac | 12716 | — | variable | no handler |
| CMSG_CONFIRM_ARTIFACT_RESPEC | 0x31ad | 12717 | — | fixed, 0 bytes | no handler |
| CMSG_ARTIFACT_SET_APPEARANCE | 0x31ae | 12718 | — | fixed, 4 bytes | no handler |
| CMSG_CANCEL_MOD_SPEED_NO_CONTROL_AURAS | 0x31af | 12719 | — | fixed, 0 bytes | no handler |
| CMSG_CANCEL_AURA | 0x31b0 | 12720 | 310 (0x136) | fixed, 4 bytes | matches |
| — | 0x31b1 | 12721 | — | fixed, 4 bytes | — |
| CMSG_GAME_EVENT_DEBUG_ENABLE | 0x31b2 | 12722 | — | empty | no handler |
| CMSG_GAME_EVENT_DEBUG_DISABLE | 0x31b3 | 12723 | — | empty | no handler |
| — | 0x31b4 | 12724 | — | empty | — |
| — | 0x31b5 | 12725 | — | variable | — |
| — | 0x31b6 | 12726 | — | variable | — |
| — | 0x31b7 | 12727 | — | variable | — |
| — | 0x31b8 | 12728 | — | variable | — |
| — | 0x31b9 | 12729 | — | variable | — |
| CMSG_SET_GAME_EVENT_DEBUG_VIEW_STATE | 0x31ba | 12730 | — | variable | no handler |
| — | 0x31bb | 12731 | — | variable | — |
| — | 0x31bc | 12732 | — | variable | — |
| — | 0x31bd | 12733 | — | variable | — |
| — | 0x31be | 12734 | — | variable | — |
| — | 0x31bf | 12735 | — | empty | — |
| — | 0x31c0 | 12736 | — | empty | — |
| — | 0x31c1 | 12737 | — | empty | — |
| — | 0x31c2 | 12738 | — | empty | — |
| — | 0x31c3 | 12739 | — | empty | — |
| — | 0x31c4 | 12740 | — | empty | — |
| — | 0x31c5 | 12741 | — | fixed, 5 bytes | — |
| — | 0x31c6 | 12742 | — | variable | — |
| — | 0x31c7 | 12743 | — | fixed, 0 bytes | — |
| — | 0x31c8 | 12744 | — | variable | — |
| — | 0x31c9 | 12745 | — | fixed, 4 bytes | — |
| — | 0x31ca | 12746 | — | fixed, 4 bytes | — |
| — | 0x31cb | 12747 | — | fixed, 4 bytes | — |
| — | 0x31cc | 12748 | — | fixed, 4 bytes | — |
| — | 0x31cd | 12749 | — | fixed, 4 bytes | — |
| — | 0x31ce | 12750 | — | fixed, 4 bytes | — |
| — | 0x31cf | 12751 | — | variable | — |
| — | 0x31d0 | 12752 | — | fixed, 4 bytes | — |
| — | 0x31d1 | 12753 | — | fixed, 4 bytes | — |
| — | 0x31d2 | 12754 | — | fixed, 4 bytes | — |
| — | 0x31d3 | 12755 | — | fixed, 12 bytes | — |
| CMSG_NEUTRAL_PLAYER_SELECT_FACTION | 0x31d4 | 12756 | — | fixed, 4 bytes | no handler |
| — | 0x31d5 | 12757 | — | variable | — |
| — | 0x31d6 | 12758 | — | variable | — |
| CMSG_AREA_TRIGGER | 0x31d7 | 12759 | 180 (0xb4) | variable | matches |
| — | 0x31d8 | 12760 | — | empty | — |
| — | 0x31d9 | 12761 | — | empty | — |
| CMSG_PET_BATTLE_REQUEST_WILD | 0x31da | 12762 | — | variable | no handler |
| CMSG_PET_BATTLE_WILD_LOCATION_FAIL | 0x31db | 12763 | — | fixed, 12 bytes | no handler |
| CMSG_PET_BATTLE_REQUEST_PVP | 0x31dc | 12764 | — | variable | no handler |
| CMSG_PET_BATTLE_REQUEST_UPDATE | 0x31dd | 12765 | — | variable | no handler |
| CMSG_JOIN_PET_BATTLE_QUEUE | 0x31de | 12766 | — | empty | no handler |
| CMSG_LEAVE_PET_BATTLE_QUEUE | 0x31df | 12767 | — | fixed, 16 bytes | no handler |
| CMSG_BATTLE_PET_UPDATE_NOTIFY | 0x31e0 | 12768 | — | fixed, 0 bytes | no handler |
| CMSG_BATTLE_PET_UPDATE_DISPLAY_NOTIFY | 0x31e1 | 12769 | — | empty | no handler |
| CMSG_PET_BATTLE_QUIT_NOTIFY | 0x31e2 | 12770 | — | empty | no handler |
| CMSG_PET_BATTLE_FINAL_NOTIFY | 0x31e3 | 12771 | — | empty | no handler |
| CMSG_PET_BATTLE_SCRIPT_ERROR_NOTIFY | 0x31e4 | 12772 | — | empty | no handler |
| — | 0x31e5 | 12773 | — | variable | — |
| — | 0x31e6 | 12774 | — | empty | — |
| — | 0x31e7 | 12775 | — | empty | — |
| — | 0x31e8 | 12776 | — | variable | — |
| — | 0x31e9 | 12777 | — | fixed, 1 bytes | — |
| — | 0x31ea | 12778 | — | variable | — |
| — | 0x31eb | 12779 | — | variable | — |
| — | 0x31ec | 12780 | — | variable | — |
| — | 0x31ed | 12781 | — | fixed, 2 bytes | — |
| — | 0x31ee | 12782 | — | fixed, 1 bytes | — |
| — | 0x31ef | 12783 | — | fixed, 1 bytes | — |
| — | 0x31f0 | 12784 | — | fixed, 4 bytes | — |
| — | 0x31f1 | 12785 | — | fixed, 0 bytes | — |
| — | 0x31f2 | 12786 | — | fixed, 5 bytes | — |
| CMSG_CAGE_BATTLE_PET | 0x31f3 | 12787 | — | fixed, 8 bytes | no handler |
| — | 0x31f4 | 12788 | — | variable | — |
| — | 0x31f5 | 12789 | — | fixed, 8 bytes | — |
| — | 0x31f6 | 12790 | — | fixed, 8 bytes | — |
| — | 0x31f7 | 12791 | — | variable | — |
| — | 0x31f8 | 12792 | — | empty | — |
| — | 0x31f9 | 12793 | — | variable | — |
| — | 0x31fa | 12794 | — | empty | — |
| — | 0x31fb | 12795 | — | variable | — |
| — | 0x31fc | 12796 | — | fixed, 4 bytes | — |
| — | 0x31fd | 12797 | — | empty | — |
| — | 0x31fe | 12798 | — | fixed, 4 bytes | — |
| — | 0x31ff | 12799 | — | fixed, 4 bytes | — |
| — | 0x3200 | 12800 | — | fixed, 4 bytes | — |
| CMSG_ADVENTURE_JOURNAL_OPEN_QUEST | 0x3201 | 12801 | — | fixed, 4 bytes | no handler |
| — | 0x3202 | 12802 | — | empty | — |
| — | 0x3203 | 12803 | — | empty | — |
| — | 0x3204 | 12804 | — | empty | — |
| — | 0x3205 | 12805 | — | empty | — |
| — | 0x3206 | 12806 | — | fixed, 4 bytes | — |
| CMSG_REQUEST_FORCED_REACTIONS | 0x3207 | 12807 | — | empty | no handler |
| — | 0x3208 | 12808 | — | fixed, 5 bytes | — |
| CMSG_ASSIGN_EQUIPMENT_SET_SPEC | 0x3209 | 12809 | — | fixed, 12 bytes | no handler |
| — | 0x320a | 12810 | — | fixed, 4 bytes | — |
| CMSG_CONFIRM_RESPEC_WIPE | 0x320b | 12811 | — | fixed, 1 bytes | matches |
| CMSG_LOOT_UNIT | 0x320c | 12812 | 349 (0x15d) | fixed, 0 bytes | matches |
| CMSG_LOOT_MONEY | 0x320d | 12813 | 350 (0x15e) | empty | matches |
| CMSG_LOOT_ITEM | 0x320e | 12814 | — | variable | matches |
| CMSG_LOOT_MASTER_GIVE | 0x320f | 12815 | 675 (0x2a3) | variable | matches |
| CMSG_LOOT_RELEASE | 0x3210 | 12816 | 351 (0x15f) | fixed, 0 bytes | matches |
| CMSG_LOOT_ROLL | 0x3211 | 12817 | 672 (0x2a0) | fixed, 2 bytes | matches |
| — | 0x3212 | 12818 | — | fixed, 4 bytes | — |
| — | 0x3213 | 12819 | — | variable | — |
| — | 0x3214 | 12820 | — | variable | — |
| — | 0x3215 | 12821 | — | empty | — |
| — | 0x3216 | 12822 | — | fixed, 4 bytes | — |
| — | 0x3217 | 12823 | — | empty | — |
| — | 0x3218 | 12824 | — | empty | — |
| — | 0x3219 | 12825 | — | variable | — |
| — | 0x321a | 12826 | — | empty | — |
| — | 0x321b | 12827 | — | empty | — |
| CMSG_SCENE_PLAYBACK_COMPLETE | 0x321c | 12828 | — | fixed, 4 bytes | no handler |
| CMSG_SCENE_PLAYBACK_CANCELED | 0x321d | 12829 | — | fixed, 4 bytes | no handler |
| CMSG_SCENE_TRIGGER_EVENT | 0x321e | 12830 | — | variable | no handler |
| CMSG_SET_DIFFICULTY_ID | 0x321f | 12831 | — | fixed, 4 bytes | no handler |
| CMSG_KEYBOUND_OVERRIDE | 0x3220 | 12832 | — | fixed, 0 bytes | no handler |
| CMSG_PET_BATTLE_QUEUE_PROPOSE_MATCH_RESULT | 0x3221 | 12833 | — | variable | no handler |
| CMSG_MAIL_DELETE | 0x3222 | 12834 | 585 (0x249) | fixed, 8 bytes | matches |
| CMSG_SET_ACHIEVEMENTS_HIDDEN | 0x3223 | 12835 | — | variable | no handler |
| — | 0x3224 | 12836 | — | fixed, 12 bytes | — |
| CMSG_MAKE_CONTITIONAL_APPEARANCE_PERMANENT | 0x3225 | 12837 | — | fixed, 4 bytes | no handler |
| — | 0x3226 | 12838 | — | variable | — |
| — | 0x3227 | 12839 | — | empty | — |
| — | 0x3228 | 12840 | — | fixed, 4 bytes | — |
| — | 0x3229 | 12841 | — | fixed, 4 bytes | — |
| — | 0x322a | 12842 | — | empty | — |
| — | 0x322b | 12843 | — | fixed, 4 bytes | — |
| — | 0x322c | 12844 | — | fixed, 8 bytes | — |
| CMSG_PERFORM_ITEM_INTERACTION | 0x322d | 12845 | — | variable | no handler |
| — | 0x322e | 12846 | — | empty | — |
| — | 0x322f | 12847 | — | fixed, 4 bytes | — |
| — | 0x3230 | 12848 | — | fixed, 4 bytes | — |
| — | 0x3231 | 12849 | — | variable | — |
| CMSG_REQUEST_VEHICLE_EXIT | 0x3232 | 12850 | — | empty | matches |
| CMSG_REQUEST_VEHICLE_PREV_SEAT | 0x3233 | 12851 | — | empty | matches |
| CMSG_REQUEST_VEHICLE_NEXT_SEAT | 0x3234 | 12852 | — | empty | matches |
| CMSG_REQUEST_VEHICLE_SWITCH_SEAT | 0x3235 | 12853 | — | fixed, 1 bytes | matches |
| CMSG_RIDE_VEHICLE_INTERACT | 0x3236 | 12854 | — | fixed, 0 bytes | matches |
| CMSG_EJECT_PASSENGER | 0x3237 | 12855 | — | fixed, 0 bytes | matches |
| — | 0x3238 | 12856 | — | fixed, 1 bytes | — |
| — | 0x3239 | 12857 | — | empty | — |
| — | 0x323a | 12858 | — | fixed, 4 bytes | — |
| — | 0x323b | 12859 | — | fixed, 0 bytes | — |
| CMSG_USE_CRITTER_ITEM | 0x323c | 12860 | — | fixed, 0 bytes | no handler |
| — | 0x323d | 12861 | — | fixed, 0 bytes | — |
| — | 0x323e | 12862 | — | fixed, 0 bytes | — |
| — | 0x323f | 12863 | — | fixed, 4 bytes | — |
| — | 0x3240 | 12864 | — | fixed, 20 bytes | — |
| — | 0x3241 | 12865 | — | fixed, 12 bytes | — |
| CMSG_CHECK_IS_ADVENTURE_MAP_POI_VALID | 0x3242 | 12866 | — | fixed, 4 bytes | no handler |
| — | 0x3243 | 12867 | — | fixed, 4 bytes | — |
| — | 0x3244 | 12868 | — | fixed, 12 bytes | — |
| — | 0x3245 | 12869 | — | empty | — |
| — | 0x3246 | 12870 | — | fixed, 4 bytes | — |
| — | 0x3247 | 12871 | — | variable | — |
| — | 0x3248 | 12872 | — | variable | — |
| — | 0x3249 | 12873 | — | fixed, 4 bytes | — |
| — | 0x324a | 12874 | — | empty | — |
| — | 0x324b | 12875 | — | fixed, 4 bytes | — |
| — | 0x324c | 12876 | — | variable | — |
| — | 0x324d | 12877 | — | variable | — |
| — | 0x324e | 12878 | — | variable | — |
| — | 0x324f | 12879 | — | fixed, 0 bytes | — |
| CMSG_ATTACK_SWING | 0x3250 | 12880 | 321 (0x141) | fixed, 0 bytes | matches |
| CMSG_ATTACK_STOP | 0x3251 | 12881 | 322 (0x142) | empty | matches |
| — | 0x3252 | 12882 | — | empty | — |
| — | 0x3253 | 12883 | — | empty | — |
| — | 0x3254 | 12884 | — | fixed, 0 bytes | — |
| — | 0x3255 | 12885 | — | fixed, 8 bytes | — |
| — | 0x3256 | 12886 | — | fixed, 8 bytes | — |
| — | 0x3257 | 12887 | — | variable | — |
| — | 0x3258 | 12888 | — | variable | — |
| — | 0x3259 | 12889 | — | variable | — |
| — | 0x325a | 12890 | — | empty | — |
| — | 0x325b | 12891 | — | fixed, 8 bytes | — |
| — | 0x325c | 12892 | — | variable | — |
| — | 0x325d | 12893 | — | fixed, 8 bytes | — |
| — | 0x325e | 12894 | — | variable | — |
| — | 0x325f | 12895 | — | variable | — |
| — | 0x3260 | 12896 | — | fixed, 16 bytes | — |
| — | 0x3261 | 12897 | — | variable | — |
| — | 0x3262 | 12898 | — | variable | — |
| — | 0x3263 | 12899 | — | fixed, 0 bytes | — |
| — | 0x3264 | 12900 | — | fixed, 0 bytes | — |
| CMSG_CANCEL_CHANNELLING | 0x3265 | 12901 | 315 (0x13b) | fixed, 8 bytes | matches |
| — | 0x3266 | 12902 | — | fixed, 4 bytes | — |
| — | 0x3267 | 12903 | — | fixed, 4 bytes | — |
| — | 0x3268 | 12904 | — | fixed, 4 bytes | — |
| — | 0x3269 | 12905 | — | fixed, 4 bytes | — |
| CMSG_CANCEL_GROWTH_AURA | 0x326a | 12906 | 667 (0x29b) | empty | no handler |
| CMSG_QUERY_CREATURE | 0x326b | 12907 | 96 (0x60) | fixed, 4 bytes | matches |
| CMSG_QUERY_GAME_OBJECT | 0x326c | 12908 | 94 (0x5e) | fixed, 4 bytes | matches |
| CMSG_QUERY_NPC_TEXT | 0x326d | 12909 | 383 (0x17f) | fixed, 4 bytes | matches |
| CMSG_QUERY_QUEST_INFO | 0x326e | 12910 | 92 (0x5c) | fixed, 4 bytes | matches |
| CMSG_QUERY_PAGE_TEXT | 0x326f | 12911 | 90 (0x5a) | fixed, 4 bytes | matches |
| CMSG_QUERY_PET_NAME | 0x3270 | 12912 | 82 (0x52) | fixed, 0 bytes | matches |
| CMSG_QUERY_BATTLE_PET_NAME | 0x3271 | 12913 | — | fixed, 0 bytes | no handler |
| CMSG_QUERY_PETITION | 0x3272 | 12914 | 454 (0x1c6) | fixed, 4 bytes | matches |
| — | 0x3273 | 12915 | — | variable | — |
| — | 0x3274 | 12916 | — | fixed, 4 bytes | — |
| CMSG_REQUEST_PLAYED_TIME | 0x3275 | 12917 | 460 (0x1cc) | variable | matches |
| — | 0x3276 | 12918 | — | variable | — |
| — | 0x3277 | 12919 | — | variable | — |
| — | 0x3278 | 12920 | — | variable | — |
| CMSG_SET_TITLE | 0x3279 | 12921 | 884 (0x374) | fixed, 4 bytes | matches |
| CMSG_CANCEL_MOUNT_AURA | 0x327a | 12922 | 885 (0x375) | empty | matches |
| CMSG_MOUNT_SPECIAL_ANIM | 0x327b | 12923 | 369 (0x171) | variable | matches |
| — | 0x327c | 12924 | — | fixed, 8 bytes | — |
| — | 0x327d | 12925 | — | fixed, 5 bytes | — |
| — | 0x327e | 12926 | — | fixed, 5 bytes | — |
| — | 0x327f | 12927 | — | fixed, 8 bytes | — |
| — | 0x3280 | 12928 | — | variable | — |
| — | 0x3281 | 12929 | — | fixed, 4 bytes | — |
| — | 0x3282 | 12930 | — | variable | — |
| — | 0x3283 | 12931 | — | variable | — |
| — | 0x3284 | 12932 | — | variable | — |
| — | 0x3285 | 12933 | — | fixed, 4 bytes | — |
| — | 0x3286 | 12934 | — | fixed, 0 bytes | — |
| — | 0x3287 | 12935 | — | empty | — |
| — | 0x3288 | 12936 | — | variable | — |
| — | 0x3289 | 12937 | — | fixed, 0 bytes | — |
| — | 0x328a | 12938 | — | fixed, 0 bytes | — |
| CMSG_DESTROY_ITEM | 0x328b | 12939 | 273 (0x111) | fixed, 6 bytes | matches |
| — | 0x328c | 12940 | — | fixed, 0 bytes | — |
| — | 0x328d | 12941 | — | fixed, 1 bytes | — |
| — | 0x328e | 12942 | — | fixed, 4 bytes | — |
| CMSG_GET_MIRROR_IMAGE_DATA | 0x328f | 12943 | 1024 (0x400) | fixed, 4 bytes | no handler |
| CMSG_USE_ITEM | 0x3290 | 12944 | 171 (0xab) | variable | differs |
| CMSG_ADD_TOY | 0x3291 | 12945 | — | fixed, 0 bytes | matches |
| CMSG_USE_TOY | 0x3292 | 12946 | — | variable | differs |
| CMSG_PET_CAST_SPELL | 0x3293 | 12947 | 496 (0x1f0) | variable | differs |
| CMSG_CAST_SPELL | 0x3294 | 12948 | 302 (0x12e) | variable | differs |
| CMSG_UPDATE_SPELL_VISUAL | 0x3295 | 12949 | — | fixed, 8 bytes | no handler |
| CMSG_UPDATE_AREA_TRIGGER_VISUAL | 0x3296 | 12950 | — | fixed, 8 bytes | no handler |
| CMSG_CANCEL_CAST | 0x3297 | 12951 | 303 (0x12f) | fixed, 4 bytes | matches |
| — | 0x3298 | 12952 | — | variable | — |
| CMSG_CHOICE_RESPONSE | 0x3299 | 12953 | — | variable | no handler |
| CMSG_CLOSE_QUEST_CHOICE | 0x329a | 12954 | — | empty | no handler |
| CMSG_HIDE_QUEST_CHOICE | 0x329b | 12955 | — | fixed, 4 bytes | no handler |
| CMSG_REQUEST_LFG_LIST_BLACKLIST | 0x329c | 12956 | — | empty | matches |
| — | 0x329d | 12957 | — | fixed, 4 bytes | — |
| — | 0x329e | 12958 | — | fixed, 4 bytes | — |
| — | 0x329f | 12959 | — | fixed, 8 bytes | — |
| CMSG_SAVE_GUILD_EMBLEM | 0x32a0 | 12960 | — | fixed, 20 bytes | matches |
| CMSG_TABARD_VENDOR_ACTIVATE | 0x32a1 | 12961 | — | fixed, 0 bytes | matches |
| — | 0x32a2 | 12962 | — | variable | — |
| CMSG_TOGGLE_PVP | 0x32a3 | 12963 | 595 (0x253) | empty | matches |
| CMSG_SET_PVP | 0x32a4 | 12964 | — | variable | matches |
| CMSG_SET_WAR_MODE | 0x32a5 | 12965 | — | variable | no handler |
| — | 0x32a6 | 12966 | — | empty | — |
| — | 0x32a7 | 12967 | — | variable | — |
| — | 0x32a8 | 12968 | — | variable | — |
| CMSG_BATTLEMASTER_HELLO | 0x32a9 | 12969 | 727 (0x2d7) | fixed, 0 bytes | matches |
| — | 0x32aa | 12970 | — | fixed, 4 bytes | — |
| — | 0x32ab | 12971 | — | variable | — |
| CMSG_REQUEST_CONQUEST_FORMULA_CONSTANTS | 0x32ac | 12972 | — | empty | matches |
| CMSG_SET_ADVANCED_COMBAT_LOGGING | 0x32ad | 12973 | — | variable | no handler |
| — | 0x32ae | 12974 | — | variable | — |
| — | 0x32af | 12975 | — | variable | — |
| — | 0x32b0 | 12976 | — | variable | — |
| — | 0x32b1 | 12977 | — | fixed, 4 bytes | — |
| — | 0x32b2 | 12978 | — | variable | — |
| — | 0x32b3 | 12979 | — | variable | — |
| — | 0x32b4 | 12980 | — | variable | — |
| — | 0x32b5 | 12981 | — | fixed, 4 bytes | — |
| — | 0x32b6 | 12982 | — | variable | — |
| — | 0x32b7 | 12983 | — | fixed, 12 bytes | — |
| — | 0x32b8 | 12984 | — | fixed, 4 bytes | — |
| — | 0x32b9 | 12985 | — | fixed, 4 bytes | — |
| — | 0x32ba | 12986 | — | fixed, 4 bytes | — |
| — | 0x32bb | 12987 | — | fixed, 4 bytes | — |
| — | 0x32bc | 12988 | — | fixed, 0 bytes | — |
| CMSG_ITEM_TEXT_QUERY | 0x32bd | 12989 | 579 (0x243) | fixed, 0 bytes | matches |
| CMSG_OPEN_ITEM | 0x32be | 12990 | 172 (0xac) | fixed, 2 bytes | matches |
| CMSG_READ_ITEM | 0x32bf | 12991 | 173 (0xad) | fixed, 2 bytes | matches |
| CMSG_CHANGE_BAG_SLOT_FLAG | 0x32c0 | 12992 | — | variable | no handler |
| CMSG_CHANGE_BANK_BAG_SLOT_FLAG | 0x32c1 | 12993 | — | variable | no handler |
| CMSG_SET_INSERT_ITEMS_LEFT_TO_RIGHT | 0x32c2 | 12994 | — | variable | no handler |
| — | 0x32c3 | 12995 | — | fixed, 4 bytes | — |
| — | 0x32c4 | 12996 | — | fixed, 0 bytes | — |
| — | 0x32c5 | 12997 | — | variable | — |
| — | 0x32c6 | 12998 | — | variable | — |
| — | 0x32c7 | 12999 | — | variable | — |
| — | 0x32c8 | 13000 | — | variable | — |
| — | 0x32c9 | 13001 | — | fixed, 4 bytes | — |
| — | 0x32ca | 13002 | — | empty | — |
| — | 0x32cb | 13003 | — | fixed, 4 bytes | — |
| — | 0x32cc | 13004 | — | fixed, 4 bytes | — |
| — | 0x32cd | 13005 | — | empty | — |
| CMSG_ADVENTURE_MAP_START_QUEST | 0x32ce | 13006 | — | fixed, 1 bytes | no handler |
| — | 0x32cf | 13007 | — | variable | — |
| — | 0x32d0 | 13008 | — | variable | — |
| — | 0x32d1 | 13009 | — | fixed, 4 bytes | — |
| — | 0x32d2 | 13010 | — | fixed, 4 bytes | — |
| CMSG_TRADE_SKILL_SET_FAVORITE | 0x32d3 | 13011 | — | variable | no handler |
| CMSG_QUERY_TREASURE_PICKER | 0x32d4 | 13012 | — | fixed, 8 bytes | no handler |
| CMSG_REQUEST_WORLD_QUEST_UPDATE | 0x32d5 | 13013 | — | empty | no handler |
| CMSG_REQUEST_AREA_POI_UPDATE | 0x32d6 | 13014 | — | empty | no handler |
| CMSG_REMOVE_NEW_ITEM | 0x32d7 | 13015 | — | fixed, 0 bytes | no handler |
| — | 0x32d8 | 13016 | — | fixed, 0 bytes | — |
| — | 0x32d9 | 13017 | — | fixed, 0 bytes | — |
| — | 0x32da | 13018 | — | fixed, 12 bytes | — |
| — | 0x32db | 13019 | — | empty | — |
| — | 0x32dc | 13020 | — | empty | — |
| — | 0x32dd | 13021 | — | empty | — |
| — | 0x32de | 13022 | — | empty | — |
| — | 0x32df | 13023 | — | empty | — |
| — | 0x32e0 | 13024 | — | empty | — |
| — | 0x32e1 | 13025 | — | variable | — |
| — | 0x32e2 | 13026 | — | fixed, 4 bytes | — |
| — | 0x32e3 | 13027 | — | fixed, 4 bytes | — |
| — | 0x32e4 | 13028 | — | variable | — |
| — | 0x32e5 | 13029 | — | variable | — |
| — | 0x32e6 | 13030 | — | fixed, 1 bytes | — |
| — | 0x32e7 | 13031 | — | fixed, 1 bytes | — |
| — | 0x32e8 | 13032 | — | variable | — |
| — | 0x32e9 | 13033 | — | variable | — |
| — | 0x32ea | 13034 | — | fixed, 10 bytes | — |
| — | 0x32eb | 13035 | — | empty | — |
| — | 0x32ec | 13036 | — | fixed, 0 bytes | — |
| — | 0x32ed | 13037 | — | variable | — |
| — | 0x32ee | 13038 | — | variable | — |
| — | 0x32ef | 13039 | — | variable | — |
| — | 0x32f0 | 13040 | — | fixed, 21 bytes | — |
| — | 0x32f1 | 13041 | — | fixed, 4 bytes | — |
| CMSG_REPORT_SERVER_LAG | 0x32f2 | 13042 | — | empty | no handler |
| — | 0x32f3 | 13043 | — | fixed, 4 bytes | — |
| CMSG_OFFER_PETITION | 0x32f4 | 13044 | 451 (0x1c3) | fixed, 4 bytes | matches |
| — | 0x32f5 | 13045 | — | variable | — |
| — | 0x32f6 | 13046 | — | empty | — |
| — | 0x32f7 | 13047 | — | variable | — |
| — | 0x32f8 | 13048 | — | variable | — |
| — | 0x32f9 | 13049 | — | fixed, 32 bytes | — |
| — | 0x3476 | 13430 | — | fixed, 8 bytes | — |
| — | 0x3477 | 13431 | — | fixed, 8 bytes | — |
| — | 0x3478 | 13432 | — | fixed, 4 bytes | — |
| — | 0x3479 | 13433 | — | fixed, 8 bytes | — |
| — | 0x347a | 13434 | — | fixed, 12 bytes | — |
| — | 0x347b | 13435 | — | empty | — |
| — | 0x347c | 13436 | — | variable | — |
| — | 0x347d | 13437 | — | variable | — |
| — | 0x347e | 13438 | — | variable | — |
| — | 0x347f | 13439 | — | variable | — |
| — | 0x3480 | 13440 | — | fixed, 0 bytes | — |
| — | 0x3481 | 13441 | — | fixed, 4 bytes | — |
| — | 0x3482 | 13442 | — | fixed, 4 bytes | — |
| — | 0x3483 | 13443 | — | fixed, 4 bytes | — |
| — | 0x3484 | 13444 | — | fixed, 8 bytes | — |
| — | 0x3485 | 13445 | — | fixed, 4 bytes | — |
| — | 0x3486 | 13446 | — | fixed, 4 bytes | — |
| — | 0x3487 | 13447 | — | fixed, 4 bytes | — |
| CMSG_SEND_TEXT_EMOTE | 0x3488 | 13448 | 260 (0x104) | variable | matches |
| CMSG_SET_SHEATHED | 0x3489 | 13449 | 480 (0x1e0) | variable | matches |
| CMSG_PET_SET_ACTION | 0x348a | 13450 | 372 (0x174) | variable | matches |
| CMSG_PET_ACTION | 0x348b | 13451 | 373 (0x175) | fixed, 16 bytes | matches |
| CMSG_PET_STOP_ATTACK | 0x348c | 13452 | 746 (0x2ea) | fixed, 0 bytes | matches |
| CMSG_PET_ABANDON | 0x348d | 13453 | 374 (0x176) | fixed, 0 bytes | matches |
| CMSG_PET_CANCEL_AURA | 0x348e | 13454 | 619 (0x26b) | fixed, 4 bytes | matches |
| CMSG_PET_SPELL_AUTOCAST | 0x348f | 13455 | 755 (0x2f3) | variable | no handler |
| CMSG_REQUEST_PET_INFO | 0x3490 | 13456 | 633 (0x279) | empty | matches |
| CMSG_REQUEST_STABLED_PETS | 0x3491 | 13457 | — | fixed, 0 bytes | matches |
| CMSG_TALK_TO_GOSSIP | 0x3492 | 13458 | 379 (0x17b) | fixed, 0 bytes | matches |
| CMSG_CLOSE_INTERACTION | 0x3493 | 13459 | — | fixed, 0 bytes | matches |
| CMSG_GOSSIP_SELECT_OPTION | 0x3494 | 13460 | 380 (0x17c) | variable | matches |
| CMSG_SPELL_CLICK | 0x3495 | 13461 | 1015 (0x3f7) | variable | matches |
| CMSG_QUEST_GIVER_HELLO | 0x3496 | 13462 | 388 (0x184) | fixed, 0 bytes | matches |
| CMSG_QUEST_GIVER_QUERY_QUEST | 0x3497 | 13463 | 390 (0x186) | variable | matches |
| CMSG_QUEST_GIVER_ACCEPT_QUEST | 0x3498 | 13464 | 393 (0x189) | variable | matches |
| CMSG_QUEST_GIVER_COMPLETE_QUEST | 0x3499 | 13465 | 394 (0x18a) | variable | matches |
| CMSG_QUEST_GIVER_CHOOSE_REWARD | 0x349a | 13466 | 398 (0x18e) | variable | matches |
| CMSG_QUEST_GIVER_REQUEST_REWARD | 0x349b | 13467 | 396 (0x18c) | fixed, 4 bytes | matches |
| CMSG_QUEST_GIVER_STATUS_QUERY | 0x349c | 13468 | 386 (0x182) | fixed, 0 bytes | matches |
| CMSG_QUEST_GIVER_STATUS_MULTIPLE_QUERY | 0x349d | 13469 | 1046 (0x416) | empty | matches |
| CMSG_QUEST_CONFIRM_ACCEPT | 0x349e | 13470 | 411 (0x19b) | fixed, 4 bytes | matches |
| CMSG_PUSH_QUEST_TO_PARTY | 0x349f | 13471 | 413 (0x19d) | fixed, 4 bytes | matches |
| CMSG_QUEST_PUSH_RESULT | 0x34a0 | 13472 | — | fixed, 5 bytes | matches |
| CMSG_LIST_INVENTORY | 0x34a1 | 13473 | 414 (0x19e) | fixed, 0 bytes | matches |
| CMSG_SELL_ITEM | 0x34a2 | 13474 | 416 (0x1a0) | fixed, 4 bytes | matches |
| CMSG_BUY_ITEM | 0x34a3 | 13475 | 418 (0x1a2) | variable | matches |
| CMSG_BUY_BACK_ITEM | 0x34a4 | 13476 | 656 (0x290) | fixed, 4 bytes | matches |
| — | 0x34a5 | 13477 | — | empty | — |
| — | 0x34a6 | 13478 | — | empty | — |
| — | 0x34a7 | 13479 | — | empty | — |
| CMSG_TAXI_NODE_STATUS_QUERY | 0x34a8 | 13480 | 426 (0x1aa) | fixed, 0 bytes | matches |
| CMSG_ENABLE_TAXI_NODE | 0x34a9 | 13481 | — | fixed, 0 bytes | matches |
| CMSG_TAXI_QUERY_AVAILABLE_NODES | 0x34aa | 13482 | 428 (0x1ac) | fixed, 0 bytes | matches |
| CMSG_ACTIVATE_TAXI | 0x34ab | 13483 | 429 (0x1ad) | fixed, 12 bytes | matches |
| CMSG_TAXI_REQUEST_EARLY_LANDING | 0x34ac | 13484 | — | empty | no handler |
| CMSG_TRAINER_LIST | 0x34ad | 13485 | 432 (0x1b0) | fixed, 0 bytes | matches |
| CMSG_TRAINER_BUY_SPELL | 0x34ae | 13486 | 434 (0x1b2) | fixed, 8 bytes | matches |
| CMSG_SPIRIT_HEALER_ACTIVATE | 0x34af | 13487 | 540 (0x21c) | fixed, 0 bytes | matches |
| CMSG_AREA_SPIRIT_HEALER_QUERY | 0x34b0 | 13488 | 738 (0x2e2) | fixed, 0 bytes | matches |
| CMSG_AREA_SPIRIT_HEALER_QUEUE | 0x34b1 | 13489 | 739 (0x2e3) | fixed, 0 bytes | matches |
| CMSG_BINDER_ACTIVATE | 0x34b2 | 13490 | 437 (0x1b5) | fixed, 0 bytes | matches |
| CMSG_BANKER_ACTIVATE | 0x34b3 | 13491 | 439 (0x1b7) | fixed, 0 bytes | matches |
| CMSG_BUY_BANK_SLOT | 0x34b4 | 13492 | 441 (0x1b9) | fixed, 0 bytes | matches |
| CMSG_GUILD_BANK_ACTIVATE | 0x34b5 | 13493 | 997 (0x3e5) | variable | matches |
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
| CMSG_GUILD_BANK_QUERY_TAB | 0x34c2 | 13506 | 998 (0x3e6) | variable | matches |
| CMSG_GUILD_BANK_BUY_TAB | 0x34c3 | 13507 | 1001 (0x3e9) | fixed, 1 bytes | matches |
| CMSG_GUILD_BANK_UPDATE_TAB | 0x34c4 | 13508 | 1002 (0x3ea) | variable | matches |
| CMSG_GUILD_BANK_DEPOSIT_MONEY | 0x34c5 | 13509 | 1003 (0x3eb) | fixed, 8 bytes | matches |
| CMSG_GUILD_BANK_WITHDRAW_MONEY | 0x34c6 | 13510 | 1004 (0x3ec) | fixed, 8 bytes | matches |
| CMSG_PETITION_SHOW_LIST | 0x34c7 | 13511 | 443 (0x1bb) | fixed, 0 bytes | no handler |
| CMSG_PETITION_BUY | 0x34c8 | 13512 | 445 (0x1bd) | variable | matches |
| CMSG_PETITION_SHOW_SIGNATURES | 0x34c9 | 13513 | 446 (0x1be) | fixed, 0 bytes | matches |
| CMSG_AUCTION_HELLO_REQUEST | 0x34ca | 13514 | — | fixed, 0 bytes | matches |
| CMSG_AUCTION_SELL_ITEM | 0x34cb | 13515 | 598 (0x256) | variable | matches |
| CMSG_AUCTION_REMOVE_ITEM | 0x34cc | 13516 | 599 (0x257) | variable | matches |
| CMSG_AUCTION_LIST_ITEMS | 0x34cd | 13517 | 600 (0x258) | variable | not checked |
| CMSG_AUCTION_REPLICATE_ITEMS | 0x34ce | 13518 | — | variable | no handler |
| CMSG_AUCTION_LIST_OWNED_ITEMS | 0x34cf | 13519 | 601 (0x259) | fixed, 4 bytes | matches |
| CMSG_AUCTION_LIST_BIDDED_ITEMS | 0x34d0 | 13520 | 612 (0x264) | variable | matches |
| CMSG_AUCTION_PLACE_BID | 0x34d1 | 13521 | 602 (0x25a) | variable | matches |
| CMSG_AUCTION_LIST_PENDING_SALES | 0x34d2 | 13522 | — | empty | no handler |
| — | 0x34d3 | 13523 | — | empty | — |
| — | 0x34d4 | 13524 | — | empty | — |
| CMSG_QUERY_TIME | 0x34d5 | 13525 | 462 (0x1ce) | empty | matches |
| CMSG_LOGOUT_REQUEST | 0x34d6 | 13526 | 75 (0x4b) | variable | matches |
| — | 0x34d7 | 13527 | — | empty | — |
| CMSG_LOGOUT_CANCEL | 0x34d8 | 13528 | 78 (0x4e) | empty | matches |
| CMSG_LOGOUT_INSTANT | 0x34d9 | 13529 | — | empty | no handler |
| — | 0x34da | 13530 | — | empty | — |
| CMSG_RECLAIM_CORPSE | 0x34db | 13531 | 466 (0x1d2) | fixed, 0 bytes | matches |
| — | 0x34dc | 13532 | — | fixed, 4 bytes | — |
| CMSG_COMPLETE_MOVIE | 0x34dd | 13533 | — | empty | no handler |
| CMSG_SET_FACTION_AT_WAR | 0x34de | 13534 | 293 (0x125) | fixed, 1 bytes | matches |
| CMSG_SET_FACTION_NOT_AT_WAR | 0x34df | 13535 | — | fixed, 1 bytes | matches |
| CMSG_SET_FACTION_INACTIVE | 0x34e0 | 13536 | 791 (0x317) | variable | matches |
| CMSG_SET_WATCHED_FACTION | 0x34e1 | 13537 | 792 (0x318) | fixed, 4 bytes | matches |
| CMSG_DUEL_RESPONSE | 0x34e2 | 13538 | — | variable | matches |
| — | 0x34e3 | 13539 | — | variable | — |
| — | 0x34e4 | 13540 | — | empty | — |
| CMSG_UNLEARN_SKILL | 0x34e5 | 13541 | 514 (0x202) | fixed, 4 bytes | matches |
| — | 0x34e6 | 13542 | — | variable | — |
| CMSG_CANCEL_AUTO_REPEAT_SPELL | 0x34e7 | 13543 | 621 (0x26d) | empty | matches |
| CMSG_FAR_SIGHT | 0x34e8 | 13544 | 634 (0x27a) | variable | matches |
| — | 0x34e9 | 13545 | — | variable | — |
| — | 0x34ea | 13546 | — | fixed, 4 bytes | — |
| CMSG_SOCKET_GEMS | 0x34eb | 13547 | 839 (0x347) | variable | matches |
| CMSG_REPAIR_ITEM | 0x34ec | 13548 | 680 (0x2a8) | variable | matches |
| — | 0x34ed | 13549 | — | fixed, 4 bytes | — |
| CMSG_GAME_OBJ_USE | 0x34ee | 13550 | 177 (0xb1) | fixed, 0 bytes | matches |
| CMSG_GAME_OBJ_REPORT_USE | 0x34ef | 13551 | — | fixed, 0 bytes | matches |
| — | 0x34f0 | 13552 | — | empty | — |
| — | 0x34f1 | 13553 | — | variable | — |
| CMSG_CANCEL_TEMP_ENCHANTMENT | 0x34f2 | 13554 | 889 (0x379) | fixed, 4 bytes | matches |
| CMSG_SET_TAXI_BENCHMARK_MODE | 0x34f3 | 13555 | 905 (0x389) | variable | no handler |
| CMSG_REPORT_PVP_PLAYER_AFK | 0x34f4 | 13556 | 995 (0x3e3) | fixed, 0 bytes | no handler |
| CMSG_ALTER_APPEARANCE | 0x34f5 | 13557 | — | variable | matches |
| CMSG_OPT_OUT_OF_LOOT | 0x34f6 | 13558 | 1032 (0x408) | variable | matches |
| — | 0x34f7 | 13559 | — | fixed, 4 bytes | — |
| CMSG_TOTEM_DESTROYED | 0x34f8 | 13560 | 1043 (0x413) | fixed, 1 bytes | matches |
| CMSG_DISMISS_CRITTER | 0x34f9 | 13561 | — | fixed, 0 bytes | matches |
| — | 0x34fa | 13562 | — | fixed, 4 bytes | — |
| — | 0x34fb | 13563 | — | variable | — |
| — | 0x34fc | 13564 | — | variable | — |
| — | 0x34fd | 13565 | — | empty | — |
| — | 0x34fe | 13566 | — | fixed, 4 bytes | — |
| — | 0x34ff | 13567 | — | fixed, 6 bytes | — |
| CMSG_QUERY_INSPECT_ACHIEVEMENTS | 0x3500 | 13568 | — | fixed, 0 bytes | no handler |
| — | 0x3501 | 13569 | — | variable | — |
| — | 0x3502 | 13570 | — | fixed, 20 bytes | — |
| — | 0x3503 | 13571 | — | empty | — |
| CMSG_RAF_CLAIM_ACTIVITY_REWARD | 0x3504 | 13572 | — | variable | no handler |
| — | 0x3505 | 13573 | — | fixed, 4 bytes | — |
| CMSG_HEARTH_AND_RESURRECT | 0x3506 | 13574 | — | empty | no handler |
| — | 0x3507 | 13575 | — | variable | — |
| — | 0x3508 | 13576 | — | fixed, 1 bytes | — |
| CMSG_SAVE_EQUIPMENT_SET | 0x3509 | 13577 | — | variable | matches |
| CMSG_DELETE_EQUIPMENT_SET | 0x350a | 13578 | — | fixed, 8 bytes | matches |
| CMSG_INSTANCE_LOCK_RESPONSE | 0x350b | 13579 | — | variable | matches |
| — | 0x350c | 13580 | — | empty | — |
| — | 0x350d | 13581 | — | variable | — |
| — | 0x350e | 13582 | — | empty | — |
| — | 0x350f | 13583 | — | variable | — |
| — | 0x3510 | 13584 | — | variable | — |
| — | 0x3511 | 13585 | — | variable | — |
| CMSG_LOW_LEVEL_RAID2 | 0x3512 | 13586 | — | variable | no handler |
| — | 0x3513 | 13587 | — | variable | — |
| — | 0x3514 | 13588 | — | variable | — |
| — | 0x3515 | 13589 | — | empty | — |
| — | 0x3516 | 13590 | — | empty | — |
| — | 0x3517 | 13591 | — | empty | — |
| — | 0x3518 | 13592 | — | empty | — |
| — | 0x3519 | 13593 | — | empty | — |
| — | 0x351a | 13594 | — | fixed, 8 bytes | — |
| — | 0x351b | 13595 | — | variable | — |
| — | 0x351c | 13596 | — | empty | — |
| CMSG_DECLINE_GUILD_INVITES | 0x351d | 13597 | — | variable | differs |
| — | 0x351e | 13598 | — | empty | — |
| CMSG_BATTLEMASTER_JOIN | 0x351f | 13599 | 750 (0x2ee) | variable | matches |
| CMSG_BATTLEMASTER_JOIN_ARENA | 0x3520 | 13600 | 856 (0x358) | fixed, 2 bytes | matches |
| CMSG_BATTLEMASTER_JOIN_SKIRMISH | 0x3521 | 13601 | — | variable | matches |
| CMSG_BATTLEMASTER_JOIN_BRAWL | 0x3522 | 13602 | — | fixed, 1 bytes | no handler |
| — | 0x3523 | 13603 | — | fixed, 5 bytes | — |
| CMSG_BATTLEFIELD_PORT | 0x3524 | 13604 | 725 (0x2d5) | variable | matches |
| CMSG_REPOP_REQUEST | 0x3525 | 13605 | 346 (0x15a) | variable | matches |
| CMSG_CLIENT_PORT_GRAVEYARD | 0x3526 | 13606 | — | empty | no handler |
| CMSG_SET_SELECTION | 0x3527 | 13607 | 317 (0x13d) | fixed, 0 bytes | matches |
| CMSG_INSPECT | 0x3528 | 13608 | 276 (0x114) | fixed, 0 bytes | matches |
| CMSG_REQUEST_CROWD_CONTROL_SPELL | 0x3529 | 13609 | — | fixed, 0 bytes | no handler |
| CMSG_BLACK_MARKET_OPEN | 0x352a | 13610 | — | fixed, 0 bytes | no handler |
| CMSG_BLACK_MARKET_REQUEST_ITEMS | 0x352b | 13611 | — | fixed, 8 bytes | no handler |
| CMSG_BLACK_MARKET_BID_ON_ITEM | 0x352c | 13612 | — | variable | no handler |
| CMSG_QUEST_LOG_REMOVE_QUEST | 0x352d | 13613 | 404 (0x194) | fixed, 1 bytes | matches |
| CMSG_GET_ITEM_PURCHASE_DATA | 0x352e | 13614 | — | fixed, 0 bytes | no handler |
| CMSG_ITEM_PURCHASE_REFUND | 0x352f | 13615 | — | fixed, 0 bytes | no handler |
| CMSG_SELF_RES | 0x3530 | 13616 | 691 (0x2b3) | fixed, 4 bytes | matches |
| CMSG_SET_ACTION_BAR_TOGGLES | 0x3531 | 13617 | 703 (0x2bf) | fixed, 1 bytes | matches |
| CMSG_SIGN_PETITION | 0x3532 | 13618 | 448 (0x1c0) | fixed, 1 bytes | matches |
| CMSG_DECLINE_PETITION | 0x3533 | 13619 | — | fixed, 0 bytes | matches |
| CMSG_TURN_IN_PETITION | 0x3534 | 13620 | 452 (0x1c4) | fixed, 20 bytes | reads the first part |
| CMSG_MAIL_GET_LIST | 0x3535 | 13621 | 570 (0x23a) | fixed, 0 bytes | matches |
| CMSG_MAIL_TAKE_MONEY | 0x3536 | 13622 | 581 (0x245) | fixed, 12 bytes | matches |
| CMSG_MAIL_TAKE_ITEM | 0x3537 | 13623 | 582 (0x246) | fixed, 8 bytes | matches |
| CMSG_QUERY_NEXT_MAIL_TIME | 0x3538 | 13624 | — | empty | matches |
| CMSG_MAIL_MARK_AS_READ | 0x3539 | 13625 | 583 (0x247) | fixed, 4 bytes | matches |
| CMSG_MAIL_CREATE_TEXT_ITEM | 0x353a | 13626 | 586 (0x24a) | fixed, 4 bytes | matches |
| — | 0x353b | 13627 | — | fixed, 4 bytes | — |
| — | 0x353c | 13628 | — | fixed, 4 bytes | — |
| — | 0x353d | 13629 | — | fixed, 4 bytes | — |
| CMSG_SET_LOOT_SPECIALIZATION | 0x353e | 13630 | — | fixed, 4 bytes | no handler |
| — | 0x353f | 13631 | — | variable | — |
| CMSG_EMOTE | 0x3540 | 13632 | 258 (0x102) | empty | no handler |
| — | 0x3541 | 13633 | — | fixed, 4 bytes | — |
| CMSG_OPENING_CINEMATIC | 0x3542 | 13634 | 249 (0xf9) | empty | matches |
| CMSG_NEXT_CINEMATIC_CAMERA | 0x3543 | 13635 | 251 (0xfb) | empty | matches |
| CMSG_COMPLETE_CINEMATIC | 0x3544 | 13636 | 252 (0xfc) | empty | matches |
| CMSG_CONVERSATION_LINE_STARTED | 0x3545 | 13637 | — | fixed, 4 bytes | no handler |
| — | 0x3546 | 13638 | — | fixed, 4 bytes | — |
| — | 0x3547 | 13639 | — | fixed, 8 bytes | — |
| CMSG_QUEST_GIVER_CLOSE_QUEST | 0x3548 | 13640 | — | fixed, 4 bytes | matches |
| CMSG_START_CHALLENGE_MODE | 0x3549 | 13641 | — | fixed, 5 bytes | no handler |
| — | 0x354a | 13642 | — | variable | — |
| — | 0x354b | 13643 | — | empty | — |
| — | 0x354c | 13644 | — | fixed, 4 bytes | — |
| — | 0x354d | 13645 | — | variable | — |
| — | 0x354e | 13646 | — | fixed, 4 bytes | — |
| — | 0x354f | 13647 | — | fixed, 4 bytes | — |
| — | 0x3550 | 13648 | — | empty | — |
| CMSG_LEARN_TALENT | 0x3551 | 13649 | 593 (0x251) | fixed, 6 bytes | matches |
| — | 0x3552 | 13650 | — | variable | — |
| — | 0x3553 | 13651 | — | variable | — |
| CMSG_LEARN_PVP_TALENTS | 0x3554 | 13652 | — | variable | no handler |
| CMSG_CONTRIBUTION_CONTRIBUTE | 0x3555 | 13653 | — | fixed, 4 bytes | no handler |
| CMSG_CONTRIBUTION_LAST_UPDATE_REQUEST | 0x3556 | 13654 | — | fixed, 8 bytes | no handler |
| — | 0x3557 | 13655 | — | fixed, 1 bytes | — |
| CMSG_SET_ACTION_BUTTON | 0x3558 | 13656 | 296 (0x128) | fixed, 5 bytes | differs |
| CMSG_SET_AMMO | 0x3559 | 13657 | 616 (0x268) | fixed, 4 bytes | matches |
| — | 0x355a | 13658 | — | empty | — |
| — | 0x355b | 13659 | — | variable | — |
| — | 0x355c | 13660 | — | variable | — |
| — | 0x355d | 13661 | — | fixed, 4 bytes | — |
| — | 0x355e | 13662 | — | fixed, 4 bytes | — |
| — | 0x355f | 13663 | — | fixed, 4 bytes | — |
| — | 0x3560 | 13664 | — | fixed, 4 bytes | — |
| — | 0x3561 | 13665 | — | fixed, 4 bytes | — |
| — | 0x3562 | 13666 | — | empty | — |
| CMSG_PLAYER_SHOWING_HELM | 0x3563 | 13667 | 697 (0x2b9) | variable | matches |
| CMSG_PLAYER_SHOWING_CLOAK | 0x3564 | 13668 | 698 (0x2ba) | variable | matches |
| CMSG_CONNECT_TO_FAILED | 0x35d4 | 13780 | — | fixed, 5 bytes | no handler |
| — | 0x35d5 | 13781 | — | variable | — |
| — | 0x35d6 | 13782 | — | variable | — |
| — | 0x35d7 | 13783 | — | fixed, 0 bytes | — |
| CMSG_ADDON_LIST | 0x35d8 | 13784 | — | variable | no handler |
| CMSG_SET_ROLE | 0x35d9 | 13785 | — | fixed, 5 bytes | matches |
| CMSG_INITIATE_ROLE_POLL | 0x35da | 13786 | — | fixed, 1 bytes | no handler |
| — | 0x35db | 13787 | — | variable | — |
| — | 0x35dc | 13788 | — | fixed, 4 bytes | — |
| CMSG_REQUEST_BATTLEFIELD_STATUS | 0x35dd | 13789 | — | empty | matches |
| — | 0x35de | 13790 | — | fixed, 12 bytes | — |
| CMSG_START_WAR_GAME | 0x35df | 13791 | — | variable | no handler |
| CMSG_START_SPECTATOR_WAR_GAME | 0x35e0 | 13792 | — | variable | no handler |
| CMSG_ACCEPT_WARGAME_INVITE | 0x35e1 | 13793 | — | variable | no handler |
| — | 0x35e2 | 13794 | — | empty | — |
| — | 0x35e3 | 13795 | — | fixed, 1 bytes | — |
| CMSG_REQUEST_RATED_PVP_INFO | 0x35e4 | 13796 | — | empty | matches |
| CMSG_DB_QUERY_BULK | 0x35e5 | 13797 | — | variable | matches |
| CMSG_HOTFIX_REQUEST | 0x35e6 | 13798 | — | variable | matches |
| — | 0x35e7 | 13799 | — | variable | — |
| CMSG_GENERATE_RANDOM_CHARACTER_NAME | 0x35e8 | 13800 | — | fixed, 2 bytes | matches |
| CMSG_ENUM_CHARACTERS | 0x35e9 | 13801 | 55 (0x37) | empty | matches |
| CMSG_REORDER_CHARACTERS | 0x35ea | 13802 | — | variable | matches |
| CMSG_PLAYER_LOGIN | 0x35eb | 13803 | 61 (0x3d) | variable | matches |
| — | 0x35ec | 13804 | — | fixed, 4 bytes | — |
| CMSG_WARDEN3_DATA | 0x35ed | 13805 | — | variable | no handler |
| — | 0x35ee | 13806 | — | variable | — |
| CMSG_GET_PVP_OPTIONS_ENABLED | 0x35ef | 13807 | — | empty | no handler |
| CMSG_COMMENTATOR_START_WARGAME | 0x35f0 | 13808 | — | variable | no handler |
| CMSG_COMMENTATOR_ENABLE | 0x35f1 | 13809 | 948 (0x3b4) | fixed, 4 bytes | no handler |
| CMSG_COMMENTATOR_GET_MAP_INFO | 0x35f2 | 13810 | 950 (0x3b6) | variable | no handler |
| CMSG_COMMENTATOR_GET_PLAYER_INFO | 0x35f3 | 13811 | 952 (0x3b8) | fixed, 11 bytes | no handler |
| CMSG_COMMENTATOR_GET_PLAYER_COOLDOWNS | 0x35f4 | 13812 | — | variable | no handler |
| CMSG_COMMENTATOR_ENTER_INSTANCE | 0x35f5 | 13813 | 955 (0x3bb) | fixed, 23 bytes | no handler |
| CMSG_COMMENTATOR_EXIT_INSTANCE | 0x35f6 | 13814 | 956 (0x3bc) | empty | no handler |
| — | 0x35f7 | 13815 | — | empty | — |
| CMSG_REQUEST_PARTY_JOIN_UPDATES | 0x35f8 | 13816 | — | fixed, 1 bytes | no handler |
| CMSG_LOADING_SCREEN_NOTIFY | 0x35f9 | 13817 | — | variable | matches |
| CMSG_WORLD_PORT_RESPONSE | 0x35fa | 13818 | — | empty | matches |
| CMSG_SEND_MAIL | 0x35fb | 13819 | 568 (0x238) | variable | matches |
| — | 0x35fc | 13820 | — | fixed, 4 bytes | — |
| CMSG_ACCEPT_GUILD_INVITE | 0x35fd | 13821 | 132 (0x84) | empty | matches |
| — | 0x35fe | 13822 | — | fixed, 0 bytes | — |
| — | 0x35ff | 13823 | — | variable | — |
| — | 0x3600 | 13824 | — | empty | — |
| — | 0x3601 | 13825 | — | empty | — |
| — | 0x3602 | 13826 | — | empty | — |
| CMSG_PARTY_INVITE | 0x3603 | 13827 | 110 (0x6e) | variable | matches |
| — | 0x3604 | 13828 | — | variable | — |
| CMSG_PARTY_INVITE_RESPONSE | 0x3605 | 13829 | — | variable | matches |
| — | 0x3606 | 13830 | — | empty | — |
| CMSG_GUILD_INVITE_BY_NAME | 0x3607 | 13831 | 130 (0x82) | variable | matches |
| CMSG_DF_PROPOSAL_RESPONSE | 0x3608 | 13832 | — | variable | matches |
| CMSG_DF_CONFIRM_EXPAND_SEARCH | 0x3609 | 13833 | — | variable | no handler |
| CMSG_DF_JOIN | 0x360a | 13834 | — | variable | differs |
| CMSG_LFG_LIST_LEAVE | 0x360b | 13835 | — | fixed, 16 bytes | no handler |
| CMSG_LFG_LIST_GET_STATUS | 0x360c | 13836 | — | empty | matches |
| CMSG_LFG_LIST_SEARCH | 0x360d | 13837 | — | variable | no handler |
| CMSG_LFG_LIST_APPLY_TO_GROUP | 0x360e | 13838 | — | variable | no handler |
| CMSG_LFG_LIST_CANCEL_APPLICATION | 0x360f | 13839 | — | fixed, 16 bytes | no handler |
| CMSG_LFG_LIST_DECLINE_APPLICANT | 0x3610 | 13840 | — | fixed, 32 bytes | no handler |
| CMSG_LFG_LIST_INVITE_APPLICANT | 0x3611 | 13841 | — | variable | no handler |
| CMSG_LFG_LIST_INVITE_RESPONSE | 0x3612 | 13842 | — | variable | no handler |
| CMSG_DF_LEAVE | 0x3613 | 13843 | — | fixed, 16 bytes | reads the first part |
| CMSG_DF_GET_SYSTEM_INFO | 0x3614 | 13844 | — | variable | matches |
| CMSG_DF_GET_JOIN_STATUS | 0x3615 | 13845 | — | empty | matches |
| CMSG_DF_SET_ROLES | 0x3616 | 13846 | — | fixed, 5 bytes | differs |
| CMSG_DF_BOOT_PLAYER_VOTE | 0x3617 | 13847 | — | variable | no handler |
| CMSG_DF_TELEPORT | 0x3618 | 13848 | — | variable | matches |
| CMSG_SET_EVERYONE_IS_ASSISTANT | 0x3619 | 13849 | — | variable | matches |
| CMSG_DF_READY_CHECK_RESPONSE | 0x361a | 13850 | — | variable | no handler |
| — | 0x361b | 13851 | — | empty | — |
| — | 0x361c | 13852 | — | fixed, 0 bytes | — |
| CMSG_LF_GUILD_ADD_RECRUIT | 0x361d | 13853 | — | variable | no handler |
| CMSG_LF_GUILD_SET_GUILD_POST | 0x361e | 13854 | — | variable | no handler |
| CMSG_LF_GUILD_BROWSE | 0x361f | 13855 | — | fixed, 16 bytes | no handler |
| — | 0x3620 | 13856 | — | empty | — |
| — | 0x3621 | 13857 | — | variable | — |
| — | 0x3622 | 13858 | — | variable | — |
| — | 0x3623 | 13859 | — | variable | — |
| CMSG_BATTLE_PET_REQUEST_JOURNAL_LOCK | 0x3624 | 13860 | — | empty | no handler |
| CMSG_BATTLE_PET_REQUEST_JOURNAL | 0x3625 | 13861 | — | empty | matches |
| CMSG_BATTLE_PET_DELETE_PET | 0x3626 | 13862 | — | fixed, 0 bytes | no handler |
| CMSG_BATTLE_PET_DELETE_PET_CHEAT | 0x3627 | 13863 | — | fixed, 0 bytes | no handler |
| — | 0x3628 | 13864 | — | empty | — |
| CMSG_BATTLE_PET_MODIFY_NAME | 0x3629 | 13865 | — | variable | no handler |
| CMSG_BATTLE_PET_SUMMON | 0x362a | 13866 | — | fixed, 0 bytes | matches |
| — | 0x362b | 13867 | — | variable | — |
| — | 0x362c | 13868 | — | variable | — |
| — | 0x362d | 13869 | — | fixed, 1 bytes | — |
| CMSG_BATTLE_PET_SET_BATTLE_SLOT | 0x362e | 13870 | — | fixed, 1 bytes | no handler |
| — | 0x362f | 13871 | — | fixed, 5 bytes | — |
| — | 0x3630 | 13872 | — | variable | — |
| — | 0x3631 | 13873 | — | variable | — |
| CMSG_BATTLE_PET_SET_FLAGS | 0x3632 | 13874 | — | variable | differs |
| — | 0x3633 | 13875 | — | variable | — |
| CMSG_COLLECTION_ITEM_SET_FAVORITE | 0x3634 | 13876 | — | variable | matches |
| CMSG_DO_READY_CHECK | 0x3635 | 13877 | — | fixed, 1 bytes | matches |
| CMSG_READY_CHECK_RESPONSE | 0x3636 | 13878 | — | variable | matches |
| — | 0x3637 | 13879 | — | fixed, 9 bytes | — |
| — | 0x3638 | 13880 | — | variable | — |
| — | 0x3639 | 13881 | — | variable | — |
| — | 0x363a | 13882 | — | variable | — |
| — | 0x363b | 13883 | — | variable | — |
| — | 0x363c | 13884 | — | fixed, 8 bytes | — |
| — | 0x363d | 13885 | — | variable | — |
| — | 0x363e | 13886 | — | variable | — |
| — | 0x363f | 13887 | — | variable | — |
| — | 0x3640 | 13888 | — | variable | — |
| — | 0x3641 | 13889 | — | fixed, 1 bytes | — |
| CMSG_PET_BATTLE_INPUT | 0x3642 | 13890 | — | variable | no handler |
| CMSG_PET_BATTLE_REPLACE_FRONT_PET | 0x3643 | 13891 | — | fixed, 1 bytes | no handler |
| — | 0x3644 | 13892 | — | fixed, 4 bytes | — |
| CMSG_CREATE_CHARACTER | 0x3645 | 13893 | 54 (0x36) | variable | matches |
| CMSG_CHECK_CHARACTER_NAME_AVAILABILITY | 0x3646 | 13894 | — | variable | no handler |
| CMSG_SUPPORT_TICKET_SUBMIT_COMPLAINT | 0x3647 | 13895 | — | variable | differs |
| CMSG_SUPPORT_TICKET_SUBMIT_BUG | 0x3648 | 13896 | — | variable | no handler |
| CMSG_SUPPORT_TICKET_SUBMIT_SUGGESTION | 0x3649 | 13897 | — | variable | no handler |
| CMSG_PARTY_UNINVITE | 0x364a | 13898 | — | variable | matches |
| CMSG_SET_LOOT_METHOD | 0x364b | 13899 | 122 (0x7a) | fixed, 6 bytes | matches |
| CMSG_LEAVE_GROUP | 0x364c | 13900 | — | fixed, 1 bytes | matches |
| CMSG_SET_PARTY_LEADER | 0x364d | 13901 | 120 (0x78) | fixed, 1 bytes | matches |
| CMSG_MINIMAP_PING | 0x364e | 13902 | — | fixed, 9 bytes | matches |
| CMSG_GROUP_CHANGE_SUB_GROUP | 0x364f | 13903 | 638 (0x27e) | fixed, 2 bytes | matches |
| CMSG_GROUP_SWAP_SUB_GROUP | 0x3650 | 13904 | 640 (0x280) | fixed, 1 bytes | matches |
| CMSG_CONVERT_RAID | 0x3651 | 13905 | 654 (0x28e) | variable | matches |
| CMSG_SET_ASSISTANT_LEADER | 0x3652 | 13906 | 655 (0x28f) | variable | matches |
| CMSG_UPDATE_RAID_TARGET | 0x3653 | 13907 | — | fixed, 2 bytes | matches |
| CMSG_SET_PARTY_ASSIGNMENT | 0x3654 | 13908 | — | variable | no handler |
| CMSG_SILENCE_PARTY_TALKER | 0x3655 | 13909 | — | variable | no handler |
| CMSG_REQUEST_PARTY_MEMBER_STATS | 0x3656 | 13910 | 639 (0x27f) | fixed, 1 bytes | matches |
| CMSG_RANDOM_ROLL | 0x3657 | 13911 | — | fixed, 9 bytes | matches |
| CMSG_MAIL_RETURN_TO_SENDER | 0x3658 | 13912 | 584 (0x248) | fixed, 4 bytes | matches |
| CMSG_TOGGLE_DIFFICULTY | 0x3659 | 13913 | — | empty | no handler |
| — | 0x365a | 13914 | — | variable | — |
| — | 0x365b | 13915 | — | variable | — |
| CMSG_ADD_BATTLENET_FRIEND | 0x365c | 13916 | — | variable | no handler |
| — | 0x365d | 13917 | — | fixed, 20 bytes | — |
| — | 0x365e | 13918 | — | fixed, 0 bytes | — |
| — | 0x365f | 13919 | — | fixed, 4 bytes | — |
| — | 0x3660 | 13920 | — | empty | — |
| — | 0x3661 | 13921 | — | empty | — |
| CMSG_QUERY_CORPSE_LOCATION_FROM_CLIENT | 0x3662 | 13922 | — | fixed, 0 bytes | matches |
| CMSG_QUERY_CORPSE_TRANSPORT | 0x3663 | 13923 | — | fixed, 0 bytes | no handler |
| CMSG_CAN_DUEL | 0x3664 | 13924 | — | fixed, 0 bytes | matches |
| — | 0x3665 | 13925 | — | variable | — |
| CMSG_UPDATE_CLIENT_SETTINGS | 0x3666 | 13926 | — | fixed, 4 bytes | no handler |
| — | 0x3667 | 13927 | — | variable | — |
| — | 0x3668 | 13928 | — | variable | — |
| — | 0x3669 | 13929 | — | empty | — |
| CMSG_RESET_INSTANCES | 0x366a | 13930 | 797 (0x31d) | empty | matches |
| — | 0x366b | 13931 | — | empty | — |
| CMSG_SUMMON_RESPONSE | 0x366c | 13932 | 684 (0x2ac) | variable | matches |
| — | 0x366d | 13933 | — | variable | — |
| CMSG_COMPLAINT | 0x366e | 13934 | 966 (0x3c6) | variable | no handler |
| — | 0x366f | 13935 | — | variable | — |
| — | 0x3670 | 13936 | — | variable | — |
| CMSG_CALENDAR_GET | 0x3671 | 13937 | — | empty | no handler |
| CMSG_CALENDAR_GET_EVENT | 0x3672 | 13938 | — | fixed, 8 bytes | no handler |
| CMSG_CALENDAR_COMMUNITY_INVITE | 0x3673 | 13939 | — | fixed, 11 bytes | no handler |
| CMSG_CALENDAR_INVITE | 0x3674 | 13940 | — | variable | no handler |
| CMSG_CALENDAR_REMOVE_INVITE | 0x3675 | 13941 | — | fixed, 24 bytes | no handler |
| CMSG_CALENDAR_RSVP | 0x3676 | 13942 | — | fixed, 17 bytes | no handler |
| CMSG_CALENDAR_STATUS | 0x3677 | 13943 | — | fixed, 25 bytes | no handler |
| CMSG_CALENDAR_MODERATOR_STATUS | 0x3678 | 13944 | — | fixed, 25 bytes | no handler |
| CMSG_CALENDAR_REMOVE_EVENT | 0x3679 | 13945 | — | fixed, 28 bytes | no handler |
| CMSG_CALENDAR_COPY_EVENT | 0x367a | 13946 | — | fixed, 28 bytes | no handler |
| CMSG_CALENDAR_COMPLAIN | 0x367b | 13947 | — | fixed, 16 bytes | no handler |
| CMSG_CALENDAR_GET_NUM_PENDING | 0x367c | 13948 | — | empty | no handler |
| CMSG_CALENDAR_EVENT_SIGN_UP | 0x367d | 13949 | — | variable | no handler |
| — | 0x367e | 13950 | — | variable | — |
| CMSG_CALENDAR_ADD_EVENT | 0x367f | 13951 | — | variable | no handler |
| CMSG_CALENDAR_UPDATE_EVENT | 0x3680 | 13952 | — | variable | no handler |
| CMSG_KEEP_ALIVE | 0x3681 | 13953 | 1030 (0x406) | empty | no handler |
| CMSG_WHO_IS | 0x3682 | 13954 | 100 (0x64) | variable | no handler |
| CMSG_WHO | 0x3683 | 13955 | 98 (0x62) | variable | matches |
| CMSG_SET_DUNGEON_DIFFICULTY | 0x3684 | 13956 | — | fixed, 4 bytes | matches |
| CMSG_RESURRECT_RESPONSE | 0x3685 | 13957 | 348 (0x15c) | fixed, 4 bytes | matches |
| CMSG_PET_RENAME | 0x3686 | 13958 | 375 (0x177) | variable | matches |
| CMSG_BUG_REPORT | 0x3687 | 13959 | — | variable | no handler |
| — | 0x3688 | 13960 | — | empty | — |
| CMSG_SET_PLAYER_DECLINED_NAMES | 0x3689 | 13961 | 1048 (0x418) | variable | matches |
| CMSG_QUERY_REALM_NAME | 0x368a | 13962 | — | fixed, 4 bytes | no handler |
| CMSG_QUERY_GUILD_INFO | 0x368b | 13963 | 84 (0x54) | fixed, 0 bytes | matches |
| CMSG_REQUEST_CHARACTER_GUILD_FOLLOW_INFO | 0x368c | 13964 | — | variable | no handler |
| CMSG_CHAR_CUSTOMIZE | 0x368d | 13965 | — | variable | no handler |
| CMSG_GM_TICKET_GET_SYSTEM_STATUS | 0x368e | 13966 | 538 (0x21a) | empty | matches |
| CMSG_GM_TICKET_GET_CASE_STATUS | 0x368f | 13967 | — | empty | matches |
| CMSG_GM_TICKET_ACKNOWLEDGE_SURVEY | 0x3690 | 13968 | — | fixed, 4 bytes | no handler |
| — | 0x3691 | 13969 | — | variable | — |
| CMSG_CHAR_RACE_OR_FACTION_CHANGE | 0x3692 | 13970 | — | variable | no handler |
| CMSG_SUBMIT_USER_FEEDBACK | 0x3693 | 13971 | — | variable | no handler |
| CMSG_REQUEST_ACCOUNT_DATA | 0x3694 | 13972 | 522 (0x20a) | variable | differs |
| CMSG_UPDATE_ACCOUNT_DATA | 0x3695 | 13973 | 523 (0x20b) | variable | differs |
| — | 0x3696 | 13974 | — | variable | — |
| — | 0x3697 | 13975 | — | variable | — |
| — | 0x3698 | 13976 | — | fixed, 6 bytes | — |
| — | 0x3699 | 13977 | — | fixed, 8 bytes | — |
| CMSG_SERVER_TIME_OFFSET_REQUEST | 0x369a | 13978 | — | empty | no handler |
| CMSG_CHAR_DELETE | 0x369b | 13979 | 56 (0x38) | fixed, 0 bytes | matches |
| — | 0x369c | 13980 | — | fixed, 0 bytes | — |
| — | 0x369d | 13981 | — | fixed, 0 bytes | — |
| — | 0x369e | 13982 | — | variable | — |
| CMSG_LOW_LEVEL_RAID1 | 0x369f | 13983 | — | variable | no handler |
| — | 0x36a0 | 13984 | — | empty | — |
| CMSG_INSPECT_PVP | 0x36a1 | 13985 | — | fixed, 0 bytes | matches |
| CMSG_ARENA_TEAM_QUERY | 0x36a2 | 13986 | 843 (0x34b) | fixed, 4 bytes | matches |
| — | 0x36a3 | 13987 | — | fixed, 0 bytes | — |
| — | 0x36a4 | 13988 | — | variable | — |
| — | 0x36a5 | 13989 | — | variable | — |
| — | 0x36a6 | 13990 | — | variable | — |
| — | 0x36a7 | 13991 | — | variable | — |
| — | 0x36a8 | 13992 | — | variable | — |
| — | 0x36a9 | 13993 | — | variable | — |
| — | 0x36aa | 13994 | — | variable | — |
| — | 0x36ab | 13995 | — | empty | — |
| — | 0x36ac | 13996 | — | fixed, 8 bytes | — |
| CMSG_QUEST_POI_QUERY | 0x36ad | 13997 | — | fixed, 4 bytes | differs |
| — | 0x36ae | 13998 | — | variable | — |
| — | 0x36af | 13999 | — | variable | — |
| — | 0x36b0 | 14000 | — | fixed, 4 bytes | — |
| — | 0x36b1 | 14001 | — | variable | — |
| — | 0x36b2 | 14002 | — | fixed, 4 bytes | — |
| — | 0x36b3 | 14003 | — | variable | — |
| — | 0x36b4 | 14004 | — | variable | — |
| CMSG_ARENA_TEAM_ROSTER | 0x36b5 | 14005 | 845 (0x34d) | fixed, 4 bytes | matches |
| CMSG_ARENA_TEAM_ACCEPT | 0x36b6 | 14006 | 849 (0x351) | fixed, 0 bytes | matches |
| CMSG_ARENA_TEAM_DECLINE | 0x36b7 | 14007 | 850 (0x352) | fixed, 0 bytes | matches |
| CMSG_ARENA_TEAM_LEAVE | 0x36b8 | 14008 | 851 (0x353) | fixed, 4 bytes | matches |
| CMSG_ARENA_TEAM_REMOVE | 0x36b9 | 14009 | 852 (0x354) | fixed, 4 bytes | matches |
| CMSG_ARENA_TEAM_DISBAND | 0x36ba | 14010 | 853 (0x355) | fixed, 4 bytes | matches |
| CMSG_ARENA_TEAM_LEADER | 0x36bb | 14011 | 854 (0x356) | fixed, 4 bytes | matches |
| CMSG_GET_ACCOUNT_CHARACTER_LIST | 0x36bc | 14012 | — | variable | matches |
| CMSG_LIVE_REGION_GET_ACCOUNT_CHARACTER_LIST | 0x36bd | 14013 | — | variable | no handler |
| CMSG_LIVE_REGION_CHARACTER_COPY | 0x36be | 14014 | — | variable | no handler |
| CMSG_LIVE_REGION_ACCOUNT_RESTORE | 0x36bf | 14015 | — | variable | no handler |
| CMSG_LIVE_REGION_KEY_BINDINGS_COPY | 0x36c0 | 14016 | — | variable | no handler |
| CMSG_BATTLE_PAY_GET_PRODUCT_LIST | 0x36c1 | 14017 | — | empty | no handler |
| CMSG_BATTLE_PAY_GET_PURCHASE_LIST | 0x36c2 | 14018 | — | empty | no handler |
| — | 0x36c3 | 14019 | — | variable | — |
| — | 0x36c4 | 14020 | — | fixed, 16 bytes | — |
| — | 0x36c5 | 14021 | — | fixed, 0 bytes | — |
| CMSG_CHARACTER_RENAME_REQUEST | 0x36c6 | 14022 | 711 (0x2c7) | variable | matches |
| CMSG_SHOW_TRADE_SKILL | 0x36c7 | 14023 | — | fixed, 8 bytes | no handler |
| CMSG_BATTLE_PAY_DISTRIBUTION_ASSIGN_TO_TARGET | 0x36c8 | 14024 | — | fixed, 16 bytes | no handler |
| CMSG_CHARACTER_UPGRADE_MANUAL_UNREVOKE_REQUEST | 0x36c9 | 14025 | — | fixed, 0 bytes | no handler |
| CMSG_CHARACTER_UPGRADE_START | 0x36ca | 14026 | — | fixed, 4 bytes | no handler |
| CMSG_CHARACTER_CHECK_UPGRADE | 0x36cb | 14027 | — | empty | no handler |
| — | 0x36cc | 14028 | — | fixed, 16 bytes | — |
| CMSG_GUILD_SET_GUILD_MASTER | 0x36cd | 14029 | 144 (0x90) | variable | matches |
| CMSG_PETITION_RENAME_GUILD | 0x36ce | 14030 | — | variable | matches |
| CMSG_REQUEST_RAID_INFO | 0x36cf | 14031 | 717 (0x2cd) | empty | matches |
| CMSG_BATTLE_PAY_START_PURCHASE | 0x36d0 | 14032 | — | variable | no handler |
| CMSG_BATTLE_PAY_CONFIRM_PURCHASE_RESPONSE | 0x36d1 | 14033 | — | variable | no handler |
| CMSG_BATTLE_PAY_ACK_FAILED_RESPONSE | 0x36d2 | 14034 | — | fixed, 4 bytes | no handler |
| — | 0x36d3 | 14035 | — | fixed, 8 bytes | — |
| CMSG_CONTACT_LIST | 0x36d4 | 14036 | 102 (0x66) | fixed, 4 bytes | matches |
| CMSG_ADD_FRIEND | 0x36d5 | 14037 | 105 (0x69) | variable | matches |
| CMSG_DEL_FRIEND | 0x36d6 | 14038 | 106 (0x6a) | fixed, 4 bytes | matches |
| CMSG_SET_CONTACT_NOTES | 0x36d7 | 14039 | 107 (0x6b) | variable | matches |
| CMSG_BATTLENET_CHALLENGE_RESPONSE | 0x36d8 | 14040 | — | variable | no handler |
| CMSG_ADD_IGNORE | 0x36d9 | 14041 | 108 (0x6c) | variable | matches |
| CMSG_DEL_IGNORE | 0x36da | 14042 | 109 (0x6d) | fixed, 4 bytes | matches |
| — | 0x36db | 14043 | — | variable | — |
| — | 0x36dc | 14044 | — | variable | — |
| — | 0x36dd | 14045 | — | variable | — |
| — | 0x36de | 14046 | — | variable | — |
| — | 0x36df | 14047 | — | fixed, 4 bytes | — |
| CMSG_SET_RAID_DIFFICULTY | 0x36e0 | 14048 | — | fixed, 5 bytes | reads the first part |
| CMSG_TUTORIAL_FLAG | 0x36e1 | 14049 | 254 (0xfe) | variable | matches |
| CMSG_ENUM_CHARACTERS_DELETED_BY_CLIENT | 0x36e2 | 14050 | — | empty | no handler |
| CMSG_UNDELETE_CHARACTER | 0x36e3 | 14051 | — | fixed, 4 bytes | no handler |
| CMSG_GET_UNDELETE_CHARACTER_COOLDOWN_STATUS | 0x36e4 | 14052 | — | empty | no handler |
| — | 0x36e5 | 14053 | — | empty | — |
| — | 0x36e6 | 14054 | — | empty | — |
| — | 0x36e7 | 14055 | — | fixed, 4 bytes | — |
| — | 0x36e8 | 14056 | — | variable | — |
| CMSG_COMMERCE_TOKEN_GET_COUNT | 0x36e9 | 14057 | — | fixed, 4 bytes | no handler |
| CMSG_COMMERCE_TOKEN_GET_MARKET_PRICE | 0x36ea | 14058 | — | fixed, 4 bytes | no handler |
| CMSG_AUCTIONABLE_TOKEN_SELL | 0x36eb | 14059 | — | fixed, 20 bytes | no handler |
| CMSG_AUCTIONABLE_TOKEN_SELL_AT_MARKET_PRICE | 0x36ec | 14060 | — | variable | no handler |
| CMSG_CONSUMABLE_TOKEN_CAN_VETERAN_BUY | 0x36ed | 14061 | — | fixed, 4 bytes | no handler |
| CMSG_CONSUMABLE_TOKEN_BUY | 0x36ee | 14062 | — | fixed, 12 bytes | no handler |
| CMSG_CONSUMABLE_TOKEN_BUY_AT_MARKET_PRICE | 0x36ef | 14063 | — | variable | no handler |
| CMSG_GET_REMAINING_GAME_TIME | 0x36f0 | 14064 | — | fixed, 4 bytes | no handler |
| CMSG_CONSUMABLE_TOKEN_REDEEM | 0x36f1 | 14065 | — | fixed, 16 bytes | no handler |
| CMSG_CONSUMABLE_TOKEN_REDEEM_CONFIRMATION | 0x36f2 | 14066 | — | variable | no handler |
| CMSG_COMMERCE_TOKEN_GET_LOG | 0x36f3 | 14067 | — | fixed, 4 bytes | no handler |
| — | 0x36f4 | 14068 | — | empty | — |
| CMSG_GET_VAS_ACCOUNT_CHARACTER_LIST | 0x36f5 | 14069 | — | fixed, 8 bytes | no handler |
| CMSG_GET_VAS_TRANSFER_TARGET_REALM_LIST | 0x36f6 | 14070 | — | fixed, 8 bytes | no handler |
| CMSG_BATTLE_PAY_START_VAS_PURCHASE | 0x36f7 | 14071 | — | variable | no handler |
| CMSG_UPDATE_VAS_PURCHASE_STATES | 0x36f8 | 14072 | — | empty | no handler |
| — | 0x36f9 | 14073 | — | empty | — |
| CMSG_BATTLENET_REQUEST | 0x36fa | 14074 | — | variable | matches |
| — | 0x36fb | 14075 | — | variable | — |
| CMSG_CLUB_PRESENCE_SUBSCRIBE | 0x36fc | 14076 | — | variable | no handler |
| — | 0x36fd | 14077 | — | variable | — |
| CMSG_CHANGE_REALM_TICKET | 0x36fe | 14078 | — | variable | matches |
| CMSG_SEND_CHARACTER_CLUB_INVITATION | 0x36ff | 14079 | — | empty | no handler |
| — | 0x3700 | 14080 | — | empty | — |
| — | 0x3701 | 14081 | — | fixed, 8 bytes | — |
| — | 0x3702 | 14082 | — | empty | — |
| — | 0x3703 | 14083 | — | fixed, 4 bytes | — |
| — | 0x3704 | 14084 | — | fixed, 4 bytes | — |
| CMSG_REPORT_ENABLED_ADDONS | 0x3705 | 14085 | — | variable | no handler |
| CMSG_REPORT_CLIENT_VARIABLES | 0x3706 | 14086 | — | variable | no handler |
| CMSG_REPORT_KEYBINDING_EXECUTION_COUNTS | 0x3707 | 14087 | — | variable | no handler |
| — | 0x3708 | 14088 | — | variable | — |
| CMSG_QUICK_JOIN_SIGNAL_TOAST_DISPLAYED | 0x3709 | 14089 | — | variable | no handler |
| CMSG_QUICK_JOIN_RESPOND_TO_INVITE | 0x370a | 14090 | — | variable | no handler |
| CMSG_QUICK_JOIN_AUTO_ACCEPT_REQUESTS | 0x370b | 14091 | — | variable | no handler |
| CMSG_CAN_REDEEM_TOKEN_FOR_BALANCE | 0x370c | 14092 | — | fixed, 4 bytes | no handler |
| CMSG_BATTLE_PAY_REQUEST_PRICE_INFO | 0x370d | 14093 | — | fixed, 8 bytes | no handler |
| CMSG_VAS_GET_SERVICE_STATUS | 0x370e | 14094 | — | empty | no handler |
| CMSG_VAS_GET_QUEUE_MINUTES | 0x370f | 14095 | — | fixed, 12 bytes | no handler |
| CMSG_VAS_CHECK_TRANSFER_OK | 0x3710 | 14096 | — | variable | no handler |
| CMSG_BATTLE_PAY_OPEN_CHECKOUT | 0x3711 | 14097 | — | fixed, 4 bytes | no handler |
| — | 0x3712 | 14098 | — | empty | — |
| CMSG_VOICE_CHAT_LOGIN | 0x3713 | 14099 | — | empty | no handler |
| CMSG_VOICE_CHANNEL_STT_TOKEN_REQUEST | 0x3714 | 14100 | — | variable | no handler |
| CMSG_VOICE_CHAT_JOIN_CHANNEL | 0x3715 | 14101 | — | fixed, 1 bytes | no handler |
| — | 0x3716 | 14102 | — | variable | — |
| — | 0x3717 | 14103 | — | variable | — |
| CMSG_BATTLE_PAY_CANCEL_OPEN_CHECKOUT | 0x3718 | 14104 | — | variable | no handler |
| — | 0x3719 | 14105 | — | variable | — |
| — | 0x371a | 14106 | — | fixed, 8 bytes | — |
| — | 0x371b | 14107 | — | variable | — |
| — | 0x371c | 14108 | — | fixed, 0 bytes | — |
| CMSG_DO_COUNTDOWN | 0x371d | 14109 | — | fixed, 5 bytes | no handler |
| CMSG_CLUB_FINDER_POST | 0x371e | 14110 | — | variable | no handler |
| CMSG_CLUB_FINDER_REQUEST_CLUBS_LIST | 0x371f | 14111 | — | variable | no handler |
| CMSG_CLUB_FINDER_REQUEST_MEMBERSHIP_TO_CLUB | 0x3720 | 14112 | — | variable | no handler |
| CMSG_CLUB_FINDER_GET_APPLICANTS_LIST | 0x3721 | 14113 | — | variable | no handler |
| CMSG_CLUB_FINDER_RESPOND_TO_APPLICANT | 0x3722 | 14114 | — | variable | no handler |
| CMSG_CLUB_FINDER_APPLICATION_RESPONSE | 0x3723 | 14115 | — | variable | no handler |
| CMSG_CLUB_FINDER_REQUEST_PENDING_CLUBS_LIST | 0x3724 | 14116 | — | variable | no handler |
| CMSG_CLUB_FINDER_REQUEST_CLUBS_DATA | 0x3725 | 14117 | — | variable | no handler |
| CMSG_CLUB_FINDER_REQUEST_SUBSCRIBED_CLUB_POSTING_IDS | 0x3726 | 14118 | — | variable | no handler |
| CMSG_GET_RAF_ACCOUNT_INFO | 0x3727 | 14119 | — | fixed, 4 bytes | no handler |
| CMSG_RAF_CLAIM_NEXT_REWARD | 0x3728 | 14120 | — | fixed, 8 bytes | no handler |
| CMSG_RAF_UPDATE_RECRUITMENT_INFO | 0x3729 | 14121 | — | fixed, 4 bytes | no handler |
| CMSG_RAF_GENERATE_RECRUITMENT_LINK | 0x372a | 14122 | — | fixed, 4 bytes | no handler |
| CMSG_REMOVE_RAF_RECRUIT | 0x372b | 14123 | — | fixed, 8 bytes | no handler |
| — | 0x372c | 14124 | — | fixed, 8 bytes | — |
| — | 0x372d | 14125 | — | fixed, 4 bytes | — |
| — | 0x372e | 14126 | — | empty | — |
| — | 0x372f | 14127 | — | empty | — |
| — | 0x3730 | 14128 | — | variable | — |
| — | 0x3731 | 14129 | — | empty | — |
| CMSG_QUEST_SESSION_REQUEST_STOP | 0x3732 | 14130 | — | empty | no handler |
| — | 0x3733 | 14131 | — | fixed, 0 bytes | — |
| — | 0x3734 | 14132 | — | variable | — |
| — | 0x3735 | 14133 | — | empty | — |
| — | 0x3736 | 14134 | — | empty | — |
| CMSG_QUICK_JOIN_REQUEST_INVITE_WITH_CONFIRMATION | 0x3737 | 14135 | — | variable | no handler |
| — | 0x3738 | 14136 | — | variable | — |
| CMSG_AUCTION_SET_FAVORITE_ITEM | 0x3739 | 14137 | — | variable | no handler |
| — | 0x373a | 14138 | — | variable | — |
| CMSG_SUSPEND_COMMS_ACK | 0x3764 | 14180 | — | fixed, 8 bytes | no handler |
| CMSG_AUTH_SESSION | 0x3765 | 14181 | 493 (0x1ed) | variable | no handler |
| CMSG_AUTH_CONTINUED_SESSION | 0x3766 | 14182 | — | variable | no handler |
| CMSG_ENTER_ENCRYPTED_MODE_ACK | 0x3767 | 14183 | — | empty | no handler |
| CMSG_PING | 0x3768 | 14184 | 476 (0x1dc) | fixed, 8 bytes | no handler |
| CMSG_LOG_DISCONNECT | 0x3769 | 14185 | — | fixed, 4 bytes | no handler |
| CMSG_SUSPEND_TOKEN_RESPONSE | 0x376a | 14186 | — | fixed, 4 bytes | no handler |
| CMSG_ENABLE_NAGLE | 0x376b | 14187 | — | empty | no handler |
| CMSG_QUEUED_MESSAGES_END | 0x376c | 14188 | — | fixed, 4 bytes | no handler |
| CMSG_LOG_STREAMING_ERROR | 0x376d | 14189 | — | variable | no handler |
| — | 0x376e | 14190 | — | variable | — |
| CMSG_QUERY_PLAYER_NAME | 0x376f | 14191 | — | fixed, 0 bytes | matches |
| CMSG_QUERY_PLAYER_NAME_BY_COMMUNITY_ID | 0x3770 | 14192 | — | fixed, 8 bytes | no handler |
| CMSG_QUERY_PLAYER_NAMES_FOR_COMMUNITY | 0x3771 | 14193 | — | variable | no handler |
| CMSG_CHAT_JOIN_CHANNEL | 0x37c8 | 14280 | 151 (0x97) | variable | differs |
| CMSG_CHAT_LEAVE_CHANNEL | 0x37c9 | 14281 | 152 (0x98) | variable | matches |
| — | 0x37ca | 14282 | — | variable | — |
| CMSG_CHAT_REPORT_IGNORED | 0x37cb | 14283 | 549 (0x225) | fixed, 1 bytes | no handler |
| CMSG_CHAT_REPORT_FILTERED | 0x37cc | 14284 | 817 (0x331) | fixed, 0 bytes | no handler |
| CMSG_CHAT_REGISTER_ADDON_PREFIXES | 0x37cd | 14285 | — | variable | matches |
| CMSG_CHAT_UNREGISTER_ALL_ADDON_PREFIXES | 0x37ce | 14286 | — | empty | matches |
| CMSG_CHAT_MESSAGE_CHANNEL | 0x37cf | 14287 | — | variable | matches |
| CMSG_CHAT_MESSAGE_WHISPER | 0x37d0 | 14288 | — | variable | matches |
| CMSG_CHAT_MESSAGE_GUILD | 0x37d1 | 14289 | — | variable | matches |
| CMSG_CHAT_MESSAGE_OFFICER | 0x37d2 | 14290 | — | variable | matches |
| CMSG_CHAT_MESSAGE_AFK | 0x37d3 | 14291 | — | variable | matches |
| CMSG_CHAT_MESSAGE_DND | 0x37d4 | 14292 | — | variable | matches |
| CMSG_CHAT_CHANNEL_LIST | 0x37d5 | 14293 | 154 (0x9a) | variable | matches |
| CMSG_CHAT_CHANNEL_DISPLAY_LIST | 0x37d6 | 14294 | 977 (0x3d1) | variable | matches |
| CMSG_CHAT_CHANNEL_PASSWORD | 0x37d7 | 14295 | 156 (0x9c) | variable | differs |
| CMSG_CHAT_CHANNEL_SET_OWNER | 0x37d8 | 14296 | 157 (0x9d) | variable | matches |
| CMSG_CHAT_CHANNEL_OWNER | 0x37d9 | 14297 | 158 (0x9e) | variable | matches |
| — | 0x37da | 14298 | — | variable | — |
| CMSG_CHAT_CHANNEL_MODERATOR | 0x37db | 14299 | 159 (0x9f) | variable | matches |
| CMSG_CHAT_CHANNEL_UNMODERATOR | 0x37dc | 14300 | 160 (0xa0) | variable | matches |
| — | 0x37dd | 14301 | — | variable | — |
| — | 0x37de | 14302 | — | variable | — |
| CMSG_CHAT_CHANNEL_INVITE | 0x37df | 14303 | 163 (0xa3) | variable | matches |
| CMSG_CHAT_CHANNEL_KICK | 0x37e0 | 14304 | 164 (0xa4) | variable | matches |
| CMSG_CHAT_CHANNEL_BAN | 0x37e1 | 14305 | 165 (0xa5) | variable | matches |
| CMSG_CHAT_CHANNEL_UNBAN | 0x37e2 | 14306 | 166 (0xa6) | variable | matches |
| CMSG_CHAT_CHANNEL_ANNOUNCEMENTS | 0x37e3 | 14307 | 167 (0xa7) | variable | matches |
| CMSG_CHAT_CHANNEL_SILENCE_ALL | 0x37e4 | 14308 | 972 (0x3cc) | variable | no handler |
| CMSG_CHAT_CHANNEL_UNSILENCE_ALL | 0x37e5 | 14309 | 974 (0x3ce) | variable | no handler |
| CMSG_CHAT_CHANNEL_DECLINE_INVITE | 0x37e6 | 14310 | 1039 (0x40f) | variable | matches |
| CMSG_CHAT_MESSAGE_SAY | 0x37e7 | 14311 | — | variable | matches |
| CMSG_CHAT_MESSAGE_EMOTE | 0x37e8 | 14312 | — | variable | matches |
| CMSG_CHAT_MESSAGE_YELL | 0x37e9 | 14313 | — | variable | matches |
| CMSG_CHAT_MESSAGE_PARTY | 0x37ea | 14314 | — | variable | matches |
| CMSG_CHAT_MESSAGE_RAID | 0x37eb | 14315 | — | variable | matches |
| CMSG_CHAT_MESSAGE_INSTANCE_CHAT | 0x37ec | 14316 | — | variable | matches |
| CMSG_CHAT_MESSAGE_RAID_WARNING | 0x37ed | 14317 | — | variable | matches |
| CMSG_CHAT_ADDON_MESSAGE | 0x37ee | 14318 | — | variable | matches |
| CMSG_CHAT_ADDON_MESSAGE_TARGETED | 0x37ef | 14319 | — | variable | matches |
| — | 0x37f0 | 14320 | — | empty | — |
| — | 0x37f1 | 14321 | — | empty | — |
| — | 0x37f2 | 14322 | — | fixed, 7 bytes | — |
| — | 0x37f3 | 14323 | — | fixed, 16 bytes | — |
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
| CMSG_MOVE_START_FORWARD | 0x39e4 | 14820 | — | variable | differs |
| CMSG_MOVE_START_BACKWARD | 0x39e5 | 14821 | — | variable | differs |
| CMSG_MOVE_STOP | 0x39e6 | 14822 | — | variable | differs |
| CMSG_MOVE_START_STRAFE_LEFT | 0x39e7 | 14823 | — | variable | differs |
| CMSG_MOVE_START_STRAFE_RIGHT | 0x39e8 | 14824 | — | variable | differs |
| CMSG_MOVE_STOP_STRAFE | 0x39e9 | 14825 | — | variable | differs |
| CMSG_MOVE_JUMP | 0x39ea | 14826 | — | variable | differs |
| CMSG_MOVE_DOUBLE_JUMP | 0x39eb | 14827 | — | variable | differs |
| CMSG_MOVE_START_TURN_LEFT | 0x39ec | 14828 | — | variable | differs |
| CMSG_MOVE_START_TURN_RIGHT | 0x39ed | 14829 | — | variable | differs |
| CMSG_MOVE_STOP_TURN | 0x39ee | 14830 | — | variable | differs |
| CMSG_MOVE_START_PITCH_UP | 0x39ef | 14831 | — | variable | differs |
| CMSG_MOVE_START_PITCH_DOWN | 0x39f0 | 14832 | — | variable | differs |
| CMSG_MOVE_STOP_PITCH | 0x39f1 | 14833 | — | variable | differs |
| CMSG_MOVE_SET_RUN_MODE | 0x39f2 | 14834 | — | variable | differs |
| CMSG_MOVE_SET_WALK_MODE | 0x39f3 | 14835 | — | variable | differs |
| — | 0x39f4 | 14836 | — | fixed, 24 bytes | — |
| — | 0x39f5 | 14837 | — | fixed, 0 bytes | — |
| — | 0x39f6 | 14838 | — | fixed, 24 bytes | — |
| — | 0x39f7 | 14839 | — | fixed, 24 bytes | — |
| CMSG_MOVE_TELEPORT_ACK | 0x39f8 | 14840 | — | fixed, 8 bytes | matches |
| CMSG_MOVE_FALL_LAND | 0x39f9 | 14841 | — | variable | differs |
| CMSG_MOVE_START_SWIM | 0x39fa | 14842 | — | variable | differs |
| CMSG_MOVE_STOP_SWIM | 0x39fb | 14843 | — | variable | differs |
| — | 0x39fc | 14844 | — | fixed, 4 bytes | — |
| — | 0x39fd | 14845 | — | fixed, 4 bytes | — |
| — | 0x39fe | 14846 | — | fixed, 4 bytes | — |
| — | 0x39ff | 14847 | — | fixed, 4 bytes | — |
| — | 0x3a00 | 14848 | — | fixed, 4 bytes | — |
| — | 0x3a01 | 14849 | — | fixed, 4 bytes | — |
| — | 0x3a02 | 14850 | — | fixed, 4 bytes | — |
| — | 0x3a03 | 14851 | — | fixed, 4 bytes | — |
| CMSG_MOVE_SET_TURN_RATE_CHEAT | 0x3a04 | 14852 | — | fixed, 4 bytes | no handler |
| — | 0x3a05 | 14853 | — | variable | — |
| — | 0x3a06 | 14854 | — | variable | — |
| CMSG_MOVE_SET_FACING | 0x3a07 | 14855 | — | variable | differs |
| CMSG_MOVE_SET_PITCH | 0x3a08 | 14856 | — | variable | differs |
| CMSG_MOVE_FORCE_RUN_SPEED_CHANGE_ACK | 0x3a09 | 14857 | 227 (0xe3) | variable | differs |
| CMSG_MOVE_FORCE_RUN_BACK_SPEED_CHANGE_ACK | 0x3a0a | 14858 | 229 (0xe5) | variable | differs |
| CMSG_MOVE_FORCE_SWIM_SPEED_CHANGE_ACK | 0x3a0b | 14859 | 231 (0xe7) | variable | differs |
| CMSG_MOVE_FORCE_ROOT_ACK | 0x3a0c | 14860 | 233 (0xe9) | variable | differs |
| CMSG_MOVE_FORCE_UNROOT_ACK | 0x3a0d | 14861 | 235 (0xeb) | variable | differs |
| CMSG_MOVE_HEARTBEAT | 0x3a0e | 14862 | — | variable | differs |
| — | 0x3a0f | 14863 | — | variable | — |
| CMSG_MOVE_KNOCK_BACK_ACK | 0x3a10 | 14864 | 240 (0xf0) | variable | differs |
| CMSG_MOVE_HOVER_ACK | 0x3a11 | 14865 | 246 (0xf6) | variable | differs |
| CMSG_MOVE_SET_VEHICLE_REC_ID_ACK | 0x3a12 | 14866 | — | variable | differs |
| CMSG_MOVE_APPLY_MOVEMENT_FORCE_ACK | 0x3a13 | 14867 | — | variable | no handler |
| CMSG_MOVE_REMOVE_MOVEMENT_FORCE_ACK | 0x3a14 | 14868 | — | variable | no handler |
| CMSG_MOVE_REMOVE_MOVEMENT_FORCES | 0x3a15 | 14869 | — | variable | differs |
| CMSG_MOVE_SPLINE_DONE | 0x3a16 | 14870 | 713 (0x2c9) | variable | differs |
| CMSG_MOVE_FALL_RESET | 0x3a17 | 14871 | 714 (0x2ca) | variable | differs |
| CMSG_MOVE_UPDATE_FALL_SPEED | 0x3a18 | 14872 | — | variable | no handler |
| CMSG_MOVE_TIME_SKIPPED | 0x3a19 | 14873 | 718 (0x2ce) | fixed, 4 bytes | matches |
| CMSG_MOVE_FEATHER_FALL_ACK | 0x3a1a | 14874 | 719 (0x2cf) | variable | differs |
| CMSG_MOVE_WATER_WALK_ACK | 0x3a1b | 14875 | 720 (0x2d0) | variable | differs |
| CMSG_MOVE_ENABLE_DOUBLE_JUMP_ACK | 0x3a1c | 14876 | — | variable | no handler |
| — | 0x3a1d | 14877 | — | variable | — |
| — | 0x3a1e | 14878 | — | variable | — |
| CMSG_MOVE_FORCE_WALK_SPEED_CHANGE_ACK | 0x3a1f | 14879 | 731 (0x2db) | variable | differs |
| CMSG_MOVE_FORCE_SWIM_BACK_SPEED_CHANGE_ACK | 0x3a20 | 14880 | 733 (0x2dd) | variable | differs |
| CMSG_MOVE_FORCE_TURN_RATE_CHANGE_ACK | 0x3a21 | 14881 | 735 (0x2df) | variable | differs |
| CMSG_MOVE_ENABLE_SWIM_TO_FLY_TRANS_ACK | 0x3a22 | 14882 | — | variable | no handler |
| CMSG_MOVE_SET_CAN_TURN_WHILE_FALLING_ACK | 0x3a23 | 14883 | — | variable | no handler |
| CMSG_MOVE_SET_IGNORE_MOVEMENT_FORCES_ACK | 0x3a24 | 14884 | — | variable | no handler |
| CMSG_MOVE_SET_CAN_FLY_ACK | 0x3a25 | 14885 | 837 (0x345) | variable | differs |
| CMSG_MOVE_SET_FLY | 0x3a26 | 14886 | 838 (0x346) | variable | differs |
| CMSG_MOVE_START_ASCEND | 0x3a27 | 14887 | — | variable | differs |
| CMSG_MOVE_STOP_ASCEND | 0x3a28 | 14888 | — | variable | differs |
| — | 0x3a29 | 14889 | — | fixed, 4 bytes | — |
| — | 0x3a2a | 14890 | — | fixed, 4 bytes | — |
| CMSG_MOVE_FORCE_FLIGHT_SPEED_CHANGE_ACK | 0x3a2b | 14891 | 898 (0x382) | variable | differs |
| CMSG_MOVE_FORCE_FLIGHT_BACK_SPEED_CHANGE_ACK | 0x3a2c | 14892 | 900 (0x384) | variable | differs |
| CMSG_MOVE_CHANGE_TRANSPORT | 0x3a2d | 14893 | 909 (0x38d) | variable | differs |
| CMSG_MOVE_START_DESCEND | 0x3a2e | 14894 | — | variable | differs |
| — | 0x3a2f | 14895 | — | fixed, 4 bytes | — |
| CMSG_MOVE_FORCE_PITCH_RATE_CHANGE_ACK | 0x3a30 | 14896 | — | variable | differs |
| CMSG_MOVE_DISMISS_VEHICLE | 0x3a31 | 14897 | — | variable | reads the first part |
| CMSG_MOVE_CHANGE_VEHICLE_SEATS | 0x3a32 | 14898 | — | variable | no handler |
| CMSG_MOVE_GRAVITY_DISABLE_ACK | 0x3a33 | 14899 | — | variable | differs |
| CMSG_MOVE_GRAVITY_ENABLE_ACK | 0x3a34 | 14900 | — | variable | differs |
| CMSG_MOVE_COLLISION_DISABLE_ACK | 0x3a35 | 14901 | — | variable | no handler |
| CMSG_MOVE_COLLISION_ENABLE_ACK | 0x3a36 | 14902 | — | variable | no handler |
| CMSG_MOVE_SET_COLLISION_HEIGHT_ACK | 0x3a37 | 14903 | — | variable | differs |
| CMSG_SET_ACTIVE_MOVER | 0x3a38 | 14904 | 618 (0x26a) | fixed, 0 bytes | matches |
| CMSG_TIME_SYNC_RESPONSE | 0x3a39 | 14905 | 913 (0x391) | fixed, 8 bytes | matches |
| CMSG_TIME_SYNC_RESPONSE_FAILED | 0x3a3a | 14906 | — | fixed, 4 bytes | no handler |
| CMSG_TIME_SYNC_RESPONSE_DROPPED | 0x3a3b | 14907 | — | fixed, 8 bytes | no handler |
| CMSG_TIME_ADJUSTMENT_RESPONSE | 0x3a3c | 14908 | — | fixed, 8 bytes | no handler |
| CMSG_DISCARDED_TIME_SYNC_ACKS | 0x3a3d | 14909 | — | fixed, 4 bytes | no handler |
| CMSG_MOVE_SET_MOD_MOVEMENT_FORCE_MAGNITUDE_ACK | 0x3a3e | 14910 | — | variable | no handler |
| CMSG_UPDATE_MISSILE_TRAJECTORY | 0x3a3f | 14911 | — | variable | no handler |
| CMSG_MOVE_SEAMLESS_TRANSFER_COMPLETE | 0x3a40 | 14912 | — | variable | no handler |
| — | 0x3a41 | 14913 | — | empty | — |
| CMSG_MOVE_INIT_ACTIVE_MOVER_COMPLETE | 0x3a42 | 14914 | — | fixed, 4 bytes | matches |
| — | 0x3a43 | 14915 | — | fixed, 4 bytes | — |
| — | 0x3a44 | 14916 | — | variable | — |
| — | 0x3a45 | 14917 | — | empty | — |
