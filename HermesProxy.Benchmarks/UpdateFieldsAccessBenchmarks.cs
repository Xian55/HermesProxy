using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Framework.Logging;
using HermesProxy.World;
using HermesProxy.World.Objects;

namespace HermesProxy.Benchmarks;

// UpdateFieldsArray as the V1_14 and V2_5 builders and QuestSystem use it, against a verbatim
// LegacyUpdateFieldsArray that took its index as object. One group per use, each with its own baseline.
//
//   Get*            one read, the index an enum at the call site
//   SetUInt32       one write that changes the field
//   ValuesBlock     a builder filling 1000 fields of mixed types
//   QuestGiverScan  QuestSystem reading the NPC flags of 200 cached objects
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class UpdateFieldsAccessBenchmarks
{
    private enum Field
    {
        Bytes = 10,
        Flags = 11,
        Guid = 20,
        Money = 30,
    }

    private const int BlockFields = 1000;
    private const int CachedObjects = 200;
    private const int NpcFlagsIndex = 82;

    private UpdateFieldsArray _fields = null!;
    private LegacyUpdateFieldsArray _legacy = null!;
    private UpdateFieldsArray[] _cache = null!;
    private LegacyUpdateFieldsArray[] _legacyCache = null!;
    private uint _value;

    [GlobalSetup]
    public void Setup()
    {
        _fields = new UpdateFieldsArray(BlockFields + 4);
        _legacy = new LegacyUpdateFieldsArray(BlockFields + 4);

        foreach (var target in (ReadOnlySpan<int>)[(int)Field.Bytes, (int)Field.Flags, (int)Field.Guid, (int)Field.Money])
        {
            _fields.m_updateValues[target].UnsignedValue = 0x11223344;
            _legacy.m_updateValues[target].UnsignedValue = 0x11223344;
        }

        _cache = new UpdateFieldsArray[CachedObjects];
        _legacyCache = new LegacyUpdateFieldsArray[CachedObjects];
        for (int i = 0; i < CachedObjects; i++)
        {
            _cache[i] = new UpdateFieldsArray(NpcFlagsIndex + 1);
            _legacyCache[i] = new LegacyUpdateFieldsArray(NpcFlagsIndex + 1);
            _cache[i].m_updateValues[NpcFlagsIndex].UnsignedValue = (uint)i;
            _legacyCache[i].m_updateValues[NpcFlagsIndex].UnsignedValue = (uint)i;
        }
    }

    [BenchmarkCategory("GetUInt32"), Benchmark(Baseline = true)]
    public uint GetUInt32_Legacy() => _legacy.GetUpdateField<uint>(Field.Flags);

    [BenchmarkCategory("GetUInt32"), Benchmark]
    public uint GetUInt32() => _fields.GetUpdateField<uint>((int)Field.Flags);

    [BenchmarkCategory("GetByte"), Benchmark(Baseline = true)]
    public byte GetByte_Legacy() => _legacy.GetUpdateField<byte>(Field.Bytes, 2);

    [BenchmarkCategory("GetByte"), Benchmark]
    public byte GetByte() => _fields.GetUpdateField<byte>((int)Field.Bytes, 2);

    [BenchmarkCategory("GetUInt64"), Benchmark(Baseline = true)]
    public ulong GetUInt64_Legacy() => _legacy.GetUpdateField<ulong>(Field.Money);

    [BenchmarkCategory("GetUInt64"), Benchmark]
    public ulong GetUInt64() => _fields.GetUpdateField<ulong>((int)Field.Money);

    [BenchmarkCategory("GetGuid"), Benchmark(Baseline = true)]
    public WowGuid128 GetGuid_Legacy() => _legacy.GetUpdateField<WowGuid128>(Field.Guid);

    [BenchmarkCategory("GetGuid"), Benchmark]
    public WowGuid128 GetGuid() => _fields.GetUpdateField<WowGuid128>((int)Field.Guid);

    [BenchmarkCategory("SetUInt32"), Benchmark(Baseline = true)]
    public void SetUInt32_Legacy() => _legacy.SetUpdateField<uint>(Field.Flags, ++_value);

    [BenchmarkCategory("SetUInt32"), Benchmark]
    public void SetUInt32() => _fields.SetUpdateField<uint>((int)Field.Flags, ++_value);

    [BenchmarkCategory("ValuesBlock"), Benchmark(Baseline = true)]
    public void ValuesBlock_Legacy()
    {
        uint value = ++_value;
        for (int i = 0; i < BlockFields; i += 4)
        {
            _legacy.SetUpdateField<uint>(i, value);
            _legacy.SetUpdateField<int>(i + 1, (int)value);
            _legacy.SetUpdateField<float>(i + 2, value);
            _legacy.SetUpdateField<byte>(i + 3, (byte)value, 1);
        }
    }

    [BenchmarkCategory("ValuesBlock"), Benchmark]
    public void ValuesBlock()
    {
        uint value = ++_value;
        for (int i = 0; i < BlockFields; i += 4)
        {
            _fields.SetUpdateField<uint>(i, value);
            _fields.SetUpdateField<int>(i + 1, (int)value);
            _fields.SetUpdateField<float>(i + 2, value);
            _fields.SetUpdateField<byte>(i + 3, (byte)value, 1);
        }
    }

    [BenchmarkCategory("QuestGiverScan"), Benchmark(Baseline = true)]
    public int QuestGiverScan_Legacy()
    {
        int found = 0;
        foreach (var fields in _legacyCache)
        {
            if (fields.GetUpdateField<uint>(NpcFlagsIndex).HasAnyFlag(2u))
                found++;
        }
        return found;
    }

    [BenchmarkCategory("QuestGiverScan"), Benchmark]
    public int QuestGiverScan()
    {
        int found = 0;
        foreach (var fields in _cache)
        {
            if (fields.GetUpdateField<uint>(NpcFlagsIndex).HasAnyFlag(2u))
                found++;
        }
        return found;
    }

    private sealed class LegacyUpdateFieldsArray
    {
        public LegacyUpdateFieldsArray(uint size)
        {
            m_updateValues = new UpdateValues[size];
            m_updateMask = new UpdateMask(size);
        }

        public readonly UpdateValues[] m_updateValues;
        public readonly UpdateMask m_updateMask;

        public void SetUpdateField<T>(object index, T value, byte offset = 0) where T : new()
        {
            if (value is byte byteValue)
            {
                if (offset > 3)
                {
                    Log.Print(LogType.Error, $"SetUpdateField<UInt8>: Wrong offset: {offset}");
                    return;
                }

                if ((byte)(m_updateValues[(int)index].UnsignedValue >> (offset * 8)) != byteValue)
                {
                    m_updateValues[(int)index].UnsignedValue &= ~(uint)(0xFF << (offset * 8));
                    m_updateValues[(int)index].UnsignedValue |= (uint)byteValue << (offset * 8);
                    m_updateMask.SetBit((int)index);
                }
            }
            else if (value is int intValue)
            {
                if (m_updateValues[(int)index].SignedValue != intValue)
                {
                    m_updateValues[(int)index].SignedValue = intValue;
                    m_updateMask.SetBit((int)index);
                }
            }
            else if (value is uint uintValue)
            {
                if (m_updateValues[(int)index].UnsignedValue != uintValue)
                {
                    m_updateValues[(int)index].UnsignedValue = uintValue;
                    m_updateMask.SetBit((int)index);
                }
            }
            else if (value is float floatValue)
            {
                if (m_updateValues[(int)index].FloatValue != floatValue)
                {
                    m_updateValues[(int)index].FloatValue = floatValue;
                    m_updateMask.SetBit((int)index);
                }
            }
            else
                throw new Exception($"Unhandled type {typeof(T).Name} in SetUpdateField!");
        }

        public T GetUpdateField<T>(object index, byte offset = 0)
        {
            int idx = (int)index;
            return default(T) switch
            {
                byte => (T)(object)(byte)((m_updateValues[idx].UnsignedValue >> (offset * 8)) & 0xFF),
                ushort => (T)(object)(ushort)((m_updateValues[idx].UnsignedValue >> (offset * 16)) & 0xFFFF),
                int => (T)(object)m_updateValues[idx].SignedValue,
                uint => (T)(object)m_updateValues[idx].UnsignedValue,
                float => (T)(object)m_updateValues[idx].FloatValue,
                ulong => (T)(object)((ulong)m_updateValues[idx + 1].UnsignedValue << 32 | m_updateValues[idx].UnsignedValue),
                WowGuid128 => (T)(object)GetUpdateFieldGuid(idx),
                _ => throw new Exception($"{typeof(T).Name} is not implemented in GetUpdateField<T>"),
            };
        }

        public WowGuid128 GetUpdateFieldGuid(object index)
        {
            int idx = (int)index;
            ulong low = (ulong)m_updateValues[idx + 1].UnsignedValue << 32 | m_updateValues[idx].UnsignedValue;
            ulong high = (ulong)m_updateValues[idx + 3].UnsignedValue << 32 | m_updateValues[idx + 2].UnsignedValue;
            return new WowGuid128(low, high);
        }
    }
}
