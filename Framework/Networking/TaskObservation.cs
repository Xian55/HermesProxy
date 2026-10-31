using Framework.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Networking;

/// <summary>
/// The one way to start work without awaiting it, which HPA001 enforces. A task nobody watches
/// keeps its failure to itself until a GC reports it, if one runs before the process exits (#540).
/// </summary>
public static class TaskObservation
{
    private static readonly Microsoft.Extensions.Logging.ILogger _melNet = Log.CreateMelLogger(Log.CategoryNetwork);
    private static readonly string _sourceFile = nameof(TaskObservation).PadRight(15);
    private const string _netDirNone = "";

    /// <param name="task">The work. One that already succeeded costs a check and nothing else.</param>
    /// <param name="context">Whose work this is, such as a remote end point. Read only if it fails.</param>
    /// <param name="operation">Filled in by the compiler with the call being observed.</param>
    public static void ObserveFaults(this Task task, object? context = null, [CallerArgumentExpression(nameof(task))] string operation = "")
    {
        if (task.IsCompletedSuccessfully)
            return;

#pragma warning disable HPA001 // This is the observer, and all the continuation does is log.
        task.ContinueWith(
            static (faulted, state) =>
            {
                var (context, operation) = ((object?, string))state!;
                var ex = faulted.Exception!.GetBaseException();
                TaskObservationLogMessages.BackgroundWorkFaulted(_melNet, ex, _sourceFile, _netDirNone,
                    operation, context?.ToString() ?? "<none>", ex.Message);
            },
            (context, operation),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
#pragma warning restore HPA001
    }
}
