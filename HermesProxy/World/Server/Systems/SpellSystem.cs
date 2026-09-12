using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's spell CMSGs: casting, cancelling, talents, resurrection.
/// </summary>
/// <remarks>
/// <para>
/// Bodies were moved from <c>World/Server/PacketHandlers/SpellHandler.cs</c>, not retyped;
/// <c>verify-handler-port.py</c> diffs each one against the original.
/// </para>
/// <para>
/// Three members here are called from outside this file and moved anyway, because one
/// implementation beats a copy on each side of the migration: <c>SendCastRequestFailed</c> (the
/// legacy item handler abandons a pending cast through it), and <c>ForwardKnownSpellCast</c> and
/// <c>UseInventoryItem</c> (<c>ToyHandler</c>, still on the reflective path). They take the context
/// explicitly; <c>WorldSocket.SessionContext</c> exists so the legacy side can supply one.
/// </para>
/// </remarks>
public static class SpellSystem
{
    // Server category, as in the handler this came from.
    private static readonly Microsoft.Extensions.Logging.ILogger _melSpellServerLog =
        Log.CreateMelLogger(Log.CategoryServer);

    static SpellCastTargetFlags ConvertSpellTargetFlags(SpellTargetData target)
    {
        SpellCastTargetFlags targetFlags = SpellCastTargetFlags.None;
        if (target.Unit != default && !target.Unit.IsEmpty())
        {
            if (target.Flags.HasFlag(SpellCastTargetFlags.Unit))
                targetFlags |= SpellCastTargetFlags.Unit;
            if (target.Flags.HasFlag(SpellCastTargetFlags.CorpseEnemy))
                targetFlags |= SpellCastTargetFlags.CorpseEnemy;
            if (target.Flags.HasFlag(SpellCastTargetFlags.GameObject))
                targetFlags |= SpellCastTargetFlags.GameObject;
            if (target.Flags.HasFlag(SpellCastTargetFlags.CorpseAlly))
                targetFlags |= SpellCastTargetFlags.CorpseAlly;
            if (target.Flags.HasFlag(SpellCastTargetFlags.UnitMinipet))
                targetFlags |= SpellCastTargetFlags.UnitMinipet;
        }
        if (target.Item != default & !target.Item.IsEmpty())
        {
            if (target.Flags.HasFlag(SpellCastTargetFlags.Item))
                targetFlags |= SpellCastTargetFlags.Item;
            if (target.Flags.HasFlag(SpellCastTargetFlags.TradeItem))
                targetFlags |= SpellCastTargetFlags.TradeItem;
        }
        if (target.SrcLocation != null)
            targetFlags |= SpellCastTargetFlags.SourceLocation;
        if (target.DstLocation != null)
            targetFlags |= SpellCastTargetFlags.DestLocation;
        if (!String.IsNullOrEmpty(target.Name))
            targetFlags |= SpellCastTargetFlags.String;
        return targetFlags;
    }

    /// <summary>
    /// Tells the modern client a queued cast failed. Called by the systems here and, through
    /// <see cref="WorldSocket.SessionContext"/>, by the legacy item handler when a pending cast is
    /// abandoned — so it moved with the rest rather than being left behind on the socket.
    /// </summary>
    internal static void SendCastRequestFailed(in SessionContext ctx, ClientCastRequest castRequest, bool isPet)
    {
        if (!castRequest.HasStarted)
        {
            SpellPrepare prepare2 = new SpellPrepare();
            prepare2.ClientCastID = castRequest.ClientGUID;
            prepare2.ServerCastID = castRequest.ServerGUID;
            ctx.SendPacket(prepare2);
        }

        // V3_4_3 RequiresSpellFocus is 123, the same number as Classic SpellInProgress.
        uint inProgress = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261
            ? (uint)SpellCastResultV343.SpellInProgress
            : (uint)SpellCastResultClassic.SpellInProgress;

        if (isPet)
        {
            PetCastFailed failed = new();
            failed.SpellID = castRequest.SpellId;
            failed.Reason = inProgress;
            failed.CastID = castRequest.ServerGUID;
            ctx.SendPacket(failed);
        }
        else
        {
            CastFailed failed = new();
            failed.SpellID = castRequest.SpellId;
            failed.SpellXSpellVisualID = castRequest.SpellXSpellVisualId;
            failed.Reason = inProgress;
            failed.CastID = castRequest.ServerGUID;
            ctx.SendPacket(failed);
        }    
    }

