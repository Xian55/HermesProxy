using System;
using System.Collections.Generic;
using Framework.GameMath;
using Framework.IO;
using HermesProxy.Enums;
using Framework.Logging;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

/// <summary>
/// CMSG_SUPPORT_TICKET_SUBMIT_COMPLAINT — the in-game "report player" form.
/// </summary>
/// <remarks>
/// The codec has two layouts and both can bail out part-way, leaving the later fields at their
/// defaults for the system to work with. That is deliberate and matches the body this replaced:
/// a complaint carrying a payload the proxy cannot translate is still forwarded as a plain
/// harassment ticket rather than dropped, because the report itself is the point.
/// </remarks>
public readonly record struct SupportTicketSubmitComplaint(
    SupportTicketHeader Header,
    WowGuid128 TargetCharacterGuid,
    ReportType ReportType,
    ReportMajorCategory MajorCategory,
    ReportMinorCategory MinorCategoryFlags,
    SupportTicketChatLog ChatLog,
    SupportTicketMailInfo? SelectedMailInfo,
    GmTicketComplaintType ComplaintType,
    string TextNote);

/// <summary>Where the reporting player was standing.</summary>
public class SupportTicketHeader
{
    /// <remarks>
    /// V3_4_3 appends a program FourCC - observed as 0x00576F57 ("WoW") in every native capture.
    /// WowPacketParser omits it entirely, which is why its parse of this packet desyncs on this
    /// build.
    /// </remarks>
    public void Read(ref SpanPacketReader r, bool withProgram)
    {
        SelfPlayerMapId = r.ReadUInt32();
        SelfPlayerPos = r.ReadVector3();
        SelfPlayerOrientation = r.ReadFloat();

        if (withProgram)
            Program = r.ReadUInt32();
    }

    public uint SelfPlayerMapId;
    public Vector3 SelfPlayerPos;
    public float SelfPlayerOrientation;
    public uint Program;
}

/// <summary>The chat lines the player selected when reporting.</summary>
public class SupportTicketChatLog
{
    /// <remarks>
    /// Each line's 12-bit text length sits inside the loop, after its own timestamp, so the lines
    /// cannot be skipped in bulk - and the optional reported-line index trails all of them.
    /// </remarks>
    public void Read(ref SpanPacketReader r)
    {
        uint chatLogLineCount = r.ReadUInt32();
        bool hasReportedLineIndex = r.ReadBool();

        for (var i = 0; i < chatLogLineCount; i++)
        {
            DateTime time = r.ReadTime64();
            uint textLength = r.ReadBits<uint>(12);
            r.ResetBitPos();
            ChatLines.Add(new ChatLine { Time = time, Text = r.ReadString(textLength) });
        }

        if (hasReportedLineIndex)
            ReportedLineIdx = r.ReadUInt32();
    }

    public List<ChatLine> ChatLines = new();
    public uint? ReportedLineIdx;

    public class ChatLine
    {
        public DateTime Time;
        public string Text = string.Empty;
    }
}

/// <summary>The mail a player is reporting, when the complaint is about one.</summary>
public class SupportTicketMailInfo
{
    /// <remarks>Both lengths are read before either string, so neither can be read in place.</remarks>
    public void Read(ref SpanPacketReader r)
    {
        MailId = r.ReadUInt32();

        uint textBodyLength = r.ReadBits<uint>(13);
        uint subjectLength = r.ReadBits<uint>(9);
        r.ResetBitPos();

        MailTextBody = r.ReadString(textBodyLength);
        MailSubject = r.ReadString(subjectLength);
    }

    public uint MailId;
    public string MailTextBody = string.Empty;
    public string MailSubject = string.Empty;
}

/// <summary>
/// Reply to <c>CMSG_GM_TICKET_GET_SYSTEM_STATUS</c>. Native writes a single int32; a capture of
/// Wrathion answering a 3.4.3 client shows exactly 4 bytes with Status = 1 (enabled).
/// </summary>
class GMTicketSystemStatus : ServerPacket
{
    public GMTicketSystemStatus() : base(Opcode.SMSG_GM_TICKET_SYSTEM_STATUS) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Status);
    }

    public int Status;
}

/// <summary>
/// Reply to <c>CMSG_GM_TICKET_GET_CASE_STATUS</c>. Legacy has no concept of GM cases, and native
/// 3.4.3 does not implement them either - its handler is a stub that returns an empty list - so
/// an empty list is the faithful answer. Native's capture is 4 bytes, CasesCount = 0.
/// </summary>
class GMTicketCaseStatus : ServerPacket
{
    public GMTicketCaseStatus() : base(Opcode.SMSG_GM_TICKET_CASE_STATUS) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(0); // CasesCount - always empty, see above
        _worldPacket.FlushBits();
    }
}
