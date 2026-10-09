# Movement — server packet layouts, 3.4.3.54261

### SMSG_TIME_SYNC_REQUEST (0x2dd2)

- Modern: 11730 (0x2dd2) · 3.3.5a: 912 (0x390) · Area: Movement
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32
- HermesProxy: `TimeSyncRequest` — matches

### — (0x2dd3)

- Modern: 11731 (0x2dd3) · 3.3.5a: — · Area: Movement
- Layout: `struct`
- Struct fields the client uses (8 bytes): +0: u32, +4: u32, +4: f32

### SMSG_ON_MONSTER_MOVE / SMSG_MONSTER_MOVE_TRANSPORT (0x2dd4)

- Modern: 11732 (0x2dd4) · 3.3.5a: 221 (0xdd), 686 (0x2ae) · Area: Movement
- Layout: `guid u32*7 u8 { u32*4 u8 guid u8*4 u8*2 opt[ u32 u32 u16 u32 u16 loop[ u16*2 ] u8 ] opt[ u32 ] opt[ guid ] opt[ u32*3 ] loop[ u32*3 ] loop[ u32 ] opt[ { guid u32*4 } ] opt[ u32*3 ] opt[ u32*3 u8 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 30: 1 · 3 · (4 unused); byte 50: 8 · 2 · 16; byte 53: 1 · 1 · 16; byte 55: 1 · 1 · 1 · 1
- HermesProxy: `MonsterMove` — differs (#369)

### SMSG_MOVE_SET_ACTIVE_MOVER (0x2dd5)

- Modern: 11733 (0x2dd5) · 3.3.5a: — · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)
- HermesProxy: `MoveSetActiveMover` — matches

### SMSG_MOVE_UPDATE_RUN_SPEED (0x2dd6)

- Modern: 11734 (0x2dd6) · 3.3.5a: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE_RUN_BACK_SPEED (0x2dd7)

- Modern: 11735 (0x2dd7) · 3.3.5a: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE_WALK_SPEED (0x2dd8)

- Modern: 11736 (0x2dd8) · 3.3.5a: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE_SWIM_SPEED (0x2dd9)

- Modern: 11737 (0x2dd9) · 3.3.5a: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE_SWIM_BACK_SPEED (0x2dda)

- Modern: 11738 (0x2dda) · 3.3.5a: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE_FLIGHT_SPEED (0x2ddb)

- Modern: 11739 (0x2ddb) · 3.3.5a: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE_FLIGHT_BACK_SPEED (0x2ddc)

- Modern: 11740 (0x2ddc) · 3.3.5a: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE_TURN_RATE (0x2ddd)

- Modern: 11741 (0x2ddd) · 3.3.5a: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE_PITCH_RATE (0x2dde)

- Modern: 11742 (0x2dde) · 3.3.5a: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x2ddf)

- Modern: 11743 (0x2ddf) · 3.3.5a: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32*2`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### SMSG_MOVE_UPDATE (0x2de0)

- Modern: 11744 (0x2de0) · 3.3.5a: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `MoveUpdate` — not checked

### — (0x2de1)

- Modern: 11745 (0x2de1) · 3.3.5a: — · Area: Movement
- Layout: `{ { guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32 u8*2 loop[ { guid u32*9 u8 } ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] opt[ u32 ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1; byte 55: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1 · (7 unused)

### SMSG_MOVE_UPDATE_KNOCK_BACK (0x2de2)

- Modern: 11746 (0x2de2) · 3.3.5a: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1
- HermesProxy: `MoveUpdateKnockBack` — not checked

### — (0x2de3)

- Modern: 11747 (0x2de3) · 3.3.5a: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x2de4)

- Modern: 11748 (0x2de4) · 3.3.5a: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } { guid u32*9 u8 }`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1; byte 89: 2 · (6 unused)

### — (0x2de5)

