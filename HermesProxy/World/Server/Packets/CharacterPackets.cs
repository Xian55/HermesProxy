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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Framework.Constants;
using Framework.GameMath;
using Framework.IO;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public sealed class EnumCharacters : ClientPacket
{
    public EnumCharacters(WorldPacket packet) : base(packet) { }

    public override void Read() { }
}

public sealed class EnumCharactersResult : ServerPacket
{
    public EnumCharactersResult() : base(Opcode.SMSG_ENUM_CHARACTERS_RESULT) { }

    public override void Write()
    {
        Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
            $"[Trace] EnumCharactersResult.Write: ENTER expansion={ModernVersion.ExpansionVersion} chars={Characters.Count}");
        int envStart = _worldPacket.GetData().Length;

        _worldPacket.WriteBit(Success);
        _worldPacket.WriteBit(IsDeletedCharacters);
        _worldPacket.WriteBit(IsNewPlayerRestrictionSkipped);
        _worldPacket.WriteBit(IsNewPlayerRestricted);
        _worldPacket.WriteBit(IsNewPlayer);

        if (ModernVersion.ExpansionVersion >= 3)
        {
            Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
                "[Trace] EnumCharactersResult.Write: branch=V3_4_3 (WPP layout, 7 bits + 5 UInt32s)");
            // 3.4.3.54261 (WotLK Classic) envelope per WowPacketParser
            // WowPacketParserModule.V3_4_0_45166/Parsers/CharacterHandler.cs:402-460
            // (gated on ClientVersionBuild.V3_4_3_51505, before V3_4_4_59817 additions).
            // 7 bits + 5 UInt32 size fields. Realmless/DontCreateCharacterDisplays/
            // RegionwideCharacters/WarbandGroups were all added in 3.4.4 — they MUST NOT
            // appear in the 3.4.3 wire format or every byte after them is misaligned.
            //_worldPacket.WriteBit(Success);
            //_worldPacket.WriteBit(IsDeletedCharacters);
            //_worldPacket.WriteBit(IsNewPlayerRestrictionSkipped);
            //_worldPacket.WriteBit(IsNewPlayerRestricted);
            //_worldPacket.WriteBit(IsNewPlayer);

            _worldPacket.WriteBit(IsTrialAccountRestricted);
            _worldPacket.WriteBit(DisabledClassesMask.HasValue);
            _worldPacket.WriteUInt32((uint)Characters.Count);
            _worldPacket.WriteInt32(MaxCharacterLevel);
            _worldPacket.WriteUInt32((uint)RaceUnlockData.Count);
            _worldPacket.WriteUInt32((uint)UnlockedConditionalAppearances.Count);
            _worldPacket.WriteUInt32((uint)RaceLimitDisablesCount);

            //_worldPacket.WriteUInt32(0u);

            if (DisabledClassesMask.HasValue)
                _worldPacket.WriteUInt32(DisabledClassesMask.Value);

            foreach (var unlockedConditionalAppearance in UnlockedConditionalAppearances)
                unlockedConditionalAppearance.Write(_worldPacket);

            // RaceLimitDisables loop intentionally absent — count is always 0.

            // Envelope hex dump BEFORE Characters loop — captures the wrapper bits/UInt32s the
            // client reads first. If the wrapper is wrong, every Character entry is misaligned.
            DumpEnvelope(envStart);

            foreach (var charInfo in Characters)
                charInfo.Write(_worldPacket);

