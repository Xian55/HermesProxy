using Framework;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using HermesProxy.World.Server.Systems;
using System;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [HandlesSmsg(Opcode.SMSG_ATTACK_START)]
    internal void HandleAttackStart(WorldPacket packet)
    {
        SAttackStart attack = new();
        attack.Attacker = packet.ReadGuid().To128(GetSession().GameState);
        attack.Victim = packet.ReadGuid().To128(GetSession().GameState);

        if (attack.Attacker == GetSession().GameState.CurrentPlayerGuid)
            MeleeAttackOrder.SwingAnswered(GetSession().ToServer);

        SendPacketToClient(attack);
    }

    [HandlesSmsg(Opcode.SMSG_DISMOUNT)]
    internal void HandleDismount(WorldPacket packet)
    {
        Dismount dismount = new();
        dismount.Guid = packet.ReadPackedGuid().To128(GetSession().GameState);
        SendPacketToClient(dismount);
    }

    [HandlesSmsg(Opcode.SMSG_BREAK_TARGET)]
    internal void HandleBreakTarget(WorldPacket packet)
    {
        BreakTarget pkt = new();
        pkt.UnitGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        SendPacketToClient(pkt);
    }

    [HandlesSmsg(Opcode.SMSG_CLEAR_TARGET)]
    internal void HandleClearTarget(WorldPacket packet)
    {
        ClearTarget pkt = new();
        pkt.Guid = packet.ReadGuid().To128(GetSession().GameState);
        SendPacketToClient(pkt);
    }

    [HandlesSmsg(Opcode.SMSG_ATTACK_STOP)]
    internal void HandleAttackStop(WorldPacket packet)
    {
        SAttackStop attack = new();
        attack.Attacker = packet.ReadPackedGuid().To128(GetSession().GameState);
        // AzerothCore's refusal path writes the attacker and stops there, so the victim can be
        // absent. ByteBuffer reads straight out of the pooled backing array without checking the
        // length, so an unguarded read invents a victim guid from whatever the last packet left
        // behind — forwarded to the client and compared against the current target below.
        WowGuid64 victim64 = packet.CanRead() ? packet.ReadPackedGuid() : default;
        attack.Victim = victim64.To128(GetSession().GameState);
        // V3_4_3 backends (e.g. AzerothCore) can emit a short SMSG_ATTACKSTOP without the
        // trailing "now dead" uint32; guard the read so it doesn't kill the WorldClient
        // receive loop (issue #102). Gated to V3_4_3 so V1_14/V2_5 keep the original
        // unconditional read (no behaviour change for older modern clients).
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            attack.NowDead = packet.CanRead() && packet.ReadUInt32() != 0;
        else
            attack.NowDead = packet.ReadUInt32() != 0;

        var state = GetSession().GameState;
        if (attack.Attacker == state.CurrentPlayerGuid)
            MeleeAttackOrder.ServerStopped(state, GetSession().ToServer, victim64);

        SendPacketToClient(attack);
    }
    [HandlesSmsg(Opcode.SMSG_ATTACKER_STATE_UPDATE)]
    internal void HandleAttackerStateUpdate(WorldPacket packet)
    {
        AttackerStateUpdate attack = new();
        uint hitInfo = packet.ReadUInt32();
        attack.HitInfo = LegacyVersion.ConvertHitInfoFlags(hitInfo);
        attack.AttackerGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        attack.VictimGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        attack.Damage = packet.ReadInt32();
        attack.OriginalDamage = attack.Damage;

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_3_9183))
            attack.OverDamage = packet.ReadInt32();
        else
            attack.OverDamage = -1;

        byte subDamageCount = packet.ReadUInt8();
        for (int i = 0; i < subDamageCount; i++)
        {
            SubDamage subDmg = new();

            uint school = packet.ReadUInt32();
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                school = (1u << (byte)school);

            subDmg.SchoolMask = school;
            subDmg.FloatDamage = packet.ReadFloat();
            subDmg.IntDamage = packet.ReadInt32();

            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_3_9183) ||
                hitInfo.HasAnyFlag(HitInfo.PartialAbsorb | HitInfo.FullAbsorb))
                subDmg.Absorbed = packet.ReadInt32();

            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_3_9183) ||
                hitInfo.HasAnyFlag(HitInfo.PartialResist | HitInfo.FullResist))
                subDmg.Resisted = packet.ReadInt32();

            attack.SubDmg.Add(subDmg);
        }

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_3_9183))
            attack.VictimState = packet.ReadUInt8();
        else
            attack.VictimState = (byte)packet.ReadUInt32();

        attack.AttackerState = packet.ReadInt32();
        attack.MeleeSpellID = packet.ReadUInt32();

        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_3_9183) ||
            hitInfo.HasAnyFlag(HitInfo.Block))
            attack.BlockAmount = packet.ReadInt32();

        if (hitInfo.HasAnyFlag(HitInfo.RageGain))
            attack.RageGained = packet.ReadInt32();

        if (hitInfo.HasAnyFlag(HitInfo.Unk0))
        {
            attack.UnkState = new();
            attack.UnkState.State1 = packet.ReadUInt32();
            attack.UnkState.State2 = packet.ReadFloat();
            attack.UnkState.State3 = packet.ReadFloat();
            attack.UnkState.State4 = packet.ReadFloat();
            attack.UnkState.State5 = packet.ReadFloat();
            attack.UnkState.State6 = packet.ReadFloat();
            attack.UnkState.State7 = packet.ReadFloat();
            attack.UnkState.State8 = packet.ReadFloat();
            attack.UnkState.State9 = packet.ReadFloat();
            attack.UnkState.State10 = packet.ReadFloat();
            attack.UnkState.State11 = packet.ReadFloat();
            attack.UnkState.State12 = packet.ReadUInt32();
            packet.ReadUInt32();
            packet.ReadUInt32();
        }

        SendPacketToClient(attack);
    }
    // A swing error is the server's answer to the swing too, so a stop held behind it can go.
    [HandlesSmsg(Opcode.SMSG_ATTACKSWING_NOTINRANGE)]
    internal void HandleAttackSwingNotInRange(WorldPacket packet)
    {
        AttackSwingError attack = new();
        attack.Reason = AttackSwingErr.NotInRange;
        MeleeAttackOrder.SwingAnswered(GetSession().ToServer);
        SendPacketToClient(attack);
    }
    [HandlesSmsg(Opcode.SMSG_ATTACKSWING_BADFACING)]
    internal void HandleAttackSwingBadFacing(WorldPacket packet)
    {
        AttackSwingError attack = new();
        attack.Reason = AttackSwingErr.BadFacing;
        MeleeAttackOrder.SwingAnswered(GetSession().ToServer);
        SendPacketToClient(attack);
    }
    [HandlesSmsg(Opcode.SMSG_ATTACKSWING_DEADTARGET)]
    internal void HandleAttackSwingDeadTarget(WorldPacket packet)
    {
        AttackSwingError attack = new();
        attack.Reason = AttackSwingErr.DeadTarget;
        MeleeAttackOrder.SwingAnswered(GetSession().ToServer);
        SendPacketToClient(attack);
    }
    [HandlesSmsg(Opcode.SMSG_ATTACKSWING_CANT_ATTACK)]
    internal void HandleAttackSwingCantAttack(WorldPacket packet)
    {
        AttackSwingError attack = new();
        attack.Reason = AttackSwingErr.CantAttack;
        MeleeAttackOrder.SwingAnswered(GetSession().ToServer);
        SendPacketToClient(attack);
    }
    [HandlesSmsg(Opcode.SMSG_CANCEL_COMBAT)]
    internal void HandleCancelCombat(WorldPacket packet)
    {
        MeleeAttackOrder.CombatCancelled(GetSession().GameState, GetSession().ToServer);
        CancelCombat combat = new();
        SendPacketToClient(combat);
    }
    [HandlesSmsg(Opcode.SMSG_AI_REACTION)]
    internal void HandleAIReaction(WorldPacket packet)
    {
        AIReaction reaction = new();
        reaction.UnitGUID = packet.ReadGuid().To128(GetSession().GameState);
        reaction.Reaction = packet.ReadUInt32();
        SendPacketToClient(reaction);
    }
    [HandlesSmsg(Opcode.SMSG_PARTY_KILL_LOG)]
    internal void HandlePartyKillLog(WorldPacket packet)
    {
        PartyKillLog log = new();
        log.Player = packet.ReadGuid().To128(GetSession().GameState);
        log.Victim = packet.ReadGuid().To128(GetSession().GameState);
        SendPacketToClient(log);
    }

    // SMSG_THREAT_UPDATE / SMSG_HIGHEST_THREAT_UPDATE — re-enabled 2026-05-20 after
    // native TC 3.4.3 sniffs (`World_questing_level_1_parsed.txt`) confirmed the
    // V3_4_3.54261 wire shape matches TC 3.4.3 `CombatPackets.cpp:54-79`:
    //   PackedGuid128 UnitGUID + int32 count + (PackedGuid128 + int64 threat) * count
    // The earlier ~18 GiB OOM was likely due to a stale guess from WPP V3_4_4+ data,
    // not the shape itself. ThreatListSanityCap defends against legacy frames where the
    // count field is garbled (e.g. truncated legacy packet read mid-stream).
    private const int ThreatListSanityCap = 256;

    [HandlesSmsg(Opcode.SMSG_THREAT_UPDATE)]
    internal void HandleThreatUpdate(WorldPacket packet)
    {
        // Wire shape (PackedGuid128 + int32 count + (PackedGuid128 + int64) * count)
        // verified against V3_4_3.54261 native sniffs only. V1_14 / V2_5 modern clients
        // may use a different shape — keep them on the pre-fix silent-drop behaviour
        // until separately verified.
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        ThreatUpdate update = new();
        update.UnitGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        uint count = packet.ReadUInt32();
        if (count > ThreatListSanityCap)
        {
            Framework.Logging.Log.Print(Framework.Logging.LogType.Warn,
                $"SMSG_THREAT_UPDATE: ThreatList count {count} exceeds sanity cap {ThreatListSanityCap}; dropping packet (UnitGUID={update.UnitGUID})");
            return;
        }
        for (uint i = 0; i < count; i++)
        {
            update.ThreatList.Add(new ThreatInfo(
                packet.ReadPackedGuid().To128(GetSession().GameState),
                packet.ReadUInt32()));
        }
        SendPacketToClient(update);
    }

    [HandlesSmsg(Opcode.SMSG_HIGHEST_THREAT_UPDATE)]
    internal void HandleHighestThreatUpdate(WorldPacket packet)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        HighestThreatUpdate update = new();
        update.UnitGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        update.HighestThreatGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        uint count = packet.ReadUInt32();
        if (count > ThreatListSanityCap)
        {
            Framework.Logging.Log.Print(Framework.Logging.LogType.Warn,
                $"SMSG_HIGHEST_THREAT_UPDATE: ThreatList count {count} exceeds sanity cap {ThreatListSanityCap}; dropping packet (UnitGUID={update.UnitGUID})");
            return;
        }
        for (uint i = 0; i < count; i++)
        {
            update.ThreatList.Add(new ThreatInfo(
                packet.ReadPackedGuid().To128(GetSession().GameState),
                packet.ReadUInt32()));
        }
        SendPacketToClient(update);
    }

    [HandlesSmsg(Opcode.SMSG_THREAT_REMOVE)]
    internal void HandleThreatRemove(WorldPacket packet)
    {
        ThreatRemove threat = new();
        threat.UnitGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        threat.AboutGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        SendPacketToClient(threat);
    }

    [HandlesSmsg(Opcode.SMSG_THREAT_CLEAR)]
    internal void HandleThreatClear(WorldPacket packet)
    {
        ThreatClear threat = new();
        threat.GUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        SendPacketToClient(threat);
    }
}
