using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HermesProxy;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;
using ObjectUpdateBuilder = HermesProxy.World.Objects.Version.V4_4_2_60895.ObjectUpdateBuilder;

namespace HermesProxy.Tests.World.Objects;

/// <summary>
/// The AnimKits in a 4.4.2 create. A 4.3.4 server's create gives a unit its AI, movement and melee
/// kits; the 4.4.2 client takes them as one block of three u16, there when any of the three is set
/// (TrinityCore cata_classic BuildMovementUpdate). The proxy wrote no such block.
/// </summary>
public sealed class CreateAnimKitWriterTests
{
    [Fact]
    public void AKitFromTheServer_GoesInTheCreate()
    {
        var update = Creature(out var session);
        update.CreateData!.AIAnimKitID = 770;

        byte[] bytes = Movement(update, session);

        // the kits come last here: AI 770, no movement or melee kit
        Assert.Equal([0x02, 0x03, 0, 0, 0, 0], bytes[^6..]);
        Assert.Equal(AnimKitBlock, bytes.Length - BareLength(session));
    }

    [Fact]
    public void NoKit_NoBlock()
    {
        var update = Creature(out var session);

        Assert.Equal(BareLength(session), Movement(update, session).Length);
    }

    private const int AnimKitBlock = 6;

    private static int BareLength(GameSessionData session) => Movement(Creature(out _), session).Length;

    private static byte[] Movement(ObjectUpdate update, GameSessionData session)
    {
        using var data = new WorldPacket(1u);
        var builder = new ObjectUpdateBuilder(update, session);
        builder.SetCreateObjectBits();
        builder.BuildMovementUpdate(data);
        return data.GetData();
    }

    private static ObjectUpdate Creature(out GameSessionData session)
    {
        session = (GameSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GameSessionData));
        typeof(GameSessionData).GetField(nameof(GameSessionData.OriginalObjectTypes))!
            .SetValue(session, new Dictionary<WowGuid128, ObjectType>());
        var globalSession = (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));

        var guid = WowGuid128.Create(HighGuidType703.Creature, 0, 43508, 1);
        var update = new ObjectUpdate(guid, UpdateTypeModern.CreateObject1, globalSession);
        update.CreateData!.ObjectType = ObjectType.Unit;
        return update;
    }
}
