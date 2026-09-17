using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// Serialises every test class that toggles <c>UpdateObject.ForceV343ForTests</c>.
/// </summary>
/// <remarks>
/// <c>ModernVersion.Build</c> is fixed for the test process, so reaching the filter's V3_4_3 arm
/// means flipping a process-global static and putting it back in a <c>finally</c>. xunit.runner.json
/// runs test classes in parallel, so two classes doing that concurrently interleave: one clears the
/// flag while the other is mid-assert, the filter takes its early return, and a test that was
/// passing reports a delta it expected to be dropped. It fails somewhere unrelated to whatever was
/// last edited and passes on its own, which is the shape of a flake nobody can place.
/// <para>
/// Same treatment as <c>LegacyItemGuidHighCollection</c>. Any new class that sets the flag belongs
/// in this collection.
/// </para>
/// </remarks>
[CollectionDefinition("V343ValuesFilter", DisableParallelization = true)]
public class V343ValuesFilterCollection;