            foreach (var raceUnlock in RaceUnlockData)
                raceUnlock.Write(_worldPacket);

            Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
                $"[Trace] EnumCharactersResult.Write: EXIT total={_worldPacket.GetData().Length}b (V3_4_3 path)");

            return;
        }

        Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
            "[Trace] EnumCharactersResult.Write: branch=Legacy (V1_14/V2_5 layout)");
        // Legacy modern (V1_14, V2_5) envelope.
        //_worldPacket.WriteBit(Success);
        //_worldPacket.WriteBit(IsDeletedCharacters);
        //_worldPacket.WriteBit(IsNewPlayerRestrictionSkipped);
        //_worldPacket.WriteBit(IsNewPlayerRestricted);
        //_worldPacket.WriteBit(IsNewPlayer);

        _worldPacket.WriteBit(DisabledClassesMask.HasValue);
        _worldPacket.WriteBit(IsAlliedRacesCreationAllowed);
        _worldPacket.WriteInt32(Characters.Count);
        _worldPacket.WriteInt32(MaxCharacterLevel);
        _worldPacket.WriteInt32(RaceUnlockData.Count);
        _worldPacket.WriteInt32(UnlockedConditionalAppearances.Count);

        if (DisabledClassesMask.HasValue)
            _worldPacket.WriteUInt32(DisabledClassesMask.Value);

        foreach (var unlockedConditionalAppearance in UnlockedConditionalAppearances)
            unlockedConditionalAppearance.Write(_worldPacket);

        foreach (var charInfo in Characters)
            charInfo.Write(_worldPacket);

        foreach (var raceUnlock in RaceUnlockData)
            raceUnlock.Write(_worldPacket);
    }

    private void DumpEnvelope(int start)
    {
        byte[] all = _worldPacket.GetData();
        int len = all.Length - start;
        int dumpLen = Math.Min(40, len);
        string hex = BitConverter.ToString(all, start, dumpLen);
        string customSummary = Characters.Count > 0 && Characters[0].Customizations.Count > 0
            ? string.Join(",", Characters[0].Customizations.Select(c => $"{c.ChrCustomizationOptionID}/{c.ChrCustomizationChoiceID}"))
            : "(none)";
        Framework.Logging.Log.Print(Framework.Logging.LogType.Network,
            $"[CharEnumEnv] charsCount={Characters.Count} maxLevel={MaxCharacterLevel} raceCount={RaceUnlockData.Count} envBytes={len} envFirst40={hex}");
        Framework.Logging.Log.Print(Framework.Logging.LogType.Network,
            $"[CharEnumEnv] customizations[0]={customSummary}");
    }

    public bool Success;
    public bool Realmless;                      // 3.4.3+ — set when the result spans realms (we never do)
    public bool IsDeletedCharacters; // used for character undelete list
    public bool IsNewPlayerRestrictionSkipped; // allows client to skip new player restrictions
    public bool IsNewPlayerRestricted; // forbids using level boost and class trials
    public bool IsNewPlayer; // forbids hero classes and allied races
    public bool IsTrialAccountRestricted;       // 3.4.3+
    public bool IsAlliedRacesCreationAllowed;
    public bool DontCreateCharacterDisplays;    // 3.4.3+

    public int MaxCharacterLevel = 1;
    public uint? DisabledClassesMask = new();

    // 3.4.3+ envelope size fields. We never populate these; they exist purely to satisfy
    // the modern client's expected packet length. All counts will be zero on the wire.
    public int RegionwideCharactersCount;
    public int RaceLimitDisablesCount;
    public int WarbandGroupsCount;

    public List<CharacterInfo> Characters = new(); // all characters on the list
    public List<RaceUnlock> RaceUnlockData = new(); //
    public List<UnlockedConditionalAppearance> UnlockedConditionalAppearances = new();

    public class CharacterInfo
    {
        public void Write(WorldPacket data)
        {
            int startSize = data.GetData().Length;

            if (ModernVersion.ExpansionVersion >= 3)
            {
                Write_V3_4_3(data);
            }
            else
            {
                WriteLegacyModern(data);
            }

            // Phase 5a diagnostic: hex-dump the per-character block so we can compare against
            // a known-good 3.4.3 capture. Drop after character-select renders correctly.
            byte[] all = data.GetData();
            int totalSize = all.Length - startSize;
            int firstLen = Math.Min(40, totalSize);
            int lastLen = Math.Min(30, totalSize);
            string firstHex = BitConverter.ToString(all, startSize, firstLen);
            string lastHex = BitConverter.ToString(all, startSize + totalSize - lastLen, lastLen);
            Framework.Logging.Log.Print(Framework.Logging.LogType.Network,
                $"[CharInfo] name={Name} race={RaceId} class={ClassId} level={ExperienceLevel} " +
                $"visItems={VisualItems.Length} bytes={totalSize}");
            Framework.Logging.Log.Print(Framework.Logging.LogType.Network,
                $"[CharInfo] first40={firstHex}");
            Framework.Logging.Log.Print(Framework.Logging.LogType.Network,
                $"[CharInfo] last30={lastHex}");
        }

        // 3.4.3.54261 (WotLK Classic) per-character body per WowPacketParser
        // WowPacketParserModule.V3_4_0_45166/Parsers/CharacterHandler.cs:45-117
        // (the 3.4.3 layout, before V3_4_4_59817 added VirtualRealmAddress, PersonalTabard,
        // TimerunningSeasonID, separate RestrictionsAndMails struct, etc.).
        private void Write_V3_4_3(WorldPacket data)
        {
            Framework.Logging.Log.Print(Framework.Logging.LogType.Trace,
                $"[Trace] CharacterInfo.Write_V3_4_3: ENTER name='{Name}' guid={Guid} race={RaceId} class={ClassId} sex={SexId} " +
                $"flags=0x{(uint)Flags:X8} flags2=0x{Flags2:X8} flags3=0x{Flags3:X8} flags4=0x{Flags4:X8}");
            data.WritePackedGuid128(Guid);
            data.WriteUInt64(GuildClubMemberID);
            data.WriteUInt8(ListPosition);
            data.WriteUInt8((byte)RaceId);
            data.WriteUInt8((byte)ClassId);
            data.WriteUInt8((byte)SexId);
            data.WriteUInt32((uint)Customizations.Count);
            data.WriteUInt8(ExperienceLevel);
            data.WriteInt32((int)ZoneId);
            data.WriteInt32((int)MapId);
            data.WriteVector3(PreloadPos);
            data.WritePackedGuid128(GuildGuid);
            data.WriteUInt32((uint)Flags);
            data.WriteUInt32(Flags2);
            data.WriteUInt32(Flags3);
            data.WriteUInt32(PetCreatureDisplayId);
            data.WriteUInt32(PetExperienceLevel);
            data.WriteUInt32(PetCreatureFamilyId);

            data.WriteInt32((int)ProfessionIds[0]);
            data.WriteInt32((int)ProfessionIds[1]);

            // 34 visual items × 14 bytes each = 476 bytes. Pad missing slots with default.
            const int VisualItemCount_V3_4_3 = 34;
            for (int vi = 0; vi < VisualItemCount_V3_4_3; vi++)
            {
                if (vi < VisualItems.Length)
                    VisualItems[vi].Write(data);
                else
                    default(VisualItemInfo).Write(data);
            }

            data.WriteUInt64(LastPlayedTime);
            data.WriteInt16((short)SpecID);
            data.WriteInt32((int)Unknown703);          // SaveVersion in WPP
            data.WriteInt32((int)LastLoginVersion);
            data.WriteUInt32(Flags4);                  // RestrictionFlags in WPP
            data.WriteUInt32((uint)MailSenders.Count);
            data.WriteUInt32((uint)MailSenderTypes.Count);
            data.WriteUInt32(OverrideSelectScreenFileDataID);

            foreach (ChrCustomizationChoice customization in Customizations)
            {
                data.WriteUInt32(customization.ChrCustomizationOptionID);
                data.WriteUInt32(customization.ChrCustomizationChoiceID);
            }

            foreach (var mailSenderType in MailSenderTypes)
                data.WriteUInt32(mailSenderType);

            data.WriteBits(Name.GetByteCount(), 6);
            data.WriteBit(FirstLogin);
            data.WriteBit(BoostInProgress);
            data.WriteBits(unkWod61x, 5);              // CantLoginReason in WPP
            data.WriteBits(0, 2);                       // Unk
            data.WriteBit(false);                       // RpeResetAvailable
            data.WriteBit(false);                       // RpeResetQuestClearAvailable

            foreach (string str in MailSenders)
                data.WriteBits(str.GetByteCount() + 1, 6);

            data.FlushBits();

            foreach (string str in MailSenders)
                if (!str.IsEmpty())
                    data.WriteCString(str);

            data.WriteString(Name);
        }

        // Legacy modern (V1_14, V2_5) per-character body — preserves the prior layout
        // exactly, so older modern clients keep working.
        private void WriteLegacyModern(WorldPacket data)
        {
            data.WritePackedGuid128(Guid);
            data.WriteUInt64(GuildClubMemberID);
            data.WriteUInt8(ListPosition);
            data.WriteUInt8((byte)RaceId);
            data.WriteUInt8((byte)ClassId);
            data.WriteUInt8((byte)SexId);
            data.WriteInt32(Customizations.Count);

            data.WriteUInt8(ExperienceLevel);
            data.WriteUInt32(ZoneId);
            data.WriteUInt32(MapId);
            data.WriteVector3(PreloadPos);
            data.WritePackedGuid128(GuildGuid);
            data.WriteUInt32((uint)Flags);
            data.WriteUInt32(Flags2);
            data.WriteUInt32(Flags3);
            data.WriteUInt32(PetCreatureDisplayId);
            data.WriteUInt32(PetExperienceLevel);
            data.WriteUInt32(PetCreatureFamilyId);

            data.WriteUInt32(ProfessionIds[0]);
            data.WriteUInt32(ProfessionIds[1]);

            foreach (var visualItem in VisualItems)
                visualItem.Write(data);

            data.WriteUInt64(LastPlayedTime);
            data.WriteUInt16(SpecID);
            data.WriteUInt32(Unknown703);
            data.WriteUInt32(LastLoginVersion);
            data.WriteUInt32(Flags4);
            data.WriteInt32(MailSenders.Count);
            data.WriteInt32(MailSenderTypes.Count);
            data.WriteUInt32(OverrideSelectScreenFileDataID);

            foreach (ChrCustomizationChoice customization in Customizations)
            {
                data.WriteUInt32(customization.ChrCustomizationOptionID);
                data.WriteUInt32(customization.ChrCustomizationChoiceID);
            }

            foreach (var mailSenderType in MailSenderTypes)
                data.WriteUInt32(mailSenderType);

            data.WriteBits(Name.GetByteCount(), 6);
            data.WriteBit(FirstLogin);
            data.WriteBit(BoostInProgress);
            data.WriteBits(unkWod61x, 5);
            data.WriteBit(false);
            data.WriteBit(ExpansionChosen);

            foreach (string str in MailSenders)
                data.WriteBits(str.GetByteCount() + 1, 6);

            data.FlushBits();

            foreach (string str in MailSenders)
                if (!str.IsEmpty())
                    data.WriteCString(str);

            data.WriteString(Name);
        }

        public WowGuid128 Guid;
        public uint VirtualRealmAddress;
        public ulong GuildClubMemberID; // same as bgs.protocol.club.v1.MemberId.unique_id, guessed basing on SMSG_QUERY_PLAYER_NAME_RESPONSE (that one is known)
        public string Name = string.Empty;
        public byte ListPosition; // Order of the characters in list
        public Race RaceId;
        public Class ClassId;
        public Gender SexId;
        public List<ChrCustomizationChoice> Customizations = new();
        public byte ExperienceLevel;
        public uint ZoneId;
        public uint MapId;
        public Vector3 PreloadPos;
        public WowGuid128 GuildGuid;
        public CharacterFlags Flags; // Character flag @see enum CharacterFlags
        public uint Flags2;
        public uint Flags3;
        public uint Flags4;
        public bool FirstLogin;
        public byte unkWod61x;
        public bool ExpansionChosen;
        public ulong LastPlayedTime;
        public ushort SpecID;
        public uint Unknown703;
        public uint LastLoginVersion;
        public uint OverrideSelectScreenFileDataID;
        public int TimerunningSeasonID;
        public uint PetCreatureDisplayId;
        public uint PetExperienceLevel;
        public uint PetCreatureFamilyId;
        public bool BoostInProgress; // @todo
        public uint[] ProfessionIds = new uint[2];      // @todo
        public VisualItemInfo[] VisualItems = new VisualItemInfo[Enums.Classic.InventorySlots.BagEnd];
        public CustomTabardInfo PersonalTabard = CustomTabardInfo.Default;
        public List<string> MailSenders = new();
        public List<uint> MailSenderTypes = new();

        public struct VisualItemInfo
        {
            // 14-byte layout used by V1_14, V2_5, and 3.4.3.54261. Per WPP V3_4_0_45166's
            // ReadVisualItemInfo, 3.4.3 still uses the older format. Only V3_4_4_59817+ moved
            // to the 24-byte layout (ItemID + TransmogrifiedItemID added).
            public void Write(WorldPacket data)
            {
                data.WriteUInt32(DisplayId);
                data.WriteUInt32(DisplayEnchantId);
                data.WriteUInt32(SecondaryItemModifiedAppearanceID);
                data.WriteUInt8(InvType);
                data.WriteUInt8(Subclass);
            }

            public uint DisplayId;
            public uint DisplayEnchantId;
            public uint SecondaryItemModifiedAppearanceID; // also -1 is some special value
            public byte InvType;
            public byte Subclass;
        }

        // 3.4.3+ adds a per-character "personal tabard" customization. Defaults to all -1
        // (= "no tabard set"); the default factory supplies that without forcing every call
        // site to remember the magic value.
        public struct CustomTabardInfo
        {
            public static CustomTabardInfo Default => new()
            {
                EmblemStyle = -1,
                EmblemColor = -1,
                BorderStyle = -1,
                BorderColor = -1,
                BackgroundColor = -1,
            };

            public int EmblemStyle;
            public int EmblemColor;
            public int BorderStyle;
            public int BorderColor;
            public int BackgroundColor;

            public void Write(WorldPacket data)
            {
                data.WriteInt32(EmblemStyle);
                data.WriteInt32(EmblemColor);
                data.WriteInt32(BorderStyle);
                data.WriteInt32(BorderColor);
                data.WriteInt32(BackgroundColor);
            }
        }

        public struct PetInfo
        {
            public uint CreatureDisplayId; // PetCreatureDisplayID
            public uint Level; // PetExperienceLevel
            public uint CreatureFamily; // PetCreatureFamilyID
        }
    }

    public struct RaceUnlock
    {
        public RaceUnlock(int raceId, bool hasExpansion, bool hasAchievement, bool hasHeritageArmor)
        {
            RaceID = raceId;
            HasExpansion = hasExpansion;
            HasAchievement = hasAchievement;
            HasHeritageArmor = hasHeritageArmor;
        }
        public void Write(WorldPacket data)
        {
            data.WriteInt32(RaceID);
            data.WriteBit(HasExpansion);
            data.WriteBit(HasAchievement);
            data.WriteBit(HasHeritageArmor);
            // 3.4.3+ added IsLocked and Unused1027. Write them unconditionally — older modern
            // clients ignore the upper bits inside the byte that FlushBits aligns to.
            data.WriteBit(IsLocked);
            data.WriteBit(Unused1027);
            data.FlushBits();
        }

        public int RaceID;
        public bool HasExpansion;
        public bool HasAchievement;
        public bool HasHeritageArmor;
        public bool IsLocked;
        public bool Unused1027;
    }

    public struct UnlockedConditionalAppearance
    {
        public void Write(WorldPacket data)
        {
            data.WriteInt32(AchievementID);
            data.WriteInt32(Unused);
        }

        public int AchievementID;
        public int Unused;
    }
}

