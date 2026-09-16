using System;
using System.Linq;
using System.Reflection;
using HermesProxy.World.Objects;
using Xunit;

namespace HermesProxy.Tests.World.Objects;

/// <summary>
/// Pins <see cref="UnitData.HasAnyValue"/> against the field list itself.
/// </summary>
/// <remarks>
/// The V3_4_3 Values filter drops a delta that carries nothing, and it used to decide that from a
/// hand-written list of 47 of UnitData's 116 fields. A delta carrying only one of the other 69 was
/// read as empty and thrown away: a warrior's <c>ShapeshiftForm</c>, so the client kept showing the
/// empty non-stance action bar (issue #300), and with it SheatheState, EmoteState, ComboTarget, the
/// pet fields and every haste and attack-power modifier.
/// <para>
/// A list like that rots the moment a field is added, and nothing fails when it does — the update
/// simply stops arriving, months later, in one subsystem. So this walks the type by reflection
/// instead of naming fields: every nullable field and every array-backed span has to make
/// <c>HasAnyValue</c> true on its own. Adding a field to UnitData without handling it here fails
/// the suite rather than losing a packet in the field.
/// </para>
/// </remarks>
public class UnitDataProbeTests
{
    private static FieldInfo[] NullableFields() =>
        typeof(UnitData)
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => Nullable.GetUnderlyingType(f.FieldType) != null)
            .ToArray();

    /// <summary>The array-backed spans, paired with the <c>Ensure*</c> that materialises them.</summary>
    private static (PropertyInfo Span, MethodInfo Ensure)[] SpanProperties() =>
        typeof(UnitData)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType &&
                        p.PropertyType.GetGenericTypeDefinition() == typeof(ReadOnlySpan<>))
            .Select(p => (Span: p, Ensure: typeof(UnitData).GetMethod("Ensure" + p.Name)!))
            .ToArray();

    /// <summary>A value that is not the type's default, so the NpcFlags "zero says nothing" rule
    /// doesn't make a set slot look unset.</summary>
    private static object NonDefault(Type underlying)
    {
        if (underlying == typeof(bool)) return true;
        if (underlying.IsPrimitive) return Convert.ChangeType(1, underlying);
        return Activator.CreateInstance(underlying)!;
    }

    [Fact]
    public void AFreshBlock_SaysNothing()
    {
        Assert.False(new UnitData().HasAnyValue());
    }

    [Fact]
    public void EveryNullableField_OnItsOwn_MakesTheBlockSaySomething()
    {
        var missed = NullableFields()
            .Where(f =>
            {
                var unit = new UnitData();
                f.SetValue(unit, NonDefault(Nullable.GetUnderlyingType(f.FieldType)!));
                return !unit.HasAnyValue();
            })
            .Select(f => f.Name)
            .ToArray();

        Assert.Empty(missed);
    }

    [Fact]
    public void EveryArrayField_OnItsOwn_MakesTheBlockSaySomething()
    {
        var missed = SpanProperties()
            .Where(pair =>
            {
                var unit = new UnitData();
                var array = (Array)pair.Ensure.Invoke(unit, null)!;
                var element = array.GetType().GetElementType()!;
                array.SetValue(NonDefault(Nullable.GetUnderlyingType(element)!), 0);
                return !unit.HasAnyValue();
            })
            .Select(pair => pair.Span.Name)
            .ToArray();

        Assert.Empty(missed);
    }

    /// <summary>
    /// The one field whose absence issue #300 was traced to, named outright so the regression has a
    /// test that reads as itself rather than only as a row in the reflection sweep.
    /// </summary>
    [Fact]
    public void ShapeshiftFormAlone_IsNotAnEmptyDelta()
    {
        var unit = new UnitData { ShapeshiftForm = 17 };
        Assert.True(unit.HasAnyValue());
    }

    /// <summary>
    /// The flood the probe exists for: cMangos sends NpcFlags slots set to zero as bookkeeping,
    /// and forwarding those made the V3_4_3 client answer every one with CMSG_OBJECT_UPDATE_FAILED.
    /// </summary>
    [Fact]
    public void NpcFlagsSetToZero_StillSayNothing()
    {
        var unit = new UnitData();
        unit.EnsureNpcFlags()[0] = 0;
        Assert.False(unit.HasAnyValue());
    }
}
