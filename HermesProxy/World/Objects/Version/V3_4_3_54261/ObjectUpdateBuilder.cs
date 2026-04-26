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
        if (_objectType == ObjectTypeBCC.GameObject)
            _createBits |= CreateObjectBits.GameObject;
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
            const bool hasRuneState = false;
            const bool hasActionButtons = true;
            data.WriteBit(hasSceneInstanceIDs);
            data.WriteBit(hasRuneState);
            data.WriteBit(hasActionButtons);
            data.FlushBits();
            for (int j = 0; j < 180; j++)
                data.WriteInt32(j < _gameState.ActionButtons.Count ? _gameState.ActionButtons[j] : 0);
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
        data.WriteUInt32(0u);
        if (IsOwner)
            data.WriteUInt16(0);
        data.WriteInt32(0);
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
        data.WriteUInt32(0u);
        if (IsOwner)
            data.WriteUInt16(0);
        data.WriteInt32(0);
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
        data.WriteUInt8(0);
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
        data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        data.WriteUInt32(0u);
        data.WritePackedGuid128(WowGuid128.Empty);
        data.WriteInt32(0);
        data.WriteFloat(0f);
        data.WriteUInt32(0u);
        if (IsOwner)
            data.WritePackedGuid128(WowGuid128.Empty);
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

        // QuestLog[25] — gated by PartyMember flag (0x02) in TC343.
        if (IsOwner)
        {
            for (int q = 0; q < 25; q++)
            {
                var quest = player.QuestLog != null && q < player.QuestLog.Length ? player.QuestLog[q] : null;
                data.WriteInt64(quest?.EndTime ?? 0);
                data.WriteInt32(quest?.QuestID ?? 0);
                data.WriteUInt32(quest?.StateFlags ?? 0);
                for (int obj = 0; obj < 24; obj++)
                    data.WriteUInt16((ushort)(quest?.ObjectiveProgress[obj] ?? 0));
            }
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
        data.WriteInt64(0L);
        data.WriteUInt32(0u);
        data.WriteInt32(0);
        data.WritePackedGuid128(WowGuid128.Empty);
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

        // InvSlots[141] mapped from legacy arrays via GetModernInvSlot.
        for (int i = 0; i < 141; i++)
            data.WritePackedGuid128(GetModernInvSlot(active, i) ?? WowGuid128.Empty);

        data.WritePackedGuid128(active.FarsightObject ?? WowGuid128.Empty);
        data.WritePackedGuid128(WowGuid128.Empty);
        data.WriteUInt32(0u);
        data.WriteUInt64(active.Coinage.GetValueOrDefault());
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
        for (int l = 0; l < 240; l++)
            data.WriteUInt64(0uL);
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
        data.WriteInt32(0);
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
        var createData = _updateData.CreateData;
        if (createData != null && createData.MoveInfo != null)
        {
            var rot = createData.MoveInfo.Rotation;
            data.WriteFloat(rot.X);
            data.WriteFloat(rot.Y);
            data.WriteFloat(rot.Z);
            data.WriteFloat(rot.W);
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
        data.WriteUInt8(go.PercentHealth ?? 100);
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
    private static void WriteUpdateSkillInfo(WorldPacket data, SkillInfo? s)
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
            Framework.Logging.Log.Print(Framework.Logging.LogType.Network,
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
        Framework.Logging.Log.Print(Framework.Logging.LogType.Network,
            $"[Phase5aTrace]   {label} ({len} bytes) head={hex}");
    }

    // FIXME(phase5a-7d): minimal stub. The full Update-path methods (~1,700 LOC of
    // bit-mask serialization for partial updates) land in a follow-up commit after
    // the Create path is smoke-tested end-to-end. Returning an empty mask here means
    // partial UpdateObject packets carry no field deltas — the client keeps the last
    // known value for those fields. Acceptable for initial world-enter (cmangos
    // re-sends a full CreateObject when the partial update is rejected with
    // CMSG_OBJECT_UPDATE_FAILED), but live combat / health bars / aura ticks /
    // movement Values updates won't propagate until this is implemented.
    private static void WriteValuesUpdate(WorldPacket data)
    {
        data.WriteUInt32(0u);
    }

    private void WriteValuesModern(WorldPacket packet)
    {
        var valuesBuffer = new WorldPacket();
        if (_updateData.Type == UpdateTypeModern.Values)
            WriteValuesUpdate(valuesBuffer);
        else
            WriteValuesCreate(valuesBuffer);

        var valuesData = valuesBuffer.GetData();
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
            Framework.Logging.Log.Print(Framework.Logging.LogType.Network,
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
            Framework.Logging.Log.Print(Framework.Logging.LogType.Network,
                $"[Phase5aTrace] ActivePlayer packet bytes={len} first80={hex}");
        }
    }
}
