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

using HermesProxy.Enums;
using HermesProxy.World.Enums;
using System;
using System.Text;
using HermesProxy.World.Objects;
using Framework.Constants;
using Framework.Logging;
using System.Collections.Generic;
using Framework.IO;
using Framework.GameMath;

namespace HermesProxy.World.Server.Packets;

public class QueryTimeResponse : ServerPacket, ISpanWritable
{
    public QueryTimeResponse() : base(Opcode.SMSG_QUERY_TIME_RESPONSE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteInt64(CurrentTime);
    }

    public int MaxSize => 8; // int64

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt64(CurrentTime);
        return writer.Position;
    }

    public long CurrentTime;
}

public readonly record struct QueryPetName(WowGuid128 UnitGUID);

class QueryPetNameResponse : ServerPacket, ISpanWritable
{
    public QueryPetNameResponse() : base(Opcode.SMSG_QUERY_PET_NAME_RESPONSE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(UnitGUID);
        _worldPacket.WriteBit(Allow);

        if (Allow)
        {
            _worldPacket.WriteBits(Name.GetByteCount(), 8);
            _worldPacket.WriteBit(HasDeclined);

            for (byte i = 0; i < PlayerConst.MaxDeclinedNameCases; ++i)
                _worldPacket.WriteBits(DeclinedNames.name[i].GetByteCount(), 7);

            for (byte i = 0; i < PlayerConst.MaxDeclinedNameCases; ++i)
                _worldPacket.WriteString(DeclinedNames.name[i]);

            _worldPacket.WriteInt64(Timestamp);
            _worldPacket.WriteString(Name);
        }

        _worldPacket.FlushBits();
    }

    // MaxSize: PackedGuid128 (18) + bits (45 -> 6) + 5 declined names (120) + timestamp (8) + name (24) = 176
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 6 + (PlayerConst.MaxDeclinedNameCases * GameLimits.MaxPetNameBytes) + 8 + GameLimits.MaxPetNameBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(UnitGUID.Low, UnitGUID.High);
        writer.WriteBit(Allow);

        if (Allow)
        {
            writer.WriteBits((uint)Encoding.UTF8.GetByteCount(Name), 8);
            writer.WriteBit(HasDeclined);

            for (byte i = 0; i < PlayerConst.MaxDeclinedNameCases; ++i)
                writer.WriteBits((uint)Encoding.UTF8.GetByteCount(DeclinedNames.name[i]), 7);

            for (byte i = 0; i < PlayerConst.MaxDeclinedNameCases; ++i)
                writer.WriteString(DeclinedNames.name[i]);

            writer.WriteInt64(Timestamp);
            writer.WriteString(Name);
        }

        writer.FlushBits();
        return writer.Position;
    }

    public WowGuid128 UnitGUID;
    public bool Allow;

    public bool HasDeclined;
    public DeclinedName DeclinedNames = new();
    public long Timestamp;
    public string Name = "";
}

public readonly record struct QueryPlayerName(WowGuid128 Player);

public readonly record struct QueryPlayerNames(List<WowGuid128> Players);

