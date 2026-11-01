using HermesProxy.Tests.Support;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using HermesProxy.World.Server.Systems;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// A modern client asks for the graveyards at login. A 3.3.5a server has no such list,
/// so the proxy answers that there is none.
/// </summary>
public sealed class CemeteryListTests
{
    [Fact]
    public void Request_OnAServerWithoutOne_IsAnsweredWithNoCemeteries()
    {
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy) return;
        var harness = new LegacyHandlerHarness(recordClientPackets: true, recordServerPackets: true);
        var ctx = new SessionContext(harness.Session, socket: null, harness.Client);

        var request = new EmptyClientPacket();
        MiscSystem.HandleRequestCemeteryList(in request, in ctx);

        var answer = Assert.Single(harness.ClientWire.Sent);
        Assert.IsType<RequestCemeteryListResponse>(answer.Packet);
        Assert.Equal([0, 0, 0, 0, 0], answer.Bytes);
        Assert.Empty(harness.ServerWire.Sent);
    }
}
