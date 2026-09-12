using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server;

public partial class WorldSocket
{


    [PacketHandler(Opcode.CMSG_BATTLE_PET_SUMMON)]
    void HandleBattlePetSummon(BattlePetSummon summon)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        if (!GetSession().GameState.BattlePetGuidToSummonSpell.TryGetValue(summon.PetGuid, out uint spellId)
            || spellId == 0)
            return;

        bool dismiss = GetSession().GameState.SummonedBattlePetGuid == summon.PetGuid
            && !summon.PetGuid.IsEmpty();

        if (dismiss)
        {
            DismissSummonedCompanion();
            return;
        }

        // Stamp the journal GUID before the legacy cast so the companion
        // CreateObject can carry BattlePetCompanionGUID (GetSummonedPetGUID).
        SetSummonedBattlePet(summon.PetGuid, (uint)summon.PetGuid.GetCounter());
        CastCompanionSpell(spellId);
    }

    [PacketHandler(Opcode.CMSG_BATTLE_PET_SET_FLAGS)]
    void HandleBattlePetSetFlags(BattlePetSetFlags setFlags)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        var favorites = Systems.BattlePetSystem.EnsureCollectionFavorites(GetSession());
        uint speciesId = (uint)setFlags.PetGuid.GetCounter();
        if (speciesId == 0)
            return;

        if ((setFlags.Flags & BattlePetInfo.FavoriteFlag) != 0)
        {
            if (setFlags.ControlType == BattlePetSetFlags.ControlRemove)
                favorites.FavoritePetSpecies.Remove(speciesId);
            else
                favorites.FavoritePetSpecies.Add(speciesId);
            GetSession().AccountMetaDataMgr.SaveCollectionFavorites(favorites);
        }

        SendPacket(BattlePetJournal.FromSession(GetSession().GameState));
    }

    [PacketHandler(Opcode.CMSG_MOUNT_SET_FAVORITE)]
    void HandleMountSetFavorite(MountSetFavorite setFavorite)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        var favorites = Systems.BattlePetSystem.EnsureCollectionFavorites(GetSession());
        if (setFavorite.IsFavorite)
            favorites.FavoriteMountSpells.Add(setFavorite.MountSpellID);
        else
            favorites.FavoriteMountSpells.Remove(setFavorite.MountSpellID);
        GetSession().AccountMetaDataMgr.SaveCollectionFavorites(favorites);
        SendPacket(AccountMountUpdate.FromSession(GetSession().GameState));
    }

    void CastCompanionSpell(uint spellId)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_CAST_SPELL);
        packet.WriteUInt8(0);
        packet.WriteUInt32(spellId);
        packet.WriteUInt8(0);
        packet.WriteUInt32(0);
        SendPacketToServer(packet);
    }

    void DismissSummonedCompanion()
    {
        var state = GetSession().GameState;
        if (!state.SummonedCompanionLegacyGuid.IsEmpty())
        {
            // AC 3.3.5a ObjectGuid >> reads a raw uint64. Packed guid is ignored.
            // Recast of the companion spell replaces the pet, it does not dismiss.
            WorldPacket packet = new WorldPacket(Opcode.CMSG_DISMISS_CRITTER);
            packet.WriteGuid(state.SummonedCompanionLegacyGuid);
            SendPacketToServer(packet);
        }

        SetSummonedBattlePet(WowGuid128.Empty, 0);
        state.SummonedCompanionCreatureGuid = WowGuid128.Empty;
    }

    void SetSummonedBattlePet(WowGuid128 guid, uint speciesId)
    {
        var session = GetSession();
        session.GameState.SummonedBattlePetGuid = guid;
        if (guid.IsEmpty())
        {
            session.GameState.SummonedCompanionCreatureGuid = WowGuid128.Empty;
            session.GameState.SummonedCompanionLegacyGuid = WowGuid64.Empty;
        }
        session.GameState.CurrentPlayerStorage?.Settings?.SetLastSummonedPetSpecies(speciesId);

        CollectionSync.SendSummonedBattlePet(session);
    }

    [PacketHandler(Opcode.CMSG_DISMISS_CRITTER)]
    void HandleDismissCritter(DismissCritter dismiss)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_DISMISS_CRITTER);
        packet.WriteGuid(dismiss.CritterGUID.To64(GetSession().GameState));
        SendPacketToServer(packet);
        SetSummonedBattlePet(WowGuid128.Empty, 0);
    }
}
