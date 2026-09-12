/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Framework.Constants;
using Framework.GameMath;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class NotifyReceivedMail : ServerPacket, ISpanWritable
{
    public NotifyReceivedMail() : base(Opcode.SMSG_NOTIFY_RECEIVED_MAIL) { }

    public override void Write()
    {
        _worldPacket.WriteFloat(Delay);
    }

    public int MaxSize => 4; // float

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteFloat(Delay);
        return writer.Position;
    }

    public float Delay;
}

public class MailQueryNextTimeResult : ServerPacket, ISpanWritable
{
    public MailQueryNextTimeResult() : base(Opcode.SMSG_MAIL_QUERY_NEXT_TIME_RESULT) { }

    public override void Write()
    {
        _worldPacket.WriteFloat(NextMailTime);
        _worldPacket.WriteInt32(Mails.Count);

        foreach (var entry in Mails)
        {
            _worldPacket.WritePackedGuid128(entry.SenderGuid);
            _worldPacket.WriteFloat(entry.TimeLeft);
            _worldPacket.WriteInt32(entry.AltSenderID);
            _worldPacket.WriteInt8(entry.AltSenderType);
            _worldPacket.WriteInt32(entry.StationeryID);
        }
    }

    // Cap for mail entries - typically just shows a few pending mails
    private const int MaxMails = 10;
    // Each entry: GUID(18) + float(4) + int(4) + sbyte(1) + int(4) = 31 bytes
    private const int MailEntrySize = PackedGuidHelper.MaxPackedGuid128Size + 4 + 4 + 1 + 4;
    // float(4) + count(4) + entries
    public int MaxSize => 4 + 4 + MaxMails * MailEntrySize;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (Mails.Count > MaxMails)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteFloat(NextMailTime);
        writer.WriteInt32(Mails.Count);

        foreach (var entry in Mails)
        {
            writer.WritePackedGuid128(entry.SenderGuid.Low, entry.SenderGuid.High);
            writer.WriteFloat(entry.TimeLeft);
            writer.WriteInt32(entry.AltSenderID);
            writer.WriteInt8(entry.AltSenderType);
            writer.WriteInt32(entry.StationeryID);
        }
        return writer.Position;
    }

    public float NextMailTime;
    public List<MailNextTimeEntry> Mails = new List<MailNextTimeEntry>();

    public class MailNextTimeEntry
    {
        public WowGuid128 SenderGuid;
        public float TimeLeft;
        public int AltSenderID;
        public sbyte AltSenderType;
        public int StationeryID;
    }
}

public readonly record struct MailGetList(WowGuid128 Mailbox);

public class MailListResult : ServerPacket
{
    public MailListResult() : base(Opcode.SMSG_MAIL_LIST_RESULT) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Mails.Count);
        _worldPacket.WriteInt32(TotalNumRecords);

        Mails.ForEach(p => p.Write(_worldPacket));
    }

    public int TotalNumRecords;
    public List<MailListEntry> Mails = new();
}

public class MailListEntry
{
    public void Write(WorldPacket data)
    {
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            // V3_4_3 layout (per CypherCore Source/Game/Networking/Packets/MailPackets.cs:395-435)
            data.WriteInt64(MailID);
            data.WriteUInt32((uint)SenderType);
            data.WriteInt64((long)Cod);
            data.WriteInt32(StationeryID);
            data.WriteInt64((long)SentMoney);
            data.WriteInt32((int)Flags);
            data.WriteFloat(DaysLeft);
            data.WriteInt32(MailTemplateID);
            data.WriteInt32(Attachments.Count);

            switch (SenderType)
            {
                case MailType.Normal:
                    data.WritePackedGuid128(SenderCharacter);
                    break;
                case MailType.Auction:
                case MailType.Creature:
                case MailType.GameObject:
                    data.WriteInt32((int)(AltSenderID ?? 0));
                    break;
            }

            data.WriteBits(Subject.GetByteCount(), 8);
            data.WriteBits(Body.GetByteCount(), 13);
            data.FlushBits();

            Attachments.ForEach(p => p.Write(data));

            data.WriteString(Subject);
            data.WriteString(Body);
            return;
        }

        data.WriteInt32((int)MailID);
        data.WriteUInt8((byte)SenderType);
        data.WriteUInt64(Cod);
        data.WriteInt32(StationeryID);
        data.WriteUInt64(SentMoney);
        data.WriteUInt32(Flags);
        data.WriteFloat(DaysLeft);
        data.WriteInt32(MailTemplateID);
        data.WriteInt32(Attachments.Count);

        data.WriteBit(SenderCharacter != default);
        data.WriteBit(AltSenderID.HasValue);
        data.WriteBits(Subject.GetByteCount(), 8);
        data.WriteBits(Body.GetByteCount(), 13);
        data.FlushBits();

        Attachments.ForEach(p => p.Write(data));

        if (SenderCharacter != default)
            data.WritePackedGuid128(SenderCharacter);

