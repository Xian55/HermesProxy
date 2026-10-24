using System.Linq;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// A refreshed aura carries <see cref="AuraFlagsModern.Refresh"/> to the modern client, which logs
/// SPELL_AURA_REFRESH for nothing else (issue #480). A 3.3.5a server sends the same aura again in
/// its slot with a new duration, so the proxy tells a refresh by the aura ending later than it did.
/// </summary>
public sealed class AuraRefreshFlagTests
{
    private const uint Fortitude = 1243; // Power Word: Fortitude, rank 1
    private const int ThirtyMinutes = 1_800_000;
    private const byte Positive = 0x10;
    private const byte Duration = 0x20;
    private const byte Effect0 = 0x01;
    private const int Start = 1_000_000;

    private readonly LegacyHandlerHarness _harness = new(recordClientPackets: true);
    private readonly WowGuid64 _player = new(HighGuidTypeLegacy.Player, 77);

    public AuraRefreshFlagTests() => _harness.SetActivePlayer(_player);

    [Fact]
    public void SameAuraWithALaterEnd_IsSentAsARefresh()
    {
        Deliver(Fortitude, ThirtyMinutes, at: Start);
        Deliver(Fortitude, ThirtyMinutes, at: Start + 3_000);

        Assert.Equal(0, SentFlags(0) & (ushort)AuraFlagsModern.Refresh);
        Assert.Equal((ushort)AuraFlagsModern.Refresh, SentFlags(1) & (ushort)AuraFlagsModern.Refresh);
    }

    [Fact]
    public void TheKeptCopy_HasNoRefresh()
    {
        Deliver(Fortitude, ThirtyMinutes, at: Start);
        Deliver(Fortitude, ThirtyMinutes, at: Start + 3_000);

        var kept = _harness.Session.GameState.KnownAuras[_player.To128(_harness.Session.GameState)][0];
        Assert.False(kept.AuraData.Flags.HasFlag(AuraFlagsModern.Refresh));
    }

    [Fact]
    public void AnUpdateThatKeepsTheEnd_IsNoRefresh()
    {
        // The same end seen 3 s later: an amount or caster change, not a new duration.
        Deliver(Fortitude, ThirtyMinutes, at: Start);
        Deliver(Fortitude, ThirtyMinutes - 3_000, at: Start + 3_000);

        Assert.Equal(0, SentFlags(1) & (ushort)AuraFlagsModern.Refresh);
    }

    [Fact]
    public void LatencyBetweenTwoPackets_IsNoRefresh()
    {
        Deliver(Fortitude, ThirtyMinutes, at: Start);
        Deliver(Fortitude, ThirtyMinutes - 3_000, at: Start + 3_000 + WorldClient.AuraRefreshMinGainMs);

        Assert.Equal(0, SentFlags(1) & (ushort)AuraFlagsModern.Refresh);
    }

    [Fact]
    public void AStackChange_IsNoRefresh()
    {
        Deliver(Fortitude, ThirtyMinutes, at: Start, applications: 1);
        Deliver(Fortitude, ThirtyMinutes, at: Start + 3_000, applications: 2);

        Assert.Equal(0, SentFlags(1) & (ushort)AuraFlagsModern.Refresh);
    }

    [Fact]
    public void AnotherSpellInTheSlot_IsNoRefresh()
    {
        Deliver(Fortitude, ThirtyMinutes, at: Start);
        Deliver(1244, ThirtyMinutes, at: Start + 3_000); // rank 2

        Assert.Equal(0, SentFlags(1) & (ushort)AuraFlagsModern.Refresh);
    }

    [Fact]
    public void AnAuraWithoutDuration_IsNeverARefresh()
    {
        Deliver(465, null, at: Start); // Devotion Aura
        Deliver(465, null, at: Start + 3_000);

        Assert.Equal(0, SentFlags(1) & (ushort)AuraFlagsModern.Refresh);
    }

    [Fact]
    public void TheSameAuraAfterItsRemoval_IsNoRefresh()
    {
        Deliver(Fortitude, ThirtyMinutes, at: Start);
        Deliver(0, null, at: Start + 1_000);
        Deliver(Fortitude, ThirtyMinutes, at: Start + 3_000);

        Assert.Equal(0, SentFlags(2) & (ushort)AuraFlagsModern.Refresh);
    }

    [Fact]
    public void AFullResync_CarriesNoRefresh()
    {
        Deliver(Fortitude, ThirtyMinutes, at: Start);
        // A second slot so the resync is not dropped as a repeat of what the client has.
        var wire = LegacyPacketBuilder.Build(Opcode.SMSG_AURA_UPDATE_ALL, packet =>
        {
            packet.WritePackedGuid(_player);
            WriteAura(packet, 0, Fortitude, ThirtyMinutes, 0);
            WriteAura(packet, 1, 465, null, 0);
        });
        _harness.Deliver(Opcode.SMSG_AURA_UPDATE_ALL, wire, _harness.Client.HandleAuraUpdate, Start + 3_000);

        var update = Assert.IsType<AuraUpdate>(_harness.ClientWire.Sent.ElementAt(1).Packet);
        Assert.True(update.UpdateAll);
        Assert.Equal(2, update.Auras.Count);
        Assert.All(update.Auras, aura => Assert.False(aura.AuraData.Flags.HasFlag(AuraFlagsModern.Refresh)));
    }

    private void Deliver(uint spellId, int? remaining, int at, byte applications = 0)
    {
        var wire = LegacyPacketBuilder.Build(Opcode.SMSG_AURA_UPDATE, packet =>
        {
            packet.WritePackedGuid(_player);
            WriteAura(packet, 0, spellId, remaining, applications);
        });
        _harness.Deliver(Opcode.SMSG_AURA_UPDATE, wire, _harness.Client.HandleAuraUpdate, at);
    }

    // The flags as written for the client, from the packet's own bytes.
    private ushort SentFlags(int index)
    {
        var sent = _harness.ClientWire.Sent.ElementAt(index);
        Assert.Single(Assert.IsType<AuraUpdate>(sent.Packet).Auras);
        using var reader = new WorldPacket(1, sent.Bytes);
        Assert.False(reader.ReadBit()); // not UpdateAll
        Assert.Equal(1u, reader.ReadBits<uint>(9));
        reader.ReadUInt8(); // slot
        Assert.True(reader.ReadBit());
        reader.ResetBitPos();
        reader.ReadPackedGuid128(); // cast ID
        reader.ReadUInt32(); // spell
        reader.ReadUInt32(); // spell visual
        return reader.ReadUInt16();
    }

    private void WriteAura(WorldPacket packet, byte slot, uint spellId, int? remaining, byte applications)
    {
        packet.WriteUInt8(slot);
        packet.WriteUInt32(spellId);
        if (spellId == 0)
            return;
        byte flags = (byte)(Positive | Effect0 | (remaining.HasValue ? Duration : 0));
        if (WorldClient.IsCataLegacy) // 4.3.4 widened the flags to 16 bits, same values
            packet.WriteUInt16(flags);
        else
            packet.WriteUInt8(flags);
        packet.WriteUInt8(80);
        packet.WriteUInt8(applications);
        packet.WritePackedGuid(_player);
        if (remaining.HasValue)
        {
            packet.WriteInt32(ThirtyMinutes);
            packet.WriteInt32(remaining.Value);
        }
    }
}
