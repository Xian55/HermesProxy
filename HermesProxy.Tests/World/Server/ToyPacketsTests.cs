using System;
using System.IO;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server;
using HermesProxy.World.Server.Packets;
using Xunit;
using V343 = HermesProxy.World.Enums.V3_4_3_54261;

namespace HermesProxy.Tests.World.Server;

public class ToyPacketsTests
{
    [Fact]
    public void V343_AddAndUseToy_AreMappedNextToUseItem()
    {
        Assert.Equal(12952u, (uint)V343.Opcode.CMSG_USE_ITEM);
        Assert.Equal(12953u, (uint)V343.Opcode.CMSG_ADD_TOY);
        Assert.Equal(12954u, (uint)V343.Opcode.CMSG_USE_TOY);
        Assert.Equal(9648u, (uint)V343.Opcode.SMSG_ACCOUNT_TOY_UPDATE);
        Assert.Equal(13876u, (uint)V343.Opcode.CMSG_COLLECTION_ITEM_SET_FAVORITE);
        Assert.Equal(12584u, (uint)V343.Opcode.CMSG_TOY_CLEAR_FANFARE);
    }

    [Fact]
    public void UseToy_CastFailedReasons_MatchV343()
    {
        Assert.Equal(33u, (uint)SpellCastResultV343.EquippedItem);
        Assert.Equal(67u, (uint)SpellCastResultV343.ItemNotFound);
        Assert.True(EquipmentSlot.End <= HermesProxy.World.Enums.Vanilla.InventorySlots.ItemStart);
        Assert.Equal(12, EquipmentSlot.Trinket1);
    }

    [Fact]
    public void AccountToyUpdate_Write_Empty_MatchesInitStub()
    {
        var update = new AccountToyUpdate { IsFullUpdate = true };
        update.WritePacketData();
        using var reader = new WorldPacket(Frame(update.GetData()!));

        Assert.True(reader.ReadBit());
        reader.ResetBitReader();
        Assert.Equal(0, reader.ReadInt32());
        Assert.Equal(0, reader.ReadInt32());
        Assert.Equal(0, reader.ReadInt32());
    }

    [Fact]
    public void AccountToyUpdate_Write_OneFavoriteToy()
    {
        var update = new AccountToyUpdate { IsFullUpdate = true };
        update.Toys.Add((1973u, true, false));
        update.WritePacketData();
        using var reader = new WorldPacket(Frame(update.GetData()!));

        Assert.True(reader.ReadBit());
        reader.ResetBitReader();
        Assert.Equal(1, reader.ReadInt32());
        Assert.Equal(1, reader.ReadInt32());
        Assert.Equal(1, reader.ReadInt32());
        Assert.Equal(1973u, reader.ReadUInt32());
        Assert.True(reader.ReadBit());
        Assert.False(reader.ReadBit());
    }

    [Fact]
    public void AddToy_Read_ReadsPackedGuid()
    {
        var guid = WowGuid128.Create(HighGuidType703.Item, 42);
        var payload = new WorldPacket(1u);
        payload.WritePackedGuid128(guid);

        var r = new SpanPacketReader(
            new WorldPacket(Frame(payload.GetData())).GetRemainingSpan());
        AddToyCodec.Read(ref r, out var packet);
        Assert.Equal(guid, packet.Guid);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void CollectionItemSetFavorite_Read_ToyStar()
    {
        var payload = new WorldPacket(1u);
        payload.WriteInt32((int)ItemCollectionType.Toy);
        payload.WriteUInt32(1973);
        payload.WriteBit(true);
        payload.FlushBits();

        var r = new SpanPacketReader(
            new WorldPacket(Frame(payload.GetData())).GetRemainingSpan());
        CollectionItemSetFavoriteCodec.Read(ref r, out var packet);
        Assert.Equal(ItemCollectionType.Toy, packet.Type);
        Assert.Equal(1973u, packet.ID);
        Assert.True(packet.IsFavorite);
    }

    [Fact]
    public void CollectionFavorites_RoundTrip_KeepsLearnedToys()
    {
        string account = "HermesToyTests_" + Guid.NewGuid().ToString("N");
        var mgr = new AccountMetaDataManager(account);
        var saved = new CollectionFavorites();
        saved.LearnedToys.Add(1973);
        saved.FavoriteToys.Add(1973);
        mgr.SaveCollectionFavorites(saved);

        var loaded = new AccountMetaDataManager(account).LoadCollectionFavorites();
        Assert.Contains(1973u, loaded.LearnedToys);
        Assert.Contains(1973u, loaded.FavoriteToys);

        Directory.Delete(Path.GetFullPath(Path.Combine("AccountData", account)), recursive: true);
    }

    static byte[] Frame(byte[] body)
    {
        var framed = new byte[body.Length + 2];
        body.CopyTo(framed, 2);
        return framed;
    }
}
