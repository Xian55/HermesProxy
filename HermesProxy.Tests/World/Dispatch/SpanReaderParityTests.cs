using System;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using Xunit;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// SpanPacketReader has no production call sites yet — the write side shipped, the read side
/// did not. Before packet bodies are converted onto it, every read they use has to agree with
/// the ByteBuffer/WorldPacket member it replaces, in value and in final position. ByteBuffer
/// is the oracle here; it is what ships today.
/// </summary>
public class SpanReaderParityTests
{
    /// Builds a payload and frames it the way the wire does. `WorldPacket(byte[])` is a
    /// read-mode ctor that consumes a 2-byte opcode prefix, but `GetData()` returns the payload
    /// without one — so the prefix has to be prepended or the WorldPacket reads two bytes into
    /// the payload while the span reader does not. Same framing as PacketDispatchBenchmarks.
    private static (WorldPacket Packet, byte[] Framed) Build(Action<WorldPacket> write)
    {
        using var w = new WorldPacket(1u);
        write(w);
        byte[] payload = w.GetData();
        byte[] framed = new byte[payload.Length + 2];
        payload.CopyTo(framed, 2);
        return (new WorldPacket(framed), framed);
    }

    private static SpanPacketReader ReaderOver(byte[] framed) => new(framed.AsSpan(2));

