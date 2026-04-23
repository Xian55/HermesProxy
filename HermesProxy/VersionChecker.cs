using Framework;
using Framework.Logging;

using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy;

// VersionBootstrap — the one-line mutable handoff point for ModernVersion / LegacyVersion.
// Assigned exactly once at Host startup (ProxyHostedService.ExecuteAsync), and by the
// test-assembly [ModuleInitializer] / benchmark GlobalSetup before any code touches the
// two static classes. The static-readonly fields of ModernVersion / LegacyVersion are
// initialized from these values via field initializers, keeping both types
// beforefieldinit-clean so the JIT can fold Build / ExpansionVersion / table references
// as constants on the per-packet hot path.
internal static class VersionBootstrap
{
    internal static ClientVersionBuild ModernBuild;
    internal static ClientVersionBuild LegacyBuild;
}

// This is class is a plain copy of ModernVersion/LegacyVersion/Opcodes but without static constructor
public static class VersionChecker
{
    // Shared with sibling classes LegacyVersion / ModernVersion in this file.
    internal static readonly Microsoft.Extensions.Logging.ILogger _melServer = Log.CreateMelLogger(Log.CategoryServer);
    internal static readonly string _sourceFile = nameof(VersionChecker).PadRight(15);
    internal const string _netDirNone = "";

    public static bool IsSupportedLegacyVersion(ClientVersionBuild legacyVersion) =>
        legacyVersion switch
        {
            ClientVersionBuild.V1_12_1_5875
                or ClientVersionBuild.V1_12_2_6005
                or ClientVersionBuild.V1_12_3_6141
                or ClientVersionBuild.V2_4_3_8606
                or ClientVersionBuild.V3_3_5a_12340 => true,
            _ => false,
        };

    public static bool IsSupportedModernVersion(ClientVersionBuild modernVersion) =>
        modernVersion switch
        {
            ClientVersionBuild.V2_5_2_39570
                or ClientVersionBuild.V2_5_2_39618
                or ClientVersionBuild.V2_5_2_39926
                or ClientVersionBuild.V2_5_2_40011
                or ClientVersionBuild.V2_5_2_40045
                or ClientVersionBuild.V2_5_2_40203
                or ClientVersionBuild.V2_5_2_40260
                or ClientVersionBuild.V2_5_2_40422
                or ClientVersionBuild.V2_5_2_40488
                or ClientVersionBuild.V2_5_2_40617
                or ClientVersionBuild.V2_5_2_40892
                or ClientVersionBuild.V2_5_2_41446
                or ClientVersionBuild.V2_5_2_41510
                or ClientVersionBuild.V1_14_0_39802
                or ClientVersionBuild.V1_14_0_39958
                or ClientVersionBuild.V1_14_0_40140
                or ClientVersionBuild.V1_14_0_40179
                or ClientVersionBuild.V1_14_0_40237
                or ClientVersionBuild.V1_14_0_40347
                or ClientVersionBuild.V1_14_0_40441
                or ClientVersionBuild.V1_14_0_40618
                or ClientVersionBuild.V1_14_1_40487
                or ClientVersionBuild.V1_14_1_40594
                or ClientVersionBuild.V1_14_1_40666
                or ClientVersionBuild.V1_14_1_40688
                or ClientVersionBuild.V1_14_1_40800
                or ClientVersionBuild.V1_14_1_40818
                or ClientVersionBuild.V1_14_1_40926
                or ClientVersionBuild.V1_14_1_40962
                or ClientVersionBuild.V1_14_1_41009
                or ClientVersionBuild.V1_14_1_41030
                or ClientVersionBuild.V1_14_1_41077
                or ClientVersionBuild.V1_14_1_41137
                or ClientVersionBuild.V1_14_1_41243
                or ClientVersionBuild.V1_14_1_41511
                or ClientVersionBuild.V1_14_1_41794
                or ClientVersionBuild.V1_14_1_42032
                or ClientVersionBuild.V2_5_3_41402
                or ClientVersionBuild.V2_5_3_41531
                or ClientVersionBuild.V2_5_3_41750
                or ClientVersionBuild.V2_5_3_41812
                or ClientVersionBuild.V2_5_3_42083
                or ClientVersionBuild.V2_5_3_42328
                or ClientVersionBuild.V2_5_3_42598
                or ClientVersionBuild.V1_14_2_41858
                or ClientVersionBuild.V1_14_2_41959
                or ClientVersionBuild.V1_14_2_42065
                or ClientVersionBuild.V1_14_2_42082
                or ClientVersionBuild.V1_14_2_42214
                or ClientVersionBuild.V1_14_2_42597
                or ClientVersionBuild.V3_4_3_54261 => true,
            _ => false,
        };

