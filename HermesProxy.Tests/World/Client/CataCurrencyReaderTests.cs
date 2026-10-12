using System;
using HermesProxy.Tests.Support;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// The 4.3.4 currency readers against packets two 4.3.4 servers sent: the list at login from
/// each, an honor gain after a kill and a gain of Tol Barad Commendations. 390 is Conquest Points,
/// 395 Justice Points and 392 4.3.4's Honor Points, which the 4.4.2 client knows as 1901: all kept
/// in hundredths by the 4.4.2 client and in whole points on a 4.3.4 wire, so they leave here a
/// hundred times what came in. 391, the commendations, passes as it is.
/// </summary>
public sealed class CataCurrencyReaderTests
{
    private const string ListOfThree =
        "00 00 06 00 08 30 00 00 00 00 8b 01 00 00 00 00 00 00 00 00 00 00 88 01 00 00 00 00 00 00 " +
        "72 06 00 00 00 00 00 00 86 01 00 00";
    private const string ListOfTwo =
        "00 00 04 0a 00 00 00 00 00 46 05 00 00 86 01 00 00 01 00 00 00 88 01 00 00 01 00 00 00";
    private const string HonorGain = "40 13 00 00 00 13 00 00 00 88 01 00 00";
    private const string CommendationGain = "00 1e 00 00 00 87 01 00 00";

    private readonly LegacyHandlerHarness _harness = new(recordClientPackets: true);

    public CataCurrencyReaderTests() => _harness.SetActivePlayer(new WowGuid64(2));

    // a received packet starts with its opcode, which the handlers never look at
    private static byte[] Wire(string hex) => [0, 0, .. Convert.FromHexString(hex.Replace(" ", ""))];

    private T Deliver<T>(Opcode opcode, string hex, Action<WorldPacket> handler) where T : ServerPacket
    {
        _harness.Deliver(opcode, Wire(hex), handler);
        return Assert.IsType<T>(_harness.ClientWire.Sent[^1].Packet);
    }

    [Fact]
    public void HandleSetupCurrency_ListOfThree_KeepsEachCurrencysOptionalFields()
    {
        var list = Deliver<SetupCurrency>(Opcode.SMSG_SETUP_CURRENCY, ListOfThree, _harness.Client.HandleSetupCurrency);

        Assert.Equal(3, list.Data.Count);

        Assert.Equal(395u, list.Data[0].Type);
        Assert.Equal(0u, list.Data[0].Quantity);
        Assert.Null(list.Data[0].WeeklyQuantity);
        Assert.Null(list.Data[0].MaxWeeklyQuantity);
        Assert.Null(list.Data[0].TrackedQuantity);

        Assert.Equal(1901u, list.Data[1].Type);
        Assert.Equal(0u, list.Data[1].TrackedQuantity);
        Assert.Equal(0, list.Data[1].Unused901);
        Assert.Null(list.Data[1].MaxQuantity);
        Assert.Null(list.Data[1].MaxWeeklyQuantity);

        Assert.Equal(390u, list.Data[2].Type);
        Assert.Equal(165_000u, list.Data[2].MaxWeeklyQuantity);
        Assert.Equal(0u, list.Data[2].TrackedQuantity);
        Assert.Null(list.Data[2].MaxQuantity);
        Assert.Null(list.Data[2].WeeklyQuantity);
    }

    [Fact]
    public void HandleSetupCurrency_ListOfTwo_ReadsTheWeeklyAmountAfterTheType()
    {
        var list = Deliver<SetupCurrency>(Opcode.SMSG_SETUP_CURRENCY, ListOfTwo, _harness.Client.HandleSetupCurrency);

        Assert.Equal(2, list.Data.Count);

        Assert.Equal(390u, list.Data[0].Type);
        Assert.Equal(0u, list.Data[0].Quantity);
        Assert.Equal(135_000u, list.Data[0].MaxWeeklyQuantity);
        Assert.Null(list.Data[0].WeeklyQuantity);

        Assert.Equal(1901u, list.Data[1].Type);
        Assert.Equal(100u, list.Data[1].Quantity);
        Assert.Equal(100u, list.Data[1].WeeklyQuantity);
        Assert.Null(list.Data[1].Unused901);
        Assert.Null(list.Data[1].MaxWeeklyQuantity);
        Assert.Equal(0, list.Data[1].Flags);
    }

    [Fact]
    public void HandleUpdateCurrency_HonorGain_CarriesTheNewTotalAndSaysSoInChat()
    {
        var gain = Deliver<SetCurrency>(Opcode.SMSG_UPDATE_CURRENCY, HonorGain, _harness.Client.HandleUpdateCurrency);

        Assert.Equal(1901u, gain.Type);
        Assert.Equal(1900u, gain.Quantity);
        Assert.Equal(1900u, gain.TrackedQuantity);
        Assert.Equal(1900u, gain.TotalEarned);
        Assert.Null(gain.WeeklyQuantity);
        Assert.False(gain.SuppressChatLog);
    }

    [Fact]
    public void HandleUpdateCurrency_OfACurrencyInWholeUnits_IsNotScaled()
    {
        var gain = Deliver<SetCurrency>(Opcode.SMSG_UPDATE_CURRENCY, CommendationGain, _harness.Client.HandleUpdateCurrency);

        Assert.Equal(391u, gain.Type);
        Assert.Equal(30u, gain.Quantity);
        Assert.Null(gain.TrackedQuantity);
        Assert.Null(gain.WeeklyQuantity);
    }
}
