using System;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// SMSG_PET_NAME_INVALID as the 4.4.2 client reads it (TrinityCore cata_classic PetNameInvalid::Write;
/// docs/protocol/4.4.2.60895: the result byte, the pet's packed GUID, its number, the name's length
/// and a declined-names bit, then the name).
/// </summary>
public sealed class PetNameInvalidTests
{
    [Fact]
    public void Write_ResultPetNumberThenName()
    {
        var packet = new PetNameInvalid
        {
            Result = 3,
            PetGUID = new WowGuid128(0x19, 0x2800000000000000),
            PetNumber = 8,
            NewName = "Ab",
        };

        packet.WritePacketData();

        string expected = "03" + "0180" + "19" + "28" + "08000000" + "02" + "00" + "4162";
        Assert.Equal(expected, Convert.ToHexString(packet.GetData()!));
    }
}
