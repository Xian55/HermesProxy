# Movement — server packet layouts, 4.4.2.60895

### SMSG_TIME_SYNC_REQUEST (0x4c0000)

- Modern: 4980736 (0x4c0000) · 4.3.4: 15524 (0x3ca4) · Area: Movement
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_TIME_ADJUSTMENT (0x4c0001)

- Modern: 4980737 (0x4c0001) · 4.3.4: 31159 (0x79b7) · Area: Movement
- Layout: `struct`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32, +4: f32

### SMSG_ON_MONSTER_MOVE (0x4c0002)

- Modern: 4980738 (0x4c0002) · 4.3.4: 28183 (0x6e17) · Area: Movement
- Layout: `guid u32*3 { u32 u8 { u32*4 u8 guid u8*6 opt[ u32*2 u16 u32 u16 loop[ u16*2 ] u8 ] opt[ u32 ] opt[ guid ] opt[ u32*3 ] loop[ u32*3 ] loop[ u32 ] opt[ { guid u32*4 } ] opt[ u32*3 ] opt[ u32*3 u8 ] } }`
- Bit fields (all-zero packet, widths in arrival order): byte 18: 1 · 3 · (4 unused); byte 38: 8 · 2 · 16; byte 41: 1 · 1 · 16; byte 43: 1 · 1 · 1 · 1

### SMSG_MOVE_SET_ACTIVE_MOVER (0x4c0003)

- Modern: 4980739 (0x4c0003) · 4.3.4: 4531 (0x11b3) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_UPDATE_RUN_SPEED (0x4c0004)

- Modern: 4980740 (0x4c0004) · 4.3.4: 5286 (0x14a6) · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE_RUN_BACK_SPEED (0x4c0005)

- Modern: 4980741 (0x4c0005) · 4.3.4: 15782 (0x3da6) · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE_WALK_SPEED (0x4c0006)

- Modern: 4980742 (0x4c0006) · 4.3.4: 21666 (0x54a2) · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE_SWIM_SPEED (0x4c0007)

- Modern: 4980743 (0x4c0007) · 4.3.4: 22965 (0x59b5) · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE_SWIM_BACK_SPEED (0x4c0008)

- Modern: 4980744 (0x4c0008) · 4.3.4: 12469 (0x30b5) · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE_FLIGHT_SPEED (0x4c0009)

- Modern: 4980745 (0x4c0009) · 4.3.4: 12465 (0x30b1) · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE_FLIGHT_BACK_SPEED (0x4c000a)

- Modern: 4980746 (0x4c000a) · 4.3.4: 29856 (0x74a0) · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE_TURN_RATE (0x4c000b)

- Modern: 4980747 (0x4c000b) · 4.3.4: 23969 (0x5da1) · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE_PITCH_RATE (0x4c000c)

- Modern: 4980748 (0x4c000c) · 4.3.4: 7605 (0x1db5) · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE_COLLISION_HEIGHT (0x4c000d)