public sealed class QueryPlayerNameResponse : ServerPacket, ISpanWritable
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<QueryPlayerNameResponse>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V3_4_3_54261, new SingleLayout()),
        (ClientVersionBuild.V3_4_3_54261, ClientVersionBuild.V4_4_2_60895, new ListLayout(timerunning: false)),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new ListLayout(timerunning: true)));

    private static readonly ServerPacketLayout<QueryPlayerNameResponse> Layout = Layouts.ForRunningClient();

    // V3_4_3 dropped the singular SMSG_QUERY_PLAYER_NAME_RESPONSE opcode and
    // expects everything via SMSG_QUERY_PLAYER_NAMES_RESPONSE (plural, with a
    // Count + array). Per WPP V3_4_0_45166 QueryHandler.cs:517 the per-entry
    // shape is { byte Result, PackedGuid128 Player, bit HasPlayerGuidLookupData,
    // bit HasNameCacheUnused920, FlushBits, optional PlayerGuidLookupData }.
    // Without this branch the packet was sent with no V3_4_3 wire opcode at
    // all, the modern client never resolved the player's name, and the
    // character panel + chat sender both rendered "Unknown".
    public QueryPlayerNameResponse() : base(GetResponseOpcode())
    {
        Data = new PlayerGuidLookupData();
    }

    private static Opcode GetResponseOpcode()
    {
        return ModernVersion.IsWotLKClassicOrLater
            ? Opcode.SMSG_QUERY_PLAYER_NAMES_RESPONSE
            : Opcode.SMSG_QUERY_PLAYER_NAME_RESPONSE;
    }

    public override void Write() => Layout.Write(this, _worldPacket);

    // Result byte(1) + GUID(18) + Data: bits(6) + 5 declined names(120) + 3 GUIDs(54) + ulong(8) + uint(4) + 5 bytes(5) + name(24) = 240 bytes max
    // V3_4_3 adds Count(4) + 1 byte for the two extra bits, 4.4.2 an int32, all within margin.
    public int MaxSize => 4 + 4 + 1 + PackedGuidHelper.MaxPackedGuid128Size + 6 +
        (PlayerConst.MaxDeclinedNameCases * GameLimits.MaxPlayerNameBytes) +
        PackedGuidHelper.MaxPackedGuid128Size * 3 + 8 + 4 + 5 + GameLimits.MaxPlayerNameBytes;

    public int WriteToSpan(Span<byte> buffer) => Layout.WriteToSpan(this, buffer);

    /// <summary>1.14 and 2.5: SMSG_QUERY_PLAYER_NAME_RESPONSE, one entry.</summary>
    internal sealed class SingleLayout : ServerPacketLayout<QueryPlayerNameResponse>
    {
        public override void Write(QueryPlayerNameResponse packet, WorldPacket data)
        {
            data.WriteInt8((sbyte)packet.Result);
            data.WritePackedGuid128(packet.Player);

            if (packet.Result == 0)
                packet.Data.Write(data, timerunning: false);
        }

        public override int WriteToSpan(QueryPlayerNameResponse packet, Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteInt8((sbyte)packet.Result);
            writer.WritePackedGuid128(packet.Player.Low, packet.Player.High);

            if (packet.Result == 0)
                packet.Data.WriteInline(ref writer, timerunning: false);

            return writer.Position;
        }
    }

    /// <summary>
    /// From 3.4.3 on: SMSG_QUERY_PLAYER_NAMES_RESPONSE with a count of one. 4.4.2 adds
    /// TimerunningSeasonID to the lookup data.
    /// </summary>
    internal sealed class ListLayout(bool timerunning) : ServerPacketLayout<QueryPlayerNameResponse>
    {
        public override void Write(QueryPlayerNameResponse packet, WorldPacket data)
        {
            data.WriteUInt32(1);   // Count: we always carry exactly one legacy SMSG_NAME_QUERY_RESPONSE.
            data.WriteUInt8(packet.Result);
            data.WritePackedGuid128(packet.Player);
            data.WriteBit(packet.Result == 0);   // HasPlayerGuidLookupData
            data.WriteBit(false);                // HasNameCacheUnused920
            data.FlushBits();
            if (packet.Result == 0)
                packet.Data.Write(data, timerunning);
        }

        public override int WriteToSpan(QueryPlayerNameResponse packet, Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            writer.WriteUInt32(1);
            writer.WriteUInt8(packet.Result);
            writer.WritePackedGuid128(packet.Player.Low, packet.Player.High);
            writer.WriteBit(packet.Result == 0);
            writer.WriteBit(false);
            writer.FlushBits();

            if (packet.Result == 0)
                packet.Data.WriteInline(ref writer, timerunning);

            return writer.Position;
        }
    }

    public WowGuid128 Player;
    public byte Result; // 0 - full packet, != 0 - only guid
    public PlayerGuidLookupData Data;
}

public class PlayerGuidLookupData
{
    /// <param name="timerunning">4.4.2 writes TimerunningSeasonID before the name.</param>
    public void Write(WorldPacket data, bool timerunning)
    {
        data.WriteBit(IsDeleted);
        data.WriteBits(Name.GetByteCount(), 6);

        for (byte i = 0; i < PlayerConst.MaxDeclinedNameCases; ++i)
            data.WriteBits(DeclinedNames.name[i].GetByteCount(), 7);

        data.FlushBits();
        for (byte i = 0; i < PlayerConst.MaxDeclinedNameCases; ++i)
            data.WriteString(DeclinedNames.name[i]);

        data.WritePackedGuid128(AccountID);
        data.WritePackedGuid128(BnetAccountID);
        data.WritePackedGuid128(GuidActual);
        data.WriteUInt64(GuildClubMemberID);
        data.WriteUInt32(VirtualRealmAddress);
        data.WriteUInt8((byte)RaceID);
        data.WriteUInt8((byte)Sex);
        data.WriteUInt8((byte)ClassID);
        data.WriteUInt8(Level);
        data.WriteUInt8(Unused915);
        if (timerunning)
            data.WriteInt32(0);         // TimerunningSeasonID
        data.WriteString(Name);
    }

    /// <summary>The same fields as <see cref="Write"/>, for the span path.</summary>
    internal void WriteInline(ref SpanPacketWriter writer, bool timerunning)
    {
        writer.WriteBit(IsDeleted);
        writer.WriteBits((uint)Encoding.UTF8.GetByteCount(Name), 6);

        for (byte i = 0; i < PlayerConst.MaxDeclinedNameCases; ++i)
            writer.WriteBits((uint)Encoding.UTF8.GetByteCount(DeclinedNames.name[i]), 7);

        writer.FlushBits();
        for (byte i = 0; i < PlayerConst.MaxDeclinedNameCases; ++i)
            writer.WriteString(DeclinedNames.name[i]);

        writer.WritePackedGuid128(AccountID.Low, AccountID.High);
        writer.WritePackedGuid128(BnetAccountID.Low, BnetAccountID.High);
        writer.WritePackedGuid128(GuidActual.Low, GuidActual.High);
        writer.WriteUInt64(GuildClubMemberID);
        writer.WriteUInt32(VirtualRealmAddress);
        writer.WriteUInt8((byte)RaceID);
        writer.WriteUInt8((byte)Sex);
        writer.WriteUInt8((byte)ClassID);
        writer.WriteUInt8(Level);
        writer.WriteUInt8(Unused915);
        if (timerunning)
            writer.WriteInt32(0);       // TimerunningSeasonID
        writer.WriteString(Name);
    }

