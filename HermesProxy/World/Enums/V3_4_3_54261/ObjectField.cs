using HermesProxy.World.Objects;
using HermesProxy.World.Objects.Version.Attributes;

namespace HermesProxy.World.Enums.V3_4_3_54261;

public enum ObjectField
{
	OBJECT_FIELD_GUID = 0,

	[DescriptorCreateField(nameof(ObjectData.EntryID), DescriptorType.Int32)]
	OBJECT_FIELD_ENTRY = 4,

	[DescriptorCreateField(nameof(ObjectData.DynamicFlags), DescriptorType.UInt32)]
	OBJECT_DYNAMIC_FLAGS = 5,

	[DescriptorCreateField(nameof(ObjectData.Scale), DescriptorType.Float, DefaultExpression = "1f")]
	OBJECT_FIELD_SCALE_X = 6,

	OBJECT_END = 7
}
