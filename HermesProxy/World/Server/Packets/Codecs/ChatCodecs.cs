using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;

namespace HermesProxy.World.Server.Packets;

// Chat CMSG codecs.
//
// These are the first packets whose *shape* differs by client build, so they are the first to use
// ranged codecs rather than an `if (ModernVersion.Build == …)` inside one reader. Two consequences
// worth stating, because they are the whole argument of docs/version-shape-dispatch.md:
//
//   1. The branch is gone from the per-packet path. Which codec runs is decided once, when the
//      dispatch table is built.
//   2. The old form was exact equality against V3_4_3_54261, so *any* future build fell into the
//      else — the V1_14/V2_5 layout. A ranged codec says `AddedIn = V3_4_3_54261`, which a
//      Cataclysm Classic client satisfies. That is the difference between adding a client and
//      rewriting every reader (issue #202).
//
// V3_4_3 widened the text length from 9 bits to 11 (WPP V3_4_0_45166 ChatHandler.cs:86-93) and
// added a secure-flag bit on some packets. Bit reads are MSB-first, so reading 9 bits of an
// 11-bit length yields len >> 2 — "gooday" (6) arrived as 1 and the client posted "g", and
// 1-3 character messages read as length 0 and posted nothing. That was issue #177; these codecs
// are the shape that makes it unrepresentable rather than fixed-in-place.

// ---- ChatMessage: SAY / YELL / GUILD / OFFICER / PARTY / RAID / RAID_WARNING / INSTANCE ----

[PacketCodec(typeof(ChatMessage), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class ChatMessageCodecWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out ChatMessage packet)
    {
        uint language = r.ReadUInt32();
        uint len = r.ReadBits<uint>(11);
        bool isSecure = r.HasBit();
        string text = r.ReadString(len);
        packet = new ChatMessage(language, text, isSecure);
    }
}

[PacketCodec(typeof(ChatMessage), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class ChatMessageCodecPreWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out ChatMessage packet)
    {
        uint language = r.ReadUInt32();
        uint len = r.ReadBits<uint>(9);
        string text = r.ReadString(len);
        packet = new ChatMessage(language, text, IsSecure: false);
    }
}

// ---- AFK / DND / EMOTE: a single length-prefixed string, 11 bits on V3_4_3 and 9 before ----

[PacketCodec(typeof(ChatMessageAFK), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class ChatMessageAFKCodecWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out ChatMessageAFK packet)
        => packet = new ChatMessageAFK(r.ReadString(r.ReadBits<uint>(11)));
}

[PacketCodec(typeof(ChatMessageAFK), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class ChatMessageAFKCodecPreWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out ChatMessageAFK packet)
        => packet = new ChatMessageAFK(r.ReadString(r.ReadBits<uint>(9)));
}

[PacketCodec(typeof(ChatMessageDND), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class ChatMessageDNDCodecWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out ChatMessageDND packet)
        => packet = new ChatMessageDND(r.ReadString(r.ReadBits<uint>(11)));
}

[PacketCodec(typeof(ChatMessageDND), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class ChatMessageDNDCodecPreWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out ChatMessageDND packet)
        => packet = new ChatMessageDND(r.ReadString(r.ReadBits<uint>(9)));
}

[PacketCodec(typeof(ChatMessageEmote), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class ChatMessageEmoteCodecWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out ChatMessageEmote packet)
        => packet = new ChatMessageEmote(r.ReadString(r.ReadBits<uint>(11)));
}

[PacketCodec(typeof(ChatMessageEmote), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class ChatMessageEmoteCodecPreWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out ChatMessageEmote packet)
        => packet = new ChatMessageEmote(r.ReadString(r.ReadBits<uint>(9)));
}

// ---- Whisper: both lengths are read before either string, so they cannot be inlined ----

[PacketCodec(typeof(ChatMessageWhisper), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class ChatMessageWhisperCodecWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out ChatMessageWhisper packet)
    {
        uint language = r.ReadUInt32();
        uint targetLen = r.ReadBits<uint>(9);
        uint textLen = r.ReadBits<uint>(11);
        string target = r.ReadString(targetLen);
        string text = r.ReadString(textLen);
        packet = new ChatMessageWhisper(language, target, text);
    }
}

[PacketCodec(typeof(ChatMessageWhisper), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class ChatMessageWhisperCodecPreWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out ChatMessageWhisper packet)
    {
        uint language = r.ReadUInt32();
        uint targetLen = r.ReadBits<uint>(9);
        uint textLen = r.ReadBits<uint>(9);
        string target = r.ReadString(targetLen);
        string text = r.ReadString(textLen);
        packet = new ChatMessageWhisper(language, target, text);
    }
}

// ---- Channel: the IsSecure pair is conditional, and only exists on V3_4_3 ----

[PacketCodec(typeof(ChatMessageChannel), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class ChatMessageChannelCodecWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out ChatMessageChannel packet)
    {
        uint language = r.ReadUInt32();
        WowGuid128 channelGuid = r.ReadPackedGuid128();
        uint targetLen = r.ReadBits<uint>(9);
        uint textLen = r.ReadBits<uint>(11);

        // A present-bit guarding the value bit. When absent the field stays false, which is what
        // the class this replaced defaulted to.
        bool isSecure = false;
        if (r.HasBit())
            isSecure = r.HasBit();

        string target = r.ReadString(targetLen);
        string text = r.ReadString(textLen);
        packet = new ChatMessageChannel(language, channelGuid, target, text, isSecure);
    }
}

[PacketCodec(typeof(ChatMessageChannel), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class ChatMessageChannelCodecPreWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out ChatMessageChannel packet)
    {
        uint language = r.ReadUInt32();
        WowGuid128 channelGuid = r.ReadPackedGuid128();
        uint targetLen = r.ReadBits<uint>(9);
        uint textLen = r.ReadBits<uint>(9);
        string target = r.ReadString(targetLen);
        string text = r.ReadString(textLen);
        packet = new ChatMessageChannel(language, channelGuid, target, text, IsSecure: false);
    }
}

// ---- Channel membership: no version variance ----

public static class JoinChannelCodec
{
    public static void Read(ref SpanPacketReader r, out JoinChannel packet)
    {
        int chatChannelId = r.ReadInt32();
        uint channelLength = r.ReadBits<uint>(7);
        uint passwordLength = r.ReadBits<uint>(7);
        r.ResetBitPos();
        string channelName = r.ReadString(channelLength);
        string password = r.ReadString(passwordLength);
        packet = new JoinChannel(chatChannelId, channelName, password);
    }
}

public static class LeaveChannelCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out LeaveChannel packet)
    {
        int zoneChannelId = r.ReadInt32();
        string channelName = r.ReadString(r.ReadBits<uint>(7));
        packet = new LeaveChannel(zoneChannelId, channelName);
    }
}

public static class ChannelCommandCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ChannelCommand packet)
        => packet = new ChannelCommand(r.ReadString(r.ReadBits<uint>(7)));
}
