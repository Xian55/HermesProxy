using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

// SMSG_ACCOUNT_HEIRLOOM_UPDATE — refresh trigger for the V3_4_3 client's
// Collections → Heirlooms panel. Wire layout (TC `MiscPackets.cpp::AccountHeirloomUpdate::Write`):
//
//   bit       IsFullUpdate  (1 = full set; partial deltas use the same packet with false)
//   FlushBits
//   int32     Unk           (TC always 0)
//   uint32    ItemCount     (number of owned heirloom item IDs)
//   uint32    FlagsCount    (must equal ItemCount per TC comment)
//   int32[]   ItemIDs       (one entry per ItemCount)
//   uint32[]  Flags         (one entry per FlagsCount; HeirloomPlayerFlags, e.g. UPGRADE_LEVEL_*)
//
// We ship the full 38-item modern heirloom set (Option α): every account sees
// every heirloom as "owned". Flags are always 0 (no upgrade tiers / no PVP marker)
// since the legacy 3.3.5a server has no account-bound collection state to mirror.
// The actual "owned count" the client renders in the panel header comes from the
// matching ActivePlayerData::Heirlooms DynamicUpdateField, not this packet — this
// packet just triggers a panel refresh against descriptor state.
public class AccountHeirloomUpdate : ServerPacket
{
    public AccountHeirloomUpdate() : base(Opcode.SMSG_ACCOUNT_HEIRLOOM_UPDATE, ConnectionType.Instance) { }

    public override void Write()
    {
        var heirlooms = GameData.Heirlooms;

        _worldPacket.WriteBit(true);                      // IsFullUpdate
        _worldPacket.FlushBits();
        _worldPacket.WriteInt32(0);                       // Unk
        _worldPacket.WriteUInt32((uint)heirlooms.Count);  // ItemCount
        _worldPacket.WriteUInt32((uint)heirlooms.Count);  // FlagsCount (== ItemCount)
        foreach (var itemId in heirlooms)
            _worldPacket.WriteInt32(itemId);              // ItemIDs[i]
        for (int i = 0; i < heirlooms.Count; i++)
            _worldPacket.WriteUInt32(0u);                 // Flags[i] (HeirloomPlayerFlags = NONE)
    }
}
