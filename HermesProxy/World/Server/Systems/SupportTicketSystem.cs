using System;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using System.Text;
using Framework.GameMath;
using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>Translation for the modern client's GM-ticket CMSGs. Behaviour only.</summary>
public static class SupportTicketSystem
{
    [HandlesCmsg(Opcode.CMSG_GM_TICKET_GET_SYSTEM_STATUS)]
    public static void HandleGMTicketGetSystemStatus(in EmptyClientPacket packet, in SessionContext ctx)
    {
        // Forward so the answer reflects the backend's own ticket-system setting.
        ctx.SendPacketToServer(new WorldPacket(Opcode.CMSG_GM_TICKET_GET_SYSTEM_STATUS));
    }

    [HandlesCmsg(Opcode.CMSG_GM_TICKET_GET_CASE_STATUS)]
    public static void HandleGMTicketGetCaseStatus(in EmptyClientPacket packet, in SessionContext ctx)
    {
        ctx.SendPacket(new GMTicketCaseStatus());
    }

    [HandlesCmsg(Opcode.CMSG_SUPPORT_TICKET_SUBMIT_COMPLAINT)]
    public static void HandleSupportTicketSubmitComplaint(in SupportTicketSubmitComplaint complaint, in SessionContext ctx)
{
        var targetPlayerName = ctx.GetSession().GameState.GetPlayerName(complaint.TargetCharacterGuid);
        if (string.IsNullOrWhiteSpace(targetPlayerName))
        {
            ctx.GetSession().SendHermesTextMessage("Unable to report player because CharacterName was not resolved (can be fixed by restarting the client)", isError: true);
            return;
        }

        var ticketText = $"[REPORTED VIA QUICKMENU]\r\nI would like to report player '{targetPlayerName}'";

        if (!WowGuid128.IsUnknownPlayerGuid(complaint.TargetCharacterGuid))
            ticketText += $"  (id: {complaint.TargetCharacterGuid.GetCounter()})";

        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            // V3_4_3 reports a major category plus a minor bitmask instead of one enum.
            ticketText += $" for {complaint.MajorCategory}";
            if (complaint.MinorCategoryFlags != ReportMinorCategory.None)
                ticketText += $" ({complaint.MinorCategoryFlags})";
        }
        else if (complaint.ComplaintType != GmTicketComplaintType.Unknown)
        {
            ticketText += $" for {complaint.ComplaintType}";
        }

        if (complaint.SelectedMailInfo != null)
            ticketText += "\r\n" + $"Mail in question (id: {complaint.SelectedMailInfo.MailId}) with subject '{complaint.SelectedMailInfo.MailSubject}'";

        if (!complaint.TextNote.IsEmpty())
        {
            ticketText += "\r\n" + "-------------";
            ticketText += "\r\n" + complaint.TextNote;
        }

        WorldPacket packet = new WorldPacket(Opcode.CMSG_GM_TICKET_CREATE);

        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            packet.WriteUInt8(2); // GMTICKET_BEHAVIOR_HARASSMENT
            packet.WriteUInt32(complaint.Header.SelfPlayerMapId);
            packet.WriteVector3(complaint.Header.SelfPlayerPos);
            packet.WriteCString(ticketText);
            packet.WriteCString(""); // Not used
        }
        else
        {
            packet.WriteUInt32(complaint.Header.SelfPlayerMapId);
            packet.WriteVector3(complaint.Header.SelfPlayerPos);
            packet.WriteCString(ticketText);
            packet.WriteUInt32(0); // needResponse - we dont need the gm to reach back

            // WotLK reads a needMoreHelp bool between needResponse and the chat-log count
            // (TrinityCore and AzerothCore HandleGMTicketCreateOpcode both declare it). Without
            // it the packet is one byte short of what the server reads and it dies with
            // "ByteBufferException occured while parsing a packet (opcode: 517)", silently -
            // the handler returns before sending SMSG_GMTICKET_CREATE, so the client sees
            // nothing at all. Left off for TBC-era backends, which have no reference here.
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.WriteBool(false); // needMoreHelp

            packet.WriteUInt32(0); // chat lines count
            packet.WriteUInt32(0); // chat text inflated size
            packet.WriteBytes(Array.Empty<byte>()); // rest of the message are deflated chat lines
        }

        ctx.SendPacketToServer(packet);
    }
}