public class AccountCharacterListEntry
{
    public void Write(WorldPacket packet)
    {
        packet.WritePackedGuid128(AccountId);
        packet.WritePackedGuid128(CharacterGuid);
        packet.WriteUInt32(RealmVirtualAddress);
        packet.WriteUInt8((byte)Race);
        packet.WriteUInt8((byte)Class);
        packet.WriteUInt8((byte)Sex);
        packet.WriteUInt8(Level);

        packet.WriteUInt64(LastLoginUnixSec);

        if (ModernVersion.AddedInClassicVersion(1, 14, 1, 2, 5, 3))
            packet.WriteUInt32(Unk);

        packet.ResetBitPos();
        packet.WriteBits(Name.GetByteCount(), 6);
        packet.WriteBits(RealmName.GetByteCount(), 9);

        packet.WriteString(Name);
        packet.WriteString(RealmName);
    }

    public WowGuid128 AccountId;

    public uint RealmVirtualAddress;
    public string RealmName = string.Empty;

    public WowGuid128 CharacterGuid;
    public string Name = string.Empty;
    public Race Race;
    public Class Class;
    public Gender Sex;
    public byte Level;
    public ulong LastLoginUnixSec;
    public uint Unk;
}

public class GetAccountCharacterListRequest : ClientPacket
{
    public GetAccountCharacterListRequest(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Token = _worldPacket.ReadUInt32();
    }

