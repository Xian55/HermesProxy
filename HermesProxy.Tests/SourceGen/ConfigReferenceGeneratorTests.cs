using System;
using System.Linq;
using HermesProxy.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace HermesProxy.Tests.SourceGen;

public sealed class ConfigReferenceGeneratorTests
{
    private const string Attributes = """
        namespace HermesProxy.Configuration
        {
            [System.AttributeUsage(System.AttributeTargets.Class)]
            internal sealed class ConfigSectionAttribute(int order) : System.Attribute;

            [System.AttributeUsage(System.AttributeTargets.Property)]
            internal sealed class SettingAttribute(string description) : System.Attribute;
        }

        """;

    private static GeneratorDriverRunResult Run(string source)
    {
        var compilation = CSharpCompilation.Create(
            "ConfigReferenceTest",
            [CSharpSyntaxTree.ParseText(Attributes + source)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return CSharpGeneratorDriver.Create(new ConfigReferenceGenerator()).RunGenerators(compilation).GetRunResult();
    }

    private static string Generated(GeneratorDriverRunResult result) => result.GeneratedTrees.Single().ToString();

    [Fact]
    public void Emits_OneEntryPerSetting_ReadingTheDefaultFromTheClass()
    {
        var result = Run("""
            [HermesProxy.Configuration.ConfigSection(1)]
            public sealed class FooOptions
            {
                [HermesProxy.Configuration.Setting("Says \"hello\".")]
                public int Bar { get; set; } = 5;
            }
            """);

        Assert.Empty(result.Diagnostics);
        Assert.Contains("""new("FooOptions", "Bar", Format(new global::FooOptions().Bar), "Says \"hello\"."),""", Generated(result));
    }

    [Fact]
    public void Emits_SectionsInDeclaredOrder_WhateverTheFileOrder()
    {
        string generated = Generated(Run("""
            [HermesProxy.Configuration.ConfigSection(2)]
            public sealed class Second { [HermesProxy.Configuration.Setting("b")] public int B { get; set; } }

            [HermesProxy.Configuration.ConfigSection(1)]
            public sealed class First { [HermesProxy.Configuration.Setting("a")] public int A { get; set; } }
            """));

        Assert.True(generated.IndexOf("\"First\"", StringComparison.Ordinal) < generated.IndexOf("\"Second\"", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("public int Derived { get; internal set; }")]
    [InlineData("public int ReadOnly { get; }")]
    [InlineData("public int InitOnly { get; init; }")]
    [InlineData("internal int Hidden { get; set; }")]
    [InlineData("public static int Shared { get; set; }")]
    public void IgnoresWhatTheBinderCannotSet(string property)
    {
        var result = Run($$"""
            [HermesProxy.Configuration.ConfigSection(1)]
            public sealed class FooOptions
            {
                [HermesProxy.Configuration.Setting("Kept.")]
                public int Kept { get; set; }

                {{property}}
            }
            """);

        Assert.Empty(result.Diagnostics);
        Assert.Single(Generated(result).Split('\n'), static line => line.Contains("new(\"FooOptions\"", StringComparison.Ordinal));
    }

    [Fact]
    public void Fails_OnASettingWithoutADescription_AtTheProperty()
    {
        var result = Run("""
            [HermesProxy.Configuration.ConfigSection(1)]
            public sealed class FooOptions
            {
                public int Bar { get; set; }
            }
            """);

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("HPSG010", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("FooOptions.Bar", diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture));
        Assert.True(diagnostic.Location.GetLineSpan().StartLinePosition.Line > 0);
    }

    [Fact]
    public void IgnoresAClassThatIsNotASection()
    {
        var result = Run("public sealed class FooOptions { public int Bar { get; set; } }");

        Assert.Empty(result.Diagnostics);
        Assert.Empty(result.GeneratedTrees);
    }
}
