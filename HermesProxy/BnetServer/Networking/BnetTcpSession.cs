// Copyright (c) CypherCore <http://github.com/CypherCore> All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE. See LICENSE file in the project root for full license information.

using Bgs.Protocol;
using Framework.Constants;
using Framework.IO;
using Framework.Logging;
using Framework.Networking;
using Google.Protobuf;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using BNetServer.Services;
using HermesProxy.World;
using HermesProxy.World.Enums;

namespace BNetServer.Networking;

/// <summary>
/// A pooled byte buffer that avoids allocations by using ArrayPool and position tracking.
/// Instead of removing bytes from the front (O(n)), we advance a read position (O(1)).
/// </summary>
internal sealed class PooledByteBuffer : IDisposable
{
    private byte[] _buffer;
    private int _readPos;
    private int _writePos;
    private const int DefaultCapacity = 8192;
    private const int MaxCapacity = 65536;

    public PooledByteBuffer()
    {
        _buffer = ArrayPool<byte>.Shared.Rent(DefaultCapacity);
        _readPos = 0;
        _writePos = 0;
    }

    public int Length => _writePos - _readPos;

    public ReadOnlySpan<byte> Span => _buffer.AsSpan(_readPos, Length);

    public void Append(byte[] data, int length)
    {
        EnsureCapacity(length);
        data.AsSpan(0, length).CopyTo(_buffer.AsSpan(_writePos));
        _writePos += length;
    }

    public void Advance(int count)
    {
        _readPos += count;

        // Compact if we've consumed more than half the buffer
        if (_readPos > _buffer.Length / 2)
        {
            Compact();
        }
    }

    private void EnsureCapacity(int additionalBytes)
    {
        int required = _writePos + additionalBytes;
        if (required <= _buffer.Length)
            return;

        // First try compacting
        if (_readPos > 0)
        {
            Compact();
            if (_writePos + additionalBytes <= _buffer.Length)
                return;
        }

        // Need to grow
        int newSize = Math.Min(Math.Max(_buffer.Length * 2, required), MaxCapacity);
        if (newSize < required)
            throw new InvalidOperationException($"Buffer would exceed maximum capacity of {MaxCapacity} bytes");

        var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
        _buffer.AsSpan(_readPos, Length).CopyTo(newBuffer);
        ArrayPool<byte>.Shared.Return(_buffer);
        _buffer = newBuffer;
        _writePos = Length;
        _readPos = 0;
    }

    private void Compact()
    {
        if (_readPos == 0)
            return;

        int len = Length;
        if (len > 0)
        {
            _buffer.AsSpan(_readPos, len).CopyTo(_buffer);
        }
        _writePos = len;
        _readPos = 0;
    }

    public void Clear()
    {
        _readPos = 0;
        _writePos = 0;
    }

    public void Dispose()
    {
        if (_buffer != null)
        {
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = null!;
        }
    }
}

/// <summary>
/// Result of attempting to parse a Bnet packet frame from a buffer.
/// </summary>
internal readonly struct BnetPacketParseResult
{
    public readonly bool Success;
    public readonly int TotalLength;
    public readonly ushort HeaderLength;
    public readonly Header? Header;
    public readonly byte[]? Payload;

    public static BnetPacketParseResult Incomplete => new(false, 0, 0, null, null);

    public BnetPacketParseResult(bool success, int totalLength, ushort headerLength, Header? header, byte[]? payload)
    {
        Success = success;
        TotalLength = totalLength;
        HeaderLength = headerLength;
        Header = header;
        Payload = payload;
    }
}

