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
using System;
using Framework.Constants;
using Framework.GameMath;
using Framework.IO;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public sealed class InitializeFactions : ServerPacket, ISpanWritable
{
    // Per-build entry count. WotLK Classic 3.4.3 expects 1000; legacy modern builds (V1_14, V2_5) keep 400.
    // Reference: HermesProxy-WOTLK fork InitializeFactions.cs:16-19, WPP V3_4_0 ReputationHandler.cs:9.
    private const ushort MaxFactionCount = 1000;

    internal static readonly ServerPacketLayouts<ServerPacketLayout<InitializeFactions>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V4_4_2_60895, new IndexedLayout()),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new FactionIdLayout()));

    private static readonly ServerPacketLayout<InitializeFactions> Layout = Layouts.ForRunningClient();

    public InitializeFactions() : base(Opcode.SMSG_INITIALIZE_FACTIONS, ConnectionType.Instance) { }

    private static ushort GetFactionCount() =>
        (ushort)(ModernVersion.ExpansionVersion >= 3 ? 1000 : 400);

    public override void Write() => Layout.Write(this, _worldPacket);

    // V3_4_3: 1000 × (UInt16 + Int32) + 1000 bits = 6000 + 125 = 6125 bytes max.
    // 4.4.2: two counts, then per faction (Int32 + UInt16 + Int32) and (Int32 + one flushed bit).
    public int MaxSize => 8 + MaxFactionCount * (10 + 5);

    public int WriteToSpan(Span<byte> buffer) => Layout.WriteToSpan(this, buffer);

    /// <summary>Up to 3.4.3: one entry per reputation list index, the whole list every time.</summary>
    internal sealed class IndexedLayout : ServerPacketLayout<InitializeFactions>
    {
        public override void Write(InitializeFactions packet, WorldPacket data)
        {
            ushort count = GetFactionCount();
            bool wide = ModernVersion.ExpansionVersion >= 3;
            for (ushort i = 0; i < count; ++i)
            {
                if (wide)
                    data.WriteUInt16((ushort)packet.FactionFlags[i]);
                else
                    data.WriteUInt8((byte)((ushort)packet.FactionFlags[i] & 0xFF));
                data.WriteInt32(packet.FactionStandings[i]);
            }

            for (ushort i = 0; i < count; ++i)
                data.WriteBit(packet.FactionHasBonus[i]);

            data.FlushBits();
        }

        public override int WriteToSpan(InitializeFactions packet, Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);

            ushort count = GetFactionCount();
            bool wide = ModernVersion.ExpansionVersion >= 3;
            for (ushort i = 0; i < count; ++i)
            {
                if (wide)
                    writer.WriteUInt16((ushort)packet.FactionFlags[i]);
                else
                    writer.WriteUInt8((byte)((ushort)packet.FactionFlags[i] & 0xFF));
                writer.WriteInt32(packet.FactionStandings[i]);
            }

            for (ushort i = 0; i < count; ++i)
                writer.WriteBit(packet.FactionHasBonus[i]);

            writer.FlushBits();
            return writer.Position;
        }
    }

    /// <summary>
    /// 4.4.2 (TrinityCore cata_classic, and the client's reader): a faction count and a bonus
    /// count, then (FactionID, Flags, Standing) per faction and (FactionID, one flushed bit) per
    /// bonus. Faction ids, not list indexes; written as the 3.4.3 array, the client read the first
    /// two flags as counts and the reputation window stayed empty.
    /// </summary>
    internal sealed class FactionIdLayout : ServerPacketLayout<InitializeFactions>
    {
        public override void Write(InitializeFactions packet, WorldPacket data)
        {
            int count = CountKnown();
            data.WriteInt32(count);
            data.WriteInt32(count);
            for (int i = 0; i < MaxFactionCount; ++i)
            {
                int factionId = GameData.GetFactionIdForReputationIndex(i);
                if (factionId == 0)
                    continue;
                data.WriteInt32(factionId);
                data.WriteUInt16((ushort)packet.FactionFlags[i]);
                data.WriteInt32(packet.FactionStandings[i]);
            }

            for (int i = 0; i < MaxFactionCount; ++i)
            {
                int factionId = GameData.GetFactionIdForReputationIndex(i);
                if (factionId == 0)
                    continue;
                data.WriteInt32(factionId);
                data.WriteBit(packet.FactionHasBonus[i]);
                data.FlushBits();
            }
        }

        public override int WriteToSpan(InitializeFactions packet, Span<byte> buffer)
        {
            var writer = new SpanPacketWriter(buffer);
            int count = CountKnown();
            writer.WriteInt32(count);
            writer.WriteInt32(count);
            for (int i = 0; i < MaxFactionCount; ++i)
            {
                int factionId = GameData.GetFactionIdForReputationIndex(i);
                if (factionId == 0)
                    continue;
                writer.WriteInt32(factionId);
                writer.WriteUInt16((ushort)packet.FactionFlags[i]);
                writer.WriteInt32(packet.FactionStandings[i]);
            }

            for (int i = 0; i < MaxFactionCount; ++i)
            {
                int factionId = GameData.GetFactionIdForReputationIndex(i);
                if (factionId == 0)
                    continue;
                writer.WriteInt32(factionId);
                writer.WriteBit(packet.FactionHasBonus[i]);
                writer.FlushBits();
            }
            return writer.Position;
        }

        private static int CountKnown()
        {
            int count = 0;
            for (int i = 0; i < MaxFactionCount; ++i)
                if (GameData.GetFactionIdForReputationIndex(i) != 0)
                    count++;
            return count;
        }
    }

    public int[] FactionStandings = new int[MaxFactionCount];
    public bool[] FactionHasBonus = new bool[MaxFactionCount]; //@todo: implement faction bonus
    public ReputationFlags[] FactionFlags = new ReputationFlags[MaxFactionCount];
}

