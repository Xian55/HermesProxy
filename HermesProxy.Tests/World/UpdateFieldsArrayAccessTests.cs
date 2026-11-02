using System;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Objects;
using Xunit;

namespace HermesProxy.Tests.World;

public class UpdateFieldsArrayGetTests
{
    private static UpdateFieldsArray WithWords(params ReadOnlySpan<uint> words)
    {
        var fields = new UpdateFieldsArray((uint)words.Length);
        for (int i = 0; i < words.Length; i++)
            fields.m_updateValues[i].UnsignedValue = words[i];
        return fields;
    }

    [Theory]
    [InlineData(0, 0x44)]
    [InlineData(1, 0x33)]
    [InlineData(2, 0x22)]
    [InlineData(3, 0x11)]
    public void GetUpdateField_Byte_ReadsTheByteAtItsOffset(byte offset, byte expected)
    {
        Assert.Equal(expected, WithWords(0x11223344).GetUpdateField<byte>(0, offset));
    }

    [Theory]
    [InlineData(0, 0x3344)]
    [InlineData(1, 0x1122)]
    public void GetUpdateField_UShort_ReadsTheHalfAtItsOffset(byte offset, ushort expected)
    {
        Assert.Equal(expected, WithWords(0x11223344).GetUpdateField<ushort>(0, offset));
    }

    [Fact]
    public void GetUpdateField_Int32_ReadsTheBitsAsSigned()
    {
        Assert.Equal(-2, WithWords(0xFFFFFFFE).GetUpdateField<int>(0));
    }

    [Fact]
    public void GetUpdateField_UInt32_IgnoresTheOffset()
    {
        Assert.Equal(0x11223344u, WithWords(0x11223344).GetUpdateField<uint>(0, 3));
    }

    [Fact]
    public void GetUpdateField_Float_ReadsTheBitsAsAFloat()
    {
        Assert.Equal(1.5f, WithWords(0x3FC00000).GetUpdateField<float>(0));
    }

    [Fact]
    public void GetUpdateField_UInt64_TakesTheLowWordFirst()
    {
        Assert.Equal(0x2222222211111111ul, WithWords(0x11111111, 0x22222222).GetUpdateField<ulong>(0));
    }

    [Fact]
    public void GetUpdateField_WowGuid128_TakesFourWordsLowFirst()
    {
        var fields = WithWords(0x11111111, 0x22222222, 0x33333333, 0x44444444);

        var guid = fields.GetUpdateField<WowGuid128>(0);

        Assert.Equal(new WowGuid128(0x2222222211111111ul, 0x4444444433333333ul), guid);
        Assert.Equal(guid, fields.GetUpdateFieldGuid(0));
    }

    [Fact]
    public void GetUpdateField_UntouchedField_ReadsZero()
    {
        var fields = new UpdateFieldsArray(4);

        Assert.Equal(0u, fields.GetUpdateField<uint>(0));
        Assert.Equal(0ul, fields.GetUpdateField<ulong>(0));
        Assert.Equal(default, fields.GetUpdateField<WowGuid128>(0));
    }

    [Fact]
    public void GetUpdateField_DoesNotMarkTheFieldDirty()
    {
        var fields = WithWords(1, 2, 3, 4);

        fields.GetUpdateField<uint>(0);
        fields.GetUpdateField<ulong>(0);
        fields.GetUpdateField<WowGuid128>(0);
        fields.GetUpdateFieldGuid(0);

        Assert.All(fields.m_updateMask.Blocks.ToArray(), block => Assert.Equal(0u, block));
    }

    [Fact]
    public void GetUpdateField_PastTheEnd_Throws()
    {
        var fields = new UpdateFieldsArray(4);

        Assert.Throws<IndexOutOfRangeException>(() => fields.GetUpdateField<uint>(4));
        Assert.Throws<IndexOutOfRangeException>(() => fields.GetUpdateField<ulong>(3));
        Assert.Throws<IndexOutOfRangeException>(() => fields.GetUpdateField<WowGuid128>(1));
    }

