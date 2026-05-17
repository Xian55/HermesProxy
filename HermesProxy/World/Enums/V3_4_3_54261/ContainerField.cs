using HermesProxy.World.Objects;
using HermesProxy.World.Objects.Version.Attributes;

namespace HermesProxy.World.Enums.V3_4_3_54261;

// Phase 5b Container section — descriptor-driven WriteCreateContainerData /
// WriteUpdateContainerData / HasAnyContainerFieldSet emit. The V3_4_3 ContainerData wire
// layout is 39 bits across blocks 0+1 with a 2-bit blocks-mask prefix:
//   bit 0 = group gate for NumSlots
//   bit 1 = NumSlots
//   bit 2 = group gate for Slots[36] (PerElement array)
//   bits 3..38 = Slots[0..35] individual change bits
//
// Previous-life note: this file used to hold a legacy descriptor-tree slot-index enum
// (CONTAINER_FIELD_SLOT_1 = 80, etc.) for the pre-Cataclysm reader. Nothing referenced it
// from V3_4_3_54261 source. Safe to repurpose.
[DescriptorSection(DataType = typeof(ContainerData), MaskMode = MaskMode.Blocks, MaskWidth = 2)]
public enum ContainerField
{
    // -------------------------------------------------------------------------
    // Create-path emit order = enum declaration order. Hand-port writes Slots
    // first (36 PackedGuid128, defaulting to Empty), then NumSlots (UInt32, default 0).
    // -------------------------------------------------------------------------

    [DescriptorCreateField(nameof(ContainerData.Slots), DescriptorType.PackedGuid128,
                           ArrayCount = 36, ArrayMode = ArrayMode.PerElement)]
    [DescriptorUpdateField(nameof(ContainerData.Slots), DescriptorType.PackedGuid128, bit: 3,
                           ArrayCount = 36, ArrayMode = ArrayMode.PerElement, ParentBit = 2)]
    CONTAINER_FIELD_SLOTS,

    [DescriptorCreateField(nameof(ContainerData.NumSlots), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(ContainerData.NumSlots), DescriptorType.UInt32, bit: 1, ParentBit = 0)]
    CONTAINER_FIELD_NUM_SLOTS,
}
