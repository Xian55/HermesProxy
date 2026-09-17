using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

// Party and raid CMSG codecs.
//
// This is the densest version-variance in the inbound set: nine of these eighteen packets need a
// V3_4_3-specific layout, more than the whole chat family. V3_4_3 moved the optional-field bits to
// the front — PartyIndex became a bit plus an optional byte instead of a mandatory one — so the two
// layouts disagree from the first byte, not just in the tail. Each is now a ranged pair, decided
// once when the table is built.
//
// The hard-won detail is ReadyCheckResponseClient, and its comment moves with it: reading
// PartyIndex first consumed the bit byte and then took the MSB of the always-zero index byte as
// IsReady, so every ready check answered "not ready". Splitting the layouts is what makes that
// unrepresentable rather than fixed-in-place.
//
// RandomRollClient and MinimapPingClient were missed by that first sweep because their index byte
// is not the leading field, so nothing crashed and nothing read past the end — the values were
// simply one byte out. Where the mandatory index byte *is* first (UpdateRaidTarget, SetPartyLeader,
// RequestPartyMemberStats) it happens to swallow the bit byte and land every later field at the
// right offset, which is why those stay single codecs: only their PartyIndex is junk, and no
// handler forwards it.

// ---- invite ----

public static class PartyInviteClientCodec
{
    public static void Read(ref SpanPacketReader r, out PartyInviteClient packet)
    {
        byte partyIndex = r.ReadUInt8();

        uint targetNameLen = r.ReadBits<uint>(9);
        uint targetRealmLen = r.ReadBits<uint>(9);

        uint virtualRealmAddress = r.ReadUInt32();
        WowGuid128 targetGuid = r.ReadPackedGuid128();

        string targetName = r.ReadString(targetNameLen);
        string targetRealm = r.ReadString(targetRealmLen);
        packet = new PartyInviteClient(partyIndex, virtualRealmAddress, targetGuid, targetName, targetRealm);
    }
}

[PacketCodec(typeof(PartyInviteResponse), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class PartyInviteResponseCodecWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out PartyInviteResponse packet)
    {
        // 3 header bits first, then the optional bytes. /reload emits this packet with all
        // flags = 0 (size = 1) as a state flush.
        bool hasPartyIndex = r.HasBit();
        bool accept = r.HasBit();
        bool hasRolesDesired = r.HasBit();

        byte partyIndex = 0;
        if (hasPartyIndex)
            partyIndex = r.ReadUInt8();

        uint? rolesDesired = null;
        if (hasRolesDesired)
            rolesDesired = r.ReadUInt8();

        packet = new PartyInviteResponse(partyIndex, accept, rolesDesired);
    }
}

[PacketCodec(typeof(PartyInviteResponse), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class PartyInviteResponseCodecPreWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out PartyInviteResponse packet)
    {
        byte partyIndex = r.ReadUInt8();
        bool accept = r.HasBit();

        // Note the width: a uint32 here where V3_4_3 sends a byte.
        uint? rolesDesired = null;
        if (r.HasBit())
            rolesDesired = r.ReadUInt32();

        packet = new PartyInviteResponse(partyIndex, accept, rolesDesired);
    }
}

public static class LeaveGroupCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out LeaveGroup packet)
        => packet = new LeaveGroup(r.ReadInt8());
}

[PacketCodec(typeof(PartyUninvite), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class PartyUninviteCodecWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out PartyUninvite packet)
    {
        // Bits first, then the GUID, then the optional PartyIndex byte.
        bool hasPartyIndex = r.HasBit();
        byte reasonLen = r.ReadBits<byte>(8);
        WowGuid128 targetGuid = r.ReadPackedGuid128();

        byte partyIndex = 0;
        if (hasPartyIndex)
            partyIndex = r.ReadUInt8();

        string reason = r.ReadString(reasonLen);
        packet = new PartyUninvite(partyIndex, targetGuid, reason);
    }
}

[PacketCodec(typeof(PartyUninvite), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class PartyUninviteCodecPreWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out PartyUninvite packet)
    {
        byte partyIndex = r.ReadUInt8();
        WowGuid128 targetGuid = r.ReadPackedGuid128();
        byte legacyReasonLen = r.ReadBits<byte>(8);
        string reason = r.ReadString(legacyReasonLen);
        packet = new PartyUninvite(partyIndex, targetGuid, reason);
    }
}

// ---- leadership ----

[PacketCodec(typeof(SetAssistantLeader), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class SetAssistantLeaderCodecWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out SetAssistantLeader packet)
    {
        // 2 header bits, then the GUID, then the optional PartyIndex byte.
        // Mirrors CypherCore WorldPackets::Party::SetAssistantLeader::Read.
        bool hasPartyIndex = r.HasBit();
        bool apply = r.HasBit();
        WowGuid128 targetGuid = r.ReadPackedGuid128();

        byte partyIndex = 0;
        if (hasPartyIndex)
            partyIndex = r.ReadUInt8();

        packet = new SetAssistantLeader(partyIndex, targetGuid, apply);
    }
}

