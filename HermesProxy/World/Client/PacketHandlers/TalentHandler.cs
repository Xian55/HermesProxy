using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using System.Collections.Generic;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Legacy 3.3.5a SMSG_UPDATE_TALENT_DATA (opcode 0x4C0).
    // Wire format: X:/Programming/refs/mangos-wotlk/src/game/Entities/Player.cpp:24148
    // (BuildPlayerTalentsInfoData). TC 3.3.5a uses the same shape.
    //
    //   uint8  isPet              // 0 for player
    //   uint32 unspentTalentPoints
    //   uint8  specsCount         // 0, 1, or 2
    //   uint8  activeSpec         // 0 or 1
    //   per spec:
    //     uint8  talentIdCount
    //     per talent: uint32 TalentID + uint8 currentRank
    //     uint8  glyphSlotCount   // = MAX_GLYPH_SLOT_INDEX = 6
    //     uint16 glyphId * 6
    //
    // V3_4_3 only: translate to modern SMSG_UPDATE_TALENT_DATA (TC TalentPackets.cpp:100-132).
    // For older clients, the legacy packet is a no-op for now (V1_14/V2_5 paths don't
    // exercise this opcode against a 3.3.5a backend in the supported topology).
    [PacketHandler(Opcode.SMSG_UPDATE_TALENT_DATA)]
    void HandleTalentsInfoUpdate(WorldPacket packet)
    {
        bool isPet = packet.ReadUInt8() != 0;
        if (isPet)
        {
            ForwardPetTalents(packet);
            return;
        }

        uint unspent = packet.ReadUInt32();
        byte specsCount = packet.ReadUInt8();
        byte activeSpec = packet.ReadUInt8();

        var cache = new TalentInfoCache
        {
            UnspentTalentPoints = unspent,
            ActiveGroup = activeSpec,
        };

        int totalTalents = 0;
        for (byte specIdx = 0; specIdx < specsCount; ++specIdx)
        {
            var group = new TalentInfoCacheGroup();

            byte talentCount = packet.ReadUInt8();
            for (byte t = 0; t < talentCount; ++t)
            {
                uint talentId = packet.ReadUInt32();
                byte rank = packet.ReadUInt8();
                group.Talents.Add(new TalentEntry { TalentID = talentId, Rank = rank });
            }

            byte glyphSlots = packet.ReadUInt8();
            for (byte g = 0; g < glyphSlots; ++g)
            {
                ushort glyphId = packet.ReadUInt16();
                group.Glyphs.Add(glyphId);

                // GameState.ActiveGlyphs[6] tracks currently-equipped glyphs only;
                // mirror just the active spec's glyphs into it.
                if (specIdx == activeSpec && g < GetSession().GameState.ActiveGlyphs.Length)
                {
                    ushort prev = GetSession().GameState.ActiveGlyphs[g];
                    if (prev != glyphId)
                    {
                        GetSession().GameState.ActiveGlyphs[g] = glyphId;
                        // Mark dirty so the next player Values update re-emits GlyphSlots
                        // (iter-14: fixes "already applied" cross-spec false positives).
                        GetSession().GameState.ActiveGlyphsDirty = true;
                        Log.Print(LogType.Network,
                            $"[Glyphs] TalentPush slot={g} GlyphID {prev} -> {glyphId} (active spec={activeSpec})");
                    }
                }
            }

            cache.Groups.Add(group);
            totalTalents += group.Talents.Count;
        }

        GetSession().GameState.TalentInfo = cache;

        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        SendPacketToClient(cache.ToPacket());
        Log.Print(LogType.Network, $"Forwarded talent state: groups={specsCount} active={activeSpec} totalTalents={totalTalents} unspent={unspent}");

        // Active spec's glyphs were just refreshed in GameState.ActiveGlyphs[6] above;
        // re-emit SMSG_ACTIVE_GLYPHS so the modern client's glyph panel reflects the
        // current state (initial socket, swap, remove, spec switch).
        var glyphs = new ActiveGlyphs { IsFullUpdate = true };
        int glyphCount = 0;
        foreach (ushort gId in GetSession().GameState.ActiveGlyphs)
        {
            if (gId == 0) continue;
            uint sId = GameData.GlyphSpellById.GetValueOrDefault(gId);
            if (sId == 0) continue;
            glyphs.Glyphs.Add((sId, gId));
            glyphCount++;
        }
        SendPacketToClient(glyphs);
        Log.Print(LogType.Network, $"Forwarded glyph state: count={glyphCount} (active spec)");
    }

    // Pet branch of legacy SMSG_UPDATE_TALENT_DATA (isPet=1).
    // Wire format (CMaNGOS Player.cpp:24203 BuildPetTalentsInfoData):
    //   uint32 unspentTalentPoints
    //   uint8  talentIdCount
    //   per talent: uint32 TalentID + uint8 currentRank
    // No specs, no glyphs.
    //
    // Translates to modern SMSG_UPDATE_TALENT_DATA with IsPetTalents=true and a single
    // TalentGroupInfo with SpecID=0 + 6 zero-padded glyphs (MaxGlyphSlotIndex per
    // CypherCoreClassicWOTLK Source/Game/Networking/Packets/TalentPackets.cs:227 — that
    // server hardcodes the slot count even when no glyphs are equipped). WPP's parser
    // notes the GlyphCount is uninitialized server-side for pet talents anyway.
    void ForwardPetTalents(WorldPacket packet)
    {
        uint unspent = packet.ReadUInt32();
        byte talentCount = packet.ReadUInt8();

        var group = new TalentInfoCacheGroup();
        for (byte t = 0; t < talentCount; ++t)
        {
            uint talentId = packet.ReadUInt32();
            byte rank = packet.ReadUInt8();
            group.Talents.Add(new TalentEntry { TalentID = talentId, Rank = rank });
        }
        // Native TC 3.4.3 ships GlyphCount=0 for pet talent groups (sniff
        // World_hunter_pet_tame_pet_actionbar_pet_spellbook lines 76367-76368);
        // padding zeros widens the body and the client refuses to bind the pet tab.

        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        var modern = new UpdateTalentData
        {
            UnspentTalentPoints = unspent,
            ActiveGroup = 0,
            IsPetTalents = true,
        };
        modern.TalentGroups.Add(new TalentGroupInfo
        {
            // 0xFF = "no spec" sentinel. Native TC 3.4.3 sniff
            // (World_hunter_pet_tame_pet_actionbar_pet_spellbook) line 76369 shows
            // pet talent groups carry SpecID=255 every time; using 0 here makes the
            // V3_4_3 client refuse to bind the spellbook pet tab.
            SpecID = 255,
            PrimarySpecialization = 0,
            Talents = new List<TalentEntry>(group.Talents),
            Glyphs = new List<ushort>(group.Glyphs),
        });
        SendPacketToClient(modern);
        Log.Print(LogType.Network, $"Forwarded pet talent state: talents={talentCount} unspent={unspent}");
    }
}
