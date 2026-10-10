using System;
using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;

namespace HermesProxy.World.Server.Packets;

// Account-data CMSG codecs.
//
// Both account-data reads pick their DataType width from ModernVersion.GetAccountDataCount(), which
// is static readonly, so the branch folds at JIT time rather than costing anything per packet. The
// width is not a build-boundary question — it keys off a count that several builds share — so it
// stays inside one codec rather than becoming a pair of ranged ones.
//
// 4.4.2 is a build boundary, though: both packets carry DataType as a uint32 there, and
// CMSG_UPDATE_ACCOUNT_DATA puts Time and Size ahead of the player GUID (TrinityCore cata_classic
// ClientConfigPackets.cpp).

[PacketCodec(typeof(UserClientUpdateAccountData), RemovedIn = ClientVersionBuild.V4_4_2_60895)]
public static class UserClientUpdateAccountDataCodecPreCataClassic
{
    /// <remarks>
    /// CompressedData must be a copy, not a view: the reader spans a pooled rental that the next
    /// packet reuses, and AccountDataMgr stores this array on the session.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out UserClientUpdateAccountData packet)
    {
        WowGuid128 playerGuid = r.ReadPackedGuid128();
        long time = r.ReadInt64();
        uint size = r.ReadUInt32();
        uint dataType = ModernVersion.GetAccountDataCount() <= 8
            ? r.ReadBits<uint>(3)
            : r.ReadBits<uint>(4);

        uint compressedSize = r.ReadUInt32();
        byte[] compressed = compressedSize != 0 ? r.ReadBytes(compressedSize) : Array.Empty<byte>();

        packet = new UserClientUpdateAccountData(playerGuid, time, size, dataType, compressed);
    }
}

[PacketCodec(typeof(UserClientUpdateAccountData), AddedIn = ClientVersionBuild.V4_4_2_60895)]
public static class UserClientUpdateAccountDataCodecCataClassic
{
    /// <remarks>Copies the blob for the same reason as the earlier layout.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out UserClientUpdateAccountData packet)
    {
        long time = r.ReadInt64();
        uint size = r.ReadUInt32();
        WowGuid128 playerGuid = r.ReadPackedGuid128();
        uint dataType = r.ReadUInt32();

        uint compressedSize = r.ReadUInt32();
        byte[] compressed = compressedSize != 0 ? r.ReadBytes(compressedSize) : Array.Empty<byte>();

        packet = new UserClientUpdateAccountData(playerGuid, time, size, dataType, compressed);
    }
}

[PacketCodec(typeof(RequestAccountData), RemovedIn = ClientVersionBuild.V4_4_2_60895)]
public static class RequestAccountDataCodecPreCataClassic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out RequestAccountData packet)
    {
        WowGuid128 playerGuid = r.ReadPackedGuid128();
        uint dataType = ModernVersion.GetAccountDataCount() <= 8
            ? r.ReadBits<uint>(3)
            : r.ReadBits<uint>(4);

        packet = new RequestAccountData(playerGuid, dataType);
    }
}

[PacketCodec(typeof(RequestAccountData), AddedIn = ClientVersionBuild.V4_4_2_60895)]
public static class RequestAccountDataCodecCataClassic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out RequestAccountData packet)
    {
        WowGuid128 playerGuid = r.ReadPackedGuid128();
        uint dataType = r.ReadUInt32();

        packet = new RequestAccountData(playerGuid, dataType);
    }
}

public static class SaveCUFProfilesCodec
{
    /// <remarks>Copies for the same reason as the account-data blob — this is stored, not consumed.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SaveCUFProfiles packet)
        => packet = new SaveCUFProfiles(r.ReadToEndArray());
}
