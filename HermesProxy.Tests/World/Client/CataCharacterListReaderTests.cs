using System;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// The 4.3.4 SMSG_ENUM_CHARACTERS_RESULT reader against a writer ported line for line from
/// TrinityCore 4.3.4's EnumCharactersResult::Write: masked GUIDs, the name length among the bits,
/// and the GUID bytes interleaved with the fields, each non-zero one XORed with 1.
/// </summary>
public sealed class CataCharacterListReaderTests
{
    private sealed record Character(ulong Guid, ulong GuildGuid, string Name, bool FirstLogin, byte Class, byte Race,
        byte Sex, byte Skin, byte Face, byte HairStyle, byte HairColor, byte FacialHair, byte Level, uint Zone, int Map,
        Vector3 Pos, uint Flags, uint Flags2, uint PetDisplay, uint PetLevel, uint PetFamily);

    private static readonly Character[] Characters =
    [
        new(0x0000000000001A2B, 0x1FF6000000000007, "Xiansword", false, 1, 3, 0, 4, 5, 6, 7, 8, 12, 1, 0,
            new Vector3(-6240.3f, 331.0f, 382.8f), 0x02000000, 0x00000003, 0, 0, 0),
        new(0x0000000000000100, 0, "Newbie", true, 3, 22, 1, 1, 0, 2, 3, 0, 1, 0, 654,
            new Vector3(-1451.5f, 1403.2f, 35.6f), 0, 0, 17252, 1, 21),
    ];

    [Fact]
    public void ReadsWhatTrinityCore434Writes()
    {
        using var packet = new WorldPacket(Write(Characters));
        var entries = WorldClient.ReadCharacterEntriesCata(packet);

        Assert.Equal(Characters.Length, entries.Count);
        for (int i = 0; i < Characters.Length; i++)
        {
            Character c = Characters[i];
            var e = entries[i];
            Assert.Equal(c.Guid, e.Guid.Low);
            Assert.Equal((uint)c.GuildGuid, e.GuildId);
            Assert.Equal(c.Name, e.Name);
            Assert.Equal(c.FirstLogin, e.FirstLogin);
            Assert.Equal((Class)c.Class, e.Class);
            Assert.Equal((Race)c.Race, e.Race);
            Assert.Equal((Gender)c.Sex, e.Sex);
            Assert.Equal((c.Skin, c.Face, c.HairStyle, c.HairColor, c.FacialHair), (e.Skin, e.Face, e.HairStyle, e.HairColor, e.FacialHair));
            Assert.Equal(c.Level, e.Level);
            Assert.Equal(c.Zone, e.ZoneId);
            Assert.Equal((uint)c.Map, e.MapId);
            Assert.Equal(c.Pos, e.PreloadPos);
            Assert.Equal(c.Flags, (uint)e.Flags);
            Assert.Equal(c.Flags2, e.CustomizationFlags);
            Assert.Equal((c.PetDisplay, c.PetLevel, c.PetFamily), (e.PetCreatureDisplayId, e.PetExperienceLevel, e.PetCreatureFamilyId));
            Assert.Equal(23, e.VisualItems.Length);
            Assert.Equal((uint)(1000 + i), e.VisualItems[4].DisplayId);
            Assert.Equal((byte)5, e.VisualItems[4].InvType);
            Assert.Equal((uint)(2000 + i), e.VisualItems[4].DisplayEnchantId);
        }

        Assert.Equal(0, packet.GetRemainingSpan().Length);
    }

    // TrinityCore 4.3.4 (Cataclysm Preservation Project) CharacterPackets.cpp
    // EnumCharactersResult::Write, ported. One faction-change rule closes the packet.
    private static byte[] Write(Character[] characters)
    {
        using var w = new WorldPacket(Opcode.SMSG_ENUM_CHARACTERS_RESULT);
        w.WriteBits(1, 23);                                 // FactionChangeRestrictions
        w.WriteBit(true);                                   // Success
        w.WriteBits(characters.Length, 17);

        foreach (Character c in characters)
        {
            byte[] g = BitConverter.GetBytes(c.Guid);
            byte[] gg = BitConverter.GetBytes(c.GuildGuid);
            w.WriteBit(g[3] != 0);
            w.WriteBit(gg[1] != 0);
            w.WriteBit(gg[7] != 0);
            w.WriteBit(gg[2] != 0);
            w.WriteBits(c.Name.Length, 7);
            w.WriteBit(g[4] != 0);
            w.WriteBit(g[7] != 0);
            w.WriteBit(gg[3] != 0);
            w.WriteBit(g[5] != 0);
            w.WriteBit(gg[6] != 0);
            w.WriteBit(g[1] != 0);
            w.WriteBit(gg[5] != 0);
            w.WriteBit(gg[4] != 0);
            w.WriteBit(c.FirstLogin);
            w.WriteBit(g[0] != 0);
            w.WriteBit(g[2] != 0);
            w.WriteBit(g[6] != 0);
            w.WriteBit(gg[0] != 0);
        }
        w.FlushBits();

        for (int i = 0; i < characters.Length; i++)
        {
            Character c = characters[i];
            byte[] g = BitConverter.GetBytes(c.Guid);
            byte[] gg = BitConverter.GetBytes(c.GuildGuid);
            void Seq(byte b) { if (b != 0) w.WriteUInt8((byte)(b ^ 1)); }

            w.WriteUInt8(c.Class);
            for (int j = 0; j < 23; j++)
            {
                w.WriteUInt8(j == 4 ? (byte)5 : (byte)0);
                w.WriteUInt32(j == 4 ? (uint)(1000 + i) : 0u);
                w.WriteUInt32(j == 4 ? (uint)(2000 + i) : 0u);
            }

            w.WriteUInt32(c.PetFamily);
            Seq(gg[2]);
            w.WriteUInt8((byte)i);                          // ListPosition
            w.WriteUInt8(c.HairStyle);
            Seq(gg[3]);
            w.WriteUInt32(c.PetDisplay);
            w.WriteUInt32(c.Flags);
            w.WriteUInt8(c.HairColor);
            Seq(g[4]);
            w.WriteInt32(c.Map);
            Seq(gg[5]);
            w.WriteFloat(c.Pos.Z);
            Seq(gg[6]);
            w.WriteUInt32(c.PetLevel);
            Seq(g[3]);
            w.WriteFloat(c.Pos.Y);
            w.WriteUInt32(c.Flags2);
            w.WriteUInt8(c.FacialHair);
            Seq(g[7]);
            w.WriteUInt8(c.Sex);
            w.WriteString(c.Name);
            w.WriteUInt8(c.Face);
            Seq(g[0]);
            Seq(g[2]);
            Seq(gg[1]);
            Seq(gg[7]);
            w.WriteFloat(c.Pos.X);
            w.WriteUInt8(c.Skin);
            w.WriteUInt8(c.Race);
            w.WriteUInt8(c.Level);
            Seq(g[6]);
            Seq(gg[4]);
            Seq(gg[0]);
            Seq(g[5]);
            Seq(g[1]);
            w.WriteUInt32(c.Zone);
        }

        w.WriteInt32(0x3FF);                                // rule mask
        w.WriteUInt8(22);                                   // rule race

        byte[] body = w.GetDataSpan().ToArray();
        byte[] framed = new byte[sizeof(ushort) + body.Length];
        BitConverter.GetBytes((ushort)1).CopyTo(framed, 0);
        body.CopyTo(framed, sizeof(ushort));
        return framed;
    }
}
