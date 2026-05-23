// Nullable disabled file-wide because the ported WriteUpdate*Data methods (lifted
// verbatim from the HermesProxy-WOTLK fork, which compiles with nullable disabled)
// access nullable struct/reference members without `.Value` / null-forgiving annotation.
// The pre-port WriteCreate*Data code in this file was nullable-safe, but the ported
// update path would otherwise produce ~400 nullability warnings (which are promoted
// to errors by Directory.Packages.props `WarningsAsErrors=nullable`). Disabling
// nullable here keeps the port mechanical; future cleanup can reintroduce per-section
// nullable contexts once the update writers stabilise.
#nullable disable

using Framework.GameMath;
using Framework.Util;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Objects.Version.V3_4_3_54261;

// Phase 5a hand-port of the WotLK Classic 3.4.3 descriptor-tree serializer.
// Phases 5b–5e progressively replace sections with source-generator output,
// using this hand-port as the byte-equivalence test oracle.
public partial class ObjectUpdateBuilder
{
    private readonly ObjectUpdate _updateData;
    private readonly GameSessionData _gameState;
    private readonly ObjectTypeBCC _objectType;
    private readonly ObjectTypeBCC _realObjectType;
    private readonly ObjectTypeMask _objectTypeMask;
    private CreateObjectBits _createBits;

    public ObjectUpdateBuilder(ObjectUpdate updateData, GameSessionData gameState)
    {
        _updateData = updateData;
        _gameState = gameState;

        var objectType = updateData.Guid.GetObjectType();
        if (updateData.CreateData != null)
        {
            objectType = updateData.CreateData.ObjectType;
            if (updateData.CreateData.ThisIsYou)
                objectType = ObjectType.ActivePlayer;
        }
        else if (_gameState.OriginalObjectTypes.TryGetValue(updateData.Guid, out var cachedType))
        {
            // Values updates: GUID-derived type can't distinguish Container vs Item
            // (both share HighGuid.Item) or ActivePlayer vs Player. Prefer the type
            // captured on CreateObject so Container Values updates correctly include
            // the Container bit in _objectTypeMask and route to WriteUpdateContainerData.
            objectType = cachedType;
        }
        if (objectType == ObjectType.Player && _gameState.CurrentPlayerGuid == updateData.Guid)
            objectType = ObjectType.ActivePlayer;

        _objectType = ObjectTypeConverter.ConvertToBCC(objectType);
        _realObjectType = _objectType;
        _objectTypeMask = ObjectTypeMask.Object;
        switch (_objectType)
        {
            case ObjectTypeBCC.Item:          _objectTypeMask |= ObjectTypeMask.Item; break;
            case ObjectTypeBCC.Container:     _objectTypeMask |= ObjectTypeMask.Item | ObjectTypeMask.Container; break;
            case ObjectTypeBCC.Unit:          _objectTypeMask |= ObjectTypeMask.Unit; break;
            case ObjectTypeBCC.Player:        _objectTypeMask |= ObjectTypeMask.Unit | ObjectTypeMask.Player; break;
            case ObjectTypeBCC.ActivePlayer:  _objectTypeMask |= ObjectTypeMask.Unit | ObjectTypeMask.Player | ObjectTypeMask.ActivePlayer; break;
            case ObjectTypeBCC.GameObject:    _objectTypeMask |= ObjectTypeMask.GameObject; break;
            case ObjectTypeBCC.DynamicObject: _objectTypeMask |= ObjectTypeMask.DynamicObject; break;
            case ObjectTypeBCC.Corpse:        _objectTypeMask |= ObjectTypeMask.Corpse; break;
        }
    }

    private bool IsOwner =>
        _realObjectType == ObjectTypeBCC.ActivePlayer ||
        _realObjectType == ObjectTypeBCC.Item ||
        _realObjectType == ObjectTypeBCC.Container;

    private bool IsGameObjectOwner
    {
        get
        {
            if (_realObjectType != ObjectTypeBCC.GameObject)
                return false;
            var createdBy = _updateData.GameObjectData?.CreatedBy;
            if (createdBy is null)
                return false;
            var playerGuid = _gameState.CurrentPlayerGuid;
            return createdBy.Value.GetCounter() == playerGuid.GetCounter() &&
                   createdBy.Value.GetHighType() == playerGuid.GetHighType();
        }
    }

    // Wire-format type mask sent in the SMSG_UPDATE_OBJECT header. The bit
    // positions here are the protocol values, not the in-memory ObjectTypeMask
    // enum values — they intentionally differ.
    private static uint ConvertTypeMask(ObjectTypeMask mask)
    {
        uint result = 0;
        if (mask.HasAnyFlag(ObjectTypeMask.Object))        result |= 0x0001;
        if (mask.HasAnyFlag(ObjectTypeMask.Item))          result |= 0x0002;
        if (mask.HasAnyFlag(ObjectTypeMask.Container))     result |= 0x0004;
        if (mask.HasAnyFlag(ObjectTypeMask.Unit))          result |= 0x0020;
        if (mask.HasAnyFlag(ObjectTypeMask.Player))        result |= 0x0040;
        if (mask.HasAnyFlag(ObjectTypeMask.ActivePlayer))  result |= 0x0080;
        if (mask.HasAnyFlag(ObjectTypeMask.GameObject))    result |= 0x0100;
        if (mask.HasAnyFlag(ObjectTypeMask.DynamicObject)) result |= 0x0200;
        if (mask.HasAnyFlag(ObjectTypeMask.Corpse))        result |= 0x0400;
        if (mask.HasAnyFlag(ObjectTypeMask.AreaTrigger))   result |= 0x0800;
        if (mask.HasAnyFlag(ObjectTypeMask.Sceneobject))   result |= 0x1000;
        if (mask.HasAnyFlag(ObjectTypeMask.Conversation))  result |= 0x2000;
        return result;
    }

    private static byte ConvertTypeId(ObjectTypeBCC type) => type switch
    {
        ObjectTypeBCC.Object        => 0,
        ObjectTypeBCC.Item          => 1,
        ObjectTypeBCC.Container     => 2,
        ObjectTypeBCC.Unit          => 5,
        ObjectTypeBCC.Player        => 6,
        ObjectTypeBCC.ActivePlayer  => 7,
        ObjectTypeBCC.GameObject    => 8,
        ObjectTypeBCC.DynamicObject => 9,
        ObjectTypeBCC.Corpse        => 10,
        ObjectTypeBCC.AreaTrigger   => 11,
        ObjectTypeBCC.SceneObject   => 12,
        _                           => 0,
    };

    private void SetCreateObjectBits()
    {
        _createBits = CreateObjectBits.None;
        var create = _updateData.CreateData;
        var moveInfo = create?.MoveInfo;
        var hasMoveInfo = moveInfo != null;

        if (hasMoveInfo && moveInfo!.Hover)
            _createBits |= CreateObjectBits.PlayHoverAnim;
        if (hasMoveInfo && _objectTypeMask.HasAnyFlag(ObjectTypeMask.Unit))
            _createBits |= CreateObjectBits.MovementUpdate;
        if (hasMoveInfo && moveInfo!.TransportGuid != default && _objectType == ObjectTypeBCC.GameObject)
            _createBits |= CreateObjectBits.MovementTransport;
        if (hasMoveInfo && !_objectTypeMask.HasAnyFlag(ObjectTypeMask.Unit))
            _createBits |= CreateObjectBits.Stationary;
        if (hasMoveInfo && (_updateData.Guid.GetHighType() == HighGuidType.Transport || _updateData.Guid.GetHighType() == HighGuidType.MOTransport))
            _createBits |= CreateObjectBits.ServerTime;
        if (create != null && create.AutoAttackVictim != null)
            _createBits |= CreateObjectBits.CombatVictim;
        if (hasMoveInfo && moveInfo!.VehicleId != 0)
            _createBits |= CreateObjectBits.Vehicle;
        if (hasMoveInfo && _objectType == ObjectTypeBCC.GameObject)
            _createBits |= CreateObjectBits.Rotation;
        // CreateObjectBits.GameObject is NOT set unconditionally here — empirical
        // capture vs CypherCore (canonical V3_4_3 server) shows the bit must stay
        // false for typical GameObjects (transports, mailboxes, doodads). It writes
        // an extra 4-byte uint32 + 1-bit (WorldEffectID + bit8) section that the
        // V3_4_3 client doesn't read for ordinary GameObjects, causing byte
        // misalignment and CMSG_OBJECT_UPDATE_FAILED. CypherCore only sets it when
        // m_goTemplateAddon.WorldEffectID != 0 (rare special GOs with world-effect
        // overlays). HermesProxy doesn't track WorldEffectID, so we never set it.
        // If a GO surfaces that needs the bit, add a targeted check here.
        if (_objectType == ObjectTypeBCC.ActivePlayer)
            _createBits |= CreateObjectBits.ActivePlayer | CreateObjectBits.ThisIsYou;
    }

    private bool Has(CreateObjectBits flag) => (_createBits & flag) != 0;