    [Fact]
    public void Integers_MatchByteBuffer()
    {
        var (p, framed) = Build(w =>
        {
            w.WriteUInt8(0xAB); w.WriteInt8(-42);
            w.WriteUInt16(0xBEEF); w.WriteInt16(-1234);
            w.WriteUInt32(0xDEADBEEF); w.WriteInt32(int.MinValue);
            w.WriteUInt64(ulong.MaxValue); w.WriteInt64(long.MinValue);
            w.WriteFloat(3.5f); w.WriteDouble(-2.25d);
        });
        var r = ReaderOver(framed);

        Assert.Equal(p.ReadUInt8(), r.ReadUInt8());
        Assert.Equal(p.ReadInt8(), r.ReadInt8());
        Assert.Equal(p.ReadUInt16(), r.ReadUInt16());
        Assert.Equal(p.ReadInt16(), r.ReadInt16());
        Assert.Equal(p.ReadUInt32(), r.ReadUInt32());
        Assert.Equal(p.ReadInt32(), r.ReadInt32());
        Assert.Equal(p.ReadUInt64(), r.ReadUInt64());
        Assert.Equal(p.ReadInt64(), r.ReadInt64());
        Assert.Equal(p.ReadFloat(), r.ReadFloat());
        Assert.Equal(p.ReadDouble(), r.ReadDouble());
        Assert.Equal(p.Remaining(), r.Remaining);   // both consumed the same bytes
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    [InlineData(0x00FF00FF00FF00FFUL)]
    [InlineData(ulong.MaxValue)]
    public void PackedGuid64_MatchesWorldPacket(ulong low)
    {
        var (p, framed) = Build(w => w.WritePackedGuid(new WowGuid64(low)));
        var r = ReaderOver(framed);
        Assert.Equal(p.ReadPackedGuid(), r.ReadPackedGuid());
        Assert.Equal(p.Remaining(), r.Remaining);   // both consumed the same bytes
    }

    [Theory]
    [InlineData(0UL, 0UL)]
    [InlineData(1UL, 0UL)]
    [InlineData(0xDEADBEEFUL, 0x0123456789ABCDEFUL)]
    [InlineData(ulong.MaxValue, ulong.MaxValue)]
    public void PackedGuid128_MatchesWorldPacket(ulong low, ulong high)
    {
        var (p, framed) = Build(w => w.WritePackedGuid128(new WowGuid128(low, high)));
        var r = ReaderOver(framed);
        Assert.Equal(p.ReadPackedGuid128(), r.ReadPackedGuid128());
        Assert.Equal(p.Remaining(), r.Remaining);   // both consumed the same bytes
    }

    [Fact]
    public void Guid64_And_Entry_MatchWorldPacket()
    {
        var (p, framed) = Build(w =>
        {
            w.WriteGuid(new WowGuid64(0x1122334455667788UL));
            w.WriteUInt32(0x80000000u | 4321u);   // invalid/GO-marker bit set
            w.WriteUInt32(4321u);                 // clear
        });
        var r = ReaderOver(framed);

        Assert.Equal(p.ReadGuid(), r.ReadGuid());
        Assert.Equal(p.ReadEntry(), r.ReadEntry());
        Assert.Equal(p.ReadEntry(), r.ReadEntry());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(31)]
    [InlineData(32)]
    public void Bits_MatchByteBuffer(int bitCount)
    {
        // ByteBuffer.ReadBits calls HasBit, SpanPacketReader.ReadBits calls ReadBit. They are
        // the same algorithm today; if either is ever "cleaned up" alone, ~97 call sites in the
        // modern packets misparse with no error. The trailing uint proves both readers land on
        // the same byte boundary, not just that they agree on the value.
        var (p, framed) = Build(w =>
        {
            w.WriteBits(0xA5A5A5A5u, bitCount);
            w.FlushBits();
            w.WriteUInt32(0xCAFEBABEu);
        });
        var r = ReaderOver(framed);

        Assert.Equal(p.ReadBits<uint>(bitCount), r.ReadBits<uint>(bitCount));
        Assert.Equal(p.ReadUInt32(), r.ReadUInt32());
    }

    [Fact]
    public void SingleBits_MatchByteBuffer()
    {
        var (p, framed) = Build(w =>
        {
            for (int i = 0; i < 11; i++)
                w.WriteBit(i % 3 == 0);
            w.FlushBits();
        });
        var r = ReaderOver(framed);

        for (int i = 0; i < 11; i++)
            Assert.Equal(p.HasBit(), r.HasBit());
    }

    [Theory]
    [InlineData("")]
    [InlineData("Dylua")]
    [InlineData("a much longer name with spaces")]
    public void Strings_MatchByteBuffer(string value)
    {
        var (p, framed) = Build(w => { w.WriteCString(value); w.WriteString(value); });
        var r = ReaderOver(framed);

        Assert.Equal(p.ReadCString(), r.ReadCString());
        Assert.Equal(p.ReadString((uint)value.Length), r.ReadString((uint)value.Length));
    }

    [Fact]
    public void TruncatedString_ClampsLikeByteBuffer_RatherThanThrowing()
    {
        // ByteBuffer clamps via ReadBytes. Slicing unclamped would turn a malformed packet
        // that used to yield a short string into an ArgumentOutOfRangeException — a behaviour
        // change the handler migration cannot absorb.
        var (p, framed) = Build(w => w.WriteBytes(new byte[] { 0x41, 0x42 }));
        var r = ReaderOver(framed);

        Assert.Equal(p.ReadString(64u), r.ReadString(64u));
    }

    [Fact]
    public void Vectors_And_PackedForms_MatchByteBuffer()
    {
        var (p, framed) = Build(w =>
        {
            w.WriteVector2(new Vector2(1.5f, -2.5f));
            w.WriteVector3(new Vector3(3f, 4f, 5f));
            w.WriteInt32(unchecked((int)0x12345678));   // ReadPackedVector3 payload
        });
        var r = ReaderOver(framed);

        Assert.Equal(p.ReadVector2(), r.ReadVector2());
        Assert.Equal(p.ReadVector3(), r.ReadVector3());
        Assert.Equal(p.ReadPackedVector3(), r.ReadPackedVector3());
    }

    [Fact]
    public void Bytes_CopyRatherThanAlias()
    {
        var (p, framed) = Build(w => w.WriteBytes(new byte[] { 1, 2, 3, 4 }));
        var r = ReaderOver(framed);

        byte[] expected = p.ReadBytes(4u);
        byte[] actual = r.ReadBytes(4u);
        Assert.Equal(expected, actual);

        // The array overload must own its storage: the span overload aliases a pooled rental
        // that the next packet reuses.
        Assert.NotSame(expected, actual);
    }
}
