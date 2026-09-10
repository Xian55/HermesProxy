using System.Buffers.Binary;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bgs.Protocol;
using Bgs.Protocol.GameUtilities.V1;
using BNetServer.Networking;
using Framework.Constants;
using Google.Protobuf;
using HermesProxy.Tests.Framework;
using Xunit;

namespace HermesProxy.Tests.BnetServer;

public class BnetTcpSessionWriteTests
{
    [Fact]
    public async Task SendRpcMessage_BehindAStalledFrame_DeliversBothFramesInOrder()
    {
        await TlsLoopback.With(s => new BnetTcpSession(s), tinyBuffers: true, async (session, client) =>
        {
            var bulk = new ClientResponse();
            bulk.Attribute.Add(new Bgs.Protocol.Attribute
            {
                Name = "Param_RealmList",
                Value = new Variant { BlobValue = ByteString.CopyFrom(new byte[4 * 1024 * 1024]) },
            });

            // Back to back, as HandleLogon does: a listener request, then the RPC response. The
            // first frame cannot drain, so the second starts while it is still being written.
            session.SendRpcMessage(0xFE, OriginalHash.GameUtilitiesService, 1, 1, BattlenetRpcErrorCode.Ok, bulk);
            session.SendRpcMessage(0xFE, OriginalHash.GameUtilitiesService, 1, 2, BattlenetRpcErrorCode.Ok, null);

            var tokens = new List<uint>();
            for (int i = 0; i < 2; i++)
            {
                var prefix = await TlsLoopback.ReadExactly(client, 2);
                var header = Header.Parser.ParseFrom(
                    await TlsLoopback.ReadExactly(client, BinaryPrimitives.ReadUInt16BigEndian(prefix)));
                if (header.Size > 0)
                    Assert.Equal(bulk, ClientResponse.Parser.ParseFrom(await TlsLoopback.ReadExactly(client, (int)header.Size)));
                tokens.Add(header.Token);
            }

            Assert.Equal(new uint[] { 1, 2 }, tokens);
        });
    }
}