    public uint Token = 0;
}

public class GetAccountCharacterListResult : ServerPacket
{
    public GetAccountCharacterListResult() : base(Opcode.SMSG_GET_ACCOUNT_CHARACTER_LIST_RESULT)
    {
    }

    public override void Write()
    {
        _worldPacket.WriteUInt32(Token);
        _worldPacket.WriteUInt32((uint)CharacterList.Count);

        _worldPacket.ResetBitPos();
        _worldPacket.WriteBit(false); // unknown bit

        foreach (var entry in CharacterList)
            entry.Write(_worldPacket);
    }

    public uint Token = 0;
    public List<AccountCharacterListEntry> CharacterList = new();
}

public class GenerateRandomCharacterNameRequest : ClientPacket
{
    public GenerateRandomCharacterNameRequest(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Race = (Race)_worldPacket.ReadUInt8();
        Sex = (Gender)_worldPacket.ReadUInt8();
    }

    public Race Race;
    public Gender Sex;
}

public class GenerateRandomCharacterNameResult : ServerPacket, ISpanWritable
{
    public GenerateRandomCharacterNameResult() : base(Opcode.SMSG_GENERATE_RANDOM_CHARACTER_NAME_RESULT) { }

    public override void Write()
    {
        _worldPacket.WriteBool(Success);

        _worldPacket.WriteBits(Name.Length, 6);
        _worldPacket.WriteString(Name);
    }

    // MaxSize: bool (1) + 6 bits (1) + player name (24) = 26
    public int MaxSize => 1 + 1 + GameLimits.MaxPlayerNameBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteBool(Success);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(Name), 6);
        writer.WriteString(Name);
        return writer.Position;
    }

    public bool Success;
    public string Name = "";
}

public class ChrCustomizationChoice : IComparable<ChrCustomizationChoice>
{
    public uint ChrCustomizationOptionID;
    public uint ChrCustomizationChoiceID;

    public ChrCustomizationChoice(uint optionId, uint chocieId)
    {
        ChrCustomizationOptionID = optionId;
        ChrCustomizationChoiceID = chocieId;
    }

    public void WriteCreate(WorldPacket data)
    {
        data.WriteUInt32(ChrCustomizationOptionID);
        data.WriteUInt32(ChrCustomizationChoiceID);
    }

    public void WriteUpdate(WorldPacket data)
    {
        data.WriteUInt32(ChrCustomizationOptionID);
        data.WriteUInt32(ChrCustomizationChoiceID);
    }

    public int CompareTo(ChrCustomizationChoice? other)
    {
        if (other is null) return 1;
        return ChrCustomizationOptionID.CompareTo(other.ChrCustomizationOptionID);
    }
}

public class CreateCharacter : ClientPacket
{
    public CreateCharacter(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        CreateInfo = new CharacterCreateInfo();
        uint nameLength = _worldPacket.ReadBits<uint>(6);
        bool hasTemplateSet = _worldPacket.HasBit();
        CreateInfo.IsTrialBoost = _worldPacket.HasBit();
        CreateInfo.UseNPE = _worldPacket.HasBit();

        CreateInfo.RaceId = (Race)_worldPacket.ReadUInt8();
        CreateInfo.ClassId = (Class)_worldPacket.ReadUInt8();
        CreateInfo.Sex = (Gender)_worldPacket.ReadUInt8();
        var customizationCount = _worldPacket.ReadUInt32();

        CreateInfo.Name = _worldPacket.ReadString(nameLength);
        if (hasTemplateSet)
            CreateInfo.TemplateSet = _worldPacket.ReadUInt32();

        for (var i = 0; i < customizationCount; ++i)
        {
            CreateInfo.Customizations.Add(new ChrCustomizationChoice(_worldPacket.ReadUInt32(), _worldPacket.ReadUInt32()));
        }

        CreateInfo.Customizations.Sort();
    }

    public CharacterCreateInfo CreateInfo = null!;
}

public class CharacterCreateInfo
{
    // User specified variables
    public Race RaceId = Race.None;
    public Class ClassId = Class.None;
    public Gender Sex = Gender.None;
    public List<ChrCustomizationChoice> Customizations = new(50);
    public uint? TemplateSet;
    public bool IsTrialBoost;
    public bool UseNPE;
    public string Name = string.Empty;

    // Server side data
    public byte CharCount = 0;
}

public class CreateChar : ServerPacket, ISpanWritable
{
    public CreateChar() : base(Opcode.SMSG_CREATE_CHAR) { }

    public override void Write()
    {
        _worldPacket.WriteUInt8(Code);
        _worldPacket.WritePackedGuid128(Guid);
    }

    public int MaxSize => 1 + PackedGuidHelper.MaxPackedGuid128Size; // byte + GUID

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt8(Code);
        writer.WritePackedGuid128(Guid.Low, Guid.High);
        return writer.Position;
    }

    public byte Code;
    public WowGuid128 Guid;
}

public class CharDelete : ClientPacket
{
    public CharDelete(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Guid = _worldPacket.ReadPackedGuid128();
    }

    public WowGuid128 Guid; // Guid of the character to delete
}

public class DeleteChar : ServerPacket, ISpanWritable
{
    public DeleteChar() : base(Opcode.SMSG_DELETE_CHAR) { }

    public override void Write()
    {
        _worldPacket.WriteUInt8(Code);
    }

    public int MaxSize => 1; // byte

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt8(Code);
        return writer.Position;
    }

    public byte Code;
}

public class LoadingScreenNotify : ClientPacket
{
    public LoadingScreenNotify(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        MapID = _worldPacket.ReadUInt32();
        Showing = _worldPacket.HasBit();
    }

    public uint MapID;
    public bool Showing;
}

public class PlayerLogin : ClientPacket
{
    public PlayerLogin(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Guid = _worldPacket.ReadPackedGuid128();
        FarClip = _worldPacket.ReadFloat();
        // 3.4.3 client doesn't send the trailing bit — packet is exactly Guid+FarClip.
        // Per WPP V3_4_0_45166 SessionHandler.cs:143, gated on V3_4_3_51505+.
        if (ModernVersion.ExpansionVersion < 3)
            UnkBit = _worldPacket.HasBit();
    }

    public WowGuid128 Guid;      // Guid of the player that is logging in
    public float FarClip;        // Visibility distance (for terrain)
    public bool UnkBit;
}

public class LoginVerifyWorld : ServerPacket, ISpanWritable
{
    public LoginVerifyWorld() : base(Opcode.SMSG_LOGIN_VERIFY_WORLD, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(MapID);
        _worldPacket.WriteFloat(Pos.X);
        _worldPacket.WriteFloat(Pos.Y);
        _worldPacket.WriteFloat(Pos.Z);
        _worldPacket.WriteFloat(Pos.Orientation);
        _worldPacket.WriteUInt32(Reason);
    }

