using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's chat CMSGs. Behaviour only.
/// </summary>
/// <remarks>
/// These are the player's own outgoing messages — a handful per session. The high-volume chat
/// traffic runs the other way (every message from every player in every joined channel) and is
/// handled on the legacy side in <c>World/Client/PacketHandlers/ChatHandler.cs</c>; that is where
/// chat allocation would actually matter on a crowded realm, and it is untouched here.
/// </remarks>
public static class ChatSystem
{
    [HandlesCmsg(Opcode.CMSG_CHAT_JOIN_CHANNEL)]
    public static void HandleChatJoinChannel(in JoinChannel join, in SessionContext ctx)
    {
        if (ctx.GetSession().WorldClient != null)
            ctx.GetSession().WorldClient!.SendChatJoinChannel(join.ChatChannelId, join.ChannelName, join.Password);
    }

    [HandlesCmsg(Opcode.CMSG_CHAT_LEAVE_CHANNEL)]
    public static void HandleChatLeaveChannel(in LeaveChannel leave, in SessionContext ctx)
    {
        if (ctx.GetSession().WorldClient != null)
        {
            ctx.GetSession().GameState.LeftChannelName = leave.ChannelName;
            ctx.GetSession().WorldClient!.SendChatLeaveChannel(leave.ZoneChannelID, leave.ChannelName);
        }
    }

