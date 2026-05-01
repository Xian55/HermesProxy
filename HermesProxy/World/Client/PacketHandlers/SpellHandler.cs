using Framework;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [PacketHandler(Opcode.SMSG_SEND_KNOWN_SPELLS)]
    void HandleSendKnownSpells(WorldPacket packet)
    {
        SendKnownSpells spells = new SendKnownSpells();
        spells.InitialLogin = packet.ReadBool();
        ushort spellCount = packet.ReadUInt16();
        for (ushort i = 0; i < spellCount; i++)
        {
            uint spellId;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                spellId = packet.ReadUInt32();
            else
                spellId = packet.ReadUInt16();
            spells.KnownSpells.Add(spellId);
            packet.ReadInt16();
        }
        SendPacketToClient(spells);

        ushort cooldownCount = packet.ReadUInt16();
        if (cooldownCount != 0)
        {
            SendSpellHistory histories = new SendSpellHistory();
            for (ushort i = 0; i < cooldownCount; i++)
            {
                SpellHistoryEntry history = new SpellHistoryEntry();

                uint spellId;
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                    spellId = packet.ReadUInt32();
                else
                    spellId = packet.ReadUInt16();
                history.SpellID = spellId;

                uint itemId;
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V4_2_2_14545))
                    itemId = packet.ReadUInt32();
                else
                    itemId = packet.ReadUInt16();
                history.ItemID = itemId;

                history.Category = packet.ReadUInt16();
                history.RecoveryTime = packet.ReadInt32();
                history.CategoryRecoveryTime = packet.ReadInt32();

                histories.Entries.Add(history);
            }
            SendPacketToClient(histories, Opcode.SMSG_SEND_UNLEARN_SPELLS);
        }

        // These packets don't exist in Vanilla.
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            SendPacketToClient(new SendUnlearnSpells());
            SendPacketToClient(new SendSpellCharges());
        }
    }

    [PacketHandler(Opcode.SMSG_SUPERCEDED_SPELLS)]
    void HandleSupercededSpells(WorldPacket packet)
    {
        SupercededSpells spells = new SupercededSpells();
        uint spellId;
        uint supercededId;
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
        {
            supercededId = packet.ReadUInt32();
            spellId = packet.ReadUInt32();
        }
        else
        {
            supercededId = packet.ReadUInt16();
            spellId = packet.ReadUInt16();
        }
        spells.SpellID.Add(spellId);
        spells.Superceded.Add(supercededId);
        SendPacketToClient(spells);
    }

    [PacketHandler(Opcode.SMSG_LEARNED_SPELL)]
    void HandleLearnedSpell(WorldPacket packet)
    {
        LearnedSpells spells = new LearnedSpells();
        uint spellId = packet.ReadUInt32();
        spells.Spells.Add(spellId);
        SendPacketToClient(spells);
    }

    [PacketHandler(Opcode.SMSG_SEND_UNLEARN_SPELLS)]
    void HandleSendUnlearnSpells(WorldPacket packet)
    {
        SendUnlearnSpells spells = new SendUnlearnSpells();
        uint spellCount = packet.ReadUInt32();
        for (uint i = 0; i < spellCount; i++)
        {
            uint spellId = packet.ReadUInt32();
            spells.Spells.Add(spellId);
        }
        SendPacketToClient(spells);
    }

    [PacketHandler(Opcode.SMSG_UNLEARNED_SPELLS)]
    void HandleUnlearnedSpells(WorldPacket packet)
    {
        UnlearnedSpells spells = new UnlearnedSpells();
        uint spellId;
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
            spellId = packet.ReadUInt32();
        else
            spellId = packet.ReadUInt16();
        spells.Spells.Add(spellId);
        SendPacketToClient(spells);
    }

    [PacketHandler(Opcode.SMSG_CAST_FAILED)]
    void HandleCastFailed(WorldPacket packet)
    {
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.ReadUInt8(); // cast count

        uint spellId = packet.ReadUInt32();
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            var status = packet.ReadUInt8();
            if (status != 2)
                return;
        }

        uint reason = packet.ReadUInt8();
        if (LegacyVersion.InVersion(ClientVersionBuild.V2_0_1_6180, ClientVersionBuild.V3_0_2_9056))
            packet.ReadUInt8(); // cast count
        int arg1 = 0;
        int arg2 = 0;
        if (packet.CanRead())
            arg1 = packet.ReadInt32();
        if (packet.CanRead())
            arg2 = packet.ReadInt32();

        // Check special casts first - try next melee, then auto repeat
        ClientCastRequest? specialCast = null;
        bool isAutoRepeat = false;

        if (GetSession().GameState.CurrentClientNextMeleeCast != null &&
            GetSession().GameState.CurrentClientNextMeleeCast!.SpellId == spellId)
        {
            specialCast = GetSession().GameState.CurrentClientNextMeleeCast;
        }
        else if (GetSession().GameState.CurrentClientAutoRepeatCast != null &&
                 GetSession().GameState.CurrentClientAutoRepeatCast!.SpellId == spellId)
        {
            specialCast = GetSession().GameState.CurrentClientAutoRepeatCast;
            isAutoRepeat = true;
        }

        if (specialCast != null)
        {
            CastFailed failed = new();
            failed.SpellID = specialCast.SpellId;
            failed.SpellXSpellVisualID = specialCast.SpellXSpellVisualId;
            failed.Reason = LegacyVersion.ConvertSpellCastResult(reason);
            failed.CastID = specialCast.ServerGUID;
            failed.FailedArg1 = arg1;
            failed.FailedArg2 = arg2;
            SendPacketToClient(failed);

            if (isAutoRepeat)
                GetSession().GameState.CurrentClientAutoRepeatCast = null;
            else
                GetSession().GameState.CurrentClientNextMeleeCast = null;
        }
        // Look up pending normal cast by SpellId (queue-based, FIFO order)
        else if (GetSession().GameState.TryDequeuePendingNormalCast(spellId, out var pendingCast))
        {
            if (!pendingCast!.HasStarted)
            {
                SpellPrepare prepare2 = new SpellPrepare();
                prepare2.ClientCastID = pendingCast.ClientGUID;
                prepare2.ServerCastID = pendingCast.ServerGUID;
                SendPacketToClient(prepare2);
            }

            CastFailed failed = new();
            failed.SpellID = pendingCast.SpellId;
            failed.SpellXSpellVisualID = pendingCast.SpellXSpellVisualId;
            failed.Reason = LegacyVersion.ConvertSpellCastResult(reason);
            failed.CastID = pendingCast.ServerGUID;
            failed.FailedArg1 = arg1;
            failed.FailedArg2 = arg2;
            SendPacketToClient(failed);
        }
    }

    [PacketHandler(Opcode.SMSG_PET_CAST_FAILED, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
    void HandlePetCastFailed(WorldPacket packet)
    {
        uint spellId = packet.ReadUInt32();
        var status = packet.ReadUInt8();
        if (status != 2)
            return;

        // Look up pending pet cast by SpellId (queue-based, FIFO order)
        if (!GetSession().GameState.TryDequeuePendingPetCast(spellId, out var pendingCast))
            return;

        if (!pendingCast!.HasStarted)
        {
            SpellPrepare prepare2 = new SpellPrepare();
            prepare2.ClientCastID = pendingCast.ClientGUID;
            prepare2.ServerCastID = pendingCast.ServerGUID;
            SendPacketToClient(prepare2);
        }

        PetCastFailed spell = new PetCastFailed();
        spell.SpellID = spellId;
        uint reason = packet.ReadUInt8();
        spell.Reason = LegacyVersion.ConvertSpellCastResult(reason);
        spell.CastID = pendingCast.ServerGUID;
        SendPacketToClient(spell);
    }

    [PacketHandler(Opcode.SMSG_PET_CAST_FAILED, ClientVersionBuild.V2_0_1_6180)]
    void HandlePetCastFailedTBC(WorldPacket packet)
    {
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.ReadUInt8(); // cast count

        uint spellId = packet.ReadUInt32();

        // Look up pending pet cast by SpellId (queue-based, FIFO order)
        if (!GetSession().GameState.TryDequeuePendingPetCast(spellId, out var pendingCast))
            return;

        if (!pendingCast!.HasStarted)
        {
            SpellPrepare prepare2 = new SpellPrepare();
            prepare2.ClientCastID = pendingCast.ClientGUID;
            prepare2.ServerCastID = pendingCast.ServerGUID;
            SendPacketToClient(prepare2);
        }

        PetCastFailed failed = new PetCastFailed();
        failed.SpellID = spellId;
        uint reason = packet.ReadUInt8();
        failed.Reason = LegacyVersion.ConvertSpellCastResult(reason);
        failed.CastID = pendingCast.ServerGUID;

        if (packet.CanRead())
            failed.FailedArg1 = packet.ReadInt32();
        if (packet.CanRead())
            failed.FailedArg2 = packet.ReadInt32();

        SendPacketToClient(failed);
    }

    [PacketHandler(Opcode.SMSG_SPELL_FAILED_OTHER)]
    void HandleSpellFailedOther(WorldPacket packet)
    {
        WowGuid128 casterUnit;
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            casterUnit = packet.ReadPackedGuid().To128(GetSession().GameState);
        else
            casterUnit = packet.ReadGuid().To128(GetSession().GameState);

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.ReadUInt8(); // Cast Count

        uint spellId = packet.ReadUInt32();
        byte reason = 61;
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            reason = (byte)LegacyVersion.ConvertSpellCastResult(packet.ReadUInt8());

        WowGuid128 castId;
        uint spellVisual;
        // Try to find pending cast info (peek, don't remove - this is informational).
        // Match by either modern SpellId or LegacySpellId for SoM-renumbered items.
        if (GetSession().GameState.CurrentPlayerGuid == casterUnit &&
            GetSession().GameState.PendingNormalCasts.FirstOrDefault(c => c.SpellId == spellId || (c.LegacySpellId != 0 && c.LegacySpellId == spellId)) is { } pendingNormal)
        {
            castId = pendingNormal.ServerGUID;
            spellVisual = pendingNormal.SpellXSpellVisualId;
            if (pendingNormal.LegacySpellId != 0)
                spellId = pendingNormal.SpellId;
        }
        else if (GetSession().GameState.CurrentPetGuid == casterUnit &&
                 GetSession().GameState.PendingPetCasts.FirstOrDefault(c => c.SpellId == spellId || (c.LegacySpellId != 0 && c.LegacySpellId == spellId)) is { } pendingPet)
        {
            castId = pendingPet.ServerGUID;
            spellVisual = pendingPet.SpellXSpellVisualId;
            if (pendingPet.LegacySpellId != 0)
                spellId = pendingPet.SpellId;
        }
        else
        {
            castId = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, (uint)GetSession().GameState.CurrentMapId!, spellId, spellId + casterUnit.GetCounter());
            spellVisual = GameData.GetSpellVisual(spellId);
        }

        SpellFailure spell = new SpellFailure();
        spell.CasterUnit = casterUnit;
        spell.CastID = castId;
        spell.SpellID = spellId;
        spell.SpellXSpellVisualID = spellVisual;
        spell.Reason = reason;
        SendPacketToClient(spell);

        SpellFailedOther spell2 = new SpellFailedOther();
        spell2.CasterUnit = casterUnit;
        spell2.CastID = castId;
        spell2.SpellID = spellId;
        spell2.SpellXSpellVisualID = spellVisual;
        spell2.Reason = reason;
        SendPacketToClient(spell2);
    }

    [PacketHandler(Opcode.SMSG_SPELL_START)]
    void HandleSpellStart(WorldPacket packet)
    {
        if (GetSession().GameState.CurrentMapId == null)
            return;

        SpellStart spell = new SpellStart();
        spell.Cast = HandleSpellStartOrGo(packet, false);

        // Mark pending cast as started (queue-based, FIFO order)
        if (GetSession().GameState.CurrentPlayerGuid == spell.Cast.CasterUnit &&
            GetSession().GameState.TryMarkPendingNormalCastStarted((uint)spell.Cast.SpellID, out var pendingCast))
        {
            spell.Cast.CastID = pendingCast!.ServerGUID;
            spell.Cast.SpellXSpellVisualID = pendingCast.SpellXSpellVisualId;
            // SoM-renumbered item: rewrite the legacy spell id back to the modern one the client expects.
            if (pendingCast.LegacySpellId != 0)
                spell.Cast.SpellID = (int)pendingCast.SpellId;

            SpellPrepare prepare = new();
            prepare.ClientCastID = pendingCast.ClientGUID;
            prepare.ServerCastID = spell.Cast.CastID;
            SendPacketToClient(prepare);

            // Clear non-started casts and send failures for them
            // (keeps the started cast so SPELL_GO can dequeue it)
            var failedCasts = GetSession().GameState.ClearNonStartedNormalCasts();
            foreach (var failed in failedCasts)
                GetSession().InstanceSocket.SendCastRequestFailed(failed, false);
        }
        else if (GetSession().GameState.CurrentPetGuid == spell.Cast.CasterUnit &&
                 GetSession().GameState.TryMarkPendingPetCastStarted((uint)spell.Cast.SpellID, out var pendingPetCast))
        {
            spell.Cast.CastID = pendingPetCast!.ServerGUID;
            spell.Cast.SpellXSpellVisualID = pendingPetCast.SpellXSpellVisualId;
            if (pendingPetCast.LegacySpellId != 0)
                spell.Cast.SpellID = (int)pendingPetCast.SpellId;

            SpellPrepare prepare = new();
            prepare.ClientCastID = pendingPetCast.ClientGUID;
            prepare.ServerCastID = spell.Cast.CastID;
            SendPacketToClient(prepare);

            // Clear non-started pet casts and send failures for them
            var failedPetCasts = GetSession().GameState.ClearNonStartedPetCasts();
            foreach (var failed in failedPetCasts)
                GetSession().InstanceSocket.SendCastRequestFailed(failed, true);
        }

        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            // We need spell id for SMSG_SPELL_DISPELL_LOG since its not sent by server
            if (GameData.DispellSpells.Contains((uint)spell.Cast.SpellID))
                GetSession().GameState.LastDispellSpellId = (uint)spell.Cast.SpellID;
        }

        SendPacketToClient(spell);
    }

    [PacketHandler(Opcode.SMSG_SPELL_GO)]
    void HandleSpellGo(WorldPacket packet)
    {
        if (GetSession().GameState.CurrentMapId == null)
            return;

        SpellGo spell = new SpellGo();
        spell.Cast = HandleSpellStartOrGo(packet, true);

        // Dequeue completed cast (queue-based, FIFO order)
        if (GetSession().GameState.CurrentPlayerGuid == spell.Cast.CasterUnit &&
            GetSession().GameState.TryDequeuePendingNormalCast((uint)spell.Cast.SpellID, out var pendingCast))
        {
            spell.Cast.CastID = pendingCast!.ServerGUID;
            spell.Cast.SpellXSpellVisualID = pendingCast.SpellXSpellVisualId;
            // SoM-renumbered item: rewrite the legacy spell id back to the modern one the client expects.
            if (pendingCast.LegacySpellId != 0)
                spell.Cast.SpellID = (int)pendingCast.SpellId;

            // For instant spells that skip SPELL_START, we need to send SpellPrepare
            // before SpellGo so the client knows which cast completed
            if (!pendingCast.HasStarted)
            {
                SpellPrepare prepare = new();
                prepare.ClientCastID = pendingCast.ClientGUID;
                prepare.ServerCastID = spell.Cast.CastID;
                SendPacketToClient(prepare);
            }
        }
        else if (GetSession().GameState.CurrentPlayerGuid == spell.Cast.CasterUnit &&
            GetSession().GameState.CurrentClientNextMeleeCast != null &&
            GetSession().GameState.CurrentClientNextMeleeCast!.SpellId == spell.Cast.SpellID)
        {
            spell.Cast.CastID = GetSession().GameState.CurrentClientNextMeleeCast!.ServerGUID;
            spell.Cast.SpellXSpellVisualID = GetSession().GameState.CurrentClientNextMeleeCast!.SpellXSpellVisualId;
            GetSession().GameState.CurrentClientNextMeleeCast = null;
        }
        else if (GetSession().GameState.CurrentPlayerGuid == spell.Cast.CasterUnit &&
            GetSession().GameState.CurrentClientAutoRepeatCast != null &&
            GetSession().GameState.CurrentClientAutoRepeatCast!.SpellId == spell.Cast.SpellID)
        {
            spell.Cast.CastID = GetSession().GameState.CurrentClientAutoRepeatCast!.ServerGUID;
            spell.Cast.SpellXSpellVisualID = GetSession().GameState.CurrentClientAutoRepeatCast!.SpellXSpellVisualId;
            // Note: Don't clear auto-repeat cast here - it stays active until cancelled
        }
        else if (GetSession().GameState.CurrentPetGuid == spell.Cast.CasterUnit &&
                 GetSession().GameState.TryDequeuePendingPetCast((uint)spell.Cast.SpellID, out var pendingPetCast))
        {
            spell.Cast.CastID = pendingPetCast!.ServerGUID;
            spell.Cast.SpellXSpellVisualID = pendingPetCast.SpellXSpellVisualId;
            if (pendingPetCast.LegacySpellId != 0)
                spell.Cast.SpellID = (int)pendingPetCast.SpellId;

            // For instant pet spells that skip SPELL_START
            if (!pendingPetCast.HasStarted)
            {
                SpellPrepare prepare = new();
                prepare.ClientCastID = pendingPetCast.ClientGUID;
                prepare.ServerCastID = spell.Cast.CastID;
                SendPacketToClient(prepare);
            }
        }

        if (!spell.Cast.CasterUnit.IsEmpty() && GameData.AuraSpells.Contains((uint)spell.Cast.SpellID))
        {
            uint spellId = (uint)spell.Cast.SpellID;
            foreach (WowGuid128 target in spell.Cast.HitTargets)
            {
                // Check if this is an aura refresh (target already has this aura)
                var updateFields = GetSession().GameState.GetCachedObjectFieldsLegacy(target);
                if (updateFields != null)
                {
                    int existingSlot = FindAuraSlotBySpellId(target, spellId, updateFields);
                    if (existingSlot >= 0)
                    {
                        // Aura refresh detected - send AuraUpdate to refresh the duration timer
                        SendAuraRefreshUpdate(target, spellId, spell.Cast.CasterUnit, (byte)existingSlot, updateFields);
                    }
                }

                GetSession().GameState.StoreLastAuraCasterOnTarget(target, spellId, spell.Cast.CasterUnit);
            }
        }

        SendPacketToClient(spell);
    }

    SpellCastData HandleSpellStartOrGo(WorldPacket packet, bool isSpellGo)
    {
        SpellCastData dbdata = new SpellCastData();

        dbdata.CasterGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        dbdata.CasterUnit = packet.ReadPackedGuid().To128(GetSession().GameState);

        // Queue-based spell tracking replaces the need for artificial delay.
        // The old Thread.Sleep workaround was needed because single-variable tracking
        // would get overwritten when spamming spells, causing CastID mismatches.

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.ReadUInt8(); // cast count

        dbdata.SpellID = packet.ReadInt32();
        dbdata.SpellXSpellVisualID = GameData.GetSpellVisual((uint)dbdata.SpellID);
        dbdata.CastID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, (uint)GetSession().GameState.CurrentMapId!, (uint)dbdata.SpellID, (ulong)dbdata.SpellID + dbdata.CasterUnit.GetCounter());

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) && LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056) && !isSpellGo)
            packet.ReadUInt8(); // cast count

        uint flags;
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            flags = packet.ReadUInt32();
        else
            flags = packet.ReadUInt16();
        dbdata.CastFlags = flags;

        if (!isSpellGo || LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            dbdata.CastTime = packet.ReadUInt32();

        if (isSpellGo)
        {
            var hitCount = packet.ReadUInt8();
            for (var i = 0; i < hitCount; i++)
            {
                WowGuid128 hitTarget = packet.ReadGuid().To128(GetSession().GameState);
                dbdata.HitTargets.Add(hitTarget);
            }

            var missCount = packet.ReadUInt8();
            for (var i = 0; i < missCount; i++)
            {
                WowGuid128 missTarget = packet.ReadGuid().To128(GetSession().GameState);
                SpellMissInfo missType = (SpellMissInfo)packet.ReadUInt8();
                SpellMissInfo reflectType = SpellMissInfo.None;
                if (missType == SpellMissInfo.Reflect)
                    reflectType = (SpellMissInfo)packet.ReadUInt8();

                dbdata.MissTargets.Add(missTarget);
                dbdata.MissStatus.Add(new SpellMissStatus(missType, reflectType));
            }
        }

        var targetFlags = LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) ?
            (SpellCastTargetFlags)packet.ReadUInt32() : (SpellCastTargetFlags)packet.ReadUInt16();
        dbdata.Target.Flags = targetFlags;

        WowGuid128 unitTarget = WowGuid128.Empty;
        if (targetFlags.HasAnyFlag(SpellCastTargetFlags.Unit | SpellCastTargetFlags.CorpseEnemy | SpellCastTargetFlags.GameObject |
            SpellCastTargetFlags.CorpseAlly | SpellCastTargetFlags.UnitMinipet))
            unitTarget = packet.ReadPackedGuid().To128(GetSession().GameState);
        dbdata.Target.Unit = unitTarget;

        WowGuid128 itemTarget = WowGuid128.Empty;
        if (targetFlags.HasAnyFlag(SpellCastTargetFlags.Item | SpellCastTargetFlags.TradeItem))
            itemTarget = packet.ReadPackedGuid().To128(GetSession().GameState);
        dbdata.Target.Item = itemTarget;

        if (targetFlags.HasAnyFlag(SpellCastTargetFlags.SourceLocation))
        {
            dbdata.Target.SrcLocation = new TargetLocation();
            dbdata.Target.SrcLocation.Transport = WowGuid128.Empty;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                dbdata.Target.SrcLocation.Transport = packet.ReadPackedGuid().To128(GetSession().GameState);

            dbdata.Target.SrcLocation.Location = packet.ReadVector3();
        }

        if (targetFlags.HasAnyFlag(SpellCastTargetFlags.DestLocation))
        {
            dbdata.Target.DstLocation = new TargetLocation();
            dbdata.Target.DstLocation.Transport = WowGuid128.Empty;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_8_9464))
                dbdata.Target.DstLocation.Transport = packet.ReadPackedGuid().To128(GetSession().GameState);

            dbdata.Target.DstLocation.Location = packet.ReadVector3();
        }

        if (targetFlags.HasAnyFlag(SpellCastTargetFlags.String))
            dbdata.Target.Name = packet.ReadCString();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
        {
            if (flags.HasAnyFlag(CastFlag.PredictedPower))
            {
                packet.ReadInt32(); // Rune Cooldown
            }

            if (flags.HasAnyFlag(CastFlag.RuneInfo))
            {
                var spellRuneState = packet.ReadUInt8();
                var playerRuneState = packet.ReadUInt8();

                for (var i = 0; i < 6; i++)
                {
                    var mask = 1 << i;
                    if ((mask & spellRuneState) == 0)
                        continue;

                    if ((mask & playerRuneState) != 0)
                        continue;

                    packet.ReadUInt8(); // Rune Cooldown Passed
                }
            }

            if (isSpellGo)
            {
                if (flags.HasAnyFlag(CastFlag.AdjustMissile))
                {
                    dbdata.MissileTrajectory.Pitch = packet.ReadFloat(); // Elevation
                    dbdata.MissileTrajectory.TravelTime = packet.ReadUInt32(); // Delay time
                }
            }
        }

        if (flags.HasAnyFlag(CastFlag.Projectile))
        {
            dbdata.AmmoDisplayId = packet.ReadInt32();
            dbdata.AmmoInventoryType = packet.ReadInt32();
        }

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
        {
            if (isSpellGo)
            {
                if (flags.HasAnyFlag(CastFlag.VisualChain))
                {
                    packet.ReadInt32();
                    packet.ReadInt32();
                }

                if (targetFlags.HasAnyFlag(SpellCastTargetFlags.DestLocation))
                    packet.ReadInt8(); // Some count

                if (targetFlags.HasAnyFlag(SpellCastTargetFlags.ExtraTargets))
                {
                    var targetCount = packet.ReadInt32();
                    if (targetCount > 0)
                    {
                        TargetLocation location = new();
                        for (var i = 0; i < targetCount; i++)
                        {
                            location.Location = packet.ReadVector3();
                            location.Transport = packet.ReadGuid().To128(GetSession().GameState);
                        }
                        dbdata.TargetPoints.Add(location);
                    }
                }
            }
            else
            {
                if (flags.HasAnyFlag(CastFlag.Immunity))
                {
                    dbdata.Immunities.School = packet.ReadUInt32();
                    dbdata.Immunities.Value = packet.ReadUInt32();
                }

                if (flags.HasAnyFlag(CastFlag.HealPrediction))
                {
                    packet.ReadInt32(); // Predicted Spell ID

                    if (packet.ReadUInt8() == 2)
                        packet.ReadPackedGuid();
                }
            }
        }

        return dbdata;
    }

    [PacketHandler(Opcode.SMSG_CANCEL_AUTO_REPEAT)]
    void HandleCancelAutoRepeat(WorldPacket packet)
    {
        // Clear the auto-repeat cast tracking
        GetSession().GameState.CurrentClientAutoRepeatCast = null;

        CancelAutoRepeat cancel = new CancelAutoRepeat();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            cancel.Guid = packet.ReadPackedGuid().To128(GetSession().GameState);
        else
            cancel.Guid = GetSession().GameState.CurrentPlayerGuid;
        SendPacketToClient(cancel);
    }

    [PacketHandler(Opcode.SMSG_SPELL_COOLDOWN)]
    void HandleSpellCooldown(WorldPacket packet)
    {
        SpellCooldownPkt cooldown = new();
        try
        {
            cooldown.Caster = packet.ReadGuid().To128(GetSession().GameState);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                cooldown.Flags = packet.ReadUInt8();
            while (packet.CanRead())
            {
                SpellCooldownStruct cd = new();
                cd.SpellID = packet.ReadUInt32();
                cd.ForcedCooldown = packet.ReadUInt32();
                cooldown.SpellCooldowns.Add(cd);
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            // wrong structure from arcemu
            // https://github.com/arcemu/arcemu/blob/2_4_3/src/arcemu-world/Spell.cpp#L1554
            packet.ResetReadPos();
            SpellCooldownStruct cd = new();
            cd.SpellID = packet.ReadUInt32();
            cooldown.Caster = packet.ReadPackedGuid().To128(GetSession().GameState);
            cd.ForcedCooldown = packet.ReadUInt32();
            cooldown.SpellCooldowns.Add(cd);
        }
        SendPacketToClient(cooldown);
    }

    [PacketHandler(Opcode.SMSG_COOLDOWN_EVENT)]
    void HandleCooldownEvent(WorldPacket packet)
    {
        CooldownEvent cooldown = new();
        cooldown.SpellID = packet.ReadUInt32();
        WowGuid64 guid = packet.ReadGuid();
        cooldown.IsPet = guid.GetHighType() == HighGuidType.Pet;
        SendPacketToClient(cooldown);
    }

    [PacketHandler(Opcode.SMSG_CLEAR_COOLDOWN)]
    void HandleClearCooldown(WorldPacket packet)
    {
        ClearCooldown cooldown = new();
        cooldown.SpellID = packet.ReadUInt32();
        WowGuid64 guid = packet.ReadGuid();
        cooldown.IsPet = guid.GetHighType() == HighGuidType.Pet;
        SendPacketToClient(cooldown);
    }

    [PacketHandler(Opcode.SMSG_COOLDOWN_CHEAT)]
    void HandleCooldownCheat(WorldPacket packet)
    {
        CooldownCheat cooldown = new();
        cooldown.Guid = packet.ReadGuid().To128(GetSession().GameState);
        SendPacketToClient(cooldown);
    }

    [PacketHandler(Opcode.SMSG_SPELL_NON_MELEE_DAMAGE_LOG)]
    void HandleSpellNonMeleeDamageLog(WorldPacket packet)
    {
        SpellNonMeleeDamageLog spell = new();
        spell.TargetGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        spell.CasterGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        spell.SpellID = packet.ReadUInt32();
        spell.SpellXSpellVisualID = GameData.GetSpellVisual(spell.SpellID);
        spell.CastID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, (uint)GetSession().GameState.CurrentMapId!, spell.SpellID, spell.SpellID + spell.CasterGUID.GetCounter());
        spell.Damage = packet.ReadInt32();
        spell.OriginalDamage = spell.Damage;

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_3_9183))
            spell.Overkill = packet.ReadInt32();
        else
            spell.Overkill = -1;

        byte school = packet.ReadUInt8();
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            school = (byte)(1u << school);

        spell.SchoolMask = school;
        spell.Absorbed = packet.ReadInt32();
        spell.Resisted = packet.ReadInt32();
        spell.Periodic = packet.ReadBool();
        packet.ReadUInt8(); // unused
        spell.ShieldBlock = packet.ReadInt32();
        spell.Flags = (SpellHitType)packet.ReadUInt32();

        bool debugOutput = packet.ReadBool();
        if (debugOutput)
        {
            if (!spell.Flags.HasAnyFlag(SpellHitType.Split))
            {
                if (spell.Flags.HasAnyFlag(SpellHitType.CritDebug))
                {
                    packet.ReadFloat(); // roll
                    packet.ReadFloat(); // needed
                }

                if (spell.Flags.HasAnyFlag(SpellHitType.HitDebug))
                {
                    packet.ReadFloat(); // roll
                    packet.ReadFloat(); // needed
                }

                if (spell.Flags.HasAnyFlag(SpellHitType.AttackTableDebug))
                {
                    packet.ReadFloat(); // miss chance
                    packet.ReadFloat(); // dodge chance
                    packet.ReadFloat(); // parry chance
                    packet.ReadFloat(); // block chance
                    packet.ReadFloat(); // glance chance
                    packet.ReadFloat(); // crush chance
                }
            }
        }

        SendPacketToClient(spell);
    }

    [PacketHandler(Opcode.SMSG_SPELL_HEAL_LOG)]
    void HandleSpellHealLog(WorldPacket packet)
    {
        SpellHealLog spell = new();
        spell.TargetGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        spell.CasterGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        spell.SpellID = packet.ReadUInt32();
        spell.HealAmount = packet.ReadInt32();
        spell.OriginalHealAmount = spell.HealAmount;

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_3_9183))
            spell.OverHeal = packet.ReadUInt32();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            spell.Absorbed = packet.ReadUInt32();

        spell.Crit = packet.ReadBool();

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            bool debugOutput = packet.ReadBool();
            if (debugOutput)
            {
                spell.CritRollMade = packet.ReadFloat();
                spell.CritRollNeeded = packet.ReadFloat();
            }
        }

        SendPacketToClient(spell);
    }

    [PacketHandler(Opcode.SMSG_SPELL_PERIODIC_AURA_LOG)]
    void HandleSpellPeriodicAuraLog(WorldPacket packet)
    {
        SpellPeriodicAuraLog spell = new();
        spell.TargetGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        spell.CasterGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        spell.SpellID = packet.ReadUInt32();

        var count = packet.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            var aura = (AuraType)packet.ReadUInt32();
            switch (aura)
            {
                case AuraType.PeriodicDamage:
                case AuraType.PeriodicDamagePercent:
                {
                    SpellPeriodicAuraLog.SpellLogEffect effect = new();
                    effect.Effect = (uint)aura;
                    effect.Amount = packet.ReadInt32();
                    effect.OriginalDamage = effect.Amount;

                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                        effect.OverHealOrKill = packet.ReadUInt32();

                    uint school = packet.ReadUInt32();
                    if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                        school = (1u << (byte)school);

                    effect.SchoolMaskOrPower = school;
                    effect.AbsorbedOrAmplitude = packet.ReadUInt32();
                    effect.Resisted = packet.ReadUInt32();

                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_2_9901))
                        effect.Crit = packet.ReadBool();

                    spell.Effects.Add(effect);
                    break;
                }
                case AuraType.PeriodicHeal:
                case AuraType.ObsModHealth:
                {
                    SpellPeriodicAuraLog.SpellLogEffect effect = new();
                    effect.Effect = (uint)aura;
                    effect.Amount = packet.ReadInt32();
                    effect.OriginalDamage = effect.Amount;

                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                        effect.OverHealOrKill = packet.ReadUInt32();

                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
                        // no idea when this was added exactly
                        effect.AbsorbedOrAmplitude = packet.ReadUInt32();

                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_2_9901))
                        effect.Crit = packet.ReadBool();

                    spell.Effects.Add(effect);
                    break;
                }
                case AuraType.ObsModPower:
                case AuraType.PeriodicEnergize:
                {
                    SpellPeriodicAuraLog.SpellLogEffect effect = new();
                    effect.Effect = (uint)aura;
                    effect.SchoolMaskOrPower = packet.ReadUInt32();
                    effect.Amount = packet.ReadInt32();
                    spell.Effects.Add(effect);
                    break;
                }
                case AuraType.PeriodicManaLeech:
                {
                    SpellPeriodicAuraLog.SpellLogEffect effect = new();
                    effect.Effect = (uint)aura;
                    effect.SchoolMaskOrPower = packet.ReadUInt32();
                    effect.Amount = packet.ReadInt32();
                    packet.ReadFloat(); // Gain multiplier
                    spell.Effects.Add(effect);
                    break;
                }
            }
        }
        SendPacketToClient(spell);
    }

    [PacketHandler(Opcode.SMSG_SPELL_ENERGIZE_LOG)]
    void HandleSpellEnergizeLog(WorldPacket packet)
    {
        SpellEnergizeLog spell = new();
        spell.TargetGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        spell.CasterGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        spell.SpellID = packet.ReadUInt32();
        spell.Type = (PowerType)packet.ReadUInt32();
        spell.Amount = packet.ReadInt32();
        SendPacketToClient(spell);
    }

    [PacketHandler(Opcode.SMSG_SPELL_DELAYED)]
    void HandleSpellDelayed(WorldPacket packet)
    {
        SpellDelayed delay = new();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            delay.CasterGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        else
            delay.CasterGUID = packet.ReadGuid().To128(GetSession().GameState);
        delay.Delay = packet.ReadInt32();
        SendPacketToClient(delay);
    }

    [PacketHandler(Opcode.MSG_CHANNEL_START)]
    void HandleSpellChannelStart(WorldPacket packet)
    {
        SpellChannelStart channel = new();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            channel.CasterGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        else
            channel.CasterGUID = GetSession().GameState.CurrentPlayerGuid;
        channel.SpellID = packet.ReadUInt32();
        channel.SpellXSpellVisualID = GameData.GetSpellVisual(channel.SpellID);
        channel.Duration = packet.ReadUInt32();
        SendPacketToClient(channel);
    }

    [PacketHandler(Opcode.MSG_CHANNEL_UPDATE)]
    void HandleSpellChannelUpdate(WorldPacket packet)
    {
        SpellChannelUpdate channel = new();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            channel.CasterGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        else
            channel.CasterGUID = GetSession().GameState.CurrentPlayerGuid;
        channel.TimeRemaining = packet.ReadInt32();
        SendPacketToClient(channel);
    }

    [PacketHandler(Opcode.SMSG_SPELL_DAMAGE_SHIELD)]
    void HandleSpellDamageShield(WorldPacket packet)
    {
        SpellDamageShield spell = new();
        spell.VictimGUID = packet.ReadGuid().To128(GetSession().GameState);
        spell.CasterGUID = packet.ReadGuid().To128(GetSession().GameState);

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            spell.SpellID = packet.ReadUInt32();
        else
            spell.SpellID = 7294; // Retribution Aura

        spell.Damage = packet.ReadInt32();
        spell.OriginalDamage = spell.Damage;

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            spell.OverKill = packet.ReadUInt32();

        uint school = packet.ReadUInt32();
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            school = (1u << (byte)school);

        spell.SchoolMask = school;
        SendPacketToClient(spell);
    }

    [PacketHandler(Opcode.SMSG_ENVIRONMENTAL_DAMAGE_LOG)]
    void HandleEnvironmentalDamageLog(WorldPacket packet)
    {
        EnvironmentalDamageLog damage = new();
        damage.Victim = packet.ReadGuid().To128(GetSession().GameState);
        damage.Type = (EnvironmentalDamage)packet.ReadUInt8();
        damage.Amount = packet.ReadInt32();
        damage.Absorbed = packet.ReadInt32();
        damage.Resisted = packet.ReadInt32();
        SendPacketToClient(damage);
    }

    [PacketHandler(Opcode.SMSG_SPELL_INSTAKILL_LOG)]
    void HandleSpellInstakillLog(WorldPacket packet)
    {
        SpellInstakillLog spell = new();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            spell.CasterGUID = packet.ReadGuid().To128(GetSession().GameState);
            spell.TargetGUID = packet.ReadGuid().To128(GetSession().GameState);
        }
        else
            spell.CasterGUID = spell.TargetGUID = packet.ReadGuid().To128(GetSession().GameState);
        spell.SpellID = packet.ReadUInt32();
        SendPacketToClient(spell);
    }

    [PacketHandler(Opcode.SMSG_SPELL_DISPELL_LOG)]
    void HandleSpellDispellLog(WorldPacket packet)
    {
        SpellDispellLog spell = new();
        spell.TargetGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
        spell.CasterGUID = packet.ReadPackedGuid().To128(GetSession().GameState);

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            spell.DispelledBySpellID = packet.ReadUInt32();
        else
            spell.DispelledBySpellID = GetSession().GameState.LastDispellSpellId;

        bool hasDebug;
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            hasDebug = packet.ReadBool();
        else
            hasDebug = false;

        int count = packet.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            SpellDispellData dispel = new SpellDispellData();
            dispel.SpellID = packet.ReadUInt32();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                dispel.Harmful = packet.ReadBool();
            spell.DispellData.Add(dispel);
        }

        if (hasDebug)
        {
            packet.ReadInt32(); // unk
            packet.ReadInt32(); // unk
        }

        SendPacketToClient(spell);
    }

    [PacketHandler(Opcode.SMSG_PLAY_SPELL_VISUAL)]
    void HandlePlaySpellVisualKit(WorldPacket packet)
    {
        PlaySpellVisualKit spell = new();
        spell.Unit = packet.ReadGuid().To128(GetSession().GameState);
        spell.KitRecID = packet.ReadUInt32();
        SendPacketToClient(spell);
    }

    [PacketHandler(Opcode.SMSG_UPDATE_AURA_DURATION)]
    void HandleUpdateAuraDuration(WorldPacket packet)
    {
        byte slot = packet.ReadUInt8();
        int duration = packet.ReadInt32();

        WowGuid128 guid = GetSession().GameState.CurrentPlayerGuid;
        if (guid == default)
            return;

        GetSession().GameState.StoreAuraDurationLeft(guid, slot, duration, (int)packet.GetReceivedTime());
        GetSession().GameState.StoreAuraDurationFull(guid, slot, duration);
        if (duration <= 0)
            return;

        var updateFields = GetSession().GameState.GetCachedObjectFieldsLegacy(guid);
        if (updateFields == null)
            return;

        AuraInfo aura = new AuraInfo();
        aura.Slot = slot;
        aura.AuraData = ReadAuraSlot(slot, guid, updateFields)!;
        if (aura.AuraData == null)
            return;

        aura.AuraData.Flags |= AuraFlagsModern.Duration;
        aura.AuraData.Duration = duration;
        aura.AuraData.Remaining = duration;

        AuraUpdate update = new AuraUpdate(guid, false);
        update.Auras.Add(aura);
        SendPacketToClient(update);
    }

    [PacketHandler(Opcode.SMSG_SET_EXTRA_AURA_INFO)]
    [PacketHandler(Opcode.SMSG_SET_EXTRA_AURA_INFO_NEED_UPDATE)]
    void HandleSetExtraAuraInfo(WorldPacket packet)
    {
        WowGuid128 guid = packet.ReadPackedGuid().To128(GetSession().GameState);
        if (!packet.CanRead())
            return;

        byte slot = packet.ReadUInt8();
        uint spellId = packet.ReadUInt32();
        int durationFull = packet.ReadInt32();
        int durationLeft = packet.ReadInt32();

        GetSession().GameState.StoreAuraDurationFull(guid, slot, durationFull);
        GetSession().GameState.StoreAuraDurationLeft(guid, slot, durationLeft, (int)packet.GetReceivedTime());

        if (packet.GetUniversalOpcode(false) == Opcode.SMSG_SET_EXTRA_AURA_INFO_NEED_UPDATE)
            GetSession().GameState.StoreAuraCaster(guid, slot, GetSession().GameState.CurrentPlayerGuid);

        if (durationFull <= 0 && durationLeft <= 0)
            return;

        var updateFields = GetSession().GameState.GetCachedObjectFieldsLegacy(guid);
        if (updateFields == null)
            return;

        AuraInfo aura = new AuraInfo();
        aura.Slot = slot;
        aura.AuraData = ReadAuraSlot(slot, guid, updateFields)!;
        if (aura.AuraData == null)
            return;
        if (aura.AuraData.SpellID != spellId)
            return;

        aura.AuraData.CastUnit = GetSession().GameState.GetAuraCaster(guid, slot, spellId);
        aura.AuraData.Flags |= AuraFlagsModern.Duration;
        aura.AuraData.Duration = durationFull;
        aura.AuraData.Remaining = durationLeft;

        AuraUpdate update = new AuraUpdate(guid, false);
        update.Auras.Add(aura);
        SendPacketToClient(update);
    }

    [PacketHandler(Opcode.SMSG_CLEAR_EXTRA_AURA_INFO)]
    void HandleClearExtraAuraInfo(WorldPacket packet)
    {
        // This TBC opcode clears aura duration info for a target.
        // The modern client doesn't use this mechanism - it uses update fields instead.
        // Simply acknowledge the packet without forwarding to the client.
        packet.ReadPackedGuid(); // target guid
    }

    // FIXME(phase5a-auras): stub. Forwards an empty modern AuraUpdate ONLY for the
    // player's own GUID — that single packet is what V3_4_3's loading screen waits
    // for after the player CreateObject. Legacy 3.3.5a sends one of these for every
    // nearby unit on world-entry; forwarding all of them floods the client and
    // triggers an auto-disconnect, so we drop the rest until full translation lands.
    // Existing buffs/debuffs on the player are also invisible until the full parse.
    // Replace with a slot-by-slot parse:
    //   while (packet.CanRead()) {
    //       slot = packet.ReadUInt8();
    //       flags = packet.ReadUInt8();
    //       if (flags == 0) emit empty aura at slot;
    //       else { spellId, level, charges, optional caster guid, optional duration } -> AuraInfo
    //   }
    // SMSG_AURA_UPDATE_ALL (one shot for all auras on a unit) — loop ReadSingleAura
    // until the packet is exhausted. SMSG_AURA_UPDATE (single update) — one read.
    // Ported from HermesProxy-WOTLK fork's WorldClient.HandleAuraUpdate*. The prior
    // stub-handler discarded all aura entries; the V3_4_3 client tolerates an empty
    // single-update but a multi-update with combat auras stripped causes a state
    // divergence (SMSG_ATTACK_START + empty AURA_UPDATE_ALL on the player) that
    // produces an immediate `CMSG_LOG_DISCONNECT reason=7` — observed in
    // hermes-20260501_004716.log line 1395.
    [PacketHandler(Opcode.SMSG_AURA_UPDATE)]
    [PacketHandler(Opcode.SMSG_AURA_UPDATE_ALL)]
    void HandleAuraUpdate(WorldPacket packet)
    {
        bool isAll = packet.GetUniversalOpcode(false) == Opcode.SMSG_AURA_UPDATE_ALL;
        uint incomingBytes = packet.GetSize();
        WowGuid128 guid = packet.ReadPackedGuid().To128(GetSession().GameState);
        bool isPlayer = guid == GetSession().GameState.CurrentPlayerGuid;

        AuraUpdate update = new AuraUpdate(guid, isAll);

        if (isAll)
        {
            while (packet.CanRead())
                ReadSingleAura(packet, guid, update);
        }
        else
        {
            ReadSingleAura(packet, guid, update);
        }

        Log.Print(LogType.Trace,
            $"[AuraUpdateTrace] guid={guid} isAll={isAll} isPlayer={isPlayer} " +
            $"incomingBytes={incomingBytes} aurasShipped={update.Auras.Count}");

        if (update.Auras.Count > 0)
            SendPacketToClient(update);
    }

    void ReadSingleAura(WorldPacket packet, WowGuid128 guid, AuraUpdate update)
    {
        byte slot = packet.ReadUInt8();
        uint spellId = packet.ReadUInt32();

        AuraInfo aura = new AuraInfo { Slot = slot };

        if (spellId == 0)
        {
            // Aura removed — modern packet ships AuraData=null in this slot.
            aura.AuraData = null!;
            update.Auras.Add(aura);
            return;
        }

        AuraDataInfo data = new AuraDataInfo
        {
            SpellID = spellId,
            CastID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Aura,
                                       (uint)(GetSession().GameState.CurrentMapId ?? 0u),
                                       spellId, guid.GetCounter()),
            SpellXSpellVisualID = GameData.GetSpellVisual(spellId),
        };

        byte legacyFlags = packet.ReadUInt8();
        data.CastLevel = packet.ReadUInt8();
        data.Applications = packet.ReadUInt8();

        data.Flags = AuraFlagsModern.None;
        data.ActiveFlags = 0u;

        // Legacy 3.3.5a aura flag bits → modern AuraFlagsModern + ActiveFlags mapping.
        // Bits 0x10 / 0x20 are Positive / has-Duration. Bits 0x01/0x02/0x04 are the three
        // aura point slots. Bit 0x08 indicates the caster is the affected unit (no GUID).
        // Bit 0x40 indicates the packet carries explicit point floats.
        if ((legacyFlags & 0x10) != 0) data.Flags |= AuraFlagsModern.Positive;
        if ((legacyFlags & 0x20) != 0) data.Flags |= AuraFlagsModern.Duration;
        if ((legacyFlags & 0x01) != 0) data.ActiveFlags |= 1u;
        if ((legacyFlags & 0x02) != 0) data.ActiveFlags |= 2u;
        if ((legacyFlags & 0x04) != 0) data.ActiveFlags |= 4u;

        data.CastUnit = (legacyFlags & 0x08) == 0
            ? packet.ReadPackedGuid().To128(GetSession().GameState)
            : guid;

        if ((legacyFlags & 0x20) != 0)
        {
            data.Duration = packet.ReadInt32();
            data.Remaining = packet.ReadInt32();
        }

        if ((legacyFlags & 0x40) != 0)
        {
            if ((legacyFlags & 0x01) != 0) data.Points.Add(packet.ReadFloat());
            if ((legacyFlags & 0x02) != 0) data.Points.Add(packet.ReadFloat());
            if ((legacyFlags & 0x04) != 0) data.Points.Add(packet.ReadFloat());
        }

        aura.AuraData = data;
        update.Auras.Add(aura);
    }

    // Legacy 3.3.5a SMSG_HEALTH_UPDATE wire format: PackedGuid + uint32 health.
    // Modern V3_4_3 expects: PackedGuid128 + int64 health.
    [PacketHandler(Opcode.SMSG_HEALTH_UPDATE)]
    void HandleHealthUpdate(WorldPacket packet)
    {
        WowGuid128 guid = packet.ReadPackedGuid().To128(GetSession().GameState);
        uint health = packet.ReadUInt32();

        HealthUpdate update = new HealthUpdate(guid)
        {
            Health = health,
        };
        SendPacketToClient(update);
        Log.Print(LogType.Trace, $"[HealthUpdateTrace] guid={guid} health={health}");
    }

    // Legacy 3.3.5a SMSG_POWER_UPDATE wire format: PackedGuid + uint8 powerType + uint32 power.
    // Modern V3_4_3 expects: PackedGuid128 + int32 count + (int32 power, uint8 type)*.
    // The `type` byte is forwarded as-is — the V3_4_3 client interprets it as the
    // global PowerType enum, same as legacy (verified in HermesProxy-WOTLK fork
    // WorldClient.cs:5500-5510 which works for warrior rage).
    [PacketHandler(Opcode.SMSG_POWER_UPDATE)]
    void HandlePowerUpdate(WorldPacket packet)
    {
        WowGuid128 guid = packet.ReadPackedGuid().To128(GetSession().GameState);
        byte powerType = packet.ReadUInt8();
        int power = (int)packet.ReadUInt32();

        PowerUpdate update = new PowerUpdate(guid);
        update.Powers.Add(new PowerUpdatePower(power, powerType));
        SendPacketToClient(update);
        Log.Print(LogType.Trace, $"[PowerUpdateTrace] guid={guid} type={(PowerType)powerType} power={power}");
    }

    [PacketHandler(Opcode.SMSG_RESURRECT_REQUEST)]
    void HandleResurrectRequest(WorldPacket packet)
    {
        ResurrectRequest revive = new();
        revive.CasterGUID = packet.ReadGuid().To128(GetSession().GameState);
        revive.CasterVirtualRealmAddress = GetSession().RealmId.GetAddress();
        packet.ReadUInt32(); // Name Length
        revive.Name = packet.ReadCString();
        revive.Sickness = packet.ReadBool();
        revive.UseTimer = packet.ReadBool();
        SendPacketToClient(revive);
    }

    [PacketHandler(Opcode.SMSG_TOTEM_CREATED)]
    void HandleTotemCreated(WorldPacket packet)
    {
        TotemCreated totem = new();
        totem.Slot = packet.ReadUInt8();
        totem.Totem = packet.ReadGuid().To128(GetSession().GameState);
        totem.Duration = packet.ReadUInt32();
        totem.SpellId = packet.ReadUInt32();
        SendPacketToClient(totem);
    }

    [PacketHandler(Opcode.SMSG_SET_FLAT_SPELL_MODIFIER)]
    [PacketHandler(Opcode.SMSG_SET_PCT_SPELL_MODIFIER)]
    void HandleSetSpellModifier(WorldPacket packet)
    {
        byte classIndex = packet.ReadUInt8();
        byte modIndex = packet.ReadUInt8();
        int modValue = packet.ReadInt32();

        if (GetSession().GameState.CurrentPlayerCreateTime != 0)
        {
            SetSpellModifier spell = new SetSpellModifier(packet.GetUniversalOpcode(false));
            SpellModifierInfo mod = new SpellModifierInfo();
            SpellModifierData data = new SpellModifierData();
            data.ClassIndex = classIndex;
            mod.ModIndex = modIndex;
            data.ModifierValue = modValue;
            mod.ModifierData.Add(data);
            spell.Modifiers.Add(mod);
            SendPacketToClient(spell);
        }

        if (packet.GetUniversalOpcode(false) == Opcode.SMSG_SET_FLAT_SPELL_MODIFIER)
            GetSession().GameState.SetFlatSpellMod(modIndex, classIndex, modValue);
        else
            GetSession().GameState.SetPctSpellMod(modIndex, classIndex, modValue);
    }

    /// <summary>
    /// Finds the aura slot containing the specified spell on a target.
    /// Returns -1 if the spell is not found in any aura slot.
    /// </summary>
    private int FindAuraSlotBySpellId(WowGuid128 target, uint spellId, Dictionary<int, UpdateField> updateFields)
    {
        int UNIT_FIELD_AURA = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_AURA);
        if (UNIT_FIELD_AURA < 0)
            return -1;

        int aurasCount = LegacyVersion.GetAuraSlotsCount();
        for (int i = 0; i < aurasCount; i++)
        {
            if (updateFields.TryGetValue(UNIT_FIELD_AURA + i, out var field) && field.UInt32Value == spellId)
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Sends an AuraUpdate packet to refresh the duration of an existing aura on a target.
    /// Called when an aura spell is recast on a target that already has the aura.
    /// </summary>
    private void SendAuraRefreshUpdate(WowGuid128 target, uint spellId, WowGuid128 caster, byte slot, Dictionary<int, UpdateField> updateFields)
    {
        AuraDataInfo? auraData = ReadAuraSlot(slot, target, updateFields);
        if (auraData == null || auraData.SpellID != spellId)
        {
            return;
        }

        auraData.CastUnit = caster;

        // For the current player, SMSG_UPDATE_AURA_DURATION will follow with the
        // correct server-authoritative duration (accounting for combo points, talents
        // like Improved Slice and Dice, etc.). Don't include a stale cached duration
        // in this proactive refresh update — it would show the wrong value briefly
        // and compound errors on subsequent refreshes.
        bool isCurrentPlayer = (target == GetSession().GameState.CurrentPlayerGuid);

        if (!isCurrentPlayer)
        {
            // For other targets (enemy debuffs, party buffs), use best-guess duration
            // since they don't receive SMSG_UPDATE_AURA_DURATION.
            GetSession().GameState.GetAuraDuration(target, slot, out int durationLeft, out int durationFull);

            if (durationFull <= 0)
            {
                durationFull = GameData.GetAuraSpellDuration(spellId);
            }

            if (durationFull > 0)
            {
                auraData.Flags |= AuraFlagsModern.Duration;
                auraData.Duration = durationFull;
                auraData.Remaining = durationFull;

                GetSession().GameState.StoreAuraDurationLeft(target, slot, durationFull, Environment.TickCount);
                GetSession().GameState.StoreAuraDurationFull(target, slot, durationFull);
            }
        }

        AuraInfo aura = new AuraInfo();
        aura.Slot = slot;
        aura.AuraData = auraData;

        AuraUpdate update = new AuraUpdate(target, false);
        update.Auras.Add(aura);
        SendPacketToClient(update);
    }
}
