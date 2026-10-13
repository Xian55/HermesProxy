using System.Collections.Generic;
using Framework.Logging;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Account-data and CUF-profile CMSGs: the client's own saved settings.
/// </summary>
/// <remarks>
/// None of these reach the legacy server. 3.3.5a stores per-account data too, but the modern
/// client's blobs are its own format and the proxy keeps them in <c>AccountDataMgr</c>, answering
/// the client from there. The one exception is the raid frame profiles on a 4.3.4 server, which
/// keeps them for the character itself.
/// </remarks>
public static class ClientConfigSystem
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melLog = Log.CreateMelLogger(Log.CategoryPacket);
    private static readonly string _sourceFile = nameof(ClientConfigSystem).PadRight(15);

    [HandlesCmsg(Opcode.CMSG_UPDATE_ACCOUNT_DATA)]
    public static void HandleUpdateAccountData(in UserClientUpdateAccountData data, in SessionContext ctx)
    {
        byte[] compressed = data.CompressedData;
        ctx.GetSession().AccountDataMgr.SaveData(data.PlayerGuid, data.Time, data.DataType, data.Size, compressed);
    }

    [HandlesCmsg(Opcode.CMSG_REQUEST_ACCOUNT_DATA)]
    public static void HandleRequestAccountData(in RequestAccountData data, in SessionContext ctx)
    {
        if (ctx.GetSession().AccountDataMgr.Data[data.DataType] == null)
        {
            Log.Print(LogType.Error, $"Client requested missing account data {data.DataType}.");
            ctx.GetSession().AccountDataMgr.Data[data.DataType] = new();
            ctx.GetSession().AccountDataMgr.Data[data.DataType].Type = data.DataType;
            ctx.GetSession().AccountDataMgr.Data[data.DataType].Timestamp = Time.UnixTime;
            ctx.GetSession().AccountDataMgr.Data[data.DataType].UncompressedSize = 0;
            ctx.GetSession().AccountDataMgr.Data[data.DataType].CompressedData = new byte[0];
        }

        ctx.GetSession().AccountDataMgr.Data[data.DataType].Guid = data.PlayerGuid;
        AccountData stored = ctx.GetSession().AccountDataMgr.Data[data.DataType];

        UpdateAccountData update = new(stored);
        ctx.SendPacket(update);
    }

    [HandlesCmsg(Opcode.CMSG_SAVE_CUF_PROFILES)]
    public static void HandleSaveCUFProfiles(in SaveCUFProfiles cuf, in SessionContext ctx)
    {
        if (HermesProxy.World.Client.WorldClient.IsCataLegacy)
        {
            SaveCUFProfilesToServer(cuf.Data, in ctx);
            return;
        }

        ctx.GetSession().AccountDataMgr.SaveCUFProfiles(cuf.Data);
    }

    /// <summary>
    /// A 4.3.4 server keeps the raid frame profiles of a character, so they go there and not into
    /// the proxy's own store. What a profile has on the server and the 4.4.2 client has no bit
    /// for is put back as the server last sent it.
    /// </summary>
    private static void SaveCUFProfilesToServer(byte[] blob, in SessionContext ctx)
    {
        List<CufProfile> profiles = [];
        if (!CufProfiles.TryReadModern(blob, profiles))
        {
            WorldSocketLogMessages.CufProfilesUnreadable(_melLog, _sourceFile, "", blob.Length);
            return;
        }

        var legacyOnly = ctx.GameState.CufOptionsKeptForServer;
        foreach (CufProfile profile in profiles)
        {
            if (legacyOnly.TryGetValue(profile.Name, out CufLegacyOnlyOptions kept))
                profile.LegacyOnly = kept;
        }

        WorldPacket packet = new WorldPacket(Opcode.CMSG_SAVE_CUF_PROFILES);
        CufProfiles.WriteLegacy(packet, profiles);
        ctx.SendPacketToServer(packet);
    }
}
