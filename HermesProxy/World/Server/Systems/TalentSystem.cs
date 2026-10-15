using Framework.Logging;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>Pet talents and glyph removal.</summary>
public static class TalentSystem
{
    // Modern V3_4_3 CMSG_PET_LEARN_TALENT (0x3554 / 13652) → legacy CMSG_PET_LEARN_TALENT (0x47A).
    // Legacy payload (CMaNGOS PetHandler.cpp:845-855 HandlePetLearnTalent):
    //   ObjectGuid guid; uint32 talent_id; uint32 requested_rank;
    // Modern client sends a separate opcode from CMSG_LEARN_TALENT (which is player-only).
    // Pet GUID is translated modern→legacy via existing GameSessionData.GetLegacyPetGuid
    // (project_pet_guid_fix infrastructure).
    [HandlesCmsg(Opcode.CMSG_PET_LEARN_TALENT)]
    public static void HandleLearnPetTalent(in LearnPetTalent talent, in SessionContext ctx)
    {
        var legacyGuid = ctx.GetSession().GameState.GetLegacyPetGuid(talent.PetGUID);
        if (legacyGuid == null)
        {
            Log.Print(LogType.Warn, $"CMSG_PET_LEARN_TALENT: no legacy pet GUID for {talent.PetGUID} — dropping");
            return;
        }

        WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_LEARN_TALENT);
        packet.WriteGuid(legacyGuid.Value);
        packet.WriteUInt32(talent.TalentID);
        packet.WriteUInt32(talent.Rank);
        ctx.SendPacketToServer(packet);
    }

    // The Cataclysm talent window's Learn. A 4.3.4 server takes the same layout (TrinityCore
    // 4.3.4 HandleLearnPreviewTalents), the tab included; a 3.3.5a one has no specialization and
    // takes the count and the pairs alone.
    [HandlesCmsg(Opcode.CMSG_LEARN_PREVIEW_TALENTS)]
    public static void HandleLearnPreviewTalents(in LearnPreviewTalents learn, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_LEARN_PREVIEW_TALENTS);
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
            packet.WriteInt32(learn.TalentTab);
        packet.WriteUInt32((uint)learn.Talents.Count);
        foreach (TalentEntry talent in learn.Talents)
        {
            packet.WriteUInt32(talent.TalentID);
            packet.WriteUInt32(talent.Rank);
        }
        ctx.SendPacketToServer(packet);
    }

    // Choosing a specialization before the first point. TrinityCore 4.3.4 leaves
    // CMSG_SET_PRIMARY_TALENT_TREE unhandled and takes the choice from a talent preview's tab, so
    // it goes out as a preview with no talents. A 3.3.5a server has no specializations.
    [HandlesCmsg(Opcode.CMSG_SET_PRIMARY_TALENT_TREE)]
    public static void HandleSetPrimaryTalentTree(in SetPrimaryTalentTree tree, in SessionContext ctx)
    {
        if (!HermesProxy.World.Client.WorldClient.IsCataLegacy)
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_LEARN_PREVIEW_TALENTS);
        packet.WriteInt32((int)tree.TalentTab);
        packet.WriteUInt32(0);
        ctx.SendPacketToServer(packet);
    }

    // Modern V3_4_3 CMSG_REMOVE_GLYPH (0x32E0 / 13056) → legacy CMSG_REMOVE_GLYPH (0x48A).
    // Both share the same payload: uint8 GlyphSlot (0-5). The legacy server replies with
    // a fresh SMSG_UPDATE_TALENT_DATA, which TalentHandler.HandleTalentsInfoUpdate processes
    // and re-emits SMSG_ACTIVE_GLYPHS — the slot will appear empty in the UI on next refresh.
    [HandlesCmsg(Opcode.CMSG_REMOVE_GLYPH)]
    public static void HandleRemoveGlyph(in RemoveGlyph remove, in SessionContext ctx)
    {
        Log.Print(LogType.Network, $"CMSG_REMOVE_GLYPH: slot={remove.GlyphSlot} → forwarding to legacy");
        WorldPacket packet = new WorldPacket(Opcode.CMSG_REMOVE_GLYPH);
        packet.WriteUInt8(remove.GlyphSlot);
        ctx.SendPacketToServer(packet);
    }
}