    [HandlesCmsg(Opcode.CMSG_CAST_SPELL)]
    public static void HandleCastSpell(in CastSpell cast, in SessionContext ctx)
    {
        // Create Heirloom (160597). V3_4_3 Collections panel right-click "Add to Bag"
        // casts this spell with Misc[0] = heirloom item ID. Legacy 3.3.5a server has
        // no such spell ("unknown spell id 160597") and silently drops the cast —
        // client then re-spams every frame. Block at proxy: reply with CastFailed
        // so the client stops, and do NOT forward to legacy. Item creation is not
        // bridged (no clean legacy path that doesn't require GM rights).
        if (cast.Cast.SpellID == KnownSpellIds.CreateHeirloom
            && ModernVersion.ExpansionVersion == 3
            && GameData.Heirlooms.Contains((int)cast.Cast.Misc[0]))
        {
            WowGuid128 serverCastId = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, (uint)ctx.GetSession().GameState.CurrentMapId!, cast.Cast.SpellID, cast.Cast.SpellID + ctx.GetSession().GameState.CurrentPlayerGuid.GetCounter());
            ctx.SendPacket(new SpellPrepare { ClientCastID = cast.Cast.CastID, ServerCastID = serverCastId });
            ctx.SendPacket(new CastFailed
            {
                SpellID = cast.Cast.SpellID,
                SpellXSpellVisualID = cast.Cast.SpellXSpellVisualID,
                Reason = (uint)SpellCastResultV343.DontReport,
                CastID = serverCastId,
            });
            return;
        }

        // Lock.dbc row 99 asks for a different lock type on 3.3.5a than the V3_4_3 client's
        // Lock.db2 does, so the client casts an "Opening" the legacy server rejects with
        // SPELL_FAILED_BAD_TARGETS. Substitute the spell the legacy lock actually wants and
        // remember both ids so the SMSG responses still match the queued cast.
        // See GameObjectLockRemap, issue #269.
        uint legacyOpenLockSpellId = ResolveLegacyOpenLockSpell(in ctx, cast.Cast);

        bool isNextMelee = GameData.NextMeleeSpells.Contains(cast.Cast.SpellID);
        bool isAutoRepeat = GameData.AutoRepeatSpells.Contains(cast.Cast.SpellID);

        if (isNextMelee || isAutoRepeat)
        {
            // Next melee and auto repeat spells are tracked separately - they can coexist
            // (e.g., hunter can have Raptor Strike queued AND Auto Shot active)
            ClientCastRequest castRequest = new ClientCastRequest();
            castRequest.Timestamp = Environment.TickCount;
            castRequest.SpellId = cast.Cast.SpellID;
            castRequest.SpellXSpellVisualId = cast.Cast.SpellXSpellVisualID;
            castRequest.ClientGUID = cast.Cast.CastID;

            // Get the appropriate tracking variable based on spell type
            ref ClientCastRequest? currentCast = ref (isAutoRepeat
                ? ref ctx.GetSession().GameState.CurrentClientAutoRepeatCast
                : ref ctx.GetSession().GameState.CurrentClientNextMeleeCast);

            if (currentCast != null)
            {
                // Already have one of this type in progress - reject
                castRequest.ServerGUID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, (uint)ctx.GetSession().GameState.CurrentMapId!, cast.Cast.SpellID, 10000 + cast.Cast.CastID.GetCounter());
                SendCastRequestFailed(in ctx, castRequest, false);
                return;
            }
            else
            {
                castRequest.ServerGUID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, (uint)ctx.GetSession().GameState.CurrentMapId!, cast.Cast.SpellID, cast.Cast.SpellID + ctx.GetSession().GameState.CurrentPlayerGuid.GetCounter());

                SpellPrepare prepare = new SpellPrepare();
                prepare.ClientCastID = cast.Cast.CastID;
                prepare.ServerCastID = castRequest.ServerGUID;
                ctx.SendPacket(prepare);

