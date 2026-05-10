using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace HermesProxy.World.Server;

public partial class WorldSocket
{
    // Handlers for CMSG opcodes coming from the modern client
    [PacketHandler(Opcode.CMSG_UPDATE_ACCOUNT_DATA)]
    void HandleUpdateAccountData(UserClientUpdateAccountData data)
    {
        byte[] compressed = data.CompressedData;
        Log.Print(LogType.Trace,
            $"[ActionBarTrace] CMSG_UPDATE_ACCOUNT_DATA type={data.DataType} ({(AccountDataType)data.DataType}) " +
            $"uncompressedSize={data.Size} compressedSize={compressed.Length} preview={DescribeCompressedConfigBlob(compressed, data.Size)}");
        GetSession().AccountDataMgr.SaveData(data.PlayerGuid, data.Time, data.DataType, data.Size, compressed);
    }

    [PacketHandler(Opcode.CMSG_REQUEST_ACCOUNT_DATA)]
    void HandleRequestAccountData(RequestAccountData data)
    {
        bool hadSlot = data.DataType < GetSession().AccountDataMgr.Data.Length
            && GetSession().AccountDataMgr.Data[data.DataType] != null;
        Log.Print(LogType.Trace,
            $"[ActionBarTrace] CMSG_REQUEST_ACCOUNT_DATA type={data.DataType} ({(AccountDataType)data.DataType}) hadSlot={hadSlot}");

        if (GetSession().AccountDataMgr.Data[data.DataType] == null)
        {
            Log.Print(LogType.Error, $"Client requested missing account data {data.DataType}.");
            GetSession().AccountDataMgr.Data[data.DataType] = new();
            GetSession().AccountDataMgr.Data[data.DataType].Type = data.DataType;
            GetSession().AccountDataMgr.Data[data.DataType].Timestamp = Time.UnixTime;
            GetSession().AccountDataMgr.Data[data.DataType].UncompressedSize = 0;
            GetSession().AccountDataMgr.Data[data.DataType].CompressedData = new byte[0];
        }

        GetSession().AccountDataMgr.Data[data.DataType].Guid = data.PlayerGuid;
        AccountData stored = GetSession().AccountDataMgr.Data[data.DataType];

        // V3_4_3: the modern UI ignores the legacy MultiActionBars descriptor field
        // and treats this request response as the authoritative source for global
        // CVars. Inject the saved action-bar visibility CVars on the way out so the
        // client UI renders the bars the user toggled in a previous session. We
        // build a one-shot augmented blob for THIS response only — never mutate
        // Data[0] or write to disk, because the disk file is account-shared while
        // the mask is per-character.
        if (data.DataType == (uint)AccountDataType.GlobalConfigCache &&
            ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            byte? mask = GetSession().GameState.CurrentPlayerStorage?.Settings?.MultiActionBarsMask;
            if (mask.HasValue)
            {
                (byte[] augCompressed, uint augUncompressed) =
                    AugmentGlobalConfigBlob(stored.CompressedData, mask.Value);
                AccountData augmented = new()
                {
                    Guid = stored.Guid,
                    Timestamp = stored.Timestamp,
                    Type = stored.Type,
                    UncompressedSize = augUncompressed,
                    CompressedData = augCompressed,
                };
                SendPacket(new UpdateAccountData(augmented));
                Log.Print(LogType.Trace,
                    $"[ActionBarTrace] augmented type-0 response with action-bar CVars (mask=0x{mask.Value:X2}) " +
                    $"originalSize={stored.UncompressedSize} augmentedSize={augUncompressed} compressedSize={augCompressed.Length}");
                return;
            }
        }

        UpdateAccountData update = new(stored);
        SendPacket(update);
    }

