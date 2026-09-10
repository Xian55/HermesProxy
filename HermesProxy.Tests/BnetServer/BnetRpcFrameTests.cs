using System;
using System.Buffers.Binary;
using System.Linq;
using Bgs.Protocol;
using Bgs.Protocol.Authentication.V1;
using BNetServer.Networking;
using Google.Protobuf;
using Xunit;

namespace HermesProxy.Tests.BnetServer;

public class BnetRpcFrameTests
{
    private static Header NewHeader() => new()
    {
        Token = 42,
        Status = 0,
        ServiceId = 0xFE,
        ServiceHash = 0x71240E35,
        MethodId = 5,
    };

    private static LogonResult NewLogonResult()
    {
        var result = new LogonResult
        {
            ErrorCode = 0,
            AccountId = new EntityId { High = 0x100000000000000, Low = 7 },
            SessionKey = ByteString.CopyFrom(Enumerable.Range(0, 64).Select(i => (byte)i).ToArray()),
        };
        result.GameAccountId.Add(new EntityId { High = 0x200000200576F57, Low = 7 });
        return result;
    }

    [Fact]
    public void BuildRpcFrame_WithMessage_MatchesLengthPrefixedHeaderThenPayload()
    {
        var message = NewLogonResult();
        var expectedHeader = NewHeader();
        expectedHeader.Size = (uint)message.CalculateSize();
        var headerBytes = expectedHeader.ToByteArray();
        var payloadBytes = message.ToByteArray();

        var frame = BnetTcpSession.BuildRpcFrame(NewHeader(), message);

        Assert.Equal(2 + headerBytes.Length + payloadBytes.Length, frame.Length);
        Assert.Equal(headerBytes.Length, BinaryPrimitives.ReadUInt16BigEndian(frame));
        Assert.Equal(headerBytes, frame.AsSpan(2, headerBytes.Length).ToArray());
        Assert.Equal(payloadBytes, frame.AsSpan(2 + headerBytes.Length).ToArray());
    }

    [Fact]
    public void BuildRpcFrame_WithMessage_RoundTripsThroughParseFromSpan()
    {
        var message = NewLogonResult();

        var frame = BnetTcpSession.BuildRpcFrame(NewHeader(), message);
        var result = BnetPacketParser.ParseFromSpan(frame);

        try
        {
            Assert.True(result.Success);
            Assert.Equal(frame.Length, result.TotalLength);
            Assert.Equal(42u, result.Header!.Token);
            Assert.Equal(0x71240E35u, result.Header.ServiceHash);
            Assert.Equal(5u, result.Header.MethodId);
            Assert.Equal(LogonResult.Parser.ParseFrom(result.PayloadSpan), message);
        }
        finally
        {
            result.ReturnPayload();
        }
    }

    [Fact]
    public void BuildRpcFrame_WithoutMessage_LeavesSizeUnsetAndCarriesNoPayload()
    {
        var header = NewHeader();

        var frame = BnetTcpSession.BuildRpcFrame(header, null);
        var result = BnetPacketParser.ParseFromSpan(frame);

        Assert.False(header.HasSize);
        Assert.Equal(2 + header.CalculateSize(), frame.Length);
        Assert.True(result.Success);
        Assert.Equal(0, result.PayloadLength);
        Assert.Null(result.PayloadArray);
    }
}