    public static ClientVersionBuild GetBestLegacyVersion(ClientVersionBuild modernVersion)
    {
        return GetExpansionVersion(modernVersion) switch
        {
            1 => ClientVersionBuild.V1_12_1_5875,
            2 => ClientVersionBuild.V2_4_3_8606,
            3 => ClientVersionBuild.V3_3_5a_12340,
            _ => ClientVersionBuild.Zero,
        };
    }

    private static byte GetExpansionVersion(ClientVersionBuild version)
    {
        ReadOnlySpan<char> span = version.ToString().AsSpan(1); // Skip 'V'
        int underscoreIndex = span.IndexOf('_');
        return byte.Parse(span[..underscoreIndex]);
    }
}
public class UpdateFieldInfo
{
    public int Value;
    public string Name = string.Empty;
    public int Size;
    public UpdateFieldType Format;
}
public static class LegacyVersion
{
    // Declaration order IS initialization order for static field initializers. Build must be
    // declared first so the loaders below can reference it through the derived fields.
    public static readonly ClientVersionBuild Build = RequireBuild();
    public static readonly byte ExpansionVersion = GetExpansionVersion();
    public static readonly byte MajorVersion = GetMajorPatchVersion();
    public static readonly byte MinorVersion = GetMinorPatchVersion();

    public static int BuildInt => (int)Build;
    public static string VersionString => Build.ToString();

    // Opcode translation uses direct-indexed arrays instead of FrozenDictionary. The per-version
    // legacy Opcode enums are uint-backed and densely-enough populated (max value ~1060 for TBC,
    // ~15k for modern) that flat arrays sized to (maxValue + 1) are both smaller than the
    // dictionary-plus-hash overhead AND reduce lookup to a bounds check + single load that the
    // JIT can inline and fold against the static-readonly array reference. Default slot value
    // (Opcode 0 = MSG_NULL_ACTION, uint 0 = invalid) matches the "not found" return of the old
    // dictionary path, so uninhabited array slots behave correctly without an explicit fill.
    private static readonly Opcode[] _currentToUniversal;
    private static readonly uint[]   _universalToCurrent;

    // Field initializers run in textual order inside a beforefieldinit cctor; to keep both
    // opcode-direction arrays populated by a single load pass (and to preserve beforefieldinit)
    // we produce them from one helper via a throwaway marker field whose initializer has the
    // side effect of assigning both arrays.
    private static readonly bool _opcodeTablesLoaded = LoadOpcodeTables(out _currentToUniversal, out _universalToCurrent);

    private static ClientVersionBuild RequireBuild()
    {
        if (VersionBootstrap.LegacyBuild == ClientVersionBuild.Zero)
            throw new InvalidOperationException(
                "LegacyVersion accessed before VersionBootstrap.LegacyBuild was set. " +
                "Host startup (ProxyHostedService.ExecuteAsync) or test/benchmark setup must assign it first.");
        return VersionBootstrap.LegacyBuild;
    }

    private static bool LoadOpcodeTables(out Opcode[] currentToUniversal, out uint[] universalToCurrent)
    {
        // The generator emits tables keyed by ClientVersionBuild members that correspond to the
        // "defining" builds (V1_12_1_5875, V2_4_3_8606, V2_5_2_39570, …). At runtime any supported
        // build is resolved to its defining build via Opcodes.GetOpcodesDefiningBuild so aliased
        // builds (V1_12_2_6005, V2_5_2_40892, etc.) pick up the right table.
        var definingBuild = Opcodes.GetOpcodesDefiningBuild(Build);
        if (!GeneratedOpcodeTables.TryGet(definingBuild, out currentToUniversal, out universalToCurrent))
        {
            Log.Print(LogType.Error, "Could not load opcodes for current legacy version.");
            return false;
        }

        ServerLogMessages.LoadedLegacyOpcodes(
            VersionChecker._melServer, VersionChecker._sourceFile, VersionChecker._netDirNone,
            CountMappings(universalToCurrent));
        return true;
    }