- Modern: 4980749 (0x4c000d) · 4.3.4: 22947 (0x59a3) · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32*2`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE (0x4c000e)

- Modern: 4980750 (0x4c000e) · 4.3.4: 31138 (0x79a2) · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE_TELEPORT (0x4c000f)

- Modern: 4980751 (0x4c000f) · 4.3.4: 20658 (0x50b2) · Area: Movement
- Layout: `{ { guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32 u8*2 loop[ { guid u32*9 u8 } ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1; byte 55: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (7 unused)

### SMSG_MOVE_UPDATE_KNOCK_BACK (0x4c0010)

- Modern: 4980752 (0x4c0010) · 4.3.4: 15794 (0x3db2) · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE_MOD_MOVEMENT_FORCE_MAGNITUDE (0x4c0011)

- Modern: 4980753 (0x4c0011) · 4.3.4: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE_APPLY_MOVEMENT_FORCE (0x4c0012)

- Modern: 4980754 (0x4c0012) · 4.3.4: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } { guid u32*9 u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1; byte 89: 2 · (6 unused)

### SMSG_MOVE_UPDATE_REMOVE_MOVEMENT_FORCE (0x4c0013)

- Modern: 4980755 (0x4c0013) · 4.3.4: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } guid`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_SET_MOD_MOVEMENT_FORCE_MAGNITUDE (0x4c0014)

- Modern: 4980756 (0x4c0014) · 4.3.4: — · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_RUN_SPEED (0x4c0015)

- Modern: 4980757 (0x4c0015) · 4.3.4: 20919 (0x51b7) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_RUN_BACK_SPEED (0x4c0016)

- Modern: 4980758 (0x4c0016) · 4.3.4: 15795 (0x3db3) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_SWIM_SPEED (0x4c0017)

- Modern: 4980759 (0x4c0017) · 4.3.4: 14756 (0x39a4) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_SWIM_BACK_SPEED (0x4c0018)

- Modern: 4980760 (0x4c0018) · 4.3.4: 22945 (0x59a1) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_FLIGHT_SPEED (0x4c0019)

- Modern: 4980761 (0x4c0019) · 4.3.4: 14752 (0x39a0) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_FLIGHT_BACK_SPEED (0x4c001a)

- Modern: 4980762 (0x4c001a) · 4.3.4: 14515 (0x38b3) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_WALK_SPEED (0x4c001b)

- Modern: 4980763 (0x4c001b) · 4.3.4: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_TURN_RATE (0x4c001c)

- Modern: 4980764 (0x4c001c) · 4.3.4: 30901 (0x78b5) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_PITCH_RATE (0x4c001d)

- Modern: 4980765 (0x4c001d) · 4.3.4: 5296 (0x14b0) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_RUN_SPEED (0x4c001e)

- Modern: 4980766 (0x4c001e) · 4.3.4: 15797 (0x3db5) · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_RUN_BACK_SPEED (0x4c001f)

- Modern: 4980767 (0x4c001f) · 4.3.4: 29105 (0x71b1) · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_SWIM_SPEED (0x4c0020)

- Modern: 4980768 (0x4c0020) · 4.3.4: 5543 (0x15a7) · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_SWIM_BACK_SPEED (0x4c0021)

- Modern: 4980769 (0x4c0021) · 4.3.4: 23718 (0x5ca6) · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_FLIGHT_SPEED (0x4c0022)

- Modern: 4980770 (0x4c0022) · 4.3.4: 29094 (0x71a6) · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_FLIGHT_BACK_SPEED (0x4c0023)

- Modern: 4980771 (0x4c0023) · 4.3.4: 12450 (0x30a2) · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_WALK_SPEED (0x4c0024)

- Modern: 4980772 (0x4c0024) · 4.3.4: 7588 (0x1da4) · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_TURN_RATE (0x4c0025)

- Modern: 4980773 (0x4c0025) · 4.3.4: 12453 (0x30a5) · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_PITCH_RATE (0x4c0026)

- Modern: 4980774 (0x4c0026) · 4.3.4: 30128 (0x75b0) · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_MOVE_ROOT (0x4c0027)

- Modern: 4980775 (0x4c0027) · 4.3.4: 32160 (0x7da0) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_UNROOT (0x4c0028)

- Modern: 4980776 (0x4c0028) · 4.3.4: 32180 (0x7db4) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_WATER_WALK (0x4c0029)

- Modern: 4980777 (0x4c0029) · 4.3.4: 30129 (0x75b1) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_ENABLE_DOUBLE_JUMP (0x4c002a)

- Modern: 4980778 (0x4c002a) · 4.3.4: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_DISABLE_DOUBLE_JUMP (0x4c002b)

- Modern: 4980779 (0x4c002b) · 4.3.4: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_LAND_WALK (0x4c002c)

- Modern: 4980780 (0x4c002c) · 4.3.4: 13495 (0x34b7) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_FEATHER_FALL (0x4c002d)

- Modern: 4980781 (0x4c002d) · 4.3.4: 31152 (0x79b0) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_NORMAL_FALL (0x4c002e)

- Modern: 4980782 (0x4c002e) · 4.3.4: 20918 (0x51b6) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_HOVERING (0x4c002f)

- Modern: 4980783 (0x4c002f) · 4.3.4: 23731 (0x5cb3) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_UNSET_HOVERING (0x4c0030)

- Modern: 4980784 (0x4c0030) · 4.3.4: 20915 (0x51b3) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_KNOCK_BACK (0x4c0031)

- Modern: 4980785 (0x4c0031) · 4.3.4: 23732 (0x5cb4) · Area: Movement
- Layout: `guid u32*5`
- Size: 20 bytes (packed GUIDs not counted)

### SMSG_MOVE_TELEPORT (0x4c0032)

- Modern: 4980786 (0x4c0032) · 4.3.4: — · Area: Movement
- Layout: `{ guid u32*5 u8*2 opt[ u8*2 ] opt[ guid ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 8 · 1 · 1 · (6 unused)

### SMSG_MOVE_SET_CAN_FLY (0x4c0033)

- Modern: 4980787 (0x4c0033) · 4.3.4: 15777 (0x3da1) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_UNSET_CAN_FLY (0x4c0034)

- Modern: 4980788 (0x4c0034) · 4.3.4: 5538 (0x15a2) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_CAN_TURN_WHILE_FALLING (0x4c0037)

- Modern: 4980791 (0x4c0037) · 4.3.4: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_UNSET_CAN_TURN_WHILE_FALLING (0x4c0038)

- Modern: 4980792 (0x4c0038) · 4.3.4: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_IGNORE_MOVEMENT_FORCES (0x4c0039)

- Modern: 4980793 (0x4c0039) · 4.3.4: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_UNSET_IGNORE_MOVEMENT_FORCES (0x4c003a)

- Modern: 4980794 (0x4c003a) · 4.3.4: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_ENABLE_TRANSITION_BETWEEN_SWIM_AND_FLY (0x4c003b)

- Modern: 4980795 (0x4c003b) · 4.3.4: 22946 (0x59a2) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_DISABLE_TRANSITION_BETWEEN_SWIM_AND_FLY (0x4c003c)

- Modern: 4980796 (0x4c003c) · 4.3.4: 32178 (0x7db2) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_DISABLE_GRAVITY (0x4c003d)

- Modern: 4980797 (0x4c003d) · 4.3.4: 30130 (0x75b2) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_ENABLE_GRAVITY (0x4c003e)

- Modern: 4980798 (0x4c003e) · 4.3.4: 12467 (0x30b3) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_DISABLE_INERTIA (0x4c003f)

- Modern: 4980799 (0x4c003f) · 4.3.4: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_ENABLE_INERTIA (0x4c0040)

- Modern: 4980800 (0x4c0040) · 4.3.4: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_DISABLE_COLLISION (0x4c0041)

- Modern: 4980801 (0x4c0041) · 4.3.4: 12720 (0x31b0) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_ENABLE_COLLISION (0x4c0042)

- Modern: 4980802 (0x4c0042) · 4.3.4: 4519 (0x11a7) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_COLLISION_HEIGHT (0x4c0043)

- Modern: 4980803 (0x4c0043) · 4.3.4: 4528 (0x11b0) · Area: Movement
- Layout: `guid u32*3 u8 u32*2`
- Size: 21 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_VEHICLE_REC_ID (0x4c0044)

- Modern: 4980804 (0x4c0044) · 4.3.4: — · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_MOVE_APPLY_MOVEMENT_FORCE (0x4c0045)

- Modern: 4980805 (0x4c0045) · 4.3.4: — · Area: Movement
- Layout: `guid u32 { guid u32*9 u8 }`
- Size: 41 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 44: 2 · (6 unused)

### SMSG_MOVE_REMOVE_MOVEMENT_FORCE (0x4c0046)

- Modern: 4980806 (0x4c0046) · 4.3.4: — · Area: Movement
- Layout: `guid u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_COMPOUND_STATE (0x4c0047)

- Modern: 4980807 (0x4c0047) · 4.3.4: 30112 (0x75a0) · Area: Movement
- Layout: `guid u32 loop[ { u32*2 u8*2 opt[ { guid u32*9 u8 } ] opt[ u32 ] opt[ u32*2 ] opt[ u32*4 ] opt[ u32 ] opt[ u32*2 u8 ] opt[ guid ] opt[ u32 ] opt[ u32 ] } ]`

### SMSG_MOVE_SKIP_TIME (0x4c0048)

- Modern: 4980808 (0x4c0048) · 4.3.4: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_ROOT (0x4c0049)

- Modern: 4980809 (0x4c0049) · 4.3.4: 20916 (0x51b4) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_UNROOT (0x4c004a)

- Modern: 4980810 (0x4c004a) · 4.3.4: 30134 (0x75b6) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_DISABLE_GRAVITY (0x4c004b)

- Modern: 4980811 (0x4c004b) · 4.3.4: 23989 (0x5db5) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_ENABLE_GRAVITY (0x4c004c)

- Modern: 4980812 (0x4c004c) · 4.3.4: 15526 (0x3ca6) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_DISABLE_COLLISION (0x4c004d)

- Modern: 4980813 (0x4c004d) · 4.3.4: 13745 (0x35b1) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_ENABLE_COLLISION (0x4c004e)

- Modern: 4980814 (0x4c004e) · 4.3.4: 15536 (0x3cb0) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_FEATHER_FALL (0x4c004f)

- Modern: 4980815 (0x4c004f) · 4.3.4: 15781 (0x3da5) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_NORMAL_FALL (0x4c0050)

- Modern: 4980816 (0x4c0050) · 4.3.4: 14514 (0x38b2) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_HOVER (0x4c0051)

- Modern: 4980817 (0x4c0051) · 4.3.4: 5302 (0x14b6) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_UNSET_HOVER (0x4c0052)

- Modern: 4980818 (0x4c0052) · 4.3.4: 32165 (0x7da5) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_WATER_WALK (0x4c0053)

- Modern: 4980819 (0x4c0053) · 4.3.4: 20642 (0x50a2) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_LAND_WALK (0x4c0054)

- Modern: 4980820 (0x4c0054) · 4.3.4: 15783 (0x3da7) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_START_SWIM (0x4c0055)

- Modern: 4980821 (0x4c0055) · 4.3.4: 12709 (0x31a5) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_STOP_SWIM (0x4c0056)

- Modern: 4980822 (0x4c0056) · 4.3.4: 7586 (0x1da2) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_RUN_MODE (0x4c0057)

- Modern: 4980823 (0x4c0057) · 4.3.4: 30119 (0x75a7) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_WALK_MODE (0x4c0058)

- Modern: 4980824 (0x4c0058) · 4.3.4: 21686 (0x54b6) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_FLYING (0x4c0059)

- Modern: 4980825 (0x4c0059) · 4.3.4: 12725 (0x31b5) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_UNSET_FLYING (0x4c005a)

- Modern: 4980826 (0x4c005a) · 4.3.4: 22694 (0x58a6) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_FLIGHT_SPLINE_SYNC (0x4c005b)

- Modern: 4980827 (0x4c005b) · 4.3.4: 2340 (0x924) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_APPLY_INERTIA (0x4c005e)

- Modern: 4980830 (0x4c005e) · 4.3.4: — · Area: Movement
- Layout: `guid u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### SMSG_MOVE_REMOVE_INERTIA (0x4c005f)

- Modern: 4980831 (0x4c005f) · 4.3.4: — · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_MOVE_UPDATE_APPLY_INERTIA (0x4c0060)

- Modern: 4980832 (0x4c0060) · 4.3.4: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32*2`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE_REMOVE_INERTIA (0x4c0061)

- Modern: 4980833 (0x4c0061) · 4.3.4: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x4c0073)

- Modern: 4980851 (0x4c0073) · 4.3.4: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)
