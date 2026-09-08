using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Bgs.Protocol;
using BNetServer.Networking;
using BNetServer.Services;
using Framework.Constants;
using Google.Protobuf;
using Xunit;

namespace HermesProxy.Tests.BnetServer;

internal sealed record BnetReply(uint ServiceId, OriginalHash Service, uint MethodId,
    uint Token, BattlenetRpcErrorCode Status, IMessage? Message);

/// <summary>
/// A real <see cref="BnetTcpSession"/> over a loopback socket pair that captures the RPC replies it
/// would have written rather than serialising them onto the wire.
/// </summary>
internal sealed class RecordingSession(Socket socket) : BnetTcpSession(socket), BnetServices.INetwork
{
    public List<BnetReply> Replies { get; } = new();

    void BnetServices.INetwork.SendRpcMessage(uint serviceId, OriginalHash service,
        uint methodId, uint token, BattlenetRpcErrorCode status, IMessage? message)
    {
        Replies.Add(new BnetReply(serviceId, service, methodId, token, status, message));
    }

    public override void Dispose()
    {
        _pooledBuffer.Dispose();
        base.Dispose();
    }
}

internal static class BnetSessionHarness
{
    public static async Task WithSession(Func<RecordingSession, Task> test)
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        using var client = new TcpClient();
        await client.ConnectAsync((IPEndPoint)listener.LocalEndpoint, TestContext.Current.CancellationToken);
        using var socket = await listener.AcceptSocketAsync(TestContext.Current.CancellationToken);
        using var session = new RecordingSession(socket);
        await test(session);
    }

    public static void AppendFrame(BnetTcpSession session, Header header, IMessage? message = null)
        => AppendRawFrame(session, header, message?.ToByteArray() ?? Array.Empty<byte>());

    /// <summary>
    /// Appends a frame carrying hand-built payload bytes, so a test can hand the dispatcher input no
    /// protobuf message would ever produce.
    /// </summary>
    public static void AppendRawFrame(BnetTcpSession session, Header header, byte[] payload)
    {
        header.Size = (uint)payload.Length;
        var headerBytes = header.ToByteArray();
        var frame = new byte[2 + headerBytes.Length + payload.Length];
        BinaryPrimitives.WriteUInt16BigEndian(frame, (ushort)headerBytes.Length);
        headerBytes.CopyTo(frame, 2);
        payload.CopyTo(frame, 2 + headerBytes.Length);
        session._pooledBuffer.Append(frame, frame.Length);
    }

    /// <summary>
    /// Appends a frame whose header bytes will not decode, which is what a desynchronised stream
    /// looks like from the parser's side.
    /// </summary>
    public static void AppendUndecodableHeader(BnetTcpSession session)
    {
        // Wire type 7 does not exist in protobuf, so Header.MergeFrom rejects these bytes.
        var headerBytes = new byte[] { 0x0F, 0x0F, 0x0F };
        var frame = new byte[2 + headerBytes.Length];
        BinaryPrimitives.WriteUInt16BigEndian(frame, (ushort)headerBytes.Length);
        headerBytes.CopyTo(frame, 2);
        session._pooledBuffer.Append(frame, frame.Length);
    }
}
