using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using Xunit;

namespace HermesProxy.Tests.World;

// The outbound UpdateMask used to be backed by System.Collections.BitArray. The oracle
// reimplements that version's serialization so these tests pin the V1_14/V2_5 wire bytes to what
// it produced, independent of whatever block math the current implementation uses.
internal static class LegacyUpdateMaskOracle
{
    public static int BlockCount(int fieldCount) => (fieldCount + 31) / 32;

    public static byte[] MaskBytes(int fieldCount, IEnumerable<int> setBits)
    {
        var mask = new BitArray(fieldCount);
        foreach (var bit in setBits)
            mask.Set(bit, true);

        var bytes = new byte[BlockCount(fieldCount) * 4];
        mask.CopyTo(bytes, 0);
        return bytes;
    }

    public static byte[] AppendToPacket(int fieldCount, IEnumerable<int> setBits)
    {
        var mask = MaskBytes(fieldCount, setBits);
        var result = new byte[1 + mask.Length];
        result[0] = (byte)BlockCount(fieldCount);
        mask.CopyTo(result, 1);
        return result;
    }

    // One DynamicUpdateFieldsArray entry as DynamicUpdateMask + the value loop wrote it,
    // including the original change-type expression.
    public static byte[] DynamicEntry(uint[] values, DynamicFieldChangeType changeType, UpdateTypeModern updateType)
    {
        var blockCount = (uint)BlockCount(values.Length);
        var header = (uint)(blockCount | ((uint)(changeType & DynamicFieldChangeType.ValueAndSizeChanged) * ((3 - (int)updateType) / 3)));

        return Capture(buffer =>
        {
            buffer.WriteUInt16((ushort)header);
            if (updateType == UpdateTypeModern.Values && changeType == DynamicFieldChangeType.ValueAndSizeChanged)
                buffer.WriteInt32(values.Length);
            buffer.WriteBytes(MaskBytes(values.Length, Enumerable.Range(0, values.Length)));
            foreach (var value in values)
                buffer.WriteUInt32(value);
        });
    }

    public static byte[] Capture(Action<ByteBuffer> write)
    {
        using var buffer = new ByteBuffer();
        write(buffer);
        return buffer.GetData();
    }

    public static byte[] Concat(params byte[][] parts) => parts.SelectMany(p => p).ToArray();

    public static byte[] UInt32s(IEnumerable<uint> values) => Capture(buffer =>
    {
        foreach (var value in values)
            buffer.WriteUInt32(value);
    });
}

