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
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Objects.Version.V3_4_3_54261;

// Phase 5a hand-port of the WotLK Classic 3.4.3 descriptor-tree serializer.
// Phases 5b–5e progressively replace sections with source-generator output,
// using this hand-port as the byte-equivalence test oracle.
public class ObjectUpdateBuilder
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
                data.WriteFloat(moveSpline.FinalOrientation);
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

    private void WriteCreateObjectData(WorldPacket data)
    {
        var obj = _updateData.ObjectData;
        data.WriteInt32(obj.EntryID.GetValueOrDefault());
        data.WriteUInt32(obj.DynamicFlags.GetValueOrDefault());
        data.WriteFloat(obj.Scale ?? 1f);
    }

    private void WriteCreateItemData(WorldPacket data)
    {
        var item = _updateData.ItemData;
        if (item == null)
        {
            WriteEmptyItemCreate(data);
            return;
        }
        data.WritePackedGuid128(item.Owner ?? WowGuid128.Empty);
        data.WritePackedGuid128(item.ContainedIn ?? WowGuid128.Empty);
        data.WritePackedGuid128(item.Creator ?? WowGuid128.Empty);
        data.WritePackedGuid128(item.GiftCreator ?? WowGuid128.Empty);
        if (IsOwner)
        {
            data.WriteUInt32(item.StackCount.GetValueOrDefault());
            data.WriteUInt32(item.Duration.GetValueOrDefault());
            for (int i = 0; i < 5; i++)
                data.WriteInt32(item.SpellCharges[i].GetValueOrDefault());
        }
        data.WriteUInt32(item.Flags.GetValueOrDefault());
        for (int j = 0; j < 13; j++)
        {
            var ench = item.Enchantment[j];
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
        data.WriteInt32((int)item.PropertySeed.GetValueOrDefault());
        data.WriteInt32((int)item.RandomProperty.GetValueOrDefault());
        if (IsOwner)
        {
            data.WriteUInt32(item.Durability.GetValueOrDefault());
            data.WriteUInt32(item.MaxDurability.GetValueOrDefault());
        }
        data.WriteUInt32(item.CreatePlayedTime.GetValueOrDefault());
        data.WriteInt32(0);
        data.WriteInt64(0L);
        if (IsOwner)
        {
            data.WriteUInt64(0uL);
            data.WriteUInt8(0);
        }
        data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        if (IsOwner)
            data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        if (IsOwner)
            data.WriteUInt16(0);
        data.WriteBits(0u, 6);
        data.FlushBits();
    }

    private void WriteEmptyItemCreate(WorldPacket data)
    {
        for (int i = 0; i < 4; i++)
            data.WritePackedGuid128(WowGuid128.Empty);
        if (IsOwner)
        {
            data.WriteUInt32(0u);
            data.WriteUInt32(0u);
            for (int j = 0; j < 5; j++)
                data.WriteInt32(0);
        }
        data.WriteUInt32(0u);
        for (int k = 0; k < 13; k++)
        {
            data.WriteInt32(0);
            data.WriteUInt32(0u);
            data.WriteInt16(0);
            data.WriteUInt16(0);
        }
        data.WriteInt32(0);
        data.WriteInt32(0);
        if (IsOwner)
        {
            data.WriteUInt32(0u);
            data.WriteUInt32(0u);
        }
        data.WriteUInt32(0u);
        data.WriteInt32(0);
        data.WriteInt64(0L);
        if (IsOwner)
        {
            data.WriteUInt64(0uL);
            data.WriteUInt8(0);
        }
        data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        if (IsOwner)
            data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        if (IsOwner)
            data.WriteUInt16(0);
        data.WriteBits(0u, 6);
        data.FlushBits();
    }

    private void WriteCreateContainerData(WorldPacket data)
    {
        var container = _updateData.ContainerData;
        for (int i = 0; i < 36; i++)
            data.WritePackedGuid128(container?.Slots[i] ?? WowGuid128.Empty);
        data.WriteUInt32((container?.NumSlots).GetValueOrDefault());
    }

    private void WriteCreateUnitData(WorldPacket data)
    {
        var unit = _updateData.UnitData ?? new UnitData();
        data.WriteInt64(unit.Health.GetValueOrDefault());
        data.WriteInt64(unit.MaxHealth.GetValueOrDefault());
        data.WriteInt32(unit.DisplayID.GetValueOrDefault());
        for (int i = 0; i < 2; i++)
            data.WriteUInt32(unit.NpcFlags?[i].GetValueOrDefault() ?? 0);
        data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        data.WritePackedGuid128(unit.Charm ?? WowGuid128.Empty);
        data.WritePackedGuid128(unit.Summon ?? WowGuid128.Empty);
        if (IsOwner)
            data.WritePackedGuid128(unit.Critter ?? WowGuid128.Empty);
        data.WritePackedGuid128(unit.CharmedBy ?? WowGuid128.Empty);
        data.WritePackedGuid128(unit.SummonedBy ?? WowGuid128.Empty);
        data.WritePackedGuid128(unit.CreatedBy ?? WowGuid128.Empty);
        data.WritePackedGuid128(WowGuid128.Empty);
        data.WritePackedGuid128(WowGuid128.Empty);
        data.WritePackedGuid128(unit.Target ?? WowGuid128.Empty);
        data.WritePackedGuid128(WowGuid128.Empty);
        data.WriteUInt64(0uL);
        data.WriteInt32(unit.ChannelData?.SpellID ?? 0);
        data.WriteInt32(unit.ChannelData?.SpellXSpellVisualID ?? 0);
        data.WriteUInt32(0u);
        data.WriteUInt8(unit.RaceId.GetValueOrDefault());
        data.WriteUInt8(unit.ClassId.GetValueOrDefault());
        data.WriteUInt8(unit.PlayerClassId.GetValueOrDefault());
        data.WriteUInt8(unit.SexId.GetValueOrDefault());
        // DisplayPower (PowerType enum: Mana=0/Rage=1/Focus=2/Energy=3/...).
        // Was hardcoded to 0 (Mana), so warriors saw an empty rage bar — the
        // V3_4_3 client UI binds the player power widget to the slot matching
        // PowerType=DisplayPower for that class. With DisplayPower=0 on a
        // warrior, the bar reads the (nonexistent) mana slot and stays empty.
        // CypherCore native-V3_4_3 capture confirmed value should be 1 for
        // warriors. Reader already populates unit.DisplayPower correctly from
        // UNIT_FIELD_BYTES_0 at UpdateHandler.cs:1875.
        data.WriteUInt8((byte)unit.DisplayPower.GetValueOrDefault());
        data.WriteUInt32(0u);
        if (IsOwner)
        {
            for (int j = 0; j < 10; j++)
            {
                data.WriteFloat(0f);
                data.WriteFloat(0f);
            }
        }
        for (int k = 0; k < 10; k++)
        {
            data.WriteInt32(k < 7 ? unit.Power[k].GetValueOrDefault() : 0);
            data.WriteInt32(k < 7 ? unit.MaxPower[k].GetValueOrDefault() : 0);
            data.WriteFloat(0f);
        }
        data.WriteInt32(unit.Level.GetValueOrDefault());
        data.WriteInt32(unit.EffectiveLevel ?? unit.Level.GetValueOrDefault());
        data.WriteInt32(unit.ContentTuningID.GetValueOrDefault());
        data.WriteInt32(unit.ScalingLevelMin.GetValueOrDefault());
        data.WriteInt32(unit.ScalingLevelMax.GetValueOrDefault());
        data.WriteInt32(unit.ScalingLevelDelta.GetValueOrDefault());
        data.WriteInt32(0);
        data.WriteInt32(0);
        data.WriteInt32(0);
        data.WriteInt32(unit.FactionTemplate.GetValueOrDefault());
        for (int l = 0; l < 3; l++)
        {
            int vItemId = unit.VirtualItems != null && unit.VirtualItems[l] is VisibleItem vi ? vi.ItemID : 0;
            // Players don't populate VirtualItems on the server side (they use PLAYER_VISIBLE_ITEM
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
        data.WriteUInt32(unit.Flags.GetValueOrDefault());
        data.WriteUInt32(unit.Flags2.GetValueOrDefault());
        data.WriteUInt32(0u);
        data.WriteUInt32(unit.AuraState.GetValueOrDefault());
        for (int m = 0; m < 2; m++)
            data.WriteUInt32(unit.AttackRoundBaseTime?[m].GetValueOrDefault() ?? 0);
        if (IsOwner)
        {
            uint rangedTime = unit.RangedAttackRoundBaseTime.GetValueOrDefault();
            // If the server didn't send a ranged attack time but the player has a ranged weapon
            // visible, default to 2300ms (standard bow speed) so the client enables Auto Shot.
            if (rangedTime == 0 && _updateData.PlayerData?.VisibleItems != null
                && _updateData.PlayerData.VisibleItems.Length > 17
                && _updateData.PlayerData.VisibleItems[17] is VisibleItem ranged && ranged.ItemID != 0)
            {
                rangedTime = 2300;
            }
            data.WriteUInt32(rangedTime);
        }
        data.WriteFloat(unit.BoundingRadius ?? 0.389f);
        data.WriteFloat(unit.CombatReach ?? 1.5f);
        data.WriteFloat(1f);
        data.WriteInt32(unit.NativeDisplayID.GetValueOrDefault());
        data.WriteFloat(1f);
        data.WriteInt32(unit.MountDisplayID.GetValueOrDefault());
        if (IsOwner)
        {
            data.WriteFloat(unit.MinDamage.GetValueOrDefault());
            data.WriteFloat(unit.MaxDamage.GetValueOrDefault());
            data.WriteFloat(unit.MinOffHandDamage.GetValueOrDefault());
            data.WriteFloat(unit.MaxOffHandDamage.GetValueOrDefault());
        }
        data.WriteUInt8(unit.StandState.GetValueOrDefault());
        data.WriteUInt8(unit.PetLoyaltyIndex.GetValueOrDefault());
        data.WriteUInt8(unit.VisFlags.GetValueOrDefault());
        data.WriteUInt8(unit.AnimTier.GetValueOrDefault());
        data.WriteUInt32(unit.PetNumber.GetValueOrDefault());
        data.WriteUInt32(unit.PetNameTimestamp.GetValueOrDefault());
        data.WriteUInt32(unit.PetExperience.GetValueOrDefault());
        data.WriteUInt32(unit.PetNextLevelExperience.GetValueOrDefault());
        data.WriteFloat(unit.ModCastSpeed ?? 1f);
        data.WriteFloat(unit.ModCastHaste ?? 1f);
        data.WriteFloat(1f);
        data.WriteFloat(1f);
        data.WriteFloat(1f);
        data.WriteFloat(1f);
        data.WriteInt32(unit.CreatedBySpell.GetValueOrDefault());
        data.WriteInt32(unit.EmoteState.GetValueOrDefault());
        data.WriteInt16(0);
        data.WriteInt16(0);
        if (IsOwner)
        {
            for (int n = 0; n < 5; n++)
            {
                data.WriteInt32(unit.Stats?[n].GetValueOrDefault() ?? 0);
                data.WriteInt32(unit.StatPosBuff?[n].GetValueOrDefault() ?? 0);
                data.WriteInt32(unit.StatNegBuff?[n].GetValueOrDefault() ?? 0);
            }
        }
        if (IsOwner)
        {
            for (int r = 0; r < 7; r++)
                data.WriteInt32(unit.Resistances?[r].GetValueOrDefault() ?? 0);
        }
        if (IsOwner)
        {
            for (int p = 0; p < 7; p++)
            {
                data.WriteInt32(unit.PowerCostModifier?[p].GetValueOrDefault() ?? 0);
                data.WriteFloat(unit.PowerCostMultiplier?[p].GetValueOrDefault() ?? 0f);
            }
        }
        for (int b = 0; b < 7; b++)
        {
            data.WriteInt32(unit.ResistanceBuffModsPositive?[b].GetValueOrDefault() ?? 0);
            data.WriteInt32(unit.ResistanceBuffModsNegative?[b].GetValueOrDefault() ?? 0);
        }
        data.WriteInt32(unit.BaseMana.GetValueOrDefault());
        if (IsOwner)
            data.WriteInt32(unit.BaseHealth.GetValueOrDefault());
        data.WriteUInt8(unit.SheatheState.GetValueOrDefault());
        data.WriteUInt8(unit.PvpFlags.GetValueOrDefault());
        data.WriteUInt8(unit.PetFlags.GetValueOrDefault());
        data.WriteUInt8(unit.ShapeshiftForm.GetValueOrDefault());
        if (IsOwner)
        {
            data.WriteInt32(unit.AttackPower.GetValueOrDefault());
            data.WriteInt32(unit.AttackPowerModPos.GetValueOrDefault());
            data.WriteInt32(unit.AttackPowerModNeg.GetValueOrDefault());
            data.WriteFloat(unit.AttackPowerMultiplier.GetValueOrDefault());
            data.WriteInt32(unit.RangedAttackPower.GetValueOrDefault());
            data.WriteInt32(unit.RangedAttackPowerModPos.GetValueOrDefault());
            data.WriteInt32(unit.RangedAttackPowerModNeg.GetValueOrDefault());
            data.WriteFloat(unit.RangedAttackPowerMultiplier.GetValueOrDefault());
            data.WriteInt32(0);
            data.WriteFloat(0f);
            data.WriteFloat(unit.MinRangedDamage.GetValueOrDefault());
            data.WriteFloat(unit.MaxRangedDamage.GetValueOrDefault());
            data.WriteFloat(unit.MaxHealthModifier ?? 1f);
        }
        data.WriteFloat(unit.HoverHeight.GetValueOrDefault());
        data.WriteInt32(unit.MinItemLevelCutoff.GetValueOrDefault());
        data.WriteInt32(unit.MinItemLevel.GetValueOrDefault());
        data.WriteInt32(unit.MaxItemLevel.GetValueOrDefault());
        data.WriteInt32(unit.WildBattlePetLevel.GetValueOrDefault());
        data.WriteUInt32(0u);
        data.WriteInt32(unit.InteractSpellID.GetValueOrDefault());
        data.WriteInt32(0);
        data.WriteInt32(unit.LooksLikeMountID.GetValueOrDefault());
        data.WriteInt32(unit.LooksLikeCreatureID.GetValueOrDefault());
        data.WriteInt32(unit.LookAtControllerID.GetValueOrDefault());
        data.WriteInt32(0);
        data.WritePackedGuid128(unit.GuildGUID ?? WowGuid128.Empty);
        data.WriteUInt32(0u);                                   // PassiveSpells.size()
        data.WriteUInt32(0u);                                   // WorldEffects.size()
        // ChannelObjects.size() — DynamicUpdateField<ObjectGuid,0,4> in TC.
        // Without this count + the matching GUID body at the end of the
        // create block, the V3_4_3 client receives an empty channel target
        // list and drops the channel-loop animation after the start anim
        // finishes (Drain Soul / Mind Flay went idle-pose mid-channel).
        // Legacy 3.3.5a server publishes the target via UNIT_FIELD_CHANNEL_OBJECT;
        // the reader populates UnitData.ChannelObject at UpdateHandler.cs:1918.
        bool hasChannelObject = unit.ChannelObject.HasValue && !unit.ChannelObject.Value.IsEmpty();
        data.WriteUInt32(hasChannelObject ? 1u : 0u);           // ChannelObjects.size()
        data.WritePackedGuid128(WowGuid128.Empty);
        data.WriteInt32(0);
        data.WriteFloat(0f);
        data.WriteUInt32(0u);
        if (IsOwner)
            data.WritePackedGuid128(WowGuid128.Empty);
        // ChannelObjects body (TC UpdateFields.cpp:856-859). PassiveSpells and
        // WorldEffects bodies are size-0 so emit nothing; only the channel
        // target GUID is written here when present.
        if (hasChannelObject)
            data.WritePackedGuid128(unit.ChannelObject!.Value);
    }

    private void WriteCreatePlayerData(WorldPacket data)
    {
        var player = _updateData.PlayerData ?? new PlayerData();
        data.WritePackedGuid128(player.DuelArbiter ?? WowGuid128.Empty);
        data.WritePackedGuid128(player.WowAccount ?? WowGuid128.Empty);
        data.WritePackedGuid128(player.LootTargetGUID ?? WowGuid128.Empty);
        data.WriteUInt32(player.PlayerFlags.GetValueOrDefault());
        data.WriteUInt32(player.PlayerFlagsEx.GetValueOrDefault());
        data.WriteUInt32(player.GuildRankID.GetValueOrDefault());
        data.WriteUInt32(player.GuildDeleteDate.GetValueOrDefault());
        data.WriteInt32(player.GuildLevel.GetValueOrDefault());

        int customizationCount = 0;
        for (int i = 0; i < player.Customizations.Length; i++)
        {
            if (player.Customizations[i] != null)
                customizationCount++;
        }
        data.WriteUInt32((uint)customizationCount);

        data.WriteUInt8(player.PartyType.GetValueOrDefault());
        data.WriteUInt8(0);
        data.WriteUInt8(player.NumBankSlots.GetValueOrDefault());
        data.WriteUInt8(player.NativeSex.GetValueOrDefault());
        data.WriteUInt8(player.Inebriation.GetValueOrDefault());
        data.WriteUInt8(player.PvpTitle.GetValueOrDefault());
        data.WriteUInt8(player.ArenaFaction.GetValueOrDefault());
        data.WriteUInt8(player.PvPRank.GetValueOrDefault());
        data.WriteInt32(0);
        data.WriteUInt32(player.DuelTeam.GetValueOrDefault());
        data.WriteInt32(player.GuildTimeStamp.GetValueOrDefault());

        // QuestLog[QuestConst.MaxQuestLogSize] — gated by PartyMember flag (0x02) in TC343.
        if (IsOwner)
        {
            int questCount = 0;
            System.Text.StringBuilder slotSummary = new();
            for (int q = 0; q < QuestConst.MaxQuestLogSize; q++)
            {
                var quest = player.QuestLog != null && q < player.QuestLog.Length ? player.QuestLog[q] : null;
                data.WriteInt64(quest?.EndTime ?? 0);
                data.WriteInt32(quest?.QuestID ?? 0);
                data.WriteUInt32(quest?.StateFlags ?? 0);
                for (int obj = 0; obj < 24; obj++)
                    data.WriteUInt16((ushort)(quest?.ObjectiveProgress[obj] ?? 0));
                if (quest != null && quest.QuestID.HasValue && quest.QuestID.Value != 0)
                {
                    questCount++;
                    slotSummary.Append($" [{q}]={quest.QuestID.Value}");
                }
            }
            Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
                $"[QuestLogCreate] populated={questCount} slots:{slotSummary}");
        }

        for (int j = 0; j < 19; j++)
        {
            if (player.VisibleItems != null && j < player.VisibleItems.Length
                && player.VisibleItems[j] is VisibleItem pv)
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

        data.WriteInt32(player.ChosenTitle.GetValueOrDefault());
        data.WriteInt32(0);
        data.WriteUInt32(player.VirtualPlayerRealm.GetValueOrDefault());
        data.WriteUInt32(player.CurrentSpecID.GetValueOrDefault());
        data.WriteInt32(0);
        for (int k = 0; k < 6; k++)
            data.WriteFloat(0f);
        data.WriteUInt8(0);
        data.WriteInt32(player.HonorLevel.GetValueOrDefault());
        // LogoutTime: TC writes a real Unix timestamp; we previously sent 0, which the client
        // may interpret as "you are mid-logout". Use current Unix time as a sensible default.
        data.WriteInt64(IsOwner ? (long)Time.UnixTime : 0L);
        data.WriteUInt32(0u);
        data.WriteInt32(0);
        // BnetAccount: TC populates with the real BNet account GUID for the local player.
        // Empty-stubbed previously, which means the V3_4_3 client cannot bind the player to
        // an account — likely contributes to the post-CreateObject world-ready stall.
        data.WritePackedGuid128(IsOwner
            ? _gameState.GlobalSession.GetBnetAccountGuidForPlayer(_updateData.Guid)
            : WowGuid128.Empty);
        data.WriteUInt32(0u);
        for (int l = 0; l < 19; l++)
            data.WriteUInt32(0u);
        for (int m = 0; m < player.Customizations.Length; m++)
        {
            var choice = player.Customizations[m];
            if (choice != null)
            {
                data.WriteUInt32(choice.ChrCustomizationOptionID);
                data.WriteUInt32(choice.ChrCustomizationChoiceID);
            }
        }
        data.WriteFloat(0f);
        data.WriteFloat(0f);
        data.WriteUInt32(0u);
    }

    private static void WriteEmptyQuestLog(WorldPacket data)
    {
        data.WriteInt64(0L);
        data.WriteInt32(0);
        data.WriteUInt32(0u);
        for (int i = 0; i < 24; i++)
            data.WriteUInt16(0);
    }

    // Maps the modern 3.4.3 InvSlots index (0-140) to the corresponding legacy slot
    // arrays on ActivePlayerData. Returns null when the modern slot has no legacy
    // equivalent or the entry is missing.
    private static WowGuid128? GetModernInvSlot(ActivePlayerData a, int modernIdx)
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

    private void WriteCreateActivePlayerData(WorldPacket data)
    {
        var active = _updateData.ActivePlayerData ?? new ActivePlayerData();

        // InvSlots[141] mapped from legacy arrays via GetModernInvSlot. WPP's
        // V3_4_0_45166/UpdateFieldsHandler343.cs:2461 reads 141 entries for
        // V3_4_3.54261 — TC's wotlk_classic source bumped this to 146 in a
        // later build (V3_4_4+), but our client expects 141.
        for (int i = 0; i < 141; i++)
            data.WritePackedGuid128(GetModernInvSlot(active, i) ?? WowGuid128.Empty);

        data.WritePackedGuid128(active.FarsightObject ?? WowGuid128.Empty);
        data.WritePackedGuid128(WowGuid128.Empty);   // SummonedBattlePetGUID
        data.WriteUInt32(0u);                         // KnownTitles.size()
        data.WriteUInt64(active.Coinage.GetValueOrDefault());
        // No AccountBankCoinage in V3_4_3.54261 (added in a later build).
        data.WriteInt32(active.XP.GetValueOrDefault());
        data.WriteInt32(active.NextLevelXP.GetValueOrDefault());
        data.WriteInt32(0);

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

        data.WriteInt32(active.CharacterPoints.GetValueOrDefault());
        data.WriteInt32(active.MaxTalentTiers.GetValueOrDefault());
        data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        for (int z = 0; z < 12; z++)
            data.WriteFloat(0f);
        for (int k = 0; k < 7; k++)
        {
            data.WriteFloat(0f);
            data.WriteInt32(0);
            data.WriteInt32(0);
            data.WriteFloat(0f);
        }
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
        // ExploredZones[240] uint64 — V3_4_3.54261 still uses the explored-zones
        // bitmask array (per WPP V3_4_0/UpdateFieldsHandler343.cs:2509). TC's
        // wotlk_classic replaced this with a BitVectors struct in a later build,
        // but for our client this 1920-byte array is correct.
        for (int l = 0; l < 240; l++)
            data.WriteUInt64(0uL);

        // RestInfo[2] — each entry: uint32 Threshold, uint8 StateID.
        data.WriteUInt32(0u);
        data.WriteUInt8(1);
        data.WriteUInt32(0u);
        data.WriteUInt8(1);
        data.WriteInt32(0);
        data.WriteFloat(0f);
        data.WriteFloat(0f);
        data.WriteFloat(0f);
        for (int m = 0; m < 3; m++)
        {
            data.WriteFloat(1f);
            data.WriteFloat(1f);
        }
        data.WriteFloat(0f);
        data.WriteFloat(0f);
        data.WriteFloat(0f);
        data.WriteFloat(0f);
        data.WriteInt32(0);
        data.WriteInt32(0);
        data.WriteUInt32(0u);
        data.WriteUInt8(0);
        data.WriteUInt8(0);
        data.WriteUInt8(0);
        data.WriteUInt8(0);
        data.WriteInt32((int)active.AmmoID.GetValueOrDefault());
        data.WriteUInt32(0u);
        for (int n = 0; n < 12; n++)
        {
            data.WriteUInt32(0u);
            data.WriteInt64(0L);
        }
        for (int o = 0; o < 8; o++)
            data.WriteUInt16(0);
        for (int p = 0; p < 7; p++)
            data.WriteUInt32(0u);
        data.WriteInt32(active.WatchedFactionIndex ?? -1);
        for (int c = 0; c < 32; c++)
            data.WriteInt32(active.CombatRatings?[c].GetValueOrDefault() ?? 0);
        data.WriteInt32(active.MaxLevel ?? LegacyVersion.GetMaxLevel());
        data.WriteInt32(0);
        data.WriteInt32(0);
        for (int q = 0; q < 4; q++)
            data.WriteUInt32(0u);
        data.WriteInt32(active.PetSpellPower.GetValueOrDefault());
        for (int s = 0; s < 2; s++)
            data.WriteInt32(active.ProfessionSkillLine?[s].GetValueOrDefault() ?? 0);
        data.WriteFloat(0f);
        data.WriteFloat(0f);
        data.WriteInt32(0);
        data.WriteFloat(active.ModPetHaste ?? 1f);
        data.WriteUInt8(0);
        data.WriteUInt8(0);
        data.WriteUInt8(active.NumBackpackSlots ?? 16);
        data.WriteInt32(0);
        data.WriteInt32(0);
        data.WriteUInt16(0);
        data.WriteUInt32(0u);
        for (int b = 0; b < 4; b++)
            data.WriteUInt32(0u);
        for (int b = 0; b < 7; b++)
            data.WriteUInt32(0u);
        for (int qc = 0; qc < 875; qc++)
            data.WriteUInt64(0uL);
        data.WriteInt32(active.Honor.GetValueOrDefault());
        data.WriteInt32(active.HonorNextLevel ?? 5500);
        data.WriteInt32(0);
        data.WriteInt32((int?)active.PvPTierMaxFromWins ?? -1);
        data.WriteInt32((int?)active.PvPLastWeeksTierMaxFromWins ?? -1);
        data.WriteUInt8(0);
        data.WriteInt32(0);
        for (int u = 0; u < 16; u++)
            data.WriteUInt32(0u);
        data.WriteInt32(0);
        data.WriteUInt32(0u);
        data.WriteUInt32(0u);

        // GlyphSlots[6] / Glyphs[6] interleaved. WotLK GlyphSlot.db2 IDs.
        ReadOnlySpan<uint> glyphSlotIds = [21, 22, 23, 24, 25, 26];
        for (int g = 0; g < 6; g++)
        {
            data.WriteUInt32(glyphSlotIds[g]);
            data.WriteUInt32(_gameState.ActiveGlyphs[g]);
        }
        data.WriteUInt8(_gameState.GlyphsEnabled);
        data.WriteUInt8(0); // LfgRoles
        data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        data.WriteUInt8(0);
        for (int t = 0; t < 7; t++)
        {
            data.WriteInt8(0);
            for (int x = 0; x < 16; x++)
                data.WriteUInt32(0u);
            data.WriteBit(false);
            data.FlushBits();
        }
        data.FlushBits();
        data.WriteBit(false);
        data.WriteBit(false);
        data.WriteBit(false);
        data.FlushBits();
        data.WriteUInt32(0u);
        for (int e = 0; e < 8; e++)
            data.WriteInt32(0);
        data.WriteInt64(0L);
        data.WriteBit(false);
        data.FlushBits();
        data.FlushBits();
    }

    private void WriteCreateGameObjectData(WorldPacket data)
    {
        var go = _updateData.GameObjectData ?? new GameObjectData();
        data.WriteInt32(go.DisplayID.GetValueOrDefault());
        data.WriteUInt32(go.SpellVisualID.GetValueOrDefault());
        data.WriteUInt32(go.StateSpellVisualID.GetValueOrDefault());
        data.WriteUInt32(go.StateAnimID.GetValueOrDefault());
        data.WriteUInt32(go.StateAnimKitID.GetValueOrDefault());
        data.WriteUInt32(0u);
        data.WritePackedGuid128(go.CreatedBy ?? WowGuid128.Empty);
        data.WritePackedGuid128(WowGuid128.Empty);
        data.WriteUInt32(go.Flags.GetValueOrDefault());
        // ParentRotation = the stored quaternion of the GameObject (cMangos's
        // GAMEOBJECT_PARENTROTATION value, plumbed through UpdateHandler.cs's GO ingest
        // for V3_4_3+). For most static GOs this matches CypherCore's DB-stored value;
        // for runeblade entry 190584 it's identity (0,0,0,1), for runeforge entry 191747
        // it's (0,0,0.292,0.956). Falls back to identity only if no rotation field was
        // ever ingested (defensive — should not happen for V3_4_3 since the ingest now
        // always populates this from cMangos). ParentRotation is float?[4] X/Y/Z/W.
        if (go.ParentRotation != null
            && (go.ParentRotation[0].HasValue || go.ParentRotation[1].HasValue
                || go.ParentRotation[2].HasValue || go.ParentRotation[3].HasValue))
        {
            data.WriteFloat(go.ParentRotation[0].GetValueOrDefault(0f));
            data.WriteFloat(go.ParentRotation[1].GetValueOrDefault(0f));
            data.WriteFloat(go.ParentRotation[2].GetValueOrDefault(0f));
            data.WriteFloat(go.ParentRotation[3].GetValueOrDefault(1f));
        }
        else
        {
            data.WriteFloat(0f);
            data.WriteFloat(0f);
            data.WriteFloat(0f);
            data.WriteFloat(1f);
        }
        data.WriteInt32(go.FactionTemplate.GetValueOrDefault());
        data.WriteInt32(go.Level.GetValueOrDefault());
        data.WriteInt8(go.State.GetValueOrDefault());
        data.WriteInt8(go.TypeID.GetValueOrDefault());
        data.WriteUInt8(go.PercentHealth ?? 0);
        data.WriteUInt32(go.ArtKit.GetValueOrDefault());
        data.WriteUInt32(0u);
        data.WriteUInt32(go.CustomParam.GetValueOrDefault());
        data.WriteUInt32(0u);
    }

    private void WriteCreateDynamicObjectData(WorldPacket data)
    {
        var dyn = _updateData.DynamicObjectData ?? new DynamicObjectData();
        data.WritePackedGuid128(dyn.Caster ?? WowGuid128.Empty);
        data.WriteUInt8(0);
        data.WriteInt32(0);
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

    private static bool HasAnySkillChanged(SkillInfo s)
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
    private static void WriteUpdateSkillInfo(WorldPacket data, SkillInfo s)
    {
        if (s == null)
        {
            data.WriteUInt32(0);
            data.WriteBits(0, 25);
            data.FlushBits();
            return;
        }

        var skillBlocks = new uint[57];
        void SB(int bit) => skillBlocks[bit / 32] |= (1u << (bit % 32));

        bool anyChanged = false;
        for (int i = 0; i < 256; i++)
        {
            if (s.SkillLineID[i].HasValue) { SB(1 + i); anyChanged = true; }
            if (s.SkillStep[i].HasValue) { SB(257 + i); anyChanged = true; }
            if (s.SkillRank[i].HasValue) { SB(513 + i); anyChanged = true; }
            if (s.SkillStartingRank[i].HasValue) { SB(769 + i); anyChanged = true; }
            if (s.SkillMaxRank[i].HasValue) { SB(1025 + i); anyChanged = true; }
            if (s.SkillTempBonus[i].HasValue) { SB(1281 + i); anyChanged = true; }
            if (s.SkillPermBonus[i].HasValue) { SB(1537 + i); anyChanged = true; }
        }

        if (anyChanged)
            SB(0);

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
            if ((skillBlocks[(1 + i) / 32] & (1u << ((1 + i) % 32))) != 0)
                data.WriteUInt16(s.SkillLineID[i]!.Value);
            if ((skillBlocks[(257 + i) / 32] & (1u << ((257 + i) % 32))) != 0)
                data.WriteUInt16(s.SkillStep[i]!.Value);
            if ((skillBlocks[(513 + i) / 32] & (1u << ((513 + i) % 32))) != 0)
                data.WriteUInt16(s.SkillRank[i]!.Value);
            if ((skillBlocks[(769 + i) / 32] & (1u << ((769 + i) % 32))) != 0)
                data.WriteUInt16(s.SkillStartingRank[i]!.Value);
            if ((skillBlocks[(1025 + i) / 32] & (1u << ((1025 + i) % 32))) != 0)
                data.WriteUInt16(s.SkillMaxRank[i]!.Value);
            if ((skillBlocks[(1281 + i) / 32] & (1u << ((1281 + i) % 32))) != 0)
                data.WriteInt16(s.SkillTempBonus[i]!.Value);
            if ((skillBlocks[(1537 + i) / 32] & (1u << ((1537 + i) % 32))) != 0)
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
        bool hasObjectChanges = _objectTypeMask.HasAnyFlag(ObjectTypeMask.Object) && _updateData.ObjectData != null && (_updateData.ObjectData.EntryID.HasValue || _updateData.ObjectData.DynamicFlags.HasValue || _updateData.ObjectData.Scale.HasValue);
        bool hasUnitChanges = _objectTypeMask.HasAnyFlag(ObjectTypeMask.Unit) && _updateData.UnitData != null && HasAnyUnitFieldSet();
        bool hasItemChanges = _objectTypeMask.HasAnyFlag(ObjectTypeMask.Item) && _updateData.ItemData != null;
        bool hasContainerChanges = _objectTypeMask.HasAnyFlag(ObjectTypeMask.Container) && _updateData.ContainerData != null && HasAnyContainerFieldSet();
        bool hasActivePlayerChanges = HasActivePlayerChanges();
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

    private bool HasAnyContainerFieldSet()
    {
        var c = _updateData.ContainerData;
        if (c == null) return false;
        if (c.NumSlots.HasValue) return true;
        for (int i = 0; i < 36; i++)
            if (c.Slots[i].HasValue) return true;
        return false;
    }

    // === HasAnyUnitFieldSet (fork lines 1140-1196) ===
    private bool HasAnyUnitFieldSet()
    {
        UnitData u = _updateData.UnitData;
        if (u == null) return false;
        if (u.Health.HasValue || u.MaxHealth.HasValue || u.DisplayID.HasValue) return true;
        if (u.Charm != null || u.Summon != null || u.CharmedBy != null) return true;
        if (u.SummonedBy != null || u.CreatedBy != null || u.Target != null) return true;
        if (u.ChannelData != null) return true;
        if (u.RaceId.HasValue || u.ClassId.HasValue || u.SexId.HasValue) return true;
        if (u.Level.HasValue || u.EffectiveLevel.HasValue || u.DisplayPower.HasValue) return true;
        if (u.FactionTemplate.HasValue || u.Flags.HasValue || u.Flags2.HasValue || u.Flags3.HasValue) return true;
        if (u.AuraState.HasValue || u.OverrideDisplayPowerID.HasValue) return true;
        if (u.BoundingRadius.HasValue || u.CombatReach.HasValue) return true;
        if (u.DisplayScale.HasValue || u.NativeXDisplayScale.HasValue) return true;
        if (u.NativeDisplayID.HasValue || u.MountDisplayID.HasValue) return true;
        if (u.HoverHeight.HasValue || u.GuildGUID != null) return true;
        if (u.NpcFlags != null)
            for (int i = 0; i < u.NpcFlags.Length; i++)
                if (u.NpcFlags[i].HasValue && u.NpcFlags[i] != 0) return true;
        if (u.Power != null)
            for (int i = 0; i < u.Power.Length; i++)
                if (u.Power[i].HasValue) return true;
        if (u.MaxPower != null)
            for (int i = 0; i < u.MaxPower.Length; i++)
                if (u.MaxPower[i].HasValue) return true;
        // Block 1 continued + Block 2 combat stats
        if (u.MinDamage.HasValue || u.MaxDamage.HasValue || u.MinOffHandDamage.HasValue || u.MaxOffHandDamage.HasValue) return true;
        if (u.StandState.HasValue || u.VisFlags.HasValue || u.AnimTier.HasValue) return true;
        if (u.ModCastSpeed.HasValue || u.ModCastHaste.HasValue || u.EmoteState.HasValue) return true;
        if (u.SheatheState.HasValue || u.ShapeshiftForm.HasValue) return true;
        if (u.AttackPower.HasValue || u.AttackPowerModPos.HasValue || u.AttackPowerModNeg.HasValue) return true;
        if (u.RangedAttackPower.HasValue || u.BaseMana.HasValue || u.BaseHealth.HasValue) return true;
        // Block 5: Stats
        if (u.Stats != null)
            for (int i = 0; i < u.Stats.Length; i++)
                if (u.Stats[i].HasValue) return true;
        if (u.StatPosBuff != null)
            for (int i = 0; i < u.StatPosBuff.Length; i++)
                if (u.StatPosBuff[i].HasValue) return true;
        if (u.StatNegBuff != null)
            for (int i = 0; i < u.StatNegBuff.Length; i++)
                if (u.StatNegBuff[i].HasValue) return true;
        // Blocks 5-6: Resistances
        if (u.Resistances != null)
            for (int i = 0; i < 7; i++)
                if (u.Resistances[i].HasValue) return true;
        if (u.ResistanceBuffModsPositive != null)
            for (int i = 0; i < 7; i++)
                if (u.ResistanceBuffModsPositive[i].HasValue) return true;
        if (u.ResistanceBuffModsNegative != null)
            for (int i = 0; i < 7; i++)
                if (u.ResistanceBuffModsNegative[i].HasValue) return true;
        if (u.AttackRoundBaseTime != null)
            for (int i = 0; i < u.AttackRoundBaseTime.Length; i++)
                if (u.AttackRoundBaseTime[i].HasValue) return true;
        return false;
    }

    // === HasAnyPlayerFieldSet (fork lines 2637-2659) ===
    private bool HasAnyPlayerFieldSet()
    {
        PlayerData p = _updateData.PlayerData;
        if (p == null) return false;
        // Scalar fields (bits 4-31)
        if (p.DuelArbiter != null || p.WowAccount != null || p.LootTargetGUID != null) return true;
        if (p.PlayerFlags.HasValue || p.PlayerFlagsEx.HasValue) return true;
        if (p.GuildRankID.HasValue || p.GuildDeleteDate.HasValue || p.GuildLevel.HasValue) return true;
        if (p.NumBankSlots.HasValue || p.NativeSex.HasValue || p.Inebriation.HasValue) return true;
        if (p.PvpTitle.HasValue || p.ArenaFaction.HasValue || p.PvPRank.HasValue) return true;
        if (p.DuelTeam.HasValue || p.GuildTimeStamp.HasValue || p.ChosenTitle.HasValue) return true;
        if (p.FakeInebriation.HasValue || p.VirtualPlayerRealm.HasValue || p.CurrentSpecID.HasValue) return true;
        if (p.HonorLevel.HasValue) return true;
        // Quest log (bits 35-60)
        if (p.QuestLog != null)
            for (int i = 0; i < p.QuestLog.Length; i++)
                if (p.QuestLog[i] != null && p.QuestLog[i].QuestID.HasValue) return true;
        // Visible items (bits 61-80)
        if (p.VisibleItems != null)
            for (int i = 0; i < p.VisibleItems.Length; i++)
                if (p.VisibleItems[i] != null) return true;
        return false;
    }

    // === HasAnyGameObjectFieldSet (fork lines 3097-3111) ===
    private bool HasAnyGameObjectFieldSet()
    {
        GameObjectData go = _updateData.GameObjectData;
        if (go == null) return false;
        if (go.DisplayID.HasValue || go.SpellVisualID.HasValue || go.StateSpellVisualID.HasValue) return true;
        if (go.StateAnimID.HasValue || go.StateAnimKitID.HasValue) return true;
        if (go.CreatedBy != null || go.GuildGUID != null) return true;
        if (go.Flags.HasValue || go.FactionTemplate.HasValue || go.Level.HasValue) return true;
        if (go.State.HasValue || go.TypeID.HasValue || go.PercentHealth.HasValue) return true;
        if (go.ArtKit.HasValue || go.CustomParam.HasValue) return true;
        if (go.ParentRotation != null)
            for (int i = 0; i < 4; i++)
                if (go.ParentRotation[i].HasValue) return true;
        return false;
    }

    // === HasActivePlayerChanges (fork lines 340-485) ===
    private bool HasActivePlayerChanges()
    {
        if (!_objectTypeMask.HasAnyFlag(ObjectTypeMask.ActivePlayer))
            return false;
        ActivePlayerData a = _updateData.ActivePlayerData;
        if (a == null) return false;

        // Block 0 scalars (bits 26-37)
        if (a.FarsightObject != null) return true;
        if (a.Coinage.HasValue || a.XP.HasValue || a.NextLevelXP.HasValue || a.TrialXP.HasValue) return true;
        if (a.CharacterPoints.HasValue || a.MaxTalentTiers.HasValue) return true;
        if (a.TrackCreatureMask.HasValue) return true;
        if (a.MainhandExpertise.HasValue || a.OffhandExpertise.HasValue) return true;

        // Block 38 scalars (bits 39-69)
        if (a.RangedExpertise.HasValue || a.CombatRatingExpertise.HasValue) return true;
        if (a.BlockPercentage.HasValue || a.DodgePercentage.HasValue || a.ParryPercentage.HasValue) return true;
        if (a.CritPercentage.HasValue || a.RangedCritPercentage.HasValue || a.OffhandCritPercentage.HasValue) return true;
        if (a.ShieldBlock.HasValue || a.Mastery.HasValue) return true;
        if (a.Speed.HasValue || a.Avoidance.HasValue || a.Sturdiness.HasValue) return true;
        if (a.Versatility.HasValue || a.VersatilityBonus.HasValue) return true;
        if (a.PvpPowerDamage.HasValue || a.PvpPowerHealing.HasValue) return true;
        if (a.ModHealingDonePos.HasValue || a.ModHealingPercent.HasValue) return true;
        if (a.ModHealingDonePercent.HasValue || a.ModPeriodicHealingDonePercent.HasValue) return true;
        if (a.ModSpellPowerPercent.HasValue || a.ModResiliencePercent.HasValue) return true;
        if (a.OverrideSpellPowerByAPPercent.HasValue || a.OverrideAPBySpellPowerPercent.HasValue) return true;
        if (a.ModTargetResistance.HasValue || a.ModTargetPhysicalResistance.HasValue) return true;
        if (a.LocalFlags.HasValue) return true;

        // Block 70 scalars (bits 71-101)
        if (a.GrantableLevels.HasValue || a.MultiActionBars.HasValue) return true;
        if (a.LifetimeMaxRank.HasValue || a.NumRespecs.HasValue) return true;
        if (a.AmmoID.HasValue || a.PvpMedals.HasValue) return true;
        if (a.TodayHonorableKills.HasValue || a.TodayDishonorableKills.HasValue) return true;
        if (a.YesterdayHonorableKills.HasValue || a.YesterdayDishonorableKills.HasValue) return true;
        if (a.LastWeekHonorableKills.HasValue || a.LastWeekDishonorableKills.HasValue) return true;
        if (a.ThisWeekHonorableKills.HasValue || a.ThisWeekDishonorableKills.HasValue) return true;
        if (a.ThisWeekContribution.HasValue || a.LifetimeHonorableKills.HasValue || a.LifetimeDishonorableKills.HasValue) return true;
        if (a.YesterdayContribution.HasValue || a.LastWeekContribution.HasValue || a.LastWeekRank.HasValue) return true;
        if (a.WatchedFactionIndex.HasValue || a.MaxLevel.HasValue) return true;
        if (a.ScalingPlayerLevelDelta.HasValue || a.MaxCreatureScalingLevel.HasValue) return true;
        if (a.PetSpellPower.HasValue || a.UiHitModifier.HasValue || a.UiSpellHitModifier.HasValue) return true;
        if (a.HomeRealmTimeOffset.HasValue || a.ModPetHaste.HasValue || a.LocalRegenFlags.HasValue) return true;

        // Block 102 scalars (bits 103-123)
        if (a.AuraVision.HasValue || a.NumBackpackSlots.HasValue) return true;
        if (a.OverrideSpellsID.HasValue || a.LfgBonusFactionID.HasValue || a.LootSpecID.HasValue) return true;
        if (a.OverrideZonePVPType.HasValue) return true;
        if (a.Honor.HasValue || a.HonorNextLevel.HasValue) return true;
        if (a.PvPTierMaxFromWins.HasValue || a.PvPLastWeeksTierMaxFromWins.HasValue) return true;
        if (a.PvPRankProgress.HasValue) return true;

        // Skill (bit 32) — nested SkillInfo struct
        if (a.Skill != null && HasAnySkillChanged(a.Skill)) return true;

        // KnownTitles (dynamic field, bit 3)
        if (a.KnownTitles != null)
            for (int i = 0; i < a.KnownTitles.Length; i++)
                if (a.KnownTitles[i].HasValue) return true;

        // InvSlots (bits 124-265)
        for (int i = 0; i < 141; i++)
            if (GetModernInvSlot(a, i) != null) return true;

        // TrackResourceMask (bits 266-268)
        if (a.TrackResourceMask != null)
            for (int i = 0; i < a.TrackResourceMask.Length; i++)
                if (a.TrackResourceMask[i].HasValue) return true;

        // SpellCritPercentage / ModDamageDone arrays (bits 269-297)
        if (a.SpellCritPercentage != null)
            for (int i = 0; i < 7; i++)
                if (a.SpellCritPercentage[i].HasValue) return true;
        if (a.ModDamageDonePos != null)
            for (int i = 0; i < 7; i++)
                if (a.ModDamageDonePos[i].HasValue) return true;
        if (a.ModDamageDoneNeg != null)
            for (int i = 0; i < 7; i++)
                if (a.ModDamageDoneNeg[i].HasValue) return true;
        if (a.ModDamageDonePercent != null)
            for (int i = 0; i < 7; i++)
                if (a.ModDamageDonePercent[i].HasValue) return true;

        // ExploredZones (bits 298-538)
        if (a.ExploredZones != null)
            for (int i = 0; i < 240; i++)
                if (a.ExploredZones[i].HasValue) return true;

        // RestInfo (bits 539-541)
        if (a.RestInfo != null)
            for (int i = 0; i < 2; i++)
                if (a.RestInfo[i] != null && (a.RestInfo[i].Threshold.HasValue || a.RestInfo[i].StateID.HasValue)) return true;

        // WeaponDmgMultipliers / WeaponAtkSpeedMultipliers (bits 542-548)
        if (a.WeaponDmgMultipliers != null)
            for (int i = 0; i < 3; i++)
                if (a.WeaponDmgMultipliers[i].HasValue) return true;
        if (a.WeaponAtkSpeedMultipliers != null)
            for (int i = 0; i < 3; i++)
                if (a.WeaponAtkSpeedMultipliers[i].HasValue) return true;

        // Buyback (bits 549-573)
        if (a.BuybackPrice != null)
            for (int i = 0; i < 12; i++)
                if (a.BuybackPrice[i].HasValue) return true;
        if (a.BuybackTimestamp != null)
            for (int i = 0; i < 12; i++)
                if (a.BuybackTimestamp[i].HasValue) return true;

        // CombatRatings (bits 574-606)
        if (a.CombatRatings != null)
            for (int i = 0; i < 32; i++)
                if (a.CombatRatings[i].HasValue) return true;

        // NoReagentCostMask (bits 615-619)
        if (a.NoReagentCostMask != null)
            for (int i = 0; i < 4; i++)
                if (a.NoReagentCostMask[i].HasValue) return true;

        // ProfessionSkillLine (bits 620-622)
        if (a.ProfessionSkillLine != null)
            for (int i = 0; i < 2; i++)
                if (a.ProfessionSkillLine[i].HasValue) return true;

        // BagSlotFlags (bits 623-627)
        if (a.BagSlotFlags != null)
            for (int i = 0; i < 4; i++)
                if (a.BagSlotFlags[i].HasValue) return true;

        // BankBagSlotFlags (bits 628-635)
        if (a.BankBagSlotFlags != null)
            for (int i = 0; i < 7; i++)
                if (a.BankBagSlotFlags[i].HasValue) return true;

        // QuestCompleted (bits 636-1511)
        if (a.QuestCompleted != null)
            for (int i = 0; i < 875; i++)
                if (a.QuestCompleted[i].HasValue) return true;

        // PvpInfo (bits 607-614)
        if (a.PvpInfo != null)
            for (int i = 0; i < a.PvpInfo.Length; i++)
                if (a.PvpInfo[i] != null && (a.PvpInfo[i].Rating != 0 || a.PvpInfo[i].SeasonPlayed != 0 || a.PvpInfo[i].Disqualified)) return true;

        return false;
    }

    // (HasAnySkillChanged already defined earlier in this file at the create-path
    //  level; no duplicate needed for the update path.)

    // === WriteUpdateObjectData (fork lines 1391-1428) ===
    private void WriteUpdateObjectData(WorldPacket data)
    {
        ObjectData obj = _updateData.ObjectData;
        uint mask = 0u;
        if (obj.EntryID.HasValue)
        {
            mask |= 2;
        }
        if (obj.DynamicFlags.HasValue)
        {
            mask |= 4;
        }
        if (obj.Scale.HasValue)
        {
            mask |= 8;
        }
        if (mask != 0)
        {
            mask |= 1;
        }
        data.WriteBits(mask, 4);
        data.FlushBits();
        if ((mask & 1) != 0)
        {
            if (obj.EntryID.HasValue)
            {
                data.WriteInt32(obj.EntryID.Value);
            }
            if (obj.DynamicFlags.HasValue)
            {
                data.WriteUInt32(obj.DynamicFlags.Value);
            }
            if (obj.Scale.HasValue)
            {
                data.WriteFloat(obj.Scale.Value);
            }
            Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
                $"[ObjectData write] guid={_updateData.Guid} typeMask={_objectTypeMask} mask=0x{mask:X1} " +
                $"entry={(obj.EntryID.HasValue ? obj.EntryID.Value.ToString() : "—")} " +
                $"dynFlags={(obj.DynamicFlags.HasValue ? "0x" + obj.DynamicFlags.Value.ToString("X8") : "—")} " +
                $"scale={(obj.Scale.HasValue ? obj.Scale.Value.ToString("F4") : "—")}");
        }
    }

    // === WriteUpdateUnitData (fork lines 1902-2518) ===
    private void WriteUpdateUnitData(WorldPacket data)
    {
        UnitData unit = _updateData.UnitData;
        if (unit == null)
        {
            data.WriteBits(0, 8);
            data.FlushBits();
            data.FlushBits();
            return;
        }
        uint[] blockMasks = new uint[8];
        if (unit.Health.HasValue)
        {
            SetBit(5);
        }
        if (unit.MaxHealth.HasValue)
        {
            SetBit(6);
        }
        if (unit.DisplayID.HasValue)
        {
            SetBit(7);
        }
        if (unit.Charm != null)
        {
            SetBit(11);
        }
        if (unit.Summon != null)
        {
            SetBit(12);
        }
        if (unit.CharmedBy != null)
        {
            SetBit(14);
        }
        if (unit.SummonedBy != null)
        {
            SetBit(15);
        }
        if (unit.CreatedBy != null)
        {
            SetBit(16);
        }
        if (unit.Target != null)
        {
            SetBit(19);
        }
        if (unit.ChannelData != null)
        {
            SetBit(22);
        }
        // ChannelObjects DynamicUpdateField — bit 4 of UnitData changesMask.
        // Sustains the channel-loop animation on V3_4_3 client by populating
        // the channel-target list. Reader at UpdateHandler.cs:1918 only assigns
        // ChannelObject when the legacy mask bit is set, so this naturally
        // covers both channel-start (target GUID) and channel-end (Empty GUID).
        if (unit.ChannelObject != null) SetBit(4);
        if (unit.RaceId.HasValue)
        {
            SetBit(24);
        }
        if (unit.ClassId.HasValue)
        {
            SetBit(25);
        }
        if (unit.SexId.HasValue)
        {
            SetBit(27);
        }
        if (unit.DisplayPower.HasValue) SetBit(28);
        if (unit.Level.HasValue)
        {
            SetBit(30);
        }
        if (unit.EffectiveLevel.HasValue)
        {
            SetBit(31);
        }
        if (unit.FactionTemplate.HasValue)
        {
            SetBit(40);
        }
        if (unit.Flags.HasValue)
        {
            SetBit(41);
        }
        if (unit.Flags2.HasValue)
        {
            SetBit(42);
        }
        if (unit.Flags3.HasValue) SetBit(43);
        if (unit.AuraState.HasValue)
        {
            SetBit(44);
        }
        if (unit.OverrideDisplayPowerID.HasValue) SetBit(45);
        if (unit.BoundingRadius.HasValue)
        {
            SetBit(46);
        }
        if (unit.CombatReach.HasValue)
        {
            SetBit(47);
        }
        if (unit.DisplayScale.HasValue) SetBit(48);
        if (unit.NativeDisplayID.HasValue)
        {
            SetBit(49);
        }
        if (unit.NativeXDisplayScale.HasValue) SetBit(50);
        if (unit.MountDisplayID.HasValue)
        {
            SetBit(51);
        }
        // Block 1 continued: damage, stance bytes, pet fields
        if (unit.MinDamage.HasValue) SetBit(52);
        if (unit.MaxDamage.HasValue) SetBit(53);
        if (unit.MinOffHandDamage.HasValue) SetBit(54);
        if (unit.MaxOffHandDamage.HasValue) SetBit(55);
        if (unit.StandState.HasValue) SetBit(56);
        // 57 = PetTalentPoints (not in UnitData)
        if (unit.VisFlags.HasValue) SetBit(58);
        if (unit.AnimTier.HasValue) SetBit(59);
        if (unit.PetNumber.HasValue) SetBit(60);
        if (unit.PetNameTimestamp.HasValue) SetBit(61);
        if (unit.PetExperience.HasValue) SetBit(62);
        if (unit.PetNextLevelExperience.HasValue) SetBit(63);
        // Block 2: ModCast/Haste, combat stats, attack power
        if (unit.ModCastSpeed.HasValue) SetBit(65);
        if (unit.ModCastHaste.HasValue) SetBit(66);
        if (unit.ModHaste.HasValue) SetBit(67);
        if (unit.ModRangedHaste.HasValue) SetBit(68);
        if (unit.ModHasteRegen.HasValue) SetBit(69);
        if (unit.ModTimeRate.HasValue) SetBit(70);
        if (unit.CreatedBySpell.HasValue) SetBit(71);
        if (unit.EmoteState.HasValue) SetBit(72);
        if (unit.TrainingPointsUsed.HasValue) SetBit(73);
        if (unit.TrainingPointsTotal.HasValue) SetBit(74);
        if (unit.BaseMana.HasValue) SetBit(75);
        if (unit.BaseHealth.HasValue) SetBit(76);
        if (unit.SheatheState.HasValue) SetBit(77);
        if (unit.PvpFlags.HasValue) SetBit(78);
        if (unit.PetFlags.HasValue) SetBit(79);
        if (unit.ShapeshiftForm.HasValue) SetBit(80);
        if (unit.AttackPower.HasValue) SetBit(81);
        if (unit.AttackPowerModPos.HasValue) SetBit(82);
        if (unit.AttackPowerModNeg.HasValue) SetBit(83);
        if (unit.AttackPowerMultiplier.HasValue) SetBit(84);
        if (unit.RangedAttackPower.HasValue) SetBit(85);
        if (unit.RangedAttackPowerModPos.HasValue) SetBit(86);
        if (unit.RangedAttackPowerModNeg.HasValue) SetBit(87);
        if (unit.RangedAttackPowerMultiplier.HasValue) SetBit(88);
        if (unit.AttackSpeedAura.HasValue) SetBit(89);
        if (unit.Lifesteal.HasValue) SetBit(90);
        if (unit.MinRangedDamage.HasValue) SetBit(91);
        if (unit.MaxRangedDamage.HasValue) SetBit(92);
        if (unit.MaxHealthModifier.HasValue) SetBit(93);
        if (unit.HoverHeight.HasValue)
        {
            SetBit(94);
        }
        if (unit.MinItemLevelCutoff.HasValue) SetBit(95);
        // Block 3: MinItemLevel..GuildGUID
        if (unit.MinItemLevel.HasValue) SetBit(97);
        if (unit.MaxItemLevel.HasValue) SetBit(98);
        if (unit.WildBattlePetLevel.HasValue) SetBit(99);
        // 100 = BattlePetCompanionNameTimestamp (not tracked)
        if (unit.InteractSpellID.HasValue) SetBit(101);
        if (unit.ScaleDuration.HasValue) SetBit(102);
        if (unit.LooksLikeMountID.HasValue) SetBit(103);
        if (unit.LooksLikeCreatureID.HasValue) SetBit(104);
        if (unit.LookAtControllerID.HasValue) SetBit(105);
        // 106 = PerksVendorItemID (not tracked)
        if (unit.GuildGUID != null)
        {
            SetBit(107);
        }
        if (unit.NpcFlags != null)
        {
            bool hasAnyNpcFlag = false;
            for (int i = 0; i < unit.NpcFlags.Length; i++)
            {
                if (unit.NpcFlags[i].HasValue && unit.NpcFlags[i] != 0)
                {
                    SetBit(114 + i);
                    hasAnyNpcFlag = true;
                }
            }
            if (hasAnyNpcFlag)
                SetBit(113); // parent bit for NpcFlags array
        }
        bool hasAnyPowerGroup = false;
        if (unit.Power != null)
        {
            for (int j = 0; j < unit.Power.Length; j++)
            {
                if (unit.Power[j].HasValue)
                {
                    SetBit(137 + j);
                    hasAnyPowerGroup = true;
                }
            }
        }
        if (unit.MaxPower != null)
        {
            for (int k = 0; k < unit.MaxPower.Length; k++)
            {
                if (unit.MaxPower[k].HasValue)
                {
                    SetBit(147 + k);
                    hasAnyPowerGroup = true;
                }
            }
        }
        if (unit.ModPowerRegen != null)
        {
            for (int j2 = 0; j2 < unit.ModPowerRegen.Length; j2++)
            {
                if (unit.ModPowerRegen[j2].HasValue)
                {
                    SetBit(157 + j2);
                    hasAnyPowerGroup = true;
                }
            }
        }
        if (hasAnyPowerGroup)
            SetBit(116); // parent bit for Power/MaxPower/Regen arrays
        // VirtualItems array (parent bit 167, elements 168-170)
        if (unit.VirtualItems != null)
        {
            bool hasAnyVI = false;
            for (int i = 0; i < 3; i++)
            {
                if (unit.VirtualItems[i].HasValue && unit.VirtualItems[i].Value.ItemID != 0)
                {
                    SetBit(168 + i);
                    hasAnyVI = true;
                }
            }
            if (hasAnyVI) SetBit(167);
        }
        // RangedAttackRoundBaseTime (bit 170 — shares VirtualItems parent 167? No, separate)
        // Actually TC343 has RangedAttackRoundBaseTime at a different position. Check:
        // AttackRoundBaseTime array (parent bit 171, elements 172-173)
        if (unit.AttackRoundBaseTime != null)
        {
            bool hasAnyART = false;
            for (int i = 0; i < unit.AttackRoundBaseTime.Length && i < 2; i++)
            {
                if (unit.AttackRoundBaseTime[i].HasValue)
                {
                    SetBit(172 + i);
                    hasAnyART = true;
                }
            }
            if (hasAnyART) SetBit(171);
        }
        // Stats/StatPosBuff/StatNegBuff array (parent bit 174, elements 175-189)
        bool hasAnyStatsGroup = false;
        if (unit.Stats != null)
        {
            for (int i = 0; i < 5; i++)
            {
                if (unit.Stats[i].HasValue) { SetBit(175 + i); hasAnyStatsGroup = true; }
            }
        }
        if (unit.StatPosBuff != null)
        {
            for (int i = 0; i < 5; i++)
            {
                if (unit.StatPosBuff[i].HasValue) { SetBit(180 + i); hasAnyStatsGroup = true; }
            }
        }
        if (unit.StatNegBuff != null)
        {
            for (int i = 0; i < 5; i++)
            {
                if (unit.StatNegBuff[i].HasValue) { SetBit(185 + i); hasAnyStatsGroup = true; }
            }
        }
        if (hasAnyStatsGroup) SetBit(174);
        // Resistances/PowerCostModifier/PowerCostMultiplier array (parent bit 190, elements 191-211)
        bool hasAnyResistGroup = false;
        if (unit.Resistances != null)
        {
            for (int i = 0; i < 7; i++)
            {
                if (unit.Resistances[i].HasValue) { SetBit(191 + i); hasAnyResistGroup = true; }
            }
        }
        if (unit.PowerCostModifier != null)
        {
            for (int i = 0; i < 7; i++)
            {
                if (unit.PowerCostModifier[i].HasValue) { SetBit(198 + i); hasAnyResistGroup = true; }
            }
        }
        if (unit.PowerCostMultiplier != null)
        {
            for (int i = 0; i < 7; i++)
            {
                if (unit.PowerCostMultiplier[i].HasValue) { SetBit(205 + i); hasAnyResistGroup = true; }
            }
        }
        if (hasAnyResistGroup) SetBit(190);
        // ResistanceBuffMods array (parent bit 212, elements 213-226)
        bool hasAnyResBuffGroup = false;
        if (unit.ResistanceBuffModsPositive != null)
        {
            for (int i = 0; i < 7; i++)
            {
                if (unit.ResistanceBuffModsPositive[i].HasValue) { SetBit(213 + i); hasAnyResBuffGroup = true; }
            }
        }
        if (unit.ResistanceBuffModsNegative != null)
        {
            for (int i = 0; i < 7; i++)
            {
                if (unit.ResistanceBuffModsNegative[i].HasValue) { SetBit(220 + i); hasAnyResBuffGroup = true; }
            }
        }
        if (hasAnyResBuffGroup) SetBit(212);
        // V3_4_3 UnitData parent-bit cascade: bit 0 of blocks 0/1/2/3 IS the
        // required parent gate for the field group housed in that block (verified
        // against WPP UpdateFieldsHandler343.ReadUpdateUnitData — the top-level
        // `if (changesMask[0..96])` checks wrap reads of Health/MaxHealth/Charm/
        // Target/Flags/Stats/etc.). For blocks 4/5/6/7 bit 0 is a real field in
        // an array group (e.g. blockMasks[4] bit 0 == changesMask[128] ==
        // PowerRegenInterruptedFlatModifier[1], a Single) and must NOT be force-
        // set — doing so tells the V3_4_3 client to read floats we never wrote,
        // truncating the cascade and dropping the entire Values update (which is
        // why the rage bar never updated and XP deltas bundled with rage were
        // lost too). Verified against CypherCore native-V3_4_3 World.pkt diff.
        // V3_4_3-only (this file is V3_4_3_54261-specific).
        for (int bi = 0; bi < 4; bi++)
        {
            if (blockMasks[bi] != 0)
            {
                blockMasks[bi] |= 1u;
            }
        }
        byte blocksMask = 0;
        for (int l = 0; l < 8; l++)
        {
            if (blockMasks[l] != 0)
            {
                blocksMask |= (byte)(1 << l);
            }
        }
        data.WriteBits(blocksMask, 8);
        for (int m = 0; m < 8; m++)
        {
            if ((blocksMask & (1 << m)) != 0)
            {
                data.WriteBits(blockMasks[m], 32);
            }
        }
        if ((blockMasks[0] & 1) != 0)
        {
        }
        data.FlushBits();
        // Dynamic update masks for the changesMask[2/3/4] field group
        // (PassiveSpells/WorldEffects/ChannelObjects). Mirrors TC
        // UpdateFields.cpp:908-931 + UpdateField.cpp:43-63
        // (WriteCompleteDynamicFieldUpdateMask). Only ChannelObjects is wired
        // here — the other two are size-0 placeholders.
        if ((blockMasks[0] & (1u << 4)) != 0)
        {
            uint channelObjectsSize = (unit.ChannelObject.HasValue && !unit.ChannelObject.Value.IsEmpty()) ? 1u : 0u;
            data.WriteBits(channelObjectsSize, 32);
            if (channelObjectsSize != 0)
                data.WriteBits(0xFFFFFFFFu, (int)channelObjectsSize); // one set bit per element
        }
        data.FlushBits();
        if ((blockMasks[0] & 1) != 0)
        {
            // ChannelObjects body — written BEFORE Health to match TC
            // UpdateFields.cpp:957-963 ordering inside the changesMask[0] body.
            if ((blockMasks[0] & (1u << 4)) != 0
                && unit.ChannelObject.HasValue && !unit.ChannelObject.Value.IsEmpty())
            {
                data.WritePackedGuid128(unit.ChannelObject.Value);
            }
            if (unit.Health.HasValue)
            {
                data.WriteInt64(unit.Health.Value);
            }
            if (unit.MaxHealth.HasValue)
            {
                data.WriteInt64(unit.MaxHealth.Value);
            }
            if (unit.DisplayID.HasValue)
            {
                data.WriteInt32(unit.DisplayID.Value);
            }
            if (unit.Charm != null)
            {
                data.WritePackedGuid128(unit.Charm.Value);
            }
            if (unit.Summon != null)
            {
                data.WritePackedGuid128(unit.Summon.Value);
            }
            if (unit.CharmedBy != null)
            {
                data.WritePackedGuid128(unit.CharmedBy.Value);
            }
            if (unit.SummonedBy != null)
            {
                data.WritePackedGuid128(unit.SummonedBy.Value);
            }
            if (unit.CreatedBy != null)
            {
                data.WritePackedGuid128(unit.CreatedBy.Value);
            }
            if (unit.Target != null)
            {
                data.WritePackedGuid128(unit.Target.Value);
            }
            if (unit.ChannelData != null)
            {
                // CypherCore UnitChannel.WriteUpdate (UpdateFields.cs:744-748) writes
                // SpellID + SpellXSpellVisualID DIRECTLY — no inner bit-prefix, no
                // FlushBits, no ChannelObject sub-field. The previous code wrote
                // `WriteBits(3 or 7, 4) + FlushBits` which inserted 1 byte of garbage
                // before SpellID, shifting the V3_4_3 client's read by 1 byte. WPP
                // parsed our wire as `(ChannelData) SpellID: 13252976
                // SpellXSpellVisualID: 88256768` (random) instead of `SpellID: 51769
                // SpellXSpellVisualID: 0`. Result: cast bar didn't render, kneel
                // animation didn't play, ESC stayed blocked because the client
                // believed the player was channeling an unknown spell. Note:
                // ChannelObject is a SEPARATE Unit field (DynamicUpdateField in
                // CypherCore), not part of the ChannelData write — handle it
                // independently elsewhere if needed.
                data.WriteInt32(unit.ChannelData.Value.SpellID);
                data.WriteInt32(unit.ChannelData.Value.SpellXSpellVisualID);
            }
            if (unit.RaceId.HasValue)
            {
                data.WriteUInt8(unit.RaceId.Value);
            }
            if (unit.ClassId.HasValue)
            {
                data.WriteUInt8(unit.ClassId.Value);
            }
            if (unit.SexId.HasValue)
            {
                data.WriteUInt8(unit.SexId.Value);
            }
            if (unit.DisplayPower.HasValue) data.WriteUInt32(unit.DisplayPower.Value);
            if (unit.Level.HasValue)
            {
                data.WriteInt32(unit.Level.Value);
            }
            if (unit.EffectiveLevel.HasValue)
            {
                data.WriteInt32(unit.EffectiveLevel.Value);
            }
        }
        if ((blocksMask & 2) != 0)
        {
            if (unit.FactionTemplate.HasValue)
            {
                data.WriteInt32(unit.FactionTemplate.Value);
            }
            if (unit.Flags.HasValue)
            {
                data.WriteUInt32(unit.Flags.Value);
            }
            if (unit.Flags2.HasValue)
            {
                data.WriteUInt32(unit.Flags2.Value);
            }
            if (unit.Flags3.HasValue) data.WriteUInt32(unit.Flags3.Value);
            if (unit.AuraState.HasValue)
            {
                data.WriteUInt32(unit.AuraState.Value);
            }
            if (unit.OverrideDisplayPowerID.HasValue) data.WriteUInt32(unit.OverrideDisplayPowerID.Value);
            if (unit.BoundingRadius.HasValue)
            {
                data.WriteFloat(unit.BoundingRadius.Value);
            }
            if (unit.CombatReach.HasValue)
            {
                data.WriteFloat(unit.CombatReach.Value);
            }
            if (unit.DisplayScale.HasValue) data.WriteFloat(unit.DisplayScale.Value);
            if (unit.NativeDisplayID.HasValue)
            {
                data.WriteInt32(unit.NativeDisplayID.Value);
            }
            if (unit.NativeXDisplayScale.HasValue) data.WriteFloat(unit.NativeXDisplayScale.Value);
            if (unit.MountDisplayID.HasValue)
            {
                data.WriteInt32(unit.MountDisplayID.Value);
            }
            // Block 1 continued: damage, stance, pet
            if (unit.MinDamage.HasValue) data.WriteFloat(unit.MinDamage.Value);
            if (unit.MaxDamage.HasValue) data.WriteFloat(unit.MaxDamage.Value);
            if (unit.MinOffHandDamage.HasValue) data.WriteFloat(unit.MinOffHandDamage.Value);
            if (unit.MaxOffHandDamage.HasValue) data.WriteFloat(unit.MaxOffHandDamage.Value);
            if (unit.StandState.HasValue) data.WriteUInt8(unit.StandState.Value);
            if (unit.VisFlags.HasValue) data.WriteUInt8(unit.VisFlags.Value);
            if (unit.AnimTier.HasValue) data.WriteUInt8(unit.AnimTier.Value);
            if (unit.PetNumber.HasValue) data.WriteUInt32(unit.PetNumber.Value);
            if (unit.PetNameTimestamp.HasValue) data.WriteUInt32(unit.PetNameTimestamp.Value);
            if (unit.PetExperience.HasValue) data.WriteUInt32(unit.PetExperience.Value);
            if (unit.PetNextLevelExperience.HasValue) data.WriteUInt32(unit.PetNextLevelExperience.Value);
        }
        // Block 2 (bits 64-95): ModCast/Haste, combat stats, attack power
        if ((blocksMask & 4) != 0)
        {
            if (unit.ModCastSpeed.HasValue) data.WriteFloat(unit.ModCastSpeed.Value);
            if (unit.ModCastHaste.HasValue) data.WriteFloat(unit.ModCastHaste.Value);
            if (unit.ModHaste.HasValue) data.WriteFloat(unit.ModHaste.Value);
            if (unit.ModRangedHaste.HasValue) data.WriteFloat(unit.ModRangedHaste.Value);
            if (unit.ModHasteRegen.HasValue) data.WriteFloat(unit.ModHasteRegen.Value);
            if (unit.ModTimeRate.HasValue) data.WriteFloat(unit.ModTimeRate.Value);
            if (unit.CreatedBySpell.HasValue) data.WriteInt32(unit.CreatedBySpell.Value);
            if (unit.EmoteState.HasValue) data.WriteInt32(unit.EmoteState.Value);
            if (unit.TrainingPointsUsed.HasValue) data.WriteUInt16(unit.TrainingPointsUsed.Value);
            if (unit.TrainingPointsTotal.HasValue) data.WriteUInt16(unit.TrainingPointsTotal.Value);
            if (unit.BaseMana.HasValue) data.WriteInt32(unit.BaseMana.Value);
            if (unit.BaseHealth.HasValue) data.WriteInt32(unit.BaseHealth.Value);
            if (unit.SheatheState.HasValue) data.WriteUInt8(unit.SheatheState.Value);
            if (unit.PvpFlags.HasValue) data.WriteUInt8(unit.PvpFlags.Value);
            if (unit.PetFlags.HasValue) data.WriteUInt8(unit.PetFlags.Value);
            if (unit.ShapeshiftForm.HasValue) data.WriteUInt8(unit.ShapeshiftForm.Value);
            if (unit.AttackPower.HasValue) data.WriteInt32(unit.AttackPower.Value);
            if (unit.AttackPowerModPos.HasValue) data.WriteInt32(unit.AttackPowerModPos.Value);
            if (unit.AttackPowerModNeg.HasValue) data.WriteInt32(unit.AttackPowerModNeg.Value);
            if (unit.AttackPowerMultiplier.HasValue) data.WriteFloat(unit.AttackPowerMultiplier.Value);
            if (unit.RangedAttackPower.HasValue) data.WriteInt32(unit.RangedAttackPower.Value);
            if (unit.RangedAttackPowerModPos.HasValue) data.WriteInt32(unit.RangedAttackPowerModPos.Value);
            if (unit.RangedAttackPowerModNeg.HasValue) data.WriteInt32(unit.RangedAttackPowerModNeg.Value);
            if (unit.RangedAttackPowerMultiplier.HasValue) data.WriteFloat(unit.RangedAttackPowerMultiplier.Value);
            if (unit.AttackSpeedAura.HasValue) data.WriteInt32(unit.AttackSpeedAura.Value);
            if (unit.Lifesteal.HasValue) data.WriteFloat(unit.Lifesteal.Value);
            if (unit.MinRangedDamage.HasValue) data.WriteFloat(unit.MinRangedDamage.Value);
            if (unit.MaxRangedDamage.HasValue) data.WriteFloat(unit.MaxRangedDamage.Value);
            if (unit.MaxHealthModifier.HasValue) data.WriteFloat(unit.MaxHealthModifier.Value);
            if (unit.HoverHeight.HasValue) data.WriteFloat(unit.HoverHeight.Value);
            if (unit.MinItemLevelCutoff.HasValue) data.WriteInt32(unit.MinItemLevelCutoff.Value);
        }
        // Block 3 (bits 96-127): MinItemLevel..ComboTarget, GuildGUID, NpcFlags
        if ((blocksMask & 8) != 0)
        {
            if (unit.MinItemLevel.HasValue) data.WriteInt32(unit.MinItemLevel.Value);
            if (unit.MaxItemLevel.HasValue) data.WriteInt32(unit.MaxItemLevel.Value);
            if (unit.WildBattlePetLevel.HasValue) data.WriteInt32(unit.WildBattlePetLevel.Value);
            // 100 = BattlePetCompanionNameTimestamp not tracked
            if (unit.InteractSpellID.HasValue) data.WriteInt32(unit.InteractSpellID.Value);
            if (unit.ScaleDuration.HasValue) data.WriteInt32(unit.ScaleDuration.Value);
            if (unit.LooksLikeMountID.HasValue) data.WriteInt32(unit.LooksLikeMountID.Value);
            if (unit.LooksLikeCreatureID.HasValue) data.WriteInt32(unit.LooksLikeCreatureID.Value);
            if (unit.LookAtControllerID.HasValue) data.WriteInt32(unit.LookAtControllerID.Value);
            // 106 = PerksVendorItemID not tracked
            if (unit.GuildGUID != null)
            {
                data.WritePackedGuid128(unit.GuildGUID.Value);
            }
            // NpcFlags array (parent bit 113, elements 114-115) — gated by block 3
            if (unit.NpcFlags != null)
            {
                for (int n = 0; n < unit.NpcFlags.Length; n++)
                {
                    if (unit.NpcFlags[n].HasValue && unit.NpcFlags[n] != 0)
                    {
                        data.WriteUInt32(unit.NpcFlags[n].Value);
                    }
                }
            }
        }
        // Power group (parent bit 116) — TC343 interleaves all power arrays per-index
        if ((blocksMask & 0x10) != 0)
        {
            int maxLen = 0;
            if (unit.Power != null && unit.Power.Length > maxLen) maxLen = unit.Power.Length;
            if (unit.MaxPower != null && unit.MaxPower.Length > maxLen) maxLen = unit.MaxPower.Length;
            for (int pi = 0; pi < maxLen; pi++)
            {
                // PowerRegenFlatModifier[pi] (bits 117+) — not tracked, skip
                // PowerRegenInterruptedFlatModifier[pi] (bits 127+) — not tracked, skip
                if (unit.Power != null && pi < unit.Power.Length && unit.Power[pi].HasValue)
                    data.WriteInt32(unit.Power[pi].Value);
                if (unit.MaxPower != null && pi < unit.MaxPower.Length && unit.MaxPower[pi].HasValue)
                    data.WriteInt32(unit.MaxPower[pi].Value);
                if (unit.ModPowerRegen != null && pi < unit.ModPowerRegen.Length && unit.ModPowerRegen[pi].HasValue)
                    data.WriteFloat(unit.ModPowerRegen[pi].Value);
            }
        }
        // VirtualItems (parent bit 167, elements 168-170)
        if ((blockMasks[167 / 32] & (1u << (167 % 32))) != 0)
        {
            for (int vi = 0; vi < 3; vi++)
            {
                if ((blockMasks[(168 + vi) / 32] & (1u << ((168 + vi) % 32))) != 0)
                {
                    var vItem = unit.VirtualItems[vi];
                    // VirtualItem::WriteUpdate: 4-bit mask + fields
                    // Bit 0=hasAny, 1=ItemID, 2=ItemAppearanceModID, 3=ItemVisual
                    data.WriteBits(0x03u, 4); // bits 0+1 set (hasAny + ItemID)
                    data.FlushBits();
                    data.WriteInt32(vItem.HasValue ? vItem.Value.ItemID : 0);
                }
            }
        }
        // AttackRoundBaseTime array (parent bit 171, elements 172-173) — block 5
        if ((blockMasks[171 / 32] & (1u << (171 % 32))) != 0)
        {
            if (unit.AttackRoundBaseTime != null)
            {
                for (int i = 0; i < unit.AttackRoundBaseTime.Length && i < 2; i++)
                {
                    if (unit.AttackRoundBaseTime[i].HasValue)
                        data.WriteUInt32(unit.AttackRoundBaseTime[i].Value);
                }
            }
        }
        // Stats/StatPosBuff/StatNegBuff (parent bit 174, interleaved per TC343) — block 5
        if ((blockMasks[174 / 32] & (1u << (174 % 32))) != 0)
        {
            for (int i = 0; i < 5; i++)
            {
                if (unit.Stats != null && unit.Stats[i].HasValue) data.WriteInt32(unit.Stats[i].Value);
                if (unit.StatPosBuff != null && unit.StatPosBuff[i].HasValue) data.WriteInt32(unit.StatPosBuff[i].Value);
                if (unit.StatNegBuff != null && unit.StatNegBuff[i].HasValue) data.WriteInt32(unit.StatNegBuff[i].Value);
            }
        }
        // Resistances/PowerCostModifier/PowerCostMultiplier (parent bit 190, spans blocks 5-6)
        if ((blockMasks[190 / 32] & (1u << (190 % 32))) != 0)
        {
            for (int i = 0; i < 7; i++)
            {
                if (unit.Resistances != null && unit.Resistances[i].HasValue) data.WriteInt32(unit.Resistances[i].Value);
                if (unit.PowerCostModifier != null && unit.PowerCostModifier[i].HasValue) data.WriteInt32(unit.PowerCostModifier[i].Value);
                if (unit.PowerCostMultiplier != null && unit.PowerCostMultiplier[i].HasValue) data.WriteFloat(unit.PowerCostMultiplier[i].Value);
            }
        }
        // ResistanceBuffMods (parent bit 212, elements 213-226, spans blocks 6-7)
        if ((blockMasks[212 / 32] & (1u << (212 % 32))) != 0)
        {
            for (int i = 0; i < 7; i++)
            {
                if (unit.ResistanceBuffModsPositive != null && unit.ResistanceBuffModsPositive[i].HasValue) data.WriteInt32(unit.ResistanceBuffModsPositive[i].Value);
                if (unit.ResistanceBuffModsNegative != null && unit.ResistanceBuffModsNegative[i].HasValue) data.WriteInt32(unit.ResistanceBuffModsNegative[i].Value);
            }
        }
        void SetBit(int idx)
        {
            blockMasks[idx / 32] |= (uint)(1 << idx % 32);
        }
    }

    // === WriteUpdatePlayerData (fork lines 2674-2813) ===
    private void WriteUpdatePlayerData(WorldPacket data)
    {
        PlayerData p = _updateData.PlayerData ?? new PlayerData();

        uint[] blocks = new uint[4];
        void SetBit(int bit) { blocks[bit / 32] |= (1u << (bit % 32)); }
        bool IsBitSet(int bit) { return (blocks[bit / 32] & (1u << (bit % 32))) != 0; }

        // Block 0: scalar fields (bits 4-31)
        if (p.DuelArbiter != null) { SetBit(0); SetBit(4); }
        if (p.WowAccount != null) { SetBit(0); SetBit(5); }
        if (p.LootTargetGUID != null) { SetBit(0); SetBit(6); }
        if (p.PlayerFlags.HasValue) { SetBit(0); SetBit(7); }
        if (p.PlayerFlagsEx.HasValue) { SetBit(0); SetBit(8); }
        if (p.GuildRankID.HasValue) { SetBit(0); SetBit(9); }
        if (p.GuildDeleteDate.HasValue) { SetBit(0); SetBit(10); }
        if (p.GuildLevel.HasValue) { SetBit(0); SetBit(11); }
        if (p.NumBankSlots.HasValue) { SetBit(0); SetBit(12); }
        if (p.NativeSex.HasValue) { SetBit(0); SetBit(13); }
        if (p.Inebriation.HasValue) { SetBit(0); SetBit(14); }
        if (p.PvpTitle.HasValue) { SetBit(0); SetBit(15); }
        if (p.ArenaFaction.HasValue) { SetBit(0); SetBit(16); }
        if (p.PvPRank.HasValue) { SetBit(0); SetBit(17); }
        // 18: Field_88 — unused
        if (p.DuelTeam.HasValue) { SetBit(0); SetBit(19); }
        if (p.GuildTimeStamp.HasValue) { SetBit(0); SetBit(20); }
        if (p.ChosenTitle.HasValue) { SetBit(0); SetBit(21); }
        if (p.FakeInebriation.HasValue) { SetBit(0); SetBit(22); }
        if (p.VirtualPlayerRealm.HasValue) { SetBit(0); SetBit(23); }
        if (p.CurrentSpecID.HasValue) { SetBit(0); SetBit(24); }
        // 25: TaxiMountAnimKitID — not tracked
        // 26: CurrentBattlePetBreedQuality — not tracked
        if (p.HonorLevel.HasValue) { SetBit(0); SetBit(27); }
        // 28-31: LogoutTime, CurrentBattlePetSpeciesID, BnetAccount, DungeonScore — not tracked

        // QuestLog (header 35, elements 36-60)
        bool hasAnyQuestLog = false;
        for (int i = 0; i < QuestConst.MaxQuestLogSize; i++)
        {
            if (p.QuestLog[i] != null && p.QuestLog[i].QuestID.HasValue)
            {
                SetBit(35);
                SetBit(36 + i);
                hasAnyQuestLog = true;
            }
        }

        // VisibleItems (header 61, elements 62-80)
        bool hasAnyVisibleItem = false;
        for (int i = 0; i < 19; i++)
        {
            if (p.VisibleItems != null && i < p.VisibleItems.Length && p.VisibleItems[i] != null)
            {
                SetBit(61);
                SetBit(62 + i);
                hasAnyVisibleItem = true;
            }
        }

        Framework.Logging.Log.Print(Framework.Logging.LogType.Trace, $"[PlayerDataUpdate] blocks=[0x{blocks[0]:X8},0x{blocks[1]:X8},0x{blocks[2]:X8},0x{blocks[3]:X8}]");

        // Write blocksMask (4 bits)
        byte blocksMask = 0;
        for (int i = 0; i < 4; i++)
            if (blocks[i] != 0) blocksMask |= (byte)(1 << i);

        data.WriteBits(blocksMask, 4);
        for (int i = 0; i < 4; i++)
            if ((blocksMask & (1 << i)) != 0)
                data.WriteBits(blocks[i], 32);

        // IsQuestLogChangesMaskSkipped = true → use WriteCreate format for quest entries
        data.WriteBit(true);

        // No dynamic fields (bits 1-3 not set)
        data.FlushBits();

        // Block 0: scalar field values in TC343 bit order (4-31)
        if (IsBitSet(0))
        {
            if (IsBitSet(4)) data.WritePackedGuid128(p.DuelArbiter.Value);
            if (IsBitSet(5)) data.WritePackedGuid128(p.WowAccount.Value);
            if (IsBitSet(6)) data.WritePackedGuid128(p.LootTargetGUID.Value);
            if (IsBitSet(7)) data.WriteUInt32(p.PlayerFlags.Value);
            if (IsBitSet(8)) data.WriteUInt32(p.PlayerFlagsEx.Value);
            if (IsBitSet(9)) data.WriteUInt32(p.GuildRankID.Value);
            if (IsBitSet(10)) data.WriteUInt32(p.GuildDeleteDate.Value);
            if (IsBitSet(11)) data.WriteInt32(p.GuildLevel.Value);
            if (IsBitSet(12)) data.WriteUInt8(p.NumBankSlots.Value);
            if (IsBitSet(13)) data.WriteUInt8(p.NativeSex.Value);
            if (IsBitSet(14)) data.WriteUInt8(p.Inebriation.Value);
            if (IsBitSet(15)) data.WriteUInt8(p.PvpTitle.Value);
            if (IsBitSet(16)) data.WriteUInt8(p.ArenaFaction.Value);
            if (IsBitSet(17)) data.WriteUInt8(p.PvPRank.Value);
            // 18: Field_88 skipped
            if (IsBitSet(19)) data.WriteUInt32(p.DuelTeam.Value);
            if (IsBitSet(20)) data.WriteInt32(p.GuildTimeStamp.Value);
            if (IsBitSet(21)) data.WriteInt32(p.ChosenTitle.Value);
            if (IsBitSet(22)) data.WriteInt32(p.FakeInebriation.Value);
            if (IsBitSet(23)) data.WriteUInt32(p.VirtualPlayerRealm.Value);
            if (IsBitSet(24)) data.WriteUInt32(p.CurrentSpecID.Value);
            // 25-26 skipped
            if (IsBitSet(27)) data.WriteInt32(p.HonorLevel.Value);
        }

        // QuestLog entries (bits 35-60) — WriteCreate format
        if (hasAnyQuestLog)
        {
            for (int i = 0; i < QuestConst.MaxQuestLogSize; i++)
            {
                if (IsBitSet(36 + i))
                {
                    QuestLog quest = p.QuestLog[i];
                    data.WriteInt64(quest?.EndTime ?? 0);
                    data.WriteInt32(quest?.QuestID ?? 0);
                    data.WriteUInt32(quest?.StateFlags ?? 0);
                    for (int obj = 0; obj < 24; obj++)
                        data.WriteUInt16((ushort)(quest?.ObjectiveProgress[obj] ?? 0));
                }
            }
        }

        // VisibleItems (bits 61-80) — TC343 VisibleItem::WriteUpdate uses WriteBits(mask, 4)
        if (hasAnyVisibleItem)
        {
            for (int i = 0; i < 19; i++)
            {
                if (IsBitSet(62 + i))
                {
                    VisibleItem item = p.VisibleItems[i].Value;
                    // VisibleItem has HasChangesMask<4>: bit 0=hasAny, 1=ItemID, 2=AppearanceModID, 3=ItemVisual
                    data.WriteBits(0x0F, 4); // all 4 bits set
                    data.FlushBits();
                    data.WriteInt32(item.ItemID);
                    data.WriteUInt16(item.ItemAppearanceModID);
                    data.WriteUInt16(item.ItemVisual);
                }
            }
        }
    }

    // === WriteUpdateActivePlayerData (fork lines 501-1138) ===
    private void WriteUpdateActivePlayerData(WorldPacket data)
    {
        ActivePlayerData a = _updateData.ActivePlayerData ?? new ActivePlayerData();

        // Build changesMask (1536 bits = 48 blocks of 32)
        uint[] blocks = new uint[48];
        void SetBit(int bit) { blocks[bit / 32] |= (1u << (bit % 32)); }
        bool IsBitSet(int bit) { return (blocks[bit / 32] & (1u << (bit % 32))) != 0; }

        // Pre-compute KnownTitles (uint?[12] → ulong[6])
        int knownTitlesCount = 0;
        ulong[] knownTitles64 = new ulong[6];
        if (a.KnownTitles != null)
        {
            bool hasAnyTitle = false;
            for (int i = 0; i < a.KnownTitles.Length; i++)
                if (a.KnownTitles[i].HasValue) { hasAnyTitle = true; break; }
            if (hasAnyTitle)
            {
                knownTitlesCount = 6;
                for (int i = 0; i < 6; i++)
                {
                    uint lo = (i * 2 < a.KnownTitles.Length && a.KnownTitles[i * 2].HasValue) ? a.KnownTitles[i * 2].Value : 0;
                    uint hi = (i * 2 + 1 < a.KnownTitles.Length && a.KnownTitles[i * 2 + 1].HasValue) ? a.KnownTitles[i * 2 + 1].Value : 0;
                    knownTitles64[i] = (ulong)lo | ((ulong)hi << 32);
                }
                SetBit(0); SetBit(3); // dynamic field KnownTitles
            }
        }

        // Pre-compute GlyphSlots/Glyphs from GameSessionData
        // NOTE: Glyphs only change via specific packets, not update fields.
        // Sending them in every Values update is wasteful and may cause format issues.
        // Glyphs are already set correctly in the CREATE path.
        bool hasGlyphChanges = false;

        // ============================================================
        // SET BITS — Block 0 scalar fields (group bit 0, fields 26-37)
        // ============================================================
        if (a.FarsightObject != null) { SetBit(0); SetBit(26); }
        // 27: SummonedBattlePetGUID — not used in WotLK
        if (a.Coinage.HasValue) { SetBit(0); SetBit(28); }
        if (a.XP.HasValue) { SetBit(0); SetBit(29); }
        if (a.NextLevelXP.HasValue) { SetBit(0); SetBit(30); }
        if (a.TrialXP.HasValue) { SetBit(0); SetBit(31); }
        if (a.Skill != null && HasAnySkillChanged(a.Skill)) { SetBit(0); SetBit(32); }
        if (a.CharacterPoints.HasValue) { SetBit(0); SetBit(33); }
        if (a.MaxTalentTiers.HasValue) { SetBit(0); SetBit(34); }
        if (a.TrackCreatureMask.HasValue) { SetBit(0); SetBit(35); }
        if (a.MainhandExpertise.HasValue) { SetBit(0); SetBit(36); }
        if (a.OffhandExpertise.HasValue) { SetBit(0); SetBit(37); }

        // ============================================================
        // SET BITS — Block 38 scalar fields (group bit 38, fields 39-69)
        // ============================================================
        if (a.RangedExpertise.HasValue) { SetBit(38); SetBit(39); }
        if (a.CombatRatingExpertise.HasValue) { SetBit(38); SetBit(40); }
        if (a.BlockPercentage.HasValue) { SetBit(38); SetBit(41); }
        if (a.DodgePercentage.HasValue) { SetBit(38); SetBit(42); }
        if (a.DodgePercentageFromAttribute.HasValue) { SetBit(38); SetBit(43); }
        if (a.ParryPercentage.HasValue) { SetBit(38); SetBit(44); }
        if (a.ParryPercentageFromAttribute.HasValue) { SetBit(38); SetBit(45); }
        if (a.CritPercentage.HasValue) { SetBit(38); SetBit(46); }
        if (a.RangedCritPercentage.HasValue) { SetBit(38); SetBit(47); }
        if (a.OffhandCritPercentage.HasValue) { SetBit(38); SetBit(48); }
        if (a.ShieldBlock.HasValue) { SetBit(38); SetBit(49); }
        // 50: ShieldBlockCritPercentage — no property
        if (a.Mastery.HasValue) { SetBit(38); SetBit(51); }
        if (a.Speed.HasValue) { SetBit(38); SetBit(52); }
        if (a.Avoidance.HasValue) { SetBit(38); SetBit(53); }
        if (a.Sturdiness.HasValue) { SetBit(38); SetBit(54); }
        if (a.Versatility.HasValue) { SetBit(38); SetBit(55); }
        if (a.VersatilityBonus.HasValue) { SetBit(38); SetBit(56); }
        if (a.PvpPowerDamage.HasValue) { SetBit(38); SetBit(57); }
        if (a.PvpPowerHealing.HasValue) { SetBit(38); SetBit(58); }
        if (a.ModHealingDonePos.HasValue) { SetBit(38); SetBit(59); }
        if (a.ModHealingPercent.HasValue) { SetBit(38); SetBit(60); }
        if (a.ModHealingDonePercent.HasValue) { SetBit(38); SetBit(61); }
        if (a.ModPeriodicHealingDonePercent.HasValue) { SetBit(38); SetBit(62); }
        if (a.ModSpellPowerPercent.HasValue) { SetBit(38); SetBit(63); }
        if (a.ModResiliencePercent.HasValue) { SetBit(38); SetBit(64); }
        if (a.OverrideSpellPowerByAPPercent.HasValue) { SetBit(38); SetBit(65); }
        if (a.OverrideAPBySpellPowerPercent.HasValue) { SetBit(38); SetBit(66); }
        if (a.ModTargetResistance.HasValue) { SetBit(38); SetBit(67); }
        if (a.ModTargetPhysicalResistance.HasValue) { SetBit(38); SetBit(68); }
        if (a.LocalFlags.HasValue) { SetBit(38); SetBit(69); }

        // ============================================================
        // SET BITS — Block 70 scalar fields (group bit 70, fields 71-101)
        // ============================================================
        if (a.GrantableLevels.HasValue) { SetBit(70); SetBit(71); }
        if (a.MultiActionBars.HasValue) { SetBit(70); SetBit(72); }
        if (a.LifetimeMaxRank.HasValue) { SetBit(70); SetBit(73); }
        if (a.NumRespecs.HasValue) { SetBit(70); SetBit(74); }
        if (a.AmmoID.HasValue) { SetBit(70); SetBit(75); }
        if (a.PvpMedals.HasValue) { SetBit(70); SetBit(76); }
        if (a.TodayHonorableKills.HasValue) { SetBit(70); SetBit(77); }
        if (a.TodayDishonorableKills.HasValue) { SetBit(70); SetBit(78); }
        if (a.YesterdayHonorableKills.HasValue) { SetBit(70); SetBit(79); }
        if (a.YesterdayDishonorableKills.HasValue) { SetBit(70); SetBit(80); }
        if (a.LastWeekHonorableKills.HasValue) { SetBit(70); SetBit(81); }
        if (a.LastWeekDishonorableKills.HasValue) { SetBit(70); SetBit(82); }
        if (a.ThisWeekHonorableKills.HasValue) { SetBit(70); SetBit(83); }
        if (a.ThisWeekDishonorableKills.HasValue) { SetBit(70); SetBit(84); }
        if (a.ThisWeekContribution.HasValue) { SetBit(70); SetBit(85); }
        if (a.LifetimeHonorableKills.HasValue) { SetBit(70); SetBit(86); }
        if (a.LifetimeDishonorableKills.HasValue) { SetBit(70); SetBit(87); }
        // 88: Field_F24 — unused
        if (a.YesterdayContribution.HasValue) { SetBit(70); SetBit(89); }
        if (a.LastWeekContribution.HasValue) { SetBit(70); SetBit(90); }
        if (a.LastWeekRank.HasValue) { SetBit(70); SetBit(91); }
        if (a.WatchedFactionIndex.HasValue) { SetBit(70); SetBit(92); }
        if (a.MaxLevel.HasValue) { SetBit(70); SetBit(93); }
        if (a.ScalingPlayerLevelDelta.HasValue) { SetBit(70); SetBit(94); }
        if (a.MaxCreatureScalingLevel.HasValue) { SetBit(70); SetBit(95); }
        if (a.PetSpellPower.HasValue) { SetBit(70); SetBit(96); }
        if (a.UiHitModifier.HasValue) { SetBit(70); SetBit(97); }
        if (a.UiSpellHitModifier.HasValue) { SetBit(70); SetBit(98); }
        if (a.HomeRealmTimeOffset.HasValue) { SetBit(70); SetBit(99); }
        if (a.ModPetHaste.HasValue) { SetBit(70); SetBit(100); }
        if (a.LocalRegenFlags.HasValue) { SetBit(70); SetBit(101); }

        // ============================================================
        // SET BITS — Block 102 scalar fields (group bit 102, fields 103-123)
        // ============================================================
        if (a.AuraVision.HasValue) { SetBit(102); SetBit(103); }
        if (a.NumBackpackSlots.HasValue) { SetBit(102); SetBit(104); }
        if (a.OverrideSpellsID.HasValue) { SetBit(102); SetBit(105); }
        if (a.LfgBonusFactionID.HasValue) { SetBit(102); SetBit(106); }
        if (a.LootSpecID.HasValue) { SetBit(102); SetBit(107); }
        if (a.OverrideZonePVPType.HasValue) { SetBit(102); SetBit(108); }
        if (a.Honor.HasValue) { SetBit(102); SetBit(109); }
        if (a.HonorNextLevel.HasValue) { SetBit(102); SetBit(110); }
        // 111: Field_F74 — unused
        if (a.PvPTierMaxFromWins.HasValue) { SetBit(102); SetBit(112); }
        if (a.PvPLastWeeksTierMaxFromWins.HasValue) { SetBit(102); SetBit(113); }
        if (a.PvPRankProgress.HasValue) { SetBit(102); SetBit(114); }
        // 115-123: GlyphsEnabled (120) set in create path only
        // Sending GlyphsEnabled in every Values update adds block 102 + FlushBits overhead

        // ============================================================
        // SET BITS — Array fields
        // ============================================================

        // InvSlots (header 124, elements 125-265)
        int invSlotsChanged = 0;
        for (int i = 0; i < 141; i++)
        {
            if (GetModernInvSlot(a, i) != null)
            {
                SetBit(124);
                SetBit(125 + i);
                invSlotsChanged++;
            }
        }

        // TrackResourceMask (header 266, elements 267-268)
        if (a.TrackResourceMask != null)
            for (int i = 0; i < 2; i++)
                if (a.TrackResourceMask[i].HasValue) { SetBit(266); SetBit(267 + i); }

        // Shared header 269: SpellCritPercentage (270-276), ModDamageDonePos (277-283),
        // ModDamageDoneNeg (284-290), ModDamageDonePercent (291-297)
        if (a.SpellCritPercentage != null)
            for (int i = 0; i < 7; i++)
                if (a.SpellCritPercentage[i].HasValue) { SetBit(269); SetBit(270 + i); }
        if (a.ModDamageDonePos != null)
            for (int i = 0; i < 7; i++)
                if (a.ModDamageDonePos[i].HasValue) { SetBit(269); SetBit(277 + i); }
        if (a.ModDamageDoneNeg != null)
            for (int i = 0; i < 7; i++)
                if (a.ModDamageDoneNeg[i].HasValue) { SetBit(269); SetBit(284 + i); }
        if (a.ModDamageDonePercent != null)
            for (int i = 0; i < 7; i++)
                if (a.ModDamageDonePercent[i].HasValue) { SetBit(269); SetBit(291 + i); }

        // ExploredZones (header 298, elements 299-538)
        if (a.ExploredZones != null)
            for (int i = 0; i < 240; i++)
                if (a.ExploredZones[i].HasValue) { SetBit(298); SetBit(299 + i); }

        // RestInfo (header 539, elements 540-541)
        if (a.RestInfo != null)
            for (int i = 0; i < 2; i++)
                if (a.RestInfo[i] != null && (a.RestInfo[i].Threshold.HasValue || a.RestInfo[i].StateID.HasValue))
                { SetBit(539); SetBit(540 + i); }

        // Shared header 542: WeaponDmgMultipliers (543-545), WeaponAtkSpeedMultipliers (546-548)
        if (a.WeaponDmgMultipliers != null)
            for (int i = 0; i < 3; i++)
                if (a.WeaponDmgMultipliers[i].HasValue) { SetBit(542); SetBit(543 + i); }
        if (a.WeaponAtkSpeedMultipliers != null)
            for (int i = 0; i < 3; i++)
                if (a.WeaponAtkSpeedMultipliers[i].HasValue) { SetBit(542); SetBit(546 + i); }

        // Shared header 549: BuybackPrice (550-561), BuybackTimestamp (562-573)
        if (a.BuybackPrice != null)
            for (int i = 0; i < 12; i++)
                if (a.BuybackPrice[i].HasValue) { SetBit(549); SetBit(550 + i); }
        if (a.BuybackTimestamp != null)
            for (int i = 0; i < 12; i++)
                if (a.BuybackTimestamp[i].HasValue) { SetBit(549); SetBit(562 + i); }

        // CombatRatings (header 574, elements 575-606)
        if (a.CombatRatings != null)
            for (int i = 0; i < 32; i++)
                if (a.CombatRatings[i].HasValue) { SetBit(574); SetBit(575 + i); }

        // NoReagentCostMask (header 615, elements 616-619)
        if (a.NoReagentCostMask != null)
            for (int i = 0; i < 4; i++)
                if (a.NoReagentCostMask[i].HasValue) { SetBit(615); SetBit(616 + i); }

        // ProfessionSkillLine (header 620, elements 621-622)
        if (a.ProfessionSkillLine != null)
            for (int i = 0; i < 2; i++)
                if (a.ProfessionSkillLine[i].HasValue) { SetBit(620); SetBit(621 + i); }

        // BagSlotFlags (header 623, elements 624-627)
        if (a.BagSlotFlags != null)
            for (int i = 0; i < 4; i++)
                if (a.BagSlotFlags[i].HasValue) { SetBit(623); SetBit(624 + i); }

        // BankBagSlotFlags (header 628, elements 629-635)
        if (a.BankBagSlotFlags != null)
            for (int i = 0; i < 7; i++)
                if (a.BankBagSlotFlags[i].HasValue) { SetBit(628); SetBit(629 + i); }

        // QuestCompleted (header 636, elements 637-1511)
        if (a.QuestCompleted != null)
            for (int i = 0; i < 875; i++)
                if (a.QuestCompleted[i].HasValue) { SetBit(636); SetBit(637 + i); }

        // PvpInfo (header 607, elements 608-614)
        if (a.PvpInfo != null)
            for (int i = 0; i < Math.Min(a.PvpInfo.Length, 7); i++)
                if (a.PvpInfo[i] != null && (a.PvpInfo[i].Rating != 0 || a.PvpInfo[i].SeasonPlayed != 0 || a.PvpInfo[i].Disqualified))
                { SetBit(607); SetBit(608 + i); }

        // GlyphSlots (header 1512, elements 1513-1518) + Glyphs (header 1512, elements 1519-1524)
        if (hasGlyphChanges)
        {
            SetBit(1512); // shared header
            uint[] glyphSlotIds = { 21, 22, 23, 24, 25, 26 };
            for (int i = 0; i < 6; i++)
            {
                SetBit(1513 + i); // GlyphSlots[i]
                SetBit(1519 + i); // Glyphs[i]
            }
        }

        // ============================================================
        // DEBUG LOG
        // ============================================================
        int setBlockCount = 0;
        System.Text.StringBuilder dbgBlocks = new System.Text.StringBuilder();
        for (int b = 0; b < 48; b++)
        {
            if (blocks[b] != 0)
            {
                setBlockCount++;
                dbgBlocks.Append($" blk{b}=0x{blocks[b]:X8}");
            }
        }
        Framework.Logging.Log.Print(Framework.Logging.LogType.Debug, $"[ActivePlayerUpdate] {setBlockCount} blocks set, InvSlots={invSlotsChanged}{dbgBlocks}");

        // ============================================================
        // WRITE BLOCK MASKS
        // ============================================================
        uint blocksMask0 = 0;
        for (int b = 0; b < 32; b++)
            if (blocks[b] != 0) blocksMask0 |= (1u << b);
        uint blocksMask1 = 0;
        for (int b = 32; b < 48; b++)
            if (blocks[b] != 0) blocksMask1 |= (1u << (b - 32));

        data.WriteUInt32(blocksMask0);
        data.WriteBits(blocksMask1, 16);

        for (int b = 0; b < 48; b++)
        {
            bool blockSet = (b < 32) ? ((blocksMask0 & (1u << b)) != 0) : ((blocksMask1 & (1u << (b - 32))) != 0);
            if (blockSet)
                data.WriteBits(blocks[b], 32);
        }

        // Dynamic field masks (TC343 order: bits 1, 2, 3, 20-25, 4-19)
        // Bit 1: SortBagsRightToLeft — not used
        // Bit 2: InsertItemsLeftToRight — not used
        if (IsBitSet(3)) // KnownTitles
        {
            data.WriteBits((uint)knownTitlesCount, 32); // array element count
            for (int i = 0; i < knownTitlesCount; i++)
                data.WriteBit(true); // all elements changed
        }
        // Bits 4-25: other dynamic fields — not used
        data.FlushBits(); // end of dynamic mask section

        // Dynamic field data (TC343 order: Research data, then KnownTitles, then others)
        if (IsBitSet(3)) // KnownTitles data
        {
            for (int i = 0; i < knownTitlesCount; i++)
                data.WriteUInt64(knownTitles64[i]);
        }

        // ============================================================
        // WRITE SCALAR DATA — Block 0 (bits 26-37)
        // ============================================================
        if (IsBitSet(0))
        {
            if (IsBitSet(26)) data.WritePackedGuid128(a.FarsightObject.Value);
            // 27: SummonedBattlePetGUID skipped
            if (IsBitSet(28)) data.WriteUInt64(a.Coinage.Value);
            if (IsBitSet(29)) data.WriteInt32(a.XP.Value);
            if (IsBitSet(30)) data.WriteInt32(a.NextLevelXP.Value);
            if (IsBitSet(31)) data.WriteInt32(a.TrialXP.Value);
            if (IsBitSet(32)) WriteUpdateSkillInfo(data, a.Skill);
            if (IsBitSet(33)) data.WriteInt32(a.CharacterPoints.Value);
            if (IsBitSet(34)) data.WriteInt32(a.MaxTalentTiers.Value);
            if (IsBitSet(35)) data.WriteUInt32(a.TrackCreatureMask.Value);
            if (IsBitSet(36)) data.WriteFloat(a.MainhandExpertise.Value);
            if (IsBitSet(37)) data.WriteFloat(a.OffhandExpertise.Value);
        }

        // ============================================================
        // WRITE SCALAR DATA — Block 38 (bits 39-69)
        // ============================================================
        if (IsBitSet(38))
        {
            if (IsBitSet(39)) data.WriteFloat(a.RangedExpertise.Value);
            if (IsBitSet(40)) data.WriteFloat(a.CombatRatingExpertise.Value);
            if (IsBitSet(41)) data.WriteFloat(a.BlockPercentage.Value);
            if (IsBitSet(42)) data.WriteFloat(a.DodgePercentage.Value);
            if (IsBitSet(43)) data.WriteFloat(a.DodgePercentageFromAttribute.Value);
            if (IsBitSet(44)) data.WriteFloat(a.ParryPercentage.Value);
            if (IsBitSet(45)) data.WriteFloat(a.ParryPercentageFromAttribute.Value);
            if (IsBitSet(46)) data.WriteFloat(a.CritPercentage.Value);
            if (IsBitSet(47)) data.WriteFloat(a.RangedCritPercentage.Value);
            if (IsBitSet(48)) data.WriteFloat(a.OffhandCritPercentage.Value);
            if (IsBitSet(49)) data.WriteInt32(a.ShieldBlock.Value);
            // 50: ShieldBlockCritPercentage skipped
            if (IsBitSet(51)) data.WriteFloat(a.Mastery.Value);
            if (IsBitSet(52)) data.WriteFloat(a.Speed.Value);
            if (IsBitSet(53)) data.WriteFloat(a.Avoidance.Value);
            if (IsBitSet(54)) data.WriteFloat(a.Sturdiness.Value);
            if (IsBitSet(55)) data.WriteInt32(a.Versatility.Value);
            if (IsBitSet(56)) data.WriteFloat(a.VersatilityBonus.Value);
            if (IsBitSet(57)) data.WriteFloat(a.PvpPowerDamage.Value);
            if (IsBitSet(58)) data.WriteFloat(a.PvpPowerHealing.Value);
            if (IsBitSet(59)) data.WriteInt32(a.ModHealingDonePos.Value);
            if (IsBitSet(60)) data.WriteFloat(a.ModHealingPercent.Value);
            if (IsBitSet(61)) data.WriteFloat(a.ModHealingDonePercent.Value);
            if (IsBitSet(62)) data.WriteFloat(a.ModPeriodicHealingDonePercent.Value);
            if (IsBitSet(63)) data.WriteFloat(a.ModSpellPowerPercent.Value);
            if (IsBitSet(64)) data.WriteFloat(a.ModResiliencePercent.Value);
            if (IsBitSet(65)) data.WriteFloat(a.OverrideSpellPowerByAPPercent.Value);
            if (IsBitSet(66)) data.WriteFloat(a.OverrideAPBySpellPowerPercent.Value);
            if (IsBitSet(67)) data.WriteInt32(a.ModTargetResistance.Value);
            if (IsBitSet(68)) data.WriteInt32(a.ModTargetPhysicalResistance.Value);
            if (IsBitSet(69)) data.WriteUInt32(a.LocalFlags.Value);
        }

        // ============================================================
        // WRITE SCALAR DATA — Block 70 (bits 71-101)
        // ============================================================
        if (IsBitSet(70))
        {
            if (IsBitSet(71)) data.WriteUInt8(a.GrantableLevels.Value);
            if (IsBitSet(72)) data.WriteUInt8(a.MultiActionBars.Value);
            if (IsBitSet(73)) data.WriteUInt8(a.LifetimeMaxRank.Value);
            if (IsBitSet(74)) data.WriteUInt8(a.NumRespecs.Value);
            if (IsBitSet(75)) data.WriteInt32((int)a.AmmoID.Value);
            if (IsBitSet(76)) data.WriteUInt32(a.PvpMedals.Value);
            if (IsBitSet(77)) data.WriteUInt16(a.TodayHonorableKills.Value);
            if (IsBitSet(78)) data.WriteUInt16(a.TodayDishonorableKills.Value);
            if (IsBitSet(79)) data.WriteUInt16(a.YesterdayHonorableKills.Value);
            if (IsBitSet(80)) data.WriteUInt16(a.YesterdayDishonorableKills.Value);
            if (IsBitSet(81)) data.WriteUInt16(a.LastWeekHonorableKills.Value);
            if (IsBitSet(82)) data.WriteUInt16(a.LastWeekDishonorableKills.Value);
            if (IsBitSet(83)) data.WriteUInt16(a.ThisWeekHonorableKills.Value);
            if (IsBitSet(84)) data.WriteUInt16(a.ThisWeekDishonorableKills.Value);
            if (IsBitSet(85)) data.WriteUInt32(a.ThisWeekContribution.Value);
            if (IsBitSet(86)) data.WriteUInt32(a.LifetimeHonorableKills.Value);
            if (IsBitSet(87)) data.WriteUInt32(a.LifetimeDishonorableKills.Value);
            // 88: Field_F24 skipped
            if (IsBitSet(89)) data.WriteUInt32(a.YesterdayContribution.Value);
            if (IsBitSet(90)) data.WriteUInt32(a.LastWeekContribution.Value);
            if (IsBitSet(91)) data.WriteUInt32(a.LastWeekRank.Value);
            if (IsBitSet(92)) data.WriteInt32(a.WatchedFactionIndex.Value);
            if (IsBitSet(93)) data.WriteInt32(a.MaxLevel.Value);
            if (IsBitSet(94)) data.WriteInt32(a.ScalingPlayerLevelDelta.Value);
            if (IsBitSet(95)) data.WriteInt32(a.MaxCreatureScalingLevel.Value);
            if (IsBitSet(96)) data.WriteInt32(a.PetSpellPower.Value);
            if (IsBitSet(97)) data.WriteFloat(a.UiHitModifier.Value);
            if (IsBitSet(98)) data.WriteFloat(a.UiSpellHitModifier.Value);
            if (IsBitSet(99)) data.WriteInt32(a.HomeRealmTimeOffset.Value);
            if (IsBitSet(100)) data.WriteFloat(a.ModPetHaste.Value);
            if (IsBitSet(101)) data.WriteUInt8(a.LocalRegenFlags.Value);
        }

        // ============================================================
        // WRITE SCALAR DATA — Block 102 (bits 103-123)
        // ============================================================
        if (IsBitSet(102))
        {
            if (IsBitSet(103)) data.WriteUInt8(a.AuraVision.Value);
            if (IsBitSet(104)) data.WriteUInt8(a.NumBackpackSlots.Value);
            if (IsBitSet(105)) data.WriteInt32(a.OverrideSpellsID.Value);
            if (IsBitSet(106)) data.WriteInt32(a.LfgBonusFactionID.Value);
            if (IsBitSet(107)) data.WriteUInt16((ushort)a.LootSpecID.Value);
            if (IsBitSet(108)) data.WriteUInt32(a.OverrideZonePVPType.Value);
            if (IsBitSet(109)) data.WriteInt32(a.Honor.Value);
            if (IsBitSet(110)) data.WriteInt32(a.HonorNextLevel.Value);
            // 111: Field_F74 skipped
            if (IsBitSet(112)) data.WriteInt32((int)a.PvPTierMaxFromWins.Value);
            if (IsBitSet(113)) data.WriteInt32((int)a.PvPLastWeeksTierMaxFromWins.Value);
            if (IsBitSet(114)) data.WriteUInt8(a.PvPRankProgress.Value);
            // 115-119 skipped
            if (IsBitSet(120)) data.WriteUInt8(this._gameState.GlyphsEnabled);
            // 121-123 skipped
            data.FlushBits(); // TC343 flushes here before complex struct fields (116/117/122)
        }

        // ============================================================
        // WRITE ARRAY DATA — TC343 write order
        // ============================================================

        // InvSlots (header 124, elements 125-265)
        if (IsBitSet(124))
        {
            for (int i = 0; i < 141; i++)
            {
                if (IsBitSet(125 + i))
                {
                    WowGuid128 guid = GetModernInvSlot(a, i) ?? WowGuid128.Empty;
                    data.WritePackedGuid128(guid);
                }
            }
        }

        // TrackResourceMask (header 266, elements 267-268)
        if (IsBitSet(266))
        {
            for (int i = 0; i < 2; i++)
                if (IsBitSet(267 + i))
                    data.WriteUInt32(a.TrackResourceMask[i].Value);
        }

        // SpellCritPercentage (header 269, elements 270-276)
        // ModDamageDonePos (header 269, elements 277-283)
        // ModDamageDoneNeg (header 269, elements 284-290)
        // ModDamageDonePercent (header 269, elements 291-297)
        if (IsBitSet(269))
        {
            for (int i = 0; i < 7; i++)
                if (IsBitSet(270 + i))
                    data.WriteFloat(a.SpellCritPercentage[i].Value);
            for (int i = 0; i < 7; i++)
                if (IsBitSet(277 + i))
                    data.WriteInt32(a.ModDamageDonePos[i].Value);
            for (int i = 0; i < 7; i++)
                if (IsBitSet(284 + i))
                    data.WriteInt32(a.ModDamageDoneNeg[i].Value);
            for (int i = 0; i < 7; i++)
                if (IsBitSet(291 + i))
                    data.WriteFloat(a.ModDamageDonePercent[i].Value);
        }

        // ExploredZones (header 298, elements 299-538)
        if (IsBitSet(298))
        {
            for (int i = 0; i < 240; i++)
                if (IsBitSet(299 + i))
                    data.WriteUInt64(a.ExploredZones[i].Value);
        }

        // RestInfo (header 539, elements 540-541) — nested struct HasChangesMask<3>
        if (IsBitSet(539))
        {
            for (int i = 0; i < 2; i++)
            {
                if (IsBitSet(540 + i))
                {
                    var ri = a.RestInfo[i];
                    uint restMask = 0;
                    if (ri != null && ri.Threshold.HasValue) restMask |= 2;
                    if (ri != null && ri.StateID.HasValue) restMask |= 4;
                    if (restMask != 0) restMask |= 1; // group bit
                    data.WriteBits(restMask, 3);
                    data.FlushBits();
                    if ((restMask & 2) != 0) data.WriteUInt32(ri.Threshold.Value);
                    if ((restMask & 4) != 0) data.WriteUInt8((byte)ri.StateID.Value);
                }
            }
        }

        // WeaponDmgMultipliers (header 542, elements 543-545)
        // WeaponAtkSpeedMultipliers (header 542, elements 546-548)
        if (IsBitSet(542))
        {
            for (int i = 0; i < 3; i++)
                if (IsBitSet(543 + i))
                    data.WriteFloat(a.WeaponDmgMultipliers[i].Value);
            for (int i = 0; i < 3; i++)
                if (IsBitSet(546 + i))
                    data.WriteFloat(a.WeaponAtkSpeedMultipliers[i].Value);
        }

        // BuybackPrice (header 549, elements 550-561)
        // BuybackTimestamp (header 549, elements 562-573)
        if (IsBitSet(549))
        {
            for (int i = 0; i < 12; i++)
                if (IsBitSet(550 + i))
                    data.WriteUInt32(a.BuybackPrice[i].Value);
            for (int i = 0; i < 12; i++)
                if (IsBitSet(562 + i))
                    data.WriteInt64((long)a.BuybackTimestamp[i].Value);
        }

        // CombatRatings (header 574, elements 575-606)
        if (IsBitSet(574))
        {
            for (int i = 0; i < 32; i++)
                if (IsBitSet(575 + i))
                    data.WriteInt32(a.CombatRatings[i].Value);
        }

        // NoReagentCostMask (header 615, elements 616-619)
        if (IsBitSet(615))
        {
            for (int i = 0; i < 4; i++)
                if (IsBitSet(616 + i))
                    data.WriteUInt32(a.NoReagentCostMask[i].Value);
        }

        // ProfessionSkillLine (header 620, elements 621-622)
        if (IsBitSet(620))
        {
            for (int i = 0; i < 2; i++)
                if (IsBitSet(621 + i))
                    data.WriteInt32(a.ProfessionSkillLine[i].Value);
        }

        // BagSlotFlags (header 623, elements 624-627)
        if (IsBitSet(623))
        {
            for (int i = 0; i < 4; i++)
                if (IsBitSet(624 + i))
                    data.WriteUInt32(a.BagSlotFlags[i].Value);
        }

        // BankBagSlotFlags (header 628, elements 629-635)
        if (IsBitSet(628))
        {
            for (int i = 0; i < 7; i++)
                if (IsBitSet(629 + i))
                    data.WriteUInt32(a.BankBagSlotFlags[i].Value);
        }

        // QuestCompleted (header 636, elements 637-1511)
        if (IsBitSet(636))
        {
            for (int i = 0; i < 875; i++)
                if (IsBitSet(637 + i))
                    data.WriteUInt64(a.QuestCompleted[i].Value);
        }

        // GlyphSlots (header 1512, elements 1513-1518) + Glyphs (elements 1519-1524)
        if (IsBitSet(1512))
        {
            uint[] glyphSlotIds = { 21, 22, 23, 24, 25, 26 };
            for (int i = 0; i < 6; i++)
                if (IsBitSet(1513 + i))
                    data.WriteUInt32(glyphSlotIds[i]);
            for (int i = 0; i < 6; i++)
                if (IsBitSet(1519 + i))
                    data.WriteUInt32((uint)(this._gameState.ActiveGlyphs[i]));
        }

        // PvpInfo (header 607, elements 608-614) — nested struct HasChangesMask<19>
        if (IsBitSet(607))
        {
            for (int i = 0; i < 7; i++)
            {
                if (IsBitSet(608 + i))
                {
                    PVPInfo pi = (a.PvpInfo != null && i < a.PvpInfo.Length) ? a.PvpInfo[i] : null;
                    // Build 19-bit changesMask for this PvpInfo entry
                    uint pvpMask = 0;
                    if (pi != null)
                    {
                        // Bit 1: Disqualified, 2: Bracket, 3: PvpRatingID
                        // 4: WeeklyPlayed, 5: WeeklyWon, 6: SeasonPlayed, 7: SeasonWon
                        // 8: Rating, 9: WeeklyBestRating, 10: SeasonBestRating
                        // 11: PvpTierID, 12: WeeklyBestWinPvpTierID, 13: Field_28, 14: Field_2C
                        // 15-18: Round stats (not in HermesProxy PVPInfo)
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
                    if (pvpMask != 0) pvpMask |= 1; // group bit

                    // Write 19-bit mask then data
                    data.WriteBits(pvpMask, 19);
                    if ((pvpMask & (1u << 1)) != 0) data.WriteBit(pi.Disqualified);
                    data.FlushBits();
                    if ((pvpMask & 1) != 0)
                    {
                        // Bit 2: Bracket (int8) — not in HermesProxy, write 0
                        // Bit 3: PvpRatingID (int32) — not in HermesProxy, write 0
                        if ((pvpMask & (1u << 4)) != 0) data.WriteUInt32(pi.WeeklyPlayed);
                        if ((pvpMask & (1u << 5)) != 0) data.WriteUInt32(pi.WeeklyWon);
                        if ((pvpMask & (1u << 6)) != 0) data.WriteUInt32(pi.SeasonPlayed);
                        if ((pvpMask & (1u << 7)) != 0) data.WriteUInt32(pi.SeasonWon);
                        if ((pvpMask & (1u << 8)) != 0) data.WriteUInt32(pi.Rating);
                        if ((pvpMask & (1u << 9)) != 0) data.WriteUInt32(pi.WeeklyBestRating);
                        if ((pvpMask & (1u << 10)) != 0) data.WriteUInt32(pi.SeasonBestRating);
                        if ((pvpMask & (1u << 11)) != 0) data.WriteUInt32(pi.PvpTierID);
                        if ((pvpMask & (1u << 12)) != 0) data.WriteUInt32(pi.WeeklyBestWinPvpTierID);
                        if ((pvpMask & (1u << 13)) != 0) data.WriteUInt32(pi.Field_28);
                        if ((pvpMask & (1u << 14)) != 0) data.WriteUInt32(pi.Field_2C);
                    }
                }
            }
        }

        data.FlushBits();
    }

    // === WriteUpdateItemData (fork lines 1554-1660) ===
    private void WriteUpdateItemData(WorldPacket data)
    {
        ItemData item = _updateData.ItemData;
        if (item == null)
        {
            data.WriteBits(0, 2);
            data.FlushBits();
            return;
        }

        // ItemData changesMask: 43 bits = 2 blocks of 32
        // TC343 bit layout:
        //   0: group bit for bits 1-22
        //   1: ArtifactPowers (dynamic), 2: Gems (dynamic)
        //   3: Owner, 4: ContainedIn, 5: Creator, 6: GiftCreator
        //   7: StackCount, 8: Expiration/Duration, 9: DynamicFlags/Flags
        //  10: PropertySeed, 11: RandomPropertiesID, 12: Durability, 13: MaxDurability
        //  14: CreatePlayedTime, 15: Context, 16: CreateTime, 17: ArtifactXP
        //  18: ItemAppearanceModID, 19: Modifiers, 20: DynamicFlags2, 21: ItemBonusKey
        //  22: DEBUGItemLevel
        //  23: group bit for SpellCharges[5] (bits 24-28)
        //  29: group bit for Enchantment[13] (bits 30-42)
        uint[] blocks = new uint[2];
        void SetBit(int bit) { blocks[bit / 32] |= (1u << (bit % 32)); }

        if (item.Owner != null) { SetBit(0); SetBit(3); }
        if (item.ContainedIn != null) { SetBit(0); SetBit(4); }
        if (item.Creator != null) { SetBit(0); SetBit(5); }
        if (item.GiftCreator != null) { SetBit(0); SetBit(6); }
        if (item.StackCount.HasValue) { SetBit(0); SetBit(7); }
        if (item.Duration.HasValue) { SetBit(0); SetBit(8); }
        if (item.Flags.HasValue) { SetBit(0); SetBit(9); }
        if (item.PropertySeed.HasValue) { SetBit(0); SetBit(10); }
        if (item.RandomProperty.HasValue) { SetBit(0); SetBit(11); }
        if (item.Durability.HasValue) { SetBit(0); SetBit(12); }
        if (item.MaxDurability.HasValue) { SetBit(0); SetBit(13); }
        if (item.CreatePlayedTime.HasValue) { SetBit(0); SetBit(14); }
        if (item.Context.HasValue) { SetBit(0); SetBit(15); }
        if (item.ArtifactXP.HasValue) { SetBit(0); SetBit(17); }
        if (item.ItemAppearanceModID.HasValue) { SetBit(0); SetBit(18); }
        for (int i = 0; i < 5; i++)
            if (item.SpellCharges[i].HasValue) { SetBit(23); SetBit(24 + i); }
        for (int i = 0; i < 13; i++)
            if (item.Enchantment[i] != null) { SetBit(29); SetBit(30 + i); }

        // Write blocksMask (2 bits) then each set block (32 bits)
        byte blocksMask = 0;
        if (blocks[0] != 0) blocksMask |= 1;
        if (blocks[1] != 0) blocksMask |= 2;
        data.WriteBits(blocksMask, 2);
        for (int b = 0; b < 2; b++)
            if ((blocksMask & (1 << b)) != 0)
                data.WriteBits(blocks[b], 32);

        // No dynamic fields (ArtifactPowers/Gems not used)
        data.FlushBits();

        // Group 0 scalar fields (bits 3-22)
        if ((blocks[0] & 1) != 0)
        {
            if (item.Owner != null) data.WritePackedGuid128(item.Owner.Value);
            if (item.ContainedIn != null) data.WritePackedGuid128(item.ContainedIn.Value);
            if (item.Creator != null) data.WritePackedGuid128(item.Creator.Value);
            if (item.GiftCreator != null) data.WritePackedGuid128(item.GiftCreator.Value);
            if (item.StackCount.HasValue) data.WriteUInt32(item.StackCount.Value);
            if (item.Duration.HasValue) data.WriteUInt32(item.Duration.Value);
            if (item.Flags.HasValue) data.WriteUInt32(item.Flags.Value);
            if (item.PropertySeed.HasValue) data.WriteInt32((int)item.PropertySeed.Value);
            if (item.RandomProperty.HasValue) data.WriteInt32((int)item.RandomProperty.Value);
            if (item.Durability.HasValue) data.WriteUInt32(item.Durability.Value);
            if (item.MaxDurability.HasValue) data.WriteUInt32(item.MaxDurability.Value);
            if (item.CreatePlayedTime.HasValue) data.WriteUInt32(item.CreatePlayedTime.Value);
            if (item.Context.HasValue) data.WriteInt32(item.Context.Value);
            if (item.ArtifactXP.HasValue) data.WriteUInt64(item.ArtifactXP.Value);
            if (item.ItemAppearanceModID.HasValue) data.WriteUInt8((byte)item.ItemAppearanceModID.Value);
        }

        // SpellCharges array (group bit 23, entries 24-28)
        if ((blocks[0] & (1u << 23)) != 0)
        {
            for (int i = 0; i < 5; i++)
                if (item.SpellCharges[i].HasValue)
                    data.WriteInt32(item.SpellCharges[i].Value);
        }

        // Enchantment array (group bit 29, entries 30-42)
        if ((blocks[0] & (1u << 29)) != 0)
        {
            for (int i = 0; i < 13; i++)
            {
                if (item.Enchantment[i] != null)
                {
                    // ItemEnchantment WriteUpdate: 4-bit mask + fields
                    uint enchMask = 0;
                    if (item.Enchantment[i].ID.HasValue) enchMask |= 2;
                    if (item.Enchantment[i].Duration.HasValue) enchMask |= 4;
                    if (item.Enchantment[i].Charges.HasValue) enchMask |= 8;
                    if (enchMask != 0) enchMask |= 1;
                    data.WriteBits(enchMask, 4);
                    data.FlushBits();
                    if (item.Enchantment[i].ID.HasValue) data.WriteInt32(item.Enchantment[i].ID.Value);
                    if (item.Enchantment[i].Duration.HasValue) data.WriteUInt32(item.Enchantment[i].Duration.Value);
                    if (item.Enchantment[i].Charges.HasValue) data.WriteUInt16(item.Enchantment[i].Charges.Value);
                }
            }
        }
    }

    private void WriteUpdateContainerData(WorldPacket data)
    {
        ContainerData container = _updateData.ContainerData;
        if (container == null)
        {
            data.WriteBits(0, 2);
            data.FlushBits();
            return;
        }

        // ContainerData changesMask: 39 bits in 2 blocks of 32
        // bit 0: group bit for NumSlots (block 0)
        // bit 1: NumSlots
        // bit 2: group bit for Slots[36]
        // bits 3..38: Slots[0..35] individual change bits
        uint[] blocks = new uint[2];
        void SetBit(int bit) { blocks[bit / 32] |= (1u << (bit % 32)); }

        if (container.NumSlots.HasValue) { SetBit(0); SetBit(1); }
        for (int i = 0; i < 36; i++)
            if (container.Slots[i].HasValue) { SetBit(2); SetBit(3 + i); }

        byte blocksMask = 0;
        if (blocks[0] != 0) blocksMask |= 1;
        if (blocks[1] != 0) blocksMask |= 2;
        data.WriteBits(blocksMask, 2);
        for (int b = 0; b < 2; b++)
            if ((blocksMask & (1 << b)) != 0)
                data.WriteBits(blocks[b], 32);
        data.FlushBits();

        if ((blocks[0] & (1u << 1)) != 0)
            data.WriteUInt32(container.NumSlots.Value);
        if ((blocks[0] & (1u << 2)) != 0)
        {
            for (int i = 0; i < 36; i++)
                if (container.Slots[i].HasValue)
                    data.WritePackedGuid128(container.Slots[i].Value);
        }
    }

    // === WriteUpdateGameObjectData (fork lines 3122-3187) ===
    private void WriteUpdateGameObjectData(WorldPacket data)
    {
        GameObjectData go = _updateData.GameObjectData ?? new GameObjectData();

        uint mask = 0;
        void SetBit(int bit) { mask |= (1u << bit); }
        bool IsBitSet(int bit) { return (mask & (1u << bit)) != 0; }

        // Set bits for changed fields
        if (go.DisplayID.HasValue) { SetBit(0); SetBit(4); }
        if (go.SpellVisualID.HasValue) { SetBit(0); SetBit(5); }
        if (go.StateSpellVisualID.HasValue) { SetBit(0); SetBit(6); }
        if (go.StateAnimID.HasValue) { SetBit(0); SetBit(7); }
        if (go.StateAnimKitID.HasValue) { SetBit(0); SetBit(8); }
        if (go.CreatedBy != null) { SetBit(0); SetBit(9); }
        if (go.GuildGUID != null) { SetBit(0); SetBit(10); }
        if (go.Flags.HasValue) { SetBit(0); SetBit(11); }
        bool hasRotation = false;
        if (go.ParentRotation != null)
            for (int i = 0; i < 4; i++)
                if (go.ParentRotation[i].HasValue) hasRotation = true;
        if (hasRotation) { SetBit(0); SetBit(12); }
        if (go.FactionTemplate.HasValue) { SetBit(0); SetBit(13); }
        if (go.Level.HasValue) { SetBit(0); SetBit(14); }
        if (go.State.HasValue) { SetBit(0); SetBit(15); }
        if (go.TypeID.HasValue) { SetBit(0); SetBit(16); }
        if (go.PercentHealth.HasValue) { SetBit(0); SetBit(17); }
        if (go.ArtKit.HasValue) { SetBit(0); SetBit(18); }
        if (go.CustomParam.HasValue) { SetBit(0); SetBit(19); }

        if (mask != 0)
        {
            var fields = new System.Collections.Generic.List<string>(8);
            if (IsBitSet(4)) fields.Add($"DisplayID={go.DisplayID.Value}");
            if (IsBitSet(5)) fields.Add($"SpellVisualID={go.SpellVisualID.Value}");
            if (IsBitSet(6)) fields.Add($"StateSpellVisualID={go.StateSpellVisualID.Value}");
            if (IsBitSet(7)) fields.Add($"StateAnimID={go.StateAnimID.Value}");
            if (IsBitSet(8)) fields.Add($"StateAnimKitID={go.StateAnimKitID.Value}");
            if (IsBitSet(9)) fields.Add($"CreatedBy={go.CreatedBy.Value}");
            if (IsBitSet(10)) fields.Add($"GuildGUID={go.GuildGUID.Value}");
            if (IsBitSet(11)) fields.Add($"Flags=0x{go.Flags.Value:X8}");
            if (IsBitSet(12)) fields.Add($"ParentRotation=({go.ParentRotation[0] ?? 0f},{go.ParentRotation[1] ?? 0f},{go.ParentRotation[2] ?? 0f},{go.ParentRotation[3] ?? 1f})");
            if (IsBitSet(13)) fields.Add($"FactionTemplate={go.FactionTemplate.Value}");
            if (IsBitSet(14)) fields.Add($"Level={go.Level.Value}");
            if (IsBitSet(15)) fields.Add($"State={go.State.Value}");
            if (IsBitSet(16)) fields.Add($"TypeID={go.TypeID.Value}");
            if (IsBitSet(17)) fields.Add($"PercentHealth={go.PercentHealth.Value}");
            if (IsBitSet(18)) fields.Add($"ArtKit={go.ArtKit.Value}");
            if (IsBitSet(19)) fields.Add($"CustomParam={go.CustomParam.Value}");
            Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
                $"[GO write] guid={_updateData.Guid} mask=0x{mask:X8} fields={{ {string.Join(", ", fields)} }}");
        }

        // V3_4_3 GameObjectData wire format is a flat 20-bit changesMask — no 1-bit
        // blocksMask prefix (see TC wotlk_classic UpdateFields.cpp:4918-4920 and WPP
        // V3_4_0 UpdateFieldsHandler343.ReadUpdateGameObjectData line 3704). The
        // earlier `WriteBits(blocksMask, 1) + WriteBits(mask, 20)` shifted every bit
        // by one position; the client decoded `0x58801` as `0xAC400`, tried to parse
        // GuildGUID/Level/State at bogus offsets, and disconnected with reason=7
        // immediately after the SMSG_UPDATE_OBJECT (signature: chained-initiate
        // unlock cast → cage GO Values update → DC).
        data.WriteBits(mask, 20);

        // Boundary 1 — TC UpdateFields.cpp:4936. Aligns after the optional bit-1
        // StateWorldEffectIDs sub-section (size + entries). We don't translate
        // StateWorldEffectIDs from the legacy server yet, but the alignment is
        // still required because the byte-aligned field writes below would
        // otherwise see 4 stray bits left over from the 20-bit changesMask above.
        data.FlushBits();

        // (Implicit boundary 2 — TC UpdateFields.cpp:4954.) Would align after the
        // optional bit-2/3 EnableDoodadSets / WorldEffects DynamicUpdateField mask
        // preambles. We don't emit those either, so a second FlushBits would be a
        // no-op today (FlushBits short-circuits when already byte-aligned). If
        // support for bits 1/2/3 is added later, restore a FlushBits() call here
        // BEFORE the field-value block.

        // Write field values in TC343 order (bits 4-19)
        if (IsBitSet(0))
        {
            if (IsBitSet(4)) data.WriteInt32(go.DisplayID.Value);
            if (IsBitSet(5)) data.WriteUInt32(go.SpellVisualID.Value);
            if (IsBitSet(6)) data.WriteUInt32(go.StateSpellVisualID.Value);
            if (IsBitSet(7)) data.WriteUInt32(go.StateAnimID.Value);
            if (IsBitSet(8)) data.WriteUInt32(go.StateAnimKitID.Value);
            if (IsBitSet(9)) data.WritePackedGuid128(go.CreatedBy.Value);
            if (IsBitSet(10)) data.WritePackedGuid128(go.GuildGUID.Value);
            if (IsBitSet(11)) data.WriteUInt32(go.Flags.Value);
            if (IsBitSet(12))
            {
                data.WriteFloat(go.ParentRotation[0] ?? 0f);
                data.WriteFloat(go.ParentRotation[1] ?? 0f);
                data.WriteFloat(go.ParentRotation[2] ?? 0f);
                data.WriteFloat(go.ParentRotation[3] ?? 1f);
            }
            if (IsBitSet(13)) data.WriteInt32(go.FactionTemplate.Value);
            if (IsBitSet(14)) data.WriteInt32(go.Level.Value);
            if (IsBitSet(15)) data.WriteInt8(go.State.Value);
            if (IsBitSet(16)) data.WriteInt8(go.TypeID.Value);
            if (IsBitSet(17)) data.WriteUInt8(go.PercentHealth.Value);
            if (IsBitSet(18)) data.WriteUInt32(go.ArtKit.Value);
            if (IsBitSet(19)) data.WriteUInt32(go.CustomParam.Value);
        }
    }


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

        // Phase 5a diagnostic — log the player's UnitData fields most likely to cause
        // ERROR #132 ACCESS_VIOLATION crashes (null model dereference).
        if (_updateData.UnitData != null && _objectType == ObjectTypeBCC.ActivePlayer)
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
            if (_objectType == ObjectTypeBCC.ActivePlayer)
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
        if (_objectType == ObjectTypeBCC.ActivePlayer)
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
        if (_updateData.Type != UpdateTypeModern.Values
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
    }
}
