using System;
using BenchmarkDotNet.Attributes;
using Framework.IO;
using Framework.GameMath;

namespace HermesProxy.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class SpanPacketWriterBenchmarks
{
    private byte[] _buffer = null!;

    [GlobalSetup]
    public void Setup()
    {
        _buffer = new byte[256];
    }

    // ========== Simple Write: Int64 ==========

    [Benchmark(Baseline = true)]
    public byte[] WriteInt64_ByteBuffer()
    {
        using var buffer = new ByteBuffer();
        buffer.WriteInt64(0x123456789ABCDEF0);
        return buffer.GetData();
    }

    [Benchmark]
    public int WriteInt64_SpanWriter()
    {
        var writer = new SpanPacketWriter(_buffer);
        writer.WriteInt64(0x123456789ABCDEF0);
        return writer.Position;
    }

    // ========== Vector3 Write ==========

    [Benchmark]
    public byte[] WriteVector3_ByteBuffer()
    {
        using var buffer = new ByteBuffer();
        buffer.WriteVector3(new Vector3(100.5f, 200.5f, 300.5f));
        return buffer.GetData();
    }

    [Benchmark]
    public int WriteVector3_SpanWriter()
    {
        var writer = new SpanPacketWriter(_buffer);
        writer.WriteVector3(new Vector3(100.5f, 200.5f, 300.5f));
        return writer.Position;
    }

    // ========== Mixed Write (simulates small packet) ==========

    [Benchmark]
    public byte[] WriteMixed_ByteBuffer()
    {
        using var buffer = new ByteBuffer();
        buffer.WriteUInt32(0xDEADBEEF);
        buffer.WriteVector3(new Vector3(1.0f, 2.0f, 3.0f));
        buffer.WriteBit(true);
        buffer.WriteBits(0b101, 3);
        buffer.FlushBits();
        buffer.WriteUInt8(0xFF);
        return buffer.GetData();
    }

    [Benchmark]
    public int WriteMixed_SpanWriter()
    {
        var writer = new SpanPacketWriter(_buffer);
        writer.WriteUInt32(0xDEADBEEF);
        writer.WriteVector3(new Vector3(1.0f, 2.0f, 3.0f));
        writer.WriteBit(true);
        writer.WriteBits(0b101, 3);
        writer.FlushBits();
        writer.WriteUInt8(0xFF);
        return writer.Position;
    }
}

[MemoryDiagnoser]
[ShortRunJob]
public class SpanPacketReaderBenchmarks
{
    private byte[] _int64Data = null!;
    private byte[] _vector3Data = null!;
    private byte[] _cstringData = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Prepare test data using ByteBuffer
        using var intBuffer = new ByteBuffer();
        intBuffer.WriteInt64(0x123456789ABCDEF0);
        _int64Data = intBuffer.GetData();

        using var vecBuffer = new ByteBuffer();
        vecBuffer.WriteVector3(new Vector3(100.5f, 200.5f, 300.5f));
        _vector3Data = vecBuffer.GetData();

        using var strBuffer = new ByteBuffer();
        strBuffer.WriteCString("TestPlayerName");
        _cstringData = strBuffer.GetData();
    }

    // ========== Read Int64 ==========

    [Benchmark(Baseline = true)]
    public long ReadInt64_ByteBuffer()
    {
        var buffer = new ByteBuffer(_int64Data);
        return buffer.ReadInt64();
    }

    [Benchmark]
    public long ReadInt64_SpanReader()
    {
        var reader = new SpanPacketReader(_int64Data);
        return reader.ReadInt64();
    }

    // ========== Read Vector3 ==========

    [Benchmark]
    public Vector3 ReadVector3_ByteBuffer()
    {
        var buffer = new ByteBuffer(_vector3Data);
        return buffer.ReadVector3();
    }

    [Benchmark]
    public Vector3 ReadVector3_SpanReader()
    {
        var reader = new SpanPacketReader(_vector3Data);
        return reader.ReadVector3();
    }

    // ========== Read CString ==========

    [Benchmark]
    public string ReadCString_ByteBuffer()
    {
        var buffer = new ByteBuffer(_cstringData);
        return buffer.ReadCString();
    }

    [Benchmark]
    public string ReadCString_SpanReader()
    {
        var reader = new SpanPacketReader(_cstringData);
        return reader.ReadCString();
    }
}

[MemoryDiagnoser]
[ShortRunJob]
public class PilotPacketBenchmarks
{
    private byte[] _buffer = null!;

    [GlobalSetup]
    public void Setup()
    {
        _buffer = new byte[64];
    }

    // ========== ServerTimeOffset (8 bytes) ==========

    [Benchmark(Baseline = true)]
    public byte[] ServerTimeOffset_ByteBuffer()
    {
        using var buffer = new ByteBuffer();
        buffer.WriteInt64(1234567890123456789L);
        return buffer.GetData();
    }

    [Benchmark]
    public int ServerTimeOffset_SpanWriter()
    {
        var writer = new SpanPacketWriter(_buffer);
        writer.WriteInt64(1234567890123456789L);
        return writer.Position;
    }

    // ========== BindPointUpdate (20 bytes) ==========

    [Benchmark]
    public byte[] BindPointUpdate_ByteBuffer()
    {
        using var buffer = new ByteBuffer();
        buffer.WriteVector3(new Vector3(100.5f, 200.5f, 300.5f));
        buffer.WriteUInt32(0); // MapID
        buffer.WriteUInt32(1); // AreaID
        return buffer.GetData();
    }

    [Benchmark]
    public int BindPointUpdate_SpanWriter()
    {
        var writer = new SpanPacketWriter(_buffer);
        writer.WriteVector3(new Vector3(100.5f, 200.5f, 300.5f));
        writer.WriteUInt32(0); // MapID
        writer.WriteUInt32(1); // AreaID
        return writer.Position;
    }
}