public class UpdateMaskWireTests
{
    private static readonly double[] Densities = [0.0, 0.05, 0.5, 1.0];

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(31)]
    [InlineData(32)]
    [InlineData(33)]
    [InlineData(63)]
    [InlineData(64)]
    [InlineData(65)]
    [InlineData(80)]    // V1_14_1 ITEM_END
    [InlineData(218)]   // V1_14_1 UNIT_END
    [InlineData(760)]   // V1_14_1 PLAYER_END
    [InlineData(4674)]  // V1_14_1 ACTIVE_PLAYER_END
    public void AppendToPacket_RandomBits_MatchesBitArrayLayout(int fieldCount)
    {
        foreach (var density in Densities)
        {
            var rng = new Random(fieldCount * 31 + (int)(density * 100));
            var bits = Enumerable.Range(0, fieldCount).Where(_ => rng.NextDouble() < density).ToArray();
            var mask = new UpdateMask((uint)fieldCount);
            foreach (var bit in bits)
                mask.SetBit(bit);

            var actual = LegacyUpdateMaskOracle.Capture(mask.AppendToPacket);

            Assert.Equal(LegacyUpdateMaskOracle.AppendToPacket(fieldCount, bits), actual);
        }
    }

    [Fact]
    public void AppendToPacket_EachBitAlone_MatchesBitArrayLayout()
    {
        const int fieldCount = 97;
        for (var bit = 0; bit < fieldCount; ++bit)
        {
            var mask = new UpdateMask(fieldCount);
            mask.SetBit(bit);

            var actual = LegacyUpdateMaskOracle.Capture(mask.AppendToPacket);

            Assert.Equal(LegacyUpdateMaskOracle.AppendToPacket(fieldCount, [bit]), actual);
        }
    }

    [Fact]
    public void GetBit_AfterSetBit_OnlyThatBitIsSet()
    {
        var mask = new UpdateMask(100);

        mask.SetBit(33);

        for (var i = 0; i < 100; ++i)
            Assert.Equal(i == 33, mask.GetBit(i));
    }

    [Fact]
    public void Clear_AfterSettingBits_WritesEmptyMask()
    {
        var mask = new UpdateMask(4674);
        mask.SetBit(0);
        mask.SetBit(31);
        mask.SetBit(32);
        mask.SetBit(4673);

        mask.Clear();

        Assert.False(mask.GetBit(0));
        Assert.False(mask.GetBit(4673));
        Assert.Equal(LegacyUpdateMaskOracle.AppendToPacket(4674, []), LegacyUpdateMaskOracle.Capture(mask.AppendToPacket));
    }

    [Fact]
    public void GetCount_ReturnsFieldCount()
    {
        Assert.Equal(218u, new UpdateMask(218).GetCount());
    }

    // 33 fields leave 31 spare bits in the second block; the bound is the field count, as it
    // was with BitArray, not the block capacity.
    [Theory]
    [InlineData(33)]
    [InlineData(63)]
    [InlineData(-1)]
    public void SetBit_OutsideFieldCount_Throws(int index)
    {
        var mask = new UpdateMask(33);

        Assert.Throws<ArgumentOutOfRangeException>(() => mask.SetBit(index));
    }

    [Theory]
    [InlineData(33)]
    [InlineData(63)]
    [InlineData(-1)]
    public void GetBit_OutsideFieldCount_Throws(int index)
    {
        var mask = new UpdateMask(33);

        Assert.Throws<ArgumentOutOfRangeException>(() => mask.GetBit(index));
    }
}

public class UpdateFieldsArrayWireTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(32)]
    [InlineData(33)]
    [InlineData(80)]
    [InlineData(218)]
    [InlineData(4674)]
    public void WriteToPacket_RandomDirtyFields_MatchesBitArrayLayout(int fieldCount)
    {
        var rng = new Random(fieldCount);
        var fields = new UpdateFieldsArray((uint)fieldCount);
        var dirty = new SortedDictionary<int, uint>();
        for (var n = 0; n < Math.Max(1, fieldCount / 10); ++n)
        {
            var index = rng.Next(fieldCount);
            var value = (uint)rng.Next() | 1u;
            fields.SetUpdateField<uint>(index, value);
            dirty[index] = value;
        }

        var actual = LegacyUpdateMaskOracle.Capture(fields.WriteToPacket);

        var expected = LegacyUpdateMaskOracle.Concat(
            LegacyUpdateMaskOracle.AppendToPacket(fieldCount, dirty.Keys),
            LegacyUpdateMaskOracle.UInt32s(dirty.Values));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void WriteToPacket_FieldsInLastPartialBlock_WrittenInIndexOrder()
    {
        var fields = new UpdateFieldsArray(70);
        fields.SetUpdateField<uint>(69, 0x69u);
        fields.SetUpdateField<uint>(64, 0x64u);
        fields.SetUpdateField<uint>(31, 0x31u);
        fields.SetUpdateField<uint>(0, 0x01u);

        var actual = LegacyUpdateMaskOracle.Capture(fields.WriteToPacket);

        var expected = LegacyUpdateMaskOracle.Concat(
            LegacyUpdateMaskOracle.AppendToPacket(70, [0, 31, 64, 69]),
            LegacyUpdateMaskOracle.UInt32s([0x01u, 0x31u, 0x64u, 0x69u]));
        Assert.Equal(expected, actual);
    }

    // The V1_14/V2_5 builders reuse a cached UpdateFieldsArray and Clear() its mask per update:
    // values stay, only the fields touched since the clear go out.
    [Fact]
    public void WriteToPacket_AfterMaskClear_WritesOnlyFieldsSetSinceClear()
    {
        var fields = new UpdateFieldsArray(218);
        fields.SetUpdateField<uint>(10, 100u);
        fields.SetUpdateField<uint>(20, 200u);
        fields.m_updateMask.Clear();

        fields.SetUpdateField<uint>(20, 201u);
        var actual = LegacyUpdateMaskOracle.Capture(fields.WriteToPacket);

        var expected = LegacyUpdateMaskOracle.Concat(
            LegacyUpdateMaskOracle.AppendToPacket(218, [20]),
            LegacyUpdateMaskOracle.UInt32s([201u]));
        Assert.Equal(expected, actual);
        Assert.Equal(100u, fields.GetUpdateField<uint>(10));
    }

    [Fact]
    public void WriteToPacket_CalledTwice_WritesSameBytes()
    {
        var fields = new UpdateFieldsArray(80);
        fields.SetUpdateField(4, new WowGuid128(0x1122334455667788, 0xAABBCCDDEEFF0011));

        var first = LegacyUpdateMaskOracle.Capture(fields.WriteToPacket);
        var second = LegacyUpdateMaskOracle.Capture(fields.WriteToPacket);

        Assert.Equal(first, second);
    }
}

