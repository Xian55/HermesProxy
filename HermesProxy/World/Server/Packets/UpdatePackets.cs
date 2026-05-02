/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Framework.Constants;
using Framework.GameMath;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class CreateObjectData
{
    public ObjectType ObjectType;
    public MovementInfo MoveInfo = null!;
    public ServerSideMovement MoveSpline = null!;
    public bool NoBirthAnim;
    public bool EnablePortals;
    public bool PlayHoverAnim;
    public bool ThisIsYou;
    public WowGuid128? AutoAttackVictim;
}
public class ObjectUpdate
{
    public ObjectUpdate(WowGuid128 guid, UpdateTypeModern type, GlobalSessionData globalSession)
    {
        Type = type;
        Guid = guid;
        GlobalSession = globalSession;
        ObjectData = new ObjectData();

        switch (type)
        {
            case UpdateTypeModern.CreateObject1:
            case UpdateTypeModern.CreateObject2:
                CreateData = new CreateObjectData();
                break;
        }

        switch (guid.GetObjectType())
        {
            case ObjectType.Item:
            case ObjectType.Container:
                ItemData = new ItemData();
                ContainerData = new ContainerData();
                break;
            case ObjectType.Unit:
                UnitData = new UnitData();
                break;
            case ObjectType.Player:
            case ObjectType.ActivePlayer:
                UnitData = new UnitData();
                PlayerData = new PlayerData();
                ActivePlayerData = new ActivePlayerData();
                break;
            case ObjectType.GameObject:
                GameObjectData = new GameObjectData();
                break;
            case ObjectType.DynamicObject:
                DynamicObjectData = new DynamicObjectData();
                break;
            case ObjectType.Corpse:
                CorpseData = new CorpseData();
                break;
        }
    }

    public UpdateTypeModern Type;
    public WowGuid128 Guid;
    public GlobalSessionData GlobalSession;
    public CreateObjectData CreateData = null!;
    public ObjectData ObjectData;
    public ItemData ItemData = null!;
    public ContainerData ContainerData = null!;
    public UnitData UnitData = null!;
    public PlayerData PlayerData = null!;
    public ActivePlayerData ActivePlayerData = null!;
    public GameObjectData GameObjectData = null!;
    public DynamicObjectData DynamicObjectData = null!;
    public CorpseData CorpseData = null!;