[PacketCodec(typeof(SetAssistantLeader), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class SetAssistantLeaderCodecPreWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out SetAssistantLeader packet)
    {
        byte partyIndex = r.ReadUInt8();
        WowGuid128 targetGuid = r.ReadPackedGuid128();
        bool apply = r.HasBit();
        packet = new SetAssistantLeader(partyIndex, targetGuid, apply);
    }
}

[PacketCodec(typeof(SetEveryoneIsAssistant), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class SetEveryoneIsAssistantCodecWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out SetEveryoneIsAssistant packet)
    {
        // 2 header bits, then the optional PartyIndex byte.
        // Mirrors CypherCore WorldPackets::Party::SetEveryoneIsAssistant::Read.
        bool hasPartyIndex = r.HasBit();
        bool apply = r.HasBit();

        byte partyIndex = 0;
        if (hasPartyIndex)
            partyIndex = r.ReadUInt8();

        packet = new SetEveryoneIsAssistant(partyIndex, apply);
    }
}

[PacketCodec(typeof(SetEveryoneIsAssistant), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class SetEveryoneIsAssistantCodecPreWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out SetEveryoneIsAssistant packet)
    {
        byte partyIndex = r.ReadUInt8();
        bool apply = r.HasBit();
        packet = new SetEveryoneIsAssistant(partyIndex, apply);
    }
}

public static class SetPartyLeaderCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SetPartyLeader packet)
    {
        sbyte partyIndex = r.ReadInt8();
        WowGuid128 targetGuid = r.ReadPackedGuid128();
        packet = new SetPartyLeader(partyIndex, targetGuid);
    }
}

public static class ConvertRaidCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ConvertRaid packet)
        => packet = new ConvertRaid(r.HasBit());
}

// ---- ready check ----

[PacketCodec(typeof(DoReadyCheck), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class DoReadyCheckCodecWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out DoReadyCheck packet)
    {
        // A HasPartyIndex bit first, then the optional byte.
        sbyte partyIndex = 0;
        if (r.HasBit())
            partyIndex = r.ReadInt8();
        packet = new DoReadyCheck(partyIndex);
    }
}

[PacketCodec(typeof(DoReadyCheck), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class DoReadyCheckCodecPreWotLKClassic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out DoReadyCheck packet)
        => packet = new DoReadyCheck(r.ReadInt8());
}

[PacketCodec(typeof(ReadyCheckResponseClient), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class ReadyCheckResponseClientCodecWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out ReadyCheckResponseClient packet)
    {
        // A HasPartyIndex bit, then IsReady, then the optional PartyIndex byte — the same
        // bits-first shape as PartyInviteResponse. Reading PartyIndex first consumed the bit byte
        // and then took the MSB of the (always zero) index byte as IsReady, so every answer
        // reached the group as "not ready". Observed bytes: Ready = C0 00, Not Ready = 80 00.
        // WPP's V3_4_0 parser orders these bits the other way round, but it is registered at
        // V3_4_4_59817 and does not hold for 54261.
        bool hasPartyIndex = r.HasBit();
        bool isReady = r.HasBit();

        byte partyIndex = 0;
        if (hasPartyIndex)
            partyIndex = r.ReadUInt8();

        packet = new ReadyCheckResponseClient(partyIndex, isReady);
    }
}

[PacketCodec(typeof(ReadyCheckResponseClient), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class ReadyCheckResponseClientCodecPreWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out ReadyCheckResponseClient packet)
    {
        byte partyIndex = r.ReadUInt8();
        bool isReady = r.HasBit();
        packet = new ReadyCheckResponseClient(partyIndex, isReady);
    }
}

// ---- raid management ----

public static class UpdateRaidTargetCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out UpdateRaidTarget packet)
    {
        sbyte partyIndex = r.ReadInt8();
        WowGuid128 target = r.ReadPackedGuid128();
        sbyte symbol = r.ReadInt8();
        packet = new UpdateRaidTarget(partyIndex, target, symbol);
    }
}

public static class SummonResponseCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SummonResponse packet)
    {
        WowGuid128 summonerGuid = r.ReadPackedGuid128();
        bool accept = r.HasBit();
        packet = new SummonResponse(summonerGuid, accept);
    }
}

[PacketCodec(typeof(MinimapPingClient), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class MinimapPingClientCodecWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out MinimapPingClient packet)
    {
        // A HasPartyIndex bit first, then the coordinates, then the optional index byte. Reading
        // the Vector2 straight away took the bit byte as the low byte of PositionX, so every ping
        // landed at a garbage spot on the minimap.
        bool hasPartyIndex = r.HasBit();
        Vector2 position = r.ReadVector2();

        sbyte partyIndex = 0;
        if (hasPartyIndex)
            partyIndex = r.ReadInt8();

        packet = new MinimapPingClient(position, partyIndex);
    }
}

