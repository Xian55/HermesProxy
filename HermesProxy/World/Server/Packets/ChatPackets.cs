/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Framework.Constants;
using Framework.GameMath;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HermesProxy.World.Server.Packets;

public class JoinChannel : ClientPacket
{
    public JoinChannel(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        ChatChannelId = _worldPacket.ReadInt32();
        uint channelLength = _worldPacket.ReadBits<uint>(7);
        uint passwordLength = _worldPacket.ReadBits<uint>(7);
        _worldPacket.ResetBitPos();
        ChannelName = _worldPacket.ReadString(channelLength);
        Password = _worldPacket.ReadString(passwordLength);
    }

    public string Password = string.Empty;
    public string ChannelName = string.Empty;
    public int ChatChannelId;
}

public class ChannelNotifyJoined : ServerPacket, ISpanWritable
{
    public ChannelNotifyJoined() : base(Opcode.SMSG_CHANNEL_NOTIFY_JOINED) { }

    public override void Write()
    {
        _worldPacket.WriteBits(Channel.GetByteCount(), 7);
        _worldPacket.WriteBits(ChannelWelcomeMsg.GetByteCount(), 11);
        _worldPacket.WriteUInt32((uint)ChannelFlags);
        _worldPacket.WriteInt32(ChatChannelID);
        _worldPacket.WriteUInt64(InstanceID);
        _worldPacket.WritePackedGuid128(ChannelGUID);
        _worldPacket.WriteString(Channel);
        _worldPacket.WriteString(ChannelWelcomeMsg);
    }

    // Cap for channel name and welcome message
    // Reduced MaxWelcomeMsgBytes from 256 to 64 based on typical usage (~50 bytes)
    private const int MaxChannelBytes = 64;
    private const int MaxWelcomeMsgBytes = 64;
    // 18 bits(3) + uint(4) + int(4) + ulong(8) + GUID(18) + channel + msg
    public int MaxSize => 3 + 4 + 4 + 8 + PackedGuidHelper.MaxPackedGuid128Size + MaxChannelBytes + MaxWelcomeMsgBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        int channelBytes = Encoding.UTF8.GetByteCount(Channel);
        int welcomeBytes = Encoding.UTF8.GetByteCount(ChannelWelcomeMsg);
        if (channelBytes > MaxChannelBytes || welcomeBytes > MaxWelcomeMsgBytes)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteBits((uint)channelBytes, 7);
        writer.WriteBits((uint)welcomeBytes, 11);
        writer.WriteUInt32((uint)ChannelFlags);
        writer.WriteInt32(ChatChannelID);
        writer.WriteUInt64(InstanceID);
        writer.WritePackedGuid128(ChannelGUID.Low, ChannelGUID.High);
        writer.WriteString(Channel);
        writer.WriteString(ChannelWelcomeMsg);
        return writer.Position;
    }

    public string ChannelWelcomeMsg = "";
    public int ChatChannelID;
    public ulong InstanceID;
    public ChannelFlags ChannelFlags;
    public string Channel = "";
    public WowGuid128 ChannelGUID;
}

public class LeaveChannel : ClientPacket
{
    public LeaveChannel(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        ZoneChannelID = _worldPacket.ReadInt32();
        ChannelName = _worldPacket.ReadString(_worldPacket.ReadBits<uint>(7));
    }

    public int ZoneChannelID;
    public string ChannelName = string.Empty;
}

public class ChannelNotifyLeft : ServerPacket, ISpanWritable
{
    public ChannelNotifyLeft() : base(Opcode.SMSG_CHANNEL_NOTIFY_LEFT) { }

    public override void Write()
    {
        _worldPacket.WriteBits(Channel.GetByteCount(), 7);
        _worldPacket.WriteBit(Suspended);
        _worldPacket.WriteInt32(ChatChannelID);
        _worldPacket.WriteString(Channel);
    }

