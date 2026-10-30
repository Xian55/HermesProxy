using System;
using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using HermesProxy.World.Server.Systems;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// An exact-match auction search on a legacy server, which has no such search (#537). The byte
/// after "usable" in the legacy request is getAll, and set it dumps the whole house, so the flag
/// stays out of it and the server's answer is narrowed on its way to the client.
/// </summary>
public sealed class AuctionExactMatchTests : IDisposable
{
    private const uint Cloth = 910001;
    private const uint Bolt = 910002;
    private const uint Tunic = 910003;
    private const uint OnlyInItemTable = 910004;
    private const uint Stranger = 910005;

    private const uint NoSuffix = 0;
    private const uint Suffix = 476;

    private static readonly WowGuid128 Npc = new(0x155A, 0x2000040000004AC0);

    private readonly LegacyHandlerHarness _harness = new(recordClientPackets: true, recordServerPackets: true);

    public AuctionExactMatchTests()
    {
        GameData.StoreItemName(Cloth, "Linen Cloth");
        GameData.StoreItemName(Bolt, "Bolt of Linen Cloth");
        GameData.StoreItemName(Tunic, "Pioneer Tunic");
        GameData.ItemSparseRecordsStore[OnlyInItemTable] = new GameData.ItemSparseRecord { Name1 = "Light Leather" };
    }

    public void Dispose()
    {
        foreach (uint entry in new[] { Cloth, Bolt, Tunic })
            GameData.ItemNames.TryRemove(entry, out _);
        GameData.ItemSparseRecordsStore.TryRemove(OnlyInItemTable, out _);
    }

    private void SearchFor(string name, bool exact)
    {
        var search = new AuctionListItems(
            Offset: 0, Auctioneer: Npc, MinLevel: 0, MaxLevel: 0, Quality: -1, MaxPetLevel: 0,
            KnownPets: [], Name: name, OnlyUsable: false, ExactMatch: exact,
            ClassFilters: [], Sorts: [new AuctionSort(5, 1)]);
        var ctx = new SessionContext(_harness.Session, socket: null, _harness.Client);
        AuctionSystem.HandleAuctionListItems(in search, in ctx);
    }

    private static void WriteMoney(WorldPacket packet)
    {
        if (WorldClient.IsCataLegacy)
            packet.WriteUInt64(100);
        else
            packet.WriteUInt32(100);
    }

    private AuctionListItemsResult Answer(int total, params (uint Item, uint RandomProperties)[] rows)
    {
        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_AUCTION_LIST_ITEMS_RESULT, packet =>
        {
            packet.WriteUInt32((uint)rows.Length);
            uint auctionId = 1;
            foreach ((uint item, uint randomProperties) in rows)
            {
                packet.WriteUInt32(auctionId++);
                packet.WriteUInt32(item);
                int enchantmentFields = (WorldClient.IsCataLegacy ? 10 : 7) * 3;
                for (int i = 0; i < enchantmentFields; i++)
                    packet.WriteUInt32(0);
                packet.WriteUInt32(randomProperties);
                packet.WriteUInt32(0);      // seed
                packet.WriteInt32(1);       // count
                packet.WriteInt32(0);       // charges
                packet.WriteUInt32(0);      // flags
                packet.WriteUInt64(0);      // owner
                WriteMoney(packet);         // min bid
                WriteMoney(packet);         // min increment
                WriteMoney(packet);         // buyout
                packet.WriteInt32(3600000); // duration left
                packet.WriteUInt64(0);      // bidder
                WriteMoney(packet);         // bid
            }
            packet.WriteInt32(total);
            packet.WriteUInt32(300);        // desired delay
        });

        _harness.Deliver(Opcode.SMSG_AUCTION_LIST_ITEMS_RESULT, wire, _harness.Client.HandleAuctionListItemsResult);
        return _harness.ClientWire.Sent.Select(sent => sent.Packet).OfType<AuctionListItemsResult>().Last();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NoSearchAsksForTheWholeHouse(bool exact)
    {
        SearchFor("Linen Cloth", exact);

        // ... usable, getAll, 4.3.4's byte, sort count, sort type, sort direction
        byte[] sent = Assert.Single(_harness.ServerWire.Sent).Bytes;
        Assert.Equal(1, sent[^3]);
        if (WorldClient.IsCataLegacy)
        {
            Assert.Equal(exact ? 1 : 0, sent[^4]);
            Assert.Equal(0, sent[^5]);
        }
        else
        {
            Assert.Equal(0, sent[^4]);
        }
    }