    [Fact]
    public void GetUpdateField_ATypeAFieldCannotHold_Throws()
    {
        var fields = new UpdateFieldsArray(4);

        Assert.Contains("Int64", Assert.Throws<Exception>(() => fields.GetUpdateField<long>(0)).Message);
        Assert.Contains("Int16", Assert.Throws<Exception>(() => fields.GetUpdateField<short>(0)).Message);
        Assert.Contains("Double", Assert.Throws<Exception>(() => fields.GetUpdateField<double>(0)).Message);
        Assert.Contains("Boolean", Assert.Throws<Exception>(() => fields.GetUpdateField<bool>(0)).Message);
        Assert.Contains("DayOfWeek", Assert.Throws<Exception>(() => fields.GetUpdateField<DayOfWeek>(0)).Message);
        Assert.Contains("String", Assert.Throws<Exception>(() => fields.GetUpdateField<string>(0)).Message);
        Assert.Contains("Nullable", Assert.Throws<Exception>(() => fields.GetUpdateField<int?>(0)).Message);
    }
}

public class UpdateFieldsArraySetRulesTests
{
    [Fact]
    public void SetUpdateField_Byte_LeavesTheOtherBytesAlone()
    {
        var fields = new UpdateFieldsArray(1);
        fields.m_updateValues[0].UnsignedValue = 0x11223344;

        fields.SetUpdateField<byte>(0, 0xAB, 2);

        Assert.Equal(0x11AB3344u, fields.GetUpdateField<uint>(0));
    }

    [Fact]
    public void SetUpdateField_UShort_LeavesTheOtherHalfAlone()
    {
        var fields = new UpdateFieldsArray(1);
        fields.m_updateValues[0].UnsignedValue = 0x11223344;

        fields.SetUpdateField<ushort>(0, 0xABCD, 1);

        Assert.Equal(0xABCD3344u, fields.GetUpdateField<uint>(0));
    }

    [Fact]
    public void SetUpdateField_AnOffsetPastTheField_ChangesNothing()
    {
        var fields = new UpdateFieldsArray(1);

        fields.SetUpdateField<byte>(0, 0xAB, 4);
        fields.SetUpdateField<ushort>(0, 0xABCD, 2);

        Assert.Equal(0u, fields.GetUpdateField<uint>(0));
        Assert.False(fields.m_updateMask.GetBit(0));
    }

    [Fact]
    public void SetUpdateField_UInt64_OneChangedWord_MarksBoth()
    {
        var fields = new UpdateFieldsArray(2);
        fields.m_updateValues[0].UnsignedValue = 1;

        fields.SetUpdateField<ulong>(0, 0x0000000500000001ul);

        Assert.True(fields.m_updateMask.GetBit(0));
        Assert.True(fields.m_updateMask.GetBit(1));
    }

    [Fact]
    public void SetUpdateField_WowGuid128_OneChangedHalf_MarksThatHalfOnly()
    {
        var fields = new UpdateFieldsArray(4);

        fields.SetUpdateField<WowGuid128>(0, new WowGuid128(0, 5));

        Assert.False(fields.m_updateMask.GetBit(0));
        Assert.False(fields.m_updateMask.GetBit(1));
        Assert.True(fields.m_updateMask.GetBit(2));
        Assert.True(fields.m_updateMask.GetBit(3));
    }

    [Fact]
    public void SetUpdateField_ATypeAFieldCannotHold_Throws()
    {
        var fields = new UpdateFieldsArray(2);

        Assert.Contains("Int64", Assert.Throws<Exception>(() => fields.SetUpdateField<long>(0, 1L)).Message);
        Assert.Contains("Double", Assert.Throws<Exception>(() => fields.SetUpdateField<double>(0, 1d)).Message);
    }
}