    private void BuildMovementUpdate(WorldPacket data)
    {
        const int PauseTimesCount = 0;

        _createBits.WriteCreateBits(data);

        if (Has(CreateObjectBits.MovementUpdate))
        {
            var moveInfo = _updateData.CreateData.MoveInfo;
            var hasSpline = _updateData.CreateData.MoveSpline != null;

            moveInfo.WriteMovementInfoModern(data, _updateData.Guid);

            data.WriteFloat(moveInfo.WalkSpeed);
            data.WriteFloat(moveInfo.RunSpeed);
            data.WriteFloat(moveInfo.RunBackSpeed);
            data.WriteFloat(moveInfo.SwimSpeed);
            data.WriteFloat(moveInfo.SwimBackSpeed);
            data.WriteFloat(moveInfo.FlightSpeed);
            data.WriteFloat(moveInfo.FlightBackSpeed);
            data.WriteFloat(moveInfo.TurnRate);
            data.WriteFloat(moveInfo.PitchRate);
            data.WriteUInt32(0u);
            data.WriteFloat(1f);
            data.WriteFloat(2f);
            data.WriteFloat(65f);
            data.WriteFloat(1f);
            data.WriteFloat(3f);
            data.WriteFloat(10f);
            data.WriteFloat(100f);
            data.WriteFloat(90f);
            data.WriteFloat(140f);
            data.WriteFloat(180f);
            data.WriteFloat(360f);
            data.WriteFloat(90f);
            data.WriteFloat(270f);
            data.WriteFloat(30f);
            data.WriteFloat(80f);
            data.WriteFloat(2.75f);
            data.WriteFloat(7f);
            data.WriteFloat(0.4f);
            data.WriteBit(hasSpline);
            data.FlushBits();
            if (hasSpline)
                WriteCreateObjectSplineDataBlock(_updateData.CreateData.MoveSpline!, data);
        }

        data.WriteInt32(PauseTimesCount);

        if (Has(CreateObjectBits.Stationary))
        {
            data.WriteFloat(_updateData.CreateData.MoveInfo.Position.X);
            data.WriteFloat(_updateData.CreateData.MoveInfo.Position.Y);
            data.WriteFloat(_updateData.CreateData.MoveInfo.Position.Z);
            data.WriteFloat(_updateData.CreateData.MoveInfo.Orientation);
        }

        if (Has(CreateObjectBits.CombatVictim))
            data.WritePackedGuid128(_updateData.CreateData.AutoAttackVictim!.Value);

        if (Has(CreateObjectBits.ServerTime))
        {
            // TC343 writes GameTime::GetGameTimeMS() = server uptime in ms.
            // Legacy 3.3.5a sends PathProgress (transport-specific counter), NOT game time.
            // The 3.4.3 client expects server uptime for transport animation sync.
            data.WriteUInt32((uint)Environment.TickCount);
        }

        if (Has(CreateObjectBits.Vehicle))
        {
            data.WriteUInt32(_updateData.CreateData.MoveInfo.VehicleId);
            data.WriteFloat(_updateData.CreateData.MoveInfo.VehicleOrientation);
        }

        if (Has(CreateObjectBits.AnimKit))
        {
            data.WriteUInt16(0);
            data.WriteUInt16(0);
            data.WriteUInt16(0);
        }

        if (Has(CreateObjectBits.Rotation))
            data.WriteInt64(_updateData.CreateData.MoveInfo.Rotation.GetPackedRotation());

        for (int i = 0; i < PauseTimesCount; i++)
            data.WriteUInt32(0u);

        if (Has(CreateObjectBits.MovementTransport))
            _updateData.CreateData.MoveInfo.WriteTransportInfoModern(data);

        if (Has(CreateObjectBits.GameObject))
        {
            data.WriteUInt32(0u);
            data.WriteBit(false);
            data.FlushBits();
        }

        if (Has(CreateObjectBits.ActivePlayer))
        {
            const bool hasSceneInstanceIDs = false;
            // RuneState is allocated for V3_4_3 DK sessions in CharacterHandler.HandlePlayerLogin.
            // The 3.3.5 server overwrites the seeded "all usable" defaults with authoritative
            // values via SMSG_RESYNC_RUNES; without this block (was hardcoded false), the V3_4_3
            // client starts up believing all 6 runes are on cooldown and refuses every rune-cost cast.
            bool hasRuneState = _gameState.RuneState != null;
            const bool hasActionButtons = true;
            data.WriteBit(hasSceneInstanceIDs);
            data.WriteBit(hasRuneState);
            data.WriteBit(hasActionButtons);
            data.FlushBits();
            if (hasRuneState)
            {
                var runeState = _gameState.RuneState;
                data.WriteUInt8(runeState.RechargingRuneMask);
                data.WriteUInt8(runeState.UsableRuneMask);
                data.WriteUInt32(RuneStateData.MaxRunes);
                for (int i = 0; i < RuneStateData.MaxRunes; i++)
                    data.WriteUInt8(runeState.Cooldowns[i]);
            }
            // Embedded ActivePlayer.ActionButtons — 180 × int32 (legacy packed action+type).
            // V3_4_3 client uses this as the authoritative bar state; the standalone
            // SMSG_UPDATE_ACTION_BUTTONS only carries updates relative to it. Writing zeros
            // here leaves every slot blank regardless of the standalone packet that follows.
            for (int j = 0; j < PlayerConst.MaxActionButtonsModern; j++)
            {
                int legacy = j < _gameState.ActionButtons.Count ? _gameState.ActionButtons[j] : 0;
                data.WriteInt32(legacy);
            }
        }
    }

    private static void WriteCreateObjectSplineDataBlock(ServerSideMovement moveSpline, WorldPacket data)
    {
        data.WriteUInt32(moveSpline.SplineId);

        if (!moveSpline.SplineFlags.HasAnyFlag(SplineFlagModern.Cyclic))
            data.WriteVector3(moveSpline.EndPosition);
        else
            data.WriteVector3(Vector3.Zero);

        var hasSplineMove = data.WriteBit(moveSpline.SplineCount != 0);
        data.FlushBits();
        if (!hasSplineMove)
            return;

        data.WriteUInt32((uint)moveSpline.SplineFlags);
        data.WriteUInt32(moveSpline.SplineTime);
        data.WriteUInt32(moveSpline.SplineTimeFull);
        data.WriteFloat(1f);
        data.WriteFloat(1f);
        data.WriteBits((byte)moveSpline.SplineType, 2);
        var hasFadeObjectTime = data.WriteBit(false);
        data.WriteBits(moveSpline.SplineCount, 16);
        data.WriteBit(false);
        data.WriteBit(false);
        data.WriteBit(false);
        data.WriteBit(false);
        data.FlushBits();

        switch (moveSpline.SplineType)
        {
            case SplineTypeModern.FacingSpot:
                data.WriteVector3(moveSpline.FinalFacingSpot);
                break;
            case SplineTypeModern.FacingTarget:
                // Modern wire layout: PackedGuid128 only — no leading orientation float.
                // The earlier extra WriteFloat shifted SplinePoints/PauseTimesCount by
                // 4 bytes, the client read garbage PauseTimesCount (~2.8 GiB array
                // alloc), froze, then sent CMSG_LOG_DISCONNECT reason=7. Triggered
                // mid-combat whenever a creature spawning into LoS had a FacingTarget
                // spline (TC AI casting/aiming at the player). Matches V2_5_3 / V1_14
                // writers.
                data.WritePackedGuid128(moveSpline.FinalFacingGuid);
                break;
            case SplineTypeModern.FacingAngle:
                data.WriteFloat(moveSpline.FinalOrientation);
                break;
        }

        if (hasFadeObjectTime)
            data.WriteInt32(0);

        foreach (var vec in moveSpline.SplinePoints)
            data.WriteVector3(vec);
    }

    // WriteCreateObjectData emitted by HermesProxy.SourceGen.ObjectUpdateBuilderGenerator
    // from [DescriptorCreateField] attributes on V3_4_3_54261.ObjectField enum members.
    // See HermesProxy/obj/Generated/HermesProxy.SourceGen/.../V3_4_3_54261.ObjectUpdateBuilder.g.cs

    // WriteCreateItemData + WriteUpdateItemData + HasAnyItemFieldSet emitted by
    // HermesProxy.SourceGen.ObjectUpdateBuilderGenerator from
    // V3_4_3_54261.ItemField. WriteEmptyItemCreate was equivalent to
    // WriteCreateItemData(new ItemData()) — generator subsumes both.
    //
    // Custom writers for the per-element ItemEnchantment[13] nested-struct payload.
    // Referenced by [DescriptorCreateField(CustomWriter = "WriteEnchantmentCreate")]
    // and [DescriptorUpdateField(CustomWriter = "WriteEnchantmentUpdate")] on
    // ITEM_ENCHANTMENT in V3_4_3_54261.ItemField.
    internal void WriteEnchantmentCreate(WorldPacket data, ItemEnchantment[] arr, int i)
    {
        var ench = arr[i];
        if (ench != null)
        {
            data.WriteInt32(ench.ID.GetValueOrDefault());
            data.WriteUInt32(ench.Duration.GetValueOrDefault());
            data.WriteInt16((short)ench.Charges.GetValueOrDefault());
            data.WriteUInt16(ench.Inactive.GetValueOrDefault());
        }
        else
        {
            data.WriteInt32(0);
            data.WriteUInt32(0u);
            data.WriteInt16(0);
            data.WriteUInt16(0);
        }
    }

    internal void WriteEnchantmentUpdate(WorldPacket data, ItemEnchantment[] arr, int i)
    {
        var ench = arr[i];   // guaranteed non-null — caller gates on element bit
        uint enchMask = 0;
        if (ench.ID.HasValue) enchMask |= 2;
        if (ench.Duration.HasValue) enchMask |= 4;
        if (ench.Charges.HasValue) enchMask |= 8;
        if (enchMask != 0) enchMask |= 1;
        data.WriteBits(enchMask, 4);
        data.FlushBits();
        if (ench.ID.HasValue) data.WriteInt32(ench.ID.Value);
        if (ench.Duration.HasValue) data.WriteUInt32(ench.Duration.Value);
        if (ench.Charges.HasValue) data.WriteUInt16(ench.Charges.Value);
    }

    // WriteCreateContainerData emitted by HermesProxy.SourceGen.ObjectUpdateBuilderGenerator
    // from V3_4_3_54261.ContainerField.

    // -----------------------------------------------------------------------------------
    // Unit Create custom writers — referenced by V3_4_3_54261.UnitField placeholder
    // members with CustomWriter set. Each method takes (WorldPacket data, UnitData src)
    // matching the generator's emitted call shape. IsOwner gating is applied by the
    // generator before invocation when the placeholder declares OwnerOnly = true.
    // -----------------------------------------------------------------------------------

    internal void WriteCreateUnitRaceId(WorldPacket data, UnitData src)
    {
        bool zeroCharBakeIds = IsImpersonatingCreatureBake();
        data.WriteUInt8(zeroCharBakeIds ? (byte)0 : src.RaceId.GetValueOrDefault());
    }

    internal void WriteCreateUnitClassId(WorldPacket data, UnitData src)
    {
        bool zeroCharBakeIds = IsImpersonatingCreatureBake();
        data.WriteUInt8(zeroCharBakeIds ? (byte)0 : src.ClassId.GetValueOrDefault());
    }

    internal void WriteCreateUnitSexId(WorldPacket data, UnitData src)
    {
        bool zeroCharBakeIds = IsImpersonatingCreatureBake();
        data.WriteUInt8(zeroCharBakeIds ? (byte)0 : src.SexId.GetValueOrDefault());
    }

    internal void WriteCreateUnitChannelDataInline(WorldPacket data, UnitData src)
    {
        data.WriteInt32(src.ChannelData?.SpellID ?? 0);
        data.WriteInt32(src.ChannelData?.SpellXSpellVisualID ?? 0);
    }

    internal void WriteCreateUnitOwnerFloatPairs(WorldPacket data, UnitData src)
    {
        // IF IsOwner: 10× (Float 0, Float 0). Generator wraps the call in if (IsOwner).
        for (int j = 0; j < 10; j++)
        {
            data.WriteFloat(0f);
            data.WriteFloat(0f);
        }
    }

    internal void WriteCreateUnitPowerInterleaved(WorldPacket data, UnitData src)
    {
        // 10 iterations × (Power[k] if k<7 else 0, MaxPower[k] if k<7 else 0, Float 0)
        for (int k = 0; k < 10; k++)
        {
            data.WriteInt32(k < 7 ? src.Power[k].GetValueOrDefault() : 0);
            data.WriteInt32(k < 7 ? src.MaxPower[k].GetValueOrDefault() : 0);
            data.WriteFloat(0f);
        }
    }

