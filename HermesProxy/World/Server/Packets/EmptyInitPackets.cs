using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

// Stub init packets sent at world-enter to a V3_4_3 modern client. Legacy 3.3.5a
// servers don't emit modern equivalents, so the proxy synthesizes empty/default
// versions to satisfy the V3_4_3 client's expectation that these "I have no X data"
// announcements arrive before it dismisses the loading screen and sends
// CMSG_MOVE_INIT_ACTIVE_MOVER_COMPLETE.
//
// Ported from HermesProxy-WOTLK fork:
// X:\Programming\HermesProxy-WOTLK\HermesProxy\World\Server\Packets\Empty*.cs
// X:\Programming\HermesProxy-WOTLK\HermesProxy\World\Client\WorldClient.cs:1085 (HandleLoginVerifyWorld)

public class EmptyInitWorldStates : ServerPacket
{
    public uint MapId;
    public int ZoneId;
    public int AreaId;

    public EmptyInitWorldStates() : base(Opcode.SMSG_INIT_WORLD_STATES, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(MapId);
        _worldPacket.WriteInt32(ZoneId);
        _worldPacket.WriteInt32(AreaId);
        _worldPacket.WriteInt32(0);
    }
}

public class EmptyAllAchievementData : ServerPacket
{
    public EmptyAllAchievementData() : base(Opcode.SMSG_ALL_ACHIEVEMENT_DATA, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(0);
        _worldPacket.WriteInt32(0);
    }
}

public class EmptyAllAccountCriteria : ServerPacket
{
    public EmptyAllAccountCriteria() : base(Opcode.SMSG_ALL_ACCOUNT_CRITERIA, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(0);
    }
}

public class EmptySetupCurrency : ServerPacket
{
    public EmptySetupCurrency() : base(Opcode.SMSG_SETUP_CURRENCY, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(0);
    }
}

public class EmptySpellHistory : ServerPacket
{
    public EmptySpellHistory() : base(Opcode.SMSG_SEND_SPELL_HISTORY, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(0);
    }
}

public class EmptySpellCharges : ServerPacket
{
    public EmptySpellCharges() : base(Opcode.SMSG_SEND_SPELL_CHARGES, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(0);
    }
}

public class EmptyTalentData : ServerPacket
{
    public EmptyTalentData() : base(Opcode.SMSG_UPDATE_TALENT_DATA, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(0u);  // UnspentTalentPoints
        _worldPacket.WriteUInt8(0);    // ActiveGroup
        _worldPacket.WriteUInt32(1u);  // GroupCount = 1
        _worldPacket.WriteUInt8(0);    // TalentCount (byte)
        _worldPacket.WriteUInt32(0u);  // TalentCount (dword)
        _worldPacket.WriteUInt8(0);    // GlyphCount (byte)
        _worldPacket.WriteUInt32(0u);  // GlyphCount (dword)
        _worldPacket.WriteUInt8(4);    // SpecID = MAX_SPECIALIZATIONS (no spec)
        _worldPacket.WriteBit(false);  // IsPetTalents
        _worldPacket.FlushBits();
    }
}

public class EmptyActiveGlyphs : ServerPacket
{
    public EmptyActiveGlyphs() : base(Opcode.SMSG_ACTIVE_GLYPHS, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(0u);
        _worldPacket.WriteBit(true);
        _worldPacket.FlushBits();
    }
}

public class EmptyEquipmentSetList : ServerPacket
{
    public EmptyEquipmentSetList() : base(Opcode.SMSG_LOAD_EQUIPMENT_SET, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(0u);
    }
}

public class EmptyAccountMountUpdate : ServerPacket
{
    public EmptyAccountMountUpdate() : base(Opcode.SMSG_ACCOUNT_MOUNT_UPDATE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteBit(true);
        _worldPacket.WriteUInt32(0u);
        _worldPacket.FlushBits();
    }
}

public class EmptyAccountToyUpdate : ServerPacket
{
    public EmptyAccountToyUpdate() : base(Opcode.SMSG_ACCOUNT_TOY_UPDATE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteBit(true);
        _worldPacket.FlushBits();
        _worldPacket.WriteInt32(0);
        _worldPacket.WriteInt32(0);
        _worldPacket.WriteInt32(0);
    }
}

public class EmptyAccountHeirloomUpdate : ServerPacket
{
    public EmptyAccountHeirloomUpdate() : base(Opcode.SMSG_ACCOUNT_HEIRLOOM_UPDATE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteBit(true);
        _worldPacket.FlushBits();
        _worldPacket.WriteInt32(0);
        _worldPacket.WriteUInt32(0u);
        _worldPacket.WriteUInt32(0u);
    }
}

public class BattlePetJournalLockAcquired : ServerPacket
{
    public BattlePetJournalLockAcquired() : base(Opcode.SMSG_BATTLE_PET_JOURNAL_LOCK_ACQUIRED, ConnectionType.Instance) { }

    public override void Write()
    {
    }
}
