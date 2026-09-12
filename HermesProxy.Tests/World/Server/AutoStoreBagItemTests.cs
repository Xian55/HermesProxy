using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// Pins the field order on <c>CMSG_AUTO_STORE_BAG_ITEM</c>: the inventory preamble, then source
/// bag, destination bag, source slot — not the bag/slot/bag/slot pairing the name suggests.
/// </summary>
public class AutoStoreBagItemTests
{
    [Fact]
    public void Read_IsInvThenSourceBagDestBagSourceSlot()
    {
        var payload = new WorldPacket(1u);
        payload.WriteBits(0u, 2);
        payload.FlushBits();
        payload.WriteUInt8(255);
        payload.WriteUInt8(255);
        payload.WriteUInt8(4);

        // Through the production accessor, as the dispatch site builds it.
        var reader = new SpanPacketReader(new WorldPacket(Frame(payload.GetData())).GetRemainingSpan());
        AutoStoreBagItemCodec.Read(ref reader, out var packet);

        Assert.Equal(255, packet.ContainerSlotA);
        Assert.Equal(255, packet.ContainerSlotB);
        Assert.Equal(4, packet.SlotA);
        Assert.Equal(0, packet.Inv.Count);
        Assert.Equal(0, reader.Remaining);
    }

    static byte[] Frame(byte[] body)
    {
        var framed = new byte[body.Length + 2];
        body.CopyTo(framed, 2);
        return framed;
    }
}
