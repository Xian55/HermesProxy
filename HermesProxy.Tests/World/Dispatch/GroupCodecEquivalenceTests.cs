using System;
using Framework.IO;
using HermesProxy;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;
using Frozen = HermesProxy.Tests.World.Dispatch.Reference.FrozenPackets;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// Equivalence for the party and raid codecs against the frozen <c>Read()</c> bodies they replaced.
/// </summary>
/// <remarks>
/// <para>
/// Seven of these eighteen packets carried a build branch inside one <c>Read</c> — more than the
/// whole chat family — so most of this file is ranged pairs. The pre-WotLK side is proven against
/// the oracle; the V3_4_3 side against explicit byte expectations, because the oracle's branch is
/// chosen by <c>ModernVersion.Build</c>, which is fixed for the process and is V1_14 here.
/// </para>
/// <para>
/// The two layouts disagree from the <em>first byte</em>, not merely in the tail: V3_4_3 moved the
/// optional-field flags to the front, so <c>PartyIndex</c> became a bit plus an optional byte where
/// it used to be mandatory. A reader that takes the wrong branch does not read a wrong value, it
/// reads a different packet.
/// </para>
/// </remarks>
public class GroupCodecEquivalenceTests
{
    static GroupCodecEquivalenceTests()
    {
        if (VersionBootstrap.ModernBuild == ClientVersionBuild.Zero)
            VersionBootstrap.ModernBuild = ClientVersionBuild.V1_14_2_42597;
        if (VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            VersionBootstrap.LegacyBuild = ClientVersionBuild.V3_3_5a_12340;
    }

    private static (WorldPacket Oracle, byte[] Framed) Build(Action<WorldPacket> write)
    {
        using var w = new WorldPacket(1u);
        write(w);
        byte[] payload = w.GetData();
        byte[] framed = new byte[payload.Length + 2];
        payload.CopyTo(framed, 2);
        return (new WorldPacket(framed), framed);
    }

    private static SpanPacketReader ReaderOver(byte[] framed)
        => new(new WorldPacket(framed).GetRemainingSpan());

    private static readonly WowGuid128 Guid = new(0xDEADBEEFCAFEUL, 0x0123456789ABCDEFUL);
    private static readonly WowGuid128 Guid2 = new(0x1122334455667788UL, 0x99AABBCCDDEEFF00UL);

    [Fact]
    public void OracleBranchIsThePreWotLKClassicOne()
        => Assert.NotEqual(ClientVersionBuild.V3_4_3_54261, ModernVersion.Build);

    // ---- no version variance ----

    [Theory]
    [InlineData("Thrall", "Bloodhoof")]
    [InlineData("A", "")]
    [InlineData("", "")]
    public void PartyInviteClient_Matches(string name, string realm)
    {
        var (o, f) = Build(w =>
        {
            w.WriteUInt8(1);
            w.WriteBits((uint)name.Length, 9);
            w.WriteBits((uint)realm.Length, 9);
            w.WriteUInt32(4711);
            w.WritePackedGuid128(Guid);
            w.WriteString(name);
            w.WriteString(realm);
        });

        var e = new Frozen.PartyInviteClient(); e.Read(o);
        var r = ReaderOver(f); PartyInviteClientCodec.Read(ref r, out var a);
        Assert.Equal(e.PartyIndex, a.PartyIndex);
        Assert.Equal(e.VirtualRealmAddress, a.VirtualRealmAddress);
        Assert.Equal(e.TargetGUID, a.TargetGUID);
        Assert.Equal(e.TargetName, a.TargetName);
        Assert.Equal(e.TargetRealm, a.TargetRealm);
        Assert.Equal(name, a.TargetName);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData((sbyte)0)]
    [InlineData((sbyte)-1)]
    public void LeaveGroup_Matches(sbyte partyIndex)
    {
        var (o, f) = Build(w => w.WriteInt8(partyIndex));
        var e = new Frozen.LeaveGroup(); e.Read(o);
        var r = ReaderOver(f); LeaveGroupCodec.Read(ref r, out var a);
        Assert.Equal(e.PartyIndex, a.PartyIndex);
        Assert.Equal(partyIndex, a.PartyIndex);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void SetPartyLeader_Matches()
    {
        var (o, f) = Build(w => { w.WriteInt8(-1); w.WritePackedGuid128(Guid); });
        var e = new Frozen.SetPartyLeader(); e.Read(o);
        var r = ReaderOver(f); SetPartyLeaderCodec.Read(ref r, out var a);
        Assert.Equal(e.PartyIndex, a.PartyIndex);
        Assert.Equal(e.TargetGUID, a.TargetGUID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// A single bit, and the handler forwards it verbatim. Without it the legacy server reads past
    /// EOF, defaults to "raid", and Convert to Party silently does nothing.
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ConvertRaid_Matches(bool raid)
    {
        var (o, f) = Build(w => w.WriteBit(raid));
        var e = new Frozen.ConvertRaid(); e.Read(o);
        var r = ReaderOver(f); ConvertRaidCodec.Read(ref r, out var a);
        Assert.Equal(e.Raid, a.Raid);
        Assert.Equal(raid, a.Raid);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData((sbyte)0)]
    [InlineData((sbyte)8)]
    public void UpdateRaidTarget_Matches(sbyte symbol)
    {
        var (o, f) = Build(w => { w.WriteInt8(-1); w.WritePackedGuid128(Guid); w.WriteInt8(symbol); });
        var e = new Frozen.UpdateRaidTarget(); e.Read(o);
        var r = ReaderOver(f); UpdateRaidTargetCodec.Read(ref r, out var a);
        Assert.Equal(e.PartyIndex, a.PartyIndex);
        Assert.Equal(e.Target, a.Target);
        Assert.Equal(e.Symbol, a.Symbol);
        Assert.Equal(symbol, a.Symbol);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SummonResponse_Matches(bool accept)
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteBit(accept); });
        var e = new Frozen.SummonResponse(); e.Read(o);
        var r = ReaderOver(f); SummonResponseCodec.Read(ref r, out var a);
        Assert.Equal(e.SummonerGUID, a.SummonerGUID);
        Assert.Equal(e.Accept, a.Accept);
        Assert.Equal(accept, a.Accept);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void MinimapPingClient_Matches()
    {
        var (o, f) = Build(w => { w.WriteVector2(new Vector2(1234.5f, -678.25f)); w.WriteInt8(-1); });
        var e = new Frozen.MinimapPingClient(); e.Read(o);
        var r = ReaderOver(f); MinimapPingClientCodec.Read(ref r, out var a);
        Assert.Equal(e.Position, a.Position);
        Assert.Equal(e.PartyIndex, a.PartyIndex);
        Assert.Equal(new Vector2(1234.5f, -678.25f), a.Position);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(1, 100)]
    [InlineData(int.MinValue, int.MaxValue)]
    public void RandomRollClient_Matches(int min, int max)
    {
        var (o, f) = Build(w => { w.WriteInt32(min); w.WriteInt32(max); w.WriteUInt8(0); });
        var e = new Frozen.RandomRollClient(); e.Read(o);
        var r = ReaderOver(f); RandomRollClientCodec.Read(ref r, out var a);
        Assert.Equal(e.Min, a.Min);
        Assert.Equal(e.Max, a.Max);
        Assert.Equal(e.PartyIndex, a.PartyIndex);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void RequestPartyMemberStats_Matches()
    {
        var (o, f) = Build(w => { w.WriteUInt8(0); w.WritePackedGuid128(Guid); });
        var e = new Frozen.RequestPartyMemberStats(); e.Read(o);
        var r = ReaderOver(f); RequestPartyMemberStatsCodec.Read(ref r, out var a);
        Assert.Equal(e.PartyIndex, a.PartyIndex);
        Assert.Equal(e.TargetGUID, a.TargetGUID);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- ranged: pre-WotLK side, proven against the oracle ----

    [Theory]
    [InlineData(true, null)]
    [InlineData(false, 3u)]
    public void PartyInviteResponse_PreWotLK_MatchesOracle(bool accept, uint? rolesDesired)
    {
        var (o, f) = Build(w =>
        {
            w.WriteUInt8(0);
            w.WriteBit(accept);
            w.WriteBit(rolesDesired != null);
            if (rolesDesired != null)
                w.WriteUInt32(rolesDesired.Value);
        });

        var e = new Frozen.PartyInviteResponse(); e.Read(o);
        var r = ReaderOver(f); PartyInviteResponseCodecPreWotLKClassic.Read(ref r, out var a);
        Assert.Equal(e.PartyIndex, a.PartyIndex);
        Assert.Equal(e.Accept, a.Accept);
        Assert.Equal(e.RolesDesired, a.RolesDesired);
        Assert.Equal(rolesDesired, a.RolesDesired);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void PartyUninvite_PreWotLK_MatchesOracle()
    {
        const string reason = "afk too long";
        var (o, f) = Build(w =>
        {
            w.WriteUInt8(0);
            w.WritePackedGuid128(Guid);
            w.WriteBits((uint)reason.Length, 8);
            w.WriteString(reason);
        });

        var e = new Frozen.PartyUninvite(); e.Read(o);
        var r = ReaderOver(f); PartyUninviteCodecPreWotLKClassic.Read(ref r, out var a);
        Assert.Equal(e.PartyIndex, a.PartyIndex);
        Assert.Equal(e.TargetGUID, a.TargetGUID);
        Assert.Equal(e.Reason, a.Reason);
        Assert.Equal(reason, a.Reason);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetAssistantLeader_PreWotLK_MatchesOracle(bool apply)
    {
        var (o, f) = Build(w => { w.WriteUInt8(0); w.WritePackedGuid128(Guid); w.WriteBit(apply); });
        var e = new Frozen.SetAssistantLeader(); e.Read(o);
        var r = ReaderOver(f); SetAssistantLeaderCodecPreWotLKClassic.Read(ref r, out var a);
        Assert.Equal(e.PartyIndex, a.PartyIndex);
        Assert.Equal(e.TargetGUID, a.TargetGUID);
        Assert.Equal(e.Apply, a.Apply);
        Assert.Equal(apply, a.Apply);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetEveryoneIsAssistant_PreWotLK_MatchesOracle(bool apply)
    {
        var (o, f) = Build(w => { w.WriteUInt8(0); w.WriteBit(apply); });
        var e = new Frozen.SetEveryoneIsAssistant(); e.Read(o);
        var r = ReaderOver(f); SetEveryoneIsAssistantCodecPreWotLKClassic.Read(ref r, out var a);
        Assert.Equal(e.PartyIndex, a.PartyIndex);
        Assert.Equal(e.Apply, a.Apply);
        Assert.Equal(apply, a.Apply);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void DoReadyCheck_PreWotLK_MatchesOracle()
    {
        var (o, f) = Build(w => w.WriteInt8(-1));
        var e = new Frozen.DoReadyCheck(); e.Read(o);
        var r = ReaderOver(f); DoReadyCheckCodecPreWotLKClassic.Read(ref r, out var a);
        Assert.Equal(e.PartyIndex, a.PartyIndex);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReadyCheckResponseClient_PreWotLK_MatchesOracle(bool isReady)
    {
        var (o, f) = Build(w => { w.WriteUInt8(0); w.WriteBit(isReady); });
        var e = new Frozen.ReadyCheckResponseClient(); e.Read(o);
        var r = ReaderOver(f); ReadyCheckResponseClientCodecPreWotLKClassic.Read(ref r, out var a);
        Assert.Equal(e.PartyIndex, a.PartyIndex);
        Assert.Equal(e.IsReady, a.IsReady);
        Assert.Equal(isReady, a.IsReady);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)2)]
    public void SetRole_PreWotLK_MatchesOracle(byte role)
    {
        var (o, f) = Build(w => { w.WriteInt8(-1); w.WritePackedGuid128(Guid); w.WriteInt32(role); });
        var e = new Frozen.SetRole(); e.Read(o);
        var r = ReaderOver(f); SetRoleCodecPreWotLKClassic.Read(ref r, out var a);
        Assert.Equal(e.PartyIndex, a.PartyIndex);
        Assert.Equal(e.ChangedUnit, a.ChangedUnit);
        Assert.Equal(e.Role, a.Role);
        Assert.Equal(role, a.Role);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    [Fact]
    public void ChangeSubGroup_PreWotLK_MatchesOracle()
    {
        var (o, f) = Build(w => { w.WritePackedGuid128(Guid); w.WriteInt8(-1); w.WriteUInt8(3); });
        var e = new Frozen.ChangeSubGroup(); e.Read(o);
        var r = ReaderOver(f); ChangeSubGroupCodecPreWotLKClassic.Read(ref r, out var a);
        Assert.Equal(e.TargetGUID, a.TargetGUID);
        Assert.Equal(e.PartyIndex, a.PartyIndex);
        Assert.Equal(e.NewSubGroup, a.NewSubGroup);
        Assert.Equal((byte)3, a.NewSubGroup);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    /// Two GUIDs read back to back; transposing them swaps the wrong pair of players.
    [Fact]
    public void SwapSubGroups_PreWotLK_MatchesOracle()
    {
        var (o, f) = Build(w => { w.WriteInt8(-1); w.WritePackedGuid128(Guid); w.WritePackedGuid128(Guid2); });
        var e = new Frozen.SwapSubGroups(); e.Read(o);
        var r = ReaderOver(f); SwapSubGroupsCodecPreWotLKClassic.Read(ref r, out var a);
        Assert.Equal(e.FirstTarget, a.FirstTarget);
        Assert.Equal(e.SecondTarget, a.SecondTarget);
        Assert.Equal(Guid, a.FirstTarget);
        Assert.Equal(Guid2, a.SecondTarget);
        Assert.Equal(o.Remaining(), r.Remaining);
    }

    // ---- ranged: V3_4_3 side, which the oracle cannot reach in-process ----

    /// <summary>
    /// /reload emits this with every flag clear and a payload of one byte. The pre-WotLK reader
    /// would take that byte as PartyIndex and then read a bit past the end.
    /// </summary>
    [Fact]
    public void PartyInviteResponse_WotLKClassic_HandlesTheEmptyStateFlush()
    {
        var (_, f) = Build(w => { w.WriteBit(false); w.WriteBit(false); w.WriteBit(false); });
        var r = ReaderOver(f); PartyInviteResponseCodecWotLKClassic.Read(ref r, out var a);
        Assert.Equal((byte)0, a.PartyIndex);
        Assert.False(a.Accept);
        Assert.Null(a.RolesDesired);
        Assert.Equal(0, r.Remaining);
    }

    [Theory]
    [InlineData(true, true, (byte)5)]
    [InlineData(false, true, (byte)0)]
    [InlineData(true, false, (byte)0)]
    public void PartyInviteResponse_WotLKClassic_ReadsBitsFirst(bool accept, bool hasRoles, byte roles)
    {
        var (_, f) = Build(w =>
        {
            w.WriteBit(true);          // hasPartyIndex
            w.WriteBit(accept);
            w.WriteBit(hasRoles);
            w.WriteUInt8(2);           // PartyIndex
            if (hasRoles)
                w.WriteUInt8(roles);   // a byte here, where pre-WotLK sends a uint32
        });

        var r = ReaderOver(f); PartyInviteResponseCodecWotLKClassic.Read(ref r, out var a);
        Assert.Equal((byte)2, a.PartyIndex);
        Assert.Equal(accept, a.Accept);
        Assert.Equal(hasRoles ? roles : (uint?)null, a.RolesDesired);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void PartyUninvite_WotLKClassic_ReadsBitsThenGuidThenIndex()
    {
        const string reason = "bye";
        var (_, f) = Build(w =>
        {
            w.WriteBit(true);
            w.WriteBits((uint)reason.Length, 8);
            w.WritePackedGuid128(Guid);
            w.WriteUInt8(1);
            w.WriteString(reason);
        });

        var r = ReaderOver(f); PartyUninviteCodecWotLKClassic.Read(ref r, out var a);
        Assert.Equal((byte)1, a.PartyIndex);
        Assert.Equal(Guid, a.TargetGUID);
        Assert.Equal(reason, a.Reason);
        Assert.Equal(0, r.Remaining);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetAssistantLeader_WotLKClassic_ReadsBitsThenGuid(bool apply)
    {
        var (_, f) = Build(w =>
        {
            w.WriteBit(false);         // no PartyIndex byte follows
            w.WriteBit(apply);
            w.WritePackedGuid128(Guid);
        });

        var r = ReaderOver(f); SetAssistantLeaderCodecWotLKClassic.Read(ref r, out var a);
        Assert.Equal((byte)0, a.PartyIndex);
        Assert.Equal(Guid, a.TargetGUID);
        Assert.Equal(apply, a.Apply);
        Assert.Equal(0, r.Remaining);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetEveryoneIsAssistant_WotLKClassic_ReadsBitsOnly(bool apply)
    {
        var (_, f) = Build(w => { w.WriteBit(false); w.WriteBit(apply); });
        var r = ReaderOver(f); SetEveryoneIsAssistantCodecWotLKClassic.Read(ref r, out var a);
        Assert.Equal((byte)0, a.PartyIndex);
        Assert.Equal(apply, a.Apply);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void DoReadyCheck_WotLKClassic_ReadsTheOptionalIndex()
    {
        var (_, f) = Build(w => { w.WriteBit(true); w.WriteInt8(2); });
        var r = ReaderOver(f); DoReadyCheckCodecWotLKClassic.Read(ref r, out var a);
        Assert.Equal((sbyte)2, a.PartyIndex);
        Assert.Equal(0, r.Remaining);
    }

    /// <summary>
    /// The bug this split exists for. Ready is <c>C0 00</c> and Not Ready is <c>80 00</c> on the
    /// wire; the pre-WotLK reader consumed the bit byte as PartyIndex and then took the MSB of the
    /// always-zero second byte as IsReady, so every answer came back "not ready".
    /// </summary>
    [Theory]
    [InlineData(new byte[] { 0xC0, 0x00 }, true)]
    [InlineData(new byte[] { 0x80, 0x00 }, false)]
    public void ReadyCheckResponseClient_WotLKClassic_ReadsCapturedBytes(byte[] body, bool expectedReady)
    {
        byte[] framed = new byte[body.Length + 2];
        body.CopyTo(framed, 2);

        var r = ReaderOver(framed);
        ReadyCheckResponseClientCodecWotLKClassic.Read(ref r, out var a);
        Assert.Equal(expectedReady, a.IsReady);
        Assert.Equal((byte)0, a.PartyIndex);
    }

    [Theory]
    [InlineData((byte)1)]
    [InlineData((byte)2)]
    public void SetRole_WotLKClassic_ReadsGuidBeforeRole(byte role)
    {
        var (_, f) = Build(w =>
        {
            w.WriteBit(true);
            w.WritePackedGuid128(Guid);
            w.WriteUInt8(role);
            w.WriteUInt8(1);
        });

        var r = ReaderOver(f); SetRoleCodecWotLKClassic.Read(ref r, out var a);
        Assert.Equal((byte)1, a.PartyIndex);
        Assert.Equal(Guid, a.ChangedUnit);
        Assert.Equal(role, a.Role);
        Assert.Equal(0, r.Remaining);
    }

    /// <summary>
    /// The bit trails NewSubGroup here, where every other V3_4_3 party packet leads with it. Read
    /// with the pre-WotLK layout, NewSubGroup comes back as the bit byte: group 1 whenever no
    /// PartyIndex follows.
    /// </summary>
    [Theory]
    [InlineData(false, (byte)4)]
    [InlineData(true, (byte)7)]
    public void ChangeSubGroup_WotLKClassic_ReadsIndexBitAfterSubGroup(bool hasPartyIndex, byte newSubGroup)
    {
        var (_, f) = Build(w =>
        {
            w.WritePackedGuid128(Guid);
            w.WriteUInt8(newSubGroup);
            w.WriteBit(hasPartyIndex);
            if (hasPartyIndex)
                w.WriteInt8(1);
        });

        var r = ReaderOver(f); ChangeSubGroupCodecWotLKClassic.Read(ref r, out var a);
        Assert.Equal(Guid, a.TargetGUID);
        Assert.Equal(newSubGroup, a.NewSubGroup);
        Assert.Equal(hasPartyIndex ? (sbyte)1 : (sbyte)0, a.PartyIndex);
        Assert.Equal(0, r.Remaining);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SwapSubGroups_WotLKClassic_ReadsBitThenGuidsThenIndex(bool hasPartyIndex)
    {
        var (_, f) = Build(w =>
        {
            w.WriteBit(hasPartyIndex);
            w.WritePackedGuid128(Guid);
            w.WritePackedGuid128(Guid2);
            if (hasPartyIndex)
                w.WriteInt8(1);
        });

        var r = ReaderOver(f); SwapSubGroupsCodecWotLKClassic.Read(ref r, out var a);
        Assert.Equal(Guid, a.FirstTarget);
        Assert.Equal(Guid2, a.SecondTarget);
        Assert.Equal(hasPartyIndex ? (sbyte)1 : (sbyte)0, a.PartyIndex);
        Assert.Equal(0, r.Remaining);
    }

    /// <summary>
    /// The opcodes were once mapped under their native names, CMSG_CHANGE_SUB_GROUP and
    /// CMSG_SWAP_SUB_GROUPS, which no handler claims — so a raid subgroup drag was named in the log
    /// and still dropped. They must resolve to the names GroupSystem registers.
    /// </summary>
    [Theory]
    [InlineData(13903u, Opcode.CMSG_GROUP_CHANGE_SUB_GROUP)]
    [InlineData(13904u, Opcode.CMSG_GROUP_SWAP_SUB_GROUP)]
    public void SubGroupOpcodes_WotLKClassic_ResolveToTheHandledNames(uint wire, Opcode expected)
        => Assert.Equal(expected, Opcodes.GetUniversalOpcode(wire, ClientVersionBuild.V3_4_3_54261));
}