sealed class SetFactionStanding : ServerPacket, ISpanWritable
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<SetFactionStanding>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V4_4_2_60895, new IndexLayout()),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new FactionIdLayout()));

    private static readonly ServerPacketLayout<SetFactionStanding> Layout = Layouts.ForRunningClient();

    public SetFactionStanding() : base(Opcode.SMSG_SET_FACTION_STANDING, ConnectionType.Instance) { }

    /// <summary>
    /// V3_4_3 dropped the leading ReferAFriendBonus float - native writes only
    /// BonusFromAchievementSystem before the count (Wrathion SetFactionStanding::Write, whose
    /// packet class has no such field; WowPacketParser's V3_4_0 parser agrees). Writing both made
    /// the client read the second float as the faction count, i.e. zero, so a live reputation gain
    /// updated nothing and only appeared after a relog re-seeded standings from
    /// SMSG_INITIALIZE_FACTIONS. Older builds still carry it - WPP's V6_0_2 parser, which covers
    /// V1_14 and V2_5, reads both.
    /// </summary>
    private static bool HasReferAFriendBonus => !ModernVersion.IsWotLKClassicOrLater;

    public override void Write() => Layout.Write(this, _worldPacket);

    // Cap for faction standing changes - usually just a few at once
    private const int MaxFactions = 16;
    // up to 2 floats(8) + count(4) + factions(12 each from 4.4.2) + 1 bit
    public int MaxSize => 8 + 4 + MaxFactions * 12 + 1;

    public int WriteToSpan(Span<byte> buffer) => Layout.WriteToSpan(this, buffer);

    /// <summary>Up to 3.4.3: each entry is (Index, Standing).</summary>
    internal sealed class IndexLayout : ServerPacketLayout<SetFactionStanding>
    {
        public override void Write(SetFactionStanding packet, WorldPacket data)
        {
            if (HasReferAFriendBonus)
                data.WriteFloat(packet.ReferAFriendBonus);
            data.WriteFloat(packet.BonusFromAchievementSystem);

            data.WriteInt32(packet.Factions.Count);
            foreach (FactionStandingData factionStanding in packet.Factions)
                factionStanding.Write(data);

            data.WriteBit(packet.ShowVisual);
            data.FlushBits();
        }

        public override int WriteToSpan(SetFactionStanding packet, Span<byte> buffer)
        {
            if (packet.Factions.Count > MaxFactions)
                return -1;

            var writer = new SpanPacketWriter(buffer);
            if (HasReferAFriendBonus)
                writer.WriteFloat(packet.ReferAFriendBonus);
            writer.WriteFloat(packet.BonusFromAchievementSystem);
            writer.WriteInt32(packet.Factions.Count);
            foreach (FactionStandingData factionStanding in packet.Factions)
            {
                writer.WriteInt32(factionStanding.Index);
                writer.WriteInt32(factionStanding.Standing);
            }
            writer.WriteBit(packet.ShowVisual);
            writer.FlushBits();
            return writer.Position;
        }
    }

    /// <summary>
    /// 4.4.2 (TrinityCore cata_classic, and the client's reader): each entry adds the faction id
    /// after (Index, Standing).
    /// </summary>
    internal sealed class FactionIdLayout : ServerPacketLayout<SetFactionStanding>
    {
        public override void Write(SetFactionStanding packet, WorldPacket data)
        {
            data.WriteFloat(packet.BonusFromAchievementSystem);
            data.WriteInt32(packet.Factions.Count);
            foreach (FactionStandingData factionStanding in packet.Factions)
            {
                data.WriteInt32(factionStanding.Index);
                data.WriteInt32(factionStanding.Standing);
                data.WriteInt32(GameData.GetFactionIdForReputationIndex(factionStanding.Index));
            }

            data.WriteBit(packet.ShowVisual);
            data.FlushBits();
        }

        public override int WriteToSpan(SetFactionStanding packet, Span<byte> buffer)
        {
            if (packet.Factions.Count > MaxFactions)
                return -1;

            var writer = new SpanPacketWriter(buffer);
            writer.WriteFloat(packet.BonusFromAchievementSystem);
            writer.WriteInt32(packet.Factions.Count);
            foreach (FactionStandingData factionStanding in packet.Factions)
            {
                writer.WriteInt32(factionStanding.Index);
                writer.WriteInt32(factionStanding.Standing);
                writer.WriteInt32(GameData.GetFactionIdForReputationIndex(factionStanding.Index));
            }
            writer.WriteBit(packet.ShowVisual);
            writer.FlushBits();
            return writer.Position;
        }
    }

    public float ReferAFriendBonus;
    public float BonusFromAchievementSystem;
    public List<FactionStandingData> Factions = new();
    public bool ShowVisual;
}

