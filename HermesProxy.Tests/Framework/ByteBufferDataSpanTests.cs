using System.Buffers;
using Framework.IO;
using HermesProxy.World;
using Xunit;

namespace HermesProxy.Tests.Framework;

/// <summary>
/// Cover for <see cref="ByteBuffer.GetDataSpan"/>, the length-honest counterpart to
/// <see cref="ByteBuffer.GetData"/>.
///
/// A received packet is read into an <see cref="ArrayPool{T}"/> rental, which is rounded up to a
/// bucket size, and the read-mode <c>GetData</c> returns that whole rental. Writing it to a sniff
/// therefore recorded the bucket, not the payload: a 42-byte <c>SMSG_AUTH_CHALLENGE</c> landed in
/// a 64-byte rental and every capture claimed a 62-byte body against a real 40. That artefact was
/// misread as evidence of a customised server core while triaging
/// <see href="https://github.com/Xian55/HermesProxy/issues/248">#248</see> — the same slack shows
/// up against a stock TrinityCore.
/// </summary>
public class ByteBufferDataSpanTests
{
    // The real shape from WorldClient.ReceiveLoop: SMSG_AUTH_CHALLENGE is 2 opcode + 40 body.
    private const int AuthChallengePacketSize = 42;

    [Fact]
    public void GetDataSpan_ReadModePooledBuffer_ReturnsPayloadLengthNotRentalLength()
    {
        byte[] rental = ArrayPool<byte>.Shared.Rent(AuthChallengePacketSize);
        try
        {
            using var packet = new WorldPacket(rental, AuthChallengePacketSize, isPooled: false);

            Assert.Equal(AuthChallengePacketSize, packet.GetDataSpan().Length);
            Assert.Equal((uint)AuthChallengePacketSize, packet.GetSize());
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rental);
        }
    }

    /// <summary>
    /// Guards the premise rather than the fix: if the pool ever handed back an exactly-sized array
    /// the bug would be invisible and the test above would pass for the wrong reason.
    /// </summary>
    [Fact]
    public void GetData_ReadModePooledBuffer_IsLongerThanThePayload()
    {
        byte[] rental = ArrayPool<byte>.Shared.Rent(AuthChallengePacketSize);
        try
        {
            Assert.True(
                rental.Length > AuthChallengePacketSize,
                $"ArrayPool returned an exact-size {rental.Length}-byte array; this test no longer proves anything.");

            using var packet = new WorldPacket(rental, AuthChallengePacketSize, isPooled: false);

            Assert.Equal(rental.Length, packet.GetData().Length);
            Assert.True(packet.GetData().Length > packet.GetDataSpan().Length);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rental);
        }
    }

    [Fact]
    public void GetDataSpan_WriteMode_MatchesGetData()
    {
        var buffer = new ByteBuffer();
        buffer.WriteUInt32(1);
        buffer.WriteUInt32(0xC94B8C0D);

        Assert.Equal(buffer.GetData(), buffer.GetDataSpan().ToArray());
        Assert.Equal(8, buffer.GetDataSpan().Length);
    }

    [Fact]
    public void GetDataSpan_ReadModeExactBuffer_MatchesGetData()
    {
        var data = new byte[] { 0xEC, 0x01, 0x01, 0x00, 0x00, 0x00 };
        var buffer = new ByteBuffer(data);

        Assert.Equal(data, buffer.GetDataSpan().ToArray());
    }
}
