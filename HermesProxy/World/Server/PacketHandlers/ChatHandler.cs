using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HermesProxy.World.Server;

public partial class WorldSocket
{

    [PacketHandler(Opcode.CMSG_CHAT_ADDON_MESSAGE)]
    void HandleAddonMessage(ChatAddonMessage packet)
    {
        uint language = (uint)Language.Addon;
        string text = packet.Params.Prefix + '\t' + packet.Params.Text;

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            ChatMessageTypeWotLK chatMsg = packet.Params.Type.CastEnum<ChatMessageTypeWotLK>();
            GetSession().WorldClient!.SendMessageChatWotLK(chatMsg, language, text, "", "");
        }
        else
        {
            ChatMessageTypeVanilla chatMsg = packet.Params.Type.CastEnum<ChatMessageTypeVanilla>();
            GetSession().WorldClient!.SendMessageChatVanilla(chatMsg, language, text, "", "");
        }
    }

    [PacketHandler(Opcode.CMSG_CHAT_ADDON_MESSAGE_TARGETED)]
    void HandleAddonMessageTargeted(ChatAddonMessageTargeted packet)
    {
        uint language = (uint)Language.Addon;
        string text = packet.Params.Prefix + '\t' + packet.Params.Text;
        string channelName = packet.ChannelGuid.IsEmpty() ? "" :
            GetSession().GameState.GetChannelName((int)packet.ChannelGuid.GetCounter());

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            ChatMessageTypeWotLK chatMsg = packet.Params.Type.CastEnum<ChatMessageTypeWotLK>();
            GetSession().WorldClient!.SendMessageChatWotLK(chatMsg, language, text, channelName, packet.Target);
        }
        else
        {
            ChatMessageTypeVanilla chatMsg = packet.Params.Type.CastEnum<ChatMessageTypeVanilla>();
            GetSession().WorldClient!.SendMessageChatVanilla(chatMsg, language, text, channelName, packet.Target);
        }
    }

    [PacketHandler(Opcode.CMSG_SEND_TEXT_EMOTE)]
    void HandleSendTextEmote(CTextEmote emote)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SEND_TEXT_EMOTE);
        packet.WriteInt32(emote.EmoteID);
        packet.WriteInt32(emote.SoundIndex);
        packet.WriteGuid(emote.Target.To64());
        SendPacketToServer(packet);
    }

    [PacketHandler(Opcode.CMSG_CHAT_REGISTER_ADDON_PREFIXES)]
    void HandleChatRegisterAddonPrefixes(ChatRegisterAddonPrefixes addons)
    {
        foreach (var prefix in addons.Prefixes)
            GetSession().GameState.AddonPrefixes.Add(prefix);
    }



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
}
