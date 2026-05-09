using Framework.Logging;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server;

public partial class WorldSocket
{
    // Modern V3_4_3 CMSG_PET_LEARN_TALENT (0x3554 / 13652) → legacy CMSG_PET_LEARN_TALENT (0x47A).
    // Legacy payload (CMaNGOS PetHandler.cpp:845-855 HandlePetLearnTalent):
    //   ObjectGuid guid; uint32 talent_id; uint32 requested_rank;
    // Modern client sends a separate opcode from CMSG_LEARN_TALENT (which is player-only).
    // Pet GUID is translated modern→legacy via existing GameSessionData.GetLegacyPetGuid
    // (project_pet_guid_fix infrastructure).
    [PacketHandler(Opcode.CMSG_PET_LEARN_TALENT)]
    void HandleLearnPetTalent(LearnPetTalent talent)
    {
        var legacyGuid = GetSession().GameState.GetLegacyPetGuid(talent.PetGUID);
        if (legacyGuid == null)
        {
            Log.Print(LogType.Warn, $"CMSG_PET_LEARN_TALENT: no legacy pet GUID for {talent.PetGUID} — dropping");
            return;
        }

        WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_LEARN_TALENT);
        packet.WriteGuid(legacyGuid.Value);
        packet.WriteUInt32(talent.TalentID);
        packet.WriteUInt32(talent.Rank);
        SendPacketToServer(packet);
    }

    // Modern V3_4_3 CMSG_REMOVE_GLYPH (0x32E0 / 13056) → legacy CMSG_REMOVE_GLYPH (0x48A).
    // Both share the same payload: uint8 GlyphSlot (0-5). The legacy server replies with
    // a fresh SMSG_UPDATE_TALENT_DATA, which TalentHandler.HandleTalentsInfoUpdate processes
    // and re-emits SMSG_ACTIVE_GLYPHS — the slot will appear empty in the UI on next refresh.
    [PacketHandler(Opcode.CMSG_REMOVE_GLYPH)]
    void HandleRemoveGlyph(RemoveGlyph remove)
    {
        Log.Print(LogType.Network, $"CMSG_REMOVE_GLYPH: slot={remove.GlyphSlot} → forwarding to legacy");
        WorldPacket packet = new WorldPacket(Opcode.CMSG_REMOVE_GLYPH);
        packet.WriteUInt8(remove.GlyphSlot);
        SendPacketToServer(packet);
    }
}
