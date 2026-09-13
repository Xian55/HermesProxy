using System;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

public class EquipmentSetPacketsTests
{
    [Fact]
    public void LoadEquipmentSet_Write_Empty_IsCountZero()
    {
        var load = new LoadEquipmentSet();
        load.WritePacketData();

        Assert.Equal(new byte[] { 0, 0, 0, 0 }, load.GetData());
    }

    [Fact]
    public void EquipmentSetID_Write_IsGuidThenTypeThenSetId()
    {
        var id = new EquipmentSetID
        {
            GUID = 0x1122334455667788ul,
            Type = 0,
            SetID = 3,
        };
        id.WritePacketData();

        using var reader = new WorldPacket(Frame(id.GetData()!));
        Assert.Equal(0x1122334455667788ul, reader.ReadUInt64());
        Assert.Equal(0, reader.ReadInt32());
        Assert.Equal(3u, reader.ReadUInt32());
    }

    [Fact]
    public void UseEquipmentSetResult_Write_IsGuidThenReason()
    {
        var result = new UseEquipmentSetResult { GUID = 99, Reason = 4 };
        result.WritePacketData();

        using var reader = new WorldPacket(Frame(result.GetData()!));
        Assert.Equal(99ul, reader.ReadUInt64());
        Assert.Equal(4, reader.ReadUInt8());
    }

    [Fact]
    public void EquipmentSetData_WriteThenRead_RoundTripsNameAndIgnoreMask()
    {
        var original = new EquipmentSetData
        {
            Type = 0,
            Guid = 7,
            SetID = 1,
            IgnoreMask = 1u << 3,
            SetName = "Tank",
            SetIcon = "INV_Helmet_01",
        };
        original.Pieces[0] = new WowGuid128(42, 0);
        original.Pieces[3] = EquipmentSetModern.IgnoredSlot;

        var buffer = new WorldPacket(1u);
        original.Write(buffer);

        var copy = new EquipmentSetData();
        using var reader = new WorldPacket(Frame(buffer.GetData()));
        copy.Read(reader);

        Assert.Equal(original.Guid, copy.Guid);
        Assert.Equal(original.SetID, copy.SetID);
        Assert.Equal(original.IgnoreMask, copy.IgnoreMask);
        Assert.Equal(original.SetName, copy.SetName);
        Assert.Equal(original.SetIcon, copy.SetIcon);
        Assert.Equal(original.Pieces[0], copy.Pieces[0]);
        Assert.Equal(EquipmentSetModern.IgnoredSlot, copy.Pieces[3]);
    }

    [Fact]
    public void UseEquipmentSet_Read_SkipsInvUpdateThenReadsSlotsAndGuid()
    {
        var payload = new WorldPacket(1u);
        payload.WriteBits(0u, 2);
        payload.FlushBits();
        for (int i = 0; i < LoadEquipmentSet.SlotCount; i++)
        {
            payload.WritePackedGuid128(i == 2 ? EquipmentSetModern.IgnoredSlot : WowGuid128.Empty);
            payload.WriteUInt8(255);
            payload.WriteUInt8((byte)i);
        }
        payload.WriteUInt64(55);

        var reader2 = new SpanPacketReader(new WorldPacket(Frame(payload.GetData())).GetRemainingSpan());
        UseEquipmentSetCodec.Read(ref reader2, out var packet);

        Assert.Equal(55ul, packet.GUID);
        var items = packet.Items;
        Assert.Equal(LoadEquipmentSet.SlotCount, ((ReadOnlySpan<EquipmentSetItem>)items).Length);
        Assert.Equal(EquipmentSetModern.IgnoredSlot, packet.Items[2].Item);
        Assert.Equal(2, packet.Items[2].Slot);
    }

    static byte[] Frame(byte[] body)
    {
        var framed = new byte[body.Length + 2];
        body.CopyTo(framed, 2);
        return framed;
    }
}
