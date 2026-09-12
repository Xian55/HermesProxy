using System;
using Framework.IO;
using HermesProxy;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;
using Frozen = HermesProxy.Tests.World.Dispatch.Reference.FrozenPackets;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Equivalence for the chat codecs — the first packets whose layout differs by client build, and
/// so the first to use ranged codecs instead of an <c>if (ModernVersion.Build == …)</c> inside the
/// reader.
/// </summary>
/// <remarks>
/// <para>
/// <b>A limitation worth naming, because it is an argument for the change.</b> The frozen oracle
/// still has the version branch inside it, and <c>ModernVersion.Build</c> is <c>static readonly</c>
/// — fixed for the life of the process. The test suite runs as <c>V1_14_2_42597</c>, so the oracle
/// can only ever take its 9-bit branch. The V3_4_3 branch of the old code was, in-process,
/// untestable.
/// </para>
/// <para>
/// That is precisely what ranged codecs fix: each build's reader is now a separate type that can be
/// called directly, so both layouts are testable regardless of which build the process is running
/// as. Below, the pre-WotLK codec is proven against the oracle, and the WotLK codec is proven
/// against explicit 11-bit expectations that the oracle cannot reach here.
/// </para>
/// </remarks>
public class ChatCodecEquivalenceTests
{
    static ChatCodecEquivalenceTests()
    {
        if (VersionBootstrap.ModernBuild == ClientVersionBuild.Zero)
            VersionBootstrap.ModernBuild = ClientVersionBuild.V1_14_2_42597;
        if (VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            VersionBootstrap.LegacyBuild = ClientVersionBuild.V3_3_5a_12340;
    }

    private static (WorldPacket Oracle, byte[] Framed) Build(Action<WorldPacket> write)
    {
        using var w = new WorldPacket(1u);
        write(w);
        byte[] payload = w.GetData();
        byte[] framed = new byte[payload.Length + 2];
        payload.CopyTo(framed, 2);
        return (new WorldPacket(framed), framed);
    }

    private static SpanPacketReader ReaderOver(byte[] framed)
        => new(new WorldPacket(framed).GetRemainingSpan());

    /// The suite's build. If this ever changes, the oracle comparisons below swap which codec they
    /// prove — so assert it rather than assume it.
    [Fact]
    public void OracleBranchIsThePreWotLKClassicOne()
        => Assert.NotEqual(ClientVersionBuild.V3_4_3_54261, ModernVersion.Build);

    // ---- pre-WotLK Classic: proven against the frozen oracle ----

    [Theory]
    [InlineData("hello")]
    [InlineData("")]
    [InlineData("a much longer message with spaces and punctuation!")]
    public void ChatMessage_PreWotLK_MatchesOracle(string text)
    {
        var (o, f) = Build(w =>
        {
            w.WriteUInt32(7);
            w.WriteBits((uint)text.Length, 9);
            w.WriteString(text);
        });

        var e = new Frozen.ChatMessage(); e.Read(o);
        var r = ReaderOver(f);
        ChatMessageCodecPreWotLKClassic.Read(ref r, out var a);

        Assert.Equal(e.Language, a.Language);
        Assert.Equal(e.Text, a.Text);
        Assert.Equal(e.IsSecure, a.IsSecure);
    }

    [Theory]
    [InlineData("Thrall", "hello from the proxy!")]
    [InlineData("A", "")]
    public void ChatMessageWhisper_PreWotLK_MatchesOracle(string target, string text)
    {
        var (o, f) = Build(w =>
        {
            w.WriteUInt32(7);
            w.WriteBits((uint)target.Length, 9);
            w.WriteBits((uint)text.Length, 9);
            w.WriteString(target);
            w.WriteString(text);
        });

        var e = new Frozen.ChatMessageWhisper(); e.Read(o);
        var r = ReaderOver(f);
        ChatMessageWhisperCodecPreWotLKClassic.Read(ref r, out var a);

        Assert.Equal(e.Language, a.Language);
        Assert.Equal(e.Target, a.Target);
        Assert.Equal(e.Text, a.Text);
    }

    [Fact]
    public void ChatMessageChannel_PreWotLK_MatchesOracle()
    {
        var guid = new WowGuid128(0xDEADBEEFUL, 0x1122334455667788UL);
        const string target = "General";
        const string text = "anyone selling linen?";

        var (o, f) = Build(w =>
        {
            w.WriteUInt32(0);
            w.WritePackedGuid128(guid);
            w.WriteBits((uint)target.Length, 9);
            w.WriteBits((uint)text.Length, 9);
            w.WriteString(target);
            w.WriteString(text);
        });

        var e = new Frozen.ChatMessageChannel(); e.Read(o);
        var r = ReaderOver(f);
        ChatMessageChannelCodecPreWotLKClassic.Read(ref r, out var a);

        Assert.Equal(e.Language, a.Language);
        Assert.Equal(e.ChannelGUID, a.ChannelGUID);
        Assert.Equal(e.Target, a.Target);
        Assert.Equal(e.Text, a.Text);
        Assert.Equal(e.IsSecure, a.IsSecure);
    }

    [Theory]
    [InlineData("brb")]
    [InlineData("")]
    public void AfkDndEmote_PreWotLK_MatchOracle(string text)
    {
        var (oAfk, fAfk) = Build(w => { w.WriteBits((uint)text.Length, 9); w.WriteString(text); });
        var eAfk = new Frozen.ChatMessageAFK(); eAfk.Read(oAfk);
        var rAfk = ReaderOver(fAfk); ChatMessageAFKCodecPreWotLKClassic.Read(ref rAfk, out var aAfk);
        Assert.Equal(eAfk.Text, aAfk.Text);

        var (oDnd, fDnd) = Build(w => { w.WriteBits((uint)text.Length, 9); w.WriteString(text); });
        var eDnd = new Frozen.ChatMessageDND(); eDnd.Read(oDnd);
        var rDnd = ReaderOver(fDnd); ChatMessageDNDCodecPreWotLKClassic.Read(ref rDnd, out var aDnd);
        Assert.Equal(eDnd.Text, aDnd.Text);

        var (oEmote, fEmote) = Build(w => { w.WriteBits((uint)text.Length, 9); w.WriteString(text); });
        var eEmote = new Frozen.ChatMessageEmote(); eEmote.Read(oEmote);
        var rEmote = ReaderOver(fEmote); ChatMessageEmoteCodecPreWotLKClassic.Read(ref rEmote, out var aEmote);
        Assert.Equal(eEmote.Text, aEmote.Text);
    }

    // ---- V3_4_3: the branch the oracle cannot reach in this process ----

    [Theory]
    [InlineData("hello", true)]
    [InlineData("hello", false)]
    [InlineData("", false)]
    public void ChatMessage_WotLKClassic_ReadsElevenBitLengthAndSecureBit(string text, bool isSecure)
    {
        var (_, f) = Build(w =>
        {
            w.WriteUInt32(7);
            w.WriteBits((uint)text.Length, 11);
            w.WriteBit(isSecure);
            w.WriteString(text);
        });

        var r = ReaderOver(f);
        ChatMessageCodecWotLKClassic.Read(ref r, out var a);

        Assert.Equal(7u, a.Language);
        Assert.Equal(text, a.Text);
        Assert.Equal(isSecure, a.IsSecure);
    }

    [Fact]
    public void ChatMessage_ShortText_SurvivesTheElevenBitLength()
    {
        // Issue #177: reading 9 bits of an 11-bit length yields len >> 2, so "gooday" (6) came
        // through as 1 and the client posted "g"; 1-3 character messages read as 0 and posted
        // nothing at all. Pin the short lengths specifically — they are the ones that fail
        // silently rather than visibly.
        foreach (string text in new[] { "g", "go", "goo", "gooday" })
        {
            var (_, f) = Build(w =>
            {
                w.WriteUInt32(0);
                w.WriteBits((uint)text.Length, 11);
                w.WriteBit(false);
                w.WriteString(text);
            });

            var r = ReaderOver(f);
            ChatMessageCodecWotLKClassic.Read(ref r, out var a);
            Assert.Equal(text, a.Text);
        }
    }

    [Fact]
    public void ChatMessageWhisper_WotLKClassic_ReadsNineThenElevenBitLengths()
    {
        const string target = "Thrall";
        const string text = "hello from the proxy!";

        var (_, f) = Build(w =>
        {
            w.WriteUInt32(7);
            w.WriteBits((uint)target.Length, 9);
            w.WriteBits((uint)text.Length, 11);
            w.WriteString(target);
            w.WriteString(text);
        });

        var r = ReaderOver(f);
        ChatMessageWhisperCodecWotLKClassic.Read(ref r, out var a);

        Assert.Equal(target, a.Target);
        Assert.Equal(text, a.Text);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public void ChatMessageChannel_WotLKClassic_HandlesTheConditionalSecurePair(bool present, bool secure)
    {
        // The secure flag is a present-bit guarding a value-bit. When the present bit is clear the
        // field keeps its default — which the class this replaced also did, via its initializer.
        var guid = new WowGuid128(1, 2);
        const string target = "General";
        const string text = "wts linen";

        var (_, f) = Build(w =>
        {
            w.WriteUInt32(0);
            w.WritePackedGuid128(guid);
            w.WriteBits((uint)target.Length, 9);
            w.WriteBits((uint)text.Length, 11);
            w.WriteBit(present);
            if (present)
                w.WriteBit(secure);
            w.WriteString(target);
            w.WriteString(text);
        });

        var r = ReaderOver(f);
        ChatMessageChannelCodecWotLKClassic.Read(ref r, out var a);

        Assert.Equal(guid, a.ChannelGUID);
        Assert.Equal(target, a.Target);
        Assert.Equal(text, a.Text);
        Assert.Equal(present && secure, a.IsSecure);
    }

    // ---- unranged channel membership ----

    [Fact]
    public void JoinChannel_MatchesOracle()
    {
        const string name = "LookingForGroup";
        const string password = "hunter2";

        var (o, f) = Build(w =>
        {
            w.WriteInt32(5);
            w.WriteBits((uint)name.Length, 7);
            w.WriteBits((uint)password.Length, 7);
            w.FlushBits();
            w.WriteString(name);
            w.WriteString(password);
        });

        var e = new Frozen.JoinChannel(); e.Read(o);
        var r = ReaderOver(f); JoinChannelCodec.Read(ref r, out var a);

        Assert.Equal(e.ChatChannelId, a.ChatChannelId);
        Assert.Equal(e.ChannelName, a.ChannelName);
        Assert.Equal(e.Password, a.Password);
    }

    [Fact]
    public void LeaveChannel_MatchesOracle()
    {
        const string name = "Trade";
        var (o, f) = Build(w => { w.WriteInt32(2); w.WriteBits((uint)name.Length, 7); w.WriteString(name); });

        var e = new Frozen.LeaveChannel(); e.Read(o);
        var r = ReaderOver(f); LeaveChannelCodec.Read(ref r, out var a);

        Assert.Equal(e.ZoneChannelID, a.ZoneChannelID);
        Assert.Equal(e.ChannelName, a.ChannelName);
    }

    [Fact]
    public void ChannelCommand_MatchesOracle()
    {
        const string name = "General";
        var (o, f) = Build(w => { w.WriteBits((uint)name.Length, 7); w.WriteString(name); });

        var e = new Frozen.ChannelCommand(); e.Read(o);
        var r = ReaderOver(f); ChannelCommandCodec.Read(ref r, out var a);

        Assert.Equal(e.ChannelName, a.ChannelName);
    }
}
