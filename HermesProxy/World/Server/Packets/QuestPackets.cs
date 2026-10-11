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
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace HermesProxy.World.Server.Packets;

public readonly record struct QuestGiverQueryQuest(WowGuid128 QuestGiverGUID, uint QuestID, bool RespondToGiver);

public sealed class QuestGiverQuestDetails : ServerPacket
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<QuestGiverQuestDetails>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V3_4_3_54261, new ClassicEraLayout()),
        (ClientVersionBuild.V3_4_3_54261, ClientVersionBuild.V4_4_2_60895, new WotLKClassicLayout()),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new CataClassicLayout()));

    private static readonly ServerPacketLayout<QuestGiverQuestDetails> Layout = Layouts.ForRunningClient();

    public QuestGiverQuestDetails() : base(Opcode.SMSG_QUEST_GIVER_QUEST_DETAILS)
    {
        for (int i = 0; i < QuestConst.QuestRewardReputationsCount; i++)
            Rewards.FactionCapIn[i] = 7;
    }

    public override void Write() => Layout.Write(this, _worldPacket);

    /// <summary>1.14 and 2.5.</summary>
    internal sealed class ClassicEraLayout : ServerPacketLayout<QuestGiverQuestDetails>
    {
        public override void Write(QuestGiverQuestDetails packet, WorldPacket data) => packet.WriteClassicEra(data);
    }

    /// <summary>3.4.3.</summary>
    internal sealed class WotLKClassicLayout : ServerPacketLayout<QuestGiverQuestDetails>
    {
        public override void Write(QuestGiverQuestDetails packet, WorldPacket data) => packet.WriteWotLKClassic(data);
    }

    /// <summary>4.4.2.</summary>
    internal sealed class CataClassicLayout : ServerPacketLayout<QuestGiverQuestDetails>
    {
        public override void Write(QuestGiverQuestDetails packet, WorldPacket data) => packet.WriteCataClassic(data);
    }

    private void WriteClassicEra(WorldPacket data)
    {
        data.WritePackedGuid128(QuestGiverGUID);
        data.WritePackedGuid128(InformUnit);
        data.WriteUInt32(QuestID);
        data.WriteInt32(QuestPackageID);
        data.WriteUInt32(PortraitGiver);
        data.WriteUInt32(PortraitGiverMount);
        data.WriteUInt32(PortraitGiverModelSceneID);
        data.WriteUInt32(PortraitTurnIn);
        data.WriteUInt32(QuestFlags[0]); // Flags
        data.WriteUInt32(QuestFlags[1]); // FlagsEx
        data.WriteUInt32(SuggestedPartyMembers);
        data.WriteInt32(LearnSpells.Count);
        data.WriteInt32(DescEmotes.Length);
        data.WriteInt32(Objectives.Count);
        data.WriteInt32(QuestStartItemID);
        data.WriteInt32(QuestSessionBonus);

        foreach (uint spell in LearnSpells)
            data.WriteUInt32(spell);

        foreach (QuestDescEmote emote in DescEmotes)
        {
            data.WriteUInt32(emote.Type);
            data.WriteUInt32(emote.Delay);
        }

        foreach (QuestObjectiveSimple obj in Objectives)
        {
            data.WriteUInt32(obj.Id);
            data.WriteInt32(obj.ObjectID);
            data.WriteInt32(obj.Amount);
            data.WriteUInt8(obj.Type);
        }

        data.WriteBits(QuestTitle.GetByteCount(), 9);
        data.WriteBits(DescriptionText.GetByteCount(), 12);
        data.WriteBits(LogDescription.GetByteCount(), 12);
        data.WriteBits(PortraitGiverText.GetByteCount(), 10);
        data.WriteBits(PortraitGiverName.GetByteCount(), 8);
        data.WriteBits(PortraitTurnInText.GetByteCount(), 10);
        data.WriteBits(PortraitTurnInName.GetByteCount(), 8);
        data.WriteBit(AutoLaunched);
        data.WriteBit(false);   // unused in client
        data.WriteBit(StartCheat);
        data.WriteBit(DisplayPopup);
        data.FlushBits();

        Rewards.Write(data);

        data.WriteString(QuestTitle);
        data.WriteString(DescriptionText);
        data.WriteString(LogDescription);
        data.WriteString(PortraitGiverText);
        data.WriteString(PortraitGiverName);
        data.WriteString(PortraitTurnInText);
        data.WriteString(PortraitTurnInName);
    }

    // Mirrors WPP V3_4_0 SMSG_QUEST_GIVER_QUEST_DETAILS parser (range
    // V3_4_3_51505 → V3_4_4_59817). Diff vs retail Write: adds QuestFlags[2],
    // QuestGiverCreatureID, ConditionalDescriptionTextCount; QuestRewards block
    // is the same as retail (NOT the V3_4_4+ shape). Objectives have the same
    // (Id, ObjectID, Amount, Type:u8) shape as retail. Newer V3_4_4 layouts
    // (QuestInfoID, Items-first QuestRewards, Objective Type:i32 reordered)
    // are NOT present in build 54261.
    private void WriteWotLKClassic(WorldPacket data)
    {
        data.WritePackedGuid128(QuestGiverGUID);
        data.WritePackedGuid128(InformUnit);
        data.WriteInt32((int)QuestID);
        data.WriteInt32(QuestPackageID);
        data.WriteInt32((int)PortraitGiver);
        data.WriteInt32((int)PortraitGiverMount);
        data.WriteInt32((int)PortraitGiverModelSceneID);
        data.WriteInt32((int)PortraitTurnIn);
        data.WriteUInt32(QuestFlags[0]);    // Flags
        data.WriteUInt32(QuestFlags[1]);    // FlagsEx
        data.WriteUInt32(0);                // FlagsEx2 (V3_4_3 only)
        data.WriteInt32((int)SuggestedPartyMembers);
        data.WriteUInt32((uint)LearnSpells.Count);
        data.WriteUInt32((uint)DescEmotes.Length);
        data.WriteUInt32((uint)Objectives.Count);
        data.WriteInt32(QuestStartItemID);
        data.WriteInt32(QuestSessionBonus);
        data.WriteInt32((int)QuestGiverCreatureID);
        data.WriteUInt32(0);                // ConditionalDescriptionText.size

        foreach (uint spell in LearnSpells)
            data.WriteInt32((int)spell);

        foreach (QuestDescEmote emote in DescEmotes)
        {
            data.WriteInt32((int)emote.Type);
            data.WriteUInt32(emote.Delay);
        }

        foreach (QuestObjectiveSimple obj in Objectives)
        {
            data.WriteUInt32(obj.Id);
            data.WriteInt32(obj.ObjectID);
            data.WriteInt32(obj.Amount);
            data.WriteUInt8(obj.Type);
        }

        data.WriteBits(QuestTitle.GetByteCount(), 9);
        data.WriteBits(DescriptionText.GetByteCount(), 12);
        data.WriteBits(LogDescription.GetByteCount(), 12);
        data.WriteBits(PortraitGiverText.GetByteCount(), 10);
        data.WriteBits(PortraitGiverName.GetByteCount(), 8);
        data.WriteBits(PortraitTurnInText.GetByteCount(), 10);
        data.WriteBits(PortraitTurnInName.GetByteCount(), 8);
        data.WriteBit(AutoLaunched);
        data.WriteBit(false);
        data.WriteBit(StartCheat);
        data.WriteBit(DisplayPopup);
        data.FlushBits();

        Rewards.Write(data);

        data.WriteString(QuestTitle);
        data.WriteString(DescriptionText);
        data.WriteString(LogDescription);
        data.WriteString(PortraitGiverText);
        data.WriteString(PortraitGiverName);
        data.WriteString(PortraitTurnInText);
        data.WriteString(PortraitTurnInName);
    }

    // 4.4.2 (TrinityCore cata_classic QuestPackets.cpp): QuestInfoID after QuestStartItemID,
    // objectives as (ID, Type:i32, ObjectID, Amount), the FromContentPush, ReplayQuest and
    // ResetByScheduler bits, and the items-first QuestRewards. The conditional description texts
    // would follow the strings; there are none.
    private void WriteCataClassic(WorldPacket data)
    {
        data.WritePackedGuid128(QuestGiverGUID);
        data.WritePackedGuid128(InformUnit);
        data.WriteInt32((int)QuestID);
        data.WriteInt32(QuestPackageID);
        data.WriteInt32((int)PortraitGiver);
        data.WriteInt32((int)PortraitGiverMount);
        data.WriteInt32((int)PortraitGiverModelSceneID);
        data.WriteInt32((int)PortraitTurnIn);
        data.WriteUInt32(QuestFlags[0]);    // Flags
        data.WriteUInt32(QuestFlags[1]);    // FlagsEx
        data.WriteUInt32(0);                // FlagsEx2
        data.WriteInt32((int)SuggestedPartyMembers);
        data.WriteUInt32((uint)LearnSpells.Count);
        data.WriteUInt32((uint)DescEmotes.Length);
        data.WriteUInt32((uint)Objectives.Count);
        data.WriteInt32(QuestStartItemID);
        data.WriteInt32(0);                 // QuestInfoID
        data.WriteInt32(QuestSessionBonus);
        data.WriteInt32((int)QuestGiverCreatureID);
        data.WriteUInt32(0);                // ConditionalDescriptionText.size

        foreach (uint spell in LearnSpells)
            data.WriteInt32((int)spell);

        foreach (QuestDescEmote emote in DescEmotes)
        {
            data.WriteInt32((int)emote.Type);
            data.WriteUInt32(emote.Delay);
        }

        foreach (QuestObjectiveSimple obj in Objectives)
        {
            data.WriteUInt32(obj.Id);
            data.WriteInt32(obj.Type);
            data.WriteInt32(obj.ObjectID);
            data.WriteInt32(obj.Amount);
        }

        data.WriteBits(QuestTitle.GetByteCount(), 9);
        data.WriteBits(DescriptionText.GetByteCount(), 12);
        data.WriteBits(LogDescription.GetByteCount(), 12);
        data.WriteBits(PortraitGiverText.GetByteCount(), 10);
        data.WriteBits(PortraitGiverName.GetByteCount(), 8);
        data.WriteBits(PortraitTurnInText.GetByteCount(), 10);
        data.WriteBits(PortraitTurnInName.GetByteCount(), 8);
        data.WriteBit(AutoLaunched);
        data.WriteBit(false);               // FromContentPush
        data.WriteBit(false);               // ReplayQuest
        data.WriteBit(false);               // ResetByScheduler
        data.WriteBit(StartCheat);
        data.WriteBit(DisplayPopup);
        data.FlushBits();

        Rewards.WriteCataClassic(data);

        data.WriteString(QuestTitle);
        data.WriteString(DescriptionText);
        data.WriteString(LogDescription);
        data.WriteString(PortraitGiverText);
        data.WriteString(PortraitGiverName);
        data.WriteString(PortraitTurnInText);
        data.WriteString(PortraitTurnInName);
    }

    public WowGuid128 QuestGiverGUID;
    public WowGuid128 InformUnit;
    public uint QuestGiverCreatureID;
    public uint QuestID;
    public int QuestPackageID;
    public uint[] QuestFlags = new uint[2];
    public uint SuggestedPartyMembers;
    public QuestRewards Rewards = new();
    public List<QuestObjectiveSimple> Objectives = new();
    public QuestDescEmote[] DescEmotes = new QuestDescEmote[QuestConst.QuestEmoteCount];
    public List<uint> LearnSpells = new();
    public uint PortraitTurnIn;
    public uint PortraitGiver;
    public uint PortraitGiverMount;
    public uint PortraitGiverModelSceneID;
    public int QuestStartItemID;
    public int QuestSessionBonus;
    public string PortraitGiverText = "";
    public string PortraitGiverName = "";
    public string PortraitTurnInText = "";
    public string PortraitTurnInName = "";
    public string QuestTitle = "";
    public string DescriptionText = "";
    public string LogDescription = "";
    public bool DisplayPopup;
    public bool StartCheat;
    public bool AutoLaunched;
}

