using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace HermesProxy.Analyzers;

/// <summary>
/// Work that fails has to fail where somebody sees it: a <c>Task</c> may not be dropped (HPA001)
/// and nothing may be <c>async void</c> (HPA002).
/// </summary>
/// <remarks>
/// A dropped task keeps its exception to itself until a GC finalizes it, so what should have been
/// logged at the failure shows up later or never. A long-lived loop dies that way without a word.
/// The compiler helps little: CS4014 fires only inside an async method, and <c>_ =</c> silences it.
/// Awaiting is the default. Work that must not be awaited goes through <c>ObserveFaults</c>.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DiscardedTaskAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor DiscardedTask = new(
        id: "HPA001",
        title: "A Task must not be dropped",
        messageFormat: "The result of '{0}' is dropped: await it, or call ObserveFaults() on it",
        category: "HermesProxy.Analyzers",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A dropped Task keeps its exception until a GC finalizes it, which may be never. Await it, or hand it to ObserveFaults, which logs the failure when it happens.");

    private static readonly DiagnosticDescriptor AsyncVoid = new(
        id: "HPA002",
        title: "Async code must not return void",
        messageFormat: "'{0}' is async and returns void: return Task",
        category: "HermesProxy.Analyzers",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "An exception out of async void has no task to land on and takes the process down.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(DiscardedTask, AsyncVoid);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            var task = start.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            var valueTask = start.Compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
            var genericValueTask = start.Compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");
            if (task is null || valueTask is null || genericValueTask is null)
                return;

            start.RegisterOperationAction(operation => CheckStatement(operation, task, valueTask, genericValueTask), OperationKind.ExpressionStatement);
            start.RegisterOperationAction(CheckLocalFunction, OperationKind.LocalFunction);
            start.RegisterOperationAction(CheckAnonymousFunction, OperationKind.AnonymousFunction);
            start.RegisterSymbolAction(CheckMethod, SymbolKind.Method);
        });
    }

    private static void CheckStatement(OperationAnalysisContext context, INamedTypeSymbol task, INamedTypeSymbol valueTask, INamedTypeSymbol genericValueTask)
    {
        var dropped = ((IExpressionStatementOperation)context.Operation).Operation;
        if (dropped is ISimpleAssignmentOperation assignment)
        {
            if (assignment.Target is not IDiscardOperation)
                return;

            dropped = assignment.Value;
        }

        if (!IsAwaitable(dropped.Type, task, valueTask, genericValueTask))
            return;

        string name = dropped is IInvocationOperation invocation ? invocation.TargetMethod.Name : dropped.Syntax.ToString();
        context.ReportDiagnostic(Diagnostic.Create(DiscardedTask, context.Operation.Syntax.GetLocation(), name));
    }

    private static bool IsAwaitable(ITypeSymbol? type, INamedTypeSymbol task, INamedTypeSymbol valueTask, INamedTypeSymbol genericValueTask)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            var definition = current.OriginalDefinition;
            if (SymbolEqualityComparer.Default.Equals(definition, task)
                || SymbolEqualityComparer.Default.Equals(definition, valueTask)
                || SymbolEqualityComparer.Default.Equals(definition, genericValueTask))
            {
                return true;
            }
        }
        return false;
    }

    private static void CheckMethod(SymbolAnalysisContext context) => Report(context.ReportDiagnostic, (IMethodSymbol)context.Symbol, null);

    private static void CheckLocalFunction(OperationAnalysisContext context) =>
        Report(context.ReportDiagnostic, ((ILocalFunctionOperation)context.Operation).Symbol, null);

    private static void CheckAnonymousFunction(OperationAnalysisContext context) =>
        Report(context.ReportDiagnostic, ((IAnonymousFunctionOperation)context.Operation).Symbol, context.Operation.Syntax);

    private static void Report(System.Action<Diagnostic> report, IMethodSymbol method, SyntaxNode? syntax)
    {
        if (!method.IsAsync || !method.ReturnsVoid)
            return;

        var location = syntax?.GetLocation() ?? method.Locations[0];
        report(Diagnostic.Create(AsyncVoid, location, syntax is null ? method.Name : "lambda"));
    }
}