    public bool IsDeleted;
    public WowGuid128 AccountID;
    public WowGuid128 BnetAccountID;
    public WowGuid128 GuidActual;
    public string Name = "";
    public ulong GuildClubMemberID;   // same as bgs.protocol.club.v1.MemberId.unique_id
    public uint VirtualRealmAddress;
    public Race RaceID = Race.None;
    public Gender Sex = Gender.None;
    public Class ClassID = Class.None;
    public byte Level;
    public byte Unused915;
    public DeclinedName DeclinedNames = new();
}

public class DeclinedName
{
    public string[] name = new string[PlayerConst.MaxDeclinedNameCases];

    public DeclinedName()
    {
        Array.Fill(name, string.Empty);
    }
}

public readonly record struct QueryQuestInfo(uint QuestID, WowGuid128 QuestGiver);

public sealed class QueryQuestInfoResponse : ServerPacket
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<QueryQuestInfoResponse>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V3_4_3_54261, new ClassicEraLayout()),
        (ClientVersionBuild.V3_4_3_54261, ClientVersionBuild.V4_4_2_60895, new WotLKClassicLayout()),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new CataClassicLayout()));

    private static readonly ServerPacketLayout<QueryQuestInfoResponse> Layout = Layouts.ForRunningClient();

    public QueryQuestInfoResponse() : base(Opcode.SMSG_QUERY_QUEST_INFO_RESPONSE, ConnectionType.Instance) { }

    public override void Write() => Layout.Write(this, _worldPacket);

    /// <summary>1.14 and 2.5.</summary>
    internal sealed class ClassicEraLayout : ServerPacketLayout<QueryQuestInfoResponse>
    {
        public override void Write(QueryQuestInfoResponse packet, WorldPacket data)
        {
            if (!packet.WriteHead(data))
                return;

            QuestTemplate info = packet.Info;
            data.WriteUInt32(info.PortraitGiver);
            data.WriteUInt32(info.PortraitGiverMount);
            data.WriteUInt32(info.PortraitTurnIn);

            data.WriteInt32(0); // Unk 2.5.2

            packet.WriteFactionsAndCurrencies(data);

            data.WriteUInt32(info.AreaGroupID);
            data.WriteUInt32(info.TimeAllowed);

            data.WriteInt32(info.Objectives.Count);
            data.WriteInt64(info.AllowableRaces);
            data.WriteInt32(info.TreasurePickerID);
            data.WriteInt32(info.Expansion);

            packet.WriteTexts(data, objectiveTypeInt32: false);
        }
    }

    // V3_4_3 layout (fork QueryQuestInfoResponse:82-112): adds
    // PortraitGiverModelSceneID between Mount and TurnIn, uses INT32 for
    // portrait fields (instead of UINT32), promotes TimeAllowed from
    // UINT32 to INT64, treats AllowableRaces as UINT64, and appends
    // ManagedWorldStateID/QuestSessionBonus/QuestGiverCreatureID. Without
    // these, the V3_4_3 client mis-parses the title-length bits at line
    // ~113 of the writer, then reads garbage as a ConditionalQuestText
    // length prefix → ~5 TB allocation crash (?AUConditionalQuestText@@).
    internal sealed class WotLKClassicLayout : ServerPacketLayout<QueryQuestInfoResponse>
    {
        public override void Write(QueryQuestInfoResponse packet, WorldPacket data)
        {
            if (!packet.WriteHead(data))
                return;

            QuestTemplate info = packet.Info;
            WritePortraits(data, info);
            packet.WriteFactionsAndCurrencies(data);

            data.WriteInt32((int)info.AreaGroupID);
            data.WriteInt64(info.TimeAllowed);

            data.WriteInt32(info.Objectives.Count);
            data.WriteUInt64((ulong)info.AllowableRaces);
            data.WriteInt32(info.TreasurePickerID);
            data.WriteInt32(info.Expansion);
            data.WriteInt32(info.ManagedWorldStateID);
            data.WriteInt32(info.QuestSessionBonus);
            data.WriteInt32((int)info.QuestGiverCreatureID);

            packet.WriteTexts(data, objectiveTypeInt32: false);
        }
    }

    /// <summary>
    /// 4.4.2 (TrinityCore cata_classic QuestPackets.cpp): TreasurePickerID became two counted lists,
    /// ManagedWorldStateID and QuestSessionBonus are gone, two conditional-text counts follow the
    /// creature id (their texts would close the packet; there are none), and the objective type is
    /// an int32.
    /// </summary>
    internal sealed class CataClassicLayout : ServerPacketLayout<QueryQuestInfoResponse>
    {
        public override void Write(QueryQuestInfoResponse packet, WorldPacket data)
        {
            if (!packet.WriteHead(data))
                return;

            QuestTemplate info = packet.Info;
            WritePortraits(data, info);
            packet.WriteFactionsAndCurrencies(data);

            data.WriteInt32((int)info.AreaGroupID);
            data.WriteInt64(info.TimeAllowed);

            data.WriteInt32(info.Objectives.Count);
            data.WriteUInt64((ulong)info.AllowableRaces);
            data.WriteUInt32(info.TreasurePickerID != 0 ? 1u : 0u);
            data.WriteUInt32(0);                        // TreasurePickerID2 count
            data.WriteInt32(info.Expansion);
            data.WriteInt32((int)info.QuestGiverCreatureID);
            data.WriteUInt32(0);                        // ConditionalQuestDescription count
            data.WriteUInt32(0);                        // ConditionalQuestCompletionLog count
            if (info.TreasurePickerID != 0)
                data.WriteInt32(info.TreasurePickerID);

            packet.WriteTexts(data, objectiveTypeInt32: true);
        }
    }

    private static void WritePortraits(WorldPacket data, QuestTemplate info)
    {
        data.WriteInt32((int)info.PortraitGiver);
        data.WriteInt32((int)info.PortraitGiverMount);
        data.WriteInt32((int)info.PortraitGiverModelSceneID);
        data.WriteInt32((int)info.PortraitTurnIn);
    }

    /// <summary>QuestID and Allow, then, when allowed, every field up to the portraits.</summary>
    private bool WriteHead(WorldPacket data)
    {
        data.WriteUInt32(QuestID);
        data.WriteBit(Allow);
        data.FlushBits();

        if (!Allow)
            return false;

        data.WriteUInt32(Info.QuestID);
        data.WriteInt32(Info.QuestType);
        data.WriteInt32(Info.QuestLevel);
        data.WriteInt32(Info.QuestScalingFactionGroup);
        data.WriteInt32(Info.QuestMaxScalingLevel);
        data.WriteUInt32(Info.QuestPackageID);
        data.WriteInt32(Info.MinLevel);
        data.WriteInt32(Info.QuestSortID);
        data.WriteUInt32(Info.QuestInfoID);
        data.WriteUInt32(Info.SuggestedGroupNum);
        data.WriteUInt32(Info.RewardNextQuest);
        data.WriteUInt32(Info.RewardXPDifficulty);

        data.WriteFloat(Info.RewardXPMultiplier);

        data.WriteInt32(Info.RewardMoney);
        data.WriteUInt32(Info.RewardMoneyDifficulty);
        data.WriteFloat(Info.RewardMoneyMultiplier);
        data.WriteUInt32(Info.RewardBonusMoney);

        for (uint i = 0; i < QuestConst.QuestRewardDisplaySpellCount; ++i)
            data.WriteUInt32(Info.RewardDisplaySpell[i]);

        data.WriteUInt32(Info.RewardSpell);
        data.WriteUInt32(Info.RewardHonor);

        data.WriteFloat(Info.RewardKillHonor);

        data.WriteInt32(Info.RewardArtifactXPDifficulty);
        data.WriteFloat(Info.RewardArtifactXPMultiplier);
        data.WriteInt32(Info.RewardArtifactCategoryID);

        data.WriteUInt32(Info.StartItem);
        data.WriteUInt32(Info.Flags);
        data.WriteUInt32(Info.FlagsEx);
        data.WriteUInt32(Info.FlagsEx2);

        for (uint i = 0; i < QuestConst.QuestRewardItemCount; ++i)
        {
            data.WriteUInt32(Info.RewardItems[i]);
            data.WriteUInt32(Info.RewardAmount[i]);
            data.WriteInt32(Info.ItemDrop[i]);
            data.WriteInt32(Info.ItemDropQuantity[i]);
        }

        for (uint i = 0; i < QuestConst.QuestRewardChoicesCount; ++i)
        {
            data.WriteUInt32(Info.UnfilteredChoiceItems[i].ItemID);
            data.WriteUInt32(Info.UnfilteredChoiceItems[i].Quantity);
            data.WriteUInt32(Info.UnfilteredChoiceItems[i].DisplayID);
        }

        data.WriteUInt32(Info.POIContinent);
        data.WriteFloat(Info.POIx);
        data.WriteFloat(Info.POIy);
        data.WriteUInt32(Info.POIPriority);

        data.WriteUInt32(Info.RewardTitle);
        data.WriteInt32(Info.RewardArenaPoints);
        data.WriteUInt32(Info.RewardSkillLineID);
        data.WriteUInt32(Info.RewardNumSkillUps);

        return true;
    }

    private void WriteFactionsAndCurrencies(WorldPacket data)
    {
        for (uint i = 0; i < QuestConst.QuestRewardReputationsCount; ++i)
        {
            data.WriteUInt32(Info.RewardFactionID[i]);
            data.WriteInt32(Info.RewardFactionValue[i]);
            data.WriteInt32(Info.RewardFactionOverride[i]);
            data.WriteInt32(Info.RewardFactionCapIn[i]);
        }

        data.WriteUInt32(Info.RewardFactionFlags);

        for (uint i = 0; i < QuestConst.QuestRewardCurrencyCount; ++i)
        {
            data.WriteUInt32(Info.RewardCurrencyID[i]);
            data.WriteUInt32(Info.RewardCurrencyQty[i]);
        }

        data.WriteUInt32(Info.AcceptedSoundKitID);
        data.WriteUInt32(Info.CompleteSoundKitID);
    }

    /// <summary>The text lengths, the objectives and the texts.</summary>
    private void WriteTexts(WorldPacket data, bool objectiveTypeInt32)
    {
        data.WriteBits(Info.LogTitle.GetByteCount(), 9);
        data.WriteBits(Info.LogDescription.GetByteCount(), 12);
        data.WriteBits(Info.QuestDescription.GetByteCount(), 12);
        data.WriteBits(Info.AreaDescription.GetByteCount(), 9);
        data.WriteBits(Info.PortraitGiverText.GetByteCount(), 10);
        data.WriteBits(Info.PortraitGiverName.GetByteCount(), 8);
        data.WriteBits(Info.PortraitTurnInText.GetByteCount(), 10);
        data.WriteBits(Info.PortraitTurnInName.GetByteCount(), 8);
        data.WriteBits(Info.QuestCompletionLog.GetByteCount(), 11);
        data.WriteBit(Info.ReadyForTranslation);
        data.FlushBits();

        foreach (QuestObjective questObjective in Info.Objectives)
        {
            data.WriteUInt32(questObjective.Id);
            if (objectiveTypeInt32)
                data.WriteInt32((int)questObjective.Type);
            else
                data.WriteUInt8((byte)questObjective.Type);
            data.WriteInt8(questObjective.StorageIndex);
            data.WriteInt32(questObjective.ObjectID);
            data.WriteInt32(questObjective.Amount);
            data.WriteUInt32((uint)questObjective.Flags);
            data.WriteUInt32(questObjective.Flags2);
            data.WriteFloat(questObjective.ProgressBarWeight);

            data.WriteInt32(questObjective.VisualEffects.Length);
            foreach (var visualEffect in questObjective.VisualEffects)
                data.WriteInt32(visualEffect);

            data.WriteBits(questObjective.Description.GetByteCount(), 8);
            data.FlushBits();

            data.WriteString(questObjective.Description);
        }

        data.WriteString(Info.LogTitle);
        data.WriteString(Info.LogDescription);
        data.WriteString(Info.QuestDescription);
        data.WriteString(Info.AreaDescription);
        data.WriteString(Info.PortraitGiverText);
        data.WriteString(Info.PortraitGiverName);
        data.WriteString(Info.PortraitTurnInText);
        data.WriteString(Info.PortraitTurnInName);
        data.WriteString(Info.QuestCompletionLog);
    }

    public bool Allow;
    public QuestTemplate Info = null!;
    public uint QuestID;
}