    // Cap for channel name - 7 bits = max 128, using 64
    private const int MaxChannelBytes = 64;
    // 8 bits(1) + int(4) + channel name
    public int MaxSize => 1 + 4 + MaxChannelBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        int channelBytes = Encoding.UTF8.GetByteCount(Channel);
        if (channelBytes > MaxChannelBytes)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteBits((uint)channelBytes, 7);
        writer.WriteBit(Suspended);
        writer.WriteInt32(ChatChannelID);
        writer.WriteString(Channel);
        return writer.Position;
    }

    public string Channel = string.Empty;
    public int ChatChannelID;
    public bool Suspended;
}

class ChannelCommand : ClientPacket
{
    public ChannelCommand(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        ChannelName = _worldPacket.ReadString(_worldPacket.ReadBits<uint>(7));
    }

    public string ChannelName = string.Empty;
}

public class ChannelListResponse : ServerPacket, ISpanWritable
{
    public ChannelListResponse() : base(Opcode.SMSG_CHANNEL_LIST)
    {
        Members = new List<ChannelPlayer>();
    }

    public override void Write()
    {
        _worldPacket.WriteBit(Display);
        _worldPacket.WriteBits(ChannelName.GetByteCount(), 7);
        _worldPacket.WriteUInt32((uint)ChannelFlags);
        _worldPacket.WriteInt32(Members.Count);
        _worldPacket.WriteString(ChannelName);

        foreach (ChannelPlayer player in Members)
        {
            _worldPacket.WritePackedGuid128(player.Guid);
            _worldPacket.WriteUInt32(player.VirtualRealmAddress);
            _worldPacket.WriteUInt8(player.Flags);
        }
    }

    // Cap for channel members - typical channel size
    private const int MaxMembers = 64;
    // Cap for channel name
    private const int MaxChannelBytes = 64;
    // Per member: GUID(18) + uint(4) + byte(1) = 23 bytes
    private const int MemberSize = PackedGuidHelper.MaxPackedGuid128Size + 4 + 1;
    // 8 bits(1) + uint(4) + int(4) + channel name + members
    public int MaxSize => 1 + 4 + 4 + MaxChannelBytes + MaxMembers * MemberSize;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (Members.Count > MaxMembers)
            return -1;

        int channelBytes = Encoding.UTF8.GetByteCount(ChannelName);
        if (channelBytes > MaxChannelBytes)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteBit(Display);
        writer.WriteBits((uint)channelBytes, 7);
        writer.WriteUInt32((uint)ChannelFlags);
        writer.WriteInt32(Members.Count);
        writer.WriteString(ChannelName);

        foreach (ChannelPlayer player in Members)
        {
            writer.WritePackedGuid128(player.Guid.Low, player.Guid.High);
            writer.WriteUInt32(player.VirtualRealmAddress);
            writer.WriteUInt8(player.Flags);
        }
        return writer.Position;
    }

    public List<ChannelPlayer> Members;
    public string ChannelName = string.Empty;
    public ChannelFlags ChannelFlags;
    public bool Display;

    public struct ChannelPlayer
    {
        public WowGuid128 Guid;
        public uint VirtualRealmAddress;
        public byte Flags;
    }
}

public class ChatMessageAFK : ClientPacket
{
    public ChatMessageAFK(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        // V3_4_3 widened the length to 11 bits (per WPP V3_4_0_45166
        // ChatHandler.cs:86-93). Pre-V3_4_3 modern clients used 9 bits.
        uint len = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261
            ? _worldPacket.ReadBits<uint>(11)
            : _worldPacket.ReadBits<uint>(9);
        Text = _worldPacket.ReadString(len);
    }

    public string Text = string.Empty;
}

public class ChatMessageDND : ClientPacket
{
    public ChatMessageDND(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        uint len = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261
            ? _worldPacket.ReadBits<uint>(11)
            : _worldPacket.ReadBits<uint>(9);
        Text = _worldPacket.ReadString(len);
    }

    public string Text = string.Empty;
}

