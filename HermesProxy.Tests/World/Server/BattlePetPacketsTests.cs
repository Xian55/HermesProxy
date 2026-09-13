using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;
using V343 = HermesProxy.World.Enums.V3_4_3_54261;

namespace HermesProxy.Tests.World.Server;

public class BattlePetPacketsTests
{
    [Fact]
    public void V343_JournalAndSummon_AreMapped()
    {
        Assert.Equal(9711u, (uint)V343.Opcode.SMSG_BATTLE_PET_JOURNAL);
        Assert.Equal(13866u, (uint)V343.Opcode.CMSG_BATTLE_PET_SUMMON);
        Assert.Equal(13861u, (uint)V343.Opcode.CMSG_BATTLE_PET_REQUEST_JOURNAL);
        Assert.Equal(13873u, (uint)V343.Opcode.CMSG_BATTLE_PET_SET_FLAGS);
        Assert.Equal(13875u, (uint)V343.Opcode.CMSG_MOUNT_SET_FAVORITE);
        Assert.Equal(9709u, (uint)V343.Opcode.SMSG_BATTLE_PET_JOURNAL_LOCK_ACQUIRED);
    }

    [Fact]
    public void BattlePetJournal_Write_Empty_HasThreeLockedSlotsAndLockBit()
    {
        var journal = new BattlePetJournal { Trap = 0, HasJournalLock = true };
        for (byte i = 0; i < BattlePetJournal.SlotCount; i++)
        {
            journal.Slots.Add(new BattlePetSlot
            {
                PetGuid = WowGuid128.Create(HighGuidType703.BattlePet, 0),
                Index = i,
                Locked = true,
            });
        }

        journal.WritePacketData();
        using var reader = new WorldPacket(Frame(journal.GetData()!));

        Assert.Equal(0, reader.ReadUInt16());
        Assert.Equal(3u, reader.ReadUInt32());
        Assert.Equal(0u, reader.ReadUInt32());
        Assert.True(reader.ReadBit());
        reader.ResetBitReader();

        for (byte i = 0; i < 3; i++)
        {
            var guid = reader.ReadPackedGuid128();
            Assert.Equal(WowGuid128.Create(HighGuidType703.BattlePet, 0), guid);
            Assert.Equal(0u, reader.ReadUInt32());
            Assert.Equal(i, reader.ReadUInt8());
            Assert.True(reader.ReadBit());
            reader.ResetBitReader();
        }

        Assert.False(reader.CanRead());
    }

    [Fact]
    public void BattlePetJournal_Write_OneCompanion_WritesSpeciesAndBattlePetGuid()
    {
        var petGuid = WowGuid128.Create(HighGuidType703.BattlePet, 59);
        var journal = new BattlePetJournal { Trap = 0, HasJournalLock = true };
        journal.Slots.Add(new BattlePetSlot
        {
            PetGuid = WowGuid128.Create(HighGuidType703.BattlePet, 0),
            Index = 0,
            Locked = true,
        });
        journal.Pets.Add(new BattlePetInfo
        {
            Guid = petGuid,
            Species = 59,
            CreatureID = 7545,
            DisplayID = 0,
            Breed = 3,
            Level = 1,
            Health = 1,
            MaxHealth = 1,
            Name = string.Empty,
        });

        journal.WritePacketData();
        using var reader = new WorldPacket(Frame(journal.GetData()!));

        Assert.Equal(0, reader.ReadUInt16());
        Assert.Equal(1u, reader.ReadUInt32());
        Assert.Equal(1u, reader.ReadUInt32());
        Assert.True(reader.ReadBit());
        reader.ResetBitReader();

        _ = reader.ReadPackedGuid128();
        _ = reader.ReadUInt32();
        _ = reader.ReadUInt8();
        _ = reader.ReadBit();
        reader.ResetBitReader();

        Assert.Equal(petGuid, reader.ReadPackedGuid128());
        Assert.Equal(59u, reader.ReadUInt32());
        Assert.Equal(7545u, reader.ReadUInt32());
        Assert.Equal(0u, reader.ReadUInt32());
        Assert.Equal(3, reader.ReadUInt16());
        Assert.Equal(1, reader.ReadUInt16());
        Assert.Equal(0, reader.ReadUInt16());
        Assert.Equal(0, reader.ReadUInt16());
        Assert.Equal(0u, reader.ReadUInt32());
        Assert.Equal(1u, reader.ReadUInt32());
        Assert.Equal(1u, reader.ReadUInt32());
        Assert.Equal(0u, reader.ReadUInt32());
        Assert.Equal(0, reader.ReadUInt8());
        Assert.Equal(0u, reader.ReadBits<uint>(7));
        Assert.False(reader.ReadBit());
        Assert.False(reader.ReadBit());
    }