    internal void WriteCreateUnitEffectiveLevel(WorldPacket data, UnitData src)
    {
        data.WriteInt32(src.EffectiveLevel ?? src.Level.GetValueOrDefault());
    }

    internal void WriteCreateUnitVirtualItems(WorldPacket data, UnitData src)
    {
        for (int l = 0; l < 3; l++)
        {
            int vItemId = src.VirtualItems != null && src.VirtualItems[l] is VisibleItem vi ? vi.ItemID : 0;
            // Players don't populate VirtualItems server-side (use PLAYER_VISIBLE_ITEM
            // descriptors instead). For the local player, fall back to PlayerData.VisibleItems:
            // slot 0=mainhand(15), 1=offhand(16), 2=ranged(17).
            if (vItemId == 0 && IsOwner && _updateData.PlayerData?.VisibleItems != null)
            {
                int playerSlot = 15 + l;
                if (playerSlot < _updateData.PlayerData.VisibleItems.Length
                    && _updateData.PlayerData.VisibleItems[playerSlot] is VisibleItem pv && pv.ItemID != 0)
                {
                    vItemId = pv.ItemID;
                }
            }
            data.WriteInt32(vItemId);
            data.WriteUInt16(0);
            data.WriteUInt16(0);
        }
    }

    internal void WriteCreateUnitFlags2Sanitized(WorldPacket data, UnitData src)
    {
        data.WriteUInt32(SanitizeFlags2(src.Flags2.GetValueOrDefault(), src.CreatedBy));
    }

    internal void WriteCreateUnitRangedAttackTime(WorldPacket data, UnitData src)
    {
        // IF IsOwner: bow-default fallback. Generator wraps in if (IsOwner) already.
        uint rangedTime = src.RangedAttackRoundBaseTime.GetValueOrDefault();
        if (rangedTime == 0 && _updateData.PlayerData?.VisibleItems != null
            && _updateData.PlayerData.VisibleItems.Length > 17
            && _updateData.PlayerData.VisibleItems[17] is VisibleItem ranged && ranged.ItemID != 0)
        {
            rangedTime = 2300;
        }
        data.WriteUInt32(rangedTime);
    }

    internal void WriteCreateUnitStatsInterleaved(WorldPacket data, UnitData src)
    {
        // IF IsOwner: 5 slots × (Stats[n], StatPosBuff[n], StatNegBuff[n]).
        for (int n = 0; n < 5; n++)
        {
            data.WriteInt32(src.Stats?[n].GetValueOrDefault() ?? 0);
            data.WriteInt32(src.StatPosBuff?[n].GetValueOrDefault() ?? 0);
            data.WriteInt32(src.StatNegBuff?[n].GetValueOrDefault() ?? 0);
        }
    }

    internal void WriteCreateUnitResistances(WorldPacket data, UnitData src)
    {
        // IF IsOwner: 7× Resistances Int32.
        for (int r = 0; r < 7; r++)
            data.WriteInt32(src.Resistances?[r].GetValueOrDefault() ?? 0);
    }

    internal void WriteCreateUnitPowerCostInterleaved(WorldPacket data, UnitData src)
    {
        // IF IsOwner: 7 slots × (PowerCostModifier[p] Int32, PowerCostMultiplier[p] Float).
        for (int p = 0; p < 7; p++)
        {
            data.WriteInt32(src.PowerCostModifier?[p].GetValueOrDefault() ?? 0);
            data.WriteFloat(src.PowerCostMultiplier?[p].GetValueOrDefault() ?? 0f);
        }
    }

    internal void WriteCreateUnitResistanceBuffModsInterleaved(WorldPacket data, UnitData src)
    {
        // 7 slots × (ResistanceBuffModsPositive[b] Int32, ResistanceBuffModsNegative[b] Int32).
        for (int b = 0; b < 7; b++)
        {
            data.WriteInt32(src.ResistanceBuffModsPositive?[b].GetValueOrDefault() ?? 0);
            data.WriteInt32(src.ResistanceBuffModsNegative?[b].GetValueOrDefault() ?? 0);
        }
    }

    internal void WriteCreateUnitChannelObjectsCount(WorldPacket data, UnitData src)
    {
        bool hasChannelObject = src.ChannelObject.HasValue && !src.ChannelObject.Value.IsEmpty();
        data.WriteUInt32(hasChannelObject ? 1u : 0u);
    }

    internal void WriteCreateUnitChannelObjectsBody(WorldPacket data, UnitData src)
    {
        if (src.ChannelObject.HasValue && !src.ChannelObject.Value.IsEmpty())
            data.WritePackedGuid128(src.ChannelObject.Value);
    }

    // -----------------------------------------------------------------------------------
    // Unit Update custom writers — referenced by V3_4_3_54261.UnitField update-side
    // attributes. Scalar CustomWriter sig: (WorldPacket, UnitData). Synthetic group +
    // mask-preamble sig: (WorldPacket, ref StackBitMask, UnitData). Per-element array
    // CustomWriter sig: (WorldPacket, VisibleItem[], int).
    // -----------------------------------------------------------------------------------

    internal void WriteUpdateUnitFlags2(WorldPacket data, UnitData src)
    {
        data.WriteUInt32(SanitizeFlags2(src.Flags2.Value, src.CreatedBy));
    }

    internal void WriteUpdateUnitChannelDataInline(WorldPacket data, UnitData src)
    {
        // ChannelData composite: 2× Int32 (SpellID + SpellXSpellVisualID). No inner mask,
        // no FlushBits — TC UpdateFields.cs UnitChannel.WriteUpdate writes raw fields.
        // The pre-Phase-5b bug history (file:2112-2127 pre-delete) was an erroneous
        // inner-mask emit that shifted SpellID by 1 byte.
        data.WriteInt32(src.ChannelData.Value.SpellID);
        data.WriteInt32(src.ChannelData.Value.SpellXSpellVisualID);
    }

    internal void WriteUpdateUnitChannelObjectsMaskPreamble(WorldPacket data, ref Framework.Util.StackBitMask blocks, UnitData src)
    {
        // DynamicUpdateField preamble for ChannelObjects: 32-bit size + per-element bitmask.
        // Runs between blocks-mask prefix write and FlushBits (bit-aligned to the prefix,
        // not byte-aligned with field payload).
        uint channelObjectsSize = (src.ChannelObject.HasValue && !src.ChannelObject.Value.IsEmpty()) ? 1u : 0u;
        data.WriteBits(channelObjectsSize, 32);
        if (channelObjectsSize != 0)
            data.WriteBits(0xFFFFFFFFu, (int)channelObjectsSize);
    }

    internal void WriteUpdateUnitChannelObjectsBody(WorldPacket data, UnitData src)
    {
        data.WritePackedGuid128(src.ChannelObject.Value);
    }

    internal void WriteUpdateUnitVirtualItem(WorldPacket data, System.Nullable<VisibleItem>[] arr, int i)
    {
        // VirtualItem inner mask: 4-bit (bit 0 = group, 1 = ItemID present). Hand-port
        // (file:2308-2316 pre-delete) emits mask 0x03 then Int32 ItemID.
        data.WriteBits(0x03u, 4);
        data.FlushBits();
        var vItem = arr[i];
        data.WriteInt32(vItem.HasValue ? vItem.Value.ItemID : 0);
    }

    internal void WriteUpdateUnitPowerGroup(WorldPacket data, ref Framework.Util.StackBitMask blocks, UnitData src)
    {
        int maxLen = 7;
        if (src.Power != null && src.Power.Length > maxLen) maxLen = src.Power.Length;
        if (src.MaxPower != null && src.MaxPower.Length > maxLen) maxLen = src.MaxPower.Length;
        for (int pi = 0; pi < maxLen; pi++)
        {
            if (src.Power != null && pi < src.Power.Length && src.Power[pi].HasValue)
                data.WriteInt32(src.Power[pi].Value);
            if (src.MaxPower != null && pi < src.MaxPower.Length && src.MaxPower[pi].HasValue)
                data.WriteInt32(src.MaxPower[pi].Value);
            if (src.ModPowerRegen != null && pi < src.ModPowerRegen.Length && src.ModPowerRegen[pi].HasValue)
                data.WriteFloat(src.ModPowerRegen[pi].Value);
        }
    }

    internal void WriteUpdateUnitStatsGroup(WorldPacket data, ref Framework.Util.StackBitMask blocks, UnitData src)
    {
        for (int i = 0; i < 5; i++)
        {
            if (src.Stats != null && src.Stats[i].HasValue) data.WriteInt32(src.Stats[i].Value);
            if (src.StatPosBuff != null && src.StatPosBuff[i].HasValue) data.WriteInt32(src.StatPosBuff[i].Value);
            if (src.StatNegBuff != null && src.StatNegBuff[i].HasValue) data.WriteInt32(src.StatNegBuff[i].Value);
        }
    }

    internal void WriteUpdateUnitResistancesGroup(WorldPacket data, ref Framework.Util.StackBitMask blocks, UnitData src)
    {
        for (int i = 0; i < 7; i++)
        {
            if (src.Resistances != null && src.Resistances[i].HasValue) data.WriteInt32(src.Resistances[i].Value);
            if (src.PowerCostModifier != null && src.PowerCostModifier[i].HasValue) data.WriteInt32(src.PowerCostModifier[i].Value);
            if (src.PowerCostMultiplier != null && src.PowerCostMultiplier[i].HasValue) data.WriteFloat(src.PowerCostMultiplier[i].Value);
        }
    }

    internal void WriteUpdateUnitResistanceBuffModsGroup(WorldPacket data, ref Framework.Util.StackBitMask blocks, UnitData src)
    {
        for (int i = 0; i < 7; i++)
        {
            if (src.ResistanceBuffModsPositive != null && src.ResistanceBuffModsPositive[i].HasValue) data.WriteInt32(src.ResistanceBuffModsPositive[i].Value);
            if (src.ResistanceBuffModsNegative != null && src.ResistanceBuffModsNegative[i].HasValue) data.WriteInt32(src.ResistanceBuffModsNegative[i].Value);
        }
    }

