using System;
using System.Buffers;
using System.Reflection;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// <see cref="SetupCurrency"/> has one layout up to 3.4.3 and one from 4.4.2 on, where two more
/// presence bits sit between the first five and the flags. The older layout must still give the
/// bytes the class gave before it had layouts.
/// </summary>
public sealed class CurrencyLayoutTests
{
    [Fact]
    public void Layouts_ByBuild()
    {
        Assert.IsType<SetupCurrency.FiveOptionalsLayout>(SetupCurrency.Layouts.For(ClientVersionBuild.V1_14_2_42597));
        Assert.IsType<SetupCurrency.FiveOptionalsLayout>(SetupCurrency.Layouts.For(ClientVersionBuild.V2_5_3_41750));
        Assert.IsType<SetupCurrency.FiveOptionalsLayout>(SetupCurrency.Layouts.For(ClientVersionBuild.V3_4_3_54261));
        Assert.IsType<SetupCurrency.SevenOptionalsLayout>(SetupCurrency.Layouts.For(ClientVersionBuild.V4_4_2_60895));
    }

    [Fact]
    public void FiveOptionalsLayout_MatchesTheClassBeforeTheLayouts()
    {
        var packet = Sample();
        AssertBothSerialisers(new SetupCurrency.FiveOptionalsLayout(), packet, OldWrite(packet));
    }

    /// <summary>
    /// Honor with a ceiling and flags 3: the fourth presence bit, two clear bits for the recharge
    /// times, then the flags, which so start at the eighth bit and not at the sixth.
    /// </summary>
    [Fact]
    public void SevenOptionalsLayout_PutsTheFlagsAfterSevenBits()
    {
        var packet = new SetupCurrency();
        packet.Data.Add(new SetupCurrency.Record { Type = 392, Quantity = 19, MaxQuantity = 4000, Flags = 3 });

        byte[] expected =
        [
            0x01, 0x00, 0x00, 0x00,     // one currency
            0x88, 0x01, 0x00, 0x00,     // 392
            0x13, 0x00, 0x00, 0x00,     // 19
            0b0001_0000, 0b0011_0000,   // bit 4 of 7: MaxQuantity; flags 00011 in the next five
            0xA0, 0x0F, 0x00, 0x00,     // 4000
        ];
        AssertBothSerialisers(new SetupCurrency.SevenOptionalsLayout(), packet, expected);
    }

    [Fact]
    public void SevenOptionalsLayout_WithoutFlags_IsTheOlderLayoutsBytes()
    {
        var packet = Sample(flags: 0);
        AssertBothSerialisers(new SetupCurrency.SevenOptionalsLayout(), packet, OldWrite(packet));
    }

    /// <summary>The honor gain of the reader tests, as the 4.4.2 client is given it.</summary>
    [Fact]
    public void SetCurrency_WritesTwelveBitsAfterTheFourInts()
    {
        var packet = new SetCurrency { Type = 1901, Quantity = 19, TrackedQuantity = 19, TotalEarned = 19 };
        packet.Write();

        byte[] expected =
        [
            0x6D, 0x07, 0x00, 0x00,     // 1901
            0x13, 0x00, 0x00, 0x00,     // 19
            0x00, 0x00, 0x00, 0x00,     // flags
            0x00, 0x00, 0x00, 0x00,     // no toasts
            0b0101_0000, 0x00,          // second and fourth of twelve bits: TrackedQuantity, TotalEarned
            0x13, 0x00, 0x00, 0x00,     // 19 tracked
            0x13, 0x00, 0x00, 0x00,     // 19 earned in all
        ];
        Assert.Equal(expected, ((WorldPacket)WorldPacketField.GetValue(packet)!).GetData());
    }

