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

public class InitWorldStates : ServerPacket, ISpanWritable
{
    public InitWorldStates() : base(Opcode.SMSG_INIT_WORLD_STATES, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(MapID);
        _worldPacket.WriteUInt32(ZoneID);
        _worldPacket.WriteUInt32(AreaID);

        _worldPacket.WriteInt32(Worldstates.Count);
        foreach (WorldStateInfo wsi in Worldstates)
        {
            _worldPacket.WriteUInt32(wsi.VariableID);
            _worldPacket.WriteInt32(wsi.Value);
        }
    }

    // Cap for world states - battlegrounds can have many, classic adds ~30
    private const int MaxWorldStates = 128;
    // 3 uints(12) + count(4) + states(8 each)
    public int MaxSize => 12 + 4 + MaxWorldStates * 8;

    public int WriteToSpan(Span<byte> buffer)
    {
        if (Worldstates.Count > MaxWorldStates)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(MapID);
        writer.WriteUInt32(ZoneID);
        writer.WriteUInt32(AreaID);
        writer.WriteInt32(Worldstates.Count);
        foreach (WorldStateInfo wsi in Worldstates)
        {
            writer.WriteUInt32(wsi.VariableID);
            writer.WriteInt32(wsi.Value);
        }
        return writer.Position;
    }

    public void AddState(uint variableID, int value)
    {
        Worldstates.Add(new WorldStateInfo(variableID, value));
    }

    public void AddMissingState(uint variableID, int value)
    {
        foreach (var state in Worldstates)
        {
            if (state.VariableID == variableID)
                return;
        }
        Worldstates.Add(new WorldStateInfo(variableID, value));
    }

    public void AddClassicStates()
    {
        foreach ((uint id, int value) in ModernVersion.ExpansionVersion == 1 ? ClassicEraWorldStates : ModernWorldStates)
            AddMissingState(id, value);

        // WotLK (V3_4_3) PvP "Battlegrounds" frame gating.
        // The 3.4.3 client hides AV/WSG/AB/IoC in the PvP UI unless these
        // "battleground enabled" world states are set. The list rows are gated by
        // BattlemasterList.Required_Player_Condition_ID -> PlayerCondition
        // (WorldStateExpressionID) -> these world state IDs:
        //   AV(1)=WS 17224>=1, WSG(2)=WS 17225>=1, AB(3)=WS 17227>=1, IoC(30)=WS 21975==1.
        // EOTS(7)/SOTA(9)/Random(32) use Required_Player_Condition_ID 0 and always show.
        // The legacy 3.3.5 server never emits these modern IDs; native TC 3.4.3 sends
        // them as 1 in the global SMSG_INIT_WORLD_STATES block.
        if (ModernVersion.ExpansionVersion >= 3)
        {
            AddMissingState((uint)WorldStates.BattlegroundAlteracValleyEnabled, 1);
            AddMissingState((uint)WorldStates.BattlegroundWarsongGulchEnabled, 1);
            AddMissingState((uint)WorldStates.BattlegroundArathiBasinEnabled, 1);
            AddMissingState((uint)WorldStates.BattlegroundIsleOfConquestEnabled, 1);
        }
    }

    // Bulk world states captured verbatim from native-client SMSG_INIT_WORLD_STATES
    // global blocks (Classic Era / WotLK-TBC respectively). These are opaque modern-client
    // "feature/UI enabled" flags plus arena-season values; no authoritative names exist
    // (TrinityCore stores them as raw numeric world_state DB rows). Treat as wire data —
    // do NOT "tidy" the values. Named battleground-enable flags live in the WorldStates enum.
    private static readonly (uint Id, int Value)[] ClassicEraWorldStates =
    {
        (17101, 1), (17222, 1), (17223, 1),
        ((uint)WorldStates.BattlegroundAlteracValleyEnabled, 1), // 17224
        ((uint)WorldStates.BattlegroundWarsongGulchEnabled, 1),  // 17225
        (17226, 1),
        ((uint)WorldStates.BattlegroundArathiBasinEnabled, 1),   // 17227
        (17228, 1), (17229, 1), (17230, 1), (17231, 1), (17232, 1),
        (17233, 1), (17234, 1), (17424, 1), (17430, 1), (17478, 1), (17560, 1),
        (17640, 1), (17641, 1), (17642, 1), (17643, 1), (17647, 1), (17648, 1),
        (17687, 1), (17697, 1), (17698, 1), (17704, 1), (17705, 1), (17706, 1),
        (17707, 1), (18261, 1), (19361, 1), (20281, 1), (20470, 1), (21260, 1),
    };

    private static readonly (uint Id, int Value)[] ModernWorldStates =
    {
        (17223, 1), (17647, 1), (17648, 1), (20445, 0), (20446, 0), (20447, 1),
        (20487, 1), (20488, 1), (20489, 1), (20491, 1), (20492, 1), (20493, 1),
        (20494, 0), (20495, 0), (20496, 0), (20497, 0), (20518, 0), (20560, 0),
        (20562, 1), (20563, 1), (20567, 0), (20738, 0), (20882, 0), (21125, 1),
        (21126, 1), (21195, 2725), (21196, 2542), (21197, 2203), (21198, 1898), (21199, 1453),
        (21200, 2548), (21201, 2391), (21202, 2086), (21203, 1777), (21204, 1431), (21205, 2354),
        (21206, 2181), (21207, 1922), (21208, 1686), (21209, 1408), (21238, 2),
    };

    public uint ZoneID;
    public uint AreaID;
    public uint MapID;

    List<WorldStateInfo> Worldstates = new();

    struct WorldStateInfo
    {
        public WorldStateInfo(uint variableID, int value)
        {
            VariableID = variableID;
            Value = value;
        }

        public uint VariableID;
        public int Value;
    }
}

public class UpdateWorldState : ServerPacket, ISpanWritable
{
    public UpdateWorldState() : base(Opcode.SMSG_UPDATE_WORLD_STATE, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32(VariableID);
        _worldPacket.WriteInt32(Value);
        _worldPacket.WriteBit(Hidden);
        _worldPacket.FlushBits();
    }

    public int MaxSize => 9; // uint + int + 1 byte for bit

    public int WriteToSpan(Span<byte> buffer)
    {
        var writer = new SpanPacketWriter(buffer);
        writer.WriteUInt32(VariableID);
        writer.WriteInt32(Value);
        writer.WriteBit(Hidden);
        writer.FlushBits();
        return writer.Position;
    }

    public uint VariableID;
    public int Value;
    public bool Hidden;
}