        if (AltSenderID.HasValue)
            data.WriteUInt32(AltSenderID.Value);

        data.WriteString(Subject);
        data.WriteString(Body);
    }

    public long MailID;
    public MailType SenderType;
    public WowGuid128 SenderCharacter;
    public uint? AltSenderID;
    public ulong Cod;
    public int StationeryID;
    public ulong SentMoney;
    public uint Flags;
    public float DaysLeft;
    public int MailTemplateID;
    public string Subject = "";
    public string Body = "";
    public uint ItemTextId; // not sent for new clients, save it here so we can fetch text prior to 3.3
    public List<MailAttachedItem> Attachments = new();
}

public class MailAttachedItem
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt8(Position);

        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            // V3_4_3 layout (per CypherCore Source/Game/Networking/Packets/MailPackets.cs:321-340)
            data.WriteInt64(AttachID);
            data.WriteInt32((int)Count);
            data.WriteInt32(Charges);
            data.WriteInt32((int)MaxDurability);
            data.WriteInt32((int)Durability);
        }
        else
        {
            data.WriteInt32((int)AttachID);
            data.WriteUInt32(Count);
            data.WriteInt32(Charges);
            data.WriteUInt32(MaxDurability);
            data.WriteUInt32(Durability);
        }

        Item.Write(data);
        data.WriteBits(Enchants.Count, 4);
        data.WriteBits(Gems.Count, 2);
        data.WriteBit(Unlocked);
        data.FlushBits();

        foreach (ItemGemData gem in Gems)
            gem.Write(data);

        foreach (ItemEnchantData en in Enchants)
            en.Write(data);
    }

    public byte Position;
    public long AttachID;
    public ItemInstance Item = new();
    public uint Count;
    public int Charges;
    public uint MaxDurability;
    public uint Durability;
    public bool Unlocked;
    public List<ItemEnchantData> Enchants = new();
    public List<ItemGemData> Gems = new();
}

public readonly record struct MailCreateTextItem(WowGuid128 Mailbox, long MailID);

public readonly record struct MailDelete(long MailID, int DeleteReason);

public readonly record struct MailMarkAsRead(WowGuid128 Mailbox, long MailID);

public readonly record struct MailReturnToSender(long MailID, WowGuid128 SenderGUID);

public readonly record struct MailTakeItem(WowGuid128 Mailbox, long MailID, long AttachID);

public readonly record struct MailTakeMoney(WowGuid128 Mailbox, long MailID, long Money);

public readonly record struct SendMail(
    WowGuid128 Mailbox, int StationeryID, long SendMoney, long Cod,
    string Target, string Subject, string Body, List<MailAttachment> Attachments);

/// <remarks>
/// Lifted out of <see cref="SendMail"/> when that became a record struct. The attachment count is
/// five bits on the wire, so a mail carries at most 31 of these however malformed the packet is.
/// </remarks>
public readonly record struct MailAttachment(byte AttachPosition, WowGuid128 ItemGUID);

public class MailCommandResult : ServerPacket, ISpanWritable
{
    public MailCommandResult() : base(Opcode.SMSG_MAIL_COMMAND_RESULT) { }

    public override void Write()
    {
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            // V3_4_3 layout (per CypherCore Source/Game/Networking/Packets/MailPackets.cs:115-123)
            _worldPacket.WriteInt64(MailID);
            _worldPacket.WriteInt32((int)Command);
            _worldPacket.WriteInt32((int)ErrorCode);
            _worldPacket.WriteInt32((int)BagResult);
            _worldPacket.WriteInt64(AttachID);
            _worldPacket.WriteInt32((int)QtyInInventory);
            return;
        }

        _worldPacket.WriteUInt32((uint)MailID);
        _worldPacket.WriteUInt32((uint)Command);
        _worldPacket.WriteUInt32((uint)ErrorCode);
        _worldPacket.WriteUInt32((uint)BagResult);
        _worldPacket.WriteUInt32((uint)AttachID);
        _worldPacket.WriteUInt32(QtyInInventory);
    }

    public int MaxSize => ModernVersion.Build == ClientVersionBuild.V3_4_3_54261 ? 32 : 24;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            writer.WriteInt64(MailID);
            writer.WriteInt32((int)Command);
            writer.WriteInt32((int)ErrorCode);
            writer.WriteInt32((int)BagResult);
            writer.WriteInt64(AttachID);
            writer.WriteInt32((int)QtyInInventory);
        }
        else
        {
            writer.WriteUInt32((uint)MailID);
            writer.WriteUInt32((uint)Command);
            writer.WriteUInt32((uint)ErrorCode);
            writer.WriteUInt32((uint)BagResult);
            writer.WriteUInt32((uint)AttachID);
            writer.WriteUInt32(QtyInInventory);
        }
        return writer.Position;
    }

    public long MailID;
    public MailActionType Command;
    public MailErrorType ErrorCode;
    public InventoryResult BagResult;
    public long AttachID;
    public uint QtyInInventory;
}
