using Framework.Constants;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

// Modern V3_4_3 SMSG_ACTIVE_GLYPHS (opcode 11345 / 0x2C49). Layout matches CypherCore
// (Source/Game/Networking/Packets/TalentPackets.cs:81-97):
//   uint32 GlyphCount
//   per glyph: uint32 SpellID + uint16 GlyphID
//   bit IsFullUpdate
// SpellID comes from GlyphProperties.dbc (looked up via GameData.GlyphSpellById).
// GlyphID is the row ID — the value 3.3.5a stores in PLAYER_FIELD_GLYPHS_* / SMSG_UPDATE_TALENT_DATA.
public sealed class ActiveGlyphs : ServerPacket
{
    public List<(uint SpellID, ushort GlyphID)> Glyphs = new();
    public bool IsFullUpdate;

    public ActiveGlyphs() : base(Opcode.SMSG_ACTIVE_GLYPHS, ConnectionType.Instance) { }

    public override void Write()
    {
        _worldPacket.WriteUInt32((uint)Glyphs.Count);
        foreach (var (spellId, glyphId) in Glyphs)
        {
            _worldPacket.WriteUInt32(spellId);
            _worldPacket.WriteUInt16(glyphId);
        }
        _worldPacket.WriteBit(IsFullUpdate);
        _worldPacket.FlushBits();
    }
}

// Modern V3_4_3 CMSG_REMOVE_GLYPH (opcode 13056 / 0x32E0). Payload: uint8 GlyphSlot (0-5).
// CypherCore Source/Game/Networking/Packets/TalentPackets.cs:166-176 confirms the wire shape.
public readonly record struct RemoveGlyph(byte GlyphSlot);

// Modern V3_4_3 CMSG_PET_LEARN_TALENT (opcode 0x3554 / 13652).
// Payload best-guess (no WPP parser, no TC handler): PackedGuid128 PetGUID + uint32 TalentID
// + uint16 Rank — matches the modern CMSG_LEARN_TALENT player payload (uint32+uint16) with
// a leading PetGUID. Verify against the first packet capture; adjust if the read overruns.
public readonly record struct LearnPetTalent(WowGuid128 PetGUID, uint TalentID, ushort Rank);

// The Cataclysm talent window's Learn: the talent tab chosen as the specialization (-1 when it is
// already chosen) and the talents picked in it.
public readonly record struct LearnPreviewTalents(int TalentTab, List<TalentEntry> Talents);

// The pet talent window's Learn: the pet and its (talent, rank) pairs.
public readonly record struct LearnPreviewTalentsPet(WowGuid128 PetGUID, List<TalentEntry> Talents);

// The Cataclysm talent window's specialization choice, made before a point is spent.
public readonly record struct SetPrimaryTalentTree(uint TalentTab);