public class QuestRewards
{
    public QuestRewards()
    {
        for (int i = 0; i < QuestConst.QuestRewardChoicesCount; i++)
            ChoiceItems[i] = new();
    }
    public uint ChoiceItemCount;
    public uint ItemCount;
    public uint Money;
    public uint XP;
    public uint ArtifactXP;
    public uint ArtifactCategoryID;
    public uint Honor;
    public uint Title;
    public uint FactionFlags;
    public int[] SpellCompletionDisplayID = new int[QuestConst.QuestRewardDisplaySpellCount];
    public uint SpellCompletionID;
    public uint SkillLineID;
    public uint NumSkillUps;
    public uint TreasurePickerID;
    public QuestChoiceItem[] ChoiceItems = new QuestChoiceItem[QuestConst.QuestRewardChoicesCount];
    public uint[] ItemID = new uint[QuestConst.QuestRewardItemCount];
    public uint[] ItemQty = new uint[QuestConst.QuestRewardItemCount];
    public uint[] FactionID = new uint[QuestConst.QuestRewardReputationsCount];
    public int[] FactionValue = new int[QuestConst.QuestRewardReputationsCount];
    public int[] FactionOverride = new int[QuestConst.QuestRewardReputationsCount];
    public int[] FactionCapIn = new int[QuestConst.QuestRewardReputationsCount];
    public uint[] CurrencyID = new uint[QuestConst.QuestRewardCurrencyCount];
    public uint[] CurrencyQty = new uint[QuestConst.QuestRewardCurrencyCount];
    public bool IsBoostSpell;

    public void Write(WorldPacket data)
    {
        data.WriteUInt32(ChoiceItemCount);
        data.WriteUInt32(ItemCount);

        for (int i = 0; i < QuestConst.QuestRewardItemCount; ++i)
        {
            data.WriteUInt32(ItemID[i]);
            data.WriteUInt32(ItemQty[i]);
        }

        data.WriteUInt32(Money);
        data.WriteUInt32(XP);
        data.WriteUInt64(ArtifactXP);
        data.WriteUInt32(ArtifactCategoryID);
        data.WriteUInt32(Honor);
        data.WriteUInt32(Title);
        data.WriteUInt32(FactionFlags);

        for (int i = 0; i < QuestConst.QuestRewardReputationsCount; ++i)
        {
            data.WriteUInt32(FactionID[i]);
            data.WriteInt32(FactionValue[i]);
            data.WriteInt32(FactionOverride[i]);
            data.WriteInt32(FactionCapIn[i]);
        }

        foreach (var id in SpellCompletionDisplayID)
            data.WriteInt32(id);

        data.WriteUInt32(SpellCompletionID);

        for (int i = 0; i < QuestConst.QuestRewardCurrencyCount; ++i)
        {
            data.WriteUInt32(CurrencyID[i]);
            data.WriteUInt32(CurrencyQty[i]);
        }

        data.WriteUInt32(SkillLineID);
        data.WriteUInt32(NumSkillUps);
        data.WriteUInt32(TreasurePickerID);

        foreach (var choice in ChoiceItems)
            choice.Write(data);

        data.WriteBit(IsBoostSpell);
        data.FlushBits();
    }

