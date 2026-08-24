using System.Collections.Generic;
using HermesProxy.World;
using HermesProxy.World.Server;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

public class CharacterListOrderTests
{
    private static EnumCharactersResult.CharacterInfo Char(ulong low, string name, byte listPosition)
    {
        return new EnumCharactersResult.CharacterInfo
        {
            Guid = new WowGuid128(low, 1),
            Name = name,
            ListPosition = listPosition,
        };
    }

    [Fact]
    public void Apply_WithEmptySave_LeavesEnumOrder()
    {
        var chars = new List<EnumCharactersResult.CharacterInfo>
        {
            Char(10, "A", 0),
            Char(20, "B", 1),
        };

        CharacterListOrder.Apply(chars, new List<CharacterListSlot>());

        Assert.Equal("A", chars[0].Name);
        Assert.Equal((byte)0, chars[0].ListPosition);
        Assert.Equal("B", chars[1].Name);
    }

    [Fact]
    public void Apply_KeepsClientListPositions()
    {
        var chars = new List<EnumCharactersResult.CharacterInfo>
        {
            Char(10, "A", 0),
            Char(20, "B", 1),
            Char(30, "C", 2),
        };

        CharacterListOrder.Apply(chars, new List<CharacterListSlot>
        {
            new(30, 10),
            new(10, 20),
            new(20, 30),
        });

        Assert.Equal(new[] { "C", "A", "B" }, chars.ConvertAll(c => c.Name));
        Assert.Equal(new byte[] { 10, 20, 30 }, chars.ConvertAll(c => c.ListPosition));
    }

    [Fact]
    public void Apply_CompactsToTenStepGrid()
    {
        var chars = new List<EnumCharactersResult.CharacterInfo>
        {
            Char(10, "A", 0),
            Char(20, "B", 1),
            Char(30, "C", 2),
        };

        CharacterListOrder.Apply(chars, new List<CharacterListSlot>
        {
            new(30, 15),
            new(10, 17),
            new(20, 40),
        });

        Assert.Equal(new[] { "C", "A", "B" }, chars.ConvertAll(c => c.Name));
        Assert.Equal(new byte[] { 10, 20, 30 }, chars.ConvertAll(c => c.ListPosition));
    }

    [Fact]
    public void Apply_AppendsUnknownCharsAfterSavedPositions()
    {
        var chars = new List<EnumCharactersResult.CharacterInfo>
        {
            Char(10, "A", 0),
            Char(20, "B", 1),
            Char(40, "D", 2),
        };

        CharacterListOrder.Apply(chars, new List<CharacterListSlot>
        {
            new(20, 10),
            new(99, 20),
        });

        Assert.Equal(new[] { "B", "A", "D" }, chars.ConvertAll(c => c.Name));
        Assert.Equal(new byte[] { 10, 20, 30 }, chars.ConvertAll(c => c.ListPosition));
    }

    [Fact]
    public void Merge_OverlaysPartialReorderOntoExisting()
    {
        var existing = new List<CharacterListSlot>
        {
            new(1, 10),
            new(2, 20),
            new(3, 30),
            new(4, 40),
        };
        var incoming = new List<CharacterListSlot>
        {
            new(2, 15),
            new(4, 25),
        };

        var merged = CharacterListOrder.Merge(existing, incoming);

        Assert.Equal(new ulong[] { 1, 2, 4, 3 }, merged.ConvertAll(s => s.GuidLow));
        Assert.Equal(new byte[] { 10, 20, 30, 40 }, merged.ConvertAll(s => s.ListPosition));
    }
}
