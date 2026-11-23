using System;
using System.Linq;

using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

using Xunit;

namespace HermesProxy.Tests.World.Movement;

/// <summary>
/// Issue #593 — a Deeprun Tram car from a cMaNGOS classic that sends it as a plain gameobject
/// (0xF110) got no path timer in its 1.14 create, so the client ran the car on its own clock while
/// the server carried the rider down the tunnel. Such a car has to reach a 1.14 or 2.5 client
/// exactly as the same car under a transport guid does.
/// </summary>
/// <remarks>
/// Runs under every pair of <c>run-version-matrix.sh</c>. 3.4.3 and later decide the timer in their
/// own builder and are left as they were, so the comparisons skip there.
/// </remarks>
public sealed class TramPathTimerTests
{
    private static ObjectUpdate Deliver(LegacyCreate create)
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true, recordServerPackets: true);
        harness.SetActivePlayer(MovementScenarios.ActivePlayer);
        harness.Deliver(Opcode.SMSG_UPDATE_OBJECT, LegacyCreateWire.Build(create), harness.Client.HandleUpdateObject);

        var update = Assert.Single(harness.ClientWire.Sent
            .Select(s => s.Packet).OfType<UpdateObject>()
            .SelectMany(p => p.ObjectUpdates));
        update.InitializePlaceholders();
        return update;
    }

    private static byte[] MovementBlock(ObjectUpdate update)
        => CreateMovementBlock.Write(update, update.GlobalSession.GameState);

    private static void SkipFromWotLKClassicOn()
        => Assert.SkipWhen(ModernVersion.IsWotLKClassicOrLater,
            "3.4.3 and later decide the path timer in their own builder.");

    [Fact]
    public void TramUnderGameObjectGuid_IsPathTimed()
    {
        SkipFromWotLKClassicOn();

        var update = Deliver(MovementScenarios.TramAsGameObjectCreate);

        Assert.Equal(HighGuidType.GameObject, update.Guid.GetHighType());
        Assert.True(update.CreateData!.HasTransportPathTimer);
        Assert.True(update.IsPathTimedTransport);
    }

    [Fact]
    public void TramUnderGameObjectGuid_GetsTheSameMovementBlockAsUnderATransportGuid()
    {
        SkipFromWotLKClassicOn();

        byte[] asTransport = MovementBlock(Deliver(MovementScenarios.TramCreate));
        byte[] asGameObject = MovementBlock(Deliver(MovementScenarios.TramAsGameObjectCreate));

        Assert.Equal(Convert.ToHexString(asTransport), Convert.ToHexString(asGameObject));
    }

    [Fact]
    public void TramUnderGameObjectGuid_CarriesTheServersPathTimer()
    {
        SkipFromWotLKClassicOn();
        uint timer = MovementScenarios.TramAsGameObjectCreate.TransportPathTimer!.Value;

        byte[] withTimer = MovementBlock(Deliver(MovementScenarios.TramAsGameObjectCreate));
        byte[] withoutTimer = MovementBlock(Deliver(MovementScenarios.TramAsGameObjectCreate with { TransportPathTimer = null }));

        Assert.Equal(withoutTimer.Length + sizeof(uint), withTimer.Length);
        Assert.Contains(Convert.ToHexString(BitConverter.GetBytes(timer)), Convert.ToHexString(withTimer));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PathTimerOfZero_ReachesTheClientAsZero(bool underTransportGuid)
    {
        // cMaNGOS sends 0 for a car whose map has only just loaded. It used to go out as the wall
        // clock, which put the client's car somewhere else on its path.
        SkipFromWotLKClassicOn();
        var tram = underTransportGuid ? MovementScenarios.TramCreate : MovementScenarios.TramAsGameObjectCreate;

        byte[] atZero = MovementBlock(Deliver(tram with { TransportPathTimer = 0 }));
        byte[] atOne = MovementBlock(Deliver(tram with { TransportPathTimer = 1 }));

        // The two blocks differ in the timer alone, and a timer of 1 is the bytes 01 00 00 00.
        Assert.Equal(atOne.Length, atZero.Length);
        int[] differing = [.. Enumerable.Range(0, atOne.Length).Where(i => atOne[i] != atZero[i])];
        int at = Assert.Single(differing);
        Assert.Equal(0u, BitConverter.ToUInt32(atZero, at));
        Assert.Equal(1u, BitConverter.ToUInt32(atOne, at));
    }

    [Fact]
    public void TransportThatCameWithoutAPathTimer_StillGetsAClock()
    {
        SkipFromWotLKClassicOn();

        var update = Deliver(MovementScenarios.TramCreate with { TransportPathTimer = null });

        Assert.True(update.IsPathTimedTransport);
        Assert.False(update.CreateData!.HasTransportPathTimer);
        Assert.NotEqual(0u, update.CreateData.ClientPathTimer);
    }

    [Fact]
    public void TramUnderGameObjectGuid_GetsTheSamePathProgressAndPeriodAsUnderATransportGuid()
    {
        SkipFromWotLKClassicOn();

        var asTransport = Deliver(MovementScenarios.TramCreate);
        var asGameObject = Deliver(MovementScenarios.TramAsGameObjectCreate);

        Assert.NotNull(asGameObject.ObjectData.DynamicFlags);
        Assert.Equal(asTransport.ObjectData.DynamicFlags, asGameObject.ObjectData.DynamicFlags);
        Assert.Equal(asTransport.GameObjectData!.Level, asGameObject.GameObjectData!.Level);
    }

    [Fact]
    public void Type11GameObjectWithoutPathTimer_IsNotPathTimed()
    {
        var update = Deliver(MovementScenarios.TramAsGameObjectCreate with { TransportPathTimer = null });

        Assert.False(update.CreateData!.HasTransportPathTimer);
        Assert.False(update.IsPathTimedTransport);
    }

    [Fact]
    public void OtherGameObjectWithAPathTimer_IsNotPathTimed()
    {
        // Only an elevator or tram (type 11) is widened; a chest with the flag set stays as it was.
        var update = Deliver(MovementScenarios.ChestCreate with { TransportPathTimer = 29470 });

        Assert.True(update.CreateData!.HasTransportPathTimer);
        Assert.False(update.IsPathTimedTransport);
    }
}
