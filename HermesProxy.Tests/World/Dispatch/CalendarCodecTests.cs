using System;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Dispatch;

/// <summary>
/// The calendar requests of the 3.4.3 client (native 3.4.3 server CalendarPackets.cpp) and the
/// 4.4.2 client (docs/protocol 4.4.2), which differ only in the update's flags. New with
/// their codecs, so there is no frozen <c>Read()</c> to compare with: each case writes the client's
/// layout, and checks the fields and the reader's final position, over a reader built the way
/// production builds it.
/// </summary>
public class CalendarCodecTests
{
    private static byte[] Framed(Action<WorldPacket> write)
    {
        using var w = new WorldPacket(1u);
        write(w);
        byte[] payload = w.GetData();
        byte[] framed = new byte[payload.Length + 2];
        payload.CopyTo(framed, 2);
        return framed;
    }

    private static SpanPacketReader ReaderOver(byte[] framed)
        => new(new WorldPacket(framed).GetRemainingSpan());

    private static readonly WowGuid128 Guid = new(0xDEADBEEFCAFEUL, 0x0123456789ABCDEFUL);

    [Fact]
    public void CalendarGetEvent_ReadsTheEvent()
    {
        var f = Framed(w => w.WriteUInt64(77));
        var r = ReaderOver(f); CalendarGetEventCodec.Read(ref r, out var a);
        Assert.Equal(77ul, a.EventId);
        Assert.Equal(0, r.Remaining);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CalendarAddEvent_ReadsTheEventAndItsInvitations(bool optionals)
    {
        var f = Framed(w =>
        {
            w.WriteUInt64(9);                   // club
            w.WriteUInt8(3);
            w.WriteInt32(-1);
            w.WriteUInt32(0x1A93BD1E);
            w.WriteUInt32(1);
            w.WriteInt32(2);
            w.WriteBits(5, 8);
            w.WriteBits(2, 11);
            w.FlushBits();
            w.WritePackedGuid128(Guid);
            w.WriteUInt8(3);
            w.WriteUInt8(2);
            w.WriteBit(optionals);
            w.WriteBit(optionals);
            w.WriteBit(optionals);
            w.FlushBits();
            if (optionals)
            {
                w.WritePackedGuid128(Guid);
                w.WriteUInt64(1);
                w.WriteUInt64(2);
            }
            w.WritePackedGuid128(WowGuid128.Empty);
            w.WriteUInt8(0);
            w.WriteUInt8(0);
            w.WriteBits(0, 3);
            w.FlushBits();
            w.WriteString("Probe");
            w.WriteString("ab");
            w.WriteUInt32(100);
        });
        var r = ReaderOver(f); CalendarAddEventCodec.Read(ref r, out var a);
        Assert.Equal(3, a.EventType);
        Assert.Equal(-1, a.TextureId);
        Assert.Equal(0x1A93BD1Eu, a.Time);
        Assert.Equal(1u, a.Flags);
        Assert.Equal(2, a.Invites.Length);
        Assert.Equal(new CalendarAddEventInvite(Guid, 3, 2), a.Invites[0]);
        Assert.Equal(WowGuid128.Empty, a.Invites[1].Guid);
        Assert.Equal("Probe", a.Title);
        Assert.Equal("ab", a.Description);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void CalendarAddEvent_ACountThePacketCannotHold_Throws()
    {
        var f = Framed(w =>
        {
            w.WriteUInt64(0); w.WriteUInt8(0); w.WriteInt32(0); w.WriteUInt32(0); w.WriteUInt32(0);
            w.WriteInt32(1_000_000);
            w.WriteBits(0, 19);
            w.FlushBits();
        });
        var r = ReaderOver(f);
        try
        {
            CalendarAddEventCodec.Read(ref r, out _);
            Assert.Fail("a million invitations in a few bytes were taken");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [Fact]
    public void CalendarUpdateEvent_HasThirtyTwoBitsOfFlags()
    {
        var f = Framed(w =>
        {
            w.WriteUInt64(9); w.WriteUInt64(1); w.WriteUInt64(4);
            w.WriteUInt8(3); w.WriteInt32(-1); w.WriteUInt32(0x1A93A54F); w.WriteUInt32(0x41);
            w.WriteBits(3, 8); w.WriteBits(1, 11); w.FlushBits();
            w.WriteString("Two"); w.WriteString("c");
            w.WriteUInt32(100);
        });
        var r = ReaderOver(f); CalendarUpdateEventCodecPreCataClassic.Read(ref r, out var a);
        Assert.Equal(new CalendarUpdateEvent(1, 4, 3, -1, 0x1A93A54F, 0x41, "Two", "c"), a);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void CalendarUpdateEvent_CataClassic_HasSixteenBitsOfFlags()
    {
        var f = Framed(w =>
        {
            w.WriteUInt64(9); w.WriteUInt64(1); w.WriteUInt64(4);
            w.WriteUInt8(3); w.WriteInt32(-1); w.WriteUInt32(0x1A93A54F); w.WriteUInt16(0x41);
            w.WriteBits(3, 8); w.WriteBits(1, 11); w.FlushBits();
            w.WriteString("Two"); w.WriteString("c");
            w.WriteUInt32(100);
        });
        var r = ReaderOver(f); CalendarUpdateEventCodecCataClassic.Read(ref r, out var a);
        Assert.Equal(new CalendarUpdateEvent(1, 4, 3, -1, 0x1A93A54F, 0x41, "Two", "c"), a);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void CalendarRemoveAndCopy_ReadPastTheClub()
    {
        var remove = Framed(w => { w.WriteUInt64(1); w.WriteUInt64(4); w.WriteUInt64(9); w.WriteUInt32(0x40); });
        var r = ReaderOver(remove); CalendarRemoveEventCodec.Read(ref r, out var a);
        Assert.Equal(new CalendarRemoveEvent(1, 4, 0x40), a);
        Assert.Equal(0, r.Remaining);

        var copy = Framed(w => { w.WriteUInt64(1); w.WriteUInt64(4); w.WriteUInt64(9); w.WriteUInt32(0x1A93A54F); });
        r = ReaderOver(copy); CalendarCopyEventCodec.Read(ref r, out var b);
        Assert.Equal(new CalendarCopyEvent(1, 4, 0x1A93A54F), b);
        Assert.Equal(0, r.Remaining);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void CalendarInvite_ReadsTheNameAfterTwoBits(bool creating, bool signUp)
    {
        var f = Framed(w =>
        {
            w.WriteUInt64(1); w.WriteUInt64(4); w.WriteUInt64(9);
            w.WriteBits(7, 9); w.WriteBit(signUp); w.WriteBit(creating); w.FlushBits();
            w.WriteString("Warriok");
        });
        var r = ReaderOver(f); CalendarInviteCodec.Read(ref r, out var a);
        Assert.Equal(new CalendarInvite(1, 4, "Warriok", creating, signUp), a);
        Assert.Equal(0, r.Remaining);
    }

    [Theory]
    // an invitation to event 1, a guild event (club 16), that exists
    [InlineData("0100000000000000010000000000000010000000000000000440476165746861616E", 1ul, false, true)]
    // one in a player event still being made (event 0; the club is whatever the client had there)
    [InlineData("000000000000000001000000000000000000C0D2C0C3CF410420476165746861616E", 0ul, true, false)]
    public void CalendarInvite_AsTheClientSentIt(string hex, ulong eventId, bool creating, bool signUp)
    {
        byte[] framed = [0, 0, .. Convert.FromHexString(hex)];
        var r = ReaderOver(framed); CalendarInviteCodec.Read(ref r, out var a);
        Assert.Equal(new CalendarInvite(eventId, 1, "Gaethaan", creating, signUp), a);
        Assert.Equal(0, r.Remaining);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CalendarEventSignUp_ReadsTheBit(bool tentative)
    {
        var f = Framed(w => { w.WriteUInt64(1); w.WriteUInt64(9); w.WriteBit(tentative); w.FlushBits(); });
        var r = ReaderOver(f); CalendarEventSignUpCodec.Read(ref r, out var a);
        Assert.Equal(new CalendarEventSignUp(1, tentative), a);
        Assert.Equal(0, r.Remaining);
    }

    [Fact]
    public void CalendarRsvp_RemoveInvite_Status_Complain_CommunityInvite()
    {
        var r = ReaderOver(Framed(w => { w.WriteUInt64(1); w.WriteUInt64(2); w.WriteUInt8(8); }));
        CalendarRsvpCodec.Read(ref r, out var rsvp);
        Assert.Equal(new CalendarRsvp(1, 2, 8), rsvp);
        Assert.Equal(0, r.Remaining);

        r = ReaderOver(Framed(w => { w.WritePackedGuid128(Guid); w.WriteUInt64(2); w.WriteUInt64(4); w.WriteUInt64(1); }));
        CalendarRemoveInviteCodec.Read(ref r, out var remove);
        Assert.Equal(new CalendarRemoveInvite(Guid, 2, 4, 1), remove);
        Assert.Equal(0, r.Remaining);

        r = ReaderOver(Framed(w => { w.WritePackedGuid128(Guid); w.WriteUInt64(1); w.WriteUInt64(2); w.WriteUInt64(4); w.WriteUInt8(3); }));
        CalendarInviteeStatusCodec.Read(ref r, out var status);
        Assert.Equal(new CalendarInviteeStatus(Guid, 1, 2, 4, 3), status);
        Assert.Equal(0, r.Remaining);

        r = ReaderOver(Framed(w => { w.WritePackedGuid128(Guid); w.WriteUInt64(1); w.WriteUInt64(2); }));
        CalendarComplainCodec.Read(ref r, out var complain);
        Assert.Equal(new CalendarComplain(Guid, 1, 2), complain);
        Assert.Equal(0, r.Remaining);

        r = ReaderOver(Framed(w => { w.WriteUInt64(9); w.WriteUInt8(1); w.WriteUInt8(80); w.WriteUInt8(10); }));
        CalendarCommunityInviteRequestCodec.Read(ref r, out var community);
        Assert.Equal(new CalendarCommunityInviteRequest(9, 1, 80, 10), community);
        Assert.Equal(0, r.Remaining);
    }
}