    public void InitializePlaceholders()
    {
        if (CreateData == null)
            return;

        if (CreateData.MoveInfo != null)
        {
            if (CreateData.MoveInfo.WalkSpeed == 0)
                CreateData.MoveInfo.WalkSpeed = 2.5f;
            if (CreateData.MoveInfo.RunSpeed == 0)
                CreateData.MoveInfo.RunSpeed = 7;
            if (CreateData.MoveInfo.RunBackSpeed == 0)
                CreateData.MoveInfo.RunBackSpeed = 4.5f;
            if (CreateData.MoveInfo.SwimSpeed == 0)
                CreateData.MoveInfo.SwimSpeed = 4.722222f;
            if (CreateData.MoveInfo.SwimBackSpeed == 0)
                CreateData.MoveInfo.SwimBackSpeed = 2.5f;
            if (CreateData.MoveInfo.FlightSpeed == 0)
                CreateData.MoveInfo.FlightSpeed = 7;
            if (CreateData.MoveInfo.FlightBackSpeed == 0)
                CreateData.MoveInfo.FlightBackSpeed = 4.5f;
            if (CreateData.MoveInfo.TurnRate == 0)
                CreateData.MoveInfo.TurnRate = 3.141594f;
            if (CreateData.MoveInfo.PitchRate == 0)
                CreateData.MoveInfo.PitchRate = CreateData.MoveInfo.TurnRate;
            if (CreateData.MoveInfo.Flags.HasAnyFlag(MovementFlagModern.WalkMode) && (CreateData.MoveSpline != null))
                CreateData.MoveInfo.Flags &= ~(uint)MovementFlagModern.WalkMode;
            if (CreateData.MoveInfo.FlagsExtra == 0)
                CreateData.MoveInfo.FlagsExtra = 512;
        }
        if (CreateData.MoveSpline != null)
        {
            if (CreateData.MoveSpline.SplineFlags == 0)
                CreateData.MoveSpline.SplineFlags = (SplineFlagModern)2432696320;
        }
        if (GameObjectData != null)
        {
            if ((GameObjectData.PercentHealth == null) &&
                (GameObjectData.State != null || GameObjectData.TypeID != null || GameObjectData.ArtKit != null))
                GameObjectData.PercentHealth = 255;
            if (GameObjectData.ParentRotation[3] == null)
                GameObjectData.ParentRotation[3] = 1;
            if (GameObjectData.StateAnimID == null)
                GameObjectData.StateAnimID = ModernVersion.GetGameObjectStateAnimId();
            if (Guid.GetHighType() == HighGuidType.Transport)
            {
                var transportTimer = CreateData.MoveInfo!.TransportPathTimer;
                uint period = GameData.GetTransportPeriod((uint)ObjectData.EntryID!);
                if (period != 0)
                {
                    if (GameObjectData.Level == null)
                        GameObjectData.Level = (int)period;
                    if (ObjectData.DynamicFlags == null)
                        ObjectData.DynamicFlags = (((uint)(((float)(transportTimer % period) / (float)period) * System.UInt16.MaxValue)) << 16);
                    GameObjectData.Flags = 1048616;
                }
                else if (ObjectData.DynamicFlags == null)
                    ObjectData.DynamicFlags = ((transportTimer % System.UInt16.MaxValue) << 16);
            }
        }
        if (CorpseData != null)
        {
            if (CorpseData.ClassId == null)
            {
                if (CorpseData.Owner != null)
                    CorpseData.ClassId = (byte)GlobalSession.GameState.GetUnitClass(CorpseData.Owner.Value);
                else
                    CorpseData.ClassId = 1;
            }
            if (CorpseData.FactionTemplate == null && CorpseData.Owner != null)
            {
                int ownerFaction = GlobalSession.GameState.GetLegacyFieldValueInt32(CorpseData.Owner.Value, UnitField.UNIT_FIELD_FACTIONTEMPLATE);
                if (ownerFaction != 0)
                    CorpseData.FactionTemplate = ownerFaction;
                else if (CorpseData.RaceId != null)
                    CorpseData.FactionTemplate = (int)GameData.GetFactionForRace((uint)CorpseData.RaceId);
            }
        }
        if (UnitData != null)
        {
            for (int i = 0; i < 6; i++)
            {
                if (UnitData.ModPowerRegen[i] == null)
                    UnitData.ModPowerRegen[i] = 1;
            }
            if (UnitData.Flags2 == null)
                UnitData.Flags2 = 2048;
            if (UnitData.DisplayScale == null)
                UnitData.DisplayScale = 1;
            if (UnitData.NativeXDisplayScale == null)
                UnitData.NativeXDisplayScale = 1;
            if (UnitData.ModCastHaste == null)
                UnitData.ModCastHaste = 1;
            if (UnitData.ModHaste == null)
                UnitData.ModHaste = 1;
            if (UnitData.ModRangedHaste == null)
                UnitData.ModRangedHaste = 1;
            if (UnitData.ModHasteRegen == null)
                UnitData.ModHasteRegen = 1;
            if (UnitData.ModTimeRate == null)
                UnitData.ModTimeRate = 1;
            if (UnitData.HoverHeight == null)
                UnitData.HoverHeight = 1;
            if (UnitData.ScaleDuration == null)
                UnitData.ScaleDuration = 100;
            if (UnitData.LookAtControllerID == null)
                UnitData.LookAtControllerID = -1;
            if (UnitData.ChannelObject == null &&
                Guid == GlobalSession.GameState.CurrentPlayerGuid)
                UnitData.ChannelObject = WowGuid128.Empty;
        }
        if (PlayerData != null)
        {
            if (PlayerData.WowAccount == null)
            {
                if (CreateData.ThisIsYou == true)
                    PlayerData.WowAccount = WowGuid128.Create(HighGuidType703.WowAccount, GlobalSession.GameAccountInfo.Id);
                else
                    PlayerData.WowAccount = WowGuid128.Create(HighGuidType703.WowAccount, Guid.GetCounter());
            }
            if (PlayerData.VirtualPlayerRealm == null)
                PlayerData.VirtualPlayerRealm = GlobalSession.RealmId.GetAddress();
            if (PlayerData.HonorLevel == null)
                PlayerData.HonorLevel = 1;
            if (PlayerData.AvgItemLevel[3] == null)
                PlayerData.AvgItemLevel[3] = 1;
        }
        if (ActivePlayerData != null)
        {
            if (ActivePlayerData.RestInfo[0] == null)
                ActivePlayerData.RestInfo[0] = new RestInfo();
            if (ActivePlayerData.RestInfo[0].Threshold == null)
                ActivePlayerData.RestInfo[0].Threshold = 1;
            if (ActivePlayerData.RestInfo[0].StateID == null)
                ActivePlayerData.RestInfo[0].StateID = 0;
            for (int i = 0; i < 7; i++)
            {
                if (ActivePlayerData.ModDamageDonePercent[i] == null)
                    ActivePlayerData.ModDamageDonePercent[i] = 1;
            }
            if (ActivePlayerData.ModHealingPercent == null)
                ActivePlayerData.ModHealingPercent = 1;
            if (ActivePlayerData.ModHealingDonePercent == null)
                ActivePlayerData.ModHealingDonePercent = 1;
            if (ActivePlayerData.ModPeriodicHealingDonePercent == null)
                ActivePlayerData.ModPeriodicHealingDonePercent = 1;
            for (int i = 0; i < 3; i++)
            {
                if (ActivePlayerData.WeaponDmgMultipliers[i] == null)
                    ActivePlayerData.WeaponDmgMultipliers[i] = 1;
                if (ActivePlayerData.WeaponAtkSpeedMultipliers[i] == null)
                    ActivePlayerData.WeaponAtkSpeedMultipliers[i] = 1;
            }
            if (ActivePlayerData.ModSpellPowerPercent == null)
                ActivePlayerData.ModSpellPowerPercent = 1;
            if (ActivePlayerData.NumBackpackSlots == null)
                ActivePlayerData.NumBackpackSlots = 16;
            if (ActivePlayerData.MultiActionBars == null)
                ActivePlayerData.MultiActionBars = 7;
            if (ActivePlayerData.MaxLevel == null)
                ActivePlayerData.MaxLevel = LegacyVersion.GetMaxLevel();
            if (ActivePlayerData.ModPetHaste == null)
                ActivePlayerData.ModPetHaste = 1;
            if (ActivePlayerData.HonorNextLevel == null)
                ActivePlayerData.HonorNextLevel = 5500;
            if (ActivePlayerData.PvPTierMaxFromWins == null)
                ActivePlayerData.PvPTierMaxFromWins = 4294967295;
            if (ActivePlayerData.PvPLastWeeksTierMaxFromWins == null)
                ActivePlayerData.PvPLastWeeksTierMaxFromWins = 4294967295;
        }
    }
}

