using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

// Social, reputation, duel and toy CMSG codecs.
//
// Nothing here is a ranged pair. AddIgnore is the one with a version branch, and it is an
// AddedInVersion over three separate client lines rather than a single build boundary, which
// AddedIn/RemovedIn cannot express — the same case PlayerLoginCodec documents. The predicate is
// static readonly either way, so the JIT folds it.

public static class ContactListRequestCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ContactListRequest packet)
        => packet = new ContactListRequest((SocialFlag)r.ReadUInt32());
}

public static class AddFriendCodec
{
    /// <remarks>
    /// Both lengths are read before either string: they share one bit block, so taking the name
    /// between them would read the note length out of the string bytes.
    /// </remarks>
    public static void Read(ref SpanPacketReader r, out AddFriend packet)
    {
        uint nameLength = r.ReadBits<uint>(9);
        uint notesLength = r.ReadBits<uint>(10);
        string name = r.ReadString(nameLength);
        packet = new AddFriend(name, r.ReadString(notesLength));
    }
}

public static class AddIgnoreCodec
{
    /// <remarks>
    /// The name length comes first, then an optional account GUID, then the name itself — so the
    /// GUID sits between a length and its string on the builds that send it.
    /// </remarks>
    public static void Read(ref SpanPacketReader r, out AddIgnore packet)
    {
        uint nameLength = r.ReadBits<uint>(9);
        WowGuid128 accountGuid = default;
        if (ModernVersion.AddedInVersion(9, 1, 5, 1, 14, 1, 2, 5, 3))
            accountGuid = r.ReadPackedGuid128();
        packet = new AddIgnore(r.ReadString(nameLength), accountGuid);
    }
}

public static class DelFriendCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out DelFriend packet)
    {
        uint virtualRealmAddress = r.ReadUInt32();
        packet = new DelFriend(virtualRealmAddress, r.ReadPackedGuid128());
    }
}

public static class SetContactNotesCodec
{
    public static void Read(ref SpanPacketReader r, out SetContactNotes packet)
    {
        uint virtualRealmAddress = r.ReadUInt32();
        WowGuid128 guid = r.ReadPackedGuid128();
        packet = new SetContactNotes(virtualRealmAddress, guid, r.ReadString(r.ReadBits<uint>(10)));
    }
}

// ---- reputation ----

public static class SetFactionAtWarCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SetFactionAtWar packet)
        => packet = new SetFactionAtWar(r.ReadUInt8());
}

public static class SetFactionNotAtWarCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SetFactionNotAtWar packet)
        => packet = new SetFactionNotAtWar(r.ReadUInt8());
}

public static class SetFactionInactiveCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SetFactionInactive packet)
    {
        uint factionIndex = r.ReadUInt32();
        packet = new SetFactionInactive(factionIndex, r.HasBit());
    }
}

public static class SetWatchedFactionCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SetWatchedFaction packet)
        => packet = new SetWatchedFaction(r.ReadUInt32());
}

// ---- duel ----

public static class CanDuelCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out CanDuel packet)
        => packet = new CanDuel(r.ReadPackedGuid128());
}

public static class DuelResponseCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out DuelResponse packet)
    {
        WowGuid128 arbiterGuid = r.ReadPackedGuid128();
        bool accepted = r.HasBit();
        packet = new DuelResponse(arbiterGuid, accepted, r.HasBit());
    }
}

// ---- toys and the collection ----

public static class ToyClearFanfareCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ToyClearFanfare packet)
        => packet = new ToyClearFanfare(r.ReadUInt32());
}

public static class AddToyCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out AddToy packet)
        => packet = new AddToy(r.ReadPackedGuid128());
}

public static class UseToyCodec
{
    public static void Read(ref SpanPacketReader r, out UseToy packet)
    {
        var cast = new SpellCastRequest();
        cast.Read(ref r);
        packet = new UseToy(cast);
    }
}

public static class CollectionItemSetFavoriteCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out CollectionItemSetFavorite packet)
    {
        // Wrathion CollectionPackets.cpp: CollectionType is int32, not uint8
        var type = (ItemCollectionType)r.ReadInt32();
        uint id = r.ReadUInt32();
        packet = new CollectionItemSetFavorite(type, id, r.ReadBit());
    }
}
