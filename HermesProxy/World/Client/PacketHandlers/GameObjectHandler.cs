using Framework;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [HandlesSmsg(Opcode.SMSG_GAME_OBJECT_DESPAWN)]
    internal void HandleGameObjectDespawn(WorldPacket packet)
    {
        WowGuid64 guid = packet.ReadGuid();
        GameObjectDespawn despawn = new GameObjectDespawn();
        despawn.ObjectGUID = guid.To128(GetSession().GameState);
        SendPacketToClient(despawn);
        GetSession().GameState.DespawnedGameObjects.Add(guid);
    }

    // The per-hit damage event for a destructible building. Without it the client shows no
    // floating damage number on a gate or wall — the health bar still moves, because that is
    // carried separately by the GameObjectData.PercentHealth Values update, so the building
    // just loses health silently.
    //
    // Legacy 3.3.5a and modern carry the same five fields in the same order, so this is a
    // straight guid widening. Verified against a native 3.4.3 capture of a siege vehicle
    // hitting a Strand of the Ancients gate.
    [HandlesSmsg(Opcode.SMSG_DESTRUCTIBLE_BUILDING_DAMAGE)]
    internal void HandleDestructibleBuildingDamage(WorldPacket packet)
    {
        DestructibleBuildingDamage damage = new DestructibleBuildingDamage();
        damage.Target = packet.ReadPackedGuid().To128(GetSession().GameState);
        damage.Caster = packet.ReadPackedGuid().To128(GetSession().GameState);
        damage.Owner = packet.ReadPackedGuid().To128(GetSession().GameState);
        damage.Damage = packet.ReadUInt32();
        damage.SpellID = packet.ReadInt32();
        SendPacketToClient(damage);
    }

    [HandlesSmsg(Opcode.SMSG_GAME_OBJECT_RESET_STATE)]
    internal void HandleGameObjectResetState(WorldPacket packet)
    {
        GameObjectResetState reset = new GameObjectResetState();
        reset.ObjectGUID = packet.ReadGuid().To128(GetSession().GameState);
        SendPacketToClient(reset);
    }

    [HandlesSmsg(Opcode.SMSG_GAME_OBJECT_CUSTOM_ANIM)]
    internal void HandleGameObjectCustomAnim(WorldPacket packet)
    {
        GameObjectCustomAnim anim = new GameObjectCustomAnim();
        anim.ObjectGUID = packet.ReadGuid().To128(GetSession().GameState);
        anim.CustomAnim = packet.ReadUInt32();
        SendPacketToClient(anim);
    }

    [HandlesSmsg(Opcode.SMSG_FISH_NOT_HOOKED)]
    internal void HandleFishNotHooked(WorldPacket packet)
    {
        FishNotHooked fish = new FishNotHooked();
        SendPacketToClient(fish);
    }

    [HandlesSmsg(Opcode.SMSG_FISH_ESCAPED)]
    internal void HandleFishEscaped(WorldPacket packet)
    {
        FishEscaped fish = new FishEscaped();
        SendPacketToClient(fish);
    }
}