    [Fact]
    public void BattlePetSummon_Read_ReadsPackedGuid128()
    {
        var guid = WowGuid128.Create(HighGuidType703.BattlePet, 59);
        var payload = new WorldPacket(1u);
        payload.WritePackedGuid128(guid);

        var reader2 = new SpanPacketReader(new WorldPacket(Frame(payload.GetData())).GetRemainingSpan());
        BattlePetSummonCodec.Read(ref reader2, out var packet);

        Assert.Equal(guid, packet.PetGuid);
    }

    [Fact]
    public void BattlePetSetFlags_Read_ReadsGuidFlagsAndControlType()
    {
        var guid = WowGuid128.Create(HighGuidType703.BattlePet, 59);
        var payload = new WorldPacket(1u);
        payload.WritePackedGuid128(guid);
        payload.WriteUInt16(BattlePetInfo.FavoriteFlag);
        payload.WriteBits(BattlePetSetFlags.ControlApply, 2);
        payload.FlushBits();

        var reader2 = new SpanPacketReader(new WorldPacket(Frame(payload.GetData())).GetRemainingSpan());
        BattlePetSetFlagsCodec.Read(ref reader2, out var packet);

        Assert.Equal(guid, packet.PetGuid);
        Assert.Equal(BattlePetInfo.FavoriteFlag, packet.Flags);
        Assert.Equal(BattlePetSetFlags.ControlApply, packet.ControlType);
    }

    [Fact]
    public void BattlePetSetFlags_Read_LiveMojoFavoriteApply()
    {
        // Live CMSG_BATTLE_PET_SET_FLAGS payload after the 2-byte opcode
        // (session 20260826, Mojo species 165, Favorite APPLY).
        var payload = new WorldPacket(1u);
        payload.WriteUInt8(0x01);
        payload.WriteUInt8(0x80);
        payload.WriteUInt8(0xA5);
        payload.WriteUInt8(0xB0);
        payload.WriteUInt16(1);
        payload.WriteBits(1, 2);
        payload.FlushBits();

        var reader2 = new SpanPacketReader(new WorldPacket(Frame(payload.GetData())).GetRemainingSpan());
        BattlePetSetFlagsCodec.Read(ref reader2, out var packet);

        Assert.Equal(WowGuid128.Create(HighGuidType703.BattlePet, 165), packet.PetGuid);
        Assert.Equal(BattlePetInfo.FavoriteFlag, packet.Flags);
        Assert.Equal(BattlePetSetFlags.ControlApply, packet.ControlType);
    }

    [Fact]
    public void MountSetFavorite_Read_ReadsSpellAndBit()
    {
        var payload = new WorldPacket(1u);
        payload.WriteUInt32(40192);
        payload.WriteBit(true);
        payload.FlushBits();

        var reader2 = new SpanPacketReader(new WorldPacket(Frame(payload.GetData())).GetRemainingSpan());
        MountSetFavoriteCodec.Read(ref reader2, out var packet);

        Assert.Equal(40192u, packet.MountSpellID);
        Assert.True(packet.IsFavorite);
    }

    [Fact]
    public void AccountMountUpdate_Write_FavoriteUsesFlagBit()
    {
        var update = new AccountMountUpdate { IsFullUpdate = true };
        update.Mounts.Add((40192, AccountMountUpdate.FavoriteFlag));
        update.WritePacketData();

        using var reader = new WorldPacket(Frame(update.GetData()!));
        Assert.True(reader.ReadBit());
        Assert.Equal(1u, reader.ReadUInt32());
        Assert.Equal(40192, reader.ReadInt32());
        Assert.Equal(AccountMountUpdate.FavoriteFlag, reader.ReadBits<uint>(4));
    }

    static byte[] Frame(byte[] body)
    {
        var framed = new byte[body.Length + 2];
        body.CopyTo(framed, 2);
        return framed;
    }
}