public readonly record struct QueryCreature(uint CreatureID);

public sealed class QueryCreatureResponse : ServerPacket
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<QueryCreatureResponse>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V4_4_2_60895, new StatsLayout(questCurrencies: false)),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new StatsLayout(questCurrencies: true)));

    private static readonly ServerPacketLayout<QueryCreatureResponse> Layout = Layouts.ForRunningClient();

    public QueryCreatureResponse() : base(Opcode.SMSG_QUERY_CREATURE_RESPONSE, ConnectionType.Instance) { }

    public override void Write() => Layout.Write(this, _worldPacket);

    /// <summary>
    /// 4.4.2 (TrinityCore cata_classic) adds a QuestCurrencies count after the QuestItems count and
    /// the currency ids after the quest items. The legacy servers have no quest currencies, so the
    /// count is always zero.
    /// </summary>
    internal sealed class StatsLayout(bool questCurrencies) : ServerPacketLayout<QueryCreatureResponse>
    {
        public override void Write(QueryCreatureResponse packet, WorldPacket data) => packet.Write(data, questCurrencies);
    }

    private void Write(WorldPacket data, bool questCurrencies)
    {
        data.WriteUInt32(CreatureID);
        data.WriteBit(Allow);
        data.FlushBits();

        if (Allow)
        {
            data.WriteBits(Stats.Title.IsEmpty() ? 0 : Stats.Title.GetByteCount() + 1, 11);
            data.WriteBits(Stats.TitleAlt.IsEmpty() ? 0 : Stats.TitleAlt.GetByteCount() + 1, 11);
            data.WriteBits(Stats.CursorName.IsEmpty() ? 0 : Stats.CursorName.GetByteCount() + 1, 6);
            data.WriteBit(Stats.Civilian);
            data.WriteBit(Stats.Leader);

            for (var i = 0; i < CreatureConst.MaxCreatureNames; ++i)
            {
                data.WriteBits(Stats.Name[i].GetByteCount() + 1, 11);
                data.WriteBits(Stats.NameAlt[i].GetByteCount() + 1, 11);
            }

            for (var i = 0; i < CreatureConst.MaxCreatureNames; ++i)
            {
                if (!string.IsNullOrEmpty(Stats.Name[i]))
                    data.WriteCString(Stats.Name[i]);
                if (!string.IsNullOrEmpty(Stats.NameAlt[i]))
                    data.WriteCString(Stats.NameAlt[i]);
            }

            for (var i = 0; i < 2; ++i)
                data.WriteUInt32(Stats.Flags[i]);

            data.WriteInt32(Stats.Type);
            data.WriteInt32(Stats.Family);
            data.WriteInt32(Stats.Classification);
            data.WriteUInt32(Stats.PetSpellDataId);

            for (var i = 0; i < CreatureConst.MaxCreatureKillCredit; ++i)
                data.WriteUInt32(Stats.ProxyCreatureID[i]);

            data.WriteInt32(Stats.Display.CreatureDisplay.Count);
            data.WriteFloat(Stats.Display.TotalProbability);

            foreach (CreatureXDisplay display in Stats.Display.CreatureDisplay)
            {
                data.WriteUInt32(display.CreatureDisplayID);
                data.WriteFloat(display.Scale);
                data.WriteFloat(display.Probability);
            }

            data.WriteFloat(Stats.HpMulti);
            data.WriteFloat(Stats.EnergyMulti);

            data.WriteInt32(Stats.QuestItems.Count);
            if (questCurrencies)
                data.WriteUInt32(0);    // QuestCurrencies count
            data.WriteUInt32(Stats.MovementInfoID);
            data.WriteInt32(Stats.HealthScalingExpansion);
            data.WriteUInt32(Stats.RequiredExpansion);
            data.WriteUInt32(Stats.VignetteID);
            data.WriteInt32(Stats.Class);
            data.WriteInt32(Stats.DifficultyID);
            data.WriteInt32(Stats.WidgetSetID);
            data.WriteInt32(Stats.WidgetSetUnitConditionID);

            if (!Stats.Title.IsEmpty())
                data.WriteCString(Stats.Title);

            if (!Stats.TitleAlt.IsEmpty())
                data.WriteCString(Stats.TitleAlt);

            if (!Stats.CursorName.IsEmpty())
                data.WriteCString(Stats.CursorName);

            foreach (var questItem in Stats.QuestItems)
                data.WriteUInt32(questItem);
        }

        if (Allow)
        {
            Log.Print(LogType.Trace,
                $"[CreatureQueryTrace][write] entry={CreatureID} allow=true packetBytes={data.GetSize()} healthScalingExp={Stats.HealthScalingExpansion} reqExp={Stats.RequiredExpansion} creatureClass={Stats.Class} displays={Stats.Display.CreatureDisplay.Count} totalProb={Stats.Display.TotalProbability}");
        }
        else
        {
            Log.Print(LogType.Trace,
                $"[CreatureQueryTrace][write] entry={CreatureID} allow=false packetBytes={data.GetSize()}");
        }
    }

    public bool Allow;
    public CreatureTemplate Stats = null!;
    public uint CreatureID;
}

