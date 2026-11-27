using Framework.IO;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects;

public class HotfixRecord
{
    public uint HotfixId;
    public uint UniqueId;
    public DB2Hash TableHash;
    public uint RecordId;
    public HotfixStatus Status;
    public ByteBuffer HotfixContent = new();

    // SMSG_AVAILABLE_HOTFIXES per-record layout (V2_5+ / V3_4_3): (int32 PushID, uint32 UniqueID).
    // UniqueID is the V3_4_3 client's cache validator — when it matches the value the client
    // has stored in Cache/WDB/HotfixCache.bin for this PushID, the client trusts its cached
    // body and skips re-requesting via CMSG_HOTFIX_REQUEST. Previously we shipped TableHash
    // here, which the client read as "version mismatch" and re-downloaded the full ~82 KB
    // hotfix payload every login. A record loaded from CSV gets a UniqueId made from its
    // content as loaded (GameData.AssignContentUniqueIds), the same on every proxy start, so
    // warm-cache logins validate and a push id that comes to name other content is asked for
    // again. Records made at runtime keep UniqueId equal to HotfixId.
    // UniqueId is set once and does not follow later changes: when GameData.UpdateHotfix
    // rewrites a record's content after a server's item query (an ItemSparse row loaded from
    // CSV on 1.14, or a record made at runtime), the record keeps the UniqueId it had.
    public void WriteAvailable(WorldPacket data)
    {
        data.WriteUInt32(HotfixId);
        data.WriteUInt32(UniqueId);
    }
    public void WriteHotFixMessageContent(WorldPacket data)
    {
        data.WriteUInt32(HotfixId);
        data.WriteUInt32(UniqueId);
        data.WriteUInt32((uint)TableHash);
        data.WriteUInt32(RecordId);
        data.WriteUInt32(HotfixContent.GetSize());
        data.WriteBits((byte)Status, 3);
        data.FlushBits();
    }
}
