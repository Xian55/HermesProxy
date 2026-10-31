using System;
using System.Threading;
using System.Threading.Tasks;
using Framework.Networking;
using Xunit;

namespace HermesProxy.Tests.Framework;

public sealed class TaskObservationTests
{
    private const string Marker = "task-observation-test";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task Fail(TaskCompletionSource release)
    {
        await release.Task;
        throw new InvalidOperationException(Marker);
    }

    private static void StartObservedFailure(TaskCompletionSource release) => Fail(release).ObserveFaults("context");

    [Fact]
    public void ObserveFaults_OnCompletedTask_DoesNothing()
    {
        Task.CompletedTask.ObserveFaults();
    }

    [Fact]
    public void ObserveFaults_OnFailedTask_DoesNotThrow()
    {
        Task.FromException(new InvalidOperationException(Marker)).ObserveFaults();
    }

    [Fact]
    public void ObserveFaults_OnCancelledTask_DoesNotThrow()
    {
        Task.FromCanceled(new CancellationToken(canceled: true)).ObserveFaults();
    }

    [Fact]
    public void ObserveFaults_CapturesTheCallItWatches()
    {
        Assert.Equal("Task.CompletedTask", CapturedOperation(Task.CompletedTask));
    }

    private static string CapturedOperation(Task task, [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(task))] string operation = "") => operation;

    [Fact]
    public async Task ObserveFaults_FailureAfterTheCall_LeavesNothingUnobserved()
    {
        Exception? unobserved = null;
        EventHandler<UnobservedTaskExceptionEventArgs> handler = (_, e) =>
        {
            if (e.Exception.ToString().Contains(Marker, StringComparison.Ordinal))
                unobserved = e.Exception;
        };

        TaskScheduler.UnobservedTaskException += handler;
        try
        {
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            StartObservedFailure(release);
            release.SetResult();

            await Task.Delay(50, Ct);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        finally
        {
            TaskScheduler.UnobservedTaskException -= handler;
        }

        Assert.Null(unobserved);
    }
}
