using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HermesProxy.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace HermesProxy.Tests.Analyzers;

/// <summary>
/// <see cref="DiscardedTaskAnalyzer"/>: a task may not be dropped (HPA001) and nothing may be
/// async void (HPA002).
/// </summary>
public sealed class DiscardedTaskAnalyzerTests
{
    private const string Prelude = """
        using System;
        using System.Threading.Tasks;

        public static class Observer
        {
            public static void ObserveFaults(this Task task) { }
        }

        """;

    [Theory]
    [InlineData("_ = Work();", "Work")]
    [InlineData("Work();", "Work")]
    [InlineData("_ = WorkWithResult();", "WorkWithResult")]
    [InlineData("WorkWithResult();", "WorkWithResult")]
    [InlineData("_ = ValueWork();", "ValueWork")]
    [InlineData("ValueWork();", "ValueWork")]
    [InlineData("_ = ValueWorkWithResult();", "ValueWorkWithResult")]
    [InlineData("_ = Task.Run(Work);", "Run")]
    [InlineData("Task.Run(Work);", "Run")]
    [InlineData("Work().ContinueWith(_ => { });", "ContinueWith")]
    public async Task DroppedTask_IsAnError(string statement, string name)
    {
        var diagnostics = await Analyze(InMethod(statement));

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("HPA001", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal($"The result of '{name}' is dropped: await it, or call ObserveFaults() on it", diagnostic.GetMessage());
    }

    [Theory]
    [InlineData("Work().ObserveFaults();")]
    [InlineData("_stored = Work();")]
    [InlineData("Task local = Work(); local.ObserveFaults();")]
    [InlineData("_ = Number();")]
    [InlineData("Number();")]
    public async Task HandledTaskOrOtherValue_IsAccepted(string statement)
    {
        Assert.Empty(await Analyze(InMethod(statement)));
    }

    [Fact]
    public async Task AwaitedTask_IsAccepted()
    {
        Assert.Empty(await Analyze("""
            public sealed class Caller
            {
                private static Task Work() => Task.CompletedTask;

                public async Task Run()
                {
                    await Work();
                }
            }
            """));
    }

    [Fact]
    public async Task ReturnedTask_IsAccepted()
    {
        Assert.Empty(await Analyze("""
            public sealed class Caller
            {
                private static Task Work() => Task.CompletedTask;

                public Task Run()
                {
                    return Work();
                }
            }
            """));
    }

    [Fact]
    public async Task AsyncVoidMethod_IsAnError()
    {
        var diagnostics = await Analyze("""
            public sealed class Caller
            {
                public async void Run()
                {
                    await Task.CompletedTask;
                }
            }
            """);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("HPA002", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal("'Run' is async and returns void: return Task", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AsyncVoidLocalFunction_IsAnError()
    {
        var diagnostics = await Analyze("""
            public sealed class Caller
            {
                public void Run()
                {
                    Inner();

                    async void Inner()
                    {
                        await Task.CompletedTask;
                    }
                }
            }
            """);

        Assert.Equal("HPA002", Assert.Single(diagnostics).Id);
    }

    [Fact]
    public async Task AsyncLambdaOnVoidDelegate_IsAnError()
    {
        var diagnostics = await Analyze("""
            public sealed class Caller
            {
                public Action Run() => async () => await Task.CompletedTask;
            }
            """);

        Assert.Equal("HPA002", Assert.Single(diagnostics).Id);
    }

    [Fact]
    public async Task AsyncLambdaOnTaskDelegate_IsAccepted()
    {
        Assert.Empty(await Analyze("""
            public sealed class Caller
            {
                public Func<Task> Run() => async () => await Task.CompletedTask;
            }
            """));
    }

    [Fact]
    public async Task SuppressedSite_IsAccepted()
    {
        Assert.Empty(await Analyze(InMethod("""
            #pragma warning disable HPA001
                    Work();
            #pragma warning restore HPA001
            """)));
    }

    private static string InMethod(string statement) => $$"""
        public sealed class Caller
        {
            private Task _stored = Task.CompletedTask;

            private static Task Work() => Task.CompletedTask;
            private static Task<int> WorkWithResult() => Task.FromResult(1);
            private static ValueTask ValueWork() => default;
            private static ValueTask<int> ValueWorkWithResult() => default;
            private static int Number() => 1;

            public void Run()
            {
                {{statement}}
            }
        }
        """;

    private static async Task<ImmutableArray<Diagnostic>> Analyze(string source)
    {
        string runtimeDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Where(path => Path.GetDirectoryName(path) == runtimeDirectory)
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create(
            nameof(DiscardedTaskAnalyzerTests),
            [CSharpSyntaxTree.ParseText(Prelude + source)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Assert.Empty(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        return await compilation
            .WithAnalyzers([new DiscardedTaskAnalyzer()])
            .GetAnalyzerDiagnosticsAsync();
    }
}
