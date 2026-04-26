using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

using static HermesProxy.World.GameData;

namespace HermesProxy.World.Server;

public partial class WorldSocket
{
    // Handlers for CMSG opcodes coming from the modern client
    [PacketHandler(Opcode.CMSG_DB_QUERY_BULK)]
    void HandleDbQueryBulk(DBQueryBulk query)
    {
        foreach (uint id in query.Queries)
        {
            DBReply reply = new();
            reply.RecordID = id;
            reply.TableHash = query.TableHash;
            reply.Status = HotfixStatus.Invalid;
            reply.Timestamp = (uint)Time.UnixTime;

            Log.PrintNet(LogType.Debug, LogNetDir.C2P, $"DB_QUERY_BULK requested ({query.TableHash}) #{id}");

            if (query.TableHash == DB2Hash.BroadcastText)
            {
                BroadcastText? bct = GameData.GetBroadcastText(id);
                if (bct == null)
                {
                    bct = new BroadcastText();
                    bct.Entry = id;
                    bct.MaleText = "Clear your cache!";
                    bct.FemaleText = "Clear your cache!";
                }

                //Log.PrintNet(LogType.Debug, LogNetDir.P2C, $"Sending broadcast text #{id}");
                reply.Status = HotfixStatus.Valid;
                reply.Data.WriteCString(bct.MaleText);
                reply.Data.WriteCString(bct.FemaleText);
                reply.Data.WriteUInt32(bct.Entry);
                reply.Data.WriteUInt32(bct.Language);
                reply.Data.WriteUInt32(0); // ConditionId
                reply.Data.WriteUInt16(0); // EmotesId
                reply.Data.WriteUInt8(0); // Flags
                reply.Data.WriteUInt32(0); // ChatBubbleDurationMs
                if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
                    reply.Data.WriteUInt32(0); // VoiceOverPriorityID
                for (int i = 0; i < 2; ++i)
                    reply.Data.WriteUInt32(0); // SoundEntriesID
                for (int i = 0; i < 3; ++i)
                    reply.Data.WriteUInt16(bct.Emotes[i]);
                for (int i = 0; i < 3; ++i)
                    reply.Data.WriteUInt16(bct.EmoteDelays[i]);
            }
            else if (query.TableHash == DB2Hash.Item)
            {
                ItemTemplate? item = GameData.GetItemTemplate(id);
                if (item != null)
                {
                    //Log.PrintNet(LogType.Debug, LogNetDir.P2C, $"Sending custom ({DB2Hash.Item}) #{id}");
                    reply.Status = HotfixStatus.Valid;
                    GameData.WriteItemHotfix(item, reply.Data);
                }
                else if (!GetSession().GameState.RequestedItemHotfixes.Contains(id) &&
                          GetSession().WorldClient != null && GetSession().WorldClient!.IsConnected())
                {
                    //Log.PrintNet(LogType.Storage, LogNetDir.P2S, $"Item #{id} not cached, requesting server data...");
                    GetSession().GameState.RequestedItemHotfixes.Add(id);
                    WorldPacket packet2 = new WorldPacket(Opcode.CMSG_ITEM_QUERY_SINGLE);
                    packet2.WriteUInt32(id);
                    if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                        packet2.WriteGuid(WowGuid64.Empty);
                    SendPacketToServer(packet2);
                    continue;
                }
            }
            else if (query.TableHash == DB2Hash.ItemSparse)
            {
                ItemTemplate? item = GameData.GetItemTemplate(id);
                if (item != null)
                {
                    //Log.PrintNet(LogType.Debug, LogNetDir.P2C, $"Sending custom ({DB2Hash.ItemSparse}) #{id}");
                    reply.Status = HotfixStatus.Valid;
                    GameData.WriteItemSparseHotfix(item, reply.Data);
                }
                else if (!GetSession().GameState.RequestedItemSparseHotfixes.Contains(id) &&
                          GetSession().WorldClient != null && GetSession().WorldClient!.IsConnected())
                {
                    GetSession().GameState.RequestedItemSparseHotfixes.Add(id);
                    //Log.PrintNet(LogType.Storage, LogNetDir.P2S, $"ItemSparse #{id} not cached, requesting server data...");
                    WorldPacket packet2 = new WorldPacket(Opcode.CMSG_ITEM_QUERY_SINGLE);
                    packet2.WriteUInt32(id);
                    if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                        packet2.WriteGuid(WowGuid64.Empty);
                    SendPacketToServer(packet2);
                    continue;
                }
            }

            SendPacket(reply);
        }
    }

    [PacketHandler(Opcode.CMSG_HOTFIX_REQUEST)]
    void HandleHotfixRequest(HotfixRequest request)
    {
        Log.Print(LogType.Network,
            $"[Hotfix] CMSG_HOTFIX_REQUEST: client requested {request.Hotfixes.Count} hotfix IDs " +
            $"(GameData.Hotfixes total available={GameData.Hotfixes.Count})");

        HotfixConnect connect = new HotfixConnect();

        // FIXME(phase5a-7b): empty hotfix response for V3_4_3. Our HotfixContent rows are
        // for an older build; the V3_4_3 client receives them but can't deserialize against
        // its DB2 schemas, then sits waiting — character-select never renders. Empty response
        // tells the client "no hotfixes for these IDs", which it accepts and proceeds. Real
        // fix: import wago.tools' 3.4.3.54261 hotfix dataset (tracked as Phase 5a-7b).
        if (ClientVersionBuild.V3_4_3_54261 == ModernVersion.Build)
        {
            Log.Print(LogType.Network,
                $"[Hotfix] V3_4_3 diagnostic: sending EMPTY SMSG_HOTFIX_CONNECT (count=0) to " +
                $"unblock character-select. If this works, hotfixes were the blocker.");
            SendPacket(connect);
            return;
        }

        int matched = 0;
        foreach (uint id in request.Hotfixes)
        {
            HotfixRecord? record;
            if (GameData.Hotfixes.TryGetValue(id, out record))
            {
                connect.Hotfixes.Add(record);
                matched++;
            }
        }
        Log.Print(LogType.Network,
            $"[Hotfix] Sending SMSG_HOTFIX_CONNECT: matched={matched}/{request.Hotfixes.Count}");
        SendPacket(connect);
    }
}
