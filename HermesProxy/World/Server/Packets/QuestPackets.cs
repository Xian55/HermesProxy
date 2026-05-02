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
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace HermesProxy.World.Server.Packets;

public class QuestGiverQueryQuest : ClientPacket
{
    public QuestGiverQueryQuest(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        QuestGiverGUID = _worldPacket.ReadPackedGuid128();
        QuestID = _worldPacket.ReadUInt32();
        RespondToGiver = _worldPacket.HasBit();
    }

    public WowGuid128 QuestGiverGUID;
    public uint QuestID;
    public bool RespondToGiver;
}

public class QuestGiverQuestDetails : ServerPacket
{
    public QuestGiverQuestDetails() : base(Opcode.SMSG_QUEST_GIVER_QUEST_DETAILS)
    {
        for (int i = 0; i < QuestConst.QuestRewardReputationsCount; i++)
            Rewards.FactionCapIn[i] = 7;
    }

    public override void Write()
    {
        // V3_4_3 (WotLK Classic) wire layout adds QuestFlags[2], QuestInfoID,
        // QuestGiverCreatureID, ConditionalDescriptionText.size, reorders the
        // Objectives field as (Id, Type:i32, ObjectID, Amount), and rebuilds
        // QuestRewards. Without this dispatch the client mis-reads a count and
        // attempts a ~112 GB allocation. Layout mirrors TC wotlk_classic
        // QuestPackets.cpp:458 exactly.
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            WriteWotLK();
            return;
        }

        _worldPacket.WritePackedGuid128(QuestGiverGUID);
        _worldPacket.WritePackedGuid128(InformUnit);
        _worldPacket.WriteUInt32(QuestID);
        _worldPacket.WriteInt32(QuestPackageID);
        _worldPacket.WriteUInt32(PortraitGiver);
        _worldPacket.WriteUInt32(PortraitGiverMount);
        _worldPacket.WriteUInt32(PortraitGiverModelSceneID);
        _worldPacket.WriteUInt32(PortraitTurnIn);
        _worldPacket.WriteUInt32(QuestFlags[0]); // Flags
        _worldPacket.WriteUInt32(QuestFlags[1]); // FlagsEx
        _worldPacket.WriteUInt32(SuggestedPartyMembers);
        _worldPacket.WriteInt32(LearnSpells.Count);
        _worldPacket.WriteInt32(DescEmotes.Length);
        _worldPacket.WriteInt32(Objectives.Count);
        _worldPacket.WriteInt32(QuestStartItemID);
        _worldPacket.WriteInt32(QuestSessionBonus);

        foreach (uint spell in LearnSpells)
            _worldPacket.WriteUInt32(spell);

        foreach (QuestDescEmote emote in DescEmotes)
        {
            _worldPacket.WriteUInt32(emote.Type);
            _worldPacket.WriteUInt32(emote.Delay);
        }

        foreach (QuestObjectiveSimple obj in Objectives)
        {
            _worldPacket.WriteUInt32(obj.Id);
            _worldPacket.WriteInt32(obj.ObjectID);
            _worldPacket.WriteInt32(obj.Amount);
            _worldPacket.WriteUInt8(obj.Type);
        }

        _worldPacket.WriteBits(QuestTitle.GetByteCount(), 9);
        _worldPacket.WriteBits(DescriptionText.GetByteCount(), 12);
        _worldPacket.WriteBits(LogDescription.GetByteCount(), 12);
        _worldPacket.WriteBits(PortraitGiverText.GetByteCount(), 10);
        _worldPacket.WriteBits(PortraitGiverName.GetByteCount(), 8);
        _worldPacket.WriteBits(PortraitTurnInText.GetByteCount(), 10);
        _worldPacket.WriteBits(PortraitTurnInName.GetByteCount(), 8);
        _worldPacket.WriteBit(AutoLaunched);
        _worldPacket.WriteBit(false);   // unused in client
        _worldPacket.WriteBit(StartCheat);
        _worldPacket.WriteBit(DisplayPopup);
        _worldPacket.FlushBits();

        Rewards.Write(_worldPacket);

