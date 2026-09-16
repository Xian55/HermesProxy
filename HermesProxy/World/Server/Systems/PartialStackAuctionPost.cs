using System;
using System.Collections.Generic;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Outbox;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>The inventory reads a partial-stack auction post needs.</summary>
internal interface IAuctionInventory
{
    uint GetItemStackCount(WowGuid128 item);
    (byte containerSlot, byte slot)? FindEmptyInventorySlot();
    (byte containerSlot, byte slot)? FindItemInInventory(WowGuid64 item);
    WowGuid64 GetInventorySlotItem(byte containerSlot, byte slot);
}

/// <summary>Reads the session's current GameState, resolved per call so a replaced one is seen.</summary>
internal sealed class SessionAuctionInventory(GlobalSessionData session) : IAuctionInventory
{
    public uint GetItemStackCount(WowGuid128 item) => session.GameState.GetItemStackCount(item);
    public (byte containerSlot, byte slot)? FindEmptyInventorySlot() => session.GameState.FindEmptyInventorySlot();
    public (byte containerSlot, byte slot)? FindItemInInventory(WowGuid64 item) => session.GameState.FindItemInInventory(item);
    public WowGuid64 GetInventorySlotItem(byte containerSlot, byte slot) => session.GameState.GetInventorySlotItem(containerSlot, slot);
}

