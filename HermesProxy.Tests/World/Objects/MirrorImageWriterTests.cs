using System;
using System.Collections.Generic;
using System.Reflection;
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
/// A Mage's Mirror Image for the 4.4.2 client. The client asks what an image looks like only when
/// the image carries UNIT_FLAG2_MIRROR_IMAGE (0x10); the proxy used to clear that bit on every
/// creature (it is also NPCBot's "cloned" bit), so the images were drawn as a bare race model.
/// </summary>
public sealed class MirrorImageWriterTests
{
    private const uint MirrorImageFlag = 0x10;

    private static readonly PropertyInfo WorldPacketField =
        typeof(ServerPacket).GetProperty("_worldPacket", BindingFlags.NonPublic | BindingFlags.Instance)!;

    [Theory]
    [InlineData(31216u, 0x30u)]
    [InlineData(1234u, 0x20u)]
    public void Flags2_KeepsTheMirrorImageBitOnlyForTheMagesImages(uint entry, uint expectedFlags2)
    {
        var session = (GameSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GameSessionData));
        typeof(GameSessionData).GetField(nameof(GameSessionData.OriginalObjectTypes))!
            .SetValue(session, new Dictionary<WowGuid128, ObjectType>());
        var globalSession = (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));

        var guid = WowGuid128.Create(HighGuidType703.Creature, 0, entry, 1);
        var update = new ObjectUpdate(guid, UpdateTypeModern.Values, globalSession);
        update.UnitData ??= new UnitData();
        update.UnitData.Flags2 = MirrorImageFlag | 0x20u;

        using var data = new WorldPacket(1u);
        new ObjectUpdateBuilder(update, session).WriteUpdateUnitData(data);
        byte[] bytes = data.GetData();

        Assert.Equal(expectedFlags2, BitConverter.ToUInt32(bytes, bytes.Length - 4));
    }

    /// <summary>
    /// The answer a 4.4.2 client got for one image of a human female mage, and drew as a copy of
    /// the caster: model 50, race, sex and class, five customization choices, no guild, eleven item
    /// displays (chest, legs and feet worn).
    /// </summary>
    [Fact]
    public void ComponentedData_WritesTheCapturedAnswer()
    {
        var image = new MirrorImageComponentedData
        {
            UnitGUID = new WowGuid128(0xB666UL, 0x20000400001E7C00UL),
            DisplayID = 50,
            RaceID = 1,
            Gender = 1,
            ClassID = 8,
            Customizations =
            [
                new ChrCustomizationChoice(14, 17224),
                new ChrCustomizationChoice(15, 17232),
                new ChrCustomizationChoice(16, 17242),
                new ChrCustomizationChoice(17, 17267),
                new ChrCustomizationChoice(18, 17277),
            ],
            ItemDisplayIDs = [0, 0, 0, 12647, 0, 1882, 9929, 0, 0, 0, 0],
        };

        image.Write();
        string hex = Convert.ToHexString(((WorldPacket)WorldPacketField.GetValue(image)!).GetData());

        Assert.Equal(
            "03A666B67C1E0420" + "32000000" + "010108" + "05000000" + "0000" + "0B000000"
            + "0E000000" + "48430000" + "0F000000" + "50430000" + "10000000" + "5A430000"
            + "11000000" + "73430000" + "12000000" + "7D430000"
            + "00000000" + "00000000" + "00000000" + "67310000" + "00000000" + "5A070000"
            + "C9260000" + "00000000" + "00000000" + "00000000" + "00000000",
            hex);
    }
}