/// <summary>
/// Packet buffer implementations for benchmarking comparison.
/// </summary>
internal static class BnetPacketParser
{
    /// <summary>
    /// Original implementation using List&lt;byte&gt; with LINQ Take/Skip/ToArray.
    /// </summary>
    public static BnetPacketParseResult ParseFromListOriginal(List<byte> buffer)
    {
        if (buffer.Count <= 2)
            return BnetPacketParseResult.Incomplete;

        var headerLengthBuffer = buffer.Take(2).ToArray();
        var headerLength = (ushort)IPAddress.HostToNetworkOrder(BitConverter.ToInt16(headerLengthBuffer));

        if (buffer.Count < 2 + headerLength)
            return BnetPacketParseResult.Incomplete;

        var headerBuffer = buffer.Skip(2).Take(headerLength).ToArray();
        var header = new Header();
        header.MergeFrom(headerBuffer);

        int payloadLength = (int)header.Size;

        if (buffer.Count < 2 + headerLength + payloadLength)
            return BnetPacketParseResult.Incomplete;

        var payloadBuffer = buffer.Skip(2).Skip(headerLength).Take(payloadLength).ToArray();
        int totalLength = 2 + headerLength + payloadLength;

        return new BnetPacketParseResult(true, totalLength, headerLength, header, payloadBuffer);
    }

    /// <summary>
    /// Optimized implementation using Span&lt;T&gt; and CollectionsMarshal.
    /// Avoids LINQ Take/Skip/ToArray allocations by using direct span slicing.
    /// </summary>
    public static BnetPacketParseResult ParseFromListOptimized(List<byte> buffer)
    {
        if (buffer.Count <= 2)
            return BnetPacketParseResult.Incomplete;

        var span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(buffer);

        ushort headerLength = BinaryPrimitives.ReadUInt16BigEndian(span);

        if (buffer.Count < 2 + headerLength)
            return BnetPacketParseResult.Incomplete;

        // Note: MergeFrom with Span requires protobuf to be regenerated with ParseContext support.
        // For now, we still need to allocate for the header buffer, but we avoid LINQ overhead.
        var headerBuffer = span.Slice(2, headerLength).ToArray();
        var header = new Header();
        header.MergeFrom(headerBuffer);

        int payloadLength = (int)header.Size;

        if (buffer.Count < 2 + headerLength + payloadLength)
            return BnetPacketParseResult.Incomplete;

        var payloadBuffer = span.Slice(2 + headerLength, payloadLength).ToArray();
        int totalLength = 2 + headerLength + payloadLength;

        return new BnetPacketParseResult(true, totalLength, headerLength, header, payloadBuffer);
    }

    /// <summary>
    /// ArrayPool-based implementation for benchmarking. Uses rented buffers to reduce allocations.
    /// Caller must call result.ReturnPayload() after processing.
    /// </summary>
    public static BnetPacketParseResultPooled ParseFromListPooled(List<byte> buffer)
    {
        if (buffer.Count <= 2)
            return BnetPacketParseResultPooled.Incomplete;

        var span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(buffer);

        ushort headerLength = BinaryPrimitives.ReadUInt16BigEndian(span);

        if (buffer.Count < 2 + headerLength)
            return BnetPacketParseResultPooled.Incomplete;

        var headerBuffer = span.Slice(2, headerLength).ToArray();
        var header = new Header();
        header.MergeFrom(headerBuffer);

        int payloadLength = (int)header.Size;

        if (buffer.Count < 2 + headerLength + payloadLength)
            return BnetPacketParseResultPooled.Incomplete;

        // Rent from ArrayPool instead of allocating
        byte[]? rentedArray = null;
        if (payloadLength > 0)
        {
            rentedArray = ArrayPool<byte>.Shared.Rent(payloadLength);
            span.Slice(2 + headerLength, payloadLength).CopyTo(rentedArray);
        }

        int totalLength = 2 + headerLength + payloadLength;

        return new BnetPacketParseResultPooled(true, totalLength, headerLength, header, rentedArray, payloadLength);
    }

    /// <summary>
    /// Zero-allocation parser using ReadOnlySpan and stackalloc for headers.
    /// Works with PooledByteBuffer for minimal allocations.
    /// </summary>
    public static BnetPacketParseResultPooled ParseFromSpan(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length <= 2)
            return BnetPacketParseResultPooled.Incomplete;