        _worldPacket.WriteString(QuestTitle);
        _worldPacket.WriteString(DescriptionText);
        _worldPacket.WriteString(LogDescription);
        _worldPacket.WriteString(PortraitGiverText);
        _worldPacket.WriteString(PortraitGiverName);
        _worldPacket.WriteString(PortraitTurnInText);
        _worldPacket.WriteString(PortraitTurnInName);
    }

    // Mirrors WPP V3_4_0 SMSG_QUEST_GIVER_QUEST_DETAILS parser (range
    // V3_4_3_51505 → V3_4_4_59817). Diff vs retail Write: adds QuestFlags[2],
    // QuestGiverCreatureID, ConditionalDescriptionTextCount; QuestRewards block
    // is the same as retail (NOT the V3_4_4+ shape). Objectives have the same
    // (Id, ObjectID, Amount, Type:u8) shape as retail. Newer V3_4_4 layouts
    // (QuestInfoID, Items-first QuestRewards, Objective Type:i32 reordered)
    // are NOT present in build 54261.
    private void WriteWotLK()
    {
        _worldPacket.WritePackedGuid128(QuestGiverGUID);
        _worldPacket.WritePackedGuid128(InformUnit);
        _worldPacket.WriteInt32((int)QuestID);
        _worldPacket.WriteInt32(QuestPackageID);
        _worldPacket.WriteInt32((int)PortraitGiver);
        _worldPacket.WriteInt32((int)PortraitGiverMount);
        _worldPacket.WriteInt32((int)PortraitGiverModelSceneID);
        _worldPacket.WriteInt32((int)PortraitTurnIn);
        _worldPacket.WriteUInt32(QuestFlags[0]);    // Flags
        _worldPacket.WriteUInt32(QuestFlags[1]);    // FlagsEx
        _worldPacket.WriteUInt32(0);                // FlagsEx2 (V3_4_3 only)
        _worldPacket.WriteInt32((int)SuggestedPartyMembers);
        _worldPacket.WriteUInt32((uint)LearnSpells.Count);
        _worldPacket.WriteUInt32((uint)DescEmotes.Length);
        _worldPacket.WriteUInt32((uint)Objectives.Count);
        _worldPacket.WriteInt32(QuestStartItemID);
        _worldPacket.WriteInt32(QuestSessionBonus);
        _worldPacket.WriteInt32(0);                 // QuestGiverCreatureID
        _worldPacket.WriteUInt32(0);                // ConditionalDescriptionText.size

        foreach (uint spell in LearnSpells)
            _worldPacket.WriteInt32((int)spell);

        foreach (QuestDescEmote emote in DescEmotes)
        {
            _worldPacket.WriteInt32((int)emote.Type);
            _worldPacket.WriteUInt32(emote.Delay);
        }

        foreach (QuestObjectiveSimple obj in Objectives)
        {
            _worldPacket.WriteUInt32(obj.Id);
            _worldPacket.WriteInt32(obj.ObjectID);
            _worldPacket.WriteInt32(obj.Amount);
            _worldPacket.WriteUInt8(obj.Type);
        }

        _worldPacket.WriteBits(QuestTitle.GetByteCount(), 9);
        _worldPacket.WriteBits(DescriptionText.GetByteCount(), 12);
        _worldPacket.WriteBits(LogDescription.GetByteCount(), 12);
        _worldPacket.WriteBits(PortraitGiverText.GetByteCount(), 10);
        _worldPacket.WriteBits(PortraitGiverName.GetByteCount(), 8);
        _worldPacket.WriteBits(PortraitTurnInText.GetByteCount(), 10);
        _worldPacket.WriteBits(PortraitTurnInName.GetByteCount(), 8);
        _worldPacket.WriteBit(AutoLaunched);
        _worldPacket.WriteBit(false);
        _worldPacket.WriteBit(StartCheat);
        _worldPacket.WriteBit(DisplayPopup);
        _worldPacket.FlushBits();

        Rewards.Write(_worldPacket);

        _worldPacket.WriteString(QuestTitle);
        _worldPacket.WriteString(DescriptionText);
        _worldPacket.WriteString(LogDescription);
        _worldPacket.WriteString(PortraitGiverText);
        _worldPacket.WriteString(PortraitGiverName);
        _worldPacket.WriteString(PortraitTurnInText);
        _worldPacket.WriteString(PortraitTurnInName);
    }

    public WowGuid128 QuestGiverGUID;
    public WowGuid128 InformUnit;
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