    // 4.4.2 (TrinityCore cata_classic): the reward items and currencies (with a BonusQty) lead, and
    // TreasurePickerID became a counted list.
    public void WriteCataClassic(WorldPacket data)
    {
        for (int i = 0; i < QuestConst.QuestRewardItemCount; ++i)
        {
            data.WriteUInt32(ItemID[i]);
            data.WriteUInt32(ItemQty[i]);
        }

        for (int i = 0; i < QuestConst.QuestRewardCurrencyCount; ++i)
        {
            data.WriteUInt32(CurrencyID[i]);
            data.WriteUInt32(CurrencyQty[i]);
            data.WriteInt32(0);             // BonusQty
        }

        data.WriteUInt32(ChoiceItemCount);
        data.WriteUInt32(ItemCount);
        data.WriteUInt32(Money);
        data.WriteUInt32(XP);
        data.WriteUInt64(ArtifactXP);
        data.WriteUInt32(ArtifactCategoryID);
        data.WriteUInt32(Honor);
        data.WriteUInt32(Title);
        data.WriteUInt32(FactionFlags);

        for (int i = 0; i < QuestConst.QuestRewardReputationsCount; ++i)
        {
            data.WriteUInt32(FactionID[i]);
            data.WriteInt32(FactionValue[i]);
            data.WriteInt32(FactionOverride[i]);
            data.WriteInt32(FactionCapIn[i]);
        }

        foreach (var id in SpellCompletionDisplayID)
            data.WriteInt32(id);

        data.WriteUInt32(SpellCompletionID);
        data.WriteUInt32(SkillLineID);
        data.WriteUInt32(NumSkillUps);

        data.WriteUInt32(TreasurePickerID != 0 ? 1u : 0u);
        if (TreasurePickerID != 0)
            data.WriteUInt32(TreasurePickerID);

        foreach (var choice in ChoiceItems)
            choice.Write(data);

        data.WriteBit(IsBoostSpell);
        data.FlushBits();
    }
}

public class QuestChoiceItem
{
    public byte LootItemType;
    public ItemInstance Item = new();
    public uint Quantity;

    public void Read(WorldPacket data)
    {
        data.ResetBitPos();
        LootItemType = data.ReadBits<byte>(2);
        Item.Read(data);
        Quantity = data.ReadUInt32();
    }

    /// <inheritdoc cref="Read(WorldPacket)"/>
    /// <remarks>Generated from the WorldPacket reader; ItemInstanceReaderEquivalenceTests keeps the pair in step.</remarks>
    public void Read(ref SpanPacketReader data)
    {
        data.ResetBitPos();
        LootItemType = data.ReadBits<byte>(2);
        Item.Read(ref data);
        Quantity = data.ReadUInt32();
    }

    public void Write(WorldPacket data)
    {
        data.WriteBits(LootItemType, 2);
        Item.Write(data);
        data.WriteUInt32(Quantity);
    }
}

public struct QuestObjectiveSimple
{
    public uint Id;
    public int ObjectID;
    public int Amount;
    public byte Type;
}

public struct QuestDescEmote
{
    public uint Type;
    public uint Delay;
}

public readonly record struct QuestGiverAcceptQuest(WowGuid128 QuestGiverGUID, uint QuestID, bool StartCheat);

public readonly record struct QuestLogRemoveQuest(byte Slot);

public readonly record struct QuestGiverCloseQuest(int QuestID);

public readonly record struct CloseInteraction(WowGuid128 Guid);

/// <param name="MissingQuestPOIs">
/// Only the populated prefix is on the wire. CypherCore over-allocates a 175-slot array but
/// still reads exactly <c>count</c> ints, so the array here is that long and no longer.
/// </param>
public readonly record struct QuestPOIQuery(int[] MissingQuestPOIs);

public class QuestPOIBlobPoint
{
    public short X;
    public short Y;
    public short Z;
}

public class QuestPOIBlobData
{
    public int BlobIndex;
    public int ObjectiveIndex;
    public int QuestObjectiveID;
    public int QuestObjectID;
    public int MapID;
    public int UiMapID;
    public int Priority;
    public int Flags;
    public int WorldEffectID;
    public int PlayerConditionID;
    public int NavigationPlayerConditionID;
    public int SpawnTrackingID;
    public bool AlwaysAllowMergingBlobs;
    public List<QuestPOIBlobPoint> Points = new();
}

public class QuestPOIData
{
    public int QuestID;
    public List<QuestPOIBlobData> Blobs = new();
}

public class QuestPOIQueryResponse : ServerPacket
{
    public QuestPOIQueryResponse() : base(Opcode.SMSG_QUEST_POI_QUERY_RESPONSE) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(QuestPOIDataStats.Count);
        _worldPacket.WriteInt32(QuestPOIDataStats.Count);

        foreach (QuestPOIData questPOIData in QuestPOIDataStats)
        {
            _worldPacket.WriteInt32(questPOIData.QuestID);
            _worldPacket.WriteInt32(questPOIData.Blobs.Count);

            foreach (QuestPOIBlobData blob in questPOIData.Blobs)
            {
                _worldPacket.WriteInt32(blob.BlobIndex);
                _worldPacket.WriteInt32(blob.ObjectiveIndex);
                _worldPacket.WriteInt32(blob.QuestObjectiveID);
                _worldPacket.WriteInt32(blob.QuestObjectID);
                _worldPacket.WriteInt32(blob.MapID);
                _worldPacket.WriteInt32(blob.UiMapID);
                _worldPacket.WriteInt32(blob.Priority);
                _worldPacket.WriteInt32(blob.Flags);
                _worldPacket.WriteInt32(blob.WorldEffectID);
                _worldPacket.WriteInt32(blob.PlayerConditionID);
                _worldPacket.WriteInt32(blob.NavigationPlayerConditionID);
                _worldPacket.WriteInt32(blob.SpawnTrackingID);
                _worldPacket.WriteInt32(blob.Points.Count);

                foreach (QuestPOIBlobPoint p in blob.Points)
                {
                    _worldPacket.WriteInt16(p.X);
                    _worldPacket.WriteInt16(p.Y);
                    _worldPacket.WriteInt16(p.Z);
                }

                _worldPacket.WriteBit(blob.AlwaysAllowMergingBlobs);
                _worldPacket.FlushBits();
            }
        }
    }

    public List<QuestPOIData> QuestPOIDataStats = new();
}

public readonly record struct QuestGiverStatusQuery(WowGuid128 QuestGiverGUID);

/// <summary>How each client build takes a quest giver's status.</summary>
internal enum QuestGiverStatusEncoding : byte
{
    /// <summary>1.14 and 2.5: QuestGiverStatusModern as a uint32.</summary>
    ClassicEra,
    /// <summary>3.4.3 (8.0+ engine, CypherCore QuestPackets.cs:55): QuestGiverStatusV343 as a uint64.</summary>
    WotLKClassic,
    /// <summary>4.4.2: QuestGiverStatusCata as a uint64.</summary>
    CataClassic,
}

internal static class QuestGiverStatusWire
{
    public const int MaxSize = 8;

    public static ulong Encode(QuestGiverStatusEncoding encoding, QuestGiverStatusModern status) => encoding switch
    {
        QuestGiverStatusEncoding.WotLKClassic => QuestGiverStatusV343Converter.FromModern(status),
        QuestGiverStatusEncoding.CataClassic => QuestGiverStatusCataConverter.FromModern(status),
        _ => (uint)status,
    };

    public static void Write(WorldPacket data, QuestGiverStatusEncoding encoding, ulong encoded)
    {
        if (encoding == QuestGiverStatusEncoding.ClassicEra)
            data.WriteUInt32((uint)encoded);
        else
            data.WriteUInt64(encoded);
    }

    public static void Write(ref SpanPacketWriter writer, QuestGiverStatusEncoding encoding, ulong encoded)
    {
        if (encoding == QuestGiverStatusEncoding.ClassicEra)
            writer.WriteUInt32((uint)encoded);
        else
            writer.WriteUInt64(encoded);
    }
}

