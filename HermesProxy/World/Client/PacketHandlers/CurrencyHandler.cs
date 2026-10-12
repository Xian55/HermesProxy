using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
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
}