public class QuestGiverAcceptQuest : ClientPacket
{
    public QuestGiverAcceptQuest(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        QuestGiverGUID = _worldPacket.ReadPackedGuid128();
        QuestID = _worldPacket.ReadUInt32();
        StartCheat = _worldPacket.HasBit();
    }

    public WowGuid128 QuestGiverGUID;
    public uint QuestID;
    public bool StartCheat;

}

public class QuestLogRemoveQuest : ClientPacket
{
    public QuestLogRemoveQuest(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        Slot = _worldPacket.ReadUInt8();
    }

    public byte Slot;
}

public class QuestGiverCloseQuest : ClientPacket
{
    public QuestGiverCloseQuest(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        QuestID = _worldPacket.ReadInt32();
    }

    public int QuestID;
}

public class QuestPOIQuery : ClientPacket
{
    public QuestPOIQuery(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        // Wire: int32 count, int32[count] questIds. CypherCore over-allocates a
        // 175-slot array but only reads `count` ints from the stream — only the
        // populated prefix is on the wire.
        int count = _worldPacket.ReadInt32();
        MissingQuestPOIs = new int[count];
        for (int i = 0; i < count; i++)
            MissingQuestPOIs[i] = _worldPacket.ReadInt32();
    }

    public int[] MissingQuestPOIs = Array.Empty<int>();
}

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

public class QuestGiverStatusQuery : ClientPacket
{
    public QuestGiverStatusQuery(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        QuestGiverGUID = _worldPacket.ReadPackedGuid128();
    }

    public WowGuid128 QuestGiverGUID;
}

public class QuestGiverStatusMultipleQuery : ClientPacket
{
    public QuestGiverStatusMultipleQuery(WorldPacket packet) : base(packet) { }

    public override void Read() { }
}

public class QuestGiverStatusPkt : ServerPacket, ISpanWritable
{
    public QuestGiverStatusPkt() : base(Opcode.SMSG_QUEST_GIVER_STATUS, ConnectionType.Instance)
    {
        QuestGiver = new QuestGiverInfo();
    }

    public override void Write()
    {
        bool useV343 = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261;
        uint encoded = useV343
            ? QuestGiverStatusV343Converter.FromModern(QuestGiver.Status)
            : (uint)QuestGiver.Status;
        Log.Print(LogType.Trace,
            $"[QuestStatusTrace] SMSG_QUEST_GIVER_STATUS write: GUID={QuestGiver.Guid} entry={QuestGiver.Guid.GetEntry()} " +
            $"modern={QuestGiver.Status} (0x{encoded:X}) build={ModernVersion.Build}");
        _worldPacket.WritePackedGuid128(QuestGiver.Guid);
        // V3_4_3 (8.0+ engine) widened the status field to uint64 — see CypherCore
        // QuestPackets.cs:55. Older clients still use uint32.
        if (useV343)
            _worldPacket.WriteUInt64(encoded);
        else
            _worldPacket.WriteUInt32(encoded);
    }

    // GUID(18) + status(8 V3_4_3 / 4 retail) — use 8 to be safe across versions.
    public int MaxSize => PackedGuidHelper.MaxPackedGuid128Size + 8;

    public int WriteToSpan(Span<byte> buffer)
    {
        bool useV343 = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261;
        uint encoded = useV343
            ? QuestGiverStatusV343Converter.FromModern(QuestGiver.Status)
            : (uint)QuestGiver.Status;
        Log.Print(LogType.Trace,
            $"[QuestStatusTrace] SMSG_QUEST_GIVER_STATUS write(span): GUID={QuestGiver.Guid} entry={QuestGiver.Guid.GetEntry()} " +
            $"modern={QuestGiver.Status} (0x{encoded:X}) build={ModernVersion.Build}");
        var writer = new SpanPacketWriter(buffer);
        writer.WritePackedGuid128(QuestGiver.Guid.Low, QuestGiver.Guid.High);
        if (useV343)
            writer.WriteUInt64(encoded);
        else
            writer.WriteUInt32(encoded);
        return writer.Position;
    }