public sealed class QuestGiverStatusPkt : ServerPacket, ISpanWritable
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<QuestGiverStatusPkt>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V3_4_3_54261, new StatusLayout(QuestGiverStatusEncoding.ClassicEra)),
        (ClientVersionBuild.V3_4_3_54261, ClientVersionBuild.V4_4_2_60895, new StatusLayout(QuestGiverStatusEncoding.WotLKClassic)),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new StatusLayout(QuestGiverStatusEncoding.CataClassic)));

    private static readonly ServerPacketLayout<QuestGiverStatusPkt> Layout = Layouts.ForRunningClient();

    public QuestGiverStatusPkt() : base(Opcode.SMSG_QUEST_GIVER_STATUS, ConnectionType.Instance)
    {
        QuestGiver = new QuestGiverInfo();
    }

    public override void Write() => Layout.Write(this, _worldPacket);

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + QuestGiverStatusWire.MaxSize;

    public int WriteToSpan(Span<byte> buffer) => Layout.WriteToSpan(this, buffer);

    internal sealed class StatusLayout(QuestGiverStatusEncoding encoding) : ServerPacketLayout<QuestGiverStatusPkt>
    {
        public override void Write(QuestGiverStatusPkt packet, WorldPacket data)
        {
            QuestGiverInfo giver = packet.QuestGiver;
            ulong encoded = QuestGiverStatusWire.Encode(encoding, giver.Status);
            QuestLogMessages.QuestGiverStatusWrite(_melQuest, "", giver.Guid.Low, giver.Guid.High,
                giver.Guid.GetEntry(), giver.Status, encoded, ModernVersion.Build);
            data.WritePackedGuid128(giver.Guid);
            QuestGiverStatusWire.Write(data, encoding, encoded);
        }

        public override int WriteToSpan(QuestGiverStatusPkt packet, Span<byte> buffer)
        {
            QuestGiverInfo giver = packet.QuestGiver;
            ulong encoded = QuestGiverStatusWire.Encode(encoding, giver.Status);
            QuestLogMessages.QuestGiverStatusWrite(_melQuest, "(span)", giver.Guid.Low, giver.Guid.High,
                giver.Guid.GetEntry(), giver.Status, encoded, ModernVersion.Build);
            var writer = new SpanPacketWriter(buffer);
            writer.WritePackedGuid128(giver.Guid.Low, giver.Guid.High);
            QuestGiverStatusWire.Write(ref writer, encoding, encoded);
            return writer.Position;
        }
    }

    public QuestGiverInfo QuestGiver;

    private static readonly Microsoft.Extensions.Logging.ILogger _melQuest = Log.CreateMelLogger(Log.CategoryServer);
}

public sealed class QuestGiverStatusMultiple : ServerPacket, ISpanWritable
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<QuestGiverStatusMultiple>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V3_4_3_54261, new StatusLayout(QuestGiverStatusEncoding.ClassicEra)),
        (ClientVersionBuild.V3_4_3_54261, ClientVersionBuild.V4_4_2_60895, new StatusLayout(QuestGiverStatusEncoding.WotLKClassic)),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new StatusLayout(QuestGiverStatusEncoding.CataClassic)));

    private static readonly ServerPacketLayout<QuestGiverStatusMultiple> Layout = Layouts.ForRunningClient();

    public QuestGiverStatusMultiple() : base(Opcode.SMSG_QUEST_GIVER_STATUS_MULTIPLE, ConnectionType.Instance) { }

    public override void Write() => Layout.Write(this, _worldPacket);

    // Cap for quest givers in view - typically only a handful visible at once
    private const int MaxQuestGivers = 32;
    public int MaxSize => 4 + MaxQuestGivers * (PackedGuidHelper.MaxPackedGuid128Size + QuestGiverStatusWire.MaxSize);

    public int WriteToSpan(Span<byte> buffer) => Layout.WriteToSpan(this, buffer);

    internal sealed class StatusLayout(QuestGiverStatusEncoding encoding) : ServerPacketLayout<QuestGiverStatusMultiple>
    {
        public override void Write(QuestGiverStatusMultiple packet, WorldPacket data)
        {
            QuestLogMessages.QuestGiverStatusMultipleWrite(_melQuest, "", packet.QuestGivers.Count, ModernVersion.Build);
            data.WriteInt32(packet.QuestGivers.Count);
            for (int i = 0; i < packet.QuestGivers.Count; i++)
            {
                QuestGiverInfo questGiver = packet.QuestGivers[i];
                ulong encoded = QuestGiverStatusWire.Encode(encoding, questGiver.Status);
                QuestLogMessages.QuestGiverStatusMultipleEntry(_melQuest, "", i, questGiver.Guid.Low, questGiver.Guid.High,
                    questGiver.Guid.GetEntry(), questGiver.Status, encoded);
                data.WritePackedGuid128(questGiver.Guid);
                QuestGiverStatusWire.Write(data, encoding, encoded);
            }
        }

        public override int WriteToSpan(QuestGiverStatusMultiple packet, Span<byte> buffer)
        {
            if (packet.QuestGivers.Count > MaxQuestGivers)
                return -1;

            QuestLogMessages.QuestGiverStatusMultipleWrite(_melQuest, "(span)", packet.QuestGivers.Count, ModernVersion.Build);
            var writer = new SpanPacketWriter(buffer);
            writer.WriteInt32(packet.QuestGivers.Count);
            for (int i = 0; i < packet.QuestGivers.Count; i++)
            {
                QuestGiverInfo questGiver = packet.QuestGivers[i];
                ulong encoded = QuestGiverStatusWire.Encode(encoding, questGiver.Status);
                QuestLogMessages.QuestGiverStatusMultipleEntry(_melQuest, "(span)", i, questGiver.Guid.Low, questGiver.Guid.High,
                    questGiver.Guid.GetEntry(), questGiver.Status, encoded);
                writer.WritePackedGuid128(questGiver.Guid.Low, questGiver.Guid.High);
                QuestGiverStatusWire.Write(ref writer, encoding, encoded);
            }
            return writer.Position;
        }
    }

    public List<QuestGiverInfo> QuestGivers = new();

    private static readonly Microsoft.Extensions.Logging.ILogger _melQuest = Log.CreateMelLogger(Log.CategoryServer);
}

public class QuestGiverInfo
{
    public QuestGiverInfo() { }
    public QuestGiverInfo(WowGuid128 guid, QuestGiverStatusModern status)
    {
        Guid = guid;
        Status = status;
    }

    public WowGuid128 Guid;
    public QuestGiverStatusModern Status = QuestGiverStatusModern.None;
}

public readonly record struct QuestGiverHello(WowGuid128 QuestGiverGUID);

