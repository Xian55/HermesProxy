using System;
using System.Text;
using BenchmarkDotNet.Attributes;
using Framework.IO;

namespace HermesProxy.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class ByteBufferGetDataBenchmarks
{
    private ByteBuffer _smallBuffer = null!;
    private ByteBuffer _mediumBuffer = null!;
    private ByteBuffer _largeBuffer = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Small: 64 bytes (typical small packet)
        _smallBuffer = new ByteBuffer();
        var smallData = new byte[64];
        new Random(42).NextBytes(smallData);
        _smallBuffer.WriteBytes(smallData);

        // Medium: 1KB (typical medium packet)
        _mediumBuffer = new ByteBuffer();
        var mediumData = new byte[1024];
        new Random(42).NextBytes(mediumData);
        _mediumBuffer.WriteBytes(mediumData);

        // Large: 64KB (large packet/update)
        _largeBuffer = new ByteBuffer();
        var largeData = new byte[65536];
        new Random(42).NextBytes(largeData);
        _largeBuffer.WriteBytes(largeData);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _smallBuffer?.Dispose();
        _mediumBuffer?.Dispose();
        _largeBuffer?.Dispose();
    }

    // ========== Small Buffer (64 bytes) ==========

    [Benchmark(Baseline = true)]
    public byte[] Small_Original()
    {
        return _smallBuffer.GetDataOriginal();
    }

    [Benchmark]
    public byte[] Small_Optimized()
    {
        return _smallBuffer.GetData();
    }

    // ========== Medium Buffer (1KB) ==========

    [Benchmark]
    public byte[] Medium_Original()
    {
        return _mediumBuffer.GetDataOriginal();
    }

    [Benchmark]
    public byte[] Medium_Optimized()
    {
        return _mediumBuffer.GetData();
    }

    // ========== Large Buffer (64KB) ==========

    [Benchmark]
    public byte[] Large_Original()
    {
        return _largeBuffer.GetDataOriginal();
    }

    [Benchmark]
    public byte[] Large_Optimized()
    {
        return _largeBuffer.GetData();
    }
}

[MemoryDiagnoser]
[ShortRunJob]
public class ByteBufferReadCStringBenchmarks
{
    private byte[] _shortStringData = null!;
    private byte[] _mediumStringData = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Short string: "Hello" (5 chars)
        _shortStringData = CreateCStringData("Hello");

        // Medium string: 100 chars
        _mediumStringData = CreateCStringData(new string('A', 100));
    }

    private static byte[] CreateCStringData(string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        var data = new byte[bytes.Length + 1];
        bytes.CopyTo(data, 0);
        data[^1] = 0x00;
        return data;
    }

    // ========== Short String (5 chars) ==========

    [Benchmark(Baseline = true)]
    public string Short_Original()
    {
        using var buffer = new ByteBuffer(_shortStringData);
        return buffer.ReadCStringOriginal();
    }

    [Benchmark]
    public string Short_Optimized()
    {
        using var buffer = new ByteBuffer(_shortStringData);
        return buffer.ReadCString();
    }

    // ========== Medium String (100 chars) ==========

    [Benchmark]
    public string Medium_Original()
    {
        using var buffer = new ByteBuffer(_mediumStringData);
        return buffer.ReadCStringOriginal();
    }

    [Benchmark]
    public string Medium_Optimized()
    {
        using var buffer = new ByteBuffer(_mediumStringData);
        return buffer.ReadCString();
    }
}

// =====================================================================
// Write-side benchmarks for the ByteBuffer hot-path tightening port of
// SpanPacketWriter commit 27d7e52. Each benchmark class pairs the new
// optimized impl (Optimized) against the frozen-original baseline kept
// internally in ByteBuffer (Original) — same precedent as the existing
// ReadCStringOriginal / GetDataOriginal benchmark pattern.
// =====================================================================

[MemoryDiagnoser]
[ShortRunJob]
public class ByteBufferWriteBitsBenchmarks
{
    [Params(4, 6, 8, 9, 16, 24, 32)]
    public int BitWidth;

