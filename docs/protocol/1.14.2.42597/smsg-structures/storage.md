# Storage — server packet layouts, 1.14.2.42597

### SMSG_VOID_STORAGE_FAILED (0x2da0)

- Modern: 11680 (0x2da0) · 1.12.1: — · Area: Storage
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none

### SMSG_VOID_STORAGE_CONTENTS (0x2da1)

- Modern: 11681 (0x2da1) · 1.12.1: — · Area: Storage
- Layout: `{ u8 loop[ guid guid u32 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } ] }`

### SMSG_VOID_STORAGE_TRANSFER_CHANGES (0x2da2)

- Modern: 11682 (0x2da2) · 1.12.1: — · Area: Storage
- Layout: `{ u8 loop[ guid guid u32 { u32*3 u8 { u8 loop[ u32 u8 ] } opt[ u8 u32 loop[ u32 ] ] } ] loop[ guid ] }`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · 4

### SMSG_VOID_TRANSFER_RESULT (0x2da3)

- Modern: 11683 (0x2da3) · 1.12.1: — · Area: Storage
- Layout: `{ struct }`
- Struct fields the client uses (4 bytes): +0: u32

### SMSG_VOID_ITEM_SWAP_RESPONSE (0x2da4)

- Modern: 11684 (0x2da4) · 1.12.1: — · Area: Storage
- Layout: `{ guid u32 guid u32 }`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_INVENTORY_CHANGE_FAILURE (0x2da5)

- Modern: 11685 (0x2da5) · 1.12.1: 274 (0x112) · Area: Storage
- Layout: `{ u8 loop[ guid ] u8 opt[ u32 ] opt[ guid u32 guid ] opt[ u32 ] }`
- HermesProxy: `InventoryChangeFailure` — matches

### SMSG_OPEN_CONTAINER (0x2da6)

- Modern: 11686 (0x2da6) · 1.12.1: 275 (0x113) · Area: Storage
- Layout: `{ guid }`
- Size: 0 bytes (packed GUIDs not counted)

### SMSG_BAG_CLEANUP_FINISHED (0x2da7)

- Modern: 11687 (0x2da7) · 1.12.1: — · Area: Storage
- Layout: `{ struct }`
- Struct fields the client uses (0 bytes): none