public sealed class QuestGiverQuestListMessage : ServerPacket
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<QuestGiverQuestListMessage>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V3_4_3_54261, new ClassicEraLayout()),
        (ClientVersionBuild.V3_4_3_54261, ClientVersionBuild.V4_4_2_60895, new WotLKClassicLayout()),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new CataClassicLayout()));

    private static readonly ServerPacketLayout<QuestGiverQuestListMessage> Layout = Layouts.ForRunningClient();

    public QuestGiverQuestListMessage() : base(Opcode.SMSG_QUEST_GIVER_QUEST_LIST_MESSAGE) { }

    public override void Write() => Layout.Write(this, _worldPacket);

    /// <summary>1.14 and 2.5.</summary>
    internal sealed class ClassicEraLayout : ServerPacketLayout<QuestGiverQuestListMessage>
    {
        public override void Write(QuestGiverQuestListMessage packet, WorldPacket data) => packet.WriteClassicEra(data);
    }

    /// <summary>3.4.3.</summary>
    internal sealed class WotLKClassicLayout : ServerPacketLayout<QuestGiverQuestListMessage>
    {
        public override void Write(QuestGiverQuestListMessage packet, WorldPacket data) => packet.WriteWotLKClassic(data);
    }

    /// <summary>4.4.2.</summary>
    internal sealed class CataClassicLayout : ServerPacketLayout<QuestGiverQuestListMessage>
    {
        public override void Write(QuestGiverQuestListMessage packet, WorldPacket data) => packet.WriteCataClassic(data);
    }

    private void WriteClassicEra(WorldPacket data)
    {
        data.WritePackedGuid128(QuestGiverGUID);
        data.WriteUInt32(GreetEmoteDelay);
        data.WriteUInt32(GreetEmoteType);
        data.WriteInt32(QuestOptions.Count);
        data.WriteBits(Greeting.GetByteCount(), 11);
        data.FlushBits();

        foreach (ClientGossipQuest quest in QuestOptions)
            quest.Write(data);

        data.WriteString(Greeting);
    }

    // V3_4_3 (WotLK Classic) wire layout — header is identical to retail; the only
    // difference is each per-quest entry uses the wotlk-shaped ClientGossipText
    // layout (see ClientGossipQuest.WriteWotLK).
    private void WriteWotLKClassic(WorldPacket data)
    {
        data.WritePackedGuid128(QuestGiverGUID);
        data.WriteUInt32(GreetEmoteDelay);
        data.WriteUInt32(GreetEmoteType);
        data.WriteUInt32((uint)QuestOptions.Count);
        data.WriteBits(Greeting.GetByteCount(), 11);
        data.FlushBits();

        foreach (ClientGossipQuest quest in QuestOptions)
            quest.WriteWotLK(data);

        data.WriteString(Greeting);
    }

    // 4.4.2: the 3.4.3 header; each quest entry is the 4.4.2 ClientGossipText.
    private void WriteCataClassic(WorldPacket data)
    {
        data.WritePackedGuid128(QuestGiverGUID);
        data.WriteUInt32(GreetEmoteDelay);
        data.WriteUInt32(GreetEmoteType);
        data.WriteUInt32((uint)QuestOptions.Count);
        data.WriteBits(Greeting.GetByteCount(), 11);
        data.FlushBits();

        foreach (ClientGossipQuest quest in QuestOptions)
            quest.WriteCataClassic(data);

        data.WriteString(Greeting);
    }

    public WowGuid128 QuestGiverGUID;
    public uint GreetEmoteDelay;
    public uint GreetEmoteType;
    public List<ClientGossipQuest> QuestOptions = new();
    public string Greeting = "";
}

public sealed class QuestGiverRequestItems : ServerPacket
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<QuestGiverRequestItems>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V3_4_3_54261, new ClassicEraLayout()),
        (ClientVersionBuild.V3_4_3_54261, ClientVersionBuild.V4_4_2_60895, new WotLKClassicLayout()),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new CataClassicLayout()));

    private static readonly ServerPacketLayout<QuestGiverRequestItems> Layout = Layouts.ForRunningClient();

    public QuestGiverRequestItems() : base(Opcode.SMSG_QUEST_GIVER_REQUEST_ITEMS) { }

    public override void Write() => Layout.Write(this, _worldPacket);

    /// <summary>1.14 and 2.5.</summary>
    internal sealed class ClassicEraLayout : ServerPacketLayout<QuestGiverRequestItems>
    {
        public override void Write(QuestGiverRequestItems packet, WorldPacket data) => packet.WriteClassicEra(data);
    }

    /// <summary>3.4.3.</summary>
    internal sealed class WotLKClassicLayout : ServerPacketLayout<QuestGiverRequestItems>
    {
        public override void Write(QuestGiverRequestItems packet, WorldPacket data) => packet.WriteWotLKClassic(data);
    }

    /// <summary>4.4.2.</summary>
    internal sealed class CataClassicLayout : ServerPacketLayout<QuestGiverRequestItems>
    {
        public override void Write(QuestGiverRequestItems packet, WorldPacket data) => packet.WriteCataClassic(data);
    }

    private void WriteClassicEra(WorldPacket data)
    {
        data.WritePackedGuid128(QuestGiverGUID);
        data.WriteUInt32(QuestGiverCreatureID);
        data.WriteUInt32(QuestID);
        data.WriteUInt32(CompEmoteDelay);
        data.WriteUInt32(CompEmoteType);
        data.WriteUInt32(QuestFlags[0]);
        data.WriteUInt32(QuestFlags[1]);
        data.WriteUInt32(SuggestPartyMembers);
        data.WriteInt32(MoneyToGet);
        data.WriteInt32(Collect.Count);
        data.WriteInt32(Currency.Count);
        data.WriteUInt32(StatusFlags);

        foreach (QuestObjectiveCollect obj in Collect)
        {
            data.WriteUInt32(obj.ObjectID);
            data.WriteUInt32(obj.Amount);
            data.WriteUInt32(obj.Flags);
        }
        foreach (QuestCurrency cur in Currency)
        {
            data.WriteUInt32(cur.CurrencyID);
            data.WriteInt32(cur.Amount);
        }

        data.WriteBit(AutoLaunched);
        data.FlushBits();

        data.WriteBits(QuestTitle.GetByteCount(), 9);
        data.WriteBits(CompletionText.GetByteCount(), 12);

        data.WriteString(QuestTitle);
        data.WriteString(CompletionText);
    }

    // V3_4_3.54261 layout — matches CypherCore Source/Game/Networking/Packets/QuestPackets.cs
    // QuestGiverRequestItems.Write at line 537. Includes QuestFlagsEx2, per-Collect
    // Flags, duplicated CreatureID, and ConditionalCompletionText.Count (=0) —
    // omitting any of these causes V3_4_3 client to read garbage as a count and
    // attempt a multi-TB allocation (`?AUConditionalQuestText@@` OOM crash).
    private void WriteWotLKClassic(WorldPacket data)
    {
        data.WritePackedGuid128(QuestGiverGUID);
        data.WriteInt32((int)QuestGiverCreatureID);
        data.WriteInt32((int)QuestID);
        data.WriteUInt32(CompEmoteDelay);
        data.WriteInt32((int)CompEmoteType);
        data.WriteUInt32(QuestFlags[0]);
        data.WriteUInt32(QuestFlags[1]);
        data.WriteUInt32(0);                   // QuestFlagsEx2
        data.WriteInt32((int)SuggestPartyMembers);
        data.WriteInt32(MoneyToGet);
        data.WriteInt32(Collect.Count);
        data.WriteInt32(Currency.Count);
        data.WriteInt32((int)StatusFlags);

        foreach (QuestObjectiveCollect obj in Collect)
        {
            data.WriteInt32((int)obj.ObjectID);
            data.WriteInt32((int)obj.Amount);
            data.WriteUInt32(obj.Flags);        // V3_4_3 only
        }
        foreach (QuestCurrency cur in Currency)
        {
            data.WriteInt32((int)cur.CurrencyID);
            data.WriteInt32(cur.Amount);
        }

        data.WriteBit(AutoLaunched);
        data.FlushBits();

        data.WriteInt32((int)QuestGiverCreatureID);  // duplicated
        data.WriteInt32(0);                          // ConditionalCompletionText.Count

        data.WriteBits(QuestTitle.GetByteCount(), 9);
        data.WriteBits(CompletionText.GetByteCount(), 12);
        data.FlushBits();

        data.WriteString(QuestTitle);
        data.WriteString(CompletionText);
    }

    // 4.4.2 (TrinityCore cata_classic): the two counts lead, the flags and StatusFlags move up
    // behind the GUID, QuestInfoID follows MoneyToGet and ResetByScheduler follows AutoLaunched.
    private void WriteCataClassic(WorldPacket data)
    {
        data.WriteInt32(Collect.Count);
        data.WriteInt32(Currency.Count);
        data.WritePackedGuid128(QuestGiverGUID);
        data.WriteUInt32(QuestFlags[0]);
        data.WriteUInt32(QuestFlags[1]);
        data.WriteUInt32(0);                   // QuestFlagsEx2
        data.WriteInt32((int)StatusFlags);
        data.WriteInt32((int)QuestGiverCreatureID);
        data.WriteInt32((int)QuestID);
        data.WriteUInt32(CompEmoteDelay);
        data.WriteInt32((int)CompEmoteType);
        data.WriteInt32((int)SuggestPartyMembers);
        data.WriteInt32(MoneyToGet);
        data.WriteInt32(0);                    // QuestInfoID

        foreach (QuestObjectiveCollect obj in Collect)
        {
            data.WriteInt32((int)obj.ObjectID);
            data.WriteInt32((int)obj.Amount);
            data.WriteUInt32(obj.Flags);
        }
        foreach (QuestCurrency cur in Currency)
        {
            data.WriteInt32((int)cur.CurrencyID);
            data.WriteInt32(cur.Amount);
        }

        data.WriteBit(AutoLaunched);
        data.WriteBit(false);                  // ResetByScheduler
        data.FlushBits();

        data.WriteInt32((int)QuestGiverCreatureID);
        data.WriteUInt32(0);                   // ConditionalCompletionText.size

        data.WriteBits(QuestTitle.GetByteCount(), 9);
        data.WriteBits(CompletionText.GetByteCount(), 12);
        data.FlushBits();

        data.WriteString(QuestTitle);
        data.WriteString(CompletionText);
    }

    public WowGuid128 QuestGiverGUID;
    public uint QuestGiverCreatureID;
    public uint QuestID;
    public uint CompEmoteDelay;
    public uint CompEmoteType;
    public bool AutoLaunched;
    public uint SuggestPartyMembers;
    public int MoneyToGet;
    public List<QuestObjectiveCollect> Collect = new();
    public List<QuestCurrency> Currency = new();
    public uint StatusFlags;
    public uint[] QuestFlags = new uint[2];
    public string QuestTitle = "";
    public string CompletionText = "";

    // 3.4.3.54261 Continue is IsQuestCompletable(). 0xFF sets NoRequestOnComplete
    // and leaves the button dead. 0xDF / 0xDB are the pair that actually work.
    public const uint StatusComplete = 0xDF;
    public const uint StatusIncomplete = 0xDB;

    // AC writes completable 0x00 / 0x03. Gossip icon 4 only means "turn-in row".
    public static uint StatusForClient(uint acStatusFlags, bool requiredItemsMet)
    {
        bool acComplete = (acStatusFlags & 3) != 0;
        return (requiredItemsMet && acComplete) ? StatusComplete : StatusIncomplete;
    }
}

