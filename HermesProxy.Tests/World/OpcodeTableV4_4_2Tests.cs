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
    // 4.4.2 has the 3.4.3 request (there OWNED, here OWNER at 0x350059) and a retail-style one of
    // the same name. Its auction house sends only the retail-style one.
    [InlineData(Opcode.CMSG_AUCTION_LIST_OWNED_ITEMS, 0x35005Fu)]
    [InlineData(Opcode.CMSG_AUCTION_LIST_BIDDED_ITEMS, 0x350060u)]
    [InlineData(Opcode.CMSG_AUCTION_BROWSE_QUERY, 0x35005Cu)]
    [InlineData(Opcode.SMSG_AUCTION_LIST_BUCKETS_RESULT, 0x3B02E5u)]
    public void MapsBothWays(Opcode universal, uint current)
    {
        Assert.True(GeneratedOpcodeTables.TryGet(ClientVersionBuild.V4_4_2_60895, out Opcode[][] groups, out uint[] universalToCurrent, out _));

        Assert.Equal(current, universalToCurrent[(int)universal]);
        Assert.Equal(universal, groups[current >> 16][current & 0xFFFF]);
    }

    /// <summary>
    /// The table is the 3.4.3 one revalued, so that a packet both builds have takes the same
    /// handler. What 4.4.2 has beyond it is listed here by hand: the retail-style auction house,
    /// and the addon message with a target, which 3.4.3 sends through the plain addon opcode.
    /// </summary>
    [Fact]
    public void HasThe3_4_3Names_AndTheRetailStyleAuctionHouse()
    {
        string[] only442 =
        [
            "CMSG_AUCTION_BROWSE_QUERY",
            "CMSG_AUCTION_LIST_ITEMS_BY_BUCKET_KEY",
            "CMSG_AUCTION_LIST_ITEMS_BY_ITEM_ID",
            "CMSG_AUCTION_LIST_BUCKETS_BY_BUCKET_KEYS",
            "CMSG_AUCTION_GET_COMMODITY_QUOTE",
            "CMSG_AUCTION_CONFIRM_COMMODITIES_PURCHASE",
            "CMSG_AUCTION_CANCEL_COMMODITIES_PURCHASE",
            "CMSG_AUCTION_SELL_COMMODITY",
            "CMSG_AUCTION_SET_FAVORITE_ITEM",
            "SMSG_AUCTION_LIST_BUCKETS_RESULT",
            "SMSG_AUCTION_GET_COMMODITY_QUOTE_RESULT",
            "SMSG_AUCTION_FAVORITE_LIST",
        ];

        Assert.Equal(
            Enum.GetNames<global::HermesProxy.World.Enums.V3_4_3_54261.Opcode>().Concat(only442).Order(),
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