public class DynamicUpdateFieldsArrayWireTests
{
    public static TheoryData<int, DynamicFieldChangeType, UpdateTypeModern> Cases()
    {
        var data = new TheoryData<int, DynamicFieldChangeType, UpdateTypeModern>();
        foreach (var count in new[] { 0, 1, 2, 4, 30, 31, 32, 33, 64, 65 })
            foreach (var changeType in Enum.GetValues<DynamicFieldChangeType>())
                foreach (var updateType in Enum.GetValues<UpdateTypeModern>())
                    data.Add(count, changeType, updateType);
        return data;
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void SetUpdateField_ValueArray_MatchesDynamicUpdateMaskLayout(int valueCount, DynamicFieldChangeType changeType, UpdateTypeModern updateType)
    {
        const int fieldCount = 18;  // V1_14_1 ACTIVE_PLAYER_DYNAMIC_END
        const int index = 13;
        var values = Enumerable.Range(0, valueCount).Select(i => 0xA0000000u | (uint)i).ToArray();
        var fields = new DynamicUpdateFieldsArray(fieldCount, updateType);

        fields.SetUpdateField(index, values, changeType);
        var actual = LegacyUpdateMaskOracle.Capture(fields.WriteToPacket);

        var expected = LegacyUpdateMaskOracle.Concat(
            LegacyUpdateMaskOracle.AppendToPacket(fieldCount, [index]),
            LegacyUpdateMaskOracle.DynamicEntry(values, changeType, updateType));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SetUpdateField_TwoFields_EntriesInCallOrder()
    {
        var gems = new uint[30];
        gems[0] = 1; gems[10] = 2; gems[20] = 3;
        var fields = new DynamicUpdateFieldsArray(18, UpdateTypeModern.Values);

        fields.SetUpdateField(10, new uint[] { 7, 8 }, DynamicFieldChangeType.ValueChanged);
        fields.SetUpdateField(3, gems, DynamicFieldChangeType.ValueAndSizeChanged);
        var actual = LegacyUpdateMaskOracle.Capture(fields.WriteToPacket);

        var expected = LegacyUpdateMaskOracle.Concat(
            LegacyUpdateMaskOracle.AppendToPacket(18, [3, 10]),
            LegacyUpdateMaskOracle.DynamicEntry([7, 8], DynamicFieldChangeType.ValueChanged, UpdateTypeModern.Values),
            LegacyUpdateMaskOracle.DynamicEntry(gems, DynamicFieldChangeType.ValueAndSizeChanged, UpdateTypeModern.Values));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void WriteToPacket_NoFieldsSet_WritesEmptyMaskOnly()
    {
        var fields = new DynamicUpdateFieldsArray(18, UpdateTypeModern.Values);

        var actual = LegacyUpdateMaskOracle.Capture(fields.WriteToPacket);

        Assert.Equal(LegacyUpdateMaskOracle.AppendToPacket(18, []), actual);
    }

    [Fact]
    public void SetUpdateFieldGeneric_EachType_MatchesValueArrayOverload()
    {
        var guid = new WowGuid128(0x1122334455667788, 0xAABBCCDDEEFF0011);
        AssertGenericMatches(-5, [unchecked((uint)-5)]);
        AssertGenericMatches(0xDEADBEEFu, [0xDEADBEEFu]);
        AssertGenericMatches(3.5f, [BitConverter.SingleToUInt32Bits(3.5f)]);
        AssertGenericMatches(0x123456789ABCDEF0ul, [0x9ABCDEF0u, 0x12345678u]);
        AssertGenericMatches(guid,
        [
            (uint)guid.GetLowValue(), (uint)(guid.GetLowValue() >> 32),
            (uint)guid.GetHighValue(), (uint)(guid.GetHighValue() >> 32),
        ]);
    }

    private static void AssertGenericMatches<T>(T value, uint[] expectedValues) where T : new()
    {
        var generic = new DynamicUpdateFieldsArray(3, UpdateTypeModern.Values);
        generic.SetUpdateField(2, value, DynamicFieldChangeType.ValueAndSizeChanged);

        var expected = LegacyUpdateMaskOracle.Concat(
            LegacyUpdateMaskOracle.AppendToPacket(3, [2]),
            LegacyUpdateMaskOracle.DynamicEntry(expectedValues, DynamicFieldChangeType.ValueAndSizeChanged, UpdateTypeModern.Values));
        Assert.Equal(expected, LegacyUpdateMaskOracle.Capture(generic.WriteToPacket));
    }
}

// Each Values update on the V1_14/V2_5 path goes through these. The BitArray version allocated
// on every call: a temp ByteBuffer, its GetData() copy and the mask byte[] per WriteToPacket;
// per dynamic field a DynamicUpdateMask with two BitArrays, a temp ByteBuffer, its copy, the mask
// byte[] and, on the generic path, the uint[] of values.
public class UpdateMaskAllocationTests
{
    private static long AllocatedBy(Action action)
    {
        var before = GC.GetAllocatedBytesForCurrentThread();
        action();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [Fact]
    public void UpdateFieldsArrayClearSetWrite_WarmPacket_AllocatesNothing()
    {
        const int fieldCount = 4674;  // V1_14_1 ACTIVE_PLAYER_END
        var fields = new UpdateFieldsArray(fieldCount);
        for (var index = 0; index < fieldCount; index += 300)
            fields.SetUpdateField<uint>(index, (uint)index + 1);
        using var packet = new ByteBuffer();
        fields.WriteToPacket(packet);
        packet.Clear();

        var allocated = AllocatedBy(() =>
        {
            fields.m_updateMask.Clear();
            for (var index = 0; index < fieldCount; index += 300)
                fields.m_updateMask.SetBit(index);
            fields.WriteToPacket(packet);
        });

        Assert.Equal(0, allocated);
    }

    [Fact]
    public void DynamicSetUpdateFieldGenericAndWrite_BufferRented_AllocatesNothing()
    {
        var guid = new WowGuid128(0x1122334455667788, 0xAABBCCDDEEFF0011);
        // Builders pass the field index as a boxed enum; box it once, outside the measurement.
        object channelIndex = 2;
        using var packet = new ByteBuffer();
        var warmup = new DynamicUpdateFieldsArray(18, UpdateTypeModern.Values);
        warmup.SetUpdateField(channelIndex, guid, DynamicFieldChangeType.ValueAndSizeChanged);
        warmup.WriteToPacket(packet);
        packet.Clear();

        var fields = new DynamicUpdateFieldsArray(18, UpdateTypeModern.Values);
        fields.SetUpdateField(3, new uint[30], DynamicFieldChangeType.ValueAndSizeChanged);

        var allocated = AllocatedBy(() =>
        {
            fields.SetUpdateField(channelIndex, guid, DynamicFieldChangeType.ValueAndSizeChanged);
            fields.WriteToPacket(packet);
        });

        Assert.Equal(0, allocated);
    }

    [Fact]
    public void DynamicWriteToPacket_NoFieldSet_AllocatesNothing()
    {
        using var packet = new ByteBuffer();
        new DynamicUpdateFieldsArray(18, UpdateTypeModern.Values).WriteToPacket(packet);
        packet.Clear();
        var fields = new DynamicUpdateFieldsArray(18, UpdateTypeModern.Values);

        var allocated = AllocatedBy(() => fields.WriteToPacket(packet));

        Assert.Equal(0, allocated);
    }
}