public struct QuestObjectiveCollect
{
    public uint ObjectID;
    public uint Amount;
    public uint Flags;
}

public struct QuestCurrency
{
    public uint CurrencyID;
    public int Amount;
}

public readonly record struct QuestGiverRequestReward(WowGuid128 QuestGiverGUID, uint QuestID);

public sealed class QuestGiverOfferRewardMessage : ServerPacket
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<QuestGiverOfferRewardMessage>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V3_4_3_54261, new ClassicEraLayout()),
        (ClientVersionBuild.V3_4_3_54261, ClientVersionBuild.V4_4_2_60895, new WotLKClassicLayout()),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new CataClassicLayout()));

    private static readonly ServerPacketLayout<QuestGiverOfferRewardMessage> Layout = Layouts.ForRunningClient();

    public QuestGiverOfferRewardMessage() : base(Opcode.SMSG_QUEST_GIVER_OFFER_REWARD_MESSAGE) { }

    public override void Write() => Layout.Write(this, _worldPacket);

    /// <summary>1.14 and 2.5.</summary>
    internal sealed class ClassicEraLayout : ServerPacketLayout<QuestGiverOfferRewardMessage>
    {
        public override void Write(QuestGiverOfferRewardMessage packet, WorldPacket data) => packet.WriteClassicEra(data);
    }

    /// <summary>3.4.3.</summary>
    internal sealed class WotLKClassicLayout : ServerPacketLayout<QuestGiverOfferRewardMessage>
    {
        public override void Write(QuestGiverOfferRewardMessage packet, WorldPacket data) => packet.WriteWotLKClassic(data);
    }

    /// <summary>4.4.2.</summary>
    internal sealed class CataClassicLayout : ServerPacketLayout<QuestGiverOfferRewardMessage>
    {
        public override void Write(QuestGiverOfferRewardMessage packet, WorldPacket data) => packet.WriteCataClassic(data);
    }

    private void WriteClassicEra(WorldPacket data)
    {
        QuestData.Write(data);
        data.WriteUInt32(QuestPackageID);
        data.WriteUInt32(PortraitGiver);
        data.WriteUInt32(PortraitGiverMount);
        data.WriteUInt32(PortraitGiverModelSceneID);
        data.WriteUInt32(PortraitTurnIn);

        data.WriteBits(QuestTitle.GetByteCount(), 9);
        data.WriteBits(RewardText.GetByteCount(), 12);
        data.WriteBits(PortraitGiverText.GetByteCount(), 10);
        data.WriteBits(PortraitGiverName.GetByteCount(), 8);
        data.WriteBits(PortraitTurnInText.GetByteCount(), 10);
        data.WriteBits(PortraitTurnInName.GetByteCount(), 8);

        data.WriteString(QuestTitle);
        data.WriteString(RewardText);
        data.WriteString(PortraitGiverText);
        data.WriteString(PortraitGiverName);
        data.WriteString(PortraitTurnInText);
        data.WriteString(PortraitTurnInName);
    }

    // V3_4_3.54261 layout — matches CypherCore Source/Game/Networking/Packets/QuestPackets.cs
    // QuestGiverOfferRewardMessage.Write at line 311. The CRITICAL field versus
    // the simpler retail layout is the int32 ConditionalRewardText.Count = 0 —
    // omitting it causes the V3_4_3 client to read garbage as a count and try
    // to allocate multi-TB arrays (`?AUConditionalQuestText@@` OOM crash).
    // QuestData sub-block delegates to QuestGiverOfferReward.WriteWotLK which
    // emits 3 QuestFlags (FlagsEx2 added) and writes Rewards LAST.
    private void WriteWotLKClassic(WorldPacket data)
    {
        QuestData.WriteWotLK(data);
        data.WriteInt32((int)QuestPackageID);
        data.WriteInt32((int)PortraitGiver);
        data.WriteInt32((int)PortraitGiverMount);
        data.WriteInt32((int)PortraitGiverModelSceneID);
        data.WriteInt32((int)PortraitTurnIn);
        data.WriteInt32((int)QuestData.QuestGiverCreatureID);  // duplicated
        data.WriteInt32(0);                                    // ConditionalRewardText.Count

        data.WriteBits(QuestTitle.GetByteCount(), 9);
        data.WriteBits(RewardText.GetByteCount(), 12);
        data.WriteBits(PortraitGiverText.GetByteCount(), 10);
        data.WriteBits(PortraitGiverName.GetByteCount(), 8);
        data.WriteBits(PortraitTurnInText.GetByteCount(), 10);
        data.WriteBits(PortraitTurnInName.GetByteCount(), 8);
        data.FlushBits();

        data.WriteString(QuestTitle);
        data.WriteString(RewardText);
        data.WriteString(PortraitGiverText);
        data.WriteString(PortraitGiverName);
        data.WriteString(PortraitTurnInText);
        data.WriteString(PortraitTurnInName);
    }

    // 4.4.2: the 3.4.3 shape around the 4.4.2 QuestGiverOfferReward.
    private void WriteCataClassic(WorldPacket data)
    {
        QuestData.WriteCataClassic(data);
        data.WriteInt32((int)QuestPackageID);
        data.WriteInt32((int)PortraitGiver);
        data.WriteInt32((int)PortraitGiverMount);
        data.WriteInt32((int)PortraitGiverModelSceneID);
        data.WriteInt32((int)PortraitTurnIn);
        data.WriteInt32((int)QuestData.QuestGiverCreatureID);
        data.WriteUInt32(0);                                     // ConditionalRewardText.size

        data.WriteBits(QuestTitle.GetByteCount(), 9);
        data.WriteBits(RewardText.GetByteCount(), 12);
        data.WriteBits(PortraitGiverText.GetByteCount(), 10);
        data.WriteBits(PortraitGiverName.GetByteCount(), 8);
        data.WriteBits(PortraitTurnInText.GetByteCount(), 10);
        data.WriteBits(PortraitTurnInName.GetByteCount(), 8);
        data.FlushBits();

        data.WriteString(QuestTitle);
        data.WriteString(RewardText);
        data.WriteString(PortraitGiverText);
        data.WriteString(PortraitGiverName);
        data.WriteString(PortraitTurnInText);
        data.WriteString(PortraitTurnInName);
    }

    public uint PortraitTurnIn;
    public uint PortraitGiver;
    public uint PortraitGiverMount;
    public uint PortraitGiverModelSceneID;
    public string QuestTitle = "";
    public string RewardText = "";
    public string PortraitGiverText = "";
    public string PortraitGiverName = "";
    public string PortraitTurnInText = "";
    public string PortraitTurnInName = "";
    public QuestGiverOfferReward QuestData = new();
    public uint QuestPackageID;
}