        ushort headerLength = BinaryPrimitives.ReadUInt16BigEndian(buffer);

        if (buffer.Length < 2 + headerLength)
            return BnetPacketParseResultPooled.Incomplete;

        // Parse header - unfortunately protobuf requires byte[] for MergeFrom
        // Use stackalloc for small headers (typical headers are < 50 bytes)
        var header = new Header();
        if (headerLength <= 128)
        {
            Span<byte> headerStack = stackalloc byte[headerLength];
            buffer.Slice(2, headerLength).CopyTo(headerStack);
            // Note: We still need to call ToArray() because protobuf doesn't support Span
            // But we avoid the intermediate List<byte> overhead
            header.MergeFrom(headerStack.ToArray());
        }
        else
        {
            var headerBuffer = ArrayPool<byte>.Shared.Rent(headerLength);
            try
            {
                buffer.Slice(2, headerLength).CopyTo(headerBuffer);
                header.MergeFrom(new ReadOnlySpan<byte>(headerBuffer, 0, headerLength).ToArray());
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(headerBuffer);
            }
        }

        int payloadLength = (int)header.Size;

        if (buffer.Length < 2 + headerLength + payloadLength)
            return BnetPacketParseResultPooled.Incomplete;

        // Use ArrayPool for payload
        byte[]? rentedPayload = null;
        if (payloadLength > 0)
        {
            rentedPayload = ArrayPool<byte>.Shared.Rent(payloadLength);
            buffer.Slice(2 + headerLength, payloadLength).CopyTo(rentedPayload);
        }

        int totalLength = 2 + headerLength + payloadLength;

        return new BnetPacketParseResultPooled(true, totalLength, headerLength, header, rentedPayload, payloadLength);
    }
}

/// <summary>
/// Result with ArrayPool-backed payload buffer for reduced allocations.
/// </summary>
internal readonly struct BnetPacketParseResultPooled
{
    public readonly bool Success;
    public readonly int TotalLength;
    public readonly ushort HeaderLength;
    public readonly Header? Header;
    public readonly byte[]? PayloadArray;
    public readonly int PayloadLength;

    public static BnetPacketParseResultPooled Incomplete => new(false, 0, 0, null, null, 0);

    public BnetPacketParseResultPooled(bool success, int totalLength, ushort headerLength, Header? header, byte[]? payloadArray, int payloadLength)
    {
        Success = success;
        TotalLength = totalLength;
        HeaderLength = headerLength;
        Header = header;
        PayloadArray = payloadArray;
        PayloadLength = payloadLength;
    }

    public ReadOnlySpan<byte> PayloadSpan => PayloadArray.AsSpan(0, PayloadLength);

    /// <summary>
    /// Returns the rented payload array to the pool. Must be called after processing.
    /// </summary>
    public void ReturnPayload()
    {
        if (PayloadArray != null)
        {
            ArrayPool<byte>.Shared.Return(PayloadArray);
        }
    }
}