public readonly record struct QueryGameObject(uint GameObjectID, WowGuid128 Guid);

public class QueryGameObjectResponse : ServerPacket
{
    public QueryGameObjectResponse() : base(Opcode.SMSG_QUERY_GAME_OBJECT_RESPONSE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(GameObjectID);
        _worldPacket.WritePackedGuid128(Guid);
        _worldPacket.WriteBit(Allow);
        _worldPacket.FlushBits();

        ByteBuffer statsData = new();
        if (Allow)
        {
            statsData.WriteUInt32(Stats.Type);
            statsData.WriteUInt32(Stats.DisplayID);
            for (int i = 0; i < 4; i++)
                statsData.WriteCString(Stats.Name[i]);

            statsData.WriteCString(Stats.IconName);
            statsData.WriteCString(Stats.CastBarCaption);
            statsData.WriteCString(Stats.UnkString);

            int dataFieldsCount = ModernVersion.AddedInClassicVersion(1, 14, 1, 2, 5, 3) ? 35 : 34;
            for (int i = 0; i < dataFieldsCount; i++)
                statsData.WriteInt32(Stats.Data[i]);

            statsData.WriteFloat(Stats.Size);
            statsData.WriteUInt8((byte)Stats.QuestItems.Count);
            foreach (uint questItem in Stats.QuestItems)
                statsData.WriteUInt32(questItem);

            statsData.WriteUInt32(Stats.ContentTuningId);
        }

        _worldPacket.WriteUInt32(statsData.GetSize());
        if (statsData.GetSize() != 0)
            _worldPacket.WriteBytes(statsData);
    }

