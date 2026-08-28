using HermesProxy.World.Server;
using Xunit;

namespace HermesProxy.Tests.World.Server;

public class AreaTriggerReconciliationTests
{
    [Fact]
    public void DarkPortal_UsesProximitySynthOnBothMaps()
    {
        Assert.True(AreaTriggerReconciliation.ProximityByMap.TryGetValue(0, out var blastedLands));
        Assert.Contains(blastedLands, e => e.LegacyId == 4354 && e.Radius == 30f);
        Assert.True(AreaTriggerReconciliation.ProximityByMap.TryGetValue(530, out var outland));
        Assert.Contains(outland, e => e.LegacyId == 4352 && e.BoxLength > 70f);
        Assert.True(AreaTriggerReconciliation.ModernToLegacy.TryGetValue(4356, out uint legacy));
        Assert.Equal(4352u, legacy);
    }

    [Fact]
    public void DarkPortal_OutlandBox_CoversPortalWidth_NotArrivalPad()
    {
        Assert.True(AreaTriggerReconciliation.ProximityByMap.TryGetValue(530, out var outland));
        var portal = Assert.Single(outland, e => e.LegacyId == 4352);

        Assert.True(portal.Contains(new Vector3(-247.677f, 895.675f, 84.362f)));
        Assert.True(portal.Contains(new Vector3(-247.677f - 30f, 895.675f, 84.362f)));
        Assert.True(portal.Contains(new Vector3(-247.677f + 30f, 895.675f, 84.362f)));
        Assert.False(portal.Contains(new Vector3(-248f, 922.9f, 84f)));
    }

    [Fact]
    public void WarsongFlagRooms_StillUseProximitySynth()
    {
        Assert.True(AreaTriggerReconciliation.ProximityByMap.TryGetValue(489, out var entries));
        Assert.Contains(entries, e => e.LegacyId == 3646 && e.ModernId == 4628);
        Assert.Contains(entries, e => e.LegacyId == 3647 && e.ModernId == 4629);
    }
}