public class BnetTcpSession : SSLSocket, BnetServices.INetwork
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melServer = Log.CreateMelLogger(Log.CategoryServer);
    private static readonly string _sourceFile = nameof(BnetTcpSession).PadRight(15);
    private const string _netDirNone = "";

    private readonly BnetServices.ServiceManager _handlerManager;

    public BnetTcpSession(Socket socket) : base(socket)
    {
        _handlerManager = new BnetServices.ServiceManager("BnetTcp", this, initialSession: null);
    }

    public override void Accept()
    {
        string ipAddress = GetRemoteIpEndPoint()!.ToString();
        BnetTcpSessionLogMessages.AcceptingConnection(_melServer, _sourceFile, _netDirNone, ipAddress);
        AsyncHandshake(BnetServerCertificate.Certificate);
    }

    public override bool Update()
    {
        if (!base.Update())
            return false;

        return true;
    }

    public override void OnClose()
    {
        _pooledBuffer.Dispose();
        base.OnClose();
    }

    internal readonly PooledByteBuffer _pooledBuffer = new();

    public override async Task ReadHandler(byte[] data, int receivedLength)
    {
        if (!IsOpen())
            return;

        _pooledBuffer.Append(data, receivedLength);

        await ProcessCurrentBuffer();

        await AsyncRead();
    }

    internal Task ProcessCurrentBuffer()
    {
        while (_pooledBuffer.Length > 2)
        {
            var result = BnetPacketParser.ParseFromSpan(_pooledBuffer.Span);

            if (!result.Success)
            {
                result.ReturnPayload();
                return Task.CompletedTask;
            }

            _pooledBuffer.Advance(result.TotalLength);

            try
            {
                var stream = new CodedInputStream(result.PayloadArray, 0, result.PayloadLength);
                if (result.Header!.ServiceId != 0xFE && result.Header.ServiceHash != 0)
                {
                    _handlerManager.Invoke(result.Header.ServiceId, (OriginalHash)result.Header.ServiceHash, result.Header.MethodId, result.Header.Token, stream);
                }
            }
            finally
            {
                result.ReturnPayload();
            }
        }

        return Task.CompletedTask;
    }

    public void SendRpcMessage(uint serviceId, OriginalHash service, uint methodId, uint token, BattlenetRpcErrorCode status, IMessage? message)
    {
        Header header = new();
        header.Token = token;
        header.Status = (uint)status;
        header.ServiceId = serviceId;
        header.ServiceHash = (uint)service;
        header.MethodId = methodId;
        if (message != null)
            header.Size = (uint)message.CalculateSize();

        ByteBuffer buffer = new();
        buffer.WriteBytes(GetHeaderSize(header), 2);
        buffer.WriteBytes(header.ToByteArray());
        if (message != null)
            buffer.WriteBytes(message.ToByteArray());

        AsyncWrite(buffer.GetData());
    }

    public byte[] GetHeaderSize(Header header)
    {
        var size = (ushort)header.CalculateSize();
        byte[] bytes = new byte[2];
        bytes[0] = (byte)((size >> 8) & 0xff);
        bytes[1] = (byte)(size & 0xff);

        var headerSizeBytes = BitConverter.GetBytes((ushort)header.CalculateSize());
        Array.Reverse(headerSizeBytes);

        return bytes;
    }
}

public class AccountInfo
{   
    public uint Id;
    public WowGuid128 BnetAccountGuid => WowGuid128.Create(HighGuidType703.BNetAccount, Id);
    public string Login;
    public uint LoginTicketExpiry;
    public bool IsBanned;
    public bool IsPermanenetlyBanned;

    public Dictionary<uint, GameAccountInfo> GameAccounts;

    public AccountInfo(string name)
    {
        Id = 1;
        Login = name;
        LoginTicketExpiry = (uint)(Time.UnixTime + 10000);
        IsBanned = false;
        IsPermanenetlyBanned = false;

        GameAccounts = new Dictionary<uint, GameAccountInfo>();
        var account = new GameAccountInfo(name);
        GameAccounts[1] = account;
    }
}

public class GameAccountInfo
{
    public uint Id;
    public WowGuid128 WoWAccountGuid => WowGuid128.Create(HighGuidType703.WowAccount, Id);
    public string Name;
    public string DisplayName;
    public uint UnbanDate;
    public bool IsBanned;
    public bool IsPermanenetlyBanned;

    public GameAccountInfo(string name)
    {
        Id = 1;
        Name = name;
        UnbanDate = 0;
        IsPermanenetlyBanned = false;
        IsBanned = IsPermanenetlyBanned || UnbanDate > Time.UnixTime;

        int hashPos = Name.IndexOf('#');
        if (hashPos != -1)
            DisplayName = "WoW" + Name[(hashPos + 1)..];
        else
            DisplayName = Name;
    }
}
