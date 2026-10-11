using System;
using Framework.Util;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
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

    [HandlesSmsg(Opcode.SMSG_ALL_ACHIEVEMENT_DATA)]
    internal void HandleAllAchievementData(WorldPacket packet)
    {
        if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            return;

        var gameState = GetSession().GameState;
        var ownerGuid = gameState.CurrentPlayerGuid;
        uint realmAddress = GetSession().RealmId.GetAddress();

        var data = new AllAchievementData();
        if (IsCataLegacy)
        {
            ReadAllAchievementDataCata(packet, data, ownerGuid, realmAddress);
            SendPacketToClient(data);
            return;
        }

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

    /// <summary>
    /// TrinityCore 4.3.4 AchievementMgr::SendAllAchievementData: the criteria count and each
    /// criterion's masked counter and owner GUID bits come first, then each criterion's bytes
    /// with those GUID bytes scattered between its fields, then the earned achievements. The
    /// counts replace 3.3.5a's terminators.
    /// </summary>
    private static void ReadAllAchievementDataCata(WorldPacket packet, AllAchievementData data, WowGuid128 ownerGuid, uint realmAddress)
    {
        int criteriaCount = (int)packet.ReadBits<uint>(21);
        // Per criterion: 8 owner GUID bits, then 8 counter bits.
        var masks = new bool[criteriaCount * 16];
        for (int i = 0; i < criteriaCount; i++)
        {
            Span<bool> guid = masks.AsSpan(i * 16, 8);
            Span<bool> counter = masks.AsSpan(i * 16 + 8, 8);
            guid[4] = packet.HasBit();
            counter[3] = packet.HasBit();
            guid[5] = packet.HasBit();
            counter[0] = packet.HasBit();
            counter[6] = packet.HasBit();
            guid[3] = packet.HasBit();
            guid[0] = packet.HasBit();
            counter[4] = packet.HasBit();
            guid[2] = packet.HasBit();
            counter[7] = packet.HasBit();
            guid[7] = packet.HasBit();
            packet.ReadBits<uint>(2);                     // Flags
            guid[6] = packet.HasBit();
            counter[2] = packet.HasBit();
            counter[1] = packet.HasBit();
            counter[5] = packet.HasBit();
            guid[1] = packet.HasBit();
        }
        int achievementCount = (int)packet.ReadBits<uint>(23);
        packet.ResetBitPos();

        Span<byte> guidBytes = stackalloc byte[8];
        Span<byte> counterBytes = stackalloc byte[8];
        for (int i = 0; i < criteriaCount; i++)
        {
            ReadOnlySpan<bool> guid = masks.AsSpan(i * 16, 8);
            ReadOnlySpan<bool> counter = masks.AsSpan(i * 16 + 8, 8);
            guidBytes.Clear();
            counterBytes.Clear();

            MaskedGuid.ReadByte(packet, guid, guidBytes, 3);
            MaskedGuid.ReadByte(packet, counter, counterBytes, 5);
            MaskedGuid.ReadByte(packet, counter, counterBytes, 6);
            MaskedGuid.ReadByte(packet, guid, guidBytes, 4);
            MaskedGuid.ReadByte(packet, guid, guidBytes, 6);
            MaskedGuid.ReadByte(packet, counter, counterBytes, 2);
            uint timeFromCreate = packet.ReadUInt32();
            MaskedGuid.ReadByte(packet, guid, guidBytes, 2);
            uint criteriaId = packet.ReadUInt32();
            MaskedGuid.ReadByte(packet, guid, guidBytes, 5);
            MaskedGuid.ReadByte(packet, counter, counterBytes, 0);
            MaskedGuid.ReadByte(packet, counter, counterBytes, 3);
            MaskedGuid.ReadByte(packet, counter, counterBytes, 1);
            MaskedGuid.ReadByte(packet, counter, counterBytes, 4);
            MaskedGuid.ReadByte(packet, guid, guidBytes, 0);
            MaskedGuid.ReadByte(packet, guid, guidBytes, 7);
            MaskedGuid.ReadByte(packet, counter, counterBytes, 7);
            uint timeFromStart = packet.ReadUInt32();
            uint packedDate = packet.ReadUInt32();
            MaskedGuid.ReadByte(packet, guid, guidBytes, 1);

            data.Progress.Add(new CriteriaProgressPkt
            {
                Id = criteriaId,
                Quantity = MaskedGuid.ToUInt64(counterBytes),
                Player = ownerGuid,
                Date = Time.GetUnixTimeFromPackedTime(packedDate),
                TimeFromStart = timeFromStart,
                TimeFromCreate = timeFromCreate,
            });
        }

        for (int i = 0; i < achievementCount; i++)
        {
            uint achievementId = packet.ReadUInt32();
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
    }

    [HandlesSmsg(Opcode.SMSG_CRITERIA_UPDATE)]
    internal void HandleCriteriaUpdate(WorldPacket packet)
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

    [HandlesSmsg(Opcode.SMSG_CRITERIA_DELETED)]
    internal void HandleCriteriaDeleted(WorldPacket packet)
    {
        if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            return;

        SendPacketToClient(new CriteriaDeletedPkt { CriteriaID = packet.ReadUInt32() });
    }

    [HandlesSmsg(Opcode.SMSG_ACHIEVEMENT_EARNED)]
    internal void HandleAchievementEarned(WorldPacket packet)
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
