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
using Framework.Constants;
using Framework.GameMath;
using Framework.IO;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class InitializeFactions : ServerPacket, ISpanWritable
{
    // Per-build entry count. WotLK Classic 3.4.3 expects 1000; legacy modern builds (V1_14, V2_5) keep 400.
    // Reference: HermesProxy-WOTLK fork InitializeFactions.cs:16-19, WPP V3_4_0 ReputationHandler.cs:9.
    private const ushort MaxFactionCount = 1000;

    public InitializeFactions() : base(Opcode.SMSG_INITIALIZE_FACTIONS, ConnectionType.Instance) { }

    private static ushort GetFactionCount() =>
        (ushort)(ModernVersion.ExpansionVersion >= 3 ? 1000 : 400);

    public override void Write()
    {
        ushort count = GetFactionCount();
        bool wide = ModernVersion.ExpansionVersion >= 3;
        for (ushort i = 0; i < count; ++i)
        {
            if (wide)
                _worldPacket.WriteUInt16((ushort)FactionFlags[i]);
            else
                _worldPacket.WriteUInt8((byte)((ushort)FactionFlags[i] & 0xFF));
            _worldPacket.WriteInt32(FactionStandings[i]);
        }

        for (ushort i = 0; i < count; ++i)
            _worldPacket.WriteBit(FactionHasBonus[i]);

        _worldPacket.FlushBits();
    }

    // V3_4_3: 1000 × (UInt16 + Int32) + 1000 bits = 6000 + 125 = 6125 bytes max.
    public int MaxSize => MaxFactionCount * 6 + (MaxFactionCount + 7) / 8;

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);

        ushort count = GetFactionCount();
        bool wide = ModernVersion.ExpansionVersion >= 3;
        for (ushort i = 0; i < count; ++i)
        {
            if (wide)
                writer.WriteUInt16((ushort)FactionFlags[i]);
            else
                writer.WriteUInt8((byte)((ushort)FactionFlags[i] & 0xFF));
            writer.WriteInt32(FactionStandings[i]);
        }

        for (ushort i = 0; i < count; ++i)
            writer.WriteBit(FactionHasBonus[i]);

        writer.FlushBits();
        return writer.Position;
    }

    public int[] FactionStandings = new int[MaxFactionCount];
    public bool[] FactionHasBonus = new bool[MaxFactionCount]; //@todo: implement faction bonus
    public ReputationFlags[] FactionFlags = new ReputationFlags[MaxFactionCount];
}

class SetFactionStanding : ServerPacket, ISpanWritable
{
    public SetFactionStanding() : base(Opcode.SMSG_SET_FACTION_STANDING, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteFloat(ReferAFriendBonus);
        _worldPacket.WriteFloat(BonusFromAchievementSystem);

        _worldPacket.WriteInt32(Factions.Count);
        foreach (FactionStandingData factionStanding in Factions)
            factionStanding.Write(_worldPacket);

        _worldPacket.WriteBit(ShowVisual);
        _worldPacket.FlushBits();
    }

    // Cap for faction standing changes - usually just a few at once
    private const int MaxFactions = 16;
    // 2 floats(8) + count(4) + factions(8 each) + 1 bit
    public int MaxSize => 8 + 4 + MaxFactions * 8 + 1;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (Factions.Count > MaxFactions)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteFloat(ReferAFriendBonus);
        writer.WriteFloat(BonusFromAchievementSystem);
        writer.WriteInt32(Factions.Count);
        foreach (FactionStandingData factionStanding in Factions)
        {
            writer.WriteInt32(factionStanding.Index);
            writer.WriteInt32(factionStanding.Standing);
        }
        writer.WriteBit(ShowVisual);
        writer.FlushBits();
        return writer.Position;
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

class SetFactionAtWar : ClientPacket
{
    public SetFactionAtWar(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        FactionIndex = _worldPacket.ReadUInt8();
    }

    public byte FactionIndex;
}

class SetFactionNotAtWar : ClientPacket
{
    public SetFactionNotAtWar(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        FactionIndex = _worldPacket.ReadUInt8();
    }

    public byte FactionIndex;
}

class SetFactionInactive : ClientPacket
{
    public SetFactionInactive(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        FactionIndex = _worldPacket.ReadUInt32();
        State = _worldPacket.HasBit();
    }

    public uint FactionIndex;
    public bool State;
}

class SetWatchedFaction : ClientPacket
{
    public SetWatchedFaction(WorldPacket packet) : base(packet) { }

    public override void Read()
    {
        FactionIndex = _worldPacket.ReadUInt32();
    }

    public uint FactionIndex;
}

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