    private const int Iterations = 64;
    private uint _value;

    [GlobalSetup]
    public void Setup()
    {
        _value = BitWidth == 32 ? 0xCAFEBABEu : (1u << BitWidth) - 1u;
    }

    [Benchmark(Baseline = true)]
    public byte[] WriteBits_Original()
    {
        using var buffer = new ByteBuffer();
        for (int i = 0; i < Iterations; i++)
            buffer.WriteBitsOriginal(_value, BitWidth);
        buffer.FlushBits();
        return buffer.GetData();
    }

    [Benchmark]
    public byte[] WriteBits_Optimized()
    {
        using var buffer = new ByteBuffer();
        for (int i = 0; i < Iterations; i++)
            buffer.WriteBits(_value, BitWidth);
        buffer.FlushBits();
        return buffer.GetData();
    }
}

[MemoryDiagnoser]
[ShortRunJob]
public class ByteBufferWriteVectorBenchmarks
{
    private const int Iterations = 256;
    private static readonly Vector2 V2 = new(1.5f, -2.25f);
    private static readonly Vector3 V3 = new(1.5f, -2.25f, 0.125f);
    private static readonly Vector4 V4 = new(1.5f, -2.25f, 0.125f, 1024.0f);

    [Benchmark(Baseline = true)]
    public byte[] WriteVector2_Original()
    {
        using var buffer = new ByteBuffer();
        for (int i = 0; i < Iterations; i++)
            buffer.WriteVector2Original(V2);
        return buffer.GetData();
    }

    [Benchmark]
    public byte[] WriteVector2_Optimized()
    {
        using var buffer = new ByteBuffer();
        for (int i = 0; i < Iterations; i++)
            buffer.WriteVector2(V2);
        return buffer.GetData();
    }

    [Benchmark]
    public byte[] WriteVector3_Original()
    {
        using var buffer = new ByteBuffer();
        for (int i = 0; i < Iterations; i++)
            buffer.WriteVector3Original(V3);
        return buffer.GetData();
    }

    [Benchmark]
    public byte[] WriteVector3_Optimized()
    {
        using var buffer = new ByteBuffer();
        for (int i = 0; i < Iterations; i++)
            buffer.WriteVector3(V3);
        return buffer.GetData();
    }

    [Benchmark]
    public byte[] WriteVector4_Original()
    {
        using var buffer = new ByteBuffer();
        for (int i = 0; i < Iterations; i++)
            buffer.WriteVector4Original(V4);
        return buffer.GetData();
    }

    [Benchmark]
    public byte[] WriteVector4_Optimized()
    {
        using var buffer = new ByteBuffer();
        for (int i = 0; i < Iterations; i++)
            buffer.WriteVector4(V4);
        return buffer.GetData();
    }
}

[MemoryDiagnoser]
[ShortRunJob]
public class ByteBufferWriteCStringBenchmarks
{
    [Params("hello", "Player_Name_Goes_Here_Filling_Sixty_Four_Bytes_Or_Thereabouts!", "héllo wörld 你好")]
    public string Value = null!;

    [Benchmark(Baseline = true)]
    public byte[] WriteCString_Original()
    {
        using var buffer = new ByteBuffer();
        buffer.WriteCStringOriginal(Value);
        return buffer.GetData();
    }

    [Benchmark]
    public byte[] WriteCString_Optimized()
    {
        using var buffer = new ByteBuffer();
        buffer.WriteCString(Value);
        return buffer.GetData();
    }

    [Benchmark]
    public byte[] WriteCString_Empty_Original()
    {
        using var buffer = new ByteBuffer();
        buffer.WriteCStringOriginal(string.Empty);
        return buffer.GetData();
    }

    [Benchmark]
    public byte[] WriteCString_Empty_Optimized()
    {
        using var buffer = new ByteBuffer();
        buffer.WriteCString(string.Empty);
        return buffer.GetData();
    }
}