- Modern: 11749 (0x2de5) · 3.3.5a: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } guid`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x2de6)

- Modern: 11750 (0x2de6) · 3.3.5a: — · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_RUN_SPEED (0x2de7)

- Modern: 11751 (0x2de7) · 3.3.5a: 766 (0x2fe) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_RUN_BACK_SPEED (0x2de8)

- Modern: 11752 (0x2de8) · 3.3.5a: 767 (0x2ff) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_SWIM_SPEED (0x2de9)

- Modern: 11753 (0x2de9) · 3.3.5a: 768 (0x300) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_SWIM_BACK_SPEED (0x2dea)

- Modern: 11754 (0x2dea) · 3.3.5a: 770 (0x302) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_FLIGHT_SPEED (0x2deb)

- Modern: 11755 (0x2deb) · 3.3.5a: 901 (0x385) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_FLIGHT_BACK_SPEED (0x2dec)

- Modern: 11756 (0x2dec) · 3.3.5a: 902 (0x386) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_WALK_SPEED (0x2ded)

- Modern: 11757 (0x2ded) · 3.3.5a: 769 (0x301) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_TURN_RATE (0x2dee)

- Modern: 11758 (0x2dee) · 3.3.5a: 771 (0x303) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_PITCH_RATE (0x2def)

- Modern: 11759 (0x2def) · 3.3.5a: 1118 (0x45e) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_FORCE_RUN_SPEED_CHANGE / SMSG_MOVE_SET_RUN_SPEED (0x2df0)

- Modern: 11760 (0x2df0) · 3.3.5a: 226 (0xe2) · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_FORCE_RUN_BACK_SPEED_CHANGE / SMSG_MOVE_SET_RUN_BACK_SPEED (0x2df1)

- Modern: 11761 (0x2df1) · 3.3.5a: 228 (0xe4) · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_FORCE_SWIM_SPEED_CHANGE / SMSG_MOVE_SET_SWIM_SPEED (0x2df2)

- Modern: 11762 (0x2df2) · 3.3.5a: 230 (0xe6) · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_SWIM_BACK_SPEED (0x2df3)

- Modern: 11763 (0x2df3) · 3.3.5a: — · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_FORCE_FLIGHT_SPEED_CHANGE / SMSG_MOVE_SET_FLIGHT_SPEED (0x2df4)

- Modern: 11764 (0x2df4) · 3.3.5a: 897 (0x381) · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_FLIGHT_BACK_SPEED (0x2df5)

- Modern: 11765 (0x2df5) · 3.3.5a: — · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_FORCE_WALK_SPEED_CHANGE / SMSG_MOVE_SET_WALK_SPEED (0x2df6)

- Modern: 11766 (0x2df6) · 3.3.5a: 730 (0x2da) · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_FORCE_TURN_RATE_CHANGE / SMSG_MOVE_SET_TURN_RATE (0x2df7)

- Modern: 11767 (0x2df7) · 3.3.5a: 734 (0x2de) · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_PITCH_RATE (0x2df8)

- Modern: 11768 (0x2df8) · 3.3.5a: — · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_MOVE_ROOT (0x2df9)

- Modern: 11769 (0x2df9) · 3.3.5a: 232 (0xe8) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_UNROOT (0x2dfa)

- Modern: 11770 (0x2dfa) · 3.3.5a: 234 (0xea) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_WATER_WALK (0x2dfb)

- Modern: 11771 (0x2dfb) · 3.3.5a: 222 (0xde) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x2dfc)

- Modern: 11772 (0x2dfc) · 3.3.5a: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x2dfd)

- Modern: 11773 (0x2dfd) · 3.3.5a: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_LAND_WALK (0x2dfe)

- Modern: 11774 (0x2dfe) · 3.3.5a: 223 (0xdf) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_FEATHER_FALL (0x2dff)

- Modern: 11775 (0x2dff) · 3.3.5a: 242 (0xf2) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_NORMAL_FALL (0x2e00)

- Modern: 11776 (0x2e00) · 3.3.5a: 243 (0xf3) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_HOVERING (0x2e01)

- Modern: 11777 (0x2e01) · 3.3.5a: 244 (0xf4) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_UNSET_HOVERING (0x2e02)

- Modern: 11778 (0x2e02) · 3.3.5a: 245 (0xf5) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_KNOCK_BACK (0x2e03)

- Modern: 11779 (0x2e03) · 3.3.5a: 239 (0xef) · Area: Movement
- Layout: `guid u32*5`
- Size: 20 bytes (packed GUIDs not counted)
- HermesProxy: `MoveKnockBack` — matches

### SMSG_MOVE_TELEPORT (0x2e04)

- Modern: 11780 (0x2e04) · 3.3.5a: — · Area: Movement
- Layout: `guid u32*5 u8*2 opt[ u8*2 ] opt[ guid ]`
- Bit fields (all-zero packet, widths in arrival order): byte 22: 8 · 1 · 1 · (6 unused)
- HermesProxy: `MoveTeleport` — matches

### SMSG_MOVE_SET_CAN_FLY (0x2e05)

- Modern: 11781 (0x2e05) · 3.3.5a: 835 (0x343) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_UNSET_CAN_FLY (0x2e06)

- Modern: 11782 (0x2e06) · 3.3.5a: 836 (0x344) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x2e07)

- Modern: 11783 (0x2e07) · 3.3.5a: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x2e08)

- Modern: 11784 (0x2e08) · 3.3.5a: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x2e09)

- Modern: 11785 (0x2e09) · 3.3.5a: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x2e0a)

- Modern: 11786 (0x2e0a) · 3.3.5a: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_ENABLE_TRANSITION_BETWEEN_SWIM_AND_FLY (0x2e0b)

- Modern: 11787 (0x2e0b) · 3.3.5a: 830 (0x33e) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_DISABLE_TRANSITION_BETWEEN_SWIM_AND_FLY (0x2e0c)

- Modern: 11788 (0x2e0c) · 3.3.5a: 831 (0x33f) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_DISABLE_GRAVITY (0x2e0d)

- Modern: 11789 (0x2e0d) · 3.3.5a: 1230 (0x4ce) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_ENABLE_GRAVITY (0x2e0e)

- Modern: 11790 (0x2e0e) · 3.3.5a: 1232 (0x4d0) · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x2e0f)

- Modern: 11791 (0x2e0f) · 3.3.5a: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x2e10)

- Modern: 11792 (0x2e10) · 3.3.5a: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x2e11)

- Modern: 11793 (0x2e11) · 3.3.5a: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x2e12)

- Modern: 11794 (0x2e12) · 3.3.5a: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SET_COLLISION_HEIGHT (0x2e13)

- Modern: 11795 (0x2e13) · 3.3.5a: — · Area: Movement
- Layout: `guid u32*3 u8 u32*2`
- Size: 21 bytes (packed GUIDs not counted)
- HermesProxy: `MoveSetCollisionHeight` — matches

### SMSG_MOVE_SET_VEHICLE_REC_ID (0x2e14)

- Modern: 11796 (0x2e14) · 3.3.5a: — · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x2e15)

- Modern: 11797 (0x2e15) · 3.3.5a: — · Area: Movement
- Layout: `guid u32 { guid u32*9 u8 }`
- Size: 41 bytes (packed GUIDs not counted)
- Bit fields (all-zero packet, widths in arrival order): byte 44: 2 · (6 unused)

### — (0x2e16)

- Modern: 11798 (0x2e16) · 3.3.5a: — · Area: Movement
- Layout: `guid u32 guid`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x2e17)

- Modern: 11799 (0x2e17) · 3.3.5a: — · Area: Movement
- Layout: `guid u32 loop[ { u16 u32 u8*2 opt[ { guid u32*9 u8 } ] opt[ u32 ] opt[ u32*2 ] opt[ u32*4 ] opt[ u32 ] opt[ u32*2 u8 ] opt[ guid ] opt[ u32 ] opt[ u32 ] } ]`

### — (0x2e18)

- Modern: 11800 (0x2e18) · 3.3.5a: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_ROOT (0x2e19)

- Modern: 11801 (0x2e19) · 3.3.5a: 794 (0x31a) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_UNROOT (0x2e1a)

- Modern: 11802 (0x2e1a) · 3.3.5a: 772 (0x304) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_DISABLE_GRAVITY (0x2e1b)

- Modern: 11803 (0x2e1b) · 3.3.5a: 1235 (0x4d3) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_ENABLE_GRAVITY (0x2e1c)

- Modern: 11804 (0x2e1c) · 3.3.5a: 1236 (0x4d4) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x2e1d)

- Modern: 11805 (0x2e1d) · 3.3.5a: — · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x2e1e)

- Modern: 11806 (0x2e1e) · 3.3.5a: — · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_FEATHER_FALL (0x2e1f)

- Modern: 11807 (0x2e1f) · 3.3.5a: 773 (0x305) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_NORMAL_FALL (0x2e20)

- Modern: 11808 (0x2e20) · 3.3.5a: 774 (0x306) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_HOVER (0x2e21)

- Modern: 11809 (0x2e21) · 3.3.5a: 775 (0x307) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_UNSET_HOVER (0x2e22)

- Modern: 11810 (0x2e22) · 3.3.5a: 776 (0x308) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_WATER_WALK (0x2e23)

- Modern: 11811 (0x2e23) · 3.3.5a: 777 (0x309) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_LAND_WALK (0x2e24)

- Modern: 11812 (0x2e24) · 3.3.5a: 778 (0x30a) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_START_SWIM (0x2e25)

- Modern: 11813 (0x2e25) · 3.3.5a: 779 (0x30b) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_STOP_SWIM (0x2e26)

- Modern: 11814 (0x2e26) · 3.3.5a: 780 (0x30c) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_RUN_MODE (0x2e27)

- Modern: 11815 (0x2e27) · 3.3.5a: 781 (0x30d) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_WALK_MODE (0x2e28)

- Modern: 11816 (0x2e28) · 3.3.5a: 782 (0x30e) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_SET_FLYING (0x2e29)

- Modern: 11817 (0x2e29) · 3.3.5a: 1058 (0x422) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_MOVE_SPLINE_UNSET_FLYING (0x2e2a)

- Modern: 11818 (0x2e2a) · 3.3.5a: 1059 (0x423) · Area: Movement
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x2e2b)

- Modern: 11819 (0x2e2b) · 3.3.5a: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)

### — (0x2e2e)

- Modern: 11822 (0x2e2e) · 3.3.5a: — · Area: Movement
- Layout: `guid u32*3`
- Size: 12 bytes (packed GUIDs not counted)

### — (0x2e2f)

- Modern: 11823 (0x2e2f) · 3.3.5a: — · Area: Movement
- Layout: `guid u32*2`
- Size: 8 bytes (packed GUIDs not counted)

### — (0x2e30)

- Modern: 11824 (0x2e30) · 3.3.5a: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32*2`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x2e31)

- Modern: 11825 (0x2e31) · 3.3.5a: — · Area: Movement
- Layout: `{ guid loop[ u32 ] u32*9 loop[ guid ] u8 opt[ { guid u32*4 u8 u32 u8 opt[ u32 ] opt[ u32 ] } ] opt[ guid ] opt[ u32*5 ] opt[ u32*2 ] opt[ u32*2 u8 opt[ u32*3 ] ] } u32`
- Bit fields (all-zero packet, widths in arrival order): byte 50: 1 · 1 · 1 · 1 · 1 · 1 · 1 · 1

### — (0x2e43)

- Modern: 11843 (0x2e43) · 3.3.5a: — · Area: Movement
- Layout: `guid u32`
- Size: 4 bytes (packed GUIDs not counted)