public class QuestGiverOfferReward
{
    public void Write(WorldPacket data)
    {
        data.WritePackedGuid128(QuestGiverGUID);
        data.WriteUInt32(QuestGiverCreatureID);
        data.WriteUInt32(QuestID);
        data.WriteUInt32(QuestFlags[0]); // Flags
        data.WriteUInt32(QuestFlags[1]); // FlagsEx
        data.WriteUInt32(SuggestedPartyMembers);

        data.WriteInt32(Emotes.Count);
        foreach (QuestDescEmote emote in Emotes)
        {
            data.WriteUInt32(emote.Type);
            data.WriteUInt32(emote.Delay);
        }

        data.WriteBit(AutoLaunched);
        data.WriteBit(false);   // Unused
        data.FlushBits();

        Rewards.Write(data);
    }

    // V3_4_3.54261 layout — matches CypherCore QuestGiverOfferReward.Write at
    // QuestPackets.cs:1192. Adds QuestFlagsEx2 (3rd uint32 flag, =0) which retail
    // doesn't have.
    public void WriteWotLK(WorldPacket data)
    {
        data.WritePackedGuid128(QuestGiverGUID);
        data.WriteUInt32(QuestGiverCreatureID);
        data.WriteUInt32(QuestID);
        data.WriteUInt32(QuestFlags[0]); // Flags
        data.WriteUInt32(QuestFlags[1]); // FlagsEx
        data.WriteUInt32(0);             // FlagsEx2 (V3_4_3 only)
        data.WriteUInt32(SuggestedPartyMembers);

        data.WriteInt32(Emotes.Count);
        foreach (QuestDescEmote emote in Emotes)
        {
            data.WriteUInt32(emote.Type);
            data.WriteUInt32(emote.Delay);
        }

        data.WriteBit(AutoLaunched);
        data.WriteBit(false);   // Unused
        data.FlushBits();

        Rewards.Write(data);
    }

    // 4.4.2 (TrinityCore cata_classic): the rewards and the emote count lead, the creature and
    // quest ids follow the flags, QuestInfoID follows SuggestedPartyMembers, and ResetByScheduler
    // follows the unused bit.
    public void WriteCataClassic(WorldPacket data)
    {
        Rewards.WriteCataClassic(data);
        data.WriteInt32(Emotes.Count);
        data.WritePackedGuid128(QuestGiverGUID);
        data.WriteUInt32(QuestFlags[0]); // Flags
        data.WriteUInt32(QuestFlags[1]); // FlagsEx
        data.WriteUInt32(0);             // FlagsEx2
        data.WriteUInt32(QuestGiverCreatureID);
        data.WriteUInt32(QuestID);
        data.WriteUInt32(SuggestedPartyMembers);
        data.WriteInt32(0);              // QuestInfoID

        foreach (QuestDescEmote emote in Emotes)
        {
            data.WriteUInt32(emote.Type);
            data.WriteUInt32(emote.Delay);
        }

        data.WriteBit(AutoLaunched);
        data.WriteBit(false);   // Unused
        data.WriteBit(false);   // ResetByScheduler
        data.FlushBits();
    }

    public WowGuid128 QuestGiverGUID;
    public uint QuestGiverCreatureID = 0;
    public uint QuestID = 0;
    public bool AutoLaunched = false;
    public uint SuggestedPartyMembers = 0;
    public QuestRewards Rewards = new();
    public List<QuestDescEmote> Emotes = new();
    public uint[] QuestFlags = new uint[2]; // Flags and FlagsEx
}

/// <param name="Choice">
/// Stays a class. <c>QuestChoiceItem</c> nests <c>ItemInstance</c>, which the outbound path
/// builds field-by-field in 30-odd places, so converting it is outbound work. The struct holds
/// the reference, so one allocation per reward click survives and nothing else does.
/// </param>
public readonly record struct QuestGiverChooseReward(
    WowGuid128 QuestGiverGUID,
    uint QuestID,
    QuestChoiceItem Choice);

public class QuestGiverQuestComplete : ServerPacket
{
    public QuestGiverQuestComplete() : base(Opcode.SMSG_QUEST_GIVER_QUEST_COMPLETE) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(QuestID);
        _worldPacket.WriteUInt32(XPReward);
        _worldPacket.WriteInt64(MoneyReward);
        _worldPacket.WriteUInt32(SkillLineIDReward);
        _worldPacket.WriteUInt32(NumSkillUpsReward);

        _worldPacket.WriteBit(UseQuestReward);
        _worldPacket.WriteBit(LaunchGossip);
        _worldPacket.WriteBit(LaunchQuest);
        _worldPacket.WriteBit(HideChatMessage);

        ItemReward.Write(_worldPacket);
    }

    public uint QuestID;
    public uint XPReward;
    public long MoneyReward;
    public uint SkillLineIDReward;
    public uint NumSkillUpsReward;
    public bool UseQuestReward;
    public bool LaunchGossip;
    public bool LaunchQuest = true;
    public bool HideChatMessage;
    public ItemInstance ItemReward = new();
}

