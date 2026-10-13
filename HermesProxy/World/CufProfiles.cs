using System;
using System.Collections.Generic;
using System.Text;
using Framework.IO;

namespace HermesProxy.World;

/// <summary>
/// The on/off options of a raid frame profile, in the order of the 4.4.2 client's bits. The order
/// is the client's own: it was made to save a profile with one option set at a time, and the bit
/// that moved was read off. It is not TrinityCore's list for the later clients, which has two
/// options ahead of these that this client does not have.
/// </summary>
public enum CufOption : byte
{
    KeepGroupsTogether = 0,
    DisplayPets = 1,
    DisplayMainTankAndAssist = 2,
    DisplayOnlyDispellableDebuffs = 3,
    DisplayPowerBar = 4,
    DisplayBorder = 5,
    UseClassColors = 6,
    HorizontalGroups = 7,
    DisplayNonBossDebuffs = 8,
    DynamicPosition = 9,
    Locked = 10,
    Shown = 11,
    AutoActivate2Players = 12,
    AutoActivate3Players = 13,
    AutoActivate5Players = 14,
    AutoActivate10Players = 15,
    AutoActivate15Players = 16,
    /// <summary>The sixth group size: 20 players to the 4.4.2 client, 25 to a 4.3.4 server.</summary>
    AutoActivate20Players = 17,
    AutoActivate40Players = 18,
    AutoActivatePvP = 19,
    AutoActivatePvE = 20,
}

/// <summary>
/// The options a 4.3.4 profile has and the 4.4.2 client has no bit for. They cannot be shown or
/// changed there; they are kept as the server has them, so a profile made on a 4.3.4 client does
/// not lose them by being saved through this one.
/// </summary>
[Flags]
public enum CufLegacyOnlyOptions : byte
{
    None = 0,
    DisplayHealPrediction = 1,
    DisplayAggroHighlight = 2,
    AutoActivateSpec1 = 4,
    AutoActivateSpec2 = 8,
}

/// <summary>One raid frame profile, as both sides have it.</summary>
internal sealed class CufProfile
{
    public string Name = "";
    /// <summary>Bit i is <see cref="CufOption"/> i.</summary>
    public uint Options;
    public CufLegacyOnlyOptions LegacyOnly;
    public ushort FrameHeight;
    public ushort FrameWidth;
    public byte SortBy;         // 0 role, 1 group, 2 alphabetical
    public byte HealthText;     // 0 none, 1 remaining, 2 lost, 3 percent
    public byte TopPoint;
    public byte BottomPoint;
    public byte LeftPoint;
    public ushort TopOffset;
    public ushort BottomOffset;
    public ushort LeftOffset;

    public bool Has(CufOption option) => (Options & (1u << (int)option)) != 0;
}

/// <summary>
/// Raid frame profiles in their two shapes. The 4.4.2 client sends and takes the same blob: a
/// count, then for each profile 7 bits of name length and 25 option bits, ten numbers and the
/// name. A 4.3.4 server packs the bits of all profiles first, in an order of its own that differs
/// between the packet it reads and the one it writes, and the numbers after.
/// </summary>
internal static class CufProfiles
{
    /// <summary>More than any client keeps (five); a bigger count is a misread.</summary>
    public const int MaxProfiles = 16;

    private const int ModernOptionBits = 25;
    private const int ModernFixedBytes = 4 + 2 + 2 + 5 + 6;

    // An entry of the 4.3.4 bit orders below: an option both sides have, one only 4.3.4 has, or
    // the place of the 8-bit name length among them.
    private const byte LegacyOnly = 0x80;
    private const byte NameLength = 0xFF;
    private const byte HealPrediction = LegacyOnly | 0;
    private const byte AggroHighlight = LegacyOnly | 1;
    private const byte Spec1 = LegacyOnly | 2;
    private const byte Spec2 = LegacyOnly | 3;

    // TrinityCore names three of its bits by their offset only (145, 156, 157) and says they are
    // "locked", "shown" and "dynamic" in an order it does not know. 145 sits right before the
    // saved position in the client's structure and is taken for the dynamic position here, 156
    // and 157 for locked and shown, the order the 4.4.2 client has them in. A profile goes to the
    // server and comes back the same whichever is which; only a 4.3.4 client reading it could tell.

