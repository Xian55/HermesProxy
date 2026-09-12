using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;

namespace HermesProxy.World.Server.Packets;

// Mail and petition CMSG codecs.
//
// Six mail packets widened their mail id from uint32 to int64 in V3_4_3, which the old readers
// expressed as `ModernVersion.Build == V3_4_3_54261 ? ReadInt64() : ReadUInt32()` inside the
// reader. Those become ranged pairs here, for the two reasons ChatCodecs.cs states: the branch
// leaves the per-packet path entirely (which codec runs is decided once, when the table is built),
// and exact equality against V3_4_3 silently drops every future build into the 32-bit branch.
// `AddedIn = V3_4_3_54261` is a build boundary rather than an expansion test, so it is the form
// ranged codecs can actually express — see PlayerLoginCodec for the case that cannot.
//
// The pairs agree over every modern build supported today, so this is a refactor now and a
// correctness fix the moment a Cataclysm Classic client is added (issue #202).

// ---- mail: guid-only ----

public static class MailGetListCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out MailGetList packet)
        => packet = new MailGetList(r.ReadPackedGuid128());
}

// ---- mail: mailbox + id ----

[PacketCodec(typeof(MailCreateTextItem), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class MailCreateTextItemCodecWotLKClassic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out MailCreateTextItem packet)
    {
        WowGuid128 mailbox = r.ReadPackedGuid128();
        packet = new MailCreateTextItem(mailbox, r.ReadInt64());
    }
}

[PacketCodec(typeof(MailCreateTextItem), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class MailCreateTextItemCodecPreWotLKClassic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out MailCreateTextItem packet)
    {
        WowGuid128 mailbox = r.ReadPackedGuid128();
        packet = new MailCreateTextItem(mailbox, r.ReadUInt32());
    }
}

[PacketCodec(typeof(MailMarkAsRead), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class MailMarkAsReadCodecWotLKClassic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out MailMarkAsRead packet)
    {
        WowGuid128 mailbox = r.ReadPackedGuid128();
        packet = new MailMarkAsRead(mailbox, r.ReadInt64());
    }
}

[PacketCodec(typeof(MailMarkAsRead), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class MailMarkAsReadCodecPreWotLKClassic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out MailMarkAsRead packet)
    {
        WowGuid128 mailbox = r.ReadPackedGuid128();
        packet = new MailMarkAsRead(mailbox, r.ReadUInt32());
    }
}

[PacketCodec(typeof(MailTakeMoney), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class MailTakeMoneyCodecWotLKClassic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out MailTakeMoney packet)
    {
        WowGuid128 mailbox = r.ReadPackedGuid128();
        long mailId = r.ReadInt64();
        packet = new MailTakeMoney(mailbox, mailId, r.ReadInt64());
    }
}

[PacketCodec(typeof(MailTakeMoney), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class MailTakeMoneyCodecPreWotLKClassic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out MailTakeMoney packet)
    {
        WowGuid128 mailbox = r.ReadPackedGuid128();
        long mailId = r.ReadUInt32();
        packet = new MailTakeMoney(mailbox, mailId, r.ReadInt64());
    }
}

[PacketCodec(typeof(MailTakeItem), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class MailTakeItemCodecWotLKClassic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out MailTakeItem packet)
    {
        WowGuid128 mailbox = r.ReadPackedGuid128();
        long mailId = r.ReadInt64();
        packet = new MailTakeItem(mailbox, mailId, r.ReadInt64());
    }
}

[PacketCodec(typeof(MailTakeItem), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class MailTakeItemCodecPreWotLKClassic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out MailTakeItem packet)
    {
        WowGuid128 mailbox = r.ReadPackedGuid128();
        long mailId = r.ReadUInt32();
        packet = new MailTakeItem(mailbox, mailId, r.ReadUInt32());
    }
}

// ---- mail: id first ----

[PacketCodec(typeof(MailDelete), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class MailDeleteCodecWotLKClassic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out MailDelete packet)
    {
        long mailId = r.ReadInt64();
        packet = new MailDelete(mailId, r.ReadInt32());
    }
}

[PacketCodec(typeof(MailDelete), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class MailDeleteCodecPreWotLKClassic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out MailDelete packet)
    {
        long mailId = r.ReadUInt32();
        packet = new MailDelete(mailId, r.ReadInt32());
    }
}

[PacketCodec(typeof(MailReturnToSender), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class MailReturnToSenderCodecWotLKClassic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out MailReturnToSender packet)
    {
        long mailId = r.ReadInt64();
        packet = new MailReturnToSender(mailId, r.ReadPackedGuid128());
    }
}

