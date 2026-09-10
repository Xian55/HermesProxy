using System;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// Byte-for-byte regression tests for the V3_4_3 <c>CMSG_SUPPORT_TICKET_SUBMIT_COMPLAINT</c>
/// layout, built from native captures.
///
/// Every packet below was captured from a 3.4.3.54261 client talking directly to a native
/// Wrathion server with no proxy in the path, one per report category the client offers. They are
/// the reason this layout is known rather than guessed: WowPacketParser applies its V6_0_2 parser
/// to this build, which lacks the header's trailing Program field and the three category ints, so
/// it desyncs and throws <c>EndOfStreamException</c> on the very same bytes.
///
/// The strongest check here is that the 10-bit note length agrees with the length of the note text
/// that follows it — if any field ahead of it were mis-sized, the bit offset would shift and the
/// string would come out wrong.
/// </summary>
public class SupportTicketComplaintV343Tests
{
    // Header(24) + packed GUID(5) + ReportType/Major/Minor(12) + ChatLog count(4) +
    // report-line bit(1) + noteLength/flag bits(3) + club-message bit(1) + Horus count(4) + note.
    private static SupportTicketSubmitComplaint Parse(byte[] raw)
    {
        var worldPacket = new WorldPacket(0u, raw);
        var complaint = new SupportTicketSubmitComplaint(worldPacket);
        complaint.ReadV343(worldPacket);
        return complaint;
    }

    private static byte[] Bytes(string hex) => Convert.FromHexString(hex.Replace(" ", ""));

    // "stolen name" - InappropriateName / CharacterName
    private const string StolenName =
        "01 00 00 00 A1 3E 21 46 54 B7 50 44 DE CB A5 44 4B F1 58 40 57 6F 57 00 01 A0 62 04 08 " +
        "01 00 00 00 03 00 00 00 00 08 00 00 00 00 00 00 00 02 C0 80 00 00 00 00 00 " +
        "73 74 6F 6C 65 6E 20 6E 61 6D 65";

    // "hacking" - Cheating / Hacking
    private const string Hacking =
        "01 00 00 00 A1 3E 21 46 54 B7 50 44 DE CB A5 44 4B F1 58 40 57 6F 57 00 01 A0 62 04 08 " +
        "01 00 00 00 02 00 00 00 40 00 00 00 00 00 00 00 00 01 C0 80 00 00 00 00 00 " +
        "68 61 63 6B 69 6E 67";

    // "botting" - Cheating / Botting
    private const string Botting =
        "01 00 00 00 A1 3E 21 46 54 B7 50 44 DE CB A5 44 4B F1 58 40 57 6F 57 00 01 A0 62 04 08 " +
        "01 00 00 00 02 00 00 00 80 00 00 00 00 00 00 00 00 01 C0 80 00 00 00 00 00 " +
        "62 6F 74 74 69 6E 67";

    // "afk/non-participation" - GameplaySabotage / Afk
    private const string Afk =
        "01 00 00 00 A1 3E 21 46 54 B7 50 44 DE CB A5 44 4B F1 58 40 57 6F 57 00 01 A0 62 04 08 " +
        "01 00 00 00 01 00 00 00 08 00 00 00 00 00 00 00 00 05 40 80 00 00 00 00 00 " +
        "61 66 6B 2F 6E 6F 6E 2D 70 61 72 74 69 63 69 70 61 74 69 6F 6E";

    // "Intentionally feeding" - GameplaySabotage / IntentionallyFeeding
    private const string Feeding =
        "01 00 00 00 A1 3E 21 46 54 B7 50 44 DE CB A5 44 4B F1 58 40 57 6F 57 00 01 A0 62 04 08 " +
        "01 00 00 00 01 00 00 00 10 00 00 00 00 00 00 00 00 05 40 80 00 00 00 00 00 " +
        "49 6E 74 65 6E 74 69 6F 6E 61 6C 6C 79 20 66 65 65 64 69 6E 67";

    // "Blocking team progress" - GameplaySabotage / BlockingProgress
    private const string Blocking =
        "01 00 00 00 A1 3E 21 46 54 B7 50 44 DE CB A5 44 4B F1 58 40 57 6F 57 00 01 A0 62 04 08 " +
        "01 00 00 00 01 00 00 00 20 00 00 00 00 00 00 00 00 05 80 80 00 00 00 00 00 " +
        "42 6C 6F 63 6B 69 6E 67 20 74 65 61 6D 20 70 72 6F 67 72 65 73 73";

    [Theory]
    [InlineData(StolenName, ReportMajorCategory.InappropriateName, ReportMinorCategory.CharacterName, "stolen name")]
    [InlineData(Hacking, ReportMajorCategory.Cheating, ReportMinorCategory.Hacking, "hacking")]
    [InlineData(Botting, ReportMajorCategory.Cheating, ReportMinorCategory.Botting, "botting")]
    [InlineData(Afk, ReportMajorCategory.GameplaySabotage, ReportMinorCategory.Afk, "afk/non-participation")]
    [InlineData(Feeding, ReportMajorCategory.GameplaySabotage, ReportMinorCategory.IntentionallyFeeding, "Intentionally feeding")]
    [InlineData(Blocking, ReportMajorCategory.GameplaySabotage, ReportMinorCategory.BlockingProgress, "Blocking team progress")]
    public void ReadV343_DecodesNativeCapture(string hex, ReportMajorCategory major, ReportMinorCategory minor, string note)
    {
        var complaint = Parse(Bytes(hex));

        Assert.Equal(ReportType.InWorld, complaint.ReportType);
        Assert.Equal(major, complaint.MajorCategory);
        Assert.Equal(minor, complaint.MinorCategoryFlags);
        Assert.Equal(note, complaint.TextNote);
    }

    [Fact]
    public void ReadV343_DecodesHeaderIncludingProgramFourCc()
    {
        var complaint = Parse(Bytes(StolenName));

        Assert.Equal(1u, complaint.Header.SelfPlayerMapId);
        Assert.Equal(10319.657f, complaint.Header.SelfPlayerPos.X, 3);
        Assert.Equal(834.8645f, complaint.Header.SelfPlayerPos.Y, 3);
        Assert.Equal(1326.3708f, complaint.Header.SelfPlayerPos.Z, 3);
        Assert.Equal(3.3897274f, complaint.Header.SelfPlayerOrientation, 5);

        // 0x00576F57 == "WoW". Its absence from the older layout is what makes this build's
        // packet a different shape rather than a superset.
        Assert.Equal(0x00576F57u, complaint.Header.Program);
    }

    [Fact]
    public void ReadV343_DecodesReportedPlayerGuid()
    {
        var complaint = Parse(Bytes(StolenName));

        // Packed as lowMask 0x01 / highMask 0xA0 -> low byte 0x62, high bytes 5 and 7.
        Assert.False(complaint.TargetCharacterGuid.IsEmpty());
        Assert.Equal(98u, complaint.TargetCharacterGuid.GetCounter());
    }

    [Fact]
    public void ReadV343_ConsumesEveryByteOfTheCapture()
    {
        // If any field were mis-sized the note would still parse, but trailing bytes would be
        // left over. The client sends no optional blocks in these captures, so the note is last.
        var raw = Bytes(StolenName);
        var worldPacket = new WorldPacket(0u, raw);
        var complaint = new SupportTicketSubmitComplaint(worldPacket);

        complaint.ReadV343(worldPacket);

        Assert.False(worldPacket.CanRead());
    }
}