                currentCast = castRequest;
            }
        }
        else
        {
            // Normal casts - use queue for proper FIFO handling of rapid casts
            ClientCastRequest castRequest = new ClientCastRequest();
            castRequest.Timestamp = Environment.TickCount;
            castRequest.SpellId = cast.Cast.SpellID;
            castRequest.SpellXSpellVisualId = cast.Cast.SpellXSpellVisualID;
            castRequest.ClientGUID = cast.Cast.CastID;
            castRequest.ServerGUID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, (uint)ctx.GetSession().GameState.CurrentMapId!, cast.Cast.SpellID, 10000 + cast.Cast.CastID.GetCounter());

            // Check if there's already a cast in progress - reject without forwarding to server
            // This prevents interrupting the current cast (player gets "Another action is in progress")
            if (ctx.GetSession().GameState.HasStartedNormalCast())
            {
                SendCastRequestFailed(in ctx, castRequest, false);
                return;
            }

            if (legacyOpenLockSpellId != 0)
                castRequest.LegacySpellId = legacyOpenLockSpellId;

            // Enqueue the cast - responses will be matched by SpellId in FIFO order
            ctx.GetSession().GameState.PendingNormalCasts.Enqueue(castRequest);

            // Native 3.4.3 sends SpellPrepare before SpellStart so the client remaps
            // its predicted ClientCastID onto the server CastID. Doing this on CMSG
            // (not after the AC round-trip) keeps the action-bar / cast visual bound.
            ctx.SendPacket(new SpellPrepare
            {
                ClientCastID = castRequest.ClientGUID,
                ServerCastID = castRequest.ServerGUID,
            });
            castRequest.PrepareSent = true;
        }

        SendLegacyCastSpell(in ctx, cast.Cast, legacyOpenLockSpellId != 0 ? legacyOpenLockSpellId : cast.Cast.SpellID);
    }

    [HandlesCmsg(Opcode.CMSG_PET_CAST_SPELL)]
    public static void HandlePetCastSpell(in PetCastSpell cast, in SessionContext ctx)
    {
        // Pet casts - use queue for proper FIFO handling
        ClientCastRequest castRequest = new ClientCastRequest();
        castRequest.Timestamp = Environment.TickCount;
        castRequest.SpellId = cast.Cast.SpellID;
        castRequest.SpellXSpellVisualId = cast.Cast.SpellXSpellVisualID;
        castRequest.ClientGUID = cast.Cast.CastID;
        castRequest.ServerGUID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, (uint)ctx.GetSession().GameState.CurrentMapId!, cast.Cast.SpellID, 10000 + cast.Cast.CastID.GetCounter());

        // Check if there's already a pet cast in progress - reject without forwarding to server
        if (ctx.GetSession().GameState.HasStartedPetCast())
        {
            SendCastRequestFailed(in ctx, castRequest, true);
            return;
        }

        // Enqueue the cast - responses will be matched by SpellId in FIFO order
        ctx.GetSession().GameState.PendingPetCasts.Enqueue(castRequest);

        SpellCastTargetFlags targetFlags = ConvertSpellTargetFlags(cast.Cast.Target);

        WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_CAST_SPELL);
        packet.WriteGuid(cast.PetGUID.To64(ctx.GetSession().GameState));
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.WriteUInt8(0); // cast count
        packet.WriteUInt32(cast.Cast.SpellID);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.WriteUInt8((byte)cast.Cast.SendCastFlags);
        WriteSpellTargets(cast.Cast.Target, targetFlags, packet);
        WriteClientCastFlagsTrailer(cast.Cast.SendCastFlags, cast.Cast.MissileTrajectory, packet);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_USE_ITEM)]
    public static void HandleUseItem(in UseItem use, in SessionContext ctx)
    {
        // Item use - use queue for proper FIFO handling
        ClientCastRequest castRequest = new ClientCastRequest();
        castRequest.Timestamp = Environment.TickCount;
        castRequest.SpellId = use.Cast.SpellID;
        castRequest.SpellXSpellVisualId = use.Cast.SpellXSpellVisualID;
        castRequest.ClientGUID = use.Cast.CastID;
        castRequest.ServerGUID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, (uint)ctx.GetSession().GameState.CurrentMapId!, use.Cast.SpellID, 10000 + use.Cast.CastID.GetCounter());
        castRequest.ItemGUID = use.CastItem;

        // Some items had their on-use spell id renumbered in SoM 1.14.1+ (e.g. Diamond Flask 17626 → 363880).
        // The 1.12 emulator only knows the legacy id, so resolve it now and remember both
        // so SMSG_SPELL_START / SPELL_GO can match the queued cast.
        uint legacySpellId = ctx.GetSession().GameState.GetLegacyItemSpellId(use.CastItem, use.Cast.SpellID);
        if (legacySpellId != 0)
            castRequest.LegacySpellId = legacySpellId;

        // Enqueue the cast - responses will be matched by SpellId (or LegacySpellId) in FIFO order
        ctx.GetSession().GameState.PendingNormalCasts.Enqueue(castRequest);

        WorldPacket packet = new WorldPacket(Opcode.CMSG_USE_ITEM);
        byte containerSlot = use.PackSlot != Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(use.PackSlot) : use.PackSlot;
        byte slot = use.PackSlot == Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustModernInventorySlotToLegacy(use.Slot) : use.Slot;
        uint resolvedSpellId = legacySpellId != 0 ? legacySpellId : use.Cast.SpellID;
        packet.WriteUInt8(containerSlot);
        packet.WriteUInt8(slot);
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            // Vanilla 1.12: bagIndex, slot, spellSlot, targets
            packet.WriteUInt8(ctx.GetSession().GameState.GetItemSpellSlot(use.CastItem, resolvedSpellId));
        }
        else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
        {
            // TBC 2.4.3: bagIndex, slot, spell_count, cast_count, itemGuid, targets
            packet.WriteUInt8(ctx.GetSession().GameState.GetItemSpellSlot(use.CastItem, resolvedSpellId));
            packet.WriteUInt8(0); // cast_count
            packet.WriteGuid(use.CastItem.To64());
        }
        else
        {
            // WotLK 3.3.5a: bagIndex, slot, cast_count, spellId, itemGuid, glyphIndex, cast_flags, targets
            packet.WriteUInt8(0); // cast_count
            packet.WriteUInt32(resolvedSpellId);
            packet.WriteGuid(use.CastItem.To64());
            // Modern V3_4_3 client encodes the target glyph slot in SpellCastRequest.Misc[0]
            // when applying a glyph item (drag-drop onto a slot). Was hardcoded to 0 — every
            // glyph cast went to slot 0 regardless of the dropped slot.
            packet.WriteUInt32(use.Cast.Misc[0]); // glyphIndex
            // Always log item-use casts so we can correlate slot index, spell, and item GUID
            // when investigating glyph-apply rejections (iter-16).
            Log.Print(LogType.Network,
                $"[Glyphs] CMSG_USE_ITEM glyphIndex={use.Cast.Misc[0]} itemSpellID={use.Cast.SpellID} itemGUID={use.CastItem}");
            packet.WriteUInt8((byte)use.Cast.SendCastFlags);
        }
        SpellCastTargetFlags targetFlags = ConvertSpellTargetFlags(use.Cast.Target);
        WriteSpellTargets(use.Cast.Target, targetFlags, packet);
        WriteClientCastFlagsTrailer(use.Cast.SendCastFlags, use.Cast.MissileTrajectory, packet);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_CANCEL_CAST)]
    public static void HandleCancelCast(in CancelCast cast, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_CANCEL_CAST);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.WriteUInt8(0);
        packet.WriteUInt32(cast.SpellID);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_CANCEL_CHANNELLING)]
    public static void HandleCancelChannelling(in CancelChannelling cast, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_CANCEL_CHANNELLING);
        packet.WriteInt32(cast.SpellID);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_CANCEL_AUTO_REPEAT_SPELL)]
    public static void HandleCancelAutoRepeatSpell(in EmptyClientPacket spell, in SessionContext ctx)
    {
        // The client has already dropped auto-repeat, so its next Auto Shot is a new cast.
        // Don't wait for SMSG_CANCEL_AUTO_REPEAT: cMaNGOS WotLK never sends it for a client
        // cancel, and the stale entry made HandleCastSpell reject every later Auto Shot as
        // SpellInProgress (#277).
        ctx.GetSession().GameState.CurrentClientAutoRepeatCast = null;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_CANCEL_AUTO_REPEAT_SPELL);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_CANCEL_AURA)]
    public static void HandleCancelAura(in CancelAura aura, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_CANCEL_AURA);
        packet.WriteUInt32(aura.SpellID);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_LEARN_TALENT)]
    public static void HandleLearnTalent(in LearnTalent talent, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_LEARN_TALENT);
        packet.WriteUInt32(talent.TalentID);
        packet.WriteUInt32(talent.Rank);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_RESURRECT_RESPONSE)]
    public static void HandleResurrectResponse(in ResurrectResponse revive, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_RESURRECT_RESPONSE);
        packet.WriteGuid(revive.CasterGUID.To64());
        packet.WriteUInt8((byte)(revive.Response != 0 ? 0 : 1));
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SELF_RES)]
    public static void HandleSelfRes(in SelfRes revive, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SELF_RES);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_TOTEM_DESTROYED)]
    public static void HandleTotemDestroyed(in TotemDestroyed totem, in SessionContext ctx)
    {
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_TOTEM_DESTROYED);
        packet.WriteUInt8(totem.Slot);
        ctx.SendPacketToServer(packet);
    }

    static void WriteSpellTargets(SpellTargetData target, SpellCastTargetFlags targetFlags, WorldPacket packet)
    {
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            packet.WriteUInt16((ushort)targetFlags);
        else
            packet.WriteUInt32((uint)targetFlags);

        if (targetFlags.HasAnyFlag(SpellCastTargetFlags.Unit | SpellCastTargetFlags.CorpseEnemy | SpellCastTargetFlags.GameObject |
            SpellCastTargetFlags.CorpseAlly | SpellCastTargetFlags.UnitMinipet))
            packet.WritePackedGuid(target.Unit.To64());

        // Check if the user wants to target the "Will not be traded" slot
        if (targetFlags.HasFlag(SpellCastTargetFlags.TradeItem) && target.Item == WowGuid128.Create(HighGuidType703.Uniq, 10))
            packet.WritePackedGuid(new WowGuid64((ulong) TradeSlots.NonTraded));
        else if (targetFlags.HasFlag(SpellCastTargetFlags.Item))
            packet.WritePackedGuid(target.Item.To64());

        if (targetFlags.HasAnyFlag(SpellCastTargetFlags.SourceLocation))
        {
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                packet.WritePackedGuid(target.SrcLocation.Transport.To64());
            packet.WriteVector3(target.SrcLocation.Location);
        }

        if (targetFlags.HasAnyFlag(SpellCastTargetFlags.DestLocation))
        {
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_8_9464))
                packet.WritePackedGuid(target.DstLocation.Transport.To64());
            packet.WriteVector3(target.DstLocation.Location);
        }

        if (targetFlags.HasAnyFlag(SpellCastTargetFlags.String))
            packet.WriteCString(target.Name);
    }

    static uint ResolveLegacyOpenLockSpell(in SessionContext ctx, SpellCastRequest cast)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return 0;
        if (!cast.Target.Flags.HasFlag(SpellCastTargetFlags.GameObject))
            return 0;
        if (cast.Target.Unit.GetHighType() != HighGuidType.GameObject)
            return 0;

        // 0 when the template has not gone past yet, which resolves to "forward unchanged".
        // UpdateHandler.RequestGameObjectLockTemplate asks for it as the object is created,
        // well before the object can be clicked.
        if (!ctx.GetSession().GameState.GoLockIdByEntry.TryGetValue(cast.Target.Unit.GetEntry(), out uint lockId))
            return 0;

        return GameObjectLockRemap.ResolveLegacyOpenLockSpell(cast.SpellID, lockId);
    }

    internal static void ForwardKnownSpellCast(in SessionContext ctx, SpellCastRequest cast, uint serverSpellId)
    {
        ClientCastRequest castRequest = new ClientCastRequest();
        castRequest.Timestamp = Environment.TickCount;
        castRequest.SpellId = cast.SpellID;
        castRequest.SpellXSpellVisualId = cast.SpellXSpellVisualID;
        castRequest.ClientGUID = cast.CastID;
        castRequest.ServerGUID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, (uint)ctx.GetSession().GameState.CurrentMapId!, cast.SpellID, 10000 + cast.CastID.GetCounter());
        if (serverSpellId != 0 && serverSpellId != cast.SpellID)
            castRequest.LegacySpellId = serverSpellId;

        if (ctx.GetSession().GameState.HasStartedNormalCast())
        {
            SendCastRequestFailed(in ctx, castRequest, false);
            return;
        }

        ctx.GetSession().GameState.PendingNormalCasts.Enqueue(castRequest);
        ctx.SendPacket(new SpellPrepare
        {
            ClientCastID = castRequest.ClientGUID,
            ServerCastID = castRequest.ServerGUID,
        });
        castRequest.PrepareSent = true;
        SendLegacyCastSpell(in ctx, cast, serverSpellId != 0 ? serverSpellId : cast.SpellID);
    }

    static void SendLegacyCastSpell(in SessionContext ctx, SpellCastRequest cast, uint spellId)
    {
        SpellCastTargetFlags targetFlags = ConvertSpellTargetFlags(cast.Target);

        // To64() repeats the conversion WriteSpellTargets performs a few lines down, so keep
        // the call behind IsEnabled rather than paying for it on every forwarded cast.
        if (_melSpellServerLog.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Trace))
        {
            World.Logging.SpellLogMessages.LegacyCastForwarded(
                _melSpellServerLog, cast.SpellID, spellId,
                (uint)cast.Target.Flags, (uint)targetFlags, cast.Target.Unit.To64().GetLowValue());
        }

        WorldPacket packet = new WorldPacket(Opcode.CMSG_CAST_SPELL);
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            packet.WriteUInt32(spellId);
        }
        else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
        {
            packet.WriteUInt32(spellId);
            packet.WriteUInt8(0); // cast count
        }
        else
        {
            packet.WriteUInt8(0); // cast count
            packet.WriteUInt32(spellId);
            packet.WriteUInt8((byte)cast.SendCastFlags);
        }
        WriteSpellTargets(cast.Target, targetFlags, packet);
        WriteClientCastFlagsTrailer(cast.SendCastFlags, cast.MissileTrajectory, packet);
        ctx.SendPacketToServer(packet);
    }

    static void WriteClientCastFlagsTrailer(uint castFlags, MissileTrajectoryRequest trajectory, WorldPacket packet)
    {
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
            return;
        if ((castFlags & (uint)CastFlag.HasTrajectory) == 0)
            return;

        packet.WriteFloat(trajectory.Pitch);
        packet.WriteFloat(trajectory.Speed);
        packet.WriteUInt8(0); // hasMovementData — proxy doesn't relay client movement here
    }

    internal static void UseInventoryItem(in SessionContext ctx, WowGuid128 itemGuid, byte containerSlot, byte slot, SpellCastRequest cast)
    {
        ClientCastRequest castRequest = new ClientCastRequest();
        castRequest.Timestamp = Environment.TickCount;
        castRequest.SpellId = cast.SpellID;
        castRequest.SpellXSpellVisualId = cast.SpellXSpellVisualID;
        castRequest.ClientGUID = cast.CastID;
        castRequest.ServerGUID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, (uint)ctx.GetSession().GameState.CurrentMapId!, cast.SpellID, 10000 + cast.CastID.GetCounter());
        castRequest.ItemGUID = itemGuid;

        uint legacySpellId = ctx.GetSession().GameState.GetLegacyItemSpellId(itemGuid, cast.SpellID);
        if (legacySpellId != 0)
            castRequest.LegacySpellId = legacySpellId;

        ctx.GetSession().GameState.PendingNormalCasts.Enqueue(castRequest);

        WorldPacket packet = new WorldPacket(Opcode.CMSG_USE_ITEM);
        packet.WriteUInt8(containerSlot);
        packet.WriteUInt8(slot);
        uint resolvedSpellId = legacySpellId != 0 ? legacySpellId : cast.SpellID;
        packet.WriteUInt8(0);
        packet.WriteUInt32(resolvedSpellId);
        packet.WriteGuid(itemGuid.To64());
        packet.WriteUInt32(0); // glyphIndex — Misc[0] is the toy item id on CMSG_USE_TOY
        packet.WriteUInt8((byte)cast.SendCastFlags);
        SpellCastTargetFlags targetFlags = ConvertSpellTargetFlags(cast.Target);
        WriteSpellTargets(cast.Target, targetFlags, packet);
        WriteClientCastFlagsTrailer(cast.SendCastFlags, cast.MissileTrajectory, packet);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_CANCEL_MOUNT_AURA)]
    public static void HandleCancelMountAura(in EmptyClientPacket cancel, in SessionContext ctx)
    {
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_CANCEL_MOUNT_AURA);
            ctx.SendPacketToServer(packet);
        }
        else
        {
            WowGuid128 guid = ctx.GetSession().GameState.CurrentPlayerGuid;
            var updateFields = ctx.GetSession().GameState.GetCachedObjectFieldsLegacy(guid);
            if (updateFields == null)
                return;

            for (byte i = 0; i < 32; i++)
            {
                var aura = ctx.GetSession().WorldClient!.ReadAuraSlot(i, guid, updateFields);
                if (aura == null)
                    continue;

                if (GameData.MountAuras.Contains(aura.SpellID))
                {
                    WorldPacket packet = new WorldPacket(Opcode.CMSG_CANCEL_AURA);
                    packet.WriteUInt32(aura.SpellID);
                    ctx.SendPacketToServer(packet);
                }
            }
        }
    }
}