    public uint GameObjectID;
    public WowGuid128 Guid;
    public bool Allow;
    public GameObjectStats Stats = null!;
}

public class GameObjectStats
{
    public GameObjectStats()
    {
        Array.Fill(Name, string.Empty);
    }

    public string[] Name = new string[4];
    public string IconName = "";
    public string CastBarCaption = "";
    public string UnkString = "";
    public uint Type;
    public uint DisplayID;
    public int[] Data = new int[35];

    /// <summary>
    /// gameobject_template.data index of destructibleBuilding.DestructibleModelRec. Same slot
    /// on 3.3.5a and 3.4.3.
    /// </summary>
    private const int DestructibleModelRecIndex = 18;

    /// <summary>
    /// DestructibleModelData.db2 id for a GAMEOBJECT_TYPE_DESTRUCTIBLE_BUILDING. The V3_4_3
    /// client resolves the object's model through this record rather than through DisplayID,
    /// and draws nothing without it — see UpdateHandler.SetDestructibleParentRotation and
    /// issue #184. Meaningless for any other GameObject type.
    /// </summary>
    public int DestructibleModelRec => Data[DestructibleModelRecIndex];

    /// <summary>
    /// Lock.dbc id for the types that carry one, mirroring the legacy server's own
    /// GameObjectTemplate::GetLockId. The slot is not the same for every type: DOOR and
    /// BUTTON keep it in data[1] and FISHINGHOLE in data[4], everything else in data[0].
    /// 0 when the type has no lock. Issue #269.
    /// </summary>
    public uint LegacyLockId => (GameObjectTypeLegacy)Type switch
    {
        GameObjectTypeLegacy.Door or
        GameObjectTypeLegacy.Button => (uint)Data[1],

        GameObjectTypeLegacy.QuestGiver or
        GameObjectTypeLegacy.Chest or
        GameObjectTypeLegacy.Trap or
        GameObjectTypeLegacy.Goober or
        GameObjectTypeLegacy.AreaDamage or
        GameObjectTypeLegacy.Camera or
        GameObjectTypeLegacy.FlagStand or
        GameObjectTypeLegacy.FlagDrop => (uint)Data[0],

        GameObjectTypeLegacy.FishingHole => (uint)Data[4],

        _ => 0u,
    };