[PacketCodec(typeof(MinimapPingClient), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class MinimapPingClientCodecPreWotLKClassic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out MinimapPingClient packet)
    {
        Vector2 position = r.ReadVector2();
        sbyte partyIndex = r.ReadInt8();
        packet = new MinimapPingClient(position, partyIndex);
    }
}

[PacketCodec(typeof(RandomRollClient), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class RandomRollClientCodecWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out RandomRollClient packet)
    {
        // A HasPartyIndex bit first, then Min and Max, then the optional index byte. Reading Min
        // straight away took the bit byte as its low byte and shifted the rest one byte along, so
        // `/roll 100` reached the legacy server as a roll of 256-25600 and came back as a result
        // in that range (issue #318). Captured bytes for `/roll 100`: 00 01 00 00 00 64 00 00 00.
        bool hasPartyIndex = r.HasBit();
        int min = r.ReadInt32();
        int max = r.ReadInt32();

        byte partyIndex = 0;
        if (hasPartyIndex)
            partyIndex = r.ReadUInt8();

        packet = new RandomRollClient(min, max, partyIndex);
    }
}

[PacketCodec(typeof(RandomRollClient), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class RandomRollClientCodecPreWotLKClassic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out RandomRollClient packet)
    {
        int min = r.ReadInt32();
        int max = r.ReadInt32();
        byte partyIndex = r.ReadUInt8();
        packet = new RandomRollClient(min, max, partyIndex);
    }
}

public static class RequestPartyMemberStatsCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out RequestPartyMemberStats packet)
    {
        byte partyIndex = r.ReadUInt8();
        WowGuid128 targetGuid = r.ReadPackedGuid128();
        packet = new RequestPartyMemberStats(partyIndex, targetGuid);
    }
}

[PacketCodec(typeof(ChangeSubGroup), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class ChangeSubGroupCodecWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out ChangeSubGroup packet)
    {
        // The one party packet whose HasPartyIndex bit trails the fixed fields instead of leading
        // them. The pre-WotLK reader takes NewSubGroup as PartyIndex and the bit byte as
        // NewSubGroup, which is zero whenever no PartyIndex follows, so every drag lands in group 1.
        WowGuid128 targetGuid = r.ReadPackedGuid128();
        byte newSubGroup = r.ReadUInt8();

        sbyte partyIndex = 0;
        if (r.HasBit())
            partyIndex = r.ReadInt8();

        packet = new ChangeSubGroup(targetGuid, partyIndex, newSubGroup);
    }
}

[PacketCodec(typeof(ChangeSubGroup), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class ChangeSubGroupCodecPreWotLKClassic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out ChangeSubGroup packet)
    {
        WowGuid128 targetGuid = r.ReadPackedGuid128();
        sbyte partyIndex = r.ReadInt8();
        byte newSubGroup = r.ReadUInt8();
        packet = new ChangeSubGroup(targetGuid, partyIndex, newSubGroup);
    }
}

[PacketCodec(typeof(SwapSubGroups), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class SwapSubGroupsCodecWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out SwapSubGroups packet)
    {
        bool hasPartyIndex = r.HasBit();
        WowGuid128 firstTarget = r.ReadPackedGuid128();
        WowGuid128 secondTarget = r.ReadPackedGuid128();

        sbyte partyIndex = 0;
        if (hasPartyIndex)
            partyIndex = r.ReadInt8();

        packet = new SwapSubGroups(partyIndex, firstTarget, secondTarget);
    }
}

[PacketCodec(typeof(SwapSubGroups), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class SwapSubGroupsCodecPreWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out SwapSubGroups packet)
    {
        sbyte partyIndex = r.ReadInt8();
        WowGuid128 firstTarget = r.ReadPackedGuid128();
        WowGuid128 secondTarget = r.ReadPackedGuid128();
        packet = new SwapSubGroups(partyIndex, firstTarget, secondTarget);
    }
}

// ---- roles ----

[PacketCodec(typeof(SetRole), AddedIn = ClientVersionBuild.V3_4_3_54261)]
public static class SetRoleCodecWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out SetRole packet)
    {
        bool hasPartyIndex = r.HasBit();
        WowGuid128 changedUnit = r.ReadPackedGuid128();
        byte role = r.ReadUInt8();

        byte partyIndex = 0;
        if (hasPartyIndex)
            partyIndex = r.ReadUInt8();

        packet = new SetRole(partyIndex, changedUnit, role);
    }
}

[PacketCodec(typeof(SetRole), RemovedIn = ClientVersionBuild.V3_4_3_54261)]
public static class SetRoleCodecPreWotLKClassic
{
    public static void Read(ref SpanPacketReader r, out SetRole packet)
    {
        // Role is a full int32 here, narrowed to the byte the rest of the proxy carries.
        byte partyIndex = (byte)r.ReadInt8();
        WowGuid128 changedUnit = r.ReadPackedGuid128();
        byte role = (byte)r.ReadInt32();
        packet = new SetRole(partyIndex, changedUnit, role);
    }
}
