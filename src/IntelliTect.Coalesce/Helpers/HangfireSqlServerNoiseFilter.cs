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
    /// <param name="sqlCommandPropertyName">
    ///   The name of a custom property attached to the Activity that holds the original <see cref="DbCommand" />. 
    ///   This can be attached by the Enrich option of AddSqlClientInstrumentation.
    /// </param>
    /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
    public static TracerProviderBuilder AddHangfireSqlServerNoiseFilter(
        this TracerProviderBuilder builder, string sqlCommandPropertyName) => builder.AddProcessor(new HangfireSqlServerNoiseFilterProcessor(sqlCommandPropertyName));
}

internal sealed class HangfireSqlServerNoiseFilterProcessor(string sqlCommandPropertyName) : BaseProcessor<Activity>
{
    private ConditionalWeakTable<Thread, StrongBox<bool>> isHangfireThread = new();

    public override void OnEnd(Activity activity)
    {
        DbCommand? command = activity.GetCustomProperty(sqlCommandPropertyName) as DbCommand;
        string? commandText =
            command?.CommandText ??
            activity.GetTagItem("db.statement") as string ??
            activity.GetTagItem("db.query.text") as string ??
            activity.GetTagItem("db.stored_procedure.text") as string;

        if (commandText is not null &&
            activity.Status != ActivityStatusCode.Error &&
            IsHangfireThread(Thread.CurrentThread) && (
            commandText?.Contains("[HangFire]") == true ||
            commandText?.StartsWith("exec sp_getapplock ", StringComparison.InvariantCultureIgnoreCase) == true ||
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
            */
            const int numBottomFramesToSearch = 5;
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