[PacketCodec(typeof(MailReturnToSender), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class MailReturnToSenderCodecPreWotLKClassic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out MailReturnToSender packet)
    {
        long mailId = r.ReadUInt32();
        packet = new MailReturnToSender(mailId, r.ReadPackedGuid128());
    }
}

// ---- mail: the composed one ----

public static class SendMailCodec
{
    /// <remarks>
    /// Three lengths and the attachment count are read as a bit block up front, then the strings
    /// follow as bytes — so every length must be taken before the first string, not interleaved.
    /// The count is five bits, which caps a mail at 31 attachments no matter what the packet
    /// claims, so this list pre-sizes on it directly rather than through
    /// <c>CodecHelpers.WireCountCapacity</c>.
    /// </remarks>
    public static void Read(ref SpanPacketReader r, out SendMail packet)
    {
        WowGuid128 mailbox = r.ReadPackedGuid128();
        int stationeryId = r.ReadInt32();
        long sendMoney = r.ReadInt64();
        long cod = r.ReadInt64();

        uint targetLength = r.ReadBits<uint>(9);
        uint subjectLength = r.ReadBits<uint>(9);
        uint bodyLength = r.ReadBits<uint>(11);

        uint count = r.ReadBits<uint>(5);

        string target = r.ReadString(targetLength);
        string subject = r.ReadString(subjectLength);
        string body = r.ReadString(bodyLength);

        var attachments = new List<MailAttachment>((int)count);
        for (var i = 0; i < count; ++i)
        {
            // Position before guid — the original built this with an object initializer, which
            // evaluates in source order.
            byte attachPosition = r.ReadUInt8();
            attachments.Add(new MailAttachment(attachPosition, r.ReadPackedGuid128()));
        }

        packet = new SendMail(mailbox, stationeryId, sendMoney, cod, target, subject, body, attachments);
    }
}

// ---- petition ----

public static class PetitionBuyCodec
{
    /// <remarks>
    /// The title length is read before the GUID but the title itself after the index, so a bit
    /// field sits between a length and its string. Reading them in any other order shifts the
    /// title.
    /// </remarks>
    public static void Read(ref SpanPacketReader r, out PetitionBuy packet)
    {
        uint titleLen = r.ReadBits<uint>(7);
        WowGuid128 unit = r.ReadPackedGuid128();
        uint index = r.ReadUInt32();
        packet = new PetitionBuy(unit, index, r.ReadString(titleLen));
    }
}

public static class PetitionShowSignaturesCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out PetitionShowSignatures packet)
        => packet = new PetitionShowSignatures(r.ReadPackedGuid128());
}

public static class QueryPetitionCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out QueryPetition packet)
    {
        uint petitionId = r.ReadUInt32();
        packet = new QueryPetition(petitionId, r.ReadPackedGuid128());
    }
}

public static class PetitionRenameGuildCodec
{
    public static void Read(ref SpanPacketReader r, out PetitionRenameGuild packet)
    {
        WowGuid128 petitionGuid = r.ReadPackedGuid128();

        r.ResetBitReader();
        uint nameLen = r.ReadBits<uint>(7);

        packet = new PetitionRenameGuild(petitionGuid, r.ReadString(nameLen));
    }
}

public static class OfferPetitionCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out OfferPetition packet)
    {
        uint unkInt = r.ReadUInt32();
        WowGuid128 itemGuid = r.ReadPackedGuid128();
        packet = new OfferPetition(unkInt, itemGuid, r.ReadPackedGuid128());
    }
}

public static class DeclinePetitionCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out DeclinePetition packet)
        => packet = new DeclinePetition(r.ReadPackedGuid128());
}

public static class SignPetitionCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SignPetition packet)
    {
        WowGuid128 petitionGuid = r.ReadPackedGuid128();
        packet = new SignPetition(petitionGuid, r.ReadUInt8());
    }
}

public static class TurnInPetitionCodec
{
    /// <remarks>
    /// The five emblem fields are present only when the client sent them, so the skip path leaves
    /// them zero — which is what the old class yielded too, since none of those fields carried a
    /// non-zero initializer. The equivalence test exercises both branches; a happy-path-only test
    /// would not distinguish a wrong default from a right one.
    /// </remarks>
    public static void Read(ref SpanPacketReader r, out TurnInPetition packet)
    {
        WowGuid128 item = r.ReadPackedGuid128();

        if (r.CanRead)
        {
            uint backgroundColor = r.ReadUInt32();
            uint emblemStyle = r.ReadUInt32();
            uint emblemColor = r.ReadUInt32();
            uint borderStyle = r.ReadUInt32();
            packet = new TurnInPetition(item, backgroundColor, emblemStyle, emblemColor, borderStyle,
                r.ReadUInt32());
            return;
        }

        packet = new TurnInPetition(item, 0, 0, 0, 0, 0);
    }
}
