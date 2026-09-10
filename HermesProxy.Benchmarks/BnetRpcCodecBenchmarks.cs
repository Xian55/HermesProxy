using System;
using System.Buffers;
using Bgs.Protocol;
using Bgs.Protocol.Authentication.V1;
using Bgs.Protocol.GameUtilities.V1;
using BenchmarkDotNet.Attributes;
using BNetServer.Networking;
using Google.Protobuf;

namespace HermesProxy.Benchmarks;

/// <summary>
/// BNet RPC payload codec: request parse the way <c>ServiceManager.Invoke</c> does it, and
/// response framing the way <c>BnetTcpSession.SendRpcMessage</c> does it, on login-shaped messages.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class BnetRpcCodecBenchmarks
{
    private byte[] _logonRequest = null!;
    private byte[] _clientRequest = null!;
    private ClientResponse _realmListResponse = null!;
    private LogonResult _logonResult = null!;

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        byte[] Blob(int size)
        {
            var bytes = new byte[size];
            random.NextBytes(bytes);
            return bytes;
        }

        _logonRequest = new LogonRequest
        {
            Program = "WoW",
            Platform = "Wn64",
            Locale = "enUS",
            Version = "3.4.3.54261",
            ApplicationVersion = 54261,
            PublicComputer = false,
            AllowLogonQueueNotifications = true,
            UserAgent = "Battle.net/2.0 WoW/3.4.3.54261 (Windows 10.0.19045)",
            DeviceId = "0f7a9f4e-2c1b-4d8e-9a6f-3b5c7d1e2f40",
        }.ToByteArray();

        var clientRequest = new ClientRequest();
        clientRequest.Attribute.Add(new Bgs.Protocol.Attribute
        {
            Name = "Command_RealmListTicketRequest_v1_b9",
            Value = new Variant { BlobValue = ByteString.CopyFrom(Blob(320)) },
        });
        _clientRequest = clientRequest.ToByteArray();

        _realmListResponse = new ClientResponse();
        _realmListResponse.Attribute.Add(new Bgs.Protocol.Attribute
        {
            Name = "Param_RealmList",
            Value = new Variant { BlobValue = ByteString.CopyFrom(Blob(1500)) },
        });
        _realmListResponse.Attribute.Add(new Bgs.Protocol.Attribute
        {
            Name = "Param_CharacterCountList",
            Value = new Variant { BlobValue = ByteString.CopyFrom(Blob(64)) },
        });

        _logonResult = new LogonResult
        {
            ErrorCode = 0,
            AccountId = new EntityId { High = 0x100000000000000, Low = 1 },
            SessionKey = ByteString.CopyFrom(Blob(64)),
        };
        _logonResult.GameAccountId.Add(new EntityId { High = 0x200000200576F57, Low = 1 });
    }

    private static Header NewHeader() => new()
    {
        Token = 7,
        Status = 0,
        ServiceId = 0xFE,
        ServiceHash = 0x3FE5849E,
        MethodId = 1,
    };

    [Benchmark]
    public IMessage ParseLogonRequest()
    {
        IMessage request = new LogonRequest();
        request.MergeFrom((ReadOnlySpan<byte>)_logonRequest);
        return request;
    }

    [Benchmark]
    public IMessage ParseClientRequest()
    {
        IMessage request = new ClientRequest();
        request.MergeFrom((ReadOnlySpan<byte>)_clientRequest);
        return request;
    }

    [Benchmark]
    public int FrameRealmListResponse() => FrameAndReturn(_realmListResponse);

    [Benchmark]
    public int FrameLogonResult() => FrameAndReturn(_logonResult);

    // Rent then return, as SendRpcMessage does once SSLSocket.AsyncWrite has sent the frame.
    private static int FrameAndReturn(IMessage message)
    {
        var frame = BnetTcpSession.RentRpcFrame(NewHeader(), message, out int length);
        ArrayPool<byte>.Shared.Return(frame);
        return length;
    }
}
