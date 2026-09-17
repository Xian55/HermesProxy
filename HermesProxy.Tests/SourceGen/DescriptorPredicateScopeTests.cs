using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using HermesProxy.World.Objects.Version.Attributes;
using Xunit;

namespace HermesProxy.Tests.SourceGen;

/// <summary>
/// Predicate strings on descriptor attributes may not name an instance member.
/// </summary>
/// <remarks>
/// <c>HasAny{Section}FieldSet</c> is emitted as a static over <c>(updateData, gameState)</c>, so the
/// V3_4_3 Values filter can ask whether a delta is worth sending before a builder exists, with an
/// instance forwarder beside it. A predicate is pasted into that static verbatim: <c>src</c>,
/// <c>updateData</c>, <c>gameState</c> and fully-qualified statics are in scope, and the builder's
/// own fields are not. <c>CustomPredicate</c> is inlined into the instance update writer as well,
/// where <c>_gameState</c> would compile — so it is bound by the stricter of the two scopes and
/// nothing at the call site says so.
/// <para>
/// The generator reports HPSG008 for this, so a violation already fails the build. Asserting it
/// here too means the rule is stated where a reader looks for it, and that a generator regression
/// which quietly stops checking is caught rather than shipped — the failure it prevents is a
/// CS0103 inside <c>obj/Generated</c>, at a line nobody wrote.
/// </para>
/// </remarks>
public class DescriptorPredicateScopeTests
{
    /// <summary>
    /// A leading underscore starts an identifier only when what precedes it cannot be part of one.
    /// Every builder field is underscore-prefixed per the project's naming rule, and nothing in
    /// scope inside the static is, so the prefix separates the two without parsing the expression.
    /// </summary>
    private static readonly Regex InstanceMember = new(@"(?<![A-Za-z0-9_])_[A-Za-z0-9_]+", RegexOptions.Compiled);

    private static IEnumerable<(string Enum, string Member, string Property, string Predicate)> Predicates()
    {
        var descriptorEnums = typeof(DescriptorSectionAttribute).Assembly
            .GetTypes()
            .Where(t => t.IsEnum && t.GetCustomAttribute<DescriptorSectionAttribute>() != null);

        foreach (var type in descriptorEnums)
        {
            foreach (var member in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                foreach (var attr in member.GetCustomAttributes<DescriptorUpdateFieldAttribute>())
                    if (attr.CustomPredicate != null)
                        yield return (type.Name, member.Name, nameof(attr.CustomPredicate), attr.CustomPredicate);

                foreach (var attr in member.GetCustomAttributes<DescriptorMaskMutatorAttribute>())
                    if (attr.HasAnyPredicate != null)
                        yield return (type.Name, member.Name, nameof(attr.HasAnyPredicate), attr.HasAnyPredicate);
            }
        }
    }

    [Fact]
    public void NoPredicateNamesAnInstanceMember()
    {
        var offenders = Predicates()
            .Select(p => (p, Match: InstanceMember.Match(p.Predicate)))
            .Where(x => x.Match.Success)
            .Select(x => $"{x.p.Enum}.{x.p.Member}: {x.p.Property} references '{x.Match.Value}'")
            .ToArray();

        Assert.Empty(offenders);
    }

    /// <summary>
    /// The sweep above is only worth anything if it is looking at something. This pins that the
    /// reflection actually reaches the descriptor enums, so the rule cannot pass by finding zero
    /// predicates after a namespace move or an attribute rename.
    /// </summary>
    [Fact]
    public void ThePredicateSweepFindsPredicates()
    {
        // 14 at the time of writing (12 CustomPredicate, 2 HasAnyPredicate). The floor is a
        // tripwire for a sweep that stopped reaching them, not a count to keep updated.
        Assert.True(Predicates().Count() >= 10, $"sweep found {Predicates().Count()} predicates");
    }
}