public class UpdateObject : ServerPacket
{
    public UpdateObject(GameSessionData gameState) : base(Opcode.SMSG_UPDATE_OBJECT, ConnectionType.Instance)
    {
        _gameState = gameState;
    }

    /// <summary>
    /// V3_4_3 Values-update filter. Drops Values entries that target an unknown guid
    /// (no prior CreateObject) or carry no concrete field changes — cMangos emits
    /// the latter as bookkeeping no-ops that materialize as a 13-byte garbage body
    /// the V3_4_3 client rejects with CMSG_OBJECT_UPDATE_FAILED.
    ///
    /// Player Values now pass through unchanged. They used to be sanitized via a
    /// StripPlayerCrashingBlocks helper (per fork research/player_values_update_crash.md),
    /// but that strip is unnecessary now that UpdateHandler splits player Values
    /// into a dedicated SMSG_UPDATE_OBJECT (port of fork commit 18caaf7) — once
    /// the player's deltas are no longer intermixed with creature deltas, the
    /// changedMask cascade is well-formed and the client accepts blocks 0/1/4 plus
    /// PlayerData and ActivePlayerData. Removing the strip restores Coinage,
    /// InvSlots, DisplayPower and the rage bar.
    /// </summary>
    public static int FilterV3_4_3Values(UpdateObject obj, GameSessionData gameState)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return 0;

        int beforeCount = obj.ObjectUpdates.Count;
        var known = gameState.ClientKnownGuids;