    public QuestGiverInfo QuestGiver;
}

public class QuestGiverStatusMultiple : ServerPacket, ISpanWritable
{
    public QuestGiverStatusMultiple() : base(Opcode.SMSG_QUEST_GIVER_STATUS_MULTIPLE, ConnectionType.Instance) { }

    public override void Write()
    {
        bool useV343 = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261;
        Log.Print(LogType.Trace,
            $"[QuestStatusTrace] SMSG_QUEST_GIVER_STATUS_MULTIPLE write: count={QuestGivers.Count} build={ModernVersion.Build}");
        _worldPacket.WriteInt32(QuestGivers.Count);
        for (int i = 0; i < QuestGivers.Count; i++)
        {
            QuestGiverInfo questGiver = QuestGivers[i];
            uint encoded = useV343
                ? QuestGiverStatusV343Converter.FromModern(questGiver.Status)
                : (uint)questGiver.Status;
            Log.Print(LogType.Trace,
                $"[QuestStatusTrace]   [{i}] GUID={questGiver.Guid} entry={questGiver.Guid.GetEntry()} " +
                $"modern={questGiver.Status} (0x{encoded:X})");
            _worldPacket.WritePackedGuid128(questGiver.Guid);
            // V3_4_3 widened status to uint64 — CypherCore QuestPackets.cs:71.
            if (useV343)
                _worldPacket.WriteUInt64(encoded);
            else
                _worldPacket.WriteUInt32(encoded);
        }
    }

    // Cap for quest givers in view - typically only a handful visible at once
    private const int MaxQuestGivers = 32;
    // Each entry: PackedGuid128 (18) + status (8 V3_4_3 / 4 retail) — use 8 to be safe.
    public int MaxSize => 4 + MaxQuestGivers * (PackedGuidHelper.MaxPackedGuid128Size + 8);

    public int WriteToSpan(Span<byte> buffer)
    {
        if (QuestGivers.Count > MaxQuestGivers)
            return -1;

        bool useV343 = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261;
        Log.Print(LogType.Trace,
            $"[QuestStatusTrace] SMSG_QUEST_GIVER_STATUS_MULTIPLE write(span): count={QuestGivers.Count} build={ModernVersion.Build}");
        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt32(QuestGivers.Count);
        for (int i = 0; i < QuestGivers.Count; i++)
        {
            QuestGiverInfo questGiver = QuestGivers[i];
            uint encoded = useV343
                ? QuestGiverStatusV343Converter.FromModern(questGiver.Status)
                : (uint)questGiver.Status;
            Log.Print(LogType.Trace,
                $"[QuestStatusTrace]   (span)[{i}] GUID={questGiver.Guid} entry={questGiver.Guid.GetEntry()} " +
                $"modern={questGiver.Status} (0x{encoded:X})");
            writer.WritePackedGuid128(questGiver.Guid.Low, questGiver.Guid.High);
            if (useV343)
                writer.WriteUInt64(encoded);
            else
                writer.WriteUInt32(encoded);
        }
        return writer.Position;
    }

    public List<QuestGiverInfo> QuestGivers = new();
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

public class QuestGiverHello : ClientPacket
{
    public QuestGiverHello(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        QuestGiverGUID = _worldPacket.ReadPackedGuid128();
    }

    public WowGuid128 QuestGiverGUID;
}

public class QuestGiverQuestListMessage : ServerPacket
{
    public QuestGiverQuestListMessage() : base(Opcode.SMSG_QUEST_GIVER_QUEST_LIST_MESSAGE) { }

    public override void Write()
    {
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            WriteWotLK();
            return;
        }

        _worldPacket.WritePackedGuid128(QuestGiverGUID);
        _worldPacket.WriteUInt32(GreetEmoteDelay);
        _worldPacket.WriteUInt32(GreetEmoteType);
        _worldPacket.WriteInt32(QuestOptions.Count);
        _worldPacket.WriteBits(Greeting.GetByteCount(), 11);
        _worldPacket.FlushBits();

