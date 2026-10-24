using System;
using System.Reflection;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// From 3.4.3 on the client reads SMSG_LOOT_START_ROLL with three ineligibility reasons after the
/// valid rolls, then the loot method and an int32 DungeonEncounterID (the 3.4.3 and 4.4.2 clients'
/// readers). Written with four reasons and no encounter, the same length, the client read the method
/// as free for all and an encounter id holding the method in its top byte.
/// </summary>
[Collection("V343ValuesFilter")]
public sealed class StartLootRollTests
{
    private static readonly PropertyInfo WorldPacketField =
        typeof(ServerPacket).GetProperty("_worldPacket", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private const string Guid = "47A033CA2E40043C";
    // map 0, 60 s to roll, need, greed and disenchant
    private const string MapTimeRolls = "00000000" + "60EA0000" + "07";
    private const string GroupLoot = "03";

    private static StartLootRoll Packet() => new()
    {
        LootObj = new WowGuid128(0x00400000002ECA33, 0x3C00040000000000),
        MapID = 0,
        RollTime = 60000,
        ValidRolls = (RollMask)7,
        Method = LootMethod.GroupLoot,
        Item = new LootItemData
        {
            Loot = new ItemInstance { ItemID = 36415 },
            Quantity = 1,
        },
    };

    private static string Item(StartLootRoll packet)
    {
        using var data = new WorldPacket(1u);
        packet.Item.Write(data);
        return Convert.ToHexString(data.GetDataSpan());
    }

    private static string ViaWrite(StartLootRoll packet)
    {
        packet.Write();
        return Convert.ToHexString(((WorldPacket)WorldPacketField.GetValue(packet)!).GetData());
    }

    private static string ViaSpan(StartLootRoll packet)
    {
        var buffer = new byte[packet.MaxSize];
        int written = packet.WriteToSpan(buffer);
        Assert.True(written > 0);
        return Convert.ToHexString(buffer.AsSpan(0, written));
    }

    [Fact]
    public void FromV343_ThreeReasonsThenTheMethodAndTheEncounter()
    {
        LootRollWire.ForceDungeonEncounterIdForTests = true;
        try
        {
            var packet = Packet();
            string expected = Guid + MapTimeRolls + "000000000000000000000000" + GroupLoot + "00000000" + Item(packet);

            Assert.Equal(expected, ViaWrite(packet));
            Assert.Equal(expected, ViaSpan(Packet()));
        }
        finally
        {
            LootRollWire.ForceDungeonEncounterIdForTests = null;
        }
    }

    [Fact]
    public void BeforeV343_TheMethodFollowsTheValidRolls()
    {
        LootRollWire.ForceDungeonEncounterIdForTests = false;
        try
        {
            var packet = Packet();
            string expected = Guid + MapTimeRolls + GroupLoot + Item(packet);

            Assert.Equal(expected, ViaWrite(packet));
            Assert.Equal(expected, ViaSpan(Packet()));
        }
        finally
        {
            LootRollWire.ForceDungeonEncounterIdForTests = null;
        }
    }
}