    public int MaxSize => 24; // uint + 4 floats + uint

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(MapID);
        writer.WriteFloat(Pos.X);
        writer.WriteFloat(Pos.Y);
        writer.WriteFloat(Pos.Z);
        writer.WriteFloat(Pos.Orientation);
        writer.WriteUInt32(Reason);
        return writer.Position;
    }

    public uint MapID;
    public Position Pos;
    public uint Reason;
}

public class CharacterLoginFailed : ServerPacket, ISpanWritable
{
    public CharacterLoginFailed() : base(Opcode.SMSG_CHARACTER_LOGIN_FAILED) { }

    public override void Write()
    {
        _worldPacket.WriteUInt8((byte)Code);
    }

    public int MaxSize => 1; // byte

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt8((byte)Code);
        return writer.Position;
    }

    public LoginFailureReason Code;
}

public class LogoutRequest : ClientPacket
{
    public LogoutRequest(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        IdleLogout = _worldPacket.HasBit();
    }

    public bool IdleLogout;
}

public class LogoutResponse : ServerPacket, ISpanWritable
{
    public LogoutResponse() : base(Opcode.SMSG_LOGOUT_RESPONSE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(LogoutResult);
        _worldPacket.WriteBit(Instant);
        _worldPacket.FlushBits();
    }

    public int MaxSize => 5; // int + bit

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt32(LogoutResult);
        writer.WriteBit(Instant);
        writer.FlushBits();
        return writer.Position;
    }

    public int LogoutResult;
    public bool Instant = false;
}

public class LogoutComplete : ServerPacket, ISpanWritable
{
    public LogoutComplete() : base(Opcode.SMSG_LOGOUT_COMPLETE) { }

    public override void Write() { }

    public int MaxSize => 0;

    public int WriteToSpan(Span<byte> buffer) => 0;
}

public class LogoutCancel : ClientPacket
{
    public LogoutCancel(WorldPacket packet) : base(packet) { }

    public override void Read() { }
}

public class LogoutCancelAck : ServerPacket, ISpanWritable
{
    public LogoutCancelAck() : base(Opcode.SMSG_LOGOUT_CANCEL_ACK, ConnectionType.Instance) { }

    public override void Write() { }

    public int MaxSize => 0;

    public int WriteToSpan(Span<byte> buffer) => 0;
}

class LogXPGain : ServerPacket, ISpanWritable
{
    public LogXPGain() : base(Opcode.SMSG_LOG_XP_GAIN) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(Victim);
        _worldPacket.WriteInt32(Original);
        _worldPacket.WriteUInt8((byte)Reason);
        _worldPacket.WriteInt32(Amount);
        _worldPacket.WriteFloat(GroupBonus);
        _worldPacket.WriteUInt8(RAFBonus);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 14; // GUID + int + byte + int + float + byte

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(Victim.Low, Victim.High);
        writer.WriteInt32(Original);
        writer.WriteUInt8((byte)Reason);
        writer.WriteInt32(Amount);
        writer.WriteFloat(GroupBonus);
        writer.WriteUInt8(RAFBonus);
        return writer.Position;
    }

    public WowGuid128 Victim;
    public int Original;
    public PlayerLogXPReason Reason;
    public int Amount;
    public float GroupBonus = 1;
    public byte RAFBonus; // 1 - 300% of normal XP; 2 - 150% of normal XP
}

public class RequestPlayedTime : ClientPacket
{
    public RequestPlayedTime(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        TriggerScriptEvent = _worldPacket.HasBit();
    }

    public bool TriggerScriptEvent;
}

public class PlayedTime : ServerPacket, ISpanWritable
{
    public PlayedTime() : base(Opcode.SMSG_PLAYED_TIME, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(TotalTime);
        _worldPacket.WriteUInt32(LevelTime);
        _worldPacket.WriteBit(TriggerEvent);
        _worldPacket.FlushBits();
    }

    public int MaxSize => 9; // uint + uint + bit

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(TotalTime);
        writer.WriteUInt32(LevelTime);
        writer.WriteBit(TriggerEvent);
        writer.FlushBits();
        return writer.Position;
    }

    public uint TotalTime;
    public uint LevelTime;
    public bool TriggerEvent;
}

public class SetTitle : ClientPacket
{
    public int TitleID;

    public SetTitle(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        TitleID = _worldPacket.ReadInt32();
    }
}

class TogglePvP : ClientPacket
{
    public TogglePvP(WorldPacket packet) : base(packet) { }

    public override void Read() { }
}

class SetPvP : ClientPacket
{
    public SetPvP(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Enable = _worldPacket.HasBit();
    }

    public bool Enable;
}

public class SetActionButton : ClientPacket
{
    public SetActionButton(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Action = _worldPacket.ReadUInt16();
        Type = _worldPacket.ReadUInt16();
        Index = _worldPacket.ReadUInt8();
    }

    public ushort Action;
    public ushort Type;
    public byte Index;
}

public class UpdateActionButtons : ServerPacket
{
    // V3_4_3 layout: PlayerConst.MaxActionButtonsModern (180) × int64
    // (packed action+type) + 1 byte Reason. Total 1441 bytes.
    // Reference: TC reference packet #151 (1441b = 180*8 + 1), HermesProxy-WOTLK
    // fork Server/Packets/UpdateActionButtons.cs.
    //
    // The packed 64-bit format (per WPP V3_4_0_45166 ActionBarHandler.cs:25-36):
    //   low 56 bits = action ID
    //   high  8 bits = ActionButtonType
    //
    // Legacy 3.3.5a sends a packed int32 where:
    //   low 24 bits = action ID
    //   high  8 bits = ActionButtonType
    //
    // Forwarding the int32 as int64 directly puts the legacy type byte at bits
    // 24-31 (garbage middle of the action value) and leaves the V3_4_3 type byte
    // (bits 56-63) at zero — every slot is read as "type=0" with a mangled
    // action ID, so the client renders the entire bar as empty on every login.
    // The fix unpacks the legacy 32-bit value and repacks for V3_4_3.
    public List<int> ActionButtons = new();
    public byte Reason;

    public UpdateActionButtons() : base(Opcode.SMSG_UPDATE_ACTION_BUTTONS, ConnectionType.Instance) { }

    public override void Write()
    {
        for (int i = 0; i < PlayerConst.MaxActionButtonsModern; i++)
        {
            int legacy = i < ActionButtons.Count ? ActionButtons[i] : 0;
            ulong action = (uint)legacy & 0x00FFFFFFu;
            byte type = (byte)(((uint)legacy >> 24) & 0xFFu);
            ulong packed = action | ((ulong)type << 56);
            _worldPacket.WriteInt64((long)packed);
        }
        _worldPacket.WriteUInt8(Reason);
    }
}

public class SetActionBarToggles : ClientPacket
{
    public SetActionBarToggles(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Mask = _worldPacket.ReadUInt8();
    }

