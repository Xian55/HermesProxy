using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // A 4.3.4 server has currency packets of its own, where the older ones keep honor and arena
    // points in player fields and everything else as items (see RefreshCurrencies).

    /// <summary>
    /// 4.3.4's Honor Points. The 4.4.2 client still has the row, named "Honor Deprecated 3"; the
    /// honor its PvP window and its currency tab mean is 1901, as in Wrath Classic.
    /// </summary>
    private const uint HonorPointsCata = 392;

    internal static uint ModernCurrencyId(uint cataCurrencyId)
        => cataCurrencyId == HonorPointsCata ? (uint)Currency.HonorPoints : cataCurrencyId;

    /// <summary>The other way round, for a currency the client names in a request.</summary>
    internal static uint CataCurrencyId(uint modernCurrencyId)
        => modernCurrencyId == (uint)Currency.HonorPoints ? HonorPointsCata : modernCurrencyId;

    /// <summary>
    /// What an amount of a currency is multiplied by on its way to the 4.4.2 client. Conquest,
    /// Honor, Justice and Valor Points and the Conquest meta currencies are kept in hundredths
    /// (CurrencyTypes flag 0x8): a 4.3.4 server divides by 100 before it sends an amount, the 4.4.2
    /// client divides what it is sent. Seen on the client: 200 sent for Conquest, Honor (1901),
    /// Justice, Valor and the arena meta currency reads as 2, where 30 Tol Barad Commendations
    /// (391) and 30 Champion's Seals (241) read as 30. The battleground meta currency (484) was
    /// not measured and is taken to be like its arena twin.
    /// </summary>
    internal static uint ModernCurrencyScale(uint modernCurrencyId)
        => modernCurrencyId is 390 or 395 or 396 or 483 or 484 or (uint)Currency.HonorPoints ? 100u : 1u;

    /// <summary>
    /// The whole list, at login and after a battle (TrinityCore 4.3.4 <c>SetupCurrency::Write</c>):
    /// a 23-bit count, seven bits a currency, then the amounts. A currency's ceiling is not in it;
    /// the client has that in its own data.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_SETUP_CURRENCY)]
    internal void HandleSetupCurrency(WorldPacket packet)
    {
        uint count = packet.ReadBits<uint>(23);
        var present = new (bool Weekly, byte Flags, bool MaxWeekly, bool Tracked)[count];
        for (uint i = 0; i < count; i++)
        {
            bool weekly = packet.HasBit();
            byte flags = packet.ReadBits<byte>(4);
            present[i] = (weekly, flags, packet.HasBit(), packet.HasBit());
        }

        SetupCurrency currencies = new();
        for (uint i = 0; i < count; i++)
        {
            // the type comes after the amounts it scales
            uint quantity = packet.ReadUInt32();
            uint? maxWeekly = present[i].MaxWeekly ? packet.ReadUInt32() : null;
            uint? tracked = present[i].Tracked ? packet.ReadUInt32() : null;
            uint type = ModernCurrencyId(packet.ReadUInt32());
            uint? weekly = present[i].Weekly ? packet.ReadUInt32() : null;

            uint scale = ModernCurrencyScale(type);
            currencies.Data.Add(new SetupCurrency.Record
            {
                Type = type,
                Quantity = quantity * scale,
                WeeklyQuantity = weekly * scale,
                MaxWeeklyQuantity = maxWeekly * scale,
                TrackedQuantity = tracked * scale,
                // 4.3.4's tracked amount is the season's total, which the 4.4.2 client shows as
                // "Season Total Earned" for Conquest and Valor Points, from this field.
                Unused901 = (int?)(tracked * scale),
                Flags = present[i].Flags,
            });
        }

        SendPacketToClient(currencies);
    }

    /// <summary>One currency's new amount (TrinityCore 4.3.4 <c>SetCurrency::Write</c>).</summary>
    [HandlesSmsg(Opcode.SMSG_UPDATE_CURRENCY)]
    internal void HandleUpdateCurrency(WorldPacket packet)
    {
        bool hasWeekly = packet.HasBit();
        bool hasTracked = packet.HasBit();
        bool suppressChatLog = packet.HasBit();

        uint? tracked = hasTracked ? packet.ReadUInt32() : null;
        uint quantity = packet.ReadUInt32();
        uint type = ModernCurrencyId(packet.ReadUInt32());
        uint? weekly = hasWeekly ? packet.ReadUInt32() : null;

        uint scale = ModernCurrencyScale(type);
        SendPacketToClient(new SetCurrency
        {
            Type = type,
            Quantity = quantity * scale,
            WeeklyQuantity = weekly * scale,
            TrackedQuantity = tracked * scale,
            TotalEarned = tracked * scale,
            SuppressChatLog = suppressChatLog,
        });
    }

    // Archaeology's completed projects. The 4.4.2 client has no packet for them and keeps them in
    // ActivePlayerData.ResearchHistory, which a player Values update carries when it changed.

    /// <summary>
    /// TrinityCore 4.3.4 SetupResearchHistory: 22 bits of count, then per project its id, how often
    /// it was completed and when first (a uint32 unix time). Sent at login.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_SETUP_RESEARCH_HISTORY)]
    internal void HandleSetupResearchHistory(WorldPacket packet)
    {
        uint count = packet.ReadBits<uint>(22);
        var history = GetSession().GameState.ResearchHistory;
        history.Clear();
        for (uint i = 0; i < count && packet.CanRead(); i++)
        {
            uint projectId = packet.ReadUInt32();
            uint completions = packet.ReadUInt32();
            long firstCompleted = packet.ReadUInt32();
            history.Add((projectId, firstCompleted, completions));
        }
        SendResearchHistoryValues();
    }

    /// <summary>
    /// TrinityCore 4.3.4 ResearchComplete, after a project is solved: when it was first completed,
    /// how often, the project.
    /// </summary>
    [HandlesSmsg(Opcode.SMSG_RESEARCH_COMPLETE)]
    internal void HandleResearchComplete(WorldPacket packet)
    {
        long firstCompleted = packet.ReadUInt32();
        uint completions = packet.ReadUInt32();
        uint projectId = packet.ReadUInt32();
        var history = GetSession().GameState.ResearchHistory;
        int index = history.FindIndex(entry => entry.ProjectId == projectId);
        if (index >= 0)
            history[index] = (projectId, firstCompleted, completions);
        else
            history.Add((projectId, firstCompleted, completions));
        SendResearchHistoryValues();
    }

    private void SendResearchHistoryValues()
    {
        var state = GetSession().GameState;
        state.ResearchHistoryDirty = true;
        // At login and after every far teleport the history comes before the player's create,
        // which writes it. A Values update sent ahead of the create is for an object the client
        // does not have yet: it answered CMSG_OBJECT_UPDATE_FAILED for its own player.
        if (!state.ClientHasPlayerObject)
            return;
        ObjectUpdate update = new ObjectUpdate(state.CurrentPlayerGuid, UpdateTypeModern.Values, GetSession());
        update.EnsureActivePlayerData();
        UpdateObject packet = new UpdateObject(state);
        packet.ObjectUpdates.Add(update);
        SendPacketToClient(packet);
    }
}