    public float Size = 1;
    public List<uint> QuestItems = new();
    public uint ContentTuningId;
}

public readonly record struct QueryPageText(uint PageTextID, WowGuid128 ItemGUID);

public class QueryPageTextResponse : ServerPacket
{
    public QueryPageTextResponse() : base(Opcode.SMSG_QUERY_PAGE_TEXT_RESPONSE) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(PageTextID);
        _worldPacket.WriteBit(Allow);
        _worldPacket.FlushBits();

        if (Allow)
        {
            _worldPacket.WriteInt32(Pages.Count);
            foreach (PageTextInfo pageText in Pages)
                pageText.Write(_worldPacket);
        }
    }

    public uint PageTextID;
    public bool Allow;
    public List<PageTextInfo> Pages = new();

    public struct PageTextInfo
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt32(Id);
            data.WriteUInt32(NextPageID);
            data.WriteInt32(PlayerConditionID);
            data.WriteUInt8(Flags);
            data.WriteBits(Text.GetByteCount(), 12);
            data.FlushBits();

            data.WriteString(Text);
        }

        public uint Id;
        public uint NextPageID;
        public int PlayerConditionID;
        public byte Flags;
        public string Text;
    }
}

public readonly record struct QueryNPCText(uint TextID, WowGuid128 Guid);

public class QueryNPCTextResponse : ServerPacket, ISpanWritable
{
    public QueryNPCTextResponse() : base(Opcode.SMSG_QUERY_NPC_TEXT_RESPONSE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(TextID);
        _worldPacket.WriteBit(Allow);

        _worldPacket.WriteInt32(Allow ? 8 * (4 + 4) : 0);
        if (Allow)
        {
            for (uint i = 0; i < 8; ++i)
                _worldPacket.WriteFloat(Probabilities[i]);

            for (uint i = 0; i < 8; ++i)
                _worldPacket.WriteUInt32(BroadcastTextID[i]);
        }
    }

    // Fixed size: uint(4) + bit(1) + int(4) + 8 floats(32) + 8 uints(32) = 73 bytes
    public int MaxSize => 4 + 1 + 4 + 32 + 32;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(TextID);
        writer.WriteBit(Allow);

        writer.WriteInt32(Allow ? 8 * (4 + 4) : 0);
        if (Allow)
        {
            for (uint i = 0; i < 8; ++i)
                writer.WriteFloat(Probabilities[i]);

            for (uint i = 0; i < 8; ++i)
                writer.WriteUInt32(BroadcastTextID[i]);
        }
        return writer.Position;
    }

    public uint TextID;
    public bool Allow;
    public float[] Probabilities = new float[8];
    public uint[] BroadcastTextID = new uint[8];
}

/// <remarks>
/// Holds <see cref="WhoRequest"/> by reference. It carries a non-zero default — ClassFilter is
/// -1, meaning "any class" — and flattening it into a positional record struct would make that
/// default a zero, which is the Warrior class id.
/// </remarks>
public readonly record struct WhoRequestPkt(WhoRequest Request, uint RequestID, List<int> Areas);

public class WhoRequest
{
    /// <remarks>
    /// Span twin of <see cref="Read(WorldPacket)"/>. The five bit lengths are all read before any
    /// of their strings and the words list sits between them and the names, so the two bodies
    /// must stay in step field for field.
    /// </remarks>
    public void Read(ref SpanPacketReader data)
    {
        MinLevel = data.ReadInt32();
        MaxLevel = data.ReadInt32();
        RaceFilter = data.ReadInt64();
        ClassFilter = data.ReadInt32();

        uint nameLength = data.ReadBits<uint>(6);
        uint virtualRealmNameLength = data.ReadBits<uint>(9);
        uint guildNameLength = data.ReadBits<uint>(7);
        uint guildVirtualRealmNameLength = data.ReadBits<uint>(9);
        uint wordsCount = data.ReadBits<uint>(3);

        ShowEnemies = data.HasBit();
        ShowArenaPlayers = data.HasBit();
        ExactName = data.HasBit();
        if (data.HasBit())
            ServerInfo = new();
        data.ResetBitPos();

        for (int i = 0; i < wordsCount; ++i)
        {
            Words.Add(data.ReadString(data.ReadBits<uint>(7)));
            data.ResetBitPos();
        }

        Name = data.ReadString(nameLength);
        VirtualRealmName = data.ReadString(virtualRealmNameLength);
        Guild = data.ReadString(guildNameLength);
        GuildVirtualRealmName = data.ReadString(guildVirtualRealmNameLength);

        if (ServerInfo != null)
            ServerInfo.Read(ref data);
    }

