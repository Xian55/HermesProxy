using System;
using System.Reflection;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// The 3.4.3 client reads each SMSG_SPELL_PERIODIC_AURA_LOG effect as seven values, a u32 supporter count
/// and its entries, then the flag byte. Without the count it read the flags past the end of the packet,
/// and every tick lost its crit flag (#365).
/// </summary>
[Collection("V343ValuesFilter")]
public class SpellPeriodicAuraLogTests
{
    private static readonly PropertyInfo WorldPacketField =
        typeof(ServerPacket).GetProperty("_worldPacket", BindingFlags.NonPublic | BindingFlags.Instance)!;

    // Captured from a 3.4.3 session on AzerothCore playerbots: a Blood Plague (55078) tick of 1109.
    private const string Head = "01A77A80A5230420" + "03A0BF020408" + "26D70000" + "01000000" + "00";
    private const string Values = "03000000" + "55040000" + "55040000" + "00000000" + "20000000" + "00000000" + "00000000";

    private static SpellPeriodicAuraLog Packet(bool crit = false) => new()
    {
        TargetGUID = new WowGuid128(0x7A, 0x200004000023A580),
        CasterGUID = new WowGuid128(0x02BF, 0x0800040000000000),
        SpellID = 55078,
        Effects =
        [
            new SpellPeriodicAuraLog.SpellLogEffect
            {
                Effect = 3,
                Amount = 1109,
                OriginalDamage = 1109,
                SchoolMaskOrPower = 32,
                Crit = crit,
            },
        ],
    };

    private static string ViaWrite(SpellPeriodicAuraLog packet)
    {
        packet.Write();
        return Convert.ToHexString(((WorldPacket)WorldPacketField.GetValue(packet)!).GetData());
    }

    [Theory]
    [InlineData(false, "00")]
    [InlineData(true, "80")]
    public void Write_OnV343_PutsTheSupporterCountBeforeTheFlagByte(bool crit, string flags)
    {
        SpellPeriodicAuraLog.ForceV343ForTests = true;
        try
        {
            Assert.Equal(Head + Values + "00000000" + flags, ViaWrite(Packet(crit)));
        }
        finally
        {
            SpellPeriodicAuraLog.ForceV343ForTests = null;
        }
    }

    [Fact]
    public void Write_BeforeV343_MatchesTheCapture()
    {
        SpellPeriodicAuraLog.ForceV343ForTests = false;
        try
        {
            Assert.Equal(Head + Values + "00", ViaWrite(Packet()));
        }
        finally
        {
            SpellPeriodicAuraLog.ForceV343ForTests = null;
        }
    }
}
