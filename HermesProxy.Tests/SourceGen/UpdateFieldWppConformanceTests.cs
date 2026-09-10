using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HermesProxy.World.Objects.Version.Attributes;
using Xunit;
using V343 = HermesProxy.World.Enums.V3_4_3_54261;

namespace HermesProxy.Tests.SourceGen;

/// <summary>
/// Checks the V3_4_3 descriptor tables — the <c>[DescriptorUpdateField]</c>, <c>[DescriptorCustomField]</c>
/// and <c>[DescriptorMaskPreamble]</c> attributes the source generator turns into WriteUpdate*Data —
/// against WowPacketParser's reader for the same client build (see Reference/WowPacketParser/README.md).
/// A wrong bit, parent bit, array size or value width puts every later byte of that Values update out
/// of alignment, which the byte-equivalence tests cannot catch: they compare against a hand-port that
/// carried the same numbers.
/// </summary>
public class UpdateFieldWppConformanceTests
{
    private static readonly Lazy<Dictionary<string, Dictionary<string, WppField>>> Wpp =
        new(() => WppUpdateFieldReader.ReadEmbedded("WowPacketParser.UpdateFieldsHandler343.cs"));

    private static readonly Dictionary<string, Type> SectionEnums = new()
    {
        ["Object"] = typeof(V343.ObjectField),
        ["Item"] = typeof(V343.ItemField),
        ["Container"] = typeof(V343.ContainerField),
        ["Unit"] = typeof(V343.UnitField),
        ["Player"] = typeof(V343.PlayerField),
        ["ActivePlayer"] = typeof(V343.ActivePlayerField),
        ["GameObject"] = typeof(V343.GameObjectField),
        ["DynamicObject"] = typeof(V343.DynamicObjectField),
        ["Corpse"] = typeof(V343.CorpseField),
    };

    public static TheoryData<string> Sections => new(SectionEnums.Keys);

    // Our data-class property name → WPP's name for the same field. Each entry is checked below:
    // both names must exist, and WPP must not also carry ours (which would make the entry stale).
    private static readonly Dictionary<(string Section, string Ours), string> Aliases = new()
    {
        [("Item", "Duration")] = "Expiration",
        [("Item", "Flags")] = "DynamicFlags",
        [("Item", "RandomProperty")] = "RandomPropertiesID",
        [("Item", "HasGemsUpdate")] = "Gems",
        [("Unit", "RaceId")] = "Race",
        [("Unit", "SexId")] = "Sex",
        [("Unit", "ModCastSpeed")] = "ModCastingSpeed",
        [("Unit", "ModCastHaste")] = "ModSpellHaste",
        [("Unit", "ChannelObject")] = "ChannelObjects",
        [("Unit", "AttackSpeedAura")] = "SetAttackSpeedAura",
        [("Player", "HasCustomizationsUpdate")] = "Customizations",
        [("Player", "PvPRank")] = "PvpRank",
        [("Player", "ChosenTitle")] = "PlayerTitle",
        [("ActivePlayer", "PvPTierMaxFromWins")] = "PvpTierMaxFromWins",
        [("ActivePlayer", "PvPLastWeeksTierMaxFromWins")] = "PvpLastWeeksTierMaxFromWins",
        [("ActivePlayer", "PvPRankProgress")] = "PvpRankProgress",
        [("GameObject", "StateAnimID")] = "SpawnTrackingStateAnimID",
        [("GameObject", "StateAnimKitID")] = "SpawnTrackingStateAnimKitID",
        [("Corpse", "RaceId")] = "RaceID",
        [("Corpse", "SexId")] = "Sex",
        [("Corpse", "ClassId")] = "Class",
    };

