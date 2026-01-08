using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using Bgs.Protocol;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BNetServer.Networking;
using Google.Protobuf;

namespace HermesProxy.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class BnetPacketParserBenchmarks
{
    private byte[] _smallPacket = null!;
    private byte[] _mediumPacket = null!;
    private byte[] _largePacket = null!;
    private byte[] _multiPacket = null!;

    [GlobalSetup]
    public void Setup()
    {
        _smallPacket = CreateValidPacket(serviceHash: 0x12345678, methodId: 1, token: 100, payloadSize: 16);
        _mediumPacket = CreateValidPacket(serviceHash: 0x12345678, methodId: 1, token: 100, payloadSize: 256);
        _largePacket = CreateValidPacket(serviceHash: 0x12345678, methodId: 1, token: 100, payloadSize: 4096);

        // Create a buffer with multiple packets
        var packet1 = CreateValidPacket(serviceHash: 0x11111111, methodId: 1, token: 1, payloadSize: 64);
        var packet2 = CreateValidPacket(serviceHash: 0x22222222, methodId: 2, token: 2, payloadSize: 128);
        var packet3 = CreateValidPacket(serviceHash: 0x33333333, methodId: 3, token: 3, payloadSize: 64);
        _multiPacket = new byte[packet1.Length + packet2.Length + packet3.Length];
        Array.Copy(packet1, 0, _multiPacket, 0, packet1.Length);
        Array.Copy(packet2, 0, _multiPacket, packet1.Length, packet2.Length);
        Array.Copy(packet3, 0, _multiPacket, packet1.Length + packet2.Length, packet3.Length);
    }

    private static byte[] CreateValidPacket(uint serviceHash, uint methodId, uint token, int payloadSize)
    {
        var header = new Header
        {
            ServiceId = 1,
            ServiceHash = serviceHash,
            MethodId = methodId,
            Token = token,
            Size = (uint)payloadSize
        };

        var headerBytes = header.ToByteArray();
        var headerLength = (ushort)headerBytes.Length;

        var packet = new byte[2 + headerLength + payloadSize];
        BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(0, 2), headerLength);
        Array.Copy(headerBytes, 0, packet, 2, headerLength);

        for (int i = 0; i < payloadSize; i++)
        {
            packet[2 + headerLength + i] = (byte)(i & 0xFF);
        }

        return packet;
    }

    // ========== Small Packet (16 bytes payload) ==========

    [Benchmark(Baseline = true)]
    public bool SmallPacket_Original()
    {
        var buffer = new List<byte>(_smallPacket);
        var result = BnetPacketParser.ParseFromListOriginal(buffer);
        return result.Success;
    }

    [Benchmark]
    public bool SmallPacket_Optimized()
    {
        var buffer = new List<byte>(_smallPacket);
        var result = BnetPacketParser.ParseFromListOptimized(buffer);
        return result.Success;
    }

    // ========== Medium Packet (256 bytes payload) ==========

    [Benchmark]
    public bool MediumPacket_Original()
    {
        var buffer = new List<byte>(_mediumPacket);
        var result = BnetPacketParser.ParseFromListOriginal(buffer);
        return result.Success;
    }

    [Benchmark]
    public bool MediumPacket_Optimized()
    {
        var buffer = new List<byte>(_mediumPacket);
        var result = BnetPacketParser.ParseFromListOptimized(buffer);
        return result.Success;
    }

    // ========== Large Packet (4096 bytes payload) ==========

    [Benchmark]
    public bool LargePacket_Original()
    {
        var buffer = new List<byte>(_largePacket);
        var result = BnetPacketParser.ParseFromListOriginal(buffer);
        return result.Success;
    }

    [Benchmark]
    public bool LargePacket_Optimized()
    {
        var buffer = new List<byte>(_largePacket);
        var result = BnetPacketParser.ParseFromListOptimized(buffer);
        return result.Success;
    }

    // ========== Multi-Packet Processing (simulates real usage) ==========

    [Benchmark]
    public int MultiPacket_Original()
    {
        var buffer = new List<byte>(_multiPacket);
        int packetsProcessed = 0;

        while (buffer.Count > 2)
        {
            var result = BnetPacketParser.ParseFromListOriginal(buffer);
            if (!result.Success)
                break;

            buffer.RemoveRange(0, result.TotalLength);
            packetsProcessed++;
        }

        return packetsProcessed;
    }

    [Benchmark]
    public int MultiPacket_Optimized()
    {
        var buffer = new List<byte>(_multiPacket);
        int packetsProcessed = 0;

        while (buffer.Count > 2)
        {
            var result = BnetPacketParser.ParseFromListOptimized(buffer);
            if (!result.Success)
                break;

            buffer.RemoveRange(0, result.TotalLength);
            packetsProcessed++;
        }

        return packetsProcessed;
    }
}
