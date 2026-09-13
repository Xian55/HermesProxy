using System.Runtime.CompilerServices;
using Framework.IO;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public static class SupportTicketSubmitComplaintCodec
{
    /// <remarks>
    /// Two layouts, and both can stop part-way. The old body was a class whose fields kept
    /// whatever had been assigned before it returned, and the handler ran on that partial state;
    /// the two early exits below construct the packet from exactly what had been read at that
    /// point, so the behaviour carries over rather than silently becoming all-defaults.
    /// </remarks>
    public static void Read(ref SpanPacketReader r, out SupportTicketSubmitComplaint packet)
    {
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            ReadV343(ref r, out packet);
            return;
        }

        var header = new SupportTicketHeader();
        header.Read(ref r, withProgram: false);
        WowGuid128 targetGuid = r.ReadPackedGuid128();

        var chatLog = new SupportTicketChatLog();
        chatLog.Read(ref r);

        var complaintType = (GmTicketComplaintType)r.ReadBits<uint>(5);

        uint noteLength = r.ReadBits<uint>(10);

        bool hasMailInfo = r.ReadBit();
        bool unk2 = r.ReadBit();
        bool unk3 = r.ReadBit();
        bool hasGuildInfo = r.ReadBit();
        bool unk5 = r.ReadBit();
        bool unk6 = r.ReadBit();
        bool hasClubMessage = r.ReadBit();
        bool unk8 = r.ReadBit();
        bool unk9 = r.ReadBit();

        r.ResetBitPos();

        if (hasClubMessage)
        {
            bool isUsingVoice = r.ReadBit();
            r.ResetBitPos();
        }

        uint unkAlwaysZero = r.ReadUInt32();
        if (unkAlwaysZero != 0)
        {
            Log.Print(LogType.Error, "You reported something that we do not handle (?)");
            Log.Print(LogType.Error, "Please create a new issue on GitHub and tell us what you did");
            packet = new SupportTicketSubmitComplaint(
                header, targetGuid, default, default, default, chatLog, null, complaintType, string.Empty);
            return;
        }

        SupportTicketMailInfo? mail = null;
        if (hasMailInfo)
        {
            mail = new SupportTicketMailInfo();
            mail.Read(ref r);
        }

        packet = new SupportTicketSubmitComplaint(
            header, targetGuid, default, default, default, chatLog, mail, complaintType,
            r.ReadString(noteLength));
    }

    /// <summary>
    /// V3_4_3 layout, byte-verified against six native captures covering every report category the
    /// client offers. It differs from the older builds in three ways: three int32 category fields
    /// sit between the target GUID and the chat log, the note is read *after* the Horus chat log
    /// rather than after the optional blocks, and the header carries a trailing Program FourCC.
    /// </summary>
    /// <remarks>
    /// Internal rather than private so the capture tests can reach it directly. The test process
    /// runs as one fixed build and it is not V3_4_3, so going through Read would take the older
    /// layout and the captures would decode as nonsense.
    /// </remarks>
    internal static void ReadV343(ref SpanPacketReader r, out SupportTicketSubmitComplaint packet)
    {
        var header = new SupportTicketHeader();
        header.Read(ref r, withProgram: true);
        WowGuid128 targetGuid = r.ReadPackedGuid128();

        var reportType = (ReportType)r.ReadInt32();
        var majorCategory = (ReportMajorCategory)r.ReadInt32();
        var minorCategoryFlags = (ReportMinorCategory)r.ReadInt32();

        var chatLog = new SupportTicketChatLog();
        chatLog.Read(ref r);

        uint noteLength = r.ReadBits<uint>(10);

        bool hasMailInfo = r.ReadBit();
        bool hasCalendarInfo = r.ReadBit();
        bool hasPetInfo = r.ReadBit();
        bool hasGuildInfo = r.ReadBit();
        bool hasLFGListSearchResult = r.ReadBit();
        bool hasLFGListApplicant = r.ReadBit();
        bool hasClubMessage = r.ReadBit();
        bool hasClubFinderResult = r.ReadBit();
        bool hasUnk910 = r.ReadBit();

        r.ResetBitPos();

        if (hasClubMessage)
        {
            r.ReadBit(); // IsPlayerUsingVoice
            r.ResetBitPos();
        }

        // HorusChatLog: a line count followed by that many lines. Always empty in the captures,
        // and the proxy has nothing to do with community chat, so the lines are not decoded -
        // bail out rather than read past a structure we cannot forward anyway.
        uint horusLineCount = r.ReadUInt32();
        if (horusLineCount != 0)
        {
            Log.Print(LogType.Error, "Support ticket carried a community chat log, which is not translated");
            packet = new SupportTicketSubmitComplaint(
                header, targetGuid, reportType, majorCategory, minorCategoryFlags, chatLog, null,
                default, string.Empty);
            return;
        }

        string textNote = r.ReadString(noteLength);

        SupportTicketMailInfo? mail = null;
        if (hasMailInfo)
        {
            mail = new SupportTicketMailInfo();
            mail.Read(ref r);
        }

        packet = new SupportTicketSubmitComplaint(
            header, targetGuid, reportType, majorCategory, minorCategoryFlags, chatLog, mail,
            default, textNote);
    }
}
