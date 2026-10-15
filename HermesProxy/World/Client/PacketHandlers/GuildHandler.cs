using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;
using static HermesProxy.World.Server.Packets.QueryGuildInfoResponse.GuildInfo;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    // TrinityCore 4.3.4 sends its results as the second opcode (GuildCommandResult is
    // SMSG_GUILD_COMMAND_RESULT_2 there), in the layout the first one always had.
    [HandlesSmsg(Opcode.SMSG_GUILD_COMMAND_RESULT)]
    [HandlesSmsg(Opcode.SMSG_GUILD_COMMAND_RESULT_2)]
    internal void HandleGuildCommandResult(WorldPacket packet)
    {
        GuildCommandResult result = new();
        result.Command = (GuildCommandType)packet.ReadUInt32();
        result.Name = packet.ReadCString();
        result.Result = (GuildCommandError)packet.ReadUInt32();
        SendPacketToClient(result);
    }

    [HandlesSmsg(Opcode.MSG_GUILD_PERMISSIONS)]
    internal void HandleGuildPermissions(WorldPacket packet)
    {
        // Guild::SendPermissions unsubscribes us from bank delta updates every time it
        // runs — deliberately, as AzerothCore's "only reliable way to handle /reload".
        // It fires both for a client permissions query and unprompted after a tab
        // purchase (HandleBuyBankTab ends with SendPermissions), so keying off this
        // inbound packet catches both. Issue #157.
        GetSession().GameState.GuildBankSubscribed = false;

        GuildPermissionsQueryResults result = new();
        result.RankID = packet.ReadUInt32();
        result.Flags = packet.ReadInt32();
        result.WithdrawGoldLimit = packet.ReadInt32();
        result.NumTabs = packet.ReadInt8();
        for (int i = 0; i < GuildConst.MaxBankTabs; i++)
        {
            GuildRankTabPermissions tab = new();
            tab.Flags = packet.ReadInt32();
            tab.WithdrawItemLimit = packet.ReadInt32();
            result.Tab.Add(tab);
        }
        SendPacketToClient(result);
    }

    /// <summary>
    /// TrinityCore 4.3.4 Guild::SendPermissions: the rank, the tabs bought, the rank's rights and
    /// daily gold, then a bit-counted list of tab rights.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_GUILD_PERMISSIONS_QUERY_RESULTS)]
    internal void HandleGuildPermissionsQueryResults(WorldPacket packet)
    {
        GuildPermissionsQueryResults result = new();
        result.RankID = packet.ReadUInt32();
        result.NumTabs = (int)packet.ReadUInt32();
        result.Flags = (int)(packet.ReadUInt32() & ~CataRankRightEmpty);
        result.WithdrawGoldLimit = (int)packet.ReadUInt32();
        uint count = packet.ReadBits<uint>(23);
        if (count > GuildConst.MaxBankTabsCata)
            return; // a bank has eight tabs; anything larger is not this layout

        for (int i = 0; i < count; i++)
        {
            GuildRankTabPermissions tab = new();
            tab.Flags = packet.ReadInt32();
            tab.WithdrawItemLimit = packet.ReadInt32();
            result.Tab.Add(tab);
        }
        SendPacketToClient(result);
    }

    /// <summary>TrinityCore 4.3.4 Guild::SendMoneyInfo.</summary>
    [HandlesSmsg(Opcode.SMSG_GUILD_BANK_REMAINING_WITHDRAW_MONEY)]
    internal void HandleGuildBankRemainingWithdrawMoney(WorldPacket packet)
    {
        GuildBankRemainingWithdrawMoney result = new();
        result.RemainingWithdrawMoney = packet.ReadInt64();
        SendPacketToClient(result);
    }

    /// <summary>TrinityCore 4.3.4 Guild::SendMemberUpdateNote.</summary>
    [HandlesSmsg(Opcode.SMSG_GUILD_MEMBER_UPDATE_NOTE)]
    internal void HandleGuildMemberUpdateNote(WorldPacket packet)
    {
        Span<bool> mask = stackalloc bool[8];
        MaskedGuid.ReadMaskBits(packet, mask, [7, 2, 3]);
        uint noteLength = packet.ReadBits<uint>(8);
        MaskedGuid.ReadMaskBits(packet, mask, [5, 0, 6, 4]);
        bool isPublic = packet.HasBit();
        MaskedGuid.ReadMaskBits(packet, mask, [1]);

        Span<byte> guidBytes = stackalloc byte[8];
        MaskedGuid.ReadBytes(packet, mask, guidBytes, [3, 0, 2, 5]);
        string note = packet.ReadString(noteLength);
        MaskedGuid.ReadBytes(packet, mask, guidBytes, [7, 6, 1, 4]);

        GuildMemberUpdateNote update = new();
        update.Member = new WowGuid64(MaskedGuid.ToUInt64(guidBytes)).To128(GetSession().GameState);
        update.Note = note;
        update.IsPublic = isPublic;
        SendPacketToClient(update);
    }

    [HandlesSmsg(Opcode.SMSG_GUILD_EVENT)]
    internal void HandleGuildEvent(WorldPacket packet)
    {
        GuildEventType eventType = (GuildEventType)packet.ReadUInt8();
        if (IsCataLegacy)
        {
            if ((byte)eventType == CataGuildEventFounder)
            {
                SendGuildFounded();
                return;
            }
            if (!TryGuildEventFromCata((byte)eventType, out eventType))
                return;
        }

        var size = packet.ReadUInt8();
        string[] strings = new string[size];
        for (var i = 0; i < size; i++)
            strings[i] = packet.ReadCString();

        WowGuid128 guid = WowGuid128.Empty;
        if (packet.CanRead())
            guid = packet.ReadGuid().To128(GetSession().GameState);

        switch (eventType)
        {
            case GuildEventType.Promotion:
            case GuildEventType.Demotion:
            {
                WowGuid128 officer = GetSession().GameState.GetPlayerGuidByName(strings[0]);
                WowGuid128 player = GetSession().GameState.GetPlayerGuidByName(strings[1]);
                uint rankId = GetSession().GetGuildRankIdByName(GetSession().GameState.GetPlayerGuildId(GetSession().GameState.CurrentPlayerGuid), strings[2]);
                if (officer != default && player != default)
                {
                    GuildSendRankChange promote = new GuildSendRankChange();
                    promote.Officer = officer;
                    promote.Other = player;
                    promote.Promote = eventType == GuildEventType.Promotion;
                    promote.RankID = rankId;
                    SendPacketToClient(promote);
                    GetSession().GameState.GuildClub.MemberRankChanged(player, (int)rankId);
                }
                break;
            }
            case GuildEventType.MOTD:
            {
                GuildEventMotd motd = new GuildEventMotd();
                motd.MotdText = strings[0];
                SendPacketToClient(motd);
                break;
            }
            case GuildEventType.PlayerJoined:
            {
                GuildEventPlayerJoined joined = new GuildEventPlayerJoined();
                joined.Guid = guid;
                joined.VirtualRealmAddress = GetSession().RealmId.GetAddress();
                joined.Name = strings[0];
                SendPacketToClient(joined);
                GetSession().GameState.GuildClub.MemberJoined(
                    guid != default ? guid : GetSession().GameState.GetPlayerGuidByName(strings[0]));
                break;
            }
            case GuildEventType.PlayerLeft:
            {
                GuildEventPlayerLeft left = new GuildEventPlayerLeft();
                left.Removed = false;
                left.LeaverGUID = guid;
                left.LeaverVirtualRealmAddress = GetSession().RealmId.GetAddress();
                left.LeaverName = strings[0];
                SendPacketToClient(left);
                GetSession().GameState.GuildClub.MemberLeft(
                    guid != default ? guid : GetSession().GameState.GetPlayerGuidByName(strings[0]), removed: false);
                break;
            }
            case GuildEventType.PlayerRemoved:
            {
                GuildEventPlayerLeft removed = new GuildEventPlayerLeft();
                removed.Removed = true;
                removed.LeaverGUID = guid;
                removed.LeaverVirtualRealmAddress = GetSession().RealmId.GetAddress();
                removed.LeaverName = strings[0];
                removed.RemoverGUID = GetSession().GameState.GetPlayerGuidByName(strings[1]);
                removed.RemoverVirtualRealmAddress = GetSession().RealmId.GetAddress();
                removed.RemoverName = strings[1];
                SendPacketToClient(removed);
                GetSession().GameState.GuildClub.MemberLeft(
                    guid != default ? guid : GetSession().GameState.GetPlayerGuidByName(strings[0]), removed: true);
                break;
            }
            case GuildEventType.LeaderIs:
            {
                break;
            }
            case GuildEventType.LeaderChanged:
            {
                WowGuid128 oldLeader = GetSession().GameState.GetPlayerGuidByName(strings[0]);
                WowGuid128 newLeader = GetSession().GameState.GetPlayerGuidByName(strings[1]);
                if (oldLeader != default && newLeader != default)
                {
                    GuildEventNewLeader leader = new GuildEventNewLeader();
                    leader.OldLeaderGUID = oldLeader;
                    leader.OldLeaderVirtualRealmAddress = GetSession().RealmId.GetAddress();
                    leader.OldLeaderName = strings[0];
                    leader.NewLeaderGUID = newLeader;
                    leader.NewLeaderVirtualRealmAddress = GetSession().RealmId.GetAddress();
                    leader.NewLeaderName = strings[1];
                    SendPacketToClient(leader);
                    GetSession().GameState.GuildClub.LeaderChanged(oldLeader, newLeader);
                }
                break;
            }
            case GuildEventType.Disbanded:
            {
                GuildEventDisbanded disband = new GuildEventDisbanded();
                SendPacketToClient(disband);
                GetSession().GameState.GuildClub.Disbanded();
                break;
            }
            case GuildEventType.TabardChange:
            {
                break;
            }
            case GuildEventType.RankUpdated:
            {
                GuildEventRanksUpdated ranks = new GuildEventRanksUpdated();
                SendPacketToClient(ranks);
                break;
            }
            case GuildEventType.Unk11:
            {
                break;
            }
            case GuildEventType.PlayerSignedOn:
            case GuildEventType.PlayerSignedOff:
            {
                GuildEventPresenceChange presence = new GuildEventPresenceChange();
                presence.Guid = guid;
                presence.VirtualRealmAddress = GetSession().RealmId.GetAddress();
                presence.LoggedOn = eventType == GuildEventType.PlayerSignedOn;
                presence.Name = strings[0];
                SendPacketToClient(presence);
                break;
            }
            case GuildEventType.BankBagSlotsChanged:
            {
                break;
            }
            case GuildEventType.BankTabPurchased:
            {
                GuildEventTabAdded tab = new GuildEventTabAdded();
                SendPacketToClient(tab);
                break;
            }
            case GuildEventType.BankTabUpdated:
            {
                // AC: _BroadcastEvent(GE_BANK_TAB_UPDATED, _, to_string(tabId), name, icon)
                // Writing tab=0 / name=tabId / icon=name makes 3.4.3 apply a
                // non-texture to tab 0 and the whole strip turns into ?.
                GuildEventTabModified tab = new GuildEventTabModified();
                if (strings.Length >= 3 && int.TryParse(strings[0], out int tabId))
                {
                    tab.Tab = tabId;
                    tab.Name = strings[1];
                    tab.Icon = strings[2];
                }
                else if (strings.Length >= 2)
                {
                    tab.Name = strings[0];
                    tab.Icon = strings[1];
                }
                SendPacketToClient(tab);
                break;
            }
            case GuildEventType.BankMoneyUpdate:
            {
                GuildEventBankMoneyChanged money = new GuildEventBankMoneyChanged();
                // sixteen hex digits on every server; a 4.3.4 bank can hold more than an int32 of copper
                money.Money = UInt64.Parse(strings[0], System.Globalization.NumberStyles.HexNumber);
                SendPacketToClient(money);
                break;
            }
            case GuildEventType.BankMoneyWithdraw:
            {
                break;
            }
            case GuildEventType.BankTextChanged:
            {
                GuildEventTabTextChanged tab = new GuildEventTabTextChanged();
                SendPacketToClient(tab);
                break;
            }
        }
    }

    /// <summary>
    /// TrinityCore 4.3.4 Guild::SendGuildRanksUpdate, to every member when one is given a rank: the
    /// setter's and the member's GUID masks with the promote bit among them, the rank, then the
    /// GUID bytes. The older servers tell a promotion as a guild event.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_GUILD_SEND_RANK_CHANGE)]
    internal void HandleGuildSendRankChange(WorldPacket packet)
    {
        Span<bool> setterMask = stackalloc bool[8];
        Span<bool> memberMask = stackalloc bool[8];
        MaskedGuid.ReadMaskBits(packet, setterMask, [7, 2]);
        MaskedGuid.ReadMaskBits(packet, memberMask, [2]);
        MaskedGuid.ReadMaskBits(packet, setterMask, [1]);
        MaskedGuid.ReadMaskBits(packet, memberMask, [1, 7, 0, 5, 4]);
        bool promote = packet.HasBit();
        MaskedGuid.ReadMaskBits(packet, setterMask, [5, 0]);
        MaskedGuid.ReadMaskBits(packet, memberMask, [6]);
        MaskedGuid.ReadMaskBits(packet, setterMask, [3, 6]);
        MaskedGuid.ReadMaskBits(packet, memberMask, [3]);
        MaskedGuid.ReadMaskBits(packet, setterMask, [4]);
        uint rank = packet.ReadUInt32();

        Span<byte> setter = stackalloc byte[8];
        Span<byte> member = stackalloc byte[8];
        MaskedGuid.ReadBytes(packet, setterMask, setter, [3]);
        MaskedGuid.ReadBytes(packet, memberMask, member, [7]);
        MaskedGuid.ReadBytes(packet, setterMask, setter, [6, 2]);
        MaskedGuid.ReadBytes(packet, memberMask, member, [5, 0]);
        MaskedGuid.ReadBytes(packet, setterMask, setter, [7, 5]);
        MaskedGuid.ReadBytes(packet, memberMask, member, [2, 1]);
        MaskedGuid.ReadBytes(packet, setterMask, setter, [0, 4, 1]);
        MaskedGuid.ReadBytes(packet, memberMask, member, [3, 6, 4]);

        GuildSendRankChange change = new GuildSendRankChange();
        change.Officer = new WowGuid64(MaskedGuid.ToUInt64(setter)).To128(GetSession().GameState);
        change.Other = new WowGuid64(MaskedGuid.ToUInt64(member)).To128(GetSession().GameState);
        change.Promote = promote;
        change.RankID = rank;
        SendPacketToClient(change);
        GetSession().GameState.GuildClub.MemberRankChanged(change.Other, (int)rank);
    }

    // TrinityCore 4.3.4 GE_FOUNDER, broadcast by Guild::Create when the guild has only its founder.
    private const byte CataGuildEventFounder = 15;

    /// <summary>
    /// A guild was just made, by this session's character: the only member a founder event reaches.
    /// A stock 4.4.2 realm tells that as a new leader with no old one (TrinityCore cata_classic
    /// Guild::Create, SendEventNewLeader(leader, nullptr)), and a native capture has it right after
    /// the player joined.
    /// </summary>
    private void SendGuildFounded()
    {
        WowGuid128 founder = GetSession().GameState.CurrentPlayerGuid;
        GuildEventNewLeader leader = new GuildEventNewLeader();
        leader.SelfPromoted = false;
        leader.NewLeaderGUID = founder;
        leader.NewLeaderVirtualRealmAddress = GetSession().RealmId.GetAddress();
        leader.NewLeaderName = GetSession().GameState.GetPlayerName(founder);
        SendPacketToClient(leader);
    }

    /// <summary>
    /// TrinityCore 4.3.4 GR_RIGHT_EMPTY, part of every one of its rank rights. A modern client reads
    /// the bit as GR_RIGHT_ROSTER, which a stock realm leaves off the ranks it makes, so it is taken
    /// off on the way to the client and put back on the way to the server, whose right checks
    /// (Guild::_HasRankRight) pass every right for a rank stored without it.
    /// </summary>
    internal const uint CataRankRightEmpty = 0x40;

    /// <summary>
    /// 4.3.4 counts guild events from 1 and has some the older servers lack (TrinityCore 4.3.4
    /// Guild.h GuildEvents). The three rank events are all "the ranks changed" to this handler;
    /// the mobile sign-on has nothing to be turned into, the founder is read before this.
    /// </summary>
    private static bool TryGuildEventFromCata(byte cataEvent, out GuildEventType eventType)
    {
        eventType = cataEvent switch
        {
            >= 1 and <= 11 => (GuildEventType)(cataEvent - 1),
            12 or 13 or 14 => GuildEventType.RankUpdated,
            16 => GuildEventType.PlayerSignedOn,
            17 => GuildEventType.PlayerSignedOff,
            18 => GuildEventType.BankBagSlotsChanged,
            19 => GuildEventType.BankTabPurchased,
            20 => GuildEventType.BankTabUpdated,
            21 => GuildEventType.BankMoneyUpdate,
            22 => GuildEventType.BankMoneyWithdraw,
            23 => GuildEventType.BankTextChanged,
            _ => GuildEventType.Unk11,
        };
        return eventType != GuildEventType.Unk11;
    }

    /// <summary>
    /// TrinityCore 4.3.4 QueryGuildInfoResponse::Write: the guild's whole GUID, the name, ten rank
    /// names, their orders and their ids, the emblem, and how many of the ten are in use.
    /// </summary>
    private void HandleQueryGuildInfoResponseCata(WorldPacket packet)
    {
        const int MaxRanks = 10;

        QueryGuildInfoResponse guild = new();
        uint guildId = (uint)packet.ReadUInt64();
        guild.GuildGUID = WowGuid128.Create(HighGuidType703.Guild, guildId);
        guild.PlayerGuid = GetSession().GameState.CurrentPlayerGuid;
        guild.HasGuildInfo = true;
        guild.Info = new QueryGuildInfoResponse.GuildInfo();
        guild.Info.GuildGuid = guild.GuildGUID;
        guild.Info.VirtualRealmAddress = GetSession().RealmId.GetAddress();

        guild.Info.GuildName = packet.ReadCString();
        GetSession().StoreGuildGuidAndName(guild.GuildGUID, guild.Info.GuildName);

        string[] names = new string[MaxRanks];
        for (int i = 0; i < MaxRanks; i++)
            names[i] = packet.ReadCString();
        Span<uint> orders = stackalloc uint[MaxRanks];
        for (int i = 0; i < MaxRanks; i++)
            orders[i] = packet.ReadUInt32();
        Span<uint> ids = stackalloc uint[MaxRanks];
        for (int i = 0; i < MaxRanks; i++)
            ids[i] = packet.ReadUInt32();

        guild.Info.EmblemStyle = packet.ReadUInt32();
        guild.Info.EmblemColor = packet.ReadUInt32();
        guild.Info.BorderStyle = packet.ReadUInt32();
        guild.Info.BorderColor = packet.ReadUInt32();
        guild.Info.BackgroundColor = packet.ReadUInt32();

        uint ranksInUse = Math.Min(packet.ReadUInt32(), MaxRanks);
        List<string> rankNames = new List<string>();
        for (int i = 0; i < ranksInUse; i++)
        {
            RankInfo rank = new RankInfo();
            rank.RankID = ids[i];
            rank.RankOrder = orders[i];
            rank.RankName = names[i];
            rankNames.Add(names[i]);
            guild.Info.Ranks.Add(rank);
        }
        GetSession().StoreGuildRankNames(guildId, rankNames);

        SendPacketToClient(guild);
        // The name may be what the player's guild club was waiting for.
        GetSession().GameState.GuildClub.Sync();
    }

    /// <summary>
    /// TrinityCore 4.3.4 GuildRanks::Write, the answer to CMSG_GUILD_GET_RANKS. The older servers
    /// send the ranks inside the roster.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_GUILD_RANKS)]
    internal void HandleGuildRanks(WorldPacket packet)
    {
        int count = (int)packet.ReadBits<uint>(18);
        if (count > 64)
            return; // a guild has ten ranks; anything larger is not this layout

        Span<int> nameLengths = stackalloc int[count];
        for (int i = 0; i < count; i++)
            nameLengths[i] = (int)packet.ReadBits<uint>(7);

        GuildRanks ranks = new GuildRanks();
        List<string> rankNames = new List<string>();
        for (int i = 0; i < count; i++)
        {
            GuildRankData rank = new GuildRankData();
            rank.RankID = (byte)packet.ReadInt32();
            for (int tab = 0; tab < GuildConst.MaxBankTabsCata; tab++)
            {
                rank.TabWithdrawItemLimit[tab] = (uint)packet.ReadInt32();
                rank.TabFlags[tab] = (uint)packet.ReadInt32();
            }
            rank.WithdrawGoldLimit = packet.ReadInt32();
            rank.Flags = (uint)packet.ReadInt32() & ~CataRankRightEmpty;
            rank.RankName = packet.ReadString((uint)nameLengths[i]);
            rank.RankOrder = (uint)packet.ReadInt32();
            rankNames.Add(rank.RankName);
            ranks.Ranks.Add(rank);
        }
        GetSession().StoreGuildRankNames(GetSession().GameState.GetPlayerGuildId(GetSession().GameState.CurrentPlayerGuid), rankNames);
        SendPacketToClient(ranks);
        GetSession().GameState.GuildClub.RanksReceived(ranks);
    }

    /// <summary>
    /// TrinityCore 4.3.4 GuildRoster::Write: every member's GUID mask and string lengths as bits
    /// first, then the members with the GUID bytes strewn among their fields, then the guild's
    /// own texts and numbers.
    /// </summary>
    private void HandleGuildRosterCata(WorldPacket packet)
    {
        GuildRoster guild = new();
        uint welcomeLength = packet.ReadBits<uint>(11);
        int count = (int)packet.ReadBits<uint>(18);
        if (count > 2000)
            return; // far past any guild; not this layout

        bool[] masks = new bool[count * 8];
        bool[] authenticated = new bool[count];
        bool[] sorEligible = new bool[count];
        int[] noteLengths = new int[count];
        int[] officerNoteLengths = new int[count];
        int[] nameLengths = new int[count];
        for (int i = 0; i < count; i++)
        {
            Span<bool> mask = masks.AsSpan(i * 8, 8);
            MaskedGuid.ReadMaskBits(packet, mask, [3, 4]);
            authenticated[i] = packet.HasBit();
            sorEligible[i] = packet.HasBit();
            noteLengths[i] = (int)packet.ReadBits<uint>(8);
            officerNoteLengths[i] = (int)packet.ReadBits<uint>(8);
            MaskedGuid.ReadMaskBits(packet, mask, [0]);
            nameLengths[i] = (int)packet.ReadBits<uint>(7);
            MaskedGuid.ReadMaskBits(packet, mask, [1, 2, 6, 5, 7]);
        }
        uint infoLength = packet.ReadBits<uint>(12);

        Span<byte> guidBytes = stackalloc byte[8];
        for (int i = 0; i < count; i++)
        {
            ReadOnlySpan<bool> mask = masks.AsSpan(i * 8, 8);
            guidBytes.Clear();

            GuildRosterMemberData member = new GuildRosterMemberData();
            PlayerCache cache = new PlayerCache();
            member.VirtualRealmAddress = GetSession().RealmId.GetAddress();
            member.Authenticated = authenticated[i];
            member.SorEligible = sorEligible[i];

            member.ClassID = cache.ClassId = (Class)packet.ReadUInt8();
            member.GuildReputation = (int)packet.ReadUInt32();
            MaskedGuid.ReadBytes(packet, mask, guidBytes, [0]);
            member.WeeklyXP = (long)packet.ReadUInt64();
            member.RankID = (int)packet.ReadUInt32();
            member.PersonalAchievementPoints = (int)packet.ReadUInt32();
            for (int profession = 0; profession < 2; profession++)
            {
                member.Profession[profession].Step = (int)packet.ReadUInt32();
                member.Profession[profession].Rank = (int)packet.ReadUInt32();
                member.Profession[profession].DbID = (int)packet.ReadUInt32();
            }
            MaskedGuid.ReadBytes(packet, mask, guidBytes, [2]);
            member.Status = packet.ReadUInt8();
            member.AreaID = (int)packet.ReadUInt32();
            member.TotalXP = (long)packet.ReadUInt64();
            MaskedGuid.ReadBytes(packet, mask, guidBytes, [7]);
            member.GuildRepToCap = (int)packet.ReadUInt32();
            member.Note = packet.ReadString((uint)noteLengths[i]);
            MaskedGuid.ReadBytes(packet, mask, guidBytes, [3]);
            member.Level = cache.Level = packet.ReadUInt8();
            packet.ReadInt32();                     // VirtualRealmAddress: the proxy's own realm stands
            MaskedGuid.ReadBytes(packet, mask, guidBytes, [5, 4]);
            member.SexID = cache.SexId = (Gender)packet.ReadUInt8();
            MaskedGuid.ReadBytes(packet, mask, guidBytes, [1]);
            member.LastSave = packet.ReadFloat();
            member.OfficerNote = packet.ReadString((uint)officerNoteLengths[i]);
            MaskedGuid.ReadBytes(packet, mask, guidBytes, [6]);
            member.Name = cache.Name = packet.ReadString((uint)nameLengths[i]);

            member.Guid = new WowGuid64(MaskedGuid.ToUInt64(guidBytes)).To128(GetSession().GameState);
            member.GuildClubMemberID = GuildClub.MemberIdOf(member.Guid);
            if (GetSession().GameState.CachedPlayers.TryGetValue(member.Guid, out var existing)
                && existing.RaceId != Race.None)
                member.RaceID = existing.RaceId;
            GetSession().GameState.UpdatePlayerCache(member.Guid, cache);
            guild.MemberData.Add(member);
        }

        guild.InfoText = packet.ReadString(infoLength);
        guild.WelcomeText = packet.ReadString(welcomeLength);
        guild.NumAccounts = packet.ReadUInt32();
        packet.ReadUInt32();                        // WeeklyRepCap
        guild.CreateDate = packet.ReadPackedTime();
        guild.GuildFlags = (int)packet.ReadUInt32();
        SendPacketToClient(guild);
        GetSession().GameState.GuildClub.RosterReceived(guild);
    }

    /// <summary>
    /// TrinityCore 4.3.4 Guild::SendInvite: the guild's level and emblem, then the new and the
    /// old guild's GUID masks with three name lengths among them, then the bytes and the names.
    /// </summary>
    private void HandleGuildInviteCata(WorldPacket packet)
    {
        GuildInvite invite = new();
        packet.ReadUInt32();                        // guild level
        invite.BorderStyle = packet.ReadUInt32();
        invite.BorderColor = packet.ReadUInt32();
        invite.EmblemStyle = packet.ReadUInt32();
        invite.BackgroundColor = packet.ReadUInt32();
        invite.EmblemColor = packet.ReadUInt32();

        Span<bool> newMask = stackalloc bool[8];
        Span<bool> oldMask = stackalloc bool[8];
        MaskedGuid.ReadMaskBits(packet, newMask, [3, 2]);
        uint oldGuildNameLength = packet.ReadBits<uint>(8);
        MaskedGuid.ReadMaskBits(packet, newMask, [1]);
        MaskedGuid.ReadMaskBits(packet, oldMask, [6, 4, 1, 5, 7, 2]);
        MaskedGuid.ReadMaskBits(packet, newMask, [7, 0, 6]);
        uint guildNameLength = packet.ReadBits<uint>(8);
        MaskedGuid.ReadMaskBits(packet, oldMask, [3, 0]);
        MaskedGuid.ReadMaskBits(packet, newMask, [5]);
        uint inviterNameLength = packet.ReadBits<uint>(7);
        MaskedGuid.ReadMaskBits(packet, newMask, [4]);

        Span<byte> newBytes = stackalloc byte[8];
        Span<byte> oldBytes = stackalloc byte[8];
        MaskedGuid.ReadBytes(packet, newMask, newBytes, [1]);
        MaskedGuid.ReadBytes(packet, oldMask, oldBytes, [3]);
        MaskedGuid.ReadBytes(packet, newMask, newBytes, [6]);
        MaskedGuid.ReadBytes(packet, oldMask, oldBytes, [2, 1]);
        MaskedGuid.ReadBytes(packet, newMask, newBytes, [0]);
        invite.OldGuildName = packet.ReadString(oldGuildNameLength);
        MaskedGuid.ReadBytes(packet, newMask, newBytes, [7, 2]);
        invite.InviterName = packet.ReadString(inviterNameLength);
        MaskedGuid.ReadBytes(packet, oldMask, oldBytes, [7, 6, 5, 0]);
        MaskedGuid.ReadBytes(packet, newMask, newBytes, [4]);
        invite.GuildName = packet.ReadString(guildNameLength);
        MaskedGuid.ReadBytes(packet, newMask, newBytes, [5, 3]);
        MaskedGuid.ReadBytes(packet, oldMask, oldBytes, [4]);

        uint realm = GetSession().RealmId.GetAddress();
        invite.InviterVirtualRealmAddress = realm;
        invite.GuildVirtualRealmAddress = realm;
        invite.OldGuildVirtualRealmAddress = realm;
        // the low half of a 4.3.4 guild GUID is the guild id
        uint newGuildId = (uint)MaskedGuid.ToUInt64(newBytes);
        uint oldGuildId = (uint)MaskedGuid.ToUInt64(oldBytes);
        invite.GuildGUID = WowGuid128.Create(HighGuidType703.Guild, newGuildId);
        if (oldGuildId != 0)
            invite.OldGuildGUID = WowGuid128.Create(HighGuidType703.Guild, oldGuildId);
        GetSession().StoreGuildGuidAndName(invite.GuildGUID, invite.GuildName);
        SendPacketToClient(invite);
    }

    [HandlesSmsg(Opcode.SMSG_QUERY_GUILD_INFO_RESPONSE)]
    internal void HandleQueryGuildInfoResponse(WorldPacket packet)
    {
        if (IsCataLegacy)
        {
            HandleQueryGuildInfoResponseCata(packet);
            return;
        }

        QueryGuildInfoResponse guild = new();
        uint guildId = packet.ReadUInt32();
        guild.GuildGUID = WowGuid128.Create(HighGuidType703.Guild, guildId);
        guild.PlayerGuid = GetSession().GameState.CurrentPlayerGuid;
        guild.HasGuildInfo = true;
        guild.Info = new QueryGuildInfoResponse.GuildInfo();
        guild.Info.GuildGuid = guild.GuildGUID;
        guild.Info.VirtualRealmAddress = GetSession().RealmId.GetAddress();

        guild.Info.GuildName = packet.ReadCString();
        GetSession().StoreGuildGuidAndName(guild.GuildGUID, guild.Info.GuildName);

        List<string> ranks = new List<string>();
        for (uint i = 0; i < 10; i++)
        {
            string rankName = packet.ReadCString();
            if (!String.IsNullOrEmpty(rankName))
            {
                RankInfo rank = new RankInfo();
                rank.RankID = i;
                rank.RankOrder = i;
                rank.RankName = rankName;
                ranks.Add(rankName);
                guild.Info.Ranks.Add(rank);
            }
        }
        GetSession().StoreGuildRankNames(guildId, ranks);

        guild.Info.EmblemStyle = packet.ReadUInt32();
        guild.Info.EmblemColor = packet.ReadUInt32();
        guild.Info.BorderStyle = packet.ReadUInt32();
        guild.Info.BorderColor = packet.ReadUInt32();
        guild.Info.BackgroundColor = packet.ReadUInt32();

        SendPacketToClient(guild);
        GetSession().GameState.GuildClub.Sync();
    }

    [HandlesSmsg(Opcode.SMSG_GUILD_INFO)]
    internal void HandleGuildInfo(WorldPacket packet)
    {
        packet.ReadCString(); // Guild Name

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            GetSession().GameState.CurrentGuildCreateTime = packet.ReadPackedTime();
        else
        {
            int day = packet.ReadInt32();
            int month = packet.ReadInt32();
            int year = packet.ReadInt32();

            DateTime date;
            try
            {
                date = new DateTime(year, month, day);
                GetSession().GameState.CurrentGuildCreateTime = (uint)Time.DateTimeToUnixTime(date);
            }
            catch
            {
                Log.Print(LogType.Error, $"Invalid guild create date: {day}-{month}-{year}");
            }
        }

        packet.ReadUInt32(); // Players Count

        GetSession().GameState.CurrentGuildNumAccounts = packet.ReadUInt32();
    }

    [HandlesSmsg(Opcode.SMSG_GUILD_ROSTER)]
    internal void HandleGuildRoster(WorldPacket packet)
    {
        if (IsCataLegacy)
        {
            HandleGuildRosterCata(packet);
            return;
        }

        GuildRoster guild = new();
        var membersCount = packet.ReadUInt32();

        if (GetSession().GameState.CurrentGuildNumAccounts != 0)
            guild.NumAccounts = GetSession().GameState.CurrentGuildNumAccounts;
        else
            guild.NumAccounts = membersCount;

        guild.WelcomeText = packet.ReadCString();
        guild.InfoText = packet.ReadCString();

        if (GetSession().GameState.CurrentGuildCreateTime != 0)
            guild.CreateDate = GetSession().GameState.CurrentGuildCreateTime;
        else
            guild.CreateDate = (uint)Time.UnixTime;

        var ranksCount = packet.ReadInt32();
        if (ranksCount > 0)
        {
            GuildRanks ranks = new GuildRanks();
            for (byte i = 0; i < ranksCount; i++)
            {
                GuildRankData rank = new GuildRankData();
                rank.RankID = i;
                rank.RankOrder = i;
                rank.RankName = GetSession().GetGuildRankNameById(GetSession().GameState.GetPlayerGuildId(GetSession().GameState.CurrentPlayerGuid), i);
                rank.Flags = packet.ReadUInt32();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                {
                    rank.WithdrawGoldLimit = packet.ReadInt32();

                    for (var j = 0; j < GuildConst.MaxBankTabs; j++)
                    {
                        rank.TabFlags[j] = packet.ReadUInt32();
                        rank.TabWithdrawItemLimit[j] = packet.ReadUInt32();
                    }
                }
                ranks.Ranks.Add(rank);
            }
            SendPacketToClient(ranks);
            GetSession().GameState.GuildClub.RanksReceived(ranks);
        }
        

        for (var i = 0; i < membersCount; i++)
        {
            GuildRosterMemberData member = new GuildRosterMemberData();
            PlayerCache cache = new PlayerCache();
            member.Guid = packet.ReadGuid().To128(GetSession().GameState);
            member.GuildClubMemberID = GuildClub.MemberIdOf(member.Guid);
            member.VirtualRealmAddress = GetSession().RealmId.GetAddress();
            member.Status = packet.ReadUInt8();
            member.Name = cache.Name = packet.ReadCString();
            member.RankID = packet.ReadInt32();
            member.Level = cache.Level = packet.ReadUInt8();
            member.ClassID = cache.ClassId =(Class)packet.ReadUInt8();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089))
                member.SexID = cache.SexId = (Gender)packet.ReadUInt8();
            if (GetSession().GameState.CachedPlayers.TryGetValue(member.Guid, out var existing)
                && existing.RaceId != Race.None)
                member.RaceID = existing.RaceId;
            GetSession().GameState.UpdatePlayerCache(member.Guid, cache);
            member.AreaID = packet.ReadInt32();

            if (member.Status == 0)
                member.LastSave = packet.ReadFloat();
            else
                member.Authenticated = true;

            member.Note = packet.ReadCString();
            member.OfficerNote = packet.ReadCString();
            guild.MemberData.Add(member);
        }
        SendPacketToClient(guild);
        GetSession().GameState.GuildClub.RosterReceived(guild);
    }

    [HandlesSmsg(Opcode.SMSG_GUILD_INVITE)]
    internal void HandleGuildInvite(WorldPacket packet)
    {
        if (IsCataLegacy)
        {
            HandleGuildInviteCata(packet);
            return;
        }

        GuildInvite invite = new();
        invite.InviterName = packet.ReadCString();
        invite.InviterVirtualRealmAddress = GetSession().RealmId.GetAddress();
        invite.GuildName = packet.ReadCString();
        invite.GuildVirtualRealmAddress = GetSession().RealmId.GetAddress();
        invite.GuildGUID = GetSession().GetGuildGuid(invite.GuildName);
        SendPacketToClient(invite);
    }

    [HandlesSmsg(Opcode.MSG_TABARDVENDOR_ACTIVATE)]
    internal void HandleTabardVendorActivate(WorldPacket packet)
    {
        PlayerTabardVendorActivate activate = new();
        activate.DesignerGUID = packet.ReadGuid().To128(GetSession().GameState);
        SendPacketToClient(activate);
    }

    [HandlesSmsg(Opcode.MSG_SAVE_GUILD_EMBLEM)]
    internal void HandleSaveGuildEmblem(WorldPacket packet)
    {
        PlayerSaveGuildEmblem emblem = new();
        emblem.Error = (GuildEmblemError)packet.ReadUInt32();
        SendPacketToClient(emblem);
    }

    [HandlesSmsg(Opcode.SMSG_GUILD_INVITE_DECLINED)]
    internal void HandleGuildInviteDeclined(WorldPacket packet)
    {
        GuildInviteDeclined invite = new();
        invite.InviterName = packet.ReadCString();
        invite.InviterVirtualRealmAddress = GetSession().RealmId.GetAddress();
        SendPacketToClient(invite);
    }

    [HandlesSmsg(Opcode.SMSG_GUILD_BANK_QUERY_RESULTS)]
    internal void HandleGuildBankQueryResults(WorldPacket packet)
    {
        if (IsCataLegacy)
        {
            HandleGuildBankQueryResultsCata(packet);
            return;
        }

        GuildBankQueryResults result = new();
        result.Money = packet.ReadUInt64();
        result.Tab = packet.ReadUInt8();
        result.WithdrawalsRemaining = packet.ReadInt32();

        bool hasTabs = false;
        if (packet.ReadBool() && result.Tab == 0)
        {
            hasTabs = true;
            var size = packet.ReadUInt8();
            for (var i = 0; i < size; i++)
            {
                GuildBankTabInfo tabInfo = new GuildBankTabInfo();
                tabInfo.TabIndex = i;
                tabInfo.Name = packet.ReadCString();
                tabInfo.Icon = packet.ReadCString();
                result.TabInfo.Add(tabInfo);
            }

            // Remember how many tabs actually exist. A query for an index at or beyond
            // this is the client opening the "Buy new guild bank tab" slot, which must
            // not trigger an activate — see HandleGuildBankQueryTab. Issue #157.
            GetSession().GameState.GuildBankPurchasedTabs = size;
        }

        var slots = packet.ReadUInt8();
        for (var i = 0; i < slots; i++)
        {
            GuildBankItemInfo itemInfo = new GuildBankItemInfo();
            itemInfo.Slot = packet.ReadUInt8();
            int entry = packet.ReadInt32();
            if (entry > 0)
            {
                itemInfo.Item.ItemID = (uint)entry;
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
                    itemInfo.Flags = packet.ReadUInt32();

                itemInfo.Item.RandomPropertiesID = packet.ReadUInt32();
                if (itemInfo.Item.RandomPropertiesID != 0)
                    itemInfo.Item.RandomPropertiesSeed = packet.ReadUInt32();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                    itemInfo.Count = packet.ReadInt32();
                else
                    itemInfo.Count = packet.ReadUInt8();

                itemInfo.EnchantmentID = packet.ReadInt32();
                itemInfo.Charges = packet.ReadUInt8();

                var enchantments = packet.ReadUInt8();
                for (var j = 0; j < enchantments; j++)
                {
                    byte slot = packet.ReadUInt8();
                    uint enchantId = packet.ReadUInt32();
                    if (enchantId != 0)
                    {
                        uint itemId = GameData.GetGemFromEnchantId(enchantId);
                        if (itemId != 0)
                        {
                            ItemGemData gem = new ItemGemData();
                            gem.Slot = slot;
                            gem.Item.ItemID = itemId;
                            itemInfo.SocketEnchant.Add(gem);
                        }
                    }
                } 
            }
            result.ItemInfo.Add(itemInfo);
        }

        // AC only attaches the tab list on tab 0. 3.4.3 will not paint
        // items unless FullUpdate is set, so any payload with tabs or
        // items is a full replace.
        result.FullUpdate = hasTabs || result.ItemInfo.Count > 0;

        WorldClientLogMessages.GuildBankQueryResults(
            _melLog, _sourceFile, "C<P S", result.Tab, result.TabInfo.Count,
            result.ItemInfo.Count, result.FullUpdate, result.Money);

        SendPacketToClient(result);
    }

    /// <summary>
    /// TrinityCore 4.3.4 GuildBankQueryResults::Write: counts and string lengths as bits, then the
    /// tabs, the money, the items, and the tab the list is for. The server sends this shape both
    /// for a tab's whole content and for the slots a move changed.
    /// </summary>
    private void HandleGuildBankQueryResultsCata(WorldPacket packet)
    {
        const int MaxSlots = 98;        // GUILD_BANK_MAX_SLOTS
        const int MaxSockets = 3;

        GuildBankQueryResults result = new();
        result.FullUpdate = packet.HasBit();
        int itemCount = (int)packet.ReadBits<uint>(20);
        int tabCount = (int)packet.ReadBits<uint>(22);
        if (itemCount > MaxSlots || tabCount > GuildConst.MaxBankTabsCata)
            return; // more than a bank holds: not this layout

        Span<bool> locked = stackalloc bool[itemCount];
        Span<int> sockets = stackalloc int[itemCount];
        for (int i = 0; i < itemCount; i++)
        {
            locked[i] = packet.HasBit();
            sockets[i] = (int)packet.ReadBits<uint>(23);
            if (sockets[i] > MaxSockets)
                return;
        }
        Span<int> iconLengths = stackalloc int[tabCount];
        Span<int> nameLengths = stackalloc int[tabCount];
        for (int i = 0; i < tabCount; i++)
        {
            iconLengths[i] = (int)packet.ReadBits<uint>(9);
            nameLengths[i] = (int)packet.ReadBits<uint>(7);
        }

        for (int i = 0; i < tabCount; i++)
        {
            GuildBankTabInfo tab = new GuildBankTabInfo();
            tab.Icon = packet.ReadString((uint)iconLengths[i]);
            tab.TabIndex = (int)packet.ReadUInt32();
            tab.Name = packet.ReadString((uint)nameLengths[i]);
            result.TabInfo.Add(tab);
        }

        result.Money = packet.ReadUInt64();

        for (int i = 0; i < itemCount; i++)
        {
            GuildBankItemInfo item = new GuildBankItemInfo();
            item.Locked = locked[i];
            for (int socket = 0; socket < sockets[i]; socket++)
            {
                uint index = packet.ReadUInt32();
                uint enchantId = packet.ReadUInt32();
                uint gemItem = enchantId != 0 ? GameData.GetGemFromEnchantId(enchantId) : 0;
                if (gemItem != 0)
                {
                    ItemGemData gem = new ItemGemData();
                    gem.Slot = (byte)index;
                    gem.Item.ItemID = gemItem;
                    item.SocketEnchant.Add(gem);
                }
            }
            item.EnchantmentID = packet.ReadInt32();
            packet.ReadUInt32();                    // ReforgeEnchantmentID: the modern packet has no place for it
            item.OnUseEnchantmentID = packet.ReadInt32();
            item.Count = packet.ReadInt32();
            item.Slot = packet.ReadInt32();
            item.Flags = packet.ReadUInt32();
            item.Item.ItemID = packet.ReadUInt32();
            item.Item.RandomPropertiesID = packet.ReadUInt32();
            item.Charges = packet.ReadInt32();
            item.Item.RandomPropertiesSeed = packet.ReadUInt32();
            result.ItemInfo.Add(item);
        }

        result.Tab = (int)packet.ReadUInt32();
        result.WithdrawalsRemaining = packet.ReadInt32();

        // Only a tab's whole content names the tabs; the list of changed slots has none and must
        // not be taken for a bank without tabs. See HandleGuildBankQueryTab for what the count is for.
        if (tabCount > 0)
            GetSession().GameState.GuildBankPurchasedTabs = tabCount;

        WorldClientLogMessages.GuildBankQueryResults(
            _melLog, _sourceFile, "C<P S", result.Tab, result.TabInfo.Count,
            result.ItemInfo.Count, result.FullUpdate, result.Money);

        SendPacketToClient(result);
    }

    /// <summary>TrinityCore 4.3.4 GuildBankTextQueryResult::Write.</summary>
    [HandlesSmsg(Opcode.SMSG_GUILD_BANK_TEXT_QUERY_RESULT)]
    internal void HandleGuildBankTextQueryResult(WorldPacket packet)
    {
        uint length = packet.ReadBits<uint>(14);
        GuildBankTextQueryResult result = new();
        result.Tab = (int)packet.ReadUInt32();
        result.Text = packet.ReadString(length);
        SendPacketToClient(result);
    }

    /// <summary>
    /// TrinityCore 4.3.4 GuildBankLogQueryResults::Write: per entry which of money, item, count and
    /// other tab it has, with the player's GUID mask among those bits, then the entries.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_GUILD_BANK_LOG_QUERY_RESULTS)]
    internal void HandleGuildBankLogQueryResults(WorldPacket packet)
    {
        bool hasWeeklyBonus = packet.HasBit();
        int count = (int)packet.ReadBits<uint>(23);
        if (count > 255)
            return; // the server keeps 25 entries a tab; anything larger is not this layout

        bool[] masks = new bool[count * 8];
        bool[] hasMoney = new bool[count];
        bool[] hasItem = new bool[count];
        bool[] hasCount = new bool[count];
        bool[] hasOtherTab = new bool[count];
        for (int i = 0; i < count; i++)
        {
            Span<bool> mask = masks.AsSpan(i * 8, 8);
            hasMoney[i] = packet.HasBit();
            MaskedGuid.ReadMaskBits(packet, mask, [4, 1]);
            hasItem[i] = packet.HasBit();
            hasCount[i] = packet.HasBit();
            MaskedGuid.ReadMaskBits(packet, mask, [2, 5, 3, 6, 0]);
            hasOtherTab[i] = packet.HasBit();
            MaskedGuid.ReadMaskBits(packet, mask, [7]);
        }

        GuildBankLogQueryResults result = new();
        Span<byte> guidBytes = stackalloc byte[8];
        for (int i = 0; i < count; i++)
        {
            ReadOnlySpan<bool> mask = masks.AsSpan(i * 8, 8);
            guidBytes.Clear();

            GuildBankLogEntry entry = new GuildBankLogEntry();
            MaskedGuid.ReadBytes(packet, mask, guidBytes, [6, 1, 5]);
            if (hasCount[i])
                entry.Count = packet.ReadInt32();
            entry.EntryType = packet.ReadInt8();
            MaskedGuid.ReadBytes(packet, mask, guidBytes, [2, 4, 0, 7, 3]);
            if (hasItem[i])
                entry.ItemID = packet.ReadInt32();
            entry.TimeOffset = packet.ReadUInt32();
            if (hasMoney[i])
                entry.Money = packet.ReadUInt64();
            if (hasOtherTab[i])
                entry.OtherTab = packet.ReadInt8();

            entry.PlayerGUID = new WowGuid64(MaskedGuid.ToUInt64(guidBytes)).To128(GetSession().GameState);
            result.Entry.Add(entry);
        }

        result.Tab = packet.ReadInt32();
        if (hasWeeklyBonus)
            result.WeeklyBonusMoney = packet.ReadUInt64();
        SendPacketToClient(result);
    }

    [HandlesSmsg(Opcode.MSG_QUERY_GUILD_BANK_TEXT)]
    internal void HandleQueryGuildBankText(WorldPacket packet)
    {
        GuildBankTextQueryResult result = new();
        result.Tab = packet.ReadUInt8();
        result.Text = packet.ReadCString();
        SendPacketToClient(result);
    }

    [HandlesSmsg(Opcode.MSG_GUILD_BANK_LOG_QUERY)]
    internal void HandleGuildBankLongQuery(WorldPacket packet)
    {
        GuildBankLogQueryResults result = new();
        result.Tab = packet.ReadUInt8();
        byte logSize = packet.ReadUInt8();
        for (byte i = 0; i < logSize; i++)
        {
            GuildBankLogEntry logEntry = new GuildBankLogEntry();
            logEntry.EntryType = packet.ReadInt8();
            logEntry.PlayerGUID = packet.ReadGuid().To128(GetSession().GameState);

            // The legacy writer switches on the event type, not the tab index — see
            // Guild::BankEventLogEntry::WritePacket / GuildBankLogQueryResults::Write on
            // 3.3.5a. Keying off the tab happened to agree only because money events are
            // queried with tab 6; a money event in an item tab (or the reverse) desynced
            // the rest of the packet.
            //
            // Count is a uint32 on the wire. Reading it as a single byte left the
            // following TimeOffset three bytes early, splicing Count's high bytes onto
            // the real offset — which is why the guild bank Log tab showed ages like
            // "35 years ago" that jumped around instead of counting up (issue #158).
            switch ((GuildBankEventType)logEntry.EntryType)
            {
                case GuildBankEventType.DepositItem:
                case GuildBankEventType.WithdrawItem:
                    logEntry.ItemID = packet.ReadInt32();
                    logEntry.Count = (int)packet.ReadUInt32();
                    break;
                case GuildBankEventType.MoveItem:
                case GuildBankEventType.MoveItem2:
                    logEntry.ItemID = packet.ReadInt32();
                    logEntry.Count = (int)packet.ReadUInt32();
                    logEntry.OtherTab = packet.ReadInt8();
                    break;
                default:
                    logEntry.Money = packet.ReadUInt32();
                    break;
            }

            logEntry.TimeOffset = packet.ReadUInt32();
            result.Entry.Add(logEntry);
        }
        SendPacketToClient(result);
    }

    [HandlesSmsg(Opcode.MSG_GUILD_BANK_MONEY_WITHDRAWN)]
    internal void HandleGuildBankMoneyWithdrawn(WorldPacket packet)
    {
        GuildBankRemainingWithdrawMoney result = new();
        result.RemainingWithdrawMoney = packet.ReadUInt32();
        SendPacketToClient(result);
    }
    /// <summary>
    /// TrinityCore 4.3.4 <c>Guild::HandleGuildPartyRequest</c>: one bit, then the experience
    /// multiplier before the two counts; the 4.4.2 client has the counts first.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_GUILD_PARTY_STATE)]
    internal void HandleGuildPartyState(WorldPacket packet)
    {
        GuildPartyState state = new();
        state.InGuildParty = packet.HasBit();
        state.GuildXPEarnedMult = packet.ReadFloat();
        state.NumMembers = packet.ReadInt32();
        state.NumRequired = packet.ReadInt32();
        SendPacketToClient(state);
    }
    // 4.3.4 has four kinds of guild challenge: none, dungeon, raid, rated battleground.
    private const int GuildChallengeTypesCata = 4;

    /// <summary>
    /// TrinityCore 4.3.4 <c>GuildChallengeUpdate::Write</c>: five runs of four numbers, in the order
    /// experience, gold at the level cap, how many count, gold, how many are done. The 4.4.2 client
    /// has no experience and reads the other four in another order, six kinds wide.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_GUILD_CHALLENGE_UPDATE)]
    internal void HandleGuildChallengeUpdate(WorldPacket packet)
    {
        GuildChallengeUpdate challenges = new();
        for (int i = 0; i < GuildChallengeTypesCata; i++)
            packet.ReadInt32(); // experience
        for (int i = 0; i < GuildChallengeTypesCata; i++)
            challenges.MaxLevelGold[i] = packet.ReadInt32();
        for (int i = 0; i < GuildChallengeTypesCata; i++)
            challenges.MaxCount[i] = packet.ReadInt32();
        for (int i = 0; i < GuildChallengeTypesCata; i++)
            challenges.Gold[i] = packet.ReadInt32();
        for (int i = 0; i < GuildChallengeTypesCata; i++)
            challenges.CurrentCount[i] = packet.ReadInt32();
        SendPacketToClient(challenges);
    }
    /// <summary>Empty on both sides.</summary>
    [HandlesSmsg(Opcode.SMSG_GUILD_MEMBER_DAILY_RESET)]
    internal void HandleGuildMemberDailyReset(WorldPacket packet)
    {
        SendPacketToClient(new GuildMemberDailyReset());
    }

    // Counts past these are a misread, not a guild.
    private const int MaxGuildAchievements = 4096;
    private const int MaxGuildEventLogEntries = 1024;
    private const int MaxGuildRewards = 1024;

    /// <summary>
    /// TrinityCore 4.3.4 <c>AchievementMgr&lt;Guild&gt;::SendAllAchievementData</c>: 23 bits of count,
    /// then a packed date and an id for each. The owner and the realm of the record are left
    /// empty, as TrinityCore leaves them for a guild's achievements on the later clients.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_ALL_GUILD_ACHIEVEMENTS)]
    internal void HandleAllGuildAchievements(WorldPacket packet)
    {
        int count = (int)packet.ReadBits<uint>(23);
        if (count > MaxGuildAchievements)
            return;

        AllGuildAchievements achievements = new();
        for (int i = 0; i < count; i++)
        {
            EarnedAchievement earned = new();
            earned.Date = packet.ReadPackedTime();
            earned.Id = packet.ReadUInt32();
            earned.Owner = WowGuid128.Empty;
            achievements.Earned.Add(earned);
        }
        SendPacketToClient(achievements);
    }

    /// <summary>
    /// TrinityCore 4.3.4 <c>GuildEventLogQueryResults::Write</c>: 23 bits of count and the masks
    /// of two GUIDs for each entry, then for each entry the GUID bytes with the rank, the age in
    /// seconds and the kind of event among them.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_GUILD_EVENT_LOG_QUERY_RESULTS)]
    internal void HandleGuildEventLogQueryResults(WorldPacket packet)
    {
        int count = (int)packet.ReadBits<uint>(23);
        if (count > MaxGuildEventLogEntries)
            return;

        bool[] masks = new bool[count * 16];
        for (int i = 0; i < count; i++)
        {
            Span<bool> player = masks.AsSpan(i * 16, 8);
            Span<bool> other = masks.AsSpan(i * 16 + 8, 8);
            MaskedGuid.ReadMaskBits(packet, player, [2, 4]);
            MaskedGuid.ReadMaskBits(packet, other, [7, 6]);
            MaskedGuid.ReadMaskBits(packet, player, [3]);
            MaskedGuid.ReadMaskBits(packet, other, [3, 5]);
            MaskedGuid.ReadMaskBits(packet, player, [7, 5, 0]);
            MaskedGuid.ReadMaskBits(packet, other, [4, 2, 0, 1]);
            MaskedGuid.ReadMaskBits(packet, player, [1, 6]);
        }

        GuildEventLogQueryResults log = new();
        Span<byte> playerBytes = stackalloc byte[8];
        Span<byte> otherBytes = stackalloc byte[8];
        for (int i = 0; i < count; i++)
        {
            ReadOnlySpan<bool> player = masks.AsSpan(i * 16, 8);
            ReadOnlySpan<bool> other = masks.AsSpan(i * 16 + 8, 8);
            playerBytes.Clear();
            otherBytes.Clear();

            GuildEventEntry entry = new();
            MaskedGuid.ReadBytes(packet, other, otherBytes, [3, 2, 5]);
            entry.RankID = packet.ReadUInt8();
            MaskedGuid.ReadBytes(packet, other, otherBytes, [4]);
            MaskedGuid.ReadBytes(packet, player, playerBytes, [0, 4]);
            entry.TransactionDate = packet.ReadUInt32();
            MaskedGuid.ReadBytes(packet, player, playerBytes, [7, 3]);
            MaskedGuid.ReadBytes(packet, other, otherBytes, [0, 6, 7]);
            MaskedGuid.ReadBytes(packet, player, playerBytes, [5]);
            entry.TransactionType = packet.ReadUInt8();
            MaskedGuid.ReadBytes(packet, other, otherBytes, [1]);
            MaskedGuid.ReadBytes(packet, player, playerBytes, [2, 6, 1]);

            entry.PlayerGUID = new WowGuid64(MaskedGuid.ToUInt64(playerBytes)).To128(GetSession().GameState);
            entry.OtherGUID = new WowGuid64(MaskedGuid.ToUInt64(otherBytes)).To128(GetSession().GameState);
            log.Entries.Add(entry);
        }
        SendPacketToClient(log);
    }

    /// <summary>
    /// TrinityCore 4.3.4 <c>GuildRewardList::Write</c>: 21 bits of count, six numbers an item, the
    /// time of the list last. A 4.3.4 item asks for one achievement or none and for no guild
    /// level; its race mask is 32 bits wide.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_GUILD_REWARD_LIST)]
    internal void HandleGuildRewardList(WorldPacket packet)
    {
        int count = (int)packet.ReadBits<uint>(21);
        if (count > MaxGuildRewards)
            return;

        GuildRewardList rewards = new();
        for (int i = 0; i < count; i++)
        {
            GuildRewardItem item = new();
            item.MinGuildRep = packet.ReadInt32();
            item.RaceMask = packet.ReadUInt32();
            item.ItemID = packet.ReadUInt32();
            item.Cost = packet.ReadUInt64();
            item.Unk4 = packet.ReadUInt32();
            uint achievementId = packet.ReadUInt32();
            if (achievementId != 0)
                item.AchievementsRequired.Add(achievementId);
            rewards.RewardItems.Add(item);
        }
        rewards.Version = packet.ReadInt32();
        SendPacketToClient(rewards);
    }

    // Counts past these are a misread: a 4.3.4 server keeps 250 news entries by default, and the
    // members named in one are a raid at most.
    private const int MaxGuildNewsEvents = 4096;
    private const int MaxGuildNewsMembers = 100;

    // News kinds, the same numbers on both sides (TrinityCore 4.3.4 GuildNews, the client's NEWS_*
    // constants). The value of looted, crafted and purchased, 3 to 5, is an item.
    private const int GuildNewsItemLooted = 3;
    private const int GuildNewsItemPurchased = 5;
    private const int GuildNewsLevelUp = 6;

    /// <summary>
    /// TrinityCore 4.3.4 <c>GuildKnownRecipes::Write</c>: 16 bits of count, then per profession its
    /// skill line and 300 bytes of recipe bits. The 4.4.2 client takes the count as a uint32.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_GUILD_KNOWN_RECIPES)]
    internal void HandleGuildKnownRecipes(WorldPacket packet)
    {
        uint count = packet.ReadBits<uint>(16);
        GuildKnownRecipes recipes = new GuildKnownRecipes();
        for (uint i = 0; i < count && packet.CanRead(); i++)
        {
            int skillLineId = packet.ReadInt32();
            recipes.Recipes.Add((skillLineId, packet.ReadBytes(GuildKnownRecipes.RecipeBytes)));
        }
        SendPacketToClient(recipes);
    }

    /// <summary>
    /// TrinityCore 4.3.4 <c>GuildNews::Write</c> (its SMSG_GUILD_NEWS): 21 bits of count; for each
    /// entry the number of members who were there with the masks of their GUIDs, inside the mask
    /// of the member the entry is about; then for each entry the GUID bytes with the flags, two
    /// values, the id, the kind and the packed date among them. The whole list answers
    /// CMSG_GUILD_QUERY_NEWS; one entry comes when something happens or an entry is pinned.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_GUILD_NEWS_UPDATE)]
    internal void HandleGuildNews(WorldPacket packet)
    {
        int count = (int)packet.ReadBits<uint>(21);
        if (count > MaxGuildNewsEvents)
            return;

        bool[] masks = new bool[count * 8];
        bool[][] memberMasks = new bool[count][];
        for (int i = 0; i < count; i++)
        {
            int members = (int)packet.ReadBits<uint>(26);
            if (members > MaxGuildNewsMembers)
                return;

            Span<bool> mask = masks.AsSpan(i * 8, 8);
            MaskedGuid.ReadMaskBits(packet, mask, [7]);
            memberMasks[i] = new bool[members * 8];
            for (int member = 0; member < members; member++)
                MaskedGuid.ReadMaskBits(packet, memberMasks[i].AsSpan(member * 8, 8), [7, 1, 5, 3, 4, 6, 0, 2]);
            MaskedGuid.ReadMaskBits(packet, mask, [0, 6, 5, 4, 3, 1, 2]);
        }

        GuildNews news = new();
        Span<byte> guidBytes = stackalloc byte[8];
        for (int i = 0; i < count; i++)
        {
            GuildNewsEvent entry = new();
            for (int member = 0; member < memberMasks[i].Length / 8; member++)
            {
                guidBytes.Clear();
                MaskedGuid.ReadBytes(packet, memberMasks[i].AsSpan(member * 8, 8), guidBytes, [0, 1, 4, 7, 5, 6, 3, 2]);
                entry.MemberList.Add(new WowGuid64(MaskedGuid.ToUInt64(guidBytes)).To128(GetSession().GameState));
            }

            ReadOnlySpan<bool> mask = masks.AsSpan(i * 8, 8);
            guidBytes.Clear();
            MaskedGuid.ReadBytes(packet, mask, guidBytes, [5]);
            entry.Flags = packet.ReadInt32();
            entry.Data[0] = packet.ReadInt32();
            entry.Data[1] = packet.ReadInt32();
            MaskedGuid.ReadBytes(packet, mask, guidBytes, [7, 6, 2, 3, 0, 4, 1]);
            entry.Id = packet.ReadInt32() + Server.Systems.GuildSystem.CataNewsIdShift;
            entry.Type = packet.ReadInt32();
            entry.CompletedDate = packet.ReadPackedTime();

            ulong memberGuid = MaskedGuid.ToUInt64(guidBytes);
            if (memberGuid != 0)
                entry.MemberGuid = new WowGuid64(memberGuid).To128(GetSession().GameState);

            // The 4.4.2 client has no guild levels and makes no entry of this kind: asked for it,
            // its news list answers with nothing at that place.
            if (entry.Type == GuildNewsLevelUp)
                continue;

            // The client takes the entry's id from the value and its link from an item record of
            // its own, which a 4.3.4 entry does not have. With the value alone the entry has no
            // link, with the record alone no id.
            if (entry.Type is >= GuildNewsItemLooted and <= GuildNewsItemPurchased)
                entry.Item = new ItemInstance { ItemID = (uint)entry.Data[0] };
            news.Events.Add(entry);
        }

        // Nothing left of what the server sent: no news to the client.
        if (count > 0 && news.Events.Count == 0)
            return;
        SendPacketToClient(news);
    }
}