[MemoryDiagnoser]
[ShortRunJob]
public class ByteBufferWriteStringBenchmarks
{
    [Params("hello", "Player_Name_Goes_Here_Filling_Sixty_Four_Bytes_Or_Thereabouts!", "héllo wörld 你好")]
    public string Value = null!;

    [Benchmark(Baseline = true)]
    public byte[] WriteString_Original()
    {
        using var buffer = new ByteBuffer();
        buffer.WriteStringOriginal(Value);
        return buffer.GetData();
    }

    [Benchmark]
    public byte[] WriteString_Optimized()
    {
        using var buffer = new ByteBuffer();
        buffer.WriteString(Value);
        return buffer.GetData();
    }
}

[MemoryDiagnoser]
[ShortRunJob]
public class ByteBufferWriteBoolBenchmarks
{
    private const int Iterations = 1024;

    [Benchmark(Baseline = true)]
    public byte[] WriteBool_Original()
    {
        using var buffer = new ByteBuffer();
        for (int i = 0; i < Iterations; i++)
            buffer.WriteBoolOriginal((i & 1) == 0);
        return buffer.GetData();
    }

    [Benchmark]
    public byte[] WriteBool_Optimized()
    {
        using var buffer = new ByteBuffer();
        for (int i = 0; i < Iterations; i++)
            buffer.WriteBool((i & 1) == 0);
        return buffer.GetData();
    }
}

[MemoryDiagnoser]
[ShortRunJob]
public class ByteBufferWritePackXYZBenchmarks
{
    private const int Iterations = 1024;
    private static readonly Vector3 Pos = new(100.5f, -200.25f, 50.125f);

    [Benchmark(Baseline = true)]
    public byte[] WritePackXYZ_Original()
    {
        using var buffer = new ByteBuffer();
        for (int i = 0; i < Iterations; i++)
            buffer.WritePackXYZOriginal(Pos);
        return buffer.GetData();
    }

    [Benchmark]
    public byte[] WritePackXYZ_Optimized()
    {
        using var buffer = new ByteBuffer();
        for (int i = 0; i < Iterations; i++)
            buffer.WritePackXYZ(Pos);
        return buffer.GetData();
    }
}

[MemoryDiagnoser]
[ShortRunJob]
public class ByteBufferMixedWorkloadBenchmarks
{
    private const int Iterations = 32;
    private static readonly Vector3 Pos = new(100.5f, -200.25f, 50.125f);
    private const string Name = "TestPlayerName";
    private const string Title = "Defender of the Realm";

    // Approximates a small bit-packed packet: 4×WriteBits (typical mask widths)
    // + 2×WriteCString + 1×WriteVector3 + 8×WriteBool, repeated 32 times.
    [Benchmark(Baseline = true)]
    public byte[] Mixed_Original()
    {
        using var buffer = new ByteBuffer();
        for (int i = 0; i < Iterations; i++)
        {
            buffer.WriteBitsOriginal(0xAu, 4);
            buffer.WriteBitsOriginal(0x123u, 9);
            buffer.WriteBitsOriginal(0xCAFEu, 16);
            buffer.WriteBitsOriginal(0xFFFFFFu, 24);
            buffer.FlushBits();
            buffer.WriteCStringOriginal(Name);
            buffer.WriteCStringOriginal(Title);
            buffer.WriteVector3Original(Pos);
            for (int b = 0; b < 8; b++)
                buffer.WriteBoolOriginal((b & 1) == 0);
        }
        return buffer.GetData();
    }

    [Benchmark]
    public byte[] Mixed_Optimized()
    {
        using var buffer = new ByteBuffer();
        for (int i = 0; i < Iterations; i++)
        {
            buffer.WriteBits(0xAu, 4);
            buffer.WriteBits(0x123u, 9);
            buffer.WriteBits(0xCAFEu, 16);
            buffer.WriteBits(0xFFFFFFu, 24);
            buffer.FlushBits();
            buffer.WriteCString(Name);
            buffer.WriteCString(Title);
            buffer.WriteVector3(Pos);
            for (int b = 0; b < 8; b++)
                buffer.WriteBool((b & 1) == 0);
        }
        return buffer.GetData();
    }
}
