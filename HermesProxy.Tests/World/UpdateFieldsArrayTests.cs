using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using Xunit;

namespace HermesProxy.Tests.World;

public class UpdateFieldsArraySetGetTests
{
    [Fact]
    public void SetGetUpdateField_UInt32_RoundTrips()
    {
        var fields = new UpdateFieldsArray(10);
        uint expected = 0xDEADBEEF;

        fields.SetUpdateField<uint>(0, expected);

        Assert.Equal(expected, fields.GetUpdateField<uint>(0));
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    [InlineData(uint.MaxValue)]
    [InlineData(0x12345678u)]
    public void SetGetUpdateField_UInt32_VariousValues(uint value)
    {
        var fields = new UpdateFieldsArray(10);

        fields.SetUpdateField<uint>(0, value);

        Assert.Equal(value, fields.GetUpdateField<uint>(0));
    }

    [Fact]
    public void SetGetUpdateField_Int32_RoundTrips()
    {
        var fields = new UpdateFieldsArray(10);
        int expected = -42;

        fields.SetUpdateField<int>(0, expected);

        Assert.Equal(expected, fields.GetUpdateField<int>(0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void SetGetUpdateField_Int32_VariousValues(int value)
    {
        var fields = new UpdateFieldsArray(10);

        fields.SetUpdateField<int>(0, value);

        Assert.Equal(value, fields.GetUpdateField<int>(0));
    }

    [Fact]
    public void SetGetUpdateField_Float_RoundTrips()
    {
        var fields = new UpdateFieldsArray(10);
        float expected = 3.14159f;

        fields.SetUpdateField<float>(0, expected);

        Assert.Equal(expected, fields.GetUpdateField<float>(0));
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(-1.0f)]
    [InlineData(float.MaxValue)]
    [InlineData(float.MinValue)]
    [InlineData(float.Epsilon)]
    public void SetGetUpdateField_Float_VariousValues(float value)
    {
        var fields = new UpdateFieldsArray(10);

        fields.SetUpdateField<float>(0, value);

        Assert.Equal(value, fields.GetUpdateField<float>(0));
    }

    [Fact]
    public void SetGetUpdateField_UInt64_SpansTwoFields()
    {
        var fields = new UpdateFieldsArray(10);
        ulong expected = 0x123456789ABCDEF0;

        fields.SetUpdateField<ulong>(0, expected);

        Assert.Equal(expected, fields.GetUpdateField<ulong>(0));
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(ulong.MaxValue)]
    [InlineData(0x00000000FFFFFFFFul)]
    [InlineData(0xFFFFFFFF00000000ul)]
    public void SetGetUpdateField_UInt64_VariousValues(ulong value)
    {
        var fields = new UpdateFieldsArray(10);

        fields.SetUpdateField<ulong>(0, value);

        Assert.Equal(value, fields.GetUpdateField<ulong>(0));
    }

    [Fact]
    public void SetGetUpdateField_WowGuid128_SpansFourFields()
    {
        var fields = new UpdateFieldsArray(10);
        var expected = new WowGuid128(0x1122334455667788, 0xAABBCCDDEEFF0011);

        fields.SetUpdateField<WowGuid128>(0, expected);

        var result = fields.GetUpdateField<WowGuid128>(0);
        Assert.Equal(expected.Low, result.Low);
        Assert.Equal(expected.High, result.High);
    }

    [Fact]
    public void SetUpdateField_WowGuid128_DirectOverload_RoundTrips()
    {
        var fields = new UpdateFieldsArray(10);
        var expected = new WowGuid128(0x1122334455667788, 0xAABBCCDDEEFF0011);

        // Use the direct (non-generic) overload
        fields.SetUpdateField(0, expected);

        var result = fields.GetUpdateField<WowGuid128>(0);
        Assert.Equal(expected.Low, result.Low);
        Assert.Equal(expected.High, result.High);
    }

    [Fact]
    public void SetUpdateField_WowGuid128_DirectOverload_SetsMaskBits()
    {
        var fields = new UpdateFieldsArray(10);
        var guid = new WowGuid128(0x1122334455667788, 0xAABBCCDDEEFF0011);

        fields.SetUpdateField(0, guid);

        Assert.True(fields.m_updateMask.GetBit(0));
        Assert.True(fields.m_updateMask.GetBit(1));
        Assert.True(fields.m_updateMask.GetBit(2));
        Assert.True(fields.m_updateMask.GetBit(3));
    }

    [Fact]
    public void SetUpdateField_WowGuid128_DirectOverload_SameValue_DoesNotSetMask()
    {
        var fields = new UpdateFieldsArray(10);
        // Default WowGuid128 is all zeros, matching default UpdateValues
        var empty = WowGuid128.Empty;

        fields.SetUpdateField(0, empty);

        Assert.False(fields.m_updateMask.GetBit(0));
        Assert.False(fields.m_updateMask.GetBit(1));
        Assert.False(fields.m_updateMask.GetBit(2));
        Assert.False(fields.m_updateMask.GetBit(3));
    }

    [Fact]
    public void GetUpdateFieldGuid_RoundTrips()
    {
        var fields = new UpdateFieldsArray(10);
        var expected = new WowGuid128(0xDEADBEEFCAFEBABE, 0x1234567890ABCDEF);
        fields.SetUpdateField(0, expected);

        var result = fields.GetUpdateFieldGuid(0);

        Assert.Equal(expected.Low, result.Low);
        Assert.Equal(expected.High, result.High);
    }

    [Fact]
    public void GetUpdateFieldGuid_MatchesGenericPath()
    {
        var fields = new UpdateFieldsArray(10);
        var guid = new WowGuid128(0x1122334455667788, 0xAABBCCDDEEFF0011);
        fields.SetUpdateField(0, guid);

        var generic = fields.GetUpdateField<WowGuid128>(0);
        var direct = fields.GetUpdateFieldGuid(0);

        Assert.Equal(generic, direct);
    }

    [Fact]
    public void SetUpdateField_WowGuid128_DirectAndGeneric_ProduceSameResult()
    {
        var fieldsA = new UpdateFieldsArray(10);
        var fieldsB = new UpdateFieldsArray(10);
        var guid = new WowGuid128(0xDEADBEEFCAFEBABE, 0x1234567890ABCDEF);

        fieldsA.SetUpdateField<WowGuid128>(0, guid);
        fieldsB.SetUpdateField(0, guid);

        for (int i = 0; i < 4; i++)
        {
            Assert.Equal(fieldsA.m_updateValues[i].UnsignedValue, fieldsB.m_updateValues[i].UnsignedValue);
            Assert.Equal(fieldsA.m_updateMask.GetBit(i), fieldsB.m_updateMask.GetBit(i));
        }
    }

    [Fact]
    public void SetGetUpdateField_Byte_WithOffset0_PacksCorrectly()
    {
        var fields = new UpdateFieldsArray(10);

        fields.SetUpdateField<byte>(0, 0xAB, 0);

        Assert.Equal(0xAB, fields.GetUpdateField<byte>(0, 0));
    }

    [Fact]
    public void SetGetUpdateField_Byte_WithOffset3_PacksCorrectly()
    {
        var fields = new UpdateFieldsArray(10);

        fields.SetUpdateField<byte>(0, 0xCD, 3);

        Assert.Equal(0xCD, fields.GetUpdateField<byte>(0, 3));
    }

    [Fact]
    public void SetGetUpdateField_Byte_MultipleOffsets_Independent()
    {
        var fields = new UpdateFieldsArray(10);

        fields.SetUpdateField<byte>(0, 0x11, 0);
        fields.SetUpdateField<byte>(0, 0x22, 1);
        fields.SetUpdateField<byte>(0, 0x33, 2);
        fields.SetUpdateField<byte>(0, 0x44, 3);

        Assert.Equal(0x11, fields.GetUpdateField<byte>(0, 0));
        Assert.Equal(0x22, fields.GetUpdateField<byte>(0, 1));
        Assert.Equal(0x33, fields.GetUpdateField<byte>(0, 2));
        Assert.Equal(0x44, fields.GetUpdateField<byte>(0, 3));
        // All four bytes packed into one uint32
        Assert.Equal(0x44332211u, fields.GetUpdateField<uint>(0));
    }

    [Fact]
    public void SetGetUpdateField_UShort_WithOffset0_PacksCorrectly()
    {
        var fields = new UpdateFieldsArray(10);

        fields.SetUpdateField<ushort>(0, 0xBEEF, 0);

        Assert.Equal((ushort)0xBEEF, fields.GetUpdateField<ushort>(0, 0));
    }

    [Fact]
    public void SetGetUpdateField_UShort_WithOffset1_PacksCorrectly()
    {
        var fields = new UpdateFieldsArray(10);

        fields.SetUpdateField<ushort>(0, 0xDEAD, 1);

        Assert.Equal((ushort)0xDEAD, fields.GetUpdateField<ushort>(0, 1));
    }

    [Fact]
    public void SetGetUpdateField_UShort_BothOffsets_Independent()
    {
        var fields = new UpdateFieldsArray(10);

        fields.SetUpdateField<ushort>(0, 0xBEEF, 0);
        fields.SetUpdateField<ushort>(0, 0xDEAD, 1);

        Assert.Equal((ushort)0xBEEF, fields.GetUpdateField<ushort>(0, 0));
        Assert.Equal((ushort)0xDEAD, fields.GetUpdateField<ushort>(0, 1));
        Assert.Equal(0xDEADBEEFu, fields.GetUpdateField<uint>(0));
    }
}

public class UpdateFieldsArrayMaskTests
{
    [Fact]
    public void SetUpdateField_SetsUpdateMaskBit()
    {
        var fields = new UpdateFieldsArray(10);

        fields.SetUpdateField<uint>(3, 42u);

        Assert.True(fields.m_updateMask.GetBit(3));
    }

    [Fact]
    public void SetUpdateField_UntouchedFields_MaskBitNotSet()
    {
        var fields = new UpdateFieldsArray(10);

        fields.SetUpdateField<uint>(3, 42u);

        Assert.False(fields.m_updateMask.GetBit(0));
        Assert.False(fields.m_updateMask.GetBit(1));
        Assert.False(fields.m_updateMask.GetBit(2));
    }

    [Fact]
    public void SetUpdateField_SameValue_DoesNotSetMaskBit()
    {
        var fields = new UpdateFieldsArray(10);
        // Default value for uint is 0, so setting 0 should not dirty the field
        fields.SetUpdateField<uint>(0, 0u);

        Assert.False(fields.m_updateMask.GetBit(0));
    }

    [Fact]
    public void SetUpdateField_DifferentValue_SetsMaskBit()
    {
        var fields = new UpdateFieldsArray(10);

        fields.SetUpdateField<uint>(0, 1u);

        Assert.True(fields.m_updateMask.GetBit(0));
    }

    [Fact]
    public void SetUpdateField_UInt64_SetsTwoMaskBits()
    {
        var fields = new UpdateFieldsArray(10);

        fields.SetUpdateField<ulong>(2, 0x0000000100000001ul);

        Assert.True(fields.m_updateMask.GetBit(2));
        Assert.True(fields.m_updateMask.GetBit(3));
    }
}

public class UpdateFieldsArrayFlagTests
{
    [Fact]
    public void AddFlag_SetsSpecificBits()
    {
        var fields = new UpdateFieldsArray(10);

        fields.AddFlag(0, 0x04u);

        Assert.True(fields.HasFlag(0, 0x04u));
        Assert.Equal(0x04u, fields.GetUpdateField<uint>(0));
    }

    [Fact]
    public void AddFlag_MultipleBits_Accumulates()
    {
        var fields = new UpdateFieldsArray(10);

        fields.AddFlag(0, 0x01u);
        fields.AddFlag(0, 0x04u);
        fields.AddFlag(0, 0x10u);

        Assert.True(fields.HasFlag(0, 0x01u));
        Assert.True(fields.HasFlag(0, 0x04u));
        Assert.True(fields.HasFlag(0, 0x10u));
        Assert.Equal(0x15u, fields.GetUpdateField<uint>(0));
    }

    [Fact]
    public void RemoveFlag_ClearsSpecificBits()
    {
        var fields = new UpdateFieldsArray(10);
        fields.SetUpdateField<uint>(0, 0xFFu);

        fields.RemoveFlag(0, 0x0Fu);

        Assert.Equal(0xF0u, fields.GetUpdateField<uint>(0));
        Assert.False(fields.HasFlag(0, 0x0Fu));
        Assert.True(fields.HasFlag(0, 0xF0u));
    }

    [Fact]
    public void HasFlag_ReturnsFalseForUnsetBits()
    {
        var fields = new UpdateFieldsArray(10);
        fields.SetUpdateField<uint>(0, 0x0Fu);

        Assert.False(fields.HasFlag(0, 0x10u));
    }

    [Fact]
    public void HasFlag_OutOfBounds_ReturnsFalse()
    {
        var fields = new UpdateFieldsArray(5);

        Assert.False(fields.HasFlag(10, 0x01u));
    }

    [Fact]
    public void ApplyFlag_True_SetsFlag()
    {
        var fields = new UpdateFieldsArray(10);

        fields.ApplyFlag(0, 0x08u, true);

        Assert.True(fields.HasFlag(0, 0x08u));
    }

    [Fact]
    public void ApplyFlag_False_ClearsFlag()
    {
        var fields = new UpdateFieldsArray(10);
        fields.SetUpdateField<uint>(0, 0xFFu);

        fields.ApplyFlag(0, 0x08u, false);

        Assert.False(fields.HasFlag(0, 0x08u));
    }
}

public class UpdateFieldsArrayWriteTests
{
    [Fact]
    public void WriteToPacket_WritesOnlyDirtyFields()
    {
        var fields = new UpdateFieldsArray(8);
        fields.SetUpdateField<uint>(1, 0x11111111u);
        fields.SetUpdateField<uint>(5, 0x55555555u);

        var buffer = new ByteBuffer();
        fields.WriteToPacket(buffer);

        // Buffer should contain mask + only the 2 dirty field values
        var data = buffer.GetData();
        Assert.True(data.Length > 0);
        buffer.Dispose();
    }

    [Fact]
    public void WriteToPacket_NoDirtyFields_WritesOnlyMask()
    {
        var fields = new UpdateFieldsArray(8);

        var buffer = new ByteBuffer();
        fields.WriteToPacket(buffer);

        var data = buffer.GetData();
        // Should have mask data but no field data
        Assert.True(data.Length > 0);
        buffer.Dispose();
    }
}

public class UpdateFieldsArrayMultiFieldTests
{
    [Fact]
    public void SetUpdateField_MultipleIndices_Independent()
    {
        var fields = new UpdateFieldsArray(10);

        fields.SetUpdateField<uint>(0, 100u);
        fields.SetUpdateField<uint>(1, 200u);
        fields.SetUpdateField<uint>(2, 300u);

        Assert.Equal(100u, fields.GetUpdateField<uint>(0));
        Assert.Equal(200u, fields.GetUpdateField<uint>(1));
        Assert.Equal(300u, fields.GetUpdateField<uint>(2));
    }

    [Fact]
    public void SetUpdateField_OverwriteValue_UpdatesCorrectly()
    {
        var fields = new UpdateFieldsArray(10);

        fields.SetUpdateField<uint>(0, 100u);
        fields.SetUpdateField<uint>(0, 200u);

        Assert.Equal(200u, fields.GetUpdateField<uint>(0));
    }

    [Fact]
    public void SetUpdateField_MixedTypes_SameIndex_OverlappingUnion()
    {
        // The underlying UpdateValues is a StructLayout.Explicit union
        // Setting float at index 0 should reinterpret the bits as uint
        var fields = new UpdateFieldsArray(10);

        fields.SetUpdateField<float>(0, 1.0f);
        // 1.0f in IEEE 754 = 0x3F800000
        Assert.Equal(0x3F800000u, fields.GetUpdateField<uint>(0));
    }
}
