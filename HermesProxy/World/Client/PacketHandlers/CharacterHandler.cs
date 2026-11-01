using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Objects;
using HermesProxy.World.Outbox;
using HermesProxy.World.Server;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [HandlesSmsg(Opcode.SMSG_ENUM_CHARACTERS_RESULT)]
    internal void HandleEnumCharactersResult(WorldPacket packet)
    {
        Log.Print(LogType.Trace, "[Trace] HandleEnumCharactersResult: ENTER — translating legacy SMSG_ENUM_CHARACTERS_RESULT to modern");
        EnumCharactersResult charEnum = new();
        charEnum.Success = true;
        charEnum.IsDeletedCharacters = false;
        charEnum.IsNewPlayerRestrictionSkipped = false;
        charEnum.IsNewPlayerRestricted = false;
        // Must be false for the 3.4.3 client to render existing characters — true marks the
        // result as "new account, no characters yet" and the client suppresses the list.
        charEnum.IsNewPlayer = false;
        charEnum.IsAlliedRacesCreationAllowed = false;
        // Attempt 3 (per _charenum_diff_report.md): TC always sends
        // HasDisabledClassesMask=true with DisabledClassesMask=0 (a 4-byte explicit
        // zero) in the V3_4_3 envelope. We were sending HasDisabledClassesMask=false
        // and skipping the field entirely, producing a 4-byte-shorter envelope.
        // Match TC's wire layout. Was: DisabledClassesMask = null.
        charEnum.DisabledClassesMask = 0u;

        GetSession().GameState.OwnCharacters.Clear();

        List<LegacyCharacterEntry> entries = IsCataLegacy ? ReadCharacterEntriesCata(packet) : ReadCharacterEntries(packet);
        byte count = (byte)entries.Count;
        Log.Print(LogType.Network, $"[CharEnum] legacy count={count}");
        uint virtualRealmAddress = GetSession().Realm?.Id.GetAddress() ?? 0u;
        Log.Print(LogType.Trace, $"[Trace] HandleEnumCharactersResult: realm.GetAddress()=0x{virtualRealmAddress:X8} ({virtualRealmAddress})");
        for (byte i = 0; i < count; i++)
        {
            LegacyCharacterEntry entry = entries[i];
            EnumCharactersResult.CharacterInfo char1 = new EnumCharactersResult.CharacterInfo();
            // Unique per-char slot. With ListPosition=0 for all chars, the V3_4_3 client
            // collapses multi-char enums into a single slot and renders nothing — confirmed
            // by the 1-char-works-vs-2-char-doesn't A/B against TC backend on 2026-04-30.
            // The earlier "TC ships all at 0" observation in _charenum_diff_report.md was
            // a misread of a single-char TC capture; with 2 chars TC also assigns unique
            // positions.
            char1.ListPosition = i;
            char1.VirtualRealmAddress = virtualRealmAddress;
            PlayerCache cache = new PlayerCache();
            char1.Guid = entry.Guid.To128(GetSession().GameState);
            char1.GuildClubMemberID = GuildClub.MemberIdOf(char1.Guid);
            char1.Name = cache.Name = entry.Name;
            char1.RaceId = cache.RaceId = entry.Race;
            char1.ClassId = cache.ClassId = entry.Class;
            char1.SexId = cache.SexId = entry.Sex;

            char1.Customizations = CharacterCustomizations.ConvertLegacyCustomizationsToModern((Race)char1.RaceId, (Gender)char1.SexId, entry.Skin, entry.Face, entry.HairStyle, entry.HairColor, entry.FacialHair);

            char1.ExperienceLevel = cache.Level = entry.Level;
            if (char1.ExperienceLevel > charEnum.MaxCharacterLevel)
                charEnum.MaxCharacterLevel = char1.ExperienceLevel;

            GetSession().GameState.UpdatePlayerCache(char1.Guid, cache);

            char1.ZoneId = entry.ZoneId;
            char1.MapId = entry.MapId;
            char1.PreloadPos = entry.PreloadPos;
            uint guildId = entry.GuildId;
            GetSession().GameState.StorePlayerGuildId(char1.Guid, guildId);
            char1.GuildGuid = guildId != 0 ? WowGuid128.Create(HighGuidType703.Guild, guildId) : WowGuid128.Empty;
            char1.Flags = entry.Flags;
            // Preserve Declined: legacy servers set it when declined names exist
            // OR the feature is disabled. Clearing it makes ruRU clients request
            // unnecessary name cases before entering the world.

            char1.Flags2 = entry.CustomizationFlags;

            char1.FirstLogin = entry.FirstLogin;

            // V3_4_3 client validates ZoneId/MapId against the race's starting-zone DB
            // iff FirstLogin=true. Both cMangos (Map=0, Zone=0) and TC (Map=valid,
            // Zone=0) under-populate the new char's location; either shape breaks the
            // client's auto-select on the just-created entry. Synthesize the canonical
            // starting (Map, Zone, Position) whenever ZoneId=0 — the legacy server
            // overwrites these on the real login.
            if (char1.FirstLogin
                && ModernVersion.IsWotLKClassicOrLater
                && char1.ZoneId == 0)
            {
                ApplyStartingLocation(char1);
            }
            char1.PetCreatureDisplayId = entry.PetCreatureDisplayId;
            char1.PetExperienceLevel = entry.PetExperienceLevel;
            char1.PetCreatureFamilyId = entry.PetCreatureFamilyId;

            for (int j = 0; j < entry.VisualItems.Length; j++)
            {
                char1.VisualItems[j].DisplayId = entry.VisualItems[j].DisplayId;
                char1.VisualItems[j].InvType = entry.VisualItems[j].InvType;
                char1.VisualItems[j].DisplayEnchantId = entry.VisualItems[j].DisplayEnchantId;
            }

            // Reset Flags2 — the legacy CustomizationFlags read above are not the same field as
            // the modern Flags2, and stale bits there cause the 3.4.3 client to silently drop
            // the character entry.
            char1.Flags2 = 0;
            char1.Flags3 = 0;
            char1.Flags4 = 0;
            char1.ProfessionIds[0] = 0;
            char1.ProfessionIds[1] = 0;
            char1.LastPlayedTime = (ulong) Time.UnixTime;
            char1.SpecID = 0;
            char1.Unknown703 = 0;
            // Attempt 1 (per _charenum_diff_report.md): TC sends LastLoginVersion=11201
            // (the historical legacy WotLK build number) for ALL chars in V3_4_3
            // SMSG_ENUM_CHARACTERS_RESULT, even when the modern V3_4_3 client connects.
            // The field describes "what build did this character last play on", not
            // "what build is the client" — so the right value is the legacy build.
            // Was: (uint)ModernVersion.BuildInt = 54261. Now matches TC.
            char1.LastLoginVersion = (uint)LegacyVersion.BuildInt;
            char1.OverrideSelectScreenFileDataID = 0;
            char1.BoostInProgress = false;
            char1.unkWod61x = 0;
            char1.ExpansionChosen = true;
            Log.Print(LogType.Trace,
                $"[Trace] HandleEnumCharactersResult: built char[{i}] guid={char1.Guid} name='{char1.Name}' " +
                $"race={char1.RaceId} class={char1.ClassId} sex={char1.SexId} level={char1.ExperienceLevel} " +
                $"zone={char1.ZoneId} map={char1.MapId} guildGuid={char1.GuildGuid} customizations={char1.Customizations.Count}");
            charEnum.Characters.Add(char1);

            GetSession().GameState.OwnCharacters.Add(new OwnCharacterInfo
            {
                AccountId = GetSession().GameAccountInfo.WoWAccountGuid,
                CharacterGuid = char1.Guid,
                Realm = GetSession().Realm!,
                LastLoginUnixSec = char1.LastPlayedTime,
                Name = char1.Name,
                RaceId = char1.RaceId,
                ClassId = char1.ClassId,
                SexId = char1.SexId,
                Level = char1.ExperienceLevel,
                MapId = char1.MapId,
            });
        }

        charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(1, true, false, false));
        charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(2, true, false, false));
        charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(3, true, false, false));
        charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(4, true, false, false));
        charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(5, true, false, false));
        charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(6, true, false, false));
        charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(7, true, false, false));
        charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(8, true, false, false));
        if (ModernVersion.ExpansionVersion >= 2 &&
            LegacyVersion.ExpansionVersion >= 2)
        {
            charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(10, true, false, false));
            charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(11, true, false, false));
        }
        // Goblin and Worgen arrived with Cataclysm; without their unlock the 4.4.2 client greys out
        // their icons on the creation screen.
        if (ModernVersion.ExpansionVersion >= 4 &&
            LegacyVersion.ExpansionVersion >= 4)
        {
            charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(9, true, false, false));
            charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(22, true, false, false));
        }
        Log.Print(LogType.Trace,
            $"[Trace] HandleEnumCharactersResult: SEND — chars={charEnum.Characters.Count} maxLvl={charEnum.MaxCharacterLevel} " +
            $"races={charEnum.RaceUnlockData.Count} success={charEnum.Success} isNewPlayer={charEnum.IsNewPlayer} " +
            $"disabledClassesMask={(charEnum.DisabledClassesMask.HasValue ? "set" : "null")}");

        // Internal-enum consumer: this enum was requested by HandleCreateChar to
        // resolve the just-created char's GUID. Look up the FirstLogin entry by
        // name, send the deferred SMSG_CREATE_CHAR with the real GUID, and DO
        // NOT forward this enum to the modern client (the client will request
        // its own next, in response to SMSG_CREATE_CHAR).
        var state = GetSession().GameState;
        if (state.IsInternalCharEnumPending && state.PendingCreateCharLegacyResult.HasValue)
        {
            // Case-insensitive match: legacy server normalises names to title case
            // ("dk" submitted → "Dk" stored), so the requested-name string we cached
            // on CMSG_CREATE_CHARACTER won't byte-match the enum entry.
            var match = charEnum.Characters.FirstOrDefault(c =>
                c.FirstLogin && string.Equals(c.Name, state.PendingCreateCharName, StringComparison.OrdinalIgnoreCase));

            CreateChar createChar = new CreateChar();
            createChar.Code = ModernVersion.ConvertResponseCodesValue(state.PendingCreateCharLegacyResult.Value);
            createChar.Guid = match != null ? match.Guid : new WowGuid128();
            SendPacketToClient(createChar);

            state.PendingCreateCharName = null;
            state.PendingCreateCharLegacyResult = null;
            state.IsInternalCharEnumPending = false;
            return;
        }

        var realmName = GetSession().Realm?.Name;
        if (!string.IsNullOrEmpty(realmName))
        {
            var saved = GetSession().AccountMetaDataMgr.LoadCharacterListOrder(realmName);
            var pruned = CharacterListOrder.Apply(charEnum.Characters, saved);
            if (pruned.Count != saved.Count)
            {
                GetSession().AccountMetaDataMgr.SaveCharacterListOrder(realmName, CharacterListOrder.Normalize(pruned));
                Log.Print(LogType.Debug, $"[CharEnum] pruned {saved.Count - pruned.Count} missing character(s) from list order");
            }
            Log.Print(LogType.Debug,
                $"[CharEnum] applied order=[{string.Join(", ", charEnum.Characters.ConvertAll(c => $"{c.Name}:{c.ListPosition}"))}]");

            GetSession().AccountMetaDataMgr.RememberRealmFromCharacterList(
                realmName,
                charEnum.Characters.ConvertAll(c => (c.Name!, c.Guid.Low)));
        }

        SendPacketToClient(charEnum);
        Log.Print(LogType.Trace, "[Trace] HandleEnumCharactersResult: EXIT — translation complete, packet queued for modern client");
    }

    /// <summary>One character of a legacy SMSG_ENUM_CHARACTERS_RESULT, as the server sent it.</summary>
    internal sealed class LegacyCharacterEntry
    {
        public WowGuid64 Guid;
        public string Name = "";
        public Race Race;
        public Class Class;
        public Gender Sex;
        public byte Skin, Face, HairStyle, HairColor, FacialHair, Level;
        public uint ZoneId, MapId, GuildId;
        public Vector3 PreloadPos;
        public CharacterFlags Flags;
        public uint CustomizationFlags;
        public bool FirstLogin;
        public uint PetCreatureDisplayId, PetExperienceLevel, PetCreatureFamilyId;
        // The equipment slots, then the bags (one before 3.3.3, four from it on).
        public (uint DisplayId, byte InvType, uint DisplayEnchantId)[] VisualItems = new (uint, byte, uint)[EquipmentSlot.Bag1 + 4];
    }

    /// <summary>The 1.12 to 3.3.5a list: a count, then one character after another.</summary>
    private static List<LegacyCharacterEntry> ReadCharacterEntries(WorldPacket packet)
    {
        byte count = packet.ReadUInt8();
        var entries = new List<LegacyCharacterEntry>(count);
        for (byte i = 0; i < count; i++)
        {
            var entry = new LegacyCharacterEntry();
            entry.Guid = packet.ReadGuid();
            entry.Name = packet.ReadCString();
            entry.Race = (Race)packet.ReadUInt8();
            entry.Class = (Class)packet.ReadUInt8();
            entry.Sex = (Gender)packet.ReadUInt8();

            entry.Skin = packet.ReadUInt8();
            entry.Face = packet.ReadUInt8();
            entry.HairStyle = packet.ReadUInt8();
            entry.HairColor = packet.ReadUInt8();
            entry.FacialHair = packet.ReadUInt8();

            entry.Level = packet.ReadUInt8();

            entry.ZoneId = packet.ReadUInt32();
            entry.MapId = packet.ReadUInt32();
            entry.PreloadPos = packet.ReadVector3();
            entry.GuildId = packet.ReadUInt32();
            entry.Flags = (CharacterFlags)packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                entry.CustomizationFlags = packet.ReadUInt32();

            entry.FirstLogin = packet.ReadUInt8() != 0;
            entry.PetCreatureDisplayId = packet.ReadUInt32();
            entry.PetExperienceLevel = packet.ReadUInt32();
            entry.PetCreatureFamilyId = packet.ReadUInt32();

            for (int j = EquipmentSlot.Start; j < EquipmentSlot.End; j++)
            {
                entry.VisualItems[j].DisplayId = packet.ReadUInt32();
                entry.VisualItems[j].InvType = packet.ReadUInt8();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    entry.VisualItems[j].DisplayEnchantId = packet.ReadUInt32();
            }

            int bagCount = LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685) ? 4 : 1;
            for (int j = 0; j < bagCount; j++)
            {
                entry.VisualItems[EquipmentSlot.Bag1 + j].DisplayId = packet.ReadUInt32();
                entry.VisualItems[EquipmentSlot.Bag1 + j].InvType = packet.ReadUInt8();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    entry.VisualItems[EquipmentSlot.Bag1 + j].DisplayEnchantId = packet.ReadUInt32();
            }

            entries.Add(entry);
        }
        return entries;
    }

    /// <summary>
    /// The 4.3.4 list (TrinityCore 4.3.4 EnumCharactersResult::Write): every character's GUID and
    /// guild GUID masks and its name length up front as bits, then each character's fields with
    /// the GUID bytes interleaved in a fixed order, each non-zero byte XORed with 1.
    /// </summary>
    internal static List<LegacyCharacterEntry> ReadCharacterEntriesCata(WorldPacket packet)
    {
        uint restrictionCount = packet.ReadBits<uint>(23);  // FactionChangeRestrictions
        packet.HasBit();                            // Success
        uint count = packet.ReadBits<uint>(17);

        var guidMasks = new bool[count][];
        var guildMasks = new bool[count][];
        var nameLengths = new uint[count];
        var firstLogins = new bool[count];
        for (uint i = 0; i < count; i++)
        {
            bool[] g = guidMasks[i] = new bool[8];
            bool[] gg = guildMasks[i] = new bool[8];
            g[3] = packet.HasBit();
            gg[1] = packet.HasBit();
            gg[7] = packet.HasBit();
            gg[2] = packet.HasBit();
            nameLengths[i] = packet.ReadBits<uint>(7);
            g[4] = packet.HasBit();
            g[7] = packet.HasBit();
            gg[3] = packet.HasBit();
            g[5] = packet.HasBit();
            gg[6] = packet.HasBit();
            g[1] = packet.HasBit();
            gg[5] = packet.HasBit();
            gg[4] = packet.HasBit();
            firstLogins[i] = packet.HasBit();
            g[0] = packet.HasBit();
            g[2] = packet.HasBit();
            g[6] = packet.HasBit();
            gg[0] = packet.HasBit();
        }
        packet.ResetBitPos();

        var entries = new List<LegacyCharacterEntry>((int)count);
        for (uint i = 0; i < count; i++)
        {
            bool[] g = guidMasks[i];
            bool[] gg = guildMasks[i];
            byte[] guid = new byte[8];
            byte[] guild = new byte[8];
            void Seq(bool[] mask, byte[] bytes, int index)
            {
                if (mask[index])
                    bytes[index] = (byte)(packet.ReadUInt8() ^ 1);
            }

            var entry = new LegacyCharacterEntry { FirstLogin = firstLogins[i] };
            entry.Class = (Class)packet.ReadUInt8();
            for (int j = 0; j < entry.VisualItems.Length; j++)
            {
                entry.VisualItems[j].InvType = packet.ReadUInt8();
                entry.VisualItems[j].DisplayId = packet.ReadUInt32();
                entry.VisualItems[j].DisplayEnchantId = packet.ReadUInt32();
            }

            entry.PetCreatureFamilyId = packet.ReadUInt32();
            Seq(gg, guild, 2);
            packet.ReadUInt8();                     // ListPosition
            entry.HairStyle = packet.ReadUInt8();
            Seq(gg, guild, 3);
            entry.PetCreatureDisplayId = packet.ReadUInt32();
            entry.Flags = (CharacterFlags)packet.ReadUInt32();
            entry.HairColor = packet.ReadUInt8();
            Seq(g, guid, 4);
            entry.MapId = packet.ReadUInt32();
            Seq(gg, guild, 5);
            float z = packet.ReadFloat();
            Seq(gg, guild, 6);
            entry.PetExperienceLevel = packet.ReadUInt32();
            Seq(g, guid, 3);
            float y = packet.ReadFloat();
            entry.CustomizationFlags = packet.ReadUInt32();
            entry.FacialHair = packet.ReadUInt8();
            Seq(g, guid, 7);
            entry.Sex = (Gender)packet.ReadUInt8();
            entry.Name = packet.ReadString(nameLengths[i]);
            entry.Face = packet.ReadUInt8();
            Seq(g, guid, 0);
            Seq(g, guid, 2);
            Seq(gg, guild, 1);
            Seq(gg, guild, 7);
            float x = packet.ReadFloat();
            entry.Skin = packet.ReadUInt8();
            entry.Race = (Race)packet.ReadUInt8();
            entry.Level = packet.ReadUInt8();
            Seq(g, guid, 6);
            Seq(gg, guild, 4);
            Seq(gg, guild, 0);
            Seq(g, guid, 5);
            Seq(g, guid, 1);
            entry.ZoneId = packet.ReadUInt32();

            entry.PreloadPos = new Vector3(x, y, z);
            entry.Guid = new WowGuid64(BitConverter.ToUInt64(guid));
            // A 4.3.4 guild GUID carries the guild id in its low half.
            entry.GuildId = (uint)BitConverter.ToUInt64(guild);
            entries.Add(entry);
        }

        for (uint i = 0; i < restrictionCount; i++)
        {
            packet.ReadInt32();                     // Mask
            packet.ReadUInt8();                     // Race
        }
        return entries;
    }

    [HandlesSmsg(Opcode.SMSG_CREATE_CHAR)]
    internal void HandleCreateChar(WorldPacket packet)
    {
        byte result = packet.ReadUInt8();
        var state = GetSession().GameState;

        // V3_4_3 client uses the GUID in SMSG_CREATE_CHAR to auto-select the
        // just-created character on the next char-list render. Legacy 3.3.5
        // SMSG_CHAR_CREATE doesn't carry a GUID, so we defer the modern reply,
        // request a fresh CMSG_CHAR_ENUM ourselves, and stamp the matching
        // FirstLogin entry's GUID into SMSG_CREATE_CHAR before forwarding.
        // Failure paths skip this dance (no GUID needed for an error reply).
        if (ModernVersion.IsWotLKClassicOrLater
            && result == (byte)Enums.V3_3_5a_12340.ResponseCodes.CharCreateSuccess
            && !string.IsNullOrEmpty(state.PendingCreateCharName))
        {
            state.PendingCreateCharLegacyResult = result;
            state.IsInternalCharEnumPending = true;
            SendPacketToServer(new WorldPacket(Opcode.CMSG_ENUM_CHARACTERS));
            return;
        }

        // Failure or non-V3_4_3 path — clear pending state and forward as before.
        state.PendingCreateCharName = null;
        state.PendingCreateCharLegacyResult = null;
        state.IsInternalCharEnumPending = false;

        CreateChar createChar = new CreateChar();
        createChar.Guid = new WowGuid128();
        createChar.Code = ModernVersion.ConvertResponseCodesValue(result);
        SendPacketToClient(createChar);
    }

    [HandlesSmsg(Opcode.SMSG_DELETE_CHAR)]
    internal void HandleDeleteChar(WorldPacket packet)
    {
        byte result = packet.ReadUInt8();

        DeleteChar deleteChar = new DeleteChar();
        deleteChar.Code = ModernVersion.ConvertResponseCodesValue(result);
        SendPacketToClient(deleteChar);
    }

    [HandlesSmsg(Opcode.SMSG_QUERY_PLAYER_NAME_RESPONSE)]
    internal void HandleQueryPlayerNameResponse(WorldPacket packet)
    {
        QueryPlayerNameResponse response = new QueryPlayerNameResponse();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
        {
            response.Player = response.Data.GuidActual = packet.ReadPackedGuid().To128(GetSession().GameState);
            var fail = packet.ReadBool();
            if (fail)
            {
                response.Result = (byte)Enums.V2_5_2_39570.ResponseCodes.Failure;
                SendPacketToClient(response);
                return;
            }
        }
        else
            response.Player = response.Data.GuidActual = packet.ReadGuid().To128(GetSession().GameState);

        PlayerCache cache = new PlayerCache();
        response.Data.Name = cache.Name = packet.ReadCString();
        packet.ReadCString(); // realm name

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
        {
            response.Data.RaceID = cache.RaceId = (Race)packet.ReadUInt8();
            response.Data.Sex = cache.SexId = (Gender)packet.ReadUInt8();
            response.Data.ClassID = cache.ClassId =(Class)packet.ReadUInt8();
        }
        else
        {
            response.Data.RaceID = cache.RaceId = (Race)packet.ReadUInt32();
            response.Data.Sex = cache.SexId = (Gender)packet.ReadUInt32();
            response.Data.ClassID = cache.ClassId = (Class)packet.ReadInt32();
        }

        if (GetSession().GameState.CachedPlayers.ContainsKey(response.Player))
            response.Data.Level = GetSession().GameState.CachedPlayers[response.Player].Level;
        if (response.Data.Level == 0)
            response.Data.Level = 1;

        GetSession().GameState.UpdatePlayerCache(response.Player, cache);

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            if (packet.ReadBool())
            {
                for (var i = 0; i < 5; i++)
                    response.Data.DeclinedNames.name[i] = packet.ReadCString();
            }
        }

        response.Data.IsDeleted = false;
        response.Data.AccountID = GetSession().GetGameAccountGuidForPlayer(response.Player);
        response.Data.BnetAccountID = GetSession().GetBnetAccountGuidForPlayer(response.Player);
        response.Data.GuildClubMemberID = GuildClub.MemberIdOf(response.Player);
        response.Data.VirtualRealmAddress = GetSession().RealmId.GetAddress();
        SendPacketToClient(response);

        // A client that reads the name from the player object was given none when the player was
        // created, unless this answer had come before. It takes the name as a change.
        if (ModernVersion.PlayerObjectCarriesName && response.Player != GetSession().GameState.CurrentPlayerGuid
            && cache.Name.Length > 0)
        {
            ObjectUpdate named = new ObjectUpdate(response.Player, UpdateTypeModern.Values, GetSession());
            named.EnsurePlayerData().Name = cache.Name;
            UpdateObject updatePacket = new UpdateObject(GetSession().GameState);
            updatePacket.ObjectUpdates.Add(named);
            SendPacketToClient(updatePacket);
        }
    }

    [HandlesSmsg(Opcode.SMSG_LOGIN_VERIFY_WORLD)]
    internal void HandleLoginVerifyWorld(WorldPacket packet)
    {
        LoginVerifyWorld verify = new LoginVerifyWorld();
        verify.MapID = packet.ReadUInt32();
        GetSession().GameState.CurrentMapId = verify.MapID;
        verify.Pos.X = packet.ReadFloat();
        verify.Pos.Y = packet.ReadFloat();
        verify.Pos.Z = packet.ReadFloat();
        verify.Pos.Orientation = packet.ReadFloat();
        SendPacketToClient(verify);

        GetSession().GameState.IsInWorld = true;
        // Name queries the client made at character select go out now.
        GetSession().ToServer.SetGate(OutboxGate.InWorld, open: true);

        bool isModernWotLK = ModernVersion.ExpansionVersion >= 3;

        // V3_4_3 modern client expects EmptyInitWorldStates BEFORE WorldServerInfo
        // (per HermesProxy-WOTLK fork's WorldClient.cs:1106).
        if (isModernWotLK)
        {
            EmptyInitWorldStates worldStates = new();
            worldStates.MapId = verify.MapID;
            SendPacketToClient(worldStates);
        }

        SendPacketToClient(WorldServerInfo.ForMap(verify.MapID));

        // SetAllTaskProgress is for pre-WotLK-Classic clients only — V3_4_3 doesn't expect it
        // and the fork explicitly skips it for ExpansionVersion >= 3.
        if (!isModernWotLK)
        {
            SetAllTaskProgress tasks = new();
            SendPacketToClient(tasks);
        }

        InitialSetup setup = new();
        setup.ServerExpansionLevel = VersionChecker.GetClientExpansionLevel(LegacyVersion.Build);
        SendPacketToClient(setup);

        // A 4.3.4 server sends the character's raid frame profiles itself, right after this
        // (HandleLoadCUFProfiles); the proxy's own copy is for the servers that keep none.
        if (!IsCataLegacy)
        {
            LoadCUFProfiles cuf = new();
            cuf.Data = GetSession().AccountDataMgr.LoadCUFProfiles();
            SendPacketToClient(cuf);
        }
        // A 4.3.4 server sends archaeology's completed projects only when asked, which its own
        // client does when the archaeology window opens. The 4.4.2 client keeps them in the player
        // data and never asks, so the proxy asks once here.
        else
            SendPacketToServer(new WorldPacket(Opcode.CMSG_REQUEST_RESEARCH_HISTORY));

        // Issue #80: modern 1.14+ Classic clients auto-cancel wand `Shoot` when target enters
        // melee range (`autoRangedCombat` CVar default-ON). Priests rely on wand for downtime
        // DPS — hint them to disable the CVar so wand keeps firing in melee.
        if (GetSession().GameState.CurrentPlayerInfo?.ClassId == Class.Priest &&
            ModernVersion.ExpansionVersion == 1 &&
            ModernVersion.AddedInVersion(ClientVersionBuild.V1_14_0_39802))
        {
            ChatPkt hint = new ChatPkt(GetSession(), ChatMessageTypeModern.System,
                "[HermesProxy] Wand tip: type |cff00ff00/console autoRangedCombat 0|r to keep your wand firing when mobs enter melee range.");
            SendPacketToClient(hint);
        }

        // V3_4_3 modern client requires a batch of "I have no X data" announcements
        // before it dismisses the loading screen and sends CMSG_MOVE_INIT_ACTIVE_MOVER_COMPLETE.
        // Legacy 3.3.5a doesn't emit these (the modern protocol added them), so the proxy
        // synthesizes empty/default versions. Ported from HermesProxy-WOTLK fork's
        // WorldClient.cs:1130-1147 — this is the actual unblock for the loading-screen stall.
        if (isModernWotLK)
        {
            // SMSG_ALL_ACHIEVEMENT_DATA arrives async from the legacy 3.3.5a server
            // and is translated by AchievementHandler.HandleAllAchievementData.
            SendPacketToClient(new EmptyAllAccountCriteria());
            // A 4.3.4 server has sent its own list by now; an empty one after it clears the tab.
            if (!IsCataLegacy)
                SendPacketToClient(new EmptySetupCurrency());
            SendPacketToClient(new EmptySpellHistory());
            SendPacketToClient(new EmptySpellCharges());
            // Use cached talent state if the legacy server already pushed it (relog case);
            // otherwise send the empty stub as a placeholder. The legacy server emits
            // SMSG_UPDATE_TALENT_DATA after world-enter on first login, and the handler
            // (TalentHandler.HandleTalentsInfoUpdate) will refresh the panel then.
            var talentCache = GetSession().GameState.TalentInfo;
            if (talentCache != null)
                SendPacketToClient(talentCache.ToPacket());
            else
                SendPacketToClient(new EmptyTalentData());
            SendPacketToClient(BuildActiveGlyphsPacket(GetSession().GameState));
            SendPacketToClient(new EmptyEquipmentSetList());
            SendPacketToClient(new EmptyAccountMountUpdate());
            if (GetSession().GameState.CollectionFavorites == null)
                GetSession().GameState.CollectionFavorites = GetSession().AccountMetaDataMgr.LoadCollectionFavorites();
            SendPacketToClient(AccountToyUpdate.FromSession(GetSession().GameState));
            SendPacketToClient(new AccountHeirloomUpdate());
            SendPacketToClient(new BattlePetJournalLockAcquired());

            PhaseShiftChange phaseShift = new();
            phaseShift.Client = GetSession().GameState.CurrentPlayerGuid;
            SendPacketToClient(phaseShift);
        }
    }

    // Build a real SMSG_ACTIVE_GLYPHS packet from cached glyph state. The cache is
    // populated by TalentHandler.HandleTalentsInfoUpdate (active spec's glyph block from
    // legacy SMSG_UPDATE_TALENT_DATA). Each non-zero GlyphID is mapped to its passive
    // SpellID via GameData.GlyphSpellById (loaded from CSV/Hotfix/GlyphProperties3.csv).
    // IsFullUpdate=true means "this is the complete current glyph set" — what the client
    // needs at world-enter and on talent push.
    static ActiveGlyphs BuildActiveGlyphsPacket(GameSessionData state)
    {
        var packet = new ActiveGlyphs { IsFullUpdate = true };
        foreach (ushort glyphId in state.ActiveGlyphs)
        {
            if (glyphId == 0)
                continue;
            uint spellId = GameData.GlyphSpellById.GetValueOrDefault(glyphId);
            if (spellId == 0)
                continue;  // glyph not in our DBC — drop rather than emit a half-built entry
            packet.Glyphs.Add((spellId, glyphId));
        }
        return packet;
    }

    [HandlesSmsg(Opcode.SMSG_CHARACTER_LOGIN_FAILED)]
    internal void HandleCharacterLoginFailed(WorldPacket packet)
    {
        CharacterLoginFailed failed = new CharacterLoginFailed();
        failed.Code = (Framework.Constants.LoginFailureReason)packet.ReadUInt8();
        SendPacketToClient(failed);

        GetSession().GameState.IsInWorld = false;
        GetSession().ToServer.SetGate(OutboxGate.InWorld, open: false);
    }

    [HandlesSmsg(Opcode.SMSG_UPDATE_ACTION_BUTTONS)]
    internal void HandleUpdateActionButtons(WorldPacket packet)
    {
        byte reason = 0;
        if (IsCataLegacy)
        {
            // 4.3.4 moved the reason behind the buttons (TrinityCore 4.3.4 UpdateActionButtons).
            ReadOnlySpan<byte> rest = packet.GetRemainingSpan();
            reason = rest.Length > 0 ? rest[^1] : (byte)0;
            if (reason == 2)
                return;
        }
        else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
        {
            reason = packet.ReadUInt8();
            if (reason == 2)
                return;
        }

        List<int> buttons = new List<int>();

        // Legacy SMSG_UPDATE_ACTION_BUTTONS array size grew across expansions:
        // Vanilla=120, TBC=132, WotLK 3.2+=144 (mangos-wotlk Player.h:201).
        int buttonCount = PlayerConst.MaxActionButtonsVanilla;
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
            buttonCount = PlayerConst.MaxActionButtonsWotLK;
        else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            buttonCount = PlayerConst.MaxActionButtonsTbc;

        int nonZeroCount = 0;
        for (int i = 0; i < buttonCount; i++)
        {
            int packed = packet.ReadInt32();
            buttons.Add(packed);
            if (packed != 0)
            {
                int actionId = packed & 0x00FFFFFF;
                int type = (packed >> 24) & 0xFF;
                Log.Print(LogType.Debug,
                    $"[V343Trace][LoadButtons] legacy slot={i} action={actionId} type={type} packed=0x{packed:X8}");
                nonZeroCount++;
            }
        }
        Log.Print(LogType.Debug,
            $"[V343Trace][LoadButtons] legacy total={buttonCount} nonZero={nonZeroCount} legacyReason={reason}");

        // Pad to TBC's 132 if we read fewer (Vanilla only sends 120). The V1_14 /
        // V2_5 ObjectUpdateBuilders later read m_gameState.ActionButtons[i] by
        // index for i = 0..131 during CreateObject — without this pad they'd
        // throw IndexOutOfRangeException for a 1.x source. WotLK's 144 entries
        // are larger than 132, so the pad is a no-op there (extras are kept).
        while (buttons.Count < PlayerConst.MaxActionButtonsTbc)
            buttons.Add(0);

        GetSession().GameState.ActionButtons = buttons;

        // V3_4_3-only: forward as a standalone modern SMSG_UPDATE_ACTION_BUTTONS. TC
        // reference at packet #151 sends this AFTER the player CreateObject2 — the
        // 1441-byte emission (180 × int64 + 1 byte Reason) is a prerequisite the
        // V3_4_3 client requires before it sends CMSG_MOVE_INIT_ACTIVE_MOVER_COMPLETE
        // and dismisses the loading screen. UpdateActionButtons.Write pads to 180
        // internally, so we don't need to pad the list here.
        if (ModernVersion.IsWotLKClassicOrLater)
        {
            // Hardcode Reason=0 (Initial sync). cmangos sends reason=1 (spec swap)
            // sometimes but the V3_4_3 client interprets reason=0 as "world-entry
            // initial" and reason=1 as "you swapped specs" — only the former gates
            // the world-ready handshake. Matches HermesProxy-WOTLK fork behaviour.
            UpdateActionButtons modern = new UpdateActionButtons
            {
                Reason = 0,
            };
            modern.ActionButtons.AddRange(buttons);
            SendPacketToClient(modern);

            Log.Print(LogType.Trace,
                $"[ActionButtonsTrace] forwarded {modern.ActionButtons.Count} legacy buttons to V3_4_3 client (legacyReason={reason} sentReason=0; Write pads to {PlayerConst.MaxActionButtonsModern})");
        }
    }

    [HandlesSmsg(Opcode.SMSG_ENABLE_BARBER_SHOP)]
    internal void HandleEnableBarberShop(WorldPacket packet)
    {
        // Legacy carries no payload. Native 3.4.3 sends CustomizationScope 0 for a barber chair,
        // which is the only scope a WotLK backend can produce.
        SendPacketToClient(new EnableBarberShop { CustomizationScope = 0 });
    }

    [HandlesSmsg(Opcode.SMSG_BARBER_SHOP_RESULT)]
    internal void HandleBarberShopResult(WorldPacket packet)
    {
        // Legacy writes uint32, modern reads int32, and the result values did not change.
        SendPacketToClient(new BarberShopResult { Result = packet.ReadInt32() });
    }

    [HandlesSmsg(Opcode.SMSG_LOGOUT_RESPONSE)]
    internal void HandleLogoutResponse(WorldPacket packet)
    {
        LogoutResponse logout = new LogoutResponse();
        logout.LogoutResult = packet.ReadInt32();
        logout.Instant = packet.ReadBool();
        SendPacketToClient(logout);
    }

    [HandlesSmsg(Opcode.SMSG_LOGOUT_COMPLETE)]
    internal void HandleLogoutComplete(WorldPacket packet)
    {
        LogoutComplete logout = new LogoutComplete();
        SendPacketToClient(logout);

        GetSession().ReplaceGameState();
        GetSession().ToClient.Detach(Framework.Constants.ConnectionType.Instance);
        GetSession().InstanceSocket.CloseSocket();
        GetSession().InstanceSocket = null!;
    }

    [HandlesSmsg(Opcode.SMSG_LOGOUT_CANCEL_ACK)]
    internal void HandleLogoutCancelAck(WorldPacket packet)
    {
        LogoutCancelAck logout = new LogoutCancelAck();
        SendPacketToClient(logout);
    }

    [HandlesSmsg(Opcode.SMSG_LOG_XP_GAIN)]
    internal void HandleLogXPGain(WorldPacket packet)
    {
        LogXPGain log = new();
        log.Victim = packet.ReadGuid().To128(GetSession().GameState);
        log.Original = packet.ReadInt32();
        log.Reason = (PlayerLogXPReason)packet.ReadUInt8();
        if (log.Reason == PlayerLogXPReason.Kill)
        {
            log.Amount = packet.ReadInt32();
            log.GroupBonus = packet.ReadFloat();
        }
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089) && packet.CanRead())
            log.RAFBonus = packet.ReadUInt8();
        SendPacketToClient(log);
    }

    [HandlesSmsg(Opcode.SMSG_PLAYED_TIME)]
    internal void HandlePlayedTime(WorldPacket packet)
    {
        PlayedTime played = new();
        played.TotalTime = packet.ReadUInt32();
        played.LevelTime = packet.ReadUInt32();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            played.TriggerEvent = packet.ReadBool();
        else
            played.TriggerEvent = GetSession().GameState.ShowPlayedTime;
        SendPacketToClient(played);
    }

    [HandlesSmsg(Opcode.SMSG_LEVEL_UP_INFO)]
    internal void HandleLevelUpInfo(WorldPacket packet)
    {
        LevelUpInfo info = new LevelUpInfo();
        info.Level = packet.ReadInt32();
        info.HealthDelta = packet.ReadInt32();

        // TrinityCore 4.3.4 LevelUpInfo::Write sends six power deltas, one more than a 4.x unit
        // has power fields. Read as five, every stat gain was taken from the slot before its own.
        int powerDeltas = IsCataLegacy ? 6 : LegacyVersion.GetPowersCount();
        for (var i = 0; i < powerDeltas; i++)
            info.PowerDelta[i] = packet.ReadInt32();

        for (var i = 0; i < 5; i++)
            info.StatDelta[i] = packet.ReadInt32();

        SendPacketToClient(info);
    }

    [HandlesSmsg(Opcode.SMSG_UPDATE_COMBO_POINTS)]
    internal void HandleUpdateComboPoints(WorldPacket packet)
    {
        ObjectUpdate updateData = new ObjectUpdate(GetSession().GameState.CurrentPlayerGuid, UpdateTypeModern.Values, GetSession());
        var comboTarget = packet.ReadPackedGuid().To128(GetSession().GameState);
        // Only the V1_14/V2_5 builders read ComboTarget from ActivePlayerData; V3_4_3 writes it
        // from UnitData. Materialising ActivePlayerData for this one field was nearly all of the
        // ~35 KB each combo-point update allocated on V3_4_3.
        if (!ModernVersion.IsWotLKClassicOrLater)
            updateData.EnsureActivePlayerData().ComboTarget = comboTarget;
        updateData.UnitData.ComboTarget = comboTarget;
        byte comboPoints = packet.ReadUInt8();
        sbyte powerSlot = ClassPowerTypes.GetPowerSlotForClass(GetSession().GameState.GetUnitClass(GetSession().GameState.CurrentPlayerGuid), PowerType.ComboPoints);
        if (powerSlot >= 0)
            updateData.UnitData.EnsurePower()[powerSlot] = comboPoints;

        UpdateObject updatePacket = new UpdateObject(GetSession().GameState);
        updatePacket.ObjectUpdates.Add(updateData);
        SendPacketToClient(updatePacket);
    }

    // TrinityCore and AzerothCore send this to the player that moves the unit, and a player moves
    // itself: every combo point change of a player comes twice, once as SMSG_UPDATE_COMBO_POINTS,
    // which the proxy translates, and once here. CMaNGOS sends it for other units only.
    // A modern client keeps a vehicle's combo points on the vehicle, and its combo frame reads them
    // there while the player rides (GetComboPoints("vehicle", "target")). Pets, charmed units and
    // NPCBots' bots are left out: no frame shows theirs, and the class table already gives a bot
    // druid's energy the slot they would take.
    [HandlesSmsg(Opcode.SMSG_PET_UPDATE_COMBO_POINTS)]
    internal void HandlePetUpdateComboPoints(WorldPacket packet)
    {
        var unit = packet.ReadPackedGuid().To128(GetSession().GameState);
        if (unit == GetSession().GameState.CurrentPlayerGuid)
            return;

        var comboTarget = packet.ReadPackedGuid().To128(GetSession().GameState);
        byte comboPoints = packet.ReadUInt8();
        if (!ModernVersion.IsWotLKClassicOrLater || GetSession().GameState.GetVehicleRecId(unit) == 0)
        {
            WorldClientLogMessages.PetComboPointsNotTranslated(_melLog, _sourceFile, _netDirRecv, comboPoints);
            return;
        }

        ObjectUpdate updateData = new ObjectUpdate(unit, UpdateTypeModern.Values, GetSession());
        updateData.UnitData.ComboTarget = comboTarget;
        updateData.UnitData.EnsurePower()[ClassPowerTypes.CreatureComboPointsSlot] = comboPoints;
        // A native server has set this max since the vehicle's create; a legacy one has no field
        // for it, and without it the frame shows no points at all.
        updateData.UnitData.EnsureMaxPower()[ClassPowerTypes.CreatureComboPointsSlot] = 5;

        UpdateObject updatePacket = new UpdateObject(GetSession().GameState);
        updatePacket.ObjectUpdates.Add(updateData);
        SendPacketToClient(updatePacket);
    }

    [HandlesSmsg(Opcode.SMSG_INSPECT_RESULT)]
    [HandlesSmsg(Opcode.SMSG_INSPECT_TALENT)]
    internal void HandleInspectResult(WorldPacket packet)
    {
        InspectResult inspect = new InspectResult();
        // TrinityCore 4.3.4 HandleInspectOpcode writes the GUID whole; 3.x packs it. Read packed
        // on 4.3.4, it named no player the proxy knew and the inspect was dropped without a word.
        if (packet.GetUniversalOpcode(false) == Opcode.SMSG_INSPECT_RESULT || IsCataLegacy)
            inspect.DisplayInfo.GUID = packet.ReadGuid().To128(GetSession().GameState);
        else
            inspect.DisplayInfo.GUID = packet.ReadPackedGuid().To128(GetSession().GameState);

        PlayerCache? cache;
        if (!GetSession().GameState.CachedPlayers.TryGetValue(inspect.DisplayInfo.GUID, out cache) || cache == null)
            return;

        inspect.DisplayInfo.Name = cache.Name ?? string.Empty;
        inspect.DisplayInfo.ClassId = cache.ClassId;
        inspect.DisplayInfo.RaceId = cache.RaceId;
        inspect.DisplayInfo.SexId = cache.SexId;

        var updates = GetSession().GameState.GetCachedObjectFieldsLegacy(inspect.DisplayInfo.GUID);
        if (updates != null)
        {
            int PLAYER_VISIBLE_ITEM_1_0 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_VISIBLE_ITEM_1_0);
            if (PLAYER_VISIBLE_ITEM_1_0 >= 0) // vanilla and tbc
            {
                byte offset = (byte)(LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) ? 16 : 12);
                for (byte i = 0; i < 19; i++)
                {
                    int itemIdIndex = PLAYER_VISIBLE_ITEM_1_0 + i * offset;
                    if (updates.ContainsKey(itemIdIndex))
                    {
                        uint itemId = updates[itemIdIndex].UInt32Value;
                        if (itemId != 0)
                        {
                            InspectItemData itemData = new InspectItemData();
                            itemData.Index = i;
                            itemData.Item.ItemID = itemId;
                            ReadVisibleItemGear(updates, itemIdIndex, offset, itemData);
                            inspect.DisplayInfo.Items.Add(itemData);
                        }
                    }
                }
            }
            // 3.3.5a and 4.3.4 list the items after the talents (ReadInspectedEquipment)
            int PLAYER_GUILDID = LegacyVersion.GetUpdateField(PlayerField.PLAYER_GUILDID);
            if (PLAYER_GUILDID >= 0 && updates.ContainsKey(PLAYER_GUILDID))
            {
                inspect.GuildData = new InspectGuildData();
                inspect.GuildData.GuildGUID = WowGuid128.CreateGuildOrEmpty(updates[PLAYER_GUILDID].UInt32Value);
            }
            int PLAYER_FIELD_BYTES = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_BYTES);
            if (PLAYER_FIELD_BYTES >= 0 && updates.ContainsKey(PLAYER_FIELD_BYTES))
            {
                inspect.LifetimeMaxRank = (byte)((updates[PLAYER_FIELD_BYTES].UInt32Value >> 24) & 0xFF);
            }
        }

        // 2.4.3 has a layout of its own here, a length and a talent bit array. Read as the specs
        // below, a set bit made a spec count that ran off the end of the packet, and the throw
        // cost the client the whole inspect.
        if (packet.GetUniversalOpcode(false) == Opcode.SMSG_INSPECT_TALENT && LegacyVersion.ExpansionVersion >= 3)
        {
            // BuildPlayerTalentsInfoData (no isPet prefix). Unspent points are
            // not a rank-byte count — treating them as one emptied the tree.
            uint unspent = packet.ReadUInt32();
            byte specsCount = packet.ReadUInt8();
            byte activeSpec = packet.ReadUInt8();
            int totalTalents = 0;

            for (byte specIdx = 0; specIdx < specsCount; ++specIdx)
            {
                // 4.3.4 Player::BuildPlayerTalentsInfoData: each group starts with its primary tree.
                if (IsCataLegacy)
                {
                    uint primaryTree = packet.ReadUInt32();
                    if (specIdx == activeSpec)
                        inspect.PrimaryTalentTree = primaryTree;
                }

                byte talentCount = packet.ReadUInt8();
                for (byte t = 0; t < talentCount; ++t)
                {
                    uint talentId = packet.ReadUInt32();
                    byte rank = packet.ReadUInt8();
                    if (specIdx == activeSpec)
                        inspect.InspectedTalents.Add(new TalentEntry { TalentID = talentId, Rank = rank });
                }

                byte glyphSlots = packet.ReadUInt8();
                for (byte g = 0; g < glyphSlots; ++g)
                {
                    ushort glyph = packet.ReadUInt16();
                    if (IsCataLegacy && specIdx == activeSpec)
                        inspect.Glyphs.Add(glyph);
                }

                totalTalents += talentCount;
            }

            WorldClientLogMessages.InspectTalents(
                _melLog, _sourceFile, _netDirRecv, unspent, specsCount, activeSpec, totalTalents);

            if (LegacyVersion.ExpansionVersion >= 3)
                ReadInspectedEquipment(packet, inspect);
        }

        SendPacketToClient(inspect);
    }

    // Player::BuildEnchantmentsInfoData, after the talents (TrinityCore 4.3.4, and AzerothCore,
    // TrinityCore and cmangos on 3.3.5a): what each equipment slot really holds. The visible items
    // carry no gems and name a transmogrification's look instead, so the inspect window showed that
    // item without its enchantment, gems or random suffix. TrinityCore cata_classic fills the 4.4.2
    // list from the items themselves, and the item level from them too.
    private void ReadInspectedEquipment(WorldPacket packet, InspectResult inspect)
    {
        uint usedSlots = packet.ReadUInt32();
        Span<(InventoryType Type, uint Level)> worn = stackalloc (InventoryType, uint)[EquipmentSlots];
        int count = 0;
        for (byte slot = 0; slot < EquipmentSlots; slot++)
        {
            if ((usedSlots & (1u << slot)) == 0)
                continue;

            InspectItemData item = new InspectItemData();
            item.Index = slot;
            item.Usable = true;
            item.Item.ItemID = packet.ReadUInt32();
            ushort enchanted = packet.ReadUInt16();
            for (int legacySlot = 0; legacySlot < 16; legacySlot++)
            {
                if ((enchanted & (1 << legacySlot)) == 0)
                    continue;

                uint enchantId = packet.ReadUInt16();
                int modernSlot = ModernEnchantmentSlot(legacySlot);
                if (modernSlot < 0)
                    continue;
                item.Enchants.Add(new InspectEnchantData(enchantId, (byte)modernSlot));

                int socket = legacySlot - Enums.Classic.EnchantmentSlot.Sock1;
                uint gem = socket is >= 0 and < ItemConst.MaxGemSockets ? GameData.GetGemFromEnchantId(enchantId) : 0;
                if (gem != 0)
                {
                    ItemGemData gemData = new ItemGemData();
                    gemData.Slot = (byte)socket;
                    gemData.Item.ItemID = gem;
                    item.Gems.Add(gemData);
                }
            }
            // signed: a random suffix is the negative id, cut to 16 bits
            item.Item.RandomPropertiesID = (uint)(int)packet.ReadInt16();
            item.CreatorGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
            item.Item.RandomPropertiesSeed = packet.ReadUInt32();
            inspect.DisplayInfo.Items.Add(item);

            if (GameData.TryGetItemLevel(item.Item.ItemID, out var type, out uint level))
                worn[count++] = (type, level);
        }

        inspect.ItemLevel = (int)AverageItemLevel.OfEquipped(inspect.DisplayInfo.ClassId, worn[..count]);
    }

    /// <summary>
    /// Fills an item's enchantments, gems and random properties from a vanilla/TBC player update's
    /// visible-item block. Those builds have no inspect gear packet: the fields the server writes
    /// in <c>Player::SetVisibleItemSlot</c> are the only copy of the item's instance data.
    /// </summary>
    private static void ReadVisibleItemGear(Dictionary<int, UpdateField> updates, int itemIdIndex,
        int offset, InspectItemData itemData)
    {
        int enchantSlots = LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180)
            ? Enums.TBC.EnchantmentSlot.MaxInspected
            : Enums.Vanilla.EnchantmentSlot.MaxInspected;
        for (int slot = 0; slot < enchantSlots; slot++)
        {
            if (!updates.TryGetValue(itemIdIndex + 1 + slot, out UpdateField enchantField) ||
                enchantField.UInt32Value == 0)
                continue;

            uint enchantId = enchantField.UInt32Value;
            int modernSlot = ModernEnchantmentSlot(slot);
            if (modernSlot < 0)
                continue;

            itemData.Enchants.Add(new InspectEnchantData(enchantId, (byte)modernSlot));

            int socket = slot - Enums.Classic.EnchantmentSlot.Sock1;
            uint gem = socket is >= 0 and < ItemConst.MaxGemSockets
                ? GameData.GetGemFromEnchantId(enchantId)
                : 0;
            if (gem != 0)
                itemData.Gems.Add(new ItemGemData { Slot = (byte)socket, Item = new ItemInstance { ItemID = gem } });
        }

        // The block is [creator(2)][entry][enchants and spare slots][properties][suffix factor].
        int propertiesIndex = itemIdIndex + offset - 4;
        if (updates.TryGetValue(propertiesIndex, out UpdateField properties) && properties.UInt32Value != 0)
        {
            // The field is two shorts; the server stores the signed random-property id in the low one.
            itemData.Item.RandomPropertiesID = (uint)(int)(short)properties.UInt32Value;
            if (updates.TryGetValue(propertiesIndex + 1, out UpdateField suffixFactor))
                itemData.Item.RandomPropertiesSeed = suffixFactor.UInt32Value;
        }
    }

    // A legacy enchantment slot as the modern client numbers it. 4.3.4's reforge and
    // transmogrification are item modifiers there, not enchantments; 3.3.5a has no engineering
    // slot, so its random property slots start one earlier than the modern client's.
    private static int ModernEnchantmentSlot(int legacySlot)
    {
        int lastShared = IsCataLegacy ? Enums.Cata.EnchantmentSlot.Engineering : Enums.WotLK.EnchantmentSlot.Prismatic;
        int prop0 = IsCataLegacy ? Enums.Cata.EnchantmentSlot.Prop0 : Enums.WotLK.EnchantmentSlot.Prop0;
        int prop4 = IsCataLegacy ? Enums.Cata.EnchantmentSlot.Prop4 : Enums.WotLK.EnchantmentSlot.Prop4;
        if (legacySlot <= lastShared)
            return legacySlot;
        if (legacySlot >= prop0 && legacySlot <= prop4)
            return legacySlot - prop0 + Enums.Classic.EnchantmentSlot.Prop0;
        return -1;
    }

    [HandlesSmsg(Opcode.MSG_INSPECT_HONOR_STATS, RemovedIn = ClientVersionBuild.V2_0_1_6180)]
    internal void HandleInspectHonorStatsVanilla(WorldPacket packet)
    {
        WowGuid128 playerGuid = packet.ReadGuid().To128(GetSession().GameState);
        byte lifetimeHighestRank = packet.ReadUInt8();
        ushort todayHonorableKills = packet.ReadUInt16();
        ushort todayDishonorableKills = packet.ReadUInt16();
        ushort yesterdayHonorableKills = packet.ReadUInt16();
        ushort yesterdayDishonorableKills = packet.ReadUInt16();
        ushort lastWeekHonorableKills = packet.ReadUInt16();
        ushort lastWeekDishonorableKills = packet.ReadUInt16();
        ushort thisWeekHonorableKills = packet.ReadUInt16();
        ushort thisWeekDishonorableKills = packet.ReadUInt16();
        uint lifetimeHonorableKills = packet.ReadUInt32();
        uint lifetimeDishonorableKills = packet.ReadUInt32();
        uint yesterdayHonor = packet.ReadUInt32();
        uint lastWeekHonor = packet.ReadUInt32();
        uint thisWeekHonor = packet.ReadUInt32();
        uint standing = packet.ReadUInt32();
        byte rankProgress = packet.ReadUInt8();

        if (ModernVersion.ExpansionVersion == 1)
        {
            InspectHonorStatsResultClassic inspect = new InspectHonorStatsResultClassic();
            inspect.PlayerGUID = playerGuid;
            inspect.LifetimeHighestRank = lifetimeHighestRank;
            inspect.TodayHonorableKills = todayHonorableKills;
            inspect.TodayDishonorableKills = todayDishonorableKills;
            inspect.YesterdayHonorableKills = yesterdayHonorableKills;
            inspect.YesterdayDishonorableKills = yesterdayDishonorableKills;
            inspect.LastWeekHonorableKills = lastWeekHonorableKills;
            inspect.LastWeekDishonorableKills = lastWeekDishonorableKills;
            inspect.ThisWeekHonorableKills = thisWeekHonorableKills;
            inspect.ThisWeekDishonorableKills = thisWeekDishonorableKills;
            inspect.LifetimeHonorableKills = lifetimeHonorableKills;
            inspect.LifetimeDishonorableKills = lifetimeDishonorableKills;
            inspect.YesterdayHonor = yesterdayHonor;
            inspect.LastWeekHonor = lastWeekHonor;
            inspect.ThisWeekHonor = thisWeekHonor;
            inspect.Standing = standing;
            inspect.RankProgress = rankProgress;
            SendPacketToClient(inspect);
        }
        else
        {
            InspectHonorStatsResultTBC inspect = new InspectHonorStatsResultTBC();
            inspect.PlayerGUID = playerGuid;
            inspect.LifetimeHighestRank = lifetimeHighestRank;
            inspect.YesterdayHonorableKills = yesterdayHonorableKills;
            inspect.LifetimeHonorableKills = (ushort)lifetimeHonorableKills;
            SendPacketToClient(inspect);
        }
    }

    [HandlesSmsg(Opcode.MSG_INSPECT_HONOR_STATS, AddedIn = ClientVersionBuild.V2_0_1_6180)]
    internal void HandleInspectHonorStatsTBC(WorldPacket packet)
    {
        WowGuid128 playerGuid = packet.ReadGuid().To128(GetSession().GameState);
        byte lifetimeHighestRank = packet.ReadUInt8();
        ushort todayHonorableKills = packet.ReadUInt16();
        ushort yesterdayHonorableKills = packet.ReadUInt16();
        uint todayHonor = packet.ReadUInt32();
        uint yesterdayHonor = packet.ReadUInt32();
        uint lifetimeHonorableKills = packet.ReadUInt32();

        // V3_4_3 answers CMSG_REQUEST_HONOR_STATS with its own 42-byte payload, not the TBC one.
        // Layout taken from the 3.4.3 server's InspectHonorStatsResult::Write and byte-checked
        // against a native 47-byte capture. The counters a 3.3.5a backend cannot supply
        // (dishonorable kills, weekly splits, standing) stay zero rather than being guessed.
        if (ModernVersion.IsWotLKClassicOrLater)
        {
            InspectHonorStatsResultWotLKClassic honor = new InspectHonorStatsResultWotLKClassic();
            honor.PlayerGUID = playerGuid;
            // Left at zero on purpose. A 3.3.5a server fills this byte with honour points
            // truncated to 8 bits (AzerothCore: uint8(GetHonorPoints())), not a rank - forwarding it
            // showed values like 185 and 7. The PvP rank it names ended with TBC, and the native
            // 3.4.3 capture carries 0 here.
            honor.TodayHK = todayHonorableKills;
            honor.YesterdayHK = yesterdayHonorableKills;
            honor.LifeTimeHK = lifetimeHonorableKills;
            honor.YesterdayHonor = yesterdayHonor;
            honor.ThisWeekHonor = todayHonor;
            SendPacketToClient(honor);

            if (_melLog.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                WorldClientLogMessages.InspectHonor(_melLog, _sourceFile, _netDirRecv,
                    playerGuid.Low, lifetimeHighestRank, todayHonorableKills,
                    yesterdayHonorableKills, lifetimeHonorableKills);
            return;
        }

        if (ModernVersion.ExpansionVersion == 1)
        {
            InspectHonorStatsResultClassic inspect = new InspectHonorStatsResultClassic();
            inspect.PlayerGUID = playerGuid;
            inspect.LifetimeHighestRank = lifetimeHighestRank;
            inspect.TodayHonorableKills = todayHonorableKills;
            inspect.YesterdayHonorableKills = yesterdayHonorableKills;
            inspect.LifetimeHonorableKills = lifetimeHonorableKills;
            inspect.YesterdayHonor = yesterdayHonor;
            inspect.LastWeekHonor = todayHonor;
            SendPacketToClient(inspect);
        }
        else
        {
            InspectHonorStatsResultTBC inspect = new InspectHonorStatsResultTBC();
            inspect.PlayerGUID = playerGuid;
            inspect.LifetimeHighestRank = lifetimeHighestRank;
            inspect.YesterdayHonorableKills = yesterdayHonorableKills;
            inspect.LifetimeHonorableKills = (ushort)lifetimeHonorableKills;
            SendPacketToClient(inspect);
        }
    }

    /// <summary>
    /// TrinityCore 4.3.4 <c>InspectHonorStats::Write</c>, the answer to CMSG_REQUEST_HONOR_STATS: the
    /// kills of today, yesterday and all time around a masked GUID. A 4.3.4 server keeps no honor
    /// of the day and leaves the rank 0; the client is given the kills.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_INSPECT_HONOR_STATS)]
    internal void HandleInspectHonorStatsCata(WorldPacket packet)
    {
        Span<bool> mask = stackalloc bool[8];
        Span<byte> guidBytes = stackalloc byte[8];
        MaskedGuid.ReadMaskBits(packet, mask, [4, 3, 6, 2, 5, 0, 7, 1]);
        packet.ReadUInt8();     // lifetime highest rank
        ushort yesterdayHonorableKills = packet.ReadUInt16();
        ushort todayHonorableKills = packet.ReadUInt16();
        MaskedGuid.ReadBytes(packet, mask, guidBytes, [2, 0, 6, 3, 4, 1, 5]);
        uint lifetimeHonorableKills = packet.ReadUInt32();
        MaskedGuid.ReadBytes(packet, mask, guidBytes, [7]);

        InspectHonorStatsResultWotLKClassic honor = new InspectHonorStatsResultWotLKClassic();
        honor.PlayerGUID = new WowGuid64(MaskedGuid.ToUInt64(guidBytes)).To128(GetSession().GameState);
        honor.TodayHK = todayHonorableKills;
        honor.YesterdayHK = yesterdayHonorableKills;
        honor.LifeTimeHK = lifetimeHonorableKills;
        SendPacketToClient(honor);
    }

    [HandlesSmsg(Opcode.MSG_INSPECT_ARENA_TEAMS)]
    internal void HandleInspectArenaTeams(WorldPacket packet)
    {
        InspectPvP inspect = new InspectPvP();
        inspect.PlayerGUID = packet.ReadGuid().To128(GetSession().GameState);
        ArenaTeamInspectData team = new ArenaTeamInspectData();
        byte slot = packet.ReadUInt8();
        uint teamId = packet.ReadUInt32();
        team.TeamGuid = WowGuid128.Create(HighGuidType703.ArenaTeam, teamId);
        team.TeamRating = packet.ReadInt32();
        team.TeamGamesPlayed = packet.ReadInt32();
        team.TeamGamesWon = packet.ReadInt32();
        team.PersonalGamesPlayed = packet.ReadInt32();
        team.PersonalRating = packet.ReadInt32();
        GetSession().GameState.StoreArenaTeamDataForPlayer(inspect.PlayerGUID, slot, team);
        // The server answers one slot per packet, so the other two come from earlier answers. The
        // loop used to add the slot just answered three times.
        for (byte i = 0; i < ArenaTeamConst.MaxArenaSlot; i++)
            inspect.ArenaTeams.Add(GetSession().GameState.GetArenaTeamDataForPlayer(inspect.PlayerGUID, i));
        SendPacketToClient(inspect);
    }

    [HandlesSmsg(Opcode.SMSG_CHARACTER_RENAME_RESULT)]
    internal void HandleCharacterRenameResult(WorldPacket packet)
    {
        byte result = packet.ReadUInt8();

        CharacterRenameResult rename = new();
        rename.Result = ModernVersion.ConvertResponseCodesValue(result);
        if (rename.Result == (byte)Enums.V1_12_1_5875.ResponseCodes.Success)
        {
            rename.Guid = packet.ReadGuid().To128(GetSession().GameState);
            rename.Name = packet.ReadCString();
        }
        SendPacketToClient(rename);
    }

    // Result codes are the same two values on both eras, so this is a GUID width change and
    // nothing else. The client will not leave the "updating character" spinner without it.
    [HandlesSmsg(Opcode.SMSG_SET_PLAYER_DECLINED_NAMES_RESULT)]
    internal void HandleSetPlayerDeclinedNamesResult(WorldPacket packet)
    {
        SetPlayerDeclinedNamesResult declined = new();
        declined.ResultCode = packet.ReadInt32();
        declined.Player = packet.ReadGuid().To128(GetSession().GameState);
        SendPacketToClient(declined);
    }

    // Synthesizes the canonical (MapId, ZoneId, Position) for a freshly-created
    // character when the legacy backend (e.g. cMangos) returns ZoneId=0/MapId=0
    // in SMSG_ENUM_CHARACTERS_RESULT. Without this, the V3_4_3 client rejects
    // the entire enum because FirstLogin=true triggers a starting-zone DB
    // lookup that fails on (0, 0). Death Knights (class 6) take precedence
    // over the race default since they share a starting area regardless of race.
    static void ApplyStartingLocation(EnumCharactersResult.CharacterInfo char1)
    {
        if (char1.ClassId == Class.Deathknight)
        {
            char1.MapId = 609;          // Plaguelands: The Scarlet Enclave
            char1.ZoneId = 4298;
            char1.PreloadPos = new Vector3(2381.33f, -5894.55f, 154.626f);
            return;
        }

        switch (char1.RaceId)
        {
            case Race.Human:
                char1.MapId = 0;        // Eastern Kingdoms
                char1.ZoneId = 12;      // Elwynn Forest
                char1.PreloadPos = new Vector3(-8949.95f, -132.493f, 83.5312f);
                break;
            case Race.Orc:
            case Race.Troll:
                char1.MapId = 1;        // Kalimdor
                char1.ZoneId = 14;      // Durotar
                char1.PreloadPos = new Vector3(-618.518f, -4251.67f, 38.718f);
                break;
            case Race.Dwarf:
            case Race.Gnome:
                char1.MapId = 0;
                char1.ZoneId = 1;       // Dun Morogh
                char1.PreloadPos = new Vector3(-6240.32f, 331.033f, 382.758f);
                break;
            case Race.NightElf:
                char1.MapId = 1;
                char1.ZoneId = 141;     // Teldrassil
                char1.PreloadPos = new Vector3(10311.3f, 832.463f, 1326.41f);
                break;
            case Race.Undead:
                char1.MapId = 0;
                char1.ZoneId = 85;      // Tirisfal Glades
                char1.PreloadPos = new Vector3(1676.71f, 1677.45f, 121.671f);
                break;
            case Race.Tauren:
                char1.MapId = 1;
                char1.ZoneId = 215;     // Mulgore
                char1.PreloadPos = new Vector3(-2917.58f, -257.98f, 52.9968f);
                break;
            case Race.BloodElf:
                char1.MapId = 530;      // Outland
                char1.ZoneId = 3431;    // Eversong Woods
                char1.PreloadPos = new Vector3(10349.6f, -6357.29f, 33.4308f);
                break;
            case Race.Draenei:
                char1.MapId = 530;
                char1.ZoneId = 3524;    // Azuremyst Isle
                char1.PreloadPos = new Vector3(-3961.64f, -13931.2f, 100.615f);
                break;
            // Cataclysm's new races (TrinityCore 4.3.4 playercreateinfo).
            case Race.Goblin:
                char1.MapId = 648;      // Lost Isles
                char1.ZoneId = 4765;    // Kezan
                char1.PreloadPos = new Vector3(-8423.81f, 1361.3f, 104.671f);
                break;
            case Race.Worgen:
                char1.MapId = 654;      // Gilneas
                char1.ZoneId = 4756;    // Gilneas City
                char1.PreloadPos = new Vector3(-1451.53f, 1403.35f, 35.5561f);
                break;
            default:
                // Last-resort fallback to keep the enum valid even for unknown races.
                char1.MapId = 0;
                char1.ZoneId = 12;
                char1.PreloadPos = new Vector3(-8949.95f, -132.493f, 83.5312f);
                break;
        }
    }
}