    // NPCBot / Playerbot frameworks (e.g. trickerer/Trinity-Bots) stamp UNIT_FLAG2_CLONED
    // (0x10) on every bot creature and populate CreatedBy with the owning player's GUID.
    // The V3_4_3 client treats CLONED as "render the appearance of CreatedBy"; when
    // CreatedBy is the local player, the client refuses to render anything (it won't
    // self-clone), which is why a hired bot is invisible until its mount mesh attaches
    // and the mount renderer takes over independently of CLONED.
    //
    // Strip the bit unconditionally for any Creature-typed object on V3_4_3 so the bot
    // renders via its real DisplayID. Real Mage Mirror Image (creature entry 31216) also
    // sets CLONED; it relies on SPELL_AURA_MIRROR_IMAGE (effect 218) for the actual
    // appearance copy, so stripping the flag should leave the clones rendering via their
    // own (already mage-shaped) DisplayID instead of invisible. Revisit if Mirror Image
    // visuals regress.
    private const uint UNIT_FLAG2_CLONED = 0x00000010;
    private uint SanitizeFlags2(uint flags2, WowGuid128? createdBy)
    {
        if ((flags2 & UNIT_FLAG2_CLONED) == 0)
            return flags2;

        bool isCreature = _updateData.Guid.GetHighType() == HighGuidType.Creature;
        uint outFlags = isCreature ? (flags2 & ~UNIT_FLAG2_CLONED) : flags2;

        if (Framework.Logging.Log.IsTraceEnabled)
        {
            Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
                $"[SanitizeFlags2] guid={_updateData.Guid} highType={_updateData.Guid.GetHighType()} in=0x{flags2:X8} out=0x{outFlags:X8} stripped={isCreature} createdBy={(createdBy?.ToString() ?? "null")}");
        }
        return outFlags;
    }

    // NPCBot/Playerbot creatures wear a player-race DisplayID (e.g. Draenei Male = 17247)
    // AND set RaceId/ClassId/SexId on the creature so internal AI works. The V3_4_3 client
    // appears to interpret a creature carrying Race/Class/Sex as a modern-bake character
    // and tries to look up ChrCustomization records the proxy never forwards — the model
    // loads but textures don't bake, leaving the bot rendered as a flat-white silhouette.
    // Real NPCs that share these DisplayIDs (e.g. Velen) leave Race=Class=Sex=0 and render
    // textured via the CreatureDisplayInfoExtra bake path.
    //
    // Detection signal: Creature-typed GUID + CLONED bit on Flags2 (NPCBot stamps it on
    // every bot creature). Zero Race/Class/Sex on the wire for those objects so the client
    // takes the legacy bake path.
    private bool IsImpersonatingCreatureBake()
    {
        if (_updateData.Guid.GetHighType() != HighGuidType.Creature) return false;
        var unit = _updateData.UnitData;
        if (unit == null || !unit.Flags2.HasValue) return false;
        return (unit.Flags2.Value & UNIT_FLAG2_CLONED) != 0;
    }

    // WriteCreateUnitData emitted by HermesProxy.SourceGen.ObjectUpdateBuilderGenerator from V3_4_3_54261.UnitField.
    // WriteCreatePlayerData emitted by HermesProxy.SourceGen.ObjectUpdateBuilderGenerator from V3_4_3_54261.PlayerField.
    // WriteEmptyQuestLog removed — was unused; QuestLog Create handled by WriteCreatePlayerQuestLog custom writer.

    // -----------------------------------------------------------------------------------
    // Player Create + Update custom writers — referenced by V3_4_3_54261.PlayerField.
    // -----------------------------------------------------------------------------------

    internal void WriteCreatePlayerCustomizationsCount(WorldPacket data, PlayerData src)
    {
        int customizationCount = 0;
        if (src.Customizations != null)
        {
            for (int i = 0; i < src.Customizations.Length; i++)
                if (src.Customizations[i] != null) customizationCount++;
        }
        data.WriteUInt32((uint)customizationCount);
    }

    internal void WriteCreatePlayerCustomizationsData(WorldPacket data, PlayerData src)
    {
        if (src.Customizations == null) return;
        for (int m = 0; m < src.Customizations.Length; m++)
        {
            var choice = src.Customizations[m];
            if (choice != null)
            {
                data.WriteUInt32(choice.ChrCustomizationOptionID);
                data.WriteUInt32(choice.ChrCustomizationChoiceID);
            }
        }
    }

    internal void WriteCreatePlayerQuestLog(WorldPacket data, PlayerData src)
    {
        // Owner-gated by generator (placeholder declares OwnerOnly = true). Iterates
        // 25 quest slots, writing each entry's 4 fields. Null entries write zeros.
        for (int q = 0; q < QuestConst.MaxQuestLogSize; q++)
        {
            var quest = src.QuestLog != null && q < src.QuestLog.Length ? src.QuestLog[q] : null;
            data.WriteInt64(quest?.EndTime ?? 0);
            data.WriteInt32(quest?.QuestID ?? 0);
            data.WriteUInt32(quest?.StateFlags ?? 0);
            for (int obj = 0; obj < 24; obj++)
                data.WriteUInt16((ushort)(quest?.ObjectiveProgress[obj] ?? 0));
        }
    }

    internal void WriteCreatePlayerVisibleItems(WorldPacket data, PlayerData src)
    {
        // 19× always-write. Null entry → zero placeholder (Int32 ItemID + 2× UInt16 0).
        for (int j = 0; j < 19; j++)
        {
            if (src.VisibleItems != null && j < src.VisibleItems.Length
                && src.VisibleItems[j] is VisibleItem pv)
            {
                data.WriteInt32(pv.ItemID);
                data.WriteUInt16(pv.ItemAppearanceModID);
                data.WriteUInt16(pv.ItemVisual);
            }
            else
            {
                data.WriteInt32(0);
                data.WriteUInt16(0);
                data.WriteUInt16(0);
            }
        }
    }

    internal void WriteCreatePlayerLogoutTime(WorldPacket data, PlayerData src)
    {
        // Owner: real UnixTime. Non-owner: 0. Matches hand-port (file:775 pre-delete).
        data.WriteInt64(IsOwner ? (long)Time.UnixTime : 0L);
    }

    internal void WriteCreatePlayerBnetAccount(WorldPacket data, PlayerData src)
    {
        // Owner: BNet account GUID lookup. Non-owner: Empty. Matches hand-port file:781-783.
        data.WritePackedGuid128(IsOwner
            ? _gameState.GlobalSession.GetBnetAccountGuidForPlayer(_updateData.Guid)
            : WowGuid128.Empty);
    }

    internal void WriteUpdatePlayerQuestLogEntry(WorldPacket data, QuestLog[] arr, int i)
    {
        // Per-element write at bit 36+i. Same shape as hand-port file:1576-1583 — uses
        // WriteCreate format (no inner mask, raw fields) per IsQuestLogChangesMaskSkipped = 1.
        QuestLog quest = arr[i];
        data.WriteInt64(quest?.EndTime ?? 0);
        data.WriteInt32(quest?.QuestID ?? 0);
        data.WriteUInt32(quest?.StateFlags ?? 0);
        for (int obj = 0; obj < 24; obj++)
            data.WriteUInt16((ushort)(quest?.ObjectiveProgress[obj] ?? 0));
    }

    internal void WriteUpdatePlayerVisibleItem(WorldPacket data, System.Nullable<VisibleItem>[] arr, int i)
    {
        // Per-element write at bit 62+i. Inner 4-bit mask (0x0F = all 4 bits set) +
        // FlushBits + Int32 ItemID + UInt16 ItemAppearanceModID + UInt16 ItemVisual.
        // Matches hand-port file:1593-1599.
        VisibleItem item = arr[i].Value;
        data.WriteBits(0x0Fu, 4);
        data.FlushBits();
        data.WriteInt32(item.ItemID);
        data.WriteUInt16(item.ItemAppearanceModID);
        data.WriteUInt16(item.ItemVisual);
    }

    // ============================================================
    // ActivePlayer custom writers (Phase 5b ActivePlayer migration)
    // ============================================================

    // Whole-Create writer — kept as one method because the byte-stream is mostly
    // zero placeholders interleaved with a few real fields; declarative per-write
    // enum members would balloon to ~200 entries with no readability win.
    internal void WriteCreateActivePlayerAll(WorldPacket data, ActivePlayerData src)
    {
        // V3_4_3 ActivePlayerData wire — written in TC field-declaration order, NOT
        // descriptor bit order. Many lines below are "zero placeholders": legacy 3.3.5
        // does not populate the field, but the modern client expects a value at this
        // wire position so we emit 0 / type-default. Where a property IS available on
        // ActivePlayerData (Class A — see plan), read it. Otherwise annotate and ship 0.
        //
        // Bit numbers refer to ActivePlayerField.cs declarations.

        var active = src;

        // InvSlots[141] — modern flat layout fanned from legacy 23/24/28/7/12/32 slots.
        for (int i = 0; i < 141; i++)
            data.WritePackedGuid128(GetModernInvSlot(active, i) ?? WowGuid128.Empty);

        data.WritePackedGuid128(active.FarsightObject ?? WowGuid128.Empty);       // bit 26: FarsightObject (PackedGuid128)
        data.WritePackedGuid128(WowGuid128.Empty);                                 // bit 27: SummonedBattlePetGUID (PackedGuid128) — descriptor: not used
        data.WriteUInt32(0u);                                                      // bit 3 dynamic field: KnownTitles.size — proxy does not track titles
        data.WriteUInt64(active.Coinage.GetValueOrDefault());                      // bit 28: Coinage (UInt64)
        data.WriteInt32(active.XP.GetValueOrDefault());                            // bit 29: XP (Int32)
        data.WriteInt32(active.NextLevelXP.GetValueOrDefault());                   // bit 30: NextLevelXP (Int32)
        data.WriteInt32(active.TrialXP.GetValueOrDefault());                       // bit 31: TrialXP (Int32) — was hardcoded 0; live property exists

        // bit 32: Skill (nested SkillInfo[256] of 7 ushorts per slot)
        var skill = active.Skill;
        for (int j = 0; j < 256; j++)
        {
            data.WriteUInt16(skill?.SkillLineID[j].GetValueOrDefault() ?? 0);
            data.WriteUInt16(skill?.SkillStep[j].GetValueOrDefault() ?? 0);
            data.WriteUInt16(skill?.SkillRank[j].GetValueOrDefault() ?? 0);
            data.WriteUInt16(skill?.SkillStartingRank[j].GetValueOrDefault() ?? 0);
            data.WriteUInt16(skill?.SkillMaxRank[j].GetValueOrDefault() ?? 0);
            data.WriteUInt16((ushort)(skill?.SkillTempBonus[j].GetValueOrDefault() ?? 0));
            data.WriteUInt16(skill?.SkillPermBonus[j].GetValueOrDefault() ?? 0);
        }

        data.WriteInt32(active.CharacterPoints.GetValueOrDefault());               // bit 33: CharacterPoints (Int32)
        data.WriteInt32(active.MaxTalentTiers.GetValueOrDefault());                // bit 34: MaxTalentTiers (Int32)
        data.WriteUInt32(active.TrackCreatureMask ?? 0u);                          // bit 35: TrackCreatureMask (UInt32) — was hardcoded 0; live property exists
        data.WriteUInt32(0u);                                                      // bit 36: MainhandExpertise (Float) — TYPE MISMATCH: wire is UInt32 here but descriptor says Float. Safe while value=0. TODO read live as Float.
        data.WriteUInt32(0u);                                                      // bit 37: OffhandExpertise (Float) — TYPE MISMATCH. TODO read live as Float.

        // Block 38 percentages (12 fields: RangedExpertise..OffhandCritPercentage + ShieldBlock-area filler).
        // All zero today; live values available on ActivePlayerData. TODO unpack per-field.
        for (int z = 0; z < 12; z++)
            data.WriteFloat(0f);

        // Multi-school combat arrays — interleaved by school index 0..6.
        // bits 270/277/284/291 (parent 269): SpellCritPercentage[7], ModDamageDonePos[7], ModDamageDoneNeg[7], ModDamageDonePercent[7].
        // All zero today; live values available on ActivePlayerData. TODO read per-element.
        for (int k = 0; k < 7; k++)
        {
            data.WriteFloat(0f);
            data.WriteInt32(0);
            data.WriteInt32(0);
            data.WriteFloat(0f);
        }

        // Block 38 remainder (bits 51-68: Mastery/Speed/Avoidance/Sturdiness/Versatility*/PvpPower*/ModHealing*/etc.).
        // All zero today; live values available on ActivePlayerData. TODO unpack per-field.
        data.WriteInt32(0);
        data.WriteFloat(0f);
        data.WriteFloat(0f);
        data.WriteFloat(0f);
        data.WriteFloat(0f);
        data.WriteFloat(0f);
        data.WriteInt32(0);
        data.WriteFloat(0f);
        data.WriteFloat(0f);
        data.WriteFloat(0f);

        // bit 299 (parent 298): ExploredZones[240] (UInt64). Live property exists; TODO per-element read.
        for (int l = 0; l < 240; l++)
            data.WriteUInt64(0uL);

        // bits 540-541 (parent 539): RestInfo[2] nested struct {Threshold:UInt32, StateID:UInt8}.
        // Defaults: Threshold=0, StateID=1 per element. TODO read active.RestInfo[r].Threshold/StateID.
        data.WriteUInt32(0u);                                                      // RestInfo[0].Threshold
        data.WriteUInt8(1);                                                        // RestInfo[0].StateID (default 1)
        data.WriteUInt32(0u);                                                      // RestInfo[1].Threshold
        data.WriteUInt8(1);                                                        // RestInfo[1].StateID (default 1)

        data.WriteInt32(0);                                                        // unmapped placeholder
        data.WriteFloat(0f);                                                       // unmapped placeholder (likely follow-up scalar)
        data.WriteFloat(0f);                                                       // unmapped placeholder
        data.WriteFloat(0f);                                                       // unmapped placeholder

        // bits 543/546 (parent 542): WeaponDmgMultipliers[3], WeaponAtkSpeedMultipliers[3] (both Float, default 1f).
        // TODO read live: active.WeaponDmgMultipliers?[m] ?? 1f / active.WeaponAtkSpeedMultipliers?[m] ?? 1f.
        for (int m = 0; m < 3; m++)
        {
            data.WriteFloat(1f);                                                   // WeaponDmgMultipliers[m]
            data.WriteFloat(1f);                                                   // WeaponAtkSpeedMultipliers[m]
        }
        data.WriteFloat(0f);                                                       // unmapped placeholder
        data.WriteFloat(0f);                                                       // unmapped placeholder
        data.WriteFloat(0f);                                                       // unmapped placeholder
        data.WriteFloat(0f);                                                       // unmapped placeholder
        data.WriteInt32(0);                                                        // unmapped placeholder
        data.WriteInt32(0);                                                        // unmapped placeholder

        data.WriteUInt32(0u);                                                      // unmapped placeholder (likely block 70 parent slot)
        // bits 71-74 (parent 70): block-70 byte cluster — fixed 2026-05-21
        // (was the action-bar 2/3/4/5 persistence bug).
        data.WriteUInt8(active.GrantableLevels ?? 0);                              // bit 71: GrantableLevels (UInt8)
        data.WriteUInt8(active.MultiActionBars ?? 0);                              // bit 72: MultiActionBars (UInt8)
        data.WriteUInt8(active.LifetimeMaxRank ?? 0);                              // bit 73: LifetimeMaxRank (UInt8)
        data.WriteUInt8(active.NumRespecs ?? 0);                                   // bit 74: NumRespecs (UInt8)
        data.WriteInt32((int)active.AmmoID.GetValueOrDefault());                   // bit 75: AmmoID (UInt32?→Int32 cast)
        data.WriteUInt32(active.PvpMedals ?? 0u);                                  // bit 76: PvpMedals (UInt32) — was hardcoded 0; live property exists

        // bits 550/562 (parent 549): BuybackPrice[12] (UInt32), BuybackTimestamp[12] (Int64 cast from uint?).
        // Interleaved by slot. Live properties exist; TODO per-element read.
        for (int n = 0; n < 12; n++)
        {
            data.WriteUInt32(0u);                                                  // BuybackPrice[n] (UInt32)
            data.WriteInt64(0L);                                                   // BuybackTimestamp[n] ((long)cast)
        }

        // bits 77-84 (block 70): 8 UInt16 honorable/dishonorable kill counters
        // (Today/Yesterday/LastWeek/ThisWeek × Honorable/Dishonorable).
        // Live properties exist; TODO per-bit read.
        for (int o = 0; o < 8; o++)
            data.WriteUInt16(0);

        // bits 85-91 (block 70): 7 UInt32 contribution / lifetime kills / yesterday & lastweek contribution / lastweek rank.
        // Live properties exist; TODO per-bit read.
        for (int p = 0; p < 7; p++)
            data.WriteUInt32(0u);

        data.WriteInt32(active.WatchedFactionIndex ?? -1);                         // bit 92: WatchedFactionIndex (Int32, default -1)

        // bit 575 (parent 574): CombatRatings[32] (Int32).
        for (int c = 0; c < 32; c++)
            data.WriteInt32(active.CombatRatings?[c].GetValueOrDefault() ?? 0);

        data.WriteInt32(active.MaxLevel ?? LegacyVersion.GetMaxLevel());           // bit 93: MaxLevel (Int32, default = per-build cap)
        data.WriteInt32(0);                                                        // bit 94: ScalingPlayerLevelDelta (Int32) — live property exists, TODO
        data.WriteInt32(0);                                                        // bit 95: MaxCreatureScalingLevel (Int32) — live property exists, TODO

        // bit 616 (parent 615): NoReagentCostMask[4] (UInt32). Live property exists; TODO per-element read.
        for (int q = 0; q < 4; q++)
            data.WriteUInt32(0u);

        data.WriteInt32(active.PetSpellPower.GetValueOrDefault());                 // bit 96: PetSpellPower (Int32)

        // bit 621 (parent 620): ProfessionSkillLine[2] (Int32).
        for (int s = 0; s < 2; s++)
            data.WriteInt32(active.ProfessionSkillLine?[s].GetValueOrDefault() ?? 0);

        data.WriteFloat(0f);                                                       // bit 97: UiHitModifier (Float) — live property exists, TODO
        data.WriteFloat(0f);                                                       // bit 98: UiSpellHitModifier (Float) — live property exists, TODO
        data.WriteInt32(0);                                                        // bit 99: HomeRealmTimeOffset (Int32) — live property exists, TODO
        data.WriteFloat(active.ModPetHaste ?? 1f);                                 // bit 100: ModPetHaste (Float, default 1f)
        data.WriteUInt8(0);                                                        // bit 101: LocalRegenFlags (UInt8) — live property exists, TODO
        data.WriteUInt8(0);                                                        // bit 103: AuraVision (UInt8) — live property exists, TODO
        data.WriteUInt8(active.NumBackpackSlots ?? 16);                            // bit 104: NumBackpackSlots (UInt8, default 16)
        data.WriteInt32(0);                                                        // bit 105: OverrideSpellsID (Int32) — live property exists, TODO
        data.WriteInt32(0);                                                        // bit 106: LfgBonusFactionID (Int32) — live property exists, TODO
        data.WriteUInt16(0);                                                       // bit 107: LootSpecID (UInt16 from uint? via (ushort) cast) — live property exists, TODO
        data.WriteUInt32(0u);                                                      // bit 108: OverrideZonePVPType (UInt32) — live property exists, TODO

        // bit 624 (parent 623): BagSlotFlags[4] (UInt32). Live property exists; TODO per-element read.
        for (int b = 0; b < 4; b++)
            data.WriteUInt32(0u);

        // bit 629 (parent 628): BankBagSlotFlags[7] (UInt32). Live property exists; TODO per-element read.
        for (int b = 0; b < 7; b++)
            data.WriteUInt32(0u);

        // bit 637 (parent 636): QuestCompleted[875] (UInt64). Live property exists; TODO per-element read.
        for (int qc = 0; qc < 875; qc++)
            data.WriteUInt64(0uL);

        data.WriteInt32(active.Honor.GetValueOrDefault());                         // bit 109: Honor (Int32)
        data.WriteInt32(active.HonorNextLevel ?? 5500);                            // bit 110: HonorNextLevel (Int32, default 5500)
        data.WriteInt32(0);                                                        // bit 111: Field_F74 — descriptor: unused
        data.WriteInt32((int?)active.PvPTierMaxFromWins ?? -1);                    // bit 112: PvPTierMaxFromWins (uint?→Int32 cast, default -1)
        data.WriteInt32((int?)active.PvPLastWeeksTierMaxFromWins ?? -1);           // bit 113: PvPLastWeeksTierMaxFromWins (uint?→Int32 cast, default -1)
        data.WriteUInt8(0);                                                        // bit 114: PvPRankProgress (UInt8) — live property exists, TODO
        data.WriteInt32(0);                                                        // bits 115-119: unused per descriptor

        // 16 UInt32 placeholder block — unmapped against current V3_4_3 descriptor.
        // Likely V3_4_3-only telemetry array (e.g. SeasonRewardsEarned / Field_F90+).
        for (int u = 0; u < 16; u++)
            data.WriteUInt32(0u);

        data.WriteInt32(0);                                                        // unmapped placeholder
        data.WriteUInt32(0u);                                                      // unmapped placeholder
        data.WriteUInt32(0u);                                                      // unmapped placeholder

        // bit 1512 GlyphsGroup (CustomField, sources from _gameState — not ActivePlayerData).
        for (int g = 0; g < PlayerConst.MaxGlyphSlots; g++)
        {
            data.WriteUInt32(_gameState.ActiveGlyphSlotIds[g]);                    // GlyphSlots[g]
            data.WriteUInt32(_gameState.ActiveGlyphs[g]);                          // Glyphs[g]
        }
        data.WriteUInt8(_gameState.GlyphsEnabled);                                 // bit 120: GlyphsEnabled (UInt8, from _gameState)
        data.WriteUInt8(0);                                                        // LfgRoles placeholder
        data.WriteUInt32(0u);                                                      // unmapped placeholder
        data.WriteUInt32(0u);                                                      // unmapped placeholder
        data.WriteUInt8(0);                                                        // unmapped placeholder

        // bits 608-614 (parent 607): PvpInfo[7] nested struct.
        // Per-element layout per WriteUpdateActivePlayerPvpInfo: Int8 + 16×UInt32 + 1 bit + FlushBits.
        // Live property is PVPInfo[6] on ActivePlayerData. TODO mirror update writer.
        for (int t = 0; t < 7; t++)
        {
            data.WriteInt8(0);                                                     // PvpInfo[t]: per-element prefix byte
            for (int x = 0; x < 16; x++)
                data.WriteUInt32(0u);                                              // PvpInfo[t]: 16× UInt32 payload (Rating/SeasonPlayed/WeeklyPlayed/etc.)
            data.WriteBit(false);                                                  // PvpInfo[t]: Disqualified bit
            data.FlushBits();
        }
        data.FlushBits();
        data.WriteBit(false);                                                      // trailing bit placeholder (PvpInfo group cap?)
        data.WriteBit(false);                                                      // trailing bit placeholder
        data.WriteBit(false);                                                      // trailing bit placeholder
        data.FlushBits();
        data.WriteUInt32(0u);                                                      // trailing UInt32 placeholder
        for (int e = 0; e < 8; e++)
            data.WriteInt32(0);                                                    // trailing 8× Int32 placeholder
        data.WriteInt64(0L);                                                       // trailing Int64 placeholder
        data.WriteBit(false);                                                      // trailing bit placeholder
        data.FlushBits();
        data.FlushBits();
    }

    // MaskMutator — pass-1, sets InvSlots bits (124 parent + 125-265 per-element)
    // for each modern slot that has a non-null mapped legacy entry.
    internal void ApplyActivePlayerInvSlotsMaskMutator(ref Framework.Util.StackBitMask blocks, ActivePlayerData src)
    {
        for (int i = 0; i < 141; i++)
        {
            if (GetModernInvSlot(src, i) != null)
            {
                blocks.SetBit(124);
                blocks.SetBit(125 + i);
            }
        }
    }

    // MaskMutator — pass-1, captures and clears _gameState.ActiveGlyphsDirty.
    // When dirty, sets 1512 + 1513-1518 + 1519-1524 (parent + GlyphSlots + Glyphs).
    internal void ApplyActivePlayerGlyphsMaskMutator(ref Framework.Util.StackBitMask blocks, ActivePlayerData src)
    {
        if (!_gameState.ActiveGlyphsDirty)
            return;
        _gameState.ActiveGlyphsDirty = false;
        blocks.SetBit(1512);
        for (int i = 0; i < PlayerConst.MaxGlyphSlots; i++)
        {
            blocks.SetBit(1513 + i);
            blocks.SetBit(1519 + i);
        }
    }

    // Folds KnownTitles uint?[12] → ulong[6] (lo + hi<<32 per pair). Used by both
    // preamble (count) and body (data).
    internal static int FoldKnownTitles(uint?[] knownTitles, ulong[] dest)
    {
        if (knownTitles == null)
            return 0;
        bool anyTitle = false;
        for (int i = 0; i < knownTitles.Length; i++)
            if (knownTitles[i].HasValue) { anyTitle = true; break; }
        if (!anyTitle)
            return 0;
        for (int i = 0; i < 6; i++)
        {
            uint lo = (i * 2 < knownTitles.Length && knownTitles[i * 2].HasValue) ? knownTitles[i * 2]!.Value : 0;
            uint hi = (i * 2 + 1 < knownTitles.Length && knownTitles[i * 2 + 1].HasValue) ? knownTitles[i * 2 + 1]!.Value : 0;
            dest[i] = (ulong)lo | ((ulong)hi << 32);
        }
        return 6;
    }

    // Static predicate referenced from ActivePlayerField.ACTIVEPLAYER_KNOWN_TITLES CustomPredicate.
    internal static bool HasAnyKnownTitle(uint?[] knownTitles)
    {
        if (knownTitles == null) return false;
        for (int i = 0; i < knownTitles.Length; i++)
            if (knownTitles[i].HasValue) return true;
        return false;
    }

    // Static wrapper for HasAnySkillChanged — referenced from CustomPredicate.
    internal static bool HasAnySkillChangedStatic(SkillInfo s) => HasAnySkillChanged(s);

    // KnownTitles preamble — between blocks-mask write and FlushBits.
    // Emits: WriteBits(count, 32) + count× WriteBit(true).
    internal void WriteUpdateActivePlayerKnownTitlesPreamble(WorldPacket data, ref Framework.Util.StackBitMask blocks, ActivePlayerData src)
    {
        ulong[] folded = new ulong[6];
        int count = FoldKnownTitles(src.KnownTitles, folded);
        data.WriteBits((uint)count, 32);
        for (int i = 0; i < count; i++)
            data.WriteBit(true);
    }

    // KnownTitles body — count× WriteUInt64(folded[i]).
    internal void WriteUpdateActivePlayerKnownTitlesBody(WorldPacket data, ActivePlayerData src)
    {
        ulong[] folded = new ulong[6];
        int count = FoldKnownTitles(src.KnownTitles, folded);
        for (int i = 0; i < count; i++)
            data.WriteUInt64(folded[i]);
    }

    // Skill (bit 32) — nested SkillInfo write via existing WriteUpdateSkillInfo helper.
    internal void WriteUpdateActivePlayerSkill(WorldPacket data, ActivePlayerData src)
    {
        WriteUpdateSkillInfo(data, src.Skill);
    }

    // InvSlots group (bit 124) — iterate 141 slots, write each via GetModernInvSlot
    // gated on its element bit (125 + i).
    internal void WriteUpdateActivePlayerInvSlotsGroup(WorldPacket data, ref Framework.Util.StackBitMask blocks, ActivePlayerData src)
    {
        for (int i = 0; i < 141; i++)
        {
            if (blocks.IsBitSet(125 + i))
            {
                WowGuid128 guid = GetModernInvSlot(src, i) ?? WowGuid128.Empty;
                data.WritePackedGuid128(guid);
            }
        }
    }

    // RestInfo per-element — 3-bit inner mask (bit 0 group, 1 Threshold, 2 StateID)
    // + FlushBits + conditional UInt32 Threshold + UInt8 StateID.
    internal void WriteUpdateActivePlayerRestInfo(WorldPacket data, RestInfo[] arr, int i)
    {
        var ri = arr[i];
        uint restMask = 0;
        if (ri != null && ri.Threshold.HasValue) restMask |= 2;
        if (ri != null && ri.StateID.HasValue) restMask |= 4;
        if (restMask != 0) restMask |= 1;
        data.WriteBits(restMask, 3);
        data.FlushBits();
        if ((restMask & 2) != 0) data.WriteUInt32(ri!.Threshold!.Value);
        if ((restMask & 4) != 0) data.WriteUInt8((byte)ri!.StateID!.Value);
    }

    // PvpInfo per-element — 19-bit inner mask + optional Disqualified bit before
    // flush + FlushBits + conditional UInt32 fields. Mirrors hand-port file:2018-2069.
    internal void WriteUpdateActivePlayerPvpInfo(WorldPacket data, PVPInfo[] arr, int i)
    {
        PVPInfo pi = (arr != null && i < arr.Length) ? arr[i] : null;
        uint pvpMask = 0;
        if (pi != null)
        {
            if (pi.Disqualified) pvpMask |= (1u << 1);
            if (pi.WeeklyPlayed != 0) pvpMask |= (1u << 4);
            if (pi.WeeklyWon != 0) pvpMask |= (1u << 5);
            if (pi.SeasonPlayed != 0) pvpMask |= (1u << 6);
            if (pi.SeasonWon != 0) pvpMask |= (1u << 7);
            if (pi.Rating != 0) pvpMask |= (1u << 8);
            if (pi.WeeklyBestRating != 0) pvpMask |= (1u << 9);
            if (pi.SeasonBestRating != 0) pvpMask |= (1u << 10);
            if (pi.PvpTierID != 0) pvpMask |= (1u << 11);
            if (pi.WeeklyBestWinPvpTierID != 0) pvpMask |= (1u << 12);
            if (pi.Field_28 != 0) pvpMask |= (1u << 13);
            if (pi.Field_2C != 0) pvpMask |= (1u << 14);
        }
        if (pvpMask != 0) pvpMask |= 1;
        data.WriteBits(pvpMask, 19);
        if ((pvpMask & (1u << 1)) != 0) data.WriteBit(pi!.Disqualified);
        data.FlushBits();
        if ((pvpMask & 1) != 0)
        {
            if ((pvpMask & (1u << 4)) != 0) data.WriteUInt32(pi!.WeeklyPlayed);
            if ((pvpMask & (1u << 5)) != 0) data.WriteUInt32(pi!.WeeklyWon);
            if ((pvpMask & (1u << 6)) != 0) data.WriteUInt32(pi!.SeasonPlayed);
            if ((pvpMask & (1u << 7)) != 0) data.WriteUInt32(pi!.SeasonWon);
            if ((pvpMask & (1u << 8)) != 0) data.WriteUInt32(pi!.Rating);
            if ((pvpMask & (1u << 9)) != 0) data.WriteUInt32(pi!.WeeklyBestRating);
            if ((pvpMask & (1u << 10)) != 0) data.WriteUInt32(pi!.SeasonBestRating);
            if ((pvpMask & (1u << 11)) != 0) data.WriteUInt32(pi!.PvpTierID);
            if ((pvpMask & (1u << 12)) != 0) data.WriteUInt32(pi!.WeeklyBestWinPvpTierID);
            if ((pvpMask & (1u << 13)) != 0) data.WriteUInt32(pi!.Field_28);
            if ((pvpMask & (1u << 14)) != 0) data.WriteUInt32(pi!.Field_2C);
        }
    }

    // Glyphs group (bit 1512) — interleaved GlyphSlots[0..5] then Glyphs[0..5],
    // each gated on its element bit (1513+i / 1519+i). Source = _gameState.
    internal void WriteUpdateActivePlayerGlyphsGroup(WorldPacket data, ref Framework.Util.StackBitMask blocks, ActivePlayerData src)
    {
        for (int i = 0; i < PlayerConst.MaxGlyphSlots; i++)
            if (blocks.IsBitSet(1513 + i))
                data.WriteUInt32(_gameState.ActiveGlyphSlotIds[i]);
        for (int i = 0; i < PlayerConst.MaxGlyphSlots; i++)
            if (blocks.IsBitSet(1519 + i))
                data.WriteUInt32((uint)_gameState.ActiveGlyphs[i]);
    }

    // HasAny helper for the InvSlots mask-mutator. Returns true when any of the 141
    // modern InvSlots positions has a non-null mapped legacy entry. Required because
    // the bits set by `ApplyActivePlayerInvSlotsMaskMutator` aren't covered by any
    // declared UpdateField presence check — without this, loot/bag-pickup that only
    // touches InvSlots would skip the Values update (items invisible until relog).
    internal static bool HasAnyInvSlotMapped(ActivePlayerData a)
    {
        if (a == null) return false;
        for (int i = 0; i < 141; i++)
            if (GetModernInvSlot(a, i) != null)
                return true;
        return false;
    }

    // Maps the modern 3.4.3 InvSlots index (0-140) to the corresponding legacy slot
    // arrays on ActivePlayerData. Returns null when the modern slot has no legacy
    // equivalent or the entry is missing.
    internal static WowGuid128? GetModernInvSlot(ActivePlayerData a, int modernIdx)
    {
        if (modernIdx <= 18)
        {
            if (a.InvSlots != null && modernIdx < a.InvSlots.Length)
                return a.InvSlots[modernIdx];
        }
        else if (modernIdx >= 30 && modernIdx <= 33)
        {
            int legacyIdx = 19 + (modernIdx - 30);
            if (a.InvSlots != null && legacyIdx < a.InvSlots.Length)
                return a.InvSlots[legacyIdx];
        }
        else if (modernIdx >= 35 && modernIdx <= 58)
        {
            int idx = modernIdx - 35;
            if (a.PackSlots != null && idx < a.PackSlots.Length)
                return a.PackSlots[idx];
        }
        else if (modernIdx >= 59 && modernIdx <= 86)
        {
            int idx = modernIdx - 59;
            if (a.BankSlots != null && idx < a.BankSlots.Length)
                return a.BankSlots[idx];
        }
        else if (modernIdx >= 87 && modernIdx <= 93)
        {
            int idx = modernIdx - 87;
            if (a.BankBagSlots != null && idx < a.BankBagSlots.Length)
                return a.BankBagSlots[idx];
        }
        else if (modernIdx >= 94 && modernIdx <= 105)
        {
            int idx = modernIdx - 94;
            if (a.BuyBackSlots != null && idx < a.BuyBackSlots.Length)
                return a.BuyBackSlots[idx];
        }
        else if (modernIdx >= 106 && modernIdx <= 137)
        {
            int idx = modernIdx - 106;
            if (a.KeyringSlots != null && idx < a.KeyringSlots.Length)
                return a.KeyringSlots[idx];
        }
        return null;
    }


    // WriteCreateGameObjectData emitted by HermesProxy.SourceGen.ObjectUpdateBuilderGenerator
    // from V3_4_3_54261.GameObjectField (see HermesProxy/obj/Generated/.../V3_4_3_54261.ObjectUpdateBuilder.g.cs).

    private void WriteCreateDynamicObjectData(WorldPacket data)
    {
        var dyn = _updateData.DynamicObjectData ?? new DynamicObjectData();
        data.WritePackedGuid128(dyn.Caster ?? WowGuid128.Empty);
        data.WriteUInt8(0);
        data.WriteInt32(dyn.SpellXSpellVisualID.GetValueOrDefault());
        data.WriteInt32(dyn.SpellID.GetValueOrDefault());
        data.WriteFloat(dyn.Radius.GetValueOrDefault());
        data.WriteUInt32(dyn.CastTime.GetValueOrDefault());
    }

    private void WriteCreateCorpseData(WorldPacket data)
    {
        var corpse = _updateData.CorpseData ?? new CorpseData();
        // TC343 field order: DynamicFlags FIRST, then Owner, Party, Guild, etc.
        data.WriteUInt32(corpse.DynamicFlags.GetValueOrDefault());
        data.WritePackedGuid128(corpse.Owner ?? WowGuid128.Empty);
        data.WritePackedGuid128(corpse.PartyGUID ?? WowGuid128.Empty);
        data.WritePackedGuid128(corpse.GuildGUID ?? WowGuid128.Empty);
        data.WriteUInt32(corpse.DisplayID.GetValueOrDefault());
        for (int i = 0; i < 19; i++)
            data.WriteUInt32(corpse.Items?[i].GetValueOrDefault() ?? 0);
        data.WriteUInt8(corpse.RaceId.GetValueOrDefault());
        data.WriteUInt8(corpse.SexId.GetValueOrDefault());
        data.WriteUInt8(corpse.ClassId.GetValueOrDefault());
        data.WriteUInt32(0u); // Customizations.size() = 0
        data.WriteUInt32(corpse.Flags.GetValueOrDefault());
        data.WriteInt32(corpse.FactionTemplate.GetValueOrDefault());
    }

    internal static bool HasAnySkillChanged(SkillInfo s)
    {
        for (int i = 0; i < 256; i++)
        {
            if (s.SkillLineID[i].HasValue) return true;
            if (s.SkillRank[i].HasValue) return true;
            if (s.SkillMaxRank[i].HasValue) return true;
        }
        return false;
    }

    // Writes SkillInfo nested update using TC343 format: HasChangesMask<1793> = 57 blocks of 32 bits.
    // Bit 0 is the root flag; bits 1..1792 are the per-skill fields, grouped 256 at a time
    // (LineID, Step, Rank, StartingRank, MaxRank, TempBonus, PermBonus). Mask encoding:
    //   1) WriteUInt32(blocksMask0) — which of blocks 0-31 have changes
    //   2) WriteBits(blocksMask1, 25) — which of blocks 32-56 have changes
    //   3) For each set block: WriteBits(block[b], 32)
    //   4) FlushBits
    //   5) Per-skill interleaved data (all 7 fields for skill i before skill i+1).
    internal static void WriteUpdateSkillInfo(WorldPacket data, SkillInfo s)
    {
        if (s == null)
        {
            data.WriteUInt32(0);
            data.WriteBits(0, 25);
            data.FlushBits();
            return;
        }

        Span<uint> skillBlockBuf = stackalloc uint[57];
        var skillBlocks = new StackBitMask(skillBlockBuf);

        bool anyChanged = false;
        for (int i = 0; i < 256; i++)
        {
            if (s.SkillLineID[i].HasValue) { skillBlocks.SetBit(1 + i); anyChanged = true; }
            if (s.SkillStep[i].HasValue) { skillBlocks.SetBit(257 + i); anyChanged = true; }
            if (s.SkillRank[i].HasValue) { skillBlocks.SetBit(513 + i); anyChanged = true; }
            if (s.SkillStartingRank[i].HasValue) { skillBlocks.SetBit(769 + i); anyChanged = true; }
            if (s.SkillMaxRank[i].HasValue) { skillBlocks.SetBit(1025 + i); anyChanged = true; }
            if (s.SkillTempBonus[i].HasValue) { skillBlocks.SetBit(1281 + i); anyChanged = true; }
            if (s.SkillPermBonus[i].HasValue) { skillBlocks.SetBit(1537 + i); anyChanged = true; }
        }

        if (anyChanged)
            skillBlocks.SetBit(0);

        uint blocksMask0 = 0;
        for (int b = 0; b < 32; b++)
            if (skillBlocks[b] != 0) blocksMask0 |= (1u << b);

        uint blocksMask1 = 0;
        for (int b = 32; b < 57; b++)
            if (skillBlocks[b] != 0) blocksMask1 |= (1u << (b - 32));

        data.WriteUInt32(blocksMask0);
        data.WriteBits(blocksMask1, 25);

        for (int b = 0; b < 57; b++)
        {
            bool blockSet = b < 32
                ? (blocksMask0 & (1u << b)) != 0
                : (blocksMask1 & (1u << (b - 32))) != 0;
            if (blockSet)
                data.WriteBits(skillBlocks[b], 32);
        }

        data.FlushBits();

        if ((skillBlocks[0] & 1) == 0)
            return;

        for (int i = 0; i < 256; i++)
        {
            if (skillBlocks.IsBitSet(1 + i))
                data.WriteUInt16(s.SkillLineID[i]!.Value);
            if (skillBlocks.IsBitSet(257 + i))
                data.WriteUInt16(s.SkillStep[i]!.Value);
            if (skillBlocks.IsBitSet(513 + i))
                data.WriteUInt16(s.SkillRank[i]!.Value);
            if (skillBlocks.IsBitSet(769 + i))
                data.WriteUInt16(s.SkillStartingRank[i]!.Value);
            if (skillBlocks.IsBitSet(1025 + i))
                data.WriteUInt16(s.SkillMaxRank[i]!.Value);
            if (skillBlocks.IsBitSet(1281 + i))
                data.WriteInt16(s.SkillTempBonus[i]!.Value);
            if (skillBlocks.IsBitSet(1537 + i))
                data.WriteUInt16(s.SkillPermBonus[i]!.Value);
        }
    }

    private void WriteValuesCreate(WorldPacket data)
    {
        var effectiveMask = _objectTypeMask;
        bool trace = _objectType == ObjectTypeBCC.ActivePlayer;

        // Owner=0x01, PartyMember=0x02 (needed for QuestLog visibility).
        byte updateFieldFlags = (byte)(IsOwner ? 0x03 : 0);
        data.WriteUInt8(updateFieldFlags);

        int p0 = data.GetData().Length;
        WriteCreateObjectData(data);
        int p1 = data.GetData().Length;

        if (effectiveMask.HasAnyFlag(ObjectTypeMask.Item))
            WriteCreateItemData(data);
        int p2 = data.GetData().Length;

        if (effectiveMask.HasAnyFlag(ObjectTypeMask.Container))
            WriteCreateContainerData(data);
        int p3 = data.GetData().Length;

        if (effectiveMask.HasAnyFlag(ObjectTypeMask.Unit))
            WriteCreateUnitData(data);
        int p4 = data.GetData().Length;

        if (effectiveMask.HasAnyFlag(ObjectTypeMask.Player))
            WriteCreatePlayerData(data);
        int p5 = data.GetData().Length;

        if (effectiveMask.HasAnyFlag(ObjectTypeMask.ActivePlayer))
            WriteCreateActivePlayerData(data);
        int p6 = data.GetData().Length;

        if (_objectTypeMask.HasAnyFlag(ObjectTypeMask.GameObject))
            WriteCreateGameObjectData(data);
        int p7 = data.GetData().Length;

        if (_objectTypeMask.HasAnyFlag(ObjectTypeMask.DynamicObject))
            WriteCreateDynamicObjectData(data);
        int p8 = data.GetData().Length;

        if (_objectTypeMask.HasAnyFlag(ObjectTypeMask.Corpse))
            WriteCreateCorpseData(data);
        int p9 = data.GetData().Length;

        // Phase 5a diagnostic — per-section byte sizes for the ActivePlayer create
        // packet. Used to bisect which descriptor section diverges from the V3_4_3
        // expected wire format (root cause of the ERROR #132 ACCESS_VIOLATION on
        // world-enter). Counts may be off by up to 1 byte per section if a section
        // ends with unflushed bits — acceptable for first-pass bisection.
        if (trace)
        {
            byte[] buf = data.GetData();
            Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
                $"[Phase5aTrace] sections flags=1 obj={p1 - p0} item={p2 - p1} container={p3 - p2} " +
                $"unit={p4 - p3} player={p5 - p4} active={p6 - p5} " +
                $"go={p7 - p6} dynobj={p8 - p7} corpse={p9 - p8} valuesTotal={p9 - p0 + 1}");

            DumpSectionHead(buf, p0, p1, "obj");
            DumpSectionHead(buf, p3, p4, "unit");
            DumpSectionHead(buf, p4, p5, "player");
            DumpSectionHead(buf, p5, p6, "active");
        }
    }

    private static void DumpSectionHead(byte[] buf, int start, int end, string label)
    {
        int len = end - start;
        if (len <= 0) return;
        int dumpLen = Math.Min(64, len);
        string hex = BitConverter.ToString(buf, start, dumpLen);
        Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
            $"[Phase5aTrace]   {label} ({len} bytes) head={hex}");
    }

    // V3_4_3 Values-update writer. Ported from HermesProxy-WOTLK fork. Builds the
    // changedMask + per-section bit-mask blocks for partial UpdateObject packets so
    // the modern client receives real field deltas. The fork's logic is reused
    // verbatim (same bit positions, same block layout) — see WriteUpdate*Data
    // methods and the HasAny*FieldSet predicates below.
    private void WriteValuesUpdate(WorldPacket data)
    {
        uint changedMask = 0u;
        bool hasObjectChanges = _objectTypeMask.HasAnyFlag(ObjectTypeMask.Object) && HasAnyObjectFieldSet();
        bool hasUnitChanges = _objectTypeMask.HasAnyFlag(ObjectTypeMask.Unit) && _updateData.UnitData != null && HasAnyUnitFieldSet();
        bool hasItemChanges = _objectTypeMask.HasAnyFlag(ObjectTypeMask.Item) && HasAnyItemFieldSet();
        bool hasContainerChanges = _objectTypeMask.HasAnyFlag(ObjectTypeMask.Container) && _updateData.ContainerData != null && HasAnyContainerFieldSet();
        bool hasActivePlayerChanges = _objectTypeMask.HasAnyFlag(ObjectTypeMask.ActivePlayer) && HasAnyActivePlayerFieldSet();
        bool hasPlayerChanges = _objectTypeMask.HasAnyFlag(ObjectTypeMask.Player) && HasAnyPlayerFieldSet();
        bool hasGameObjectChanges = _objectTypeMask.HasAnyFlag(ObjectTypeMask.GameObject) && _updateData.GameObjectData != null && HasAnyGameObjectFieldSet();

        if (hasObjectChanges) changedMask |= 1;
        if (hasItemChanges) changedMask |= 2;
        if (hasContainerChanges) changedMask |= 0x04;
        if (hasUnitChanges) changedMask |= 0x20;
        if (hasPlayerChanges) changedMask |= 0x40;
        if (hasActivePlayerChanges) changedMask |= 0x80;
        if (hasGameObjectChanges) changedMask |= 0x100;

        // Safety: if changedMask is 0, nothing to write — emit empty mask so the
        // outer wire format stays valid. Filter at QueryHandler/UpdateHandler will
        // drop the empty entry before shipping.
        if (changedMask == 0)
        {
            data.WriteUInt32(0u);
            return;
        }

        data.WriteUInt32(changedMask);
        if (hasObjectChanges) WriteUpdateObjectData(data);
        if (hasItemChanges) WriteUpdateItemData(data);
        if (hasContainerChanges) WriteUpdateContainerData(data);
        if (hasUnitChanges) WriteUpdateUnitData(data);
        if (hasPlayerChanges) WriteUpdatePlayerData(data);
        if (hasActivePlayerChanges) WriteUpdateActivePlayerData(data);
        if (hasGameObjectChanges) WriteUpdateGameObjectData(data);
    }

    // HasAnyContainerFieldSet emitted by HermesProxy.SourceGen.ObjectUpdateBuilderGenerator.

    // === HasAnyUnitFieldSet (fork lines 1140-1196) ===
    // HasAnyUnitFieldSet emitted by HermesProxy.SourceGen.ObjectUpdateBuilderGenerator from V3_4_3_54261.UnitField.
    // HasAnyPlayerFieldSet emitted by HermesProxy.SourceGen.ObjectUpdateBuilderGenerator from V3_4_3_54261.PlayerField.
    // HasAnyGameObjectFieldSet emitted by HermesProxy.SourceGen.ObjectUpdateBuilderGenerator.


    private void WriteValuesModern(WorldPacket packet)
    {
        var valuesBuffer = new WorldPacket();
        if (_updateData.Type == UpdateTypeModern.Values)
            WriteValuesUpdate(valuesBuffer);
        else
            WriteValuesCreate(valuesBuffer);

        var valuesData = valuesBuffer.GetData();

        // Debug: dump the bytes we produce for Values updates so we can compare against
        // TC's accepted format. CMSG_OBJECT_UPDATE_FAILED or `CMSG_LOG_DISCONNECT(reason=7)`
        // immediately after an UPDATE_OBJECT means the V3_4_3 client cannot parse what we
        // wrote — diff the hex against a known-good capture to find the bad byte.
        if (_updateData.Type == UpdateTypeModern.Values
            && _objectTypeMask.HasAnyFlag(ObjectTypeMask.Unit | ObjectTypeMask.GameObject | ObjectTypeMask.DynamicObject | ObjectTypeMask.Corpse))
        {
            int dumpLen = System.Math.Min(96, valuesData.Length);
            string hex = System.BitConverter.ToString(valuesData, 0, dumpLen);
            Framework.Logging.Log.Print(Framework.Logging.LogType.Debug,
                $"[ValuesUpdateHex] guid={_updateData.Guid} type={_objectType} size={valuesData.Length} hasUnit={_updateData.UnitData != null} hasPlayer={_updateData.PlayerData != null} hasActive={_updateData.ActivePlayerData != null} hasGO={_updateData.GameObjectData != null} hex={hex}");
        }

        packet.WriteUInt32((uint)valuesData.Length);
        packet.WriteBytes(valuesData);
    }

    public void WriteToPacket(WorldPacket packet)
    {
        int startPos = packet.GetData().Length;
        bool traceOn = Framework.Logging.Log.IsTraceEnabled;

        // Phase 5a diagnostic — log the player's UnitData fields most likely to cause
        // ERROR #132 ACCESS_VIOLATION crashes (null model dereference).
        if (traceOn && _updateData.UnitData != null && _objectType == ObjectTypeBCC.ActivePlayer)
        {
            var u = _updateData.UnitData;
            Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
                $"[Phase5aTrace] WriteToPacket type={_updateData.Type} guid={_updateData.Guid} " +
                $"DisplayID={u.DisplayID?.ToString() ?? "null"} NativeDisplayID={u.NativeDisplayID?.ToString() ?? "null"} " +
                $"MountDisplayID={u.MountDisplayID?.ToString() ?? "null"} " +
                $"Race={u.RaceId?.ToString() ?? "null"} Class={u.ClassId?.ToString() ?? "null"} Sex={u.SexId?.ToString() ?? "null"} " +
                $"Health={u.Health?.ToString() ?? "null"}/{u.MaxHealth?.ToString() ?? "null"} Level={u.Level?.ToString() ?? "null"} " +
                $"FactionTemplate={u.FactionTemplate?.ToString() ?? "null"} BoundingRadius={u.BoundingRadius?.ToString() ?? "null"} " +
                $"CombatReach={u.CombatReach?.ToString() ?? "null"}");
        }

        packet.WriteUInt8((byte)_updateData.Type);
        packet.WritePackedGuid128(_updateData.Guid);
        if (_updateData.Type != UpdateTypeModern.Values)
        {
            packet.WriteUInt8(ConvertTypeId(_objectType));
            SetCreateObjectBits();
            if (traceOn && _objectType == ObjectTypeBCC.ActivePlayer)
            {
                Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
                    $"[Phase5aTrace] ActivePlayer createBits=0x{(uint)_createBits:X} " +
                    $"hasThisIsYou={Has(CreateObjectBits.ThisIsYou)} " +
                    $"hasActivePlayer={Has(CreateObjectBits.ActivePlayer)} " +
                    $"hasMovementUpdate={Has(CreateObjectBits.MovementUpdate)} " +
                    $"hasNoBirthAnim={Has(CreateObjectBits.NoBirthAnim)}");
            }
            BuildMovementUpdate(packet);
        }
        WriteValuesModern(packet);

        // Hex-dump the produced packet body so we can correlate with the client crash dump.
        // Limited to first 80 bytes to avoid log spam — the header + first descriptor section
        // is enough to identify which object type was being written.
        if (traceOn && _objectType == ObjectTypeBCC.ActivePlayer)
        {
            byte[] all = packet.GetData();
            int len = all.Length - startPos;
            int dumpLen = Math.Min(80, len);
            string hex = BitConverter.ToString(all, startPos, dumpLen);
            Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
                $"[Phase5aTrace] ActivePlayer packet bytes={len} first80={hex}");
        }

        // Phase 5a-7c diagnostic — dump the wire bytes for Transport/GameObject creates
        // so we can byte-diff against the fork's working output. Per-object, capped at 200
        // bytes so a populated Stormwind area (5+ MOTransports) doesn't drown the log.
        if (traceOn
            && _updateData.Type != UpdateTypeModern.Values
            && _objectType == ObjectTypeBCC.GameObject)
        {
            byte[] all = packet.GetData();
            int len = all.Length - startPos;
            int dumpLen = Math.Min(200, len);
            string hex = BitConverter.ToString(all, startPos, dumpLen);
            var moveInfo = _updateData.CreateData?.MoveInfo;
            var go = _updateData.GameObjectData;
            Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
                $"GameObject packet entry={_updateData.ObjectData?.EntryID} guid={_updateData.Guid} " +
                $"highType={_updateData.Guid.GetHighType()} typeIdByte={ConvertTypeId(_objectType)} " +
                $"createBits=0x{(uint)_createBits:X} typeID={go?.TypeID?.ToString() ?? "null"} " +
                $"state={go?.State?.ToString() ?? "null"} display={go?.DisplayID?.ToString() ?? "null"} " +
                $"flags=0x{go?.Flags?.ToString("X") ?? "null"} " +
                $"pos=({moveInfo?.Position.X.ToString("F2") ?? "?"},{moveInfo?.Position.Y.ToString("F2") ?? "?"},{moveInfo?.Position.Z.ToString("F2") ?? "?"}) " +
                $"orient={moveInfo?.Orientation.ToString("F3") ?? "?"} bytes={len} first200={hex}");
        }

        // NPCBot-render diagnostic — dump the first ~256 bytes of any Creature-typed
        // CreateObject so we can see the actual Flags2 byte that hit the wire (and confirm
        // whether SanitizeFlags2 stripped UNIT_FLAG2_CLONED before emit).
        if (traceOn
            && _updateData.Type != UpdateTypeModern.Values
            && _updateData.Guid.GetHighType() == HighGuidType.Creature)
        {
            byte[] all = packet.GetData();
            int len = all.Length - startPos;
            int dumpLen = Math.Min(256, len);
            string hex = BitConverter.ToString(all, startPos, dumpLen);
            Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
                $"[CreateObjectHex] guid={_updateData.Guid} entry={_updateData.ObjectData?.EntryID?.ToString() ?? "null"} " +
                $"type={_objectType} bytes={len} first256={hex}");
        }
    }
}
