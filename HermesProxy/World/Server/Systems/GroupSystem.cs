using System;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's party and raid CMSGs.
/// </summary>
/// <remarks>
/// <para>
/// Bodies were moved from <c>World/Server/PacketHandlers/GroupHandler.cs</c>, not retyped;
/// <c>verify-handler-port.py</c> diffs each one against the original.
/// </para>
/// <para>
/// Five methods were renamed on the way across, and only renamed — the bodies are untouched. Two
/// pairs were overloads distinguished solely by packet type, with names inherited from whichever
/// handler was copied to make them: a party-invite handler called <c>HandleUpdateRaidTarget</c> and
/// a random-roll one called <c>HandleMinimapPing</c>. Two more were missing a letter
/// (<c>HandlReadyCheck</c>). verify-handler-port maps old name to new, so the diff still holds.
/// </para>
/// <para>
/// Most of the version variance in this domain lives in the codecs rather than here: V3_4_3 moved
/// the optional-field bits to the front of seven of these packets, so the layouts disagree from the
/// first byte. See <c>GroupCodecs.cs</c>.
/// </para>
/// </remarks>
public static class GroupSystem
{
    [HandlesCmsg(Opcode.CMSG_PARTY_INVITE)]
    public static void HandlePartyInvite(in PartyInviteClient invite, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_PARTY_INVITE);
        packet.WriteCString(invite.TargetName);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.WriteUInt32(0);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_PARTY_INVITE_RESPONSE)]
    public static void HandlePartyInviteResponse(in PartyInviteResponse invite, in SessionContext ctx)
    {
        if (invite.Accept)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GROUP_ACCEPT);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.WriteUInt32(0);
            ctx.SendPacketToServer(packet);
        }
        else
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GROUP_DECLINE);
            ctx.SendPacketToServer(packet);
        }
    }

    [HandlesCmsg(Opcode.CMSG_LEAVE_GROUP)]
    public static void HandleLeaveGroup(in LeaveGroup leave, in SessionContext ctx)
    {
        ctx.GetSession().GameState.WeWantToLeaveGroup = true;
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GROUP_DISBAND);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_PARTY_UNINVITE)]
    public static void HandlePartyUninvite(in PartyUninvite kick, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GROUP_UNINVITE_GUID);
        packet.WriteGuid(kick.TargetGUID.To64());
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.WriteCString(kick.Reason);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SET_ASSISTANT_LEADER)]
    public static void HandleSetAssistantLeader(in SetAssistantLeader assist, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_ASSISTANT_LEADER);
        packet.WriteGuid(assist.TargetGUID.To64());
        packet.WriteBool(assist.Apply);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SET_EVERYONE_IS_ASSISTANT)]
    public static void HandleSetEveryoneIsAssistant(in SetEveryoneIsAssistant assist, in SessionContext ctx)
    {
        var groupMembers = ctx.GetSession().GameState.GetCurrentGroup()!.PlayerList;
        foreach (var member in groupMembers)
        {
            if (member.GUID == ctx.GetSession().GameState.CurrentPlayerGuid)
                continue;

            WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_ASSISTANT_LEADER);
            packet.WriteGuid(member.GUID.To64());
            packet.WriteBool(assist.Apply);
            ctx.SendPacketToServer(packet);
        }
    }

    [HandlesCmsg(Opcode.CMSG_SET_PARTY_LEADER)]
    public static void HandleSetPartyLeader(in SetPartyLeader leader, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_PARTY_LEADER);
        packet.WriteGuid(leader.TargetGUID.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_CONVERT_RAID)]
    public static void HandleConvertRaid(in ConvertRaid raid, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_CONVERT_RAID);
        // wotlk_classic TC reads a single bit: true = ConvertToRaid, false = ConvertToGroup.
        // Without this bit the server reads past EOF, defaults to "raid", and "Convert to Party" silently no-ops.
        //
        // By expansion and patch, not by build number: raw builds are only ordered within one
        // branch, and V3_4_3_54261 is a Classic build while a 3.3.5a server is a Retail one. The
        // comparison happened to give the right answer for both, but it asserts in Debug builds —
        // which is how it was found, when converting a party to a raid killed the proxy.
        if (LegacyVersion.ExpansionVersion == 3 && LegacyVersion.MajorVersion >= 4)
        {
            packet.WriteBit(raid.Raid);
            packet.FlushBits();
        }
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_DO_READY_CHECK)]
    public static void HandleReadyCheck(in DoReadyCheck raid, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_RAID_READY_CHECK);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_READY_CHECK_RESPONSE)]
    public static void HandleReadyCheckResponse(in ReadyCheckResponseClient raid, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_RAID_READY_CHECK);
        packet.WriteBool(raid.IsReady);
        ctx.SendPacketToServer(packet);

        // The legacy server broadcasts MSG_RAID_READY_CHECK_CONFIRM to the leader and
        // assistants only, so a plain member never sees its own answer come back. Echo it
        // locally under the real party GUID - a placeholder GUID does not match the party
        // the client is tracking, so the echo was discarded.
        ReadyCheckResponse ready = new ReadyCheckResponse();
        ready.Player = ctx.GetSession().GameState.CurrentPlayerGuid;
        ready.IsReady = raid.IsReady;
        ready.PartyGUID = ctx.GetSession().GameState.GetCurrentGroupGuid();
        ctx.SendPacket(ready);
    }

    [HandlesCmsg(Opcode.CMSG_UPDATE_RAID_TARGET)]
    public static void HandleUpdateRaidTarget(in UpdateRaidTarget update, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_RAID_TARGET_UPDATE);
        packet.WriteInt8(update.Symbol);
        packet.WriteGuid(update.Target.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SUMMON_RESPONSE)]
    public static void HandleSummonResponse(in SummonResponse update, in SessionContext ctx)
    {
        if (update.Accept || LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_SUMMON_RESPONSE);
            packet.WriteGuid(update.SummonerGUID.To64());
            packet.WriteBool(update.Accept);
            ctx.SendPacketToServer(packet);
        }
    }

    [HandlesCmsg(Opcode.CMSG_MINIMAP_PING)]
    public static void HandleMinimapPing(in MinimapPingClient ping, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_MINIMAP_PING);
        packet.WriteVector2(ping.Position);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_RANDOM_ROLL)]
    public static void HandleRandomRoll(in RandomRollClient roll, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_RANDOM_ROLL);
        packet.WriteInt32(roll.Min);
        packet.WriteInt32(roll.Max);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_REQUEST_PARTY_MEMBER_STATS)]
    public static void HandleRequestPartyMemberStats(in RequestPartyMemberStats request, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_REQUEST_PARTY_MEMBER_STATS);
        packet.WriteGuid(request.TargetGUID.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GROUP_CHANGE_SUB_GROUP)]
    public static void HandleGroupChangeSubGroup(in ChangeSubGroup group, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GROUP_CHANGE_SUB_GROUP);
        packet.WriteCString(ctx.GetSession().GameState.GetPlayerName(group.TargetGUID));
        packet.WriteUInt8(group.NewSubGroup);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_GROUP_SWAP_SUB_GROUP)]
    public static void HandleGroupSwapSubGroup(in SwapSubGroups group, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_GROUP_SWAP_SUB_GROUP);
        packet.WriteCString(ctx.GetSession().GameState.GetPlayerName(group.FirstTarget));
        packet.WriteCString(ctx.GetSession().GameState.GetPlayerName(group.SecondTarget));
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SET_ROLE)]
    public static void HandleSetRole(in SetRole packet, in SessionContext ctx)
    {
        // AC has no CMSG_GROUP_SET_ROLES. Native 3.4.3 replies with
        // SMSG_ROLE_CHANGED_INFORM and stores the role on the group.
        byte newRole = packet.Role;
        byte oldRole = 0;
        var assigned = ctx.GetSession().GameState.GroupAssignedRoles;
        if (assigned.TryGetValue(packet.ChangedUnit, out var stored))
            oldRole = stored;

        PartyUpdate? group = FindGroupForRoleAssign(in ctx, packet.PartyIndex, packet.ChangedUnit);

        if (group != null)
        {
            for (int i = 0; i < group.PlayerList.Count; i++)
            {
                var member = group.PlayerList[i];
                if (member.GUID != packet.ChangedUnit)
                    continue;
                if (!assigned.ContainsKey(packet.ChangedUnit))
                    oldRole = member.RolesAssigned;
                member.RolesAssigned = newRole;
                group.PlayerList[i] = member;
                break;
            }
        }

        if (oldRole == newRole)
            return;

        assigned[packet.ChangedUnit] = newRole;

        if (packet.ChangedUnit == ctx.GetSession().GameState.CurrentPlayerGuid)
        {
            ctx.GetSession().GameState.LfgRequestedRoles = newRole;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            {
                WorldPacket legacy = new WorldPacket(Opcode.CMSG_LFG_SET_ROLES);
                legacy.WriteUInt8(newRole);
                ctx.SendPacketToServer(legacy);
            }
        }

        RoleChangedInform inform = new();
        inform.PartyIndex = packet.PartyIndex;
        inform.From = ctx.GetSession().GameState.CurrentPlayerGuid;
        inform.ChangedUnit = packet.ChangedUnit;
        inform.OldRole = oldRole;
        inform.NewRole = packet.Role;
        ctx.SendPacket(inform);

        // INFORM is the chat line. The Set Role radio and UnitGroupRolesAssigned
        // come from SMSG_PARTY_UPDATE.RolesAssigned.
        if (group != null)
        {
            for (int i = 0; i < group.PlayerList.Count; i++)
            {
                var member = group.PlayerList[i];
                if (assigned.TryGetValue(member.GUID, out var role))
                {
                    member.RolesAssigned = role;
                    group.PlayerList[i] = member;
                }
            }

            var update = group.CloneUnwritten();
            update.SequenceNum = ctx.GetSession().GameState.GroupUpdateCounter++;
            if (update.PartyIndex < ctx.GetSession().GameState.CurrentGroups.Length)
                ctx.GetSession().GameState.CurrentGroups[update.PartyIndex] = update;
            ctx.SendPacket(update);
        }
    }

    static PartyUpdate? FindGroupForRoleAssign(in SessionContext ctx, byte partyIndex, WowGuid128 changedUnit)
    {
        var groups = ctx.GetSession().GameState.CurrentGroups;
        if (partyIndex < groups.Length && GroupContains(groups[partyIndex], changedUnit))
            return groups[partyIndex];

        foreach (var candidate in groups)
        {
            if (GroupContains(candidate, changedUnit))
                return candidate;
        }

        if (ctx.GetSession().GameState.LastAnnouncedPartyIndex < groups.Length)
        {
            var last = groups[ctx.GetSession().GameState.LastAnnouncedPartyIndex];
            if (last != null)
                return last;
        }

        return ctx.GetSession().GameState.GetCurrentGroup();
    }

    static bool GroupContains(PartyUpdate? group, WowGuid128 guid)
    {
        if (group == null)
            return false;
        foreach (var member in group.PlayerList)
        {
            if (member.GUID == guid)
                return true;
        }
        return false;
    }
}