    [Fact]
    public void SevenOptionalsLayout_TakesACataclysmCharactersFortyCurrenciesWithoutTheSlowPath()
    {
        var layout = new SetupCurrency.SevenOptionalsLayout();
        var packet = new SetupCurrency();
        for (uint id = 1; id <= 40; id++)
            packet.Data.Add(new SetupCurrency.Record { Type = id, Quantity = 700, WeeklyQuantity = 1, MaxWeeklyQuantity = 2, TrackedQuantity = 3, MaxQuantity = 4, Unused901 = 5 });

        using var data = new WorldPacket(1u);
        layout.Write(packet, data);
        byte[] buffer = new byte[layout.MaxSize];

        int written = layout.WriteToSpan(packet, buffer);

        Assert.Equal(data.GetDataSpan().ToArray(), buffer.AsSpan(0, written).ToArray());
    }

    [Fact]
    public void FiveOptionalsLayout_StillSendsALongerListThanWrathHasByTheSlowPath()
    {
        var packet = new SetupCurrency();
        for (uint id = 1; id <= 25; id++)
            packet.Data.Add(new SetupCurrency.Record { Type = id });

        var layout = new SetupCurrency.FiveOptionalsLayout();

        Assert.Equal(-1, layout.WriteToSpan(packet, new byte[layout.MaxSize]));
    }

    private static readonly PropertyInfo WorldPacketField =
        typeof(ServerPacket).GetProperty("_worldPacket", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static SetupCurrency Sample(byte flags = 0x15)
    {
        var packet = new SetupCurrency();
        packet.Data.Add(new SetupCurrency.Record { Type = 1901, Quantity = 1200, MaxQuantity = 75000, Flags = flags });
        packet.Data.Add(new SetupCurrency.Record { Type = 1900, Quantity = 0, WeeklyQuantity = 3, MaxWeeklyQuantity = 5000, Flags = flags });
        packet.Data.Add(new SetupCurrency.Record { Type = 101, Quantity = 7, TrackedQuantity = 9, Unused901 = 11 });
        return packet;
    }

    private static void AssertBothSerialisers(ServerPacketLayout<SetupCurrency> layout, SetupCurrency packet, byte[] expected)
    {
        using var data = new WorldPacket(1u);
        layout.Write(packet, data);
        Assert.Equal(expected, data.GetDataSpan().ToArray());

        byte[] buffer = ArrayPool<byte>.Shared.Rent(layout.MaxSize);
        try
        {
            int written = layout.WriteToSpan(packet, buffer);
            Assert.Equal(expected, buffer.AsSpan(0, written).ToArray());
            Assert.True(written <= layout.MaxSize);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    // SetupCurrency.Write as it stood before the layouts, frozen.
    private static byte[] OldWrite(SetupCurrency p)
    {
        using var data = new WorldPacket(1u);
        data.WriteInt32(p.Data.Count);
        foreach (SetupCurrency.Record record in p.Data)
        {
            data.WriteUInt32(record.Type);
            data.WriteUInt32(record.Quantity);

            data.WriteBit(record.WeeklyQuantity.HasValue);
            data.WriteBit(record.MaxWeeklyQuantity.HasValue);
            data.WriteBit(record.TrackedQuantity.HasValue);
            data.WriteBit(record.MaxQuantity.HasValue);
            data.WriteBit(record.Unused901.HasValue);
            data.WriteBits(record.Flags, 5);
            data.FlushBits();

            if (record.WeeklyQuantity.HasValue)
                data.WriteUInt32(record.WeeklyQuantity.Value);
            if (record.MaxWeeklyQuantity.HasValue)
                data.WriteUInt32(record.MaxWeeklyQuantity.Value);
            if (record.TrackedQuantity.HasValue)
                data.WriteUInt32(record.TrackedQuantity.Value);
            if (record.MaxQuantity.HasValue)
                data.WriteInt32(record.MaxQuantity.Value);
            if (record.Unused901.HasValue)
                data.WriteInt32(record.Unused901.Value);
        }
        return data.GetDataSpan().ToArray();
    }
}