public class ChatMessageChannel : ClientPacket
{
    public ChatMessageChannel(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Language = _worldPacket.ReadUInt32();
        ChannelGUID = _worldPacket.ReadPackedGuid128();
        uint targetLen = _worldPacket.ReadBits<uint>(9);
        uint textLen = _worldPacket.ReadBits<uint>(9);
        Target = _worldPacket.ReadString(targetLen);
        Text = _worldPacket.ReadString(textLen);
    }

    public uint Language;
    public WowGuid128 ChannelGUID;
    public string Text = string.Empty;
    public string Target = string.Empty;
}

public class ChatMessageWhisper : ClientPacket
{
    public ChatMessageWhisper(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Language = _worldPacket.ReadUInt32();

        // V3_4_3.54261 uses the V8.1+ whisper layout (per WPP
        // V8_0_1_27101/Parsers/ChannelHandler.cs:78-90). The V3_4_4 build
        // (59817) added TargetGUID + TargetVirtualRealmAddress and
        // narrowed/widened the lengths — those changes do NOT apply to
        // V3_4_3.54261 because WPP gates them at V3_4_4_59817. An earlier
        // attempt at the V3_4_4 layout caused the proxy to read TargetGUID
        // bytes that didn't exist on the wire, garbling target/text — log
        // showed `to="y" textLen=0` for a `/w Xii gooday`.
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            uint targetLen = _worldPacket.ReadBits<uint>(9);
            uint textLen = _worldPacket.ReadBits<uint>(10);
            Target = _worldPacket.ReadString(targetLen);
            Text = _worldPacket.ReadString(textLen);
        }
        else
        {
            uint targetLen = _worldPacket.ReadBits<uint>(9);
            uint textLen = _worldPacket.ReadBits<uint>(9);
            Target = _worldPacket.ReadString(targetLen);
            Text = _worldPacket.ReadString(textLen);
        }
    }

    public uint Language = 0;
    public string Text = string.Empty;
    public string Target = string.Empty;
}

public class ChatMessageEmote : ClientPacket
{
    public ChatMessageEmote(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        // V3_4_3 widened the length to 11 bits — same DND/EMOTE/AFK group
        // (WPP V3_4_0_45166 ChatHandler.cs:86-93). Reading 9 bits truncates
        // the text and leaves the bit cursor mid-byte, so the legacy server
        // sees a malformed CMSG_MESSAGECHAT and replies "unknown language".
        uint len = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261
            ? _worldPacket.ReadBits<uint>(11)
            : _worldPacket.ReadBits<uint>(9);
        Text = _worldPacket.ReadString(len);
    }

    public string Text = string.Empty;
}

public class ChatMessage : ClientPacket
{
    public ChatMessage(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Language = _worldPacket.ReadUInt32();

        // V3_4_3 widened the length to 11 bits and added a trailing IsSecure
        // bit AFTER the length (per WPP V3_4_0_45166 ChatHandler.cs:233-238).
        // The pre-V3_4_3 modern clients used a 9-bit length with no IsSecure.
        // Reading the wrong layout leaves the packet bytes short and the text
        // shifted, so HandleChatMessage sees garbage and chat / GM commands
        // silently fail.
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            uint len = _worldPacket.ReadBits<uint>(11);
            IsSecure = _worldPacket.HasBit();
            Text = _worldPacket.ReadString(len);
        }
        else
        {
            uint len = _worldPacket.ReadBits<uint>(9);
            Text = _worldPacket.ReadString(len);
        }
    }

    public string Text = string.Empty;
    public uint Language;
    public bool IsSecure;
}

public class ChatAddonMessage : ClientPacket
{
    public ChatAddonMessage(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Params.Read(_worldPacket);
    }

    public ChatAddonMessageParams Params = new();
}

class ChatAddonMessageTargeted : ClientPacket
{
    public ChatAddonMessageTargeted(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        uint targetLen = _worldPacket.ReadBits<uint>(9);
        Params.Read(_worldPacket);
        ChannelGuid = _worldPacket.ReadPackedGuid128();
        Target = _worldPacket.ReadString(targetLen);
    }

    public ChatAddonMessageParams Params = new();
    public WowGuid128 ChannelGuid;
    public string Target = string.Empty;
}

