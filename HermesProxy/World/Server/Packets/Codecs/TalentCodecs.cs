using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World.Dispatch;

namespace HermesProxy.World.Server.Packets;

/// <summary>
/// The count of (talent, rank) pairs, the tab (-1 once a specialization is chosen), then the
/// pairs: so the 4.4.2 client sent it (01 00 00 00 FF FF FF FF 12 25 00 00 00 00 00 00 for One
/// with Nature). TrinityCore 4.3.4 reads the tab first. Only the 4.4.2 client has the opcode.
/// </summary>
public static class LearnPreviewTalentsCodec
{
    public static void Read(ref SpanPacketReader r, out LearnPreviewTalents packet)
    {
        uint count = r.ReadUInt32();
        int talentTab = r.ReadInt32();
        var talents = new List<TalentEntry>(CodecHelpers.WireCountCapacity(count, in r, 8));
        for (uint i = 0; i < count; i++)
        {
            uint talentId = r.ReadUInt32();
            uint rank = r.ReadUInt32();
            talents.Add(new TalentEntry { TalentID = talentId, Rank = rank });
        }
        packet = new LearnPreviewTalents(talentTab, talents);
    }
}

/// <summary>
/// The pet, the count of (talent, rank) pairs, then the pairs (docs/protocol/4.4.2.60895). Only
/// the 4.4.2 client has the opcode.
/// </summary>
public static class LearnPreviewTalentsPetCodec
{
    public static void Read(ref SpanPacketReader r, out LearnPreviewTalentsPet packet)
    {
        WowGuid128 petGuid = r.ReadPackedGuid128();
        uint count = r.ReadUInt32();
        var talents = new List<TalentEntry>(CodecHelpers.WireCountCapacity(count, in r, 8));
        for (uint i = 0; i < count; i++)
        {
            uint talentId = r.ReadUInt32();
            uint rank = r.ReadUInt32();
            talents.Add(new TalentEntry { TalentID = talentId, Rank = rank });
        }
        packet = new LearnPreviewTalentsPet(petGuid, talents);
    }
}

/// <summary>The talent tab (docs/protocol/4.4.2.60895). Only the 4.4.2 client has the opcode.</summary>
public static class SetPrimaryTalentTreeCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SetPrimaryTalentTree packet)
        => packet = new SetPrimaryTalentTree(r.ReadUInt32());
}