    public void Read(WorldPacket data)
    {
        MinLevel = data.ReadInt32();
        MaxLevel = data.ReadInt32();
        RaceFilter = data.ReadInt64();
        ClassFilter = data.ReadInt32();

        uint nameLength = data.ReadBits<uint>(6);
        uint virtualRealmNameLength = data.ReadBits<uint>(9);
        uint guildNameLength = data.ReadBits<uint>(7);
        uint guildVirtualRealmNameLength = data.ReadBits<uint>(9);
        uint wordsCount = data.ReadBits<uint>(3);

        ShowEnemies = data.HasBit();
        ShowArenaPlayers = data.HasBit();
        ExactName = data.HasBit();
        if (data.HasBit())
            ServerInfo = new();
        data.ResetBitPos();

        for (int i = 0; i < wordsCount; ++i)
        {
            Words.Add(data.ReadString(data.ReadBits<uint>(7)));
            data.ResetBitPos();
        }

        Name = data.ReadString(nameLength);
        VirtualRealmName = data.ReadString(virtualRealmNameLength);
        Guild = data.ReadString(guildNameLength);
        GuildVirtualRealmName = data.ReadString(guildVirtualRealmNameLength);

        if (ServerInfo != null)
            ServerInfo.Read(data);
    }

    public int MinLevel;
    public int MaxLevel;
    public string Name = string.Empty;
    public string VirtualRealmName = string.Empty;
    public string Guild = string.Empty;
    public string GuildVirtualRealmName = string.Empty;
    public long RaceFilter;
    public int ClassFilter = -1;
    public List<string> Words = new();
    public bool ShowEnemies;
    public bool ShowArenaPlayers;
    public bool ExactName;
    public WhoRequestServerInfo ServerInfo = null!;
}

public class WhoRequestServerInfo
{
    /// <remarks>Span twin; kept in step with the ByteBuffer form below.</remarks>
    public void Read(ref SpanPacketReader data)
    {
        FactionGroup = data.ReadInt32();
        Locale = data.ReadInt32();
        RequesterVirtualRealmAddress = data.ReadUInt32();
    }

    public void Read(WorldPacket data)
    {
        FactionGroup = data.ReadInt32();
        Locale = data.ReadInt32();
        RequesterVirtualRealmAddress = data.ReadUInt32();
    }

    public int FactionGroup;
    public int Locale;
    public uint RequesterVirtualRealmAddress;
}

public sealed class WhoResponsePkt : ServerPacket
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<WhoResponsePkt>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V4_4_2_60895, new EntriesLayout(timerunning: false)),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new EntriesLayout(timerunning: true)));

    private static readonly ServerPacketLayout<WhoResponsePkt> Layout = Layouts.ForRunningClient();

    public WhoResponsePkt() : base(Opcode.SMSG_WHO) { }

    public override void Write() => Layout.Write(this, _worldPacket);

    /// <summary>Every entry carries a <see cref="PlayerGuidLookupData"/>, which grew a field in 4.4.2.</summary>
    internal sealed class EntriesLayout(bool timerunning) : ServerPacketLayout<WhoResponsePkt>
    {
        public override void Write(WhoResponsePkt packet, WorldPacket data)
        {
            data.WriteUInt32(packet.RequestID);
            data.WriteBits(packet.Players.Count, 6);
            data.FlushBits();

            foreach (WhoEntry entry in packet.Players)
                entry.Write(data, timerunning);
        }
    }

    public uint RequestID;
    public List<WhoEntry> Players = new();
}

public class WhoEntry
{
    public void Write(WorldPacket data, bool timerunning)
    {
        PlayerData.Write(data, timerunning);

        data.WritePackedGuid128(GuildGUID);
        data.WriteUInt32(GuildVirtualRealmAddress);
        data.WriteInt32(AreaID);

        data.WriteBits(GuildName.GetByteCount(), 7);
        data.WriteBit(IsGM);
        data.WriteString(GuildName);

        data.FlushBits();
    }

    public PlayerGuidLookupData PlayerData = new();
    public WowGuid128 GuildGUID = WowGuid128.Empty;
    public uint GuildVirtualRealmAddress;
    public string GuildName = "";
    public int AreaID;
    public bool IsGM;
}

public readonly record struct ItemTextQuery(WowGuid128 Id);

class QueryItemTextResponse : ServerPacket
{
    public QueryItemTextResponse() : base(Opcode.SMSG_QUERY_ITEM_TEXT_RESPONSE) { }

    public override void Write()
    {
        _worldPacket.WriteBit(Valid);
        _worldPacket.FlushBits();

        // ItemTextCache is written unconditionally, even when Valid is false.
        _worldPacket.WriteBits(Text.GetByteCount(), 13);
        _worldPacket.FlushBits();
        _worldPacket.WriteString(Text);

        _worldPacket.WritePackedGuid128(Id);
    }

    public WowGuid128 Id = WowGuid128.Empty;
    public bool Valid;
    public string Text = string.Empty;
}
