using BenchmarkDotNet.Attributes;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Objects;

namespace HermesProxy.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class UpdateFieldsArraySetBenchmarks
{
    private UpdateFieldsArray _fields = null!;

    [GlobalSetup]
    public void Setup()
    {
        _fields = new UpdateFieldsArray(128);
    }

    [Benchmark(Baseline = true)]
    public void SetUpdateField_UInt32()
    {
        _fields.SetUpdateField<uint>(0, 0xDEADBEEFu);
    }

    [Benchmark]
    public void SetUpdateField_Int32()
    {
        _fields.SetUpdateField<int>(1, -42);
    }

    [Benchmark]
    public void SetUpdateField_Float()
    {
        _fields.SetUpdateField<float>(2, 3.14159f);
    }

    [Benchmark]
    public void SetUpdateField_UInt64()
    {
        _fields.SetUpdateField<ulong>(4, 0x123456789ABCDEF0ul);
    }

    [Benchmark]
    public void SetUpdateField_WowGuid128_Generic()
    {
        _fields.SetUpdateField<WowGuid128>(8, new WowGuid128(0x1122334455667788, 0xAABBCCDDEEFF0011));
    }

    [Benchmark]
    public void SetUpdateField_WowGuid128_Direct()
    {
        _fields.SetUpdateField(8, new WowGuid128(0x1122334455667788, 0xAABBCCDDEEFF0011));
    }

    [Benchmark]
    public void SetUpdateField_ByteWithOffset()
    {
        _fields.SetUpdateField<byte>(12, 0xAB, 2);
    }

    [Benchmark]
    public void SetUpdateField_UShortWithOffset()
    {
        _fields.SetUpdateField<ushort>(13, 0xBEEF, 1);
    }
}

[MemoryDiagnoser]
[ShortRunJob]
public class UpdateFieldsArrayGetBenchmarks
{
    private UpdateFieldsArray _fields = null!;

    [GlobalSetup]
    public void Setup()
    {
        _fields = new UpdateFieldsArray(128);
        // Pre-populate with values
        _fields.SetUpdateField<uint>(0, 0xDEADBEEFu);
        _fields.SetUpdateField<int>(1, -42);
        _fields.SetUpdateField<float>(2, 3.14159f);
        _fields.SetUpdateField<ulong>(4, 0x123456789ABCDEF0ul);
        _fields.SetUpdateField<WowGuid128>(8, new WowGuid128(0x1122334455667788, 0xAABBCCDDEEFF0011));
    }

    [Benchmark(Baseline = true)]
    public uint GetUpdateField_UInt32()
    {
        return _fields.GetUpdateField<uint>(0);
    }

    [Benchmark]
    public int GetUpdateField_Int32()
    {
        return _fields.GetUpdateField<int>(1);
    }

    [Benchmark]
    public float GetUpdateField_Float()
    {
        return _fields.GetUpdateField<float>(2);
    }

    [Benchmark]
    public ulong GetUpdateField_UInt64()
    {
        return _fields.GetUpdateField<ulong>(4);
    }

    [Benchmark]
    public WowGuid128 GetUpdateField_WowGuid128_Generic()
    {
        return _fields.GetUpdateField<WowGuid128>(8);
    }

    [Benchmark]
    public WowGuid128 GetUpdateField_WowGuid128_Direct()
    {
        return _fields.GetUpdateFieldGuid(8);
    }
}

[MemoryDiagnoser]
[ShortRunJob]
public class UpdateFieldsArrayMixedBenchmarks
{
    private UpdateFieldsArray _fields = null!;

    [GlobalSetup]
    public void Setup()
    {
        _fields = new UpdateFieldsArray(256);
    }

    /// <summary>
    /// Simulates a realistic object update: set ~20 fields of mixed types, then serialize.
    /// This is the hot path for every object update packet.
    /// </summary>
    [Benchmark]
    public byte[] RealisticObjectUpdate()
    {
        // Typical unit update fields
        _fields.SetUpdateField<uint>(0, 1u);             // ObjectType
        _fields.SetUpdateField<float>(1, 100.0f);        // ScaleX
        _fields.SetUpdateField<ulong>(6, 0x12345678ul);  // Charm GUID low
        _fields.SetUpdateField<uint>(20, 35u);            // Level
        _fields.SetUpdateField<uint>(22, 1u);             // FactionTemplate
        _fields.SetUpdateField<uint>(36, 100u);           // Health
        _fields.SetUpdateField<uint>(37, 100u);           // MaxHealth
        _fields.SetUpdateField<uint>(38, 50u);            // Power
        _fields.SetUpdateField<uint>(39, 50u);            // MaxPower
        _fields.SetUpdateField<float>(44, 1.0f);          // MinDamage
        _fields.SetUpdateField<float>(45, 5.0f);          // MaxDamage
        _fields.SetUpdateField<byte>(46, 1, 0);           // StandState
        _fields.SetUpdateField<byte>(46, 0, 1);           // PetTalentPoints
        _fields.SetUpdateField<byte>(46, 0, 2);           // VisFlags
        _fields.SetUpdateField<byte>(46, 0, 3);           // AnimTier

        var buffer = new ByteBuffer();
        _fields.WriteToPacket(buffer);
        var data = buffer.GetData();
        buffer.Dispose();
        return data;
    }
}
