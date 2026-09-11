using System;
using System.Collections;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;

namespace HermesProxy.Benchmarks;

// The V1_14/V2_5 flat update-field path, side by side with the BitArray-backed UpdateMask it
// replaced (verbatim copies at the bottom of this file). Each iteration is what a cached-object
// Values update does: clear the mask, dirty a few fields, serialize.
// FieldCount covers the V1_14_1 Item (80), Unit (218) and ActivePlayer (4674) arrays.
[MemoryDiagnoser]
[ShortRunJob]
public class UpdateFieldsWriteBenchmarks
{
    private const int DirtyFields = 16;

    [Params(80, 218, 4674)]
    public int FieldCount;

    private int[] _dirty = null!;
    private UpdateFieldsArray _fields = null!;
    private LegacyUpdateFieldsArray _legacy = null!;
    private ByteBuffer _packet = null!;

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);
        _dirty = Enumerable.Range(0, FieldCount).OrderBy(_ => rng.Next()).Take(DirtyFields).Order().ToArray();
        _fields = new UpdateFieldsArray((uint)FieldCount);
        _legacy = new LegacyUpdateFieldsArray((uint)FieldCount);
        foreach (var index in _dirty)
        {
            _fields.m_updateValues[index].UnsignedValue = (uint)index * 7 + 1;
            _legacy.Values[index].UnsignedValue = (uint)index * 7 + 1;
        }
        _packet = new ByteBuffer();
    }

    [GlobalCleanup]
    public void Cleanup() => _packet.Dispose();

    [Benchmark(Baseline = true)]
    public uint Legacy_ClearSetWrite()
    {
        _packet.Clear();
        _legacy.Mask.Clear();
        foreach (var index in _dirty)
            _legacy.Mask.SetBit(index);
        _legacy.WriteToPacket(_packet);
        return _packet.GetSize();
    }

    [Benchmark]
    public uint ClearSetWrite()
    {
        _packet.Clear();
        _fields.m_updateMask.Clear();
        foreach (var index in _dirty)
            _fields.m_updateMask.SetBit(index);
        _fields.WriteToPacket(_packet);
        return _packet.GetSize();
    }
}

// Every V1_14/V2_5 ObjectUpdateBuilder constructs a DynamicUpdateFieldsArray and serializes it.
// None is the common case (no dynamic field touched); ChannelObject is the WowGuid128 generic
// path (UNIT_DYNAMIC_FIELD_CHANNEL_OBJECTS); Gems is the largest array the builders write (30).
[MemoryDiagnoser]
[ShortRunJob]
public class DynamicUpdateFieldsBenchmarks
{
    public enum Scenario { None, ChannelObject, Gems }

    [Params(Scenario.None, Scenario.ChannelObject, Scenario.Gems)]
    public Scenario Case;

    private readonly WowGuid128 _guid = new(0x1122334455667788, 0xAABBCCDDEEFF0011);
    private uint[] _gems = null!;
    private ByteBuffer _packet = null!;

    [GlobalSetup]
    public void Setup()
    {
        _gems = new uint[30];
        _gems[0] = 41401;
        _gems[10] = 41398;
        _gems[20] = 40111;
        _packet = new ByteBuffer();
    }

    [GlobalCleanup]
    public void Cleanup() => _packet.Dispose();

    [Benchmark(Baseline = true)]
    public uint Legacy_BuildAndWrite()
    {
        _packet.Clear();
        var fields = new LegacyDynamicUpdateFieldsArray(18, UpdateTypeModern.Values);
        switch (Case)
        {
            case Scenario.ChannelObject:
                fields.SetUpdateField(2, _guid, DynamicFieldChangeType.ValueAndSizeChanged);
                break;
            case Scenario.Gems:
                fields.SetUpdateField(3, _gems, DynamicFieldChangeType.ValueAndSizeChanged);
                break;
        }
        fields.WriteToPacket(_packet);
        return _packet.GetSize();
    }

    [Benchmark]
    public uint BuildAndWrite()
    {
        _packet.Clear();
        var fields = new DynamicUpdateFieldsArray(18, UpdateTypeModern.Values);
        switch (Case)
        {
            case Scenario.ChannelObject:
                fields.SetUpdateField<WowGuid128>(2, _guid, DynamicFieldChangeType.ValueAndSizeChanged);
                break;
            case Scenario.Gems:
                fields.SetUpdateField(3, _gems, DynamicFieldChangeType.ValueAndSizeChanged);
                break;
        }
        fields.WriteToPacket(_packet);
        return _packet.GetSize();
    }
}

