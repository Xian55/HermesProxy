using System;
using System.Buffers;
using HermesProxy.Configuration.Options;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// <see cref="ChatPkt"/> went from one class with a 3.4.3 branch in each serialiser to one layout
/// per shape. Both serialisers of the 1.14/2.5 and 3.4.3 layouts must give the bytes the old
/// branches did; both serialisers of the 4.4.2 layout must give the bytes of a native TrinityCore
/// cata_classic packet.
/// </summary>
public sealed class ChatPktLayoutTests
{
    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.IsType<ChatPkt.ClassicEraLayout>(ChatPkt.Layouts.For(ClientVersionBuild.V1_14_2_42597));
        Assert.IsType<ChatPkt.ClassicEraLayout>(ChatPkt.Layouts.For(ClientVersionBuild.V2_5_3_41750));
        Assert.IsType<ChatPkt.WotLKClassicLayout>(ChatPkt.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<ChatPkt.CataClassicLayout>(ChatPkt.Layouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    [Theory]
    [InlineData("gooday", false)]
    [InlineData("Ткацкая мастерская", true)]
    public void ClassicEraLayout_MatchesTheOldLegacyBranch(string text, bool channel)
        => WithPacket(text, channel, p => AssertBothSerialisers(new ChatPkt.ClassicEraLayout(), p, OldChat(p, isV343: false)));

    [Theory]
    [InlineData("gooday", false)]
    [InlineData("Ткацкая мастерская", true)]
    public void WotLKClassicLayout_MatchesTheOld3_4_3Branch(string text, bool channel)
        => WithPacket(text, channel, p => AssertBothSerialisers(new ChatPkt.WotLKClassicLayout(), p, OldChat(p, isV343: true)));

    [Fact]
    public void CataClassicLayout_MatchesANativePacket()
    {
        WithSession(session =>
        {
            var player = new WowGuid128(1, 0x0800040000000000);
            var packet = new ChatPkt(session, ChatMessageTypeModern.Say, "gooday", senderName: " ")
            {
                SenderGUID = player,
                SenderName = "",
                SenderGuildGUID = new WowGuid128(0, 0x7000040000000000),
                SenderAccountGUID = new WowGuid128(1, 0x7400000000000000),
                TargetGUID = player,
                SenderVirtualAddress = 0x1010001,
                TargetVirtualAddress = 0x1010001,
            };
            AssertBothSerialisers(new ChatPkt.CataClassicLayout(), packet, Convert.FromHexString(NativeSay));
        });
    }

    // SMSG_CHAT (a /say) from TrinityCore cata_classic to a 4.4.2.60895 client
    // (refs/native-captures/tc_cata_442_20261010-150803.pkt, packet 791).
    private const string NativeSay =
        "010000000001A001040800A004700180017401A00104080100010101000101000000000000000000000000000000000000001800676F6F646179";

    private static void WithSession(Action<GlobalSessionData> body)
    {
        var session = new GlobalSessionData(new ClientOptions(), new LegacyServerOptions(),
            new ProxyNetworkOptions(), new DiagnosticsOptions { PacketsLog = false }, new ThrottlingOptions());
        try
        {
            body(session);
        }
        finally
        {
            session.Executor.Dispose();
        }
    }

    private static void WithPacket(string text, bool channel, Action<ChatPkt> body) => WithSession(session =>
    {
        var packet = new ChatPkt(session, channel ? ChatMessageTypeModern.Channel : ChatMessageTypeModern.Say, text,
            language: 7, senderName: "Xiii", receiverName: "Brannoch", channelName: channel ? "General - Elwynn Forest" : "",
            chatFlags: ChatFlags.GM, addonPrefix: channel ? "DBM" : "", achievementId: 6)
        {
            SenderGUID = new WowGuid128(4023, 0x0800040000000000),
            SenderGuildGUID = new WowGuid128(5, 0x7000040000000000),
            SenderAccountGUID = new WowGuid128(1, 0x7400000000000000),
            TargetGUID = new WowGuid128(4024, 0x0800040000000000),
            PartyGUID = new WowGuid128(3, 0x1F00000000000000),
            SenderVirtualAddress = 0x1010001,
            TargetVirtualAddress = 0x1010001,
            DisplayTime = 2.5f,
            SpellID = 133,
            Unused_801 = channel ? 9u : null,
            HideChatLog = channel,
            FakeSenderName = !channel,
        };
        if (channel)
            packet.ChannelGUID = new WowGuid128(2, 0x2C00000000000000);
        body(packet);
    });

    private static void AssertBothSerialisers(ServerPacketLayout<ChatPkt> layout, ChatPkt packet, byte[] expected)
    {
        using var data = new WorldPacket(1u);
        layout.Write(packet, data);
        Assert.Equal(expected, data.GetDataSpan().ToArray());

        byte[] buffer = ArrayPool<byte>.Shared.Rent(packet.MaxSize);
        try
        {
            int written = layout.WriteToSpan(packet, buffer);
            Assert.Equal(expected, buffer.AsSpan(0, written).ToArray());
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    // ChatPkt.Write as it stood before the layouts, frozen; isV343 was ModernVersion.IsWotLKClassicOrLater.

    private static byte[] OldChat(ChatPkt p, bool isV343)
    {
        using var w = new WorldPacket(1u);
        if (isV343)
        {
            w.WriteUInt8((byte)p.SlashCmd);
            w.WriteUInt32(p._Language);
            w.WritePackedGuid128(p.SenderGUID);
            w.WritePackedGuid128(p.SenderGuildGUID);
            w.WritePackedGuid128(p.SenderAccountGUID);
            w.WritePackedGuid128(p.TargetGUID);
            w.WriteUInt32(p.TargetVirtualAddress);
            w.WriteUInt32(p.SenderVirtualAddress);
            w.WriteInt32((int)p.AchievementID);
            w.WriteFloat(p.DisplayTime);
            w.WriteInt32(p.SpellID);

            w.WriteBits(p.SenderName.GetByteCount(), 11);
            w.WriteBits(p.TargetName.GetByteCount(), 11);
            w.WriteBits(p.Prefix.GetByteCount(), 5);
            w.WriteBits(p.Channel.GetByteCount(), 7);
            w.WriteBits(p.ChatText.GetByteCount(), 12);
            w.WriteBits((uint)p._ChatFlags, 15);
            w.WriteBit(p.HideChatLog);
            w.WriteBit(p.FakeSenderName);
            w.WriteBit(p.Unused_801.HasValue);
            w.WriteBit(p.ChannelGUID != default);
            w.FlushBits();

            w.WriteString(p.SenderName);
            w.WriteString(p.TargetName);
            w.WriteString(p.Prefix);
            w.WriteString(p.Channel);
            w.WriteString(p.ChatText);

            if (p.Unused_801.HasValue)
                w.WriteUInt32(p.Unused_801.Value);
            if (p.ChannelGUID != default)
                w.WritePackedGuid128(p.ChannelGUID);
            return w.GetDataSpan().ToArray();
        }

        w.WriteUInt8((byte)p.SlashCmd);
        w.WriteUInt32((uint)p._Language);
        w.WritePackedGuid128(p.SenderGUID);
        w.WritePackedGuid128(p.SenderGuildGUID);
        w.WritePackedGuid128(p.SenderAccountGUID);
        w.WritePackedGuid128(p.TargetGUID);
        w.WriteUInt32(p.TargetVirtualAddress);
        w.WriteUInt32(p.SenderVirtualAddress);
        w.WritePackedGuid128(p.PartyGUID);
        w.WriteUInt32(p.AchievementID);
        w.WriteFloat(p.DisplayTime);
        w.WriteBits(p.SenderName.GetByteCount(), 11);
        w.WriteBits(p.TargetName.GetByteCount(), 11);
        w.WriteBits(p.Prefix.GetByteCount(), 5);
        w.WriteBits(p.Channel.GetByteCount(), 7);
        w.WriteBits(p.ChatText.GetByteCount(), 12);
        w.WriteBits((byte)p._ChatFlags, 14);
        w.WriteBit(p.HideChatLog);
        w.WriteBit(p.FakeSenderName);
        w.WriteBit(p.Unused_801.HasValue);
        w.WriteBit(p.ChannelGUID != default);
        w.FlushBits();

        w.WriteString(p.SenderName);
        w.WriteString(p.TargetName);
        w.WriteString(p.Prefix);
        w.WriteString(p.Channel);
        w.WriteString(p.ChatText);

        if (p.Unused_801.HasValue)
            w.WriteUInt32(p.Unused_801.Value);

        if (p.ChannelGUID != default)
            w.WritePackedGuid128(p.ChannelGUID);
        return w.GetDataSpan().ToArray();
    }
}