        foreach (ClientGossipQuest quest in QuestOptions)
            quest.Write(_worldPacket);

        _worldPacket.WriteString(Greeting);
    }

    // V3_4_3 (WotLK Classic) wire layout — header is identical to retail; the only
    // difference is each per-quest entry uses the wotlk-shaped ClientGossipText
    // layout (see ClientGossipQuest.WriteWotLK).
    private void WriteWotLK()
    {
        _worldPacket.WritePackedGuid128(QuestGiverGUID);
        _worldPacket.WriteUInt32(GreetEmoteDelay);
        _worldPacket.WriteUInt32(GreetEmoteType);
        _worldPacket.WriteUInt32((uint)QuestOptions.Count);
        _worldPacket.WriteBits(Greeting.GetByteCount(), 11);
        _worldPacket.FlushBits();

        foreach (ClientGossipQuest quest in QuestOptions)
            quest.WriteWotLK(_worldPacket);

        _worldPacket.WriteString(Greeting);
    }

    public WowGuid128 QuestGiverGUID;
    public uint GreetEmoteDelay;
    public uint GreetEmoteType;
    public List<ClientGossipQuest> QuestOptions = new();
    public string Greeting = "";
}

public class QuestGiverRequestItems : ServerPacket
{
    public QuestGiverRequestItems() : base(Opcode.SMSG_QUEST_GIVER_REQUEST_ITEMS) { }

    public override void Write()
    {
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            WriteWotLK();
            return;
        }

        _worldPacket.WritePackedGuid128(QuestGiverGUID);
        _worldPacket.WriteUInt32(QuestGiverCreatureID);
        _worldPacket.WriteUInt32(QuestID);
        _worldPacket.WriteUInt32(CompEmoteDelay);
        _worldPacket.WriteUInt32(CompEmoteType);
        _worldPacket.WriteUInt32(QuestFlags[0]);
        _worldPacket.WriteUInt32(QuestFlags[1]);
        _worldPacket.WriteUInt32(SuggestPartyMembers);
        _worldPacket.WriteInt32(MoneyToGet);
        _worldPacket.WriteInt32(Collect.Count);
        _worldPacket.WriteInt32(Currency.Count);
        _worldPacket.WriteUInt32(StatusFlags);

        foreach (QuestObjectiveCollect obj in Collect)
        {
            _worldPacket.WriteUInt32(obj.ObjectID);
            _worldPacket.WriteUInt32(obj.Amount);
            _worldPacket.WriteUInt32(obj.Flags);
        }
        foreach (QuestCurrency cur in Currency)
        {
            _worldPacket.WriteUInt32(cur.CurrencyID);
            _worldPacket.WriteInt32(cur.Amount);
        }

        _worldPacket.WriteBit(AutoLaunched);
        _worldPacket.FlushBits();

        _worldPacket.WriteBits(QuestTitle.GetByteCount(), 9);
        _worldPacket.WriteBits(CompletionText.GetByteCount(), 12);