public class ChatAddonMessageParams
{
    public void Read(WorldPacket data)
    {
        data.ResetBitPos();
        uint prefixLen = data.ReadBits<uint>(5);
        uint textLen = data.ReadBits<uint>(8);
        IsLogged = data.HasBit();
        Type = (ChatMessageTypeModern)data.ReadInt32();
        Prefix = data.ReadString(prefixLen);
        Text = data.ReadString(textLen);
    }

    public string Prefix = string.Empty;
    public string Text = string.Empty;
    public ChatMessageTypeModern Type;
    public bool IsLogged;
}

public class ChatPkt : ServerPacket, ISpanWritable
{
    public ChatPkt(GlobalSessionData globalSession, ChatMessageTypeModern chatType, string message, uint language = 0, WowGuid128 sender = default, string senderName = "", WowGuid128 receiver = default, string receiverName = "", string channelName = "", ChatFlags chatFlags = ChatFlags.None, string addonPrefix = "", uint achievementId = 0) : base(Opcode.SMSG_CHAT)
    {
        SlashCmd = chatType;
        _Language = language;
        _ChatFlags = chatFlags;
        ChatText = message;
        Channel = channelName;
        AchievementID = achievementId;
        Prefix = addonPrefix;

        SenderGUID = sender;
        if (String.IsNullOrEmpty(senderName) && sender != default)
            SenderName = globalSession.GameState.GetPlayerName(sender);
        else
            SenderName = senderName;

        SenderAccountGUID = sender != default ? globalSession.GetGameAccountGuidForPlayer(sender) : default;
        SenderGuildGUID = WowGuid128.Empty;
        PartyGUID = WowGuid128.Empty;

        TargetGUID = receiver;
        if (String.IsNullOrEmpty(receiverName) && receiver != default)
            TargetName = globalSession.GameState.GetPlayerName(receiver);
        else
            TargetName = receiverName;

        if (!SenderGUID.IsEmpty())
            SenderVirtualAddress = globalSession.RealmId.GetAddress();
        if (!TargetGUID.IsEmpty())
            TargetVirtualAddress = globalSession.RealmId.GetAddress();
    }
    public static bool CheckAddonPrefix(HashSet<string> registeredPrefixes, ref uint language, ref string text, ref string addonPrefix)
    {
        if (language == (uint)Language.Addon)
        {
            language = (uint)Language.AddonBfA;
            char tab = '\t';
            if (text.Contains(tab))
            {
                string[] parts = text.Split(tab);
                addonPrefix = parts[0];
                text = string.Join(" ", parts.Skip(1).ToList());

                if (!registeredPrefixes.Contains(addonPrefix))
                    return false;
            }
            else
                return false;
        }
        return true;
    }
    public override void Write()
    {
        // V3_4_3.54261 layout per WPP V9_0_1 ChatHandler.cs:12-83 (gated by
        // WotLK branch >= V3_4_2_50129 and < V10_2_7_54577):
        //   - PartyGUID is dropped (V3_4_2+)
        //   - SpellID int32 added after DisplayTime (V3_4_2+)
        //   - ChatFlags is NOT byte-aligned uint16 (that's V10_2_7+); instead
        //     it lives as a 15-bit field inside the bit section, between
        //     textLen and HideChatLog.
        // Total bits: 11+11+5+7+12 + 15 + 4 = 65 bits = 9 bytes.
        // Earlier "fix" wrote uint16 ChatFlags byte-aligned + 50 bits — those
        // 2 extra bytes shifted every following field, so WPP saw Prefix="Xii"
        // and Text="" and the V3_4_3 client silently dropped the message.
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            _worldPacket.WriteUInt8((byte)SlashCmd);
            _worldPacket.WriteUInt32(_Language);
            _worldPacket.WritePackedGuid128(SenderGUID);
            _worldPacket.WritePackedGuid128(SenderGuildGUID);
            _worldPacket.WritePackedGuid128(SenderAccountGUID);
            _worldPacket.WritePackedGuid128(TargetGUID);
            _worldPacket.WriteUInt32(TargetVirtualAddress);
            _worldPacket.WriteUInt32(SenderVirtualAddress);
            _worldPacket.WriteInt32((int)AchievementID);
            _worldPacket.WriteFloat(DisplayTime);
            _worldPacket.WriteInt32(SpellID);

            _worldPacket.WriteBits(SenderName.GetByteCount(), 11);
            _worldPacket.WriteBits(TargetName.GetByteCount(), 11);
            _worldPacket.WriteBits(Prefix.GetByteCount(), 5);
            _worldPacket.WriteBits(Channel.GetByteCount(), 7);
            _worldPacket.WriteBits(ChatText.GetByteCount(), 12);
            _worldPacket.WriteBits((uint)_ChatFlags, 15);
            _worldPacket.WriteBit(HideChatLog);
            _worldPacket.WriteBit(FakeSenderName);
            _worldPacket.WriteBit(Unused_801.HasValue);
            _worldPacket.WriteBit(ChannelGUID != default);
            _worldPacket.FlushBits();

            _worldPacket.WriteString(SenderName);
            _worldPacket.WriteString(TargetName);
            _worldPacket.WriteString(Prefix);
            _worldPacket.WriteString(Channel);
            _worldPacket.WriteString(ChatText);

            if (Unused_801.HasValue)
                _worldPacket.WriteUInt32(Unused_801.Value);
            if (ChannelGUID != default)
                _worldPacket.WritePackedGuid128(ChannelGUID);
            return;
        }

