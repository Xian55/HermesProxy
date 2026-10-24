using HermesProxy.Configuration.Options;
using HermesProxy.World;
using HermesProxy.World.Client;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// World markers shared between a party's sessions (#415): a 4.3.4 server sends only the mask, and
/// the places reach only the members near a marker.
/// </summary>
public sealed class RaidMarkerBoardTests
{
    private static GlobalSessionData NewSession() => new(new ClientOptions(), new LegacyServerOptions(),
        new ProxyNetworkOptions(), new DiagnosticsOptions { PacketsLog = false }, new ThrottlingOptions());

    [Fact]
    public void Place_SeenByOneMember_IsThereForTheParty()
    {
        var party = new WowGuid128(0x1F00000000000001, 0x415);
        var session = NewSession();
        try
        {
            Assert.False(RaidMarkerBoard.TryGet(party, 0, out _, out _));

            RaidMarkerBoard.Set(party, 0, 0, new Vector3(-8786.9f, 829.1f, 97.65f), session);

            Assert.True(RaidMarkerBoard.TryGet(party, 0, out uint map, out Vector3 place));
            Assert.Equal(0u, map);
            Assert.Equal(new Vector3(-8786.9f, 829.1f, 97.65f), place);
            Assert.False(RaidMarkerBoard.TryGet(party, 1, out _, out _));
            Assert.False(RaidMarkerBoard.TryGet(new WowGuid128(0x1F00000000000002, 0x415), 0, out _, out _));
        }
        finally
        {
            RaidMarkerBoard.Clear(party, 0);
            session.Executor.Dispose();
        }
    }

    [Fact]
    public void Marker_PutDownAgain_TakesTheNewPlace()
    {
        var party = new WowGuid128(0x1F00000000000003, 0x415);
        var session = NewSession();
        try
        {
            RaidMarkerBoard.Set(party, 2, 0, new Vector3(1, 2, 3), session);
            RaidMarkerBoard.Set(party, 2, 1, new Vector3(4, 5, 6), session);

            Assert.True(RaidMarkerBoard.TryGet(party, 2, out uint map, out Vector3 place));
            Assert.Equal(1u, map);
            Assert.Equal(new Vector3(4, 5, 6), place);
        }
        finally
        {
            RaidMarkerBoard.Clear(party, 2);
            session.Executor.Dispose();
        }
    }

    [Fact]
    public void Cleared_Marker_IsGone_AndTheOthersStay()
    {
        var party = new WowGuid128(0x1F00000000000004, 0x415);
        var session = NewSession();
        try
        {
            RaidMarkerBoard.Set(party, 0, 0, new Vector3(1, 1, 1), session);
            RaidMarkerBoard.Set(party, 1, 0, new Vector3(2, 2, 2), session);

            RaidMarkerBoard.Clear(party, 0);
            Assert.False(RaidMarkerBoard.TryGet(party, 0, out _, out _));
            Assert.True(RaidMarkerBoard.TryGet(party, 1, out _, out _));

            // The party's last marker goes, and a marker put down after that is kept again.
            RaidMarkerBoard.Clear(party, 1);
            Assert.False(RaidMarkerBoard.TryGet(party, 1, out _, out _));
            RaidMarkerBoard.Set(party, 1, 0, new Vector3(3, 3, 3), session);
            Assert.True(RaidMarkerBoard.TryGet(party, 1, out _, out Vector3 place));
            Assert.Equal(new Vector3(3, 3, 3), place);
        }
        finally
        {
            RaidMarkerBoard.Clear(party, 0);
            RaidMarkerBoard.Clear(party, 1);
            session.Executor.Dispose();
        }
    }
}
