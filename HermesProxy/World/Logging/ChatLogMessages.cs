using Microsoft.Extensions.Logging;

namespace HermesProxy.World.Logging;

/// <summary>
/// Source-generated logging for the chat translation path, replacing interpolated
/// <c>Log.Print(LogType.Trace, $"[ChatTrace] …")</c> calls.
/// </summary>
/// <remarks>
/// <c>Log.Print</c> takes an already-built string, so those calls paid for a <c>Substring</c>,
/// a concat and a full interpolation on every message even with Trace disabled — three times per
/// outgoing message and once per incoming one, on a path that carries every message from every
/// player in every joined channel. The preview itself still has to be computed by the caller, so
/// each site guards it with an explicit <c>IsEnabled</c> check: <c>[LoggerMessage]</c> stops the
/// formatting, not the argument evaluation.
/// </remarks>
internal static partial class ChatLogMessages
{
    // EventId 1400-1409 reserved for chat translation.

    [LoggerMessage(EventId = 1400, Level = LogLevel.Trace,
        Message = "[ChatTrace] CMSG_CHAT_MESSAGE_* received: opcode={Opcode} lang={Language} textLen={TextLength} preview=\"{Preview}\"")]
    public static partial void OutgoingReceived(ILogger logger, string Opcode, uint Language, int TextLength, string Preview);

    [LoggerMessage(EventId = 1401, Level = LogLevel.Trace,
        Message = "[ChatTrace] CMSG_CHAT_MESSAGE_EMOTE received: textLen={TextLength} preview=\"{Preview}\"")]
    public static partial void EmoteReceived(ILogger logger, int TextLength, string Preview);

    [LoggerMessage(EventId = 1402, Level = LogLevel.Trace,
        Message = "[ChatTrace] -> legacy CMSG_MESSAGECHAT (WotLK): type={Type} lang={Language} textLen={TextLength} channel=\"{Channel}\" to=\"{To}\" preview=\"{Preview}\"")]
    public static partial void ForwardedToLegacy(ILogger logger, string Type, uint Language, int TextLength, string Channel, string To, string Preview);

    [LoggerMessage(EventId = 1403, Level = LogLevel.Trace,
        Message = "[ChatTrace] handled internally as Hermes command, no legacy send")]
    public static partial void HandledAsInternalCommand(ILogger logger);

    [LoggerMessage(EventId = 1404, Level = LogLevel.Trace,
        Message = "[ChatTrace] <- legacy SMSG_CHAT (WotLK): chatType={ChatType} -> modern={ModernType} lang={Language} senderName=\"{SenderName}\" channel=\"{Channel}\" textLen={TextLength} preview=\"{Preview}\"")]
    public static partial void ReceivedFromLegacy(ILogger logger, string ChatType, string ModernType, uint Language, string SenderName, string Channel, int TextLength, string Preview);

    /// <summary>
    /// The 30-char preview the old traces built inline. Only ever called from inside an
    /// <c>IsEnabled(LogLevel.Trace)</c> guard, so the <c>Substring</c> is not paid in production.
    /// </summary>
    public static string Preview(string text)
        => text.Length > 30 ? text.Substring(0, 30) + "…" : text;
}
