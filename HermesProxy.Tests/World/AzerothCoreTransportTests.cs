using System.Runtime.CompilerServices;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World;

[Collection("V343ValuesFilter")]
public class AzerothCoreTransportTests
{
    private static readonly WowGuid128 Boat = WowGuid128.Create(HighGuidType703.Transport, 1);

    private static GlobalSessionData Session()
    {
        var session = (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));
        session.GameState = GameSessionData.CreateNewGameSessionData(session);
        return session;
    }

    private static ObjectUpdate Create(GlobalSessionData session, WowGuid128 guid, sbyte type, int level,
        sbyte state = 1, uint dynamicFlags = 0xFFFF0000)
    {
        var update = new ObjectUpdate(guid, UpdateTypeModern.CreateObject1, session);
        update.CreateData.MoveInfo = new MovementInfo();
        update.GameObjectData.TypeID = type;
        update.GameObjectData.Level = level;
        update.GameObjectData.State = state;
        update.ObjectData.DynamicFlags = dynamicFlags;
        update.ObjectData.EntryID = 20808;
        update.InitializePlaceholders();
        return update;
    }

    [Fact]
    public void JoiningAnActiveAzerothCoreCrossing_UsesRemainingPathTime()
    {
        ObjectUpdate.ForceTransportV343ForTests = true;
        try
        {
            var session = Session();
            uint before = Time.GetMSTime();
            var create = Create(session, Boat, (sbyte)GameObjectTypeModern.Transport, 60133,
                state: 0, dynamicFlags: 0x80000000);
            uint after = Time.GetMSTime();

            uint deadline = (uint)create.GameObjectData.Level!.Value;
            Assert.Equal((sbyte)25, create.GameObjectData.State);
            Assert.InRange(unchecked(deadline - before), 30000u, 30100u);
            Assert.InRange(unchecked(deadline - after), 29900u, 30100u);
            Assert.Equal(0u, create.ObjectData.DynamicFlags);
        }
        finally
        {
            ObjectUpdate.ForceTransportV343ForTests = null;
        }
    }

    [Fact]
    public void AzerothCoreBoat_ParksSailsOnceAndIgnoresRepeatedPathProgress()
    {
        ObjectUpdate.ForceTransportV343ForTests = true;
        try
        {
            var session = Session();
            var create = Create(session, Boat, (sbyte)GameObjectTypeModern.Transport, 60133);

            Assert.Equal(60133u, create.TransportStopFrame);
            Assert.NotNull(create.TransportServerTime);
            Assert.Equal((sbyte)24, create.GameObjectData.State);
            Assert.Equal(0u, create.ObjectData.DynamicFlags);

            var flip = new ObjectUpdate(Boat, UpdateTypeModern.Values, session);
            flip.GameObjectData.TypeID = (sbyte)GameObjectTypeModern.Transport;
            flip.GameObjectData.State = 0;
            flip.ObjectData.DynamicFlags = 0x80000000;
            flip.ApplyTransportGameObjectFixups();

            Assert.Equal((sbyte)25, flip.GameObjectData.State);
            Assert.Equal(0u, flip.ObjectData.DynamicFlags);
            uint deadline = (uint)flip.GameObjectData.Level!.Value;
            Assert.Equal(deadline, session.GameState.SynthesizedTransports[Boat].SailDeadline);

            var repeated = new ObjectUpdate(Boat, UpdateTypeModern.Values, session);
            repeated.GameObjectData.TypeID = (sbyte)GameObjectTypeModern.Transport;
            repeated.GameObjectData.State = 0;
            repeated.ObjectData.DynamicFlags = 0xFFFF0000;
            repeated.ApplyTransportGameObjectFixups();
            Assert.Equal((int)deadline, repeated.GameObjectData.Level);
            Assert.Equal(0u, repeated.ObjectData.DynamicFlags);

            var progressOnly = new ObjectUpdate(Boat, UpdateTypeModern.Values, session);
            progressOnly.ObjectData.DynamicFlags = 0x12340000;
            progressOnly.ApplyTransportGameObjectFixups();
            Assert.Equal(0u, progressOnly.ObjectData.DynamicFlags);
            Assert.Null(progressOnly.GameObjectData.Level);

            var recreated = Create(session, Boat, (sbyte)GameObjectTypeModern.Transport, 60133,
                state: 0);
            Assert.Equal((int)deadline, recreated.GameObjectData.Level);
        }
        finally
        {
            ObjectUpdate.ForceTransportV343ForTests = null;
        }
    }

    [Theory]
    [InlineData((sbyte)GameObjectTypeModern.MOTransport, 60133)]
    [InlineData((sbyte)GameObjectTypeModern.Transport, 0)]
    public void LoopingTransportWithoutType11StopFrame_IsUnchanged(sbyte type, int level)
    {
        ObjectUpdate.ForceTransportV343ForTests = true;
        try
        {
            var session = Session();
            var create = Create(session, Boat, type, level);

            Assert.Null(create.TransportStopFrame);
            Assert.Null(create.TransportServerTime);
            Assert.Equal((sbyte)1, create.GameObjectData.State);
            Assert.Equal(0xFFFF0000u, create.ObjectData.DynamicFlags);
            Assert.Empty(session.GameState.SynthesizedTransports);
        }
        finally
        {
            ObjectUpdate.ForceTransportV343ForTests = null;
        }
    }
}
