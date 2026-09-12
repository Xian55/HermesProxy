using System;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using HermesProxy.World.Server.Systems;

namespace HermesProxy.World.Server;

public partial class WorldSocket
{
    // The guild CMSG translation lives in World/Server/Systems/GuildSystem.cs. What is left here
    // is socket state the systems cannot own: the rank-permissions coalescer, which OnClose also
    // drains, and CMSG_TABARD_VENDOR_ACTIVATE, which shares InteractWithNPC with five handlers in
    // three other files and so converts with that type rather than with this one.

    // One Apply in the 3.4.3 guild control panel sends a CMSG_GUILD_SET_RANK_PERMISSIONS per
    // changed setting, all in the same millisecond and each carrying the rank's complete state:
    // a native Wrathion capture shows five for one Apply. A 3.3.5a client sends one
    // CMSG_GUILD_RANK, and AzerothCore kicks after three in one second (antidos_opcode_policies,
    // opcode 561). Only the last of a burst matters, so hold them briefly and forward the newest
    // per rank. Issue #283.
    private const int RankPermissionsCoalesceMs = 100;
    private readonly LatestPerKeyCoalescer<uint, GuildSetRankPermissions> _pendingRankPermissions = new();
    // Armed on the packet thread and disposed from it, the timer callback or OnClose, so every
    // swap goes through Interlocked.
    private System.Threading.Timer? _rankPermissionsTimer;

    /// <summary>
    /// Holds one rank's newest state and arms the flush. Called by
    /// <c>GuildSystem.HandleGuildSetRankPermissions</c>, which has no <c>this</c> to reach these.
    /// </summary>
    internal void OfferRankPermissions(in GuildSetRankPermissions rank)
    {
        if (_pendingRankPermissions.Offer(rank.RankID, rank))
        {
            var timer = new System.Threading.Timer(OnRankPermissionsDue, null, RankPermissionsCoalesceMs, System.Threading.Timeout.Infinite);
            System.Threading.Interlocked.Exchange(ref _rankPermissionsTimer, timer)?.Dispose();
        }
    }

    private void OnRankPermissionsDue(object? state)
    {
        System.Threading.Interlocked.Exchange(ref _rankPermissionsTimer, null)?.Dispose();
        FlushRankPermissions();
    }

    private void FlushRankPermissions()
    {
        // Runs on a timer thread or from OnClose. An exception escaping a timer callback has no
        // handler above it and would take the whole proxy down.
        try
        {
            var ranks = _pendingRankPermissions.Drain(out int received);
            if (ranks.Count == 0)
                return;

            WorldSocketLogMessages.GuildRankPermissionsCoalesced(_melLog, _sourceFile, _netDirRecv, received, ranks.Count);
            foreach (var rank in ranks)
                SendPacketToServer(GuildSystem.BuildLegacyGuildRank(rank));
        }
        catch (Exception ex)
        {
            WorldSocketLogMessages.GuildRankPermissionsFlushFailed(_melLog, _sourceFile, _netDirRecv, ex);
        }
    }

    [PacketHandler(Opcode.CMSG_TABARD_VENDOR_ACTIVATE)]
    void HandleTabardVendorActivate(InteractWithNPC interact)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_TABARDVENDOR_ACTIVATE);
        packet.WriteGuid(interact.CreatureGUID.To64());
        SendPacketToServer(packet);
    }
}