struct FactionStandingData
{
    public void Write(WorldPacket data)
    {
        data.WriteInt32(Index);
        data.WriteInt32(Standing);
    }

    public int Index;
    public int Standing;
}

public readonly record struct SetFactionAtWar(byte FactionIndex);

public readonly record struct SetFactionNotAtWar(byte FactionIndex);

public readonly record struct SetFactionInactive(uint FactionIndex, bool State);

public readonly record struct SetWatchedFaction(uint FactionIndex);

class SetForcedReactions : ServerPacket, ISpanWritable
{
    public SetForcedReactions() : base(Opcode.SMSG_SET_FORCED_REACTIONS, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteInt32(Reactions.Count);
        foreach (ForcedReaction reaction in Reactions)
            reaction.Write(_worldPacket);
    }

    // Cap for forced reactions - rarely more than a few
    private const int MaxReactions = 8;
    // count(4) + reactions(8 each)
    public int MaxSize => 4 + MaxReactions * 8;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (Reactions.Count > MaxReactions)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteInt32(Reactions.Count);
        foreach (ForcedReaction reaction in Reactions)
        {
            writer.WriteInt32(reaction.Faction);
            writer.WriteInt32(reaction.Reaction);
        }
        return writer.Position;
    }

    public List<ForcedReaction> Reactions = new();
}

struct ForcedReaction
{
    public void Write(WorldPacket data)
    {
        data.WriteInt32(Faction);
        data.WriteInt32(Reaction);
    }

    public int Faction;
    public int Reaction;
}

class SetFactionVisible : ServerPacket, ISpanWritable
{
    public SetFactionVisible(bool visible) : base(visible ? Opcode.SMSG_SET_FACTION_VISIBLE : Opcode.SMSG_SET_FACTION_NOT_VISIBLE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(FactionIndex);
    }

    public int MaxSize => 4; // uint

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(FactionIndex);
        return writer.Position;
    }

    public uint FactionIndex;
}