    // Decompress saved type-0 GlobalConfigCache blob, drop any pre-existing
    // action-bar visibility CVar lines so our values take precedence, append
    // synthesised values from the saved mask, and recompress. In-memory only;
    // never written back to disk (the disk file is account-shared but the mask
    // is per-character — writing would corrupt other characters' state).
    private static (byte[] compressed, uint uncompressedSize) AugmentGlobalConfigBlob(byte[] existingCompressed, byte mask)
    {
        var sb = new StringBuilder(2048);

        if (existingCompressed != null && existingCompressed.Length > 0)
        {
            try
            {
                using var src = new MemoryStream(existingCompressed);
                using var inflater = new ZLibStream(src, CompressionMode.Decompress);
                using var sr = new StreamReader(inflater, Encoding.UTF8);
                string existing = sr.ReadToEnd();
                foreach (var rawLine in existing.Split('\n'))
                {
                    string line = rawLine.TrimEnd('\r');
                    if (line.Length == 0) continue;
                    if (line.Contains("bottomLeftActionBar", StringComparison.OrdinalIgnoreCase)) continue;
                    if (line.Contains("bottomRightActionBar", StringComparison.OrdinalIgnoreCase)) continue;
                    if (line.Contains("rightActionBar", StringComparison.OrdinalIgnoreCase)) continue;
                    sb.Append(line).Append('\n');
                }
            }
            catch
            {
                // Disk blob corrupt → fall through and emit a fresh blob with just our CVars.
                sb.Clear();
            }
        }

        int b1 = (mask & 0x01) != 0 ? 1 : 0; // Action Bar 2 (bottom-left)
        int b2 = (mask & 0x02) != 0 ? 1 : 0; // Action Bar 3 (bottom-right)
        int b3 = (mask & 0x04) != 0 ? 1 : 0; // Action Bar 4 (right)
        int b4 = (mask & 0x08) != 0 ? 1 : 0; // Action Bar 5 (right 2)
        sb.Append($"SET bottomLeftActionBar \"{b1}\"\n");
        sb.Append($"SET bottomRightActionBar \"{b2}\"\n");
        sb.Append($"SET rightActionBar \"{b3}\"\n");
        sb.Append($"SET rightActionBar2 \"{b4}\"\n");

        byte[] uncompressed = Encoding.UTF8.GetBytes(sb.ToString());

        using var dst = new MemoryStream();
        using (var deflate = new ZLibStream(dst, CompressionLevel.Fastest, leaveOpen: true))
            deflate.Write(uncompressed, 0, uncompressed.Length);

        return (dst.ToArray(), (uint)uncompressed.Length);
    }

    // [ActionBarTrace] Decompress the wire blob and return a printable preview.
    // CMSG_UPDATE_ACCOUNT_DATA payloads are zlib-wrapped Lua-style "SET key val"
    // text; we want to confirm whether the V3_4_3 client sends action bar CVars
    // (bottomLeftActionBar etc.) and via which AccountDataType.
    private static string DescribeCompressedConfigBlob(byte[] compressed, uint uncompressedSize)
    {
        if (compressed == null || compressed.Length == 0)
            return "<empty>";
        try
        {
            using var src = new MemoryStream(compressed);
            using var inflater = new ZLibStream(src, CompressionMode.Decompress);
            int cap = (int)Math.Min(uncompressedSize, 256u);
            if (cap <= 0) cap = 256;
            byte[] buf = new byte[cap];
            int read = 0;
            int n;
            while (read < buf.Length && (n = inflater.Read(buf, read, buf.Length - read)) > 0)
                read += n;
            string text = Encoding.UTF8.GetString(buf, 0, read).Replace('\n', '|').Replace('\r', ' ');
            bool hasActionBarCVar = text.Contains("ActionBar", StringComparison.OrdinalIgnoreCase)
                || text.Contains("actionBar", StringComparison.Ordinal);
            return $"hasActionBarCVar={hasActionBarCVar} text=\"{text}\"";
        }
        catch (Exception ex)
        {
            return $"<inflate failed: {ex.GetType().Name}: {ex.Message}>";
        }
    }

    [PacketHandler(Opcode.CMSG_SAVE_CUF_PROFILES)]
    void HandleUpdateAccountData(SaveCUFProfiles cuf)
    {
        GetSession().AccountDataMgr.SaveCUFProfiles(cuf.Data);
    }
}
