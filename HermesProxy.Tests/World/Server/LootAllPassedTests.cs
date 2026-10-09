using System;
using System.Reflection;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// The 3.4.3 client reads SMSG_LOOT_ALL_PASSED as the loot object, an int32 DungeonEncounterID, then the
/// item, as TrinityCore's 3.4.3 writer sends it. Without the encounter id the client read the item 4 bytes
/// late, so "everyone passed" named item 0 and the read ran past the end of the packet (#360).
/// </summary>
[Collection("V343ValuesFilter")]
public class LootAllPassedTests
{
    private static readonly PropertyInfo WorldPacketField =
        typeof(ServerPacket).GetProperty("_worldPacket", BindingFlags.NonPublic | BindingFlags.Instance)!;

    // Captured from a 3.4.3 session on AzerothCore playerbots before the fix: everyone passed on item 36415.
    private const string Guid = "47A033CA2E40043C";
    private const string Item = "00" + "3F8E0000" + "F3FFFFFF" + "3A000000" + "00" + "00" + "01000000" + "00" + "00";

    private static LootAllPassed Packet() => new()
    {
        LootObj = new WowGuid128(0x00400000002ECA33, 0x3C00040000000000),
        Item = new LootItemData
        {
            Loot = new ItemInstance { ItemID = 36415, RandomPropertiesSeed = 0xFFFFFFF3, RandomPropertiesID = 58 },
            Quantity = 1,
        },
    };

    private static string ViaWrite(LootAllPassed packet)
    {
        packet.Write();
        return Convert.ToHexString(((WorldPacket)WorldPacketField.GetValue(packet)!).GetData());
    }

    [Fact]
    public void Write_OnV343_PutsTheEncounterIdBetweenTheLootObjectAndTheItem()
    {
        LootRollWire.ForceDungeonEncounterIdForTests = true;
        try
        {
            Assert.Equal(Guid + "00000000" + Item, ViaWrite(Packet()));
        }
        finally
        {
            LootRollWire.ForceDungeonEncounterIdForTests = null;
        }
    }

    [Fact]
    public void Write_BeforeV343_SendsTheItemRightAfterTheLootObject()
    {
        LootRollWire.ForceDungeonEncounterIdForTests = false;
        try
        {
            Assert.Equal(Guid + Item, ViaWrite(Packet()));
        }
        finally
        {
            LootRollWire.ForceDungeonEncounterIdForTests = null;
        }
    }
}
