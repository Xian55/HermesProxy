using Framework.Util;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Legacy 3.3.5a -> modern V3_4_3 (build 54261) achievement bridge.
    // Layouts: legacy = CMaNGOS mangos-wotlk AchievementMgr.cpp BuildAllDataPacket
    // / SendCriteriaUpdate / earned-broadcast; modern = TC 3.4.3
    // AchievementPackets.{h,cpp}. Version-gated to V3_0_2+ so V1_14/V2_5 fall
    // through unchanged.

    [PacketHandler(Opcode.SMSG_ALL_ACHIEVEMENT_DATA)]
    void HandleAllAchievementData(WorldPacket packet)
    {
        if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            return;

        var gameState = GetSession().GameState;
        var ownerGuid = gameState.CurrentPlayerGuid;
        uint realmAddress = GetSession().RealmId.GetAddress();

        var data = new AllAchievementData();

        // Earned achievements — loop until 0xFFFFFFFF terminator.
        while (true)
        {
            uint achievementId = packet.ReadUInt32();
            if (achievementId == 0xFFFFFFFF)
                break;
            uint packedDate = packet.ReadUInt32();
            data.Earned.Add(new EarnedAchievement
            {
                Id = achievementId,
                Date = Time.GetUnixTimeFromPackedTime(packedDate),
                Owner = ownerGuid,
                VirtualRealmAddress = realmAddress,
                NativeRealmAddress = realmAddress,
            });
        }

        // Criteria progress — loop until 0xFFFFFFFF terminator.
        while (true)
        {
            uint criteriaId = packet.ReadUInt32();
            if (criteriaId == 0xFFFFFFFF)
                break;
            ulong counter = packet.ReadPackedGuid().Low;   // legacy packs counter as PackedGuid64
            packet.ReadPackedGuid();                       // legacy player PackedGuid64 — already known
            uint flags = packet.ReadUInt32();              // 1 = criteriaFailed, else 0
            uint packedDate = packet.ReadUInt32();
            uint timeFromStart = packet.ReadUInt32();
            uint timeFromCreate = packet.ReadUInt32();

            data.Progress.Add(new CriteriaProgressPkt
            {
                Id = criteriaId,
                Quantity = counter,
                Player = ownerGuid,
                Flags = flags,
                Date = Time.GetUnixTimeFromPackedTime(packedDate),
                TimeFromStart = timeFromStart,
                TimeFromCreate = timeFromCreate,
            });
        }

        SendPacketToClient(data);
    }

    [PacketHandler(Opcode.SMSG_CRITERIA_UPDATE)]
    void HandleCriteriaUpdate(WorldPacket packet)
    {
        if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            return;

        var gameState = GetSession().GameState;
        var ownerGuid = gameState.CurrentPlayerGuid;

        uint criteriaId = packet.ReadUInt32();
        ulong counter = packet.ReadPackedGuid().Low;
        packet.ReadPackedGuid();                           // legacy player PackedGuid64
        uint flags = packet.ReadUInt32();
        uint packedDate = packet.ReadUInt32();
        uint elapsed = packet.ReadUInt32();
        uint created = packet.ReadUInt32();

        var update = new CriteriaUpdatePkt
        {
            CriteriaID = criteriaId,
            Quantity = counter,
            PlayerGUID = ownerGuid,
            Flags = flags,
            CurrentTime = Time.GetUnixTimeFromPackedTime(packedDate),
            ElapsedTime = elapsed,
            CreationTime = created,
        };
        SendPacketToClient(update);
    }

    [PacketHandler(Opcode.SMSG_ACHIEVEMENT_EARNED)]
    void HandleAchievementEarned(WorldPacket packet)
    {
        if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            return;

        var gameState = GetSession().GameState;
        var earnerGuid64 = packet.ReadPackedGuid();
        uint achievementId = packet.ReadUInt32();
        uint packedDate = packet.ReadUInt32();
        packet.ReadUInt32();                               // legacy effect-skip placeholder, unused
        uint realmAddress = GetSession().RealmId.GetAddress();

        var earnerGuid128 = earnerGuid64.To128(gameState);
        SendPacketToClient(new AchievementEarnedPkt
        {
            // Legacy carries one GUID (the earner); modern wants both. Mirror it.
            Sender = earnerGuid128,
            Earner = earnerGuid128,
            AchievementID = achievementId,
            Time = Time.GetUnixTimeFromPackedTime(packedDate),
            EarnerNativeRealm = realmAddress,
            EarnerVirtualRealm = realmAddress,
            Initial = false,
        });
    }
}
