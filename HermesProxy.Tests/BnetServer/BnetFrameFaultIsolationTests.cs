using System.Threading.Tasks;
using Bgs.Protocol;
using Bgs.Protocol.Connection.V1;
using Framework.Constants;
using Xunit;
using static HermesProxy.Tests.BnetServer.BnetSessionHarness;

namespace HermesProxy.Tests.BnetServer;

/// <summary>
/// Issue #266. A frame that makes the service dispatcher throw used to escape ProcessCurrentBuffer,
/// which meant BnetTcpSession.ReadHandler never reached its AsyncRead() and SSLSocket discarded the
/// faulted task — the session stayed open but stopped reading, with nothing in the log.
/// </summary>
public class BnetFrameFaultIsolationTests
{
    // Field 1 with wire type 7. No valid protobuf encoding uses it, so MergeFrom throws while the
    // dispatcher is decoding the request — the same catch that covers a throwing service handler.
    private static readonly byte[] UndecodablePayload = [0x0F, 0x0F];

    private static Header ConnectHeader(uint token) => new()
    {
        ServiceId = 0,
        ServiceHash = (uint)OriginalHash.ConnectionService,
        MethodId = 1,
        Token = token
    };

    [Fact]
    public async Task UndecodablePayload_SkipsOnlyThatFrame_AndKeepsDispatching()
    {
        await WithSession(async session =>
        {
            AppendRawFrame(session, ConnectHeader(1), UndecodablePayload);
            AppendFrame(session, ConnectHeader(2), new ConnectRequest { UseBindlessRpc = true });

            await session.ProcessCurrentBuffer();

            Assert.Equal(0, session._pooledBuffer.Length);
            Assert.True(session.IsOpen());

            // Only the good frame answered: the bad one throws before the dispatcher can reply.
            var reply = Assert.Single(session.Replies);
            Assert.Equal(2u, reply.Token);
            Assert.Equal(BattlenetRpcErrorCode.Ok, reply.Status);
            Assert.IsType<ConnectResponse>(reply.Message);
        });
    }

    [Fact]
    public async Task ConsecutiveUndecodablePayloads_AllSkipped_SessionStaysOpen()
    {
        await WithSession(async session =>
        {
            AppendRawFrame(session, ConnectHeader(1), UndecodablePayload);
            AppendRawFrame(session, ConnectHeader(2), UndecodablePayload);
            AppendRawFrame(session, ConnectHeader(3), UndecodablePayload);

            await session.ProcessCurrentBuffer();

            Assert.Equal(0, session._pooledBuffer.Length);
            Assert.True(session.IsOpen());
            Assert.Empty(session.Replies);
        });
    }

    [Fact]
    public async Task ReadHandler_SurvivesADispatchFault_AndDrainsTheBuffer()
    {
        await WithSession(async session =>
        {
            AppendRawFrame(session, ConnectHeader(1), UndecodablePayload);

            // The guard lives in ReadHandler too: before #266 this path let the exception escape
            // into a task nobody awaited, so the read loop was never re-armed.
            await session.ProcessCurrentBuffer();

            Assert.Equal(0, session._pooledBuffer.Length);
            Assert.True(session.IsOpen());

            // The session is still usable afterwards — a later frame dispatches normally.
            AppendFrame(session, ConnectHeader(2), new ConnectRequest());
            await session.ProcessCurrentBuffer();

            Assert.Equal(0, session._pooledBuffer.Length);
            Assert.Equal(2u, Assert.Single(session.Replies).Token);
        });
    }

    [Fact]
    public async Task UndecodableHeader_ClosesConnection_AndDrainsTheBuffer()
    {
        await WithSession(async session =>
        {
            AppendUndecodableHeader(session);
            AppendFrame(session, ConnectHeader(9), new ConnectRequest());

            await session.ProcessCurrentBuffer();

            // A header that will not decode leaves no way to find the next frame boundary, so the
            // rest of the buffer is unusable and the connection has to go.
            Assert.Equal(0, session._pooledBuffer.Length);
            Assert.False(session.IsOpen());
            Assert.Empty(session.Replies);
        });
    }

    [Fact]
    public async Task ReadHandler_OversizedRead_ClosesConnectionInsteadOfFaulting()
    {
        await WithSession(async session =>
        {
            // PooledByteBuffer caps at 65536 bytes, so a larger single read cannot be buffered and
            // EnsureCapacity throws. That used to kill the read loop silently.
            var oversized = new byte[70000];

            await session.ReadHandler(oversized, oversized.Length);

            Assert.False(session.IsOpen());
        });
    }
}