    public byte Mask;
}

public class LevelUpInfo : ServerPacket, ISpanWritable
{
    public LevelUpInfo() : base(Opcode.SMSG_LEVEL_UP_INFO) { }

    // V3_4_3 (WotLK Classic) and later expansions reserve 10 power-delta slots
    // (the legacy 7 plus SoulShards/HolyPower/AlternatePower). Writing only 7
    // shifts every stat that follows by 12 bytes, so the modern client reads
    // garbage past the buffer for Spirit and NumNewTalents (observed: "Spirit
    // increases by 121619977", "1354810122 talent points"). The legacy 3.3.5a
    // server only sends 7; we pad the trailing slots with zero, matching the
    // HermesProxy-WOTLK fork's LevelUpInfo writer.
    private static int GetWirePowerCount() =>
        ModernVersion.ExpansionVersion >= 3 ? 10 : ModernVersion.GetPowerCountForClientVersion();

    public override void Write()
    {
        _worldPacket.WriteInt32(Level);
        _worldPacket.WriteInt32(HealthDelta);

        int powerCount = GetWirePowerCount();
        for (int i = 0; i < powerCount; i++)
            _worldPacket.WriteInt32(i < PowerDelta.Length ? PowerDelta[i] : 0);

        foreach (int stat in StatDelta)
            _worldPacket.WriteInt32(stat);

        _worldPacket.WriteInt32(NumNewTalents);
        _worldPacket.WriteInt32(NumNewPvpTalentSlots);
    }

    // Level(4) + Health(4) + 10 powers(40) + 5 stats(20) + 2 ints(8) = 76 bytes
    public int MaxSize => 4 + 4 + 10 * 4 + 5 * 4 + 4 + 4;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt32(Level);
        writer.WriteInt32(HealthDelta);

        int powerCount = GetWirePowerCount();
        for (int i = 0; i < powerCount; i++)
            writer.WriteInt32(i < PowerDelta.Length ? PowerDelta[i] : 0);

        foreach (int stat in StatDelta)
            writer.WriteInt32(stat);

        writer.WriteInt32(NumNewTalents);
        writer.WriteInt32(NumNewPvpTalentSlots);
        return writer.Position;
    }

    public int Level = 0;
    public int HealthDelta = 0;
    public int[] PowerDelta = new int[10];
    public int[] StatDelta = new int[5];
    public int NumNewTalents;
    public int NumNewPvpTalentSlots;
}

class UnlearnSkill : ClientPacket
{
    public UnlearnSkill(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        SkillLine = _worldPacket.ReadUInt32();
    }

    public uint SkillLine;
}

class PlayerShowingHelmOrCloak : ClientPacket
{
    public PlayerShowingHelmOrCloak(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        _worldPacket.ResetBitPos();
        Showing = _worldPacket.HasBit();
    }

    public bool Showing;
}

public class Inspect : ClientPacket
{
    public Inspect(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Target = _worldPacket.ReadPackedGuid128();
    }

    public WowGuid128 Target;
}

public class InspectResult : ServerPacket
{
    public InspectResult() : base(Opcode.SMSG_INSPECT_RESULT) { }

    public override void Write()
    {
        DisplayInfo.Write(_worldPacket);
        _worldPacket.WriteInt32(Glyphs.Count);
        _worldPacket.WriteInt32(Talents.Count);
        _worldPacket.WriteInt32(ItemLevel);
        _worldPacket.WriteUInt8(LifetimeMaxRank);
        _worldPacket.WriteUInt16(TodayHK);
        _worldPacket.WriteUInt16(YesterdayHK);
        _worldPacket.WriteUInt32(LifetimeHK);
        _worldPacket.WriteUInt32(HonorLevel);

        for (int i = 0; i < Glyphs.Count; ++i)
            _worldPacket.WriteUInt16(Glyphs[i]);

        for (int i = 0; i < Talents.Count; ++i)
            _worldPacket.WriteUInt8(Talents[i]);

        _worldPacket.WriteBit(GuildData != null);
        _worldPacket.WriteBit(AzeriteLevel.HasValue);
        _worldPacket.FlushBits();

        foreach (PVPBracketData bracket in Bracket)
            bracket.Write(_worldPacket);

        if (GuildData != null)
            GuildData.Write(_worldPacket);

        if (AzeriteLevel.HasValue)
            _worldPacket.WriteUInt32((uint)AzeriteLevel);
    }

    public PlayerModelDisplayInfo DisplayInfo = new();
    public List<ushort> Glyphs = new();
    public List<byte> Talents = new();
    public InspectGuildData GuildData = null!;
    public PVPBracketData[] Bracket = new PVPBracketData[6];
    public uint? AzeriteLevel;
    public int ItemLevel;
    public uint LifetimeHK;
    public uint HonorLevel = 1;
    public ushort TodayHK;
    public ushort YesterdayHK;
    public byte LifetimeMaxRank;
}

public class PlayerModelDisplayInfo
{
    public void Write(WorldPacket data)
    {
        data.WritePackedGuid128(GUID);
        data.WriteUInt32(SpecializationID);
        data.WriteInt32(Items.Count);
        data.WriteBits(Name.GetByteCount(), 6);
        data.WriteUInt8((byte)SexId);
        data.WriteUInt8((byte)RaceId);
        data.WriteUInt8((byte)ClassId);
        data.WriteInt32(Customizations.Count);
        data.WriteString(Name);

        foreach (var customization in Customizations)
        {
            data.WriteUInt32(customization.ChrCustomizationOptionID);
            data.WriteUInt32(customization.ChrCustomizationChoiceID);
        }

        foreach (InspectItemData item in Items)
            item.Write(data);
    }

    public WowGuid128 GUID;
    public List<InspectItemData> Items = new();
    public string Name = string.Empty;
    public uint SpecializationID;
    public Gender SexId;
    public Race RaceId;
    public Class ClassId;
    public List<ChrCustomizationChoice> Customizations = new();

}

public class InspectItemData
{
    public void Write(WorldPacket data)
    {
        data.WritePackedGuid128(CreatorGUID);
        data.WriteUInt8(Index);
        data.WriteInt32(AzeritePowers.Count);
        data.WriteInt32(AzeriteEssences.Count);
        foreach (var id in AzeritePowers)
            data.WriteInt32(id);

        Item.Write(data);
        data.WriteBit(Usable);
        data.WriteBits(Enchants.Count, 4);
        data.WriteBits(Gems.Count, 2);
        data.FlushBits();

        foreach (var azeriteEssenceData in AzeriteEssences)
            azeriteEssenceData.Write(data);

        foreach (var enchantData in Enchants)
            enchantData.Write(data);

        foreach (var gem in Gems)
            gem.Write(data);
    }

