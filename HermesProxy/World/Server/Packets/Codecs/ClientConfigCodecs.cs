using System;
using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World.Dispatch;

namespace HermesProxy.World.Server.Packets;

// Account-data CMSG codecs.
//
// Both account-data reads pick their DataType width from ModernVersion.GetAccountDataCount(), which
// is static readonly, so the branch folds at JIT time rather than costing anything per packet. The
// width is not a build-boundary question — it keys off a count that several builds share — so it
// stays inside one codec rather than becoming a pair of ranged ones.

public static class UserClientUpdateAccountDataCodec
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

public static class RequestAccountDataCodec
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

public static class SaveCUFProfilesCodec
{
    /// <remarks>Copies for the same reason as the account-data blob — this is stored, not consumed.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SaveCUFProfiles packet)
        => packet = new SaveCUFProfiles(r.ReadToEndArray());
}
