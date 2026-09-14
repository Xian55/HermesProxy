using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Auth;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's character CMSGs: the character list, creation and deletion,
/// login and logout, appearance, action bars and inspection.
/// </summary>
/// <remarks>
/// <para>
/// Bodies were moved from <c>World/Server/PacketHandlers/CharacterHandler.cs</c>, not retyped;
/// <c>verify-handler-port.py</c> diffs each one against the original.
/// </para>
/// <para>
/// Two methods were renamed, and only renamed. Three overloads shared the name
/// <c>HandleTogglePvP</c> and only one of them toggles PvP; the other two set a title and set the
/// PvP flag explicitly. verify-handler-port maps old name to new.
/// </para>
/// <para>
/// <see cref="HandlePlayerLogin"/> is the one handler here that reaches back into the socket.
/// <c>AbortLogin</c> and <c>SendConnectToInstance</c> are connection lifecycle rather than packet
/// translation — they open the instance socket and tear the login down — so they stay on
/// <c>WorldSocket</c> and are called through <c>ctx.Socket</c>. Its ordering comment is load-bearing:
/// everything it publishes to GameState must happen before the instance connection opens, because
/// the client starts pushing packets on that socket from another thread the moment it is up.
/// </para>
/// </remarks>
public static class CharacterSystem
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melLog = Log.CreateMelLogger(Log.CategoryPacket);
    private static readonly string _sourceFile = nameof(WorldSocket).PadRight(15);
    private static readonly string _netDirRecv = Log.FormatDir(LogNetDir.C2P);

    [HandlesCmsg(Opcode.CMSG_ENUM_CHARACTERS)]
    public static void HandleEnumCharacters(in EmptyClientPacket charEnum, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_ENUM_CHARACTERS);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_REORDER_CHARACTERS)]
    public static void HandleReorderCharacters(in ReorderCharacters reorder, in SessionContext ctx)
    {
        var realm = ctx.GetSession().Realm;
        if (realm == null || reorder.Entries.Length == 0)
            return;

        var incoming = new List<CharacterListSlot>(reorder.Entries.Length);
        foreach (var entry in reorder.Entries)
            incoming.Add(new CharacterListSlot(entry.PlayerGuid.Low, entry.NewPosition));
        var merged = CharacterListOrder.Merge(
            ctx.GetSession().AccountMetaDataMgr.LoadCharacterListOrder(realm.Name),
            incoming);
        ctx.GetSession().AccountMetaDataMgr.SaveCharacterListOrder(realm.Name, merged);
        Log.Print(LogType.Debug, $"[CharEnum] saved list order count={merged.Count} incoming={incoming.Count} pos=[{string.Join(",", merged.ConvertAll(s => s.ListPosition.ToString()))}]");
    }

    [HandlesCmsg(Opcode.CMSG_GET_ACCOUNT_CHARACTER_LIST)]
    public static void HandleGetAccountCharacterList(in GetAccountCharacterListRequest request, in SessionContext ctx)
    {
        GetAccountCharacterListResult response = new();
        response.Token = request.Token;

        foreach (var ownCharacter in ctx.GetSession().GameState.OwnCharacters)
        {
            response.CharacterList.Add(new AccountCharacterListEntry
            {
                AccountId = WowGuid128.Create(HighGuidType703.WowAccount, ctx.GetSession().GameAccountInfo.Id),
                CharacterGuid = ownCharacter.CharacterGuid,
                RealmVirtualAddress = ctx.GetSession().RealmId.GetAddress(),
                RealmName = "", // If empty the realm name will not be displayed
                LastLoginUnixSec = ownCharacter.LastLoginUnixSec,

                Name = ownCharacter.Name ?? string.Empty,
                Race = ownCharacter.RaceId,
                Class = ownCharacter.ClassId,
                Sex = ownCharacter.SexId,
                Level = ownCharacter.Level,
            });
        }

        ctx.SendPacket(response);
    }

    [HandlesCmsg(Opcode.CMSG_GENERATE_RANDOM_CHARACTER_NAME)]
    public static void HandleGenerateRandomCharacterNameRequest(in GenerateRandomCharacterNameRequest randomCharacterName, in SessionContext ctx)
    {
        GenerateRandomCharacterNameResult result = new();

        // The client can generate the name itself
        result.Success = false;

        ctx.SendPacket(result);
    }

    [HandlesCmsg(Opcode.CMSG_CREATE_CHARACTER)]
    public static void HandleCreateCharacter(in CreateCharacter charCreate, in SessionContext ctx)
    {
        // Cache the requested name so HandleCreateChar can resolve the new char's
        // GUID via an internal CMSG_CHAR_ENUM and stamp it into SMSG_CREATE_CHAR
        // (V3_4_3 client uses that GUID for auto-select on the next char list).
        ctx.GetSession().GameState.PendingCreateCharName = charCreate.CreateInfo.Name;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_CREATE_CHARACTER);
        packet.WriteCString(charCreate.CreateInfo.Name);
        packet.WriteUInt8((byte)charCreate.CreateInfo.RaceId);
        packet.WriteUInt8((byte)charCreate.CreateInfo.ClassId);
        packet.WriteUInt8((byte)charCreate.CreateInfo.Sex);

        CharacterCustomizations.ConvertModernCustomizationsToLegacy(charCreate.CreateInfo.Customizations, out byte skin, out byte face, out byte hairStyle, out byte hairColor, out byte facialhair);
        packet.WriteUInt8(skin);
        packet.WriteUInt8(face);
        packet.WriteUInt8(hairStyle);
        packet.WriteUInt8(hairColor);
        packet.WriteUInt8(facialhair);
        packet.WriteUInt8(0); // outfit
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_CHAR_DELETE)]
    public static void HandleCharDelete(in CharDelete charDelete, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_CHAR_DELETE);
        packet.WriteGuid(charDelete.Guid.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_LOADING_SCREEN_NOTIFY)]
    public static void HandleLoadScreen(in LoadingScreenNotify loadingScreenNotify, in SessionContext ctx)
    {
        // Authoritative map comes from SMSG_LOGIN_VERIFY_WORLD / SMSG_NEW_WORLD.
        // The 3.4.3 client also sends this CMSG when the loading screen hides:
        // MapID=0xFFFFFFFF (cinematic) or MapID=0 + Showing=false (mid-BG relog).
        // Storing either writes UPDATE_OBJECT.MapID=0/65535 and the client
        // drops Values (frozen HP, broken walk). Only keep a real map while showing.
        if (loadingScreenNotify.Showing
            && loadingScreenNotify.MapID != 0
            && loadingScreenNotify.MapID != 0xFFFFFFFFu)
            ctx.GetSession().GameState.CurrentMapId = loadingScreenNotify.MapID;
    }

    [HandlesCmsg(Opcode.CMSG_QUERY_PLAYER_NAME)]
    public static void HandleNameQueryRequest(in QueryPlayerName queryPlayerName, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_NAME_QUERY);
        packet.WriteGuid(queryPlayerName.Player.To64());
        ctx.SendPacketToServer(packet, ctx.GetSession().GameState.IsInWorld ? Opcode.MSG_NULL_ACTION : Opcode.SMSG_LOGIN_VERIFY_WORLD);
    }

    [HandlesCmsg(Opcode.CMSG_QUERY_PLAYER_NAMES)]
    public static void HandleNamesQueryRequest(in QueryPlayerNames queryPlayerNames, in SessionContext ctx)
    {
        foreach (var guid in queryPlayerNames.Players)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_NAME_QUERY);
            packet.WriteGuid(guid.To64());
            ctx.SendPacketToServer(packet, ctx.GetSession().GameState.IsInWorld ? Opcode.MSG_NULL_ACTION : Opcode.SMSG_LOGIN_VERIFY_WORLD);
        }
    }

    [HandlesCmsg(Opcode.CMSG_PLAYER_LOGIN)]
    public static void HandlePlayerLogin(in PlayerLogin playerLogin, in SessionContext ctx)
    {
        if (ctx.GetSession().WorldClient == null || !ctx.GetSession().WorldClient!.IsConnected())
        {
            Log.Print(LogType.Error, "WorldClient is disconnected, cannot enter world.");
            ctx.Socket!.AbortLogin(LoginFailureReason.NoWorld);
            return;
        }

        if (!ctx.GetSession().GameState.CachedPlayers.TryGetValue(playerLogin.Guid, out var selectedChar))
        {
            Log.Print(LogType.Error, $"Player tried to log in with unknown char id: {playerLogin.Guid}");
            ctx.Socket!.AbortLogin(LoginFailureReason.NoCharacter);
            return;
        }

        var realm = ctx.GetSession().RealmManager.GetRealm(ctx.GetSession().RealmId);
        if (realm == null)
        {
            Log.Print(LogType.Error, $"Player tried to log in to unknown realm id: {ctx.GetSession().RealmId}");
            ctx.Socket!.AbortLogin(LoginFailureReason.NoWorld);
            return;
        }

        ctx.GetSession().AccountMetaDataMgr.SaveLastSelectedCharacter(realm.Name, selectedChar.Name!, playerLogin.Guid.Low, Time.UnixTime);
        ctx.GetSession().GameState.CollectionFavorites ??= ctx.GetSession().AccountMetaDataMgr.LoadCollectionFavorites();

        if (ctx.GetSession().AuthClient != null)
            ctx.GetSession().AuthClient.Disconnect();

        // C# forbids capturing an `in` parameter in a lambda, so the guid is hoisted first.
        var loginGuid = playerLogin.Guid;
        var ownCharacter = ctx.GetSession().GameState.OwnCharacters.FirstOrDefault(x => x.CharacterGuid == loginGuid);
        if (ownCharacter == null)
        {
            Log.Print(LogType.Error, $"Player tried to log in with a char missing from the enumerated list: {playerLogin.Guid}");
            ctx.Socket!.AbortLogin(LoginFailureReason.NoCharacter);
            return;
        }

        // All GameState must be published BEFORE the client is told to open the instance
        // connection. The instance socket runs on its own network thread and the client
        // starts pushing packets (CMSG_SET_ACTION_BAR_TOGGLES first) the moment it is up,
        // so anything set after SendConnectToInstance is a live race. That race crashed
        // the proxy with a NullReferenceException on every enter-world attempt.
        ctx.GetSession().GameState.IsFirstEnterWorld = true;
        ctx.GetSession().GameState.CurrentPlayerGuid = playerLogin.Guid;
        ctx.GetSession().GameState.CurrentPlayerInfo = ownCharacter;
        ctx.GetSession().GameState.CurrentPlayerStorage.LoadCurrentPlayer();

        // V3_4_3-only: DKs need rune state in ActivePlayerData CREATE. Without it
        // the client starts up believing all 6 runes are on cooldown and refuses to
        // send rune-cost CMSG_CAST_SPELL until a SpellGo proves otherwise. SMSG_RESYNC_RUNES
        // from the legacy server later overwrites this default with authoritative values.
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261 &&
            ctx.GetSession().GameState.CurrentPlayerInfo!.ClassId == Class.Deathknight)
        {
            ctx.GetSession().GameState.RuneState = new RuneStateData();
        }

        Log.Print(LogType.Server,
            $"[Login] entering world as '{ownCharacter.Name}' ({ownCharacter.RaceId} {ownCharacter.ClassId} lvl {ownCharacter.Level}) " +
            $"guid={playerLogin.Guid} realm='{realm.Name}': state published, opening instance connection");
        ctx.Socket!.SendConnectToInstance(ConnectToSerial.WorldAttempt1);
        ctx.GetSession().GameState.IsConnectedToInstance = true;

        WorldPacket packet = new WorldPacket(Opcode.CMSG_PLAYER_LOGIN);
        packet.WriteGuid(playerLogin.Guid.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_LOGOUT_REQUEST)]
    public static void HandleLogoutRequest(in LogoutRequest logoutRequest, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_LOGOUT_REQUEST);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_LOGOUT_CANCEL)]
    public static void HandleLogoutCancel(in EmptyClientPacket logoutCancel, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_LOGOUT_CANCEL);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_REQUEST_PLAYED_TIME)]
    public static void HandleRequestPlayedTime(in RequestPlayedTime played, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_REQUEST_PLAYED_TIME);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.WriteBool(played.TriggerScriptEvent);
        ctx.SendPacketToServer(packet);
        ctx.GetSession().GameState.ShowPlayedTime = played.TriggerScriptEvent;
    }

    [HandlesCmsg(Opcode.CMSG_SET_TITLE)]
    public static void HandleSetTitle(in SetTitle title, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_TITLE);
        packet.WriteInt32(title.TitleID);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_ALTER_APPEARANCE)]
    public static void HandleAlterAppearance(in AlterAppearance appearance, in SessionContext ctx)
    {
        // Barber shops arrived in 3.0.2; older backends have no opcode to forward to.
        if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            return;

        var gameState = ctx.GetSession().GameState;
        if (!gameState.TryGetCachedPlayerAppearance(gameState.CurrentPlayerGuid, out Race race, out _, out Gender sex))
            return;

        // The legacy barber cannot change gender, and the modern client has already switched to
        // the target gender's customization options by the time it sends this. Mapping those
        // against the character's real gender would pick unrelated styles, and the legacy server
        // would answer a mismatched BarberShopStyle row with silence, so refuse it here instead.
        if (appearance.NewSexId != sex)
        {
            ctx.SendPacket(new BarberShopResult { Result = 1 });
            return;
        }

        CharacterCustomizations.ConvertModernCustomizationsToLegacy(appearance.Customizations,
            out byte skin, out _, out byte hairStyle, out byte hairColor, out byte facialHair);

        uint hairStyleId = GameData.GetBarberShopStyleId(0, race, sex, hairStyle);
        uint facialHairId = GameData.GetBarberShopStyleId(2, race, sex, facialHair);
        uint skinId = GameData.GetBarberShopStyleId(3, race, sex, skin);

        // Types 0 and 2 are mandatory on the legacy side; a row it cannot resolve makes it drop
        // the request without replying, which the client shows as a barber that does nothing.
        if (hairStyleId == 0 || facialHairId == 0)
        {
            WorldSocketLogMessages.BarberShopStyleUnresolved(_melLog, _sourceFile, _netDirRecv, (byte)race, (byte)sex, hairStyle, facialHair, hairStyleId, facialHairId);
            ctx.SendPacket(new BarberShopResult { Result = 1 });
            return;
        }

        WorldPacket packet = new WorldPacket(Opcode.CMSG_ALTER_APPEARANCE);
        packet.WriteUInt32(hairStyleId);
        packet.WriteUInt32(hairColor);
        packet.WriteUInt32(facialHairId);
        packet.WriteUInt32(skinId);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_TOGGLE_PVP)]
    public static void HandleTogglePvP(in EmptyClientPacket pvp, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_TOGGLE_PVP);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SET_PVP)]
    public static void HandleSetPvP(in SetPvP pvp, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_TOGGLE_PVP);
        packet.WriteBool(pvp.Enable);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SET_ACTION_BUTTON)]
    public static void HandleSetActionButton(in SetActionButton button, in SessionContext ctx)
    {
        // Legacy 3.3.5a CMSG_SET_ACTION_BUTTON wire format (per mangos-wotlk
        // Player.cpp + WPP V3_4_0_45166 ActionBarHandler.cs:12-19):
        //   byte 0   = button index (uint8)
        //   bytes 1-4 = packed uint32 (low 24 bits = action ID, high 8 bits = type)
        // Total: 5 bytes.
        //
        // The CLIENT-side wire format differs by version:
        //   - V1_14 / V2_5 (per WPP V1_13_2 ActionBarHandler.cs): two
        //     INDEPENDENT fields — Int16 Action + Int16 Type + Byte Slot.
        //     SetActionButton.Read() reads them as such; we just repack the
        //     byte-shifted legacy form.
        //   - V3_4_3 (per WPP V3_4_0_45166): Int32 packed (low 24 = action,
        //     high 8 = ActionButtonType) + Byte Slot. SetActionButton.Read()
        //     splits that int32 across two uint16 fields (Action = low 16,
        //     Type = high 16 = (action_high8 | (type<<8))), so the V3_4_3
        //     branch must recombine the bits.
        //
        // Picking the wrong branch silently miscoded macros/items/mounts/
        // equipment-sets/companions as SPELLs with truncated IDs — observed
        // crashing the V3_4_3 client when the resulting bogus slot was
        // rendered on a side bar.
        ushort actionLow16 = button.Action;
        ushort typeHi16    = button.Type;
        uint actionReal;
        byte typeReal;
        if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
        {
            actionReal = (uint)actionLow16 | (((uint)typeHi16 & 0xFFu) << 16);
            typeReal   = (byte)((typeHi16 >> 8) & 0xFFu);
        }
        else
        {
            // V1_14 / V2_5 / pre-V3_4_3: independent uint16 fields.
            actionReal = (uint)actionLow16;
            typeReal   = (byte)(typeHi16 & 0xFFu);
        }
        uint packed = (actionReal & 0x00FFFFFFu) | ((uint)typeReal << 24);

        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_ACTION_BUTTON);
        packet.WriteUInt8(button.Index);
        packet.WriteUInt32(packed);
        ctx.SendPacketToServer(packet);

        Log.Print(LogType.Debug,
            $"[V343Trace][SaveButton] modern→legacy idx={button.Index} ({DescribeActionButtonSlot(button.Index)}) " +
            $"wire(action=0x{actionLow16:X4} type=0x{typeHi16:X4}) build={ModernVersion.Build} → " +
            $"actionReal={actionReal} typeReal=0x{typeReal:X2} ({DescribeActionButtonType(typeReal)}) " +
            $"packedLE=0x{packed:X8}");
    }

    [HandlesCmsg(Opcode.CMSG_SET_ACTION_BAR_TOGGLES)]
    public static void HandleSetActionBarToggles(in SetActionBarToggles bars, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_ACTION_BAR_TOGGLES);
        packet.WriteUInt8(bars.Mask);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_UNLEARN_SKILL)]
    public static void HandleUnlearnSkill(in UnlearnSkill skill, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_UNLEARN_SKILL);
        packet.WriteUInt32(skill.SkillLine);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_PLAYER_SHOWING_CLOAK)]
    [HandlesCmsg(Opcode.CMSG_PLAYER_SHOWING_HELM)]
    public static void HandleShowHelmOrCloak(Opcode opcode, in PlayerShowingHelmOrCloak show, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(opcode);
        packet.WriteBool(show.Showing);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_INSPECT)]
    public static void HandleInspect(in Inspect inspect, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_INSPECT);
        packet.WriteGuid(inspect.Target.To64());
        ctx.SendPacketToServer(packet);
    }

    /// <remarks>
    /// V3_4_3 renamed this to <c>CMSG_REQUEST_HONOR_STATS</c> (0x317E) from
    /// <c>CMSG_INSPECT_HONOR_STATS</c> (0x317) — same request, same single-GUID body. A native
    /// 3.4.3 server answers it with <c>SMSG_INSPECT_HONOR_STATS</c> and only it; the client sends it
    /// when the inspect PvP tab opens, so there is nothing to request ahead of that.
    /// </remarks>
    [HandlesCmsg(Opcode.CMSG_INSPECT_HONOR_STATS)]
    [HandlesCmsg(Opcode.CMSG_REQUEST_HONOR_STATS)]
    public static void HandleInspectHonorStats(in Inspect inspect, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_INSPECT_HONOR_STATS);
        packet.WriteGuid(inspect.Target.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_INSPECT_PVP)]
    public static void HandleInspectArenaTeams(in Inspect inspect, in SessionContext ctx)
    {
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            WorldPacket packet = new WorldPacket(Opcode.MSG_INSPECT_ARENA_TEAMS);
            packet.WriteGuid(inspect.Target.To64());
            ctx.SendPacketToServer(packet);
        }
        else
        {
            InspectPvP pvp = new InspectPvP();
            pvp.PlayerGUID = inspect.Target;
            pvp.ArenaTeams.Add(new ArenaTeamInspectData());
            pvp.ArenaTeams.Add(new ArenaTeamInspectData());
            pvp.ArenaTeams.Add(new ArenaTeamInspectData());
            ctx.SendPacket(pvp);
        }
    }

    [HandlesCmsg(Opcode.CMSG_CHARACTER_RENAME_REQUEST)]
    public static void HandleCharacterRenameRequest(in CharacterRenameRequest rename, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_CHARACTER_RENAME_REQUEST);
        packet.WriteGuid(rename.Guid.To64());
        packet.WriteCString(rename.NewName);
        ctx.SendPacketToServer(packet);
    }

    private static string DescribeActionButtonSlot(byte idx) => idx switch
    {
        < 12  => $"MainBar btn{idx + 1}",
        < 24  => $"BonusBar btn{idx - 11}",
        < 36  => $"MultiBarRight (Bar 4) btn{idx - 23}",
        < 48  => $"MultiBarLeft (Bar 5) btn{idx - 35}",
        < 60  => $"MultiBarBottomRight (Bar 3) btn{idx - 47}",
        < 72  => $"MultiBarBottomLeft (Bar 2) btn{idx - 59}",
        < 84  => $"PetBar/Bonus2 btn{idx - 71}",
        < 144 => $"slot {idx}",
        _     => $"OOB slot {idx}"
    };

    private static string DescribeActionButtonType(byte t) => t switch
    {
        0x00 => "SPELL",
        0x01 => "C",
        0x20 => "EQSET",
        0x30 => "DROPDOWN",
        0x40 => "MACRO",
        0x50 => "CMACRO",
        0x60 => "MOUNT",
        0x80 => "ITEM",
        0x90 => "COMPANION",
        _    => $"unknown 0x{t:X2}"
    };
}