    public WowGuid128 CreatorGUID = WowGuid128.Empty;
    public ItemInstance Item = new();
    public byte Index;
    public bool Usable;
    public List<InspectEnchantData> Enchants = new();
    public List<ItemGemData> Gems = new();
    public List<int> AzeritePowers = new();
    public List<AzeriteEssenceData> AzeriteEssences = new();
}

public struct InspectEnchantData
{
    public InspectEnchantData(uint id, byte index)
    {
        Id = id;
        Index = index;
    }

    public void Write(WorldPacket data)
    {
        data.WriteUInt32(Id);
        data.WriteUInt8(Index);
    }

    public uint Id;
    public byte Index;
}

public struct AzeriteEssenceData
{
    public uint Index;
    public uint AzeriteEssenceID;
    public uint Rank;
    public bool SlotUnlocked;

    public void Write(WorldPacket data)
    {
        data.WriteUInt32(Index);
        data.WriteUInt32(AzeriteEssenceID);
        data.WriteUInt32(Rank);
        data.WriteBit(SlotUnlocked);
        data.FlushBits();
    }
}

public class InspectGuildData
{
    public void Write(WorldPacket data)
    {
        data.WritePackedGuid128(GuildGUID);
        data.WriteInt32(NumGuildMembers);
        data.WriteInt32(AchievementPoints);
    }

    public WowGuid128 GuildGUID = WowGuid128.Empty;
    public int NumGuildMembers;
    public int AchievementPoints;
}

public struct PVPBracketData
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt8(Bracket);
        data.WriteInt32(Rating);
        data.WriteInt32(Rank);
        data.WriteInt32(WeeklyPlayed);
        data.WriteInt32(WeeklyWon);
        data.WriteInt32(SeasonPlayed);
        data.WriteInt32(SeasonWon);
        data.WriteInt32(WeeklyBestRating);
        data.WriteInt32(SeasonBestRating);
        data.WriteInt32(PvpTierID);
        if (ModernVersion.AddedInVersion(9, 1, 0, 1, 14, 0, 2, 5, 2))
            data.WriteInt32(WeeklyBestWinPvpTierID);
        if (ModernVersion.AddedInVersion(9, 1, 5, 1, 14, 1, 2, 5, 3))
        {
            data.WriteInt32(Unused1);
            data.WriteInt32(Unused2);
        }
        data.WriteBit(Disqualified);
        data.FlushBits();
    }

    public int Rating;
    public int Rank;
    public int WeeklyPlayed;
    public int WeeklyWon;
    public int SeasonPlayed;
    public int SeasonWon;
    public int WeeklyBestRating;
    public int SeasonBestRating;
    public int PvpTierID;
    public int WeeklyBestWinPvpTierID;
    public int Unused1;
    public int Unused2;
    public byte Bracket;
    public bool Disqualified;
}

public class InspectHonorStatsResultClassic : ServerPacket, ISpanWritable
{
    public InspectHonorStatsResultClassic() : base(Opcode.SMSG_INSPECT_HONOR_STATS) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(PlayerGUID);
        _worldPacket.WriteUInt8(LifetimeHighestRank);
        _worldPacket.WriteUInt16(TodayHonorableKills);
        _worldPacket.WriteUInt16(TodayDishonorableKills);
        _worldPacket.WriteUInt16(YesterdayHonorableKills);
        _worldPacket.WriteUInt16(YesterdayDishonorableKills);
        _worldPacket.WriteUInt16(LastWeekHonorableKills);
        _worldPacket.WriteUInt16(LastWeekDishonorableKills);
        _worldPacket.WriteUInt16(ThisWeekHonorableKills);
        _worldPacket.WriteUInt16(ThisWeekDishonorableKills);
        _worldPacket.WriteUInt32(LifetimeHonorableKills);
        _worldPacket.WriteUInt32(LifetimeDishonorableKills);
        _worldPacket.WriteUInt32(YesterdayHonor);
        _worldPacket.WriteUInt32(LastWeekHonor);
        _worldPacket.WriteUInt32(ThisWeekHonor);
        _worldPacket.WriteUInt32(Standing);
        _worldPacket.WriteUInt8(RankProgress);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 42; // GUID + byte + 8 ushorts + 6 uints + byte

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(PlayerGUID.Low, PlayerGUID.High);
        writer.WriteUInt8(LifetimeHighestRank);
        writer.WriteUInt16(TodayHonorableKills);
        writer.WriteUInt16(TodayDishonorableKills);
        writer.WriteUInt16(YesterdayHonorableKills);
        writer.WriteUInt16(YesterdayDishonorableKills);
        writer.WriteUInt16(LastWeekHonorableKills);
        writer.WriteUInt16(LastWeekDishonorableKills);
        writer.WriteUInt16(ThisWeekHonorableKills);
        writer.WriteUInt16(ThisWeekDishonorableKills);
        writer.WriteUInt32(LifetimeHonorableKills);
        writer.WriteUInt32(LifetimeDishonorableKills);
        writer.WriteUInt32(YesterdayHonor);
        writer.WriteUInt32(LastWeekHonor);
        writer.WriteUInt32(ThisWeekHonor);
        writer.WriteUInt32(Standing);
        writer.WriteUInt8(RankProgress);
        return writer.Position;
    }

    public WowGuid128 PlayerGUID;
    public byte LifetimeHighestRank;
    public ushort TodayHonorableKills;
    public ushort TodayDishonorableKills;
    public ushort YesterdayHonorableKills;
    public ushort YesterdayDishonorableKills;
    public ushort LastWeekHonorableKills;
    public ushort LastWeekDishonorableKills;
    public ushort ThisWeekHonorableKills;
    public ushort ThisWeekDishonorableKills;
    public uint LifetimeHonorableKills;
    public uint LifetimeDishonorableKills;
    public uint YesterdayHonor;
    public uint LastWeekHonor;
    public uint ThisWeekHonor;
    public uint Standing;
    public byte RankProgress;
}

public class InspectHonorStatsResultTBC : ServerPacket, ISpanWritable
{
    public InspectHonorStatsResultTBC() : base(Opcode.SMSG_INSPECT_HONOR_STATS) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(PlayerGUID);
        _worldPacket.WriteUInt8(LifetimeHighestRank);
        _worldPacket.WriteUInt16(Unused1);
        _worldPacket.WriteUInt16(YesterdayHonorableKills);
        _worldPacket.WriteUInt16(Unused3);
        _worldPacket.WriteUInt16(LifetimeHonorableKills);
        _worldPacket.WriteUInt32(Unused4);
        _worldPacket.WriteUInt32(Unused5);
        _worldPacket.WriteUInt32(Unused6);
        _worldPacket.WriteUInt32(Unused7);
        _worldPacket.WriteUInt32(Unused8);
        _worldPacket.WriteUInt8(Unused9);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 30; // GUID + byte + 4 ushorts + 5 uints + byte

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(PlayerGUID.Low, PlayerGUID.High);
        writer.WriteUInt8(LifetimeHighestRank);
        writer.WriteUInt16(Unused1);
        writer.WriteUInt16(YesterdayHonorableKills);
        writer.WriteUInt16(Unused3);
        writer.WriteUInt16(LifetimeHonorableKills);
        writer.WriteUInt32(Unused4);
        writer.WriteUInt32(Unused5);
        writer.WriteUInt32(Unused6);
        writer.WriteUInt32(Unused7);
        writer.WriteUInt32(Unused8);
        writer.WriteUInt8(Unused9);
        return writer.Position;
    }

    public WowGuid128 PlayerGUID;
    public byte LifetimeHighestRank;
    public ushort Unused1;
    public ushort YesterdayHonorableKills;
    public ushort Unused3;
    public ushort LifetimeHonorableKills;
    public uint Unused4;
    public uint Unused5;
    public uint Unused6;
    public uint Unused7;
    public uint Unused8;
    public byte Unused9;
}

