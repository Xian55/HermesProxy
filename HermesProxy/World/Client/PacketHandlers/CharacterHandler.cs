using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System.Collections.Generic;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [PacketHandler(Opcode.SMSG_ENUM_CHARACTERS_RESULT)]
    void HandleEnumCharactersResult(WorldPacket packet)
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

        byte count = packet.ReadUInt8();
        Log.Print(LogType.Network, $"[CharEnum] legacy count={count}");
        uint virtualRealmAddress = GetSession().Realm?.Id.GetAddress() ?? 0u;
        Log.Print(LogType.Trace, $"[Trace] HandleEnumCharactersResult: realm.GetAddress()=0x{virtualRealmAddress:X8} ({virtualRealmAddress})");
        for (byte i = 0; i < count; i++)
        {
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
            char1.Guid = packet.ReadGuid().To128(GetSession().GameState);
            char1.Name = cache.Name = packet.ReadCString();
            char1.RaceId = cache.RaceId = (Race)packet.ReadUInt8();
            char1.ClassId = cache.ClassId = (Class)packet.ReadUInt8();
            char1.SexId = cache.SexId = (Gender)packet.ReadUInt8();

            byte skin = packet.ReadUInt8();
            byte face = packet.ReadUInt8();
            byte hairStyle = packet.ReadUInt8();
            byte hairColor = packet.ReadUInt8();
            byte facialHair = packet.ReadUInt8();
            char1.Customizations = CharacterCustomizations.ConvertLegacyCustomizationsToModern((Race)char1.RaceId, (Gender)char1.SexId, skin, face, hairStyle, hairColor, facialHair);

            char1.ExperienceLevel = cache.Level = packet.ReadUInt8();
            if (char1.ExperienceLevel > charEnum.MaxCharacterLevel)
                charEnum.MaxCharacterLevel = char1.ExperienceLevel;

            GetSession().GameState.UpdatePlayerCache(char1.Guid, cache);

            char1.ZoneId = packet.ReadUInt32();
            char1.MapId = packet.ReadUInt32();
            char1.PreloadPos = packet.ReadVector3();
            uint guildId = packet.ReadUInt32();
            GetSession().GameState.StorePlayerGuildId(char1.Guid, guildId);
            char1.GuildGuid = guildId != 0 ? WowGuid128.Create(HighGuidType703.Guild, guildId) : WowGuid128.Empty;
            char1.Flags = (CharacterFlags)packet.ReadUInt32();
            // Attempt 2 (per _charenum_diff_report.md): TC sends Flags=0 for ALL
            // chars (verified across all 4 chars in TC capture). cMangos sets the
            // Declined bit (0x02000000) on all chars by default. Strip it here to
            // match TC's wire pattern. Combined with the ListPosition=0 change
            // above — neither change individually unblocked rendering this evening.
            char1.Flags &= ~CharacterFlags.Declined;

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                char1.Flags2 = packet.ReadUInt32(); // Customization Flags

            byte legacyFirstLogin = packet.ReadUInt8();
            char1.FirstLogin = legacyFirstLogin != 0;

            // V3_4_3 client validates ZoneId/MapId against the race's starting-zone DB
            // iff FirstLogin=true. cMangos returns ZoneId=0/MapId=0 for a freshly-created
            // char, which fails that check and the whole enum is rejected. Synthesize
            // the canonical starting (Map, Zone, Position) per race so the validation
            // passes — the legacy server will overwrite these on actual login. Keeping
            // FirstLogin=true is what tells the V3_4_3 client to auto-select this char
            // as the just-created one (the previous blanket-false override broke that).
            if (char1.FirstLogin
                && ModernVersion.Build == ClientVersionBuild.V3_4_3_54261
                && (char1.MapId == 0 && char1.ZoneId == 0))
            {
                ApplyStartingLocation(char1);
            }
            char1.PetCreatureDisplayId = packet.ReadUInt32();
            char1.PetExperienceLevel = packet.ReadUInt32();
            char1.PetCreatureFamilyId = packet.ReadUInt32();

            for (int j = EquipmentSlot.Start; j < EquipmentSlot.End; j++)
            {
                char1.VisualItems[j].DisplayId = packet.ReadUInt32();
                char1.VisualItems[j].InvType = packet.ReadUInt8();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    char1.VisualItems[j].DisplayEnchantId = packet.ReadUInt32();
            }

            int bagCount = LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685) ? 4 : 1;
            for (int j = 0; j < bagCount; j++)
            {
                char1.VisualItems[EquipmentSlot.Bag1 + j].DisplayId = packet.ReadUInt32();
                char1.VisualItems[EquipmentSlot.Bag1 + j].InvType = packet.ReadUInt8();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    char1.VisualItems[EquipmentSlot.Bag1 + j].DisplayEnchantId = packet.ReadUInt32();
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
            Log.Print(LogType.Network,
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
        Log.Print(LogType.Network,
            $"[Trace] HandleEnumCharactersResult: SEND — chars={charEnum.Characters.Count} maxLvl={charEnum.MaxCharacterLevel} " +
            $"races={charEnum.RaceUnlockData.Count} success={charEnum.Success} isNewPlayer={charEnum.IsNewPlayer} " +
            $"disabledClassesMask={(charEnum.DisabledClassesMask.HasValue ? "set" : "null")}");
        SendPacketToClient(charEnum);
        Log.Print(LogType.Trace, "[Trace] HandleEnumCharactersResult: EXIT — translation complete, packet queued for modern client");
    }

    [PacketHandler(Opcode.SMSG_CREATE_CHAR)]
    void HandleCreateChar(WorldPacket packet)
    {
        byte result = packet.ReadUInt8();

        CreateChar createChar = new CreateChar();
        createChar.Guid = new WowGuid128();
        createChar.Code = ModernVersion.ConvertResponseCodesValue(result);
        SendPacketToClient(createChar);
    }

    [PacketHandler(Opcode.SMSG_DELETE_CHAR)]
    void HandleDeleteChar(WorldPacket packet)
    {
        byte result = packet.ReadUInt8();

        DeleteChar deleteChar = new DeleteChar();
        deleteChar.Code = ModernVersion.ConvertResponseCodesValue(result);
        SendPacketToClient(deleteChar);
    }

    [PacketHandler(Opcode.SMSG_QUERY_PLAYER_NAME_RESPONSE)]
    void HandleQueryPlayerNameResponse(WorldPacket packet)
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
        response.Data.VirtualRealmAddress = GetSession().RealmId.GetAddress();
        SendPacketToClient(response);
    }

    [PacketHandler(Opcode.SMSG_LOGIN_VERIFY_WORLD)]
    void HandleLoginVerifyWorld(WorldPacket packet)
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

        bool isModernWotLK = ModernVersion.ExpansionVersion >= 3;

        // V3_4_3 modern client expects EmptyInitWorldStates BEFORE WorldServerInfo
        // (per HermesProxy-WOTLK fork's WorldClient.cs:1106).
        if (isModernWotLK)
        {
            EmptyInitWorldStates worldStates = new();
            worldStates.MapId = verify.MapID;
            SendPacketToClient(worldStates);
        }

        WorldServerInfo info = new();
        if (verify.MapID > 1)
        {
            info.DifficultyID = 1;
            info.InstanceGroupSize = 5;
        }
        SendPacketToClient(info);

        // SetAllTaskProgress is for pre-WotLK-Classic clients only — V3_4_3 doesn't expect it
        // and the fork explicitly skips it for ExpansionVersion >= 3.
        if (!isModernWotLK)
        {
            SetAllTaskProgress tasks = new();
            SendPacketToClient(tasks);
        }

        InitialSetup setup = new();
        setup.ServerExpansionLevel = (byte)(LegacyVersion.ExpansionVersion - 1);
        SendPacketToClient(setup);

        LoadCUFProfiles cuf = new();
        cuf.Data = GetSession().AccountDataMgr.LoadCUFProfiles();
        SendPacketToClient(cuf);

        // V3_4_3 modern client requires a batch of "I have no X data" announcements
        // before it dismisses the loading screen and sends CMSG_MOVE_INIT_ACTIVE_MOVER_COMPLETE.
        // Legacy 3.3.5a doesn't emit these (the modern protocol added them), so the proxy
        // synthesizes empty/default versions. Ported from HermesProxy-WOTLK fork's
        // WorldClient.cs:1130-1147 — this is the actual unblock for the loading-screen stall.
        if (isModernWotLK)
        {
            SendPacketToClient(new EmptyAllAchievementData());
            SendPacketToClient(new EmptyAllAccountCriteria());
            SendPacketToClient(new EmptySetupCurrency());
            SendPacketToClient(new EmptySpellHistory());
            SendPacketToClient(new EmptySpellCharges());
            SendPacketToClient(new EmptyTalentData());
            SendPacketToClient(new EmptyActiveGlyphs());
            SendPacketToClient(new EmptyEquipmentSetList());
            SendPacketToClient(new EmptyAccountMountUpdate());
            SendPacketToClient(new EmptyAccountToyUpdate());
            SendPacketToClient(new EmptyAccountHeirloomUpdate());
            SendPacketToClient(new BattlePetJournalLockAcquired());

            PhaseShiftChange phaseShift = new();
            phaseShift.Client = GetSession().GameState.CurrentPlayerGuid;
            SendPacketToClient(phaseShift);

            // TC reference (`World_login_parsed.txt` packet #138) sends
            // SMSG_MOVE_SET_ACTIVE_MOVER BEFORE the player CreateObject — the client
            // immediately acks with CMSG_SET_ACTIVE_MOVER (#139), then receives the
            // CreateObject batch. This early "the GUID you're about to receive is
            // your active mover" announcement is what wires the client's input layer
            // to the player object as soon as it appears.
            MoveSetActiveMover setActiveMover = new()
            {
                MoverGUID = GetSession().GameState.CurrentPlayerGuid,
            };
            SendPacketToClient(setActiveMover);
        }
    }

    [PacketHandler(Opcode.SMSG_CHARACTER_LOGIN_FAILED)]
    void HandleCharacterLoginFailed(WorldPacket packet)
    {
        CharacterLoginFailed failed = new CharacterLoginFailed();
        failed.Code = (Framework.Constants.LoginFailureReason)packet.ReadUInt8();
        SendPacketToClient(failed);

        GetSession().GameState.IsInWorld = false;
    }

    [PacketHandler(Opcode.SMSG_UPDATE_ACTION_BUTTONS)]
    void HandleUpdateActionButtons(WorldPacket packet)
    {
        byte reason = 0;
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
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
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
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

    [PacketHandler(Opcode.SMSG_LOGOUT_RESPONSE)]
    void HandleLogoutResponse(WorldPacket packet)
    {
        LogoutResponse logout = new LogoutResponse();
        logout.LogoutResult = packet.ReadInt32();
        logout.Instant = packet.ReadBool();
        SendPacketToClient(logout);
    }

    [PacketHandler(Opcode.SMSG_LOGOUT_COMPLETE)]
    void HandleLogoutComplete(WorldPacket packet)
    {
        LogoutComplete logout = new LogoutComplete();
        SendPacketToClient(logout);

        GetSession().GameState = GameSessionData.CreateNewGameSessionData(GetSession());
        GetSession().InstanceSocket.CloseSocket();
        GetSession().InstanceSocket = null!;
    }

    [PacketHandler(Opcode.SMSG_LOGOUT_CANCEL_ACK)]
    void HandleLogoutCancelAck(WorldPacket packet)
    {
        LogoutCancelAck logout = new LogoutCancelAck();
        SendPacketToClient(logout);
    }

    [PacketHandler(Opcode.SMSG_LOG_XP_GAIN)]
    void HandleLogXPGain(WorldPacket packet)
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

    [PacketHandler(Opcode.SMSG_PLAYED_TIME)]
    void HandlePlayedTime(WorldPacket packet)
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

    [PacketHandler(Opcode.SMSG_LEVEL_UP_INFO)]
    void HandleLevelUpInfo(WorldPacket packet)
    {
        LevelUpInfo info = new LevelUpInfo();
        info.Level = packet.ReadInt32();
        info.HealthDelta = packet.ReadInt32();

        for (var i = 0; i < LegacyVersion.GetPowersCount(); i++)
            info.PowerDelta[i] = packet.ReadInt32();

        for (var i = 0; i < 5; i++)
            info.StatDelta[i] = packet.ReadInt32();

        SendPacketToClient(info);
    }

    [PacketHandler(Opcode.SMSG_UPDATE_COMBO_POINTS)]
    void HandleUpdateComboPoints(WorldPacket packet)
    {
        ObjectUpdate updateData = new ObjectUpdate(GetSession().GameState.CurrentPlayerGuid, UpdateTypeModern.Values, GetSession());
        updateData.ActivePlayerData.ComboTarget = packet.ReadPackedGuid().To128(GetSession().GameState);
        byte comboPoints = packet.ReadUInt8();
        sbyte powerSlot = ClassPowerTypes.GetPowerSlotForClass(GetSession().GameState.GetUnitClass(GetSession().GameState.CurrentPlayerGuid), PowerType.ComboPoints);
        if (powerSlot >= 0)
            updateData.UnitData.Power[powerSlot] = comboPoints;

        UpdateObject updatePacket = new UpdateObject(GetSession().GameState);
        updatePacket.ObjectUpdates.Add(updateData);
        SendPacketToClient(updatePacket);
    }

    [PacketHandler(Opcode.SMSG_INSPECT_RESULT)]
    [PacketHandler(Opcode.SMSG_INSPECT_TALENT)]
    void HandleInspectResult(WorldPacket packet)
    {
        InspectResult inspect = new InspectResult();
        if (packet.GetUniversalOpcode(false) == Opcode.SMSG_INSPECT_RESULT)
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
                    if (updates.ContainsKey(PLAYER_VISIBLE_ITEM_1_0 + i * offset))
                    {
                        uint itemId = updates[PLAYER_VISIBLE_ITEM_1_0 + i * offset].UInt32Value;
                        if (itemId != 0)
                        {
                            InspectItemData itemData = new InspectItemData();
                            itemData.Index = i;
                            itemData.Item.ItemID = itemId;
                            inspect.DisplayInfo.Items.Add(itemData);
                        }
                    }
                }
            }
            int PLAYER_VISIBLE_ITEM_1_ENTRYID = LegacyVersion.GetUpdateField(PlayerField.PLAYER_VISIBLE_ITEM_1_ENTRYID);
            if (PLAYER_VISIBLE_ITEM_1_ENTRYID >= 0) // wotlk
            {
                int offset = 2;
                for (byte i = 0; i < 19; i++)
                {
                    if (updates.ContainsKey(PLAYER_VISIBLE_ITEM_1_ENTRYID + i * offset))
                    {
                        uint itemId = updates[PLAYER_VISIBLE_ITEM_1_ENTRYID + i * offset].UInt32Value;
                        if (itemId != 0)
                        {
                            InspectItemData itemData = new InspectItemData();
                            itemData.Index = i;
                            itemData.Item.ItemID = itemId;
                            inspect.DisplayInfo.Items.Add(itemData);
                        }
                    }
                }
            }
            int PLAYER_GUILDID = LegacyVersion.GetUpdateField(PlayerField.PLAYER_GUILDID);
            if (PLAYER_GUILDID >= 0 && updates.ContainsKey(PLAYER_GUILDID))
            {
                inspect.GuildData = new InspectGuildData();
                inspect.GuildData.GuildGUID = WowGuid128.Create(HighGuidType703.Guild, updates[PLAYER_GUILDID].UInt32Value);
            }
            int PLAYER_FIELD_BYTES = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_BYTES);
            if (PLAYER_FIELD_BYTES >= 0 && updates.ContainsKey(PLAYER_FIELD_BYTES))
            {
                inspect.LifetimeMaxRank = (byte)((updates[PLAYER_FIELD_BYTES].UInt32Value >> 24) & 0xFF);
            }
        }

        // TODO: format seems to be different in new client
        if (packet.GetUniversalOpcode(false) == Opcode.SMSG_INSPECT_TALENT)
        {
            uint talentsCount = packet.ReadUInt32();
            for (uint i = 0; i < talentsCount; i++)
            {
                byte talent = packet.ReadUInt8();
                if (i < 25)
                    inspect.Talents.Add(talent);
            }
        }

        SendPacketToClient(inspect);
    }

    [PacketHandler(Opcode.MSG_INSPECT_HONOR_STATS, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
    void HandleInspectHonorStatsVanilla(WorldPacket packet)
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

    [PacketHandler(Opcode.MSG_INSPECT_HONOR_STATS, ClientVersionBuild.V2_0_1_6180)]
    void HandleInspectHonorStatsTBC(WorldPacket packet)
    {
        WowGuid128 playerGuid = packet.ReadGuid().To128(GetSession().GameState);
        byte lifetimeHighestRank = packet.ReadUInt8();
        ushort todayHonorableKills = packet.ReadUInt16();
        ushort yesterdayHonorableKills = packet.ReadUInt16();
        uint todayHonor = packet.ReadUInt32();
        uint yesterdayHonor = packet.ReadUInt32();
        uint lifetimeHonorableKills = packet.ReadUInt32();

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

    [PacketHandler(Opcode.MSG_INSPECT_ARENA_TEAMS)]
    void HandleInspectArenaTeams(WorldPacket packet)
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
        for (byte i = 0; i < ArenaTeamConst.MaxArenaSlot; i++)
            inspect.ArenaTeams.Add(GetSession().GameState.GetArenaTeamDataForPlayer(inspect.PlayerGUID, slot));
        SendPacketToClient(inspect);
    }

    [PacketHandler(Opcode.SMSG_CHARACTER_RENAME_RESULT)]
    void HandleCharacterRenameResult(WorldPacket packet)
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
            default:
                // Last-resort fallback to keep the enum valid even for unknown races.
                char1.MapId = 0;
                char1.ZoneId = 12;
                char1.PreloadPos = new Vector3(-8949.95f, -132.493f, 83.5312f);
                break;
        }
    }
}