    [Theory]
    [MemberData(nameof(Sections))]
    public void UpdateFields_MatchWpp343(string section)
    {
        var wpp = Wpp.Value[section];
        var problems = new List<string>();
        var declared = new HashSet<string>();

        foreach (var (member, field) in Attributes<DescriptorUpdateFieldAttribute>(section))
        {
            string wppName = Aliases.GetValueOrDefault((section, field.SourceProperty), field.SourceProperty);
            declared.Add(wppName);
            string label = $"{SectionEnums[section].Name}.{member} ({field.SourceProperty})";

            if (!wpp.TryGetValue(wppName, out var w))
            {
                problems.Add($"{label}: WPP 343 has no field named '{wppName}'; WPP bit {field.Bit} is {At(wpp, field.Bit)}");
                continue;
            }

            if (field.Bit != w.Bit)
                problems.Add($"{label}: bit {field.Bit}, WPP 343 reads it at {w.Bit}; WPP bit {field.Bit} is {At(wpp, field.Bit)}");

            if (Normalize(field.ParentBit, field.Bit) != Normalize(w.Parent, w.Bit))
                problems.Add($"{label}: parent bit {field.ParentBit}, WPP 343 nests it under {w.Parent}");

            bool perElement = field.ArrayCount > 1 && field.ArrayMode == ArrayMode.PerElement;
            if (w.PerElement && (!perElement || field.ArrayCount > w.Count))
                problems.Add($"{label}: {(perElement ? $"{field.ArrayCount} elements" : "not per-element")}, WPP 343 has a {w.Count}-element per-element array");
            else if (!w.PerElement && perElement)
                problems.Add($"{label}: per-element array of {field.ArrayCount}, WPP 343 reads a single value");

            if (field.CustomWriter is null && !TypeAgrees(field, w.Reader))
                problems.Add($"{label}: {field.Type}, WPP 343 reads it with {w.Reader}");
        }

        foreach (var (member, custom) in Attributes<DescriptorCustomFieldAttribute>(section))
        {
            if (!wpp.Values.Any(f => f.Bit == custom.Bit || f.Parent == custom.Bit))
                problems.Add($"{SectionEnums[section].Name}.{member} (custom '{custom.Label}'): bit {custom.Bit} is neither a field nor an array parent in WPP 343");
        }

        foreach (var (member, preamble) in Attributes<DescriptorMaskPreambleAttribute>(section))
        {
            if (!wpp.Values.Any(f => f.Bit == preamble.Bit && f.Reader == "ReadUpdateMask"))
                problems.Add($"{SectionEnums[section].Name}.{member} (mask preamble): bit {preamble.Bit} is {At(wpp, preamble.Bit)}, not a dynamic field");
        }

        // Missing fields are safe — the client keeps its create-time value — so they are reported,
        // not failed. They are where a field silently stops updating after spawn.
        var undeclared = wpp.Values.Where(f => !declared.Contains(f.Name)).OrderBy(f => f.Bit).ToList();
        TestContext.Current.TestOutputHelper?.WriteLine(
            $"{section}: {undeclared.Count} WPP 343 field(s) not declared: " +
            string.Join(", ", undeclared.Select(f => $"{f.Name}@{f.Bit}")));

        Assert.True(problems.Count == 0,
            $"{section}: {problems.Count} descriptor(s) disagree with WPP 343:{Environment.NewLine}  "
            + string.Join(Environment.NewLine + "  ", problems));
    }

    [Fact]
    public void WppReference_ParsesKnownAnchors()
    {
        // Guards against the walker silently finding nothing after the reference file is refreshed.
        foreach (var section in SectionEnums.Keys)
            Assert.True(Wpp.Value.TryGetValue(section, out var fields) && fields.Count > 0, $"no fields read for {section}");

        var unit = Wpp.Value["Unit"];
        Assert.True(unit.Count > 100, $"only {unit.Count} Unit fields read");
        Assert.Equal(new WppField("Health", 5, false, 0, 0, "ReadInt64"), unit["Health"]);
        Assert.Equal(new WppField("NpcFlags", 114, true, 2, 113, "ReadUInt32"), unit["NpcFlags"]);
        Assert.Equal("ReadUpdateMask", unit["ChannelObjects"].Reader);
    }

    [Fact]
    public void Aliases_NameRealFieldsOnBothSides()
    {
        foreach (var ((section, ours), wppName) in Aliases)
        {
            Assert.True(Attributes<DescriptorUpdateFieldAttribute>(section).Any(a => a.Attribute.SourceProperty == ours),
                $"alias {section}.{ours}: no [DescriptorUpdateField] declares it");
            Assert.True(Wpp.Value[section].ContainsKey(wppName), $"alias {section}.{ours} → {wppName}: WPP 343 has no such field");
            Assert.False(Wpp.Value[section].ContainsKey(ours), $"alias {section}.{ours} is stale: WPP 343 now uses the same name");
        }
    }

    private static IEnumerable<(string Member, T Attribute)> Attributes<T>(string section) where T : Attribute
        => SectionEnums[section].GetFields(BindingFlags.Public | BindingFlags.Static)
            .SelectMany(f => f.GetCustomAttributes<T>().Select(a => (f.Name, a)));

    // A parent on the block leader (bit 0, 32, 64, ... — what every field in that block sits under)
    // says nothing more than no parent; our tables write it either way.
    private static int Normalize(int parent, int bit) => parent == bit / 32 * 32 ? -1 : parent;

    private static string At(Dictionary<string, WppField> wpp, int bit)
    {
        var owner = wpp.Values.FirstOrDefault(f => bit >= f.Bit && bit < f.Bit + Math.Max(1, f.Count));
        return owner is null ? "unused" : $"{owner.Name} ({owner.Reader})";
    }

    private static bool TypeAgrees(DescriptorUpdateFieldAttribute field, string reader) => reader switch
    {
        "ReadQuaternion" => field.Type == DescriptorType.Float && field.ArrayCount == 4,
        "ReadPackedGuid128" => field.Type == DescriptorType.PackedGuid128,
        "ReadSingle" => field.Type == DescriptorType.Float,
        // Signedness does not change the bytes, so only width is compared for integers.
        "ReadByte" or "ReadSByte" => field.Type is DescriptorType.UInt8 or DescriptorType.Int8,
        "ReadInt16" or "ReadUInt16" => field.Type is DescriptorType.UInt16 or DescriptorType.Int16,
        "ReadInt32" or "ReadUInt32" => field.Type is DescriptorType.UInt32 or DescriptorType.Int32,
        "ReadInt64" or "ReadUInt64" => field.Type is DescriptorType.UInt64 or DescriptorType.Int64,
        // Dynamic-field masks, bit fields and nested structures are not a single typed value.
        _ => true,
    };
}
