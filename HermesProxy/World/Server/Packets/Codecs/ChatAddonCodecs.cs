using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

// Addon-channel and emote CMSG codecs.
//
// The two addon messages share ChatAddonMessageParams, whose bit lengths are read before the int32
// they precede — prefix 5 bits, text 8, then a logged flag, then the type. The targeted variant
// puts its own 9-bit target length ahead of all of that, so the three lengths are read in an order
// that has nothing to do with where their strings land.

public static class ChatAddonMessageParamsCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ChatAddonMessageParams packet)
    {
        r.ResetBitPos();
        uint prefixLen = r.ReadBits<uint>(5);
        uint textLen = r.ReadBits<uint>(8);
        bool isLogged = r.HasBit();
        var type = (ChatMessageTypeModern)r.ReadInt32();
        string prefix = r.ReadString(prefixLen);
        packet = new ChatAddonMessageParams(prefix, r.ReadString(textLen), type, isLogged);
    }
}

public static class ChatAddonMessageCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ChatAddonMessage packet)
    {
        ChatAddonMessageParamsCodec.Read(ref r, out var p);
        packet = new ChatAddonMessage(p);
    }
}

[PacketCodec(typeof(ChatAddonMessageTargeted), RemovedIn = ClientVersionBuild.V4_4_2_60895)]
public static class ChatAddonMessageTargetedCodecPreCataClassic
{
    /// <remarks>
    /// The target length is read first and the target string last, with the whole params block and
    /// a packed GUID in between — reading it next to its own string would misalign everything.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ChatAddonMessageTargeted packet)
    {
        uint targetLen = r.ReadBits<uint>(9);
        ChatAddonMessageParamsCodec.Read(ref r, out var p);
        WowGuid128 channelGuid = r.ReadPackedGuid128();
        packet = new ChatAddonMessageTargeted(p, channelGuid, r.ReadString(targetLen));
    }
}

// 4.4.2 (docs/protocol, and a captured addon whisper): the params come first, then the channel's
// GUID, the target's GUID and realm, and two 9-bit lengths that count a trailing NUL which is on
// the wire, as in this build's whisper. TrinityCore cata_classic reads two 7-bit lengths; the
// captured packet has 04 80 00 there for "Elpriest", which is 9 and 0 in 9 bits each and makes
// no sense in 7. The opcode is this build's own as well: 3.4.3 sends an addon whisper through
// the plain addon opcode.
[PacketCodec(typeof(ChatAddonMessageTargeted), AddedIn = ClientVersionBuild.V4_4_2_60895)]
public static class ChatAddonMessageTargetedCodecCataClassic
{
    public static void Read(ref SpanPacketReader r, out ChatAddonMessageTargeted packet)
    {
        ChatAddonMessageParamsCodec.Read(ref r, out var p);
        WowGuid128 channelGuid = r.ReadPackedGuid128();
        r.ReadPackedGuid128();                      // PlayerGUID
        r.ReadUInt32();                             // PlayerVirtualRealmAddress
        uint targetLen = r.ReadBits<uint>(9);
        uint channelLen = r.ReadBits<uint>(9);
        string target = ReadTerminated(ref r, targetLen);
        ReadTerminated(ref r, channelLen);          // the channel is found by its GUID
        packet = new ChatAddonMessageTargeted(p, channelGuid, target);
    }

    private static string ReadTerminated(ref SpanPacketReader r, uint len)
    {
        if (len <= 1)
            return "";

        string value = r.ReadString(len - 1);
        r.ReadUInt8();                              // NUL
        return value;
    }
}

public static class CTextEmoteCodec
{
    /// <remarks>
    /// SequenceVariation sits between the kit count and the kits themselves, and only on 9.2/1.14.2
    /// and later — so the count is read, then optionally the variation, then the kits.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out CTextEmote packet)
    {
        WowGuid128 target = r.ReadPackedGuid128();
        int emoteId = r.ReadInt32();
        int soundIndex = r.ReadInt32();

        int sequenceVariation = 0;
        uint[] kits = System.Array.Empty<uint>();

        if (ModernVersion.AddedInVersion(9, 0, 5, 1, 14, 0, 2, 5, 1))
        {
            uint count = r.ReadUInt32();
            if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
                sequenceVariation = r.ReadInt32();

            int capacity = CodecHelpers.WireCountCapacity(count, in r, sizeof(uint));
            kits = capacity != 0 ? new uint[capacity] : System.Array.Empty<uint>();
            for (int i = 0; i < kits.Length; ++i)
                kits[i] = r.ReadUInt32();
        }

        packet = new CTextEmote(target, emoteId, soundIndex, sequenceVariation, kits);
    }
}

public static class ChatRegisterAddonPrefixesCodec
{
    /// <remarks>
    /// Each prefix carries its own 5-bit length inline, so the count cannot pre-size against
    /// remaining bytes the way a fixed-width element list can — the 64 cap the original body
    /// applied is the bound, and the list grows into it.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ChatRegisterAddonPrefixes packet)
    {
        int count = r.ReadInt32();
        int capped = count < 0 ? 0 : (count < MaxPrefixes ? count : MaxPrefixes);

        var prefixes = new List<string>(capped);
        for (int i = 0; i < capped; ++i)
            prefixes.Add(r.ReadString(r.ReadBits<uint>(5)));

        packet = new ChatRegisterAddonPrefixes(prefixes);
    }

    private const int MaxPrefixes = 64;
}
