using System;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace OpenTelemetry.Trace;

public static class TracerProviderBuilderExtensions
{
    /// <summary>
    /// Adds a processor that disables recording of SQL Server telemetry for Hangfire's constant background polling queries.
    /// Errors are not disabled and will still be logged.
    /// </summary>
    /// <param name="builder"><see cref="TracerProviderBuilder"/> being configured.</param>
    /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
    public static TracerProviderBuilder AddHangfireSqlServerNoiseFilter(
        this TracerProviderBuilder builder
    ) => builder.AddProcessor<HangfireSqlServerNoiseFilterProcessor>();
}

internal sealed class HangfireSqlServerNoiseFilterProcessor : BaseProcessor<Activity>
{
    private ConditionalWeakTable<Thread, StrongBox<bool>> isHangfireThread = new();

    public override void OnEnd(Activity activity)
    {
        string? commandText =
            // Could be either db.statement or db.query.text, depending on whether using old or new conventions.
            activity.GetTagItem("db.statement") as string ??
            activity.GetTagItem("db.query.text") as string ??
            activity.GetTagItem("db.stored_procedure.text") as string;

        if (commandText is not null &&
            activity.Status != ActivityStatusCode.Error &&
            IsHangfireThread(Thread.CurrentThread) &&
            (
                commandText.Contains("[HangFire]") ||
                commandText.StartsWith("exec sp_getapplock ", StringComparison.InvariantCultureIgnoreCase) ||
                commandText is
                    "Commit" or
                    "sp_getapplock" or
                    "sp_releaseapplock" or
                    "set xact_abort on;set nocount on;" or
                    "SELECT SYSUTCDATETIME()"
            ))
        {
            // Sample out
            activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
        }
    }

    private bool IsHangfireThread(Thread thread)
    {
        return thread.IsBackground && !thread.IsThreadPoolThread && isHangfireThread.GetValue(thread, _ =>
        {
            var stackTrace = new System.Diagnostics.StackTrace();
            /*  Looking for stack traces where the bottom looks like this:
                ...
                at Hangfire.Processing.BackgroundDispatcher.DispatchLoop()
                at System.Threading.Thread.StartCallback()

                or this:
                at Hangfire.Processing.BackgroundDispatcher.DispatchLoop()
                at System.Threading.Thread.StartHelper.Callback(Object state)
                at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
                at System.Threading.Thread.StartCallback()

                or this:
                at Hangfire.Processing.BackgroundDispatcher.DispatchLoop()
                at System.Threading.Thread+StartHelper.RunWorker(...) ## DANGER - THIS FRAME IS HIDDEN IN StackTrace.ToString()
                at System.Threading.Thread+StartHelper.Callback(...)
                at System.Threading.ExecutionContext.RunInternal(...)
                at System.Threading.Thread+StartHelper.Run(...) ## DANGER - THIS FRAME IS HIDDEN IN StackTrace.ToString()
                at System.Threading.Thread.StartCallback()
            */
            const int numBottomFramesToSearch = 15;
            for (int i = stackTrace.FrameCount - 1; i >= 0 && i >= stackTrace.FrameCount - numBottomFramesToSearch; i--)
            {
                var frame = stackTrace.GetFrame(i);
                var method = frame?.GetMethod();
                if (method?.Name == "DispatchLoop" && method?.DeclaringType?.FullName == "Hangfire.Processing.BackgroundDispatcher")
                {
                    return new StrongBox<bool>(true);
                }
            }

            return new StrongBox<bool>(false);
        }).Value;
    }
}