    // Counts non-zero entries in the reverse table — matches the "pairs.Count" diagnostic the
    // reflective loader used to print. Called once at startup, so the O(N) sweep is negligible.
    private static int CountMappings(uint[] universalToCurrent)
    {
        int count = 0;
        for (int i = 0; i < universalToCurrent.Length; i++)
            if (universalToCurrent[i] != 0) count++;
        return count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Opcode GetUniversalOpcode(uint opcode)
    {
        var table = _currentToUniversal;
        return opcode < (uint)table.Length ? table[opcode] : Opcode.MSG_NULL_ACTION;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GetCurrentOpcode(Opcode universalOpcode)
    {
        var table = _universalToCurrent;
        uint idx = (uint)universalOpcode;
        return idx < (uint)table.Length ? table[idx] : 0u;
    }

    public static ClientVersionBuild GetUpdateFieldsDefiningBuild()
    {
        return GetUpdateFieldsDefiningBuild(Build);
    }

    public static ClientVersionBuild GetUpdateFieldsDefiningBuild(ClientVersionBuild version) =>
        version switch
        {
            ClientVersionBuild.V1_12_1_5875
                or ClientVersionBuild.V1_12_2_6005
                or ClientVersionBuild.V1_12_3_6141 => ClientVersionBuild.V1_12_1_5875,
            ClientVersionBuild.V2_4_3_8606 => ClientVersionBuild.V2_4_3_8606,
            ClientVersionBuild.V3_3_5a_12340 => ClientVersionBuild.V3_3_5a_12340,
            _ => ClientVersionBuild.Zero,
        };

    // Per-T generic static cache. The CLR allocates one set of static fields per closed
    // generic instantiation (UpdateFields<PlayerField>, UpdateFields<UnitField>, …), so
    // lookups become direct static-field reads with no Dictionary<Type,_> hop. Nested inside
    // LegacyVersion so the same T used by ModernVersion.UpdateFields<T> resolves to a
    // different (per-version) cache. Tables are emitted at compile time by
    // HermesProxy.SourceGen.UpdateFieldTableGenerator.
    private static class UpdateFields<T> where T : System.Enum
    {
        public static readonly int[] Keys;
        public static readonly UpdateFieldInfo[] Infos;
        public static readonly Dictionary<string, int>? NamesToValues;

        static UpdateFields()
        {
            var definingBuild = GetUpdateFieldsDefiningBuild(Build);
            if (GeneratedUpdateFieldTables.TryGet(definingBuild, typeof(T),
                out var keys, out var infos, out var names))
            {
                Keys = keys;
                Infos = infos;
                NamesToValues = names;
            }
            else
            {
                Keys = Array.Empty<int>();
                Infos = Array.Empty<UpdateFieldInfo>();
                NamesToValues = null;
            }
        }
    }

    public static int GetUpdateField<T>(T field) where T: System.Enum // C# 7.3
    {
        var names = UpdateFields<T>.NamesToValues;
        if (names != null && names.TryGetValue(field.ToString(), out int fieldValue))
            return fieldValue;
        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetUpdateFieldName<T>(int field) where T: System.Enum // C# 7.3
    {
        var keys = UpdateFields<T>.Keys;
        var infos = UpdateFields<T>.Infos;
        if (keys.Length == 0)
            return field.ToString(CultureInfo.InvariantCulture);

        int idx = Array.BinarySearch(keys, field);
        if (idx >= 0)
            return infos[idx].Name;

        idx = ~idx - 1;
        if (idx < 0) // field lower than every key
            return field.ToString(CultureInfo.InvariantCulture);
        return infos[idx].Name + " + " + (field - keys[idx]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UpdateFieldInfo? GetUpdateFieldInfo<T>(int field) where T: System.Enum // C# 7.3
    {
        var keys = UpdateFields<T>.Keys;
        if (keys.Length == 0)
            return null;

        int idx = Array.BinarySearch(keys, field);
        if (idx >= 0)
            return UpdateFields<T>.Infos[idx];

        idx = ~idx - 1;
        if (idx < 0) // field lower than every key
            return null;
        return UpdateFields<T>.Infos[idx];
    }

    public static Type? GetResponseCodesEnum() =>
        Opcodes.GetOpcodesDefiningBuild(Build) switch
        {
            ClientVersionBuild.V1_12_1_5875 => typeof(World.Enums.V1_12_1_5875.ResponseCodes),
            ClientVersionBuild.V2_4_3_8606
                or ClientVersionBuild.V3_3_5a_12340 => typeof(World.Enums.V2_4_3_8606.ResponseCodes),
            _ => null,
        };

    private static byte GetExpansionVersion()
    {
        ReadOnlySpan<char> span = VersionString.AsSpan(1); // Skip 'V'
        int underscoreIndex = span.IndexOf('_');
        return byte.Parse(span[..underscoreIndex]);
    }
    private static byte GetMajorPatchVersion()
    {
        ReadOnlySpan<char> span = VersionString.AsSpan();
        int firstUnderscore = span.IndexOf('_');
        span = span[(firstUnderscore + 1)..];
        int secondUnderscore = span.IndexOf('_');
        return byte.Parse(span[..secondUnderscore]);
    }
    private static byte GetMinorPatchVersion()
    {
        ReadOnlySpan<char> span = VersionString.AsSpan();
        int firstUnderscore = span.IndexOf('_');
        span = span[(firstUnderscore + 1)..];
        int secondUnderscore = span.IndexOf('_');
        span = span[(secondUnderscore + 1)..];
        int thirdUnderscore = span.IndexOf('_');
        var segment = span[..thirdUnderscore];
        // Strip trailing non-digit patch-letter (e.g. 'a' in "5a" for WotLK 3.3.5a)
        while (segment.Length > 0 && !char.IsDigit(segment[^1]))
            segment = segment[..^1];
        return byte.Parse(segment);
    }

    public static bool InVersion(ClientVersionBuild build1, ClientVersionBuild build2)
    {
        return AddedInVersion(build1) && RemovedInVersion(build2);
    }

    public static bool AddedInVersion(ClientVersionBuild build)
    {
        return Build >= build;
    }

    public static bool RemovedInVersion(ClientVersionBuild build)
    {
        return Build < build;
    }

    public static int GetPowersCount()
    {
        if (RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
            return 5;

        return 7;
    }

    public static byte GetMaxLevel()
    {
        if (RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            return 60;
        else if (RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
            return 70;
        else
            return 80;
    }

    public static HitInfo ConvertHitInfoFlags(uint hitInfo)
    {
        if (RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
            return ((HitInfoVanilla)hitInfo).CastFlags<HitInfo>();
        else
            return (HitInfo)hitInfo;
    }

    public static uint ConvertSpellCastResult(uint result)
    {
        if (AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            return (uint)((SpellCastResultWotLK)result).CastEnum<SpellCastResultClassic>();
        else if (AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            return (uint)((SpellCastResultTBC)result).CastEnum<SpellCastResultClassic>();
        else
            return (uint)((SpellCastResultVanilla)result).CastEnum<SpellCastResultClassic>();
    }

    public static QuestGiverStatusModern ConvertQuestGiverStatus(byte status)
    {
        if (AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            return ((QuestGiverStatusWotLK)status).CastEnum<QuestGiverStatusModern>();
        else if (AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            return ((QuestGiverStatusTBC)status).CastEnum<QuestGiverStatusModern>();
        else
            return ((QuestGiverStatusVanilla)status).CastEnum<QuestGiverStatusModern>();
    }

    public static InventoryResult ConvertInventoryResult(uint result)
    {
        if (RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            return ((InventoryResultVanilla)result).CastEnum<InventoryResult>();
        else if (RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
            return ((InventoryResultTBC)result).CastEnum<InventoryResult>();

        return (InventoryResult)result;
    }

    public static int GetQuestLogSize()
    {
        return AddedInVersion(ClientVersionBuild.V2_0_1_6180) ? 25 : 20;  // 2.0.0.5849 Alpha
    }

    public static int GetAuraSlotsCount()
    {
        return AddedInVersion(ClientVersionBuild.V2_0_1_6180) ? 56 : 48;
    }
}

public static class ModernVersion
{
    // Declaration order IS initialization order for static field initializers. Build must be
    // declared first so the loaders below can reference it through the derived fields.
    public static readonly ClientVersionBuild Build = RequireBuild();
    public static readonly byte ExpansionVersion = GetExpansionVersion();
    public static readonly byte MajorVersion = GetMajorPatchVersion();
    public static readonly byte MinorVersion = GetMinorPatchVersion();

    public static int BuildInt => (int)Build;
    public static string VersionString => Build.ToString();

    // Same direct-indexed array scheme as LegacyVersion — see LegacyVersion for the rationale.
    private static readonly Opcode[] _currentToUniversal;
    private static readonly uint[]   _universalToCurrent;

    private static readonly bool _opcodeTablesLoaded = LoadOpcodeTables(out _currentToUniversal, out _universalToCurrent);

    private static ClientVersionBuild RequireBuild()
    {
        if (VersionBootstrap.ModernBuild == ClientVersionBuild.Zero)
            throw new InvalidOperationException(
                "ModernVersion accessed before VersionBootstrap.ModernBuild was set. " +
                "Host startup (ProxyHostedService.ExecuteAsync) or test/benchmark setup must assign it first.");
        return VersionBootstrap.ModernBuild;
    }

    private static bool LoadOpcodeTables(out Opcode[] currentToUniversal, out uint[] universalToCurrent)
    {
        // Same generator-backed path as LegacyVersion. See LegacyVersion.LoadOpcodeTables for the rationale.
        var definingBuild = Opcodes.GetOpcodesDefiningBuild(Build);
        if (!GeneratedOpcodeTables.TryGet(definingBuild, out currentToUniversal, out universalToCurrent))
        {
            Log.Print(LogType.Error, "Could not load opcodes for current modern version.");
            return false;
        }

        ServerLogMessages.LoadedModernOpcodes(
            VersionChecker._melServer, VersionChecker._sourceFile, VersionChecker._netDirNone,
            CountMappings(universalToCurrent));
        return true;
    }

    // Same helper as LegacyVersion — call once at startup.
    private static int CountMappings(uint[] universalToCurrent)
    {
        int count = 0;
        for (int i = 0; i < universalToCurrent.Length; i++)
            if (universalToCurrent[i] != 0) count++;
        return count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Opcode GetUniversalOpcode(uint opcode)
    {
        var table = _currentToUniversal;
        return opcode < (uint)table.Length ? table[opcode] : Opcode.MSG_NULL_ACTION;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GetCurrentOpcode(Opcode universalOpcode)
    {
        var table = _universalToCurrent;
        uint idx = (uint)universalOpcode;
        return idx < (uint)table.Length ? table[idx] : 0u;
    }

    public static ClientVersionBuild GetUpdateFieldsDefiningBuild()
    {
        return GetUpdateFieldsDefiningBuild(Build);
    }

    public static ClientVersionBuild GetUpdateFieldsDefiningBuild(ClientVersionBuild version) =>
        version switch
        {
            ClientVersionBuild.V1_14_0_39802
                or ClientVersionBuild.V1_14_0_39958
                or ClientVersionBuild.V1_14_0_40140
                or ClientVersionBuild.V1_14_0_40179
                or ClientVersionBuild.V1_14_0_40237
                or ClientVersionBuild.V1_14_0_40347
                or ClientVersionBuild.V1_14_0_40441
                or ClientVersionBuild.V1_14_0_40618 => ClientVersionBuild.V1_14_0_40237,
            ClientVersionBuild.V1_14_1_40487
                or ClientVersionBuild.V1_14_1_40594
                or ClientVersionBuild.V1_14_1_40666
                or ClientVersionBuild.V1_14_1_40688
                or ClientVersionBuild.V1_14_1_40800
                or ClientVersionBuild.V1_14_1_40818
                or ClientVersionBuild.V1_14_1_40926
                or ClientVersionBuild.V1_14_1_40962
                or ClientVersionBuild.V1_14_1_41009
                or ClientVersionBuild.V1_14_1_41030
                or ClientVersionBuild.V1_14_1_41077
                or ClientVersionBuild.V1_14_1_41137
                or ClientVersionBuild.V1_14_1_41243
                or ClientVersionBuild.V1_14_1_41511
                or ClientVersionBuild.V1_14_1_41794
                or ClientVersionBuild.V1_14_1_42032
                or ClientVersionBuild.V1_14_2_41858
                or ClientVersionBuild.V1_14_2_41959
                or ClientVersionBuild.V1_14_2_42065
                or ClientVersionBuild.V1_14_2_42082
                or ClientVersionBuild.V1_14_2_42214
                or ClientVersionBuild.V1_14_2_42597 => ClientVersionBuild.V1_14_1_40688,
            ClientVersionBuild.V2_5_2_39570
                or ClientVersionBuild.V2_5_2_39618
                or ClientVersionBuild.V2_5_2_39926
                or ClientVersionBuild.V2_5_2_40011
                or ClientVersionBuild.V2_5_2_40045
                or ClientVersionBuild.V2_5_2_40203
                or ClientVersionBuild.V2_5_2_40260
                or ClientVersionBuild.V2_5_2_40422
                or ClientVersionBuild.V2_5_2_40488
                or ClientVersionBuild.V2_5_2_40617
                or ClientVersionBuild.V2_5_2_40892
                or ClientVersionBuild.V2_5_2_41446
                or ClientVersionBuild.V2_5_2_41510 => ClientVersionBuild.V2_5_2_39570,
            ClientVersionBuild.V2_5_3_41402
                or ClientVersionBuild.V2_5_3_41531
                or ClientVersionBuild.V2_5_3_41750
                or ClientVersionBuild.V2_5_3_41812
                or ClientVersionBuild.V2_5_3_42083
                or ClientVersionBuild.V2_5_3_42328
                or ClientVersionBuild.V2_5_3_42598 => ClientVersionBuild.V2_5_3_41750,
            ClientVersionBuild.V3_4_3_54261 => ClientVersionBuild.V3_4_3_54261,
            _ => ClientVersionBuild.Zero,
        };

    // Same per-T generic static cache pattern as LegacyVersion. See the LegacyVersion copy.
    // Tables are emitted at compile time by HermesProxy.SourceGen.UpdateFieldTableGenerator.
    private static class UpdateFields<T> where T : System.Enum
    {
        public static readonly int[] Keys;
        public static readonly UpdateFieldInfo[] Infos;
        public static readonly Dictionary<string, int>? NamesToValues;

        static UpdateFields()
        {
            var definingBuild = GetUpdateFieldsDefiningBuild(Build);
            if (GeneratedUpdateFieldTables.TryGet(definingBuild, typeof(T),
                out var keys, out var infos, out var names))
            {
                Keys = keys;
                Infos = infos;
                NamesToValues = names;
            }
            else
            {
                Keys = Array.Empty<int>();
                Infos = Array.Empty<UpdateFieldInfo>();
                NamesToValues = null;
            }
        }
    }

    public static int GetUpdateField<T>(T field) where T: System.Enum // C# 7.3
    {
        var names = UpdateFields<T>.NamesToValues;
        if (names != null && names.TryGetValue(field.ToString(), out int fieldValue))
            return fieldValue;
        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetUpdateFieldName<T>(int field) where T: System.Enum // C# 7.3
    {
        var keys = UpdateFields<T>.Keys;
        var infos = UpdateFields<T>.Infos;
        if (keys.Length == 0)
            return field.ToString(CultureInfo.InvariantCulture);

        int idx = Array.BinarySearch(keys, field);
        if (idx >= 0)
            return infos[idx].Name;

        idx = ~idx - 1;
        if (idx < 0) // field lower than every key
            return field.ToString(CultureInfo.InvariantCulture);
        return infos[idx].Name + " + " + (field - keys[idx]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UpdateFieldInfo? GetUpdateFieldInfo<T>(int field) where T: System.Enum // C# 7.3
    {
        var keys = UpdateFields<T>.Keys;
        if (keys.Length == 0)
            return null;

        int idx = Array.BinarySearch(keys, field);
        if (idx >= 0)
            return UpdateFields<T>.Infos[idx];

        idx = ~idx - 1;
        if (idx < 0) // field lower than every key
            return null;
        return UpdateFields<T>.Infos[idx];
    }

    public static Type? GetResponseCodesEnum() =>
        Opcodes.GetOpcodesDefiningBuild(Build) switch
        {
            ClientVersionBuild.V2_5_2_39570 => typeof(World.Enums.V2_5_2_39570.ResponseCodes),
            ClientVersionBuild.V2_5_3_41750
                or ClientVersionBuild.V1_14_1_40688 => typeof(World.Enums.V1_14_1_40688.ResponseCodes),
            ClientVersionBuild.V3_4_3_54261 => typeof(World.Enums.V3_4_3_54261.ResponseCodes),
            _ => null,
        };

    private static byte GetExpansionVersion()
    {
        ReadOnlySpan<char> span = VersionString.AsSpan(1); // Skip 'V'
        int underscoreIndex = span.IndexOf('_');
        return byte.Parse(span[..underscoreIndex]);
    }
    private static byte GetMajorPatchVersion()
    {
        ReadOnlySpan<char> span = VersionString.AsSpan();
        int firstUnderscore = span.IndexOf('_');
        span = span[(firstUnderscore + 1)..];
        int secondUnderscore = span.IndexOf('_');
        return byte.Parse(span[..secondUnderscore]);
    }
    private static byte GetMinorPatchVersion()
    {
        ReadOnlySpan<char> span = VersionString.AsSpan();
        int firstUnderscore = span.IndexOf('_');
        span = span[(firstUnderscore + 1)..];
        int secondUnderscore = span.IndexOf('_');
        span = span[(secondUnderscore + 1)..];
        int thirdUnderscore = span.IndexOf('_');
        var segment = span[..thirdUnderscore];
        // Strip trailing non-digit patch-letter (e.g. 'a' in "5a" for WotLK 3.3.5a)
        while (segment.Length > 0 && !char.IsDigit(segment[^1]))
            segment = segment[..^1];
        return byte.Parse(segment);
    }

    public static bool AddedInVersion(byte expansion, byte major, byte minor)
    {
        if (ExpansionVersion < expansion)
            return false;

        if (ExpansionVersion > expansion)
            return true;

        if (MajorVersion < major)
            return false;

        if (MajorVersion > major)
            return true;

        return MinorVersion >= minor;
    }

    public static bool AddedInVersion(byte retailExpansion, byte retailMajor, byte retailMinor, byte classicEraExpansion, byte classicEraMajor, byte classicEraMinor, byte classicExpansion, byte classicMajor, byte classicMinor)
    {
        if (ExpansionVersion == 1)
            return AddedInVersion(classicEraExpansion, classicEraMajor, classicEraMinor);
        else if (ExpansionVersion == 2 || ExpansionVersion == 3)
            return AddedInVersion(classicExpansion, classicMajor, classicMinor);

        return AddedInVersion(retailExpansion, retailMajor, retailMinor);
    }

    public static bool RemovedInVersion(byte retailExpansion, byte retailMajor, byte retailMinor, byte classicEraExpansion, byte classicEraMajor, byte classicEraMinor, byte classicExpansion, byte classicMajor, byte classicMinor)
    {
        return !AddedInVersion(retailExpansion, retailMajor, retailMinor, classicEraExpansion, classicEraMajor, classicEraMinor, classicExpansion, classicMajor, classicMinor);
    }

    public static bool AddedInClassicVersion(byte classicEraExpansion, byte classicEraMajor, byte classicEraMinor, byte classicExpansion, byte classicMajor, byte classicMinor)
    {
        if (ExpansionVersion == 1)
            return AddedInVersion(classicEraExpansion, classicEraMajor, classicEraMinor);
        else if (ExpansionVersion == 2 || ExpansionVersion == 3)
            return AddedInVersion(classicExpansion, classicMajor, classicMinor);

        return false;
    }

    public static bool RemovedInClassicVersion(byte classicEraExpansion, byte classicEraMajor, byte classicEraMinor, byte classicExpansion, byte classicMajor, byte classicMinor)
    {
        return !AddedInClassicVersion(classicEraExpansion, classicEraMajor, classicEraMinor, classicExpansion, classicMajor, classicMinor);
    }

    public static bool IsVersion(byte expansion, byte major, byte minor)
    {
        return ExpansionVersion == expansion && MajorVersion == major && MinorVersion == minor;
    }

    public static bool InVersion(ClientVersionBuild build1, ClientVersionBuild build2)
    {
        return AddedInVersion(build1) && RemovedInVersion(build2);
    }

    public static bool AddedInVersion(ClientVersionBuild build)
    {
        return Build >= build;
    }

    public static bool RemovedInVersion(ClientVersionBuild build)
    {
        return Build < build;
    }

    public static bool IsClassicVersionBuild()
    {
        return ExpansionVersion == 1 && MajorVersion >= 13 ||
               ExpansionVersion == 2 && MajorVersion >= 5 ||
               ExpansionVersion == 3 && MajorVersion >= 4;
    }

    public static int GetAccountDataCount()
    {
        if (ExpansionVersion == 1 && MajorVersion >= 14)
        {
            if (AddedInVersion(1, 14, 1))
                return 13;
            else
                return 10;
        }
        else if (ExpansionVersion == 2 && MajorVersion >= 5 ||
                 ExpansionVersion == 3 && MajorVersion >= 4)
        {
            if (AddedInVersion(2, 5, 3))
                return 13;
        }
        else if (!IsClassicVersionBuild())
        {
            if (AddedInVersion(9, 2, 0))
                return 13;
            else if (AddedInVersion(9, 1, 5))
                return 12;
        }

        return 8;
    }

    public static int GetPowerCountForClientVersion()
    {
        if (IsClassicVersionBuild())
        {
            if (AddedInClassicVersion(1, 14, 1, 2, 5, 3))
                return 7;

            return 6;
        }
        else
        {
            if (RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                return 5;
            if (RemovedInVersion(ClientVersionBuild.V4_0_6_13596))
                return 7;
            if (RemovedInVersion(ClientVersionBuild.V6_0_2_19033))
                return 5;
            if (RemovedInVersion(ClientVersionBuild.V9_1_5_40772))
                return 6;

            return 7;
        }
    }

    public static uint GetGameObjectStateAnimId()
    {
        if (IsVersion(1, 14, 0) || IsVersion(2, 5, 2))
            return 1556;
        if (IsVersion(1, 14, 1))
            return 1618;
        if (IsVersion(1, 14, 2) || IsVersion(2, 5, 3))
            return 1672;
        if (IsVersion(3, 4, 3))
            return 1772;
        return 0;
    }

    public static byte AdjustInventorySlot(byte slot)
    {
        byte offset = 0;
        if (slot >= World.Enums.Classic.InventorySlots.BankItemStart && slot < World.Enums.Classic.InventorySlots.BankItemEnd)
        {
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                offset = World.Enums.Classic.InventorySlots.BankItemStart - World.Enums.Vanilla.InventorySlots.BankItemStart;
            else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                offset = World.Enums.Classic.InventorySlots.BankItemStart - World.Enums.TBC.InventorySlots.BankItemStart;
            else
                offset = World.Enums.Classic.InventorySlots.BankItemStart - World.Enums.WotLK.InventorySlots.BankItemStart;
        }
        else if (slot >= World.Enums.Classic.InventorySlots.BankBagStart && slot < World.Enums.Classic.InventorySlots.BankBagEnd)
        {
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                offset = World.Enums.Classic.InventorySlots.BankBagStart - World.Enums.Vanilla.InventorySlots.BankBagStart;
            else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                offset = World.Enums.Classic.InventorySlots.BankBagStart - World.Enums.TBC.InventorySlots.BankBagStart;
            else
                offset = World.Enums.Classic.InventorySlots.BankBagStart - World.Enums.WotLK.InventorySlots.BankBagStart;
        }
        else if (slot >= World.Enums.Classic.InventorySlots.BuyBackStart && slot < World.Enums.Classic.InventorySlots.BuyBackEnd)
        {
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                offset = World.Enums.Classic.InventorySlots.BuyBackStart - World.Enums.Vanilla.InventorySlots.BuyBackStart;
            else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                offset = World.Enums.Classic.InventorySlots.BuyBackStart - World.Enums.TBC.InventorySlots.BuyBackStart;
            else
                offset = World.Enums.Classic.InventorySlots.BuyBackStart - World.Enums.WotLK.InventorySlots.BuyBackStart;
        }
        else if (slot >= World.Enums.Classic.InventorySlots.KeyringStart && slot < World.Enums.Classic.InventorySlots.KeyringEnd)
        {
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                offset = World.Enums.Classic.InventorySlots.KeyringStart - World.Enums.Vanilla.InventorySlots.KeyringStart;
            else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                offset = World.Enums.Classic.InventorySlots.KeyringStart - World.Enums.TBC.InventorySlots.KeyringStart;
            else
                offset = World.Enums.Classic.InventorySlots.KeyringStart - World.Enums.WotLK.InventorySlots.KeyringStart;
        }
        return (byte)(slot - offset);
    }
    public static void ConvertAuraFlags(ushort oldFlags, byte slot, out AuraFlagsModern newFlags, out uint activeFlags)
    {
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            activeFlags = 0;
            newFlags = AuraFlagsModern.None;

            if (slot >= 32)
                newFlags |= AuraFlagsModern.Negative;
            else
                newFlags |= AuraFlagsModern.Positive;

            if (oldFlags.HasAnyFlag(AuraFlagsVanilla.Cancelable))
                newFlags |= AuraFlagsModern.Cancelable;

            if (oldFlags.HasAnyFlag(AuraFlagsVanilla.EffectIndex0))
                activeFlags |= 1;
            if (oldFlags.HasAnyFlag(AuraFlagsVanilla.EffectIndex1))
                activeFlags |= 2;
            if (oldFlags.HasAnyFlag(AuraFlagsVanilla.EffectIndex2))
                activeFlags |= 4;
        }
        else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
        {
            activeFlags = 1;
            newFlags = AuraFlagsModern.None;

            if (oldFlags.HasAnyFlag(AuraFlagsTBC.NotCancelable))
                newFlags |= AuraFlagsModern.Negative;
            else if (oldFlags.HasAnyFlag(AuraFlagsTBC.Cancelable))
                newFlags |= (AuraFlagsModern.Positive | AuraFlagsModern.Cancelable);
            else if (slot >= 40)
                newFlags |= AuraFlagsModern.Negative;

            if (oldFlags.HasAnyFlag(AuraFlagsTBC.EffectIndex0))
                activeFlags |= 1;
            if (oldFlags.HasAnyFlag(AuraFlagsTBC.EffectIndex1))
                activeFlags |= 2;
            if (oldFlags.HasAnyFlag(AuraFlagsTBC.EffectIndex2))
                activeFlags |= 4;
        }
        else
        {
            activeFlags = 0;
            newFlags = AuraFlagsModern.None;

            if (oldFlags.HasAnyFlag(AuraFlagsWotLK.Negative))
                newFlags |= AuraFlagsModern.Negative;
            else if (oldFlags.HasAnyFlag(AuraFlagsWotLK.Positive))
                newFlags |= (AuraFlagsModern.Positive | AuraFlagsModern.Cancelable);

            if (oldFlags.HasAnyFlag(AuraFlagsWotLK.NoCaster))
                newFlags |= AuraFlagsModern.NoCaster;
            if (oldFlags.HasAnyFlag(AuraFlagsWotLK.Duration))
                newFlags |= AuraFlagsModern.Duration;

            if (oldFlags.HasAnyFlag(AuraFlagsWotLK.EffectIndex0))
                activeFlags |= 1;
            if (oldFlags.HasAnyFlag(AuraFlagsWotLK.EffectIndex1))
                activeFlags |= 2;
            if (oldFlags.HasAnyFlag(AuraFlagsWotLK.EffectIndex2))
                activeFlags |= 4;
        }
    }

    public static uint GetArenaTeamSizeFromIndex(uint index) =>
        index switch
        {
            0 => 2,
            1 => 3,
            2 => 5,
            _ => 0,
        };

    public static uint GetArenaTeamIndexFromSize(uint size) =>
        size switch
        {
            2 => 0,
            3 => 1,
            5 => 2,
            _ => 0,
        };

    public static byte ConvertResponseCodesValue(byte legacyValue)
    {
        string legacyName = Enum.ToObject(LegacyVersion.GetResponseCodesEnum()!, legacyValue).ToString()!;
        byte modernValue = (byte)Enum.Parse(GetResponseCodesEnum()!, legacyName);
        return modernValue;
    }

    public static byte ConvertSocketColor(byte legacyValue)
    {
        return (byte)((SocketColorLegacy)legacyValue).CastEnum<SocketColorModern>();
    }
}