        _worldPacket.WriteUInt8((byte)SlashCmd);
        _worldPacket.WriteUInt32((uint)_Language);
        _worldPacket.WritePackedGuid128(SenderGUID);
        _worldPacket.WritePackedGuid128(SenderGuildGUID);
        _worldPacket.WritePackedGuid128(SenderAccountGUID);
        _worldPacket.WritePackedGuid128(TargetGUID);
        _worldPacket.WriteUInt32(TargetVirtualAddress);
        _worldPacket.WriteUInt32(SenderVirtualAddress);
        _worldPacket.WritePackedGuid128(PartyGUID);
        _worldPacket.WriteUInt32(AchievementID);
        _worldPacket.WriteFloat(DisplayTime);
        _worldPacket.WriteBits(SenderName.GetByteCount(), 11);
        _worldPacket.WriteBits(TargetName.GetByteCount(), 11);
        _worldPacket.WriteBits(Prefix.GetByteCount(), 5);
        _worldPacket.WriteBits(Channel.GetByteCount(), 7);
        _worldPacket.WriteBits(ChatText.GetByteCount(), 12);
        _worldPacket.WriteBits((byte)_ChatFlags, 14);
        _worldPacket.WriteBit(HideChatLog);
        _worldPacket.WriteBit(FakeSenderName);
        _worldPacket.WriteBit(Unused_801.HasValue);
        _worldPacket.WriteBit(ChannelGUID != default);
        _worldPacket.FlushBits();

        _worldPacket.WriteString(SenderName);
        _worldPacket.WriteString(TargetName);
        _worldPacket.WriteString(Prefix);
        _worldPacket.WriteString(Channel);
        _worldPacket.WriteString(ChatText);

        if (Unused_801.HasValue)
            _worldPacket.WriteUInt32(Unused_801.Value);