public sealed class DisplayToast : ServerPacket
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<DisplayToast>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V4_4_2_60895, new ByteMethodLayout()),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new IntMethodLayout()));

    private static readonly ServerPacketLayout<DisplayToast> Layout = Layouts.ForRunningClient();

    public DisplayToast() : base(Opcode.SMSG_DISPLAY_TOAST, ConnectionType.Instance) { }

    public override void Write() => Layout.Write(this, _worldPacket);

    /// <summary>Up to 3.4.3; the bits and the item trailer differ from the client's reader there too, see #360.</summary>
    internal sealed class ByteMethodLayout : ServerPacketLayout<DisplayToast>
    {
        public override void Write(DisplayToast packet, WorldPacket data)
        {
            data.WriteUInt64(packet.Quantity);
            data.WriteUInt8(packet.DisplayToastMethod);
            data.WriteUInt32(packet.QuestID);
            data.WriteBit(packet.Mailed);
            data.WriteBits(packet.Type, 2);

            if (packet.Type == 0)
            {
                data.WriteBit(packet.BonusRoll);
                data.FlushBits();
                packet.ItemReward.Write(data);
                data.WriteUInt32(packet.SpecializationID);
                data.WriteUInt32(packet.ItemQuantity);
            }
            else
                data.FlushBits();

            if (packet.Type == 1)
                data.WriteUInt32(packet.CurrencyID);
        }
    }

    /// <summary>
    /// 4.4.2 (TrinityCore cata_classic, and the client's reader): the method is a uint32, an
    /// IsSecondaryResult bit follows the type, and an item toast ends with (LootSpec int32,
    /// Gender int8). Written the 3.4.3 way, every quest turn-in toast read its quest id three
    /// bytes early.
    /// </summary>
    internal sealed class IntMethodLayout : ServerPacketLayout<DisplayToast>
    {
        public override void Write(DisplayToast packet, WorldPacket data)
        {
            data.WriteUInt64(packet.Quantity);
            data.WriteUInt32(packet.DisplayToastMethod);
            data.WriteUInt32(packet.QuestID);
            data.WriteBit(packet.Mailed);
            data.WriteBits(packet.Type, 2);
            data.WriteBit(false);               // IsSecondaryResult

            if (packet.Type == 0)
            {
                data.WriteBit(packet.BonusRoll);
                data.FlushBits();
                packet.ItemReward.Write(data);
                data.WriteInt32((int)packet.SpecializationID);
                data.WriteInt8(0);              // Gender
            }
            else
                data.FlushBits();

            if (packet.Type == 1)
                data.WriteUInt32(packet.CurrencyID);
        }
    }

    public ulong Quantity;
    public byte DisplayToastMethod = 16;
    public uint QuestID;
    public bool Mailed;
    public byte Type;
    public bool BonusRoll;
    public ItemInstance ItemReward = new();
    public uint SpecializationID;
    public uint ItemQuantity;
    public uint CurrencyID;
}

/// <param name="QuestGiverGUID">NPC / GameObject guid for normal quest completion. Player guid for self-completed quests.</param>
/// <param name="FromScript">0 - standart complete quest mode with npc, 1 - auto-complete mode.</param>
public readonly record struct QuestGiverCompleteQuest(
    WowGuid128 QuestGiverGUID,
    uint QuestID,
    bool FromScript);

class QuestGiverQuestFailed : ServerPacket, ISpanWritable
{
    public QuestGiverQuestFailed() : base(Opcode.SMSG_QUEST_GIVER_QUEST_FAILED) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(QuestID);
        _worldPacket.WriteUInt32((uint)Reason);
    }

    public int MaxSize => 8; // 2 uints

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(QuestID);
        writer.WriteUInt32((uint)Reason);
        return writer.Position;
    }

    public uint QuestID;
    public InventoryResult Reason;
}

class QuestGiverInvalidQuest : ServerPacket, ISpanWritable
{
    public QuestGiverInvalidQuest() : base(Opcode.SMSG_QUEST_GIVER_INVALID_QUEST) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32((uint)Reason);
        _worldPacket.WriteInt32(ContributionRewardID);

        _worldPacket.WriteBit(SendErrorMessage);
        _worldPacket.WriteBits(ReasonText.GetByteCount(), 9);
        _worldPacket.FlushBits();

        _worldPacket.WriteString(ReasonText);
    }

    // Cap for reason text - usually short error messages
    private const int MaxReasonTextBytes = 256;
    // uint(4) + int(4) + 10 bits(2) + text
    public int MaxSize => 4 + 4 + 2 + MaxReasonTextBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        int textBytes = Encoding.UTF8.GetByteCount(ReasonText);
        if (textBytes > MaxReasonTextBytes)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32((uint)Reason);
        writer.WriteInt32(ContributionRewardID);
        writer.WriteBit(SendErrorMessage);
        writer.WriteBits((uint)textBytes, 9);
        writer.FlushBits();
        writer.WriteString(ReasonText);
        return writer.Position;
    }

    public QuestFailedReasons Reason;
    public int ContributionRewardID;
    public bool SendErrorMessage = true;
    public string ReasonText = "";
}

class QuestUpdateStatus : ServerPacket, ISpanWritable
{
    public QuestUpdateStatus(Opcode opcode) : base(opcode) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(QuestID);
    }

    public int MaxSize => 4; // uint

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(QuestID);
        return writer.Position;
    }

    public uint QuestID;
}
public class QuestUpdateAddCredit : ServerPacket, ISpanWritable
{
    public QuestUpdateAddCredit() : base(Opcode.SMSG_QUEST_UPDATE_ADD_CREDIT, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(VictimGUID);
        _worldPacket.WriteUInt32(QuestID);
        _worldPacket.WriteInt32(ObjectID);
        _worldPacket.WriteUInt16(Count);
        _worldPacket.WriteUInt16(Required);
        _worldPacket.WriteUInt8((byte)ObjectiveType);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 13; // GUID + uint + int + 2 ushorts + byte

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(VictimGUID.Low, VictimGUID.High);
        writer.WriteUInt32(QuestID);
        writer.WriteInt32(ObjectID);
        writer.WriteUInt16(Count);
        writer.WriteUInt16(Required);
        writer.WriteUInt8((byte)ObjectiveType);
        return writer.Position;
    }

    public WowGuid128 VictimGUID;
    public int ObjectID;
    public uint QuestID;
    public ushort Count;
    public ushort Required;
    public QuestObjectiveType ObjectiveType;
}

class QuestUpdateAddCreditSimple : ServerPacket, ISpanWritable
{
    public QuestUpdateAddCreditSimple() : base(Opcode.SMSG_QUEST_UPDATE_ADD_CREDIT_SIMPLE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(QuestID);
        _worldPacket.WriteInt32(ObjectID);
        _worldPacket.WriteUInt8((byte)ObjectiveType);
    }

    public int MaxSize => 9; // uint + int + byte

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(QuestID);
        writer.WriteInt32(ObjectID);
        writer.WriteUInt8((byte)ObjectiveType);
        return writer.Position;
    }

    public uint QuestID;
    public int ObjectID;
    public QuestObjectiveType ObjectiveType;
}

class QuestConfirmAccept : ServerPacket, ISpanWritable
{
    public QuestConfirmAccept() : base(Opcode.SMSG_QUEST_CONFIRM_ACCEPT) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(QuestID);
        _worldPacket.WritePackedGuid128(InitiatedBy);

        _worldPacket.WriteBits(QuestTitle.GetByteCount(), 10);
        _worldPacket.WriteString(QuestTitle);
    }

    // Cap for quest title - most are well under 128 bytes
    private const int MaxTitleBytes = 128;
    // uint(4) + GUID(18) + 10 bits(2) + title
    public int MaxSize => 4 + PackedGuidHelper.MaxPackedGuid128Size + 2 + MaxTitleBytes;

    public int WriteToSpan(Span<byte> buffer)
    {
        int titleBytes = Encoding.UTF8.GetByteCount(QuestTitle);
        if (titleBytes > MaxTitleBytes)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(QuestID);
        writer.WritePackedGuid128(InitiatedBy.Low, InitiatedBy.High);
        writer.WriteBits((uint)titleBytes, 10);
        writer.WriteString(QuestTitle);
        return writer.Position;
    }

    public WowGuid128 InitiatedBy;
    public uint QuestID;
    public string QuestTitle = string.Empty;
}

public readonly record struct QuestConfirmAcceptResponse(uint QuestID);

public readonly record struct PushQuestToParty(uint QuestID);

class QuestPushResult : ServerPacket, ISpanWritable
{
    public QuestPushResult() : base(Opcode.SMSG_QUEST_PUSH_RESULT) { }

    public override void Write()
    {
        _worldPacket.WritePackedGuid128(SenderGUID);
        _worldPacket.WriteUInt8((byte)Result);
    }

    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 1; // GUID + byte

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(SenderGUID.Low, SenderGUID.High);
        writer.WriteUInt8((byte)Result);
        return writer.Position;
    }

    public WowGuid128 SenderGUID;
    public QuestPushReason Result;
}

public readonly record struct QuestPushResultResponse(WowGuid128 SenderGUID, uint QuestID, QuestPushReason Result);
