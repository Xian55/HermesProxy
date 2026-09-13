using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

using static HermesProxy.World.GameData;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// DB2 hotfix queries: the client asking for record data the proxy serves from its own tables.
/// </summary>
/// <remarks>
/// Neither of these translates a legacy packet. The bulk query answers per record from GameData,
/// and falls back to asking the legacy server for item data it has not cached yet - which is why
/// it can forward CMSG_ITEM_QUERY_SINGLE mid-loop and skip that record's reply until the answer
/// arrives.
/// </remarks>
public static class HotfixSystem
{
    [HandlesCmsg(Opcode.CMSG_DB_QUERY_BULK)]
    public static void HandleDbQueryBulk(in DBQueryBulk query, in SessionContext ctx)
    {
        foreach (uint id in query.Queries)
        {
            DBReply reply = new();
            reply.RecordID = id;
            reply.TableHash = query.TableHash;
            reply.Status = HotfixStatus.Invalid;
            reply.Timestamp = (uint)Time.UnixTime;

            Log.PrintNet(LogType.Debug, LogNetDir.C2P, $"DB_QUERY_BULK requested ({query.TableHash}) #{id}");

            // TactKey is the per-record encryption key for protected DB2 content
            // (race/class/customization tables can be encrypted; client uses TactKey
            // to decrypt). cMangos doesn't have these keys; sending Status=Invalid
            // (the default) made the V3_4_3 client log every TactKey reply as
            // VALIDATION_RESULT_INVALID and silently refuse to render any characters
            // that depend on encrypted records — observed in the WoW client's own
            // Hotfix.log. NotPublic tells the client "this key exists server-side
            // but isn't exposed to your account" so the client falls back to its
            // baseline / unencrypted DB2 records.
            if (query.TableHash == DB2Hash.TactKey)
            {
                reply.Status = HotfixStatus.NotPublic;
                ctx.SendPacket(reply);
                continue;
            }

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
                else if (!ctx.GetSession().GameState.RequestedItemHotfixes.Contains(id) &&
                          ctx.GetSession().WorldClient != null && ctx.GetSession().WorldClient!.IsConnected())
                {
                    //Log.PrintNet(LogType.Storage, LogNetDir.P2S, $"Item #{id} not cached, requesting server data...");
                    ctx.GetSession().GameState.RequestedItemHotfixes.Add(id);
                    WorldPacket packet2 = new WorldPacket(Opcode.CMSG_ITEM_QUERY_SINGLE);
                    packet2.WriteUInt32(id);
                    if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                        packet2.WriteGuid(WowGuid64.Empty);
                    ctx.SendPacketToServer(packet2);
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
                else if (!ctx.GetSession().GameState.RequestedItemSparseHotfixes.Contains(id) &&
                          ctx.GetSession().WorldClient != null && ctx.GetSession().WorldClient!.IsConnected())
                {
                    ctx.GetSession().GameState.RequestedItemSparseHotfixes.Add(id);
                    //Log.PrintNet(LogType.Storage, LogNetDir.P2S, $"ItemSparse #{id} not cached, requesting server data...");
                    WorldPacket packet2 = new WorldPacket(Opcode.CMSG_ITEM_QUERY_SINGLE);
                    packet2.WriteUInt32(id);
                    if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                        packet2.WriteGuid(WowGuid64.Empty);
                    ctx.SendPacketToServer(packet2);
                    continue;
                }
            }

            ctx.SendPacket(reply);
        }
    }

    [HandlesCmsg(Opcode.CMSG_HOTFIX_REQUEST)]
    public static void HandleHotfixRequest(in HotfixRequest request, in SessionContext ctx)
    {
        Log.Print(LogType.Network,
            $"[Hotfix] CMSG_HOTFIX_REQUEST: client requested {request.Hotfixes.Count} hotfix IDs " +
            $"(GameData.Hotfixes total available={GameData.Hotfixes.Count})");

        HotfixConnect connect = new HotfixConnect();

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
        ctx.SendPacket(connect);
    }
}
