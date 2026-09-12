using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's query CMSGs. Behaviour only.
/// </summary>
public static class QuerySystem
{
    // Opening a letter created from a mail (CMSG_MAIL_CREATE_TEXT_ITEM) makes the client
    // query the item's body text. The opcode was mapped on both sides and the response was
    // already handled, but nothing forwarded the request, so the letter never opened.
    // Legacy took a uint32 item_text id before 3.3.0 and an item GUID from 3.3.0 on; the
    // modern client only ever sends a GUID, so this bridge is 3.3.0+ only. On older cores
    // the body still arrives through the mail-list path in MailHandler.
    [HandlesCmsg(Opcode.CMSG_ITEM_TEXT_QUERY)]
    public static void HandleItemTextQuery(in ItemTextQuery query, in SessionContext ctx)
    {
        if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_ITEM_TEXT_QUERY);
        packet.WriteGuid(query.Id.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_QUERY_QUEST_INFO)]
    public static void HandleQueryQuestInfo(in QueryQuestInfo queryQuest, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_QUEST_INFO);
        packet.WriteUInt32(queryQuest.QuestID);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_QUERY_CREATURE)]
    public static void HandleQueryCreature(in QueryCreature queryCreature, in SessionContext ctx)
    {
        // GameData stays static: it is load-once reference data, and a lookup here is a
        // dictionary hit either way.
        bool cached = GameData.GetCreatureTemplate(queryCreature.CreatureID) != null;
        Log.Print(LogType.Trace,
            $"[CreatureQueryTrace][req] entry={queryCreature.CreatureID} cached={cached}");

        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_CREATURE);
        packet.WriteUInt32(queryCreature.CreatureID);
        packet.WriteGuid(new WowGuid64(HighGuidTypeLegacy.Creature, queryCreature.CreatureID, 1));
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_QUERY_GAME_OBJECT)]
    public static void HandleQueryGameObject(in QueryGameObject queryGo, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_GAME_OBJECT);
        packet.WriteUInt32(queryGo.GameObjectID);
        packet.WriteGuid(queryGo.Guid.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_QUERY_PAGE_TEXT)]
    public static void HandleQueryPageText(in QueryPageText queryText, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_PAGE_TEXT);
        packet.WriteUInt32(queryText.PageTextID);
        packet.WriteGuid(queryText.ItemGUID.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_QUERY_NPC_TEXT)]
    public static void HandleQueryNpcText(in QueryNPCText queryText, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_NPC_TEXT);
        packet.WriteUInt32(queryText.TextID);
        packet.WriteGuid(queryText.Guid.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_QUERY_PET_NAME)]
    public static void HandleQueryPetName(in QueryPetName queryName, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_PET_NAME);
        // The legacy CMSG body wants pet_number (per-character spawn counter), which on
        // cMaNGOS-style backends is the legacy GUID's entry slot. The modern Pet GUID's
        // entry slot now carries creature_template.entry post-fix, so we reverse-resolve.
        var legacy = ctx.GetSession().GameState.GetLegacyPetGuid(queryName.UnitGUID);
        uint petNumber = legacy?.GetEntry() ?? queryName.UnitGUID.GetEntry();
        packet.WriteUInt32(petNumber);
        packet.WriteGuid(legacy ?? queryName.UnitGUID.To64(ctx.GetSession().GameState));
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_QUERY_TIME)]
    public static void HandleQueryTime(in EmptyClientPacket queryTime, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_TIME);
        ctx.SendPacketToServer(packet);
    }
}