        if (ChannelGUID != default)
            _worldPacket.WritePackedGuid128(ChannelGUID);
    }

    // MaxSize: byte(1) + uint(4) + 6 GUIDs(108) + 2 uints(8) + uint(4) + float(4)
    // + bits(8) + strings: sender(128) + target(128) + prefix(32) + channel(128) + chat(512)
    // + opt uint(4) + opt GUID(18) = ~430 bytes
    // Note: Real player names are max 24 bytes, typical chat is <200 bytes.
    // If exceeded, WriteToSpan returns -1 to trigger fallback to Write().
    private const int MaxSenderNameBytes = 128;
    private const int MaxChatTextBytes = 512;
    public int MaxSize => 1 + 4 + PackedGuidHelper.MaxPackedGuid128Size * 6 + 16 + 8 +
        MaxSenderNameBytes * 2 + 32 + 128 + MaxChatTextBytes + 22;

    public int WriteToSpan(Span<byte> buffer)
    {
        int senderNameBytes = Encoding.UTF8.GetByteCount(SenderName ?? "");
        int targetNameBytes = Encoding.UTF8.GetByteCount(TargetName ?? "");
        int prefixBytes = Encoding.UTF8.GetByteCount(Prefix ?? "");
        int channelBytes = Encoding.UTF8.GetByteCount(Channel ?? "");
        int chatTextBytes = Encoding.UTF8.GetByteCount(ChatText ?? "");

        // Check against our reduced MaxSize limits - fallback to Write() for oversized messages
        // Protocol limits are larger (2047/4095) but we optimize for typical usage
        if (senderNameBytes > MaxSenderNameBytes || targetNameBytes > MaxSenderNameBytes ||
            prefixBytes > 31 || channelBytes > 127 || chatTextBytes > MaxChatTextBytes)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt8((byte)SlashCmd);
        writer.WriteUInt32(_Language);
        writer.WritePackedGuid128(SenderGUID.Low, SenderGUID.High);
        writer.WritePackedGuid128(SenderGuildGUID.Low, SenderGuildGUID.High);
        writer.WritePackedGuid128(SenderAccountGUID.Low, SenderAccountGUID.High);
        writer.WritePackedGuid128(TargetGUID.Low, TargetGUID.High);
        writer.WriteUInt32(TargetVirtualAddress);
        writer.WriteUInt32(SenderVirtualAddress);

        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            writer.WriteInt32((int)AchievementID);
            writer.WriteFloat(DisplayTime);
            writer.WriteInt32(SpellID);

            writer.WriteBits((uint)senderNameBytes, 11);
            writer.WriteBits((uint)targetNameBytes, 11);
            writer.WriteBits((uint)prefixBytes, 5);
            writer.WriteBits((uint)channelBytes, 7);
            writer.WriteBits((uint)chatTextBytes, 12);
            writer.WriteBits((uint)_ChatFlags, 15);
            writer.WriteBit(HideChatLog);
            writer.WriteBit(FakeSenderName);
            writer.WriteBit(Unused_801.HasValue);
            writer.WriteBit(ChannelGUID != default);
            writer.FlushBits();

            writer.WriteString(SenderName ?? "");
            writer.WriteString(TargetName ?? "");
            writer.WriteString(Prefix ?? "");
            writer.WriteString(Channel ?? "");
            writer.WriteString(ChatText ?? "");

            if (Unused_801.HasValue)
                writer.WriteUInt32(Unused_801.Value);
            if (ChannelGUID != default)
                writer.WritePackedGuid128(ChannelGUID.Low, ChannelGUID.High);

            return writer.Position;
        }

        writer.WritePackedGuid128(PartyGUID.Low, PartyGUID.High);
        writer.WriteUInt32(AchievementID);
        writer.WriteFloat(DisplayTime);
        writer.WriteBits((uint)senderNameBytes, 11);
        writer.WriteBits((uint)targetNameBytes, 11);
        writer.WriteBits((uint)prefixBytes, 5);
        writer.WriteBits((uint)channelBytes, 7);
        writer.WriteBits((uint)chatTextBytes, 12);
        writer.WriteBits((uint)_ChatFlags, 14);
        writer.WriteBit(HideChatLog);
        writer.WriteBit(FakeSenderName);
        writer.WriteBit(Unused_801.HasValue);
        writer.WriteBit(ChannelGUID != default);
        writer.FlushBits();

        writer.WriteString(SenderName ?? "");
        writer.WriteString(TargetName ?? "");
        writer.WriteString(Prefix ?? "");
        writer.WriteString(Channel ?? "");
        writer.WriteString(ChatText ?? "");

        if (Unused_801.HasValue)
            writer.WriteUInt32(Unused_801.Value);

        if (ChannelGUID != default)
            writer.WritePackedGuid128(ChannelGUID.Low, ChannelGUID.High);

        return writer.Position;
    }

    public ChatMessageTypeModern SlashCmd = 0;
    public uint _Language = 0;
    public WowGuid128 SenderGUID;
    public WowGuid128 SenderGuildGUID;
    public WowGuid128 SenderAccountGUID;
    public WowGuid128 TargetGUID;
    public WowGuid128 PartyGUID;
    public WowGuid128 ChannelGUID;
    public uint SenderVirtualAddress;
    public uint TargetVirtualAddress;
    public string SenderName = "";
    public string TargetName = "";
    public string Prefix = "";
    public string Channel = "";
    public string ChatText = "";
    public uint AchievementID;
    public ChatFlags _ChatFlags = 0;
    public float DisplayTime = 0.0f;
    public int SpellID = 0;
    public uint? Unused_801;
    public bool HideChatLog = false;
    public bool FakeSenderName = false;
}

