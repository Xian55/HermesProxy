// Copyright (c) CypherCore <http://github.com/CypherCore> All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE. See LICENSE file in the project root for full license information.

using Bgs.Protocol;
using Framework.Constants;
using Framework.IO;
using Framework.Logging;
using Framework.Networking;
using Google.Protobuf;
using System;
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

namespace BNetServer.Networking
{
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
    }

    public class BnetTcpSession : SSLSocket, BnetServices.INetwork
    {
        private readonly BnetServices.ServiceManager _handlerManager;

        public BnetTcpSession(Socket socket) : base(socket)
        {
            _handlerManager = new BnetServices.ServiceManager("BnetTcp", this, initialSession: null);
        }

        public override void Accept()
        {
            string ipAddress = GetRemoteIpEndPoint().ToString();
            Log.Print(LogType.Server, $"Accepting connection from {ipAddress}.");
            AsyncHandshake(BnetServerCertificate.Certificate);
        }

        public override bool Update()
        {
            if (!base.Update())
                return false;

            return true;
        }

        internal List<byte> _currentBuffer = new List<byte>();

        internal const bool UseOptimizedParser = true;

        public override async Task ReadHandler(byte[] data, int receivedLength)
        {
            if (!IsOpen())
                return;
            
            _currentBuffer.AddRange(data.Take(receivedLength));

            await ProcessCurrentBuffer();

            await AsyncRead();
        }

        internal Task ProcessCurrentBuffer()
        {
            while (_currentBuffer.Count > 2)
            {
                var result = UseOptimizedParser
                    ? BnetPacketParser.ParseFromListOptimized(_currentBuffer)
                    : BnetPacketParser.ParseFromListOriginal(_currentBuffer);

                if (!result.Success)
                    return Task.CompletedTask;

                _currentBuffer.RemoveRange(0, result.TotalLength);

                var stream = new CodedInputStream(result.Payload);
                if (result.Header!.ServiceId != 0xFE && result.Header.ServiceHash != 0)
                {
                    _handlerManager.Invoke(result.Header.ServiceId, (OriginalHash)result.Header.ServiceHash, result.Header.MethodId, result.Header.Token, stream);
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
}
