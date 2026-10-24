using System;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using HermesProxy.World.Server.Systems;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// The BroadcastText row the proxy sends as a hotfix for an NPC's text. Its Flags is a u8 up to 3.4.3
/// and a u16 from 4.4.1 on (WoWDBDefs BroadcastText, WowPacketParser's 4.4 reader). Written as a u8
/// for the 4.4.2 client, every field after it came a byte early: the text's emote was read as the
/// top byte of a sound kit id and never played.
/// </summary>
/// <remarks>Each test runs under the client builds it names (<c>HERMES_TEST_MODERN_BUILD</c>).</remarks>
public sealed class BroadcastTextReplyTests
{
    // ID, LanguageID, ConditionID, EmotesID
    private const string AfterTexts = "{0}" + "00000000" + "00000000" + "0000";

    // ChatBubbleDurationMs, VoiceOverPriorityID, two SoundKitIDs, the emotes 6 (question), 0, 0 and
    // their delays 0, 0, 0
    private const string AfterFlags = "00000000" + "00000000" + "0000000000000000" + "060000000000" + "000000000000";

    [Fact]
    public void Cataclysm_FlagsIsAU16()
    {
        if (ModernVersion.ExpansionVersion < 4)
            return;

        var (id, data) = Reply();

        Assert.Equal(Texts() + string.Format(AfterTexts, Id(id)) + "0000" + AfterFlags, data);
    }

    [Fact]
    public void BeforeCataclysm_FlagsIsAU8()
    {
        if (ModernVersion.ExpansionVersion >= 4)
            return;

        var (id, data) = Reply();

        Assert.Equal(Texts() + string.Format(AfterTexts, Id(id)) + "00" + AfterFlags, data);
    }

    private static (uint Id, string Data) Reply()
    {
        uint id = GameData.GetBroadcastTextId("Hi there.", "Hi there.", 0, [0, 0, 0], [6, 0, 0]);
        var reply = new DBReply();
        HotfixSystem.FillBroadcastTextReply(reply, id);
        return (id, Convert.ToHexString(reply.Data.GetData()));
    }

    private static string Texts()
    {
        string text = Convert.ToHexString("Hi there.\0"u8);
        return text + text;
    }

    private static string Id(uint id) => Convert.ToHexString(BitConverter.GetBytes(id));
}
