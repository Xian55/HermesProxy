using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World.Dispatch;

namespace HermesProxy.World.Server.Packets;

// Query CMSG codecs. Bytes in, packet out, nothing else.
//
// Every field is read into a local before the struct is constructed. The reader is stateful, so
// relying on constructor-argument evaluation order would make wire order an implementation detail
// of the compiler; and several of these packets declare their fields in a different order than
// they read them, which makes that failure easy to introduce and invisible once made.

public static class ItemTextQueryCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ItemTextQuery packet)
        => packet = new ItemTextQuery(r.ReadPackedGuid128());
}

public static class QueryQuestInfoCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out QueryQuestInfo packet)
    {
        uint questId = r.ReadUInt32();
        WowGuid128 questGiver = r.ReadPackedGuid128();
        packet = new QueryQuestInfo(questId, questGiver);
    }
}

public static class QueryCreatureCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out QueryCreature packet)
        => packet = new QueryCreature(r.ReadUInt32());
}

public static class QueryGameObjectCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out QueryGameObject packet)
    {
        uint gameObjectId = r.ReadUInt32();
        WowGuid128 guid = r.ReadPackedGuid128();
        packet = new QueryGameObject(gameObjectId, guid);
    }
}

public static class QueryPageTextCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out QueryPageText packet)
    {
        uint pageTextId = r.ReadUInt32();
        WowGuid128 itemGuid = r.ReadPackedGuid128();
        packet = new QueryPageText(pageTextId, itemGuid);
    }
}

public static class QueryNPCTextCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out QueryNPCText packet)
    {
        uint textId = r.ReadUInt32();
        WowGuid128 guid = r.ReadPackedGuid128();
        packet = new QueryNPCText(textId, guid);
    }
}

public static class QueryPetNameCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out QueryPetName packet)
        => packet = new QueryPetName(r.ReadPackedGuid128());
}