// Verbatim copies of the pre-change UpdateMask, DynamicUpdateMask and the two write paths that
// used them; only the WowGuid128 branch of the generic dynamic setter is kept.
internal class LegacyUpdateMask
{
    public LegacyUpdateMask(uint valuesCount = 0)
    {
        _fieldCount = valuesCount;
        _blockCount = (valuesCount + 32 - 1) / 32;

        _mask = new BitArray((int)valuesCount, false);
    }

    public void SetCount(int valuesCount)
    {
        _fieldCount = (uint)valuesCount;
        _blockCount = (uint)(valuesCount + 32 - 1) / 32;

        _mask = new BitArray(valuesCount, false);
    }

    public virtual void AppendToPacket(ByteBuffer data)
    {
        data.WriteUInt8((byte)_blockCount);
        var maskArray = new byte[_blockCount << 2];

        _mask.CopyTo(maskArray, 0);
        data.WriteBytes(maskArray);
    }

    public bool GetBit(int index) => _mask.Get(index);

    public void SetBit(int index) => _mask.Set(index, true);

    public void Clear() => _mask.SetAll(false);

    uint _fieldCount;
    protected uint _blockCount;
    protected BitArray _mask;
}

internal sealed class LegacyDynamicUpdateMask : LegacyUpdateMask
{
    public LegacyDynamicUpdateMask(uint valuesCount) : base(valuesCount) { }

    public void EncodeDynamicFieldChangeType(DynamicFieldChangeType changeType, UpdateTypeModern updateType)
    {
        DynamicFieldChangeType = (uint)(_blockCount | ((uint)(changeType & HermesProxy.World.Objects.DynamicFieldChangeType.ValueAndSizeChanged) * ((3 - (int)updateType) / 3)));
    }

    public override void AppendToPacket(ByteBuffer data)
    {
        data.WriteUInt16((ushort)DynamicFieldChangeType);
        if (ValueCount != null)
            data.WriteInt32((int)ValueCount);

        var maskArray = new byte[_blockCount << 2];

        _mask.CopyTo(maskArray, 0);
        data.WriteBytes(maskArray);
    }

    public uint DynamicFieldChangeType;
    public int? ValueCount;
}

internal sealed class LegacyUpdateFieldsArray
{
    public LegacyUpdateFieldsArray(uint size)
    {
        ValuesCount = size;
        Values = new UpdateValues[size];
        Mask = new LegacyUpdateMask(size);
    }

    public readonly uint ValuesCount;
    public readonly UpdateValues[] Values;
    public readonly LegacyUpdateMask Mask;

    public void WriteToPacket(ByteBuffer buffer)
    {
        var fieldBuffer = new ByteBuffer();
        for (var index = 0; index < ValuesCount; ++index)
        {
            if (Mask.GetBit(index))
            {
                fieldBuffer.WriteUInt32(Values[index].UnsignedValue);
            }
        }
        Mask.AppendToPacket(buffer);
        buffer.WriteBytes(fieldBuffer);
    }
}

internal sealed class LegacyDynamicUpdateFieldsArray
{
    public LegacyDynamicUpdateFieldsArray(uint size, UpdateTypeModern updateType)
    {
        m_updateType = updateType;
        m_updateMask = new LegacyUpdateMask(size);
        m_fieldBuffer = new();
    }

    readonly UpdateTypeModern m_updateType;
    readonly LegacyUpdateMask m_updateMask;
    readonly ByteBuffer m_fieldBuffer;

    public void WriteToPacket(ByteBuffer buffer)
    {
        m_updateMask.AppendToPacket(buffer);
        buffer.WriteBytes(m_fieldBuffer);
    }

    public void SetUpdateField(int index, uint[] values, DynamicFieldChangeType changeType)
    {
        var valueBuffer = new ByteBuffer();
        m_updateMask.SetBit(index);

        var arrayMask = new LegacyDynamicUpdateMask((uint)values.Length);
        arrayMask.EncodeDynamicFieldChangeType(changeType, m_updateType);
        if (m_updateType == UpdateTypeModern.Values && changeType == DynamicFieldChangeType.ValueAndSizeChanged)
        {
            arrayMask.ValueCount = values.Length;
            arrayMask.SetCount(values.Length);
        }

        for (var v = 0; v < values.Length; ++v)
        {
            arrayMask.SetBit(v);
            valueBuffer.WriteUInt32(values[v]);
        }

        arrayMask.AppendToPacket(m_fieldBuffer);
        m_fieldBuffer.WriteBytes(valueBuffer);
    }

    public void SetUpdateField(object index, WowGuid128 guid, DynamicFieldChangeType changeType)
    {
        uint[] values = new uint[4];
        values[0] = (uint)guid.GetLowValue();
        values[1] = (uint)(guid.GetLowValue() >> 32);
        values[2] = (uint)guid.GetHighValue();
        values[3] = (uint)(guid.GetHighValue() >> 32);
        SetUpdateField((int)index, values, changeType);
    }
}
