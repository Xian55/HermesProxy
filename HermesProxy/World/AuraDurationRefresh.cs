using System;
using System.Runtime.InteropServices;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World;

/// <summary>
/// Re-applies the durations the proxy has tracked to an aura list that was built before those
/// durations arrived. A login sends the player's auras in the create block and the duration
/// packets just after it; the create is held while item templates are resolved, so the updates
/// built from it can predate <c>SMSG_UPDATE_AURA_DURATION</c> and <c>SMSG_SET_EXTRA_AURA_INFO</c>.
/// </summary>
internal static class AuraDurationRefresh
{
    public static void Apply(AuraUpdate update, GameSessionData gameState)
    {
        foreach (ref var aura in CollectionsMarshal.AsSpan(update.Auras))
        {
            if (aura.AuraData == null)
                continue;

            gameState.GetAuraDuration(update.UnitGUID, aura.Slot, out int left, out int full);
            if (left <= 0 && full <= 0)
                continue;

            int duration = full > 0 ? full : left;
            int remaining = left > 0 ? left : full;
            aura.AuraData.Flags |= AuraFlagsModern.Duration;
            aura.AuraData.Duration = duration;
            aura.AuraData.Remaining = remaining;
            aura.ExpiresAt = remaining > 0 ? Environment.TickCount + remaining : 0;
        }
    }
}
