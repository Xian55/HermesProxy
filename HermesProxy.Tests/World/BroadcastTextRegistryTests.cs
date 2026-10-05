using HermesProxy.World;

using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// The modern client keeps a BroadcastText row in its own cache across sessions, so an id has to
/// mean the same text on every proxy run. Each test builds its own registry; nothing here touches
/// the process-wide one.
/// </summary>
public class BroadcastTextRegistryTests
{
    private static readonly ushort[] _none = [0, 0, 0];

    private static GameData.BroadcastText Row(uint entry, string male, string female, uint language = 0, ushort[]? emotes = null)
        => new() { Entry = entry, MaleText = male, FemaleText = female, Language = language, Emotes = emotes ?? [0, 0, 0] };

    private static uint Resolve(BroadcastTextRegistry registry, string male, string female, uint language = 0, ushort[]? emotes = null)
        => registry.Resolve(male, female, language, _none, emotes ?? _none);

    [Fact]
    public void Resolve_TextNotInTheTable_GetsTheSameIdWhateverArrivedFirst()
    {
        BroadcastTextRegistry firstRun = new([]);
        uint bank = Resolve(firstRun, "The bank is in the Trade District.", "The bank is in the Trade District.");
        uint inn = Resolve(firstRun, "The inn is by the canal.", "The inn is by the canal.");

        BroadcastTextRegistry secondRun = new([]);
        Assert.Equal(inn, Resolve(secondRun, "The inn is by the canal.", "The inn is by the canal."));
        Assert.Equal(bank, Resolve(secondRun, "The bank is in the Trade District.", "The bank is in the Trade District."));
        Assert.NotEqual(bank, inn);
    }

    // Clients keep these ids. A change to the hash orphans every one they have cached, so the
    // values are pinned; they were computed by a separate implementation of the same FNV-1a.
    [Theory]
    [InlineData("Hey there, $N. How can I help you?", "Hey there, $N. How can I help you?", 0x693012E1u)]
    [InlineData("Это МОЯ ТЕЛЕГА!\r\n\r\nМОЯ!", "Это МОЯ ТЕЛЕГА!\r\n\r\nМОЯ!", 0x4FD9FB56u)]
    [InlineData("a", "", 0x7D58AF02u)]
    public void Resolve_TextNotInTheTable_DerivesAPinnedId(string male, string female, uint expected)
    {
        BroadcastTextRegistry registry = new([]);

        Assert.Equal(expected, Resolve(registry, male, female));
    }

    [Fact]
    public void Resolve_LanguageDelaysAndEmotes_ArePartOfThePinnedId()
    {
        BroadcastTextRegistry registry = new([]);

        Assert.Equal(0x73EF14A7u, registry.Resolve("Greetings.", "Welcome.", 7, [0, 100, 0], [1, 2, 396]));
    }

    [Fact]
    public void Resolve_TextNotInTheTable_StaysClearOfRealIds()
    {
        BroadcastTextRegistry registry = new([]);

        uint id = Resolve(registry, "Custom greeting", "Custom greeting");

        Assert.InRange(id, BroadcastTextRegistry.DerivedIdBase, (uint)int.MaxValue);
    }

    [Fact]
    public void Resolve_TableRowWithOneGender_IsFoundByWhatTheServerSends()
    {
        // The servers fill an empty gender with the other one before sending.
        BroadcastTextRegistry registry = new([Row(2878, "Gryphons, eh?", ""), Row(7449, "", "Do you require an illusion?")]);

        Assert.Equal(2878u, Resolve(registry, "Gryphons, eh?", "Gryphons, eh?"));
        Assert.Equal(7449u, Resolve(registry, "Do you require an illusion?", "Do you require an illusion?"));
    }

    [Fact]
    public void Resolve_TableRowWithTrailingWhitespace_IsFoundByTheTrimmedText()
    {
        // The handler trims what it read off the wire; a row is stored as the database has it.
        BroadcastTextRegistry registry = new([Row(2590, "Yes, I am. ", "")]);

        Assert.Equal(2590u, Resolve(registry, "Yes, I am.", "Yes, I am."));
    }

    [Fact]
    public void Resolve_OnlyOneGenderMatchesATableRow_IsNotThatRow()
    {
        // Matching on either text alone handed this greeting to row 5884, which shows a
        // different text to a male NPC's visitor.
        BroadcastTextRegistry registry = new([Row(5884, "Us have skinning hook.", "Us have skinning hook.")]);

        uint id = Resolve(registry, "Head to the Rogues' Quarter.", "Us have skinning hook.");

        Assert.NotEqual(5884u, id);
        Assert.Equal("Head to the Rogues' Quarter.", registry.Get(id)!.MaleText);
    }

    [Fact]
    public void Resolve_SameTextOtherLanguageOrEmotes_IsAnotherId()
    {
        BroadcastTextRegistry registry = new([Row(2974, "Greetings.", "", language: 7, emotes: [1, 0, 0])]);

        Assert.Equal(2974u, Resolve(registry, "Greetings.", "Greetings.", language: 7, emotes: [1, 0, 0]));
        Assert.NotEqual(2974u, Resolve(registry, "Greetings.", "Greetings.", language: 0, emotes: [1, 0, 0]));
        Assert.NotEqual(2974u, Resolve(registry, "Greetings.", "Greetings.", language: 7, emotes: [0, 0, 0]));
    }

    [Fact]
    public void Resolve_TwoTableRowsCarryTheText_PicksTheLowerId()
    {
        BroadcastTextRegistry registry = new([Row(16967, "Welcome.", ""), Row(2821, "Welcome.", "")]);

        Assert.Equal(2821u, Resolve(registry, "Welcome.", "Welcome."));
    }

    [Fact]
    public void Resolve_DerivedIdAlreadyTaken_TakesTheNextOne()
    {
        uint wanted = Resolve(new BroadcastTextRegistry([]), "Custom greeting", "Custom greeting");
        BroadcastTextRegistry registry = new([Row(wanted, "Something else", "")]);

        uint id = Resolve(registry, "Custom greeting", "Custom greeting");

        Assert.Equal(wanted + 1, id);
        Assert.Equal("Custom greeting", registry.Get(id)!.MaleText);
        Assert.Equal("Something else", registry.Get(wanted)!.MaleText);
    }

    [Fact]
    public void Get_DerivedId_ReturnsTheTextExactlyAsItArrived()
    {
        BroadcastTextRegistry registry = new([]);
        ushort[] delays = [0, 250, 0], emotes = [1, 5, 0];

        uint id = registry.Resolve("Это МОЯ ТЕЛЕГА!\r\n\r\nМОЯ!", "", 7, delays, emotes);
        delays[1] = 9;
        emotes[0] = 9;

        GameData.BroadcastText text = registry.Get(id)!;
        Assert.Equal(id, text.Entry);
        Assert.Equal("Это МОЯ ТЕЛЕГА!\r\n\r\nМОЯ!", text.MaleText);
        Assert.Equal("", text.FemaleText);
        Assert.Equal(7u, text.Language);
        Assert.Equal<ushort>([0, 250, 0], text.EmoteDelays);
        Assert.Equal<ushort>([1, 5, 0], text.Emotes);
    }

    [Fact]
    public void Get_IdNeverHandedOut_IsNull()
    {
        Assert.Null(new BroadcastTextRegistry([]).Get(BroadcastTextRegistry.DerivedIdBase));
    }
}
