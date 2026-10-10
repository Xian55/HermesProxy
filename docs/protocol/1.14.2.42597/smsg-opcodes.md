# Server opcodes (SMSG), 1.14.2.42597

Every server-to-client opcode of the client, with its 1.12.1 counterpart where HermesProxy's universal name exists in both. Layouts are in [smsg-structures](smsg-structures/README.md).

| Name | Hex | Dec | 1.12.1 | Area | Layout | HermesProxy |
|---|---|---:|---|---|---|---|
| SMSG_AUTH_FAILED | 0x256c | 9580 | — | General | fixed struct | — |
| SMSG_AUTH_RESPONSE | 0x256d | 9581 | 494 (0x1ee) | General | variable | differs |
| SMSG_WAIT_QUEUE_UPDATE | 0x256e | 9582 | — | General | fixed, 9 bytes | matches |
| SMSG_WAIT_QUEUE_FINISH | 0x256f | 9583 | — | General | fixed struct | matches |
| SMSG_ALL_ACHIEVEMENT_DATA | 0x2570 | 9584 | — | General | variable | differs |
| SMSG_ALL_ACCOUNT_CRITERIA | 0x2571 | 9585 | — | General | variable | matches |
| SMSG_RESPOND_INSPECT_ACHIEVEMENTS | 0x2572 | 9586 | — | General | variable | — |
| SMSG_SETUP_CURRENCY | 0x2573 | 9587 | — | General | variable | matches |
| SMSG_SET_CURRENCY | 0x2574 | 9588 | — | General | variable | — |
| SMSG_RESET_WEEKLY_CURRENCY | 0x2575 | 9589 | — | General | fixed struct | — |
| SMSG_MESSAGE_BOX | 0x2576 | 9590 | — | General | variable | — |
| SMSG_WARDEN3_DATA | 0x2577 | 9591 | — | General | variable | — |
| SMSG_PHASE_SHIFT_CHANGE | 0x2578 | 9592 | — | General | variable | matches |
| SMSG_PRELOAD_CHILD_MAP | 0x2579 | 9593 | — | General | fixed struct | — |
| SMSG_UNLOAD_CHILD_MAP | 0x257a | 9594 | — | General | fixed struct | — |
| SMSG_MOUNT_RESULT | 0x257b | 9595 | 366 (0x16e) | General | fixed struct | — |
| SMSG_DISMOUNT_RESULT | 0x257c | 9596 | 367 (0x16f) | General | fixed struct | — |
| SMSG_BIND_POINT_UPDATE | 0x257d | 9597 | 341 (0x155) | General | fixed struct | matches |
| SMSG_RESURRECT_REQUEST | 0x257e | 9598 | 347 (0x15b) | General | variable | matches |
| SMSG_CLEAR_RESURRECT | 0x257f | 9599 | — | General | ignored by the client | — |
| SMSG_INITIAL_SETUP | 0x2580 | 9600 | — | General | fixed struct | matches |
| SMSG_TRADE_UPDATED | 0x2581 | 9601 | — | General | variable | matches |
| SMSG_TRADE_STATUS | 0x2582 | 9602 | 288 (0x120) | General | variable | matches |
| SMSG_ENUM_CHARACTERS_RESULT | 0x2583 | 9603 | 59 (0x3b) | General | variable | matches |
| SMSG_CHECK_CHARACTER_NAME_AVAILABILITY_RESULT | 0x2584 | 9604 | — | General | ignored by the client | — |
| SMSG_GENERATE_RANDOM_CHARACTER_NAME_RESULT | 0x2585 | 9605 | — | General | variable | matches |
| SMSG_SETUP_RESEARCH_HISTORY | 0x2586 | 9606 | — | General | variable | — |
| SMSG_RESEARCH_COMPLETE | 0x2587 | 9607 | — | General | fixed, 16 bytes | — |
| SMSG_ARCHAEOLOGY_SURVERY_CAST | 0x2588 | 9608 | — | General | fixed, 13 bytes | — |
| SMSG_PET_NEWLY_TAMED | 0x2589 | 9609 | — | General | fixed, 1 bytes | — |
| — | 0x258a | 9610 | — | General | ignored by the client | — |
| SMSG_PET_SLOT_UPDATED | 0x258b | 9611 | — | General | fixed struct | — |
| SMSG_PET_MODE | 0x258c | 9612 | 378 (0x17a) | General | fixed, 3 bytes | — |
| SMSG_DIFFERENT_INSTANCE_FROM_PARTY | 0x258d | 9613 | — | General | fixed struct | — |
| SMSG_ROLE_CHANGED_INFORM | 0x258e | 9614 | — | General | fixed, 9 bytes | differs |
| SMSG_ROLE_POLL_INFORM | 0x258f | 9615 | — | General | fixed, 1 bytes | — |
| SMSG_SUMMON_RAID_MEMBER_VALIDATE_FAILED | 0x2590 | 9616 | — | General | variable | — |
| SMSG_GROUP_ACTION_THROTTLED | 0x2591 | 9617 | — | General | fixed struct | — |
| SMSG_REQUEST_CEMETERY_LIST_RESPONSE | 0x2592 | 9618 | — | General | variable | — |
| SMSG_CHECK_WARGAME_ENTRY | 0x2593 | 9619 | — | General | fixed, 17 bytes | — |
| — | 0x2594 | 9620 | — | General | ignored by the client | — |
| — | 0x2595 | 9621 | — | General | ignored by the client | — |
| SMSG_PET_ADDED | 0x2596 | 9622 | — | General | variable | — |
| SMSG_PET_STABLE_LIST | 0x2597 | 9623 | — | General | variable | matches |
| SMSG_PET_STABLE_RESULT | 0x2598 | 9624 | 627 (0x273) | General | fixed struct | matches |
| SMSG_NEW_WORLD | 0x2599 | 9625 | 62 (0x3e) | General | fixed struct | matches |
| SMSG_LOGIN_VERIFY_WORLD | 0x259a | 9626 | 566 (0x236) | General | fixed struct | matches |
| SMSG_ABORT_NEW_WORLD | 0x259b | 9627 | — | General | fixed struct | — |
| SMSG_NOTIFY_MONEY | 0x259c | 9628 | — | General | fixed struct | — |
| SMSG_ITEM_PURCHASE_REFUND_RESULT | 0x259d | 9629 | — | General | variable | — |
| SMSG_SET_ITEM_PURCHASE_DATA | 0x259e | 9630 | — | General | variable | — |
| SMSG_ITEM_EXPIRE_PURCHASE_REFUND | 0x259f | 9631 | — | General | fixed, 0 bytes | — |
| SMSG_DISPLAY_GAME_ERROR | 0x25a0 | 9632 | — | General | variable | — |
| SMSG_SET_MAX_WEEKLY_QUANTITY | 0x25a1 | 9633 | — | General | fixed struct | — |
| SMSG_PETITION_ALREADY_SIGNED | 0x25a2 | 9634 | — | General | fixed, 0 bytes | — |
| SMSG_RAID_MARKERS_CHANGED | 0x25a3 | 9635 | — | General | variable | — |
| SMSG_STREAMING_MOVIES | 0x25a4 | 9636 | — | General | variable | — |
| SMSG_START_TIMER | 0x25a5 | 9637 | — | General | fixed struct | — |
| SMSG_DISENCHANT_CREDIT | 0x25a6 | 9638 | — | General | variable | — |
| SMSG_SUSPEND_TOKEN | 0x25a7 | 9639 | — | General | fixed, 5 bytes | matches |
| SMSG_RESUME_TOKEN | 0x25a8 | 9640 | — | General | fixed, 5 bytes | matches |
| SMSG_ADD_ITEM_PASSIVE | 0x25a9 | 9641 | — | General | fixed struct | — |
| SMSG_REMOVE_ITEM_PASSIVE | 0x25aa | 9642 | — | General | fixed struct | — |
| SMSG_SEND_ITEM_PASSIVES | 0x25ab | 9643 | — | General | variable | — |
| SMSG_WORLD_SERVER_INFO | 0x25ac | 9644 | — | General | variable | matches |
| SMSG_ACCOUNT_MOUNT_UPDATE | 0x25ad | 9645 | — | General | ignored by the client | matches |
| SMSG_ACCOUNT_TOY_UPDATE | 0x25ae | 9646 | — | General | variable | matches |
| SMSG_ACCOUNT_HEIRLOOM_UPDATE | 0x25af | 9647 | — | General | ignored by the client | matches |
| SMSG_ACCOUNT_TRANSMOG_UPDATE | 0x25b0 | 9648 | — | General | ignored by the client | — |
| SMSG_ACCOUNT_TRANSMOG_SET_FAVORITES_UPDATE | 0x25b1 | 9649 | — | General | ignored by the client | — |
| — | 0x25b2 | 9650 | — | General | ignored by the client | — |
| SMSG_RUNE_REGEN_DEBUG | 0x25b3 | 9651 | — | General | variable | — |
| — | 0x25b4 | 9652 | — | General | ignored by the client | — |
| SMSG_VENDOR_INVENTORY | 0x25b5 | 9653 | 415 (0x19f) | General | variable | matches |
| SMSG_RESTRICTED_ACCOUNT_WARNING | 0x25b6 | 9654 | — | General | ignored by the client | — |
| SMSG_SET_PLAY_HOVER_ANIM | 0x25b7 | 9655 | — | General | fixed, 1 bytes | — |
| SMSG_CLEAR_BOSS_EMOTES | 0x25b8 | 9656 | — | General | fixed struct | — |
| SMSG_LOAD_CUF_PROFILES | 0x25b9 | 9657 | — | General | variable | not checked |
| SMSG_PARTY_INVITE | 0x25ba | 9658 | 111 (0x6f) | General | variable | matches |
| — | 0x25bb | 9659 | — | General | ignored by the client | — |
| SMSG_FEATURE_SYSTEM_STATUS | 0x25bc | 9660 | — | General | variable | matches |
| SMSG_FEATURE_SYSTEM_STATUS_GLUE_SCREEN | 0x25bd | 9661 | — | General | variable | differs |
| SMSG_SEASON_INFO | 0x25be | 9662 | — | General | fixed, 21 bytes | matches |
| SMSG_GAME_OBJECT_ACTIVATE_ANIM_KIT | 0x25bf | 9663 | — | General | fixed, 5 bytes | — |
| SMSG_GAME_OBJECT_CUSTOM_ANIM | 0x25c0 | 9664 | 179 (0xb3) | General | fixed, 5 bytes | matches |
| SMSG_GAME_OBJECT_DESPAWN | 0x25c1 | 9665 | 533 (0x215) | General | fixed, 0 bytes | matches |
| SMSG_MAP_OBJ_EVENTS | 0x25c2 | 9666 | — | General | variable | — |
| SMSG_MISSILE_CANCEL | 0x25c3 | 9667 | — | General | fixed, 5 bytes | — |
| SMSG_CHAIN_MISSILE_BOUNCE | 0x25c4 | 9668 | — | General | ignored by the client | — |
| SMSG_XP_GAIN_ABORTED | 0x25c5 | 9669 | — | General | fixed, 12 bytes | — |
| SMSG_PRINT_NOTIFICATION | 0x25c6 | 9670 | 459 (0x1cb) | General | variable | matches |
| SMSG_CUSTOM_LOAD_SCREEN | 0x25c7 | 9671 | — | General | fixed struct | — |
| SMSG_SPELL_VISUAL_LOAD_SCREEN | 0x25c8 | 9672 | — | General | fixed struct | — |
| SMSG_TRANSFER_PENDING | 0x25c9 | 9673 | 63 (0x3f) | General | variable | matches |
| — | 0x25ca | 9674 | — | General | ignored by the client | — |
| — | 0x25cb | 9675 | — | General | ignored by the client | — |
| SMSG_ADJUST_SPLINE_DURATION | 0x25cc | 9676 | — | General | fixed, 4 bytes | — |
| SMSG_LEARN_TALENT_FAILED | 0x25cd | 9677 | — | General | variable | — |
| SMSG_LEARN_PVP_TALENT_FAILED | 0x25ce | 9678 | — | General | variable | — |
| — | 0x25cf | 9679 | — | General | ignored by the client | — |
| SMSG_UPDATE_TALENT_DATA | 0x25d0 | 9680 | — | General | variable | differs |
| SMSG_UPDATE_PRIMARY_SPEC | 0x25d1 | 9681 | — | General | fixed struct | — |
| — | 0x25d2 | 9682 | — | General | ignored by the client | — |
| — | 0x25d3 | 9683 | — | General | ignored by the client | — |
| SMSG_SHOW_NEUTRAL_PLAYER_FACTION_SELECT_UI | 0x25d4 | 9684 | — | General | fixed struct | — |
| SMSG_NEUTRAL_PLAYER_FACTION_SELECT_RESULT | 0x25d5 | 9685 | — | General | fixed, 5 bytes | — |
| — | 0x25d6 | 9686 | — | General | fixed struct | — |
| — | 0x25d7 | 9687 | — | General | ignored by the client | — |
| SMSG_SET_CHR_UPGRADE_TIER | 0x25d8 | 9688 | — | General | fixed struct | — |
| SMSG_UPDATE_ACTION_BUTTONS | 0x25d9 | 9689 | 297 (0x129) | General | ignored by the client | client takes a plain struct |
| SMSG_DONT_AUTO_PUSH_SPELLS_TO_ACTION_BAR | 0x25da | 9690 | — | General | fixed struct | — |
| SMSG_SCENE_OBJECT_EVENT | 0x25db | 9691 | — | General | variable | — |
| SMSG_SCENE_OBJECT_PET_BATTLE_INITIAL_UPDATE | 0x25dc | 9692 | — | General | variable | — |
| SMSG_SCENE_OBJECT_PET_BATTLE_FIRST_ROUND | 0x25dd | 9693 | — | General | variable | — |
| SMSG_SCENE_OBJECT_PET_BATTLE_ROUND_RESULT | 0x25de | 9694 | — | General | variable | — |
| SMSG_SCENE_OBJECT_PET_BATTLE_REPLACEMENTS_MADE | 0x25df | 9695 | — | General | variable | — |
| SMSG_SCENE_OBJECT_PET_BATTLE_FINAL_ROUND | 0x25e0 | 9696 | — | General | variable | — |
| SMSG_SCENE_OBJECT_PET_BATTLE_FINISHED | 0x25e1 | 9697 | — | General | fixed, 0 bytes | — |
| — | 0x25e2 | 9698 | — | General | ignored by the client | — |
| SMSG_BATTLE_PET_UPDATES | 0x25e3 | 9699 | — | General | variable | — |
| SMSG_BATTLE_PET_TRAP_LEVEL | 0x25e4 | 9700 | — | General | fixed struct | — |
| SMSG_PET_BATTLE_SLOT_UPDATES | 0x25e5 | 9701 | — | General | variable | — |
| SMSG_BATTLE_PET_JOURNAL_LOCK_ACQUIRED | 0x25e6 | 9702 | — | General | fixed struct | matches |
| SMSG_BATTLE_PET_JOURNAL_LOCK_DENIED | 0x25e7 | 9703 | — | General | fixed struct | — |
| SMSG_BATTLE_PET_JOURNAL | 0x25e8 | 9704 | — | General | variable | matches |
| SMSG_BATTLE_PET_DELETED | 0x25e9 | 9705 | — | General | fixed, 0 bytes | — |
| SMSG_BATTLE_PET_REVOKED | 0x25ea | 9706 | — | General | fixed, 0 bytes | — |
| SMSG_BATTLE_PET_RESTORED | 0x25eb | 9707 | — | General | fixed, 0 bytes | — |
| SMSG_BATTLE_PETS_HEALED | 0x25ec | 9708 | — | General | fixed struct | — |
| SMSG_BATTLE_PET_LICENSE_CHANGED | 0x25ed | 9709 | — | General | fixed struct | — |
| SMSG_PARTY_UPDATE | 0x25ee | 9710 | — | General | variable | matches |
| SMSG_READY_CHECK_STARTED | 0x25ef | 9711 | — | General | fixed, 9 bytes | matches |
| SMSG_READY_CHECK_RESPONSE | 0x25f0 | 9712 | — | General | fixed, 1 bytes | matches |
| SMSG_READY_CHECK_COMPLETED | 0x25f1 | 9713 | — | General | fixed, 1 bytes | matches |
| SMSG_PET_BATTLE_REQUEST_FAILED | 0x25f2 | 9714 | — | General | ignored by the client | — |
| SMSG_PET_BATTLE_PVP_CHALLENGE | 0x25f3 | 9715 | — | General | ignored by the client | — |
| SMSG_PET_BATTLE_FINALIZE_LOCATION | 0x25f4 | 9716 | — | General | ignored by the client | — |
| SMSG_PET_BATTLE_INITIAL_UPDATE | 0x25f5 | 9717 | — | General | ignored by the client | — |
| SMSG_PET_BATTLE_FIRST_ROUND | 0x25f6 | 9718 | — | General | ignored by the client | — |
| SMSG_PET_BATTLE_ROUND_RESULT | 0x25f7 | 9719 | — | General | ignored by the client | — |
| SMSG_PET_BATTLE_REPLACEMENTS_MADE | 0x25f8 | 9720 | — | General | ignored by the client | — |
| SMSG_PET_BATTLE_FINAL_ROUND | 0x25f9 | 9721 | — | General | ignored by the client | — |
| SMSG_PET_BATTLE_FINISHED | 0x25fa | 9722 | — | General | ignored by the client | — |
| SMSG_PET_BATTLE_CHAT_RESTRICTED | 0x25fb | 9723 | — | General | ignored by the client | — |
| SMSG_PET_BATTLE_MAX_GAME_LENGTH_WARNING | 0x25fc | 9724 | — | General | ignored by the client | — |
| SMSG_START_ELAPSED_TIMER | 0x25fd | 9725 | — | General | fixed, 12 bytes | — |
| SMSG_STOP_ELAPSED_TIMER | 0x25fe | 9726 | — | General | fixed, 5 bytes | — |
| SMSG_START_ELAPSED_TIMERS | 0x25ff | 9727 | — | General | variable | — |
| — | 0x2600 | 9728 | — | General | ignored by the client | — |
| — | 0x2601 | 9729 | — | General | ignored by the client | — |
| — | 0x2602 | 9730 | — | General | ignored by the client | — |
| — | 0x2603 | 9731 | — | General | ignored by the client | — |
| — | 0x2604 | 9732 | — | General | ignored by the client | — |
| — | 0x2605 | 9733 | — | General | ignored by the client | — |
| — | 0x2606 | 9734 | — | General | ignored by the client | — |
| — | 0x2607 | 9735 | — | General | ignored by the client | — |
| — | 0x2608 | 9736 | — | General | ignored by the client | — |
| — | 0x2609 | 9737 | — | General | ignored by the client | — |
| — | 0x260a | 9738 | — | General | ignored by the client | — |
| SMSG_RESPEC_WIPE_CONFIRM | 0x260b | 9739 | — | General | fixed, 5 bytes | matches |
| — | 0x260c | 9740 | — | General | ignored by the client | — |
| SMSG_LOOT_RESPONSE | 0x260d | 9741 | 352 (0x160) | General | variable | matches |
| SMSG_LOOT_REMOVED | 0x260e | 9742 | 354 (0x162) | General | fixed, 1 bytes | matches |
| — | 0x260f | 9743 | — | General | ignored by the client | — |
| SMSG_COIN_REMOVED | 0x2610 | 9744 | — | General | fixed, 0 bytes | matches |
| SMSG_AE_LOOT_TARGETS | 0x2611 | 9745 | — | General | fixed struct | — |
| SMSG_AE_LOOT_TARGET_ACK | 0x2612 | 9746 | — | General | fixed struct | — |
| SMSG_LOOT_RELEASE_ALL | 0x2613 | 9747 | — | General | fixed struct | — |
| SMSG_LOOT_RELEASE | 0x2614 | 9748 | 353 (0x161) | General | fixed, 0 bytes | matches |
| SMSG_LOOT_MONEY_NOTIFY | 0x2615 | 9749 | 355 (0x163) | General | fixed, 17 bytes | matches |
| SMSG_LOOT_START_ROLL | 0x2616 | 9750 | 673 (0x2a1) | General | variable | matches |
| SMSG_LOOT_ROLL | 0x2617 | 9751 | 674 (0x2a2) | General | variable | matches |
| SMSG_LOOT_MASTER_LIST | 0x2618 | 9752 | 676 (0x2a4) | General | variable | matches |
| SMSG_LOOT_ROLLS_COMPLETE | 0x2619 | 9753 | — | General | fixed, 1 bytes | matches |
| SMSG_LOOT_ALL_PASSED | 0x261a | 9754 | 670 (0x29e) | General | variable | matches |
| SMSG_LOOT_ROLL_WON | 0x261b | 9755 | 671 (0x29f) | General | variable | differs |
| SMSG_ITEM_PUSH_RESULT | 0x261c | 9756 | 358 (0x166) | General | variable | matches |
| SMSG_DISPLAY_TOAST | 0x261d | 9757 | — | General | variable | differs |
| SMSG_SET_PET_SPECIALIZATION | 0x261e | 9758 | — | General | fixed struct | — |
| SMSG_BLACK_MARKET_OPEN_RESULT | 0x261f | 9759 | — | General | fixed, 1 bytes | — |
| SMSG_BLACK_MARKET_REQUEST_ITEMS_RESULT | 0x2620 | 9760 | — | General | variable | — |
| SMSG_BLACK_MARKET_BID_ON_ITEM_RESULT | 0x2621 | 9761 | — | General | variable | — |
| SMSG_BLACK_MARKET_OUTBID | 0x2622 | 9762 | — | General | variable | — |
| SMSG_BLACK_MARKET_WON | 0x2623 | 9763 | — | General | variable | — |
| SMSG_SCENARIO_STATE | 0x2624 | 9764 | — | General | variable | — |
| SMSG_SCENARIO_PROGRESS_UPDATE | 0x2625 | 9765 | — | General | variable | — |
| SMSG_GROUP_NEW_LEADER | 0x2626 | 9766 | 121 (0x79) | General | variable | matches |
| SMSG_SEND_RAID_TARGET_UPDATE_ALL | 0x2627 | 9767 | — | General | variable | matches |
| SMSG_SEND_RAID_TARGET_UPDATE_SINGLE | 0x2628 | 9768 | — | General | fixed, 2 bytes | matches |
| SMSG_RANDOM_ROLL | 0x2629 | 9769 | — | General | fixed, 12 bytes | matches |
| SMSG_INSPECT_RESULT | 0x262a | 9770 | 277 (0x115) | General | variable | matches |
| SMSG_ARENA_CROWD_CONTROL_SPELL_RESULT | 0x262b | 9771 | — | General | fixed, 8 bytes | — |
| SMSG_SCENARIO_POIS | 0x262c | 9772 | — | General | variable | — |
| SMSG_RAID_INSTANCE_INFO | 0x262d | 9773 | 716 (0x2cc) | General | variable | matches |
| SMSG_CONSOLE_WRITE | 0x262e | 9774 | — | General | variable | — |
| SMSG_PLAY_SCENE | 0x262f | 9775 | — | General | fixed, 33 bytes | — |
| SMSG_CANCEL_SCENE | 0x2630 | 9776 | — | General | fixed struct | — |
| SMSG_BATTLE_PET_ERROR | 0x2631 | 9777 | — | General | fixed, 5 bytes | — |
| SMSG_PET_BATTLE_QUEUE_PROPOSE_MATCH | 0x2632 | 9778 | — | General | ignored by the client | — |
| SMSG_PET_BATTLE_QUEUE_STATUS | 0x2633 | 9779 | — | General | ignored by the client | — |
| SMSG_MAIL_COMMAND_RESULT | 0x2634 | 9780 | 569 (0x239) | General | fixed struct | matches |
| SMSG_NOTIFY_RECEIVED_MAIL | 0x2635 | 9781 | 645 (0x285) | General | fixed struct | matches |
| SMSG_ADD_BATTLENET_FRIEND_RESPONSE | 0x2636 | 9782 | — | General | variable | — |
| — | 0x2637 | 9783 | — | General | ignored by the client | — |
| — | 0x2638 | 9784 | — | General | ignored by the client | — |
| — | 0x2639 | 9785 | — | General | ignored by the client | — |
| — | 0x263a | 9786 | — | General | ignored by the client | — |
| SMSG_ADDON_LIST_REQUEST | 0x263b | 9787 | — | General | fixed, 7 bytes | — |
| SMSG_ACHIEVEMENT_EARNED | 0x263c | 9788 | — | General | fixed, 17 bytes | matches |
| — | 0x263d | 9789 | — | General | ignored by the client | — |
| SMSG_BONUS_ROLL_EMPTY | 0x263e | 9790 | — | General | fixed struct | — |
| SMSG_UPDATE_EXPANSION_LEVEL | 0x263f | 9791 | — | General | variable | — |
| SMSG_CONTROL_UPDATE | 0x2640 | 9792 | 345 (0x159) | General | fixed, 1 bytes | matches |
| SMSG_ARENA_PREP_OPPONENT_SPECIALIZATIONS | 0x2641 | 9793 | — | General | variable | — |
| SMSG_ARENA_CLEAR_OPPONENTS | 0x2642 | 9794 | — | General | ignored by the client | — |
| — | 0x2643 | 9795 | — | General | ignored by the client | — |
| SMSG_FORCE_OBJECT_RELINK | 0x2644 | 9796 | — | General | fixed, 0 bytes | — |
| SMSG_DISPLAY_PROMOTION | 0x2645 | 9797 | — | General | fixed struct | — |
| — | 0x2646 | 9798 | — | General | ignored by the client | — |
| SMSG_SERVER_FIRST_ACHIEVEMENTS | 0x2647 | 9799 | — | General | variable | — |
| SMSG_CORPSE_LOCATION | 0x2648 | 9800 | — | General | fixed, 21 bytes | matches |
| — | 0x2649 | 9801 | — | General | ignored by the client | — |
| SMSG_REFRESH_COMPONENT | 0x264a | 9802 | — | General | fixed struct | — |
| — | 0x264b | 9803 | — | General | ignored by the client | — |
| — | 0x264c | 9804 | — | General | ignored by the client | — |
| — | 0x264d | 9805 | — | General | ignored by the client | — |
| SMSG_DEBUG_MENU_MANAGER_FULL_UPDATE | 0x264e | 9806 | — | General | fixed struct | — |
| — | 0x264f | 9807 | — | General | ignored by the client | — |
| — | 0x2650 | 9808 | — | General | ignored by the client | — |
| — | 0x2651 | 9809 | — | General | ignored by the client | — |
| — | 0x2652 | 9810 | — | General | ignored by the client | — |
| — | 0x2653 | 9811 | — | General | ignored by the client | — |
| — | 0x2654 | 9812 | — | General | ignored by the client | — |
| — | 0x2655 | 9813 | — | General | ignored by the client | — |
| — | 0x2656 | 9814 | — | General | ignored by the client | — |
| — | 0x2657 | 9815 | — | General | ignored by the client | — |
| — | 0x2658 | 9816 | — | General | ignored by the client | — |
| — | 0x2659 | 9817 | — | General | ignored by the client | — |
| — | 0x265a | 9818 | — | General | ignored by the client | — |
| — | 0x265b | 9819 | — | General | ignored by the client | — |
| — | 0x265c | 9820 | — | General | ignored by the client | — |
| — | 0x265d | 9821 | — | General | ignored by the client | — |
| — | 0x265e | 9822 | — | General | ignored by the client | — |
| — | 0x265f | 9823 | — | General | ignored by the client | — |
| — | 0x2660 | 9824 | — | General | ignored by the client | — |
| — | 0x2661 | 9825 | — | General | ignored by the client | — |
| — | 0x2662 | 9826 | — | General | ignored by the client | — |
| — | 0x2663 | 9827 | — | General | ignored by the client | — |
| — | 0x2664 | 9828 | — | General | ignored by the client | — |
| — | 0x2665 | 9829 | — | General | ignored by the client | — |
| — | 0x2666 | 9830 | — | General | ignored by the client | — |
| — | 0x2667 | 9831 | — | General | ignored by the client | — |
| — | 0x2668 | 9832 | — | General | ignored by the client | — |
| SMSG_LOSS_OF_CONTROL_AURA_UPDATE | 0x2669 | 9833 | — | General | variable | — |
| SMSG_ADD_LOSS_OF_CONTROL | 0x266a | 9834 | — | General | fixed, 18 bytes | — |
| — | 0x266b | 9835 | — | General | ignored by the client | — |
| — | 0x266c | 9836 | — | General | ignored by the client | — |
| — | 0x266d | 9837 | — | General | ignored by the client | — |
| SMSG_PET_BATTLE_DEBUG_QUEUE_DUMP_RESPONSE | 0x266e | 9838 | — | General | ignored by the client | — |
| — | 0x266f | 9839 | — | General | ignored by the client | — |
| SMSG_SET_TIME_ZONE_INFORMATION | 0x2670 | 9840 | — | General | variable | matches |
| SMSG_BATTLE_PET_CAGE_DATE_ERROR | 0x2671 | 9841 | — | General | fixed struct | — |
| — | 0x2672 | 9842 | — | General | ignored by the client | — |
| SMSG_TEXT_EMOTE | 0x2673 | 9843 | 261 (0x105) | General | fixed, 8 bytes | matches |
| SMSG_PET_GOD_MODE | 0x2674 | 9844 | — | General | fixed, 1 bytes | — |
| SMSG_TAXI_NODE_STATUS | 0x2675 | 9845 | 427 (0x1ab) | General | fixed, 1 bytes | matches |
| SMSG_ACTIVATE_TAXI_REPLY | 0x2676 | 9846 | 430 (0x1ae) | General | fixed, 1 bytes | matches |
| SMSG_NEW_TAXI_PATH | 0x2677 | 9847 | 431 (0x1af) | General | fixed struct | matches |
| SMSG_SHOW_BANK | 0x2678 | 9848 | 440 (0x1b8) | General | fixed, 0 bytes | matches |
| — | 0x2679 | 9849 | — | General | ignored by the client | — |
| SMSG_GAME_SPEED_SET | 0x267a | 9850 | 71 (0x47) | General | fixed struct | — |
| SMSG_SERVER_TIME | 0x267b | 9851 | — | General | fixed struct | — |
| SMSG_LOGOUT_RESPONSE | 0x267c | 9852 | 76 (0x4c) | General | fixed, 5 bytes | matches |
| SMSG_LOGOUT_COMPLETE | 0x267d | 9853 | 77 (0x4d) | General | fixed struct | matches |
| SMSG_LOGOUT_CANCEL_ACK | 0x267e | 9854 | 79 (0x4f) | General | fixed struct | matches |
| SMSG_INSTANCE_RESET | 0x267f | 9855 | 798 (0x31e) | General | fixed struct | matches |
| SMSG_INSTANCE_RESET_FAILED | 0x2680 | 9856 | 799 (0x31f) | General | fixed, 5 bytes | matches |
| SMSG_UPDATE_LAST_INSTANCE | 0x2681 | 9857 | 800 (0x320) | General | fixed struct | matches |
| SMSG_KICK_REASON | 0x2682 | 9858 | — | General | fixed struct | — |
| — | 0x2683 | 9859 | — | General | ignored by the client | — |
| SMSG_CALENDAR_SEND_CALENDAR | 0x2684 | 9860 | — | General | variable | — |
| SMSG_CALENDAR_SEND_EVENT | 0x2685 | 9861 | — | General | variable | — |
| SMSG_CALENDAR_COMMUNITY_INVITE | 0x2686 | 9862 | — | General | variable | — |
| SMSG_CALENDAR_INVITE_ADDED | 0x2687 | 9863 | — | General | fixed, 24 bytes | — |
| SMSG_CALENDAR_INVITE_REMOVED | 0x2688 | 9864 | — | General | fixed, 13 bytes | — |
| SMSG_CALENDAR_INVITE_STATUS | 0x2689 | 9865 | — | General | fixed, 22 bytes | — |
| SMSG_CALENDAR_MODERATOR_STATUS | 0x268a | 9866 | — | General | fixed, 10 bytes | — |
| SMSG_CALENDAR_INVITE_ALERT | 0x268b | 9867 | — | General | variable | — |
| SMSG_CALENDAR_INVITE_STATUS_ALERT | 0x268c | 9868 | — | General | fixed struct | — |
| SMSG_CALENDAR_INVITE_REMOVED_ALERT | 0x268d | 9869 | — | General | fixed struct | — |
| SMSG_CALENDAR_EVENT_REMOVED_ALERT | 0x268e | 9870 | — | General | fixed, 13 bytes | — |
| SMSG_CALENDAR_EVENT_UPDATED_ALERT | 0x268f | 9871 | — | General | variable | — |
| SMSG_CALENDAR_INVITE_NOTES | 0x2690 | 9872 | — | General | variable | — |
| SMSG_CALENDAR_INVITE_NOTES_ALERT | 0x2691 | 9873 | — | General | variable | — |
| SMSG_CALENDAR_RAID_LOCKOUT_ADDED | 0x2692 | 9874 | — | General | fixed struct | — |
| SMSG_CALENDAR_RAID_LOCKOUT_REMOVED | 0x2693 | 9875 | — | General | fixed struct | — |
| SMSG_CALENDAR_RAID_LOCKOUT_UPDATED | 0x2694 | 9876 | — | General | fixed struct | — |
| SMSG_CALENDAR_SEND_NUM_PENDING | 0x2695 | 9877 | — | General | fixed struct | — |
| SMSG_CALENDAR_CLEAR_PENDING_ACTION | 0x2696 | 9878 | — | General | fixed struct | — |
| SMSG_CALENDAR_COMMAND_RESULT | 0x2697 | 9879 | — | General | variable | — |
| SMSG_SPECIAL_MOUNT_ANIM | 0x2698 | 9880 | 370 (0x172) | General | variable | matches |
| SMSG_PET_ACTION_SOUND | 0x2699 | 9881 | 804 (0x324) | General | fixed, 4 bytes | matches |
| SMSG_PET_DISMISS_SOUND | 0x269a | 9882 | 805 (0x325) | General | fixed struct | — |
| SMSG_GM_TICKET_SYSTEM_STATUS | 0x269b | 9883 | 539 (0x21b) | General | fixed struct | matches |
| SMSG_GM_TICKET_CASE_STATUS | 0x269c | 9884 | — | General | variable | matches |
| SMSG_SET_DUNGEON_DIFFICULTY | 0x269d | 9885 | — | General | fixed struct | matches |
| SMSG_WHO_IS | 0x269e | 9886 | 101 (0x65) | General | variable | — |
| SMSG_WEATHER | 0x269f | 9887 | 756 (0x2f4) | General | fixed, 9 bytes | matches |
| SMSG_START_LIGHTNING_STORM | 0x26a0 | 9888 | — | General | fixed struct | matches |
| SMSG_END_LIGHTNING_STORM | 0x26a1 | 9889 | — | General | fixed struct | — |
| SMSG_UPDATE_INSTANCE_OWNERSHIP | 0x26a2 | 9890 | 811 (0x32b) | General | fixed struct | matches |
| SMSG_NOTIFY_MISSILE_TRAJECTORY_COLLISION | 0x26a3 | 9891 | — | General | fixed, 12 bytes | — |
| SMSG_COMPLAINT_RESULT | 0x26a4 | 9892 | — | General | fixed struct | — |
| — | 0x26a5 | 9893 | — | General | ignored by the client | — |
| — | 0x26a6 | 9894 | — | General | ignored by the client | — |
| — | 0x26a7 | 9895 | — | General | ignored by the client | — |
| — | 0x26a8 | 9896 | — | General | ignored by the client | — |
| SMSG_SUMMON_CANCEL | 0x26a9 | 9897 | — | General | fixed struct | — |
| SMSG_DISMOUNT | 0x26aa | 9898 | — | General | fixed, 0 bytes | matches |
| SMSG_EQUIPMENT_SET_ID | 0x26ab | 9899 | — | General | fixed struct | matches |
| SMSG_PET_TAME_FAILURE | 0x26ac | 9900 | 371 (0x173) | General | fixed struct | matches |
| — | 0x26ad | 9901 | — | General | ignored by the client | — |
| SMSG_AI_REACTION | 0x26ae | 9902 | 316 (0x13c) | General | fixed, 4 bytes | matches |
| SMSG_OFFER_PETITION_ERROR | 0x26af | 9903 | — | General | fixed, 0 bytes | — |
| SMSG_RESET_FAILED_NOTIFY | 0x26b0 | 9904 | — | General | fixed struct | matches |
| SMSG_ADD_RUNE_POWER | 0x26b1 | 9905 | — | General | fixed struct | — |
| SMSG_COOLDOWN_EVENT | 0x26b2 | 9906 | 309 (0x135) | General | fixed, 5 bytes | matches |
| SMSG_CLEAR_COOLDOWN | 0x26b3 | 9907 | 478 (0x1de) | General | fixed, 5 bytes | matches |
| SMSG_OVERRIDE_LIGHT | 0x26b4 | 9908 | — | General | fixed struct | — |
| SMSG_ENABLE_BARBER_SHOP | 0x26b5 | 9909 | — | General | ignored by the client | client takes a plain struct |
| SMSG_BARBER_SHOP_RESULT | 0x26b6 | 9910 | — | General | ignored by the client | client takes a plain struct |
| SMSG_PETITION_SHOW_LIST | 0x26b7 | 9911 | 444 (0x1bc) | General | variable | matches |
| SMSG_PETITION_SHOW_SIGNATURES | 0x26b8 | 9912 | 447 (0x1bf) | General | variable | matches |
| SMSG_RECRUIT_A_FRIEND_FAILURE | 0x26b9 | 9913 | — | General | variable | — |
| SMSG_CROSSED_INEBRIATION_THRESHOLD | 0x26ba | 9914 | — | General | fixed, 8 bytes | matches |
| — | 0x26bb | 9915 | — | General | ignored by the client | — |
| SMSG_PET_NAME_INVALID | 0x26bc | 9916 | 376 (0x178) | General | variable | — |
| SMSG_SELL_RESPONSE | 0x26bd | 9917 | 417 (0x1a1) | General | fixed, 1 bytes | matches |
| SMSG_BUY_SUCCEEDED | 0x26be | 9918 | 420 (0x1a4) | General | fixed, 12 bytes | matches |
| SMSG_BUY_FAILED | 0x26bf | 9919 | 421 (0x1a5) | General | fixed, 5 bytes | matches |
| SMSG_TOTEM_CREATED | 0x26c0 | 9920 | — | General | fixed, 14 bytes | matches |
| — | 0x26c1 | 9921 | — | General | ignored by the client | — |
| SMSG_TOTEM_MOVED | 0x26c2 | 9922 | — | General | fixed, 2 bytes | — |
| SMSG_TRIGGER_MOVIE | 0x26c3 | 9923 | — | General | fixed struct | — |
| — | 0x26c4 | 9924 | — | General | ignored by the client | — |
| SMSG_SHOW_TAXI_NODES | 0x26c5 | 9925 | 425 (0x1a9) | General | variable | matches |
| SMSG_MINIMAP_PING | 0x26c6 | 9926 | — | General | fixed, 8 bytes | matches |
| SMSG_FISH_NOT_HOOKED | 0x26c7 | 9927 | 456 (0x1c8) | General | fixed struct | matches |
| SMSG_FISH_ESCAPED | 0x26c8 | 9928 | 457 (0x1c9) | General | fixed struct | matches |
| SMSG_HEALTH_UPDATE | 0x26c9 | 9929 | — | General | fixed, 8 bytes | matches |
| SMSG_POWER_UPDATE | 0x26ca | 9930 | — | General | variable | matches |
| SMSG_DEATH_RELEASE_LOC | 0x26cb | 9931 | — | General | fixed struct | matches |
| SMSG_FORCED_DEATH_UPDATE | 0x26cc | 9932 | — | General | fixed struct | — |
| SMSG_PLAYED_TIME | 0x26cd | 9933 | 461 (0x1cd) | General | fixed, 9 bytes | matches |
| — | 0x26ce | 9934 | — | General | ignored by the client | — |
| SMSG_TITLE_EARNED | 0x26cf | 9935 | — | General | fixed struct | — |
| SMSG_TITLE_LOST | 0x26d0 | 9936 | — | General | fixed struct | — |
| SMSG_HIGHEST_THREAT_UPDATE | 0x26d1 | 9937 | — | General | variable | matches |
| SMSG_THREAT_UPDATE | 0x26d2 | 9938 | — | General | variable | matches |
| SMSG_THREAT_REMOVE | 0x26d3 | 9939 | — | General | fixed, 0 bytes | matches |
| SMSG_THREAT_CLEAR | 0x26d4 | 9940 | — | General | fixed, 0 bytes | matches |
| SMSG_TOTEM_DURATION_CHANGED | 0x26d5 | 9941 | — | General | fixed, 0 bytes | — |
| SMSG_CANCEL_AUTO_REPEAT | 0x26d6 | 9942 | 668 (0x29c) | General | fixed, 0 bytes | matches |
| SMSG_TRAINER_LIST | 0x26d7 | 9943 | 433 (0x1b1) | General | variable | matches |
| SMSG_TRAINER_BUY_FAILED | 0x26d8 | 9944 | 436 (0x1b4) | General | fixed, 8 bytes | matches |
| SMSG_CRITERIA_UPDATE | 0x26d9 | 9945 | — | General | variable | differs |
| SMSG_CHAR_CUSTOMIZE_FAILURE | 0x26da | 9946 | — | General | fixed, 1 bytes | — |
| SMSG_CHAR_CUSTOMIZE_SUCCESS | 0x26db | 9947 | — | General | variable | — |
| SMSG_QUERY_TIME_RESPONSE | 0x26dc | 9948 | 463 (0x1cf) | General | fixed struct | matches |
| SMSG_LOG_XP_GAIN | 0x26dd | 9949 | 464 (0x1d0) | General | fixed, 14 bytes | matches |
| SMSG_ON_CANCEL_EXPECTED_RIDE_VEHICLE_AURA | 0x26de | 9950 | — | General | fixed struct | matches |
| SMSG_CRITERIA_DELETED | 0x26df | 9951 | — | General | fixed struct | matches |
| SMSG_ACHIEVEMENT_DELETED | 0x26e0 | 9952 | — | General | fixed struct | — |
| SMSG_LEVEL_UP_INFO | 0x26e1 | 9953 | 468 (0x1d4) | General | fixed struct | differs |
| SMSG_ITEM_CHANGED | 0x26e2 | 9954 | — | General | variable | — |
| — | 0x26e3 | 9955 | — | General | ignored by the client | — |
| — | 0x26e4 | 9956 | — | General | ignored by the client | — |
| SMSG_AUCTION_HELLO_RESPONSE | 0x26e5 | 9957 | — | General | fixed, 5 bytes | matches |
| SMSG_AUCTION_REPLICATE_RESPONSE | 0x26e6 | 9958 | — | General | variable | — |
| SMSG_AUCTION_COMMAND_RESULT | 0x26e7 | 9959 | 603 (0x25b) | General | fixed, 36 bytes | matches |
| SMSG_AUCTION_WON_NOTIFICATION | 0x26e8 | 9960 | — | General | variable | matches |
| SMSG_AUCTION_OUTBID_NOTIFICATION | 0x26e9 | 9961 | — | General | variable | matches |
| SMSG_AUCTION_CLOSED_NOTIFICATION | 0x26ea | 9962 | — | General | variable | matches |
| SMSG_AUCTION_OWNER_BID_NOTIFICATION | 0x26eb | 9963 | — | General | variable | matches |
| SMSG_AUCTION_LIST_PENDING_SALES_RESULT | 0x26ec | 9964 | — | General | variable | — |
| — | 0x26ed | 9965 | — | General | ignored by the client | — |
| SMSG_SET_VEHICLE_REC_ID | 0x26ee | 9966 | — | General | fixed, 4 bytes | matches |
| SMSG_PENDING_RAID_LOCK | 0x26ef | 9967 | — | General | fixed, 9 bytes | matches |
| SMSG_DESTRUCTIBLE_BUILDING_DAMAGE | 0x26f0 | 9968 | — | General | ignored by the client | matches |
| SMSG_INSTANCE_GROUP_SIZE_CHANGED | 0x26f1 | 9969 | — | General | fixed struct | — |
| — | 0x26f2 | 9970 | — | General | ignored by the client | — |
| SMSG_GOD_MODE | 0x26f3 | 9971 | 35 (0x23) | General | fixed, 1 bytes | — |
| SMSG_BINDER_CONFIRM | 0x26f4 | 9972 | 747 (0x2eb) | General | fixed, 0 bytes | matches |
| SMSG_PLAY_TIME_WARNING | 0x26f5 | 9973 | 757 (0x2f5) | General | ignored by the client | — |
| — | 0x26f6 | 9974 | — | General | ignored by the client | — |
| SMSG_SET_FACTION_AT_WAR | 0x26f7 | 9975 | 787 (0x313) | General | fixed struct | — |
| SMSG_CREATE_CHAR | 0x26f8 | 9976 | 58 (0x3a) | General | fixed, 1 bytes | matches |
| SMSG_DELETE_CHAR | 0x26f9 | 9977 | 60 (0x3c) | General | fixed struct | matches |
| SMSG_TRANSFER_ABORTED | 0x26fa | 9978 | 64 (0x40) | General | fixed, 10 bytes | matches |
| SMSG_PET_GUIDS | 0x26fb | 9979 | — | General | variable | matches |
| SMSG_CHARACTER_LOGIN_FAILED | 0x26fc | 9980 | 65 (0x41) | General | fixed struct | matches |
| SMSG_COMMENTATOR_STATE_CHANGED | 0x26fd | 9981 | — | General | fixed, 1 bytes | — |
| SMSG_COMMENTATOR_MAP_INFO | 0x26fe | 9982 | — | General | variable | — |
| SMSG_COMMENTATOR_PLAYER_INFO | 0x26ff | 9983 | — | General | variable | — |
| SMSG_UPDATE_ACCOUNT_DATA | 0x2700 | 9984 | 524 (0x20c) | General | variable | matches |
| SMSG_ACCOUNT_DATA_TIMES | 0x2701 | 9985 | 521 (0x209) | General | variable | matches |
| SMSG_GAME_TIME_UPDATE | 0x2702 | 9986 | 67 (0x43) | General | fixed struct | — |
| SMSG_GAME_TIME_SET | 0x2703 | 9987 | 69 (0x45) | General | fixed struct | — |
| SMSG_LOGIN_SET_TIME_SPEED | 0x2704 | 9988 | 66 (0x42) | General | fixed struct | matches |
| SMSG_LOAD_EQUIPMENT_SET | 0x2705 | 9989 | — | General | variable | matches |
| SMSG_START_MIRROR_TIMER | 0x2706 | 9990 | 473 (0x1d9) | General | fixed, 21 bytes | matches |
| SMSG_PAUSE_MIRROR_TIMER | 0x2707 | 9991 | 474 (0x1da) | General | fixed, 5 bytes | matches |
| SMSG_STOP_MIRROR_TIMER | 0x2708 | 9992 | 475 (0x1db) | General | fixed struct | matches |
| SMSG_CORPSE_TRANSPORT_QUERY | 0x2709 | 9993 | — | General | fixed, 16 bytes | — |
| SMSG_ENCHANTMENT_LOG | 0x270a | 9994 | 471 (0x1d7) | General | fixed, 12 bytes | matches |
| SMSG_SERVER_TIME_OFFSET | 0x270b | 9995 | — | General | fixed struct | matches |
| SMSG_SPIRIT_HEALER_CONFIRM | 0x270c | 9996 | 546 (0x222) | General | fixed, 0 bytes | matches |
| SMSG_AREA_TRIGGER_NO_CORPSE | 0x270d | 9997 | — | General | fixed struct | — |
| SMSG_TALENTS_INVOLUNTARILY_RESET | 0x270e | 9998 | — | General | fixed, 1 bytes | — |
| SMSG_SPEC_INVOLUNTARILY_CHANGED | 0x270f | 9999 | — | General | fixed, 1 bytes | — |
| SMSG_PAGE_TEXT | 0x2710 | 10000 | 479 (0x1df) | General | fixed, 0 bytes | — |
| SMSG_GAME_OBJECT_UI_LINK | 0x2711 | 10001 | — | General | fixed, 4 bytes | — |
| SMSG_OPEN_ANIMA_DIVERSION_UI | 0x2712 | 10002 | — | General | ignored by the client | — |
| SMSG_STAND_STATE_UPDATE | 0x2713 | 10003 | 669 (0x29d) | General | fixed struct | matches |
| SMSG_SET_FORCED_REACTIONS | 0x2714 | 10004 | 677 (0x2a5) | General | variable | matches |
| SMSG_GAME_OBJECT_RESET_STATE | 0x2715 | 10005 | 679 (0x2a7) | General | fixed, 0 bytes | matches |
| — | 0x2716 | 10006 | — | General | ignored by the client | — |
| — | 0x2717 | 10007 | — | General | ignored by the client | — |
| SMSG_SUMMON_REQUEST | 0x2718 | 10008 | 683 (0x2ab) | General | fixed, 10 bytes | matches |
| SMSG_INSPECT_PVP | 0x2719 | 10009 | — | General | variable | differs |
| — | 0x271a | 10010 | — | General | fixed, 0 bytes | — |
| SMSG_INITIALIZE_FACTIONS | 0x271b | 10011 | 290 (0x122) | General | variable | matches |
| SMSG_FACTION_BONUS_INFO | 0x271c | 10012 | — | General | variable | — |
| SMSG_CAMERA_EFFECT | 0x271d | 10013 | — | General | fixed, 8 bytes | — |
| SMSG_SOCKET_GEMS_SUCCESS | 0x271e | 10014 | — | General | fixed, 0 bytes | matches |
| SMSG_SOCKET_GEMS_FAILURE | 0x271f | 10015 | — | General | fixed, 0 bytes | — |
| — | 0x2720 | 10016 | — | General | ignored by the client | — |
| SMSG_SET_FACTION_VISIBLE | 0x2721 | 10017 | 291 (0x123) | General | fixed struct | — |
| SMSG_SET_FACTION_NOT_VISIBLE | 0x2722 | 10018 | — | General | fixed struct | — |
| SMSG_SET_FACTION_STANDING | 0x2723 | 10019 | 292 (0x124) | General | variable | matches |
| — | 0x2724 | 10020 | — | General | ignored by the client | — |
| — | 0x2725 | 10021 | — | General | ignored by the client | — |
| — | 0x2726 | 10022 | — | General | ignored by the client | — |
| SMSG_SET_AI_ANIM_KIT | 0x2727 | 10023 | — | General | fixed, 2 bytes | — |
| SMSG_PLAY_ONE_SHOT_ANIM_KIT | 0x2728 | 10024 | — | General | fixed, 2 bytes | — |
| SMSG_SET_MOVEMENT_ANIM_KIT | 0x2729 | 10025 | — | General | fixed, 2 bytes | — |
| SMSG_SET_MELEE_ANIM_KIT | 0x272a | 10026 | — | General | fixed, 2 bytes | — |
| SMSG_SET_ANIM_TIER | 0x272b | 10027 | — | General | fixed, 1 bytes | — |
| SMSG_SET_PROFICIENCY | 0x272c | 10028 | 295 (0x127) | General | fixed struct | matches |
| — | 0x272d | 10029 | — | General | ignored by the client | — |
| — | 0x272e | 10030 | — | General | ignored by the client | — |
| — | 0x272f | 10031 | — | General | ignored by the client | — |
| SMSG_COOLDOWN_CHEAT | 0x2730 | 10032 | 481 (0x1e1) | General | fixed, 1 bytes | differs |
| — | 0x2731 | 10033 | — | General | ignored by the client | — |
| — | 0x2732 | 10034 | — | General | ignored by the client | — |
| — | 0x2733 | 10035 | — | General | ignored by the client | — |
| — | 0x2734 | 10036 | — | General | ignored by the client | — |
| — | 0x2735 | 10037 | — | General | ignored by the client | — |
| — | 0x2736 | 10038 | — | General | ignored by the client | — |
| SMSG_AREA_SPIRIT_HEALER_TIME | 0x2737 | 10039 | 740 (0x2e4) | General | fixed, 4 bytes | matches |
| SMSG_LOOT_LIST | 0x2738 | 10040 | — | General | variable | matches |
| SMSG_DESTROY_ARENA_UNIT | 0x2739 | 10041 | — | General | fixed, 0 bytes | — |
| — | 0x273a | 10042 | — | General | ignored by the client | — |
| SMSG_UI_HEALING_RANGE_MODIFIED | 0x273b | 10043 | — | General | fixed struct | — |
| SMSG_FEIGN_DEATH_RESISTED | 0x273c | 10044 | 692 (0x2b4) | General | fixed struct | — |
| SMSG_DURABILITY_DAMAGE_DEATH | 0x273d | 10045 | 701 (0x2bd) | General | fixed struct | matches |
| SMSG_INIT_WORLD_STATES | 0x273e | 10046 | 706 (0x2c2) | General | variable | matches |
| SMSG_UPDATE_WORLD_STATE | 0x273f | 10047 | 707 (0x2c3) | General | fixed, 9 bytes | matches |
| SMSG_PET_ACTION_FEEDBACK | 0x2740 | 10048 | 710 (0x2c6) | General | fixed struct | — |
| SMSG_CORPSE_RECLAIM_DELAY | 0x2741 | 10049 | 617 (0x269) | General | fixed struct | matches |
| SMSG_REATTACH_RESURRECT | 0x2742 | 10050 | — | General | fixed, 0 bytes | — |
| SMSG_PETITION_SIGN_RESULTS | 0x2743 | 10051 | 449 (0x1c1) | General | fixed, 1 bytes | matches |
| — | 0x2744 | 10052 | — | General | ignored by the client | — |
| SMSG_TURN_IN_PETITION_RESULT | 0x2745 | 10053 | 453 (0x1c5) | General | fixed, 1 bytes | matches |
| SMSG_USE_EQUIPMENT_SET_RESULT | 0x2746 | 10054 | — | General | fixed struct | matches |
| — | 0x2747 | 10055 | — | General | ignored by the client | — |
| SMSG_FORCE_ANIM | 0x2748 | 10056 | — | General | variable | — |
| SMSG_FORCE_ANIMATIONS | 0x2749 | 10057 | — | General | ignored by the client | — |
| SMSG_INVALID_PROMOTION_CODE | 0x274a | 10058 | 487 (0x1e7) | General | fixed struct | — |
| SMSG_ITEM_TIME_UPDATE | 0x274b | 10059 | 490 (0x1ea) | General | fixed, 4 bytes | — |
| SMSG_ITEM_ENCHANT_TIME_UPDATE | 0x274c | 10060 | 491 (0x1eb) | General | fixed, 8 bytes | matches |
| SMSG_MAIL_LIST_RESULT | 0x274d | 10061 | 571 (0x23b) | General | variable | matches |
| SMSG_MAIL_QUERY_NEXT_TIME_RESULT | 0x274e | 10062 | — | General | variable | matches |
| SMSG_PARTY_MEMBER_PARTIAL_STATE | 0x274f | 10063 | 126 (0x7e) | General | variable | matches |
| SMSG_PARTY_MEMBER_FULL_STATE | 0x2750 | 10064 | 754 (0x2f2) | General | variable | matches |
| SMSG_PARTY_KILL_LOG | 0x2751 | 10065 | 501 (0x1f5) | General | fixed, 0 bytes | matches |
| SMSG_PROC_RESIST | 0x2752 | 10066 | 608 (0x260) | General | variable | — |
| SMSG_ISLAND_AZERITE_GAIN | 0x2753 | 10067 | — | General | fixed, 8 bytes | — |
| SMSG_ISLAND_COMPLETE | 0x2754 | 10068 | — | General | variable | — |
| SMSG_WARFRONT_COMPLETE | 0x2755 | 10069 | — | General | fixed struct | — |
| SMSG_EXPLORATION_EXPERIENCE | 0x2756 | 10070 | 504 (0x1f8) | General | fixed struct | matches |
| SMSG_ARENA_TEAM_ROSTER | 0x2757 | 10071 | — | General | variable | matches |
| SMSG_ARENA_TEAM_INVITE | 0x2758 | 10072 | — | General | variable | matches |
| SMSG_ARENA_TEAM_EVENT | 0x2759 | 10073 | — | General | variable | matches |
| SMSG_ARENA_TEAM_COMMAND_RESULT | 0x275a | 10074 | — | General | variable | matches |
| SMSG_ARENA_TEAM_STATS | 0x275b | 10075 | — | General | fixed struct | — |
| SMSG_GET_ACCOUNT_CHARACTER_LIST_RESULT | 0x275c | 10076 | — | General | variable | matches |
| SMSG_LIVE_REGION_GET_ACCOUNT_CHARACTER_LIST_RESULT | 0x275d | 10077 | — | General | variable | — |
| SMSG_CHARACTER_RENAME_RESULT | 0x275e | 10078 | 712 (0x2c8) | General | variable | matches |
| SMSG_MODIFY_COOLDOWN | 0x275f | 10079 | — | General | fixed, 9 bytes | — |
| SMSG_UPDATE_COOLDOWN | 0x2760 | 10080 | — | General | fixed struct | — |
| SMSG_UPDATE_CHARGE_CATEGORY_COOLDOWN | 0x2761 | 10081 | — | General | fixed, 13 bytes | — |
| SMSG_PRE_RESSURECT | 0x2762 | 10082 | — | General | fixed, 0 bytes | matches |
| SMSG_PLAY_SOUND | 0x2763 | 10083 | 722 (0x2d2) | General | fixed, 8 bytes | matches |
| SMSG_PLAY_MUSIC | 0x2764 | 10084 | 631 (0x277) | General | fixed struct | matches |
| SMSG_PLAY_OBJECT_SOUND | 0x2765 | 10085 | 632 (0x278) | General | fixed, 20 bytes | matches |
| SMSG_PLAY_SPEAKERBOT_SOUND | 0x2766 | 10086 | — | General | fixed, 4 bytes | — |
| SMSG_STOP_SPEAKERBOT_SOUND | 0x2767 | 10087 | — | General | fixed, 0 bytes | — |
| SMSG_LIVE_REGION_CHARACTER_COPY_RESULT | 0x2768 | 10088 | — | General | fixed, 5 bytes | — |
| SMSG_LIVE_REGION_ACCOUNT_RESTORE_RESULT | 0x2769 | 10089 | — | General | fixed, 5 bytes | — |
| SMSG_LIVE_REGION_KEY_BINDINGS_COPY_RESULT | 0x276a | 10090 | — | General | ignored by the client | — |
| SMSG_SHOW_TRADE_SKILL_RESPONSE | 0x276b | 10091 | — | General | variable | — |
| SMSG_BATTLE_PAY_GET_PRODUCT_LIST_RESPONSE | 0x276c | 10092 | — | General | variable | — |
| SMSG_BATTLE_PAY_GET_PURCHASE_LIST_RESPONSE | 0x276d | 10093 | — | General | variable | — |
| SMSG_BATTLE_PAY_GET_DISTRIBUTION_LIST_RESPONSE | 0x276e | 10094 | — | General | variable | — |
| SMSG_BATTLE_PAY_DISTRIBUTION_UNREVOKED | 0x276f | 10095 | — | General | fixed, 8 bytes | — |
| SMSG_BATTLE_PAY_DISTRIBUTION_UPDATE | 0x2770 | 10096 | — | General | variable | — |
| SMSG_BATTLE_PAY_DELIVERY_STARTED | 0x2771 | 10097 | — | General | fixed struct | — |
| SMSG_BATTLE_PAY_DELIVERY_ENDED | 0x2772 | 10098 | — | General | variable | — |
| SMSG_BATTLE_PAY_MOUNT_DELIVERED | 0x2773 | 10099 | — | General | fixed struct | — |
| SMSG_BATTLE_PAY_BATTLE_PET_DELIVERED | 0x2774 | 10100 | — | General | fixed, 4 bytes | — |
| SMSG_BATTLE_PAY_COLLECTION_ITEM_DELIVERED | 0x2775 | 10101 | — | General | fixed struct | — |
| — | 0x2776 | 10102 | — | General | ignored by the client | — |
| SMSG_INSTANCE_SAVE_CREATED | 0x2777 | 10103 | 715 (0x2cb) | General | fixed, 1 bytes | matches |
| SMSG_ENCOUNTER_START | 0x2778 | 10104 | — | General | variable | — |
| SMSG_ENCOUNTER_END | 0x2779 | 10105 | — | General | fixed, 17 bytes | — |
| SMSG_BATTLE_PAY_START_PURCHASE_RESPONSE | 0x277a | 10106 | — | General | fixed struct | — |
| SMSG_BATTLE_PAY_START_DISTRIBUTION_ASSIGN_TO_TARGET_RESPONSE | 0x277b | 10107 | — | General | fixed struct | — |
| — | 0x277c | 10108 | — | General | ignored by the client | — |
| SMSG_BATTLE_PAY_PURCHASE_UPDATE | 0x277d | 10109 | — | General | variable | — |
| SMSG_BATTLE_PAY_CONFIRM_PURCHASE | 0x277e | 10110 | — | General | fixed struct | — |
| SMSG_BATTLE_PAY_ACK_FAILED | 0x277f | 10111 | — | General | fixed struct | — |
| SMSG_CONQUEST_FORMULA_CONSTANTS | 0x2780 | 10112 | — | General | fixed struct | matches |
| — | 0x2781 | 10113 | — | General | ignored by the client | — |
| — | 0x2782 | 10114 | — | General | ignored by the client | — |
| SMSG_CONTACT_LIST | 0x2783 | 10115 | — | General | variable | matches |
| SMSG_FRIEND_STATUS | 0x2784 | 10116 | 104 (0x68) | General | variable | matches |
| SMSG_CHARACTER_OBJECT_TEST_RESPONSE | 0x2785 | 10117 | — | General | fixed struct | — |
| SMSG_BATTLENET_CHALLENGE_START | 0x2786 | 10118 | — | General | variable | — |
| SMSG_BATTLENET_CHALLENGE_ABORT | 0x2787 | 10119 | — | General | fixed, 5 bytes | — |
| SMSG_UPDATE_TASK_PROGRESS | 0x2788 | 10120 | — | General | variable | — |
| SMSG_SET_ALL_TASK_PROGRESS | 0x2789 | 10121 | — | General | variable | matches |
| SMSG_SET_TASK_COMPLETE | 0x278a | 10122 | — | General | fixed struct | — |
| SMSG_GROUP_DECLINE | 0x278b | 10123 | 116 (0x74) | General | variable | matches |
| SMSG_GROUP_UNINVITE | 0x278c | 10124 | 119 (0x77) | General | fixed struct | matches |
| SMSG_GROUP_DESTROYED | 0x278d | 10125 | 124 (0x7c) | General | fixed struct | matches |
| SMSG_GROUP_AUTO_KICK | 0x278e | 10126 | — | General | fixed struct | — |
| SMSG_PARTY_COMMAND_RESULT | 0x278f | 10127 | 127 (0x7f) | General | variable | matches |
| SMSG_GOSSIP_POI | 0x2790 | 10128 | 548 (0x224) | General | variable | matches |
| — | 0x2791 | 10129 | — | General | ignored by the client | — |
| SMSG_OPEN_SHIPMENT_NPC_FROM_GOSSIP | 0x2792 | 10130 | — | General | ignored by the client | — |
| SMSG_GET_SHIPMENT_INFO_RESPONSE | 0x2793 | 10131 | — | General | ignored by the client | — |
| SMSG_OPEN_SHIPMENT_NPC_RESULT | 0x2794 | 10132 | — | General | ignored by the client | — |
| SMSG_CREATE_SHIPMENT_RESPONSE | 0x2795 | 10133 | — | General | ignored by the client | — |
| SMSG_COMPLETE_SHIPMENT_RESPONSE | 0x2796 | 10134 | — | General | ignored by the client | — |
| SMSG_GET_SHIPMENTS_OF_TYPE_RESPONSE | 0x2797 | 10135 | — | General | ignored by the client | — |
| SMSG_GET_LANDING_PAGE_SHIPMENTS_RESPONSE | 0x2798 | 10136 | — | General | ignored by the client | — |
| SMSG_READ_ITEM_RESULT_OK | 0x2799 | 10137 | 174 (0xae) | General | fixed, 0 bytes | matches |
| — | 0x279a | 10138 | — | General | ignored by the client | — |
| — | 0x279b | 10139 | — | General | ignored by the client | — |
| — | 0x279c | 10140 | — | General | ignored by the client | — |
| — | 0x279d | 10141 | — | General | ignored by the client | — |
| — | 0x279e | 10142 | — | General | ignored by the client | — |
| — | 0x279f | 10143 | — | General | ignored by the client | — |
| — | 0x27a0 | 10144 | — | General | ignored by the client | — |
| SMSG_READ_ITEM_RESULT_FAILED | 0x27a1 | 10145 | 175 (0xaf) | General | fixed, 5 bytes | matches |
| SMSG_SCENARIO_VACATE | 0x27a2 | 10146 | — | General | fixed, 9 bytes | — |
| SMSG_SHOW_MAILBOX | 0x27a3 | 10147 | — | General | fixed, 0 bytes | — |
| SMSG_CHAR_FACTION_CHANGE_RESULT | 0x27a4 | 10148 | — | General | variable | — |
| SMSG_RAID_DIFFICULTY_SET | 0x27a5 | 10149 | — | General | fixed struct | matches |
| SMSG_XP_GAIN_ENABLED | 0x27a6 | 10150 | — | General | fixed, 1 bytes | — |
| SMSG_RAID_GROUP_ONLY | 0x27a7 | 10151 | 646 (0x286) | General | fixed struct | matches |
| SMSG_INSTANCE_ENCOUNTER_ENGAGE_UNIT | 0x27a8 | 10152 | — | General | fixed, 1 bytes | — |
| SMSG_INSTANCE_ENCOUNTER_DISENGAGE_UNIT | 0x27a9 | 10153 | — | General | fixed, 0 bytes | — |
| SMSG_INSTANCE_ENCOUNTER_CHANGE_PRIORITY | 0x27aa | 10154 | — | General | fixed, 1 bytes | — |
| SMSG_INSTANCE_ENCOUNTER_TIMER_START | 0x27ab | 10155 | — | General | fixed struct | — |
| SMSG_INSTANCE_ENCOUNTER_OBJECTIVE_START | 0x27ac | 10156 | — | General | fixed struct | — |
| SMSG_INSTANCE_ENCOUNTER_OBJECTIVE_COMPLETE | 0x27ad | 10157 | — | General | fixed struct | — |
| SMSG_INSTANCE_ENCOUNTER_START | 0x27ae | 10158 | — | General | fixed, 17 bytes | — |
| SMSG_INSTANCE_ENCOUNTER_UPDATE_SUPPRESS_RELEASE | 0x27af | 10159 | — | General | fixed, 1 bytes | — |
| SMSG_INSTANCE_ENCOUNTER_UPDATE_ALLOW_RELEASE_IN_PROGRESS | 0x27b0 | 10160 | — | General | fixed, 1 bytes | — |
| SMSG_INSTANCE_ENCOUNTER_OBJECTIVE_UPDATE | 0x27b1 | 10161 | — | General | fixed struct | — |
| SMSG_INSTANCE_ENCOUNTER_END | 0x27b2 | 10162 | — | General | fixed struct | — |
| SMSG_INSTANCE_ENCOUNTER_IN_COMBAT_RESURRECTION | 0x27b3 | 10163 | — | General | fixed struct | — |
| SMSG_INSTANCE_ENCOUNTER_GAIN_COMBAT_RESURRECTION_CHARGE | 0x27b4 | 10164 | — | General | fixed struct | — |
| SMSG_INSTANCE_ENCOUNTER_PHASE_SHIFT_CHANGED | 0x27b5 | 10165 | — | General | fixed struct | — |
| SMSG_TUTORIAL_FLAGS | 0x27b6 | 10166 | 253 (0xfd) | General | fixed struct | matches |
| SMSG_CHARACTER_UPGRADE_STARTED | 0x27b7 | 10167 | — | General | fixed, 0 bytes | — |
| SMSG_CHARACTER_UPGRADE_COMPLETE | 0x27b8 | 10168 | — | General | variable | — |
| SMSG_CHARACTER_UPGRADE_ABORTED | 0x27b9 | 10169 | — | General | fixed, 0 bytes | — |
| SMSG_CHARACTER_CHECK_UPGRADE_RESULT | 0x27ba | 10170 | — | General | fixed struct | — |
| SMSG_CHARACTER_UPGRADE_MANUAL_UNREVOKE_RESULT | 0x27bb | 10171 | — | General | fixed struct | — |
| SMSG_UPDATE_CHARACTER_FLAGS | 0x27bc | 10172 | — | General | variable | — |
| SMSG_REPLACE_TROPHY_RESPONSE | 0x27bd | 10173 | — | General | ignored by the client | — |
| SMSG_GET_TROPHY_LIST_RESPONSE | 0x27be | 10174 | — | General | ignored by the client | — |
| SMSG_GET_SELECTED_TROPHY_ID_RESPONSE | 0x27bf | 10175 | — | General | ignored by the client | — |
| SMSG_ITEM_COOLDOWN | 0x27c0 | 10176 | 176 (0xb0) | General | fixed, 8 bytes | matches |
| SMSG_DAMAGE_CALC_LOG | 0x27c1 | 10177 | 636 (0x27c) | General | variable | — |
| SMSG_EMOTE | 0x27c2 | 10178 | 259 (0x103) | General | variable | matches |
| SMSG_TRIGGER_CINEMATIC | 0x27c3 | 10179 | 250 (0xfa) | General | fixed struct | matches |
| SMSG_UPDATE_OBJECT | 0x27c4 | 10180 | 169 (0xa9) | General | variable | matches |
| SMSG_REALM_LOOKUP_INFO | 0x27c5 | 10181 | — | General | variable | — |
| SMSG_UNDELETE_CHARACTER_RESPONSE | 0x27c6 | 10182 | — | General | fixed, 8 bytes | — |
| SMSG_UNDELETE_COOLDOWN_STATUS_RESPONSE | 0x27c7 | 10183 | — | General | fixed, 9 bytes | — |
| — | 0x27c8 | 10184 | — | General | ignored by the client | — |
| — | 0x27c9 | 10185 | — | General | ignored by the client | — |
| — | 0x27ca | 10186 | — | General | ignored by the client | — |
| SMSG_SET_LOOT_METHOD_FAILED | 0x27cb | 10187 | — | General | fixed struct | — |
| SMSG_COMMERCE_TOKEN_GET_COUNT_RESPONSE | 0x27cc | 10188 | — | General | variable | — |
| SMSG_COMMERCE_TOKEN_UPDATE | 0x27cd | 10189 | — | General | variable | — |
| SMSG_COMMERCE_TOKEN_GET_MARKET_PRICE_RESPONSE | 0x27ce | 10190 | — | General | fixed struct | — |
| SMSG_AUCTIONABLE_TOKEN_SELL_CONFIRM_REQUIRED | 0x27cf | 10191 | — | General | fixed struct | — |
| SMSG_AUCTIONABLE_TOKEN_SELL_AT_MARKET_PRICE_RESPONSE | 0x27d0 | 10192 | — | General | fixed struct | — |
| SMSG_AUCTIONABLE_TOKEN_AUCTION_SOLD | 0x27d1 | 10193 | — | General | fixed struct | — |
| SMSG_CONSUMABLE_TOKEN_CAN_VETERAN_BUY_RESPONSE | 0x27d2 | 10194 | — | General | fixed struct | — |
| SMSG_CONSUMABLE_TOKEN_BUY_CHOICE_REQUIRED | 0x27d3 | 10195 | — | General | fixed struct | — |
| SMSG_CONSUMABLE_TOKEN_BUY_AT_MARKET_PRICE_RESPONSE | 0x27d4 | 10196 | — | General | fixed struct | — |
| SMSG_GET_REMAINING_GAME_TIME_RESPONSE | 0x27d5 | 10197 | — | General | fixed, 9 bytes | — |
| SMSG_CONSUMABLE_TOKEN_REDEEM_CONFIRM_REQUIRED | 0x27d6 | 10198 | — | General | fixed, 33 bytes | — |
| SMSG_CONSUMABLE_TOKEN_REDEEM_RESPONSE | 0x27d7 | 10199 | — | General | fixed struct | — |
| SMSG_COMMERCE_TOKEN_GET_LOG_RESPONSE | 0x27d8 | 10200 | — | General | variable | — |
| SMSG_MULTI_FLOOR_NEW_FLOOR | 0x27d9 | 10201 | — | General | ignored by the client | — |
| SMSG_MULTI_FLOOR_LEAVE_FLOOR | 0x27da | 10202 | — | General | ignored by the client | — |
| — | 0x27db | 10203 | — | General | ignored by the client | — |
| — | 0x27dc | 10204 | — | General | ignored by the client | — |
| SMSG_GAIN_MAW_POWER | 0x27dd | 10205 | — | General | ignored by the client | — |
| — | 0x27de | 10206 | — | General | ignored by the client | — |
| — | 0x27df | 10207 | — | General | ignored by the client | — |
| — | 0x27e0 | 10208 | — | General | ignored by the client | — |
| — | 0x27e1 | 10209 | — | General | ignored by the client | — |
| — | 0x27e2 | 10210 | — | General | ignored by the client | — |
| — | 0x27e3 | 10211 | — | General | ignored by the client | — |
| — | 0x27e4 | 10212 | — | General | ignored by the client | — |
| — | 0x27e5 | 10213 | — | General | ignored by the client | — |
| — | 0x27e6 | 10214 | — | General | ignored by the client | — |
| SMSG_SCENARIO_COMPLETED | 0x27e7 | 10215 | — | General | fixed struct | — |
| SMSG_ARTIFACT_XP_GAIN | 0x27e8 | 10216 | — | General | ignored by the client | — |
| SMSG_DISPLAY_WORLD_TEXT | 0x27e9 | 10217 | — | General | ignored by the client | — |
| SMSG_GET_VAS_ACCOUNT_CHARACTER_LIST_RESULT | 0x27ea | 10218 | — | General | variable | — |
| SMSG_GET_VAS_TRANSFER_TARGET_REALM_LIST_RESULT | 0x27eb | 10219 | — | General | variable | — |
| SMSG_VAS_PURCHASE_STATE_UPDATE | 0x27ec | 10220 | — | General | variable | — |
| SMSG_VAS_PURCHASE_COMPLETE | 0x27ed | 10221 | — | General | variable | — |
| SMSG_ENUM_VAS_PURCHASE_STATES_RESPONSE | 0x27ee | 10222 | — | General | variable | — |
| SMSG_ADVENTURE_MAP_OPEN_NPC | 0x27ef | 10223 | — | General | fixed, 4 bytes | — |
| SMSG_WORLD_MAP_OPEN_NPC | 0x27f0 | 10224 | — | General | ignored by the client | — |
| SMSG_TRANSMOGRIFY_NPC | 0x27f1 | 10225 | — | General | ignored by the client | — |
| SMSG_AZERITE_RESPEC_NPC | 0x27f2 | 10226 | — | General | ignored by the client | — |
| SMSG_UI_ITEM_INTERACTION_NPC | 0x27f3 | 10227 | — | General | ignored by the client | — |
| SMSG_ISLANDS_MISSION_NPC | 0x27f4 | 10228 | — | General | ignored by the client | — |
| SMSG_ALLIED_RACE_DETAILS | 0x27f5 | 10229 | — | General | ignored by the client | — |
| — | 0x27f6 | 10230 | — | General | ignored by the client | — |
| SMSG_CHROMIE_TIME_OPEN_NPC | 0x27f7 | 10231 | — | General | ignored by the client | — |
| SMSG_COVENANT_PREVIEW_OPEN_NPC | 0x27f8 | 10232 | — | General | ignored by the client | — |
| SMSG_RUNEFORGE_LEGENDARY_CRAFTING_OPEN_NPC | 0x27f9 | 10233 | — | General | ignored by the client | — |
| — | 0x27fa | 10234 | — | General | ignored by the client | — |
| SMSG_SCENARIO_UI_UPDATE | 0x27fb | 10235 | — | General | variable | — |
| SMSG_SCENARIO_SHOW_CRITERIA | 0x27fc | 10236 | — | General | fixed, 1 bytes | — |
| — | 0x27fd | 10237 | — | General | ignored by the client | — |
| SMSG_GAME_OBJECT_SET_STATE_LOCAL | 0x27fe | 10238 | — | General | fixed, 1 bytes | — |
| SMSG_BATTLENET_RESPONSE | 0x27ff | 10239 | — | General | variable | matches |
| SMSG_BATTLENET_NOTIFICATION | 0x2800 | 10240 | — | General | variable | matches |
| SMSG_BATTLE_NET_CONNECTION_STATUS | 0x2801 | 10241 | — | General | fixed, 1 bytes | matches |
| SMSG_CHANGE_REALM_TICKET_RESPONSE | 0x2802 | 10242 | — | General | variable | differs |
| — | 0x2803 | 10243 | — | General | ignored by the client | — |
| — | 0x2804 | 10244 | — | General | ignored by the client | — |
| — | 0x2805 | 10245 | — | General | ignored by the client | — |
| — | 0x2806 | 10246 | — | General | ignored by the client | — |
| — | 0x2807 | 10247 | — | General | ignored by the client | — |
| — | 0x2808 | 10248 | — | General | ignored by the client | — |
| — | 0x2809 | 10249 | — | General | ignored by the client | — |
| — | 0x280a | 10250 | — | General | ignored by the client | — |
| SMSG_FAILED_QUEST_TURN_IN | 0x280b | 10251 | — | General | variable | — |
| SMSG_QUEUE_SUMMARY_UPDATE | 0x280c | 10252 | — | General | variable | — |
| SMSG_INVENTORY_FIXUP_COMPLETE | 0x280d | 10253 | — | General | fixed, 1 bytes | — |
| SMSG_CONFIRM_PARTY_INVITE | 0x280e | 10254 | — | General | variable | — |
| SMSG_CAN_REDEEM_TOKEN_FOR_BALANCE_RESPONSE | 0x280f | 10255 | — | General | fixed, 25 bytes | — |
| SMSG_BATTLE_PAY_VALIDATE_PURCHASE_RESPONSE | 0x2810 | 10256 | — | General | fixed, 25 bytes | — |
| SMSG_VAS_GET_SERVICE_STATUS_RESPONSE | 0x2811 | 10257 | — | General | fixed, 1 bytes | — |
| SMSG_VAS_GET_QUEUE_MINUTES_RESPONSE | 0x2812 | 10258 | — | General | fixed struct | — |
| — | 0x2813 | 10259 | — | General | ignored by the client | — |
| SMSG_VAS_CHECK_TRANSFER_OK_RESPONSE | 0x2814 | 10260 | — | General | variable | — |
| SMSG_CONTRIBUTION_LAST_UPDATE_RESPONSE | 0x2815 | 10261 | — | General | fixed struct | — |
| SMSG_GENERATE_SSO_TOKEN_RESPONSE | 0x2816 | 10262 | — | General | variable | — |
| SMSG_VOICE_LOGIN_RESPONSE | 0x2817 | 10263 | — | General | variable | — |
| SMSG_VOICE_CHANNEL_INFO_RESPONSE | 0x2818 | 10264 | — | General | variable | — |
| SMSG_UPDATE_CELESTIAL_BODY | 0x2819 | 10265 | — | General | fixed struct | — |
| SMSG_WARDEN3_ENABLED | 0x281a | 10266 | — | General | fixed struct | — |
| SMSG_WARDEN3_DISABLED | 0x281b | 10267 | — | General | fixed struct | — |
| SMSG_BATTLE_PAY_START_CHECKOUT | 0x281c | 10268 | — | General | variable | — |
| SMSG_UPDATE_BNET_SESSION_KEY | 0x281d | 10269 | — | General | variable | — |
| SMSG_INVENTORY_FULL_OVERFLOW | 0x281e | 10270 | — | General | fixed struct | — |
| SMSG_WILL_BE_KICKED_FOR_ADDED_SUBSCRIPTION_TIME | 0x281f | 10271 | — | General | fixed struct | — |
| SMSG_UPDATE_GAME_TIME_STATE | 0x2820 | 10272 | — | General | fixed, 13 bytes | — |
| SMSG_OPEN_HEART_FORGE | 0x2821 | 10273 | — | General | ignored by the client | — |
| SMSG_CLOSE_HEART_FORGE | 0x2822 | 10274 | — | General | ignored by the client | — |
| SMSG_GAME_OBJECT_BASE | 0x2823 | 10275 | — | General | variable | — |
| SMSG_LEGACY_LOOT_RULES | 0x2824 | 10276 | — | General | fixed, 1 bytes | — |
| — | 0x2825 | 10277 | — | General | ignored by the client | — |
| — | 0x2826 | 10278 | — | General | ignored by the client | — |
| — | 0x2827 | 10279 | — | General | ignored by the client | — |
| — | 0x2828 | 10280 | — | General | ignored by the client | — |
| — | 0x2829 | 10281 | — | General | ignored by the client | — |
| — | 0x282a | 10282 | — | General | ignored by the client | — |
| — | 0x282b | 10283 | — | General | ignored by the client | — |
| — | 0x282c | 10284 | — | General | ignored by the client | — |
| — | 0x282d | 10285 | — | General | ignored by the client | — |
| — | 0x282e | 10286 | — | General | ignored by the client | — |
| — | 0x282f | 10287 | — | General | ignored by the client | — |
| — | 0x2830 | 10288 | — | General | ignored by the client | — |
| — | 0x2831 | 10289 | — | General | ignored by the client | — |
| — | 0x2832 | 10290 | — | General | ignored by the client | — |
| — | 0x2833 | 10291 | — | General | ignored by the client | — |
| — | 0x2834 | 10292 | — | General | ignored by the client | — |
| — | 0x2835 | 10293 | — | General | ignored by the client | — |
| — | 0x2836 | 10294 | — | General | ignored by the client | — |
| — | 0x2837 | 10295 | — | General | ignored by the client | — |
| — | 0x2838 | 10296 | — | General | ignored by the client | — |
| — | 0x2839 | 10297 | — | General | ignored by the client | — |
| — | 0x283a | 10298 | — | General | ignored by the client | — |
| — | 0x283b | 10299 | — | General | ignored by the client | — |
| — | 0x283c | 10300 | — | General | ignored by the client | — |
| — | 0x283d | 10301 | — | General | ignored by the client | — |
| — | 0x283e | 10302 | — | General | ignored by the client | — |
| — | 0x283f | 10303 | — | General | ignored by the client | — |
| — | 0x2840 | 10304 | — | General | ignored by the client | — |
| — | 0x2841 | 10305 | — | General | ignored by the client | — |
| — | 0x2842 | 10306 | — | General | ignored by the client | — |
| — | 0x2843 | 10307 | — | General | ignored by the client | — |
| — | 0x2844 | 10308 | — | General | ignored by the client | — |
| — | 0x2845 | 10309 | — | General | ignored by the client | — |
| — | 0x2846 | 10310 | — | General | ignored by the client | — |
| — | 0x2847 | 10311 | — | General | ignored by the client | — |
| — | 0x2848 | 10312 | — | General | ignored by the client | — |
| — | 0x2849 | 10313 | — | General | ignored by the client | — |
| — | 0x284a | 10314 | — | General | ignored by the client | — |
| — | 0x284b | 10315 | — | General | ignored by the client | — |
| — | 0x284c | 10316 | — | General | ignored by the client | — |
| — | 0x284d | 10317 | — | General | ignored by the client | — |
| — | 0x284e | 10318 | — | General | ignored by the client | — |
| — | 0x284f | 10319 | — | General | ignored by the client | — |
| — | 0x2850 | 10320 | — | General | ignored by the client | — |
| — | 0x2851 | 10321 | — | General | ignored by the client | — |
| — | 0x2852 | 10322 | — | General | ignored by the client | — |
| — | 0x2853 | 10323 | — | General | ignored by the client | — |
| SMSG_BATCH_PRESENCE_SUBSCRIPTION | 0x2854 | 10324 | — | General | variable | — |
| SMSG_MOVEMENT_ENFORCEMENT_ALERT | 0x2855 | 10325 | — | General | variable | — |
| SMSG_BROADCAST_SUMMON_CAST | 0x2856 | 10326 | — | General | ignored by the client | — |
| SMSG_BROADCAST_SUMMON_RESPONSE | 0x2857 | 10327 | — | General | ignored by the client | — |
| SMSG_PREPOPULATE_NAME_CACHE | 0x2858 | 10328 | — | General | variable | — |
| — | 0x2859 | 10329 | — | General | ignored by the client | — |
| — | 0x285a | 10330 | — | General | ignored by the client | — |
| SMSG_RETURN_RECRUITING_CLUBS | 0x285b | 10331 | — | General | ignored by the client | — |
| SMSG_RETURN_APPLICANT_LIST | 0x285c | 10332 | — | General | ignored by the client | — |
| SMSG_CLUB_FINDER_RESPONSE_CHARACTER_APPLICATION_LIST | 0x285d | 10333 | — | General | ignored by the client | — |
| SMSG_CLUB_FINDER_UPDATE_APPLICATIONS | 0x285e | 10334 | — | General | ignored by the client | — |
| SMSG_CLUB_FINDER_ERROR_MESSAGE | 0x285f | 10335 | — | General | ignored by the client | — |
| SMSG_CLUB_FINDER_LOOKUP_CLUB_POSTINGS_LIST | 0x2860 | 10336 | — | General | ignored by the client | — |
| SMSG_CLUB_FINDER_RESPONSE_POST_RECRUITMENT_MESSAGE | 0x2861 | 10337 | — | General | ignored by the client | — |
| SMSG_CLUB_FINDER_GET_CLUB_POSTING_IDS_RESPONSE | 0x2862 | 10338 | — | General | ignored by the client | — |
| SMSG_APPLY_MOUNT_EQUIPMENT_RESULT | 0x2863 | 10339 | — | General | ignored by the client | — |
| — | 0x2864 | 10340 | — | General | ignored by the client | — |
| SMSG_LEVEL_LINKING_RESULT | 0x2865 | 10341 | — | General | ignored by the client | — |
| SMSG_RAF_ACCOUNT_INFO | 0x2866 | 10342 | — | General | ignored by the client | — |
| SMSG_CLAIM_RAF_REWARD_RESPONSE | 0x2867 | 10343 | — | General | ignored by the client | — |
| — | 0x2868 | 10344 | — | General | fixed, 1 bytes | — |
| — | 0x2869 | 10345 | — | General | fixed, 0 bytes | — |
| — | 0x286a | 10346 | — | General | fixed, 1 bytes | — |
| — | 0x286b | 10347 | — | General | ignored by the client | — |
| — | 0x286c | 10348 | — | General | ignored by the client | — |
| — | 0x286d | 10349 | — | General | ignored by the client | — |
| — | 0x286e | 10350 | — | General | ignored by the client | — |
| — | 0x286f | 10351 | — | General | ignored by the client | — |
| — | 0x2870 | 10352 | — | General | ignored by the client | — |
| SMSG_AUCTION_LIST_ITEMS_RESULT | 0x2871 | 10353 | 604 (0x25c) | General | variable | differs |
| — | 0x2872 | 10354 | — | General | ignored by the client | — |
| — | 0x2873 | 10355 | — | General | ignored by the client | — |
| — | 0x2874 | 10356 | — | General | ignored by the client | — |
| — | 0x2875 | 10357 | — | General | ignored by the client | — |
| SMSG_ACCOUNT_CRITERIA_UPDATE | 0x2876 | 10358 | — | General | variable | — |
| SMSG_RAF_ACTIVITY_STATE_CHANGED | 0x2877 | 10359 | — | General | ignored by the client | — |
| — | 0x2878 | 10360 | — | General | ignored by the client | — |
| SMSG_SYNC_WOW_ENTITLEMENTS | 0x2879 | 10361 | — | General | variable | — |
| SMSG_WOW_ENTITLEMENT_NOTIFICATION | 0x287a | 10362 | — | General | variable | — |
| SMSG_QUEST_SESSION_INFO_RESPONSE | 0x287b | 10363 | — | General | ignored by the client | — |
| — | 0x287c | 10364 | — | General | ignored by the client | — |
| SMSG_AUCTION_FAVORITE_LIST | 0x287d | 10365 | — | General | ignored by the client | — |
| — | 0x287e | 10366 | — | General | variable | — |
| — | 0x287f | 10367 | — | General | ignored by the client | — |
| SMSG_ITEM_INTERACTION_COMPLETE | 0x2880 | 10368 | — | General | ignored by the client | — |
| SMSG_CHROMIE_TIME_SELECT_EXPANSION_SUCCESS | 0x2881 | 10369 | — | General | ignored by the client | — |
| SMSG_SPLASH_SCREEN_SHOW_LATEST | 0x2882 | 10370 | — | General | ignored by the client | — |
| SMSG_WEEKLY_REWARDS_RESULT | 0x2883 | 10371 | — | General | ignored by the client | — |
| SMSG_WEEKLY_REWARD_CLAIM_RESULT | 0x2884 | 10372 | — | General | ignored by the client | — |
| SMSG_WEEKLY_REWARDS_PROGRESS_RESULT | 0x2885 | 10373 | — | General | ignored by the client | — |
| SMSG_PARTY_NOTIFY_LFG_LEADER_CHANGE | 0x2886 | 10374 | — | General | fixed struct | — |
| SMSG_ADVENTURE_JOURNAL_DATA_RESPONSE | 0x2887 | 10375 | — | General | ignored by the client | — |
| SMSG_EXTERNAL_TRANSACTION_ID_GENERATED | 0x2888 | 10376 | — | General | ignored by the client | — |
| SMSG_COVENANT_RENOWN_OPEN_NPC | 0x2889 | 10377 | — | General | ignored by the client | — |
| SMSG_CONVERT_ITEMS_TO_CURRENCY_VALUE | 0x288a | 10378 | — | General | ignored by the client | — |
| SMSG_AUCTION_LIST_OWNED_ITEMS_RESULT | 0x288b | 10379 | 605 (0x25d) | General | variable | — |
| SMSG_AUCTION_LIST_BIDDED_ITEMS_RESULT | 0x288c | 10380 | 613 (0x265) | General | variable | — |
| — | 0x288d | 10381 | — | General | ignored by the client | — |
| — | 0x288e | 10382 | — | General | ignored by the client | — |
| SMSG_AREA_TRIGGER_MESSAGE | 0x288f | 10383 | 696 (0x2b8) | General | fixed struct | matches |
| — | 0x2890 | 10384 | — | General | ignored by the client | — |
| SMSG_VOICE_CHANNEL_STT_TOKEN_RESPONSE | 0x2891 | 10385 | — | General | variable | — |
| SMSG_ACCOUNT_NOTIFICATIONS_RESPONSE | 0x2892 | 10386 | — | General | variable | — |
| SMSG_LATENCY_REPORT_PING | 0x2893 | 10387 | — | General | variable | — |
| — | 0x2894 | 10388 | — | General | ignored by the client | — |
| SMSG_UPDATE_AADC_STATUS_RESPONSE | 0x2895 | 10389 | — | General | fixed, 1 bytes | — |
| — | 0x2896 | 10390 | — | General | ignored by the client | — |
| — | 0x2897 | 10391 | — | General | fixed struct | — |
| — | 0x28fa | 10490 | — | Area trigger | ignored by the client | — |
| — | 0x28fb | 10491 | — | Area trigger | ignored by the client | — |
| — | 0x28fc | 10492 | — | Area trigger | ignored by the client | — |
| SMSG_AREA_TRIGGER_RE_PATH | 0x28fd | 10493 | — | Area trigger | variable | — |
| — | 0x28fe | 10494 | — | Area trigger | fixed, 9 bytes | — |
| — | 0x28ff | 10495 | — | Area trigger | ignored by the client | — |
| SMSG_AREA_TRIGGER_FORCE_SET_POSITION_AND_FACING | 0x2900 | 10496 | — | Area trigger | variable | — |
| SMSG_AREA_TRIGGER_UNATTACH | 0x2901 | 10497 | — | Area trigger | variable | — |
| SMSG_AREA_TRIGGER_RE_SHAPE | 0x2902 | 10498 | — | Area trigger | variable | — |
| SMSG_AREA_TRIGGER_DENIED | 0x2903 | 10499 | — | Area trigger | fixed, 5 bytes | — |
| SMSG_DB_REPLY | 0x290e | 10510 | — | Cache | variable | matches |
| SMSG_AVAILABLE_HOTFIXES | 0x290f | 10511 | — | Cache | variable | matches |
| SMSG_HOTFIX_MESSAGE | 0x2910 | 10512 | — | Cache | variable | matches |
| SMSG_HOTFIX_CONNECT | 0x2911 | 10513 | — | Cache | variable | matches |
| — | 0x2912 | 10514 | — | Cache | ignored by the client | — |
| SMSG_REALM_QUERY_RESPONSE | 0x2913 | 10515 | — | Cache | variable | — |
| SMSG_QUERY_CREATURE_RESPONSE | 0x2914 | 10516 | 97 (0x61) | Cache | variable | matches |
| SMSG_QUERY_GAME_OBJECT_RESPONSE | 0x2915 | 10517 | 95 (0x5f) | Cache | variable | matches |
| SMSG_QUERY_NPC_TEXT_RESPONSE | 0x2916 | 10518 | 384 (0x180) | Cache | variable | matches |
| SMSG_QUERY_PAGE_TEXT_RESPONSE | 0x2917 | 10519 | 91 (0x5b) | Cache | variable | matches |
| SMSG_INVALIDATE_PAGE_TEXT | 0x2918 | 10520 | — | Cache | fixed struct | — |
| SMSG_QUERY_PET_NAME_RESPONSE | 0x2919 | 10521 | 83 (0x53) | Cache | variable | matches |
| SMSG_QUERY_BATTLE_PET_NAME_RESPONSE | 0x291a | 10522 | — | Cache | variable | — |
| SMSG_QUERY_PETITION_RESPONSE | 0x291b | 10523 | 455 (0x1c7) | Cache | variable | matches |
| SMSG_CACHE_VERSION | 0x291c | 10524 | — | Cache | fixed struct | matches |
| SMSG_CACHE_INFO | 0x291d | 10525 | — | Cache | variable | — |
| SMSG_QUERY_ITEM_TEXT_RESPONSE | 0x291e | 10526 | 580 (0x244) | Cache | variable | matches |
| SMSG_TREASURE_PICKER_RESPONSE | 0x291f | 10527 | — | Cache | variable | — |
| SMSG_QUERY_ARENA_TEAM_RESPONSE | 0x2920 | 10528 | — | Cache | variable | matches |
| SMSG_BATTLEFIELD_STATUS_NEED_CONFIRMATION | 0x2922 | 10530 | — | Combat | variable | matches |
| SMSG_BATTLEFIELD_STATUS_ACTIVE | 0x2923 | 10531 | — | Combat | variable | differs |
| SMSG_BATTLEFIELD_STATUS_QUEUED | 0x2924 | 10532 | 744 (0x2e8) | Combat | variable | matches |
| SMSG_BATTLEFIELD_STATUS_NONE | 0x2925 | 10533 | — | Combat | fixed, 16 bytes | — |
| SMSG_BATTLEFIELD_STATUS_FAILED | 0x2926 | 10534 | — | Combat | fixed, 28 bytes | matches |
| SMSG_BATTLEFIELD_LIST | 0x2927 | 10535 | 573 (0x23d) | Combat | variable | matches |
| SMSG_BATTLEGROUND_PLAYER_POSITIONS | 0x2928 | 10536 | — | Combat | variable | matches |
| SMSG_UPDATE_CAPTURE_POINT | 0x2929 | 10537 | — | Combat | ignored by the client | — |
| SMSG_CAPTURE_POINT_REMOVED | 0x292a | 10538 | — | Combat | ignored by the client | — |
| SMSG_BATTLEGROUND_PLAYER_JOINED | 0x292b | 10539 | 748 (0x2ec) | Combat | fixed, 0 bytes | — |
| SMSG_BATTLEGROUND_PLAYER_LEFT | 0x292c | 10540 | 749 (0x2ed) | Combat | fixed, 0 bytes | — |
| SMSG_BATTLEFIELD_PORT_DENIED | 0x292d | 10541 | — | Combat | fixed struct | — |
| SMSG_BATTLEGROUND_INFO_THROTTLED | 0x292e | 10542 | — | Combat | fixed struct | — |
| SMSG_BATTLEFIELD_STATUS_WAIT_FOR_GROUPS | 0x292f | 10543 | — | Combat | variable | — |
| SMSG_RATED_PVP_INFO | 0x2930 | 10544 | — | Combat | variable | differs |
| — | 0x2931 | 10545 | — | Combat | ignored by the client | — |
| SMSG_INSPECT_HONOR_STATS | 0x2932 | 10546 | — | Combat | fixed, 42 bytes | matches |
| SMSG_PVP_MATCH_STATISTICS | 0x2933 | 10547 | — | Combat | variable | matches |
| SMSG_WARGAME_REQUEST_SUCCESSFULLY_SENT_TO_OPPONENT | 0x2934 | 10548 | — | Combat | fixed, 0 bytes | — |
| — | 0x2935 | 10549 | — | Combat | ignored by the client | — |
| SMSG_PVP_OPTIONS_ENABLED | 0x2936 | 10550 | — | Combat | fixed, 1 bytes | — |
| SMSG_REQUEST_PVP_REWARDS_RESPONSE | 0x2937 | 10551 | — | Combat | variable | — |
| SMSG_REQUEST_SCHEDULED_PVP_INFO_RESPONSE | 0x2938 | 10552 | — | Combat | variable | — |
| — | 0x2939 | 10553 | — | Combat | ignored by the client | — |
| SMSG_BREAK_TARGET | 0x293a | 10554 | — | Combat | fixed, 0 bytes | matches |
| SMSG_ATTACK_START | 0x293b | 10555 | 323 (0x143) | Combat | fixed, 0 bytes | matches |
| SMSG_ATTACK_STOP | 0x293c | 10556 | 324 (0x144) | Combat | fixed, 1 bytes | matches |
| SMSG_COMBAT_EVENT_FAILED | 0x293d | 10557 | — | Combat | fixed, 0 bytes | — |
| SMSG_DUEL_REQUESTED | 0x293e | 10558 | 359 (0x167) | Combat | fixed, 0 bytes | matches |
| SMSG_DUEL_ARRANGED | 0x293f | 10559 | — | Combat | fixed, 0 bytes | — |
| SMSG_DUEL_OUT_OF_BOUNDS | 0x2940 | 10560 | 360 (0x168) | Combat | fixed struct | matches |
| SMSG_DUEL_IN_BOUNDS | 0x2941 | 10561 | 361 (0x169) | Combat | fixed struct | matches |
| SMSG_DUEL_COUNTDOWN | 0x2942 | 10562 | 695 (0x2b7) | Combat | fixed struct | matches |
| SMSG_DUEL_COMPLETE | 0x2943 | 10563 | 362 (0x16a) | Combat | fixed, 1 bytes | matches |
| SMSG_DUEL_WINNER | 0x2944 | 10564 | 363 (0x16b) | Combat | variable | matches |
| SMSG_CAN_DUEL_RESULT | 0x2945 | 10565 | — | Combat | fixed, 1 bytes | matches |
| SMSG_CLEAR_TARGET | 0x2946 | 10566 | — | Combat | fixed, 0 bytes | matches |
| SMSG_RESET_RANGED_COMBAT_TIMER | 0x2947 | 10567 | — | Combat | fixed struct | — |
| SMSG_PVP_CREDIT | 0x2948 | 10568 | 652 (0x28c) | Combat | fixed, 12 bytes | matches |
| SMSG_CANCEL_COMBAT | 0x2949 | 10569 | 334 (0x14e) | Combat | fixed struct | matches |
| SMSG_ATTACK_SWING_ERROR | 0x294a | 10570 | — | Combat | fixed, 1 bytes | matches |
| SMSG_ATTACK_SWING_LANDED_LOG | 0x294b | 10571 | — | Combat | variable | — |
| SMSG_BATTLEGROUND_POINTS | 0x294c | 10572 | — | Combat | fixed, 3 bytes | — |
| SMSG_BATTLEGROUND_INIT | 0x294d | 10573 | — | Combat | fixed struct | matches |
| SMSG_MAP_OBJECTIVES_INIT | 0x294e | 10574 | — | Combat | variable | — |
| SMSG_BOSS_KILL | 0x294f | 10575 | — | Combat | fixed struct | — |
| SMSG_ATTACKER_STATE_UPDATE | 0x2950 | 10576 | 330 (0x14a) | Combat | variable | matches |
| SMSG_PVP_MATCH_START | 0x2951 | 10577 | — | Combat | variable | — |
| SMSG_PVP_MATCH_COMPLETE | 0x2952 | 10578 | — | Combat | ignored by the client | — |
| SMSG_PVP_MATCH_INITIALIZE | 0x2953 | 10579 | — | Combat | fixed, 27 bytes | — |
| SMSG_ALL_GUILD_ACHIEVEMENTS | 0x29b8 | 10680 | — | Guild | variable | — |
| SMSG_GUILD_SEND_RANK_CHANGE | 0x29b9 | 10681 | — | Guild | fixed, 5 bytes | matches |
| SMSG_GUILD_COMMAND_RESULT | 0x29ba | 10682 | 147 (0x93) | Guild | variable | matches |
| SMSG_GUILD_ROSTER | 0x29bb | 10683 | 138 (0x8a) | Guild | variable | matches |
| SMSG_GUILD_ROSTER_UPDATE | 0x29bc | 10684 | — | Guild | variable | — |
| SMSG_GUILD_MEMBER_RECIPES | 0x29bd | 10685 | — | Guild | variable | — |
| SMSG_GUILD_KNOWN_RECIPES | 0x29be | 10686 | — | Guild | variable | — |
| SMSG_GUILD_MEMBERS_WITH_RECIPE | 0x29bf | 10687 | — | Guild | variable | — |
| SMSG_GUILD_REWARD_LIST | 0x29c0 | 10688 | — | Guild | variable | — |
| SMSG_GUILD_NEWS | 0x29c1 | 10689 | — | Guild | variable | — |
| SMSG_GUILD_NEWS_DELETED | 0x29c2 | 10690 | — | Guild | fixed struct | — |
| SMSG_GUILD_CRITERIA_UPDATE | 0x29c3 | 10691 | — | Guild | variable | — |
| SMSG_GUILD_ACHIEVEMENT_EARNED | 0x29c4 | 10692 | — | Guild | fixed, 8 bytes | — |
| SMSG_GUILD_ACHIEVEMENT_DELETED | 0x29c5 | 10693 | — | Guild | fixed, 8 bytes | — |
| SMSG_GUILD_CRITERIA_DELETED | 0x29c6 | 10694 | — | Guild | fixed, 4 bytes | — |
| SMSG_GUILD_ACHIEVEMENT_MEMBERS | 0x29c7 | 10695 | — | Guild | variable | — |
| SMSG_GUILD_RANKS | 0x29c8 | 10696 | — | Guild | variable | matches |
| SMSG_GUILD_MEMBER_UPDATE_NOTE | 0x29c9 | 10697 | — | Guild | variable | — |
| SMSG_GUILD_INVITE | 0x29ca | 10698 | 131 (0x83) | Guild | variable | matches |
| SMSG_GUILD_PARTY_STATE | 0x29cb | 10699 | — | Guild | fixed, 13 bytes | — |
| SMSG_GUILD_REPUTATION_REACTION_CHANGED | 0x29cc | 10700 | — | Guild | fixed, 0 bytes | — |
| — | 0x29cd | 10701 | — | Guild | variable | — |
| — | 0x29ce | 10702 | — | Guild | variable | — |
| — | 0x29cf | 10703 | — | Guild | variable | — |
| — | 0x29d0 | 10704 | — | Guild | fixed struct | — |
| — | 0x29d1 | 10705 | — | Guild | variable | — |
| SMSG_GUILD_CHALLENGE_UPDATE | 0x29d2 | 10706 | — | Guild | fixed struct | — |
| SMSG_GUILD_CHALLENGE_COMPLETED | 0x29d3 | 10707 | — | Guild | fixed struct | — |
| SMSG_GUILD_ITEM_LOOTED_NOTIFY | 0x29d4 | 10708 | — | Guild | variable | — |
| — | 0x29d5 | 10709 | — | Guild | fixed struct | — |
| — | 0x29d6 | 10710 | — | Guild | fixed struct | — |
| — | 0x29d7 | 10711 | — | Guild | ignored by the client | — |
| SMSG_GUILD_RESET | 0x29d8 | 10712 | — | Guild | fixed, 0 bytes | — |
| SMSG_GUILD_MOVE_STARTING | 0x29d9 | 10713 | — | Guild | fixed, 0 bytes | — |
| SMSG_GUILD_MOVED | 0x29da | 10714 | — | Guild | variable | — |
| SMSG_GUILD_NAME_CHANGED | 0x29db | 10715 | — | Guild | variable | — |
| SMSG_GUILD_FLAGGED_FOR_RENAME | 0x29dc | 10716 | — | Guild | fixed, 1 bytes | — |
| SMSG_GUILD_CHANGE_NAME_RESULT | 0x29dd | 10717 | — | Guild | fixed, 1 bytes | — |
| SMSG_GUILD_BANK_QUERY_RESULTS | 0x29de | 10718 | — | Guild | variable | matches |
| SMSG_GUILD_BANK_LOG_QUERY_RESULTS | 0x29df | 10719 | — | Guild | variable | matches |
| SMSG_GUILD_BANK_REMAINING_WITHDRAW_MONEY | 0x29e0 | 10720 | — | Guild | fixed struct | matches |
| SMSG_GUILD_PERMISSIONS_QUERY_RESULTS | 0x29e1 | 10721 | — | Guild | variable | matches |
| SMSG_GUILD_EVENT_LOG_QUERY_RESULTS | 0x29e2 | 10722 | — | Guild | variable | — |
| SMSG_GUILD_BANK_TEXT_QUERY_RESULT | 0x29e3 | 10723 | — | Guild | variable | matches |
| SMSG_GUILD_MEMBER_DAILY_RESET | 0x29e4 | 10724 | — | Guild | fixed struct | — |
| SMSG_QUERY_GUILD_INFO_RESPONSE | 0x29e5 | 10725 | 85 (0x55) | Guild | variable | matches |
| SMSG_QUERY_REALM_GUILD_MASTER_INFO_RESPONSE | 0x29e6 | 10726 | — | Guild | ignored by the client | — |
| SMSG_QUERY_GUILD_FOLLOW_INFO_RESPONSE | 0x29e7 | 10727 | — | Guild | ignored by the client | — |
| SMSG_GUILD_INVITE_DECLINED | 0x29e8 | 10728 | 134 (0x86) | Guild | variable | matches |
| SMSG_GUILD_INVITE_EXPIRED | 0x29e9 | 10729 | — | Guild | fixed struct | — |
| SMSG_GUILD_EVENT_PLAYER_JOINED | 0x29ea | 10730 | — | Guild | variable | matches |
| SMSG_GUILD_EVENT_PLAYER_LEFT | 0x29eb | 10731 | — | Guild | variable | matches |
| SMSG_GUILD_EVENT_NEW_LEADER | 0x29ec | 10732 | — | Guild | variable | matches |
| SMSG_GUILD_EVENT_DISBANDED | 0x29ed | 10733 | — | Guild | fixed struct | matches |
| SMSG_GUILD_EVENT_MOTD | 0x29ee | 10734 | — | Guild | variable | matches |
| SMSG_GUILD_EVENT_PRESENCE_CHANGE | 0x29ef | 10735 | — | Guild | variable | matches |
| SMSG_GUILD_EVENT_RANKS_UPDATED | 0x29f0 | 10736 | — | Guild | fixed struct | matches |
| SMSG_GUILD_EVENT_RANK_CHANGED | 0x29f1 | 10737 | — | Guild | fixed struct | — |
| SMSG_GUILD_EVENT_TAB_ADDED | 0x29f2 | 10738 | — | Guild | fixed struct | matches |
| SMSG_GUILD_EVENT_TAB_DELETED | 0x29f3 | 10739 | — | Guild | fixed struct | — |
| SMSG_GUILD_EVENT_TAB_MODIFIED | 0x29f4 | 10740 | — | Guild | variable | matches |
| SMSG_GUILD_EVENT_TAB_TEXT_CHANGED | 0x29f5 | 10741 | — | Guild | fixed struct | matches |
| SMSG_GUILD_EVENT_BANK_MONEY_CHANGED | 0x29f6 | 10742 | — | Guild | fixed struct | matches |
| SMSG_GUILD_EVENT_BANK_CONTENTS_CHANGED | 0x29f7 | 10743 | — | Guild | fixed struct | — |
| SMSG_PLAYER_SAVE_GUILD_EMBLEM | 0x29f8 | 10744 | — | Guild | fixed struct | matches |
| SMSG_PETITION_RENAME_GUILD_RESPONSE | 0x29f9 | 10745 | — | Guild | variable | matches |
| — | 0x29fa | 10746 | — | Guild | ignored by the client | — |
| SMSG_LFG_JOIN_RESULT | 0x2a1c | 10780 | — | LFG | variable | differs |
| SMSG_LFG_LIST_JOIN_RESULT | 0x2a1d | 10781 | — | LFG | fixed, 18 bytes | — |
| SMSG_LFG_LIST_SEARCH_RESULTS | 0x2a1e | 10782 | — | LFG | variable | — |
| SMSG_LFG_LIST_SEARCH_STATUS | 0x2a1f | 10783 | — | LFG | fixed, 18 bytes | — |
| SMSG_LFG_QUEUE_STATUS | 0x2a20 | 10784 | — | LFG | variable | differs |
| SMSG_LFG_ROLE_CHECK_UPDATE | 0x2a21 | 10785 | — | LFG | variable | differs |
| SMSG_LFG_READY_CHECK_UPDATE | 0x2a22 | 10786 | — | LFG | variable | — |
| — | 0x2a23 | 10787 | — | LFG | ignored by the client | — |
| SMSG_LFG_UPDATE_STATUS | 0x2a24 | 10788 | — | LFG | variable | differs |
| SMSG_LFG_INSTANCE_SHUTDOWN_COUNTDOWN | 0x2a25 | 10789 | — | LFG | fixed, 20 bytes | — |
| SMSG_LFG_LIST_UPDATE_STATUS | 0x2a26 | 10790 | — | LFG | variable | — |
| SMSG_LFG_LIST_UPDATE_EXPIRATION | 0x2a27 | 10791 | — | LFG | fixed, 25 bytes | — |
| SMSG_LFG_LIST_APPLICATION_STATUS_UPDATE | 0x2a28 | 10792 | — | LFG | ignored by the client | — |
| SMSG_LFG_LIST_APPLY_TO_GROUP_RESULT | 0x2a29 | 10793 | — | LFG | ignored by the client | — |
| SMSG_LFG_LIST_UPDATE_BLACKLIST | 0x2a2a | 10794 | — | LFG | variable | matches |
| SMSG_LFG_LIST_APPLICANT_LIST_UPDATE | 0x2a2b | 10795 | — | LFG | ignored by the client | — |
| SMSG_LFG_LIST_SEARCH_RESULTS_UPDATE | 0x2a2c | 10796 | — | LFG | variable | — |
| SMSG_LFG_PROPOSAL_UPDATE | 0x2a2d | 10797 | — | LFG | variable | differs |
| SMSG_SET_DF_FAST_LAUNCH_RESULT | 0x2a2e | 10798 | — | LFG | fixed, 1 bytes | — |
| — | 0x2a2f | 10799 | — | LFG | ignored by the client | — |
| SMSG_LFG_SLOT_INVALID | 0x2a30 | 10800 | — | LFG | fixed struct | — |
| SMSG_OPEN_LFG_DUNGEON_FINDER | 0x2a31 | 10801 | — | LFG | fixed struct | — |
| SMSG_LFG_TELEPORT_DENIED | 0x2a32 | 10802 | — | LFG | fixed, 1 bytes | matches |
| SMSG_LFG_DISABLED | 0x2a33 | 10803 | — | LFG | fixed struct | matches |
| SMSG_LFG_OFFER_CONTINUE | 0x2a34 | 10804 | — | LFG | fixed struct | matches |
| SMSG_LFG_BOOT_PLAYER | 0x2a35 | 10805 | — | LFG | variable | — |
| SMSG_LFG_PARTY_INFO | 0x2a36 | 10806 | — | LFG | variable | matches |
| SMSG_LFG_PLAYER_INFO | 0x2a37 | 10807 | — | LFG | variable | differs |
| SMSG_LFG_PLAYER_REWARD | 0x2a38 | 10808 | — | LFG | variable | differs |
| SMSG_ROLE_CHOSEN | 0x2a39 | 10809 | — | LFG | fixed, 5 bytes | differs |
| SMSG_LFG_READY_CHECK_RESULT | 0x2a3a | 10810 | — | LFG | fixed, 1 bytes | — |
| SMSG_LFG_EXPAND_SEARCH_PROMPT | 0x2a3b | 10811 | — | LFG | ignored by the client | — |
| SMSG_DAILY_QUESTS_RESET | 0x2a80 | 10880 | — | Quest | fixed struct | — |
| SMSG_QUEST_COMPLETION_NPC_RESPONSE | 0x2a81 | 10881 | — | Quest | variable | — |
| — | 0x2a82 | 10882 | — | Quest | ignored by the client | — |
| SMSG_QUEST_GIVER_QUEST_COMPLETE | 0x2a83 | 10883 | 401 (0x191) | Quest | variable | matches |
| SMSG_IS_QUEST_COMPLETE_RESPONSE | 0x2a84 | 10884 | — | Quest | fixed, 5 bytes | — |
| SMSG_QUEST_GIVER_INVALID_QUEST | 0x2a85 | 10885 | 399 (0x18f) | Quest | variable | matches |
| SMSG_QUEST_GIVER_QUEST_FAILED | 0x2a86 | 10886 | 402 (0x192) | Quest | fixed struct | matches |
| SMSG_QUEST_LOG_FULL | 0x2a87 | 10887 | 405 (0x195) | Quest | fixed struct | — |
| SMSG_QUEST_NON_LOG_UPDATE_COMPLETE | 0x2a88 | 10888 | — | Quest | fixed struct | — |
| SMSG_QUEST_UPDATE_COMPLETE | 0x2a89 | 10889 | 408 (0x198) | Quest | fixed struct | — |
| SMSG_QUEST_UPDATE_FAILED | 0x2a8a | 10890 | 406 (0x196) | Quest | fixed struct | — |
| SMSG_QUEST_UPDATE_FAILED_TIMER | 0x2a8b | 10891 | 407 (0x197) | Quest | fixed struct | — |
| SMSG_QUEST_UPDATE_ADD_CREDIT | 0x2a8c | 10892 | — | Quest | fixed, 13 bytes | matches |
| SMSG_QUEST_UPDATE_ADD_CREDIT_SIMPLE | 0x2a8d | 10893 | — | Quest | fixed struct | matches |
| SMSG_QUEST_UPDATE_ADD_PVP_CREDIT | 0x2a8e | 10894 | — | Quest | fixed struct | — |
| SMSG_QUEST_CONFIRM_ACCEPT | 0x2a8f | 10895 | 412 (0x19c) | Quest | variable | matches |
| SMSG_QUEST_PUSH_RESULT | 0x2a90 | 10896 | — | Quest | variable | differs |
| SMSG_QUEST_GIVER_STATUS_MULTIPLE | 0x2a91 | 10897 | — | Quest | variable | matches |
| SMSG_QUEST_GIVER_QUEST_DETAILS | 0x2a92 | 10898 | 392 (0x188) | Quest | variable | matches |
| SMSG_QUEST_GIVER_REQUEST_ITEMS | 0x2a93 | 10899 | 395 (0x18b) | Quest | variable | matches |
| SMSG_QUEST_GIVER_OFFER_REWARD_MESSAGE | 0x2a94 | 10900 | 397 (0x18d) | Quest | variable | matches |
| SMSG_SHOW_QUEST_COMPLETION_TEXT | 0x2a95 | 10901 | — | Quest | variable | — |
| SMSG_QUERY_QUEST_INFO_RESPONSE | 0x2a96 | 10902 | 93 (0x5d) | Quest | variable | matches |
| SMSG_GOSSIP_COMPLETE | 0x2a97 | 10903 | 382 (0x17e) | Quest | fixed, 1 bytes | matches |
| SMSG_GOSSIP_MESSAGE | 0x2a98 | 10904 | 381 (0x17d) | Quest | variable | matches |
| SMSG_GOSSIP_QUEST_UPDATE | 0x2a99 | 10905 | — | Quest | variable | — |
| SMSG_QUEST_GIVER_QUEST_LIST_MESSAGE | 0x2a9a | 10906 | 389 (0x185) | Quest | variable | matches |
| SMSG_QUEST_GIVER_STATUS | 0x2a9b | 10907 | 387 (0x183) | Quest | fixed, 4 bytes | matches |
| SMSG_QUEST_FORCE_REMOVED | 0x2a9c | 10908 | — | Quest | fixed struct | — |
| SMSG_QUEST_POI_QUERY_RESPONSE | 0x2a9d | 10909 | — | Quest | ignored by the client | matches |
| SMSG_DISPLAY_QUEST_POPUP | 0x2a9e | 10910 | — | Quest | fixed struct | — |
| SMSG_QUEST_POI_UPDATE_RESPONSE | 0x2a9f | 10911 | — | Quest | ignored by the client | — |
| SMSG_RESET_QUEST_POI | 0x2aa0 | 10912 | — | Quest | ignored by the client | — |
| SMSG_CLEAR_TREASURE_PICKER_CACHE | 0x2aa1 | 10913 | — | Quest | ignored by the client | — |
| SMSG_UI_MAP_QUEST_LINES_RESPONSE | 0x2aa2 | 10914 | — | Quest | ignored by the client | — |
| SMSG_COVENANT_CALLINGS_AVAILABILITY_RESPONSE | 0x2aa3 | 10915 | — | Quest | variable | — |
| — | 0x2aa4 | 10916 | — | Quest | ignored by the client | — |
| — | 0x2aa5 | 10917 | — | Quest | ignored by the client | — |
| SMSG_GOSSIP_REFRESH_OPTIONS | 0x2aa6 | 10918 | — | Quest | ignored by the client | — |
| SMSG_CHAT_IGNORED_ACCOUNT_MUTED | 0x2bac | 11180 | — | Chat | fixed struct | — |
| SMSG_CHAT | 0x2bad | 11181 | 150 (0x96) | Chat | variable | matches |
| SMSG_WHO | 0x2bae | 11182 | 99 (0x63) | Chat | variable | matches |
| SMSG_MOTD | 0x2baf | 11183 | 829 (0x33d) | Chat | variable | matches |
| SMSG_CHAT_PLAYER_AMBIGUOUS | 0x2bb0 | 11184 | 813 (0x32d) | Chat | variable | — |
| SMSG_EXPECTED_SPAM_RECORDS | 0x2bb1 | 11185 | 818 (0x332) | Chat | variable | — |
| SMSG_CHAT_NOT_IN_PARTY | 0x2bb2 | 11186 | — | Chat | fixed struct | — |
| SMSG_CHAT_RESTRICTED | 0x2bb3 | 11187 | 765 (0x2fd) | Chat | fixed struct | — |
| SMSG_RAID_INSTANCE_MESSAGE | 0x2bb4 | 11188 | 762 (0x2fa) | Chat | fixed, 10 bytes | matches |
| — | 0x2bb5 | 11189 | — | Chat | fixed struct | — |
| SMSG_DEFENSE_MESSAGE | 0x2bb6 | 11190 | 827 (0x33b) | Chat | variable | matches |
| SMSG_CHAT_PLAYER_NOTFOUND | 0x2bb7 | 11191 | 681 (0x2a9) | Chat | variable | matches |
| SMSG_CHAT_AUTO_RESPONDED | 0x2bb8 | 11192 | — | Chat | variable | — |
| SMSG_USERLIST_ADD | 0x2bb9 | 11193 | — | Chat | variable | — |
| SMSG_USERLIST_REMOVE | 0x2bba | 11194 | — | Chat | variable | — |
| SMSG_USERLIST_UPDATE | 0x2bbb | 11195 | — | Chat | variable | — |
| SMSG_BROADCAST_ACHIEVEMENT | 0x2bbc | 11196 | — | Chat | variable | — |
| SMSG_CHAT_DOWN | 0x2bbd | 11197 | — | Chat | fixed struct | — |
| SMSG_CHAT_IS_DOWN | 0x2bbe | 11198 | — | Chat | fixed struct | — |
| SMSG_CHAT_RECONNECT | 0x2bbf | 11199 | — | Chat | fixed struct | — |
| SMSG_CHANNEL_NOTIFY | 0x2bc0 | 11200 | 153 (0x99) | Chat | variable | matches |
| SMSG_CHANNEL_NOTIFY_JOINED | 0x2bc1 | 11201 | — | Chat | variable | matches |
| SMSG_CHANNEL_NOTIFY_LEFT | 0x2bc2 | 11202 | — | Chat | variable | matches |
| SMSG_CHANNEL_LIST | 0x2bc3 | 11203 | 155 (0x9b) | Chat | variable | matches |
| SMSG_CHAT_SERVER_MESSAGE | 0x2bc4 | 11204 | 657 (0x291) | Chat | variable | matches |
| — | 0x2bc5 | 11205 | — | Chat | ignored by the client | — |
| — | 0x2bc6 | 11206 | — | Chat | ignored by the client | — |
| — | 0x2bc7 | 11207 | — | Chat | ignored by the client | — |
| — | 0x2bc8 | 11208 | — | Chat | ignored by the client | — |
| — | 0x2bc9 | 11209 | — | Chat | ignored by the client | — |
| — | 0x2c10 | 11280 | — | Spell | ignored by the client | — |
| — | 0x2c11 | 11281 | — | Spell | ignored by the client | — |
| SMSG_CHEAT_IGNORE_DIMISHING_RETURNS | 0x2c12 | 11282 | — | Spell | fixed, 1 bytes | — |
| SMSG_MIRROR_IMAGE_CREATURE_DATA | 0x2c13 | 11283 | — | Spell | fixed, 8 bytes | — |
| SMSG_MIRROR_IMAGE_COMPONENTED_DATA | 0x2c14 | 11284 | — | Spell | variable | — |
| SMSG_SPELL_COOLDOWN | 0x2c15 | 11285 | 308 (0x134) | Spell | variable | matches |
| SMSG_CATEGORY_COOLDOWN | 0x2c16 | 11286 | — | Spell | variable | — |
| SMSG_SPELL_CATEGORY_COOLDOWN | 0x2c17 | 11287 | — | Spell | fixed, 13 bytes | — |
| SMSG_WEEKLY_SPELL_USAGE | 0x2c18 | 11288 | — | Spell | variable | — |
| SMSG_UPDATE_WEEKLY_SPELL_USAGE | 0x2c19 | 11289 | — | Spell | fixed, 5 bytes | — |
| SMSG_SPELL_DISPELL_LOG | 0x2c1a | 11290 | 635 (0x27b) | Spell | variable | matches |
| SMSG_SPELL_PERIODIC_AURA_LOG | 0x2c1b | 11291 | 590 (0x24e) | Spell | variable | differs |
| SMSG_SPELL_ENERGIZE_LOG | 0x2c1c | 11292 | 337 (0x151) | Spell | variable | matches |
| SMSG_SPELL_HEAL_LOG | 0x2c1d | 11293 | 336 (0x150) | Spell | variable | differs |
| SMSG_SPELL_HEAL_ABSORB_LOG | 0x2c1e | 11294 | — | Spell | variable | — |
| SMSG_SPELL_ABSORB_LOG | 0x2c1f | 11295 | — | Spell | variable | — |
| SMSG_SPELL_INTERRUPT_LOG | 0x2c20 | 11296 | — | Spell | fixed, 8 bytes | — |
| SMSG_ENVIRONMENTAL_DAMAGE_LOG | 0x2c21 | 11297 | 508 (0x1fc) | Spell | variable | matches |
| SMSG_AURA_UPDATE | 0x2c22 | 11298 | — | Spell | variable | differs |
| SMSG_AURA_POINTS_DEPLETED | 0x2c23 | 11299 | — | Spell | fixed, 2 bytes | — |
| SMSG_PET_CLEAR_SPELLS | 0x2c24 | 11300 | — | Spell | fixed struct | matches |
| SMSG_PET_SPELLS_MESSAGE | 0x2c25 | 11301 | 377 (0x179) | Spell | variable | matches |
| SMSG_CLEAR_COOLDOWNS | 0x2c26 | 11302 | — | Spell | variable | — |
| SMSG_CLEAR_ALL_SPELL_CHARGES | 0x2c27 | 11303 | — | Spell | fixed, 1 bytes | — |
| SMSG_CLEAR_SPELL_CHARGES | 0x2c28 | 11304 | — | Spell | fixed, 5 bytes | — |
| SMSG_SET_SPELL_CHARGES | 0x2c29 | 11305 | — | Spell | fixed, 14 bytes | — |
| SMSG_SEND_KNOWN_SPELLS | 0x2c2a | 11306 | 298 (0x12a) | Spell | variable | matches |
| SMSG_SEND_SPELL_HISTORY | 0x2c2b | 11307 | — | Spell | variable | matches |
| SMSG_REFRESH_SPELL_HISTORY | 0x2c2c | 11308 | — | Spell | variable | — |
| SMSG_SEND_SPELL_CHARGES | 0x2c2d | 11309 | — | Spell | variable | matches |
| SMSG_SEND_UNLEARN_SPELLS | 0x2c2e | 11310 | — | Spell | variable | matches |
| SMSG_SPELL_OR_DAMAGE_IMMUNE | 0x2c2f | 11311 | 611 (0x263) | Spell | fixed, 5 bytes | — |
| SMSG_DISPEL_FAILED | 0x2c30 | 11312 | 610 (0x262) | Spell | variable | matches |
| SMSG_SPELL_DAMAGE_SHIELD | 0x2c31 | 11313 | 591 (0x24f) | Spell | variable | matches |
| SMSG_SPELL_NON_MELEE_DAMAGE_LOG | 0x2c32 | 11314 | 592 (0x250) | Spell | variable | differs |
| SMSG_SPELL_INSTAKILL_LOG | 0x2c33 | 11315 | 815 (0x32f) | Spell | fixed, 4 bytes | matches |
| SMSG_SPELL_CHANNEL_START | 0x2c34 | 11316 | — | Spell | variable | matches |
| SMSG_SPELL_CHANNEL_UPDATE | 0x2c35 | 11317 | — | Spell | fixed, 4 bytes | matches |
| SMSG_SET_FLAT_SPELL_MODIFIER | 0x2c36 | 11318 | 614 (0x266) | Spell | variable | — |
| SMSG_SET_PCT_SPELL_MODIFIER | 0x2c37 | 11319 | 615 (0x267) | Spell | variable | — |
| SMSG_SPELL_PREPARE | 0x2c38 | 11320 | — | Spell | fixed, 0 bytes | matches |
| SMSG_SPELL_GO | 0x2c39 | 11321 | 306 (0x132) | Spell | variable | matches |
| SMSG_SPELL_START | 0x2c3a | 11322 | 305 (0x131) | Spell | variable | matches |
| SMSG_RESUME_CAST | 0x2c3b | 11323 | — | Spell | fixed, 8 bytes | — |
| — | 0x2c3c | 11324 | — | Spell | ignored by the client | — |
| — | 0x2c3d | 11325 | — | Spell | ignored by the client | — |
| SMSG_RESUME_CAST_BAR | 0x2c3e | 11326 | — | Spell | variable | — |
| SMSG_SPELL_DELAYED | 0x2c3f | 11327 | 482 (0x1e2) | Spell | fixed, 4 bytes | matches |
| SMSG_SPELL_EXECUTE_LOG | 0x2c40 | 11328 | 588 (0x24c) | Spell | variable | matches |
| SMSG_SPELL_MISS_LOG | 0x2c41 | 11329 | 587 (0x24b) | Spell | variable | matches |
| — | 0x2c42 | 11330 | — | Spell | ignored by the client | — |
| SMSG_NOTIFY_DEST_LOC_SPELL_CAST | 0x2c43 | 11331 | — | Spell | fixed, 45 bytes | — |
| SMSG_CANCEL_SPELL_VISUAL | 0x2c44 | 11332 | — | Spell | fixed, 4 bytes | — |
| SMSG_PLAY_SPELL_VISUAL | 0x2c45 | 11333 | 499 (0x1f3) | Spell | fixed, 35 bytes | — |
| SMSG_CANCEL_ORPHAN_SPELL_VISUAL | 0x2c46 | 11334 | — | Spell | fixed struct | — |
| SMSG_PLAY_ORPHAN_SPELL_VISUAL | 0x2c47 | 11335 | — | Spell | fixed, 53 bytes | — |
| SMSG_CANCEL_SPELL_VISUAL_KIT | 0x2c48 | 11336 | — | Spell | fixed, 5 bytes | — |
| SMSG_PLAY_SPELL_VISUAL_KIT | 0x2c49 | 11337 | — | Spell | fixed, 13 bytes | matches |
| SMSG_GAME_OBJECT_PLAY_SPELL_VISUAL_KIT | 0x2c4a | 11338 | — | Spell | fixed, 12 bytes | — |
| SMSG_GAME_OBJECT_PLAY_SPELL_VISUAL | 0x2c4b | 11339 | — | Spell | fixed, 4 bytes | — |
| SMSG_SUPERCEDED_SPELLS | 0x2c4c | 11340 | 300 (0x12c) | Spell | variable | matches |
| SMSG_LEARNED_SPELLS | 0x2c4d | 11341 | — | Spell | variable | matches |
| SMSG_UNLEARNED_SPELLS | 0x2c4e | 11342 | 515 (0x203) | Spell | variable | matches |
| SMSG_PET_LEARNED_SPELLS | 0x2c4f | 11343 | — | Spell | variable | matches |
| SMSG_PET_UNLEARNED_SPELLS | 0x2c50 | 11344 | — | Spell | variable | matches |
| SMSG_PUSH_SPELL_TO_ACTION_BAR | 0x2c51 | 11345 | — | Spell | fixed struct | — |
| SMSG_REMOVE_SPELL_FROM_ACTION_BAR | 0x2c52 | 11346 | — | Spell | ignored by the client | — |
| SMSG_SPELL_FAILURE | 0x2c53 | 11347 | 307 (0x133) | Spell | fixed, 10 bytes | matches |
| SMSG_ACTIVE_GLYPHS | 0x2c54 | 11348 | — | Spell | variable | matches |
| SMSG_SPELL_FAILED_OTHER | 0x2c55 | 11349 | 678 (0x2a6) | Spell | fixed, 9 bytes | matches |
| SMSG_SCRIPT_CAST | 0x2c56 | 11350 | — | Spell | fixed struct | — |
| SMSG_CAST_FAILED | 0x2c57 | 11351 | 304 (0x130) | Spell | fixed, 20 bytes | matches |
| SMSG_PET_CAST_FAILED | 0x2c58 | 11352 | 312 (0x138) | Spell | fixed, 16 bytes | matches |
| SMSG_INTERRUPT_POWER_REGEN | 0x2c59 | 11353 | — | Spell | fixed struct | — |
| SMSG_SPELL_FAILURE_MESSAGE | 0x2c5a | 11354 | — | Spell | fixed struct | — |
| — | 0x2c5b | 11355 | — | Spell | ignored by the client | — |
| — | 0x2c5c | 11356 | — | Spell | ignored by the client | — |
| — | 0x2c5d | 11357 | — | Spell | ignored by the client | — |
| — | 0x2c5e | 11358 | — | Spell | ignored by the client | — |
| SMSG_RESYNC_RUNES | 0x2c5f | 11359 | — | Spell | variable | — |
| SMSG_RESTART_GLOBAL_COOLDOWN | 0x2c60 | 11360 | — | Spell | ignored by the client | — |
| SMSG_VOID_STORAGE_FAILED | 0x2da0 | 11680 | — | Storage | fixed struct | — |
| SMSG_VOID_STORAGE_CONTENTS | 0x2da1 | 11681 | — | Storage | variable | — |
| SMSG_VOID_STORAGE_TRANSFER_CHANGES | 0x2da2 | 11682 | — | Storage | variable | — |
| SMSG_VOID_TRANSFER_RESULT | 0x2da3 | 11683 | — | Storage | fixed struct | — |
| SMSG_VOID_ITEM_SWAP_RESPONSE | 0x2da4 | 11684 | — | Storage | fixed, 8 bytes | — |
| SMSG_INVENTORY_CHANGE_FAILURE | 0x2da5 | 11685 | 274 (0x112) | Storage | variable | matches |
| SMSG_OPEN_CONTAINER | 0x2da6 | 11686 | 275 (0x113) | Storage | fixed, 0 bytes | — |
| SMSG_BAG_CLEANUP_FINISHED | 0x2da7 | 11687 | — | Storage | fixed struct | — |
| SMSG_TIME_SYNC_REQUEST | 0x2dd2 | 11730 | — | Movement | fixed struct | matches |
| SMSG_TIME_ADJUSTMENT | 0x2dd3 | 11731 | — | Movement | fixed struct | — |
| SMSG_ON_MONSTER_MOVE | 0x2dd4 | 11732 | 221 (0xdd) | Movement | variable | differs |
| SMSG_MOVE_SET_ACTIVE_MOVER | 0x2dd5 | 11733 | — | Movement | fixed, 0 bytes | matches |
| SMSG_MOVE_UPDATE_RUN_SPEED | 0x2dd6 | 11734 | — | Movement | variable | — |
| SMSG_MOVE_UPDATE_RUN_BACK_SPEED | 0x2dd7 | 11735 | — | Movement | variable | — |
| SMSG_MOVE_UPDATE_WALK_SPEED | 0x2dd8 | 11736 | — | Movement | variable | — |
| SMSG_MOVE_UPDATE_SWIM_SPEED | 0x2dd9 | 11737 | — | Movement | variable | — |
| SMSG_MOVE_UPDATE_SWIM_BACK_SPEED | 0x2dda | 11738 | — | Movement | variable | — |
| SMSG_MOVE_UPDATE_FLIGHT_SPEED | 0x2ddb | 11739 | — | Movement | variable | — |
| SMSG_MOVE_UPDATE_FLIGHT_BACK_SPEED | 0x2ddc | 11740 | — | Movement | variable | — |
| SMSG_MOVE_UPDATE_TURN_RATE | 0x2ddd | 11741 | — | Movement | variable | — |
| SMSG_MOVE_UPDATE_PITCH_RATE | 0x2dde | 11742 | — | Movement | variable | — |
| SMSG_MOVE_UPDATE_COLLISION_HEIGHT | 0x2ddf | 11743 | — | Movement | variable | — |
| SMSG_MOVE_UPDATE | 0x2de0 | 11744 | — | Movement | variable | not checked |
| SMSG_MOVE_UPDATE_TELEPORT | 0x2de1 | 11745 | — | Movement | variable | — |
| SMSG_MOVE_UPDATE_KNOCK_BACK | 0x2de2 | 11746 | — | Movement | variable | not checked |
| SMSG_MOVE_UPDATE_MOD_MOVEMENT_FORCE_MAGNITUDE | 0x2de3 | 11747 | — | Movement | variable | — |
| SMSG_MOVE_UPDATE_APPLY_MOVEMENT_FORCE | 0x2de4 | 11748 | — | Movement | variable | — |
| SMSG_MOVE_UPDATE_REMOVE_MOVEMENT_FORCE | 0x2de5 | 11749 | — | Movement | variable | — |
| SMSG_MOVE_SET_MOD_MOVEMENT_FORCE_MAGNITUDE | 0x2de6 | 11750 | — | Movement | fixed, 8 bytes | — |
| SMSG_MOVE_SPLINE_SET_RUN_SPEED | 0x2de7 | 11751 | 766 (0x2fe) | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_SPLINE_SET_RUN_BACK_SPEED | 0x2de8 | 11752 | 767 (0x2ff) | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_SPLINE_SET_SWIM_SPEED | 0x2de9 | 11753 | 768 (0x300) | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_SPLINE_SET_SWIM_BACK_SPEED | 0x2dea | 11754 | 770 (0x302) | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_SPLINE_SET_FLIGHT_SPEED | 0x2deb | 11755 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_SPLINE_SET_FLIGHT_BACK_SPEED | 0x2dec | 11756 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_SPLINE_SET_WALK_SPEED | 0x2ded | 11757 | 769 (0x301) | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_SPLINE_SET_TURN_RATE | 0x2dee | 11758 | 771 (0x303) | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_SPLINE_SET_PITCH_RATE | 0x2def | 11759 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_SET_RUN_SPEED | 0x2df0 | 11760 | — | Movement | fixed, 8 bytes | — |
| SMSG_MOVE_SET_RUN_BACK_SPEED | 0x2df1 | 11761 | — | Movement | fixed, 8 bytes | — |
| SMSG_MOVE_SET_SWIM_SPEED | 0x2df2 | 11762 | — | Movement | fixed, 8 bytes | — |
| SMSG_MOVE_SET_SWIM_BACK_SPEED | 0x2df3 | 11763 | — | Movement | fixed, 8 bytes | — |
| SMSG_MOVE_SET_FLIGHT_SPEED | 0x2df4 | 11764 | — | Movement | fixed, 8 bytes | — |
| SMSG_MOVE_SET_FLIGHT_BACK_SPEED | 0x2df5 | 11765 | — | Movement | fixed, 8 bytes | — |
| SMSG_MOVE_SET_WALK_SPEED | 0x2df6 | 11766 | — | Movement | fixed, 8 bytes | — |
| SMSG_MOVE_SET_TURN_RATE | 0x2df7 | 11767 | — | Movement | fixed, 8 bytes | — |
| SMSG_MOVE_SET_PITCH_RATE | 0x2df8 | 11768 | — | Movement | fixed, 8 bytes | — |
| SMSG_MOVE_ROOT | 0x2df9 | 11769 | 232 (0xe8) | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_UNROOT | 0x2dfa | 11770 | 234 (0xea) | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_SET_WATER_WALK | 0x2dfb | 11771 | 222 (0xde) | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_ENABLE_DOUBLE_JUMP | 0x2dfc | 11772 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_DISABLE_DOUBLE_JUMP | 0x2dfd | 11773 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_SET_LAND_WALK | 0x2dfe | 11774 | 223 (0xdf) | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_SET_FEATHER_FALL | 0x2dff | 11775 | 242 (0xf2) | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_SET_NORMAL_FALL | 0x2e00 | 11776 | 243 (0xf3) | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_SET_HOVERING | 0x2e01 | 11777 | 244 (0xf4) | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_UNSET_HOVERING | 0x2e02 | 11778 | 245 (0xf5) | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_KNOCK_BACK | 0x2e03 | 11779 | 239 (0xef) | Movement | fixed, 20 bytes | matches |
| SMSG_MOVE_TELEPORT | 0x2e04 | 11780 | — | Movement | variable | matches |
| SMSG_MOVE_SET_CAN_FLY | 0x2e05 | 11781 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_UNSET_CAN_FLY | 0x2e06 | 11782 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_SET_CAN_TURN_WHILE_FALLING | 0x2e07 | 11783 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_UNSET_CAN_TURN_WHILE_FALLING | 0x2e08 | 11784 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_SET_IGNORE_MOVEMENT_FORCES | 0x2e09 | 11785 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_UNSET_IGNORE_MOVEMENT_FORCES | 0x2e0a | 11786 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_ENABLE_TRANSITION_BETWEEN_SWIM_AND_FLY | 0x2e0b | 11787 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_DISABLE_TRANSITION_BETWEEN_SWIM_AND_FLY | 0x2e0c | 11788 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_DISABLE_GRAVITY | 0x2e0d | 11789 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_ENABLE_GRAVITY | 0x2e0e | 11790 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_DISABLE_INERTIA | 0x2e0f | 11791 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_ENABLE_INERTIA | 0x2e10 | 11792 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_DISABLE_COLLISION | 0x2e11 | 11793 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_ENABLE_COLLISION | 0x2e12 | 11794 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_SET_COLLISION_HEIGHT | 0x2e13 | 11795 | — | Movement | fixed, 21 bytes | matches |
| SMSG_MOVE_SET_VEHICLE_REC_ID | 0x2e14 | 11796 | — | Movement | fixed, 8 bytes | matches |
| SMSG_MOVE_APPLY_MOVEMENT_FORCE | 0x2e15 | 11797 | — | Movement | variable | — |
| SMSG_MOVE_REMOVE_MOVEMENT_FORCE | 0x2e16 | 11798 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_SET_COMPOUND_STATE | 0x2e17 | 11799 | — | Movement | variable | — |
| SMSG_MOVE_SKIP_TIME | 0x2e18 | 11800 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_SPLINE_ROOT | 0x2e19 | 11801 | 794 (0x31a) | Movement | fixed, 0 bytes | — |
| SMSG_MOVE_SPLINE_UNROOT | 0x2e1a | 11802 | 772 (0x304) | Movement | fixed, 0 bytes | — |
| SMSG_MOVE_SPLINE_DISABLE_GRAVITY | 0x2e1b | 11803 | — | Movement | fixed, 0 bytes | — |
| SMSG_MOVE_SPLINE_ENABLE_GRAVITY | 0x2e1c | 11804 | — | Movement | fixed, 0 bytes | — |
| SMSG_MOVE_SPLINE_DISABLE_COLLISION | 0x2e1d | 11805 | — | Movement | fixed, 0 bytes | — |
| SMSG_MOVE_SPLINE_ENABLE_COLLISION | 0x2e1e | 11806 | — | Movement | fixed, 0 bytes | — |
| SMSG_MOVE_SPLINE_SET_FEATHER_FALL | 0x2e1f | 11807 | 773 (0x305) | Movement | fixed, 0 bytes | — |
| SMSG_MOVE_SPLINE_SET_NORMAL_FALL | 0x2e20 | 11808 | 774 (0x306) | Movement | fixed, 0 bytes | — |
| SMSG_MOVE_SPLINE_SET_HOVER | 0x2e21 | 11809 | 775 (0x307) | Movement | fixed, 0 bytes | — |
| SMSG_MOVE_SPLINE_UNSET_HOVER | 0x2e22 | 11810 | 776 (0x308) | Movement | fixed, 0 bytes | — |
| SMSG_MOVE_SPLINE_SET_WATER_WALK | 0x2e23 | 11811 | 777 (0x309) | Movement | fixed, 0 bytes | — |
| SMSG_MOVE_SPLINE_SET_LAND_WALK | 0x2e24 | 11812 | 778 (0x30a) | Movement | fixed, 0 bytes | — |
| SMSG_MOVE_SPLINE_START_SWIM | 0x2e25 | 11813 | 779 (0x30b) | Movement | fixed, 0 bytes | — |
| SMSG_MOVE_SPLINE_STOP_SWIM | 0x2e26 | 11814 | 780 (0x30c) | Movement | fixed, 0 bytes | — |
| SMSG_MOVE_SPLINE_SET_RUN_MODE | 0x2e27 | 11815 | 781 (0x30d) | Movement | fixed, 0 bytes | — |
| SMSG_MOVE_SPLINE_SET_WALK_MODE | 0x2e28 | 11816 | 782 (0x30e) | Movement | fixed, 0 bytes | — |
| SMSG_MOVE_SPLINE_SET_FLYING | 0x2e29 | 11817 | — | Movement | fixed, 0 bytes | — |
| SMSG_MOVE_SPLINE_UNSET_FLYING | 0x2e2a | 11818 | — | Movement | fixed, 0 bytes | — |
| SMSG_FLIGHT_SPLINE_SYNC | 0x2e2b | 11819 | — | Movement | fixed, 4 bytes | — |
| — | 0x2e2c | 11820 | — | Movement | ignored by the client | — |
| — | 0x2e2d | 11821 | — | Movement | ignored by the client | — |
| SMSG_MOVE_APPLY_INERTIA | 0x2e2e | 11822 | — | Movement | fixed, 8 bytes | — |
| SMSG_MOVE_REMOVE_INERTIA | 0x2e2f | 11823 | — | Movement | fixed, 4 bytes | — |
| SMSG_MOVE_UPDATE_APPLY_INERTIA | 0x2e30 | 11824 | — | Movement | variable | — |
| SMSG_MOVE_UPDATE_REMOVE_INERTIA | 0x2e31 | 11825 | — | Movement | variable | — |
| SMSG_PLAYER_BOUND | 0x2ff8 | 12280 | 344 (0x158) | Player | fixed, 4 bytes | matches |
| — | 0x2ff9 | 12281 | — | Player | ignored by the client | — |
| SMSG_FAILED_PLAYER_CONDITION | 0x2ffa | 12282 | — | Player | fixed struct | — |
| SMSG_GM_REQUEST_PLAYER_INFO | 0x2ffb | 12283 | — | Player | variable | — |
| SMSG_DISPLAY_PLAYER_CHOICE | 0x2ffc | 12284 | — | Player | variable | — |
| SMSG_PLAYER_CHOICE_DISPLAY_ERROR | 0x2ffd | 12285 | — | Player | fixed struct | — |
| SMSG_PLAYER_CHOICE_CLEAR | 0x2ffe | 12286 | — | Player | fixed struct | — |
| SMSG_INVALIDATE_PLAYER | 0x2fff | 12287 | 796 (0x31c) | Player | fixed, 0 bytes | matches |
| — | 0x3000 | 12288 | — | Player | ignored by the client | — |
| SMSG_REPORT_PVP_PLAYER_AFK_RESULT | 0x3001 | 12289 | — | Player | fixed, 3 bytes | — |
| SMSG_QUERY_PLAYER_NAME_RESPONSE | 0x3002 | 12290 | 81 (0x51) | Player | variable | — |
| SMSG_QUERY_PLAYER_NAME_BY_COMMUNITY_ID_RESPONSE | 0x3003 | 12291 | — | Player | variable | — |
| SMSG_SET_PLAYER_DECLINED_NAMES_RESULT | 0x3004 | 12292 | — | Player | fixed, 4 bytes | matches |
| SMSG_CHANGE_PLAYER_DIFFICULTY_RESULT | 0x3005 | 12293 | — | Player | variable | — |
| SMSG_GM_PLAYER_INFO | 0x3006 | 12294 | 560 (0x230) | Player | variable | — |
| SMSG_PLAYER_SKINNED | 0x3007 | 12295 | 700 (0x2bc) | Player | fixed, 1 bytes | matches |
| SMSG_PLAYER_TABARD_VENDOR_ACTIVATE | 0x3008 | 12296 | — | Player | fixed, 0 bytes | matches |
| SMSG_VIGNETTE_UPDATE | 0x3009 | 12297 | — | Player | variable | — |
| SMSG_PLAYER_IS_ADVENTURE_MAP_POI_VALID | 0x300a | 12298 | — | Player | fixed, 5 bytes | — |
| SMSG_PLAYER_CONDITION_RESULT | 0x300b | 12299 | — | Player | fixed struct | — |
| — | 0x300c | 12300 | — | Player | ignored by the client | — |
| SMSG_PLAYER_TUTORIAL_UNHIGHLIGHT_SPELL | 0x300d | 12301 | — | Player | fixed struct | — |
| SMSG_PLAYER_TUTORIAL_HIGHLIGHT_SPELL | 0x300e | 12302 | — | Player | variable | — |
| SMSG_PLAYER_OPEN_SUBSCRIPTION_INTERSTITIAL | 0x300f | 12303 | — | Player | ignored by the client | — |
| SMSG_WORLD_QUEST_UPDATE_RESPONSE | 0x3010 | 12304 | — | Player | variable | — |
| SMSG_AREA_POI_UPDATE_RESPONSE | 0x3011 | 12305 | — | Player | variable | — |
| — | 0x3012 | 12306 | — | Player | ignored by the client | — |
| — | 0x3013 | 12307 | — | Player | ignored by the client | — |
| SMSG_PLAYER_AZERITE_ITEM_GAINS | 0x3014 | 12308 | — | Player | ignored by the client | — |
| SMSG_PLAYER_AZERITE_ITEM_EQUIPPED_STATUS_CHANGED | 0x3015 | 12309 | — | Player | ignored by the client | — |
| SMSG_ACTIVATE_ESSENCE_FAILED | 0x3016 | 12310 | — | Player | ignored by the client | — |
| SMSG_PLAYER_BONUS_ROLL_FAILED | 0x3017 | 12311 | — | Player | fixed struct | — |
| SMSG_ACTIVATE_SOULBIND_FAILED | 0x3018 | 12312 | — | Player | ignored by the client | — |
| SMSG_CHECK_ABANDON_NPE | 0x3019 | 12313 | — | Player | ignored by the client | — |
| SMSG_PLAYER_SHOW_UI_EVENT_TOAST | 0x301a | 12314 | — | Player | fixed struct | — |
| — | 0x301b | 12315 | — | Player | ignored by the client | — |
| SMSG_QUERY_PLAYER_NAMES_RESPONSE | 0x301c | 12316 | — | Player | variable | — |
| — | 0x303e | 12350 | — | Social | fixed struct | — |
| — | 0x303f | 12351 | — | Social | fixed struct | — |
| — | 0x3040 | 12352 | — | Social | variable | — |
| — | 0x3041 | 12353 | — | Social | variable | — |
| — | 0x3042 | 12354 | — | Social | variable | — |
| SMSG_TWITTER_STATUS | 0x3043 | 12355 | — | Social | variable | — |
