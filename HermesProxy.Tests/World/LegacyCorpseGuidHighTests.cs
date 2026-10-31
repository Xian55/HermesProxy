using System;
using HermesProxy.World;
using HermesProxy.World.Enums;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// CMaNGOS numbers corpses 0xF500, AzerothCore and TrinityCore 0xF101. The proxy knew only the second: a corpse
/// in view became a null guid, and the exception took the whole update batch with it, the player's
/// own create included, so a dead character could not log in.
/// </summary>
[Collection("LegacyItemGuidHigh")]
public sealed class LegacyCorpseGuidHighTests : IDisposable
{
    public LegacyCorpseGuidHighTests() => LegacyCorpseGuidHigh.Reset();

    public void Dispose() => LegacyCorpseGuidHigh.Reset();

    [Fact]
    public void FromLegacy_CMaNGOSCorpse_IsACorpse()
        => Assert.Equal(HighGuidType.Corpse, HighGuid.FromLegacy(HighGuidTypeLegacy.CorpseMangos));

    [Fact]
    public void To64_CorpseArrivedAsF500_KeepsF500()
    {
        var legacy = new WowGuid64(HighGuidTypeLegacy.CorpseMangos, 579);

        var modern = legacy.To128(null!);

        Assert.False(modern.IsEmpty());
        Assert.Equal(HighGuidTypeLegacy.CorpseMangos, modern.To64().GetHighGuidTypeLegacy());
        Assert.Equal(579UL, modern.To64().GetCounter());
    }

    [Fact]
    public void To64_CorpseArrivedAsF101_KeepsF101()
    {
        var legacy = new WowGuid64(HighGuidTypeLegacy.Corpse, 579);

        Assert.Equal(HighGuidTypeLegacy.Corpse, legacy.To128(null!).To64().GetHighGuidTypeLegacy());
    }
}
