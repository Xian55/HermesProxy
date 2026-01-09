using System;
using BenchmarkDotNet.Attributes;
using Framework.Util;

namespace HermesProxy.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class ExtensionsCombineBenchmarks
{
    private byte[] _smallData = null!;
    private byte[] _smallArr1 = null!;
    private byte[] _smallArr2 = null!;

    private byte[] _mediumData = null!;
    private byte[] _mediumArr1 = null!;
    private byte[] _mediumArr2 = null!;
    private byte[] _mediumArr3 = null!;

    private byte[] _largeData = null!;
    private byte[] _largeArr1 = null!;
    private byte[] _largeArr2 = null!;

    // Typical crypto usage scenario
    private byte[] _counterBytes = null!;
    private byte[] _magicBytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);

        // Small: 8 + 4 bytes (typical crypto nonce construction)
        _smallData = new byte[8];
        _smallArr1 = new byte[4];
        _smallArr2 = new byte[4];
        rng.NextBytes(_smallData);
        rng.NextBytes(_smallArr1);
        rng.NextBytes(_smallArr2);

        // Medium: 100 + 200 + 300 + 400 bytes
        _mediumData = new byte[100];
        _mediumArr1 = new byte[200];
        _mediumArr2 = new byte[300];
        _mediumArr3 = new byte[400];
        rng.NextBytes(_mediumData);
        rng.NextBytes(_mediumArr1);
        rng.NextBytes(_mediumArr2);
        rng.NextBytes(_mediumArr3);

        // Large: 1KB + 4KB + 4KB
        _largeData = new byte[1024];
        _largeArr1 = new byte[4096];
        _largeArr2 = new byte[4096];
        rng.NextBytes(_largeData);
        rng.NextBytes(_largeArr1);
        rng.NextBytes(_largeArr2);

        // Crypto scenario: BitConverter.GetBytes(counter).Combine(BitConverter.GetBytes(magic))
        _counterBytes = BitConverter.GetBytes(12345678UL);
        _magicBytes = BitConverter.GetBytes(0x52565253);
    }

    // ========== Small (Crypto nonce: 8 + 4 + 4 = 16 bytes) ==========

    [Benchmark(Baseline = true)]
    public byte[] Small_Original()
    {
        return _smallData.CombineOriginal(_smallArr1, _smallArr2);
    }

    [Benchmark]
    public byte[] Small_Optimized()
    {
        return _smallData.Combine(_smallArr1, _smallArr2);
    }

    // ========== Medium (100 + 200 + 300 + 400 = 1000 bytes) ==========

    [Benchmark]
    public byte[] Medium_Original()
    {
        return _mediumData.CombineOriginal(_mediumArr1, _mediumArr2, _mediumArr3);
    }

    [Benchmark]
    public byte[] Medium_Optimized()
    {
        return _mediumData.Combine(_mediumArr1, _mediumArr2, _mediumArr3);
    }

    // ========== Large (1KB + 4KB + 4KB = 9KB) ==========

    [Benchmark]
    public byte[] Large_Original()
    {
        return _largeData.CombineOriginal(_largeArr1, _largeArr2);
    }

    [Benchmark]
    public byte[] Large_Optimized()
    {
        return _largeData.Combine(_largeArr1, _largeArr2);
    }

    // ========== Crypto scenario (typical PacketCrypt usage) ==========

    [Benchmark]
    public byte[] Crypto_Original()
    {
        return _counterBytes.CombineOriginal(_magicBytes);
    }

    [Benchmark]
    public byte[] Crypto_Optimized()
    {
        return _counterBytes.Combine(_magicBytes);
    }
}