    /// <summary>
    /// Two opcodes forwarded verbatim under their own id. Shape B, so the opcode arrives as a
    /// constant instead of being read back off the packet.
    /// </summary>
    [HandlesCmsg(Opcode.CMSG_CHAT_CHANNEL_OWNER)]
    [HandlesCmsg(Opcode.CMSG_CHAT_CHANNEL_ANNOUNCEMENTS)]
    public static void HandleChatChannelCommand(Opcode opcode, in ChannelCommand command, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(opcode);
        packet.WriteCString(command.ChannelName);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_CHAT_CHANNEL_LIST)]
    public static void HandleChatChannelList(in ChannelCommand command, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_CHAT_CHANNEL_LIST);
        packet.WriteCString(command.ChannelName);
        ctx.SendPacketToServer(packet);
        ctx.GetSession().GameState.ChannelDisplayList = false;
    }

    [HandlesCmsg(Opcode.CMSG_CHAT_CHANNEL_DISPLAY_LIST)]
    public static void HandleChatChannelDisplayList(in ChannelCommand command, in SessionContext ctx)
    {
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_CHAT_CHANNEL_LIST);
            packet.WriteCString(command.ChannelName);
            ctx.SendPacketToServer(packet);
        }
        else
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_CHAT_CHANNEL_DISPLAY_LIST);
            packet.WriteCString(command.ChannelName);
            ctx.SendPacketToServer(packet);
        }
        ctx.GetSession().GameState.ChannelDisplayList = true;
    }

    [HandlesCmsg(Opcode.CMSG_CHAT_CHANNEL_DECLINE_INVITE)]
    public static void HandleChatChannelDeclineInvite(in ChannelCommand command, in SessionContext ctx)
    {
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            return;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_CHAT_CHANNEL_DECLINE_INVITE);
        packet.WriteCString(command.ChannelName);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_CHAT_MESSAGE_AFK)]
    public static void HandleChatMessageAFK(in ChatMessageAFK afk, in SessionContext ctx)
    {
        var toBeSentTextParts = ConvertTextMessageIntoMaxLengthParts(afk.Text);
        if (toBeSentTextParts.Count < 1)
            return;

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            ctx.GetSession().WorldClient!.SendMessageChatWotLK(ChatMessageTypeWotLK.Afk, 0, toBeSentTextParts[0], "", "");
        else
            ctx.GetSession().WorldClient!.SendMessageChatVanilla(ChatMessageTypeVanilla.Afk, 0, toBeSentTextParts[0], "", "");
    }

    [HandlesCmsg(Opcode.CMSG_CHAT_MESSAGE_DND)]
    public static void HandleChatMessageDND(in ChatMessageDND dnd, in SessionContext ctx)
    {
        var toBeSentTextParts = ConvertTextMessageIntoMaxLengthParts(dnd.Text);
        if (toBeSentTextParts.Count < 1)
            return;

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            ctx.GetSession().WorldClient!.SendMessageChatWotLK(ChatMessageTypeWotLK.Dnd, 0, toBeSentTextParts[0], "", "");
        else
            ctx.GetSession().WorldClient!.SendMessageChatVanilla(ChatMessageTypeVanilla.Dnd, 0, toBeSentTextParts[0], "", "");
    }

    [HandlesCmsg(Opcode.CMSG_CHAT_MESSAGE_CHANNEL)]
    public static void HandleChatMessageChannel(in ChatMessageChannel channel, in SessionContext ctx)
    {
        var toBeSentTextParts = ConvertTextMessageIntoMaxLengthParts(channel.Text);
        foreach (string text in toBeSentTextParts)
        {
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                ctx.GetSession().WorldClient!.SendMessageChatWotLK(ChatMessageTypeWotLK.Channel, channel.Language, text, channel.Target, "");
            else
                ctx.GetSession().WorldClient!.SendMessageChatVanilla(ChatMessageTypeVanilla.Channel, channel.Language, text, channel.Target, "");
        }
    }

    [HandlesCmsg(Opcode.CMSG_CHAT_MESSAGE_WHISPER)]
    public static void HandleChatMessageWhisper(in ChatMessageWhisper whisper, in SessionContext ctx)
    {
        var toBeSentTextParts = ConvertTextMessageIntoMaxLengthParts(whisper.Text);
        foreach (string text in toBeSentTextParts)
        {
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                ctx.GetSession().WorldClient!.SendMessageChatWotLK(ChatMessageTypeWotLK.Whisper, whisper.Language, text, "", whisper.Target);
            else
                ctx.GetSession().WorldClient!.SendMessageChatVanilla(ChatMessageTypeVanilla.Whisper, whisper.Language, text, "", whisper.Target);
        }
    }

    [HandlesCmsg(Opcode.CMSG_CHAT_MESSAGE_EMOTE)]
    public static void HandleChatMessageEmote(in ChatMessageEmote emote, in SessionContext ctx)
    {
        var rawPreview = emote.Text.Length > 30 ? emote.Text.Substring(0, 30) + "…" : emote.Text;
        Log.Print(LogType.Trace,
            $"[ChatTrace] CMSG_CHAT_MESSAGE_EMOTE received: textLen={emote.Text.Length} preview=\"{rawPreview}\"");

        var toBeSentTextParts = ConvertTextMessageIntoMaxLengthParts(emote.Text);
        if (toBeSentTextParts.Count < 1)
            return;

        // Modern client doesn't carry a Language field for emote — but legacy
        // CMSG_MESSAGECHAT requires one. Forwarding lang=0 (Universal) makes
        // cMaNGOS reject the packet with a "unknown language" notification,
        // because Universal isn't allowed for player chat types (incl. EMOTE).
        // TC repack accepts it, which is why the fork "works" on TC but not
        // cMaNGOS. Common (7) is the language /say uses by default and is
        // accepted everywhere.
        const uint defaultLang = (uint)Language.Common;
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            ctx.GetSession().WorldClient!.SendMessageChatWotLK(ChatMessageTypeWotLK.Emote, defaultLang, toBeSentTextParts[0], "", "");
        else
            ctx.GetSession().WorldClient!.SendMessageChatVanilla(ChatMessageTypeVanilla.Emote, defaultLang, toBeSentTextParts[0], "", "");
    }

    /// <summary>
    /// The main say/yell/guild/party/raid path — eight opcodes, one body, distinguished only by
    /// the opcode. Shape B, so the type switch runs on a constant rather than on a value read
    /// back off the packet.
    /// </summary>
    [HandlesCmsg(Opcode.CMSG_CHAT_MESSAGE_GUILD)]
    [HandlesCmsg(Opcode.CMSG_CHAT_MESSAGE_OFFICER)]
    [HandlesCmsg(Opcode.CMSG_CHAT_MESSAGE_PARTY)]
    [HandlesCmsg(Opcode.CMSG_CHAT_MESSAGE_RAID)]
    [HandlesCmsg(Opcode.CMSG_CHAT_MESSAGE_RAID_WARNING)]
    [HandlesCmsg(Opcode.CMSG_CHAT_MESSAGE_SAY)]
    [HandlesCmsg(Opcode.CMSG_CHAT_MESSAGE_YELL)]
    [HandlesCmsg(Opcode.CMSG_CHAT_MESSAGE_INSTANCE_CHAT)]
    public static void HandleChatMessage(Opcode opcode, in ChatMessage packet, in SessionContext ctx)
    {
        var preview = packet.Text.Length > 30 ? packet.Text.Substring(0, 30) + "…" : packet.Text;
        Log.Print(LogType.Trace,
            $"[ChatTrace] CMSG_CHAT_MESSAGE_* received: opcode={opcode} " +
            $"lang={packet.Language} textLen={packet.Text.Length} preview=\"{preview}\"");

        ChatMessageTypeModern type;

        switch (opcode)
        {
            case Opcode.CMSG_CHAT_MESSAGE_SAY:
                type = ChatMessageTypeModern.Say;
                break;
            case Opcode.CMSG_CHAT_MESSAGE_YELL:
                type = ChatMessageTypeModern.Yell;
                break;
            case Opcode.CMSG_CHAT_MESSAGE_GUILD:
                type = ChatMessageTypeModern.Guild;
                break;
            case Opcode.CMSG_CHAT_MESSAGE_OFFICER:
                type = ChatMessageTypeModern.Officer;
                break;
            case Opcode.CMSG_CHAT_MESSAGE_PARTY:
                type = ChatMessageTypeModern.Party;
                break;
            case Opcode.CMSG_CHAT_MESSAGE_RAID:
                type = ChatMessageTypeModern.Raid;
                break;
            case Opcode.CMSG_CHAT_MESSAGE_RAID_WARNING:
                type = ChatMessageTypeModern.RaidWarning;
                break;
            case Opcode.CMSG_CHAT_MESSAGE_INSTANCE_CHAT:
                if (ctx.GetSession().GameState.IsInBattleground())
                    type = ChatMessageTypeModern.Battleground;
                else
                    type = ChatMessageTypeModern.Party;
                break;
            default:
                Log.Print(LogType.Error, $"HandleMessagechatOpcode : Unknown chat opcode ({opcode})");
                return;
        }

        var toBeSentTextParts = ConvertTextMessageIntoMaxLengthParts(packet.Text);
        foreach (string text in toBeSentTextParts)
        {
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                ChatMessageTypeWotLK chatMsg = type.CastEnum<ChatMessageTypeWotLK>();
                ctx.GetSession().WorldClient!.SendMessageChatWotLK(chatMsg, packet.Language, text, "", "");
            }
            else
            {
                ChatMessageTypeVanilla chatMsg = type.CastEnum<ChatMessageTypeVanilla>();
                ctx.GetSession().WorldClient!.SendMessageChatVanilla(chatMsg, packet.Language, text, "", "");
            }
        }
    }

    [HandlesCmsg(Opcode.CMSG_CHAT_ADDON_MESSAGE)]
    public static void HandleAddonMessage(in ChatAddonMessage packet, in SessionContext ctx)
    {
        uint language = (uint)Language.Addon;
        string text = packet.Params.Prefix + '\t' + packet.Params.Text;

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            ChatMessageTypeWotLK chatMsg = packet.Params.Type.CastEnum<ChatMessageTypeWotLK>();
            ctx.GetSession().WorldClient!.SendMessageChatWotLK(chatMsg, language, text, "", "");
        }
        else
        {
            ChatMessageTypeVanilla chatMsg = packet.Params.Type.CastEnum<ChatMessageTypeVanilla>();
            ctx.GetSession().WorldClient!.SendMessageChatVanilla(chatMsg, language, text, "", "");
        }
    }

    [HandlesCmsg(Opcode.CMSG_CHAT_ADDON_MESSAGE_TARGETED)]
    public static void HandleAddonMessageTargeted(in ChatAddonMessageTargeted packet, in SessionContext ctx)
    {
        uint language = (uint)Language.Addon;
        string text = packet.Params.Prefix + '\t' + packet.Params.Text;
        string channelName = packet.ChannelGuid.IsEmpty() ? "" :
            ctx.GetSession().GameState.GetChannelName((int)packet.ChannelGuid.GetCounter());

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            ChatMessageTypeWotLK chatMsg = packet.Params.Type.CastEnum<ChatMessageTypeWotLK>();
            ctx.GetSession().WorldClient!.SendMessageChatWotLK(chatMsg, language, text, channelName, packet.Target);
        }
        else
        {
            ChatMessageTypeVanilla chatMsg = packet.Params.Type.CastEnum<ChatMessageTypeVanilla>();
            ctx.GetSession().WorldClient!.SendMessageChatVanilla(chatMsg, language, text, channelName, packet.Target);
        }
    }

    [HandlesCmsg(Opcode.CMSG_SEND_TEXT_EMOTE)]
    public static void HandleSendTextEmote(in CTextEmote emote, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SEND_TEXT_EMOTE);
        packet.WriteInt32(emote.EmoteID);
        packet.WriteInt32(emote.SoundIndex);
        packet.WriteGuid(emote.Target.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_CHAT_REGISTER_ADDON_PREFIXES)]
    public static void HandleChatRegisterAddonPrefixes(in ChatRegisterAddonPrefixes addons, in SessionContext ctx)
    {
        foreach (var prefix in addons.Prefixes)
            ctx.GetSession().GameState.AddonPrefixes.Add(prefix);
    }

    /// <summary>
    /// Splits a message into parts the legacy server will accept, without breaking item links.
    /// </summary>
    /// <remarks>
    /// Moved here verbatim with the handlers it serves. The <c>255</c> it used to hardcode now
    /// comes from <see cref="GameLimits.MaxChatMessageChars"/>.
    /// <para>
    /// This is the allocating part of the chat path — a <c>List&lt;string&gt;</c> plus a string per
    /// part — and it is why a bounded or span-based packet field would not help on its own:
    /// <c>SendMessageChat*</c> takes <c>string</c>, so the text has to materialise here regardless
    /// of how the codec read it. Making chat allocation-free means changing those signatures,
    /// which is outbound work.
    /// </para>
    /// </remarks>
    private static List<string> ConvertTextMessageIntoMaxLengthParts(string originalTextMessage)
    {
        List<string> toBeSendTextParts = new List<string>();
        const int maxAllowedTextLength = GameLimits.MaxChatMessageChars;
        if (originalTextMessage.Length <= maxAllowedTextLength)
        {
            // We fit in a single packet
            toBeSendTextParts.Add(originalTextMessage);
        }
        else
        {
            // We must split the text into chunks of max length 255
            // Since we dont want to break item links, we first split the text by links
            var linkBegin = @"(?=\|c[a-f0-9]{8}\|H)";
            var linkEnd = @"(?<=\|h\|r)";
            var splitted = Regex.Split(originalTextMessage, $"{linkBegin}|{linkEnd}");
            var splittedAndSlicedToMaxLength = splitted.SelectMany(x => x.Chunk(maxAllowedTextLength));

            var strBuilder = new StringBuilder();
            foreach (var part in splittedAndSlicedToMaxLength)
            {
                if ((strBuilder.Length + part.Length) > maxAllowedTextLength)
                { // Flush now
                    toBeSendTextParts.Add(strBuilder.ToString());
                    strBuilder.Clear();
                }
                strBuilder.Append(part);
            }

            // Flush last part of the message
            toBeSendTextParts.Add(strBuilder.ToString());
        }

        return toBeSendTextParts;
    }

    [HandlesCmsg(Opcode.CMSG_CHAT_UNREGISTER_ALL_ADDON_PREFIXES)]
    public static void HandleChatUnregisterAllAddonPrefixes(in EmptyClientPacket addons, in SessionContext ctx)
    {
        ctx.GetSession().GameState.AddonPrefixes.Clear();
    }
}
