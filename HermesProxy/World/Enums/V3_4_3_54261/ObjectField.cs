using HermesProxy.World.Objects;
using HermesProxy.World.Objects.Version.Attributes;

namespace HermesProxy.World.Enums.V3_4_3_54261;

// Phase 5b: descriptor-driven WriteCreateObjectData emit. Attribute-decorated enum members
// drive HermesProxy.SourceGen.ObjectUpdateBuilderGenerator, which emits the partial method
// body on V3_4_3_54261.ObjectUpdateBuilder. OBJECT_FIELD_GUID is intentionally undecorated —
// the GUID is written in the update preamble (WriteToPacket), not as a descriptor field.
// OBJECT_END is a sentinel.
[DescriptorSection(DataType = typeof(ObjectData), MaskMode = MaskMode.Flat, MaskWidth = 4)]
public enum ObjectField
{
    OBJECT_FIELD_GUID = 0,

    [DescriptorCreateField(nameof(ObjectData.EntryID), DescriptorType.Int32)]
    [DescriptorUpdateField(nameof(ObjectData.EntryID), DescriptorType.Int32, bit: 1)]
    OBJECT_FIELD_ENTRY = 4,

    [DescriptorCreateField(nameof(ObjectData.DynamicFlags), DescriptorType.UInt32)]
    [DescriptorUpdateField(nameof(ObjectData.DynamicFlags), DescriptorType.UInt32, bit: 2)]
    OBJECT_DYNAMIC_FLAGS = 5,

    [DescriptorCreateField(nameof(ObjectData.Scale), DescriptorType.Float, DefaultExpression = "1f")]
    [DescriptorUpdateField(nameof(ObjectData.Scale), DescriptorType.Float, bit: 3)]
    OBJECT_FIELD_SCALE_X = 6,

    OBJECT_END = 7
}
