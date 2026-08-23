using HermesProxy.World;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World;

public class TransportGuidTests
{
    // The legacy server numbers its Transport and MOTransport spawn tables independently
    // and embeds no entry in either guid, so the same counter shows up in both.
    [Fact]
    public void SameCounter_TransportAndMoTransport_ProduceDifferentModernGuids()
    {
        var elevator = new WowGuid64(HighGuidTypeLegacy.Transport, 0, 6);
        var zeppelin = new WowGuid64(HighGuidTypeLegacy.MOTransport, 6);

        var modernElevator = WowGuid128.Create(elevator, null!);
        var modernZeppelin = WowGuid128.Create(zeppelin, null!);

        Assert.NotEqual(modernElevator, modernZeppelin);
    }

    [Theory]
    [InlineData(1u)]
    [InlineData(6u)]
    [InlineData(20u)]
    public void MoTransportGuid_RoundTripsBackToMoTransport(uint counter)
    {
        var legacy = new WowGuid64(HighGuidTypeLegacy.MOTransport, counter);

        var modern = WowGuid128.Create(legacy, null!);
        var back = WowGuid64.Create(modern);

        Assert.Equal(HighGuidTypeLegacy.MOTransport, back.GetHighGuidTypeLegacy());
        Assert.Equal(counter, (uint)back.GetCounter());
    }

    [Theory]
    [InlineData(1u)]
    [InlineData(6u)]
    public void TransportGuid_RoundTripsBackToTransport(uint counter)
    {
        var legacy = new WowGuid64(HighGuidTypeLegacy.Transport, 0, counter);

        var modern = WowGuid128.Create(legacy, null!);
        var back = WowGuid64.Create(modern);

        Assert.Equal(HighGuidTypeLegacy.Transport, back.GetHighGuidTypeLegacy());
        Assert.Equal(counter, (uint)back.GetCounter());
    }

    [Fact]
    public void BothTransportKinds_StillDecodeAsTransportOnTheModernSide()
    {
        var zeppelin = WowGuid128.Create(new WowGuid64(HighGuidTypeLegacy.MOTransport, 6), null!);

        Assert.Equal(HighGuidType.Transport, zeppelin.GetHighType());
        Assert.True(zeppelin.IsTransport());
    }
}
