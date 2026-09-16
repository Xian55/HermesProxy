using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Outbox;

/// <summary>What kind of occurrence an <see cref="OutboxEvent"/> names.</summary>
public enum OutboxEventKind : byte
{
    /// <summary>A packet with this universal opcode was written through the outbox.</summary>
    OpcodeSent = 1,
    /// <summary>The legacy server's packet with this universal opcode finished its handler.</summary>
    OpcodeHandled = 2,
    /// <summary>A named point in a handler, raised explicitly with <c>Notify</c>.</summary>
    Signal = 3,
    /// <summary>The item template with this entry arrived.</summary>
    ItemTemplate = 4,
    /// <summary>The item text with this id arrived.</summary>
    ItemText = 5,
    /// <summary>
    /// The client has this guid's object. Raised for the player at the end of every update batch
    /// while the client knows it, so a hold registered late still goes out at the next batch.
    /// </summary>
    GuidKnown = 6,
    /// <summary>Internal: a gate opened. Raised by <c>SetGate</c>, never by callers.</summary>
    GateOpened = 7,
}

/// <summary>What a hold waits for. Only used in logs.</summary>
internal enum OutboxHoldKind : byte
{
    Event,
    Gate,
    Timer,
    Lane,
}

/// <summary>Named points in a handler that holds can wait for.</summary>
public enum OutboxSignal : ulong
{
    /// <summary>The legacy <c>SMSG_UPDATE_OBJECT</c> handler has sent everything for its batch.</summary>
    UpdateBatchEnd = 1,
}

/// <summary>
/// A state a hold waits for. Unlike an <see cref="OutboxEvent"/>, a gate is remembered: a hold
/// registered while its gate is already open goes out immediately.
/// </summary>
public enum OutboxGate : byte
{
    /// <summary>The player is in the world (legacy <c>SMSG_LOGIN_VERIFY_WORLD</c> seen).</summary>
    InWorld = 0,
    /// <summary>
    /// Server outbox: open unless our last <c>CMSG_ATTACK_SWING</c> is still waiting for the server's
    /// answer. Opened for every new GameState. See <c>MeleeAttackOrder</c>.
    /// </summary>
    SwingAnswered = 1,
}

/// <summary>What a hold is tied to, so tearing that thing down drops the hold with it.</summary>
public enum OutboxScope : byte
{
    /// <summary>Dropped when the session's GameState is replaced (logout, disconnect).</summary>
    GameState = 0,
    /// <summary>Dropped when the legacy world connection goes away (change realm, disconnect).</summary>
    LegacyConnection = 1,
    /// <summary>Dropped only when the whole session ends.</summary>
    Session = 2,
}

/// <summary>What happens to a hold whose timeout expires.</summary>
public enum OutboxTimeoutAction : byte
{
    /// <summary>Send it anyway. For holds where late is better than never.</summary>
    Release = 0,
    /// <summary>Drop it. For holds whose payload is useless once stale.</summary>
    Discard = 1,
}

/// <summary>Identifies an occurrence a hold can wait for. Value type; building one allocates nothing.</summary>
public readonly record struct OutboxEvent(OutboxEventKind Kind, ulong A, ulong B = 0)
{
    public static OutboxEvent OpcodeSent(Opcode opcode) => new(OutboxEventKind.OpcodeSent, (ulong)opcode);
    public static OutboxEvent OpcodeHandled(Opcode opcode) => new(OutboxEventKind.OpcodeHandled, (ulong)opcode);
    public static OutboxEvent Signal(OutboxSignal signal) => new(OutboxEventKind.Signal, (ulong)signal);
    public static OutboxEvent ItemTemplate(uint entry) => new(OutboxEventKind.ItemTemplate, entry);
    public static OutboxEvent ItemText(uint id) => new(OutboxEventKind.ItemText, id);
    public static OutboxEvent GuidKnown(WowGuid128 guid) => new(OutboxEventKind.GuidKnown, guid.Low, guid.High);
    internal static OutboxEvent GateOpened(OutboxGate gate) => new(OutboxEventKind.GateOpened, (ulong)gate);
}

/// <summary>
/// Names a hold so it can be cancelled, released early, coalesced or paced. The kind keeps
/// unrelated features from colliding on the same number.
/// </summary>
public readonly record struct HoldKey(HoldKeyKind Kind, ulong A = 0, ulong B = 0);

/// <summary>Namespaces for <see cref="HoldKey"/>. Add one per feature that needs a keyed hold.</summary>
public enum HoldKeyKind : ushort
{
    /// <summary>For tests and ad-hoc use; never used by production code.</summary>
    Test = 0,
    /// <summary>A = rank id. Newest rank-permission edit per rank, see GuildSystem.</summary>
    GuildRankPermissions = 1,
    /// <summary>Vanilla multi-attachment mail, paced so the server's antiflood doesn't fire.</summary>
    MailAntiflood = 2,
    /// <summary>Lane that runs one partial-stack auction post at a time.</summary>
    AuctionSplitLane = 3,
    /// <summary>The running auction post's wait for the split or freed bag slot.</summary>
    AuctionSplitWait = 4,
    /// <summary>A = guid low, B = guid high. A V3_4_3 corpse destroy held to the end of the next update batch.</summary>
    CorpseDestroy = 5,
    /// <summary>V3_4_3 toy box sync held until the client has the player object.</summary>
    ToysSync = 6,
    /// <summary>V3_4_3 pet spell bar held until the pet's create has gone out.</summary>
    PetSpells = 7,
    /// <summary>V3_4_3 pet create batches held until the client has the player object.</summary>
    PetUpdateBatch = 8,
    /// <summary>The newest pre-3.3.0 mail list, held until its letter texts arrive.</summary>
    MailList = 9,
    /// <summary>A <c>CMSG_ATTACK_STOP</c> held until the server answers the swing before it.</summary>
    AttackStop = 10,
    /// <summary>V3_4_3 Values for the player's own guid, held until the client has the player.</summary>
    PlayerValuesBatch = 11,
    /// <summary>V3_4_3 speed changes aimed at the player, held until the client has the player.</summary>
    PlayerMoveSpeed = 12,
}

/// <summary>How a hold behaves while it waits.</summary>
/// <param name="Timeout">
/// How long the hold may wait. Null means <see cref="OutboxOptions.DefaultTimeout"/>, so no hold
/// can wait forever.
/// </param>
/// <param name="OnTimeout">Send or drop when the timeout expires.</param>
/// <param name="Scope">What tearing down drops this hold.</param>
/// <param name="Key">Optional name for <c>Cancel</c> / <c>Release</c>.</param>
/// <param name="RunOnTimer">
/// The release reads no session state, so a deadline may run it straight from the timer thread.
/// Without this, a deadline-driven release of a continuation or a client packet waits for
/// <c>Tick</c>, which runs on a thread that owns session state.
/// </param>
public readonly record struct HoldOptions(
    TimeSpan? Timeout = null,
    OutboxTimeoutAction OnTimeout = OutboxTimeoutAction.Release,
    OutboxScope Scope = OutboxScope.GameState,
    HoldKey? Key = null,
    bool RunOnTimer = false);

/// <summary>Per-outbox limits.</summary>
public sealed class OutboxOptions
{
    /// <summary>Timeout applied to a hold that did not choose one. Dropped, not sent, when it expires.</summary>
    public TimeSpan DefaultTimeout { get; init; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Most holds one outbox may carry. A new hold past this is refused and the overflow callback
    /// runs, so a peer that never answers can't grow the proxy's memory without bound.
    /// </summary>
    public int MaxPendingHolds { get; init; } = 4096;
}
