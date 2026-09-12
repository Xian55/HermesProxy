using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;

namespace HermesProxy.World.Server.Packets;

// Spell and pet CMSG codecs.
//
// The three casting packets share SpellCastRequest, which is the deepest nested read in the
// inbound set: a target block with four optional sub-blocks, two optional-cost lists, a weight
// list, and an optional MovementInfo. It keeps its class form for the same reason MovementInfo
// does — that MovementInfo is a mutable builder the outbound path fills in field by field — so the
// packet structs hold a reference and the one allocation per cast survives until outbound lands.
//
// Converting these retires the last WorldPacket caller of ReadMovementInfoModern; the two readers
// and the test pinning them against each other can go once nothing else needs the pair.

public static class CastSpellCodec
{
    public static void Read(ref SpanPacketReader r, out CastSpell packet)
    {
        var cast = new SpellCastRequest();
        cast.Read(ref r);
        packet = new CastSpell(cast);
    }
}

public static class PetCastSpellCodec
{
    public static void Read(ref SpanPacketReader r, out PetCastSpell packet)
    {
        WowGuid128 petGuid = r.ReadPackedGuid128();
        var cast = new SpellCastRequest();
        cast.Read(ref r);
        packet = new PetCastSpell(petGuid, cast);
    }
}

public static class UseItemCodec
{
    public static void Read(ref SpanPacketReader r, out UseItem packet)
    {
        byte packSlot = r.ReadUInt8();
        byte slot = r.ReadUInt8();
        WowGuid128 castItem = r.ReadPackedGuid128();
        var cast = new SpellCastRequest();
        cast.Read(ref r);
        packet = new UseItem(packSlot, slot, castItem, cast);
    }
}

// ---- cancels ----

public static class CancelCastCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out CancelCast packet)
    {
        WowGuid128 castId = r.ReadPackedGuid128();
        uint spellId = r.ReadUInt32();
        packet = new CancelCast(castId, spellId);
    }
}

public static class CancelChannellingCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out CancelChannelling packet)
    {
        int spellId = r.ReadInt32();
        int reason = r.ReadInt32();
        packet = new CancelChannelling(spellId, reason);
    }
}

public static class CancelAuraCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out CancelAura packet)
    {
        uint spellId = r.ReadUInt32();
        WowGuid128 casterGuid = r.ReadPackedGuid128();
        packet = new CancelAura(spellId, casterGuid);
    }
}

// ---- talents, resurrection, totems ----

public static class LearnTalentCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out LearnTalent packet)
    {
        uint talentId = r.ReadUInt32();
        ushort rank = r.ReadUInt16();
        packet = new LearnTalent(talentId, rank);
    }
}

public static class ResurrectResponseCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ResurrectResponse packet)
    {
        WowGuid128 casterGuid = r.ReadPackedGuid128();
        uint response = r.ReadUInt32();
        packet = new ResurrectResponse(casterGuid, response);
    }
}

public static class SelfResCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SelfRes packet)
        => packet = new SelfRes(r.ReadUInt32());
}

public static class TotemDestroyedCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out TotemDestroyed packet)
    {
        byte slot = r.ReadUInt8();
        WowGuid128 guid = r.ReadPackedGuid128();
        packet = new TotemDestroyed(slot, guid);
    }
}

// ---- pet ----

public static class PetActionCodec
{
    public static void Read(ref SpanPacketReader r, out PetAction packet)
    {
        WowGuid128 petGuid = r.ReadPackedGuid128();
        uint action = r.ReadUInt32();
        WowGuid128 targetGuid = r.ReadPackedGuid128();
        Vector3 actionPosition = r.ReadVector3();
        packet = new PetAction(petGuid, action, targetGuid, actionPosition);
    }
}

public static class PetStopAttackCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out PetStopAttack packet)
        => packet = new PetStopAttack(r.ReadPackedGuid128());
}

public static class PetSetActionCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out PetSetAction packet)
    {
        WowGuid128 petGuid = r.ReadPackedGuid128();
        uint index = r.ReadUInt32();
        uint action = r.ReadUInt32();
        packet = new PetSetAction(petGuid, index, action);
    }
}

public static class PetRenameCodec
{
    /// <remarks>
    /// The name length is read before the declined-names block but the name itself after it, so the
    /// five 7-bit counts sit between a length and its string. Reading them in any other order
    /// shifts every string that follows.
    /// </remarks>
    public static void Read(ref SpanPacketReader r, out PetRename packet)
    {
        PetRenameData renameData = default;
        renameData.PetGUID = r.ReadPackedGuid128();
        renameData.PetNumber = r.ReadInt32();

        uint nameLen = r.ReadBits<uint>(8);

        renameData.HasDeclinedNames = r.HasBit();
        if (renameData.HasDeclinedNames)
        {
            renameData.DeclinedNames = new DeclinedName();
            uint[] count = new uint[PlayerConst.MaxDeclinedNameCases];
            for (int i = 0; i < PlayerConst.MaxDeclinedNameCases; i++)
                count[i] = r.ReadBits<uint>(7);

            for (int i = 0; i < PlayerConst.MaxDeclinedNameCases; i++)
                renameData.DeclinedNames.name[i] = r.ReadString(count[i]);
        }

        renameData.NewName = r.ReadString(nameLen);
        packet = new PetRename(renameData);
    }
}

public static class RequestStabledPetsCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out RequestStabledPets packet)
        => packet = new RequestStabledPets(r.ReadPackedGuid128());
}

public static class BuyStableSlotCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out BuyStableSlot packet)
        => packet = new BuyStableSlot(r.ReadPackedGuid128());
}

public static class PetAbandonCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out PetAbandon packet)
        => packet = new PetAbandon(r.ReadPackedGuid128());
}

public static class StablePetCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out StablePet packet)
        => packet = new StablePet(r.ReadPackedGuid128());
}

public static class UnstablePetCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out UnstablePet packet)
    {
        uint petNumber = r.ReadUInt32();
        WowGuid128 stableMaster = r.ReadPackedGuid128();
        packet = new UnstablePet(petNumber, stableMaster);
    }
}

public static class StableSwapPetCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out StableSwapPet packet)
    {
        uint petNumber = r.ReadUInt32();
        WowGuid128 stableMaster = r.ReadPackedGuid128();
        packet = new StableSwapPet(petNumber, stableMaster);
    }
}

public static class PetCancelAuraCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out PetCancelAura packet)
    {
        WowGuid128 petGuid = r.ReadPackedGuid128();
        uint spellId = r.ReadUInt32();
        packet = new PetCancelAura(petGuid, spellId);
    }
}
