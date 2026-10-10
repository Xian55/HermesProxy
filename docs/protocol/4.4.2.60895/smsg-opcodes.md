# Server opcodes (SMSG), 4.4.2.60895

Every server-to-client opcode of the client, with its 4.3.4 counterpart where the same name exists in both. Layouts are in [smsg-structures](smsg-structures/README.md).

| Name | Hex | Dec | 4.3.4 | Area | Layout |
|---|---|---:|---|---|---|
| SMSG_AUTH_FAILED | 0x3b0000 | 3866624 | — | General | fixed struct |
| SMSG_AUTH_RESPONSE | 0x3b0001 | 3866625 | 23990 (0x5db6) | General | variable |
| SMSG_WAIT_QUEUE_UPDATE | 0x3b0002 | 3866626 | 22689 (0x58a1) | General | fixed, 13 bytes |
| SMSG_WAIT_QUEUE_FINISH | 0x3b0003 | 3866627 | 30135 (0x75b7) | General | fixed struct |
| SMSG_ALL_ACHIEVEMENT_DATA | 0x3b0004 | 3866628 | 22705 (0x58b1) | General | variable |
| SMSG_ALL_ACCOUNT_CRITERIA | 0x3b0005 | 3866629 | — | General | variable |
| SMSG_RESPOND_INSPECT_ACHIEVEMENTS | 0x3b0006 | 3866630 | 5552 (0x15b0) | General | variable |
| SMSG_SETUP_CURRENCY | 0x3b0007 | 3866631 | 5541 (0x15a5) | General | variable |
| SMSG_SET_CURRENCY | 0x3b0008 | 3866632 | — | General | variable |
| SMSG_RESET_WEEKLY_CURRENCY | 0x3b0009 | 3866633 | — | General | fixed struct |
| SMSG_MESSAGE_BOX | 0x3b000a | 3866634 | 12449 (0x30a1) | General | variable |
| SMSG_WARDEN3_DATA | 0x3b000b | 3866635 | — | General | variable |
| SMSG_PHASE_SHIFT_CHANGE | 0x3b000c | 3866636 | 28832 (0x70a0) | General | variable |
| SMSG_PRELOAD_CHILD_MAP | 0x3b000d | 3866637 | — | General | fixed struct |
| SMSG_UNLOAD_CHILD_MAP | 0x3b000e | 3866638 | — | General | fixed struct |
| SMSG_MOUNT_RESULT | 0x3b000f | 3866639 | 8741 (0x2225) | General | fixed struct |
| SMSG_DISMOUNT_RESULT | 0x3b0010 | 3866640 | 3365 (0xd25) | General | fixed struct |
| SMSG_BIND_POINT_UPDATE | 0x3b0011 | 3866641 | 1319 (0x527) | General | fixed, 20 bytes |
| SMSG_RESURRECT_REQUEST | 0x3b0012 | 3866642 | 10501 (0x2905) | General | variable |
| — | 0x3b0013 | 3866643 | — | General | ignored by the client |
| SMSG_INITIAL_SETUP | 0x3b0014 | 3866644 | — | General | fixed struct |
| SMSG_REFORGE_RESULT | 0x3b0015 | 3866645 | 22692 (0x58a4) | General | fixed, 1 bytes |
| SMSG_TRADE_UPDATED | 0x3b0016 | 3866646 | — | General | variable |
| SMSG_TRADE_STATUS | 0x3b0017 | 3866647 | 23715 (0x5ca3) | General | variable |
| SMSG_ENUM_CHARACTERS_RESULT | 0x3b0018 | 3866648 | 4272 (0x10b0) | General | variable |
| — | 0x3b0019 | 3866649 | — | General | ignored by the client |
| — | 0x3b001a | 3866650 | — | General | ignored by the client |
| — | 0x3b001b | 3866651 | — | General | ignored by the client |
| SMSG_GENERATE_RANDOM_CHARACTER_NAME_RESULT | 0x3b001c | 3866652 | 14513 (0x38b1) | General | variable |
| SMSG_ARCHAEOLOGY_SURVERY_CAST | 0x3b001d | 3866653 | — | General | fixed, 13 bytes |
| SMSG_PET_NEWLY_TAMED | 0x3b001e | 3866654 | — | General | fixed, 1 bytes |
| SMSG_PET_MODE | 0x3b001f | 3866655 | 8757 (0x2235) | General | fixed, 3 bytes |
| SMSG_DIFFERENT_INSTANCE_FROM_PARTY | 0x3b0020 | 3866656 | 5553 (0x15b1) | General | fixed struct |
| SMSG_ROLE_CHANGED_INFORM | 0x3b0021 | 3866657 | — | General | fixed, 3 bytes |
| SMSG_ROLE_POLL_INFORM | 0x3b0022 | 3866658 | — | General | fixed, 1 bytes |
| SMSG_TALENT_GROUP_ROLE_CHANGED | 0x3b0023 | 3866659 | — | General | fixed, 2 bytes |
| SMSG_SUMMON_RAID_MEMBER_VALIDATE_FAILED | 0x3b0024 | 3866660 | — | General | variable |
| SMSG_GROUP_ACTION_THROTTLED | 0x3b0025 | 3866661 | 25892 (0x6524) | General | fixed struct |
| SMSG_REQUEST_CEMETERY_LIST_RESPONSE | 0x3b0026 | 3866662 | 12455 (0x30a7) | General | variable |
| SMSG_SET_FORGE_MASTER | 0x3b0027 | 3866663 | — | General | fixed, 0 bytes |
| SMSG_CHECK_WARGAME_ENTRY | 0x3b0028 | 3866664 | — | General | fixed, 17 bytes |
| — | 0x3b0029 | 3866665 | — | General | ignored by the client |
| — | 0x3b002a | 3866666 | — | General | ignored by the client |
| SMSG_PET_STABLE_RESULT | 0x3b002b | 3866667 | — | General | fixed struct |
| SMSG_NEW_WORLD | 0x3b002c | 3866668 | 31153 (0x79b1) | General | fixed, 40 bytes |
| SMSG_PRELOAD_WORLD | 0x3b002d | 3866669 | — | General | fixed, 45 bytes |
| — | 0x3b002e | 3866670 | — | General | ignored by the client |
| SMSG_CANCEL_PRELOAD_WORLD | 0x3b002f | 3866671 | — | General | fixed struct |
| SMSG_LOGIN_VERIFY_WORLD | 0x3b0030 | 3866672 | 8197 (0x2005) | General | fixed, 24 bytes |
| SMSG_ABORT_NEW_WORLD | 0x3b0031 | 3866673 | — | General | fixed struct |
| SMSG_NOTIFY_MONEY | 0x3b0032 | 3866674 | — | General | fixed struct |
| SMSG_ITEM_PURCHASE_REFUND_RESULT | 0x3b0033 | 3866675 | 23985 (0x5db1) | General | variable |
| SMSG_SET_ITEM_PURCHASE_DATA | 0x3b0034 | 3866676 | — | General | fixed, 96 bytes |
| SMSG_ITEM_EXPIRE_PURCHASE_REFUND | 0x3b0035 | 3866677 | 7328 (0x1ca0) | General | fixed, 0 bytes |
| SMSG_DISPLAY_GAME_ERROR | 0x3b0036 | 3866678 | 12710 (0x31a6) | General | variable |
| SMSG_SET_MAX_WEEKLY_QUANTITY | 0x3b0037 | 3866679 | — | General | fixed struct |
| SMSG_PETITION_ALREADY_SIGNED | 0x3b0038 | 3866680 | 23971 (0x5da3) | General | fixed, 0 bytes |
| SMSG_RAID_MARKERS_CHANGED | 0x3b0039 | 3866681 | 4257 (0x10a1) | General | variable |
| — | 0x3b003a | 3866682 | — | General | ignored by the client |
| — | 0x3b003b | 3866683 | — | General | ignored by the client |
| — | 0x3b003c | 3866684 | — | General | ignored by the client |
| SMSG_STREAMING_MOVIES | 0x3b003d | 3866685 | 5559 (0x15b7) | General | variable |
| SMSG_START_TIMER | 0x3b003e | 3866686 | 22949 (0x59a5) | General | variable |
| — | 0x3b003f | 3866687 | — | General | ignored by the client |
| SMSG_DISENCHANT_CREDIT | 0x3b0040 | 3866688 | 21922 (0x55a2) | General | variable |
| SMSG_SUSPEND_TOKEN | 0x3b0041 | 3866689 | 5297 (0x14b1) | General | fixed, 5 bytes |
| SMSG_RESUME_TOKEN | 0x3b0042 | 3866690 | — | General | fixed, 5 bytes |
| SMSG_ADD_ITEM_PASSIVE | 0x3b0043 | 3866691 | — | General | fixed struct |
| SMSG_REMOVE_ITEM_PASSIVE | 0x3b0044 | 3866692 | — | General | fixed struct |
| SMSG_SEND_ITEM_PASSIVES | 0x3b0045 | 3866693 | — | General | variable |
| SMSG_WORLD_SERVER_INFO | 0x3b0046 | 3866694 | 12706 (0x31a2) | General | variable |
| SMSG_ACCOUNT_MOUNT_UPDATE | 0x3b0047 | 3866695 | — | General | variable |
| SMSG_ACCOUNT_MOUNT_REMOVED | 0x3b0048 | 3866696 | — | General | fixed struct |
| SMSG_ACCOUNT_TOY_UPDATE | 0x3b0049 | 3866697 | — | General | variable |
| — | 0x3b004a | 3866698 | — | General | ignored by the client |
| SMSG_FORCE_RANDOM_TRANSMOG_TOAST | 0x3b004b | 3866699 | — | General | fixed struct |
| SMSG_ACCOUNT_TRANSMOG_UPDATE | 0x3b004c | 3866700 | — | General | variable |
| SMSG_ACCOUNT_TRANSMOG_SET_FAVORITES_UPDATE | 0x3b004d | 3866701 | — | General | variable |
| — | 0x3b004e | 3866702 | — | General | ignored by the client |
| SMSG_RUNE_REGEN_DEBUG | 0x3b004f | 3866703 | 12723 (0x31b3) | General | variable |
| SMSG_ACCOUNT_HEIRLOOM_UPDATE | 0x3b0050 | 3866704 | — | General | ignored by the client |
| SMSG_VENDOR_INVENTORY | 0x3b0051 | 3866705 | 31920 (0x7cb0) | General | variable |
| — | 0x3b0052 | 3866706 | — | General | ignored by the client |
| SMSG_SET_PLAY_HOVER_ANIM | 0x3b0053 | 3866707 | 12454 (0x30a6) | General | fixed, 1 bytes |
| SMSG_CLEAR_BOSS_EMOTES | 0x3b0054 | 3866708 | 6563 (0x19a3) | General | fixed struct |
| SMSG_LOAD_CUF_PROFILES | 0x3b0055 | 3866709 | 20657 (0x50b1) | General | variable |
| SMSG_PARTY_INVITE | 0x3b0056 | 3866710 | — | General | variable |
| — | 0x3b0057 | 3866711 | — | General | ignored by the client |
| SMSG_FEATURE_SYSTEM_STATUS | 0x3b0058 | 3866712 | 15799 (0x3db7) | General | variable |
| SMSG_FEATURE_SYSTEM_STATUS_GLUE_SCREEN | 0x3b0059 | 3866713 | — | General | variable |
| SMSG_PVP_SEASON | 0x3b005a | 3866714 | — | General | fixed, 29 bytes |
| — | 0x3b005b | 3866715 | — | General | ignored by the client |
| SMSG_GAME_OBJECT_ACTIVATE_ANIM_KIT | 0x3b005c | 3866716 | 5283 (0x14a3) | General | fixed, 5 bytes |
| SMSG_GAME_OBJECT_CUSTOM_ANIM | 0x3b005d | 3866717 | 18742 (0x4936) | General | fixed, 5 bytes |
| SMSG_GAME_OBJECT_DESPAWN | 0x3b005e | 3866718 | — | General | fixed, 0 bytes |
| SMSG_MAP_OBJ_EVENTS | 0x3b005f | 3866719 | 21682 (0x54b2) | General | variable |
| SMSG_MISSILE_CANCEL | 0x3b0060 | 3866720 | 15796 (0x3db4) | General | fixed, 5 bytes |
| — | 0x3b0061 | 3866721 | — | General | ignored by the client |
| SMSG_XP_GAIN_ABORTED | 0x3b0062 | 3866722 | 20660 (0x50b4) | General | fixed, 12 bytes |
| SMSG_PRINT_NOTIFICATION | 0x3b0063 | 3866723 | — | General | variable |
| SMSG_CUSTOM_LOAD_SCREEN | 0x3b0064 | 3866724 | 7606 (0x1db6) | General | fixed struct |
| SMSG_SPELL_VISUAL_LOAD_SCREEN | 0x3b0065 | 3866725 | — | General | fixed struct |
| SMSG_TRANSFER_PENDING | 0x3b0066 | 3866726 | 6310 (0x18a6) | General | variable |
| — | 0x3b0067 | 3866727 | — | General | ignored by the client |
| — | 0x3b0068 | 3866728 | — | General | ignored by the client |
| SMSG_ADJUST_SPLINE_DURATION | 0x3b0069 | 3866729 | — | General | fixed, 4 bytes |
| SMSG_TRAIT_CONFIG_COMMIT_FAILED | 0x3b006a | 3866730 | — | General | fixed, 9 bytes |
| — | 0x3b006b | 3866731 | — | General | ignored by the client |
| SMSG_LEARN_TALENT_FAILED | 0x3b006c | 3866732 | — | General | variable |
| SMSG_LEARN_PVP_TALENT_FAILED | 0x3b006d | 3866733 | — | General | variable |
| — | 0x3b006e | 3866734 | — | General | ignored by the client |
| — | 0x3b006f | 3866735 | — | General | variable |
| — | 0x3b0070 | 3866736 | — | General | variable |
| SMSG_UPDATE_PRIMARY_SPEC | 0x3b0071 | 3866737 | — | General | fixed struct |
| — | 0x3b0072 | 3866738 | — | General | ignored by the client |
| — | 0x3b0073 | 3866739 | — | General | ignored by the client |
| SMSG_SHOW_NEUTRAL_PLAYER_FACTION_SELECT_UI | 0x3b0074 | 3866740 | — | General | fixed struct |
| SMSG_NEUTRAL_PLAYER_FACTION_SELECT_RESULT | 0x3b0075 | 3866741 | — | General | fixed, 5 bytes |
| SMSG_SOR_START_EXPERIENCE_INCOMPLETE | 0x3b0076 | 3866742 | 31911 (0x7ca7) | General | fixed struct |
| — | 0x3b0077 | 3866743 | — | General | ignored by the client |
| SMSG_SET_CHR_UPGRADE_TIER | 0x3b0078 | 3866744 | — | General | fixed struct |
| SMSG_UPDATE_ACTION_BUTTONS | 0x3b0079 | 3866745 | 14517 (0x38b5) | General | fixed struct |
| SMSG_DONT_AUTO_PUSH_SPELLS_TO_ACTION_BAR | 0x3b007a | 3866746 | 14498 (0x38a2) | General | fixed struct |
| SMSG_SCENE_OBJECT_EVENT | 0x3b007b | 3866747 | — | General | variable |
| SMSG_SCENE_OBJECT_PET_BATTLE_INITIAL_UPDATE | 0x3b007c | 3866748 | — | General | variable |
| SMSG_SCENE_OBJECT_PET_BATTLE_FIRST_ROUND | 0x3b007d | 3866749 | — | General | variable |
| SMSG_SCENE_OBJECT_PET_BATTLE_ROUND_RESULT | 0x3b007e | 3866750 | — | General | variable |
| SMSG_SCENE_OBJECT_PET_BATTLE_REPLACEMENTS_MADE | 0x3b007f | 3866751 | — | General | variable |
| SMSG_SCENE_OBJECT_PET_BATTLE_FINAL_ROUND | 0x3b0080 | 3866752 | — | General | variable |
| SMSG_SCENE_OBJECT_PET_BATTLE_FINISHED | 0x3b0081 | 3866753 | — | General | fixed, 0 bytes |
| — | 0x3b0082 | 3866754 | — | General | ignored by the client |
| SMSG_BATTLE_PET_UPDATES | 0x3b0083 | 3866755 | — | General | variable |
| SMSG_BATTLE_PET_TRAP_LEVEL | 0x3b0084 | 3866756 | — | General | fixed struct |
| SMSG_PET_BATTLE_SLOT_UPDATES | 0x3b0085 | 3866757 | — | General | variable |
| SMSG_BATTLE_PET_JOURNAL_LOCK_ACQUIRED | 0x3b0086 | 3866758 | — | General | fixed struct |
| SMSG_BATTLE_PET_JOURNAL_LOCK_DENIED | 0x3b0087 | 3866759 | — | General | fixed struct |
| SMSG_BATTLE_PET_JOURNAL | 0x3b0088 | 3866760 | — | General | variable |
| SMSG_BATTLE_PET_DELETED | 0x3b0089 | 3866761 | — | General | fixed, 0 bytes |
| SMSG_BATTLE_PET_REVOKED | 0x3b008a | 3866762 | — | General | fixed, 0 bytes |
| SMSG_BATTLE_PET_RESTORED | 0x3b008b | 3866763 | — | General | fixed, 0 bytes |
| SMSG_BATTLE_PETS_HEALED | 0x3b008c | 3866764 | — | General | fixed struct |
| SMSG_PARTY_UPDATE | 0x3b008d | 3866765 | — | General | variable |
| — | 0x3b008e | 3866766 | — | General | ignored by the client |
| SMSG_READY_CHECK_STARTED | 0x3b008f | 3866767 | — | General | fixed, 9 bytes |
| SMSG_READY_CHECK_RESPONSE | 0x3b0090 | 3866768 | — | General | fixed, 1 bytes |
| SMSG_READY_CHECK_COMPLETED | 0x3b0091 | 3866769 | — | General | fixed, 1 bytes |
| — | 0x3b0092 | 3866770 | — | General | ignored by the client |
| — | 0x3b0093 | 3866771 | — | General | ignored by the client |
| — | 0x3b0094 | 3866772 | — | General | ignored by the client |
| — | 0x3b0095 | 3866773 | — | General | ignored by the client |
| — | 0x3b0096 | 3866774 | — | General | ignored by the client |
| — | 0x3b0097 | 3866775 | — | General | ignored by the client |
| — | 0x3b0098 | 3866776 | — | General | ignored by the client |
| — | 0x3b0099 | 3866777 | — | General | ignored by the client |
| — | 0x3b009a | 3866778 | — | General | ignored by the client |
| — | 0x3b009b | 3866779 | — | General | ignored by the client |
| — | 0x3b009c | 3866780 | — | General | ignored by the client |
| SMSG_START_ELAPSED_TIMER | 0x3b009d | 3866781 | — | General | fixed, 12 bytes |
| SMSG_STOP_ELAPSED_TIMER | 0x3b009e | 3866782 | — | General | fixed, 5 bytes |
| SMSG_START_ELAPSED_TIMERS | 0x3b009f | 3866783 | — | General | variable |
| — | 0x3b00a0 | 3866784 | — | General | ignored by the client |
| — | 0x3b00a1 | 3866785 | — | General | ignored by the client |
| — | 0x3b00a2 | 3866786 | — | General | ignored by the client |
| — | 0x3b00a3 | 3866787 | — | General | ignored by the client |
| — | 0x3b00a4 | 3866788 | — | General | ignored by the client |
| — | 0x3b00a5 | 3866789 | — | General | ignored by the client |
| — | 0x3b00a6 | 3866790 | — | General | ignored by the client |
| — | 0x3b00a7 | 3866791 | — | General | ignored by the client |
| — | 0x3b00a8 | 3866792 | — | General | ignored by the client |
| — | 0x3b00a9 | 3866793 | — | General | ignored by the client |
| — | 0x3b00aa | 3866794 | — | General | ignored by the client |
| SMSG_RESPEC_WIPE_CONFIRM | 0x3b00ab | 3866795 | — | General | fixed, 5 bytes |
| — | 0x3b00ac | 3866796 | — | General | ignored by the client |
| SMSG_LOOT_RESPONSE | 0x3b00ad | 3866797 | 19478 (0x4c16) | General | variable |
| SMSG_LOOT_REMOVED | 0x3b00ae | 3866798 | 26647 (0x6817) | General | fixed, 1 bytes |
| — | 0x3b00af | 3866799 | — | General | ignored by the client |
| SMSG_COIN_REMOVED | 0x3b00b0 | 3866800 | — | General | fixed, 0 bytes |
| SMSG_AE_LOOT_TARGETS | 0x3b00b1 | 3866801 | — | General | fixed struct |
| SMSG_AE_LOOT_TARGET_ACK | 0x3b00b2 | 3866802 | — | General | fixed struct |
| SMSG_LOOT_RELEASE_ALL | 0x3b00b3 | 3866803 | — | General | fixed struct |
| SMSG_LOOT_RELEASE | 0x3b00b4 | 3866804 | 27941 (0x6d25) | General | fixed, 0 bytes |
| SMSG_LOOT_MONEY_NOTIFY | 0x3b00b5 | 3866805 | 10294 (0x2836) | General | fixed, 17 bytes |
| SMSG_START_LOOT_ROLL | 0x3b00b6 | 3866806 | — | General | variable |
| SMSG_LOOT_ROLL | 0x3b00b7 | 3866807 | 25863 (0x6507) | General | variable |
| SMSG_MASTER_LOOT_CANDIDATE_LIST | 0x3b00b8 | 3866808 | — | General | variable |
| SMSG_LOOT_ROLLS_COMPLETE | 0x3b00b9 | 3866809 | — | General | fixed, 5 bytes |
| SMSG_LOOT_ALL_PASSED | 0x3b00ba | 3866810 | 25143 (0x6237) | General | variable |
| SMSG_LOOT_ROLL_WON | 0x3b00bb | 3866811 | 26135 (0x6617) | General | variable |
| SMSG_ITEM_PUSH_RESULT | 0x3b00bc | 3866812 | 3605 (0xe15) | General | variable |
| SMSG_DISPLAY_TOAST | 0x3b00bd | 3866813 | — | General | variable |
| SMSG_SET_PET_SPECIALIZATION | 0x3b00be | 3866814 | — | General | fixed struct |
| — | 0x3b00bf | 3866815 | — | General | ignored by the client |
| SMSG_BLACK_MARKET_REQUEST_ITEMS_RESULT | 0x3b00c0 | 3866816 | — | General | variable |
| SMSG_BLACK_MARKET_BID_ON_ITEM_RESULT | 0x3b00c1 | 3866817 | — | General | variable |
| SMSG_BLACK_MARKET_OUTBID | 0x3b00c2 | 3866818 | — | General | variable |
| SMSG_BLACK_MARKET_WON | 0x3b00c3 | 3866819 | — | General | variable |
| SMSG_SCENARIO_STATE | 0x3b00c4 | 3866820 | — | General | variable |
| SMSG_SCENARIO_PROGRESS_UPDATE | 0x3b00c5 | 3866821 | — | General | variable |
| SMSG_GROUP_NEW_LEADER | 0x3b00c6 | 3866822 | — | General | variable |
| SMSG_SEND_RAID_TARGET_UPDATE_ALL | 0x3b00c7 | 3866823 | — | General | variable |
| SMSG_SEND_RAID_TARGET_UPDATE_SINGLE | 0x3b00c8 | 3866824 | — | General | fixed, 2 bytes |
| SMSG_RANDOM_ROLL | 0x3b00c9 | 3866825 | — | General | fixed, 12 bytes |
| SMSG_INSPECT_RESULT | 0x3b00ca | 3866826 | — | General | variable |
| SMSG_ARENA_CROWD_CONTROL_SPELL_RESULT | 0x3b00cb | 3866827 | — | General | fixed, 8 bytes |
| SMSG_SCENARIO_POIS | 0x3b00cc | 3866828 | — | General | variable |
| SMSG_INSTANCE_INFO | 0x3b00cd | 3866829 | — | General | variable |
| SMSG_CONSOLE_WRITE | 0x3b00ce | 3866830 | — | General | variable |
| SMSG_PLAY_SCENE | 0x3b00cf | 3866831 | — | General | fixed, 37 bytes |
| SMSG_CANCEL_SCENE | 0x3b00d0 | 3866832 | — | General | fixed struct |
| SMSG_BATTLE_PET_ERROR | 0x3b00d1 | 3866833 | — | General | fixed, 5 bytes |
| — | 0x3b00d2 | 3866834 | — | General | ignored by the client |
| — | 0x3b00d3 | 3866835 | — | General | ignored by the client |
| SMSG_MAIL_COMMAND_RESULT | 0x3b00d4 | 3866836 | 18727 (0x4927) | General | fixed, 32 bytes |
| SMSG_NOTIFY_RECEIVED_MAIL | 0x3b00d5 | 3866837 | — | General | fixed struct |
| SMSG_ADD_BATTLENET_FRIEND_RESPONSE | 0x3b00d6 | 3866838 | — | General | variable |
| — | 0x3b00d7 | 3866839 | — | General | ignored by the client |
| — | 0x3b00d8 | 3866840 | — | General | ignored by the client |
| — | 0x3b00d9 | 3866841 | — | General | ignored by the client |
| — | 0x3b00da | 3866842 | — | General | ignored by the client |
| SMSG_ADDON_LIST_REQUEST | 0x3b00db | 3866843 | — | General | fixed, 7 bytes |
| SMSG_ACHIEVEMENT_EARNED | 0x3b00dc | 3866844 | 17413 (0x4405) | General | fixed, 17 bytes |
| — | 0x3b00dd | 3866845 | — | General | ignored by the client |
| SMSG_BONUS_ROLL_EMPTY | 0x3b00de | 3866846 | — | General | fixed struct |
| SMSG_UPDATE_EXPANSION_LEVEL | 0x3b00df | 3866847 | — | General | variable |
| SMSG_CONTROL_UPDATE | 0x3b00e0 | 3866848 | 10295 (0x2837) | General | fixed, 1 bytes |
| SMSG_ARENA_PREP_OPPONENT_SPECIALIZATIONS | 0x3b00e1 | 3866849 | — | General | variable |
| — | 0x3b00e2 | 3866850 | — | General | ignored by the client |
| — | 0x3b00e3 | 3866851 | — | General | ignored by the client |
| SMSG_FORCE_OBJECT_RELINK | 0x3b00e4 | 3866852 | — | General | fixed, 0 bytes |
| SMSG_DISPLAY_PROMOTION | 0x3b00e5 | 3866853 | — | General | fixed struct |
| — | 0x3b00e6 | 3866854 | — | General | ignored by the client |
| SMSG_SERVER_FIRST_ACHIEVEMENTS | 0x3b00e7 | 3866855 | — | General | variable |
| SMSG_CORPSE_LOCATION | 0x3b00e8 | 3866856 | — | General | fixed, 21 bytes |
| — | 0x3b00e9 | 3866857 | — | General | ignored by the client |
| SMSG_REFRESH_COMPONENT | 0x3b00ea | 3866858 | — | General | fixed struct |
| — | 0x3b00eb | 3866859 | — | General | ignored by the client |
| — | 0x3b00ec | 3866860 | — | General | ignored by the client |
| — | 0x3b00ed | 3866861 | — | General | ignored by the client |
| — | 0x3b00ee | 3866862 | — | General | ignored by the client |
| — | 0x3b00ef | 3866863 | — | General | ignored by the client |
| SMSG_DEBUG_MENU_MANAGER_FULL_UPDATE | 0x3b00f0 | 3866864 | — | General | fixed struct |
| — | 0x3b00f1 | 3866865 | — | General | ignored by the client |
| — | 0x3b00f2 | 3866866 | — | General | ignored by the client |
| — | 0x3b00f3 | 3866867 | — | General | ignored by the client |
| — | 0x3b00f4 | 3866868 | — | General | ignored by the client |
| — | 0x3b00f5 | 3866869 | — | General | ignored by the client |
| — | 0x3b00f6 | 3866870 | — | General | ignored by the client |
| — | 0x3b00f7 | 3866871 | — | General | ignored by the client |
| — | 0x3b00f8 | 3866872 | — | General | ignored by the client |
| — | 0x3b00f9 | 3866873 | — | General | ignored by the client |
| — | 0x3b00fa | 3866874 | — | General | ignored by the client |
| — | 0x3b00fb | 3866875 | — | General | ignored by the client |
| — | 0x3b00fc | 3866876 | — | General | ignored by the client |
| — | 0x3b00fd | 3866877 | — | General | ignored by the client |
| — | 0x3b00fe | 3866878 | — | General | ignored by the client |
| — | 0x3b00ff | 3866879 | — | General | ignored by the client |
| — | 0x3b0100 | 3866880 | — | General | ignored by the client |
| — | 0x3b0101 | 3866881 | — | General | ignored by the client |
| — | 0x3b0102 | 3866882 | — | General | ignored by the client |
| — | 0x3b0103 | 3866883 | — | General | ignored by the client |
| — | 0x3b0104 | 3866884 | — | General | ignored by the client |
| — | 0x3b0105 | 3866885 | — | General | ignored by the client |
| — | 0x3b0106 | 3866886 | — | General | ignored by the client |
| — | 0x3b0107 | 3866887 | — | General | ignored by the client |
| — | 0x3b0108 | 3866888 | — | General | ignored by the client |
| — | 0x3b0109 | 3866889 | — | General | ignored by the client |
| — | 0x3b010a | 3866890 | — | General | ignored by the client |
| SMSG_LOSS_OF_CONTROL_AURA_UPDATE | 0x3b010b | 3866891 | — | General | variable |
| SMSG_ADD_LOSS_OF_CONTROL | 0x3b010c | 3866892 | — | General | fixed, 18 bytes |
| — | 0x3b010d | 3866893 | — | General | ignored by the client |
| — | 0x3b010e | 3866894 | — | General | ignored by the client |
| — | 0x3b010f | 3866895 | — | General | ignored by the client |
| — | 0x3b0110 | 3866896 | — | General | ignored by the client |
| — | 0x3b0111 | 3866897 | — | General | ignored by the client |
| — | 0x3b0112 | 3866898 | — | General | ignored by the client |
| SMSG_SET_TIME_ZONE_INFORMATION | 0x3b0113 | 3866899 | — | General | variable |
| SMSG_BATTLE_PET_CAGE_DATE_ERROR | 0x3b0114 | 3866900 | — | General | fixed struct |
| — | 0x3b0115 | 3866901 | — | General | ignored by the client |
| SMSG_TEXT_EMOTE | 0x3b0116 | 3866902 | 2821 (0xb05) | General | fixed, 8 bytes |
| SMSG_PET_GOD_MODE | 0x3b0117 | 3866903 | — | General | fixed, 1 bytes |
| SMSG_TAXI_NODE_STATUS | 0x3b0118 | 3866904 | 10550 (0x2936) | General | fixed, 1 bytes |
| SMSG_ACTIVATE_TAXI_REPLY | 0x3b0119 | 3866905 | 27191 (0x6a37) | General | fixed, 1 bytes |
| SMSG_NEW_TAXI_PATH | 0x3b011a | 3866906 | 19253 (0x4b35) | General | fixed struct |
| — | 0x3b011b | 3866907 | — | General | ignored by the client |
| — | 0x3b011c | 3866908 | — | General | ignored by the client |
| SMSG_GAME_SPEED_SET | 0x3b011d | 3866909 | 20020 (0x4e34) | General | fixed struct |
| SMSG_SERVER_TIME | 0x3b011e | 3866910 | — | General | fixed struct |
| SMSG_LOGOUT_RESPONSE | 0x3b011f | 3866911 | 1316 (0x524) | General | fixed, 5 bytes |
| SMSG_LOGOUT_COMPLETE | 0x3b0120 | 3866912 | 8503 (0x2137) | General | variable |
| SMSG_LOGOUT_CANCEL_ACK | 0x3b0121 | 3866913 | 25876 (0x6514) | General | fixed struct |
| SMSG_INSTANCE_RESET | 0x3b0122 | 3866914 | 28421 (0x6f05) | General | fixed struct |
| SMSG_INSTANCE_RESET_FAILED | 0x3b0123 | 3866915 | 18213 (0x4725) | General | fixed, 5 bytes |
| SMSG_UPDATE_LAST_INSTANCE | 0x3b0124 | 3866916 | 1079 (0x437) | General | fixed struct |
| SMSG_KICK_REASON | 0x3b0125 | 3866917 | 16423 (0x4027) | General | fixed struct |
| — | 0x3b0126 | 3866918 | — | General | ignored by the client |
| SMSG_CALENDAR_SEND_CALENDAR | 0x3b0127 | 3866919 | 26629 (0x6805) | General | variable |
| SMSG_CALENDAR_SEND_EVENT | 0x3b0128 | 3866920 | 3125 (0xc35) | General | variable |
| SMSG_CALENDAR_COMMUNITY_INVITE | 0x3b0129 | 3866921 | — | General | variable |
| SMSG_CALENDAR_INVITE_ADDED | 0x3b012a | 3866922 | — | General | fixed, 24 bytes |
| SMSG_CALENDAR_INVITE_REMOVED | 0x3b012b | 3866923 | — | General | fixed, 13 bytes |
| SMSG_CALENDAR_INVITE_STATUS | 0x3b012c | 3866924 | — | General | fixed, 22 bytes |
| SMSG_CALENDAR_MODERATOR_STATUS | 0x3b012d | 3866925 | — | General | fixed, 10 bytes |
| SMSG_CALENDAR_INVITE_ALERT | 0x3b012e | 3866926 | — | General | variable |
| SMSG_CALENDAR_INVITE_STATUS_ALERT | 0x3b012f | 3866927 | — | General | fixed, 17 bytes |
| SMSG_CALENDAR_INVITE_REMOVED_ALERT | 0x3b0130 | 3866928 | — | General | fixed, 17 bytes |
| SMSG_CALENDAR_EVENT_REMOVED_ALERT | 0x3b0131 | 3866929 | 27957 (0x6d35) | General | fixed, 13 bytes |
| SMSG_CALENDAR_EVENT_UPDATED_ALERT | 0x3b0132 | 3866930 | 2311 (0x907) | General | variable |
| SMSG_CALENDAR_INVITE_NOTES | 0x3b0133 | 3866931 | — | General | variable |
| SMSG_CALENDAR_INVITE_NOTES_ALERT | 0x3b0134 | 3866932 | — | General | variable |
| SMSG_CALENDAR_RAID_LOCKOUT_ADDED | 0x3b0135 | 3866933 | 8965 (0x2305) | General | fixed struct |
| SMSG_CALENDAR_RAID_LOCKOUT_REMOVED | 0x3b0136 | 3866934 | 11813 (0x2e25) | General | fixed struct |
| SMSG_CALENDAR_RAID_LOCKOUT_UPDATED | 0x3b0137 | 3866935 | 17974 (0x4636) | General | fixed struct |
| SMSG_CALENDAR_SEND_NUM_PENDING | 0x3b0138 | 3866936 | 3095 (0xc17) | General | fixed struct |
| SMSG_CALENDAR_CLEAR_PENDING_ACTION | 0x3b0139 | 3866937 | 8454 (0x2106) | General | fixed struct |
| SMSG_CALENDAR_COMMAND_RESULT | 0x3b013a | 3866938 | 28470 (0x6f36) | General | variable |
| SMSG_SPECIAL_MOUNT_ANIM | 0x3b013b | 3866939 | — | General | variable |
| SMSG_PET_ACTION_SOUND | 0x3b013c | 3866940 | 17188 (0x4324) | General | fixed, 4 bytes |
| SMSG_PET_DISMISS_SOUND | 0x3b013d | 3866941 | 11013 (0x2b05) | General | fixed, 16 bytes |
| SMSG_GM_TICKET_SYSTEM_STATUS | 0x3b013e | 3866942 | — | General | fixed struct |
| SMSG_GM_TICKET_CASE_STATUS | 0x3b013f | 3866943 | — | General | variable |
| SMSG_SET_DUNGEON_DIFFICULTY | 0x3b0140 | 3866944 | — | General | fixed struct |
| SMSG_WHO_IS | 0x3b0141 | 3866945 | 26903 (0x6917) | General | variable |
| SMSG_WEATHER | 0x3b0142 | 3866946 | 10500 (0x2904) | General | fixed, 9 bytes |
| SMSG_START_LIGHTNING_STORM | 0x3b0143 | 3866947 | — | General | fixed struct |
| SMSG_END_LIGHTNING_STORM | 0x3b0144 | 3866948 | — | General | fixed struct |
| SMSG_UPDATE_INSTANCE_OWNERSHIP | 0x3b0145 | 3866949 | 18709 (0x4915) | General | fixed struct |
| SMSG_NOTIFY_MISSILE_TRAJECTORY_COLLISION | 0x3b0146 | 3866950 | — | General | fixed, 12 bytes |
| SMSG_COMPLAINT_RESULT | 0x3b0147 | 3866951 | 27940 (0x6d24) | General | fixed struct |
| — | 0x3b0148 | 3866952 | — | General | ignored by the client |
| — | 0x3b0149 | 3866953 | — | General | ignored by the client |
| — | 0x3b014a | 3866954 | — | General | ignored by the client |
| — | 0x3b014b | 3866955 | — | General | ignored by the client |
| SMSG_SUMMON_CANCEL | 0x3b014c | 3866956 | 2868 (0xb34) | General | fixed struct |
| SMSG_DISMOUNT | 0x3b014d | 3866957 | 8501 (0x2135) | General | fixed, 0 bytes |
| SMSG_EQUIPMENT_SET_ID | 0x3b014e | 3866958 | 8726 (0x2216) | General | fixed, 16 bytes |
| SMSG_PET_TAME_FAILURE | 0x3b014f | 3866959 | 27428 (0x6b24) | General | fixed struct |
| — | 0x3b0150 | 3866960 | — | General | ignored by the client |
| SMSG_AI_REACTION | 0x3b0151 | 3866961 | 1591 (0x637) | General | fixed, 4 bytes |
| SMSG_OFFER_PETITION_ERROR | 0x3b0152 | 3866962 | 10006 (0x2716) | General | fixed, 0 bytes |
| SMSG_RESET_FAILED_NOTIFY | 0x3b0153 | 3866963 | 17942 (0x4616) | General | fixed struct |
| SMSG_ADD_RUNE_POWER | 0x3b0154 | 3866964 | 26901 (0x6915) | General | fixed struct |
| SMSG_COOLDOWN_EVENT | 0x3b0155 | 3866965 | 20262 (0x4f26) | General | fixed, 5 bytes |
| SMSG_CLEAR_COOLDOWN | 0x3b0156 | 3866966 | 1575 (0x627) | General | fixed, 5 bytes |
| SMSG_OVERRIDE_LIGHT | 0x3b0157 | 3866967 | 16933 (0x4225) | General | fixed struct |
| SMSG_ENABLE_BARBER_SHOP | 0x3b0158 | 3866968 | 11542 (0x2d16) | General | fixed struct |
| SMSG_CONFIRM_BARBERS_CHOICE | 0x3b0159 | 3866969 | — | General | fixed struct |
| SMSG_BARBER_SHOP_RESULT | 0x3b015a | 3866970 | 24869 (0x6125) | General | fixed, 5 bytes |
| SMSG_PETITION_SHOW_LIST | 0x3b015b | 3866971 | 25605 (0x6405) | General | variable |
| SMSG_PETITION_SHOW_SIGNATURES | 0x3b015c | 3866972 | 1814 (0x716) | General | variable |
| SMSG_RECRUIT_A_FRIEND_FAILURE | 0x3b015d | 3866973 | — | General | variable |
| SMSG_CROSSED_INEBRIATION_THRESHOLD | 0x3b015e | 3866974 | 8246 (0x2036) | General | fixed, 8 bytes |
| — | 0x3b015f | 3866975 | — | General | ignored by the client |
| SMSG_PET_NAME_INVALID | 0x3b0160 | 3866976 | 24583 (0x6007) | General | variable |
| SMSG_SELL_RESPONSE | 0x3b0161 | 3866977 | — | General | variable |
| SMSG_BUY_SUCCEEDED | 0x3b0162 | 3866978 | 3878 (0xf26) | General | fixed, 12 bytes |
| SMSG_BUY_FAILED | 0x3b0163 | 3866979 | 25653 (0x6435) | General | fixed, 8 bytes |
| SMSG_TOTEM_CREATED | 0x3b0164 | 3866980 | 9236 (0x2414) | General | fixed, 14 bytes |
| — | 0x3b0165 | 3866981 | — | General | ignored by the client |
| SMSG_TOTEM_MOVED | 0x3b0166 | 3866982 | — | General | fixed, 2 bytes |
| SMSG_TRIGGER_MOVIE | 0x3b0167 | 3866983 | 17957 (0x4625) | General | fixed struct |
| — | 0x3b0168 | 3866984 | — | General | ignored by the client |
| SMSG_SHOW_TAXI_NODES | 0x3b0169 | 3866985 | 10806 (0x2a36) | General | variable |
| SMSG_MINIMAP_PING | 0x3b016a | 3866986 | — | General | fixed, 8 bytes |
| SMSG_FISH_NOT_HOOKED | 0x3b016b | 3866987 | 2583 (0xa17) | General | fixed struct |
| SMSG_FISH_ESCAPED | 0x3b016c | 3866988 | 8709 (0x2205) | General | fixed struct |
| SMSG_HEALTH_UPDATE | 0x3b016d | 3866989 | 18228 (0x4734) | General | fixed, 8 bytes |
| SMSG_POWER_UPDATE | 0x3b016e | 3866990 | 18951 (0x4a07) | General | variable |
| SMSG_DEATH_RELEASE_LOC | 0x3b016f | 3866991 | 12039 (0x2f07) | General | fixed, 16 bytes |
| SMSG_FORCED_DEATH_UPDATE | 0x3b0170 | 3866992 | 9734 (0x2606) | General | fixed struct |
| SMSG_PLAYED_TIME | 0x3b0171 | 3866993 | 24631 (0x6037) | General | fixed, 9 bytes |
| — | 0x3b0172 | 3866994 | — | General | ignored by the client |
| SMSG_TITLE_EARNED | 0x3b0173 | 3866995 | 9254 (0x2426) | General | fixed struct |
| SMSG_TITLE_LOST | 0x3b0174 | 3866996 | — | General | fixed struct |
| SMSG_HIGHEST_THREAT_UPDATE | 0x3b0175 | 3866997 | 16644 (0x4104) | General | variable |
| SMSG_THREAT_UPDATE | 0x3b0176 | 3866998 | 18229 (0x4735) | General | variable |
| SMSG_THREAT_REMOVE | 0x3b0177 | 3866999 | 11781 (0x2e05) | General | fixed, 0 bytes |
| SMSG_THREAT_CLEAR | 0x3b0178 | 3867000 | 25655 (0x6437) | General | fixed, 0 bytes |
| SMSG_PROPOSE_LEVEL_GRANT | 0x3b0179 | 3867001 | 24852 (0x6114) | General | fixed, 0 bytes |
| SMSG_CANCEL_AUTO_REPEAT | 0x3b017a | 3867002 | 25654 (0x6436) | General | fixed, 0 bytes |
| SMSG_TRAINER_LIST | 0x3b017b | 3867003 | 17428 (0x4414) | General | variable |
| SMSG_TRAINER_BUY_FAILED | 0x3b017c | 3867004 | 4 (0x4) | General | fixed, 8 bytes |
| SMSG_CRITERIA_UPDATE | 0x3b017d | 3867005 | 28215 (0x6e37) | General | variable |
| SMSG_CHAR_CUSTOMIZE_FAILURE | 0x3b017e | 3867006 | — | General | fixed, 1 bytes |
| SMSG_CHAR_CUSTOMIZE_SUCCESS | 0x3b017f | 3867007 | — | General | variable |
| SMSG_QUERY_TIME_RESPONSE | 0x3b0180 | 3867008 | 8484 (0x2124) | General | fixed struct |
| SMSG_LOG_XP_GAIN | 0x3b0181 | 3867009 | 17684 (0x4514) | General | fixed, 13 bytes |
| SMSG_ON_CANCEL_EXPECTED_RIDE_VEHICLE_AURA | 0x3b0182 | 3867010 | 19764 (0x4d34) | General | fixed struct |
| SMSG_CRITERIA_DELETED | 0x3b0183 | 3867011 | 10517 (0x2915) | General | fixed struct |
| SMSG_ACHIEVEMENT_DELETED | 0x3b0184 | 3867012 | 27158 (0x6a16) | General | fixed struct |
| SMSG_LEVEL_UP_INFO | 0x3b0185 | 3867013 | 1077 (0x435) | General | fixed struct |
| — | 0x3b0186 | 3867014 | — | General | ignored by the client |
| SMSG_ITEM_CHANGED | 0x3b0187 | 3867015 | — | General | variable |
| — | 0x3b0188 | 3867016 | — | General | ignored by the client |
| — | 0x3b0189 | 3867017 | — | General | ignored by the client |
| SMSG_AUCTION_HELLO_RESPONSE | 0x3b018a | 3867018 | — | General | fixed, 13 bytes |
| SMSG_AUCTION_REPLICATE_RESPONSE | 0x3b018b | 3867019 | — | General | variable |
| SMSG_AUCTION_COMMAND_RESULT | 0x3b018c | 3867020 | 19493 (0x4c25) | General | fixed, 36 bytes |
| SMSG_AUCTION_WON_NOTIFICATION | 0x3b018d | 3867021 | — | General | variable |
| SMSG_AUCTION_OUTBID_NOTIFICATION | 0x3b018e | 3867022 | — | General | variable |
| SMSG_AUCTION_CLOSED_NOTIFICATION | 0x3b018f | 3867023 | — | General | variable |
| SMSG_AUCTION_OWNER_BID_NOTIFICATION | 0x3b0190 | 3867024 | — | General | variable |
| — | 0x3b0191 | 3867025 | — | General | ignored by the client |
| — | 0x3b0192 | 3867026 | — | General | ignored by the client |
| SMSG_SET_VEHICLE_REC_ID | 0x3b0193 | 3867027 | — | General | fixed, 4 bytes |
| SMSG_PENDING_RAID_LOCK | 0x3b0194 | 3867028 | — | General | fixed, 9 bytes |
| SMSG_DESTRUCTIBLE_BUILDING_DAMAGE | 0x3b0195 | 3867029 | 18469 (0x4825) | General | fixed, 8 bytes |
| SMSG_INSTANCE_GROUP_SIZE_CHANGED | 0x3b0196 | 3867030 | — | General | fixed struct |
| — | 0x3b0197 | 3867031 | — | General | ignored by the client |
| SMSG_GOD_MODE | 0x3b0198 | 3867032 | 1029 (0x405) | General | fixed, 1 bytes |
| — | 0x3b0199 | 3867033 | — | General | ignored by the client |
| — | 0x3b019a | 3867034 | — | General | ignored by the client |
| — | 0x3b019b | 3867035 | — | General | ignored by the client |
| SMSG_SET_FACTION_AT_WAR | 0x3b019c | 3867036 | 16918 (0x4216) | General | fixed, 6 bytes |
| SMSG_CREATE_CHAR | 0x3b019d | 3867037 | 11525 (0x2d05) | General | fixed, 1 bytes |
| SMSG_DELETE_CHAR | 0x3b019e | 3867038 | 772 (0x304) | General | fixed struct |
| SMSG_TRANSFER_ABORTED | 0x3b019f | 3867039 | 1335 (0x537) | General | fixed, 10 bytes |
| SMSG_PET_GUIDS | 0x3b01a0 | 3867040 | 11558 (0x2d26) | General | variable |
| SMSG_CHARACTER_LOGIN_FAILED | 0x3b01a1 | 3867041 | 17431 (0x4417) | General | fixed, 1 bytes |
| SMSG_COMMENTATOR_STATE_CHANGED | 0x3b01a2 | 3867042 | 1847 (0x737) | General | fixed, 1 bytes |
| SMSG_COMMENTATOR_MAP_INFO | 0x3b01a3 | 3867043 | 807 (0x327) | General | variable |
| SMSG_COMMENTATOR_PLAYER_INFO | 0x3b01a4 | 3867044 | 12086 (0x2f36) | General | variable |
| SMSG_UPDATE_ACCOUNT_DATA | 0x3b01a5 | 3867045 | 26679 (0x6837) | General | variable |
| SMSG_ACCOUNT_DATA_TIMES | 0x3b01a6 | 3867046 | 19205 (0x4b05) | General | variable |
| SMSG_GAME_TIME_UPDATE | 0x3b01a7 | 3867047 | 16679 (0x4127) | General | fixed struct |
| SMSG_GAME_TIME_SET | 0x3b01a8 | 3867048 | 20 (0x14) | General | fixed struct |
| SMSG_LOGIN_SET_TIME_SPEED | 0x3b01a9 | 3867049 | 19733 (0x4d15) | General | fixed struct |
| SMSG_LOAD_EQUIPMENT_SET | 0x3b01aa | 3867050 | 11780 (0x2e04) | General | variable |
| SMSG_START_MIRROR_TIMER | 0x3b01ab | 3867051 | 26660 (0x6824) | General | fixed, 18 bytes |
| SMSG_PAUSE_MIRROR_TIMER | 0x3b01ac | 3867052 | 16405 (0x4015) | General | fixed, 2 bytes |
| SMSG_STOP_MIRROR_TIMER | 0x3b01ad | 3867053 | 2822 (0xb06) | General | fixed, 1 bytes |
| SMSG_CORPSE_TRANSPORT_QUERY | 0x3b01ae | 3867054 | — | General | fixed, 16 bytes |
| SMSG_ENCHANTMENT_LOG | 0x3b01af | 3867055 | 24629 (0x6035) | General | fixed, 12 bytes |
| SMSG_SERVER_TIME_OFFSET | 0x3b01b0 | 3867056 | — | General | fixed struct |
| — | 0x3b01b1 | 3867057 | — | General | ignored by the client |
| SMSG_AREA_TRIGGER_NO_CORPSE | 0x3b01b2 | 3867058 | 10772 (0x2a14) | General | fixed struct |
| SMSG_TALENTS_INVOLUNTARILY_RESET | 0x3b01b3 | 3867059 | 11303 (0x2c27) | General | fixed, 1 bytes |
| SMSG_SPEC_INVOLUNTARILY_CHANGED | 0x3b01b4 | 3867060 | — | General | fixed, 1 bytes |
| SMSG_PAGE_TEXT | 0x3b01b5 | 3867061 | 10533 (0x2925) | General | fixed, 0 bytes |
| SMSG_GAME_OBJECT_UI_LINK | 0x3b01b6 | 3867062 | — | General | fixed, 4 bytes |
| — | 0x3b01b7 | 3867063 | — | General | ignored by the client |
| SMSG_STAND_STATE_UPDATE | 0x3b01b8 | 3867064 | 28420 (0x6f04) | General | fixed, 5 bytes |
| SMSG_GAME_OBJECT_RESET_STATE | 0x3b01b9 | 3867065 | 10774 (0x2a16) | General | fixed, 0 bytes |
| — | 0x3b01ba | 3867066 | — | General | ignored by the client |
| — | 0x3b01bb | 3867067 | — | General | ignored by the client |
| SMSG_SUMMON_REQUEST | 0x3b01bc | 3867068 | 10759 (0x2a07) | General | fixed, 10 bytes |
| SMSG_INSPECT_PVP | 0x3b01bd | 3867069 | — | General | variable |
| SMSG_REFER_A_FRIEND_EXPIRED | 0x3b01be | 3867070 | 18740 (0x4934) | General | fixed, 0 bytes |
| SMSG_INITIALIZE_FACTIONS | 0x3b01bf | 3867071 | 17972 (0x4634) | General | variable |
| SMSG_FACTION_BONUS_INFO | 0x3b01c0 | 3867072 | — | General | variable |
| SMSG_CAMERA_EFFECT | 0x3b01c1 | 3867073 | — | General | fixed, 8 bytes |
| SMSG_SOCKET_GEMS_SUCCESS | 0x3b01c2 | 3867074 | — | General | fixed, 0 bytes |
| SMSG_SOCKET_GEMS_FAILURE | 0x3b01c3 | 3867075 | — | General | fixed, 0 bytes |
| — | 0x3b01c4 | 3867076 | — | General | ignored by the client |
| SMSG_SET_FACTION_VISIBLE | 0x3b01c5 | 3867077 | 9509 (0x2525) | General | fixed struct |
| SMSG_SET_FACTION_NOT_VISIBLE | 0x3b01c6 | 3867078 | 26423 (0x6737) | General | fixed struct |
| SMSG_SET_FACTION_STANDING | 0x3b01c7 | 3867079 | 294 (0x126) | General | variable |
| — | 0x3b01c8 | 3867080 | — | General | ignored by the client |
| — | 0x3b01c9 | 3867081 | — | General | ignored by the client |
| — | 0x3b01ca | 3867082 | — | General | ignored by the client |
| SMSG_SET_AI_ANIM_KIT | 0x3b01cb | 3867083 | 17958 (0x4626) | General | fixed, 2 bytes |
| SMSG_PLAY_ONE_SHOT_ANIM_KIT | 0x3b01cc | 3867084 | 18997 (0x4a35) | General | fixed, 2 bytes |
| SMSG_SET_MOVEMENT_ANIM_KIT | 0x3b01cd | 3867085 | 3860 (0xf14) | General | fixed, 2 bytes |
| SMSG_SET_MELEE_ANIM_KIT | 0x3b01ce | 3867086 | 26389 (0x6715) | General | fixed, 2 bytes |
| SMSG_SET_ANIM_TIER | 0x3b01cf | 3867087 | — | General | fixed, 1 bytes |
| SMSG_SET_PROFICIENCY | 0x3b01d0 | 3867088 | 25095 (0x6207) | General | fixed struct |
| — | 0x3b01d1 | 3867089 | — | General | ignored by the client |
| — | 0x3b01d2 | 3867090 | — | General | ignored by the client |
| — | 0x3b01d3 | 3867091 | — | General | ignored by the client |
| SMSG_COOLDOWN_CHEAT | 0x3b01d4 | 3867092 | 17719 (0x4537) | General | fixed, 1 bytes |
| — | 0x3b01d5 | 3867093 | — | General | ignored by the client |
| — | 0x3b01d6 | 3867094 | — | General | ignored by the client |
| — | 0x3b01d7 | 3867095 | — | General | ignored by the client |
| — | 0x3b01d8 | 3867096 | — | General | ignored by the client |
| — | 0x3b01d9 | 3867097 | — | General | ignored by the client |
| — | 0x3b01da | 3867098 | — | General | ignored by the client |
| SMSG_AREA_SPIRIT_HEALER_TIME | 0x3b01db | 3867099 | 1844 (0x734) | General | fixed, 4 bytes |
| SMSG_LOOT_LIST | 0x3b01dc | 3867100 | 26631 (0x6807) | General | variable |
| SMSG_DESTROY_ARENA_UNIT | 0x3b01dd | 3867101 | — | General | fixed, 0 bytes |
| — | 0x3b01de | 3867102 | — | General | ignored by the client |
| SMSG_FEIGN_DEATH_RESISTED | 0x3b01df | 3867103 | 3333 (0xd05) | General | fixed struct |
| SMSG_DURABILITY_DAMAGE_DEATH | 0x3b01e0 | 3867104 | 19495 (0x4c27) | General | fixed struct |
| SMSG_INIT_WORLD_STATES | 0x3b01e1 | 3867105 | 19477 (0x4c15) | General | variable |
| — | 0x3b01e2 | 3867106 | — | General | ignored by the client |
| SMSG_UPDATE_WORLD_STATE | 0x3b01e3 | 3867107 | 18454 (0x4816) | General | fixed, 9 bytes |
| SMSG_PET_ACTION_FEEDBACK | 0x3b01e4 | 3867108 | 2055 (0x807) | General | fixed struct |
| SMSG_CORPSE_RECLAIM_DELAY | 0x3b01e5 | 3867109 | 3380 (0xd34) | General | fixed struct |
| SMSG_REATTACH_RESURRECT | 0x3b01e6 | 3867110 | — | General | fixed, 0 bytes |
| SMSG_PETITION_SIGN_RESULTS | 0x3b01e7 | 3867111 | 25111 (0x6217) | General | fixed, 1 bytes |
| — | 0x3b01e8 | 3867112 | — | General | ignored by the client |
| SMSG_TURN_IN_PETITION_RESULT | 0x3b01e9 | 3867113 | 3847 (0xf07) | General | fixed, 1 bytes |
| SMSG_USE_EQUIPMENT_SET_RESULT | 0x3b01ea | 3867114 | 9252 (0x2424) | General | fixed, 12 bytes |
| — | 0x3b01eb | 3867115 | — | General | ignored by the client |
| SMSG_FORCE_ANIM | 0x3b01ec | 3867116 | 19461 (0x4c05) | General | variable |
| — | 0x3b01ed | 3867117 | — | General | ignored by the client |
| SMSG_INVALID_PROMOTION_CODE | 0x3b01ee | 3867118 | 28453 (0x6f25) | General | fixed struct |
| SMSG_ITEM_TIME_UPDATE | 0x3b01ef | 3867119 | 9223 (0x2407) | General | fixed, 4 bytes |
| SMSG_ITEM_ENCHANT_TIME_UPDATE | 0x3b01f0 | 3867120 | 3879 (0xf27) | General | fixed, 8 bytes |
| SMSG_MAIL_LIST_RESULT | 0x3b01f1 | 3867121 | 16919 (0x4217) | General | variable |
| SMSG_MAIL_QUERY_NEXT_TIME_RESULT | 0x3b01f2 | 3867122 | — | General | variable |
| SMSG_PARTY_MEMBER_PARTIAL_STATE | 0x3b01f3 | 3867123 | — | General | variable |
| SMSG_PARTY_MEMBER_FULL_STATE | 0x3b01f4 | 3867124 | — | General | variable |
| SMSG_PARTY_KILL_LOG | 0x3b01f5 | 3867125 | 18743 (0x4937) | General | fixed, 0 bytes |
| SMSG_PROC_RESIST | 0x3b01f6 | 3867126 | 1062 (0x426) | General | variable |
| SMSG_ISLAND_AZERITE_GAIN | 0x3b01f7 | 3867127 | — | General | fixed, 8 bytes |
| SMSG_ISLAND_COMPLETE | 0x3b01f8 | 3867128 | — | General | variable |
| SMSG_WARFRONT_COMPLETE | 0x3b01f9 | 3867129 | — | General | fixed struct |
| SMSG_EXPLORATION_EXPERIENCE | 0x3b01fa | 3867130 | 26390 (0x6716) | General | fixed struct |
| SMSG_ARENA_TEAM_ROSTER | 0x3b01fb | 3867131 | 10007 (0x2717) | General | variable |
| SMSG_ARENA_TEAM_INVITE | 0x3b01fc | 3867132 | 3894 (0xf36) | General | variable |
| SMSG_ARENA_TEAM_EVENT | 0x3b01fd | 3867133 | 1559 (0x617) | General | variable |
| SMSG_ARENA_TEAM_COMMAND_RESULT | 0x3b01fe | 3867134 | 14771 (0x39b3) | General | variable |
| SMSG_ARENA_TEAM_STATS | 0x3b01ff | 3867135 | 17445 (0x4425) | General | fixed struct |
| SMSG_GET_ACCOUNT_CHARACTER_LIST_RESULT | 0x3b0200 | 3867136 | — | General | variable |
| SMSG_LIVE_REGION_GET_ACCOUNT_CHARACTER_LIST_RESULT | 0x3b0201 | 3867137 | — | General | variable |
| SMSG_CHARACTER_RENAME_RESULT | 0x3b0202 | 3867138 | 8228 (0x2024) | General | variable |
| SMSG_MODIFY_COOLDOWN | 0x3b0203 | 3867139 | 24598 (0x6016) | General | fixed, 9 bytes |
| SMSG_UPDATE_COOLDOWN | 0x3b0204 | 3867140 | — | General | fixed struct |
| SMSG_UPDATE_CHARGE_CATEGORY_COOLDOWN | 0x3b0205 | 3867141 | — | General | fixed, 13 bytes |
| SMSG_PRE_RESSURECT | 0x3b0206 | 3867142 | 27702 (0x6c36) | General | fixed, 0 bytes |
| SMSG_PLAY_SOUND | 0x3b0207 | 3867143 | 8500 (0x2134) | General | fixed, 8 bytes |
| SMSG_PLAY_MUSIC | 0x3b0208 | 3867144 | 19206 (0x4b06) | General | fixed struct |
| SMSG_UI_ACTION | 0x3b0209 | 3867145 | — | General | fixed, 4 bytes |
| SMSG_PLAY_OBJECT_SOUND | 0x3b020a | 3867146 | 9781 (0x2635) | General | fixed, 20 bytes |
| SMSG_PLAY_SPEAKERBOT_SOUND | 0x3b020b | 3867147 | — | General | fixed, 4 bytes |
| SMSG_STOP_SPEAKERBOT_SOUND | 0x3b020c | 3867148 | — | General | fixed, 0 bytes |
| SMSG_LIVE_REGION_CHARACTER_COPY_RESULT | 0x3b020d | 3867149 | — | General | fixed, 5 bytes |
| SMSG_LIVE_REGION_ACCOUNT_RESTORE_RESULT | 0x3b020e | 3867150 | — | General | fixed, 5 bytes |
| — | 0x3b020f | 3867151 | — | General | ignored by the client |
| SMSG_SHOW_TRADE_SKILL_RESPONSE | 0x3b0210 | 3867152 | — | General | variable |
| SMSG_BATTLE_PAY_GET_PRODUCT_LIST_RESPONSE | 0x3b0211 | 3867153 | — | General | variable |
| SMSG_BATTLE_PAY_GET_PURCHASE_LIST_RESPONSE | 0x3b0212 | 3867154 | — | General | variable |
| SMSG_BATTLE_PAY_GET_DISTRIBUTION_LIST_RESPONSE | 0x3b0213 | 3867155 | — | General | variable |
| SMSG_BATTLE_PAY_DISTRIBUTION_UNREVOKED | 0x3b0214 | 3867156 | — | General | fixed, 8 bytes |
| SMSG_BATTLE_PAY_DISTRIBUTION_UPDATE | 0x3b0215 | 3867157 | — | General | variable |
| SMSG_BATTLE_PAY_DELIVERY_STARTED | 0x3b0216 | 3867158 | — | General | fixed struct |
| SMSG_BATTLE_PAY_DELIVERY_ENDED | 0x3b0217 | 3867159 | — | General | variable |
| SMSG_BATTLE_PAY_MOUNT_DELIVERED | 0x3b0218 | 3867160 | — | General | fixed struct |
| SMSG_BATTLE_PAY_BATTLE_PET_DELIVERED | 0x3b0219 | 3867161 | — | General | fixed, 4 bytes |
| SMSG_BATTLE_PAY_COLLECTION_ITEM_DELIVERED | 0x3b021a | 3867162 | — | General | fixed struct |
| — | 0x3b021b | 3867163 | — | General | ignored by the client |
| SMSG_INSTANCE_SAVE_CREATED | 0x3b021c | 3867164 | 292 (0x124) | General | fixed, 1 bytes |
| SMSG_ENCOUNTER_START | 0x3b021d | 3867165 | — | General | variable |
| SMSG_ENCOUNTER_END | 0x3b021e | 3867166 | — | General | fixed, 17 bytes |
| SMSG_BATTLE_PAY_START_PURCHASE_RESPONSE | 0x3b021f | 3867167 | — | General | fixed, 16 bytes |
| SMSG_BATTLE_PAY_START_DISTRIBUTION_ASSIGN_TO_TARGET_RESPONSE | 0x3b0220 | 3867168 | — | General | fixed struct |
| — | 0x3b0221 | 3867169 | — | General | ignored by the client |
| SMSG_BATTLE_PAY_PURCHASE_UPDATE | 0x3b0222 | 3867170 | — | General | variable |
| SMSG_BATTLE_PAY_CONFIRM_PURCHASE | 0x3b0223 | 3867171 | — | General | fixed struct |
| SMSG_BATTLE_PAY_ACK_FAILED | 0x3b0224 | 3867172 | — | General | fixed, 20 bytes |
| — | 0x3b0225 | 3867173 | — | General | ignored by the client |
| — | 0x3b0226 | 3867174 | — | General | ignored by the client |
| SMSG_CONTACT_LIST | 0x3b0227 | 3867175 | 24599 (0x6017) | General | variable |
| SMSG_FRIEND_STATUS | 0x3b0228 | 3867176 | 1815 (0x717) | General | variable |
| SMSG_CHARACTER_OBJECT_TEST_RESPONSE | 0x3b0229 | 3867177 | — | General | fixed struct |
| SMSG_BATTLENET_CHALLENGE_START | 0x3b022a | 3867178 | — | General | variable |
| SMSG_BATTLENET_CHALLENGE_ABORT | 0x3b022b | 3867179 | — | General | fixed, 5 bytes |
| SMSG_GROUP_DECLINE | 0x3b022c | 3867180 | 26677 (0x6835) | General | variable |
| SMSG_GROUP_REQUEST_DECLINE | 0x3b022d | 3867181 | — | General | variable |
| SMSG_GROUP_UNINVITE | 0x3b022e | 3867182 | 2567 (0xa07) | General | fixed struct |
| SMSG_GROUP_DESTROYED | 0x3b022f | 3867183 | 8711 (0x2207) | General | fixed struct |
| SMSG_GROUP_AUTO_KICK | 0x3b0230 | 3867184 | — | General | fixed struct |
| SMSG_PARTY_COMMAND_RESULT | 0x3b0231 | 3867185 | 28167 (0x6e07) | General | variable |
| SMSG_SUGGEST_INVITE_INFORM | 0x3b0232 | 3867186 | — | General | variable |
| SMSG_GOSSIP_POI | 0x3b0233 | 3867187 | 17174 (0x4316) | General | variable |
| — | 0x3b0234 | 3867188 | — | General | ignored by the client |
| — | 0x3b0235 | 3867189 | — | General | ignored by the client |
| — | 0x3b0236 | 3867190 | — | General | ignored by the client |
| — | 0x3b0237 | 3867191 | — | General | ignored by the client |
| — | 0x3b0238 | 3867192 | — | General | ignored by the client |
| — | 0x3b0239 | 3867193 | — | General | ignored by the client |
| — | 0x3b023a | 3867194 | — | General | ignored by the client |
| — | 0x3b023b | 3867195 | — | General | ignored by the client |
| SMSG_READ_ITEM_RESULT_OK | 0x3b023c | 3867196 | 9733 (0x2605) | General | fixed, 0 bytes |
| — | 0x3b023d | 3867197 | — | General | ignored by the client |
| — | 0x3b023e | 3867198 | — | General | ignored by the client |
| — | 0x3b023f | 3867199 | — | General | ignored by the client |
| — | 0x3b0240 | 3867200 | — | General | ignored by the client |
| — | 0x3b0241 | 3867201 | — | General | ignored by the client |
| — | 0x3b0242 | 3867202 | — | General | ignored by the client |
| — | 0x3b0243 | 3867203 | — | General | ignored by the client |
| SMSG_READ_ITEM_RESULT_FAILED | 0x3b0244 | 3867204 | 3862 (0xf16) | General | fixed, 5 bytes |
| SMSG_SCENARIO_VACATE | 0x3b0245 | 3867205 | — | General | fixed, 9 bytes |
| — | 0x3b0246 | 3867206 | — | General | ignored by the client |
| SMSG_CHAR_FACTION_CHANGE_RESULT | 0x3b0247 | 3867207 | 19462 (0x4c06) | General | variable |
| SMSG_RAID_DIFFICULTY_SET | 0x3b0248 | 3867208 | — | General | fixed struct |
| SMSG_XP_GAIN_ENABLED | 0x3b0249 | 3867209 | — | General | fixed, 1 bytes |
| SMSG_RAID_GROUP_ONLY | 0x3b024a | 3867210 | 2103 (0x837) | General | fixed struct |
| SMSG_INSTANCE_ENCOUNTER_ENGAGE_UNIT | 0x3b024b | 3867211 | — | General | fixed, 1 bytes |
| SMSG_INSTANCE_ENCOUNTER_DISENGAGE_UNIT | 0x3b024c | 3867212 | — | General | fixed, 0 bytes |
| SMSG_INSTANCE_ENCOUNTER_CHANGE_PRIORITY | 0x3b024d | 3867213 | — | General | fixed, 1 bytes |
| SMSG_INSTANCE_ENCOUNTER_TIMER_START | 0x3b024e | 3867214 | — | General | fixed struct |
| SMSG_INSTANCE_ENCOUNTER_OBJECTIVE_START | 0x3b024f | 3867215 | — | General | fixed struct |
| SMSG_INSTANCE_ENCOUNTER_OBJECTIVE_COMPLETE | 0x3b0250 | 3867216 | — | General | fixed struct |
| SMSG_INSTANCE_ENCOUNTER_START | 0x3b0251 | 3867217 | — | General | fixed, 17 bytes |
| SMSG_INSTANCE_ENCOUNTER_UPDATE_SUPPRESS_RELEASE | 0x3b0252 | 3867218 | — | General | fixed, 1 bytes |
| SMSG_INSTANCE_ENCOUNTER_UPDATE_ALLOW_RELEASE_IN_PROGRESS | 0x3b0253 | 3867219 | — | General | fixed, 1 bytes |
| SMSG_INSTANCE_ENCOUNTER_OBJECTIVE_UPDATE | 0x3b0254 | 3867220 | — | General | fixed struct |
| SMSG_INSTANCE_ENCOUNTER_END | 0x3b0255 | 3867221 | — | General | fixed struct |
| SMSG_INSTANCE_ENCOUNTER_IN_COMBAT_RESURRECTION | 0x3b0256 | 3867222 | — | General | fixed struct |
| SMSG_INSTANCE_ENCOUNTER_GAIN_COMBAT_RESURRECTION_CHARGE | 0x3b0257 | 3867223 | — | General | fixed struct |
| SMSG_INSTANCE_ENCOUNTER_PHASE_SHIFT_CHANGED | 0x3b0258 | 3867224 | — | General | fixed struct |
| SMSG_TUTORIAL_FLAGS | 0x3b0259 | 3867225 | 2869 (0xb35) | General | fixed struct |
| SMSG_CHARACTER_UPGRADE_STARTED | 0x3b025a | 3867226 | — | General | fixed, 0 bytes |
| SMSG_CHARACTER_UPGRADE_COMPLETE | 0x3b025b | 3867227 | — | General | fixed, 0 bytes |
| SMSG_CHARACTER_UPGRADE_ABORTED | 0x3b025c | 3867228 | — | General | fixed, 0 bytes |
| SMSG_CHARACTER_CHECK_UPGRADE_RESULT | 0x3b025d | 3867229 | — | General | fixed struct |
| SMSG_CHARACTER_UPGRADE_MANUAL_UNREVOKE_RESULT | 0x3b025e | 3867230 | — | General | fixed struct |
| SMSG_UPDATE_CHARACTER_FLAGS | 0x3b025f | 3867231 | — | General | variable |
| — | 0x3b0260 | 3867232 | — | General | ignored by the client |
| — | 0x3b0261 | 3867233 | — | General | ignored by the client |
| — | 0x3b0262 | 3867234 | — | General | ignored by the client |
| SMSG_ITEM_COOLDOWN | 0x3b0263 | 3867235 | 19732 (0x4d14) | General | fixed, 8 bytes |
| SMSG_EMOTE | 0x3b0264 | 3867236 | 2612 (0xa34) | General | variable |
| SMSG_TRIGGER_CINEMATIC | 0x3b0265 | 3867237 | 27687 (0x6c27) | General | fixed struct |
| SMSG_UNDELETE_CHARACTER_RESPONSE | 0x3b0266 | 3867238 | — | General | fixed, 8 bytes |
| SMSG_UNDELETE_COOLDOWN_STATUS_RESPONSE | 0x3b0267 | 3867239 | — | General | fixed, 9 bytes |
| — | 0x3b0268 | 3867240 | — | General | ignored by the client |
| — | 0x3b0269 | 3867241 | — | General | ignored by the client |
| — | 0x3b026a | 3867242 | — | General | ignored by the client |
| SMSG_SET_LOOT_METHOD_FAILED | 0x3b026b | 3867243 | — | General | fixed struct |
| SMSG_COMMERCE_TOKEN_GET_COUNT_RESPONSE | 0x3b026c | 3867244 | — | General | variable |
| SMSG_COMMERCE_TOKEN_UPDATE | 0x3b026d | 3867245 | — | General | variable |
| SMSG_COMMERCE_TOKEN_GET_MARKET_PRICE_RESPONSE | 0x3b026e | 3867246 | — | General | fixed struct |
| SMSG_AUCTIONABLE_TOKEN_SELL_CONFIRM_REQUIRED | 0x3b026f | 3867247 | — | General | fixed struct |
| SMSG_AUCTIONABLE_TOKEN_SELL_AT_MARKET_PRICE_RESPONSE | 0x3b0270 | 3867248 | — | General | fixed struct |
| SMSG_AUCTIONABLE_TOKEN_AUCTION_SOLD | 0x3b0271 | 3867249 | — | General | fixed struct |
| SMSG_CONSUMABLE_TOKEN_CAN_VETERAN_BUY_RESPONSE | 0x3b0272 | 3867250 | — | General | fixed struct |
| SMSG_CONSUMABLE_TOKEN_BUY_CHOICE_REQUIRED | 0x3b0273 | 3867251 | — | General | fixed struct |
| SMSG_CONSUMABLE_TOKEN_BUY_AT_MARKET_PRICE_RESPONSE | 0x3b0274 | 3867252 | — | General | fixed struct |
| SMSG_GET_REMAINING_GAME_TIME_RESPONSE | 0x3b0275 | 3867253 | — | General | fixed, 9 bytes |
| SMSG_CONSUMABLE_TOKEN_REDEEM_CONFIRM_REQUIRED | 0x3b0276 | 3867254 | — | General | fixed, 33 bytes |
| SMSG_CONSUMABLE_TOKEN_REDEEM_RESPONSE | 0x3b0277 | 3867255 | — | General | fixed, 12 bytes |
| SMSG_COMMERCE_TOKEN_GET_LOG_RESPONSE | 0x3b0278 | 3867256 | — | General | variable |
| — | 0x3b0279 | 3867257 | — | General | ignored by the client |
| — | 0x3b027a | 3867258 | — | General | ignored by the client |
| — | 0x3b027b | 3867259 | — | General | ignored by the client |
| — | 0x3b027c | 3867260 | — | General | ignored by the client |
| — | 0x3b027d | 3867261 | — | General | ignored by the client |
| — | 0x3b027e | 3867262 | — | General | ignored by the client |
| — | 0x3b027f | 3867263 | — | General | ignored by the client |
| — | 0x3b0280 | 3867264 | — | General | ignored by the client |
| — | 0x3b0281 | 3867265 | — | General | ignored by the client |
| — | 0x3b0282 | 3867266 | — | General | ignored by the client |
| — | 0x3b0283 | 3867267 | — | General | ignored by the client |
| — | 0x3b0284 | 3867268 | — | General | ignored by the client |
| — | 0x3b0285 | 3867269 | — | General | ignored by the client |
| — | 0x3b0286 | 3867270 | — | General | ignored by the client |
| SMSG_SCENARIO_COMPLETED | 0x3b0287 | 3867271 | — | General | fixed struct |
| — | 0x3b0288 | 3867272 | — | General | ignored by the client |
| — | 0x3b0289 | 3867273 | — | General | ignored by the client |
| SMSG_GET_VAS_ACCOUNT_CHARACTER_LIST_RESULT | 0x3b028a | 3867274 | — | General | variable |
| SMSG_GET_VAS_TRANSFER_TARGET_REALM_LIST_RESULT | 0x3b028b | 3867275 | — | General | variable |
| SMSG_VAS_PURCHASE_STATE_UPDATE | 0x3b028c | 3867276 | — | General | variable |
| SMSG_VAS_PURCHASE_COMPLETE | 0x3b028d | 3867277 | — | General | variable |
| SMSG_ENUM_VAS_PURCHASE_STATES_RESPONSE | 0x3b028e | 3867278 | — | General | variable |
| — | 0x3b028f | 3867279 | — | General | ignored by the client |
| — | 0x3b0290 | 3867280 | — | General | ignored by the client |
| — | 0x3b0291 | 3867281 | — | General | ignored by the client |
| — | 0x3b0292 | 3867282 | — | General | ignored by the client |
| — | 0x3b0293 | 3867283 | — | General | ignored by the client |
| — | 0x3b0294 | 3867284 | — | General | ignored by the client |
| SMSG_ALLIED_RACE_DETAILS | 0x3b0295 | 3867285 | — | General | fixed, 4 bytes |
| — | 0x3b0296 | 3867286 | — | General | ignored by the client |
| — | 0x3b0297 | 3867287 | — | General | ignored by the client |
| SMSG_COVENANT_PREVIEW_OPEN_NPC | 0x3b0298 | 3867288 | — | General | fixed, 4 bytes |
| — | 0x3b0299 | 3867289 | — | General | ignored by the client |
| — | 0x3b029a | 3867290 | — | General | ignored by the client |
| — | 0x3b029b | 3867291 | — | General | ignored by the client |
| SMSG_SCENARIO_UI_UPDATE | 0x3b029c | 3867292 | — | General | variable |
| SMSG_SCENARIO_SHOW_CRITERIA | 0x3b029d | 3867293 | — | General | fixed, 1 bytes |
| — | 0x3b029e | 3867294 | — | General | ignored by the client |
| SMSG_GAME_OBJECT_SET_STATE_LOCAL | 0x3b029f | 3867295 | — | General | fixed, 1 bytes |
| SMSG_BATTLENET_RESPONSE | 0x3b02a0 | 3867296 | — | General | variable |
| SMSG_BATTLENET_NOTIFICATION | 0x3b02a1 | 3867297 | — | General | variable |
| SMSG_BATTLE_NET_CONNECTION_STATUS | 0x3b02a2 | 3867298 | — | General | fixed, 1 bytes |
| SMSG_CHANGE_REALM_TICKET_RESPONSE | 0x3b02a3 | 3867299 | — | General | variable |
| — | 0x3b02a4 | 3867300 | — | General | ignored by the client |
| — | 0x3b02a5 | 3867301 | — | General | ignored by the client |
| — | 0x3b02a6 | 3867302 | — | General | ignored by the client |
| — | 0x3b02a7 | 3867303 | — | General | ignored by the client |
| — | 0x3b02a8 | 3867304 | — | General | ignored by the client |
| — | 0x3b02a9 | 3867305 | — | General | ignored by the client |
| — | 0x3b02aa | 3867306 | — | General | ignored by the client |
| — | 0x3b02ab | 3867307 | — | General | ignored by the client |
| SMSG_FAILED_QUEST_TURN_IN | 0x3b02ac | 3867308 | — | General | variable |
| SMSG_QUEUE_SUMMARY_UPDATE | 0x3b02ad | 3867309 | — | General | variable |
| SMSG_INVENTORY_FIXUP_COMPLETE | 0x3b02ae | 3867310 | — | General | fixed, 1 bytes |
| SMSG_CONFIRM_PARTY_INVITE | 0x3b02af | 3867311 | — | General | variable |
| SMSG_CAN_REDEEM_TOKEN_FOR_BALANCE_RESPONSE | 0x3b02b0 | 3867312 | — | General | fixed, 25 bytes |
| SMSG_BATTLE_PAY_VALIDATE_PURCHASE_RESPONSE | 0x3b02b1 | 3867313 | — | General | fixed, 25 bytes |
| SMSG_VAS_GET_SERVICE_STATUS_RESPONSE | 0x3b02b2 | 3867314 | — | General | fixed, 1 bytes |
| SMSG_VAS_GET_QUEUE_MINUTES_RESPONSE | 0x3b02b3 | 3867315 | — | General | fixed struct |
| — | 0x3b02b4 | 3867316 | — | General | ignored by the client |
| SMSG_VAS_CHECK_TRANSFER_OK_RESPONSE | 0x3b02b5 | 3867317 | — | General | variable |
| SMSG_CONTRIBUTION_LAST_UPDATE_RESPONSE | 0x3b02b6 | 3867318 | — | General | fixed struct |
| SMSG_GENERATE_SSO_TOKEN_RESPONSE | 0x3b02b7 | 3867319 | — | General | variable |
| SMSG_VOICE_LOGIN_RESPONSE | 0x3b02b8 | 3867320 | — | General | variable |
| SMSG_VOICE_CHANNEL_INFO_RESPONSE | 0x3b02b9 | 3867321 | — | General | variable |
| SMSG_UPDATE_CELESTIAL_BODY | 0x3b02ba | 3867322 | — | General | fixed struct |
| SMSG_WARDEN3_ENABLED | 0x3b02bb | 3867323 | — | General | fixed struct |
| SMSG_WARDEN3_DISABLED | 0x3b02bc | 3867324 | — | General | fixed struct |
| SMSG_BATTLE_PAY_START_CHECKOUT | 0x3b02bd | 3867325 | — | General | variable |
| SMSG_UPDATE_BNET_SESSION_KEY | 0x3b02be | 3867326 | — | General | variable |
| SMSG_INVENTORY_FULL_OVERFLOW | 0x3b02bf | 3867327 | — | General | fixed struct |
| SMSG_WILL_BE_KICKED_FOR_ADDED_SUBSCRIPTION_TIME | 0x3b02c0 | 3867328 | — | General | fixed struct |
| SMSG_UPDATE_GAME_TIME_STATE | 0x3b02c1 | 3867329 | — | General | fixed, 13 bytes |
| — | 0x3b02c2 | 3867330 | — | General | ignored by the client |
| SMSG_GAME_OBJECT_BASE | 0x3b02c3 | 3867331 | — | General | variable |
| SMSG_LEGACY_LOOT_RULES | 0x3b02c4 | 3867332 | — | General | fixed, 1 bytes |
| — | 0x3b02c5 | 3867333 | — | General | ignored by the client |
| — | 0x3b02c6 | 3867334 | — | General | ignored by the client |
| — | 0x3b02c7 | 3867335 | — | General | ignored by the client |
| — | 0x3b02c8 | 3867336 | — | General | ignored by the client |
| SMSG_BATCH_PRESENCE_SUBSCRIPTION | 0x3b02c9 | 3867337 | — | General | variable |
| SMSG_MOVEMENT_ENFORCEMENT_ALERT | 0x3b02ca | 3867338 | — | General | variable |
| — | 0x3b02cb | 3867339 | — | General | ignored by the client |
| — | 0x3b02cc | 3867340 | — | General | ignored by the client |
| SMSG_PREPOPULATE_NAME_CACHE | 0x3b02cd | 3867341 | — | General | variable |
| — | 0x3b02ce | 3867342 | — | General | ignored by the client |
| — | 0x3b02cf | 3867343 | — | General | ignored by the client |
| SMSG_RETURN_RECRUITING_CLUBS | 0x3b02d0 | 3867344 | — | General | variable |
| SMSG_RETURN_APPLICANT_LIST | 0x3b02d1 | 3867345 | — | General | variable |
| SMSG_CLUB_FINDER_RESPONSE_CHARACTER_APPLICATION_LIST | 0x3b02d2 | 3867346 | — | General | variable |
| SMSG_CLUB_FINDER_UPDATE_APPLICATIONS | 0x3b02d3 | 3867347 | — | General | variable |
| SMSG_CLUB_FINDER_ERROR_MESSAGE | 0x3b02d4 | 3867348 | — | General | fixed, 1 bytes |
| SMSG_CLUB_FINDER_LOOKUP_CLUB_POSTINGS_LIST | 0x3b02d5 | 3867349 | — | General | variable |
| SMSG_CLUB_FINDER_RESPONSE_POST_RECRUITMENT_MESSAGE | 0x3b02d6 | 3867350 | — | General | fixed, 1 bytes |
| SMSG_CLUB_FINDER_GET_CLUB_POSTING_IDS_RESPONSE | 0x3b02d7 | 3867351 | — | General | variable |
| — | 0x3b02d8 | 3867352 | — | General | ignored by the client |
| — | 0x3b02d9 | 3867353 | — | General | ignored by the client |
| — | 0x3b02da | 3867354 | — | General | ignored by the client |
| — | 0x3b02db | 3867355 | — | General | ignored by the client |
| — | 0x3b02dc | 3867356 | — | General | ignored by the client |
| SMSG_QUEST_SESSION_RESULT | 0x3b02dd | 3867357 | — | General | fixed, 1 bytes |
| SMSG_QUEST_SESSION_READY_CHECK | 0x3b02de | 3867358 | — | General | fixed, 0 bytes |
| SMSG_QUEST_SESSION_READY_CHECK_RESPONSE | 0x3b02df | 3867359 | — | General | fixed, 1 bytes |
| — | 0x3b02e0 | 3867360 | — | General | ignored by the client |
| — | 0x3b02e1 | 3867361 | — | General | ignored by the client |
| — | 0x3b02e2 | 3867362 | — | General | ignored by the client |
| — | 0x3b02e3 | 3867363 | — | General | ignored by the client |
| — | 0x3b02e4 | 3867364 | — | General | ignored by the client |
| SMSG_AUCTION_LIST_BUCKETS_RESULT | 0x3b02e5 | 3867365 | — | General | variable |
| SMSG_AUCTION_LIST_ITEMS_RESULT | 0x3b02e6 | 3867366 | — | General | variable |
| — | 0x3b02e7 | 3867367 | — | General | ignored by the client |
| SMSG_AUCTION_LIST_OWNED_ITEMS_RESULT | 0x3b02e8 | 3867368 | — | General | variable |
| SMSG_AUCTION_LIST_BIDDED_ITEMS_RESULT | 0x3b02e9 | 3867369 | — | General | variable |
| SMSG_AUCTION_GET_COMMODITY_QUOTE_RESULT | 0x3b02ea | 3867370 | — | General | variable |
| SMSG_ACCOUNT_CRITERIA_UPDATE | 0x3b02eb | 3867371 | — | General | variable |
| — | 0x3b02ec | 3867372 | — | General | ignored by the client |
| — | 0x3b02ed | 3867373 | — | General | ignored by the client |
| SMSG_SYNC_WOW_ENTITLEMENTS | 0x3b02ee | 3867374 | — | General | variable |
| SMSG_WOW_ENTITLEMENT_NOTIFICATION | 0x3b02ef | 3867375 | — | General | variable |
| — | 0x3b02f0 | 3867376 | — | General | ignored by the client |
| — | 0x3b02f1 | 3867377 | — | General | ignored by the client |
| SMSG_AUCTION_FAVORITE_LIST | 0x3b02f2 | 3867378 | — | General | variable |
| — | 0x3b02f3 | 3867379 | — | General | ignored by the client |
| — | 0x3b02f4 | 3867380 | — | General | ignored by the client |
| — | 0x3b02f5 | 3867381 | — | General | ignored by the client |
| — | 0x3b02f6 | 3867382 | — | General | ignored by the client |
| — | 0x3b02f7 | 3867383 | — | General | ignored by the client |
| — | 0x3b02f8 | 3867384 | — | General | ignored by the client |
| — | 0x3b02f9 | 3867385 | — | General | ignored by the client |
| SMSG_PARTY_NOTIFY_LFG_LEADER_CHANGE | 0x3b02fa | 3867386 | — | General | fixed struct |
| — | 0x3b02fb | 3867387 | — | General | ignored by the client |
| — | 0x3b02fc | 3867388 | — | General | ignored by the client |
| — | 0x3b02fd | 3867389 | — | General | ignored by the client |
| — | 0x3b02fe | 3867390 | — | General | ignored by the client |
| — | 0x3b02ff | 3867391 | — | General | ignored by the client |
| — | 0x3b0300 | 3867392 | — | General | ignored by the client |
| — | 0x3b0301 | 3867393 | — | General | ignored by the client |
| — | 0x3b0302 | 3867394 | — | General | ignored by the client |
| SMSG_AREA_TRIGGER_MESSAGE | 0x3b0303 | 3867395 | 17669 (0x4505) | General | fixed struct |
| SMSG_VOICE_CHANNEL_STT_TOKEN_RESPONSE | 0x3b0304 | 3867396 | — | General | variable |
| SMSG_ACCOUNT_NOTIFICATIONS_RESPONSE | 0x3b0305 | 3867397 | — | General | variable |
| SMSG_LATENCY_REPORT_PING | 0x3b0306 | 3867398 | — | General | variable |
| — | 0x3b0307 | 3867399 | — | General | ignored by the client |
| SMSG_UPDATE_AADC_STATUS_RESPONSE | 0x3b0308 | 3867400 | — | General | fixed, 1 bytes |
| — | 0x3b0309 | 3867401 | — | General | ignored by the client |
| SMSG_BATTLE_PAY_DISTRIBUTION_ASSIGN_VAS_RESPONSE | 0x3b030a | 3867402 | — | General | fixed, 12 bytes |
| SMSG_NPC_INTERACTION_OPEN_RESULT | 0x3b030b | 3867403 | — | General | fixed, 5 bytes |
| SMSG_GAME_OBJECT_INTERACTION | 0x3b030c | 3867404 | — | General | fixed, 4 bytes |
| SMSG_GAME_OBJECT_CLOSE_INTERACTION | 0x3b030d | 3867405 | — | General | fixed struct |
| SMSG_CLUB_FINDER_WHISPER_APPLICANT_RESPONSE | 0x3b030e | 3867406 | — | General | fixed, 0 bytes |
| SMSG_LOBBY_MATCHMAKER_LOBBY_ACQUIRED_SERVER | 0x3b030f | 3867407 | — | General | fixed, 11 bytes |
| SMSG_LOBBY_MATCHMAKER_PARTY_INFO | 0x3b0310 | 3867408 | — | General | variable |
| SMSG_LOBBY_MATCHMAKER_PARTY_INVITE_REJECTED | 0x3b0311 | 3867409 | — | General | variable |
| SMSG_LOBBY_MATCHMAKER_RECEIVE_INVITE | 0x3b0312 | 3867410 | — | General | variable |
| SMSG_LOBBY_MATCHMAKER_QUEUE_PROPOSED | 0x3b0313 | 3867411 | — | General | fixed struct |
| SMSG_LOBBY_MATCHMAKER_QUEUE_RESULT | 0x3b0314 | 3867412 | — | General | fixed, 1 bytes |
| — | 0x3b0315 | 3867413 | — | General | ignored by the client |
| SMSG_SOCIAL_CONTRACT_REQUEST_RESPONSE | 0x3b0316 | 3867414 | — | General | fixed, 1 bytes |
| SMSG_WOW_LABS_NOTIFY_PLAYERS_MATCH_END | 0x3b0317 | 3867415 | — | General | fixed struct |
| SMSG_WOW_LABS_NOTIFY_PLAYERS_MATCH_STATE_CHANGED | 0x3b0318 | 3867416 | — | General | fixed, 4 bytes |
| SMSG_WOW_LABS_SET_WOW_LABS_AREA_ID_RESPONSE | 0x3b0319 | 3867417 | — | General | fixed, 1 bytes |
| SMSG_QUERY_SELECTED_WOW_LABS_AREA_RESPONSE | 0x3b031a | 3867418 | — | General | fixed struct |
| SMSG_QUERY_WOW_LABS_AREA_INFO_RESPONSE | 0x3b031b | 3867419 | — | General | variable |
| — | 0x3b031c | 3867420 | — | General | fixed struct |
| SMSG_WOW_LABS_SET_PREDICTION_CIRCLE | 0x3b031d | 3867421 | — | General | fixed, 16 bytes |
| — | 0x3b031e | 3867422 | — | General | ignored by the client |
| — | 0x3b031f | 3867423 | — | General | ignored by the client |
| — | 0x3b0320 | 3867424 | — | General | ignored by the client |
| — | 0x3b0321 | 3867425 | — | General | ignored by the client |
| SMSG_AUCTION_DISABLE_NEW_POSTINGS | 0x3b0322 | 3867426 | — | General | fixed struct |
| — | 0x3b0323 | 3867427 | — | General | ignored by the client |
| SMSG_WOW_LABS_PARTY_ERROR | 0x3b0324 | 3867428 | — | General | fixed, 1 bytes |
| — | 0x3b0325 | 3867429 | — | General | ignored by the client |
| — | 0x3b0326 | 3867430 | — | General | ignored by the client |
| SMSG_ACCOUNT_EXPORT_RESPONSE | 0x3b0327 | 3867431 | — | General | variable |
| — | 0x3b0328 | 3867432 | — | General | ignored by the client |
| SMSG_SPECTATE_PLAYER | 0x3b0329 | 3867433 | — | General | fixed, 0 bytes |
| SMSG_SPECTATE_END | 0x3b032a | 3867434 | — | General | fixed struct |
| SMSG_NEW_DATA_BUILD | 0x3b032b | 3867435 | — | General | variable |
| SMSG_GET_REALM_HIDDEN_RESULT | 0x3b032c | 3867436 | — | General | variable |
| — | 0x3b032d | 3867437 | — | General | ignored by the client |
| — | 0x3b032e | 3867438 | — | General | ignored by the client |
| — | 0x3b032f | 3867439 | — | General | ignored by the client |
| — | 0x3b0330 | 3867440 | — | General | ignored by the client |
| — | 0x3b0331 | 3867441 | — | General | ignored by the client |
| — | 0x3b0332 | 3867442 | — | General | ignored by the client |
| — | 0x3b0333 | 3867443 | — | General | ignored by the client |
| SMSG_HARDCORE_DEATH_ALERT | 0x3b0334 | 3867444 | — | General | variable |
| — | 0x3b0335 | 3867445 | — | General | ignored by the client |
| SMSG_ACCOUNT_CHARACTER_CURRENCY_LISTS | 0x3b0336 | 3867446 | — | General | variable |
| — | 0x3b0337 | 3867447 | — | General | ignored by the client |
| — | 0x3b0338 | 3867448 | — | General | ignored by the client |
| — | 0x3b0339 | 3867449 | — | General | ignored by the client |
| — | 0x3b033a | 3867450 | — | General | ignored by the client |
| — | 0x3b033b | 3867451 | — | General | ignored by the client |
| — | 0x3c0000 | 3932160 | — | Area trigger | ignored by the client |
| — | 0x3c0001 | 3932161 | — | Area trigger | ignored by the client |
| — | 0x3c0002 | 3932162 | — | Area trigger | ignored by the client |
| SMSG_AREA_TRIGGER_RE_PATH | 0x3c0003 | 3932163 | — | Area trigger | variable |
| — | 0x3c0004 | 3932164 | — | Area trigger | ignored by the client |
| — | 0x3c0005 | 3932165 | — | Area trigger | ignored by the client |
| SMSG_AREA_TRIGGER_FORCE_SET_POSITION_AND_FACING | 0x3c0006 | 3932166 | — | Area trigger | variable |
| SMSG_AREA_TRIGGER_UNATTACH | 0x3c0007 | 3932167 | — | Area trigger | variable |
| SMSG_AREA_TRIGGER_RE_SHAPE | 0x3c0008 | 3932168 | — | Area trigger | variable |
| SMSG_AREA_TRIGGER_DENIED | 0x3c0009 | 3932169 | — | Area trigger | fixed, 5 bytes |
| SMSG_DB_REPLY | 0x3f0000 | 4128768 | 14500 (0x38a4) | Cache | variable |
| SMSG_AVAILABLE_HOTFIXES | 0x3f0001 | 4128769 | — | Cache | variable |
| SMSG_HOTFIX_MESSAGE | 0x3f0002 | 4128770 | — | Cache | variable |
| SMSG_HOTFIX_CONNECT | 0x3f0003 | 4128771 | — | Cache | variable |
| — | 0x3f0004 | 4128772 | — | Cache | ignored by the client |
| SMSG_REALM_QUERY_RESPONSE | 0x3f0005 | 4128773 | — | Cache | variable |
| SMSG_QUERY_CREATURE_RESPONSE | 0x3f0006 | 4128774 | 24612 (0x6024) | Cache | variable |
| SMSG_QUERY_GAME_OBJECT_RESPONSE | 0x3f0007 | 4128775 | 2325 (0x915) | Cache | variable |
| SMSG_QUERY_NPC_TEXT_RESPONSE | 0x3f0008 | 4128776 | 17462 (0x4436) | Cache | variable |
| SMSG_QUERY_PAGE_TEXT_RESPONSE | 0x3f0009 | 4128777 | 11028 (0x2b14) | Cache | variable |
| SMSG_INVALIDATE_PAGE_TEXT | 0x3f000a | 4128778 | — | Cache | fixed struct |
| SMSG_QUERY_PET_NAME_RESPONSE | 0x3f000b | 4128779 | 19511 (0x4c37) | Cache | variable |
| SMSG_QUERY_BATTLE_PET_NAME_RESPONSE | 0x3f000c | 4128780 | — | Cache | variable |
| SMSG_QUERY_PETITION_RESPONSE | 0x3f000d | 4128781 | — | Cache | variable |
| SMSG_CACHE_VERSION | 0x3f000e | 4128782 | 10036 (0x2734) | Cache | fixed struct |
| SMSG_CACHE_INFO | 0x3f000f | 4128783 | — | Cache | variable |
| SMSG_QUERY_ITEM_TEXT_RESPONSE | 0x3f0010 | 4128784 | 10021 (0x2725) | Cache | variable |
| SMSG_TREASURE_PICKER_RESPONSE | 0x3f0011 | 4128785 | — | Cache | variable |
| SMSG_QUERY_ARENA_TEAM_RESPONSE | 0x3f0012 | 4128786 | — | Cache | variable |
| SMSG_CHAT_IGNORED_ACCOUNT_MUTED | 0x400000 | 4194304 | 5540 (0x15a4) | Chat | fixed struct |
| SMSG_CHAT | 0x400001 | 4194305 | 8230 (0x2026) | Chat | variable |
| SMSG_WHO | 0x400002 | 4194306 | 26887 (0x6907) | Chat | variable |
| — | 0x400003 | 4194307 | — | Chat | ignored by the client |
| SMSG_CHAT_PLAYER_AMBIGUOUS | 0x400004 | 4194308 | 12084 (0x2f34) | Chat | variable |
| SMSG_EXPECTED_SPAM_RECORDS | 0x400005 | 4194309 | 19766 (0x4d36) | Chat | variable |
| SMSG_CHAT_NOT_IN_PARTY | 0x400006 | 4194310 | 27156 (0x6a14) | Chat | fixed struct |
| SMSG_CHAT_RESTRICTED | 0x400007 | 4194311 | 25910 (0x6536) | Chat | fixed, 4 bytes |
| SMSG_RAID_INSTANCE_MESSAGE | 0x400008 | 4194312 | 28181 (0x6e15) | Chat | variable |
| SMSG_ZONE_UNDER_ATTACK | 0x400009 | 4194313 | 2566 (0xa06) | Chat | fixed struct |
| SMSG_DEFENSE_MESSAGE | 0x40000a | 4194314 | 788 (0x314) | Chat | variable |
| SMSG_CHAT_PLAYER_NOTFOUND | 0x40000b | 4194315 | 9510 (0x2526) | Chat | variable |
| SMSG_CHAT_AUTO_RESPONDED | 0x40000c | 4194316 | — | Chat | variable |
| SMSG_USERLIST_ADD | 0x40000d | 4194317 | 3895 (0xf37) | Chat | variable |
| SMSG_USERLIST_REMOVE | 0x40000e | 4194318 | 8198 (0x2006) | Chat | variable |
| SMSG_USERLIST_UPDATE | 0x40000f | 4194319 | 309 (0x135) | Chat | variable |
| SMSG_BROADCAST_ACHIEVEMENT | 0x400010 | 4194320 | — | Chat | variable |
| SMSG_BROADCAST_LEVELUP | 0x400011 | 4194321 | — | Chat | variable |
| SMSG_CHAT_DOWN | 0x400012 | 4194322 | — | Chat | fixed struct |
| SMSG_CHAT_IS_DOWN | 0x400013 | 4194323 | — | Chat | fixed struct |
| SMSG_CHAT_RECONNECT | 0x400014 | 4194324 | — | Chat | fixed struct |
| SMSG_CHANNEL_NOTIFY | 0x400015 | 4194325 | 2085 (0x825) | Chat | variable |
| SMSG_CHANNEL_NOTIFY_NPE_JOINED_BATCH | 0x400016 | 4194326 | — | Chat | fixed struct |
| SMSG_CHANNEL_NOTIFY_JOINED | 0x400017 | 4194327 | — | Chat | variable |
| SMSG_CHANNEL_NOTIFY_LEFT | 0x400018 | 4194328 | — | Chat | variable |
| SMSG_CHANNEL_LIST | 0x400019 | 4194329 | 8724 (0x2214) | Chat | variable |
| SMSG_CHAT_SERVER_MESSAGE | 0x40001a | 4194330 | 27652 (0x6c04) | Chat | variable |
| — | 0x40001b | 4194331 | — | Chat | ignored by the client |
| — | 0x40001c | 4194332 | — | Chat | ignored by the client |
| — | 0x40001d | 4194333 | — | Chat | ignored by the client |
| — | 0x40001e | 4194334 | — | Chat | ignored by the client |
| — | 0x40001f | 4194335 | — | Chat | ignored by the client |
| — | 0x400020 | 4194336 | — | Chat | ignored by the client |
| SMSG_CHAT_NOT_IN_GUILD | 0x400021 | 4194337 | — | Chat | fixed struct |
| SMSG_BATTLEFIELD_STATUS_NEED_CONFIRMATION | 0x410000 | 4259840 | 22944 (0x59a0) | Combat | variable |
| SMSG_BATTLEFIELD_STATUS_ACTIVE | 0x410001 | 4259841 | 29860 (0x74a4) | Combat | variable |
| SMSG_BATTLEFIELD_STATUS_QUEUED | 0x410002 | 4259842 | 13729 (0x35a1) | Combat | variable |
| SMSG_BATTLEFIELD_STATUS_NONE | 0x410003 | 4259843 | — | Combat | fixed, 17 bytes |
| SMSG_BATTLEFIELD_STATUS_FAILED | 0x410004 | 4259844 | 29095 (0x71a7) | Combat | fixed, 29 bytes |
| SMSG_BATTLEFIELD_LIST | 0x410005 | 4259845 | 29109 (0x71b5) | Combat | variable |
| SMSG_BATTLEGROUND_PLAYER_POSITIONS | 0x410006 | 4259846 | — | Combat | variable |
| — | 0x410007 | 4259847 | — | Combat | ignored by the client |
| — | 0x410008 | 4259848 | — | Combat | ignored by the client |
| SMSG_BATTLEGROUND_PLAYER_JOINED | 0x410009 | 4259849 | 20656 (0x50b0) | Combat | fixed, 0 bytes |
| SMSG_BATTLEGROUND_PLAYER_LEFT | 0x41000a | 4259850 | 22950 (0x59a6) | Combat | fixed, 0 bytes |
| SMSG_BATTLEFIELD_PORT_DENIED | 0x41000b | 4259851 | 13731 (0x35a3) | Combat | fixed struct |
| SMSG_BATTLEGROUND_INFO_THROTTLED | 0x41000c | 4259852 | 13490 (0x34b2) | Combat | fixed struct |
| SMSG_BATTLEFIELD_STATUS_WAIT_FOR_GROUPS | 0x41000d | 4259853 | 30114 (0x75a2) | Combat | variable |
| — | 0x41000e | 4259854 | — | Combat | ignored by the client |
| SMSG_RATED_PVP_INFO | 0x41000f | 4259855 | — | Combat | variable |
| — | 0x410010 | 4259856 | — | Combat | ignored by the client |
| SMSG_INSPECT_HONOR_STATS | 0x410011 | 4259857 | 31141 (0x79a5) | Combat | fixed, 42 bytes |
| SMSG_PVP_LOG_DATA | 0x410012 | 4259858 | 23730 (0x5cb2) | Combat | variable |
| SMSG_WARGAME_REQUEST_SUCCESSFULLY_SENT_TO_OPPONENT | 0x410013 | 4259859 | — | Combat | fixed, 0 bytes |
| — | 0x410014 | 4259860 | — | Combat | ignored by the client |
| SMSG_WARGAME_REQUEST_OPPONENT_RESPONSE | 0x410015 | 4259861 | — | Combat | fixed, 1 bytes |
| SMSG_PVP_OPTIONS_ENABLED | 0x410016 | 4259862 | 20641 (0x50a1) | Combat | fixed, 2 bytes |
| SMSG_REQUEST_PVP_REWARDS_RESPONSE | 0x410017 | 4259863 | 23972 (0x5da4) | Combat | variable |
| SMSG_REQUEST_SCHEDULED_PVP_INFO_RESPONSE | 0x410018 | 4259864 | — | Combat | variable |
| — | 0x410019 | 4259865 | — | Combat | ignored by the client |
| SMSG_BREAK_TARGET | 0x41001a | 4259866 | 261 (0x105) | Combat | fixed, 0 bytes |
| SMSG_ATTACK_START | 0x41001b | 4259867 | 11541 (0x2d15) | Combat | fixed, 0 bytes |
| SMSG_ATTACK_STOP | 0x41001c | 4259868 | 2356 (0x934) | Combat | fixed, 1 bytes |
| SMSG_COMBAT_EVENT_FAILED | 0x41001d | 4259869 | 11015 (0x2b07) | Combat | fixed, 0 bytes |
| SMSG_DUEL_REQUESTED | 0x41001e | 4259870 | 17668 (0x4504) | Combat | fixed, 1 bytes |
| SMSG_DUEL_ARRANGED | 0x41001f | 4259871 | — | Combat | fixed, 0 bytes |
| SMSG_DUEL_OUT_OF_BOUNDS | 0x410020 | 4259872 | 3110 (0xc26) | Combat | fixed struct |
| SMSG_DUEL_IN_BOUNDS | 0x410021 | 4259873 | 2599 (0xa27) | Combat | fixed struct |
| SMSG_DUEL_COUNTDOWN | 0x410022 | 4259874 | 18486 (0x4836) | Combat | fixed struct |
| SMSG_DUEL_COMPLETE | 0x410023 | 4259875 | 9511 (0x2527) | Combat | fixed, 1 bytes |
| SMSG_DUEL_WINNER | 0x410024 | 4259876 | 11574 (0x2d36) | Combat | variable |
| SMSG_CAN_DUEL_RESULT | 0x410025 | 4259877 | — | Combat | fixed, 1 bytes |
| SMSG_CLEAR_TARGET | 0x410026 | 4259878 | 19238 (0x4b26) | Combat | fixed, 0 bytes |
| SMSG_RESET_RANGED_COMBAT_TIMER | 0x410027 | 4259879 | — | Combat | fixed struct |
| SMSG_PVP_CREDIT | 0x410028 | 4259880 | 24597 (0x6015) | Combat | fixed, 13 bytes |
| SMSG_CANCEL_COMBAT | 0x410029 | 4259881 | 20228 (0x4f04) | Combat | fixed struct |
| SMSG_ATTACK_SWING_ERROR | 0x41002a | 4259882 | — | Combat | variable |
| SMSG_ATTACK_SWING_LANDED_LOG | 0x41002b | 4259883 | — | Combat | variable |
| SMSG_BATTLEGROUND_POINTS | 0x41002c | 4259884 | — | Combat | fixed, 3 bytes |
| SMSG_BATTLEGROUND_INIT | 0x41002d | 4259885 | — | Combat | fixed struct |
| SMSG_MAP_OBJECTIVES_INIT | 0x41002e | 4259886 | — | Combat | variable |
| SMSG_BOSS_KILL | 0x41002f | 4259887 | — | Combat | fixed struct |
| SMSG_ATTACKER_STATE_UPDATE | 0x410030 | 4259888 | 2853 (0xb25) | Combat | variable |
| SMSG_PVP_MATCH_START | 0x410031 | 4259889 | — | Combat | variable |
| — | 0x410032 | 4259890 | — | Combat | ignored by the client |
| — | 0x410033 | 4259891 | — | Combat | ignored by the client |
| SMSG_PVP_MATCH_INITIALIZE | 0x410034 | 4259892 | — | Combat | variable |
| — | 0x430000 | 4390912 | — | Debug | ignored by the client |
| — | 0x430001 | 4390913 | — | Debug | ignored by the client |
| — | 0x430002 | 4390914 | — | Debug | ignored by the client |
| — | 0x430003 | 4390915 | — | Debug | ignored by the client |
| — | 0x430004 | 4390916 | — | Debug | ignored by the client |
| — | 0x430005 | 4390917 | — | Debug | ignored by the client |
| — | 0x430006 | 4390918 | — | Debug | ignored by the client |
| — | 0x430007 | 4390919 | — | Debug | ignored by the client |
| — | 0x430008 | 4390920 | — | Debug | ignored by the client |
| — | 0x430009 | 4390921 | — | Debug | ignored by the client |
| — | 0x43000a | 4390922 | — | Debug | ignored by the client |
| — | 0x43000b | 4390923 | — | Debug | ignored by the client |
| — | 0x450000 | 4521984 | — | Garrison | ignored by the client |
| — | 0x450001 | 4521985 | — | Garrison | ignored by the client |
| — | 0x450002 | 4521986 | — | Garrison | ignored by the client |
| — | 0x450003 | 4521987 | — | Garrison | ignored by the client |
| — | 0x450004 | 4521988 | — | Garrison | ignored by the client |
| — | 0x450005 | 4521989 | — | Garrison | ignored by the client |
| — | 0x450006 | 4521990 | — | Garrison | ignored by the client |
| — | 0x450007 | 4521991 | — | Garrison | ignored by the client |
| — | 0x450008 | 4521992 | — | Garrison | ignored by the client |
| — | 0x450009 | 4521993 | — | Garrison | ignored by the client |
| — | 0x45000a | 4521994 | — | Garrison | ignored by the client |
| — | 0x45000b | 4521995 | — | Garrison | ignored by the client |
| — | 0x45000c | 4521996 | — | Garrison | ignored by the client |
| — | 0x45000d | 4521997 | — | Garrison | ignored by the client |
| — | 0x45000e | 4521998 | — | Garrison | ignored by the client |
| — | 0x45000f | 4521999 | — | Garrison | ignored by the client |
| — | 0x450010 | 4522000 | — | Garrison | ignored by the client |
| — | 0x450011 | 4522001 | — | Garrison | ignored by the client |
| — | 0x450012 | 4522002 | — | Garrison | ignored by the client |
| — | 0x450013 | 4522003 | — | Garrison | ignored by the client |
| — | 0x450014 | 4522004 | — | Garrison | ignored by the client |
| — | 0x450015 | 4522005 | — | Garrison | ignored by the client |
| — | 0x450016 | 4522006 | — | Garrison | ignored by the client |
| — | 0x450017 | 4522007 | — | Garrison | ignored by the client |
| — | 0x450018 | 4522008 | — | Garrison | ignored by the client |
| — | 0x450019 | 4522009 | — | Garrison | ignored by the client |
| — | 0x45001a | 4522010 | — | Garrison | ignored by the client |
| — | 0x45001b | 4522011 | — | Garrison | ignored by the client |
| — | 0x45001c | 4522012 | — | Garrison | ignored by the client |
| — | 0x45001d | 4522013 | — | Garrison | ignored by the client |
| — | 0x45001e | 4522014 | — | Garrison | ignored by the client |
| — | 0x45001f | 4522015 | — | Garrison | ignored by the client |
| — | 0x450020 | 4522016 | — | Garrison | ignored by the client |
| — | 0x450021 | 4522017 | — | Garrison | ignored by the client |
| — | 0x450022 | 4522018 | — | Garrison | ignored by the client |
| — | 0x450023 | 4522019 | — | Garrison | ignored by the client |
| — | 0x450024 | 4522020 | — | Garrison | ignored by the client |
| — | 0x450025 | 4522021 | — | Garrison | ignored by the client |
| — | 0x450026 | 4522022 | — | Garrison | ignored by the client |
| — | 0x450027 | 4522023 | — | Garrison | ignored by the client |
| — | 0x450028 | 4522024 | — | Garrison | ignored by the client |
| — | 0x450029 | 4522025 | — | Garrison | ignored by the client |
| — | 0x45002a | 4522026 | — | Garrison | ignored by the client |
| — | 0x45002b | 4522027 | — | Garrison | ignored by the client |
| — | 0x45002c | 4522028 | — | Garrison | ignored by the client |
| — | 0x45002d | 4522029 | — | Garrison | ignored by the client |
| — | 0x45002e | 4522030 | — | Garrison | ignored by the client |
| — | 0x45002f | 4522031 | — | Garrison | ignored by the client |
| — | 0x450030 | 4522032 | — | Garrison | ignored by the client |
| — | 0x450031 | 4522033 | — | Garrison | ignored by the client |
| — | 0x450032 | 4522034 | — | Garrison | ignored by the client |
| — | 0x450033 | 4522035 | — | Garrison | ignored by the client |
| — | 0x450034 | 4522036 | — | Garrison | ignored by the client |
| — | 0x450035 | 4522037 | — | Garrison | ignored by the client |
| — | 0x450036 | 4522038 | — | Garrison | ignored by the client |
| — | 0x450037 | 4522039 | — | Garrison | ignored by the client |
| — | 0x450038 | 4522040 | — | Garrison | ignored by the client |
| — | 0x450039 | 4522041 | — | Garrison | ignored by the client |
| — | 0x45003a | 4522042 | — | Garrison | ignored by the client |
| — | 0x45003b | 4522043 | — | Garrison | ignored by the client |
| — | 0x45003c | 4522044 | — | Garrison | ignored by the client |
| — | 0x45003d | 4522045 | — | Garrison | ignored by the client |
| — | 0x45003e | 4522046 | — | Garrison | ignored by the client |
| — | 0x45003f | 4522047 | — | Garrison | ignored by the client |
| — | 0x450040 | 4522048 | — | Garrison | ignored by the client |
| — | 0x450041 | 4522049 | — | Garrison | ignored by the client |
| — | 0x450042 | 4522050 | — | Garrison | ignored by the client |
| — | 0x450043 | 4522051 | — | Garrison | ignored by the client |
| — | 0x450044 | 4522052 | — | Garrison | ignored by the client |
| — | 0x450045 | 4522053 | — | Garrison | ignored by the client |
| — | 0x450046 | 4522054 | — | Garrison | ignored by the client |
| — | 0x450047 | 4522055 | — | Garrison | ignored by the client |
| — | 0x450048 | 4522056 | — | Garrison | ignored by the client |
| — | 0x450049 | 4522057 | — | Garrison | ignored by the client |
| — | 0x45004a | 4522058 | — | Garrison | ignored by the client |
| — | 0x45004b | 4522059 | — | Garrison | ignored by the client |
| — | 0x45004c | 4522060 | — | Garrison | ignored by the client |
| — | 0x45004d | 4522061 | — | Garrison | ignored by the client |
| — | 0x45004e | 4522062 | — | Garrison | ignored by the client |
| — | 0x45004f | 4522063 | — | Garrison | ignored by the client |
| — | 0x450050 | 4522064 | — | Garrison | ignored by the client |
| — | 0x450051 | 4522065 | — | Garrison | ignored by the client |
| SMSG_ALL_GUILD_ACHIEVEMENTS | 0x470000 | 4653056 | 21687 (0x54b7) | Guild | variable |
| SMSG_GUILD_SEND_RANK_CHANGE | 0x470001 | 4653057 | 23968 (0x5da0) | Guild | fixed, 5 bytes |
| SMSG_GUILD_COMMAND_RESULT | 0x470002 | 4653058 | 32179 (0x7db3) | Guild | variable |
| SMSG_GUILD_ROSTER | 0x470003 | 4653059 | 15779 (0x3da3) | Guild | variable |
| SMSG_GUILD_HARDCORE_MEMBER_DEATH | 0x470004 | 4653060 | — | Guild | variable |
| SMSG_GUILD_MEMBER_RECIPES | 0x470005 | 4653061 | 7344 (0x1cb0) | Guild | variable |
| SMSG_GUILD_KNOWN_RECIPES | 0x470006 | 4653062 | 4275 (0x10b3) | Guild | variable |
| SMSG_GUILD_MEMBERS_WITH_RECIPE | 0x470007 | 4653063 | — | Guild | variable |
| SMSG_GUILD_REWARD_LIST | 0x470008 | 4653064 | 7600 (0x1db0) | Guild | variable |
| SMSG_GUILD_NEWS | 0x470009 | 4653065 | — | Guild | variable |
| SMSG_GUILD_NEWS_DELETED | 0x47000a | 4653066 | 29863 (0x74a7) | Guild | fixed struct |
| SMSG_GUILD_CRITERIA_UPDATE | 0x47000b | 4653067 | — | Guild | variable |
| SMSG_GUILD_ACHIEVEMENT_EARNED | 0x47000c | 4653068 | 20661 (0x50b5) | Guild | fixed, 8 bytes |
| SMSG_GUILD_ACHIEVEMENT_DELETED | 0x47000d | 4653069 | 13728 (0x35a0) | Guild | fixed, 8 bytes |
| SMSG_GUILD_CRITERIA_DELETED | 0x47000e | 4653070 | 21937 (0x55b1) | Guild | fixed, 4 bytes |
| SMSG_GUILD_ACHIEVEMENT_MEMBERS | 0x47000f | 4653071 | 14501 (0x38a5) | Guild | variable |
| SMSG_GUILD_RANKS | 0x470010 | 4653072 | 12468 (0x30b4) | Guild | variable |
| SMSG_GUILD_MEMBER_UPDATE_NOTE | 0x470011 | 4653073 | 31904 (0x7ca0) | Guild | variable |
| SMSG_GUILD_INVITE | 0x470012 | 4653074 | 5282 (0x14a2) | Guild | variable |
| SMSG_GUILD_PARTY_STATE | 0x470013 | 4653075 | 20646 (0x50a6) | Guild | fixed, 13 bytes |
| SMSG_GUILD_REPUTATION_REACTION_CHANGED | 0x470014 | 4653076 | 29872 (0x74b0) | Guild | fixed, 0 bytes |
| — | 0x470015 | 4653077 | — | Guild | ignored by the client |
| — | 0x470016 | 4653078 | — | Guild | ignored by the client |
| — | 0x470017 | 4653079 | — | Guild | ignored by the client |
| — | 0x470018 | 4653080 | — | Guild | ignored by the client |
| — | 0x470019 | 4653081 | — | Guild | ignored by the client |
| SMSG_GUILD_CHALLENGE_UPDATE | 0x47001a | 4653082 | 6321 (0x18b1) | Guild | variable |
| SMSG_GUILD_CHALLENGE_COMPLETED | 0x47001b | 4653083 | 14755 (0x39a3) | Guild | fixed struct |
| SMSG_GUILD_ITEM_LOOTED_NOTIFY | 0x47001c | 4653084 | — | Guild | variable |
| — | 0x47001d | 4653085 | — | Guild | ignored by the client |
| — | 0x47001e | 4653086 | — | Guild | ignored by the client |
| — | 0x47001f | 4653087 | — | Guild | ignored by the client |
| SMSG_GUILD_RESET | 0x470020 | 4653088 | 7349 (0x1cb5) | Guild | fixed, 0 bytes |
| SMSG_GUILD_MOVE_STARTING | 0x470021 | 4653089 | 28836 (0x70a4) | Guild | fixed, 0 bytes |
| SMSG_GUILD_MOVED | 0x470022 | 4653090 | — | Guild | variable |
| SMSG_GUILD_NAME_CHANGED | 0x470023 | 4653091 | — | Guild | variable |
| SMSG_GUILD_FLAGGED_FOR_RENAME | 0x470024 | 4653092 | 12470 (0x30b6) | Guild | fixed, 1 bytes |
| SMSG_GUILD_CHANGE_NAME_RESULT | 0x470025 | 4653093 | 15537 (0x3cb1) | Guild | fixed, 1 bytes |
| SMSG_GUILD_BANK_QUERY_RESULTS | 0x470026 | 4653094 | 30885 (0x78a5) | Guild | variable |
| SMSG_GUILD_BANK_LOG_QUERY_RESULTS | 0x470027 | 4653095 | 12466 (0x30b2) | Guild | variable |
| SMSG_GUILD_BANK_REMAINING_WITHDRAW_MONEY | 0x470028 | 4653096 | 23988 (0x5db4) | Guild | fixed struct |
| SMSG_GUILD_PERMISSIONS_QUERY_RESULTS | 0x470029 | 4653097 | 13475 (0x34a3) | Guild | variable |
| SMSG_GUILD_EVENT_LOG_QUERY_RESULTS | 0x47002a | 4653098 | 4274 (0x10b2) | Guild | variable |
| SMSG_GUILD_BANK_TEXT_QUERY_RESULT | 0x47002b | 4653099 | 30115 (0x75a3) | Guild | variable |
| SMSG_GUILD_MEMBER_DAILY_RESET | 0x47002c | 4653100 | 4261 (0x10a5) | Guild | fixed struct |
| SMSG_QUERY_GUILD_INFO_RESPONSE | 0x47002d | 4653101 | 3590 (0xe06) | Guild | variable |
| — | 0x47002e | 4653102 | — | Guild | ignored by the client |
| — | 0x47002f | 4653103 | — | Guild | ignored by the client |
| SMSG_GUILD_INVITE_DECLINED | 0x470030 | 4653104 | — | Guild | variable |
| SMSG_GUILD_INVITE_EXPIRED | 0x470031 | 4653105 | — | Guild | fixed struct |
| SMSG_GUILD_EVENT_PLAYER_JOINED | 0x470032 | 4653106 | — | Guild | variable |
| SMSG_GUILD_EVENT_PLAYER_LEFT | 0x470033 | 4653107 | — | Guild | variable |
| SMSG_GUILD_EVENT_NEW_LEADER | 0x470034 | 4653108 | — | Guild | variable |
| SMSG_GUILD_EVENT_DISBANDED | 0x470035 | 4653109 | — | Guild | fixed struct |
| SMSG_GUILD_EVENT_MOTD | 0x470036 | 4653110 | — | Guild | variable |
| SMSG_GUILD_EVENT_PRESENCE_CHANGE | 0x470037 | 4653111 | — | Guild | variable |
| SMSG_GUILD_EVENT_STATUS_CHANGE | 0x470038 | 4653112 | — | Guild | fixed, 1 bytes |
| SMSG_GUILD_EVENT_RANKS_UPDATED | 0x470039 | 4653113 | — | Guild | fixed struct |
| SMSG_GUILD_EVENT_RANK_CHANGED | 0x47003a | 4653114 | — | Guild | fixed struct |
| SMSG_GUILD_EVENT_TAB_ADDED | 0x47003b | 4653115 | — | Guild | fixed struct |
| SMSG_GUILD_EVENT_TAB_DELETED | 0x47003c | 4653116 | — | Guild | fixed struct |
| SMSG_GUILD_EVENT_TAB_MODIFIED | 0x47003d | 4653117 | — | Guild | variable |
| SMSG_GUILD_EVENT_TAB_TEXT_CHANGED | 0x47003e | 4653118 | — | Guild | fixed struct |
| SMSG_GUILD_EVENT_BANK_MONEY_CHANGED | 0x47003f | 4653119 | — | Guild | fixed struct |
| SMSG_GUILD_EVENT_BANK_CONTENTS_CHANGED | 0x470040 | 4653120 | — | Guild | fixed struct |
| SMSG_PLAYER_SAVE_GUILD_EMBLEM | 0x470041 | 4653121 | — | Guild | fixed struct |
| SMSG_PETITION_RENAME_GUILD_RESPONSE | 0x470042 | 4653122 | — | Guild | variable |
| SMSG_LFG_JOIN_RESULT | 0x490000 | 4784128 | 14518 (0x38b6) | LFG | variable |
| SMSG_LFG_LIST_JOIN_RESULT | 0x490001 | 4784129 | — | LFG | fixed, 19 bytes |
| SMSG_LFG_LIST_SEARCH_RESULTS | 0x490002 | 4784130 | — | LFG | variable |
| SMSG_LFG_LIST_SEARCH_STATUS | 0x490003 | 4784131 | — | LFG | fixed, 19 bytes |
| SMSG_LFG_QUEUE_STATUS | 0x490004 | 4784132 | 30900 (0x78b4) | LFG | variable |
| SMSG_LFG_ROLE_CHECK_UPDATE | 0x490005 | 4784133 | 822 (0x336) | LFG | variable |
| SMSG_LFG_READY_CHECK_UPDATE | 0x490006 | 4784134 | — | LFG | variable |
| — | 0x490007 | 4784135 | — | LFG | ignored by the client |
| SMSG_LFG_UPDATE_STATUS | 0x490008 | 4784136 | 12708 (0x31a4) | LFG | variable |
| SMSG_LFG_INSTANCE_SHUTDOWN_COUNTDOWN | 0x490009 | 4784137 | — | LFG | fixed, 21 bytes |
| SMSG_LFG_LIST_UPDATE_STATUS | 0x49000a | 4784138 | — | LFG | variable |
| SMSG_LFG_LIST_UPDATE_EXPIRATION | 0x49000b | 4784139 | — | LFG | fixed, 26 bytes |
| SMSG_LFG_LIST_APPLICATION_STATUS_UPDATE | 0x49000c | 4784140 | — | LFG | fixed, 45 bytes |
| SMSG_LFG_LIST_APPLY_TO_GROUP_RESULT | 0x49000d | 4784141 | — | LFG | variable |
| SMSG_LFG_LIST_UPDATE_BLACKLIST | 0x49000e | 4784142 | — | LFG | variable |
| SMSG_LFG_LIST_APPLICANT_LIST_UPDATE | 0x49000f | 4784143 | — | LFG | variable |
| SMSG_LFG_LIST_SEARCH_RESULTS_UPDATE | 0x490010 | 4784144 | — | LFG | variable |
| SMSG_LFG_PROPOSAL_UPDATE | 0x490011 | 4784145 | 32166 (0x7da6) | LFG | variable |
| SMSG_SET_DF_FAST_LAUNCH_RESULT | 0x490012 | 4784146 | 13750 (0x35b6) | LFG | fixed, 1 bytes |
| — | 0x490013 | 4784147 | — | LFG | ignored by the client |
| SMSG_LFG_SLOT_INVALID | 0x490014 | 4784148 | 21685 (0x54b5) | LFG | fixed struct |
| SMSG_OPEN_LFG_DUNGEON_FINDER | 0x490015 | 4784149 | 11319 (0x2c37) | LFG | fixed struct |
| SMSG_LFG_TELEPORT_DENIED | 0x490016 | 4784150 | 3604 (0xe14) | LFG | fixed, 1 bytes |
| SMSG_LFG_DISABLED | 0x490017 | 4784151 | 2069 (0x815) | LFG | fixed struct |
| SMSG_LFG_OFFER_CONTINUE | 0x490018 | 4784152 | 27431 (0x6b27) | LFG | fixed struct |
| SMSG_LFG_BOOT_PLAYER | 0x490019 | 4784153 | — | LFG | variable |
| SMSG_LFG_PARTY_INFO | 0x49001a | 4784154 | 8997 (0x2325) | LFG | variable |
| SMSG_LFG_PLAYER_INFO | 0x49001b | 4784155 | 19254 (0x4b36) | LFG | variable |
| SMSG_LFG_PLAYER_REWARD | 0x49001c | 4784156 | 26676 (0x6834) | LFG | variable |
| SMSG_ROLE_CHOSEN | 0x49001d | 4784157 | — | LFG | fixed, 2 bytes |
| SMSG_LFG_READY_CHECK_RESULT | 0x49001e | 4784158 | — | LFG | fixed, 1 bytes |
| SMSG_LFG_EXPAND_SEARCH_PROMPT | 0x49001f | 4784159 | — | LFG | fixed, 17 bytes |
| SMSG_LFG_JOIN_LOBBY_MATCHMAKER_QUEUE | 0x490020 | 4784160 | — | LFG | fixed, 4 bytes |
| SMSG_UPDATE_OBJECT | 0x4b0000 | 4915200 | 18197 (0x4715) | Object update | variable |
| SMSG_TIME_SYNC_REQUEST | 0x4c0000 | 4980736 | 15524 (0x3ca4) | Movement | fixed struct |
| SMSG_TIME_ADJUSTMENT | 0x4c0001 | 4980737 | 31159 (0x79b7) | Movement | fixed struct |
| SMSG_ON_MONSTER_MOVE | 0x4c0002 | 4980738 | 28183 (0x6e17) | Movement | variable |
| SMSG_MOVE_SET_ACTIVE_MOVER | 0x4c0003 | 4980739 | 4531 (0x11b3) | Movement | fixed, 0 bytes |
| SMSG_MOVE_UPDATE_RUN_SPEED | 0x4c0004 | 4980740 | 5286 (0x14a6) | Movement | variable |
| SMSG_MOVE_UPDATE_RUN_BACK_SPEED | 0x4c0005 | 4980741 | 15782 (0x3da6) | Movement | variable |
| SMSG_MOVE_UPDATE_WALK_SPEED | 0x4c0006 | 4980742 | 21666 (0x54a2) | Movement | variable |
| SMSG_MOVE_UPDATE_SWIM_SPEED | 0x4c0007 | 4980743 | 22965 (0x59b5) | Movement | variable |
| SMSG_MOVE_UPDATE_SWIM_BACK_SPEED | 0x4c0008 | 4980744 | 12469 (0x30b5) | Movement | variable |
| SMSG_MOVE_UPDATE_FLIGHT_SPEED | 0x4c0009 | 4980745 | 12465 (0x30b1) | Movement | variable |
| SMSG_MOVE_UPDATE_FLIGHT_BACK_SPEED | 0x4c000a | 4980746 | 29856 (0x74a0) | Movement | variable |
| SMSG_MOVE_UPDATE_TURN_RATE | 0x4c000b | 4980747 | 23969 (0x5da1) | Movement | variable |
| SMSG_MOVE_UPDATE_PITCH_RATE | 0x4c000c | 4980748 | 7605 (0x1db5) | Movement | variable |
| SMSG_MOVE_UPDATE_COLLISION_HEIGHT | 0x4c000d | 4980749 | 22947 (0x59a3) | Movement | variable |
| SMSG_MOVE_UPDATE | 0x4c000e | 4980750 | 31138 (0x79a2) | Movement | variable |
| SMSG_MOVE_UPDATE_TELEPORT | 0x4c000f | 4980751 | 20658 (0x50b2) | Movement | variable |
| SMSG_MOVE_UPDATE_KNOCK_BACK | 0x4c0010 | 4980752 | 15794 (0x3db2) | Movement | variable |
| SMSG_MOVE_UPDATE_MOD_MOVEMENT_FORCE_MAGNITUDE | 0x4c0011 | 4980753 | — | Movement | variable |
| SMSG_MOVE_UPDATE_APPLY_MOVEMENT_FORCE | 0x4c0012 | 4980754 | — | Movement | variable |
| SMSG_MOVE_UPDATE_REMOVE_MOVEMENT_FORCE | 0x4c0013 | 4980755 | — | Movement | variable |
| SMSG_MOVE_SET_MOD_MOVEMENT_FORCE_MAGNITUDE | 0x4c0014 | 4980756 | — | Movement | fixed, 8 bytes |
| SMSG_MOVE_SPLINE_SET_RUN_SPEED | 0x4c0015 | 4980757 | 20919 (0x51b7) | Movement | fixed, 4 bytes |
| SMSG_MOVE_SPLINE_SET_RUN_BACK_SPEED | 0x4c0016 | 4980758 | 15795 (0x3db3) | Movement | fixed, 4 bytes |
| SMSG_MOVE_SPLINE_SET_SWIM_SPEED | 0x4c0017 | 4980759 | 14756 (0x39a4) | Movement | fixed, 4 bytes |
| SMSG_MOVE_SPLINE_SET_SWIM_BACK_SPEED | 0x4c0018 | 4980760 | 22945 (0x59a1) | Movement | fixed, 4 bytes |
| SMSG_MOVE_SPLINE_SET_FLIGHT_SPEED | 0x4c0019 | 4980761 | 14752 (0x39a0) | Movement | fixed, 4 bytes |
| SMSG_MOVE_SPLINE_SET_FLIGHT_BACK_SPEED | 0x4c001a | 4980762 | 14515 (0x38b3) | Movement | fixed, 4 bytes |
| SMSG_MOVE_SPLINE_SET_WALK_SPEED | 0x4c001b | 4980763 | — | Movement | fixed, 4 bytes |
| SMSG_MOVE_SPLINE_SET_TURN_RATE | 0x4c001c | 4980764 | 30901 (0x78b5) | Movement | fixed, 4 bytes |
| SMSG_MOVE_SPLINE_SET_PITCH_RATE | 0x4c001d | 4980765 | 5296 (0x14b0) | Movement | fixed, 4 bytes |
| SMSG_MOVE_SET_RUN_SPEED | 0x4c001e | 4980766 | 15797 (0x3db5) | Movement | fixed, 8 bytes |
| SMSG_MOVE_SET_RUN_BACK_SPEED | 0x4c001f | 4980767 | 29105 (0x71b1) | Movement | fixed, 8 bytes |
| SMSG_MOVE_SET_SWIM_SPEED | 0x4c0020 | 4980768 | 5543 (0x15a7) | Movement | fixed, 8 bytes |
| SMSG_MOVE_SET_SWIM_BACK_SPEED | 0x4c0021 | 4980769 | 23718 (0x5ca6) | Movement | fixed, 8 bytes |
| SMSG_MOVE_SET_FLIGHT_SPEED | 0x4c0022 | 4980770 | 29094 (0x71a6) | Movement | fixed, 8 bytes |
| SMSG_MOVE_SET_FLIGHT_BACK_SPEED | 0x4c0023 | 4980771 | 12450 (0x30a2) | Movement | fixed, 8 bytes |
| SMSG_MOVE_SET_WALK_SPEED | 0x4c0024 | 4980772 | 7588 (0x1da4) | Movement | fixed, 8 bytes |
| SMSG_MOVE_SET_TURN_RATE | 0x4c0025 | 4980773 | 12453 (0x30a5) | Movement | fixed, 8 bytes |
| SMSG_MOVE_SET_PITCH_RATE | 0x4c0026 | 4980774 | 30128 (0x75b0) | Movement | fixed, 8 bytes |
| SMSG_MOVE_ROOT | 0x4c0027 | 4980775 | 32160 (0x7da0) | Movement | fixed, 4 bytes |
| SMSG_MOVE_UNROOT | 0x4c0028 | 4980776 | 32180 (0x7db4) | Movement | fixed, 4 bytes |
| SMSG_MOVE_SET_WATER_WALK | 0x4c0029 | 4980777 | 30129 (0x75b1) | Movement | fixed, 4 bytes |
| SMSG_MOVE_ENABLE_DOUBLE_JUMP | 0x4c002a | 4980778 | — | Movement | fixed, 4 bytes |
| SMSG_MOVE_DISABLE_DOUBLE_JUMP | 0x4c002b | 4980779 | — | Movement | fixed, 4 bytes |
| SMSG_MOVE_SET_LAND_WALK | 0x4c002c | 4980780 | 13495 (0x34b7) | Movement | fixed, 4 bytes |
| SMSG_MOVE_SET_FEATHER_FALL | 0x4c002d | 4980781 | 31152 (0x79b0) | Movement | fixed, 4 bytes |
| SMSG_MOVE_SET_NORMAL_FALL | 0x4c002e | 4980782 | 20918 (0x51b6) | Movement | fixed, 4 bytes |
| SMSG_MOVE_SET_HOVERING | 0x4c002f | 4980783 | 23731 (0x5cb3) | Movement | fixed, 4 bytes |
| SMSG_MOVE_UNSET_HOVERING | 0x4c0030 | 4980784 | 20915 (0x51b3) | Movement | fixed, 4 bytes |
| SMSG_MOVE_KNOCK_BACK | 0x4c0031 | 4980785 | 23732 (0x5cb4) | Movement | fixed, 20 bytes |
| SMSG_MOVE_TELEPORT | 0x4c0032 | 4980786 | — | Movement | variable |
| SMSG_MOVE_SET_CAN_FLY | 0x4c0033 | 4980787 | 15777 (0x3da1) | Movement | fixed, 4 bytes |
| SMSG_MOVE_UNSET_CAN_FLY | 0x4c0034 | 4980788 | 5538 (0x15a2) | Movement | fixed, 4 bytes |
| — | 0x4c0035 | 4980789 | — | Movement | ignored by the client |
| — | 0x4c0036 | 4980790 | — | Movement | ignored by the client |
| SMSG_MOVE_SET_CAN_TURN_WHILE_FALLING | 0x4c0037 | 4980791 | — | Movement | fixed, 4 bytes |
| SMSG_MOVE_UNSET_CAN_TURN_WHILE_FALLING | 0x4c0038 | 4980792 | — | Movement | fixed, 4 bytes |
| SMSG_MOVE_SET_IGNORE_MOVEMENT_FORCES | 0x4c0039 | 4980793 | — | Movement | fixed, 4 bytes |
| SMSG_MOVE_UNSET_IGNORE_MOVEMENT_FORCES | 0x4c003a | 4980794 | — | Movement | fixed, 4 bytes |
| SMSG_MOVE_ENABLE_TRANSITION_BETWEEN_SWIM_AND_FLY | 0x4c003b | 4980795 | 22946 (0x59a2) | Movement | fixed, 4 bytes |
| SMSG_MOVE_DISABLE_TRANSITION_BETWEEN_SWIM_AND_FLY | 0x4c003c | 4980796 | 32178 (0x7db2) | Movement | fixed, 4 bytes |
| SMSG_MOVE_DISABLE_GRAVITY | 0x4c003d | 4980797 | 30130 (0x75b2) | Movement | fixed, 4 bytes |
| SMSG_MOVE_ENABLE_GRAVITY | 0x4c003e | 4980798 | 12467 (0x30b3) | Movement | fixed, 4 bytes |
| SMSG_MOVE_DISABLE_INERTIA | 0x4c003f | 4980799 | — | Movement | fixed, 4 bytes |
| SMSG_MOVE_ENABLE_INERTIA | 0x4c0040 | 4980800 | — | Movement | fixed, 4 bytes |
| SMSG_MOVE_DISABLE_COLLISION | 0x4c0041 | 4980801 | 12720 (0x31b0) | Movement | fixed, 4 bytes |
| SMSG_MOVE_ENABLE_COLLISION | 0x4c0042 | 4980802 | 4519 (0x11a7) | Movement | fixed, 4 bytes |
| SMSG_MOVE_SET_COLLISION_HEIGHT | 0x4c0043 | 4980803 | 4528 (0x11b0) | Movement | fixed, 21 bytes |
| SMSG_MOVE_SET_VEHICLE_REC_ID | 0x4c0044 | 4980804 | — | Movement | fixed, 8 bytes |
| SMSG_MOVE_APPLY_MOVEMENT_FORCE | 0x4c0045 | 4980805 | — | Movement | fixed, 41 bytes |
| SMSG_MOVE_REMOVE_MOVEMENT_FORCE | 0x4c0046 | 4980806 | — | Movement | fixed, 4 bytes |
| SMSG_MOVE_SET_COMPOUND_STATE | 0x4c0047 | 4980807 | 30112 (0x75a0) | Movement | variable |
| SMSG_MOVE_SKIP_TIME | 0x4c0048 | 4980808 | — | Movement | fixed, 4 bytes |
| SMSG_MOVE_SPLINE_ROOT | 0x4c0049 | 4980809 | 20916 (0x51b4) | Movement | fixed, 0 bytes |
| SMSG_MOVE_SPLINE_UNROOT | 0x4c004a | 4980810 | 30134 (0x75b6) | Movement | fixed, 0 bytes |
| SMSG_MOVE_SPLINE_DISABLE_GRAVITY | 0x4c004b | 4980811 | 23989 (0x5db5) | Movement | fixed, 0 bytes |
| SMSG_MOVE_SPLINE_ENABLE_GRAVITY | 0x4c004c | 4980812 | 15526 (0x3ca6) | Movement | fixed, 0 bytes |
| SMSG_MOVE_SPLINE_DISABLE_COLLISION | 0x4c004d | 4980813 | 13745 (0x35b1) | Movement | fixed, 0 bytes |
| SMSG_MOVE_SPLINE_ENABLE_COLLISION | 0x4c004e | 4980814 | 15536 (0x3cb0) | Movement | fixed, 0 bytes |
| SMSG_MOVE_SPLINE_SET_FEATHER_FALL | 0x4c004f | 4980815 | 15781 (0x3da5) | Movement | fixed, 0 bytes |
| SMSG_MOVE_SPLINE_SET_NORMAL_FALL | 0x4c0050 | 4980816 | 14514 (0x38b2) | Movement | fixed, 0 bytes |
| SMSG_MOVE_SPLINE_SET_HOVER | 0x4c0051 | 4980817 | 5302 (0x14b6) | Movement | fixed, 0 bytes |
| SMSG_MOVE_SPLINE_UNSET_HOVER | 0x4c0052 | 4980818 | 32165 (0x7da5) | Movement | fixed, 0 bytes |
| SMSG_MOVE_SPLINE_SET_WATER_WALK | 0x4c0053 | 4980819 | 20642 (0x50a2) | Movement | fixed, 0 bytes |
| SMSG_MOVE_SPLINE_SET_LAND_WALK | 0x4c0054 | 4980820 | 15783 (0x3da7) | Movement | fixed, 0 bytes |
| SMSG_MOVE_SPLINE_START_SWIM | 0x4c0055 | 4980821 | 12709 (0x31a5) | Movement | fixed, 0 bytes |
| SMSG_MOVE_SPLINE_STOP_SWIM | 0x4c0056 | 4980822 | 7586 (0x1da2) | Movement | fixed, 0 bytes |
| SMSG_MOVE_SPLINE_SET_RUN_MODE | 0x4c0057 | 4980823 | 30119 (0x75a7) | Movement | fixed, 0 bytes |
| SMSG_MOVE_SPLINE_SET_WALK_MODE | 0x4c0058 | 4980824 | 21686 (0x54b6) | Movement | fixed, 0 bytes |
| SMSG_MOVE_SPLINE_SET_FLYING | 0x4c0059 | 4980825 | 12725 (0x31b5) | Movement | fixed, 0 bytes |
| SMSG_MOVE_SPLINE_UNSET_FLYING | 0x4c005a | 4980826 | 22694 (0x58a6) | Movement | fixed, 0 bytes |
| SMSG_FLIGHT_SPLINE_SYNC | 0x4c005b | 4980827 | 2340 (0x924) | Movement | fixed, 4 bytes |
| — | 0x4c005c | 4980828 | — | Movement | ignored by the client |
| — | 0x4c005d | 4980829 | — | Movement | ignored by the client |
| SMSG_MOVE_APPLY_INERTIA | 0x4c005e | 4980830 | — | Movement | fixed, 12 bytes |
| SMSG_MOVE_REMOVE_INERTIA | 0x4c005f | 4980831 | — | Movement | fixed, 8 bytes |
| SMSG_MOVE_UPDATE_APPLY_INERTIA | 0x4c0060 | 4980832 | — | Movement | variable |
| SMSG_MOVE_UPDATE_REMOVE_INERTIA | 0x4c0061 | 4980833 | — | Movement | variable |
| — | 0x4c0062 | 4980834 | — | Movement | ignored by the client |
| — | 0x4c0063 | 4980835 | — | Movement | ignored by the client |
| — | 0x4c0064 | 4980836 | — | Movement | ignored by the client |
| — | 0x4c0065 | 4980837 | — | Movement | ignored by the client |
| — | 0x4c0066 | 4980838 | — | Movement | ignored by the client |
| — | 0x4c0067 | 4980839 | — | Movement | ignored by the client |
| — | 0x4c0068 | 4980840 | — | Movement | ignored by the client |
| — | 0x4c0069 | 4980841 | — | Movement | ignored by the client |
| — | 0x4c006a | 4980842 | — | Movement | ignored by the client |
| — | 0x4c006b | 4980843 | — | Movement | ignored by the client |
| — | 0x4c006c | 4980844 | — | Movement | ignored by the client |
| — | 0x4c006d | 4980845 | — | Movement | ignored by the client |
| — | 0x4c006e | 4980846 | — | Movement | ignored by the client |
| — | 0x4c006f | 4980847 | — | Movement | ignored by the client |
| — | 0x4c0070 | 4980848 | — | Movement | ignored by the client |
| — | 0x4c0071 | 4980849 | — | Movement | ignored by the client |
| — | 0x4c0072 | 4980850 | — | Movement | ignored by the client |
| — | 0x4c0073 | 4980851 | — | Movement | fixed, 4 bytes |
| — | 0x4c0074 | 4980852 | — | Movement | ignored by the client |
| — | 0x4c0075 | 4980853 | — | Movement | ignored by the client |
| SMSG_PLAYER_BOUND | 0x4e0000 | 5111808 | 9494 (0x2516) | Player | fixed, 4 bytes |
| — | 0x4e0001 | 5111809 | — | Player | ignored by the client |
| SMSG_FAILED_PLAYER_CONDITION | 0x4e0002 | 5111810 | 6564 (0x19a4) | Player | fixed struct |
| SMSG_GM_REQUEST_PLAYER_INFO | 0x4e0003 | 5111811 | — | Player | variable |
| SMSG_DISPLAY_PLAYER_CHOICE | 0x4e0004 | 5111812 | — | Player | variable |
| SMSG_PLAYER_CHOICE_DISPLAY_ERROR | 0x4e0005 | 5111813 | — | Player | fixed struct |
| SMSG_PLAYER_CHOICE_CLEAR | 0x4e0006 | 5111814 | — | Player | fixed, 5 bytes |
| SMSG_INVALIDATE_PLAYER | 0x4e0007 | 5111815 | 25381 (0x6325) | Player | fixed, 0 bytes |
| — | 0x4e0008 | 5111816 | — | Player | ignored by the client |
| SMSG_REPORT_PVP_PLAYER_AFK_RESULT | 0x4e0009 | 5111817 | — | Player | fixed, 3 bytes |
| SMSG_QUERY_PLAYER_NAME_BY_COMMUNITY_ID_RESPONSE | 0x4e000a | 5111818 | — | Player | variable |
| SMSG_SET_PLAYER_DECLINED_NAMES_RESULT | 0x4e000b | 5111819 | 11045 (0x2b25) | Player | fixed, 4 bytes |
| SMSG_CHANGE_PLAYER_DIFFICULTY_RESULT | 0x4e000c | 5111820 | — | Player | variable |
| SMSG_GM_PLAYER_INFO | 0x4e000d | 5111821 | 18965 (0x4a15) | Player | variable |
| SMSG_PLAYER_SKINNED | 0x4e000e | 5111822 | 278 (0x116) | Player | fixed, 1 bytes |
| — | 0x4e000f | 5111823 | — | Player | ignored by the client |
| SMSG_VIGNETTE_UPDATE | 0x4e0010 | 5111824 | — | Player | variable |
| SMSG_PLAYER_IS_ADVENTURE_MAP_POI_VALID | 0x4e0011 | 5111825 | — | Player | fixed, 5 bytes |
| SMSG_PLAYER_CONDITION_RESULT | 0x4e0012 | 5111826 | — | Player | fixed struct |
| — | 0x4e0013 | 5111827 | — | Player | ignored by the client |
| SMSG_PLAYER_TUTORIAL_UNHIGHLIGHT_SPELL | 0x4e0014 | 5111828 | — | Player | fixed struct |
| SMSG_PLAYER_TUTORIAL_HIGHLIGHT_SPELL | 0x4e0015 | 5111829 | — | Player | variable |
| — | 0x4e0016 | 5111830 | — | Player | ignored by the client |
| SMSG_WORLD_QUEST_UPDATE_RESPONSE | 0x4e0017 | 5111831 | — | Player | variable |
| SMSG_AREA_POI_UPDATE_RESPONSE | 0x4e0018 | 5111832 | — | Player | variable |
| — | 0x4e0019 | 5111833 | — | Player | ignored by the client |
| — | 0x4e001a | 5111834 | — | Player | ignored by the client |
| — | 0x4e001b | 5111835 | — | Player | ignored by the client |
| — | 0x4e001c | 5111836 | — | Player | ignored by the client |
| — | 0x4e001d | 5111837 | — | Player | ignored by the client |
| — | 0x4e001e | 5111838 | — | Player | ignored by the client |
| — | 0x4e001f | 5111839 | — | Player | ignored by the client |
| SMSG_PLAYER_BONUS_ROLL_FAILED | 0x4e0020 | 5111840 | — | Player | fixed struct |
| — | 0x4e0021 | 5111841 | — | Player | ignored by the client |
| — | 0x4e0022 | 5111842 | — | Player | ignored by the client |
| SMSG_PLAYER_SHOW_UI_EVENT_TOAST | 0x4e0023 | 5111843 | — | Player | fixed struct |
| — | 0x4e0024 | 5111844 | — | Player | ignored by the client |
| SMSG_QUERY_PLAYER_NAMES_RESPONSE | 0x4e0025 | 5111845 | — | Player | variable |
| SMSG_PLAYER_BATTLEFIELD_AUTO_QUEUE | 0x4e0026 | 5111846 | — | Player | fixed, 5 bytes |
| SMSG_PLAYER_WORLD_PVP_QUEUE | 0x4e0027 | 5111847 | — | Player | fixed struct |
| SMSG_PLAYER_SHOW_GENERIC_WIDGET_DISPLAY | 0x4e0028 | 5111848 | — | Player | fixed struct |
| SMSG_PLAYER_SHOW_PARTY_POSE_UI | 0x4e0029 | 5111849 | — | Player | fixed, 5 bytes |
| SMSG_PLAYER_SHOW_ARROW_CALLOUT | 0x4e002a | 5111850 | — | Player | fixed struct |
| SMSG_PLAYER_HIDE_ARROW_CALLOUT | 0x4e002b | 5111851 | — | Player | fixed struct |
| SMSG_PLAYER_ACKNOWLEDGE_ARROW_CALLOUT | 0x4e002c | 5111852 | — | Player | fixed struct |
| — | 0x4e002d | 5111853 | — | Player | ignored by the client |
| SMSG_PLAYER_END_OF_MATCH_DETAILS | 0x4e002e | 5111854 | — | Player | fixed, 13 bytes |
| SMSG_DAILY_QUESTS_RESET | 0x4f0000 | 5177344 | — | Quest | fixed struct |
| SMSG_QUEST_COMPLETION_NPC_RESPONSE | 0x4f0001 | 5177345 | 30113 (0x75a1) | Quest | variable |
| SMSG_QUEST_ITEM_USABILITY_RESPONSE | 0x4f0002 | 5177346 | — | Quest | variable |
| SMSG_QUEST_GIVER_QUEST_COMPLETE | 0x4f0003 | 5177347 | 21924 (0x55a4) | Quest | variable |
| SMSG_IS_QUEST_COMPLETE_RESPONSE | 0x4f0004 | 5177348 | — | Quest | fixed, 5 bytes |
| SMSG_QUEST_GIVER_INVALID_QUEST | 0x4f0005 | 5177349 | 16406 (0x4016) | Quest | variable |
| SMSG_QUEST_GIVER_QUEST_FAILED | 0x4f0006 | 5177350 | 16950 (0x4236) | Quest | fixed struct |
| SMSG_QUEST_LOG_FULL | 0x4f0007 | 5177351 | 3638 (0xe36) | Quest | fixed struct |
| SMSG_QUEST_NON_LOG_UPDATE_COMPLETE | 0x4f0008 | 5177352 | — | Quest | fixed struct |
| SMSG_QUEST_UPDATE_COMPLETE | 0x4f0009 | 5177353 | 10551 (0x2937) | Quest | fixed struct |
| SMSG_QUEST_UPDATE_FAILED | 0x4f000a | 5177354 | 25380 (0x6324) | Quest | fixed struct |
| SMSG_QUEST_UPDATE_FAILED_TIMER | 0x4f000b | 5177355 | 25639 (0x6427) | Quest | fixed struct |
| SMSG_QUEST_UPDATE_ADD_CREDIT | 0x4f000c | 5177356 | — | Quest | fixed, 13 bytes |
| SMSG_QUEST_UPDATE_ADD_CREDIT_SIMPLE | 0x4f000d | 5177357 | — | Quest | fixed struct |
| SMSG_QUEST_UPDATE_ADD_PVP_CREDIT | 0x4f000e | 5177358 | 17430 (0x4416) | Quest | fixed struct |
| SMSG_QUEST_CONFIRM_ACCEPT | 0x4f000f | 5177359 | 28423 (0x6f07) | Quest | variable |
| SMSG_QUEST_PUSH_RESULT | 0x4f0010 | 5177360 | — | Quest | variable |
| SMSG_QUEST_GIVER_STATUS_MULTIPLE | 0x4f0011 | 5177361 | 20261 (0x4f25) | Quest | variable |
| SMSG_QUEST_GIVER_QUEST_DETAILS | 0x4f0012 | 5177362 | 9253 (0x2425) | Quest | variable |
| SMSG_QUEST_GIVER_REQUEST_ITEMS | 0x4f0013 | 5177363 | 25142 (0x6236) | Quest | variable |
| SMSG_QUEST_GIVER_OFFER_REWARD_MESSAGE | 0x4f0014 | 5177364 | 9255 (0x2427) | Quest | variable |
| SMSG_SHOW_QUEST_COMPLETION_TEXT | 0x4f0015 | 5177365 | — | Quest | variable |
| SMSG_QUERY_QUEST_INFO_RESPONSE | 0x4f0016 | 5177366 | 26934 (0x6936) | Quest | variable |
| SMSG_GOSSIP_COMPLETE | 0x4f0017 | 5177367 | 2054 (0x806) | Quest | fixed, 1 bytes |
| SMSG_GOSSIP_MESSAGE | 0x4f0018 | 5177368 | 8245 (0x2035) | Quest | variable |
| SMSG_GOSSIP_QUEST_UPDATE | 0x4f0019 | 5177369 | — | Quest | variable |
| SMSG_QUEST_GIVER_QUEST_LIST_MESSAGE | 0x4f001a | 5177370 | 308 (0x134) | Quest | variable |
| SMSG_QUEST_GIVER_STATUS | 0x4f001b | 5177371 | 8469 (0x2115) | Quest | fixed, 8 bytes |
| SMSG_QUEST_FORCE_REMOVED | 0x4f001c | 5177372 | 26117 (0x6605) | Quest | fixed struct |
| SMSG_QUEST_POI_QUERY_RESPONSE | 0x4f001d | 5177373 | 25348 (0x6304) | Quest | variable |
| SMSG_DISPLAY_QUEST_POPUP | 0x4f001e | 5177374 | — | Quest | fixed struct |
| SMSG_QUEST_POI_UPDATE_RESPONSE | 0x4f001f | 5177375 | — | Quest | variable |
| SMSG_RESET_QUEST_POI | 0x4f0020 | 5177376 | — | Quest | fixed struct |
| — | 0x4f0021 | 5177377 | — | Quest | ignored by the client |
| — | 0x4f0022 | 5177378 | — | Quest | ignored by the client |
| — | 0x4f0023 | 5177379 | — | Quest | ignored by the client |
| SMSG_COVENANT_CALLINGS_AVAILABILITY_RESPONSE | 0x4f0024 | 5177380 | — | Quest | variable |
| — | 0x4f0025 | 5177381 | — | Quest | ignored by the client |
| — | 0x4f0026 | 5177382 | — | Quest | ignored by the client |
| — | 0x4f0027 | 5177383 | — | Quest | ignored by the client |
| SMSG_GOSSIP_OPTION_NPC_INTERACTION | 0x4f0028 | 5177384 | — | Quest | variable |
| — | 0x510000 | 5308416 | — | Spell | ignored by the client |
| — | 0x510001 | 5308417 | — | Spell | ignored by the client |
| SMSG_CHEAT_IGNORE_DIMISHING_RETURNS | 0x510002 | 5308418 | — | Spell | fixed, 1 bytes |
| SMSG_MIRROR_IMAGE_CREATURE_DATA | 0x510003 | 5308419 | — | Spell | fixed, 8 bytes |
| SMSG_MIRROR_IMAGE_COMPONENTED_DATA | 0x510004 | 5308420 | 9780 (0x2634) | Spell | variable |
| SMSG_SPELL_COOLDOWN | 0x510005 | 5308421 | 19222 (0x4b16) | Spell | variable |
| SMSG_SPELL_CATEGORY_COOLDOWN | 0x510006 | 5308422 | — | Spell | fixed, 13 bytes |
| SMSG_SPELL_DISPELL_LOG | 0x510007 | 5308423 | 17686 (0x4516) | Spell | variable |
| SMSG_SPELL_PERIODIC_AURA_LOG | 0x510008 | 5308424 | 1046 (0x416) | Spell | variable |
| SMSG_SPELL_ENERGIZE_LOG | 0x510009 | 5308425 | 1044 (0x414) | Spell | variable |
| SMSG_SPELL_HEAL_LOG | 0x51000a | 5308426 | 10262 (0x2816) | Spell | variable |
| SMSG_SPELL_HEAL_ABSORB_LOG | 0x51000b | 5308427 | — | Spell | variable |
| SMSG_SPELL_ABSORB_LOG | 0x51000c | 5308428 | — | Spell | variable |
| SMSG_SPELL_INTERRUPT_LOG | 0x51000d | 5308429 | 7591 (0x1da7) | Spell | fixed, 8 bytes |
| SMSG_ENVIRONMENTAL_DAMAGE_LOG | 0x51000e | 5308430 | 27653 (0x6c05) | Spell | variable |
| — | 0x51000f | 5308431 | — | Spell | ignored by the client |
| — | 0x510010 | 5308432 | — | Spell | ignored by the client |
| SMSG_AURA_UPDATE | 0x510011 | 5308433 | 18183 (0x4707) | Spell | variable |
| SMSG_AURA_POINTS_DEPLETED | 0x510012 | 5308434 | 31927 (0x7cb7) | Spell | fixed, 2 bytes |
| SMSG_PET_CLEAR_SPELLS | 0x510013 | 5308435 | — | Spell | fixed struct |
| SMSG_PET_SPELLS_MESSAGE | 0x510014 | 5308436 | 16660 (0x4114) | Spell | variable |
| SMSG_CLEAR_COOLDOWNS | 0x510015 | 5308437 | 22964 (0x59b4) | Spell | variable |
| SMSG_CLEAR_ALL_SPELL_CHARGES | 0x510016 | 5308438 | — | Spell | fixed, 1 bytes |
| SMSG_CLEAR_SPELL_CHARGES | 0x510017 | 5308439 | — | Spell | fixed, 5 bytes |
| SMSG_SET_SPELL_CHARGES | 0x510018 | 5308440 | — | Spell | fixed, 14 bytes |
| SMSG_SEND_KNOWN_SPELLS | 0x510019 | 5308441 | 260 (0x104) | Spell | variable |
| SMSG_SEND_SPELL_HISTORY | 0x51001a | 5308442 | — | Spell | variable |
| SMSG_REFRESH_SPELL_HISTORY | 0x51001b | 5308443 | — | Spell | variable |
| SMSG_SEND_SPELL_CHARGES | 0x51001c | 5308444 | — | Spell | variable |
| SMSG_SEND_UNLEARN_SPELLS | 0x51001d | 5308445 | 20005 (0x4e25) | Spell | variable |
| SMSG_SPELL_OR_DAMAGE_IMMUNE | 0x51001e | 5308446 | 17671 (0x4507) | Spell | fixed, 5 bytes |
| SMSG_DISPEL_FAILED | 0x51001f | 5308447 | 775 (0x307) | Spell | variable |
| SMSG_SPELL_DAMAGE_SHIELD | 0x510020 | 5308448 | 10535 (0x2927) | Spell | variable |
| SMSG_SPELL_NON_MELEE_DAMAGE_LOG | 0x510021 | 5308449 | 17173 (0x4315) | Spell | variable |
| SMSG_SPELL_INSTAKILL_LOG | 0x510022 | 5308450 | 25110 (0x6216) | Spell | fixed, 4 bytes |
| SMSG_SPELL_CHANNEL_START | 0x510023 | 5308451 | — | Spell | variable |
| SMSG_SPELL_CHANNEL_UPDATE | 0x510024 | 5308452 | — | Spell | fixed, 4 bytes |
| SMSG_SET_FLAT_SPELL_MODIFIER | 0x510025 | 5308453 | 10292 (0x2834) | Spell | variable |
| SMSG_SET_PCT_SPELL_MODIFIER | 0x510026 | 5308454 | 548 (0x224) | Spell | variable |
| SMSG_SPELL_PREPARE | 0x510027 | 5308455 | — | Spell | fixed, 0 bytes |
| SMSG_SPELL_GO | 0x510028 | 5308456 | 28182 (0x6e16) | Spell | variable |
| SMSG_SPELL_START | 0x510029 | 5308457 | 25621 (0x6415) | Spell | variable |
| SMSG_RESUME_CAST | 0x51002a | 5308458 | — | Spell | fixed, 8 bytes |
| — | 0x51002b | 5308459 | — | Spell | ignored by the client |
| — | 0x51002c | 5308460 | — | Spell | ignored by the client |
| SMSG_RESUME_CAST_BAR | 0x51002d | 5308461 | — | Spell | variable |
| SMSG_SPELL_DELAYED | 0x51002e | 5308462 | 1813 (0x715) | Spell | fixed, 4 bytes |
| SMSG_SPELL_EXECUTE_LOG | 0x51002f | 5308463 | 1574 (0x626) | Spell | variable |
| SMSG_SPELL_MISS_LOG | 0x510030 | 5308464 | 1573 (0x625) | Spell | variable |
| — | 0x510031 | 5308465 | — | Spell | ignored by the client |
| SMSG_NOTIFY_DEST_LOC_SPELL_CAST | 0x510032 | 5308466 | 25092 (0x6204) | Spell | fixed, 45 bytes |
| SMSG_CANCEL_SPELL_VISUAL | 0x510033 | 5308467 | — | Spell | fixed, 4 bytes |
| SMSG_PLAY_SPELL_VISUAL | 0x510034 | 5308468 | 4273 (0x10b1) | Spell | fixed, 35 bytes |
| SMSG_CANCEL_ORPHAN_SPELL_VISUAL | 0x510035 | 5308469 | — | Spell | fixed struct |
| SMSG_PLAY_ORPHAN_SPELL_VISUAL | 0x510036 | 5308470 | — | Spell | fixed, 53 bytes |
| SMSG_CANCEL_SPELL_VISUAL_KIT | 0x510037 | 5308471 | — | Spell | fixed, 5 bytes |
| SMSG_PLAY_SPELL_VISUAL_KIT | 0x510038 | 5308472 | 21925 (0x55a5) | Spell | fixed, 13 bytes |
| SMSG_GAME_OBJECT_PLAY_SPELL_VISUAL_KIT | 0x510039 | 5308473 | — | Spell | fixed, 12 bytes |
| SMSG_GAME_OBJECT_PLAY_SPELL_VISUAL | 0x51003a | 5308474 | — | Spell | fixed, 4 bytes |
| SMSG_SUPERCEDED_SPELLS | 0x51003b | 5308475 | 13744 (0x35b0) | Spell | variable |
| SMSG_LEARNED_SPELLS | 0x51003c | 5308476 | — | Spell | variable |
| SMSG_UNLEARNED_SPELLS | 0x51003d | 5308477 | 18436 (0x4804) | Spell | variable |
| SMSG_PET_LEARNED_SPELLS | 0x51003e | 5308478 | 1287 (0x507) | Spell | variable |
| SMSG_PET_UNLEARNED_SPELLS | 0x51003f | 5308479 | 27140 (0x6a04) | Spell | variable |
| SMSG_PUSH_SPELL_TO_ACTION_BAR | 0x510040 | 5308480 | — | Spell | fixed struct |
| SMSG_REMOVE_SPELL_FROM_ACTION_BAR | 0x510041 | 5308481 | — | Spell | fixed struct |
| SMSG_SPELL_FAILURE | 0x510042 | 5308482 | 17717 (0x4535) | Spell | fixed, 10 bytes |
| SMSG_ACTIVE_GLYPHS | 0x510043 | 5308483 | — | Spell | variable |
| SMSG_SPELL_FAILED_OTHER | 0x510044 | 5308484 | 3124 (0xc34) | Spell | fixed, 9 bytes |
| SMSG_SCRIPT_CAST | 0x510045 | 5308485 | — | Spell | fixed struct |
| SMSG_CAST_FAILED | 0x510046 | 5308486 | 19734 (0x4d16) | Spell | fixed, 20 bytes |
| SMSG_PET_CAST_FAILED | 0x510047 | 5308487 | 11029 (0x2b15) | Spell | fixed, 16 bytes |
| SMSG_INTERRUPT_POWER_REGEN | 0x510048 | 5308488 | — | Spell | fixed, 1 bytes |
| SMSG_SPELL_FAILURE_MESSAGE | 0x510049 | 5308489 | — | Spell | fixed struct |
| — | 0x51004a | 5308490 | — | Spell | ignored by the client |
| — | 0x51004b | 5308491 | — | Spell | ignored by the client |
| — | 0x51004c | 5308492 | — | Spell | ignored by the client |
| — | 0x51004d | 5308493 | — | Spell | ignored by the client |
| SMSG_RESYNC_RUNES | 0x51004e | 5308494 | 25124 (0x6224) | Spell | variable |
| SMSG_CONVERT_RUNE | 0x51004f | 5308495 | 20244 (0x4f14) | Spell | variable |
| — | 0x510050 | 5308496 | — | Spell | ignored by the client |
| SMSG_DAMAGE_CALC_LOG | 0x510051 | 5308497 | 9270 (0x2436) | Spell | variable |
| SMSG_VOID_STORAGE_FAILED | 0x520000 | 5373952 | 6311 (0x18a7) | Storage | fixed struct |
| SMSG_VOID_STORAGE_CONTENTS | 0x520001 | 5373953 | 30132 (0x75b4) | Storage | variable |
| SMSG_VOID_STORAGE_TRANSFER_CHANGES | 0x520002 | 5373954 | 20902 (0x51a6) | Storage | variable |
| SMSG_VOID_TRANSFER_RESULT | 0x520003 | 5373955 | 7590 (0x1da6) | Storage | fixed struct |
| SMSG_VOID_ITEM_SWAP_RESPONSE | 0x520004 | 5373956 | 30882 (0x78a2) | Storage | fixed, 8 bytes |
| SMSG_INVENTORY_CHANGE_FAILURE | 0x520005 | 5373957 | 8758 (0x2236) | Storage | variable |
| SMSG_OPEN_CONTAINER | 0x520006 | 5373958 | 18196 (0x4714) | Storage | fixed, 0 bytes |
| SMSG_BAG_CLEANUP_FINISHED | 0x520007 | 5373959 | — | Storage | fixed struct |
| — | 0x540000 | 5505024 | — | Walk-in | ignored by the client |
