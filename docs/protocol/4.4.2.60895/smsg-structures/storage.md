# Storage — server packet layouts, 4.4.2.60895

### SMSG_VOID_STORAGE_FAILED (0x520000)

- Modern: 5373952 (0x520000) · 4.3.4: 6311 (0x18a7) · Area: Storage
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_VOID_STORAGE_CONTENTS (0x520001)

- Modern: 5373953 (0x520001) · 4.3.4: 30132 (0x75b4) · Area: Storage
- Layout: `{ u8 loop[ guid guid u32 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } ] }`

### SMSG_VOID_STORAGE_TRANSFER_CHANGES (0x520002)

- Modern: 5373954 (0x520002) · 4.3.4: 20902 (0x51a6) · Area: Storage
- Layout: `{ u8 loop[ guid guid u32 { u32*3 u8 { u8 loop[ u8 u32 ] } opt[ u8 u32 loop[ u32 ] ] } ] loop[ guid ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · 4

### SMSG_VOID_TRANSFER_RESULT (0x520003)

- Modern: 5373955 (0x520003) · 4.3.4: 7590 (0x1da6) · Area: Storage
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_VOID_ITEM_SWAP_RESPONSE (0x520004)

- Modern: 5373956 (0x520004) · 4.3.4: 30882 (0x78a2) · Area: Storage
- Layout: `{ guid u32 guid u32 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_INVENTORY_CHANGE_FAILURE (0x520005)

- Modern: 5373957 (0x520005) · 4.3.4: 8758 (0x2236) · Area: Storage
- Layout: `{ u32 loop[ guid ] u8 opt[ u32 ] opt[ guid u32 guid ] opt[ u32 ] }`

### SMSG_OPEN_CONTAINER (0x520006)

- Modern: 5373958 (0x520006) · 4.3.4: 18196 (0x4714) · Area: Storage
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_BAG_CLEANUP_FINISHED (0x520007)

- Modern: 5373959 (0x520007) · 4.3.4: — · Area: Storage
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