    /// <summary>TrinityCore 4.3.4 <c>LoadCUFProfiles::Write</c>.</summary>
    private static ReadOnlySpan<byte> LegacyLoadBits =>
    [
        (byte)CufOption.Shown, (byte)CufOption.AutoActivate10Players, (byte)CufOption.AutoActivate5Players,
        (byte)CufOption.AutoActivate20Players, HealPrediction, (byte)CufOption.AutoActivatePvE,
        (byte)CufOption.HorizontalGroups, (byte)CufOption.AutoActivate40Players, (byte)CufOption.AutoActivate3Players,
        AggroHighlight, (byte)CufOption.DisplayBorder, (byte)CufOption.AutoActivate2Players,
        (byte)CufOption.DisplayNonBossDebuffs, (byte)CufOption.DisplayMainTankAndAssist, (byte)CufOption.Locked,
        Spec2, (byte)CufOption.UseClassColors, (byte)CufOption.DisplayPowerBar, Spec1,
        NameLength,
        (byte)CufOption.DisplayOnlyDispellableDebuffs, (byte)CufOption.KeepGroupsTogether, (byte)CufOption.DynamicPosition,
        (byte)CufOption.AutoActivate15Players, (byte)CufOption.DisplayPets, (byte)CufOption.AutoActivatePvP,
    ];

    /// <summary>TrinityCore 4.3.4 <c>HandleSaveCUFProfiles</c>.</summary>
    private static ReadOnlySpan<byte> LegacySaveBits =>
    [
        Spec2, (byte)CufOption.AutoActivate10Players, (byte)CufOption.Shown, HealPrediction, Spec1,
        (byte)CufOption.AutoActivatePvP, (byte)CufOption.DisplayPowerBar, (byte)CufOption.AutoActivate15Players,
        (byte)CufOption.AutoActivate40Players, (byte)CufOption.DisplayPets, (byte)CufOption.AutoActivate5Players,
        (byte)CufOption.DisplayOnlyDispellableDebuffs, (byte)CufOption.AutoActivate2Players, (byte)CufOption.Locked,
        (byte)CufOption.DisplayNonBossDebuffs, (byte)CufOption.DisplayMainTankAndAssist, AggroHighlight,
        (byte)CufOption.AutoActivate3Players, (byte)CufOption.DisplayBorder, (byte)CufOption.UseClassColors,
        (byte)CufOption.DynamicPosition,
        NameLength,
        (byte)CufOption.AutoActivatePvE, (byte)CufOption.HorizontalGroups, (byte)CufOption.AutoActivate20Players,
        (byte)CufOption.KeepGroupsTogether,
    ];

    /// <summary>The blob of the 4.4.2 client, as it sends it to be saved and takes it at login.</summary>
    public static bool TryReadModern(ReadOnlySpan<byte> blob, List<CufProfile> profiles)
    {
        if (blob.Length < 4)
            return false;

        var reader = new SpanPacketReader(blob);
        uint count = reader.ReadUInt32();
        if (count > MaxProfiles)
            return false;

        for (uint i = 0; i < count; i++)
        {
            if (reader.Remaining < ModernFixedBytes)
                return false;

            CufProfile profile = new();
            int nameLength = (int)reader.ReadBits<uint>(7);
            uint options = reader.ReadBits<uint>(ModernOptionBits);
            // the first option is the highest of the 25 bits as they arrive; the four the client
            // has no option for are the lowest and are let go
            for (int option = 0; option <= (int)CufOption.AutoActivatePvE; option++)
            {
                if ((options & (1u << (ModernOptionBits - 1 - option))) != 0)
                    profile.Options |= 1u << option;
            }
            reader.ResetBitReader();

            profile.FrameHeight = reader.ReadUInt16();
            profile.FrameWidth = reader.ReadUInt16();
            profile.SortBy = reader.ReadUInt8();
            profile.HealthText = reader.ReadUInt8();
            profile.TopPoint = reader.ReadUInt8();
            profile.BottomPoint = reader.ReadUInt8();
            profile.LeftPoint = reader.ReadUInt8();
            profile.TopOffset = reader.ReadUInt16();
            profile.BottomOffset = reader.ReadUInt16();
            profile.LeftOffset = reader.ReadUInt16();

            if (reader.Remaining < nameLength)
                return false;
            profile.Name = reader.ReadString(nameLength);
            profiles.Add(profile);
        }

        return reader.Remaining == 0;
    }