        // First pass: register every CreateObject in this batch as a guid the client
        // will know after this packet is sent. We add BEFORE the strip pass so that a
        // Values entry later in the same batch (uncommon but legal) wouldn't be
        // dropped just because we register guids only after.
        int createKept = 0;
        foreach (var u in obj.ObjectUpdates)
        {
            if (u.Type == UpdateTypeModern.CreateObject1 || u.Type == UpdateTypeModern.CreateObject2)
            {
                known.Add(u.Guid);
                createKept++;
            }
        }

        int valuesKept = 0;
        int valuesUnknownStripped = 0;
        int valuesEmptyStripped = 0;
        obj.ObjectUpdates.RemoveAll(u =>
        {
            if (u.Type != UpdateTypeModern.Values)
                return false;
            if (!known.Contains(u.Guid))
            {
                valuesUnknownStripped++;
                return true;
            }
            if (IsEmptyValuesDelta(u))
            {
                valuesEmptyStripped++;
                return true;
            }
            valuesKept++;
            return false;
        });

        Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
            $"[UpdateObjectTrace] V3_4_3 filter: in={beforeCount} valuesKept={valuesKept} valuesEmpty={valuesEmptyStripped} valuesUnknown={valuesUnknownStripped} createKept={createKept} mapId={gameState.CurrentMapId}");

        return valuesUnknownStripped + valuesEmptyStripped;
    }

    private static bool IsEmptyValuesDelta(ObjectUpdate u)
    {
        // A Values delta is "empty" if no concrete field has a value. The data objects
        // (UnitData/PlayerData/ActivePlayerData) themselves may be non-null but contain
        // only nullable fields all set to null — the V3_4_3 client treats the resulting
        // bit-mask body as malformed.

        // ObjectData fields apply to every entity type. A Values update that only
        // clears UNIT_DYNAMIC_FLAGS (e.g. dropping the Lootable bit after loot
        // release) populates ObjectData.DynamicFlags = 0 and nothing else; without
        // this probe the filter classified that as empty and dropped it, leaving
        // the corpse sparkly on the modern client.
        var obj = u.ObjectData;
        if (obj != null)
        {
            if (obj.DynamicFlags.HasValue) return false;
            if (obj.EntryID.HasValue) return false;
            if (obj.Scale.HasValue) return false;
        }

        var unit = u.UnitData;
        if (unit != null)
        {
            if (unit.Health.HasValue || unit.MaxHealth.HasValue || unit.DisplayID.HasValue) return false;
            if (unit.Charm != null || unit.Summon != null || unit.CharmedBy != null) return false;
            if (unit.SummonedBy != null || unit.CreatedBy != null || unit.Target != null) return false;
            if (unit.ChannelData != null) return false;
            if (unit.RaceId.HasValue || unit.ClassId.HasValue || unit.SexId.HasValue) return false;
            if (unit.Level.HasValue || unit.EffectiveLevel.HasValue || unit.DisplayPower.HasValue) return false;
            if (unit.FactionTemplate.HasValue || unit.Flags.HasValue || unit.Flags2.HasValue || unit.Flags3.HasValue) return false;
            if (unit.AuraState.HasValue) return false;
            if (unit.BoundingRadius.HasValue || unit.CombatReach.HasValue) return false;
            if (unit.NativeDisplayID.HasValue || unit.MountDisplayID.HasValue) return false;
            if (unit.HoverHeight.HasValue || unit.GuildGUID != null) return false;
            if (unit.MinDamage.HasValue || unit.MaxDamage.HasValue) return false;
            if (unit.StandState.HasValue || unit.AnimTier.HasValue) return false;
            if (unit.AttackPower.HasValue || unit.RangedAttackPower.HasValue) return false;
            if (unit.BaseMana.HasValue || unit.BaseHealth.HasValue) return false;
            if (unit.NpcFlags != null)
                for (int i = 0; i < unit.NpcFlags.Length; i++)
                    if (unit.NpcFlags[i].HasValue && unit.NpcFlags[i] != 0) return false;
            if (unit.Power != null)
                for (int i = 0; i < unit.Power.Length; i++)
                    if (unit.Power[i].HasValue) return false;
            if (unit.MaxPower != null)
                for (int i = 0; i < unit.MaxPower.Length; i++)
                    if (unit.MaxPower[i].HasValue) return false;
            if (unit.Stats != null)
                for (int i = 0; i < unit.Stats.Length; i++)
                    if (unit.Stats[i].HasValue) return false;
            if (unit.Resistances != null)
                for (int i = 0; i < 7; i++)
                    if (unit.Resistances[i].HasValue) return false;
        }
        var player = u.PlayerData;
        if (player != null)
        {
            if (player.PlayerFlags.HasValue || player.PlayerFlagsEx.HasValue) return false;
            if (player.NativeSex.HasValue || player.HonorLevel.HasValue) return false;
            if (player.GuildRankID.HasValue || player.GuildLevel.HasValue) return false;
            if (player.DuelArbiter != null || player.WowAccount != null || player.LootTargetGUID != null) return false;
        }
        // ActivePlayerData has hundreds of fields; check the most common ones cMangos
        // populates as part of real updates. If none are set, treat as empty.
        var ap = u.ActivePlayerData;
        if (ap != null)
        {
            if (ap.Coinage.HasValue || ap.XP.HasValue || ap.NextLevelXP.HasValue) return false;
            if (ap.CharacterPoints.HasValue) return false;
            if (ap.FarsightObject != null) return false;
            if (ap.InvSlots != null)
                for (int i = 0; i < ap.InvSlots.Length; i++)
                    if (ap.InvSlots[i] != null) return false;
            // PackSlots / BankSlots / BankBagSlots / BuyBackSlots / KeyringSlots
            // are where cMaNGOS stores main-backpack and bank items. A Values
            // update that only adds a looted item to the backpack populates
            // PackSlots[N] but no other ActivePlayerData field — without these
            // checks the filter classified the delta as empty and dropped it,
            // leaving the V3_4_3 client unaware of the new item until relog.
            if (ap.PackSlots != null)
                for (int i = 0; i < ap.PackSlots.Length; i++)
                    if (ap.PackSlots[i] != null) return false;
            if (ap.BankSlots != null)
                for (int i = 0; i < ap.BankSlots.Length; i++)
                    if (ap.BankSlots[i] != null) return false;
            if (ap.BankBagSlots != null)
                for (int i = 0; i < ap.BankBagSlots.Length; i++)
                    if (ap.BankBagSlots[i] != null) return false;
            if (ap.BuyBackSlots != null)
                for (int i = 0; i < ap.BuyBackSlots.Length; i++)
                    if (ap.BuyBackSlots[i] != null) return false;
            if (ap.KeyringSlots != null)
                for (int i = 0; i < ap.KeyringSlots.Length; i++)
                    if (ap.KeyringSlots[i] != null) return false;
            if (ap.Skill != null)
            {
                for (int i = 0; i < 256; i++)
                    if (ap.Skill.SkillLineID[i].HasValue) return false;
            }
        }
        return true;
    }

    public override void Write()
    {
        // Filter is now invoked from UpdateHandler / QueryHandler BEFORE Write() so the
        // outer code can decide to skip the send when nothing useful remains. Leaving
        // a no-op call here as a safety net so that any caller that bypasses the
        // pre-filter still sees Values stripped.
        FilterV3_4_3Values(this, _gameState);

        NumObjUpdates = (uint)ObjectUpdates.Count;
        MapID = (ushort)_gameState.CurrentMapId!;

        _worldPacket.WriteUInt32(NumObjUpdates);
        _worldPacket.WriteUInt16(MapID);

        WorldPacket buffer = new();
        if (buffer.WriteBit(!OutOfRangeGuids.Empty() || !DestroyedGuids.Empty()))
        {
            buffer.WriteUInt16((ushort)DestroyedGuids.Count);
            buffer.WriteInt32(DestroyedGuids.Count + OutOfRangeGuids.Count);

            foreach (var destroyGuid in DestroyedGuids)
                buffer.WritePackedGuid128(destroyGuid);

            foreach (var outOfRangeGuid in OutOfRangeGuids)
                buffer.WritePackedGuid128(outOfRangeGuid);
        }

        WorldPacket data = new();
        foreach (var update in ObjectUpdates)
        {
            update.InitializePlaceholders();
            switch (ModernVersion.GetUpdateFieldsDefiningBuild())
            {
                case ClientVersionBuild.V1_14_0_40237:
                {
                    Objects.Version.V1_14_0_40237.ObjectUpdateBuilder builder = new Objects.Version.V1_14_0_40237.ObjectUpdateBuilder(update, _gameState);
                    builder.WriteToPacket(data);
                    break;
                }
                case ClientVersionBuild.V1_14_1_40688:
                {
                    Objects.Version.V1_14_1_40688.ObjectUpdateBuilder builder = new Objects.Version.V1_14_1_40688.ObjectUpdateBuilder(update, _gameState);
                    builder.WriteToPacket(data);
                    break;
                }
                case ClientVersionBuild.V2_5_2_39570:
                {
                    Objects.Version.V2_5_2_39570.ObjectUpdateBuilder builder = new Objects.Version.V2_5_2_39570.ObjectUpdateBuilder(update, _gameState);
                    builder.WriteToPacket(data);
                    break;
                }
                case ClientVersionBuild.V2_5_3_41750:
                {
                    Objects.Version.V2_5_3_41750.ObjectUpdateBuilder builder = new Objects.Version.V2_5_3_41750.ObjectUpdateBuilder(update, _gameState);
                    builder.WriteToPacket(data);
                    break;
                }
                case ClientVersionBuild.V3_4_3_54261:
                {
                    Objects.Version.V3_4_3_54261.ObjectUpdateBuilder builder = new Objects.Version.V3_4_3_54261.ObjectUpdateBuilder(update, _gameState);
                    builder.WriteToPacket(data);
                    break;
                }
                default:
                    throw new System.ArgumentOutOfRangeException("No object update builder defined for current build.");
            }
        }    
        
        var bytes = data.GetData();
        buffer.WriteInt32(bytes.Length);
        buffer.WriteBytes(bytes);
        Data = buffer.GetData();

        _worldPacket.WriteBytes(Data);
    }

    GameSessionData _gameState;
    public uint NumObjUpdates;
    public ushort MapID;
    public byte[] Data = Array.Empty<byte>();

    public List<WowGuid128> OutOfRangeGuids = new List<WowGuid128>();
    public List<WowGuid128> DestroyedGuids = new List<WowGuid128>();
    public List<ObjectUpdate> ObjectUpdates = new List<ObjectUpdate>();
}

