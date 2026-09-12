using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

// Codecs for the NPC-interaction family: gossip, trainers, flight masters, the auctioneer and the
// tabard vendor.
//
// InteractWithNPC is the most-shared inbound packet in the proxy — sixteen opcodes across four
// handler files reduce to one packed GUID — which is why it converts as a unit rather than with any
// one of those files. Its systems are shape B: the opcode is what distinguishes a banker from a
// spirit healer, and it arrives as a JIT constant rather than being read back off a packet that no
// longer carries it.

public static class InteractWithNPCCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out InteractWithNPC packet)
        => packet = new InteractWithNPC(r.ReadPackedGuid128());
}

public static class GossipSelectOptionCodec
{
    public static void Read(ref SpanPacketReader r, out GossipSelectOption packet)
    {
        WowGuid128 gossipUnit = r.ReadPackedGuid128();
        uint gossipId = r.ReadUInt32();
        uint gossipIndex = r.ReadUInt32();
        string promotionCode = r.ReadString(r.ReadBits<uint>(8));
        packet = new GossipSelectOption(gossipUnit, gossipId, gossipIndex, promotionCode);
    }
}

public static class BuyBankSlotCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out BuyBankSlot packet)
        => packet = new BuyBankSlot(r.ReadPackedGuid128());
}

public static class TrainerBuySpellCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out TrainerBuySpell packet)
    {
        WowGuid128 trainerGuid = r.ReadPackedGuid128();
        uint trainerId = r.ReadUInt32();
        uint spellId = r.ReadUInt32();
        packet = new TrainerBuySpell(trainerGuid, trainerId, spellId);
    }
}

public static class ConfirmRespecWipeCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ConfirmRespecWipe packet)
    {
        WowGuid128 trainerGuid = r.ReadPackedGuid128();
        var respecType = (SpecResetType)r.ReadUInt8();
        packet = new ConfirmRespecWipe(trainerGuid, respecType);
    }
}

public static class ActivateTaxiCodec
{
    public static void Read(ref SpanPacketReader r, out ActivateTaxi packet)
    {
        WowGuid128 flightMaster = r.ReadPackedGuid128();
        uint node = r.ReadUInt32();
        uint groundMountId = r.ReadUInt32();
        uint flyingMountId = r.ReadUInt32();
        packet = new ActivateTaxi(flightMaster, node, groundMountId, flyingMountId);
    }
}

public static class AuctionListOwnerItemsCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out AuctionListOwnerItems packet)
    {
        WowGuid128 auctioneer = r.ReadPackedGuid128();
        uint offset = r.ReadUInt32();
        packet = new AuctionListOwnerItems(auctioneer, offset);
    }
}
