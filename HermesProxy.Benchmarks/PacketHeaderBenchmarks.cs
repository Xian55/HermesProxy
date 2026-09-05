using System;
using System.Buffers;
using System.Buffers.Binary;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Framework.Cryptography;
using Framework.IO;
using HermesProxy.World;

namespace HermesProxy.Benchmarks;

// PacketHeader: reference class with a heap byte[12] tag vs struct with an [InlineArray] tag.
//
// The header is touched three times per packet round trip — WorldSocket.ReadHeader and
// ReadData each build one on the way in, SendPacket builds one on the way out — so the
// per-instance cost (one object + one byte[12], both gen0 garbage) is paid on every packet.
//
// ClassPacketHeader below is a verbatim copy of the pre-refactor type, kept here only as
// the benchmark baseline; production code has the struct.
[MemoryDiagnoser]
[ShortRunJob]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class PacketHeaderBenchmarks
{
    private static readonly byte[] Key16 = "0123456789ABCDEF"u8.ToArray();

    private readonly byte[] _wireHeader = new byte[PacketHeader.StructSize];
    private byte[] _payload = null!;
    private WorldCrypt _crypt = null!;

    [GlobalSetup]
    public void Setup()
    {
        BinaryPrimitives.WriteInt32LittleEndian(_wireHeader, 512);
        for (int i = 0; i < PacketHeader.TagSize; i++)
            _wireHeader[sizeof(int) + i] = (byte)(0xA0 + i);

        _payload = new byte[512];
        Random.Shared.NextBytes(_payload);

        WorldCrypt.ForceBouncyCastleForTests = false;
        _crypt = new WorldCrypt();
        _crypt.Initialize(Key16);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _crypt.Dispose();
    }

    // ---- Parse: one header off the wire ----

    [BenchmarkCategory("Read"), Benchmark(Baseline = true)]
    public int Read_Class()
    {
        ClassPacketHeader header = new();
        header.Read(_wireHeader);
        return header.Size + header.Tag[11];
    }

    [BenchmarkCategory("Read"), Benchmark]
    public int Read_Struct()
    {
        PacketHeader header = new();
        header.Read(_wireHeader);
        return header.Size + header.Tag[11];
    }

    // ---- Inbound: ReadHeader + ReadData, exactly as WorldSocket does it (two headers) ----

    [BenchmarkCategory("Inbound"), Benchmark(Baseline = true)]
    public int Inbound_Class()
    {
        ClassPacketHeader sizeOnly = new();
        sizeOnly.Read(_wireHeader);

        ClassPacketHeader full = new();
        full.Read(_wireHeader);
        return sizeOnly.Size + full.Tag[0] + (full.IsValidSize() ? 1 : 0);
    }

    [BenchmarkCategory("Inbound"), Benchmark]
    public int Inbound_Struct()
    {
        PacketHeader sizeOnly = new();
        sizeOnly.Read(_wireHeader);

        PacketHeader full = new();
        full.Read(_wireHeader);
        return sizeOnly.Size + full.Tag[0] + (full.IsValidSize() ? 1 : 0);
    }

    // ---- Outbound: set Size, AES-GCM the body into the tag, frame header + body ----
    //
    // The class baseline is the old SendPacket tail verbatim: a ByteBuffer that rents a
    // buffer, then GetData() copies the whole frame out again. SpanFrame is one exact-sized
    // array with the header written straight into it; PooledFrame is what SendPacket does
    // now — same layout out of ArrayPool, legal because AsyncWrite is a blocking send.

    [BenchmarkCategory("Outbound"), Benchmark(Baseline = true)]
    public int Outbound_Class_ByteBuffer()
    {
        ClassPacketHeader header = new();
        header.Size = _payload.Length;
        _crypt.Encrypt(_payload, header.Tag);

        using ByteBuffer framed = new();
        header.Write(framed);
        framed.WriteBytes(_payload);
        return framed.GetData().Length;
    }

    [BenchmarkCategory("Outbound"), Benchmark]
    public int Outbound_Struct_PooledFrame()
    {
        PacketHeader header = new();
        header.Size = _payload.Length;
        _crypt.Encrypt(_payload, header.Tag);

        int framedSize = PacketHeader.StructSize + _payload.Length;
        byte[] framed = ArrayPool<byte>.Shared.Rent(framedSize);
        try
        {
            header.Write(framed);
            _payload.CopyTo(framed, PacketHeader.StructSize);
            return framedSize;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(framed);
        }
    }

    [BenchmarkCategory("Outbound"), Benchmark]
    public int Outbound_Struct_SpanFrame()
    {
        PacketHeader header = new();
        header.Size = _payload.Length;
        _crypt.Encrypt(_payload, header.Tag);

        byte[] framed = new byte[PacketHeader.StructSize + _payload.Length];
        header.Write(framed);
        _payload.CopyTo(framed, PacketHeader.StructSize);
        return framed.Length;
    }

}

// Pre-refactor PacketHeader, kept verbatim as the benchmark baseline.
internal class ClassPacketHeader
{
    public int Size;
    public byte[] Tag = new byte[12];

    public void Read(byte[] buffer)
    {
        Size = BinaryPrimitives.ReadInt32LittleEndian(buffer);
        Buffer.BlockCopy(buffer, 4, Tag, 0, 12);
    }

    public void Write(ByteBuffer byteBuffer)
    {
        byteBuffer.WriteInt32(Size);
        byteBuffer.WriteBytes(Tag, 12);
    }

    public bool IsValidSize() { return Size < 0x40000; }
}
