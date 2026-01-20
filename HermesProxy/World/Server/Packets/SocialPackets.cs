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


using System;
using System.Text;
using Framework.Constants;
using Framework.GameMath;
using Framework.IO;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    public class ContactListRequest : ClientPacket
    {
        public ContactListRequest(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Flags = (SocialFlag)_worldPacket.ReadUInt32();
        }

        public SocialFlag Flags;
    }

    public class ContactList : ServerPacket, ISpanWritable
    {
        public ContactList() : base(Opcode.SMSG_CONTACT_LIST)
        {
            Contacts = new List<ContactInfo>();
        }

        public override void Write()
        {
            _worldPacket.WriteUInt32((uint)Flags);
            _worldPacket.WriteBits(Contacts.Count, 8);
            _worldPacket.FlushBits();

            foreach (ContactInfo contact in Contacts)
                contact.Write(_worldPacket);
        }

        // Cap for contacts (8 bits = 256 max, reduced from 200 to 16 based on typical usage)
        private const int MaxContacts = 16;
        // Cap for note string (10 bits = 1024, using 128)
        private const int MaxNoteBytes = 128;
        // Per contact: 2 GUIDs(36) + 4 uints(16) + byte(1) + bits(2) + note = 183 bytes max
        private const int ContactSize = PackedGuidHelper.MaxPackedGuid128Size * 2 + 16 + 1 + 2 + MaxNoteBytes;
        // uint(4) + bits(1) + contacts
        public int MaxSize => 4 + 1 + MaxContacts * ContactSize;

        public int WriteToSpan(Span<byte> buffer)
        {
            if (Contacts.Count > MaxContacts)
                return -1;

            // Pre-validate note lengths
            foreach (var contact in Contacts)
            {
                if (Encoding.UTF8.GetByteCount(contact.Note) > MaxNoteBytes)
                    return -1;
            }

            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32((uint)Flags);
            writer.WriteBits((uint)Contacts.Count, 8);
            writer.FlushBits();

            foreach (ContactInfo contact in Contacts)
            {
                writer.WritePackedGuid128(contact.Guid.Low, contact.Guid.High);
                writer.WritePackedGuid128(contact.WowAccountGuid.Low, contact.WowAccountGuid.High);
                writer.WriteUInt32(contact.VirtualRealmAddr);
                writer.WriteUInt32(contact.NativeRealmAddr);
                writer.WriteUInt32((uint)contact.TypeFlags);
                writer.WriteUInt8((byte)contact.Status);
                writer.WriteUInt32(contact.AreaID);
                writer.WriteUInt32(contact.Level);
                writer.WriteUInt32((uint)contact.ClassID);
                writer.WriteBits((uint)Encoding.UTF8.GetByteCount(contact.Note), 10);
                writer.WriteBit(contact.Mobile);
                writer.FlushBits();
                writer.WriteString(contact.Note);
            }
            return writer.Position;
        }

        public List<ContactInfo> Contacts;
        public SocialFlag Flags;
    }

    public class ContactInfo
    {
        public void Write(WorldPacket data)
        {
            data.WritePackedGuid128(Guid);
            data.WritePackedGuid128(WowAccountGuid);
            data.WriteUInt32(VirtualRealmAddr);
            data.WriteUInt32(NativeRealmAddr);
            data.WriteUInt32((uint)TypeFlags);
            data.WriteUInt8((byte)Status);
            data.WriteUInt32(AreaID);
            data.WriteUInt32(Level);
            data.WriteUInt32((uint)ClassID);
            data.WriteBits(Note.GetByteCount(), 10);
            data.WriteBit(Mobile);
            data.FlushBits();
            data.WriteString(Note);
        }

        public WowGuid128 Guid;
        public WowGuid128 WowAccountGuid;
        public uint VirtualRealmAddr;
        public uint NativeRealmAddr;
        public SocialFlag TypeFlags;
        public FriendStatus Status;
        public uint AreaID;
        public uint Level;
        public Class ClassID;
        public bool Mobile;
        public string Note = "";
    }

    public class FriendStatusPkt : ServerPacket, ISpanWritable
    {
        public FriendStatusPkt() : base(Opcode.SMSG_FRIEND_STATUS) { }

        public override void Write()
        {
            _worldPacket.WriteUInt8((byte)FriendResult);
            _worldPacket.WritePackedGuid128(Guid);
            _worldPacket.WritePackedGuid128(WowAccountGuid);
            _worldPacket.WriteUInt32(VirtualRealmAddress);
            _worldPacket.WriteUInt8((byte)Status);
            _worldPacket.WriteUInt32(AreaID);
            _worldPacket.WriteUInt32(Level);
            _worldPacket.WriteUInt32((uint)ClassID);
            _worldPacket.WriteBits(Notes.GetByteCount(), 10);
            _worldPacket.WriteBit(Mobile);
            _worldPacket.FlushBits();
            _worldPacket.WriteString(Notes);
        }

        // Cap for friend notes - 10 bits = 1024 max, using 128
        private const int MaxNotesBytes = 128;
        // byte(1) + 2 GUIDs(36) + 4 uints(16) + byte(1) + 11 bits(2) + notes
        public int MaxSize => 1 + PackedGuidHelper.MaxPackedGuid128Size * 2 + 16 + 1 + 2 + MaxNotesBytes;

        public int WriteToSpan(Span<byte> buffer)
        {
            int notesBytes = Notes != null ? Encoding.UTF8.GetByteCount(Notes) : 0;
            if (notesBytes > MaxNotesBytes)
                return -1;

            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt8((byte)FriendResult);
            writer.WritePackedGuid128(Guid.Low, Guid.High);
            writer.WritePackedGuid128(WowAccountGuid.Low, WowAccountGuid.High);
            writer.WriteUInt32(VirtualRealmAddress);
            writer.WriteUInt8((byte)Status);
            writer.WriteUInt32(AreaID);
            writer.WriteUInt32(Level);
            writer.WriteUInt32((uint)ClassID);
            writer.WriteBits((uint)notesBytes, 10);
            writer.WriteBit(Mobile);
            writer.FlushBits();
            if (Notes != null)
                writer.WriteString(Notes);
            return writer.Position;
        }

        public FriendsResult FriendResult;
        public WowGuid128 Guid;
        public WowGuid128 WowAccountGuid;
        public uint VirtualRealmAddress;
        public FriendStatus Status;
        public uint AreaID;
        public uint Level;
        public Class ClassID = Class.None;
        public string Notes;
        public bool Mobile;
    }

    public class AddFriend : ClientPacket
    {
        public AddFriend(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            uint nameLength = _worldPacket.ReadBits<uint>(9);
            uint noteslength = _worldPacket.ReadBits<uint>(10);
            Name = _worldPacket.ReadString(nameLength);
            Note = _worldPacket.ReadString(noteslength);
        }

        public string Note;
        public string Name;
    }

    public class AddIgnore : ClientPacket
    {
        public AddIgnore(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            uint nameLength = _worldPacket.ReadBits<uint>(9);
            if (ModernVersion.AddedInVersion(9, 1, 5, 1, 14, 1, 2, 5, 3))
                AccountGuid = _worldPacket.ReadPackedGuid128();
            Name = _worldPacket.ReadString(nameLength);
        }

        WowGuid128 AccountGuid;
        public string Name;
    }

    public class DelFriend : ClientPacket
    {
        public DelFriend(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            VirtualRealmAddress = _worldPacket.ReadUInt32();
            Guid = _worldPacket.ReadPackedGuid128();
        }

        public uint VirtualRealmAddress;
        public WowGuid128 Guid;
    }

    public class SetContactNotes : ClientPacket
    {
        public SetContactNotes(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            VirtualRealmAddress = _worldPacket.ReadUInt32();
            Guid = _worldPacket.ReadPackedGuid128();
            Notes = _worldPacket.ReadString(_worldPacket.ReadBits<uint>(10));
        }

        public uint VirtualRealmAddress;
        public WowGuid128 Guid;
        public string Notes;
    }
}