    [Fact]
    public void AnExactSearch_AnswersWithThatNameOnly()
    {
        SearchFor("Linen Cloth", exact: true);

        AuctionListItemsResult result = Answer(total: 3, (Bolt, NoSuffix), (Cloth, NoSuffix), (Cloth, NoSuffix));

        Assert.Equal([Cloth, Cloth], result.Items.Select(auction => auction.Item.ItemID));
        Assert.Equal(2, result.TotalItemsCount);
    }

    [Fact]
    public void AnExactSearch_KeepsTheNameARandomSuffixGivesAnItem()
    {
        SearchFor("Pioneer Tunic of the Whale", exact: true);

        AuctionListItemsResult result = Answer(total: 1, (Tunic, Suffix));

        Assert.Equal(Tunic, Assert.Single(result.Items).Item.ItemID);
    }

    [Fact]
    public void AnExactSearch_KeepsAnItemWhoseNameIsNotKnown()
    {
        SearchFor("明丝束带", exact: true);

        AuctionListItemsResult result = Answer(total: 1, (Stranger, NoSuffix));

        Assert.Equal(Stranger, Assert.Single(result.Items).Item.ItemID);
    }

    [Fact]
    public void APageOfALongerList_KeepsTheServersTotal()
    {
        SearchFor("Linen Cloth", exact: true);

        AuctionListItemsResult result = Answer(total: 60, (Bolt, NoSuffix), (Cloth, NoSuffix));

        Assert.Single(result.Items);
        Assert.Equal(60, result.TotalItemsCount);
    }

    [Fact]
    public void AnotherSearch_IsNotNarrowed()
    {
        SearchFor("Linen Cloth", exact: true);
        Answer(total: 1, (Cloth, NoSuffix));

        SearchFor("Linen Cloth", exact: false);
        AuctionListItemsResult result = Answer(total: 2, (Bolt, NoSuffix), (Cloth, NoSuffix));

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalItemsCount);
    }

    [Theory]
    [InlineData("Linen Cloth", "Linen Cloth", NoSuffix, true)]
    [InlineData("Linen Cloth", "linen cloth", NoSuffix, true)]
    [InlineData("Bolt of Linen Cloth", "Linen Cloth", NoSuffix, false)]
    [InlineData("Linen Cloth", "Linen", NoSuffix, false)]
    [InlineData("Linen Cloth", "Linen Cloth of Spirit", NoSuffix, false)]
    [InlineData("Pioneer Tunic", "Pioneer Tunic", Suffix, true)]
    [InlineData("Pioneer Tunic", "Pioneer Tunic of Spirit", Suffix, true)]
    [InlineData("Pioneer Tunic", "Pioneer Tunics", Suffix, false)]
    [InlineData("", "Anything", NoSuffix, true)]
    public void IsExactMatch(string itemName, string searched, uint randomProperties, bool expected)
    {
        var auction = new AuctionItem { Item = new ItemInstance { ItemID = Cloth, RandomPropertiesID = randomProperties } };

        Assert.Equal(expected, RetailAuctionShape.IsExactMatch(auction, itemName, searched));
    }

    [Fact]
    public void IsExactMatch_NoItem_IsNoMatch()
        => Assert.False(RetailAuctionShape.IsExactMatch(new AuctionItem(), "Linen Cloth", "Linen Cloth"));

    [Theory]
    [InlineData(Cloth, "enUS", "Linen Cloth")]
    [InlineData(Cloth, "zhCN", "Linen Cloth")]
    [InlineData(OnlyInItemTable, "enUS", "Light Leather")]
    [InlineData(OnlyInItemTable, "zhCN", "")]
    [InlineData(OnlyInItemTable, null, "")]
    [InlineData(Stranger, "enUS", "")]
    public void SearchNameOf_TrustsTheEnglishItemTableForEnglishClientsOnly(uint item, string? locale, string expected)
        => Assert.Equal(expected, AuctionItemFacts.SearchNameOf(item, locale));
}
