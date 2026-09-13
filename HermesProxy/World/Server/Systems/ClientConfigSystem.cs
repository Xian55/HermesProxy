using Framework.Logging;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Account-data and CUF-profile CMSGs: the client's own saved settings.
/// </summary>
/// <remarks>
/// None of these reach the legacy server. 3.3.5a stores per-account data too, but the modern
/// client's blobs are its own format and the proxy keeps them in <c>AccountDataMgr</c>, answering
/// the client from there.
/// </remarks>
public static class ClientConfigSystem
{
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
        ctx.GetSession().AccountDataMgr.SaveCUFProfiles(cuf.Data);
    }
}