public class EmoteMessage : ServerPacket, ISpanWritable
{
    public EmoteMessage() : base(Opcode.SMSG_EMOTE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Guid);
        _worldPacket.WriteUInt32(EmoteID);
        if (ModernVersion.AddedInVersion(9, 0, 5, 1, 14, 0, 2, 5, 1))
        {
            _worldPacket.WriteInt32(SpellVisualKitIDs.Count);
            if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
                _worldPacket.WriteInt32(SequenceVariation);
            foreach (var id in SpellVisualKitIDs)
                _worldPacket.WriteUInt32(id);
        }
    }

    // Cap for spell visual kit IDs - rarely more than 1-2
    private const int MaxSpellVisualKitIDs = 4;
    // GUID(18) + EmoteID(4) + count(4) + SequenceVariation(4) + IDs(4 each)
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 4 + 4 + 4 + MaxSpellVisualKitIDs * 4;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (SpellVisualKitIDs.Count > MaxSpellVisualKitIDs)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Guid.Low, Guid.High);
        writer.WriteUInt32(EmoteID);
        if (ModernVersion.AddedInVersion(9, 0, 5, 1, 14, 0, 2, 5, 1))
        {
            writer.WriteInt32(SpellVisualKitIDs.Count);
            if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
                writer.WriteInt32(SequenceVariation);
            foreach (var id in SpellVisualKitIDs)
                writer.WriteUInt32(id);
        }
        return writer.Position;
    }

    public WowGuid128 Guid;
    public uint EmoteID;
    public int SequenceVariation;
    public List<uint> SpellVisualKitIDs = new();
}

public class CTextEmote : ClientPacket
{
    public CTextEmote(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Target = _worldPacket.ReadPackedGuid128();
        EmoteID = _worldPacket.ReadInt32();
        SoundIndex = _worldPacket.ReadInt32();

        if (ModernVersion.AddedInVersion(9, 0, 5, 1, 14, 0, 2, 5, 1))
        {
            SpellVisualKitIDs = new uint[_worldPacket.ReadUInt32()];
            if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
                SequenceVariation = _worldPacket.ReadInt32();
            for (var i = 0; i < SpellVisualKitIDs.Length; ++i)
                SpellVisualKitIDs[i] = _worldPacket.ReadUInt32();
        }
    }

    public WowGuid128 Target;
    public int EmoteID;
    public int SoundIndex;
    public int SequenceVariation;
    public uint[] SpellVisualKitIDs = Array.Empty<uint>();
}

public class STextEmote : ServerPacket, ISpanWritable
{
    public STextEmote() : base(Opcode.SMSG_TEXT_EMOTE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(SourceGUID);
        _worldPacket.WritePackedGuid128(SourceAccountGUID);
        _worldPacket.WriteInt32(EmoteID);
        _worldPacket.WriteInt32(SoundIndex);
        _worldPacket.WritePackedGuid128(TargetGUID);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size * 3 + 8; // 3 GUIDs + 2 ints

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(SourceGUID.Low, SourceGUID.High);
        writer.WritePackedGuid128(SourceAccountGUID.Low, SourceAccountGUID.High);
        writer.WriteInt32(EmoteID);
        writer.WriteInt32(SoundIndex);
        writer.WritePackedGuid128(TargetGUID.Low, TargetGUID.High);
        return writer.Position;
    }

