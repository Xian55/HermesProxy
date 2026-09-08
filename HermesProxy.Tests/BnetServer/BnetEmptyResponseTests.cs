using System.Threading.Tasks;
using Bgs.Protocol;
using Bgs.Protocol.Connection.V1;
using Framework.Constants;
using Xunit;
using static HermesProxy.Tests.BnetServer.BnetSessionHarness;

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
}