// Modern V3_4_3.54261 SMSG_UPDATE_TALENT_DATA. Layout matches WPP's V3_4_0 parser
// (canonical reader): X:/Programming/RioMcBoo/WowPacketParser/WowPacketParserModule.V3_4_0_45166/Parsers/SpellHandler.cs:436-475
// (ReadTalentInfoUpdate). Two fields are V3_4_4_59817+ only and MUST NOT be emitted
// for V3_4_3.54261 — they would shift the bit stream and the client would decode
// rank as garbage (talents render grey instead of "learned"):
//   - PrimarySpecialization (uint32 after SpecID)
//   - Rank widened from uint8 to uint32
//
// TC's wotlk_classic TalentPackets.cpp:100-132 represents the V3_4_4 layout, which
// is why we can't use it directly as the wire reference for our build.
//
// Legacy source for the body data: X:/Programming/refs/mangos-wotlk/src/game/Entities/Player.cpp:24148
// (BuildPlayerTalentsInfoData).
//
// P1 scope: single talent group, no glyph forwarding (drained on the legacy side but not
// emitted as SMSG_ACTIVE_GLYPHS — see EmptyActiveGlyphs / wotlk.md follow-ups).
//
// The 4.4.2 client has the later shape (docs/protocol/4.4.2.60895, 0x3B0070; TrinityCore
// cata_classic TalentPackets.cpp): each group's PrimarySpecialization, the talent tab its points
// went into, after the SpecID, and the rank as a uint32.
public sealed class UpdateTalentData : ServerPacket
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<UpdateTalentData>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V4_4_2_60895, new GroupsLayout(cataClassic: false)),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new GroupsLayout(cataClassic: true)));

    private static readonly ServerPacketLayout<UpdateTalentData> Layout = Layouts.ForRunningClient();

    public uint UnspentTalentPoints;
    public byte ActiveGroup;
    public List<TalentGroupInfo> TalentGroups = new();
    public bool IsPetTalents;

    public UpdateTalentData() : base(Opcode.SMSG_UPDATE_TALENT_DATA, ConnectionType.Instance) { }

    public override void Write() => Layout.Write(this, _worldPacket);

    internal sealed class GroupsLayout(bool cataClassic) : ServerPacketLayout<UpdateTalentData>
    {
        public override void Write(UpdateTalentData packet, WorldPacket data)
        {
            data.WriteUInt32(packet.UnspentTalentPoints);
            data.WriteUInt8(packet.ActiveGroup);
            data.WriteUInt32((uint)packet.TalentGroups.Count);

            foreach (var group in packet.TalentGroups)
            {
                data.WriteUInt8((byte)group.Talents.Count);
                data.WriteUInt32((uint)group.Talents.Count);

                data.WriteUInt8((byte)group.Glyphs.Count);
                data.WriteUInt32((uint)group.Glyphs.Count);

                data.WriteUInt8(group.SpecID);
                if (cataClassic)
                    data.WriteUInt32(group.PrimarySpecialization);

                foreach (var talent in group.Talents)
                {
                    data.WriteUInt32(talent.TalentID);
                    if (cataClassic)
                        data.WriteUInt32(talent.Rank);
                    else
                        data.WriteUInt8((byte)talent.Rank);
                }

                foreach (var glyph in group.Glyphs)
                    data.WriteUInt16(glyph);
            }

            data.WriteBit(packet.IsPetTalents);
            data.FlushBits();
        }
    }
}

public sealed class TalentGroupInfo
{
    public byte SpecID;
    public uint PrimarySpecialization;  // TalentTab.dbc ID; 0 when no points spent
    public List<TalentEntry> Talents = new();
    public List<ushort> Glyphs = new();
}

public struct TalentEntry
{
    public uint TalentID;
    public uint Rank;  // legacy is uint8 (0-4); widened to uint32 to match modern wire layout
}

public sealed class TalentInfoCacheGroup
{
    // The talent tab the group's points went into, which a 4.3.4 server sends; 0 from the older ones.
    public uint PrimaryTree;
    public List<TalentEntry> Talents = new();
    public List<ushort> Glyphs = new();
}

// Per-session snapshot of the legacy server's last SMSG_UPDATE_TALENT_DATA push.
// Cached so HandleLoginVerifyWorld (V3_4_3) can re-emit real data on relog without
// waiting for the legacy server's post-login push to arrive.
//
// Carries all spec groups (1 or 2) — dual-spec is signalled per-group via SpecID
// matching TC's SendTalentsInfoData encoding (Player.cpp:26152): SpecID=0 when
// single-spec, SpecID=N (= group count) on every group when dual-spec.
public sealed class TalentInfoCache
{
    public uint UnspentTalentPoints;
    public byte ActiveGroup;
    public List<TalentInfoCacheGroup> Groups = new();

    public UpdateTalentData ToPacket()
    {
        var packet = new UpdateTalentData
        {
            UnspentTalentPoints = UnspentTalentPoints,
            ActiveGroup = ActiveGroup,
            IsPetTalents = false,
        };

        byte specId = Groups.Count > 1 ? (byte)Groups.Count : (byte)0;

        foreach (var src in Groups)
        {
            packet.TalentGroups.Add(new TalentGroupInfo
            {
                SpecID = specId,
                PrimarySpecialization = src.PrimaryTree,
                Talents = new List<TalentEntry>(src.Talents),
                Glyphs = new List<ushort>(src.Glyphs),
            });
        }

        return packet;
    }
}
