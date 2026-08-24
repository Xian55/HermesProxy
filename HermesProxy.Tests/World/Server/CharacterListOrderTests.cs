using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

    [Fact]
    public void Prune_DropsSlotsMissingFromLiveEnum()
    {
        var chars = new List<EnumCharactersResult.CharacterInfo>
        {
            Char(10, "A", 0),
            Char(20, "B", 1),
        };
        var saved = new List<CharacterListSlot>
        {
            new(10, 10),
            new(99, 20),
            new(20, 30),
        };

        var pruned = CharacterListOrder.Prune(saved, chars);

        Assert.Equal(new ulong[] { 10, 20 }, pruned.ConvertAll(s => s.GuidLow));
    }

    [Fact]
    public void Apply_ReturnsPrunedSaveWithoutDeadGuids()
    {
        var chars = new List<EnumCharactersResult.CharacterInfo>
        {
            Char(10, "A", 0),
            Char(20, "B", 1),
        };
        var saved = new List<CharacterListSlot>
        {
            new(20, 10),
            new(99, 20),
            new(10, 30),
        };

        var pruned = CharacterListOrder.Apply(chars, saved);

        Assert.Equal(new ulong[] { 20, 10 }, pruned.ConvertAll(s => s.GuidLow));
        Assert.Equal(new[] { "B", "A" }, chars.ConvertAll(c => c.Name));
        Assert.Equal(new byte[] { 10, 20 }, chars.ConvertAll(c => c.ListPosition));
    }

    [Fact]
    public void Normalize_ClampsPositionsThatWouldOverflowByte()
    {
        var slots = new List<CharacterListSlot>(30);
        for (int i = 0; i < 30; i++)
            slots.Add(new CharacterListSlot((ulong)(i + 1), 10));

        var normalized = CharacterListOrder.Normalize(slots);

        Assert.Equal((byte)250, normalized[24].ListPosition);
        Assert.Equal(byte.MaxValue, normalized[25].ListPosition);
        Assert.Equal(byte.MaxValue, normalized[29].ListPosition);
        Assert.NotEqual((byte)4, normalized[25].ListPosition);
    }

    [Fact]
    public void PositionAt_ClampsInsteadOfWrapping()
    {
        Assert.Equal((byte)10, CharacterListOrder.PositionAt(0));
        Assert.Equal((byte)250, CharacterListOrder.PositionAt(24));
        Assert.Equal(byte.MaxValue, CharacterListOrder.PositionAt(25));
        Assert.Equal(byte.MaxValue, CharacterListOrder.PositionAt(29));
    }
}

public class CharacterListOrderFileTests : IDisposable
{
    private readonly string _accountName = "HermesTests_CharOrder_" + Guid.NewGuid().ToString("N");
    private readonly string _realmName = "TestRealm";

    private string OrderPath =>
        Path.GetFullPath(Path.Combine("AccountData", _accountName, _realmName, "char_list_order.txt"));

    public void Dispose()
    {
        var accountDir = Path.GetFullPath(Path.Combine("AccountData", _accountName));
        if (Directory.Exists(accountDir))
            Directory.Delete(accountDir, recursive: true);
    }

    [Fact]
    public void Save_WritesUtf8WithoutBom()
    {
        var mgr = new AccountMetaDataManager(_accountName);
        mgr.SaveCharacterListOrder(_realmName, new List<CharacterListSlot> { new(1, 10), new(2, 20) });

        var bytes = File.ReadAllBytes(OrderPath);
        Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);
        Assert.Equal("1,10", File.ReadAllLines(OrderPath)[0]);
    }

    [Fact]
    public void Load_StillReadsLegacyBomFile()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(OrderPath)!);
        File.WriteAllLines(OrderPath, new[] { "1,10", "2,20" }, Encoding.UTF8);

        var loaded = new AccountMetaDataManager(_accountName).LoadCharacterListOrder(_realmName);

        Assert.Equal(new ulong[] { 1, 2 }, loaded.ConvertAll(s => s.GuidLow));
        Assert.Equal(new byte[] { 10, 20 }, loaded.ConvertAll(s => s.ListPosition));
    }

    [Fact]
    public void Load_SkipsUnparseableLines()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(OrderPath)!);
        File.WriteAllLines(OrderPath, new[] { "not-a-guid,10", "5,20", "6,nope", "7,30" });

        var loaded = new AccountMetaDataManager(_accountName).LoadCharacterListOrder(_realmName);

        Assert.Equal(new ulong[] { 5, 7 }, loaded.ConvertAll(s => s.GuidLow));
        Assert.Equal(new byte[] { 20, 30 }, loaded.ConvertAll(s => s.ListPosition));
    }
}