// Computed indexes, as the array loops of the builders pass them: a constant boxed without
// allocating, so a test on one would have passed with the object index these replaced.
public class UpdateFieldsArrayAllocationTests
{
    private static long AllocatedBy(Action action)
    {
        // A first call is JIT and type loading, which now and then lands on a runtime table that
        // has to grow, and the tens of KB for it count against this thread. A box costs every call.
        action();

        var before = GC.GetAllocatedBytesForCurrentThread();
        action();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [Fact]
    public void GetUpdateField_ComputedIndex_AllocatesNothing()
    {
        var fields = new UpdateFieldsArray(64);
        uint sum = 0;

        var allocated = AllocatedBy(() =>
        {
            for (int index = 0; index < 60; index += 4)
            {
                sum += fields.GetUpdateField<uint>(index);
                sum += fields.GetUpdateField<byte>(index, 1);
                sum += (uint)fields.GetUpdateField<ulong>(index);
                sum += (uint)fields.GetUpdateField<WowGuid128>(index).Low;
                sum += (uint)fields.GetUpdateFieldGuid(index).High;
            }
        });

        Assert.Equal(0, allocated);
        Assert.Equal(0u, sum);
    }

    [Fact]
    public void SetUpdateField_ComputedIndex_AllocatesNothing()
    {
        var fields = new UpdateFieldsArray(64);

        var allocated = AllocatedBy(() =>
        {
            for (int index = 0; index < 60; index += 4)
            {
                fields.SetUpdateField<uint>(index, 1u);
                fields.SetUpdateField<int>(index + 1, 2);
                fields.SetUpdateField<float>(index + 2, 3f);
                fields.SetUpdateField<byte>(index + 3, 4, 1);
            }
        });

        Assert.Equal(0, allocated);
    }

    [Fact]
    public void DynamicSetUpdateField_ComputedIndex_AllocatesNothing()
    {
        using var warm = new ByteBuffer();
        var fields = new DynamicUpdateFieldsArray(8, HermesProxy.World.Enums.UpdateTypeModern.Values);
        fields.SetUpdateField(0, new uint[64], DynamicFieldChangeType.ValueChanged);

        var allocated = AllocatedBy(() =>
        {
            for (int index = 1; index < 4; index++)
                fields.SetUpdateField<uint>(index, 1u, DynamicFieldChangeType.ValueChanged);
        });

        Assert.Equal(0, allocated);
    }
}

public class UpdateFieldsArrayWireBytesTests
{
    // What a client reads, so no change of how fields are addressed may move a byte of it.
    [Fact]
    public void WriteToPacket_EveryKindOfField_WritesTheseBytes()
    {
        var fields = new UpdateFieldsArray(64);
        fields.SetUpdateField<uint>(0, 0xDEADBEEFu);
        fields.SetUpdateField<int>(1, -2);
        fields.SetUpdateField<float>(2, 1.5f);
        fields.SetUpdateField<ulong>(4, 0x1122334455667788ul);
        fields.SetUpdateField<byte>(6, 0xAB, 2);
        fields.SetUpdateField<ushort>(7, 0xCDEF, 1);
        fields.SetUpdateField<WowGuid128>(33, new WowGuid128(0x0102030405060708ul, 0x1112131415161718ul));

        using var buffer = new ByteBuffer();
        fields.WriteToPacket(buffer);

        Assert.Equal(
            "02" + "F7000000" + "1E000000" +
            "EFBEADDE" + "FEFFFFFF" + "0000C03F" + "88776655" + "44332211" + "0000AB00" + "0000EFCD" +
            "08070605" + "04030201" + "18171615" + "14131211",
            Convert.ToHexString(buffer.GetData()));
    }

    [Fact]
    public void DynamicField_EveryKindOfValue_WritesTheseBytes()
    {
        var fields = new DynamicUpdateFieldsArray(8, HermesProxy.World.Enums.UpdateTypeModern.Values);
        fields.SetUpdateField<uint>(0, 0xDEADBEEFu, DynamicFieldChangeType.Unchanged);
        fields.SetUpdateField<int>(1, -2, DynamicFieldChangeType.ValueChanged);
        fields.SetUpdateField<float>(2, 1.5f, DynamicFieldChangeType.ValueAndSizeChanged);
        fields.SetUpdateField<ulong>(3, 0x1122334455667788ul, DynamicFieldChangeType.ValueChanged);
        fields.SetUpdateField<WowGuid128>(4, new WowGuid128(1, 2), DynamicFieldChangeType.ValueChanged);

        using var buffer = new ByteBuffer();
        fields.WriteToPacket(buffer);

        Assert.Equal(
            "01" + "1F000000" +
            "0100" + "01000000" + "EFBEADDE" +
            "0100" + "01000000" + "FEFFFFFF" +
            "0180" + "01000000" + "01000000" + "0000C03F" +
            "0100" + "03000000" + "88776655" + "44332211" +
            "0100" + "0F000000" + "01000000" + "00000000" + "02000000" + "00000000",
            Convert.ToHexString(buffer.GetData()));
    }
}
