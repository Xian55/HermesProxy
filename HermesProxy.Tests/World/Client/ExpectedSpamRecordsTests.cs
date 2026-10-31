using HermesProxy.Tests.Support;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// CMaNGOS sends its spam_records table at login: a count, then C strings. A modern client reads
/// 9 bits of length, flushed, then the text.
/// </summary>
public sealed class ExpectedSpamRecordsTests
{
    private static byte[] Answer(params string[] records)
    {
        var harness = new LegacyHandlerHarness(recordClientPackets: true);
        byte[] wire = LegacyPacketBuilder.Build(Opcode.SMSG_EXPECTED_SPAM_RECORDS, packet =>
        {
            packet.WriteUInt32((uint)records.Length);
            foreach (string record in records)
                packet.WriteCString(record);
        });
        harness.Deliver(Opcode.SMSG_EXPECTED_SPAM_RECORDS, wire, harness.Client.HandleExpectedSpamRecords);
        return Assert.Single(harness.ClientWire.Sent).Bytes;
    }

    [Fact]
    public void AnEmptyList_IsACountOfNone()
        => Assert.Equal([0, 0, 0, 0], Answer());

    [Fact]
    public void Records_GoOutWithNineBitLengths()
        => Assert.Equal(
            [2, 0, 0, 0, 0x02, 0x00, (byte)'g', (byte)'o', (byte)'l', (byte)'d', 0x01, 0x80, (byte)'w', (byte)'t', (byte)'s'],
            Answer("gold", "wts"));
}