        _worldPacket.WriteString(QuestTitle);
        _worldPacket.WriteString(CompletionText);
    }

    // V3_4_3.54261 layout — matches CypherCore Source/Game/Networking/Packets/QuestPackets.cs
    // QuestGiverRequestItems.Write at line 537. Includes QuestFlagsEx2, per-Collect
    // Flags, duplicated CreatureID, and ConditionalCompletionText.Count (=0) —
    // omitting any of these causes V3_4_3 client to read garbage as a count and
    // attempt a multi-TB allocation (`?AUConditionalQuestText@@` OOM crash).
    private void WriteWotLK()
    {
        _worldPacket.WritePackedGuid128(QuestGiverGUID);
        _worldPacket.WriteInt32((int)QuestGiverCreatureID);
        _worldPacket.WriteInt32((int)QuestID);
        _worldPacket.WriteUInt32(CompEmoteDelay);
        _worldPacket.WriteInt32((int)CompEmoteType);
        _worldPacket.WriteUInt32(QuestFlags[0]);
        _worldPacket.WriteUInt32(QuestFlags[1]);
        _worldPacket.WriteUInt32(0);                   // QuestFlagsEx2
        _worldPacket.WriteInt32((int)SuggestPartyMembers);
        _worldPacket.WriteInt32(MoneyToGet);
        _worldPacket.WriteInt32(Collect.Count);
        _worldPacket.WriteInt32(Currency.Count);
        _worldPacket.WriteInt32((int)StatusFlags);

        foreach (QuestObjectiveCollect obj in Collect)
        {
            _worldPacket.WriteInt32((int)obj.ObjectID);
            _worldPacket.WriteInt32((int)obj.Amount);
            _worldPacket.WriteUInt32(obj.Flags);        // V3_4_3 only
        }
        foreach (QuestCurrency cur in Currency)
        {
            _worldPacket.WriteInt32((int)cur.CurrencyID);
            _worldPacket.WriteInt32(cur.Amount);
        }

        _worldPacket.WriteBit(AutoLaunched);
        _worldPacket.FlushBits();

        _worldPacket.WriteInt32((int)QuestGiverCreatureID);  // duplicated
        _worldPacket.WriteInt32(0);                          // ConditionalCompletionText.Count

        _worldPacket.WriteBits(QuestTitle.GetByteCount(), 9);
        _worldPacket.WriteBits(CompletionText.GetByteCount(), 12);
        _worldPacket.FlushBits();

        _worldPacket.WriteString(QuestTitle);
        _worldPacket.WriteString(CompletionText);
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

public class QuestGiverRequestReward : ClientPacket
{
    public QuestGiverRequestReward(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        QuestGiverGUID = _worldPacket.ReadPackedGuid128();
        QuestID = _worldPacket.ReadUInt32();
    }

    public WowGuid128 QuestGiverGUID;
    public uint QuestID;
}

public class QuestGiverOfferRewardMessage : ServerPacket
{
    public QuestGiverOfferRewardMessage() : base(Opcode.SMSG_QUEST_GIVER_OFFER_REWARD_MESSAGE) { }

    public override void Write()
    {
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            WriteWotLK();
            return;
        }

        QuestData.Write(_worldPacket);
        _worldPacket.WriteUInt32(QuestPackageID);
        _worldPacket.WriteUInt32(PortraitGiver);
        _worldPacket.WriteUInt32(PortraitGiverMount);
        _worldPacket.WriteUInt32(PortraitGiverModelSceneID);
        _worldPacket.WriteUInt32(PortraitTurnIn);

        _worldPacket.WriteBits(QuestTitle.GetByteCount(), 9);
        _worldPacket.WriteBits(RewardText.GetByteCount(), 12);
        _worldPacket.WriteBits(PortraitGiverText.GetByteCount(), 10);
        _worldPacket.WriteBits(PortraitGiverName.GetByteCount(), 8);
        _worldPacket.WriteBits(PortraitTurnInText.GetByteCount(), 10);
        _worldPacket.WriteBits(PortraitTurnInName.GetByteCount(), 8);

        _worldPacket.WriteString(QuestTitle);
        _worldPacket.WriteString(RewardText);
        _worldPacket.WriteString(PortraitGiverText);
        _worldPacket.WriteString(PortraitGiverName);
        _worldPacket.WriteString(PortraitTurnInText);
        _worldPacket.WriteString(PortraitTurnInName);
    }

    // V3_4_3.54261 layout — matches CypherCore Source/Game/Networking/Packets/QuestPackets.cs
    // QuestGiverOfferRewardMessage.Write at line 311. The CRITICAL field versus
    // the simpler retail layout is the int32 ConditionalRewardText.Count = 0 —
    // omitting it causes the V3_4_3 client to read garbage as a count and try
    // to allocate multi-TB arrays (`?AUConditionalQuestText@@` OOM crash).
    // QuestData sub-block delegates to QuestGiverOfferReward.WriteWotLK which
    // emits 3 QuestFlags (FlagsEx2 added) and writes Rewards LAST.
    private void WriteWotLK()
    {
        QuestData.WriteWotLK(_worldPacket);
        _worldPacket.WriteInt32((int)QuestPackageID);
        _worldPacket.WriteInt32((int)PortraitGiver);
        _worldPacket.WriteInt32((int)PortraitGiverMount);
        _worldPacket.WriteInt32((int)PortraitGiverModelSceneID);
        _worldPacket.WriteInt32((int)PortraitTurnIn);
        _worldPacket.WriteInt32((int)QuestData.QuestGiverCreatureID);  // duplicated
        _worldPacket.WriteInt32(0);                                    // ConditionalRewardText.Count

        _worldPacket.WriteBits(QuestTitle.GetByteCount(), 9);
        _worldPacket.WriteBits(RewardText.GetByteCount(), 12);
        _worldPacket.WriteBits(PortraitGiverText.GetByteCount(), 10);
        _worldPacket.WriteBits(PortraitGiverName.GetByteCount(), 8);
        _worldPacket.WriteBits(PortraitTurnInText.GetByteCount(), 10);
        _worldPacket.WriteBits(PortraitTurnInName.GetByteCount(), 8);
        _worldPacket.FlushBits();

        _worldPacket.WriteString(QuestTitle);
        _worldPacket.WriteString(RewardText);
        _worldPacket.WriteString(PortraitGiverText);
        _worldPacket.WriteString(PortraitGiverName);
        _worldPacket.WriteString(PortraitTurnInText);
        _worldPacket.WriteString(PortraitTurnInName);
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


    public WowGuid128 QuestGiverGUID;
    public uint QuestGiverCreatureID = 0;
    public uint QuestID = 0;
    public bool AutoLaunched = false;
    public uint SuggestedPartyMembers = 0;
    public QuestRewards Rewards = new();
    public List<QuestDescEmote> Emotes = new();
    public uint[] QuestFlags = new uint[2]; // Flags and FlagsEx
}

public class QuestGiverChooseReward : ClientPacket
{
    public QuestGiverChooseReward(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        QuestGiverGUID = _worldPacket.ReadPackedGuid128();
        QuestID = _worldPacket.ReadUInt32();
        Choice.Read(_worldPacket);
    }

    public WowGuid128 QuestGiverGUID;
    public uint QuestID;
    public QuestChoiceItem Choice = new();
}

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

public class DisplayToast : ServerPacket
{
    public DisplayToast() : base(Opcode.SMSG_DISPLAY_TOAST, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt64(Quantity);
        _worldPacket.WriteUInt8(DisplayToastMethod);
        _worldPacket.WriteUInt32(QuestID);
        _worldPacket.WriteBit(Mailed);
        _worldPacket.WriteBits(Type, 2);

        if (Type == 0)
        {
            _worldPacket.WriteBit(BonusRoll);
            _worldPacket.FlushBits();
            ItemReward.Write(_worldPacket);
            _worldPacket.WriteUInt32(SpecializationID);
            _worldPacket.WriteUInt32(ItemQuantity);
        }
        else
            _worldPacket.FlushBits();

        if (Type == 1)
            _worldPacket.WriteUInt32(CurrencyID);
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

public class QuestGiverCompleteQuest : ClientPacket
{
    public QuestGiverCompleteQuest(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        QuestGiverGUID = _worldPacket.ReadPackedGuid128();
        QuestID = _worldPacket.ReadUInt32();
        FromScript = _worldPacket.HasBit();
    }

    public WowGuid128 QuestGiverGUID; // NPC / GameObject guid for normal quest completion. Player guid for self-completed quests
    public uint QuestID;
    public bool FromScript; // 0 - standart complete quest mode with npc, 1 - auto-complete mode
}

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

class QuestConfirmAcceptResponse : ClientPacket
{
    public QuestConfirmAcceptResponse(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        QuestID = _worldPacket.ReadUInt32();
    }

    public uint QuestID;
}

class PushQuestToParty : ClientPacket
{
    public PushQuestToParty(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        QuestID = _worldPacket.ReadUInt32();
    }

    public uint QuestID;
}

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

class QuestPushResultResponse : ClientPacket
{
    public QuestPushResultResponse(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        SenderGUID = _worldPacket.ReadPackedGuid128();
        QuestID = _worldPacket.ReadUInt32();
        Result = (QuestPushReason)_worldPacket.ReadUInt8();
    }

    public WowGuid128 SenderGUID;
    public uint QuestID;
    public QuestPushReason Result;
}
