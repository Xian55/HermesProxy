using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World;

/// <summary>
/// The durations the proxy tracks for a unit's aura slots, from <c>SMSG_UPDATE_AURA_DURATION</c> and
/// <c>SMSG_SET_EXTRA_AURA_INFO</c>, put on the auras read from its update fields.
/// </summary>
internal static class AuraDurationRefresh
{
    /// <summary>
    /// Gives the aura its tracked duration and the time it has left on the proxy's clock. One that
    /// has none, or whose time has run out, gets no timer: its full duration again would show a
    /// fresh timer on an aura that is about to go.
    /// </summary>
    public static void Apply(AuraDataInfo aura, WowGuid128 unit, byte slot, GameSessionData gameState)
    {
        gameState.GetAuraDuration(unit, slot, out int left, out int full);
        if (left <= 0 || full <= 0)
            return;

        aura.Flags |= AuraFlagsModern.Duration;
        aura.Duration = full;
        aura.Remaining = left;
    }

    /// <summary>
    /// Re-applies the tracked durations to an aura list built before they arrived. A login sends the
    /// player's auras in the create block and the duration packets just after it; the create is held
    /// while item templates are resolved, so the list built from it can predate them.
    /// </summary>
    public static void Apply(AuraUpdate update, GameSessionData gameState)
    {
        foreach (var aura in update.Auras)
        {
            if (aura.AuraData != null)
                Apply(aura.AuraData, update.UnitGUID, aura.Slot, gameState);
        }
    }
}
