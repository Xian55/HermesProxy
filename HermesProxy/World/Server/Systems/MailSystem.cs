using System;
using System.Collections.Generic;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>Translation for the modern client's mail CMSGs.</summary>
/// <remarks>
/// Six of these widened their mail id to 64-bit in V3_4_3 and read it through a ranged codec pair
/// rather than a branch, so nothing here is version-aware on the modern axis. The legacy axis
/// still is: 3.3.5a moved several fields relative to vanilla, which is what the
/// <c>LegacyVersion.AddedInVersion</c> guards below are.
/// </remarks>
public static class MailSystem
{
    [HandlesCmsg(Opcode.CMSG_QUERY_NEXT_MAIL_TIME)]
    public static void HandleQueryNextMailTime(in EmptyClientPacket mail, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_QUERY_NEXT_MAIL_TIME);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_MAIL_GET_LIST)]
    public static void HandleMailGetList(in MailGetList mail, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_MAIL_GET_LIST);
        packet.WriteGuid(mail.Mailbox.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_MAIL_CREATE_TEXT_ITEM)]
    public static void HandleMailCreateTextItem(in MailCreateTextItem mail, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_MAIL_CREATE_TEXT_ITEM);
        packet.WriteGuid(mail.Mailbox.To64());
        packet.WriteUInt32((uint)mail.MailID);
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.WriteUInt32(0); // Mail Template Id
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_MAIL_DELETE)]
    public static void HandleMailDelete(in MailDelete mail, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_MAIL_DELETE);
        packet.WriteGuid(ctx.GetSession().GameState.CurrentInteractedWithGO.To64());
        packet.WriteUInt32((uint)mail.MailID);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            packet.WriteUInt32(0); // Mail Template Id
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_MAIL_MARK_AS_READ)]
    public static void HandleMailMarkAsRead(in MailMarkAsRead mail, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_MAIL_MARK_AS_READ);
        packet.WriteGuid(mail.Mailbox.To64());
        packet.WriteUInt32((uint)mail.MailID);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_MAIL_RETURN_TO_SENDER)]
    public static void HandleMailReturnToSender(in MailReturnToSender mail, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_MAIL_RETURN_TO_SENDER);
        packet.WriteGuid(ctx.GetSession().GameState.CurrentInteractedWithGO.To64());
        packet.WriteUInt32((uint)mail.MailID);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            packet.WriteGuid(mail.SenderGUID.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_MAIL_TAKE_ITEM)]
    public static void HandleMailTakeItem(in MailTakeItem mail, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_MAIL_TAKE_ITEM);
        packet.WriteGuid(mail.Mailbox.To64());
        packet.WriteUInt32((uint)mail.MailID);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            packet.WriteUInt32((uint)mail.AttachID);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_MAIL_TAKE_MONEY)]
    public static void HandleMailTakeMoney(in MailTakeMoney mail, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_MAIL_TAKE_MONEY);
        packet.WriteGuid(mail.Mailbox.To64());
        packet.WriteUInt32((uint)mail.MailID);
        ctx.SendPacketToServer(packet);
    }

    static void BuildSendMail(in SessionContext ctx, in SendMail mail, long sendMoney, long cod,
        List<MailAttachment> attachments)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SEND_MAIL);
        packet.WriteGuid(mail.Mailbox.To64());
        packet.WriteCString(mail.Target);
        packet.WriteCString(mail.Subject);
        packet.WriteCString(mail.Body);
        packet.WriteInt32(mail.StationeryID);
        packet.WriteUInt32(0); // unk

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            packet.WriteUInt8((byte)attachments.Count);
            foreach (var item in attachments)
            {
                packet.WriteUInt8(item.AttachPosition);
                packet.WriteGuid(item.ItemGUID.To64());
            }
        }
        else
        {
            if (attachments.Count > 0)
                packet.WriteGuid(attachments[0].ItemGUID.To64());
            else
                packet.WriteGuid(WowGuid64.Empty);
        }

        packet.WriteUInt32((uint)sendMoney);
        packet.WriteUInt32((uint)cod);
        packet.WriteUInt64(0); // unk
        packet.WriteUInt8(0); // unk
        ctx.SendPacketToServer(packet);
    }

    /// <remarks>
    /// The vanilla split path divided <c>SendMoney</c> and <c>Cod</c> in place on the packet before
    /// the loop. A readonly record struct cannot be mutated, so the divided values are locals passed
    /// to <see cref="BuildSendMail"/> instead — computed once before the loop, as they were.
    /// </remarks>
    [HandlesCmsg(Opcode.CMSG_SEND_MAIL)]
    public static void HandleSendMail(in SendMail mail, in SessionContext ctx)
    {
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) ||
            mail.Attachments.Count <= 1)
            BuildSendMail(in ctx, in mail, mail.SendMoney, mail.Cod, mail.Attachments);
        else
        {
            // only 1 item can be attached in vanilla
            // split them into multiple mails
            long sendMoney = mail.SendMoney / mail.Attachments.Count;
            long cod = mail.Cod / mail.Attachments.Count;
            foreach (var item in mail.Attachments)
            {
                List<MailAttachment> attachments = new List<MailAttachment>();
                attachments.Add(item);
                BuildSendMail(in ctx, in mail, sendMoney, cod, attachments);
                System.Threading.Thread.Sleep(500); // prevent triggering antiflood on server
            }
        }
    }
}
