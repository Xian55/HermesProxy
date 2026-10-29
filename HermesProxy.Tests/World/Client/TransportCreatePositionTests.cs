using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World.Client;

public sealed class TransportCreatePositionTests
{
    private const sbyte Elevator = (sbyte)GameObjectTypeModern.Transport;
    private const sbyte Zeppelin = (sbyte)GameObjectTypeModern.MOTransport;

    // cMaNGOS writes (0,0,0) for every MO_TRANSPORT create (GameObject::GetStationaryX); the
    // client places a zeppelin or a boat from its taxi path, so the create must still go out (#101).
    [Fact]
    public void HasUsableTransportPosition_MOTransportAtOrigin_ReturnsTrue() =>
        Assert.True(WorldClient.HasUsableTransportPosition(Zeppelin, Vector3.Zero));

    [Fact]
    public void HasUsableTransportPosition_TransportAtOrigin_ReturnsFalse() =>
        Assert.False(WorldClient.HasUsableTransportPosition(Elevator, Vector3.Zero));

    [Fact]
    public void HasUsableTransportPosition_UnknownTypeAtOrigin_ReturnsFalse() =>
        Assert.False(WorldClient.HasUsableTransportPosition(null, Vector3.Zero));

    [Theory]
    [InlineData(Elevator)]
    [InlineData(Zeppelin)]
    public void HasUsableTransportPosition_RealPosition_ReturnsTrue(sbyte gameObjectType) =>
        Assert.True(WorldClient.HasUsableTransportPosition(gameObjectType, new Vector3(1185.0225f, -4144.2085f, 70.62553f)));

    [Theory]
    [InlineData(Elevator)]
    [InlineData(Zeppelin)]
    public void HasUsableTransportPosition_NoMovementBlock_ReturnsFalse(sbyte gameObjectType) =>
        Assert.False(WorldClient.HasUsableTransportPosition(gameObjectType, null));
}
