# Storage — server packet layouts, 3.4.3.54261

### — (0x2da0)

- Modern: 11680 (0x2da0) · 3.3.5a: — · Area: Storage
- Layout: `struct`
- Struct fields the client uses (0 bytes): none

### — (0x2da1)

- Modern: 11681 (0x2da1) · 3.3.5a: — · Area: Storage
- Layout: `u8 loop[ guid guid u32 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } ]`

### — (0x2da2)

- Modern: 11682 (0x2da2) · 3.3.5a: — · Area: Storage
- Layout: `u8 loop[ guid guid u32 { u32*3 u8*2 loop[ u32 u8 ] opt[ u8 u32 loop[ u32 ] ] } ] loop[ guid ]`
- Bit fields (all-zero packet, widths in arrival order): byte 0: 4 · 4

### — (0x2da3)

- Modern: 11683 (0x2da3) · 3.3.5a: — · Area: Storage
- Layout: `struct`
- Struct fields the client uses (4 bytes): +0: u32

### — (0x2da4)

- Modern: 11684 (0x2da4) · 3.3.5a: — · Area: Storage
- Layout: `guid u32 guid u32`
- Size: 8 bytes (packed GUIDs not counted)

### SMSG_INVENTORY_CHANGE_FAILURE (0x2da5)

- Modern: 11685 (0x2da5) · 3.3.5a: 274 (0x112) · Area: Storage
- Layout: `u32 loop[ guid ] u8 opt[ u32 ] opt[ guid u32 guid ] opt[ u32 ]`
- HermesProxy: `InventoryChangeFailure` — matches

### — (0x2da6)

- Modern: 11686 (0x2da6) · 3.3.5a: — · Area: Storage
- Layout: `guid`
- Size: 0 bytes (packed GUIDs not counted)

### — (0x2da7)

- Modern: 11687 (0x2da7) · 3.3.5a: — · Area: Storage
- Layout: `struct`
- Struct fields the client uses (0 bytes): none
