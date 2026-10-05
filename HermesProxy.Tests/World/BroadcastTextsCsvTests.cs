using System.Collections.Generic;
using System.IO;
using System.Linq;

using HermesProxy.World;

using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// CSV/BroadcastTexts3.csv maps a greeting the 3.3.5a server sends to the id the client will
/// know it by. A greeting that is missing still works, on a session-allocated id, so nothing
/// fails loudly when the file loses rows or a row stops matching. These tests read the shipped
/// file through the loader's own parser, without touching the process-wide store.
/// </summary>
public class BroadcastTextsCsvTests
{
    private static List<GameData.BroadcastText> LoadShipped()
        => GameData.ReadBroadcastTexts(Path.Combine("CSV", "BroadcastTexts3.csv"));

    [Fact]
    public void ShippedTable_EveryEntry_IsUnique()
    {
        // LoadBroadcastTexts adds into a dictionary: a repeated entry throws at startup.
        var texts = LoadShipped();

        Assert.Equal(texts.Count, texts.Select(text => text.Entry).Distinct().Count());
    }

    [Fact]
    public void ShippedTable_CoversNorthrend()
    {
        // The file was once a copy of the TBC one, which ends at 28566.
        Assert.Contains(LoadShipped(), text => text.Entry > 28566);
    }

    [Fact]
    public void ShippedTable_LineBreakInsideAText_IsABareLineFeed()
    {
        // The server sends "\n". A checkout that rewrote it to "\r\n" would load fine and
        // silently stop matching; .gitattributes pins the file to LF for that reason.
        var texts = LoadShipped();

        Assert.Contains(texts, text => text.MaleText.Contains('\n'));
        Assert.DoesNotContain(texts, text => text.MaleText.Contains('\r') || text.FemaleText.Contains('\r'));
    }

    [Fact]
    public void ShippedTable_StormwindGuardDirections_KeepTheirRealId()
    {
        var text = Assert.Single(LoadShipped(), text => text.Entry == 2878);

        Assert.StartsWith("Gryphons, eh? Never really cared for the beasts but to each their own.\n", text.MaleText);
    }
}