    public WowGuid128 SourceGUID;
    public WowGuid128 SourceAccountGUID;
    public WowGuid128 TargetGUID;
    public int SoundIndex = -1;
    public int EmoteID;
}

public class PrintNotification : ServerPacket, ISpanWritable
{
    public PrintNotification() : base(Opcode.SMSG_PRINT_NOTIFICATION) { }

    public override void Write()
    {
        _worldPacket.WriteBits(NotifyText.GetByteCount(), 12);
        _worldPacket.WriteString(NotifyText);
    }

    // Cap for notification text - most are short system messages
    private const int MaxTextBytes = 256;
    // 12 bits(2) + text
    public int MaxSize => 2 + MaxTextBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        int textBytes = Encoding.UTF8.GetByteCount(NotifyText);
        if (textBytes > MaxTextBytes)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteBits((uint)textBytes, 12);
        writer.WriteString(NotifyText);
        return writer.Position;
    }

    public string NotifyText = string.Empty;
}

class ChatPlayerNotfound : ServerPacket, ISpanWritable
{
    public ChatPlayerNotfound() : base(Opcode.SMSG_CHAT_PLAYER_NOTFOUND) { }

    public override void Write()
    {
        _worldPacket.WriteBits(Name.GetByteCount(), 9);
        _worldPacket.WriteString(Name);
    }

    // MaxSize: 9 bits (2 bytes) + max player name bytes
    public int MaxSize => 2 + GameLimits.MaxPlayerNameBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(Name), 9);
        writer.WriteString(Name);
        return writer.Position;
    }

    public string Name = string.Empty;
}

class DefenseMessage : ServerPacket, ISpanWritable
{
    public DefenseMessage() : base(Opcode.SMSG_DEFENSE_MESSAGE) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(ZoneID);
        _worldPacket.WriteBits(MessageText.GetByteCount(), 12);
        _worldPacket.FlushBits();
        _worldPacket.WriteString(MessageText);
    }

    // Cap for defense message - zone attack notifications
    private const int MaxTextBytes = 256;
    // uint(4) + 12 bits(2) + text
    public int MaxSize => 4 + 2 + MaxTextBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        int textBytes = Encoding.UTF8.GetByteCount(MessageText);
        if (textBytes > MaxTextBytes)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(ZoneID);
        writer.WriteBits((uint)textBytes, 12);
        writer.FlushBits();
        writer.WriteString(MessageText);
        return writer.Position;
    }

    public uint ZoneID;
    public string MessageText = "";
}

class ChatServerMessage : ServerPacket, ISpanWritable
{
    public ChatServerMessage() : base(Opcode.SMSG_CHAT_SERVER_MESSAGE) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(MessageID);

        _worldPacket.WriteBits(StringParam.GetByteCount(), 11);
        _worldPacket.WriteString(StringParam);
    }

    // Cap for server message param - usually short strings
    private const int MaxParamBytes = 256;
    // int(4) + 11 bits(2) + string
    public int MaxSize => 4 + 2 + MaxParamBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        int paramBytes = Encoding.UTF8.GetByteCount(StringParam);
        if (paramBytes > MaxParamBytes)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt32(MessageID);
        writer.WriteBits((uint)paramBytes, 11);
        writer.WriteString(StringParam);
        return writer.Position;
    }

    public int MessageID;
    public string StringParam = "";
}

class ChatRegisterAddonPrefixes : ClientPacket
{
    public ChatRegisterAddonPrefixes(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        int count = _worldPacket.ReadInt32();

        for (int i = 0; i < count && i < 64; ++i)
            Prefixes.Add(_worldPacket.ReadString(_worldPacket.ReadBits<uint>(5)));
    }

    public List<string> Prefixes = new List<string>();
}
