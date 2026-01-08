using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using Framework.IO;

namespace HermesProxy.Benchmarks;

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