/// <summary>
/// Posts auctions to a pre-3.2.2a server, splitting partial stacks into a temporary bag slot first.
/// </summary>
/// <remarks>
/// <para>
/// These servers auction a whole item, so selling part of a stack means:
/// <list type="number">
/// <item><c>CMSG_SPLIT_ITEM</c> the wanted count into an empty slot;</item>
/// <item>wait for the inventory update that fills that slot;</item>
/// <item>auction the new item;</item>
/// <item>when more items follow, wait for the slot to empty before the next split.</item>
/// </list>
/// The old handler did the waiting with <c>Thread.Sleep</c> on the socket thread, which froze every
/// other packet from the client and read the slot on the hope the other thread had filled it.
/// </para>
/// <para>
/// Here each wait is an outbox hold released at the end of every legacy update batch. The hold is
/// registered <i>before</i> the slot is checked, so an update landing between the two can't be
/// missed. Posts run one at a time per session: two would pick the same empty slot.
/// </para>
/// <para>
/// A split whose slot never fills within <see cref="SplitFillTimeout"/> aborts the rest of the post:
/// guessing on would auction the wrong item or quantity. A slot that doesn't empty within
/// <see cref="SlotFreeTimeout"/> is waited out and the post continues, as before.
/// </para>
/// </remarks>
internal sealed class PartialStackAuctionPost
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melLog = Log.CreateMelLogger(Log.CategoryPacket);
    private static readonly string _sourceFile = nameof(WorldSocket).PadRight(15);
    private static readonly string _netDirSend = Log.FormatDir(LogNetDir.P2S);

    internal static readonly TimeSpan SplitFillTimeout = TimeSpan.FromSeconds(3);
    internal static readonly TimeSpan SlotFreeTimeout = TimeSpan.FromSeconds(2);

    private static readonly HoldKey Lane = new(HoldKeyKind.AuctionSplitLane);

    // A post still queued behind another after this long is dropped; the one ahead is stuck.
    private static readonly HoldOptions LaneOptions = new(Timeout: TimeSpan.FromSeconds(60));

    private readonly ServerOutbox _toServer;
    private readonly IAuctionInventory _inventory;
    private readonly TimeProvider _time;
    private readonly WowGuid128 _auctioneer;
    private readonly ulong _minBid;
    private readonly ulong _buyout;
    private readonly uint _expireTime;
    private readonly List<AuctionItemForSale> _items;

    private (byte containerSlot, byte slot)? _splitSlot;
    private int _next;
    private ulong _waits;

    public PartialStackAuctionPost(ServerOutbox toServer, IAuctionInventory inventory,
        WowGuid128 auctioneer, ulong minBid, ulong buyout, uint expireTime, List<AuctionItemForSale> items,
        TimeProvider? time = null)
    {
        _toServer = toServer;
        _inventory = inventory;
        _auctioneer = auctioneer;
        _minBid = minBid;
        _buyout = buyout;
        _expireTime = expireTime;
        _items = items;
        _time = time ?? TimeProvider.System;
    }

    /// <summary>Starts the post now, or after the post already running for this session finishes.</summary>
    public void Start() => _toServer.Exclusive(Lane, Begin, LaneOptions);

    private void Begin()
    {
        try
        {
            bool needsSplit = false;
            foreach (var item in _items)
            {
                if (item.UseCount > 0 && item.UseCount < _inventory.GetItemStackCount(item.Guid))
                {
                    needsSplit = true;
                    break;
                }
            }

            if (needsSplit)
            {
                _splitSlot = _inventory.FindEmptyInventorySlot();
                if (_splitSlot == null)
                    Log.Print(LogType.Error, "AuctionSellItem: Cannot split stack — no empty bag slot");
            }

            PostNext();
        }
        catch
        {
            _toServer.LaneDone(Lane);
            throw;
        }
    }

    private void PostNext()
    {
        while (_next < _items.Count)
        {
            var item = _items[_next];

            if (item.UseCount > 0 && _splitSlot is { } slot && item.UseCount < _inventory.GetItemStackCount(item.Guid))
            {
                var itemLocation = _inventory.FindItemInInventory(item.Guid.To64());
                if (itemLocation == null)
                {
                    Log.Print(LogType.Error, "AuctionSellItem: Cannot split stack — item not found in inventory");
                    _next++;
                    continue;
                }

                WorldPacket splitPacket = new WorldPacket(Opcode.CMSG_SPLIT_ITEM);
                splitPacket.WriteUInt8(itemLocation.Value.containerSlot);
                splitPacket.WriteUInt8(itemLocation.Value.slot);
                splitPacket.WriteUInt8(slot.containerSlot);
                splitPacket.WriteUInt8(slot.slot);
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                    splitPacket.WriteInt32((int)item.UseCount);
                else
                    splitPacket.WriteUInt8((byte)item.UseCount);
                _toServer.Send(splitPacket);

                Wait(SplitSlotFilled, SplitFillTimeout, OnSplitSlotFilled);
                return;
            }

            SellAndContinue(item.Guid.To64());
            if (_next < 0)
                return;
        }

        Finish();
    }

    private void OnSplitSlotFilled(bool filled)
    {
        if (!filled)
        {
            WorldSocketLogMessages.AuctionSplitNeverFilled(_melLog, _sourceFile, _netDirSend, SplitFillTimeout.TotalSeconds, _items.Count - _next);
            Finish();
            return;
        }

        var slot = _splitSlot!.Value;
        SellAndContinue(_inventory.GetInventorySlotItem(slot.containerSlot, slot.slot));
        if (_next >= 0)
            PostNext();
    }

    /// <summary>
    /// Posts one auction and advances. When more items follow a split, waits for the temp slot to
    /// empty first and sets <see cref="_next"/> negative to tell the caller the sequence is parked.
    /// </summary>
    private void SellAndContinue(WowGuid64 itemGuid)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_AUCTION_SELL_ITEM);
        packet.WriteGuid(_auctioneer.To64());
        packet.WriteGuid(itemGuid);
        packet.WriteUInt32((uint)_minBid);
        packet.WriteUInt32((uint)_buyout);
        packet.WriteUInt32(_expireTime);
        _toServer.Send(packet);
        _next++;

        if (_splitSlot != null && _items.Count > 1 && _next < _items.Count)
        {
            int resumeAt = _next;
            _next = -1;
            Wait(SplitSlotEmpty, SlotFreeTimeout, _ =>
            {
                _next = resumeAt;
                PostNext();
            });
        }
    }

    private bool SplitSlotFilled()
        => _inventory.GetInventorySlotItem(_splitSlot!.Value.containerSlot, _splitSlot.Value.slot) != WowGuid64.Empty;

    private bool SplitSlotEmpty()
        => _inventory.GetInventorySlotItem(_splitSlot!.Value.containerSlot, _splitSlot.Value.slot) == WowGuid64.Empty;

    private void Wait(Func<bool> condition, TimeSpan timeout, Action<bool> then)
    {
        // A key per wait, so a check left over from this wait can never cancel the next one.
        var key = new HoldKey(HoldKeyKind.AuctionSplitWait, ++_waits);
        long started = _time.GetTimestamp();
        Arm(key, started, condition, timeout, then);
    }

    private void Arm(HoldKey key, long started, Func<bool> condition, TimeSpan timeout, Action<bool> then)
    {
        var remaining = timeout - _time.GetElapsedTime(started);
        if (remaining <= TimeSpan.Zero)
        {
            Resume(then, condition());
            return;
        }

        // Released by the end of the next update batch, or by the deadline. Either way re-check.
        _toServer.When(
            OutboxEvent.Signal(OutboxSignal.UpdateBatchEnd),
            () =>
            {
                if (condition())
                    Resume(then, true);
                else
                    Arm(key, started, condition, timeout, then);
            },
            new HoldOptions(Timeout: remaining, OnTimeout: OutboxTimeoutAction.Release, Key: key));

        // The update may already have landed before the hold existed. Cancel succeeds only while
        // the hold still waits, so exactly one of this check and the hold continues the post.
        if (condition() && _toServer.Cancel(key))
            Resume(then, true);
    }

    private void Resume(Action<bool> then, bool satisfied)
    {
        try
        {
            then(satisfied);
        }
        catch
        {
            Finish();
            throw;
        }
    }

    private void Finish()
    {
        _next = _items.Count;
        _toServer.LaneDone(Lane);
    }
}
