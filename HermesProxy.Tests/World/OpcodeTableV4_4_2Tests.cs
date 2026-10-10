using System;
using System.Linq;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// The 4.4.2.60895 opcode table: the 3.4.3.54261 names, with the 4.4.2 client's values.
/// </summary>
public class OpcodeTableV4_4_2Tests
{
    [Fact]
    public void FourByteOpcodesSplitIntoGroups()
    {
        Assert.True(GeneratedOpcodeTables.TryGet(ClientVersionBuild.V4_4_2_60895, out Opcode[][] groups, out _, out int opcodeSize));

        Assert.Equal(sizeof(uint), opcodeSize);
        Assert.True(groups.Length > 1);
    }

    // TrinityCore cata_classic Opcodes.h; the in-world ones also in docs/protocol/4.4.2.60895.
    [Theory]
    [InlineData(Opcode.CMSG_AUTH_SESSION, 0x3A0001u)]
    [InlineData(Opcode.CMSG_PING, 0x3A0004u)]
    [InlineData(Opcode.CMSG_ENUM_CHARACTERS, 0x390014u)]
    [InlineData(Opcode.SMSG_AUTH_CHALLENGE, 0x420000u)]
    [InlineData(Opcode.SMSG_ENTER_ENCRYPTED_MODE, 0x420001u)]
    [InlineData(Opcode.SMSG_COMPRESSED_PACKET, 0x42000Au)]
    [InlineData(Opcode.SMSG_AUTH_RESPONSE, 0x3B0001u)]
    [InlineData(Opcode.SMSG_UPDATE_OBJECT, 0x4B0000u)]
    // The 3.4.3 request HermesProxy calls OWNED has the layout of 4.4.2's OWNER request.
    [InlineData(Opcode.CMSG_AUCTION_LIST_OWNED_ITEMS, 0x350059u)]
    public void MapsBothWays(Opcode universal, uint current)
    {
        Assert.True(GeneratedOpcodeTables.TryGet(ClientVersionBuild.V4_4_2_60895, out Opcode[][] groups, out uint[] universalToCurrent, out _));

        Assert.Equal(current, universalToCurrent[(int)universal]);
        Assert.Equal(universal, groups[current >> 16][current & 0xFFFF]);
    }

    [Fact]
    public void HasThe3_4_3Names()
    {
        Assert.Equal(
            Enum.GetNames<global::HermesProxy.World.Enums.V3_4_3_54261.Opcode>().Order(),
            Enum.GetNames<global::HermesProxy.World.Enums.V4_4_2_60895.Opcode>().Order());
    }

    /// <summary>
    /// Every opcode both builds have reaches the same universal opcode, so a 4.4.2 packet takes the
    /// handler its 3.4.3 counterpart takes. Where several names share one opcode, the same one wins.
    /// </summary>
    [Fact]
    public void ResolvesEverySharedOpcodeLike3_4_3()
    {
        Assert.True(GeneratedOpcodeTables.TryGet(ClientVersionBuild.V3_4_3_54261, out Opcode[][] groups343, out uint[] toCurrent343, out _));
        Assert.True(GeneratedOpcodeTables.TryGet(ClientVersionBuild.V4_4_2_60895, out Opcode[][] groups442, out uint[] toCurrent442, out _));

        int shared = 0;
        for (int universal = 0; universal < toCurrent343.Length && universal < toCurrent442.Length; universal++)
        {
            uint current343 = toCurrent343[universal];
            uint current442 = toCurrent442[universal];
            if (current343 == 0 || current442 == 0)
                continue;

            Assert.Equal(groups343[0][current343], groups442[current442 >> 16][current442 & 0xFFFF]);
            shared++;
        }

        Assert.True(shared > 900, $"only {shared} opcodes shared");
    }
}
