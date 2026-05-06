using Framework.Constants;
using Framework.Logging;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server;

public partial class WorldSocket
{
    // Handlers for CMSG opcodes coming from the modern client
    [PacketHandler(Opcode.CMSG_QUERY_TIME)]
    void HandleQueryTime(EmptyClientPacket queryTime)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_TIME);
        SendPacketToServer(packet);
    }
    [PacketHandler(Opcode.CMSG_QUERY_QUEST_INFO)]
    void HandleQueryQuestInfo(QueryQuestInfo queryQuest)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_QUEST_INFO);
        packet.WriteUInt32(queryQuest.QuestID);
        SendPacketToServer(packet);
    }
    [PacketHandler(Opcode.CMSG_QUERY_CREATURE)]
    void HandleQueryCreature(QueryCreature queryCreature)
    {
        bool cached = GameData.GetCreatureTemplate(queryCreature.CreatureID) != null;
        Log.Print(LogType.Trace,
            $"[CreatureQueryTrace][req] entry={queryCreature.CreatureID} cached={cached}");

        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_CREATURE);
        packet.WriteUInt32(queryCreature.CreatureID);
        packet.WriteGuid(new WowGuid64(HighGuidTypeLegacy.Creature, queryCreature.CreatureID, 1));
        SendPacketToServer(packet);
    }
    [PacketHandler(Opcode.CMSG_QUERY_GAME_OBJECT)]
    void HandleQueryGameObject(QueryGameObject queryGo)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_GAME_OBJECT);
        packet.WriteUInt32(queryGo.GameObjectID);
        packet.WriteGuid(queryGo.Guid.To64());
        SendPacketToServer(packet);
    }
    [PacketHandler(Opcode.CMSG_QUERY_PAGE_TEXT)]
    void HandleQueryPageText(QueryPageText queryText)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_PAGE_TEXT);
        packet.WriteUInt32(queryText.PageTextID);
        packet.WriteGuid(queryText.ItemGUID.To64());
        SendPacketToServer(packet);
    }
    [PacketHandler(Opcode.CMSG_QUERY_NPC_TEXT)]
    void HandleQueryNpcText(QueryNPCText queryText)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_NPC_TEXT);
        packet.WriteUInt32(queryText.TextID);
        packet.WriteGuid(queryText.Guid.To64());
        SendPacketToServer(packet);
    }
    [PacketHandler(Opcode.CMSG_QUERY_PET_NAME)]
    void HandleQueryPetName(QueryPetName queryName)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_PET_NAME);
        // The legacy CMSG body wants pet_number (per-character spawn counter), which on
        // cMaNGOS-style backends is the legacy GUID's entry slot. The modern Pet GUID's
        // entry slot now carries creature_template.entry post-fix, so we reverse-resolve.
        var legacy = GetSession().GameState.GetLegacyPetGuid(queryName.UnitGUID);
        uint petNumber = legacy?.GetEntry() ?? queryName.UnitGUID.GetEntry();
        packet.WriteUInt32(petNumber);
        packet.WriteGuid(legacy ?? queryName.UnitGUID.To64(GetSession().GameState));
        SendPacketToServer(packet);
    }
    [PacketHandler(Opcode.CMSG_WHO)]
    void HandleWhoRequest(WhoRequestPkt who)
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

        SendPacketToServer(packet);
        GetSession().GameState.LastWhoRequestId = who.RequestID;
    }
}