public class HealthUpdate : ServerPacket
{
    // Modern V3_4_3 SMSG_HEALTH_UPDATE: PackedGuid128 Guid + int64 Health.
    // Reference: WPP V3_4_0 / TC343 — health is i64 in modern (post-Legion).
    public HealthUpdate(WowGuid128 guid) : base(Opcode.SMSG_HEALTH_UPDATE, ConnectionType.Instance)
    {
        Guid = guid;
    }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Guid);
        _worldPacket.WriteInt64(Health);
    }

    public WowGuid128 Guid;
    public long Health;
}

public class PowerUpdate : ServerPacket, ISpanWritable
{
    // WoW has ~20 power types (mana, rage, focus, energy, combo points, runes, etc.)
    // Practical cap is much lower since a unit only has a few power types
    private const int MaxPowerTypes = 16;

    public PowerUpdate(WowGuid128 guid) : base(Opcode.SMSG_POWER_UPDATE)
    {
        Guid = guid;
        Powers = new List<PowerUpdatePower>();
    }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Guid);
        _worldPacket.WriteInt32(Powers.Count);
        foreach (var power in Powers)
        {
            _worldPacket.WriteInt32(power.Power);
            _worldPacket.WriteUInt8(power.PowerType);
        }
    }

    // MaxSize: PackedGuid128 (18) + int (4) + 16 * (int (4) + byte (1)) = 102
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 4 + MaxPowerTypes * 5;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (Powers.Count > MaxPowerTypes)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Guid.Low, Guid.High);
        writer.WriteInt32(Powers.Count);

        foreach (var power in Powers)
        {
            writer.WriteInt32(power.Power);
            writer.WriteUInt8(power.PowerType);
        }

        return writer.Position;
    }

    public WowGuid128 Guid;
    public List<PowerUpdatePower> Powers;
}

public struct PowerUpdatePower
{
    public PowerUpdatePower(int power, byte powerType)
    {
        Power = power;
        PowerType = powerType;
    }

    public int Power;
    public byte PowerType;
}

public class ObjectUpdateFailed : ClientPacket
{
    public ObjectUpdateFailed(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        ObjectGuid = _worldPacket.ReadPackedGuid128();
    }

    public WowGuid128 ObjectGuid;
}
