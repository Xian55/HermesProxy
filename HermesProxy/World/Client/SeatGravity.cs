using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;

namespace HermesProxy.World.Client;

/// <summary>Where the proxy's own hold on the player's gravity stands.</summary>
public enum SeatGravityState : byte
{
    /// <summary>The proxy has not touched the player's gravity.</summary>
    None,

    /// <summary>Switched off by the proxy for a vehicle seat taken on a transport.</summary>
    Held,

    /// <summary>Switched back on; the client's ack has not come back yet.</summary>
    Releasing,
}

/// <summary>
/// Keeps the player in a vehicle seat it takes while standing on a boat.
/// </summary>
/// <remarks>
/// A 3.4.3 client ends every seat move with a one-tick fall and a landing, and a landing on a
/// transport attaches it to that transport. So a player who boards a vehicle from a boat's deck
/// is put back on the boat by its own client: rooted on the deck, the seat empty, with nothing to
/// click to get out until the server unseats it. A native 3.4.3 server shows the same flaw.
/// <para>
/// Seats flagged to disable gravity do not have it: with gravity off the client neither falls
/// nor lands after the seat move and stays on the vehicle. The proxy borrows that. It switches
/// the passenger's gravity off ahead of such a seat move and back on when the server unseats it.
/// The legacy server asked for neither change, so both acks stop at the proxy and the flag is
/// kept out of the movement the server is shown.
/// </para>
/// </remarks>
internal static class SeatGravity
{
    /// <summary>
    /// Sequence index on the proxy's own gravity packets. A legacy server counts a player's
    /// movement changes up from zero, so a gravity ack carrying this index is the proxy's.
    /// </summary>
    public const uint SequenceIndex = 0xFFFFFF00;

    /// <summary>Only a 3.4.3 client behind a server that has vehicles needs any of this.</summary>
    public static bool Applies =>
        ModernVersion.Build == ClientVersionBuild.V3_4_3_54261 &&
        LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056);

    /// <summary>
    /// True when a seat the player is about to take needs its gravity off: the client last
    /// reported itself on a transport object, which is what its landing would put it back on.
    /// </summary>
    public static bool ShouldHold(GameSessionData gameState) =>
        Applies &&
        gameState.SeatGravity != SeatGravityState.Held &&
        !gameState.ServerDisabledGravity &&
        gameState.LastReportedTransportGuid.GetObjectType() == ObjectType.GameObject;

    /// <summary>
    /// True for the ack of a gravity packet the proxy sent itself, which the legacy server must
    /// not see. Ends the hold when it is the ack of the release.
    /// </summary>
    public static bool ConsumeOwnAck(Opcode opcode, uint moveCounter, GameSessionData gameState)
    {
        if (moveCounter != SequenceIndex)
            return false;

        if (opcode == Opcode.CMSG_MOVE_GRAVITY_ENABLE_ACK)
        {
            if (gameState.SeatGravity == SeatGravityState.Releasing)
                gameState.SeatGravity = SeatGravityState.None;
            return true;
        }
        return opcode == Opcode.CMSG_MOVE_GRAVITY_DISABLE_ACK;
    }

    /// <summary>
    /// Writes the client's movement block for the legacy server, without the gravity flag while
    /// the proxy is the one that set it.
    /// </summary>
    public static void WriteClientMovement(WorldPacket packet, in MovementInfo moveInfo, GameSessionData gameState)
    {
        if (gameState.SeatGravity == SeatGravityState.None)
        {
            LegacyMovementCodec.Write(packet, in moveInfo);
            return;
        }

        MovementInfo shown = moveInfo;
        shown.Flags &= ~MovementFlagModern.DisableGravity;
        LegacyMovementCodec.Write(packet, in shown);
    }
}
