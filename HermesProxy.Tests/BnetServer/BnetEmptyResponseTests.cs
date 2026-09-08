using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Bgs.Protocol;
using Bgs.Protocol.Connection.V1;
using BNetServer.Networking;
using BNetServer.Services;
using Framework.Constants;
using Google.Protobuf;
using Xunit;

namespace HermesProxy.Tests.BnetServer;

public class BnetEmptyResponseTests
{
    [Fact]
    public async Task ProcessCurrentBuffer_EmptyAcknowledgementThenConnect_DispatchesConnect()
    {
        await WithSession(async session =>
        {
            AppendFrame(session, new Header { ServiceId = 0xFE, Token = 1 });
            var request = new ConnectRequest
            {
                UseBindlessRpc = true
            };
            AppendFrame(session, new Header
            {
                ServiceId = 0,
                ServiceHash = (uint)OriginalHash.ConnectionService,
                MethodId = 1,
                Token = 2
            }, request);

            await session.ProcessCurrentBuffer();

            Assert.Equal(0, session._pooledBuffer.Length);
            var reply = Assert.Single(session.Replies);
            Assert.Equal(0xFEu, reply.ServiceId);
            Assert.Equal(OriginalHash.ConnectionService, reply.Service);
            Assert.Equal(1u, reply.MethodId);
            Assert.Equal(2u, reply.Token);
            Assert.Equal(BattlenetRpcErrorCode.Ok, reply.Status);
            var response = Assert.IsType<ConnectResponse>(reply.Message);
            Assert.True(response.UseBindlessRpc);
        });
    }

    [Fact]
    public async Task ProcessCurrentBuffer_EmptyKeepAlive_CompletesWithoutErrorResponse()
    {
        await WithSession(async session =>
        {
            var request = new NoData();
            Assert.Equal(0, request.CalculateSize());
            AppendFrame(session, new Header
            {
                ServiceId = 0,
                ServiceHash = (uint)OriginalHash.ConnectionService,
                MethodId = 5,
                Token = 3
            }, request);

            await session.ProcessCurrentBuffer();

            Assert.Equal(0, session._pooledBuffer.Length);
            Assert.Empty(session.Replies);
        });
    }

    [Fact]
    public async Task EmptyAcknowledgementDoesNotStopProcessingFollowingFrames()
    {
        await WithSession(async session =>
        {
            AppendFrame(session, new Header { ServiceId = 0xFE, Token = 1 });
            AppendFrame(session, new Header { ServiceId = 0xFE, Token = 2 });

            await session.ProcessCurrentBuffer();

            Assert.Equal(0, session._pooledBuffer.Length);
            Assert.Empty(session.Replies);
        });
    }

    private static void AppendFrame(BnetTcpSession session, Header header, IMessage? message = null)
    {
        var payload = message?.ToByteArray() ?? Array.Empty<byte>();
        header.Size = (uint)payload.Length;
        var headerBytes = header.ToByteArray();
        var frame = new byte[2 + headerBytes.Length + payload.Length];
        BinaryPrimitives.WriteUInt16BigEndian(frame, (ushort)headerBytes.Length);
        headerBytes.CopyTo(frame, 2);
        payload.CopyTo(frame, 2 + headerBytes.Length);
        session._pooledBuffer.Append(frame, frame.Length);
    }

    private static async Task WithSession(Func<RecordingSession, Task> test)
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        using var client = new TcpClient();
        await client.ConnectAsync((IPEndPoint)listener.LocalEndpoint, TestContext.Current.CancellationToken);
        using var socket = await listener.AcceptSocketAsync(TestContext.Current.CancellationToken);
        using var session = new RecordingSession(socket);
        await test(session);
    }

    private sealed record Reply(uint ServiceId, OriginalHash Service, uint MethodId,
        uint Token, BattlenetRpcErrorCode Status, IMessage? Message);

    private sealed class RecordingSession(Socket socket) : BnetTcpSession(socket), BnetServices.INetwork
    {
        public List<Reply> Replies { get; } = new();

        void BnetServices.INetwork.SendRpcMessage(uint serviceId, OriginalHash service,
            uint methodId, uint token, BattlenetRpcErrorCode status, IMessage? message)
        {
            Replies.Add(new Reply(serviceId, service, methodId, token, status, message));
        }

        public override void Dispose()
        {
            _pooledBuffer.Dispose();
            base.Dispose();
        }
    }
}
