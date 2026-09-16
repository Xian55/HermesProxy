using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Outbox;
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
    // modern client only ever sends a GUID, so older cores need the id looked up first.
    [HandlesCmsg(Opcode.CMSG_ITEM_TEXT_QUERY)]
    public static void HandleItemTextQuery(in ItemTextQuery query, in SessionContext ctx)
    {
        if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
        {
            HandleLegacyItemTextQuery(query.Id, ctx);
            return;
        }

        WorldPacket packet = new WorldPacket(Opcode.CMSG_ITEM_TEXT_QUERY);
        packet.WriteGuid(query.Id.To64());
        ctx.SendPacketToServer(packet);
    }

    // The id lives on the item as ITEM_FIELD_ITEM_TEXT_ID, which UpdateHandler remembers against
    // the GUID. A mail list already pulls the bodies of its own letters, so the text is often
    // cached before the copy is ever right-clicked.
    private static void HandleLegacyItemTextQuery(WowGuid128 itemGuid, in SessionContext ctx)
    {
        var gameState = ctx.GameState;
        if (!gameState.ItemTextIds.TryGetValue(itemGuid, out uint itemTextId))
        {
            // Answering anyway: the client keeps the reading frame open until it hears back.
            ctx.SendPacketToClient(new QueryItemTextResponse { Id = itemGuid });
            return;
        }

        if (gameState.ItemTexts.TryGetValue(itemTextId, out string? cachedText))
        {
            SendItemText(itemGuid, cachedText, ctx.ToClient);
            return;
        }

        if (!gameState.PendingItemTextQueries.TryGetValue(itemTextId, out var waiting))
            gameState.PendingItemTextQueries[itemTextId] = waiting = [];
        if (!waiting.Contains(itemGuid))
            waiting.Add(itemGuid);

        WorldPacket packet = new WorldPacket(Opcode.CMSG_ITEM_TEXT_QUERY);
        packet.WriteUInt32(itemTextId);
        packet.WriteInt32(0); // mail id, unused by the server
        packet.WriteUInt32(0); // unk
        ctx.SendPacketToServer(packet);
    }

    // Legacy answers an unknown id with an empty string, which the modern Valid bit spells out.
    internal static void SendItemText(WowGuid128 itemGuid, string text, ClientOutbox toClient)
    {
        toClient.Send(new QueryItemTextResponse
        {
            Id = itemGuid,
            Valid = text.Length != 0,
            Text = text,
        });
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
        // The response comes back carrying this number and nothing else, so record what it stands
        // for. Without it a response that lands before the pet's create is unroutable (issue #299).
        ctx.GetSession().GameState.RegisterPetNameQuery(petNumber, queryName.UnitGUID);
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

    [HandlesCmsg(Opcode.CMSG_WHO)]
    public static void HandleWhoRequest(in WhoRequestPkt who, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_WHO);
        packet.WriteInt32(who.Request.MinLevel);
        packet.WriteInt32(who.Request.MaxLevel);
        packet.WriteCString(who.Request.Name);
        packet.WriteCString(who.Request.Guild);
        packet.WriteInt32((int)who.Request.RaceFilter);
        packet.WriteInt32(who.Request.ClassFilter);

        packet.WriteInt32(who.Areas.Count);
        foreach (int area in who.Areas)
            packet.WriteInt32(area);

        packet.WriteInt32(who.Request.Words.Count);
        foreach (string word in who.Request.Words)
            packet.WriteCString(word);

        ctx.SendPacketToServer(packet);
        ctx.GetSession().GameState.LastWhoRequestId = who.RequestID;
    }
}