public class InspectPvP : ServerPacket, ISpanWritable
{
    public InspectPvP() : base(Opcode.SMSG_INSPECT_PVP) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(PlayerGUID);
        _worldPacket.WriteBits(Brackets.Count, 3);
        _worldPacket.WriteBits(ArenaTeams.Count, 2);
        _worldPacket.FlushBits();

        foreach (var bracket in Brackets)
            bracket.Write(_worldPacket);

        foreach (var team in ArenaTeams)
            team.Write(_worldPacket);
    }

    // MaxSize: GUID (18) + bits (5 -> 1 byte) + max 8 brackets (42 bytes each) + max 3 teams (38 bytes each)
    // PvPBracketInspectData: byte(1) + 10 ints(40) + bool(1) = 42
    // ArenaTeamInspectData: GUID(18) + 5 ints(20) = 38
    private const int MaxBrackets = 8;
    private const int MaxArenaTeams = 3;
    private const int BracketSize = 42;
    private const int ArenaTeamSize = PackedGuidHelper.MaxPackedGuid128Size + 20;
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 1 + MaxBrackets * BracketSize + MaxArenaTeams * ArenaTeamSize;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (Brackets.Count > MaxBrackets || ArenaTeams.Count > MaxArenaTeams)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(PlayerGUID.Low, PlayerGUID.High);
        writer.WriteBits((uint)Brackets.Count, 3);
        writer.WriteBits((uint)ArenaTeams.Count, 2);
        writer.FlushBits();

        foreach (var bracket in Brackets)
        {
            writer.WriteUInt8(bracket.Bracket);
            writer.WriteInt32(bracket.Rating);
            writer.WriteInt32(bracket.Rank);
            writer.WriteInt32(bracket.WeeklyPlayed);
            writer.WriteInt32(bracket.WeeklyWon);
            writer.WriteInt32(bracket.SeasonPlayed);
            writer.WriteInt32(bracket.SeasonWon);
            writer.WriteInt32(bracket.WeeklyBestRating);
            writer.WriteInt32(bracket.SeasonBestRating);
            writer.WriteInt32(bracket.PvpTierID);
            writer.WriteInt32(bracket.WeeklyBestWinPvpTierID);
            writer.WriteBool(bracket.Disqualified);
        }

        foreach (var team in ArenaTeams)
        {
            writer.WritePackedGuid128(team.TeamGuid.Low, team.TeamGuid.High);
            writer.WriteInt32(team.TeamRating);
            writer.WriteInt32(team.TeamGamesPlayed);
            writer.WriteInt32(team.TeamGamesWon);
            writer.WriteInt32(team.PersonalGamesPlayed);
            writer.WriteInt32(team.PersonalRating);
        }
        return writer.Position;
    }

    public WowGuid128 PlayerGUID;
    public List<PvPBracketInspectData> Brackets = new List<PvPBracketInspectData>();
    public List<ArenaTeamInspectData> ArenaTeams = new List<ArenaTeamInspectData>();
}

public class PvPBracketInspectData
{
    public void Write(WorldPacket data)
    {
        data.WriteUInt8(Bracket);
        data.WriteInt32(Rating);
        data.WriteInt32(Rank);
        data.WriteInt32(WeeklyPlayed);
        data.WriteInt32(WeeklyWon);
        data.WriteInt32(SeasonPlayed);
        data.WriteInt32(SeasonWon); ;
        data.WriteInt32(WeeklyBestRating);
        data.WriteInt32(SeasonBestRating);
        data.WriteInt32(PvpTierID);
        data.WriteInt32(WeeklyBestWinPvpTierID);
        data.WriteBool(Disqualified);
    }

    public byte Bracket;
    public int Rating;
    public int Rank;
    public int WeeklyPlayed;
    public int WeeklyWon;
    public int SeasonPlayed;
    public int SeasonWon;
    public int WeeklyBestRating;
    public int SeasonBestRating;
    public int PvpTierID;
    public int WeeklyBestWinPvpTierID;
    public bool Disqualified;
}

public class ArenaTeamInspectData
{
    public void Write(WorldPacket data)
    {
        data.WritePackedGuid128(TeamGuid);
        data.WriteInt32(TeamRating);
        data.WriteInt32(TeamGamesPlayed);
        data.WriteInt32(TeamGamesWon);
        data.WriteInt32(PersonalGamesPlayed);
        data.WriteInt32(PersonalRating);
    }

    public WowGuid128 TeamGuid = WowGuid128.Empty;
    public int TeamRating;
    public int TeamGamesPlayed;
    public int TeamGamesWon;
    public int PersonalGamesPlayed;
    public int PersonalRating;
}

public class CharacterRenameRequest : ClientPacket
{
    public CharacterRenameRequest(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Guid = _worldPacket.ReadPackedGuid128();
        NewName = _worldPacket.ReadString(_worldPacket.ReadBits<uint>(6));
    }

    public string NewName = string.Empty;
    public WowGuid128 Guid;
}

public class CharacterRenameResult : ServerPacket, ISpanWritable
{
    public CharacterRenameResult() : base(Opcode.SMSG_CHARACTER_RENAME_RESULT) { }

    public override void Write()
    {
        _worldPacket.WriteUInt8(Result);
        _worldPacket.WriteBit(Guid != default);
        _worldPacket.WriteBits(Name.GetByteCount(), 6);
        _worldPacket.FlushBits();

        if (Guid != default)
            _worldPacket.WritePackedGuid128(Guid);

        _worldPacket.WriteString(Name);
    }

    // MaxSize: byte (1) + bits (1+6=7 -> 1) + optional GUID (18) + name (24) = 44
    public int MaxSize => 1 + 1 + PackedGuidHelper.MaxPackedGuid128Size + GameLimits.MaxPlayerNameBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt8(Result);
        writer.WriteBit(Guid != default);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(Name), 6);
        writer.FlushBits();

        if (Guid != default)
            writer.WritePackedGuid128(Guid.Low, Guid.High);

        writer.WriteString(Name);
        return writer.Position;
    }

    public string Name = "";
    public byte Result = 0;
    public WowGuid128 Guid;
}