    public static byte[] WriteModern(List<CufProfile> profiles)
    {
        using ByteBuffer blob = new();
        blob.WriteUInt32((uint)profiles.Count);
        foreach (CufProfile profile in profiles)
        {
            // the name length has seven bits; a 4.3.4 name can be longer than that holds
            string name = profile.Name;
            while (Encoding.UTF8.GetByteCount(name) > 127)
                name = name[..^1];

            blob.WriteBits((uint)Encoding.UTF8.GetByteCount(name), 7);
            for (int option = 0; option < ModernOptionBits; option++)
                blob.WriteBit(option <= (int)CufOption.AutoActivatePvE && (profile.Options & (1u << option)) != 0);
            blob.FlushBits();

            blob.WriteUInt16(profile.FrameHeight);
            blob.WriteUInt16(profile.FrameWidth);
            blob.WriteUInt8(profile.SortBy);
            blob.WriteUInt8(profile.HealthText);
            blob.WriteUInt8(profile.TopPoint);
            blob.WriteUInt8(profile.BottomPoint);
            blob.WriteUInt8(profile.LeftPoint);
            blob.WriteUInt16(profile.TopOffset);
            blob.WriteUInt16(profile.BottomOffset);
            blob.WriteUInt16(profile.LeftOffset);
            blob.WriteString(name);
        }
        return blob.GetData();
    }

    /// <summary>SMSG_LOAD_CUF_PROFILES of a 4.3.4 server.</summary>
    public static bool TryReadLegacy(WorldPacket packet, List<CufProfile> profiles)
    {
        if (!packet.CanRead(3))
            return false;

        int count = (int)packet.ReadBits<uint>(20);
        // 20 bits of count, then 25 options and 8 bits of name length a profile
        if (count > MaxProfiles || !packet.CanRead((20 + count * 33 + 7) / 8 - 3))
            return false;

        Span<int> nameLengths = stackalloc int[MaxProfiles];
        for (int i = 0; i < count; i++)
        {
            CufProfile profile = new();
            foreach (byte entry in LegacyLoadBits)
            {
                if (entry == NameLength)
                    nameLengths[i] = (int)packet.ReadBits<uint>(8);
                else if (packet.HasBit())
                    Set(profile, entry);
            }
            profiles.Add(profile);
        }
        packet.ResetBitPos();

        for (int i = 0; i < count; i++)
        {
            if (!packet.CanRead(15 + nameLengths[i]))
                return false;

            CufProfile profile = profiles[i];
            profile.LeftOffset = packet.ReadUInt16();
            profile.FrameHeight = packet.ReadUInt16();
            profile.BottomOffset = packet.ReadUInt16();
            profile.BottomPoint = packet.ReadUInt8();
            profile.TopOffset = packet.ReadUInt16();
            profile.TopPoint = packet.ReadUInt8();
            profile.HealthText = packet.ReadUInt8();
            profile.SortBy = packet.ReadUInt8();
            profile.FrameWidth = packet.ReadUInt16();
            profile.LeftPoint = packet.ReadUInt8();
            profile.Name = packet.ReadString((uint)nameLengths[i]);
        }
        return true;
    }

    /// <summary>CMSG_SAVE_CUF_PROFILES for a 4.3.4 server.</summary>
    public static void WriteLegacy(WorldPacket packet, List<CufProfile> profiles)
    {
        packet.WriteBits((uint)profiles.Count, 20);
        foreach (CufProfile profile in profiles)
        {
            foreach (byte entry in LegacySaveBits)
            {
                if (entry == NameLength)
                    packet.WriteBits((uint)Encoding.UTF8.GetByteCount(profile.Name), 8);
                else
                    packet.WriteBit(IsSet(profile, entry));
            }
        }
        packet.FlushBits();

        foreach (CufProfile profile in profiles)
        {
            packet.WriteUInt8(profile.TopPoint);
            packet.WriteString(profile.Name);
            packet.WriteUInt16(profile.BottomOffset);
            packet.WriteUInt16(profile.FrameHeight);
            packet.WriteUInt16(profile.FrameWidth);
            packet.WriteUInt16(profile.TopOffset);
            packet.WriteUInt8(profile.HealthText);
            packet.WriteUInt8(profile.BottomPoint);
            packet.WriteUInt8(profile.SortBy);
            packet.WriteUInt16(profile.LeftOffset);
            packet.WriteUInt8(profile.LeftPoint);
        }
    }

    private static void Set(CufProfile profile, byte entry)
    {
        if ((entry & LegacyOnly) != 0)
            profile.LegacyOnly |= (CufLegacyOnlyOptions)(1 << (entry & 0x7F));
        else
            profile.Options |= 1u << entry;
    }

    private static bool IsSet(CufProfile profile, byte entry)
        => (entry & LegacyOnly) != 0
            ? (profile.LegacyOnly & (CufLegacyOnlyOptions)(1 << (entry & 0x7F))) != 0
            : (profile.Options & (1u << entry)) != 0;
}
